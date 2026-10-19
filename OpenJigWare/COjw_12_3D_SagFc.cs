// ================================================================
// C3d — 첫닿음 바닥 모델 (FC 모델)  2026-10-02
//
// 바닥 접촉의 학습 목표를 "첫닿음 FK 높이(FC)" 로 바꾼 엔코더 밖 처짐 모델 — 반경 보정표·바닥 GP 의 다음 단계.
//   옛 데이터셋 fkz 의 가까운 쪽(r ≤ 200) 꺾임은 공중 상태가 아니라 첫닿음(FK 38~46) 뒤 14~30mm 를 더 누른 끝의 단단한 정지점이었다.
//   어느 꺾임이 기록되는지는 측정 계획의 하강 한도가 정했다 → "두 상태 띠"·r≈190 계단은 절차가 만든 것.
//   그 꺾임을 학습하면 먼쪽−가까운쪽 차이가 35mm, 첫닿음으로 바꾸면 11mm 이고 계단이 없다 (사용자 보고 ≈28mm 와 맞음).
//
//   FC(q) = c0 + k12·φ12 + k13·φ13 + b14·Jz14 + clip(g(x), ±2)      φ_j = Jz_j·L_j
//   dz    = (FC(q) − Ref)·w_env,  |dz| ≤ 15  → SagComp_FloorJointDelta (끝의 반경·공구 피치 유지 — 보정표·GP 와 같은 경로)
//     q     = 출력 훅의 이상 자세 (보정 전). Jz_j = ∂z_TCP/∂q_j (±0.5° 중앙차분, mm/deg), L_j = 중력 지레 (SagComp_JointLevers, mm/deg)
//     물리 평균: k12 ∈ [0,3], k13 ∈ [0,2], b14 ∈ [−8,8]° — 처짐은 끝을 낮추기만 한다(FK 바닥이 오른다). 능선 사전 표준편차 5/5/3 (잡음 1mm)
//     잔차 g : 영평균 ARD-RBF GP (CSagGp), 입력 x = (L12, L13, L14, q11) — 높이와 무관한 지레·요만 (학습 자세의 높이가 입력으로 새지 않게).
//              데이터에서 멀어지면 저절로 0 → 물리 평균만 남는다. 출력은 ±2mm 로 자른다 (첫닿음이 없는 가장자리에서 넘겨짚지 않게)
//     w_env : 작업 범위(r 100~275mm, 피치 60~125°, z −20~180mm) 밖에서만 선형 감쇠 (30mm·15° 에서 0) — 접힌 자세·위로 편 자세 보호.
//             범위 안에는 띠·데이터 거리 감쇠가 없다 (gpbadr·gpdom 같은 장치 없음 — 사용자 요구)
//     표준편차 = sqrt(hᵀΣh + 잔차 분산) — 진단값(SagComp_LastGpStd)일 뿐 보정을 줄이지 않는다
// 우선순위 (출력 훅·처짐 쌍둥이): fcon=1 → 첫닿음 모델, 아니면 바닥 GP, 그것도 없으면 반경 보정표. flooron=0 이면 전부 끈다.
// 모델 파일: fcon / fcbeta / fcbnd / fcprior / fccov / fcell / fcsig / fccap / fcenv / [fcref] / fcx (CSagComp.Save·Load).
//   fcbeta·fcell·fcsig(기준 Ref 포함)·fcx 가 없거나 fc* 줄이 하나라도 잘못되면 불러올 때 통째로 버린다 → 바닥 GP·반경 보정표 (CSagComp.FcDropped, Summary 에 표시)
//   불러온 뒤 학습점(fcx: 이상 관절각, 첫닿음 FK 높이, 묶음)마다 지레·∂z/∂q 를 다시 계산해 잔차를 구하고 촐레스키를 푼다
//   — SagComp_FcPrepare (SagComp_Load·Set·Enable·SagTwin_SetModel 이 부르고, 안 됐으면 처음 쓸 때).
// 스레드: 첫닿음 모델의 지레·∂z/∂q 는 CalcF 와 같은 DH 계산을 **관절값 사본**으로 한다 (SagFc_Fk) — 3D 관절값(SetData)·CalcF 정적 상태를
//   건드리지 않으므로 불러오기·전환·준비·학습·예측을 모션·실물 스트리밍 중에 다른 스레드에서 해도 보내는 명령에 섞이지 않는다.
//   (10-02 검증: 준비를 부른 스레드에서 3D 관절값을 잠깐 바꿔 가며 하자, 작업 스레드 PlayXyz → SendPose 가 그 학습 자세를 실물 명령으로 보냈다 — 한 틱 최대 68°)
//   측정 계획(SagComp_FcPlan)의 IK 도 CalcInv_Tool 과 같은 풀이를 사본으로 한다 (SagFc_CalcInvTool).
//   전환·켬끔·쓰고 있는 모델의 다시 학습은 모델·첫닿음 모델·전환 기준·시계를 한 lock 안에서 함께 바꾸고, 출력 틱은 틱 시작 때 한 번에 읽는다
//   (SagComp_SwitchLocked) — 다른 스레드에서 해도 1초 크로스페이드가 깨지지 않는다 (보정이 한 틱에 0 으로 빠지거나 새 값으로 튀지 않는다).
// 학습: SagComp_FitFloorFc(데이터셋 CSV, 모델, out 보고) — kind=F·hold=0 행 (kind 열이 없으면 전부), 묶음 = session_group.
// 측정: SagComp_FcPlan 이 점마다 하강 시작 = 예측 FC + 20, 한도 = 예측 FC − 12 를 준다 → 기존 꺾임 맞춤(아래 14mm 창)이 곧 첫닿음.
//       옛 궤적은 SagComp_FloorContactsFromTrace(csv, cfg{ FirstTouch = true }) — F0 = 저장된 꺾임이 첫닿음, F1 = 저장된 꺾임 위를 다시 훑음.
//       데이터셋 kind: F = F0 (학습은 이것만), H = 단단한 정지 위에서 F1 이 찾음, C = 단단한 정지·첫닿음 없음,
//       X = 저장된 꺾임을 쓸 수 없음 — fc 가 차 있으면 F1 이 찾은 첫닿음 (교차 확인용, 학습 안 함).
// ================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            /// <summary>첫닿음 바닥 모델 — 부호·상한을 제약한 물리 평균 + 잔차 GP. 끝을 올릴 양 dz = (FC − Ref)·w_env.
            /// 계수·학습점은 모델 파일 fc* 키로 저장하고, 쓰기 전에 C3d.SagComp_FcPrepare 가 지레·야코비안(관절값 사본 FK — CalcF 와 같은 값)으로 잔차 GP 를 푼다.</summary>
            public class CSagFc
            {
                // ── 물리 평균 FC = c0 + k12·φ12 + k13·φ13 + b14·Jz14 ──
                public double[] Beta = { 0, 0, 0, 0 };                  // c0 (mm), k12, k13 (deg²/mm), b14 (deg)
                public double[] Bounds = { 0, 3, 0, 2, -8, 8 };         // k12·k13·b14 하한·상한
                public double[] PriorSd = { 5, 5, 3 };                  // 능선 사전 표준편차 (k12, k13, b14) — 잡음 1mm 기준
                public double[] Cov = null;                             // β 공분산 4×4 (행 우선) — 예측 표준편차용
                // ── 잔차 GP (입력 L12, L13, L14, q11) ──
                public double[] Ell = { 0.25, 1, 0.3, 1000 };           // 길이 척도 (L12·L13·L14 mm/deg, 요 deg)
                public double SigF = 4, SigN = 0.5;                     // 잔차 신호·잡음 (mm)
                // ── 적용 ──
                public double Ref = 0;                                  // 기준 FK 높이 (mm) — dz = FC − Ref. 학습 평균 또는 fcref
                public bool RefFixed = false;                           // fcref — 다시 학습해도 학습 평균 대신 이 값을 쓴다
                public double StdLimit = 4;                             // 진단 한계 표준편차 (mm) — 넘으면 넘겨짚기로 표시만 (보정은 그대로)
                public double ResidCap = 2, DzMax = 15;                 // 잔차 상한, 전체 |dz| 상한 (mm)
                public double[] Env = { 100, 275, 60, 125, -20, 180, 30, 15 };   // 작업 범위 r(mm)·피치(deg)·z(mm) 하한·상한 + 바깥 감쇠폭 (mm, deg)
                public bool Enabled = true;                             // fcon — false 면 출력 훅은 GP·반경 보정표로 (있으면)
                // ── 학습점 ──
                public double[][] Q = new double[0][];                  // 이상 관절각 (PosIDs 3개 + 손목, deg)
                public double[] Y = new double[0];                      // 첫닿음 FK 높이 (mm)
                public string[] Groups = new string[0];                 // 묶음 이름 (교차검증 단위)
                // ── 학습 보고 (SagComp_FitFloorFc) ──
                public double CvRmse = double.NaN;                      // 묶음 교차검증 RMSE (잔차 포함, ±ResidCap 자름), mm
                public double CvPhysRmse = double.NaN;                  // 같은 교차검증, 물리 평균만 (묶음마다 다시 맞춤), mm
                public string Report = "";

                private volatile CSagGp m_gp = null;                    // 준비된 잔차 GP
                private volatile bool m_bReady = false;
                private volatile bool m_bPrepFailed = false;            // 준비 실패 — Invalidate 전까지 다시 하지 않는다 (출력 틱마다 다시 풀지 않게)
                private readonly object m_oPrep = new object();         // 같은 모델을 두 스레드가 함께 준비하지 않게
                private double m_dRefLoaded = double.NaN;               // 불러오는 중 fcref
                private int m_nLoadReq = 0;                             // 불러오는 중 — 꼭 있어야 하는 줄을 제대로 읽은 표시 (1 fcbeta, 2 fcell, 4 fcsig)
                private int m_nLoadBad = 0;                             // 불러오는 중 — 잘못된 fc* 줄 수 (값 개수·숫자 오류, 유한하지 않은 값)

                public int Count { get { return Q == null ? 0 : Q.Length; } }
                /// <summary>계수·학습점이 온전한가 (준비 전이어도 true). 학습점이 없는 빈 모델은 쓰지 않는다.</summary>
                public bool HasData
                {
                    get
                    {
                        if (Beta == null || Beta.Length != 4 || Ell == null || Ell.Length < 4) return false;
                        foreach (double v in Beta) if (double.IsNaN(v) || double.IsInfinity(v)) return false;
                        if (Q == null || Y == null || Q.Length != Y.Length || Q.Length == 0) return false;
                        foreach (double[] q in Q) if (q == null || q.Length < 4) return false;
                        return !double.IsNaN(Ref) && !double.IsInfinity(Ref);
                    }
                }
                /// <summary>잔차 GP 까지 준비됨 (SagComp_FcPrepare)</summary>
                public bool IsReady { get { return m_bReady && HasData; } }
                /// <summary>준비가 실패했다 (특이 행렬 등) — Invalidate 하거나 SagComp_FcPrepare 를 다시 부르기 전까지 쓰지 않는다</summary>
                public bool PrepareFailed { get { return m_bPrepFailed; } }
                /// <summary>준비된 잔차 GP (진단용, 준비 전이면 null)</summary>
                public CSagGp ResidualGp { get { return m_gp; } }
                /// <summary>계수·학습점을 손으로 바꿨으면 부른다 — 다음 사용 때 다시 준비</summary>
                public void Invalidate() { m_bReady = false; m_gp = null; m_bPrepFailed = false; }
                internal void SetPrepared(CSagGp gp) { m_gp = gp; m_bPrepFailed = false; m_bReady = true; }
                internal void SetFailed() { m_bReady = false; m_gp = null; m_bPrepFailed = true; }
                internal object PrepLock { get { return m_oPrep; } }

                /// <summary>물리 평균 (mm) = c0 + k12·φ12 + k13·φ13 + b14·Jz14</summary>
                public double PhysMean(double phi12, double phi13, double jz14)
                {
                    return Beta[0] + Beta[1] * phi12 + Beta[2] * phi13 + Beta[3] * jz14;
                }

                /// <summary>첫닿음 FK 높이 예측 (mm) = 물리 평균 + 잘린 잔차. l·jz = 그 자세의 지레·∂z/∂q (12·13·14 순, mm/deg), yaw = 베이스 요 (deg).
                /// phys = 물리 평균, resid = ±ResidCap 로 자른 잔차, residRaw = 자르기 전, std = sqrt(hᵀΣh + 잔차 분산). 준비 전이면 NaN.</summary>
                public double Predict(double l12, double l13, double l14, double yaw, double jz12, double jz13, double jz14,
                                      out double phys, out double resid, out double residRaw, out double std)
                {
                    phys = resid = residRaw = std = double.NaN;
                    if (!IsReady) return double.NaN;
                    double[] h = { 1.0, jz12 * l12, jz13 * l13, jz14 };
                    phys = PhysMean(h[1], h[2], h[3]);
                    double vp = 0;
                    if (Cov != null && Cov.Length == 16)
                        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) vp += h[i] * Cov[i * 4 + j] * h[j];
                    double g = 0, vg = 0;
                    CSagGp gp = m_gp;                                   // 한 번만 읽는다 (다른 스레드가 다시 준비해도 이 틱은 같은 GP)
                    if (gp != null && gp.IsReady)
                    {
                        double gm, gs; gp.Predict(new double[] { l12, l13, l14, yaw }, out gm, out gs);
                        g = gm; vg = gs * gs;
                    }
                    residRaw = g;
                    resid = Math.Max(-ResidCap, Math.Min(ResidCap, g));
                    std = Math.Sqrt(Math.Max(0, vp) + vg);
                    return phys + resid;
                }

                /// <summary>작업 범위 가중 — 범위 안 1, 밖은 감쇠폭에 걸쳐 선형으로 0 (r·z 는 mm 폭, 피치는 deg 폭), 세 축의 곱. NaN 이면 0.</summary>
                public double EnvWeight(double r, double pitch, double z)
                {
                    if (Env == null || Env.Length < 8) return 1;
                    return EnvRamp(r, Env[0], Env[1], Env[6]) * EnvRamp(pitch, Env[2], Env[3], Env[7]) * EnvRamp(z, Env[4], Env[5], Env[6]);
                }
                private static double EnvRamp(double v, double lo, double hi, double w)
                {
                    if (double.IsNaN(v)) return 0;
                    double d = v < lo ? lo - v : (v > hi ? v - hi : 0);
                    if (d <= 0) return 1;
                    return w > 0 ? Math.Max(0, 1 - d / w) : 0;
                }

                /// <summary>끝을 올릴 양 (mm) = (FC − Ref)·w_env, |dz| ≤ DzMax, NaN 이면 0. jz = ∂z/∂q (12·13·14), r·z = 이상 자세 TCP 반경·높이, pitch = 공구 피치 (12+13+14).</summary>
                public double Correction(double l12, double l13, double l14, double yaw, double[] jz, double r, double z, double pitch, out double std)
                {
                    double phys, resid, wenv;
                    return Correction(l12, l13, l14, yaw, jz, r, z, pitch, out std, out phys, out resid, out wenv);
                }
                /// <summary>phys·resid·wenv = 물리 평균, 잘린 잔차, 범위 가중 (진단용)</summary>
                public double Correction(double l12, double l13, double l14, double yaw, double[] jz, double r, double z, double pitch,
                                         out double std, out double phys, out double resid, out double wenv)
                {
                    std = phys = resid = double.NaN; wenv = 0;
                    if (!IsReady || jz == null || jz.Length < 3) return 0;
                    double raw;
                    double fc = Predict(l12, l13, l14, yaw, jz[0], jz[1], jz[2], out phys, out resid, out raw, out std);
                    wenv = EnvWeight(r, pitch, z);
                    double dz = (fc - Ref) * wenv;
                    if (double.IsNaN(dz) || double.IsInfinity(dz)) return 0;
                    return Math.Max(-DzMax, Math.Min(DzMax, dz));
                }

                /// <summary>Summary 한 토막</summary>
                public string SummaryText(bool bFloorOn)
                {
                    return string.Format(CultureInfo.InvariantCulture, ", 첫닿음 바닥 모델 {0}점 (k12 {1:F2} k13 {2:F2}{3}, 기준 {4:F2}mm{5}){6}",
                        Count, Beta[1], Beta[2], double.IsNaN(CvRmse) ? "" : string.Format(CultureInfo.InvariantCulture, ", 교차검증 {0:F2}mm", CvRmse),
                        Ref, RefFixed ? " 고정" : "", !Enabled ? " [꺼짐 — fcon=0]" : (bFloorOn ? "" : " [꺼짐 — flooron=0]"));
                }

                // ── 모델 파일 ──
                /// <summary>모델 파일 fc* 줄 (CSagComp.Save 가 부른다)</summary>
                public void Save(StringBuilder sb)
                {
                    var ci = CultureInfo.InvariantCulture;
                    Func<double[], int, string> J = (a, n) => { var s = new string[Math.Min(n, a.Length)]; for (int i = 0; i < s.Length; i++) s[i] = a[i].ToString("R", ci); return string.Join(",", s); };
                    sb.AppendLine("fcon=" + (Enabled ? "1" : "0") + "   # 0 이면 첫닿음 바닥 모델 끔 (gp*·floor 표는 그대로 남는다)");
                    sb.AppendLine("fcbeta=" + J(Beta, 4) + "   # 물리 평균 c0(mm), k12, k13, b14 — FC = c0 + k12·Jz12·L12 + k13·Jz13·L13 + b14·Jz14");
                    sb.AppendLine("fcbnd=" + J(Bounds, 6) + "   # k12·k13·b14 하한·상한");
                    sb.AppendLine("fcprior=" + J(PriorSd, 3) + "   # 능선 사전 표준편차 (k12, k13, b14), 잡음 1mm 기준");
                    if (Cov != null && Cov.Length == 16) sb.AppendLine("fccov=" + J(Cov, 16) + "   # β 공분산 4×4 (행 우선)");
                    sb.AppendLine("fcell=" + J(Ell, 4) + "   # 잔차 GP 길이 척도 (L12, L13, L14 mm/deg, 요 deg)");
                    sb.AppendLine("fcsig=" + J(new double[] { SigF, SigN, Ref, StdLimit }, 4) + "   # 잔차 신호·잡음 (mm), 기준 Ref (mm), 진단 한계 std (mm)");
                    sb.AppendLine("fccap=" + J(new double[] { ResidCap, DzMax }, 2) + "   # 잔차 상한 (mm), 전체 |dz| 상한 (mm)");
                    sb.AppendLine("fcenv=" + J(Env, 8) + "   # 작업 범위 r(mm)·피치(deg)·z(mm) 와 바깥 감쇠폭 (mm, deg) — 안에서는 감쇠 없음");
                    if (RefFixed) sb.AppendLine("fcref=" + Ref.ToString("R", ci) + "   # 기준 높이 고정 — 다시 학습해도 학습 평균 대신 이 값");
                    if (!double.IsNaN(CvRmse))
                        sb.AppendLine(string.Format(ci, "# 첫닿음 모델 학습: 점 {0}, 묶음 교차검증 {1:F3}mm (물리 평균만 {2:F3}mm)", Count, CvRmse, CvPhysRmse));
                    for (int i = 0; i < Count; i++)
                        sb.AppendLine("fcx=" + J(Q[i], 4) + "," + Y[i].ToString("R", ci) + "," + (Groups != null && i < Groups.Length && Groups[i] != null ? Groups[i] : ""));
                }

                /// <summary>모델 파일 fc* 줄 하나 (v = # 주석을 뗀 값). 반환 = 처리한 키인가. 숫자가 틀리면 예외 (호출자가 그 줄을 건너뛴다).
                /// 값 개수가 틀리거나 유한하지 않은 값이 있는 줄, 숫자 오류로 건너뛴 줄은 '잘못된 줄' 로 세어 두고 값은 쓰지 않는다 → EndLoad 가 모델 전체를 버린다.
                /// fcon 은 세지 않는다 (1 이 아니면 끔 — 끄는 쪽이라 안전).</summary>
                public bool ParseKey(string key, string v)
                {
                    var ci = CultureInfo.InvariantCulture;
                    // 값 nMin~nMax 개, 모두 유한 — 아니면 null. 읽기 전에 잘못된 줄로 세고 맞으면 되돌린다 (숫자 오류 예외로 끝나도 셈이 남는다)
                    Func<int, int, double[]> A = (nMin, nMax) =>
                    {
                        m_nLoadBad++;
                        double[] a = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), ci));
                        if (a.Length < nMin || a.Length > nMax) return null;
                        foreach (double x in a) if (double.IsNaN(x) || double.IsInfinity(x)) return null;
                        m_nLoadBad--;
                        return a;
                    };
                    double[] r;
                    switch (key)
                    {
                        case "fcon": Enabled = v.Trim() == "1"; break;
                        case "fcbeta": if ((r = A(4, 4)) != null) { Beta = r; m_nLoadReq |= 1; } break;
                        case "fcbnd": if ((r = A(6, 6)) != null) Bounds = r; break;
                        case "fcprior": if ((r = A(3, 3)) != null) PriorSd = r; break;
                        case "fccov": if ((r = A(16, 16)) != null) Cov = r; break;
                        case "fcell": if ((r = A(4, 4)) != null) { Ell = r; m_nLoadReq |= 2; } break;
                        case "fcsig": if ((r = A(3, 4)) != null) { SigF = r[0]; SigN = r[1]; Ref = r[2]; if (r.Length > 3) StdLimit = r[3]; m_nLoadReq |= 4; } break;   // 기준 Ref 까지 있어야 한다
                        case "fccap": if ((r = A(1, 2)) != null) { ResidCap = r[0]; if (r.Length > 1) DzMax = r[1]; } break;
                        case "fcenv": if ((r = A(8, 8)) != null) Env = r; break;
                        case "fcref": if ((r = A(1, 1)) != null) m_dRefLoaded = r[0]; break;
                        case "fcx":
                            {
                                m_nLoadBad++;                                   // 위와 같은 셈 — 값 5개 이상(뒤는 묶음 이름), 앞 5개 유한
                                string[] t = v.Split(',');
                                if (t.Length < 5) break;
                                var q = new double[4];
                                for (int k = 0; k < 4; k++) q[k] = double.Parse(t[k].Trim(), ci);
                                double y = double.Parse(t[4].Trim(), ci);
                                bool bFin = !double.IsNaN(y) && !double.IsInfinity(y);
                                foreach (double x in q) if (double.IsNaN(x) || double.IsInfinity(x)) bFin = false;
                                if (!bFin) break;
                                m_nLoadBad--;
                                string g = t.Length > 5 ? string.Join(",", t, 5, t.Length - 5).Trim() : "";
                                var lq = new List<double[]>(Q); lq.Add(q); Q = lq.ToArray();
                                var ly = new List<double>(Y); ly.Add(y); Y = ly.ToArray();
                                var lg = new List<string>(Groups); while (lg.Count < Q.Length - 1) lg.Add(""); lg.Add(g); Groups = lg.ToArray();
                                break;
                            }
                        default: return false;
                    }
                    Invalidate();
                    return true;
                }
                /// <summary>불러오기 끝 — fcref 적용. 반환 = 쓸 수 있는가. 꼭 있어야 하는 줄(fcbeta 4개, fcell 4개, fcsig 3~4개 — 기준 Ref 포함, fcx 하나 이상)이
                /// 없거나 잘못된 fc* 줄이 하나라도 있으면 false → CSagComp.Load 가 첫닿음 모델을 버리고 바닥 GP·반경 보정표로 돌아간다.
                /// (10-02 검증: 그대로 쓰면 fcbeta 가 빠지거나 값이 3개일 때 β = 0 이라 dz = −Ref 로 작업 범위 전체 −15mm (펜이 판을 누름),
                ///  fcsig 가 빠지면 Ref = 0 이라 전체 +15mm 였다)</summary>
                public bool EndLoad()
                {
                    bool bOk = m_nLoadReq == 7 && m_nLoadBad == 0;
                    m_nLoadReq = m_nLoadBad = 0;
                    if (!double.IsNaN(m_dRefLoaded)) { Ref = m_dRefLoaded; RefFixed = true; m_dRefLoaded = double.NaN; }
                    return bOk && HasData;
                }
            }

            // ────────────────────────────────────────────────────────────
            // 특징·준비·예측
            //   지레·∂z/∂q 는 3D 관절값을 바꾸지 않고 관절값 사본으로 계산한다 (SagFc_Fk — CalcF 와 같은 DH 계산이라 같은 값).
            //   SagComp_JointLevers·SagComp_TcpRZ 처럼 SetData 로 잠깐 바꿨다 되돌리는 방식은 모션 스레드에서만 안전하다 —
            //   다른 스레드에서 하면 그 사이 SendPose 가 바뀐 값을 실물로 보내고, CalcF 의 정적 상태(CForward.SetCalcLimit_Axis)가
            //   모션 스레드의 지레 계산과 섞인다. 첫닿음 모델의 준비(학습점 73개 ≈ FK 2000회)는 모델을 불러오거나 전환하는
            //   스레드(앱 화면·파이썬 작업 스레드)에서 돌기 때문에 사본으로 한다.
            // ────────────────────────────────────────────────────────────
            private float[] m_afSagLastFc = null;
            /// <summary>직전 출력 틱의 첫닿음 모델 계산 {물리 평균 FC (mm), 잘린 잔차 (mm), 범위 가중 w_env} — 첫닿음 모델을 안 썼으면 NaN 셋. 진단용</summary>
            public float[] SagComp_LastFcInfo { get { float[] v = m_afSagLastFc; return v == null ? new float[] { float.NaN, float.NaN, float.NaN } : (float[])v.Clone(); } }

            // 첫닿음 모델이 쓰는 관절 = PosIDs 3개 + 손목 (지레·학습점 순서). 맞지 않으면 null
            private static int[] SagFc_Ids(CSagComp m)
            {
                if (m == null || m.PosIDs == null || m.PosIDs.Length < 3) return null;
                return new int[] { m.PosIDs[0], m.PosIDs[1], m.PosIDs[2], m.WristID };
            }
            private static float[] SagFc_Q4(double[] q) { return new float[] { (float)q[0], (float)q[1], (float)q[2], (float)q[3] }; }

            // ── 관절값 사본 FK ──
            // 지금 3D 관절값의 사본 (GetData_Forward 와 같은 256칸, 읽기만 한다). 계산에서 바꾸지 않는 관절은 이 값 그대로 — SetData 방식과 같다
            private double[] SagFc_MotCopy()
            {
                var ad = new double[256];
                for (int i = 0; i < ad.Length; i++) ad[i] = GetData(i);
                return ad;
            }
            // 사본에 관절값 넣기 — SetData 와 같은 규칙 (NaN·무한대는 무시, 관절 한계 CalcLimit)
            private void SagFc_Set(double[] ad, int id, float v)
            {
                if (id >= 0 && id < ad.Length && !float.IsNaN(v) && !float.IsInfinity(v)) ad[id] = CalcLimit(id, v);
            }
            // 수식 nFunc 의 DH 체인을 사본 ad 로 계산 — nAxis ≥ 0 이면 그 모터가 처음 나오는 줄 앞까지(관절 원점), −1 이면 끝점(TCP).
            //   CForward.CalcDhParamAll 과 같은 순서·같은 CMath.CalcT·CalcMatrix 라 CalcF(nFunc, nAxis, false) 와 같은 값이다.
            //   CForward 의 정적 상태(축·수식 번호·깊이 제한)를 쓰지 않아 어느 스레드에서 불러도 된다 (Sag 계산은 수식 번호·깊이 제한을 쓰지 않는다).
            private void SagFc_Fk(int nFunc, double[] ad, int nAxis, out float fX, out float fY, out float fZ)
            {
                fX = fY = fZ = 0;
                CDhParamAll dh = (m_CHeader != null && m_CHeader.pDhParamAll != null && nFunc >= 0 && nFunc < m_CHeader.pDhParamAll.Length) ? m_CHeader.pDhParamAll[nFunc] : null;
                int nCnt = (dh == null) ? 0 : dh.GetCount();
                if (nCnt <= 0) return;
                var aT = new double[nCnt][,];
                int nStop = -1;
                for (int i = 0; i < nCnt; i++)
                {
                    CDhParam p = dh.GetData(i);
                    double dAng = (p.nAxisNum >= 0 && p.nAxisNum < ad.Length) ? ad[p.nAxisNum] : 0;
                    if (nAxis >= 0 && p.nAxisNum >= 0 && p.nAxisNum == nAxis) nStop = i;
                    double dTheta = p.dTheta, dA = p.dA, dD = p.dD;
                    if ((p.nAxisDir < 2) || (p.nAxisDir >= 4)) dTheta += (((p.nAxisNum >= 0) ? dAng : 0) * (((p.nAxisDir == 0) || (p.nAxisDir == 4)) ? 1.0f : -1.0f));   // 회전 (0,1,4,5)
                    else dD += (((p.nAxisNum >= 0) ? dAng : 0) * ((p.nAxisDir == 2) ? 1.0f : -1.0f));                                                                    // 직선 (2,3)
                    Ojw.CMath.CalcT(dA, p.dAlpha, dD, dTheta, out aT[i]);
                    if (nStop >= 0) break;
                }
                int n = (nStop >= 0) ? nStop : nCnt;
                double[,] adR;
                Ojw.CMath.CalcT(0, 0, 0, 0, out adR);
                Ojw.CMath.CalcMatrix(4, adR, aT[0], out adR);
                for (int i = 1; i < n; i++) Ojw.CMath.CalcMatrix(4, adR, aT[i], out adR);
                fX = (float)adR[0, 3]; fY = (float)adR[1, 3]; fZ = (float)adR[2, 3];
            }
            // SagComp_TcpRZ 와 같은 값 — fid 3개에 q 를 넣은 사본의 TCP 반경·높이
            private void SagFc_TcpRZ(int nFunc, double[] adBase, int[] fid, float[] q, out double r, out double z)
            {
                double[] ad = (double[])adBase.Clone();
                for (int k = 0; k < 3; k++) SagFc_Set(ad, fid[k], q[k]);
                float x, y, zz;
                SagFc_Fk(nFunc, ad, -1, out x, out y, out zz);
                r = Math.Sqrt((double)x * x + (double)y * y); z = zz;
            }
            // SagComp_TcpZJac 의 본체 — ±0.5° 중앙차분 (TcpRZ 6회)
            private double[] SagFc_TcpZJac(int nFunc, double[] adBase, int[] fid, float[] q)
            {
                const float D = 0.5f;
                var jz = new double[3];
                for (int k = 0; k < 3; k++)
                {
                    float[] qp = (float[])q.Clone(), qm = (float[])q.Clone();
                    qp[k] = q[k] + D; qm[k] = q[k] - D;
                    double r, zp, zm;
                    SagFc_TcpRZ(nFunc, adBase, fid, qp, out r, out zp);
                    SagFc_TcpRZ(nFunc, adBase, fid, qm, out r, out zm);
                    jz[k] = (zp - zm) / (2 * D);
                }
                return jz;
            }
            // SagComp_JointLevers 와 같은 값 — 관절 j 하류 질량점(뒤 관절 원점들 + TCP) 평균 높이의 ±0.5° 중앙차분
            private float[] SagFc_Levers(int nFunc, int[] jids, float[] pose, double[] adBase)
            {
                const float D = 0.5f;
                int nj = jids.Length;
                float[] lev = new float[nj];
                double[] ad = (double[])adBase.Clone();
                for (int j = 0; j < nj; j++) SagFc_Set(ad, jids[j], pose[j]);
                for (int j = 0; j < nj; j++)
                {
                    SagFc_Set(ad, jids[j], pose[j] + D); double zp = SagFc_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j] - D); double zm = SagFc_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j]);
                    lev[j] = (float)((zp - zm) / (2 * D));
                }
                return lev;
            }
            private double SagFc_MeanZ(int nFunc, double[] ad, int[] jids, int j)
            {
                double s = 0; int n = 0; float x, y, z;
                for (int k = j + 1; k < jids.Length; k++) { SagFc_Fk(nFunc, ad, jids[k], out x, out y, out z); s += z; n++; }   // 관절 k 원점
                SagFc_Fk(nFunc, ad, -1, out x, out y, out z); s += z; n++;                                                   // TCP
                return s / n;
            }
            private static float SagFc_Get(double[] ad, int id) { return (id >= 0 && id < ad.Length) ? (float)ad[id] : 0f; }   // GetData 와 같다

            // ── 관절값 사본 IK — CalcInv_Tool 과 같은 풀이 (손목 분리: 위치 관절 3개 DLS 로 손목 원점 → 손목각은 공구 피치를 실측해 보정) ──
            //   CalcInv_Tool 은 3D 관절값을 바꿔 가며 풀고 끝나면 원복한다 — 다른 스레드의 모션 스텝·SendPose 와 겹치면 그 사이 값이 실물로 나간다
            //   (10-02 시험: 계획을 모션 시작과 겹쳐 부르자 한 틱 48°). 계획(SagComp_FcPlan)은 어느 스레드에서든 부를 수 있게 같은 풀이를
            //   사본으로 한다 — 같은 시드(지금 자세 → 굽힘 시드)·같은 순서·같은 수치라 해도 CalcInv_Tool 과 같다.
            private float[] SagFc_CalcInvTool(int nFunc, double[] adBase, float fX, float fY, float fZ, float fPitchDeg, int nWristID, int[] anPosMotorIDs)
            {
                if (anPosMotorIDs == null || anPosMotorIDs.Length != 3) return null;
                double[] ad = (double[])adBase.Clone();
                float fTx, fTy, fTz, fWx, fWy, fWz;
                SagFc_Fk(nFunc, ad, -1, out fTx, out fTy, out fTz);
                SagFc_Fk(nFunc, ad, nWristID, out fWx, out fWy, out fWz);
                double dWristLen = Math.Sqrt((fTx - fWx) * (fTx - fWx) + (fTy - fWy) * (fTy - fWy) + (fTz - fWz) * (fTz - fWz));   // 손목-TCP 강체 길이
                float[] afSave = { SagFc_Get(ad, anPosMotorIDs[0]), SagFc_Get(ad, anPosMotorIDs[1]), SagFc_Get(ad, anPosMotorIDs[2]), SagFc_Get(ad, nWristID) };
                double[] qSeed1 = { afSave[0], afSave[1], afSave[2] };            // 지금 자세 (연속 해)
                double[] qSeed2 = { afSave[0], -30.0, 60.0 };                     // 굽힘 시드 (재시도)
                float[] afRes = SagFc_SolveTool(nFunc, ad, nWristID, anPosMotorIDs, qSeed1, fX, fY, fZ, fPitchDeg, dWristLen, 1.0);
                if (afRes == null)
                    afRes = SagFc_SolveTool(nFunc, ad, nWristID, anPosMotorIDs, qSeed2, fX, fY, fZ, fPitchDeg, dWristLen, 1.0);
                return afRes;
            }
            // SolveTool_From 과 같은 풀이 (ad 는 풀이용 사본 — 손목각·위치 관절을 바꿔 가며 쓴다)
            private float[] SagFc_SolveTool(int nFunc, double[] ad, int nWristID, int[] anPosIDs, double[] qSeed,
                                            float fX, float fY, float fZ, double dPitchDeg, double dWristLen, double dTolMm)
            {
                double[] q = (double[])qSeed.Clone();
                double dQw = SagFc_Get(ad, nWristID);
                double dPitchRad = dPitchDeg * Math.PI / 180.0;
                double dYawRad = Math.Atan2(fY, fX);
                double dDirX = Math.Cos(dPitchRad) * Math.Cos(dYawRad);
                double dDirY = Math.Cos(dPitchRad) * Math.Sin(dYawRad);
                double dDirZ = -Math.Sin(dPitchRad);
                double dWx = fX - dDirX * dWristLen;
                double dWy = fY - dDirY * dWristLen;
                double dWz = fZ - dDirZ * dWristLen;
                for (int nOuter = 0; nOuter < 6; nOuter++)
                {
                    if (!SagFc_SolveWrist(nFunc, ad, nWristID, anPosIDs, q, dWx, dWy, dWz)) break;
                    SagFc_Set(ad, nWristID, (float)dQw);
                    double dP0 = SagFc_ToolPitch(nFunc, ad, nWristID);
                    SagFc_Set(ad, nWristID, (float)(dQw + 10.0));
                    double dSlope = (SagFc_ToolPitch(nFunc, ad, nWristID) - dP0) / 10.0;
                    if (Math.Abs(dSlope) < 0.2) break;                                 // 피치축이 아님
                    dQw += (dPitchDeg - dP0) / dSlope;
                    SagFc_Set(ad, nWristID, (float)dQw);
                    dQw += (dPitchDeg - SagFc_ToolPitch(nFunc, ad, nWristID)) / dSlope;  // 2차 보정
                    SagFc_Set(ad, nWristID, (float)dQw);
                    float fCx, fCy, fCz;
                    SagFc_Fk(nFunc, ad, -1, out fCx, out fCy, out fCz);
                    double dEx = fX - fCx, dEy = fY - fCy, dEz = fZ - fCz;
                    if (Math.Sqrt(dEx * dEx + dEy * dEy + dEz * dEz) < dTolMm)
                        return new[] { (float)q[0], (float)q[1], (float)q[2], (float)dQw };
                    dWx += dEx; dWy += dEy; dWz += dEz;
                }
                return null;
            }
            // SolveWrist_VT 와 같은 풀이 — 위치 관절 3개 DLS 로 손목축 원점을 (wx, wy, wz) 에
            private bool SagFc_SolveWrist(int nFunc, double[] ad, int nWristID, int[] anPosIDs, double[] q, double dWx, double dWy, double dWz)
            {
                const double dH = 0.5, dLam = 0.5;
                double dBest = double.MaxValue;
                int nNoImprove = 0;
                for (int nIter = 0; nIter < 100; nIter++)
                {
                    for (int i = 0; i < 3; i++) SagFc_Set(ad, anPosIDs[i], (float)q[i]);
                    float fCx, fCy, fCz;
                    SagFc_Fk(nFunc, ad, nWristID, out fCx, out fCy, out fCz);
                    double dEx = dWx - fCx, dEy = dWy - fCy, dEz = dWz - fCz;
                    double dErr2 = dEx * dEx + dEy * dEy + dEz * dEz;
                    if (dErr2 < 0.04) return true;   // 0.2mm
                    if (dErr2 < dBest - 0.25 || dErr2 < dBest * 0.98) { dBest = dErr2; nNoImprove = 0; }
                    else if (++nNoImprove >= 25) return false;
                    double[,] adJ = new double[3, 3];
                    for (int j = 0; j < 3; j++)
                    {
                        q[j] += dH;
                        for (int i = 0; i < 3; i++) SagFc_Set(ad, anPosIDs[i], (float)q[i]);
                        float fPx, fPy, fPz;
                        SagFc_Fk(nFunc, ad, nWristID, out fPx, out fPy, out fPz);
                        q[j] -= dH;
                        adJ[0, j] = (fPx - fCx) / dH; adJ[1, j] = (fPy - fCy) / dH; adJ[2, j] = (fPz - fCz) / dH;
                    }
                    double[,] adA = new double[3, 3];                                  // (JᵀJ + λ²I) dq = Jᵀe — 3x3 크래머
                    double[] adB = new double[3];
                    double[] adE = { dEx, dEy, dEz };
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        { double dS = 0; for (int k = 0; k < 3; k++) dS += adJ[k, r] * adJ[k, c]; adA[r, c] = dS; }
                        adA[r, r] += dLam * dLam;
                        for (int k = 0; k < 3; k++) adB[r] += adJ[k, r] * adE[k];
                    }
                    double dDet = Det3_VT(adA);
                    if (Math.Abs(dDet) < 1e-12) return false;
                    for (int j = 0; j < 3; j++)
                    {
                        double[,] adAj = (double[,])adA.Clone();
                        for (int r = 0; r < 3; r++) adAj[r, j] = adB[r];
                        double dDq = Det3_VT(adAj) / dDet;
                        if (dDq > 20) dDq = 20; else if (dDq < -20) dDq = -20;
                        q[j] += dDq;
                    }
                }
                return false;
            }
            // CalcToolPitch 와 같다 — 'TCP − 손목축 원점' 벡터의 피치 (수평 0 / 수직 아래 +90)
            private float SagFc_ToolPitch(int nFunc, double[] ad, int nWristID)
            {
                float fTx, fTy, fTz, fWx, fWy, fWz;
                SagFc_Fk(nFunc, ad, -1, out fTx, out fTy, out fTz);
                SagFc_Fk(nFunc, ad, nWristID, out fWx, out fWy, out fWz);
                double dVx = fTx - fWx, dVy = fTy - fWy, dVz = fTz - fWz;
                return (float)(Math.Atan2(-dVz, Math.Sqrt(dVx * dVx + dVy * dVy)) * 180.0 / Math.PI);
            }

            /// <summary>TCP 높이의 관절 (fid 3개) 기울기 ∂z/∂θ (mm/deg) — ±0.5° 중앙차분 (SagComp_TcpRZ 6회와 같은 값).
            /// 3D 관절값을 바꾸지 않고 관절값 사본으로 계산한다 (fid 밖 관절은 지금 3D 값) — 어느 스레드에서 불러도 된다.</summary>
            public double[] SagComp_TcpZJac(int nFunc, int[] fid, float[] q)
            {
                return SagFc_TcpZJac(nFunc, SagFc_MotCopy(), fid, q);
            }

            // 특징 — q4 (PosIDs 3개 + 손목) → {L12, L13, L14, Jz12, Jz13, Jz14, φ12, φ13}. ad = 관절값 사본 (바꾸지 않는다)
            private double[] SagFc_Feat(int nFunc, int[] ids, float[] q4, double[] ad)
            {
                float[] lev = SagFc_Levers(nFunc, ids, q4, ad);
                double[] jz = SagFc_TcpZJac(nFunc, ad, new int[] { ids[1], ids[2], ids[3] }, new float[] { q4[1], q4[2], q4[3] });
                return new double[] { lev[1], lev[2], lev[3], jz[0], jz[1], jz[2], jz[0] * lev[1], jz[1] * lev[2] };
            }

            /// <summary>첫닿음 모델 특징 (진단·검증용) — 이상 자세 q4 = (PosIDs 3개 + 손목, deg) 의
            /// {L12, L13, L14 (mm/deg), Jz12, Jz13, Jz14 (mm/deg), φ12, φ13 (mm²/deg²)}. 관절값 사본으로 계산 (3D 관절값 그대로).</summary>
            public double[] SagComp_FcFeatures(CSagComp m, float[] q4)
            {
                int[] ids = SagFc_Ids(m);
                if (ids == null || q4 == null || q4.Length < 4) return null;
                return SagFc_Feat(m.FuncNumber, ids, q4, SagFc_MotCopy());
            }

            /// <summary>첫닿음 모델 준비 — 학습점(fcx)마다 지레·∂z/∂q 를 계산해 물리 평균 잔차를 구하고 잔차 GP 촐레스키를 푼다 (73점 ≈ 15ms).
            /// 관절값 사본으로 계산해 3D 관절값·CalcF 를 건드리지 않는다 — 모션·실물 스트리밍 중에 다른 스레드에서 불러도 보내는 명령에 섞이지 않는다.
            /// 모델 전환(SagComp_Load·Set·Enable)과 쌍둥이 지정 때 부르고, 안 됐으면 처음 쓸 때 부른다. 이 함수는 늘 다시 계산한다.
            /// 다 푼 뒤에 한 번에 바꿔 끼운다 (준비 중에도 이전 상태로 계속 쓸 수 있게). 실패하면 이전 준비를 그대로 두고
            /// (쓰고 있는 보정이 0 으로 뚝 떨어지지 않게), 준비된 적이 없으면 PrepareFailed — 출력 훅은 Invalidate 전까지 0 을 얹는다. 반환 = 이번 준비 성공.</summary>
            public bool SagComp_FcPrepare(CSagComp m)
            {
                return SagFc_Prepare(m, m == null ? null : m.Fc, true);
            }
            // bForce = false 면 이미 준비됐거나 준비에 실패한 모델은 그대로 둔다 (쓰기 전 확인용)
            private bool SagFc_Prepare(CSagComp m, CSagFc f, bool bForce)
            {
                if (m == null || f == null) return false;
                lock (f.PrepLock)
                {
                    if (!bForce && f.IsReady) return true;
                    if (!bForce && f.PrepareFailed) return false;
                    int[] ids = SagFc_Ids(m);
                    double[][] Q = f.Q; double[] Y = f.Y;
                    if (ids == null || !f.HasData || Q == null || Y == null || Q.Length != Y.Length) { if (!f.IsReady) f.SetFailed(); return false; }
                    int n = Q.Length;
                    double[] ad = SagFc_MotCopy();
                    var X = new double[n][]; var R = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        double[] ft = SagFc_Feat(m.FuncNumber, ids, SagFc_Q4(Q[i]), ad);
                        X[i] = new double[] { ft[0], ft[1], ft[2], Q[i][0] };
                        R[i] = Y[i] - f.PhysMean(ft[6], ft[7], ft[5]);
                    }
                    var gp = new CSagGp { Ell = (double[])f.Ell.Clone(), SigF = f.SigF, SigN = f.SigN, Ref = 0, X = X, Y = R };
                    if (!gp.Solve()) { if (!f.IsReady) f.SetFailed(); return false; }
                    f.SetPrepared(gp);
                    return true;
                }
            }
            // 쓰기 전 확인 — 준비된 첫닿음 모델 (안 됐으면 지금 준비, 준비에 실패한 모델은 다시 하지 않는다). 없으면 null.
            //   부른 쪽은 m.Fc 를 다시 읽지 말고 이것을 쓴다 (그 사이 다른 스레드가 m.Fc 를 바꿔 끼울 수 있다)
            private CSagFc SagFc_Ready(CSagComp m) { return SagFc_Ready(m, m == null ? null : m.Fc); }
            // f = 부른 쪽이 이미 읽어 둔 m 의 첫닿음 모델 (출력 틱은 틱 시작 때 전환 상태와 함께 읽는다)
            private CSagFc SagFc_Ready(CSagComp m, CSagFc f)
            {
                if (m == null || f == null || !f.HasData) return null;
                if (f.IsReady) return f;
                if (f.PrepareFailed) return null;
                return SagFc_Prepare(m, f, false) ? f : null;
            }
            private bool SagFc_EnsureReady(CSagComp m) { return SagFc_Ready(m) != null; }

            /// <summary>이상 자세 q4 = (PosIDs 3개 + 손목, deg) 에서 첫닿음 FK 높이 예측 (mm, 범위 감쇠·상한 없음) — 계획·검증용.
            /// std = 예측 표준편차, phys = 물리 평균, resid·residRaw = 잔차 (±ResidCap 자름 / 자르기 전). 모델이 없으면 false.</summary>
            public bool SagComp_FcPredict(CSagComp m, float[] q4, out double fc, out double std, out double phys, out double resid, out double residRaw)
            {
                fc = std = phys = resid = residRaw = double.NaN;
                CSagFc f = (q4 == null || q4.Length < 4) ? null : SagFc_Ready(m);
                if (f == null) return false;
                double[] ft = SagFc_Feat(m.FuncNumber, SagFc_Ids(m), q4, SagFc_MotCopy());
                fc = f.Predict(ft[0], ft[1], ft[2], q4[0], ft[3], ft[4], ft[5], out phys, out resid, out residRaw, out std);
                return !double.IsNaN(fc);
            }
            public bool SagComp_FcPredict(CSagComp m, float[] q4, out double fc, out double std)
            {
                double phys, resid, raw;
                return SagComp_FcPredict(m, q4, out fc, out std, out phys, out resid, out raw);
            }

            /// <summary>출력 훅이 이상 자세 q4 = (PosIDs 3개 + 손목, deg) 에서 얹을 끝 높이 보정 dz (mm) — (FC − Ref)·w_env, |dz| ≤ DzMax.
            /// fcon·flooron 과 무관하게 계산만 한다. info = {물리 평균 FC, 잘린 잔차, w_env}. 모델이 없으면 0. 관절값 사본으로 계산 (3D 관절값 그대로).</summary>
            public double SagComp_FcCorrection(CSagComp m, float[] q4, out double std, out float[] info)
            {
                std = double.NaN; info = new float[] { float.NaN, float.NaN, float.NaN };
                CSagFc f = (q4 == null || q4.Length < 4) ? null : SagFc_Ready(m);
                if (f == null) return 0;
                int[] ids = SagFc_Ids(m);
                int[] fid = { ids[1], ids[2], ids[3] };
                float[] fq = { q4[1], q4[2], q4[3] };
                double[] ad = SagFc_MotCopy();
                double r0, z0; SagFc_TcpRZ(m.FuncNumber, ad, fid, fq, out r0, out z0);
                float[] lev = SagFc_Levers(m.FuncNumber, ids, q4, ad);
                double[] jz = SagFc_TcpZJac(m.FuncNumber, ad, fid, fq);
                double phys, resid, wenv;
                double dz = f.Correction(lev[1], lev[2], lev[3], q4[0], jz, r0, z0, fq[0] + fq[1] + fq[2], out std, out phys, out resid, out wenv);
                info = new float[] { (float)phys, (float)resid, (float)wenv };
                return dz;
            }

            // 출력 훅(SagComp_JointOutModel)의 첫닿음 분기 — pose·lev = 관절 모델 JointIDs 순 이상 자세·지레, fq = (PosIDs[1], PosIDs[2], Wrist) 이상 각,
            //   r0·z0 = 그 자세의 TCP 반경·높이. 반환 = 끝을 올릴 양 dz (mm). 진단값 SagComp_LastFcInfo 를 남긴다.
            //   관절 모델 JointIDs 가 (PosIDs 3개 + 손목) 순서 그대로면(보통) 이미 계산한 지레를 그대로 쓴다 (사본 지레와 같은 값).
            //   fcTick = 출력 틱이 시작 때 전환 상태와 함께 읽은 m.Fc — m.Fc 를 다시 읽지 않는다 (다시 학습이 틱 도중에 바꿔 끼워도 이 틱은 같은 모델).
            //   모델이 아직 준비 전이면 여기서(모션 스레드) 준비한다 — 보통은 전환할 때 이미 준비돼 있다.
            private double SagComp_FcOut(CSagComp m, CSagFc fcTick, int[] ids, float[] afDeg, float[] pose, float[] lev, float[] fq, double r0, double z0, out double std)
            {
                std = double.NaN;
                CSagFc f = SagFc_Ready(m, fcTick);
                if (f == null) return 0;
                int[] fcIds = SagFc_Ids(m);
                float[] q4 = new float[4];
                bool bSame = m.JointIDs != null && m.JointIDs.Length == 4 && lev != null && lev.Length == 4;
                for (int k = 0; k < 4; k++)
                {
                    int jm = m.JointIDs == null ? -1 : Array.IndexOf(m.JointIDs, fcIds[k]), fi = Array.IndexOf(ids, fcIds[k]);
                    q4[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afDeg[fi] : GetData(fcIds[k]));
                    if (bSame && m.JointIDs[k] != fcIds[k]) bSame = false;
                }
                double[] ad = SagFc_MotCopy();
                float[] l4 = bSame ? lev : SagFc_Levers(m.FuncNumber, fcIds, q4, ad);
                double[] jz = SagFc_TcpZJac(m.FuncNumber, ad, new int[] { fcIds[1], fcIds[2], fcIds[3] }, fq);
                double phys, resid, wenv;
                double dz = f.Correction(l4[1], l4[2], l4[3], q4[0], jz, r0, z0, fq[0] + fq[1] + fq[2], out std, out phys, out resid, out wenv);
                m_afSagLastFc = new float[] { (float)phys, (float)resid, (float)wenv };
                return dz;
            }

            // 처짐 쌍둥이(SagTwin_ExternalDz)의 첫닿음 분기 — 출력 훅과 같은 식·같은 자리(이상 자세)라 보정 켬이면 보정분과 상쇄된다. fcNow = 부른 쪽이 읽어 둔 m.Fc
            private double SagTwin_FcDz(CSagComp m, CSagFc fcNow, int[] ids, float[] ideal, float[] fq, double r0, double z0)
            {
                CSagFc f = SagFc_Ready(m, fcNow);
                if (f == null) return 0;
                int[] fcIds = SagFc_Ids(m);
                float[] q4 = new float[4];
                for (int k = 0; k < 4; k++) { int i = Array.IndexOf(ids, fcIds[k]); q4[k] = i >= 0 ? ideal[i] : GetData(fcIds[k]); }
                double[] ad = SagFc_MotCopy();
                float[] l4 = SagFc_Levers(m.FuncNumber, fcIds, q4, ad);
                double[] jz = SagFc_TcpZJac(m.FuncNumber, ad, new int[] { fcIds[1], fcIds[2], fcIds[3] }, fq);
                double std;
                return f.Correction(l4[1], l4[2], l4[3], q4[0], jz, r0, z0, fq[0] + fq[1] + fq[2], out std);
            }

            /// <summary>새 측정 계획 — 점 (x, y, 공구 피치 Ry) 의 첫닿음 FK 높이를 FC 모델로 예측해 하강 시작·한도(명령 높이, mm)를 정한다.
            /// zStart = 예측 + 20 (시작은 이 이상), zMin = 예측 − 12 → 기존 꺾임 맞춤(SagComp_PressKnee, 아래 14mm 창)이 곧 첫닿음을 잡고,
            /// 깊게 누르지 않으니 들 때 처짐 유격이 떨어지는 일도 없다. 예측 자세는 고정점 반복 z = FC(IK(x, y, z, Ry)) 로 찾는다
            /// (IK = CalcInv_Tool 과 같은 풀이, 시드 = 지금 3D 자세).
            /// SagComp_FloorPress 에 쓸 때: 반경마다 따로 부르고(cfg.Radii = {r}) cfg.ZStart = zStart, cfg.ZMin = zMin,
            /// cfg.Margin ≥ 20 (반복 하강의 시작 = 직전 꺾임 + Margin). fcStd 가 모델의 StdLimit 를 넘으면 넘겨짚기(가까운 반경 등) — 결과를 보고 다시 학습.
            /// 스레드: IK·예측을 관절값 사본으로 한다 (3D 관절값·CalcF 를 건드리지 않음) — 모션·실물 스트리밍 중 다른 스레드에서 불러도 된다.</summary>
            public bool SagComp_FcPlan(CSagComp m, float x, float y, float pitchDeg, out float zStart, out float zMin, out double fcPred, out double fcStd)
            {
                zStart = zMin = float.NaN; fcPred = fcStd = double.NaN;
                CSagFc f = SagFc_Ready(m);
                if (f == null) return false;
                int[] pos = { m.PosIDs[0], m.PosIDs[1], m.PosIDs[2] };
                double[] ad = SagFc_MotCopy();                                         // IK 시드·나머지 관절 = 지금 3D 자세 (CalcInv_Tool 과 같다)
                double z = (f.Ref > 0 && f.Ref < 1000) ? f.Ref : 40;
                for (int it = 0; it < 12; it++)
                {
                    float[] q = SagFc_CalcInvTool(m.FuncNumber, ad, x, y, (float)z, pitchDeg, m.WristID, pos);
                    double fc, sd;
                    if (q == null || !SagComp_FcPredict(m, q, out fc, out sd)) return false;
                    double zn = 0.5 * z + 0.5 * fc;
                    bool bDone = Math.Abs(zn - z) < 0.02;
                    z = zn;
                    if (bDone) break;
                }
                float[] qf = SagFc_CalcInvTool(m.FuncNumber, ad, x, y, (float)z, pitchDeg, m.WristID, pos);
                if (qf == null || !SagComp_FcPredict(m, qf, out fcPred, out fcStd)) return false;
                zStart = (float)(fcPred + 20.0);
                zMin = (float)(fcPred - 12.0);
                return true;
            }

            // ────────────────────────────────────────────────────────────
            // 학습
            // ────────────────────────────────────────────────────────────
            /// <summary>데이터셋 CSV 로 첫닿음 모델을 학습해 model.Fc 에 넣는다 (손목 유지 켬 행은 뺀다). 반환 = 성공, report = 요약.</summary>
            public bool SagComp_FitFloorFc(string datasetCsv, CSagComp model, out string report)
            {
                return SagComp_FitFloorFc(datasetCsv, model, false, out report);
            }
            /// <summary>데이터셋 CSV 로 첫닿음 모델을 학습해 model.Fc 에 넣는다.
            /// · 열: fc (첫닿음 FK 높이), q11..q14 또는 q1..q4 (이상 관절각), group (+ session) — 머리줄 이름으로 찾는다.
            ///   kind 열이 있으면 kind=F 행만, hold 열이 있으면 bUseHold 가 아니면 hold=0 행만 (켬·끔은 첫닿음 수준에서 구별되지 않지만 기본은 끔만).
            ///   묶음 = session_group. 옛 머리줄(16칸) 파일에 새 행(kind,fc,fccmdz,hold)을 이어 붙였어도 읽는다.
            /// · 특징: 행마다 관절값 사본에 q 를 넣어 — 지레 L12·L13·L14 (SagComp_JointLevers 와 같은 값), ∂z/∂q ±0.5° 중앙차분 (SagComp_TcpZJac), φ_j = Jz_j·L_j.
            ///   3D 관절값·CalcF 를 건드리지 않으므로 모션·실물 스트리밍 중에 다른 스레드(앱 화면 등)에서 불러도 보내는 명령에 섞이지 않는다.
            /// · 물리 평균: [H; R]β ≈ [y; 0] (H = [1, φ12, φ13, Jz14], R = diag(1/사전 표준편차), 상수항 제외) — 상·하한 있는 계수 셋의
            ///   활성 조합 27가지(자유/하한/상한)를 모두 풀어 범위를 지키는 해 중 비용 최소 (볼록 문제라 전역 해). 공분산 Σ = (HᵀH + RᵀR)⁻¹.
            /// · 잔차 GP: 길이 척도 L12 {0.25, 0.5, 1, 2}, L13 {1, 2, 4}, L14 {0.3} (피치가 섞여 있으면 1 도), 신호 {1, 2, 4}, 잡음 {0.5, 1, 1.5}, 요 1000 —
            ///   묶음 하나씩 빼는 교차검증에서 ±ResidCap 로 자른 예측의 RMSE 가 가장 작은 조합.
            /// · Ref = 학습 FC 평균 (모델에 fcref 가 있으면 그 값 유지). 상·하한·사전·상한·범위·fcon 은 기존 model.Fc 값을 이어받는다.
            /// · 보고: 점 수, β, 교차검증 (물리 평균만 / 잔차 포함), 같은 자리(x·y 3mm, 피치 1°) 다른 측정 사이 4mm 넘는 엇갈림 목록 — 보정을 0 으로 만들지는 않는다.
            /// · 출력 훅이 쓰고 있는 모델이면 새 보정으로 1초에 걸쳐 넘어간다 (보통은 사본에 학습해 저장·불러오기). 모션·실물 스트리밍 중 다른 스레드에서 불러도 된다
            ///   — 새 Fc 를 전환과 같은 lock 안에서 바꿔 끼운다 (SagComp_SwitchLocked).</summary>
            public bool SagComp_FitFloorFc(string datasetCsv, CSagComp model, bool bUseHold, out string report)
            {
                return SagComp_FitFloorFc(datasetCsv, model, bUseHold, null, null, out report);
            }
            /// <summary>교차검증 겹 상한 — 묶음이 이보다 많으면 이름 순으로 이어 SAGFC_MAX_FOLDS 겹으로 묶는다 (같은 측정 회차의 묶음이 한 겹에 모인다).
            /// 측정이 덧붙일수록 묶음이 늘어 시간(초매개변수 216가지 × 겹 × 촐레스키 점 수³)이 묶음 수에 비례해 더 늘던 것을 막는다 (10-02 검토: 365점 112초).
            /// 묶음이 이 값 이하이면 예전과 같은 겹(묶음 하나씩)이라 결과가 비트까지 같다</summary>
            private const int SAGFC_MAX_FOLDS = 10;
            /// <summary>SagComp_FitFloorFc 와 같고 진행·중단을 받는다 — progress 는 초매개변수 교차검증 10% 마다 "진행 학습: …" 한 줄 (null = 없음),
            /// stop 이 true 를 돌려주면 그 자리에서 멈추고 false (model 은 그대로, report = "중단 요청 …"). 블로킹</summary>
            public bool SagComp_FitFloorFc(string datasetCsv, CSagComp model, bool bUseHold, CSim2Real.DLog progress, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (model == null || string.IsNullOrEmpty(datasetCsv)) { report = "모델 또는 데이터 없음"; return false; }
                int[] ids = SagFc_Ids(model);
                if (ids == null) { report = "모델의 posids·wrist 가 맞지 않음"; return false; }
                string[] lines = datasetCsv.Replace("\r", "").Split('\n');
                var hd = new List<string>();
                foreach (string h in lines[0].TrimStart('﻿').Split(',')) hd.Add(h.Trim());
                if (hd.IndexOf("fc") < 0 && hd.Count == 16 && hd.IndexOf("fkz") == 6)
                    hd.AddRange(new string[] { "kind", "fc", "fccmdz", "hold" });         // 옛 머리줄 파일에 새 행을 이어 붙인 경우
                int iFc = hd.IndexOf("fc"), iG = hd.IndexOf("group"), iS = hd.IndexOf("session"), iK = hd.IndexOf("kind"), iH = hd.IndexOf("hold");
                int iX = hd.IndexOf("x"), iY = hd.IndexOf("y"), iP = hd.IndexOf("pitch"), iR = hd.IndexOf("r");
                int[] iQ = { hd.IndexOf("q11"), hd.IndexOf("q12"), hd.IndexOf("q13"), hd.IndexOf("q14") };
                if (Array.IndexOf(iQ, -1) >= 0) iQ = new int[] { hd.IndexOf("q1"), hd.IndexOf("q2"), hd.IndexOf("q3"), hd.IndexOf("q4") };
                if (iFc < 0 || Array.IndexOf(iQ, -1) >= 0) { report = "데이터셋에 fc 또는 q 열이 없음"; return false; }
                int need = Math.Max(iFc, Math.Max(Math.Max(iQ[0], iQ[1]), Math.Max(iQ[2], iQ[3]))) + 1;
                foreach (int ix in new int[] { iK, iH, iG, iS }) need = Math.Max(need, ix + 1);
                Func<string[], int, double> D = (ff, k) =>
                {
                    double v; return (k >= 0 && k < ff.Length && double.TryParse(ff[k].Trim(), NumberStyles.Float, ci, out v)) ? v : double.NaN;
                };

                // ── 행 고르기 ──
                var lq = new List<double[]>(); var ly = new List<double>(); var lg = new List<string>(); var ls = new List<string>();
                var lpx = new List<double>(); var lpy = new List<double>(); var lpp = new List<double>();
                int nKind = 0, nHold = 0, nBad = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Trim().Length == 0) continue;
                    string[] f = lines[i].Split(',');
                    if (f[0].Trim() == hd[0]) continue;                                    // 이어 붙인 머리줄
                    if (f.Length < need) { nBad++; continue; }
                    if (iK >= 0 && f[iK].Trim() != "F") { nKind++; continue; }
                    if (iH >= 0 && !bUseHold && f[iH].Trim() == "1") { nHold++; continue; }
                    double fc = D(f, iFc);
                    double[] q = { D(f, iQ[0]), D(f, iQ[1]), D(f, iQ[2]), D(f, iQ[3]) };
                    bool ok = !double.IsNaN(fc) && !double.IsInfinity(fc);
                    foreach (double v in q) if (double.IsNaN(v) || double.IsInfinity(v)) ok = false;
                    if (!ok) { nBad++; continue; }
                    string sess = iS >= 0 ? f[iS].Trim() : "";
                    lq.Add(q); ly.Add(fc); ls.Add(sess);
                    lg.Add((sess.Length > 0 ? sess + "_" : "") + (iG >= 0 ? f[iG].Trim() : ""));
                    double px = D(f, iX), py = D(f, iY), rr = D(f, iR);
                    if ((double.IsNaN(px) || double.IsNaN(py)) && !double.IsNaN(rr)) { px = rr * Math.Cos(q[0] * Math.PI / 180); py = rr * Math.Sin(q[0] * Math.PI / 180); }
                    double pp = D(f, iP); if (double.IsNaN(pp)) pp = q[1] + q[2] + q[3];
                    lpx.Add(px); lpy.Add(py); lpp.Add(pp);
                }
                int n = lq.Count;
                string skip = string.Format(ci, "{0}{1}{2}", nKind > 0 ? string.Format(ci, ", kind≠F {0}행 제외", nKind) : "",
                    nHold > 0 ? string.Format(ci, ", 손목 유지 켬 {0}행 제외", nHold) : "", nBad > 0 ? string.Format(ci, ", 읽지 못한 {0}행", nBad) : "");
                if (n < 6) { report = string.Format(ci, "첫닿음 모델 학습: 쓸 수 있는 행이 {0}개 — 6개 이상 필요{1}", n, skip); return false; }

                // ── 특징 ──
                var fc0 = new CSagFc();
                CSagFc old = model.Fc;
                if (old != null)
                {
                    if (old.Bounds != null && old.Bounds.Length == 6) fc0.Bounds = (double[])old.Bounds.Clone();
                    if (old.PriorSd != null && old.PriorSd.Length == 3) fc0.PriorSd = (double[])old.PriorSd.Clone();
                    if (old.Env != null && old.Env.Length == 8) fc0.Env = (double[])old.Env.Clone();
                    fc0.ResidCap = old.ResidCap; fc0.DzMax = old.DzMax; fc0.StdLimit = old.StdLimit; fc0.Enabled = old.Enabled;
                    if (old.RefFixed) { fc0.RefFixed = true; fc0.Ref = old.Ref; }
                }
                var H = new double[n][]; var X = new double[n][];
                double l14Min = double.MaxValue, l14Max = double.MinValue;
                double[] adMot = SagFc_MotCopy();                                     // 관절값 사본 — 3D 관절값은 바꾸지 않는다
                for (int i = 0; i < n; i++)
                {
                    double[] ft = SagFc_Feat(model.FuncNumber, ids, SagFc_Q4(lq[i]), adMot);
                    H[i] = new double[] { 1.0, ft[6], ft[7], ft[5] };
                    X[i] = new double[] { ft[0], ft[1], ft[2], lq[i][0] };
                    l14Min = Math.Min(l14Min, ft[2]); l14Max = Math.Max(l14Max, ft[2]);
                }
                double[] y = ly.ToArray();

                // ── 물리 평균 (한 번만 — 잡음 1mm 고정) ──
                double[] beta, cov;
                if (!SagFc_FitMean(H, y, null, fc0.Bounds, fc0.PriorSd, out beta, out cov)) { report = "물리 평균 맞춤 실패 (특이 행렬)" + skip; return false; }
                var res = new double[n];
                for (int i = 0; i < n; i++) res[i] = y[i] - (beta[0] + beta[1] * H[i][1] + beta[2] * H[i][2] + beta[3] * H[i][3]);

                // ── 묶음 ── (교차검증 겹 = 묶음 하나씩, 묶음이 SAGFC_MAX_FOLDS 보다 많으면 이름 순으로 이어 그만큼으로 묶는다)
                var names = new List<string>(); foreach (string g in lg) if (!names.Contains(g)) names.Add(g);
                names.Sort(string.CompareOrdinal);
                int nFolds = Math.Min(names.Count, SAGFC_MAX_FOLDS);
                var foldOf = new Dictionary<string, int>();
                for (int k = 0; k < names.Count; k++) foldOf[names[k]] = (names.Count <= SAGFC_MAX_FOLDS) ? k : (int)((long)k * nFolds / names.Count);
                var foldTr = new List<int[]>(); var foldTe = new List<int[]>();
                for (int fk = 0; fk < nFolds; fk++)
                {
                    var tr = new List<int>(); var te = new List<int>();
                    for (int i = 0; i < n; i++) if (foldOf[lg[i]] == fk) te.Add(i); else tr.Add(i);
                    if (tr.Count < 5 || te.Count == 0) continue;
                    foldTr.Add(tr.ToArray()); foldTe.Add(te.ToArray());
                }

                // 물리 평균만 — 묶음마다 다시 맞춘 교차검증 (비교용)
                double sePhys = 0; int cntPhys = 0;
                for (int fi = 0; fi < foldTr.Count; fi++)
                {
                    double[] bf, cf;
                    if (!SagFc_FitMean(H, y, foldTr[fi], fc0.Bounds, fc0.PriorSd, out bf, out cf)) continue;
                    foreach (int t in foldTe[fi])
                    {
                        double e = bf[0] + bf[1] * H[t][1] + bf[2] * H[t][2] + bf[3] * H[t][3] - y[t];
                        sePhys += e * e; cntPhys++;
                    }
                }
                double cvPhys = cntPhys > 0 ? Math.Sqrt(sePhys / cntPhys) : double.NaN;

                // ── 잔차 GP 초매개변수 (묶음 교차검증, 예측을 ±ResidCap 로 자름) ──
                double[] g12 = { 0.25, 0.5, 1, 2 }, g13 = { 1, 2, 4 }, gsf = { 1, 2, 4 }, gsn = { 0.5, 1, 1.5 };
                double[] g14 = (l14Max - l14Min) > 0.05 ? new double[] { 0.3, 1 } : new double[] { 0.3 };
                double cap = fc0.ResidCap;
                double best = double.MaxValue; double[] bHp = null;
                int nCombo = g12.Length * g13.Length * g14.Length * gsf.Length * gsn.Length, nDone = 0, nTenth = 0;
                foreach (double l12 in g12) foreach (double l13 in g13) foreach (double l14 in g14) foreach (double sf in gsf) foreach (double sn in gsn)
                {
                    if (stop != null && SagFc_StopReq(stop)) { report = "중단 요청 — 첫닿음 모델을 학습하지 않음 (모델 그대로)"; return false; }
                    if (progress != null && (nDone * 10) / nCombo > nTenth)
                    {
                        nTenth = (nDone * 10) / nCombo;
                        try { progress(string.Format(ci, "진행 학습: 교차검증 {0}% (초매개변수 {1}/{2}, {3}겹)", nTenth * 10, nDone, nCombo, foldTr.Count)); } catch { }
                    }
                    nDone++;
                    double se = 0; int cnt = 0; bool bad = false;
                    for (int fi = 0; fi < foldTr.Count && !bad; fi++)
                    {
                        int[] tr = foldTr[fi];
                        var tx = new double[tr.Length][]; var tyv = new double[tr.Length];
                        for (int j = 0; j < tr.Length; j++) { tx[j] = X[tr[j]]; tyv[j] = res[tr[j]]; }
                        var gp = new CSagGp { Ell = new double[] { l12, l13, l14, 1000 }, SigF = sf, SigN = sn, Ref = 0, X = tx, Y = tyv };
                        if (!gp.Solve()) { bad = true; break; }
                        foreach (int t in foldTe[fi])
                        {
                            double mu, sd; gp.Predict(X[t], out mu, out sd);
                            double e = Math.Max(-cap, Math.Min(cap, mu)) - res[t];
                            se += e * e; cnt++;
                        }
                    }
                    if (bad || cnt == 0) continue;
                    double cv = Math.Sqrt(se / cnt);
                    if (cv < best) { best = cv; bHp = new double[] { l12, l13, l14, sf, sn }; }
                }
                bool bCv = bHp != null;
                if (!bCv) bHp = (old != null && old.Ell != null && old.Ell.Length >= 3) ? new double[] { old.Ell[0], old.Ell[1], old.Ell[2], old.SigF, old.SigN }
                                                                                        : new double[] { 0.25, 1, 0.3, 4, 0.5 };
                var gpF = new CSagGp { Ell = new double[] { bHp[0], bHp[1], bHp[2], 1000 }, SigF = bHp[3], SigN = bHp[4], Ref = 0, X = X, Y = res };
                if (!gpF.Solve()) { report = "잔차 GP 촐레스키 실패" + skip; return false; }

                // ── 모델 ──
                fc0.Beta = beta; fc0.Cov = cov;
                fc0.Ell = new double[] { bHp[0], bHp[1], bHp[2], 1000 }; fc0.SigF = bHp[3]; fc0.SigN = bHp[4];
                if (!fc0.RefFixed) { double s = 0; foreach (double v in y) s += v; fc0.Ref = s / n; }
                fc0.Q = lq.ToArray(); fc0.Y = y; fc0.Groups = lg.ToArray();
                fc0.CvRmse = bCv ? best : double.NaN; fc0.CvPhysRmse = cvPhys;
                fc0.SetPrepared(gpF);                                                  // 학습 특징 그대로 — 다시 불러와 준비해도 같은 값

                // ── 보고 ──
                var sb = new StringBuilder();
                Func<int, string> B = j => beta[j] <= fc0.Bounds[2 * (j - 1)] + 1e-9 ? " [하한]" : (beta[j] >= fc0.Bounds[2 * (j - 1) + 1] - 1e-9 ? " [상한]" : "");
                sb.AppendLine(string.Format(ci, "첫닿음 모델 학습: 점 {0} (묶음 {1}){2}{3}", n, names.Count, skip,
                    names.Count > SAGFC_MAX_FOLDS ? string.Format(ci, " — 교차검증은 묶음을 이름 순으로 이어 {0}겹", nFolds) : ""));
                sb.AppendLine(string.Format(ci, "  물리 평균 β: c0 {0:F3}mm, k12 {1:F4}{2}, k13 {3:F4}{4}, b14 {5:+0.000;-0.000}°{6}  (사전 표준편차 {7}/{8}/{9}, 잡음 1mm)",
                    beta[0], beta[1], B(1), beta[2], B(2), beta[3], B(3), fc0.PriorSd[0], fc0.PriorSd[1], fc0.PriorSd[2]));
                sb.AppendLine(string.Format(ci, "  묶음 교차검증 RMSE: 물리 평균만 {0:F3}mm, 잔차 포함 {1}  — 잔차 GP 길이 척도 L12 {2:G4} L13 {3:G4} L14 {4:G4}, 신호 {5:G4} 잡음 {6:G4}mm, 잔차 ±{7:G3}mm 자름",
                    cvPhys, bCv ? best.ToString("F3", ci) + "mm" : "계산 불가(묶음 부족 — 기존·기본 초매개변수)", bHp[0], bHp[1], bHp[2], bHp[3], bHp[4], cap));
                sb.AppendLine(string.Format(ci, "  기준 Ref {0:F3}mm ({1}), 출력 |dz| ≤ {2:G3}mm, 작업 범위 밖에서만 감쇠", fc0.Ref, fc0.RefFixed ? "fcref 고정" : "학습 FC 평균", fc0.DzMax));
                var conf = new List<string>();
                for (int i = 0; i < n; i++)
                    for (int j = i + 1; j < n; j++)
                        if (ls[i] != ls[j] && !double.IsNaN(lpx[i]) && !double.IsNaN(lpx[j]) && Math.Abs(lpx[i] - lpx[j]) <= 3 && Math.Abs(lpy[i] - lpy[j]) <= 3
                            && Math.Abs(lpp[i] - lpp[j]) <= 1 && Math.Abs(y[i] - y[j]) > 4)
                            conf.Add(string.Format(ci, "    ({0:F0}, {1:F0}) 피치 {2:F0}: {3} {4:F2} / {5} {6:F2} mm (차 {7:F1})", lpx[i], lpy[i], lpp[i], lg[i], y[i], lg[j], y[j], Math.Abs(y[i] - y[j])));
                if (conf.Count == 0) sb.Append("  같은 자리 다른 측정 사이 4mm 넘는 엇갈림: 없음");
                else { sb.AppendLine(string.Format(ci, "  같은 자리 다른 측정 사이 4mm 넘는 엇갈림 {0}쌍 (보정은 그대로 — 재측정 권장):", conf.Count)); sb.Append(string.Join(Environment.NewLine, conf.ToArray())); }
                fc0.Report = sb.ToString();
                // 쓰고 있는 모델이면 새 보정으로 1초에 걸쳐 넘어간다 (SagComp_Enable 과 같은 방식) — Fc 를 전환과 같은 lock 안에서 바꿔 끼운다.
                //   출력 틱은 틱 시작 때 모델·Fc·전환 기준을 한 번에 읽으므로, 어느 틱도 새 Fc 를 이전 보정 그대로(w=1) 쓰지 않는다 (SagComp_SwitchLocked)
                lock (m_dicSagDir)
                {
                    model.Fc = fc0;
                    if (ReferenceEquals(model, m_SagComp)) SagComp_SwitchLocked(model);
                }
                report = fc0.Report;
                return true;
            }

            // 중단 요청인가 (대리자가 던지면 계속한다)
            private static bool SagFc_StopReq(CSim2Real.DIsStopped stop)
            {
                try { return stop(); } catch { return false; }
            }
            // 물리 평균 맞춤 — [H; R]β ≈ [y; 0], R = diag(1/사전 표준편차) (상수항 제외, 잡음 1mm), k12·k13·b14 상·하한.
            //   rows = 쓸 행 (null = 전부). 계수 셋이 각각 자유/하한/상한인 27가지를 모두 풀고(각 4×4 이하 정규방정식), 범위를 지키는 해 중 비용 최소.
            //   볼록 문제라 이것이 제약 최소제곱의 전역 해다. cov = (HᵀH + RᵀR)⁻¹ (행 우선 16개).
            private static bool SagFc_FitMean(double[][] H, double[] y, int[] rows, double[] bnd, double[] prior, out double[] beta, out double[] cov)
            {
                beta = null; cov = null;
                var A = new double[4, 4]; var b = new double[4];
                int cnt = rows == null ? H.Length : rows.Length;
                if (cnt < 4) return false;
                for (int ii = 0; ii < cnt; ii++)
                {
                    int i = rows == null ? ii : rows[ii];
                    for (int p = 0; p < 4; p++) { b[p] += H[i][p] * y[i]; for (int q = 0; q < 4; q++) A[p, q] += H[i][p] * H[i][q]; }
                }
                for (int j = 0; j < 3; j++) if (prior[j] > 0 && !double.IsInfinity(prior[j])) A[j + 1, j + 1] += 1.0 / (prior[j] * prior[j]);
                double bestCost = double.MaxValue;
                for (int st = 0; st < 27; st++)
                {
                    int[] s = { st % 3, (st / 3) % 3, st / 9 };                        // k12, k13, b14: 0 = 자유, 1 = 하한, 2 = 상한
                    var bt = new double[4]; var free = new List<int> { 0 }; var fix = new List<int>();
                    for (int j = 0; j < 3; j++)
                    {
                        if (s[j] == 0) free.Add(j + 1);
                        else { bt[j + 1] = s[j] == 1 ? bnd[2 * j] : bnd[2 * j + 1]; fix.Add(j + 1); }
                    }
                    int nf = free.Count;
                    var Af = new double[nf, nf]; var bf = new double[nf];
                    for (int p = 0; p < nf; p++)
                    {
                        bf[p] = b[free[p]];
                        foreach (int c in fix) bf[p] -= A[free[p], c] * bt[c];
                        for (int q = 0; q < nf; q++) Af[p, q] = A[free[p], free[q]];
                    }
                    double[] xf = SagFc_Solve(Af, bf);
                    if (xf == null) continue;
                    for (int p = 0; p < nf; p++) bt[free[p]] = xf[p];
                    bool ok = true;
                    for (int j = 0; j < 3; j++) if (bt[j + 1] < bnd[2 * j] - 1e-12 || bt[j + 1] > bnd[2 * j + 1] + 1e-12) ok = false;
                    if (!ok) continue;
                    double cost = 0;                                                    // βᵀAβ − 2bᵀβ (+ 상수)
                    for (int p = 0; p < 4; p++) { cost -= 2 * b[p] * bt[p]; for (int q = 0; q < 4; q++) cost += bt[p] * A[p, q] * bt[q]; }
                    if (cost < bestCost) { bestCost = cost; beta = bt; }
                }
                if (beta == null) return false;
                double[,] inv = SagFc_Inverse(A);
                if (inv != null) { cov = new double[16]; for (int p = 0; p < 4; p++) for (int q = 0; q < 4; q++) cov[p * 4 + q] = inv[p, q]; }
                return true;
            }
            // 작은 정규방정식 — 부분 피벗 가우스 소거. 특이하면 null
            private static double[] SagFc_Solve(double[,] A0, double[] b0)
            {
                int n = b0.Length;
                var A = (double[,])A0.Clone(); var b = (double[])b0.Clone();
                for (int c = 0; c < n; c++)
                {
                    int p = c; for (int r = c + 1; r < n; r++) if (Math.Abs(A[r, c]) > Math.Abs(A[p, c])) p = r;
                    if (Math.Abs(A[p, c]) < 1e-12) return null;
                    if (p != c) { for (int j = 0; j < n; j++) { double t = A[c, j]; A[c, j] = A[p, j]; A[p, j] = t; } double tb = b[c]; b[c] = b[p]; b[p] = tb; }
                    for (int r = c + 1; r < n; r++) { double m = A[r, c] / A[c, c]; for (int j = c; j < n; j++) A[r, j] -= m * A[c, j]; b[r] -= m * b[c]; }
                }
                var x = new double[n];
                for (int r = n - 1; r >= 0; r--) { double s = b[r]; for (int j = r + 1; j < n; j++) s -= A[r, j] * x[j]; x[r] = s / A[r, r]; }
                return x;
            }
            private static double[,] SagFc_Inverse(double[,] A)
            {
                int n = A.GetLength(0);
                var inv = new double[n, n];
                for (int c = 0; c < n; c++)
                {
                    var e = new double[n]; e[c] = 1;
                    double[] col = SagFc_Solve(A, e);
                    if (col == null) return null;
                    for (int r = 0; r < n; r++) inv[r, c] = col[r];
                }
                return inv;
            }

            // ────────────────────────────────────────────────────────────
            // 첫닿음 추출 — 궤적 재분석 (SagComp_FloorContactsFromTrace, cfg.FirstTouch). 10-02 판정의 추출 규칙.
            //   F0: 저장된 꺾임(SagComp_PressKnee, 하강 전체)이 아래 검사를 모두 통과하면 그대로 첫닿음
            //       · 단단한 정지가 아님: 꺾임점 공구 피치 오차 − 무장 시작 피치 오차 ≤ 2.0°  (피치 오차 = a12+a13+a14 − q12−q13−q14)
            //         관절 열이 없는 옛 궤적: 꺾임이 무장 시작 20mm 아래이고 위 10mm 어깨 부하 |중앙값| < 1.5% 면 단단한 정지
            //       · 접촉 신호: 위 8mm 어깨 부하 중앙값 ≤ −2% 이고 아래 4~8mm 중앙값 ÷ 위 ≤ 0.75, 또는 손목 잔차 d14 (= a14 − q14) 가
            //         6mm 안에서 0.5° 이상 증가 (가까운 반경은 어깨 부하가 ~0 이라 d14 로 본다)
            //       · 공중 기울기: 꺾임 위 (k+0.5, k+6] 의 FK/명령 기울기 0.5~1.6 (먼 쪽은 공중에서도 0.6~0.85)
            //       · 출발 따라잡기가 아님: 무장 시작보다 8mm 이상 아래 — 안전 정지로 끝난 하강은 15mm (10-02 r=240 끔: 출발 벌어짐 7.6mm 를
            //         기울기 0.56 으로 따라잡는 공중 구간을 꺾임으로 잡았다)
            //   F1: 아니면 무장 시작 8mm(안전 정지 15mm) 아래부터 0.25mm 씩 내려가며 첫 꺾임 — 단단한 정지였으면 그 꺾임 +5mm 위쪽만.
            //       경첩 창 [k−6, k+10] (위쪽 기울기 1 고정): 아래 기울기 s ≤ 0.70, (1−s)·6 ≥ 1.5, 잔차 ≤ 0.9mm, 공중 기울기 0.75~1.7,
            //       피치 오차 증가 ≤ 1.5° (무장 시작 대비)·≤ 1.0° (6mm 위 대비), 접촉 신호.
            //       값 = 하강을 k−12 에서 자른 표본의 SagComp_PressKnee FK (먼 쪽 행과 같은 척도 — 보정 상수가 없다)
            //       데이터셋 kind: 저장된 꺾임이 단단한 정지였으면 H, 아니었으면 X — X 인데 fc 가 차 있는 행은 F1 이 위에서 찾은 첫닿음으로
            //       교차 확인용일 뿐 학습에는 쓰지 않는다 (학습 = kind F 행만. 판정 참조와 비교하는 시험이 이 표시를 쓰므로 표시는 바꾸지 않는다)
            //   둘 다 아니면 단단한 정지였으면 C (검열 — 처음부터 닿아 있었다), 아니면 X (저장된 꺾임을 쓸 수 없음, fc 빈칸).
            //   기존 두 추출기 참조 86개 중 80개가 3mm 안 (평균 −0.55, 표준편차 1.21). 놓치는 것은 첫닿음 1~3mm 위에서 시작한 하강.
            // ────────────────────────────────────────────────────────────
            private const double SAG_FT_START_MM = 8.0, SAG_FT_START_SAFETY_MM = 15.0, SAG_FT_STEP_MM = 0.25;
            private const int SAG_FT_MIN_ARMED = 15;

            // 하강 하나 (무장 뒤 표본, 시간 순)
            private sealed class CSagFtDesc
            {
                public double[] Cz, Fz, Ld, Pe, D14;                                    // Pe·D14 = null 이면 관절 열 없음
                public double Top = double.MinValue, Bot = double.MaxValue;
                public CSagFtDesc(List<double> cz, List<double> fz, List<double> ld, List<double> pe, List<double> d14)
                {
                    Cz = cz.ToArray(); Fz = fz.ToArray(); Ld = ld.ToArray();
                    Pe = pe == null ? null : pe.ToArray(); D14 = d14 == null ? null : d14.ToArray();
                    foreach (double v in Cz) { if (v > Top) Top = v; if (v < Bot) Bot = v; }
                }
                // 명령 높이 z 에 가장 가까운 표본 (같으면 앞의 것)
                public int At(double z)
                {
                    int best = 0; double bd = double.MaxValue;
                    for (int i = 0; i < Cz.Length; i++) { double d = Math.Abs(Cz[i] - z); if (d < bd) { bd = d; best = i; } }
                    return best;
                }
                // 꺾임 위 (k+0.5, k+6] 의 FK/명령 기울기 (표본 4개 미만이면 NaN)
                public double Sa(double k)
                {
                    double sx = 0, sy = 0; int n = 0;
                    for (int i = 0; i < Cz.Length; i++) if (Cz[i] > k + 0.5 && Cz[i] <= k + 6.0) { sx += Cz[i]; sy += Fz[i]; n++; }
                    if (n < 4) return double.NaN;
                    double mx = sx / n, my = sy / n, sxx = 0, sxy = 0;
                    for (int i = 0; i < Cz.Length; i++) if (Cz[i] > k + 0.5 && Cz[i] <= k + 6.0) { sxx += (Cz[i] - mx) * (Cz[i] - mx); sxy += (Cz[i] - mx) * (Fz[i] - my); }
                    return sxx > 0 ? sxy / sxx : double.NaN;
                }
                // 접촉 신호 — "load" (어깨 부하가 풀림) / "d14" (손목 잔차 증가) / ""
                public string Contact(double k)
                {
                    var ab = new List<double>(); var be = new List<double>();
                    for (int i = 0; i < Cz.Length; i++)
                    {
                        if (Cz[i] > k && Cz[i] <= k + 8) ab.Add(Ld[i]);
                        if (Cz[i] < k - 4 && Cz[i] >= k - 8) be.Add(Ld[i]);
                    }
                    double lf = SagFc_Median(ab), la = SagFc_Median(be);
                    if (SagFc_Finite(lf) && SagFc_Finite(la) && lf <= -2.0 && la / lf <= 0.75) return "load";
                    if (D14 != null && D14[At(k - 6.0)] - D14[At(k)] >= 0.5) return "d14";
                    return "";
                }
                // 단단한 정지 — 피치 오차가 무장 시작보다 2° 넘게 커짐 (관절 열이 없으면: 무장 시작 20mm 아래 + 위 10mm 부하가 ~0)
                public bool Hard(double k)
                {
                    if (Pe != null) return Pe[At(k)] - Pe[At(Top)] > 2.0;
                    var ab = new List<double>();
                    for (int i = 0; i < Cz.Length; i++) if (Cz[i] > k && Cz[i] <= k + 10) ab.Add(Ld[i]);
                    return k <= Top - 20 && ab.Count > 0 && Math.Abs(SagFc_Median(ab)) < 1.5;
                }
            }
            private static bool SagFc_Finite(double v) { return !double.IsNaN(v) && !double.IsInfinity(v); }
            private static double SagFc_Median(List<double> v)
            {
                if (v.Count == 0) return double.NaN;
                foreach (double t in v) if (double.IsNaN(t)) return double.NaN;
                double[] a = v.ToArray(); Array.Sort(a);
                int n = a.Length;
                return (n % 2 == 1) ? a[n / 2] : (a[n / 2 - 1] + a[n / 2]) / 2.0;
            }
            // SagComp_PressKnee 와 같은 꺾임 맞춤 — 꺾임점(명령 k, FK fk)을 반올림 없이 double 로
            private static bool SagFc_PressKnee(List<double> dX, List<double> dY, CFloorProbeCfg cfg, out double k, out double fk)
            {
                k = fk = double.NaN;
                if (dX == null || dX.Count < 9) return false;
                double czEnd = double.MaxValue, czAll = double.MinValue;
                foreach (double v in dX) { czEnd = Math.Min(czEnd, v); czAll = Math.Max(czAll, v); }
                bool ok = false;
                for (int nw = 0; nw < 3 && !ok; nw++)
                {
                    double win = cfg.FitWindowMm + 6 * nw;
                    if (nw > 0 && czEnd + win - 6 >= czAll) break;
                    var wX = new List<double>(); var wY = new List<double>();
                    for (int i = 0; i < dX.Count; i++) if (dX[i] <= czEnd + win) { wX.Add(dX[i]); wY.Add(dY[i]); }
                    double kk, y0, sL, rms;
                    if (!SagComp_FitKnee1(wX.ToArray(), wY.ToArray(), 4, out kk, out y0, out sL, out rms)) continue;
                    double czTop = double.MinValue; foreach (double v in wX) czTop = Math.Max(czTop, v);
                    double gain = (1 - sL) * (kk - czEnd);
                    k = kk; fk = y0;
                    ok = sL < 0.9 && gain >= cfg.KneeMinGainMm && (kk - czEnd) >= 1.5 && (czTop - kk) >= 2;
                }
                return ok;
            }
            // 경첩 맞춤 (위쪽 기울기 1 고정) — fz = a + cz (cz ≥ k), a + k + s·(cz − k) (cz < k). (a, s) 최소제곱, rms = 잔차
            private static void SagFc_Hinge(double[] cz, double[] fz, List<int> idx, double k, out double a, out double s, out double rms)
            {
                double n = 0, sm = 0, smm = 0, st = 0, smt = 0;
                foreach (int i in idx)
                {
                    bool up = cz[i] >= k;
                    double m = up ? 0 : cz[i] - k, t = fz[i] - (up ? cz[i] : k);
                    n += 1; sm += m; smm += m * m; st += t; smt += m * t;
                }
                double det = n * smm - sm * sm;
                if (Math.Abs(det) < 1e-12) { a = s = rms = double.NaN; return; }
                a = (st * smm - smt * sm) / det; s = (n * smt - sm * st) / det;
                double se = 0;
                foreach (int i in idx)
                {
                    bool up = cz[i] >= k;
                    double m = up ? 0 : cz[i] - k, t = fz[i] - (up ? cz[i] : k), e = t - a - s * m;
                    se += e * e;
                }
                rms = Math.Sqrt(se / n);
            }
            private static void SagFc_SetTouch(CFloorContact c, CSagFtDesc D, List<float[]> dQ, List<float[]> dA, string stage, string kind, double kCmd, double fk, string why)
            {
                c.FcStage = stage; c.Kind = kind; c.FcCmdZ = (float)kCmd; c.FcZ = (float)fk; c.FcWhy = why;
                int i = D.At(kCmd);                                                     // 첫닿음 명령 높이에 가장 가까운 표본 = 첫닿음 자세
                if (dQ != null && dA != null && i < dQ.Count && i < dA.Count && dA[i] != null && !float.IsNaN(dA[i][0])) { c.FcJointCmd = dQ[i]; c.FcJointAct = dA[i]; }
            }

            // 하강 하나의 첫닿음 판정 — dX·dY·dL = 무장 뒤 명령 높이·FK 높이·어깨 부하(%), dPe·dW = 공구 피치 오차·손목 잔차 (관절 열이 없으면 NaN),
            //   dQ·dA = 이상·실측 관절각, bSafety = 안전 정지로 끝난 하강. c 의 Kind·FcStage·FcZ·FcCmdZ·FcJoint*·KneeHard 를 채운다.
            private static void SagFc_FirstTouch(CFloorContact c, List<double> dX, List<double> dY, List<double> dL, List<double> dPe, List<double> dW,
                                                 List<float[]> dQ, List<float[]> dA, bool bSafety, CFloorProbeCfg cfg)
            {
                var ci = CultureInfo.InvariantCulture;
                c.ArmedN = dX.Count; c.SafetyStop = bSafety; c.KneeHard = false;
                c.FcStage = "X"; c.Kind = "X"; c.FcZ = c.FcCmdZ = float.NaN; c.FcJointCmd = c.FcJointAct = null;
                if (dX.Count < SAG_FT_MIN_ARMED || dY.Count != dX.Count || dL.Count != dX.Count) { c.FcWhy = "무장 뒤 표본 15개 미만"; return; }
                bool hasJ = dA != null && dA.Count == dX.Count && dA[0] != null && dA[0].Length > 1 && !float.IsNaN(dA[0][1]) && dPe.Count == dX.Count && dW.Count == dX.Count;
                var D = new CSagFtDesc(dX, dY, dL, hasJ ? dPe : null, hasJ ? dW : null);
                double startMm = bSafety ? SAG_FT_START_SAFETY_MM : SAG_FT_START_MM;
                double kd, fkd;
                bool pk = SagFc_PressKnee(dX, dY, cfg, out kd, out fkd);
                bool hard = false; double lo = double.NegativeInfinity; string why = "저장된 꺾임 없음";
                if (pk)
                {
                    hard = D.Hard(kd);
                    double sa = D.Sa(kd); string ct = D.Contact(kd);
                    if (!hard && ct.Length > 0 && SagFc_Finite(sa) && sa >= 0.5 && sa <= 1.6 && D.Top - kd >= startMm)
                    {
                        SagFc_SetTouch(c, D, dQ, dA, "F0", "F", kd, fkd, "저장된 꺾임 (" + ct + ")");
                        return;
                    }
                    why = hard ? "단단한 정지" : (D.Top - kd < startMm ? (bSafety ? "출발 따라잡기 (안전 정지 — 15mm)" : "출발 따라잡기")
                                                                      : (ct.Length == 0 ? "접촉 신호 없음" : string.Format(ci, "공중 기울기 {0:F2}", sa)));
                    if (hard) lo = kd + 5.0;
                }
                c.KneeHard = hard;
                // 위에서부터 0.25mm 씩 — 후보 k = start + i·step (개수 ceil((stop − start)/step), 끝 stop 은 빼고)
                double start = D.Top - startMm, stop = Math.Max(D.Bot + 6.0, lo), step = -SAG_FT_STEP_MM;
                double len = Math.Ceiling((stop - start) / step);
                int nk = len > 0 ? (int)len : 0;
                double delta = (start + step) - start;
                var w = new List<int>();
                for (int ik = 0; ik < nk; ik++)
                {
                    double k = ik == 0 ? start : (ik == 1 ? start + step : start + ik * delta);
                    w.Clear(); int nUp = 0, nDn = 0;
                    for (int j = 0; j < D.Cz.Length; j++)
                        if (D.Cz[j] >= k - 6.0 && D.Cz[j] <= k + 10.0) { w.Add(j); if (D.Cz[j] >= k) nUp++; else nDn++; }
                    if (nUp < 5 || nDn < 5) continue;
                    double a, s, rms; SagFc_Hinge(D.Cz, D.Fz, w, k, out a, out s, out rms);
                    if (!(s <= 0.70 && (1 - s) * 6.0 >= 1.5 && rms <= 0.9)) continue;
                    double sa = D.Sa(k);
                    if (!(SagFc_Finite(sa) && sa >= 0.75 && sa <= 1.7)) continue;
                    if (D.Pe != null && (D.Pe[D.At(k)] - D.Pe[D.At(D.Top)] > 1.5 || D.Pe[D.At(k)] - D.Pe[D.At(Math.Min(k + 6.0, D.Top))] > 1.0)) continue;
                    string ct = D.Contact(k);
                    if (ct.Length == 0) continue;
                    var cx = new List<double>(); var cy = new List<double>();                // 하강을 k − 12 에서 자른다 (처음으로 그 아래로 간 표본부터 버림)
                    for (int j = 0; j < D.Cz.Length; j++) { if (D.Cz[j] < k - 12.0) break; cx.Add(D.Cz[j]); cy.Add(D.Fz[j]); }
                    double k2, fk2;
                    bool good = SagFc_PressKnee(cx, cy, cfg, out k2, out fk2);
                    // kind: 단단한 정지 위에서 찾았으면 H. 아니면 X 그대로 두되 FcZ 는 채운다 — 저장된 꺾임은 못 쓰고, 이 첫닿음은 교차 확인용 (학습은 F 만)
                    SagFc_SetTouch(c, D, dQ, dA, "F1", hard ? "H" : "X", good ? k2 : k, good ? fk2 : a + k, why + " → 위를 다시 훑음 (" + ct + ")");
                    return;
                }
                c.FcStage = hard ? "C" : "X"; c.Kind = hard ? "C" : "X"; c.FcWhy = why + " → 없음";
            }
        }
    }
}
