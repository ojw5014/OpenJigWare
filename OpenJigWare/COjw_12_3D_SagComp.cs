// ================================================================
// C3d — 처짐(중력) 보상 + 주행 동특성 보상  2026-09-28
//
// 실물 로봇팔은 중력·백래시·서보 데드밴드 때문에 팔을 뻗을수록 끝이 아래로 처진다.
// 같은 (x,y,z) 를 명령해도 가까우면 덜, 멀면 더 내려앉는다. 시뮬은 그런 게 없다.
// 움직일 때는 여기에 **속도(추종 지연)·가속도(관성)** 에 비례하는 오차가 더 얹힌다.
//
// 이 파일은 그 차이를 세 단계로 다룬다 — **측정·학습·적용 전부 라이브러리 안**이다.
//   ① 측정(Survey)  : 정지 — PlayXyz 로 지점마다 세우고 정착 후 엔코더 → FK → "명령 vs 실측" 표본.
//                     주행 — PlayXyzPath 로 같은 직선을 여러 속도로 달리며 틱 중간중간 엔코더를 읽어
//                     그 순간의 명령점·속도·가속도와 짝지은 표본.
//   ② 학습(Fit)     : 정지 처짐 Δ(r,z) 를 다항식으로(최소제곱), 주행 잔차를 v·a 의 선형항으로.
//                     외부 라이브러리 없음 — 정규방정식을 가우스 소거로 푼다.
//   ③ 적용(Apply)   : 모델이 로드돼 있으면 PlayXyz / PlayXyzPath 가 IK 직전에
//                     목표를 (목표 − 예상 처짐 − 예상 주행오차) 로 바꾼다. 로드 안 됐으면 아무것도 안 한다.
//
// 사용자 계약: 사용법은 예전 그대로다. 처음에 모델 파일을 한 번 로드하는 것 말고는
// PlayXyz / PlayXyzPath 를 똑같이 부른다. 보상은 라이브러리가 조용히 한다.
//
// 좌표 규약: 처짐 Δ = 실측 − 명령 (mm). 아래로 처지면 Δz < 0.
//            보상 명령 = 목표 − Δ(목표)  (정지항은 2회 반복해 보상점 자체의 처짐 차이까지 흡수)
// 요(yaw) 대칭 가정: 처짐은 베이스 회전각과 무관 — (r, z) 두 변수와, 속도·가속도의
//   (반경 r 방향, 수직 z 방향) 성분만 쓴다. 적용 시 Δr 을 그 지점의 방향(x/r, y/r)으로 되돌린다.
//
// ★관절 모델 (같은 날 실측 분석 후 추가 — 있으면 이쪽이 우선):
//   관절 오차 = 중력 지레 × c − 데드밴드 h × (그 관절의 마지막 이동 방향). OMX 실측 h ≈ 1.1° (어깨·팔꿈치 공통).
//   직교 모델이 못 가르던 "접근 방향 차(최대 4.9mm)" 가 관절별 이동 방향으로 설명된다.
//   보상은 IK 목표가 아니라 **실물 출력 단**(CScene_t.SendPose → SagComp_JointOut)에서 관절각에 얹는다 —
//   3D·IK·경로 시작점은 이상 자세 그대로다. 방향 추적은 모델 없이도 늘 돌아 측정 표본에 기록된다.
// ================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            // ────────────────────────────────────────────────────────────
            // 모델
            // ────────────────────────────────────────────────────────────

            /// <summary>처짐 표본 하나 — 명령 TCP 와 실측 TCP, 그 순간의 명령 속도·가속도, 관절별 명령/실측 각도.</summary>
            public class CSagSample
            {
                public float CmdX, CmdY, CmdZ;        // 명령한 TCP (mm)
                public float ActX, ActY, ActZ;        // 엔코더 각도 → FK 로 얻은 실측 TCP (mm)
                public int Approach;                  // +1 = 가까이서 멀리로, -1 = 멀리서 가까이로 (백래시 추적)
                public bool Dynamic;                  // true = 주행 중 표본 (Vr/Vz/Ar/Az 유효)
                public float Vr, Vz;                  // 명령 속도 (mm/s): 반경 방향, 수직
                public float Ar, Az;                  // 명령 가속도 (mm/s²)
                public float[] JointCmd;              // 명령 관절각 (posIDs 3 + wrist)
                public float[] JointAct;              // 실측 관절각
                public float[] JointDir;              // 관절별 마지막 이동 방향 (+1/-1, 0 = 모름) — 데드밴드로 뒤처지는 쪽
                public float[] JointLever;            // 관절별 중력 지레 ∂z̄/∂θ (mm/deg) — 하류 질량점 평균 높이의 변화율
                public bool Excluded;                 // 학습에서 뺀다 (바닥 접촉 의심 등) — 표본은 남겨 둔다
                public float[] JointVel;              // 주행 표본: 그 틱의 관절 명령 속도 (deg/s) — 출력 훅의 추정기와 같은 값
                public float[] JointAcc;              // 주행 표본: 관절 명령 가속도 (deg/s²)

                public float R { get { return (float)Math.Sqrt(CmdX * CmdX + CmdY * CmdY); } }
                public float DX { get { return ActX - CmdX; } }
                public float DY { get { return ActY - CmdY; } }
                public float DZ { get { return ActZ - CmdZ; } }
                /// <summary>수평 방향(뻗는 방향) 성분의 처짐 — 양수면 명령보다 더 뻗었다는 뜻</summary>
                public float DR
                {
                    get
                    {
                        float r = R; if (r < 1e-3f) return 0f;
                        return (DX * CmdX + DY * CmdY) / r;
                    }
                }
            }

            /// <summary>처짐 보상 모델 — 표본 + 정지 다항 계수 + 주행 선형 계수. 파일로 저장/로드.
            ///
            /// 정지: φ(r,z) = [1, r, r², z, r·z]  (표본이 적으면 [1, r])  → Δr = φ·Cr, Δz = φ·Cz
            /// 주행: 정지 예측을 뺀 잔차를  e_r ≈ Kr·[v_r, a_r],  e_z ≈ Kz·[v_z, a_z, a_r]  로 맞춘다
            ///       (절편 없음 — 정지에서는 주행항이 0 이어야 한다).</summary>
            public class CSagComp
            {
                public const int FORMAT_VERSION = 1;
                public string Robot = "";
                public int FuncNumber = 0;
                public int[] PosIDs = new int[] { 11, 12, 13 };
                public int WristID = 14;
                public float PitchDeg = 90f;
                public List<CSagSample> Samples = new List<CSagSample>();

                public int Degree = 0;                             // 0 = 미학습, 1 = [1,r], 2 = [1,r,r²,z,rz]
                public double[] Cr = null, Cz = null;              // 정지 계수
                public double[] Kr = null;                         // 주행: Δr += Kr[0]·v_r + Kr[1]·a_r
                public double[] Kz = null;                         // 주행: Δz += Kz[0]·v_z + Kz[1]·a_z + Kz[2]·a_r
                public float RMin = 0, RMax = 0, ZMin = 0, ZMax = 0;
                public float RmseBefore = 0, RmseAfter = 0;        // 정지: 원시 처짐 / 잔차 (Δz, mm)
                public float DynRmseBefore = 0, DynRmseAfter = 0;  // 주행: 정지모델만 뺀 잔차 / 주행항까지 뺀 잔차
                public float MaxAbsDelta = 40f;                    // 보상량 안전 상한 (mm)
                public bool Enabled = true;

                // ── 관절 모델 (2026-09-28 실측 분석으로 추가) ──
                // 실측 표본을 관절별로 보면 오차가 두 성분으로 깔끔하게 갈린다:
                //   e_j = 실측 − 명령 = Jc_j·L_j − Jh_j·s_j
                //   L_j = 중력 지레 (하류 질량점 평균 높이의 θ_j 미분, mm/deg) → 중력 부하에 비례해 처진다
                //   s_j = 그 관절의 마지막 이동 방향 (±1) → 서보 데드밴드(쿨롱 마찰 ÷ P 게인)만큼 뒤처진다 (Jh ≈ 1.1°)
                //   절편 없음 — 명령과 실측이 같은 엔코더라 중력·마찰 말고는 오프셋 원인이 없다.
                // 직교좌표 "접근 방향" 은 틀린 변수였다: r=120 "멀리" 표본은 휴지 자세에서 내려오느라 어깨가 실제로는
                // 음(−)방향으로 움직였고, 값도 "가까이" 표본과 같았다. 관절 모델이 있으면 직교 보상은 쓰지 않는다
                // (보상은 C3d.SagComp_JointOut 이 실물 출력 단에서 한다 — 3D·IK 는 이상 자세 그대로).
                public int[] JointIDs = null;                      // 관절 모델 ID 순서 = 표본 JointCmd 순서 (PosIDs + WristID, 체인 순)
                public double[] Jc = null, Jh = null;              // 중력 컴플라이언스 (deg per mm/deg) / 데드밴드 반폭 (deg)
                public float JRmseBefore = 0, JRmseAfter = 0;      // 정지 TCP 3D 오차 RMSE (mm): 원시 / 관절 모델 잔차
                public float MaxAbsJointDeg = 6f;                  // 관절 보정량 안전 상한 (deg)
                // 관절 추종 지연 (09-28 z=90 주행 실측: 정지 모델을 뺀 잔차가 관절 속도에 비례, 방향 대칭 — 어깨 ≈60ms, 팔꿈치 ≈40ms)
                //   잔차 = −τ·ω − κ·α  →  출력 명령에 τ·ω + κ·α 를 더해 "앞서 보낸다" (되먹임이 아닌 앞먹임 — 발산하지 않는다)
                public double[] Jtau = null;                       // 관절별 지연 τ (s)
                public double[] Jkap = null;                       // 관절별 가속도 계수 κ (s²)
                public float JLagRmseBefore = 0, JLagRmseAfter = 0;// 주행 TCP Δz RMSE (mm): 정지 관절 모델만 / 지연까지
                public float MaxLeadDeg = 3f;                      // 앞서 보내는 양 안전 상한 (deg)
                // 가속도항 κ 사용 여부 — 09-28 z=90 실측: 느린 두 속도로 학습해 3000ms 를 맞히면 κ 를 넣은 쪽이 오히려 나빴다
                //   (TCP Δz 2.00 → 2.09mm). 식별은 되지만 방향에 따라 부호가 갈려(중력 상호작용) 일반화가 안 된다 → 기본 끔.
                public bool LagUseAccel = false;
                // 지연 보정(앞서 보내기) 적용 여부 — τ 는 학습해 두되 켜는 것은 명시적으로.
                //   09-28 실기(z=90): 8000·5000ms 멀리(→) 는 절반(1.99→1.14, 1.81→0.94mm)으로 줄었지만 3000ms → 는
                //   1.26→2.48mm 로 나빠지고 가까이(←) 는 그대로 — 전체 2.25→2.15mm 로 순이득이 없어 기본 끔.
                //   (어깨 겉보기 지연이 빠를수록 줄어듦 78→54ms — 고정 τ 는 고속에서 과보정)
                public bool LagEnabled = false;
                // 방향 전환 램프 폭 = SigmaRamp × h. 1.0 이면 전환 중 최대 h 만큼 뒤처지고, 0.35 면 약 0.5h.
                //   09-28 z=90 주행: ← 경로(r=260 에서 전 관절 반전 출발) 첫 1/4 에서만 오차 3.3~4.6mm → 줄이는 후보. 실기 비교 전까지 1.0.
                public float SigmaRamp = 1.0f;
                // 외부 휨 배율 κ (09-28 바닥 접촉 캘리브레이션) — 엔코더로 안 보이는 링크·브래킷 휨도 중력 부하에 비례한다고 보고,
                //   **적용할 때만** 관절 보정의 중력 몫을 (1+κ) 배로 한다. c·h 는 엔코더 데이터로 맞춘 그대로(학습·평가에는 κ 없음).
                //   바닥(수평, 외부 기준면)에 그리퍼 끝이 닿는 명령 높이가 반경마다 같아지도록 SagComp_FitFlex 가 정한다.
                public double FlexGain = 0;
                // 바닥 보정표 (09-28) — 14번 손목 브래킷이 무게로 처져(사용자: 조여서 없앨 수 없음) 그리퍼가 지면에 수직이 못 된다.
                //   엔코더는 수직(피치 89~92°)이라 보지만 끝은 자세마다 다른 만큼 낮다 → 관절 모델로는 못 잡는다.
                //   FloorDz(r) = 바닥 접촉 꺾임점의 FK 높이(r) − 기준 (mm). 적용 = 끝이 Δz 만큼 더 높이 가도록 관절 12·13·14 를 바꾼다
                //   (끝의 반경 방향 위치·공구 피치 유지). 바닥 높이에서 잰 값이라 높이가 멀어질수록 근사. SagComp_FitFloorMap 이 만든다.
                //   효과(09-28/29 실기): 계단 밖 6곳 닿은 명령 높이 폭 38.1mm → 4.0/5.7mm (12회 RMS ±1.7mm).
                // ── 사용법 ──
                //   · 켜기  : 모델 파일에 floorr= / floordz= / flooron=1 세 줄 (SagComp_FitFloorMap 뒤 Save 가 쓴다). 출력 훅(SendPose → SagComp_JointOut)이 자동 적용.
                //   · 끄기  : flooron=0 (표는 남김) 또는 floor 세 줄을 지우면 **원래(관절 모델만) 그대로** 동작. 코드에선 FloorMapEnabled = false.
                //   · 만들기: ① SagComp_FloorPress(scene, cfg, 활성모델, …) 로 반경별 바닥 접촉 (ID11~16 토크 ON, 그리퍼 고정)
                //             또는 ① ′ SagComp_FloorContactsFromTrace(SagComp_GetFloorTraceCsv(), cfg) 로 궤적 재분석
                //             ② SagComp_FitFloorMap(contacts, model, double.NaN, 2.0, out rep) ③ model.Save(경로).
                //   · 주의  : 180~190mm 에 약 30mm 계단(처짐 방향이 뒤집히는 자세) — 계단 안은 보간 오차 ±11~14mm. 바닥 높이 밖에선 근사.
                public float[] FloorR = null, FloorDz = null;
                public float FloorRef = 0;                           // 기준 높이(FK) — 기록용
                public bool FloorFromCmd = false;                    // true = 명령 높이 기준 표(관절 모델 잔차 포함 → 관절 모델을 다시 학습하면 무효)
                public bool FloorMapEnabled = true;
                // 바닥 접촉 GP (09-29) — 있으면 반경 보정표 대신 이것을 쓴다(자세 기반, 데이터에서 멀면 저절로 0). flooron=0 이면 둘 다 끈다.
                public CSagGp Gp = null;
                public bool HasGp { get { return Gp != null && Gp.IsReady; } }
                /// <summary>다른 모델의 바닥 보정표를 복사 — FK 기준 표는 엔코더 밖 오차라 관절 모델을 다시 학습해도 그대로 유효하다.
                /// 명령 기준 표(FloorFromCmd)는 bForce 가 아니면 복사하지 않는다. 반환 = 복사했는가.</summary>
                public bool CopyFloorMapFrom(CSagComp other, bool bForce = false)
                {
                    if (other == null || (!other.HasFloorMap && !other.HasGp)) return false;
                    if (!other.HasFloorMap) { Gp = other.Gp; FloorMapEnabled = other.FloorMapEnabled; return true; }
                    if (other.FloorFromCmd && !bForce) return false;
                    FloorR = (float[])other.FloorR.Clone(); FloorDz = (float[])other.FloorDz.Clone();
                    FloorRef = other.FloorRef; FloorFromCmd = other.FloorFromCmd; FloorMapEnabled = other.FloorMapEnabled;
                    if (other.HasGp) Gp = other.Gp;                // GP 도 FK(엔코더 밖) 기준 — 관절 모델과 무관
                    return true;
                }
                public bool HasFloorMap { get { return FloorR != null && FloorDz != null && FloorR.Length >= 2 && FloorR.Length == FloorDz.Length; } }
                private float FloorDzMin() { float v = float.MaxValue; foreach (float d in FloorDz) v = Math.Min(v, d); return v; }
                private float FloorDzMax() { float v = float.MinValue; foreach (float d in FloorDz) v = Math.Max(v, d); return v; }
                /// <summary>반경 r 의 보정표 값 (선형 보간, 표 밖은 끝값)</summary>
                public double FloorDzAt(double r)
                {
                    if (!HasFloorMap) return 0;
                    int n = FloorR.Length;
                    if (r <= FloorR[0]) return FloorDz[0];
                    if (r >= FloorR[n - 1]) return FloorDz[n - 1];
                    for (int i = 1; i < n; i++)
                        if (r <= FloorR[i])
                        {
                            double t = (r - FloorR[i - 1]) / Math.Max(1e-6, FloorR[i] - FloorR[i - 1]);
                            return FloorDz[i - 1] + t * (FloorDz[i] - FloorDz[i - 1]);
                        }
                    return FloorDz[n - 1];
                }
                /// <summary>실물 명령에 얹을 관절 보정 (deg, 부호 = 명령에 더할 값) — 정지 관절 모델 + 외부 휨 배율. 지연 제외.</summary>
                public double JointCorrStatic(int j, double lever, double sigma)
                {
                    if (!HasJoint || j < 0 || j >= Jc.Length) return 0;
                    return -((1.0 + FlexGain) * Jc[j] * lever - Jh[j] * sigma);
                }
                public bool HasJointLag { get { return HasJoint && Jtau != null && Jtau.Length == JointIDs.Length && (Jkap == null || Jkap.Length == JointIDs.Length); } }

                public bool IsFitted { get { return Degree > 0 && Cz != null; } }
                public bool HasDynamic { get { return Kr != null && Kz != null; } }
                public bool HasJoint { get { return JointIDs != null && Jc != null && Jh != null && Jc.Length == JointIDs.Length && Jh.Length == JointIDs.Length; } }
                /// <summary>보상에 쓸 수 있는 모델이 하나라도 있다 (관절 모델 또는 직교 정지 모델)</summary>
                public bool IsUsable { get { return IsFitted || HasJoint; } }

                // 특징 단계: 1 = [1, r]   2 = [1, r, r²]   3 = [1, r, r², z, r·z]
                //   높이가 한 층뿐인 측정에서도 r² 항이 살도록 (처짐은 뻗은 거리에 대해 대체로 2차) z 없이 2단계를 둔다
                private static int FeatCount(int deg) { return deg >= 3 ? 5 : (deg == 2 ? 3 : 2); }
                private static void Feat(int deg, double r, double z, double[] f)
                {
                    f[0] = 1; f[1] = r;
                    if (deg >= 2) f[2] = r * r;
                    if (deg >= 3) { f[3] = z; f[4] = r * z; }
                }

                /// <summary>정지 표본으로 정지 계수를 맞춘다. 6개 이상이면 2차(r,z), 2개 이상이면 1차(r).</summary>
                public string Fit() { Degree = 0; return FitAt(); }
                private string FitAt()
                {
                    var st = Samples.FindAll(s => !s.Dynamic && !s.Excluded);
                    int n = st.Count;
                    if (n < 2) { Degree = 0; Cr = Cz = null; return "정지 표본이 2개 미만 — 학습 불가"; }

                    RMin = float.MaxValue; RMax = float.MinValue; ZMin = float.MaxValue; ZMax = float.MinValue;
                    foreach (var s in st)
                    {
                        float r = s.R;
                        if (r < RMin) RMin = r; if (r > RMax) RMax = r;
                        if (s.CmdZ < ZMin) ZMin = s.CmdZ; if (s.CmdZ > ZMax) ZMax = s.CmdZ;
                    }
                    bool bZ = (ZMax - ZMin) >= 1e-3f;
                    if (Degree <= 0 || Degree > 3) Degree = (n >= 8 && bZ) ? 3 : (n >= 4 ? 2 : 1);   // 재시도(후퇴) 중이면 지정된 차수 유지
                    if (Degree == 3 && !bZ) Degree = 2;                                          // z 한 값뿐 = z 열 특이
                    int k = FeatCount(Degree);

                    double[,] A = new double[k, k];
                    double[] br = new double[k], bz = new double[k], f = new double[k];
                    foreach (var s in st)
                    {
                        Feat(Degree, s.R, s.CmdZ, f);
                        for (int i = 0; i < k; i++)
                        {
                            br[i] += f[i] * s.DR; bz[i] += f[i] * s.DZ;
                            for (int j = 0; j < k; j++) A[i, j] += f[i] * f[j];
                        }
                    }
                    for (int i = 1; i < k; i++) A[i, i] += 1e-6 * (1 + A[i, i]);   // 릿지 소량
                    double[] cr = Solve(A, br), cz = Solve(A, bz);
                    if (cr == null || cz == null)
                    {
                        if (Degree > 1) { Degree = Degree - 1; return FitAt(); }   // 한 단계 후퇴해 재시도
                        Degree = 0; Cr = Cz = null; return "정규방정식 특이 — 학습 실패";
                    }
                    Cr = cr; Cz = cz;

                    double seB = 0, seA = 0;
                    foreach (var s in st)
                    {
                        double pr, pz; PredictStatic(s.CmdX, s.CmdY, s.CmdZ, out pr, out pz);
                        seB += s.DZ * s.DZ; seA += (s.DZ - pz) * (s.DZ - pz);
                    }
                    RmseBefore = (float)Math.Sqrt(seB / n);
                    RmseAfter = (float)Math.Sqrt(seA / n);
                    return string.Format(CultureInfo.InvariantCulture,
                        "정지 학습: 표본 {0}개, 특징 [{1}], r {2:F0}~{3:F0}mm, z {4:F0}~{5:F0}mm — 처짐 RMSE {6:F2}mm → 잔차 {7:F2}mm",
                        n, Degree >= 3 ? "1,r,r²,z,rz" : (Degree == 2 ? "1,r,r²" : "1,r"), RMin, RMax, ZMin, ZMax, RmseBefore, RmseAfter);
                }

                /// <summary>주행 표본으로 속도·가속도 계수를 맞춘다 (정지 모델이 먼저 있어야 한다).</summary>
                public string FitDynamic()
                {
                    Kr = Kz = null;
                    if (!IsFitted) return "정지 모델이 없어 주행 학습 불가";
                    var dy = Samples.FindAll(s => s.Dynamic && !s.Excluded);
                    int n = dy.Count;
                    if (n < 8) return string.Format("주행 표본 {0}개 — 8개 미만이라 주행 학습 생략", n);

                    // 잔차 = 실측 처짐 − 정지 예측
                    // ★2026-09-28 실측 사고: 수평 경로라 v_z·a_z 가 전 표본에서 0 → 그 열이 특이 → kv_z=+172 같은 헛계수가
                    //   붙었고, 검증 때 (보정된 현재 TCP 를 앞에 붙인 첫 세그먼트의) 미세한 v_z 와 곱해져 클램프(40mm)에
                    //   부딪혔다 — 주행 검증 27mm. ⇒ 특징의 RMS 가 문턱 미만이면 그 계수는 0 으로 두고(식별 불가),
                    //   식별된 계수도 물리적으로 말이 되는 상한(지연 200ms 상당 / 가속 항 0.01) 을 넘으면 버린다.
                    double[] fr2 = new double[2], fz2 = new double[3];
                    foreach (var s in dy)
                    {
                        fr2[0] += s.Vr * s.Vr; fr2[1] += s.Ar * s.Ar;
                        fz2[0] += s.Vz * s.Vz; fz2[1] += s.Az * s.Az; fz2[2] += s.Ar * s.Ar;
                    }
                    bool[] ur = { Math.Sqrt(fr2[0] / n) >= 2.0, Math.Sqrt(fr2[1] / n) >= 10.0 };              // v ≥ 2mm/s, a ≥ 10mm/s² RMS
                    bool[] uz = { Math.Sqrt(fz2[0] / n) >= 2.0, Math.Sqrt(fz2[1] / n) >= 10.0, Math.Sqrt(fz2[2] / n) >= 10.0 };
                    double[,] Ar = new double[2, 2]; double[] br = new double[2];
                    double[,] Az = new double[3, 3]; double[] bz = new double[3];
                    double seB = 0;
                    foreach (var s in dy)
                    {
                        double pr, pz; PredictStatic(s.CmdX, s.CmdY, s.CmdZ, out pr, out pz);
                        double er = s.DR - pr, ez = s.DZ - pz;
                        seB += ez * ez;
                        double[] fr = { ur[0] ? s.Vr : 0, ur[1] ? s.Ar : 0 };
                        double[] fz = { uz[0] ? s.Vz : 0, uz[1] ? s.Az : 0, uz[2] ? s.Ar : 0 };
                        for (int i = 0; i < 2; i++) { br[i] += fr[i] * er; for (int j = 0; j < 2; j++) Ar[i, j] += fr[i] * fr[j]; }
                        for (int i = 0; i < 3; i++) { bz[i] += fz[i] * ez; for (int j = 0; j < 3; j++) Az[i, j] += fz[i] * fz[j]; }
                    }
                    for (int i = 0; i < 2; i++) Ar[i, i] += (ur[i] ? 1e-6 * (1 + Ar[i, i]) : 1.0);   // 배제 열은 항등으로 → 계수 0
                    for (int i = 0; i < 3; i++) Az[i, i] += (uz[i] ? 1e-6 * (1 + Az[i, i]) : 1.0);
                    double[] kr = Solve(Ar, br), kz = Solve(Az, bz);
                    if (kr == null || kz == null) return "주행 정규방정식 특이 — 주행 학습 실패 (속도 종류가 부족?)";
                    // 물리 상한: 속도항 |kv| ≤ 0.2 mm/(mm/s) (= 지연 200ms 상당), 가속항 |ka| ≤ 0.01 mm/(mm/s²)
                    var dropped = new List<string>();
                    if (Math.Abs(kr[0]) > 0.2) { kr[0] = 0; dropped.Add("kv_r"); }
                    if (Math.Abs(kr[1]) > 0.01) { kr[1] = 0; dropped.Add("ka_r"); }
                    if (Math.Abs(kz[0]) > 0.2) { kz[0] = 0; dropped.Add("kv_z"); }
                    if (Math.Abs(kz[1]) > 0.01) { kz[1] = 0; dropped.Add("ka_z"); }
                    if (Math.Abs(kz[2]) > 0.01) { kz[2] = 0; dropped.Add("ka_rz"); }
                    Kr = kr; Kz = kz;
                    string strGate = string.Format("식별 열: v_r {0} a_r {1} | v_z {2} a_z {3} a_rz {4}{5}",
                        ur[0] ? "○" : "×", ur[1] ? "○" : "×", uz[0] ? "○" : "×", uz[1] ? "○" : "×", uz[2] ? "○" : "×",
                        dropped.Count > 0 ? " / 상한 초과로 버림: " + string.Join(",", dropped.ToArray()) : "");

                    double seA = 0;
                    foreach (var s in dy)
                    {
                        double pr, pz; Predict(s.CmdX, s.CmdY, s.CmdZ, s.Vr, s.Vz, s.Ar, s.Az, out pr, out pz);
                        seA += (s.DZ - pz) * (s.DZ - pz);
                    }
                    DynRmseBefore = (float)Math.Sqrt(seB / n);
                    DynRmseAfter = (float)Math.Sqrt(seA / n);
                    return string.Format(CultureInfo.InvariantCulture,
                        "주행 학습: 표본 {0}개 — Δz 잔차 {1:F2}mm → {2:F2}mm  (kv_z {3:+0.0000;-0.0000} mm/(mm/s), ka_z {4:+0.000000;-0.000000} mm/(mm/s²), ka_rz {5:+0.000000;-0.000000}; kv_r {6:+0.0000;-0.0000}, ka_r {7:+0.000000;-0.000000})  {8}",
                        n, DynRmseBefore, DynRmseAfter, Kz[0], Kz[1], Kz[2], Kr[0], Kr[1], strGate);
                }

                /// <summary>관절 모델 학습 — 방향(JointDir)·지레(JointLever)가 기록된 정지 표본으로 관절마다
                /// e = c·L − h·s 를 맞춘다 (미지수 2개, 절편 없음). 지레가 사실상 0 인 관절(수직축 베이스)은 h 만.
                /// h 는 음이 될 수 없다(데드밴드는 뒤처지기만 한다) — 음으로 나오면 0 으로 두고 c 만 다시 맞춘다.
                /// TCP 기준 평가는 FK 가 필요해 C3d.SagComp_FitJoint 가 한다.</summary>
                public string FitJoint()
                {
                    Jc = Jh = null;
                    int nj = PosIDs.Length + 1;
                    if (JointIDs == null || JointIDs.Length != nj)
                    { JointIDs = new int[nj]; Array.Copy(PosIDs, JointIDs, PosIDs.Length); JointIDs[nj - 1] = WristID; }
                    var st = Samples.FindAll(s => !s.Dynamic && !s.Excluded && s.JointCmd != null && s.JointAct != null && s.JointDir != null && s.JointLever != null &&
                                                  s.JointCmd.Length >= nj && s.JointAct.Length >= nj && s.JointDir.Length >= nj && s.JointLever.Length >= nj);
                    if (st.Count < 3) return "관절 모델: 방향·지레가 기록된 정지 표본이 3개 미만 — 생략";
                    double[] c = new double[nj], h = new double[nj];
                    var sb = new StringBuilder("관절 모델 (e = c·지레 − h·방향):");
                    for (int j = 0; j < nj; j++)
                    {
                        double sLL = 0, sLS = 0, sSS = 0, sEL = 0, sES = 0, sEE = 0; int n = 0, nPos = 0, nNeg = 0;
                        foreach (var s in st)
                        {
                            double sd = s.JointDir[j]; if (sd == 0) continue;   // 방향 모름 = 뺀다
                            double L = s.JointLever[j], e = s.JointAct[j] - s.JointCmd[j];
                            sLL += L * L; sLS += L * sd; sSS += sd * sd; sEL += e * L; sES += e * sd; sEE += e * e; n++;
                            if (sd > 0) nPos++; else nNeg++;
                        }
                        if (n < 2) { sb.AppendFormat(" | ID{0} 표본 부족", JointIDs[j]); continue; }
                        // 데드밴드 h 는 양쪽 방향이 다 관측돼야 믿는다 — 한쪽뿐이면 "뒤처짐" 과 다른 원인을 가를 수 없다.
                        //   ★09-28 실측: 베이스는 휴지→0° 로 한 번(+)만 움직여 h=0.74 가 잡혔는데, 검증 때 +0.74° 명령에
                        //   거의 끝까지 따라가 옆(y) 오차가 반대쪽으로 같은 크기(±2mm)로 남았다 — 느린 접근과 빠른 접근의 멈춤 위치가 다르다.
                        bool bBoth = nPos > 0 && nNeg > 0;
                        // e = c·L + b·s  (b = −h)
                        // 지레 RMS 가 0.3 mm/deg 미만이면 중력항은 식별 불가로 본다 (수직축 베이스, 수직 툴의 손목 —
                        //   09-28 실측에서 손목 c 가 IK 자세가 흐트러진 표본 1개로만 −2.87 로 잡혔다)
                        double cj = 0, bj = 0;
                        bool bL = Math.Sqrt(sLL / n) >= 0.3;
                        if (bL && bBoth)
                        {
                            double det = sLL * sSS - sLS * sLS;
                            if (Math.Abs(det) > 1e-9 * Math.Max(1.0, sLL * sSS)) { cj = (sEL * sSS - sES * sLS) / det; bj = (sES * sLL - sEL * sLS) / det; }
                            else cj = sEL / sLL;                             // 방향과 지레가 완전히 얽힘 — 중력항만
                        }
                        else if (bL) cj = sEL / sLL;                         // 한쪽 방향뿐 — 중력항만
                        else if (bBoth) bj = sES / sSS;                      // 중력 없는 관절 — 데드밴드만
                        // 물리 제약: 중력은 질량을 낮추는 쪽으로만 관절을 밀고(c ≤ 0), 데드밴드는 뒤처지기만 한다(h ≥ 0)
                        if (cj > 0) { cj = 0; bj = bBoth ? sES / sSS : 0; }
                        if (bj > 0) { bj = 0; cj = bL ? Math.Min(0.0, sEL / sLL) : 0; }
                        c[j] = cj; h[j] = -bj;
                        double seA = 0;
                        foreach (var s in st)
                        {
                            double sd = s.JointDir[j]; if (sd == 0) continue;
                            double r = (s.JointAct[j] - s.JointCmd[j]) - (cj * s.JointLever[j] + bj * sd);
                            seA += r * r;
                        }
                        sb.AppendFormat(CultureInfo.InvariantCulture, " | ID{0} c={1:+0.0000;-0.0000} h={2:F2}°{5} (오차 {3:F2}→{4:F2}°)",
                            JointIDs[j], cj, -bj, Math.Sqrt(sEE / n), Math.Sqrt(seA / n), bBoth ? "" : "(한쪽 방향만 관측 — h 안 씀)");
                    }
                    Jc = c; Jh = h;
                    return sb.ToString();
                }

                /// <summary>관절 j 의 예상 오차 (실측 − 명령, deg). sigma = 이동 방향 (−1..+1)</summary>
                public double PredictJointErr(int j, double lever, double sigma)
                {
                    if (!HasJoint || j < 0 || j >= Jc.Length) return 0;
                    return Jc[j] * lever - Jh[j] * sigma;
                }

                /// <summary>관절 j 의 주행 지연 오차 (실측 − 명령, deg) = −τ·ω − κ·α. 지연 모델이 없으면 0.</summary>
                public double PredictJointLag(int j, double omega, double alpha)
                {
                    if (!HasJointLag || j < 0 || j >= Jtau.Length) return 0;
                    return -Jtau[j] * omega - (Jkap != null ? Jkap[j] * alpha : 0);
                }

                /// <summary>관절 지연 학습 — 주행 표본(관절 명령·실측·방향·지레·속도 기록)에서 정지 관절 모델의 예측을 뺀
                /// 잔차를 관절마다 −τ·ω − κ·α 로 맞춘다 (절편 없음 — 멈추면 0). 정지 관절 모델이 먼저 있어야 한다.
                /// 식별 문턱: ω RMS ≥ 2 deg/s, α RMS ≥ 20 deg/s² (못 넘으면 그 계수 0).
                /// 물리 범위: 0 ≤ τ ≤ 0.2 s, |κ| ≤ 0.01 s² (벗어나면 버리고 나머지로 다시).</summary>
                public string FitJointLag()
                {
                    Jtau = Jkap = null;
                    if (!HasJoint) return "지연 학습: 정지 관절 모델이 없어 생략";
                    int nj = JointIDs.Length;
                    var dy = Samples.FindAll(s => s.Dynamic && !s.Excluded && s.JointCmd != null && s.JointAct != null && s.JointDir != null &&
                                                  s.JointLever != null && s.JointVel != null && s.JointCmd.Length >= nj && s.JointAct.Length >= nj &&
                                                  s.JointDir.Length >= nj && s.JointLever.Length >= nj && s.JointVel.Length >= nj);
                    if (dy.Count < 20) return string.Format("지연 학습: 관절 속도가 기록된 주행 표본 {0}개 — 20개 미만이라 생략", dy.Count);
                    double[] tau = new double[nj], kap = new double[nj];
                    var sb = new StringBuilder("관절 지연 (잔차 = −τ·ω − κ·α):");
                    for (int j = 0; j < nj; j++)
                    {
                        double sWW = 0, sWA = 0, sAA = 0, sRW = 0, sRA = 0, sRR = 0; int n = 0;
                        foreach (var s in dy)
                        {
                            double r = (s.JointAct[j] - s.JointCmd[j]) - PredictJointErr(j, s.JointLever[j], s.JointDir[j]);
                            double fw = -s.JointVel[j], fa = (s.JointAcc != null && s.JointAcc.Length > j) ? -s.JointAcc[j] : 0;
                            sWW += fw * fw; sWA += fw * fa; sAA += fa * fa; sRW += r * fw; sRA += r * fa; sRR += r * r; n++;
                        }
                        bool bW = Math.Sqrt(sWW / n) >= 2.0, bA = LagUseAccel && Math.Sqrt(sAA / n) >= 20.0;
                        double t = 0, k = 0;
                        if (bW && bA)
                        {
                            double det = sWW * sAA - sWA * sWA;
                            if (Math.Abs(det) > 1e-9 * Math.Max(1.0, sWW * sAA)) { t = (sRW * sAA - sRA * sWA) / det; k = (sRA * sWW - sRW * sWA) / det; }
                            else t = sRW / sWW;
                        }
                        else if (bW) t = sRW / sWW;
                        string strDrop = "";
                        if (Math.Abs(k) > 0.01) { k = 0; t = bW ? sRW / sWW : 0; strDrop = " κ범위밖"; }
                        if (t < 0 || t > 0.2) { strDrop += string.Format(CultureInfo.InvariantCulture, " τ범위밖({0:F3})", t); t = 0; k = (bA && Math.Abs(sRA / sAA) <= 0.01) ? sRA / sAA : 0; }
                        tau[j] = t; kap[j] = k;
                        double seA = 0;
                        foreach (var s in dy)
                        {
                            double r = (s.JointAct[j] - s.JointCmd[j]) - PredictJointErr(j, s.JointLever[j], s.JointDir[j]);
                            double fa = (s.JointAcc != null && s.JointAcc.Length > j) ? s.JointAcc[j] : 0;
                            double e = r + t * s.JointVel[j] + k * fa;
                            seA += e * e;
                        }
                        sb.AppendFormat(CultureInfo.InvariantCulture, " | ID{0} τ={1:F0}ms κ={2:+0.0000;-0.0000}{3} ({4:F2}→{5:F2}°){6}",
                            JointIDs[j], t * 1000, k, bW ? "" : "(ω 부족)", Math.Sqrt(sRR / n), Math.Sqrt(seA / n), strDrop);
                    }
                    Jtau = tau; Jkap = kap;
                    return sb.ToString();
                }

                /// <summary>정지 처짐 예측 (Δr, Δz). 학습 구간 밖은 경계로 클램프.</summary>
                public void PredictStatic(double x, double y, double z, out double dR, out double dZ)
                {
                    dR = dZ = 0;
                    if (!IsFitted) return;
                    double r = Math.Sqrt(x * x + y * y);
                    if (r < RMin) r = RMin; if (r > RMax) r = RMax;
                    if (z < ZMin) z = ZMin; if (z > ZMax) z = ZMax;
                    int k = FeatCount(Degree);
                    double[] f = new double[k];
                    Feat(Degree, r, z, f);
                    for (int i = 0; i < k; i++) { dR += f[i] * Cr[i]; dZ += f[i] * Cz[i]; }
                }

                /// <summary>정지 + 주행 예측. 속도·가속도가 0 이면 정지 예측과 같다. 안전 상한 적용.</summary>
                public void Predict(double x, double y, double z, double vr, double vz, double ar, double az,
                                    out double dR, out double dZ)
                {
                    PredictStatic(x, y, z, out dR, out dZ);
                    if (HasDynamic)
                    {
                        dR += Kr[0] * vr + Kr[1] * ar;
                        dZ += Kz[0] * vz + Kz[1] * az + Kz[2] * ar;
                    }
                    if (dR > MaxAbsDelta) dR = MaxAbsDelta; if (dR < -MaxAbsDelta) dR = -MaxAbsDelta;
                    if (dZ > MaxAbsDelta) dZ = MaxAbsDelta; if (dZ < -MaxAbsDelta) dZ = -MaxAbsDelta;
                }

                /// <summary>목표를 보상 명령으로 바꾼다: cmd = target − Δ(cmd, v, a). 정지항은 2회 반복.
                /// 반환 = 적용한 (Δx,Δy,Δz) (진단용).</summary>
                public float[] Compensate(ref float fX, ref float fY, ref float fZ,
                                          double vr, double vz, double ar, double az)
                {
                    if (!Enabled || !IsFitted || HasJoint) return new float[3];   // 관절 모델이 있으면 출력 단(SagComp_JointOut)이 보상한다
                    double tx = fX, ty = fY, tz = fZ;
                    double cx = tx, cy = ty, cz = tz;
                    double dx = 0, dy = 0, dz = 0;
                    double r = Math.Sqrt(tx * tx + ty * ty);
                    for (int it = 0; it < 2; it++)
                    {
                        double dR, dZ; Predict(cx, cy, cz, vr, vz, ar, az, out dR, out dZ);
                        dx = (r > 1e-6) ? dR * tx / r : 0; dy = (r > 1e-6) ? dR * ty / r : 0; dz = dZ;
                        cx = tx - dx; cy = ty - dy; cz = tz - dz;
                    }
                    fX = (float)cx; fY = (float)cy; fZ = (float)cz;
                    return new float[] { (float)dx, (float)dy, (float)dz };
                }
                public float[] Compensate(ref float fX, ref float fY, ref float fZ)
                { return Compensate(ref fX, ref fY, ref fZ, 0, 0, 0, 0); }

                // ── 저장/로드 (사람이 읽는 텍스트) ──
                public void Save(string strPath)
                {
                    var sb = new StringBuilder();
                    var ci = CultureInfo.InvariantCulture;
                    sb.AppendLine("# OpenJigWare SagComp v" + FORMAT_VERSION + " — 처짐(중력)+주행 보상 모델 (측정: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ")");
                    sb.AppendLine("robot=" + Robot);
                    sb.AppendLine("func=" + FuncNumber);
                    sb.AppendLine("posids=" + string.Join(",", Array.ConvertAll(PosIDs, v => v.ToString(ci))));
                    sb.AppendLine("wrist=" + WristID);
                    sb.AppendLine("pitch=" + PitchDeg.ToString(ci));
                    sb.AppendLine("maxdelta=" + MaxAbsDelta.ToString(ci));
                    sb.AppendLine("degree=" + Degree);
                    sb.AppendLine("range=" + string.Join(",", new[] { RMin.ToString(ci), RMax.ToString(ci), ZMin.ToString(ci), ZMax.ToString(ci) }));
                    if (Cr != null) sb.AppendLine("cr=" + string.Join(",", Array.ConvertAll(Cr, v => v.ToString("R", ci))));
                    if (Cz != null) sb.AppendLine("cz=" + string.Join(",", Array.ConvertAll(Cz, v => v.ToString("R", ci))));
                    if (Kr != null) sb.AppendLine("kr=" + string.Join(",", Array.ConvertAll(Kr, v => v.ToString("R", ci))));
                    if (Kz != null) sb.AppendLine("kz=" + string.Join(",", Array.ConvertAll(Kz, v => v.ToString("R", ci))));
                    sb.AppendLine("rmse=" + RmseBefore.ToString(ci) + "," + RmseAfter.ToString(ci));
                    sb.AppendLine("dynrmse=" + DynRmseBefore.ToString(ci) + "," + DynRmseAfter.ToString(ci));
                    if (JointIDs != null) sb.AppendLine("jids=" + string.Join(",", Array.ConvertAll(JointIDs, v => v.ToString(ci))));
                    if (Jc != null) sb.AppendLine("jc=" + string.Join(",", Array.ConvertAll(Jc, v => v.ToString("R", ci))));
                    if (Jh != null) sb.AppendLine("jh=" + string.Join(",", Array.ConvertAll(Jh, v => v.ToString("R", ci))));
                    if (HasJoint) sb.AppendLine("jrmse=" + JRmseBefore.ToString(ci) + "," + JRmseAfter.ToString(ci));
                    sb.AppendLine("maxjoint=" + MaxAbsJointDeg.ToString(ci));
                    if (Jtau != null) sb.AppendLine("jtau=" + string.Join(",", Array.ConvertAll(Jtau, v => v.ToString("R", ci))));
                    if (Jkap != null) sb.AppendLine("jkap=" + string.Join(",", Array.ConvertAll(Jkap, v => v.ToString("R", ci))));
                    if (Jtau != null) sb.AppendLine("jlagrmse=" + JLagRmseBefore.ToString(ci) + "," + JLagRmseAfter.ToString(ci));
                    sb.AppendLine("maxlead=" + MaxLeadDeg.ToString(ci));
                    sb.AppendLine("lagaccel=" + (LagUseAccel ? "1" : "0"));
                    sb.AppendLine("lagon=" + (LagEnabled ? "1" : "0"));
                    sb.AppendLine("sigmaramp=" + SigmaRamp.ToString(ci));
                    if (FlexGain != 0) sb.AppendLine("flex=" + FlexGain.ToString("R", ci) + "   # 외부 휨 배율 κ (바닥 접촉 캘리브레이션)");
                    if (HasFloorMap)
                    {
                        sb.AppendLine("floorr=" + string.Join(",", Array.ConvertAll(FloorR, v => v.ToString("0.###", ci))) + "   # 바닥 보정표 반경 (mm)");
                        sb.AppendLine("floordz=" + string.Join(",", Array.ConvertAll(FloorDz, v => v.ToString("0.###", ci))) + "   # 끝 높이 보정 Δz (mm, 기준 FK " + FloorRef.ToString("0.##", ci) + ")");
                        sb.AppendLine("flooron=" + (FloorMapEnabled ? "1" : "0") + "   # 0 이면 표는 남기고 끔 (floor 줄을 지워도 원래대로)");
                        if (FloorFromCmd) sb.AppendLine("floorcmd=1   # 명령 높이 기준 표 — 관절 모델을 다시 학습하면 다시 재야 한다");
                    }
                    if (HasGp)
                    {
                        if (!HasFloorMap) sb.AppendLine("flooron=" + (FloorMapEnabled ? "1" : "0") + "   # 0 이면 바닥 보정(GP) 끔 — 관절 모델만");
                        sb.AppendLine("gpids=" + string.Join(",", Array.ConvertAll(Gp.FeatIDs, v => v.ToString(ci))) + "   # 바닥 접촉 GP 입력 관절");
                        sb.AppendLine("gpell=" + string.Join(",", Array.ConvertAll(Gp.Ell, v => v.ToString("R", ci))));
                        sb.AppendLine("gpsig=" + Gp.SigF.ToString("R", ci) + "," + Gp.SigN.ToString("R", ci) + "," + Gp.Ref.ToString("R", ci) + "," + Gp.MaxStd.ToString("R", ci) + "   # 신호, 잡음, 기준 FK, 확신 한계 (mm)");
                        sb.AppendLine("gpon=" + (Gp.Enabled ? "1" : "0"));
                        if (Gp.BadR != null && Gp.BadR.Length > 0)
                            sb.AppendLine("gpbadr=" + string.Join(",", Array.ConvertAll(Gp.BadR, v => v.ToString("0.#", ci))) + "," + Gp.BadBandMm.ToString("0.#", ci) + "   # 두 상태 반경들, 마지막 = 띠 반폭(mm) — 띠 안 보정 0");
                        sb.AppendLine("gpdom=" + Gp.DomainDeg.ToString("0.##", ci) + "   # 가장 가까운 학습 자세와 이만큼(°) 넘게 떨어지면 줄이고 2배에서 0");
                        for (int gi = 0; gi < Gp.X.Length; gi++)
                            sb.AppendLine("gpx=" + string.Join(",", Array.ConvertAll(Gp.X[gi], v => v.ToString("0.###", ci))) + "," + Gp.Y[gi].ToString("0.###", ci));
                    }
                    var lstEx = new List<string>();
                    for (int i = 0; i < Samples.Count; i++) if (Samples[i].Excluded) lstEx.Add(i.ToString(ci));
                    if (lstEx.Count > 0) sb.AppendLine("excl=" + string.Join(",", lstEx.ToArray()) + "   # 학습에서 뺀 표본 번호(0부터, s= 줄 순서) — 바닥 접촉 의심 등");
                    sb.AppendLine("# s=cmdX,cmdY,cmdZ,actX,actY,actZ,approach,dyn,vr,vz,ar,az,jointCmd...,|,jointAct...[,|,jointDir...,|,jointLever...[,|,jointVel...,|,jointAcc...]]");
                    foreach (var s in Samples)
                    {
                        sb.Append("s=");
                        sb.Append(string.Join(",", new[] { s.CmdX.ToString(ci), s.CmdY.ToString(ci), s.CmdZ.ToString(ci),
                                                           s.ActX.ToString(ci), s.ActY.ToString(ci), s.ActZ.ToString(ci),
                                                           s.Approach.ToString(ci), s.Dynamic ? "1" : "0",
                                                           s.Vr.ToString(ci), s.Vz.ToString(ci), s.Ar.ToString(ci), s.Az.ToString(ci) }));
                        if (s.JointCmd != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointCmd, v => v.ToString(ci)))); }
                        sb.Append(",|");
                        if (s.JointAct != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointAct, v => v.ToString(ci)))); }
                        if (s.JointDir != null || s.JointLever != null)
                        {
                            sb.Append(",|");
                            if (s.JointDir != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointDir, v => v.ToString(ci)))); }
                            sb.Append(",|");
                            if (s.JointLever != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointLever, v => v.ToString("G6", ci)))); }
                            if (s.JointVel != null || s.JointAcc != null)
                            {
                                sb.Append(",|");
                                if (s.JointVel != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointVel, v => v.ToString("G6", ci)))); }
                                sb.Append(",|");
                                if (s.JointAcc != null) { sb.Append(","); sb.Append(string.Join(",", Array.ConvertAll(s.JointAcc, v => v.ToString("G6", ci)))); }
                            }
                        }
                        sb.AppendLine();
                    }
                    File.WriteAllText(strPath, sb.ToString(), new UTF8Encoding(false));
                }

                public static CSagComp Load(string strPath)
                {
                    if (!File.Exists(strPath)) return null;
                    var ci = CultureInfo.InvariantCulture;
                    var m = new CSagComp();
                    const int HEAD = 12;
                    var lstExcl = new List<int>();
                    foreach (string raw in File.ReadAllLines(strPath))
                    {
                        string line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        int eq = line.IndexOf('=');
                        if (eq < 0) continue;
                        string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                        string val = line.Substring(eq + 1).Trim();
                        try
                        {
                            switch (key)
                            {
                                case "robot": m.Robot = val; break;
                                case "func": m.FuncNumber = int.Parse(val, ci); break;
                                case "posids": m.PosIDs = Array.ConvertAll(val.Split(','), v => int.Parse(v.Trim(), ci)); break;
                                case "wrist": m.WristID = int.Parse(val, ci); break;
                                case "pitch": m.PitchDeg = float.Parse(val, ci); break;
                                case "maxdelta": m.MaxAbsDelta = float.Parse(val, ci); break;
                                case "degree": m.Degree = int.Parse(val, ci); break;
                                case "range":
                                    { var a = Array.ConvertAll(val.Split(','), v => float.Parse(v.Trim(), ci)); if (a.Length >= 4) { m.RMin = a[0]; m.RMax = a[1]; m.ZMin = a[2]; m.ZMax = a[3]; } break; }
                                case "cr": m.Cr = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "cz": m.Cz = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "kr": m.Kr = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "kz": m.Kz = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "rmse":
                                    { var a = Array.ConvertAll(val.Split(','), v => float.Parse(v.Trim(), ci)); if (a.Length >= 2) { m.RmseBefore = a[0]; m.RmseAfter = a[1]; } break; }
                                case "dynrmse":
                                    { var a = Array.ConvertAll(val.Split(','), v => float.Parse(v.Trim(), ci)); if (a.Length >= 2) { m.DynRmseBefore = a[0]; m.DynRmseAfter = a[1]; } break; }
                                case "jids": m.JointIDs = Array.ConvertAll(val.Split(','), v => int.Parse(v.Trim(), ci)); break;
                                case "jc": m.Jc = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "jh": m.Jh = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "jrmse":
                                    { var a = Array.ConvertAll(val.Split(','), v => float.Parse(v.Trim(), ci)); if (a.Length >= 2) { m.JRmseBefore = a[0]; m.JRmseAfter = a[1]; } break; }
                                case "maxjoint": m.MaxAbsJointDeg = float.Parse(val, ci); break;
                                case "jtau": m.Jtau = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "jkap": m.Jkap = Array.ConvertAll(val.Split(','), v => double.Parse(v.Trim(), ci)); break;
                                case "jlagrmse":
                                    { var a = Array.ConvertAll(val.Split(','), v => float.Parse(v.Trim(), ci)); if (a.Length >= 2) { m.JLagRmseBefore = a[0]; m.JLagRmseAfter = a[1]; } break; }
                                case "maxlead": m.MaxLeadDeg = float.Parse(val, ci); break;
                                case "lagaccel": m.LagUseAccel = val.Trim() == "1"; break;
                                case "lagon": m.LagEnabled = val.Trim() == "1"; break;
                                case "sigmaramp": m.SigmaRamp = Math.Max(0.05f, float.Parse(val, ci)); break;
                                case "flex":
                                    { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.FlexGain = double.Parse(v.Trim(), ci); break; }
                                case "floorr":
                                case "floordz":
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash);
                                        float[] a = Array.ConvertAll(v.Trim().Split(','), t => float.Parse(t.Trim(), ci));
                                        if (key == "floorr") m.FloorR = a; else m.FloorDz = a;
                                        if (key == "floordz" && hash >= 0)
                                        {
                                            int i0 = val.IndexOf("기준 FK ", hash); float fr;
                                            if (i0 >= 0 && float.TryParse(val.Substring(i0 + 6).TrimEnd(')', ' '), NumberStyles.Float, ci, out fr)) m.FloorRef = fr;
                                        }
                                        break;
                                    }
                                case "flooron": { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.FloorMapEnabled = v.Trim() == "1"; break; }
                                case "floorcmd": { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.FloorFromCmd = v.Trim() == "1"; break; }
                                case "gpids": case "gpell": case "gpsig": case "gpon": case "gpx": case "gpbadr": case "gpdom":
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); v = v.Trim();
                                        if (m.Gp == null) m.Gp = new CSagGp { X = new double[0][], Y = new double[0] };
                                        if (key == "gpids") m.Gp.FeatIDs = Array.ConvertAll(v.Split(','), t => int.Parse(t.Trim(), ci));
                                        else if (key == "gpell") m.Gp.Ell = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), ci));
                                        else if (key == "gpsig") { var a = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), ci)); m.Gp.SigF = a[0]; m.Gp.SigN = a[1]; m.Gp.Ref = a[2]; if (a.Length > 3) m.Gp.MaxStd = a[3]; }
                                        else if (key == "gpon") m.Gp.Enabled = v == "1";
                                        else if (key == "gpbadr") { var a = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), ci)); m.Gp.BadBandMm = a[a.Length - 1]; m.Gp.BadR = new double[a.Length - 1]; Array.Copy(a, m.Gp.BadR, a.Length - 1); }
                                        else if (key == "gpdom") m.Gp.DomainDeg = double.Parse(v, ci);
                                        else
                                        {
                                            var a = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), ci));
                                            var xa = new double[a.Length - 1]; Array.Copy(a, xa, xa.Length);
                                            var lx = new List<double[]>(m.Gp.X); lx.Add(xa); m.Gp.X = lx.ToArray();
                                            var ly = new List<double>(m.Gp.Y); ly.Add(a[a.Length - 1]); m.Gp.Y = ly.ToArray();
                                        }
                                        break;
                                    }
                                case "excl":
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash);
                                        foreach (string t in v.Split(',')) { int k; if (int.TryParse(t.Trim(), NumberStyles.Integer, ci, out k)) lstExcl.Add(k); }
                                        break;
                                    }
                                case "s":
                                    {
                                        // 칸: 머리+명령관절 | 실측관절 | 방향 | 지레   (뒤 두 칸은 2026-09-28 관절 모델부터)
                                        string[] parts = val.Split('|');
                                        string head = parts[0];
                                        string tail = parts.Length > 1 ? parts[1] : "";
                                        var h = new List<string>(head.Split(',')); h.RemoveAll(x => x.Trim().Length == 0);
                                        if (h.Count < HEAD) break;
                                        var s = new CSagSample
                                        {
                                            CmdX = float.Parse(h[0], ci), CmdY = float.Parse(h[1], ci), CmdZ = float.Parse(h[2], ci),
                                            ActX = float.Parse(h[3], ci), ActY = float.Parse(h[4], ci), ActZ = float.Parse(h[5], ci),
                                            Approach = int.Parse(h[6], ci), Dynamic = h[7].Trim() == "1",
                                            Vr = float.Parse(h[8], ci), Vz = float.Parse(h[9], ci), Ar = float.Parse(h[10], ci), Az = float.Parse(h[11], ci)
                                        };
                                        if (h.Count > HEAD) s.JointCmd = Array.ConvertAll(h.GetRange(HEAD, h.Count - HEAD).ToArray(), v => float.Parse(v.Trim(), ci));
                                        var t = new List<string>(tail.Split(',')); t.RemoveAll(x => x.Trim().Length == 0);
                                        if (t.Count > 0) s.JointAct = Array.ConvertAll(t.ToArray(), v => float.Parse(v.Trim(), ci));
                                        if (parts.Length > 3)
                                        {
                                            var d = new List<string>(parts[2].Split(',')); d.RemoveAll(x => x.Trim().Length == 0);
                                            var l = new List<string>(parts[3].Split(',')); l.RemoveAll(x => x.Trim().Length == 0);
                                            if (d.Count > 0) s.JointDir = Array.ConvertAll(d.ToArray(), v => float.Parse(v.Trim(), ci));
                                            if (l.Count > 0) s.JointLever = Array.ConvertAll(l.ToArray(), v => float.Parse(v.Trim(), ci));
                                        }
                                        if (parts.Length > 5)
                                        {
                                            var v1 = new List<string>(parts[4].Split(',')); v1.RemoveAll(x => x.Trim().Length == 0);
                                            var a1 = new List<string>(parts[5].Split(',')); a1.RemoveAll(x => x.Trim().Length == 0);
                                            if (v1.Count > 0) s.JointVel = Array.ConvertAll(v1.ToArray(), v => float.Parse(v.Trim(), ci));
                                            if (a1.Count > 0) s.JointAcc = Array.ConvertAll(a1.ToArray(), v => float.Parse(v.Trim(), ci));
                                        }
                                        m.Samples.Add(s);
                                        break;
                                    }
                            }
                        }
                        catch { /* 손상 줄은 건너뛴다 */ }
                    }
                    foreach (int k in lstExcl) if (k >= 0 && k < m.Samples.Count) m.Samples[k].Excluded = true;
                    bool bBad = m.IsFitted && (m.Cz.Length != FeatCount(m.Degree) || m.Cr == null || m.Cr.Length != FeatCount(m.Degree));
                    if ((!m.IsFitted || bBad) && m.Samples.Count >= 2) { m.Fit(); m.FitDynamic(); }
                    if (m.Kr != null && m.Kr.Length != 2) m.Kr = null;
                    if (m.Kz != null && m.Kz.Length != 3) m.Kz = null;
                    if ((m.Kr == null) != (m.Kz == null)) m.Kr = m.Kz = null;
                    if ((m.Jc != null || m.Jh != null) && !m.HasJoint) m.Jc = m.Jh = null;   // 길이 어긋난 관절 계수 = 버린다 (자동 재학습은 안 한다 — 명시 호출만)
                    if ((m.Jtau != null || m.Jkap != null) && !m.HasJointLag) m.Jtau = m.Jkap = null;
                    if (m.Gp != null && (m.Gp.X.Length == 0 || !m.Gp.Solve())) m.Gp = null;       // 학습점으로 촐레스키 다시 풀기
                    return m;
                }

                public string Summary()
                {
                    if (HasJoint)
                    {
                        var sj = new StringBuilder();
                        for (int j = 0; j < JointIDs.Length; j++)
                            sj.AppendFormat(CultureInfo.InvariantCulture, "{0}ID{1} h{2:F1}°{3}", j > 0 ? " " : "", JointIDs[j], Jh[j],
                                HasJointLag && Jtau[j] > 0 ? string.Format(CultureInfo.InvariantCulture, " τ{0:F0}ms", Jtau[j] * 1000) : "");
                        return string.Format(CultureInfo.InvariantCulture,
                            "처짐 보상: 관절 모델(중력+데드밴드{5}) [{0}], 표본 {1}, 정지 TCP 오차 {2:F2}→{3:F2}mm{6}{4}",
                            sj, Samples.Count, JRmseBefore, JRmseAfter, Enabled ? "" : " [꺼짐]", HasJointLag && LagEnabled ? "+지연" : "",
                            HasJointLag ? string.Format(CultureInfo.InvariantCulture, ", 지연 τ 학습됨({0}, 주행 설명 Δz {1:F2}→{2:F2}mm)",
                                LagEnabled ? "켜짐" : "꺼짐 — LagEnabled", JLagRmseBefore, JLagRmseAfter) : "")
                            + (HasGp ? string.Format(CultureInfo.InvariantCulture, ", 바닥 GP {0}점 (길이 척도 팔 {1:F0}°, 두 상태 반경 {3}곳){2}", Gp.Count, Gp.Ell.Length > 1 ? Gp.Ell[1] : 0,
                                FloorMapEnabled && Gp.Enabled ? "" : " [꺼짐]", Gp.BadR == null ? 0 : Gp.BadR.Length)
                               : HasFloorMap ? string.Format(CultureInfo.InvariantCulture, ", 바닥 보정표 {0}점 r {1:F0}~{2:F0}mm Δz {3:+0.0;-0.0}~{4:+0.0;-0.0}mm{5}",
                                FloorR.Length, FloorR[0], FloorR[FloorR.Length - 1], FloorDzMin(), FloorDzMax(), FloorMapEnabled ? "" : " [꺼짐 — flooron=0]") : "");
                    }
                    if (!IsFitted) return string.Format("처짐 보상: 미학습 (표본 {0})", Samples.Count);
                    double dR0, dZ0, dR1, dZ1;
                    PredictStatic(RMin, 0, (ZMin + ZMax) / 2, out dR0, out dZ0);
                    PredictStatic(RMax, 0, (ZMin + ZMax) / 2, out dR1, out dZ1);
                    return string.Format(CultureInfo.InvariantCulture,
                        "처짐 보상: [{0}], 표본 {1}, r {2:F0}~{3:F0}mm (처짐 z {4:+0.0;-0.0}→{5:+0.0;-0.0}mm), 정지 잔차 {6:F2}mm{7}{8}",
                        Degree >= 3 ? "r,r²,z" : (Degree == 2 ? "r,r²" : "r"), Samples.Count, RMin, RMax, dZ0, dZ1, RmseAfter,
                        HasDynamic ? string.Format(CultureInfo.InvariantCulture, ", 주행항 있음(잔차 {0:F2}→{1:F2}mm)", DynRmseBefore, DynRmseAfter) : ", 주행항 없음",
                        Enabled ? "" : " [꺼짐]");
                }

                private static double[] Solve(double[,] A0, double[] b0)
                {
                    int n = b0.Length;
                    double[,] A = (double[,])A0.Clone(); double[] b = (double[])b0.Clone();
                    for (int c = 0; c < n; c++)
                    {
                        int p = c; double best = Math.Abs(A[c, c]);
                        for (int r = c + 1; r < n; r++) if (Math.Abs(A[r, c]) > best) { best = Math.Abs(A[r, c]); p = r; }
                        if (best < 1e-12) return null;
                        if (p != c)
                        {
                            for (int j = 0; j < n; j++) { double t = A[c, j]; A[c, j] = A[p, j]; A[p, j] = t; }
                            double tb = b[c]; b[c] = b[p]; b[p] = tb;
                        }
                        for (int r = c + 1; r < n; r++)
                        {
                            double m = A[r, c] / A[c, c];
                            if (m == 0) continue;
                            for (int j = c; j < n; j++) A[r, j] -= m * A[c, j];
                            b[r] -= m * b[c];
                        }
                    }
                    double[] x = new double[n];
                    for (int r = n - 1; r >= 0; r--)
                    {
                        double s = b[r];
                        for (int j = r + 1; j < n; j++) s -= A[r, j] * x[j];
                        x[r] = s / A[r, r];
                    }
                    return x;
                }
            }

            // ────────────────────────────────────────────────────────────
            // C3d 쪽 상태 + 적용 훅 + 경로 추적 버퍼
            // ────────────────────────────────────────────────────────────
            private CSagComp m_SagComp = null;
            private float[] m_afSagLastDelta = new float[3];
            private readonly List<float[]> m_lstSagTrace = new List<float[]>();   // [ms, x,y,z(보정 전), vr,vz,ar,az]
            private volatile bool m_bSagTraceOn = false;

            /// <summary>모델 파일 로드 → 이후 PlayXyz/PlayXyzPath 가 자동 보상. 실패하면 false (기존 모델 유지).</summary>
            public bool SagComp_Load(string strPath)
            {
                CSagComp m = CSagComp.Load(strPath);
                if (m == null || !m.IsUsable) return false;
                SagComp_Switch(m);
                return true;
            }
            public void SagComp_Set(CSagComp model) { SagComp_Switch(model); }
            public void SagComp_Clear() { SagComp_Switch(null); }
            public bool SagComp_IsLoaded { get { return m_SagComp != null && m_SagComp.IsUsable; } }
            public CSagComp SagComp_Get() { return m_SagComp; }
            public void SagComp_Enable(bool bOn) { if (m_SagComp != null) m_SagComp.Enabled = bOn; SagComp_Switch(m_SagComp); }
            /// <summary>직전 IK 호출에 적용된 보상 (Δx,Δy,Δz mm) — 진단용 (직교 모델)</summary>
            public float[] SagComp_LastDelta { get { return (float[])m_afSagLastDelta.Clone(); } }
            /// <summary>직전 출력 틱에 얹은 관절 보정 (deg, 관절 모델 JointIDs 순) — 진단용</summary>
            public float[] SagComp_LastJointCorr { get { return (float[])m_afSagLastJoint.Clone(); } }

            // ────────────────────────────────────────────────────────────
            // 관절 모델: 방향 추적 + 중력 지레 + 실물 출력 훅
            // ────────────────────────────────────────────────────────────
            private const float SAG_DIR_THR_DEG = 0.3f;                                    // 이만큼 움직여야 "그쪽으로 움직였다"
            private readonly Dictionary<int, float> m_dicSagAnchor = new Dictionary<int, float>();   // 방향 추적 기준점 (놀이 연산자)
            private readonly Dictionary<int, int> m_dicSagDir = new Dictionary<int, int>();          // 관절별 마지막 이동 방향
            private readonly Dictionary<int, float> m_dicSagSigma = new Dictionary<int, float>();    // 보상용 매끈한 방향 (−1..+1)
            private readonly Dictionary<int, float> m_dicSagPrev = new Dictionary<int, float>();
            private System.Diagnostics.Stopwatch m_swSagBlend = null;                       // 보정 전환 시계 — 전환 뒤 첫 출력 틱에 시작
            private readonly Dictionary<int, float> m_dicSagApplied = new Dictionary<int, float>();  // 직전 출력 틱에 실제로 얹은 보정 (관절 ID → deg, 전환 포함)
            private Dictionary<int, float> m_dicSagFrom = null;                                      // 전환 순간 걸려 있던 보정 — 여기서 새 보정으로 1초에 걸쳐 넘어간다

            /// <summary>보정 모델 전환 — 즉시 0 으로 떨어뜨리지 않고, 전환 순간 실물에 걸려 있던 보정에서 새 모델의 보정으로 1초에 걸쳐 넘어간다.
            /// ★09-28 버그: 전환마다 페이드를 0 부터 다시 시작해, 먼 곳(보정이 끝을 20~30mm 올리던 곳)에서 다음 동작 첫 틱에 끝이 뚝 떨어져
            /// 바닥을 내려찍었다(사용자 목격 3회 — 먼 3점 뒤 전환마다). null(끄기)도 같은 방식으로 1초에 걸쳐 빠진다.</summary>
            private void SagComp_Switch(CSagComp m)
            {
                lock (m_dicSagDir) m_dicSagFrom = new Dictionary<int, float>(m_dicSagApplied);
                m_SagComp = m;
                m_swSagBlend = null;
            }
            private float[] m_afSagLastJoint = new float[0];
            // 관절 명령 속도·가속도 추정 — 출력 틱마다 (이번 각 − 지난 각) / 모션 시계 간격. 명령이 시간의 매끈한 함수(smoothstep)라
            //   걸러 낼 잡음이 거의 없다. 학습 표본(SurveyDynamic)과 실행(앞서 보내기)이 **같은 추정기**를 쓴다.
            private const int SAG_VEL_GAP_MS = 150;                                          // 이보다 긴 틈 = 새 동작 (속도 0 에서 다시)
            private readonly Dictionary<int, float> m_dicSagVelDeg = new Dictionary<int, float>();
            private readonly Dictionary<int, int> m_dicSagVelT = new Dictionary<int, int>();
            private readonly Dictionary<int, float> m_dicSagOmega = new Dictionary<int, float>();    // deg/s
            private readonly Dictionary<int, float> m_dicSagAlpha = new Dictionary<int, float>();    // deg/s²

            /// <summary>방향 추적 초기화 — 토크 ON 직후(CScene_t) 처럼 실물 자세를 새로 잡았을 때 부른다.</summary>
            public void SagComp_JointReset()
            {
                lock (m_dicSagDir)
                {
                    m_dicSagAnchor.Clear(); m_dicSagDir.Clear(); m_dicSagSigma.Clear(); m_dicSagPrev.Clear();
                    m_dicSagVelDeg.Clear(); m_dicSagVelT.Clear(); m_dicSagOmega.Clear(); m_dicSagAlpha.Clear();
                    m_dicSagApplied.Clear(); m_dicSagFrom = null;    // 실물 자세를 새로 잡았다 — 걸려 있는 보정 없음에서 시작
                }
                m_swSagBlend = null;
            }

            private void SagComp_TrackVel(int id, float deg, int nNowMs)
            {
                float p; int t;
                if (!m_dicSagVelDeg.TryGetValue(id, out p) || !m_dicSagVelT.TryGetValue(id, out t))
                { m_dicSagVelDeg[id] = deg; m_dicSagVelT[id] = nNowMs; m_dicSagOmega[id] = 0; m_dicSagAlpha[id] = 0; return; }
                int dt = nNowMs - t;
                if (dt < 3) return;                                                        // 같은 틱에 두 번 불림 — 그대로
                float w0; m_dicSagOmega.TryGetValue(id, out w0);
                if (dt > SAG_VEL_GAP_MS) { m_dicSagOmega[id] = 0; m_dicSagAlpha[id] = 0; }
                else
                {
                    float w = (deg - p) * 1000f / dt;
                    float a0; m_dicSagAlpha.TryGetValue(id, out a0);
                    m_dicSagOmega[id] = w;
                    m_dicSagAlpha[id] = 0.5f * a0 + 0.5f * ((w - w0) * 1000f / dt);        // 2차 차분이라 가볍게 거른다
                }
                m_dicSagVelDeg[id] = deg; m_dicSagVelT[id] = nNowMs;
            }

            /// <summary>관절별 현재 명령 속도 (deg/s) — 출력 훅이 추정한 값. 주행 표본 기록용.</summary>
            public float[] SagComp_JointVelNow(int[] ids)
            {
                float[] v = new float[ids.Length];
                lock (m_dicSagDir) for (int i = 0; i < ids.Length; i++) { float x; if (m_dicSagOmega.TryGetValue(ids[i], out x)) v[i] = x; }
                return v;
            }
            /// <summary>관절별 현재 명령 가속도 (deg/s²)</summary>
            public float[] SagComp_JointAccNow(int[] ids)
            {
                float[] v = new float[ids.Length];
                lock (m_dicSagDir) for (int i = 0; i < ids.Length; i++) { float x; if (m_dicSagAlpha.TryGetValue(ids[i], out x)) v[i] = x; }
                return v;
            }

            /// <summary>관절별 마지막 이동 방향 (+1/−1, 0 = 아직 모름) — 측정 표본에 기록한다.</summary>
            public float[] SagComp_JointDirNow(int[] ids)
            {
                float[] d = new float[ids.Length];
                lock (m_dicSagDir)
                    for (int i = 0; i < ids.Length; i++) { int v; if (m_dicSagDir.TryGetValue(ids[i], out v)) d[i] = v; }
                return d;
            }

            // 놀이(play) 연산자: 기준점에서 문턱 이상 반대로 가면 방향이 바뀌고, 같은 방향으로 가는 동안은 기준점을 끝점으로 끌고 간다
            private void SagComp_TrackDir(int id, float deg)
            {
                float a;
                if (!m_dicSagAnchor.TryGetValue(id, out a)) { m_dicSagAnchor[id] = deg; if (!m_dicSagDir.ContainsKey(id)) m_dicSagDir[id] = 0; return; }
                int d; m_dicSagDir.TryGetValue(id, out d);
                float diff = deg - a;
                if (d > 0) { if (diff > 0) m_dicSagAnchor[id] = deg; else if (-diff >= SAG_DIR_THR_DEG) { m_dicSagDir[id] = -1; m_dicSagAnchor[id] = deg; } }
                else if (d < 0) { if (diff < 0) m_dicSagAnchor[id] = deg; else if (diff >= SAG_DIR_THR_DEG) { m_dicSagDir[id] = +1; m_dicSagAnchor[id] = deg; } }
                else { if (diff >= SAG_DIR_THR_DEG) { m_dicSagDir[id] = +1; m_dicSagAnchor[id] = deg; } else if (-diff >= SAG_DIR_THR_DEG) { m_dicSagDir[id] = -1; m_dicSagAnchor[id] = deg; } }
            }

            /// <summary>실물로 나가는 관절각에 관절 보상을 얹는다 — CScene_t.SendPose 가 모션 틱마다 부른다.
            /// afDeg = ids 순서의 3D(이상) 관절각 → 보정된 실물 명령으로 바뀐다. **3D 모델은 건드리지 않는다**
            /// (그래서 IK 시드·현재 TCP·경로 시작점이 보정에 오염되지 않는다 — 09-28 주행 검증 실패의 구조적 원인 제거).
            /// 명령 = θ − c·L(θ) + h·σ.  σ 는 이동량/h 로 −1..+1 사이를 매끈하게 오가서, 방향이 바뀌는 순간에도
            /// 명령이 2h 만큼 튀지 않는다. 모델을 켠 뒤 1초에 걸쳐 들어온다.
            /// 지연 모델이 있으면 τ·ω + κ·α 만큼 앞서 보낸다 (상한 MaxLeadDeg). smoothstep 이라 동작 끝에서 ω→0 → 앞섬도 0.
            /// 모델 유무와 무관하게 관절별 이동 방향·속도를 추적한다 (측정 표본 기록용).</summary>
            public void SagComp_JointOut(int[] ids, float[] afDeg)
            {
                if (ids == null || afDeg == null) return;
                int nNow = MotionTick_ms();
                lock (m_dicSagDir)
                    for (int i = 0; i < ids.Length && i < afDeg.Length; i++) { SagComp_TrackDir(ids[i], afDeg[i]); SagComp_TrackVel(ids[i], afDeg[i], nNow); }
                CSagComp m = m_SagComp;
                bool bModel = m != null && m.Enabled && m.HasJoint;
                Dictionary<int, float> from = m_dicSagFrom;
                if (!bModel && (from == null || from.Count == 0))
                {
                    lock (m_dicSagDir) { m_dicSagApplied.Clear(); m_dicSagFrom = null; }
                    m_afSagLastJoint = new float[0]; m_fSagLastFloorDz = 0; m_swSagBlend = null;
                    return;
                }
                if (m_swSagBlend == null) m_swSagBlend = System.Diagnostics.Stopwatch.StartNew();
                double w = Math.Min(1.0, m_swSagBlend.ElapsedMilliseconds / 1000.0);

                var target = new Dictionary<int, float>();                               // 새 모델의 보정 (전환 전 값, 관절 ID → deg)
                float dzMap = 0;
                if (bModel)
                {
                    int nj = m.JointIDs.Length;
                    int[] idx = new int[nj]; float[] pose = new float[nj];
                    for (int j = 0; j < nj; j++) { idx[j] = Array.IndexOf(ids, m.JointIDs[j]); pose[j] = idx[j] >= 0 ? afDeg[idx[j]] : GetData(m.JointIDs[j]); }
                    float[] lev = SagComp_JointLevers(m.FuncNumber, m.JointIDs, pose);
                    lock (m_dicSagDir)
                        for (int j = 0; j < nj; j++)
                        {
                            int id = m.JointIDs[j];
                            float sg, prev;
                            if (!m_dicSagSigma.TryGetValue(id, out sg)) { int d; m_dicSagDir.TryGetValue(id, out d); sg = d; }   // 첫 틱 = 추적된 방향에서 시작
                            else if (m_dicSagPrev.TryGetValue(id, out prev))
                            {
                                sg += (pose[j] - prev) / (float)Math.Max(0.2, m.SigmaRamp * m.Jh[j]);
                                if (sg > 1f) sg = 1f; if (sg < -1f) sg = -1f;
                            }
                            m_dicSagSigma[id] = sg; m_dicSagPrev[id] = pose[j];
                            double c = m.JointCorrStatic(j, lev[j], sg);             // = −(c·L·(1+κ) − h·σ)
                            if (m.HasJointLag && m.LagEnabled)
                            {
                                float om, al; m_dicSagOmega.TryGetValue(id, out om); m_dicSagAlpha.TryGetValue(id, out al);
                                double lead = -m.PredictJointLag(j, om, al);                     // = τ·ω + κ·α
                                if (lead > m.MaxLeadDeg) lead = m.MaxLeadDeg; if (lead < -m.MaxLeadDeg) lead = -m.MaxLeadDeg;
                                c += lead;
                            }
                            if (c > m.MaxAbsJointDeg) c = m.MaxAbsJointDeg; if (c < -m.MaxAbsJointDeg) c = -m.MaxAbsJointDeg;
                            target[id] = (float)c;
                        }
                    // 바닥 보정표 — 이상 자세(보정 전)의 TCP 반경에서 Δz 를 읽어 관절 12·13·14 로 바꾼다
                    if ((m.HasGp ? m.Gp.Enabled : m.HasFloorMap) && m.FloorMapEnabled && m.PosIDs != null && m.PosIDs.Length >= 3)
                    {
                        int[] fid = { m.PosIDs[1], m.PosIDs[2], m.WristID };
                        float[] fq = new float[3];
                        for (int k = 0; k < 3; k++)
                        {
                            int fi = Array.IndexOf(ids, fid[k]);
                            int jm = Array.IndexOf(m.JointIDs, fid[k]);
                            fq[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afDeg[fi] : GetData(fid[k]));
                        }
                        double r0, z0, dz;
                        if (m.HasGp)
                        {
                            double[] gx = new double[m.Gp.FeatIDs.Length];
                            for (int k = 0; k < gx.Length; k++)
                            {
                                int jm = Array.IndexOf(m.JointIDs, m.Gp.FeatIDs[k]), fi = Array.IndexOf(ids, m.Gp.FeatIDs[k]);
                                gx[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afDeg[fi] : GetData(m.Gp.FeatIDs[k]));
                            }
                            SagComp_TcpRZ(m.FuncNumber, fid, fq, out r0, out z0);
                            double gsd; dz = m.Gp.Correction(gx, r0, out gsd); m_fSagLastGpStd = (float)gsd;
                        }
                        else { SagComp_TcpRZ(m.FuncNumber, fid, fq, out r0, out z0); dz = m.FloorDzAt(r0); }
                        double[] dq;
                        if (Math.Abs(dz) > 1e-3 && SagComp_FloorJointDelta(m.FuncNumber, fid, fq, dz, out dq))
                        {
                            dzMap = (float)dz;
                            for (int k = 0; k < 3; k++)
                            {
                                float t0; target.TryGetValue(fid[k], out t0);
                                target[fid[k]] = t0 + (float)Math.Max(-m.MaxAbsJointDeg, Math.Min(m.MaxAbsJointDeg, dq[k]));
                            }
                        }
                    }
                }
                // 출력 = w·새 보정 + (1−w)·전환 순간 보정 — 모델 전환·끄기에도 실물 명령이 이어진다
                lock (m_dicSagDir)
                {
                    m_dicSagApplied.Clear();
                    for (int i = 0; i < ids.Length && i < afDeg.Length; i++)
                    {
                        float tn, to;
                        target.TryGetValue(ids[i], out tn);
                        if (from == null || !from.TryGetValue(ids[i], out to)) to = 0;
                        float o = (float)(w * tn + (1 - w) * to);
                        if (o == 0) continue;
                        afDeg[i] += o;
                        m_dicSagApplied[ids[i]] = o;
                    }
                    if (w >= 1.0) m_dicSagFrom = null;
                }
                if (bModel)
                {
                    float[] corr = new float[m.JointIDs.Length];
                    lock (m_dicSagDir) for (int j = 0; j < corr.Length; j++) { float o; if (m_dicSagApplied.TryGetValue(m.JointIDs[j], out o)) corr[j] = o; }
                    m_afSagLastJoint = corr;
                }
                else m_afSagLastJoint = new float[0];
                m_fSagLastFloorDz = (float)(w * dzMap);
            }

            private float m_fSagLastFloorDz = 0, m_fSagLastGpStd = float.NaN;
            /// <summary>직전 출력 틱의 바닥 GP 예측 표준편차 (mm) — 진단용</summary>
            public float SagComp_LastGpStd { get { return m_fSagLastGpStd; } }
            /// <summary>직전 출력 틱에 바닥 보정표로 얹은 끝 높이 보정 (mm) — 진단용</summary>
            public float SagComp_LastFloorDz { get { return m_fSagLastFloorDz; } }

            // 관절 (12,13,14) 자세 q 에서의 TCP 수평 반경·높이 — 3D 관절값은 잠깐 바꿨다가 원복
            private void SagComp_TcpRZ(int nFunc, int[] fid, float[] q, out double r, out double z)
            {
                float[] keep = new float[3];
                for (int k = 0; k < 3; k++) keep[k] = GetData(fid[k]);
                float x, y, zz;
                try
                {
                    for (int k = 0; k < 3; k++) SetData(fid[k], q[k]);
                    CalcF(nFunc, -1, false, out x, out y, out zz);
                }
                finally { for (int k = 0; k < 3; k++) SetData(fid[k], keep[k]); }
                r = Math.Sqrt((double)x * x + (double)y * y); z = zz;
            }

            /// <summary>끝을 dz(mm) 만큼 올리는 관절 (12,13,14) 변화 (deg) — 반경 방향 위치와 공구 피치(세 관절 합)는 그대로.
            /// 피치를 지키는 두 방향 (+12,−14), (+13,−14) 의 수치 야코비안(±0.5°) 2×2 를 풀고 뉴턴 1회로 다듬는다 (CalcF 4회).</summary>
            public bool SagComp_FloorJointDelta(int nFunc, int[] fid, float[] q, double dz, out double[] dq)
            {
                dq = null;
                const float D = 0.5f;
                double r0, z0, ra, za, rb, zb;
                SagComp_TcpRZ(nFunc, fid, q, out r0, out z0);
                SagComp_TcpRZ(nFunc, fid, new float[] { q[0] + D, q[1], q[2] - D }, out ra, out za);
                SagComp_TcpRZ(nFunc, fid, new float[] { q[0], q[1] + D, q[2] - D }, out rb, out zb);
                double j11 = (ra - r0) / D, j12 = (rb - r0) / D, j21 = (za - z0) / D, j22 = (zb - z0) / D;
                double det = j11 * j22 - j12 * j21;
                if (Math.Abs(det) < 1e-6) return false;
                double a = (-j12 * dz) / det, b = (j11 * dz) / det;       // [j11 j12; j21 j22]·[a b] = [0 dz]
                // 한 번 더 (뉴턴 1회, 같은 야코비안) — ±10mm 보정에서 선형 근사로 반경이 0.2~0.5mm 밀리던 것을 없앤다
                double r1, z1;
                SagComp_TcpRZ(nFunc, fid, new float[] { q[0] + (float)a, q[1] + (float)b, q[2] - (float)(a + b) }, out r1, out z1);
                double er = r0 - r1, ez = (z0 + dz) - z1;
                a += (j22 * er - j12 * ez) / det; b += (j11 * ez - j21 * er) / det;
                dq = new double[] { a, b, -a - b };
                return true;
            }

            /// <summary>관절별 중력 지레 L_j = ∂z̄_j/∂θ_j (mm/deg). z̄_j = 관절 j 하류 질량점(뒤 관절 원점들 + TCP)의 평균 높이.
            /// 수치 미분(±0.5°) 이라 축 방향을 몰라도 되고 어떤 체인에도 쓴다. jids 는 체인 순서.
            /// 3D 관절값은 잠깐 바꿨다가 원복한다.</summary>
            public float[] SagComp_JointLevers(int nFunc, int[] jids, float[] pose)
            {
                int nj = jids.Length;
                float[] keep = new float[nj], lev = new float[nj];
                for (int j = 0; j < nj; j++) keep[j] = GetData(jids[j]);
                const float D = 0.5f;
                try
                {
                    for (int j = 0; j < nj; j++) SetData(jids[j], pose[j]);
                    for (int j = 0; j < nj; j++)
                    {
                        SetData(jids[j], pose[j] + D); double zp = SagComp_MeanZ(nFunc, jids, j);
                        SetData(jids[j], pose[j] - D); double zm = SagComp_MeanZ(nFunc, jids, j);
                        SetData(jids[j], pose[j]);
                        lev[j] = (float)((zp - zm) / (2 * D));
                    }
                }
                finally { for (int j = 0; j < nj; j++) SetData(jids[j], keep[j]); }
                return lev;
            }
            private double SagComp_MeanZ(int nFunc, int[] jids, int j)
            {
                double s = 0; int n = 0; float x, y, z;
                for (int k = j + 1; k < jids.Length; k++) { CalcF(nFunc, jids[k], false, out x, out y, out z); s += z; n++; }   // 관절 k 원점
                CalcF(nFunc, -1, false, out x, out y, out z); s += z; n++;                                                   // TCP
                return s / n;
            }

            /// <summary>관절 모델이 예측하는 TCP 처짐 (실측 예상 − 명령, mm) — FK(명령 + 예상 관절 오차) − FK(명령).</summary>
            private void SagComp_JointPredictTcp(CSagComp m, float[] cmd, float[] dir, float[] lev, out float dx, out float dy, out float dz)
            {
                dx = dy = dz = 0;
                if (m == null || !m.HasJoint || cmd == null || dir == null || lev == null) return;
                int nj = m.JointIDs.Length;
                float[] keep = new float[nj];
                for (int j = 0; j < nj; j++) keep[j] = GetData(m.JointIDs[j]);
                try
                {
                    float x0, y0, z0, x1, y1, z1;
                    for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], cmd[j]);
                    CalcF(m.FuncNumber, -1, false, out x0, out y0, out z0);
                    for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], (float)(cmd[j] + m.PredictJointErr(j, lev[j], dir[j])));
                    CalcF(m.FuncNumber, -1, false, out x1, out y1, out z1);
                    dx = x1 - x0; dy = y1 - y0; dz = z1 - z0;
                }
                finally { for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], keep[j]); }
            }

            /// <summary>관절 지연 학습 + TCP 기준 평가. 관절 속도가 기록되지 않은 옛 주행 표본은 같은 경로(연속·같은 방향)의
            /// 이웃 표본으로 유도한다: ω = (dθ/dr)·v_r,  α = (dθ/dr)·a_r + (d²θ/dr²)·v_r²  (v_r·a_r 는 해석식 기록값).</summary>
            public string SagComp_FitJointLag(CSagComp m)
            {
                if (m == null || !m.HasJoint) return "지연 학습: 정지 관절 모델이 없어 생략";
                int nj = m.JointIDs.Length;
                int nDerived = SagComp_DeriveJointVel(m, nj);
                string r = m.FitJointLag();
                if (!m.HasJointLag) return r;
                double se0 = 0, se1 = 0; int n = 0;
                foreach (var s in m.Samples)
                {
                    if (!s.Dynamic || s.Excluded || s.JointCmd == null || s.JointDir == null || s.JointLever == null || s.JointVel == null || s.JointCmd.Length < nj) continue;
                    float[] p0 = new float[nj], p1 = new float[nj];
                    for (int j = 0; j < nj; j++)
                    {
                        double st = m.PredictJointErr(j, s.JointLever[j], s.JointDir[j]);
                        double lg = m.PredictJointLag(j, s.JointVel[j], s.JointAcc != null && s.JointAcc.Length > j ? s.JointAcc[j] : 0);
                        p0[j] = (float)(s.JointCmd[j] + st); p1[j] = (float)(s.JointCmd[j] + st + lg);
                    }
                    float x, y, z0, z1;
                    SagComp_FkPose(m, p0, out x, out y, out z0);
                    SagComp_FkPose(m, p1, out x, out y, out z1);
                    se0 += (s.ActZ - z0) * (s.ActZ - z0); se1 += (s.ActZ - z1) * (s.ActZ - z1); n++;
                }
                if (n > 0) { m.JLagRmseBefore = (float)Math.Sqrt(se0 / n); m.JLagRmseAfter = (float)Math.Sqrt(se1 / n); }
                return r + string.Format(CultureInfo.InvariantCulture,
                    "\r\n  주행 TCP 기준({0}개{1}): 예측과 실측의 Δz RMSE — 정지 관절 모델만 {2:F2}mm → 지연까지 {3:F2}mm",
                    n, nDerived > 0 ? string.Format(", 속도 유도 {0}개", nDerived) : "", m.JLagRmseBefore, m.JLagRmseAfter);
            }

            // 속도 미기록 주행 표본의 ω·α 를 같은 경로 이웃으로 유도한다. 반환 = 유도한 표본 수.
            private int SagComp_DeriveJointVel(CSagComp m, int nj)
            {
                var dy = m.Samples.FindAll(s => s.Dynamic);
                int nDone = 0, i0 = 0;
                while (i0 < dy.Count)
                {
                    int i1 = i0;   // [i0, i1] = 한 경로 (같은 접근 방향, 이웃 간 r 이 60mm 안에서 이어짐)
                    while (i1 + 1 < dy.Count && dy[i1 + 1].Approach == dy[i0].Approach && Math.Abs(dy[i1 + 1].R - dy[i1].R) < 60f) i1++;
                    for (int i = i0 + 1; i < i1; i++)
                    {
                        CSagSample a = dy[i - 1], s = dy[i], b = dy[i + 1];
                        if (s.JointVel != null || a.JointCmd == null || s.JointCmd == null || b.JointCmd == null ||
                            a.JointCmd.Length < nj || s.JointCmd.Length < nj || b.JointCmd.Length < nj) continue;
                        double h1 = s.R - a.R, h2 = b.R - s.R;
                        if (Math.Abs(h1 + h2) < 0.5) continue;
                        bool b2 = Math.Abs(h1) > 0.2 && Math.Abs(h2) > 0.2;
                        float[] w = new float[nj], al = new float[nj];
                        for (int j = 0; j < nj; j++)
                        {
                            double d1 = (b.JointCmd[j] - a.JointCmd[j]) / (h1 + h2);
                            double d2 = b2 ? 2 * (h1 * b.JointCmd[j] - (h1 + h2) * s.JointCmd[j] + h2 * a.JointCmd[j]) / (h1 * h2 * (h1 + h2)) : 0;
                            w[j] = (float)(d1 * s.Vr);
                            al[j] = (float)(d1 * s.Ar + d2 * s.Vr * s.Vr);
                        }
                        s.JointVel = w; s.JointAcc = al; nDone++;
                    }
                    i0 = i1 + 1;
                }
                return nDone;
            }

            private void SagComp_FkPose(CSagComp m, float[] pose, out float x, out float y, out float z)
            {
                int nj = m.JointIDs.Length;
                float[] keep = new float[nj];
                for (int j = 0; j < nj; j++) keep[j] = GetData(m.JointIDs[j]);
                try
                {
                    for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], pose[j]);
                    CalcF(m.FuncNumber, -1, false, out x, out y, out z);
                }
                finally { for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], keep[j]); }
            }

            /// <summary>측정 중 바닥 접촉 의심 표본을 자동으로 학습에서 뺀다 (기본 켬).</summary>
            public bool SagComp_ContactGuard = true;

            /// <summary>바닥 접촉 의심 표본 검출 — 한 점씩 빼고 학습해 그 점의 TCP z 를 예측하고, 실측이 예측보다
            /// 두드러지게 **들려** 있으면(바닥이 팔을 받쳐 덜 처짐) 접촉으로 보고 Excluded 로 표시한다.
            /// 문턱 = max(2mm, 2.5 × 강건 척도(1.4826·중앙값|들림|)). 한 번에 최대 표본의 1/5.
            /// ★09-28 실측: r=260 멀리(→) 가 두 세션 모두 깨끗한 모델보다 +2.9mm 들리고 2.6~3.1mm 덜 뻗었다
            /// (사용자가 그리퍼가 바닥에 닿는 것을 목격). 반환 = 로그 문장 (뺀 게 없으면 "").</summary>
            public string SagComp_ExcludeContact(CSagComp m)
            {
                if (m == null || m.JointIDs == null) return "";
                int nj = m.JointIDs.Length;
                var use = m.Samples.FindAll(s => !s.Dynamic && !s.Excluded && s.JointCmd != null && s.JointDir != null && s.JointLever != null &&
                                                 s.JointCmd.Length >= nj && Array.IndexOf(s.JointDir, 0f) < 0);
                if (use.Count < 6) return "";
                var lift = new double[use.Count];
                for (int i = 0; i < use.Count; i++)
                {
                    // 같은 지점·같은 접근 방향 표본은 함께 뺀다 — 여러 세션을 합치면 접촉 표본끼리 서로를 "정상" 으로 받쳐 준다
                    var t = new CSagComp { FuncNumber = m.FuncNumber, PosIDs = m.PosIDs, WristID = m.WristID, JointIDs = m.JointIDs, MaxAbsJointDeg = m.MaxAbsJointDeg };
                    for (int k = 0; k < use.Count; k++)
                    {
                        bool bSame = Math.Abs(use[k].CmdX - use[i].CmdX) < 0.5f && Math.Abs(use[k].CmdY - use[i].CmdY) < 0.5f &&
                                     Math.Abs(use[k].CmdZ - use[i].CmdZ) < 0.5f && Math.Sign(use[k].Approach) == Math.Sign(use[i].Approach);
                        if (!bSame) t.Samples.Add(use[k]);
                    }
                    t.FitJoint();
                    float dx, dy, dz; SagComp_JointPredictTcp(t, use[i].JointCmd, use[i].JointDir, use[i].JointLever, out dx, out dy, out dz);
                    lift[i] = t.HasJoint ? use[i].DZ - dz : 0;
                }
                var abs = new List<double>(); foreach (double v in lift) abs.Add(Math.Abs(v)); abs.Sort();
                double scale = 1.4826 * abs[abs.Count / 2];
                double thr = Math.Max(2.0, 2.5 * scale);
                var cand = new List<int>();
                for (int i = 0; i < use.Count; i++) if (lift[i] > thr) cand.Add(i);
                cand.Sort((p, q) => lift[q].CompareTo(lift[p]));
                if (cand.Count > use.Count / 5) cand.RemoveRange(use.Count / 5, cand.Count - use.Count / 5);
                if (cand.Count == 0) return "";
                var sb = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "바닥 접촉 의심으로 학습에서 뺌 (들림 문턱 {0:F2}mm):", thr));
                foreach (int i in cand)
                {
                    use[i].Excluded = true;
                    sb.AppendFormat(CultureInfo.InvariantCulture, " ({0:F0},{1:F0},{2:F0}){3} 들림 +{4:F2}mm",
                        use[i].CmdX, use[i].CmdY, use[i].CmdZ, use[i].Approach > 0 ? "→" : (use[i].Approach < 0 ? "←" : ""), lift[i]);
                }
                sb.Append(" — 측정 높이를 올리거나 바닥과의 간격을 확인하세요");
                return sb.ToString();
            }

            /// <summary>관절 모델 학습 + TCP 기준 평가(FK). bRefeature = 표본의 방향·지레를 정지 표본의 명령 관절각 순서로
            /// 다시 계산한다 — 방향·지레 없이 저장된 옛 표본용. 첫 표본의 직전 자세는 afStartPose (null = 첫 표본 방향 모름 → 제외).</summary>
            public string SagComp_FitJoint(CSagComp m, float[] afStartPose, bool bRefeature)
            {
                if (m == null) return "모델 없음";
                int nj = m.PosIDs.Length + 1;
                int[] jids = new int[nj]; Array.Copy(m.PosIDs, jids, m.PosIDs.Length); jids[nj - 1] = m.WristID;
                m.JointIDs = jids;
                if (bRefeature)
                {
                    float[] prev = afStartPose; float[] dir = new float[nj];
                    foreach (var s in m.Samples)
                    {
                        if (s.Dynamic || s.JointCmd == null || s.JointCmd.Length < nj) continue;
                        if (prev != null)
                            for (int j = 0; j < nj; j++)
                            {
                                float df = s.JointCmd[j] - prev[j];
                                if (Math.Abs(df) >= SAG_DIR_THR_DEG) dir[j] = df > 0 ? 1f : -1f;   // 안 움직였으면 직전 방향 유지
                            }
                        s.JointDir = (float[])dir.Clone();
                        s.JointLever = SagComp_JointLevers(m.FuncNumber, jids, s.JointCmd);
                        prev = s.JointCmd;
                    }
                }
                string r = m.FitJoint();
                if (!m.HasJoint) return r;
                string strContact = SagComp_ContactGuard ? SagComp_ExcludeContact(m) : "";
                if (strContact.Length > 0) r = m.FitJoint() + "\r\n  " + strContact;
                double se0 = 0, se1 = 0, sz0 = 0, sz1 = 0; int n = 0;
                foreach (var s in m.Samples)
                {
                    if (s.Dynamic || s.Excluded || s.JointCmd == null || s.JointDir == null || s.JointLever == null || s.JointCmd.Length < nj) continue;
                    bool bKnown = true; for (int j = 0; j < nj; j++) if (s.JointDir[j] == 0) bKnown = false;
                    if (!bKnown) continue;
                    float dx, dy, dz; SagComp_JointPredictTcp(m, s.JointCmd, s.JointDir, s.JointLever, out dx, out dy, out dz);
                    double ex0 = s.DX, ey0 = s.DY, ez0 = s.DZ;                   // 원시 처짐 (실측 − 명령)
                    double ex1 = ex0 - dx, ey1 = ey0 - dy, ez1 = ez0 - dz;      // 모델 잔차
                    se0 += ex0 * ex0 + ey0 * ey0 + ez0 * ez0; se1 += ex1 * ex1 + ey1 * ey1 + ez1 * ez1;
                    sz0 += ez0 * ez0; sz1 += ez1 * ez1; n++;
                }
                if (n > 0) { m.JRmseBefore = (float)Math.Sqrt(se0 / n); m.JRmseAfter = (float)Math.Sqrt(se1 / n); }
                return r + string.Format(CultureInfo.InvariantCulture,
                    "\r\n  관절 모델 TCP 기준(정지 {0}개): 3D 오차 RMSE {1:F2}→{2:F2}mm, Δz RMSE {3:F2}→{4:F2}mm",
                    n, m.JRmseBefore, m.JRmseAfter, n > 0 ? Math.Sqrt(sz0 / n) : 0, n > 0 ? Math.Sqrt(sz1 / n) : 0);
            }

            /// <summary>IK 직전 훅(정지) — 모델이 없으면 그대로(비용 0).</summary>
            private void SagComp_Apply(ref float fX, ref float fY, ref float fZ)
            { SagComp_Apply(ref fX, ref fY, ref fZ, 0, 0, 0, 0); }

            /// <summary>IK 직전 훅(주행) — 명령 속도·가속도의 반경/수직 성분을 함께 넘긴다.</summary>
            private void SagComp_Apply(ref float fX, ref float fY, ref float fZ, double vr, double vz, double ar, double az)
            {
                if (m_SagComp == null || !m_SagComp.Enabled || !m_SagComp.IsFitted)
                { m_afSagLastDelta[0] = m_afSagLastDelta[1] = m_afSagLastDelta[2] = 0; return; }
                m_afSagLastDelta = m_SagComp.Compensate(ref fX, ref fY, ref fZ, vr, vz, ar, az);
            }

            /// <summary>경로 틱이 부른다 — (시각, 보정 전 목표, 반경/수직 속도·가속도) 를 기록. 주행 측정 중에만 켠다.</summary>
            private void SagComp_TraceAdd(int nMs, float x, float y, float z, float vr, float vz, float ar, float az)
            {
                if (!m_bSagTraceOn) return;
                lock (m_lstSagTrace)
                {
                    if (m_lstSagTrace.Count < 20000) m_lstSagTrace.Add(new float[] { nMs, x, y, z, vr, vz, ar, az });
                }
            }
            /// <summary>경로 목표를 시각 nMs 에서 선형 보간 — 주행 표본과 짝짓기. 범위 밖이면 false.</summary>
            private bool SagComp_TraceAt(int nMs, out float x, out float y, out float z, out float vr, out float vz, out float ar, out float az)
            {
                x = y = z = vr = vz = ar = az = 0;
                lock (m_lstSagTrace)
                {
                    int n = m_lstSagTrace.Count;
                    if (n == 0) return false;
                    if (nMs < m_lstSagTrace[0][0] - 5 || nMs > m_lstSagTrace[n - 1][0] + 5) return false;
                    int i = 0; while (i < n - 1 && m_lstSagTrace[i + 1][0] < nMs) i++;
                    float[] a = m_lstSagTrace[i], b = m_lstSagTrace[Math.Min(i + 1, n - 1)];
                    float u = (b[0] > a[0]) ? (nMs - a[0]) / (b[0] - a[0]) : 0; if (u < 0) u = 0; if (u > 1) u = 1;
                    x = a[1] + (b[1] - a[1]) * u; y = a[2] + (b[2] - a[2]) * u; z = a[3] + (b[3] - a[3]) * u;
                    vr = a[4] + (b[4] - a[4]) * u; vz = a[5] + (b[5] - a[5]) * u;
                    ar = a[6] + (b[6] - a[6]) * u; az = a[7] + (b[7] - a[7]) * u;
                    return true;
                }
            }
            // ── 진단 CSV (09-28 오후: ← 주행 오차 원인 분석용) — 측정·검증 표본마다 관절별 이상 명령 / 실측 / 실제로 보낸 보정 /
            //    이동 방향 / 명령 속도와 경로 진행률을 남긴다. 모델 파일과 별개. SagComp_ClearDiag 로 비우고 SagComp_GetDiagCsv 로 꺼낸다.
            private readonly StringBuilder m_sbSagDiag = new StringBuilder();
            public void SagComp_ClearDiag() { lock (m_sbSagDiag) m_sbSagDiag.Length = 0; }
            public string SagComp_GetDiagCsv() { lock (m_sbSagDiag) return m_sbSagDiag.ToString(); }
            /// <summary>직전 출력 틱의 관절 보정을 ids 순서로 (모델 없거나 해당 관절 없으면 0)</summary>
            private float[] SagComp_CorrFor(int[] ids)
            {
                float[] c = new float[ids.Length];
                CSagComp m = m_SagComp; float[] last = m_afSagLastJoint;
                if (m == null || m.JointIDs == null || last == null) return c;
                for (int i = 0; i < ids.Length; i++) { int j = Array.IndexOf(m.JointIDs, ids[i]); if (j >= 0 && j < last.Length) c[i] = last[j]; }
                return c;
            }
            private void SagComp_DiagRow(string mode, float z, int pathMs, int dir, int tRel, double prog,
                                         float tx, float ty, float tz, float ax, float ay, float az,
                                         int[] ids, float[] cmd, float[] act, float[] corr, float[] dirs, float[] vel)
            {
                var ci = CultureInfo.InvariantCulture;
                lock (m_sbSagDiag)
                {
                    if (m_sbSagDiag.Length == 0)
                    {
                        m_sbSagDiag.Append("mode,z,path_ms,dir,t_ms,prog,tx,ty,tz,ax,ay,az,dz");
                        foreach (int id in ids) m_sbSagDiag.AppendFormat(ci, ",q{0}_cmd,q{0}_act,q{0}_corr,q{0}_dir,q{0}_vel", id);
                        m_sbSagDiag.Append("\r\n");
                    }
                    m_sbSagDiag.AppendFormat(ci, "{0},{1:F0},{2},{3},{4},{5:F3},{6:F2},{7:F2},{8:F2},{9:F2},{10:F2},{11:F2},{12:F2}",
                        mode, z, pathMs, dir, tRel, prog, tx, ty, tz, ax, ay, az, az - tz);
                    for (int i = 0; i < ids.Length; i++)
                        m_sbSagDiag.AppendFormat(ci, ",{0:F3},{1:F3},{2:F3},{3:F0},{4:F2}",
                            cmd != null ? cmd[i] : 0, act != null ? act[i] : 0, corr != null ? corr[i] : 0, dirs != null ? dirs[i] : 0, vel != null ? vel[i] : 0);
                    m_sbSagDiag.Append("\r\n");
                }
            }

            /// <summary>진단용 — 직전 주행 측정의 경로 추적(ms,x,y,z,vr,vz,ar,az) CSV</summary>
            public string SagComp_GetTraceCsv()
            {
                var sb = new StringBuilder("ms,x,y,z,vr,vz,ar,az\r\n");
                lock (m_lstSagTrace)
                    foreach (var t in m_lstSagTrace)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F0},{1:F2},{2:F2},{3:F2},{4:F1},{5:F1},{6:F1},{7:F1}\r\n", t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7]);
                return sb.ToString();
            }

            // ────────────────────────────────────────────────────────────
            // 측정(Survey) / 검증(Verify) — 실물 필요. 호출자가 실기 승인 뒤에 부른다.
            // ────────────────────────────────────────────────────────────

            /// <summary>측정 설정. 기본값 = OMX follower (IK 수식 0, 관절 11·12·13, 손목 14, 수직 툴).</summary>
            public class CSagSurveyCfg
            {
                public int FuncNumber = 0;
                public int[] PosIDs = new int[] { 11, 12, 13 };
                public int WristID = 14;
                public float RxDeg = 0, RyDeg = 90, RzDeg = 0;   // 툴 자세 (Ry=90 수직 아래)
                public float[] YawDegs = new float[] { 0 };
                public float RStart = 120, REnd = 280;
                public int RSteps = 5;
                public float[] ZLevels = new float[] { 60 };
                public bool BothDirections = true;
                public int MoveMs = 2500;
                public int SettleMs = 800;
                public int[] PathMs = new int[] { 4000, 2000, 1000 };   // 주행 측정: 같은 직선을 이 시간들로 (왕복)
                public int SampleEveryTicks = 4;                        // 주행 중 엔코더 표본 간격 (틱)
                public float MaxAbsDelta = 40f;
                public string Robot = "OMX follower";
            }

            /// <summary>정지 처짐 표본 수집 + 정지 학습. 진행은 log, 중단은 stop. 실패/중단 시 null.</summary>
            public CSagComp SagComp_Survey(CScene_t scene, CSagSurveyCfg cfg,
                                           CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var sb = new StringBuilder();
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn)
                { report = "실물 미연결 또는 토크 OFF — 측정 불가"; return null; }

                var model = new CSagComp
                {
                    Robot = cfg.Robot, FuncNumber = cfg.FuncNumber, PosIDs = (int[])cfg.PosIDs.Clone(),
                    WristID = cfg.WristID, PitchDeg = cfg.RyDeg, MaxAbsDelta = cfg.MaxAbsDelta
                };
                CSagComp prev = m_SagComp;
                // 바닥 보정표는 이어받는다 — FK 기준 표는 엔코더 밖 오차(14번 브래킷 처짐)라 관절 모델을 다시 학습해도 그대로 유효 (09-29)
                if (model.CopyFloorMapFrom(prev) && log != null)
                    log(string.Format(CultureInfo.InvariantCulture, "바닥 보정표 이어받음: {0}점 r {1:F0}~{2:F0}mm (FK 기준 — 관절 모델과 무관, 끄려면 파일의 flooron=0)",
                        model.FloorR.Length, model.FloorR[0], model.FloorR[model.FloorR.Length - 1]));
                else if (prev != null && prev.HasFloorMap && prev.FloorFromCmd && log != null)
                    log("바닥 보정표(명령 높이 기준)는 관절 모델에 묶여 있어 이어받지 않음 — 다시 재야 한다");
                SagComp_Switch(null);                               // 측정 중엔 보상 끔 — 원시 처짐을 재야 한다 (1초에 걸쳐 빠진다)
                try
                {
                    var pts = new List<float[]>();
                    foreach (float z in cfg.ZLevels)
                        foreach (float yaw in cfg.YawDegs)
                        {
                            var line = new List<float[]>();
                            for (int i = 0; i <= cfg.RSteps; i++)
                            {
                                float r = cfg.RStart + (cfg.REnd - cfg.RStart) * i / Math.Max(1, cfg.RSteps);
                                double a = yaw * Math.PI / 180.0;
                                line.Add(new float[] { (float)(r * Math.Cos(a)), (float)(r * Math.Sin(a)), z, +1 });
                            }
                            pts.AddRange(line);
                            if (cfg.BothDirections)
                                for (int i = line.Count - 2; i >= 0; i--)
                                    pts.Add(new float[] { line[i][0], line[i][1], line[i][2], -1 });
                        }
                    if (log != null) log(string.Format("정지 측정: {0}지점 (r {1:F0}~{2:F0}, z {3}개, 요 {4}개{5})",
                        pts.Count, cfg.RStart, cfg.REnd, cfg.ZLevels.Length, cfg.YawDegs.Length, cfg.BothDirections ? ", 왕복" : ""));
                    if (log != null) log("  idx   명령(x,y,z)                실측(x,y,z)                 Δr     Δz    접근");

                    for (int i = 0; i < pts.Count; i++)
                    {
                        if (stop != null && stop()) { if (log != null) log("중단"); return null; }
                        float x = pts[i][0], y = pts[i][1], z = pts[i][2];
                        CSagSample s; string why;
                        if (!SagComp_MeasureOne(scene, cfg, x, y, z, (int)pts[i][3], out s, out why))
                        { if (log != null) log(string.Format("  [{0}] ({1:F0},{2:F0},{3:F0}) 실패: {4}", i, x, y, z, why)); continue; }
                        model.Samples.Add(s);
                        if (log != null) log(string.Format(CultureInfo.InvariantCulture,
                            "  [{0,2}] ({1,6:F1},{2,6:F1},{3,6:F1}) → ({4,6:F1},{5,6:F1},{6,6:F1})  {7,6:+0.00;-0.00} {8,6:+0.00;-0.00}   {9}",
                            i, s.CmdX, s.CmdY, s.CmdZ, s.ActX, s.ActY, s.ActZ, s.DR, s.DZ, s.Approach > 0 ? "→멀리" : "←가까이"));
                    }
                    string fit = model.Fit();
                    if (log != null) log(fit);
                    sb.AppendLine(fit);
                    string fitJ = SagComp_FitJoint(model, null, false);   // 관절 모델(중력+데드밴드) — 있으면 이쪽이 보상을 맡는다
                    if (log != null) log(fitJ);
                    sb.AppendLine(fitJ);
                    if (model.Samples.Exists(q => q.Excluded))            // 접촉 의심을 뺐으면 직교 모델도 다시
                    { fit = model.Fit(); if (log != null) log("  (접촉 의심 제외 후) " + fit); sb.AppendLine(fit); }

                    if (cfg.BothDirections)
                    {
                        double worst = 0; int nPair = 0;
                        foreach (var a in model.Samples)
                            if (a.Approach > 0)
                                foreach (var b in model.Samples)
                                    if (b.Approach < 0 && Math.Abs(a.CmdX - b.CmdX) < 1e-3 && Math.Abs(a.CmdY - b.CmdY) < 1e-3 && Math.Abs(a.CmdZ - b.CmdZ) < 1e-3)
                                    { worst = Math.Max(worst, Math.Abs(a.DZ - b.DZ)); nPair++; }
                        string hy = string.Format(CultureInfo.InvariantCulture, "접근 방향 차(백래시 지표): 같은 지점 {0}쌍, Δz 최대 {1:F2}mm — {2}",
                            nPair, worst, model.HasJoint ? "관절 모델이 관절별 이동 방향으로 가른다" : "직교 모델은 평균을 쓴다");
                        if (log != null) log(hy);
                        sb.AppendLine(hy);
                    }
                    report = sb.ToString();
                    return model.IsUsable ? model : null;
                }
                finally { SagComp_Switch(prev); }
            }

            /// <summary>주행 표본 수집 + 주행 학습. 같은 직선(첫 요각, 각 z)을 cfg.PathMs 의 시간들로 왕복하며
            /// SampleEveryTicks 틱마다 엔코더를 읽어 그 순간의 명령점·속도·가속도와 짝짓는다.
            /// 정지 모델(model)이 있어야 한다. 표본은 model.Samples 에 Dynamic=true 로 쌓인다.</summary>
            public bool SagComp_SurveyDynamic(CScene_t scene, CSagSurveyCfg cfg, CSagComp model,
                                              CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn || model == null || !model.IsFitted) return false;
                CSagComp prev = m_SagComp;
                SagComp_Switch(null);                               // 원시 주행 오차를 잰다 (1초에 걸쳐 빠진다)
                int[] ids = new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID };
                var sb = new StringBuilder();
                try
                {
                    double a0 = cfg.YawDegs[0] * Math.PI / 180.0;
                    int nBefore = model.Samples.Count;
                    foreach (float z in cfg.ZLevels)
                        foreach (int ms in cfg.PathMs)
                            for (int dir = 0; dir < 2; dir++)
                            {
                                if (stop != null && stop()) { report = "중단"; return false; }
                                float rA = dir == 0 ? cfg.RStart : cfg.REnd, rB = dir == 0 ? cfg.REnd : cfg.RStart;
                                float xA = (float)(rA * Math.Cos(a0)), yA = (float)(rA * Math.Sin(a0));
                                float xB = (float)(rB * Math.Cos(a0)), yB = (float)(rB * Math.Sin(a0));
                                // 시작점에 정지로 세운다
                                if (!PlayXyz(cfg.MoveMs, cfg.SettleMs, cfg.FuncNumber, xA, yA, z, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs))
                                { if (log != null) log("  시작점 이동 실패"); continue; }

                                // 틱 싱크를 감싼다: 전송 뒤 N 틱마다 엔코더 2회 읽기(첫 응답은 스테일).
                                // 그 틱에 보낸 관절 명령(3D 이상 각)과 관절별 이동 방향도 함께 남긴다 — 관절공간 주행 지연 모델용
                                var reads = new List<KeyValuePair<int, float[]>>();
                                var cmds = new List<float[]>(); var dirs = new List<float[]>();
                                var vels = new List<float[]>(); var accs = new List<float[]>();
                                int nTick = 0;
                                Action<int> sink = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);                                          // 안에서 출력 훅이 방향·속도를 갱신
                                    if (++nTick % Math.Max(1, cfg.SampleEveryTicks) != 0) return;
                                    float[] c = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++) c[i] = GetData(ids[i]);
                                    float[] dn = SagComp_JointDirNow(ids);
                                    float[] vn = SagComp_JointVelNow(ids), an = SagComp_JointAccNow(ids);
                                    string w; scene.ReadDegQuick(out w);
                                    int t = MotionTick_ms();
                                    var d = scene.ReadDegQuick(out w);
                                    if (d == null) return;
                                    float[] a = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++) { if (!d.ContainsKey(ids[i])) return; a[i] = d[ids[i]]; }
                                    reads.Add(new KeyValuePair<int, float[]>(t, a)); cmds.Add(c); dirs.Add(dn); vels.Add(vn); accs.Add(an);
                                };
                                lock (m_lstSagTrace) m_lstSagTrace.Clear();
                                m_bSagTraceOn = true;
                                MoveJoints_SetMotionSink(sink);
                                bool bOk;
                                int t0 = MotionTick_ms();
                                try { bOk = PlayXyzPath(ms, cfg.SettleMs, cfg.FuncNumber, new float[] { xB, yB, z }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                                finally { m_bSagTraceOn = false; MoveJoints_SetMotionSink(scene.SendPose); }
                                if (!bOk) { if (log != null) log("  경로 실패"); continue; }

                                // 표본 짝짓기: 읽은 시각의 명령점·v·a 를 추적 버퍼에서 보간, 실측은 FK
                                int nAdded = 0; double se = 0;
                                float[] keep = new float[ids.Length];
                                for (int i = 0; i < ids.Length; i++) keep[i] = GetData(ids[i]);
                                for (int q = 0; q < reads.Count; q++)
                                {
                                    var kv = reads[q];
                                    float tx, ty, tz, vr, vz, ar, az;
                                    if (!SagComp_TraceAt(kv.Key, out tx, out ty, out tz, out vr, out vz, out ar, out az)) continue;
                                    float[] lev = SagComp_JointLevers(cfg.FuncNumber, ids, cmds[q]);
                                    float ax, ay, azz;
                                    for (int i = 0; i < ids.Length; i++) SetData(ids[i], kv.Value[i]);
                                    CalcF(cfg.FuncNumber, -1, false, out ax, out ay, out azz);
                                    var s = new CSagSample
                                    {
                                        CmdX = tx, CmdY = ty, CmdZ = tz, ActX = ax, ActY = ay, ActZ = azz,
                                        Approach = dir == 0 ? +1 : -1, Dynamic = true, Vr = vr, Vz = vz, Ar = ar, Az = az,
                                        JointAct = (float[])kv.Value.Clone(), JointCmd = cmds[q], JointDir = dirs[q], JointLever = lev,
                                        JointVel = vels[q], JointAcc = accs[q]
                                    };
                                    model.Samples.Add(s); nAdded++; se += s.DZ * s.DZ;
                                    SagComp_DiagRow("raw", z, ms, dir == 0 ? +1 : -1, kv.Key - t0, (double)(kv.Key - t0) / ms,
                                        tx, ty, tz, ax, ay, azz, ids, cmds[q], kv.Value, null, dirs[q], vels[q]);
                                }
                                for (int i = 0; i < ids.Length; i++) SetData(ids[i], keep[i]);
                                string row = string.Format(CultureInfo.InvariantCulture,
                                    "  주행 z={0:F0} {1}ms {2}: 표본 {3}개, 원시 Δz RMSE {4:F2}mm (최고속 {5:F0}mm/s)",
                                    z, ms, dir == 0 ? "→멀리" : "←가까이", nAdded, nAdded > 0 ? Math.Sqrt(se / nAdded) : 0,
                                    1.5 * Math.Abs(rB - rA) / (ms / 1000.0));
                                if (log != null) log(row);
                                sb.AppendLine(row);
                            }
                    string fit = model.FitDynamic();
                    if (log != null) log(fit);
                    sb.AppendLine(fit);
                    if (model.HasJoint)
                    {   // 관절 모델이면 주행 잔차를 관절 지연(τ·ω + κ·α)으로 — 출력 단에서 앞서 보내는 데 쓴다
                        string fitL = SagComp_FitJointLag(model);
                        if (model.HasJointLag && !model.LagEnabled) fitL += "\r\n  (지연 보정은 학습만 — 적용은 LagEnabled=true 로 켠다. 09-28 실기에서 순이득이 없어 기본 끔)";
                        if (log != null) log(fitL);
                        sb.AppendLine(fitL);
                    }
                    report = sb.ToString();
                    return model.HasDynamic || model.HasJointLag || model.Samples.Count > nBefore;
                }
                finally { SagComp_Switch(prev); }
            }

            /// <summary>정지 검증 — 모델을 켠 채 같은 지점들을 다시 세워 잔차를 잰다 (PlayXyz 경로). 반환 = 잔차 RMSE.</summary>
            public float SagComp_Verify(CScene_t scene, CSagSurveyCfg cfg, CSagComp model,
                                        CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn || model == null || !model.IsUsable) return -1f;
                CSagComp prev = m_SagComp;
                // 엔코더 기준 평가(FK vs 명령) — 바닥 보정표는 일부러 FK 를 Δz 만큼 옮기므로 이 동안만 끈다 (09-29)
                bool bFloorKeep = model != null && model.FloorMapEnabled; if (model != null) model.FloorMapEnabled = false;
                if (model != null) model.Enabled = true; SagComp_Switch(model);
                try
                {
                    var sb = new StringBuilder();
                    double se = 0, seRaw = 0, se3 = 0; int n = 0;
                    double[] seDir = new double[2]; int[] nDir = new int[2];
                    if (log != null) log(string.Format("정지 검증(보상 ON — {0}, PlayXyz{1}): 모델 예상 처짐 → 보상 후 실측 잔차",
                        model.HasJoint ? "관절 모델, 실물 출력 단" : "직교 모델, IK 목표", cfg.BothDirections ? ", 왕복" : ""));
                    // 측정과 같은 순서로 세운다 — 멀리 갔다가(→) 되돌아온다(←). 데드밴드 처리는 되돌아오는 쪽에서 드러난다
                    var pts = new List<float[]>();
                    foreach (float z in cfg.ZLevels)
                    {
                        for (int i = 0; i <= cfg.RSteps; i++) pts.Add(new float[] { cfg.RStart + (cfg.REnd - cfg.RStart) * i / Math.Max(1, cfg.RSteps), z, +1 });
                        if (cfg.BothDirections)
                            for (int i = cfg.RSteps - 1; i >= 0; i--) pts.Add(new float[] { cfg.RStart + (cfg.REnd - cfg.RStart) * i / Math.Max(1, cfg.RSteps), z, -1 });
                    }
                    foreach (float[] pt in pts)
                        {
                            if (stop != null && stop()) break;
                            float r = pt[0], z = pt[1]; int approach = (int)pt[2];
                            double a = cfg.YawDegs[0] * Math.PI / 180.0;
                            float x = (float)(r * Math.Cos(a)), y = (float)(r * Math.Sin(a));
                            CSagSample s; string why;
                            if (!SagComp_MeasureOne(scene, cfg, x, y, z, approach, out s, out why)) { if (log != null) log("  실패: " + why); continue; }
                            double dR0, dZ0;
                            if (model.HasJoint)
                            {   // 관절 모델이 예상한 (보상 안 했을 때의) 처짐 — 방향은 이번 이동의 추적값
                                float px, py, pz; SagComp_JointPredictTcp(model, s.JointCmd, s.JointDir, s.JointLever, out px, out py, out pz);
                                dR0 = (r > 1e-3f) ? (px * x + py * y) / r : 0; dZ0 = pz;
                            }
                            else model.PredictStatic(x, y, z, out dR0, out dZ0);
                            double e3 = s.DX * s.DX + s.DY * s.DY + s.DZ * s.DZ;
                            se += s.DZ * s.DZ; seRaw += dZ0 * dZ0; se3 += e3; n++;
                            int k = approach > 0 ? 0 : 1; seDir[k] += e3; nDir[k]++;
                            SagComp_DiagRow("static", z, 0, approach, 0, -1, x, y, z, s.ActX, s.ActY, s.ActZ,
                                new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID }, s.JointCmd, s.JointAct,
                                SagComp_CorrFor(new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID }), s.JointDir, null);
                            double dT = -s.DX * Math.Sin(a) + s.DY * Math.Cos(a);   // 옆(접선) 방향 — 베이스 회전 오차
                            string row = string.Format(CultureInfo.InvariantCulture,
                                "  r={0,5:F0} z={1,4:F0} {2}: 예상 처짐 {3,6:+0.00;-0.00} → 잔차 Δz {4,6:+0.00;-0.00}  Δr {5,6:+0.00;-0.00}  Δt {7,6:+0.00;-0.00}  |Δ| {6,5:F2}",
                                r, z, approach > 0 ? "→" : "←", dZ0, s.DZ, s.DR, Math.Sqrt(e3), dT);
                            if (log != null) log(row); sb.AppendLine(row);
                        }
                    float rmse = n > 0 ? (float)Math.Sqrt(se / n) : -1f;
                    string sum = string.Format(CultureInfo.InvariantCulture,
                        "정지 검증 {0}지점: 보상 없을 때 예상 처짐 RMSE {1:F2}mm → 보상 후 실측 잔차 Δz RMSE {2:F2}mm, 3D RMSE {3:F2}mm (→ {4:F2} / ← {5:F2})",
                        n, n > 0 ? Math.Sqrt(seRaw / n) : 0, rmse, n > 0 ? Math.Sqrt(se3 / n) : 0,
                        nDir[0] > 0 ? Math.Sqrt(seDir[0] / nDir[0]) : 0, nDir[1] > 0 ? Math.Sqrt(seDir[1] / nDir[1]) : 0);
                    if (log != null) log(sum); sb.AppendLine(sum);
                    report = sb.ToString();
                    return rmse;
                }
                finally { if (model != null) model.FloorMapEnabled = bFloorKeep; SagComp_Switch(prev); }
            }

            /// <summary>주행 검증 — 모델(정지+주행항)을 켠 채 같은 직선을 같은 속도들로 달리며 실측 잔차를 잰다 (PlayXyzPath 경로).
            /// 반환 = 잔차 Δz RMSE (전 속도 합산).</summary>
            public float SagComp_VerifyDynamic(CScene_t scene, CSagSurveyCfg cfg, CSagComp model,
                                               CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn || model == null || !model.IsUsable) return -1f;
                CSagComp prev = m_SagComp;
                // 엔코더 기준 평가(FK vs 명령) — 바닥 보정표는 일부러 FK 를 Δz 만큼 옮기므로 이 동안만 끈다 (09-29)
                bool bFloorKeep = model != null && model.FloorMapEnabled; if (model != null) model.FloorMapEnabled = false;
                if (model != null) model.Enabled = true; SagComp_Switch(model);
                int[] ids = new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID };
                var sb = new StringBuilder();
                double seAll = 0; int nAll = 0;
                try
                {
                    double a0 = cfg.YawDegs[0] * Math.PI / 180.0;
                    if (log != null) log("주행 검증(보상 ON, PlayXyzPath): 속도별 실측 잔차");
                    foreach (float z in cfg.ZLevels)
                        foreach (int ms in cfg.PathMs)
                            for (int dir = 0; dir < 2; dir++)
                            {
                                if (stop != null && stop()) break;
                                float rA = dir == 0 ? cfg.RStart : cfg.REnd, rB = dir == 0 ? cfg.REnd : cfg.RStart;
                                float xA = (float)(rA * Math.Cos(a0)), yA = (float)(rA * Math.Sin(a0));
                                float xB = (float)(rB * Math.Cos(a0)), yB = (float)(rB * Math.Sin(a0));
                                if (!PlayXyz(cfg.MoveMs, cfg.SettleMs, cfg.FuncNumber, xA, yA, z, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs)) continue;
                                var reads = new List<KeyValuePair<int, float[]>>();
                                var cmds = new List<float[]>(); var corrs = new List<float[]>(); var dirs = new List<float[]>(); var vels = new List<float[]>();
                                int nTick = 0;
                                Action<int> sink = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);                                          // 안에서 출력 훅이 보정·방향·속도를 갱신
                                    if (++nTick % Math.Max(1, cfg.SampleEveryTicks) != 0) return;
                                    float[] c = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++) c[i] = GetData(ids[i]);
                                    float[] cr = SagComp_CorrFor(ids), dn = SagComp_JointDirNow(ids), vn = SagComp_JointVelNow(ids);
                                    string w; scene.ReadDegQuick(out w);
                                    int t = MotionTick_ms();
                                    var d = scene.ReadDegQuick(out w);
                                    if (d == null) return;
                                    float[] a = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++) { if (!d.ContainsKey(ids[i])) return; a[i] = d[ids[i]]; }
                                    reads.Add(new KeyValuePair<int, float[]>(t, a)); cmds.Add(c); corrs.Add(cr); dirs.Add(dn); vels.Add(vn);
                                };
                                lock (m_lstSagTrace) m_lstSagTrace.Clear();
                                m_bSagTraceOn = true;
                                MoveJoints_SetMotionSink(sink);
                                bool bOk;
                                int t0 = MotionTick_ms();
                                try { bOk = PlayXyzPath(ms, cfg.SettleMs, cfg.FuncNumber, new float[] { xB, yB, z }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                                finally { m_bSagTraceOn = false; MoveJoints_SetMotionSink(scene.SendPose); }
                                if (!bOk) continue;
                                float[] keep = new float[ids.Length];
                                for (int i = 0; i < ids.Length; i++) keep[i] = GetData(ids[i]);
                                double se = 0; int n = 0; double worst = 0;
                                for (int q = 0; q < reads.Count; q++)
                                {
                                    var kv = reads[q];
                                    float tx, ty, tz, vr, vz, ar, az;
                                    if (!SagComp_TraceAt(kv.Key, out tx, out ty, out tz, out vr, out vz, out ar, out az)) continue;
                                    float ax, ay, azz;
                                    for (int i = 0; i < ids.Length; i++) SetData(ids[i], kv.Value[i]);
                                    CalcF(cfg.FuncNumber, -1, false, out ax, out ay, out azz);
                                    double dz = azz - tz; se += dz * dz; n++; worst = Math.Max(worst, Math.Abs(dz));
                                    SagComp_DiagRow("verify", z, ms, dir == 0 ? +1 : -1, kv.Key - t0, (double)(kv.Key - t0) / ms,
                                        tx, ty, tz, ax, ay, azz, ids, cmds[q], kv.Value, corrs[q], dirs[q], vels[q]);
                                }
                                for (int i = 0; i < ids.Length; i++) SetData(ids[i], keep[i]);
                                seAll += se; nAll += n;
                                string row = string.Format(CultureInfo.InvariantCulture,
                                    "  z={0:F0} {1}ms {2}: 표본 {3}개, 보상 후 Δz RMSE {4:F2}mm (최대 {5:F2})",
                                    z, ms, dir == 0 ? "→멀리" : "←가까이", n, n > 0 ? Math.Sqrt(se / n) : 0, worst);
                                if (log != null) log(row); sb.AppendLine(row);
                            }
                    float rmse = nAll > 0 ? (float)Math.Sqrt(seAll / nAll) : -1f;
                    string sum = string.Format(CultureInfo.InvariantCulture, "주행 검증 {0}표본: 보상 후 Δz RMSE {1:F2}mm (학습 때 원시 {2:F2} → 모델 잔차 {3:F2})",
                        nAll, rmse, model.DynRmseBefore, model.DynRmseAfter);
                    if (log != null) log(sum); sb.AppendLine(sum);
                    report = sb.ToString();
                    return rmse;
                }
                finally { if (model != null) model.FloorMapEnabled = bFloorKeep; SagComp_Switch(prev); }
            }

            // ── 스트리밍·떨림 진단 (09-28 오후) ──
            // 앞서 보내기가 서보에서 시간 이동으로 실현되지 않고, 주행 중 떨림(부들거림)도 있다 → 서보가 실제로 받은 값을 본다.
            private readonly StringBuilder m_sbSagProbe = new StringBuilder();
            public void SagComp_ClearProbe() { lock (m_sbSagProbe) m_sbSagProbe.Length = 0; }
            public string SagComp_GetProbeCsv() { lock (m_sbSagProbe) return m_sbSagProbe.ToString(); }

            /// <summary>스트리밍·떨림 진단 — 주행 경로(cfg 의 z·PathMs 왕복)를 달리며 매 틱 (이상 명령, 실제로 보낸 명령, horizon) 을,
            /// nReadEvery 틱마다 서보 레지스터(받은 목표·속도 한도·현재 위치·속도·PWM·부하·Moving)를 기록한다.
            /// model = null 이면 보상 없이. 기록은 SagComp_GetProbeCsv. 반환 = 기록한 줄 수.
            /// 단위: 각 deg, 속도 한도·현재 속도 deg/s (0.229 rpm/raw), PWM % (0.113 %/raw), 부하 % (0.1 %/raw).</summary>
            public int SagComp_StreamProbe(CScene_t scene, CSagSurveyCfg cfg, CSagComp model, string label, int nReadEvery,
                                           CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn) return 0;
                CSagComp prev = m_SagComp;
                // 엔코더 기준 평가(FK vs 명령) — 바닥 보정표는 일부러 FK 를 Δz 만큼 옮기므로 이 동안만 끈다 (09-29)
                bool bFloorKeep = model != null && model.FloorMapEnabled; if (model != null) model.FloorMapEnabled = false;
                if (model != null) model.Enabled = true; SagComp_Switch(model);
                int[] ids = scene.IDs;
                var ci = CultureInfo.InvariantCulture;
                int nRows = 0;
                try
                {
                    lock (m_sbSagProbe)
                        if (m_sbSagProbe.Length == 0)
                        {
                            m_sbSagProbe.Append("label,z,path_ms,dir,t_ms,prog,horizon_ms,rt_ms");
                            foreach (int id in ids)
                                m_sbSagProbe.AppendFormat(ci, ",q{0}_ideal,q{0}_sent,q{0}_goal,q{0}_pv,q{0}_pos,q{0}_vel,q{0}_pwm,q{0}_load,q{0}_mov", id);
                            m_sbSagProbe.Append("\r\n");
                        }
                    double a0 = cfg.YawDegs[0] * Math.PI / 180.0;
                    foreach (float z in cfg.ZLevels)
                        foreach (int ms in cfg.PathMs)
                            for (int dir = 0; dir < 2; dir++)
                            {
                                if (stop != null && stop()) return nRows;
                                float rA = dir == 0 ? cfg.RStart : cfg.REnd, rB = dir == 0 ? cfg.REnd : cfg.RStart;
                                if (!PlayXyz(cfg.MoveMs, cfg.SettleMs, cfg.FuncNumber, (float)(rA * Math.Cos(a0)), (float)(rA * Math.Sin(a0)), z,
                                             cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs)) continue;
                                int nTick = 0, t0 = 0, nPath = 0;
                                Action<int> sink = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);
                                    int t = MotionTick_ms();
                                    float[] sent = scene.LastSentDeg;
                                    float hz = scene.StreamHorizonMs;
                                    int[][] blk = null;
                                    if (nReadEvery > 0 && (nTick % nReadEvery) == 0) { string w; blk = scene.ReadServoBlock(8, out w); }
                                    nTick++;
                                    var sb = new StringBuilder();
                                    sb.AppendFormat(ci, "{0},{1:F0},{2},{3},{4},{5:F3},{6:F1},{7}", label, z, ms, dir == 0 ? +1 : -1, t - t0,
                                        (double)(t - t0) / ms, hz, blk != null ? blk[0][2].ToString(ci) : "");
                                    for (int i = 0; i < ids.Length; i++)
                                    {
                                        sb.AppendFormat(ci, ",{0:F3},{1:F3}", GetData(ids[i]), sent != null && i < sent.Length ? sent[i] : 0f);
                                        if (blk != null)
                                        {
                                            int sg = scene.SignOf(i); int[] b = blk[i];
                                            sb.AppendFormat(ci, ",{0:F3},{1:F2},{2:F3},{3:F2},{4:F1},{5:F1},{6}",
                                                CSim2Real.Deg(b[1]) * sg, b[0] * 0.229f * 6f, CSim2Real.Deg(b[7]) * sg, b[6] * 0.229f * 6f * sg,
                                                b[4] * 0.113f, b[5] * 0.1f, b[3]);
                                        }
                                        else sb.Append(",,,,,,,");
                                    }
                                    sb.Append("\r\n");
                                    lock (m_sbSagProbe) m_sbSagProbe.Append(sb.ToString());
                                    nPath++;
                                };
                                MoveJoints_SetMotionSink(sink);
                                t0 = MotionTick_ms();
                                try { PlayXyzPath(ms, cfg.SettleMs, cfg.FuncNumber, new float[] { (float)(rB * Math.Cos(a0)), (float)(rB * Math.Sin(a0)), z },
                                                  cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                                finally { MoveJoints_SetMotionSink(scene.SendPose); }
                                nRows += nPath;
                                if (log != null) log(string.Format(ci, "  [{0}] z={1:F0} {2}ms {3}: {4}틱 기록 (평균 틱 {5:F1}ms)", label, z, ms,
                                    dir == 0 ? "→멀리" : "←가까이", nPath, nPath > 0 ? (double)ms / nPath : 0));
                            }
                    return nRows;
                }
                finally { if (model != null) model.FloorMapEnabled = bFloorKeep; SagComp_Switch(prev); }
            }

            // ────────────────────────────────────────────────────────────
            // 바닥 접촉 캘리브레이션 (09-28, 사용자 제안) — 엔코더로 안 보이는 오차(링크·브래킷 휨, 기구 치수)를 외부 기준면으로 잰다.
            // 수평 바닥에 그리퍼 끝을 위에서 천천히 내려 닿는 순간을 잡는다: 명령은 계속 내려가는데 엔코더 FK 높이가 멈추면
            // (명령 − FK) 가 기준선에서 벌어진다. 바닥 높이는 몰라도 반경마다의 "닿은 명령 높이" 차이가 곧 외부 오차다.
            // ────────────────────────────────────────────────────────────
            /// <summary>바닥 접촉 학습 모델 — 가우시안 프로세스 회귀 (09-29, 사용자: "바닥만으로, 촘촘히 여러 방향에서 재서 인공지능 학습").
            /// 입력 = 접촉 순간 관절각(FeatIDs, deg), 출력 = 그 자세에서 끝이 바닥에 닿을 때의 FK 높이(mm).
            /// 예측 − Ref = 엔코더 밖 오차 → 출력 단계에서 끝을 그만큼 올린다(바닥 보정표와 같은 관절 12·13·14 변환).
            /// 데이터에서 먼 자세는 사후 평균이 사전 평균(Ref)으로 돌아가 보정이 저절로 0 이 되고, 표준편차가 MaxStd 를 넘으면 더 줄인다
            /// — 잰 적 없는 자세(예: 바닥보다 높은 곳)에서 넘겨짚어 과보정하지 않는다. 커널 = 관절별 길이 척도의 RBF.
            /// 모델 파일에는 학습점과 초매개변수만 저장하고 불러올 때 촐레스키를 다시 푼다(N 이 수백이면 수 ms).</summary>
            public class CSagGp
            {
                public int[] FeatIDs = { 11, 12, 13, 14 };
                public double[] Ell = { 20, 8, 8, 8 };          // 관절별 길이 척도 (deg)
                public double SigF = 10, SigN = 1;              // 신호 크기·잡음 (mm)
                public double Ref = 0;                          // 사전 평균 = 기준 FK 높이 (mm)
                public double MaxStd = 3;                       // 예측 표준편차가 이보다 크면 보정을 줄이기 시작 (2배에서 0)
                public bool Enabled = true;
                public double[][] X; public double[] Y;         // 학습점 (관절각, FK 높이)
                // ── 가드 (09-29) — GP 가 스스로 확신해도 믿지 않는 두 경우 ──
                // ① 두 상태 반경 띠: 같은 바닥점(x·y·피치)을 다른 측정에서 쟀더니 FK 바닥이 크게 다름 = 14번 브래킷이 이력에 따라 위/아래로
                //    넘어가는 곳(r≈185~210: 09-28 은 185, 09-29 는 205 에서 넘어감, 끝 위치 ~30mm 차). 한쪽 상태로 확신하면 30mm 틀린다 → 띠 안은 어느 높이든 보정 0.
                public double[] BadR = new double[0];
                public double BadBandMm = 12;
                // ② 데이터 거리: 가장 가까운 학습 자세와의 팔 관절 최대 차가 DomainDeg 이내면 그대로, 2배에서 0.
                //    바닥 접촉 데이터는 공구·(x,y) 마다 자세 하나(1차원 띠)라 다른 높이는 잰 적이 없다 — 요 묶음 교차검증은 높이 넘겨짚기를
                //    시험하지 못하므로 길이 척도가 길게 뽑혀도(20°) 바닥에서 40mm 위까지 확신하는 일이 생긴다(09-29 확인).
                public double DomainDeg = 3.0;
                public string CvReport = "";
                public bool InBadBand(double r)
                {
                    if (BadR == null) return false;
                    foreach (double b in BadR) if (Math.Abs(r - b) <= BadBandMm) return true;
                    return false;
                }
                /// <summary>가장 가까운 학습 자세까지의 거리 (deg, 베이스 요 제외 팔 관절 최대 차)</summary>
                public double DataDistDeg(double[] x)
                {
                    double best = double.MaxValue;
                    if (X == null) return best;
                    foreach (double[] t in X)
                    {
                        double mx = 0; for (int d = 1; d < x.Length && d < t.Length; d++) mx = Math.Max(mx, Math.Abs(x[d] - t[d]));
                        if (mx < best) best = mx;
                    }
                    return best;
                }
                private double[] m_alpha; private double[,] m_L;
                public bool IsReady { get { return m_alpha != null && X != null && X.Length > 0; } }
                public int Count { get { return X == null ? 0 : X.Length; } }

                private double K(double[] a, double[] b)
                {
                    double s = 0;
                    for (int d = 0; d < a.Length; d++) { double t = (a[d] - b[d]) / Ell[d]; s += t * t; }
                    return SigF * SigF * Math.Exp(-0.5 * s);
                }
                /// <summary>학습(촐레스키). 실패하면 false.</summary>
                public bool Solve()
                {
                    m_alpha = null; m_L = null;
                    if (X == null || Y == null || X.Length != Y.Length || X.Length == 0) return false;
                    int n = X.Length;
                    var L = new double[n, n];
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j <= i; j++)
                        {
                            double sum = K(X[i], X[j]) + (i == j ? SigN * SigN : 0);
                            for (int k = 0; k < j; k++) sum -= L[i, k] * L[j, k];
                            if (i == j) { if (sum <= 1e-12) return false; L[i, i] = Math.Sqrt(sum); }
                            else L[i, j] = sum / L[j, j];
                        }
                    var z = new double[n];
                    for (int i = 0; i < n; i++) { double sum = Y[i] - Ref; for (int k = 0; k < i; k++) sum -= L[i, k] * z[k]; z[i] = sum / L[i, i]; }
                    var al = new double[n];
                    for (int i = n - 1; i >= 0; i--) { double sum = z[i]; for (int k = i + 1; k < n; k++) sum -= L[k, i] * al[k]; al[i] = sum / L[i, i]; }
                    m_L = L; m_alpha = al;
                    return true;
                }
                /// <summary>예측: 평균(mm), 표준편차(mm). 준비 안 됐으면 Ref, SigF.</summary>
                public void Predict(double[] x, out double mean, out double std)
                {
                    mean = Ref; std = SigF;
                    if (!IsReady) return;
                    int n = X.Length;
                    var ks = new double[n];
                    double m = Ref;
                    for (int i = 0; i < n; i++) { ks[i] = K(x, X[i]); m += ks[i] * m_alpha[i]; }
                    var v = new double[n]; double vv = 0;
                    for (int i = 0; i < n; i++) { double sum = ks[i]; for (int k = 0; k < i; k++) sum -= m_L[i, k] * v[k]; v[i] = sum / m_L[i, i]; vv += v[i] * v[i]; }
                    mean = m; std = Math.Sqrt(Math.Max(0, SigF * SigF - vv));
                }
                /// <summary>끝을 올릴 양(mm) = (예측 − Ref) × 확신 가중. 확신 = 1 (std ≤ MaxStd) → 0 (std ≥ 2·MaxStd).</summary>
                public double Correction(double[] x, out double std) { return Correction(x, double.NaN, out std); }
                /// <summary>r = 이상 자세의 TCP 수평 반경 (두 상태 띠 검사, NaN 이면 생략)</summary>
                public double Correction(double[] x, double r, out double std)
                {
                    std = SigF;
                    if (!double.IsNaN(r) && InBadBand(r)) return 0;
                    double dd = DataDistDeg(x);
                    double wd = dd <= DomainDeg ? 1.0 : Math.Max(0.0, 2.0 - dd / DomainDeg);
                    if (wd <= 0) return 0;
                    double mean; Predict(x, out mean, out std);
                    double w = std <= MaxStd ? 1.0 : Math.Max(0.0, 2.0 - std / MaxStd);
                    return (mean - Ref) * w * wd;
                }
                /// <summary>묶음 하나씩 통째로 빼고 맞히는 교차검증 RMSE(mm) — 넘겨짚는 능력. groups[i] = 표본 i 의 묶음 이름.</summary>
                public double GroupCvRmse(string[] groups)
                {
                    if (X == null || groups == null) return double.NaN;
                    var names = new List<string>(); foreach (string g in groups) if (!names.Contains(g)) names.Add(g);
                    if (names.Count < 2) return double.NaN;
                    double se = 0; int cnt = 0;
                    foreach (string g in names)
                    {
                        var tx = new List<double[]>(); var ty = new List<double>(); var vx = new List<double[]>(); var vy = new List<double>();
                        for (int i = 0; i < X.Length; i++) if (groups[i] == g) { vx.Add(X[i]); vy.Add(Y[i]); } else { tx.Add(X[i]); ty.Add(Y[i]); }
                        if (tx.Count < 3) continue;
                        var gp = new CSagGp { FeatIDs = FeatIDs, Ell = Ell, SigF = SigF, SigN = SigN, Ref = Ref, X = tx.ToArray(), Y = ty.ToArray() };
                        if (!gp.Solve()) return double.NaN;
                        for (int i = 0; i < vx.Count; i++) { double m, sd; gp.Predict(vx[i], out m, out sd); se += (m - vy[i]) * (m - vy[i]); cnt++; }
                    }
                    return cnt > 0 ? Math.Sqrt(se / cnt) : double.NaN;
                }
                /// <summary>초매개변수 격자 탐색 — 묶음 교차검증 RMSE 가 가장 작은 조합. Ref = 학습 FK 높이 평균, SigF = 표준편차 기준.
                /// 반환 = 성공. CvReport 에 요약(보정 없음 = 평균만 쓴 경우와 비교).</summary>
                public bool FitAuto(double[][] x, double[] y, string[] groups)
                {
                    var ci = CultureInfo.InvariantCulture;
                    X = x; Y = y;
                    if (x == null || y == null || x.Length < 5) { CvReport = "표본이 5개 미만"; return false; }
                    double mean = 0; foreach (double v in y) mean += v; mean /= y.Length;
                    double sd = 0; foreach (double v in y) sd += (v - mean) * (v - mean); sd = Math.Sqrt(sd / y.Length);
                    Ref = mean;
                    double se0 = 0; foreach (double v in y) se0 += (v - mean) * (v - mean);
                    double rmse0 = Math.Sqrt(se0 / y.Length);                               // 보정 없음(= 모든 자세를 평균으로)
                    double best = double.MaxValue; double[] bEll = null; double bF = 0, bN = 0;
                    double[] yawL = { 15, 40, 1000 }, armL = { 4, 6, 9, 13, 20 }, fs = { 0.7, 1.5 }, ns = { 0.7, 1.2, 2.0 };
                    foreach (double ly in yawL) foreach (double la in armL) foreach (double f in fs) foreach (double nz in ns)
                    {
                        Ell = new double[FeatIDs.Length];
                        for (int d = 0; d < Ell.Length; d++) Ell[d] = d == 0 ? ly : la;         // 0 번 = 베이스(요), 나머지 = 팔
                        SigF = Math.Max(1, f * sd); SigN = nz;
                        double cv = GroupCvRmse(groups);
                        if (!double.IsNaN(cv) && cv < best) { best = cv; bEll = (double[])Ell.Clone(); bF = SigF; bN = SigN; }
                    }
                    if (bEll == null) { CvReport = "초매개변수를 못 찾음"; return false; }
                    Ell = bEll; SigF = bF; SigN = bN;
                    bool ok = Solve();
                    CvReport = string.Format(ci, "GP 학습: 표본 {0}, 묶음 교차검증 RMSE {1:F2}mm (보정 없음 {2:F2}mm), 길이 척도 요 {3:F0}° 팔 {4:F0}°, 신호 {5:F1} 잡음 {6:F1}mm, 기준 {7:F2}mm",
                        x.Length, best, rmse0, Ell[0], Ell.Length > 1 ? Ell[1] : 0, SigF, SigN, Ref);
                    return ok;
                }
            }

            /// <summary>바닥 접촉 데이터셋 한 줄씩 (누적 파일용) — 머리줄: session,group,x,y,pitch,r,fkz,cmdz,a1,a2,a3,a4,q1,q2,q3,q4.
            /// 접촉 자세(JointAct)가 있는 유효 접촉만.</summary>
            public static string SagComp_FloorDatasetRows(List<CFloorContact> contacts, string session, bool bHeader)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                if (bHeader) sb.Append("session,group,x,y,pitch,r,fkz,cmdz,a1,a2,a3,a4,q1,q2,q3,q4\r\n");
                if (contacts == null) return sb.ToString();
                foreach (var c in contacts)
                {
                    if (!c.Detected || c.JointAct == null || c.JointAct.Length < 4) continue;
                    double yaw = Math.Atan2(c.Y, c.X) * 180 / Math.PI;
                    string grp = string.Format(ci, "p{0:F0}y{1:F0}", c.Pitch, Math.Round(yaw / 5) * 5);
                    sb.AppendFormat(ci, "{0},{1},{2:F2},{3:F2},{4:F1},{5:F1},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F3},{12},{13},{14},{15}\r\n",
                        session, grp, c.X, c.Y, c.Pitch, c.R, c.FkZ, c.CmdZ, c.JointAct[0], c.JointAct[1], c.JointAct[2], c.JointAct[3],
                        c.JointCmd != null ? c.JointCmd[0].ToString("F3", ci) : "", c.JointCmd != null ? c.JointCmd[1].ToString("F3", ci) : "",
                        c.JointCmd != null ? c.JointCmd[2].ToString("F3", ci) : "", c.JointCmd != null ? c.JointCmd[3].ToString("F3", ci) : "");
                }
                return sb.ToString();
            }

            /// <summary>누적 데이터셋 CSV 로 GP 를 학습해 model.Gp 에 넣는다. 묶음 = group 칸(피치·요). 반환 = 성공, report = 교차검증 요약.</summary>
            public static bool SagComp_FitFloorGp(string datasetCsv, CSagComp model, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (model == null || string.IsNullOrEmpty(datasetCsv)) { report = "모델 또는 데이터 없음"; return false; }
                string[] lines = datasetCsv.Replace("\r", "").Split('\n');
                string[] hd = lines[0].Split(',');
                int iG = Array.IndexOf(hd, "group"), iF = Array.IndexOf(hd, "fkz");
                // 입력 = 접촉 순간의 **이상(명령 전) 관절각** q — 출력 훅이 이상 자세로 예측하므로 학습도 같은 좌표로 (09-29: 실측각 a 로 학습하면
                //   관절 모델 보정·데드밴드만큼(가까운 쪽 수 °) 어긋나 학습한 자세에서도 불확실도 5~9mm → 보정 0). q 가 비었으면 실측각으로.
                int[] iA = { Array.IndexOf(hd, "q1"), Array.IndexOf(hd, "q2"), Array.IndexOf(hd, "q3"), Array.IndexOf(hd, "q4") };
                if (Array.IndexOf(iA, -1) >= 0 || lines.Length < 2 || lines[1].Split(',').Length <= iA[3] || lines[1].Split(',')[iA[3]].Trim().Length == 0)
                    iA = new int[] { Array.IndexOf(hd, "a1"), Array.IndexOf(hd, "a2"), Array.IndexOf(hd, "a3"), Array.IndexOf(hd, "a4") };
                if (iG < 0 || iF < 0 || Array.IndexOf(iA, -1) >= 0) { report = "데이터셋 머리줄이 다름"; return false; }
                var xs = new List<double[]>(); var ys = new List<double>(); var gs = new List<string>();
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] f = lines[i].Split(',');
                    if (f.Length < hd.Length || f[0] == "session") continue;
                    xs.Add(new double[] { double.Parse(f[iA[0]], ci), double.Parse(f[iA[1]], ci), double.Parse(f[iA[2]], ci), double.Parse(f[iA[3]], ci) });
                    ys.Add(double.Parse(f[iF], ci)); gs.Add(f[iG]);
                }
                // 두 상태 검출: 서로 다른 측정(session)에서 같은 바닥점(x·y 3mm, 피치 1° 안)의 FK 바닥이 ConflictMm 넘게 다르면 그 반경이 두 상태
                const double ConflictMm = 6.0;
                int iS = Array.IndexOf(hd, "session"), iX = Array.IndexOf(hd, "x"), iY = Array.IndexOf(hd, "y"), iPt = Array.IndexOf(hd, "pitch");
                var ss = new List<string>(); var px = new List<double>(); var py = new List<double>(); var pp = new List<double>();
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] f = lines[i].Split(',');
                    if (f.Length < hd.Length || f[0] == "session") continue;
                    ss.Add(iS >= 0 ? f[iS] : ""); px.Add(double.Parse(f[iX], ci)); py.Add(double.Parse(f[iY], ci)); pp.Add(double.Parse(f[iPt], ci));
                }
                var gp = new CSagGp();
                var badR = new List<double>();
                for (int i = 0; i < xs.Count; i++)
                    for (int j = i + 1; j < xs.Count; j++)
                        if (ss[i] != ss[j] && Math.Abs(px[i] - px[j]) <= 3 && Math.Abs(py[i] - py[j]) <= 3 && Math.Abs(pp[i] - pp[j]) <= 1 && Math.Abs(ys[i] - ys[j]) > ConflictMm)
                        {
                            double r = Math.Sqrt(px[i] * px[i] + py[i] * py[i]);
                            bool dup = false; foreach (double b in badR) if (Math.Abs(b - r) < 3) dup = true;
                            if (!dup) badR.Add(r);
                        }
                gp.BadR = badR.ToArray();
                var tx = new List<double[]>(); var ty = new List<double>(); var tg = new List<string>(); int nEx = 0;
                for (int i = 0; i < xs.Count; i++)
                {
                    if (gp.InBadBand(Math.Sqrt(px[i] * px[i] + py[i] * py[i]))) { nEx++; continue; }
                    tx.Add(xs[i]); ty.Add(ys[i]); tg.Add(gs[i]);
                }
                if (model.JointIDs != null && model.JointIDs.Length == 4) gp.FeatIDs = (int[])model.JointIDs.Clone();
                bool ok = gp.FitAuto(tx.ToArray(), ty.ToArray(), tg.ToArray());
                gp.BadR = badR.ToArray();
                report = gp.CvReport + (badR.Count > 0
                    ? string.Format(ci, ", 두 상태 반경 {0} (측정마다 {1:F0}mm 넘게 엇갈림) ±{2:F0}mm 띠 {3}점 제외 — 띠 안은 보정 0",
                        string.Join("/", badR.ConvertAll(v => v.ToString("F0", ci)).ToArray()), ConflictMm, gp.BadBandMm, nEx)
                    : ", 두 상태 없음") + string.Format(ci, ", 데이터 거리 {0:F0}° 넘으면 줄임({1:F0}° 에서 0)", gp.DomainDeg, 2 * gp.DomainDeg);
                if (ok) model.Gp = gp;
                return ok;
            }

            public class CFloorProbeCfg
            {
                public int FuncNumber = 0;
                public int[] PosIDs = new int[] { 11, 12, 13 };
                public int WristID = 14;
                public float RxDeg = 0, RyDeg = 90, RzDeg = 0;
                public float[] Radii = new float[] { 120, 155, 190, 225, 260 };
                public float YawDeg = 0;
                public int Repeats = 2;
                public float ZStart = 70;              // 첫 접촉을 찾기 시작하는 명령 높이 (mm)
                public float ZMin = 36;                // 첫 접촉의 하강 한도 (명령) — 바닥은 엔코더 기준 ~46mm 부근(09-28 z=60 측정)
                public float Margin = 10;              // 이후: 시작 = 직전 접촉 FK + Margin, 한도 = 직전 접촉 FK − Margin
                public float DescentMmPerSec = 3f;     // 하강 속도
                public float DetectMm = 0.6f;          // 무장 뒤 (명령 − FK) 가 기준선보다 이만큼 작아지면(2틱 연속) 접촉 후보
                public float SafetyMm = 4f;            // 무장과 무관하게 "명령 하강 거리 − FK 하강 거리" 가 이만큼이면 즉시 정지
                public float ArmTravelMm = 1.5f;       // FK 가 이만큼 따라 내려온 뒤에만 무장 (출발 걸림을 접촉으로 오인하지 않게)
                public int HoldMs = 400;               // 후보가 나오면 명령을 멈추고 이만큼 기다려 확인
                public int SkipTicks = 3;              // 하강 시작 직후 무장 판단에서 뺄 틱
                public int MoveMs = 2500;
                // ── 누르고 들어 올리기(원점 센서 방식, SagComp_FloorPress) ──
                public int StaleTicks = 2;                 // 스트리밍 직후 첫 서보 응답은 스테일(09-28 궤적: FK 5~15mm 튄 값) — 버린다
                public float PressDescentMmPerSec = 4f;    // 누르러 내려가는 속도
                public float PressLagMm = 2.5f;            // 무장 뒤 "명령 하강 − FK 하강" 이 이만큼 (공중 요동 −0.5~+0.9mm 의 3배)
                public float PressLoadPct = 5f;            //   그리고 어깨 또는 팔꿈치 부하가 이만큼 변하면 누른 것 (공중 요동 ±1~2%)
                // 위 조기 누름 규칙 사용 여부 — 09-28 보정표 측정: 두 번째 하강 출발 직후 부하 흐름(8~9%)으로 공중 오판 5회,
                //   물렁한 접촉은 반대로 문턱을 못 넘음. 꺾임 맞춤이 믿을 만하므로 기본은 "벌어짐 PressSafetyMm = 누름 끝" 만 쓴다.
                public bool PressUseLoadRule = false;
                public float PressSafetyMm = 6f;           // 무장 전 벌어짐 한도의 기준 (무장 전 한도 = 이 값 + PreArmExtraMm)
                // 무장 뒤 누름 끝 — 기본 끔(0): 한도(ZMin) 또는 부하 PressSafetyLoadPct 까지 눌러 궤적 전체로 꺾임을 맞춘다.
                //   09-29 궤적 46개 재현: 벌어짐 4.5mm 끝은 공중 끊김(정지 마찰로 멈췄다 튐)으로 4회 오판(r=210~240) → 얕게 멈추면 꺾임도 틀어진다.
                public float PressEndLagMm = 0f;
                public float PreArmExtraMm = 4f;           // 무장 전 허용 추가분: 출발 걸림(데드밴드)이 벌어짐 4~6mm 를 만든다(09-28 3차 r=225·190) → 무장 전 한도 = PressSafetyMm + 이 값
                public float PressSafetyLoadPct = 25f;     // 부하가 이만큼 변하면 즉시 정지(비정상 — 안전 정지로 표시)
                public int PressHoldMs = 300;              // 누른 채 유지
                public float FitWindowMm = 14f;            // 꺾임 맞춤은 누름 끝 위 이 구간만 — 위쪽 출발 걸림·따라잡기가 맞춤을 흐린다
                public float KneeMinGainMm = 2f;           // 꺾임 뒤 벌어짐 증가량이 이만큼은 돼야 진짜 누름
                public float LiftMm = 12f;                 // 들어 올리는 거리 (누름 깊이 + 방향 전환 데드밴드보다 넉넉히)
                // ★먼 쪽(r≥190) 처짐 유격 — 바닥을 누르면 유격이 위로 밀렸다가 들 때 한 번에 ~30mm 떨어져 바닥을 친다(09-28 검증, 사용자 목격 3회).
                //   ⇒ 누르는 동안 꺾임을 실시간으로 맞춰 꺾임점보다 이만큼 내려가면 멈추고(유격을 끝까지 밀지 않게), 복귀는 꺾임점 위 RetractAboveKneeMm 까지 천천히.
                public float PressPastKneeMm = 0f;         // 기본 끔 — 09-29 재현: r=250 에서 공중 걸림을 꺾임으로 오판(47.2 vs 실제 35.9). 켤 땐 5 정도
                public int OnlineKneeEveryTicks = 5;
                public float RetractAboveKneeMm = 40f;
                public int RetractMs = 2500;
                public float LiftMmPerSec = 2f;            // 들어 올리는 속도
            }
            public class CFloorContact
            {
                public float R, X, Y;
                public float CmdZ;                     // 끝이 바닥에 닿은 순간의 명령(이상) 높이 = FK + 기준선 (mm)
                public float FkZ;                      // 그 순간 엔코더 FK 높이 (mm)
                public float BaseErr;                  // 하강 중 (명령 − FK) 기준선 (mm) — 관절 모델 잔차
                public bool Detected, Safety;
                public int Ticks;
                public float Load12Before, Load12At;   // 어깨 부하(%) — 하강 중 평균 / 접촉 순간
                public float[] JointCmd, JointAct;     // 접촉 순간 이상 관절각 / 실측 관절각 (PosIDs + Wrist)
                public string Note = "";
                // 누르고 들어 올리기(SagComp_FloorPress) 결과
                public bool FitOk;                     // 하강 꺾임 맞춤이 물리적으로 말이 되는가 (공중 기울기 0.5~1.5, 누름 기울기 |·|<0.5)
                public float SlopeFree, SlopePress, FitRms;   // 하강 꺾임 맞춤의 공중/누름 기울기, 잔차 (mm)
                public float KneeCmdLift = float.NaN, FkLift = float.NaN;   // 들어 올릴 때의 꺾임 (정지 마찰로 늦게 떨어진다)
                public float LagAtPress, LoadDeltaAtPress;    // 누름 판정 순간의 벌어짐(mm)·부하 변화(%)
                public bool FromPress;                 // SagComp_FloorPress 결과 — FitFlex 가 FkZ(꺾임점) 로 κ 를 절대값으로 맞춘다
                public float Pitch = 90;               // 공구 피치(deg) — 궤적 재분석 때 채운다
            }

            // 하강 궤적 진단 — 매 틱 (명령·FK 높이, 차이, 무장, 어깨·팔꿈치 부하·PWM). SagComp_GetFloorTraceCsv 로 꺼낸다.
            private readonly StringBuilder m_sbSagFloor = new StringBuilder();
            public void SagComp_ClearFloorTrace() { lock (m_sbSagFloor) m_sbSagFloor.Length = 0; }
            public string SagComp_GetFloorTraceCsv() { lock (m_sbSagFloor) return m_sbSagFloor.ToString(); }
            private void SagComp_FloorTraceRow(float r, int seg, int tick, bool armed, float cz, float fz, double e, double d, double lag,
                                               float load12, float pwm12, float load13, float pwm13)
            { SagComp_FloorTraceRow(r, seg, tick, armed, cz, fz, e, d, lag, load12, pwm12, load13, pwm13, float.NaN, float.NaN, float.NaN, null, null); }
            // 09-29: x·y·피치와 매 틱 이상(q)·실측(a) 관절각(PosIDs + Wrist 순)까지 — 궤적 재분석이 접촉 순간 자세를 그대로 얻는다 (GP 학습 입력)
            private void SagComp_FloorTraceRow(float r, int seg, int tick, bool armed, float cz, float fz, double e, double d, double lag,
                                               float load12, float pwm12, float load13, float pwm13, float x, float y, float pitch, float[] q, float[] a)
            {
                var ci = CultureInfo.InvariantCulture;
                Func<float[], int, string> J = (v, i) => (v != null && i < v.Length) ? v[i].ToString("F3", ci) : "";
                lock (m_sbSagFloor)
                {
                    if (m_sbSagFloor.Length == 0) m_sbSagFloor.Append("r,seg,tick,armed,cz,fz,e,d,lag,load12,pwm12,load13,pwm13,x,y,pitch,q1,q2,q3,q4,a1,a2,a3,a4\r\n");
                    m_sbSagFloor.AppendFormat(ci, "{0:F0},{1},{2},{3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F1},{10:F1},{11:F1},{12:F1},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}\r\n",
                        r, seg, tick, armed ? 1 : 0, cz, fz, e, d, lag, load12, pwm12, load13, pwm13,
                        float.IsNaN(x) ? "" : x.ToString("F2", ci), float.IsNaN(y) ? "" : y.ToString("F2", ci), float.IsNaN(pitch) ? "" : pitch.ToString("F1", ci),
                        J(q, 0), J(q, 1), J(q, 2), J(q, 3), J(a, 0), J(a, 1), J(a, 2), J(a, 3));
                }
            }

            /// <summary>바닥 접촉 캘리브레이션 측정 — 반경마다 위에서 내려와 자리 잡고(방향 전환 과도 제거), 천천히 수직 하강하며
            /// 매 틱 서보 위치를 읽어 접촉을 감지하면 즉시 멈추고 기록한 뒤 들어 올린다. model = 보상 모델(보통 활성 관절 모델).
            /// 안전: 한도 높이, 3mm 강제 정지, 반경마다 직전 접촉 기준으로 시작·한도. 실기 승인 뒤에 부를 것.</summary>
            public List<CFloorContact> SagComp_FloorProbe(CScene_t scene, CFloorProbeCfg cfg, CSagComp model,
                                                         CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var res = new List<CFloorContact>();
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn) { report = "실물 미연결 또는 토크 OFF"; return res; }
                CSagComp prev = m_SagComp;
                if (model != null) model.Enabled = true; SagComp_Switch(model);
                int[] ids = new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID };
                int[] sIds = scene.IDs;
                int[] map = new int[ids.Length];
                for (int i = 0; i < ids.Length; i++) map[i] = Array.IndexOf(sIds, ids[i]);
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                double a0 = cfg.YawDeg * Math.PI / 180.0;
                float lastFk = float.NaN;
                try
                {
                    if (Array.IndexOf(map, -1) >= 0) { report = "장면에 없는 관절 ID"; return res; }
                    if (log != null) log(string.Format(ci, "바닥 접촉 캘리브레이션: 반경 {0}, 반경마다 {1}회, 하강 {2:F1} mm/s, 감지 {3:F1} mm",
                        string.Join("/", Array.ConvertAll(cfg.Radii, v => v.ToString("F0", ci))), cfg.Repeats, cfg.DescentMmPerSec, cfg.DetectMm));
                    foreach (float r in cfg.Radii)
                        for (int rep = 0; rep < cfg.Repeats; rep++)
                        {
                            if (stop != null && stop()) return res;
                            float x = (float)(r * Math.Cos(a0)), y = (float)(r * Math.Sin(a0));
                            float zS = float.IsNaN(lastFk) ? cfg.ZStart : Math.Min(cfg.ZStart, lastFk + cfg.Margin);
                            float zMin = float.IsNaN(lastFk) ? cfg.ZMin : Math.Max(cfg.ZMin, lastFk - cfg.Margin);
                            // 위에서 내려오는 방향으로 자리 잡기 — 하강 시작 때 관절이 방향을 바꾸면 데드밴드 과도가 접촉처럼 보인다
                            if (!PlayXyz(cfg.MoveMs, 300, cfg.FuncNumber, x, y, zS + 8, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs) ||
                                !PlayXyz(1500, 500, cfg.FuncNumber, x, y, zS, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs))
                            { if (log != null) log(string.Format(ci, "  r={0:F0}: 시작점 이동 실패 — 건너뜀", r)); continue; }

                            var c = new CFloorContact { R = r, X = x, Y = y };
                            int nLoad = 0, nTickAll = 0, nSeg = 0; double loadSum = 0, baseKeep = double.NaN;
                            float zCur = zS;
                            // ★1차 실기(16:51) 교훈: 멈춘 상태에서 하강을 시작하면 서보가 100~400ms 걸려(정지 마찰) 명령만 먼저 내려가고,
                            //   그게 접촉과 똑같이 보여 15회 모두 바닥 위 16~30mm 에서 오감지. ⇒ ① FK 가 실제로 ArmTravelMm 따라 내려온 뒤에만
                            //   무장 ② 후보가 나오면 명령을 멈추고 HoldMs 기다려 확인(걸림이면 서보가 따라와 차이가 준다 → 이어서 하강)
                            //   ③ 무장과 무관하게 "명령 하강 거리 − FK 하강 거리" 가 SafetyMm 을 넘으면 즉시 정지.
                            while (!c.Detected && nSeg < 12)
                            {
                                if (stop != null && stop()) break;
                                nSeg++;
                                int nTick = 0, nOver = 0; bool bCand = false, bSafety = false, bArmed = false;
                                double baseE = double.NaN; float czStart = float.NaN, fzStart = float.NaN; double lagAtCand = 0, gapAtCand = 0;
                                int seg = nSeg; float rr = r;
                                Action<int> sink = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);
                                    if (bCand) return;
                                    string w; int[][] b = scene.ReadServoBlock(6, out w);
                                    if (b == null) return;
                                    float cx, cy, cz, fx, fy, fz;
                                    CalcF(cfg.FuncNumber, -1, false, out cx, out cy, out cz);                  // 이상(명령) TCP
                                    float[] keep = new float[ids.Length], act = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++)
                                    {
                                        keep[i] = GetData(ids[i]);
                                        act[i] = CSim2Real.Deg(b[map[i]][7]) * scene.SignOf(map[i]);
                                        SetData(ids[i], act[i]);
                                    }
                                    CalcF(cfg.FuncNumber, -1, false, out fx, out fy, out fz);                  // 엔코더 FK TCP
                                    for (int i = 0; i < ids.Length; i++) SetData(ids[i], keep[i]);
                                    float load12 = b[map[1]][5] * 0.1f, pwm12 = b[map[1]][4] * 0.113f;
                                    float load13 = b[map[2]][5] * 0.1f, pwm13 = b[map[2]][4] * 0.113f;
                                    nTick++; nTickAll++;
                                    if (float.IsNaN(czStart)) { czStart = cz; fzStart = fz; }
                                    double lagAbs = (czStart - cz) - (fzStart - fz);                        // 명령이 내려간 거리 − FK 가 내려간 거리
                                    double e = cz - fz;
                                    double d = double.IsNaN(baseE) ? 0 : baseE - e;                         // 무장 뒤: 기준선 대비 벌어짐
                                    SagComp_FloorTraceRow(rr, seg, nTick, bArmed, cz, fz, e, d, lagAbs, load12, pwm12, load13, pwm13);
                                    bool bStop = false;
                                    if (lagAbs > cfg.SafetyMm) { bSafety = true; bStop = true; }
                                    else if (!bArmed)
                                    {
                                        if (nTick > cfg.SkipTicks && (fzStart - fz) >= cfg.ArmTravelMm) { bArmed = true; baseE = e; }
                                    }
                                    else
                                    {
                                        if (d > cfg.DetectMm) { if (++nOver >= 2) bStop = true; } else nOver = 0;
                                        if (!bStop && d < cfg.DetectMm * 0.5) { baseE = 0.85 * baseE + 0.15 * e; loadSum += load12; nLoad++; }
                                    }
                                    if (bStop)
                                    {
                                        bCand = true; lagAtCand = lagAbs; gapAtCand = bArmed ? d : lagAbs;
                                        c.Load12At = load12; c.Ticks = nTickAll;
                                        MoveJoints_Stop();                                                   // 경로 중단 — 서보는 마지막 목표를 잡는다
                                    }
                                };
                                MoveJoints_SetMotionSink(sink);
                                int ms = (int)Math.Max(300, (zCur - zMin) / Math.Max(0.5f, cfg.DescentMmPerSec) * 1000f);
                                try { PlayXyzPath(ms, 100, cfg.FuncNumber, new float[] { x, y, zMin }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                                finally { MoveJoints_SetMotionSink(scene.SendPose); }
                                if (!bCand) break;                                                          // 한도까지 내려감 — 접촉 없음

                                // ── 멈춰서 확인: 명령을 멈추고 기다린 뒤 정착 읽기. 걸림이면 서보가 따라와 차이가 준다 ──
                                System.Threading.Thread.Sleep(cfg.HoldMs);
                                string why2; Dictionary<int, float> hold = scene.ReadDeg(out why2);
                                if (hold == null) { c.Note = "확인 읽기 실패: " + why2; break; }
                                float hx, hy, hcz, hfz;
                                CalcF(cfg.FuncNumber, -1, false, out hx, out hy, out hcz);                     // 멈춘 명령(이상) 높이
                                float[] keep2 = new float[ids.Length], act2 = new float[ids.Length];
                                for (int i = 0; i < ids.Length; i++) { keep2[i] = GetData(ids[i]); act2[i] = hold.ContainsKey(ids[i]) ? hold[ids[i]] : keep2[i]; SetData(ids[i], act2[i]); }
                                CalcF(cfg.FuncNumber, -1, false, out hx, out hy, out hfz);
                                for (int i = 0; i < ids.Length; i++) SetData(ids[i], keep2[i]);
                                double eh = hcz - hfz;
                                double lagHold = (czStart - hcz) - (fzStart - hfz);
                                bool bContact = !double.IsNaN(baseE) ? (baseE - eh) >= cfg.DetectMm * 0.7 : lagHold >= Math.Min(cfg.SafetyMm * 0.7, lagAtCand * 0.7);
                                if (log != null) log(string.Format(ci, "    r={0:F0} 구간{1}: 후보({2}) → 확인 {3} (벌어짐 {4:F2}mm, 멈춘 뒤 {5:F2}mm)",
                                    r, nSeg, bSafety ? "안전정지" : "감지", bContact ? "접촉" : "걸림 — 이어서 하강",
                                    gapAtCand, double.IsNaN(baseE) ? lagHold : baseE - eh));
                                if (!double.IsNaN(baseE)) baseKeep = baseE;
                                if (bContact)
                                {
                                    double bref = !double.IsNaN(baseE) ? baseE : (!double.IsNaN(baseKeep) ? baseKeep : (double)(czStart - fzStart));
                                    c.Detected = true; c.Safety = bSafety;
                                    c.FkZ = hfz; c.CmdZ = (float)(hfz + bref); c.BaseErr = (float)bref;
                                    c.JointCmd = keep2; c.JointAct = act2;
                                    c.Load12Before = nLoad > 0 ? (float)(loadSum / nLoad) : float.NaN;
                                }
                                else zCur = hcz;                                                            // 걸림 — 지금 명령 높이에서 이어서 하강
                            }
                            // 곧바로 들어 올린다 (바닥을 누른 채 두지 않는다) — 목표는 이번 하강의 시작 높이 위: 바닥 위임이 확실한 곳
                            PlayXyz(1500, 200, cfg.FuncNumber, x, y, zS + 5, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs);
                            if (!c.Detected && c.Note.Length == 0) c.Note = "한도까지 내려가도 접촉 없음";
                            if (c.Safety) c.Note = string.Format(ci, "안전 정지({0:F1}mm)로 잡힘 — κ 에선 뺀다", cfg.SafetyMm);
                            if (c.Detected) lastFk = c.FkZ;
                            res.Add(c);
                            string row = string.Format(ci, "  r={0,3:F0} #{1}: {2}  명령 {3,6:F2}  FK {4,6:F2}  기준선 {5,6:+0.00;-0.00}  어깨부하 {6,5:F1}→{7,5:F1}%  ({8}틱){9}",
                                r, rep + 1, c.Detected ? "접촉" : "없음", c.CmdZ, c.FkZ, c.BaseErr, c.Load12Before, c.Load12At, c.Ticks,
                                c.Note.Length > 0 ? " — " + c.Note : "");
                            if (log != null) log(row); sb.AppendLine(row);
                        }
                    var ok = res.FindAll(q => q.Detected && !q.Safety);
                    if (ok.Count >= 2)
                    {
                        float mn = float.MaxValue, mx = float.MinValue, fmn = float.MaxValue, fmx = float.MinValue;
                        foreach (var q in ok) { mn = Math.Min(mn, q.CmdZ); mx = Math.Max(mx, q.CmdZ); fmn = Math.Min(fmn, q.FkZ); fmx = Math.Max(fmx, q.FkZ); }
                        string sum = string.Format(ci, "바닥 접촉 {0}회: 닿은 명령 높이 폭 {1:F2}mm ({2:F2}~{3:F2}) — 이것이 남은 높이 오차(바닥이 수평이면), 엔코더 FK 폭 {4:F2}mm",
                            ok.Count, mx - mn, mn, mx, fmx - fmn);
                        if (log != null) log(sum); sb.AppendLine(sum);
                    }
                    report = sb.ToString();
                    return res;
                }
                finally { SagComp_Switch(prev); }
            }

            /// <summary>바닥 접촉 — 원점 센서 방식 (09-28 사용자 제안, FloorProbe 의 한 틱 문턱 오감지를 대체).
            /// 반경마다: 위에서 내려와 자리 잡고 → 초당 PressDescentMmPerSec 로 하강 → FK 가 명령보다 PressLagMm 이상 덜 내려가고
            /// **동시에** 어깨·팔꿈치 부하가 PressLoadPct 이상 변하면 "확실히 누름" → 멈춰 PressHoldMs 유지 → LiftMm 천천히 들어 올림 → 복귀.
            /// 접촉 높이 = 누름 끝 위 FitWindowMm 구간에 꺾임(공중 기울기 1 고정 / 누름 기울기 자유)을 맞춘 꺾임점의 FK — 한 틱 문턱이 아니라 선으로 잡는다.
            /// ★반복되는 양은 FK 바닥이다(09-28 3차: r=260 51.1/51.7, r=155 18.4/18.7) — 명령 높이는 이동 이력(데드밴드)에 따라 수 mm 흔들린다.
            /// 들어 올릴 때의 꺾임도 따로 맞춘다(정지 마찰만큼 늦게 떨어진다 — 그 차이가 마찰의 크기).
            /// 안전: 무장과 무관한 PressSafetyMm 벌어짐·PressSafetyLoadPct 부하 즉시 정지, 반경마다 직전 접촉 기준 시작·한도, 스테일 첫 응답 버림.</summary>
            public List<CFloorContact> SagComp_FloorPress(CScene_t scene, CFloorProbeCfg cfg, CSagComp model,
                                                         CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var res = new List<CFloorContact>();
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn) { report = "실물 미연결 또는 토크 OFF"; return res; }
                CSagComp prev = m_SagComp;
                if (model != null) model.Enabled = true; SagComp_Switch(model);
                int[] ids = new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID };
                int[] sIds = scene.IDs;
                int[] map = new int[ids.Length];
                for (int i = 0; i < ids.Length; i++) map[i] = Array.IndexOf(sIds, ids[i]);
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                double a0 = cfg.YawDeg * Math.PI / 180.0;
                float lastCmd = float.NaN;   // 직전 접촉의 명령 높이 — 창은 명령 기준 (FK 는 자세에 따라 명령과 10mm 가까이 다를 수 있다, 09-28 r=260)
                try
                {
                    if (Array.IndexOf(map, -1) >= 0) { report = "장면에 없는 관절 ID"; return res; }
                    if (log != null) log(string.Format(ci, "바닥 접촉(누르고 들어 올리기): 반경 {0}, 반경마다 {1}회, 하강 {2:F1} mm/s, 누름 판정 벌어짐 {3:F1}mm + 부하 {4:F0}%",
                        string.Join("/", Array.ConvertAll(cfg.Radii, v => v.ToString("F0", ci))), cfg.Repeats, cfg.PressDescentMmPerSec, cfg.PressLagMm, cfg.PressLoadPct));
                    foreach (float r in cfg.Radii)
                        for (int rep = 0; rep < cfg.Repeats; rep++)
                        {
                            if (stop != null && stop()) return res;
                            float x = (float)(r * Math.Cos(a0)), y = (float)(r * Math.Sin(a0));
                            float zS = float.IsNaN(lastCmd) ? cfg.ZStart : Math.Min(cfg.ZStart, lastCmd + cfg.Margin);
                            float zMin = float.IsNaN(lastCmd) ? cfg.ZMin : Math.Max(cfg.ZMin, lastCmd - cfg.Margin);
                            if (!PlayXyz(cfg.MoveMs, 300, cfg.FuncNumber, x, y, zS + 8, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs) ||
                                !PlayXyz(1500, 500, cfg.FuncNumber, x, y, zS, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs))
                            { if (log != null) log(string.Format(ci, "  r={0:F0}: 시작점 이동 실패 — 건너뜀", r)); continue; }
                            var c = new CFloorContact { R = r, X = x, Y = y, FromPress = true };
                            var dX = new List<double>(); var dY = new List<double>();   // 하강: (명령 높이, FK 높이)
                            var uX = new List<double>(); var uY = new List<double>();   // 들어 올림
                            float[] pressCmd = null, pressAct = null, lastCmdQ = null, lastActQ = null;
                            float rr = r;

                            // 한 틱 읽기: zz = (명령 z, FK z, 어깨·팔꿈치 부하 %, 어깨·팔꿈치 PWM %), q = 이상 관절, a = 실측 관절. 실패하면 false
                            Func<float[], float[], float[], bool> readTick = delegate(float[] zz, float[] q, float[] a)
                            {
                                string w; int[][] b = scene.ReadServoBlock(6, out w);
                                if (b == null) return false;
                                float cx, cy, cz, fx, fy, fz;
                                CalcF(cfg.FuncNumber, -1, false, out cx, out cy, out cz);
                                for (int i = 0; i < ids.Length; i++) { q[i] = GetData(ids[i]); a[i] = CSim2Real.Deg(b[map[i]][7]) * scene.SignOf(map[i]); SetData(ids[i], a[i]); }
                                CalcF(cfg.FuncNumber, -1, false, out fx, out fy, out fz);
                                for (int i = 0; i < ids.Length; i++) SetData(ids[i], q[i]);
                                zz[0] = cz; zz[1] = fz; zz[2] = b[map[1]][5] * 0.1f; zz[3] = b[map[2]][5] * 0.1f;
                                zz[4] = b[map[1]][4] * 0.113f; zz[5] = b[map[2]][4] * 0.113f;
                                return true;
                            };

                            // ── ① 누르러 내려가기 ──
                            int nTick = 0; bool bPressed = false, bSafety = false, bArmed = false;
                            float czStart = float.NaN, fzStart = float.NaN, lagArm = 0, base12 = 0, base13 = 0, start12 = 0, start13 = 0;
                            string why = "";
                            Action<int> sinkDown = delegate(int nHint)
                            {
                                scene.SendPose(nHint);
                                if (bPressed) return;
                                float[] zz = new float[6], q = new float[ids.Length], a = new float[ids.Length];
                                if (!readTick(zz, q, a)) return;
                                nTick++;
                                if (nTick <= cfg.StaleTicks) return;                                        // 스테일 응답 버림
                                lastCmdQ = q; lastActQ = a;
                                if (float.IsNaN(czStart)) { czStart = zz[0]; fzStart = zz[1]; start12 = zz[2]; start13 = zz[3]; }
                                float lag = (czStart - zz[0]) - (fzStart - zz[1]);
                                float dl = bArmed ? Math.Max(Math.Abs(zz[2] - base12), Math.Abs(zz[3] - base13))
                                                  : Math.Max(Math.Abs(zz[2] - start12), Math.Abs(zz[3] - start13));
                                SagComp_FloorTraceRow(rr, 1, nTick, bArmed, zz[0], zz[1], zz[0] - zz[1], dl, lag, zz[2], zz[4], zz[3], zz[5], x, y, cfg.RyDeg, q, a);
                                bool bStop = false;
                                if (dl >= cfg.PressSafetyLoadPct) { bSafety = true; bStop = true; why = "부하"; }
                                else if (!bArmed)
                                {
                                    if (lag >= cfg.PressSafetyMm + cfg.PreArmExtraMm) { bSafety = true; bStop = true; why = "무장 전 벌어짐"; }
                                    else if ((fzStart - zz[1]) >= cfg.ArmTravelMm) { bArmed = true; lagArm = lag; base12 = zz[2]; base13 = zz[3]; }
                                }
                                else
                                {
                                    // ★09-29 바로잡음: 전에는 아래 "무장 뒤 표본 추가" 가 이 블록 앞에 끼어 else 를 가로채, 이 판정이 무장 **전**에만 돌았다
                                    //   (09-28 실기 5회 모두 무장 뒤엔 부하 25% 안전 정지와 명령 한도만으로 멈춤 — 그 방식이 꺾임 맞춤엔 가장 잘 맞았다).
                                    float lagRel = lag - lagArm;
                                    if (cfg.PressEndLagMm > 0 && lagRel >= cfg.PressEndLagMm) bStop = true;         // 누름 끝(선택)
                                    else if (cfg.PressPastKneeMm > 0 && cfg.OnlineKneeEveryTicks > 0 && nTick % cfg.OnlineKneeEveryTicks == 0 && dX.Count >= 12)
                                    {
                                        var tmp = new CFloorContact();                                     // 실시간 꺾임 — 꺾임점보다 PressPastKneeMm 내려갔으면 누름 끝
                                        if (SagComp_PressKnee(dX, dY, cfg, tmp) && tmp.CmdZ - zz[0] >= cfg.PressPastKneeMm) bStop = true;
                                    }
                                    else if (cfg.PressUseLoadRule && lagRel >= cfg.PressLagMm && dl >= cfg.PressLoadPct) bStop = true;
                                    else if (lagRel < cfg.PressLagMm * 0.4f) { base12 = 0.8f * base12 + 0.2f * zz[2]; base13 = 0.8f * base13 + 0.2f * zz[3]; }
                                }
                                // 꺾임 맞춤 표본은 무장 뒤부터 — 출발 정지 마찰(FK 가 늦게 출발)이 위쪽에 거꾸로 된 꺾임을 만든다
                                if (bArmed) { dX.Add(zz[0]); dY.Add(zz[1]); }
                                if (bStop)
                                {
                                    bPressed = true; c.LagAtPress = lag - lagArm; c.LoadDeltaAtPress = dl; c.Ticks = nTick;
                                    pressCmd = q; pressAct = a;
                                    MoveJoints_Stop();
                                }
                            };
                            MoveJoints_SetMotionSink(sinkDown);
                            int msDown = (int)Math.Max(300, (zS - zMin) / Math.Max(0.5f, cfg.PressDescentMmPerSec) * 1000f);
                            try { PlayXyzPath(msDown, 100, cfg.FuncNumber, new float[] { x, y, zMin }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                            finally { MoveJoints_SetMotionSink(scene.SendPose); }

                            if (bPressed)
                            {
                                // ── ② 누른 채 유지 → ③ 천천히 들어 올리기 ──
                                System.Threading.Thread.Sleep(cfg.PressHoldMs);
                                float hx, hy, hz; CalcF(cfg.FuncNumber, -1, false, out hx, out hy, out hz);
                                int nUp = 0;
                                Action<int> sinkUp = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);
                                    float[] zz = new float[6], q = new float[ids.Length], a = new float[ids.Length];
                                    if (!readTick(zz, q, a)) return;
                                    nUp++;
                                    if (nUp <= cfg.StaleTicks) return;
                                    uX.Add(zz[0]); uY.Add(zz[1]);
                                    SagComp_FloorTraceRow(rr, 3, nUp, true, zz[0], zz[1], zz[0] - zz[1], 0, 0, zz[2], zz[4], zz[3], zz[5], x, y, cfg.RyDeg, q, a);
                                };
                                MoveJoints_SetMotionSink(sinkUp);
                                int msUp = (int)Math.Max(300, cfg.LiftMm / Math.Max(0.5f, cfg.LiftMmPerSec) * 1000f);
                                try { PlayXyzPath(msUp, 100, cfg.FuncNumber, new float[] { x, y, hz + cfg.LiftMm }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs); }
                                finally { MoveJoints_SetMotionSink(scene.SendPose); }
                            }
                            // 복귀 — 시작 높이 위, 그리고 꺾임점 위 RetractAboveKneeMm 이상 (유격이 한 번에 떨어져도 바닥에 닿지 않게) 천천히
                            {
                                var tmpK = new CFloorContact();
                                float zBack = zS + 5;
                                if (SagComp_PressKnee(dX, dY, cfg, tmpK)) zBack = Math.Max(zBack, tmpK.CmdZ + cfg.RetractAboveKneeMm);
                                else if (dX.Count > 0) { double lo = double.MaxValue; foreach (double v in dX) lo = Math.Min(lo, v); zBack = Math.Max(zBack, (float)lo + cfg.RetractAboveKneeMm); }
                                PlayXyz(Math.Max(1500, cfg.RetractMs), 200, cfg.FuncNumber, x, y, zBack, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs);
                            }

                            // ── 꺾임 맞춤 ──
                            c.Safety = bSafety;
                            {
                                double k, y0, sL, sH, rms;
                                SagComp_PressKnee(dX, dY, cfg, c);
                                if (SagComp_FitHinge(uX.ToArray(), uY.ToArray(), 4, out k, out y0, out sL, out sH, out rms))
                                { c.KneeCmdLift = (float)k; c.FkLift = (float)y0; }
                                c.JointCmd = pressCmd ?? lastCmdQ; c.JointAct = pressAct ?? lastActQ;
                                c.Detected = c.FitOk;
                                if (!bPressed) c.Note = c.FitOk ? "누름 판정 없이 한도 도달 — 꺾임 맞춤으로 접촉" : "한도까지 내려가도 누름·꺾임 없음";
                                else if (!c.FitOk) c.Note = "꺾임 맞춤이 물리적으로 맞지 않음 — 제외";
                                if (bSafety) c.Note += (c.Note.Length > 0 ? ", " : "") + "안전 정지(" + why + ")";
                            }
                            if (c.Detected) lastCmd = c.CmdZ;
                            res.Add(c);
                            string row = string.Format(ci, "  r={0,3:F0} #{1}: {2}  FK 바닥 {3,6:F2}mm (명령 {4,6:F2})  누름 기울기 {6:F2} 잔차 {7:F2}  | 들어올림 FK {8,6:F2} (명령 {9,6:F2})  | 판정 벌어짐 {10:F1}mm 부하 {11:F1}%{12}",
                                r, rep + 1, c.Detected ? "접촉" : "제외", c.FkZ, c.CmdZ, c.SlopeFree, c.SlopePress, c.FitRms, c.FkLift, c.KneeCmdLift,
                                c.LagAtPress, c.LoadDeltaAtPress, c.Note.Length > 0 ? " — " + c.Note : "");
                            if (log != null) log(row); sb.AppendLine(row);
                        }
                    var ok = res.FindAll(q => q.Detected && !q.Safety);
                    if (ok.Count >= 2)
                    {
                        float fmn = float.MaxValue, fmx = float.MinValue, cmn = float.MaxValue, cmx = float.MinValue;
                        foreach (var q in ok) { fmn = Math.Min(fmn, q.FkZ); fmx = Math.Max(fmx, q.FkZ); cmn = Math.Min(cmn, q.CmdZ); cmx = Math.Max(cmx, q.CmdZ); }
                        string sum = string.Format(ci, "바닥 접촉 {0}회: 엔코더 FK 바닥 높이 폭 {1:F2}mm ({2:F2}~{3:F2}) = 엔코더 밖 오차(바닥 수평),  명령 높이 폭 {4:F2}mm ({5:F2}~{6:F2}) = 사용자가 보는 남은 오차",
                            ok.Count, fmx - fmn, fmn, fmx, cmx - cmn, cmn, cmx);
                        if (log != null) log(sum); sb.AppendLine(sum);
                    }
                    report = sb.ToString();
                    return res;
                }
                finally { SagComp_Switch(prev); }
            }

            /// <summary>하강 표본(무장 뒤, 명령 z → FK z)에서 접촉 꺾임을 찾아 c 에 채운다 — FloorPress 와 궤적 재분석이 같이 쓴다.
            /// 누름 끝 위 FitWindowMm 구간만, 공중 기울기 1 고정 (09-28 오프라인: 전체 구간·자유 기울기는 출발 걸림에 끌려 r=120 이 4mm 빗나감).
            /// 창을 넓혀 가며(+6mm, 최대 2번) 꺾임점이 창 안쪽(위 끝에서 2mm 이상)에 서는 첫 창을 쓴다 — 접촉 뒤 깊이 눌린 경우 대비.</summary>
            private static bool SagComp_PressKnee(List<double> dX, List<double> dY, CFloorProbeCfg cfg, CFloorContact c)
            {
                if (dX == null || dX.Count < 9) return false;
                double czEnd = double.MaxValue, czAll = double.MinValue;
                foreach (double v in dX) { czEnd = Math.Min(czEnd, v); czAll = Math.Max(czAll, v); }
                for (int nw = 0; nw < 3 && !c.FitOk; nw++)
                {
                    double win = cfg.FitWindowMm + 6 * nw;
                    if (nw > 0 && czEnd + win - 6 >= czAll) break;                       // 더 넓혀도 표본이 같다
                    var wX = new List<double>(); var wY = new List<double>();
                    for (int i = 0; i < dX.Count; i++) if (dX[i] <= czEnd + win) { wX.Add(dX[i]); wY.Add(dY[i]); }
                    double k, y0, sL, rms;
                    if (!SagComp_FitKnee1(wX.ToArray(), wY.ToArray(), 4, out k, out y0, out sL, out rms)) continue;
                    double czTop = double.MinValue; foreach (double v in wX) czTop = Math.Max(czTop, v);
                    double gain = (1 - sL) * (k - czEnd);
                    c.CmdZ = (float)k; c.FkZ = (float)y0; c.SlopePress = (float)sL; c.SlopeFree = 1f; c.FitRms = (float)rms;
                    c.FitOk = sL < 0.9 && gain >= cfg.KneeMinGainMm && (k - czEnd) >= 1.5 && (czTop - k) >= 2;
                    c.BaseErr = (float)(k - y0);
                }
                return c.FitOk;
            }

            /// <summary>바닥 접촉 궤적(SagComp_GetFloorTraceCsv 형식: r,seg,tick,armed,cz,fz,...)을 다시 분석해 접촉을 뽑는다.
            /// 하강 구간(seg 1)마다 무장 뒤 표본으로 SagComp_PressKnee — 실기 중 판정 문턱을 못 넘은 물렁한 접촉도 꺾임으로 건진다.
            /// 관절 자세는 궤적에 없어 JointCmd/JointAct = null (보정표용, κ 맞춤에는 못 씀).</summary>
            public static List<CFloorContact> SagComp_FloorContactsFromTrace(string csv, CFloorProbeCfg cfg)
            {
                var res = new List<CFloorContact>();
                if (string.IsNullOrEmpty(csv)) return res;
                if (cfg == null) cfg = new CFloorProbeCfg();
                var ci = CultureInfo.InvariantCulture;
                string[] lines = csv.Replace("\r", "").Split('\n');
                if (lines.Length < 2) return res;
                string[] hd = lines[0].Split(',');
                int iR = Array.IndexOf(hd, "r"), iSeg = Array.IndexOf(hd, "seg"), iT = Array.IndexOf(hd, "tick"), iA = Array.IndexOf(hd, "armed"),
                    iCz = Array.IndexOf(hd, "cz"), iFz = Array.IndexOf(hd, "fz");
                if (iR < 0 || iSeg < 0 || iT < 0 || iA < 0 || iCz < 0 || iFz < 0) return res;
                int iX = Array.IndexOf(hd, "x"), iY = Array.IndexOf(hd, "y"), iP = Array.IndexOf(hd, "pitch");
                int[] iQ = { Array.IndexOf(hd, "q1"), Array.IndexOf(hd, "q2"), Array.IndexOf(hd, "q3"), Array.IndexOf(hd, "q4") };
                int[] iAj = { Array.IndexOf(hd, "a1"), Array.IndexOf(hd, "a2"), Array.IndexOf(hd, "a3"), Array.IndexOf(hd, "a4") };
                Func<string[], int, float> F = (ff, k) => (k >= 0 && k < ff.Length && ff[k].Trim().Length > 0) ? float.Parse(ff[k], ci) : float.NaN;
                float curR = float.NaN, curX = float.NaN, curY = float.NaN, curP = float.NaN; int curSeg = -1, prevT = int.MaxValue;
                var dX = new List<double>(); var dY = new List<double>(); var dQ = new List<float[]>(); var dA = new List<float[]>();
                Action flush = delegate()
                {
                    if (curSeg == 1 && !float.IsNaN(curR))
                    {
                        var c = new CFloorContact { R = curR, X = float.IsNaN(curX) ? curR : curX, Y = float.IsNaN(curY) ? 0 : curY,
                                                    Pitch = float.IsNaN(curP) ? 90 : curP, FromPress = true };
                        c.Detected = SagComp_PressKnee(dX, dY, cfg, c);
                        if (c.Detected && dQ.Count == dX.Count)
                        {
                            int best = 0; double bd = double.MaxValue;                   // 꺾임점(명령 높이)에 가장 가까운 표본 = 접촉 순간 자세
                            for (int i = 0; i < dX.Count; i++) { double dd = Math.Abs(dX[i] - c.CmdZ); if (dd < bd) { bd = dd; best = i; } }
                            if (dA[best] != null && !float.IsNaN(dA[best][0])) { c.JointAct = dA[best]; c.JointCmd = dQ[best]; }
                        }
                        c.Note = c.Detected ? "궤적 재분석" : "궤적 재분석 — 꺾임 없음";
                        res.Add(c);
                    }
                    dX = new List<double>(); dY = new List<double>(); dQ = new List<float[]>(); dA = new List<float[]>();
                };
                for (int li = 1; li < lines.Length; li++)
                {
                    string[] f = lines[li].Split(',');
                    if (f.Length < hd.Length) continue;
                    float r = float.Parse(f[iR], ci); int seg = int.Parse(f[iSeg], ci), t = int.Parse(f[iT], ci);
                    float fx = F(f, iX), fy = F(f, iY), fp = F(f, iP);
                    bool bNewPt = !(float.IsNaN(fx) && float.IsNaN(curX)) && (fx != curX || fy != curY || fp != curP);
                    if (seg != curSeg || r != curR || t <= prevT || bNewPt) { flush(); curR = r; curSeg = seg; curX = fx; curY = fy; curP = fp; }
                    prevT = t;
                    if (seg == 1 && f[iA].Trim() == "1")
                    {
                        dX.Add(double.Parse(f[iCz], ci)); dY.Add(double.Parse(f[iFz], ci));
                        dQ.Add(new float[] { F(f, iQ[0]), F(f, iQ[1]), F(f, iQ[2]), F(f, iQ[3]) });
                        dA.Add(new float[] { F(f, iAj[0]), F(f, iAj[1]), F(f, iAj[2]), F(f, iAj[3]) });
                    }
                }
                flush();
                return res;
            }

            /// <summary>꺾임(힌지) 맞춤 — y = y0 + sLow·(x − k)  (x &lt; k),  y0 + sHigh·(x − k)  (x ≥ k), 꺾임점에서 연속.
            /// k 를 표본 값들 사이에서 훑어 제곱오차가 가장 작은 곳을 고른다 (양쪽에 minSide 개 이상).
            /// 바닥 접촉: x = 명령 높이, y = 엔코더 FK 높이 — 위쪽(공중)은 기울기 ≈ 1, 아래쪽(누름)은 ≈ 0. y0 = 닿은 순간의 FK 높이.</summary>
            public static bool SagComp_FitHinge(double[] x, double[] y, int minSide, out double k, out double y0, out double sLow, out double sHigh, out double rms)
            {
                k = y0 = sLow = sHigh = rms = double.NaN;
                if (x == null || y == null || x.Length != y.Length || x.Length < 2 * minSide + 1) return false;
                int n = x.Length;
                var xs = (double[])x.Clone(); Array.Sort(xs);
                double best = double.MaxValue;
                for (int c = minSide; c <= n - minSide; c++)
                {
                    double kc = (c < n) ? 0.5 * (xs[c - 1] + xs[c]) : xs[n - 1];
                    // 회귀자 [1, min(0, x−k), max(0, x−k)] 의 정규방정식 3×3
                    double[,] A = new double[3, 3]; double[] b = new double[3];
                    for (int i = 0; i < n; i++)
                    {
                        double[] f = { 1.0, Math.Min(0.0, x[i] - kc), Math.Max(0.0, x[i] - kc) };
                        for (int p = 0; p < 3; p++) { b[p] += f[p] * y[i]; for (int q = 0; q < 3; q++) A[p, q] += f[p] * f[q]; }
                    }
                    double[] s = CSagComp_Solve3(A, b);
                    if (s == null) continue;
                    double se = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double e = y[i] - (s[0] + s[1] * Math.Min(0.0, x[i] - kc) + s[2] * Math.Max(0.0, x[i] - kc));
                        se += e * e;
                    }
                    if (se < best) { best = se; k = kc; y0 = s[0]; sLow = s[1]; sHigh = s[2]; }
                }
                if (best == double.MaxValue) return false;
                rms = Math.Sqrt(best / n);
                return true;
            }
            /// <summary>꺾임 맞춤(공중 기울기 1 고정) — y = y0 + (x − k)  (x ≥ k),  y0 + sLow·(x − k)  (x &lt; k).
            /// 공중에선 FK 가 명령을 1:1 로 따라간다는 사실을 박아 두면, 누름 기울기가 0.8 처럼 1 에 가까운 물렁한 접촉에서도 꺾임점이 선다.</summary>
            public static bool SagComp_FitKnee1(double[] x, double[] y, int minSide, out double k, out double y0, out double sLow, out double rms)
            {
                k = y0 = sLow = rms = double.NaN;
                if (x == null || y == null || x.Length != y.Length || x.Length < 2 * minSide + 1) return false;
                int n = x.Length;
                var xs = (double[])x.Clone(); Array.Sort(xs);
                double best = double.MaxValue;
                int stride = Math.Max(1, (n - 2 * minSide) / 60);                     // 꺾임 후보는 최대 ~60개 (실시간 호출 부담, 간격 ≈ 창/60)
                for (int c = minSide; c <= n - minSide; c += stride)
                {
                    double kc = (c < n) ? 0.5 * (xs[c - 1] + xs[c]) : xs[n - 1];
                    // t = y − max(0, x−k) = y0 + sLow·min(0, x−k) — 2×2 정규방정식
                    double s11 = 0, s12 = 0, s22 = 0, b1 = 0, b2 = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double m = Math.Min(0.0, x[i] - kc), t = y[i] - Math.Max(0.0, x[i] - kc);
                        s11 += 1; s12 += m; s22 += m * m; b1 += t; b2 += m * t;
                    }
                    double det = s11 * s22 - s12 * s12;
                    if (Math.Abs(det) < 1e-12) continue;
                    double a0 = (b1 * s22 - b2 * s12) / det, a1 = (s11 * b2 - s12 * b1) / det;
                    double se = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double e = y[i] - (a0 + a1 * Math.Min(0.0, x[i] - kc) + Math.Max(0.0, x[i] - kc));
                        se += e * e;
                    }
                    if (se < best) { best = se; k = kc; y0 = a0; sLow = a1; }
                }
                if (best == double.MaxValue) return false;
                rms = Math.Sqrt(best / n);
                return true;
            }
            private static double[] CSagComp_Solve3(double[,] A, double[] b)
            {
                double[,] M = (double[,])A.Clone(); double[] v = (double[])b.Clone();
                for (int c = 0; c < 3; c++)
                {
                    int p = c; for (int r = c + 1; r < 3; r++) if (Math.Abs(M[r, c]) > Math.Abs(M[p, c])) p = r;
                    if (Math.Abs(M[p, c]) < 1e-12) return null;
                    if (p != c) { for (int j = 0; j < 3; j++) { double t = M[c, j]; M[c, j] = M[p, j]; M[p, j] = t; } double tb = v[c]; v[c] = v[p]; v[p] = tb; }
                    for (int r = c + 1; r < 3; r++) { double m = M[r, c] / M[c, c]; for (int j = c; j < 3; j++) M[r, j] -= m * M[c, j]; v[r] -= m * v[c]; }
                }
                double[] xo = new double[3];
                for (int r = 2; r >= 0; r--) { double s = v[r]; for (int j = r + 1; j < 3; j++) s -= M[r, j] * xo[j]; xo[r] = s / M[r, r]; }
                return xo;
            }

            /// <summary>외부 휨 배율 κ 한 단위가 관절 자세 q 에서 TCP 를 올리는 양(mm) — A = −Σ_j (∂z/∂θ_j)·c_j·L_j.</summary>
            public double SagComp_FlexBasis(CSagComp m, float[] q)
            {
                if (m == null || !m.HasJoint || q == null) return 0;
                int nj = m.JointIDs.Length;
                float[] lev = SagComp_JointLevers(m.FuncNumber, m.JointIDs, q);
                float[] keep = new float[nj];
                for (int j = 0; j < nj; j++) keep[j] = GetData(m.JointIDs[j]);
                double A = 0;
                try
                {
                    for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], q[j]);
                    for (int j = 0; j < nj; j++)
                    {
                        if (m.Jc[j] == 0) continue;
                        float x, y, zp, zm;
                        SetData(m.JointIDs[j], q[j] + 0.5f); CalcF(m.FuncNumber, -1, false, out x, out y, out zp);
                        SetData(m.JointIDs[j], q[j] - 0.5f); CalcF(m.FuncNumber, -1, false, out x, out y, out zm);
                        SetData(m.JointIDs[j], q[j]);
                        A += -((zp - zm) / 1.0) * m.Jc[j] * lev[j];
                    }
                }
                finally { for (int j = 0; j < nj; j++) SetData(m.JointIDs[j], keep[j]); }
                return A;
            }

            /// <summary>바닥 접촉(SagComp_FloorPress) 결과로 바닥 보정표를 만든다 — 반경별로 유효 접촉(Detected = 꺾임 맞춤 통과)의
            /// 꺾임점 FK 높이를 평균해 FloorDz = 평균 − 기준. 기준 = refZ (NaN 이면 반경별 평균들의 평균 → 보정이 0 을 중심으로 앉는다).
            /// 반환 = 표에 들어간 반경 수 (2 미만이면 표를 바꾸지 않는다). 같은 반경의 반복 폭이 maxSpreadMm 를 넘으면 그 반경은 뺀다.</summary>
            public int SagComp_FitFloorMap(List<CFloorContact> contacts, CSagComp model, double refZ, double maxSpreadMm, out string report)
            { return SagComp_FitFloorMap(contacts, model, refZ, maxSpreadMm, false, out report); }
            /// <summary>bUseCmd = true: FK 대신 꺾임점의 **명령 높이**로 표를 만든다 — 사용자가 보는 "명령 높이 vs 실제" 를 그대로 평평하게
            /// (측정 때 켜져 있던 관절 모델의 남은 오차까지 포함, 위에서 내려오는 접근 기준). false: FK(엔코더 밖 오차만).</summary>
            public int SagComp_FitFloorMap(List<CFloorContact> contacts, CSagComp model, double refZ, double maxSpreadMm, bool bUseCmd, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (model == null || contacts == null) { report = "모델 또는 접촉 없음"; return 0; }
                var dic = new SortedDictionary<double, List<float>>();
                foreach (var c in contacts)
                {
                    float zc = bUseCmd ? c.CmdZ : c.FkZ;
                    if (!c.Detected || float.IsNaN(zc)) continue;                    // 안전 정지여도 꺾임 맞춤이 통과했으면 쓴다 — 꺾임점은 멈추기 전 표본으로 정해진다
                    double key = Math.Round(c.R * 2.0) / 2.0;
                    List<float> l; if (!dic.TryGetValue(key, out l)) { l = new List<float>(); dic[key] = l; }
                    l.Add(zc);
                }
                var sb = new StringBuilder();
                var lr = new List<float>(); var lz = new List<double>();
                foreach (var kv in dic)
                {
                    double mn = double.MaxValue, mx = double.MinValue, sum = 0;
                    foreach (float v in kv.Value) { mn = Math.Min(mn, v); mx = Math.Max(mx, v); sum += v; }
                    double mean = sum / kv.Value.Count, spread = mx - mn;
                    bool ok = spread <= maxSpreadMm;
                    sb.AppendLine(string.Format(ci, "  r={0,5:F1}: {5} 바닥 {1,6:F2}mm (n={2}, 반복 폭 {3:F2}){4}", kv.Key, mean, kv.Value.Count, spread, ok ? "" : " — 반복 폭 초과, 제외", bUseCmd ? "명령" : "FK"));
                    if (ok) { lr.Add((float)kv.Key); lz.Add(mean); }
                }
                if (lr.Count < 2) { report = sb.ToString() + "유효 반경이 2개 미만 — 보정표를 바꾸지 않음"; return lr.Count; }
                double rf = refZ;
                if (double.IsNaN(rf)) { rf = 0; foreach (double v in lz) rf += v; rf /= lz.Count; }
                model.FloorR = lr.ToArray();
                model.FloorDz = lz.ConvertAll(v => (float)(v - rf)).ToArray();
                model.FloorRef = (float)rf;
                model.FloorFromCmd = bUseCmd;
                double zmn = double.MaxValue, zmx = double.MinValue; foreach (double v in lz) { zmn = Math.Min(zmn, v); zmx = Math.Max(zmx, v); }
                sb.AppendLine(string.Format(ci, "바닥 보정표({9} 기준): {0}개 반경 {1:F0}~{2:F0}mm, 바닥 폭 {3:F2}mm ({4:F2}~{5:F2}), 기준 {6:F2}mm → Δz {7:+0.00;-0.00}~{8:+0.00;-0.00}mm",
                    lr.Count, lr[0], lr[lr.Count - 1], zmx - zmn, zmn, zmx, rf, zmn - rf, zmx - rf, bUseCmd ? "명령 높이" : "FK"));
                report = sb.ToString();
                return lr.Count;
            }

            /// <summary>바닥 접촉 결과로 외부 휨 배율 κ 를 맞춘다. 반환 = 이번에 바꾼 양. 접촉이 3개 미만이거나 A 변화가 작으면 0.
            /// · FloorPress 결과(FromPress): 꺾임점의 엔코더 FK 높이 = 바닥 + κ·A(자세) — 닿는 순간(접촉력 0)의 FK 는 명령·이미 적용된 κ 와
            ///   무관하게 "바닥 + 엔코더 밖 휨" 이므로 κ 를 절대값으로 정한다 (model.FlexGain = κ).
            /// · FloorProbe 결과: 닿은 명령 높이 = 바닥 + κ'·A(자세), model.FlexGain += κ' (측정 때 적용돼 있던 κ 위에 더한다).</summary>
            public double SagComp_FitFlex(List<CFloorContact> contacts, CSagComp model, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (model == null || !model.HasJoint || contacts == null) { report = "관절 모델 없음"; return 0; }
                var ok = contacts.FindAll(q => q.Detected && !q.Safety && q.JointCmd != null);
                if (ok.Count < 3) { report = "유효 접촉이 3개 미만 — κ 를 맞추지 않음"; return 0; }
                bool bAbs = ok.TrueForAll(q => q.FromPress);
                int n = ok.Count; double[] A = new double[n], Z = new double[n];
                for (int i = 0; i < n; i++) { A[i] = SagComp_FlexBasis(model, ok[i].JointCmd); Z[i] = bAbs ? ok[i].FkZ : ok[i].CmdZ; }
                double ma = 0, mz = 0; for (int i = 0; i < n; i++) { ma += A[i]; mz += Z[i]; } ma /= n; mz /= n;
                double saa = 0, saz = 0; for (int i = 0; i < n; i++) { saa += (A[i] - ma) * (A[i] - ma); saz += (A[i] - ma) * (Z[i] - mz); }
                if (Math.Sqrt(saa / n) < 0.3) { report = "자세에 따른 휨 기저 변화가 너무 작음 — 반경을 넓혀 다시"; return 0; }
                double k = saz / saa;
                double kClamp = Math.Max(-1.0, Math.Min(3.0, k));
                double se0 = 0, se1 = 0, mn0 = double.MaxValue, mx0 = double.MinValue, mn1 = double.MaxValue, mx1 = double.MinValue;
                for (int i = 0; i < n; i++)
                {
                    double z1 = Z[i] - kClamp * A[i];
                    se0 += (Z[i] - mz) * (Z[i] - mz); se1 += (z1 - (mz - kClamp * ma)) * (z1 - (mz - kClamp * ma));
                    mn0 = Math.Min(mn0, Z[i]); mx0 = Math.Max(mx0, Z[i]); mn1 = Math.Min(mn1, z1); mx1 = Math.Max(mx1, z1);
                }
                double kOld = model.FlexGain;
                model.FlexGain = bAbs ? kClamp : kOld + kClamp;
                report = string.Format(ci, "외부 휨 배율({9}): κ {0:+0.000;-0.000} → {1:+0.000;-0.000} ({2:+0.000;-0.000}{3}) — {10} 폭 {4:F2} → {5:F2}mm (예상), RMS {6:F2} → {7:F2}mm, 바닥 ≈ {8:F2}mm",
                    kOld, model.FlexGain, kClamp, kClamp != k ? string.Format(ci, ", 범위 밖 {0:F3} 을 자름", k) : "",
                    mx0 - mn0, mx1 - mn1, Math.Sqrt(se0 / n), Math.Sqrt(se1 / n), mz - kClamp * ma,
                    bAbs ? "누름 꺾임 FK, 절대값" : "닿은 명령 높이, 누적", bAbs ? "꺾임점 FK 높이" : "닿은 명령 높이");
                return model.FlexGain - kOld;
            }

            /// <summary>한 지점(정지): PlayXyz 로 이동 → 정착 → 엔코더 읽기 → FK 로 실측 TCP.
            /// 실측 TCP 는 3D 관절 데이터를 잠시 실측 각도로 바꿔 CalcF 를 부른 뒤 원복해 얻는다.</summary>
            private bool SagComp_MeasureOne(CScene_t scene, CSagSurveyCfg cfg, float x, float y, float z, int approach,
                                            out CSagSample sample, out string why)
            {
                sample = null; why = "";
                int[] ids = new int[] { cfg.PosIDs[0], cfg.PosIDs[1], cfg.PosIDs[2], cfg.WristID };
                if (!PlayXyz(cfg.MoveMs, cfg.SettleMs, cfg.FuncNumber, x, y, z, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs))
                { why = "PlayXyz 실패(IK 불가?)"; return false; }
                float[] cmd = new float[ids.Length];
                for (int i = 0; i < ids.Length; i++) cmd[i] = GetData(ids[i]);
                Dictionary<int, float> act = scene.ReadDeg(out why);   // 정착 재시도 포함 — 스테일 대비 2회
                if (act == null) return false;
                act = scene.ReadDeg(out why);
                if (act == null) return false;
                float[] a = new float[ids.Length];
                for (int i = 0; i < ids.Length; i++) { if (!act.ContainsKey(ids[i])) { why = "ID " + ids[i] + " 없음"; return false; } a[i] = act[ids[i]]; }
                float ax, ay, az;
                for (int i = 0; i < ids.Length; i++) SetData(ids[i], a[i]);
                CalcF(cfg.FuncNumber, -1, false, out ax, out ay, out az);
                for (int i = 0; i < ids.Length; i++) SetData(ids[i], cmd[i]);
                sample = new CSagSample
                {
                    CmdX = x, CmdY = y, CmdZ = z, ActX = ax, ActY = ay, ActZ = az,
                    JointCmd = cmd, JointAct = a, Approach = approach, Dynamic = false,
                    JointDir = SagComp_JointDirNow(ids),                            // 출력 틱(SendPose)이 추적한 관절별 이동 방향
                    JointLever = SagComp_JointLevers(cfg.FuncNumber, ids, cmd)
                };
                return true;
            }
        }
    }
}
