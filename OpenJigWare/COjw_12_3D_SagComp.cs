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
// ★데드밴드 넘기 (10-16, CSagComp.SigmaTakeDeg — sigmatake=, 0 이면 예전): 관절이 반전하면 이상 관절이 0.1° 가는 사이(100ms 보다 빠르지 않게)
//   데드밴드 항 h·σ 를 넘긴다. 예전 램프(h 폭)는 반전 뒤 실물이 이상 관절 h 만큼 멈춰 있어 사각형 먼 변(가운데에서 12·13·14 가 느리게 반전)에서
//   끝이 ±4.7mm 꿀렁였다 (놀이 모델 예측). 쌍둥이는 데드밴드를 보낸 명령의 놀이(폭 h)로 예측한다 (SagTwin_Predict).
// ★예측 보정 (10-16, CSagComp.Predictive — 기본 켬, predictive=0 이면 끔): 출력 단의 1차 보정을 처짐 쌍둥이 예측으로 다듬어
//   쌍둥이(실물이 있을 자세)의 끝 = 목표, 공구 = 3D 의 공구 각이 되게 한다 (SagComp_PredRefine — 고정점 2~4번, 0.05mm·0.05° 에서 멈춤).
//   10-16 격자 (r120~260·요 0/±30·z 바닥+5~110, 보정 켬, 쌍둥이 끝 − 3D 끝): 최대 7.1 → 0.04mm, 평균 0.78 → 0.01mm, 공구 각 최대 2.7 → 0.01°.
//   출력 틱 비용 0.38 → 0.77ms (최대 약 1.0ms, 대개 평가 2번). 측정·실습 학습의 바닥 데이터 모으기는 1차 보정으로 한다 (옛 회차와 같은 조건).
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
                public float DownSumDeg = 90f;                     // 이 모델을 만든 로봇 정의의 수직 관절 합 (posids[1]+posids[2]+wrist, 파일 downsum= — 없으면 90 = OMX)
                public string KinSig = "";                         // 이 모델을 만든 기구학 표시 (계획 기구학의 그 수식 DH, 파일 kinsig= — 빈 값 = 내장 OMX). 쓰기·대조는 COjw_12_3D_SagRun.cs
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
                public string Note = null;                         // 저장 때 머리 주석(# 줄)으로 — 만든 과정 기록 (학습 시각·바탕 파일·데이터셋 등). 읽을 때는 무시한다
                public string MotorMap = null;                     // 학습 때 모터 매핑 키 (축:방향:영점:틱/회전:각/회전|…) — 파일 motormap= 줄, 없으면 null (= 기본 매핑으로 본다)
                public string SourcePath = null;                   // 읽은 파일 (Load 가 채운다 — 전체 경로, 메모리에서 만든 모델은 null). 저장하지 않는다.
                                                                   //   측정 계획이 '계획을 만든 첫닿음 모델' 로 적어 둔다 (측정 때 그 모델로 창을 다시 확인)

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
                public float MaxAbsJointDeg = 6f;                  // 관절 보정량 안전 상한 (deg) — 파일 maxjoint= (적힌 값을 그대로 쓴다)
                // 학습([1. 학습]·⑨ 처짐보상)이 새로 만드는 관절 모델의 보정 상한 (deg, 사용자 결정 10-16) — 예전에는 위 기본값 6 이 들어가
                //   손목 공구 기울기 τ (6.5~8.5°)·먼 자리 바닥 항이 잘려 쌍둥이 공구가 0.3~4° 기울었다. 결과 모델은 바탕 관절 모델의 값을 이어받는다
                public const float LEARN_MAX_JOINT_DEG = 12f;
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
                //   10-16 부터는 SigmaTakeDeg = 0 (예전 방식) 일 때만 쓴다.
                public float SigmaRamp = 1.0f;
                // 데드밴드 넘기 (10-16, sigmatake=) — 관절이 방향을 바꾸면 이상 관절이 이만큼(deg) 움직이는 사이 σ 가 −1↔+1 을 넘어간다
                //   (보낸 명령은 그동안 데드밴드 2h 를 건너므로 실물이 반전 자리에서 거의 멈추지 않는다). 넘어가는 시간은 SAG_SIGMA_FLIP_MS 보다 짧지 않다.
                //   10-16 draw_square 먼 변 (x=240, 모서리와 가운데에서 12·13·14 가 반전 — 가운데는 아주 느리게): 예전 램프(h 폭)는 반전 뒤 이상 관절이 h 를 갈 때까지
                //   실물이 멈춰 놀이 모델 예측 끝 높이가 −4.7~+4.7mm (y=−35·+35 근처), 0.1° 에서 −0.4~+0.4mm. 보낸 명령 한 틱 최대 변화 12/13/14 1.40/2.43/1.11 → 1.36/1.83/0.80°.
                //   같은 날 실물 (먼 변 공중 6초): 13번은 가운데 반전 뒤 명령이 3.0~3.5° 움직여야 따라 나섰다 — 학습된 2h (2.15°) 보다 넓어 그만큼은 남는다
                //   (먼 자리 데드밴드를 다시 재거나 서보 게인으로 줄일 몫).
                //   0 = 예전 방식 (SigmaRamp × h 램프, 쌍둥이도 예전 식). 키가 없으면 SIGMA_TAKE_DEF
                public const float SIGMA_TAKE_DEF = 0.1f;
                public float SigmaTakeDeg = SIGMA_TAKE_DEF;
                public bool SigmaTakeKey = false;                    // 파일에 sigmatake 키가 있었다 (다시 저장할 때 줄을 남긴다)
                // 외부 휨 배율 κ (09-28 바닥 접촉 캘리브레이션) — 엔코더로 안 보이는 링크·브래킷 휨도 중력 부하에 비례한다고 보고,
                //   **적용할 때만** 관절 보정의 중력 몫을 (1+κ) 배로 한다. c·h 는 엔코더 데이터로 맞춘 그대로(학습·평가에는 κ 없음).
                //   바닥(수평, 외부 기준면)에 그리퍼 끝이 닿는 명령 높이가 반경마다 같아지도록 SagComp_FitFlex 가 정한다.
                public double FlexGain = 0;
                // 예측 보정 (10-16) — 출력 단이 1차 보정(이상 자세에서 잰 지레·바닥 dz·공구 기울기)을 처짐 쌍둥이 예측으로 다듬는다:
                //   예측(보낸 명령) = 이상 자세 — 쌍둥이 끝 = 목표, 쌍둥이 공구 = 이상 자세의 공구 각 (SagComp_PredRefine). 1차 보정에서 시작하는 고정점 반복
                //   (평가 최대 4번, 끝 0.05mm·공구 0.05° 안이면 끝). 끄면(predictive=0) 1차 보정 그대로 — 예전과 비트 단위로 같다.
                //   키가 없으면 켬. 측정·실습 학습의 바닥 데이터 모으기는 1차 보정으로 한다 (옛 회차 데이터셋의 첫닿음 명령 높이와 같은 조건 — SagPrac_LearnJob·SagRun 측정)
                public bool Predictive = true;                       // predictive= (0 = 끔)
                public bool PredictiveKey = false;                   // 파일에 predictive 키가 있었다 (다시 저장할 때 줄을 남긴다)
                // 손목 공구 기울기 보정 (10-15) — 엔코더 밖 기울기 (영점·펜 장착·브래킷 등): 엔코더가 '바닥으로 수직' 이라 볼 때 실물 공구가 τ 만큼 기운다
                //   (+ = 공구 끝이 베이스 쪽).  τ(q) = c + b12·(g12(q) − g12ref) + b13·(g13(q) − g13ref),  g = −L (중력 지레 — 출력 단이 계산하는 것)
                //   적용 = 출력 단에서 손목 명령 += −τ(이상 자세) — 관절 보정과 같은 단계, 같은 MaxAbsJointDeg 자르기·1초 전환 안 (SagComp_JointOutModel).
                //   관절 모델·바닥 모델이 없어도 쓴다 (기울기만 있는 모델). 꺾는 손목 없음(WristID −1)이면 해당 없음 (쓰지 않는다).
                //   수직 눈 확인 [수직임] 이 c (먼 자리도 재면 b12) 를 배워 적용 중 모델에 넣는다 (SagPrac_TiltApply). 키가 없으면 끔 — 옛 파일은 그대로 읽고 그대로 쓴다.
                //   기계 성질이라 다시 학습·적용해도 넘겨받는다 (CopyWristTiltFrom — SagRun_Learn·SagRun_Activate, JointOnlyCopy 는 그대로 가진다)
                public bool WristTiltOn = false;                     // wtilton=1
                public double WristTiltC = 0;                        // wtiltc= (deg)
                public double[] WristTiltB = null;                   // wtilt=b12,b13 (deg / (mm/deg)) — null = 상수만
                public double[] WristTiltRef = null;                 // wtiltref=g12ref,g13ref (mm/deg)
                public bool WristTiltKeys = false;                   // 파일에 wtilt* 키가 있었다 (없을 때만 넘겨받는다 — 키로 끈 것은 그대로)
                /// <summary>손목 공구 기울기 보정이 켜져 있고 쓸 수 있다 (꺾는 손목이 있다)</summary>
                public bool HasWristTilt { get { return WristTiltOn && WristID >= 0; } }
                /// <summary>τ (deg) — 중력 지레 g12 = −L12, g13 = −L13 (mm/deg) 에서. 자세 항이 없으면 c</summary>
                public double WristTiltAt(double g12, double g13)
                {
                    double t = WristTiltC;
                    if (WristTiltB != null && WristTiltB.Length >= 2)
                    {
                        double r12 = (WristTiltRef != null && WristTiltRef.Length >= 1) ? WristTiltRef[0] : 0, r13 = (WristTiltRef != null && WristTiltRef.Length >= 2) ? WristTiltRef[1] : 0;
                        t += WristTiltB[0] * (g12 - r12) + WristTiltB[1] * (g13 - r13);
                    }
                    return t;
                }
                /// <summary>다른 모델의 손목 공구 기울기 보정을 넘겨받는다 (기계 성질 — 관절 모델을 다시 학습해도 그대로). other 에 키가 없으면 false (그대로)</summary>
                public bool CopyWristTiltFrom(CSagComp other)
                {
                    if (other == null || !other.WristTiltKeys) return false;
                    WristTiltOn = other.WristTiltOn; WristTiltC = other.WristTiltC;
                    WristTiltB = other.WristTiltB == null ? null : (double[])other.WristTiltB.Clone();
                    WristTiltRef = other.WristTiltRef == null ? null : (double[])other.WristTiltRef.Clone();
                    WristTiltKeys = true;
                    return true;
                }
                /// <summary>파일의 wtilt* 줄 (Save 와 같은 글, 줄 끝 nl) — 키가 없고 값도 없으면 "" (옛 파일은 줄이 없다)</summary>
                public string WristTiltLines(string nl)
                {
                    if (!(WristTiltKeys || WristTiltOn || WristTiltC != 0 || WristTiltB != null)) return "";
                    var ci = CultureInfo.InvariantCulture;
                    var sb = new StringBuilder();
                    sb.Append("wtiltc=" + WristTiltC.ToString("R", ci) + "   # 손목 공구 기울기 보정 상수 c (deg, + = 공구 끝이 베이스 쪽) — 엔코더 밖 기울기 (영점·펜 장착·브래킷 등), 손목 명령 += −τ").Append(nl);
                    if (WristTiltB != null && WristTiltB.Length >= 2) sb.Append("wtilt=" + WristTiltB[0].ToString("R", ci) + "," + WristTiltB[1].ToString("R", ci) + "   # 자세 항 b12,b13 (deg / 중력 지레 mm/deg) — τ = c + b·(g − gref), g = −L").Append(nl);
                    if (WristTiltRef != null && WristTiltRef.Length >= 2) sb.Append("wtiltref=" + WristTiltRef[0].ToString("R", ci) + "," + WristTiltRef[1].ToString("R", ci) + "   # 기준 지레 g12ref,g13ref").Append(nl);
                    sb.Append("wtilton=" + (WristTiltOn ? "1" : "0") + "   # 0 이면 기울기 보정 끔 (키를 지워도 끔)").Append(nl);
                    return sb.ToString();
                }
                /// <summary>손목 공구 기울기 보정을 지운다 (키째 — 저장하면 줄이 빠진다)</summary>
                public void ClearWristTilt() { WristTiltOn = false; WristTiltC = 0; WristTiltB = null; WristTiltRef = null; WristTiltKeys = false; }
                /// <summary>쉬운 말 — "공구 기울기 보정 +5.2° (일정)" / "… +5.2° (r 에 따라 바뀜 — b12 +0.31)" / "공구 기울기 보정 없음" (+ = 공구 끝이 베이스 쪽)</summary>
                public string WristTiltText()
                {
                    var ci = CultureInfo.InvariantCulture;
                    if (!HasWristTilt) return WristID < 0 ? "공구 기울기 보정 해당 없음 (꺾는 손목 없음)" : "공구 기울기 보정 없음";
                    bool bPose = WristTiltB != null && WristTiltB.Length >= 2 && (WristTiltB[0] != 0 || WristTiltB[1] != 0);
                    return "공구 기울기 보정 " + WristTiltC.ToString("+0.0;-0.0;0.0", ci) + "°" +
                           (bPose ? string.Format(ci, " (자세에 따라 바뀜 — b12 {0:+0.00;-0.00;0.00}, b13 {1:+0.00;-0.00;0.00})", WristTiltB[0], WristTiltB[1]) : " (일정)");
                }
                /// <summary>수직 눈 확인 기록용 서명 — "none" 또는 "c5.19|b0.31,0|r1.33,2.83" (값이 바뀌면 눈 확인을 다시 해야 한다)</summary>
                public string WristTiltSig()
                {
                    if (!HasWristTilt) return "none";
                    var ci = CultureInfo.InvariantCulture;
                    Func<double[], string> J = a => a == null ? "" : string.Join(",", Array.ConvertAll(a, v => v.ToString("0.####", ci)));
                    return "c" + WristTiltC.ToString("0.####", ci) + "|b" + J(WristTiltB) + "|r" + J(WristTiltRef);
                }
                // 바닥 보정표 (09-28) — 14번 손목 브래킷이 무게로 처져(사용자: 조여서 없앨 수 없음) 그리퍼가 지면에 수직이 못 된다.
                //   엔코더는 수직(피치 89~92°)이라 보지만 끝은 자세마다 다른 만큼 낮다 → 관절 모델로는 못 잡는다.
                //   FloorDz(r) = 바닥 접촉 꺾임점의 FK 높이(r) − 기준 (mm). 적용 = 끝이 Δz 만큼 더 높이 가도록 관절 12·13·14 를 바꾼다
                //   (끝의 반경 방향 위치·공구 피치 유지). 바닥 높이에서 잰 값이라 높이가 멀어질수록 근사. SagComp_FitFloorMap 이 만든다.
                //   효과(09-28/29 실기): 계단 밖 6곳 닿은 명령 높이 폭 38.1mm → 4.0/5.7mm (12회 RMS ±1.7mm).
                // ── 사용법 ──
                //   · 켜기  : 모델 파일에 floorr= / floordz= / flooron=1 세 줄 (SagComp_FitFloorMap 뒤 Save 가 쓴다). 출력 훅(SendPose → SagComp_JointOut)이 자동 적용.
                //   · 끄기  : flooron=0 (표는 남김) 또는 floor 세 줄을 지우면 **원래(관절 모델만) 그대로** 동작. 코드에선 FloorMapEnabled = false.
                //   · 만들기: ① SagComp_FloorPress(scene, cfg, 활성모델, …) 로 반경별 바닥 접촉 (ID11~16 토크 ON, 그리퍼 고정)
                //             측정하는 동안 손목(14번)을 엔코더 FK 기준 지면에 수직으로 붙잡는다 (cfg.WristHold, 기본 켬 — 10-02, 측정 때만)
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
                // 첫닿음 바닥 모델 (10-02, COjw_12_3D_SagFc.cs) — fcon=1 이면 GP·반경 보정표보다 먼저 쓴다. flooron=0 이면 이것도 끈다. 모델 파일 fc* 키
                //   쓰고 있는 모델의 Fc 를 손으로 바꿔 끼우면 크로스페이드 없이 다음 출력 틱부터 바뀐다 — 사본 모델에 넣고 SagComp_Set 하거나 SagComp_FitFloorFc 를 쓸 것
                public CSagFc Fc = null;
                public bool HasFc { get { return Fc != null && Fc.HasData; } }
                public bool FcDropped = false;                       // 불러올 때 fc* 줄이 있었지만 버렸다 (꼭 있어야 하는 줄이 없거나 잘못된 줄 — CSagFc.EndLoad). Summary 에 표시
                /// <summary>관절 모델만 쓰는 사본 (10-03) — 얕은 복사 (표본·계수 배열·바닥 모델 객체는 함께 쓴다 — 사본으로 바꾸지 말 것), 바닥 보정 끔(FloorMapEnabled = false),
                /// 켬(Enabled = true). 로봇팔 작업이 옮기는 동안 거는 보정 (실물 끝이 관절 처짐만큼 낮게 지나가지 않게). 관절 모델이 없으면 null.
                /// 손목 공구 기울기 보정(10-15)은 그대로 가진다 — 옮기는 동안·눈 확인 동안에도 실물 공구가 수직 (기울기만 있는 모델도 사본을 준다)</summary>
                public CSagComp JointOnlyCopy()
                {
                    if (!HasJoint && !HasWristTilt) return null;
                    var c = (CSagComp)MemberwiseClone();
                    c.FloorMapEnabled = false; c.Enabled = true;
                    return c;
                }
                /// <summary>다른 모델의 바닥 보정표를 복사 — FK 기준 표는 엔코더 밖 오차라 관절 모델을 다시 학습해도 그대로 유효하다.
                /// 명령 기준 표(FloorFromCmd)는 bForce 가 아니면 복사하지 않는다. 반환 = 복사했는가.</summary>
                public bool CopyFloorMapFrom(CSagComp other, bool bForce = false)
                {
                    if (other == null || (!other.HasFloorMap && !other.HasGp && !other.HasFc)) return false;
                    if (!other.HasFloorMap) { if (other.HasGp) Gp = other.Gp; if (other.HasFc) Fc = other.Fc; FloorMapEnabled = other.FloorMapEnabled; return true; }
                    if (other.FloorFromCmd && !bForce) return false;
                    FloorR = (float[])other.FloorR.Clone(); FloorDz = (float[])other.FloorDz.Clone();
                    FloorRef = other.FloorRef; FloorFromCmd = other.FloorFromCmd; FloorMapEnabled = other.FloorMapEnabled;
                    if (other.HasGp) Gp = other.Gp;                // GP 도 FK(엔코더 밖) 기준 — 관절 모델과 무관
                    if (other.HasFc) Fc = other.Fc;                // 첫닿음 모델도 FK 기준
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
                public bool IsUsable { get { return IsFitted || HasJoint || HasWristTilt; } }   // 손목 공구 기울기 보정만 있는 모델도 (10-15)

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
                    int nj = PosIDs.Length + (WristID >= 0 ? 1 : 0);                   // 꺾는 손목 없음이면 위치 관절만 (10-13)
                    if (JointIDs == null || JointIDs.Length != nj)
                    { JointIDs = new int[nj]; Array.Copy(PosIDs, JointIDs, PosIDs.Length); if (WristID >= 0) JointIDs[nj - 1] = WristID; }
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
                    if (!string.IsNullOrEmpty(Note))
                        foreach (string ln in Note.Replace("\r", "").Split('\n')) sb.AppendLine("# " + ln);   // 만든 과정 기록 — Load 는 # 줄을 건너뛴다
                    sb.AppendLine("robot=" + Robot);
                    if (!string.IsNullOrEmpty(MotorMap)) sb.AppendLine("motormap=" + MotorMap.Replace("\r", "").Replace("\n", "").Replace("#", "") + "   # 학습 때 모터 매핑 (축:방향:영점:틱/회전:각/회전) — 지금 매핑과 다르면 경고, 없으면 기본 매핑");
                    sb.AppendLine("func=" + FuncNumber);
                    sb.AppendLine("posids=" + string.Join(",", Array.ConvertAll(PosIDs, v => v.ToString(ci))));
                    sb.AppendLine("wrist=" + (WristID < 0 ? "none" : WristID.ToString(ci)));   // none = 꺾는 손목 없는 팔 (10-13 — 손목 있는 정의에는 쓰지 않는다)
                    // 로봇 정의 (10-04) — 기구학 표시가 있거나 수직 관절 합이 90 이 아니면 두 줄을 적는다 (표시 없는 옛 OMX 모델은 다시 저장해도 줄이 그대로 — 없으면 90 · 내장 OMX 로 읽힌다)
                    if (WristID < 0) sb.AppendLine("downsum=none   # 꺾는 손목 없음 — 바닥으로 수직(관절 합)이 없다");
                    else if (!string.IsNullOrEmpty(KinSig) || DownSumDeg != 90f) sb.AppendLine("downsum=" + DownSumDeg.ToString("0.###", ci) + "   # 로봇 정의의 수직 관절 합 (posids2+posids3+wrist)");
                    if (!string.IsNullOrEmpty(KinSig)) sb.AppendLine("kinsig=" + KinSig.Replace("\r", "").Replace("\n", "").Replace("#", "") + "   # 이 모델을 만든 기구학 (DH) 표시");
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
                    if (SigmaTakeKey || SigmaTakeDeg != SIGMA_TAKE_DEF) sb.AppendLine("sigmatake=" + SigmaTakeDeg.ToString(ci) + "   # 방향 전환 때 데드밴드를 넘기는 이상 관절 이동 (deg) — 0 이면 예전 램프 (sigmaramp × h)");
                    if (FlexGain != 0) sb.AppendLine("flex=" + FlexGain.ToString("R", ci) + "   # 외부 휨 배율 κ (바닥 접촉 캘리브레이션)");
                    if (PredictiveKey || !Predictive) sb.AppendLine("predictive=" + (Predictive ? "1" : "0") + "   # 0 이면 예측 보정 끔 — 1차 보정만 (쌍둥이 예측으로 다듬지 않는다, 키가 없으면 켬)");
                    sb.Append(WristTiltLines(Environment.NewLine));                                       // 손목 공구 기울기 보정 (10-15) — 키가 없으면 줄도 없음
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
                    if (HasFc)
                    {
                        if (!HasFloorMap && !HasGp) sb.AppendLine("flooron=" + (FloorMapEnabled ? "1" : "0") + "   # 0 이면 바닥 보정(첫닿음 모델) 끔 — 관절 모델만");
                        Fc.Save(sb);
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
                                case "motormap": { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); v = v.Trim(); m.MotorMap = v.Length > 0 ? v : null; break; }
                                case "func": m.FuncNumber = int.Parse(val, ci); break;
                                case "posids": m.PosIDs = Array.ConvertAll(val.Split(','), v => int.Parse(v.Trim(), ci)); break;
                                case "wrist": m.WristID = CSagArmSetup.IsWristNone(val) ? CSagArmCfg.WRIST_NONE : int.Parse(val, ci); break;   // none = 꺾는 손목 없음 (10-13)
                                case "downsum":
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash);
                                        m.DownSumDeg = string.Equals(v.Trim(), "none", StringComparison.OrdinalIgnoreCase) ? float.NaN : float.Parse(v.Trim(), NumberStyles.Float, ci); break;
                                    }
                                case "kinsig": { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.KinSig = v.Trim(); break; }
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
                                case "sigmatake":                                                       // 데드밴드 넘기 (10-16) — 0 이면 예전 램프
                                    { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.SigmaTakeDeg = Math.Max(0f, float.Parse(v.Trim(), NumberStyles.Float, ci)); m.SigmaTakeKey = true; break; }
                                case "wtiltc": case "wtilt": case "wtiltref": case "wtilton":           // 손목 공구 기울기 보정 (10-15)
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); v = v.Trim();
                                        m.WristTiltKeys = true;
                                        if (key == "wtiltc") m.WristTiltC = double.Parse(v, NumberStyles.Float, ci);
                                        else if (key == "wtilton") m.WristTiltOn = v == "1";
                                        else
                                        {
                                            double[] a = Array.ConvertAll(v.Split(','), t => double.Parse(t.Trim(), NumberStyles.Float, ci));
                                            if (a.Length < 2) throw new FormatException(key);
                                            if (key == "wtilt") m.WristTiltB = new double[] { a[0], a[1] }; else m.WristTiltRef = new double[] { a[0], a[1] };
                                        }
                                        break;
                                    }
                                case "flex":
                                    { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.FlexGain = double.Parse(v.Trim(), ci); break; }
                                case "predictive":                                                      // 예측 보정 (10-16) — 0 이면 끔, 그 밖은 켬
                                    { string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash); m.Predictive = v.Trim() != "0"; m.PredictiveKey = true; break; }
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
                                case "fcon": case "fcbeta": case "fcbnd": case "fcprior": case "fccov": case "fcell": case "fcsig": case "fccap": case "fcenv": case "fcref": case "fcx":
                                    {
                                        string v = val; int hash = v.IndexOf('#'); if (hash >= 0) v = v.Substring(0, hash);
                                        if (m.Fc == null) m.Fc = new CSagFc();
                                        m.Fc.ParseKey(key, v.Trim());                                  // 학습점의 특징·잔차 GP 는 C3d 가 준비한다 (SagComp_FcPrepare)
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
                    if (m.WristID < 0) m.DownSumDeg = float.NaN;                       // 꺾는 손목 없음 — 수직 기준 없음 (줄이 없어도)
                    bool bBad = m.IsFitted && (m.Cz.Length != FeatCount(m.Degree) || m.Cr == null || m.Cr.Length != FeatCount(m.Degree));
                    if ((!m.IsFitted || bBad) && m.Samples.Count >= 2) { m.Fit(); m.FitDynamic(); }
                    if (m.Kr != null && m.Kr.Length != 2) m.Kr = null;
                    if (m.Kz != null && m.Kz.Length != 3) m.Kz = null;
                    if ((m.Kr == null) != (m.Kz == null)) m.Kr = m.Kz = null;
                    if ((m.Jc != null || m.Jh != null) && !m.HasJoint) m.Jc = m.Jh = null;   // 길이 어긋난 관절 계수 = 버린다 (자동 재학습은 안 한다 — 명시 호출만)
                    if ((m.Jtau != null || m.Jkap != null) && !m.HasJointLag) m.Jtau = m.Jkap = null;
                    if (m.Gp != null && (m.Gp.X.Length == 0 || !m.Gp.Solve())) m.Gp = null;       // 학습점으로 촐레스키 다시 풀기
                    if (m.Fc != null && !m.Fc.EndLoad()) { m.Fc = null; m.FcDropped = true; }    // fcbeta·fcell·fcsig·fcx 가 없거나 fc* 줄이 하나라도 잘못되면 통째로 버린다 → 바닥 GP·보정표
                    try { m.SourcePath = Path.GetFullPath(strPath); } catch { m.SourcePath = strPath; }
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
                            + (HasFc ? Fc.SummaryText(FloorMapEnabled) : "")                    // 첫닿음 모델 (fcon=1 이면 GP·보정표는 안 쓰므로 생략)
                            + (HasFc && Fc.Enabled ? "" : HasGp ? string.Format(CultureInfo.InvariantCulture, ", 바닥 GP {0}점 (길이 척도 팔 {1:F0}°, 두 상태 반경 {3}곳){2}", Gp.Count, Gp.Ell.Length > 1 ? Gp.Ell[1] : 0,
                                FloorMapEnabled && Gp.Enabled ? "" : " [꺼짐]", Gp.BadR == null ? 0 : Gp.BadR.Length)
                               : HasFloorMap ? string.Format(CultureInfo.InvariantCulture, ", 바닥 보정표 {0}점 r {1:F0}~{2:F0}mm Δz {3:+0.0;-0.0}~{4:+0.0;-0.0}mm{5}",
                                FloorR.Length, FloorR[0], FloorR[FloorR.Length - 1], FloorDzMin(), FloorDzMax(), FloorMapEnabled ? "" : " [꺼짐 — flooron=0]") : "")
                            + (FcDropped ? ", 첫닿음 모델 버림 (파일의 fc* 줄이 모자라거나 잘못됨 — fcbeta·fcell·fcsig·fcx 확인)" : "")
                            + (HasWristTilt ? ", " + WristTiltText() : "");                      // 손목 공구 기울기 보정 (10-15)
                    }
                    if (!IsFitted && HasWristTilt) return "처짐 보상: " + WristTiltText() + " — 관절·바닥 모델 없음 (기울기 보정만)" + (Enabled ? "" : " [꺼짐]");
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
                SagTwin_SetModel(m);                                // 처짐 쌍둥이도 같은 모델로 (보정을 꺼도 남는다)
                return true;
            }
            public void SagComp_Set(CSagComp model) { SagComp_Switch(model); if (model != null) SagTwin_SetModel(model); }
            public void SagComp_Clear() { SagComp_Switch(null); }
            public bool SagComp_IsLoaded { get { return m_SagComp != null && m_SagComp.IsUsable; } }
            public CSagComp SagComp_Get() { return m_SagComp; }
            public void SagComp_Enable(bool bOn)
            {
                SagFc_EnsureReady(m_SagComp);
                lock (m_dicSagDir) { CSagComp m = m_SagComp; if (m != null) m.Enabled = bOn; SagComp_SwitchLocked(m); }   // 켬·끔 플래그도 전환과 같은 lock 안에서 (SagComp_SwitchLocked)
            }
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
            private readonly Dictionary<int, float> m_dicSagApplied = new Dictionary<int, float>();  // 직전 출력 틱에 실제로 얹은 보정 (관절 ID → deg, 전환 포함)
            private Dictionary<int, float> m_dicSagFrom = null;                                      // 전환 순간 걸려 있던 보정 — 여기서 새 보정으로 1초에 걸쳐 넘어간다 (관절마다)
            private int m_nSagSwitchGen = 0;                                                         // 전환 세대 — 전환·초기화마다 1 증가 (출력 틱이 사이에 들어온 전환을 지우지 않게)
            // 전환 진행 = 출력 틱이 흐른 시간 (10-03) — 벽시계가 아니다. 첫 틱은 전환 순간 값 그대로(w 0), 그다음 틱마다 직전 출력 틱과의 간격만큼
            //   나아가되 한 틱에 SAG_BLEND_TICK_MAX_MS 까지만. ★10-02 검토: 벽시계로 재면 토크 ON → 짧은 조그(300ms) → 1.5초 쉼 → 다음 조그의 첫 틱에
            //   남은 전환(보정의 70%, 12/13/14 3~4°)이 한 번에 들어갔다. 쉬는 동안(출력 틱 없음)은 전환이 멈춰 있어 실물로 가는 보정도 그대로다.
            //   40ms 스트리밍이면 예전과 같다 (틱 간격 ≤ 60ms 는 그대로 더한다).
            private const double SAG_BLEND_MS = 1000.0;                                              // 전환에 드는 출력 틱 시간 (ms)
            private const int SAG_BLEND_TICK_MAX_MS = 60;                                            // 출력 틱 하나가 전환을 밀어 주는 최대 시간 (ms)
            private bool m_bSagBlendRun = false;                                                     // 이 전환의 첫 출력 틱을 지났다
            private double m_dSagBlendMs = 0;                                                        // 전환 뒤 출력 틱이 쌓은 시간 (ms)
            private int m_nSagOutLastMs = int.MinValue;                                              // 직전 출력 틱 시각 (모션 시계) — 틱 간격용
            // ── 쌍둥이 그림자 출력 단 (10-03) — 실물 출력 단 상태를 바꾸지 않는다 ──
            //   시뮬만(싱크 없음·3D 만 작업의 빈 싱크·토크 꺼짐)일 때 쌍둥이는 출력 단을 그림자로 돌려 "보냈을 명령" 을 만든다 (SagTwin_Update).
            //   ★10-03 검토: 예전에는 그림자가 실물 출력 단의 상태(직전 틱 보정·전환 기준·전환 시계·세대·방향/속도 추적·손목 유지·진단값)를 그대로 바꿔,
            //   토크를 붙잡은 실물 옆에서 3D 만 작업(쌍둥이 보기)을 돌린 뒤 다음 조그 첫 틱에 그 잔재(1.5~6°)가 얹힐 수 있었다.
            //   이제 그림자는 제 상태 사본으로 돈다 (SagComp_JointOutShadow) — lock (m_dicSagDir) 안에서 실물 상태를 잠깐 바꿔 끼우고 같은 계산을 한 뒤 되돌린다.
            //   그림자 사본은 실물 상태가 통째로 바뀌면(방향 추적 초기화·전환 씨앗·상태 되돌림) 또는 실물 출력 틱이 돌았으면 실물 상태에서 다시 시작하고,
            //   모델 전환만 있었으면 그림자에도 같은 전환(기준 = 그림자가 직전에 얹은 보정, 진행 0)을 건다 — 1초 넘어가기가 쌍둥이에도 그대로 보인다
            private CSagOutState m_SagShadowState = null;                                            // 그림자 출력 단의 상태 사본 (null = 다음 그림자 틱에 실물 상태에서 시작)
            private int m_nSagShadowGen = -1, m_nSagShadowHard = -1, m_nSagShadowReal = -1;         // 그림자 사본을 맞춘 때의 실물 전환 세대·통째 바뀜 세대·실물 출력 틱 수
            private int m_nSagHardGen = 0;                                                           // 실물 출력 단 상태를 통째로 바꾼 횟수 (방향 추적 초기화·전환 씨앗·상태 되돌림)
            private int m_nSagRealTicks = 0;                                                         // 실물 출력 틱 수 (그림자 틱은 세지 않는다)
            private bool m_bSagShadowTick = false;                                                   // 그림자 출력 틱이 도는 중 (lock (m_dicSagDir) 안에서만 true)

            /// <summary>보정 모델 전환 — 즉시 0 으로 떨어뜨리지 않고, 전환 순간 실물에 걸려 있던 보정에서 새 모델의 보정으로 1초에 걸쳐 넘어간다.
            /// ★09-28 버그: 전환마다 페이드를 0 부터 다시 시작해, 먼 곳(보정이 끝을 20~30mm 올리던 곳)에서 다음 동작 첫 틱에 끝이 뚝 떨어져
            /// 바닥을 내려찍었다(사용자 목격 3회 — 먼 3점 뒤 전환마다). null(끄기)도 같은 방식으로 1초에 걸쳐 빠진다.
            /// 1초는 출력 틱이 흐른 시간이다 (틱 하나에 최대 60ms — 멈춰 있는 동안은 나아가지 않는다).
            /// 모션·실물 스트리밍 중에 다른 스레드에서 불러도 된다 (SagComp_SwitchLocked).</summary>
            private void SagComp_Switch(CSagComp m)
            {
                SagFc_EnsureReady(m);                               // 첫닿음 모델(fc* 키)은 학습점 특징·잔차 GP 를 전환할 때 준비 (~15ms — 출력 틱에서 하지 않게).
                                                                    //   관절값 사본으로 계산해 3D 관절값·CalcF 를 건드리지 않는다 — 어느 스레드에서 전환해도 모션 스레드가 보내는 명령에 섞이지 않는다
                lock (m_dicSagDir) SagComp_SwitchLocked(m);
            }
            // 전환의 본체 — lock (m_dicSagDir) 안에서만 부른다. 전환 기준(직전 출력 틱에 얹은 보정)·모델·시계·세대를 함께 바꾼다.
            //   출력 틱(SagComp_JointOutModel)은 틱 시작 때 같은 lock 안에서 모델·첫닿음 모델·기준·시계·세대를 한 번에 읽어 그 틱 끝까지 쓰고,
            //   틱 끝에서는 세대가 그대로일 때만 기준을 지운다. 켬·끔 플래그(SagComp_Enable)나 쓰고 있는 모델의 첫닿음 모델(SagComp_FitFloorFc)도
            //   같은 lock 안에서 바꾼다 — 어느 틱도 새 값을 이전 보정 그대로(w=1) 쓰지 않는다.
            //   ★10-02 검증: 기준·모델·시계를 따로 바꾸던 때는 w=1 출력 틱이 그 사이에 들어온 전환 기준을 지워, 다른 스레드에서 전환하면
            //   보정 전체(관절 모델 + 바닥 dz)가 한 틱에 0 으로 빠졌다가 1초에 걸쳐 돌아왔다 (빠른 틱 4/4, 30ms 틱 60번 중 10번 — 09-28 과 같은 모양).
            private void SagComp_SwitchLocked(CSagComp m)
            {
                m_dicSagFrom = new Dictionary<int, float>(m_dicSagApplied);
                m_SagComp = m;
                m_bSagBlendRun = false; m_dSagBlendMs = 0;
                m_nSagSwitchGen++;
            }
            private float[] m_afSagLastJoint = new float[0];
            // 관절 명령 속도·가속도 추정 — 출력 틱마다 (이번 각 − 지난 각) / 모션 시계 간격. 명령이 시간의 매끈한 함수(smoothstep)라
            //   걸러 낼 잡음이 거의 없다. 학습 표본(SurveyDynamic)과 실행(앞서 보내기)이 **같은 추정기**를 쓴다.
            private const int SAG_VEL_GAP_MS = 150;                                          // 이보다 긴 틈 = 새 동작 (속도 0 에서 다시)
            private readonly Dictionary<int, float> m_dicSagVelDeg = new Dictionary<int, float>();
            private readonly Dictionary<int, int> m_dicSagVelT = new Dictionary<int, int>();
            private readonly Dictionary<int, float> m_dicSagOmega = new Dictionary<int, float>();    // deg/s
            private readonly Dictionary<int, float> m_dicSagAlpha = new Dictionary<int, float>();    // deg/s²

            /// <summary>방향 추적 초기화 — 토크 ON 직후(CScene_t) 처럼 실물 자세를 새로 잡았을 때 부른다. 걸린 보정이 없음에서 시작하므로
            /// 직전 틱 보정 진단값(SagComp_LastJointCorr·LastFloorDz)도 비운다 (10-03 — 낡은 진단값을 이어받기 판단이 '보정이 걸려 있음' 으로 보지 않게)</summary>
            public void SagComp_JointReset()
            {
                lock (m_dicSagDir)
                {
                    m_dicSagAnchor.Clear(); m_dicSagDir.Clear(); m_dicSagSigma.Clear(); m_dicSagPrev.Clear();
                    m_dicSagVelDeg.Clear(); m_dicSagVelT.Clear(); m_dicSagOmega.Clear(); m_dicSagAlpha.Clear();
                    m_dicSagApplied.Clear(); m_dicSagFrom = null;    // 실물 자세를 새로 잡았다 — 걸려 있는 보정 없음에서 시작
                    m_bSagBlendRun = false; m_dSagBlendMs = 0; m_nSagSwitchGen++; m_nSagHardGen++;
                    m_afSagLastJoint = new float[0]; m_fSagLastFloorDz = 0; m_afSagLastFc = null; m_afSagLastPred = null;
                }
                lock (m_oWristHold) { m_nWristHoldID = -1; m_fWristHold = 0; m_bWristHoldFading = false; m_dWristHoldFadeMs = 0; m_bWristHoldFadeTicked = false; m_fWristHoldLast = 0; }
            }

            /// <summary>처짐 보정을 페이드 없이 끈다 (10-03 이어받기 — CArmConn_t 가 서보가 이미 붙잡은 목표를 3D 로 그대로 이어받을 때만 부른다).
            /// 적용 중 모델의 켬 플래그를 끄고(SagComp_Enable(false) 와 같은 결과 — 앱의 '처짐 보정 켬' 이 따라온다), 출력 단에 걸린 보정·전환 기준·전환 진행·
            /// 방향/속도 추적·손목 유지·진단값(직전 틱 관절 보정·바닥 dz·첫닿음)을 비운다 → 전환은 끝난 상태, 다음 출력 틱 = 3D 그대로.
            /// 3D = 서보 목표일 때만 쓴다 — 그러면 첫 틱 변화가 0 이다 (3D 가 목표가 아닌 채 부르면 걸려 있던 보정만큼 한 틱에 빠진다).
            /// 다시 켜면(SagComp_Enable(true)) 다음 동작의 출력 틱 1초에 걸쳐 0 에서 들어온다 (전환 기준 = 걸린 보정 0).
            /// 반환 = 꺼진 것이 있었다 (모델이 켜져 있었거나 출력 단에 보정·손목 유지가 걸려 있었다). 모션 중에 불러도 된다 (출력 틱과 같은 lock). 예외 없음</summary>
            internal bool SagComp_OffForAdopt()
            {
                bool bWas = false;
                lock (m_dicSagDir)
                {
                    CSagComp m = m_SagComp;
                    if (m != null && m.Enabled) { bWas = true; m.Enabled = false; }
                    foreach (float v in m_dicSagApplied.Values) if (v != 0f) bWas = true;
                    if (m_dicSagFrom != null) foreach (float v in m_dicSagFrom.Values) if (v != 0f) bWas = true;
                    m_dicSagAnchor.Clear(); m_dicSagDir.Clear(); m_dicSagSigma.Clear(); m_dicSagPrev.Clear();
                    m_dicSagVelDeg.Clear(); m_dicSagVelT.Clear(); m_dicSagOmega.Clear(); m_dicSagAlpha.Clear();
                    m_dicSagApplied.Clear(); m_dicSagFrom = null;            // 걸린 보정 0, 빠질 보정도 없음 — 전환은 끝난 것 (SagComp_BlendProgress = 1)
                    m_bSagBlendRun = false; m_dSagBlendMs = 0; m_nSagSwitchGen++; m_nSagHardGen++;
                    m_afSagLastJoint = new float[0]; m_fSagLastFloorDz = 0; m_afSagLastFc = null; m_afSagLastPred = null;
                    lock (m_oWristHold)
                    {
                        if (m_fWristHoldLast != 0f || (m_nWristHoldID >= 0 && WristHoldCurrent() != 0f)) bWas = true;
                        m_nWristHoldID = -1; m_fWristHold = 0; m_bWristHoldFading = false; m_dWristHoldFadeMs = 0; m_bWristHoldFadeTicked = false; m_fWristHoldLast = 0;
                    }
                }
                return bWas;
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

            // 보상용 이동 방향 σ (−1..+1) 한 틱 — 출력 단 (이상 관절의 이번 틱 이동 dPose, 직전 출력 틱과의 간격 nGapMs).
            //   sigmatake > 0 (10-16 기본 0.1°): σ += dPose / sigmatake — 반전 뒤 이상 관절이 sigmatake 만큼 가는 사이 보낸 명령(θ + h·σ)이 데드밴드 2h 를 건넌다.
            //     놀이(play) 꼴이라 그 자체가 이력이다 — 속도 부호를 보지 않으므로 반전 자리의 0 근처 속도에서 떨지 않고, 작은 흔들림은 그만큼만 σ 를 움직인다.
            //     한 틱에 넘는 양은 σ 전체 전환(2)이 SAG_SIGMA_FLIP_MS 보다 빠르지 않게 자른다 (꺾인 경로처럼 한 틱에 반전해도 명령이 한 틱에 2h 튀지 않는다 —
            //     틱 간격은 전환과 같이 SAG_BLEND_TICK_MAX_MS 까지만 센다). 한 방향으로 계속 가면 ±1 그대로 — 예전과 같다.
            //   sigmatake = 0: 예전 — σ += dPose / max(0.2, sigmaramp·h) (반전 뒤 이상 관절이 h 를 갈 때까지 실물이 멈춰 최대 h 뒤처진다)
            private const double SAG_SIGMA_FLIP_MS = 100.0;                                  // σ 가 −1↔+1 을 넘어가는 가장 짧은 시간 (출력 틱 시간, ms)
            private static float SagComp_SigmaStep(CSagComp m, int j, float sg, float dPose, int nGapMs)
            {
                if (m.SigmaTakeDeg > 0)
                {
                    double ds = dPose / m.SigmaTakeDeg;
                    double lim = 2.0 * Math.Max(0, Math.Min(SAG_BLEND_TICK_MAX_MS, nGapMs)) / SAG_SIGMA_FLIP_MS;
                    if (ds > lim) ds = lim; if (ds < -lim) ds = -lim;
                    sg = (float)(sg + ds);
                }
                else sg += dPose / (float)Math.Max(0.2, m.SigmaRamp * m.Jh[j]);
                if (sg > 1f) sg = 1f; if (sg < -1f) sg = -1f;
                return sg;
            }

            /// <summary>실물로 나가는 관절각에 관절 보상을 얹는다 — CScene_t.SendPose 가 모션 틱마다 부른다.
            /// afDeg = ids 순서의 3D(이상) 관절각 → 보정된 실물 명령으로 바뀐다. **3D 모델은 건드리지 않는다**
            /// (그래서 IK 시드·현재 TCP·경로 시작점이 보정에 오염되지 않는다 — 09-28 주행 검증 실패의 구조적 원인 제거).
            /// 명령 = θ − c·L(θ) + h·σ.  σ 는 반전 뒤 이상 관절이 sigmatake(기본 0.1°) 를 가는 사이 −1..+1 을 매끈하게 넘어가서
            /// (100ms 보다 빠르지 않게 — SagComp_SigmaStep), 방향이 바뀌는 순간에도 명령이 한 틱에 2h 튀지 않고 실물도 반전 자리에서 거의 멈추지 않는다
            /// (sigmatake=0 이면 예전 램프 — 이동량/(sigmaramp·h)). 모델을 켠 뒤 1초에 걸쳐 들어온다.
            /// 지연 모델이 있으면 τ·ω + κ·α 만큼 앞서 보낸다 (상한 MaxLeadDeg). smoothstep 이라 동작 끝에서 ω→0 → 앞섬도 0.
            /// 예측 보정(CSagComp.Predictive, 10-16 — 기본 켬)이면 이 1차 보정을 처짐 쌍둥이 예측으로 다듬는다 (SagComp_PredRefine — 쌍둥이 끝 = 목표, 공구 = 3D 의 공구 각).
            /// 모델 유무와 무관하게 관절별 이동 방향·속도를 추적한다 (측정 표본 기록용).</summary>
            public void SagComp_JointOut(int[] ids, float[] afDeg)
            {
                if (ids == null || afDeg == null) return;
                float[] ideal = SagTwin_Wanted ? (float[])afDeg.Clone() : null;   // 처짐 쌍둥이용 — 보정 전 이상각
                float[] db;
                SagComp_JointOutModel(ids, afDeg, out db);
                SagComp_WristHoldOut(ids, afDeg);   // 측정 중에만(SagComp_FloorPress) — 손목 수직 유지. 방향 추적·모델 전환과 별개
                if (ideal != null) SagTwin_NoteCommand(ids, ideal, afDeg, db);   // 실제로 보낸 명령 기록 (COjw_12_3D_SagTwin.cs)
            }
            // 쌍둥이 그림자 출력 틱 (SagTwin_Update — 실물로 나가지 않을 때) — SagComp_JointOut 과 같은 계산(관절·바닥 보정 + 전환 + 손목 유지)을
            //   그림자 상태 사본(m_SagShadowState)으로 한다. 실물 출력 단의 상태는 이 틱 전후로 똑같다 (부작용 없음 — 위 설명).
            //   omega·alpha·dir = 그림자 틱이 끝난 순간의 그림자 쪽 방향·속도 추적 (ids 순) — 쌍둥이의 지연·방향 예측이 실물 추적 대신 쓴다
            //   db = 그림자 명령에 든 데드밴드 항 (ids 순, 없으면 null — SagComp_JointOutModel)
            private void SagComp_JointOutShadow(int[] ids, float[] afDeg, out float[] omega, out float[] alpha, out float[] dir, out float[] db)
            {
                omega = alpha = dir = db = null;
                if (ids == null || afDeg == null) return;
                float[] ideal = SagTwin_Wanted ? (float[])afDeg.Clone() : null;
                lock (m_dicSagDir)
                {
                    CSagOutState real = SagComp_CaptureOutStateLocked();
                    CSagOutState sh = m_SagShadowState;
                    if (sh == null || m_nSagShadowHard != m_nSagHardGen || m_nSagShadowReal != m_nSagRealTicks) sh = real;   // 실물 상태에서 (다시) 시작 — 사본이라 그대로 써도 된다
                    else if (m_nSagShadowGen != m_nSagSwitchGen)
                    {
                        // 그사이 모델 전환만 있었다 — 그림자에도 같은 전환 (기준 = 그림자가 직전 틱에 얹은 보정, 진행 0 에서)
                        sh.From = new Dictionary<int, float>(sh.Applied); sh.BlendRun = false; sh.BlendMs = 0;
                    }
                    SagComp_InstallOutStateLocked(sh);
                    m_bSagShadowTick = true;
                    try
                    {
                        SagComp_JointOutModel(ids, afDeg, out db);
                        SagComp_WristHoldOut(ids, afDeg);
                        omega = new float[ids.Length]; alpha = new float[ids.Length]; dir = new float[ids.Length];
                        for (int i = 0; i < ids.Length; i++)
                        {
                            float v; int d;
                            if (m_dicSagOmega.TryGetValue(ids[i], out v)) omega[i] = v;
                            if (m_dicSagAlpha.TryGetValue(ids[i], out v)) alpha[i] = v;
                            if (m_dicSagDir.TryGetValue(ids[i], out d)) dir[i] = d;
                        }
                    }
                    finally
                    {
                        m_bSagShadowTick = false;
                        m_SagShadowState = SagComp_CaptureOutStateLocked();
                        SagComp_InstallOutStateLocked(real);                             // 실물 상태 그대로 되돌림 (세대는 올리지 않는다 — 실물에는 아무 일도 없었다)
                        m_nSagShadowGen = m_nSagSwitchGen; m_nSagShadowHard = m_nSagHardGen; m_nSagShadowReal = m_nSagRealTicks;
                    }
                }
                if (ideal != null) SagTwin_NoteCommand(ids, ideal, afDeg, db);   // 그림자가 만든 "보냈을 명령" (lock 밖에서 — 쌍둥이 lock 과 순서가 엇갈리지 않게)
            }
            // dbOut = 이 틱 명령에 든 데드밴드 항 w·h·σ (ids 순, 10-16 — 쌍둥이가 놀이로 실물의 반전 멈춤을 본다). 관절 모델을 쓰지 않은 틱은 null
            private void SagComp_JointOutModel(int[] ids, float[] afDeg, out float[] dbOut)
            {
                dbOut = null;
                int nNow = MotionTick_ms();
                CSagComp m; CSagFc fc; Dictionary<int, float> from; double w; int nGen; bool bModel;
                int nGap;
                lock (m_dicSagDir)
                {
                    if (!m_bSagShadowTick) m_nSagRealTicks++;                                         // 실물 출력 틱 — 그림자 사본은 다음 그림자 틱에 실물 상태에서 다시 시작한다
                    nGap = (m_nSagOutLastMs == int.MinValue) ? 0 : unchecked(nNow - m_nSagOutLastMs);   // 직전 출력 틱과의 간격 (ms) — 전환 진행·σ 넘기 속도 (10-16)
                    m_nSagOutLastMs = nNow;
                    for (int i = 0; i < ids.Length && i < afDeg.Length; i++) { SagComp_TrackDir(ids[i], afDeg[i]); SagComp_TrackVel(ids[i], afDeg[i], nNow); }
                    // 모델·첫닿음 모델·전환 기준·진행·세대를 한 번에 읽고 이 틱 끝까지 쓴다 — 전환(SagComp_SwitchLocked)이 같은 lock 안에서 함께 바꾼다
                    m = m_SagComp; fc = m == null ? null : m.Fc; from = m_dicSagFrom; nGen = m_nSagSwitchGen;
                    bModel = m != null && m.Enabled && (m.HasJoint || m.HasWristTilt);              // 손목 공구 기울기만 있는 모델도 (10-15)
                    if (!bModel && (from == null || from.Count == 0))
                    {
                        m_dicSagApplied.Clear(); m_dicSagFrom = null; m_bSagBlendRun = false; m_dSagBlendMs = 0;
                        m_afSagLastJoint = new float[0]; m_fSagLastFloorDz = 0; m_afSagLastFc = null; m_afSagLastPred = null;
                        return;
                    }
                    // 전환은 출력 틱이 흐를 때만 나아간다 — 첫 틱은 전환 순간 값 그대로, 그다음은 틱 간격만큼 (한 틱에 SAG_BLEND_TICK_MAX_MS 까지)
                    if (!m_bSagBlendRun) { m_bSagBlendRun = true; m_dSagBlendMs = 0; }
                    else m_dSagBlendMs += Math.Max(0, Math.Min(SAG_BLEND_TICK_MAX_MS, nGap));
                    w = Math.Min(1.0, m_dSagBlendMs / SAG_BLEND_MS);
                }

                var target = new Dictionary<int, float>();                               // 새 모델의 보정 (전환 전 값, 관절 ID → deg)
                float dzMap = 0;
                m_afSagLastFc = null;                                                     // 첫닿음 모델 진단 — 이번 틱에 쓰면 SagComp_FcOut 이 채운다
                float[] levOut = null; int[] jidsOut = null;                              // 이 틱의 중력 지레 (손목 공구 기울기의 자세 항이 같은 값을 쓴다)
                float[] poseOut = null, sgOut = null; double[] leadOut = null;            // 예측 보정(10-16)이 쓰는 이상 자세·이동 방향 σ·앞서 보내기 (JointIDs 순)
                bool bFloorOut = false; double dzOut = 0; int[] fidOut = null; float[] fqOut = null;   // 1차 보정이 얹은 바닥 항
                if (bModel && m.HasJoint)
                {
                    int nj = m.JointIDs.Length;
                    int[] idx = new int[nj]; float[] pose = new float[nj];
                    for (int j = 0; j < nj; j++) { idx[j] = Array.IndexOf(ids, m.JointIDs[j]); pose[j] = idx[j] >= 0 ? afDeg[idx[j]] : GetData(m.JointIDs[j]); }
                    float[] lev = SagComp_JointLevers(m.FuncNumber, m.JointIDs, pose);
                    levOut = lev; jidsOut = m.JointIDs;
                    poseOut = pose; sgOut = new float[nj];
                    lock (m_dicSagDir)
                        for (int j = 0; j < nj; j++)
                        {
                            int id = m.JointIDs[j];
                            float sg, prev;
                            if (!m_dicSagSigma.TryGetValue(id, out sg)) { int d; m_dicSagDir.TryGetValue(id, out d); sg = d; }   // 첫 틱 = 추적된 방향에서 시작
                            else if (m_dicSagPrev.TryGetValue(id, out prev)) sg = SagComp_SigmaStep(m, j, sg, pose[j] - prev, nGap);
                            m_dicSagSigma[id] = sg; m_dicSagPrev[id] = pose[j];
                            sgOut[j] = sg;
                            double c = m.JointCorrStatic(j, lev[j], sg);             // = −(c·L·(1+κ) − h·σ)
                            if (m.HasJointLag && m.LagEnabled)
                            {
                                float om, al; m_dicSagOmega.TryGetValue(id, out om); m_dicSagAlpha.TryGetValue(id, out al);
                                double lead = -m.PredictJointLag(j, om, al);                     // = τ·ω + κ·α
                                if (lead > m.MaxLeadDeg) lead = m.MaxLeadDeg; if (lead < -m.MaxLeadDeg) lead = -m.MaxLeadDeg;
                                c += lead;
                                if (leadOut == null) leadOut = new double[nj];
                                leadOut[j] = lead;
                            }
                            if (c > m.MaxAbsJointDeg) c = m.MaxAbsJointDeg; if (c < -m.MaxAbsJointDeg) c = -m.MaxAbsJointDeg;
                            target[id] = (float)c;
                        }
                    // 바닥 보정표 — 이상 자세(보정 전)의 TCP 반경에서 Δz 를 읽어 관절 12·13·14 로 바꾼다
                    //   우선순위: fcon=1 → 첫닿음 모델 (COjw_12_3D_SagFc.cs), 아니면 바닥 GP, 그것도 없으면 반경 보정표. flooron=0 이면 전부 끈다
                    bool bFc = fc != null && fc.HasData && fc.Enabled;                   // = m.HasFc && m.Fc.Enabled — 틱 시작 때 읽은 첫닿음 모델
                    if ((bFc || (m.HasGp ? m.Gp.Enabled : m.HasFloorMap)) && m.FloorMapEnabled && m.PosIDs != null && m.PosIDs.Length >= 3)
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
                        if (bFc)
                        {
                            // 첫닿음 모델: dz = (물리 + clip(잔차, ±2) − Ref)·w_env, |dz| ≤ 15 — 범위 안에는 띠·데이터 거리 감쇠 없음. 표준편차는 진단값만
                            SagComp_TcpRZ(m.FuncNumber, fid, fq, out r0, out z0);
                            double fsd; dz = SagComp_FcOut(m, fc, ids, afDeg, pose, lev, fq, r0, z0, out fsd); m_fSagLastGpStd = (float)fsd;
                        }
                        else if (m.HasGp)
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
                            bFloorOut = true; dzOut = dz; fidOut = fid; fqOut = fq;
                            for (int k = 0; k < 3; k++)
                            {
                                if (fid[k] < 0) continue;                                    // 꺾는 손목 없음 — dz 는 피치 두 관절로만 (2×2 야코비안)
                                float t0; target.TryGetValue(fid[k], out t0);
                                target[fid[k]] = t0 + (float)Math.Max(-m.MaxAbsJointDeg, Math.Min(m.MaxAbsJointDeg, dq[k]));
                            }
                        }
                    }
                }
                // 손목 공구 기울기 보정 (10-15) — 손목 명령 += −τ(이상 자세), 관절 보정과 같은 자르기 (MaxAbsJointDeg)·아래 1초 전환 안
                float fTiltNow = 0;
                if (bModel && m.HasWristTilt)
                {
                    fTiltNow = (float)SagComp_TiltAt(m, ids, afDeg, levOut, jidsOut, null);
                    float t0; target.TryGetValue(m.WristID, out t0);
                    target[m.WristID] = t0 + (float)Math.Max(-m.MaxAbsJointDeg, Math.Min(m.MaxAbsJointDeg, -fTiltNow));
                }
                // 예측 보정 (10-16) — 1차 보정을 쌍둥이 예측으로 다듬는다 (아래 전환 앞 — 자르기는 항마다 1차와 같다. 끄면 건너뛰어 예전과 같다)
                float[] predInfo = null;
                if (bModel && m.HasJoint && m.Predictive)
                {
                    double[] adP = SagFc_MotCopy();
                    for (int i = 0; i < ids.Length && i < afDeg.Length; i++) SagFc_Set(adP, ids[i], afDeg[i]);
                    SagComp_PredRefine(m, adP, poseOut, sgOut, leadOut, bFloorOut, dzOut, fidOut, fqOut, m.HasWristTilt ? (double)fTiltNow : double.NaN, target, out predInfo);
                }
                m_afSagLastPred = predInfo;
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
                    if (w >= 1.0 && nGen == m_nSagSwitchGen) m_dicSagFrom = null;      // 넘어가기 끝 — 이 틱 사이에 들어온 전환(새 기준)은 지우지 않는다
                }
                m_fSagLastTilt = (float)(w * fTiltNow);
                if (bModel && m.HasJoint)
                {
                    float[] corr = new float[m.JointIDs.Length];
                    lock (m_dicSagDir) for (int j = 0; j < corr.Length; j++) { float o; if (m_dicSagApplied.TryGetValue(m.JointIDs[j], out o)) corr[j] = o; }
                    m_afSagLastJoint = corr;
                    if (sgOut != null)
                    {
                        dbOut = new float[ids.Length];
                        for (int j = 0; j < corr.Length; j++) { int k = Array.IndexOf(ids, m.JointIDs[j]); if (k >= 0) dbOut[k] = (float)(w * m.Jh[j] * sgOut[j]); }
                    }
                }
                else m_afSagLastJoint = new float[0];
                m_fSagLastFloorDz = (float)(w * dzMap);
            }

            private float m_fSagLastFloorDz = 0, m_fSagLastGpStd = float.NaN, m_fSagLastTilt = 0;
            /// <summary>직전 출력 틱에 손목 공구 기울기 보정으로 손목 명령에서 뺀 τ (deg, 전환 무게 포함) — 진단용 (10-15)</summary>
            public float SagComp_LastTilt { get { return m_fSagLastTilt; } }

            // ── 손목 공구 기울기 τ (10-15) ──
            // 모델 m 의 항을 이상 자세에서 (ids·afDeg, 그 밖 관절은 3D 값 또는 사본 ad 의 값). 자세 항이 있으면 중력 지레 g = −L 이 필요하다 —
            //   lev·jids 가 주어지면 (출력 단이 이 틱에 관절 모델로 계산한 것) 그것을, 아니면 관절 모델 ID (없으면 위치 3 + 손목 — SagComp_TiltLeverIds) 로 계산.
            //   ad = 관절값 사본 (예측 — SagFc_Levers, 부작용 없음), null = 3D 관절값 (출력 단 — SagComp_JointLevers). 항이 없거나 손목 없음이면 0
            private double SagComp_TiltAt(CSagComp m, int[] ids, float[] afDeg, float[] lev, int[] jids, double[] ad)
            {
                if (m == null || !m.HasWristTilt) return 0;
                if (m.WristTiltB == null || m.WristTiltB.Length < 2 || m.PosIDs == null || m.PosIDs.Length < 3) return m.WristTiltC;   // 상수만 — 지레가 필요 없다
                if (lev == null || jids == null)
                {
                    jids = SagComp_TiltLeverIds(m);
                    float[] pose = new float[jids.Length];
                    for (int j = 0; j < jids.Length; j++)
                    {
                        int k = (ids == null) ? -1 : Array.IndexOf(ids, jids[j]);
                        pose[j] = (k >= 0 && afDeg != null && k < afDeg.Length) ? afDeg[k] : (ad != null ? SagFc_Get(ad, jids[j]) : GetData(jids[j]));
                    }
                    lev = (ad != null) ? SagFc_Levers(m.FuncNumber, jids, pose, ad) : SagComp_JointLevers(m.FuncNumber, jids, pose);
                }
                int i12 = Array.IndexOf(jids, m.PosIDs[1]), i13 = Array.IndexOf(jids, m.PosIDs[2]);
                double g12 = (i12 >= 0 && i12 < lev.Length) ? -lev[i12] : 0, g13 = (i13 >= 0 && i13 < lev.Length) ? -lev[i13] : 0;
                return m.WristTiltAt(g12, g13);
            }
            // 자세 항의 지레를 잴 관절 (체인 순) — 관절 모델이 있으면 그 ID (출력 단이 같은 목록으로 잰다), 없으면 위치 3 + 손목
            private static int[] SagComp_TiltLeverIds(CSagComp m)
            {
                if (m.HasJoint) return m.JointIDs;
                return new int[] { m.PosIDs[0], m.PosIDs[1], m.PosIDs[2], m.WristID };
            }
            /// <summary>적용 중 모델의 손목 공구 기울기 τ (deg, + = 공구 끝이 베이스 쪽) — 지금 3D 자세에서 (출력 단이 전환을 마친 뒤 손목에서 빼는 값 — 자르기 전).
            /// 모델이 없거나 꺼져 있거나 항이 없으면 0. 관절값 사본으로 계산한다 (부작용 없음 — 어느 스레드에서도 된다). 예외 없음</summary>
            public float SagComp_TiltNow()
            {
                try
                {
                    CSagComp m = m_SagComp;
                    if (m == null || !m.Enabled || !m.HasWristTilt) return 0f;
                    return (float)SagComp_TiltAt(m, null, null, null, null, SagFc_MotCopy());
                }
                catch { return 0f; }
            }
            /// <summary>모델 m 의 τ 를 관절 자세 (ids·q, 그 밖은 지금 3D 값) 에서 — 관절값 사본으로 (부작용 없음). 항이 없으면 0. 예외 없음</summary>
            public float SagComp_TiltOf(CSagComp m, int[] ids, float[] q)
            {
                try
                {
                    if (m == null || !m.HasWristTilt) return 0f;
                    double[] ad = SagFc_MotCopy();
                    if (ids != null && q != null) for (int i = 0; i < ids.Length && i < q.Length; i++) SagFc_Set(ad, ids[i], q[i]);
                    return (float)SagComp_TiltAt(m, ids, q, null, null, ad);
                }
                catch { return 0f; }
            }
            /// <summary>자세 항에 쓰는 중력 지레 g12 = −L12, g13 = −L13 (mm/deg) — 관절 자세 (ids·q, 그 밖은 지금 3D 값) 에서, 모델 m 의 지레 관절 목록으로
            /// (m 이 null 이거나 관절 모델이 없으면 위치 3 + 손목 — 기울기만 있는 모델의 출력 단과 같다). 관절값 사본으로 (부작용 없음). 못 재면 false. 예외 없음</summary>
            public bool SagComp_TiltLevers(CSagComp m, CSagArmCfg c, int[] ids, float[] q, out double g12, out double g13)
            {
                g12 = g13 = double.NaN;
                try
                {
                    if (c == null || c.PosIDs == null || c.PosIDs.Length < 3 || c.NoWrist) return false;
                    int[] jids = (m != null && m.HasJoint) ? m.JointIDs : new int[] { c.PosIDs[0], c.PosIDs[1], c.PosIDs[2], c.WristID };
                    double[] ad = SagFc_MotCopy();
                    if (ids != null && q != null) for (int i = 0; i < ids.Length && i < q.Length; i++) SagFc_Set(ad, ids[i], q[i]);
                    float[] pose = new float[jids.Length];
                    for (int j = 0; j < jids.Length; j++) pose[j] = SagFc_Get(ad, jids[j]);
                    float[] lev = SagFc_Levers((m != null) ? m.FuncNumber : c.FuncNumber, jids, pose, ad);
                    int i12 = Array.IndexOf(jids, c.PosIDs[1]), i13 = Array.IndexOf(jids, c.PosIDs[2]);
                    if (lev == null || i12 < 0 || i13 < 0) return false;
                    g12 = -lev[i12]; g13 = -lev[i13];
                    return !double.IsNaN(g12) && !double.IsNaN(g13);
                }
                catch { return false; }
            }
            /// <summary>직전 출력 틱의 바닥 GP 예측 표준편차 (mm) — 진단용</summary>
            public float SagComp_LastGpStd { get { return m_fSagLastGpStd; } }
            /// <summary>직전 출력 틱에 바닥 보정표로 얹은 끝 높이 보정 (mm) — 진단용</summary>
            public float SagComp_LastFloorDz { get { return m_fSagLastFloorDz; } }

            /// <summary>출력 단에 보정이 걸려 있거나 다음 출력 틱에 걸린다 — 직전 틱에 얹은 보정, 전환 기준(SagComp_SeedFrom 으로 심은 값 포함),
            /// 손목 유지 중 하나라도 0 이 아니다. 모델 켬 여부는 보지 않는다 (CScene_t.SagCompInPlay 가 함께 본다). 버스·3D 를 건드리지 않는다</summary>
            public bool SagComp_HasOffsets
            {
                get
                {
                    lock (m_dicSagDir)
                    {
                        foreach (float v in m_dicSagApplied.Values) if (v != 0f) return true;
                        if (m_dicSagFrom != null) foreach (float v in m_dicSagFrom.Values) if (v != 0f) return true;
                    }
                    return SagComp_WristHoldValue() != 0f;
                }
            }
            /// <summary>직전 출력 틱에 실물 명령에 얹은 관절 보정 (deg, ids 순 — 전환 중 값·SagComp_SeedFrom 으로 심은 값 포함, 손목 유지는 빼고). 없는 축은 0.
            /// 로봇팔 작업이 시작·끝의 서보 목표에 든 보정을 비교할 때 (원위치 확인). 부작용 없음</summary>
            public float[] SagComp_AppliedNow(int[] ids)
            {
                float[] r = new float[ids == null ? 0 : ids.Length];
                lock (m_dicSagDir) for (int i = 0; i < r.Length; i++) { float v; if (m_dicSagApplied.TryGetValue(ids[i], out v)) r[i] = v; }
                return r;
            }
            /// <summary>보정 전환 진행 (0..1) — 출력 단이 다음 틱에 쓸 전환 무게 w 와 같은 값. 1 = 걸린 전환이 없거나 끝났다
            /// (모델이 꺼져 있고 빠질 보정도 없으면 1). 전환·켬끔·방향 추적 초기화(SagComp_JointReset — 보정이 0 에서 다시 들어온다) 뒤
            /// 첫 출력 틱 전에는 0. 출력 틱이 흘러야 나아간다 (멈춰 있으면 그대로) — 모델을 바꾼 뒤 실물에 새 보정이 다 들어갔는지 볼 때
            /// (틈 재기는 이것이 1 이 될 때까지 자세를 보내며 붙잡는다)</summary>
            public double SagComp_BlendProgress
            {
                get
                {
                    lock (m_dicSagDir)
                    {
                        CSagComp m = m_SagComp;
                        bool bModel = m != null && m.Enabled && (m.HasJoint || m.HasWristTilt);
                        if (!bModel && (m_dicSagFrom == null || m_dicSagFrom.Count == 0)) return 1.0;   // 들어올 보정도 빠질 보정도 없다
                        if (!m_bSagBlendRun) return 0.0;                                                 // 전환 뒤 첫 출력 틱 전
                        return Math.Min(1.0, m_dSagBlendMs / SAG_BLEND_MS);
                    }
                }
            }

            // ────────────────────────────────────────────────────────────
            // 출력 단 예측·되짚기·전환 씨앗 (10-03) — 공용 도구. 서보 목표에서 3D 자세 h 를 h + C(h) = 목표 로 되짚고(SagComp_InvertGoal) 전환 기준을
            //   관절마다 목표 − h 로 심으면(SagComp_SeedFrom) 다음 출력 틱 = 목표, 그 뒤에도 C(h) = 목표 − h 라 보정이 두 번 얹히지 않는다.
            //   이어받기(CArmConn_t: 연결 때·토크 켤 때·움직이기 전 확인)는 이것을 쓰지 않는다 (10-03 설계 바꿈): 바닥 보정은 엔코더로 보이지 않는
            //   휨을 메우므로 실물 지금 위치로는 해가 여럿일 때 어느 해인지 가릴 수 없다 — 이어받기는 서보 목표를 그대로 3D 로 삼고 걸린 보정을
            //   페이드 없이 끈다 (SagComp_OffForAdopt).
            //   예측·되짚기는 부작용이 없다 — 방향·속도 추적, 전환 진행·기준, 쌍둥이, 진단값을 건드리지 않고 관절값 사본으로 계산한다
            //   (모션 스레드의 출력 틱과 겹쳐 불러도 된다). 첫닿음 모델이 아직 준비 전이면 준비한다 (전환 때와 같은 일 — 결과가 같다).
            // ────────────────────────────────────────────────────────────
            /// <summary>정지 자세에서 출력 단이 전환을 마친 뒤(w = 1) 얹을 관절 보정 (deg, ids 순, 부호 = 명령에 더할 값) — 적용 중 모델로.
            /// = 관절 모델(중력·데드밴드, MaxAbsJointDeg 로 자름) + 바닥 보정(첫닿음 모델 / GP / 반경 보정표 → 관절 12·13·14, 축마다 MaxAbsJointDeg 로 자름)
            /// + 손목 공구 기울기, 예측 보정이 켜져 있으면 출력 단과 같이 다듬은 값 (SagComp_PredRefine, 10-16).
            /// 앞서 보내기(속도·가속도)와 손목 유지는 넣지 않는다. afIdeal = ids 순 이상(3D) 관절각, afSigma = ids 순 이동 방향 σ (−1..+1, null·NaN = 0).
            /// ids 밖 관절은 지금 3D 값. 모델이 없거나 꺼져 있으면 0 배열, 입력이 틀리면 null. 부작용 없음 — 어느 스레드에서 불러도 된다. 예외 없음.</summary>
            public float[] SagComp_PredictJointCorr(int[] ids, float[] afIdeal, float[] afSigma)
            {
                CSagComp m = m_SagComp;
                if (ids == null || afIdeal == null || afIdeal.Length != ids.Length) return null;
                if (m == null || !m.Enabled || (!m.HasJoint && !m.HasWristTilt)) return new float[ids.Length];
                return SagComp_PredictJointCorr(m, ids, afIdeal, afSigma);
            }
            /// <summary>SagComp_PredictJointCorr 와 같고 모델을 준다 — 그 모델을 켜서 적용하면 얹을 보정 (Enabled 플래그는 보지 않는다,
            /// 바닥 부분은 그 모델의 FloorMapEnabled·첫닿음 fcon 을 따른다). 관절 모델이 없으면 0 배열. 부작용 없음. 예외 없음.</summary>
            public float[] SagComp_PredictJointCorr(CSagComp m, int[] ids, float[] afIdeal, float[] afSigma)
            {
                try
                {
                    if (ids == null || afIdeal == null || afIdeal.Length != ids.Length) return null;
                    float[] r = new float[ids.Length];
                    if (m == null || (!m.HasJoint && !m.HasWristTilt)) return r;
                    Dictionary<int, float> t = SagComp_PredictCore(m, m.Fc, ids, afIdeal, afSigma);
                    for (int i = 0; i < ids.Length; i++) { float v; if (t.TryGetValue(ids[i], out v)) r[i] = v; }
                    return r;
                }
                catch { return null; }
            }
            /// <summary>서보가 붙잡은 목표 → 3D 이상 자세 h: h + C(h) = 목표 (C = SagComp_PredictJointCorr, 적용 중 모델, σ = afSigma) 를
            /// 고정점 반복 h ← 목표 − C(h) 로 푼다 (최대 10번, 앞뒤 차가 0.01° 안이면 끝 — 수렴하지 않으면 차가 잘 줄지 않을 때 반씩 가며 한 번 더 10번,
            /// 그래도 안 되면 뉴턴 — 수치 야코비안, 걸음 2° 까지, 같은 0.01° 기준 (10-03: C 의 기울기가 1 을 넘는 바닥 안쪽 자세)).
            /// 모델이 없거나 꺼져 있으면, 그래도 수렴하지 않으면(C 가 끊기는 곳을 건너야 닿는 목표) null. 부작용 없음. 예외 없음.</summary>
            public float[] SagComp_InvertGoal(int[] ids, float[] afGoal, float[] afSigma) { return SagComp_InvertGoal(ids, afGoal, afSigma, null); }
            /// <summary>SagComp_InvertGoal 과 같고 abFixed[i] 가 true 인 축은 h = 목표 그대로 둔다 (보정 없이 붙잡은 축 — 토크가 꺼져 있다 켠 축의
            /// 지금 위치 등. 다른 축의 보정 계산에는 그 값이 들어간다)</summary>
            public float[] SagComp_InvertGoal(int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed)
            {
                return SagComp_InvertGoal(ids, afGoal, afSigma, abFixed, null);
            }
            /// <summary>SagComp_InvertGoal 과 같고 반복을 시작할 자세를 준다 (ids 순 — null 이거나 길이가 다르면 목표에서). 보정이 자세에 따라 끊기는
            /// 모델(작업 범위·학습점 밖에서 빠지는 바닥 보정 등)은 h + C(h) = 목표 의 해가 여럿일 수 있다 — 실물의 지금 위치(처짐을 보정이 메우므로 원래
            /// 3D 자세 근처)에서 찾은 해 가운데 시작 자세에 가장 가까운 해를 고른다: 시작 자세에서 뉴턴·그대로 → (0.5° 안의 해가 없으면) 내려가기(기울기 없이
            /// 잔차 제곱합을 줄여 가는 축별 탐색)·반씩 → 목표에서 그대로·반씩·뉴턴 → 그래도 0.5° 안의 해가 없으면 둘레 훑기 (바닥 보정 축 셋을 시작 자세 둘레
            /// 0.5° 간격으로 — 그때까지 찾은 가장 가까운 해까지의 거리 안, 3° 까지). 10-03 검토: 바닥 GP 가 학습 자세 둘레에만 보정을 내는 먼 바닥 자세
            /// r250~260 z60 에서 고정점은 시작 자세 0.1~1.5° 옆에서도 바닥 보정이 빠지는 둘째 해(약 6° 떨어짐, TCP 15~19mm 위)로 갔다.
            /// 어느 해든 다음 틱 = 목표, 보정은 두 번 얹히지 않는다</summary>
            public float[] SagComp_InvertGoal(int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] afStart)
            {
                try
                {
                    CSagComp m = m_SagComp;
                    if (ids == null || afGoal == null || afGoal.Length != ids.Length) return null;
                    if (m == null || !m.Enabled || (!m.HasJoint && !m.HasWristTilt)) return null;
                    CSagFc fc = m.Fc;                                           // 한 번만 읽는다 (다시 학습이 다른 스레드에서 바꿔 끼울 수 있다)
                    bool bStart = afStart != null && afStart.Length == ids.Length;
                    if (!bStart)
                    {
                        // 시작 자세가 없으면 목표에서 그대로 → 반씩 → 뉴턴 (앞의 것이 수렴하지 않을 때만 다음 — 예전과 같은 풀이)
                        float[] h0 = SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afGoal, false);
                        if (h0 == null) h0 = SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afGoal, true);
                        if (h0 == null) h0 = SagComp_InvertNewton(m, fc, ids, afGoal, afSigma, abFixed, afGoal);
                        return h0;
                    }
                    bool[] mask = SagComp_InvertMask(m, ids, abFixed);   // 거리를 재는 축 = 보정에 관여하는 축 중 고정 아닌 것 (그리퍼처럼 보정 없는 축은 해가 늘 목표라 뺀다)
                    float[] best = null;
                    double dBest = double.MaxValue;
                    // 1) 시작 자세에서 뉴턴(가까운 해로 한두 걸음 — 실물 r230 z40: 한 걸음에 잔차 0.001°)·그대로
                    var cands = new List<float[]>();
                    cands.Add(SagComp_InvertNewton(m, fc, ids, afGoal, afSigma, abFixed, afStart));
                    cands.Add(SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afStart, false));
                    SagComp_InvertPick(cands, afStart, mask, ref best, ref dBest);
                    if (best != null && dBest <= 0.5) return best;
                    // 2) 내려가기·반씩 — 바닥 보정이 축마다 MaxAbsJointDeg 로 잘린 곳의 가장자리 바로 밖에서 시작하면 뉴턴의 야코비안이 반대쪽
                    //    기울기를 보고, 고정점은 바닥 보정이 빠지는 먼 해로 미끄러진다
                    cands.Clear();
                    cands.Add(SagComp_InvertDescend(m, fc, ids, afGoal, afSigma, abFixed, afStart, 1200));
                    cands.Add(SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afStart, true));
                    SagComp_InvertPick(cands, afStart, mask, ref best, ref dBest);
                    if (best != null && dBest <= 0.5) return best;
                    // 3) 목표에서 (그대로 → 반씩 → 뉴턴)
                    float[] h = SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afGoal, false);
                    if (h == null) h = SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, afGoal, true);
                    if (h == null) h = SagComp_InvertNewton(m, fc, ids, afGoal, afSigma, abFixed, afGoal);
                    cands.Clear(); cands.Add(h);
                    SagComp_InvertPick(cands, afStart, mask, ref best, ref dBest);
                    if (best != null && dBest <= 0.5) return best;
                    // 4) 둘레 훑기 — 보정이 좁은 곳에만 있으면(학습 자세 둘레의 바닥 GP) 그 안의 해는 밖에서 내려가도·고정점으로도 닿지 않는다.
                    //    더 가까운 해는 지금 가장 가까운 해까지의 거리 안에만 있다 (없으면 3°)
                    double fHalf = Math.Min(3.0, best != null ? dBest : 3.0);
                    SagComp_InvertPick(SagComp_InvertScan(m, fc, ids, afGoal, afSigma, abFixed, afStart, fHalf), afStart, mask, ref best, ref dBest);
                    return best;
                }
                catch { return null; }
            }
            // 보정에 관여하는 축 ID — 관절 모델 축 + 바닥 보정 축(위치 셋·손목)
            private static List<int> SagComp_InvertCorrIds(CSagComp m)
            {
                var l = new List<int>(m.JointIDs);
                if (m.PosIDs != null && m.PosIDs.Length >= 3) { l.Add(m.PosIDs[0]); l.Add(m.PosIDs[1]); l.Add(m.PosIDs[2]); l.Add(m.WristID); }
                return l;
            }
            // 되짚기가 움직이는 축 (ids 색인 마스크) — 보정에 관여하는 축 중 고정 아닌 것 (나머지 축의 해는 늘 목표)
            private static bool[] SagComp_InvertMask(CSagComp m, int[] ids, bool[] abFixed)
            {
                List<int> corr = SagComp_InvertCorrIds(m);
                bool[] mask = new bool[ids.Length];
                for (int i = 0; i < ids.Length; i++) mask[i] = corr.Contains(ids[i]) && !(abFixed != null && i < abFixed.Length && abFixed[i]);
                return mask;
            }
            // 두 자세의 거리 (deg) — 축마다 차의 최대 (mask 가 있으면 그 축만, 수가 아닌 값의 축은 뺀다)
            private static double SagComp_InvertDist(float[] h, float[] afStart, bool[] mask)
            {
                double d = 0;
                for (int i = 0; i < h.Length && i < afStart.Length; i++)
                {
                    if (mask != null && (i >= mask.Length || !mask[i])) continue;
                    if (float.IsNaN(afStart[i]) || float.IsInfinity(afStart[i]) || float.IsNaN(h[i]) || float.IsInfinity(h[i])) continue;
                    d = Math.Max(d, Math.Abs((double)h[i] - afStart[i]));
                }
                return d;
            }
            // 후보 해 중 시작 자세에 가장 가까운 것을 best 로 (수렴하지 않은 후보 = null 은 뺀다)
            private static void SagComp_InvertPick(List<float[]> cands, float[] afStart, bool[] mask, ref float[] best, ref double dBest)
            {
                if (cands == null) return;
                foreach (float[] c in cands)
                {
                    if (c == null) continue;
                    double d = SagComp_InvertDist(c, afStart, mask);
                    if (d < dBest) { dBest = d; best = c; }
                }
            }
            // 내려가기 되짚기 (보정이 잘리거나 끊겨 기울기가 갑자기 바뀌는 곳에서 뉴턴·고정점이 엉뚱한 쪽으로 갈 때) — 기울기를 쓰지 않는 무늬 탐색:
            //   바닥 보정 축 셋(위치 둘째·셋째·손목)은 {−1, 0, +1} 조합 방향(셋이면 26 — 축끼리 엮인 비스듬한 골짜기도 따라간다), 다른 보정 축은 하나씩 ±로
            //   지금 걸음만큼 가 보고 잔차 제곱합 Σ F² 이 가장 많이 주는 쪽으로 간다. 어느 쪽도 줄지 않으면 걸음을 반으로 (0.5° → 0.0005°).
            //   시작 자세에서 내려가 닿는 해(가까운 해)를 찾는다. max|F| < 0.01° 면 끝 (같은 기준), 평가 nMaxEval 번까지 — 못 닿으면 null
            private float[] SagComp_InvertDescend(CSagComp m, CSagFc fc, int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] afStart, int nMaxEval)
            {
                int n = ids.Length;
                float[] h = (float[])afStart.Clone();
                List<int> corrIds = SagComp_InvertCorrIds(m);
                int[] flIds = (m.PosIDs != null && m.PosIDs.Length >= 3) ? new int[] { m.PosIDs[1], m.PosIDs[2], m.WristID } : new int[0];
                var fl = new List<int>(); var other = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    bool bFix = abFixed != null && i < abFixed.Length && abFixed[i];
                    if (bFix || float.IsNaN(h[i]) || float.IsInfinity(h[i]) || !corrIds.Contains(ids[i])) h[i] = afGoal[i];
                    if (bFix || !corrIds.Contains(ids[i])) continue;
                    if (Array.IndexOf(flIds, ids[i]) >= 0 && fl.Count < 3) fl.Add(i); else other.Add(i);
                }
                var dirs = new List<int[]>();                                   // 방향마다 ids 길이의 −1/0/+1
                int nComb = 1;
                for (int a = 0; a < fl.Count; a++) nComb *= 3;
                for (int code = 0; code < nComb; code++)
                {
                    int[] d = new int[n];
                    int c = code; bool bAny = false;
                    for (int a = 0; a < fl.Count; a++) { d[fl[a]] = (c % 3) - 1; if (d[fl[a]] != 0) bAny = true; c /= 3; }
                    if (bAny) dirs.Add(d);
                }
                foreach (int a in other) { int[] dp = new int[n]; dp[a] = 1; dirs.Add(dp); int[] dm = new int[n]; dm[a] = -1; dirs.Add(dm); }
                double[] F = SagComp_InvertResidual(m, fc, ids, afGoal, afSigma, abFixed, h);
                if (F == null) return null;
                if (dirs.Count == 0) return SagComp_MaxAbs(F) < 0.01 ? h : null;
                double S = 0; foreach (double v in F) S += v * v;
                double step = 0.5;
                int nEval = 1;
                while (SagComp_MaxAbs(F) >= 0.01 && step >= 0.0005 && nEval < nMaxEval)
                {
                    float[] hBest = null; double[] FBest = null; double SBest = S;
                    foreach (int[] d in dirs)
                    {
                        float[] hn = (float[])h.Clone();
                        for (int i = 0; i < n; i++) if (d[i] != 0) hn[i] = (float)(h[i] + d[i] * step);
                        double[] Fn = SagComp_InvertResidual(m, fc, ids, afGoal, afSigma, abFixed, hn);
                        nEval++;
                        if (Fn == null) continue;
                        double Sn = 0; foreach (double v in Fn) Sn += v * v;
                        if (Sn < SBest) { SBest = Sn; hBest = hn; FBest = Fn; }
                    }
                    if (hBest != null) { h = hBest; F = FBest; S = SBest; }
                    else step *= 0.5;
                }
                return SagComp_MaxAbs(F) < 0.01 ? h : null;
            }
            // 둘레 훑기 되짚기 (시작 자세 둘레에서 다른 풀이가 가까운 해를 못 찾을 때) — 바닥 보정 축 셋(위치 둘째·셋째·손목, 고정 아닌 것)을
            //   시작 자세 ± fHalf 안에서 0.5° 이하 간격으로 훑는다 (다른 보정 축은 시작 자세, 보정 없는 축·고정 축은 목표). 점마다
            //   ① 고정점 한 걸음의 상 = 목표 − C(점) — 보정이 거의 일정한 구역(잘린 평평한 곳·보정이 없는 곳) 안의 점은 상이 그 구역의 해다:
            //      서로 다른 상(0.05° 넘게 다른 것, 최대 40개)마다 고정점(안 되면 뉴턴)으로 다듬는다
            //   ② 잔차 제곱합 Σ F² — 훑은 격자에서 이웃보다 작은 점(골짜기, 작은 것부터 3개)에서 내려가기 (보정이 가파른 구역 안의 해 — 고정점·상으로는 안 닿는다).
            //   보정이 학습 자세 둘레의 좁은 곳에만 있어도 그 안에 든 격자점이 하나면 된다. 수렴한 해들을 돌려준다 (평가 최대 약 2200 + 다듬기)
            private List<float[]> SagComp_InvertScan(CSagComp m, CSagFc fc, int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] afStart, double fHalf)
            {
                var roots = new List<float[]>();
                int n = ids.Length;
                List<int> corrIds = SagComp_InvertCorrIds(m);
                float[] b = (float[])afStart.Clone();
                for (int i = 0; i < n; i++)
                {
                    bool bFix = abFixed != null && i < abFixed.Length && abFixed[i];
                    if (bFix || float.IsNaN(b[i]) || float.IsInfinity(b[i]) || !corrIds.Contains(ids[i])) b[i] = afGoal[i];
                }
                var ax = new List<int>();                                              // 훑을 축 (ids 색인) — 바닥 보정이 움직이는 셋
                if (m.PosIDs != null && m.PosIDs.Length >= 3)
                    foreach (int id in new int[] { m.PosIDs[1], m.PosIDs[2], m.WristID })
                    {
                        int k = Array.IndexOf(ids, id);
                        if (k >= 0 && !(abFixed != null && k < abFixed.Length && abFixed[k]) && !ax.Contains(k)) ax.Add(k);
                    }
                if (ax.Count == 0 || !(fHalf > 0)) return roots;
                int nStep = (int)Math.Ceiling(fHalf / 0.5 - 1e-6);
                double dStep = fHalf / Math.Max(1, nStep);
                int nSide = 2 * nStep + 1, nAll = 1;
                for (int a = 0; a < ax.Count; a++) nAll *= nSide;
                var imgs = new List<float[]>();
                double[] sg = new double[nAll];
                for (int code = 0; code < nAll; code++)
                {
                    float[] g = SagComp_ScanPoint(b, ax, code, nSide, nStep, dStep);
                    Dictionary<int, float> cr = SagComp_PredictCore(m, fc, ids, g, afSigma);
                    float[] img = new float[n];
                    bool bOk = true;
                    double s = 0;
                    for (int i = 0; i < n; i++)
                    {
                        bool bFix = abFixed != null && i < abFixed.Length && abFixed[i];
                        float v; if (!cr.TryGetValue(ids[i], out v)) v = 0;
                        img[i] = bFix ? afGoal[i] : afGoal[i] - v;
                        if (float.IsNaN(img[i]) || float.IsInfinity(img[i])) bOk = false;
                        if (!bFix) { double f = (double)g[i] + v - afGoal[i]; s += f * f; }
                    }
                    sg[code] = (bOk && !double.IsNaN(s) && !double.IsInfinity(s)) ? s : double.NaN;
                    if (!bOk) continue;
                    bool bSeen = false;
                    foreach (float[] o in imgs) if (SagComp_InvertDist(img, o, null) < 0.05) { bSeen = true; break; }
                    if (!bSeen && imgs.Count < 40) imgs.Add(img);
                }
                foreach (float[] img in imgs)
                {
                    float[] r = SagComp_InvertCore(m, fc, ids, afGoal, afSigma, abFixed, img, false);
                    if (r == null) r = SagComp_InvertNewton(m, fc, ids, afGoal, afSigma, abFixed, img);
                    SagComp_AddRoot(roots, r);
                }
                // 골짜기 — 이웃(축마다 −1·0·+1 칸) 어느 것보다도 Σ F² 이 작은 격자점, 작은 것부터 3개에서 내려가기
                var mins = new List<KeyValuePair<double, int>>();
                for (int code = 0; code < nAll; code++)
                {
                    if (double.IsNaN(sg[code])) continue;
                    bool bMin = true;
                    int nNb = 1; for (int a = 0; a < ax.Count; a++) nNb *= 3;
                    for (int nb = 0; nb < nNb && bMin; nb++)
                    {
                        int c = code, t = nb, code2 = 0, mul = 1; bool bIn = true, bSelf = true;
                        for (int a = 0; a < ax.Count; a++)
                        {
                            int dg = (c % nSide) + (t % 3) - 1;
                            if ((t % 3) != 1) bSelf = false;
                            if (dg < 0 || dg >= nSide) bIn = false;
                            code2 += dg * mul; mul *= nSide; c /= nSide; t /= 3;
                        }
                        if (!bIn || bSelf || double.IsNaN(sg[code2])) continue;
                        if (sg[code2] < sg[code]) bMin = false;
                    }
                    if (bMin) mins.Add(new KeyValuePair<double, int>(sg[code], code));
                }
                mins.Sort(delegate(KeyValuePair<double, int> x, KeyValuePair<double, int> y) { return x.Key.CompareTo(y.Key); });
                for (int i = 0; i < mins.Count && i < 3; i++)
                    SagComp_AddRoot(roots, SagComp_InvertDescend(m, fc, ids, afGoal, afSigma, abFixed, SagComp_ScanPoint(b, ax, mins[i].Value, nSide, nStep, dStep), 800));
                return roots;
            }
            // 훑기 격자점 — 기준 자세 b 의 축 ax 를 code(nSide 진법 자리)만큼 옮긴 자세
            private static float[] SagComp_ScanPoint(float[] b, List<int> ax, int code, int nSide, int nStep, double dStep)
            {
                float[] g = (float[])b.Clone();
                int c = code;
                for (int a = 0; a < ax.Count; a++) { g[ax[a]] = (float)(b[ax[a]] + ((c % nSide) - nStep) * dStep); c /= nSide; }
                return g;
            }
            // 해 목록에 더한다 (null·이미 있는 해 — 0.02° 안 — 는 빼고)
            private static void SagComp_AddRoot(List<float[]> roots, float[] r)
            {
                if (r == null) return;
                foreach (float[] o in roots) if (SagComp_InvertDist(r, o, null) < 0.02) return;
                roots.Add(r);
            }
            // 고정점 반복 h ← 목표 − C(h) (최대 10번, 앞뒤 차가 0.01° 안이면 끝) — 시작 = afStart (고정 축·수가 아닌 값은 목표). 수렴하지 않으면 null.
            //   bDamp: 차가 잘 줄지 않으면 (바닥 보정이 자세에 가파르게 따라 바뀌는 곳 — C 의 기울기가 −1 가까이라 해 둘레를 오락가락) 반만 간다
            //   (h ← h + ½·(목표 − C(h) − h)). 그대로 반복해 수렴하는 곳은 그대로가 빠르다 (부른 쪽이 그대로 → 반씩 순서로 부른다)
            private float[] SagComp_InvertCore(CSagComp m, CSagFc fc, int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] afStart, bool bDamp)
            {
                float[] h = (float[])afStart.Clone();
                for (int i = 0; i < ids.Length; i++)
                    if ((abFixed != null && i < abFixed.Length && abFixed[i]) || float.IsNaN(h[i]) || float.IsInfinity(h[i])) h[i] = afGoal[i];
                double dStep = 1.0, errPrev = double.MaxValue;
                for (int it = 0; it < 10; it++)
                {
                    Dictionary<int, float> c = SagComp_PredictCore(m, fc, ids, h, afSigma);
                    double err = 0;
                    float[] hn = new float[h.Length];                                   // 목표 − C(h)
                    for (int i = 0; i < ids.Length; i++)
                    {
                        float v; if (!c.TryGetValue(ids[i], out v)) v = 0;
                        hn[i] = (abFixed != null && i < abFixed.Length && abFixed[i]) ? afGoal[i] : afGoal[i] - v;
                        if (float.IsNaN(hn[i]) || float.IsInfinity(hn[i])) return null;
                        err = Math.Max(err, Math.Abs(hn[i] - h[i]));
                    }
                    if (err < 0.01) return hn;
                    if (bDamp && err > 0.7 * errPrev) dStep = 0.5;                        // 차가 잘 줄지 않는다 — 반만 간다
                    errPrev = err;
                    for (int i = 0; i < h.Length; i++) h[i] = (float)(h[i] + dStep * (hn[i] - h[i]));
                }
                return null;
            }
            // 뉴턴 되짚기 (시작 자세가 있으면 먼저 — 그 가까운 해, 없으면 고정점·반씩이 안 될 때) — F(h) = h + C(h) − 목표 = 0 을 보정에 관여하는 축(관절 모델 축·바닥 보정 축 12·13·14·첫닿음 축 중 고정 아닌 것)으로 푼다.
            //   나머지 축은 h = 목표 (보정이 없다 — 그 값은 다른 축의 보정 계산에 그대로 들어간다). 야코비안 = 축마다 0.05° 앞쪽 차분 (C 를 다시 계산),
            //   한 걸음은 축마다 2° 까지로 자르고 max|F| 가 줄지 않으면 반씩 (최대 6번). 최대 20걸음, max|F| < 0.01° 면 끝 (고정점과 같은 기준).
            //   C 가 끊기는 곳(바닥 GP 두 상태 띠 경계 등)을 건너야만 닿는 목표면 수렴하지 않는다 → null (부른 쪽이 정한다 — 공용 도구.
            //   서보 목표 이어받기(CArmConn_t)는 이 되짚기를 쓰지 않는다: 3D = 서보 목표, 걸려 있던 보정은 페이드 없이 끔)
            private float[] SagComp_InvertNewton(CSagComp m, CSagFc fc, int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] afStart)
            {
                int n = ids.Length;
                float[] h = (float[])afStart.Clone();
                var rel = new List<int>();                                              // 풀 축 (ids 색인)
                var corrIds = new List<int>(m.JointIDs);
                if (m.PosIDs != null && m.PosIDs.Length >= 3) { corrIds.Add(m.PosIDs[0]); corrIds.Add(m.PosIDs[1]); corrIds.Add(m.PosIDs[2]); corrIds.Add(m.WristID); }
                for (int i = 0; i < n; i++)
                {
                    bool bFix = abFixed != null && i < abFixed.Length && abFixed[i];
                    if (bFix || float.IsNaN(h[i]) || float.IsInfinity(h[i]) || !corrIds.Contains(ids[i])) h[i] = afGoal[i];
                    if (!bFix && corrIds.Contains(ids[i])) rel.Add(i);
                }
                double[] F = SagComp_InvertResidual(m, fc, ids, afGoal, afSigma, abFixed, h);
                if (F == null) return null;
                if (rel.Count == 0) return SagComp_MaxAbs(F) < 0.01 ? h : null;
                const float EPS = 0.05f;
                const double STEP_MAX = 2.0;
                int k = rel.Count;
                for (int it = 0; it < 20; it++)
                {
                    double err = SagComp_MaxAbs(F);
                    if (err < 0.01) return h;
                    var J = new double[k, k];
                    for (int b = 0; b < k; b++)
                    {
                        float[] hp = (float[])h.Clone(); hp[rel[b]] += EPS;
                        double[] Fp = SagComp_InvertResidual(m, fc, ids, afGoal, afSigma, abFixed, hp);
                        if (Fp == null) return null;
                        for (int a = 0; a < k; a++) J[a, b] = (Fp[rel[a]] - F[rel[a]]) / EPS;
                    }
                    var rhs = new double[k];
                    for (int a = 0; a < k; a++) rhs[a] = -F[rel[a]];
                    double[] d = SagComp_SolveDense(J, rhs);
                    if (d == null) return null;
                    double dm = 0; foreach (double v in d) dm = Math.Max(dm, Math.Abs(v));
                    double sc = dm > STEP_MAX ? STEP_MAX / dm : 1.0;
                    bool bTook = false;
                    for (int ls = 0; ls < 6 && !bTook; ls++, sc *= 0.5)
                    {
                        float[] hn = (float[])h.Clone();
                        for (int a = 0; a < k; a++) hn[rel[a]] = (float)(h[rel[a]] + sc * d[a]);
                        double[] Fn = SagComp_InvertResidual(m, fc, ids, afGoal, afSigma, abFixed, hn);
                        if (Fn == null) return null;
                        if (SagComp_MaxAbs(Fn) < err) { h = hn; F = Fn; bTook = true; }
                    }
                    if (!bTook) return null;                                            // 어느 걸음도 잔차를 줄이지 못함 — 끊긴 곳
                }
                return SagComp_MaxAbs(F) < 0.01 ? h : null;
            }
            // 되짚기 잔차 F_i = h_i + C_i(h) − 목표_i (고정 축 0) — 수가 아니면 null
            private double[] SagComp_InvertResidual(CSagComp m, CSagFc fc, int[] ids, float[] afGoal, float[] afSigma, bool[] abFixed, float[] h)
            {
                Dictionary<int, float> c = SagComp_PredictCore(m, fc, ids, h, afSigma);
                var F = new double[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    if (abFixed != null && i < abFixed.Length && abFixed[i]) continue;
                    float v; if (!c.TryGetValue(ids[i], out v)) v = 0;
                    double f = (double)h[i] + v - afGoal[i];
                    if (double.IsNaN(f) || double.IsInfinity(f)) return null;
                    F[i] = f;
                }
                return F;
            }
            private static double SagComp_MaxAbs(double[] v)
            {
                double m = 0; foreach (double x in v) m = Math.Max(m, Math.Abs(x));
                return m;
            }
            // 작은 연립방정식 A x = b (부분 피벗 가우스 소거) — 특이하면 null
            private static double[] SagComp_SolveDense(double[,] A0, double[] b0)
            {
                int n = b0.Length;
                var A = (double[,])A0.Clone(); var b = (double[])b0.Clone();
                for (int c = 0; c < n; c++)
                {
                    int p = c; double best = Math.Abs(A[c, c]);
                    for (int r = c + 1; r < n; r++) if (Math.Abs(A[r, c]) > best) { best = Math.Abs(A[r, c]); p = r; }
                    if (!(best > 1e-9)) return null;
                    if (p != c) { for (int j = 0; j < n; j++) { double t = A[c, j]; A[c, j] = A[p, j]; A[p, j] = t; } double tb = b[c]; b[c] = b[p]; b[p] = tb; }
                    for (int r = c + 1; r < n; r++)
                    {
                        double f = A[r, c] / A[c, c];
                        if (f == 0) continue;
                        for (int j = c; j < n; j++) A[r, j] -= f * A[c, j];
                        b[r] -= f * b[c];
                    }
                }
                var x = new double[n];
                for (int r = n - 1; r >= 0; r--)
                {
                    double s = b[r];
                    for (int j = r + 1; j < n; j++) s -= A[r, j] * x[j];
                    x[r] = s / A[r, r];
                }
                return x;
            }
            /// <summary>전환 씨앗 — 다음 출력 틱이 이 값에서 시작하게 전환을 다시 건다 (관절마다):
            /// 전환 기준(from) = 지금 걸려 있는 보정(직전 출력 틱 값 — 그대로 이어 간다), 단 ids 의 축은 afFrom[i] (NaN = 그 축도 걸려 있는 값 그대로).
            /// afHold[i] 가 수이면 그 축의 방향 추적을 그 자세에서 σ = afSigma[i] (null·NaN = 0) 로 새로 시작하고 속도 추적을 비운다 (NaN = 그 축의 추적 그대로).
            /// 전환 진행은 0 에서 다시 — 출력 틱이 흐르며 1초에 걸쳐 지금 모델의 보정으로 넘어간다. 세대 + 1. 손목 유지는 건드리지 않는다.
            /// 쓰임 (공용 도구): h = SagComp_InvertGoal(목표) 로 3D·붙잡은 자세를 맞추고 afFrom = 목표 − h 로 심으면 다음 틱 = 목표,
            /// 그 뒤에도 C(h) = 목표 − h 라 움직이지 않는다 (보정이 두 번 얹히지 않음). 토크가 꺼졌다 켠 축은 afFrom = 0 (보정이 0 부터 들어온다).
            /// 서보 목표 이어받기(CArmConn_t)는 이것을 쓰지 않는다 — 보정을 페이드 없이 끄고 목표를 그대로 3D 로 삼는다 (SagComp_OffForAdopt, 10-03).
            /// SagComp_JointReset 뒤에 부른다 (초기화가 기준을 지운다). 모션 중에 불러도 된다 (출력 틱과 같은 lock). 예외 없음.</summary>
            public void SagComp_SeedFrom(int[] ids, float[] afHold, float[] afFrom, float[] afSigma)
            {
                if (ids == null) return;
                lock (m_dicSagDir)
                {
                    var from = new Dictionary<int, float>(m_dicSagApplied);
                    for (int i = 0; i < ids.Length; i++)
                    {
                        int id = ids[i];
                        if (afFrom != null && i < afFrom.Length && !float.IsNaN(afFrom[i]) && !float.IsInfinity(afFrom[i]))
                        { if (afFrom[i] == 0f) from.Remove(id); else from[id] = afFrom[i]; }
                        if (afHold != null && i < afHold.Length && !float.IsNaN(afHold[i]) && !float.IsInfinity(afHold[i]))
                        {
                            float sg = (afSigma != null && i < afSigma.Length && !float.IsNaN(afSigma[i])) ? Math.Max(-1f, Math.Min(1f, afSigma[i])) : 0f;
                            m_dicSagAnchor[id] = afHold[i]; m_dicSagPrev[id] = afHold[i]; m_dicSagSigma[id] = sg;
                            m_dicSagDir[id] = sg > 0.5f ? 1 : (sg < -0.5f ? -1 : 0);
                            m_dicSagVelDeg.Remove(id); m_dicSagVelT.Remove(id); m_dicSagOmega.Remove(id); m_dicSagAlpha.Remove(id);
                        }
                    }
                    m_dicSagFrom = from;
                    m_dicSagApplied.Clear();
                    foreach (KeyValuePair<int, float> kv in from) m_dicSagApplied[kv.Key] = kv.Value;   // 실물에 걸린 보정 = 심은 값 (다음 전환도 여기서)
                    m_bSagBlendRun = false; m_dSagBlendMs = 0;
                    m_nSagSwitchGen++; m_nSagHardGen++;
                }
            }
            // 출력 단 목표 보정의 사본 계산 — SagComp_JointOutModel 의 target 과 같은 식 (앞서 보내기 없음, σ 는 주어진 값).
            //   관절값 사본(지금 3D)에 ids = afIdeal 을 넣어 계산한다 — 지레(SagFc_Levers)·TCP(SagFc_TcpRZ)·∂z/∂q 는 출력 단의 SetData 방식과 같은 값.
            //   반환 = 관절 ID → 보정 (deg)
            private Dictionary<int, float> SagComp_PredictCore(CSagComp m, CSagFc fc, int[] ids, float[] afIdeal, float[] afSigma)
            {
                var target = new Dictionary<int, float>();
                double[] ad = SagFc_MotCopy();
                for (int i = 0; i < ids.Length; i++) SagFc_Set(ad, ids[i], afIdeal[i]);
                if (!m.HasJoint) { SagComp_PredictTilt(m, ids, afIdeal, null, null, ad, target); return target; }   // 손목 공구 기울기만 있는 모델 (10-15)
                int nj = m.JointIDs.Length;
                float[] pose = new float[nj];
                for (int j = 0; j < nj; j++) { int k = Array.IndexOf(ids, m.JointIDs[j]); pose[j] = k >= 0 ? afIdeal[k] : SagFc_Get(ad, m.JointIDs[j]); }
                float[] lev = SagFc_Levers(m.FuncNumber, m.JointIDs, pose, ad);
                float[] sgj = new float[nj];                                                     // 이동 방향 σ (JointIDs 순 — 예측 보정이 쓴다)
                for (int j = 0; j < nj; j++)
                {
                    int k = Array.IndexOf(ids, m.JointIDs[j]);
                    float sg = (afSigma != null && k >= 0 && k < afSigma.Length && !float.IsNaN(afSigma[k])) ? afSigma[k] : 0f;
                    if (sg > 1f) sg = 1f; if (sg < -1f) sg = -1f;
                    sgj[j] = sg;
                    double c = m.JointCorrStatic(j, lev[j], sg);
                    if (c > m.MaxAbsJointDeg) c = m.MaxAbsJointDeg; if (c < -m.MaxAbsJointDeg) c = -m.MaxAbsJointDeg;
                    target[m.JointIDs[j]] = (float)c;
                }
                bool bFloorP = false; double dzP = 0; int[] fidP = null; float[] fqP = null;      // 1차 보정이 얹은 바닥 항 (예측 보정이 쓴다)
                bool bFc = fc != null && fc.HasData && fc.Enabled;
                if ((bFc || (m.HasGp ? m.Gp.Enabled : m.HasFloorMap)) && m.FloorMapEnabled && m.PosIDs != null && m.PosIDs.Length >= 3)
                {
                    int[] fid = { m.PosIDs[1], m.PosIDs[2], m.WristID };
                    float[] fq = new float[3];
                    for (int k = 0; k < 3; k++)
                    {
                        int fi = Array.IndexOf(ids, fid[k]), jm = Array.IndexOf(m.JointIDs, fid[k]);
                        fq[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afIdeal[fi] : SagFc_Get(ad, fid[k]));
                    }
                    double r0, z0, dz = 0;
                    SagFc_TcpRZ(m.FuncNumber, ad, fid, fq, out r0, out z0);
                    if (bFc)
                    {
                        CSagFc f = SagFc_Ready(m, fc);
                        if (f != null)
                        {
                            int[] fcIds = SagFc_Ids(m);
                            float[] q4 = new float[4];
                            bool bSame = nj == 4 && lev.Length == 4;
                            for (int k = 0; k < 4; k++)
                            {
                                int jm = Array.IndexOf(m.JointIDs, fcIds[k]), fi = Array.IndexOf(ids, fcIds[k]);
                                q4[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afIdeal[fi] : SagFc_Get(ad, fcIds[k]));
                                if (bSame && m.JointIDs[k] != fcIds[k]) bSame = false;
                            }
                            float[] l4 = bSame ? lev : SagFc_Levers(m.FuncNumber, fcIds, q4, ad);
                            double[] jz = SagFc_TcpZJac(m.FuncNumber, ad, new int[] { fcIds[1], fcIds[2], fcIds[3] }, fq);
                            double std;
                            dz = f.Correction(l4[1], l4[2], l4[3], q4[0], jz, r0, z0, fq[0] + fq[1] + fq[2], out std);
                        }
                    }
                    else if (m.HasGp)
                    {
                        double[] gx = new double[m.Gp.FeatIDs.Length];
                        for (int k = 0; k < gx.Length; k++)
                        {
                            int jm = Array.IndexOf(m.JointIDs, m.Gp.FeatIDs[k]), fi = Array.IndexOf(ids, m.Gp.FeatIDs[k]);
                            gx[k] = jm >= 0 ? pose[jm] : (fi >= 0 ? afIdeal[fi] : SagFc_Get(ad, m.Gp.FeatIDs[k]));
                        }
                        double gsd; dz = m.Gp.Correction(gx, r0, out gsd);
                    }
                    else dz = m.FloorDzAt(r0);
                    double[] dq;
                    if (Math.Abs(dz) > 1e-3 && SagComp_FloorJointDeltaCopy(m.FuncNumber, ad, fid, fq, dz, out dq))
                    {
                        bFloorP = true; dzP = dz; fidP = fid; fqP = fq;
                        for (int k = 0; k < 3; k++)
                        {
                            if (fid[k] < 0) continue;                                        // 꺾는 손목 없음 — 피치 두 관절로만
                            float t0; target.TryGetValue(fid[k], out t0);
                            target[fid[k]] = t0 + (float)Math.Max(-m.MaxAbsJointDeg, Math.Min(m.MaxAbsJointDeg, dq[k]));
                        }
                    }
                }
                double tauP = SagComp_PredictTilt(m, ids, afIdeal, lev, m.JointIDs, ad, target);   // 손목 공구 기울기 (출력 단과 같은 식·같은 자르기)
                // 예측 보정 (10-16) — 출력 단과 같다 (앞서 보내기 없음)
                if (m.Predictive)
                {
                    float[] info;
                    SagComp_PredRefine(m, ad, pose, sgj, null, bFloorP, dzP, fidP, fqP, tauP, target, out info);
                }
                return target;
            }
            // 손목 공구 기울기 보정 (10-15) — 출력 단과 같다: 손목 += −τ, MaxAbsJointDeg 로 자른 뒤 더한다. 관절값 사본 ad 로 (부작용 없음).
            //   반환 = τ (deg, 항이 없으면 NaN — 예측 보정이 쓴다)
            private double SagComp_PredictTilt(CSagComp m, int[] ids, float[] afIdeal, float[] lev, int[] jids, double[] ad, Dictionary<int, float> target)
            {
                if (m == null || !m.HasWristTilt) return double.NaN;
                double tau = SagComp_TiltAt(m, ids, afIdeal, lev, jids, ad);
                float t0; target.TryGetValue(m.WristID, out t0);
                target[m.WristID] = t0 + (float)Math.Max(-m.MaxAbsJointDeg, Math.Min(m.MaxAbsJointDeg, -tau));
                return tau;
            }
            // SagComp_FloorJointDelta 와 같은 풀이 — 관절값 사본으로 (3D 관절값을 바꾸지 않는다, 어느 스레드에서도 된다)
            private bool SagComp_FloorJointDeltaCopy(int nFunc, double[] adBase, int[] fid, float[] q, double dz, out double[] dq)
            {
                dq = null;
                const float D = 0.5f;
                double r0, z0, ra, za, rb, zb;
                SagFc_TcpRZ(nFunc, adBase, fid, q, out r0, out z0);
                SagFc_TcpRZ(nFunc, adBase, fid, new float[] { q[0] + D, q[1], q[2] - D }, out ra, out za);
                SagFc_TcpRZ(nFunc, adBase, fid, new float[] { q[0], q[1] + D, q[2] - D }, out rb, out zb);
                double j11 = (ra - r0) / D, j12 = (rb - r0) / D, j21 = (za - z0) / D, j22 = (zb - z0) / D;
                double det = j11 * j22 - j12 * j21;
                if (Math.Abs(det) < 1e-6) return false;
                double a = (-j12 * dz) / det, b = (j11 * dz) / det;
                double r1, z1;
                SagFc_TcpRZ(nFunc, adBase, fid, new float[] { q[0] + (float)a, q[1] + (float)b, q[2] - (float)(a + b) }, out r1, out z1);
                double er = r0 - r1, ez = (z0 + dz) - z1;
                a += (j22 * er - j12 * ez) / det; b += (j11 * ez - j21 * er) / det;
                dq = new double[] { a, b, -a - b };
                return true;
            }

            // ────────────────────────────────────────────────────────────
            // 예측 보정 (10-16, CSagComp.Predictive) — 1차 보정을 처짐 쌍둥이 예측으로 다듬는다: 쌍둥이(보낸 명령) = 이상 자세
            //   1차 보정은 이상 자세에서 잰 지레로 관절 처짐을 메우고, 바닥 dz 는 '이상 자세에서 끝을 dz 올리는 관절 변화', 공구 기울기는 −τ 다.
            //   쌍둥이(SagTwin_Predict)는 관절 처짐을 보낸 명령의 지레로 내고, 엔코더 밖 처짐을 '이상 자세에서 끝을 dz 내리는 관절 변화' 와 τ 로 더한다 —
            //   그래서 보정 켬에도 쌍둥이가 2차 잔차만큼 3D 에서 벗어났다 (10-16 격자 r120~260·요 0/±30·z 바닥+5~110: 끝 평균 0.78mm·공구 0.24°,
            //   r260 z90 은 바닥 관절 변화가 MaxAbsJointDeg 에 잘려 7.1mm·2.7° — 공구 끝이 베이스 쪽).
            //   여기서는 보낸 명령 q = 이상 + c 를 쌍둥이 식에 넣고 c ← 관절 보정(지레 = q) + 바닥 몫 −(끝을 dz 내리는 관절 변화) + 기울기 몫 −τ 를 되풀이한다
            //   (고정점 — 지레는 자세에 천천히 바뀌어 한두 번이면 수렴). 자르기는 항마다 1차 보정과 같다 (MaxAbsJointDeg, 앞서 보내기는 MaxLeadDeg 로 잘린 값 그대로).
            //   평가마다 예측 자세 (q + 관절 처짐 + 엔코더 밖 처짐 — 쌍둥이와 같은 식, 지연 항 없음) 를 끝까지 FK 해 이상 자세와 끝 거리·공구 축 각 차를 재고,
            //   0.05mm·0.05° 안이면 멈춘다 (평가 최대 4번 — 1차 보정 평가 포함). 마지막 평가를 쓴다 (자르기가 걸린 자세도 잘린 고정점으로 이어진다 — 자세에 따라 매끈).
            //   1차 보정 그대로: 1차가 이미 허용 안, 또는 다음 걸음이 수가 아니거나 관절 한계(CalcLimit) 밖이거나 1차 보정에서 MaxAbsJointDeg 넘게 벗어남(발산),
            //   또는 마지막 평가가 1차보다 나쁨.
            //   보정하지 않는 항은 넣지 않는다 — 바닥 보정이 꺼졌거나(flooron=0·옮기는 동안의 관절 모델) dz 를 얹지 못했으면 바닥 몫 없음. 지연(τ·ω)은 1차와 같다.
            //   관절값 사본으로 계산한다 (3D 관절값을 바꾸지 않는다). 비용: 평가마다 지레(중력 항이 있는 관절만 — OMX FK 10번) + FK 2번, 바닥 몫 FK 4번, 기준 FK 2번 (10-16 측정: 대개 평가 2번, 출력 틱 0.38 → 0.77ms)
            // ────────────────────────────────────────────────────────────
            private const int SAG_PRED_MAX_EVAL = 4;                                         // 예측 평가 최대 횟수 (1차 보정 평가 포함 — 다듬는 걸음은 최대 3번)
            private const double SAG_PRED_TOL_MM = 0.05, SAG_PRED_TOL_DEG = 0.05;           // 멈춤 허용 — 쌍둥이 끝 거리 (mm)·공구 축 각 차 (deg)
            private float[] m_afSagLastPred = null;                                          // 직전 출력 틱의 예측 보정 진단 {다듬음, 평가 수, 끝 거리, 공구 각 차}
            /// <summary>직전 출력 틱의 예측 보정 (10-16) — {다듬음 (1 = 예측 보정을 얹음, 0 = 1차 보정 그대로), 평가 수, 쓴 보정의 예측 끝 − 이상 끝 거리 (mm),
            /// 예측 공구 축 − 이상 공구 축 각 (deg, 꺾는 손목 없음이면 NaN)}. 예측 보정을 쓰지 않았으면 (끔·관절 모델 없음) NaN 넷. 전환 무게 앞의 값. 진단용</summary>
            public float[] SagComp_LastPredInfo { get { float[] v = m_afSagLastPred; return v == null ? new float[] { float.NaN, float.NaN, float.NaN, float.NaN } : (float[])v.Clone(); } }

            // target = 1차 보정 (관절 ID → deg, 항마다 자른 뒤) — 다듬었으면 고쳐 쓰고 true, 아니면 그대로 false.
            //   ad = 관절값 사본 (ids 에 이상 자세를 넣은 것 — 바꾸지 않는다), pose = 이상 자세 (m.JointIDs 순), sg = 이동 방향 σ (JointIDs 순),
            //   lead = 앞서 보내기 (JointIDs 순, MaxLeadDeg 로 자른 값 — null = 없음), bFloor·dz·fid·fq = 1차 보정이 얹은 바닥 항 (이상 자세의 끝을 올린 양 dz,
            //   관절 fid 와 그 이상 각 fq), tau = 손목 공구 기울기 τ (NaN = 항 없음). info = {다듬음 1/0, 평가 수, 끝 거리 mm, 공구 각 차 deg}
            private bool SagComp_PredRefine(CSagComp m, double[] ad, float[] pose, float[] sg, double[] lead, bool bFloor, double dz, int[] fid, float[] fq,
                                            double tau, Dictionary<int, float> target, out float[] info)
            {
                info = null;
                if (m == null || !m.HasJoint || ad == null || pose == null || sg == null || target == null) return false;
                int[] jids = m.JointIDs;
                int nj = jids.Length;
                if (pose.Length < nj || sg.Length < nj) return false;
                double mx = m.MaxAbsJointDeg;
                // 다룰 관절 — 관절 모델 + 바닥 항 관절 + 손목 (기울기)
                var aids = new List<int>(jids);
                if (bFloor && fid != null) foreach (int id in fid) if (id >= 0 && !aids.Contains(id)) aids.Add(id);
                bool bTilt = !double.IsNaN(tau) && m.WristID >= 0;
                if (bTilt && !aids.Contains(m.WristID)) aids.Add(m.WristID);
                int na = aids.Count;
                var ja = new int[nj];                                                       // JointIDs 칸 → aids 칸
                for (int j = 0; j < nj; j++) ja[j] = aids.IndexOf(jids[j]);
                var qi = new double[na];                                                    // 이상 자세 (aids 순)
                for (int a = 0; a < na; a++) { int j = Array.IndexOf(jids, aids[a]); qi[a] = j >= 0 ? pose[j] : SagFc_Get(ad, aids[a]); }
                // 쌍둥이의 엔코더 밖 항 X (이상 자세에서 — 보낸 명령과 무관) 와 보정의 그 몫 K = −X (항마다 자름)
                var X = new double[na]; var K = new double[na];
                if (bFloor)
                {
                    double[] dqm;
                    if (fid == null || fq == null || !SagComp_FloorJointDeltaCopy(m.FuncNumber, ad, fid, fq, -dz, out dqm)) return false;   // 끝을 dz 내리는 관절 변화 (쌍둥이 ②) — 못 풀면 1차 그대로
                    for (int k = 0; k < 3; k++)
                    {
                        if (fid[k] < 0) continue;                                            // 꺾는 손목 없음 — 피치 두 관절로만
                        int a = aids.IndexOf(fid[k]);
                        X[a] += dqm[k];
                        K[a] += Math.Max(-mx, Math.Min(mx, -dqm[k]));
                    }
                }
                if (bTilt)
                {
                    int a = aids.IndexOf(m.WristID);
                    X[a] += tau;                                                              // 쌍둥이 ③
                    K[a] += Math.Max(-mx, Math.Min(mx, -tau));
                }
                // 앞서 보내기 (켬일 때) — 쌍둥이의 지연 항이 메운다고 보고 예측 자세에서 뺀다 (1차 보정과 같은 짝)
                var Ld = new double[na];
                if (lead != null) for (int j = 0; j < nj && j < lead.Length; j++) Ld[ja[j]] = lead[j];
                // 기준 — 이상 자세의 끝·공구 축 (손목 원점 → 끝)
                bool bWrist = m.WristID >= 0;
                double[] adI = (double[])ad.Clone();
                for (int a = 0; a < na; a++) SagFc_Set(adI, aids[a], (float)qi[a]);
                float tx0, ty0, tz0, wx0 = 0, wy0 = 0, wz0 = 0;
                SagFc_Fk(m.FuncNumber, adI, -1, out tx0, out ty0, out tz0);
                if (bWrist) SagFc_Fk(m.FuncNumber, adI, m.WristID, out wx0, out wy0, out wz0);
                // 1차 보정에서 시작
                var c0 = new double[na];
                for (int a = 0; a < na; a++) { float v; if (target.TryGetValue(aids[a], out v)) c0[a] = v; }
                double[] c = c0;
                var need = new bool[nj];                                                    // 지레가 필요한 관절 — 중력 항이 있는 관절 (c ≠ 0). c = 0 이면 지레가 식에 들어가지 않는다
                for (int j = 0; j < nj; j++) need[j] = m.Jc[j] != 0;
                double score0 = double.NaN, score = double.NaN, dMm = double.NaN, dDeg = double.NaN;
                int nEval = 0;
                float[] lev;
                for (int it = 0; ; it++)
                {
                    // 평가 — 보낸 명령 q = 이상 + c 의 지레 (쌍둥이 ①) → 예측 자세 = q + 관절 처짐 e + X → 끝·공구 축을 이상 자세와
                    if (!SagComp_PredEval(m, ad, jids, ja, aids, pose, qi, sg, X, Ld, c, need, bWrist, tx0, ty0, tz0, wx0, wy0, wz0, out lev, out dMm, out dDeg)) { score = double.NaN; break; }
                    nEval++;
                    score = Math.Max(dMm / SAG_PRED_TOL_MM, bWrist ? dDeg / SAG_PRED_TOL_DEG : 0);
                    if (it == 0) score0 = score;
                    if (score <= 1.0 || it >= SAG_PRED_MAX_EVAL - 1) break;                   // 허용 안 (또는 마지막 평가)
                    // 다음 c = 관절 보정 (지레 = q, 1차와 같은 자르기) + 바닥·기울기 몫 K
                    var cn = (double[])K.Clone();
                    for (int j = 0; j < nj; j++)
                    {
                        double cj = m.JointCorrStatic(j, lev[j], sg[j]);                     // = −(c·L(q)·(1+κ) − h·σ)
                        if (lead != null && j < lead.Length) cj += lead[j];
                        if (cj > mx) cj = mx; if (cj < -mx) cj = -mx;
                        cn[ja[j]] += cj;
                    }
                    for (int a = 0; a < na; a++)
                    {
                        if (double.IsNaN(cn[a]) || double.IsInfinity(cn[a]) || Math.Abs(cn[a] - c0[a]) > mx) { score = double.NaN; break; }   // 수가 아님·1차에서 너무 멀다 (발산)
                        float q = (float)(qi[a] + cn[a]);
                        if (Math.Abs(CalcLimit(aids[a], q) - q) > 1e-4f) { score = double.NaN; break; }                                     // 관절 한계 밖
                    }
                    if (double.IsNaN(score)) break;
                    c = cn;
                }
                // 쓴다: 마지막 평가 (1차 보정이 이미 허용 안이면 1차 그대로). 1차로 돌아감: 다음 걸음이 수가 아님·관절 한계 밖·발산, 또는 마지막 평가가 1차보다 나쁨
                bool bUse = !ReferenceEquals(c, c0) && !double.IsNaN(score) && !double.IsNaN(score0) && score <= score0;
                info = new float[] { bUse ? 1f : 0f, nEval, bUse ? (float)dMm : float.NaN, bUse ? (float)dDeg : float.NaN };
                if (!bUse) return false;
                for (int a = 0; a < na; a++) target[aids[a]] = (float)c[a];
                return true;
            }
            // 예측 보정 평가 한 번 — c (aids 순 보정) 를 보낸 명령으로 쌍둥이 식의 예측 자세 (q + e + X − 앞서 보내기 Ld) 를 만들고 끝을 FK: 이상 끝(tx0..)과 거리 dMm,
            //   공구 축 각 차 dDeg (손목 없음 NaN).
            //   lev = 그 명령 자세의 지레 (JointIDs 순 — 다음 걸음의 관절 보정이 쓴다). 수가 아니면 false
            private bool SagComp_PredEval(CSagComp m, double[] ad, int[] jids, int[] ja, List<int> aids, float[] pose, double[] qi, float[] sg, double[] X, double[] Ld, double[] c, bool[] need,
                                          bool bWrist, float tx0, float ty0, float tz0, float wx0, float wy0, float wz0, out float[] lev, out double dMm, out double dDeg)
            {
                int nj = jids.Length, na = aids.Count;
                dMm = dDeg = double.NaN;
                var qj = new float[nj];
                for (int j = 0; j < nj; j++) qj[j] = (float)(pose[j] + c[ja[j]]);
                lev = SagComp_PredLevers(m.FuncNumber, jids, qj, ad, need);
                var e = new double[na];
                for (int j = 0; j < nj; j++) e[ja[j]] = (1.0 + m.FlexGain) * m.Jc[j] * lev[j] - m.Jh[j] * sg[j];
                double[] adP = (double[])ad.Clone();
                for (int a = 0; a < na; a++)
                {
                    double v = qi[a] + c[a] + e[a] + X[a] - Ld[a];
                    if (double.IsNaN(v) || double.IsInfinity(v)) return false;
                    SagFc_Set(adP, aids[a], (float)v);
                }
                float tx, ty, tz;
                SagFc_Fk(m.FuncNumber, adP, -1, out tx, out ty, out tz);
                dMm = Math.Sqrt((double)(tx - tx0) * (tx - tx0) + (double)(ty - ty0) * (ty - ty0) + (double)(tz - tz0) * (tz - tz0));
                if (double.IsNaN(dMm) || double.IsInfinity(dMm)) return false;
                if (bWrist)
                {
                    float wx, wy, wz;
                    SagFc_Fk(m.FuncNumber, adP, m.WristID, out wx, out wy, out wz);
                    dDeg = SagComp_PredAngle(tx0 - wx0, ty0 - wy0, tz0 - wz0, tx - wx, ty - wy, tz - wz);
                    if (double.IsNaN(dDeg)) return false;
                }
                return true;
            }
            // SagFc_Levers 와 같은 값 — need[j] 인 관절만 계산하고 나머지는 0 (중력 항 c = 0 인 관절은 지레가 보정·예측에 들어가지 않는다 — 출력 틱 비용을 줄인다)
            private float[] SagComp_PredLevers(int nFunc, int[] jids, float[] pose, double[] adBase, bool[] need)
            {
                const float D = 0.5f;
                int nj = jids.Length;
                float[] lev = new float[nj];
                double[] ad = (double[])adBase.Clone();
                for (int j = 0; j < nj; j++) SagFc_Set(ad, jids[j], pose[j]);
                for (int j = 0; j < nj; j++)
                {
                    if (!need[j]) continue;
                    SagFc_Set(ad, jids[j], pose[j] + D); double zp = SagFc_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j] - D); double zm = SagFc_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j]);
                    lev[j] = (float)((zp - zm) / (2 * D));
                }
                return lev;
            }
            // 두 공구 축 벡터 사이 각 (deg) — atan2(|a×b|, a·b) (작은 각도 정확). 길이가 0 이면 NaN
            private static double SagComp_PredAngle(double ax, double ay, double az, double bx, double by, double bz)
            {
                double cx = ay * bz - az * by, cy = az * bx - ax * bz, cz = ax * by - ay * bx;
                double s = Math.Sqrt(cx * cx + cy * cy + cz * cz), d = ax * bx + ay * by + az * bz;
                if (Math.Sqrt(ax * ax + ay * ay + az * az) < 1e-9 || Math.Sqrt(bx * bx + by * by + bz * bz) < 1e-9) return double.NaN;
                return Math.Atan2(s, d) * 180.0 / Math.PI;
            }

            // 출력 단 상태 사본 — 3D 만 작업(SagRun)이 시작 때 떠 두고 끝에 되돌린다 (그사이 작업이 건 모델 전환의 기준·시계까지 시작 때로),
            //   쌍둥이 그림자 출력 단(SagComp_JointOutShadow)은 틱마다 실물 상태를 떠 두고 그림자 사본을 바꿔 끼웠다가 되돌린다 (그림자가 실물 상태를 바꾸지 않게).
            //   방향·속도 추적, 직전 틱 보정, 전환 기준·시계, 손목 유지, 진단값(직전 틱 관절 보정·바닥 dz·첫닿음·GP 표준편차) 전부
            private sealed class CSagOutState
            {
                public Dictionary<int, float> Anchor, Sigma, Prev, VelDeg, Omega, Alpha, Applied, From;
                public Dictionary<int, int> Dir, VelT;
                public bool BlendRun; public double BlendMs; public int OutLastMs;
                public float[] LastJoint; public float LastFloorDz; public float[] LastFc; public float LastGpStd; public float[] LastPred;
                public int WhID, WhLastOutMs; public float Wh, WhFadeFrom, WhLast; public bool WhFading, WhFadeTicked; public double WhFadeMs;
            }
            private object SagComp_SaveOutState()
            {
                lock (m_dicSagDir) return SagComp_CaptureOutStateLocked();
            }
            // 상태 되돌림 (3D 만 작업 끝) — 통째로 바뀐 것으로 친다 (전환 세대·통째 바뀜 세대 + 1 — 도는 출력 틱이 사이에 들어온 기준을 지우지 않게, 그림자는 다시 시작)
            private void SagComp_RestoreOutState(object o)
            {
                CSagOutState s = o as CSagOutState;
                if (s == null) return;
                lock (m_dicSagDir)
                {
                    SagComp_InstallOutStateLocked(s);
                    m_nSagSwitchGen++; m_nSagHardGen++;
                }
            }
            // 지금 출력 단 상태의 사본 — lock (m_dicSagDir) 안에서 부른다 (손목 유지는 그 안에서 m_oWristHold 로 — 잠금 순서 m_dicSagDir → m_oWristHold)
            private CSagOutState SagComp_CaptureOutStateLocked()
            {
                var s = new CSagOutState();
                s.Anchor = new Dictionary<int, float>(m_dicSagAnchor); s.Sigma = new Dictionary<int, float>(m_dicSagSigma); s.Prev = new Dictionary<int, float>(m_dicSagPrev);
                s.VelDeg = new Dictionary<int, float>(m_dicSagVelDeg); s.Omega = new Dictionary<int, float>(m_dicSagOmega); s.Alpha = new Dictionary<int, float>(m_dicSagAlpha);
                s.Applied = new Dictionary<int, float>(m_dicSagApplied); s.From = m_dicSagFrom == null ? null : new Dictionary<int, float>(m_dicSagFrom);
                s.Dir = new Dictionary<int, int>(m_dicSagDir); s.VelT = new Dictionary<int, int>(m_dicSagVelT);
                s.BlendRun = m_bSagBlendRun; s.BlendMs = m_dSagBlendMs; s.OutLastMs = m_nSagOutLastMs;
                s.LastJoint = m_afSagLastJoint; s.LastFloorDz = m_fSagLastFloorDz; s.LastFc = m_afSagLastFc; s.LastGpStd = m_fSagLastGpStd; s.LastPred = m_afSagLastPred;
                lock (m_oWristHold)
                {
                    s.WhID = m_nWristHoldID; s.Wh = m_fWristHold; s.WhFading = m_bWristHoldFading; s.WhFadeFrom = m_fWristHoldFadeFrom; s.WhFadeMs = m_dWristHoldFadeMs;
                    s.WhFadeTicked = m_bWristHoldFadeTicked; s.WhLastOutMs = m_nWristHoldLastOutMs; s.WhLast = m_fWristHoldLast;
                }
                return s;
            }
            // 사본을 출력 단 상태로 (값을 복사해 넣는다 — 사본은 그대로) — lock (m_dicSagDir) 안에서 부른다. 세대는 건드리지 않는다
            private void SagComp_InstallOutStateLocked(CSagOutState s)
            {
                SagComp_CopyInto(m_dicSagAnchor, s.Anchor); SagComp_CopyInto(m_dicSagSigma, s.Sigma); SagComp_CopyInto(m_dicSagPrev, s.Prev);
                SagComp_CopyInto(m_dicSagVelDeg, s.VelDeg); SagComp_CopyInto(m_dicSagOmega, s.Omega); SagComp_CopyInto(m_dicSagAlpha, s.Alpha);
                SagComp_CopyInto(m_dicSagApplied, s.Applied);
                m_dicSagFrom = s.From == null ? null : new Dictionary<int, float>(s.From);
                m_dicSagDir.Clear(); if (s.Dir != null) foreach (KeyValuePair<int, int> kv in s.Dir) m_dicSagDir[kv.Key] = kv.Value;
                m_dicSagVelT.Clear(); if (s.VelT != null) foreach (KeyValuePair<int, int> kv in s.VelT) m_dicSagVelT[kv.Key] = kv.Value;
                m_bSagBlendRun = s.BlendRun; m_dSagBlendMs = s.BlendMs; m_nSagOutLastMs = s.OutLastMs;
                m_afSagLastJoint = s.LastJoint; m_fSagLastFloorDz = s.LastFloorDz; m_afSagLastFc = s.LastFc; m_fSagLastGpStd = s.LastGpStd; m_afSagLastPred = s.LastPred;
                lock (m_oWristHold)
                {
                    m_nWristHoldID = s.WhID; m_fWristHold = s.Wh; m_bWristHoldFading = s.WhFading; m_fWristHoldFadeFrom = s.WhFadeFrom; m_dWristHoldFadeMs = s.WhFadeMs;
                    m_bWristHoldFadeTicked = s.WhFadeTicked; m_nWristHoldLastOutMs = s.WhLastOutMs; m_fWristHoldLast = s.WhLast;
                }
            }
            private static void SagComp_CopyInto(Dictionary<int, float> dst, Dictionary<int, float> src)
            {
                dst.Clear();
                if (src != null) foreach (KeyValuePair<int, float> kv in src) dst[kv.Key] = kv.Value;
            }

            // ────────────────────────────────────────────────────────────
            // 측정 때만: 손목 수직 유지 (10-02 사용자 제안)
            //   바닥을 찍는 동안 그리퍼 기울기가 자세·이동 방향마다 달라지면(손목 데드밴드, 12·13 잔차) 닿는 높이가 흔들린다.
            //   공구 피치 = 12+13+14 세 관절 합(SagComp_FloorJointDelta 와 같은 규약)이므로, 엔코더로 읽은 합이 이상 각의 합
            //   (= 명령 피치, 보통 90° = 지면에 수직)과 같아지도록 **14번 명령에만** 각을 더한다.
            //   SagComp_FloorPress 안에서만 켜지고, 끝나면 다음 출력 틱부터 1초에 걸쳐 빠진다 — 평소 동작(PlayXyz·재생)에는 얹히지 않는다.
            //   엔코더 밖(14번 브래킷 휨)은 FK 로 보이지 않으므로 이것으로 잡지 못한다.
            // ────────────────────────────────────────────────────────────
            private readonly object m_oWristHold = new object();
            private int m_nWristHoldID = -1;                       // 손목 유지를 얹는 관절 (−1 = 없음)
            private float m_fWristHold = 0;                        // 그 관절 명령에 더하는 각 (deg)
            private bool m_bWristHoldFading = false;               // 놓는 중 — 출력 틱이 흐르는 동안 1초에 걸쳐 0 으로
            private float m_fWristHoldFadeFrom = 0;
            private double m_dWristHoldFadeMs = 0;                 // 페이드 진행 시간 — 출력 틱 사이 간격을 틱당 50ms 까지만 더한다 (멈춰 있는 동안은 안 흐른다)
            private bool m_bWristHoldFadeTicked = false;           // 놓은 뒤 첫 출력 틱을 지났는가 (첫 틱은 그대로 보낸다)
            private int m_nWristHoldLastOutMs = int.MinValue;      // 직전 출력 틱 시각 (모션 시계)
            private float m_fWristHoldLast = 0;                    // 직전 출력 틱에 실제로 얹은 값
            private const double SAG_WRIST_DP_SANE = 10.0;         // 피치 오차가 이보다 크면 읽기 이상으로 보고 무시 (공중 실측 −0.4~+1.2°)

            /// <summary>직전 출력 틱에 손목에 얹은 수직 유지 각 (deg) — 바닥 측정 중에만 0 이 아니다. 진단용</summary>
            public float SagComp_WristHoldNow { get { lock (m_oWristHold) return m_fWristHoldLast; } }

            // 지금 걸려 있는 값 (놓는 중이면 페이드 값) — lock 안에서 부를 것
            private float WristHoldCurrent()
            {
                if (m_nWristHoldID < 0) return 0;
                if (!m_bWristHoldFading) return m_fWristHold;
                double w = Math.Min(1.0, m_dWristHoldFadeMs / 1000.0);
                return (float)(m_fWristHoldFadeFrom * (1 - w));
            }
            private float SagComp_WristHoldValue() { lock (m_oWristHold) return WristHoldCurrent(); }

            // 측정 시작 — 놓는 중이었으면 실물이 지금 들고 있는 값(직전 출력 틱 값)에서 이어받는다 (튀지 않게)
            private void SagComp_WristHoldBegin(int nID)
            {
                lock (m_oWristHold)
                {
                    float cur = (m_nWristHoldID == nID) ? (m_bWristHoldFading ? m_fWristHoldLast : m_fWristHold) : 0;
                    m_nWristHoldID = nID; m_fWristHold = cur; m_bWristHoldFading = false; m_dWristHoldFadeMs = 0;
                }
            }

            // 한 걸음 — 피치 오차 dP(실측 − 이상, deg)를 줄이는 쪽으로 gain·dP (한 걸음 상한 maxStep, 전체 상한 maxAbs).
            //   |dP| ≤ tol 이면 그대로. 반환 = 실제로 바꾼 양 (0 = 안 바꿈: 허용 안 / 상한 / 읽기 이상 / 꺼짐)
            private float SagComp_WristHoldStep(double dP, double gain, double tol, double maxStep, double maxAbs)
            {
                if (double.IsNaN(dP) || Math.Abs(dP) <= tol || Math.Abs(dP) > SAG_WRIST_DP_SANE) return 0;
                double s = -gain * dP;
                if (s > maxStep) s = maxStep; if (s < -maxStep) s = -maxStep;
                lock (m_oWristHold)
                {
                    if (m_nWristHoldID < 0 || m_bWristHoldFading) return 0;
                    float old = m_fWristHold;
                    double v = old + s;
                    if (v > maxAbs) v = maxAbs; if (v < -maxAbs) v = -maxAbs;
                    m_fWristHold = (float)v;
                    return m_fWristHold - old;
                }
            }

            // 측정 끝 — 다음 동작의 출력 틱이 흐르는 동안 1초에 걸쳐 0 으로. 멈춰 있는 동안은 그대로라 실물이 튀지 않는다
            private void SagComp_WristHoldRelease()
            {
                lock (m_oWristHold)
                {
                    if (m_nWristHoldID < 0 || m_bWristHoldFading) return;
                    m_fWristHoldFadeFrom = m_fWristHold; m_bWristHoldFading = true; m_dWristHoldFadeMs = 0; m_bWristHoldFadeTicked = false;
                }
            }

            // 출력 훅 (SagComp_JointOut 끝) — 관절 보정 위에 더한다
            private void SagComp_WristHoldOut(int[] ids, float[] afDeg)
            {
                lock (m_oWristHold)
                {
                    int nNow = MotionTick_ms();
                    int dt = m_nWristHoldLastOutMs == int.MinValue ? 0 : nNow - m_nWristHoldLastOutMs;
                    m_nWristHoldLastOutMs = nNow;
                    m_fWristHoldLast = 0;
                    if (m_nWristHoldID < 0) return;
                    // 놓은 뒤 첫 틱은 그대로, 그다음부터 틱 간격만큼 — 한 틱에 50ms 넘게는 안 더한다(쉬었다 다시 움직여도 남은 값이 한 번에 빠지지 않게)
                    if (m_bWristHoldFading)
                    {
                        if (m_bWristHoldFadeTicked) m_dWristHoldFadeMs += Math.Max(0, Math.Min(50, dt));
                        else m_bWristHoldFadeTicked = true;
                    }
                    float v = WristHoldCurrent();
                    int i = Array.IndexOf(ids, m_nWristHoldID);
                    if (m_bWristHoldFading && m_dWristHoldFadeMs >= 1000)
                    { m_nWristHoldID = -1; m_bWristHoldFading = false; m_fWristHold = 0; v = 0; }
                    if (i >= 0 && i < afDeg.Length && v != 0) afDeg[i] += v;
                    m_fWristHoldLast = v;
                }
            }

            // 공구 피치 오차 — 손목 공구 기울기 보정(10-15)이 걸려 있으면 기준 = 이상 합 − τ(이상 자세): 실측 합 − (이상 합 − τ).
            //   손목 유지(측정 누름·자리 잡기)가 출력 단의 −τ 를 지우지 않게 — 실물 공구가 수직인 손목 자리가 유지의 목표다. 걸린 모델 = 출력 단의 지금 모델 (꺼져 있으면 τ 없음)
            private double SagComp_PitchErrTilt(int[] ids, float[] q, float[] a)
            {
                double d = SagComp_PitchErr(q, a);
                if (double.IsNaN(d)) return d;
                CSagComp m = m_SagComp;
                if (m == null || !m.Enabled || !m.HasWristTilt) return d;
                return d + SagComp_TiltOf(m, ids, q);
            }
            // 공구 피치 오차 (deg) = 실측 합 − 이상 합. q·a = PosIDs 3 + Wrist 순 — 피치 관절 = PosIDs[1]·PosIDs[2]·Wrist
            private static double SagComp_PitchErr(float[] q, float[] a)
            {
                if (q == null || a == null || q.Length < 4 || a.Length < 4) return double.NaN;
                return (a[1] + a[2] + a[3]) - (q[1] + q[2] + q[3]);
            }

            // 관절 (12,13,14) 자세 q 에서의 TCP 수평 반경·높이 — 관절값 사본으로 (3D 관절값을 바꾸지 않는다).
            //   10-15: 예전에는 3D 관절값을 잠깐 바꿨다 되돌려, 출력 틱·쌍둥이가 계산하는 사이 화면이 그 자세를 그려 검은 팔·쌍둥이가 떨렸다
            private void SagComp_TcpRZ(int nFunc, int[] fid, float[] q, out double r, out double z)
            {
                SagFc_TcpRZ(nFunc, SagFc_MotCopy(), fid, q, out r, out z);
            }
            // ids 에 q 를 넣은 관절값 사본의 TCP (수식 nFunc) — CalcF(nFunc, −1, false) 와 같은 값, 3D 관절값을 바꾸지 않는다 (엔코더 FK 등)
            private void SagComp_FkCopy(int nFunc, int[] ids, float[] q, out float x, out float y, out float z)
            {
                double[] ad = SagFc_MotCopy();
                for (int i = 0; i < ids.Length && i < q.Length; i++) SagFc_Set(ad, ids[i], q[i]);
                SagFc_Fk(nFunc, ad, -1, out x, out y, out z);
            }

            /// <summary>끝을 dz(mm) 만큼 올리는 관절 (12,13,14) 변화 (deg) — 반경 방향 위치와 공구 피치(세 관절 합)는 그대로.
            /// 피치를 지키는 두 방향 (+12,−14), (+13,−14) 의 수치 야코비안(±0.5°) 2×2 를 풀고 뉴턴 1회로 다듬는다 (CalcF 4회).
            /// 꺾는 손목 없음 (fid[2] = −1, 10-13): 손목 칸은 3D 관절값에 쓰이지 않아 두 방향이 (+12), (+13) 이 된다 — 피치 두 관절의 2×2 야코비안으로
            /// 반경을 지키며 끝을 올린다 (공구 기울기는 지키지 못한다). dq[2] 는 쓰는 쪽이 버린다</summary>
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
            /// 관절값 사본으로 계산한다 — 3D 관절값을 바꾸지 않는다 (10-15: 예전에는 잠깐 바꿨다 원복해 그사이 화면이 ±0.5° 자세를 그렸다).</summary>
            public float[] SagComp_JointLevers(int nFunc, int[] jids, float[] pose)
            {
                int nj = jids.Length;
                float[] lev = new float[nj];
                const float D = 0.5f;
                double[] ad = SagFc_MotCopy();
                for (int j = 0; j < nj; j++) SagFc_Set(ad, jids[j], pose[j]);
                for (int j = 0; j < nj; j++)
                {
                    SagFc_Set(ad, jids[j], pose[j] + D); double zp = SagComp_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j] - D); double zm = SagComp_MeanZ(nFunc, ad, jids, j);
                    SagFc_Set(ad, jids[j], pose[j]);
                    lev[j] = (float)((zp - zm) / (2 * D));
                }
                return lev;
            }
            // 관절 j 하류 질량점 평균 높이 (관절값 사본 ad) — 예전 CalcF 판과 같은 값 (번호 −1 칸은 CalcF(−1) 처럼 TCP 로 센다)
            private double SagComp_MeanZ(int nFunc, double[] ad, int[] jids, int j)
            {
                double s = 0; int n = 0; float x, y, z;
                for (int k = j + 1; k < jids.Length; k++) { SagFc_Fk(nFunc, ad, jids[k], out x, out y, out z); s += z; n++; }   // 관절 k 원점
                SagFc_Fk(nFunc, ad, -1, out x, out y, out z); s += z; n++;                                                   // TCP
                return s / n;
            }

            /// <summary>관절 모델이 예측하는 TCP 처짐 (실측 예상 − 명령, mm) — FK(명령 + 예상 관절 오차) − FK(명령).</summary>
            private void SagComp_JointPredictTcp(CSagComp m, float[] cmd, float[] dir, float[] lev, out float dx, out float dy, out float dz)
            {
                dx = dy = dz = 0;
                if (m == null || !m.HasJoint || cmd == null || dir == null || lev == null) return;
                int nj = m.JointIDs.Length;
                float x0, y0, z0, x1, y1, z1;
                float[] q1 = new float[nj];
                for (int j = 0; j < nj; j++) q1[j] = (float)(cmd[j] + m.PredictJointErr(j, lev[j], dir[j]));
                SagComp_FkCopy(m.FuncNumber, m.JointIDs, cmd, out x0, out y0, out z0);          // 관절값 사본 — 3D 관절값을 바꾸지 않는다
                SagComp_FkCopy(m.FuncNumber, m.JointIDs, q1, out x1, out y1, out z1);
                dx = x1 - x0; dy = y1 - y0; dz = z1 - z0;
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
                SagComp_FkCopy(m.FuncNumber, m.JointIDs, pose, out x, out y, out z);           // 관절값 사본 — 3D 관절값을 바꾸지 않는다
            }

            /// <summary>측정 중 바닥 접촉 의심 표본을 자동으로 학습에서 뺀다 (기본 켬).</summary>
            public bool SagComp_ContactGuard = true;

            /// <summary>바닥 접촉 의심 표본 검출 — 한 점씩 빼고 학습해 그 점의 TCP z 를 예측하고, 실측이 예측보다
            /// 두드러지게 **들려** 있으면(바닥이 팔을 받쳐 덜 처짐) 접촉으로 보고 Excluded 로 표시한다.
            /// 문턱 = max(2mm, 2.5 × 강건 척도(1.4826·중앙값|들림|)). 한 번에 최대 표본의 1/5.
            /// ★09-28 실측: r=260 멀리(→) 가 두 측정 모두 깨끗한 모델보다 +2.9mm 들리고 2.6~3.1mm 덜 뻗었다
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
                    // 같은 지점·같은 접근 방향 표본은 함께 뺀다 — 여러 측정을 합치면 접촉 표본끼리 서로를 "정상" 으로 받쳐 준다
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
                int nj = m.PosIDs.Length + (m.WristID >= 0 ? 1 : 0);                   // 꺾는 손목 없음이면 위치 관절만 (10-13)
                int[] jids = new int[nj]; Array.Copy(m.PosIDs, jids, m.PosIDs.Length); if (m.WristID >= 0) jids[nj - 1] = m.WristID;
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
                public float DownSumDeg = float.NaN;                    // 로봇 정의의 수직 관절 합 (NaN = 3D 에서 잰 값 — 정의 확인 V7 생략). 앱은 ID 가 정의와 같으면 정의 값으로 채운다
            }

            // 처짐 측정(Survey)이 이어받은 바닥 보정 로그 한 줄 — 보정표가 없고 GP·첫닿음 모델만 있으면 FloorR 이 null 이라
            //   보정표 문장을 만들면 측정 전에 NullReferenceException 으로 죽었다 (지금 활성 파일 = GP 만). 있는 것만 적는다
            private static string SagComp_InheritText(CSagComp model)
            {
                var ci = CultureInfo.InvariantCulture;
                if (model == null) return "";
                if (model.HasFloorMap)
                    return string.Format(ci, "바닥 보정표 이어받음: {0}점 r {1:F0}~{2:F0}mm (FK 기준 — 관절 모델과 무관, 끄려면 파일의 flooron=0)",
                        model.FloorR.Length, model.FloorR[0], model.FloorR[model.FloorR.Length - 1]);
                var parts = new List<string>();
                if (model.HasFc) parts.Add(string.Format(ci, "첫닿음 모델 {0}점", model.Fc.Count));
                if (model.HasGp) parts.Add(string.Format(ci, "GP {0}점", model.Gp.Count));
                return "바닥 보정 이어받음: " + (parts.Count > 0 ? string.Join(" + ", parts.ToArray()) : "없음") + " (FK 기준 — 관절 모델과 무관, 끄려면 파일의 flooron=0)";
            }

            // ⑨ 입구 로봇 정의 확인 (10-04) — SagDef_CheckSurvey (cfg 의 func·posids·wrist·downsum, 공구 피치 규칙). 안 되면 report "★로봇 정의: …" (로그에도) 와 false.
            //   예전 경로 경고(기운 공구 — 손목 갈래는 시드가 정함)는 로그 ★ 한 줄
            private bool SagComp_SurveyDefOk(CSagSurveyCfg cfg, CSim2Real.DLog log, out CSagDefCheck chk, out string report)
            {
                report = "";
                chk = SagDef_CheckSurvey(cfg);
                if (!chk.Ok)
                {
                    report = "★로봇 정의: " + chk.Why;
                    if (log != null) { try { log(report); } catch { } }
                    return false;
                }
                foreach (string it in chk.Items)
                    if (it.StartsWith("Ry ★", StringComparison.Ordinal) && log != null) { try { log(it.Substring(3)); } catch { } }
                return true;
            }

            /// <summary>정지 처짐 표본 수집 + 정지 학습. 진행은 log, 중단은 stop. 실패/중단 시 null. 입구에서 로봇 정의를 확인한다 (SagDef_CheckSurvey — 안 되면 null + report).</summary>
            public CSagComp SagComp_Survey(CScene_t scene, CSagSurveyCfg cfg,
                                           CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var sb = new StringBuilder();
                CSagDefCheck chkDef;
                if (!SagComp_SurveyDefOk(cfg, log, out chkDef, out report)) return null;   // 로봇 정의 (10-04) — 움직이기 전에
                if (scene == null || !scene.IsConnected || !scene.IsTorqueOn)
                { report = "실물 미연결 또는 토크 OFF — 측정 불가"; return null; }

                var model = new CSagComp
                {
                    Robot = cfg.Robot, FuncNumber = cfg.FuncNumber, PosIDs = (int[])cfg.PosIDs.Clone(),
                    WristID = cfg.WristID, PitchDeg = cfg.RyDeg, MaxAbsDelta = cfg.MaxAbsDelta,
                    DownSumDeg = float.IsNaN(cfg.DownSumDeg) ? (float)Math.Round(chkDef.DownSumMeasDeg, 3) : cfg.DownSumDeg, KinSig = chkDef.KinSig,
                    MaxAbsJointDeg = CSagComp.LEARN_MAX_JOINT_DEG                    // 관절 보정 상한 12° (10-16)
                };
                CSagComp prev = m_SagComp;
                // 바닥 보정표는 이어받는다 — FK 기준 표는 엔코더 밖 오차(14번 브래킷 처짐)라 관절 모델을 다시 학습해도 그대로 유효 (09-29)
                if (model.CopyFloorMapFrom(prev) && log != null)
                    log(SagComp_InheritText(model));
                if (model.CopyWristTiltFrom(prev) && model.HasWristTilt && log != null)       // 손목 공구 기울기 보정도 기계 성질 — 이어받는다 (10-15)
                    log("손목 공구 기울기 보정 이어받음 — " + model.WristTiltText());
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
                CSagDefCheck chkDef;
                if (!SagComp_SurveyDefOk(cfg, log, out chkDef, out report)) return false;   // 로봇 정의 (10-04) — 움직이기 전에
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
                                for (int q = 0; q < reads.Count; q++)
                                {
                                    var kv = reads[q];
                                    float tx, ty, tz, vr, vz, ar, az;
                                    if (!SagComp_TraceAt(kv.Key, out tx, out ty, out tz, out vr, out vz, out ar, out az)) continue;
                                    float[] lev = SagComp_JointLevers(cfg.FuncNumber, ids, cmds[q]);
                                    float ax, ay, azz;
                                    SagComp_FkCopy(cfg.FuncNumber, ids, kv.Value, out ax, out ay, out azz);   // 엔코더 FK — 관절값 사본
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
                CSagDefCheck chkDef;
                if (!SagComp_SurveyDefOk(cfg, log, out chkDef, out report)) return -1f;   // 로봇 정의 (10-04) — 움직이기 전에
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
                CSagDefCheck chkDef;
                if (!SagComp_SurveyDefOk(cfg, log, out chkDef, out report)) return -1f;   // 로봇 정의 (10-04) — 움직이기 전에
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
                                double se = 0; int n = 0; double worst = 0;
                                for (int q = 0; q < reads.Count; q++)
                                {
                                    var kv = reads[q];
                                    float tx, ty, tz, vr, vz, ar, az;
                                    if (!SagComp_TraceAt(kv.Key, out tx, out ty, out tz, out vr, out vz, out ar, out az)) continue;
                                    float ax, ay, azz;
                                    SagComp_FkCopy(cfg.FuncNumber, ids, kv.Value, out ax, out ay, out azz);   // 엔코더 FK — 관절값 사본
                                    double dz = azz - tz; se += dz * dz; n++; worst = Math.Max(worst, Math.Abs(dz));
                                    SagComp_DiagRow("verify", z, ms, dir == 0 ? +1 : -1, kv.Key - t0, (double)(kv.Key - t0) / ms,
                                        tx, ty, tz, ax, ay, azz, ids, cmds[q], kv.Value, corrs[q], dirs[q], vels[q]);
                                }
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
                                            int[] b = blk[i];   // 틱·속도 → 3D 각은 장면의 세팅으로 (모터 세팅이 없으면 예전 식 Deg(틱) × 부호와 같다). 속도 한도 b[0] 은 부호 없는 상한이라 그대로
                                            sb.AppendFormat(ci, ",{0:F3},{1:F2},{2:F3},{3:F2},{4:F1},{5:F1},{6}",
                                                scene.RawToSim(i, b[1]), b[0] * 0.229f * 6f, scene.RawToSim(i, b[7]), scene.RawVelToSim(i, b[6]),
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

            /// <summary>바닥 접촉 데이터셋 한 줄씩 (누적 파일용) — 머리줄: session,group,x,y,pitch,r,fkz,cmdz,a1,a2,a3,a4,q1,q2,q3,q4,kind,fc,fccmdz,hold.
            /// 접촉 자세(JointAct)가 있는 유효 접촉만. 앞 16칸은 예전 그대로(꺾임점 — 옛 GP 학습 호환), 뒤 4칸은 첫닿음(10-02):
            /// kind = F(저장된 꺾임이 곧 첫닿음 — 첫닿음 모델 학습에 쓰는 것은 이 행뿐) / H(저장된 꺾임은 단단한 정지, 첫닿음은 위에서 따로 찾음 — fc 칸) /
            /// C(단단한 정지, 첫닿음 못 찾음 — 검열) / X(저장된 꺾임을 쓸 수 없음 — fc 가 차 있으면 F1 이 위를 다시 훑어 찾은 첫닿음이다:
            /// 교차 확인용일 뿐 학습에는 쓰지 않는다) / 빈칸(판정 안 함 — 궤적을 cfg.FirstTouch 로 다시 읽으면 채워진다).
            /// fc·fccmdz = 첫닿음 FK·명령 높이, hold = 손목 수직 유지 켬. cmdz·fccmdz 는 이상 명령 높이다 — 바닥 보정을 켠 채 잰 하강(궤적 fd 칸)도
            /// 꺾임은 유효 명령(cz + fd)으로 맞추고 이상 명령 높이로 되돌려 적는다.
            /// pitch 칸·group 이름(p{피치}y{요})은 접촉의 Pitch = 공구 피치 Ry 다. 첫닿음 데이터셋(floor_dataset_fc.csv)의 pitch 칸은 관절 합(12+13+14) 규약이라
            /// (Ry 70 = 110) 그 파일에 덧붙일 행은 SagRun_DatasetRows (pitch 칸만 관절 합으로 바꿈)로 만든다.</summary>
            public static string SagComp_FloorDatasetRows(List<CFloorContact> contacts, string session, bool bHeader)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                if (bHeader) sb.Append("session,group,x,y,pitch,r,fkz,cmdz,a1,a2,a3,a4,q1,q2,q3,q4,kind,fc,fccmdz,hold\r\n");
                if (contacts == null) return sb.ToString();
                foreach (var c in contacts)
                {
                    if (!c.Detected || c.JointAct == null || c.JointAct.Length < 4) continue;
                    double yaw = Math.Atan2(c.Y, c.X) * 180 / Math.PI;
                    string grp = string.Format(ci, "p{0:F0}y{1:F0}", c.Pitch, Math.Round(yaw / 5) * 5);
                    sb.AppendFormat(ci, "{0},{1},{2:F2},{3:F2},{4:F1},{5:F1},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F3},{12},{13},{14},{15}",
                        session, grp, c.X, c.Y, c.Pitch, c.R, c.FkZ, c.CmdZ, c.JointAct[0], c.JointAct[1], c.JointAct[2], c.JointAct[3],
                        c.JointCmd != null ? c.JointCmd[0].ToString("F3", ci) : "", c.JointCmd != null ? c.JointCmd[1].ToString("F3", ci) : "",
                        c.JointCmd != null ? c.JointCmd[2].ToString("F3", ci) : "", c.JointCmd != null ? c.JointCmd[3].ToString("F3", ci) : "");
                    sb.AppendFormat(ci, ",{0},{1},{2},{3}\r\n", c.Kind ?? "", float.IsNaN(c.FcZ) ? "" : c.FcZ.ToString("F3", ci),
                        float.IsNaN(c.FcCmdZ) ? "" : c.FcCmdZ.ToString("F3", ci), (c.Hold || !float.IsNaN(c.WristHoldDeg)) ? 1 : 0);
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
                public float DownSumDeg = 90f;         // 로봇 정의의 수직 관절 합 (Ry 90 의 관절 합 — 작업이 누를 때마다 정의에서 채운다, 입구 정의 확인 SagDef_Check).
                                                       //   NaN = 자동 — 누름 입구가 3D FK 로 잰 값을 여기 채운다 (10-13, SagDef_Eff)
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
                // ── 움직임 (10-03, SagComp_FloorPress 만) — 시작점·하강·들어 올림·복귀 모두 끝 자세를 관절 합(12+13+14 = 90, Ry < 90 이면 180 − Ry)을 정한 IK 로
                //   미리 풀고 그 사이를 관절 보간한다 (SagComp_PressMove). ★10-03 검토: 공구 피치를 90 에서 접히는 값으로 맞추는 IK(PlayXyz·PlayXyzPath →
                //   CalcInv_Tool)는 수직(Ry 90)에서 접힌 양쪽 해(관절 합 89.9 ↔ 90.5)를 오가, 하강 시작 0.3초쯤 뒤 한 틱에 12·13·14 가 0.8~3.6° 튀었다
                //   (버스 흉내 기본 측정 22번 중 7번 한 틱 관절 변화 막이로 멈춤, 막이 아래 2° 안팎은 그대로 서보로 나감). false = 예전 방식 (PlayXyz·PlayXyzPath)
                public bool SumIk = true;
                public float SumIkDevMm = 2f;              // 수직 다리(하강·들어 올림·복귀)의 관절 보간 경로가 수직선에서 벗어나도 되는 거리 (mm) — 넘으면 가운데 점으로 나눈다
                                                           //   (조각마다 멈췄다 가므로 하강은 한 조각이 좋다: 32mm 하강 r120 0.96, r250 0.5mm, 닿기 시험 창 41mm r125 ≈ 1.6mm.
                                                           //    공구 기울기(관절 합)는 보간 내내 그대로, 높이는 한쪽으로만)
                // ── 손목 수직 유지 (10-02 사용자 제안, SagComp_FloorPress 만) — 측정하는 동안만 공구를 지면에 수직으로 붙잡는다 ──
                //   엔코더 FK 공구 피치(12+13+14 실측 합)가 명령 피치(이상 각 합, 보통 90°)와 같아지도록 14번 명령에만 각을 더한다.
                //   ① 시작점 자리 잡기: 재고 → 고치고 → WristSettleWaitMs 기다려 다시 재기, |오차| ≤ WristHoldTolDeg 면 끝 (최대 WristSettleIter 번)
                //   ② 하강 중 실시간 따라가기 (팔은 그대로 내려가고 손목만): 틱마다 WristTrackGain·오차 (한 틱 상한 WristTrackMaxStepDeg).
                //      무장 뒤 벌어짐이 WristFreezeLagMm 이면(닿음) 그 값으로 고정, 누름·들어 올림 동안 그대로.
                //      피치 오차가 WristFreezeDpDeg 를 넘으면 무장과 무관하게 고정 — 이상 상황 막이.
                //   측정이 끝나면 다음 동작의 출력 틱이 흐르는 1초에 걸쳐 빠진다. 평소 동작에는 얹히지 않는다. 엔코더 밖(브래킷 휨)은 FK 로 안 보여 못 잡는다.
                //   09-29 궤적(공중): 피치 오차 반경 120~230 −0.35~+0.27°, 240~260 +0.9~+1.2° — 방향(요)과 무관.
                //   10-02 실기 A/B (반경 260·240·160·130 × 끔·켬 2회씩, 사용자 결정: 측정 때만 쓴다):
                //     · 먼 쪽(260·240)은 꺾임점 피치 오차 끔 +0.35~+1.67° → 켬 −0.59~0.00°.
                //     · 가까운 쪽(160·130)은 손목 서보(XL330)가 하강 중 공중에서 3.6~5.1° 뒤처지는데(틱의 60~80% 멈춤 — 정지 마찰),
                //       그때 "피치 오차 1° = 닿기 시작" 고정이 공중(꺾임점 9~12mm 위)에서 걸려 따라가기가 멈췄다 → 꺾임점 +2.8~+3.7° 로 끔과 같음.
                //       ⇒ 고정은 진짜 닿음(무장 뒤 벌어짐 WristFreezeLagMm)으로만, 피치 오차 한도는 이상 상황 막이(WristFreezeDpDeg)로.
                //     · 고정 규칙을 바꾼 재실험(14:52, r=160·130 × 끔·켬 2회): 꺾임점 피치 오차 끔 +3.08~+4.57° → 켬 −0.53~−1.49° (|평균| 3.80 → 1.06°).
                //       서보 목표 위치(g4)를 기록해 보니 손목 서보가 목표와 4~8° 떨어진 채 멈춰 있다(먼 쪽 0.6°) — 서보 위치 게인 부족. 유지는 그만큼 목표를 더 민다.
                //     · r=240 은 켜면 FK 바닥 50.0/50.3, 끄면 58.4/60.2. 다만 r=240 은 공중에서도 FK 가 명령을 0.6~0.85 로만 따라가 꺾임 맞춤이 불확실하다.
                //       켜고 모은 데이터는 끄고 모은 데이터(09-28·29)와 섞지 말 것.
                public bool WristHold = true;
                public float WristHoldTolDeg = 0.25f;      // 허용 오차 (엔코더 0.088°/틱 — 세 관절 합의 양자화 ±0.13°)
                public float WristHoldMaxDeg = 8f;         // 손목에 더하는 각 전체 상한 — 10-02 재실험: r=130 에서 손목 서보가 목표와 4~8° 떨어져 5° 상한에 닿음
                public int WristSettleIter = 6;            // 자리 잡기 반복 상한
                public int WristSettleWaitMs = 150;        // 고친 뒤 서보가 자리 잡을 시간
                public float WristSettleGain = 0.8f;       // 자리 잡기 한 번에 고치는 비율 — 데드밴드(h≈0.47°)를 넘어 과보정하지 않게 1 보다 작게
                public float WristSettleMaxStepDeg = 2f;
                public bool WristTrack = true;             // 하강 중 따라가기
                public float WristTrackGain = 0.25f;       // 틱당 비율 — 서보 응답이 1~2틱 늦게 읽혀도 진동하지 않게 작게
                public float WristTrackMaxStepDeg = 0.1f;  // 한 틱 상한
                public float WristFreezeLagMm = 2.0f;      // 무장 뒤 벌어짐이 이만큼이면 닿음 → 고정 (공중 요동 ±1mm, 닿은 뒤 누름 기울기 0.3~0.7 → 3~6mm 누른 뒤)
                public float WristFreezeDpDeg = 4.0f;      // 피치 오차가 이만큼이면 무장과 무관하게 고정 — 이상 상황 막이 (따라가는 중 공중 최대 ~2°, 바닥에 끼면 +5~9°)
                // ── 첫닿음 (10-02 판정, SagComp_FloorContactsFromTrace 만) ──
                //   꺾임점 결과(Detected·FkZ·CmdZ·JointCmd·JointAct)는 그대로 두고 Kind·FcStage·FcZ·FcCmdZ·FcJointCmd/Act 를 더 채운다 (COjw_12_3D_SagFc.cs).
                //   새 측정은 SagComp_FcPlan 의 시작·한도로 재면 꺾임점이 곧 첫닿음(F0)이다 — 그 궤적도 이것으로 다시 읽어 kind 를 붙인다.
                public bool FirstTouch = false;
                // ── 첫닿음에서 멈춤 (10-04, 처짐 보정 실습 — SagComp_FloorPress 만) ──
                //   켜면 하강 중 틱마다 실시간 첫닿음 검출기(CSagFtLive — 손목 잔차 d14 = a14 − q14 가 명령 하강 1mm 에 꾸준히 커짐, 보조: 공중 기준보다
                //   명령−FK 차가 줄고 어깨 부하가 풀림)를 돌려 걸리는 그 틱에 누름 끝으로 멈춘다 — 얕게 (단단한 정지까지 누르지 않는다:
                //   첫닿음 뒤에도 베이스가 기울며 FK 가 명령을 따라 내려가 꺾임은 첫닿음보다 15~23mm 아래에 생긴다). 끄면(기본) 예전과 같다
                public bool StopAtFirstTouch = false;
                /// <summary>복사본 — 점마다 반경·창을 바꿔 쓸 때 (배열도 새로)</summary>
                public CFloorProbeCfg Clone()
                {
                    var c = (CFloorProbeCfg)MemberwiseClone();
                    if (PosIDs != null) c.PosIDs = (int[])PosIDs.Clone();
                    if (Radii != null) c.Radii = (float[])Radii.Clone();
                    return c;
                }
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
                // 공구 피치 / 손목 수직 유지 (cfg.WristHold)
                public float PitchErrDeg = float.NaN;      // 공중 마지막(닿기 시작 판정 직전) 공구 피치 오차 — 엔코더 FK, 실측 − 명령 (deg). 손목 유지를 꺼도 채운다
                public float PitchErrKnee = float.NaN;     // 꺾임점(닿는 순간) 공구 피치 오차 — 하강 표본 중 꺾임점 명령 높이에 가장 가까운 것. 손목 유지를 꺼도 채운다
                public float WristHoldDeg = float.NaN;     // 고정 순간 14번에 더한 각 (deg) — 손목 유지 끔이면 NaN
                public float PitchErrStart = float.NaN;    // 시작점 자리 잡기 전 피치 오차 — 손목 유지 끔이면 NaN
                public float PitchErrSettled = float.NaN;  // 자리 잡기 뒤 피치 오차 — 손목 유지 끔이면 NaN
                public int WristSettleFix;                 // 자리 잡기에서 고친 횟수
                // 바닥 보정을 켠 채 잰 하강 (10-02) — 꺾임은 유효 명령(이상 명령 cz + 그 틱에 바닥 보정이 끝을 올린 양 fd)으로 맞춘다.
                //   CmdZ 는 이상 명령 높이로 되돌린 값 (사용자가 명령하는 높이), CmdZEff − FdAtKnee = CmdZ
                public float CmdZEff = float.NaN;          // 꺾임점의 유효 명령 높이 (이상 명령 + 바닥 보정) — 보정이 꺼져 있으면 CmdZ 와 같다, 꺾임이 없으면 NaN
                public float FdAtKnee = 0;                 // 꺾임점에서 바닥 보정이 끝을 올린 양 (mm)
                public bool Hold;                          // 손목 수직 유지 켬으로 잰 하강 (궤적 재분석: wh 칸이 찬 무장 표본이 있음)
                public bool UserStop;                      // 하강 중 중단 요청으로 끊긴 누름 (SagComp_FloorPress) — 끝까지 누르지 않았다: 판정·데이터셋에서 뺀다
                // 첫닿음 (궤적 재분석 cfg.FirstTouch 가 채운다 — COjw_12_3D_SagFc.cs). 데이터셋 kind,fc,fccmdz 칸
                public string Kind = "";                   // F = 꺾임점이 곧 첫닿음 (학습은 이것만), H = 꺾임점은 단단한 정지·첫닿음은 위에서 찾음, C = 단단한 정지·첫닿음 못 찾음(검열),
                                                           //   X = 저장된 꺾임을 쓸 수 없음 (FcZ 가 차 있으면 F1 이 위에서 찾은 첫닿음 — 교차 확인용, 학습 안 함), "" = 판정 안 함
                public string FcStage = "";                // 추출 단계 F0 (저장된 꺾임) / F1 (위를 다시 훑음) / C / X
                public float FcZ = float.NaN, FcCmdZ = float.NaN;   // 첫닿음 FK 높이 / 명령 높이 (mm)
                public float[] FcJointCmd, FcJointAct;     // 첫닿음 순간 이상 / 실측 관절각 (PosIDs + Wrist)
                public bool KneeHard;                      // 저장된 꺾임점이 단단한 정지 (피치 오차 증가 > 2°)
                public bool SafetyStop;                    // 안전 정지로 끝난 하강 — 출발 따라잡기 기준이 8 → 15mm
                public int ArmedN;                         // 무장 뒤 하강 표본 수 (15개 미만이면 판정 안 함 → X)
                public string FcWhy = "";                  // 판정 이유 (진단용)
                public bool LiveTouch;                     // StopAtFirstTouch — 실시간 첫닿음 검출기가 걸려 그 틱에 멈췄다
                public string LiveWhy = "";                // 그때 검출기의 까닭 (d14 기울기·e 감소 …)
            }

            // 하강 궤적 진단 — 매 틱 (명령·FK 높이, 차이, 무장, 어깨·팔꿈치 부하·PWM). SagComp_GetFloorTraceCsv 로 꺼낸다.
            private readonly StringBuilder m_sbSagFloor = new StringBuilder();
            public void SagComp_ClearFloorTrace() { lock (m_sbSagFloor) m_sbSagFloor.Length = 0; }
            public string SagComp_GetFloorTraceCsv() { lock (m_sbSagFloor) return m_sbSagFloor.ToString(); }
            private void SagComp_FloorTraceRow(float r, int seg, int tick, bool armed, float cz, float fz, double e, double d, double lag,
                                               float load12, float pwm12, float load13, float pwm13)
            { SagComp_FloorTraceRow(r, seg, tick, armed, cz, fz, e, d, lag, load12, pwm12, load13, pwm13, float.NaN, float.NaN, float.NaN, null, null); }
            // 09-29: x·y·피치와 매 틱 이상(q)·실측(a) 관절각(PosIDs + Wrist 순)까지 — 궤적 재분석이 접촉 순간 자세를 그대로 얻는다 (GP 학습 입력)
            // 10-02: wh = 그 틱에 14번에 얹은 손목 수직 유지 각(손목 유지 끔이면 빈칸), dp = 공구 피치 오차(실측 − 이상, deg — FloorPress 는 늘 채움),
            //        g1..g4 = 서보가 받아 둔 목표 위치(Goal Position, 보정 포함, deg) — g − a = 서보가 못 따라간 양 (손목 정지 마찰 진단)
            //        fd = 그 틱에 바닥 보정(첫닿음 모델·GP·보정표)이 끝을 올린 양 (mm, 보정이 꺼져 있으면 0, FloorProbe 는 빈칸) — cz 는 이상 명령 그대로,
            //             유효 명령 = cz + fd (FloorPress 의 벌어짐·꺾임과 궤적 재분석은 이 값으로). e·lag 도 유효 명령 기준. fd 칸이 없는 옛 궤적은 fd = 0.
            //             칸 자리는 a4 뒤 (wh·dp·g1..g4 는 끝 여섯 칸 그대로) — 읽는 쪽은 머리줄 이름으로 찾는다
            private const string SAG_FLOOR_TRACE_HEAD = "r,seg,tick,armed,cz,fz,e,d,lag,load12,pwm12,load13,pwm13,x,y,pitch,q1,q2,q3,q4,a1,a2,a3,a4,fd,wh,dp,g1,g2,g3,g4";
            private void SagComp_FloorTraceRow(float r, int seg, int tick, bool armed, float cz, float fz, double e, double d, double lag,
                                               float load12, float pwm12, float load13, float pwm13, float x, float y, float pitch, float[] q, float[] a,
                                               float wh = float.NaN, double dp = double.NaN, float[] g = null)
            { SagComp_FloorTraceRow(r, seg, tick, armed, cz, fz, e, d, lag, load12, pwm12, load13, pwm13, x, y, pitch, q, a, wh, dp, g, float.NaN); }
            // fd 까지 (SagComp_FloorPress)
            private void SagComp_FloorTraceRow(float r, int seg, int tick, bool armed, float cz, float fz, double e, double d, double lag,
                                               float load12, float pwm12, float load13, float pwm13, float x, float y, float pitch, float[] q, float[] a,
                                               float wh, double dp, float[] g, float fd)
            {
                var ci = CultureInfo.InvariantCulture;
                Func<float[], int, string> J = (v, i) => (v != null && i < v.Length) ? v[i].ToString("F3", ci) : "";
                lock (m_sbSagFloor)
                {
                    if (m_sbSagFloor.Length == 0) m_sbSagFloor.Append(SAG_FLOOR_TRACE_HEAD + "\r\n");
                    m_sbSagFloor.AppendFormat(ci, "{0:F0},{1},{2},{3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F1},{10:F1},{11:F1},{12:F1},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30}\r\n",
                        r, seg, tick, armed ? 1 : 0, cz, fz, e, d, lag, load12, pwm12, load13, pwm13,
                        float.IsNaN(x) ? "" : x.ToString("F2", ci), float.IsNaN(y) ? "" : y.ToString("F2", ci), float.IsNaN(pitch) ? "" : pitch.ToString("F1", ci),
                        J(q, 0), J(q, 1), J(q, 2), J(q, 3), J(a, 0), J(a, 1), J(a, 2), J(a, 3), float.IsNaN(fd) ? "" : fd.ToString("F3", ci),
                        float.IsNaN(wh) ? "" : wh.ToString("F3", ci), double.IsNaN(dp) ? "" : dp.ToString("F3", ci),
                        J(g, 0), J(g, 1), J(g, 2), J(g, 3));
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
                string whyDef;
                if (!SagDef_ProbeGate(cfg, true, log, out whyDef)) { report = "★로봇 정의: " + whyDef; if (log != null) log(report); return res; }   // 로봇 정의 (10-04) — 예전 경로(PlayXyz) 공구 피치 규칙 포함
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
                                    float[] act = new float[ids.Length];
                                    for (int i = 0; i < ids.Length; i++) act[i] = scene.RawToSim(map[i], b[map[i]][7]);   // 틱 → 3D 각 (장면의 모터 세팅 — 없으면 예전 식)
                                    SagComp_FkCopy(cfg.FuncNumber, ids, act, out fx, out fy, out fz);          // 엔코더 FK TCP — 관절값 사본 (3D 는 명령 자세 그대로)
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
                                for (int i = 0; i < ids.Length; i++) { keep2[i] = GetData(ids[i]); act2[i] = hold.ContainsKey(ids[i]) ? hold[ids[i]] : keep2[i]; }
                                SagComp_FkCopy(cfg.FuncNumber, ids, act2, out hx, out hy, out hfz);           // 관절값 사본
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
            /// 안전: 무장과 무관한 PressSafetyMm 벌어짐·PressSafetyLoadPct 부하 즉시 정지, 반경마다 직전 접촉 기준 시작·한도, 스테일 첫 응답 버림.
            /// 움직임 (10-03, cfg.SumIk): 시작점·하강·들어 올림·복귀 모두 끝 자세를 관절 합을 정한 IK 로 미리 풀고 관절 보간 (SagComp_PressMove) — 틱마다 IK 를
            /// 풀지 않아 공구 피치가 90 에서 접히는 해를 오가지 않는다. 못 가는 시작점은 움직이지 않고 건너뛴다. 장면의 한 틱 관절 변화 막이(작업 중)가 걸리면
            /// 그 자리에서 누르지 않고 위로 복귀한 뒤 멈춘다 (걸린 채 들어오면 누르지 않는다 — 부른 쪽이 StepGuardTrip 을 보고 멈춘다).
            /// 매 틱 FK(이상·실측)는 관절값 사본으로 계산한다 (3D 관절값을 바꾸지 않는다).</summary>
            public List<CFloorContact> SagComp_FloorPress(CScene_t scene, CFloorProbeCfg cfg, CSagComp model,
                                                         CSim2Real.DLog log, CSim2Real.DIsStopped stop, out string report)
            {
                report = "";
                var res = new List<CFloorContact>();
                string whyDef;
                if (!SagDef_ProbeGate(cfg, cfg != null && !cfg.SumIk, log, out whyDef)) { report = "★로봇 정의: " + whyDef; if (log != null) log(report); return res; }   // 로봇 정의 (10-04, 캐시) — 공개 함수라 작업 입구를 거치지 않고 불러도 (SumIk=false 예전 경로는 공구 피치 규칙도)
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
                bool bHold = cfg.WristHold && cfg.WristID >= 0;   // 손목 수직 유지 (측정 동안만 — 꺾는 손목 없는 팔은 없다, 10-13)
                try
                {
                    if (SagComp_MapMissing(ids, map)) { report = "장면에 없는 관절 ID"; return res; }   // 손목 없음(−1) 칸은 관절이 아니다
                    string whyG0;
                    if (SagComp_PressGuardWhy(scene, out whyG0)) { report = whyG0 + " — 걸린 채라 누르지 않음"; if (log != null) log("★" + report); return res; }   // 작업이 걸림을 보고 멈춰야 한다
                    if (log != null) log(string.Format(ci, "바닥 접촉(누르고 들어 올리기): 반경 {0}, 반경마다 {1}회, 하강 {2:F1} mm/s, 누름 판정 벌어짐 {3:F1}mm + 부하 {4:F0}%",
                        string.Join("/", Array.ConvertAll(cfg.Radii, v => v.ToString("F0", ci))), cfg.Repeats, cfg.PressDescentMmPerSec, cfg.PressLagMm, cfg.PressLoadPct));
                    if (bHold)
                    {
                        SagComp_WristHoldBegin(cfg.WristID);
                        if (log != null) log(string.Format(ci, "  손목 수직 유지: 엔코더 FK 공구 피치를 명령 피치에 맞춤 (허용 ±{0:F2}°, 상한 ±{1:F1}°, 하강 중 {2}) — 측정 동안만",
                            cfg.WristHoldTolDeg, cfg.WristHoldMaxDeg, cfg.WristTrack ? "따라가기, 닿기 시작하면 고정" : "고정"));
                    }
                    foreach (float r in cfg.Radii)
                        for (int rep = 0; rep < cfg.Repeats; rep++)
                        {
                            if (stop != null && stop()) return res;
                            float x = (float)(r * Math.Cos(a0)), y = (float)(r * Math.Sin(a0));
                            float zS = float.IsNaN(lastCmd) ? cfg.ZStart : Math.Min(cfg.ZStart, lastCmd + cfg.Margin);
                            float zMin = float.IsNaN(lastCmd) ? cfg.ZMin : Math.Max(cfg.ZMin, lastCmd - cfg.Margin);
                            // 시작점 — 위(+8)로 갔다가 수직으로 내려와 자리 잡는다 (관절 합을 정한 IK 끝 자세로 관절 보간 — SagComp_PressMove).
                            //   중단 요청은 하강과 같이 출력 틱마다 본다 (10-03 검토: 두 다리 — 옮기는 높이에서 시작점까지 첫 행 약 44mm·4초 — 동안 중단을 보지 않아
                            //   다 내려간 뒤에야 원위치를 시작했다). 중단이면 그 틱에서 모션을 멈추고(MoveJoints_Stop) 남은 조각·둘째 다리로 가지 않는다 —
                            //   누르지 않고 돌아가 부른 쪽(작업)이 그 자리에서 원위치한다 (SagArm_Close — 지금 피치 그대로 수직으로 들어 올림).
                            //   중단 확인 싱크는 지금 걸린 모션 싱크를 감싼다 (명령이 가는 곳은 예전 다리와 같다 — 작업 중이면 장면의 SendPose) — 끝나면 그 싱크 그대로
                            string whyMv;
                            bool bApprStop = false;
                            Action<int> sinkPrev = MoveJoints_GetMotionSink();
                            Action<int> sinkAppr = delegate(int nHint)
                            {
                                if (sinkPrev != null) sinkPrev(nHint);
                                if (bApprStop) return;
                                bool bReq = false;
                                try { bReq = stop != null && stop(); } catch { }
                                if (bReq) { bApprStop = true; MoveJoints_Stop(); }                      // 이 틱까지 — 다음 틱은 없다
                            };
                            Func<bool> apprStopped = delegate() { return bApprStop; };
                            int nMv;
                            MoveJoints_SetMotionSink(sinkAppr);
                            try
                            {
                                nMv = SagComp_PressMove(scene, cfg, x, y, zS + 8, cfg.MoveMs, 300, false, false, apprStopped, out whyMv);
                                if (nMv == 0 && !bApprStop && !(stop != null && stop()))
                                    nMv = SagComp_PressMove(scene, cfg, x, y, zS, 1500, 500, true, false, apprStopped, out whyMv);
                            }
                            finally { if (MoveJoints_GetMotionSink() == sinkAppr) MoveJoints_SetMotionSink(sinkPrev); }
                            if (bApprStop || (stop != null && stop()))
                            {
                                if (log != null) log(string.Format(ci, "  r={0:F0}: 시작점으로 가다 중단 요청 — 그 자리에서 멈춤 (누르지 않음)", r));
                                report = sb.ToString(); return res;
                            }
                            if (nMv == 2) { if (log != null) log(string.Format(ci, "  r={0:F0}: ★시작점으로 가다 {1} — 누름 멈춤", r, whyMv)); report = sb.ToString(); return res; }   // 부른 쪽(작업)이 걸림을 보고 멈춘다
                            if (nMv != 0) { if (log != null) log(string.Format(ci, "  r={0:F0}: 시작점 이동 실패 ({1}) — 건너뜀", r, whyMv)); continue; }
                            float[] qStart = new float[ids.Length];                  // 시작점 자세 — 들어 올림·복귀 IK 가 안 되면 여기로 되돌아간다 (지나온 길, 바닥 위)
                            for (int i = 0; i < ids.Length; i++) qStart[i] = GetData(ids[i]);
                            var c = new CFloorContact { R = r, X = x, Y = y, FromPress = true };
                            var dX = new List<double>(); var dY = new List<double>();   // 하강: (유효 명령 높이, FK 높이)
                            var uX = new List<double>(); var uY = new List<double>();   // 들어 올림
                            // 유효 명령 = 이상 명령 cz + 그 틱에 바닥 보정이 끝을 올린 양 fd (10-02) — 보정을 켠 모델로 누르면 fd 가 자세(높이)에 따라 변해
                            //   이상 명령 기준으로는 공중 기울기 1 이 깨지고 공중에서도 벌어짐이 10mm 넘게 쌓여 "무장 전 벌어짐" 안전 정지가 걸린다.
                            //   벌어짐·꺾임·손목 고정은 모두 유효 명령으로 보고, 꺾임점·복귀 높이는 이상 명령 높이로 되돌린다. fd = 0 (보정 끔)이면 예전과 같다
                            var dRaw = new List<double>(); var uRaw = new List<double>();   // dX·uX 와 같은 순서의 이상 명령 높이
                            bool bUserStop = false;                                       // 하강 중 중단 요청
                            float[] pressCmd = null, pressAct = null, lastCmdQ = null, lastActQ = null;
                            float rr = r;
                            // 손목 수직 자리 잡기 — 하강 전에, 멈춘 채로
                            if (bHold)
                            {
                                float dpS0, dpS1; int nFix;
                                SagComp_WristSettle(scene, ids, cfg, stop, out dpS0, out dpS1, out nFix);
                                c.PitchErrStart = dpS0; c.PitchErrSettled = dpS1; c.WristSettleFix = nFix;
                                if (stop != null && stop()) return res;            // 자리 잡기 중 멈춤 요청 — 바닥으로 내려가지 않는다 (finally 가 손목 유지를 놓는다)
                            }
                            var dDp = new List<double>();                          // 하강 표본(dX 와 같은 순서)의 공구 피치 오차 — 꺾임점 값을 뽑는다
                            // 공중 구간 끝(닿기 시작) — 손목 유지는 이때 고정(누름·들어 올림 동안 그대로). 피치 오차는 손목 유지를 꺼도 여기까지 기록한다(켬·끔 비교용)
                            bool bFrozen = false;
                            float holdAtFreeze = float.NaN; double dpAir = double.NaN;

                            // 한 틱 읽기: zz = (명령 z, FK z, 어깨·팔꿈치 부하 %, 어깨·팔꿈치 PWM %), q = 이상 관절, a = 실측 관절, g = 서보 목표 위치. 실패하면 false
                            //   FK 는 관절값 사본으로 (10-03) — 3D 관절값을 잠깐 실측으로 바꿨다 되돌리지 않는다 (CalcF 정적 상태·다른 스레드가 보는 3D 와 섞이지 않게).
                            //   사본도 SetData 와 같은 관절 한계 규칙 (SagFc_Set) 이라 값은 예전과 같다
                            Func<float[], float[], float[], float[], bool> readTick = delegate(float[] zz, float[] q, float[] a, float[] g)
                            {
                                string w; int[][] b = scene.ReadServoBlock(6, out w);
                                if (b == null) return false;
                                double[] adQ = SagFc_MotCopy(), adA = (double[])adQ.Clone();
                                for (int i = 0; i < ids.Length; i++)
                                {
                                    q[i] = SagFc_Get(adQ, ids[i]);
                                    if (map[i] < 0) { a[i] = q[i]; g[i] = q[i]; continue; }        // 꺾는 손목 없음 — 손목 칸 (늘 0, 10-13)
                                    a[i] = scene.RawToSim(map[i], b[map[i]][7]); g[i] = scene.RawToSim(map[i], b[map[i]][1]);   // 틱 → 3D 각은 장면의 모터 세팅으로
                                    SagFc_Set(adA, ids[i], a[i]);
                                }
                                float cx, cy, cz, fx, fy, fz;
                                SagFc_Fk(cfg.FuncNumber, adQ, -1, out cx, out cy, out cz);                 // 이상(명령) TCP
                                SagFc_Fk(cfg.FuncNumber, adA, -1, out fx, out fy, out fz);                 // 엔코더 FK TCP
                                zz[0] = cz; zz[1] = fz; zz[2] = b[map[1]][5] * 0.1f; zz[3] = b[map[2]][5] * 0.1f;
                                zz[4] = b[map[1]][4] * 0.113f; zz[5] = b[map[2]][4] * 0.113f;
                                return true;
                            };

                            // ── ① 누르러 내려가기 ──
                            CSagFtLive ftLive = cfg.StopAtFirstTouch ? new CSagFtLive { NoWrist = cfg.WristID < 0 } : null;      // 첫닿음에서 멈춤 (켰을 때만 — 손목 없음은 팔꿈치 잔차·보조 판정)
                            string ftWhy = null;
                            int nTick = 0; bool bPressed = false, bSafety = false, bArmed = false;
                            float czStart = float.NaN, fzStart = float.NaN, lagArm = 0, base12 = 0, base13 = 0, start12 = 0, start13 = 0;
                            string why = "";
                            Action<int> sinkDown = delegate(int nHint)
                            {
                                scene.SendPose(nHint);
                                if (bPressed || bUserStop) return;
                                bool bStopReq = false;
                                try { bStopReq = stop != null && stop(); } catch { }
                                if (bStopReq) { bUserStop = true; MoveJoints_Stop(); return; }              // 하강 중 중단 — 누르지 않고 아래 복귀(꺾임점 위)로 이어진다
                                float fd = m_fSagLastFloorDz;                                                // 이 틱에 바닥 보정이 끝을 올린 양 (mm) — 보정이 꺼져 있으면 0 (SagComp_JointOutModel)
                                float[] zz = new float[6], q = new float[ids.Length], a = new float[ids.Length], g = new float[ids.Length];
                                if (!readTick(zz, q, a, g)) return;
                                float czRaw = zz[0]; zz[0] = czRaw + fd;                                    // 유효 명령 — 아래 벌어짐·꺾임은 모두 이 값으로
                                nTick++;
                                if (nTick <= cfg.StaleTicks) return;                                        // 스테일 응답 버림
                                lastCmdQ = q; lastActQ = a;
                                if (float.IsNaN(czStart)) { czStart = zz[0]; fzStart = zz[1]; start12 = zz[2]; start13 = zz[3]; }
                                float lag = (czStart - zz[0]) - (fzStart - zz[1]);
                                float dl = bArmed ? Math.Max(Math.Abs(zz[2] - base12), Math.Abs(zz[3] - base13))
                                                  : Math.Max(Math.Abs(zz[2] - start12), Math.Abs(zz[3] - start13));
                                double dP = SagComp_PitchErrTilt(ids, q, a);                                 // 기준 = 이상 합 − τ (손목 공구 기울기 보정, 10-15)
                                SagComp_FloorTraceRow(rr, 1, nTick, bArmed, czRaw, zz[1], zz[0] - zz[1], dl, lag, zz[2], zz[4], zz[3], zz[5], x, y, cfg.RyDeg, q, a,
                                                      bHold ? SagComp_WristHoldNow : float.NaN, dP, g, fd);
                                // 무장 뒤 닿으면(벌어짐) 공중 구간 끝 — 손목 유지는 고정, 그 전에는 따라간다. 피치 오차가 한도를 넘으면 무장과 무관하게 고정(이상 상황)
                                if (!bFrozen)
                                {
                                    if ((bArmed && (lag - lagArm) >= cfg.WristFreezeLagMm) || Math.Abs(dP) >= cfg.WristFreezeDpDeg)
                                    { bFrozen = true; if (bHold) holdAtFreeze = SagComp_WristHoldValue(); }
                                    else
                                    {
                                        if (!double.IsNaN(dP) && Math.Abs(dP) <= SAG_WRIST_DP_SANE) dpAir = dP;
                                        if (bHold && cfg.WristTrack) SagComp_WristHoldStep(dP, cfg.WristTrackGain, cfg.WristHoldTolDeg, cfg.WristTrackMaxStepDeg, cfg.WristHoldMaxDeg);
                                    }
                                }
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
                                if (bArmed) { dX.Add(zz[0]); dY.Add(zz[1]); dDp.Add(dP); dRaw.Add(czRaw); }
                                if (!bStop && ftLive != null && ftLive.Add(zz[0], zz[1], zz[2], CSagFtLive.Residual(q, a, ftLive.NoWrist)))
                                { bStop = true; ftWhy = ftLive.Why; }                                   // 첫닿음 — 이 틱에 멈춘다 (얕게)
                                if (bStop)
                                {
                                    bPressed = true; c.LagAtPress = lag - lagArm; c.LoadDeltaAtPress = dl; c.Ticks = nTick;
                                    pressCmd = q; pressAct = a;
                                    if (!bFrozen) { bFrozen = true; if (bHold) holdAtFreeze = SagComp_WristHoldValue(); }
                                    MoveJoints_Stop();
                                }
                            };
                            MoveJoints_SetMotionSink(sinkDown);
                            int msDown = (int)Math.Max(300, (zS - zMin) / Math.Max(0.5f, cfg.PressDescentMmPerSec) * 1000f);
                            string whyDn = "", whyTrip = "";
                            int nDn;
                            try { nDn = SagComp_PressMove(scene, cfg, x, y, zMin, msDown, 100, true, true, delegate() { return bPressed || bUserStop; }, out whyDn); }
                            finally { MoveJoints_SetMotionSink(scene.SendPose); }
                            if (nDn == 2) whyTrip = whyDn;
                            else if (nDn == 1 && log != null) log(string.Format(ci, "  r={0:F0}: 하강하지 못함 ({1}) — 복귀", r, whyDn));

                            if (bPressed && whyTrip.Length == 0)
                            {
                                // ── ② 누른 채 유지 → ③ 천천히 들어 올리기 ──
                                System.Threading.Thread.Sleep(cfg.PressHoldMs);
                                float hx, hy, hz; SagFc_Fk(cfg.FuncNumber, SagFc_MotCopy(), -1, out hx, out hy, out hz);   // 멈춘 명령(이상) 높이 — 관절값 사본 FK
                                int nUp = 0;
                                Action<int> sinkUp = delegate(int nHint)
                                {
                                    scene.SendPose(nHint);
                                    float fdU = m_fSagLastFloorDz;                                           // 하강과 같이 유효 명령으로
                                    float[] zz = new float[6], q = new float[ids.Length], a = new float[ids.Length], g = new float[ids.Length];
                                    if (!readTick(zz, q, a, g)) return;
                                    float czRawU = zz[0]; zz[0] = czRawU + fdU;
                                    nUp++;
                                    if (nUp <= cfg.StaleTicks) return;
                                    uX.Add(zz[0]); uY.Add(zz[1]); uRaw.Add(czRawU);
                                    SagComp_FloorTraceRow(rr, 3, nUp, true, czRawU, zz[1], zz[0] - zz[1], 0, 0, zz[2], zz[4], zz[3], zz[5], x, y, cfg.RyDeg, q, a,
                                                          bHold ? SagComp_WristHoldNow : float.NaN, SagComp_PitchErrTilt(ids, q, a), g, fdU);
                                };
                                MoveJoints_SetMotionSink(sinkUp);
                                int msUp = (int)Math.Max(300, cfg.LiftMm / Math.Max(0.5f, cfg.LiftMmPerSec) * 1000f);
                                string whyUp = "";
                                int nLift;
                                try { nLift = SagComp_PressMove(scene, cfg, x, y, hz + cfg.LiftMm, msUp, 100, true, true, null, out whyUp); }
                                finally { MoveJoints_SetMotionSink(scene.SendPose); }
                                if (nLift == 2) whyTrip = whyUp;
                                else if (nLift == 1 && log != null) log(string.Format(ci, "  r={0:F0}: 들어 올리지 못함 ({1}) — 바로 복귀", r, whyUp));
                            }
                            // 복귀 — 시작 높이 위, 그리고 꺾임점 위 RetractAboveKneeMm 이상 (유격이 한 번에 떨어져도 바닥에 닿지 않게) 천천히
                            //   꺾임은 유효 명령으로 맞췄으니 이상 명령 높이로 되돌려 쓴다 (복귀 목표는 이상 명령).
                            //   한 틱 관절 변화 막이가 걸렸어도 복귀는 한다 (막이가 3D 를 붙잡은 자세로 되돌려 놓았다 — 거기서 위로 간다. 걸림 표시는 그대로 남아
                            //   부른 쪽(작업)이 보고 멈춘다). 복귀 IK 가 안 되면 시작점 자세로 (지나온 길 — 바닥 위)
                            {
                                var tmpK = new CFloorContact();
                                float zBack = zS + 5;
                                if (SagComp_PressKnee(dX, dY, cfg, tmpK)) zBack = Math.Max(zBack, (float)SagComp_EffToRaw(dX, dRaw, tmpK.CmdZ) + cfg.RetractAboveKneeMm);
                                else if (dRaw.Count > 0) { double lo = double.MaxValue; foreach (double v in dRaw) lo = Math.Min(lo, v); zBack = Math.Max(zBack, (float)lo + cfg.RetractAboveKneeMm); }
                                string whyBk;
                                int nBk = SagComp_PressMove(scene, cfg, x, y, zBack, Math.Max(1500, cfg.RetractMs), 200, true, false, null, out whyBk);
                                if (nBk == 1)
                                {
                                    if (log != null) log(string.Format(ci, "  r={0:F0}: 복귀 경로 안 됨 ({1}) — 시작점 자세로 되돌아감", r, whyBk));
                                    if (cfg.SumIk) SagComp_PlayQ4(ids, qStart, Math.Max(1500, cfg.RetractMs), 200);
                                }
                                else if (nBk == 2 && whyTrip.Length == 0) whyTrip = whyBk;
                            }

                            // ── 꺾임 맞춤 ──
                            c.Safety = bSafety;
                            {
                                double k, y0, sL, sH, rms;
                                SagComp_PressKnee(dX, dY, cfg, c);
                                if (c.FitOk) { c.CmdZEff = c.CmdZ; c.CmdZ = (float)SagComp_EffToRaw(dX, dRaw, c.CmdZEff); c.FdAtKnee = c.CmdZEff - c.CmdZ; }   // 꺾임점 = 이상 명령 높이 (+ 그때 바닥 보정)
                                if (c.FitOk && dDp.Count == dX.Count)
                                {
                                    int best = -1; double bd = double.MaxValue;               // 꺾임점(유효 명령 높이)에 가장 가까운 하강 표본 = 닿는 순간
                                    for (int i = 0; i < dX.Count; i++) { double dd = Math.Abs(dX[i] - c.CmdZEff); if (dd < bd) { bd = dd; best = i; } }
                                    if (best >= 0) c.PitchErrKnee = (float)dDp[best];
                                }
                                if (SagComp_FitHinge(uX.ToArray(), uY.ToArray(), 4, out k, out y0, out sL, out sH, out rms))
                                { c.KneeCmdLift = (float)SagComp_EffToRaw(uX, uRaw, k); c.FkLift = (float)y0; }
                                c.JointCmd = pressCmd ?? lastCmdQ; c.JointAct = pressAct ?? lastActQ;
                                c.Detected = c.FitOk;
                                c.UserStop = bUserStop;
                                if (ftWhy != null) { c.LiveTouch = true; c.LiveWhy = ftWhy; }
                                if (bUserStop) c.Note = c.FitOk ? "하강 중 중단 요청 — 그때까지의 꺾임 맞춤으로 접촉" : "하강 중 중단 요청 — 꺾임 없음";
                                else if (!bPressed && whyTrip.Length > 0) c.Note = "하강 중 멈춤";   // 막이가 걸려 끊긴 하강 — 한도까지 가지 않았다 (까닭은 아래에 붙는다)
                                else if (!bPressed) c.Note = c.FitOk ? "누름 판정 없이 한도 도달 — 꺾임 맞춤으로 접촉" : "한도까지 내려가도 누름·꺾임 없음";
                                else if (!c.FitOk) c.Note = "꺾임 맞춤이 물리적으로 맞지 않음 — 제외";
                                if (bSafety) c.Note += (c.Note.Length > 0 ? ", " : "") + "안전 정지(" + why + ")";
                                if (nDn == 1) c.Note += (c.Note.Length > 0 ? ", " : "") + "하강하지 못함 (" + whyDn + ")";
                                if (whyTrip.Length > 0) { c.Detected = false; c.Note += (c.Note.Length > 0 ? ", " : "") + "★" + whyTrip + " — 제외"; }   // 걸린 누름은 쓰지 않는다
                                if (ftWhy != null) c.Note = "첫닿음에서 멈춤 (" + ftWhy + ")" + (c.Note.Length > 0 ? " — " + c.Note : "");
                            }
                            c.PitchErrDeg = (float)dpAir;
                            if (bHold) c.WristHoldDeg = float.IsNaN(holdAtFreeze) ? SagComp_WristHoldValue() : holdAtFreeze;
                            if (c.Detected) lastCmd = c.CmdZ;
                            res.Add(c);
                            string row = string.Format(ci, "  r={0,3:F0} #{1}: {2}  FK 바닥 {3,6:F2}mm (명령 {4,6:F2})  누름 기울기 {6:F2} 잔차 {7:F2}  | 들어올림 FK {8,6:F2} (명령 {9,6:F2})  | 판정 벌어짐 {10:F1}mm 부하 {11:F1}%{12}",
                                r, rep + 1, c.Detected ? "접촉" : "제외", c.FkZ, c.CmdZ, c.SlopeFree, c.SlopePress, c.FitRms, c.FkLift, c.KneeCmdLift,
                                c.LagAtPress, c.LoadDeltaAtPress, c.Note.Length > 0 ? " — " + c.Note : "");
                            if (c.FitOk && Math.Abs(c.FdAtKnee) > 0.05f)
                                row += string.Format(ci, "  | 바닥 보정 {0:+0.0;-0.0}mm (유효 명령 {1:F2})", c.FdAtKnee, c.CmdZEff);
                            if (bHold)
                                row += string.Format(ci, "  | 손목 {0:+0.00;-0.00}° (피치 오차 {1:+0.00;-0.00} → 자리 잡기 {2:+0.00;-0.00}° {3}회, 공중 마지막 {4:+0.00;-0.00}°, 꺾임점 {5:+0.00;-0.00}°)",
                                    c.WristHoldDeg, c.PitchErrStart, c.PitchErrSettled, c.WristSettleFix, c.PitchErrDeg, c.PitchErrKnee);
                            else row += string.Format(ci, "  | 피치 오차 공중 마지막 {0:+0.00;-0.00}°, 꺾임점 {1:+0.00;-0.00}° (손목 유지 끔)", c.PitchErrDeg, c.PitchErrKnee);
                            if (log != null) log(row); sb.AppendLine(row);
                            if (whyTrip.Length > 0) { report = sb.ToString(); return res; }   // 한 틱 관절 변화 막이 — 다음 반경으로 가지 않는다 (복귀는 했다)
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
                    {
                        // 공구 피치 요약 — 꺾임점(닿는 순간) 피치 오차 (|평균|·최대). 손목 유지를 켰으면 자리 잡기 전 오차·더한 각도 함께
                        double s0 = 0, s1 = 0, m0 = 0, m1 = 0, hMin = double.MaxValue, hMax = double.MinValue; int n0 = 0, n1 = 0;
                        foreach (var q in res)
                        {
                            if (!float.IsNaN(q.PitchErrStart)) { s0 += Math.Abs(q.PitchErrStart); m0 = Math.Max(m0, Math.Abs(q.PitchErrStart)); n0++; }
                            if (!float.IsNaN(q.PitchErrKnee)) { s1 += Math.Abs(q.PitchErrKnee); m1 = Math.Max(m1, Math.Abs(q.PitchErrKnee)); n1++; }
                            if (!float.IsNaN(q.WristHoldDeg)) { hMin = Math.Min(hMin, q.WristHoldDeg); hMax = Math.Max(hMax, q.WristHoldDeg); }
                        }
                        string sumW = null;
                        if (bHold && (n0 > 0 || n1 > 0))
                            sumW = string.Format(ci, "손목 수직 유지: 시작점 피치 오차 |평균| {0:F2}° (최대 {1:F2}°) → 꺾임점 {2:F2}° (최대 {3:F2}°), 손목에 더한 각 {4:+0.00;-0.00}~{5:+0.00;-0.00}°",
                                n0 > 0 ? s0 / n0 : double.NaN, m0, n1 > 0 ? s1 / n1 : double.NaN, m1,
                                hMin == double.MaxValue ? double.NaN : hMin, hMax == double.MinValue ? double.NaN : hMax);
                        else if (!bHold && n1 > 0)
                            sumW = string.Format(ci, "공구 피치(손목 유지 끔): 꺾임점 피치 오차 |평균| {0:F2}° (최대 {1:F2}°)", s1 / n1, m1);
                        if (sumW != null) { if (log != null) log(sumW); sb.AppendLine(sumW); }
                    }
                    report = sb.ToString();
                    return res;
                }
                finally
                {
                    SagComp_Switch(prev);
                    if (bHold) SagComp_WristHoldRelease();   // 다음 동작의 첫 1초에 걸쳐 빠진다 (이어서 측정하면 그 값에서 이어받는다)
                }
            }

            // ── 누름 동작 (10-03) — SagComp_FloorPress·3D 만 누름(SagRun_DryPress) ──
            //   끝 자세 = 관절 합을 정한 IK (SagArm_IkSum — 지금 3D 자세 시드, 관절 합 = SagArm_SumFor(Ry): downsum 또는 downsum + (90 − Ry) — OMX 90 또는 180 − Ry. 손목 원점이 하나로 정해져 공구 피치가
            //   90 에서 접히는 해 고르기가 없다. 관절 한계·TCP 0.5mm 확인) → 그 관절 보간 경로를 관절값 사본 FK 로 미리 훑는다 (SagArm_PlanSplit: TCP 가 두 끝 중
            //   낮은 쪽 아래로 가지 않음, 반경 띠 ±15mm, 수직 다리(bVert)면 수평 벗어남 SumIkDevMm 안 — 넘으면 TCP 가운데 점으로 나눈다) → 조각마다 Play (관절 보간,
            //   시간은 조각의 TCP 거리 비례, 합 = nMs). 틱마다 IK 를 풀지 않으므로 해가 건너뛰지 않고, 관절 합(공구 기울기)은 보간 내내 그대로다.
            //   stopNow 가 true 면 (싱크가 누름·중단을 알림 — 그 싱크가 MoveJoints_Stop 으로 지금 조각을 멈췄다) 남은 조각을 가지 않는다.
            //   cfg.SumIk 가 false 면 예전 방식 (bPathOld ? PlayXyzPath : PlayXyz).
            //   반환: 0 = 끝까지 (또는 stopNow 로 멈춤), 1 = 움직이기 전에 거절 (IK·경로 확인 — why, 3D·실물 그대로), 2 = 움직이는 동안 장면의 한 틱 관절 변화 막이가
            //   새로 걸림 (why — 모션은 막이가 멈췄고 3D 는 붙잡은 자세로 돌아가 있다). scene 이 null(3D 만)이면 막이를 보지 않는다
            private int SagComp_PressMove(CScene_t scene, CFloorProbeCfg cfg, float x, float y, float z, int nMs, int nDelay, bool bVert, bool bPathOld,
                                          Func<bool> stopNow, out string why)
            {
                why = "";
                var ci = CultureInfo.InvariantCulture;
                string t0 = null;
                try { if (scene != null) t0 = scene.StepGuardTrip; } catch { }
                if (!cfg.SumIk)
                {
                    bool bOk = bPathOld ? PlayXyzPath(nMs, nDelay, cfg.FuncNumber, new float[] { x, y, z }, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs)
                                        : PlayXyz(nMs, nDelay, cfg.FuncNumber, x, y, z, cfg.RxDeg, cfg.RyDeg, cfg.RzDeg, cfg.WristID, cfg.PosIDs);
                    if (SagComp_PressNewTrip(scene, t0, out why)) return 2;
                    if (!bOk) { why = "이동 실패"; return 1; }
                    return 0;
                }
                var c = new CSagArmCfg { FuncNumber = cfg.FuncNumber, PosIDs = cfg.PosIDs == null ? null : (int[])cfg.PosIDs.Clone(), WristID = cfg.WristID, HoldIDs = new int[0], DownSumDeg = cfg.DownSumDeg };
                if (!c.IdsOk) { why = "관절 ID 가 맞지 않음 (위치 3개 + 손목)"; return 1; }
                int[] ids = c.QIDs;                                               // 4칸 (손목 없음이면 넷째 −1 — 그 칸은 늘 0, 보내지 않는다)
                double[] ad = SagFc_MotCopy();
                float[] qA = new float[4];
                for (int k = 0; k < 4; k++) qA[k] = SagFc_Get(ad, ids[k]);
                float xa, ya, za;
                SagFc_Fk(c.FuncNumber, ad, -1, out xa, out ya, out za);
                float sumA = qA[1] + qA[2] + qA[3], sumB = SagArm_SumFor(c, cfg.RyDeg), bend;
                float[] qB = SagArm_IkSum(this, c, ad, x, y, z, sumB, out bend);
                if (qB == null) { why = c.NoWrist ? string.Format(ci, "IK 로 못 감 ({0:F1}, {1:F1}, {2:F1}) 위치만 (손목 없음)", x, y, z)
                                                  : string.Format(ci, "IK 로 못 감 ({0:F1}, {1:F1}, {2:F1}) 관절 합 {3:0.#}", x, y, z, sumB); return 1; }
                float[] pA = { xa, ya, za }, pB = { x, y, z };
                double rA = Math.Sqrt((double)xa * xa + (double)ya * ya), rB = Math.Sqrt((double)x * x + (double)y * y);
                float rLo = (float)Math.Min(rA, rB) - SAGARM_BAND_MM, rHi = (float)Math.Max(rA, rB) + SAGARM_BAND_MM;
                var lq = new List<float[]>(); var lp = new List<float[]>();
                string w;
                if (!SagArm_PlanSplit(this, c, ad, qA, pA, qB, pB, sumA, sumB, Math.Min(za, z), rLo, rHi, bVert ? Math.Max(0.1f, cfg.SumIkDevMm) : 0f, 0, lq, lp, out w))
                { why = "관절 보간 경로 확인 실패 — " + w; return 1; }
                double[] dist = new double[lq.Count];
                double sum = 0, dqMax = 0;
                float[] pp = pA, qp = qA;
                for (int i = 0; i < lq.Count; i++)
                {
                    dist[i] = Math.Sqrt((double)(lp[i][0] - pp[0]) * (lp[i][0] - pp[0]) + (double)(lp[i][1] - pp[1]) * (lp[i][1] - pp[1]) + (double)(lp[i][2] - pp[2]) * (lp[i][2] - pp[2]));
                    sum += dist[i]; pp = lp[i];
                    for (int k = 0; k < 4; k++) dqMax = Math.Max(dqMax, Math.Abs(lq[i][k] - qp[k]));
                    qp = lq[i];
                }
                // 관절 최고 속도 상한 — smoothstep 최고 = 평균의 1.5 배가 SAGARM_JOINT_DEG_PER_SEC 를 넘지 않게 시간을 늘린다 (계획의 이웃 점은 몇 도라 그대로,
                //   멀리 떨어진 다음 점으로 가는 시작점 이동만 늘어난다 — 출력 틱마다 0.3~0.9° 안)
                nMs = Math.Max(nMs, (int)(1.5 * dqMax / SAGARM_JOINT_DEG_PER_SEC * 1000.0));
                for (int i = 0; i < lq.Count; i++)
                {
                    if (stopNow != null && stopNow()) return 0;
                    int t = lq.Count == 1 || sum < 1e-6 ? Math.Max(1, nMs) : Math.Max(1, (int)Math.Round(nMs * dist[i] / sum));
                    SagComp_PlayQ4(ids, lq[i], t, i == lq.Count - 1 ? nDelay : 0);
                    if (SagComp_PressNewTrip(scene, t0, out why)) return 2;
                }
                return 0;
            }
            // 장면의 한 틱 관절 변화 막이가 이 동작 동안 새로 걸렸나 (t0 = 동작 전 걸림 글 — 걸림마다 새 글이 들어간다). 막이가 꺼져 있으면 false
            private static bool SagComp_PressNewTrip(CScene_t scene, string t0, out string why)
            {
                why = "";
                try
                {
                    if (scene == null || !scene.StepGuardOn) return false;
                    string t = scene.StepGuardTrip;
                    if (t == null || ReferenceEquals(t, t0)) return false;
                    why = t; return true;
                }
                catch { return false; }
            }
            // 장면의 한 틱 관절 변화 막이가 걸려 있나 (작업 중에만 켜진다 — CScene_t.StepGuard)
            private static bool SagComp_PressGuardWhy(CScene_t scene, out string why)
            {
                why = "";
                try { if (scene != null && scene.StepGuardOn && scene.StepGuardTrip != null) { why = scene.StepGuardTrip; return true; } }
                catch { }
                return false;
            }
            // 장면에 없는 관절이 있나 — map[i] < 0 인 관절 (손목 없음 −1 칸은 관절이 아니라 빼고 본다, 10-13)
            private static bool SagComp_MapMissing(int[] ids, int[] map)
            {
                for (int i = 0; i < ids.Length && i < map.Length; i++) if (ids[i] >= 0 && map[i] < 0) return true;
                return false;
            }
            // Play 값 (ID, 각 …) — 손목 없음(−1) 칸은 보내지 않는다. 손목 로봇은 예전 8칸 그대로
            private static float[] SagComp_PlayVals(int[] ids, float[] q)
            {
                var v = new List<float>();
                for (int k = 0; k < ids.Length && k < q.Length; k++) { if (ids[k] < 0) continue; v.Add(ids[k]); v.Add(q[k]); }
                return v.ToArray();
            }
            // 관절 보간 (위치 3개 + 손목) — Play (블로킹)
            private void SagComp_PlayQ4(int[] ids, float[] q, int nMs, int nDelay)
            {
                Play(nMs, nDelay, SagComp_PlayVals(ids, q));
            }

            /// <summary>시작점에서 손목 수직 자리 잡기 (멈춘 채) — 재고(정착 확인 읽기) → 14번 고치고 → WristSettleWaitMs 기다려 다시 재기.
            /// |피치 오차| ≤ WristHoldTolDeg 이면 끝, 최대 WristSettleIter 번. dp0 = 처음 잰 오차, dpEnd = 마지막으로 잰 오차, nFix = 고친 횟수.
            /// 바뀌는 것은 14번 명령뿐 — 다른 관절은 같은 목표를 다시 보낸다.</summary>
            private void SagComp_WristSettle(CScene_t scene, int[] ids, CFloorProbeCfg cfg, CSim2Real.DIsStopped stop,
                                             out float dp0, out float dpEnd, out int nFix)
            {
                Func<double> read = delegate()
                {
                    string why;
                    Dictionary<int, float> d = scene.ReadDeg(out why);           // 두 번 이상 읽어 정착 확인 — 첫 응답 스테일 문제 없음
                    if (d == null) return double.NaN;
                    float[] q = new float[ids.Length], a = new float[ids.Length];
                    for (int i = 0; i < ids.Length; i++) { q[i] = GetData(ids[i]); float v; a[i] = d.TryGetValue(ids[i], out v) ? v : float.NaN; }
                    return SagComp_PitchErrTilt(ids, q, a);
                };
                SagComp_WristSettleCore(read, delegate() { scene.SendPose(100); }, delegate() { return stop != null && stop(); }, cfg, out dp0, out dpEnd, out nFix);
            }
            // 자리 잡기 반복 — 읽기·보내기를 받아서 (헤드리스 시험에서 가짜 서보로 돌린다)
            private void SagComp_WristSettleCore(Func<double> readPitchErr, Action send, Func<bool> isStopped, CFloorProbeCfg cfg,
                                                 out float dp0, out float dpEnd, out int nFix)
            {
                dp0 = dpEnd = float.NaN; nFix = 0;
                for (int it = 0; it <= cfg.WristSettleIter; it++)
                {
                    if (isStopped != null && isStopped()) return;
                    double dP = readPitchErr();
                    if (double.IsNaN(dP) || Math.Abs(dP) > SAG_WRIST_DP_SANE) return;
                    dpEnd = (float)dP; if (it == 0) dp0 = dpEnd;
                    if (Math.Abs(dP) <= cfg.WristHoldTolDeg || it == cfg.WristSettleIter) return;
                    if (SagComp_WristHoldStep(dP, cfg.WristSettleGain, cfg.WristHoldTolDeg, cfg.WristSettleMaxStepDeg, cfg.WristHoldMaxDeg) == 0) return;   // 상한에 닿음
                    nFix++;
                    send();
                    System.Threading.Thread.Sleep(Math.Max(50, cfg.WristSettleWaitMs));
                }
            }

            /// <summary>유효 명령(이상 명령 + 바닥 보정 fd) 높이 k 를 이상 명령 높이로 — k 에 가장 가까운 표본의 fd 를 뺀다 (fd 는 틱 사이에 매끈하다).
            /// eff·raw = 같은 순서의 유효·이상 명령 높이. 표본이 없거나 길이가 다르면 k 그대로 (fd = 0 이면 늘 k).</summary>
            private static double SagComp_EffToRaw(List<double> eff, List<double> raw, double k)
            {
                if (eff == null || raw == null || eff.Count == 0 || eff.Count != raw.Count || double.IsNaN(k)) return k;
                int best = 0; double bd = double.MaxValue;
                for (int i = 0; i < eff.Count; i++) { double d = Math.Abs(eff[i] - k); if (d < bd) { bd = d; best = i; } }
                return k - (eff[best] - raw[best]);
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
            /// x·y·피치·q·a 칸이 있으면(09-29 부터) 접촉 순간 자세(JointCmd/JointAct)를 채운다 (GP·첫닿음 학습 입력). 옛 궤적은 null.
            /// fd 칸이 있으면(10-02 부터, 바닥 보정을 켠 채 잰 하강) 유효 명령(cz + fd)으로 꺾임을 맞추고 CmdZ 는 이상 명령 높이로 되돌린다
            /// (CmdZEff·FdAtKnee). fd 칸이 없거나 비면 fd = 0 — 예전과 같은 결과.
            /// 여러 궤적을 이어 붙인 파일도 읽는다: 중간의 머리줄은 칸 위치를 다시 읽고(옛 30칸 ↔ fd 칸 31칸이 섞여도), 칸이 모자라거나 숫자가 깨진 줄은 건너뛴다.
            /// cfg.FirstTouch 면 하강마다 첫닿음도 판정한다 (F0/F1/C/X, 어깨 부하·피치 오차·손목 잔차·안전 정지 — COjw_12_3D_SagFc.cs).</summary>
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
                int iLd = Array.IndexOf(hd, "load12"), iDl = Array.IndexOf(hd, "d"), iLag = Array.IndexOf(hd, "lag"), iWh = Array.IndexOf(hd, "wh");   // 첫닿음 판정·손목 유지 표시
                int iFd = Array.IndexOf(hd, "fd");                                                    // 바닥 보정이 끝을 올린 양 (없으면 0)
                const NumberStyles NS = NumberStyles.Float | NumberStyles.AllowThousands;              // float.Parse 기본과 같은 규칙
                Func<string[], int, float> F = (ff, k) => { float v; return (k >= 0 && k < ff.Length && ff[k].Trim().Length > 0 && float.TryParse(ff[k], NS, ci, out v)) ? v : float.NaN; };
                Func<string[], int, double> Dv = (ff, k) => { double v; return (k >= 0 && k < ff.Length && double.TryParse(ff[k], NumberStyles.Float, ci, out v)) ? v : double.NaN; };
                float curR = float.NaN, curX = float.NaN, curY = float.NaN, curP = float.NaN; int curSeg = -1, prevT = int.MaxValue;
                var dX = new List<double>(); var dY = new List<double>(); var dQ = new List<float[]>(); var dA = new List<float[]>();
                var dRaw = new List<double>();                                                        // dX 와 같은 순서의 이상 명령 높이 (dX = 유효 명령 = cz + fd)
                var dL = new List<double>(); var dPe = new List<double>(); var dW = new List<double>();   // 첫닿음: 어깨 부하(%), 공구 피치 오차·손목 잔차 (deg)
                bool bHoldSeg = false, bLastArmed = false; double lastD = double.NaN, lastLag = double.NaN;   // 하강 마지막 줄 — 안전 정지(부하·무장 전 벌어짐)로 끝났는가
                Action flush = delegate()
                {
                    if (curSeg == 1 && !float.IsNaN(curR))
                    {
                        var c = new CFloorContact { R = curR, X = float.IsNaN(curX) ? curR : curX, Y = float.IsNaN(curY) ? 0 : curY,
                                                    Pitch = float.IsNaN(curP) ? 90 : curP, FromPress = true };
                        c.Detected = SagComp_PressKnee(dX, dY, cfg, c);
                        if (c.Detected) { c.CmdZEff = c.CmdZ; c.CmdZ = (float)SagComp_EffToRaw(dX, dRaw, c.CmdZEff); c.FdAtKnee = c.CmdZEff - c.CmdZ; }   // 꺾임점 = 이상 명령 높이 (+ 그때 바닥 보정)
                        if (c.Detected && dQ.Count == dX.Count)
                        {
                            int best = 0; double bd = double.MaxValue;                   // 꺾임점(유효 명령 높이)에 가장 가까운 표본 = 접촉 순간 자세
                            for (int i = 0; i < dX.Count; i++) { double dd = Math.Abs(dX[i] - c.CmdZEff); if (dd < bd) { bd = dd; best = i; } }
                            if (dA[best] != null && !float.IsNaN(dA[best][0])) { c.JointAct = dA[best]; c.JointCmd = dQ[best]; }
                        }
                        c.Note = c.Detected ? "궤적 재분석" : "궤적 재분석 — 꺾임 없음";
                        c.Hold = bHoldSeg;
                        if (cfg.FirstTouch)
                        {
                            SagFc_FirstTouch(c, dX, dY, dL, dPe, dW, dQ, dA,
                                             lastD >= cfg.PressSafetyLoadPct || (!bLastArmed && lastLag >= cfg.PressSafetyMm + cfg.PreArmExtraMm), cfg);
                            if (!float.IsNaN(c.FcCmdZ)) c.FcCmdZ = (float)SagComp_EffToRaw(dX, dRaw, c.FcCmdZ);   // 첫닿음 명령 높이도 이상 명령으로
                        }
                        res.Add(c);
                    }
                    dX = new List<double>(); dY = new List<double>(); dQ = new List<float[]>(); dA = new List<float[]>(); dRaw = new List<double>();
                    dL = new List<double>(); dPe = new List<double>(); dW = new List<double>();
                    bHoldSeg = false; bLastArmed = false; lastD = lastLag = double.NaN;
                };
                for (int li = 1; li < lines.Length; li++)
                {
                    string[] f = lines[li].Split(',');
                    if (f.Length > 1 && f[0].Trim() == "r" && Array.IndexOf(f, "seg") >= 0 && Array.IndexOf(f, "cz") >= 0)
                    {
                        // 이어 붙인 파일의 머리줄 — 지금 하강을 끝내고 칸 위치를 다시 읽는다 (옛 30칸·fd 31칸이 섞여도)
                        flush(); curSeg = -1; prevT = int.MaxValue;
                        string[] h2 = new string[f.Length]; for (int k = 0; k < f.Length; k++) h2[k] = f[k].Trim();
                        int r2 = Array.IndexOf(h2, "r"), s2 = Array.IndexOf(h2, "seg"), t2 = Array.IndexOf(h2, "tick"), a2 = Array.IndexOf(h2, "armed"),
                            c2 = Array.IndexOf(h2, "cz"), z2 = Array.IndexOf(h2, "fz");
                        if (r2 < 0 || s2 < 0 || t2 < 0 || a2 < 0 || c2 < 0 || z2 < 0) continue;              // 모르는 머리줄 — 앞 머리줄 그대로
                        hd = h2; iR = r2; iSeg = s2; iT = t2; iA = a2; iCz = c2; iFz = z2;
                        iX = Array.IndexOf(hd, "x"); iY = Array.IndexOf(hd, "y"); iP = Array.IndexOf(hd, "pitch");
                        iQ = new int[] { Array.IndexOf(hd, "q1"), Array.IndexOf(hd, "q2"), Array.IndexOf(hd, "q3"), Array.IndexOf(hd, "q4") };
                        iAj = new int[] { Array.IndexOf(hd, "a1"), Array.IndexOf(hd, "a2"), Array.IndexOf(hd, "a3"), Array.IndexOf(hd, "a4") };
                        iLd = Array.IndexOf(hd, "load12"); iDl = Array.IndexOf(hd, "d"); iLag = Array.IndexOf(hd, "lag"); iWh = Array.IndexOf(hd, "wh"); iFd = Array.IndexOf(hd, "fd");
                        continue;
                    }
                    if (f.Length < hd.Length) continue;                                                  // 칸이 모자란 줄 (끊긴 줄)
                    float r; int seg, t; double cz, fz;
                    if (!float.TryParse(f[iR], NS, ci, out r) || !int.TryParse(f[iSeg], NumberStyles.Integer, ci, out seg) || !int.TryParse(f[iT], NumberStyles.Integer, ci, out t) ||
                        !double.TryParse(f[iCz], NS, ci, out cz) || !double.TryParse(f[iFz], NS, ci, out fz)) continue;   // 깨진 줄 (하강 경계도 바꾸지 않는다)
                    float fx = F(f, iX), fy = F(f, iY), fp = F(f, iP);
                    bool bNewPt = !(float.IsNaN(fx) && float.IsNaN(curX)) && (fx != curX || fy != curY || fp != curP);
                    if (seg != curSeg || r != curR || t <= prevT || bNewPt) { flush(); curR = r; curSeg = seg; curX = fx; curY = fy; curP = fp; }
                    prevT = t;
                    if (seg == 1)
                    {
                        bool bArm = f[iA].Trim() == "1";
                        bLastArmed = bArm; lastD = Dv(f, iDl); lastLag = Dv(f, iLag);
                        if (bArm)
                        {
                            double fdv = Dv(f, iFd); if (double.IsNaN(fdv)) fdv = 0;
                            dX.Add(cz + fdv); dY.Add(fz); dRaw.Add(cz);
                            dQ.Add(new float[] { F(f, iQ[0]), F(f, iQ[1]), F(f, iQ[2]), F(f, iQ[3]) });
                            dA.Add(new float[] { F(f, iAj[0]), F(f, iAj[1]), F(f, iAj[2]), F(f, iAj[3]) });
                            if (iWh >= 0 && iWh < f.Length && f[iWh].Trim().Length > 0 && f[iWh].Trim() != "NaN") bHoldSeg = true;
                            if (cfg.FirstTouch)
                            {
                                dL.Add(Dv(f, iLd));
                                dPe.Add((Dv(f, iAj[1]) + Dv(f, iAj[2]) + Dv(f, iAj[3])) - (Dv(f, iQ[1]) + Dv(f, iQ[2]) + Dv(f, iQ[3])));   // 실측 합 − 이상 합 (deg)
                                dW.Add(Dv(f, iAj[3]) - Dv(f, iQ[3]));                                                                     // 손목 잔차 a14 − q14
                            }
                        }
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
                double A = 0;
                double[] ad = SagFc_MotCopy();                                                   // 관절값 사본 — 3D 관절값을 바꾸지 않는다
                for (int j = 0; j < nj; j++) SagFc_Set(ad, m.JointIDs[j], q[j]);
                for (int j = 0; j < nj; j++)
                {
                    if (m.Jc[j] == 0) continue;
                    float x, y, zp, zm;
                    SagFc_Set(ad, m.JointIDs[j], q[j] + 0.5f); SagFc_Fk(m.FuncNumber, ad, -1, out x, out y, out zp);
                    SagFc_Set(ad, m.JointIDs[j], q[j] - 0.5f); SagFc_Fk(m.FuncNumber, ad, -1, out x, out y, out zm);
                    SagFc_Set(ad, m.JointIDs[j], q[j]);
                    A += -((zp - zm) / 1.0) * m.Jc[j] * lev[j];
                }
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
                SagComp_FkCopy(cfg.FuncNumber, ids, a, out ax, out ay, out az);                 // 엔코더 FK — 관절값 사본 (3D 는 명령 자세 그대로)
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
