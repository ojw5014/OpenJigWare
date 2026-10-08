using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenJigWare
{
    public partial class Ojw
    {
        /// <summary>
        /// COjwWalking_c — 반걸음(게이트) 단위 보행 생성기.
        ///
        /// 레거시 엔진은 "두 걸음 = 한 사이클"을 한 파라미터 세트로 만든다. 시작/종료 보행은 그 사이클의 반쪽(한 게이트)에
        /// 정지 파라미터 → 보행 파라미터 램프를 넣은 것이다. 여기서는 그 원리를 일반화한다:
        ///   게이트 k = [양발지지 DoubleSteps 스텝] + [한 발 스윙 SingleSteps 스텝]
        ///   게이트 안의 모양(들림 반사인, 전진 램프+타원, Sway 사인, 바운스, 팔·허리)은 레거시 수식 그대로,
        ///   게이트의 "양 끝 상태"(발 위치·앉기·숙이기)만 이전 게이트에서 이어받아 연속성을 보장한다.
        ///   지지발 기준: 몸은 −D_prev/2 에서 +D_k/2 로 전진, 스윙발은 −D_prev 에서 +D_k 로 이동 (D = 착지 보폭).
        ///   D_prev = 0 이면 시작 반걸음, D_k = 0 이면 종료 반걸음 → 시작/반복/종료가 하나의 규칙이 된다.
        ///
        /// 실시간 입력 두 층
        ///   ① 게이트 파라미터(StepParams): 다음 게이트 시작 전 아무 때나 SetNextStep. 진행 중 게이트도 SetCurrentStep 으로
        ///      바꿀 수 있고 BlendSec 동안 선형 블렌드된다(수식이 파라미터에 연속이므로 위치가 튀지 않는다).
        ///   ② 틱 잔차(발 XYZ mm, 발목 롤/피치 도, 몸통 숙임 도, 위상배율, 홀드): 매 Advance 에 그대로 더해진다.
        ///   → 학습 정책의 행동 = ① ∪ ②. 관측용 상태(게이트 번호·지지발·진행률·발 위치)는 프로퍼티로 노출한다.
        ///
        /// 정상 상태(파라미터 불변)에서는 레거시 반복 보행의 발 목표와 일치한다(RunHalfStepTest 로 검증).
        /// 출력은 Frame 이므로 Rig.Solve → IK → 3D/AMP 내보내기 경로를 그대로 쓴다.
        /// </summary>
        public sealed partial class COjwWalking_c
        {
            public enum SwingProfile { Legacy = 0, Cosine = 1 }

            /// <summary>한 게이트(반걸음)의 파라미터. 단위 mm / deg / ms.</summary>
            public sealed class StepParams
            {
                public float StepLength = 80;       // 착지점이 지지발보다 앞선 거리 (레거시 반복 = 2·전진량)
                public float StepWidth = 70.5f;     // 착지 시 두 발 측방 간격 (홈 = 2·|hip y| = 70.5)
                public float StepHeight = 0;        // 착지 지면 높이 − 지지발 지면 높이 (mm, +위) — 계단/단차. 몸은 게이트 동안 절반씩 오르내린다
                public float LiftHeight = 30;       // 발 들림 (지면 보간 위에 더해진다; 계단은 StepHeight 이상으로 줄 것)
                public int SingleSteps = 24;        // 스윙 스텝 수
                public int DoubleSteps = 9;         // 양발지지 스텝 수
                public float StepMs = 23;           // ms/스텝 → 게이트 시간 = (S+D)·StepMs
                public float Sway = 14;             // 지지발 쪽으로의 몸통 횡변위 진폭
                public float Crouch = 30;           // 앉기 (발 Y 오프셋)
                public float Elastic = 5;           // 게이트마다 몸통 바운스
                public float EllipseFront = 35, EllipseBack = 10;
                public float HipSpread = 4;         // 스윙 다리 벌리기 (도)
                public float SwingFootRoll = -20;   // 뜨는 발바닥 롤링 (도, 스윙 발 피치)
                public float AnkleTilt = 0;         // 발목 숙이기 (도, 양발)
                public float HipTilt = 12;          // 고관절 숙이기 (도)
                public float ArmUpShift = 30, ArmUpAngle = -20, ArmWingShift = -5, ArmWingAngle = 5, Waist = 20;
                public float ComForward = 5;        // 무게중심(+전진): 발 Z 에서 뺀다
                public float LegSpread = 0;         // 다리벌리기: 양발 X 대칭 오프셋
                public float AnkleSway = 6;         // 발목SwayOffset(도): 지지발 발목 롤 진폭, sin(π·u). 몸을 지지발 위로 기울인다 (레거시 P{발목 W ID} 채널)
                public SwingProfile Profile = SwingProfile.Legacy;

                public int GateSteps { get { return SingleSteps + DoubleSteps; } }
                public float GateSec { get { return GateSteps * StepMs / 1000f; } }
                public StepParams Clone() { return (StepParams)MemberwiseClone(); }

                /// <summary>레거시(그리드) 파라미터 → 게이트 파라미터. mode 1(반복) 기본.</summary>
                public static StepParams FromEngine(COjwWalking_c e, int mode)
                {
                    StepParams p = new StepParams();
                    p.StepLength = 2f * ParseF(e.GetData_Str(mode, P_STRIDE));
                    p.LiftHeight = ParseF(e.GetData_Str(mode, P_LIFT));
                    p.SingleSteps = ParseI(e.GetData_Str(1, P_SINGLE));
                    p.DoubleSteps = ParseI(e.GetData_Str(1, P_DOUBLE));
                    p.StepMs = ParseF(e.GetData_Str(mode, P_SPEED));
                    p.Sway = ParseF(e.GetData_Str(mode, P_SWAY));
                    p.Crouch = ParseF(e.GetData_Str(mode, P_CROUCH));
                    p.Elastic = ParseF(e.GetData_Str(mode, P_ELASTIC));
                    p.EllipseFront = ParseF(e.GetData_Str(mode, P_ELLIPSE_F));
                    p.EllipseBack = ParseF(e.GetData_Str(mode, P_ELLIPSE_B));
                    p.HipSpread = ParseF(e.GetData_Str(mode, P_SWING_SPREAD));
                    p.SwingFootRoll = ParseF(e.GetData_Str(mode, P_SWING_ROLL));
                    p.AnkleTilt = ParseF(e.GetData_Str(mode, P_ANKLE_TILT));
                    p.HipTilt = ParseF(e.GetData_Str(mode, P_HIPTILT));
                    p.ArmUpShift = -ParseF(e.GetData_Str(mode, P_ARMUP_SHIFT));
                    float cg = ParseF(e.GetData_Str(mode, P_STRIDE)); float b34 = ParseF(e.GetData_Str(mode, P_ARMUP_ANGLE));
                    p.ArmUpAngle = (cg >= 0) ? b34 : -b34;
                    p.ArmWingShift = ParseF(e.GetData_Str(mode, P_ARMW_SHIFT));
                    p.ArmWingAngle = ParseF(e.GetData_Str(mode, P_ARMW_ANGLE));
                    p.Waist = ParseF(e.GetData_Str(mode, P_WAIST_ANGLE));
                    p.ComForward = ParseF(e.GetData_Str(mode, P_COM_FWD));
                    p.LegSpread = ParseF(e.GetData_Str(mode, P_LEG_SPREAD));
                    p.AnkleSway = ParseF(e.GetData_Str(mode, P_ANKLE_SWAY));
                    return p;
                }
                /// <summary>dst = a + (b − a)·t (정수 스텝 수·프로파일은 b 를 따른다).</summary>
                public static void Lerp(StepParams a, StepParams b, float t, StepParams dst)
                {
                    float u = 1f - t;
                    dst.StepLength = a.StepLength * u + b.StepLength * t; dst.StepWidth = a.StepWidth * u + b.StepWidth * t; dst.LiftHeight = a.LiftHeight * u + b.LiftHeight * t;
                    dst.StepHeight = a.StepHeight * u + b.StepHeight * t;
                    dst.SingleSteps = b.SingleSteps; dst.DoubleSteps = b.DoubleSteps; dst.StepMs = a.StepMs * u + b.StepMs * t;
                    dst.Sway = a.Sway * u + b.Sway * t; dst.Crouch = a.Crouch * u + b.Crouch * t; dst.Elastic = a.Elastic * u + b.Elastic * t;
                    dst.EllipseFront = a.EllipseFront * u + b.EllipseFront * t; dst.EllipseBack = a.EllipseBack * u + b.EllipseBack * t;
                    dst.HipSpread = a.HipSpread * u + b.HipSpread * t; dst.SwingFootRoll = a.SwingFootRoll * u + b.SwingFootRoll * t; dst.AnkleTilt = a.AnkleTilt * u + b.AnkleTilt * t;
                    dst.HipTilt = a.HipTilt * u + b.HipTilt * t; dst.ArmUpShift = a.ArmUpShift * u + b.ArmUpShift * t; dst.ArmUpAngle = a.ArmUpAngle * u + b.ArmUpAngle * t;
                    dst.ArmWingShift = a.ArmWingShift * u + b.ArmWingShift * t; dst.ArmWingAngle = a.ArmWingAngle * u + b.ArmWingAngle * t; dst.Waist = a.Waist * u + b.Waist * t;
                    dst.ComForward = a.ComForward * u + b.ComForward * t; dst.LegSpread = a.LegSpread * u + b.LegSpread * t; dst.AnkleSway = a.AnkleSway * u + b.AnkleSway * t; dst.Profile = b.Profile;
                }
                public string ToCsv() { return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24}", StepLength, StepWidth, StepHeight, LiftHeight, SingleSteps, DoubleSteps, StepMs, Sway, Crouch, Elastic, EllipseFront, EllipseBack, HipSpread, SwingFootRoll, AnkleTilt, HipTilt, ArmUpShift, ArmUpAngle, ArmWingShift, ArmWingAngle, Waist, ComForward, LegSpread, AnkleSway, (int)Profile); }
                public const string CsvHeader = "StepLength,StepWidth,StepHeight,LiftHeight,SingleSteps,DoubleSteps,StepMs,Sway,Crouch,Elastic,EllipseFront,EllipseBack,HipSpread,SwingFootRoll,AnkleTilt,HipTilt,ArmUpShift,ArmUpAngle,ArmWingShift,ArmWingAngle,Waist,ComForward,LegSpread,AnkleSway,Profile";
                /// <summary>ToCsv 순서대로 파싱.</summary>
                public static StepParams FromCsv(string[] f, int offset)
                {
                    StepParams p = new StepParams(); int i = offset;
                    p.StepLength = F(f[i++]); p.StepWidth = F(f[i++]); p.StepHeight = F(f[i++]); p.LiftHeight = F(f[i++]); p.SingleSteps = (int)F(f[i++]); p.DoubleSteps = (int)F(f[i++]); p.StepMs = F(f[i++]);
                    p.Sway = F(f[i++]); p.Crouch = F(f[i++]); p.Elastic = F(f[i++]); p.EllipseFront = F(f[i++]); p.EllipseBack = F(f[i++]); p.HipSpread = F(f[i++]); p.SwingFootRoll = F(f[i++]); p.AnkleTilt = F(f[i++]);
                    p.HipTilt = F(f[i++]); p.ArmUpShift = F(f[i++]); p.ArmUpAngle = F(f[i++]); p.ArmWingShift = F(f[i++]); p.ArmWingAngle = F(f[i++]); p.Waist = F(f[i++]); p.ComForward = F(f[i++]); p.LegSpread = F(f[i++]); p.AnkleSway = F(f[i++]); p.Profile = (SwingProfile)(int)F(f[i++]);
                    return p;
                }
                private static float F(string s) { return float.Parse(s, CultureInfo.InvariantCulture); }
                public const int CsvFieldCount = 25;
            }

            /// <summary>
            /// 반걸음 스트리머. Advance(dt) 마다 현재 시각의 Frame 을 계산한다(정수 스텝에 얽매이지 않는 연속 시간).
            /// </summary>
            public sealed class HalfStepStreamer
            {
                public enum Mode { Idle = 0, Walking = 1, Stopping = 2 }
                public readonly Frame Current = new Frame();
                public float HomeHalfWidth = 35.25f;         // 홈 자세 발 측방 |y| (mm) — 스텝 폭의 기준
                public float BlendSec = 0.25f;               // 게이트 도중 파라미터 변경 블렌드 시간
                public float PhaseScale = 1f;                // 위상 배율(잔차 채널)
                public bool Hold = false;                    // 위상 정지(착지 대기)
                // 틱 잔차 (mm / deg): 엔진축 X 측방(+왼쪽) Y 위 Z 전진
                public readonly float[] FootResidualR = new float[3], FootResidualL = new float[3];
                public float AnkleTiltResidualR, AnkleTiltResidualL, AnkleRollResidualR, AnkleRollResidualL, TorsoLeanResidual;

                // 상태
                private Mode m_mode = Mode.Idle;
                private int m_gate = 0;                      // 게이트 번호(0부터)
                private int m_stance = 2;                    // 지지발: 1 오른발, 2 왼발  (첫 게이트는 오른발 스윙 = 왼발 지지, 레거시와 동일)
                private float m_p = 0;                       // 게이트 내 스텝 위치 [0, GateSteps)
                private float m_DPrev = 0;                   // 직전 게이트 보폭 (시작 시 0)
                private float m_stanceX0, m_stanceY0;        // 게이트 시작 시 지지발 위치(몸 기준, 홈 대비 오프셋, 엔진축 X=측방 Z=전진 → 여기서는 X=측방, Z=전진)
                private float m_stanceZ0;
                private float m_swingX0, m_swingZ0;          // 게이트 시작 시 스윙발 위치
                private float m_HPrev = 0;                   // 직전 게이트 착지 높이차 (계단)
                private float m_crouchPrev, m_hipTiltPrev, m_legSpreadPrev, m_comPrev;   // 램프 시작값
                private float m_stopD = float.NaN;
                private int m_gateStepsSeen = 0;             // 게이트 길이 변경 시 진행률 보존용
                private readonly StepParams m_cur = new StepParams();      // 유효(블렌드된) 파라미터
                private readonly StepParams m_from = new StepParams();     // 블렌드 시작값
                private StepParams m_target;                               // 블렌드 목표(현재 게이트)
                private StepParams m_next;                                 // 다음 게이트 파라미터(없으면 현재 유지)
                private float m_blendT = 1f;                               // 블렌드 진행 0..1
                private float m_armPhase = 0;                              // 팔 위상(게이트 단위, 2게이트 주기)
                public float TimeSec { get; private set; }

                public Mode State { get { return m_mode; } }
                public int GateIndex { get { return m_gate; } }
                public int StanceLeg { get { return m_stance; } }
                public int SwingLeg { get { return (m_stance == 1) ? 2 : 1; } }
                public float StepPosition { get { return m_p; } }
                public float GateProgress { get { return (m_cur.GateSteps > 0) ? m_p / m_cur.GateSteps : 0; } }
                public float SwingProgress { get { return SwingS(m_p, m_cur); } }
                public float PrevStepLength { get { return m_DPrev; } }
                public float PrevStepHeight { get { return m_HPrev; } }
                /// <summary>유효 파라미터. 블렌드 중이 아니면(SetCurrentStep 뒤 BlendSec 경과) 필드를 직접 고쳐도 다음 Advance 에 즉시 반영된다.</summary>
                public StepParams Effective { get { return m_cur; } }
                /// <summary>매 틱 정책 출력처럼 "지금 값"을 그대로 쓰고 싶을 때: 블렌드 없이 즉시 적용.</summary>
                public void ApplyImmediate(StepParams p) { Copy(p, m_cur); Copy(p, m_from); m_target = p.Clone(); m_blendT = 1f; }

                public HalfStepStreamer(StepParams initial)
                {
                    m_target = initial.Clone(); Copy(m_target, m_cur); Copy(m_target, m_from);
                }
                private static void Copy(StepParams a, StepParams b) { StepParams.Lerp(a, a, 0f, b); }

                /// <summary>정지 자세에서 시작. 첫 게이트는 오른발 스윙(레거시와 동일). D_prev = 0 이라 자동으로 시작 반걸음이 된다.</summary>
                public void Start()
                {
                    m_mode = Mode.Walking; m_gate = 0; m_stance = 2; m_p = 0; m_DPrev = 0; m_HPrev = 0; TimeSec = 0; m_armPhase = 0; m_stopD = float.NaN;
                    m_stanceX0 = 0; m_stanceZ0 = 0; m_swingX0 = 0; m_swingZ0 = 0; m_stanceY0 = 0;
                    m_crouchPrev = 0; m_hipTiltPrev = 0; m_legSpreadPrev = 0; m_comPrev = 0;   // 정지 자세(오프셋 0)에서 램프
                    m_blendT = 1f; Copy(m_target, m_cur); Copy(m_target, m_from);
                    m_gateStepsSeen = m_cur.GateSteps;
                    Evaluate();
                }
                /// <summary>정지 요청: 다음 게이트를 D=0(옆에 착지)으로 만들고 끝나면 Idle.</summary>
                public void RequestStop() { if (m_mode == Mode.Walking) m_mode = Mode.Stopping; }
                /// <summary>다음 게이트 파라미터 예약(게이트 경계에서 적용).</summary>
                public void SetNextStep(StepParams p) { m_next = p.Clone(); }
                /// <summary>현재 게이트 파라미터 변경(BlendSec 동안 블렌드). blendSec 를 주면 그 시간으로, 0 이면 즉시.</summary>
                public void SetCurrentStep(StepParams p) { SetCurrentStep(p, BlendSec); }
                public void SetCurrentStep(StepParams p, float blendSec)
                {
                    if (blendSec <= 1e-4f) { ApplyImmediate(p); return; }
                    Copy(m_cur, m_from); m_target = p.Clone(); m_blendT = 0f; BlendSec = blendSec;
                }

                private static float SwingS(float p, StepParams c)
                {
                    if (c.SingleSteps <= 0) return 0;
                    float s = (p - c.DoubleSteps) / c.SingleSteps;
                    return s < 0 ? 0 : (s > 1 ? 1 : s);
                }

                /// <summary>dt 초 진행. 반환: Current.Valid.</summary>
                public bool Advance(float dtSec)
                {
                    if (m_mode == Mode.Idle) { Current.Valid = false; return false; }
                    BlendStep(dtSec);
                    float ms = m_cur.StepMs; if (ms <= 0) ms = 50f;
                    float scale = PhaseScale; if (scale < 0.05f) scale = 0.05f;
                    return AdvanceP(Hold ? 0f : (dtSec * 1000f / ms) * scale, dtSec);
                }
                /// <summary>스텝 단위로 진행(정수 스텝 정렬 검증용). 시간은 StepMs 로 환산해 누적.</summary>
                public bool AdvanceSteps(float dSteps)
                {
                    if (m_mode == Mode.Idle) { Current.Valid = false; return false; }
                    float dtSec = dSteps * m_cur.StepMs / 1000f;
                    BlendStep(dtSec);
                    return AdvanceP(dSteps, dtSec);
                }
                private void BlendStep(float dtSec)
                {
                    if (m_blendT < 1f)
                    {
                        m_blendT += (BlendSec > 1e-4f) ? dtSec / BlendSec : 1f; if (m_blendT > 1f) m_blendT = 1f;
                        StepParams.Lerp(m_from, m_target, m_blendT, m_cur);
                    }
                    // 게이트 길이(스텝 수)가 바뀌면 진행률 u 를 보존한다
                    if (m_cur.GateSteps != m_gateStepsSeen && m_gateStepsSeen > 0 && m_cur.GateSteps > 0) { m_p = m_p * m_cur.GateSteps / m_gateStepsSeen; }
                    m_gateStepsSeen = m_cur.GateSteps;
                }
                private bool AdvanceP(float dp, float dtSec)
                {
                    TimeSec += dtSec;
                    m_p += dp;
                    // 게이트 경계
                    while (m_p >= m_cur.GateSteps)
                    {
                        m_p -= m_cur.GateSteps;
                        EndGate();
                        if (m_mode == Mode.Idle) { Current.Valid = false; return false; }
                    }
                    Evaluate();
                    return true;
                }

                private float CurrentD()
                {
                    if (!float.IsNaN(m_stopD)) return m_stopD;
                    return m_cur.StepLength;
                }

                /// <summary>게이트 끝: 지지/스윙 교대, 끝 상태를 다음 시작 상태로.</summary>
                private void EndGate()
                {
                    float D = CurrentD();
                    float H = float.IsNaN(m_stopD) ? m_cur.StepHeight : 0f;
                    // 끝 상태(몸 기준): 지지발 Z = -D/2, 스윙발 Z = +D/2 ; 측방: 스윙발 착지 = 지지발 X ± StepWidth
                    float landX = SwingXEnd();
                    float stanceZEnd = -D * 0.5f, swingZEnd = D * 0.5f;
                    // 교대
                    m_swingX0 = StanceXEnd(); m_swingZ0 = stanceZEnd;             // 옛 지지발 → 새 스윙발
                    m_stanceX0 = landX; m_stanceZ0 = swingZEnd;                   // 착지한 발 → 새 지지발
                    m_stance = (m_stance == 1) ? 2 : 1;
                    m_DPrev = D; m_HPrev = H;
                    m_crouchPrev = m_cur.Crouch; m_hipTiltPrev = m_cur.HipTilt; m_legSpreadPrev = m_cur.LegSpread; m_comPrev = m_cur.ComForward;
                    m_gate++;
                    m_armPhase = m_gate % 2;
                    if (m_mode == Mode.Stopping)
                    {
                        if (!float.IsNaN(m_stopD)) { m_mode = Mode.Idle; m_stopD = float.NaN; return; }   // 종료 반걸음까지 마침
                        m_stopD = 0f;                                                                     // 이번 게이트가 종료 반걸음
                    }
                    if (m_next != null) { Copy(m_cur, m_from); m_target = m_next; m_next = null; m_blendT = 0f; }
                }
                // 게이트 끝 측방 위치(스윙발 착지 X): 지지발 X 에서 StepWidth 만큼 반대쪽 (홈 대비 오프셋으로 표현)
                private float SwingXEnd()
                {
                    float sideSw = (m_stance == 2) ? -1f : 1f;    // 스윙발 측방 부호(+왼쪽)
                    // 홈 위치 ±HomeHalfWidth 기준 오프셋: 착지 폭 W 이면 스윙발 절대 X = 지지발 절대 X + sideSw·W
                    float stanceAbs = -sideSw * HomeHalfWidth + m_stanceX0;         // 지지발 절대(홈+오프셋, 게이트 시작값; 지지발 X 는 게이트 중 불변)
                    float swingAbs = stanceAbs + sideSw * m_cur.StepWidth;
                    return swingAbs - sideSw * HomeHalfWidth;                        // 홈 대비 오프셋
                }
                private float StanceXEnd() { return m_stanceX0; }

                /// <summary>현재 (m_p, m_cur) 로 Frame 계산.</summary>
                private void Evaluate()
                {
                    StepParams c = m_cur; Frame f = Current;
                    int n = c.GateSteps; if (n <= 0) { f.Valid = false; return; }
                    float u = m_p / n;                             // 게이트 진행 0..1
                    float s = SwingS(m_p, c);                      // 스윙 진행 0..1
                    float D = CurrentD();
                    float side = (m_stance == 2) ? 1f : -1f;       // 지지발 측방 부호
                    bool rightSwing = (m_stance == 2);
                    // 전진(Z): 지지발 = 선형 드리프트 (-D_prev/2 → -D/2), 스윙발 = 시작 → 착지 (레거시 램프 + 타원)
                    float stanceZ = m_stanceZ0 + (-D * 0.5f - m_stanceZ0) * u;
                    float prof = (c.Profile == SwingProfile.Cosine) ? (1f - (float)Math.Cos(Math.PI * s)) * 0.5f : s;
                    // 레거시: 스윙발(몸 기준) = 2D·s − (드리프트) − D/2 + 2·fCS  →  지지발 기준으로 −D_prev → +D
                    float swingRelStance = -m_DPrev + (m_DPrev + D) * prof;
                    float bodyRelStance = -stanceZ;                // 지지발 기준 몸 위치 = −(지지발 몸기준)
                    float sinS = (float)Math.Sin(Math.PI * s);
                    float fCR = (c.SingleSteps > 0) ? 2f * s * c.SingleSteps / n : 0f;       // 레거시 fCR = 2·fAM/fZ
                    float ellipse = 2f * (fCR * c.EllipseFront * sinS - (2f - fCR) * c.EllipseBack * sinS);
                    float swingZ = swingRelStance - bodyRelStance + ellipse;
                    // 측방(X): 지지발 고정(게이트 시작값) / 스윙발 시작 → 착지 폭, + 몸 Sway(양발 공통) + 다리벌리기
                    float swingXEnd = SwingXEnd();
                    float swingX = m_swingX0 + (swingXEnd - m_swingX0) * prof;
                    float stanceX = m_stanceX0;
                    float sway = -side * c.Sway * (float)Math.Sin(Math.PI * u);   // 양발 X 에 더함 → 몸은 지지발 쪽으로
                    float legSpread = m_legSpreadPrev + (c.LegSpread - m_legSpreadPrev) * u;
                    // 수직(Y): 앉기 램프 + 들림(스윙) + 바운스 + 계단(지면 높이차)
                    //   지지발 기준 몸 높이: (h − H_prev/2) → (h + H_k/2) 선형. FootY 오프셋(+ = 발이 몸 쪽으로) 는 그만큼 줄어든다.
                    //   스윙발 지면(지지발 지면 기준): −H_prev → +H_k 를 스윙 프로파일로 보간 (전진과 같은 규칙).
                    float H = float.IsNaN(m_stopD) ? c.StepHeight : 0f;
                    float bodyRise = -m_HPrev * 0.5f + (m_HPrev * 0.5f + H * 0.5f) * u;      // 지지발 지면 기준 몸 높이 변화
                    float swingGround = -m_HPrev + (m_HPrev + H) * prof;                       // 스윙발 지면 (지지발 지면 기준)
                    float crouch = m_crouchPrev + (c.Crouch - m_crouchPrev) * u;
                    float elastic = -Math.Abs(c.Elastic * (float)Math.Sin(Math.PI * u)) * (c.Elastic >= 0 ? 1f : -1f);
                    float lift = (float)Math.Round(c.LiftHeight * sinS, 3);
                    float stanceY = crouch + elastic - bodyRise;
                    float swingY = stanceY + swingGround + lift;
                    float com = m_comPrev + (c.ComForward - m_comPrev) * u;
                    float hipTilt = m_hipTiltPrev + (c.HipTilt - m_hipTiltPrev) * u;
                    // 팔·허리 (2게이트 주기, 위상 연속)
                    float armT = (m_armPhase + u) * 0.5f;                                   // 0..1 over two gates
                    float armUp = c.ArmUpAngle * (float)Math.Sin(Math.PI * armT) - c.ArmUpAngle * 0.5f;
                    // 레거시(엑셀 DS/FW 열)는 팔W Shift 를 두 번 더한다(fDS 에 한 번, 출력에서 또 한 번) → 유효 Shift = 2·B37. 동일하게 재현.
                    float armWing = Math.Abs(c.ArmWingAngle * (float)Math.Sin(Math.PI * u)) + 2f * c.ArmWingShift;
                    float waist = c.Waist * (float)Math.Cos(Math.PI * u) * ((m_gate % 2 == 0) ? 1f : -1f);
                    // 채널
                    float spread = c.HipSpread * sinS;
                    float footRoll = c.SwingFootRoll * sinS;
                    float ankleSway = c.AnkleSway * (float)Math.Sin(Math.PI * u);   // 레거시 fBA/fBF: 지지발 발목 롤 = 발목SwayOffset·sin(π·게이트 진행)

                    f.Valid = true; f.Mode = 1; f.Step = (int)m_p + 1; f.FrameCount = n; f.Gate = m_gate; f.GatePos = m_p + 1;
                    f.HasFootR = true; f.HasFootL = true;
                    float swX = swingX + sway, stX = stanceX + sway;
                    if (rightSwing)
                    {
                        f.FootR_X = swX - legSpread; f.FootR_Y = swingY; f.FootR_Z = swingZ - com;
                        f.FootL_X = stX + legSpread; f.FootL_Y = stanceY; f.FootL_Z = stanceZ - com;
                        f.HipSpreadR = spread; f.HipSpreadL = 0; f.AnkleTiltR = footRoll + c.AnkleTilt; f.AnkleTiltL = c.AnkleTilt;
                        f.AnkleSwayL = ankleSway; f.AnkleSwayR = 0;
                        f.SwingLeg = (s > 0 && s < 1) ? 1 : 0;
                    }
                    else
                    {
                        f.FootL_X = swX + legSpread; f.FootL_Y = swingY; f.FootL_Z = swingZ - com;
                        f.FootR_X = stX - legSpread; f.FootR_Y = stanceY; f.FootR_Z = stanceZ - com;
                        f.HipSpreadL = spread; f.HipSpreadR = 0; f.AnkleTiltL = footRoll + c.AnkleTilt; f.AnkleTiltR = c.AnkleTilt;
                        f.AnkleSwayR = ankleSway; f.AnkleSwayL = 0;
                        f.SwingLeg = (s > 0 && s < 1) ? 2 : 0;
                    }
                    f.SwingProgress = s; f.Sway = sway;
                    f.HipTilt = hipTilt; f.HipPanR = 0; f.HipPanL = 0;
                    f.ArmUpR = armUp + c.ArmUpShift; f.ArmUpL = -armUp + c.ArmUpShift;
                    f.ArmWingR = armWing; f.ArmWingL = armWing;
                    f.Waist = waist; f.Neck = 0;
                    f.SpeedMs = c.StepMs; f.DelayMs = 0;
                    // 잔차
                    f.FootR_X += FootResidualR[0]; f.FootR_Y += FootResidualR[1]; f.FootR_Z += FootResidualR[2];
                    f.FootL_X += FootResidualL[0]; f.FootL_Y += FootResidualL[1]; f.FootL_Z += FootResidualL[2];
                    f.AnkleTiltR += AnkleTiltResidualR; f.AnkleTiltL += AnkleTiltResidualL;
                    f.AnkleSwayR += AnkleRollResidualR; f.AnkleSwayL += AnkleRollResidualL;
                    f.HipTilt += TorsoLeanResidual;
                    f.SyncLegacyRaw();                             // ToLegacyString(편집기 명령 문자열 → 3D 미리보기/실기) 용 원값
                }

                /// <summary>관측용 상태 한 줄(csv): gate,stance,p,u,s,DPrev,stanceX0,stanceZ0,swingX0,swingZ0,armPhase,mode</summary>
                public string StateCsv()
                {
                    return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", m_gate, m_stance, m_p, GateProgress, SwingProgress, m_DPrev, m_stanceX0, m_stanceZ0, m_swingX0, m_swingZ0, m_armPhase, (int)m_mode, m_HPrev);
                }
                public const string StateCsvHeader = "gate,stance,p,u,s,DPrev,stanceX0,stanceZ0,swingX0,swingZ0,armPhase,mode,HPrev";
            }

            #region 반걸음 내보내기 (AMP 클립)
            /// <summary>시나리오 콜백: 매 틱 Advance 직전에 호출된다 (streamer, 시각 s, 틱 번호).</summary>
            public delegate void HalfStepScenario(HalfStepStreamer hs, float t, int tick);

            /// <summary>데모 시나리오: 보폭/폭/속도/앉기/계단 변화 + 잔차 + 정지 (RunHalfStepTest 와 같은 순서).</summary>
            public static void DemoScenario(HalfStepStreamer hs, float t, int tick)
            {
                StepParams p0 = hs.Effective;   // 시작 파라미터 기준으로 복제
                if (hs.GateIndex != s_demoLastGate)
                {
                    s_demoLastGate = hs.GateIndex;
                    StepParams cmd = s_demoBase != null ? s_demoBase.Clone() : p0.Clone();
                    switch (s_demoLastGate)
                    {
                        case 0: s_demoBase = p0.Clone(); break;
                        case 2: cmd.StepLength = 100; cmd.LiftHeight = 40; hs.SetNextStep(cmd); break;
                        case 4: cmd.StepWidth = 100; cmd.Sway = 22; cmd.StepMs = s_demoBase.StepMs * 0.7f; hs.SetNextStep(cmd); break;
                        case 6: cmd.StepLength = 40; cmd.Crouch = 45; cmd.HipTilt = 4; hs.SetNextStep(cmd); break;
                        case 7: cmd.StepHeight = 25; cmd.LiftHeight = 40; cmd.Crouch = 40; hs.SetNextStep(cmd); break;
                        case 9: cmd.StepHeight = -25; cmd.LiftHeight = 35; cmd.Crouch = 40; hs.SetNextStep(cmd); break;
                        case 11: hs.RequestStop(); break;
                    }
                }
                float resAmp = 0f;
                if (t >= 3.0f && t < 3.6f) { float e1 = (t - 3.0f) / 0.1f, e2 = (3.6f - t) / 0.1f; resAmp = 12f * Math.Min(1f, Math.Min(e1, e2)); }
                hs.FootResidualL[0] = resAmp; hs.PhaseScale = (t >= 3.0f && t < 3.6f) ? 1.3f : 1f;
            }
            private static int s_demoLastGate = -1; private static StepParams s_demoBase = null;

            /// <summary>
            /// 반걸음 스트리머를 fps 로 재생하며 IK 를 풀고 AMP 클립을 쓴다. 스플라인 없이 연속 시간 평가(프로파일은 파라미터의 Profile).
            /// 시작 반걸음 → 시나리오 → 정지 반걸음까지 포함되므로 비주기 클립이다(루프 검사 없음).
            /// </summary>
            public static ClipResult ExportHalfStepScenario(COjwWalking_c engine, Rig rig, ExportOptions opt, float maxSec, HalfStepScenario scenario)
            {
                return ExportHalfStepScenario(engine, rig, opt, maxSec, scenario, SwingProfile.Legacy);
            }
            public static ClipResult ExportHalfStepScenario(COjwWalking_c engine, Rig rig, ExportOptions opt, float maxSec, HalfStepScenario scenario, SwingProfile profile)
            {
                StepParams p0 = StepParams.FromEngine(engine, 1); p0.Profile = profile;
                HalfStepStreamer hs = new HalfStepStreamer(p0);
                s_demoLastGate = -1; s_demoBase = null;
                hs.Start();
                double dt = 1.0 / opt.Fps;
                int dof = rig.Model.DofCount;
                List<double[]> Q = new List<double[]>(); List<int> hint = new List<int>();
                string extra = engine.GetData_Str(1, P_EXTRA);
                rig.LegR.hasLast = false; rig.LegL.hasLast = false;
                bool ikOk = true; double ikErr = 0;
                int maxTicks = (int)(maxSec / dt);
                for (int tick = 0; tick < maxTicks; tick++)
                {
                    if (scenario != null) scenario(hs, (float)(tick * dt), tick);
                    if (!hs.Advance((float)dt)) break;
                    hint.Add(hs.StanceLeg);
                    double[] q = new double[dof];
                    bool ok = rig.Solve(hs.Current, extra, q);
                    ikOk &= ok; ikErr = Math.Max(ikErr, Math.Max(rig.LegR.LastError, rig.LegL.LastError));
                    Q.Add(q);
                }
                int frames = Q.Count;
                ClipResult res = new ClipResult(); res.Name = opt.Name; res.Fps = opt.Fps; res.IkOk = ikOk; res.IkMaxErrMm = ikErr * 1000.0;
                if (frames < 3) throw new InvalidOperationException("halfstep scenario produced too few frames");
                double[][] q2 = Q.ToArray(); double[][] qd = new double[frames][];
                for (int j = 0; j < frames; j++)
                {
                    qd[j] = new double[dof]; int j0 = Math.Max(0, j - 1), j1 = Math.Min(frames - 1, j + 1); double span = (j1 - j0) * dt;
                    for (int d = 0; d < dof; d++) qd[j][d] = (q2[j1][d] - q2[j0][d]) / span;
                }
                if (!ikOk) res.Warnings.Add("IK not converged on some frames");
                return FinishClip(rig, q2, qd, frames, dt, opt, res, 0, 0, 0, 1, engine.ToStrings(1), hint.ToArray());
            }
            #endregion

            #region 반걸음 검증 + 골든 CSV
            /// <summary>
            /// ① 정상 상태(파라미터 불변)에서 반걸음 스트리머가 레거시 반복 보행의 발 목표와 일치하는지(정수 스텝 시각에서 비교).
            /// ② 게이트마다 파라미터를 바꾸는 시나리오에서 틱 간 최대 점프(연속성)와 게이트 경계 상태.
            /// ③ 시나리오 전체를 골든 CSV 로 기록(파이썬 포팅 대조용).
            /// </summary>
            public static string RunHalfStepTest(COjwWalking_c engine, Rig rig, string goldenCsvPath, out bool pass)
            {
                StringBuilder sb = new StringBuilder(); pass = true;
                double[] qIk = (rig != null) ? new double[rig.Model.DofCount] : null; string extraCmd = engine.GetData_Str(1, P_EXTRA);
                double ikMaxErr = 0; int ikFail = 0; int ikCount = 0;
                if (rig != null) { rig.LegR.hasLast = false; rig.LegL.hasLast = false; }
                StepParams p0 = StepParams.FromEngine(engine, 1);
                int n = engine.FrameCount; float ms = engine.FrameMs(1, 2);
                // ① 정상 상태 대조: 첫 게이트는 시작 반걸음이므로 2 게이트 워밍업 뒤 한 사이클을 비교
                HalfStepStreamer hs = new HalfStepStreamer(p0);
                hs.Start();
                Frame lf = new Frame();
                for (int k = 0; k < 2 * n; k++) hs.AdvanceSteps(1f);       // 2 사이클 워밍업 → 정상 상태 (정수 스텝 정렬)
                double maxDiff = 0; int worst = -1; string worstField = "";
                string[] names = new string[] { "FootR_X", "FootR_Y", "FootR_Z", "FootL_X", "FootL_Y", "FootL_Z", "HipSpreadR", "HipSpreadL", "AnkleTiltR", "AnkleTiltL", "ArmUpR", "ArmWingR", "Waist", "AnkleSwayR", "AnkleSwayL" };
                double[] fieldMax = new double[names.Length];
                for (int k = 1; k <= n; k++)
                {
                    hs.AdvanceSteps(1f);
                    // 반걸음 스트리머 게이트 2·(k-1)+... : 레거시 step k 와 대응 (게이트 0 = 오른발 스윙)
                    engine.Evaluate(1, k, lf);
                    Frame c = hs.Current;
                    double[] diffs = new double[] {
                        Math.Abs(c.FootR_X - lf.FootR_X), Math.Abs(c.FootR_Y - lf.FootR_Y), Math.Abs(c.FootR_Z - lf.FootR_Z), Math.Abs(c.FootL_X - lf.FootL_X), Math.Abs(c.FootL_Y - lf.FootL_Y), Math.Abs(c.FootL_Z - lf.FootL_Z),
                        Math.Abs(c.HipSpreadR - lf.HipSpreadR), Math.Abs(c.HipSpreadL - lf.HipSpreadL), Math.Abs(c.AnkleTiltR - lf.AnkleTiltR), Math.Abs(c.AnkleTiltL - lf.AnkleTiltL), Math.Abs(c.ArmUpR - lf.ArmUpR), Math.Abs(c.ArmWingR - lf.ArmWingR), Math.Abs(c.Waist - lf.Waist), Math.Abs(c.AnkleSwayR - lf.AnkleSwayR), Math.Abs(c.AnkleSwayL - lf.AnkleSwayL) };
                    for (int i = 0; i < diffs.Length; i++) { if (diffs[i] > fieldMax[i]) fieldMax[i] = diffs[i]; if (diffs[i] > maxDiff) { maxDiff = diffs[i]; worst = k; worstField = names[i]; } }
                }
                sb.AppendFormat(CultureInfo.InvariantCulture, "halfstep vs legacy repeat (steady state, {0} steps): max |diff| {1:F4} (mm/deg) at step {2} [{3}]\r\n  per-field max:", n, maxDiff, worst, worstField);
                for (int i = 0; i < names.Length; i++) sb.AppendFormat(CultureInfo.InvariantCulture, " {0}={1:F3}", names[i], fieldMax[i]);
                sb.Append("\r\n");
                if (maxDiff > 0.05) { pass = false; sb.Append("FAIL steady-state mismatch\r\n"); }
                // ② 시나리오: 게이트별 파라미터 변경 + 틱 잔차, 연속성 검사, 골든 CSV
                hs = new HalfStepStreamer(p0); hs.BlendSec = 0.25f; hs.Start();
                float dt = 0.02f;
                double maxJump = 0; int jumpAt = -1; float prevRX = 0, prevRY = 0, prevRZ = 0, prevLX = 0, prevLY = 0, prevLZ = 0; bool havePrev = false;
                StreamWriter w = null;
                if (!string.IsNullOrEmpty(goldenCsvPath)) { Directory.CreateDirectory(Path.GetDirectoryName(goldenCsvPath)); w = new StreamWriter(goldenCsvPath, false, new UTF8Encoding(false)); w.WriteLine("tick,t,dt,cmd," + StepParams.CsvHeader + ",resRX,resRY,resRZ,resLX,resLY,resLZ,phaseScale,hold,eff," + StepParams.CsvHeader + "," + HalfStepStreamer.StateCsvHeader + ",FootR_X,FootR_Y,FootR_Z,FootL_X,FootL_Y,FootL_Z,HipSpreadR,HipSpreadL,AnkleTiltR,AnkleTiltL,HipTilt,ArmUpR,ArmUpL,ArmWingR,ArmWingL,Waist,SwingLeg,SwingProgress,Sway,AnkleSwayR,AnkleSwayL"); }
                int lastGate = -1; string cmdTag = "";
                StepParams cmd = p0.Clone();
                for (int tick = 0; tick < 1400; tick++)
                {
                    float t = tick * dt;
                    // 시나리오 (게이트 경계에서 다음 게이트 예약 / 특정 시각에 현재 게이트 변경 / 틱 잔차)
                    if (hs.GateIndex != lastGate)
                    {
                        lastGate = hs.GateIndex;
                        switch (lastGate)
                        {
                            case 2: cmd = p0.Clone(); cmd.StepLength = 100; cmd.LiftHeight = 40; hs.SetNextStep(cmd); cmdTag = "next:D100;H40"; break;          // 게이트 3 부터 긴 보폭
                            case 4: cmd = p0.Clone(); cmd.StepWidth = 100; cmd.Sway = 22; cmd.StepMs = p0.StepMs * 0.7f; hs.SetNextStep(cmd); cmdTag = "next:W100;S22;fast"; break;   // 옆으로 넓고 빠르게 (기울어짐 대응)
                            case 6: cmd = p0.Clone(); cmd.StepLength = 40; cmd.Crouch = 45; cmd.HipTilt = 4; hs.SetNextStep(cmd); cmdTag = "next:D40;C45;T4"; break;
                            case 7: cmd = p0.Clone(); cmd.StepHeight = 25; cmd.LiftHeight = 40; cmd.Crouch = 40; hs.SetNextStep(cmd); cmdTag = "next:stairs+25"; break;   // 계단 오르기
                            case 9: cmd = p0.Clone(); cmd.StepHeight = -25; cmd.LiftHeight = 35; cmd.Crouch = 40; hs.SetNextStep(cmd); cmdTag = "next:stairs-25"; break;  // 계단 내려가기
                            case 11: hs.RequestStop(); cmdTag = "stop"; break;
                            default: cmdTag = ""; break;
                        }
                    }
                    if (tick == 260) { StepParams c2 = hs.Effective.Clone(); c2.LiftHeight = 20; c2.StepWidth = 85; cmd = c2; hs.SetCurrentStep(c2); cmdTag = "cur:H20;W85"; }   // 게이트 도중 변경(블렌드)
                    // 틱 잔차: 3.0~3.6 s 동안 왼발 옆으로 +12 mm (외란 대응 예, 0.1 s 램프), 위상배율 1.3
                    float resAmp = 0f;
                    if (t >= 3.0f && t < 3.6f) { float e1 = (t - 3.0f) / 0.1f, e2 = (3.6f - t) / 0.1f; resAmp = 12f * Math.Min(1f, Math.Min(e1, e2)); }
                    hs.FootResidualL[0] = resAmp; hs.PhaseScale = (t >= 3.0f && t < 3.6f) ? 1.3f : 1f;
                    bool ok = hs.Advance(dt);
                    if (!ok) { if (w != null) w.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2},idle", tick, t, dt)); break; }
                    Frame c = hs.Current;
                    if (rig != null)
                    {
                        bool ikOk = rig.Solve(c, extraCmd, qIk); ikCount++;
                        if (!ikOk) ikFail++;
                        ikMaxErr = Math.Max(ikMaxErr, Math.Max(rig.LegR.LastError, rig.LegL.LastError));
                    }
                    if (havePrev)
                    {
                        double j = Math.Max(Math.Max(Math.Abs(c.FootR_X - prevRX), Math.Abs(c.FootR_Y - prevRY)), Math.Max(Math.Abs(c.FootR_Z - prevRZ), Math.Max(Math.Max(Math.Abs(c.FootL_X - prevLX), Math.Abs(c.FootL_Y - prevLY)), Math.Abs(c.FootL_Z - prevLZ))));
                        if (j > maxJump) { maxJump = j; jumpAt = tick; }
                    }
                    prevRX = c.FootR_X; prevRY = c.FootR_Y; prevRZ = c.FootR_Z; prevLX = c.FootL_X; prevLY = c.FootL_Y; prevLZ = c.FootL_Z; havePrev = true;
                    if (w != null)
                        w.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36}",
                            tick, t, dt, cmdTag, cmd.ToCsv(), hs.FootResidualR[0], hs.FootResidualR[1], hs.FootResidualR[2], hs.FootResidualL[0], hs.FootResidualL[1], hs.FootResidualL[2], hs.PhaseScale, hs.Hold ? 1 : 0, "eff", hs.Effective.ToCsv(), hs.StateCsv(),
                            c.FootR_X, c.FootR_Y, c.FootR_Z, c.FootL_X, c.FootL_Y, c.FootL_Z, c.HipSpreadR, c.HipSpreadL, c.AnkleTiltR, c.AnkleTiltL, c.HipTilt, c.ArmUpR, c.ArmUpL, c.ArmWingR, c.ArmWingL, c.Waist, c.SwingLeg, c.SwingProgress, c.Sway, c.AnkleSwayR, c.AnkleSwayL));
                    cmdTag = "";
                }
                if (w != null) w.Close();
                sb.AppendFormat(CultureInfo.InvariantCulture, "scenario: {0} gates, max foot jump per 20 ms tick {1:F2} mm at tick {2} (residual on/off steps included), state {3}\r\n", hs.GateIndex, maxJump, jumpAt, hs.State);
                if (maxJump > 15) { pass = false; sb.Append("FAIL discontinuity > 15 mm/tick\r\n"); }
                if (rig != null)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "scenario ik: {0}/{1} frames not converged, max err {2:F3} mm (stairs ±25 mm, D100, W100, fast gate included)\r\n", ikFail, ikCount, ikMaxErr * 1000.0);
                    if (ikFail > 0) sb.Append("  (IK failures mean a step is outside leg reach — reduce StepLength/StepHeight or Crouch for that gate)\r\n");
                }
                if (!string.IsNullOrEmpty(goldenCsvPath)) sb.Append("golden csv: ").Append(goldenCsvPath).Append("\r\n");
                sb.Append(pass ? "HALFSTEP PASS\r\n" : "HALFSTEP FAIL\r\n");
                return sb.ToString();
            }
            #endregion
        }
    }
}
