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
        /// COjwWalking_c — 휴머노이드 보행 생성기(엑셀 이식 COjwWalking)의 실시간·AMP 지향 재구성.
        ///
        /// 층 구조
        ///   Params      : 54개 파라미터를 타입으로 보관(문자열은 레거시 호환용으로만 유지)
        ///   Kernel      : 레거시 MakeWalkingMotion(mode, step) 수식을 "수치 프레임"으로 산출 — 정수 step 에서 비트 단위 동일
        ///   Frame       : 발 목표(엔진축 X=측방(+왼쪽), Y=수직(+위), Z=전진, mm) + 상체/발목/엉치 채널(도) + 위상 정보
        ///   LegacyText  : Frame → 기존 탭 구분 문자열(WalkingMaker/FCommand 호환, 바이트 동일)
        ///   Streamer    : 연속 시간 위상(dt 기반), 시작/반복/종료 상태기계, 잔차 훅
        ///   Amp(.Amp.cs): URDF FK·평발 구속 IK·리타겟·npz/csv 내보내기·자체검증
        ///
        /// 설계 원칙
        ///   - 프레임 계산 경로에 문자열 파싱·예외·할당이 없다(Prepare 이후).
        ///   - 레거시 결과와의 동일성은 GoldenCompare 로 증명한다(같은 DLL 안의 Ojw.COjwWalking 과 직접 비교).
        ///   - 레거시의 알려진 결함(F19 문자열 연결, 목 출력 상수)은 LegacyQuirks 플래그로 재현/수정 선택.
        /// </summary>
        public sealed partial class COjwWalking_c
        {
            public const int PARAM_COUNT = 54;
            public const int MODE_START = 0, MODE_REPEAT = 1, MODE_END = 2;

            /// <summary>파라미터 라벨(레거시 그리드와 동일 순서). 인덱스 i = 엑셀 B(i+4).</summary>
            public static readonly string[] ParamNames = new string[] {
                "첫프레임 속도(종료시 제외)", "Sway 고정(발들어 올리는 시점)", "1점지지 시작구간", "시작점(Sway Sequence Offset)",
                "1점지지 Step 수", "2점지지 Step 수", "Sway", "발들어 올리는 높이", "전진량", "수식 Leg(Right)", "수식 Leg(Left)",
                "전진성 타원(앞)", "전진성 타원(뒤)", "상체 좌우기울임(상체회전각)", "", "무게중심(+전진)", "걸음새를 위한 앉기",
                "다리벌리기 ", "들어올리는 다리 벌리기", "엉치(W) ID(Right)", "엉치(W) ID(Left)", "발목(W) ID(Right)", "발목(W) ID(Left)",
                "발목SwayOffset", "속도시간", "딜레이", "Sway Shift", "팔Up ID(Right)", "팔Up ID(Left)", "팔Up Shift", "팔Up 동작각",
                "팔W ID(Right)", "팔W ID(Left)", "팔W Shift", "팔W 동작각", "걸음시 궤적높이(Elastic)", "평행회전각(대각이동)",
                "슬립 카운터(1점지지보다 숫자가 낮아야 한다.)", "", "고관절 Tilt ID(Right)", "고관절 Tilt ID(Left)", "고관절 숙이기",
                "발목 Tilt ID(Right)", "발목 Tilt ID(Left)", "뜨는 발바닥 롤링 각", "발목 숙이기", "고관절 회전을 이용한 회전각",
                "고관절 Pan(오른다리) ID", "고관절 Pan(왼다리) ID", "덧붙임명령어", "허리ID", "허리 동작각", "목 ID", "목 동작각" };

            // 파라미터 인덱스 상수 (가독성용)
            public const int P_FIRST_SPEED = 0, P_SWAY_HOLD = 1, P_SUPPORT_START = 2, P_SWAY_SEQ = 3, P_SINGLE = 4, P_DOUBLE = 5,
                P_SWAY = 6, P_LIFT = 7, P_STRIDE = 8, P_LEG_R = 9, P_LEG_L = 10, P_ELLIPSE_F = 11, P_ELLIPSE_B = 12, P_BODY_TILT = 13,
                P_COM_FWD = 15, P_CROUCH = 16, P_LEG_SPREAD = 17, P_SWING_SPREAD = 18, P_HIPROLL_R = 19, P_HIPROLL_L = 20,
                P_ANKROLL_R = 21, P_ANKROLL_L = 22, P_ANKLE_SWAY = 23, P_SPEED = 24, P_DELAY = 25, P_SWAY_SHIFT = 26,
                P_ARMUP_R = 27, P_ARMUP_L = 28, P_ARMUP_SHIFT = 29, P_ARMUP_ANGLE = 30, P_ARMW_R = 31, P_ARMW_L = 32,
                P_ARMW_SHIFT = 33, P_ARMW_ANGLE = 34, P_ELASTIC = 35, P_DIAGONAL = 36, P_SLIP = 37, P_HIPTILT_R = 39, P_HIPTILT_L = 40,
                P_HIPTILT = 41, P_ANKTILT_R = 42, P_ANKTILT_L = 43, P_SWING_ROLL = 44, P_ANKLE_TILT = 45, P_HIPPAN_ANGLE = 46,
                P_HIPPAN_R = 47, P_HIPPAN_L = 48, P_EXTRA = 49, P_WAIST = 50, P_WAIST_ANGLE = 51, P_NECK = 52, P_NECK_ANGLE = 53;

            #region 레거시 기본값 (COjwWalking 과 동일)
            private static readonly string[] s_DefaultStart = new string[PARAM_COUNT];
            private static readonly string[] s_DefaultIng = new string[] {
                "", "1", "0", "0", "4", "2", "10", "50", "40", "0", "1", "60", "30", "0", "", "5", "10", "0", "2", "5", "7", "10", "12", "4",
                "50", "0", "0", "1", "3", "0", "0", "2", "4", "-5", "0", "0", "0", "0", "", "", "", "10", "", "", "-10", "", "", "", "", "",
                "", "", "", "" };
            private static readonly string[] s_DefaultEnd = new string[PARAM_COUNT];
            static COjwWalking_c()
            {
                for (int i = 0; i < PARAM_COUNT; i++) { s_DefaultStart[i] = ""; s_DefaultEnd[i] = ""; }
                s_DefaultStart[0] = "500";
            }
            #endregion

            #region 문자열 파라미터 저장소 (레거시 m_aStrParam 와 동일 규약, 3 x 100)
            // 레거시는 string[3,100] 을 쓰고 54 이후 칸은 null 로 남는다. 바이트 동일 출력을 위해 그대로 둔다.
            private readonly string[][] m_raw = new string[3][] { new string[100], new string[100], new string[100] };
            private readonly bool[] m_dirty = new bool[3] { true, true, true };
            private int m_walkingCount = 1;
            private bool m_mirror = false;

            /// <summary>레거시 결함 재현 여부. true(기본): F19 문자열 연결·목 출력 상수까지 동일하게 재현(골든 동일). false: 의미상 올바른 계산.</summary>
            public bool LegacyQuirks = true;

            public COjwWalking_c()
            {
                InitDefaults();
            }

            /// <summary>레거시 InitValue 와 같은 초기값(시작/종료 빈칸은 반복값을 상속).</summary>
            public void InitDefaults()
            {
                for (int j = 0; j < PARAM_COUNT; j++)
                {
                    m_raw[0][j] = (s_DefaultStart[j].Length > 0) ? s_DefaultStart[j] : s_DefaultIng[j];
                    m_raw[1][j] = s_DefaultIng[j];
                    m_raw[2][j] = (s_DefaultEnd[j].Length > 0) ? s_DefaultEnd[j] : s_DefaultIng[j];
                }
                for (int j = PARAM_COUNT; j < 100; j++) { m_raw[0][j] = null; m_raw[1][j] = null; m_raw[2][j] = null; }
                Invalidate(-1);
            }

            public string GetData_Str(int mode, int index) { return m_raw[mode][index]; }
            public void SetData(int mode, int index, string value) { m_raw[mode][index] = value; Invalidate(mode); }
            public int GetWalkingCount() { return m_walkingCount; }
            public int SetWalkingCount(int n) { int v = (n < 1) ? 1 : n; if (v == m_walkingCount) return 0; m_walkingCount = v; return 1; }
            public void SetMirror(bool b) { m_mirror = b; }
            public bool GetMirror() { return m_mirror; }
            public void Invalidate(int mode) { if (mode < 0) { m_dirty[0] = m_dirty[1] = m_dirty[2] = true; } else m_dirty[mode] = true; }

            /// <summary>
            /// 레거시 SetData_Str 와 동일한 병합 규칙(시작/종료 빈칸 → 반복값 상속, 반복 설정 시 시작열 교차 대입 포함).
            /// WalkingMaker 와 같은 순서(1 → 0 → 2)로 호출해야 같은 결과가 된다. 반환: 변경 건수.
            /// </summary>
            public int SetData_Str(int mode, string[] values)
            {
                int changed = 0;
                if (mode != 1) for (int i = 0; i < values.Length; i++) m_raw[mode][i] = "";
                for (int i = 0; i < values.Length; i++)
                {
                    string v = values[i];
                    if (mode == 0)
                    {
                        if (v != "" && v != null) { if (m_raw[0][i] != v) changed++; m_raw[0][i] = v; }
                        else m_raw[0][i] = m_raw[1][i];
                    }
                    else if (mode == 1)
                    {
                        if (m_raw[1][i] != v) changed++;
                        m_raw[1][i] = v;
                        // 레거시 교차 대입: 시작열이 비어 있지 않으면 반복값으로 덮어쓴다 (뒤이은 SetData_Str(0,...) 이 다시 채운다)
                        if (m_raw[0][i] != "") m_raw[0][i] = m_raw[1][i];
                    }
                    else
                    {
                        if (v != "" && v != null) { if (m_raw[2][i] != v) changed++; m_raw[2][i] = v; }
                        else m_raw[2][i] = m_raw[1][i];
                    }
                }
                Invalidate(-1);
                return changed;
            }

            /// <summary>세 모드를 한 번에 설정(WalkingMaker 순서 1→0→2).</summary>
            public void SetAll(string[] start, string[] ing, string[] end)
            {
                SetData_Str(1, ing); SetData_Str(0, start); SetData_Str(2, end);
            }

            public string[] ToStrings(int mode)
            {
                string[] r = new string[PARAM_COUNT];
                for (int i = 0; i < PARAM_COUNT; i++) r[i] = m_raw[mode][i] ?? "";
                return r;
            }
            #endregion

            #region 파일 로더 (WalkingParam.dat / *.prm)
            /// <summary>WalkingParam.dat ("s시작:반복:종료" 한 줄에 한 항목) → 세 배열. 실패 시 false.</summary>
            public static bool LoadWalkingParamDat(string path, out string[] start, out string[] ing, out string[] end)
            {
                start = new string[PARAM_COUNT]; ing = new string[PARAM_COUNT]; end = new string[PARAM_COUNT];
                for (int i = 0; i < PARAM_COUNT; i++) { start[i] = ""; ing[i] = ""; end[i] = ""; }
                if (!File.Exists(path)) return false;
                string[] lines = File.ReadAllLines(path, Encoding.Default);
                int n = Math.Min(lines.Length, PARAM_COUNT);
                for (int i = 0; i < n; i++)
                {
                    string s = lines[i];
                    if (s.Length > 0 && s[0] == 's') s = s.Substring(1);
                    string[] p = s.Split(':');
                    if (p.Length == 3) { start[i] = p[0]; ing[i] = p[1]; end[i] = p[2]; }
                }
                return true;
            }

            /// <summary>
            /// WalkingMaker *.prm 로더. 파일 규약: 라벨들 → "[Name]" → 시작값들 → "[Start]" → 반복값들 → "[Repeat]" → 종료값들 → "[End]".
            /// (구분자 이름과 내용이 한 칸 어긋나 있는 것이 원본 규약이다.)
            /// </summary>
            public static bool LoadPrm(string path, out string[] start, out string[] ing, out string[] end)
            {
                start = new string[PARAM_COUNT]; ing = new string[PARAM_COUNT]; end = new string[PARAM_COUNT];
                for (int i = 0; i < PARAM_COUNT; i++) { start[i] = ""; ing[i] = ""; end[i] = ""; }
                if (!File.Exists(path)) return false;
                string[] lines = File.ReadAllLines(path, Encoding.Default);
                List<string> items = new List<string>();
                foreach (string l in lines) items.Add((l.Length > 0 && l[0] == 's') ? l.Substring(1) : l);
                int iName = -1, iStart = -1, iRepeat = -1, iEnd = -1;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].IndexOf("[Name]") == 0) iName = i;
                    else if (items[i].IndexOf("[Start]") == 0) iStart = i;
                    else if (items[i].IndexOf("[Repeat]") == 0) iRepeat = i;
                    else if (items[i].IndexOf("[End]") == 0) iEnd = i;
                }
                if (iName < 0 || iStart < 0 || iRepeat < 0) return false;
                if (iEnd < 0) iEnd = items.Count;
                CopySection(items, iName + 1, iStart, start);
                CopySection(items, iStart + 1, iRepeat, ing);
                CopySection(items, iRepeat + 1, iEnd, end);
                return true;
            }
            private static void CopySection(List<string> items, int from, int to, string[] dst)
            {
                int k = 0;
                for (int i = from; i < to && k < dst.Length; i++, k++) dst[k] = items[i] ?? "";
            }
            #endregion

            #region 숫자 파싱 (레거시 CConvert 와 동일한 결과, 예외 없음)
            private static float ParseF(string s)
            {
                if (string.IsNullOrEmpty(s)) return 0f;
                float v;
                if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out v)) return v;
                return 0f;
            }
            private static int ParseI(string s)
            {
                if (string.IsNullOrEmpty(s)) return 0;
                int v;
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out v)) return v;
                return 0;
            }
            private const float DEG_TO_RAD = (float)(Math.PI / 180.0);
            #endregion

            #region 모드별 준비값 (레거시 UpdateParamCache 와 동일 + 시작/종료 보간 양끝값)
            private sealed class ModeCache
            {
                public float fO, fP, fQ, fS, fT, fW, fX, fY, fZ, fAA, fAB, fAT, fAU, fBG, fD50, fBW, fE19, fF19, fI19, fI20;
                public float fCB, fCB_Walk, fCB_Start_End, fCG, fCN, fCO, fE17, fE18, fCZ, fDK, fDQ, fDR, fDW, fDX, fEQ, fER, fBM, fBN, fDM, fDN, fDU, fIB, fIE;
                // 시작/종료 모드에서 프레임마다 다시 파싱하던 값들(반복 모드 기준값 포함)
                public float cz_m, cz_1, dn_m, dn_1, dk_0, dk_1, dm_0, dm_1, dq_0, dq_1, eq_0, eq_1, fd_0, fd_1, fd_repeat;
                public bool hasHipTiltL, hasNeck;
                public float sinDX, cosDX;
                public float speedMs, firstMs, delayMs;
                public int gate;
            }
            private readonly ModeCache[] m_cache = new ModeCache[3] { new ModeCache(), new ModeCache(), new ModeCache() };
            private int m_frameCount = 0;

            private void Prepare(int mode)
            {
                if (!m_dirty[mode] && !m_dirty[1]) return;
                // 반복 모드가 바뀌면 시작/종료의 상속값도 바뀐다 → 셋 다 갱신
                for (int m = 0; m < 3; m++) if (m_dirty[m] || m_dirty[1]) BuildCache(m);
                m_dirty[0] = m_dirty[1] = m_dirty[2] = false;
                m_frameCount = Size_Frame(1);
            }

            private void BuildCache(int nMode)
            {
                string[] S = m_raw[nMode];
                string[] R = m_raw[1];
                ModeCache c = m_cache[nMode];
                c.fO = ParseF(S[1]);
                c.fP = -ParseF(S[6]);
                c.fQ = c.fP * 0.5f;
                c.fS = ParseF(S[3]);
                c.fT = -ParseF(S[26]);
                c.fW = -ParseF(S[2]);
                c.fX = ParseF(S[4]);
                c.fY = ParseF(S[5]);
                c.fZ = c.fX + c.fY;
                float fAA_temp = ParseF(S[37]);
                c.fAB = -fAA_temp;
                c.fAA = (fAA_temp > 0 ? fAA_temp : 0.0f);
                c.fAB = (c.fAB > 0 ? c.fAB : 0.0f);
                c.fAT = ParseF(S[23]);
                c.fAU = c.fAT * 0.5f;
                c.fBG = ParseF(S[18]);
                c.fD50 = ParseF(S[46]);
                c.fBW = ParseF(S[7]);
                c.fE19 = 72;
                if (LegacyQuirks)
                    c.fF19 = ParseF(S[17] + c.fE19);           // 레거시 결함 재현: 문자열 연결 ("10"+72 → 1072)
                else
                    c.fF19 = ParseF(S[17]) + c.fE19;           // 엑셀 원본: F19 = B21 + 72
                float fB17 = ParseF(S[13]);
                c.fI19 = c.fF19 / 2 * (float)Math.Tan(fB17 * DEG_TO_RAD);
                c.fI20 = -c.fI19;
                c.fCB = ParseF(S[16]);
                c.fCB_Walk = ParseF(R[16]);
                c.fCB_Start_End = c.fCB;
                c.fCG = ParseF(S[8]);
                c.fCN = ParseF(S[11]);
                c.fCO = ParseF(S[12]);
                c.fE17 = -fB17;
                c.fE18 = fB17;
                c.fCZ = ParseF(S[15]);
                c.fDK = -ParseF(S[17]);
                c.fDQ = ParseF(S[33]);
                c.fDR = ParseF(S[34]);
                c.fDW = ParseF(S[36]) * ((nMode != 1) ? -1 : 1);
                c.fDX = c.fDW * DEG_TO_RAD;
                c.fEQ = ParseF(S[45]);
                c.fER = ParseF(S[44]);
                c.fBM = ParseF(S[47]);
                c.fBN = ParseF(S[48]);
                c.fDM = -ParseF(S[29]);
                float fCG_val = ParseF(S[8]);
                float fB34 = ParseF(S[30]);
                c.fDN = (fCG_val >= 0) ? fB34 : -fB34;
                c.fDU = ParseF(S[35]);
                c.fIB = ParseF(S[51]);
                c.fIE = ParseF(S[53]);
                // 시작/종료 보간용 양끝값
                c.cz_m = ParseF(S[15]); c.cz_1 = ParseF(R[15]);
                c.dn_m = (ParseF(S[8]) >= 0) ? ParseF(S[30]) : -ParseF(S[30]);
                c.dn_1 = (ParseF(R[8]) >= 0) ? ParseF(R[30]) : -ParseF(R[30]);
                c.dk_0 = -ParseF(R[17]); c.dk_1 = -ParseF(S[17]);
                c.dm_0 = -ParseF(R[29]); c.dm_1 = -ParseF(S[29]);
                c.dq_0 = ParseF(R[33]); c.dq_1 = ParseF(S[33]);
                c.eq_0 = ParseF(R[45]); c.eq_1 = ParseF(S[45]);
                c.fd_0 = ParseF(R[41]); c.fd_1 = ParseF(S[41]); c.fd_repeat = ParseF(S[41]);
                c.hasHipTiltL = !string.IsNullOrEmpty(S[40]);
                c.hasNeck = !string.IsNullOrEmpty(S[52]);
                c.sinDX = (float)Math.Sin(c.fDX);
                c.cosDX = (float)Math.Cos(c.fDX);
                c.speedMs = ParseF(S[24]); c.firstMs = ParseF(S[0]); c.delayMs = ParseF(S[25]);
                c.gate = ParseI(S[4]) + ParseI(S[5]);
            }

            public int Size_Gate(int mode) { return (int)(ParseI(m_raw[mode][4]) + ParseI(m_raw[mode][5])); }
            public int Size_Frame(int mode) { return Size_Gate(mode) * ((mode == 1) ? 2 : 1); }
            /// <summary>레거시와 같이 모든 모드가 반복 모드의 프레임 수(2·게이트)를 쓴다.</summary>
            public int FrameCount { get { Prepare(1); return m_frameCount; } }
            public int GateSize { get { Prepare(1); return m_cache[1].gate; } }
            /// <summary>프레임당 시간(ms) = 속도시간(B28). 시작 모드 1번 프레임은 첫프레임 속도(B4).</summary>
            public float FrameMs(int mode, int step)
            {
                Prepare(mode);
                if (mode == 0 && step == 1) return m_cache[0].firstMs;
                return m_cache[mode].speedMs;
            }
            #endregion

            #region Frame (수치 출력)
            /// <summary>한 프레임의 수치 결과. 호출자가 버퍼를 소유하고 Evaluate 가 덮어쓴다.</summary>
            public sealed class Frame
            {
                public bool Valid;
                public int Mode, Step, FrameCount;
                // 발 목표 (엔진축: X 측방 +왼쪽, Y 수직 +위, Z 전진, mm) — 영자세 발 위치에 대한 상대값 (레거시 'I' 증분 규약)
                public float FootR_X, FootR_Y, FootR_Z, FootL_X, FootL_Y, FootL_Z;
                public bool HasFootR, HasFootL;
                public float Sway;                          // fU (양발 X 에 공통으로 들어간 체중이동)
                public int Gate;                            // 0: 첫 게이트(오른발 스윙), 1: 둘째(왼발 스윙)
                public float GatePos;                       // fAC (1..게이트)
                public int SwingLeg;                        // 0 없음(양발지지), 1 오른발, 2 왼발
                public float SwingProgress;                 // 0..1 (슬립카운터 반영 후 fAM/fX 또는 fAQ/fX)
                // 의미 채널(도). 부호는 "의미" 기준: 벌리기 +=바깥, 숙이기 +=앞으로, 팔Up +=앞으로 등. 레거시 문자열 값은 별도 보관.
                public float HipSpreadR, HipSpreadL;        // 들어올리는 다리 벌리기 (스윙 다리)
                public float AnkleSwayL, AnkleSwayR;        // 발목 Sway 오프셋(지지 발목 롤) + 상체 좌우기울임 상수
                public float AnkleTiltR, AnkleTiltL;        // 뜨는 발바닥 롤링(스윙 발 피치) + 발목 숙이기
                public float HipTilt;                       // 고관절 숙이기(양쪽 고관절 피치 가산)
                public float HipPanR, HipPanL;              // 고관절 회전(요) — 이 로봇엔 없음
                public float ArmUpR, ArmUpL, ArmWingR, ArmWingL;
                public float Waist, Neck;
                public float SpeedMs, DelayMs;
                // 레거시 문자열용 원값
                internal float lg_fBA_fCX, lg_fBF_fCY, lg_fBI, lg_fBK, lg_fDO_fDM, lg_fDP_fDM, lg_fDS_fDQ, lg_fDT_fDQ, lg_fES, lg_fET, lg_fFD, lg_fBT, lg_fBU, lg_fIC, lg_neck;
                internal float lg_fEK, lg_fEL, lg_fEM, lg_fEN, lg_fEO, lg_fEP;

                public void CopyFrom(Frame o)
                {
                    Valid = o.Valid; Mode = o.Mode; Step = o.Step; FrameCount = o.FrameCount;
                    FootR_X = o.FootR_X; FootR_Y = o.FootR_Y; FootR_Z = o.FootR_Z; FootL_X = o.FootL_X; FootL_Y = o.FootL_Y; FootL_Z = o.FootL_Z;
                    HasFootR = o.HasFootR; HasFootL = o.HasFootL; Sway = o.Sway; Gate = o.Gate; GatePos = o.GatePos; SwingLeg = o.SwingLeg; SwingProgress = o.SwingProgress;
                    HipSpreadR = o.HipSpreadR; HipSpreadL = o.HipSpreadL; AnkleSwayL = o.AnkleSwayL; AnkleSwayR = o.AnkleSwayR;
                    AnkleTiltR = o.AnkleTiltR; AnkleTiltL = o.AnkleTiltL; HipTilt = o.HipTilt; HipPanR = o.HipPanR; HipPanL = o.HipPanL;
                    ArmUpR = o.ArmUpR; ArmUpL = o.ArmUpL; ArmWingR = o.ArmWingR; ArmWingL = o.ArmWingL; Waist = o.Waist; Neck = o.Neck;
                    SpeedMs = o.SpeedMs; DelayMs = o.DelayMs;
                    lg_fBA_fCX = o.lg_fBA_fCX; lg_fBF_fCY = o.lg_fBF_fCY; lg_fBI = o.lg_fBI; lg_fBK = o.lg_fBK; lg_fDO_fDM = o.lg_fDO_fDM; lg_fDP_fDM = o.lg_fDP_fDM; lg_fDS_fDQ = o.lg_fDS_fDQ; lg_fDT_fDQ = o.lg_fDT_fDQ;
                    lg_fES = o.lg_fES; lg_fET = o.lg_fET; lg_fFD = o.lg_fFD; lg_fBT = o.lg_fBT; lg_fBU = o.lg_fBU; lg_fIC = o.lg_fIC; lg_neck = o.lg_neck;
                    lg_fEK = o.lg_fEK; lg_fEL = o.lg_fEL; lg_fEM = o.lg_fEM; lg_fEN = o.lg_fEN; lg_fEO = o.lg_fEO; lg_fEP = o.lg_fEP;
                }
                /// <summary>두 프레임의 연속 채널을 선형 보간(스트리머용). 이산 채널은 a 를 따른다.</summary>
                public static void Lerp(Frame a, Frame b, float t, Frame dst)
                {
                    dst.CopyFrom(a);
                    float u = 1f - t;
                    dst.FootR_X = a.FootR_X * u + b.FootR_X * t; dst.FootR_Y = a.FootR_Y * u + b.FootR_Y * t; dst.FootR_Z = a.FootR_Z * u + b.FootR_Z * t;
                    dst.FootL_X = a.FootL_X * u + b.FootL_X * t; dst.FootL_Y = a.FootL_Y * u + b.FootL_Y * t; dst.FootL_Z = a.FootL_Z * u + b.FootL_Z * t;
                    dst.Sway = a.Sway * u + b.Sway * t;
                    dst.HipSpreadR = a.HipSpreadR * u + b.HipSpreadR * t; dst.HipSpreadL = a.HipSpreadL * u + b.HipSpreadL * t;
                    dst.AnkleSwayL = a.AnkleSwayL * u + b.AnkleSwayL * t; dst.AnkleSwayR = a.AnkleSwayR * u + b.AnkleSwayR * t;
                    dst.AnkleTiltR = a.AnkleTiltR * u + b.AnkleTiltR * t; dst.AnkleTiltL = a.AnkleTiltL * u + b.AnkleTiltL * t;
                    dst.HipTilt = a.HipTilt * u + b.HipTilt * t; dst.HipPanR = a.HipPanR * u + b.HipPanR * t; dst.HipPanL = a.HipPanL * u + b.HipPanL * t;
                    dst.ArmUpR = a.ArmUpR * u + b.ArmUpR * t; dst.ArmUpL = a.ArmUpL * u + b.ArmUpL * t; dst.ArmWingR = a.ArmWingR * u + b.ArmWingR * t; dst.ArmWingL = a.ArmWingL * u + b.ArmWingL * t;
                    dst.Waist = a.Waist * u + b.Waist * t; dst.Neck = a.Neck * u + b.Neck * t;
                    dst.lg_fBA_fCX = a.lg_fBA_fCX * u + b.lg_fBA_fCX * t; dst.lg_fBF_fCY = a.lg_fBF_fCY * u + b.lg_fBF_fCY * t; dst.lg_fBI = a.lg_fBI * u + b.lg_fBI * t; dst.lg_fBK = a.lg_fBK * u + b.lg_fBK * t;
                    dst.lg_fDO_fDM = a.lg_fDO_fDM * u + b.lg_fDO_fDM * t; dst.lg_fDP_fDM = a.lg_fDP_fDM * u + b.lg_fDP_fDM * t; dst.lg_fDS_fDQ = a.lg_fDS_fDQ * u + b.lg_fDS_fDQ * t; dst.lg_fDT_fDQ = a.lg_fDT_fDQ * u + b.lg_fDT_fDQ * t;
                    dst.lg_fES = a.lg_fES * u + b.lg_fES * t; dst.lg_fET = a.lg_fET * u + b.lg_fET * t; dst.lg_fFD = a.lg_fFD * u + b.lg_fFD * t; dst.lg_fBT = a.lg_fBT * u + b.lg_fBT * t;
                    dst.lg_fBU = a.lg_fBU * u + b.lg_fBU * t; dst.lg_fIC = a.lg_fIC * u + b.lg_fIC * t; dst.lg_neck = a.lg_neck * u + b.lg_neck * t; dst.lg_fEK = a.lg_fEK * u + b.lg_fEK * t;
                    dst.lg_fEL = a.lg_fEL * u + b.lg_fEL * t; dst.lg_fEM = a.lg_fEM * u + b.lg_fEM * t; dst.lg_fEN = a.lg_fEN * u + b.lg_fEN * t; dst.lg_fEO = a.lg_fEO * u + b.lg_fEO * t;
                    dst.lg_fEP = a.lg_fEP * u + b.lg_fEP * t;
                    if (t >= 0.5f) { dst.SwingLeg = b.SwingLeg; dst.Gate = b.Gate; dst.Step = b.Step; }
                    dst.SwingProgress = a.SwingProgress * u + b.SwingProgress * t;
                    if (a.SwingLeg != b.SwingLeg) dst.SwingProgress = (t >= 0.5f) ? b.SwingProgress : a.SwingProgress;
                }
                /// <summary>
                /// 의미 채널 → 레거시 문자열 원값(lg_*). 커널(EvaluateCore) 밖에서 만든 프레임(반걸음 생성기 등)을 ToLegacyString 으로
                /// 편집기 명령 문자열(E/I/S/D/P/T/G 토큰)로 바꿀 때 쓴다. 부호 규약은 EvaluateCore 와 동일: 다리 벌리기(HipSpread)만 방향상수 -1(fBH/fBJ)이 곱해진다.
                /// </summary>
                public void SyncLegacyRaw()
                {
                    lg_fEK = FootR_X; lg_fEL = FootR_Y; lg_fEM = FootR_Z; lg_fEN = FootL_X; lg_fEO = FootL_Y; lg_fEP = FootL_Z;
                    lg_fBA_fCX = AnkleSwayL; lg_fBF_fCY = AnkleSwayR;
                    lg_fBI = -HipSpreadR; lg_fBK = -HipSpreadL;
                    lg_fDO_fDM = ArmUpR; lg_fDP_fDM = ArmUpL; lg_fDS_fDQ = ArmWingR; lg_fDT_fDQ = ArmWingL;
                    lg_fES = AnkleTiltR; lg_fET = AnkleTiltL; lg_fFD = HipTilt; lg_fBT = HipPanR; lg_fBU = HipPanL; lg_fIC = Waist; lg_neck = Neck;
                }
            }
            #endregion

            #region Kernel — 레거시 MakeWalkingMotion(mode, step) 수식 (정수 step 에서 비트 동일)
            private readonly List<float> m_tiltAll = new List<float>();
            private readonly List<float>[] m_tiltMode = new List<float>[3] { new List<float>(), new List<float>(), new List<float>() };

            /// <summary>
            /// 한 프레임 계산. step 은 1 기반(레거시와 동일). 범위 밖이면 Valid=false.
            /// 부작용 없음(Tilt 기록은 GenerateWalking 계열에서만).
            /// </summary>
            public void Evaluate(int nStart_0_Repeat_1_End_2, int nStep, Frame f)
            {
                EvaluateCore(nStart_0_Repeat_1_End_2, nStep, f, false);
            }

            private void EvaluateCore(int nStart_0_Repeat_1_End_2, int nStep, Frame f, bool recordTilt)
            {
                int mode = nStart_0_Repeat_1_End_2;
                Prepare(mode);
                int nSize_Frame = m_frameCount;
                f.Valid = false; f.Mode = mode; f.Step = nStep; f.FrameCount = nSize_Frame;
                if ((nStep < 1) || (nStep > nSize_Frame)) return;
                ModeCache cache = m_cache[mode];
                string[] S = m_raw[mode];

                float fQ4 = 0.5f;
                float fK, fL, fM, fN;
                float fO = cache.fO, fP = cache.fP, fQ = cache.fQ, fR, fS = cache.fS, fT = cache.fT, fU, fV, fW = cache.fW, fX = cache.fX, fY = cache.fY, fZ = cache.fZ;
                float fAA = cache.fAA, fAB = cache.fAB, fAC, fAD, fAE, fAF, fAG, fAH, fAI, fAJ, fAK, fAL, fAM, fAN, fAO, fAP, fAQ, fAR, fAS;
                float fAT = cache.fAT, fAU = cache.fAU, fAV, fAW, fAX;
                float fF25 = 1.0f, fD25 = 1.0f, fAY = fF25, fAZ = fD25;
                float fBA, fBB, fBC, fF26 = 1.0f, fD26 = 1.0f, fBD = fF26, fBE = fD26, fBF;
                float fBG = cache.fBG, fD23 = -1, fBH = fD23, fD24 = -1, fBJ = fD24;
                float fD50 = cache.fD50, fD51 = fD50, fBR = fD50, fBS = fD51, fBW = cache.fBW;
                float fI19 = cache.fI19, fI20 = cache.fI20;
                if (mode != 1) { fI20 = fI19; fI19 = -fI20; }
                float fCB = cache.fCB, fCC = fI19, fCB_Walk = cache.fCB_Walk, fCB_Start_End = cache.fCB_Start_End, fCB_Result;
                float fCD = fI20, fCE = fCB + fCC, fCF = fCB + fCD;
                float fCG = cache.fCG, fCH = fCG, fCI = fCG, fCN = cache.fCN, fCO = cache.fCO, fE17 = cache.fE17, fE18 = cache.fE18;
                if (mode != 1) { fE17 *= -1.0f; fE18 *= -1.0f; }
                float fCX = fE17, fCY = fE18, fCZ = cache.fCZ;
                float fDA = -fCG, fDB = fCG;
                if (mode != 1) { fDA = fDB = 0.0f; }
                float fDK = cache.fDK, fDL = -fDK, fDQ = cache.fDQ, fDR = cache.fDR;
                float fD13 = 1, fE13 = 1, fF13 = 1, fD14 = 1, fE14 = 1, fF14 = 1;
                float fDX = cache.fDX, fDY = fD13, fDZ = fE13, fEA = fF13, fEE = fD14, fEF = fE14, fEG = fF14, fEH = fDL;
                float fEQ = cache.fEQ, fER = cache.fER;
                float fBI, fBK, fBL, fBO, fBP, fBQ, fBT, fBU, fBX, fBY, fBZ, fCA, fCJ, fCK, fCL, fCM, fCP, fCQ, fCR, fCS, fCT, fCU, fCV, fCW, fDC, fDD, fDG, fDH;
                float fDM = cache.fDM, fDN = cache.fDN, fDO, fDP, fDS, fDT, fDU = cache.fDU, fDV;
                float fEB = fDK, fEC, fED, fEI, fEJ, fEK, fEL, fEM, fEN, fEO, fEP, fES, fET;
                float fIB = cache.fIB, fIC, fIE = cache.fIE, fIF;

                int i = nStep;
                fK = (i <= nSize_Frame ? 1.0f : 0.0f);
                fL = i;
                fM = i * fK;
                fV = fM - 1.0f;
                fAC = ((float)((int)fV % (int)fZ) + 1.0f) * fK;
                fAH = (((fAC - (fY + fW)) <= fX && fAC > (fY + fW)) ? 1 : 0);
                fAE = (fAC - (fY + fW)) * fAH;
                fAF = (float)Math.Round((fV + 0.001f) / fZ - 0.5f, 0);
                fAG = (float)Math.Pow(-1.0f, fAF) * fK;
                fAI = fAH * fAG;
                fAJ = (fAI > 0 ? 1 : 0);
                fAN = (fAI < 0 ? 1 : 0);
                fAP = (fAN == 0 ? 0 : fAE);
                fAL = (fAJ == 0 ? 0 : fAE);
                fAS = (fAG < 0 ? 1.0f : 0.0f);
                fAR = (fAG > 0 ? 1.0f : 0.0f);
                fAK = (fAR == 0 ? 0 : fAC);
                fAO = (fAS == 0 ? 0 : fAC);
                fN = (fO == 1 ? (fM >= fZ ? fZ : fAK - fAL) + (fM >= fZ * 2.0f ? fZ : fAO - fAP) : fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ);
                fR = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fP : fQ) * fAR + ((fM > fZ && fAA != 0) ? fP : fQ) * fAS : fP * fK);
                fU = (float)(fR * (float)Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI)) + fT;

                fAD = fAF * fZ + fAG * fAC + fAF;
                fAM = (float)Math.Round((fAL > fAA ? (fAL - fAA) / (fX - fAA) * fX : 0), 3);
                fAQ = (float)Math.Round((fAP > fAB ? (fAP - fAB) / (fX - fAB) * fX : 0.0f), 3);
                fAV = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fAT : fAU) * fAR + ((fM > fZ && fAA != 0) ? fAT : fAU) * fAS : fAT * fK);
                fAW = fAV * (float)Math.Sin(((fO == 0 ? fAK : fN) / fZ * 180.0f) / 180.0f * Math.PI);
                fAX = fAV * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * Math.PI);
                fBA = (fAY == 1 ? fAW : fAX) * fAZ;
                fBB = fAV * (float)Math.Sin((fAO / fZ * 180.0f) / 180.0f * Math.PI);
                fBC = fAV * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * Math.PI);
                fBF = (fBD == 1 ? fBB : fBC) * fBE;

                // 같은 인자의 사인은 한 번만 계산 (동일 비트)
                float sinAM = (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                float sinAQ = (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                float sinMZ = (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI);

                fBI = fBG * sinAM * fBH;
                fBK = fBG * sinAQ * fBJ;
                fBL = (fAD - (fY + fW)) * fK;
                fBO = (fAR + fAJ + fAS + fAN - 1.0f) * fAG;
                fBP = (fBL < 0 ? 0 : (fBL > fX ? fX : fBL));
                fBQ = (fBO < 0 ? fBP + 1.0f : fBP);
                fBT = (fBR * fBQ / fX);
                fBU = (fBS * fBQ / fX);
                fBX = fAJ * fBW * fK;
                fBY = (float)Math.Round(fBX * sinAM, 3);
                fBZ = fAN * fBW * fK;
                fCA = (float)Math.Round(fBZ * sinAQ, 3);
                fCJ = (fM > (fZ + fW) ? fCG * 2.0f : fCG / fX * fAL * 2.0f);
                fCK = (fM > (fZ + fW + fY + fX) ? fCG * 2.0f : fCG / fX * fAP * 2.0f);
                fCL = fCH * (fM / (fZ * 2.0f) * 2.0f);
                fCM = fCI * (fM / (fZ * 2.0f) * 2.0f);
                fCP = fCN * sinAM;
                fCQ = fCO * sinAM;
                fCR = fAM / fZ * 2.0f;
                fCS = (fCR * fCP - (2.0f - fCR) * fCQ);
                fCT = fCN * sinAQ;
                fCU = fCO * sinAQ;
                fCV = fAQ / fZ * 2.0f;
                fCW = (fCV * fCT - (2.0f - fCV) * fCU);

                if (mode != 1)
                {
                    fCZ = cache.cz_m;
                    float fCZ_1 = cache.cz_1;
                    fCZ = (fCZ + (fCZ_1 - fCZ) / fZ * fAD) * fK;
                }
                fDC = (fCJ - fCL) + fCS;
                fDD = fDC * ((mode != 1) ? 1.0f : 2.0f) + fDA;
                fDG = (fCK - fCM) + fCW;
                fDH = fDG * ((mode != 1) ? 1.0f : 2.0f) + fDB;

                if (mode != 1)
                {
                    fDN = cache.dn_m;
                    float fDN_1 = cache.dn_1;
                    fDN = (fDN + (fDN_1 - fDN) / fZ * fAD) * fK;
                }
                fDO = fDN * (float)Math.Sin((fM / (fZ * 2.0f) * 180.0f) / 180.0f * (float)Math.PI) - fDN / 2.0f;
                fDP = -fDO;
                fDS = (float)Math.Abs(fDR * sinMZ) + fDQ;
                fDT = fDS;
                fDV = -(float)Math.Abs(fDU * sinMZ) * (fDU >= 0 ? 1.0f : -1.0f);

                if (mode != 1)
                {
                    fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;
                    fCE = fCB_Result + fCC;
                    fCF = fCB_Result + fCD;
                }
                fEC = fBY + fCE + fDV;
                fED = (float)Math.Round(fDD, 3);
                fEI = fCA + fCF + fDV;
                fEJ = (float)Math.Round(fDH, 3);

                if (mode != 1)
                {
                    float fDK_0 = cache.dk_0, fDK_1 = cache.dk_1;
                    fDK = (fDK_1 + (fDK_0 - fDK_1) / fX * fBQ) * fK;
                    fDL = -fDK; fEB = fDK; fEH = fDL;
                    float fDM_0 = cache.dm_0, fDM_1 = cache.dm_1;
                    fDM = (fDM_1 + (fDM_0 - fDM_1) / fZ * fAD) * fK;
                    float fDQ_0 = cache.dq_0, fDQ_1 = cache.dq_1;
                    fDQ = (fDQ_1 + (fDQ_0 - fDQ_1) / fZ * fAD) * fK;
                    float fEQ_0 = cache.eq_0, fEQ_1 = cache.eq_1;
                    fEQ = (fEQ_1 + (fEQ_0 - fEQ_1) / fZ * fAD) * fK;
                }

                fEK = fU + (fEB + (float)Math.Sin(fDX) * fED) * fDY;
                fEL = fEC * fDZ;
                fEM = ((float)Math.Cos(fDX) * fED) * fEA - fCZ;
                fEN = fU + (fEH + (float)Math.Sin(fDX) * fEJ) * fEE;
                fEO = fEI * fEF;
                fEP = ((float)Math.Cos(fDX) * fEJ) * fEG - fCZ;

                fES = (fAL > 0 ? fER * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                fET = (fAP > 0 ? fER * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;

                if (mode == 1) fIC = fIB * (float)Math.Sin(((fM / fZ * 180.0f) + 90.0f) / 180.0f * Math.PI);
                else fIC = fIB * (float)Math.Sin((-fM / (fZ * 2.0f) * 180.0f) / 180.0f * Math.PI);
                if (mode == 1) fIF = fIE * (float)Math.Sin(((fM / fZ * 180.0f) + 90.0f) / 180.0f * Math.PI);
                else fIF = fIE * (float)Math.Sin((-fM / (fZ * 2.0f) * 180.0f) / 180.0f * Math.PI);

                // 고관절 숙이기 (레거시: Tilt ID(Left) 칸의 유무가 보간 계산 여부를 정한다)
                float fFD = 0;
                if (mode != 1)
                {
                    if (cache.hasHipTiltL)
                    {
                        float fFD_0 = cache.fd_0, fFD_1 = cache.fd_1;
                        fFD = (fFD_1 + (fFD_0 - fFD_1) / fZ * fAD) * fK;
                    }
                    if (recordTilt)
                    {
                        if (mode == 0) { if (fM <= fZ) { m_tiltAll.Add(fFD); m_tiltMode[0].Add(fFD); } }
                        else { if (fM > fZ) { m_tiltAll.Add(fFD); m_tiltMode[2].Add(fFD); } }
                    }
                }
                else
                {
                    if (cache.hasHipTiltL) fFD = cache.fd_repeat;
                    if (recordTilt) { m_tiltAll.Add(fFD); m_tiltMode[1].Add(fFD); }
                }

                // ---- 출력 채우기
                f.Valid = true;
                f.FootR_X = fEK; f.FootR_Y = fEL; f.FootR_Z = fEM;
                f.FootL_X = fEN; f.FootL_Y = fEO; f.FootL_Z = fEP;
                f.HasFootR = !string.IsNullOrEmpty(S[9]);
                f.HasFootL = !string.IsNullOrEmpty(S[10]);
                f.Sway = fU;
                f.Gate = (int)fAF;
                f.GatePos = fAC;
                f.SwingLeg = (fAJ > 0) ? 1 : ((fAN > 0) ? 2 : 0);
                f.SwingProgress = (fAJ > 0) ? ((fX != 0) ? fAM / fX : 0f) : ((fAN > 0) ? ((fX != 0) ? fAQ / fX : 0f) : 0f);
                // 의미 채널: 방향 상수(fBH/fBJ=-1)를 제거한 값 = 파라미터 부호 그대로
                f.HipSpreadR = fBG * sinAM;
                f.HipSpreadL = fBG * sinAQ;
                f.AnkleSwayL = fBA + fCX;
                f.AnkleSwayR = fBF + fCY;
                f.AnkleTiltR = fES; f.AnkleTiltL = fET;
                f.HipTilt = fFD;
                f.HipPanR = fBT; f.HipPanL = fBU;
                f.ArmUpR = fDO + fDM; f.ArmUpL = fDP + fDM;
                f.ArmWingR = fDS + fDQ; f.ArmWingL = fDT + fDQ;
                f.Waist = fIC;
                f.Neck = fIF;
                f.SpeedMs = (mode == 0 && nStep == 1) ? cache.firstMs : cache.speedMs;
                f.DelayMs = cache.delayMs;
                // 레거시 원값
                f.lg_fEK = fEK; f.lg_fEL = fEL; f.lg_fEM = fEM; f.lg_fEN = fEN; f.lg_fEO = fEO; f.lg_fEP = fEP;
                f.lg_fBA_fCX = fBA + fCX; f.lg_fBF_fCY = fBF + fCY; f.lg_fBI = fBI; f.lg_fBK = fBK;
                f.lg_fDO_fDM = fDO + fDM; f.lg_fDP_fDM = fDP + fDM; f.lg_fDS_fDQ = fDS + fDQ; f.lg_fDT_fDQ = fDT + fDQ;
                f.lg_fES = fES; f.lg_fET = fET; f.lg_fFD = fFD; f.lg_fBT = fBT; f.lg_fBU = fBU; f.lg_fIC = fIC;
                f.lg_neck = LegacyQuirks ? fIE : fIF;
            }
            #endregion

            #region LegacyText — 레거시 문자열(바이트 동일) 및 호환 API
            /// <summary>Frame → 레거시 탭 구분 문자열. 레거시 MakeWalkingMotion(mode, step) 와 동일한 바이트열.</summary>
            public string ToLegacyString(Frame f)
            {
                if (!f.Valid) return String.Empty;
                int mode = f.Mode; int i = f.Step; int nSize_Frame = f.FrameCount;
                string[] S = m_raw[mode];
                if ((m_walkingCount <= 0) && (mode == 1)) return String.Empty;
                StringBuilder sb = new StringBuilder(4096);
                sb.Append("E\t1\t");
                if (S[9] != "") sb.AppendFormat("I{0}\t{1}\t{2}\t{3}\t", S[9], f.lg_fEK, f.lg_fEL, f.lg_fEM);
                if (S[10] != "") sb.AppendFormat("I{0}\t{1}\t{2}\t{3}\t", S[10], f.lg_fEN, f.lg_fEO, f.lg_fEP);
                if ((i == 1) && (mode == 0)) sb.AppendFormat("S\t{0}\tD\t{1}\t", S[0], S[25]);
                else sb.AppendFormat("S\t{0}\tD\t{1}\t", S[24], S[25]);
                if (S[22] != "") sb.AppendFormat("P{0}\t{1}\t", S[22], f.lg_fBA_fCX);
                if (S[21] != "") sb.AppendFormat("P{0}\t{1}\t", S[21], f.lg_fBF_fCY);
                if (S[19] != "") sb.AppendFormat("P{0}\t{1}\t", S[19], f.lg_fBI);
                if (S[20] != "") sb.AppendFormat("P{0}\t{1}\t", S[20], f.lg_fBK);
                if (S[27] != "") sb.AppendFormat("T{0}\t{1}\t", S[27], f.lg_fDO_fDM);
                if (S[28] != "") sb.AppendFormat("T{0}\t{1}\t", S[28], f.lg_fDP_fDM);
                if (S[31] != "") sb.AppendFormat("T{0}\t{1}\t", S[31], f.lg_fDS_fDQ);
                if (S[32] != "") sb.AppendFormat("T{0}\t{1}\t", S[32], f.lg_fDT_fDQ);
                if (S[42] != "") sb.AppendFormat("P{0}\t{1}\t", S[42], f.lg_fES);
                if (S[43] != "") sb.AppendFormat("P{0}\t{1}\t", S[43], f.lg_fET);
                if (S[39] != "") sb.AppendFormat("P{0}\t{1}\t", S[39], f.lg_fFD);
                if (S[40] != "") sb.AppendFormat("P{0}\t{1}\t", S[40], f.lg_fFD);
                if (S[47] != "") sb.AppendFormat("P{0}\t{1}\t", S[47], f.lg_fBT);
                if (S[48] != "") sb.AppendFormat("P{0}\t{1}\t", S[48], f.lg_fBU);
                if (S[50] != "") sb.AppendFormat("P{0}\t{1}\t", S[50], f.lg_fIC);
                if (S[52] != "") sb.AppendFormat("P{0}\t{1}\t", S[52], f.lg_neck);
                if (mode != 1) sb.Append("X\t-1\t");
                sb.AppendFormat("G\t{0}\t", mode + 1);
                if (m_mirror) sb.Append("X\t-1\t");
                if ((mode == 1) && (m_walkingCount > 1) && (i == 1))
                    sb.AppendFormat("@SET_COMMAND,1\t@SET_DATA0,{0}\t@SET_DATA1,{1}\t", nSize_Frame - 1, m_walkingCount);
                if (S[54] != "") sb.AppendFormat("{0}\t", S[54]);       // 레거시: 55번째 칸(null)도 "" 과 다르므로 탭 하나가 붙는다
                if (S[49] != null) sb.Append(Ojw.CConvert.ChangeChar(S[49], ' ', '\t'));
                return sb.ToString();
            }

            private readonly Frame m_tmpFrame = new Frame();
            /// <summary>레거시 호환: 문자열 한 프레임.</summary>
            public string MakeWalkingMotion(int mode, int step)
            {
                EvaluateCore(mode, step, m_tmpFrame, false);
                return ToLegacyString(m_tmpFrame);
            }
            /// <summary>레거시 호환: 한 모드 전체(2·게이트 프레임). Tilt 기록 포함.</summary>
            public List<string> MakeWalkingMotion(int mode)
            {
                List<string> l = new List<string>();
                int n = FrameCount;
                for (int i = 1; i <= n; i++) { EvaluateCore(mode, i, m_tmpFrame, true); l.Add(ToLegacyString(m_tmpFrame)); }
                return l;
            }
            /// <summary>레거시 호환: 시작+반복+종료 문자열 목록. Tilt 목록 초기화 후 기록.</summary>
            public List<string> GenerateWalking(int nWalkingCount)
            {
                m_tiltAll.Clear(); m_tiltMode[0].Clear(); m_tiltMode[1].Clear(); m_tiltMode[2].Clear();
                List<string> l = new List<string>();
                l.AddRange(MakeWalkingMotion(0));
                if (nWalkingCount > 0) l.AddRange(MakeWalkingMotion(1));
                l.AddRange(MakeWalkingMotion(2));
                return l;
            }
            public float GetTilt(int step) { return m_tiltAll[step]; }
            public float GetTilt(int mode, int step) { return m_tiltMode[mode][step]; }
            public int TiltCount { get { return m_tiltAll.Count; } }

            /// <summary>
            /// 골든 검증: 같은 파라미터로 레거시 Ojw.COjwWalking 과 이 클래스의 문자열을 모든 모드·프레임에서 비교.
            /// 반환: 불일치 건수. report 에 요약(첫 불일치 프레임 포함).
            /// </summary>
            public int GoldenCompare(out string report)
            {
                Ojw.COjwWalking legacy = new Ojw.COjwWalking();
                legacy.SetData_Str(1, ToStrings(1)); legacy.SetData_Str(0, ToStrings(0)); legacy.SetData_Str(2, ToStrings(2));
                legacy.SetWalkingCount(m_walkingCount); legacy.SetMirror(m_mirror);
                // 주의: 레거시는 SetData_Str 안에서 모드1 교차대입을 하므로 이 클래스도 같은 순서로 다시 적용한다.
                string[] s0 = ToStrings(0), s1 = ToStrings(1), s2 = ToStrings(2);
                SetData_Str(1, s1); SetData_Str(0, s0); SetData_Str(2, s2);
                int n = FrameCount;
                int mismatch = 0, total = 0;
                StringBuilder sb = new StringBuilder();
                string first = null;
                for (int mode = 0; mode < 3; mode++)
                {
                    for (int i = 1; i <= n; i++)
                    {
                        string a = legacy.MakeWalkingMotion(mode, i);
                        string b = MakeWalkingMotion(mode, i);
                        total++;
                        if (a != b)
                        {
                            mismatch++;
                            if (first == null) first = String.Format("mode={0} step={1}\r\n legacy: {2}\r\n new   : {3}", mode, i, a.Replace("\t", "|"), b.Replace("\t", "|"));
                        }
                    }
                }
                sb.AppendFormat("golden: {0}/{1} frames identical (frameCount={2}, quirks={3})", total - mismatch, total, n, LegacyQuirks);
                if (first != null) sb.Append("\r\n first mismatch: ").Append(first);
                report = sb.ToString();
                return mismatch;
            }
            #endregion

            #region Streamer — 연속 시간 위상, 시작/반복/종료 상태기계, 잔차 훅
            public enum Phase { Idle = 0, Start = 1, Repeat = 2, End = 3 }

            /// <summary>
            /// 실시간 스트리머. Advance(dt) 마다 현재 위상의 프레임(이웃 두 정수 스텝의 선형 보간)을 낸다.
            /// 위상 단위 = 레거시 step. 시간 → step 변환은 속도시간(ms/step) × PhaseScale.
            /// 정지 요청은 반복 사이클 경계에서만 종료 보행으로 넘어간다(자세 점프 방지).
            /// </summary>
            public sealed class Streamer
            {
                private readonly COjwWalking_c m_engine;
                private readonly Frame m_a = new Frame(), m_b = new Frame();
                public readonly Frame Current = new Frame();
                private Phase m_phase = Phase.Idle;
                private float m_pos = 1f;           // 현재 step 위치(실수). Start: 1..gate, Repeat: 1..2gate 순환, End: gate+1..2gate
                private bool m_stopRequest = false;
                private int m_cycles = 0;
                public float PhaseScale = 1f;       // 0.25~2.5 권장. 외란 시 위상 가속/감속
                public bool Hold = false;           // true 면 위상 정지(착지 대기)
                public float TimeSec { get; private set; }
                public int CyclesCompleted { get { return m_cycles; } }
                public Phase State { get { return m_phase; } }
                public float StepPosition { get { return m_pos; } }
                /// <summary>반복 모드 기준 정규화 위상 0..1 (1 사이클 = 2 게이트).</summary>
                public float CyclePhase { get { int n = m_engine.FrameCount; return (n > 0) ? ((m_pos - 1f) / n) : 0f; } }

                // 잔차 훅(단위: mm / deg). 값은 프레임 채널에 더해진다.
                public readonly float[] FootResidualR = new float[3], FootResidualL = new float[3];   // 엔진축 X,Y,Z
                public float AnkleTiltResidualR, AnkleTiltResidualL, AnkleRollResidualR, AnkleRollResidualL, TorsoLeanResidual;

                public Streamer(COjwWalking_c engine) { m_engine = engine; }

                public void Reset() { m_phase = Phase.Idle; m_pos = 1f; m_stopRequest = false; m_cycles = 0; TimeSec = 0; Current.Valid = false; }
                public void Start() { Reset(); m_phase = Phase.Start; }
                /// <summary>시작 보행 없이 반복 사이클로 바로 진입(AMP 루프 재생용).</summary>
                public void StartRepeat() { Reset(); m_phase = Phase.Repeat; }
                public void RequestStop() { m_stopRequest = true; }

                /// <summary>dt 초만큼 진행하고 Current 를 갱신. 반환: Current.Valid.</summary>
                public bool Advance(float dtSec)
                {
                    if (m_phase == Phase.Idle) { Current.Valid = false; return false; }
                    int gate = m_engine.GateSize;
                    int n = m_engine.FrameCount;
                    if (gate <= 0 || n <= 0) { Current.Valid = false; return false; }
                    float ms = m_engine.FrameMs(ModeOf(m_phase), 2);
                    if (ms <= 0) ms = 50f;
                    float scale = PhaseScale; if (scale < 0.05f) scale = 0.05f;
                    float dstep = Hold ? 0f : (dtSec * 1000f / ms) * scale;
                    TimeSec += dtSec;
                    m_pos += dstep;
                    // 상태 전이
                    if (m_phase == Phase.Start && m_pos >= gate + 1) { m_pos -= gate; m_phase = Phase.Repeat; }
                    if (m_phase == Phase.Repeat)
                    {
                        while (m_pos >= n + 1)
                        {
                            m_pos -= n; m_cycles++;
                            if (m_stopRequest) { m_phase = Phase.End; m_pos = gate + (m_pos - 1f); break; }
                        }
                    }
                    if (m_phase == Phase.End && m_pos >= n + 1) { m_phase = Phase.Idle; Current.Valid = false; return false; }
                    // 보간 프레임
                    int mode = ModeOf(m_phase);
                    int k0 = (int)Math.Floor(m_pos);
                    float t = m_pos - k0;
                    int k1 = k0 + 1;
                    if (m_phase == Phase.Repeat) { if (k1 > n) k1 = 1; }
                    else if (k1 > n) k1 = n;
                    m_engine.Evaluate(mode, k0, m_a);
                    m_engine.Evaluate(mode, k1, m_b);
                    if (!m_a.Valid) { Current.Valid = false; return false; }
                    if (!m_b.Valid) Current.CopyFrom(m_a); else Frame.Lerp(m_a, m_b, t, Current);
                    ApplyResiduals(Current);
                    return true;
                }
                private static int ModeOf(Phase p) { return (p == Phase.Start) ? 0 : ((p == Phase.End) ? 2 : 1); }
                private void ApplyResiduals(Frame f)
                {
                    f.FootR_X += FootResidualR[0]; f.FootR_Y += FootResidualR[1]; f.FootR_Z += FootResidualR[2];
                    f.FootL_X += FootResidualL[0]; f.FootL_Y += FootResidualL[1]; f.FootL_Z += FootResidualL[2];
                    f.AnkleTiltR += AnkleTiltResidualR; f.AnkleTiltL += AnkleTiltResidualL;
                    f.AnkleSwayR += AnkleRollResidualR; f.AnkleSwayL += AnkleRollResidualL;
                    f.HipTilt += TorsoLeanResidual;
                }
            }
            #endregion
        }
    }
}
