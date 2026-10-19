// ================================================================
// C3d — 처짐 보정 실습 (학생용 네 단계)  2026-10-04
//   MakeUrdf 로봇팔 세팅 탭의 "처짐 보정 실습" 페이지가 버튼마다 부르는 절차. 계산·판정·파일은 모두 여기 — 앱은 부르고 그림만 그린다.
//   1. 오차 재기 (SagPrac_ErrorJob) — 보정을 모두 끈 채 (관절 모델·바닥 모델 없음 = 3D 가 믿는 그대로) 점마다 바닥을 살짝 눌러
//      끝이 처음 닿은 순간의 명령 높이(첫닿음 명령 높이 — 3D 가 '끝이 여기 있다' 고 믿는 높이)를 잰다. 자도 손 입력도 없다.
//      바닥 높이를 모르면 (처음 쓰는 로봇) 가운데 점에서 먼저 바닥을 찾는다: 옮기는 높이 아래에서 천천히 내려가며 누름 감시(벌어짐·부하)로
//      닿으면 바로 멈춤 → 그 높이로 창을 잡아 다시 눌러 첫닿음. 나머지 점은 잰 점들로 넘겨짚은 높이 ± 여유로 창을 잡는다.
//      바닥은 평평하므로 점마다 높이 차(흩어짐 = 최대 − 최소)가 곧 3D 와 실물의 차다 → <urdf>\sagcomp_practice\<시각>\before.csv
//   2. 보정 데이터 만들기 (SagPrac_BuildJob) — 먼저 가운데 점에서 바닥이 1단계와 같은지 본다 (보정 끔).
//      (a) 관절 처짐 측정: 바닥보다 충분히 높은 공중에서 반경마다 멈춰 엔코더로 실측 → 관절 모델 (⑨ 처짐보상과 같은 표본·같은 학습 —
//          옮기기는 관절 보간 안전 이동, 점 사이에서 중단을 본다). 실습 폴더에 새 파일로 (적용 중 파일은 건드리지 않는다)
//      (b) 바닥 첫닿음 측정: 1단계 높이로 창을 잡는다 (첫닿음 모델이 없어도 된다) — 로봇마다의 데이터셋 <urdf>\floor_dataset_fc.csv 에 덧붙임
//      (c) 학습: SagRun_Learn (바탕 = (a) 의 관절 모델) → <urdf>\sagcomp_results\sagcomp_model_<시각>.txt + 보고서
//   3. 효과 보기 (SagPrac_VerifyJob) — 1단계와 같은 점을 결과 모델을 켠 채 다시 눌러 after.csv. 흩어짐·Ref 기준 RMS 전·후, 점마다 값,
//      결과 파일 옆 <결과>_test.txt 에 [실습] 한 줄
//   4. 적용 (SagPrac_Apply) — SagRun_Activate (백업 + 적용). 앱이 시작할 때 적용 중 파일을 읽어 그 뒤 모든 동작이 보정된다
//   진행 상태는 <urdf>\sagcomp_practice\state.txt — 앱을 다시 켜도 이어서 (버튼 차례: 2 는 1 뒤, 3 은 2 뒤, 4 는 3 뒤).
//   로봇 정의 (10-04, COjw_12_3D_SagDef.cs): 1단계가 정의 서명(def)을 상태에 적고 2·3·4단계는 지금 정의와 대조한다 (다르면 1단계부터 — 앱을 다시 켜거나
//   sagarm.txt 를 밖에서 고쳐도). 실물 1단계는 이 로봇·정의·모터 영점으로 처음이면 펼친 자리에서 수직 눈 확인을 묻는다 (SagPrac_ErrorJob 의 eyeAsk — 상태 eyedef·eyemap).
//   수직 눈 확인만 하는 작업도 있다 (SagPrac_EyeJob, 10-05) — 펼치기 → 묻기 → 기록 → 원위치, 바닥은 누르지 않고 실습 진행(단계·폴더·결과)은 그대로.
//   이미 쓰던 설치가 1단계를 다시 하지 않고 ③ 측정·⑤ 닿기 시험·실습 2·3단계의 눈 확인 관문을 지나는 길이다.
//   그래프 자료는 SagPrac_ChartData (반경·전·후 높이·기준선) — 앱은 그리기만 한다.
// 3D 만 (bDry): 실물 없이 가상 로봇(관절 처짐 c·지레 + 데드밴드, 끝 휨, 가상 바닥 — CSagPracCfg.Sim*)으로 같은 순서를 계산하고 3D 로 보여 준다.
//   1·3단계는 3D 팔이 점마다 내려가는 모습을 보이고, 2단계는 계산만. 결과·데이터셋은 그 실습 폴더 안에만 (로봇 데이터셋·결과 목록에 섞지 않는다).
//   3D 만 실습은 4단계(적용)를 하지 않는다. 실물 실습과 3D 만 실습은 서로 이어지지 않는다 (1단계부터 같은 종류로).
// 안전 (실물): 연결 빌리기·휴지 자세·토크 시작 상태·원위치는 SagArm_Open/Close, 옮기기는 SagArm_MoveSafeCore (관절 보간·경로 확인·다리 사이 중단),
//   누름은 모두 SagComp_FloorPress (누름 감시·한도·관절 보간·하강 중 중단), 창은 계획 확인(SagRun_CheckPlanCore — IK·옮기는 높이·창 시작 ≥ 예상 + 5)을
//   지난 것만. 창 시작은 예상 닿는 높이 + 12mm 아래로 내리지 않는다 (시작까지는 감시 없이 내려간다). 한 틱 관절 변화 막이가 걸리면 멈춘다.
//   공개 함수는 예외를 던지지 않는다. 로그는 점·단계마다 (틱마다 없음), "진행 " 으로 시작하는 줄은 상태 표시용, "★" 는 경고.
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
        public partial class C3d
        {
            // ────────────────────────────────────────────────────────────
            // 설정·결과
            // ────────────────────────────────────────────────────────────

            /// <summary>처짐 보정 실습 설정 — 기본값 = OMX follower 교실용. 단위 mm / deg / ms</summary>
            public class CSagPracCfg
            {
                // ── 1·3단계 점 (요 0, 공구 피치 Ry 90 = 수직 아래) ──
                public float[] QuickRadii = new float[] { 130, 160, 190, 220, 250 };                         // 빠르게 (수업 — 5점)
                public float[] PreciseRadii = new float[] { 130, 145, 160, 175, 190, 205, 220, 235, 250 };   // 정밀 — 9점 + 아래 요 줄
                public float[] PreciseYaws = new float[] { 20, -20 };                                        // 정밀: 요 줄 (요마다 아래 반경)
                public float[] PreciseYawRadii = new float[] { 150, 190, 230 };
                public float Pitch = 90;                       // 공구 피치 Ry (90 = 수직 아래)
                // ── 창 (누름 하나의 하강 시작·한도, 명령 높이) ──
                public float AboveMm = 20;                     // 시작 = 예상 닿는 높이 + 이 값 (+ 넘겨짚기 여유)
                public float MinAboveMm = 12;                  // 시작은 예상 + 이 값 아래로 내리지 않는다 (옮길 높이가 모자라면 여유를 줄여 본다)
                public float BelowMm = 12;                     // 한도 = 예상 − 이 값 (− 넘겨짚기 여유) — 첫닿음 계획 규칙 (시작 = 예상 + 20, 한도 = 예상 − 12)
                public float RetryMm = 15;                     // 시작부터 닿아 있었으면(창이 낮음) 창을 이만큼 올려 한 번 더
                public float DescentMmPerSec = 4;              // 누름 하강 속도 (첫닿음 검출 — d14 기울기는 명령 1mm 당이라 속도와 무관하게 3mm 창)
                public float UnknownMm = 6;                    // 넘겨짚기 여유 — 잰 점 사이가 아닌 점(바깥으로 늘임·다른 요)의 창을 이만큼 넓힌다
                public float SameLineMm = 35;                  // 같은 요 줄에서 반경이 이만큼 안이면 들어 옮기지 않고 바로 다음 점으로 (측정과 같다)
                // ── 바닥 찾기 (처음 쓰는 로봇 — 1단계 가운데 점) ──
                public float FindStartZ = 100;                 // 하강 시작 (옮기는 높이 − 10 을 넘지 않는다)
                public float FindMinZ = -40;                   // 하강 한도 — 이 아래로는 내려가지 않는다 (IK 가 안 되면 10mm 씩 올린다)
                // ── 2·3단계 시작 확인 ──
                public float CheckTolMm = 8;                   // 가운데 점 바닥이 1단계와 이만큼 넘게 다르면 멈춤 (바닥·팔 자리가 바뀜)
                // ── 2단계 (a) 관절 처짐 측정 (공중) ──
                public float[] QuickSurveyRadii = new float[] { 130, 160, 190, 220, 250 };
                public float[] PreciseSurveyRadii = new float[] { 130, 150, 170, 190, 210, 230, 250 };
                public float SurveyClearMm = 40;               // 측정 높이 = 1단계 가장 높은 닿는 높이 + 이 값 (옮기는 높이를 넘지 않게)
                public float SurveyClearMinMm = 25;            // 그래도 바닥 위 이만큼은 돼야 (아니면 측정하지 않는다)
                public float PreciseSurveyStepMm = 15;         // 정밀: 한 높이 더 (측정 높이 − 이 값, 바닥 위 SurveyClearMinMm 이상일 때만)
                public int SurveySettleMs = 800;               // 멈춘 뒤 기다림 (⑨ 와 같다)
                // ── 2단계 (b) 바닥 첫닿음 측정 ──
                public float[] QuickDataRadii = new float[] { 140, 155, 170, 185, 200, 215, 230, 245 };   // 요 0 (1단계 점 사이 — 같은 점을 배우지 않는다)
                public float[] QuickDataYaws = new float[] { 15 };
                public float[] QuickDataYawRadii = new float[] { 175, 215 };
                public float[] PreciseDataRadii = new float[] { 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250 };
                public float[] PreciseDataYaws = new float[] { 15, -15 };                                 // 정밀: 요 줄마다 PreciseDataRadii 전부
                public bool DataWristHold = false;             // 손목 수직 유지 — 끔 (1·3단계와 같은 조건: 첫닿음 검출이 손목 잔차를 본다 — 유지가 손목 명령을 움직이면 헷갈린다)
                // ── 누름 공통 ──
                public CFloorProbeCfg Probe = new CFloorProbeCfg { Repeats = 1, Margin = 20, RetractAboveKneeMm = 25 };
                // ── 3D 만 가상 로봇 (실물이 없을 때 미리 보기 — 실제 처짐과 다르다) ──
                public float SimFloorZ = 40;                   // 가상 바닥 (엔코더 FK 높이, mm)
                public float[] SimJc = new float[] { 0, -0.98f, -0.57f, 0 };   // 관절 처짐 c (위치 3개 + 손목 — 관절 모델과 같은 식 e = c·지레 − h·방향)
                public float[] SimJh = new float[] { 0, 0.5f, 0.5f, 0.3f };    // 데드밴드 h (deg)
                public float SimExtMm = 6;                     // 엔코더 밖 휨 — 끝이 FK 보다 이만큼(반경 120 → 0, 260 → 이 값) 더 낮다
                public float SimRippleMm = 1.5f;               // 그 위에 반경을 따라 출렁이는 휨 (진폭, 주기 약 125mm) — 모델이 다 맞히지 못하는 몫
                public float SimNoiseMm = 1.0f;                // 자리마다 다른 작은 높이 차 (±, 같은 자리는 늘 같다 — 바닥 결·나사 놀음 흉내, 모델이 배우지 못한다)
                /// <summary>복사본 (배열도 새로)</summary>
                public CSagPracCfg Clone()
                {
                    var c = (CSagPracCfg)MemberwiseClone();
                    Func<float[], float[]> A = a => a == null ? null : (float[])a.Clone();
                    c.QuickRadii = A(QuickRadii); c.PreciseRadii = A(PreciseRadii); c.PreciseYaws = A(PreciseYaws); c.PreciseYawRadii = A(PreciseYawRadii);
                    c.QuickSurveyRadii = A(QuickSurveyRadii); c.PreciseSurveyRadii = A(PreciseSurveyRadii);
                    c.QuickDataRadii = A(QuickDataRadii); c.QuickDataYaws = A(QuickDataYaws); c.QuickDataYawRadii = A(QuickDataYawRadii);
                    c.PreciseDataRadii = A(PreciseDataRadii); c.PreciseDataYaws = A(PreciseDataYaws);
                    c.SimJc = A(SimJc); c.SimJh = A(SimJh);
                    c.Probe = (Probe != null ? Probe : new CFloorProbeCfg { Repeats = 1, Margin = 20, RetractAboveKneeMm = 25 }).Clone();
                    return c;
                }
            }

            /// <summary>실습 점 하나 (1·3단계 — before.csv / after.csv 한 행)</summary>
            public class CSagPracPoint
            {
                public int Idx;
                public float R, Yaw, X, Y, Pitch = 90;
                public float Est = float.NaN;          // 누르기 전 예상 닿는 높이 (명령, mm)
                public float Touch = float.NaN;        // 잰 첫닿음 명령 높이 (3D 가 믿는 끝 높이, mm) — 닿지 않았으면 NaN
                public float Fk = float.NaN;           // 그때 엔코더 FK 높이
                public float Knee = float.NaN;         // 누름 꺾임점 명령 높이 (참고)
                public string Kind = "";               // 첫닿음 판정 F / H (첫닿음 찾음) / C / X / 빈칸
                public string Note = "";
                internal bool NoTouch;                 // 마지막 누름이 한도까지 첫닿음 없이 내려갔다
                internal bool StartContact;            // 마지막 누름이 시작부터 닿아 있었다 (창이 낮음)
                internal float[] TouchQ, TouchA;       // 첫닿음 순간 이상·실측 관절각 (2단계 데이터셋 행)
                public int Tries;                      // 다시 누른 수
            }

            /// <summary>실습 진행 상태 — <urdf>\sagcomp_practice\state.txt (key=value, UTF-8 BOM 없음, CRLF)</summary>
            public class CSagPracState
            {
                public string Folder = "";             // 실습 폴더 이름 (yyyyMMdd_HHmmss[_3d])
                public string FolderDir = "";         // 그 전체 경로 (읽을 때 채운다)
                public bool Dry, Precise, Applied;
                public int Step;                       // 마친 단계 0~4
                public string JointPath = "", ResultPath = "";   // 2단계가 만든 관절 모델·결과 파일 (전체 경로)
                public string Line1 = "", Line2 = "", Line3 = "", Line4 = "";   // 단계마다 마지막 결과 한 줄
                public string Def = "";                // 1단계를 한 로봇 정의의 서명 (SagDef_Sig — 2·3·4단계가 대조, 빈 값 = 옛 상태 = 내장 OMX)
                public string EyeDef = "", EyeMap = "", EyeTime = "";   // 수직 눈 확인을 한 정의 서명·모터 매핑 키·시각 (둘 중 하나가 다르면 다시 묻는다)
                public bool Exists;                    // 파일이 있었다
            }

            /// <summary>실습 단계 하나의 결과</summary>
            public class CSagPracResult
            {
                public int Step;
                public bool Ok, Stopped, Dry, Ran, RestOk;
                public string Why = "", Line = "", Report = "";
                public string FolderDir, RunDir, CsvPath, ReportPath;
                public List<CSagPracPoint> Points = new List<CSagPracPoint>();
                public float FloorFoundZ = float.NaN;  // 1단계: 바닥 찾기로 처음 닿은 높이 (명령, mm)
                public float CheckDiffMm = float.NaN;  // 2·3단계: 시작 확인 — 가운데 점 바닥 − 1단계 값
                public double SpreadBefore = double.NaN, SpreadAfter = double.NaN;   // 흩어짐 (최대 − 최소, 같은 점끼리)
                public double RmsBefore = double.NaN, RmsAfter = double.NaN;         // 기준선 Ref 에서의 RMS (Ref 가 없으면 평균에서)
                public double Ref = double.NaN, Reduction = double.NaN;              // 결과 모델 Ref, 흩어짐 줄어든 비율 (%)
                public int NTouched;
                // 2단계
                public string JointPath, ResultPath, DatasetPath, JointLine = "";
                public int SurveySamples, DataRows, DatasetRowsAdded;
                public CSagLearnResult Learn;
                public int[] UncheckedAxes = new int[0];
                public string EyeAns = "";             // 1단계 실물 수직 눈 확인 답 ("" 묻지 않음 / 예 / 아니오 / 중단 / 생략 — 묻는 곳 없음)
            }

            /// <summary>그래프 자료 — 반경마다 전(빨강)·후(초록) 첫닿음 명령 높이와 평평한 바닥 기준선. 앱은 그리기만 한다</summary>
            public class CSagPracChart
            {
                public bool HasBefore, HasAfter, Dry;
                public int Step;
                public float[] R = new float[0], Yaw = new float[0], Before = new float[0], After = new float[0];   // 점 순서 (닿지 않은 값 NaN)
                public double RefZ = double.NaN;       // 기준선 — 결과 모델 Ref (없으면 '전' 평균): 바닥이 평평하면 모두 이 높이여야 한다
                public string RefLabel = "";
                public double SpreadBefore = double.NaN, SpreadAfter = double.NaN, Reduction = double.NaN;
                public string Line = "";               // "흩어짐 38.1mm → 4.2mm (89% 줄어듦)" — 3단계 전이면 "흩어짐 38.1mm"
                public string Folder = "";              // 실습 폴더 이름
            }

            // ────────────────────────────────────────────────────────────
            // 경로·상태
            // ────────────────────────────────────────────────────────────
            public const string SAGPRAC_DIR = "sagcomp_practice";
            private const string SAGPRAC_STATE = "state.txt";
            private const string SAGPRAC_OWNER = "처짐 실습";
            /// <summary><urdf>\sagcomp_practice</summary>
            public static string SagPrac_Dir(string strUrdfDir) { return SagRun_Join(strUrdfDir, SAGPRAC_DIR); }

            /// <summary>진행 상태 읽기 — 없으면 Step 0. 실습 폴더가 없어졌으면 Step 0 으로 본다. 예외 없음</summary>
            public static CSagPracState SagPrac_LoadState(string strUrdfDir)
            {
                var s = new CSagPracState();
                try
                {
                    string p = Path.Combine(SagPrac_Dir(strUrdfDir), SAGPRAC_STATE);
                    if (!File.Exists(p)) return s;
                    s.Exists = true;
                    var ci = CultureInfo.InvariantCulture;
                    foreach (string raw in File.ReadAllLines(p, Encoding.UTF8))
                    {
                        string ln = raw.Trim();
                        if (ln.Length == 0 || ln.StartsWith("//")) continue;
                        int eq = ln.IndexOf('='); if (eq <= 0) continue;
                        string k = ln.Substring(0, eq).Trim().ToLowerInvariant(), v = ln.Substring(eq + 1).Trim();
                        int n;
                        switch (k)
                        {
                            case "folder": s.Folder = v; break;
                            case "dry": s.Dry = v == "1"; break;
                            case "precise": s.Precise = v == "1"; break;
                            case "applied": s.Applied = v == "1"; break;
                            case "step": if (int.TryParse(v, NumberStyles.Integer, ci, out n)) s.Step = Math.Max(0, Math.Min(4, n)); break;
                            case "joint": s.JointPath = SagRun_Resolve(strUrdfDir, v, ""); break;
                            case "result": s.ResultPath = SagRun_Resolve(strUrdfDir, v, ""); break;
                            case "line1": s.Line1 = v; break;
                            case "line2": s.Line2 = v; break;
                            case "line3": s.Line3 = v; break;
                            case "line4": s.Line4 = v; break;
                            case "def": s.Def = v; break;
                            case "eyedef": s.EyeDef = v; break;
                            case "eyemap": s.EyeMap = v; break;
                            case "eyetime": s.EyeTime = v; break;
                        }
                    }
                    s.FolderDir = s.Folder.Length > 0 ? Path.Combine(SagPrac_Dir(strUrdfDir), s.Folder) : "";
                    if (s.FolderDir.Length == 0 || !Directory.Exists(s.FolderDir)) { s.Step = 0; s.Applied = false; }
                }
                catch { s.Step = 0; }
                return s;
            }
            // 상태 쓰기 (UTF-8 BOM 없음, CRLF) — 실패는 로그만
            private static bool SagPrac_SaveState(string strUrdfDir, CSagPracState s, CSim2Real.DLog L)
            {
                try
                {
                    string dir = SagPrac_Dir(strUrdfDir);
                    Directory.CreateDirectory(dir);
                    Func<string, string> One = v => (v ?? "").Replace("\r", " ").Replace("\n", " ");
                    var sb = new StringBuilder();
                    sb.Append("// 처짐 보정 실습 진행 상태 — 실습 페이지가 단계마다 씀 (step = 마친 단계)\r\n");
                    sb.Append("folder=").Append(One(s.Folder)).Append("\r\n");
                    sb.Append("dry=").Append(s.Dry ? "1" : "0").Append("\r\n");
                    sb.Append("precise=").Append(s.Precise ? "1" : "0").Append("\r\n");
                    sb.Append("step=").Append(s.Step.ToString(CultureInfo.InvariantCulture)).Append("\r\n");
                    sb.Append("joint=").Append(One(SagRun_Relative(strUrdfDir, s.JointPath ?? ""))).Append("\r\n");
                    sb.Append("result=").Append(One(SagRun_Relative(strUrdfDir, s.ResultPath ?? ""))).Append("\r\n");
                    sb.Append("applied=").Append(s.Applied ? "1" : "0").Append("\r\n");
                    sb.Append("line1=").Append(One(s.Line1)).Append("\r\n");
                    sb.Append("line2=").Append(One(s.Line2)).Append("\r\n");
                    sb.Append("line3=").Append(One(s.Line3)).Append("\r\n");
                    sb.Append("line4=").Append(One(s.Line4)).Append("\r\n");
                    sb.Append("def=").Append(One(s.Def)).Append("\r\n");                // 1단계를 한 로봇 정의 (2·3·4단계가 대조)
                    sb.Append("eyedef=").Append(One(s.EyeDef)).Append("\r\n");          // 수직 눈 확인 — 정의 서명·모터 매핑 키·시각
                    sb.Append("eyemap=").Append(One(s.EyeMap)).Append("\r\n");
                    sb.Append("eyetime=").Append(One(s.EyeTime)).Append("\r\n");
                    string p = Path.Combine(dir, SAGPRAC_STATE), tmp = p + ".tmp";
                    File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
                    if (File.Exists(p)) File.Replace(tmp, p, null); else File.Move(tmp, p);
                    return true;
                }
                catch (Exception ex) { if (L != null) L("★실습 상태를 쓰지 못함: " + SagRun_Msg(ex)); return false; }
            }

            /// <summary>1·3단계 점 — 빠르게: QuickRadii (요 0), 정밀: PreciseRadii (요 0) + PreciseYaws × PreciseYawRadii. 피치 = cfg.Pitch</summary>
            public static List<CSagPracPoint> SagPrac_MakePoints(CSagPracCfg cfg, bool bPrecise)
            {
                if (cfg == null) cfg = new CSagPracCfg();
                var l = new List<CSagPracPoint>();
                Action<float, float> Add = (r, yaw) =>
                {
                    double a = yaw * Math.PI / 180.0;
                    l.Add(new CSagPracPoint { Idx = l.Count, R = r, Yaw = yaw, X = (float)(r * Math.Cos(a)), Y = (float)(r * Math.Sin(a)), Pitch = cfg.Pitch });
                };
                foreach (float r in (bPrecise ? cfg.PreciseRadii : cfg.QuickRadii) ?? new float[0]) Add(r, 0);
                if (bPrecise && cfg.PreciseYaws != null && cfg.PreciseYawRadii != null)
                    foreach (float y in cfg.PreciseYaws) foreach (float r in cfg.PreciseYawRadii) Add(r, y);
                return l;
            }

            // ────────────────────────────────────────────────────────────
            // 1. 오차 재기
            // ────────────────────────────────────────────────────────────

            /// <summary>1단계 오차 재기 (블로킹 — 작업 스레드). 보정을 모두 끈 채 점마다 첫닿음 명령 높이를 잰다 (헤더 설명).
            /// 새 실습 폴더 <urdf>\sagcomp_practice\<yyyyMMdd_HHmmss>[_3d] 를 만들고 before.csv·before_summary.txt, 작업 기록은 그 안 1_error\.
            /// 성공하면 진행 상태를 1단계로 (전 실습의 2·3·4 단계는 지운다). 실물은 연결이 있어야 한다 (없으면 움직이지 않고 거절). 예외 없음.
            /// 수직 눈 확인을 묻지 않는 판 (eyeAsk = null — 확인이 필요해도 로그 "★수직 눈 확인 생략" 뒤 예전처럼 진행. 기록은 남지 않으므로
            /// 실물 2·3단계·③ 측정·⑤ 닿기 시험은 눈 확인 전으로 거절된다 — 묻는 판으로 1단계를 다시 하거나 눈 확인만 하는 SagPrac_EyeJob)</summary>
            public CSagPracResult SagPrac_ErrorJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_ErrorJob(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, null, log, stop);
            }
            /// <summary>1단계 오차 재기 — 수직 눈 확인 (10-04): 실물이고 이 로봇·정의·모터 영점으로 아직 확인하지 않았으면 (SagPrac_EyeNeeded) 펼치기가 끝난 자리
            /// (가운데 점 위, 옮기는 높이, 바닥으로 수직 자세 — 바닥 찾기 시작점보다 높다, 따로 움직이지 않는다) 에서 eyeAsk 로 "실물 펜이 수직입니까?" 를 묻는다
            /// (작업 스레드에서 부른다 — 앱은 UI 스레드로 Invoke 해 대화창). 예 → 기록 (state.txt eyedef·eyemap·eyetime) 뒤 바닥 찾기, 아니오 → 누르지 않고 원위치
            /// (res.EyeAns "아니오", Why — 모터 세팅에서 영점), 중단·예외 → 중단. eyeAsk 가 null 이면 묻지 않고 예전처럼 (로그 ★, EyeAns "생략"). 3D 만은 묻지 않는다</summary>
            public CSagPracResult SagPrac_ErrorJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_ErrorJobBody(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, eyeAsk, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_ErrorJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                        DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 1, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 폴더를 만들거나 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
                    string eyeDef = "", eyeMap = "", eyeText;
                    bool bEye = !bDry && SagPrac_EyeNeededCore(strUrdfDir, conn, armCfg, out eyeDef, out eyeMap, out eyeText);
                    List<CSagPracPoint> pts = SagPrac_MakePoints(cfg, bPrecise);
                    if (pts.Count < 3) return SagPrac_Refuse(res, "점이 3개 미만 — 설정의 반경 확인", L);
                    int iMid = SagPrac_MidIndex(pts);
                    DateTime t0 = DateTime.Now;
                    string strFolderNew = SagPrac_NewFolder(strUrdfDir, t0, bDry);
                    res.FolderDir = strFolderNew;
                    res.RunDir = Path.Combine(strFolderNew, "1_error");
                    Directory.CreateDirectory(res.RunDir);
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    L((bDry ? "3D 만 — " : "") + "1. 오차 재기 시작 — " + t0.ToString("yyyy-MM-dd HH:mm:ss", ci) + string.Format(ci, " · 점 {0} ({1})", pts.Count, bPrecise ? "정밀" : "빠르게"));
                    L("보정을 모두 끔 (관절 모델·바닥 모델 없음 — 3D 가 믿는 높이 그대로). 점마다 바닥을 살짝 눌러 처음 닿은 명령 높이를 잽니다");
                    // 3D 만 — 가상 로봇의 닿는 높이 (보정 없음)
                    var sim = new Dictionary<int, float>();
                    if (bDry)
                    {
                        foreach (CSagPracPoint p in pts)
                        {
                            float z, fk; float[] qi, qa;
                            if (SagPrac_SimTouch(kin, armCfg, cfg, p.X, p.Y, p.Pitch, null, out z, out fk, out qi, out qa)) sim[p.Idx] = z;
                        }
                        if (!sim.ContainsKey(pts[iMid].Idx)) return SagPrac_Refuse(res, "가상 로봇 계산 실패 (IK) — 반경 확인", L);
                        L(string.Format(ci, "3D 만 — 가상 로봇: 바닥 FK {0:0.#}mm, 관절 처짐 c {1}, 끝 휨 {2:0.#}mm (실제 로봇과 다름 — 미리 보기)", cfg.SimFloorZ,
                            string.Join("/", Array.ConvertAll(cfg.SimJc, v => v.ToString("0.##", ci))), cfg.SimExtMm));
                    }
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    SagArm_SetMoveModel(arm, null, L);                       // 옮기기도 보정 없이 (창·옮기는 높이는 보정 없는 닿는 높이로 잡는다)
                    var run = SagPrac_NewRun(arm, cfg, res.RunDir, L, stop);
                    CSagPracPoint mid = pts[iMid];
                    if (!SagArm_Unfold(arm, mid.X, mid.Y, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        var known = new List<CSagPracPoint>();
                        // ── 바닥 찾기 + 가운데 점 ── (첫닿음에서 멈춤 — 단단한 정지까지 누르지 않는다)
                        float top = Math.Min(cfg.FindStartZ, arm.Cfg.SafeZ - 10);
                        bool bMid = false;
                        float est;
                        // ── 수직 눈 확인 (실물, 이 로봇·정의·영점으로 처음 한 번) — 펼치기가 끝난 자리 (가운데 점 위, 옮기는 높이, 수직 자세) 에서 묻는다 ──
                        if (bEye) SagPrac_EyeCheck(run, eyeAsk, strUrdfDir, eyeDef, eyeMap, res);
                        if (bDry) { est = sim[mid.Idx]; L(string.Format(ci, "진행 바닥 찾기 — 3D 만: 가상 바닥에 닿는 높이 z={0:F1}", est)); }
                        else if (run.Stopped || run.Why.Length > 0) est = float.NaN;         // 눈 확인에서 멈춤 — 누르지 않고 원위치
                        else
                        {
                            L(string.Format(ci, "진행 바닥 찾기 — 가운데 점 r={0:0.#} 에서 z={1:0.#} 부터 천천히({2:0.#}mm/s) 내려가며 첫닿음에서 멈춤", mid.R, top, cfg.DescentMmPerSec));
                            est = SagPrac_FindFloor(run, mid, top);
                            if (!run.Stopped && run.Why.Length == 0 && float.IsNaN(est))
                                run.Why = "바닥을 찾지 못함 — " + (mid.Note.Length > 0 ? mid.Note : "한도까지 첫닿음 없음") + " (바닥이 너무 낮거나 팔이 닿지 않는 자리)";
                            if (!float.IsNaN(est)) L(string.Format(ci, "바닥 찾음: 첫닿음 명령 z={0:F1} → 그 높이로 창(위 {1:0}, 아래 {2:0})을 잡아 가운데 점을 잼", est, cfg.AboveMm, cfg.BelowMm));
                        }
                        if (!run.Stopped && run.Why.Length == 0 && !float.IsNaN(est))
                        {
                            res.FloorFoundZ = est;
                            mid.Est = est; mid.Note = "";
                            ESagPracPress o = SagPrac_PressAt(run, mid, est, est, 0, null, false, bDry ? est : float.NaN);
                            if (o == ESagPracPress.Ok && !float.IsNaN(mid.Touch)) { known.Add(mid); bMid = true; }
                            else if (o == ESagPracPress.Ok || o == ESagPracPress.Skip) run.Why = "가운데 점에서 첫닿음을 재지 못함 (" + mid.Note + ")";
                        }
                        // ── 나머지 점 — 잰 점으로 넘겨짚은 높이 ± 여유 ──
                        if (bMid)
                        {
                            List<CSagPracPoint> order = SagPrac_Order(pts, iMid);
                            int nDone = 1;
                            L(string.Format(ci, "진행 1/{0} — r={1:0.#} 요 {2:0.#}: 닿은 높이 {3:F1}", pts.Count, mid.R, mid.Yaw, mid.Touch));
                            foreach (CSagPracPoint p in order)
                            {
                                if (ReferenceEquals(p, mid)) continue;
                                if (SagRun_Stopped(stop)) { SagPrac_StopLog(run, "점 사이"); break; }
                                float u; float estP = SagPrac_Estimate(known, p.R, p.Yaw, cfg.UnknownMm, out u);
                                if (float.IsNaN(estP)) { p.Note = "예상 높이 없음 — 뺌"; continue; }
                                p.Est = estP;
                                float dz = 0; bool bSim = bDry && sim.TryGetValue(p.Idx, out dz);
                                if (bDry && !bSim) { p.Note = "가상 로봇 계산 실패 — 뺌"; continue; }
                                ESagPracPress o = SagPrac_PressAt(run, p, estP, estP, u, null, false, bSim ? dz : float.NaN);
                                if (o == ESagPracPress.Stop || o == ESagPracPress.Fail) break;
                                nDone++;
                                if (!float.IsNaN(p.Touch)) known.Add(p);
                                L(string.Format(ci, "진행 {0}/{1} — r={2:0.#} 요 {3:0.#}: {4}", nDone, pts.Count, p.R, p.Yaw,
                                    float.IsNaN(p.Touch) ? "닿지 않음 (" + p.Note + ")" : string.Format(ci, "닿은 높이 {0:F1} (예상 {1:F1})", p.Touch, estP)));
                            }
                        }
                    }
                    res.Stopped = res.Stopped || run.Stopped;
                    if (run.Why.Length > 0) res.Why = SagRun_WhyJoin(res.Why, run.Why);
                    res.Points = pts;
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                if (res.Ran) SagPrac_FinishError(res, strUrdfDir, bPrecise, armCfg, L);
                return res;
            }
            // 1단계 끝 — before.csv·요약, 상태 (3점 이상 닿았고 멈추지 않았으면 1단계 마침)
            private void SagPrac_FinishError(CSagPracResult res, string strUrdfDir, bool bPrecise, CSagArmCfg armCfg, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    var v = new List<double>();
                    foreach (CSagPracPoint p in res.Points) if (!float.IsNaN(p.Touch)) v.Add(p.Touch);
                    res.NTouched = v.Count;
                    if (v.Count >= 2) { res.SpreadBefore = SagPrac_Spread(v); res.RmsBefore = SagPrac_Rms(v, SagPrac_Mean(v)); }
                    res.CsvPath = Path.Combine(res.FolderDir, "before.csv");
                    SagRun_WriteText(res.CsvPath, SagPrac_PointsCsv(res.Points));
                    res.Ok = !res.Stopped && res.Why.Length == 0 && res.NTouched >= 3;
                    if (!res.Stopped && res.Why.Length == 0 && res.NTouched < 3) res.Why = string.Format(ci, "닿은 점이 {0}개 — 3개 이상 필요", res.NTouched);
                    int nMiss = res.Points.Count - res.NTouched;
                    res.Line = res.Ok ? string.Format(ci, "{0}오차: 흩어짐 {1:F1}mm ({2}점, {3:F1}~{4:F1}){6}{5}", res.Dry ? "3D 만 · " : "", res.SpreadBefore, res.NTouched,
                                                      SagPrac_Min(v), SagPrac_Max(v), float.IsNaN(res.FloorFoundZ) ? "" : string.Format(ci, " · 바닥 찾기 z={0:F1}", res.FloorFoundZ),
                                                      nMiss > 0 ? string.Format(ci, " · 재지 못한 점 {0} (흩어짐에서 뺌)", nMiss) : "")
                                     : (res.Stopped ? "1. 오차 재기 중단" : "1. 오차 재기 실패 — " + res.Why);
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 처짐 보정 실습 1. 오차 재기 (3D 만 — 가상 로봇) ===" : "=== 처짐 보정 실습 1. 오차 재기 ===");
                    sb.AppendLine("실습 폴더: " + res.FolderDir);
                    sb.AppendLine("보정: 모두 끔 (관절 모델·바닥 모델 없음) — 명령 높이 = 3D 가 믿는 끝 높이");
                    sb.AppendLine("잰 값: 첫닿음 (끝이 바닥에 처음 닿은 순간 — 손목 잔차가 커지기 시작한 꺾임), 닿으면 바로 멈춤 (단단한 정지까지 누르지 않음). 재지 못한 점은 흩어짐에서 뺀다");
                    if (!float.IsNaN(res.FloorFoundZ)) sb.AppendLine(string.Format(ci, "바닥 찾기: 명령 z={0:F1} 에서 처음 닿음", res.FloorFoundZ));
                    sb.AppendLine("     r     요   예상     닿은 높이   FK      판정  비고");
                    foreach (CSagPracPoint p in res.Points)
                        sb.AppendLine(string.Format(ci, "  {0,5:0.#} {1,5:0.#}  {2,6}  {3,9}  {4,6}  {5,-4}  {6}", p.R, p.Yaw, SagPrac_V(p.Est), SagPrac_V(p.Touch), SagPrac_V(p.Fk),
                            p.Kind.Length > 0 ? p.Kind : "—", p.Note));
                    sb.AppendLine(res.NTouched >= 2 ? string.Format(ci, "흩어짐 (최대 − 최소): {0:F1}mm — 바닥은 평평하므로 이것이 3D 와 실물의 높이 차", res.SpreadBefore) : "흩어짐: 잴 수 없음 (닿은 점 부족)");
                    if (res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!res.Dry) sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    if (res.Why.Length > 0) sb.AppendLine("★" + res.Why);
                    sb.AppendLine(res.Line);
                    res.Report = sb.ToString();
                    res.ReportPath = Path.Combine(res.FolderDir, "before_summary.txt");
                    SagRun_WriteText(res.ReportPath, res.Report);
                    if (res.Ok)
                    {
                        CSagPracState old = SagPrac_LoadState(strUrdfDir);            // 수직 눈 확인 기록은 넘겨받는다 (로봇·정의·영점마다 한 번)
                        var st = new CSagPracState { Folder = Path.GetFileName(res.FolderDir), Dry = res.Dry, Precise = bPrecise, Step = 1, Line1 = res.Line,
                                                     Def = SagDef_Sig(armCfg), EyeDef = old.EyeDef, EyeMap = old.EyeMap, EyeTime = old.EyeTime };
                        SagPrac_SaveState(strUrdfDir, st, L);
                    }
                    L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★1단계 정리 오류: " + SagRun_Msg(ex)); }
            }

            // 바닥 찾기 (실물) — 가운데 점에서 top 부터 한도(FindMinZ — IK 가 안 되면 10mm 씩 올림)까지 천천히 내려가며 첫닿음에서 멈춤
            //   (실시간 첫닿음 검출 — 부하·벌어짐 안전 정지도 그대로). 반환 = 첫닿음 명령 높이 (이상 명령), 못 찾았으면 NaN (까닭은 mid.Note).
            //   멈춤·실패는 run.Stopped·run.Why
            private float SagPrac_FindFloor(CSagPracRun run, CSagPracPoint mid, float top)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagPracCfg cfg = run.Cfg;
                float bottom = cfg.FindMinZ;
                float retract = SagPrac_Retract(cfg);
                CFloorPlanRow ok = null;
                for (; bottom <= top - 30; bottom += 10)
                {
                    CFloorPlanRow r = SagPrac_Row(mid, top, bottom, float.NaN);
                    string rep;
                    SagRun_CheckPlanCore(new List<CFloorPlanRow> { r }, run.Arm.Cfg, retract, false, null, false, false, out rep);
                    if (r.IkOk) { ok = r; break; }
                }
                if (ok == null) { run.Why = string.Format(ci, "바닥 찾기 창 (z {0:0.#}~{1:0.#}) 을 IK 로 갈 수 없음 — 옮기는 높이·자리 확인", top, cfg.FindMinZ); return float.NaN; }
                if (bottom > cfg.FindMinZ) run.L(string.Format(ci, "바닥 찾기 한도: z={0:0.#} (그 아래는 IK 로 못 감)", bottom));
                CSagPracTouch t;
                mid.Note = "";
                ESagPracPress o = SagPrac_Press(run, mid, ok, null, false, float.NaN, out t);
                if (o != ESagPracPress.Ok || t == null) { if (o == ESagPracPress.Skip && run.Why.Length == 0) run.Why = "바닥 찾기 누름을 하지 못함 — " + mid.Note; return float.NaN; }
                SagPrac_TakeTouch(t, mid, run.L);
                float z = mid.Touch;
                mid.Touch = mid.Fk = float.NaN; mid.Kind = "";                 // 가운데 점 값은 이어지는 창으로 다시 잰다
                return z;
            }

            // ── 수직 눈 확인 (10-04) — 영점 오차(실물 펜이 기욺)는 엔코더로 볼 수 없어 새 로봇·정의·영점의 첫 누름 전에 사람이 한 번 본다 ──
            /// <summary>수직 눈 확인 답 — 예 (수직), 아니오 (기울어짐 — 누르지 않고 원위치), 중단</summary>
            public enum ESagEyeAns { Yes, No, Stop }
            /// <summary>수직 눈 확인 — 작업 스레드에서 부른다 (앱은 UI 스레드로 Invoke 해 대화창). 반환 = 사용자 답</summary>
            public delegate ESagEyeAns DSagEyeAsk(string strPrompt);

            /// <summary>이 로봇·정의·모터 영점으로 수직 눈 확인을 해야 하나 (실습 상태 state.txt 의 eyedef = 지금 정의 서명 SagDef_Sig, eyemap = 지금 모터 매핑 키 —
            /// 둘 중 하나라도 다르면 다시 묻는다). conn 이 없거나 모터 세팅이 없으면 영점은 보지 않는다. text = 실습 카드용 쉬운 말 ("수직 눈 확인 끝 (시각)" / "수직 눈 확인 전"). 예외 없음</summary>
            public bool SagPrac_EyeNeeded(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, out string text)
            {
                string d, m;
                return SagPrac_EyeNeededCore(strUrdfDir, conn, arm, out d, out m, out text);
            }
            private bool SagPrac_EyeNeededCore(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, out string eyeDef, out string eyeMap, out string text)
            {
                eyeDef = ""; eyeMap = ""; text = "수직 눈 확인 전";
                try
                {
                    if (arm == null) arm = new CSagArmCfg();
                    eyeDef = SagDef_Sig(arm);
                    eyeMap = SagPrac_EyeMapKey(conn, arm);
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    bool bDef = st.EyeDef.Length > 0 && string.Equals(st.EyeDef, eyeDef, StringComparison.Ordinal);
                    bool bMap = eyeMap.Length == 0 || string.Equals(st.EyeMap, eyeMap, StringComparison.Ordinal);
                    if (bDef && bMap) { text = "수직 눈 확인 끝" + (st.EyeTime.Length > 0 ? " (" + st.EyeTime + ")" : ""); return false; }
                    return true;
                }
                catch { return true; }
            }
            /// <summary>수직 눈 확인 전 거절 문구 (실물 바닥 작업 입구 — SagPrac_EyeGate). 하는 곳 = 로봇팔 세팅 탭 '로봇 정의' 페이지의 [수직 눈 확인] (SagPrac_EyeJob)</summary>
            public const string SAGPRAC_EYE_FIRST = "수직 눈 확인 전 (이 로봇·정의·모터 영점으로 처음 한 번) — 로봇팔 세팅 탭 '로봇 정의' 페이지의 [수직 눈 확인] 으로 먼저: " +
                                                    "팔을 바닥 위 옮기는 높이에서 수직 자세로 펼쳐 실물 펜이 바닥에 수직인지 묻는다 (바닥은 누르지 않고 실습 진행은 그대로)";
            // 실물 바닥 작업 입구 (③ 측정 SagRun_MeasureJob·⑤ 닿기 시험 SagRun_TestJob·실습 2·3단계) — 움직이기 전에. 3D 만이면 통과.
            //   로봇 정의 확인 (SagDef_Gate — 정의가 틀렸으면 그 까닭이 먼저) 뒤, 이 로봇·정의·모터 영점으로 수직 눈 확인을 하지 않았으면 (SagPrac_EyeNeeded —
            //   state.txt 의 eyedef·eyemap 이 지금과 다름) 거절. 영점 오차(실물 펜이 기욺)는 엔코더로 볼 수 없어 사람이 한 번 봐야 한다 — 묻는 곳은
            //   눈 확인만 하는 작업 SagPrac_EyeJob (앱: '로봇 정의' 페이지 [수직 눈 확인]) 과 실습 1단계 (SagPrac_ErrorJob 이 펼친 자리에서).
            //   모터 영점·정의를 고쳤으면 다시 필요하다
            private bool SagPrac_EyeGate(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, bool bDry, out string why)
            {
                why = "";
                if (bDry) return true;
                if (!SagDef_Gate(arm, false, null, out why)) { why = "로봇 정의 — " + why; return false; }
                string t;
                if (!SagPrac_EyeNeeded(strUrdfDir, conn, arm, out t)) { why = ""; return true; }
                why = SAGPRAC_EYE_FIRST;
                return false;
            }
            // 눈 확인의 모터 매핑 키 — 연결의 모터 세팅에서 움직이는 관절(위치 3 + 손목)만 (방향·영점·틱 — CArmMotorSet_t.MappingKey). 없으면 ""
            private static string SagPrac_EyeMapKey(CArmConn_t conn, CSagArmCfg arm)
            {
                try
                {
                    if (conn == null) return "";
                    CArmMotorSet_t set = conn.Motors;
                    if (set == null) return "";
                    int[] taken;
                    CArmMotorSet_t sub = set.MergeFor(arm.MoveIDs, out taken);
                    return sub == null ? "" : (sub.MappingKey() ?? "");
                }
                catch { return ""; }
            }
            // 눈 확인 기록 — state.txt 의 eyedef·eyemap·eyetime 줄만 바꾸고 (없으면 끝에 더한다) 다른 줄은 바이트까지 그대로 둔 채 원자 교체.
            //   실습 진행 (단계·폴더·결과·적용·정의 서명) 은 건드리지 않는다 — 읽고 다시 쓰는 길(SagPrac_SaveState)은 폴더가 없어진 상태를 단계 0 으로 적으므로
            //   쓰지 않는다. 상태 파일이 없으면 단계 0 상태로 만든다. 실패는 로그만 (false)
            private static bool SagPrac_SaveEye(string strUrdfDir, string eyeDef, string eyeMap, CSim2Real.DLog L)
            {
                string strTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                Func<string, string> One = v => (v ?? "").Replace("\r", " ").Replace("\n", " ");
                try
                {
                    string p = Path.Combine(SagPrac_Dir(strUrdfDir), SAGPRAC_STATE);
                    if (!File.Exists(p)) return SagPrac_SaveState(strUrdfDir, new CSagPracState { EyeDef = One(eyeDef), EyeMap = One(eyeMap), EyeTime = strTime }, L);
                    byte[] raw = File.ReadAllBytes(p);
                    bool bBom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
                    string text = new UTF8Encoding(false).GetString(raw, bBom ? 3 : 0, raw.Length - (bBom ? 3 : 0));
                    var vals = new Dictionary<string, string> { { "eyedef", One(eyeDef) }, { "eyemap", One(eyeMap) }, { "eyetime", strTime } };
                    var done = new HashSet<string>();
                    string[] lines = text.Split('\n');                          // 줄 끝 '\r' 는 줄에 남긴다 (CRLF·LF 그대로)
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string ln = lines[i].Trim();
                        if (ln.Length == 0 || ln.StartsWith("//")) continue;
                        int eq = ln.IndexOf('='); if (eq <= 0) continue;
                        string k = ln.Substring(0, eq).Trim().ToLowerInvariant();
                        string v;
                        if (!vals.TryGetValue(k, out v)) continue;
                        lines[i] = k + "=" + v + (lines[i].EndsWith("\r") ? "\r" : "");   // 같은 키가 여러 줄이면 모두 (읽기는 마지막 줄을 쓴다)
                        done.Add(k);
                    }
                    var sb = new StringBuilder(string.Join("\n", lines));
                    string nl = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0 || text.IndexOf('\n') < 0 ? "\r\n" : "\n";
                    foreach (string k in new[] { "eyedef", "eyemap", "eyetime" })
                    {
                        if (done.Contains(k)) continue;
                        if (sb.Length > 0 && sb[sb.Length - 1] != '\n') sb.Append(nl);
                        sb.Append(k).Append('=').Append(vals[k]).Append(nl);
                    }
                    byte[] body = new UTF8Encoding(false).GetBytes(sb.ToString());
                    string tmp = p + ".tmp";
                    using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write))
                    {
                        if (bBom) fs.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                        fs.Write(body, 0, body.Length);
                    }
                    File.Replace(tmp, p, null);
                    return true;
                }
                catch (Exception ex) { if (L != null) L("★수직 눈 확인 기록을 쓰지 못함: " + SagRun_Msg(ex)); return false; }
            }
            // 묻는 글 (지금 3D 자세 — 펼치기가 끝난 자리). nMode: 1 = 실습 1단계 ([예] 뒤 바닥 찾기), 0 = 눈 확인만 (SagPrac_EyeJob — [예] 면 기록하고 원위치),
            //   −1 = 3D 만 미리 보기 (기록하지 않음)
            private string SagPrac_EyePrompt(CSagArmCfg c) { return SagPrac_EyePrompt(c, 1); }
            private string SagPrac_EyePrompt(CSagArmCfg c, int nMode)
            {
                var ci = CultureInfo.InvariantCulture;
                float x, y, z, p;
                SagArm_Tcp(c, out x, out y, out z, out p);
                double r = Math.Sqrt((double)x * x + (double)y * y);
                var sb = new StringBuilder();
                if (nMode < 0) sb.Append("수직 눈 확인 — 3D 만 미리 보기 (실물은 움직이지 않음, 기록하지 않음)\r\n\r\n");
                else if (nMode == 0) sb.Append("수직 눈 확인 (이 로봇·정의·모터 영점으로 한 번 — 바닥은 누르지 않음)\r\n\r\n");
                else sb.Append("수직 눈 확인 (이 로봇·정의로 처음 한 번)\r\n\r\n");
                sb.Append(string.Format(ci, "{6}팔이 가운데 점 위 (반경 {0:0}, 높이 {1:0}) 에서 '바닥으로 수직' 자세로 멈춰 있습니다 (관절 합 {2}+{3}+{4} = {5:0.#}°).\r\n",
                    r, z, c.PosIDs[1], c.PosIDs[2], c.WristID, c.DownSumDeg, nMode < 0 ? "3D " : ""));
                sb.Append("3D 에는 공구 끝(빨강)·공구축(초록 선)·꺾는 손목(주황)이 보입니다.\r\n\r\n");
                if (nMode < 0)
                {
                    sb.Append("실물로 하면 이 자세에서 실물 펜이 바닥에 수직인지 묻고, [예] 면 기록합니다.\r\n\r\n");
                    sb.Append("[예]·[아니오] 어느 쪽이든 기록하지 않고 3D 를 원위치합니다");
                    return sb.ToString();
                }
                sb.Append("실물 펜도 바닥에 수직입니까? 옆과 앞에서 보십시오.\r\n\r\n");
                if (nMode == 0)
                {
                    sb.Append("[예] 수직 — 기록하고 원위치합니다 (그 뒤 ③ 측정·⑤ 닿기 시험·실습 2·3단계를 할 수 있음)\r\n");
                    sb.Append("[아니오] 기울어짐 — 기록하지 않고 원위치합니다 (모터 세팅에서 영점을 맞춘 뒤 다시)");
                }
                else
                {
                    sb.Append("[예] 수직 — 바닥 찾기를 이어 갑니다\r\n");
                    sb.Append("[아니오] 기울어짐 — 누르지 않고 원위치합니다 (모터 세팅에서 영점을 맞춘 뒤 다시)");
                }
                return sb.ToString();
            }
            // 1단계 실물 — 펼친 자리에서 수직 눈 확인 (따로 움직이지 않는다). 예 → 기록, 아니오 → run.Why (누르지 않고 원위치), 중단·예외 → run.Stopped.
            //   묻는 곳이 없으면 (eyeAsk null — 옛 앱·스크립트) 예전처럼 진행 (★ 로그, EyeAns "생략")
            private void SagPrac_EyeCheck(CSagPracRun run, DSagEyeAsk eyeAsk, string strUrdfDir, string eyeDef, string eyeMap, CSagPracResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = run.Arm.Cfg;
                if (eyeAsk == null) { run.L("★수직 눈 확인 생략 (묻는 곳 없음)"); res.EyeAns = "생략"; return; }
                if (SagRun_Stopped(run.Stop)) { res.EyeAns = "중단"; SagPrac_StopLog(run, "수직 눈 확인 전"); return; }
                float x, y, z, p;
                SagArm_Tcp(c, out x, out y, out z, out p);
                run.L(string.Format(ci, "진행 수직 눈 확인 — 대답을 기다림 (r{0:0}, z{1:0})", Math.Sqrt((double)x * x + (double)y * y), z));
                ESagEyeAns ans;
                try { ans = eyeAsk(SagPrac_EyePrompt(c)); }
                catch (Exception ex) { run.L("★수직 눈 확인 오류: " + SagRun_Msg(ex)); ans = ESagEyeAns.Stop; }
                if (ans == ESagEyeAns.Yes)
                {
                    res.EyeAns = "예";
                    bool bSaved = SagPrac_SaveEye(strUrdfDir, eyeDef, eyeMap, run.L);
                    run.L("수직 눈 확인: 예 — " + (bSaved ? "기록함" : "★기록하지 못함 (다음에 다시 묻는다)") + ", 바닥 찾기를 이어 감");
                }
                else if (ans == ESagEyeAns.No)
                {
                    res.EyeAns = "아니오";
                    run.Why = string.Format(ci, "실물 펜이 수직이 아님 (확인: 아니오) — 모터 세팅에서 손목 {0}·관절 {1}·{2} 영점을 맞춘 뒤 다시", c.WristID, c.PosIDs[1], c.PosIDs[2]);
                    run.L("★" + run.Why + " — 누르지 않고 원위치");
                }
                else { res.EyeAns = "중단"; SagPrac_StopLog(run, "수직 눈 확인"); }
            }

            // ── 수직 눈 확인만 (10-05) — 이미 쓰던 설치가 실습 1단계를 다시 하지 않고 기록을 남기는 길 ──
            private const string SAGPRAC_EYE_OWNER = "수직 눈 확인";
            /// <summary>수직 눈 확인만 — 실물 (SagPrac_EyeJob(conn, armCfg, strUrdfDir, false, eyeAsk, log, stop) 와 같다)</summary>
            public CSagPracResult SagPrac_EyeJob(CArmConn_t conn, CSagArmCfg armCfg, string strUrdfDir, DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_EyeJob(conn, armCfg, strUrdfDir, false, eyeAsk, log, stop);
            }
            /// <summary>수직 눈 확인만 (블로킹 — 작업 스레드, 10-05). 이미 쓰던 설치가 실습 1단계를 다시 하지 않고 눈 확인 기록을 남기는 길
            /// (실물 ③ 측정·⑤ 닿기 시험·실습 2·3단계의 관문 SAGPRAC_EYE_FIRST 를 지나게 한다).
            /// 순서: 작업 입구 확인 (다른 작업 중·urdf 폴더·연결) → 로봇 정의 확인 (SagDef_Gate — 틀리면 그 까닭) → 실물인데 묻는 곳(eyeAsk)이 없으면 움직이지 않고 거절 →
            /// SagArm_Open (높이·정의·연결·잠금·휴지 자세·토크 시작 상태 — 다른 작업과 같은 관문. 옮기기 보정도 같다: 적용 중 모델의 관절 모델만) →
            /// 펼치기 (SagArm_Unfold — 실습 1단계와 같은 가운데 점 위, 옮기는 높이, 바닥으로 수직 자세) → 그 자리가 바닥 찾기 시작 높이 (min(FindStartZ, SafeZ − 10))
            /// 아래거나 관절 합이 정의와 다르면 묻지 않고 원위치 → 3D 표시를 이 정의로 켜고 (ToolMark_Hold — 끝나면 되돌림) eyeAsk 로 "실물 펜이 수직입니까?"
            /// (작업 스레드에서 부른다 — 앱은 UI 스레드로 Invoke 해 대화창) → 예: state.txt 의 eyedef·eyemap·eyetime 만 기록 (SagPrac_SaveEye — 다른 줄은 그대로,
            /// 실습 진행을 바꾸지 않는다), 아니오: 기록하지 않음 (res.EyeAns "아니오", Why — 모터 세팅에서 영점), 중단·예외: 중단 → SagArm_Close (원위치·토크 시작 상태·잠금 놓기).
            /// 바닥을 누르는 함수는 부르지 않는다. 이미 확인한 로봇도 다시 할 수 있다 (예면 시각이 새로). 3D 만 (bDry): 3D 팔만 같은 자리로 움직여 보이고
            /// (eyeAsk 가 있으면 미리 보기 글로 묻는다) 기록하지 않는다. 작업 기록은 <urdf>\sagcomp_runs\<시각>_eye[_3d]\ (run.log, rest_pose.txt).
            /// 결과: Step 0, Ok = 실물은 [예] 로 기록함 (3D 만은 미리 보기를 마침), EyeAns, Why, Stopped, Ran, RestOk, RunDir, Line. 예외 없음.</summary>
            public CSagPracResult SagPrac_EyeJob(CArmConn_t conn, CSagArmCfg armCfg, string strUrdfDir, bool bDry, DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_EyeJobBody(conn, armCfg, strUrdfDir, bDry, eyeAsk, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_EyeJobBody(CArmConn_t conn, CSagArmCfg armCfg, string strUrdfDir, bool bDry, DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 0, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 폴더를 만들거나 움직이기 전에 (작업 입구 SagArm_Open 도 본다)
                    if (!bDry && eyeAsk == null) return SagPrac_Refuse(res, "묻는 곳 없음 — 수직 눈 확인은 사람이 실물 펜을 보고 답해야 함 (움직이지 않음)", L);
                    string eyeDef, eyeMap, eyeText;
                    bool bNeed = SagPrac_EyeNeededCore(strUrdfDir, bDry ? null : conn, armCfg, out eyeDef, out eyeMap, out eyeText);
                    var cfg = new CSagPracCfg();                                 // 자리는 실습 1단계와 같다 (가운데 점, 바닥 찾기 시작 높이)
                    List<CSagPracPoint> pts = SagPrac_MakePoints(cfg, false);
                    CSagPracPoint mid = pts[SagPrac_MidIndex(pts)];
                    DateTime t0 = DateTime.Now;
                    res.RunDir = SagRun_NewRunDir(strUrdfDir, t0, bDry ? "eye_3d" : "eye");
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    L((bDry ? "3D 만 — " : "") + "수직 눈 확인 시작 — " + t0.ToString("yyyy-MM-dd HH:mm:ss", ci) + (bDry ? " (미리 보기 — 기록하지 않음)" : (bNeed ? " (확인 전)" : " (" + eyeText + " — 다시 확인)")));
                    L(string.Format(ci, "팔을 가운데 점 r={0:0.#} 위 옮기는 높이에서 '바닥으로 수직' 자세로 펼쳐 묻습니다 — 바닥은 누르지 않고 실습 진행은 그대로", mid.R));
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_EYE_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    if (!SagArm_Unfold(arm, mid.X, mid.Y, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        float x, y, z, p;
                        SagArm_Tcp(arm.Cfg, out x, out y, out z, out p);
                        float top = Math.Min(cfg.FindStartZ, arm.Cfg.SafeZ - 10);
                        float sum = SagArm_SumNow(arm.Cfg);
                        if (!(z >= top - 0.5f))
                            res.Why = string.Format(ci, "펼친 자리의 높이 z={0:0.#} 가 바닥 찾기 시작 높이 {1:0.#} 보다 낮음 — 묻지 않고 원위치", z, top);
                        else if (!(Math.Abs(sum - arm.Cfg.DownSumDeg) <= SAGDEF_TOL_DEG))
                            res.Why = string.Format(ci, "펼친 자세의 관절 합 {0:F1}° 가 정의 {1:0.###}° 와 다름 — 묻지 않고 원위치", sum, arm.Cfg.DownSumDeg);
                        else if (SagRun_Stopped(stop)) { res.Stopped = true; res.EyeAns = "중단"; L("중단 요청 — 수직 눈 확인 전에 멈춤, 원위치로 돌아가 토크를 시작 상태로"); }
                        else SagPrac_EyeAskOnly(arm, eyeAsk, strUrdfDir, eyeDef, eyeMap, bDry, res, L);
                        if (res.Why.Length > 0) L("★" + res.Why);
                    }
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                SagPrac_FinishEye(res, L);
                return res;
            }
            // 눈 확인만 — 펼친 자리에서 묻는다 (따로 움직이지 않는다). 묻는 동안 3D 표시를 이 작업의 정의로 켠다 (끝나면 되돌림 — 그사이 UI 가 바꿨으면 그대로).
            //   예 → 기록 (실물만), 아니오 → res.Why (기록 없음), 중단·예외 → res.Stopped. 3D 만은 기록하지 않는다 (묻는 곳이 없으면 묻지 않고 지나간다)
            private void SagPrac_EyeAskOnly(CSagArm arm, DSagEyeAsk eyeAsk, string strUrdfDir, string eyeDef, string eyeMap, bool bDry, CSagPracResult res, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                float x, y, z, p;
                SagArm_Tcp(c, out x, out y, out z, out p);
                double r = Math.Sqrt((double)x * x + (double)y * y);
                object mark = ToolMark_Hold(c);
                try
                {
                    if (eyeAsk == null)
                    {
                        res.EyeAns = "3D 만";
                        L(string.Format(ci, "3D 만 — 수직 자세 미리 보기 (r{0:0}, z{1:0}) — 묻는 곳 없음, 기록하지 않음", r, z));
                        return;
                    }
                    L(string.Format(ci, "진행 수직 눈 확인 — 대답을 기다림 (r{0:0}, z{1:0})", r, z));
                    ESagEyeAns ans;
                    try { ans = eyeAsk(SagPrac_EyePrompt(c, bDry ? -1 : 0)); }
                    catch (Exception ex) { L("★수직 눈 확인 오류: " + SagRun_Msg(ex)); ans = ESagEyeAns.Stop; }
                    if (bDry)
                    {
                        res.EyeAns = "3D 만";
                        if (ans == ESagEyeAns.Stop) { res.Stopped = true; L("중단 — 3D 미리 보기에서 멈춤, 원위치"); }
                        else L("3D 만 — 미리 보기 끝 (기록하지 않음), 원위치");
                        return;
                    }
                    if (ans == ESagEyeAns.Yes)
                    {
                        res.EyeAns = "예";
                        if (SagPrac_SaveEye(strUrdfDir, eyeDef, eyeMap, L)) L("수직 눈 확인: 예 — 기록함 (실습 진행은 그대로), 원위치");
                        else res.Why = "수직 눈 확인 기록을 쓰지 못함 — 다시 하십시오";
                    }
                    else if (ans == ESagEyeAns.No)
                    {
                        res.EyeAns = "아니오";
                        res.Why = string.Format(ci, "실물 펜이 수직이 아님 (확인: 아니오) — 모터 세팅에서 손목 {0}·관절 {1}·{2} 영점을 맞춘 뒤 다시", c.WristID, c.PosIDs[1], c.PosIDs[2]);
                    }
                    else { res.EyeAns = "중단"; res.Stopped = true; L("중단 요청 — 수직 눈 확인에서 멈춤, 원위치로 돌아가 토크를 시작 상태로"); }
                }
                finally { ToolMark_Release(mark); }
            }
            // 눈 확인만 — 끝 줄 (Ok = 실물은 [예] 로 기록함, 3D 만은 미리 보기를 마침). 시작하지 않았으면 (SagPrac_Refuse 가 줄을 씀 — 아니면 오류 까닭으로) 그대로
            private static void SagPrac_FinishEye(CSagPracResult res, CSim2Real.DLog L)
            {
                if (!res.Ran)
                {
                    if (res.Line.Length == 0 && res.Why.Length > 0) { res.Line = "시작하지 않음 — " + res.Why; res.Report = res.Line; }
                    return;
                }
                res.Ok = !res.Stopped && res.Why.Length == 0 && (res.Dry || res.EyeAns == "예");
                string strRest = res.Dry ? "" : (res.RestOk ? "" : " (★원위치 확인 못 함)");
                if (res.Dry) res.Line = res.Ok ? "3D 만 · 수직 자세 미리 보기 — 기록하지 않음" : (res.Stopped ? "수직 눈 확인 (3D 만) 중단" : "수직 눈 확인 (3D 만) 실패 — " + res.Why);
                else if (res.Ok) res.Line = "수직 눈 확인 끝 — 기록함 (실습 진행은 그대로)" + strRest;
                else if (res.Stopped) res.Line = "수직 눈 확인 중단 — 기록하지 않음" + strRest;
                else res.Line = "수직 눈 확인 — 기록하지 않음: " + res.Why + strRest;
                res.Report = res.Line;
                if (L != null) L(res.Line);
            }
            // urdf 폴더의 로봇 정의 — sagarm.txt 가 있으면 그 정의 (읽기만), 없으면 기본값 (파일을 만들지 않는다)
            private static CSagArmCfg SagPrac_DefOf(string strUrdfDir)
            {
                try
                {
                    if (!string.IsNullOrEmpty(strUrdfDir) && File.Exists(Path.Combine(strUrdfDir, CSagArmSetup.FILE))) return CSagArmSetup.Load(strUrdfDir).Arm;
                }
                catch { }
                return new CSagArmCfg();
            }
            // 2·3·4단계 — 실습 상태의 정의(1단계를 한 정의)가 지금 정의와 같은가. 빈 값(옛 상태)은 지금 정의가 내장 OMX 일 때만 통과
            private bool SagPrac_DefMatch(CSagPracState st, CSagArmCfg arm, out string why)
            {
                why = "";
                string now = SagDef_Sig(arm), was = st == null ? "" : (st.Def ?? "");
                if (was.Length == 0)
                {
                    if (string.Equals(now, SagDef_OmxSig(), StringComparison.Ordinal)) return true;
                    why = "1단계를 다른 로봇 정의로 했음 (그때 내장 OMX, 지금 " + now + ") — 1. 오차 재기부터";
                    return false;
                }
                if (string.Equals(was, now, StringComparison.Ordinal)) return true;
                why = "1단계를 다른 로봇 정의로 했음 (그때 " + was + ", 지금 " + now + ") — 1. 오차 재기부터";
                return false;
            }

            // ────────────────────────────────────────────────────────────
            // 2. 보정 데이터 만들기
            // ────────────────────────────────────────────────────────────

            /// <summary>2단계 보정 데이터 만들기 (블로킹 — 작업 스레드). 1단계를 마친 실습만 (같은 종류 — 실물/3D 만). 순서: 가운데 점 바닥 확인 (보정 끔) →
            /// (a) 관절 처짐 측정 (공중) → 관절 모델 (실습 폴더 joint_model.txt — 새 파일) → (b) 바닥 첫닿음 측정 (관절 모델 켬, 손목 유지 DataWristHold)
            /// → 데이터셋 덧붙임 (실물: <urdf>\floor_dataset_fc.csv — 없으면 머리줄과 함께 만듦, 3D 만: 실습 폴더 dataset_3d.csv) → 원위치 →
            /// (c) 학습 SagRun_Learn (바탕 = 새 관절 모델, 손목 유지 켬 행 포함) → 결과 파일 (실물: <urdf>\sagcomp_results, 3D 만: 실습 폴더 안 sagcomp_results).
            /// strMotorMapKey = 지금 모터 매핑 키 (관절 모델·결과 파일의 motormap). 작업 기록은 실습 폴더 2_build\.
            /// 실물은 이 로봇·정의·모터 영점으로 수직 눈 확인을 했어야 한다 (SagPrac_EyeNeeded — 1단계 뒤 영점을 고쳤으면 다시, 아니면 움직이기 전에 거절: SAGPRAC_EYE_FIRST). 예외 없음.</summary>
            public CSagPracResult SagPrac_BuildJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_BuildJobBody(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, strMotorMapKey, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_BuildJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                        string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 2, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                CSagComp joint = null;
                string dsPath = null;
                CSagPracState st = null;
                bool bDataOk = false;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
                    List<CSagPracPoint> before;
                    st = SagPrac_StateFor(strUrdfDir, 1, bDry, out before, out why);
                    if (st == null) return SagPrac_Refuse(res, why, L);
                    if (!SagPrac_DefMatch(st, armCfg, out why)) return SagPrac_Refuse(res, why, L);   // 1단계와 같은 로봇 정의인가 (state.txt def)
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, out why)) return SagPrac_Refuse(res, why, L);   // 실물 — 이 로봇·정의·모터 영점으로 수직 눈 확인을 했나 (움직이기 전) — 1단계 뒤 영점을 고쳤으면 다시
                    res.FolderDir = st.FolderDir;
                    res.RunDir = SagPrac_NewRunDir(st.FolderDir, "2_build");
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    var known = before.FindAll(p => !float.IsNaN(p.Touch));
                    CSagPracPoint mid = known[SagPrac_MidIndex(known)];
                    float hMax = float.MinValue; foreach (CSagPracPoint p in known) hMax = Math.Max(hMax, p.Touch);
                    L((bDry ? "3D 만 — " : "") + "2. 보정 데이터 만들기 시작 — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", ci) + " (" + (bPrecise ? "정밀" : "빠르게") + ")");
                    // 측정 높이 (공중) — 1단계 가장 높은 닿는 높이 + SurveyClearMm, 옮기는 높이를 넘지 않게
                    float zS = Math.Min(armCfg.SafeZ, hMax + cfg.SurveyClearMm);
                    if (zS < hMax + cfg.SurveyClearMinMm)
                        return SagPrac_Refuse(res, string.Format(ci, "관절 처짐 측정 높이 z={0:0.#} 가 바닥(1단계 닿는 높이 최고 {1:0.#}) 위 {2:0} 이 안 됨 — 옮기는 높이(safez) 확인", zS, hMax, cfg.SurveyClearMinMm), L);
                    List<float[]> svy = SagPrac_SurveyPoints(armCfg, cfg, bPrecise, zS, hMax, L);
                    if (svy.Count < 4) return SagPrac_Refuse(res, "관절 처짐 측정 점이 4개 미만 (IK·옮기는 높이)", L);
                    // (b) 의 점·데이터셋 경로
                    List<CSagPracPoint> data = SagPrac_DataPoints(cfg, bPrecise);
                    dsPath = bDry ? Path.Combine(res.RunDir, "dataset_3d.csv") : SagRun_FcDatasetPath(strUrdfDir);
                    res.DatasetPath = dsPath;
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    SagArm_SetMoveModel(arm, null, L);                       // 확인·관절 처짐 측정은 보정 없이 (원시 처짐을 잰다)
                    var run = SagPrac_NewRun(arm, cfg, res.RunDir, L, stop);
                    if (!bDry && !SagArm_Unfold(arm, mid.X, mid.Y, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        // ── 시작 확인 — 가운데 점 바닥 (보정 끔) ──
                        if (!bDry) SagPrac_CheckFloor(run, mid, res);
                        // ── (a) 관절 처짐 측정 ──
                        if (!run.Stopped && run.Why.Length == 0)
                        {
                            if (!bDry && !float.IsNaN(arm.CurZLow)) arm.CurZLow = Math.Min(arm.CurZLow, zS);   // 측정 높이는 바닥 위 SurveyClearMinMm 이상 (위에서 확인)
                            List<CSagSample> samples = bDry ? SagPrac_SimSurvey(kin, armCfg, cfg, svy) : SagPrac_Survey(run, svy);
                            run.LastKey = null;                                   // 다음 누름은 들어 옮긴다 (공중 측정 자리에서)
                            res.SurveySamples = samples == null ? 0 : samples.Count;
                            if (!run.Stopped && run.Why.Length == 0)
                            {
                                string strJ;
                                joint = SagPrac_FitJointModel(armCfg, samples, strMotorMapKey, zS, out strJ);
                                res.JointLine = strJ;
                                L(strJ.Replace("\r\n", " / "));
                                if (joint == null) run.Why = "관절 모델 학습 실패 — " + strJ;
                                else
                                {
                                    res.JointPath = SagPrac_FreeName(res.FolderDir, "joint_model", ".txt");
                                    joint.Save(res.JointPath);
                                    CSagComp chk = SagRun_LoadQuiet(res.JointPath);
                                    if (chk == null || !chk.HasJoint) { run.Why = "관절 모델 파일을 다시 읽지 못함: " + res.JointPath; joint = null; }
                                    else { joint = chk; joint.FloorMapEnabled = false; L("관절 모델: " + res.JointPath); }
                                }
                            }
                        }
                        // ── (b) 바닥 첫닿음 측정 (관절 모델 켬) ──
                        if (joint != null && !run.Stopped && run.Why.Length == 0)
                        {
                            SagArm_SetMoveModel(arm, joint, L);
                            bDataOk = bDry ? SagPrac_SimData(kin, armCfg, cfg, joint, data, known, dsPath, res, L)
                                           : SagPrac_FloorData(run, joint, data, known, dsPath, res);
                        }
                    }
                    res.Stopped = res.Stopped || run.Stopped;
                    if (run.Why.Length > 0) res.Why = SagRun_WhyJoin(res.Why, run.Why);
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                if (!res.Ran) return res;
                // ── (c) 학습 (원위치 뒤 — 로봇은 쉬고 있다) ──
                try
                {
                    if (bDataOk && joint != null && !res.Stopped && res.Why.Length == 0 && !SagRun_Stopped(stop))
                    {
                        L("진행 학습 — 바탕 관절 모델 " + Path.GetFileName(res.JointPath) + ", 데이터 " + Path.GetFileName(dsPath));
                        var lc = new CSagLearnCfg { DatasetPath = dsPath, BaseModelPath = res.JointPath, UseHold = true, RefMm = float.NaN };
                        string outDir = bDry ? res.FolderDir : strUrdfDir;           // 3D 만은 실습 폴더 안 sagcomp_results (결과 목록·적용에 섞지 않는다)
                        res.Learn = SagRun_Learn(lc, outDir, strMotorMapKey, L, stop);
                        if (res.Learn != null && res.Learn.Ok) res.ResultPath = res.Learn.ModelPath;
                        else if (res.Learn != null && res.Learn.Stopped) res.Stopped = true;
                        else res.Why = SagRun_WhyJoin(res.Why, "학습 실패 — " + (res.Learn != null ? res.Learn.Why : ""));
                    }
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★학습 오류: " + SagRun_Msg(ex)); L("★학습 오류: " + SagRun_Msg(ex)); }
                SagPrac_FinishBuild(res, strUrdfDir, st, bPrecise, L);
                return res;
            }
            // 2단계 끝 — 보고서 (실습 폴더 build_report.txt), 상태 (결과 파일이 생겼으면 2단계 마침, 3·4 단계는 지운다)
            private void SagPrac_FinishBuild(CSagPracResult res, string strUrdfDir, CSagPracState st, bool bPrecise, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    res.Ok = !res.Stopped && res.Why.Length == 0 && !string.IsNullOrEmpty(res.ResultPath) && File.Exists(res.ResultPath);
                    CSagLearnResult lr = res.Learn;
                    res.Line = res.Ok ? string.Format(ci, "{0}보정 데이터: 관절 처짐 {1}점 · 바닥 {2}점 (데이터셋 +{3}행) → {4}{5}", res.Dry ? "3D 만 · " : "", res.SurveySamples, res.DataRows,
                                                      res.DatasetRowsAdded, Path.GetFileNameWithoutExtension(res.ResultPath),
                                                      (lr != null && !double.IsNaN(lr.CvRmse)) ? string.Format(ci, " · CV {0:F2}mm", lr.CvRmse) : "")
                                     : (res.Stopped ? "2. 보정 데이터 만들기 중단" : "2. 보정 데이터 만들기 실패 — " + res.Why);
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 처짐 보정 실습 2. 보정 데이터 만들기 (3D 만 — 가상 로봇) ===" : "=== 처짐 보정 실습 2. 보정 데이터 만들기 ===");
                    sb.AppendLine("실습 폴더: " + (res.FolderDir ?? ""));
                    if (!float.IsNaN(res.CheckDiffMm)) sb.AppendLine(string.Format(ci, "시작 확인: 가운데 점 바닥이 1단계와 {0:+0.0;-0.0}mm", res.CheckDiffMm));
                    sb.AppendLine(string.Format(ci, "(a) 관절 처짐 측정: {0}점 → {1}", res.SurveySamples, res.JointPath ?? "(없음)"));
                    if (res.JointLine.Length > 0) sb.AppendLine("    " + res.JointLine.Replace("\r\n", "\r\n    "));
                    sb.AppendLine(string.Format(ci, "(b) 바닥 첫닿음 측정: {0}점 → 데이터셋 +{1}행 ({2})", res.DataRows, res.DatasetRowsAdded, res.DatasetPath ?? ""));
                    sb.AppendLine("(c) 학습: " + (lr == null ? "하지 않음" : (lr.Ok ? lr.Line : "실패 — " + lr.Why)));
                    if (lr != null && lr.Ok) sb.AppendLine("    결과 파일: " + lr.ModelPath + "\r\n    보고서: " + lr.ReportPath);
                    if (res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!res.Dry) sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    if (res.Why.Length > 0) sb.AppendLine("★" + res.Why);
                    sb.AppendLine(res.Line);
                    res.Report = sb.ToString();
                    if (!string.IsNullOrEmpty(res.RunDir)) { res.ReportPath = Path.Combine(res.RunDir, "build_report.txt"); SagRun_WriteText(res.ReportPath, res.Report); }
                    if (res.Ok && st != null)
                    {
                        st.Step = 2; st.Applied = false; st.JointPath = res.JointPath; st.ResultPath = res.ResultPath; st.Line2 = res.Line; st.Line3 = ""; st.Line4 = "";
                        SagPrac_SaveState(strUrdfDir, st, L);
                    }
                    L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★2단계 정리 오류: " + SagRun_Msg(ex)); }
            }

            // 관절 처짐 측정 점 (x, y, z, 접근 +1/−1) — 요 0, 반경 오름차순 → (왕복) 내림차순. 높이 zS (정밀이면 zS − 단계도, 바닥 위 SurveyClearMinMm 이상일 때만).
            //   점마다 그 높이 그대로에서 리치 여유를 두고 옮길 수 있어야 한다 (옮기는 높이 = z 로 본 SagRun_TransferHeight) — 아니면 뺀다
            private List<float[]> SagPrac_SurveyPoints(CSagArmCfg arm, CSagPracCfg cfg, bool bPrecise, float zS, float hMax, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                var l = new List<float[]>();
                var zs = new List<float> { zS };
                if (bPrecise && zS - cfg.PreciseSurveyStepMm >= hMax + cfg.SurveyClearMinMm) zs.Add(zS - cfg.PreciseSurveyStepMm);
                float[] rs = (bPrecise ? cfg.PreciseSurveyRadii : cfg.QuickSurveyRadii) ?? new float[0];
                var skip = new List<string>();
                foreach (float z in zs)
                {
                    var line = new List<float>();
                    CSagArmCfg az = arm.Clone(); az.SafeZ = z;           // 그 높이 그대로에서 리치 여유(팔꿈치 굽힘)를 두고 갈 수 있나 (옮기기는 그 높이로 수평)
                    foreach (float r in rs)
                    {
                        float tz = SagRun_TransferHeight(az, r, 0, 90, z);
                        if (float.IsNaN(tz) || Math.Abs(tz - z) > 0.01f) { skip.Add(string.Format(ci, "r{0:0} z{1:0}", r, z)); continue; }
                        line.Add(r);
                    }
                    foreach (float r in line) l.Add(new float[] { r, 0, z, +1 });
                    for (int i = line.Count - 2; i >= 0; i--) l.Add(new float[] { line[i], 0, z, -1 });
                }
                if (L != null)
                    L(string.Format(ci, "관절 처짐 측정: {0}점 (높이 {1}, 왕복){2}", l.Count, string.Join("/", zs.ConvertAll(v => v.ToString("0.#", ci)).ToArray()),
                        skip.Count > 0 ? " — 뺀 점 (리치 여유 없음): " + string.Join(", ", skip.ToArray()) : ""));
                return l;
            }
            // 관절 처짐 측정 (실물) — 점마다 안전 이동 (옮기는 높이 = 측정 높이 — 수평으로) → SurveySettleMs 기다림 → 엔코더 두 번 읽기 → 표본
            //   (⑨ 의 SagComp_MeasureOne 과 같은 표본: 명령·실측 TCP, 관절 명령·실측, 출력 단이 추적한 방향, 중력 지레). 실측 FK 는 관절값 사본으로.
            //   점 사이·기다리는 동안 중단을 본다. 읽기 실패한 점은 건너뛴다
            private List<CSagSample> SagPrac_Survey(CSagPracRun run, List<float[]> pts)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArm arm = run.Arm;
                var l = new List<CSagSample>();
                int[] ids = arm.Cfg.MoveIDs;
                for (int i = 0; i < pts.Count; i++)
                {
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "관절 처짐 점 사이"); break; }
                    float[] p = pts[i];
                    float x = (float)(p[0] * Math.Cos(p[1] * Math.PI / 180.0)), y = (float)(p[0] * Math.Sin(p[1] * Math.PI / 180.0));
                    if (!SagArm_MoveSafeCore(arm, x, y, p[2], 90, p[2], float.NaN, run.L, run.Stop))
                    {
                        if (arm.StopSeen) { SagPrac_StopLog(run, "관절 처짐 점으로 옮기던 다리를 마친 자리"); break; }
                        run.Why = SagRun_WhyJoin(string.Format(ci, "관절 처짐 측정 점으로 못 감 (r{0:0.#} z{1:0.#})", p[0], p[2]), arm.LastFail); break;
                    }
                    arm.CurTransferZ = p[2]; arm.CurZLow = float.NaN;
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < Math.Max(0, run.Cfg.SurveySettleMs)) { if (SagRun_Stopped(run.Stop)) break; Thread.Sleep(20); }
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "관절 처짐 점에서 기다리던 중"); break; }
                    string w;
                    Dictionary<int, float> act = arm.Scene.ReadDeg(out w);
                    if (act != null) act = arm.Scene.ReadDeg(out w);              // 두 번 (첫 응답이 낡을 수 있다)
                    float[] a = new float[ids.Length];
                    bool bOk = act != null;
                    for (int k = 0; k < ids.Length && bOk; k++) { float v; if (!act.TryGetValue(ids[k], out v)) bOk = false; else a[k] = v; }
                    if (!bOk) { run.L(string.Format(ci, "  ★r={0:0.#} z={1:0.#}: 읽기 실패 ({2}) — 건너뜀", p[0], p[2], w)); continue; }
                    float[] cmd = new float[ids.Length];
                    for (int k = 0; k < ids.Length; k++) cmd[k] = GetData(ids[k]);
                    CSagSample s = SagPrac_Sample(this, arm.Cfg, cmd, a, (int)p[3], SagComp_JointDirNow(ids));
                    l.Add(s);
                    run.L(string.Format(ci, "진행 관절 처짐 {0}/{1} — r={2:0.#} z={3:0.#}: 처짐 Δz {4:+0.00;-0.00}mm, Δr {5:+0.00;-0.00}mm", i + 1, pts.Count, p[0], p[2], s.DZ, s.DR));
                }
                return l;
            }
            // 표본 하나 — 명령·실측 TCP 는 관절값 사본 FK (kin 의 DH = 이 3D 와 같은 OMX), 지레 = SagFc_Levers (SagComp_JointLevers 와 같은 값)
            private static CSagSample SagPrac_Sample(C3d kin, CSagArmCfg c, float[] cmd, float[] act, int approach, float[] dir)
            {
                int[] ids = c.MoveIDs;
                double[] adC = kin.SagFc_MotCopy(), adA = (double[])adC.Clone();
                for (int k = 0; k < ids.Length; k++) { kin.SagFc_Set(adC, ids[k], cmd[k]); kin.SagFc_Set(adA, ids[k], act[k]); }
                float cx, cy, cz, ax, ay, az;
                kin.SagFc_Fk(c.FuncNumber, adC, -1, out cx, out cy, out cz);
                kin.SagFc_Fk(c.FuncNumber, adA, -1, out ax, out ay, out az);
                return new CSagSample
                {
                    CmdX = cx, CmdY = cy, CmdZ = cz, ActX = ax, ActY = ay, ActZ = az,
                    JointCmd = (float[])cmd.Clone(), JointAct = (float[])act.Clone(), Approach = approach, Dynamic = false,
                    JointDir = dir != null ? (float[])dir.Clone() : new float[ids.Length],
                    JointLever = kin.SagFc_Levers(c.FuncNumber, ids, cmd, adC)
                };
            }
            // 관절 모델 학습 — ⑨ 처짐보상과 같은 학습 (정지 계수 Fit + 관절 모델 SagComp_FitJoint, 접촉 의심을 뺐으면 정지 계수 다시). 관절 모델이 없으면 null
            private CSagComp SagPrac_FitJointModel(CSagArmCfg arm, List<CSagSample> samples, string strMapKey, float zS, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (samples == null || samples.Count < 4) { report = string.Format(ci, "표본 {0}개 — 4개 이상 필요", samples == null ? 0 : samples.Count); return null; }
                var m = new CSagComp { Robot = arm.DefName ?? "", FuncNumber = arm.FuncNumber, PosIDs = (int[])arm.PosIDs.Clone(), WristID = arm.WristID, PitchDeg = 90, MaxAbsDelta = 40,
                                       DownSumDeg = arm.DownSumDeg, KinSig = SagRun_KinSig(SagRun_KinState(), arm.FuncNumber) };   // 로봇 정의 (이름·수직 관절 합·기구학 표시)
                m.Samples.AddRange(samples);
                var sb = new StringBuilder();
                sb.AppendLine(m.Fit());
                sb.AppendLine(SagComp_FitJoint(m, null, false));
                if (m.Samples.Exists(q => q.Excluded)) sb.AppendLine("(접촉 의심 제외 후) " + m.Fit());
                report = sb.ToString().Trim();
                if (!m.HasJoint) return null;
                m.Enabled = true;
                m.MotorMap = SagRun_NormMapKey(strMapKey);
                m.Note = "처짐 보정 실습 관절 처짐 측정 — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", ci) + string.Format(ci, "\n표본 {0}, 높이 {1:0.#}mm (공중)", samples.Count, zS);
                return m;
            }

            // (b) 바닥 첫닿음 측정 (실물) — 점마다 창: 시작 = 1단계 높이(보정 끔) + 위 (관절 모델을 켜면 끝이 올라가 닿는 명령 높이는 그보다 낮다 — 시작은 더 위),
            //   한도 = (1단계 높이 − 관절 모델이 올리는 양) − 아래. 누름 모델 = 관절 모델, 손목 유지 DataWristHold, 첫닿음에서 멈춤. 첫닿음 행 → 데이터셋에 덧붙임.
            //   반환 = 덧붙인 행이 있다
            private bool SagPrac_FloorData(CSagPracRun run, CSagComp joint, List<CSagPracPoint> data, List<CSagPracPoint> known, string dsPath, CSagPracResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                C3d kin = SagRun_Kin();
                var rows = new List<CFloorPlanRow>();
                string trace = Path.Combine(run.RunDir, "data_trace.csv");
                run.TracePath = trace; run.CutPath = Path.Combine(run.RunDir, "data_trace_stopped.csv"); run.Hdr = false; run.HdrCut = false;
                if (File.Exists(trace)) File.Delete(trace);
                SagComp_ClearFloorTrace();
                DateTime t0 = DateTime.Now;
                int nDone = 0;
                foreach (CSagPracPoint p in data)
                {
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "점 사이"); break; }
                    float u; float h = SagPrac_Estimate(known, p.R, p.Yaw, run.Cfg.UnknownMm, out u);
                    if (float.IsNaN(h)) { p.Note = "예상 높이 없음 — 뺌"; continue; }
                    float lift = SagPrac_Lift(kin, run.Arm.Cfg, joint, p.X, p.Y, h, p.Pitch);
                    float exp = h - Math.Max(0f, lift);
                    p.Est = exp;
                    CFloorPlanRow row;
                    ESagPracPress o = SagPrac_PressAt(run, p, h, exp, u, joint, run.Cfg.DataWristHold, float.NaN, out row);
                    if (o == ESagPracPress.Stop || o == ESagPracPress.Fail) break;
                    if (o != ESagPracPress.Ok) continue;
                    nDone++;
                    if (row != null) { row.PredZ = exp; rows.Add(row); }
                    run.L(string.Format(ci, "진행 바닥 {0}/{1} — r={2:0.#} 요 {3:0.#}: {4}", nDone, data.Count, p.R, p.Yaw,
                        float.IsNaN(p.Touch) ? "닿지 않음 (" + p.Note + ")" : string.Format(ci, "첫닿음 {0} 명령 {1:F1} FK {2:F1}", p.Kind.Length > 0 ? p.Kind : "—", p.Touch, p.Fk)));
                }
                res.DataRows = nDone;
                // 첫닿음 → 데이터셋 (멈춘 회차도 — 점마다 독립 자료). 1·3단계와 같은 검출(SagPrac_TouchFromTrace)의 첫닿음 명령·FK·관절각 — 꺾임점은 쓰지 않는다
                var cs = new List<CFloorContact>();
                foreach (CSagPracPoint p in data)
                {
                    if (float.IsNaN(p.Touch) || p.TouchQ == null || p.TouchA == null) continue;
                    cs.Add(new CFloorContact { R = p.R, X = p.X, Y = p.Y, Pitch = p.Pitch, CmdZ = p.Touch, FkZ = p.Fk, Detected = true, FromPress = true,
                                               JointCmd = p.TouchQ, JointAct = p.TouchA, Kind = "F", FcStage = "L", FcZ = p.Fk, FcCmdZ = p.Touch,
                                               FcJointCmd = p.TouchQ, FcJointAct = p.TouchA, Hold = run.Cfg.DataWristHold });
                }
                string strRound = SagRun_NewSession(dsPath, t0);
                string rowsCsv = SagRun_DatasetRows(cs, strRound, true);
                SagRun_WriteText(Path.Combine(run.RunDir, "floor_dataset_run.csv"), rowsCsv);
                int nAdd = cs.Count > 0 ? SagRun_AppendDataset(dsPath, rowsCsv) : 0;
                if (nAdd < 0) { run.L("★데이터셋에 덧붙이지 못함: " + dsPath); run.Why = SagRun_WhyJoin(run.Why, "★데이터셋에 덧붙이지 못함"); nAdd = 0; }
                res.DatasetRowsAdded = nAdd;
                run.L(string.Format(ci, "바닥 첫닿음: 누른 점 {0} → 첫닿음 {1} (재지 못함 {2}) — 데이터셋 +{3}행 (회차 {4}, {5})", nDone, cs.Count, nDone - cs.Count,
                    nAdd, strRound, Path.GetFileName(dsPath)));
                return nAdd > 0;
            }

            // ────────────────────────────────────────────────────────────
            // 3. 효과 보기
            // ────────────────────────────────────────────────────────────

            /// <summary>3단계 효과 보기 (블로킹 — 작업 스레드). 2단계를 마친 실습만. 결과 파일(strResultPath — null 이면 진행 상태의 결과 파일)을 켠 채
            /// 1단계와 같은 점을 같은 순서로 눌러 첫닿음 명령 높이를 잰다 (손목 유지 끔 — 1단계와 같은 조건, 옮기기는 결과 파일의 관절 모델).
            /// 먼저 가운데 점 바닥이 1단계와 같은지 본다 (보정 끔). after.csv, 전·후 흩어짐·Ref 기준 RMS·점별 값 (실습 폴더 after_summary.txt),
            /// 결과 파일 옆 <결과>_test.txt 에 [실습] 한 줄. 시험 동안 쌍둥이 = 결과 모델. 실물은 이 로봇·정의·모터 영점으로 수직 눈 확인을 했어야 한다 (SagPrac_EyeNeeded — 아니면 움직이기 전에 거절: SAGPRAC_EYE_FIRST). 예외 없음.</summary>
            public CSagPracResult SagPrac_VerifyJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, string strResultPath, bool bDry,
                                                    CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_VerifyJobBody(conn, armCfg, cfg, strUrdfDir, strResultPath, bDry, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_VerifyJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, string strResultPath, bool bDry,
                                                         CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 3, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                CSagPracState st = null;
                List<CSagPracPoint> before = null;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
                    st = SagPrac_StateFor(strUrdfDir, 2, bDry, out before, out why);
                    if (st == null) return SagPrac_Refuse(res, why, L);
                    if (!SagPrac_DefMatch(st, armCfg, out why)) return SagPrac_Refuse(res, why, L);   // 1단계와 같은 로봇 정의인가 (state.txt def)
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, out why)) return SagPrac_Refuse(res, why, L);   // 실물 — 이 로봇·정의·모터 영점으로 수직 눈 확인을 했나 (움직이기 전) — 1단계 뒤 영점을 고쳤으면 다시
                    string strRes = string.IsNullOrEmpty(strResultPath) ? st.ResultPath : strResultPath;
                    res.ResultPath = strRes;
                    CSagComp mOn = SagRun_LoadQuiet(strRes);
                    if (mOn == null || !mOn.HasJoint || !mOn.HasFc) return SagPrac_Refuse(res, "결과 파일에 관절 모델·첫닿음 모델이 없음 — 2단계를 다시: " + strRes, L);
                    string wd;
                    if (!SagRun_SameDef(mOn, armCfg, out wd)) return SagPrac_Refuse(res, "결과 파일과 설정의 로봇 정의가 다름 — " + wd, L);
                    mOn.Enabled = true; mOn.FloorMapEnabled = true; mOn.Fc.Enabled = true;
                    res.Ref = mOn.Fc.Ref;
                    res.FolderDir = st.FolderDir;
                    res.RunDir = SagPrac_NewRunDir(st.FolderDir, "3_verify");
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    L((bDry ? "3D 만 — " : "") + "3. 효과 보기 시작 — " + Path.GetFileName(strRes) + string.Format(ci, " 켬 (Ref {0:F1}mm), 1단계와 같은 점 {1}개", mOn.Fc.Ref, before.Count));
                    var known = before.FindAll(p => !float.IsNaN(p.Touch));
                    var pts = new List<CSagPracPoint>();
                    foreach (CSagPracPoint b in before)
                        pts.Add(new CSagPracPoint { Idx = b.Idx, R = b.R, Yaw = b.Yaw, X = b.X, Y = b.Y, Pitch = b.Pitch, Note = float.IsNaN(b.Touch) ? "1단계에서 닿지 않음 — 뺌" : "" });
                    var sim = new Dictionary<int, float>();
                    if (bDry)
                        foreach (CSagPracPoint p in pts)
                        {
                            float z, fk; float[] qi, qa;
                            if (SagPrac_SimTouch(kin, armCfg, cfg, p.X, p.Y, p.Pitch, mOn, out z, out fk, out qi, out qa)) sim[p.Idx] = z;
                        }
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    res.Learn = null;
                    string wm = SagArm_MotorMapWarn(bDry ? null : conn, mOn);
                    if (wm.Length > 0) L(wm);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    SagArm_SetMoveModel(arm, mOn, L);                        // 옮기기 = 결과 파일의 관절 모델 (바닥 부분 끔)
                    SagTwin_SetModel(mOn);                                   // 쌍둥이 = 결과 모델 (끝에 SagArm_Close 가 되돌린다)
                    var run = SagPrac_NewRun(arm, cfg, res.RunDir, L, stop);
                    int iMidB = SagPrac_MidIndex(known);
                    CSagPracPoint midB = known[iMidB];
                    if (!SagArm_Unfold(arm, midB.X, midB.Y, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        if (!bDry) SagPrac_CheckFloor(run, midB, res);
                        if (!run.Stopped && run.Why.Length == 0)
                        {
                            // 1단계와 같은 순서 (가운데부터)
                            List<CSagPracPoint> order = SagPrac_Order(pts, pts.FindIndex(p => p.Idx == midB.Idx));
                            int nDone = 0, nUse = 0;
                            foreach (CSagPracPoint p in order) if (p.Note.Length == 0) nUse++;
                            foreach (CSagPracPoint p in order)
                            {
                                if (p.Note.Length > 0) continue;
                                if (SagRun_Stopped(stop)) { SagPrac_StopLog(run, "점 사이"); break; }
                                CSagPracPoint b = before.Find(q => q.Idx == p.Idx);
                                float h = b.Touch;                                      // 1단계 닿는 높이 (보정 끔)
                                float lift = SagPrac_Lift(kin, armCfg, mOn, p.X, p.Y, h, p.Pitch);   // 결과 모델(관절 + 바닥)이 끝을 올리는 양
                                float lo = Math.Min((float)mOn.Fc.Ref, h - lift), hi = Math.Max(h, Math.Max((float)mOn.Fc.Ref, h - lift));
                                p.Est = (float)mOn.Fc.Ref;
                                float dz = 0; bool bSim = bDry && sim.TryGetValue(p.Idx, out dz);
                                if (bDry && !bSim) { p.Note = "가상 로봇 계산 실패 — 뺌"; continue; }
                                CFloorPlanRow row;
                                ESagPracPress o = SagPrac_PressAt(run, p, hi, lo, 0, mOn, false, bSim ? dz : float.NaN, out row);
                                if (o == ESagPracPress.Stop || o == ESagPracPress.Fail) break;
                                nDone++;
                                L(string.Format(ci, "진행 {0}/{1} — r={2:0.#} 요 {3:0.#}: 보정 켬 {4} (끔 {5:F1})", nDone, nUse, p.R, p.Yaw,
                                    float.IsNaN(p.Touch) ? "닿지 않음 (" + p.Note + ")" : string.Format(ci, "닿은 높이 {0:F1}", p.Touch), h));
                            }
                        }
                    }
                    res.Stopped = res.Stopped || run.Stopped;
                    if (run.Why.Length > 0) res.Why = SagRun_WhyJoin(res.Why, run.Why);
                    res.Points = pts;
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                if (res.Ran) SagPrac_FinishVerify(res, strUrdfDir, st, before, L);
                return res;
            }
            // 3단계 끝 — after.csv·요약, 전·후 지표, 결과 파일 시험 기록 [실습], 상태 (멈추지 않고 3점 이상 닿았으면 3단계 마침)
            private void SagPrac_FinishVerify(CSagPracResult res, string strUrdfDir, CSagPracState st, List<CSagPracPoint> before, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    var vb = new List<double>(); var va = new List<double>();
                    foreach (CSagPracPoint p in res.Points)
                    {
                        CSagPracPoint b = before.Find(q => q.Idx == p.Idx);
                        if (b == null || float.IsNaN(b.Touch) || float.IsNaN(p.Touch)) continue;
                        vb.Add(b.Touch); va.Add(p.Touch);
                    }
                    res.NTouched = va.Count;
                    double rf = double.IsNaN(res.Ref) ? SagPrac_Mean(va) : res.Ref;
                    if (va.Count >= 2)
                    {
                        res.SpreadBefore = SagPrac_Spread(vb); res.SpreadAfter = SagPrac_Spread(va);
                        res.RmsBefore = SagPrac_Rms(vb, rf); res.RmsAfter = SagPrac_Rms(va, rf);
                        res.Reduction = res.SpreadBefore > 1e-6 ? (1.0 - res.SpreadAfter / res.SpreadBefore) * 100.0 : double.NaN;
                    }
                    res.CsvPath = Path.Combine(res.FolderDir, "after.csv");
                    SagRun_WriteText(res.CsvPath, SagPrac_PointsCsv(res.Points));
                    res.Ok = !res.Stopped && res.Why.Length == 0 && res.NTouched >= 3;
                    if (!res.Stopped && res.Why.Length == 0 && res.NTouched < 3) res.Why = string.Format(ci, "전·후 둘 다 닿은 점이 {0}개 — 3개 이상 필요", res.NTouched);
                    string strSpread = SagPrac_SpreadText(res.SpreadBefore, res.SpreadAfter, res.Reduction);
                    res.Line = res.Ok ? (res.Dry ? "3D 만 · " : "") + strSpread + string.Format(ci, " · 기준선 RMS {0:F1} → {1:F1}mm", res.RmsBefore, res.RmsAfter)
                                     : (res.Stopped ? "3. 효과 보기 중단" : "3. 효과 보기 실패 — " + res.Why);
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 처짐 보정 실습 3. 효과 보기 (3D 만 — 가상 로봇) ===" : "=== 처짐 보정 실습 3. 효과 보기 ===");
                    sb.AppendLine("결과 파일: " + (res.ResultPath ?? ""));
                    if (!float.IsNaN(res.CheckDiffMm)) sb.AppendLine(string.Format(ci, "시작 확인: 가운데 점 바닥이 1단계와 {0:+0.0;-0.0}mm", res.CheckDiffMm));
                    sb.AppendLine(string.Format(ci, "기준선 Ref {0:F2}mm — 바닥이 평평하고 보정이 맞으면 모든 점이 이 높이에서 닿는다", rf));
                    sb.AppendLine("     r     요   보정 끔(1단계)   보정 켬   차(켬 − Ref)  판정  비고");
                    foreach (CSagPracPoint p in res.Points)
                    {
                        CSagPracPoint b = before.Find(q => q.Idx == p.Idx);
                        float bt = b == null ? float.NaN : b.Touch;
                        sb.AppendLine(string.Format(ci, "  {0,5:0.#} {1,5:0.#}  {2,14}  {3,8}  {4,12}  {5,-4}  {6}", p.R, p.Yaw, SagPrac_V(bt), SagPrac_V(p.Touch),
                            float.IsNaN(p.Touch) ? "—" : (p.Touch - rf).ToString("+0.00;-0.00", ci), p.Kind.Length > 0 ? p.Kind : "—", p.Note));
                    }
                    sb.AppendLine(strSpread);
                    sb.AppendLine(string.Format(ci, "기준선(Ref) RMS: {0:F2} → {1:F2}mm ({2}점)", res.RmsBefore, res.RmsAfter, res.NTouched));
                    if (res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!res.Dry) sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    if (res.Why.Length > 0) sb.AppendLine("★" + res.Why);
                    res.Report = sb.ToString();
                    res.ReportPath = Path.Combine(res.FolderDir, "after_summary.txt");
                    SagRun_WriteText(res.ReportPath, res.Report);
                    if (!string.IsNullOrEmpty(res.RunDir)) SagRun_WriteText(Path.Combine(res.RunDir, "verify_report.txt"), res.Report);
                    if (!string.IsNullOrEmpty(res.ResultPath) && res.NTouched > 0)
                    {
                        string rec = string.Format(ci, "[실습] {0} {1} 점 {2} | 흩어짐 끔 {3:F2} → 켬 {4:F2}mm ({5}) | Ref RMS {6:F2} → {7:F2}mm{8} | {9}",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm", ci), res.Dry ? "3D" : "실물", res.NTouched, res.SpreadBefore, res.SpreadAfter,
                            double.IsNaN(res.Reduction) ? "—" : res.Reduction.ToString("0", ci) + "% 줄어듦", res.RmsBefore, res.RmsAfter, res.Stopped ? " · 중단" : "", res.ReportPath);
                        SagRun_AppendTestLine(res.ResultPath, rec);
                    }
                    if (res.Ok && st != null)
                    {
                        st.Step = 3; st.Applied = false; st.ResultPath = res.ResultPath; st.Line3 = res.Line; st.Line4 = "";
                        SagPrac_SaveState(strUrdfDir, st, L);
                    }
                    L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★3단계 정리 오류: " + SagRun_Msg(ex)); }
            }

            // ────────────────────────────────────────────────────────────
            // 4. 적용
            // ────────────────────────────────────────────────────────────

            /// <summary>4단계 적용 (UI 스레드 — SagRun_Activate 규칙). 3단계를 마친 실물 실습의 결과 파일을 적용한다 (적용 중 파일 백업 + 바꿔 끼움 + 1초 전환).
            /// 3D 만 실습은 적용하지 않는다. 앱은 시작할 때 적용 중 파일을 읽으므로 그 뒤 모든 동작이 보정된다. 반환 = 적용됨. 예외 없음.</summary>
            public bool SagPrac_Apply(string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                strBackupPath = null; report = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    if (st.Step < 3) { report = "3. 효과 보기를 먼저 하십시오"; return false; }
                    if (st.Dry) { report = "3D 만 실습의 결과(가상 로봇)는 실물에 적용하지 않습니다 — 실물로 1단계부터"; return false; }
                    string whyDef;                                                       // 1단계와 같은 로봇 정의인가 — 정의 = <urdf>\sagarm.txt (없으면 기본값, 파일은 만들지 않는다)
                    if (!SagPrac_DefMatch(st, SagPrac_DefOf(strUrdfDir), out whyDef)) { report = whyDef; return false; }
                    if (string.IsNullOrEmpty(st.ResultPath) || !File.Exists(st.ResultPath)) { report = "결과 파일이 없음: " + st.ResultPath; return false; }
                    if (!SagRun_Activate(st.ResultPath, strUrdfDir, strMotorMapKey, L, out strBackupPath, out report)) return false;
                    st.Step = 4; st.Applied = true;
                    st.Line4 = "적용됨 — " + Path.GetFileName(st.ResultPath) + " (이제 모든 동작이 자동 보정)";
                    SagPrac_SaveState(strUrdfDir, st, L);
                    return true;
                }
                catch (Exception ex) { report = "★오류: " + SagRun_Msg(ex); return false; }
            }

            // ────────────────────────────────────────────────────────────
            // 그래프 자료
            // ────────────────────────────────────────────────────────────

            /// <summary>그래프 자료 — 진행 상태의 실습 폴더 before.csv / after.csv (3단계를 마쳤을 때만 '후'). 점 순서는 before.csv 그대로.
            /// 기준선 = 결과 파일의 Ref (3단계 후) — 없으면 '전' 평균. 없으면 빈 자료. 예외 없음.</summary>
            public static CSagPracChart SagPrac_ChartData(string strUrdfDir)
            {
                var ch = new CSagPracChart();
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    ch.Step = st.Step; ch.Dry = st.Dry; ch.Folder = st.Folder;
                    if (st.Step < 1) return ch;
                    List<CSagPracPoint> b = SagPrac_ReadPoints(Path.Combine(st.FolderDir, "before.csv"));
                    if (b == null || b.Count == 0) return ch;
                    List<CSagPracPoint> a = st.Step >= 3 ? SagPrac_ReadPoints(Path.Combine(st.FolderDir, "after.csv")) : null;
                    int n = b.Count;
                    ch.R = new float[n]; ch.Yaw = new float[n]; ch.Before = new float[n]; ch.After = new float[n];
                    var vb = new List<double>(); var va = new List<double>(); var vb2 = new List<double>();
                    for (int i = 0; i < n; i++)
                    {
                        ch.R[i] = b[i].R; ch.Yaw[i] = b[i].Yaw; ch.Before[i] = b[i].Touch; ch.After[i] = float.NaN;
                        if (!float.IsNaN(b[i].Touch)) vb.Add(b[i].Touch);
                        if (a != null)
                        {
                            CSagPracPoint q = a.Find(x => x.Idx == b[i].Idx);
                            if (q != null) ch.After[i] = q.Touch;
                            if (q != null && !float.IsNaN(q.Touch) && !float.IsNaN(b[i].Touch)) { va.Add(q.Touch); vb2.Add(b[i].Touch); }
                        }
                    }
                    ch.HasBefore = vb.Count > 0; ch.HasAfter = va.Count > 0;
                    if (ch.HasAfter)
                    {
                        CSagComp m = SagRun_LoadQuiet(st.ResultPath);
                        ch.RefZ = (m != null && m.HasFc) ? m.Fc.Ref : SagPrac_Mean(va);
                        ch.RefLabel = (m != null && m.HasFc) ? string.Format(ci, "기준선 (보정 후 모두 이 높이) {0:F1}mm", ch.RefZ) : "보정 후 평균";
                        ch.SpreadBefore = vb2.Count >= 2 ? SagPrac_Spread(vb2) : double.NaN;
                        ch.SpreadAfter = va.Count >= 2 ? SagPrac_Spread(va) : double.NaN;
                        ch.Reduction = (ch.SpreadBefore > 1e-6 && !double.IsNaN(ch.SpreadAfter)) ? (1.0 - ch.SpreadAfter / ch.SpreadBefore) * 100.0 : double.NaN;
                        ch.Line = SagPrac_SpreadText(ch.SpreadBefore, ch.SpreadAfter, ch.Reduction);
                    }
                    else if (ch.HasBefore)
                    {
                        ch.RefZ = SagPrac_Mean(vb);
                        ch.RefLabel = string.Format(ci, "평평한 바닥이면 모두 같은 높이 (평균 {0:F1}mm)", ch.RefZ);
                        ch.SpreadBefore = vb.Count >= 2 ? SagPrac_Spread(vb) : double.NaN;
                        ch.Line = double.IsNaN(ch.SpreadBefore) ? "" : string.Format(ci, "흩어짐 {0:F1}mm (보정 끔)", ch.SpreadBefore);
                    }
                }
                catch { }
                return ch;
            }
            private static string SagPrac_SpreadText(double b, double a, double red)
            {
                var ci = CultureInfo.InvariantCulture;
                if (double.IsNaN(b) || double.IsNaN(a)) return "흩어짐 —";
                return string.Format(ci, "흩어짐 {0:F1}mm → {1:F1}mm ({2})", b, a,
                    double.IsNaN(red) ? "—" : (red >= 0 ? red.ToString("0", ci) + "% 줄어듦" : (-red).ToString("0", ci) + "% 늘어남"));
            }

            /// <summary>단계 안내 한 줄 (확인창용) — 점 수와 대략 걸리는 시간 (분, 실물 기준). step = 1~3</summary>
            public static string SagPrac_PlanText(CSagPracCfg cfg, int step, bool bPrecise)
            {
                if (cfg == null) cfg = new CSagPracCfg();
                var ci = CultureInfo.InvariantCulture;
                int n1 = SagPrac_MakePoints(cfg, bPrecise).Count;
                if (step == 1) return string.Format(ci, "점 {0}개 (+ 처음이면 바닥 찾기) — 약 {1:0}분", n1, Math.Ceiling(0.6 + n1 * 0.4 + 0.7));
                if (step == 2)
                {
                    int ns = ((bPrecise ? cfg.PreciseSurveyRadii : cfg.QuickSurveyRadii) ?? new float[0]).Length * 2 - 1;
                    if (bPrecise) ns *= 2;
                    int nd = SagPrac_DataPoints(cfg, bPrecise).Count;
                    return string.Format(ci, "바닥 확인 1점 + 공중 관절 처짐 약 {0}점 + 바닥 첫닿음 {1}점 + 학습 — 약 {2:0}분", ns, nd, Math.Ceiling(0.6 + 0.4 + ns * 0.06 + nd * 0.4 + 0.5));
                }
                return string.Format(ci, "바닥 확인 1점 + 1단계와 같은 점 — 약 {0:0}분", Math.Ceiling(0.6 + 0.4 + n1 * 0.4));
            }

            // ────────────────────────────────────────────────────────────
            // 공용 — 누름·창·넘겨짚기
            // ────────────────────────────────────────────────────────────

            private enum ESagPracPress { Ok, Skip, Stop, Fail }
            // 한 단계 작업의 누름 상태 (작업 스레드 하나)
            private class CSagPracRun
            {
                public CSagArm Arm; public CSagPracCfg Cfg; public CSim2Real.DLog L; public CSim2Real.DIsStopped Stop;
                public string RunDir, TracePath, CutPath; public bool Hdr, HdrCut;
                public string LastKey; public float LastR = float.NaN;
                public bool Stopped; public string Why = "";
            }
            private static CSagPracRun SagPrac_NewRun(CSagArm arm, CSagPracCfg cfg, string runDir, CSim2Real.DLog L, CSim2Real.DIsStopped stop)
            {
                var r = new CSagPracRun { Arm = arm, Cfg = cfg, L = L, Stop = stop, RunDir = runDir };
                if (!arm.IsDry && !string.IsNullOrEmpty(runDir)) { r.TracePath = Path.Combine(runDir, "press_trace.csv"); r.CutPath = Path.Combine(runDir, "press_trace_stopped.csv"); }
                return r;
            }
            private static float SagPrac_Retract(CSagPracCfg cfg) { return (cfg.Probe != null ? cfg.Probe : new CFloorProbeCfg { RetractAboveKneeMm = 25 }).RetractAboveKneeMm; }
            private static CFloorPlanRow SagPrac_Row(CSagPracPoint p, float zStart, float zMin, float predZ)
            {
                return new CFloorPlanRow { Set = "P", Pitch = p.Pitch, Yaw = p.Yaw, R = p.R, X = p.X, Y = p.Y, ZStart = zStart, ZMin = zMin, PredZ = predZ, Src = "prac", LimitByCaller = true };
            }

            // 점 하나를 예상 높이로 누른다 (첫닿음에서 멈춤) — 창: 시작 = expHi + 위 (+ 여유 u) 를 높은 쪽부터 줄여 가며 (최소 expHi + MinAboveMm)
            //   계획 확인(IK·옮기는 높이)을 지나는 첫 값, 한도 = expLo − 아래 − u. fDry = 3D 만 누름 높이 (가상 로봇). 결과는 p (Touch·Fk·Kind·Note).
            //   실물: 첫닿음이 없으면 꺾임점으로 바꾸지 않는다 — 시작부터 닿아 있었으면(창이 낮음) 창을 RetryMm 위로 한 번만 다시, 한도까지 첫닿음이
            //   없었으면 더 아래로 가지 않는다 (검출을 놓쳤을 수 있다). 그래도 없으면 '재지 못함' (Touch NaN — 흩어짐에서 빼고 수를 알린다)
            private ESagPracPress SagPrac_PressAt(CSagPracRun run, CSagPracPoint p, float expHi, float expLo, float u, CSagComp model, bool bHold, float fDry)
            {
                CFloorPlanRow row; return SagPrac_PressAt(run, p, expHi, expLo, u, model, bHold, fDry, out row);
            }
            private ESagPracPress SagPrac_PressAt(CSagPracRun run, CSagPracPoint p, float expHi, float expLo, float u, CSagComp model, bool bHold, float fDry, out CFloorPlanRow rowOut)
            {
                var ci = CultureInfo.InvariantCulture;
                ESagPracPress o = SagPrac_PressOnce(run, p, expHi, expLo, u, model, bHold, fDry, out rowOut);
                if (o != ESagPracPress.Ok || run.Arm.IsDry || !float.IsNaN(p.Touch) || rowOut == null) return o;
                // 다시 한 번 — 창을 옮겨서
                float hi2, lo2;
                if (p.StartContact) { hi2 = expHi + run.Cfg.RetryMm; lo2 = expLo; }
                else return o;                                            // 한도까지 첫닿음이 없으면 더 아래로 가지 않는다 (검출을 놓쳤을 수 있다 — 더 누르지 않는다)
                string why0 = "시작부터 닿아 있었음 — 창을 위로";
                run.L(string.Format(ci, "  r={0:0.#} 요 {1:0.#}: {2} 한 번 더", p.R, p.Yaw, why0));
                p.Tries++; p.Note = ""; p.StartContact = p.NoTouch = false;
                CFloorPlanRow row2;
                o = SagPrac_PressOnce(run, p, hi2, lo2, 0, model, bHold, fDry, out row2);
                if (row2 != null) rowOut = row2;
                if (o == ESagPracPress.Ok && float.IsNaN(p.Touch)) p.Note = SagRun_WhyJoin("재지 못함 (두 번 모두 첫닿음 없음)", p.Note);
                return o;
            }
            private ESagPracPress SagPrac_PressOnce(CSagPracRun run, CSagPracPoint p, float expHi, float expLo, float u, CSagComp model, bool bHold, float fDry, out CFloorPlanRow rowOut)
            {
                rowOut = null;
                var ci = CultureInfo.InvariantCulture;
                CSagPracCfg cfg = run.Cfg;
                if (float.IsNaN(u) || u < 0) u = 0;
                float zMin = expLo - cfg.BelowMm - u;
                var cand = new List<float>();
                foreach (float a in new float[] { cfg.AboveMm + u, cfg.AboveMm, cfg.MinAboveMm })
                {
                    float z = Math.Min(expHi + a, run.Arm.Cfg.SafeZ - 10);
                    if (z < expHi + cfg.MinAboveMm - 1e-3f) continue;
                    bool bDup = false; foreach (float c in cand) if (Math.Abs(c - z) < 0.5f) bDup = true;
                    if (!bDup) cand.Add(z);
                }
                string last = "";
                foreach (float zs in cand)
                {
                    CFloorPlanRow r = SagPrac_Row(p, zs, zMin, expHi);
                    string rep;
                    SagRun_CheckPlanCore(new List<CFloorPlanRow> { r }, run.Arm.Cfg, SagPrac_Retract(cfg), true, null, false, false, out rep);
                    if (!r.IkOk) { last = r.Why; continue; }
                    rowOut = r;
                    CSagPracTouch t;
                    ESagPracPress o = SagPrac_Press(run, p, r, model, bHold, fDry, out t);
                    if (o == ESagPracPress.Ok)
                    {
                        if (run.Arm.IsDry) { p.Touch = fDry; p.Fk = float.NaN; p.Kind = "F"; p.Knee = float.NaN; }
                        else SagPrac_TakeTouch(t, p, run.L);
                    }
                    return o;
                }
                p.Note = SagRun_WhyJoin("창을 잡지 못함 — " + (last.Length > 0 ? last : string.Format(ci, "옮기는 높이(safez {0:0.#})가 예상 닿는 높이 {1:F1} + {2:0} 보다 낮음", run.Arm.Cfg.SafeZ, expHi, cfg.MinAboveMm + 10)), p.Note);
                run.L("★r=" + p.R.ToString("0.#", ci) + " 요 " + p.Yaw.ToString("0.#", ci) + ": " + p.Note);
                return ESagPracPress.Skip;
            }

            // 누름 하나 (확인된 창 row) — 줄이 바뀌었거나 멀면 안전 이동 → SagComp_FloorPress (실물 — 첫닿음에서 멈춤 StopAtFirstTouch) / SagRun_DryPress (3D 만).
            //   하강 중 중단·한 틱 관절 변화 막이로 끊긴 누름은 쓰지 않는다 (궤적은 따로). 실물은 그 누름의 궤적 조각을 같은 검출기로 다시 읽어
            //   첫닿음(경첩 꺾임)을 낸다 (SagPrac_TouchFromTrace — 실시간으로 멈춘 그 판정과 같은 계산). 꺾임점(단단한 정지)은 쓰지 않는다
            private ESagPracPress SagPrac_Press(CSagPracRun run, CSagPracPoint p, CFloorPlanRow row, CSagComp model, bool bHold, float fDry, out CSagPracTouch tOut)
            {
                tOut = null;
                var ci = CultureInfo.InvariantCulture;
                CSagArm arm = run.Arm;
                if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 누르기 전", p.R)); return ESagPracPress.Stop; }
                bool bMove = run.LastKey != row.LineKey || float.IsNaN(run.LastR) || Math.Abs(run.LastR - p.R) > run.Cfg.SameLineMm;
                if (bMove)
                {
                    float tz = float.IsNaN(row.TransferZ) ? arm.Cfg.SafeZ : row.TransferZ;
                    if (!SagArm_MoveSafeCore(arm, row.X, row.Y, tz, row.Pitch, tz, row.ZStart + 10, run.L, run.Stop))
                    {
                        if (arm.StopSeen) { SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 로 옮기던 다리를 마친 자리", p.R)); return ESagPracPress.Stop; }
                        run.Why = SagRun_WhyJoin(string.Format(ci, "옮기기 실패 (r{0:0.#} 요 {1:0.#})", p.R, p.Yaw), arm.LastFail);
                        return ESagPracPress.Fail;
                    }
                }
                CFloorProbeCfg pc = (run.Cfg.Probe != null ? run.Cfg.Probe : new CFloorProbeCfg { Repeats = 1, Margin = 20, RetractAboveKneeMm = 25 }).Clone();
                pc.FuncNumber = arm.Cfg.FuncNumber; pc.PosIDs = (int[])arm.Cfg.PosIDs.Clone(); pc.WristID = arm.Cfg.WristID; pc.DownSumDeg = arm.Cfg.DownSumDeg;
                pc.Radii = new float[] { p.R }; pc.YawDeg = p.Yaw; pc.RyDeg = p.Pitch; pc.ZStart = row.ZStart; pc.ZMin = row.ZMin;
                pc.WristHold = bHold; pc.Repeats = 1; if (pc.Margin < 20) pc.Margin = 20;
                pc.StopAtFirstTouch = true; pc.PressEndLagMm = 0;
                if (run.Cfg.DescentMmPerSec > 0) pc.PressDescentMmPerSec = run.Cfg.DescentMmPerSec;
                if (!arm.IsDry)
                {
                    string rep;
                    List<CFloorContact> cs = SagComp_FloorPress(arm.Scene, pc, model, run.L, run.Stop, out rep);
                    string raw = SagRun_TraceTakeRaw();
                    string g = SagArm_GuardWhy(arm);
                    bool bCut = g.Length > 0;
                    foreach (CFloorContact x in cs) if (x != null && x.UserStop) bCut = true;
                    if (bCut)
                    {
                        if (run.CutPath != null && raw.Length > 0) SagRun_TraceAppend(run.CutPath, raw, ref run.HdrCut);
                        if (g.Length > 0) { run.Why = SagRun_WhyJoin(run.Why, g + " — 멈춤"); run.L("★" + g + " — 멈춤 (이 누름은 쓰지 않음)"); return ESagPracPress.Fail; }
                        SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 하강 중 (이 누름은 쓰지 않음)", p.R));
                        return ESagPracPress.Stop;
                    }
                    if (cs.Count == 0)
                    {
                        if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 시작점으로 가던 중 (누르지 않음)", p.R)); return ESagPracPress.Stop; }
                        p.Note = SagRun_WhyJoin("시작점으로 못 감 — 뺌", p.Note);
                        run.L(string.Format(ci, "★r={0:0.#} 요 {1:0.#}: 시작점으로 못 감 — 이 점은 빼고 다음으로", p.R, p.Yaw));
                        run.LastKey = null;                                       // 다음 점은 들어 옮긴다
                        return ESagPracPress.Skip;
                    }
                    if (run.TracePath != null && raw.Length > 0) SagRun_TraceAppend(run.TracePath, raw, ref run.Hdr);
                    List<CSagPracTouch> ts = SagPrac_TouchFromTrace(raw);
                    tOut = ts.Count > 0 ? ts[ts.Count - 1] : new CSagPracTouch { Why = "궤적 없음" };
                    CFloorContact c0 = cs[cs.Count - 1];
                    if (c0 != null && c0.Safety && !tOut.Fired) tOut.Why = SagRun_WhyJoin(tOut.Why, "안전 정지 (" + c0.Note + ")");
                }
                else
                {
                    CFloorContact c = SagRun_DryPress(arm, pc, model, fDry, run.L);
                    if (c == null) { run.Why = SagRun_WhyJoin("3D 작업 멈춤", arm.LastFail); return ESagPracPress.Fail; }
                    tOut = new CSagPracTouch { Fired = true, Valid = !float.IsNaN(fDry), CmdZ = fDry, Why = "3D 만 (가상 로봇)" };
                }
                arm.CurTransferZ = row.TransferZ; arm.CurZLow = row.ZStart + 10;
                run.LastKey = row.LineKey; run.LastR = p.R;
                if (SagRun_Stopped(run.Stop)) SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 누름을 마친 뒤 (이 누름은 씀)", p.R));   // 복귀 중에 온 중단 — 다음으로 가지 않는다
                return ESagPracPress.Ok;
            }
            // 중단 요청을 본 자리 — 한 번만 로그 (원위치는 부른 쪽의 SagArm_Close)
            private static void SagPrac_StopLog(CSagPracRun run, string strWhere)
            {
                if (!run.Stopped && run.L != null) run.L("중단 요청 — " + strWhere + " 에서 멈춤, 원위치로 돌아가 토크를 시작 상태로");
                run.Stopped = true;
            }
            // 검출 결과 → 점: 첫닿음(경첩 꺾임)이 있고 시작부터 닿아 있지 않았으면 그 명령 높이(이상 명령)·FK. 아니면 NaN — 꺾임점으로 바꾸지 않는다
            private static void SagPrac_TakeTouch(CSagPracTouch t, CSagPracPoint p, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                p.Knee = float.NaN;
                p.StartContact = t != null && t.Fired && t.StartContact;
                p.NoTouch = t == null || !t.Fired;
                if (t != null && t.Valid)
                {
                    p.Touch = t.CmdZ; p.Fk = t.FkZ; p.Kind = "F"; p.TouchQ = t.JointCmd; p.TouchA = t.JointAct;
                    if (L != null) L(string.Format(ci, "  r={0:0.#} 요 {1:0.#}: 첫닿음 명령 {2:F1} (FK {3:F1}) — {4}, 시작 아래 {5:F1}mm 에서 멈춤", p.R, p.Yaw, t.CmdZ, t.FkZ, t.Why, t.StartCz - t.StopCz));
                }
                else
                {
                    p.Touch = p.Fk = float.NaN; p.Kind = "";
                    string w = t == null ? "검출 없음" : (t.StartContact ? "시작부터 닿아 있었음 (창이 낮음)" : (t.Fired ? "첫닿음 꺾임을 못 찾음 (" + t.Why + ")" : "한도까지 첫닿음 없음" + (t.Why.Length > 0 ? " (" + t.Why + ")" : "")));
                    p.Note = SagRun_WhyJoin(p.Note, w);
                    if (L != null) L(string.Format(ci, "  ★r={0:0.#} 요 {1:0.#}: {2}", p.R, p.Yaw, w));
                }
            }
            // 2·3단계 시작 확인 — 가운데 점을 보정 없이 눌러(첫닿음에서 멈춤) 1단계 값과 견준다 (창 = 1단계 값 + 30 ~ − 22). 다르면 run.Why
            //   (바닥·팔 자리가 바뀜 → 1단계부터)
            private void SagPrac_CheckFloor(CSagPracRun run, CSagPracPoint mid, CSagPracResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                var p = new CSagPracPoint { Idx = mid.Idx, R = mid.R, Yaw = mid.Yaw, X = mid.X, Y = mid.Y, Pitch = mid.Pitch, Est = mid.Touch };
                run.L(string.Format(ci, "진행 시작 확인 — 가운데 점 r={0:0.#} 를 보정 없이 눌러 1단계({1:F1})와 견줌", mid.R, mid.Touch));
                var cfg0 = run.Cfg;
                CFloorPlanRow rowChk;
                ESagPracPress o = SagPrac_PressAt(run, p, mid.Touch, mid.Touch, 10, null, false, float.NaN, out rowChk);
                if (o == ESagPracPress.Stop || o == ESagPracPress.Fail) return;
                if (o != ESagPracPress.Ok || float.IsNaN(p.Touch))
                { run.Why = "시작 확인: 가운데 점에서 첫닿음을 재지 못함 (" + p.Note + ") — 바닥·팔 자리가 바뀌었으면 1단계부터 다시"; return; }
                res.CheckDiffMm = p.Touch - mid.Touch;
                run.L(string.Format(ci, "시작 확인: 첫닿음 {0:F1} (1단계 {1:F1}, 차 {2:+0.0;-0.0}mm)", p.Touch, mid.Touch, res.CheckDiffMm));
                if (Math.Abs(res.CheckDiffMm) > cfg0.CheckTolMm)
                    run.Why = string.Format(ci, "시작 확인: 가운데 점 바닥이 1단계와 {0:+0.0;-0.0}mm 다름 (허용 ±{1:0.#}) — 바닥·팔 자리가 바뀜: 1단계부터 다시", res.CheckDiffMm, cfg0.CheckTolMm);
            }

            // ────────────────────────────────────────────────────────────
            // 첫닿음 검출 (실시간 멈춤·궤적 다시 읽기가 같은 계산)
            //   실물 궤적(10-04, r190 등): 첫닿음 뒤에도 베이스가 기울며 FK 가 명령을 거의 1:1 로 따라 내려간다 — 그래서 '명령−FK 벌어짐' 만으로는
            //   늦게(단단한 정지) 보인다. 첫닿음에서 바로 보이는 것은 손목 잔차 d14 (= a14 − q14) 가 명령 하강 1mm 에 0.25~0.4° 씩 꾸준히 커지는 것
            //   (공중에서는 ±0.1° 안에서 흔들린다). 보조로 공중 기준(위 3~7mm)보다 명령−FK 차가 2mm 넘게 줄고 어깨 부하가 2% 넘게 풀림.
            //   첫닿음 높이 = d14 경첩(위 평평, 아래 기울기) 맞춤의 꺾임 — 멈춘 자리 위 8mm 안. 그 꺾임이 하강 시작 2mm 안이면 '시작부터 닿음'
            //   (창이 낮음 — 높이를 모른다). 시작 때 출발 걸림(명령−FK 차가 10mm 에서 4mm 로 줄어드는 구간)은 d14 가 천천히(≤0.15°/mm) 변해 걸리지 않는다
            // ────────────────────────────────────────────────────────────

            /// <summary>실시간 첫닿음 검출기 — 하강 틱마다 Add (유효 명령 높이, FK 높이, 어깨 부하 %, 손목 잔차 d14°). true = 닿음 (그 틱에 멈춘다)</summary>
            public sealed class CSagFtLive
            {
                public float WindowMm = 3f;            // d14 기울기를 보는 창 (명령 높이 mm)
                public float RateDegPerMm = 0.22f;     // d14 가 명령 하강 1mm 에 이만큼 넘게 커지면
                public int Repeat = 3;                 //   이만큼 잇달아 (틱)
                public float RiseDeg = 0.4f;           //   그리고 하강 중 가장 작았던 d14 보다 이만큼 위
                public float BaseSkipMm = 6f, BaseLoMm = 3f, BaseHiMm = 7f;   // 보조 판정의 공중 기준: 시작 6mm 뒤의, 지금보다 3~7mm 위 표본
                public float DeMm = 2f, DlPct = 2f;    // 보조: 명령−FK 차가 이만큼 줄고 어깨 부하가 이만큼 풀림
                public bool Fired; public string Why = ""; public int FireIndex = -1;
                internal readonly List<float[]> S = new List<float[]>();   // 명령, FK, 명령−FK, 어깨 부하, d14
                private int m_nRep; private float m_fDmin = float.NaN;
                public bool Add(float cz, float fz, float load12, float d14)
                {
                    if (Fired) return true;
                    if (float.IsNaN(cz) || float.IsNaN(fz) || float.IsInfinity(cz) || float.IsInfinity(fz)) return false;
                    S.Add(new float[] { cz, fz, cz - fz, load12, d14 });
                    float cz0 = S[0][0];
                    float dNow = Med(4, S.Count - 3, S.Count);
                    if (!float.IsNaN(dNow)) m_fDmin = float.IsNaN(m_fDmin) ? dNow : Math.Min(m_fDmin, dNow);
                    string why = null;
                    var ci = CultureInfo.InvariantCulture;
                    if (cz0 - cz >= WindowMm)
                    {
                        double sx = 0, sy = 0, sxx = 0, sxy = 0; int n = 0;
                        for (int i = S.Count - 1; i >= 0; i--)
                        {
                            if (S[i][0] > cz + WindowMm) continue;
                            if (float.IsNaN(S[i][4])) continue;
                            sx += S[i][0]; sy += S[i][4]; sxx += S[i][0] * S[i][0]; sxy += S[i][0] * S[i][4]; n++;
                        }
                        double rt = double.NaN;
                        if (n >= 5) { double d = n * sxx - sx * sx; if (d > 1e-9) rt = -(n * sxy - sx * sy) / d; }
                        m_nRep = (!double.IsNaN(rt) && rt >= RateDegPerMm) ? m_nRep + 1 : 0;
                        if (m_nRep >= Repeat && dNow - m_fDmin >= RiseDeg) why = string.Format(ci, "손목 잔차 {0:F2}°/mm, +{1:F2}°", rt, dNow - m_fDmin);
                    }
                    if (why == null && cz0 - cz >= BaseSkipMm + BaseHiMm)
                    {
                        var e = new List<float>(); var l = new List<float>();
                        foreach (float[] s in S) if (s[0] > cz + BaseLoMm && s[0] <= cz + BaseHiMm && s[0] <= cz0 - BaseSkipMm) { e.Add(s[2]); l.Add(s[3]); }
                        if (e.Count >= 6)
                        {
                            float de = MedOf(e) - Med(2, S.Count - 3, S.Count), dl = Med(3, S.Count - 3, S.Count) - MedOf(l);
                            if (de >= DeMm && dl >= DlPct) why = string.Format(ci, "명령−FK −{0:F1}mm, 어깨 부하 +{1:F1}%", de, dl);
                        }
                    }
                    if (why != null) { Fired = true; Why = why; FireIndex = S.Count - 1; }
                    return Fired;
                }
                private float Med(int col, int i0, int i1)
                {
                    var v = new List<float>();
                    for (int i = Math.Max(0, i0); i < i1; i++) if (!float.IsNaN(S[i][col])) v.Add(S[i][col]);
                    return MedOf(v);
                }
                private static float MedOf(List<float> v)
                {
                    if (v.Count == 0) return float.NaN;
                    float[] a = v.ToArray(); Array.Sort(a);
                    return (a.Length % 2 == 1) ? a[a.Length / 2] : 0.5f * (a[a.Length / 2 - 1] + a[a.Length / 2]);
                }
                /// <summary>첫닿음 — d14 경첩(꺾임 위 평평 b, 아래 b + m·(k − 명령)) 을 멈춘 자리 위 0.25~8mm 에서 0.25mm 씩 맞춰 잔차가 가장 작은 꺾임 k (m &gt; 0).
                /// idx = 꺾임에 가장 가까운 표본. 검출이 없으면 false</summary>
                public bool Onset(out float k, out int idx)
                {
                    k = float.NaN; idx = -1;
                    if (!Fired || FireIndex < 0) return false;
                    float czf = S[FireIndex][0], top = S[0][0];
                    var pts = new List<float[]>();
                    for (int i = 0; i <= FireIndex; i++) if (S[i][0] <= czf + 10f && !float.IsNaN(S[i][4])) pts.Add(S[i]);
                    double best = double.MaxValue;
                    for (float kk = czf + 0.25f; kk <= Math.Min(czf + 8f, top) + 1e-4f; kk += 0.25f)
                    {
                        int nUp = 0, nDn = 0; double sx = 0, sxx = 0, sy = 0, sxy = 0;
                        foreach (float[] p in pts)
                        {
                            if (p[0] >= kk) nUp++; else nDn++;
                            double uu = Math.Max(0.0, kk - p[0]); sx += uu; sxx += uu * uu; sy += p[4]; sxy += uu * p[4];
                        }
                        if (nUp < 4 || nDn < 3) continue;
                        int N = pts.Count; double det = N * sxx - sx * sx;
                        if (det <= 1e-9) continue;
                        double m = (N * sxy - sx * sy) / det, b = (sy - m * sx) / N;
                        if (m <= 0) continue;
                        double se = 0; foreach (float[] p in pts) { double r = p[4] - b - m * Math.Max(0.0, kk - p[0]); se += r * r; }
                        double rms = Math.Sqrt(se / N);
                        if (rms < best - 1e-9) { best = rms; k = kk; }
                    }
                    if (float.IsNaN(k)) return false;
                    double bd = double.MaxValue;
                    for (int i = 0; i <= FireIndex; i++) { double dd = Math.Abs(S[i][0] - k); if (dd < bd) { bd = dd; idx = i; } }
                    return true;
                }
            }

            /// <summary>하강 하나의 첫닿음 (SagPrac_TouchFromTrace)</summary>
            public class CSagPracTouch
            {
                public float R, X, Y, Pitch = 90;
                public bool Fired;                      // 검출기가 걸렸다 (실시간이면 그 틱에 멈췄다)
                public bool Valid;                      // 첫닿음 높이를 냈다 (걸렸고, 꺾임을 찾았고, 시작부터 닿아 있지 않았다)
                public bool StartContact;               // 꺾임이 하강 시작 2mm 안 — 시작부터 닿아 있었다 (창이 낮음)
                public float StartCz = float.NaN, StopCz = float.NaN;   // 하강 시작·검출(또는 끝) 명령 높이 (이상 명령)
                public float CmdZ = float.NaN, FkZ = float.NaN;          // 첫닿음 명령 높이 (이상 명령 — 3D 가 믿는 끝 높이)·FK 높이
                public float[] JointCmd, JointAct;      // 첫닿음 순간 이상·실측 관절각 (위치 3개 + 손목)
                public string Why = "";
                public bool Hold;                       // 손목 유지 켬으로 잰 하강
            }
            /// <summary>바닥 궤적(SagComp_GetFloorTraceCsv 형식, 이어 붙인 파일도)을 하강(seg 1)마다 실시간 첫닿음 검출기(CSagFtLive)로 다시 읽는다 — 실물 누름이
            /// 멈춘 그 판정과 같은 계산. 하강마다 결과 하나 (시간 순). 유효 명령(명령 + fd) 으로 검출하고 높이는 이상 명령으로 되돌린다. 예외 없음.</summary>
            public static List<CSagPracTouch> SagPrac_TouchFromTrace(string strTraceCsv)
            {
                var res = new List<CSagPracTouch>();
                try
                {
                    if (string.IsNullOrEmpty(strTraceCsv)) return res;
                    var ci = CultureInfo.InvariantCulture;
                    string[] hd = null;
                    Dictionary<string, int> ix = null;
                    CSagFtLive ft = null; CSagPracTouch cur = null;
                    var fds = new List<float>(); var qs = new List<float[]>(); var acts = new List<float[]>();
                    int lastTick = int.MaxValue; string lastR = null;
                    Action close = delegate()
                    {
                        if (cur == null) return;
                        SagPrac_FinishTouch(cur, ft, fds, qs, acts);
                        res.Add(cur); cur = null; ft = null;
                    };
                    Func<string[], string, float> F = (f, n) =>
                    {
                        int i; float v;
                        return (ix != null && ix.TryGetValue(n, out i) && i < f.Length && float.TryParse(f[i], NumberStyles.Float, ci, out v)) ? v : float.NaN;
                    };
                    foreach (string ln0 in strTraceCsv.Replace("\r", "").Split('\n'))
                    {
                        string ln = ln0.Trim(); if (ln.Length == 0) continue;
                        string[] f = ln.Split(',');
                        if (f[0] == "r") { hd = f; ix = new Dictionary<string, int>(); for (int i = 0; i < f.Length; i++) ix[f[i].Trim()] = i; continue; }
                        if (ix == null || !ix.ContainsKey("seg") || !ix.ContainsKey("cz")) continue;
                        int seg; int.TryParse(f[ix["seg"]], NumberStyles.Integer, ci, out seg);
                        float tk = F(f, "tick");
                        if (seg != 1) { close(); continue; }
                        int tick = float.IsNaN(tk) ? 0 : (int)tk;
                        if (cur == null || tick <= lastTick || f[0] != lastR)
                        {
                            close();
                            cur = new CSagPracTouch { R = F(f, "r"), X = F(f, "x"), Y = F(f, "y"), Pitch = F(f, "pitch") };
                            if (float.IsNaN(cur.Pitch)) cur.Pitch = 90;
                            ft = new CSagFtLive(); fds.Clear(); qs.Clear(); acts.Clear();
                        }
                        lastTick = tick; lastR = f[0];
                        if (ft.Fired) continue;                                       // 실시간이면 여기서 멈췄다 — 그 뒤 표본은 보지 않는다
                        float fd = F(f, "fd"); if (float.IsNaN(fd)) fd = 0;
                        float cz = F(f, "cz"), fz = F(f, "fz");
                        float[] q = { F(f, "q1"), F(f, "q2"), F(f, "q3"), F(f, "q4") }, a = { F(f, "a1"), F(f, "a2"), F(f, "a3"), F(f, "a4") };
                        float wh = F(f, "wh"); if (!float.IsNaN(wh)) cur.Hold = true;
                        float d14 = (float.IsNaN(a[3]) || float.IsNaN(q[3])) ? float.NaN : a[3] - q[3];
                        int n0 = ft.S.Count;
                        ft.Add(cz + fd, fz, F(f, "load12"), d14);
                        if (ft.S.Count > n0) { fds.Add(fd); qs.Add(q); acts.Add(a); }
                    }
                    close();
                }
                catch { }
                return res;
            }
            private static void SagPrac_FinishTouch(CSagPracTouch t, CSagFtLive ft, List<float> fds, List<float[]> qs, List<float[]> acts)
            {
                if (ft == null || ft.S.Count == 0) { t.Why = "표본 없음"; return; }
                t.StartCz = ft.S[0][0] - fds[0];
                int iStop = ft.Fired ? ft.FireIndex : ft.S.Count - 1;
                t.StopCz = ft.S[iStop][0] - fds[iStop];
                t.Fired = ft.Fired; t.Why = ft.Why;
                if (!ft.Fired) return;
                float k; int idx;
                if (!ft.Onset(out k, out idx)) { t.Why = SagRun_WhyJoin(t.Why, "꺾임 맞춤 안 됨"); return; }
                t.CmdZ = k - fds[idx]; t.FkZ = ft.S[idx][1];
                t.JointCmd = qs[idx]; t.JointAct = acts[idx];
                t.StartContact = ft.S[0][0] - k < 2f;
                t.Valid = !t.StartContact;
            }

            // 넘겨짚기 — 잰 점(known, Touch)으로 (r, 요) 의 닿는 높이. 같은 요의 점 사이면 직선 보간 (여유 0), 바깥이면 가까운 두 점으로 늘임
            //   (기울기 ±0.6 로 자름, 여유 fUnknown), 한 점뿐이면 그 값 (여유 fUnknown + 0.1·거리). 같은 요가 없으면 요 0 (없으면 전부) 로 하고 여유 + fUnknown
            private static float SagPrac_Estimate(List<CSagPracPoint> known, float r, float yaw, float fUnknown, out float u)
            {
                u = float.NaN;
                if (known == null) return float.NaN;
                float extra = 0;
                List<CSagPracPoint> same = known.FindAll(k => Math.Abs(k.Yaw - yaw) < 0.5f && !float.IsNaN(k.Touch));
                if (same.Count == 0) { same = known.FindAll(k => Math.Abs(k.Yaw) < 0.5f && !float.IsNaN(k.Touch)); extra = fUnknown; }
                if (same.Count == 0) { same = known.FindAll(k => !float.IsNaN(k.Touch)); extra = fUnknown; }
                if (same.Count == 0) return float.NaN;
                same.Sort((a, b) => a.R.CompareTo(b.R));
                foreach (CSagPracPoint k in same) if (Math.Abs(k.R - r) < 0.5f) { u = extra; return k.Touch; }
                for (int i = 0; i + 1 < same.Count; i++)
                    if (same[i].R < r && same[i + 1].R > r)
                    {
                        float t = (r - same[i].R) / (same[i + 1].R - same[i].R);
                        u = extra; return same[i].Touch + t * (same[i + 1].Touch - same[i].Touch);
                    }
                if (same.Count == 1) { u = extra + fUnknown + 0.1f * Math.Abs(r - same[0].R); return same[0].Touch; }
                CSagPracPoint a0, b0;
                if (r < same[0].R) { a0 = same[0]; b0 = same[1]; } else { a0 = same[same.Count - 1]; b0 = same[same.Count - 2]; }
                float slope = (b0.Touch - a0.Touch) / (b0.R - a0.R);
                slope = Math.Max(-0.6f, Math.Min(0.6f, slope));
                u = extra + fUnknown;
                return a0.Touch + slope * (r - a0.R);
            }
            // 가운데 점 — 요 0 점 중 반경이 가운데(중앙값)에 가장 가까운 점 (요 0 이 없으면 전체에서)
            private static int SagPrac_MidIndex(List<CSagPracPoint> pts)
            {
                var y0 = new List<float>();
                foreach (CSagPracPoint p in pts) if (Math.Abs(p.Yaw) < 0.5f) y0.Add(p.R);
                bool bAll = y0.Count == 0;
                if (bAll) foreach (CSagPracPoint p in pts) y0.Add(p.R);
                y0.Sort();
                float med = y0.Count == 0 ? 0 : y0[y0.Count / 2];
                int best = 0; float bd = float.MaxValue;
                for (int i = 0; i < pts.Count; i++)
                {
                    if (!bAll && Math.Abs(pts[i].Yaw) >= 0.5f) continue;
                    float d = Math.Abs(pts[i].R - med);
                    if (d < bd) { bd = d; best = i; }
                }
                return best;
            }
            // 누를 순서 — 가운데 점, 같은 요의 바깥쪽(반경 오름), 안쪽(반경 내림), 그다음 다른 요 줄 (요마다 가운데 반경에서 가까운 순)
            private static List<CSagPracPoint> SagPrac_Order(List<CSagPracPoint> pts, int iMid)
            {
                var l = new List<CSagPracPoint>();
                if (iMid < 0 || iMid >= pts.Count) iMid = 0;
                CSagPracPoint m = pts[iMid];
                l.Add(m);
                var same = pts.FindAll(p => !ReferenceEquals(p, m) && Math.Abs(p.Yaw - m.Yaw) < 0.5f);
                var outer = same.FindAll(p => p.R > m.R); outer.Sort((a, b) => a.R.CompareTo(b.R));
                var inner = same.FindAll(p => p.R <= m.R); inner.Sort((a, b) => b.R.CompareTo(a.R));
                l.AddRange(outer); l.AddRange(inner);
                var rest = pts.FindAll(p => !l.Contains(p));
                rest.Sort((a, b) => { int c = a.Yaw.CompareTo(b.Yaw); return c != 0 ? c : Math.Abs(a.R - m.R).CompareTo(Math.Abs(b.R - m.R)); });
                l.AddRange(rest);
                return l;
            }
            // 2단계 (b) 의 점
            private static List<CSagPracPoint> SagPrac_DataPoints(CSagPracCfg cfg, bool bPrecise)
            {
                var l = new List<CSagPracPoint>();
                Action<float, float> Add = (r, yaw) =>
                {
                    double a = yaw * Math.PI / 180.0;
                    l.Add(new CSagPracPoint { Idx = l.Count, R = r, Yaw = yaw, X = (float)(r * Math.Cos(a)), Y = (float)(r * Math.Sin(a)), Pitch = cfg.Pitch });
                };
                if (bPrecise)
                {
                    foreach (float r in cfg.PreciseDataRadii ?? new float[0]) Add(r, 0);
                    foreach (float y in cfg.PreciseDataYaws ?? new float[0]) foreach (float r in cfg.PreciseDataRadii ?? new float[0]) Add(r, y);
                }
                else
                {
                    foreach (float r in cfg.QuickDataRadii ?? new float[0]) Add(r, 0);
                    foreach (float y in cfg.QuickDataYaws ?? new float[0]) foreach (float r in cfg.QuickDataYawRadii ?? new float[0]) Add(r, y);
                }
                return l;
            }
            // 모델 m 이 (x, y, z, 피치) 자세에서 끝을 올리는 양 (mm) — 그 자세의 이상 관절각에 보정(SagComp_PredictJointCorr — 관절 + 바닥, 방향 0)을 더한 FK − 이상 FK.
            //   −20 ~ 40 으로 자른다. 계산 못 하면 0
            private static float SagPrac_Lift(C3d kin, CSagArmCfg arm, CSagComp m, float x, float y, float z, float pitch)
            {
                try
                {
                    if (kin == null || m == null) return 0;
                    float bend;
                    double[] ad = SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), pitch);
                    float[] q = SagArm_IkSum(kin, arm, ad, x, y, z, SagArm_SumFor(arm, pitch), out bend);
                    if (q == null) return 0;
                    float[] corr = kin.SagComp_PredictJointCorr(m, arm.MoveIDs, q, null);
                    if (corr == null) return 0;
                    double[] a1 = SagArm_AdWith(kin, arm, ad, q);
                    float[] q2 = new float[4]; for (int k = 0; k < 4; k++) q2[k] = q[k] + corr[k];
                    double[] a2 = SagArm_AdWith(kin, arm, ad, q2);
                    float x1, y1, z1, x2, y2, z2;
                    kin.SagFc_Fk(arm.FuncNumber, a1, -1, out x1, out y1, out z1);
                    kin.SagFc_Fk(arm.FuncNumber, a2, -1, out x2, out y2, out z2);
                    float d = z2 - z1;
                    if (float.IsNaN(d) || float.IsInfinity(d)) return 0;
                    return Math.Max(-20f, Math.Min(40f, d));
                }
                catch { return 0; }
            }

            // ────────────────────────────────────────────────────────────
            // 3D 만 — 가상 로봇
            //   실물 = 보낸 관절각 + c·지레(보낸 자세) − h·방향 (관절 모델과 같은 식), 끝 = 그 FK − 휨 (반경 120 → 0, 260 → SimExtMm, + 출렁임 SimRippleMm + 자리마다 SimNoiseMm), 바닥 = SimFloorZ.
            //   보정 모델이 있으면 보낸 관절각 = 이상 + SagComp_PredictJointCorr (출력 단과 같은 보정, 전환 끝난 뒤)
            // ────────────────────────────────────────────────────────────
            private static float[] SagPrac_SimAct(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, float[] sent, float[] sigma)
            {
                float[] lev = kin.SagFc_Levers(arm.FuncNumber, arm.MoveIDs, sent, kin.SagFc_MotCopy());
                float[] a = new float[4];
                for (int k = 0; k < 4; k++)
                {
                    float c = (cfg.SimJc != null && k < cfg.SimJc.Length) ? cfg.SimJc[k] : 0, h = (cfg.SimJh != null && k < cfg.SimJh.Length) ? cfg.SimJh[k] : 0;
                    a[k] = sent[k] + c * lev[k] - h * (sigma != null && k < sigma.Length ? sigma[k] : 0);
                }
                return a;
            }
            // 가상 로봇의 실제 끝 높이 (바닥 기준과 같은 FK 높이에서 휨을 뺀 값), fk = 실측 관절의 FK 높이
            private static double SagPrac_SimTip(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, float[] act, out double fk)
            {
                double[] ad = SagArm_AdWith(kin, arm, kin.SagFc_MotCopy(), act);
                float x, y, z;
                kin.SagFc_Fk(arm.FuncNumber, ad, -1, out x, out y, out z);
                fk = z;
                double r = Math.Sqrt((double)x * x + (double)y * y);
                double ext = cfg.SimExtMm * Math.Max(0.0, Math.Min(1.0, (r - 120.0) / 140.0)) + cfg.SimRippleMm * Math.Sin(r * 0.05);
                double h = Math.Sin(Math.Round(x / 5.0) * 12.9898 + Math.Round(y / 5.0) * 78.233) * 43758.5453;
                ext += cfg.SimNoiseMm * (2.0 * (h - Math.Floor(h)) - 1.0);        // 자리(5mm 칸)마다 정해진 값
                return z - ext;
            }
            // 가상 로봇이 (x, y, 피치) 에서 바닥에 닿는 명령 높이 — 위에서 내려온다 (방향 = 내려갈 때의 관절 변화 부호). 이분법 (바닥 −80 ~ +120 사이).
            //   m = 켠 보정 모델 (null = 보정 없음). qIdeal·qAct = 그때 이상·실측 관절각, fk = 실측 FK 높이
            private static bool SagPrac_SimTouch(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, float x, float y, float pitch, CSagComp m,
                                                 out float zCmd, out float fk, out float[] qIdeal, out float[] qAct)
            {
                zCmd = fk = float.NaN; qIdeal = qAct = null;
                try
                {
                    if (kin == null) return false;
                    double[] seed = SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), pitch);
                    float sum = SagArm_SumFor(arm, pitch);
                    float lo = cfg.SimFloorZ - 80, hi = cfg.SimFloorZ + 120;
                    float[] qi = null, qa = null; double fkv = double.NaN;
                    Func<float, double> F = delegate(float z)
                    {
                        float bend;
                        float[] q = SagArm_IkSum(kin, arm, seed, x, y, z, sum, out bend);
                        float[] qUp = SagArm_IkSum(kin, arm, seed, x, y, z + 10, sum, out bend);
                        if (q == null) return double.NaN;
                        float[] sg = new float[4];
                        if (qUp != null) for (int k = 0; k < 4; k++) sg[k] = Math.Sign(q[k] - qUp[k]);
                        float[] sent = (float[])q.Clone();
                        if (m != null)
                        {
                            float[] corr = kin.SagComp_PredictJointCorr(m, arm.MoveIDs, q, sg);
                            if (corr != null) for (int k = 0; k < 4; k++) sent[k] += corr[k];
                        }
                        float[] act = SagPrac_SimAct(kin, arm, cfg, sent, sg);
                        double f;
                        double tip = SagPrac_SimTip(kin, arm, cfg, act, out f);
                        qi = q; qa = act; fkv = f;
                        return tip - cfg.SimFloorZ;
                    };
                    double fl = F(lo), fh = F(hi);
                    for (int i = 0; i < 12 && (double.IsNaN(fl) || fl > 0); i++) { lo += 10; fl = F(lo); }   // 아래 끝이 IK 로 안 되면 올린다
                    for (int i = 0; i < 16 && double.IsNaN(fh) && hi - 10 > lo; i++) { hi -= 10; fh = F(hi); }   // 위 끝이 팔이 닿지 않는 높이면 내린다
                    if (double.IsNaN(fl) || double.IsNaN(fh) || fl > 0 || fh < 0) return false;
                    for (int it = 0; it < 40; it++)
                    {
                        float mid = 0.5f * (lo + hi);
                        double fm = F(mid);
                        if (double.IsNaN(fm)) return false;
                        if (fm > 0) hi = mid; else lo = mid;
                        if (hi - lo < 0.01f) break;
                    }
                    float zz = 0.5f * (lo + hi);
                    if (double.IsNaN(F(zz))) return false;
                    zCmd = zz; fk = (float)fkv; qIdeal = qi; qAct = qa;
                    return true;
                }
                catch { return false; }
            }
            // 가상 관절 처짐 측정 — 점마다 이상 자세 (관절 합 IK), 방향 = 앞 점에서의 변화 부호 (안 바뀌면 그대로), 실측 = 가상 로봇
            private static List<CSagSample> SagPrac_SimSurvey(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, List<float[]> pts)
            {
                var l = new List<CSagSample>();
                float[] prev = null, dir = new float[] { 1, 1, 1, 1 };
                double[] ad = SagRun_Seed(kin, arm, 0, 90);
                foreach (float[] p in pts)
                {
                    float x = (float)(p[0] * Math.Cos(p[1] * Math.PI / 180.0)), y = (float)(p[0] * Math.Sin(p[1] * Math.PI / 180.0));
                    float bend;
                    float[] q = SagArm_IkSum(kin, arm, ad, x, y, p[2], arm.DownSumDeg, out bend);      // 피치 90 = 정의의 수직 관절 합
                    if (q == null) continue;
                    if (prev != null) for (int k = 0; k < 4; k++) if (Math.Abs(q[k] - prev[k]) >= 0.05f) dir[k] = q[k] > prev[k] ? 1 : -1;
                    prev = q;
                    float[] act = SagPrac_SimAct(kin, arm, cfg, q, dir);
                    l.Add(SagPrac_Sample(kin, arm, q, act, (int)p[3], dir));
                }
                return l;
            }
            // 가상 바닥 첫닿음 측정 (관절 모델 켬) → 데이터셋 (실습 폴더 안 파일, 새로) — 반환 = 6행 이상 썼다
            private bool SagPrac_SimData(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, CSagComp joint, List<CSagPracPoint> data, List<CSagPracPoint> known,
                                         string dsPath, CSagPracResult res, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                var cs = new List<CFloorContact>();
                foreach (CSagPracPoint p in data)
                {
                    float z, fk; float[] qi, qa;
                    if (!SagPrac_SimTouch(kin, arm, cfg, p.X, p.Y, p.Pitch, joint, out z, out fk, out qi, out qa)) continue;
                    p.Touch = z; p.Fk = fk; p.Kind = "F";
                    cs.Add(new CFloorContact { R = p.R, X = p.X, Y = p.Y, Pitch = p.Pitch, CmdZ = z, FkZ = fk, Detected = true, JointCmd = qi, JointAct = qa,
                                               Kind = "F", FcZ = fk, FcCmdZ = z, Hold = cfg.DataWristHold, FcJointCmd = qi, FcJointAct = qa });
                }
                res.DataRows = cs.Count;
                try { if (File.Exists(dsPath)) File.Delete(dsPath); } catch { }
                int n = SagRun_AppendDataset(dsPath, SagRun_DatasetRows(cs, "sim_" + DateTime.Now.ToString("MMdd_HHmmss", ci), true));
                res.DatasetRowsAdded = Math.Max(0, n);
                if (L != null) L(string.Format(ci, "3D 만 — 가상 바닥 첫닿음 {0}점 → {1} ({2}행)", cs.Count, Path.GetFileName(dsPath), res.DatasetRowsAdded));
                return res.DatasetRowsAdded >= 6;
            }

            // ────────────────────────────────────────────────────────────
            // 파일·상태 도우미
            // ────────────────────────────────────────────────────────────
            private static CSagPracResult SagPrac_Refuse(CSagPracResult res, string why, CSim2Real.DLog L)
            {
                res.Ok = false; res.Why = why ?? ""; res.Line = "시작하지 않음 — " + res.Why; res.Report = res.Line;
                if (L != null) L("★" + res.Line);
                return res;
            }
            // 새 실습 폴더 <urdf>\sagcomp_practice\<yyyyMMdd_HHmmss>[_3d][_n]
            private static string SagPrac_NewFolder(string strUrdfDir, DateTime t, bool bDry)
            {
                string root = SagPrac_Dir(strUrdfDir);
                Directory.CreateDirectory(root);
                string stem = t.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + (bDry ? "_3d" : "");
                for (int n = 1; n < 1000; n++)
                {
                    string p = Path.Combine(root, stem + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : ""));
                    if (Directory.Exists(p) || File.Exists(p)) continue;
                    Directory.CreateDirectory(p);
                    return p;
                }
                throw new IOException("실습 폴더 이름을 만들지 못함 (" + root + ")");
            }
            // 실습 폴더 안 단계 작업 폴더 (다시 하면 _2, _3 …)
            private static string SagPrac_NewRunDir(string strFolder, string strName)
            {
                for (int n = 1; n < 1000; n++)
                {
                    string p = Path.Combine(strFolder, strName + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : ""));
                    if (Directory.Exists(p) || File.Exists(p)) continue;
                    Directory.CreateDirectory(p);
                    return p;
                }
                throw new IOException("작업 폴더 이름을 만들지 못함 (" + strFolder + ")");
            }
            // 폴더 안 새 파일 이름 (있으면 _2, _3 …)
            private static string SagPrac_FreeName(string strDir, string strStem, string strExt)
            {
                for (int n = 1; n < 1000; n++)
                {
                    string p = Path.Combine(strDir, strStem + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : "") + strExt);
                    if (!File.Exists(p)) return p;
                }
                return Path.Combine(strDir, strStem + "_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture) + strExt);
            }
            // 단계 앞 확인 — 진행 상태가 nNeed 단계 이상이고 같은 종류(실물/3D 만)이고 before.csv 에 닿은 점이 3개 이상. 아니면 null + 까닭
            private static CSagPracState SagPrac_StateFor(string strUrdfDir, int nNeed, bool bDry, out List<CSagPracPoint> before, out string why)
            {
                before = null; why = "";
                CSagPracState st = SagPrac_LoadState(strUrdfDir);
                if (st.Step < 1) { why = "1. 오차 재기를 먼저 하십시오"; return null; }
                if (st.Step < nNeed) { why = nNeed == 2 ? "2. 보정 데이터 만들기를 먼저 하십시오" : "앞 단계를 먼저 하십시오"; return null; }
                if (st.Dry != bDry) { why = st.Dry ? "마지막 1단계는 3D 만이었습니다 — 실물로 하려면 1. 오차 재기부터 실물로" : "마지막 1단계는 실물이었습니다 — 3D 만으로 하려면 1. 오차 재기부터 3D 만으로"; return null; }
                before = SagPrac_ReadPoints(Path.Combine(st.FolderDir, "before.csv"));
                int n = 0; if (before != null) foreach (CSagPracPoint p in before) if (!float.IsNaN(p.Touch)) n++;
                if (n < 3) { why = "1단계 결과(before.csv)를 읽지 못했거나 닿은 점이 3개 미만 — 1. 오차 재기부터 다시"; return null; }
                return st;
            }
            private static string SagPrac_PointsCsv(List<CSagPracPoint> pts)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder("idx,r,yaw,x,y,pitch,touch,fk,kind,knee,est,note\r\n");
                Func<float, string> V = f => (float.IsNaN(f) || float.IsInfinity(f)) ? "" : f.ToString("0.###", ci);
                if (pts != null)
                    foreach (CSagPracPoint p in pts)
                        sb.Append(string.Format(ci, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\r\n", p.Idx, V(p.R), V(p.Yaw), V(p.X), V(p.Y), V(p.Pitch),
                            V(p.Touch), V(p.Fk), (p.Kind ?? "").Replace(",", ";"), V(p.Knee), V(p.Est), (p.Note ?? "").Replace(",", ";").Replace("\r", " ").Replace("\n", " ")));
                return sb.ToString();
            }
            /// <summary>before.csv / after.csv 읽기 (머리줄 이름으로) — 없거나 못 읽으면 null. 예외 없음</summary>
            public static List<CSagPracPoint> SagPrac_ReadPoints(string strPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) return null;
                    string[] lines = File.ReadAllLines(strPath, Encoding.UTF8);
                    if (lines.Length == 0) return null;
                    var hd = new List<string>(); foreach (string h in lines[0].TrimStart('﻿').Split(',')) hd.Add(h.Trim());
                    var ci = CultureInfo.InvariantCulture;
                    Func<string[], string, float> F = (f, n) =>
                    {
                        int i = hd.IndexOf(n); float v;
                        return (i >= 0 && i < f.Length && float.TryParse(f[i].Trim(), NumberStyles.Float, ci, out v)) ? v : float.NaN;
                    };
                    Func<string[], string, string> S = (f, n) => { int i = hd.IndexOf(n); return (i >= 0 && i < f.Length) ? f[i].Trim() : ""; };
                    var l = new List<CSagPracPoint>();
                    for (int k = 1; k < lines.Length; k++)
                    {
                        if (lines[k].Trim().Length == 0) continue;
                        string[] f = lines[k].Split(',');
                        float idx = F(f, "idx"), r = F(f, "r"), yaw = F(f, "yaw");
                        if (float.IsNaN(idx) || float.IsNaN(r) || float.IsNaN(yaw)) continue;
                        var p = new CSagPracPoint { Idx = (int)idx, R = r, Yaw = yaw, X = F(f, "x"), Y = F(f, "y"), Pitch = F(f, "pitch"), Touch = F(f, "touch"), Fk = F(f, "fk"),
                                                    Kind = S(f, "kind"), Knee = F(f, "knee"), Est = F(f, "est"), Note = S(f, "note") };
                        if (float.IsNaN(p.Pitch)) p.Pitch = 90;
                        double a = yaw * Math.PI / 180.0;
                        if (float.IsNaN(p.X) || float.IsNaN(p.Y)) { p.X = (float)(r * Math.Cos(a)); p.Y = (float)(r * Math.Sin(a)); }
                        l.Add(p);
                    }
                    return l;
                }
                catch { return null; }
            }
            private static bool SagPrac_Finite(float f) { return !float.IsNaN(f) && !float.IsInfinity(f); }
            private static string SagPrac_V(float f) { return SagPrac_Finite(f) ? f.ToString("F1", CultureInfo.InvariantCulture) : "—"; }
            private static double SagPrac_Mean(List<double> v) { if (v == null || v.Count == 0) return double.NaN; double s = 0; foreach (double x in v) s += x; return s / v.Count; }
            private static double SagPrac_Min(List<double> v) { double m = double.NaN; foreach (double x in v) if (double.IsNaN(m) || x < m) m = x; return m; }
            private static double SagPrac_Max(List<double> v) { double m = double.NaN; foreach (double x in v) if (double.IsNaN(m) || x > m) m = x; return m; }
            private static double SagPrac_Spread(List<double> v) { return (v == null || v.Count == 0) ? double.NaN : SagPrac_Max(v) - SagPrac_Min(v); }
            private static double SagPrac_Rms(List<double> v, double c)
            {
                if (v == null || v.Count == 0 || double.IsNaN(c)) return double.NaN;
                double s = 0; foreach (double x in v) s += (x - c) * (x - c);
                return Math.Sqrt(s / v.Count);
            }
        }
    }
}
