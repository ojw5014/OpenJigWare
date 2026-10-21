// ================================================================
// 로봇팔 모터 세팅 + 실물 연결 하나  2026-10-02
//
// MakeUrdf "로봇팔 세팅" 탭의 본체 — 앱은 표·버튼·확인창만 가지고, 계산·검증·파일·버스는 전부 여기서 한다.
//   CArmMotor_t     모터 하나의 세팅 (방향·영점·틱/회전·각/회전·리밋·미러 …). 필드 이름과 뜻은 디자이너의 SMotorInfo_t 와 같다
//   CArmMotorSet_t  로봇 한 대의 세팅 = 디자이너 파일 .ojw 의 모터 정보 (실물 방향·영점의 정본 — 저장은 .ojw 만, 위치는 사용자가 고른다.
//                   예전 <urdf>\armsetup\motors_<축서명>.txt 는 읽기만 한다)
//   CArmConn_t      실물 연결 하나 (CScene_t 한 개) — 탭의 읽기·조그·영점과 측정·시험 작업이 소유자 잠금으로 빌려 쓴다
//
// 3D 각 ↔ 서보 틱 변환은 CScene_t 한 곳에서만 한다 (COjw_42_Sim2Real.cs):
//   쓰기: 3D 각 → (처짐 보정) → 리밋 → 틱 = 영점 + 방향 × 각 × 틱/회전 ÷ 각/회전
//   읽기: 틱 → 3D 각.  식은 디자이너(C3d.CalcAngle2Evd / CalcEvd2Angle)와 같고 반올림 순서는 CProtocol 과 같다.
// CProtocol 은 전송층으로만 쓴다 (X 프리셋, 방향 정, 리밋 0). 이 경로에서는 CProtocol 의 방향·리밋·모터 정보 배열 설정을
//   부르지 않는다 — 방향이 두 번 뒤집히고, 중심·메카 값이 프리셋으로 돌아가며, 각도 리밋이 속도 raw 에도 걸리기 때문이다.
// C3d 헤더(m_CHeader.pSMotorInfo)에는 세팅을 투영만 한다 — 3D 드래그·IK 리밋이 같은 값을 보게. 리밋 = 수동 ∩ EDH $jointrange
//   (둘이 겹치지 않는 축이 있으면 그 세트는 적용하지 않는다 — 어느 쪽 값을 골라도 다른 쪽 경계를 넘을 수 있다).
//   연결 중에는 DH 를 다시 해석해도(MakeAll·URDF 내보내기의 ApplyEdhMotorLimits) 연결된 축의 헤더 리밋은 바뀌지 않는다 (C3d 헤더 리밋
//   붙잡기 훅) — 새 $jointrange 는 ReapplyAfterRebuild 가 사용 중이 아닐 때 실물 장면과 함께 한 번에 반영한다.
// EEPROM 은 쓰지 않는다. Drive Mode·Homing Offset·위치 한계는 읽어서 보여 주기만 한다.
//
// 실물 안전
//   · 연결해도 토크는 켜지 않는다 (연결 = 읽기만). 서보가 이미 토크 ON 이면 끄지도 다시 켜지도 않고 그 상태를 이어받는다.
//   · 세팅(방향·영점·틱/회전)을 바꾸는 순간 실물은 움직이지 않는다 — 3D 를 새 해석으로 옮긴다 (토크 ON 이면 실물에 걸린 목표 틱 그대로).
//   · 토크 켜기는 읽기 실패·리밋 밖을 이유로 거부하지 않는다 (경고만 — 예전 torqon 그대로 켠다).
//     대신 움직이는 동작(조그·측정·시험)은 EnsureSynced 를 통과해야 시작한다: 3D 와 실물이 맞는지 다시 읽어 확인하고,
//     리밋 밖 축이 있으면 막는다 (보낼 때 리밋으로 잘리면 그 축이 한 틱에 튄다).
//   · 토크 ON 인데 일부 서보가 꺼져 있으면(전원이 잠깐 끊김·오류) 토크 켜기는 그 축만 켠다 (켠 축 = 지금 위치, 켜져 있던 축 = 붙잡은 자세 그대로).
//   · 서보가 이미 붙잡은 목표를 이어받을 때(연결 때·토크 켤 때·일부 축 다시 켤 때·움직이기 전 확인·작업 시작) 3D 자세 = 서보 목표 그대로다.
//     목표에는 지난 처짐 보정이 들어 있을 수 있는데, 바닥 보정은 엔코더로 보이지 않는 휨을 메우므로 실물 지금 위치로는 그것을 가릴 수 없다
//     (되짚기는 해가 여럿일 때 엉뚱한 해를 골라 3D 가 실물 펜보다 15~25mm 위였다 — 10-03 검토). 그래서 처짐 보정이 걸려 있었으면(모델 켬·걸린 보정·
//     페이드·손목 유지) 되짚지 않고 보정을 페이드 없이 끄고(C3d.SagComp_OffForAdopt) 알린다 → 다음 전송 틱 = 서보 목표 (튐 0), 그 뒤에도 보정이
//     얹히지 않는다. 다시 켜면 다음 동작의 출력 틱 1초에 걸쳐 들어온다. 보정이 걸려 있지 않았으면 예전 그대로 (목표 그대로, 바꿀 것 없음).
//     앱이 포트를 다른 프로그램에 넘겼다가 다시 연결한 경우 (10-15 검토) 넘기기 전에 켜져 있던 보정은 RestoreSagComp 가 보통 켜기 길로 되살린다
//     (이어받은 자세가 정해진 뒤 — 다음 동작의 출력 틱 1초에 걸쳐 0 에서 들어온다).
//   · 서보 목표(Goal Position)를 읽지 못한 채 토크 ON 인 축을 맞춰야 하면 움직이는 동작을 막는다 (처진 지금 위치로 맞춰 보내면 하중만큼 떨어진다).
// ================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenJigWare
{
    partial class Ojw
    {
        // ────────────────────────────────────────────────────────────
        // 모터 하나
        // ────────────────────────────────────────────────────────────
        /// <summary>로봇팔 모터 하나의 세팅. 필드 이름과 뜻은 디자이너의 SMotorInfo_t 와 같고, 3D 각 ↔ 틱 변환식도 같다.
        /// 리밋은 3D 각(도)이고 NaN = 없음 (헤더의 "0 = 없음" 규약과 달리 0° 경계를 그대로 쓸 수 있다)</summary>
        public class CArmMotor_t
        {
            public int nAxis;                                       // EDH 축 번호 = 3D 인덱스 (표의 키)
            public int nMotorID;                                    // 버스 ID — 지금은 축 번호와 같아야 한다
            public int nMotorDir = 0;                               // 0 정 / 1 역 (정 = 3D + 방향이 틱 증가)
            public int nCenter_Evd = 2048;                          // 3D 0° 일 때의 서보 틱 (영점)
            public int nMechMove = 4096;                            // 한 바퀴 틱
            public float fMechAngle = 360f;                         // 한 바퀴 각
            public float fLimit_Down = float.NaN;                   // 수동 하한 (3D 각, NaN = 없음)
            public float fLimit_Up = float.NaN;                     // 수동 상한
            public float fInitAngle = 0f, fInitAngle2 = 0f;         // 저장·헤더 반영만
            public int nAxis_Mirror = -1;                           // 짝 축 / -1 없음 / -2 자기 반전 (헤더 기본값 0 = "축 0 과 짝" 함정을 피해 -1)
            public int nGroupNumber = 0;
            public int nMotorControlType = 0;                       // CScene_t 는 0(위치)만
            public int nProtocolVersion = 2;                        // CScene_t 는 2만
            public int nHwMotor_Key = 0;                            // 모델번호 (addr 0) — 0 = 미지정
            public string strNickName = "";                         // 쉼표·줄바꿈·// 금지
            public bool bChecked = false;                           // 방향 확인함 (사람 눈으로)
            public string strSource = "기본";                       // 기본 / 파일 / 자동 / 디자이너 / 수정 (저장하지 않는다)
            public float fEffLo = float.NaN, fEffHi = float.NaN;    // 실행 중만 — 수동 ∩ EDH $jointrange (CArmMotorSet_t.ComputeEffective). 겹치지 않으면 하한 > 상한 (적용 거부)

            /// <summary>방향 부호 — 0(정) = +1, 1(역) = -1</summary>
            public int DirSign { get { return (nMotorDir == 0) ? 1 : -1; } }

            /// <summary>3D 각(도) → 서보 틱. 식은 C3d.CalcAngle2Evd 와 같고, 영점을 더한 뒤 반올림하는 순서는 CProtocol.CalcAngle2Evd 와 같다
            /// (기본값이면 CProtocol 기본 변환과 같은 틱 — 예전 경로와 같은 명령이 나간다)</summary>
            public int SimToTick(float fSim)
            {
                return (int)Math.Round(((float)nMechMove * (fSim * DirSign)) / fMechAngle + nCenter_Evd);
            }
            /// <summary>서보 틱 → 3D 각(도). C3d.CalcEvd2Angle 과 같은 식 (기본값이면 CSim2Real.Deg 와 비트까지 같다)</summary>
            public float TickToSim(int nTick)
            {
                return ((fMechAngle * (float)(nTick - nCenter_Evd)) / (float)nMechMove) * DirSign;
            }
            /// <summary>쓰이는 하한 — ComputeEffective 가 채운 값(수동 ∩ EDH), 아직 없으면 수동 값. NaN = 없음</summary>
            public float LimitLo { get { return float.IsNaN(fEffLo) ? fLimit_Down : fEffLo; } }
            /// <summary>쓰이는 상한 — LimitLo 와 같은 규칙</summary>
            public float LimitHi { get { return float.IsNaN(fEffHi) ? fLimit_Up : fEffHi; } }
            /// <summary>쓰이는 리밋으로 자른다 (NaN 경계는 건너뜀)</summary>
            public float Clamp(float fSim)
            {
                float fLo = LimitLo, fHi = LimitHi;
                if (!float.IsNaN(fLo) && fSim < fLo) fSim = fLo;
                if (!float.IsNaN(fHi) && fSim > fHi) fSim = fHi;
                return fSim;
            }
            /// <summary>쓰이는 리밋 밖으로 fTolDeg 넘게 나갔나</summary>
            public bool IsOutside(float fSim, float fTolDeg)
            {
                float fLo = LimitLo, fHi = LimitHi;
                if (!float.IsNaN(fLo) && fSim < fLo - fTolDeg) return true;
                if (!float.IsNaN(fHi) && fSim > fHi + fTolDeg) return true;
                return false;
            }
            /// <summary>틱 해석이 같나 — 방향·영점·틱/회전·각/회전·실물 ID (리밋·이름 등은 보지 않는다)</summary>
            public bool SameMapping(CArmMotor_t o)
            {
                if (o == null) return false;
                return nMotorDir == o.nMotorDir && nCenter_Evd == o.nCenter_Evd && nMechMove == o.nMechMove
                    && fMechAngle == o.fMechAngle && nMotorID == o.nMotorID;
            }
            /// <summary>복사본 (필드가 값·문자열뿐이라 얕은 복사로 충분하다)</summary>
            public CArmMotor_t Clone() { return (CArmMotor_t)MemberwiseClone(); }
            /// <summary>기본 세팅 — 방향 정, 영점 2048, 4096/360, 리밋 없음, 미러 없음, 기종 미지정, 확인 안 됨</summary>
            public static CArmMotor_t CreateDefault(int nAxis)
            {
                CArmMotor_t m = new CArmMotor_t();
                m.nAxis = nAxis;
                m.nMotorID = nAxis;
                return m;
            }
            /// <summary>"ID 13 리밋 밖(105.0°, 상한 97°)" 문구 — 어느 경계를 넘었는지</summary>
            internal string OutsideText(float fSim)
            {
                float fHi = LimitHi;
                string strLim = (!float.IsNaN(fHi) && fSim > fHi) ? "상한 " + CArmMotorSet_t.Fmt(fHi) + "°" : "하한 " + CArmMotorSet_t.Fmt(LimitLo) + "°";
                return string.Format(CultureInfo.InvariantCulture, "ID {0} 리밋 밖({1:0.0}°, {2})", nAxis, fSim, strLim);
            }
        }

        // ────────────────────────────────────────────────────────────
        // 로봇 한 대의 세팅 + 파일
        // ────────────────────────────────────────────────────────────
        /// <summary>로봇 한 대의 모터 세팅 — 저장은 디자이너 파일(.ojw, SaveDesignerFile), 읽기는 .ojw(ImportDesignerFile)와 예전 motors_&lt;축서명&gt;.txt(Load).
        /// 편집은 SetField (검증·미러 짝 맞춤), 적용은 CArmConn_t.ApplyMotors (검증 → 리밋 교집합 → CScene_t → C3d 헤더 투영)</summary>
        public class CArmMotorSet_t
        {
            public const int FILE_VERSION = 1;
            public string strRobot = "";                                    // 표시용 (예제 이름)
            public string strSaved = "";                                    // 파일의 saved= (표시용)
            public List<CArmMotor_t> Motors = new List<CArmMotor_t>();      // EDH 등장 순서
            public string LimitConflict = "";                               // ComputeEffective 가 채운다 — 수동 ∩ EDH 가 비는 축 (줄마다 하나, "" = 없음). 있으면 적용 거부

            private static readonly int[] s_anKnownModels = { 1060, 1200, 1190, 1020, 1030, 1070, 1080, 1090, 1160 };

            /// <summary>수동 리밋과 EDH $jointrange 가 겹치지 않는 축이 있나 (ComputeEffective 뒤에 본다) — 있으면 ApplyMotors 가 거부한다</summary>
            public bool HasLimitConflict { get { return !string.IsNullOrEmpty(LimitConflict); } }

            /// <summary>축 번호 (Motors 순서)</summary>
            public int[] Axes
            {
                get
                {
                    int[] an = new int[Motors.Count];
                    for (int i = 0; i < an.Length; i++) an[i] = Motors[i].nAxis;
                    return an;
                }
            }
            /// <summary>축 번호로 찾기 — 없으면 null</summary>
            public CArmMotor_t Get(int nAxis)
            {
                foreach (CArmMotor_t m in Motors) if (m != null && m.nAxis == nAxis) return m;
                return null;
            }
            private int IndexOf(int nAxis)
            {
                for (int i = 0; i < Motors.Count; i++) if (Motors[i] != null && Motors[i].nAxis == nAxis) return i;
                return -1;
            }
            /// <summary>깊은 복사본</summary>
            public CArmMotorSet_t Clone()
            {
                CArmMotorSet_t s = new CArmMotorSet_t();
                s.strRobot = strRobot; s.strSaved = strSaved; s.LimitConflict = LimitConflict;
                foreach (CArmMotor_t m in Motors) if (m != null) s.Motors.Add(m.Clone());
                return s;
            }
            /// <summary>기본 세트 (방향 정·영점 2048·4096/360·리밋 없음) — 축은 주어진 순서, 중복은 한 번만</summary>
            public static CArmMotorSet_t CreateDefault(int[] anAxes, string strRobot)
            {
                CArmMotorSet_t s = new CArmMotorSet_t();
                s.strRobot = (strRobot == null) ? "" : strRobot;
                if (anAxes != null)
                    foreach (int ax in anAxes) if (s.Get(ax) == null) s.Motors.Add(CArmMotor_t.CreateDefault(ax));
                return s;
            }

            // ── 서명·경로 ──
            /// <summary>축 서명 — 정렬 후 연속 구간 압축: {11..16} → "11-16", {1,2,3,5} → "1-3_5" (파일 이름에 쓴다). 비면 "none"</summary>
            public static string SignatureOf(int[] anAxes) { return JoinRuns(anAxes, "-", "_", "none"); }
            /// <summary>사람이 읽는 ID 목록 — {1..17} → "1~17", {1,2,3,5} → "1~3, 5"</summary>
            public static string RangeText(int[] anIds) { return JoinRuns(anIds, "~", ", ", ""); }
            private static string JoinRuns(int[] an, string strDash, string strSep, string strEmpty)
            {
                if (an == null || an.Length == 0) return strEmpty;
                List<int> lst = new List<int>();
                foreach (int v in an) if (!lst.Contains(v)) lst.Add(v);
                lst.Sort();
                StringBuilder sb = new StringBuilder();
                int i = 0;
                while (i < lst.Count)
                {
                    int j = i;
                    while (j + 1 < lst.Count && lst[j + 1] == lst[j] + 1) j++;
                    if (sb.Length > 0) sb.Append(strSep);
                    sb.Append(lst[i].ToString(CultureInfo.InvariantCulture));
                    if (j > i) sb.Append(strDash).Append(lst[j].ToString(CultureInfo.InvariantCulture));
                    i = j + 1;
                }
                return sb.ToString();
            }
            /// <summary>이 세트의 축 서명</summary>
            public string Signature { get { return SignatureOf(Axes); } }
            /// <summary>예전 txt 파일 경로 — &lt;urdf&gt;\armsetup\motors_&lt;서명&gt;.txt (읽기 전용 대체 경로 — 새로 쓰지 않는다)</summary>
            public static string DefaultPath(string strUrdfDir, int[] anAxes)
            {
                return Path.Combine(Path.Combine(strUrdfDir, "armsetup"), "motors_" + SignatureOf(anAxes) + ".txt");
            }
            /// <summary>.ojw 저장 대화의 기본 파일 이름 — motors_&lt;서명&gt;.ojw</summary>
            public static string DefaultOjwName(int[] anAxes) { return "motors_" + SignatureOf(anAxes) + ".ojw"; }

            // ── 최근 .ojw 경로 기록 (<urdf>\armsetup\ojw_paths.txt — 줄마다 서명=경로, last=마지막으로 쓴 경로. UTF-8(BOM)·CRLF) ──
            /// <summary>최근 .ojw 경로 기록 파일 — &lt;urdf&gt;\armsetup\ojw_paths.txt</summary>
            public static string RecentOjwFile(string strUrdfDir)
            {
                return Path.Combine(Path.Combine(strUrdfDir, "armsetup"), "ojw_paths.txt");
            }
            /// <summary>기록에서 키(축 서명 또는 "last")의 경로 — 없거나 못 읽으면 null (파일이 실제로 있는지는 보지 않는다)</summary>
            public static string GetRecentOjw(string strUrdfDir, string strKey)
            {
                if (string.IsNullOrEmpty(strUrdfDir) || string.IsNullOrEmpty(strKey)) return null;
                string v;
                return ReadRecent(RecentOjwFile(strUrdfDir)).TryGetValue(strKey.Trim().ToLowerInvariant(), out v) ? v : null;
            }
            /// <summary>기록에 서명=경로 와 last=경로 를 남긴다 (다른 줄은 그대로). 실패하면 false 와 사유 — 예외는 밖으로 내지 않는다</summary>
            public static bool SetRecentOjw(string strUrdfDir, string strSignature, string strPath, out string strWhy)
            {
                strWhy = "";
                if (string.IsNullOrEmpty(strUrdfDir) || string.IsNullOrEmpty(strSignature) || string.IsNullOrEmpty(strPath)) { strWhy = "경로 없음"; return false; }
                try
                {
                    string strFile = RecentOjwFile(strUrdfDir);
                    Dictionary<string, string> d = ReadRecent(strFile);
                    string strFull = Path.GetFullPath(strPath);
                    d[strSignature.Trim().ToLowerInvariant()] = strFull;
                    d["last"] = strFull;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("// 로봇팔 모터 세팅 — 축 서명별 마지막 디자이너 파일(.ojw). 서명=경로, last = 마지막으로 쓴 파일\r\n");
                    List<string> lstKeys = new List<string>(d.Keys);
                    lstKeys.Sort(StringComparer.Ordinal);
                    foreach (string k in lstKeys) sb.Append(k).Append('=').Append(d[k]).Append("\r\n");
                    string strDir = Path.GetDirectoryName(strFile);
                    if (!string.IsNullOrEmpty(strDir)) Directory.CreateDirectory(strDir);
                    File.WriteAllText(strFile, sb.ToString(), new UTF8Encoding(true));
                    return true;
                }
                catch (Exception ex) { strWhy = "최근 경로 기록 실패: " + ex.Message; return false; }
            }
            private static Dictionary<string, string> ReadRecent(string strFile)
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                try
                {
                    if (!File.Exists(strFile)) return d;
                    foreach (string s0 in File.ReadAllLines(strFile, Encoding.UTF8))
                    {
                        string s = s0.Trim();
                        if (s.Length == 0 || s.StartsWith("//", StringComparison.Ordinal)) continue;   // 경로 안의 // 는 주석이 아니다 — 줄 맨 앞만
                        int nEq = s.IndexOf('=');
                        if (nEq <= 0) continue;
                        string k = s.Substring(0, nEq).Trim().ToLowerInvariant(), v = s.Substring(nEq + 1).Trim();
                        if (v.Length > 0) d[k] = v;
                    }
                }
                catch { }
                return d;
            }
            /// <summary>자동 방향 결과(joint_map.json) 찾기 — &lt;urdf&gt;\armsetup\ → &lt;urdf&gt;\sim2real\ 순서. 없으면 null</summary>
            public static string FindJointMap(string strUrdfDir)
            {
                if (string.IsNullOrEmpty(strUrdfDir)) return null;
                string a = Path.Combine(Path.Combine(strUrdfDir, "armsetup"), "joint_map.json");
                if (File.Exists(a)) return a;
                string b = Path.Combine(Path.Combine(strUrdfDir, "sim2real"), "joint_map.json");
                return File.Exists(b) ? b : null;
            }

            // ── 파일 ──
            /// <summary>예전 txt 형식 저장 — UTF-8(BOM)·CRLF, 실수는 "0.###", NaN 은 빈칸. 있던 파일은 &lt;파일&gt;.bak 로 남긴다.
            /// MakeUrdf 는 더 이상 쓰지 않는다 (모터 세팅 저장은 SaveDesignerFile 의 .ojw 만) — 다른 도구와의 호환용으로 남겨 둔다</summary>
            public bool Save(string strPath, out string strWhy)
            {
                strWhy = "";
                if (string.IsNullOrEmpty(strPath)) { strWhy = "경로 없음"; return false; }
                try
                {
                    string strDir = Path.GetDirectoryName(strPath);
                    if (!string.IsNullOrEmpty(strDir)) Directory.CreateDirectory(strDir);
                    CultureInfo ci = CultureInfo.InvariantCulture;
                    string strNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm", ci);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("// 로봇팔 모터 세팅 — 손으로 고쳐도 됩니다. 각도 단위 도(°), 틱 = 서보 위치 값\r\n");
                    sb.Append("// 방향 0 = 정(3D + 방향 = 틱 증가), 1 = 역 / 영점 틱 = 3D 0° 일 때의 서보 틱\r\n");
                    sb.Append("// 하한·상한 빈칸 = 리밋 없음 / 미러 -1 없음, -2 자기 반전, 숫자 = 짝 축(모션 좌우 반전용)\r\n");
                    sb.Append("version=").Append(FILE_VERSION.ToString(ci)).Append("\r\n");
                    sb.Append("robot=").Append(CleanText(strRobot)).Append("\r\n");
                    List<string> lstAx = new List<string>();
                    foreach (int ax in Axes) lstAx.Add(ax.ToString(ci));
                    sb.Append("axes=").Append(string.Join(",", lstAx.ToArray())).Append("\r\n");
                    sb.Append("saved=").Append(strNow).Append("\r\n");
                    sb.Append("// 축,실물ID,방향,영점틱,틱/회전,각/회전,하한,상한,초기각,초기각2,미러,그룹,제어,프로토콜,기종,확인,이름\r\n");
                    foreach (CArmMotor_t m in Motors)
                    {
                        if (m == null) continue;
                        string[] a = {
                            m.nAxis.ToString(ci), m.nMotorID.ToString(ci), m.nMotorDir.ToString(ci), m.nCenter_Evd.ToString(ci),
                            m.nMechMove.ToString(ci), Fmt(m.fMechAngle), Fmt(m.fLimit_Down), Fmt(m.fLimit_Up),
                            Fmt(m.fInitAngle), Fmt(m.fInitAngle2), m.nAxis_Mirror.ToString(ci), m.nGroupNumber.ToString(ci),
                            m.nMotorControlType.ToString(ci), m.nProtocolVersion.ToString(ci), m.nHwMotor_Key.ToString(ci),
                            m.bChecked ? "1" : "0", CleanName(m.strNickName) };
                        sb.Append("motor=").Append(string.Join(",", a)).Append("\r\n");
                    }
                    if (File.Exists(strPath)) File.Copy(strPath, strPath + ".bak", true);
                    File.WriteAllText(strPath, sb.ToString(), new UTF8Encoding(true));
                    strSaved = strNow;
                    return true;
                }
                catch (Exception ex) { strWhy = "저장 실패: " + ex.Message; return false; }
            }

            /// <summary>읽기 — key=value 줄, 줄 전체 또는 뒤쪽의 // 는 주석. motor= 는 17칸(이름이 마지막 칸), 빠진 칸은 기본값.
            /// version 이 1 보다 크면 아는 17칸만 읽는다 — 이름 뒤에 붙은 새 칸은 이름에 섞지 않고 버리며 경고한다 (version 1 은 예전처럼
            /// 17번째 칸부터 끝까지가 이름 — 쉼표는 공백으로 정리).
            /// 실패하면 null 과 사유. 성공해도 경고(모르는 키, 높은 version, 못 읽은 칸, 검증 문제)는 strWhy 에 줄로 담는다</summary>
            public static CArmMotorSet_t Load(string strPath, out string strWhy)
            {
                strWhy = "";
                if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { strWhy = "파일 없음: " + strPath; return null; }
                string[] astrLines;
                try { astrLines = File.ReadAllLines(strPath, Encoding.UTF8); }
                catch (Exception ex) { strWhy = "읽기 실패: " + ex.Message; return null; }
                CArmMotorSet_t set = new CArmMotorSet_t();
                List<string> lstWarn = new List<string>();
                List<int> lstAxesLine = null;
                int nFileVer = FileVersionOf(astrLines);         // motor= 줄을 어떻게 나눌지 — version 줄이 어디에 있든 먼저 본다
                for (int n = 0; n < astrLines.Length; n++)
                {
                    string s = astrLines[n];
                    int nCmt = s.IndexOf("//", StringComparison.Ordinal);
                    if (nCmt >= 0) s = s.Substring(0, nCmt);
                    s = s.Trim();
                    if (s.Length == 0) continue;
                    int nEq = s.IndexOf('=');
                    if (nEq <= 0) { lstWarn.Add(string.Format("{0}행: 알 수 없는 줄 '{1}'", n + 1, s)); continue; }
                    string k = s.Substring(0, nEq).Trim().ToLowerInvariant(), v = s.Substring(nEq + 1).Trim();
                    switch (k)
                    {
                        case "version":
                            {
                                int nVer;
                                if (!TryInt(v, out nVer)) lstWarn.Add(string.Format("{0}행: version '{1}' 을 읽지 못함", n + 1, v));
                                else if (nVer > FILE_VERSION) lstWarn.Add(string.Format("version={0} — 이 프로그램은 {1} 까지 안다. 아는 칸만 읽음", nVer, FILE_VERSION));
                            }
                            break;
                        case "robot": set.strRobot = v; break;
                        case "saved": set.strSaved = v; break;
                        case "axes":
                            lstAxesLine = new List<int>();
                            foreach (string t in v.Split(','))
                            {
                                int ax;
                                if (t.Trim().Length == 0) continue;
                                if (TryInt(t, out ax)) lstAxesLine.Add(ax);
                                else lstWarn.Add(string.Format("{0}행: axes 의 '{1}' 을 읽지 못함", n + 1, t.Trim()));
                            }
                            break;
                        case "motor":
                            {
                                CArmMotor_t m = ParseMotorLine(v, n + 1, nFileVer, lstWarn);
                                if (m == null) break;
                                if (set.Get(m.nAxis) != null) { lstWarn.Add(string.Format("{0}행: ID {1} 이 두 번 — 뒤 줄 무시", n + 1, m.nAxis)); break; }
                                set.Motors.Add(m);
                            }
                            break;
                        default:
                            lstWarn.Add(string.Format("{0}행: 모르는 키 '{1}'", n + 1, k));
                            break;
                    }
                }
                if (set.Motors.Count == 0)
                {
                    lstWarn.Insert(0, "motor= 줄이 없음");
                    strWhy = string.Join("\r\n", lstWarn.ToArray());
                    return null;
                }
                if (lstAxesLine != null && SignatureOf(lstAxesLine.ToArray()) != set.Signature)
                    lstWarn.Add("axes= (" + RangeText(lstAxesLine.ToArray()) + ") 와 motor= 줄의 축(" + RangeText(set.Axes) + ")이 다름 — motor= 줄을 따름");
                string strBad;
                if (!set.Validate(out strBad)) lstWarn.Add("검증: " + strBad.Replace("\r\n", " / "));
                strWhy = string.Join("\r\n", lstWarn.ToArray());
                return set;
            }

            private const int MOTOR_FIELDS = 17;                 // motor= 줄의 칸 수 (version 1) — 축 … 이름
            /// <summary>파일의 version= 값 (못 읽거나 없으면 1) — motor= 줄을 나누는 방법을 정한다</summary>
            private static int FileVersionOf(string[] astrLines)
            {
                int nVer = 1;
                foreach (string s0 in astrLines)
                {
                    string s = s0;
                    int nCmt = s.IndexOf("//", StringComparison.Ordinal);
                    if (nCmt >= 0) s = s.Substring(0, nCmt);
                    int nEq = s.IndexOf('=');
                    if (nEq <= 0 || s.Substring(0, nEq).Trim().ToLowerInvariant() != "version") continue;
                    int n;
                    if (TryInt(s.Substring(nEq + 1), out n)) nVer = n;
                }
                return nVer;
            }
            private static CArmMotor_t ParseMotorLine(string strValue, int nLine, int nFileVer, List<string> lstWarn)
            {
                // version 1: 17번째 칸부터 끝까지가 이름 (쉼표는 아래 CleanName 이 공백으로). 그보다 새 version: 아는 17칸만 — 뒤에 붙은 새 칸은 버린다
                string[] p = (nFileVer > FILE_VERSION) ? strValue.Split(',') : strValue.Split(new char[] { ',' }, MOTOR_FIELDS);
                for (int i = 0; i < p.Length; i++) p[i] = p[i].Trim();
                int nAxis;
                if (p.Length < 1 || !TryInt(p[0], out nAxis) || nAxis < 0)
                {
                    lstWarn.Add(string.Format("{0}행: 축 번호를 읽지 못함 — 줄 무시", nLine));
                    return null;
                }
                if (p.Length > MOTOR_FIELDS)
                    lstWarn.Add(string.Format("{0}행 ID {1}: 칸 {2}개 — 이 프로그램은 {3}칸까지 안다. {3}칸 뒤는 무시 (version={4})", nLine, nAxis, p.Length, MOTOR_FIELDS, nFileVer));
                CArmMotor_t m = CArmMotor_t.CreateDefault(nAxis);
                m.strSource = "파일";
                string strP = string.Format("{0}행 ID {1} ", nLine, nAxis);
                m.nMotorID = FieldInt(p, 1, nAxis, strP + "실물ID", lstWarn);
                m.nMotorDir = FieldInt(p, 2, 0, strP + "방향", lstWarn);
                m.nCenter_Evd = FieldInt(p, 3, 2048, strP + "영점틱", lstWarn);
                m.nMechMove = FieldInt(p, 4, 4096, strP + "틱/회전", lstWarn);
                m.fMechAngle = FieldFloat(p, 5, 360f, strP + "각/회전", lstWarn);
                m.fLimit_Down = FieldFloat(p, 6, float.NaN, strP + "하한", lstWarn);
                m.fLimit_Up = FieldFloat(p, 7, float.NaN, strP + "상한", lstWarn);
                m.fInitAngle = FieldFloat(p, 8, 0f, strP + "초기각", lstWarn);
                m.fInitAngle2 = FieldFloat(p, 9, 0f, strP + "초기각2", lstWarn);
                m.nAxis_Mirror = FieldInt(p, 10, -1, strP + "미러", lstWarn);
                m.nGroupNumber = FieldInt(p, 11, 0, strP + "그룹", lstWarn);
                m.nMotorControlType = FieldInt(p, 12, 0, strP + "제어", lstWarn);
                m.nProtocolVersion = FieldInt(p, 13, 2, strP + "프로토콜", lstWarn);
                m.nHwMotor_Key = FieldInt(p, 14, 0, strP + "기종", lstWarn);
                m.bChecked = FieldInt(p, 15, 0, strP + "확인", lstWarn) != 0;
                m.strNickName = (p.Length > 16) ? CleanName(p[16]) : "";
                return m;
            }
            private static int FieldInt(string[] p, int i, int nDef, string strWhat, List<string> lstWarn)
            {
                if (i >= p.Length || p[i].Length == 0) return nDef;
                int n;
                if (TryInt(p[i], out n)) return n;
                lstWarn.Add(strWhat + " '" + p[i] + "' 을 읽지 못함 — 기본값 " + nDef.ToString(CultureInfo.InvariantCulture));
                return nDef;
            }
            private static float FieldFloat(string[] p, int i, float fDef, string strWhat, List<string> lstWarn)
            {
                if (i >= p.Length || p[i].Length == 0) return fDef;
                float f;
                if (TryFloat(p[i], out f)) return f;
                lstWarn.Add(strWhat + " '" + p[i] + "' 을 읽지 못함 — " + (float.IsNaN(fDef) ? "빈칸(없음)" : "기본값 " + Fmt(fDef)));
                return fDef;
            }

            // ── 검증 ──
            /// <summary>세트 검증 — 축 중복, 실물 ID 중복·범위, 미러 짝의 상호성(짝 축이 표 안에 있고 서로 가리킴) + 모터마다 ValidateMotor.
            /// 실패하면 false 와 사유(줄마다 하나)</summary>
            public bool Validate(out string strWhy)
            {
                List<string> lst = new List<string>();
                if (Motors.Count == 0) lst.Add("모터 없음");
                List<int> lstAx = new List<int>(), lstBus = new List<int>();
                foreach (CArmMotor_t m in Motors)
                {
                    if (m == null) { lst.Add("빈 모터"); continue; }
                    if (lstAx.Contains(m.nAxis)) lst.Add("ID " + m.nAxis + ": 축 중복"); else lstAx.Add(m.nAxis);
                    if (lstBus.Contains(m.nMotorID)) lst.Add("ID " + m.nAxis + ": 실물 ID " + m.nMotorID + " 중복"); else lstBus.Add(m.nMotorID);
                    lst.AddRange(MotorErrors(m));
                }
                foreach (CArmMotor_t m in Motors)
                {
                    if (m == null) continue;
                    string p = "ID " + m.nAxis + ": ";
                    if (m.nAxis_Mirror >= 0)
                    {
                        CArmMotor_t q = Get(m.nAxis_Mirror);
                        if (m.nAxis_Mirror == m.nAxis) lst.Add(p + "미러가 자기 자신 — 자기 반전은 -2");
                        else if (q == null) lst.Add(p + "미러 짝 축 " + m.nAxis_Mirror + " 이 표에 없음");
                        else if (q.nAxis_Mirror != m.nAxis) lst.Add(p + "미러 짝 " + m.nAxis_Mirror + " 이 " + m.nAxis + " 를 가리키지 않음");
                    }
                    else if (m.nAxis_Mirror != -1 && m.nAxis_Mirror != -2) lst.Add(p + "미러 값 " + m.nAxis_Mirror + " — -1(없음)·-2(자기 반전)·축 번호만");
                }
                strWhy = string.Join("\r\n", lst.ToArray());
                return lst.Count == 0;
            }
            /// <summary>모터 하나 검증 — 축·실물 ID 범위(지금은 실물 ID = 축), 방향 0/1, 틱/회전·각/회전 &gt; 0, 영점 0~4095,
            /// 리밋 -360~360·하한 &lt; 상한, 프로토콜 2·위치 제어, 이름 규칙, 수동 리밋의 틱이 [1, 4094] 안
            /// (읽기가 틱 0·-1 을 무응답으로 보고, 위치 모드는 0 ↔ 4095 에서 감기기 때문). 쓰이는 리밋(수동 ∩ EDH — ComputeEffective 를 거친
            /// 세트)이 있으면 그 틱도 [1, 4094] 안이어야 한다</summary>
            public static bool ValidateMotor(CArmMotor_t m, out string strWhy)
            {
                List<string> lst = MotorErrors(m);
                strWhy = string.Join("\r\n", lst.ToArray());
                return lst.Count == 0;
            }
            private static List<string> MotorErrors(CArmMotor_t m)
            {
                List<string> lst = new List<string>();
                if (m == null) { lst.Add("빈 모터"); return lst; }
                string p = "ID " + m.nAxis + ": ";
                if (m.nAxis < 0 || m.nAxis > 252) lst.Add(p + "축 번호 범위 밖 (0~252)");
                if (m.nMotorID < 0 || m.nMotorID > 252) lst.Add(p + "실물 ID 범위 밖 (0~252)");
                else if (m.nMotorID != m.nAxis) lst.Add(p + "실물 ID " + m.nMotorID + " 가 축 번호와 다름 — 다른 번호는 다음 단계 기능 (지금은 같아야 함)");
                if (m.nMotorDir != 0 && m.nMotorDir != 1) lst.Add(p + "방향은 0(정)·1(역)만");
                bool bMech = true;
                if (m.nMechMove <= 0) { lst.Add(p + "틱/회전은 0 보다 커야 함"); bMech = false; }
                if (!(m.fMechAngle > 0f) || float.IsInfinity(m.fMechAngle)) { lst.Add(p + "각/회전은 0 보다 커야 함"); bMech = false; }
                if (m.nCenter_Evd < 0 || m.nCenter_Evd > 4095) lst.Add(p + "영점 틱 " + m.nCenter_Evd + " — 0~4095 밖");
                bool bLo = !float.IsNaN(m.fLimit_Down), bHi = !float.IsNaN(m.fLimit_Up);
                if (bLo && (m.fLimit_Down < -360f || m.fLimit_Down > 360f || float.IsInfinity(m.fLimit_Down))) lst.Add(p + "하한 " + Fmt(m.fLimit_Down) + "° — -360~360 밖");
                if (bHi && (m.fLimit_Up < -360f || m.fLimit_Up > 360f || float.IsInfinity(m.fLimit_Up))) lst.Add(p + "상한 " + Fmt(m.fLimit_Up) + "° — -360~360 밖");
                if (bLo && bHi && !(m.fLimit_Down < m.fLimit_Up)) lst.Add(p + "하한(" + Fmt(m.fLimit_Down) + "°)이 상한(" + Fmt(m.fLimit_Up) + "°)보다 작아야 함");
                if (m.nProtocolVersion != 2) lst.Add(p + "프로토콜 2 만 (지금 " + m.nProtocolVersion + ")");
                if (m.nMotorControlType != 0) lst.Add(p + "위치 제어(0)만 (지금 " + m.nMotorControlType + ")");
                if (!NameOk(m.strNickName)) lst.Add(p + "이름에 쉼표·줄바꿈·// 는 쓸 수 없음");
                if (bMech)
                    foreach (string s in TickWindowErrors(m)) lst.Add(p + s);
                return lst;
            }
            /// <summary>리밋을 틱으로 바꿨을 때 [1, 4094] 밖인 것 (문구 목록, "ID n: " 없이) — 수동 하한·상한, 그리고 쓰이는 리밋(수동 ∩ EDH)이
            /// 계산돼 있고 수동 값과 다르면 그것도. 틱/회전·각/회전이 0 이하면 빈 목록 (그 오류는 따로 나온다)</summary>
            internal static List<string> TickWindowErrors(CArmMotor_t m)
            {
                List<string> lst = new List<string>();
                if (m == null || m.nMechMove <= 0 || !(m.fMechAngle > 0f) || float.IsInfinity(m.fMechAngle)) return lst;
                TickWindowCheck(m, m.fLimit_Down, "하한", lst);
                TickWindowCheck(m, m.fLimit_Up, "상한", lst);
                if (!SameF(m.fEffLo, m.fLimit_Down)) TickWindowCheck(m, m.fEffLo, "쓰이는 하한(EDH $jointrange 포함)", lst);
                if (!SameF(m.fEffHi, m.fLimit_Up)) TickWindowCheck(m, m.fEffHi, "쓰이는 상한(EDH $jointrange 포함)", lst);
                return lst;
            }
            private static void TickWindowCheck(CArmMotor_t m, float fDeg, string strWhat, List<string> lst)
            {
                if (float.IsNaN(fDeg) || float.IsInfinity(fDeg)) return;
                int t = m.SimToTick(fDeg);
                if (t < 1 || t > 4094) lst.Add(strWhat + " " + Fmt(fDeg) + "° → 틱 " + t + " — [1, 4094] 밖");
            }
            /// <summary>쓰이는 리밋 계산 결과를 지운다 (fEffLo/Hi = NaN, LimitConflict = "") — 수동 값만으로 검증할 때</summary>
            internal void ClearEffective()
            {
                foreach (CArmMotor_t m in Motors) if (m != null) { m.fEffLo = float.NaN; m.fEffHi = float.NaN; }
                LimitConflict = "";
            }

            // ── 표 한 칸 편집 ──
            /// <summary>GetField/SetField 가 아는 칸 이름 (표 열 ↔ 키 대응용). 읽기 전용 표시 키: axis, efflo, effhi, proto, control, source</summary>
            public static string[] FieldKeys { get { return new string[] { "name", "dir", "center", "lo", "hi", "mirror", "group", "model", "busid", "mechmove", "mechangle", "init", "init2", "checked" }; } }

            /// <summary>표 한 칸 편집 — 검증하고, 거부하면 false 와 사유(값은 그대로). 받아들였는데 경고가 있으면 true 와 경고.
            /// 키: name, dir(정/역/0/1 — 바꾸면 확인 해제), center(0~4095), lo/hi(빈칸 = 없음), mirror(없음/자기 반전/축 번호 — 짝을 맞춘다),
            /// group, model(번호 또는 이름), busid(지금은 축 번호와 같아야 함), mechmove, mechangle, init, init2, checked(1/0)</summary>
            public bool SetField(int nAxis, string strKey, string strValue, out string strWhy)
            {
                strWhy = "";
                int nIdx = IndexOf(nAxis);
                if (nIdx < 0) { strWhy = "ID " + nAxis + " 이 표에 없음"; return false; }
                CArmMotor_t m = Motors[nIdx];
                string k = (strKey == null) ? "" : strKey.Trim().ToLowerInvariant();
                string v = (strValue == null) ? "" : strValue.Trim();
                if (k == "mirror") return SetMirror(m, v, out strWhy);
                CArmMotor_t t = m.Clone();
                string strWarn = "";
                int n;
                float f;
                switch (k)
                {
                    case "name":
                        if (!NameOk(v)) { strWhy = "이름에 쉼표·줄바꿈·// 는 쓸 수 없음"; return false; }
                        t.strNickName = v;
                        break;
                    case "dir":
                        if (v == "정" || v == "0") n = 0;
                        else if (v == "역" || v == "1") n = 1;
                        else { strWhy = "방향은 정·역 (0·1)"; return false; }
                        if (n != t.nMotorDir) { t.nMotorDir = n; t.bChecked = false; }   // 방향을 바꾸면 다시 확인해야 한다
                        break;
                    case "center":
                        {
                            if (!TryInt(v, out n)) { strWhy = "영점 틱은 정수"; return false; }
                            if (n < 0 || n > 4095) { strWhy = "영점 틱 " + n + " — 0~4095 밖"; return false; }
                            List<string> lstW = new List<string>();
                            if (n < 256 || n > 4095 - 256) lstW.Add("영점 틱 " + n + " 이 끝(0·4095)에서 256틱 안 — 관절을 조금만 돌려도 한 바퀴 경계를 넘을 수 있음");
                            float fDeg;
                            if (IsLargeZeroChange(m, n, out fDeg))   // U3 — 45° 넘게 바뀌면 UI 가 한 번 더 확인한다
                                lstW.Add(ZeroChangeText(n - m.nCenter_Evd, fDeg));
                            strWarn = string.Join("\r\n", lstW.ToArray());
                            t.nCenter_Evd = n;
                        }
                        break;
                    case "lo":
                    case "hi":
                        if (v.Length == 0) f = float.NaN;
                        else if (!TryFloat(v, out f)) { strWhy = "리밋은 실수 또는 빈칸(없음)"; return false; }
                        else if (f < -360f || f > 360f) { strWhy = "리밋 " + Fmt(f) + "° — -360~360 밖"; return false; }
                        if (k == "lo") t.fLimit_Down = f; else t.fLimit_Up = f;
                        t.fEffLo = float.NaN; t.fEffHi = float.NaN;   // EDH 와의 교집합은 적용(ApplyMotors) 때 다시 구한다
                        break;
                    case "group":
                        if (!TryInt(v, out n) || n < 0) { strWhy = "그룹은 0 이상 정수"; return false; }
                        t.nGroupNumber = n;
                        break;
                    case "model":
                        if (!ParseModel(v, out n)) { strWhy = "모르는 기종 '" + v + "' — 모델번호(예 1060) 또는 이름(예 XL430-W250)"; return false; }
                        t.nHwMotor_Key = n;
                        break;
                    case "busid":
                        if (!TryInt(v, out n) || n < 0 || n > 252) { strWhy = "실물 ID 는 0~252 정수"; return false; }
                        foreach (CArmMotor_t o in Motors)
                            if (o != null && o != m && o.nMotorID == n) { strWhy = "실물 ID " + n + " 는 ID " + o.nAxis + " 가 이미 씀"; return false; }
                        if (n != m.nAxis) { strWhy = "실물 ID 를 축 번호와 다르게 쓰는 것은 다음 단계 기능 (지금은 같아야 함)"; return false; }
                        t.nMotorID = n;
                        break;
                    case "mechmove":
                        if (!TryInt(v, out n) || n <= 0) { strWhy = "틱/회전은 0 보다 큰 정수"; return false; }
                        t.nMechMove = n;
                        break;
                    case "mechangle":
                        if (!TryFloat(v, out f) || !(f > 0f)) { strWhy = "각/회전은 0 보다 큰 실수"; return false; }
                        t.fMechAngle = f;
                        break;
                    case "init":
                    case "init2":
                        if (!TryFloat(v, out f)) { strWhy = "초기각은 실수"; return false; }
                        if (k == "init") t.fInitAngle = f; else t.fInitAngle2 = f;
                        break;
                    case "checked":
                        if (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase)) t.bChecked = true;
                        else if (v.Length == 0 || v == "0" || v.Equals("false", StringComparison.OrdinalIgnoreCase)) t.bChecked = false;
                        else { strWhy = "확인은 1·0"; return false; }
                        break;
                    default:
                        strWhy = "모르는 칸 '" + strKey + "'";
                        return false;
                }
                // 이 편집으로 새로 생긴 문제만 거부한다 (파일에서 읽은 다른 칸의 문제는 하나씩 고칠 수 있게 둔다)
                List<string> lstOld = MotorErrors(m);
                List<string> lstBad = new List<string>();
                foreach (string s in MotorErrors(t)) if (!lstOld.Contains(s)) lstBad.Add(s);
                if (lstBad.Count > 0) { strWhy = string.Join("\r\n", lstBad.ToArray()); return false; }
                if (!SameFields(m, t))
                {
                    if (k != "checked") t.strSource = "수정";
                    Motors[nIdx] = t;
                }
                strWhy = strWarn;
                return true;
            }
            private bool SetMirror(CArmMotor_t m, string v, out string strWhy)
            {
                strWhy = "";
                int nNew;
                if (v.Length == 0 || v == "없음" || v == "-1") nNew = -1;
                else if (v == "자기 반전" || v == "-2") nNew = -2;
                else if (!TryInt(v, out nNew) || nNew < 0) { strWhy = "미러는 없음·자기 반전·축 번호"; return false; }
                if (nNew == m.nAxis_Mirror) return true;
                if (nNew >= 0)
                {
                    if (nNew == m.nAxis) { strWhy = "자기 자신과는 짝지을 수 없음 — '자기 반전' 을 고르십시오"; return false; }
                    CArmMotor_t b = Get(nNew);
                    if (b == null) { strWhy = "축 " + nNew + " 이 표에 없음"; return false; }
                    if (b.nAxis_Mirror >= 0 && b.nAxis_Mirror != m.nAxis)
                    { strWhy = string.Format("{0} 는 이미 {1} 과 짝 — 먼저 푸십시오", nNew, b.nAxis_Mirror); return false; }
                    Unpair(m);
                    m.nAxis_Mirror = nNew; m.strSource = "수정";
                    b.nAxis_Mirror = m.nAxis; b.strSource = "수정";
                }
                else
                {
                    Unpair(m);
                    m.nAxis_Mirror = nNew; m.strSource = "수정";
                }
                return true;
            }
            /// <summary>m 의 지금 짝이 m 을 가리키고 있으면 그 짝을 푼다 (-1)</summary>
            private void Unpair(CArmMotor_t m)
            {
                if (m.nAxis_Mirror < 0) return;
                CArmMotor_t p = Get(m.nAxis_Mirror);
                if (p != null && p.nAxis_Mirror == m.nAxis) { p.nAxis_Mirror = -1; p.strSource = "수정"; }
            }
            private static bool SameFields(CArmMotor_t a, CArmMotor_t b)
            {
                return a.nAxis == b.nAxis && a.nMotorID == b.nMotorID && a.nMotorDir == b.nMotorDir && a.nCenter_Evd == b.nCenter_Evd
                    && a.nMechMove == b.nMechMove && a.fMechAngle == b.fMechAngle && SameF(a.fLimit_Down, b.fLimit_Down) && SameF(a.fLimit_Up, b.fLimit_Up)
                    && a.fInitAngle == b.fInitAngle && a.fInitAngle2 == b.fInitAngle2 && a.nAxis_Mirror == b.nAxis_Mirror
                    && a.nGroupNumber == b.nGroupNumber && a.nMotorControlType == b.nMotorControlType && a.nProtocolVersion == b.nProtocolVersion
                    && a.nHwMotor_Key == b.nHwMotor_Key && a.strNickName == b.strNickName && a.bChecked == b.bChecked;
            }
            private static bool SameF(float a, float b) { return (float.IsNaN(a) && float.IsNaN(b)) || a == b; }

            /// <summary>표 표시 문자열 — SetField 와 같은 키 (+ 읽기 전용 axis, efflo, effhi, proto, control, source). 없는 축·키는 ""</summary>
            public string GetField(int nAxis, string strKey)
            {
                CArmMotor_t m = Get(nAxis);
                if (m == null) return "";
                string k = (strKey == null) ? "" : strKey.Trim().ToLowerInvariant();
                CultureInfo ci = CultureInfo.InvariantCulture;
                switch (k)
                {
                    case "axis": return m.nAxis.ToString(ci);
                    case "name": return m.strNickName;
                    case "dir": return (m.nMotorDir == 0) ? "정" : "역";
                    case "center": return m.nCenter_Evd.ToString(ci);
                    case "lo": return Fmt(m.fLimit_Down);
                    case "hi": return Fmt(m.fLimit_Up);
                    case "efflo": return Fmt(m.LimitLo);
                    case "effhi": return Fmt(m.LimitHi);
                    case "mirror": return (m.nAxis_Mirror == -1) ? "없음" : ((m.nAxis_Mirror == -2) ? "자기 반전" : m.nAxis_Mirror.ToString(ci));
                    case "group": return m.nGroupNumber.ToString(ci);
                    case "model": return ModelName(m.nHwMotor_Key);
                    case "busid": return m.nMotorID.ToString(ci);
                    case "mechmove": return m.nMechMove.ToString(ci);
                    case "mechangle": return Fmt(m.fMechAngle);
                    case "init": return Fmt(m.fInitAngle);
                    case "init2": return Fmt(m.fInitAngle2);
                    case "proto": return m.nProtocolVersion.ToString(ci);
                    case "control": return m.nMotorControlType.ToString(ci);
                    case "checked": return m.bChecked ? "1" : "0";
                    case "source": return m.strSource;
                }
                return "";
            }

            // ── 다른 출처에서 가져오기 ──
            /// <summary>서명이 다른 세트(다른 로봇 파일)에서 같은 축만 옮긴다 — 나머지는 기본값, 짝이 따라오지 않은 미러는 푼다.
            /// anTaken = 옮긴 축</summary>
            public CArmMotorSet_t MergeFor(int[] anAxes, out int[] anTaken)
            {
                CArmMotorSet_t s = CreateDefault(anAxes, strRobot);
                List<int> lst = new List<int>();
                for (int i = 0; i < s.Motors.Count; i++)
                {
                    CArmMotor_t m = Get(s.Motors[i].nAxis);
                    if (m == null) continue;
                    s.Motors[i] = m.Clone();
                    lst.Add(m.nAxis);
                }
                foreach (CArmMotor_t m in s.Motors)
                {
                    if (m.nAxis_Mirror < 0) continue;
                    CArmMotor_t p = s.Get(m.nAxis_Mirror);
                    if (p == null || p.nAxis_Mirror != m.nAxis) m.nAxis_Mirror = -1;
                }
                anTaken = lst.ToArray();
                return s;
            }

            /// <summary>자동 방향 결과(joint_map.json, CSim2Real.SignCalib) 가져오기 — real_id 가 전부 이 표의 실물 ID 안에 있고
            /// 하나 이상 겹칠 때만 받아들인다 (다른 로봇 결과로 축이 뒤집히는 사고 방지). 받아들이면 그 축들의 방향 = (sign &lt; 0 ? 역 : 정),
            /// 출처 "자동", 확인 해제. 영점·리밋은 건드리지 않는다. 반환 = 방향이 바뀐 축 수, 거부 -1 (세트 그대로)</summary>
            public int ImportJointMap(string strPath, out string strWhy)
            {
                strWhy = "";
                if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { strWhy = "자동 방향 결과 파일 없음: " + strPath; return -1; }
                Dictionary<int, int> map = CSim2Real.LoadJointMap(strPath);
                if (map.Count == 0) { strWhy = "자동 방향 결과가 비어 있음 (real_id·sign 없음)"; return -1; }
                List<int> lstBus = new List<int>();
                foreach (CArmMotor_t m in Motors) if (m != null) lstBus.Add(m.nMotorID);
                List<int> lstIds = new List<int>(map.Keys);
                lstIds.Sort();
                int nOverlap = 0;
                bool bAllIn = true;
                foreach (int id in lstIds) { if (lstBus.Contains(id)) nOverlap++; else bAllIn = false; }
                if (!bAllIn || nOverlap == 0)
                {
                    strWhy = "다른 로봇(ID " + RangeText(lstIds.ToArray()) + ") 결과 — 가져오지 않음 (이 표는 ID " + RangeText(lstBus.ToArray()) + ")";
                    return -1;
                }
                int nChanged = 0;
                foreach (CArmMotor_t m in Motors)
                {
                    int sg;
                    if (m == null || !map.TryGetValue(m.nMotorID, out sg)) continue;
                    int d = (sg < 0) ? 1 : 0;
                    if (d != m.nMotorDir) nChanged++;
                    m.nMotorDir = d;
                    m.strSource = "자동";
                    m.bChecked = false;
                }
                strWhy = string.Format("자동 방향 결과 {0}축 — 방향 바뀜 {1}축 (영점·리밋은 그대로, 확인은 다시)", nOverlap, nChanged);
                return nChanged;
            }

            // ── 디자이너 파일 (.ojw) ──
            /// <summary>디자이너 .ojw 에서 이 세트의 축만 가져오기. 반환 = 가져온 축 수, 거부 -1 과 사유 (세트 그대로). 예외는 밖으로 내지 않는다.
            /// ⚠ UI 스레드(STA)에서만 부른다 — 화면 3D 를 건드리지 않으려고 임시 C3d(컨트롤)를 만들어 그것으로 읽는다
            ///   (FileOpen_Without_Event 는 그 인스턴스의 m_CRobotis·m_CMonster·m_lstMotorsEn 을 바꾼다). 지금 GL 컨텍스트는 끝나면 되돌린다.
            /// 헤더의 pSMotorInfo[축] 만 쓴다. 바꾸는 규칙:
            ///   방향 0 이 아니면 1 (역) / 리밋 0 → NaN(없음), ∓0.001 → 0 (헤더 규약을 되돌림) / 미러 -1·-2 는 그대로, 축 번호는 그 짝이 이 세트에 있고
            ///   서로 가리킬 때만 (아니면 -1 — 헤더 기본값 0 이 "축 0 과 짝" 으로 읽히는 함정) / 프로토콜 0 → 2 / 실물 ID 는 축 번호 (지금 규칙) /
            ///   이름은 첫 널 문자까지. 방향 확인(bChecked)은 확장 구역 armMotors 가 없는 옛 .ojw 면 지금 값을 두되, 방향·영점·틱/회전·각/회전이 바뀐 축은 해제한다.
            /// 결과는 Validate 규칙으로 검사하고, 통과하지 못하면 가져오지 않는다. 출처 = "디자이너", strSaved = 파일 고친 시각.
            /// 2026-10-15: 파일에 확장 구역 armMotors 가 있으면 방향 확인·출처를 그 축의 매핑(방향·영점·틱/회전·각/회전)이 저장 때와 같을 때만 되살린다
            ///   (텍스트·바이너리 형식 모두 — 바이너리는 끝 꼬리 블록). 그 밖의 확장 구역은 겹함수의 ext 로 돌려준다 (앱이 묻고 적용)</summary>
            public int ImportDesignerFile(string strOjwPath, out string strWhy)
            {
                C3d.COjwFileExt ext;
                return ImportDesignerFile(strOjwPath, out ext, out strWhy);
            }
            /// <summary>ImportDesignerFile 과 같고, 파일의 확장 구역(덧붙임 — CArmOjwExtras.From 으로 읽는다)을 ext 로 돌려준다 (못 읽으면 빈 것)</summary>
            public int ImportDesignerFile(string strOjwPath, out C3d.COjwFileExt ext, out string strWhy)
            {
                strWhy = "";
                ext = new C3d.COjwFileExt();
                try
                {
                    if (string.IsNullOrEmpty(strOjwPath) || !File.Exists(strOjwPath)) { strWhy = "디자이너 파일 없음: " + strOjwPath; return -1; }
                    if (Motors.Count == 0) { strWhy = "표에 모터가 없음"; return -1; }
                    string strThr;
                    if (!StaOk(out strThr)) { strWhy = strThr; return -1; }
                    C3d.COjwDesignerHeader hdr = null;
                    string strErr, strTextWarn = "";
                    bool bOk = WithTempC3d(delegate(C3d t) { bool b = t.FileOpen_Without_Event(strOjwPath, out hdr); strTextWarn = t.GetFileOpen_Warn() ?? ""; return b; }, out strErr);
                    if (!bOk || hdr == null || hdr.pSMotorInfo == null)
                    {
                        strWhy = "디자이너 파일을 읽지 못함" + ((strErr.Length > 0) ? " (" + strErr + ")" : "") + ((strTextWarn.Length > 0) ? " (" + strTextWarn.Replace("\r\n", " / ") + ")" : "") + ": " + strOjwPath;
                        return -1;
                    }
                    if (hdr.Ext != null) ext = hdr.Ext;
                    SMotorInfo_t[] a = hdr.pSMotorInfo;
                    CArmMotorSet_t s = Clone();
                    List<string> lstWarn = new List<string>();
                    if (strTextWarn.Length > 0) lstWarn.Add("텍스트 형식 읽기 경고: " + strTextWarn.Replace("\r\n", " / "));
                    int nTaken = 0;
                    for (int i = 0; i < s.Motors.Count; i++)
                    {
                        CArmMotor_t old = s.Motors[i];
                        if (old == null) continue;
                        if (old.nAxis < 0 || old.nAxis >= a.Length)
                        {
                            lstWarn.Add("ID " + old.nAxis + ": 디자이너 파일의 축 범위(0~" + (a.Length - 1) + ") 밖 — 지금 값 그대로");
                            continue;
                        }
                        s.Motors[i] = MotorFromHeader(old, a, s, lstWarn);
                        nTaken++;
                    }
                    if (nTaken == 0) { strWhy = "가져올 축이 없음 — " + string.Join(" / ", lstWarn.ToArray()); return -1; }
                    s.ApplyArmMotorsSection(ext.Get(C3d.OJWEXT_ARMMOTORS), lstWarn);
                    s.ClearEffective();
                    string strBad;
                    if (!s.Validate(out strBad))
                    {
                        strWhy = "가져오지 않음 — 디자이너 파일의 모터 정보가 이 표의 규칙에 맞지 않음:\r\n" + strBad +
                                 ((lstWarn.Count > 0) ? "\r\n" + string.Join("\r\n", lstWarn.ToArray()) : "");
                        return -1;
                    }
                    Motors = s.Motors;
                    if (string.IsNullOrEmpty(strRobot)) strRobot = s.strRobot;     // 표시용 로봇 이름 (확장 구역 armMotors 의 robot=)
                    LimitConflict = "";
                    try { strSaved = File.GetLastWriteTime(strOjwPath).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture); } catch { }
                    lstWarn.Insert(0, "디자이너 파일에서 " + nTaken + "축 가져옴 (ID " + RangeText(Axes) + ", " + ((hdr.eFileFormat == C3d.EOjwFileFormat.Text) ? "텍스트 형식" : "바이너리 형식") + ")");
                    strWhy = string.Join("\r\n", lstWarn.ToArray());
                    return nTaken;
                }
                catch (Exception ex) { strWhy = "디자이너 파일 가져오기 오류: " + ex.Message; return -1; }
            }
            private static CArmMotor_t MotorFromHeader(CArmMotor_t old, SMotorInfo_t[] a, CArmMotorSet_t set, List<string> lstWarn)
            {
                int ax = old.nAxis;
                SMotorInfo_t h = a[ax];
                string p = "ID " + ax + ": ";
                CArmMotor_t m = CArmMotor_t.CreateDefault(ax);
                if (h.nMotorID != ax) lstWarn.Add(p + "파일의 실물 ID " + h.nMotorID + " — 지금은 축 번호와 같아야 해 " + ax + " 로 씀");
                m.nMotorDir = (h.nMotorDir != 0) ? 1 : 0;
                m.nCenter_Evd = h.nCenter_Evd;
                if (h.nMechMove > 0) m.nMechMove = h.nMechMove;
                else lstWarn.Add(p + "파일의 틱/회전 " + h.nMechMove + " — 기본 4096");
                if (h.fMechAngle > 0f && !float.IsInfinity(h.fMechAngle)) m.fMechAngle = h.fMechAngle;
                else lstWarn.Add(p + "파일의 각/회전 " + Fmt(h.fMechAngle) + " — 기본 360");
                m.fLimit_Down = LimitFromHeader(h.fLimit_Down, -0.001f);
                m.fLimit_Up = LimitFromHeader(h.fLimit_Up, 0.001f);
                m.fInitAngle = FiniteOr0(h.fInitAngle);
                m.fInitAngle2 = FiniteOr0(h.fInitAngle2);
                int mr = h.nAxis_Mirror;
                if (mr == -1 || mr == -2) m.nAxis_Mirror = mr;
                else if (mr >= 0 && mr != ax && mr < a.Length && set.Get(mr) != null && a[mr].nAxis_Mirror == ax) m.nAxis_Mirror = mr;
                else
                {
                    m.nAxis_Mirror = -1;
                    if (mr != 0) lstWarn.Add(p + "파일의 미러 " + mr + " — 짝이 이 표에서 맞지 않아 없음으로");
                }
                m.nGroupNumber = (h.nGroupNumber >= 0) ? h.nGroupNumber : 0;
                m.nMotorControlType = h.nMotorControlType;
                m.nProtocolVersion = (h.nProtocolVersion == 0) ? 2 : h.nProtocolVersion;   // 헤더 규약: 0 해도 2 와 같다
                m.nHwMotor_Key = (h.nHwMotor_Key >= 0 && h.nHwMotor_Key <= 65535) ? h.nHwMotor_Key : 0;
                m.strNickName = NickFromHeader(h.strNickName);
                m.bChecked = old.bChecked && m.SameMapping(old);     // 확장 구역 armMotors 가 없는 .ojw (옛 파일) — 지금 확인을 두되 해석이 바뀐 축은 다시 확인 (있으면 ApplyArmMotorsSection 이 덮는다)
                m.strSource = "디자이너";
                return m;
            }
            /// <summary>헤더 리밋 → 세트 리밋: 0 = 없음(NaN), 헤더 규약의 ∓0.001(0° 경계) → 0</summary>
            private static float LimitFromHeader(float f, float fZeroCode)
            {
                if (float.IsNaN(f) || float.IsInfinity(f) || f == 0f) return float.NaN;
                if (Math.Abs(f - fZeroCode) < 1e-6f) return 0f;
                return f;
            }
            private static float FiniteOr0(float f) { return (float.IsNaN(f) || float.IsInfinity(f)) ? 0f : f; }
            private static string NickFromHeader(string s)
            {
                if (s == null) return "";
                int n = s.IndexOf('\0');
                if (n >= 0) s = s.Substring(0, n);
                return CleanName(s);
            }

            // ── 확장 구역 armMotors (2026-10-15) — 방향 확인·출처 (헤더 SMotorInfo_t 에 칸이 없는 것) ──
            //   "motor=축,확인(1/0),매핑(방향:영점:틱/회전:각/회전),출처" 줄. 되살릴 때 그 축의 매핑이 같아야 확인을 되살린다 (디자이너에서 방향·영점을 고쳤으면 다시 확인)
            /// <summary>이 세트의 확장 구역 armMotors 글 (UTF-8 글, CRLF)</summary>
            public string ArmMotorsSectionText()
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                StringBuilder sb = new StringBuilder();
                sb.Append("// 모터 세팅 표 — motor=축,방향 확인(1/0),매핑(방향:영점:틱/회전:각/회전),출처. 매핑이 같을 때만 방향 확인을 되살린다\r\n");
                sb.Append("robot=").Append(CleanText(strRobot)).Append("\r\n");
                foreach (CArmMotor_t m in Motors)
                {
                    if (m == null) continue;
                    sb.Append("motor=").Append(m.nAxis.ToString(ci)).Append(',').Append(m.bChecked ? "1" : "0").Append(',')
                      .Append(MotorMapText(m)).Append(',').Append(CleanText(m.strSource)).Append("\r\n");
                }
                return sb.ToString();
            }
            // 매핑 한 칸 "방향:영점:틱/회전:각/회전" (MappingKey 의 축 뒤 네 칸과 같다)
            private static string MotorMapText(CArmMotor_t m)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return m.nMotorDir.ToString(ci) + ":" + m.nCenter_Evd.ToString(ci) + ":" + m.nMechMove.ToString(ci) + ":" + Fmt(m.fMechAngle);
            }
            /// <summary>확장 구역 armMotors 를 이 세트에 — 축마다 매핑이 같으면 방향 확인·출처를 되살리고, 다르면 확인 해제 + 출처 "디자이너" (경고 한 줄).
            /// 글이 없으면(옛 .ojw) 아무것도 하지 않는다. 표시용 로봇 이름은 비었을 때만. 예외 없음</summary>
            internal void ApplyArmMotorsSection(string strText, List<string> lstWarn)
            {
                if (string.IsNullOrEmpty(strText)) return;
                try
                {
                    int nOk = 0;
                    List<int> lstMis = new List<int>();
                    foreach (string raw in strText.Split('\n'))
                    {
                        string s = raw.Trim();
                        if (s.Length == 0 || s.StartsWith("//", StringComparison.Ordinal)) continue;
                        int nEq = s.IndexOf('=');
                        if (nEq <= 0) continue;
                        string k = s.Substring(0, nEq).Trim().ToLowerInvariant(), v = s.Substring(nEq + 1).Trim();
                        if (k == "robot") { if (string.IsNullOrEmpty(strRobot)) strRobot = v; continue; }
                        if (k != "motor") continue;
                        string[] p = v.Split(new char[] { ',' }, 4);
                        int ax;
                        if (p.Length < 3 || !TryInt(p[0], out ax)) continue;
                        CArmMotor_t m = Get(ax);
                        if (m == null) continue;
                        if (p[2].Trim() != MotorMapText(m)) { m.bChecked = false; m.strSource = "디자이너"; lstMis.Add(ax); continue; }
                        m.bChecked = (p[1].Trim() == "1");
                        if (p.Length > 3 && p[3].Trim().Length > 0) m.strSource = p[3].Trim();
                        nOk++;
                    }
                    if (lstMis.Count > 0 && lstWarn != null)
                        lstWarn.Add("ID " + RangeText(lstMis.ToArray()) + ": 저장 뒤 방향·영점·틱/회전·각/회전이 바뀌어 (디자이너 등) 방향 확인을 되살리지 않음 — 다시 확인");
                }
                catch { }
            }

            /// <summary>디자이너 파일(.ojw)로 저장. 실패하면 false 와 사유 — 예외는 밖으로 내지 않는다.
            /// ⚠ UI 스레드(STA)에서만 부른다 — 임시 C3d 로 쓰고 다시 읽는다 (화면 3D·헤더·연결·버스는 건드리지 않는다).
            /// 내용 = 화면 3D 헤더(c3d.m_CHeader — 모델·DH·그룹 등 지금 3D 그대로)의 사본에 이 세트의 축을 덮어쓴 것.
            ///   리밋은 수동 리밋만 (EDH $jointrange 는 DH 안에 있어 따로 쓰지 않는다 — 헤더 규약: 없음 = 0, 0° = ∓0.001), 기종 0(미지정)도 그대로.
            ///   방향 확인(bChecked)·출처는 확장 구역 armMotors 로 (2026-10-15 — 이 겹함수는 바이너리 형식, 확장 구역은 끝 꼬리 블록).
            /// 순서: 검증(Validate) → &lt;파일&gt;.tmp 에 쓰기 → ImportDesignerFile 로 다시 읽어 비교 → 있던 파일은 &lt;파일&gt;.bak 로 남기고 바꿔 넣기.
            ///   중간에 실패하면 있던 파일은 그대로다. 다시 읽은 값이 다른 칸이 있으면(예: 이름이 31바이트를 넘어 잘림) 저장은 하고 strWhy 에 그 목록</summary>
            public bool SaveDesignerFile(C3d c3d, string strPath, out string strWhy)
            {
                return SaveDesignerFile(c3d, strPath, null, C3d.EOjwFileFormat.Binary, out strWhy);
            }
            /// <summary>디자이너 파일(.ojw)로 저장 — 형식을 고른다 (Text = 텍스트 형식, Binary = 예전 바이너리 — 다른 프로그램·옛 DLL 용).
            /// strUrdfDir 가 있으면 그 폴더의 덧붙임 3가지(로봇 정의·로봇팔 설정 sagarm.txt, 수직 눈 확인 기록, 적용 중 처짐 보정 모델 — CArmOjwExtras.Collect)를
            /// 확장 구역으로 함께 담는다 (null = 담지 않음). 방향 확인·출처(armMotors)는 늘 담는다. 확장 구역은 바이너리면 끝 꼬리 블록 (옛 읽기는 무시한다).
            /// 그 밖은 3 인자 겹함수와 같다 (다시 읽어 표의 칸·방향 확인·출처·확장 구역이 같은지 비교)</summary>
            public bool SaveDesignerFile(C3d c3d, string strPath, string strUrdfDir, C3d.EOjwFileFormat eFormat, out string strWhy)
            {
                return SaveDesignerFileEx(c3d, strPath, (strUrdfDir != null) ? CArmOjwExtras.Collect(strUrdfDir) : null, eFormat, out strWhy);
            }
            // 디자이너 파일(.ojw)로 저장 본체 — 덧붙임 extras (null 이면 덧붙임 3가지를 담지 않는다). 그 밖은 위 겹함수와 같다
            private bool SaveDesignerFileEx(C3d c3d, string strPath, CArmOjwExtras extras, C3d.EOjwFileFormat eFormat, out string strWhy)
            {
                strWhy = "";
                string strTmp = null;
                try
                {
                    if (c3d == null || c3d.m_CHeader == null || c3d.m_CHeader.pSMotorInfo == null) { strWhy = "3D 가 없음 — 저장하지 않음"; return false; }
                    if (string.IsNullOrEmpty(strPath)) { strWhy = "경로 없음"; return false; }
                    if (Motors.Count == 0) { strWhy = "모터 없음 — 저장하지 않음"; return false; }
                    string strThr;
                    if (!StaOk(out strThr)) { strWhy = strThr; return false; }
                    CArmMotorSet_t set = Clone();
                    set.ClearEffective();                                       // 파일에는 수동 리밋만
                    string strBad;
                    if (!set.Validate(out strBad)) { strWhy = "저장하지 않음 — 검증:\r\n" + strBad; return false; }

                    string strFull = Path.GetFullPath(strPath);
                    string strDir = Path.GetDirectoryName(strFull);
                    if (!string.IsNullOrEmpty(strDir)) Directory.CreateDirectory(strDir);
                    strTmp = strFull + ".tmp";
                    if (File.Exists(strTmp)) File.Delete(strTmp);

                    C3d.COjwDesignerHeader hdr;
                    lock (c3d.EdhLimit_SyncRoot) hdr = CopyHeader(c3d.m_CHeader);
                    List<int> lstEn = new List<int>(c3d.m_lstMotorsEn);
                    set.WriteMotorFields(hdr.pSMotorInfo, true);
                    // 확장 구역 (2026-10-15) — 사본에만 (화면 헤더는 그대로): 방향 확인·출처는 늘, 덧붙임 3가지는 extras 가 있을 때 (urdf 폴더의 파일에서 모은 것 등)
                    hdr.Ext.Set(C3d.OJWEXT_ARMMOTORS, set.ArmMotorsSectionText());
                    if (extras != null) extras.PutTo(hdr.Ext);
                    string strFileTmp = strTmp, strErr, strSaveErr = "";
                    bool bWrote = WithTempC3d(delegate(C3d t)
                    {
                        t.m_CHeader = hdr;
                        t.m_lstMotorsEn.Clear();
                        t.m_lstMotorsEn.AddRange(lstEn);
                        t.FileSave(strFileTmp, hdr, eFormat);                   // 안에서 예외를 삼킨다 — 쓰다 멈춘 까닭(GetFileSave_Error)·파일이 생겼는지·다시 읽기로 확인
                        strSaveErr = t.GetFileSave_Error() ?? "";
                        return strSaveErr.Length == 0 && File.Exists(strFileTmp) && new FileInfo(strFileTmp).Length > 0;
                    }, out strErr);
                    if (!bWrote)
                    {
                        TryDelete(strTmp);
                        if (strSaveErr.Length > 0) strErr = (strErr.Length > 0) ? strSaveErr + " / " + strErr : strSaveErr;   // 쓰다 멈춘 까닭 (예전에는 잘린 파일의 '다시 읽기 실패' 로만 보였다)
                        strWhy = "저장 실패 — 디자이너 파일을 쓰지 못함" + ((strErr.Length > 0) ? " (" + strErr + ")" : "") + ". 있던 파일은 그대로";
                        return false;
                    }

                    CArmMotorSet_t chk = CreateDefault(set.Axes, strRobot);
                    string strChk;
                    C3d.COjwFileExt extChk;
                    if (chk.ImportDesignerFile(strTmp, out extChk, out strChk) < 0)
                    {
                        TryDelete(strTmp);
                        strWhy = "저장 실패 — 쓴 파일을 다시 읽지 못함: " + strChk + ". 있던 파일은 그대로";
                        return false;
                    }
                    List<string> lstDiff = new List<string>();
                    List<string> lstKeys = new List<string>(FieldKeys);
                    lstKeys.Add("source");                                      // 방향 확인·출처도 확장 구역 armMotors 로 다시 읽힌다
                    foreach (int ax in set.Axes)
                        foreach (string k in lstKeys)
                        {
                            string v0 = set.GetField(ax, k), v1 = chk.GetField(ax, k);
                            if (v0 != v1) lstDiff.Add("ID " + ax + " " + k + ": '" + v0 + "' → 파일 '" + v1 + "'");
                        }
                    foreach (string strSec in hdr.Ext.Names)
                        if (!string.Equals(hdr.Ext.Get(strSec), extChk.Get(strSec), StringComparison.Ordinal)) lstDiff.Add("확장 구역 " + strSec + " 이 다시 읽은 것과 다름");

                    try
                    {
                        if (File.Exists(strFull)) File.Replace(strTmp, strFull, strFull + ".bak", true);
                        else File.Move(strTmp, strFull);
                    }
                    catch (Exception exR)
                    {
                        // File.Replace 를 못 쓰는 곳 — 복사로 (있던 파일은 먼저 .bak 으로)
                        try
                        {
                            if (File.Exists(strFull)) File.Copy(strFull, strFull + ".bak", true);
                            File.Copy(strTmp, strFull, true);
                            TryDelete(strTmp);
                        }
                        catch (Exception exC)
                        {
                            TryDelete(strTmp);
                            strWhy = "저장 실패: " + exR.Message + " / " + exC.Message + ". 있던 파일은 그대로 (또는 " + Path.GetFileName(strFull) + ".bak)";
                            return false;
                        }
                    }
                    strSaved = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    if (lstDiff.Count > 0) strWhy = "저장했지만 다시 읽은 값이 다른 칸이 있음:\r\n" + string.Join("\r\n", lstDiff.ToArray());
                    return true;
                }
                catch (Exception ex)
                {
                    if (strTmp != null) TryDelete(strTmp);
                    strWhy = "저장 실패: " + ex.Message + ". 있던 파일은 그대로";
                    return false;
                }
            }

            /// <summary>디자이너 헤더 사본 — 저장 중 FileSave 가 고치는 배열(모터 정보·DH·IK 코드)은 따로, 나머지(문자열·모델)는 같이 쓴다</summary>
            private static C3d.COjwDesignerHeader CopyHeader(C3d.COjwDesignerHeader h)
            {
                System.Reflection.MethodInfo mi = typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                C3d.COjwDesignerHeader c = (C3d.COjwDesignerHeader)mi.Invoke(h, null);
                c.pSMotorInfo = CloneArr(h.pSMotorInfo);
                c.pDhParamAll = CloneArr(h.pDhParamAll);
                c.pSOjwCode = CloneArr(h.pSOjwCode);
                c.Ext = (h.Ext != null) ? h.Ext.Clone() : new C3d.COjwFileExt();   // 확장 구역 — 저장할 사본에 구역을 넣어도 화면 헤더는 그대로
                if (h.lstGroupInfo != null) c.lstGroupInfo = new List<SGroupInfo_t>(h.lstGroupInfo);
                return c;
            }
            private static T[] CloneArr<T>(T[] a) { return (a == null) ? null : (T[])a.Clone(); }
            private static void TryDelete(string strPath) { try { if (File.Exists(strPath)) File.Delete(strPath); } catch { } }
            private static bool StaOk(out string strWhy)
            {
                strWhy = "";
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) return true;
                strWhy = "디자이너 파일은 UI 스레드(STA)에서만 읽고 쓸 수 있음 — 임시 3D 컨트롤을 만든다";
                return false;
            }

            [System.Runtime.InteropServices.DllImport("opengl32.dll")] private static extern IntPtr wglGetCurrentDC();
            [System.Runtime.InteropServices.DllImport("opengl32.dll")] private static extern IntPtr wglGetCurrentContext();
            [System.Runtime.InteropServices.DllImport("opengl32.dll")] private static extern bool wglMakeCurrent(IntPtr hDc, IntPtr hRc);

            /// <summary>임시 C3d 로 f 를 부른다 — 화면 3D 를 건드리지 않게. C3d 를 만들면 그 GL 컨텍스트가 지금 것이 되므로 끝나면(Dispose 뒤) 원래 것으로 되돌린다.
            /// C3d 생성자는 현재 폴더에 freeglut.dll 을 쓰는데, 그 DLL 이 이미 올라와 있으면 덮어쓰지 못해 실패한다 — 그때는 임시 폴더에서 한 번 더.
            /// 예외는 strErr 로 (false)</summary>
            private static bool WithTempC3d(Func<C3d, bool> f, out string strErr)
            {
                strErr = "";
                IntPtr hDc = IntPtr.Zero, hRc = IntPtr.Zero;
                bool bCtx = false;
                try { hDc = wglGetCurrentDC(); hRc = wglGetCurrentContext(); bCtx = true; } catch { }
                C3d t = null;
                try
                {
                    try { t = new C3d(); }
                    catch (Exception ex1)
                    {
                        string strCwd = Environment.CurrentDirectory;
                        try
                        {
                            string strAlt = Path.Combine(Path.GetTempPath(), "OpenJigWare_ojw");
                            Directory.CreateDirectory(strAlt);
                            Environment.CurrentDirectory = strAlt;
                            t = new C3d();
                        }
                        catch (Exception ex2) { strErr = "임시 3D 를 만들지 못함: " + ex1.Message + " / " + ex2.Message; return false; }
                        finally { try { Environment.CurrentDirectory = strCwd; } catch { } }
                    }
                    return f(t);
                }
                catch (Exception ex) { strErr = ex.Message; return false; }
                finally
                {
                    if (t != null) { try { t.Dispose(); } catch { } }
                    if (bCtx) { try { wglMakeCurrent(hDc, hRc); } catch { } }
                }
            }

            // ── 적용 (3D 쪽) ──
            /// <summary>쓰이는 리밋 계산: fEffLo/Hi = 수동 ∩ EDH $jointrange (c3d.EdhLimit_Get). 한쪽만 있으면 그쪽 (빈 칸은 다른 쪽 값).
            /// 교집합이 비는 축은 LimitConflict 에 남기고, 그 축의 fEffLo/Hi 는 양쪽에서 더 좁은 경계 그대로(하한 &gt; 상한)로 둔다 —
            /// 어느 입력보다도 넓어지지 않는다 (예전처럼 수동 값만 쓰면 수동 칸이 빈 쪽의 EDH 경계가 사라진다).
            /// 이런 세트는 ApplyMotors 가 거부하고(적용된 세트·헤더·장면은 이전 그대로), DH 를 다시 만든 뒤라면 움직이는 동작을 막는다</summary>
            public void ComputeEffective(C3d c3d)
            {
                List<string> lstBad = new List<string>();
                object oSync = (c3d != null) ? c3d.EdhLimit_SyncRoot : new object();
                lock (oSync)                                    // 모든 축을 같은 DH 해석 결과로 (해석 도중의 반쯤 지운 기록을 섞지 않게)
                {
                    foreach (CArmMotor_t m in Motors)
                    {
                        if (m == null) continue;
                        float fLo = m.fLimit_Down, fHi = m.fLimit_Up;
                        float eLo, eHi;
                        if (c3d != null && c3d.EdhLimit_Get(m.nAxis, out eLo, out eHi))
                        {
                            float fNewLo = float.IsNaN(fLo) ? eLo : Math.Max(fLo, eLo);
                            float fNewHi = float.IsNaN(fHi) ? eHi : Math.Min(fHi, eHi);
                            if (fNewLo > fNewHi)
                                lstBad.Add(string.Format(CultureInfo.InvariantCulture, "ID {0}: 수동 리밋({1}~{2}°)과 EDH $jointrange({3}~{4}°)가 겹치지 않음 — 수동 리밋이나 $jointrange 를 고치십시오",
                                                         m.nAxis, Fmt(fLo), Fmt(fHi), Fmt(eLo), Fmt(eHi)));
                            fLo = fNewLo;
                            fHi = fNewHi;
                        }
                        m.fEffLo = fLo;
                        m.fEffHi = fHi;
                    }
                }
                LimitConflict = string.Join("\r\n", lstBad.ToArray());
            }

            /// <summary>C3d 헤더(m_CHeader.pSMotorInfo)에 투영 — 3D 드래그·IK 리밋과 디자이너 계열 기능이 같은 값을 보게.
            /// ① prevApplied 에만 있던 축은 기본값(방향 0·영점 2048·4096/360·미러 -1, 리밋 = EDH 또는 0)으로 되돌린다
            /// ② 이 세트의 축은 배열 원소에 직접 쓴다 (SMotorInfo_t 는 struct) — 기종은 0(미지정)이 아닐 때만
            /// ③ 리밋은 쓰이는 값(수동 ∩ EDH)을 헤더 규약으로: NaN → 0(없음), 정확히 0 → ∓0.001. 간섭·Guide·주소·rpm·예비 칸은 건드리지 않는다.
            ///    수동 ∩ EDH 가 빈 축(LimitConflict — 하한 &gt; 상한)은 헤더 리밋을 그대로 둔다 (거꾸로 된 리밋은 3D 를 상한에 붙여 버린다).
            /// 쓰는 동안 C3d 헤더 리밋 잠금(EdhLimit_SyncRoot)을 잡는다 — DH 해석(ApplyEdhMotorLimits)과 섞이지 않게</summary>
            public void ApplyToHeader(C3d c3d, CArmMotorSet_t prevApplied)
            {
                if (c3d == null || c3d.m_CHeader == null || c3d.m_CHeader.pSMotorInfo == null) return;
                lock (c3d.EdhLimit_SyncRoot) ApplyToHeader_Locked(c3d, prevApplied);
            }
            private void ApplyToHeader_Locked(C3d c3d, CArmMotorSet_t prevApplied)
            {
                SMotorInfo_t[] a = c3d.m_CHeader.pSMotorInfo;
                if (prevApplied != null)
                {
                    foreach (CArmMotor_t p in prevApplied.Motors)
                    {
                        if (p == null || Get(p.nAxis) != null) continue;
                        int ax = p.nAxis;
                        if (ax < 0 || ax >= a.Length) continue;
                        a[ax].nMotorID = ax;
                        a[ax].nMotorDir = 0;
                        a[ax].nCenter_Evd = 2048;
                        a[ax].nMechMove = 4096;
                        a[ax].fMechAngle = 360f;
                        a[ax].fInitAngle = 0f;
                        a[ax].fInitAngle2 = 0f;
                        a[ax].nAxis_Mirror = -1;
                        a[ax].nGroupNumber = 0;
                        a[ax].nMotorControlType = 0;
                        a[ax].nProtocolVersion = 2;
                        a[ax].strNickName = "";
                        float eLo, eHi;
                        if (c3d.EdhLimit_Get(ax, out eLo, out eHi)) { a[ax].fLimit_Down = HdrLo(eLo); a[ax].fLimit_Up = HdrHi(eHi); }
                        else { a[ax].fLimit_Down = 0f; a[ax].fLimit_Up = 0f; }
                    }
                }
                WriteMotorFields(a, false);
            }
            /// <summary>이 세트의 축을 SMotorInfo_t 배열 원소에 쓴다 (ApplyToHeader ②③ 와 .ojw 저장이 같이 쓴다).
            /// bFile = 파일로 저장할 헤더 사본 — 기종 0(미지정)도 그대로 쓴다 (화면 헤더에는 0 을 쓰지 않는다)</summary>
            private void WriteMotorFields(SMotorInfo_t[] a, bool bFile)
            {
                foreach (CArmMotor_t m in Motors)
                {
                    if (m == null) continue;
                    int ax = m.nAxis;
                    if (ax < 0 || ax >= a.Length) continue;
                    a[ax].nMotorID = m.nMotorID;
                    a[ax].nMotorDir = m.nMotorDir;
                    a[ax].nCenter_Evd = m.nCenter_Evd;
                    a[ax].nMechMove = m.nMechMove;
                    a[ax].fMechAngle = m.fMechAngle;
                    a[ax].fInitAngle = m.fInitAngle;
                    a[ax].fInitAngle2 = m.fInitAngle2;
                    a[ax].nAxis_Mirror = m.nAxis_Mirror;
                    a[ax].nGroupNumber = m.nGroupNumber;
                    a[ax].nMotorControlType = m.nMotorControlType;
                    a[ax].nProtocolVersion = m.nProtocolVersion;
                    a[ax].strNickName = (m.strNickName == null) ? "" : m.strNickName;
                    if (bFile || m.nHwMotor_Key != 0) a[ax].nHwMotor_Key = m.nHwMotor_Key;   // 화면 헤더: 기본값(1060)을 미지정(0)으로 지우지 않는다
                    float fLo, fHi;
                    if (!HeaderLimits(m, out fLo, out fHi)) continue;   // 수동 ∩ EDH 가 빈 축 — 헤더 리밋은 그대로
                    a[ax].fLimit_Down = fLo;
                    a[ax].fLimit_Up = fHi;
                }
            }
            private static float HdrLo(float f) { if (float.IsNaN(f)) return 0f; return (f == 0f) ? -0.001f : f; }   // ApplyEdhMotorLimits 와 같은 규약
            private static float HdrHi(float f) { if (float.IsNaN(f)) return 0f; return (f == 0f) ? 0.001f : f; }
            /// <summary>그 모터의 쓰이는 리밋을 헤더 규약으로 (ApplyToHeader 가 쓰는 값): NaN → 0(없음), 정확히 0 → ∓0.001.
            /// 수동 ∩ EDH 가 비어 하한 &gt; 상한이면 false (헤더에 쓰지 않는다)</summary>
            internal static bool HeaderLimits(CArmMotor_t m, out float fLo, out float fHi)
            {
                float lo = m.LimitLo, hi = m.LimitHi;
                fLo = HdrLo(lo);
                fHi = HdrHi(hi);
                return float.IsNaN(lo) || float.IsNaN(hi) || !(lo > hi);
            }

            // ── 매핑 키 (학습 결과 파일에 기록 — 방향·영점이 바뀌었는지 비교) ──
            /// <summary>"11:0:2048:4096:360|12:0:2048:4096:360|…" — 축:방향:영점:틱/회전:각/회전, 축 오름차순 (Motors 순서와 무관)</summary>
            public string MappingKey()
            {
                List<CArmMotor_t> lst = new List<CArmMotor_t>();
                foreach (CArmMotor_t m in Motors) if (m != null) lst.Add(m);
                lst.Sort(delegate(CArmMotor_t x, CArmMotor_t y) { return x.nAxis.CompareTo(y.nAxis); });
                CultureInfo ci = CultureInfo.InvariantCulture;
                StringBuilder sb = new StringBuilder();
                foreach (CArmMotor_t m in lst)
                {
                    if (sb.Length > 0) sb.Append('|');
                    sb.Append(m.nAxis.ToString(ci)).Append(':').Append(m.nMotorDir.ToString(ci)).Append(':')
                      .Append(m.nCenter_Evd.ToString(ci)).Append(':').Append(m.nMechMove.ToString(ci)).Append(':').Append(Fmt(m.fMechAngle));
                }
                return sb.ToString();
            }
            /// <summary>그 축들의 기본 세팅 매핑 키 — 키가 없는 옛 결과 파일은 이것으로 본다</summary>
            public static string DefaultMappingKey(int[] anAxes) { return CreateDefault(anAxes, "").MappingKey(); }
            /// <summary>두 매핑 키의 차이 문구 — "ID 13 영점 2048→2131, ID 12 방향 정→역". 같으면 "".
            /// 옛 키가 비어 있으면(키 없는 옛 파일) 새 키의 축들의 기본 매핑으로 본다</summary>
            public static string DescribeMappingDiff(string strKeyOld, string strKeyNew)
            {
                Dictionary<int, string[]> dNew = ParseKey(strKeyNew);
                Dictionary<int, string[]> dOld;
                if (string.IsNullOrEmpty(strKeyOld)) dOld = ParseKey(DefaultMappingKey(new List<int>(dNew.Keys).ToArray()));
                else dOld = ParseKey(strKeyOld);
                List<int> lstAx = new List<int>(dNew.Keys);
                foreach (int ax in dOld.Keys) if (!lstAx.Contains(ax)) lstAx.Add(ax);
                lstAx.Sort();
                List<string> lst = new List<string>();
                foreach (int ax in lstAx)
                {
                    string[] o, n;
                    bool bO = dOld.TryGetValue(ax, out o), bN = dNew.TryGetValue(ax, out n);
                    if (!bO) { lst.Add("ID " + ax + " 새로 생김"); continue; }
                    if (!bN) { lst.Add("ID " + ax + " 없어짐"); continue; }
                    if (o[1] != n[1]) lst.Add("ID " + ax + " 방향 " + DirWord(o[1]) + "→" + DirWord(n[1]));
                    if (o[2] != n[2]) lst.Add("ID " + ax + " 영점 " + o[2] + "→" + n[2]);
                    if (o[3] != n[3]) lst.Add("ID " + ax + " 틱/회전 " + o[3] + "→" + n[3]);
                    if (o[4] != n[4]) lst.Add("ID " + ax + " 각/회전 " + o[4] + "→" + n[4]);
                }
                return string.Join(", ", lst.ToArray());
            }
            private static Dictionary<int, string[]> ParseKey(string strKey)
            {
                Dictionary<int, string[]> d = new Dictionary<int, string[]>();
                if (string.IsNullOrEmpty(strKey)) return d;
                foreach (string s in strKey.Split('|'))
                {
                    string[] p = s.Split(':');
                    int ax;
                    if (p.Length < 5 || !TryInt(p[0], out ax)) continue;
                    for (int i = 0; i < p.Length; i++) p[i] = p[i].Trim();
                    d[ax] = p;
                }
                return d;
            }
            private static string DirWord(string s) { return (s == "0") ? "정" : ((s == "1") ? "역" : s); }

            /// <summary>방향 확인 안 된 축 (오름차순) — 측정·시험 시작 때 앱이 경고에 쓴다 (막지는 않는다)</summary>
            public int[] UncheckedAxes()
            {
                List<int> lst = new List<int>();
                foreach (CArmMotor_t m in Motors) if (m != null && !m.bChecked && !lst.Contains(m.nAxis)) lst.Add(m.nAxis);
                lst.Sort();
                return lst.ToArray();
            }

            // ── 영점 변경 (U3: 허용 + 경고, 45° 넘게 바뀌면 한 번 더 확인) ──
            /// <summary>이 각도를 넘게 영점이 바뀌면 UI 가 한 번 더 확인한다</summary>
            public const float ZERO_RECONFIRM_DEG = 45f;
            /// <summary>영점(중심 틱)을 nNewCenter 로 바꾸면 ZERO_RECONFIRM_DEG(45°) 를 넘게 바뀌나. fDeg = 바뀌는 각 (틱 차 × 각/회전 ÷ 틱/회전, 부호 있음)</summary>
            public static bool IsLargeZeroChange(CArmMotor_t m, int nNewCenter, out float fDeg)
            {
                fDeg = 0f;
                if (m == null || m.nMechMove <= 0 || !(m.fMechAngle > 0f)) return false;
                fDeg = (float)(nNewCenter - m.nCenter_Evd) * m.fMechAngle / (float)m.nMechMove;
                return Math.Abs(fDeg) > ZERO_RECONFIRM_DEG;
            }
            internal static string ZeroChangeText(int nDiffTick, float fDeg)
            {
                return string.Format(CultureInfo.InvariantCulture, "영점이 {0:+0;-0}틱({1:+0.0;-0.0}°) 바뀜 — 45° 넘게 바뀌면 관절 모양을 한 번 더 확인하십시오", nDiffTick, fDeg);
            }
            /// <summary>U3 경고 문구 — 처짐 보정·바닥 모델이 있을 때 매핑(방향·영점·틱/회전·각/회전)이 바뀌면</summary>
            public const string RetrainWarnText = "처짐 보정·바닥 모델은 옛 영점·방향에서 학습됨 — 다시 학습하기 전까지 틀릴 수 있음";

            // ── 기종 ──
            /// <summary>표의 기종 콤보에 넣는 모델번호 (X 계열 — XL430·XL330·XM430·XC430·2XL430·2XC430)</summary>
            public static int[] KnownModelKeys { get { return (int[])s_anKnownModels.Clone(); } }
            /// <summary>모델번호 → 이름: 0 → "미지정", 아는 번호 → 정식 이름(CSim2Real.ModelNameOf), 모르는 번호 → "기타(n)"</summary>
            public static string ModelName(int nKey)
            {
                if (nKey == 0) return "미지정";
                string s = CSim2Real.ModelNameOf(nKey);
                return (s != null) ? s : "기타(" + nKey.ToString(CultureInfo.InvariantCulture) + ")";
            }
            /// <summary>기종 표시·입력 → 모델번호: "미지정"/빈칸 → 0, "기타(n)"·숫자 → n, 정식 이름 → 번호 (대소문자 무시)</summary>
            public static bool ParseModel(string s, out int nKey)
            {
                nKey = 0;
                string v = (s == null) ? "" : s.Trim();
                if (v.Length == 0 || v == "미지정") return true;
                if (v.StartsWith("기타(", StringComparison.Ordinal) && v.EndsWith(")", StringComparison.Ordinal)) v = v.Substring(3, v.Length - 4).Trim();
                int n;
                if (TryInt(v, out n)) { if (n < 0 || n > 65535) return false; nKey = n; return true; }
                foreach (int k in s_anKnownModels)
                    if (string.Equals(CSim2Real.ModelNameOf(k), v, StringComparison.OrdinalIgnoreCase)) { nKey = k; return true; }
                return false;
            }

            // ── 도우미 ──
            internal static string Fmt(float f) { return float.IsNaN(f) ? "" : f.ToString("0.###", CultureInfo.InvariantCulture); }
            internal static bool TryInt(string s, out int n)
            {
                return int.TryParse((s == null) ? "" : s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n);
            }
            internal static bool TryFloat(string s, out float f)
            {
                if (!float.TryParse((s == null) ? "" : s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out f)) return false;
                return !float.IsNaN(f) && !float.IsInfinity(f);
            }
            private static bool NameOk(string s)
            {
                if (s == null) return true;
                return s.IndexOf(',') < 0 && s.IndexOf('\r') < 0 && s.IndexOf('\n') < 0 && s.IndexOf("//", StringComparison.Ordinal) < 0;
            }
            private static string CleanText(string s)
            {
                if (s == null) return "";
                string t = s.Replace("\r", " ").Replace("\n", " ");
                while (t.IndexOf("//", StringComparison.Ordinal) >= 0) t = t.Replace("//", "/");
                return t.Trim();
            }
            private static string CleanName(string s) { return CleanText(s).Replace(",", " "); }
        }

        // ────────────────────────────────────────────────────────────
        // 모터 세팅 .ojw 의 덧붙임 (2026-10-15)
        // ────────────────────────────────────────────────────────────
        /// <summary>MakeUrdf 모터 세팅 .ojw 에 함께 담는 덧붙임 3가지 — 디자이너 파일의 확장 구역 (COjw_12_3D_OjwText.cs).
        ///   robotDef  = 로봇 정의·로봇팔 설정 (&lt;urdf&gt;\sagarm.txt 글 그대로)
        ///   eyeRecord = 수직 눈 확인 기록 (실습 상태 state.txt 의 eyedef·eyemap·eyetime)
        ///   sagComp   = 적용 중 처짐 보정 모델 (&lt;urdf&gt;\sagcomp_model.txt 글 그대로)
        /// 방향 확인·출처(armMotors)는 CArmMotorSet_t 가 저장·불러오기에서 직접 다룬다.
        /// 불러올 때: 로봇 정의·처짐 보정 모델은 앱이 묻고(기본 [아니오]) 적용한다 — 정의는 sagarm.txt 저장 길(CSagArmSetup.CopySettingsFromText),
        /// 모델은 C3d.SagRun_ActivateText (백업 + 적용). 눈 확인 기록은 정의 서명과 모터 매핑 키가 지금과 같을 때만 되살린다 (C3d.SagPrac_EyeRestore).</summary>
        public class CArmOjwExtras
        {
            public string RobotDefText = null;                  // null = 없음
            public string EyeRecordText = null;
            public string SagCompText = null;

            public bool HasRobotDef { get { return !string.IsNullOrEmpty(RobotDefText); } }
            public bool HasEyeRecord { get { return !string.IsNullOrEmpty(EyeRecordText); } }
            public bool HasSagComp { get { return !string.IsNullOrEmpty(SagCompText); } }
            public bool IsEmpty { get { return !HasRobotDef && !HasEyeRecord && !HasSagComp; } }
            /// <summary>urdf 폴더의 파일에서 모은다 — 없는 파일의 칸은 null. 예외 없음</summary>
            public static CArmOjwExtras Collect(string strUrdfDir)
            {
                CArmOjwExtras x = new CArmOjwExtras();
                if (string.IsNullOrEmpty(strUrdfDir)) return x;
                try
                {
                    string p = Path.Combine(strUrdfDir, C3d.CSagArmSetup.FILE);
                    if (File.Exists(p))
                    {
                        byte[] raw = File.ReadAllBytes(p);
                        int n = (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF) ? 3 : 0;
                        x.RobotDefText = new UTF8Encoding(false).GetString(raw, n, raw.Length - n);
                    }
                }
                catch { }
                x.EyeRecordText = C3d.SagPrac_EyeRecordText(strUrdfDir);
                x.SagCompText = C3d.SagRun_ActiveModelText(strUrdfDir);
                return x;
            }
            /// <summary>확장 구역에 넣는다 — 있는 것은 넣고 없는 것은 그 구역을 지운다 (옛 내용이 남지 않게)</summary>
            public void PutTo(C3d.COjwFileExt ext)
            {
                if (ext == null) return;
                ext.Set(C3d.OJWEXT_ROBOTDEF, HasRobotDef ? RobotDefText : null);
                ext.Set(C3d.OJWEXT_EYERECORD, HasEyeRecord ? EyeRecordText : null);
                ext.Set(C3d.OJWEXT_SAGCOMP, HasSagComp ? SagCompText : null);
            }
            /// <summary>확장 구역에서 읽는다 (ImportDesignerFile 의 ext). 없으면 빈 것</summary>
            public static CArmOjwExtras From(C3d.COjwFileExt ext)
            {
                CArmOjwExtras x = new CArmOjwExtras();
                if (ext == null) return x;
                x.RobotDefText = ext.Get(C3d.OJWEXT_ROBOTDEF);
                x.EyeRecordText = ext.Get(C3d.OJWEXT_EYERECORD);
                x.SagCompText = ext.Get(C3d.OJWEXT_SAGCOMP);
                return x;
            }
            /// <summary>담긴 로봇 정의·설정 (sagarm.txt 와 같은 규칙으로 읽은 것 — 읽기 경고는 LoadWarn). 없으면 null</summary>
            public C3d.CSagArmSetup RobotDef() { return HasRobotDef ? C3d.CSagArmSetup.FromText(RobotDefText) : null; }
            /// <summary>담긴 것 이름 ("로봇 정의·설정, 눈 확인 기록, 처짐 보정 모델" — 없으면 "")</summary>
            public string Describe()
            {
                List<string> l = new List<string>();
                if (HasRobotDef) l.Add("로봇 정의·설정");
                if (HasEyeRecord) l.Add("수직 눈 확인 기록");
                if (HasSagComp) l.Add("처짐 보정 모델");
                return string.Join(", ", l.ToArray());
            }
        }

        // ────────────────────────────────────────────────────────────
        // 결과 구조체
        // ────────────────────────────────────────────────────────────
        /// <summary>읽기 결과 (축 하나) — 레지스터는 표시·경고용 (EEPROM 은 쓰지 않는다). 행 색: 무응답·리밋 밖 = 빨강, bWarn = 노랑</summary>
        public class CArmMotorState_t
        {
            public int nAxis, nBusID;
            public bool bResponded;                                     // addr 0 (모델번호) 응답
            public int nModel; public string strModel = "";             // addr 0 (2)
            public int nDriveMode = -1, nOperatingMode = -1;            // addr 10 (1), 11 (1) — -1 = 못 읽음
            public int nHomingOffset;                                   // addr 20 (4)
            public int nMaxPos, nMinPos;                                // addr 48 (4), 52 (4)
            public bool bTorque; public int nHwError;                   // addr 64 (1), 70 (1)
            public bool bTorqueKnown;                                   // 토크 상태를 읽었다 (bTorque 유효)
            public bool bTickOk;                                        // 위치를 읽었다 (nTick·fSim 유효)
            public int nTick; public float fSim = float.NaN;            // addr 132 (4, 정착 읽기) → 세팅으로 해석한 3D 각
            public float fVolt = float.NaN; public int nTempC;          // addr 144 (2) × 0.1 V, 146 (1) °C
            public bool bOutside;                                       // 쓰이는 리밋 밖 (OutsideTolDeg 넘게)
            public bool bWarn;                                          // Drive Mode 역방향·Homing Offset·위치 제어 아님·기종 다름·하드웨어 오류
            public bool bModelFilled;                                   // 표의 기종이 미지정이라 실물 기종으로 채웠다 (적용된 세트에 반영됨 — 앱 편집본에도 옮길 것)
            public string strNote = "";                                 // 경고 모음 (" · " 로 이음)
            public CArmMotorState_t Clone() { return (CArmMotorState_t)MemberwiseClone(); }
        }

        /// <summary>조그 한 번의 결과 — 3D 명령과 실측 (3D 만이면 실측은 NaN)</summary>
        public class CArmJogResult_t
        {
            public int nAxis;
            public float fFrom, fTo;                                                    // 3D 명령 (도)
            public float fRealFrom = float.NaN, fRealTo = float.NaN, fRealDelta = float.NaN;   // 실측 (3D 각)
            public string strNote = "";
        }

        // ────────────────────────────────────────────────────────────
        // 실물 연결 하나
        // ────────────────────────────────────────────────────────────
        /// <summary>로봇팔 실물 연결 하나 — CScene_t 한 개를 가진다. 탭의 연결 줄·모터 세팅 표와 측정·시험 작업이 같은 연결을 빌려 쓴다.
        /// 잠금: 소유자 문자열 하나. TryAcquire(소유자) 로 잡은 동안 다른 소유자의 버스 동작(읽기·조그·영점·토크·재부팅·연결 해제·따라가기·세팅 적용)은 거부된다.
        /// 같은 소유자 이름으로 부르면 다시 잡아도 통과(재진입) — 이름 하나는 한 스레드만 쓸 것. 소유자 null 로 부르는 함수는
        /// 그 동작 동안만 잡았다 놓는다 (소유자 없는 짧은 동작).
        /// 기다림: 작업 스레드에서 잡을 때 다른 스레드의 소유자 없는 짧은 동작(따라가기 읽기·읽기·조그 등)이 잡고 있으면 LockWaitMs 까지 기다렸다가
        /// 판정한다. 이름 있는 소유자(측정 등 작업)가 잡고 있으면 기다리지 않고 거부. UI 스레드(WinForms 동기화 컨텍스트가 걸린 스레드)는
        /// 따라가기 읽기만 (최대 200ms) 기다린다 — 다른 동작은 잡고 있는 작업 스레드가 로그를 UI 로 보내므로(Invoke) 기다리면 서로 멈춘다.
        /// 같은 스레드가 잡고 있으면(DoEvents 안에서 다시 불린 경우) 기다리지 않는다. 따라가기(FollowOnce)는 기다리지 않고, 기다리는 쪽이 있으면
        /// 건너뛴다 — 따라가기 읽기는 BusyOwner·IsBusy 에도 보이지 않는다.
        /// 스레드: Connect·Disconnect·ReadState·TorqueOn·TorqueOff·Reboot·EnsureSynced·Jog·PreviewZero·SetZeroHere·ApplyMotors(연결 중)는
        /// 버스를 쓰는 블로킹 함수라 작업 스레드에서 부른다. FollowOnce 는 UI 타이머(UI 스레드)에서 — 작업 스레드의 동작이 그 읽기를 기다렸다 잡는다.
        /// StateChanged 는 부른 스레드에서 온다 — UI 는 BeginInvoke 로 받는다.
        /// DH 를 다시 만든 뒤의 리밋 반영(ReapplyAfterRebuild)은 잠금이 잡혀 있거나 3D 모션 중이면 미뤘다가 잠금을 새로 잡을 때·놓을 때 한다
        /// (작업 중에 리밋이 바뀌면 움직이지 않던 축이 새 리밋으로 잘려 튈 수 있다).
        /// 헤더 리밋 붙잡기: 연결 중에는 C3d 의 헤더 리밋 붙잡기 훅(C3d.EdhLimit_SetHold)이 이 연결의 것이라, DH 해석(ApplyEdhMotorLimits —
        /// MakeAll·URDF 내보내기)이 연결된 축의 헤더 리밋을 바꾸지 못한다 — 헤더 = 지금 적용된 리밋(= 장면이 실물로 보낼 때 자르는 리밋).
        /// 새 $jointrange 는 기록만 되고, ReapplyAfterRebuild 가 사용 중이 아닐 때 장면과 헤더에 한 번에 반영한다 (반영하지 못하면 그대로 + 막힘).
        /// 훅은 생성할 때(자리가 비었거나 그 주인이 연결돼 있지 않으면)와 연결할 때 걸고, 연결 해제 때 뗀다 — 연결 안 됨이면 훅은 아무 축도 붙잡지 않아
        /// DH 해석은 예전과 같다</summary>
        public class CArmConn_t
        {
            private readonly C3d m_C3d;
            private readonly object m_oSync = new object();
            private readonly object m_oHdr;                             // C3d 헤더 리밋 잠금(EdhLimit_SyncRoot) — 헤더 투영 + 적용 세트 교체 + DH 해석의 헤더 쓰기를 한 덩어리로
            private CArmMotorSet_t m_HdrSet = null;                     // 헤더에 투영해 둔 세트 (m_oHdr 안에서) — 보통 m_Motors, ApplyMotors 가 장면을 바꾸는 동안은 새 세트
            private readonly C3d.DEdhLimitHold m_fnHold;                // C3d 헤더 리밋 붙잡기 훅 (HoldHeaderLimit)
            private volatile CScene_t m_Scene = null;
            private volatile CArmMotorSet_t m_Motors = null;
            private CArmMotorState_t[] m_aState = null;
            private string m_strOwner = null;                           // 잠금 소유자 (null = 비어 있음)
            private bool m_bOwnerAnon = false;                          // 소유자 null 로 부른 동작이 잠깐 잡은 것 (재진입 안 됨)
            private int m_nOwnerThread = 0;                             // 잠금을 잡은 스레드 — 같은 스레드는 기다리지 않는다 (DoEvents 안에서 다시 불린 경우 등)
            private int m_nWaiters = 0;                                 // 잠금을 기다리는 스레드 수 — 있으면 따라가기는 건너뛴다
            private bool m_bHdrApplying = false;                        // ApplyMotors 가 헤더를 바꾸는 중 (m_oHdr 안에서만 읽고 쓴다)
            private const string FOLLOW_NAME = "따라가기";
            private const int FOLLOW_WAIT_UI_MS = 200;                  // UI 스레드가 따라가기 읽기를 기다리는 최대 시간 (읽기 한 번은 보통 15ms, 무응답 서보가 있으면 약 70ms)
            private volatile bool m_bUnsynced = false;                  // 토크 ON 때 3D 정렬을 못 했다 → 움직이기 전에 실측으로 맞춘다
            private string m_strAxesNowSig = null;                      // 지금 EDH 축 서명 (Connect·ReapplyAfterRebuild)
            private int m_nPort = 0, m_nBaud = 0;
            private volatile bool m_bRebuildPending = false;            // DH 다시 만든 뒤 리밋 반영이 남아 있다 (사용 중이라 미뤘거나 장면이 거부)
            private volatile string m_strRebuildBlock = null;           // 장면이 새 리밋을 거부한 사유 — 풀릴 때까지 움직이는 동작 막힘
            private bool m_bRebuildDeferLogged = false;                 // "미룸" 로그를 한 번만

            /// <summary>c3d = 화면의 3D (실물과 같이 움직일 장면). C3d 의 헤더 리밋 붙잡기 훅 자리가 비었거나 그 주인(다른 연결 객체)이 연결돼 있지 않으면
            /// 이 연결 객체가 맡는다 (연결 전에는 아무 축도 붙잡지 않는다)</summary>
            public CArmConn_t(C3d c3d)
            {
                m_C3d = c3d;
                m_oHdr = (c3d != null) ? c3d.EdhLimit_SyncRoot : new object();
                m_fnHold = new C3d.DEdhLimitHold(HoldHeaderLimit);
                HookHeaderLimits(false);
            }

            public C3d c3d { get { return m_C3d; } }
            /// <summary>연결 중일 때의 장면 (측정 함수에 그대로 넘긴다), 아니면 null</summary>
            public CScene_t Scene { get { CScene_t sc = m_Scene; return (sc != null && sc.IsConnected) ? sc : null; } }
            /// <summary>지금 적용된 세트의 복사본 — 아직 적용한 적이 없으면 null</summary>
            public CArmMotorSet_t Motors { get { CArmMotorSet_t s = m_Motors; return (s == null) ? null : s.Clone(); } }
            public bool IsConnected { get { CScene_t sc = m_Scene; return sc != null && sc.IsConnected; } }
            public bool IsTorqueOn { get { CScene_t sc = m_Scene; return sc != null && sc.IsTorqueOn; } }
            /// <summary>마지막으로 연결한 포트 번호·속도</summary>
            public int Port { get { return m_nPort; } }
            public int Baud { get { return m_nBaud; } }
            /// <summary>잠금 소유자 — null = 비어 있음. 따라가기 읽기(잠깐 — 작업 스레드는 기다렸다 잡는다)는 소유자로 보지 않는다</summary>
            public string BusyOwner { get { lock (m_oSync) return IsFollowHeld() ? null : m_strOwner; } }
            /// <summary>소유자가 있거나 3D 모션 중</summary>
            public bool IsBusy { get { return BusyOwner != null || (m_C3d != null && m_C3d.MoveJoints_IsMoving()); } }
            /// <summary>연결 뒤 EDH 모터 구성(또는 적용된 세트의 축)이 연결된 모터와 달라졌다 → 토크 켜기·움직이는 기능 막힘 (다시 연결)</summary>
            public bool ConfigMismatch
            {
                get
                {
                    CScene_t sc = m_Scene;
                    if (sc == null || !sc.IsConnected) return false;
                    string strScene = CArmMotorSet_t.SignatureOf(sc.IDs);
                    string strNow;
                    lock (m_oSync) strNow = m_strAxesNowSig;
                    if (strNow != null && strNow != strScene) return true;
                    CArmMotorSet_t s = m_Motors;
                    return s != null && s.Signature != strScene;
                }
            }
            /// <summary>토크 ON 인데 3D 정렬을 못 한 상태 — 움직이기 전에 EnsureSynced 가 실측으로 맞춘다</summary>
            public bool NeedsSync { get { return IsTorqueOn && m_bUnsynced; } }
            /// <summary>마지막 읽기 결과 (따라가기 갱신 포함, 복사본). 없으면 null</summary>
            public CArmMotorState_t[] LastState { get { lock (m_oSync) return CloneStates(m_aState); } }
            /// <summary>연결·토크·잠금·세팅·읽기 결과가 바뀌면 — 작업 스레드에서 올 수 있다 (UI 는 BeginInvoke)</summary>
            public event EventHandler StateChanged;
            /// <summary>로그 받는 곳 (null = Ojw.printf)</summary>
            public CSim2Real.DLog Logger = null;

            /// <summary>조그 한 번의 최대 각(°)과 최고 속도(°/s) — 조그 시간 = max(JogMinMs, 1.5 × |Δ| ÷ JogDegPerSec) 라 smoothstep 보간의
            /// 최고 속도(평균의 1.5 배)가 JogDegPerSec 을 넘지 않는다</summary>
            public float JogMaxDeg = 10f, JogDegPerSec = 15f;
            public int JogMinMs = 300;
            public float SyncTolDeg = 6f, OutsideTolDeg = 1f;
            /// <summary>처짐 보정이 걸린 채 서보가 붙잡은 목표를 이어받았을 때의 알림 (토크 켜기·연결·움직이기 전 확인의 보고와 작업 로그) —
            /// 보정은 페이드 없이 꺼졌고(C3d.SagComp_OffForAdopt) 3D = 서보 목표다. 앱의 '처짐 보정 켬' 은 StateChanged 로 따라온다</summary>
            public const string ADOPT_COMP_OFF = "이어받기: 서보가 붙잡은 자세 그대로 — 처짐 보정 끔 (다시 켜면 다음 동작의 출력 틱 1초에 걸쳐 들어옴)";
            /// <summary>시험용: 미연결이면 3D 만 조그</summary>
            public bool AllowSimOnly = false;
            /// <summary>잠금을 잡을 때 다른 스레드의 소유자 없는 짧은 동작이 잡고 있으면 기다리는 최대 시간 (ms, 0 = 기다리지 않음) — 작업 스레드 기준,
            /// UI 스레드는 따라가기 읽기만 최대 200ms. 따라가기 읽기 한 번은 보통 15ms, 무응답 서보가 있으면 약 70ms (응답 대기 50ms),
            /// 상태 읽기(ReadState)는 수백 ms</summary>
            public int LockWaitMs = 500;

            // ── 잠금 ──
            /// <summary>잠금 잡기 — 비어 있거나 같은 소유자면 true. 다른 스레드의 소유자 없는 짧은 동작(따라가기 읽기 등)이 잡고 있으면 끝나기를
            /// LockWaitMs 까지 기다린다 (작업 스레드 — UI 스레드는 따라가기 읽기만). 이름 있는 소유자가 잡고 있으면 기다리지 않고 false.
            /// null 이면 잡지 않고 "지금 잡을 수 있나" 만 돌려준다 — 이 스레드가 기다리면 비는 따라가기 읽기는 빈 것으로 본다</summary>
            public bool TryAcquire(string strOwner) { return TryAcquireFor(strOwner, LockWaitMs); }
            /// <summary>TryAcquire 와 같고 기다리는 최대 시간을 준다 (ms, 0 = 기다리지 않음). 기다리는 것은 다른 스레드의 소유자 없는 짧은 동작이
            /// 잡고 있을 때뿐이다 (UI 스레드는 따라가기 읽기만, 최대 200ms)</summary>
            public bool TryAcquireFor(string strOwner, int nWaitMs)
            {
                if (strOwner == null) { lock (m_oSync) return m_strOwner == null || (IsFollowHeld() && CanWaitHere()); }
                bool bTook;
                string w;
                return EnterLock(strOwner, strOwner, true, nWaitMs, false, out bTook, out w);
            }
            /// <summary>잠금 놓기 — 지금 소유자일 때만 (다른 이름은 무시). 작업 중에 미뤄 둔 DH 리밋 반영이 있으면 놓은 뒤 한다</summary>
            public void Release(string strOwner)
            {
                if (strOwner == null) return;
                bool bFreed = false;
                lock (m_oSync)
                {
                    if (m_strOwner == strOwner && !m_bOwnerAnon) { m_strOwner = null; m_nOwnerThread = 0; bFreed = true; Monitor.PulseAll(m_oSync); }
                }
                if (!bFreed) return;
                if (m_bRebuildPending && !m_C3d.MoveJoints_IsMoving())
                {
                    bool bTook;
                    string w;
                    if (EnterLock(null, "DH 반영", false, 0, false, out bTook, out w)) Leave(bTook);   // 잠금을 새로 잡으면 EnterLock 이 반영한다
                }
                Notify();
            }
            /// <summary>잠금 잡기 — 기다림은 LockWaitMs (EnterLock)</summary>
            private bool Enter(string strOwner, string strName, bool bNotify, out bool bTook, out string strWhy)
            {
                return EnterLock(strOwner, strName, bNotify, LockWaitMs, false, out bTook, out strWhy);
            }
            /// <summary>잠금 잡기. strOwner null = 소유자 없는 짧은 동작 (strName 으로 보인다, 재진입 안 됨), 아니면 그 소유자 (같은 소유자는 재진입 — bTook = false).
            /// 사용 중이면: 잡은 쪽이 소유자 없는 동작이고 다른 스레드면 nWaitMs 까지 기다렸다가 다시 본다 (UI 스레드는 따라가기 읽기만, 최대 200ms).
            /// 이름 있는 소유자가 잡고 있으면 바로 거부. bYield (따라가기) = 기다리는 쪽이 있으면 잡지 않고, 스스로는 기다리지 않는다.
            /// 새로 잡으면 미뤄 둔 DH 리밋 반영을 하고, bNotify 면 StateChanged</summary>
            private bool EnterLock(string strOwner, string strName, bool bNotify, int nWaitMs, bool bYield, out bool bTook, out string strWhy)
            {
                bTook = false;
                strWhy = "";
                int nMe = Thread.CurrentThread.ManagedThreadId;
                int nStart = Environment.TickCount;
                bool bWaited = false;
                lock (m_oSync)
                {
                    while (true)
                    {
                        if (bYield && m_nWaiters > 0) { strWhy = "기다리는 동작 있음"; return false; }
                        if (m_strOwner == null)
                        {
                            m_strOwner = (strOwner == null) ? strName : strOwner;
                            m_bOwnerAnon = (strOwner == null);
                            m_nOwnerThread = nMe;
                            bTook = true;
                            break;
                        }
                        if (strOwner != null && m_strOwner == strOwner && !m_bOwnerAnon) break;   // 같은 소유자 — 재진입
                        int nLeft = 0;
                        if (!bYield && nWaitMs > 0 && m_bOwnerAnon && CanWaitHere())
                        {
                            int nCap = IsUiThread() ? Math.Min(nWaitMs, FOLLOW_WAIT_UI_MS) : nWaitMs;
                            nLeft = nCap - unchecked(Environment.TickCount - nStart);
                        }
                        if (nLeft <= 0)
                        {
                            strWhy = "사용 중: " + m_strOwner + (bWaited ? " (" + unchecked(Environment.TickCount - nStart) + "ms 기다림)" : "");
                            return false;
                        }
                        m_nWaiters++;
                        bWaited = true;
                        try { Monitor.Wait(m_oSync, nLeft); }
                        finally { m_nWaiters--; }
                    }
                }
                // 미뤄 둔 DH 리밋 반영 — 잠금을 새로 잡은 순간에만 (같은 소유자가 다시 잡는 작업 중간에는 바꾸지 않는다)
                if (bTook && m_bRebuildPending) ReapplyPendingLocked();
                if (bTook && bNotify) Notify();
                return true;
            }
            private void Leave(bool bTook)
            {
                if (!bTook) return;
                lock (m_oSync) { m_strOwner = null; m_bOwnerAnon = false; m_nOwnerThread = 0; Monitor.PulseAll(m_oSync); }
            }
            /// <summary>지금 잠금이 따라가기 읽기인가 (m_oSync 안에서 부른다)</summary>
            private bool IsFollowHeld() { return m_bOwnerAnon && m_strOwner == FOLLOW_NAME; }
            /// <summary>이 스레드가 지금의 소유자 없는 동작을 기다릴 수 있나 (m_oSync 안에서 부른다) — 같은 스레드면 안 된다 (DoEvents 안에서 다시 불린 경우:
            /// 기다리면 스스로 막힌다). UI 스레드는 따라가기 읽기만 기다린다 — 다른 동작은 작업 스레드가 잡은 채 로그를 UI 로 보내므로(Invoke) 서로 멈춘다</summary>
            private bool CanWaitHere()
            {
                if (m_nOwnerThread == Thread.CurrentThread.ManagedThreadId) return false;
                return !IsUiThread() || IsFollowHeld();
            }
            /// <summary>UI 스레드인가 — WinForms(또는 WPF) 동기화 컨텍스트가 걸린 스레드 (컨트롤을 만든 스레드). 따라가기 읽기만 FOLLOW_WAIT_UI_MS 까지 기다린다</summary>
            private static bool IsUiThread()
            {
                SynchronizationContext ctx = SynchronizationContext.Current;
                return ctx != null && ctx.GetType() != typeof(SynchronizationContext);
            }
            private void Notify()
            {
                EventHandler h = StateChanged;
                if (h == null) return;
                try { h(this, EventArgs.Empty); }
                catch (Exception ex) { Log("상태 알림 처리 오류: " + ex.Message); }
            }
            private void Log(string s)
            {
                CSim2Real.DLog lg = Logger;
                if (lg != null) { try { lg(s); } catch { } return; }
                try { Ojw.printf("{0}", "[로봇팔] " + s + "\r\n"); } catch { }
            }
            private static CArmMotorState_t[] CloneStates(CArmMotorState_t[] a)
            {
                if (a == null) return null;
                CArmMotorState_t[] b = new CArmMotorState_t[a.Length];
                for (int i = 0; i < a.Length; i++) b[i] = (a[i] == null) ? null : a[i].Clone();
                return b;
            }

            // ── 입력 해석 ──
            /// <summary>"COM8"/"com8"/" 8 "/"COM 12" → 8/8/8/12. 범위 1~256</summary>
            public static bool ParsePort(string s, out int nPort)
            {
                nPort = 0;
                if (s == null) return false;
                string t = s.Trim().ToUpperInvariant();
                if (t.StartsWith("COM", StringComparison.Ordinal)) t = t.Substring(3).Trim();
                int n;
                if (!int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) return false;
                if (n < 1 || n > 256) return false;
                nPort = n;
                return true;
            }
            private static readonly int[] s_anStdBaud = { 9600, 57600, 115200, 1000000, 2000000, 3000000, 4000000, 4500000 };
            /// <summary>통신 속도 — 양의 정수. 표준(9600·57600·115200·1M·2M·3M·4M·4.5M) 밖이면 경고만</summary>
            public static bool ParseBaud(string s, out int nBaud, out string strWarn)
            {
                nBaud = 0;
                strWarn = "";
                if (s == null) return false;
                string t = s.Trim().Replace(",", "").Replace("_", "").Replace(" ", "");
                int n;
                if (!int.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out n) || n <= 0) return false;
                if (Array.IndexOf(s_anStdBaud, n) < 0) strWarn = "표준 통신 속도가 아님 (" + n + ") — 서보 설정과 같은지 확인";
                nBaud = n;
                return true;
            }
            private static string PortList(string[] astr)
            {
                if (astr == null || astr.Length == 0) return "(없음)";
                List<string> lst = new List<string>(astr);
                lst.Sort(delegate(string a, string b)
                {
                    int x, y;
                    bool bx = ParsePort(a, out x), by = ParsePort(b, out y);
                    if (bx && by) return x.CompareTo(y);
                    return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
                });
                return string.Join(", ", lst.ToArray());
            }

            // ── 세팅 적용 ──
            /// <summary>세트 적용: 검증 → 리밋 교집합(수동 ∩ EDH — 겹치지 않는 축이 있거나 그 리밋의 틱이 [1, 4094] 밖이면 거부) → C3d 헤더 투영 → 연결 중이면 CScene_t.SetMotors
            /// (실물은 움직이지 않고 3D 를 새 해석으로 옮김 — 헤더를 먼저 바꿔 3D 가 새 리밋으로 잘리게. 장면이 거부하면 헤더를 되돌리고 거부) → 적용된 세트로 기억.
            /// 거부하면 false 와 사유 — 적용된 세트·헤더·장면은 이전 그대로.
            /// 받아들였는데 경고가 있으면 true 와 경고 (남아 있던 보정만큼 3D 이동, 처짐 보정·바닥 모델이 있는데 매핑이 바뀜 — U3).
            /// 연결된 모터와 축 구성이 다른 세트면 장면은 건드리지 않고 ConfigMismatch (다시 연결할 때까지 움직이는 기능 막힘)</summary>
            public bool ApplyMotors(CArmMotorSet_t set, string strOwner, out string strWhy)
            {
                strWhy = "";
                if (set == null) { strWhy = "세팅 없음"; return false; }
                bool bTook;
                if (!Enter(strOwner, "세팅 적용", true, out bTook, out strWhy)) return false;
                try { return ApplyMotorsCore(set, out strWhy); }
                catch (Exception ex) { strWhy = "세팅 적용 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }
            private bool ApplyMotorsCore(CArmMotorSet_t setIn, out string strWhy)
            {
                strWhy = "";
                string w;
                CArmMotorSet_t setChk = setIn.Clone();
                setChk.ClearEffective();                    // 세트에 남아 있던 지난 교집합(옛 DH 기준일 수 있다)은 보지 않는다 — 쓰이는 리밋은 아래에서 지금 EDH 로 다시 구해 본다
                if (!setChk.Validate(out w)) { strWhy = "세팅 검증 실패:\r\n" + w; return false; }
                // 지금 EDH 로 리밋을 다시 구하니 미뤄 둔 DH 반영은 이것으로 대신한다 (먼저 내린다 — 거부되면 되돌린다)
                bool bPendingWas = m_bRebuildPending;
                m_bRebuildPending = false;
                bool bOk = false;
                try { bOk = ApplyMotorsCore2(setIn, out strWhy); }
                finally { if (!bOk && bPendingWas) m_bRebuildPending = true; }
                return bOk;
            }
            private bool ApplyMotorsCore2(CArmMotorSet_t setIn, out string strWhy)
            {
                strWhy = "";
                string w = "";
                CArmMotorSet_t set = setIn.Clone();
                set.ComputeEffective(m_C3d);
                // 수동 리밋과 EDH $jointrange 가 겹치지 않는 축 — 어느 쪽을 골라도 다른 쪽 경계를 넘을 수 있으니 받지 않는다
                if (set.HasLimitConflict) { strWhy = "리밋이 겹치지 않아 적용하지 않음 (지금 세팅 그대로):\r\n" + set.LimitConflict; return false; }
                // 쓰이는 리밋(EDH 포함)이 서보 틱 [1, 4094] 밖 — 읽기가 틱 0·-1 을 무응답으로 보고 위치 모드는 0 ↔ 4095 에서 감긴다
                if (!set.Validate(out w)) { strWhy = "리밋이 서보 틱 범위 밖이라 적용하지 않음 (지금 세팅 그대로 — 영점·방향·리밋·$jointrange 확인):\r\n" + w; return false; }
                w = "";
                List<string> lstWarn = new List<string>();
                CArmMotorSet_t old = m_Motors;
                CScene_t sc = m_Scene;
                bool bScene = false, bRemap = false;
                if (sc != null && sc.IsConnected)
                {
                    int[] ids = sc.IDs;
                    if (set.Signature == CArmMotorSet_t.SignatureOf(ids))
                    {
                        bScene = true;
                        for (int i = 0; i < ids.Length && !bRemap; i++)
                        {
                            CArmMotor_t a = (old == null) ? null : old.Get(ids[i]);
                            if (a == null || !a.SameMapping(set.Get(ids[i]))) bRemap = true;
                        }
                    }
                    else lstWarn.Add("연결된 모터(ID " + CArmMotorSet_t.RangeText(ids) + ")와 축 구성이 다른 세트 — 다시 연결할 때까지 움직이는 기능은 막힘");
                }
                // 헤더(3D 리밋)를 먼저 — 장면이 3D 를 새 해석으로 옮길 때(SetData) 옛 리밋으로 잘리지 않게. 장면이 거부하면 헤더를 되돌린다.
                // 적용하는 동안은 DH 반영을 미루며 헤더를 지금 세트로 되돌리는 일(ReapplyAfterRebuild)이 끼어들지 않게 표시해 두고,
                // 헤더 리밋 붙잡기 훅도 새 세트 값을 주게 한다 (그사이 DH 해석이 끼어들어도 헤더 = 새 세트)
                int[] anHdrAx;
                SMotorInfo_t[] aHdrSave;
                CArmMotorSet_t hdrPrev;
                lock (m_oHdr)
                {
                    m_bHdrApplying = true;
                    hdrPrev = m_HdrSet;
                    aHdrSave = SaveHeader(set, old, out anHdrAx);
                    set.ApplyToHeader(m_C3d, old);
                    m_HdrSet = set;
                }
                bool bOk = !bScene;
                try { if (bScene) bOk = sc.SetMotors(set, out w); }
                finally
                {
                    lock (m_oHdr)
                    {
                        if (bOk) m_Motors = set; else { RestoreHeader(anHdrAx, aHdrSave); m_HdrSet = hdrPrev; }
                        m_bHdrApplying = false;
                    }
                }
                if (!bOk) { strWhy = w; return false; }
                if (w.Length > 0) lstWarn.Add(w);
                m_strRebuildBlock = null;            // 지금 EDH 로 구한 리밋을 장면이 받아들였다
                // U3 — 같은 로봇에서 매핑(방향·영점·틱/회전·각/회전)이 바뀌었는데 처짐 보정·바닥 모델이 있으면 다시 학습 권장
                if (old != null && old.Signature == set.Signature && m_C3d.SagComp_Get() != null)
                {
                    string strDiff = CArmMotorSet_t.DescribeMappingDiff(old.MappingKey(), set.MappingKey());
                    if (strDiff.Length > 0) lstWarn.Add(CArmMotorSet_t.RetrainWarnText + " (" + strDiff + ")");
                }
                if (bRemap && sc != null)
                {
                    // 읽어 둔 틱은 그대로, 3D 각만 새 해석으로 다시 계산
                    lock (m_oSync)
                    {
                        if (m_aState != null)
                        {
                            int[] ids = sc.IDs;
                            for (int i = 0; i < m_aState.Length; i++)
                            {
                                CArmMotorState_t s = m_aState[i];
                                int k = (s == null) ? -1 : Array.IndexOf(ids, s.nAxis);
                                if (k < 0 || !s.bTickOk) continue;
                                s.fSim = sc.RawToSim(k, s.nTick);
                                CArmMotor_t m = set.Get(s.nAxis);
                                s.bOutside = (m != null) && m.IsOutside(s.fSim, OutsideTolDeg);
                            }
                        }
                    }
                }
                strWhy = string.Join("\r\n", lstWarn.ToArray());
                return true;
            }

            /// <summary>DH 를 다시 만든 뒤 (MakeAll, UI 스레드) — 버스를 쓰지 않는다. 축 서명이 적용된 세트와 같으면 리밋 교집합(수동 ∩ 새 $jointrange)을
            /// 다시 구해 장면(실물로 보낼 때 자르는 리밋)과 헤더(3D 리밋)에 한 번에 반영한다 (매핑은 그대로라 리밋만 바뀐다). 다르면 그대로 두고,
            /// 연결 중이면 ConfigMismatch 가 된다 (앱이 새 로봇의 세트를 불러 ApplyMotors 한다).
            /// 잠금이 잡혀 있거나(측정·조그 등) 3D 모션 중이면 반영을 미룬다 — 잠금을 새로 잡을 때·놓을 때 한다.
            /// 연결 중에는 DH 해석(ApplyEdhMotorLimits — MakeAll·URDF 내보내기) 자체가 연결된 축의 헤더 리밋을 바꾸지 못한다 (C3d 헤더 리밋 붙잡기 훅 —
            /// 새 $jointrange 는 기록만 된다). 그래서 이 함수가 불리기 전에도, 미루는 동안에도 작업·모션이 SetData 로 움직이는 축은 새 리밋으로
            /// 잘리지 않고 장면도 같은 리밋으로 보낸다 (작업 중 한 틱에 튀지 않는다). 미룰 때 헤더를 지금 세팅으로 다시 투영하는 것은 다른 경로로
            /// 헤더가 바뀐 경우를 위한 덧댐일 뿐이다. 실물이 없으면 훅이 아무 축도 붙잡지 않아 헤더는 DH 해석 값 그대로다 (예전처럼).
            /// 새 리밋을 반영할 수 없으면(수동 리밋과 새 $jointrange 가 겹치지 않음, 쓰이는 리밋이 서보 틱 [1, 4094] 밖, 또는 토크 ON 이고 실물이
            /// 붙잡은 자세·걸린 목표가 새 리밋 밖) 장면·세팅·헤더는 그대로 두고 로그를 남기며, 풀릴 때까지(리밋 고치기·토크 끄기) 움직이는 동작을 막는다</summary>
            public void ReapplyAfterRebuild(int[] anAxesNow)
            {
                string strSig = CArmMotorSet_t.SignatureOf(anAxesNow);
                lock (m_oSync) m_strAxesNowSig = strSig;
                m_bRebuildPending = true;
                bool bTook;
                string w;
                if (!m_C3d.MoveJoints_IsMoving() && EnterLock(null, "DH 반영", false, 0, false, out bTook, out w)) Leave(bTook);   // 비어 있으면 EnterLock 이 바로 반영
                else
                {
                    CScene_t scNow = m_Scene;
                    bool bHold = scNow != null && scNow.IsConnected;
                    if (bHold) HoldHeaderToApplied(strSig);   // 실물이 없으면 3D 리밋은 DH 해석 값 그대로 (예전처럼)
                    string strHolder;
                    lock (m_oSync) strHolder = m_strOwner;
                    if (strHolder != FOLLOW_NAME && !m_bRebuildDeferLogged)   // 따라가기 읽기는 곧 끝난다 — 다음에 잡을 때 반영 (로그 없음)
                    {
                        m_bRebuildDeferLogged = true;
                        Log("DH 다시 만듦 — 사용 중(" + ((strHolder != null) ? strHolder : "3D 모션") + ")이라 리밋 반영은 끝난 뒤로 미룸"
                            + (bHold ? " (그동안 3D 리밋도 지금 세팅 그대로)" : ""));
                    }
                }
                Notify();
            }
            /// <summary>헤더(3D 리밋 등)를 지금 적용된 세트로 다시 투영한다 — 축 서명이 같고 ApplyMotors 가 헤더를 바꾸는 중이 아닐 때만 (버스를 쓰지 않는다)</summary>
            private void HoldHeaderToApplied(string strSig)
            {
                lock (m_oHdr)
                {
                    if (m_bHdrApplying) return;
                    CArmMotorSet_t cur = m_Motors;
                    if (cur != null && strSig != null && cur.Signature == strSig) { cur.ApplyToHeader(m_C3d, null); m_HdrSet = cur; }
                }
            }
            /// <summary>미뤄 둔 DH 리밋 반영 — 잠금을 잡은 채 부른다 (버스를 쓰지 않는다). 3D 모션 중이면 그대로 미룬다.
            /// 반영할 수 없으면(수동 리밋과 새 $jointrange 가 겹치지 않음, 쓰이는 리밋이 서보 틱 [1, 4094] 밖, 또는 장면이 거부 — 토크 ON 이고
            /// 붙잡은 자세·걸린 목표가 새 리밋 밖) 장면·세팅은 그대로, 헤더도 지금 세팅으로 되돌리고 사유를 남긴다 — EnsureSynced 가 그동안
            /// 움직이는 동작을 막고, 다음에 잠금을 잡을 때(토크를 끈 뒤 등) 다시 해 본다. 리밋을 고친 세트를 ApplyMotors 하면 풀린다.
            /// 반영할 때는 장면 리밋과 헤더 리밋을 헤더 잠금 안에서 한 번에 바꾼다 (그사이 DH 해석이 끼어들어 둘이 어긋나지 않게)</summary>
            private void ReapplyPendingLocked()
            {
                if (!m_bRebuildPending || m_C3d.MoveJoints_IsMoving()) return;
                m_bRebuildPending = false;                  // 먼저 내린다 — 계산하는 사이 DH 가 또 바뀌면 다시 선다
                m_bRebuildDeferLogged = false;
                try { ReapplyPendingCore(); }
                catch (Exception ex) { m_bRebuildPending = true; Log("DH 다시 만든 뒤 리밋 반영 오류: " + ex.Message); }
            }
            private void ReapplyPendingCore()
            {
                string strSig;
                lock (m_oSync) strSig = m_strAxesNowSig;
                CArmMotorSet_t set = m_Motors;
                if (set == null || strSig == null || set.Signature != strSig) { m_strRebuildBlock = null; return; }
                CArmMotorSet_t s2 = set.Clone();
                s2.ComputeEffective(m_C3d);
                string strBlock = null, w;
                if (s2.HasLimitConflict) strBlock = "DH 리밋을 반영하지 못함 — " + s2.LimitConflict.Replace("\r\n", " / ");
                else if (!s2.Validate(out w)) strBlock = "DH 리밋을 반영하지 못함 — " + w.Replace("\r\n", " / ");   // 쓰이는 리밋이 서보 틱 [1, 4094] 밖 등
                else
                {
                    CScene_t sc = m_Scene;
                    bool bScene = sc != null && sc.IsConnected && CArmMotorSet_t.SignatureOf(sc.IDs) == strSig;
                    if (bScene && !sc.SameMappingAs(s2)) strBlock = "DH 리밋을 반영하지 못함 — 연결된 장면의 매핑이 적용된 세팅과 다름 (다시 연결하십시오)";
                    else
                    {
                        // 장면(실물로 보낼 때 자르는 리밋)과 헤더(3D 리밋)를 헤더 잠금 안에서 한 번에 — 그사이 DH 해석이 끼어들어 둘이 어긋나지 않게
                        // (모션 중에는 여기 오지 않는다). 매핑이 같아 장면은 리밋만 바꿔 끼운다 — 버스·로그 없음이라 잠금 안에서 불러도 된다
                        lock (m_oHdr)
                        {
                            w = "";
                            if (!bScene || sc.SetMotors(s2, out w)) { s2.ApplyToHeader(m_C3d, set); m_Motors = s2; m_HdrSet = s2; }
                            else strBlock = "DH 리밋을 반영하지 못함 — " + w;
                        }
                    }
                }
                if (strBlock != null)
                {
                    // 장면·세팅은 그대로 — 헤더(3D 리밋)도 지금 세팅으로 되돌려 장면과 같은 리밋을 보게 한다
                    HoldHeaderToApplied(strSig);
                    if (strBlock != m_strRebuildBlock) Log(strBlock + " — 그동안 움직이는 동작은 막힘");
                    m_strRebuildBlock = strBlock;
                    m_bRebuildPending = true;           // 다음에 잠금을 잡을 때 다시 해 본다 (토크를 끈 뒤 등)
                    return;
                }
                m_strRebuildBlock = null;
            }

            // ── C3d 헤더 리밋 붙잡기 (연결 중 DH 해석이 연결된 축의 3D 리밋을 바꾸지 못하게) ──
            /// <summary>헤더 리밋 붙잡기 훅 본체 — C3d.ApplyEdhMotorLimits 가 헤더 잠금 안에서 축마다 부른다. 연결 중이고 이 연결의 장면이 그 축을 쓰면
            /// true 와 헤더에 둘 리밋(헤더에 투영해 둔 세트의 쓰이는 리밋, 헤더 규약)을 준다 — 장면이 실물로 보낼 때 자르는 리밋과 같다.
            /// 아니면 false (DH 해석이 예전처럼 쓴다). 버스·로그·UI 기다림 없음</summary>
            private bool HoldHeaderLimit(int nAxis, out float fLo, out float fHi)
            {
                fLo = 0f;
                fHi = 0f;
                try
                {
                    lock (m_oHdr)
                    {
                        CScene_t sc = m_Scene;
                        if (sc == null || !sc.IsConnected || Array.IndexOf(sc.IDs, nAxis) < 0) return false;   // 연결 안 됨·이 연결이 쓰지 않는 축
                        CArmMotorSet_t set = m_HdrSet;
                        CArmMotor_t m = (set == null) ? null : set.Get(nAxis);
                        if (m == null) return false;
                        if (CArmMotorSet_t.HeaderLimits(m, out fLo, out fHi)) return true;
                        // 쓰이는 리밋이 빈 축 (ApplyToHeader 도 헤더 리밋을 건드리지 않는다) — 지금 헤더 값 그대로
                        SMotorInfo_t[] a = m_C3d.m_CHeader.pSMotorInfo;
                        fLo = a[nAxis].fLimit_Down;
                        fHi = a[nAxis].fLimit_Up;
                        return true;
                    }
                }
                catch { return false; }
            }
            /// <summary>C3d 의 헤더 리밋 붙잡기 훅을 이 연결 객체로 — bForce(연결할 때)면 늘, 아니면(생성할 때) 자리가 비었거나
            /// 그 주인(다른 연결 객체)이 연결돼 있지 않을 때만 (실물을 쓰는 다른 연결의 훅은 빼앗지 않는다)</summary>
            private void HookHeaderLimits(bool bForce)
            {
                if (m_C3d == null) return;
                lock (m_oHdr)
                {
                    C3d.DEdhLimitHold cur = m_C3d.EdhLimit_GetHold();
                    if (cur == m_fnHold) return;
                    if (!bForce && cur != null)
                    {
                        CArmConn_t o = cur.Target as CArmConn_t;
                        if (o == null || o.IsConnected) return;
                    }
                    m_C3d.EdhLimit_SetHold(m_fnHold);
                }
            }
            /// <summary>헤더 리밋 붙잡기 훅이 이 연결 객체의 것이면 뗀다 (연결 해제)</summary>
            private void UnhookHeaderLimits()
            {
                if (m_C3d == null) return;
                lock (m_oHdr) { if (m_C3d.EdhLimit_GetHold() == m_fnHold) m_C3d.EdhLimit_SetHold(null); }
            }
            /// <summary>연결된 축의 헤더 리밋이 투영해 둔 세트(적용된 리밋 = 장면이 자르는 리밋)와 다르면 다시 쓴다 — 훅을 거치지 않고 헤더가 바뀐 경우
            /// (디자이너의 리밋 칸, 훅을 걸기 전의 DH 해석 등). 바꾼 축 문구 ("" = 같음). 로그는 부른 쪽이 헤더 잠금 밖에서 남긴다</summary>
            private string HoldHeaderLimitsNow()
            {
                if (m_C3d == null) return "";
                lock (m_oHdr)
                {
                    if (m_bHdrApplying) return "";
                    CScene_t sc = m_Scene;
                    CArmMotorSet_t set = m_HdrSet;
                    if (sc == null || !sc.IsConnected || set == null || m_C3d.m_CHeader == null || m_C3d.m_CHeader.pSMotorInfo == null) return "";
                    SMotorInfo_t[] a = m_C3d.m_CHeader.pSMotorInfo;
                    List<string> lst = new List<string>();
                    foreach (int ax in sc.IDs)
                    {
                        CArmMotor_t m = set.Get(ax);
                        float fLo, fHi;
                        if (m == null || ax < 0 || ax >= a.Length || !CArmMotorSet_t.HeaderLimits(m, out fLo, out fHi)) continue;
                        if (a[ax].fLimit_Down == fLo && a[ax].fLimit_Up == fHi) continue;
                        lst.Add("ID " + ax + " " + HdrText(a[ax].fLimit_Down, a[ax].fLimit_Up) + " → " + HdrText(fLo, fHi));
                        a[ax].fLimit_Down = fLo;
                        a[ax].fLimit_Up = fHi;
                    }
                    return string.Join(", ", lst.ToArray());
                }
            }
            private static string HdrText(float fLo, float fHi)
            {
                return ((fLo == 0f) ? "없음" : CArmMotorSet_t.Fmt(fLo)) + "~" + ((fHi == 0f) ? "없음" : CArmMotorSet_t.Fmt(fHi)) + "°";
            }
            /// <summary>헤더(pSMotorInfo)에서 두 세트의 축 원소를 복사해 둔다 (SMotorInfo_t 는 struct — 값 복사). 헤더가 없으면 null</summary>
            private SMotorInfo_t[] SaveHeader(CArmMotorSet_t a, CArmMotorSet_t b, out int[] anAx)
            {
                anAx = new int[0];
                if (m_C3d == null || m_C3d.m_CHeader == null || m_C3d.m_CHeader.pSMotorInfo == null) return null;
                SMotorInfo_t[] hdr = m_C3d.m_CHeader.pSMotorInfo;
                List<int> lst = new List<int>();
                foreach (CArmMotorSet_t s in new CArmMotorSet_t[] { a, b })
                {
                    if (s == null) continue;
                    foreach (CArmMotor_t m in s.Motors)
                        if (m != null && m.nAxis >= 0 && m.nAxis < hdr.Length && !lst.Contains(m.nAxis)) lst.Add(m.nAxis);
                }
                anAx = lst.ToArray();
                SMotorInfo_t[] aSave = new SMotorInfo_t[anAx.Length];
                for (int i = 0; i < anAx.Length; i++) aSave[i] = hdr[anAx[i]];
                return aSave;
            }
            private void RestoreHeader(int[] anAx, SMotorInfo_t[] aSave)
            {
                if (aSave == null || anAx == null || m_C3d.m_CHeader == null || m_C3d.m_CHeader.pSMotorInfo == null) return;
                SMotorInfo_t[] hdr = m_C3d.m_CHeader.pSMotorInfo;
                for (int i = 0; i < anAx.Length && i < aSave.Length; i++) if (anAx[i] >= 0 && anAx[i] < hdr.Length) hdr[anAx[i]] = aSave[i];
            }

            // ── 연결 ──
            /// <summary>연결 (작업 스레드, 블로킹) — 포트 확인 → CScene_t 생성(싱크 등록) → 세팅 붙이기 → 열기 → 모델번호 응답 확인
            /// (전부 무응답이면 닫고 거부, 일부만이면 연결 유지) → 레지스터 읽기(경고) → 토크는 켜지 않는다.
            /// 단 서보가 이미 토크 ON 이면(연결 해제 때 토크를 유지했던 경우 등) 그 상태를 이어받는다 — 버스에 쓰지 않고, 서보가 붙잡은 목표로
            /// 3D 를 맞춘 뒤 토크 ON 으로 연결 (IsTorqueOn = true, 끝낼 때 토크를 끄지 않게).
            /// 세팅이 적용돼 있지 않으면 기본값을 적용하고, 적용된 세트의 축이 anAxes 와 다르면 거부한다. strReport = 보고(성공) 또는 사유(실패)</summary>
            public bool Connect(int[] anAxes, int nPort, int nBaud, out string strReport)
            {
                strReport = "";
                if (anAxes == null || anAxes.Length == 0) { strReport = "연결할 축이 없음 (EDH 에 모터 없음)"; return false; }
                if (IsConnected) { strReport = "이미 연결됨 (COM" + m_nPort + ")"; return false; }
                bool bTook;
                if (!Enter(null, "연결", true, out bTook, out strReport)) return false;
                CScene_t scNew = null;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strReport = "3D 모션 중 — 끝난 뒤 다시"; return false; }
                    CScene_t scOld = m_Scene;
                    if (scOld != null) { scOld.Release(); m_Scene = null; }   // 밖에서 닫힌 옛 장면 — 싱크를 되돌리고 버린다
                    string[] astrPorts = System.IO.Ports.SerialPort.GetPortNames();
                    string strCom = "COM" + nPort.ToString(CultureInfo.InvariantCulture);
                    if (Array.FindIndex(astrPorts, delegate(string s) { return s.Equals(strCom, StringComparison.OrdinalIgnoreCase); }) < 0)
                    { strReport = strCom + " 없음 — 지금 포트: " + PortList(astrPorts); return false; }
                    List<string> lst = new List<string>();
                    string strSig = CArmMotorSet_t.SignatureOf(anAxes);
                    CArmMotorSet_t set = m_Motors;
                    bool bDefault = false;
                    if (set == null)
                    {
                        // 세팅을 적용한 적이 없으면 기본값으로 — 헤더·적용 세트는 연결에 성공한 뒤에 바꾼다
                        set = CArmMotorSet_t.CreateDefault(anAxes, "");
                        set.ComputeEffective(m_C3d);
                        if (set.HasLimitConflict) { strReport = "기본 세팅의 리밋을 정하지 못함:\r\n" + set.LimitConflict; return false; }
                        string wv;
                        if (!set.Validate(out wv)) { strReport = "기본 세팅의 리밋이 서보 틱 범위 밖 ($jointrange 확인):\r\n" + wv; return false; }
                        bDefault = true;
                        lst.Add("모터 세팅이 적용돼 있지 않아 기본값(방향 정·영점 2048)으로 연결");
                    }
                    else if (set.Signature != strSig)
                    {
                        strReport = "적용된 모터 세팅(ID " + CArmMotorSet_t.RangeText(set.Axes) + ")이 연결할 축(ID " + CArmMotorSet_t.RangeText(anAxes) + ")과 다름 — 세팅을 먼저 적용하십시오";
                        return false;
                    }
                    int[] anOne = new int[anAxes.Length];
                    for (int i = 0; i < anOne.Length; i++) anOne[i] = 1;
                    string w;
                    scNew = new CScene_t(m_C3d, anAxes, anOne);           // 이때 모션 싱크가 등록된다 — 실패하면 Release 가 이전 싱크로 되돌린다
                    if (!scNew.SetMotors(set, out w)) { strReport = "모터 세팅을 붙이지 못함: " + w; return false; }
                    scNew.open(nPort, nBaud);
                    if (!scNew.IsConnected) { strReport = strCom + " 열기 실패 — 다른 프로그램(Dynamixel Wizard, 다른 MakeUrdf 등)이 쓰고 있지 않은지"; return false; }
                    Dictionary<int, long> dModel = scNew.ReadReg(0, 2, out w);
                    if (dModel == null || dModel.Count == 0) { strReport = strCom + " @ " + nBaud + " 응답 없음 — 통신 속도·전원·ID 확인"; return false; }
                    if (bDefault) { lock (m_oHdr) { set.ApplyToHeader(m_C3d, null); m_Motors = set; m_HdrSet = set; } }
                    m_Scene = scNew;
                    m_nPort = nPort;
                    m_nBaud = nBaud;
                    m_bUnsynced = false;
                    lock (m_oSync) m_strAxesNowSig = strSig;
                    // 이제부터 DH 해석(MakeAll·URDF 내보내기)은 연결된 축의 3D 리밋(헤더)을 바꾸지 못한다 — 헤더 = 장면이 자르는 리밋
                    HookHeaderLimits(true);
                    string strHdr = HoldHeaderLimitsNow();
                    if (strHdr.Length > 0) lst.Add("3D 리밋(헤더)을 적용된 세팅으로 맞춤 (" + strHdr + ")");

                    int[] ids = scNew.IDs;
                    List<int> lstMiss = new List<int>();
                    foreach (int id in ids) if (!dModel.ContainsKey(id)) lstMiss.Add(id);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} @ {1} 연결 — 응답 {2}/{3} (ID {4})\r\n", strCom, nBaud, ids.Length - lstMiss.Count, ids.Length, CArmMotorSet_t.RangeText(ids));
                    if (lstMiss.Count > 0) sb.Append("무응답: ID " + CArmMotorSet_t.RangeText(lstMiss.ToArray()) + " — 그 행은 빨강, 움직이는 동작 전에 확인\r\n");
                    foreach (string s in lst) sb.Append(s).Append("\r\n");
                    CArmMotorState_t[] aSt;
                    string strAdopt = "";
                    try { aSt = ReadStateCore(scNew, true, out w, out strAdopt); }
                    catch (Exception exRead) { aSt = new CArmMotorState_t[0]; w = "상태 읽기 오류: " + exRead.Message; }   // 연결은 유지 — 표는 '읽기' 로 다시
                    foreach (CArmMotorState_t s in aSt)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "ID {0} {1}", s.nAxis, s.bResponded ? s.strModel : "-");
                        if (s.bTickOk) sb.AppendFormat(CultureInfo.InvariantCulture, " · 틱 {0} ({1:0.0}°)", s.nTick, s.fSim);
                        if (s.strNote.Length > 0) sb.Append(" · ").Append(s.strNote);
                        sb.Append("\r\n");
                    }
                    if (w.Length > 0) sb.Append(w).Append("\r\n");
                    if (strAdopt.Length > 0) sb.Append(strAdopt).Append("\r\n");
                    // 서보가 이미 토크 ON 이었으면(연결 해제 때 토크를 유지했던 경우 등) 그 상태를 이어받았다 — 어느 쪽이든 새로 켜지는 않는다
                    sb.Append(scNew.IsTorqueOn ? "토크를 새로 켜지는 않았습니다 — 이미 켜져 있던 상태를 이어받음 (토크 ON)" : "토크는 켜지 않았습니다 (연결 = 읽기만)");
                    strReport = sb.ToString();
                    Log(strCom + " 연결 — 응답 " + (ids.Length - lstMiss.Count) + "/" + ids.Length);
                    return true;
                }
                catch (Exception ex) { strReport = "연결 오류: " + ex.Message; return false; }
                finally
                {
                    if (scNew != null && m_Scene != scNew) scNew.Release();   // 실패 — 닫고 싱크 되돌림
                    Leave(bTook);
                    Notify();
                }
            }

            /// <summary>연결 해제 (작업 스레드) — 바쁘면(잠금·모션) false. bTorqueOff 면 토크를 끈 뒤(팔이 처질 수 있음 — 확인은 UI 에서),
            /// 아니면 토크를 켠 채(서보가 마지막 자세를 붙잡는다) 닫는다. 토크 끄기는 연결이 아는 상태와 상관없이 전 축에 보낸다
            /// (다른 경로로 켜진 서보도 끈다). 모션 싱크는 연결 전 것으로 되돌린다</summary>
            public bool Disconnect(bool bTorqueOff)
            {
                CScene_t sc = m_Scene;
                if (sc == null) return true;
                bool bTook;
                string w;
                if (!Enter(null, "연결 해제", true, out bTook, out w)) { Log("연결 해제 거부 — " + w); return false; }
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { Log("연결 해제 거부 — 3D 모션 중"); return false; }
                    if (bTorqueOff && sc.IsConnected) sc.torqoff();
                    sc.Release();
                    m_Scene = null;
                    UnhookHeaderLimits();                  // 실물이 없으니 DH 해석은 예전처럼 헤더 리밋을 쓴다
                    m_bUnsynced = false;
                    lock (m_oSync) m_aState = null;
                    ReapplyPendingLocked();                // 장면이 거부해 남아 있던 DH 리밋 반영 — 이제 장면이 없으니 세팅·헤더에만
                    Log("연결 해제" + (bTorqueOff ? " (토크 끔)" : " (토크 그대로)"));
                    return true;
                }
                catch (Exception ex) { Log("연결 해제 오류: " + ex.Message); return false; }
                finally { Leave(bTook); Notify(); }
            }

            /// <summary>다시 연결한 뒤 처짐 보정 되살리기 (2026-10-15 검토 — 앱이 포트를 적용 예제 실행기에 넘겼다가 같은 포트로 다시 연결할 때).
            /// 연결이 토크를 붙잡은 서보를 이어받으면 걸려 있던 처짐 보정을 페이드 없이 끈다 (AdoptTorque → C3d.SagComp_OffForAdopt, 보고에 ADOPT_COMP_OFF,
            /// 3D = 서보 목표). 넘기기 전에 켜져 있던 보정 mBefore 를 보통 켜기 길로 다시 켠다 (C3d.SagComp_Enable(true) — 끈 직후라 전환 기준 = 걸린 보정 0:
            /// 다음 출력 틱은 서보 목표 그대로이고, 보정은 다음 동작의 출력 틱 1초에 걸쳐 0 에서 들어온다 — ⑦ '처짐 보정 켬' 을 다시 누른 것과 같다).
            /// 이어받은 자세가 정해진 뒤에만 켠다: 연결됨 · 토크 ON · 구성 그대로 · 다시 맞출 것 없음 (NeedsSync 가 남았으면 움직이기 전 확인 EnsureSynced 가
            /// 서보 목표로 맞추며 보정을 다시 끈다) · 잠금·3D 모션 없음 (잠금을 잠깐 잡고 본다). mBefore 가 지금도 적용 중 모델이고 꺼져 있을 때만.
            /// 버스를 쓰지 않는다 (UI 스레드에서 불러도 된다). 반환 = 다시 켰다. strWhy = 켜지 않은 까닭 ("" = 켰거나 되살릴 것이 없음). 예외 없음</summary>
            public bool RestoreSagComp(C3d.CSagComp mBefore, out string strWhy)
            {
                strWhy = "";
                try
                {
                    if (mBefore == null || m_C3d == null) return false;                          // 넘기기 전에 켜진 보정이 없었다
                    if (!object.ReferenceEquals(m_C3d.SagComp_Get(), mBefore)) { strWhy = "그사이 적용 중 처짐 보정 모델이 바뀜"; return false; }
                    if (mBefore.Enabled) return false;                                           // 꺼지지 않았다 — 되살릴 것 없음
                    CScene_t sc = m_Scene;
                    if (sc == null || !sc.IsConnected) { strWhy = "연결 안 됨"; return false; }
                    if (!sc.IsTorqueOn) { strWhy = "토크 OFF — 이어받은 자세가 없음"; return false; }
                    if (ConfigMismatch) { strWhy = "모터 구성이 연결 뒤 바뀜 — 다시 연결"; return false; }
                    if (m_bUnsynced) { strWhy = "서보가 붙잡은 목표를 다 읽지 못함 — 움직이기 전 확인이 목표로 맞추며 보정을 끈다"; return false; }
                    bool bTook;
                    string w;
                    if (!EnterLock(null, "처짐 보정 되살리기", false, 0, false, out bTook, out w)) { strWhy = w; return false; }
                    try
                    {
                        if (m_C3d.MoveJoints_IsMoving()) { strWhy = "3D 모션 중"; return false; }
                        if (!object.ReferenceEquals(m_C3d.SagComp_Get(), mBefore) || mBefore.Enabled) return false;
                        m_C3d.SagComp_Enable(true);                                              // 보통 켜기 길 — 전환 기준 = 걸린 보정 (이어받기 뒤라 0)
                    }
                    finally { Leave(bTook); }
                    Log("처짐 보정 다시 켬 — 이어받기로 꺼졌던 보정 (지금 자세 그대로, 다음 동작의 출력 틱 1초에 걸쳐 0 에서 들어옴)");
                    Notify();
                    return true;
                }
                catch (Exception ex) { strWhy = "오류: " + ex.Message; return false; }
            }

            // ── 토크 ──
            /// <summary>토크 켜기 (작업 스레드). 거부는 미연결·잠금 실패(바쁨·모션 중)·ConfigMismatch 뿐 — 읽기 실패·리밋 밖·DH 리밋 반영 못 함은
            /// 경고만(strWhy) 돌려주고 켠다 (예전 torqon 그대로: 정렬 → 처짐 보정 방향 추적 리셋 → 토크 → Stream_Reset). 정렬을 못 했으면
            /// 움직이기 전에 EnsureSynced 가 실측으로 맞춘다. 켠 뒤 주소 64 로 확인 — 안 켜진 ID 는 경고만 (끄지 않는다).
            /// 이미 토크가 켜져 있던 서보(연결이 모르는 사이 켜진 경우)는 정렬한 지금 위치 대신 서보가 붙잡은 목표를 그대로 3D·붙잡은 자세로 이어받는다 —
            /// 지금 위치는 하중만큼 목표에서 처져 있어 그것을 보내면 그만큼 떨어진다 (첫 위치 읽기가 실패해도 토크 상태·목표는 따로 읽는다).
            /// 그 목표에는 지난 처짐 보정이 들어 있을 수 있다 — 처짐 보정이 걸려 있었으면(CScene_t.SagCompInPlay, 켜기 전에 본다) 되짚지 않고 보정을 페이드 없이
            /// 끈다 (C3d.SagComp_OffForAdopt, 10-03 설계 바꿈) → 다음 전송 틱 = 서보 목표 (튐 0), 그 뒤에도 보정이 얹히지 않는다. 끈 것은 strWhy 에 알리고
            /// (ADOPT_COMP_OFF) StateChanged 로 앱의 '처짐 보정 켬' 이 따라온다 — 다시 켜면 다음 동작의 출력 틱 1초에 걸쳐 들어온다.
            /// 보정이 걸려 있지 않았으면 예전 그대로 (목표 그대로, 바꿀 것 없음).
            /// 이미 켜진 서보나 토크 상태를 모르는 서보가 있으면 켜는 동안 붙잡은 자세를 모름으로 둔다 (CScene_t.TorqueOnUnheld — 그사이 다른 스레드의 3D 모션이
            /// 처진 지금 위치를 그 서보로 보내지 않게). 목표를 못 읽으면 보내지 않고 EnsureSynced 가 다시 읽어 맞춘다 (못 읽으면 움직이는 동작은 막힘).
            /// 연결은 토크 ON 으로 아는데 꺼진 축이 섞여 있으면(연결 때 이어받음, 전원이 잠깐 끊기거나 오류로 서보가 스스로 꺼짐) 그 축만 켠다 (TorqueOnRest)</summary>
            public bool TorqueOn(string strOwner, out string strWhy)
            {
                strWhy = "";
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) { strWhy = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                if (ConfigMismatch) { strWhy = "모터 구성이 연결 뒤 바뀜 — 다시 연결하십시오"; return false; }
                bool bTook;
                if (!Enter(strOwner, "토크", true, out bTook, out strWhy)) return false;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strWhy = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                    if (sc.IsTorqueOn) return TorqueOnRest(sc, out strWhy);
                    List<string> lstWarn = new List<string>();
                    string w;
                    int[] ids = sc.IDs;
                    int[] anTick = sc.ReadTicks(out w);
                    string strReadFail = (anTick == null) ? w : null;
                    List<int> lstWasOn = new List<int>();
                    float[] afGoal0 = null;
                    if (anTick != null)
                    {
                        CArmMotorSet_t set = m_Motors;
                        for (int i = 0; i < ids.Length; i++)
                        {
                            CArmMotor_t m = (set == null) ? null : set.Get(ids[i]);
                            float f = sc.RawToSim(i, anTick[i]);
                            if (m != null && m.IsOutside(f, OutsideTolDeg)) lstWarn.Add(m.OutsideText(f) + " — 움직이는 동작은 막힘");
                        }
                    }
                    // 이미 토크가 켜져 있는 서보 — 그 축은 켠 뒤 서보가 붙잡은 목표(Goal Position)를 이어받는다.
                    // 첫 위치 읽기가 실패했어도 따로 읽는다 — torqon 이 다시 읽어 정렬에 성공하면 그 축의 처진 지금 위치를 보내게 되므로
                    Dictionary<int, long> dTq0 = sc.ReadReg(64, 1, out w);
                    List<int> lstTqUnknown = new List<int>();
                    foreach (int id in ids)
                    {
                        long v;
                        if (dTq0 == null || !dTq0.TryGetValue(id, out v)) lstTqUnknown.Add(id);
                        else if (v == 1) lstWasOn.Add(id);
                    }
                    if (lstWasOn.Count > 0) afGoal0 = ReadGoalAll(sc);
                    sc.ForgetHold();                                    // 연결이 토크 OFF 로 알던 동안의 붙잡은 자세는 낡은 값 (켜기가 다시 정한다) — 아래 보정 판단에 넣지 않는다
                    bool bComp = sc.SagCompInPlay();                    // 켜기 전에 본다 — 켜기가 출력 단의 걸린 보정을 지운다 (모델 켬은 그대로)
                    // 이어받을 축이 있으면(이미 켜짐·토크 상태 모름) 붙잡은 자세를 모름으로 켠다 — 아래에서 서보 목표로 정하거나 움직이기 전 확인이 맞춘다
                    float[] afAligned;                                  // 켤 때 정렬한 실물 지금 자세 (읽기 실패면 null)
                    if (lstWasOn.Count > 0 || lstTqUnknown.Count > 0) afAligned = sc.TorqueOnUnheld();
                    else { sc.torqon(); afAligned = sc.HoldSimDeg; }
                    bool bForgot = false;
                    if (strReadFail != null)
                        lstWarn.Insert(0, (afAligned != null) ? "첫 위치 읽기 실패 — 토크 켤 때 다시 읽어 맞춤 (" + strReadFail + ")"
                                                              : "읽기 실패 — 3D 와 실물이 어긋났을 수 있음. 움직이기 전에 '읽기' (" + strReadFail + ")");
                    if (lstWasOn.Count > 0)
                    {
                        float[] afHeld = (afAligned != null) ? (float[])afAligned.Clone() : null;   // 켜져 있던 축 = 서보 목표, 꺼져 있다 켠 축 = 지금 위치
                        bool bGoal = afHeld != null && afGoal0 != null;
                        for (int i = 0; i < ids.Length && bGoal; i++)
                        {
                            if (!lstWasOn.Contains(ids[i])) continue;
                            if (!float.IsNaN(afGoal0[i])) afHeld[i] = afGoal0[i];
                            else bGoal = false;
                        }
                        string strWasOn = "ID " + CArmMotorSet_t.RangeText(lstWasOn.ToArray());
                        if (bGoal)
                        {
                            // 서보 목표 그대로 3D·붙잡은 자세로 (되짚지 않는다 — 목표에 보정이 들었는지 실물로는 가릴 수 없다, 10-03).
                            //   처짐 보정이 걸려 있었으면 페이드 없이 끈다 → 다음 전송 틱 = 서보 목표 (튐 0), 그 뒤에도 보정이 얹히지 않는다
                            if (bComp) m_C3d.SagComp_OffForAdopt();
                            sc.AdoptTorqueOnGoal(afHeld, afHeld);
                            lstWarn.Add(strWasOn + " 은 토크가 이미 켜져 있었음 — 그 축은 서보가 붙잡은 목표를 이어받음");
                            if (bComp) lstWarn.Add(ADOPT_COMP_OFF);
                        }
                        else if (afHeld != null)
                        {
                            // 켜져 있던 축의 목표를 못 읽음 — 정렬한 지금 위치는 하중만큼 처져 있어 그것을 보내면 떨어진다 (K5).
                            // 보내지 않고(붙잡은 자세 모름 그대로), 움직이기 전에 EnsureSynced 가 서보 목표를 다시 읽어 맞춘다 (그때도 못 읽으면 움직이는 동작은 막힘)
                            sc.ForgetHold();
                            bForgot = true;
                            lstWarn.Add(strWasOn + " 은 토크가 이미 켜져 있었는데 붙잡은 목표를 읽지 못함 — 실물로 보내지 않음. 움직이기 전에 서보 목표를 다시 읽어 맞춤 (못 읽으면 움직이는 동작은 막힘)");
                        }
                    }
                    if (lstTqUnknown.Count > 0 && afAligned != null)
                    {
                        // 토크 상태를 읽지 못한 축은 이미 켜져 있었을 수 있다 — 정렬한 지금 위치는 하중만큼 처져 있을 수 있으니 보내지 않고,
                        // 움직이기 전에 EnsureSynced 가 서보 목표로 맞춘다
                        sc.ForgetHold();
                        bForgot = true;
                        lstWarn.Add("ID " + CArmMotorSet_t.RangeText(lstTqUnknown.ToArray()) + " 토크 상태를 읽지 못함 — 이미 켜져 있었을 수 있어 실물로 보내지 않음. 움직이기 전에 서보 목표로 다시 맞춤");
                    }
                    m_bUnsynced = (sc.HoldSimDeg == null);
                    if (m_bUnsynced && anTick != null && !bForgot) lstWarn.Add("토크 켤 때 정렬 읽기 실패 — 움직이기 전에 다시 맞춘다");
                    Dictionary<int, long> dTq = sc.ReadReg(64, 1, out w);
                    List<int> lstOff = new List<int>();
                    foreach (int id in ids) { long v; if (dTq == null || !dTq.TryGetValue(id, out v) || v != 1) lstOff.Add(id); }
                    if (lstOff.Count > 0) lstWarn.Add("ID " + CArmMotorSet_t.RangeText(lstOff.ToArray()) + " 토크 확인 안 됨 (전원·배선 점검)");
                    string strBlock = m_strRebuildBlock;
                    if (strBlock != null) lstWarn.Add(strBlock + " — 움직이는 동작은 막힘");
                    strWhy = string.Join("\r\n", lstWarn.ToArray());
                    Log("토크 ON" + (lstWarn.Count > 0 ? " — 경고: " + strWhy.Replace("\r\n", " / ") : ""));
                    return true;
                }
                catch (Exception ex) { strWhy = "토크 켜기 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }
            /// <summary>토크 ON 으로 아는데 꺼진 축이 있으면(연결 때 일부 축만 토크 ON 이던 상태를 이어받음, 전원이 잠깐 끊기거나 오류로 서보가 스스로 꺼짐)
            /// 그 축만 켠다 — 거부하지 않는다 (U5).
            /// 켜는 축은 지금 위치(정착 읽기)를 3D·붙잡은 자세로 삼고, 이미 켜진 축은 붙잡은 자세·걸린 목표를 그대로 둔다 → 정렬 없이 바로 움직일 수 있다.
            /// 처짐 보정이 걸려 있거나 막 걸려 있었으면(CScene_t.SagCompInPlay — 모델 켬, 끈 뒤 페이드, 손목 유지, 걸린 보정) 이어받기와 같이 한다
            /// (10-03 설계 바꿈): 켜져 있던 축의 3D·붙잡은 자세 = 서보 목표 (주소 116 — 못 읽은 축은 마지막으로 보낸 목표), 켜는 축 = 지금 위치,
            /// 보정은 페이드 없이 끈다 (C3d.SagComp_OffForAdopt — strWhy 에 ADOPT_COMP_OFF) → 다음 전송 틱 = 켜져 있던 축은 서보 목표, 켜는 축은 지금 위치
            /// (튐 0), 그 뒤에도 보정이 얹히지 않는다. 다시 켜면 다음 동작의 출력 틱 1초에 걸쳐 들어온다.
            /// 전 축이 꺼져 있었으면(붙잡고 있던 서보 없음 — 이어받을 서보 목표가 없다) 토크 켜기와 같다: 지금 위치에서 켜고 보정은 끄지 않는다 —
            /// 0 부터 다음 동작의 출력 틱에 걸쳐 1초 동안 들어온다 (CScene_t.TorqueOnAxes 의 전환 씨앗).
            /// 토크 상태를 읽지 못한 축도 켠다 (켜진 서보에 다시 켜기는 해가 없다) — 다만 그 축은 켜져 있었을 수 있어(처진 지금 위치를 보내면 떨어진다)
            /// 붙잡은 자세를 모름으로 둔다. 위치를 못 읽어도 마찬가지 — 실물로 보내지 않고, 움직이기 전에 EnsureSynced 가 서보 목표로 맞춘다
            /// (처짐 보정이 걸려 있으면 그때 끄고 목표 그대로 — EnsureSyncedCore).
            /// 켠 뒤 주소 64 로 다시 확인 — 안 켜진 축은 경고만. 꺼진 축이 없으면 "이미 토크 ON" (쓰기 없음)</summary>
            private bool TorqueOnRest(CScene_t sc, out string strWhy)
            {
                strWhy = "";
                string w;
                int[] ids = sc.IDs;
                Dictionary<int, long> dTq0 = sc.ReadReg(64, 1, out w);
                List<int> lstOff = new List<int>(), lstUnknown = new List<int>();
                foreach (int id in ids)
                {
                    long v;
                    if (dTq0 == null || !dTq0.TryGetValue(id, out v)) lstUnknown.Add(id);
                    else if (v != 1) lstOff.Add(id);
                }
                if (lstOff.Count == 0 && lstUnknown.Count == 0) { strWhy = "이미 토크 ON"; return true; }
                List<int> lstOn = new List<int>(lstOff);
                lstOn.AddRange(lstUnknown);
                List<string> lstWarn = new List<string>();
                bool bComp = sc.SagCompInPlay();               // 켜기 전에 본다
                float[] afPose = null;
                if (lstUnknown.Count == 0)
                {
                    int[] anTick = sc.ReadTicks(out w);
                    if (anTick != null)
                    {
                        afPose = new float[ids.Length];
                        CArmMotorSet_t set = m_Motors;
                        for (int i = 0; i < ids.Length; i++)
                        {
                            afPose[i] = sc.RawToSim(i, anTick[i]);
                            CArmMotor_t m = (set == null) ? null : set.Get(ids[i]);
                            if (m != null && lstOff.Contains(ids[i]) && m.IsOutside(afPose[i], OutsideTolDeg)) lstWarn.Add(m.OutsideText(afPose[i]) + " — 움직이는 동작은 막힘");
                        }
                    }
                    else lstWarn.Add("읽기 실패 — 켜는 축의 자세를 몰라 실물로 보내지 않음. 움직이기 전에 다시 맞춤 (" + w + ")");
                }
                else lstWarn.Add("ID " + CArmMotorSet_t.RangeText(lstUnknown.ToArray()) + " 토크 상태를 읽지 못함 — 붙잡은 자세를 몰라 실물로 보내지 않음. 움직이기 전에 서보 목표로 다시 맞춤 (" + w + ")");
                bool bCompOff = false;
                bool bHeldAny = lstOn.Count < ids.Length;      // 토크를 붙잡고 있던 축이 있다 — 없으면(전 축이 꺼져 있었다) 토크 켜기와 같다 (보정은 0 부터)
                if (bComp && afPose != null && bHeldAny)
                {
                    // 처짐 보정이 걸려 있다 (10-03 설계 바꿈) — 켜져 있던 축 = 서보 목표, 켜는 축 = 지금 위치로 3D·붙잡은 자세를 맞추고 보정을 페이드 없이 끈다
                    //   → 다음 전송 틱 = 서보가 붙잡은 값 그대로 (튐 0), 그 뒤에도 보정이 얹히지 않는다. 켜져 있던 축의 목표를 모르면(붙잡은 자세를 원래 몰랐고
                    //   목표도 못 읽음) 붙잡은 자세를 모름으로 — 움직이기 전 확인이 서보 목표로 맞추며 그때 보정을 끈다
                    float[] afAll = (sc.HoldSimDeg != null) ? HeldPose(sc, lstOn, afPose) : null;
                    if (afAll != null)
                    {
                        m_C3d.SagComp_OffForAdopt();
                        for (int i = 0; i < ids.Length; i++) m_C3d.SetData(ids[i], afAll[i]);
                        sc.HoldFromRealGoal(afAll, afAll);
                        afPose = afAll;                                // 켜는 축 = 지금 위치 그대로 (TorqueOnAxes 가 그 값으로)
                        bCompOff = true;
                    }
                    else afPose = null;
                }
                sc.TorqueOnAxes(lstOn.ToArray(), afPose);
                if (sc.HoldSimDeg == null) m_bUnsynced = true;
                if (bCompOff)
                {
                    lstWarn.Add("처짐 보정이 걸려 있었음 — 켜져 있던 축은 서보가 붙잡은 목표, 켠 축은 지금 위치 그대로 이어받음");
                    lstWarn.Add(ADOPT_COMP_OFF);
                }
                else if (bComp && m_bUnsynced)
                    lstWarn.Add("처짐 보정이 걸려 있음 — 붙잡은 자세를 몰라 실물로 보내지 않음. 움직이기 전에 전 축을 서보 목표로 맞춤 (그때 처짐 보정을 끄고 목표 그대로 이어받음)");
                else if (bComp)
                    lstWarn.Add("처짐 보정이 걸려 있음 — 켠 축은 지금(처진) 위치에서 보정이 0 부터 다음 동작의 출력 틱에 걸쳐 1초 동안 들어옴");   // 전 축이 꺼져 있었다 (토크 켜기와 같다)
                Dictionary<int, long> dTq = sc.ReadReg(64, 1, out w);
                List<int> lstStill = new List<int>();
                foreach (int id in ids) { long v; if (dTq == null || !dTq.TryGetValue(id, out v) || v != 1) lstStill.Add(id); }
                if (lstStill.Count > 0) lstWarn.Add("ID " + CArmMotorSet_t.RangeText(lstStill.ToArray()) + " 토크 확인 안 됨 (전원·배선 점검)");
                string strBlock = m_strRebuildBlock;
                if (strBlock != null) lstWarn.Add(strBlock + " — 움직이는 동작은 막힘");
                lstWarn.Insert(0, "ID " + CArmMotorSet_t.RangeText(lstOn.ToArray()) + " 토크를 켬"
                                  + ((lstOn.Count < ids.Length) ? " (나머지 축은 이미 켜져 있었음 — 붙잡은 자세 그대로)" : " (전 축)"));
                strWhy = string.Join("\r\n", lstWarn.ToArray());
                Log("토크 ON (꺼진 축만) — " + strWhy.Replace("\r\n", " / "));
                return true;
            }

            /// <summary>토크 끄기 (작업 스레드) — 팔이 처지거나 떨어질 수 있다는 확인은 UI 에서. 미연결·잠금 실패면 false</summary>
            public bool TorqueOff(string strOwner)
            {
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) return false;
                bool bTook;
                string w;
                if (!Enter(strOwner, "토크", true, out bTook, out w)) { Log("토크 끄기 거부 — " + w); return false; }
                try
                {
                    sc.torqoff();
                    m_bUnsynced = false;
                    ReapplyPendingLocked();                // 토크 ON 이라 장면이 거부했던 DH 리밋 반영 — 토크 OFF 면 받아들인다
                    return true;
                }
                catch (Exception ex) { Log("토크 끄기 오류: " + ex.Message); return false; }
                finally { Leave(bTook); Notify(); }
            }

            // ── 재부팅 (오류 지우기, 10-16) ──
            /// <summary>전 서보 재부팅 (작업 스레드, 블로킹 — 보통 1~3초) — 하드웨어 오류(주소 70)를 지운다 (CScene_t.reboot — REBOOT 브로드캐스트). 오류가 없어도 된다.
            /// 거부: 미연결 · 잠금을 잡지 못함 (이름 있는 작업이 쥐고 있으면 기다리지 않고 거부 — Enter 규칙 그대로) · 3D 모션 중 (스트리밍 중에는 보내지 않는다).
            /// 순서: 축마다 하드웨어 오류를 먼저 읽고 (보고의 '전') → 재부팅 → 다시 대답할 때까지 기다림 → 상태를 다시 읽는다 (ReadStateCore — 토크·오류·위치,
            /// 토크 OFF 라 3D = 실물 자세, LastState 갱신). 재부팅하면 서보 토크가 꺼지므로 연결도 토크 OFF 로 맞춘다 — 토크 끄기(TorqueOff)와 같이
            /// 다시 맞출 것 없음 (NeedsSync 해제), 미뤄 둔 DH 리밋 반영. 장면은 붙잡은 자세 모름·처짐 보정의 출력 단 상태를 비운다 (서보가 붙잡은 보정이 없다)
            /// → 뒤의 [토크 켜기] 는 보통 길 (지금 자세 그대로 켬) 로 간다. 팔이 떨어질 수 있다는 확인은 UI 에서.
            /// strReport = 요약 한 줄 + 축마다 '오류 전 → 후 · 응답' + 토크 줄 (거부면 사유). 반환 = 재부팅을 보냈다 (다시 대답하지 않은 축·남은 오류가 있어도 true — 보고에 적는다)</summary>
            public bool Reboot(string strOwner, out string strReport)
            {
                strReport = "";
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) { strReport = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                bool bTook;
                if (!Enter(strOwner, "재부팅", true, out bTook, out strReport)) return false;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strReport = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                    int[] ids = sc.IDs;
                    string w;
                    Dictionary<int, long> dErr0 = sc.ReadReg(70, 1, out w);          // 재부팅 전 하드웨어 오류 (못 읽은 축은 빠진다 — '?')
                    if (dErr0 == null) dErr0 = new Dictionary<int, long>();
                    bool bWasOn = sc.IsTorqueOn;
                    int nT0 = Environment.TickCount;
                    if (sc.RebootCore(-1) < 0) { strReport = "재부팅을 보내지 못함 — 3D 모션 중이거나 연결이 끊김 (Log 탭)"; return false; }   // 보내기 전 거부 — 장면은 그대로
                    int nMs = unchecked(Environment.TickCount - nT0);
                    m_bUnsynced = false;
                    CArmMotorState_t[] aSt;
                    string strRead = "", strAdopt;
                    try { aSt = ReadStateCore(sc, false, out strRead, out strAdopt); }
                    catch (Exception exRead) { aSt = new CArmMotorState_t[0]; strRead = "상태 읽기 오류: " + exRead.Message; }   // 연결은 유지 — 표는 '읽기' 로 다시
                    ReapplyPendingLocked();                // 토크 ON 이라 장면이 거부했던 DH 리밋 반영 — 이제 토크 OFF
                    // 보고 — 축마다 오류 전 → 후 · 응답
                    StringBuilder sb = new StringBuilder();
                    List<int> lstMiss = new List<int>(), lstErr = new List<int>(), lstCleared = new List<int>();
                    foreach (int id in ids)
                    {
                        CArmMotorState_t s = null;
                        foreach (CArmMotorState_t t in aSt) if (t != null && t.nAxis == id) { s = t; break; }
                        bool bResp = s != null && s.bResponded;
                        long v0;
                        bool bKnown0 = dErr0.TryGetValue(id, out v0);
                        if (!bResp) lstMiss.Add(id);
                        else if (s.nHwError != 0) lstErr.Add(id);
                        else if (bKnown0 && v0 != 0) lstCleared.Add(id);
                        sb.AppendFormat(CultureInfo.InvariantCulture, "ID {0} {1}: 오류 {2} → {3} · {4}",
                                        id, bResp ? s.strModel : "-", bKnown0 ? HwErrorText((int)v0) : "? (못 읽음)",
                                        bResp ? HwErrorText(s.nHwError) : "-", bResp ? "응답" : "무응답");
                        if (bResp && s.bTorqueKnown && s.bTorque) sb.Append(" · ★토크 ON 남음");
                        sb.Append("\r\n");
                    }
                    string strHead = string.Format(CultureInfo.InvariantCulture, "재부팅 ({0:0.0}초) — 응답 {1}/{2}", nMs / 1000f, ids.Length - lstMiss.Count, ids.Length);
                    if (lstCleared.Count > 0) strHead += " · 오류 지움 ID " + CArmMotorSet_t.RangeText(lstCleared.ToArray());
                    if (lstMiss.Count > 0) strHead += " · ★무응답 ID " + CArmMotorSet_t.RangeText(lstMiss.ToArray()) + " (전원·배선 확인)";
                    if (lstErr.Count > 0) strHead += " · ★오류 남음 ID " + CArmMotorSet_t.RangeText(lstErr.ToArray()) + " (원인을 없앤 뒤 다시 — 과부하면 걸림·하중, 전압이면 전원)";
                    if (lstMiss.Count == 0 && lstErr.Count == 0) strHead += " · 하드웨어 오류 없음";
                    sb.Insert(0, strHead + "\r\n");
                    if (strRead.Length > 0) sb.Append(strRead).Append("\r\n");
                    sb.Append("토크 OFF (재부팅하면 서보 토크가 꺼짐" + (bWasOn ? " — 재부팅 전에는 토크 ON 이었음" : "") + ") — 다시 붙잡으려면 [토크 켜기]");
                    strReport = sb.ToString();
                    Log(strHead);
                    return true;
                }
                catch (Exception ex) { strReport = "재부팅 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }
            /// <summary>하드웨어 오류 (주소 70, X 시리즈 비트) 글 — "0x20 과부하" 꼴 (0 이면 "0x00"). 비트: 0x01 전압 · 0x04 과열 · 0x08 엔코더 · 0x10 전기 충격 · 0x20 과부하</summary>
            public static string HwErrorText(int nErr)
            {
                string s = "0x" + (nErr & 0xFF).ToString("X2", CultureInfo.InvariantCulture);
                List<string> lst = new List<string>();
                if ((nErr & 0x01) != 0) lst.Add("전압");
                if ((nErr & 0x04) != 0) lst.Add("과열");
                if ((nErr & 0x08) != 0) lst.Add("엔코더");
                if ((nErr & 0x10) != 0) lst.Add("전기 충격");
                if ((nErr & 0x20) != 0) lst.Add("과부하");
                return (lst.Count > 0) ? s + " " + string.Join("·", lst.ToArray()) : s;
            }

            // ── 움직이기 전 확인 ──
            /// <summary>움직이는 동작(조그·측정·시험)을 시작하기 전에 부른다 (작업 스레드). 3D 만 바꾸고 실물은 움직이지 않는다.
            /// · 토크 OFF: 실측 → 3D = 실측.
            /// · 토크 ON 인데 정렬을 못 했던 경우(읽기 실패)·붙잡은 자세 모름(토크 상태·켜져 있던 축의 목표를 못 읽음 등): 그 뒤 명령을 보낸 적이 없으니
            ///   서보가 붙잡은 목표(Goal Position) 그대로 3D·붙잡은 자세를 맞춘다 → 첫 틱 = 서보 목표 (목표와 실측이 SyncTolDeg 넘게 다르면 거부).
            ///   목표에는 지난 처짐 보정이 들어 있을 수 있다 — 처짐 보정이 걸려 있으면 되짚지 않고 보정을 페이드 없이 끈다 (C3d.SagComp_OffForAdopt,
            ///   10-03 설계 바꿈 — 통과하면 strWhy = ADOPT_COMP_OFF 알림, StateChanged 로 앱의 '처짐 보정 켬' 이 따라온다). 보정이 걸려 있지 않으면
            ///   전환을 0 에서 다시 시작한다. 목표를 못 읽으면 거부한다 (10-03 — 처진 지금 위치로 맞춰 보내면 하중만큼 떨어진다).
            /// · 토크 ON 정상: 3D 가 붙잡은 자세에서 0.3° 넘게 벗어났으면(Control 탭·마우스, 3D 리밋이 장면 리밋보다 넓었던 동안 리밋 밖으로 나간 3D —
            ///   붙잡은 자세는 장면 리밋으로 자른 값) 되돌리고, 실측이 걸린 목표와도 붙잡은 자세와도 SyncTolDeg 넘게 다르면 거부 (막힘·토크 풀림·전원 —
            ///   처짐 보정이 켜져 있으면 실물은 보정을 뺀 붙잡은 자세 근처에 있다).
            /// 연결 중이면 먼저 연결된 축의 3D 리밋(헤더)이 적용된 리밋과 다를 때 다시 쓴다 (3D 가 장면이 자르는 리밋 밖으로 나가지 않게).
            /// 공통: 읽기 실패, 리밋 밖(OutsideTolDeg 넘게 — 보낼 때 잘리며 그 축이 튄다) 축이 있으면 거부. 토크 ON 인데 토크가 꺼진(과부하·전원
            /// 오류로 서보가 스스로 끈 경우 등) 축이 있으면 거부. DH 를 다시 만든 뒤 리밋을 반영하지 못한 상태(수동 리밋과 $jointrange 가 겹치지 않음,
            /// 장면이 거부)면 거부. 미연결이면 AllowSimOnly 때만 — 그 상태와 3D 자세의 리밋만 본다</summary>
            public bool EnsureSynced(string strOwner, out string strWhy)
            {
                strWhy = "";
                bool bTook;
                if (!Enter(strOwner, "동기화", true, out bTook, out strWhy)) return false;
                try { return EnsureSyncedCore(out strWhy); }
                catch (Exception ex) { strWhy = "동기화 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }
            private bool EnsureSyncedCore(out string strWhy)
            {
                strWhy = "";
                CScene_t sc = m_Scene;
                CArmMotorSet_t set = m_Motors;
                if (sc == null || !sc.IsConnected)
                {
                    if (!AllowSimOnly) { strWhy = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                    string strBlockSim = m_strRebuildBlock;
                    if (strBlockSim != null) { strWhy = strBlockSim; return false; }
                    if (set == null) return true;
                    List<string> lstSim = new List<string>();
                    foreach (CArmMotor_t m in set.Motors)
                    {
                        float f = m_C3d.GetData(m.nAxis);
                        if (m.IsOutside(f, OutsideTolDeg)) lstSim.Add(m.OutsideText(f));
                    }
                    if (lstSim.Count > 0) { strWhy = string.Join(", ", lstSim.ToArray()) + " — 3D 를 범위 안에 놓거나 리밋을 고치십시오"; return false; }
                    return true;
                }
                if (ConfigMismatch) { strWhy = "모터 구성이 연결 뒤 바뀜 — 다시 연결하십시오"; return false; }
                if (m_C3d.MoveJoints_IsMoving()) { strWhy = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                // 3D 리밋(헤더)이 적용된 리밋(= 장면이 자르는 리밋)과 다르면 되돌린다 — 훅을 거치지 않고 헤더가 바뀐 경우
                string strHdr = HoldHeaderLimitsNow();
                if (strHdr.Length > 0) Log("3D 리밋(헤더)을 적용된 세팅으로 되돌림 (" + strHdr + ")");
                string strBlock = m_strRebuildBlock;
                if (strBlock != null) { strWhy = strBlock; return false; }
                string w;
                int[] ids = sc.IDs;
                int[] anTick = sc.ReadTicks(out w);
                if (anTick == null) { strWhy = "읽기 실패 — " + w + " (전원·배선·ID 확인)"; return false; }
                if (sc.IsTorqueOn)
                {
                    // 토크가 정말 켜져 있나 — 과부하·전압 오류면 서보가 스스로 끈다. 꺼진 축이 섞여 있으면 움직이지 않는다
                    Dictionary<int, long> dTq = sc.ReadReg(64, 1, out w);
                    List<int> lstOff = new List<int>();
                    foreach (int id in ids) { long v; if (dTq == null || !dTq.TryGetValue(id, out v) || v != 1) lstOff.Add(id); }
                    if (lstOff.Count > 0)
                    { strWhy = "ID " + CArmMotorSet_t.RangeText(lstOff.ToArray()) + " 토크 꺼짐(또는 확인 안 됨) — 토크를 끈 뒤 다시 켜십시오 (전원·과부하 오류 확인)"; return false; }
                }
                float[] afReal = new float[ids.Length];
                for (int i = 0; i < ids.Length; i++) afReal[i] = sc.RawToSim(i, anTick[i]);
                float[] afHold = sc.HoldSimDeg;
                bool bFromReal = !sc.IsTorqueOn || m_bUnsynced || afHold == null;
                float[] afPose = bFromReal ? afReal : afHold;          // 다음 명령이 시작할 3D 자세
                bool bGoal = false, bCompAdopt = false;
                float[] afGoalHeld = null;
                if (bFromReal && sc.IsTorqueOn)
                {
                    // 붙잡은 자세를 모르는 채 켜진 토크 — 그 뒤 명령을 보낸 적이 없으니 서보가 붙잡은 목표(Goal Position)가 곧 붙잡은 자세.
                    // 지금 위치는 하중만큼 목표에서 처져 있어 그대로 보내면 그만큼 떨어진다 → 목표를 못 읽으면 맞추지 않고 거부 (K5, 10-03 — 전에는 지금 위치로 맞췄다).
                    // 목표 그대로 3D·붙잡은 자세로 삼는다 — 되짚지 않는다 (10-03 설계 바꿈). 처짐 보정이 걸려 있으면 맞출 때 페이드 없이 끈다 (아래)
                    float[] afGoal = ReadGoalSim(sc);
                    if (afGoal == null)
                    {
                        for (int i = 0; i < ids.Length; i++) m_C3d.SetData(ids[i], afReal[i]);   // 보여 주기만 — 붙잡은 자세는 모름 그대로 (보내지 않는다)
                        strWhy = "서보 목표(Goal Position)를 읽지 못함 — 토크 ON 인 서보는 목표를 붙잡고 있고 지금 위치는 하중만큼 처져 있어, 지금 위치로 맞춰 보내면 그만큼 떨어진다. "
                               + "다시 하십시오 (계속 안 되면 팔을 받친 뒤 토크를 껐다 켜십시오)";
                        return false;
                    }
                    List<string> lstFar = new List<string>();
                    for (int i = 0; i < ids.Length; i++)
                        if (Math.Abs(afReal[i] - afGoal[i]) > SyncTolDeg)
                            lstFar.Add(string.Format(CultureInfo.InvariantCulture, "ID {0} 실물 {1:0.0}° / 목표 {2:0.0}°", ids[i], afReal[i], afGoal[i]));
                    if (lstFar.Count > 0) { strWhy = string.Join(", ", lstFar.ToArray()) + " — 실물이 서보 목표와 다름 (막힘·토크 풀림·전원 확인)"; return false; }
                    afPose = afGoal;
                    afGoalHeld = afGoal;
                    bGoal = true;
                    bCompAdopt = sc.SagCompInPlay();                    // 처짐 보정이 걸려 있다 — 맞출 때 페이드 없이 끈다 (붙잡은 자세는 아직 모름 — 모델·출력 단만 본다)
                }
                List<string> lstOut = new List<string>();
                for (int i = 0; i < ids.Length; i++)
                {
                    CArmMotor_t m = (set == null) ? null : set.Get(ids[i]);
                    if (m != null && m.IsOutside(afPose[i], OutsideTolDeg)) lstOut.Add(m.OutsideText(afPose[i]));
                }
                if (bFromReal)
                {
                    for (int i = 0; i < ids.Length; i++) m_C3d.SetData(ids[i], afPose[i]);   // 3D 를 실물 자세로 (보여 주기는 늘 맞게)
                    if (lstOut.Count > 0) { strWhy = string.Join(", ", lstOut.ToArray()) + " — 토크를 끄고 손으로 범위 안에 놓거나 리밋을 고치십시오"; return false; }
                    if (sc.IsTorqueOn)
                    {
                        if (bCompAdopt) m_C3d.SagComp_OffForAdopt();                      // 처짐 보정이 걸려 있었다 — 페이드 없이 끈다 → 다음 틱 = 서보 목표 (튐 0)
                        sc.HoldFromRealGoal(afPose, bGoal ? afGoalHeld : afPose);         // 붙잡은 자세 = 걸린 목표 = 서보 목표 그대로
                        m_C3d.SagComp_JointReset();
                        m_bUnsynced = false;
                        Log("토크 ON 상태에서 3D 를 " + (bGoal ? "서보가 붙잡은 목표로" : "실측 자세로") + " 맞춤 (토크 켤 때 붙잡은 자세를 정하지 못함)"
                            + (bCompAdopt ? " — " + ADOPT_COMP_OFF : ""));
                        if (bCompAdopt) strWhy = ADOPT_COMP_OFF;                          // 통과 — 알림 (부른 쪽이 보여 준다)
                    }
                    return true;
                }
                if (lstOut.Count > 0) { strWhy = string.Join(", ", lstOut.ToArray()) + " — 토크를 끄고 손으로 범위 안에 놓거나 리밋을 고치십시오"; return false; }
                int nBack = 0;
                for (int i = 0; i < ids.Length; i++)
                    if (Math.Abs(m_C3d.GetData(ids[i]) - afHold[i]) > 0.3f) { m_C3d.SetData(ids[i], afHold[i]); nBack++; }
                if (nBack > 0) Log("3D 를 실물이 붙잡은 자세로 되돌림 (" + nBack + "축 — Control 탭·마우스로 바꾸거나 리밋 밖으로 나간 3D 는 실물로 가지 않는다)");
                float[] afExpect = sc.GoalSimDeg;
                if (afExpect == null) afExpect = afHold;
                List<string> lstGap = new List<string>();
                for (int i = 0; i < ids.Length; i++)
                {
                    float d = afReal[i] - afExpect[i];
                    // 처짐 보정이 켜져 있으면 실물 ≈ 붙잡은 자세 (보정이 처짐을 메운다) — 명령(붙잡은 자세 + 보정, 바닥 보정까지 6° 넘을 수 있다)과도, 붙잡은 자세와도 멀어야 거부
                    if (Math.Abs(d) > SyncTolDeg && Math.Abs(afReal[i] - afHold[i]) > SyncTolDeg)
                        lstGap.Add(string.Format(CultureInfo.InvariantCulture, "ID {0} 실물 {1:0.0}° / 명령 {2:0.0}° (차 {3:0.0}°)", ids[i], afReal[i], afExpect[i], d));
                }
                if (lstGap.Count > 0) { strWhy = string.Join(", ", lstGap.ToArray()) + " — 실물이 명령과 다름 (막힘·토크 풀림·전원 확인)"; return false; }
                return true;
            }

            // ── 읽기·따라가기 ──
            /// <summary>전 축 상태 읽기 (작업 스레드) — 레지스터(모델·Drive Mode·Operating Mode·Homing Offset·위치 한계·토크·하드웨어 오류·전압·온도)와
            /// 정착 위치. 토크 OFF 면 3D 도 실물 자세로 맞추고, 토크 ON 이면 3D 는 건드리지 않는다. 표의 기종이 미지정이면 실물 기종으로 채운다
            /// (bModelFilled). 서보의 토크 상태가 연결이 아는 것과 다르면 그 축에 경고 (bWarn). 실패하면 null 과 사유</summary>
            public CArmMotorState_t[] ReadState(string strOwner, out string strWhy)
            {
                strWhy = "";
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) { strWhy = "연결 안 됨 — 먼저 연결하십시오"; return null; }
                bool bTook;
                if (!Enter(strOwner, "읽기", true, out bTook, out strWhy)) return null;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strWhy = "사용 중: 3D 모션 — 끝난 뒤 다시"; return null; }
                    string strAdopt;
                    return CloneStates(ReadStateCore(sc, false, out strWhy, out strAdopt));
                }
                catch (Exception ex) { strWhy = "읽기 오류: " + ex.Message; return null; }
                finally { Leave(bTook); Notify(); }
            }
            private static Dictionary<int, long> Reg(CScene_t sc, int nAddr, int nLen, int nTol)
            {
                string w;
                Dictionary<int, long> d = sc.ReadReg(nAddr, nLen, nTol, out w);
                return (d == null) ? new Dictionary<int, long>() : d;
            }
            /// <summary>상태 읽기 본체. bAdopt(연결 때) 면 서보가 이미 토크 ON 일 때 그 상태를 이어받는다 (AdoptTorque — strAdopt = 보고 문구)</summary>
            private CArmMotorState_t[] ReadStateCore(CScene_t sc, bool bAdopt, out string strWhy, out string strAdopt)
            {
                strWhy = "";
                strAdopt = "";
                int[] ids = sc.IDs;
                Dictionary<int, long> dModel = Reg(sc, 0, 2, 0), dDrive = Reg(sc, 10, 1, 0), dOp = Reg(sc, 11, 1, 0), dHome = Reg(sc, 20, 4, 0),
                                      dMax = Reg(sc, 48, 4, 0), dMin = Reg(sc, 52, 4, 0), dTq = Reg(sc, 64, 1, 0), dErr = Reg(sc, 70, 1, 0),
                                      dVolt = Reg(sc, 144, 2, 3), dTemp = Reg(sc, 146, 1, 1);   // 전압·온도는 조금씩 흔들린다 (0.3 V, 1 °C 안이면 확정)
                string strPos;
                int[] anTick = sc.ReadTicks(out strPos);
                CArmMotorSet_t set = m_Motors;
                bool bMis = ConfigMismatch;
                CArmMotorState_t[] aSt = new CArmMotorState_t[ids.Length];
                bool bFill = false;
                for (int i = 0; i < ids.Length; i++)
                {
                    int id = ids[i];
                    CArmMotorState_t s = new CArmMotorState_t();
                    s.nAxis = id;
                    s.nBusID = id;
                    CArmMotor_t m = (set == null) ? null : set.Get(id);
                    List<string> lstN = new List<string>();
                    long v;
                    s.bResponded = dModel.TryGetValue(id, out v);
                    if (s.bResponded) { s.nModel = (int)v; s.strModel = CArmMotorSet_t.ModelName(s.nModel); }
                    else lstN.Add("무응답");
                    if (dDrive.TryGetValue(id, out v))
                    {
                        s.nDriveMode = (int)v;
                        if ((v & 1) != 0) { lstN.Add("Drive Mode 역방향 비트 켜짐 — 서보가 스스로 방향을 뒤집고 있음(다른 프로그램이 쓴 값일 수 있음)"); s.bWarn = true; }
                    }
                    if (dOp.TryGetValue(id, out v)) { s.nOperatingMode = (int)v; if (v != 3) { lstN.Add("위치 제어 모드 아님(Operating Mode " + v + ")"); s.bWarn = true; } }
                    if (dHome.TryGetValue(id, out v)) { s.nHomingOffset = (int)v; if (v != 0) { lstN.Add("Homing Offset " + v + "틱"); s.bWarn = true; } }
                    if (dMax.TryGetValue(id, out v)) s.nMaxPos = (int)v;
                    if (dMin.TryGetValue(id, out v)) s.nMinPos = (int)v;
                    if (dTq.TryGetValue(id, out v)) { s.bTorque = (v != 0); s.bTorqueKnown = true; }
                    if (dErr.TryGetValue(id, out v)) { s.nHwError = (int)v; if (v != 0) { lstN.Add("하드웨어 오류 " + HwErrorText((int)v) + " — 재부팅(Reboot)으로 지울 수 있음"); s.bWarn = true; } }
                    if (dVolt.TryGetValue(id, out v)) s.fVolt = v * 0.1f;
                    if (dTemp.TryGetValue(id, out v)) s.nTempC = (int)v;
                    if (anTick != null) { s.bTickOk = true; s.nTick = anTick[i]; s.fSim = sc.RawToSim(i, anTick[i]); }
                    else if (s.bResponded) lstN.Add("위치 읽기 실패");
                    if (m != null && s.bResponded)
                    {
                        if (m.nHwMotor_Key != 0 && m.nHwMotor_Key != s.nModel)
                        { lstN.Add("기종 다름(표 " + CArmMotorSet_t.ModelName(m.nHwMotor_Key) + " / 실물 " + s.strModel + ")"); s.bWarn = true; }
                        else if (m.nHwMotor_Key == 0 && s.nModel > 0 && !bMis)
                        { s.bModelFilled = true; bFill = true; lstN.Add("기종 채움(" + s.strModel + ")"); }
                    }
                    if (m != null && s.bTickOk && m.IsOutside(s.fSim, OutsideTolDeg)) { s.bOutside = true; lstN.Add("리밋 밖"); }
                    s.strNote = string.Join(" · ", lstN.ToArray());
                    aSt[i] = s;
                }
                // 연결 때 서보가 이미 토크 ON 이면(연결 해제 때 토크를 유지했던 경우 등) 그 상태를 이어받는다 — 버스에 쓰지 않는다.
                // 이어받지 않으면 연결은 토크 OFF 로 알아서, 끝낼 때 토크를 꺼 팔이 떨어지거나 굳은 관절을 '따라가기' 하게 된다
                if (bAdopt && !sc.IsTorqueOn && !bMis) strAdopt = AdoptTorque(sc, aSt, anTick);
                bool bTqScene = sc.IsTorqueOn;
                foreach (CArmMotorState_t s in aSt)
                {
                    if (!s.bResponded || !s.bTorqueKnown || s.bTorque == bTqScene) continue;
                    string strTq = s.bTorque ? "토크 켜져 있음 (연결은 OFF 로 앎 — 토크 켜기로 이어받은 뒤 필요하면 끄기)" : "토크 꺼져 있음 (연결은 ON — 전원·과부하 오류 확인, 토크를 끈 뒤 다시 켜기)";
                    s.strNote = (s.strNote.Length > 0) ? s.strNote + " · " + strTq : strTq;
                    s.bWarn = true;
                }
                if (!sc.IsTorqueOn && anTick != null && !bMis)
                    for (int i = 0; i < ids.Length; i++) m_C3d.SetData(ids[i], aSt[i].fSim);   // 토크 OFF — 3D 를 실물 자세로
                if (bFill && set != null)
                {
                    // 표의 기종이 미지정인 축을 실물 기종으로 채운다 — 매핑과 무관해 버스·장면은 그대로
                    CArmMotorSet_t s2 = set.Clone();
                    foreach (CArmMotorState_t s in aSt)
                    {
                        if (!s.bModelFilled) continue;
                        CArmMotor_t m = s2.Get(s.nAxis);
                        if (m != null) { m.nHwMotor_Key = s.nModel; m.strSource = "수정"; }
                    }
                    lock (m_oHdr)
                    {
                        s2.ApplyToHeader(m_C3d, set);
                        m_Motors = s2;
                        m_HdrSet = s2;
                    }
                }
                lock (m_oSync) m_aState = aSt;
                List<string> lstWhy = new List<string>();
                List<int> lstMiss = new List<int>();
                foreach (CArmMotorState_t s in aSt) if (!s.bResponded) lstMiss.Add(s.nAxis);
                if (lstMiss.Count > 0) lstWhy.Add("무응답: ID " + CArmMotorSet_t.RangeText(lstMiss.ToArray()));
                if (anTick == null) lstWhy.Add("위치 읽기 실패 — " + strPos);
                strWhy = string.Join("\r\n", lstWhy.ToArray());
                return aSt;
            }
            /// <summary>연결 때 토크 켜진 서보가 있으면 그 상태를 이어받는다 (ReadStateCore) — 버스에 쓰지 않는다.
            /// 토크 켜진 축은 서보가 붙잡은 목표(Goal Position, 주소 116)를, 꺼진 축은 지금 위치를 세팅으로 해석해 그대로 3D·붙잡은 자세로 삼는다
            /// → 다음 전송 틱 = 지금 목표 틱 (지금 위치는 하중만큼 목표에서 처져 있어 그것을 보내면 그만큼 떨어진다).
            /// 목표에는 지난 처짐 보정이 들어 있을 수 있다 — 처짐 보정이 걸려 있으면(CScene_t.SagCompInPlay) 되짚지 않고 보정을 페이드 없이 끈다
            /// (C3d.SagComp_OffForAdopt, 10-03 설계 바꿈 — 보고에 ADOPT_COMP_OFF) → 그 뒤에도 보정이 얹히지 않는다.
            /// 목표·위치를 못 읽은 축이 있으면 붙잡은 자세는 모름 — 보내지 않고(SendPose), 움직이기 전에 EnsureSynced 가 서보 목표로 맞춘다 (못 읽으면 막힘).
            /// 반환 = 보고 문구 ("" = 토크 켜진 서보 없음)</summary>
            private string AdoptTorque(CScene_t sc, CArmMotorState_t[] aSt, int[] anTick)
            {
                int[] ids = sc.IDs;
                List<int> lstOn = new List<int>(), lstOff = new List<int>(), lstUnknown = new List<int>();
                for (int i = 0; i < ids.Length && i < aSt.Length; i++)
                {
                    CArmMotorState_t s = aSt[i];
                    if (s == null || !s.bResponded || !s.bTorqueKnown) lstUnknown.Add(ids[i]);
                    else if (s.bTorque) lstOn.Add(ids[i]);
                    else lstOff.Add(ids[i]);
                }
                if (lstOn.Count == 0) return "";
                float[] afGoal = ReadGoalAll(sc);
                float[] afHeld = new float[ids.Length];
                List<int> lstNoGoal = new List<int>();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (lstOn.Contains(ids[i]))
                    {
                        if (afGoal != null && !float.IsNaN(afGoal[i])) afHeld[i] = afGoal[i];
                        else lstNoGoal.Add(ids[i]);
                    }
                    else if (lstOff.Contains(ids[i]) && anTick != null) afHeld[i] = sc.RawToSim(i, anTick[i]);   // 토크 꺼진 축 — 지금 자세
                    else lstNoGoal.Add(ids[i]);
                }
                bool bHold = lstNoGoal.Count == 0;
                // 서보 목표 그대로 이어받는다 (되짚지 않는다 — 10-03 설계 바꿈). 처짐 보정이 걸려 있으면 페이드 없이 끈다 → 다음 전송 틱 = 서보 목표 (튐 0),
                //   그 뒤에도 보정이 얹히지 않는다. 목표를 못 읽은 축이 있으면 붙잡은 자세는 모름 — 움직이기 전 확인이 맞추며 그때 끈다
                bool bCompOff = bHold && sc.SagCompInPlay();
                if (bCompOff) m_C3d.SagComp_OffForAdopt();
                sc.AdoptTorqueOnGoal(bHold ? afHeld : null, bHold ? afHeld : null);
                m_bUnsynced = !bHold;
                List<string> lst = new List<string>();
                lst.Add("토크가 이미 켜져 있음 (ID " + CArmMotorSet_t.RangeText(lstOn.ToArray()) + ") — 새로 켜지 않고 토크 ON 상태로 이어받음"
                        + (bHold ? " (3D = 서보가 붙잡은 목표)" : ""));
                if (bCompOff) lst.Add(ADOPT_COMP_OFF);
                if (lstOff.Count > 0 || lstUnknown.Count > 0)
                {
                    List<int> lstBad = new List<int>(lstOff);
                    lstBad.AddRange(lstUnknown);
                    lst.Add("ID " + CArmMotorSet_t.RangeText(lstBad.ToArray()) + " 은 토크 꺼짐(또는 확인 안 됨) — 움직이는 동작은 막힘. 토크를 끈 뒤 다시 켜면 전 축이 켜짐");
                }
                if (!bHold) lst.Add("붙잡은 목표를 읽지 못함 (ID " + CArmMotorSet_t.RangeText(lstNoGoal.ToArray()) + ") — 실물로 보내지 않고, 움직이기 전에 다시 읽어 맞춤");
                string strRep = string.Join("\r\n", lst.ToArray());
                Log(strRep.Replace("\r\n", " / "));
                return strRep;
            }
            /// <summary>일부 축만 다시 켤 때(TorqueOnRest) 처짐 보정이 걸려 있으면 이어받을 자세 — 켜는 축 = 지금 위치(afPose), 켜져 있던 축 = 서보 목표
            /// (주소 116 — 못 읽은 축은 장면이 마지막으로 보낸 목표, 같은 값이어야 한다). 켜져 있던 축의 목표를 모르면 null. IDs 순 3D 각</summary>
            private static float[] HeldPose(CScene_t sc, List<int> lstOn, float[] afPose)
            {
                int[] ids = sc.IDs;
                if (afPose == null || afPose.Length != ids.Length) return null;
                float[] afReg = ReadGoalAll(sc), afSent = sc.GoalSimDeg;
                float[] a = new float[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    if (lstOn.Contains(ids[i])) { a[i] = afPose[i]; continue; }
                    if (afReg != null && i < afReg.Length && !float.IsNaN(afReg[i])) a[i] = afReg[i];
                    else if (afSent != null && afSent.Length == ids.Length) a[i] = afSent[i];
                    else return null;
                }
                return a;
            }
            /// <summary>서보 목표(Goal Position, 주소 116)를 세팅으로 해석한 3D 각 (IDs 순, 못 읽은 축은 NaN). 미연결이면 null</summary>
            private static float[] ReadGoalAll(CScene_t sc)
            {
                string w;
                Dictionary<int, long> d = sc.ReadReg(116, 4, out w);
                if (d == null) return null;
                int[] ids = sc.IDs;
                float[] af = new float[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    long v;
                    af[i] = (d.TryGetValue(ids[i], out v) && v != 0 && v != -1 && v >= -8192 && v <= 12288) ? sc.RawToSim(i, (int)v) : float.NaN;
                }
                return af;
            }
            /// <summary>전 축 서보 목표 (3D 각, IDs 순) — 하나라도 못 읽으면 null</summary>
            private static float[] ReadGoalSim(CScene_t sc)
            {
                float[] af = ReadGoalAll(sc);
                if (af == null) return null;
                foreach (float f in af) if (float.IsNaN(f)) return null;
                return af;
            }

            /// <summary>따라가기 한 번 (UI 타이머용, 약 15ms — 무응답 서보가 있으면 약 70ms, 로그·알림 없음) — 연결 && 토크 OFF && 잠금이 비어 있고
            /// 모션이 없을 때만: 빠른 읽기 → 3D = 실물, LastState 의 각만 갱신. 아니면 false. 잠금을 잡은 뒤에 연결·토크·모션을 다시 본다
            /// (잡는 사이에 끝난 토크 켜기 뒤에 읽은 값으로 3D 를 덮지 않게).
            /// 잠금을 기다리지 않고, 잠금을 기다리는 쪽이 있으면 건너뛴다 (작업 스레드의 다른 동작은 이 읽기가 끝나기를 기다렸다 잡는다 — BusyOwner 에도 보이지 않는다).
            /// ※ 버스 읽기는 안에서 Application.DoEvents 를 돈다 — UI 타이머에서 부르면 그동안 끼어든 UI 이벤트가 같은 스레드에서 이 연결의
            ///   잠금 함수(ApplyMotors 등)를 부르면 기다릴 수 없어 "사용 중: 따라가기" 로 거부된다 (표 편집 중에는 따라가기 Tick 을 건너뛸 것)</summary>
            public bool FollowOnce()
            {
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected || sc.IsTorqueOn) return false;
                if (m_C3d.MoveJoints_IsMoving() || ConfigMismatch) return false;
                bool bTook;
                string w;
                if (!EnterLock(null, FOLLOW_NAME, false, 0, true, out bTook, out w)) return false;
                try
                {
                    // 잠금을 잡는 사이 토크가 켜졌거나(TorqueOn 이 막 끝남) 연결·모션이 바뀌었으면 읽은 값으로 3D 를 덮지 않는다 — 다시 본다
                    if (m_Scene != sc || !sc.IsConnected || sc.IsTorqueOn || m_C3d.MoveJoints_IsMoving()) return false;
                    Dictionary<int, float> d = sc.ReadDegQuick(out w);
                    if (d == null) return false;
                    int[] ids = sc.IDs;
                    foreach (int id in ids) { float f; if (d.TryGetValue(id, out f)) m_C3d.SetData(id, f); }
                    CArmMotorSet_t set = m_Motors;
                    lock (m_oSync)
                    {
                        if (m_aState != null)
                            foreach (CArmMotorState_t s in m_aState)
                            {
                                float f;
                                if (s == null || !d.TryGetValue(s.nAxis, out f)) continue;
                                s.fSim = f;
                                CArmMotor_t m = (set == null) ? null : set.Get(s.nAxis);
                                if (m == null) continue;
                                s.nTick = m.SimToTick(f);
                                s.bTickOk = true;
                                s.bOutside = m.IsOutside(f, OutsideTolDeg);
                            }
                    }
                    return true;
                }
                catch { return false; }
                finally { Leave(bTook); }
            }

            // ── 조그 ──
            /// <summary>조그 계획 (순수 계산) — from = 지금 3D 각, to = 리밋으로 자른 from + step, 시간 = max(JogMinMs, 1.5 × |to−from| ÷ JogDegPerSec)
            /// (c3d.Play 는 smoothstep 이라 최고 속도가 평균의 1.5 배 — 최고 속도가 JogDegPerSec 을 넘지 않게. 3° 면 300ms, 10° 면 1000ms).
            /// step 0·|step| &gt; JogMaxDeg·리밋에 닿음(움직임 0.2° 미만이거나 반대로 가게 됨)이면 false</summary>
            public bool PlanJog(int nAxis, float fStepDeg, out float fFrom, out float fTo, out int nTimeMs, out string strWhy)
            {
                fFrom = float.NaN;
                fTo = float.NaN;
                nTimeMs = 0;
                strWhy = "";
                if (float.IsNaN(fStepDeg) || float.IsInfinity(fStepDeg) || fStepDeg == 0f) { strWhy = "조그 각이 0"; return false; }
                if (Math.Abs(fStepDeg) > JogMaxDeg) { strWhy = "조그는 한 번에 최대 " + CArmMotorSet_t.Fmt(JogMaxDeg) + "°"; return false; }
                if (Math.Abs(fStepDeg) < 0.2f) { strWhy = "조그 각이 너무 작음 (0.2° 이상)"; return false; }
                CArmMotorSet_t set = m_Motors;
                CArmMotor_t m = (set == null) ? null : set.Get(nAxis);
                if (m == null) { strWhy = "ID " + nAxis + " 모터 세팅 없음"; return false; }
                fFrom = m_C3d.GetData(nAxis);
                fTo = m.Clamp(fFrom + fStepDeg);
                float d = fTo - fFrom;
                if (Math.Abs(d) < 0.2f || d * fStepDeg < 0f)
                {
                    strWhy = (fStepDeg > 0f) ? "리밋에 닿음 (상한 " + CArmMotorSet_t.Fmt(m.LimitHi) + "°)" : "리밋에 닿음 (하한 " + CArmMotorSet_t.Fmt(m.LimitLo) + "°)";
                    return false;
                }
                double dSpeed = (JogDegPerSec > 0f && !float.IsInfinity(JogDegPerSec)) ? JogDegPerSec : 15.0;
                nTimeMs = Math.Max(JogMinMs, (int)Math.Ceiling(JOG_PEAK_RATIO * Math.Abs((double)d) / dSpeed * 1000.0 - 1e-6));   // 최고 속도 ≤ JogDegPerSec
                return true;
            }
            /// <summary>smoothstep(3t² − 2t³) 보간의 최고 속도 ÷ 평균 속도 — c3d.Play 의 관절 보간 (COjw_12_3D.cs MoveJoints_Step)</summary>
            private const double JOG_PEAK_RATIO = 1.5;

            /// <summary>조그 (작업 스레드, 블로킹) — 연결 + 토크 ON 에서만 (AllowSimOnly 면 미연결 때 3D 만). 잠금("조그") → EnsureSynced →
            /// PlanJog → 실측 → c3d.Play (스텝마다 SendPose 가 전 축을 보낸다, 다른 축은 지금 자세 유지) → 300ms 뒤 실측.
            /// 방향이 틀려도 읽기·쓰기가 같은 세팅이라 실측 부호는 명령과 같게 나온다 — 방향 판정은 사람 눈으로만 (UI 가 묻는다)</summary>
            public bool Jog(int nAxis, float fStepDeg, out CArmJogResult_t res, out string strWhy)
            {
                res = null;
                strWhy = "";
                CScene_t sc = m_Scene;
                bool bReal = sc != null && sc.IsConnected;
                if (!bReal && !AllowSimOnly) { strWhy = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                if (bReal && !sc.IsTorqueOn) { strWhy = "토크 OFF — 조그는 토크 ON 에서만"; return false; }
                if (float.IsNaN(fStepDeg) || fStepDeg == 0f) { strWhy = "조그 각이 0"; return false; }
                if (Math.Abs(fStepDeg) > JogMaxDeg) { strWhy = "조그는 한 번에 최대 " + CArmMotorSet_t.Fmt(JogMaxDeg) + "°"; return false; }
                if (bReal && ConfigMismatch) { strWhy = "모터 구성이 연결 뒤 바뀜 — 다시 연결하십시오"; return false; }
                bool bTook;
                if (!Enter(null, "조그", true, out bTook, out strWhy)) return false;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strWhy = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                    if (!EnsureSyncedCore(out strWhy)) return false;
                    float fFrom, fTo;
                    int nMs;
                    if (!PlanJog(nAxis, fStepDeg, out fFrom, out fTo, out nMs, out strWhy)) return false;
                    CArmJogResult_t r = new CArmJogResult_t();
                    r.nAxis = nAxis;
                    r.fFrom = fFrom;
                    r.fTo = fTo;
                    string w;
                    if (bReal)
                    {
                        if (Array.IndexOf(sc.IDs, nAxis) < 0) { strWhy = "ID " + nAxis + " 이 연결된 모터에 없음"; return false; }
                        Dictionary<int, float> d0 = sc.ReadDeg(out w);
                        if (d0 == null || !d0.ContainsKey(nAxis)) { strWhy = "조그 전 읽기 실패 — " + w; return false; }
                        r.fRealFrom = d0[nAxis];
                    }
                    m_C3d.Play(nMs, 0, (float)nAxis, fTo);
                    if (bReal)
                    {
                        Thread.Sleep(300);
                        Dictionary<int, float> d1 = sc.ReadDeg(out w);
                        if (d1 == null || !d1.ContainsKey(nAxis)) r.strNote = "조그 뒤 읽기 실패 — " + w;
                        else
                        {
                            r.fRealTo = d1[nAxis];
                            r.fRealDelta = r.fRealTo - r.fRealFrom;
                            float fCmd = fTo - fFrom;
                            if (Math.Abs(r.fRealDelta) < 0.5f) r.strNote = "실물이 거의 안 움직임 — 토크·전원·ID 확인";
                            else if (Math.Abs(r.fRealDelta - fCmd) > 3f) r.strNote = "따라가지 못함(막힘?)";
                        }
                    }
                    else r.strNote = "3D 만 (실물 연결 없음)";
                    res = r;
                    Log(string.Format(CultureInfo.InvariantCulture, "조그 ID {0}: 명령 {1:0.0}→{2:0.0}° (Δ{3:+0.0;-0.0}), 실측 Δ{4:+0.0;-0.0}° {5}",
                                      nAxis, fFrom, fTo, fTo - fFrom, r.fRealDelta, r.strNote));
                    return true;
                }
                catch (Exception ex) { strWhy = "조그 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }

            // ── 영점 ──
            /// <summary>영점 계산 (순수) — 지금 틱이 기준각 fRefDeg 이 되는 새 영점: tickNow − round(방향 × ref × 틱/회전 ÷ 각/회전)
            /// (SimToTick 과 같은 식이라 새 영점에서 SimToTick(ref) = tickNow). 거부: 새 영점이 0~4095 밖, 새 영점에서 수동 리밋이나 쓰이는 리밋
            /// (수동 ∩ EDH $jointrange — m 이 적용된 세트의 모터면 계산돼 있다)의 틱이 [1, 4094] 밖.
            /// 경고: 45° 넘게 바뀜 (CArmMotorSet_t.IsLargeZeroChange — 틱/회전·각/회전 기준) — UI 가 한 번 더 확인한다</summary>
            public static bool PlanZero(CArmMotor_t m, int nTickNow, float fRefDeg, out int nNewCenter, out string strWarn)
            {
                nNewCenter = 0;
                strWarn = "";
                if (m == null) { strWarn = "모터 세팅 없음"; return false; }
                if (m.nMechMove <= 0 || !(m.fMechAngle > 0f)) { strWarn = "틱/회전·각/회전이 0 이하"; return false; }
                if (float.IsNaN(fRefDeg) || float.IsInfinity(fRefDeg)) { strWarn = "기준 각이 숫자가 아님"; return false; }
                int nOff = (int)Math.Round(((float)m.nMechMove * (fRefDeg * m.DirSign)) / m.fMechAngle);
                int nNew = nTickNow - nOff;
                if (nNew < 0 || nNew > 4095)
                {
                    strWarn = string.Format(CultureInfo.InvariantCulture, "새 영점 틱 {0} — 0~4095 밖 (지금 틱 {1}, 기준 {2}°)", nNew, nTickNow, CArmMotorSet_t.Fmt(fRefDeg));
                    return false;
                }
                CArmMotor_t t = m.Clone();
                t.nCenter_Evd = nNew;
                List<string> lstTick = CArmMotorSet_t.TickWindowErrors(t);   // 수동 하한·상한, 그다음 쓰이는 리밋(EDH 포함)
                if (lstTick.Count > 0)
                {
                    bool bEdh = lstTick[0].IndexOf("EDH", StringComparison.Ordinal) >= 0;
                    strWarn = "새 영점이면 " + lstTick[0] + (bEdh ? ". 리밋이나 $jointrange 를 먼저 고치십시오" : ". 리밋을 먼저 고치십시오");
                    return false;
                }
                nNewCenter = nNew;
                float fDeg;
                if (CArmMotorSet_t.IsLargeZeroChange(m, nNew, out fDeg))   // U3 — 45° 넘게 바뀌면 UI 가 한 번 더 확인한다 (틱/회전·각/회전 기준)
                    strWarn = CArmMotorSet_t.ZeroChangeText(nNew - m.nCenter_Evd, fDeg);
                return true;
            }

            /// <summary>영점 미리보기 (작업 스레드) — 정착 읽기로 그 축의 틱을 읽고 PlanZero 로 계산만 한다 (세팅은 그대로)</summary>
            public bool PreviewZero(int nAxis, float fRefDeg, out int nTickNow, out int nNewCenter, out string strWarn)
            {
                nTickNow = 0;
                nNewCenter = 0;
                strWarn = "";
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) { strWarn = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                bool bTook;
                if (!Enter(null, "영점", true, out bTook, out strWarn)) return false;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strWarn = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                    CArmMotorSet_t set = m_Motors;
                    CArmMotor_t m = (set == null) ? null : set.Get(nAxis);
                    int k = Array.IndexOf(sc.IDs, nAxis);
                    if (m == null || k < 0) { strWarn = "ID " + nAxis + " 이 연결된 모터에 없음"; return false; }
                    string w;
                    int[] anTick = sc.ReadTicks(out w);
                    if (anTick == null) { strWarn = "읽기 실패 — " + w; return false; }
                    nTickNow = anTick[k];
                    return PlanZero(m, nTickNow, fRefDeg, out nNewCenter, out strWarn);
                }
                catch (Exception ex) { strWarn = "영점 읽기 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }

            /// <summary>"현재 위치를 0 으로" 적용 (작업 스레드) — 다시 읽어 계산한 새 영점이 미리보기 값(nExpectCenter)과 3틱 넘게 다르면 거부
            /// ("확인하는 사이 관절이 움직임"). 같으면 새 세트로 ApplyMotors — 세팅을 바꾸는 순간 실물은 움직이지 않고 3D 의 그 축이 기준각이 된다.
            /// 받아들였는데 경고가 있으면 true 와 경고 (45° 넘게 바뀜, 처짐 보정·바닥 모델은 옛 영점에서 학습됨 — ApplyMotors 와 같은 U3 경고)</summary>
            public bool SetZeroHere(int nAxis, float fRefDeg, int nExpectCenter, out string strWhy)
            {
                strWhy = "";
                CScene_t sc = m_Scene;
                if (sc == null || !sc.IsConnected) { strWhy = "연결 안 됨 — 먼저 연결하십시오"; return false; }
                bool bTook;
                if (!Enter(null, "영점", true, out bTook, out strWhy)) return false;
                try
                {
                    if (m_C3d.MoveJoints_IsMoving()) { strWhy = "사용 중: 3D 모션 — 끝난 뒤 다시"; return false; }
                    CArmMotorSet_t set = m_Motors;
                    CArmMotor_t m = (set == null) ? null : set.Get(nAxis);
                    int k = Array.IndexOf(sc.IDs, nAxis);
                    if (m == null || k < 0) { strWhy = "ID " + nAxis + " 이 연결된 모터에 없음"; return false; }
                    string w;
                    int[] anTick = sc.ReadTicks(out w);
                    if (anTick == null) { strWhy = "읽기 실패 — " + w; return false; }
                    int nNew;
                    string strWarn;
                    if (!PlanZero(m, anTick[k], fRefDeg, out nNew, out strWarn)) { strWhy = strWarn; return false; }
                    if (Math.Abs(nNew - nExpectCenter) > 3)
                    { strWhy = "확인하는 사이 관절이 움직임 (" + nExpectCenter + " → " + nNew + ") — 다시 하십시오"; return false; }
                    CArmMotorSet_t s2 = set.Clone();
                    CArmMotor_t m2 = s2.Get(nAxis);
                    int nOld = m2.nCenter_Evd;
                    m2.nCenter_Evd = nNew;
                    m2.strSource = "수정";
                    if (!ApplyMotorsCore(s2, out w)) { strWhy = w; return false; }   // 처짐 보정·바닥 모델 경고(U3)는 여기서 나온다
                    List<string> lstWarn = new List<string>();
                    if (strWarn.Length > 0) lstWarn.Add(strWarn);
                    if (w.Length > 0) lstWarn.Add(w);
                    strWhy = string.Join("\r\n", lstWarn.ToArray());
                    Log("ID " + nAxis + " 영점 " + nOld + " → " + nNew + " (기준 " + CArmMotorSet_t.Fmt(fRefDeg) + "°)");
                    return true;
                }
                catch (Exception ex) { strWhy = "영점 적용 오류: " + ex.Message; return false; }
                finally { Leave(bTook); Notify(); }
            }
        }
    }
}
