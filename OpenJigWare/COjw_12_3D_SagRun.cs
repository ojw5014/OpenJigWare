// ================================================================
// C3d — 로봇팔 세팅 절차 (처짐 바닥 실험)  2026-10-02
//   MakeUrdf "로봇팔 세팅" 탭이 버튼마다 부르는 절차를 한곳에 모은다. 앱은 부르기만 한다.
//   ② 측정 계획 (SagRun_MakePlan/CheckPlan) — 첫닿음 모델 예측으로 점마다 하강 창 (시작 = 예측 + 20, 한도 = max(3, 예측 − 12))
//   ④ 학습 → 결과 파일 (SagRun_Learn) — 관절 모델 바탕 + 첫닿음 데이터셋(<urdf>\floor_dataset_fc.csv)
//      → <urdf>\sagcomp_results\sagcomp_model_<yyyyMMdd_HHmm>.txt + _report.txt + _data.csv
//   ⑤ 결과 파일 시험: 틈 재기 계획(SagRun_GapPlan)·닿기 시험 점(SagRun_TestPoints), 판정(SagRun_GapEvaluate/TestEvaluate),
//      시험 기록(<결과>_test.txt 에 [틈]/[닿기] 한 줄씩)
//   ⑥ 적용·백업·되돌리기 (SagRun_Activate/Rollback), 쌍둥이 미리 보기 (SagRun_TwinUse), 결과·백업 목록
//   설정 파일 <urdf>\sagarm.txt (CSagArmSetup), 경로·데이터셋 도우미.
//   실물을 움직이는 작업(연결 빌리기·펼치기·안전 이동·원위치, 바닥 측정, 틈 재기·닿기 시험 실행)은 로봇팔 세팅 탭의
//   연결 객체를 빌려 쓰는 함수로 이 파일에 더한다 — 여기 있는 것은 실물·포트 없이 도는 부분이고, 그 작업들이 이 계획·판정·파일 함수를 쓴다.
// 계산은 계획 기구학에서 한다 — 지금 3D 의 수식 글(m_CHeader.pstrKinematics)로 만든 따로 된 C3d (3D 에 수식이 없으면 내장 OMX follower —
//   CScene_t.CreateOmx 의 DH, 09-28/29 데이터셋을 만든 그 기구학). 화면 3D 를 흔들지 않는다. 로봇 정의(공구 끝·꺾는 손목·바닥으로 수직)의 확인은
//   COjw_12_3D_SagDef.cs (SagDef_Check — 작업 입구·계획·학습이 부른다).
//   IK·FK·첫닿음 특징은 관절값 사본으로 계산한다 (SagFc_CalcInvTool·SagFc_Fk, COjw_12_3D_SagFc.cs) — 3D 관절값·CalcF 정적 상태를
//   건드리지 않아 모션·실물 스트리밍 중에 다른 스레드에서 불러도 보내는 명령에 섞이지 않는다.
//   계획 기구학(C3d = OpenGL 컨트롤)은 UI(STA) 스레드에서 처음 한 번만 만든다 (화면 3D 의 GL 컨텍스트는 되돌린다) — 앱은 시작 때·로봇팔 세팅 탭을 볼 때 SagRun_Prepare() 를 부른다
//   (3D 의 DH 행이 바뀌면 STA 이고 아무도 계획 기구학을 쓰지 않을 때 같은 C3d 의 수식 슬롯만 바꿔 CheckForward — 글만 바뀌고 DH 행이 같으면 그대로.
//   작업 스레드·작업·학습·계획 중에는 바꾸지 않는다 — DH 행까지 바뀌었으면 정의 확인이 '3D 모델이 바뀜' 으로 거절).
// 공개 함수는 예외를 밖으로 던지지 않는다 — 안에서 잡아 "★오류: …" 로그 + 실패 결과. 로그는 점·단계마다만 (틱마다 금지).
// 결과 파일·백업·기록은 활성 파일(<urdf>\sagcomp_model.txt)과 같은 형식이고, 활성 파일을 바꾸는 길은 적용·되돌리기뿐이다 (늘 백업 + apply_log,
//   새 내용은 같은 폴더 임시 파일로 받아 확인한 뒤 File.Replace 로 바꿔 끼운다 — 활성 파일을 먼저 지우지 않으므로 도중에 실패해도 활성 파일이
//   없어지거나 반만 쓰인 채 남지 않는다. 다른 프로그램이 파일을 잡고 있어 끝내 바꾸지 못하면 활성 파일 그대로 실패한다).
// 옮기는 높이(TransferZ)는 창 위(측정·시험 = 시작 + 10, 틈 재기 = 펜 높이 + 5)보다 낮지 않다 — SafeZ 가 그보다 낮으면 그 점은 빠진다 (IK 확인 실패로 표시).
// ================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            // ────────────────────────────────────────────────────────────
            // 설정
            // ────────────────────────────────────────────────────────────

            /// <summary>실물 이동 설정 (OMX follower 기본값). 단위 mm / deg / ms.
            /// 포트·속도는 로봇팔 세팅 탭의 연결 줄이 가진다 (여기·sagarm.txt 에 없다).</summary>
            public class CSagArmCfg
            {
                // ── 로봇 정의 (sagarm.txt 의 robot·func·posids·wrist·holdids·tcplen·downsum — 확인은 SagDef_Check, COjw_12_3D_SagDef.cs) ──
                public string DefName = "OMX follower";              // 정의 이름 (robot=)
                public int FuncNumber = 0;                           // 공구 끝(TCP)이 끝점인 IK 수식 — TCP = 그 체인의 첫 '!' 바로 앞 DH 줄 (OMX 예제 5·6 = 0, TCP = 손목(14) 축에서 공구 쪽 103.2mm)
                public int[] PosIDs = new int[] { 11, 12, 13 };      // 베이스(요), 피치 관절 1, 2
                public int WristID = 14;                             // 공구를 바닥으로 수직으로 꺾는 손목 (공구 끝 앞 마지막 관절)
                public int[] HoldIDs = new int[] { 15, 16 };         // 토크만 켜서 켜는 순간 각도로 고정 (그리퍼) — 움직이지 않는다 (공구 끝 체인 밖)
                public float TcpLenMm = 103.2f;                      // 손목 원점 → 공구 끝 기대 거리 (mm, NaN = 확인 안 함)
                public float DownSumDeg = 90f;                       // 바닥으로 수직 = 관절 합 PosIDs[1] + PosIDs[2] + WristID (deg, 3D 에서 잰 값과 0.5° 안) — Ry 90 의 관절 합
                public float SafeZ = 110f;                           // 옮길 때 높이
                public float TransferMmPerSec = 40f;                 // 직선 이동 속도 상한
                public int MinMoveMs = 1500;                         // 어떤 이동도 이보다 짧지 않게 — 보정 전환 1초 페이드가 이동 안에서 끝나게
                public float[] UnfoldVia = new float[] { 120, 0, 130 };
                public int UnfoldViaMs = 7000;                       // 휴지 → 경유점 (관절 보간, 0도 경유 금지)
                public float[] RestoreVia = new float[] { 120, 0, 110 };
                public int RestoreJointMs = 7000;                    // 경유점 → 휴지 자세 (관절 보간)
                public float RestTolDeg = 3f;                        // 원위치 확인 허용 (넘으면 4초 재시도 1회)
                public float MaxStartDeg = 175f;                     // 시작 때 읽은 각이 이보다 크면 이상값 — 작업을 시작하지 않는다
                public float IkTolMm = 2f, PitchTolDeg = 1.5f;       // 이동 뒤 3D 도착 확인
                public float DrySpeed = 3f;                          // 3D 만: 시간 배속 (한 다리 최소 300ms)
                /// <summary>설정의 계획 세트 (sagarm.txt planX — CSagArmSetup 이 자기 Plan.Sets 를 넣는다). 계획 확인의 하강 한도(zm) 확인은 행의 아래·하한
                /// (계획 CSV 의 below·zfloor 칸)을 이 세트들보다 깊게 보지 않는다 — 행의 세트 이름과 같은 세트, 없으면 세트들 중 가장 얕은 값
                /// (계획 파일의 두 칸을 한도와 함께 깊게 고쳐도 지나가지 않게). null = 행의 값 그대로 (설정 없이 부르는 경우)</summary>
                public List<CFloorPlanSet> PlanSets = null;
                /// <summary>움직이는 관절 = 위치 3개 + 손목</summary>
                public int[] MoveIDs { get { return new int[] { PosIDs[0], PosIDs[1], PosIDs[2], WristID }; } }
                /// <summary>작업이 쓰는 관절 전부 = 움직이는 관절 + 고정(그리퍼)</summary>
                public int[] AllIDs
                {
                    get
                    {
                        var l = new List<int>(MoveIDs);
                        if (HoldIDs != null) foreach (int id in HoldIDs) if (!l.Contains(id)) l.Add(id);
                        return l.ToArray();
                    }
                }
                /// <summary>관절 ID 가 쓸 만한가 (위치 3개 + 손목, 겹침 없음)</summary>
                public bool IdsOk
                {
                    get
                    {
                        if (PosIDs == null || PosIDs.Length != 3) return false;
                        int[] a = MoveIDs;
                        for (int i = 0; i < a.Length; i++) for (int j = i + 1; j < a.Length; j++) if (a[i] == a[j]) return false;
                        return true;
                    }
                }
                public CSagArmCfg Clone()
                {
                    var c = (CSagArmCfg)MemberwiseClone();
                    if (PosIDs != null) c.PosIDs = (int[])PosIDs.Clone();
                    if (HoldIDs != null) c.HoldIDs = (int[])HoldIDs.Clone();
                    if (UnfoldVia != null) c.UnfoldVia = (float[])UnfoldVia.Clone();
                    if (RestoreVia != null) c.RestoreVia = (float[])RestoreVia.Clone();
                    if (PlanSets != null) { c.PlanSets = new List<CFloorPlanSet>(); foreach (CFloorPlanSet st in PlanSets) c.PlanSets.Add(st != null ? st.Clone() : null); }
                    return c;
                }
            }

            /// <summary>측정 계획 한 행 = 바닥을 한 번 누를 점.
            /// Pitch 는 공구 피치 Ry (PlayXyz·SagComp_FloorPress 의 RyDeg, 90 = 수직 아래, Ry ≤ 90). 손목 해는 관절 합(12+13+14) = 180 − Ry 쪽
            /// (로봇 정의의 수직 관절 합이 downsum 이면 downsum + (90 − Ry) — OMX 는 downsum 90)
            /// (판정의 '피치' 와 첫닿음 데이터셋 floor_dataset_fc.csv 의 pitch 칸은 관절 합 — 피치 110 = Ry 70. 측정이 덧붙이는 행도
            /// SagRun_DatasetRows 가 관절 합으로 적는다. group 이름 p70y0 은 Ry).</summary>
            public class CFloorPlanRow
            {
                public string Set = "A";
                public float Pitch = 90, Yaw = 0, R, X, Y, ZStart, ZMin;   // ZStart·ZMin = 하강 시작·한도 (이상 명령 높이, mm)
                public float PredZ = float.NaN;      // 예상 첫닿음 FK 높이 (첫닿음 모델) — 3D 미리보기가 여기서 멈춘다
                public float Std = float.NaN;        // 그 예측 표준편차 (mm) — 모델 한계(StdLimit)를 넘으면 넘겨짚기
                public float QSum = float.NaN;       // 예측 자세의 관절 합 12+13+14 (deg) — 판정·데이터셋의 피치 (손목 해 확인용)
                public float TransferZ = float.NaN;  // 옮기는 높이 (IK 확인이 정함 — SafeZ, 팔이 닿지 않는 먼 점은 그 아래, NaN = 확인 전)
                public string Src = "";              // fc = 첫닿음 모델 예측 / 빈칸 = 불러온 계획(예측 칸 없음)
                public bool IkOk = true;             // IK 확인 통과 — 측정은 이 행만. false 로 들어온 행(계획 파일의 ik=0 등)은 계획 확인이 다시 넣지 않는다
                public string Why = "";              // 표시: "IK: z=…" (못 감), "넘겨짚기 (std …)", "첫닿음 예측 실패" — " · " 로 이어 붙인다
                public string FcModel = "";          // 계획을 만든 첫닿음 모델 파일 (전체 경로 — ② 계획 만들기가 적는다, 빈칸 = 모름: 손으로 만든·예전 계획).
                                                     //   계획 확인이 이 모델로 첫닿음을 다시 예측한다 (계획 CSV 의 fcmodel 칸)
                public float BelowMm = float.NaN;    // 계획을 만든 세트의 '아래' — 한도 = max(하한, 예측 − 아래). NaN = 모름 (예전 계획 — 세트 기본 12 로 본다). CSV below 칸
                public float ZMinFloor = float.NaN;  // 계획을 만든 세트의 '하한' (mm). NaN = 모름 (세트 기본 3 으로 본다). CSV zfloor 칸
                                                     //   계획 확인이 하강 한도(zm)를 이 둘과 다시 낸 바닥 예상으로 확인한다 (손으로 깊게 고친 한도는 뺀다)
                internal bool LimitByCaller;         // 하강 한도를 부른 쪽이 모드마다 확인했다 (닿기 시험 점 — 끔·켬 한도가 다르다) — 계획 확인은 한도를 다시 보지 않는다
                internal bool ExcludedByFile;        // 계획 파일의 ik 칸이 0 (손으로 뺐거나 계획 만들기가 뺀 행 — SagRun_PlanParse). 계획 확인은 이 행을 다시 넣지 않는다 (까닭 칸 표시용)
                /// <summary>줄 이름 (세트/피치/요) — 줄이 바뀌면 SafeZ 로 들어 옮긴다</summary>
                public string LineKey { get { return string.Format(CultureInfo.InvariantCulture, "{0}/p{1:F0}/y{2:F0}", Set, Pitch, Yaw); } }
                /// <summary>복사본 (작업이 계획을 다시 확인할 때 — 부른 쪽 목록을 바꾸지 않게)</summary>
                public CFloorPlanRow Clone() { return (CFloorPlanRow)MemberwiseClone(); }
            }

            /// <summary>측정 계획 세트 — 피치들 × 요들 × 반경(먼 쪽부터)</summary>
            public class CFloorPlanSet
            {
                public string Name = "A";
                public float[] Pitches = new float[] { 90 };                 // 공구 피치 Ry (≤ 90, 관절 합 = 180 − Ry)
                public float[] Yaws = new float[] { 0, 15, -15, 30, -30 };   // 줄 순서 그대로
                public float RFar = 260, RNear = 120, RStep = 10;          // 먼 쪽부터
                public float AboveMm = 20, BelowMm = 12, ZMinFloor = 3;    // 시작 = 예측 + Above, 한도 = max(ZMinFloor, 예측 − Below)
                public CFloorPlanSet Clone()
                {
                    var c = (CFloorPlanSet)MemberwiseClone();
                    if (Pitches != null) c.Pitches = (float[])Pitches.Clone();
                    if (Yaws != null) c.Yaws = (float[])Yaws.Clone();
                    return c;
                }
            }

            /// <summary>측정 계획 설정 — 세트 목록 (기본 A·B)</summary>
            public class CFloorPlanCfg
            {
                public List<CFloorPlanSet> Sets = DefaultSets();
                /// <summary>기본 세트: A = 피치 90, 요 {0, 15, −15, 30, −30}, 반경 260→120 / 10 (75행).
                /// B = 피치(Ry) 70 = 관절 합 110 (판정의 '피치 110' — 09-29 피치 70 묶음과 같은 손목 쪽), 요 {0, 15, −15}, 반경 240→140 / 20 (18행)
                /// — 첫닿음 판정이 피치 넘겨짚기를 약점으로 꼽아 넣은 세트. 그 손목 해가 없는 점(반경 240 등)은 IK 확인이 뺀다</summary>
                public static List<CFloorPlanSet> DefaultSets()
                {
                    return new List<CFloorPlanSet>
                    {
                        new CFloorPlanSet { Name = "A", Pitches = new float[] { 90 }, Yaws = new float[] { 0, 15, -15, 30, -30 }, RFar = 260, RNear = 120, RStep = 10 },
                        new CFloorPlanSet { Name = "B", Pitches = new float[] { 70 }, Yaws = new float[] { 0, 15, -15 }, RFar = 240, RNear = 140, RStep = 20 },
                    };
                }
            }

            /// <summary>바닥 측정 설정 (실물 측정 작업이 쓴다)</summary>
            public class CSagMeasureCfg
            {
                // 점마다 계획 창 그대로, 반경 하나·한 번씩 (Repeats 1), 반복 하강의 창 = 직전 꺾임 ± Margin (≥ 20)
                public CFloorProbeCfg Probe = new CFloorProbeCfg { Repeats = 1, Margin = 20, RetractAboveKneeMm = 25 };
                public bool WristHold = true;          // 측정 때만 손목 수직 유지 (10-02 사용자 결정) — 데이터셋 hold 칸
                public string CollectModelPath = null; // null = <urdf>\sagcomp_model.txt — 관절 모델만 쓴다 (바닥 부분 끔)
                public string DatasetPath = null;      // null = <urdf>\floor_dataset_fc.csv (첫닿음 데이터셋에 덧붙인다)
            }

            /// <summary>학습 설정 (SagRun_Learn)</summary>
            public class CSagLearnCfg
            {
                public string DatasetPath = null;      // null = <urdf>\floor_dataset_fc.csv (상대경로면 urdf 폴더 기준)
                public string BaseModelPath = null;    // null = <urdf>\sagcomp_model.txt 의 관절 모델 (바닥 부분 Gp·보정표·Fc 를 벗겨 쓴다)
                public bool UseHold = true;            // 손목 유지 켬(hold=1) 행도 학습 — 측정 기본이 손목 유지 켬 (첫닿음 수준에서 켬·끔 구별 안 됨)
                public float RefMm = float.NaN;        // NaN = 학습 평균, 숫자 = 기준 높이 고정 (fcref — 사각형 가운데 FC ≈ 48.8 이면 지금 펜 높이 75 가 거의 그대로)
                public float[][] PreviewPts = null;    // 보고서 예상 표 점 (x, y) — null = 사각형 (170,−35),(250,−35),(250,50),(170,50),(210,7.5)
                public float PreviewZ = 80;            // 그 점들의 높이 (틈 재기와 같은 값)
            }

            /// <summary>닿기 시험 설정 — 결과 파일을 보정 끔·켬으로 번갈아 눌러 본다</summary>
            public class CSagTestCfg
            {
                public float[] Radii = new float[] { 255, 235, 175, 145, 125 };   // 학습 반경(10mm 간격)의 가운데 — 배운 점을 다시 재지 않는다
                public float[] Yaws = new float[] { 0 };
                public float Pitch = 90;                   // 공구 피치 Ry (90 = 수직, ≤ 90 — 손목 해는 관절 합 = 180 − Ry 쪽. 판정의 '피치 100' = Ry 80)
                public int Repeats = 2;                    // 점마다 끔·켬·끔·켬
                public bool WristHold = false;             // 실사용 조건 = 끔
                // 창: 시작 = max(끔 예측, Ref) + Above (끔·켬 같음), 한도는 모드마다 = max(ZMinFloor, 그 모드의 예상 첫닿음 − Below) — 끔 = 끔 예측(PredFc), 켬 = Ref.
                //   두 모드 모두 예상 첫닿음보다 Below(12) 아래까지만 누른다 → 첫닿음이 꺾임 맞춤 창(FitWindowMm 14 — 누름 끝 위) 안에 든다 (측정 계획과 같은 규칙).
                //   한 한도 max(ZMinFloor, min(예측, Ref) − Below) 를 같이 쓰면 높은 쪽 모드는 첫닿음 아래 18~21mm 까지 눌러 첫닿음이 창 밖으로 나간다
                //   (옛 궤적: 가까운 반경에서 19~21mm 누른 하강은 꺾임이 단단한 정지·못 씀, 17mm 안은 첫닿음) — 켬 닿는 높이(주 지표)가 틀어진다
                public float AboveMm = 20, BelowMm = 12, ZMinFloor = 3;
                public float PassSpreadMm = 6;             // 합격: 켬 닿는 명령 높이 폭 ≤ 이 값 그리고 끔 폭보다 작음
                public float PassPredMm = 2, PassPredNearMm = 4, NearRMm = 150;   // 합격: 끔 예측 오차 ≤ 2mm (반경 150 이하는 4mm)
                public CFloorProbeCfg Probe = new CFloorProbeCfg { Repeats = 1, Margin = 20, RetractAboveKneeMm = 25 };
            }

            /// <summary>틈 재기 설정 — 펜을 바닥 위 Z 로 띄워 점마다 보정 끔·켬 틈을 자로 잰다 (닿지 않음 — 바닥 데이터와 독립)</summary>
            public class CSagGapCfg
            {
                public float[][] Pts = DefaultPts();
                public float Z = 80, Pitch = 90;               // 펜 높이 (이상 명령, mm), 공구 피치 Ry
                public float PassSameMm = 2, PassPredMm = 2;   // 합격: 켬 틈이 켬 평균 ±2 안, (켬 − 끔) 변화가 예측 dz ±2 안
                public int SettleMs = 1500;                    // 모델 전환(1초 페이드) 뒤 기다림
                /// <summary>사각형 네 꼭짓점 + 가운데</summary>
                public static float[][] DefaultPts()
                {
                    return new float[][] { new float[] { 170, -35 }, new float[] { 250, -35 }, new float[] { 250, 50 }, new float[] { 170, 50 }, new float[] { 210, 7.5f } };
                }
            }

            /// <summary>틈 재기에서 사용자에게 묻기 — 작업 스레드에서 부른다. 반환 = 숫자 문자열(mm), "" = 건너뜀, null = 중단</summary>
            public delegate string DSagAsk(string strPrompt);

            /// <summary>로봇팔 세팅 탭 설정 묶음 — <urdf>\sagarm.txt 읽기/쓰기 (key=value  // 설명, UTF-8 BOM 없음, CRLF).
            /// 모르는 키·옛 키(port, baud, neighbor, learnhold, learnaug, learnband, learndomain, testabove, testbelow …)는 무시한다.</summary>
            public class CSagArmSetup
            {
                public const string FILE = "sagarm.txt";
                public CSagArmCfg Arm = new CSagArmCfg();
                public CFloorPlanCfg Plan = new CFloorPlanCfg();
                public CSagMeasureCfg Measure = new CSagMeasureCfg();
                public CSagLearnCfg Learn = new CSagLearnCfg();
                public CSagTestCfg Test = new CSagTestCfg();
                public CSagGapCfg Gap = new CSagGapCfg();
                public string PlanFile = "";                         // 마지막 계획 (urdf 폴더 기준 상대경로)
                public string ResultFile = "";                       // 마지막으로 고른 결과 파일 (상대경로, 비면 활성 파일)
                public string LoadWarn = "";                         // 읽기 중 못 읽은 줄 (키 이름) — 값은 기본값 그대로

                /// <summary>옮기는 높이 safez 의 하한 (mm) — 이보다 낮은 값(오타 등)은 읽지 않고 기본값 + LoadWarn (첫닿음 창 위 ≈ 60~85, 틈 재기 펜 높이 80)</summary>
                public const float SAFEZ_MIN = 60f;

                private string m_strReadFailed = null;               // 있는데 못 읽은 파일 — 이 객체(기본값)로는 그 파일을 덮어쓰지 않는다

                /// <summary>기본값 — 이 설정의 계획 세트를 이동 설정에도 건다 (Arm.PlanSets = Plan.Sets: 계획 확인이 하강 한도를 설정의 세트로 본다)</summary>
                public CSagArmSetup() { Arm.PlanSets = Plan.Sets; }

                /// <summary>읽기 — 파일이 없으면 기본값으로 만들어 둔다. 줄마다 따로 읽고 틀린 줄은 건너뛴다 (LoadWarn). 파일이 잠겨 있으면 잠깐 다시 해 보고,
                /// 끝내 못 읽으면 기본값 + LoadWarn "★오류: …" — 그 객체의 Save 는 있는 파일을 덮어쓰지 않는다 (false, 사용자의 설정이 기본값으로 바뀌지 않게
                /// — 다시 Load 해서 쓴다). 예외 없음.</summary>
                public static CSagArmSetup Load(string strUrdfDir)
                {
                    var s = new CSagArmSetup();
                    try
                    {
                        if (string.IsNullOrEmpty(strUrdfDir)) { s.LoadWarn = "★오류: urdf 폴더가 비었음 — 기본값 (파일 안 만듦)"; return s; }
                        string strPath = Path.Combine(strUrdfDir, FILE);
                        if (!File.Exists(strPath)) { s.Save(strUrdfDir); return s; }
                        s.m_strReadFailed = strPath;                                  // 끝까지 읽으면 푼다
                        string[] lines = null;
                        for (int i = 0; lines == null; i++)
                        {
                            try { lines = File.ReadAllLines(strPath, Encoding.UTF8); }
                            catch (IOException) { if (i >= 4) throw; Thread.Sleep(100); }   // 편집기·동기화가 잠깐 잡고 있으면 다시 (모두 5번)
                        }
                        s.m_strReadFailed = null;
                        var sets = new List<CFloorPlanSet>();
                        var bad = new List<string>(); var warn = new List<string>();
                        foreach (string raw in lines)
                        {
                            string line = StripNote(raw);                                 // '//' 뒤 설명 (robot 줄은 '// 정의 이름' 만 — 이름 안의 '//' 는 이름 확인이 거절)
                            line = line.Trim(); if (line.Length == 0) continue;
                            int eq = line.IndexOf('='); if (eq <= 0) continue;
                            string kRaw = line.Substring(0, eq).Trim(), v = line.Substring(eq + 1).Trim();
                            try { s.ParseKey(kRaw, v, sets, warn); }
                            catch
                            {
                                bad.Add(kRaw);
                                // 값이 하나인 키에 쉼표 — 소수점을 쉼표로 쓴 것 (예전에는 천 단위 구분으로 읽혀 10배·100배가 됐다)
                                if (v.IndexOf(',') >= 0 && IsSingleValueKey(kRaw)) warn.Add(kRaw + "=" + v + " — 소수점은 '.' 로 (쉼표는 안 됨), 기본값 유지");
                            }
                        }
                        if (sets.Count > 0) s.Plan.Sets = sets;
                        s.Arm.PlanSets = s.Plan.Sets;                                  // 계획 확인의 하강 한도 = 이 파일의 세트 (CSagArmCfg.PlanSets)
                        var w = new List<string>();
                        if (bad.Count > 0) w.Add("읽지 못한 줄 (기본값 유지): " + string.Join(", ", bad.ToArray()));
                        w.AddRange(warn);
                        s.LoadWarn = string.Join(" · ", w.ToArray());
                    }
                    catch (Exception ex) { s.LoadWarn = "★오류: " + SagRun_Msg(ex); }
                    return s;
                }

                /// <summary>창 시작이 예측 첫닿음보다 적어도 이만큼 위여야 한다 (mm) — planX·testwin 의 '위' 하한, 계획·시험 점 확인</summary>
                public const float ABOVE_MIN = 5f;
                /// <summary>틈 재기 펜 높이 하한 (mm) — gapz. 점마다는 예측 첫닿음 + 10 아래면 뺀다 (SagRun_GapPlan)</summary>
                public const float GAPZ_MIN = 60f;

                // 키 하나 — 모르는 키는 그냥 넘어간다 (옛 키 포함). 값이 틀리면 예외 (부른 쪽이 그 줄을 건너뛴다), 값이 범위 밖이면 warn 에 적고 기본값 그대로.
                //   값이 하나인 키는 천 단위 구분 없이 읽는다 (NumberStyles.Float) — '6,5' 는 읽지 못한 줄 (예전에는 65 로 읽혀 합격 기준이 10배 느슨해졌다)
                private void ParseKey(string kRaw, string v, List<CFloorPlanSet> sets, List<string> warn)
                {
                    var ci = CultureInfo.InvariantCulture;
                    string k = kRaw.ToLowerInvariant();
                    if (k.StartsWith("plan") && k.Length > 4)
                    {
                        // planA=피치들|요들|먼,가까운,간격|위,아래,하한
                        string[] p = v.Split('|');
                        var st = new CFloorPlanSet { Name = kRaw.Substring(4).Trim().ToUpperInvariant() };
                        if (p.Length > 0 && p[0].Trim().Length > 0) st.Pitches = Floats(p[0]);
                        if (p.Length > 1 && p[1].Trim().Length > 0) st.Yaws = Floats(p[1]);
                        if (p.Length > 2 && p[2].Trim().Length > 0) { float[] a = Floats(p[2]); st.RFar = a[0]; if (a.Length > 1) st.RNear = a[1]; if (a.Length > 2) st.RStep = a[2]; }
                        if (p.Length > 3 && p[3].Trim().Length > 0)
                        {
                            float[] a = Floats(p[3]);
                            if (InRange(kRaw + " 위", a[0].ToString("0.###", ci), a[0], ABOVE_MIN, 100, warn)) st.AboveMm = a[0];   // 창 시작이 예측 첫닿음 위 5mm 는 돼야 (아래면 바닥으로 그냥 내려간다)
                            if (a.Length > 1 && InRange(kRaw + " 아래", a[1].ToString("0.###", ci), a[1], 0, 60, warn)) st.BelowMm = a[1];
                            if (a.Length > 2 && InRange(kRaw + " 하한", a[2].ToString("0.###", ci), a[2], 0, 200, warn)) st.ZMinFloor = a[2];
                        }
                        if (st.Pitches.Length == 0 || st.Yaws.Length == 0) throw new FormatException("빈 세트");
                        sets.Add(st);
                        return;
                    }
                    switch (k)
                    {
                        case "robot":
                            {
                                string w = DefNameWhy(v);
                                if (w.Length > 0 && v.IndexOf("//", StringComparison.Ordinal) >= 0) w += " (이 줄 뒤 설명은 '// " + DEF_NAME_NOTE + "' 만 쓸 수 있음)";
                                if (w.Length > 0) { warn.Add("robot=" + v + " — " + w + " — 기본값 유지"); break; }
                                Arm.DefName = v.Trim(); break;
                            }
                        case "tcplen":
                            {
                                if (v.Length == 0) { Arm.TcpLenMm = float.NaN; break; }      // 빈칸 = 손목 → 공구 끝 거리를 확인하지 않음
                                float f = F1(v); if (InRange(kRaw, v, f, 10, 500, warn)) Arm.TcpLenMm = f; break;
                            }
                        case "downsum": { float f = F1(v); if (InRange(kRaw, v, f, 0, 180, warn)) Arm.DownSumDeg = f; break; }
                        case "func": { int n = I1(v); if (InRange(kRaw, v, n, 0, 99, warn)) Arm.FuncNumber = n; break; }
                        case "posids": { int[] a = Ints(v); if (a.Length != 3) throw new FormatException("posids"); if (IdsInRange(kRaw, v, a, warn)) Arm.PosIDs = a; break; }
                        case "wrist": { int n = I1(v); if (InRange(kRaw, v, n, 0, 253, warn)) Arm.WristID = n; break; }
                        case "holdids": { int[] a = v.Length == 0 ? new int[0] : Ints(v); if (IdsInRange(kRaw, v, a, warn)) Arm.HoldIDs = a; break; }
                        case "safez":
                            {
                                float f = F1(v);
                                if (float.IsNaN(f) || float.IsInfinity(f) || f < SAFEZ_MIN)
                                { warn.Add(string.Format(ci, "safez={0} 은 {1:0} 보다 낮거나 수가 아님 — 기본값 {2:0.#} (옮기다 바닥·창을 지나지 않게)", v, SAFEZ_MIN, Arm.SafeZ)); break; }
                                if (f > 300) { warn.Add(string.Format(ci, "safez={0} 은 300 보다 높음 — 기본값 {1:0.#}", v, Arm.SafeZ)); break; }
                                Arm.SafeZ = f; break;
                            }
                        case "transfer": { float f = F1(v); if (InRange(kRaw, v, f, 1, 200, warn)) Arm.TransferMmPerSec = f; break; }
                        case "minmove": { int n = I1(v); if (InRange(kRaw, v, n, 300, 20000, warn)) Arm.MinMoveMs = n; break; }
                        case "unfoldvia":
                            {
                                float[] a = Floats(v); if (a.Length < 3) throw new FormatException("unfoldvia");
                                if (!InRange(kRaw + " z", a[2].ToString("0.###", ci), a[2], SAFEZ_MIN, 300, warn)) break;   // 경유점까지는 관절 보간(멈춤 없음) — 바닥 가까이로 가지 않게
                                if (a.Length > 3 && !InRange(kRaw + " ms", a[3].ToString("0.###", ci), a[3], 1000, 60000, warn)) break;
                                Arm.UnfoldVia = new float[] { a[0], a[1], a[2] }; if (a.Length > 3) Arm.UnfoldViaMs = (int)a[3]; break;
                            }
                        case "restorevia":
                            {
                                float[] a = Floats(v); if (a.Length < 3) throw new FormatException("restorevia");
                                if (!InRange(kRaw + " z", a[2].ToString("0.###", ci), a[2], SAFEZ_MIN, 300, warn)) break;
                                Arm.RestoreVia = new float[] { a[0], a[1], a[2] }; break;
                            }
                        case "restorems": { int n = I1(v); if (InRange(kRaw, v, n, 1000, 60000, warn)) Arm.RestoreJointMs = n; break; }
                        case "resttol": { float f = F1(v); if (InRange(kRaw, v, f, 0.5, 20, warn)) Arm.RestTolDeg = f; break; }
                        case "maxstart": { float f = F1(v); if (InRange(kRaw, v, f, 10, 180, warn)) Arm.MaxStartDeg = f; break; }
                        case "iktol":
                            {
                                float[] a = Floats(v); if (a.Length == 0) throw new FormatException("iktol");
                                if (!InRange(kRaw + " mm", a[0].ToString("0.###", ci), a[0], 0.1, 20, warn)) break;
                                if (a.Length > 1 && !InRange(kRaw + " deg", a[1].ToString("0.###", ci), a[1], 0.1, 10, warn)) break;
                                Arm.IkTolMm = a[0]; if (a.Length > 1) Arm.PitchTolDeg = a[1]; break;
                            }
                        case "dryspeed": { float f = F1(v); if (InRange(kRaw, v, f, 0.1, 50, warn)) Arm.DrySpeed = f; break; }
                        case "plan": PlanFile = v; break;
                        case "hold": Measure.WristHold = v != "0"; break;
                        case "retract": { float f = F1(v); if (InRange(kRaw, v, f, 5, 100, warn)) Measure.Probe.RetractAboveKneeMm = f; break; }
                        case "descent": { float f = F1(v); if (InRange(kRaw, v, f, 0.5, 20, warn)) Measure.Probe.PressDescentMmPerSec = f; break; }
                        case "learndataset": Learn.DatasetPath = v.Length == 0 ? null : v; break;
                        case "learnbase": Learn.BaseModelPath = v.Length == 0 ? null : v; break;
                        case "learnusehold": Learn.UseHold = v != "0"; break;
                        case "learnref":
                            {
                                if (v.Length == 0) { Learn.RefMm = float.NaN; break; }
                                float f = F1(v); if (InRange(kRaw, v, f, 3, 300, warn)) Learn.RefMm = f; break;
                            }
                        case "previewz": { float f = F1(v); if (InRange(kRaw, v, f, 20, 300, warn)) Learn.PreviewZ = f; break; }
                        case "result": ResultFile = v; break;
                        case "testr":
                            {
                                float[] a = Floats(v); if (a.Length == 0) throw new FormatException("testr");
                                bool ok = true; foreach (float f in a) if (!(f >= 50 && f <= 300)) ok = false;
                                if (!ok) { warn.Add(kRaw + "=" + v + " — 반경은 50~300 — 기본값 유지"); break; }
                                Test.Radii = a; break;
                            }
                        case "testyaw":
                            {
                                float[] a = Floats(v); if (a.Length == 0) throw new FormatException("testyaw");
                                bool ok = true; foreach (float f in a) if (!(f >= -180 && f <= 180)) ok = false;
                                if (!ok) { warn.Add(kRaw + "=" + v + " — 요는 -180~180 — 기본값 유지"); break; }
                                Test.Yaws = a; break;
                            }
                        case "testpitch": Test.Pitch = F1(v); break;                   // 범위 밖(> 90)은 시험 점 계획이 까닭과 함께 뺀다
                        case "testrep": { int n = I1(v); if (InRange(kRaw, v, n, 1, 10, warn)) Test.Repeats = n; break; }
                        case "testhold": Test.WristHold = v != "0"; break;
                        case "testwin":
                            {
                                float[] a = Floats(v); if (a.Length == 0) throw new FormatException("testwin");
                                if (!InRange(kRaw + " 위", a[0].ToString("0.###", ci), a[0], ABOVE_MIN, 100, warn)) break;
                                if (a.Length > 1 && !InRange(kRaw + " 아래", a[1].ToString("0.###", ci), a[1], 0, 60, warn)) break;
                                if (a.Length > 2 && !InRange(kRaw + " 하한", a[2].ToString("0.###", ci), a[2], 0, 200, warn)) break;
                                Test.AboveMm = a[0]; if (a.Length > 1) Test.BelowMm = a[1]; if (a.Length > 2) Test.ZMinFloor = a[2]; break;
                            }
                        case "testpass": { float f = F1(v); if (InRange(kRaw, v, f, 0.5, 50, warn)) Test.PassSpreadMm = f; break; }
                        case "testpred":
                            {
                                float[] a = Floats(v); if (a.Length == 0) throw new FormatException("testpred");
                                if (!InRange(kRaw, a[0].ToString("0.###", ci), a[0], 0.2, 20, warn)) break;
                                if (a.Length > 1 && !InRange(kRaw + " 가까운 쪽", a[1].ToString("0.###", ci), a[1], 0.2, 20, warn)) break;
                                if (a.Length > 2 && !InRange(kRaw + " 반경", a[2].ToString("0.###", ci), a[2], 0, 300, warn)) break;
                                Test.PassPredMm = a[0]; if (a.Length > 1) Test.PassPredNearMm = a[1]; if (a.Length > 2) Test.NearRMm = a[2]; break;
                            }
                        case "gappts":
                            {
                                var l = new List<float[]>();
                                foreach (string t in v.Split(';')) { if (t.Trim().Length == 0) continue; float[] a = Floats(t); if (a.Length < 2) throw new FormatException("gappts"); l.Add(new float[] { a[0], a[1] }); }
                                if (l.Count == 0) throw new FormatException("gappts");
                                Gap.Pts = l.ToArray(); break;
                            }
                        case "gapz": { float f = F1(v); if (InRange(kRaw, v, f, GAPZ_MIN, 200, warn)) Gap.Z = f; break; }   // 틈 재기는 펜을 그 높이로 그냥 내린다 (누름 정지 없음)
                        case "gappitch": { float f = F1(v); if (InRange(kRaw, v, f, 0, 90, warn)) Gap.Pitch = f; break; }
                        case "gappasssame": { float f = F1(v); if (InRange(kRaw, v, f, 0.1, 20, warn)) Gap.PassSameMm = f; break; }
                        case "gappasspred": { float f = F1(v); if (InRange(kRaw, v, f, 0.1, 20, warn)) Gap.PassPredMm = f; break; }
                        case "gapsettle": { int n = I1(v); if (InRange(kRaw, v, n, 0, 10000, warn)) Gap.SettleMs = n; break; }
                        default: break;                              // 모르는 키·옛 키 (port, baud, neighbor, learnaug …) — 무시
                    }
                }
                // 값 하나 읽기 — 천 단위 구분 없이 (쉼표가 있으면 예외 → 읽지 못한 줄)
                private static float F1(string v) { return float.Parse(v, NumberStyles.Float, CultureInfo.InvariantCulture); }
                private static int I1(string v) { return int.Parse(v, NumberStyles.Integer, CultureInfo.InvariantCulture); }
                // 범위 확인 — 밖이면(수가 아니어도) warn 에 적고 false (기본값 그대로)
                private static bool InRange(string strKey, string strValue, double x, double lo, double hi, List<string> warn)
                {
                    if (!double.IsNaN(x) && !double.IsInfinity(x) && x >= lo && x <= hi) return true;
                    var ci = CultureInfo.InvariantCulture;
                    warn.Add(string.Format(ci, "{0}={1} 은 {2}~{3} 밖 — 기본값 유지", strKey, strValue, lo.ToString("0.###", ci), hi.ToString("0.###", ci)));
                    return false;
                }
                private static bool IdsInRange(string strKey, string strValue, int[] a, List<string> warn)
                {
                    foreach (int n in a) if (n < 0 || n > 253) { warn.Add(strKey + "=" + strValue + " — ID 는 0~253 — 기본값 유지"); return false; }
                    return true;
                }
                // 값이 하나인 키 (쉼표가 있으면 소수점을 쉼표로 쓴 것)
                private static bool IsSingleValueKey(string kRaw)
                {
                    string k = (kRaw ?? "").Trim().ToLowerInvariant();
                    switch (k)
                    {
                        case "func": case "wrist": case "tcplen": case "downsum": case "robot": case "safez": case "transfer": case "minmove": case "restorems": case "resttol": case "maxstart":
                        case "dryspeed": case "retract": case "descent": case "learnref": case "previewz": case "testpitch": case "testrep":
                        case "testpass": case "gapz": case "gappitch": case "gappasssame": case "gappasspred": case "gapsettle":
                            return true;
                        default: return false;
                    }
                }
                private static float[] Floats(string v)
                {
                    var ci = CultureInfo.InvariantCulture; var l = new List<float>();
                    foreach (string t in v.Split(',')) if (t.Trim().Length > 0) l.Add(float.Parse(t.Trim(), NumberStyles.Float, ci));
                    return l.ToArray();
                }
                private static int[] Ints(string v)
                {
                    var ci = CultureInfo.InvariantCulture; var l = new List<int>();
                    foreach (string t in v.Split(',')) if (t.Trim().Length > 0) l.Add(int.Parse(t.Trim(), NumberStyles.Integer, ci));
                    return l.ToArray();
                }

                /// <summary>통째로 다시 쓴다 (UTF-8 BOM 없음, CRLF). 반환 = 성공 (urdf 폴더가 비었으면, 있는 파일을 못 읽은 객체로 그 파일을 덮어쓰려 하면 false). 예외 없음.</summary>
                public bool Save(string strUrdfDir)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(strUrdfDir)) return false;
                        if (m_strReadFailed != null && SagRun_SamePath(m_strReadFailed, Path.Combine(strUrdfDir, FILE)) && File.Exists(m_strReadFailed)) return false;
                        var ci = CultureInfo.InvariantCulture;
                        Func<float, string> N = f => f.ToString("0.###", ci);
                        Func<float[], string> NA = a => a == null ? "" : string.Join(",", Array.ConvertAll(a, f => f.ToString("0.###", ci)));
                        Func<int[], string> IA = a => a == null ? "" : string.Join(",", Array.ConvertAll(a, n => n.ToString(ci)));
                        var sb = new StringBuilder();
                        Action<string, string, string> L = (k, v, d) =>
                        {
                            string s = k + "=" + v;
                            if (d.Length > 0) s = s.PadRight(24) + " // " + d;
                            sb.Append(s).Append("\r\n");
                        };
                        sb.Append("// 로봇팔 세팅 탭 설정. 단위 mm / deg / ms. // 뒤는 설명. 포트·속도는 연결 줄이 가진다 (여기 없음)\r\n");
                        DefLines(sb, Arm);
                        sb.Append("// ── 이동·계획·시험 ──\r\n");
                        L("safez", N(Arm.SafeZ), "옮길 때 높이");
                        L("transfer", N(Arm.TransferMmPerSec), "직선 이동 속도 상한 (mm/s)");
                        L("minmove", Arm.MinMoveMs.ToString(ci), "이동 하나의 최소 시간 (ms)");
                        L("unfoldvia", NA(Arm.UnfoldVia) + "," + Arm.UnfoldViaMs.ToString(ci), "펼치기 경유점 x,y,z,ms");
                        L("restorevia", NA(Arm.RestoreVia), "원위치 경유점");
                        L("restorems", Arm.RestoreJointMs.ToString(ci), "경유점 → 휴지 자세 관절 보간 (ms)");
                        L("resttol", N(Arm.RestTolDeg), "원위치 허용 (deg)");
                        L("maxstart", N(Arm.MaxStartDeg), "시작 때 읽은 각 한계 (deg)");
                        L("iktol", N(Arm.IkTolMm) + "," + N(Arm.PitchTolDeg), "이동 뒤 3D 도착 확인 (mm, deg)");
                        L("dryspeed", N(Arm.DrySpeed), "3D 만 배속");
                        L("plan", PlanFile ?? "", "마지막 계획 파일 (urdf 폴더 기준)");
                        if (Plan != null && Plan.Sets != null)
                            foreach (CFloorPlanSet st in Plan.Sets)
                                L("plan" + st.Name, NA(st.Pitches) + "|" + NA(st.Yaws) + "|" + N(st.RFar) + "," + N(st.RNear) + "," + N(st.RStep) + "|" + N(st.AboveMm) + "," + N(st.BelowMm) + "," + N(st.ZMinFloor),
                                  "피치들|요들|먼 반경,가까운 반경,간격|위,아래,하한 (시작 = 예측 + 위, 한도 = max(하한, 예측 − 아래))");
                        L("hold", Measure.WristHold ? "1" : "0", "측정 때 손목 수직 유지");
                        L("retract", N(Measure.Probe.RetractAboveKneeMm), "꺾임점 위 복귀 (mm)");
                        L("descent", N(Measure.Probe.PressDescentMmPerSec), "하강 속도 (mm/s)");
                        L("learndataset", Learn.DatasetPath ?? "", "비우면 floor_dataset_fc.csv");
                        L("learnbase", Learn.BaseModelPath ?? "", "비우면 적용 중 파일의 관절 모델");
                        L("learnusehold", Learn.UseHold ? "1" : "0", "손목 유지 켬(hold=1) 행도 학습");
                        L("learnref", float.IsNaN(Learn.RefMm) ? "" : N(Learn.RefMm), "비우면 학습 평균, 숫자면 기준 높이 고정 (fcref)");
                        L("previewz", N(Learn.PreviewZ), "보고서 예상 표 높이");
                        L("result", ResultFile ?? "", "마지막으로 고른 결과 파일");
                        L("testr", NA(Test.Radii), "닿기 시험 반경");
                        L("testyaw", NA(Test.Yaws), "닿기 시험 요");
                        L("testpitch", N(Test.Pitch), "닿기 시험 피치");
                        L("testrep", Test.Repeats.ToString(ci), "점마다 끔·켬 반복");
                        L("testhold", Test.WristHold ? "1" : "0", "시험 때 손목 수직 유지 (0 = 실사용 조건)");
                        L("testwin", N(Test.AboveMm) + "," + N(Test.BelowMm) + "," + N(Test.ZMinFloor), "창: 위, 아래, 하한 (시작 = max(예측, Ref) + 위, 한도 끔 = max(하한, 예측 − 아래), 켬 = max(하한, Ref − 아래))");
                        L("testpass", N(Test.PassSpreadMm), "합격: 켬 폭 ≤ 이 값(mm) 그리고 끔 폭보다 작음");
                        L("testpred", N(Test.PassPredMm) + "," + N(Test.PassPredNearMm) + "," + N(Test.NearRMm), "합격: 끔 예측 오차 ≤ 첫 값 (반경 셋째 값 이하는 둘째 값)");
                        var gp = new List<string>(); if (Gap.Pts != null) foreach (float[] p in Gap.Pts) if (p != null && p.Length >= 2) gp.Add(N(p[0]) + "," + N(p[1]));
                        L("gappts", string.Join(";", gp.ToArray()), "틈 재기 점 x,y (; 로 나눔)");
                        L("gapz", N(Gap.Z), "틈 재기 높이");
                        L("gappitch", N(Gap.Pitch), "틈 재기 피치");
                        L("gappasssame", N(Gap.PassSameMm), "합격: 켬 틈이 평균 ± 이 값 안");
                        L("gappasspred", N(Gap.PassPredMm), "합격: (켬 − 끔) 변화가 예측 dz ± 이 값 안");
                        L("gapsettle", Gap.SettleMs.ToString(ci), "모델 전환 뒤 기다림 (ms)");
                        if (!string.IsNullOrEmpty(strUrdfDir)) Directory.CreateDirectory(strUrdfDir);
                        File.WriteAllText(Path.Combine(strUrdfDir ?? "", FILE), sb.ToString(), new UTF8Encoding(false));
                        return true;
                    }
                    catch { return false; }
                }

                // ── 로봇 정의 7키 (sagarm.txt 맨 앞 묶음·본보기 파일) ──
                /// <summary>본보기 폴더 이름 (<urdf>\sagarm_defs\<이름>.txt) — 정의 7키만 담은 본보기. 작업은 읽지 않는다 (고르면 화면 칸만 채운다)</summary>
                public const string DEF_DIR = "sagarm_defs";
                /// <summary>내장 본보기 이름 (OMX follower — 기본값). 본보기 파일 이름으로는 쓰지 않는다</summary>
                public const string DEF_OMX = "OMX follower";
                private static readonly string[] s_astrDefKeys = { "robot", "func", "posids", "wrist", "holdids", "tcplen", "downsum" };
                // 정의 7키 줄 (머리 주석 한 줄 + 7줄, CRLF)
                private static void DefLines(StringBuilder sb, CSagArmCfg a)
                {
                    var ci = CultureInfo.InvariantCulture;
                    if (a == null) a = new CSagArmCfg();
                    Func<int[], string> IA = x => x == null ? "" : string.Join(",", Array.ConvertAll(x, n => n.ToString(ci)));
                    Action<string, string, string> L = (k, v, d) => { string s = k + "=" + v; if (d.Length > 0) s = s.PadRight(24) + " // " + d; sb.Append(s).Append("\r\n"); };
                    sb.Append("// ── 로봇 정의 (로봇 정의 페이지) — 공구 끝(TCP) = 수식 func 체인 끝 ('!' 바로 앞 DH 줄), 손목 = 공구를 수직으로 꺾는 관절 ──\r\n");
                    L("robot", (a.DefName ?? "").Replace("\r", " ").Replace("\n", " "), DEF_NAME_NOTE);
                    L("func", a.FuncNumber.ToString(ci), "공구 끝(TCP)이 끝점인 IK 수식");
                    L("posids", IA(a.PosIDs), "베이스(요), 피치 관절 1, 2");
                    L("wrist", a.WristID.ToString(ci), "공구를 수직으로 꺾는 손목 (공구 끝 앞 마지막 관절)");
                    L("holdids", IA(a.HoldIDs), "토크만 켜서 고정 (그리퍼 — 공구 끝 체인 밖)");
                    L("tcplen", float.IsNaN(a.TcpLenMm) ? "" : a.TcpLenMm.ToString("0.###", ci), "손목 원점 → 공구 끝 기대 거리 mm (빈칸 = 확인 안 함)");
                    L("downsum", a.DownSumDeg.ToString("0.###", ci), "바닥으로 수직 = 관절 합 posids2+posids3+wrist (3D 와 ±0.5°)");
                }
                /// <summary>정의 이름이 쓸 만한가 — 앞뒤 공백을 자르고 1~40자, '//' '=' '|' '#' 줄바꿈과 파일 이름에 못 쓰는 글자가 없어야 한다
                /// (sagarm.txt 는 '//' 뒤를 설명으로 자르고 모델 파일은 '#' 줄을 건너뛴다 — 다시 읽으면 깨진다). 반환 = 까닭 (괜찮으면 "")</summary>
                public static string DefNameWhy(string strName)
                {
                    string t = (strName ?? "").Trim();
                    if (t.Length == 0) return "이름이 비었음";
                    if (t.Length > 40) return "이름은 40자까지";
                    if (t.IndexOf("//", StringComparison.Ordinal) >= 0 || t.IndexOfAny(new char[] { '=', '|', '#', '\r', '\n' }) >= 0) return "이름에 // = | # 줄바꿈은 쓸 수 없음";
                    if (t.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "이름에 파일 이름에 못 쓰는 글자 (\\ / : * ? \" < > |) 는 쓸 수 없음";
                    return "";
                }
                // robot 줄의 설명 — Save·본보기가 쓰는 글 (이 설명만 이름 뒤 설명으로 자른다)
                private const string DEF_NAME_NOTE = "정의 이름";
                // 설정 줄에서 '//' 뒤 설명을 자른다. robot 줄 (정의 이름) 은 Save 가 붙이는 '// 정의 이름' 만 설명으로 자르고, 그 밖의 '//' 는 앞뒤에 빈칸이 있어도
                //   이름 안의 글로 남긴다 — 이름 확인(DefNameWhy)이 LoadWarn + 기본값으로 거절한다 ('robot=My // robot' 이 말없이 'My' 가 되지 않게).
                //   다른 줄은 첫 '//' 에서 자른다 (예전과 같다)
                private static string StripNote(string raw)
                {
                    string line = raw ?? "";
                    int c = line.IndexOf("//", StringComparison.Ordinal);
                    if (c < 0) return line;
                    int eq = line.IndexOf('=');
                    bool bRobot = eq > 0 && eq < c && string.Equals(line.Substring(0, eq).Trim(), "robot", StringComparison.OrdinalIgnoreCase);
                    if (!bRobot) return line.Substring(0, c);
                    int last = line.LastIndexOf("//", StringComparison.Ordinal);
                    if (string.Equals(line.Substring(last + 2).Trim(), DEF_NAME_NOTE, StringComparison.Ordinal)) return line.Substring(0, last);
                    return line;
                }
                /// <summary>내장 정의 (OMX follower — 수식 0, 위치 11·12·13, 손목 14, 토크만 15·16, 공구 끝 103.2mm, 수직 관절 합 90). 정의 7키 밖 값은 기본값</summary>
                public static CSagArmCfg OmxDef() { return new CSagArmCfg(); }
                /// <summary>본보기 파일 목록 (<urdf>\sagarm_defs\*.txt 전체 경로, 이름 순). 없으면 빈 배열. 예외 없음</summary>
                public static string[] DefPresets(string strUrdfDir)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(strUrdfDir)) return new string[0];
                        string d = Path.Combine(strUrdfDir, DEF_DIR);
                        if (!Directory.Exists(d)) return new string[0];
                        string[] a = Directory.GetFiles(d, "*.txt");
                        Array.Sort(a, StringComparer.OrdinalIgnoreCase);
                        return a;
                    }
                    catch { return new string[0]; }
                }
                /// <summary>본보기 읽기 — 정의 7키만 (sagarm.txt 와 같은 규칙, 없는 키는 기본값). 다른 키·못 읽은 줄은 warn. 파일을 못 읽으면 null. 예외 없음</summary>
                public static CSagArmCfg LoadDefPreset(string strPath, out string warn)
                {
                    warn = "";
                    try
                    {
                        if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { warn = "본보기 파일 없음: " + strPath; return null; }
                        var s = new CSagArmSetup();
                        var w = new List<string>(); var bad = new List<string>(); var other = new List<string>();
                        foreach (string raw in File.ReadAllLines(strPath, Encoding.UTF8))
                        {
                            string line = StripNote(raw);                                 // sagarm.txt 와 같은 규칙 (robot 줄은 '// 정의 이름' 만 설명)
                            line = line.Trim(); if (line.Length == 0) continue;
                            int eq = line.IndexOf('='); if (eq <= 0) continue;
                            string kRaw = line.Substring(0, eq).Trim(), v = line.Substring(eq + 1).Trim();
                            if (Array.IndexOf(s_astrDefKeys, kRaw.ToLowerInvariant()) < 0) { other.Add(kRaw); continue; }
                            try { s.ParseKey(kRaw, v, new List<CFloorPlanSet>(), w); }
                            catch { bad.Add(kRaw); if (v.IndexOf(',') >= 0 && IsSingleValueKey(kRaw)) w.Add(kRaw + "=" + v + " — 소수점은 '.' 로 (쉼표는 안 됨), 기본값 유지"); }
                        }
                        var l = new List<string>();
                        if (bad.Count > 0) l.Add("읽지 못한 줄 (기본값 유지): " + string.Join(", ", bad.ToArray()));
                        l.AddRange(w);
                        if (other.Count > 0) l.Add("본보기에 정의 밖 키 (무시): " + string.Join(", ", other.ToArray()));
                        warn = string.Join(" · ", l.ToArray());
                        return s.Arm;
                    }
                    catch (Exception ex) { warn = "★오류: " + SagRun_Msg(ex); return null; }
                }
                /// <summary>본보기로 저장 — <urdf>\sagarm_defs\<c.DefName>.txt (정의 7키, UTF-8 BOM 없음, CRLF, 있으면 덮어씀). 이름이 내장 본보기(OMX follower)이거나
                /// 쓸 수 없는 이름이면 거절. sagarm.txt 는 바꾸지 않는다. 반환 = 성공 (strPath = 쓴 파일). 예외 없음</summary>
                public static bool SaveDefPreset(string strUrdfDir, CSagArmCfg c, out string strPath, out string why)
                {
                    strPath = null; why = "";
                    try
                    {
                        if (c == null) { why = "정의 없음"; return false; }
                        if (string.IsNullOrEmpty(strUrdfDir)) { why = "urdf 폴더가 비었음"; return false; }
                        string n = (c.DefName ?? "").Trim();
                        string w = DefNameWhy(n);
                        if (w.Length > 0) { why = w; return false; }
                        if (string.Equals(n, DEF_OMX, StringComparison.OrdinalIgnoreCase)) { why = "'" + DEF_OMX + "' 은 내장 본보기 이름 — 다른 이름으로"; return false; }
                        string d = Path.Combine(strUrdfDir, DEF_DIR);
                        Directory.CreateDirectory(d);
                        strPath = Path.Combine(d, n + ".txt");
                        var sb = new StringBuilder();
                        sb.Append("// 로봇 정의 본보기 — 정의 7키만 (로봇 정의 페이지에서 고르면 칸만 채움, [확인·저장] 을 눌러야 sagarm.txt 에 들어감)\r\n");
                        DefLines(sb, c);
                        File.WriteAllText(strPath, sb.ToString(), new UTF8Encoding(false));
                        return true;
                    }
                    catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
                }
            }

            // ────────────────────────────────────────────────────────────
            // 결과
            // ────────────────────────────────────────────────────────────

            /// <summary>학습 결과 (SagRun_Learn)</summary>
            public class CSagLearnResult
            {
                public bool Ok;
                public CSagComp Model;                                     // 저장한 모델 (첫닿음 모델 준비됨)
                public string ModelPath, ReportPath, DataPath, Report = "", Line = "", Why = "";
                public int NRows, NUsed, NHold;                            // 데이터셋 행, 학습에 쓴 첫닿음 점, 그중 손목 유지 켬
                public int Groups, Sessions;                               // 데이터셋의 묶음(session_group)·측정 회차 수
                public bool Stopped;                                       // 중단 요청으로 멈춤 (파일 없음)
                public double Ref = double.NaN, CvRmse = double.NaN, CvPhysRmse = double.NaN;
                public double[] Beta = null;                               // 물리 평균 c0, k12, k13, b14
                public double SquareSpread = double.NaN;                   // 예상 표 먼쪽 − 가까운쪽 dz (mm)
                public double MaxStep = double.NaN;                        // 반경선(z 60·75·90) 이웃 dz 최대 계단 (mm)
                public float[] PreviewDz = null, PreviewStd = null;        // 예상 표 점마다 dz·표준편차 (IK 안 되면 NaN)
                public string MotorMapWarn = "";                           // 바탕 관절 모델이 지금과 다른 모터 매핑(방향·영점)에서 학습됨 (motormap 비교) — 비면 같음.
                                                                           //   그때 결과 파일의 motormap 은 바탕 파일의 것 (적용·시험 때 '학습 때와 방향·영점이 다름' 이 계속 나온다)
            }

            /// <summary>측정 회차 결과 (실물 측정 작업이 채운다)</summary>
            public class CSagRunResult
            {
                public bool Ok, Stopped, Dry;
                public int RowsPlanned, RowsDone, LiveDetected, SafetyStops, Detected, DatasetRowsAdded;
                public int NKindF, NKindH, NKindC, NKindX;                 // 궤적 재분석(첫닿음)의 kind 별 수
                public int PredErrN;                                       // 첫닿음 FK 와 계획 예측을 비교한 점 수
                public double PredErrMean = double.NaN, PredErrMax = double.NaN;   // 첫닿음 FK − 계획 예측 (미측정 구간 예측의 실측 확인)
                public List<CFloorContact> Contacts = new List<CFloorContact>();   // 궤적 재분석 결과 (학습 입력과 같은 것)
                public string RunDir, TracePath, LivePath, RunDatasetPath, DatasetPath, Session, Report = "", Line = "";
                public double ElapsedSec;
                public string Why = "";                                    // 시작하지 않음·실패의 까닭 (" · " 로 이음)
                public string ReportPath;                                  // 작업 폴더의 measure_report.txt
                public int[] UncheckedAxes = new int[0];                   // 작업이 쓰는 축 중 방향 확인 안 된 축 (U4 — 앱이 시작 전에 경고, DLL 은 막지 않는다)
                public string MotorMapWarn = "";                           // 측정 모델 파일의 매핑이 지금 연결과 다름 (C10) — 비면 같음
                public bool RestOk;                                        // 끝에 휴지 자세로 돌아왔다 (RestTolDeg 안, 3D 만이면 true)
                public int NoTouchAtLimit;                                 // 한도까지 내려가도 누름·꺾임이 없었던 누름 수 (실물 — 누른 점 모두면 보고서·한 줄에 ★)
                public List<CSagPredErrRow> PredErrRows = new List<CSagPredErrRow>();   // 하강마다 첫닿음 FK − 계획 예측 (궤적 재분석 순서, 보고서의 점별 표)
            }

            /// <summary>측정 하강 하나의 첫닿음 FK − 계획 예측 (측정 보고서의 점별 표 — 미측정 구간 예측의 실측 확인)</summary>
            public class CSagPredErrRow
            {
                public string LineKey = "";                // 계획 행의 줄 (세트/피치/요) — 맞는 행이 없으면 ""
                public float R, X, Y, Pitch;               // 하강 자리 (궤적 재분석), 공구 피치 Ry
                public string Kind = "";                   // 첫닿음 kind (F/H/C/X, 빈칸 = 판정 없음)
                public float FcZ = float.NaN;              // 첫닿음 FK 높이 (mm)
                public float PredZ = float.NaN;            // 그 행의 계획 예측 (mm)
                public float Err = float.NaN;              // FcZ − PredZ (kind F 만 — 아니면 NaN)
            }

            /// <summary>닿기 시험 점 하나</summary>
            public class CSagTestPoint
            {
                public float R, Yaw, Pitch = 90, X, Y;
                public float ZStart;                       // 하강 시작 (명령 높이) — 끔·켬 같음 = max(PredFc, Ref) + Above
                public float ZMin;                         // 두 모드 한도 중 낮은 쪽 = min(ZMinOff, ZMinOn) — IK 확인·표시용
                public float ZMinOff = float.NaN;          // 끔 하강 한도 = max(ZMinFloor, PredFc − Below) — 끔 누름의 SagComp_FloorPress ZMin
                public float ZMinOn = float.NaN;           // 켬 하강 한도 = max(ZMinFloor, Ref − Below) — 켬 누름의 SagComp_FloorPress ZMin
                public float PredFc = float.NaN;           // 끔에서 닿는 첫닿음 FK 높이 예측 (첫닿음 모델)
                public float PredStd = float.NaN;          // 그 예측 표준편차
                public float QSum = float.NaN;             // 예측 자세의 관절 합 12+13+14 (deg, 손목 해 확인용)
                public float TransferZ = float.NaN;        // 옮기는 높이 (IK 확인이 정함)
                public float Ref = float.NaN;              // 켬에서 닿는 명령 높이 예측 (= 결과 모델 Ref)
                public List<CFloorContact> Off = new List<CFloorContact>(), On = new List<CFloorContact>();
                public bool Skipped; public string Note = "";
                // 판정 (SagRun_TestEvaluate 가 채운다) — 판정에 쓰는 하강만 (실물 = 첫닿음 kind F·H, 3D 만 = 예상)
                public float MeanOff = float.NaN, MeanOn = float.NaN;         // 닿는 명령 높이 평균 (실물 = 첫닿음 명령 높이 FcCmdZ)
                public float MeanFkOff = float.NaN, MeanFkOn = float.NaN;     // 첫닿음 FK 평균 (실물 = FcZ — 실시간 꺾임점이 아니다)
                public float PredErrOff = float.NaN;                          // 끔 첫닿음 FK − PredFc
                public float RepeatOff = float.NaN, RepeatOn = float.NaN;     // 반복 폭 (max − min)
                /// <summary>그 모드의 하강 한도 (SagComp_FloorPress 에 넘길 ZMin) — 켬 = ZMinOn, 끔 = ZMinOff (비었으면 ZMin)</summary>
                public float ZMinFor(bool bOn)
                {
                    float z = bOn ? ZMinOn : ZMinOff;
                    return float.IsNaN(z) ? ZMin : z;
                }
            }

            /// <summary>닿기 시험 결과</summary>
            public class CSagTestResult
            {
                public bool Ok, Pass, Stopped, Dry;
                public List<CSagTestPoint> Points = new List<CSagTestPoint>();
                public double Ref = double.NaN;
                public double SpreadOff = double.NaN, SpreadOn = double.NaN, StdOff = double.NaN, StdOn = double.NaN, RmsOnFromRef = double.NaN;
                public double FkSpreadOff = double.NaN, FkSpreadOn = double.NaN, RepeatOff = double.NaN, RepeatOn = double.NaN;
                public double PredSpreadOff = double.NaN, PredSpreadOn = double.NaN;   // 예상 폭 (끔 = 첫닿음 예측의 폭, 켬 = 0 — 모두 Ref)
                public double[] PredErrOff = new double[0];                          // 점마다 끔 예측 오차 (Skipped 아닌 점 순서)
                public double PredErrOffMax = double.NaN;                            // |끔 예측 오차| 최대
                public bool PredErrPass;                                             // 끔 예측 오차 기준 (2mm, 반경 150 이하 4mm) 만족
                public int NOff, NOn, SafetyStops;
                public int NJudged;                                                  // 판정에 쓴 점 (Skipped 아님) — 1점이면 끔 예측 오차·켬 접촉으로만 판정
                public int NNoFirstTouch;                                            // 첫닿음을 못 찾아(C·X·판정 없음) 판정에서 뺀 하강 수 (실물)
                public int NUnpressed;                                               // 계획했는데 돌지 않은 누름 수 (쓸 점 × 끔·켬 × 반복 중 — 펼치기·옮기기 실패, 시작점으로 못 감 등).
                                                                                     //   0 보다 크면 합격·불합격을 내지 않는다 (판정 불가 — 돌지 않은 누름은 '켬 접촉 없음' 으로 세지 않는다)
                public string Verdict = "";                                          // 합격 / 불합격 / 3D만 / 중단 / 판정 불가
                public string Why = "";                                              // 불합격·판정 불가의 까닭, 1점 판정 표시 (" · " 로 이음) — 시작하지 않았으면 그 까닭
                public string RunDir, CsvPath, ReportPath, SidecarPath, Report = "", Line = "";
                public string TracePath;                                             // 작업 폴더의 test_trace.csv (실물)
                public int[] UncheckedAxes = new int[0];                             // 방향 확인 안 된 축 (U4 — 경고만)
                public string MotorMapWarn = "";                                     // 결과 파일의 매핑이 지금 연결과 다름 (C10)
                public bool RestOk;                                                  // 끝에 휴지 자세로 돌아왔다
            }

            /// <summary>틈 재기 점 하나</summary>
            public class CSagGapPoint
            {
                public float X, Y, Z = 80, Pitch = 90;
                public float[] Q = null;                   // 높이 Z 의 IK 자세 (위치 3개 + 손목, deg) — null = IK 불가
                public bool IkOk = true;                   // 옮기는 높이·Z 둘 다 IK 됨
                public float TransferZ = float.NaN;        // 옮기는 높이 (SafeZ, 먼 점은 그 아래)
                public float DzPred = float.NaN, Std = float.NaN;   // 켬 − 끔 틈 변화 예상 (첫닿음 모델이 끝을 올리는 양, 위로 +) 와 표준편차
                public float GapOff = float.NaN, GapOn = float.NaN; // 잰 틈 (mm, 빈칸 NaN)
                public string Note = "";
            }

            /// <summary>틈 재기 결과</summary>
            public class CSagGapResult
            {
                public bool Ok, Pass, Dry, Stopped;
                public List<CSagGapPoint> Points = new List<CSagGapPoint>();
                public float[] GapOff = new float[0], GapOn = new float[0], DzPred = new float[0];   // 점 순서 (빈칸 NaN)
                public float[] Change = new float[0], ChangeErr = new float[0];                     // 켬 − 끔, (켬 − 끔) − 예측
                public double OnMean = double.NaN, OnSpread = double.NaN, MaxChangeErr = double.NaN;
                public int NJudged;                                       // 끔·켬 둘 다 잰 점 수
                public int NOn;                                           // 켬 틈을 잰 점 수 (켬 틈 고르기는 2점 이상이어야 판정)
                public string Verdict = "";                               // 합격 / 불합격 / 3D만 / 중단 / 판정 불가
                public string Why = "";                                   // 불합격·판정 불가의 까닭 (" · " 로 이음) — 시작하지 않았으면 그 까닭
                public string RunDir, ReportPath, Report = "", Line = "";
                public string CsvPath, SidecarPath;                       // 작업 폴더의 gap.csv, 결과 파일 옆 시험 기록
                public int[] UncheckedAxes = new int[0];                  // 방향 확인 안 된 축 (U4 — 경고만)
                public string MotorMapWarn = "";                          // 결과 파일의 매핑이 지금 연결과 다름 (C10)
                public bool RestOk;                                       // 끝에 휴지 자세로 돌아왔다
            }

            /// <summary>결과·백업 파일 목록 한 줄</summary>
            public class CSagFileInfo
            {
                public string Path = "", Name = "", Display = "", TestState = "", Brief = "";
                public string GapVerdict = "", TouchVerdict = "";   // 시험 기록의 마지막 실물 판정 — 합격·불합격 (없으면 판정 불가, 그것도 없으면 ""). 뒤에 온 3D만·중단 줄은 보지 않는다
                public string MotorMap = null;             // 파일의 motormap= (없으면 null = 기본 매핑)
                public DateTime Time;
                public bool IsActive;                      // 적용 중 파일 (<urdf>\sagcomp_model.txt)
                public bool SameAsActive;                  // 결과 파일이 적용 중 파일과 바이트가 같다 (적용한 파일)
                public int NFc, NGp;                       // 첫닿음 학습점 수, 바닥 GP 학습점 수
            }

            /// <summary>첫닿음 데이터셋 통계 (kind·hold 별 행 수)</summary>
            public class CSagDatasetStat
            {
                public bool Exists;
                public string Path = "", Why = "";
                public int Rows, Broken;                   // 읽은 행, 칸이 모자라거나 머리줄 없이 온 행
                public int F, FHold, H, C, X, Blank, Hold; // kind 별 수 (F 중 손목 유지 켬 = FHold), hold=1 전체
                public int Sessions, Groups;               // 측정 회차 수, 묶음(session_group) 수
            }

            // ────────────────────────────────────────────────────────────
            // 경로
            // ────────────────────────────────────────────────────────────
            public const string SAGRUN_ACTIVE = "sagcomp_model.txt";
            public const string SAGRUN_FC_DATASET = "floor_dataset_fc.csv";
            /// <summary><urdf>\sagcomp_model.txt (적용 중 = 활성 파일)</summary>
            public static string SagRun_ActivePath(string strUrdfDir) { return SagRun_Join(strUrdfDir, SAGRUN_ACTIVE); }
            /// <summary><urdf>\sagcomp_results — 학습 결과 파일·보고서·시험 기록·apply_log</summary>
            public static string SagRun_ResultsDir(string strUrdfDir) { return SagRun_Join(strUrdfDir, "sagcomp_results"); }
            /// <summary><urdf>\sagcomp_runs — 계획·작업 폴더</summary>
            public static string SagRun_RunsDir(string strUrdfDir) { return SagRun_Join(strUrdfDir, "sagcomp_runs"); }
            /// <summary><urdf>\sagcomp_backup — 적용·되돌리기 전 활성 파일 백업</summary>
            public static string SagRun_BackupDir(string strUrdfDir) { return SagRun_Join(strUrdfDir, "sagcomp_backup"); }
            /// <summary><urdf>\floor_dataset_fc.csv — 첫닿음 데이터셋 (측정이 덧붙이고 학습이 읽는다)</summary>
            public static string SagRun_FcDatasetPath(string strUrdfDir) { return SagRun_Join(strUrdfDir, SAGRUN_FC_DATASET); }
            /// <summary>모델 파일의 시험 기록 경로 — X.txt → X_test.txt (활성 sagcomp_model.txt → sagcomp_model_test.txt)</summary>
            public static string SagRun_TestRecordPath(string strModelPath)
            {
                if (string.IsNullOrEmpty(strModelPath)) return "";
                try { return Path.Combine(Path.GetDirectoryName(strModelPath) ?? "", Path.GetFileNameWithoutExtension(strModelPath) + "_test.txt"); }
                catch { return ""; }
            }
            /// <summary>설정의 경로 → 실제 경로: 비면 urdf 폴더의 strDefault, 상대경로면 urdf 폴더 기준</summary>
            public static string SagRun_Resolve(string strUrdfDir, string strPath, string strDefault)
            {
                try
                {
                    if (string.IsNullOrEmpty(strPath) || strPath.Trim().Length == 0) return string.IsNullOrEmpty(strDefault) ? "" : SagRun_Join(strUrdfDir, strDefault);
                    string p = strPath.Trim();
                    return Path.IsPathRooted(p) ? p : SagRun_Join(strUrdfDir, p);
                }
                catch { return strPath ?? ""; }
            }
            /// <summary>urdf 폴더 안이면 상대경로로 (설정 파일에 적을 때), 밖이면 그대로</summary>
            public static string SagRun_Relative(string strUrdfDir, string strPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(strPath) || string.IsNullOrEmpty(strUrdfDir)) return strPath ?? "";
                    string d = Path.GetFullPath(strUrdfDir).TrimEnd('\\', '/') + Path.DirectorySeparatorChar, p = Path.GetFullPath(strPath);
                    return p.StartsWith(d, StringComparison.OrdinalIgnoreCase) ? p.Substring(d.Length) : strPath;
                }
                catch { return strPath ?? ""; }
            }
            private static string SagRun_Join(string strDir, string strName)
            {
                try { return Path.Combine(strDir ?? "", strName); } catch { return strName; }
            }
            private static bool SagRun_SamePath(string a, string b)
            {
                try { return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase); }
                catch { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
            }

            /// <summary>첫닿음 데이터셋 통계 — kind(F/H/C/X/빈칸)·hold 별 행 수, 회차·묶음 수. 파일이 없으면 Exists = false. 예외 없음.</summary>
            public static CSagDatasetStat SagRun_DatasetStat(string strPath)
            {
                var st = new CSagDatasetStat { Path = strPath ?? "" };
                try
                {
                    if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { st.Why = "파일 없음"; return st; }
                    st.Exists = true;
                    string why; int nBroken;
                    List<CSagDsRow> rows = SagRun_ReadDataset(File.ReadAllText(strPath), out why, out nBroken);
                    SagRun_CountRows(rows, st);
                    st.Broken = nBroken; st.Why = why;
                }
                catch (Exception ex) { st.Why = "★오류: " + SagRun_Msg(ex); }
                return st;
            }
            /// <summary>데이터셋 한 줄 설명 — "floor_dataset_fc.csv · 첫닿음 73행 (켬 0) · 회차 3 · 묶음 10" (H/C/X·판정 없음·못 읽은 행은 있으면 붙인다).
            /// 반환 = 행 수 (파일이 없으면 −1)</summary>
            public static int SagRun_DatasetInfo(string strPath, out string strInfo)
            {
                CSagDatasetStat st = SagRun_DatasetStat(strPath);
                string name = "";
                try { name = Path.GetFileName(strPath ?? ""); } catch { name = strPath ?? ""; }
                if (!st.Exists) { strInfo = name + " 없음"; return -1; }
                var sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} · 첫닿음 {1}행 (켬 {2})", name, st.F, st.FHold);
                if (st.H > 0) sb.AppendFormat(CultureInfo.InvariantCulture, " · 단단한 정지 {0}", st.H);
                if (st.C > 0) sb.AppendFormat(CultureInfo.InvariantCulture, " · 검열 {0}", st.C);
                if (st.X > 0) sb.AppendFormat(CultureInfo.InvariantCulture, " · 못 씀 {0}", st.X);
                if (st.Blank > 0) sb.AppendFormat(CultureInfo.InvariantCulture, " · 판정 없음 {0}", st.Blank);
                if (st.Broken > 0) sb.AppendFormat(CultureInfo.InvariantCulture, " · 못 읽은 줄 {0}", st.Broken);
                sb.AppendFormat(CultureInfo.InvariantCulture, " · 회차 {0} · 묶음 {1}", st.Sessions, st.Groups);
                strInfo = sb.ToString();
                return st.Rows;
            }
            private static void SagRun_CountRows(List<CSagDsRow> rows, CSagDatasetStat st)
            {
                var ss = new List<string>(); var gs = new List<string>();
                foreach (CSagDsRow r in rows)
                {
                    st.Rows++;
                    if (r.Hold) st.Hold++;
                    switch (r.Kind)
                    {
                        case "F": st.F++; if (r.Hold) st.FHold++; break;
                        case "H": st.H++; break;
                        case "C": st.C++; break;
                        case "X": st.X++; break;
                        default: st.Blank++; break;
                    }
                    if (!ss.Contains(r.Session)) ss.Add(r.Session);
                    string g = r.Session + "_" + r.Group; if (!gs.Contains(g)) gs.Add(g);
                }
                st.Sessions = ss.Count; st.Groups = gs.Count;
            }

            // ────────────────────────────────────────────────────────────
            // 계획 기구학 — 지금 3D 의 수식 글로 만든 따로 된 C3d (계획·IK 확인·예상 — 화면 3D 를 흔들지 않는다)
            // ────────────────────────────────────────────────────────────
            private static int s_nSagRunBusy = 0;                    // 실물·3D 동작 작업은 한 번에 하나 (작업이 Interlocked 로 잡고 놓는다)
            /// <summary>로봇팔 작업(측정·시험)이 진행 중 — 적용·되돌리기·쌍둥이 바꾸기는 거절한다</summary>
            public static bool SagRun_IsBusy { get { return Thread.VolatileRead(ref s_nSagRunBusy) != 0; } }
            // 계획 기구학 묶음 — 3D 의 수식 글(Src)과 그 글로 채운 계획 기구학 C3d. C3d(OpenGL 컨트롤 — 창 핸들·GL 컨텍스트)는 처음 한 번만 만들고
            //   (CScene_t.CreateOmx — 예전 복제 기구학과 같은 생성), 3D 의 DH 행이 바뀌면 그 C3d 의 수식 슬롯만 바꿔 CheckForward 해 새 묶음(차례 Gen + 1)으로 한다.
            //   다시 만들지 않으므로 창 핸들·GL 컨텍스트가 늘지 않고 화면 3D 의 GL 컨텍스트도 건드리지 않는다. 같은 C3d 를 고쳐 쓰므로 누가 계획 기구학을
            //   쓰는 동안 (공개 입구가 잡는 SagRun_KinUse — 작업·학습·계획·정의 확인, 작업 중 SagRun_IsBusy) 은 바꾸지 않는다
            private sealed class CSagRunKin
            {
                public C3d Kin;                                      // 계획 기구학 (묶음이 바뀌어도 같은 C3d — STA 에서 처음 한 번 만든다)
                public string[] Src;                                 // 이 묶음이 본 3D 의 pstrKinematics[0..511] 참조 사본 (null 포함) — 글만 바뀌고 DH 행이 같으면 새 글로 바꿔 둔다
                public int Gen;                                      // 만든 차례 (정의 확인 캐시 키 — 늘 늘어난다)
                public bool BuiltIn;                                 // 3D 에 수식이 하나도 없어 내장 OMX (CScene_t.CreateOmx 의 수식 0) 로 채웠다
                public Dictionary<int, string> Sig = new Dictionary<int, string>();   // 수식별 기구학 표시 (SagRun_KinSig — 잠금 m_oSagRunKin)
            }
            private volatile CSagRunKin m_SagRunKinSt = null;        // 지금 계획 기구학 묶음 (없으면 null)
            private C3d m_SagRunKin = null;                          // 계획 기구학 C3d — 처음 한 번 만든 뒤 바꾸지 않는다 (리플렉션으로 계획 기구학을 보는 진단 도구가 쓰던 이름)
            private readonly object m_oSagRunKin = new object();    // 묶음 바꾸기·쓰는 수 (m_nSagRunKinUse)·계획 기구학의 관절값을 시드로 쓰는 동안 (SagComp_FcPlan) 한 스레드만
            private int m_nSagRunKinUse = 0;                         // 계획 기구학을 쓰는 중인 공개 입구 수 (SagRun_KinUse — 잠금 m_oSagRunKin). 0 이 아니면 묶음을 바꾸지 않는다
            private int m_nSagRunKinGen = 0;                         // 마지막 묶음 차례
            private static string s_strOmxKinSig = null;             // 내장 OMX 수식 0 의 기구학 표시 (처음 C3d 를 만들 때 한 번) — 표시가 빈 옛 모델 = 이 값
            private static string s_strOmxKinText = null;            // 내장 OMX 수식 0 의 글 (처음 C3d 를 만들 때 한 번) — 3D 에 수식이 없어지면 이 글로 다시 채운다
            private const int SAGRUN_KIN_SLOTS = 512;

            /// <summary>UI 스레드에서 — 계획 기구학을 준비한다 (C3d 는 OpenGL 컨트롤이라 작업 스레드에서 만들지 않는다). 앱은 시작 때 한 번, 로봇팔 세팅 탭을 볼 때·작업 전에 부른다.
            /// SagRun_Prepare(out why) 와 같다 (까닭은 버린다). 반환 = 준비됨</summary>
            public bool SagRun_Prepare()
            {
                string why;
                return SagRun_Prepare(out why);
            }
            /// <summary>UI(STA) 스레드에서 — 계획 기구학을 준비한다. 처음이면 C3d 를 만들고 (CScene_t.CreateOmx — 만든 뒤 화면 3D 의 GL 컨텍스트를 되돌린다,
            /// 현재 폴더에 freeglut.dll 을 쓰지 못하면 임시 폴더에서 한 번 더), 3D 의 DH 행이 묶음과 다르면 같은 C3d 의 수식 슬롯을 지금 3D 의 글로 바꾼다
            /// (글만 바뀌고 DH 행이 같으면 C3d 는 그대로). 작업·학습·계획이 계획 기구학을 쓰는 동안이나 작업 스레드에서 부르면 바꾸지 않고 지금 묶음 그대로 (true).
            /// 반환 = 준비됨. false 면 why = 까닭 (만들지 못함·지금 3D 의 수식으로 바꾸지 못함 — 그때는 묶음이 없어 계획·작업이 거절한다). 예외 없음</summary>
            public bool SagRun_Prepare(out string why)
            {
                why = "";
                try
                {
                    CSagRunKin st = SagRun_KinState(out why);
                    if (st != null && why.Length == 0) return true;
                    if (why.Length == 0) why = SAGRUN_NO_KIN;
                    return false;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
            }
            // 계획 기구학 — 지금 묶음의 C3d (없으면 null — 부른 함수가 "복제 기구학 없음" 으로 실패)
            private C3d SagRun_Kin()
            {
                CSagRunKin st = SagRun_KinState();
                return st == null ? null : st.Kin;
            }
            private CSagRunKin SagRun_KinState()
            {
                string why;
                return SagRun_KinState(out why);
            }
            // 계획 기구학 묶음 — 3D 수식 글이 묶음의 글과 같으면 그대로. 다르면 (또는 아직 없으면) STA 스레드이고 아무도 쓰지 않을 때만 (작업 SagRun_IsBusy·
            //   SagRun_KinUse 0) 맞춘다: C3d 가 없으면 만들고 (SagRun_NewKinC3d — 3D 에 수식이 없으면 만든 그대로 = 내장 OMX), 지금 3D 의 글을 먼저 따로 해석해
            //   (C3d 는 아직 그대로) 글 있는 슬롯이 같고 DH 행이 계획 기구학과 비트까지 같으면 (설명·'@' 부품 줄·'!' 뒤 줄만 고침) 새 글만 기억한다. 아니면 같은 C3d 의
            //   512 슬롯을 비우고 3D 슬롯 글을 그대로 넣어 CheckForward (3D 가 합쳐 둔 글 없는 '전체 수식' 슬롯은 따라오지 않는다) — 3D 에 수식이 없으면 내장 OMX 글로.
            //   새 묶음이면 정의 확인 캐시를 비운다. STA 가 아니거나 누가 쓰는 중이면 지금 묶음 그대로 (DH 행까지 다르면 정의 확인이 '3D 모델이 바뀜' 으로 알린다 —
            //   SagRun_SameKinematics). 만들거나 바꾸지 못하면 why·m_strSagRunKinErr 에 까닭 — C3d 를 고치다 실패했으면 묶음을 버린다 (다음 STA 호출이 다시 채운다)
            private CSagRunKin SagRun_KinState(out string why)
            {
                why = "";
                lock (m_oSagRunKin)
                {
                    CSagRunKin st = m_SagRunKinSt;
                    if (st != null && SagRun_SameSrc(st)) return st;
                    if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) return st;
                    if (st != null && (SagRun_IsBusy || m_nSagRunKinUse > 0)) return st;   // 작업·학습·계획 중 — 쓰는 기구학을 바꾸지 않는다 (DH 행이 같으면 그대로 써도 된다 — SagRun_SameRows)
                    string[] a = m_CHeader != null ? m_CHeader.pstrKinematics : null;
                    var src = new string[SAGRUN_KIN_SLOTS];
                    bool bAny = false;
                    for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                    {
                        src[i] = (a != null && i < a.Length) ? a[i] : null;
                        if (!string.IsNullOrEmpty(src[i])) bAny = true;
                    }
                    bool bTouched = false;                                       // 계획 기구학 C3d 의 슬롯을 고치기 시작했다 (실패하면 묶음을 버린다)
                    try
                    {
                        C3d k = m_SagRunKin;
                        if (k == null)
                        {
                            k = SagRun_NewKinC3d();
                            if (s_strOmxKinSig == null) s_strOmxKinSig = SagRun_KinSigOf(k, 0);
                            if (s_strOmxKinText == null) s_strOmxKinText = k.m_CHeader.pstrKinematics[0];
                            m_SagRunKin = k;
                            st = null;                                           // 새 C3d — 앞 묶음(없음)과 견주지 않는다
                            if (!bAny) return SagRun_KinSwap(k, src, true);      // 만든 그대로 = 내장 OMX
                        }
                        if (!bAny && s_strOmxKinText == null) throw new InvalidOperationException("내장 OMX 수식 글 없음");
                        // 새 글을 먼저 따로 해석 (C3d 는 아직 그대로) — 해석이 같은 CKinematics.CForward.MakeDhParam (CheckForward 와 같다)
                        var parsed = new CDhParamAll[SAGRUN_KIN_SLOTS];
                        for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                        {
                            string t = bAny ? src[i] : (i == 0 ? s_strOmxKinText : null);
                            if (!string.IsNullOrEmpty(t)) CKinematics.CForward.MakeDhParam(t, out parsed[i]);
                        }
                        if (st != null && st.BuiltIn == !bAny && SagRun_SameSlots(st.Src, src) && SagRun_RowsSame(parsed, k))
                        {
                            st.Src = src;                                        // 글만 바뀜 — 계산은 그대로 (C3d·차례·캐시 그대로)
                            m_strSagRunKinErr = "";
                            return st;
                        }
                        bTouched = true;
                        SagRun_KinLoad(k, bAny ? src : null);
                        return SagRun_KinSwap(k, src, !bAny);
                    }
                    catch (Exception ex)
                    {
                        why = "계획 기구학을 지금 3D 모델로 " + (m_SagRunKin == null ? "만들지" : "바꾸지") + " 못함 — " + SagRun_Msg(ex);
                        m_strSagRunKinErr = why;
                        if (bTouched) { m_SagRunKinSt = null; SagDef_CacheClear(); }
                        return m_SagRunKinSt;
                    }
                }
            }
            private string m_strSagRunKinErr = "";                   // 마지막으로 계획 기구학을 만들거나 바꾸지 못한 까닭 ("" = 없음)
            // 새 묶음 (잠금 m_oSagRunKin 안에서) — 차례를 늘리고 정의 확인 캐시를 비운다
            private CSagRunKin SagRun_KinSwap(C3d k, string[] src, bool bBuiltIn)
            {
                var nst = new CSagRunKin { Kin = k, Src = src, Gen = ++m_nSagRunKinGen, BuiltIn = bBuiltIn };
                m_SagRunKinSt = nst;
                m_strSagRunKinErr = "";
                SagDef_CacheClear();
                return nst;
            }
            // 계획 기구학 C3d 의 수식 슬롯을 다시 채운다 — src 가 있으면 512 슬롯을 비우고 (글·해석 결과 null) 3D 의 글을 그대로 넣어 CheckForward
            //   (처음 만든 C3d 에 3D 수식을 넣던 예전 차례와 같다). src 가 null 이면 내장 OMX — CScene_t.CreateOmx 직후와 같게
            //   (0~255 = 빈 글·빈 해석 결과, 256~511 = null, 수식 0 = 내장 OMX 글) 채워 CheckForward
            private static void SagRun_KinLoad(C3d k, string[] src)
            {
                string[] t = k.m_CHeader.pstrKinematics;
                CDhParamAll[] d = k.m_CHeader.pDhParamAll;
                int n = Math.Min(SAGRUN_KIN_SLOTS, Math.Min(t.Length, d.Length));
                if (src != null)
                {
                    for (int i = 0; i < n; i++) { t[i] = null; d[i] = null; }
                    for (int i = 0; i < n; i++) if (!string.IsNullOrEmpty(src[i])) t[i] = src[i];
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (i < 256) { t[i] = ""; d[i] = new CDhParamAll(); d[i].DeleteAll(); }
                        else { t[i] = null; d[i] = null; }
                    }
                    t[0] = s_strOmxKinText;
                }
                k.CheckForward();
            }

            [System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "wglGetCurrentDC")] private static extern IntPtr SagRun_GlCurDc();
            [System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "wglGetCurrentContext")] private static extern IntPtr SagRun_GlCurRc();
            [System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")] private static extern bool SagRun_GlMakeCur(IntPtr hDc, IntPtr hRc);

            // 계획 기구학 C3d 를 처음 만든다 (CScene_t.CreateOmx — 예전 복제 기구학과 같은 생성, 관절값 0·싱크 같음). C3d 를 만들면 그 GL 컨텍스트가 지금 것이 되므로
            //   만든 뒤 원래 것으로 되돌린다 (화면 3D 는 그리기 전에 컨텍스트를 다시 잡지 않는다 — 되돌리지 않으면 다음 그리기가 이 숨은 컨트롤로 간다).
            //   C3d 생성자는 현재 폴더에 freeglut.dll 을 쓰는데, 쓰지 못하면 (이미 올라와 잠김·쓰기 금지 폴더) 임시 폴더에서 한 번 더
            //   (COjw_44_ArmMotor.cs WithTempC3d 와 같은 방법). 둘 다 실패하면 예외
            private static C3d SagRun_NewKinC3d()
            {
                IntPtr hDc = IntPtr.Zero, hRc = IntPtr.Zero;
                bool bCtx = false;
                try { hDc = SagRun_GlCurDc(); hRc = SagRun_GlCurRc(); bCtx = true; } catch { }
                try
                {
                    try { return CScene_t.CreateOmx().c3d; }
                    catch (Exception ex1)
                    {
                        string strCwd = Environment.CurrentDirectory;
                        try
                        {
                            string strAlt = Path.Combine(Path.GetTempPath(), "OpenJigWare_ojw");
                            Directory.CreateDirectory(strAlt);
                            Environment.CurrentDirectory = strAlt;
                            return CScene_t.CreateOmx().c3d;
                        }
                        catch (Exception ex2) { throw new InvalidOperationException("3D 컨트롤을 만들지 못함: " + ex1.Message + " / " + ex2.Message, ex2); }
                        finally { try { Environment.CurrentDirectory = strCwd; } catch { } }
                    }
                }
                finally { if (bCtx) { try { SagRun_GlMakeCur(hDc, hRc); } catch { } } }
            }

            // 계획 기구학을 쓰는 동안 — 계획 기구학을 읽는 공개 입구(작업·학습·계획·정의 확인·정의 서명·적용·되돌리기·쌍둥이)가 using 으로 잡는다. 잡혀 있으면 묶음을 바꾸지 않는다 (같은 C3d 를 고쳐 쓰므로 —
            //   SagRun_KinState). 잡기 전에 한 번 묶음을 맞춘다 (UI(STA) 스레드면 바뀐 3D 수식으로 — 잡은 뒤에는 그 스레드도 바꾸지 않는다)
            private IDisposable SagRun_KinUse()
            {
                try { SagRun_KinState(); } catch { }
                lock (m_oSagRunKin) m_nSagRunKinUse++;
                return new CSagRunKinUse(this);
            }
            private sealed class CSagRunKinUse : IDisposable
            {
                private C3d m_c;
                public CSagRunKinUse(C3d c) { m_c = c; }
                public void Dispose()
                {
                    C3d c = Interlocked.Exchange(ref m_c, null);
                    if (c != null) lock (c.m_oSagRunKin) c.m_nSagRunKinUse--;
                }
            }

            // 3D 의 수식이 묶음과 같은 기구학인가 — 글이 같거나 (SagRun_SameSrc), 글은 바뀌었어도 3D 가 해석해 둔 DH 행이 글 있는 슬롯마다 묶음과 비트까지 같으면
            //   (설명·'@' 부품 줄·'!' 뒤 줄만 고침 — 계산이 바뀌지 않는다). 작업 스레드는 묶음을 새로 만들 수 없으므로 이 경우를 '바뀜' 으로 치지 않는다
            private bool SagRun_SameKinematics(CSagRunKin st)
            {
                return SagRun_SameSrc(st) || SagRun_SameRows(st);
            }
            private bool SagRun_SameRows(CSagRunKin st)
            {
                if (st == null || st.Kin == null || m_CHeader == null) return false;
                string[] a = m_CHeader.pstrKinematics;
                for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                {
                    bool b3 = a != null && i < a.Length && !string.IsNullOrEmpty(a[i]);
                    bool bK = st.Src != null && i < st.Src.Length && !string.IsNullOrEmpty(st.Src[i]);
                    if (b3 != bK) return false;
                    if (!b3) continue;
                    CDhParamAll d3 = (m_CHeader.pDhParamAll != null && i < m_CHeader.pDhParamAll.Length) ? m_CHeader.pDhParamAll[i] : null;
                    CDhParamAll dk = i < st.Kin.m_CHeader.pDhParamAll.Length ? st.Kin.m_CHeader.pDhParamAll[i] : null;
                    if (!SagRun_DhSame(d3, dk)) return false;
                }
                return true;
            }
            // 두 슬롯 글 배열의 '글 있는 슬롯' 이 같은가
            private static bool SagRun_SameSlots(string[] x, string[] y)
            {
                for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                {
                    bool bx = x != null && i < x.Length && !string.IsNullOrEmpty(x[i]), by = y != null && i < y.Length && !string.IsNullOrEmpty(y[i]);
                    if (bx != by) return false;
                }
                return true;
            }
            // 따로 해석한 DH 행 (parsed) 이 계획 기구학 C3d 의 해석 결과와 모든 슬롯에서 같은가 (없음 = 0 행)
            private static bool SagRun_RowsSame(CDhParamAll[] parsed, C3d k)
            {
                CDhParamAll[] d = k.m_CHeader.pDhParamAll;
                for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                    if (!SagRun_DhSame(i < parsed.Length ? parsed[i] : null, i < d.Length ? d[i] : null)) return false;
                return true;
            }
            // DH 행이 비트까지 같은가 — 계획 계산(SagFc_Fk)이 쓰는 칸: dA·dD·dTheta·dAlpha·축·방향 (null = 0 행)
            private static bool SagRun_DhSame(CDhParamAll p3, CDhParamAll pk)
            {
                int n3 = p3 == null ? 0 : p3.GetCount(), nk = pk == null ? 0 : pk.GetCount();
                if (n3 != nk) return false;
                for (int r = 0; r < n3; r++)
                {
                    CDhParam p = p3.GetData(r), q = pk.GetData(r);
                    if (p == null || q == null) return false;
                    if (BitConverter.DoubleToInt64Bits(p.dA) != BitConverter.DoubleToInt64Bits(q.dA) || BitConverter.DoubleToInt64Bits(p.dD) != BitConverter.DoubleToInt64Bits(q.dD)
                        || BitConverter.DoubleToInt64Bits(p.dTheta) != BitConverter.DoubleToInt64Bits(q.dTheta) || BitConverter.DoubleToInt64Bits(p.dAlpha) != BitConverter.DoubleToInt64Bits(q.dAlpha)
                        || p.nAxisNum != q.nAxisNum || p.nAxisDir != q.nAxisDir) return false;
                }
                return true;
            }
            // 3D 수식 글이 묶음의 글과 같은가 (같은 참조면 바로 같다, null 과 빈 글은 같다)
            private bool SagRun_SameSrc(CSagRunKin st)
            {
                if (st == null) return false;
                string[] a = m_CHeader != null ? m_CHeader.pstrKinematics : null;
                string[] s = st.Src;
                for (int i = 0; i < SAGRUN_KIN_SLOTS; i++)
                {
                    string x = (a != null && i < a.Length) ? a[i] : null, y = (s != null && i < s.Length) ? s[i] : null;
                    if (ReferenceEquals(x, y)) continue;
                    if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y)) continue;
                    if (!string.Equals(x, y, StringComparison.Ordinal)) return false;
                }
                return true;
            }
            // 기구학 표시 — 수식 nFunc 의 DH 행 (dA, dD, dTheta, dAlpha 는 "R", 축·방향) 을 이어 붙인 UTF-8 의 FNV-1a 32비트 ("X8"). 수식이 비면 ""
            private static string SagRun_KinSigOf(C3d k, int nFunc)
            {
                if (k == null || k.m_CHeader == null || k.m_CHeader.pDhParamAll == null || nFunc < 0 || nFunc >= k.m_CHeader.pDhParamAll.Length) return "";
                CDhParamAll dh = k.m_CHeader.pDhParamAll[nFunc];
                int n = dh == null ? 0 : dh.GetCount();
                if (n <= 0) return "";
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                for (int i = 0; i < n; i++)
                {
                    CDhParam p = dh.GetData(i);
                    sb.Append(string.Format(ci, "{0:R},{1:R},{2:R},{3:R},{4},{5};", p.dA, p.dD, p.dTheta, p.dAlpha, p.nAxisNum, p.nAxisDir));
                }
                uint h = 2166136261u;
                unchecked { foreach (byte b in Encoding.UTF8.GetBytes(sb.ToString())) { h ^= b; h *= 16777619u; } }
                return h.ToString("X8", ci);
            }
            // 묶음 st 의 수식 nFunc 기구학 표시 (묶음마다 캐시)
            private string SagRun_KinSig(CSagRunKin st, int nFunc)
            {
                if (st == null) return "";
                string s;
                lock (m_oSagRunKin) { if (st.Sig.TryGetValue(nFunc, out s)) return s; }
                s = SagRun_KinSigOf(st.Kin, nFunc);
                lock (m_oSagRunKin) { st.Sig[nFunc] = s; }
                return s;
            }
            // 모델의 기구학 표시 — 빈 값(옛 모델·표시 없음) = 내장 OMX 의 표시 (옛 모델은 모두 CreateOmx 기구학으로 만들어졌다)
            private static string SagRun_KinSigNorm(string strSig)
            {
                return string.IsNullOrEmpty(strSig) ? (s_strOmxKinSig ?? "") : strSig;
            }
            // 계획 기구학이 없음 — 앱은 시작 때 UI 스레드에서 SagRun_Prepare 를 부른다 (스크립트도 같다). 화면 글이라 함수 이름은 넣지 않는다
            private const string SAGRUN_NO_KIN = "계획 기구학을 만들지 못했습니다 — 앱을 다시 켜십시오";
            private const float SAGRUN_GAP_CLEAR_MM = 10f;           // 틈 재기 펜 높이 ≥ 그 점의 예측 닿는 높이(끔 첫닿음 FK, 켬 Ref 중 높은 쪽) + 이 값 (mm)

            // ── 피치와 손목 해 ──
            //   피치(Ry)는 PlayXyz·SagComp_FloorPress·CalcInv_Tool 의 규약이다: 90 = 수직 아래, 공구 피치는 'TCP − 손목 원점' 벡터의 기울기 (CalcToolPitch).
            //   이 값은 90 에서 접힌다 (수평 성분이 늘 + 라 90 을 넘지 못한다) — 피치 70 에는 손목 해가 둘 있다: 관절 합(12+13+14) = 110 (공구 끝이 베이스 쪽으로 기욺)
            //   과 70 (바깥쪽으로 기욺). PlayXyz 는 수직 자세에서 이어 가며 110 쪽으로 간다 (반경 140~220, 10-02 확인 — 09-29 의 '피치 70' 자료도 관절 합 110).
            //   판정의 '피치' 와 첫닿음 데이터셋(floor_dataset_fc.csv)의 pitch 칸은 관절 합이다 (피치 110 = 여기 Ry 70). 측정이 덧붙이는 행도
            //   SagRun_DatasetRows 가 관절 합으로 적는다 — SagComp_FloorDatasetRows 는 Ry 를 적으므로 그대로 쓰면 한 파일에 두 규약이 섞여
            //   첫닿음 학습의 '같은 자리 다른 측정 엇갈림' 검사(피치 1° 안)가 놓친다. group 이름(p70y0)은 두 쪽 모두 Ry. Ry > 90 은 IK 로 풀리지 않는다.
            //   그래서 계획·확인은 손목 해를 관절 합 = 180 − 피치 쪽으로 시드하고, 풀린 해가 그쪽인지 확인한다 (아니면 IK 불가로 친다).
            //   로봇 정의 (10-04): 수직(Ry 90)의 관절 합은 정의의 downsum (OMX 90 — 3D 에서 잰 값과 0.5° 안, SagDef_Check V7). Ry < 90 은 downsum + (90 − Ry).
            //   식은 (downsum − 90) + (180 − Ry) 꼴로 쓴다 — downsum 90 이면 예전 식 180 − Ry 와 float 비트까지 같다 ((90 − 90) = +0, 0 + x = x).

            // 시드 자세가 든 관절값 사본 — 위치 3개 = (요, 0, 40), 손목 = 관절 합이 SagArm_SumFor(피치) 가 되게 (OMX 피치 90 → 50). 다른 관절은 계획 기구학 값 그대로
            private static double[] SagRun_Seed(C3d kin, CSagArmCfg arm, float fYaw, float fPitch)
            {
                double[] ad = kin.SagFc_MotCopy();
                kin.SagFc_Set(ad, arm.PosIDs[0], fYaw); kin.SagFc_Set(ad, arm.PosIDs[1], 0f);
                kin.SagFc_Set(ad, arm.PosIDs[2], 40f); kin.SagFc_Set(ad, arm.WristID, SagRun_SeedWrist(arm, Math.Min(90f, fPitch)));
                return ad;
            }
            // 시드 손목 (피치 1·2 = 0·40 일 때 관절 합 = SagArm_SumFor(p) − 그 둘) — (downsum − 90) + (140 − p) 꼴 (downsum 90 이면 예전 140 − p 와 비트까지 같다.
            //   downsum + (90 − p) − 40 꼴은 float 1 ulp 다를 수 있다)
            private static float SagRun_SeedWrist(CSagArmCfg arm, float p) { return (arm.DownSumDeg - 90f) + (140f - p); }
            // 사본 IK (CalcInv_Tool 과 같은 풀이, 시드 = 사본의 지금 값) — 손목 해를 관절 합 = 180 − 피치 쪽으로 (피치 < 90 이면 시드 손목을 그쪽으로 옮기고 풀린 해를 확인).
            //   bRelax = PlayXyz 처럼 피치를 10° 씩 30° 까지 늦춰 본다 (복귀 높이 — 공중이라 피치는 상관없다). pUsed = 실제로 푼 피치.
            //   풀린 해의 관절 합이 그 피치의 관절 합(SagArm_SumFor)에서 1° 넘게 벗어나면 버린다 — 피치 90 도 (10-03: 이 풀이는 TCP 만 확인해, 시드가 멀면
            //   공구가 몇 도 기운 해를 내기도 한다 — 요 −8° 시드로 (250, 50, 100) → 관절 합 81°). 사본의 시드로 안 되면 계획 시드 (요, 0, 40) 로 한 번 더.
            //   성공하면 사본을 해로 바꿔 다음 시드로 (이어지는 해)
            private static float[] SagRun_Ik(C3d kin, CSagArmCfg arm, double[] ad, float x, float y, float z, float pitch, bool bRelax, out float pUsed)
            {
                pUsed = pitch;
                for (int nSeed = 0; nSeed < 2; nSeed++)
                {
                    double[] adS = nSeed == 0 ? ad : SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), Math.Min(90f, pitch));
                    for (float relax = 0; relax <= (bRelax ? 30f : 0f); relax += 10f)
                    {
                        float p = pitch - relax;
                        double[] a2 = (double[])adS.Clone();
                        if (p < 89.99f) kin.SagFc_Set(a2, arm.WristID, (float)(((double)(arm.DownSumDeg - 90f) + (180.0 - p)) - a2[arm.PosIDs[1]] - a2[arm.PosIDs[2]]));
                        float[] q = kin.SagFc_CalcInvTool(arm.FuncNumber, a2, x, y, z, p, arm.WristID, arm.PosIDs);
                        if (q == null) continue;
                        if (Math.Abs((q[1] + q[2] + q[3]) - SagArm_SumFor(arm, p)) > 1.0) continue;   // 다른 쪽 손목 해 또는 기운 공구
                        for (int k = 0; k < 3; k++) kin.SagFc_Set(ad, arm.PosIDs[k], q[k]);
                        kin.SagFc_Set(ad, arm.WristID, q[3]);
                        pUsed = p;
                        return q;
                    }
                }
                return null;
            }
            private static float[] SagRun_Ik(C3d kin, CSagArmCfg arm, double[] ad, float x, float y, float z, float pitch)
            {
                float pUsed; return SagRun_Ik(kin, arm, ad, x, y, z, pitch, false, out pUsed);
            }
            // 옮기는 높이 — (x, y) 위 SafeZ 에 리치 여유를 두고 갈 수 있으면 SafeZ, 아니면 5mm 씩 내려 zLow 까지 처음으로 되는 높이. 없으면 NaN.
            //   된다 = 관절 합을 정한 IK(SagArm_IkSum — 피치를 늦추지 않는다)가 피치 90 으로(옮기기) 그리고 pitch 가 90 이 아니면 그 피치로도(옮기는 높이에서
            //   피치를 바꾼다) 풀리고, 두 자세 모두 팔꿈치 굽힘이 SAGARM_BEND_MIN_DEG 이상 (리치 경계에서 떨어져 있다).
            //   ★10-03 검토: 예전에는 피치를 30° 까지 늦춰 풀리는 첫 높이를 골라 리치 경계 바로 위였다 (r260 → z105 굽힘 3.5°, (250, 50) z110 → 15.9°)
            //   — 그 높이의 경로는 틱마다 IK 가 다른 해로 건너뛰었다 (한 틱 5~46°). 이제 r260 → z90 (23°), (250, 50) → z105 (21°).
            //   zLow = 옮기는 높이의 아래 한계 (측정·시험 = 창 위 = 시작 + 10, 틈 재기 = 펜 높이 + 5) — SafeZ 부터 그보다 낮으면, zLow 가 NaN·무한이면 NaN
            //   (옮기다 창·바닥을 지나지 않게 — sagarm.txt 의 safez 가 틀려도 그 아래로 옮기는 점을 만들지 않는다). 성공하면 사본을 그 높이의 피치 90 해로 바꾼다
            private static float SagRun_TransferZ(C3d kin, CSagArmCfg arm, double[] ad, float x, float y, float pitch, float zLow)
            {
                if (float.IsNaN(zLow) || float.IsInfinity(zLow) || float.IsNaN(arm.SafeZ) || float.IsInfinity(arm.SafeZ)) return float.NaN;
                bool bPitch = pitch < 89.99f && !float.IsNaN(pitch);
                for (int i = 0; i < 100; i++)
                {
                    float z = arm.SafeZ - 5f * i;
                    if (z < zLow - 1e-3f) break;
                    float bend;
                    float[] q = SagArm_IkSum(kin, arm, ad, x, y, z, arm.DownSumDeg, out bend);      // 피치 90 (정의의 수직 관절 합)
                    if (q == null || !(bend >= SAGARM_BEND_MIN_DEG)) continue;
                    if (bPitch)
                    {
                        float bend2;
                        double[] a2 = SagArm_AdWith(kin, arm, ad, q);
                        if (SagArm_IkSum(kin, arm, a2, x, y, z, SagArm_SumFor(arm, pitch), out bend2) == null || !(bend2 >= SAGARM_BEND_MIN_DEG)) continue;
                    }
                    for (int k = 0; k < 3; k++) kin.SagFc_Set(ad, arm.PosIDs[k], q[k]);
                    kin.SagFc_Set(ad, arm.WristID, q[3]);
                    return z;
                }
                return float.NaN;
            }

            // ── 관절 합을 정한 IK·팔꿈치 굽힘 (10-03) ──
            //   OMX 의 위치 관절 셋(11 요, 12·13 피치 면)과 손목(14)은 한 수직 면 안에 있고 공구 방향은 관절 합 12+13+14 와 요로만 정해진다
            //   (관절 합 0 = 바깥 수평, 90 = 수직 아래, 90 보다 크면 끝이 베이스 쪽). 그래서 TCP 와 관절 합이 정해지면 손목 원점이 정해지고,
            //   위치 관절 셋을 그 손목 원점에 맞춘 뒤 손목 = 관절 합 − 12 − 13 이면 된다 — 공구 피치를 접힌 값(90 에서 꺾임)으로 맞추는 CalcInv_Tool 과 달리
            //   기운 해가 나오지 않는다. 실물을 움직이는 다리(SagArm_LegTo)·옮기는 높이(SagRun_TransferZ)는 이것을 쓴다.
            private const float SAGARM_BEND_MIN_DEG = 20f;           // 옮기는 높이·다리 끝 자세의 팔꿈치 굽힘 하한 (deg, 0 = 다 폈음 — 리치 경계)
            /// <summary>공구 피치 Ry → 관절 합 PosIDs[1] + PosIDs[2] + WristID (deg) — Ry 90 = 정의의 수직 관절 합 downsum (OMX 90 — 수직 아래),
            /// Ry &lt; 90 = downsum + (90 − Ry) (공구 끝이 베이스 쪽 — 계획·데이터셋의 규약, OMX 180 − Ry). (d − 90) + (180 − Ry) 꼴 — d 90 이면 예전 식과 비트까지 같다.
            /// c 가 null 이면 d = 90</summary>
            private static float SagArm_SumFor(CSagArmCfg c, float fPitch)
            {
                float d = (c == null) ? 90f : c.DownSumDeg;
                return fPitch >= 89.99f ? d : (d - 90f) + (180f - fPitch);
            }
            // 표시용 — 관절 합 ↔ 피치를 바꿔 적는다 (정의의 수직 관절 합 d 를 가운데로 뒤집기: (d − 90) + (180 − x), OMX 180 − x). 피치 &gt; 90 을 관절 합으로 적은 것으로 보고 그 피치를 알릴 때
            private static float SagArm_SumMirror(CSagArmCfg c, float x)
            {
                float d = (c == null) ? 90f : c.DownSumDeg;
                return (d - 90f) + (180f - x);
            }
            // 사본 adBase 에 위치 3개 + 손목 자세 q 를 넣은 새 사본
            private static double[] SagArm_AdWith(C3d kin, CSagArmCfg c, double[] adBase, float[] q)
            {
                double[] ad = (double[])adBase.Clone();
                for (int k = 0; k < 3; k++) kin.SagFc_Set(ad, c.PosIDs[k], q[k]);
                kin.SagFc_Set(ad, c.WristID, q[3]);
                return ad;
            }
            // 관절 합을 정한 IK (계획 기구학, 관절값 사본 — 어느 스레드에서도 된다). 손목 원점 목표 = TCP − 공구 길이 × 공구 방향(관절 합 fSum, 요 = atan2(y, x)),
            //   공구 각(수평 아래) φ = fSum − (downsum − 90) — 정의의 수직 관절 합 downsum 에서 공구가 −Z (정의 확인 V6·V7 이 이 모형을 3D 로 확인한다, OMX 는 φ = fSum),
            //   위치 관절 셋을 DLS 로 그 점에 (SagFc_SolveWrist — 0.2mm), 손목 = fSum − q12 − q13. 시드는 차례로 adSeed 의 자세(요만 목표 요로) → 계획 시드 (요, 0, 40)
            //   → 굽힘 시드 (요, −30, 60) — 앞 시드로 풀리면 그것 (이어지는 해). 관절 한계(복제 기구학 CalcLimit)에 걸리거나 FK TCP 가 0.5mm 넘게 어긋나면 그 시드는 버린다.
            //   반환 = 위치 3개 + 손목 (deg), 못 풀면 null. fBend = 팔꿈치 굽힘 (SagArm_BendOf)
            private static float[] SagArm_IkSum(C3d kin, CSagArmCfg c, double[] adSeed, float x, float y, float z, float fSum, out float fBend)
            {
                fBend = float.NaN;
                if (kin == null || adSeed == null || c == null || !c.IdsOk) return null;
                if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z) || float.IsNaN(fSum) || float.IsInfinity(x) || float.IsInfinity(y) || float.IsInfinity(z)) return null;
                float tx, ty, tz, wx, wy, wz;
                kin.SagFc_Fk(c.FuncNumber, adSeed, -1, out tx, out ty, out tz);
                kin.SagFc_Fk(c.FuncNumber, adSeed, c.WristID, out wx, out wy, out wz);
                double dLen = Math.Sqrt((double)(tx - wx) * (tx - wx) + (double)(ty - wy) * (ty - wy) + (double)(tz - wz) * (tz - wz));   // 손목 원점 → TCP (자세와 무관한 상수)
                double yaw = Math.Atan2(y, x), phi = ((double)fSum - ((double)c.DownSumDeg - 90.0)) * Math.PI / 180.0;   // downsum 90 이면 예전 fSum 그대로 (− 0.0)
                double dWx = x - Math.Cos(phi) * Math.Cos(yaw) * dLen, dWy = y - Math.Cos(phi) * Math.Sin(yaw) * dLen, dWz = z + Math.Sin(phi) * dLen;
                double yawDeg = yaw * 180.0 / Math.PI;
                double[][] seeds = { new double[] { yawDeg, adSeed[c.PosIDs[1]], adSeed[c.PosIDs[2]] }, new double[] { yawDeg, 0, 40 }, new double[] { yawDeg, -30, 60 } };
                foreach (double[] s in seeds)
                {
                    double[] q = (double[])s.Clone();
                    double[] a2 = (double[])adSeed.Clone();
                    if (!kin.SagFc_SolveWrist(c.FuncNumber, a2, c.WristID, c.PosIDs, q, dWx, dWy, dWz)) continue;
                    float[] r = { (float)q[0], (float)q[1], (float)q[2], (float)(fSum - q[1] - q[2]) };
                    bool bLim = false;
                    for (int k = 0; k < 4; k++)
                    {
                        int id = k < 3 ? c.PosIDs[k] : c.WristID;
                        kin.SagFc_Set(a2, id, r[k]);
                        if (Math.Abs(a2[id] - r[k]) > 0.01) bLim = true;                   // 관절 한계에 걸림
                    }
                    if (bLim) continue;
                    float fx, fy, fz;
                    kin.SagFc_Fk(c.FuncNumber, a2, -1, out fx, out fy, out fz);
                    double e = Math.Sqrt((double)(fx - x) * (fx - x) + (double)(fy - y) * (fy - y) + (double)(fz - z) * (fz - z));
                    if (double.IsNaN(e) || e > 0.5) continue;
                    fBend = SagArm_BendOf(kin, c, a2);
                    return r;
                }
                return null;
            }
            // 팔꿈치 굽힘 (deg, 0 = 다 폈음 — 리치 경계) — 어깨(위치 관절 둘째)·팔꿈치(셋째)·손목 원점이 이루는 삼각형의 팔꿈치 각의 보각 (관절값 사본 FK)
            private static float SagArm_BendOf(C3d kin, CSagArmCfg c, double[] ad)
            {
                float sx, sy, sz, ex, ey, ez, wx, wy, wz;
                kin.SagFc_Fk(c.FuncNumber, ad, c.PosIDs[1], out sx, out sy, out sz);
                kin.SagFc_Fk(c.FuncNumber, ad, c.PosIDs[2], out ex, out ey, out ez);
                kin.SagFc_Fk(c.FuncNumber, ad, c.WristID, out wx, out wy, out wz);
                double a = Math.Sqrt((double)(sx - ex) * (sx - ex) + (double)(sy - ey) * (sy - ey) + (double)(sz - ez) * (sz - ez));
                double b = Math.Sqrt((double)(ex - wx) * (ex - wx) + (double)(ey - wy) * (ey - wy) + (double)(ez - wz) * (ez - wz));
                double d = Math.Sqrt((double)(sx - wx) * (sx - wx) + (double)(sy - wy) * (sy - wy) + (double)(sz - wz) * (sz - wz));
                if (a < 1e-6 || b < 1e-6) return float.NaN;
                double ci = (a * a + b * b - d * d) / (2 * a * b);
                if (ci > 1) ci = 1; if (ci < -1) ci = -1;
                return (float)(180.0 - Math.Acos(ci) * 180.0 / Math.PI);
            }
            // SafeZ 가 옮기는 높이의 아래 한계(zLow)보다 낮다 (그 점은 옮길 높이가 없다 — 이유를 따로 알린다). NaN 이면 true
            private static bool SagRun_SafeZBelow(CSagArmCfg arm, float zLow)
            {
                return !(arm.SafeZ >= zLow - 1e-3f) || float.IsInfinity(arm.SafeZ);
            }

            /// <summary>옮기는 높이 (실물 이동의 SafeZ 를 점마다) — (x, y) 위 SafeZ 에서 리치 여유를 두고 IK 가 되면 SafeZ, 아니면 5mm 씩 내려 zLow 까지 처음으로 되는 높이.
            /// 된다 = 피치 90 과 (pitch 가 90 이 아니면) 그 피치 둘 다 관절 합을 정한 IK 로 풀리고(피치를 늦추지 않는다, 손목 해는 관절 합 180 − 피치 쪽)
            /// 팔꿈치 굽힘이 20° 이상 (리치 경계에서 떨어짐 — r260 → 90, (250, 50) → 105). 없으면 NaN — SafeZ 가 zLow 보다 낮을 때, zLow 가 NaN 일 때도
            /// (zLow = 지나면 안 되는 높이: 측정 창 위 = 시작 + 10, 틈 재기 = 펜 높이 + 5). 복제 기구학·관절값 사본 — 어느 스레드에서도 된다. 예외 없음.</summary>
            public float SagRun_TransferHeight(CSagArmCfg arm, float x, float y, float pitch, float zLow)
            {
                using (SagRun_KinUse()) return SagRun_TransferHeightBody(arm, x, y, pitch, zLow);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private float SagRun_TransferHeightBody(CSagArmCfg arm, float x, float y, float pitch, float zLow)
            {
                try
                {
                    if (arm == null) arm = new CSagArmCfg();
                    C3d kin = arm.IdsOk ? SagRun_Kin() : null;
                    if (kin == null) return float.NaN;
                    double[] ad = SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), Math.Min(90f, pitch));
                    return SagRun_TransferZ(kin, arm, ad, x, y, pitch, zLow);
                }
                catch { return float.NaN; }
            }
            // 첫닿음 모델 계획 (SagComp_FcPlan) — 복제 기구학의 관절값을 시드 (요, 0, 40, 손목 = 관절 합 180 − 피치) 로 맞춘 뒤.
            //   점마다 같은 시드라 행 순서와 무관하다. 예측 자세의 손목 해가 관절 합 180 − 피치 쪽이 아니면 실패 (why). qSum = 예측 자세의 관절 합
            private bool SagRun_FcPlanSeeded(C3d kin, CSagComp m, CSagArmCfg arm, float fYaw, float x, float y, float pitch, out double pred, out double std, out float qSum, out string why)
            {
                pred = std = double.NaN; qSum = float.NaN; why = "";
                var ci = CultureInfo.InvariantCulture;
                if (pitch > 90.01f) { why = string.Format(ci, "피치 {0:0.#} 는 IK 로 못 감 (피치 ≤ 90 — 관절 합 {0:0.#} 은 피치 {1:0.#})", pitch, SagArm_SumMirror(arm, pitch)); return false; }
                lock (m_oSagRunKin)
                {
                    kin.SetData(arm.PosIDs[0], fYaw); kin.SetData(arm.PosIDs[1], 0f); kin.SetData(arm.PosIDs[2], 40f); kin.SetData(arm.WristID, SagRun_SeedWrist(arm, pitch));
                    float zs, zm;
                    if (!kin.SagComp_FcPlan(m, x, y, pitch, out zs, out zm, out pred, out std)) { why = "첫닿음 예측 실패 (IK 불가)"; return false; }
                    float[] q = kin.SagFc_CalcInvTool(arm.FuncNumber, kin.SagFc_MotCopy(), x, y, (float)pred, pitch, arm.WristID, arm.PosIDs);   // 예측 자세 (같은 시드)
                    if (q == null) { why = "첫닿음 예측 실패 (IK 불가)"; return false; }
                    qSum = q[1] + q[2] + q[3];
                    if (pitch < 89.99f && Math.Abs(qSum - SagArm_SumFor(arm, pitch)) > 1.0)
                    { why = string.Format(ci, "다른 손목 해 (관절 합 {0:F0} — {1:F0} 쪽은 못 감)", qSum, SagArm_SumFor(arm, pitch)); return false; }
                    return true;
                }
            }
            // 점 (x, y, z, 피치) 의 IK 자세 (시드 → 옮기는 높이 → z, 실제 이동 순서) 에서 첫닿음 모델이 얹을 dz.
            //   zT = 옮기는 높이 (z + 5 아래로는 내리지 않는다 — NaN = 없음, SafeZ 가 z + 5 보다 낮을 때도)
            private static bool SagRun_DzAt(C3d kin, CSagComp m, CSagArmCfg arm, float x, float y, float z, float pitch,
                                            out float[] q, out float zT, out double dz, out double std)
            {
                q = null; dz = std = double.NaN;
                double[] ad = SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), pitch);
                zT = SagRun_TransferZ(kin, arm, ad, x, y, pitch, z + 5);
                q = SagRun_Ik(kin, arm, ad, x, y, z, pitch);
                if (q == null) return false;
                float[] info;
                dz = kin.SagComp_FcCorrection(m, q, out std, out info);
                return true;
            }
            // 모델과 설정의 관절 구성이 같은가 (IK 자세를 모델에 그대로 넣는다)
            private static bool SagRun_SameIds(CSagComp m, CSagArmCfg arm)
            {
                if (m == null || arm == null || m.PosIDs == null || arm.PosIDs == null || m.PosIDs.Length != 3 || arm.PosIDs.Length != 3) return false;
                for (int k = 0; k < 3; k++) if (m.PosIDs[k] != arm.PosIDs[k]) return false;
                return m.WristID == arm.WristID && m.FuncNumber == arm.FuncNumber;
            }
            // 모델과 설정의 로봇 정의가 같은가 (10-04) — 관절 구성(SameIds) + 수직 관절 합(모델 downsum ↔ 정의 downsum, ±0.5°) + 기구학 표시
            //   (모델 kinsig ↔ 지금 3D 의 그 수식 — 빈 값 = 내장 OMX). why = 다른 것 (쉬운 말)
            private bool SagRun_SameDef(CSagComp m, CSagArmCfg arm, out string why)
            {
                why = "";
                var ci = CultureInfo.InvariantCulture;
                if (!SagRun_SameIds(m, arm)) { why = "관절 구성(func·posids·wrist)"; return false; }
                if (!(Math.Abs(m.DownSumDeg - arm.DownSumDeg) <= SAGDEF_TOL_DEG))
                { why = string.Format(ci, "수직 기준 (모델 {0:0.###} → 정의 {1:0.###})", m.DownSumDeg, arm.DownSumDeg); return false; }
                CSagRunKin st = SagRun_KinState();
                if (st == null) { why = SAGRUN_NO_KIN; return false; }
                string now = SagRun_KinSig(st, arm.FuncNumber), mk = SagRun_KinSigNorm(m.KinSig);
                if (now.Length == 0 || !string.Equals(now, mk, StringComparison.Ordinal))
                { why = "기구학 (모델 " + (mk.Length > 0 ? mk : "?") + " ≠ 지금 3D " + (now.Length > 0 ? now : "없음") + " — 다른 DH 로 학습)"; return false; }
                return true;
            }

            /// <summary>로봇 정의와 3D 확인 — SagArm_CheckKinematics(cfg, bFull, out why, out kind) 와 같고 실패 종류를 버린다</summary>
            public bool SagArm_CheckKinematics(CSagArmCfg cfg, bool bFull, out string why)
            {
                ESagDefFail kind;
                return SagArm_CheckKinematics(cfg, bFull, out why, out kind);
            }
            /// <summary>로봇 정의와 3D 확인 (작업 입구·앱의 탭 상태) — ① 정의 확인 SagDef_Check (V0~V9 — 공구 끝 체인·꺾는 손목·바닥으로 수직, 캐시) → 실패면 그 까닭·종류
            /// ② 이 3D 와 계획 기구학의 TCP 를 같은 관절각에서 비교 (0.5mm 안이면 true, 아니면 kind = Model): bFull = 지금 자세 + 세 자세 (0,0,40,50), (20,30,−20,80),
            /// (−30,10,10,70), 아니면 지금 자세만. 관절값 사본 FK 로 계산해 3D 관절값을 바꾸지 않는다 — 어느 스레드에서 불러도 된다.
            /// 계획 기구학은 지금 3D 의 수식 글로 만들므로 OMX 가 아닌 로봇도 정의가 맞으면 통과한다.</summary>
            public bool SagArm_CheckKinematics(CSagArmCfg cfg, bool bFull, out string why, out ESagDefFail kind)
            {
                using (SagRun_KinUse()) return SagArm_CheckKinematicsBody(cfg, bFull, out why, out kind);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private bool SagArm_CheckKinematicsBody(CSagArmCfg cfg, bool bFull, out string why, out ESagDefFail kind)
            {
                why = ""; kind = ESagDefFail.None;
                try
                {
                    if (cfg == null) cfg = new CSagArmCfg();
                    if (!cfg.IdsOk) { why = "설정의 관절 ID 가 맞지 않음 (위치 3개 + 손목)"; kind = ESagDefFail.Def; return false; }
                    CSagDefCheck chk = SagDef_Check(cfg);
                    if (!chk.Ok) { why = chk.Why; kind = chk.Kind; return false; }
                    C3d kin = SagRun_Kin();
                    if (kin == null) { why = SAGRUN_NO_KIN; kind = ESagDefFail.NoKin; return false; }
                    int[] ids = cfg.MoveIDs;
                    var poses = new List<float[]>();
                    float[] cur = new float[4]; for (int i = 0; i < 4; i++) cur[i] = GetData(ids[i]);
                    poses.Add(cur);
                    if (bFull) { poses.Add(new float[] { 0, 0, 40, 50 }); poses.Add(new float[] { 20, 30, -20, 80 }); poses.Add(new float[] { -30, 10, 10, 70 }); }
                    double[] adMain = SagFc_MotCopy(), adKin = kin.SagFc_MotCopy();
                    double worst = 0;
                    foreach (float[] p in poses)
                    {
                        double[] a1 = (double[])adMain.Clone(), a2 = (double[])adKin.Clone();
                        for (int i = 0; i < 4; i++) { a1[ids[i]] = p[i]; a2[ids[i]] = p[i]; }   // 관절 한계 없이 DH 만 비교
                        float x1, y1, z1, x2, y2, z2;
                        SagFc_Fk(cfg.FuncNumber, a1, -1, out x1, out y1, out z1);
                        kin.SagFc_Fk(cfg.FuncNumber, a2, -1, out x2, out y2, out z2);
                        double d = Math.Sqrt((double)(x1 - x2) * (x1 - x2) + (double)(y1 - y2) * (y1 - y2) + (double)(z1 - z2) * (z1 - z2));
                        if (double.IsNaN(d) || double.IsInfinity(d)) d = double.MaxValue;
                        worst = Math.Max(worst, d);
                    }
                    if (worst <= 0.5) return true;
                    why = string.Format(CultureInfo.InvariantCulture, "3D 와 계획 기구학의 공구 끝이 다름 — TCP 차 {0}",   // 안내(탭 다시 열기)는 앱이 종류별로 붙인다
                        worst >= 1e6 ? "계산 불가" : worst.ToString("F1", CultureInfo.InvariantCulture) + "mm");
                    kind = ESagDefFail.Model;
                    return false;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); kind = ESagDefFail.Def; return false; }
            }

            /// <summary>표시용 — "11 -1.2° / 12 49.5° / …" (ID 순)</summary>
            public static string SagArm_PoseText(Dictionary<int, float> pose)
            {
                if (pose == null || pose.Count == 0) return "(없음)";
                var ids = new List<int>(pose.Keys); ids.Sort();
                var parts = new List<string>();
                foreach (int id in ids) parts.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1:F1}°", id, pose[id]));
                return string.Join(" / ", parts.ToArray());
            }

            // ────────────────────────────────────────────────────────────
            // ② 측정 계획 — 첫닿음 모델 예측 창
            // ────────────────────────────────────────────────────────────

            /// <summary>측정 계획 — 세트·피치·요·반경(먼 쪽부터) 순으로 점을 만들고, 점마다 첫닿음 모델로 하강 창을 정한다:
            /// SagComp_FcPlan 의 예측 pred → 시작 = pred + Above(20), 한도 = max(ZMinFloor(3), pred − Below(12)).
            /// 예측 표준편차가 모델 한계(StdLimit)를 넘으면 Why = "넘겨짚기 (std n.n)" 표시만. 끝에 SagRun_CheckPlan (IK 확인).
            /// fcModel = 첫닿음 모델이 있는 모델 (고른 결과 파일 → 없으면 적용 중 파일). 없으면 빈 목록 + 이유.
            /// fRetractMm = 꺾임점 위 복귀 (IK 확인에 쓴다). 예외 없음.</summary>
            public List<CFloorPlanRow> SagRun_MakePlan(CSagComp fcModel, CFloorPlanCfg cfg, CSagArmCfg arm, float fRetractMm, out string report)
            {
                using (SagRun_KinUse()) return SagRun_MakePlanBody(fcModel, cfg, arm, fRetractMm, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private List<CFloorPlanRow> SagRun_MakePlanBody(CSagComp fcModel, CFloorPlanCfg cfg, CSagArmCfg arm, float fRetractMm, out string report)
            {
                report = "";
                var plan = new List<CFloorPlanRow>();
                try
                {
                    if (fcModel == null || !fcModel.HasFc) { report = "첫닿음 모델 없음 — ④ 학습 먼저 (floor_dataset_fc.csv)"; return plan; }
                    if (cfg == null) cfg = new CFloorPlanCfg();
                    if (arm == null) arm = new CSagArmCfg();
                    string wd = "관절 구성(func·posids·wrist)", noteDef;
                    if (!SagDef_Gate(arm, true, null, out noteDef)) { report = "★로봇 정의: " + noteDef; return plan; }   // 정의 확인 (3D 에 수식이 없으면 내장 OMX 로 — ★ 알림)
                    if (!arm.IdsOk || !SagRun_SameDef(fcModel, arm, out wd)) { report = "모델과 설정의 로봇 정의가 다름 — " + wd; return plan; }
                    C3d kin = SagRun_Kin();
                    if (kin == null) { report = SAGRUN_NO_KIN; return plan; }
                    var ci = CultureInfo.InvariantCulture;
                    double stdLim = fcModel.Fc.StdLimit;
                    int nFail = 0, nExtrap = 0;
                    var perSet = new List<string>();
                    string strFrom = fcModel.SourcePath ?? "";                // 계획을 만든 첫닿음 모델 파일 — 행마다 적는다 (측정 때 그 모델로 창을 다시 확인)
                    foreach (CFloorPlanSet st in cfg.Sets ?? new List<CFloorPlanSet>())
                    {
                        if (st == null || st.Pitches == null || st.Yaws == null) continue;
                        int n0 = plan.Count;
                        foreach (float p in st.Pitches)
                            foreach (float yaw in st.Yaws)
                                foreach (float r in SagRun_Radii(st))
                                {
                                    double a = yaw * Math.PI / 180.0;
                                    var row = new CFloorPlanRow { Set = st.Name, Pitch = p, Yaw = yaw, R = r, X = (float)(r * Math.Cos(a)), Y = (float)(r * Math.Sin(a)), Src = "fc", FcModel = strFrom,
                                                                  BelowMm = st.BelowMm, ZMinFloor = st.ZMinFloor };
                                    double pred, std; float qs; string whyFc;
                                    if (!SagRun_FcPlanSeeded(kin, fcModel, arm, yaw, row.X, row.Y, p, out pred, out std, out qs, out whyFc))
                                    {
                                        row.ZStart = row.ZMin = float.NaN; row.IkOk = false; row.Why = whyFc; nFail++;
                                    }
                                    else
                                    {
                                        row.PredZ = (float)pred; row.Std = (float)std; row.QSum = qs;
                                        row.ZStart = (float)(pred + st.AboveMm);
                                        row.ZMin = (float)Math.Max(st.ZMinFloor, pred - st.BelowMm);
                                        if (std > stdLim) { row.Why = string.Format(ci, "넘겨짚기 (std {0:F1})", std); nExtrap++; }
                                    }
                                    plan.Add(row);
                                }
                        perSet.Add(string.Format(ci, "{0} {1}", st.Name, plan.Count - n0));
                    }
                    if (plan.Count == 0) { report = "계획 세트가 비었음"; return plan; }
                    string repIk;
                    CSagArmCfg armChk = arm.Clone(); armChk.PlanSets = cfg.Sets;   // 하강 한도는 이 계획을 만든 세트로 (설정의 세트는 나중 확인이 본다 — CSagArmCfg.PlanSets)
                    int nOk = SagRun_CheckPlanCore(plan, armChk, fRetractMm, true, null, false, false, out repIk);   // 예측은 방금 이 모델로 냈다 — 다시 내지 않는다
                    var lines = new List<string>(); foreach (CFloorPlanRow r in plan) if (r.IkOk && !lines.Contains(r.LineKey)) lines.Add(r.LineKey);
                    var probe = new CSagMeasureCfg().Probe; probe.RetractAboveKneeMm = fRetractMm;
                    double sec = SagRun_EstimateSec(plan, probe, arm);
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format(ci, "측정 계획 (첫닿음 모델 {0}점, Ref {1:F1}): {2}행 ({3}) — 측정 {4}행, {5}줄, 예상 약 {6:F0}분",
                        fcModel.Fc.Count, fcModel.Fc.Ref, plan.Count, string.Join(", ", perSet.ToArray()), nOk, lines.Count, sec / 60.0));
                    sb.AppendLine("  창: 시작 = 예측 첫닿음 + 위, 한도 = max(하한, 예측 − 아래) (세트별 값, 기본 20 / 12 / 3)");
                    if (noteDef.Length > 0) sb.AppendLine("  " + noteDef);
                    if (nFail > 0) sb.AppendLine(string.Format(ci, "  첫닿음 예측 못 함 {0}행 (IK 불가 또는 관절 합 {1:0.#} + (90 − 피치) 쪽 손목 해 없음)", nFail, arm.DownSumDeg));
                    if (nExtrap > 0) sb.AppendLine(string.Format(ci, "  넘겨짚기 (예측 std > {0:F1}mm) {1}행 — 재 보고 다시 학습", stdLim, nExtrap));
                    var low = new List<string>();
                    foreach (CFloorPlanRow r in plan)
                        if (r.IkOk && r.TransferZ < arm.SafeZ - 0.01f) low.Add(string.Format(ci, "{0} r{1:F0} → z {2:F0}", r.LineKey, r.R, r.TransferZ));
                    if (low.Count > 0) sb.AppendLine(string.Format(ci, "  SafeZ {0:F0} 에서 팔이 닿지 않아 낮게 옮기는 점 {1}행: {2}", arm.SafeZ, low.Count, string.Join(", ", low.ToArray())));
                    sb.Append(repIk);
                    report = sb.ToString();
                }
                catch (Exception ex) { report = "★오류: " + SagRun_Msg(ex); }
                return plan;
            }
            // 세트의 반경 (먼 쪽부터, 끝 포함)
            private static List<float> SagRun_Radii(CFloorPlanSet st)
            {
                var l = new List<float>();
                float hi = Math.Max(st.RFar, st.RNear), lo = Math.Min(st.RFar, st.RNear);
                if (!(st.RStep > 0)) { l.Add(hi); return l; }
                for (int i = 0; i < 1000; i++) { float r = hi - i * st.RStep; if (r < lo - 1e-3f) break; l.Add(r); }
                return l;
            }

            /// <summary>계획 IK 확인 (복제 기구학, 관절값 사본). 처음 시드 (첫 행 요, 0, 40, 50) — 행마다 차례로 (성공하면 그 해가 다음 시드):
            /// (x,y,옮기는 높이)@90 그리고 @행 피치 (피치가 90 이 아니면 — 피치는 옮기는 높이에서 바꾼다) → zs+8, zs, (zs+zm)/2, zm @행 피치 → zs+fRetractMm.
            /// 옮기는 높이 = SafeZ, 리치 여유(팔꿈치 굽힘 ≥ 20°)가 없는 먼 점(r 260 → 90)은 5mm 씩 내려 zs+10 까지 처음으로 되는 높이
            /// (TransferZ — 실물 이동도 이 높이로, SagRun_TransferZ: 관절 합을 정한 IK, 피치를 늦추지 않는다).
            /// 옮기는 높이는 창 위(zs+10) 아래로 내리지 않는다 — SafeZ 가 그보다 낮으면 IkOk = false, Why "IK: 옮길 높이(SafeZ n)가 창 위 m 보다 낮음".
            /// 복귀 높이는 PlayXyz 처럼 피치를 30° 까지 늦춰도 된다 (공중). 창 안(zs+8 ~ zm)은 행 피치 그대로, 풀린 해의 관절 합이 그 피치의 관절 합
            /// (90, 피치가 90 이 아니면 180 − 피치) 에서 1° 안이어야 한다 (QSum 에 zs 자세의 관절 합).
            /// IK 앞에 (불러온·손으로 고친 계획도 — 10-03 검토): 자리 (X, Y) 가 반경·요의 자리와 0.5mm 안이어야 한다 (누름은 반경·요로, 옮기기는 X·Y 로 간다),
            /// 창 시작 ≥ 바닥 예상 높이 + ABOVE_MIN (창 시작까지는 누름 감시 없이 내려간다) — 바닥 예상 높이 = 행의 예측 첫닿음(PredZ)과 행에 적힌
            /// 첫닿음 모델 파일(FcModel — ② 계획 만들기가 적는다)로 다시 낸 예측 중 높은 쪽 (계획 파일의 pred 칸을 창과 함께 낮춰도 지나가지 않는다).
            /// 하강 한도(zm)도 본다 (10-03 검토): zm ≥ max(하한, 바닥 예상 − 아래) − 1mm — 아래·하한은 계획을 만든 세트의 값 (행의 BelowMm·ZMinFloor,
            /// 없으면 기본 12·3 — arm.PlanSets(설정의 계획 세트)가 있으면 그 세트 값보다 깊게 보지 않는다), 바닥 예상은 행의 예측과 계획을 만든 모델로
            /// 다시 낸 예측 중 높은 쪽 (그 모델을 못 쓰면 창 시작과 같은 바닥 예상) — 손으로 깊게 고친 한도는 누름 감시만 믿고 바닥을 더 누르게 된다.
            /// 하나라도 안 되면 IkOk = false, Why 에 "IK: …" (조용히 버리지 않고 표시 — 측정은 IkOk 행만). 창이 없는 행(예측 실패)도 IkOk = false.
            /// IkOk = false 로 들어온 행(계획 파일의 ik=0 — 손으로 뺐거나 계획 만들기가 뺀 행, 앞 확인이 뺀 행)은 다시 넣지 않는다 (10-04 — 측정·미리보기가
            /// 화면이 보인 점 수보다 많이 누르지 않게). 반환 = IkOk 행 수. 예외 없음.</summary>
            public int SagRun_CheckPlan(List<CFloorPlanRow> plan, CSagArmCfg arm, float fRetractMm, out string report)
            {
                return SagRun_CheckPlan(plan, arm, fRetractMm, true, out report);
            }
            /// <summary>SagRun_CheckPlan 과 같고 bNeedPred 를 준다 — true (4인자 판의 기본) 면 창은 있는데 예측 첫닿음(PredZ)이 없는 행도 뺀다 (IkOk = false,
            /// Why "예측 없는 행 — ② 계획 만들기로 다시 …" — 창이 없는 행·피치 &gt; 90 행·자리가 반경·요와 다른 행은 그 까닭으로 먼저 빠진다):
            /// 창 시작이 예측 첫닿음 + ABOVE_MIN 위인지 확인할 수 없는 행(예측 칸이 없는 예전 8칸 계획·손으로 고친 계획)은
            /// 그 창 시작까지 누름 감시 없이 그냥 내려가다 바닥에 닿을 수 있다 (10-03 검토: 8칸 계획의 zs 30 행 → 바닥 아래 15mm). ② 계획 만들기로 다시 만들면
            /// 예측이 들어간다. false 면 예측 없는 행도 IK 로만 본다 (보고서에 그 수를 적는다 — 측정 작업은 그래도 재지 않는다).
            /// 첫닿음 다시 예측은 행에 적힌 모델 파일로만 한다 (적혀 있지 않으면 계획의 예측만 — 측정 작업은 적용 중 파일의 첫닿음 모델도 쓰고,
            /// 다시 예측할 모델이 하나도 없으면 재지 않는다: fcModel 을 받는 판)</summary>
            public int SagRun_CheckPlan(List<CFloorPlanRow> plan, CSagArmCfg arm, float fRetractMm, bool bNeedPred, out string report)
            {
                return SagRun_CheckPlanCore(plan, arm, fRetractMm, bNeedPred, null, true, false, out report);
            }
            /// <summary>측정 작업과 같은 계획 확인 — SagRun_CheckPlan(bNeedPred) 과 같고 첫닿음을 fcModel(첫닿음 모델이 있는 모델 — 적용 중 파일 등, null·첫닿음 모델
            /// 없음이면 뺀다)과 행에 적힌 모델 파일(FcModel) 모두로 다시 예측한다 (바닥 예상 높이 = 계획의 예측과 다시 낸 예측 중 가장 높은 것).
            /// 다시 예측할 모델이 하나도 없으면 (fcModel 이 없고 행에 적힌 파일도 없음·못 읽음·첫닿음 모델 없음) 창이 있는 행을 모두 뺀다
            /// — 창 시작이 바닥 위인지 다시 확인할 수 없다. 모델의 관절 구성(func·posids·wrist)이 설정과 다르면 그 모델은 쓰지 않는다.
            /// 반환 = IkOk 행 수. 예외 없음.</summary>
            public int SagRun_CheckPlan(List<CFloorPlanRow> plan, CSagArmCfg arm, float fRetractMm, bool bNeedPred, CSagComp fcModel, out string report)
            {
                var given = new List<CSagComp>();
                if (fcModel != null) given.Add(fcModel);
                return SagRun_CheckPlanCore(plan, arm, fRetractMm, bNeedPred, given, true, true, out report);
            }
            // 계획 확인 본체 — fcGiven = 첫닿음을 다시 예측할 모델 (null·빈 목록 = 없음), bRecorded = 행에 적힌 모델 파일(FcModel)도 읽어 쓴다,
            //   bNeedModel = 다시 예측할 모델이 하나도 없으면 창이 있는 행을 뺀다 (측정 작업·SagRun_Measure). 계획 만들기는 예측을 방금 그 모델로 냈으니 다시 내지 않는다
            private int SagRun_CheckPlanCore(List<CFloorPlanRow> plan, CSagArmCfg arm, float fRetractMm, bool bNeedPred, List<CSagComp> fcGiven, bool bRecorded, bool bNeedModel, out string report)
            {
                using (SagRun_KinUse()) return SagRun_CheckPlanCoreBody(plan, arm, fRetractMm, bNeedPred, fcGiven, bRecorded, bNeedModel, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private int SagRun_CheckPlanCoreBody(List<CFloorPlanRow> plan, CSagArmCfg arm, float fRetractMm, bool bNeedPred, List<CSagComp> fcGiven, bool bRecorded, bool bNeedModel, out string report)
            {
                report = "";
                int nOk = 0;
                try
                {
                    if (plan == null || plan.Count == 0) { report = "계획 없음"; return 0; }
                    if (arm == null) arm = new CSagArmCfg();
                    var ci = CultureInfo.InvariantCulture;
                    C3d kin = arm.IdsOk ? SagRun_Kin() : null;
                    if (kin == null)
                    {
                        string w = arm.IdsOk ? SAGRUN_NO_KIN : "설정의 관절 ID 가 맞지 않음";
                        foreach (CFloorPlanRow r in plan) { if (r == null) continue; r.IkOk = false; r.Why = SagRun_WhyJoin("IK 확인 못 함", SagRun_WhyStrip(r.Why, "IK")); }
                        report = w; return 0;
                    }
                    string noteDef;
                    if (!SagDef_Gate(arm, true, null, out noteDef))                 // 로봇 정의 확인 (캐시) — 안 되면 모든 행을 뺀다
                    {
                        foreach (CFloorPlanRow r in plan) { if (r == null) continue; r.IkOk = false; r.Why = SagRun_WhyJoin("IK 확인 못 함 — 로봇 정의", SagRun_WhyStrip(r.Why, "IK")); }
                        report = "★로봇 정의: " + noteDef; return 0;
                    }
                    string repModels;
                    List<CSagComp> fcm = SagRun_PlanFcModels(plan, arm, fcGiven, bRecorded, out repModels);
                    float fYaw0 = 0;
                    foreach (CFloorPlanRow r in plan) if (r != null) { fYaw0 = r.Yaw; break; }
                    double[] ad = SagRun_Seed(kin, arm, fYaw0, 90);
                    var bad = new List<string>();
                    int nLowSafe = 0, nLowWin = 0, nNoPred = 0, nXY = 0, nNoModel = 0, nNoRe = 0, nPredLow = 0, nDeep = 0, nPlanOut = 0;
                    foreach (CFloorPlanRow r in plan)
                    {
                        if (r == null) continue;
                        string rest = SagRun_WhyStrip(SagRun_WhyStrip(SagRun_WhyStrip(SagRun_WhyStrip(r.Why, "IK"), "예측 없는 행"), SAGRUN_WHY_FILE_OUT), SAGRUN_WHY_PLAN_OUT);
                        r.TransferZ = float.NaN;
                        if (float.IsNaN(r.ZStart) || float.IsNaN(r.ZMin) || float.IsInfinity(r.ZStart) || float.IsInfinity(r.ZMin) || r.ZMin > r.ZStart)
                        {
                            r.IkOk = false; r.Why = rest.Length > 0 ? rest : "창 없음";
                            bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); continue;
                        }
                        // 계획에서 이미 빠진 행 (IkOk = false 로 들어옴 — 계획 파일의 ik=0: 손으로 뺐거나 계획 만들기가 뺀 행, 계획 불러오기 확인이 뺀 행)은 다시 넣지 않는다
                        //   (10-04 검토: ② 라벨·③ 확인창은 들어온 IkOk 로 점 수·시간을 세는데 미리보기·측정은 ik=0 행도 다시 확인해 통과하면 눌렀다 — 첫 시험을 2점으로
                        //   줄인 계획이 확인창 '2점, 약 2분' 뒤 90점 약 30분이 됐다). 미리보기·측정은 화면이 보인 수보다 많이 누르지 않는다. 다시 재려면 계획 파일의 ik 를 1 로
                        //   (그 행은 아래 확인을 다시 받는다) 또는 ② 계획 만들기
                        if (!r.IkOk)
                        {
                            r.Why = SagRun_PlanOutWhy(r);
                            nPlanOut++; continue;                                    // 보고서에는 수만 (행 목록은 다른 까닭으로 뺀 행)
                        }
                        if (r.Pitch > 90.01f)
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 피치 {0:0.#} 는 못 감 (피치 ≤ 90 — 관절 합 {0:0.#} 은 피치 {1:0.#})", r.Pitch, SagArm_SumMirror(arm, r.Pitch)), rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} 피치 > 90", r.LineKey, r.R)); continue;
                        }
                        // 자리 (X, Y) 가 반경·요의 자리와 0.5mm 안 — 누름은 반경·요로, 옮기기·IK 확인은 X·Y 로 간다. r 만 고친 행은 X·Y 에서 확인하고
                        //   다른 반경을 그 창으로 누른다 (10-03 검토: r 200 → 150 으로 고친 행이 r 200 의 창으로 r 150 을 눌렀다). 닿기 시험 점 확인과 같은 기준
                        double fYawRad = r.Yaw * Math.PI / 180.0;
                        float xr = (float)(r.R * Math.Cos(fYawRad)), yr = (float)(r.R * Math.Sin(fYawRad));
                        if (!(Math.Abs(r.X - xr) <= 0.5f && Math.Abs(r.Y - yr) <= 0.5f))
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 자리 ({0:F1}, {1:F1}) 가 반경 {2:0.#}·요 {3:0.#} 의 자리 ({4:F1}, {5:F1}) 와 다름 — 계획 파일이 고쳐짐 (② 계획 만들기로 다시)",
                                                                       r.X, r.Y, r.R, r.Yaw, xr, yr), rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); nXY++; continue;
                        }
                        // 창은 있는데 예측 첫닿음이 없는 행 (예측 칸이 없는 예전 8칸 계획·손으로 고친 계획) — 창 시작이 바닥 위인지 확인할 수 없다.
                        //   창이 없는 행(첫닿음 예측 실패 등)은 위에서 그 까닭 그대로 빠진다 (② 를 다시 해도 예측이 생기지 않는다)
                        if (float.IsNaN(r.PredZ) || float.IsInfinity(r.PredZ))
                        {
                            nNoPred++;
                            if (bNeedPred)
                            {
                                r.IkOk = false; r.Why = SagRun_WhyJoin("예측 없는 행 — ② 계획 만들기로 다시 (창 시작이 예측 첫닿음 위인지 확인할 수 없다)", rest);
                                bad.Add(string.Format(ci, "{0} r{1:F0} 예측 없음", r.LineKey, r.R)); continue;
                            }
                        }
                        // 바닥 예상 높이 = 행의 예측과 첫닿음 모델로 다시 낸 예측(행에 적힌 계획을 만든 모델·적용 중 파일 — 있는 것 모두) 중 가장 높은 것.
                        //   계획 파일의 pred 칸만 믿으면 창과 함께 낮춘 손으로 고친 행이 지나간다 (10-03 검토: 예측 45.6 자리를 zs 30.6 / pred 24.6 으로 고친 행을 눌렀다)
                        double rowPred = (float.IsNaN(r.PredZ) || float.IsInfinity(r.PredZ)) ? double.NaN : r.PredZ;
                        double top = rowPred;
                        double pmRec = double.NaN;                                   // 계획을 만든 모델(행에 적힌 파일)로 다시 낸 예측 — 하강 한도 확인
                        string strRec = string.IsNullOrEmpty(r.FcModel) ? "" : r.FcModel.Trim();
                        bool bPredLow = false;
                        if (fcm.Count > 0)
                        {
                            double pmMax = double.NaN;
                            string whyRe = null;
                            foreach (CSagComp mm in fcm)
                            {
                                double pm, sd; float qs; string wf;
                                if (!SagRun_FcPlanSeeded(kin, mm, arm, r.Yaw, xr, yr, r.Pitch, out pm, out sd, out qs, out wf) || double.IsNaN(pm) || double.IsInfinity(pm))
                                { whyRe = string.IsNullOrEmpty(wf) ? "첫닿음 예측 실패" : wf; break; }
                                if (double.IsNaN(pmMax) || pm > pmMax) pmMax = pm;
                                if (strRec.Length > 0 && !string.IsNullOrEmpty(mm.SourcePath) && SagRun_SamePath(mm.SourcePath, strRec) && (double.IsNaN(pmRec) || pm > pmRec)) pmRec = pm;
                            }
                            if (whyRe != null)
                            {
                                r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 첫닿음 모델로 다시 예측 못 함 ({0}) — 창 시작이 바닥 위인지 확인할 수 없다 (② 계획 만들기로 다시)", whyRe), rest);
                                bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); nNoRe++; continue;
                            }
                            bPredLow = !double.IsNaN(top) && pmMax - top > CSagArmSetup.ABOVE_MIN;
                            top = double.IsNaN(top) ? pmMax : Math.Max(top, pmMax);
                        }
                        else if (bNeedModel)
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin("IK: 첫닿음 모델 없음 — 창 시작이 바닥 위인지 다시 확인할 수 없다 (계획을 만든 모델 파일·적용 중 파일 모두 첫닿음 모델 없음)", rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} 첫닿음 모델 없음", r.LineKey, r.R)); nNoModel++; continue;
                        }
                        // 창 시작까지는 그냥 내려간다 (누름 정지는 시작 아래부터) — 바닥 예상 높이 위 ABOVE_MIN(5mm) 은 돼야 한다 (불러온·손으로 고친 계획도)
                        if (!double.IsNaN(top) && r.ZStart < top + CSagArmSetup.ABOVE_MIN - 1e-3)
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 창 시작 z={0:F1} 이 바닥 예상 높이 {1:F1}{2} 위 {3:0}mm 가 안 됨 (누름 감시 없이 내려가다 닿는다) — ② 계획 만들기로 다시",
                                                                       r.ZStart, top, fcm.Count > 0 ? " (첫닿음 모델 다시 예측 포함)" : "", CSagArmSetup.ABOVE_MIN), rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); nLowWin++; continue;
                        }
                        // 하강 한도 zm (10-03 검토: 손으로 고친 zm 이 바닥 예상 아래 60mm 여도 지나가, 무장 뒤에는 부하 25% 안전 정지만으로 멈췄다) —
                        //   계획을 만든 세트의 설계 max(하한, 바닥 예상 − 아래) 보다 1mm 넘게 깊으면 뺀다. 아래·하한 = 행의 BelowMm·ZMinFloor (② 계획 만들기가
                        //   적는다 — 없으면 기본 12·3). 바닥 예상 = 행의 예측과 계획을 만든 모델(행에 적힌 파일)로 다시 낸 예측 중 높은 쪽 (예측 칸을 한도와 함께
                        //   낮춰도 지나가지 않는다). 그 모델을 못 쓰면 창 시작과 같은 바닥 예상 (다시 낸 예측 모두). 다른 모델(적용 중 파일 등)과의 예측 차는
                        //   창 시작 확인이 본다 — 한도까지 그 차로 빼면 다른 결과 파일로 만든 계획을 쓸 수 없다 (누름은 감시된다).
                        //   아래·하한은 설정의 계획 세트(arm.PlanSets — sagarm.txt planX)보다 깊게 보지 않는다 (10-04: 계획 CSV 의 below·zfloor 칸을 zm 과 함께
                        //   깊게 고친 행이 지나갔다 — SagRun_RowWindow).
                        //   닿기 시험 점은 부른 쪽이 모드마다 본다 (LimitByCaller — 끔·켬 한도가 다르다)
                        if (!r.LimitByCaller)
                        {
                            double fl = double.IsNaN(pmRec) ? top : (double.IsNaN(rowPred) ? pmRec : Math.Max(rowPred, pmRec));
                            if (!double.IsNaN(fl))
                            {
                                float below, zfl; string strCap;
                                SagRun_RowWindow(r, arm, out below, out zfl, out strCap);
                                double zmMin = Math.Max(zfl, fl - below);
                                if (r.ZMin < zmMin - SAGRUN_ZM_TOL_MM - 1e-3)
                                {
                                    r.IkOk = false;
                                    r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 하강 한도 z={0:F1} 이 설계 한도 {1:F1} (바닥 예상 {2:F1} − 아래 {3:0.#}, 하한 {4:0.#}{6}) 보다 {5:0.#}mm 넘게 깊음 — 계획보다 깊이 누른다 (② 계획 만들기로 다시)",
                                                                         r.ZMin, zmMin, fl, below, zfl, SAGRUN_ZM_TOL_MM, strCap.Length > 0 ? " — " + strCap : ""), rest);
                                    bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); nDeep++; continue;
                                }
                            }
                        }
                        if (bPredLow) nPredLow++;
                        // 옮기는 높이 (피치 90 으로 옮기고, 피치는 거기서 바꾼다) — 창 위(시작 + 10)보다 낮게 옮기지 않는다
                        float zTop = r.ZStart + 10;
                        if (SagRun_SafeZBelow(arm, zTop))
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 옮길 높이(SafeZ {0:0.#})가 창 위 {1:0.#} 보다 낮음", arm.SafeZ, zTop), rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); nLowSafe++; continue;
                        }
                        // 옮기는 높이 — 피치 90 과 행 피치 모두 리치 여유(팔꿈치 굽힘 ≥ SAGARM_BEND_MIN_DEG)를 두고 풀리는 높이 (SagRun_TransferZ, 10-03)
                        r.TransferZ = SagRun_TransferZ(kin, arm, ad, r.X, r.Y, r.Pitch, zTop);
                        if (float.IsNaN(r.TransferZ))
                        {
                            r.IkOk = false; r.Why = SagRun_WhyJoin(string.Format(ci, "IK: 옮길 높이 없음 (z {0:F0}~{1:F0}, 리치 여유 팔꿈치 {2:0}°)", r.ZStart + 10, arm.SafeZ, SAGARM_BEND_MIN_DEG), rest);
                            bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, r.Why)); continue;
                        }
                        var tz = new List<float[]>();                                // (z, 피치, 늦춤 허용 1/0) — 옮기는 높이는 위에서 확인 (시드 = 그 높이의 피치 90 해)
                        tz.Add(new float[] { r.ZStart + 8, r.Pitch, 0 }); tz.Add(new float[] { r.ZStart, r.Pitch, 0 }); tz.Add(new float[] { (r.ZStart + r.ZMin) / 2, r.Pitch, 0 });
                        tz.Add(new float[] { r.ZMin, r.Pitch, 0 }); tz.Add(new float[] { r.ZStart + fRetractMm, r.Pitch, 1 });
                        string fail = null;
                        for (int ti = 0; ti < tz.Count; ti++)
                        {
                            float[] t = tz[ti]; float pUsed;
                            float[] q = SagRun_Ik(kin, arm, ad, r.X, r.Y, t[0], t[1], t[2] > 0, out pUsed);
                            if (q == null)
                            { fail = string.Format(ci, "IK: z={0:F1}{1}", t[0], Math.Abs(t[1] - 90) > 0.01f ? string.Format(ci, " 피치 {0:F0} (관절 합 {1:F0} 쪽)", t[1], SagArm_SumMirror(arm, t[1])) : ""); break; }
                            if (ti == 1) r.QSum = q[1] + q[2] + q[3];                      // zs 자세
                        }
                        r.IkOk = fail == null;
                        r.Why = r.IkOk ? rest : SagRun_WhyJoin(fail, rest);
                        if (r.IkOk) nOk++; else bad.Add(string.Format(ci, "{0} r{1:F0} {2}", r.LineKey, r.R, fail));
                    }
                    var sb = new StringBuilder();
                    sb.Append(string.Format(ci, "  IK 확인: {0}/{1}행 통과", nOk, plan.Count));
                    if (bad.Count > 0 || nPlanOut > 0)
                    {
                        sb.Append(string.Format(ci, ", 제외 {0}행:", bad.Count + nPlanOut));
                        if (nLowSafe > 0)
                            sb.Append(string.Format(ci, "\r\n  ★SafeZ {0:0.#} 이 창 위(시작 + 10)보다 낮아 뺀 행 {1} — 옮기는 높이는 창 위 아래로 내리지 않는다 (sagarm.txt 의 safez 확인)", arm.SafeZ, nLowSafe));
                        if (nLowWin > 0)
                            sb.Append(string.Format(ci, "\r\n  ★창 시작이 바닥 예상 높이(계획의 예측·첫닿음 모델 다시 예측 중 높은 쪽) + {0:0} 보다 낮아 뺀 행 {1} — 계획 파일을 고쳤거나 그 뒤 모델이 바뀜: ② 계획 만들기로 다시",
                                                    CSagArmSetup.ABOVE_MIN, nLowWin));
                        if (nXY > 0)
                            sb.Append(string.Format(ci, "\r\n  ★자리 (x, y) 가 반경·요의 자리와 다른 행 {0} 뺌 — 계획 파일이 고쳐짐: ② 계획 만들기로 다시", nXY));
                        if (nDeep > 0)
                            sb.Append(string.Format(ci, "\r\n  ★하강 한도가 설계 한도(바닥 예상 − 아래, 하한)보다 {0:0.#}mm 넘게 깊은 행 {1} 뺌 — 계획 파일이 고쳐짐: ② 계획 만들기로 다시", SAGRUN_ZM_TOL_MM, nDeep));
                        if (nNoRe > 0)
                            sb.Append(string.Format(ci, "\r\n  ★첫닿음 모델로 다시 예측하지 못한 행 {0} 뺌 — ② 계획 만들기로 다시", nNoRe));
                        if (nNoModel > 0)
                            sb.Append(string.Format(ci, "\r\n  ★첫닿음 모델이 없어 창을 다시 확인할 수 없는 행 {0} 뺌 — 첫닿음 모델이 있는 결과 파일을 고르고 ② 계획 만들기로 다시 (계획을 만든 모델 파일이 없거나 예전 계획)", nNoModel));
                        if (nNoPred > 0 && bNeedPred)
                            sb.Append(string.Format(ci, "\r\n  ★예측 첫닿음이 없는 행 {0} 뺌 — 창이 바닥 위인지 확인할 수 없다 (예전 계획 파일): ② 계획 만들기로 다시", nNoPred));
                        if (nPlanOut > 0)
                            sb.Append(string.Format(ci, "\r\n  계획에서 빠진 행(ik=0) {0} 은 다시 넣지 않음 — 다시 재려면 계획 파일의 ik 를 1 로 고치거나 ② 계획 만들기로 다시", nPlanOut));
                        for (int i = 0; i < bad.Count && i < 40; i++) sb.Append("\r\n    " + bad[i]);
                        if (bad.Count > 40) sb.Append(string.Format(ci, "\r\n    … 외 {0}행", bad.Count - 40));
                    }
                    if (nNoPred > 0 && !bNeedPred)
                        sb.Append(string.Format(ci, "\r\n  ★예측 첫닿음이 없는 행 {0} — IK 로만 봄, 측정 작업은 이 행을 재지 않는다 (② 계획 만들기로 다시)", nNoPred));
                    if (nPredLow > 0)
                        sb.Append(string.Format(ci, "\r\n  ★계획의 예측 첫닿음이 첫닿음 모델 다시 예측보다 {0:0}mm 넘게 낮은 행 {1} — 창은 높은 쪽으로 확인함 (계획 파일이 고쳐졌거나 다른 모델의 계획 — 측정 보고서의 예측 오차는 계획 값 기준)",
                                                CSagArmSetup.ABOVE_MIN, nPredLow));
                    if (repModels.Length > 0) sb.Append("\r\n" + repModels);
                    if (noteDef.Length > 0) sb.Append("\r\n  " + noteDef);
                    report = sb.ToString();
                }
                catch (Exception ex) { report = "★오류: " + SagRun_Msg(ex); }
                return nOk;
            }
            // 하강 한도 확인의 여유 (mm) — 설계 한도보다 이만큼까지 깊은 것은 받는다 (반올림·칸 소수 둘째 자리)
            private const float SAGRUN_ZM_TOL_MM = 1f;
            // 행의 아래·하한 (계획을 만든 세트의 값 — ② 계획 만들기가 적는다). 없으면 세트 기본값 (12 / 3). 범위는 sagarm.txt 와 같게 아래 [0, 60], 하한 [0, 200]
            private static float SagRun_RowBelow(CFloorPlanRow r) { return SagRun_BelowOf(r.BelowMm); }
            private static float SagRun_RowZFloor(CFloorPlanRow r) { return SagRun_ZFloorOf(r.ZMinFloor); }
            private static float SagRun_BelowOf(float v)
            {
                if (float.IsNaN(v) || float.IsInfinity(v)) return new CFloorPlanSet().BelowMm;
                return Math.Max(0f, Math.Min(60f, v));
            }
            private static float SagRun_ZFloorOf(float v)
            {
                if (float.IsNaN(v) || float.IsInfinity(v)) return new CFloorPlanSet().ZMinFloor;
                return Math.Max(0f, Math.Min(200f, v));
            }
            // 하강 한도 확인의 아래·하한 — 행의 값(SagRun_RowBelow·RowZFloor)을 설정의 계획 세트(arm.PlanSets)보다 깊게 보지 않는다:
            //   아래 = min(행, 세트), 하한 = max(행, 세트). 세트 = 행의 세트 이름과 같은 것 (대소문자 무시), 없으면 세트들 중 가장 얕은 값 (아래 최소·하한 최대).
            //   arm.PlanSets 가 null 이거나 비었으면 행의 값 그대로. cap = 설정 값으로 바꿨으면 그 설명 (까닭 칸), 아니면 ""
            private static void SagRun_RowWindow(CFloorPlanRow r, CSagArmCfg arm, out float below, out float zfl, out string cap)
            {
                below = SagRun_RowBelow(r); zfl = SagRun_RowZFloor(r); cap = "";
                List<CFloorPlanSet> sets = (arm != null) ? arm.PlanSets : null;
                if (sets == null) return;
                var ci = CultureInfo.InvariantCulture;
                CFloorPlanSet same = null;
                float bMin = float.NaN, zMax = float.NaN;
                string strName = (r.Set ?? "").Trim();
                foreach (CFloorPlanSet st in sets)
                {
                    if (st == null) continue;
                    float sb = SagRun_BelowOf(st.BelowMm), sz = SagRun_ZFloorOf(st.ZMinFloor);
                    if (float.IsNaN(bMin) || sb < bMin) bMin = sb;
                    if (float.IsNaN(zMax) || sz > zMax) zMax = sz;
                    if (same == null && string.Equals((st.Name ?? "").Trim(), strName, StringComparison.OrdinalIgnoreCase)) same = st;
                }
                if (float.IsNaN(bMin)) return;
                float cb = (same != null) ? SagRun_BelowOf(same.BelowMm) : bMin;
                float cz = (same != null) ? SagRun_ZFloorOf(same.ZMinFloor) : zMax;
                string strFrom = (same != null) ? "sagarm.txt plan" + same.Name : "sagarm.txt 계획 세트 (같은 이름 " + (strName.Length > 0 ? strName : "?") + " 없음)";
                var parts = new List<string>();
                if (cb < below - 1e-3f) { parts.Add(string.Format(ci, "아래는 {0} 의 {1:0.#} (계획 파일의 below {2:0.#} 대신)", strFrom, cb, below)); below = cb; }
                if (cz > zfl + 1e-3f) { parts.Add(string.Format(ci, "하한은 {0} 의 {1:0.#} (계획 파일의 zfloor {2:0.#} 대신)", strFrom, cz, zfl)); zfl = cz; }
                cap = string.Join(", ", parts.ToArray());
            }
            // 계획에서 이미 빠진 행의 까닭 칸 — 앞 까닭(IK·예측 없음·창 없음·뺀 행)이 적혀 있으면 그대로 (계획 만들기·앞 확인이 뺀 행), 아니면 계획 파일의 ik=0 이면
            //   "계획 파일에서 뺀 행 (ik=0)" 을, 그 밖이면 "계획에서 뺀 행" 을 앞에 (두 번 붙이지 않는다)
            private const string SAGRUN_WHY_FILE_OUT = "계획 파일에서 뺀 행 (ik=0)";
            private const string SAGRUN_WHY_PLAN_OUT = "계획에서 뺀 행";
            private static string SagRun_PlanOutWhy(CFloorPlanRow r)
            {
                string w = (r.Why ?? "").Trim();
                foreach (string s in w.Split(new string[] { " · " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string t = s.Trim();
                    if (t.StartsWith("IK") || t.StartsWith("예측 없는 행") || t == "창 없음" || t.StartsWith(SAGRUN_WHY_FILE_OUT) || t.StartsWith(SAGRUN_WHY_PLAN_OUT)) return w;
                }
                return SagRun_WhyJoin(r.ExcludedByFile ? SAGRUN_WHY_FILE_OUT : SAGRUN_WHY_PLAN_OUT, w);
            }
            // 계획 확인이 첫닿음을 다시 예측할 모델 — 준 모델 + (bRecorded) 행에 적힌 모델 파일 (같은 파일은 한 번 — SourcePath 로 견준다).
            //   첫닿음 모델이 없거나 관절 구성(func·posids·wrist)이 설정과 다르면 뺀다. report = 쓴 모델·못 쓴 파일 (둘 다 없으면 빈칸)
            private List<CSagComp> SagRun_PlanFcModels(List<CFloorPlanRow> plan, CSagArmCfg arm, List<CSagComp> given, bool bRecorded, out string report)
            {
                var ci = CultureInfo.InvariantCulture;
                var cand = new List<CSagComp>(); var bFromPlan = new List<bool>();
                var notes = new List<string>();
                if (given != null) foreach (CSagComp m in given) if (m != null) { cand.Add(m); bFromPlan.Add(false); }
                if (bRecorded && plan != null)
                {
                    var paths = new List<string>();
                    foreach (CFloorPlanRow r in plan)
                    {
                        if (r == null || string.IsNullOrEmpty(r.FcModel) || r.FcModel.Trim().Length == 0) continue;
                        string p = r.FcModel.Trim();
                        bool bDup = false;
                        foreach (string q in paths) if (SagRun_SamePath(q, p)) { bDup = true; break; }
                        if (!bDup) paths.Add(p);
                    }
                    foreach (string p in paths)
                    {
                        bool bHave = false;
                        foreach (CSagComp m in cand) if (!string.IsNullOrEmpty(m.SourcePath) && SagRun_SamePath(m.SourcePath, p)) { bHave = true; break; }
                        if (bHave) continue;
                        CSagComp mm = SagRun_LoadQuiet(p);
                        if (mm == null) { notes.Add("★계획을 만든 첫닿음 모델 파일을 읽지 못함 (없음·잠김): " + p); continue; }
                        cand.Add(mm); bFromPlan.Add(true);
                    }
                }
                var use = new List<CSagComp>(); var names = new List<string>();
                for (int i = 0; i < cand.Count; i++)
                {
                    CSagComp m = cand[i];
                    string nm = string.IsNullOrEmpty(m.SourcePath) ? "(파일 없는 모델)" : Path.GetFileName(m.SourcePath);
                    bool bDup = false;
                    foreach (CSagComp u in use) if (!string.IsNullOrEmpty(m.SourcePath) && !string.IsNullOrEmpty(u.SourcePath) && SagRun_SamePath(m.SourcePath, u.SourcePath)) { bDup = true; break; }
                    if (bDup) continue;
                    if (!m.HasFc) { if (bFromPlan[i]) notes.Add("★계획을 만든 모델 파일에 첫닿음 모델이 없음: " + nm); continue; }
                    string wd;
                    if (!SagRun_SameDef(m, arm, out wd)) { notes.Add("★첫닿음 모델의 로봇 정의가 설정과 다름 (" + wd + ") — 다시 예측에 쓰지 않음: " + nm); continue; }
                    use.Add(m); names.Add(nm + (bFromPlan[i] ? " (계획을 만든 모델)" : ""));
                }
                var sb = new StringBuilder();
                if (use.Count > 0)
                    sb.Append("  첫닿음 다시 예측: " + string.Join(", ", names.ToArray())
                              + string.Format(ci, " — 창 시작 ≥ 바닥 예상 높이(계획의 예측·다시 낸 예측 중 높은 쪽) + {0:0}", CSagArmSetup.ABOVE_MIN));
                foreach (string s in notes) { if (sb.Length > 0) sb.Append("\r\n"); sb.Append("  " + s); }
                report = sb.ToString();
                return use;
            }
            // 경로들의 모델 중 첫닿음 모델이 있는 것 (같은 파일은 한 번, 못 읽으면 뺀다) — 측정 작업의 계획 확인이 다시 예측에 쓴다 (적용 중 파일·측정 모델 파일)
            private static List<CSagComp> SagRun_FcModelsAt(string[] paths)
            {
                var l = new List<CSagComp>();
                var seen = new List<string>();
                if (paths == null) return l;
                foreach (string p in paths)
                {
                    if (string.IsNullOrEmpty(p) || p.Trim().Length == 0) continue;
                    bool bDup = false;
                    foreach (string q in seen) if (SagRun_SamePath(q, p)) { bDup = true; break; }
                    if (bDup) continue;
                    seen.Add(p);
                    CSagComp m = SagRun_LoadQuiet(p);
                    if (m != null && m.HasFc) l.Add(m);
                }
                return l;
            }
            // Why 칸 — " · " 로 이은 조각들. prefix 로 시작하는 조각을 뺀다 / 두 조각 잇기
            private static string SagRun_WhyStrip(string why, string prefix)
            {
                if (string.IsNullOrEmpty(why)) return "";
                var keep = new List<string>();
                foreach (string s in why.Split(new string[] { " · " }, StringSplitOptions.RemoveEmptyEntries)) if (!s.Trim().StartsWith(prefix)) keep.Add(s.Trim());
                return string.Join(" · ", keep.ToArray());
            }
            private static string SagRun_WhyJoin(string a, string b)
            {
                if (string.IsNullOrEmpty(a)) return b ?? "";
                if (string.IsNullOrEmpty(b)) return a;
                return a + " · " + b;
            }

            /// <summary>계획 CSV — 머리줄 set,pitch,yaw,r,x,y,zs,zm,pred,src,ik,why,std,qsum,tz,fcmodel,below,zfloor (앞 8칸은 예전 계획 파일과 같다). why 의 쉼표는 ';',
            /// fcmodel = 계획을 만든 첫닿음 모델 파일 (경로의 '%'·',' 는 %25·%2C), below·zfloor = 계획을 만든 세트의 아래·하한 (하강 한도 확인)</summary>
            public static string SagRun_PlanCsv(List<CFloorPlanRow> plan)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder("set,pitch,yaw,r,x,y,zs,zm,pred,src,ik,why,std,qsum,tz,fcmodel,below,zfloor\r\n");
                if (plan == null) return sb.ToString();
                Func<float, string, string> V = (f, fmt) => (float.IsNaN(f) || float.IsInfinity(f)) ? "" : f.ToString(fmt, ci);
                foreach (CFloorPlanRow r in plan)
                {
                    if (r == null) continue;
                    sb.Append(string.Join(",", new string[] {
                        (r.Set ?? "").Replace(",", ";"), V(r.Pitch, "0.###"), V(r.Yaw, "0.###"), V(r.R, "0.###"), V(r.X, "F2"), V(r.Y, "F2"),
                        V(r.ZStart, "F2"), V(r.ZMin, "F2"), V(r.PredZ, "F2"), (r.Src ?? "").Replace(",", ";"), r.IkOk ? "1" : "0",
                        (r.Why ?? "").Replace(",", ";").Replace("\r", " ").Replace("\n", " "), V(r.Std, "F2"), V(r.QSum, "F1"), V(r.TransferZ, "F1"),
                        SagRun_CsvPathEsc(r.FcModel), V(r.BelowMm, "0.###"), V(r.ZMinFloor, "0.###") }));
                    sb.Append("\r\n");
                }
                return sb.ToString();
            }
            // 계획 CSV 의 경로 칸 — 경로에 쉼표가 있을 수 있어 '%' → %25, ',' → %2C (줄바꿈은 뺀다) / 되돌리기 (모르는 %xx 는 그대로)
            private static string SagRun_CsvPathEsc(string s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                return s.Replace("%", "%25").Replace(",", "%2C").Replace("\r", "").Replace("\n", "");
            }
            private static string SagRun_CsvPathUnesc(string s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                var sb = new StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '%' && i + 3 <= s.Length)
                    {
                        string h = s.Substring(i + 1, 2).ToUpperInvariant();
                        if (h == "2C") { sb.Append(','); i += 2; continue; }
                        if (h == "25") { sb.Append('%'); i += 2; continue; }
                    }
                    sb.Append(s[i]);
                }
                return sb.ToString();
            }

            /// <summary>계획 CSV 읽기 — 머리줄 이름으로 칸을 찾는다 (예전 8칸 파일 set,pitch,yaw,r,x,y,zs,zm 도). x·y 가 없으면 r·yaw 로 계산,
            /// ik 가 없으면 true (ik=0 행은 IkOk = false — 계획 확인이 다시 넣지 않는다), pred·std 가 없으면 NaN, fcmodel 이 없으면 빈칸 (계획을 만든 모델을 모름),
            /// below·zfloor 가 없으면 NaN (세트 기본값으로 본다).
            /// 읽지 못한 행은 건너뛰고 why 에 센다.
            /// 머리줄이 틀리면 null. 읽은 행은 그대로다 — 측정 전에 SagRun_CheckPlan 이 자리·창·IK 를 다시 확인한다. 예외 없음.</summary>
            public static List<CFloorPlanRow> SagRun_PlanParse(string strCsv, out string why)
            {
                why = "";
                try
                {
                    if (string.IsNullOrEmpty(strCsv)) { why = "빈 계획"; return null; }
                    var ci = CultureInfo.InvariantCulture;
                    string[] lines = strCsv.Replace("\r", "").Split('\n');
                    string[] hd = lines[0].TrimStart('﻿').Split(',');
                    for (int i = 0; i < hd.Length; i++) hd[i] = hd[i].Trim().ToLowerInvariant();
                    int iSet = Array.IndexOf(hd, "set"), iP = Array.IndexOf(hd, "pitch"), iYaw = Array.IndexOf(hd, "yaw"), iR = Array.IndexOf(hd, "r"),
                        iX = Array.IndexOf(hd, "x"), iY = Array.IndexOf(hd, "y"), iZs = Array.IndexOf(hd, "zs"), iZm = Array.IndexOf(hd, "zm"),
                        iPred = Array.IndexOf(hd, "pred"), iSrc = Array.IndexOf(hd, "src"), iIk = Array.IndexOf(hd, "ik"), iWhy = Array.IndexOf(hd, "why"), iStd = Array.IndexOf(hd, "std"),
                        iQs = Array.IndexOf(hd, "qsum"), iTz = Array.IndexOf(hd, "tz"), iFcm = Array.IndexOf(hd, "fcmodel"),
                        iBel = Array.IndexOf(hd, "below"), iZf = Array.IndexOf(hd, "zfloor");
                    if (iP < 0 || iYaw < 0 || iR < 0 || iZs < 0 || iZm < 0) { why = "계획 머리줄이 다름 (pitch, yaw, r, zs, zm 칸 필요)"; return null; }
                    Func<string[], int, float> F = (f, k) => { float v; return (k >= 0 && k < f.Length && float.TryParse(f[k].Trim(), NumberStyles.Float, ci, out v)) ? v : float.NaN; };
                    Func<string[], int, string> S = (f, k) => (k >= 0 && k < f.Length) ? f[k].Trim() : "";
                    var plan = new List<CFloorPlanRow>();
                    int nBad = 0;
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (lines[i].Trim().Length == 0) continue;
                        string[] f = lines[i].Split(',');
                        if (S(f, iP).ToLowerInvariant() == "pitch") continue;                  // 이어 붙인 머리줄
                        float p = F(f, iP), yaw = F(f, iYaw), r = F(f, iR), zs = F(f, iZs), zm = F(f, iZm);
                        if (float.IsNaN(p) || float.IsNaN(yaw) || float.IsNaN(r)) { nBad++; continue; }
                        var row = new CFloorPlanRow { Set = iSet >= 0 && S(f, iSet).Length > 0 ? S(f, iSet) : "A", Pitch = p, Yaw = yaw, R = r, ZStart = zs, ZMin = zm };
                        float x = F(f, iX), y = F(f, iY);
                        if (float.IsNaN(x) || float.IsNaN(y)) { double a = yaw * Math.PI / 180.0; x = (float)(r * Math.Cos(a)); y = (float)(r * Math.Sin(a)); }
                        row.X = x; row.Y = y;
                        row.PredZ = F(f, iPred); row.Std = F(f, iStd); row.QSum = F(f, iQs); row.TransferZ = F(f, iTz); row.Src = S(f, iSrc);
                        row.FcModel = SagRun_CsvPathUnesc(S(f, iFcm));
                        row.BelowMm = F(f, iBel); row.ZMinFloor = F(f, iZf);
                        string ik = S(f, iIk); row.IkOk = ik.Length == 0 || ik != "0";
                        row.ExcludedByFile = !row.IkOk;                                     // ik=0 — 계획 확인이 다시 넣지 않는다 (손으로 뺀 행·계획 만들기가 뺀 행)
                        row.Why = S(f, iWhy);
                        if (float.IsNaN(zs) || float.IsNaN(zm)) row.IkOk = false;
                        plan.Add(row);
                    }
                    if (plan.Count == 0) { why = "계획 행 없음" + (nBad > 0 ? string.Format(ci, " (읽지 못한 행 {0})", nBad) : ""); return null; }
                    if (nBad > 0) why = string.Format(ci, "읽지 못한 행 {0} 건너뜀", nBad);
                    return plan;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return null; }
            }

            /// <summary>계획 저장 — <urdf>\sagcomp_runs\plan_<yyyyMMdd_HHmm>.csv (같은 분이면 _2 …, UTF-8 BOM 없음). 반환 = 경로 (실패하면 null —
            /// urdf 폴더가 비었거나 없으면도: 지금 작업 폴더(실행 파일 쪽)에 쓰지 않는다)</summary>
            public static string SagRun_SavePlan(List<CFloorPlanRow> plan, string strUrdfDir)
            {
                try
                {
                    if (string.IsNullOrEmpty(strUrdfDir) || strUrdfDir.Trim().Length == 0 || !Directory.Exists(strUrdfDir)) return null;
                    string dir = SagRun_RunsDir(strUrdfDir);
                    Directory.CreateDirectory(dir);
                    string stem = "plan_" + DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
                    for (int n = 1; n < 1000; n++)
                    {
                        string p = Path.Combine(dir, stem + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : "") + ".csv");
                        if (File.Exists(p)) continue;
                        try { using (var fs = new FileStream(p, FileMode.CreateNew, FileAccess.Write)) { byte[] b = new UTF8Encoding(false).GetBytes(SagRun_PlanCsv(plan)); fs.Write(b, 0, b.Length); } }
                        catch (IOException) { if (File.Exists(p)) continue; throw; }
                        return p;
                    }
                    return null;
                }
                catch { return null; }
            }

            /// <summary>측정 예상 시간 (초) — 행마다 (MoveMs+300+1500+500)/1000 + 0.6·(zs−zm)/하강속도 + 누름 유지 + 들어 올림 + 복귀 + 3,
            /// 줄이 바뀔 때마다 + (옮기는 높이(TransferZ, 없으면 SafeZ) − zm)/옮기기 속도 + 4, 전체 + 15(펼치기) + 13(원위치). IkOk 행만. (09-29 실측 93점 30분과 맞춘 값)</summary>
            public static double SagRun_EstimateSec(List<CFloorPlanRow> plan, CFloorProbeCfg probe, CSagArmCfg arm)
            {
                try
                {
                    if (plan == null) return 0;
                    if (probe == null) probe = new CSagMeasureCfg().Probe;
                    if (arm == null) arm = new CSagArmCfg();
                    double s = 0; string last = null; int n = 0;
                    foreach (CFloorPlanRow r in plan)
                    {
                        if (r == null || !r.IkOk || float.IsNaN(r.ZStart) || float.IsNaN(r.ZMin)) continue;
                        n++;
                        s += (probe.MoveMs + 300 + 1500 + 500) / 1000.0 + 0.6 * (r.ZStart - r.ZMin) / Math.Max(0.5, probe.PressDescentMmPerSec)
                           + probe.PressHoldMs / 1000.0 + probe.LiftMm / Math.Max(0.5, probe.LiftMmPerSec) + probe.RetractMs / 1000.0 + 3;
                        if (r.LineKey != last) { s += Math.Max(0, (float.IsNaN(r.TransferZ) ? arm.SafeZ : r.TransferZ) - r.ZMin) / Math.Max(1.0, arm.TransferMmPerSec) + 4; last = r.LineKey; }
                    }
                    return n > 0 ? s + 15 + 13 : 0;
                }
                catch { return 0; }
            }

            // ────────────────────────────────────────────────────────────
            // ④ 학습 → 결과 파일
            // ────────────────────────────────────────────────────────────

            /// <summary>학습 → 결과 파일. 로봇 없음 — 작업 스레드에서 불러도 된다 (복제 기구학은 미리 SagRun_Prepare).
            /// 1. 바탕 = BaseModelPath(비면 활성 파일)를 새로 읽어 바닥 부분(Gp·보정표·Fc)을 벗긴다. 관절 모델이 없으면 실패.
            /// 2. SagComp_FitFloorFc(데이터셋, 바탕, UseHold) — 실패면 파일을 쓰지 않는다. 3. RefMm 이 숫자면 Ref 고정(fcref).
            /// 4. 첫닿음·보정·바닥 켬, MotorMap = strMotorMapKey (null = motormap 줄을 쓰지 않는다) — 단 바탕 파일의 motormap(없으면 기본 매핑)이 모델 축
            ///    (관절 모델·위치 3개·손목)에서 strMotorMapKey 와 다르면 바탕의 키를 적고 MotorMapWarn·Line 맨 앞·보고서에 ★ (관절 모델은 옛 매핑의 것 — ⑨ 부터 다시).
            ///    값 확인 — 학습 첫닿음 높이 평균 3~300mm, Ref 가 그 평균에서 보정 상한(15mm) 안, β 유한, 교차검증 ≤ 20mm 가 아니면 파일을 쓰지 않고 실패
            ///    (미터로 적힌 데이터셋·엉뚱한 값·fcref 오타). 실패해도 결과의 Ref·CvRmse·Beta 는 채워 둔다.
            /// 5. 예상 표 (복제 기구학, 피치 90): 사각형 점(PreviewZ) dz·std·먼−가까 차, 반경선 r 120~260/10 요 0 의 z 60·75·90 dz 범위·이웃 최대 계단.
            /// 6. 저장 <urdf>\sagcomp_results\sagcomp_model_<yyyyMMdd_HHmm>[_n].txt (Note 머리 주석) → 다시 읽어 첫닿음 모델 준비·예측이 같은지 확인
            ///    (아니면 지우고 실패), _report.txt, _data.csv (쓴 데이터셋 사본). urdf 폴더가 없으면 실패. 예외 없음.</summary>
            public CSagLearnResult SagRun_Learn(CSagLearnCfg cfg, string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log)
            {
                return SagRun_Learn(cfg, strUrdfDir, strMotorMapKey, log, null);
            }
            /// <summary>SagRun_Learn 과 같고 중단 요청(stop)을 받는다 — 학습(초매개변수 교차검증) 중에도 본다. 중단하면 Ok = false, Stopped = true, 파일을 남기지 않는다.
            /// 학습 전에 "진행 학습: 첫닿음 n점 · 묶음 g (교차검증 f겹) — 약 m초" 를 로그로 알리고, 학습 중에도 진행을 10% 마다 알린다
            /// (교차검증은 묶음을 최대 10겹으로 묶어 측정 회차가 늘어도 시간이 묶음 수에 따라 늘지 않는다 — SagComp_FitFloorFc). 블로킹 — 작업 스레드에서</summary>
            public CSagLearnResult SagRun_Learn(CSagLearnCfg cfg, string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_LearnBody(cfg, strUrdfDir, strMotorMapKey, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagLearnResult SagRun_LearnBody(CSagLearnCfg cfg, string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagLearnResult();
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var made = new List<string>();                                   // 실패하면 지울 파일
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    if (cfg == null) cfg = new CSagLearnCfg();
                    if (string.IsNullOrEmpty(strUrdfDir) || !Directory.Exists(strUrdfDir)) return SagRun_LearnFail(res, "urdf 폴더 없음: " + strUrdfDir, L);
                    string dsPath = SagRun_Resolve(strUrdfDir, cfg.DatasetPath, SAGRUN_FC_DATASET);
                    string basePath = SagRun_Resolve(strUrdfDir, cfg.BaseModelPath, SAGRUN_ACTIVE);
                    L("진행 학습: 데이터 " + Path.GetFileName(dsPath) + ", 바탕 " + Path.GetFileName(basePath) + (cfg.UseHold ? ", 손목 유지 켬 행 포함" : ", 손목 유지 켬 행 뺌"));
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagRun_LearnFail(res, SAGRUN_NO_KIN, L);
                    if (!File.Exists(dsPath)) return SagRun_LearnFail(res, "데이터셋 없음: " + dsPath, L);
                    string why, baseMap;
                    CSagComp m = SagRun_LoadBase(basePath, out why, out baseMap);
                    if (m == null) return SagRun_LearnFail(res, why, L);
                    // 로봇 정의 (10-04) — 바탕 모델의 관절 구성·수직 관절 합으로 정의를 확인하고 (3D 에 수식이 없으면 내장 OMX 로 — ★ 로그),
                    //   바탕 모델의 기구학 표시가 지금 3D 의 그 수식과 같아야 한다 (다른 DH 에서 만든 관절 모델에 첫닿음을 얹지 않는다 — 빈 표시 = 내장 OMX)
                    var armDef = new CSagArmCfg { FuncNumber = m.FuncNumber, PosIDs = m.PosIDs == null ? null : (int[])m.PosIDs.Clone(), WristID = m.WristID,
                                                  HoldIDs = new int[0], TcpLenMm = float.NaN, DownSumDeg = m.DownSumDeg };
                    string whyDef;
                    if (!SagDef_Gate(armDef, true, L, out whyDef)) return SagRun_LearnFail(res, "★로봇 정의: " + whyDef, L);
                    string kinNow = SagRun_KinSig(SagRun_KinState(), m.FuncNumber), kinBase = SagRun_KinSigNorm(m.KinSig);
                    if (kinNow.Length == 0 || !string.Equals(kinNow, kinBase, StringComparison.Ordinal))
                        return SagRun_LearnFail(res, "바탕 관절 모델은 다른 기구학(DH)에서 만들어짐 (모델 " + (kinBase.Length > 0 ? kinBase : "?") + " ≠ 지금 3D " + (kinNow.Length > 0 ? kinNow : "없음") + ") — 관절 모델부터 다시", L);
                    string baseSummary = m.Summary();
                    string csv = File.ReadAllText(dsPath);
                    int nBroken; string whyDs;
                    List<CSagDsRow> rows = SagRun_ReadDataset(csv, out whyDs, out nBroken);
                    var st = new CSagDatasetStat { Exists = true, Path = dsPath }; SagRun_CountRows(rows, st); st.Broken = nBroken;
                    res.NRows = st.Rows; res.Groups = st.Groups; res.Sessions = st.Sessions;

                    // 2. 첫닿음 모델 (복제 기구학 — 관절값 사본 특징)
                    //    걸리는 시간 예상: 초매개변수 216가지 × 교차검증 겹 × 촐레스키(점 수³) — 73점·10겹 ≈ 0.4초 (10-02 측정)
                    int nUse = cfg.UseHold ? st.F : st.F - st.FHold, nFold = Math.Min(Math.Max(1, st.Groups), SAGRUN_LEARN_MAX_FOLDS);
                    double secEst = 0.4 * (nFold / 10.0) * Math.Pow(Math.Max(1, nUse) / 73.0, 3);
                    L(string.Format(ci, "진행 학습: 첫닿음 {0}점 · 묶음 {1} (교차검증 {2}겹) — 약 {3}", nUse, st.Groups, nFold,
                        secEst < 60 ? Math.Max(1, Math.Ceiling(secEst)).ToString("0", ci) + "초" : (secEst / 60).ToString("0.#", ci) + "분 ([중단] 으로 멈출 수 있다)"));
                    if (SagRun_Stopped(stop)) return SagRun_LearnStop(res, L);
                    string rep;
                    if (!kin.SagComp_FitFloorFc(csv, m, cfg.UseHold, L, stop, out rep) || !m.HasFc)
                    {
                        if (SagRun_Stopped(stop)) return SagRun_LearnStop(res, L);
                        return SagRun_LearnFail(res, "첫닿음 학습 실패 — " + rep, L);
                    }
                    if (SagRun_Stopped(stop)) return SagRun_LearnStop(res, L);
                    // 3. 기준 높이
                    if (!float.IsNaN(cfg.RefMm)) { m.Fc.Ref = cfg.RefMm; m.Fc.RefFixed = true; }
                    // 4. 켬 + 매핑 (키는 파일에 적히고 다시 읽히는 모양으로 — 줄바꿈·'#' 빼고 앞뒤 공백 없이).
                    //   바탕 관절 모델을 학습한 매핑(바탕 파일의 motormap, 없으면 기본 매핑)이 모델 축(관절 모델·위치 3개·손목)에서 지금 매핑과 다르면 (10-04 검토:
                    //   영점·방향을 바꾼 뒤의 학습이 지금 키를 적어 motormap 비교를 조용히 지나갔다 — 관절 모델은 옛 매핑의 것) 결과에도 바탕의 키를 적는다 →
                    //   적용·시험·시작 때 '학습 때와 방향·영점이 다름 — 다시 학습 권장' 이 계속 나온다. 보고서·한 줄·로그에 ★ (⑨ 로 관절 모델부터 다시 학습한 뒤 ④)
                    string mapKey = SagRun_NormMapKey(strMotorMapKey);
                    int[] mapAxes = SagRun_ModelAxes(m);
                    string dBase = (mapKey == null) ? "" : SagRun_MotorMapDiff(SagRun_MapKeyFor(baseMap, mapAxes), SagRun_MapKeyFor(mapKey, mapAxes));
                    string stampKey = (dBase.Length > 0) ? baseMap : mapKey;
                    if (dBase.Length > 0)
                    {
                        res.MotorMapWarn = "★바탕 관절 모델은 다른 매핑에서 학습됨 (" + dBase + ") — ⑨ 처짐보상으로 관절 모델부터 다시 학습한 뒤 ④ (결과 파일에는 바탕의 매핑을 적음 — 적용·시험 때 경고)";
                        L(res.MotorMapWarn);
                    }
                    m.Fc.Enabled = true; m.Enabled = true; m.FloorMapEnabled = true;
                    m.MotorMap = stampKey;
                    m.KinSig = kinNow;                                               // 이 결과를 만든 기구학 (수직 관절 합 downsum 은 바탕 값 그대로)
                    res.NUsed = m.Fc.Count; res.NHold = cfg.UseHold ? SagRun_CountUsedHold(rows) : 0;
                    res.Ref = m.Fc.Ref; res.CvRmse = m.Fc.CvRmse; res.CvPhysRmse = m.Fc.CvPhysRmse; res.Beta = (double[])m.Fc.Beta.Clone();
                    // 4b. 값 확인 — 단위·자료가 틀린 학습(미터로 적힌 fc, 엉뚱한 큰 값, 동떨어진 fcref)은 결과 파일로 남기지 않는다
                    string whyVal = SagRun_FcSanity(m.Fc);
                    if (whyVal.Length > 0) return SagRun_LearnFail(res, "학습 결과가 이상함 — " + whyVal + " (데이터셋 fc 칸이 mm 인지·기준 높이 확인)", L);

                    // 5. 예상 표
                    L("진행 학습: 예상 표 (사각형·반경선)");
                    var arm = new CSagArmCfg { FuncNumber = m.FuncNumber, PosIDs = (int[])m.PosIDs.Clone(), WristID = m.WristID, DownSumDeg = m.DownSumDeg };
                    string preview = SagRun_LearnPreview(kin, m, arm, cfg, res);

                    // 6. 저장 (중단 요청이 왔으면 파일을 만들지 않는다)
                    if (SagRun_Stopped(stop)) return SagRun_LearnStop(res, L);
                    DateTime now = DateTime.Now;
                    string resultsDir = SagRun_ResultsDir(strUrdfDir);
                    Directory.CreateDirectory(resultsDir);
                    string path = SagRun_NewResultPath(resultsDir, now);
                    if (path == null) return SagRun_LearnFail(res, "결과 파일 이름을 만들지 못함 (" + resultsDir + ")", L);
                    made.Add(path);
                    string stem = Path.Combine(resultsDir, Path.GetFileNameWithoutExtension(path));
                    string strRef = m.Fc.RefFixed ? string.Format(ci, "고정 {0:F3}mm (fcref)", m.Fc.Ref) : string.Format(ci, "학습 평균 {0:F3}mm", m.Fc.Ref);
                    var note = new List<string> {
                        "로봇팔 세팅 학습 결과 — " + now.ToString("yyyy-MM-dd HH:mm:ss", ci),
                        "바탕(관절 모델): " + basePath,
                        string.Format(ci, "데이터셋: {0} — {1}행 (첫닿음 F {2}, 그중 손목 유지 켬 {3}; H {4}, C {5}, X {6}, 판정 없음 {7}), 회차 {8}, 묶음 {9}",
                            dsPath, st.Rows, st.F, st.FHold, st.H, st.C, st.X, st.Blank, st.Sessions, st.Groups),
                        string.Format(ci, "학습: 첫닿음 {0}점 — 손목 유지 켬 행 {1} (UseHold={2})", m.Fc.Count, cfg.UseHold ? "넣음" : "뺌", cfg.UseHold ? 1 : 0),
                        "기준: " + strRef };
                    if (dBase.Length > 0) note.Add("★바탕 관절 모델은 지금과 다른 모터 매핑에서 학습됨 (" + dBase + ") — motormap 은 바탕 파일의 것");
                    note.Add("dataset=" + dsPath);
                    m.Note = string.Join("\n", note.ToArray());
                    m.Save(path);
                    // 다시 읽어 확인 — 첫닿음 모델이 온전하고 준비되며 같은 보정을 내는가
                    CSagComp m2 = CSagComp.Load(path);
                    string chk = SagRun_LearnReloadCheck(kin, m, m2, arm, stampKey);
                    if (chk.Length > 0) return SagRun_LearnFail(res, "저장한 결과 파일을 다시 읽어 보니 다름 — " + chk + " (파일 지움)", L, made);

                    string dataPath = stem + "_data.csv", reportPath = stem + "_report.txt";
                    made.Add(dataPath); File.Copy(dsPath, dataPath, true);
                    string mm = string.IsNullOrEmpty(m.MotorMap) ? "기록 안 함 (키 없음 = 기본 매핑으로 본다)" : m.MotorMap;
                    var sb = new StringBuilder();
                    sb.AppendLine("=== 처짐 바닥 학습 결과 (첫닿음 모델) ===");
                    sb.AppendLine("결과 파일: " + path);
                    sb.AppendLine("만든 시각: " + now.ToString("yyyy-MM-dd HH:mm:ss", ci));
                    sb.AppendLine("바탕(관절 모델): " + basePath);
                    sb.AppendLine("  " + baseSummary);
                    sb.AppendLine("데이터셋: " + dsPath);
                    sb.AppendLine(string.Format(ci, "  {0}행 — 첫닿음 F {1} (손목 유지 켬 {2}), 단단한 정지 H {3}, 검열 C {4}, 못 씀 X {5}, 판정 없음 {6}{7}; 회차 {8}, 묶음 {9}",
                        st.Rows, st.F, st.FHold, st.H, st.C, st.X, st.Blank, nBroken > 0 ? string.Format(ci, ", 못 읽은 줄 {0}", nBroken) : "", st.Sessions, st.Groups));
                    sb.AppendLine(string.Format(ci, "학습에 쓴 점: 첫닿음 {0}점 (손목 유지 켬 {1} — {2})", m.Fc.Count, res.NHold, cfg.UseHold ? "켬 행 넣음" : "켬 행 뺌"));
                    sb.AppendLine("기준 Ref: " + strRef);
                    sb.AppendLine("모터 매핑: " + mm);
                    if (dBase.Length > 0)
                    {
                        sb.AppendLine("  ★바탕 관절 모델은 다른 매핑에서 학습됨 (" + dBase + ") — 관절 모델은 바꾸기 전의 방향·영점 그대로라, 이 파일의 motormap 도 바탕 파일의 것을 적었다");
                        sb.AppendLine("    (적용·시험·앱 시작 때 '학습 때와 방향·영점이 다름 — 다시 학습 권장' 이 나온다). ⑨ 처짐보상으로 관절 모델부터 다시 학습한 뒤 ④ 를 다시");
                    }
                    sb.AppendLine("  데이터셋 행에는 모터 매핑이 적혀 있지 않다 — 방향·영점을 바꾸기 전에 잰 행도 그대로 섞여 학습된다 (바꿨다면 ③ 으로 새로 잰 행으로 학습 권장 —");
                    sb.AppendLine("    sagarm.txt 의 learndataset 로 다른 데이터셋을 고를 수 있다)");
                    sb.AppendLine();
                    sb.AppendLine("[첫닿음 모델 학습]");
                    sb.AppendLine(rep);
                    sb.AppendLine();
                    sb.Append(preview);
                    sb.AppendLine();
                    sb.AppendLine("[모델 요약]");
                    sb.AppendLine(m.Summary());
                    made.Add(reportPath);
                    File.WriteAllText(reportPath, sb.ToString(), new UTF8Encoding(false));

                    res.Ok = true; res.Model = m; res.ModelPath = path; res.ReportPath = reportPath; res.DataPath = dataPath; res.Report = sb.ToString();
                    double mn = double.NaN, mx = double.NaN; SagRun_MinMax(res.PreviewDz, out mn, out mx);
                    string strLine = string.Format(ci, "{0} · 첫닿음 {1}점{2} · CV {3} · Ref {4:F1}{5} · 사각형 dz {6}",
                        Path.GetFileNameWithoutExtension(path), res.NUsed, res.NHold > 0 ? string.Format(ci, " (켬 {0})", res.NHold) : "",
                        double.IsNaN(res.CvRmse) ? "계산 불가" : string.Format(ci, "{0:F2}mm (물리만 {1:F2})", res.CvRmse, res.CvPhysRmse),
                        res.Ref, m.Fc.RefFixed ? " 고정" : "",
                        double.IsNaN(mn) ? "—" : string.Format(ci, "{0:+0.0;-0.0}~{1:+0.0;-0.0}", mn, mx));
                    res.Line = (res.MotorMapWarn.Length > 0) ? res.MotorMapWarn + "\r\n" + strLine : strLine;   // 바탕 매핑이 다르면 그 경고를 맨 앞에 (로그에는 위에서 한 번)
                    made.Clear();                                                // 성공 — 남긴다
                    L(strLine);
                    L("결과 파일: " + path);
                }
                catch (Exception ex) { res.Ok = false; res.Why = "★오류: " + SagRun_Msg(ex); L(res.Why); }
                finally { foreach (string p in made) { try { if (File.Exists(p)) File.Delete(p); } catch { } } }
                return res;
            }
            // 학습한 첫닿음 모델이 이 실험에서 말이 되는가 — 아니면 이유 (되면 ""). 첫닿음 FK 높이는 OMX 바닥에서 ≈ 30~60mm 이고 측정 창 한도는 3mm 아래로
            //   내려가지 않는다: 학습 높이 평균 3~300mm, 모두 유한, 기준 Ref 가 그 평균에서 보정 상한(DzMax) 안 (밖이면 모든 점의 보정이 상한에 걸린다),
            //   β 유한, 교차검증 RMSE ≤ 20mm (묶음이 하나라 못 하면 NaN — 통과). 미터로 적힌 fc (평균 ≈ 0.05)·엉뚱한 큰 값·fcref 오타를 막는다
            private static string SagRun_FcSanity(CSagFc fc)
            {
                var ci = CultureInfo.InvariantCulture;
                if (fc == null) return "첫닿음 모델 없음";
                double sy = 0; int ny = 0;
                if (fc.Y != null)
                    foreach (double y in fc.Y)
                    {
                        if (double.IsNaN(y) || double.IsInfinity(y)) return "학습 첫닿음 높이에 수가 아닌 값";
                        sy += y; ny++;
                    }
                if (ny == 0) return "학습점 없음";
                double my = sy / ny;
                if (my < 3 || my > 300) return string.Format(ci, "학습 첫닿음 높이 평균 {0:G4}mm 가 3~300mm 밖", my);
                if (double.IsNaN(fc.Ref) || double.IsInfinity(fc.Ref) || Math.Abs(fc.Ref - my) > fc.DzMax)
                    return string.Format(ci, "기준 Ref {0:G4}mm 가 학습 평균 {1:F1}mm 에서 보정 상한 {2:G3}mm 넘게 떨어짐 (모든 점의 보정이 상한에 걸린다)", fc.Ref, my, fc.DzMax);
                if (fc.Beta == null) return "물리 평균 계수 없음";
                foreach (double b in fc.Beta) if (double.IsNaN(b) || double.IsInfinity(b)) return "물리 평균 계수가 수가 아님";
                if (double.IsInfinity(fc.CvRmse) || fc.CvRmse > 20) return string.Format(ci, "교차검증 오차 {0:G4}mm > 20mm", fc.CvRmse);
                return "";
            }
            // 매핑 키 — CSagComp.Save/Load 를 거친 모양으로 (줄바꿈·'#' 빼고 앞뒤 공백 없이), 비면 null
            private static string SagRun_NormMapKey(string strKey)
            {
                if (strKey == null) return null;
                string k = strKey.Replace("\r", "").Replace("\n", "").Replace("#", "").Trim();
                return k.Length > 0 ? k : null;
            }
            // 학습에 실제로 들어간 손목 유지 켬 행 — SagComp_FitFloorFc 의 행 고르기와 같은 조건 (kind=F, hold=1, fc·q 네 칸이 수)
            private static int SagRun_CountUsedHold(List<CSagDsRow> rows)
            {
                int n = 0;
                if (rows == null) return 0;
                foreach (CSagDsRow r in rows)
                {
                    if (r.Kind != "F" || !r.Hold || float.IsNaN(r.Fc) || float.IsInfinity(r.Fc) || r.Q == null || r.Q.Length < 4) continue;
                    bool ok = true;
                    foreach (float v in r.Q) if (float.IsNaN(v) || float.IsInfinity(v)) ok = false;
                    if (ok) n++;
                }
                return n;
            }
            private const int SAGRUN_LEARN_MAX_FOLDS = 10;                   // 첫닿음 학습 교차검증 겹 상한 (SagComp_FitFloorFc 의 SAGFC_MAX_FOLDS 와 같다 — 시간 예상용)
            private static CSagLearnResult SagRun_LearnStop(CSagLearnResult res, CSim2Real.DLog L)
            {
                SagRun_LearnFail(res, "중단 요청 — 결과 파일을 만들지 않음", L);
                res.Stopped = true;
                return res;
            }
            // 중단 요청인가 (대리자가 던져도 계속한다)
            private static bool SagRun_Stopped(CSim2Real.DIsStopped stop)
            {
                if (stop == null) return false;
                try { return stop(); } catch { return false; }
            }
            private static CSagLearnResult SagRun_LearnFail(CSagLearnResult res, string why, CSim2Real.DLog L) { return SagRun_LearnFail(res, why, L, null); }
            private static CSagLearnResult SagRun_LearnFail(CSagLearnResult res, string why, CSim2Real.DLog L, List<string> made)
            {
                res.Ok = false; res.Why = why; res.Line = "학습 실패 — " + why; res.Report = res.Line;
                if (L != null) L("★" + res.Line);
                if (made != null) foreach (string p in made) { try { if (File.Exists(p)) File.Delete(p); } catch { } }
                if (made != null) made.Clear();
                return res;
            }
            // 바탕 모델 — 새로 읽어 바닥 부분(Gp·보정표·Fc)과 기록(Note·MotorMap)을 벗긴다. 관절 모델이 없으면 null.
            //   strBaseMap = 벗기기 전의 motormap (관절 모델을 학습한 매핑 — null = 키 없음, 기본 매핑으로 본다)
            private static CSagComp SagRun_LoadBase(string strPath, out string why, out string strBaseMap)
            {
                why = ""; strBaseMap = null;
                if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { why = "바탕 모델 파일 없음: " + strPath; return null; }
                CSagComp m = CSagComp.Load(strPath);
                if (m == null || !m.HasJoint) { why = "바탕 파일에 관절 모델이 없음 — ⑨ 처짐보상으로 먼저 (" + strPath + ")"; return null; }
                if (m.PosIDs == null || m.PosIDs.Length != 3) { why = "바탕 파일의 posids 가 3개가 아님"; return null; }
                strBaseMap = SagRun_NormMapKey(m.MotorMap);
                m.Gp = null; m.FloorR = m.FloorDz = null; m.FloorRef = 0; m.FloorFromCmd = false; m.Fc = null; m.FcDropped = false;
                m.FloorMapEnabled = true; m.Note = null; m.MotorMap = null;
                return m;
            }
            // 모델이 쓰는 관절 (관절 모델 ID — 없으면 위치 3개 + 손목) — 매핑 비교를 이 축으로 한정한다 (그리퍼 등 다른 축의 매핑은 모델과 상관없다)
            private static int[] SagRun_ModelAxes(CSagComp m)
            {
                var l = new List<int>();
                if (m != null && m.JointIDs != null) foreach (int id in m.JointIDs) if (!l.Contains(id)) l.Add(id);
                if (m != null && m.PosIDs != null) foreach (int id in m.PosIDs) if (!l.Contains(id)) l.Add(id);
                if (m != null && !l.Contains(m.WristID)) l.Add(m.WristID);
                return l.ToArray();
            }
            // 매핑 키에서 그 축들의 칸만 (키 순서 그대로, '|' 로) — 키가 비었으면 그대로 (null = 기본 매핑)
            private static string SagRun_MapKeyFor(string strKey, int[] axes)
            {
                if (string.IsNullOrEmpty(strKey) || axes == null) return strKey;
                var keep = new List<string>();
                foreach (string s in strKey.Split('|'))
                {
                    string[] p = s.Split(':');
                    int ax;
                    if (p.Length >= 1 && int.TryParse(p[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ax) && Array.IndexOf(axes, ax) >= 0) keep.Add(s.Trim());
                }
                return string.Join("|", keep.ToArray());
            }
            // 결과 파일 이름 — sagcomp_model_<yyyyMMdd_HHmm>[_n].txt, 같은 이름의 모델·보고서·데이터·시험 기록이 없을 때까지 n 을 올리고 빈 파일로 자리를 잡는다
            private static string SagRun_NewResultPath(string strResultsDir, DateTime t)
            {
                string stamp = "sagcomp_model_" + t.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
                for (int n = 1; n < 1000; n++)
                {
                    string stem = Path.Combine(strResultsDir, stamp + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : ""));
                    string p = stem + ".txt";
                    if (File.Exists(p) || File.Exists(stem + "_report.txt") || File.Exists(stem + "_data.csv") || File.Exists(stem + "_test.txt")) continue;
                    try { using (new FileStream(p, FileMode.CreateNew, FileAccess.Write)) { } }
                    catch (IOException) { if (File.Exists(p)) continue; throw; }
                    return p;
                }
                return null;
            }
            // 다시 읽은 모델 확인 — 다르면 이유 (같으면 "")
            private static string SagRun_LearnReloadCheck(C3d kin, CSagComp m, CSagComp m2, CSagArmCfg arm, string strMotorMapKey)
            {
                var ci = CultureInfo.InvariantCulture;
                if (m2 == null) return "읽기 실패";
                if (!m2.HasJoint) return "관절 모델 없음";
                if (!m2.HasFc || m2.FcDropped) return "첫닿음 모델이 읽히지 않음";
                if (!kin.SagComp_FcPrepare(m2) || !m2.Fc.IsReady) return "첫닿음 모델 준비 실패";
                if (m2.Fc.Count != m.Fc.Count) return string.Format(ci, "학습점 수 {0} ≠ {1}", m2.Fc.Count, m.Fc.Count);
                if (Math.Abs(m2.Fc.Ref - m.Fc.Ref) > 1e-6 || m2.Fc.RefFixed != m.Fc.RefFixed) return "기준 Ref 다름";
                if (!m2.Fc.Enabled || !m2.FloorMapEnabled) return "켬 표시가 꺼져 읽힘";
                if ((m2.MotorMap ?? "") != (string.IsNullOrEmpty(strMotorMapKey) ? "" : strMotorMapKey)) return "motormap 다름";
                float[] q; float zT; double dz1, sd1, dz2, sd2;
                if (SagRun_DzAt(kin, m, arm, 210, 7.5f, 80, 90, out q, out zT, out dz1, out sd1))
                {
                    float[] info; dz2 = kin.SagComp_FcCorrection(m2, q, out sd2, out info);
                    if (Math.Abs(dz2 - dz1) > 1e-6) return string.Format(ci, "가운데 점 보정 {0:F4} ≠ {1:F4}", dz2, dz1);
                }
                return "";
            }
            // 예상 표 — 결과에 PreviewDz·PreviewStd·SquareSpread·MaxStep 을 채우고 보고서 토막을 돌려준다
            private static string SagRun_LearnPreview(C3d kin, CSagComp m, CSagArmCfg arm, CSagLearnCfg cfg, CSagLearnResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                float[][] pts = cfg.PreviewPts ?? CSagGapCfg.DefaultPts();
                float pz = cfg.PreviewZ;
                double stdLim = m.Fc.StdLimit;
                res.PreviewDz = new float[pts.Length]; res.PreviewStd = new float[pts.Length];
                var rr = new double[pts.Length];
                sb.AppendLine(string.Format(ci, "[예상 표 — 펜 높이 z={0:0.#}, 피치 90 · dz = 보정 켬이면 끝을 올리는 양 (위로 +) = 켬 − 끔 틈 변화]", pz));
                sb.AppendLine("       x       y       r      dz    std");
                for (int i = 0; i < pts.Length; i++)
                {
                    res.PreviewDz[i] = res.PreviewStd[i] = float.NaN; rr[i] = double.NaN;
                    if (pts[i] == null || pts[i].Length < 2) continue;
                    float[] q; float zT; double dz, sd;
                    rr[i] = Math.Sqrt((double)pts[i][0] * pts[i][0] + (double)pts[i][1] * pts[i][1]);
                    if (SagRun_DzAt(kin, m, arm, pts[i][0], pts[i][1], pz, 90, out q, out zT, out dz, out sd)) { res.PreviewDz[i] = (float)dz; res.PreviewStd[i] = (float)sd; }
                    sb.AppendLine(string.Format(ci, "  {0,6:F1}  {1,6:F1}  {2,6:F1}  {3}  {4}", pts[i][0], pts[i][1], rr[i],
                        float.IsNaN(res.PreviewDz[i]) ? "  IK 불가" : res.PreviewDz[i].ToString("+0.00;-0.00", ci).PadLeft(6),
                        float.IsNaN(res.PreviewStd[i]) ? "" : res.PreviewStd[i].ToString("F2", ci) + (res.PreviewStd[i] > stdLim ? " *" : "")));
                }
                // 먼쪽(최대 반경 15mm 안) − 가까운쪽(최소 반경 15mm 안)
                double rMin = double.MaxValue, rMax = double.MinValue;
                for (int i = 0; i < pts.Length; i++) if (!double.IsNaN(rr[i]) && !float.IsNaN(res.PreviewDz[i])) { rMin = Math.Min(rMin, rr[i]); rMax = Math.Max(rMax, rr[i]); }
                if (rMax - rMin > 30)
                {
                    double sf = 0, sn = 0; int nf = 0, nn = 0;
                    for (int i = 0; i < pts.Length; i++)
                    {
                        if (double.IsNaN(rr[i]) || float.IsNaN(res.PreviewDz[i])) continue;
                        if (rr[i] >= rMax - 15) { sf += res.PreviewDz[i]; nf++; }
                        else if (rr[i] <= rMin + 15) { sn += res.PreviewDz[i]; nn++; }
                    }
                    if (nf > 0 && nn > 0) res.SquareSpread = sf / nf - sn / nn;
                }
                double mn, mx; SagRun_MinMax(res.PreviewDz, out mn, out mx);
                sb.AppendLine(string.Format(ci, "  먼쪽 − 가까운쪽 dz 차 {0}, dz {1} — 켬에서는 이 차이만큼 먼 쪽 끝을 더 올린다",
                    double.IsNaN(res.SquareSpread) ? "—" : res.SquareSpread.ToString("F1", ci) + "mm", double.IsNaN(mn) ? "—" : string.Format(ci, "{0:+0.0;-0.0} ~ {1:+0.0;-0.0}mm", mn, mx)));
                // 반경선
                float[] zs = { 60, 75, 90 };
                var radii = new List<float>(); for (float r = 120; r <= 260.001f; r += 10) radii.Add(r);
                var tbl = new double[zs.Length, radii.Count]; var tsd = new double[zs.Length, radii.Count];
                for (int zi = 0; zi < zs.Length; zi++)
                    for (int ri = 0; ri < radii.Count; ri++)
                    {
                        float[] q; float zT; double dz, sd;
                        tbl[zi, ri] = tsd[zi, ri] = double.NaN;
                        if (SagRun_DzAt(kin, m, arm, radii[ri], 0, zs[zi], 90, out q, out zT, out dz, out sd)) { tbl[zi, ri] = dz; tsd[zi, ri] = sd; }
                    }
                sb.AppendLine();
                sb.AppendLine("[반경선 — 요 0, 피치 90, r 120~260 / 10mm · dz (mm), * = 예측 표준편차가 한계를 넘음 (넘겨짚기)]");
                sb.AppendLine("      r     z=60     z=75     z=90");
                bool bStar = false;
                for (int ri = 0; ri < radii.Count; ri++)
                {
                    var line = new StringBuilder(string.Format(ci, "  {0,5:F0}", radii[ri]));
                    for (int zi = 0; zi < zs.Length; zi++)
                    {
                        string v = double.IsNaN(tbl[zi, ri]) ? "  —  " : tbl[zi, ri].ToString("+0.00;-0.00", ci);
                        bool st = !double.IsNaN(tsd[zi, ri]) && tsd[zi, ri] > stdLim; if (st) bStar = true;
                        line.Append("   " + (v + (st ? "*" : " ")).PadLeft(7));
                    }
                    sb.AppendLine(line.ToString());
                }
                double maxStep = double.NaN;
                for (int zi = 0; zi < zs.Length; zi++)
                {
                    double lo = double.MaxValue, hi = double.MinValue, step = 0; int n = 0;
                    for (int ri = 0; ri < radii.Count; ri++)
                    {
                        if (double.IsNaN(tbl[zi, ri])) continue;
                        lo = Math.Min(lo, tbl[zi, ri]); hi = Math.Max(hi, tbl[zi, ri]); n++;
                        if (ri > 0 && !double.IsNaN(tbl[zi, ri - 1])) step = Math.Max(step, Math.Abs(tbl[zi, ri] - tbl[zi, ri - 1]));
                    }
                    if (n > 0) maxStep = double.IsNaN(maxStep) ? step : Math.Max(maxStep, step);
                    sb.AppendLine(n > 0 ? string.Format(ci, "  z={0:F0}: dz {1:+0.0;-0.0} ~ {2:+0.0;-0.0}mm, 이웃 최대 계단 {3:F1}mm", zs[zi], lo, hi, step)
                                        : string.Format(ci, "  z={0:F0}: IK 불가", zs[zi]));
                }
                if (bStar) sb.AppendLine(string.Format(ci, "  * 예측 표준편차 > {0:F1}mm — 잰 적 없는 곳 (넘겨짚기). 그 근처를 재서 다시 학습하면 줄어든다", stdLim));
                res.MaxStep = maxStep;
                return sb.ToString();
            }
            private static void SagRun_MinMax(float[] v, out double mn, out double mx)
            {
                mn = mx = double.NaN;
                if (v == null) return;
                foreach (float f in v)
                {
                    if (float.IsNaN(f)) continue;
                    if (double.IsNaN(mn) || f < mn) mn = f;
                    if (double.IsNaN(mx) || f > mx) mx = f;
                }
            }

            // ────────────────────────────────────────────────────────────
            // ⑤ 결과 파일 시험 — 계획 (움직임 없음)·판정·기록
            // ────────────────────────────────────────────────────────────

            /// <summary>닿기 시험 점·창 (오프라인). 점 = 요들 × 반경들 (요마다 반경 순서 그대로). 점마다 첫닿음 모델(mOn)의 SagComp_FcPlan:
            /// PredFc = 끔에서 닿는 FK 예측, Ref = 켬에서 닿는 명령 높이 예측 → 시작은 끔·켬 같게 ZStart = max(PredFc, Ref) + Above(20),
            /// 한도는 모드마다 그 모드의 예상 첫닿음 Below(12) 아래: ZMinOff = max(ZMinFloor(3), PredFc − Below), ZMinOn = max(ZMinFloor, Ref − Below)
            /// — 두 모드가 같은 깊이만큼만 눌러 첫닿음이 꺾임 맞춤 창(FitWindowMm 14) 안에 든다 (CSagTestCfg 설명). ZMin = 둘 중 낮은 쪽 (IK 확인은 이것으로).
            /// 실물 누름은 모드마다 ZMinFor(켬) 을 SagComp_FloorPress 의 ZMin 으로 쓴다.
            /// 첫닿음 예측이 안 되거나 IK 확인(SagRun_CheckPlan 과 같음)이 안 되면 Skipped. 예외 없음.</summary>
            public List<CSagTestPoint> SagRun_TestPoints(CSagComp mOn, CSagTestCfg cfg, CSagArmCfg arm, out string report)
            {
                using (SagRun_KinUse()) return SagRun_TestPointsBody(mOn, cfg, arm, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private List<CSagTestPoint> SagRun_TestPointsBody(CSagComp mOn, CSagTestCfg cfg, CSagArmCfg arm, out string report)
            {
                report = "";
                var pts = new List<CSagTestPoint>();
                try
                {
                    if (cfg == null) cfg = new CSagTestCfg();
                    if (arm == null) arm = new CSagArmCfg();
                    if (mOn == null || !mOn.HasFc) { report = "첫닿음 모델이 없는 파일 — 닿기 시험 창을 정할 수 없다 (④ 학습 결과 파일을 고르십시오)"; return pts; }
                    string wd = "관절 구성(func·posids·wrist)", noteDef;
                    if (!SagDef_Gate(arm, true, null, out noteDef)) { report = "★로봇 정의: " + noteDef; return pts; }   // 정의 확인 (3D 에 수식이 없으면 내장 OMX 로 — ★ 알림)
                    if (!arm.IdsOk || !SagRun_SameDef(mOn, arm, out wd)) { report = "모델과 설정의 로봇 정의가 다름 — " + wd; return pts; }
                    C3d kin = SagRun_Kin();
                    if (kin == null) { report = SAGRUN_NO_KIN; return pts; }
                    var ci = CultureInfo.InvariantCulture;
                    double refv = mOn.Fc.Ref, stdLim = mOn.Fc.StdLimit;
                    foreach (float yaw in cfg.Yaws ?? new float[] { 0 })
                        foreach (float r in cfg.Radii ?? new float[0])
                        {
                            double a = yaw * Math.PI / 180.0;
                            var p = new CSagTestPoint { R = r, Yaw = yaw, Pitch = cfg.Pitch, X = (float)(r * Math.Cos(a)), Y = (float)(r * Math.Sin(a)), Ref = (float)refv,
                                                        ZStart = float.NaN, ZMin = float.NaN };
                            double pred, std; float qs; string whyFc;
                            if (!SagRun_FcPlanSeeded(kin, mOn, arm, yaw, p.X, p.Y, cfg.Pitch, out pred, out std, out qs, out whyFc))
                            { p.Skipped = true; p.Note = whyFc + " — 시험 제외"; pts.Add(p); continue; }
                            p.PredFc = (float)pred; p.PredStd = (float)std; p.QSum = qs;
                            p.ZStart = (float)(Math.Max(pred, refv) + cfg.AboveMm);
                            p.ZMinOff = (float)Math.Max(cfg.ZMinFloor, pred - cfg.BelowMm);
                            p.ZMinOn = (float)Math.Max(cfg.ZMinFloor, refv - cfg.BelowMm);
                            p.ZMin = Math.Min(p.ZMinOff, p.ZMinOn);
                            if (std > stdLim) p.Note = string.Format(ci, "넘겨짚기 (std {0:F1})", std);
                            if (!(p.ZStart >= Math.Max(pred, refv) + CSagArmSetup.ABOVE_MIN - 1e-3))   // 창 시작까지는 그냥 내려간다 — 예측 닿는 높이 위 5mm 는 돼야
                            { p.Skipped = true; p.Note = SagRun_WhyJoin(string.Format(ci, "창 시작 z={0:F1} 이 예측 닿는 높이 {1:F1} + {2:0} 보다 낮음 (창 '위' {3:0.#})", p.ZStart, Math.Max(pred, refv), CSagArmSetup.ABOVE_MIN, cfg.AboveMm), p.Note) + " — 시험 제외"; }
                            pts.Add(p);
                        }
                    // IK 확인 — 측정 계획과 같은 순서 (SafeZ → 창 → 복귀)
                    var rows = new List<CFloorPlanRow>(); var map = new List<CSagTestPoint>();
                    foreach (CSagTestPoint p in pts)
                        if (!p.Skipped)
                        {
                            // 예측 = 두 모드 중 높은 닿는 높이 (창 시작은 그보다 Above 위 — CheckPlan 의 ABOVE_MIN 규칙이 한 번 더 본다)
                            rows.Add(new CFloorPlanRow { Set = "T", Pitch = p.Pitch, Yaw = p.Yaw, R = p.R, X = p.X, Y = p.Y, ZStart = p.ZStart, ZMin = p.ZMin, PredZ = Math.Max(p.PredFc, p.Ref),
                                                         LimitByCaller = true });   // 한도는 모드마다 위에서 정했다 (끔 = 예측 − 아래, 켬 = Ref − 아래)
                            map.Add(p);
                        }
                    string repIk = "";
                    if (rows.Count > 0) SagRun_CheckPlan(rows, arm, cfg.Probe != null ? cfg.Probe.RetractAboveKneeMm : 25, out repIk);
                    for (int i = 0; i < rows.Count; i++)
                    {
                        map[i].TransferZ = rows[i].TransferZ;
                        if (!rows[i].IkOk) { map[i].Skipped = true; map[i].Note = SagRun_WhyJoin(rows[i].Why, map[i].Note) + " — 시험 제외"; }
                    }
                    int nUse = 0; foreach (CSagTestPoint p in pts) if (!p.Skipped) nUse++;
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format(ci, "닿기 시험 점 {0} (쓸 점 {1}) — 피치 {2:0.#}, 반복 {3}, 손목 유지 {4}, Ref {5:F2} (켬에서 닿는 명령 높이 예측)",
                        pts.Count, nUse, cfg.Pitch, cfg.Repeats, cfg.WristHold ? "켬" : "끔", refv));
                    sb.AppendLine(string.Format(ci, "  창: 시작 = max(끔 예측, Ref) + {0:0.#} (끔·켬 같음), 한도 끔 = max({1:0.#}, 끔 예측 − {2:0.#}), 켬 = max({1:0.#}, Ref − {2:0.#})",
                        cfg.AboveMm, cfg.ZMinFloor, cfg.BelowMm));
                    sb.AppendLine("     — 두 모드 모두 예상 첫닿음 아래 같은 깊이까지만 누른다 (첫닿음이 꺾임 맞춤 창 안에)");
                    sb.AppendLine("      r    요   끔 예측(FK)   std     시작  한도 끔  한도 켬   비고");
                    foreach (CSagTestPoint p in pts)
                        sb.AppendLine(string.Format(ci, "  {0,5:F0} {1,5:F0}   {2,9}   {3,5}   {4,6}  {5,7}  {6,7}   {7}", p.R, p.Yaw,
                            float.IsNaN(p.PredFc) ? "—" : p.PredFc.ToString("F2", ci), float.IsNaN(p.PredStd) ? "" : p.PredStd.ToString("F1", ci),
                            float.IsNaN(p.ZStart) ? "" : p.ZStart.ToString("F1", ci), float.IsNaN(p.ZMinOff) ? "" : p.ZMinOff.ToString("F1", ci),
                            float.IsNaN(p.ZMinOn) ? "" : p.ZMinOn.ToString("F1", ci), p.Skipped ? "제외 — " + p.Note : p.Note));
                    sb.Append(repIk);
                    report = sb.ToString();
                }
                catch (Exception ex) { report = "★오류: " + SagRun_Msg(ex); }
                return pts;
            }

            /// <summary>틈 재기 계획 (오프라인) — 점마다 높이 Z·피치에서 IK 자세(시드 → SafeZ → Z, 복제 기구학)와
            /// 그 자세에서 첫닿음 모델이 끝을 올리는 양 dz_pred = SagComp_FcCorrection(mOn, q) (= 켬 − 끔 틈 변화 예상, 위로 +).
            /// IK 가 안 되는 점은 IkOk = false (Note). 예외 없음.</summary>
            public List<CSagGapPoint> SagRun_GapPlan(CSagComp mOn, CSagGapCfg cfg, CSagArmCfg arm, out string report)
            {
                using (SagRun_KinUse()) return SagRun_GapPlanBody(mOn, cfg, arm, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private List<CSagGapPoint> SagRun_GapPlanBody(CSagComp mOn, CSagGapCfg cfg, CSagArmCfg arm, out string report)
            {
                report = "";
                var pts = new List<CSagGapPoint>();
                try
                {
                    if (cfg == null) cfg = new CSagGapCfg();
                    if (arm == null) arm = new CSagArmCfg();
                    if (mOn == null || !mOn.HasFc) { report = "첫닿음 모델이 없는 파일 — 틈 변화 예상을 낼 수 없다"; return pts; }
                    string wd = "관절 구성(func·posids·wrist)", noteDef;
                    if (!SagDef_Gate(arm, true, null, out noteDef)) { report = "★로봇 정의: " + noteDef; return pts; }   // 정의 확인 (3D 에 수식이 없으면 내장 OMX 로 — ★ 알림)
                    if (!arm.IdsOk || !SagRun_SameDef(mOn, arm, out wd)) { report = "모델과 설정의 로봇 정의가 다름 — " + wd; return pts; }
                    C3d kin = SagRun_Kin();
                    if (kin == null) { report = SAGRUN_NO_KIN; return pts; }
                    var ci = CultureInfo.InvariantCulture;
                    double stdLim = mOn.Fc.StdLimit;
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format(ci, "틈 재기 계획 — 펜 높이 z={0:0.#}, 피치 {1:0.#}, 점 {2} (보정 끔 → 켬: 끝이 dz 만큼 올라가 켬 틈 = 끔 틈 + dz 예상)", cfg.Z, cfg.Pitch, cfg.Pts == null ? 0 : cfg.Pts.Length));
                    if (SagRun_SafeZBelow(arm, cfg.Z + 5))
                        sb.AppendLine(string.Format(ci, "  ★SafeZ {0:0.#} 이 펜 높이 + 5 ({1:0.#}) 보다 낮음 — 옮길 높이가 없어 모든 점 제외 (sagarm.txt 의 safez 확인)", arm.SafeZ, cfg.Z + 5));
                    if (noteDef.Length > 0) sb.AppendLine("  " + noteDef);
                    sb.AppendLine("       x       y   dz 예상    std   비고");
                    int nLow = 0;
                    if (cfg.Pts != null)
                        foreach (float[] xy in cfg.Pts)
                        {
                            if (xy == null || xy.Length < 2) continue;
                            var p = new CSagGapPoint { X = xy[0], Y = xy[1], Z = cfg.Z, Pitch = cfg.Pitch };
                            float[] q; float zT; double dz, sd;
                            if (SagRun_DzAt(kin, mOn, arm, p.X, p.Y, p.Z, p.Pitch, out q, out zT, out dz, out sd))
                            {
                                p.Q = q; p.DzPred = (float)dz; p.Std = (float)sd;
                                p.TransferZ = zT;
                                // 펜 높이는 그냥 내려가는 높이다 (누름 정지 없음) — 그 점의 예측 첫닿음(끔 FK)과 켬에서 닿는 명령 높이(Ref) 위 10mm 는 돼야 한다
                                double fcPred, fcStd; float qs; string whyFc;
                                double touch = SagRun_FcPlanSeeded(kin, mOn, arm, (float)(Math.Atan2(p.Y, p.X) * 180.0 / Math.PI), p.X, p.Y, p.Pitch, out fcPred, out fcStd, out qs, out whyFc)
                                             ? Math.Max(fcPred, mOn.Fc.Ref) : double.NaN;
                                if (float.IsNaN(zT))
                                {
                                    p.IkOk = false;
                                    p.Note = SagRun_SafeZBelow(arm, p.Z + 5) ? string.Format(ci, "IK: 옮길 높이(SafeZ {0:0.#})가 펜 높이 + 5 ({1:0.#}) 보다 낮음", arm.SafeZ, p.Z + 5)
                                                                           : string.Format(ci, "IK: 옮길 높이 없음 (z {0:F0}~{1:F0})", p.Z + 5, arm.SafeZ);
                                }
                                else if (double.IsNaN(touch) || p.Z < touch + SAGRUN_GAP_CLEAR_MM)
                                {
                                    p.IkOk = false; nLow++;
                                    p.Note = double.IsNaN(touch) ? "IK: 이 점의 예측 첫닿음을 못 구함 — 펜 높이를 확인할 수 없어 뺌"
                                                                 : string.Format(ci, "IK: 펜 높이 {0:0.#} 이 예측 닿는 높이 {1:F1} + {2:0} 보다 낮음 — 닿을 수 있어 뺌", p.Z, touch, SAGRUN_GAP_CLEAR_MM);
                                }
                                else if (sd > stdLim) p.Note = string.Format(ci, "넘겨짚기 (std {0:F1})", sd);
                            }
                            else { p.IkOk = false; p.Note = string.Format(ci, "IK: z={0:F1}", p.Z); }
                            pts.Add(p);
                            sb.AppendLine(string.Format(ci, "  {0,6:F1}  {1,6:F1}   {2,7}   {3,5}   {4}", p.X, p.Y,
                                float.IsNaN(p.DzPred) ? "—" : p.DzPred.ToString("+0.00;-0.00", ci), float.IsNaN(p.Std) ? "" : p.Std.ToString("F1", ci), p.Note));
                        }
                    if (nLow > 0) sb.AppendLine(string.Format(ci, "  ★펜 높이 z={0:0.#} 가 예측 닿는 높이 + {1:0} 보다 낮은 점 {2} 뺌 (sagarm.txt 의 gapz 확인)", cfg.Z, SAGRUN_GAP_CLEAR_MM, nLow));
                    var dzs = new List<float>(); foreach (CSagGapPoint p in pts) if (!float.IsNaN(p.DzPred)) dzs.Add(p.DzPred);
                    double mn, mx; SagRun_MinMax(dzs.ToArray(), out mn, out mx);
                    if (!double.IsNaN(mn)) sb.AppendLine(string.Format(ci, "  dz 예상 {0:+0.0;-0.0} ~ {1:+0.0;-0.0}mm (폭 {2:F1}mm) — 합격: 켬 틈이 평균 ±{3:0.#} 안, 변화가 예상 ±{4:0.#} 안",
                        mn, mx, mx - mn, cfg.PassSameMm, cfg.PassPredMm));
                    report = sb.ToString().TrimEnd();
                }
                catch (Exception ex) { report = "★오류: " + SagRun_Msg(ex); }
                return pts;
            }

            /// <summary>틈 재기 판정 — Points 의 GapOff·GapOn·DzPred 로 배열·변화(켬 − 끔)·변화 − 예측을 채우고
            /// Pass = 실물 && 모든 켬 틈이 켬 평균 ±PassSameMm 안 && 모든 |변화 − 예측| ≤ PassPredMm (빈칸은 빼고 "n점만 판정").
            /// 불합격은 한 점으로도 확정한다. 합격은 두 기준을 다 잴 수 있을 때만 — 켬 틈 2점 이상(고르기), 끔·켬 둘 다 잰 점 1점 이상(변화).
            /// 모자라면 판정 불가 (Why 에 까닭 — 한 점의 켬 틈은 고르기를 말해 주지 않는다). 켬 폭은 2점 이상일 때만 (아니면 NaN).
            /// Stopped 면 중단 (3D 만도 — 10-03 검토: 멈춘 3D 회차가 3D만 으로 적혔다), 아니고 Dry 면 판정=3D만. Verdict·Why·Line 을 채운다. 예외 없음.</summary>
            public static void SagRun_GapEvaluate(CSagGapResult r, CSagGapCfg cfg)
            {
                if (r == null) return;
                try
                {
                    if (cfg == null) cfg = new CSagGapCfg();
                    var ci = CultureInfo.InvariantCulture;
                    if (r.Points == null) r.Points = new List<CSagGapPoint>();
                    int n = r.Points.Count;
                    r.GapOff = new float[n]; r.GapOn = new float[n]; r.DzPred = new float[n]; r.Change = new float[n]; r.ChangeErr = new float[n];
                    var on = new List<float>(); double maxErr = double.NaN; int nJ = 0;
                    for (int i = 0; i < n; i++)
                    {
                        CSagGapPoint p = r.Points[i];
                        if (p == null) { r.GapOff[i] = r.GapOn[i] = r.DzPred[i] = r.Change[i] = r.ChangeErr[i] = float.NaN; continue; }   // 빈 칸 (잰 것 없음)
                        r.GapOff[i] = p.GapOff; r.GapOn[i] = p.GapOn; r.DzPred[i] = p.DzPred;
                        r.Change[i] = (float.IsNaN(p.GapOff) || float.IsNaN(p.GapOn)) ? float.NaN : p.GapOn - p.GapOff;
                        r.ChangeErr[i] = (float.IsNaN(r.Change[i]) || float.IsNaN(p.DzPred)) ? float.NaN : r.Change[i] - p.DzPred;
                        if (!float.IsNaN(p.GapOn)) on.Add(p.GapOn);
                        if (!float.IsNaN(r.ChangeErr[i])) { nJ++; maxErr = double.IsNaN(maxErr) ? Math.Abs(r.ChangeErr[i]) : Math.Max(maxErr, Math.Abs(r.ChangeErr[i])); }
                    }
                    r.NJudged = nJ; r.NOn = on.Count; r.MaxChangeErr = maxErr;
                    r.OnMean = r.OnSpread = double.NaN;
                    bool bSame = on.Count > 0;
                    if (on.Count > 0)
                    {
                        double s = 0, lo = double.MaxValue, hi = double.MinValue; foreach (float v in on) { s += v; lo = Math.Min(lo, v); hi = Math.Max(hi, v); }
                        r.OnMean = s / on.Count; r.OnSpread = on.Count >= 2 ? hi - lo : double.NaN;
                        foreach (float v in on) if (Math.Abs(v - r.OnMean) > cfg.PassSameMm) bSame = false;
                    }
                    double mn, mx; SagRun_MinMax(r.DzPred, out mn, out mx);
                    string sPred = double.IsNaN(mn) ? "—" : string.Format(ci, "{0:+0.0;-0.0}~{1:+0.0;-0.0}mm", mn, mx);
                    r.Why = "";
                    if (r.Dry)
                    {
                        r.Pass = false; r.Verdict = r.Stopped ? "중단" : "3D만";
                        r.Line = "3D 틈 · 예상 dz " + sPred + " · 판정=" + r.Verdict;
                        return;
                    }
                    // 불합격은 한 점으로도 확정, 합격은 두 기준을 다 잴 수 있을 때만 (켬 틈 고르기 = 켬 2점 이상, 변화 − 예측 = 끔·켬 1점 이상)
                    bool bSameFail = on.Count >= 2 && !bSame;
                    bool bPredFail = nJ > 0 && maxErr > cfg.PassPredMm;
                    bool bEnough = on.Count >= 2 && nJ > 0;
                    var why = new List<string>();
                    if (bSameFail) why.Add(string.Format(ci, "켬 틈이 켬 평균 ±{0:0.#}mm 밖", cfg.PassSameMm));
                    if (bPredFail) why.Add(string.Format(ci, "변화−예측 {0:F1} > {1:0.#}mm", maxErr, cfg.PassPredMm));
                    if (!bSameFail && !bPredFail && !bEnough)
                    {
                        if (on.Count < 2) why.Add(string.Format(ci, "켬 틈 {0}점 — 고르기는 2점 이상", on.Count));
                        if (nJ == 0) why.Add("끔·켬 둘 다 잰 점 없음 — 변화를 예측과 비교 못 함");
                    }
                    r.Pass = !r.Stopped && !bSameFail && !bPredFail && bEnough;
                    r.Verdict = r.Stopped ? "중단" : (bSameFail || bPredFail ? "불합격" : (bEnough ? "합격" : "판정 불가"));
                    r.Why = r.Stopped ? "" : string.Join(" · ", why.ToArray());
                    r.Line = string.Format(ci, "실물 틈 · 켬 폭 {0} · 변화−예측 최대 {1}{2} · {3}{4}",
                        double.IsNaN(r.OnSpread) ? "—" : r.OnSpread.ToString("F1", ci) + "mm", double.IsNaN(maxErr) ? "—" : maxErr.ToString("F1", ci) + "mm",
                        nJ < n ? string.Format(ci, " ({0}점만 판정)", nJ) : "", r.Verdict, r.Verdict == "판정 불가" && r.Why.Length > 0 ? " (" + r.Why + ")" : "");
                }
                catch (Exception ex) { r.Pass = false; r.Verdict = "판정 불가"; r.Why = "★오류: " + SagRun_Msg(ex); r.Line = "★오류: " + SagRun_Msg(ex); }
            }

            /// <summary>닿기 시험 판정 — 점마다 판정에 쓰는 하강만 (SagRun_ContactStats — 실물은 첫닿음 kind F·H 의 명령 높이 FcCmdZ·FK 높이 FcZ,
            /// 3D 만은 예상 CmdZ. 첫닿음을 못 찾은 하강(C·X·판정 없음)은 빼고 NNoFirstTouch 로 센다, 끝까지 누르지 않은 하강(UserStop)도 뺀다, 안전 정지는 따로 센다):
            /// 끔 예측 오차 = 끔 첫닿음 FK 평균 − PredFc (합격 |오차| ≤ PassPredMm, 반경 ≤ NearRMm 은 PassPredNearMm),
            /// 켬 닿는 명령 높이(점별 첫닿음 명령 높이 평균)의 폭 ≤ PassSpreadMm·Ref 와의 RMS, 끔 폭 (켬 폭 &lt; 끔 폭 이어야 함). 폭은 2점 이상일 때만 (아니면 NaN).
            /// ★10-03 검토: 예전에는 실시간 꺾임점(단단한 정지·검열 하강도)으로 판정해, 맞는 모델도 H 하강 하나로 불합격이 될 수 있었다 (꺾임점 FK 와 예측 차 평균 25mm,
            ///   같은 하강의 첫닿음은 1.2mm) — 측정 데이터셋과 같은 첫닿음 판정을 쓴다.
            /// Pass = 실물 && 켬에서 모든 점 접촉 && 켬 폭 ≤ PassSpreadMm && 켬 폭 &lt; 끔 폭 && 끔 예측 오차 기준.
            /// "켬 접촉 없는 점" = 켬 하강을 했는데 모두 닿지 않은 점 (꺾임·첫닿음 아무것도 없음 — 불합격). 닿았는데 첫닿음을 못 찾은 하강만 있는 점은 그 모드 판정에서 빠진다.
            /// ★10-03 검토: 켬 누름이 아예 돌지 않은 점(펼치기·옮기기 실패, 시작점으로 못 감, 막이로 멈춤)은 '켬 접촉 없는 점' 이 아니다 — 판정하지 못한 점이다.
            /// 계획한 누름(쓸 점 × 끔·켬 × Repeats) 중 돌지 않은 것이 있으면(NUnpressed) 합격·불합격을 내지 않는다 — 판정 불가 (Why 에 그 점, 누른 점에서 본 것도 함께).
            /// 불합격은 한 점으로도 확정 (켬 접촉 없는 점·끔 예측 오차 초과·켬 폭 초과·켬 폭 ≥ 끔 폭). 합격은 기준을 잴 수 있을 때만 —
            /// 2점 이상: 켬·끔 접촉 각 2점 이상 + 끔 예측 오차 1점 이상. 1점 (판정 §7-3 의 안 잰 점 하나씩): 끔 예측 오차와 켬 접촉으로만 (Why 에 표시).
            /// 모자라면 판정 불가 (Why). Stopped 면 중단 (3D 만도), 아니고 Dry 면 판정=3D만 (예상 폭만). 예외 없음.</summary>
            public static void SagRun_TestEvaluate(CSagTestResult r, CSagTestCfg cfg)
            {
                if (r == null) return;
                try
                {
                    if (cfg == null) cfg = new CSagTestCfg();
                    var ci = CultureInfo.InvariantCulture;
                    var mOff = new List<double>(); var mOn = new List<double>(); var fOff = new List<double>(); var fOn = new List<double>(); var pred = new List<double>();
                    var errs = new List<double>();
                    double repOff = double.NaN, repOn = double.NaN, rmsS = 0; int rmsN = 0, nUse = 0, nNoOn = 0;
                    bool bPredOk = true; int nPredErr = 0;
                    int nRep = Math.Max(1, cfg.Repeats), nUnpressed = 0;
                    var lstUnp = new List<string>();                              // 누름이 다 돌지 않은 점 (r…: 끔·켬)
                    r.NOff = r.NOn = r.SafetyStops = 0; r.NNoFirstTouch = 0;
                    if (r.Points == null) r.Points = new List<CSagTestPoint>();
                    foreach (CSagTestPoint p in r.Points)
                    {
                        if (p == null) continue;                                  // 빈 칸 — 판정에서 뺀다
                        if (p.Off != null) foreach (CFloorContact c in p.Off) if (c != null && c.Safety) r.SafetyStops++;
                        if (p.On != null) foreach (CFloorContact c in p.On) if (c != null && c.Safety) r.SafetyStops++;
                        if (p.Skipped) continue;
                        nUse++;
                        // 돈 누름 (끝까지 누른 하강 — 중단 요청으로 끊긴 하강은 작업이 넣지 않는다) — 계획(Repeats)보다 적으면 그 점·모드는 판정하지 못했다
                        int ranOff = SagRun_PressesRan(p.Off), ranOn = SagRun_PressesRan(p.On);
                        if (ranOff < nRep || ranOn < nRep)
                        {
                            nUnpressed += Math.Max(0, nRep - ranOff) + Math.Max(0, nRep - ranOn);
                            lstUnp.Add(string.Format(ci, "r{0:0.#}{1} {2}", p.R, Math.Abs(p.Yaw) > 0.01f ? string.Format(ci, "/요{0:0.#}", p.Yaw) : "",
                                (ranOff < nRep && ranOn < nRep) ? "끔·켬" : (ranOff < nRep ? "끔" : "켬")));
                        }
                        double mo, mn, fo, fn, ro, rn; int no, nn, ftOff, ftOn, airOff, airOn;
                        SagRun_ContactStats(p.Off, out mo, out fo, out ro, out no, out ftOff, out airOff);
                        SagRun_ContactStats(p.On, out mn, out fn, out rn, out nn, out ftOn, out airOn);
                        r.NOff += no; r.NOn += nn; r.NNoFirstTouch += ftOff + ftOn;
                        p.MeanOff = (float)mo; p.MeanOn = (float)mn; p.MeanFkOff = (float)fo; p.MeanFkOn = (float)fn; p.RepeatOff = (float)ro; p.RepeatOn = (float)rn;
                        p.PredErrOff = (float.IsNaN(p.PredFc) || double.IsNaN(fo)) ? float.NaN : (float)(fo - p.PredFc);
                        errs.Add(p.PredErrOff);
                        if (!double.IsNaN(mo)) mOff.Add(mo);
                        if (!double.IsNaN(mn)) { mOn.Add(mn); if (!float.IsNaN(p.Ref)) { rmsS += (mn - p.Ref) * (mn - p.Ref); rmsN++; } }
                        else if (ftOn == 0 && ranOn > 0) nNoOn++;                 // 켬 하강을 했는데 하나도 닿지 않음 (첫닿음을 못 찾은 하강만 있으면 그 점의 켬은 판정에서 빠진다,
                                                                                  //   켬 누름이 돌지 않았으면 판정하지 못한 점 — 아래 NUnpressed)
                        if (!double.IsNaN(fo)) fOff.Add(fo);
                        if (!double.IsNaN(fn)) fOn.Add(fn);
                        if (!float.IsNaN(p.PredFc)) pred.Add(p.PredFc);
                        if (!double.IsNaN(ro)) repOff = double.IsNaN(repOff) ? ro : Math.Max(repOff, ro);
                        if (!double.IsNaN(rn)) repOn = double.IsNaN(repOn) ? rn : Math.Max(repOn, rn);
                        if (!float.IsNaN(p.PredErrOff))
                        {
                            nPredErr++;
                            double tol = p.R <= cfg.NearRMm ? cfg.PassPredNearMm : cfg.PassPredMm;
                            if (Math.Abs(p.PredErrOff) > tol) bPredOk = false;
                        }
                    }
                    r.PredErrOff = errs.ToArray();
                    r.PredErrOffMax = double.NaN; foreach (double e in errs) if (!double.IsNaN(e)) r.PredErrOffMax = double.IsNaN(r.PredErrOffMax) ? Math.Abs(e) : Math.Max(r.PredErrOffMax, Math.Abs(e));
                    r.PredErrPass = bPredOk && nPredErr > 0;
                    r.SpreadOff = SagRun_Spread(mOff); r.SpreadOn = SagRun_Spread(mOn); r.StdOff = SagRun_Std(mOff); r.StdOn = SagRun_Std(mOn);
                    r.FkSpreadOff = SagRun_Spread(fOff); r.FkSpreadOn = SagRun_Spread(fOn);
                    r.RepeatOff = repOff; r.RepeatOn = repOn;
                    r.RmsOnFromRef = rmsN > 0 ? Math.Sqrt(rmsS / rmsN) : double.NaN;
                    r.PredSpreadOff = SagRun_Spread(pred); r.PredSpreadOn = pred.Count >= 2 ? 0 : double.NaN;
                    if (double.IsNaN(r.Ref)) foreach (CSagTestPoint p in r.Points) if (p != null && !float.IsNaN(p.Ref)) { r.Ref = p.Ref; break; }
                    r.NJudged = nUse; r.Why = ""; r.NUnpressed = nUnpressed;
                    if (r.Dry)
                    {
                        // 3D 만 — 멈춘 회차는 중단 (10-03 검토: 3D만 으로 적혀 결과 목록에서 중단이 보이지 않았다)
                        r.Pass = false; r.Verdict = r.Stopped ? "중단" : "3D만";
                        r.Line = string.Format(ci, "3D 닿기 · 예상 끔 폭 {0} → 켬 폭 {1} · 판정={2}",
                            double.IsNaN(r.PredSpreadOff) ? "—" : r.PredSpreadOff.ToString("F1", ci) + "mm", double.IsNaN(r.PredSpreadOn) ? "—" : r.PredSpreadOn.ToString("F1", ci) + "mm", r.Verdict);
                        return;
                    }
                    bool bAllOn = nUse > 0 && nNoOn == 0;
                    // 불합격은 한 점으로도 확정, 합격은 잴 수 있는 기준을 다 잰 뒤에만. 한 점은 판정 §7-3 처럼 끔 예측 오차·켬 접촉으로만 (폭은 2점 이상에서만 뜻이 있다)
                    bool bNoOn = nUse > 0 && nNoOn > 0;
                    bool bPredFail = nPredErr > 0 && !bPredOk;
                    bool bSpreadFail = mOn.Count >= 2 && r.SpreadOn > cfg.PassSpreadMm;
                    bool bOrderFail = mOn.Count >= 2 && mOff.Count >= 2 && !(r.SpreadOn < r.SpreadOff);
                    bool bFail = bNoOn || bPredFail || bSpreadFail || bOrderFail;
                    bool bEnough = nUse == 1 ? (nPredErr > 0 && mOn.Count == 1) : (nUse >= 2 && mOn.Count >= 2 && mOff.Count >= 2 && nPredErr > 0);
                    bool bShort = nUnpressed > 0;                                 // 계획한 누름이 다 돌지 않음 — 합격·불합격을 내지 않는다 (판정 불가)
                    var why = new List<string>();
                    var fails = new List<string>();
                    if (bNoOn) fails.Add(string.Format(ci, "켬 접촉 없는 점 {0}", nNoOn));
                    if (bPredFail) fails.Add(string.Format(ci, "끔 예측 오차 {0:F1}mm — 기준 {1:0.#}mm (반경 {2:0.#} 이하 {3:0.#}mm) 넘음", r.PredErrOffMax, cfg.PassPredMm, cfg.NearRMm, cfg.PassPredNearMm));
                    if (bSpreadFail) fails.Add(string.Format(ci, "켬 폭 {0:F1} > {1:0.#}mm", r.SpreadOn, cfg.PassSpreadMm));
                    if (bOrderFail) fails.Add(string.Format(ci, "켬 폭 {0:F1} ≥ 끔 폭 {1:F1}mm", r.SpreadOn, r.SpreadOff));
                    if (bShort)
                    {
                        // 판정 불가 — 까닭(돌지 않은 점)을 먼저, 누른 점에서 본 불합격 조건은 참고로 (그것으로 불합격을 확정하지 않는다)
                        var lu = new List<string>(); for (int i = 0; i < lstUnp.Count && i < 6; i++) lu.Add(lstUnp[i]);
                        if (lstUnp.Count > 6) lu.Add(string.Format(ci, "외 {0}점", lstUnp.Count - 6));
                        why.Add(string.Format(ci, "누르지 못한 누름 {0} ({1}) — 시험이 계획대로 끝나지 않아 합격·불합격을 내지 않음", nUnpressed, string.Join(", ", lu.ToArray())));
                        if (fails.Count > 0) why.Add("누른 점에서: " + string.Join(", ", fails.ToArray()));
                    }
                    else why.AddRange(fails);
                    if (!bFail && !bEnough)
                    {
                        if (nUse == 0) why.Add("쓸 점 없음");
                        else if (nPredErr == 0) why.Add("끔 첫닿음이 잡힌 점 없음 — 예측 오차를 못 잼");
                        else if (mOff.Count < 2 && nUse >= 2) why.Add("끔 접촉이 1점뿐 — 끔·켬 폭을 비교 못 함");
                        else if (mOn.Count < (nUse == 1 ? 1 : 2)) why.Add("켬 첫닿음이 모자람 — 켬 폭을 못 잼");
                    }
                    if (r.NNoFirstTouch > 0) why.Add(string.Format(ci, "{0} 하강 첫닿음 못 찾음 (C·X — 판정에서 뺌)", r.NNoFirstTouch));
                    if (!bShort && !bFail && bEnough && nUse == 1) why.Add("1점 — 끔 예측 오차·켬 접촉으로만 판정");
                    r.Pass = !r.Stopped && !bShort && !bFail && bEnough;
                    r.Verdict = r.Stopped ? "중단" : (bShort ? "판정 불가" : (bFail ? "불합격" : (bEnough ? "합격" : "판정 불가")));
                    r.Why = r.Stopped ? "" : string.Join(" · ", why.ToArray());
                    r.Line = string.Format(ci, "실물 닿기 · 끔 폭 {0} → 켬 폭 {1} · 끔 예측 오차 최대 {2}{3}{4} · {5}{6}",
                        double.IsNaN(r.SpreadOff) ? "—" : r.SpreadOff.ToString("F1", ci), double.IsNaN(r.SpreadOn) ? "—" : r.SpreadOn.ToString("F1", ci) + "mm",
                        double.IsNaN(r.PredErrOffMax) ? "—" : r.PredErrOffMax.ToString("F1", ci) + "mm", bAllOn ? "" : string.Format(ci, " (켬 접촉 없는 점 {0})", nNoOn),
                        r.NNoFirstTouch > 0 ? string.Format(ci, " · {0} 하강 첫닿음 못 찾음", r.NNoFirstTouch) : "", r.Verdict,
                        (r.Verdict == "판정 불가" || (r.Pass && nUse == 1)) && r.Why.Length > 0 ? " (" + r.Why + ")" : "");
                }
                catch (Exception ex) { r.Pass = false; r.Verdict = "판정 불가"; r.Why = "★오류: " + SagRun_Msg(ex); r.Line = "★오류: " + SagRun_Msg(ex); }
            }
            // 접촉 목록 → 판정에 쓰는 하강의 명령 높이 평균·FK 평균·명령 높이 반복 폭(max − min)·수.
            //   실물 누름(FromPress): 첫닿음 kind F·H 이고 첫닿음 명령 높이가 수인 것만 — (FcCmdZ, FcZ). 첫닿음을 못 찾은 하강은 nNoFt
            //   (C = 단단한 정지·첫닿음 없음, X = 저장된 꺾임을 못 씀 — FcZ 가 있어도 교차 확인용이라 뺀다, 판정 없음),
            //   그중 아무것도 닿지 않은 하강(꺾임 없음·첫닿음 없음·안전 정지 아님)은 nAir 로 따로 센다 (nNoFt 에 넣지 않는다).
            //   3D 만(예상, FromPress = false): Detected 의 (CmdZ, FkZ). 끝까지 누르지 않은 하강(UserStop)은 어디에도 넣지 않는다
            private static void SagRun_ContactStats(List<CFloorContact> l, out double meanCmd, out double meanFk, out double rep, out int n, out int nNoFt, out int nAir)
            {
                meanCmd = meanFk = rep = double.NaN; n = 0; nNoFt = 0; nAir = 0;
                if (l == null) return;
                double sc = 0, sf = 0, lo = double.MaxValue, hi = double.MinValue; int nf = 0;
                foreach (CFloorContact c in l)
                {
                    if (c == null || c.UserStop) continue;
                    float cmd, fk;
                    if (!c.FromPress)
                    {
                        if (!c.Detected || float.IsNaN(c.CmdZ)) continue;
                        cmd = c.CmdZ; fk = c.FkZ;
                    }
                    else if ((c.Kind == "F" || c.Kind == "H") && !float.IsNaN(c.FcCmdZ) && !float.IsInfinity(c.FcCmdZ))
                    {
                        cmd = c.FcCmdZ; fk = c.FcZ;
                    }
                    else
                    {
                        if (!c.Detected && !c.Safety && float.IsNaN(c.FcZ) && (c.Kind == "X" || string.IsNullOrEmpty(c.Kind))) nAir++;
                        else nNoFt++;
                        continue;
                    }
                    sc += cmd; n++; lo = Math.Min(lo, cmd); hi = Math.Max(hi, cmd);
                    if (!float.IsNaN(fk) && !float.IsInfinity(fk)) { sf += fk; nf++; }
                }
                if (n > 0) { meanCmd = sc / n; rep = hi - lo; }
                if (nf > 0) meanFk = sf / nf;
            }
            // 돈 누름 수 — 접촉 목록에 들어 있는 하강 중 끝까지 누른 것 (중단 요청으로 끊긴 하강 UserStop 은 빼고, 닿았든 아니든). 작업은 돈 누름만 목록에 넣는다
            private static int SagRun_PressesRan(List<CFloorContact> l)
            {
                int n = 0;
                if (l != null) foreach (CFloorContact c in l) if (c != null && !c.UserStop) n++;
                return n;
            }
            // 한도까지 내려가도 닿지 않은 실물 누름 — 누름 판정·꺾임 맞춤 모두 없고 안전 정지·중단도 아니다 (SagComp_FloorPress "한도까지 내려가도 누름·꺾임 없음")
            private static bool SagRun_NoTouch(CFloorContact c)
            {
                return c != null && c.FromPress && !c.UserStop && !c.Safety && !c.Detected && !c.FitOk && c.Ticks == 0;
            }
            // 폭 (max − min) — 2점 이상일 때만 (한 점의 폭 0 은 아무것도 말해 주지 않는다), 아니면 NaN
            private static double SagRun_Spread(List<double> v)
            {
                if (v == null || v.Count < 2) return double.NaN;
                double lo = double.MaxValue, hi = double.MinValue; foreach (double x in v) { lo = Math.Min(lo, x); hi = Math.Max(hi, x); }
                return hi - lo;
            }
            private static double SagRun_Std(List<double> v)
            {
                if (v == null || v.Count == 0) return double.NaN;
                double s = 0; foreach (double x in v) s += x; double m = s / v.Count, e = 0; foreach (double x in v) e += (x - m) * (x - m);
                return Math.Sqrt(e / v.Count);
            }

            /// <summary>시험 기록 한 줄 (닿기) — "[닿기] 2026-10-02 17:20 실물 점 5 반복 2 손목유지 끔 | 끔 폭 38.10 → 켬 폭 4.21mm (Ref 34.20, 켬 RMS 1.80, 끔 예측 오차 최대 1.20) | 판정=합격 | <보고서>"</summary>
            public static string SagRun_TestRecordLine(CSagTestResult r, CSagTestCfg cfg)
            {
                if (r == null) return "";
                try
                {
                    if (cfg == null) cfg = new CSagTestCfg();
                    var ci = CultureInfo.InvariantCulture;
                    int nUse = 0; if (r.Points != null) foreach (CSagTestPoint p in r.Points) if (p != null && !p.Skipped) nUse++;
                    Func<double, string> V = d => double.IsNaN(d) ? "—" : d.ToString("F2", ci);
                    Func<double, string> VU = d => double.IsNaN(d) ? "—" : d.ToString("F2", ci) + "mm";    // 단위는 수일 때만 (빈 값 "—mm" 이 아니게)
                    return string.Format(ci, "[닿기] {0} {1} 점 {2} 반복 {3} 손목유지 {4} | 끔 폭 {5} → 켬 폭 {6} (Ref {7}, 켬 RMS {8}, 끔 예측 오차 최대 {9}) | 판정={10} | {11}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm", ci), r.Dry ? "3D" : "실물", nUse, cfg.Repeats, cfg.WristHold ? "켬" : "끔",
                        V(r.Dry ? r.PredSpreadOff : r.SpreadOff), VU(r.Dry ? r.PredSpreadOn : r.SpreadOn), V(r.Ref), V(r.RmsOnFromRef), V(r.PredErrOffMax),
                        string.IsNullOrEmpty(r.Verdict) ? "—" : r.Verdict, r.ReportPath ?? "");
                }
                catch { return ""; }
            }
            /// <summary>시험 기록 한 줄 (틈) — "[틈] 2026-10-02 17:20 실물 점 5 (z 80, 피치 90) | 켬 폭 1.60mm, 변화−예측 최대 1.20mm (5점 판정) | 판정=합격 | <보고서>"</summary>
            public static string SagRun_GapRecordLine(CSagGapResult r, CSagGapCfg cfg)
            {
                if (r == null) return "";
                try
                {
                    if (cfg == null) cfg = new CSagGapCfg();
                    var ci = CultureInfo.InvariantCulture;
                    Func<double, string> V = d => double.IsNaN(d) ? "—" : d.ToString("F2", ci);
                    Func<double, string> VU = d => double.IsNaN(d) ? "—" : d.ToString("F2", ci) + "mm";    // 단위는 수일 때만 (빈 값 "—mm" 이 아니게)
                    double mn, mx; SagRun_MinMax(r.DzPred, out mn, out mx);
                    return string.Format(ci, "[틈] {0} {1} 점 {2} (z {3:0.#}, 피치 {4:0.#}) | {5} | 판정={6} | {7}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm", ci), r.Dry ? "3D" : "실물", r.Points != null ? r.Points.Count : 0, cfg.Z, cfg.Pitch,
                        r.Dry ? (double.IsNaN(mn) ? "예상 dz —" : string.Format(ci, "예상 dz {0} ~ {1}", V(mn), VU(mx)))
                              : string.Format(ci, "켬 폭 {0}, 변화−예측 최대 {1} ({2}점 판정)", VU(r.OnSpread), VU(r.MaxChangeErr), r.NJudged),
                        string.IsNullOrEmpty(r.Verdict) ? "—" : r.Verdict, r.ReportPath ?? "");
                }
                catch { return ""; }
            }
            /// <summary>결과 파일 옆 시험 기록(<결과>_test.txt)에 한 줄 덧붙인다 (UTF-8 BOM 없음, 기록 끝에 줄바꿈이 없으면 줄을 바꿔서). 반환 = 기록 파일 경로 (실패하면 null)</summary>
            public static string SagRun_AppendTestLine(string strResultPath, string strLine)
            {
                try
                {
                    string p = SagRun_TestRecordPath(strResultPath);
                    if (string.IsNullOrEmpty(p) || string.IsNullOrEmpty(strLine)) return null;
                    SagRun_AppendLine(p, strLine);
                    return p;
                }
                catch { return null; }
            }

            // ────────────────────────────────────────────────────────────
            // ⑥ 파일 목록·적용·되돌리기·쌍둥이
            // ────────────────────────────────────────────────────────────

            /// <summary>결과 파일 목록 — [0] = 적용 중 파일(있으면), 이어서 sagcomp_results\sagcomp_model_<yyyyMMdd_HHmm>[_n].txt 최신순.
            /// Display = "20261002_1830 · 첫닿음 73점 · 시험: 틈 합격 / 닿기 —" (첫닿음 모델이 없고 GP 만 있으면 "GP n점"),
            /// 시험 상태는 <결과>_test.txt 의 [틈]·[닿기] 마다 마지막 실물 판정 (합격·불합격, 없으면 실물 판정 불가) — 그 뒤에 온 3D만·중단 줄은 "(뒤에 3D만)" 처럼
            /// 덧붙이기만 한다 (실물 '불합격' 을 가리지 않는다. 실물 판정이 없으면 마지막 줄의 판정). GapVerdict·TouchVerdict 에 그 실물 판정.
            /// 지난 적용·되돌리기가 남긴 임시 파일(1분 넘은 것)도 치운다. 예외 없음.</summary>
            public static List<CSagFileInfo> SagRun_ListResults(string strUrdfDir)
            {
                var lst = new List<CSagFileInfo>();
                try
                {
                    string active = SagRun_ActivePath(strUrdfDir);
                    if (!string.IsNullOrEmpty(strUrdfDir)) SagRun_SweepStage(active);
                    byte[] activeBytes = File.Exists(active) ? File.ReadAllBytes(active) : null;
                    if (activeBytes != null)
                    {
                        CSagFileInfo fi = SagRun_FileInfo(active);
                        fi.IsActive = true; fi.Time = File.GetLastWriteTime(active);
                        fi.Display = "지금 적용 중 — " + SAGRUN_ACTIVE + " (" + fi.Brief + " · 시험: " + fi.TestState + ")";
                        lst.Add(fi);
                    }
                    string dir = SagRun_ResultsDir(strUrdfDir);
                    if (!Directory.Exists(dir)) return lst;
                    var re = new Regex(@"^sagcomp_model_(\d{8}_\d{4})(?:_(\d+))?\.txt$", RegexOptions.IgnoreCase);
                    var res = new List<CSagFileInfo>(); var ord = new List<int>();
                    foreach (string p in Directory.GetFiles(dir, "sagcomp_model_*.txt"))
                    {
                        try
                        {
                            Match mt = re.Match(Path.GetFileName(p));
                            if (!mt.Success) continue;
                            CSagFileInfo fi = SagRun_FileInfo(p);
                            DateTime t;
                            fi.Time = DateTime.TryParseExact(mt.Groups[1].Value, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t) ? t : File.GetLastWriteTime(p);
                            int n;                                                  // 뒤 번호 — int 를 넘는 이름(손으로 붙인 시각 등)은 순서만 맨 뒤로 (목록을 비우지 않게)
                            if (!mt.Groups[2].Success) n = 1;
                            else if (!int.TryParse(mt.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out n)) n = int.MaxValue;
                            fi.SameAsActive = activeBytes != null && SagRun_SameBytes(p, activeBytes);
                            fi.Display = mt.Groups[1].Value + (n > 1 ? "_" + mt.Groups[2].Value : "") + " · " + fi.Brief + " · 시험: " + fi.TestState + (fi.SameAsActive ? " · 적용 중" : "");
                            int k = 0; while (k < res.Count && (res[k].Time > fi.Time || (res[k].Time == fi.Time && ord[k] > n))) k++;
                            res.Insert(k, fi); ord.Insert(k, n);
                        }
                        catch { }                                                   // 파일 하나가 이상해도 나머지는 보인다
                    }
                    lst.AddRange(res);
                }
                catch { }
                return lst;
            }
            /// <summary>백업 목록 — sagcomp_backup\sagcomp_model_<yyyyMMdd_HHmmss>[_n].txt + 옛 <urdf>\sagcomp_model_*_backup.txt, 최신순. 예외 없음.</summary>
            public static List<CSagFileInfo> SagRun_ListBackups(string strUrdfDir)
            {
                var lst = new List<CSagFileInfo>();
                try
                {
                    var re = new Regex(@"^sagcomp_model_(\d{8}_\d{6})(?:_(\d+))?\.txt$", RegexOptions.IgnoreCase);
                    string dir = SagRun_BackupDir(strUrdfDir);
                    if (Directory.Exists(dir))
                        foreach (string p in Directory.GetFiles(dir, "sagcomp_model_*.txt"))
                        {
                            try
                            {
                                Match mt = re.Match(Path.GetFileName(p));
                                if (!mt.Success) continue;
                                CSagFileInfo fi = SagRun_FileInfo(p);
                                DateTime t;
                                fi.Time = DateTime.TryParseExact(mt.Groups[1].Value, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out t) ? t : File.GetLastWriteTime(p);
                                int n;                                              // 같은 초 안의 순서 (int 를 넘는 번호는 999 로)
                                if (mt.Groups[2].Success) fi.Time = fi.Time.AddMilliseconds(int.TryParse(mt.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out n) ? Math.Min(999, n) : 999);
                                fi.Display = fi.Time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + (mt.Groups[2].Success ? " (" + mt.Groups[2].Value + ")" : "") + " · " + fi.Brief;
                                lst.Add(fi);
                            }
                            catch { }
                        }
                    if (!string.IsNullOrEmpty(strUrdfDir) && Directory.Exists(strUrdfDir))
                        foreach (string p in Directory.GetFiles(strUrdfDir, "sagcomp_model_*_backup.txt"))
                        {
                            try
                            {
                                CSagFileInfo fi = SagRun_FileInfo(p);
                                fi.Time = File.GetLastWriteTime(p);
                                fi.Display = "옛 백업 " + fi.Name + " · " + fi.Brief;
                                lst.Add(fi);
                            }
                            catch { }
                        }
                    lst.Sort(delegate(CSagFileInfo a, CSagFileInfo b) { return b.Time.CompareTo(a.Time); });
                }
                catch { }
                return lst;
            }
            // 파일 한 줄 정보 — 모델 내용 요약 (첫닿음 n점 / GP n점 / 보정표 / 관절 모델만), 매핑, 시험 상태
            private static CSagFileInfo SagRun_FileInfo(string strPath)
            {
                var fi = new CSagFileInfo { Path = strPath, Name = Path.GetFileName(strPath) };
                try
                {
                    CSagComp m = CSagComp.Load(strPath);
                    fi.Brief = SagRun_ModelBrief(m);
                    if (m != null) { fi.MotorMap = m.MotorMap; fi.NFc = m.HasFc ? m.Fc.Count : 0; fi.NGp = m.HasGp ? m.Gp.Count : 0; }
                }
                catch { fi.Brief = "읽기 실패"; }
                string gv, tv;
                fi.TestState = SagRun_TestStateEx(SagRun_TestRecordPath(strPath), out gv, out tv);
                fi.GapVerdict = gv; fi.TouchVerdict = tv;
                return fi;
            }
            /// <summary>모델 요약 한 토막 — "첫닿음 73점" / "GP 87점" / "보정표 7점" / "관절 모델만" / "직교 모델" / "읽기 실패" (+ 꺼짐 표시)</summary>
            public static string SagRun_ModelBrief(CSagComp m)
            {
                var ci = CultureInfo.InvariantCulture;
                if (m == null) return "읽기 실패";
                string s;
                if (m.HasFc) s = string.Format(ci, "첫닿음 {0}점{1}", m.Fc.Count, m.Fc.Enabled ? "" : " (fcon=0)");
                else if (m.HasGp) s = string.Format(ci, "GP {0}점", m.Gp.Count);
                else if (m.HasFloorMap) s = string.Format(ci, "보정표 {0}점", m.FloorR.Length);
                else if (m.HasJoint) s = "관절 모델만";
                else if (m.IsUsable) s = "직교 모델";
                else return "쓸 수 없는 모델";
                if ((m.HasFc || m.HasGp || m.HasFloorMap) && !m.FloorMapEnabled) s += " [바닥 끔]";
                if (m.FcDropped) s += " [첫닿음 줄 버림]";
                return s;
            }
            // 시험 기록 파일의 [틈]·[닿기] 상태 — "틈 합격 (뒤에 3D만) / 닿기 불합격" (옛 형식의 머리표 없는 줄은 닿기로 본다).
            //   종류마다 마지막 '실물' 판정(합격·불합격 — 없으면 실물 판정 불가)을 보인다. 그 뒤에 온 줄(3D만·중단·실물 판정 불가)은 '(뒤에 …)' 로만 붙인다
            //   — ★10-03 검토: 예전에는 마지막 줄 그대로라 뒤에 돌린 3D 미리보기·멈춘 회차가 실물 '불합격' 을 가렸다 (적용 전에 보는 목록).
            //   실물 판정이 하나도 없으면 마지막 줄의 판정 (3D만·중단 …). gapV·touchV = 그 종류의 마지막 실물 판정 (없으면 "") — SagRun_TestStateEx
            private static string SagRun_TestState(string strRecordPath)
            {
                string gv, tv; return SagRun_TestStateEx(strRecordPath, out gv, out tv);
            }
            private static string SagRun_TestStateEx(string strRecordPath, out string gapV, out string touchV)
            {
                string[] dec = new string[2], und = new string[2], last = new string[2];   // 종류마다: 마지막 실물 합격·불합격 / 실물 판정 불가 / 마지막 줄의 판정
                int[] iDec = { -1, -1 }, iUnd = { -1, -1 }, iLast = { -1, -1 };
                try
                {
                    if (!string.IsNullOrEmpty(strRecordPath) && File.Exists(strRecordPath))
                    {
                        int n = 0;
                        foreach (string raw in File.ReadAllLines(strRecordPath, Encoding.UTF8))
                        {
                            string ln = raw.Trim(); if (ln.Length == 0) continue;
                            string v = SagRun_VerdictOf(ln); if (v == null) continue;
                            int k = ln.StartsWith("[틈]") ? 0 : 1;
                            bool bReal = SagRun_IsRealRecord(ln);
                            if (bReal && (v == "합격" || v == "불합격")) { dec[k] = v; iDec[k] = n; }
                            else if (bReal && v == "판정 불가") { und[k] = v; iUnd[k] = n; }
                            last[k] = v; iLast[k] = n;
                            n++;
                        }
                    }
                }
                catch { }
                string[] s = new string[2];
                for (int k = 0; k < 2; k++)
                {
                    int iMain = iDec[k] >= 0 ? iDec[k] : iUnd[k];
                    string main = iDec[k] >= 0 ? dec[k] : und[k];
                    if (main == null) s[k] = last[k] ?? "—";
                    else s[k] = main + (iLast[k] > iMain ? " (뒤에 " + last[k] + ")" : "");
                }
                gapV = dec[0] ?? (und[0] ?? ""); touchV = dec[1] ?? (und[1] ?? "");
                return "틈 " + s[0] + " / 닿기 " + s[1];
            }
            // 실물 회차의 기록 줄인가 — 첫 칸("[틈] 날짜 시각 실물 …", 옛 형식은 머리표 없이)에 '실물' 이 있다 (3D 만은 "3D")
            private static bool SagRun_IsRealRecord(string ln)
            {
                int e = ln.IndexOf(" | ");
                string head = e >= 0 ? ln.Substring(0, e) : ln;
                return head.StartsWith("실물 ") || head.IndexOf(" 실물 ") >= 0;
            }
            private static string SagRun_VerdictOf(string ln)
            {
                int i = ln.IndexOf("판정=");
                if (i < 0) return null;
                string v = ln.Substring(i + 3);
                int e = v.IndexOf(" |"); if (e >= 0) v = v.Substring(0, e);
                v = v.Trim();
                return v.Length > 0 ? v : null;
            }
            private static bool SagRun_SameBytes(string strPath, byte[] b)
            {
                try
                {
                    var fi = new FileInfo(strPath);
                    if (fi.Length != b.Length) return false;
                    byte[] a = File.ReadAllBytes(strPath);
                    for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
                    return true;
                }
                catch { return false; }
            }

            /// <summary>두 모터 매핑 키의 차이 문구 — "ID 13 영점 2048→2131, ID 12 방향 정→역". 같으면 "".
            /// 키 형식 "축:방향:영점:틱/회전:각/회전|…". 파일 키가 비었으면(키 없는 옛 파일) 지금 키의 축들의 기본 매핑(0:2048:4096:360)으로 본다.
            /// 지금 키가 비었으면 비교하지 않는다 (""). 예외 없음.</summary>
            public static string SagRun_MotorMapDiff(string strFileKey, string strNowKey)
            {
                try
                {
                    if (string.IsNullOrEmpty(strNowKey)) return "";
                    Dictionary<int, double[]> now = SagRun_ParseMap(strNowKey), old;
                    if (now.Count == 0) return (strFileKey ?? "") == strNowKey ? "" : "지금 매핑 키를 읽지 못함";
                    if (string.IsNullOrEmpty(strFileKey))
                    {
                        old = new Dictionary<int, double[]>();
                        foreach (int ax in now.Keys) old[ax] = new double[] { 0, 2048, 4096, 360 };
                    }
                    else
                    {
                        old = SagRun_ParseMap(strFileKey);
                        if (old.Count == 0) return "파일의 매핑 키를 읽지 못함";
                    }
                    var axes = new List<int>(now.Keys); foreach (int ax in old.Keys) if (!axes.Contains(ax)) axes.Add(ax);
                    axes.Sort();
                    var ci = CultureInfo.InvariantCulture;
                    var lst = new List<string>();
                    string[] names = { "방향", "영점", "틱/회전", "각/회전" };
                    foreach (int ax in axes)
                    {
                        double[] o, n;
                        bool bO = old.TryGetValue(ax, out o), bN = now.TryGetValue(ax, out n);
                        if (!bO) { lst.Add("ID " + ax.ToString(ci) + " 새로 생김"); continue; }
                        if (!bN) { lst.Add("ID " + ax.ToString(ci) + " 없어짐"); continue; }
                        for (int k = 0; k < 4; k++)
                            if (Math.Abs(o[k] - n[k]) > 1e-6)
                                lst.Add(string.Format(ci, "ID {0} {1} {2}→{3}", ax, names[k], k == 0 ? (o[k] == 0 ? "정" : "역") : o[k].ToString("0.###", ci), k == 0 ? (n[k] == 0 ? "정" : "역") : n[k].ToString("0.###", ci)));
                    }
                    return string.Join(", ", lst.ToArray());
                }
                catch { return (strFileKey ?? "") == (strNowKey ?? "") ? "" : "매핑 키를 비교하지 못함"; }
            }
            private static Dictionary<int, double[]> SagRun_ParseMap(string strKey)
            {
                var d = new Dictionary<int, double[]>();
                if (string.IsNullOrEmpty(strKey)) return d;
                var ci = CultureInfo.InvariantCulture;
                foreach (string s in strKey.Split('|'))
                {
                    string[] p = s.Split(':');
                    int ax; if (p.Length < 5 || !int.TryParse(p[0].Trim(), NumberStyles.Integer, ci, out ax)) continue;
                    var v = new double[4]; bool ok = true;
                    for (int k = 0; k < 4; k++) if (!double.TryParse(p[k + 1].Trim(), NumberStyles.Float, ci, out v[k])) ok = false;
                    if (ok) d[ax] = v;
                }
                return d;
            }

            /// <summary>결과 파일 적용 (UI 스레드에서) — 1. 작업 중이면 거절, 결과 파일을 읽어 쓸 수 있는지(관절 모델, GP·첫닿음 줄이 있으면 그 모델까지) 확인.
            /// 2. 적용 중 파일과 관절 구성(func·posids·wrist·jids)이 다르면 거절 (적용 중 파일에 관절 모델이 없으면 — 비었거나 깨졌거나 직교 모델 — 비교 없이 경고만).
            /// 3. strMotorMapKey 가 있으면 파일의 motormap 과 비교 — 다르면 경고만.
            /// 4. 결과를 urdf 폴더의 임시 파일로 복사해 바이트 확인 (읽다 실패하면 아무것도 바뀌지 않는다).
            /// 5. 적용 중 파일 → sagcomp_backup\sagcomp_model_<yyyyMMdd_HHmmss>[_n].txt (시험 기록 sagcomp_model_test.txt 도 같은 이름 _test.txt 로).
            /// 6. 임시 파일을 적용 중 파일 자리로 바꿔 끼움 (File.Replace — 옛 파일을 먼저 지우지 않는다. 다른 프로그램이 임시 파일·적용 중 파일을 잡고 있어
            ///    약 1초 다시 해 봐도 안 되면 바꾸지 않고 실패 — 적용 중 파일·메모리 모델 그대로), 결과의 _test.txt → sagcomp_model_test.txt (없으면 지움).
            /// 7. SagComp_Load (1초 페이드, 쌍둥이도) — 바꿔 끼운 파일을 다른 프로그램이 잠깐 잡고 있으면 200ms 뒤 다시 (모두 6번).
            ///    끝내 못 읽으면 같은 바이트인 결과 파일에서 읽는다 (4 에서 확인한 바이트와 같을 때만 — SagRun_LoadSourceRetry): 성공이고 디스크(새 파일)와
            ///    메모리(같은 내용)가 같다 — report·apply_log 에 "같은 내용의 결과 파일에서 읽음" (10-03 검토: 예전에는 잠긴 새 파일 때문에 되돌리다 그것도 막혀
            ///    디스크 = 새 파일, 메모리 = 이전 모델로 남았다).
            ///    그것도 안 되면 백업으로 되돌려 놓는다 — 적용 중 파일이 반만 쓰인 채 남지 않고, 디스크(이전 파일)와 메모리(이전 모델)가 같다.
            ///    되돌리기도 막히면 report 와 apply_log 가 지금 디스크의 적용 중 파일이 무엇인지(새 파일 (… 바꿔 끼움은 끝났고 되돌리기는 막힘) / 이전 파일 / 없음)와
            ///    메모리 모델(이전 모델 — 앱을 다시 켜면 디스크의 새 파일이 읽힘)을 알리고 백업 경로를 준다.
            /// 8. sagcomp_results\apply_log.txt 한 줄 (못 적으면 report 에 표시) — 실패도 적는다. report = 이전 → 지금 요약 (+ 경고).
            /// 지난 적용이 남긴 임시 파일(1분 넘은 것)은 시작 때 치운다. urdf 폴더가 없으면 거절. 예외 없음.</summary>
            public bool SagRun_Activate(string strResultPath, string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                using (SagRun_KinUse()) return SagRun_ActivateBody(strResultPath, strUrdfDir, strMotorMapKey, log, out strBackupPath, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private bool SagRun_ActivateBody(string strResultPath, string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                strBackupPath = null; report = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                string active = SagRun_ActivePath(strUrdfDir), activeTest = SagRun_TestRecordPath(active);
                string bak = null, tmp = null; bool bTouched = false, bHadActive = false, bSwapped = false;
                CSagComp mMemBefore = m_SagComp;                                      // 실패할 때 메모리 모델이 그대로인지 알리려고
                try
                {
                    if (SagRun_IsBusy) return SagRun_Refuse(out report, "다른 로봇팔 작업이 진행 중 — 끝난 뒤에 적용하십시오", L);
                    if (string.IsNullOrEmpty(strUrdfDir) || !Directory.Exists(strUrdfDir)) return SagRun_Refuse(out report, "urdf 폴더 없음: " + strUrdfDir, L);
                    SagRun_SweepStage(active);
                    if (string.IsNullOrEmpty(strResultPath) || !File.Exists(strResultPath)) return SagRun_Refuse(out report, "결과 파일 없음: " + strResultPath, L);
                    if (SagRun_SamePath(strResultPath, active)) return SagRun_Refuse(out report, "이미 적용 중인 파일", L);
                    CSagComp mNew = CSagComp.Load(strResultPath);
                    string why;
                    if (!SagRun_CheckUsable(mNew, strResultPath, out why)) return SagRun_Refuse(out report, "적용하지 않음 — " + why, L);
                    // 로봇 정의 (10-04) — 결과 모델을 만든 기구학(kinsig, 빈 값 = 내장 OMX)이 지금 3D 의 그 수식과 다르면 적용하지 않는다 (다른 DH 로 학습한 보정)
                    CSagRunKin stA = SagRun_KinState();
                    if (stA != null)
                    {
                        string kNow = SagRun_KinSig(stA, mNew.FuncNumber), kNew = SagRun_KinSigNorm(mNew.KinSig);
                        if (kNow.Length == 0 || !string.Equals(kNow, kNew, StringComparison.Ordinal))
                            return SagRun_Refuse(out report, "다른 기구학으로 학습한 모델 — 지금 3D 에 맞지 않음 (모델 " + (kNew.Length > 0 ? kNew : "?") + " ≠ 지금 3D " + (kNow.Length > 0 ? kNow : "없음") + ") — 적용하지 않음", L);
                    }
                    bHadActive = File.Exists(active);
                    CSagComp mOld = bHadActive ? CSagComp.Load(active) : null;
                    // 관절 구성 비교는 적용 중 파일에 관절 모델이 있을 때만 — 비었거나 깨졌거나 직교 모델이면 비교할 것이 없다 (막으면 고칠 길이 없다): 경고만, 이전 파일은 백업
                    if (mOld != null && mOld.HasJoint && !SagRun_SameJoints(mOld, mNew, out why)) return SagRun_Refuse(out report, "다른 관절 구성 — 적용하지 않음 (" + why + ")", L);
                    var warns = new List<string>();
                    if (bHadActive && (mOld == null || !mOld.HasJoint))
                        warns.Add("★지금 적용 중 파일에 관절 모델이 없음 (" + SagRun_ModelBrief(mOld) + ") — 관절 구성 비교 없이 적용, 이전 파일은 백업");
                    if (!string.IsNullOrEmpty(strMotorMapKey))
                    {
                        string d = SagRun_MotorMapDiff(mNew.MotorMap, strMotorMapKey);
                        if (d.Length > 0) warns.Add("★학습 때와 방향·영점이 다름 (" + d + ") — 다시 학습 권장");
                    }
                    string warn = string.Join(" · ", warns.ToArray());
                    string oldSummary = mOld != null ? mOld.Summary() : "(적용 중 파일 없음)";
                    byte[] staged;
                    tmp = SagRun_StageCopyBytes(strResultPath, active, out staged);      // 먼저 옆에 복사·확인 — 원본을 읽다 실패해도 적용 중 파일은 그대로
                    if (bHadActive) bak = SagRun_BackupActive(strUrdfDir, active, activeTest);
                    strBackupPath = bak;
                    bTouched = true;                                               // 여기부터 실패하면 백업으로 되돌린다
                    string whySwap;
                    if (!SagRun_SwapIn(tmp, active, out whySwap))
                    {
                        bTouched = false;                                          // 바꿔 끼우지 않았다 — 되돌릴 것 없음
                        report = "★오류: " + whySwap + " — 적용 중 파일은 그대로"; L(report); return false;
                    }
                    bSwapped = true;                                               // 디스크의 적용 중 파일 = 새 파일 (되돌리기 전까지)
                    string resTest = SagRun_TestRecordPath(strResultPath);
                    if (File.Exists(resTest)) File.Copy(resTest, activeTest, true); else if (File.Exists(activeTest)) File.Delete(activeTest);
                    string whyLoad, fromSrc = "";
                    if (!SagRun_LoadActiveRetry(active, out whyLoad))
                    {
                        // 적용 중 파일을 다른 프로그램(검사기 등)이 잡고 있어 다시 읽지 못함 — 바꿔 끼운 것과 같은 바이트인 결과 파일에서 읽는다 (디스크 = 새 파일, 메모리 = 같은 내용)
                        string whySrc;
                        if (SagRun_LoadSourceRetry(strResultPath, staged, out whySrc))
                            fromSrc = "적용 중 파일을 다시 읽지 못해 (" + whyLoad + ") 같은 내용의 결과 파일에서 읽음 — 디스크·메모리 같음";
                        else
                        {
                            string tail = SagRun_Recover(strUrdfDir, active, activeTest, bak, bHadActive, strResultPath, mMemBefore, true);
                            bTouched = false;
                            SagRun_ApplyLog(strUrdfDir, "적용 실패 " + Path.GetFileName(strResultPath) + " — 다시 읽지 못함 (" + whyLoad + ", 결과 파일에서도: " + whySrc + ")" + tail);
                            return SagRun_Refuse(out report, "적용한 파일을 다시 읽지 못함 (" + whyLoad + ", 결과 파일에서도: " + whySrc + ")" + tail, L);
                        }
                    }
                    bTouched = false;
                    bool bLog = SagRun_ApplyLog(strUrdfDir, "적용 " + Path.GetFileName(strResultPath) + (bak != null ? " (백업 " + Path.GetFileName(bak) + ")" : " (이전 파일 없음)")
                                                             + (fromSrc.Length > 0 ? " — " + fromSrc : "") + (warn.Length > 0 ? " " + warn : ""));
                    CSagComp mNow = SagComp_Get();
                    report = "적용: " + strResultPath + (bak != null ? "\r\n백업: " + bak : "") + "\r\n이전: " + oldSummary + "\r\n지금: " + (mNow != null ? mNow.Summary() : "")
                           + (fromSrc.Length > 0 ? "\r\n★" + fromSrc : "") + (warn.Length > 0 ? "\r\n" + warn : "") + (bLog ? "" : "\r\n" + SAGRUN_NO_APPLYLOG);
                    L("적용 — " + Path.GetFileName(strResultPath) + (bak != null ? " (이전 파일은 " + Path.GetFileName(bak) + " 로 백업)" : ""));
                    if (fromSrc.Length > 0) L("★" + fromSrc);
                    if (warn.Length > 0) L(warn);
                    if (!bLog) L(SAGRUN_NO_APPLYLOG);
                    return true;
                }
                catch (Exception ex)
                {
                    string tail = bTouched ? SagRun_Recover(strUrdfDir, active, activeTest, bak, bHadActive, strResultPath, mMemBefore, bSwapped) : " — 적용 중 파일은 그대로";
                    if (bTouched) SagRun_ApplyLog(strUrdfDir, "적용 실패 " + SagRun_FileName(strResultPath) + " — " + SagRun_Msg(ex) + tail);
                    report = "★오류: " + SagRun_Msg(ex) + tail; L(report); return false;
                }
                finally { SagRun_DeleteRetry(tmp); }
            }
            /// <summary>적용 (매핑 비교 없이)</summary>
            public bool SagRun_Activate(string strResultPath, string strUrdfDir, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                return SagRun_Activate(strResultPath, strUrdfDir, null, log, out strBackupPath, out report);
            }

            /// <summary>되돌리기 (UI 스레드에서) — 고른 백업을 임시 파일로 복사·확인 → 지금 적용 중 파일도 백업 (되돌리기도 되돌릴 수 있다)
            /// → 임시 파일을 적용 중 파일 자리로 (백업의 _test.txt 가 있으면 시험 기록도) → SagComp_Load → apply_log.
            /// 바꿔 끼우기를 끝내 못 하면 그대로 실패. 바꿔 끼운 적용 중 파일을 다시 읽지 못하면 같은 바이트인 고른 백업에서 읽고 (성공 — 디스크·메모리 같음),
            /// 그것도 안 되면 방금 만든 백업으로 되돌린다 (적용과 같음). 관절 구성이 다르면 경고만. 예외 없음.</summary>
            public bool SagRun_Rollback(string strBackupPath, string strUrdfDir, CSim2Real.DLog log, out string report)
            {
                using (SagRun_KinUse()) return SagRun_RollbackBody(strBackupPath, strUrdfDir, log, out report);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private bool SagRun_RollbackBody(string strBackupPath, string strUrdfDir, CSim2Real.DLog log, out string report)
            {
                report = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                string active = SagRun_ActivePath(strUrdfDir), activeTest = SagRun_TestRecordPath(active);
                string bak = null, tmp = null; bool bTouched = false, bHadActive = false, bSwapped = false;
                CSagComp mMemBefore = m_SagComp;
                try
                {
                    if (SagRun_IsBusy) return SagRun_Refuse(out report, "다른 로봇팔 작업이 진행 중 — 끝난 뒤에 되돌리십시오", L);
                    if (string.IsNullOrEmpty(strUrdfDir) || !Directory.Exists(strUrdfDir)) return SagRun_Refuse(out report, "urdf 폴더 없음: " + strUrdfDir, L);
                    SagRun_SweepStage(active);
                    if (string.IsNullOrEmpty(strBackupPath) || !File.Exists(strBackupPath)) return SagRun_Refuse(out report, "백업 파일 없음: " + strBackupPath, L);
                    if (SagRun_SamePath(strBackupPath, active)) return SagRun_Refuse(out report, "적용 중 파일 자체 — 백업을 고르십시오", L);
                    CSagComp mBak = CSagComp.Load(strBackupPath);
                    if (mBak == null || !mBak.IsUsable) return SagRun_Refuse(out report, "백업 파일을 쓸 수 없음 (보상 모델 없음)", L);
                    bHadActive = File.Exists(active);
                    CSagComp mOld = bHadActive ? CSagComp.Load(active) : null;
                    string warn = "", why;
                    if (mOld != null && !SagRun_SameJoints(mOld, mBak, out why)) warn = "★지금과 다른 관절 구성으로 되돌림 (" + why + ")";
                    CSagRunKin stB = SagRun_KinState();                              // 되돌리기는 경고만 — 지금 3D 와 다른 기구학으로 학습한 백업
                    if (stB != null)
                    {
                        string kNow = SagRun_KinSig(stB, mBak.FuncNumber), kBak = SagRun_KinSigNorm(mBak.KinSig);
                        if (kNow.Length == 0 || !string.Equals(kNow, kBak, StringComparison.Ordinal))
                            warn = SagRun_WhyJoin(warn, "★지금 3D 와 다른 기구학으로 학습한 백업 (" + (kBak.Length > 0 ? kBak : "?") + " ≠ " + (kNow.Length > 0 ? kNow : "없음") + ") — 다시 학습 권장");
                    }
                    string oldSummary = mOld != null ? mOld.Summary() : "(적용 중 파일 없음)";
                    byte[] staged;
                    tmp = SagRun_StageCopyBytes(strBackupPath, active, out staged);      // 먼저 옆에 복사·확인 — 백업을 읽다 실패해도 적용 중 파일은 그대로
                    if (bHadActive) bak = SagRun_BackupActive(strUrdfDir, active, activeTest);
                    bTouched = true;                                               // 여기부터 실패하면 방금 만든 백업으로 되돌린다
                    string whySwap;
                    if (!SagRun_SwapIn(tmp, active, out whySwap))
                    {
                        bTouched = false;                                          // 바꿔 끼우지 않았다 — 되돌릴 것 없음
                        report = "★오류: " + whySwap + " — 적용 중 파일은 그대로"; L(report); return false;
                    }
                    bSwapped = true;                                               // 디스크의 적용 중 파일 = 고른 백업 (되돌리기 전까지)
                    string bakTest = SagRun_TestRecordPath(strBackupPath);
                    if (File.Exists(bakTest)) File.Copy(bakTest, activeTest, true); else if (File.Exists(activeTest)) File.Delete(activeTest);
                    string whyLoad, fromSrc = "";
                    if (!SagRun_LoadActiveRetry(active, out whyLoad))
                    {
                        // 적용 중 파일이 잠겨 다시 읽지 못함 — 바꿔 끼운 것과 같은 바이트인 고른 백업에서 읽는다 (디스크·메모리 같음)
                        string whySrc;
                        if (SagRun_LoadSourceRetry(strBackupPath, staged, out whySrc))
                            fromSrc = "적용 중 파일을 다시 읽지 못해 (" + whyLoad + ") 같은 내용의 고른 백업에서 읽음 — 디스크·메모리 같음";
                        else
                        {
                            string tail = SagRun_Recover(strUrdfDir, active, activeTest, bak, bHadActive, strBackupPath, mMemBefore, true);
                            bTouched = false;
                            SagRun_ApplyLog(strUrdfDir, "되돌리기 실패 " + Path.GetFileName(strBackupPath) + " — 다시 읽지 못함 (" + whyLoad + ", 고른 백업에서도: " + whySrc + ")" + tail);
                            return SagRun_Refuse(out report, "되돌린 파일을 다시 읽지 못함 (" + whyLoad + ", 고른 백업에서도: " + whySrc + ")" + tail, L);
                        }
                    }
                    bTouched = false;
                    bool bLog = SagRun_ApplyLog(strUrdfDir, "되돌리기 " + Path.GetFileName(strBackupPath) + (bak != null ? " (지금 파일 백업 " + Path.GetFileName(bak) + ")" : "")
                                                             + (fromSrc.Length > 0 ? " — " + fromSrc : "") + (warn.Length > 0 ? " " + warn : ""));
                    CSagComp mNow = SagComp_Get();
                    report = "되돌리기: " + strBackupPath + (bak != null ? "\r\n지금 파일 백업: " + bak : "") + "\r\n이전: " + oldSummary + "\r\n지금: " + (mNow != null ? mNow.Summary() : "")
                           + (fromSrc.Length > 0 ? "\r\n★" + fromSrc : "") + (warn.Length > 0 ? "\r\n" + warn : "") + (bLog ? "" : "\r\n" + SAGRUN_NO_APPLYLOG);
                    L("되돌리기 — " + Path.GetFileName(strBackupPath) + (bak != null ? " (지금 파일은 " + Path.GetFileName(bak) + " 로 백업)" : ""));
                    if (fromSrc.Length > 0) L("★" + fromSrc);
                    if (warn.Length > 0) L(warn);
                    if (!bLog) L(SAGRUN_NO_APPLYLOG);
                    return true;
                }
                catch (Exception ex)
                {
                    string tail = bTouched ? SagRun_Recover(strUrdfDir, active, activeTest, bak, bHadActive, strBackupPath, mMemBefore, bSwapped) : " — 적용 중 파일은 그대로";
                    if (bTouched) SagRun_ApplyLog(strUrdfDir, "되돌리기 실패 " + SagRun_FileName(strBackupPath) + " — " + SagRun_Msg(ex) + tail);
                    report = "★오류: " + SagRun_Msg(ex) + tail; L(report); return false;
                }
                finally { SagRun_DeleteRetry(tmp); }
            }
            private static string SagRun_FileName(string strPath)
            {
                try { return Path.GetFileName(strPath ?? ""); } catch { return strPath ?? ""; }
            }
            // 바꿔 끼운 적용 중 파일 다시 읽기 (SagComp_Load) — 검사기·동기화 프로그램이 잠깐 잡고 있으면(공유 위반·접근 거부) 200ms 뒤 다시 (모두 6번).
            //   반환 = 읽어 메모리 모델이 됐다. 내용이 쓸 수 없는 모델이면 다시 하지 않는다. why = 마지막 실패 까닭
            private bool SagRun_LoadActiveRetry(string strActive, out string why)
            {
                why = "";
                for (int i = 0; i < SAGRUN_SWAP_TRIES; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    try
                    {
                        if (SagComp_Load(strActive)) return true;
                        why = File.Exists(strActive) ? "쓸 수 있는 모델이 아님" : "적용 중 파일 없음";
                        return false;
                    }
                    catch (IOException ex) { why = SagRun_Msg(ex); }
                    catch (UnauthorizedAccessException ex) { why = SagRun_Msg(ex); }
                }
                return false;
            }
            // 바꿔 끼운 뒤 적용 중 파일이 잠겨 다시 읽지 못할 때 — 원본(바꿔 넣은 결과 파일·고른 백업)이 바꿔 끼운 내용(staged)과 같은 바이트면 원본에서 읽는다
            //   (SagComp_Load — 1초 페이드, 쌍둥이도). 원본이 잠깐 잡혀 있으면 200ms 뒤 다시 (모두 6번). 반환 = 읽어 메모리 모델이 됐다 (디스크 = 새 파일과 같은 내용).
            //   원본이 없거나 내용이 달라졌거나 쓸 수 없는 모델이면 다시 하지 않는다. why = 마지막 실패 까닭
            private bool SagRun_LoadSourceRetry(string strSrc, byte[] staged, out string why)
            {
                why = "";
                for (int i = 0; i < SAGRUN_SWAP_TRIES; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    try
                    {
                        if (staged == null || string.IsNullOrEmpty(strSrc) || !File.Exists(strSrc)) { why = "원본 없음"; return false; }
                        byte[] a = File.ReadAllBytes(strSrc);
                        bool bSame = a.Length == staged.Length;
                        for (int k = 0; bSame && k < a.Length; k++) if (a[k] != staged[k]) bSame = false;
                        if (!bSame) { why = "원본이 바꿔 끼운 내용과 다름"; return false; }
                        if (SagComp_Load(strSrc)) return true;
                        why = "쓸 수 있는 모델이 아님";
                        return false;
                    }
                    catch (IOException ex) { why = SagRun_Msg(ex); }
                    catch (UnauthorizedAccessException ex) { why = SagRun_Msg(ex); }
                }
                return false;
            }
            // 지금 디스크의 적용 중 파일이 무엇인가 — 새 파일(바꿔 넣으려던 것) / 이전 파일(백업과 같음) / 없음 / 알 수 없음
            private static string SagRun_DiskState(string strActive, string strNewPath, string strBak)
            {
                try
                {
                    if (!File.Exists(strActive)) return "없음";
                    byte[] b = File.ReadAllBytes(strActive);
                    if (!string.IsNullOrEmpty(strNewPath) && File.Exists(strNewPath) && SagRun_SameBytes(strNewPath, b)) return "새 파일 (" + Path.GetFileName(strNewPath) + " 과 같음)";
                    if (!string.IsNullOrEmpty(strBak) && File.Exists(strBak) && SagRun_SameBytes(strBak, b)) return "이전 파일 (백업 " + Path.GetFileName(strBak) + " 과 같음)";
                    return "알 수 없는 내용";
                }
                catch (Exception ex) { return "읽지 못함 (" + SagRun_Msg(ex) + ")"; }
            }
            private static bool SagRun_Refuse(out string report, string why, CSim2Real.DLog L)
            {
                report = why;
                if (L != null) L("★" + why);
                return false;
            }

            // ── 적용 중 파일 바꾸기 (적용·되돌리기·복원 공통) ──
            //   원본을 적용 중 파일과 같은 폴더의 임시 파일로 복사해 바이트를 확인한 뒤(SagRun_StageCopy) 그 자리로 바꿔 끼운다(SagRun_SwapIn).
            //   CopyFile 은 도중에 실패하면(원본 읽기 잠금·디스크 가득 등) 덜 쓴 대상 파일을 지운다 — 적용 중 파일에 바로 복사하면 그 파일이 없어진 채
            //   남는다 (10-02 확인: 원본에 바이트 범위 잠금 → 적용 중 파일 없음). 임시 파일로 받아 두면 실패해도 적용 중 파일은 그대로다.
            //   바꿔 끼우기는 적용 중 파일을 먼저 지우지 않는다 — 새 파일이 그 자리에 들어간 뒤에야 옛 파일이 없어진다 (지우고 옮기다 옮기기가 막히면
            //   적용 중 파일이 없어진다: 10-02 검토 재현 — 검사기처럼 임시 파일을 지우기 공유 없이 잡고 있으면 Replace·Move 가 함께 막힌다).
            private const int SAGRUN_SWAP_TRIES = 6, SAGRUN_SWAP_WAIT_MS = 200;   // 바꿔 끼우기·되돌려 놓기 — 200ms 뒤 다시, 모두 6번 (약 1초)
            private const int SAGRUN_STAGE_STALE_MIN = 1;                          // 이보다 오래된 임시·옆 이름 파일만 치운다 (쓰는 중인 것은 몇 초 안에 끝난다)

            // 원본 → 같은 폴더 임시 파일 (<이름>.<8자>.tmp), 바이트 확인. 반환 = 임시 파일 경로. 실패하면 임시 파일을 지우고 예외
            //   복사본의 읽기 전용 표시는 푼다 (원본이 읽기 전용이면 복사본도 그렇게 되어 Replace 가 막힌다)
            private static string SagRun_StageCopy(string strSrc, string strActive)
            {
                byte[] staged;
                return SagRun_StageCopyBytes(strSrc, strActive, out staged);
            }
            // SagRun_StageCopy 와 같고 staged = 확인한 바이트 (바꿔 끼운 뒤 적용 중 파일이 잠겨 다시 읽지 못하면, 원본이 아직 이 바이트인지 보고 원본에서 읽는다 —
            //   SagRun_LoadSourceRetry)
            private static string SagRun_StageCopyBytes(string strSrc, string strActive, out byte[] staged)
            {
                staged = null;
                string dir = Path.GetDirectoryName(Path.GetFullPath(strActive)) ?? "";
                if (dir.Length > 0) Directory.CreateDirectory(dir);
                string tmp = Path.Combine(dir, Path.GetFileName(strActive) + "." + Guid.NewGuid().ToString("N").Substring(0, 8) + ".tmp");
                try
                {
                    File.Copy(strSrc, tmp, false);
                    SagRun_NotReadOnly(tmp);
                    byte[] src = File.ReadAllBytes(strSrc);
                    if (!SagRun_SameBytes(tmp, src)) throw new IOException("임시 복사본이 원본과 다름 (" + strSrc + ")");
                    staged = src;
                    return tmp;
                }
                catch
                {
                    SagRun_DeleteRetry(tmp);
                    throw;
                }
            }
            // 확인한 임시 파일을 적용 중 파일 자리로 바꿔 끼운다. 반환 = 바꿔 끼움. false = 못 함 (why) — 적용 중 파일은 원래 내용 그대로 (없었으면 없는 그대로).
            //   적용 중 파일이 있으면 File.Replace(임시 → 적용 중, 옛 파일 → 옆 이름 <이름>.<8자>.old) 로 한 번에 바꿔 끼우고 옆 이름을 지운다, 없으면 File.Move.
            //   검사기·동기화 프로그램이 임시 파일이나 적용 중 파일을 잠깐 잡고 있으면 실패하므로 200ms 뒤 다시 (모두 6번) — 끝내 안 되면 바꾸지 않는다.
            //   (Replace 는 실패하면 두 파일을 그대로 둔다 — 읽기 전용·잠김·공유 위반 10-02 확인.) 옛 파일을 옆 이름으로 옮긴 뒤 새 파일을 못 옮기고
            //   끝나면(ERROR_UNABLE_TO_MOVE_REPLACEMENT_2) 옆 이름을 제자리로 돌린다. 그것도 안 되거나 임시 파일이 없어져 결과를 알 수 없으면 예외
            //   — 부른 쪽이 백업에서 되돌린다 (옆 이름 파일도 남는다).
            private static bool SagRun_SwapIn(string strTmp, string strActive, out string why)
            {
                why = "";
                Exception last = null;
                for (int i = 0; i < SAGRUN_SWAP_TRIES; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    if (!File.Exists(strTmp)) throw new IOException("바꿔 끼울 임시 파일이 없어짐 (" + strTmp + ")");
                    string side = null;
                    try
                    {
                        if (!File.Exists(strActive)) { File.Move(strTmp, strActive); return true; }
                        side = strActive + "." + Guid.NewGuid().ToString("N").Substring(0, 8) + ".old";
                        File.Replace(strTmp, strActive, side);
                    }
                    catch (Exception ex)
                    {
                        last = ex;
                        if (side != null && !File.Exists(strActive) && File.Exists(side)) SagRun_PutBack(side, strActive);   // 옛 파일을 제자리로 (못 하면 예외)
                        if (!File.Exists(strTmp)) throw;                         // 임시 파일이 옮겨졌는데 실패 — 결과를 알 수 없다
                        if (!File.Exists(strActive) && side != null) throw;      // 있던 적용 중 파일이 없다 — 부른 쪽이 백업에서 되돌린다
                        continue;
                    }
                    SagRun_DeleteRetry(side);
                    return true;
                }
                why = SagRun_Msg(last);
                return false;
            }
            // Replace 가 옛 파일을 옆 이름에 남기고 끝났을 때 — 옆 이름을 적용 중 파일 자리로 되돌린다 (200ms 뒤 다시, 모두 6번). 못 하면 예외 (옆 이름 경로를 알린다)
            private static void SagRun_PutBack(string strSide, string strActive)
            {
                Exception last = null;
                for (int i = 0; i < SAGRUN_SWAP_TRIES; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    try
                    {
                        if (!File.Exists(strActive)) File.Move(strSide, strActive);
                        return;
                    }
                    catch (Exception ex) { last = ex; }
                }
                throw new IOException("옛 적용 중 파일을 제자리로 돌리지 못함 — " + strSide + " (" + SagRun_Msg(last) + ")");
            }
            // 예외 문구 한 줄 — Windows 입출력 오류 문구 끝의 줄바꿈 등을 빈칸으로 (보고·기록 줄이 둘로 갈리지 않게)
            private static string SagRun_Msg(Exception ex)
            {
                if (ex == null) return "";
                return Regex.Replace(ex.Message ?? "", @"\s*[\r\n]+\s*", " ").Trim();
            }
            private static void SagRun_DeleteQuiet(string strPath)
            {
                if (string.IsNullOrEmpty(strPath)) return;
                try { if (File.Exists(strPath)) File.Delete(strPath); } catch { }
            }
            // 복사본의 읽기 전용 표시 풀기 (File.Copy 는 속성을 따라 복사한다)
            private static void SagRun_NotReadOnly(string strPath)
            {
                FileAttributes at = File.GetAttributes(strPath);
                if ((at & FileAttributes.ReadOnly) != 0) File.SetAttributes(strPath, at & ~FileAttributes.ReadOnly);
            }
            // 임시·옆 이름 파일 지우기 — 검사기 등이 잠깐 잡고 있으면 200ms 뒤 다시 (모두 5번). 끝내 못 지우면 남겨 둔다 (다음 적용·되돌리기·결과 목록이 치운다)
            private static void SagRun_DeleteRetry(string strPath)
            {
                if (string.IsNullOrEmpty(strPath)) return;
                for (int i = 0; i < 5; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    try
                    {
                        if (!File.Exists(strPath)) return;
                        File.Delete(strPath);
                        if (!File.Exists(strPath)) return;
                    }
                    catch { }
                }
            }
            // 지난 적용·되돌리기가 지우지 못하고 남긴 임시·옆 이름 파일 (<적용 중 이름>.<8자>.tmp / .old) 치우기 — 만든 지도 고친 지도 1분이 넘은 것만
            //   (쓰는 중인 파일은 몇 초 안에 끝난다. 임시 복사본의 만든 시각 = 복사한 때, 옆 이름의 만든 시각 = 바꿔 끼운 때 — 10-02 확인). 예외 없음
            private static void SagRun_SweepStage(string strActive)
            {
                try
                {
                    if (string.IsNullOrEmpty(strActive)) return;
                    string full = Path.GetFullPath(strActive);
                    string dir = Path.GetDirectoryName(full), name = Path.GetFileName(full);
                    if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name) || !Directory.Exists(dir)) return;
                    var re = new Regex("^" + Regex.Escape(name) + @"\.[0-9a-f]{8}\.(tmp|old)$", RegexOptions.IgnoreCase);
                    DateTime lim = DateTime.UtcNow.AddMinutes(-SAGRUN_STAGE_STALE_MIN);
                    foreach (string p in Directory.GetFiles(dir, name + ".*"))
                    {
                        if (!re.IsMatch(Path.GetFileName(p))) continue;
                        try { if (File.GetCreationTimeUtc(p) < lim && File.GetLastWriteTimeUtc(p) < lim) File.Delete(p); }
                        catch { }
                    }
                }
                catch { }
            }
            // 적용·되돌리기가 적용 중 파일을 건드린 뒤 실패 — 방금 만든 백업으로 되돌린다. 반환 = 보고 꼬리
            //   (" — 원래 파일로 되돌림" / 되돌리기가 실패했어도 바꿔 끼우기 전에 멈췄으면 " — 적용 중 파일은 그대로 …" /
            //    아니면 지금 디스크의 적용 중 파일이 무엇인지·메모리 모델·백업 경로를 알린다). strNewPath = 바꿔 넣으려던 파일 (결과·고른 백업),
            //   mMemBefore = 시작 때 메모리 모델 (실패한 적용은 메모리 모델을 바꾸지 않는다 — 다시 읽기가 실패해야 여기 온다).
            //   bSwapped = 바꿔 끼우기가 끝났다 (디스크 = 새 파일). 되돌리기가 적용 중 파일을 바꾸지 못하고 끝났으면(바꿔 끼우기가 막혀 그대로 — SagRun_RestoreActive)
            //   디스크는 새 파일이다 — 잠긴 파일을 읽어 보지 않고 그렇게 알린다 (10-03 검토: 잠긴 새 파일을 읽으려다 "읽지 못함" 이라고 알렸다)
            private string SagRun_Recover(string strUrdfDir, string active, string activeTest, string bak, bool bHadActive, string strNewPath, CSagComp mMemBefore, bool bSwapped)
            {
                string strMem = ReferenceEquals(m_SagComp, mMemBefore) ? "메모리 = 이전 모델" : "메모리 = 바뀐 모델";
                bool bUnchanged = false;
                try
                {
                    SagRun_RestoreActive(strUrdfDir, active, activeTest, bak, bHadActive, out bUnchanged);
                    if (!ReferenceEquals(m_SagComp, mMemBefore)) SagComp_Set(mMemBefore);   // 드물게 메모리가 이미 바뀌었으면 시작 때 모델로 (디스크와 같게)
                    return " — 원래 파일로 되돌림";                     // 디스크·메모리 모두 이전 모델
                }
                catch (Exception ex)
                {
                    bool bSame = false;
                    try { bSame = bak != null && File.Exists(bak) && File.Exists(active) && SagRun_SameBytes(active, File.ReadAllBytes(bak)); }
                    catch { bSame = false; }
                    if (bSame) return " — 적용 중 파일은 그대로 (되돌리는 중 " + SagRun_Msg(ex) + ") · " + strMem;
                    string strDisk = (bSwapped && bUnchanged) ? "새 파일 (" + SagRun_FileName(strNewPath) + " — 바꿔 끼움은 끝났고 되돌리기는 막힘)"
                                                              : SagRun_DiskState(active, strNewPath, bak);
                    return " — ★되돌리기도 실패 (" + SagRun_Msg(ex) + ") — ★디스크의 적용 중 파일 = " + strDisk + ", " + strMem
                         + (strDisk.StartsWith("새 파일") ? " (앱을 다시 켜면 디스크의 새 파일이 읽힌다)" : "")
                         + (bak != null ? " — 백업 " + bak + " 을 되돌리기로 고르십시오" : "");
                }
            }
            // 백업 → 적용 중 파일 (임시 파일로 받아 바꿔 끼움 — 이미 백업과 같은 바이트면 그대로), 시험 기록도. 적용 전 파일이 없었으면 지운다. 실패하면 예외.
            //   bUnchanged = 예외로 끝났을 때 적용 중 파일을 바꾸지 않았다고 확실한가 (임시 복사 실패·바꿔 끼우기가 막혀 false — 적용 중 파일은 원래 내용 그대로.
            //   바꿔 끼우기 예외·바꿔 끼운 뒤의 실패는 false).
            //   메모리 모델은 다시 읽지 않는다 — 실패한 적용·되돌리기는 메모리 모델을 바꾸지 않았다 (다시 읽기가 실패해 여기 온다) → 이전 바이트로 돌아가면 디스크·메모리가 같다.
            //   (메모리가 이미 바뀌었으면 부른 쪽(SagRun_Recover)이 시작 때 모델 객체로 되돌린다)
            private void SagRun_RestoreActive(string strUrdfDir, string active, string activeTest, string bak, bool bHadActive, out bool bUnchanged)
            {
                bUnchanged = true;
                if (bak != null)
                {
                    if (!File.Exists(bak)) throw new IOException("백업 파일 없음: " + bak);
                    bool bSame = false;
                    try { bSame = File.Exists(active) && SagRun_SameBytes(active, File.ReadAllBytes(bak)); }
                    catch { bSame = false; }
                    if (!bSame)
                    {
                        string tmp = SagRun_StageCopy(bak, active), why;                   // 실패하면 예외 — 적용 중 파일은 그대로
                        try
                        {
                            bUnchanged = false;                                            // 바꿔 끼우기 예외 = 결과를 모른다
                            bool bOk = SagRun_SwapIn(tmp, active, out why);
                            if (!bOk) { bUnchanged = true; throw new IOException(why); }   // 막힘 — 적용 중 파일은 그대로 (SagRun_SwapIn)
                        }
                        finally { SagRun_DeleteRetry(tmp); }
                    }
                    bUnchanged = false;                                                    // 적용 중 파일 = 백업과 같은 바이트
                    string bakTest = SagRun_TestRecordPath(bak);
                    if (File.Exists(bakTest)) File.Copy(bakTest, activeTest, true); else if (File.Exists(activeTest)) File.Delete(activeTest);
                }
                else if (!bHadActive)
                {
                    if (File.Exists(active)) File.Delete(active);                          // 예외면 적용 중 파일 그대로
                    bUnchanged = false;
                    if (File.Exists(activeTest)) File.Delete(activeTest);
                }
            }
            // 적용 중 파일 백업 — sagcomp_backup\sagcomp_model_<yyyyMMdd_HHmmss>[_n].txt (+ 시험 기록 _test.txt). 반환 = 백업 경로
            private static string SagRun_BackupActive(string strUrdfDir, string active, string activeTest)
            {
                string dir = SagRun_BackupDir(strUrdfDir);
                Directory.CreateDirectory(dir);
                string stamp = "sagcomp_model_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                for (int n = 1; n < 1000; n++)
                {
                    string p = Path.Combine(dir, stamp + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : "") + ".txt");
                    if (File.Exists(p) || File.Exists(SagRun_TestRecordPath(p))) continue;
                    try { File.Copy(active, p, false); }
                    catch (IOException) { if (File.Exists(p)) continue; throw; }
                    try
                    {
                        SagRun_NotReadOnly(p);                                   // 읽기 전용 적용 중 파일의 백업도 다루기 쉽게
                        if (File.Exists(activeTest)) { File.Copy(activeTest, SagRun_TestRecordPath(p), true); SagRun_NotReadOnly(SagRun_TestRecordPath(p)); }
                    }
                    catch { SagRun_DeleteQuiet(p); throw; }                      // 시험 기록 없는 반쪽 백업을 남기지 않는다
                    return p;
                }
                throw new IOException("백업 이름을 만들지 못함 (" + dir + ")");
            }
            private const string SAGRUN_NO_APPLYLOG = "★apply_log.txt 에 적지 못함 (파일이 잠김 등) — 적용·되돌리기는 됐다";
            // sagcomp_results\apply_log.txt 에 한 줄 (시각 + 내용). 반환 = 적음 (잠겨 있으면 200ms 뒤 다시, 모두 3번)
            private static bool SagRun_ApplyLog(string strUrdfDir, string strLine)
            {
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " " + strLine;
                for (int i = 0; i < 3; i++)
                {
                    if (i > 0) Thread.Sleep(SAGRUN_SWAP_WAIT_MS);
                    try
                    {
                        string dir = SagRun_ResultsDir(strUrdfDir);
                        Directory.CreateDirectory(dir);
                        SagRun_AppendLine(Path.Combine(dir, "apply_log.txt"), line);
                        return true;
                    }
                    catch { }
                }
                return false;
            }
            // 기록 파일에 한 줄 덧붙이기 (UTF-8 BOM 없음, CRLF) — 파일 끝에 줄바꿈이 없으면(손으로 고친 기록) 먼저 줄을 바꿔 앞 줄에 붙지 않게. 실패하면 예외
            private static void SagRun_AppendLine(string strPath, string strLine)
            {
                string pre = "";
                if (File.Exists(strPath))
                    using (var fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                        if (fs.Length > 0) { fs.Seek(-1, SeekOrigin.End); if (fs.ReadByte() != '\n') pre = "\r\n"; }
                File.AppendAllText(strPath, pre + (strLine ?? "").Replace("\r", " ").Replace("\n", " ") + "\r\n", new UTF8Encoding(false));
            }
            // 적용할 수 있는 결과 파일인가 — 관절 모델, 파일에 gpx 줄이 있으면 바닥 GP, fc* 줄이 있으면 첫닿음 모델(버리지 않고 준비됨)까지
            private bool SagRun_CheckUsable(CSagComp m, string strPath, out string why)
            {
                why = "";
                if (m == null) { why = "파일을 읽지 못함"; return false; }
                if (!m.IsUsable || !m.HasJoint) { why = "관절 모델이 없음"; return false; }
                int nGp = 0, nFc = 0;
                foreach (string raw in File.ReadAllLines(strPath))
                {
                    string ln = raw.TrimStart();
                    if (ln.StartsWith("gpx=", StringComparison.OrdinalIgnoreCase)) nGp++;
                    else if (ln.StartsWith("fc", StringComparison.OrdinalIgnoreCase) && ln.IndexOf('=') > 0) nFc++;
                }
                if (nGp > 0 && !m.HasGp) { why = "바닥 GP 줄이 있지만 읽히지 않음"; return false; }
                if (nFc > 0)
                {
                    if (!m.HasFc || m.FcDropped) { why = "첫닿음 모델 줄(fc*)이 모자라거나 잘못됨"; return false; }
                    C3d kin = SagRun_Kin();
                    if (!(kin ?? this).SagComp_FcPrepare(m)) { why = "첫닿음 모델 준비 실패"; return false; }
                }
                return true;
            }
            // 관절 구성 비교 — func·posids·wrist·jids + 로봇 정의 (수직 관절 합 downsum ±0.5°, 기구학 표시 kinsig — 빈 값 = 내장 OMX)
            private static bool SagRun_SameJoints(CSagComp a, CSagComp b, out string why)
            {
                why = "";
                var d = new List<string>();
                if (a.FuncNumber != b.FuncNumber) d.Add(string.Format(CultureInfo.InvariantCulture, "func {0}→{1}", a.FuncNumber, b.FuncNumber));
                if (!SagRun_SameInts(a.PosIDs, b.PosIDs)) d.Add("posids " + SagRun_Ints(a.PosIDs) + "→" + SagRun_Ints(b.PosIDs));
                if (a.WristID != b.WristID) d.Add(string.Format(CultureInfo.InvariantCulture, "wrist {0}→{1}", a.WristID, b.WristID));
                if (!SagRun_SameInts(a.JointIDs, b.JointIDs)) d.Add("jids " + SagRun_Ints(a.JointIDs) + "→" + SagRun_Ints(b.JointIDs));
                if (!(Math.Abs(a.DownSumDeg - b.DownSumDeg) <= SAGDEF_TOL_DEG))                       // 로봇 정의의 수직 관절 합
                    d.Add(string.Format(CultureInfo.InvariantCulture, "downsum {0:0.###}→{1:0.###}", a.DownSumDeg, b.DownSumDeg));
                string ka = SagRun_KinSigNorm(a.KinSig), kb = SagRun_KinSigNorm(b.KinSig);       // 기구학 표시 (빈 값 = 내장 OMX)
                if (!string.Equals(ka, kb, StringComparison.Ordinal)) d.Add("kinsig " + (ka.Length > 0 ? ka : "?") + "→" + (kb.Length > 0 ? kb : "?"));
                why = string.Join(", ", d.ToArray());
                return d.Count == 0;
            }
            private static bool SagRun_SameInts(int[] a, int[] b)
            {
                if (a == null || b == null) return a == b;
                if (a.Length != b.Length) return false;
                for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
                return true;
            }
            private static string SagRun_Ints(int[] a)
            {
                return a == null ? "없음" : string.Join(",", Array.ConvertAll(a, v => v.ToString(CultureInfo.InvariantCulture)));
            }

            /// <summary>쌍둥이 미리 보기 — strModelPath 가 null 이면 적용 중 모델로, 경로면 그 파일로 (적용하지 않고 실물이 있을 자세만 본다).
            /// 작업 중이면 거절 (작업이 쌍둥이를 정한다). strInfo = 표시 문구. 반환 = 쌍둥이 모델이 있다. 예외 없음.</summary>
            public bool SagRun_TwinUse(string strModelPath, out string strInfo)
            {
                using (SagRun_KinUse()) return SagRun_TwinUseBody(strModelPath, out strInfo);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private bool SagRun_TwinUseBody(string strModelPath, out string strInfo)
            {
                strInfo = "";
                try
                {
                    if (SagRun_IsBusy) { strInfo = "작업 중 — 쌍둥이 모델은 작업이 정한다"; return false; }
                    if (string.IsNullOrEmpty(strModelPath))
                    {
                        CSagComp m = SagComp_Get();
                        SagTwin_SetModel(m);
                        if (m == null || !m.HasJoint) { strInfo = "적용 중 모델 없음 — 쌍둥이 없음"; return false; }
                        strInfo = "쌍둥이 = 적용 중 모델 (" + SagRun_ModelBrief(m) + ")";
                        return true;
                    }
                    if (!File.Exists(strModelPath)) { strInfo = "파일 없음: " + strModelPath; return false; }
                    CSagComp mf = CSagComp.Load(strModelPath);
                    if (mf == null || !mf.HasJoint) { strInfo = "관절 모델이 없는 파일 — 쌍둥이로 쓸 수 없다"; return false; }
                    SagTwin_SetModel(mf);
                    strInfo = "쌍둥이 = " + Path.GetFileName(strModelPath) + " (" + SagRun_ModelBrief(mf) + ") — 적용하지 않은 미리 보기";
                    return true;
                }
                catch (Exception ex) { strInfo = "★오류: " + SagRun_Msg(ex); return false; }
            }

            // ────────────────────────────────────────────────────────────
            // ① 실물(또는 3D 만) 작업 손잡이 — 연결 빌리기·펼치기·안전 이동·원위치 (10-03)
            //   포트를 직접 열지 않는다 — 로봇팔 세팅 탭의 연결 객체(CArmConn_t)를 소유자 잠금으로 빌린다 (new CScene_t·open/close·싱크 null 없음).
            //   시작: 잠금 → 휴지 자세·서보 토크 상태 읽기 (토크를 건드리기 전 — 못 읽으면 시작하지 않는다) → 연결이 토크 OFF 로 알면 토크 켜기
            //   (서보가 이미 붙잡고 있던 축은 그 목표를 이어받는다 — 그 축이 있으면 시작 상태 = 토크 ON, 이 작업은 토크를 끄지 않는다)
            //   → 움직이기 전 확인(EnsureSynced) → 이 작업이 토크를 켠 축은 3D = 실물 → 옮기는 보정 (관절 모델만 — 작업이 SagArm_SetMoveModel 로 정한다).
            //   옮기는 보정 (10-03): 펼치기·옮기기·원위치 들어 올림과 경유점까지 관절 모델(바닥 부분 끔)이 걸린다 — 실물 끝이 관절 처짐만큼(r260 ~20mm) 계획보다
            //   낮게 지나가지 않는다. 다른 보정(누름의 모델·틈 재기 켬)은 다음 옮기기가 옮기는 높이까지 들어 올린 뒤에야 옮기는 보정으로 넘어간다
            //   (★10-03 검토: 보정을 끄고 옮기면 r260 옮기는 높이 90 에서 실물 끝이 예측 첫닿음 위 8~19mm, 원위치가 보정을 빼며 들어 올릴 때 14mm 였다).
            //   끝 (오류·중단·예외 포함, finally): 손목 유지 놓기 → 지금 피치·지금 보정 그대로 수직으로 들어 올림 → (옮기는 높이에서 옮기는 보정으로) 경유점
            //   → 끝 보정 (시작 때 토크 ON 이면 시작 때 모델, 아니면 끔 — 경유점에서) → 관절 보간으로 시작 자세 → 확인 (넘으면 한 번 더) → 토크를 시작 때 상태로
            //   → 잠금 놓기 (포트는 열린 채 — 연결 해제는 사용자가 연결 줄에서).
            //   옮기기: 지금 피치 그대로 옮기는 높이까지 수직 → 피치는 옮기는 높이에서만 바꾼다 → 옮기기 (요 차가 20° 넘으면 호 가운데를 거친다) → 수직.
            //   중단 요청은 다리 사이마다 본다 (10-03 검토) — 다리를 마친 자리(보통 옮기는 높이)에서 멈추고 원위치는 거기서 시작한다
            //   (다음 점으로 옮기거나 펜 높이로 내려가지 않는다).
            //   옮기는 높이는 점마다다 (계획 행·시험 점의 TransferZ — 먼 점은 SafeZ 아래, 리치 여유 팔꿈치 굽힘 ≥ 20°, 창 위 아래로는 내리지 않는다).
            //   다리는 모두 관절 보간이다 — 끝 자세를 관절 합을 정한 IK 로 미리 풀고(못 풀면 움직이지 않는다) 그 관절 보간 경로를 복제 기구학 FK 로 훑어
            //   높이 하한·반경 띠·수직 벗어남을 확인한 뒤(안 되면 가운데 점으로 나눈다) 움직이고, 끝나면 3D TCP·관절 합을 확인한다.
            //   ★10-03 검토: 예전에는 직선 경로(PlayXyzPath)의 틱마다 IK 를 풀어, 리치 경계 근처에서 해가 건너뛰었다 (버스 흉내 한 틱 5~46°).
            //   실물 작업 동안 장면의 한 틱 관절 변화 막이(CScene_t.StepGuard)가 켜져 있다 — 다리·누름마다 걸렸는지 보고 걸렸으면 작업을 멈춘다.
            //   3D 만(bDry): 빈 싱크를 걸어 실물로 보내지 않고, 끝에 싱크·모델·쌍둥이·출력 단 상태를 되돌린다 (연결 객체가 있으면 연결 여부와 상관없이
            //   잠금을 잡는다 — 그동안 연결·토크 켜기가 거절된다. 다리·누름마다 싱크가 그대로인지도 본다).
            // ────────────────────────────────────────────────────────────

            /// <summary>실물(또는 3D 만) 작업 한 번의 손잡이 — SagArm_Open 이 만들고 SagArm_Close 가 정리한다. 작업 스레드 하나가 쓴다.</summary>
            public class CSagArm
            {
                public CSagArmCfg Cfg;                       // 작업 설정 (복사본)
                public CArmConn_t Conn;                      // 빌린 연결 (3D 만이고 연결이 없으면 null)
                public string Owner;                         // 연결 잠금 소유자 이름 (잡지 않았으면 null)
                public CScene_t Scene;                       // 실물 장면 (= Conn.Scene) — 3D 만이면 null
                public bool IsDry { get { return Scene == null; } }
                public bool WasTorqueOn;                     // 시작 때 토크 ON 이었다 (연결이 ON 으로 알았거나 서보가 작업 축의 토크를 붙잡고 있었다) — 끝에 그 상태로 (OFF 였으면 끈다)
                internal int[] StartTorqueAxes;              // 시작 때 서보가 토크를 붙잡고 있던 축 (주소 64 로 읽음) — 이 작업은 이 축들의 토크를 끄지 않는다
                internal bool StopSeen;                      // 옮기기 다리 사이에서 중단 요청을 보고 멈췄다 (SagArm_MoveSafeCore — 부른 쪽은 실패가 아니라 중단으로 친다)
                public Dictionary<int, float> RestPose;      // 휴지 자세 — 실물: 시작 때 정착 읽기 (토크를 건드리기 전), 3D 만: 시작 때 3D 각
                public string RunDir;                        // 작업 폴더 (null 이면 파일 없음)
                public float Speed = 1f;                     // 시간 나눗수 (3D 만 = Cfg.DrySpeed)
                public bool Moved;                           // 휴지 자세를 떠났다 → 원위치 때 들어 올림·경유점을 거친다
                internal CSagComp PrevModel, PrevTwin;       // 시작 때 적용 중 보정·쌍둥이 모델 — Close 가 되돌린다
                internal CSagComp MoveModel;                 // 옮기는 동안(펼치기·옮기기·원위치) 출력 단에 거는 보정 — 관절 모델만 (바닥 부분 끔), null = 보정 없이
                internal Dictionary<int, float> StartApplied;// 시작 때 (맞춘 뒤) 서보 목표에 들어 있던 보정 (관절 ID → deg) — 원위치 확인이 끝의 보정과의 차만큼 감안한다
                internal Action<int> PrevSink;               // 시작 때 모션 싱크 — 3D 만: 빈 싱크를 걸었다가 되돌린다
                internal Action<int> DrySink;                // 3D 만: 건 빈 싱크
                internal object OutState;                    // 3D 만: 시작 때 출력 단 상태 (끝에 되돌린다)
                internal Dictionary<int, float> StartCmd;    // 시작 때 (맞춘 뒤) 3D 자세 — 원위치 관절 보간의 목표
                internal bool TorqueTurnedOn, LockTaken, SinkSet, ModelSwitched, Closed;
                internal float CurTransferZ = float.NaN;     // 지금 점에서 옮길 수 있는 높이 (모르면 NaN — 복제 기구학으로 구한다)
                internal float CurZLow = float.NaN;          // 지금 점에서 옮기는 높이의 아래 한계 (창 위 등, 모르면 NaN)
                internal CScene_t GuardScene;                // 한 틱 관절 변화 막이를 켠 장면 (실물 — 끝에 끈다)
                internal bool SinkLost;                      // 3D 만: 작업 중에 모션 싱크가 바뀌었다 — 3D 를 더 움직이지 않는다 (실물로 나갈 수 있다)
                /// <summary>마지막으로 멈춘 까닭 (한 틱 관절 변화 막이·싱크 바뀜·IK·경로 확인 등, 없으면 "") — 작업 결과의 Why 에 붙인다</summary>
                public string LastFail = "";
            }

            private static void SagRun_NoopSink(int n) { }           // 3D 만: 모션 스텝마다 아무것도 보내지 않는다 (싱크가 있으면 UI 의 쌍둥이 정지 갱신도 3D 를 건드리지 않는다)

            /// <summary>작업이 쓰는 축 중 방향 확인이 안 된 축 (연결의 모터 세팅 UncheckedAxes ∩ 설정의 AllIDs) — 앱이 측정·시험 시작 전에 경고한다
            /// (U4 — DLL 은 막지 않는다). 연결·세팅이 없으면 빈 배열. 예외 없음.</summary>
            public static int[] SagArm_UncheckedAxes(CArmConn_t conn, CSagArmCfg cfg)
            {
                try
                {
                    if (conn == null) return new int[0];
                    if (cfg == null) cfg = new CSagArmCfg();
                    CArmMotorSet_t set = conn.Motors;
                    if (set == null) return new int[0];
                    var l = new List<int>();
                    int[] all = cfg.AllIDs;
                    foreach (int id in set.UncheckedAxes()) if (Array.IndexOf(all, id) >= 0) l.Add(id);
                    return l.ToArray();
                }
                catch { return new int[0]; }
            }
            /// <summary>모델(결과) 파일의 모터 매핑(motormap)과 지금 연결의 매핑 비교 (C10) — 다르면 "★학습 때와 방향·영점이 다름 (…) — 다시 학습 권장", 같으면 "".
            /// 연결·세팅이 없으면 "" (비교하지 않는다). 키가 없는 옛 파일은 기본 매핑으로 본다. 예외 없음.</summary>
            public static string SagArm_MotorMapWarn(CArmConn_t conn, CSagComp m)
            {
                try
                {
                    if (conn == null || m == null) return "";
                    CArmMotorSet_t set = conn.Motors;
                    if (set == null) return "";
                    string d = SagRun_MotorMapDiff(m.MotorMap, set.MappingKey());
                    return d.Length > 0 ? "★학습 때와 방향·영점이 다름 (" + d + ") — 다시 학습 권장" : "";
                }
                catch { return ""; }
            }

            /// <summary>작업 시작 — 연결 빌리기 (블로킹, 작업 스레드). conn = 로봇팔 세팅 탭의 연결 객체, strOwner = 잠금 소유자 이름 ("측정"·"닿기 시험"·"틈 재기"),
            /// strRunDir = 작업 폴더 (rest_pose.txt), bDry = 3D 만 (실물은 움직이지 않는다 — conn 이 null 이어도 된다. conn 이 있으면 연결 여부와 상관없이
            /// 잠금만 잡는다 — 작업 동안 그 연결 객체의 연결·토크 켜기·연결 해제·조그가 거절된다. 앱은 탭의 연결 객체를 늘 넘긴다).
            /// 먼저 (3D 만도) 설정의 높이 — 옮기는 높이 SafeZ·펼치기·원위치 경유점 z 가 CSagArmSetup.SAFEZ_MIN 아래거나 수가 아니면 "안전하지 않은 높이" 로 거절.
            /// 그다음 로봇 정의 (10-04, SagArm_CheckKinematics → SagDef_Check: 공구 끝 체인·꺾는 손목·바닥으로 수직이 3D 와 맞는가) — 안 되면 아무것도 건드리지 않고 거절.
            /// 실물: ① 연결 중 ② 구성 불일치 아님, 장면 ID ⊇ 설정의 AllIDs ③ 잠금 (못 잡으면 "사용 중: 소유자")
            /// ④ 휴지 자세 = 정착 읽기 (토크를 건드리기 전 — 못 읽거나 ID 가 빠졌거나 |각| > MaxStartDeg 면 시작하지 않는다, 토크 그대로)
            /// ⑤ 시작 때 토크 상태 = 서보에서 읽는다 (주소 64, 장면의 모든 축 — 10-03 검토: 연결은 OFF 로 아는데 서보는 토크를 붙잡고 있을 수 있다.
            ///   연결 때 토크 상태를 못 읽었거나 다른 프로그램이 켠 경우). 못 읽은 축이 있으면 "서보 토크 상태를 읽지 못함" 으로 시작하지 않는다 (토크 그대로).
            ///   WasTorqueOn = 연결이 토크 ON 으로 앎 또는 작업 축 중 토크를 붙잡고 있던 축이 있음
            /// ⑥ 연결이 토크 OFF 로 알면 TorqueOn (경고는 로그 — 붙잡고 있던 축은 서보 목표를 그대로 이어받고, 처짐 보정이 걸려 있었으면 페이드 없이 끈다:
            ///   그 보정은 작업이 끝나도 꺼진 채 — 작업 안의 옮기는 보정·누름 모델은 다음 동작의 출력 틱에 걸쳐 들어온다). 붙잡고 있던 축이 하나도 없을 때만 '이 작업이 켠 토크' 로 친다
            ///   — 붙잡고 있던 축이 있으면 시작 실패·끝 어느 쪽에서도 토크를 끄지 않는다 (시작 때 꺼져 있던 축이 섞여 있었으면 그 축도 켠 채 둔다 — 로그)
            /// ⑦ EnsureSynced (실패면 거절 — ⑥ 에서 이 작업이 전 축 꺼진 상태에서 켰을 때만 토크를 다시 끈다. 통과 알림(서보 목표 이어받기·보정 끔)은 로그)
            /// ⑧ 이 작업이 토크를 켠 축(시작 때 꺼져 있던 축)만 3D = 실물 1° 안 (정렬 읽기가 낡았으면 막는다 — 붙잡고 있던 축은 3D = 서보 목표라 처진 지금 위치와
            ///   하중만큼 다르다. 그 축은 EnsureSynced 가 서보 목표와 비교했다) ⑨ 장면의 한 틱 관절 변화 막이 켬 (작업이 끝날 때 끈다 — CScene_t.StepGuard)
            /// ⑩ 옮기는 보정으로 바꿈 (10-03) — 시작 때 적용 중 모델의 관절 모델만 (바닥 부분 끔, CSagComp.JointOnlyCopy — 관절 모델이 없으면 보정 끔).
            ///   걸려 있던 보정에서 다음 이동(펼치기)의 출력 틱에 걸쳐 넘어간다. 작업마다 모델을 정하려면 곧바로 SagArm_SetMoveModel (측정·시험 작업이 그렇게 한다).
            ///   펼치기·옮기기·원위치 들어 올림과 경유점까지 이 보정이 걸린다 — 실물 끝이 관절 처짐만큼(r260 에서 ~20mm) 계획보다 낮게 지나가지 않는다
            ///   (★10-03 검토: 보정을 끄고 옮기면 r260 옮기는 높이 90 에서 실물 끝이 예측 첫닿음 위 8~19mm, 원위치가 보정을 빼며 들어 올릴 때 14mm 였다).
            ///   시작 때 서보 목표에 든 보정(연결이 토크 ON 으로 알던 때 걸려 있던 보정 — 서보 목표를 이어받았거나 이 작업이 토크를 켰으면 0)도 기억한다
            ///   (원위치 확인 — SagArm_Close).
            /// 실패하면 null 과 why — 그때까지 한 일(토크·잠금·싱크·모델)은 되돌린다. 동시에 한 작업만 (다른 작업 중이면 거절). 예외 없음.</summary>
            public CSagArm SagArm_Open(CArmConn_t conn, CSagArmCfg cfg, string strRunDir, string strOwner, bool bDry, CSim2Real.DLog log, out string why)
            {
                why = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var ci = CultureInfo.InvariantCulture;
                try { SagRun_Kin(); } catch { }                                  // 바쁨을 잡기 전 — UI(STA) 스레드에서 부르면 바뀐 수식 글로 계획 기구학을 새로 만든다
                if (Interlocked.CompareExchange(ref s_nSagRunBusy, 1, 0) != 0) { why = "다른 로봇팔 작업이 진행 중"; L("★작업을 시작하지 않음 — " + why); return null; }
                CSagArm arm = null;
                bool bOk = false;
                try
                {
                    if (cfg == null) cfg = new CSagArmCfg();
                    if (string.IsNullOrEmpty(strOwner)) strOwner = "로봇팔 작업";
                    if (!cfg.IdsOk) { why = "설정의 관절 ID 가 맞지 않음 (위치 3개 + 손목)"; return null; }
                    string wh = SagArm_HeightsWhy(cfg);
                    if (wh.Length > 0) { why = wh; return null; }
                    string wk;
                    if (!SagArm_CheckKinematics(cfg, false, out wk)) { why = wk; return null; }   // 로봇 정의 확인 (SagDef_Check) + 지금 자세의 3D·계획 기구학 TCP — 토크·잠금·싱크 전
                    if (MoveJoints_IsMoving()) { why = "3D 모션 중 — 끝난 뒤 다시"; return null; }
                    arm = new CSagArm();
                    arm.Cfg = cfg.Clone(); arm.RunDir = strRunDir; arm.Speed = bDry ? Math.Max(0.1f, cfg.DrySpeed) : 1f;
                    arm.PrevModel = m_SagComp; arm.PrevTwin = SagTwin_GetModel(); arm.PrevSink = MoveJoints_GetMotionSink();
                    arm.MoveModel = arm.PrevModel != null ? arm.PrevModel.JointOnlyCopy() : null;   // 관절 모델만 (사본) — 관절 모델이 없으면 null (보정 없이). 작업이 SagArm_SetMoveModel 로 바꾼다
                    if (!string.IsNullOrEmpty(strRunDir)) Directory.CreateDirectory(strRunDir);
                    if (bDry)
                    {
                        // 연결 객체가 있으면 연결 여부와 상관없이 잠금을 잡는다 (10-03 검토) — 그동안 연결·토크 켜기·연결 해제가 거절된다.
                        //   잠그지 않으면 3D 만 작업의 이동 사이에 연결되어 새 장면의 싱크가 빈 싱크를 덮고, 남은 3D 동작(3배속, 늦춤 없는 3D 누름)이
                        //   토크를 붙잡은 실물로 나갔다 (버스 흉내 605 스텝). 다리·누름마다 싱크도 다시 본다 (SagArm_SinkOk)
                        if (conn != null)
                        {
                            if (conn.c3d != this) { why = "연결 객체가 다른 3D 의 것"; return null; }
                            if (!conn.TryAcquire(strOwner)) { why = "사용 중: " + SagArm_BusyName(conn); return null; }
                            arm.Conn = conn; arm.Owner = strOwner; arm.LockTaken = true;
                        }
                        arm.OutState = SagComp_SaveOutState();
                        arm.DrySink = new Action<int>(SagRun_NoopSink);
                        MoveJoints_SetMotionSink(arm.DrySink); arm.SinkSet = true;
                        arm.RestPose = SagArm_Pose3D(arm.Cfg.AllIDs); arm.StartCmd = SagArm_Pose3D(arm.Cfg.AllIDs);
                        SagComp_Switch(arm.MoveModel); arm.ModelSwitched = true;
                        L("3D 만 — 실물은 움직이지 않는다" + (arm.LockTaken ? " (연결 객체 잠금을 잡음 — 그동안 연결·토크·조그·읽기를 받지 않는다)" : ""));
                        bOk = true;
                        return arm;
                    }
                    if (conn == null || !conn.IsConnected) { why = "로봇팔 세팅 탭에서 먼저 연결하십시오"; return null; }
                    if (conn.c3d != this) { why = "연결 객체가 다른 3D 의 것"; return null; }
                    if (conn.ConfigMismatch) { why = "모터 구성이 연결 뒤 바뀜 — 다시 연결하십시오"; return null; }
                    CScene_t sc = conn.Scene;
                    if (sc == null) { why = "로봇팔 세팅 탭에서 먼저 연결하십시오"; return null; }
                    var miss = new List<int>(); int[] scIds = sc.IDs;
                    foreach (int id in arm.Cfg.AllIDs) if (Array.IndexOf(scIds, id) < 0) miss.Add(id);
                    if (miss.Count > 0) { why = "연결된 모터(ID " + SagRun_Ints(scIds) + ")에 작업이 쓰는 ID " + SagRun_Ints(miss.ToArray()) + " 가 없음"; return null; }
                    if (!conn.TryAcquire(strOwner)) { why = "사용 중: " + SagArm_BusyName(conn); return null; }
                    arm.Conn = conn; arm.Owner = strOwner; arm.LockTaken = true;
                    if (MoveJoints_IsMoving()) { why = "3D 모션 중 — 끝난 뒤 다시"; return null; }
                    bool bConnOn = conn.IsTorqueOn;                              // 연결이 아는 토크 상태 (서보와 다를 수 있다 — 아래에서 서보를 읽는다)
                    string w;
                    Dictionary<int, float> rest = sc.ReadDeg(out w);
                    if (rest == null) { why = "휴지 자세 읽기 실패 — " + w + " (전원·배선·통신 속도 확인) — 시작하지 않음 (토크 그대로)"; return null; }
                    foreach (int id in arm.Cfg.AllIDs)
                    {
                        float v;
                        if (!rest.TryGetValue(id, out v)) { why = "휴지 자세에 ID " + id + " 가 없음 — 시작하지 않음 (토크 그대로)"; return null; }
                        if (float.IsNaN(v) || Math.Abs(v) > arm.Cfg.MaxStartDeg)
                        { why = string.Format(ci, "ID {0} 휴지 각 {1:F1}° — 이상값 (|각| > {2:0.#}°) — 시작하지 않음 (토크 그대로)", id, v, arm.Cfg.MaxStartDeg); return null; }
                    }
                    arm.RestPose = rest;
                    // 시작 때 토크 상태는 서보에서 읽는다 (주소 64, 장면의 모든 축 — 10-03 검토). 연결은 OFF 로 아는데 서보는 토크를 붙잡고 있을 수 있다
                    //   (연결 때 토크 상태를 못 읽음, 다른 프로그램이 켬) — 그 상태로 '이 작업이 켠 토크' 로 치면 시작 실패·끝에서 토크를 꺼 붙잡혀 있던 팔이 떨어졌다.
                    //   못 읽은 축이 있으면 토크를 건드리지 않고 시작하지 않는다
                    Dictionary<int, long> dTq = sc.ReadReg(64, 1, out w);
                    var lstHeld = new List<int>(); var lstTqUnk = new List<int>();
                    foreach (int id in scIds)
                    {
                        long v;
                        if (dTq == null || !dTq.TryGetValue(id, out v)) lstTqUnk.Add(id);
                        else if (v != 0) lstHeld.Add(id);
                    }
                    if (lstTqUnk.Count > 0)
                    { why = "서보 토크 상태를 읽지 못함 — ID " + SagRun_Ints(lstTqUnk.ToArray()) + (string.IsNullOrEmpty(w) ? "" : " (" + w + ")") + " (전원·배선·통신 확인) — 시작하지 않음 (토크 그대로)"; return null; }
                    arm.StartTorqueAxes = lstHeld.ToArray();
                    bool bJobHeld = false;
                    foreach (int id in arm.Cfg.AllIDs) if (lstHeld.Contains(id)) bJobHeld = true;
                    arm.WasTorqueOn = bConnOn || bJobHeld;
                    var lstJobOn = new List<int>();                              // 이 작업이 토크를 켠 작업 축 (시작 때 꺼져 있던 축) — 3D = 실물 1° 확인 대상
                    if (!bConnOn)
                    {
                        string warn;
                        if (!conn.TorqueOn(strOwner, out warn)) { why = "토크 켜기 실패 — " + warn; return null; }
                        arm.TorqueTurnedOn = lstHeld.Count == 0;                 // 붙잡고 있던 축이 있으면 이 작업이 켠 토크가 아니다 — 시작 실패·끝에서 끄지 않는다
                        foreach (int id in arm.Cfg.AllIDs) if (!lstHeld.Contains(id)) lstJobOn.Add(id);
                        if (!string.IsNullOrEmpty(warn)) L("토크 켜기 경고: " + warn.Replace("\r\n", " / "));
                        if (lstHeld.Count > 0)
                        {
                            var lstOff0 = new List<int>(); foreach (int id in scIds) if (!lstHeld.Contains(id)) lstOff0.Add(id);
                            L("시작 때 서보 토크: ID " + SagRun_Ints(lstHeld.ToArray()) + " 이 이미 토크를 붙잡고 있었음 (연결은 토크 OFF 로 알고 있었음) — 서보 목표를 이어받음, "
                              + "이 작업은 토크를 끄지 않는다" + (lstOff0.Count > 0 ? " (시작 때 꺼져 있던 ID " + SagRun_Ints(lstOff0.ToArray()) + " 도 켠 채 둔다)" : ""));
                        }
                    }
                    if (!conn.EnsureSynced(strOwner, out w)) { why = "움직이기 전 확인 실패 — " + w; return null; }
                    if (!string.IsNullOrEmpty(w)) L("움직이기 전 확인: " + w.Replace("\r\n", " / "));   // 통과 알림 — 서보 목표 이어받기·처짐 보정 끔 (CArmConn_t.ADOPT_COMP_OFF)
                    if (lstJobOn.Count > 0)
                    {
                        float worst = 0; int idW = 0;
                        foreach (int id in lstJobOn) { float d = Math.Abs(GetData(id) - rest[id]); if (d > worst) { worst = d; idW = id; } }
                        if (worst > 1f)
                        {
                            why = string.Format(ci, "3D 동기화 실패 — ID {0} 3D 와 실물 차 {1:F1}° ({2})", idW, worst,
                                                arm.TorqueTurnedOn ? "토크 끔" : "토크는 켠 채 둠 — 시작 때 토크를 붙잡고 있던 축이 있음");
                            return null;
                        }
                    }
                    arm.Scene = sc;
                    arm.StartCmd = SagArm_Pose3D(arm.Cfg.AllIDs);
                    SagArm_WriteRest(arm);
                    sc.StepGuard_Begin(SAGARM_GUARD_MIN_DEG, SAGARM_GUARD_DEG_PER_SEC); arm.GuardScene = sc;   // 작업 동안 한 틱 관절 변화 막이 (CScene_t.SendPose)
                    arm.StartApplied = SagArm_AppliedDict(arm.Cfg.MoveIDs);                                // 서보 목표에 든 보정 (연결이 토크 ON 으로 알던 때의 걸린 보정 — 이어받았거나 토크를 켰으면 0)
                    SagComp_Switch(arm.MoveModel); arm.ModelSwitched = true;
                    L(string.Format(ci, "토크 {0} — 휴지 자세 {1}{2}", bConnOn ? "그대로 ON (시작 때 켜져 있었음)" : (arm.TorqueTurnedOn ? "ON" : "ON (붙잡고 있던 서보 목표를 이어받음)"), SagArm_PoseText(rest),
                        (arm.Cfg.HoldIDs != null && arm.Cfg.HoldIDs.Length > 0) ? ", " + SagRun_Ints(arm.Cfg.HoldIDs) + " 은 붙잡은 각도 그대로" : ""));
                    bOk = true;
                    return arm;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return null; }
                finally
                {
                    if (!bOk)
                    {
                        if (why.Length > 0) L("★작업을 시작하지 않음 — " + why);
                        SagArm_Undo(arm, L);
                        Interlocked.Exchange(ref s_nSagRunBusy, 0);
                    }
                }
            }
            // 설정의 높이가 안전한가 — 옮기는 높이(SafeZ)·펼치기·원위치 경유점의 z 는 SAFEZ_MIN 이상 (경유점까지는 관절 보간이라 멈춤 없이 간다).
            //   sagarm.txt 는 읽을 때 이 기준으로 거르지만, 앱이 설정을 직접 만들어 넘기면 거치지 않는다 — 작업 시작 때 한 번 더 본다. 반환 = 까닭 (괜찮으면 "")
            private static string SagArm_HeightsWhy(CSagArmCfg c)
            {
                var ci = CultureInfo.InvariantCulture;
                float zMin = CSagArmSetup.SAFEZ_MIN;
                if (!(c.SafeZ >= zMin) || float.IsInfinity(c.SafeZ))
                    return string.Format(ci, "안전하지 않은 높이 — 옮기는 높이 safez={0} 가 {1:0} 보다 낮거나 수가 아님", c.SafeZ, zMin);
                string[] names = { "펼치기 경유점(unfoldvia)", "원위치 경유점(restorevia)" };
                float[][] vias = { c.UnfoldVia, c.RestoreVia };
                for (int k = 0; k < 2; k++)
                {
                    float[] v = vias[k];
                    if (v == null || v.Length < 3) return "안전하지 않은 높이 — " + names[k] + " 없음";
                    for (int i = 0; i < 3; i++)
                        if (float.IsNaN(v[i]) || float.IsInfinity(v[i])) return "안전하지 않은 높이 — " + names[k] + " 에 수가 아닌 값";
                    if (v[2] < zMin) return string.Format(ci, "안전하지 않은 높이 — {0} z={1} 가 {2:0} 보다 낮음 (관절 보간이라 멈춤 없이 간다)", names[k], v[2], zMin);
                }
                return "";
            }
            // 시작 실패 — 그때까지 한 일을 되돌린다 (켠 토크 끄기, 모델·싱크·출력 단 상태, 잠금). 예외 없음
            private void SagArm_Undo(CSagArm arm, CSim2Real.DLog L)
            {
                if (arm == null) return;
                try { if (arm.GuardScene != null) { arm.GuardScene.StepGuard_End(); arm.GuardScene = null; } } catch { }
                try { if (arm.TorqueTurnedOn && arm.Conn != null && arm.Conn.TorqueOff(arm.Owner)) { arm.TorqueTurnedOn = false; L("켠 토크를 다시 끔"); } } catch { }
                try { if (arm.ModelSwitched) SagComp_Switch(arm.PrevModel); } catch { }
                try { if (arm.SinkSet) SagArm_RestoreSink(arm, L); } catch { }
                try { if (arm.OutState != null) SagComp_RestoreOutState(arm.OutState); } catch { }
                try { if (arm.LockTaken && arm.Conn != null) arm.Conn.Release(arm.Owner); } catch { }
                arm.LockTaken = false;
                arm.Closed = true;
            }

            /// <summary>작업 끝 — finally 전용 (블로킹, 작업 스레드). stop 을 보지 않고 예외를 던지지 않는다. 단계마다 따로 잡아 하나가 실패해도 끝까지 간다:
            /// ① 손목 유지 놓기 (다음 동작의 출력 틱에 걸쳐 빠진다). 보정은 아직 바꾸지 않는다 (실물: 한 틱 관절 변화 막이의 걸림 표시를 지운다 — 원위치는 그 자리에서 다시 간다)
            /// ② 휴지 자세를 떠났으면 지금 피치 그대로 수직으로 들어 올림 — 지금 걸린 보정 그대로 → 옮기는 높이에서 옮기는 보정(관절 모델만)으로 → 경유점(RestoreVia)
            ///   (SagArm_MoveSafeCore, 다리는 모두 관절 보간). ★10-03 검토: 예전에는 여기 들어서자마자 보정을 끄고 들어 올려, 1초 페이드가 12mm 들어 올림보다 빨리
            ///   끝을 떨어뜨렸다 (r260 누름 뒤 실물 끝이 예측 첫닿음 위 14mm). 경로가 실패하면 관절 보간만으로 (2배 느리게, 보정은 그대로)
            /// ③ 끝 보정 — 실물이고 시작 때 토크 ON 이었으면 시작 때 보정 모델, 아니면 끔. 경유점(베이스 가까이·옮기는 높이)에 왔으면 거기서 바꿔 다음 원위치 관절 보간의
            ///   출력 틱이 전환을 끝낸다. 경유점까지 못 갔으면 지금 보정 그대로 시작 자세까지 가서 거기서 바꾼다 (바닥 가까이에서 보정을 빼지 않는다).
            ///   시작 때 토크 ON 이면 끝에 시작 자세를 보내며 붙잡아 전환을 마친다 (SagArm_HoldUntilBlended) → 서보 목표 = 시작 자세 + 시작 때 모델의 보정.
            ///   시작 때 토크 OFF 면 보정 없이 돌아와 끈다 (끄면 보정은 상관없다).
            /// ④ 관절 보간으로 시작 자세 (RestoreJointMs ÷ Speed — 관절 최고 속도 20°/s 를 넘지 않게 늘림, 위치 3개 + 손목만 — 그리퍼는 그대로)
            /// ⑤ 실물: 정착 읽기로 휴지 자세와 비교 — 서보 목표에 든 보정이 시작 때와 다르면(사용자가 그사이 보정을 끔 등) 그 차만큼 옮긴 휴지 자세와 (SagArm_RestDiff).
            ///   RestTolDeg 넘으면 4초 한 번 더 ⑥ 실물: 토크를 시작 때 상태로 (꺼져 있었으면 끈다, 켜져 있었으면 켠 채 휴지 자세를 붙잡는다 — 시작 때 서보가
            ///   토크를 붙잡고 있던 축이 하나라도 있었으면 연결이 OFF 로 알았어도 끄지 않는다) ⑦ 보정·쌍둥이 모델 되돌림
            ///   (3D 만: 싱크·출력 단 상태도), 한 틱 관절 변화 막이 끔 ⑧ 연결 잠금 놓기 (포트는 열린 채, 싱크는 연결의 것 그대로) ⑨ 작업 표시 내림.
            ///   3D 만인데 작업 중에 모션 싱크가 바뀌었으면(그사이 연결 등) 3D 를 움직이지 않는다 (실물로 나갈 수 있다).
            /// 반환 = 휴지 자세 RestTolDeg 안 (3D 만이면 true). arm 이 null 이면 아무것도 하지 않는다 (false). 두 번 불러도 한 번만 정리한다.</summary>
            public bool SagArm_Close(CSagArm arm, CSim2Real.DLog log)
            {
                if (arm == null || arm.Closed) return false;
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var ci = CultureInfo.InvariantCulture;
                bool bRest = arm.IsDry;
                bool bModelBack = false;
                try
                {
                    try { SagComp_WristHoldRelease(); } catch { }
                    if (!arm.IsDry) { try { SagArm_EnsureSceneSink(arm); if (arm.GuardScene != null) arm.GuardScene.StepGuard_ClearTrip(); } catch { } }
                    bool bMove = !arm.IsDry || MoveJoints_GetMotionSink() == arm.DrySink;
                    if (!bMove) L("★모션 싱크가 작업 중에 바뀜 — 3D 만 작업의 원위치를 하지 않는다 (3D 를 움직이면 실물로 나갈 수 있다)");
                    float slow = 1f;
                    bool bVia = !arm.Moved || !bMove;                         // 경유점에 왔다 (또는 움직이지 않는다) — 여기서 끝 보정으로 바꿔도 된다
                    if (arm.Moved && bMove)
                    {
                        bool ok = false;
                        try
                        {
                            float[] v = arm.Cfg.RestoreVia;
                            ok = v != null && v.Length >= 3 && SagArm_MoveSafeCore(arm, v[0], v[1], v[2], 90, float.NaN, float.NaN, L, null);
                        }
                        catch (Exception ex) { L("★원위치 경로 오류: " + SagRun_Msg(ex)); ok = false; }
                        if (!ok)
                        {
                            L("★원위치 경로 실패 — 관절 보간만으로 (2배 느리게, 보정은 그대로 — 시작 자세에 간 뒤 끝 보정으로)"); slow = 2f;
                            if (arm.GuardScene != null) { try { arm.GuardScene.StepGuard_ClearTrip(); } catch { } }
                        }
                        bVia = ok;
                    }
                    if (bVia) bModelBack = SagArm_FinalModel(arm, L);         // ③ 경유점에서 — 원위치 관절 보간의 출력 틱이 전환을 끝낸다
                    Dictionary<int, float> tgt = (arm.StartCmd != null) ? arm.StartCmd : arm.RestPose;
                    try { if (tgt != null && bMove) SagArm_JointTo(arm, tgt, Math.Max(300, (int)(arm.Cfg.RestoreJointMs * slow / arm.Speed))); }
                    catch (Exception ex) { L("★원위치 관절 보간 오류: " + SagRun_Msg(ex)); }
                    if (!bVia) bModelBack = SagArm_FinalModel(arm, L);        // ③ 경유점에 못 왔다 — 시작 자세에서
                    if (!arm.IsDry)
                    {
                        float dmax = float.NaN;
                        try
                        {
                            if (bModelBack && !SagArm_HoldUntilBlended(arm, 4000, null))
                                L(string.Format(ci, "★시작 때 보정이 아직 다 안 들어감 ({0:F0}%)", SagComp_BlendProgress * 100));
                            dmax = SagArm_RestDiff(arm);
                            if (!(dmax <= arm.Cfg.RestTolDeg) && tgt != null)
                            {
                                L(string.Format(ci, "★원위치 차 {0} — 한 번 더", float.IsNaN(dmax) ? "(읽기 실패)" : dmax.ToString("F1", ci) + "°"));
                                if (arm.GuardScene != null) arm.GuardScene.StepGuard_ClearTrip();
                                SagArm_JointTo(arm, tgt, 4000);
                                if (bModelBack) SagArm_HoldUntilBlended(arm, 4000, null);
                                dmax = SagArm_RestDiff(arm);
                            }
                        }
                        catch (Exception ex) { L("★원위치 확인 오류: " + SagRun_Msg(ex)); }
                        bRest = dmax <= arm.Cfg.RestTolDeg;
                        string strTq;
                        try
                        {
                            // 토크는 시작 때 상태로 — 시작 때 서보가 토크를 붙잡고 있던 축이 있으면(StartTorqueAxes, 연결이 OFF 로 알았어도) 끄지 않는다.
                            //   이 작업이 켠 토크(시작 때 전 축 꺼짐)만 끈다
                            // 시작 때 모델로 되돌렸어도 그 모델이 꺼져 있으면(서보 목표 이어받기가 보정을 끈 경우 등) 보정은 걸리지 않는다 — 그대로 적는다
                            if (arm.WasTorqueOn) strTq = "토크 ON 그대로 (시작 때처럼 — 휴지 자세를 붙잡음" +
                                                         (!bModelBack ? ")" : (arm.PrevModel != null && arm.PrevModel.Enabled && arm.PrevModel.HasJoint) ? ", 시작 때 보정 다시 걸림)" : ", 보정 끔 그대로 — 시작 때처럼)");
                            else if (arm.TorqueTurnedOn) strTq = (arm.Conn != null && arm.Conn.TorqueOff(arm.Owner)) ? "토크 끔" : "★토크 끄기 실패 — 팔을 받친 뒤 로봇팔 세팅 탭에서 토크를 끄십시오";
                            else strTq = "토크 켠 채 둠 — 시작 때 토크를 붙잡고 있던 축(ID " + SagRun_Ints(arm.StartTorqueAxes) + ")이 있어 끄지 않는다 (끄려면 팔을 받친 뒤 연결 줄에서)";
                        }
                        catch (Exception ex) { strTq = "★토크 되돌리기 오류: " + SagRun_Msg(ex); }
                        L(string.Format(ci, "{0}원위치 — 휴지 자세와 최대 차 {1}, {2}", bRest ? "" : "★", float.IsNaN(dmax) ? "(읽기 실패)" : dmax.ToString("F1", ci) + "°", strTq));
                    }
                    else L("3D 원위치 — 실물은 움직이지 않았다");
                }
                catch (Exception ex) { L("★원위치 오류: " + SagRun_Msg(ex)); }
                finally
                {
                    if (!bModelBack) { try { SagComp_Switch(arm.PrevModel); } catch { } }
                    try { SagTwin_SetModel(arm.PrevTwin); } catch { }
                    if (arm.GuardScene != null) { try { arm.GuardScene.StepGuard_End(); } catch { } arm.GuardScene = null; }
                    if (arm.SinkSet) { try { SagArm_RestoreSink(arm, L); } catch { } }
                    if (arm.OutState != null) { try { SagComp_RestoreOutState(arm.OutState); } catch { } }
                    if (arm.LockTaken && arm.Conn != null) { try { arm.Conn.Release(arm.Owner); } catch { } }
                    arm.LockTaken = false;
                    arm.Closed = true;
                    Interlocked.Exchange(ref s_nSagRunBusy, 0);
                }
                return bRest;
            }
            // 원위치의 끝 보정으로 — 실물이고 시작 때 토크 ON 이면 시작 때 모델 (반환 true — 원위치 끝에 붙잡아 전환을 마친다), 아니면 끔 (토크를 끄거나 3D 만 —
            //   3D 만은 끝에 출력 단 상태까지 되돌린다). 예외 없음
            private bool SagArm_FinalModel(CSagArm arm, CSim2Real.DLog L)
            {
                try
                {
                    if (!arm.IsDry && arm.WasTorqueOn && arm.PrevModel != null) { SagComp_Switch(arm.PrevModel); arm.ModelSwitched = false; return true; }
                    if (m_SagComp != null) SagComp_Switch(null);
                }
                catch (Exception ex) { if (L != null) L("★보정 되돌리기 오류: " + SagRun_Msg(ex)); }
                return false;
            }
            /// <summary>옮기는 동안의 보정 모델 정하기 (10-03, 작업 스레드) — m 의 관절 모델만 (사본: 바닥 부분 끔·켬, CSagComp.JointOnlyCopy). m 이 null 이거나 관절 모델이
            /// 없으면 보정 없이 옮긴다 (로그 ★ — 실물 끝이 관절 처짐만큼 계획보다 낮게 지나간다). SagArm_Open 바로 뒤(펼치기 전)에 부른다 — 측정 작업 = 측정 모델,
            /// 닿기 시험·틈 재기 = 결과 파일의 끔 모델 (그 작업들이 부른다). 지금 걸린 모델이 옮기는 보정이면(열기 직후 등) 곧바로 바꾸고 (다음 출력 틱부터 1초에 걸쳐),
            /// 다른 보정이 걸려 있으면(누름·틈 재기 켬 등) 다음 옮기기가 옮기는 높이에서 바꾼다. 반환 = 관절 모델이 있다. 예외 없음.</summary>
            public bool SagArm_SetMoveModel(CSagArm arm, CSagComp m, CSim2Real.DLog log)
            {
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    if (arm == null || arm.Closed) { L("★옮기는 보정: 작업 손잡이 없음"); return false; }
                    bool bNow = ReferenceEquals(m_SagComp, arm.MoveModel);
                    arm.MoveModel = m != null ? m.JointOnlyCopy() : null;
                    if (bNow) SagComp_Switch(arm.MoveModel);
                    L(arm.MoveModel != null ? "옮기는 동안 보정: 관절 모델만 (바닥 부분 끔) — 펼치기·옮기기·원위치 들어 올림"
                                            : "★옮기는 동안 보정 없음 (관절 모델 없음) — 실물 끝이 관절 처짐만큼 계획보다 낮게 지나간다");
                    return arm.MoveModel != null;
                }
                catch (Exception ex) { L("★옮기는 보정 오류: " + SagRun_Msg(ex)); return false; }
            }
            // 옮기는 보정으로 (10-03) — 지금 걸린 모델이 그것이면 그대로 (전환을 다시 시작하지 않는다). 옮기기(SagArm_MoveSafeCore)가 들어 올린 뒤 옮기는 높이에서 부른다
            private void SagArm_ToMoveModel(CSagArm arm)
            {
                if (arm == null || ReferenceEquals(m_SagComp, arm.MoveModel)) return;
                SagComp_Switch(arm.MoveModel);
            }
            // 직전 출력 틱에 얹은 보정 (관절 ID → deg)
            private Dictionary<int, float> SagArm_AppliedDict(int[] ids)
            {
                var d = new Dictionary<int, float>();
                float[] v = SagComp_AppliedNow(ids);
                for (int i = 0; i < ids.Length && i < v.Length; i++) d[ids[i]] = v[i];
                return d;
            }

            /// <summary>펼치기 (블로킹, 작업 스레드) — 휴지 자세에서 경유점(UnfoldVia)까지 관절 보간 한 번 (UnfoldViaMs ÷ Speed, 관절 최고 속도 20°/s 를 넘지 않게
            /// 늘림 — 경유점 해는 관절 합을 정한 IK 의 계획 시드 쪽 해, 0도를 거치지 않는다. 그 관절 보간 경로의 TCP 가 두 끝 중 낮은 쪽보다 5mm 넘게
            /// 내려가면 움직이지 않는다) → 첫 점 (x0, y0) 위 옮기는 높이로 안전 이동 (fTransferZ = 그 점의 TransferZ, NaN = SafeZ). stop 이면 경유점에서 멈추고 false.
            /// 실패하면 "★펼치기 실패 — 원위치" 로그와 false (원위치는 부른 쪽의 SagArm_Close — 첫 다리가 움직이기 전에 실패했으면 Close 도 움직이지 않는다). 예외 없음.</summary>
            public bool SagArm_Unfold(CSagArm arm, float fX0, float fY0, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagArm_Unfold(arm, fX0, fY0, float.NaN, log, stop);
            }
            public bool SagArm_Unfold(CSagArm arm, float fX0, float fY0, float fTransferZ, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    if (arm == null || arm.Closed) { L("★펼치기: 작업 손잡이 없음"); return false; }
                    float[] v = arm.Cfg.UnfoldVia;
                    if (v == null || v.Length < 3) { L("★펼치기 경유점 없음"); return false; }
                    L(string.Format(ci, "진행 펼치기: 경유점 ({0:0.#}, {1:0.#}, {2:0.#}) 까지 관절 보간 {3:F1}초", v[0], v[1], v[2], Math.Max(300, (int)(arm.Cfg.UnfoldViaMs / arm.Speed)) / 1000.0));
                    if (!SagArm_UnfoldLeg(arm, v, arm.Cfg.UnfoldViaMs, L)) { L("★펼치기 실패 — 원위치"); return false; }
                    arm.CurTransferZ = float.NaN; arm.CurZLow = float.NaN;
                    if (SagRun_Stopped(stop)) { L("중단 요청 — 펼치기에서 멈춤, 원위치"); return false; }
                    float tz = arm.Cfg.SafeZ;
                    if (!float.IsNaN(fTransferZ) && !float.IsInfinity(fTransferZ)) tz = Math.Min(tz, fTransferZ);
                    if (!SagArm_MoveSafeCore(arm, fX0, fY0, tz, 90, fTransferZ, float.NaN, L, stop))
                    {
                        if (arm.StopSeen) L("중단 요청 — 펼치기에서 멈춤, 원위치");    // 다리 사이에서 본 중단 — 그 자리(경유점·옮기는 높이)에서 원위치
                        else L("★펼치기 실패 — 원위치");
                        return false;
                    }
                    return true;
                }
                catch (Exception ex) { L("★펼치기 오류: " + SagRun_Msg(ex) + " — 원위치"); return false; }
            }
            // 펼치기 첫 다리 — 지금(휴지) 자세에서 경유점 해(피치 90, 계획 시드 (요, 0, 40) 쪽 — 안 되면 지금 자세 시드)까지 관절 보간 한 번
            private bool SagArm_UnfoldLeg(CSagArm arm, float[] v, int nMs, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                C3d kin = SagRun_Kin();
                if (kin == null) return SagArm_Fail(arm, L, "★" + SAGRUN_NO_KIN);
                if (!SagArm_SinkOk(arm, L)) return false;
                float[] qA = SagArm_QNow(c);
                double[] adBase = SagArm_SeedNow(kin, c);
                float xa, ya, za, pa;
                SagArm_Tcp(c, out xa, out ya, out za, out pa);
                float bend;
                float yawV = (float)(Math.Atan2(v[1], v[0]) * 180.0 / Math.PI);
                float[] qB = SagArm_IkSum(kin, c, SagRun_Seed(kin, c, yawV, 90f), v[0], v[1], v[2], c.DownSumDeg, out bend);   // 피치 90 (Ry) = 정의의 수직 관절 합
                if (qB == null) qB = SagArm_IkSum(kin, c, adBase, v[0], v[1], v[2], c.DownSumDeg, out bend);
                if (qB == null) return SagArm_Fail(arm, L, string.Format(ci, "★펼치기: 경유점 ({0:0.#}, {1:0.#}, {2:0.#}) IK 로 못 감 — 움직이지 않음", v[0], v[1], v[2]));
                string why = SagArm_PathWhy(kin, c, adBase, qA, qB, new float[] { xa, ya, za }, new float[] { v[0], v[1], v[2] }, Math.Min(za, v[2]) - 5f, -1e6f, 1e6f, 0f);
                if (why.Length > 0) return SagArm_Fail(arm, L, "★펼치기: 휴지 → 경유점 관절 보간 경로 확인 실패 — " + why + " — 움직이지 않음");
                string wl = SagArm_LimitWhy(c, qB);
                if (wl.Length > 0) return SagArm_Fail(arm, L, "★펼치기: " + wl + " — 움직이지 않음");
                float dq = SagArm_MaxDq(qA, qB);
                int t = Math.Max(300, (int)(Math.Max(nMs, 1.5 * dq / SAGARM_JOINT_DEG_PER_SEC * 1000.0) / Math.Max(0.1f, arm.Speed)));
                arm.Moved = true;                                            // 여기부터 휴지 자세를 떠난다 — 원위치 때 들어 올림·경유점을 거친다
                if (!SagArm_PlayQ(arm, "펼치기", qB, t, 200, L)) return false;
                return SagArm_Arrived(arm, "펼치기", v[0], v[1], v[2], c.DownSumDeg, L);
            }

            /// <summary>안전 이동 (블로킹, 작업 스레드) — 지금 TCP 에서 (x, y, z, 피치) 로:
            /// ① 지금 관절 합(피치) 그대로 옮기는 높이까지 수직 ② (지금 피치가 90 이 아니면) 옮기는 높이에서 피치 90 으로 ③ 옮기는 높이에서 옮기기 — 요 차가 20° 넘으면
            /// 호 가운데 (두 반경 평균, 두 요 평균) 를 거친다 ④ (목적지 피치가 90 이 아니면) 옮기는 높이에서 그 피치로 ⑤ z 까지 수직.
            /// 다리는 모두 관절 보간이다 (SagArm_LegTo — 끝 자세는 관절 합을 정한 IK, 틱마다 IK 를 풀지 않아 해가 건너뛰지 않는다. 10-03 검토: 직선 경로의 틱마다
            /// IK 가 리치 경계에서 다른 해로 건너뛰어 한 틱 5~46°). 미리 그 관절 보간 경로의 TCP 를 복제 기구학으로 훑어 높이 하한 (옮기기 = 옮기는 높이 − 2 와
            /// 창 위 중 높은 쪽, 수직 = 두 끝 중 낮은 쪽)·반경 띠 (두 끝 ±15mm)·수직 다리의 수평 벗어남 (3mm) 을 확인하고, 안 되면 TCP 가운데 점으로 나눠 다시 본다
            /// (최대 8 조각, 그래도 안 되면 움직이지 않고 false).
            /// 옮기는 높이 = SafeZ, 지금 점·목적지에서 리치 여유(팔꿈치 굽힘 ≥ 20°)를 두고 갈 수 있는 가장 높은 곳 (복제 기구학 — 먼 점은 SafeZ 아래).
            /// 다리 시간 = max(MinMoveMs, 거리 ÷ TransferMmPerSec, 1.5 × 최대 관절 변화 ÷ 20°/s) ÷ Speed (조각마다 최소 300ms). 끝나면 3D TCP 가 IkTolMm 안,
            /// 관절 합이 PitchTolDeg 안인지 확인한다. 실물이면 다리마다 장면의 한 틱 관절 변화 막이도 본다 (걸리면 false). 예외 없음.</summary>
            public bool SagArm_MoveSafe(CSagArm arm, float fX, float fY, float fZ, float fPitch, CSim2Real.DLog log)
            {
                return SagArm_MoveSafe(arm, fX, fY, fZ, fPitch, float.NaN, log);
            }
            /// <summary>SagArm_MoveSafe 와 같고 옮기는 높이를 준다 — 계획 행·시험 점의 TransferZ (SafeZ 를 넘지 않는다, NaN = SafeZ).
            /// 지금 점에서 그 높이에 리치 여유를 두고 팔이 닿지 않으면 둘 중 낮은 쪽 (지금 점의 창 위 아래로는 내리지 않는다 — 못 옮기면 false)</summary>
            public bool SagArm_MoveSafe(CSagArm arm, float fX, float fY, float fZ, float fPitch, float fTransferZ, CSim2Real.DLog log)
            {
                return SagArm_MoveSafe(arm, fX, fY, fZ, fPitch, fTransferZ, log, null);
            }
            /// <summary>SagArm_MoveSafe 와 같고 중단 요청을 다리 사이마다 본다 (10-03) — 걸리면 그 다리를 마친 자리(보통 옮기는 높이)에서 멈추고
            /// "중단 요청 — …" 로그와 false (원위치는 부른 쪽의 SagArm_Close 가 지금 자리에서 시작한다. 다음 점으로 옮기거나 내려가지 않는다)</summary>
            public bool SagArm_MoveSafe(CSagArm arm, float fX, float fY, float fZ, float fPitch, float fTransferZ, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    if (arm == null || arm.Closed) { L("★옮기기: 작업 손잡이 없음"); return false; }
                    return SagArm_MoveSafeCore(arm, fX, fY, fZ, fPitch, fTransferZ, float.NaN, L, stop);
                }
                catch (Exception ex) { L("★옮기기 오류: " + SagRun_Msg(ex)); return false; }
            }
            // 다리 사이 중단 요청 (10-03 검토: 예전에는 옮기기가 끝까지 — 틈 재기에서 중단 7.6초 뒤에도 다음 점으로 가 펜 높이까지 내려갔다).
            //   걸리면 지금 자리(마친 다리의 끝)에서 멈춘다 — arm.StopSeen (부른 쪽은 실패가 아니라 중단으로 친다), 원위치는 부른 쪽의 SagArm_Close
            private static bool SagArm_StopBetweenLegs(CSagArm arm, CSim2Real.DIsStopped stop, CSim2Real.DLog L, string strNext)
            {
                if (stop == null || !SagRun_Stopped(stop)) return false;
                arm.StopSeen = true; arm.LastFail = "";
                if (L != null) L("중단 요청 — 옮기기 다리 사이에서 멈춤 (" + strNext + " 하지 않음), 지금 자리에서 원위치");
                return true;
            }
            // 안전 이동 본체 — fZLow = 목적지에서 옮기는 높이의 아래 한계 (창 위 등, NaN = 없음). 성공하면 지금 점 = 목적지 (옮기는 높이·아래 한계 기억).
            //   stop = 다리 사이마다 보는 중단 요청 (null = 보지 않음 — 원위치). 걸리면 false + arm.StopSeen
            private bool SagArm_MoveSafeCore(CSagArm arm, float fX, float fY, float fZ, float fPitch, float fTransferZ, float fZLow, CSim2Real.DLog L, CSim2Real.DIsStopped stop)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                arm.StopSeen = false;
                C3d kin = SagRun_Kin();
                if (kin == null) return SagArm_Fail(arm, L, "★" + SAGRUN_NO_KIN);
                if (float.IsNaN(fPitch) || fPitch > 90.01f) return SagArm_Fail(arm, L, string.Format(ci, "★옮기기: 피치 {0} 는 못 감 (피치 ≤ 90)", fPitch));
                float x0, y0, z0, p0;
                SagArm_Tcp(c, out x0, out y0, out z0, out p0);
                float sum0 = SagArm_SumNow(c), sumDest = SagArm_SumFor(c, fPitch);
                // 옮기는 높이 — 목적지 값(SafeZ 이하) 과 지금 점·목적지에서 리치 여유를 두고 갈 수 있는 높이 중 낮은 쪽
                float zT = c.SafeZ;
                bool bTz = !float.IsNaN(fTransferZ) && !float.IsInfinity(fTransferZ);
                if (bTz) zT = Math.Min(zT, fTransferZ);
                float zCur = arm.CurTransferZ;
                if (float.IsNaN(zCur))
                {
                    float zLowHere = Math.Min(z0, zT);
                    zCur = SagRun_TransferZ(kin, c, SagArm_SeedNow(kin, c), x0, y0, 90f, zLowHere);
                    float zCan = SagRun_TransferZ(kin, c, SagRun_Seed(kin, c, (float)(Math.Atan2(y0, x0) * 180.0 / Math.PI), 90f), x0, y0, 90f, zLowHere);   // 계획과 같은 시드로도
                    if (float.IsNaN(zCur) || (!float.IsNaN(zCan) && zCan > zCur)) zCur = zCan;
                    if (float.IsNaN(zCur)) zCur = zLowHere;                   // 여유 있는 높이를 못 찾음 — 지금 높이에서 옮긴다 (아래 한계·다리 IK 가 막는다)
                }
                zT = Math.Min(zT, zCur);
                if (!bTz)
                {
                    // 계획 값이 없는 목적지 (원위치 경유점 등) — 거기서도 리치 여유를 두고 갈 수 있는 높이로
                    float zLowDest = Math.Min(float.IsNaN(fZLow) ? fZ : fZLow, zT);
                    float zDest = SagRun_TransferZ(kin, c, SagRun_Seed(kin, c, (float)(Math.Atan2(fY, fX) * 180.0 / Math.PI), 90f), fX, fY, fPitch, zLowDest);
                    if (!float.IsNaN(zDest)) zT = Math.Min(zT, zDest);
                }
                float zLowNeed = float.MinValue;
                if (!float.IsNaN(arm.CurZLow)) zLowNeed = Math.Max(zLowNeed, arm.CurZLow);
                if (!float.IsNaN(fZLow)) zLowNeed = Math.Max(zLowNeed, fZLow);
                if (zT < zLowNeed - 0.01f)
                    return SagArm_Fail(arm, L, string.Format(ci, "★옮길 높이 없음 — 갈 수 있는 높이 {0:F0} 이 창 위 {1:F0} 보다 낮음 (({2:F0}, {3:F0}) → ({4:F0}, {5:F0}))", zT, zLowNeed, x0, y0, fX, fY));
                float zFloorT = zLowNeed > float.MinValue ? Math.Max(zT - 2f, zLowNeed) : zT - 2f;   // 옮기는 동안 TCP 높이 하한
                int nMin = c.MinMoveMs;
                // 중단 요청은 다리 사이마다 본다 — 걸리면 마친 다리의 끝(①을 지났으면 옮기는 높이)에서 멈춘다 (SagArm_StopBetweenLegs)
                if (SagArm_StopBetweenLegs(arm, stop, L, "들어 올리기·옮기기")) return false;
                // ① 지금 관절 합(피치) 그대로 옮기는 높이까지 수직 — 지금 걸린 보정 그대로 (바닥 가까이에서 보정을 바꾸지 않는다)
                if (Math.Abs(z0 - zT) > 1f && !SagArm_LegTo(arm, "수직 이동", x0, y0, zT, sum0, Math.Min(z0, zT), SAGARM_VERT_DEV_MM, nMin, L)) return false;
                // 옮기는 높이에서 옮기는 보정(관절 모델만)으로 — 다른 보정(틈 재기 켬 모델 등)이 걸려 있었으면 여기부터 출력 틱에 걸쳐 넘어간다 (10-03)
                SagArm_ToMoveModel(arm);
                if (SagArm_StopBetweenLegs(arm, stop, L, "옮기기")) return false;
                // ② 옮기는 높이에서 피치 90 (관절 합 = 정의의 수직 관절 합 downsum, OMX 90)
                float xa, ya, za, pa;
                SagArm_Tcp(c, out xa, out ya, out za, out pa);
                if (Math.Abs(SagArm_SumNow(c) - c.DownSumDeg) > 0.5f)
                {
                    if (!SagArm_LegTo(arm, "피치 90 으로", xa, ya, za, c.DownSumDeg, za - 5f, 5f, 2 * nMin, L)) return false;
                    if (SagArm_StopBetweenLegs(arm, stop, L, "옮기기")) return false;
                }
                // ③ 옮기기 (요 차 > 20° 면 호 가운데를 거친다)
                SagArm_Tcp(c, out xa, out ya, out za, out pa);
                double r0 = Math.Sqrt((double)xa * xa + (double)ya * ya), r1 = Math.Sqrt((double)fX * fX + (double)fY * fY);
                double hd = Math.Sqrt((double)(fX - xa) * (fX - xa) + (double)(fY - ya) * (fY - ya));
                if (hd > 1)
                {
                    double a0 = Math.Atan2(ya, xa), a1 = Math.Atan2(fY, fX), da = a1 - a0;
                    while (da > Math.PI) da -= 2 * Math.PI; while (da < -Math.PI) da += 2 * Math.PI;
                    if (Math.Abs(da) * 180.0 / Math.PI > 20)
                    {
                        double am = a0 + da / 2, rm = (r0 + r1) / 2;
                        if (!SagArm_LegTo(arm, "옮기기 (호 가운데)", (float)(rm * Math.Cos(am)), (float)(rm * Math.Sin(am)), zT, c.DownSumDeg, zFloorT, 0f, nMin / 2, L)) return false;
                        if (SagArm_StopBetweenLegs(arm, stop, L, "옮기기 나머지")) return false;
                        if (!SagArm_LegTo(arm, "옮기기", fX, fY, zT, c.DownSumDeg, zFloorT, 0f, nMin / 2, L)) return false;
                    }
                    else if (!SagArm_LegTo(arm, "옮기기", fX, fY, zT, c.DownSumDeg, zFloorT, 0f, nMin, L)) return false;
                    arm.CurTransferZ = zT; arm.CurZLow = fZLow;                 // 목적지 위 옮기는 높이에 왔다 — 여기서 멈춰도 원위치가 이 높이·아래 한계를 쓴다
                }
                // ④ 목적지 피치 (옮기는 높이에서)
                if (Math.Abs(sumDest - c.DownSumDeg) > 0.5f)
                {
                    if (SagArm_StopBetweenLegs(arm, stop, L, "피치 바꾸기")) return false;
                    if (!SagArm_LegTo(arm, "피치 " + fPitch.ToString("0.#", ci) + " 로", fX, fY, zT, sumDest, zT - 5f, 5f, 2 * nMin, L)) return false;
                }
                // ⑤ z 까지 수직
                if (Math.Abs(fZ - zT) > 1f)
                {
                    if (SagArm_StopBetweenLegs(arm, stop, L, "내려가기")) return false;
                    if (!SagArm_Descend(arm, fX, fY, fZ, fPitch, L)) return false;
                }
                arm.CurTransferZ = zT; arm.CurZLow = fZLow;
                return true;
            }
            // 수직으로 z 까지 (관절 보간 — 끝 자세의 관절 합 = 그 피치의 관절 합, 수평 벗어남 3mm 안, TCP 는 두 끝 중 낮은 쪽 아래로 가지 않는다)
            private bool SagArm_Descend(CSagArm arm, float fX, float fY, float fZ, float fPitch, CSim2Real.DLog L)
            {
                float x0, y0, z0, p0;
                SagArm_Tcp(arm.Cfg, out x0, out y0, out z0, out p0);
                return SagArm_LegTo(arm, "수직 이동", fX, fY, fZ, SagArm_SumFor(arm.Cfg, fPitch), Math.Min(z0, fZ), SAGARM_VERT_DEV_MM, arm.Cfg.MinMoveMs, L);
            }

            // ── 관절 보간 다리 (10-03) ──
            private const float SAGARM_JOINT_DEG_PER_SEC = 20f;      // 관절 보간 다리의 관절 최고 속도 상한 (°/s, smoothstep 최고 = 평균의 1.5 배) — 시간을 늘려 지킨다
            private const float SAGARM_GUARD_MIN_DEG = 3f;           // 한 틱 관절 변화 막이 — 틱 하나의 최소 한도 (°)
            private const float SAGARM_GUARD_DEG_PER_SEC = 60f;      //   직전 전송 뒤 시간에 비례하는 한도 (°/s — 다리의 관절 최고 속도 20°/s 의 세 배, 시간은 0.1초까지)
            private const int SAGARM_SPLIT_DEPTH = 3;                // 관절 보간 경로 확인이 안 되면 TCP 가운데 점으로 나누는 깊이 (최대 8 조각)
            private const float SAGARM_VERT_DEV_MM = 3f;             // 수직 다리의 수평 벗어남 한도 (mm)
            private const float SAGARM_BAND_MM = 15f;                // 다리 경로의 반경 띠 — 두 끝 반경 ±15mm (팔 구성이 뒤집힌 해·베이스 쪽 쏠림을 막는다)

            // 다리 하나 — 지금 자세에서 TCP (x, y, z)·관절 합 fSum 으로 관절 보간. 미리: 끝 자세 = 관절 합을 정한 IK (지금 자세 시드) → 관절 보간 경로를 FK 로 훑어
            //   TCP 높이 ≥ zFloor, 반경이 두 끝 ±15mm 띠 안, devH > 0 이면 수평으로 두 끝 선분에서 devH 안 — 안 되면 TCP 가운데 점으로 나눠 다시 본다 (SagArm_PlanSplit).
            //   다 되면 조각마다 Play (관절 보간) → 조각 도착 (3D 관절 = 목표)·한 틱 관절 변화 막이·싱크 확인 → 끝 TCP·관절 합 확인. 미리 확인이 안 되면 움직이지 않는다.
            //   시간 = 조각마다 max(거리 ÷ TransferMmPerSec, 1.5 × 최대 관절 변화 ÷ 20°/s), 합이 nMinMs 보다 짧으면 늘림, ÷ Speed, 조각 최소 300ms
            private bool SagArm_LegTo(CSagArm arm, string strName, float x, float y, float z, float fSum, float zFloor, float devH, int nMinMs, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                C3d kin = SagRun_Kin();
                if (kin == null) return SagArm_Fail(arm, L, "★" + SAGRUN_NO_KIN);
                if (!SagArm_SinkOk(arm, L)) return false;
                string g = SagArm_GuardWhy(arm);
                if (g.Length > 0) return SagArm_Fail(arm, L, "★" + strName + ": " + g + " — 움직이지 않음");
                float[] qA = SagArm_QNow(c);
                double[] adBase = SagArm_SeedNow(kin, c);
                float xa, ya, za, pa;
                SagArm_Tcp(c, out xa, out ya, out za, out pa);
                float[] pA = { xa, ya, za }, pB = { x, y, z };
                float bend;
                float[] qB = SagArm_IkSum(kin, c, adBase, x, y, z, fSum, out bend);
                if (qB == null)
                    return SagArm_Fail(arm, L, string.Format(ci, "★{0}: IK 로 못 감 ({1:F1}, {2:F1}, {3:F1}) 관절 합 {4:0.#} — 움직이지 않음", strName, x, y, z, fSum));
                double rA = Math.Sqrt((double)xa * xa + (double)ya * ya), rB = Math.Sqrt((double)x * x + (double)y * y);
                float rLo = (float)Math.Min(rA, rB) - SAGARM_BAND_MM, rHi = (float)Math.Max(rA, rB) + SAGARM_BAND_MM;
                var lq = new List<float[]>(); var lp = new List<float[]>();
                string why;
                if (!SagArm_PlanSplit(kin, c, adBase, qA, pA, qB, pB, qA[1] + qA[2] + qA[3], fSum, zFloor, rLo, rHi, devH, 0, lq, lp, out why))
                    return SagArm_Fail(arm, L, string.Format(ci, "★{0}: 관절 보간 경로 확인 실패 ({1:F1}, {2:F1}, {3:F1}) → ({4:F1}, {5:F1}, {6:F1}) — {7} — 움직이지 않음", strName, xa, ya, za, x, y, z, why));
                foreach (float[] q in lq)
                {
                    string wl = SagArm_LimitWhy(c, q);
                    if (wl.Length > 0) return SagArm_Fail(arm, L, "★" + strName + ": " + wl + " — 움직이지 않음");
                }
                double[] ms = new double[lq.Count];
                double sum = 0;
                float[] qp = qA, pp = pA;
                for (int i = 0; i < lq.Count; i++)
                {
                    double dist = Math.Sqrt((double)(lp[i][0] - pp[0]) * (lp[i][0] - pp[0]) + (double)(lp[i][1] - pp[1]) * (lp[i][1] - pp[1]) + (double)(lp[i][2] - pp[2]) * (lp[i][2] - pp[2]));
                    ms[i] = Math.Max(dist / Math.Max(1.0, c.TransferMmPerSec) * 1000.0, 1.5 * SagArm_MaxDq(qp, lq[i]) / SAGARM_JOINT_DEG_PER_SEC * 1000.0);
                    sum += ms[i]; qp = lq[i]; pp = lp[i];
                }
                double scale = (sum > 0 && sum < nMinMs) ? nMinMs / sum : 1.0;
                arm.Moved = true;                                            // 여기부터 움직인다 (미리 확인을 지났다)
                for (int i = 0; i < lq.Count; i++)
                {
                    int t = Math.Max(300, (int)(ms[i] * scale / Math.Max(0.1f, arm.Speed)));
                    if (!SagArm_PlayQ(arm, strName, lq[i], t, i == lq.Count - 1 ? 150 : 0, L)) return false;
                }
                return SagArm_Arrived(arm, strName, x, y, z, fSum, L);
            }
            // 다리 계획 — qA(TCP pA) → qB(pB) 관절 보간이 확인(SagArm_PathWhy)을 지나면 qB 를 넣고, 아니면 TCP 가운데 점(관절 합도 가운데)을
            //   관절 합을 정한 IK 로 풀어(시드 qA) 둘로 나눠 다시 본다 (깊이 SAGARM_SPLIT_DEPTH 까지). 반환 = 조각 끝 자세·TCP 목록을 채움
            private static bool SagArm_PlanSplit(C3d kin, CSagArmCfg c, double[] adBase, float[] qA, float[] pA, float[] qB, float[] pB, float sumA, float sumB,
                                                 float zFloor, float rLo, float rHi, float devH, int depth, List<float[]> outQ, List<float[]> outP, out string why)
            {
                why = SagArm_PathWhy(kin, c, adBase, qA, qB, pA, pB, zFloor, rLo, rHi, devH);
                if (why.Length == 0) { outQ.Add(qB); outP.Add(pB); return true; }
                if (depth >= SAGARM_SPLIT_DEPTH) return false;
                float[] pM = { (pA[0] + pB[0]) / 2, (pA[1] + pB[1]) / 2, (pA[2] + pB[2]) / 2 };
                float sM = (sumA + sumB) / 2, bend;
                float[] qM = SagArm_IkSum(kin, c, SagArm_AdWith(kin, c, adBase, qA), pM[0], pM[1], pM[2], sM, out bend);
                if (qM == null) { why = SagRun_WhyJoin(why, "가운데 점 IK 못 함"); return false; }
                if (!SagArm_PlanSplit(kin, c, adBase, qA, pA, qM, pM, sumA, sM, zFloor, rLo, rHi, devH, depth + 1, outQ, outP, out why)) return false;
                return SagArm_PlanSplit(kin, c, adBase, qM, pM, qB, pB, sM, sumB, zFloor, rLo, rHi, devH, depth + 1, outQ, outP, out why);
            }
            // 관절 직선 보간 qA → qB 를 복제 기구학 FK 로 훑는다 (1° 마다, 최소 8 점) — TCP 높이 ≥ zFloor − 0.5, 반경 [rLo, rHi] 안,
            //   devH > 0 이면 수평으로 pA→pB 선분(xy)에서 devH 안. 반환 = 까닭 (괜찮으면 "")
            private static string SagArm_PathWhy(C3d kin, CSagArmCfg c, double[] adBase, float[] qA, float[] qB, float[] pA, float[] pB, float zFloor, float rLo, float rHi, float devH)
            {
                var ci = CultureInfo.InvariantCulture;
                int[] ids = c.MoveIDs;
                int n = Math.Max(8, Math.Min(400, (int)Math.Ceiling(SagArm_MaxDq(qA, qB) / 1.0)));
                double[] ad = (double[])adBase.Clone();
                double ux = pB[0] - pA[0], uy = pB[1] - pA[1], uu = ux * ux + uy * uy;
                for (int i = 1; i < n; i++)
                {
                    double s = i / (double)n;
                    for (int k = 0; k < 4; k++) kin.SagFc_Set(ad, ids[k], (float)(qA[k] + s * (qB[k] - qA[k])));
                    float x, y, z;
                    kin.SagFc_Fk(c.FuncNumber, ad, -1, out x, out y, out z);
                    if (z < zFloor - 0.5f) return string.Format(ci, "TCP 가 z {0:F1} 까지 내려감 (하한 {1:F1})", z, zFloor);
                    double r = Math.Sqrt((double)x * x + (double)y * y);
                    if (r < rLo || r > rHi) return string.Format(ci, "TCP 반경 {0:F0} 이 {1:F0}~{2:F0} 밖", r, rLo, rHi);
                    if (devH > 0)
                    {
                        double t = uu > 1e-9 ? Math.Max(0, Math.Min(1, ((x - pA[0]) * ux + (y - pA[1]) * uy) / uu)) : 0;
                        double hx = x - (pA[0] + t * ux), hy = y - (pA[1] + t * uy), h = Math.Sqrt(hx * hx + hy * hy);
                        if (h > devH) return string.Format(ci, "수평으로 {0:F1}mm 벗어남 (한도 {1:0.#})", h, devH);
                    }
                }
                return "";
            }
            // 관절 보간 한 조각 (위치 3개 + 손목) → 조각 도착 확인: 3D 관절이 목표에서 0.05° 안 (아니면 한 틱 관절 변화 막이가 멈추고 되돌렸거나 관절 한계·시간 초과)
            //   + 막이·싱크 확인
            private bool SagArm_PlayQ(CSagArm arm, string strName, float[] q, int nMs, int nDelay, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                if (!SagArm_SinkOk(arm, L)) return false;
                int[] ids = arm.Cfg.MoveIDs;
                Play(nMs, nDelay, new float[] { ids[0], q[0], ids[1], q[1], ids[2], q[2], ids[3], q[3] });
                string g = SagArm_GuardWhy(arm);
                if (g.Length > 0) return SagArm_Fail(arm, L, "★" + strName + ": " + g + " — 멈춤");
                if (arm.IsDry && MoveJoints_GetMotionSink() != arm.DrySink) return SagArm_SinkOk(arm, L);
                float e = 0;
                for (int k = 0; k < 4; k++) e = Math.Max(e, Math.Abs(GetData(ids[k]) - q[k]));
                if (!(e <= 0.05f)) return SagArm_Fail(arm, L, string.Format(ci, "★{0}: 도착 확인 실패 — 3D 관절이 목표와 {1:F2}° 다름 (관절 한계·멈춤)", strName, e));
                return true;
            }
            // 다리 끝 확인 — 3D TCP 가 목표에서 IkTolMm 안, 관절 합이 fSum 에서 PitchTolDeg 안
            private bool SagArm_Arrived(CSagArm arm, string strName, float x, float y, float z, float fSum, CSim2Real.DLog L)
            {
                CSagArmCfg c = arm.Cfg;
                float ax, ay, az, ap;
                SagArm_Tcp(c, out ax, out ay, out az, out ap);
                double d = Math.Sqrt((double)(ax - x) * (ax - x) + (double)(ay - y) * (ay - y) + (double)(az - z) * (az - z));
                float s = SagArm_SumNow(c);
                if (d <= c.IkTolMm && Math.Abs(s - fSum) <= c.PitchTolDeg) return true;
                return SagArm_Fail(arm, L, string.Format(CultureInfo.InvariantCulture, "★이동 확인 실패 ({0}): 목표 ({1:F1}, {2:F1}, {3:F1}) 관절 합 {4:0.#} — 도착 ({5:F1}, {6:F1}, {7:F1}) 관절 합 {8:F1}, 차 {9:F1}mm",
                    strName, x, y, z, fSum, ax, ay, az, s, d));
            }
            // 화면 3D 의 관절 한계(CalcLimit — 연결 중이면 모터 세팅 리밋 ∩ EDH)에 걸리는 축이 있으면 까닭 (없으면 "") — 걸리면 Play 가 한계에서 멈춰 끝 자세가 틀어진다
            private string SagArm_LimitWhy(CSagArmCfg c, float[] q)
            {
                int[] ids = c.MoveIDs;
                for (int k = 0; k < 4 && k < q.Length; k++)
                {
                    float f = CalcLimit(ids[k], q[k]);
                    if (Math.Abs(f - q[k]) > 0.01f)
                        return string.Format(CultureInfo.InvariantCulture, "ID {0} 목표 {1:F1}° 가 관절 한계 밖 (한계 {2:F1}°)", ids[k], q[k], f);
                }
                return "";
            }
            private static float SagArm_MaxDq(float[] a, float[] b)
            {
                float m = 0;
                for (int k = 0; k < 4 && k < a.Length && k < b.Length; k++) m = Math.Max(m, Math.Abs(b[k] - a[k]));
                return m;
            }
            // 실패 — 까닭을 손잡이에 남기고(작업 결과의 Why 에 붙는다) 로그. 늘 false
            private static bool SagArm_Fail(CSagArm arm, CSim2Real.DLog L, string strWhy)
            {
                if (arm != null) arm.LastFail = (strWhy ?? "").TrimStart('★');
                if (L != null) L(strWhy);
                return false;
            }
            // 장면의 한 틱 관절 변화 막이가 걸렸으면 그 까닭 (3D 만·막이 없음이면 "")
            private static string SagArm_GuardWhy(CSagArm arm)
            {
                if (arm == null || arm.IsDry || arm.GuardScene == null) return "";
                string t = arm.GuardScene.StepGuardTrip;
                return t ?? "";
            }
            // 모션 싱크가 이 작업의 것인가 — 3D 만: 건 빈 싱크 그대로 (작업 중에 바뀌었으면 — 그사이 연결 등 — 3D 를 움직이면 실물로 나갈 수 있어 멈춘다, SinkLost).
            //   실물: 싱크가 비어 있으면(실물로 아무것도 나가지 않아 3D 만 움직인다) 장면의 SendPose 를 다시 건다 — 원위치(SagArm_Close)는 장면의 것이 아니면 늘 다시 건다
            private bool SagArm_SinkOk(CSagArm arm, CSim2Real.DLog L)
            {
                if (arm == null) return false;
                if (!arm.IsDry) { if (MoveJoints_GetMotionSink() == null) SagArm_EnsureSceneSink(arm); return true; }
                if (!arm.SinkLost && MoveJoints_GetMotionSink() == arm.DrySink) return true;
                if (!arm.SinkLost) SagArm_Fail(arm, L, "★모션 싱크가 작업 중에 바뀜 (3D 만 작업 — 그사이 연결 등) — 3D 를 더 움직이지 않고 멈춤");
                arm.SinkLost = true;
                return false;
            }
            // 지금 3D TCP·공구 피치 — 관절값 사본 FK (3D 를 건드리지 않는다)
            private void SagArm_Tcp(CSagArmCfg c, out float fX, out float fY, out float fZ, out float fPitch)
            {
                double[] ad = SagFc_MotCopy();
                SagFc_Fk(c.FuncNumber, ad, -1, out fX, out fY, out fZ);
                fPitch = SagFc_ToolPitch(c.FuncNumber, ad, c.WristID);
            }
            // 지금 3D 자세 (위치 3개 + 손목) / 그 관절 합 12+13+14
            private float[] SagArm_QNow(CSagArmCfg c)
            {
                return new float[] { GetData(c.PosIDs[0]), GetData(c.PosIDs[1]), GetData(c.PosIDs[2]), GetData(c.WristID) };
            }
            private float SagArm_SumNow(CSagArmCfg c) { return GetData(c.PosIDs[1]) + GetData(c.PosIDs[2]) + GetData(c.WristID); }
            // 복제 기구학 관절값 사본에 지금 3D 자세(위치 3개 + 손목)를 넣는다 — IK 미리 확인의 시드
            private double[] SagArm_SeedNow(C3d kin, CSagArmCfg c)
            {
                double[] ad = kin.SagFc_MotCopy();
                foreach (int id in c.MoveIDs) kin.SagFc_Set(ad, id, GetData(id));
                return ad;
            }
            private Dictionary<int, float> SagArm_Pose3D(int[] ids)
            {
                var d = new Dictionary<int, float>();
                foreach (int id in ids) d[id] = GetData(id);
                return d;
            }
            // 관절 보간 (위치 3개 + 손목만 — 그리퍼는 그대로) — 관절 최고 속도 20°/s 를 넘지 않게 시간을 늘린다 (원위치 — Speed 로 나눈 뒤)
            private bool SagArm_JointTo(CSagArm arm, Dictionary<int, float> tgt, int nMs)
            {
                var v = new List<float>();
                float dq = 0;
                foreach (int id in arm.Cfg.MoveIDs) { float f; if (tgt.TryGetValue(id, out f)) { v.Add(id); v.Add(f); dq = Math.Max(dq, Math.Abs(f - GetData(id))); } }
                if (v.Count == 0) return false;
                int ms = Math.Max(nMs, (int)(1.5 * dq / SAGARM_JOINT_DEG_PER_SEC * 1000.0 / Math.Max(0.1f, arm.Speed)));
                return Play(ms, 300, v.ToArray());
            }
            // 휴지 자세(시작 때 정착 읽기)와 지금 실물(정착 읽기)의 최대 차 — 위치 3개 + 손목. 못 읽으면 NaN.
            //   시작 때 토크 ON 이었으면(서보가 붙잡은 목표에 보정이 들어 있었다) 서보 목표에 든 보정이 시작 때(StartApplied)와 지금(직전 출력 틱) 다른 만큼
            //   실물도 옮겨 서 있다 (같은 자세에서 처짐은 같다) — 휴지 자세 + (지금 보정 − 시작 때 보정) 과 비교한다 (10-03 검토: 사용자가 작업 직전에 보정을 끄면
            //   시작 목표엔 보정이 있고 끝 목표엔 없어 처짐만큼(먼 휴지 자세 5°) 쓸모없는 재시도·RestOk false 가 나왔다).
            //   시작 때 토크 OFF 였으면 휴지 자세는 힘 뺀 채 읽은 자리라 차 없이 (예전과 같다)
            private float SagArm_RestDiff(CSagArm arm)
            {
                if (arm.Scene == null || arm.RestPose == null) return float.NaN;
                string w;
                Dictionary<int, float> d = arm.Scene.ReadDeg(out w);
                if (d == null) return float.NaN;
                int[] ids = arm.Cfg.MoveIDs;
                float[] now = SagComp_AppliedNow(ids);
                float mx = 0;
                for (int i = 0; i < ids.Length; i++)
                {
                    float a, b, s0 = 0;
                    if (!d.TryGetValue(ids[i], out a) || !arm.RestPose.TryGetValue(ids[i], out b)) return float.NaN;
                    bool bShift = arm.WasTorqueOn && arm.StartApplied != null;
                    if (bShift) arm.StartApplied.TryGetValue(ids[i], out s0);
                    float shift = bShift ? now[i] - s0 : 0f;
                    mx = Math.Max(mx, Math.Abs(a - (b + shift)));
                }
                return mx;
            }
            // 실물 작업 — 모션 싱크가 장면의 것인지 (누름 측정은 싱크를 잠깐 바꿨다 되돌린다 — 예외로 끊겼어도 원위치는 실물로 가게). null 로는 바꾸지 않는다
            private void SagArm_EnsureSceneSink(CSagArm arm)
            {
                if (arm.Scene == null) return;
                Action<int> cur = MoveJoints_GetMotionSink();
                if (cur == null || !ReferenceEquals(cur.Target, arm.Scene)) MoveJoints_SetMotionSink(new Action<int>(arm.Scene.SendPose));
            }
            // 3D 만 — 건 빈 싱크를 시작 때 싱크로 (그사이 다른 것이 바꿔 놓았으면 건드리지 않는다)
            private void SagArm_RestoreSink(CSagArm arm, CSim2Real.DLog L)
            {
                Action<int> cur = MoveJoints_GetMotionSink();
                if (cur == arm.DrySink) MoveJoints_SetMotionSink(arm.PrevSink);
                else if (L != null) L("★모션 싱크가 작업 중에 바뀌어 있음 — 되돌리지 않음");
                arm.SinkSet = false;
            }
            private static string SagArm_BusyName(CArmConn_t conn)
            {
                string o = conn.BusyOwner;
                return (o == null) ? "3D 모션 또는 짧은 동작" : o;
            }
            private static void SagArm_WriteRest(CSagArm arm)
            {
                try
                {
                    if (string.IsNullOrEmpty(arm.RunDir) || arm.RestPose == null) return;
                    var ci = CultureInfo.InvariantCulture;
                    var sb = new StringBuilder("// 휴지 자세 — 작업 시작 때 정착 읽기 (deg), " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", ci) + ", 시작 때 토크 " + (arm.WasTorqueOn ? "ON" : "OFF") + "\r\n");
                    var ids = new List<int>(arm.RestPose.Keys); ids.Sort();
                    foreach (int id in ids) sb.Append(id.ToString(ci)).Append('=').Append(arm.RestPose[id].ToString("0.###", ci)).Append("\r\n");
                    File.WriteAllText(Path.Combine(arm.RunDir, "rest_pose.txt"), sb.ToString(), new UTF8Encoding(false));
                }
                catch { }
            }
            // 자세를 보내며 붙잡기 — 보정 전환이 끝날 때까지 같은 자세(위치 3개 + 손목)를 제자리 관절 보간으로 계속 보낸다 (최대 nMaxMs).
            //   전환은 출력 틱이 흘러야 나아간다 — 멈춰 기다리기만 하면 실물에 새 보정이 들어가지 않는다 (10-02 검토). 반환 = 전환 끝
            //   (중단 요청·한 틱 관절 변화 막이·3D 만 싱크 바뀜이면 그 자리에서 false)
            private bool SagArm_HoldUntilBlended(CSagArm arm, int nMaxMs, CSim2Real.DIsStopped stop)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int[] ids = arm.Cfg.MoveIDs;
                while (SagComp_BlendProgress < 1.0 && sw.ElapsedMilliseconds < nMaxMs)
                {
                    if (SagRun_Stopped(stop)) return false;
                    if (SagArm_GuardWhy(arm).Length > 0) return false;
                    if (arm.IsDry && !SagArm_SinkOk(arm, null)) return false;
                    var v = new List<float>();
                    foreach (int id in ids) { v.Add(id); v.Add(GetData(id)); }
                    Play(200, 0, v.ToArray());
                }
                return SagComp_BlendProgress >= 1.0;
            }

            // ────────────────────────────────────────────────────────────
            // ③ 바닥 측정 · ⑤ 결과 파일 시험 (닿기·틈) — 작업 (블로킹, 작업 스레드)
            //   Job = 확인(연결·잠금·계획) → 작업 폴더 → SagArm_Open → 펼치기 → 본체 → finally SagArm_Close → 보고서.
            //   실물 작업은 연결 안 됨·구성 불일치·잠금 사용 중이면 포트를 건드리기 전에 깨끗이 거절한다 (Ok = false, Why·Line).
            //   방향 확인 안 된 축(UncheckedAxes, U4)·결과 파일 매핑 차이(MotorMapWarn, C10)는 결과에 담아 알릴 뿐 막지 않는다.
            //   로그는 이동·점마다 (틱마다 없음), "진행 " 으로 시작하는 줄은 상태 표시용, "★" 는 경고·오류. 작업 폴더의 run.log 에도 남는다.
            // ────────────────────────────────────────────────────────────

            /// <summary>바닥 측정 작업 (버튼 하나 = 이것 하나, 블로킹 — 작업 스레드). conn = 연결 객체 (bDry 면 null 이어도 된다), plan = 측정 계획
            /// (사본으로 다시 확인한다 — 자리 (X, Y) = 반경·요, 창 시작 ≥ 바닥 예상 높이 + 5: 계획의 예측과 첫닿음 모델(적용 중 파일·측정 모델 파일에 있으면 그것,
            /// 행에 적힌 계획을 만든 모델 파일)로 다시 낸 예측 중 높은 쪽 — 다시 예측할 모델이 없는 행은 재지 않는다, 옮기는 높이·IK.
            /// IkOk = false 로 들어온 행 — 계획 파일의 ik=0 등 — 은 다시 넣지 않는다: 재는 행은 부른 쪽이 센 IkOk 행 수를 넘지 않는다),
            /// cfg = 측정 설정 (CollectModelPath·DatasetPath 가 비면 urdf 폴더의 적용 중 파일·floor_dataset_fc.csv). 잠금 소유자 "측정".
            /// 작업 폴더 <urdf>\sagcomp_runs\<yyyyMMdd_HHmmss>_measure[_3d] (run.log, rest_pose.txt, floor_plan.csv, floor_trace.csv, floor_live.csv,
            /// floor_dataset_run.csv, measure_report.txt). 3D 만이면 실물은 움직이지 않고 데이터셋도 건드리지 않는다. 실물은 이 로봇·정의·모터 영점으로 수직 눈 확인을 했어야 한다 (SagPrac_EyeNeeded — 아니면 움직이기 전에 거절: SAGPRAC_EYE_FIRST). 예외 없음.</summary>
            public CSagRunResult SagRun_MeasureJob(CArmConn_t conn, CSagArmCfg armCfg, List<CFloorPlanRow> plan, CSagMeasureCfg cfg, string strUrdfDir, bool bDry,
                                                   CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_MeasureJobBody(conn, armCfg, plan, cfg, strUrdfDir, bDry, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagRunResult SagRun_MeasureJobBody(CArmConn_t conn, CSagArmCfg armCfg, List<CFloorPlanRow> plan, CSagMeasureCfg cfg, string strUrdfDir, bool bDry,
                                                        CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagRunResult { Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                DateTime t0 = DateTime.Now;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                CSagMeasureCfg mc = null;
                bool bRan = false;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    if (cfg == null) cfg = new CSagMeasureCfg();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagRun_RunRefuse(res, why, L);
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, out why)) return SagRun_RunRefuse(res, why, L);   // 실물 — 이 로봇·정의·모터 영점으로 수직 눈 확인을 했나 (움직이기 전)
                    if (plan == null || plan.Count == 0) return SagRun_RunRefuse(res, "계획 없음 — ② 계획 만들기 먼저", L);
                    string runDir = SagRun_NewRunDir(strUrdfDir, t0, bDry ? "measure_3d" : "measure");
                    res.RunDir = runDir;
                    L = SagRun_SafeLog(log, Path.Combine(runDir, "run.log"));
                    L((bDry ? "3D 측정 미리보기" : "바닥 측정") + " 시작 — " + t0.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    mc = new CSagMeasureCfg { Probe = (cfg.Probe != null ? cfg.Probe : new CSagMeasureCfg().Probe).Clone(), WristHold = cfg.WristHold,
                                              CollectModelPath = SagRun_Resolve(strUrdfDir, cfg.CollectModelPath, SAGRUN_ACTIVE),
                                              DatasetPath = SagRun_Resolve(strUrdfDir, cfg.DatasetPath, SAGRUN_FC_DATASET) };
                    List<CFloorPlanRow> work = SagRun_ClonePlan(plan);
                    string repIk;
                    // 예측 첫닿음이 없는 행(예전 계획 파일)은 재지 않는다. 첫닿음은 적용 중 파일·측정 모델 파일(첫닿음 모델이 있으면)과 행에 적힌 계획을 만든 모델로
                    //   다시 예측해 창 시작을 확인한다 — 다시 예측할 모델이 없으면 재지 않는다 (10-03 검토: 계획 파일의 pred 칸만 보면 창과 함께 낮춘 행이 지나갔다)
                    int nOk = SagRun_CheckPlanCore(work, armCfg, mc.Probe.RetractAboveKneeMm, true,
                                                   SagRun_FcModelsAt(new string[] { SagRun_ActivePath(strUrdfDir), mc.CollectModelPath }), true, true, out repIk);
                    SagRun_WriteText(Path.Combine(runDir, "floor_plan.csv"), SagRun_PlanCsv(work));
                    if (nOk == 0) return SagRun_RunRefuse(res, "측정할 행 없음 —" + repIk.Replace("\r\n", " /"), L);
                    L(repIk.Trim());
                    res.RowsPlanned = nOk;                                   // 펼치기에서 멈춰도 보고에 계획 행 수 (본체가 다시 센다)
                    res.UncheckedAxes = SagArm_UncheckedAxes(conn, armCfg);
                    if (res.UncheckedAxes.Length > 0) L("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes) + " — 계속 (확인 표를 채우십시오)");
                    string whyM;
                    CSagComp mMove = SagRun_LoadJointOnly(mc.CollectModelPath, out whyM);   // 측정 모델 (관절 모델만) — 누름과 옮기기 모두 이 보정 (10-03)
                    if (mMove == null) return SagRun_RunRefuse(res, whyM, L);
                    res.MotorMapWarn = SagArm_MotorMapWarn(bDry ? null : conn, mMove);
                    if (res.MotorMapWarn.Length > 0) L(res.MotorMapWarn);
                    arm = SagArm_Open(conn, armCfg, runDir, "측정", bDry, L, out why);
                    if (arm == null) return SagRun_RunRefuse(res, why, L);
                    SagArm_SetMoveModel(arm, mMove, L);
                    bRan = true;
                    CFloorPlanRow first = null;
                    foreach (CFloorPlanRow r in work) if (r.IkOk) { first = r; break; }
                    if (!SagArm_Unfold(arm, first.X, first.Y, first.TransferZ, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop);
                        res.Why = res.Stopped ? "중단 요청" : SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else SagRun_MeasureCore(arm, work, mc, res, L, stop, t0);
                }
                catch (Exception ex) { res.Ok = false; res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally
                {
                    if (arm != null) res.RestOk = SagArm_Close(arm, L);
                }
                if (bRan)
                {
                    res.ElapsedSec = sw.Elapsed.TotalSeconds;
                    res.Ok = res.Why.Length == 0 && !res.Stopped && res.RowsDone == res.RowsPlanned && res.RowsDone > 0;
                    SagRun_MeasureReport(res, mc, L);
                }
                return res;
            }

            /// <summary>바닥 측정 본체 (블로킹, 작업 스레드) — 열린 손잡이로 계획의 IkOk 행을 차례로 누른다. 펼치기·원위치는 부른 쪽 (SagRun_MeasureJob 이 한다).
            /// cfg.CollectModelPath (관절 모델만 쓴다 — 바닥 부분 끔)·DatasetPath 가 채워져 있어야 한다.
            /// 행마다: 줄(세트/피치/요)이 바뀌면 그 행의 옮기는 높이로 안전 이동 → SagComp_FloorPress (반경 하나, 계획 창, 한 번, Margin ≥ 20, 손목 유지 cfg.WristHold)
            /// — 3D 만이면 SagRun_DryPress (예상 높이까지 내려가는 모습). 궤적은 점마다 작업 폴더 floor_trace.csv 에 덧붙이고, 접촉은 floor_live.csv 에.
            /// 끝 (멈춰도, 실물): 궤적 재분석(첫닿음 켬) → floor_dataset_run.csv + 데이터셋에 덧붙임 (새 측정 회차 이름), kind 별 수, 첫닿음 FK − 계획 예측.
            /// 예측 첫닿음(PredZ)이 없는 행은 재지 않는다 (창 시작이 바닥 위인지 모른다). 중단 요청은 점 사이·하강 중·마지막 행 뒤에 본다 — 하강 중에 끊긴 행은
            /// 잰 것으로 치지 않고(RowsDone 에 넣지 않음) 그 궤적은 floor_trace_stopped.csv 로 (데이터셋에 넣지 않는다). 한 틱 관절 변화 막이가 걸린 행도 같다
            /// (그때는 Why). 받은 계획은 작업(SagRun_MeasureJob)과 같이 다시 확인한다 (사본 — 부른 쪽 목록은 그대로, 10-03 검토): SagRun_CheckPlan (예측 첫닿음 필요,
            /// 자리 (X, Y) = 반경·요, 창 시작 ≥ 바닥 예상 높이 + ABOVE_MIN — 바닥 예상 높이 = 계획의 예측과 첫닿음 모델(측정 모델 파일 cfg.CollectModelPath 에
            /// 첫닿음 모델이 있으면 그것, 행에 적힌 계획을 만든 모델 파일)로 다시 낸 예측 중 높은 쪽, 다시 예측할 모델이 없으면 그 행은 빼고, 옮기는 높이·IK) 를 지난
            /// 행만 누른다 — 손으로 고친 계획(ik=1, pred 칸을 창과 함께 낮춘 행, r 만 고친 행)도 바닥 아래로 그냥 내려가지 않는다. ik=0 행(IkOk = false 로 들어온 행)은
            /// 다시 넣지 않는다. 지난 행이 없으면 움직이지 않는다. 예외 없음.</summary>
            public CSagRunResult SagRun_Measure(CSagArm arm, List<CFloorPlanRow> plan, CSagMeasureCfg cfg, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_MeasureBody(arm, plan, cfg, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagRunResult SagRun_MeasureBody(CSagArm arm, List<CFloorPlanRow> plan, CSagMeasureCfg cfg, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagRunResult();
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    // 밖에서 받은 계획 — 작업과 같은 높이·IK 확인 (부른 쪽이 SagRun_PlanParse 로 읽었거나 손으로 고친 행도)
                    List<CFloorPlanRow> work = plan;
                    if (arm != null && !arm.Closed && plan != null && cfg != null)
                    {
                        work = SagRun_ClonePlan(plan);
                        string repIk;
                        int nOk = SagRun_CheckPlanCore(work, arm.Cfg, (cfg.Probe != null ? cfg.Probe : new CSagMeasureCfg().Probe).RetractAboveKneeMm, true,
                                                       SagRun_FcModelsAt(new string[] { cfg.CollectModelPath }), true, true, out repIk);
                        L(repIk.Trim());
                        if (nOk == 0) { res.Dry = arm.IsDry; res.RunDir = arm.RunDir; res.Why = "측정할 행 없음 —" + repIk.Replace("\r\n", " /"); L("★" + res.Why); }
                    }
                    if (res.Why.Length == 0) SagRun_MeasureCore(arm, work, cfg, res, L, stop, DateTime.Now);
                }
                catch (Exception ex) { res.Ok = false; res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                res.ElapsedSec = sw.Elapsed.TotalSeconds;
                res.Ok = res.Why.Length == 0 && !res.Stopped && res.RowsDone == res.RowsPlanned && res.RowsDone > 0;
                SagRun_MeasureReport(res, cfg, L);
                return res;
            }
            private void SagRun_MeasureCore(CSagArm arm, List<CFloorPlanRow> plan, CSagMeasureCfg cfg, CSagRunResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop, DateTime tStart)
            {
                var ci = CultureInfo.InvariantCulture;
                if (arm == null || arm.Closed) { res.Why = "작업 손잡이 없음 — SagArm_Open 먼저"; L("★" + res.Why); return; }
                res.Dry = arm.IsDry; if (res.RunDir == null) res.RunDir = arm.RunDir;
                if (plan == null || cfg == null) { res.Why = "계획·설정 없음"; L("★" + res.Why); return; }
                string why;
                CSagComp collect = SagRun_LoadJointOnly(cfg.CollectModelPath, out why);
                if (collect == null) { res.Why = why; L("★" + why); return; }
                var rows = new List<CFloorPlanRow>();
                int nNoPred = 0;
                foreach (CFloorPlanRow r in plan)
                {
                    if (r == null || !r.IkOk || float.IsNaN(r.ZStart) || float.IsNaN(r.ZMin)) continue;
                    if (float.IsNaN(r.PredZ) || float.IsInfinity(r.PredZ)) { nNoPred++; continue; }   // 예측 첫닿음이 없는 행 — 창 시작이 바닥 위인지 모른다 (SagRun_CheckPlan bNeedPred)
                    rows.Add(r);
                }
                if (nNoPred > 0) L(string.Format(ci, "★예측 첫닿음이 없는 행 {0} 은 재지 않음 — ② 계획 만들기로 다시", nNoPred));
                res.RowsPlanned = rows.Count;
                if (rows.Count == 0) { res.Why = "측정할 행 없음 (IK 확인 통과·예측 있는 행 0)"; L("★" + res.Why); return; }
                CFloorProbeCfg p0 = (cfg.Probe != null ? cfg.Probe : new CSagMeasureCfg().Probe).Clone();
                p0.FuncNumber = arm.Cfg.FuncNumber; p0.PosIDs = (int[])arm.Cfg.PosIDs.Clone(); p0.WristID = arm.Cfg.WristID; p0.DownSumDeg = arm.Cfg.DownSumDeg;
                p0.WristHold = cfg.WristHold; p0.Repeats = 1; if (p0.Margin < 20) p0.Margin = 20;
                string dir = arm.RunDir;
                string tracePath = (!arm.IsDry && !string.IsNullOrEmpty(dir)) ? Path.Combine(dir, "floor_trace.csv") : null;
                string cutPath = (!arm.IsDry && !string.IsNullOrEmpty(dir)) ? Path.Combine(dir, "floor_trace_stopped.csv") : null;   // 끝까지 누르지 않은 하강 (데이터셋에 넣지 않는다)
                string livePath = !string.IsNullOrEmpty(dir) ? Path.Combine(dir, "floor_live.csv") : null;
                res.TracePath = tracePath; res.LivePath = livePath;
                if (livePath != null) SagRun_WriteText(livePath, SagRun_ContactCsvHeader());
                L(string.Format(ci, "측정 {0}행 — 손목 수직 유지 {1}, 관절 모델 {2}", rows.Count, cfg.WristHold ? "켬" : "끔", Path.GetFileName(cfg.CollectModelPath)));
                bool bHdr = false, bHdrCut = false;
                if (!arm.IsDry) SagComp_ClearFloorTrace();
                string lastKey = null;
                try
                {
                    for (int k = 0; k < rows.Count; k++)
                    {
                        CFloorPlanRow row = rows[k];
                        if (SagRun_Stopped(stop)) { res.Stopped = true; L("중단 요청 — 측정 멈춤, 원위치"); break; }
                        if (row.LineKey != lastKey)
                        {
                            float tz = float.IsNaN(row.TransferZ) ? arm.Cfg.SafeZ : row.TransferZ;
                            L(string.Format(ci, "── 줄 {0}: 옮기는 높이 z={1:0.#} 로 들어 옮김 ──", row.LineKey, tz));
                            if (!SagArm_MoveSafeCore(arm, row.X, row.Y, tz, row.Pitch, tz, row.ZStart + 10, L, stop))
                            {
                                if (arm.StopSeen) { res.Stopped = true; L("중단 요청 — 측정 멈춤 (옮기던 다리를 마친 자리에서), 원위치"); break; }
                                res.Why = SagRun_WhyJoin(string.Format(ci, "옮기기 실패 ({0} r{1:0.#}) — 측정 멈춤", row.LineKey, row.R), arm.LastFail); break;
                            }
                            lastKey = row.LineKey;
                        }
                        CFloorProbeCfg p = p0.Clone();
                        p.Radii = new float[] { row.R }; p.YawDeg = row.Yaw; p.RyDeg = row.Pitch; p.ZStart = row.ZStart; p.ZMin = row.ZMin;
                        List<CFloorContact> cs;
                        if (!arm.IsDry)
                        {
                            string rep;
                            cs = SagComp_FloorPress(arm.Scene, p, collect, L, stop, out rep);
                            // 하강 중 중단 요청(UserStop)·한 틱 관절 변화 막이로 끊긴 누름은 잰 것으로 치지 않는다 — 궤적은 따로 두고(floor_trace_stopped.csv) 데이터셋에 넣지 않는다
                            string g = SagArm_GuardWhy(arm);
                            bool bCut = g.Length > 0;
                            foreach (CFloorContact c in cs) if (c != null && c.UserStop) bCut = true;
                            string raw = SagRun_TraceTakeRaw();
                            if (raw.Length > 0)
                            {
                                if (bCut) { if (cutPath != null) SagRun_TraceAppend(cutPath, raw, ref bHdrCut); }
                                else if (tracePath != null) SagRun_TraceAppend(tracePath, raw, ref bHdr);
                            }
                            if (bCut)
                            {
                                if (g.Length > 0) { res.Why = SagRun_WhyJoin(res.Why, g + " — 측정 멈춤"); L("★" + g + " — 측정 멈춤 (이 행은 재지 않은 것으로)"); }
                                else { res.Stopped = true; L(string.Format(ci, "중단 요청 — r={0:0.#} 하강 중에 멈춤: 이 행은 재지 않은 것으로 (궤적은 floor_trace_stopped.csv, 데이터셋에 넣지 않음), 원위치", row.R)); }
                                break;
                            }
                            if (cs.Count == 0)
                            {
                                // 누르지 않고 돌아옴 — 중단 요청(시작점·자리 잡기에서) 또는 시작점으로 못 감 (관절 합 IK·경로 확인 — 움직이기 전에 거절, 팔은 그 자리)
                                if (SagRun_Stopped(stop)) { res.Stopped = true; L("중단 요청 — 측정 멈춤, 원위치"); break; }
                                res.Why = SagRun_WhyJoin(res.Why, string.Format(ci, "시작점으로 못 감 ({0} r{1:0.#})", row.LineKey, row.R));
                                L(string.Format(ci, "★{0} r={1:0.#}: 시작점으로 못 감 — 이 행은 재지 않고 다음 행으로", row.LineKey, row.R));
                                continue;
                            }
                        }
                        else
                        {
                            CFloorContact dc = SagRun_DryPress(arm, p, collect, row.PredZ, L);
                            if (dc == null) { res.Why = SagRun_WhyJoin("3D 작업 멈춤", arm.LastFail); break; }   // 모션 싱크가 바뀜
                            cs = new List<CFloorContact> { dc };
                        }
                        arm.CurTransferZ = row.TransferZ; arm.CurZLow = row.ZStart + 10;
                        res.RowsDone++;
                        foreach (CFloorContact c in cs)
                        {
                            if (c == null) continue;
                            if (c.Detected) res.LiveDetected++;
                            if (c.Safety) res.SafetyStops++;
                            if (SagRun_NoTouch(c)) res.NoTouchAtLimit++;
                            if (livePath != null) File.AppendAllText(livePath, SagRun_ContactCsvRow(row.LineKey, arm.IsDry ? "3d" : "measure", 1, row.Pitch, row.Yaw, row.ZStart, row.ZMin, c), new UTF8Encoding(false));
                        }
                        double left = SagRun_EstimateSec(rows.GetRange(k + 1, rows.Count - k - 1), p0, arm.Cfg) / Math.Max(0.1, arm.Speed);
                        L(string.Format(ci, "진행 {0}/{1} — 접촉 {2}, 안전 정지 {3}, 남은 시간 약 {4:F0}분", k + 1, rows.Count, res.LiveDetected, res.SafetyStops, left / 60.0));
                    }
                    if (!res.Stopped && SagRun_Stopped(stop)) { res.Stopped = true; L("중단 요청 — 측정 멈춤, 원위치"); }   // 마지막 행 뒤(복귀 중)에 온 중단 요청도 알린다
                }
                finally
                {
                    // 궤적 재분석 → 데이터셋 (멈춘 회차도 덧붙인다 — 점마다 독립 자료)
                    if (!arm.IsDry)
                    {
                        try { SagRun_MeasureData(rows, cfg, res, L, tStart, tracePath, string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, "floor_dataset_run.csv")); }
                        catch (Exception ex) { L("★데이터셋 정리 오류: " + SagRun_Msg(ex)); res.Why = SagRun_WhyJoin(res.Why, "★데이터셋 정리 오류: " + SagRun_Msg(ex)); }
                    }
                }
            }
            // 측정 끝 — 궤적 재분석(첫닿음 켬) → 이번 회차 데이터셋 + 누적 데이터셋에 덧붙임, kind 별 수, 첫닿음 FK − 계획 예측
            private static void SagRun_MeasureData(List<CFloorPlanRow> rows, CSagMeasureCfg cfg, CSagRunResult res, CSim2Real.DLog L, DateTime tStart, string tracePath, string runDsPath)
            {
                var ci = CultureInfo.InvariantCulture;
                if (string.IsNullOrEmpty(tracePath) || !File.Exists(tracePath)) { L("궤적 없음 — 데이터셋에 덧붙일 것 없음"); return; }
                string ds = cfg.DatasetPath;
                string session = SagRun_NewSession(ds, tStart);
                List<CFloorContact> all; string stats;
                string csv = SagRun_ExtractTrace(File.ReadAllText(tracePath), session, true, out all, out stats);
                res.Contacts = all; res.Session = session;
                foreach (CFloorContact c in all)
                {
                    if (!c.Detected || c.JointAct == null || c.JointAct.Length < 4) continue;
                    res.Detected++;
                    if (c.Kind == "F") res.NKindF++; else if (c.Kind == "H") res.NKindH++; else if (c.Kind == "C") res.NKindC++; else if (c.Kind == "X") res.NKindX++;
                }
                if (!string.IsNullOrEmpty(runDsPath)) { SagRun_WriteText(runDsPath, csv); res.RunDatasetPath = runDsPath; }
                int nAdd = string.IsNullOrEmpty(ds) ? -1 : SagRun_AppendDataset(ds, csv);
                res.DatasetRowsAdded = Math.Max(0, nAdd); res.DatasetPath = ds;
                if (nAdd < 0) { L("★데이터셋에 덧붙이지 못함: " + ds); res.Why = SagRun_WhyJoin(res.Why, "★데이터셋에 덧붙이지 못함"); }
                // 첫닿음 FK − 계획 예측 (미측정 구간 예측의 실측 확인) — 같은 자리(x·y 2mm, 피치 1°) 행의 예측. 하강마다 점별 표에도 (kind F 만 차를 낸다)
                double se = 0, mx = 0; int n = 0;
                res.PredErrRows = new List<CSagPredErrRow>();
                foreach (CFloorContact c in all)
                {
                    if (c == null) continue;
                    CFloorPlanRow hit = null;
                    foreach (CFloorPlanRow r in rows)
                    {
                        if (float.IsNaN(r.PredZ) || Math.Abs(r.X - c.X) > 2 || Math.Abs(r.Y - c.Y) > 2 || Math.Abs(r.Pitch - c.Pitch) > 1) continue;
                        hit = r; break;
                    }
                    var pe = new CSagPredErrRow { LineKey = hit != null ? hit.LineKey : "", R = c.R, X = c.X, Y = c.Y, Pitch = c.Pitch, Kind = c.Kind ?? "", FcZ = c.FcZ,
                                                  PredZ = hit != null ? hit.PredZ : float.NaN };
                    if (hit != null && c.Kind == "F" && !float.IsNaN(c.FcZ) && !float.IsInfinity(c.FcZ))
                    {
                        pe.Err = c.FcZ - hit.PredZ;
                        double e = pe.Err; se += e; mx = Math.Max(mx, Math.Abs(e)); n++;
                    }
                    res.PredErrRows.Add(pe);
                }
                res.PredErrN = n;
                if (n > 0) { res.PredErrMean = se / n; res.PredErrMax = mx; }
                L(string.Format(ci, "궤적 재분석: {0} → 데이터셋 +{1}행 (회차 {2}, {3})", stats, res.DatasetRowsAdded, session, Path.GetFileName(ds ?? "")));
            }
            // 측정 보고서·한 줄 (작업 폴더 measure_report.txt)
            private static void SagRun_MeasureReport(CSagRunResult res, CSagMeasureCfg cfg, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                sb.AppendLine(res.Dry ? "=== 3D 측정 미리보기 ===" : "=== 바닥 측정 결과 ===");
                sb.AppendLine(string.Format(ci, "계획 {0}행 → 누른 점 {1} (실시간 접촉 {2}, 안전 정지 {3}){4}, 걸린 시간 {5:F1}분",
                    res.RowsPlanned, res.RowsDone, res.LiveDetected, res.SafetyStops, res.Stopped ? " — 중단" : "", res.ElapsedSec / 60.0));
                if (cfg != null) sb.AppendLine("손목 수직 유지: " + (cfg.WristHold ? "켬" : "끔") + ", 관절 모델: " + (cfg.CollectModelPath ?? ""));
                if (!res.Dry)
                {
                    sb.AppendLine(string.Format(ci, "궤적 재분석: 접촉 {0} — 첫닿음 F {1}, 단단한 정지 H {2}, 검열 C {3}, 못 씀 X {4}", res.Detected, res.NKindF, res.NKindH, res.NKindC, res.NKindX));
                    sb.AppendLine(string.Format(ci, "데이터셋: +{0}행 (회차 {1}) — {2}", res.DatasetRowsAdded, res.Session ?? "", res.DatasetPath ?? ""));
                    sb.AppendLine(res.PredErrN > 0 ? string.Format(ci, "첫닿음 FK − 계획 예측: 평균 {0:+0.00;-0.00}mm, 최대 |{1:F2}|mm ({2}점) — 미측정 구간 예측의 실측 확인", res.PredErrMean, res.PredErrMax, res.PredErrN)
                                                   : "첫닿음 FK − 계획 예측: 비교한 점 없음");
                    // 한도까지 닿지 않은 누름 (10-03 검토: 모든 누름이 바닥을 못 찾아도 보고에 드러나지 않았다 — Ok 는 그대로, ★ 로 알린다)
                    bool bNoneTouched = res.RowsDone > 0 && res.NoTouchAtLimit >= res.RowsDone;
                    sb.AppendLine(string.Format(ci, "한도까지 내려가도 닿지 않은 누름: {0} / 누른 점 {1}{2}", res.NoTouchAtLimit, res.RowsDone,
                        bNoneTouched ? " — ★누른 점 모두 닿지 않음: 바닥 높이·계획 창(한도)·첫닿음 모델 확인 (데이터셋에 넣을 첫닿음이 없다)" : ""));
                    if (res.PredErrRows != null && res.PredErrRows.Count > 0)
                    {
                        sb.AppendLine("점별 첫닿음 FK − 계획 예측 (하강 순서, 차는 kind F 만):");
                        sb.AppendLine("  줄              r   kind   첫닿음 FK    예측       차");
                        Func<float, string, string> V = (f, fmt) => (float.IsNaN(f) || float.IsInfinity(f)) ? "—" : f.ToString(fmt, ci);
                        foreach (CSagPredErrRow pe in res.PredErrRows)
                            sb.AppendLine(string.Format(ci, "  {0,-12} {1,5:F0}   {2,-4} {3,10} {4,7} {5,8}", pe.LineKey.Length > 0 ? pe.LineKey : "(계획 밖)", pe.R,
                                pe.Kind.Length > 0 ? pe.Kind : "—", V(pe.FcZ, "F2"), V(pe.PredZ, "F2"), V(pe.Err, "+0.00;-0.00")));
                    }
                }
                if (res.UncheckedAxes != null && res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                if (!string.IsNullOrEmpty(res.MotorMapWarn)) sb.AppendLine(res.MotorMapWarn);
                sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                if (res.Why.Length > 0) sb.AppendLine("★" + res.Why);
                sb.AppendLine("파일: " + (res.TracePath ?? "") + " · " + (res.LivePath ?? "") + " · " + (res.RunDatasetPath ?? ""));
                res.Report = sb.ToString();
                string strNoTouch = "";
                if (!res.Dry && res.NoTouchAtLimit > 0)
                    strNoTouch = (res.NoTouchAtLimit >= res.RowsDone && res.RowsDone > 0)
                        ? string.Format(ci, " · ★닿은 누름 없음 ({0}점 모두 한도까지)", res.RowsDone)
                        : string.Format(ci, " · 한도까지 닿지 않음 {0}", res.NoTouchAtLimit);
                res.Line = res.Dry ? string.Format(ci, "3D 측정 미리보기 · {0}/{1}점{2}", res.RowsDone, res.RowsPlanned, res.Stopped ? " · 중단" : "")
                                   : string.Format(ci, "측정 {0}/{1}점 · 첫닿음 {2} · 데이터셋 +{3}행{4}{5}{6}", res.RowsDone, res.RowsPlanned, res.NKindF, res.DatasetRowsAdded,
                                         res.PredErrN > 0 ? string.Format(ci, " · 예측 오차 평균 {0:+0.0;-0.0} 최대 {1:F1}mm", res.PredErrMean, res.PredErrMax) : "",
                                         strNoTouch, res.Stopped ? " · 중단" : "");
                if (res.Why.Length > 0 && !res.Stopped) res.Line += " · ★" + res.Why;
                if (!string.IsNullOrEmpty(res.RunDir)) { res.ReportPath = Path.Combine(res.RunDir, "measure_report.txt"); SagRun_WriteText(res.ReportPath, res.Report); }
                if (L != null) L(res.Line);
            }

            /// <summary>3D 만 누름 — SagComp_FloorPress 와 같은 순서·높이를 Speed 배로: 결과 모델로 바꿔(쌍둥이 그림자 출력 단이 "보냈을 명령" 을 만든다)
            /// 시작점 +8 → 시작점 → 예상 높이 − 2 (또는 한도) 까지 하강 → 누름 유지 → 들어 올림 → 복귀 (max(시작 + 5, 예상 + 복귀)). 끝에 이전 모델로.
            /// 반환 = { CmdZ = 예상 높이, FkZ = NaN, Detected = 예상이 있음, Note = "3D 예상" } — 작업 중에 모션 싱크가 바뀌었으면 움직이지 않고 null (SagArm_SinkOk)</summary>
            private CFloorContact SagRun_DryPress(CSagArm arm, CFloorProbeCfg p, CSagComp model, float fPredZ, CSim2Real.DLog L)
            {
                float r = (p.Radii != null && p.Radii.Length > 0) ? p.Radii[0] : 0;
                double a = p.YawDeg * Math.PI / 180.0;
                float x = (float)(r * Math.Cos(a)), y = (float)(r * Math.Sin(a));
                var c = new CFloorContact { R = r, X = x, Y = y, Pitch = p.RyDeg, CmdZ = fPredZ, CmdZEff = fPredZ, FkZ = float.NaN, Detected = !float.IsNaN(fPredZ), Note = "3D 예상" };
                CSagComp prev = m_SagComp;
                float sp = Math.Max(0.1f, arm.Speed);
                try
                {
                    // 움직이기 전마다 싱크 확인 — 3D 만 작업 중에 싱크가 바뀌었으면(그사이 연결 등) 3D 를 움직이지 않는다 (null).
                    //   움직임은 실물 누름과 같은 관절 합을 정한 IK 끝 자세 관절 보간 (SagComp_PressMove — 10-03)
                    if (!SagArm_SinkOk(arm, L)) return null;
                    if (model != null) { model.Enabled = true; SagComp_Switch(model); }
                    string w;
                    int n = SagComp_PressMove(null, p, x, y, p.ZStart + 8, SagRun_DryMs(p.MoveMs, sp), 0, false, false, null, out w);
                    if (!SagArm_SinkOk(arm, L)) return null;
                    if (n == 0) n = SagComp_PressMove(null, p, x, y, p.ZStart, SagRun_DryMs(1500, sp), 0, true, false, null, out w);
                    if (n != 0) { c.Detected = false; c.Note = "3D: 시작점으로 못 감 (" + w + ")"; L("  ★" + c.Note); return c; }
                    float zStop = float.IsNaN(fPredZ) ? p.ZMin : Math.Max(p.ZMin, fPredZ - 2);
                    if (!SagArm_SinkOk(arm, L)) return null;
                    bool bDown = true;
                    if (zStop < p.ZStart - 0.1f && SagComp_PressMove(null, p, x, y, zStop, SagRun_DryMs((p.ZStart - zStop) / Math.Max(0.5, p.PressDescentMmPerSec) * 1000.0, sp), 0, true, true, null, out w) != 0)
                    { bDown = false; c.Note = "3D: 하강 못 함 (" + w + ")"; L("  ★" + c.Note); }
                    Thread.Sleep(Math.Max(0, (int)(p.PressHoldMs / sp)));
                    if (!SagArm_SinkOk(arm, L)) return null;
                    if (bDown) SagComp_PressMove(null, p, x, y, zStop + p.LiftMm, SagRun_DryMs(p.LiftMm / Math.Max(0.5, p.LiftMmPerSec) * 1000.0, sp), 0, true, true, null, out w);
                    float zBack = float.IsNaN(fPredZ) ? p.ZStart + 5 : Math.Max(p.ZStart + 5, fPredZ + p.RetractAboveKneeMm);
                    if (!SagArm_SinkOk(arm, L)) return null;
                    if (SagComp_PressMove(null, p, x, y, zBack, SagRun_DryMs(p.RetractMs, sp), 0, true, false, null, out w) != 0) L("  ★3D: 복귀 경로 안 됨 (" + w + ")");
                    if (!SagArm_SinkOk(arm, L)) return null;
                }
                finally { SagComp_Switch(prev); }
                return c;
            }
            private static int SagRun_DryMs(double dMs, float fSpeed) { return Math.Max(300, (int)(dMs / Math.Max(0.1f, fSpeed))); }

            /// <summary>닿기 시험 작업 (블로킹 — 작업 스레드) — 결과 파일을 보정 끔(관절 모델만)·켬(첫닿음 모델 포함)으로 번갈아 눌러 본다. 잠금 소유자 "닿기 시험".
            /// 점·창은 SagRun_TestPoints (첫닿음 예측 pred·Ref 를 모두 담는 창) — 쓸 점이 없으면 포트를 건드리기 전에 실패.
            /// 작업 폴더 …_test[_3d] (run.log, test.csv, test_trace.csv, test_report.txt), 결과 파일 옆 <결과>_test.txt 에 "[닿기] …" 한 줄 (중단이면 판정=중단).
            /// 판정은 누름마다 궤적을 첫닿음으로 다시 읽은 값 (SagRun_Test 설명). 실물은 이 로봇·정의·모터 영점으로 수직 눈 확인을 했어야 한다 (SagPrac_EyeNeeded — 아니면 움직이기 전에 거절: SAGPRAC_EYE_FIRST). 예외 없음.</summary>
            public CSagTestResult SagRun_TestJob(CArmConn_t conn, CSagArmCfg armCfg, string strResultPath, CSagTestCfg cfg, string strUrdfDir, bool bDry,
                                                 CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_TestJobBody(conn, armCfg, strResultPath, cfg, strUrdfDir, bDry, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagTestResult SagRun_TestJobBody(CArmConn_t conn, CSagArmCfg armCfg, string strResultPath, CSagTestCfg cfg, string strUrdfDir, bool bDry,
                                                      CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagTestResult { Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                bool bRan = false;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    if (cfg == null) cfg = new CSagTestCfg();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagRun_TestRefuse(res, why, L);
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, out why)) return SagRun_TestRefuse(res, why, L);   // 실물 — 이 로봇·정의·모터 영점으로 수직 눈 확인을 했나 (움직이기 전)
                    if (string.IsNullOrEmpty(strResultPath) || !File.Exists(strResultPath)) return SagRun_TestRefuse(res, "결과 파일 없음: " + strResultPath, L);
                    CSagComp mOn = CSagComp.Load(strResultPath);
                    if (mOn == null || !mOn.HasJoint || !mOn.HasFc) return SagRun_TestRefuse(res, "결과 파일에 관절 모델·첫닿음 모델이 없음 — ④ 학습 결과 파일을 고르십시오", L);
                    string rep;
                    List<CSagTestPoint> pts = SagRun_TestPoints(mOn, cfg, armCfg, out rep);
                    CSagTestPoint first = null;
                    foreach (CSagTestPoint p in pts) if (!p.Skipped) { first = p; break; }
                    if (first == null) return SagRun_TestRefuse(res, "시험할 점 없음 — " + rep.Replace("\r\n", " / "), L);
                    DateTime t0 = DateTime.Now;
                    string runDir = SagRun_NewRunDir(strUrdfDir, t0, bDry ? "test_3d" : "test");
                    res.RunDir = runDir;
                    L = SagRun_SafeLog(log, Path.Combine(runDir, "run.log"));
                    L((bDry ? "3D 닿기 시험" : "닿기 시험") + " 시작 — " + Path.GetFileName(strResultPath));
                    L(rep.Trim());
                    res.UncheckedAxes = SagArm_UncheckedAxes(conn, armCfg);
                    if (res.UncheckedAxes.Length > 0) L("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes) + " — 계속 (확인 표를 채우십시오)");
                    res.MotorMapWarn = SagArm_MotorMapWarn(bDry ? null : conn, mOn);
                    if (res.MotorMapWarn.Length > 0) L(res.MotorMapWarn);
                    string whyM;
                    CSagComp mMove = SagRun_LoadJointOnly(strResultPath, out whyM);         // 결과 파일의 관절 모델만 (= 끔 모델) — 옮기는 동안의 보정 (10-03)
                    if (mMove == null) return SagRun_TestRefuse(res, whyM, L);
                    arm = SagArm_Open(conn, armCfg, runDir, "닿기 시험", bDry, L, out why);
                    if (arm == null) return SagRun_TestRefuse(res, why, L);
                    SagArm_SetMoveModel(arm, mMove, L);
                    bRan = true;
                    if (!SagArm_Unfold(arm, first.X, first.Y, first.TransferZ, L, stop))
                    {
                        res.Points = pts; res.Stopped = SagRun_Stopped(stop);
                        res.Why = res.Stopped ? "" : SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else SagRun_TestCore(arm, strResultPath, cfg, pts, false, res, L, stop);
                }
                catch (Exception ex) { res.Ok = false; res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally
                {
                    if (arm != null) res.RestOk = SagArm_Close(arm, L);
                }
                if (bRan) SagRun_TestFinish(res, strResultPath, cfg, L, true);
                return res;
            }

            /// <summary>닿기 시험 본체 (블로킹, 작업 스레드) — 열린 손잡이로 (펼치기·원위치는 부른 쪽). mOn = 결과 파일 (보정·바닥·첫닿음 켬),
            /// mOff = 같은 파일의 관절 모델만 (바닥 끔 — 판정의 flooron=0). pts 가 null 이면 SagRun_TestPoints. 시험 동안 쌍둥이 = 결과 모델 (끝에 되돌림).
            /// 점(Skipped 아님)마다: 그 점의 옮기는 높이로 안전 이동 → 반복마다 끔·켬 순으로 SagComp_FloorPress (시작 = 끔·켬 같은 ZStart, 한도 = 그 모드의 ZMinFor)
            /// — 3D 만이면 SagRun_DryPress (끔 = 첫닿음 예측, 켬 = Ref). 누름마다 그 궤적 조각을 첫닿음으로 다시 읽어(SagComp_FloorContactsFromTrace FirstTouch)
            /// 접촉에 kind·첫닿음 FK·명령 높이를 붙인다 — 판정은 그 값으로 (SagRun_TestEvaluate). 하강 중 중단 요청으로 끊긴 누름·한 틱 관절 변화 막이가 걸린 누름은
            /// 판정에 넣지 않고 멈춘다 (중단이면 Stopped → 판정=중단, 궤적은 test_trace_stopped.csv). 누름 뒤·끝에도 중단 요청을 본다.
            /// 받은 점(pts)은 다시 확인한다 (10-03 검토 — 작업은 SagRun_TestPoints 로 만든 점을 쓴다): 점마다 결과 모델로 첫닿음 예측을 다시 내 창 시작 ≥ max(예측, 점의 PredFc, Ref)
            /// + ABOVE_MIN, 자리 (X, Y) = 반경·요, SagRun_CheckPlan 과 같은 옮기는 높이·IK (TransferZ 도 다시) — 안 되면 그 점은 Skipped (Note).
            /// 끝에 판정·보고서, 결과 파일 옆 기록 줄은 시험이 실제로 시작했을 때만 (손잡이가 열려 있고 결과 파일이 관절·첫닿음 모델이고 쓸 점이 있을 때 —
            /// 손잡이 없음·모델 아닌 파일이면 기록 줄도 <파일>_test.txt 도 만들지 않는다). 예외 없음.</summary>
            public CSagTestResult SagRun_Test(CSagArm arm, string strResultPath, CSagTestCfg cfg, List<CSagTestPoint> pts, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_TestBody(arm, strResultPath, cfg, pts, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagTestResult SagRun_TestBody(CSagArm arm, string strResultPath, CSagTestCfg cfg, List<CSagTestPoint> pts, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagTestResult();
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                bool bStarted = false;
                try
                {
                    if (arm != null) res.Dry = arm.IsDry;
                    bStarted = SagRun_TestCore(arm, strResultPath, cfg, pts, true, res, L, stop);
                }
                catch (Exception ex) { res.Ok = false; res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                SagRun_TestFinish(res, strResultPath, cfg, L, bStarted);
                return res;
            }
            // 닿기 시험 본체 — 반환 = 시험을 시작했다 (손잡이·결과 모델·쓸 점이 있어 누름 차례에 들어감 — 기록 줄을 남길지). bRecheck = 받은 pts 를 다시 확인 (SagRun_Test)
            private bool SagRun_TestCore(CSagArm arm, string strResultPath, CSagTestCfg cfg, List<CSagTestPoint> pts, bool bRecheck, CSagTestResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop)
            {
                var ci = CultureInfo.InvariantCulture;
                if (cfg == null) cfg = new CSagTestCfg();
                if (arm == null || arm.Closed) { res.Why = "작업 손잡이 없음 — SagArm_Open 먼저"; L("★" + res.Why); return false; }
                res.Dry = arm.IsDry; if (res.RunDir == null) res.RunDir = arm.RunDir;
                CSagComp mOn = SagRun_LoadQuiet(strResultPath), mOff = SagRun_LoadQuiet(strResultPath);
                if (mOn == null || !mOn.HasJoint || !mOn.HasFc || mOff == null) { res.Why = "결과 파일에 관절 모델·첫닿음 모델이 없음 — 끔/켬 차이가 없다"; L("★" + res.Why); return false; }
                mOn.Enabled = true; mOn.FloorMapEnabled = true; mOn.Fc.Enabled = true;
                mOff.Enabled = true; mOff.FloorMapEnabled = false;
                if (pts == null) { string rep; pts = SagRun_TestPoints(mOn, cfg, arm.Cfg, out rep); L(rep.Trim()); }
                else if (bRecheck) { string repC; SagRun_TestPointsRecheck(mOn, pts, cfg, arm.Cfg, out repC); L(repC.Trim()); }
                res.Points = pts; res.Ref = mOn.Fc.Ref;
                var use = new List<CSagTestPoint>();
                foreach (CSagTestPoint p in pts) if (p != null && !p.Skipped) use.Add(p);
                if (use.Count == 0) { res.Why = "시험할 점 없음"; L("★" + res.Why); return false; }
                string dir = arm.RunDir;
                res.CsvPath = string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, "test.csv");
                res.TracePath = (arm.IsDry || string.IsNullOrEmpty(dir)) ? null : Path.Combine(dir, "test_trace.csv");
                string cutPath = (arm.IsDry || string.IsNullOrEmpty(dir)) ? null : Path.Combine(dir, "test_trace_stopped.csv");   // 끝까지 누르지 않은 하강 (판정에서 뺀다)
                if (res.CsvPath != null) SagRun_WriteText(res.CsvPath, SagRun_ContactCsvHeader());
                CSagComp twinPrev = SagTwin_GetModel();
                SagTwin_SetModel(mOn);                                       // 시험 내내 쌍둥이 = 결과 모델 예측 (끔에서는 처지고, 켬에서는 ≈ 목표)
                int nRep = Math.Max(1, cfg.Repeats), nTotal = use.Count * nRep * 2, nDone = 0;
                bool bHdr = false, bHdrCut = false, bEnd = false;
                try
                {
                    if (!arm.IsDry) SagComp_ClearFloorTrace();
                    foreach (CSagTestPoint p in use)
                    {
                        if (SagRun_Stopped(stop)) { res.Stopped = true; break; }
                        float tz = float.IsNaN(p.TransferZ) ? arm.Cfg.SafeZ : p.TransferZ;
                        L(string.Format(ci, "── 점 r={0:0.#} 요 {1:0.#}: 옮기는 높이 z={2:0.#} ──", p.R, p.Yaw, tz));
                        if (!SagArm_MoveSafeCore(arm, p.X, p.Y, tz, p.Pitch, tz, p.ZStart + 10, L, stop))
                        {
                            if (arm.StopSeen) { res.Stopped = true; L("중단 요청 — 시험 멈춤 (옮기던 다리를 마친 자리에서), 원위치"); break; }
                            res.Why = SagRun_WhyJoin(string.Format(ci, "옮기기 실패 (r{0:0.#}) — 시험 멈춤", p.R), arm.LastFail); break;
                        }
                        for (int k = 1; k <= nRep && !bEnd; k++)
                            for (int mode = 0; mode < 2 && !bEnd; mode++)
                            {
                                if (SagRun_Stopped(stop)) { res.Stopped = true; bEnd = true; break; }
                                bool bOn = mode == 1;
                                CFloorProbeCfg pc = (cfg.Probe != null ? cfg.Probe : new CSagTestCfg().Probe).Clone();
                                pc.FuncNumber = arm.Cfg.FuncNumber; pc.PosIDs = (int[])arm.Cfg.PosIDs.Clone(); pc.WristID = arm.Cfg.WristID; pc.DownSumDeg = arm.Cfg.DownSumDeg;
                                pc.Radii = new float[] { p.R }; pc.YawDeg = p.Yaw; pc.RyDeg = p.Pitch; pc.ZStart = p.ZStart; pc.ZMin = p.ZMinFor(bOn);
                                pc.WristHold = cfg.WristHold; pc.Repeats = 1; if (pc.Margin < 20) pc.Margin = 20;
                                List<CFloorContact> cs;
                                if (!arm.IsDry)
                                {
                                    string rep;
                                    cs = SagComp_FloorPress(arm.Scene, pc, bOn ? mOn : mOff, L, stop, out rep);
                                    string raw = SagRun_TraceTakeRaw();
                                    // 하강 중 중단 요청(UserStop)·한 틱 관절 변화 막이로 끊긴 누름은 판정에 넣지 않는다 (궤적은 따로)
                                    string g = SagArm_GuardWhy(arm);
                                    bool bCut = g.Length > 0;
                                    foreach (CFloorContact c in cs) if (c != null && c.UserStop) bCut = true;
                                    if (bCut)
                                    {
                                        if (cutPath != null && raw.Length > 0) SagRun_TraceAppend(cutPath, raw, ref bHdrCut);
                                        if (g.Length > 0) { res.Why = SagRun_WhyJoin(res.Why, g + " — 시험 멈춤"); L("★" + g + " — 시험 멈춤 (이 누름은 판정에서 뺌)"); }
                                        else { res.Stopped = true; L(string.Format(ci, "중단 요청 — r={0:0.#} {1} 하강 중에 멈춤: 이 누름은 판정에서 뺌 (궤적은 test_trace_stopped.csv), 원위치", p.R, bOn ? "켬" : "끔")); }
                                        bEnd = true; break;
                                    }
                                    if (cs.Count == 0)
                                    {
                                        // 누르지 않고 돌아옴 — 중단 요청 또는 시작점으로 못 감 (움직이기 전에 거절, 팔은 그 자리) — 이 누름은 판정에 없다
                                        if (SagRun_Stopped(stop)) { res.Stopped = true; bEnd = true; break; }
                                        res.Why = SagRun_WhyJoin(res.Why, string.Format(ci, "시작점으로 못 감 (r{0:0.#} {1})", p.R, bOn ? "켬" : "끔"));
                                        L(string.Format(ci, "★r={0:0.#} {1}: 시작점으로 못 감 — 이 누름은 빼고 다음으로", p.R, bOn ? "켬" : "끔"));
                                        continue;
                                    }
                                    if (res.TracePath != null && raw.Length > 0) SagRun_TraceAppend(res.TracePath, raw, ref bHdr);
                                    SagRun_AttachFirstTouch(cs, raw, pc);              // 판정은 첫닿음으로 (kind F·H 의 FK·명령 높이 — 실시간 꺾임점이 아니다)
                                }
                                else
                                {
                                    CFloorContact dc = SagRun_DryPress(arm, pc, bOn ? mOn : mOff, bOn ? p.Ref : p.PredFc, L);
                                    if (dc == null) { res.Why = SagRun_WhyJoin("3D 작업 멈춤", arm.LastFail); bEnd = true; break; }   // 모션 싱크가 바뀜
                                    cs = new List<CFloorContact> { dc };
                                }
                                arm.CurTransferZ = p.TransferZ; arm.CurZLow = p.ZStart + 10;
                                nDone++;
                                CFloorContact c0 = null;
                                foreach (CFloorContact c in cs)
                                {
                                    if (c == null) continue;
                                    if (c0 == null) c0 = c;
                                    (bOn ? p.On : p.Off).Add(c);
                                    if (res.CsvPath != null) File.AppendAllText(res.CsvPath, SagRun_ContactCsvRow(string.Format(ci, "r{0:0.#}y{1:0.#}", p.R, p.Yaw), bOn ? "on" : "off", k, p.Pitch, p.Yaw, p.ZStart, pc.ZMin, c), new UTF8Encoding(false));
                                }
                                if (c0 != null && c0.FromPress)
                                    L(string.Format(ci, "진행 {0}/{1} — r={2:0.#} {3} #{4}: 첫닿음 {5} — 명령 {6} (FK {7}), 꺾임점 명령 {8}", nDone, nTotal, p.R, bOn ? "켬" : "끔", k,
                                        string.IsNullOrEmpty(c0.Kind) ? "판정 없음" : c0.Kind, float.IsNaN(c0.FcCmdZ) ? "—" : c0.FcCmdZ.ToString("F1", ci), float.IsNaN(c0.FcZ) ? "—" : c0.FcZ.ToString("F1", ci),
                                        float.IsNaN(c0.CmdZ) || !c0.Detected ? "—" : c0.CmdZ.ToString("F1", ci)));
                                else
                                    L(string.Format(ci, "진행 {0}/{1} — r={2:0.#} {3} #{4}: 명령 {5} (FK {6})", nDone, nTotal, p.R, bOn ? "켬" : "끔", k,
                                        c0 == null || float.IsNaN(c0.CmdZ) ? "—" : c0.CmdZ.ToString("F1", ci), c0 == null || float.IsNaN(c0.FkZ) ? "—" : c0.FkZ.ToString("F1", ci)));
                            }
                        if (bEnd) break;
                    }
                    if (!res.Stopped && SagRun_Stopped(stop)) res.Stopped = true;   // 마지막 누름 뒤(복귀 중)에 온 중단 요청도 알린다
                }
                finally { SagTwin_SetModel(twinPrev); }
                return true;
            }
            // 받은 시험 점 다시 확인 (SagRun_Test — 밖에서 만들었거나 고친 점): 점마다 결과 모델 mOn 으로 첫닿음 예측을 다시 내 (SagRun_FcPlanSeeded)
            //   창 시작 ≥ max(다시 낸 예측, 점의 PredFc, 모델·점의 Ref) + ABOVE_MIN (창 시작까지는 누름 감시 없이 그냥 내려간다), 자리 (X, Y) 가 반경·요와 0.5mm 안
            //   (누름은 반경·요로, 옮기기는 X·Y 로 간다), 하강 한도는 모드마다 (10-03 검토): 끔 한도 ≥ max(하한, max(다시 낸 예측, PredFc) − 아래) − 1mm,
            //   켬 한도 ≥ max(하한, max(모델·점의 Ref) − 아래) − 1mm (아래·하한 = 시험 설정), 그다음 SagRun_CheckPlan 과 같은 옮기는 높이·IK 확인
            //   (TransferZ 도 그 값으로 바꾼다).
            //   안 되는 점은 Skipped + Note ("… — 시험 제외"). 반환 = 쓸 점 수. 복제 기구학이 없으면 모두 제외
            private int SagRun_TestPointsRecheck(CSagComp mOn, List<CSagTestPoint> pts, CSagTestCfg cfg, CSagArmCfg arm, out string report)
            {
                report = "";
                var ci = CultureInfo.InvariantCulture;
                if (pts == null) return 0;
                if (cfg == null) cfg = new CSagTestCfg();
                string wdR = "";
                C3d kin = (arm != null && arm.IdsOk && SagRun_SameDef(mOn, arm, out wdR)) ? SagRun_Kin() : null;
                var rows = new List<CFloorPlanRow>(); var map = new List<CSagTestPoint>();
                int nOut = 0;
                foreach (CSagTestPoint p in pts)
                {
                    if (p == null || p.Skipped) continue;
                    string why = "";
                    double pred = double.NaN, std; float qs;
                    float xr = (float)(p.R * Math.Cos(p.Yaw * Math.PI / 180.0)), yr = (float)(p.R * Math.Sin(p.Yaw * Math.PI / 180.0));
                    if (kin == null) why = (arm != null && arm.IdsOk && wdR.Length == 0) ? SAGRUN_NO_KIN : "모델과 설정의 로봇 정의가 다름 — " + (wdR.Length > 0 ? wdR : "관절 구성(func·posids·wrist)");
                    else if (float.IsNaN(p.ZStart) || float.IsInfinity(p.ZStart) || float.IsNaN(p.ZMinFor(true)) || float.IsNaN(p.ZMinFor(false)) || p.ZMinFor(true) > p.ZStart || p.ZMinFor(false) > p.ZStart)
                        why = "창 없음";
                    else if (!(Math.Abs(p.X - xr) <= 0.5f && Math.Abs(p.Y - yr) <= 0.5f))
                        why = string.Format(ci, "자리 ({0:F1}, {1:F1}) 가 반경 {2:0.#}·요 {3:0.#} 의 자리 ({4:F1}, {5:F1}) 와 다름", p.X, p.Y, p.R, p.Yaw, xr, yr);
                    else if (!SagRun_FcPlanSeeded(kin, mOn, arm, p.Yaw, xr, yr, p.Pitch, out pred, out std, out qs, out why)) { }
                    else
                    {
                        double top = Math.Max(pred, mOn.Fc.Ref);
                        if (!float.IsNaN(p.PredFc) && !float.IsInfinity(p.PredFc)) top = Math.Max(top, p.PredFc);
                        if (!float.IsNaN(p.Ref) && !float.IsInfinity(p.Ref)) top = Math.Max(top, p.Ref);
                        // 하강 한도 (모드마다) — 끔 바닥 예상 = 다시 낸 예측과 점의 PredFc 중 높은 쪽, 켬 = 모델·점의 Ref 중 높은 쪽
                        double flOff = pred, flOn = mOn.Fc.Ref;
                        if (!float.IsNaN(p.PredFc) && !float.IsInfinity(p.PredFc)) flOff = Math.Max(flOff, p.PredFc);
                        if (!float.IsNaN(p.Ref) && !float.IsInfinity(p.Ref)) flOn = Math.Max(flOn, p.Ref);
                        double zOffMin = Math.Max(cfg.ZMinFloor, flOff - cfg.BelowMm), zOnMin = Math.Max(cfg.ZMinFloor, flOn - cfg.BelowMm);
                        if (!(p.ZStart >= top + CSagArmSetup.ABOVE_MIN - 1e-3))
                            why = string.Format(ci, "창 시작 z={0:F1} 이 예측 닿는 높이 {1:F1} + {2:0} 보다 낮음 (그냥 내려가다 닿는다)", p.ZStart, top, CSagArmSetup.ABOVE_MIN);
                        else if (p.ZMinFor(false) < zOffMin - SAGRUN_ZM_TOL_MM - 1e-3)
                            why = string.Format(ci, "끔 하강 한도 z={0:F1} 이 설계 한도 {1:F1} (끔 예측 {2:F1} − 아래 {3:0.#}, 하한 {4:0.#}) 보다 {5:0.#}mm 넘게 깊음 (계획보다 깊이 누른다)",
                                                p.ZMinFor(false), zOffMin, flOff, cfg.BelowMm, cfg.ZMinFloor, SAGRUN_ZM_TOL_MM);
                        else if (p.ZMinFor(true) < zOnMin - SAGRUN_ZM_TOL_MM - 1e-3)
                            why = string.Format(ci, "켬 하강 한도 z={0:F1} 이 설계 한도 {1:F1} (Ref {2:F1} − 아래 {3:0.#}, 하한 {4:0.#}) 보다 {5:0.#}mm 넘게 깊음 (계획보다 깊이 누른다)",
                                                p.ZMinFor(true), zOnMin, flOn, cfg.BelowMm, cfg.ZMinFloor, SAGRUN_ZM_TOL_MM);
                        else
                        {
                            rows.Add(new CFloorPlanRow { Set = "T", Pitch = p.Pitch, Yaw = p.Yaw, R = p.R, X = p.X, Y = p.Y, ZStart = p.ZStart,
                                                         ZMin = Math.Min(p.ZMinFor(true), p.ZMinFor(false)), PredZ = (float)top, LimitByCaller = true });
                            map.Add(p);
                            continue;
                        }
                    }
                    p.Skipped = true; p.Note = SagRun_WhyJoin(why, p.Note) + " — 시험 제외"; nOut++;
                }
                string repIk = "";
                if (rows.Count > 0) SagRun_CheckPlan(rows, arm, cfg.Probe != null ? cfg.Probe.RetractAboveKneeMm : 25, true, out repIk);
                int nUse = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    map[i].TransferZ = rows[i].TransferZ;
                    if (!rows[i].IkOk) { map[i].Skipped = true; map[i].Note = SagRun_WhyJoin(rows[i].Why, map[i].Note) + " — 시험 제외"; nOut++; }
                    else nUse++;
                }
                report = string.Format(ci, "받은 시험 점 다시 확인: 쓸 점 {0}, 뺀 점 {1}", nUse, nOut) + (repIk.Length > 0 ? "\r\n" + repIk : "");
                foreach (CSagTestPoint p in pts) if (p != null && p.Skipped && p.Note.EndsWith(" — 시험 제외")) report += "\r\n  제외 r" + p.R.ToString("0.#", ci) + ": " + p.Note;
                return nUse;
            }
            // 누름 하나의 궤적 조각(머리줄 포함)을 첫닿음으로 다시 읽어 (SagComp_FloorContactsFromTrace, FirstTouch) kind·첫닿음 FK·명령 높이를
            //   그 누름의 실시간 접촉에 붙인다 — 하강(seg 1) 하나 = 접촉 하나 (수가 다르면 끝에서부터 맞춘다). 측정의 궤적 재분석과 같은 판정
            private static void SagRun_AttachFirstTouch(List<CFloorContact> cs, string strRaw, CFloorProbeCfg pc)
            {
                if (cs == null || cs.Count == 0 || string.IsNullOrEmpty(strRaw)) return;
                CFloorProbeCfg fcfg = (pc != null ? pc : new CFloorProbeCfg()).Clone();
                fcfg.FirstTouch = true;
                List<CFloorContact> ft = SagComp_FloorContactsFromTrace(strRaw, fcfg);
                int n = Math.Min(cs.Count, ft.Count);
                for (int i = 1; i <= n; i++)
                {
                    CFloorContact c = cs[cs.Count - i], f = ft[ft.Count - i];
                    if (c == null || f == null) continue;
                    c.Kind = f.Kind; c.FcStage = f.FcStage; c.FcZ = f.FcZ; c.FcCmdZ = f.FcCmdZ; c.FcJointCmd = f.FcJointCmd; c.FcJointAct = f.FcJointAct;
                    c.KneeHard = f.KneeHard; c.SafetyStop = f.SafetyStop; c.ArmedN = f.ArmedN; c.FcWhy = f.FcWhy;
                }
            }
            // 닿기 시험 끝 — 판정, 기록 줄 (<결과>_test.txt — bRecord 일 때만: 시험이 실제로 시작했다), 보고서 (작업 폴더 test_report.txt)
            private static void SagRun_TestFinish(CSagTestResult res, string strResultPath, CSagTestCfg cfg, CSim2Real.DLog L, bool bRecord)
            {
                try
                {
                    var ci = CultureInfo.InvariantCulture;
                    if (cfg == null) cfg = new CSagTestCfg();
                    string whyJob = res.Why;
                    SagRun_TestEvaluate(res, cfg);
                    if (whyJob.Length > 0 && !res.Stopped) { res.Why = SagRun_WhyJoin(whyJob, res.Why); if (res.Verdict == "합격") { res.Pass = false; res.Verdict = "판정 불가"; } }
                    res.Ok = whyJob.Length == 0 && !res.Stopped;
                    if (!string.IsNullOrEmpty(res.RunDir)) res.ReportPath = Path.Combine(res.RunDir, "test_report.txt");
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 3D 닿기 시험 (예상) ===" : "=== 닿기 시험 결과 ===");
                    sb.AppendLine("결과 파일: " + (strResultPath ?? ""));
                    sb.AppendLine(string.Format(ci, "Ref {0:F2}mm, 반복 {1}, 손목 유지 {2}", res.Ref, cfg.Repeats, cfg.WristHold ? "켬" : "끔"));
                    if (!res.Dry) sb.AppendLine(string.Format(ci, "판정 값 = 하강마다 궤적을 첫닿음으로 다시 읽은 값 (kind F·H 의 첫닿음 명령 높이·FK) — 첫닿음 못 찾은 하강 {0} 은 뺌", res.NNoFirstTouch));
                    sb.AppendLine("      r    요  끔 예측   끔 명령 평균  켬 명령 평균  끔 FK 평균  끔 예측 오차  비고");
                    if (res.Points != null)
                        foreach (CSagTestPoint p in res.Points)
                        {
                            if (p == null) continue;
                            Func<float, string> V = f => float.IsNaN(f) ? "—" : f.ToString("F2", ci);
                            sb.AppendLine(string.Format(ci, "  {0,5:F0} {1,5:F0}  {2,7}  {3,11}  {4,11}  {5,10}  {6,11}  {7}", p.R, p.Yaw, V(p.PredFc), V(p.MeanOff), V(p.MeanOn), V(p.MeanFkOff), V(p.PredErrOff), p.Skipped ? "제외 — " + p.Note : p.Note));
                        }
                    sb.AppendLine("판정: " + res.Verdict + (res.Why.Length > 0 ? " (" + res.Why + ")" : ""));
                    sb.AppendLine(res.Line);
                    if (res.UncheckedAxes != null && res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!string.IsNullOrEmpty(res.MotorMapWarn)) sb.AppendLine(res.MotorMapWarn);
                    sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    res.Report = sb.ToString();
                    if (res.ReportPath != null) SagRun_WriteText(res.ReportPath, res.Report);
                    if (bRecord && !string.IsNullOrEmpty(strResultPath)) res.SidecarPath = SagRun_AppendTestLine(strResultPath, SagRun_TestRecordLine(res, cfg));
                    if (L != null) L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★시험 정리 오류: " + SagRun_Msg(ex)); }
            }

            /// <summary>틈 재기 작업 (블로킹 — 작업 스레드) — 판정 §7-1: 펜을 바닥 위 Z 로 띄워 점마다 보정 끔·켬 틈을 사용자가 자로 재고 그 변화를 첫닿음 모델 예측(dz_pred)과 비교한다
            /// (닿지 않으므로 바닥 데이터와 독립). 잠금 소유자 "틈 재기". 점 = SagRun_GapPlan (IkOk 만 — 펜 높이가 예측 닿는 높이 + 10 아래인 점은 빠진다).
            /// 점마다: 옮기는 높이로 안전 이동 → 끔 모델(같은 결과 파일의 관절 모델만)로 바꾸고 Z 까지 수직 하강 → 자세를 보내며 붙잡아 보정 전환이 끝날 때까지
            /// (출력 틱이 흘러야 실물에 들어간다) → SettleMs → ask("… 보정 끔 …") → 켬 모델로 바꾸고 같은 붙잡기 → SettleMs → ask("… 보정 켬 …").
            /// ask: 작업 스레드에서 부른다 — 숫자 문자열(mm, -50~200), "" = 건너뜀, null(또는 예외) = 중단 (원위치). 3D 만이면 ask 를 부르지 않고 같은 동작을 3D 로 걷는다
            /// (판정 = 3D만, dz_pred 표만). 끝에 판정(SagRun_GapEvaluate)·"[틈] …" 기록 줄·보고서 (작업 폴더 gap_report.txt, gap.csv). 예외 없음.</summary>
            public CSagGapResult SagRun_GapJob(CArmConn_t conn, CSagArmCfg armCfg, string strResultPath, CSagGapCfg cfg, string strUrdfDir, bool bDry,
                                               DSagAsk ask, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagRun_GapJobBody(conn, armCfg, strResultPath, cfg, strUrdfDir, bDry, ask, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagGapResult SagRun_GapJobBody(CArmConn_t conn, CSagArmCfg armCfg, string strResultPath, CSagGapCfg cfg, string strUrdfDir, bool bDry,
                                                    DSagAsk ask, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagGapResult { Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                var ci = CultureInfo.InvariantCulture;
                CSagArm arm = null;
                bool bRan = false;
                try
                {
                    if (armCfg == null) armCfg = new CSagArmCfg();
                    if (cfg == null) cfg = new CSagGapCfg();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagRun_GapRefuse(res, why, L);
                    if (!bDry && ask == null) return SagRun_GapRefuse(res, "잰 값을 물을 곳(ask)이 없음 — 실물 틈 재기는 사용자가 잰 틈을 넣어야 한다", L);
                    if (string.IsNullOrEmpty(strResultPath) || !File.Exists(strResultPath)) return SagRun_GapRefuse(res, "결과 파일 없음: " + strResultPath, L);
                    CSagComp mOn = CSagComp.Load(strResultPath), mOff = CSagComp.Load(strResultPath);
                    if (mOn == null || !mOn.HasJoint || !mOn.HasFc || mOff == null) return SagRun_GapRefuse(res, "결과 파일에 관절 모델·첫닿음 모델이 없음 — ④ 학습 결과 파일을 고르십시오", L);
                    mOn.Enabled = true; mOn.FloorMapEnabled = true; mOn.Fc.Enabled = true;
                    mOff.Enabled = true; mOff.FloorMapEnabled = false;            // 관절 모델만 (판정의 flooron=0)
                    string rep;
                    List<CSagGapPoint> pts = SagRun_GapPlan(mOn, cfg, armCfg, out rep);
                    res.Points = pts;
                    var use = new List<CSagGapPoint>();
                    foreach (CSagGapPoint p in pts) if (p != null && p.IkOk) use.Add(p);
                    if (use.Count == 0) return SagRun_GapRefuse(res, "잴 점 없음 — " + rep.Replace("\r\n", " / "), L);
                    DateTime t0 = DateTime.Now;
                    string runDir = SagRun_NewRunDir(strUrdfDir, t0, bDry ? "gap_3d" : "gap");
                    res.RunDir = runDir;
                    L = SagRun_SafeLog(log, Path.Combine(runDir, "run.log"));
                    L((bDry ? "3D 틈 재기" : "틈 재기") + " 시작 — " + Path.GetFileName(strResultPath));
                    L(rep.Trim());
                    res.UncheckedAxes = SagArm_UncheckedAxes(conn, armCfg);
                    if (res.UncheckedAxes.Length > 0) L("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes) + " — 계속 (확인 표를 채우십시오)");
                    res.MotorMapWarn = SagArm_MotorMapWarn(bDry ? null : conn, mOn);
                    if (res.MotorMapWarn.Length > 0) L(res.MotorMapWarn);
                    arm = SagArm_Open(conn, armCfg, runDir, "틈 재기", bDry, L, out why);
                    if (arm == null) return SagRun_GapRefuse(res, why, L);
                    SagArm_SetMoveModel(arm, mOff, L);                                                    // 옮기는 동안 = 끔 모델 (관절 모델만, 10-03)
                    bRan = true;
                    if (!SagArm_Unfold(arm, use[0].X, use[0].Y, use[0].TransferZ, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop);
                        res.Why = res.Stopped ? "" : SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        for (int i = 0; i < use.Count; i++)
                        {
                            CSagGapPoint p = use[i];
                            if (SagRun_Stopped(stop)) { res.Stopped = true; break; }
                            float tz = float.IsNaN(p.TransferZ) ? arm.Cfg.SafeZ : p.TransferZ;
                            L(string.Format(ci, "진행 {0}/{1} — 점 ({2:0.#}, {3:0.#}) 위 z={4:0.#} 로 옮김", i + 1, use.Count, p.X, p.Y, tz));
                            if (!SagArm_MoveSafeCore(arm, p.X, p.Y, tz, p.Pitch, tz, p.Z + 5, L, stop))
                            {
                                if (arm.StopSeen) { res.Stopped = true; L("중단 요청 — 틈 재기 멈춤 (옮기던 다리를 마친 자리에서), 원위치"); break; }
                                res.Why = SagRun_WhyJoin(string.Format(ci, "옮기기 실패 ({0:0.#}, {1:0.#}) — 멈춤", p.X, p.Y), arm.LastFail); break;
                            }
                            // 펜 높이로 내려가기 전에 중단 요청 — 옮기는 높이에서 멈추고 원위치 (10-03 검토: 예전에는 다음 점의 펜 높이까지 내려간 뒤에야 멈췄다)
                            if (SagRun_Stopped(stop)) { res.Stopped = true; L("중단 요청 — 펜 높이로 내려가지 않고 멈춤, 원위치"); break; }
                            // 끔: 관절 모델만 (= 옮기는 보정 — 옮기기가 옮기는 높이에서 이미 걸었다, 그대로면 전환 없음) 으로 펜 높이까지 수직 하강
                            //   (MinMoveMs 이상 — 전환이 남았으면 하강 안에서 대부분 끝난다)
                            SagArm_ToMoveModel(arm);
                            if (!SagArm_Descend(arm, p.X, p.Y, p.Z, p.Pitch, L)) { res.Why = SagRun_WhyJoin(string.Format(ci, "펜 높이로 내려가지 못함 ({0:0.#}, {1:0.#}) — 멈춤", p.X, p.Y), arm.LastFail); break; }
                            arm.CurTransferZ = tz; arm.CurZLow = p.Z + 5;
                            bool bStop;
                            p.GapOff = SagRun_GapMeasure(arm, ask, cfg, string.Format(ci, "점 {0}/{1} ({2:0.#}, {3:0.#}) 보정 끔 — 펜 끝과 바닥 사이 틈(mm)", i + 1, use.Count, p.X, p.Y), L, stop, out bStop);
                            if (bStop) { if (!SagRun_GapAbort(arm, res)) res.Stopped = true; break; }
                            // 켬: 결과 모델 (첫닿음 dz 가 끝을 올린다)
                            SagComp_Switch(mOn);
                            p.GapOn = SagRun_GapMeasure(arm, ask, cfg, string.Format(ci, "점 {0}/{1} ({2:0.#}, {3:0.#}) 보정 켬 — 펜 끝과 바닥 사이 틈(mm)", i + 1, use.Count, p.X, p.Y), L, stop, out bStop);
                            if (bStop) { if (!SagRun_GapAbort(arm, res)) res.Stopped = true; break; }
                            if (!arm.IsDry)
                                L(string.Format(ci, "  점 {0}: 끔 {1} → 켬 {2} (변화 예상 {3:+0.0;-0.0})", i + 1, float.IsNaN(p.GapOff) ? "—" : p.GapOff.ToString("0.##", ci),
                                    float.IsNaN(p.GapOn) ? "—" : p.GapOn.ToString("0.##", ci), p.DzPred));
                        }
                    }
                }
                catch (Exception ex) { res.Ok = false; res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally
                {
                    if (arm != null) res.RestOk = SagArm_Close(arm, L);
                }
                if (bRan) SagRun_GapFinish(res, strResultPath, cfg, L);
                return res;
            }
            // 틈 재기를 멈춘 까닭이 중단 요청이 아니면(한 틱 관절 변화 막이·3D 만 싱크 바뀜) res.Why 에 남기고 true
            private static bool SagRun_GapAbort(CSagArm arm, CSagGapResult res)
            {
                string g = SagArm_GuardWhy(arm);
                if (g.Length == 0 && !arm.SinkLost) return false;
                res.Why = SagRun_WhyJoin(res.Why, g.Length > 0 ? g + " — 멈춤" : arm.LastFail);
                return true;
            }
            // 틈 하나 — 실물: 자세를 보내며 붙잡아 보정 전환이 끝날 때까지 → SettleMs → 묻기 / 3D 만: 잠깐 붙잡기만 (묻지 않는다). 반환 = 잰 틈 (빈칸 NaN).
            //   bStop = 멈춘다 (중단 요청·묻기 null — 또는 한 틱 관절 변화 막이·3D 만 싱크 바뀜: SagRun_GapAbort 가 가린다)
            private float SagRun_GapMeasure(CSagArm arm, DSagAsk ask, CSagGapCfg cfg, string strPrompt, CSim2Real.DLog L, CSim2Real.DIsStopped stop, out bool bStop)
            {
                bStop = false;
                var ci = CultureInfo.InvariantCulture;
                if (arm.IsDry)
                {
                    if (!SagArm_SinkOk(arm, L)) { bStop = true; return float.NaN; }
                    var v = new List<float>(); foreach (int id in arm.Cfg.MoveIDs) { v.Add(id); v.Add(GetData(id)); }
                    Play(SagRun_DryMs(1000, arm.Speed), 0, v.ToArray());               // 3D 에 전환을 잠깐 보여 주기만
                    return float.NaN;
                }
                if (!SagArm_HoldUntilBlended(arm, 5000, stop))
                {
                    if (SagRun_Stopped(stop)) { bStop = true; return float.NaN; }
                    string strGuard = SagArm_GuardWhy(arm);
                    if (strGuard.Length > 0) { L("★" + strGuard + " — 틈 재기 멈춤"); bStop = true; return float.NaN; }
                    L(string.Format(ci, "★보정 전환이 아직 다 안 들어감 ({0:F0}%) — 그대로 잼", SagComp_BlendProgress * 100));
                }
                if (cfg.SettleMs > 0) Thread.Sleep(cfg.SettleMs);
                if (SagRun_Stopped(stop)) { bStop = true; return float.NaN; }
                L(strPrompt + " — 입력을 기다림");
                string a;
                try { a = ask(strPrompt); }
                catch (Exception ex) { L("★묻기 오류: " + SagRun_Msg(ex) + " — 중단으로 봄"); bStop = true; return float.NaN; }
                if (a == null) { L("입력 중단 — 원위치"); bStop = true; return float.NaN; }
                float g = SagRun_ParseGap(a);
                if (float.IsNaN(g)) L(a.Trim().Length == 0 ? "  건너뜀" : "  ★수가 아니거나 -50~200 밖 '" + a + "' — 건너뜀");
                else L(string.Format(ci, "  틈 {0:0.##}mm", g));
                return g;
            }
            // 사용자가 넣은 틈 (mm) — 소수점은 '.' 또는 ',', -50~200 밖·수가 아니면 NaN (건너뜀)
            private static float SagRun_ParseGap(string s)
            {
                if (s == null) return float.NaN;
                string t = s.Trim().Replace(',', '.');
                float v;
                if (t.Length == 0 || !float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out v) || float.IsNaN(v) || float.IsInfinity(v) || v < -50 || v > 200) return float.NaN;
                return v;
            }
            // 틈 재기 끝 — 판정, 기록 줄, gap.csv, 보고서
            private static void SagRun_GapFinish(CSagGapResult res, string strResultPath, CSagGapCfg cfg, CSim2Real.DLog L)
            {
                try
                {
                    var ci = CultureInfo.InvariantCulture;
                    if (cfg == null) cfg = new CSagGapCfg();
                    string whyJob = res.Why;
                    SagRun_GapEvaluate(res, cfg);
                    if (whyJob.Length > 0 && !res.Stopped) { res.Why = SagRun_WhyJoin(whyJob, res.Why); if (res.Verdict == "합격") { res.Pass = false; res.Verdict = "판정 불가"; } }
                    res.Ok = whyJob.Length == 0 && !res.Stopped;
                    Func<float, string> V = f => float.IsNaN(f) ? "" : f.ToString("F2", ci);
                    var csv = new StringBuilder("i,x,y,z,pitch,tz,dz_pred,std,gap_off,gap_on,change,change_err,ik,note\r\n");
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 3D 틈 재기 (예상) ===" : "=== 틈 재기 결과 ===");
                    sb.AppendLine("결과 파일: " + (strResultPath ?? ""));
                    sb.AppendLine(string.Format(ci, "펜 높이 z={0:0.#}, 피치 {1:0.#} — 변화 = 켬 틈 − 끔 틈 (위로 +), 예상 = 첫닿음 모델 dz", cfg.Z, cfg.Pitch));
                    sb.AppendLine("       x       y   dz 예상    끔 틈    켬 틈    변화   변화−예상  비고");
                    if (res.Points != null)
                        for (int i = 0; i < res.Points.Count; i++)
                        {
                            CSagGapPoint p = res.Points[i];
                            if (p == null) continue;
                            float ch = (i < res.Change.Length) ? res.Change[i] : float.NaN, ce = (i < res.ChangeErr.Length) ? res.ChangeErr[i] : float.NaN;
                            csv.Append(string.Join(",", new string[] { (i + 1).ToString(ci), V(p.X), V(p.Y), V(p.Z), V(p.Pitch), V(p.TransferZ), V(p.DzPred), V(p.Std), V(p.GapOff), V(p.GapOn), V(ch), V(ce), p.IkOk ? "1" : "0",
                                                                     (p.Note ?? "").Replace(",", ";").Replace("\r", " ").Replace("\n", " ") })).Append("\r\n");
                            sb.AppendLine(string.Format(ci, "  {0,6:F1}  {1,6:F1}   {2,7}  {3,7}  {4,7}  {5,6}  {6,9}  {7}", p.X, p.Y, float.IsNaN(p.DzPred) ? "—" : p.DzPred.ToString("+0.00;-0.00", ci),
                                float.IsNaN(p.GapOff) ? "—" : p.GapOff.ToString("F2", ci), float.IsNaN(p.GapOn) ? "—" : p.GapOn.ToString("F2", ci),
                                float.IsNaN(ch) ? "—" : ch.ToString("+0.00;-0.00", ci), float.IsNaN(ce) ? "—" : ce.ToString("+0.00;-0.00", ci), p.IkOk ? p.Note : "제외 — " + p.Note));
                        }
                    sb.AppendLine("판정: " + res.Verdict + (res.Why.Length > 0 ? " (" + res.Why + ")" : ""));
                    sb.AppendLine(res.Line);
                    if (res.UncheckedAxes != null && res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!string.IsNullOrEmpty(res.MotorMapWarn)) sb.AppendLine(res.MotorMapWarn);
                    sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    res.Report = sb.ToString();
                    if (!string.IsNullOrEmpty(res.RunDir))
                    {
                        res.CsvPath = Path.Combine(res.RunDir, "gap.csv"); SagRun_WriteText(res.CsvPath, csv.ToString());
                        res.ReportPath = Path.Combine(res.RunDir, "gap_report.txt"); SagRun_WriteText(res.ReportPath, res.Report);
                    }
                    if (!string.IsNullOrEmpty(strResultPath)) res.SidecarPath = SagRun_AppendTestLine(strResultPath, SagRun_GapRecordLine(res, cfg));
                    if (L != null) L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★틈 재기 정리 오류: " + SagRun_Msg(ex)); }
            }

            // ── 작업 공용 ──
            // 작업을 시작해도 되나 — 포트·잠금 전에 바로 거절할 것들 (다른 작업 중, urdf 폴더 없음, 실물인데 연결 안 됨·구성 불일치). 잠금은 SagArm_Open 이 잡는다
            private static bool SagRun_JobPrecheck(CArmConn_t conn, bool bDry, string strUrdfDir, out string why)
            {
                why = "";
                if (SagRun_IsBusy) { why = "다른 로봇팔 작업이 진행 중"; return false; }
                if (string.IsNullOrEmpty(strUrdfDir) || strUrdfDir.Trim().Length == 0 || !Directory.Exists(strUrdfDir)) { why = "urdf 폴더 없음: " + strUrdfDir; return false; }
                if (bDry) return true;
                if (conn == null || !conn.IsConnected) { why = "로봇팔 세팅 탭에서 먼저 연결하십시오"; return false; }
                if (conn.ConfigMismatch) { why = "모터 구성이 연결 뒤 바뀜 — 다시 연결하십시오"; return false; }
                return true;
            }
            // 작업 폴더 — <urdf>\sagcomp_runs\<yyyyMMdd_HHmmss>_<kind>[_n]
            private static string SagRun_NewRunDir(string strUrdfDir, DateTime t, string strKind)
            {
                string root = SagRun_RunsDir(strUrdfDir);
                Directory.CreateDirectory(root);
                string stem = t.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + "_" + strKind;
                for (int n = 1; n < 1000; n++)
                {
                    string p = Path.Combine(root, stem + (n > 1 ? "_" + n.ToString(CultureInfo.InvariantCulture) : ""));
                    if (Directory.Exists(p) || File.Exists(p)) continue;
                    Directory.CreateDirectory(p);
                    return p;
                }
                throw new IOException("작업 폴더 이름을 만들지 못함 (" + root + ")");
            }
            private static List<CFloorPlanRow> SagRun_ClonePlan(List<CFloorPlanRow> plan)
            {
                var l = new List<CFloorPlanRow>();
                if (plan != null) foreach (CFloorPlanRow r in plan) if (r != null) l.Add(r.Clone());
                return l;
            }
            private static CSagComp SagRun_LoadQuiet(string strPath)
            {
                try { return (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) ? null : CSagComp.Load(strPath); }
                catch { return null; }
            }
            // 파일 쓰기 (UTF-8 BOM 없음) — 실패해도 작업은 이어진다
            private static void SagRun_WriteText(string strPath, string strText)
            {
                try { if (!string.IsNullOrEmpty(strPath)) File.WriteAllText(strPath, strText ?? "", new UTF8Encoding(false)); } catch { }
            }
            private static CSagRunResult SagRun_RunRefuse(CSagRunResult res, string why, CSim2Real.DLog L)
            {
                res.Ok = false; res.Why = why; res.Line = "시작하지 않음 — " + why; res.Report = res.Line;
                if (L != null) L("★" + res.Line);
                return res;
            }
            private static CSagTestResult SagRun_TestRefuse(CSagTestResult res, string why, CSim2Real.DLog L)
            {
                res.Ok = false; res.Pass = false; res.Why = why; res.Verdict = "판정 불가"; res.Line = "시작하지 않음 — " + why; res.Report = res.Line;
                if (L != null) L("★" + res.Line);
                return res;
            }
            private static CSagGapResult SagRun_GapRefuse(CSagGapResult res, string why, CSim2Real.DLog L)
            {
                res.Ok = false; res.Pass = false; res.Why = why; res.Verdict = "판정 불가"; res.Line = "시작하지 않음 — " + why; res.Report = res.Line;
                if (L != null) L("★" + res.Line);
                return res;
            }

            // ────────────────────────────────────────────────────────────
            // 측정 데이터 — 궤적 재분석 → 데이터셋 (측정 작업 끝·옛 궤적 다시 뽑기)
            // ────────────────────────────────────────────────────────────

            /// <summary>접촉 → 첫닿음 데이터셋 행 (SagComp_FloorDatasetRows 형식 20칸, kind·fc·fccmdz·hold 포함) — pitch 칸만 관절 합 q2+q3+q4
            /// (= 12+13+14 이상 관절각, q 칸)으로 바꿔 적는다: floor_dataset_fc.csv 의 규약 (처음 73점 = 판정 자료, Ry 70 묶음 = 110).
            /// SagComp_FloorDatasetRows 는 공구 피치 Ry 를 적는다 — 같은 자세가 두 값으로 적히면 첫닿음 학습의 '같은 자리 다른 측정 엇갈림' 검사(피치 1° 안)가
            /// 놓친다. q 가 비면 180 − Ry (계획·IK 가 고르는 손목 해 쪽). group 칸(p{Ry}y{요})·나머지 칸은 그대로. session 의 쉼표·줄바꿈은 '_'·빈칸으로.
            /// 측정 작업이 데이터셋(floor_dataset_run.csv·floor_dataset_fc.csv)에 쓸 행은 이 함수로 만든다. 예외 없음 (실패하면 "").</summary>
            public static string SagRun_DatasetRows(List<CFloorContact> contacts, string strSession, bool bHeader)
            {
                try
                {
                    string s = (strSession ?? "").Replace(",", "_").Replace("\r", "").Replace("\n", "");
                    return SagRun_PitchAsJointSum(SagComp_FloorDatasetRows(contacts, s, bHeader), true);
                }
                catch { return ""; }
            }

            /// <summary>궤적(SagComp_GetFloorTraceCsv 형식, 이어 붙인 파일도) → 첫닿음 재분석(FirstTouch 켬) → 데이터셋 행 (SagRun_DatasetRows — pitch 칸 = 관절 합).
            /// bHeader = 머리줄 포함. contacts = 하강마다 재분석 결과, strStats = "하강 93 · 행 78 (F 60, H 10, C 3, X 5)". 예외 없음 (실패하면 "").</summary>
            public static string SagRun_ExtractTrace(string strTraceCsv, string strSession, bool bHeader, out List<CFloorContact> contacts, out string strStats)
            {
                contacts = new List<CFloorContact>(); strStats = "";
                try
                {
                    var cfg = new CFloorProbeCfg { FirstTouch = true };
                    contacts = SagComp_FloorContactsFromTrace(strTraceCsv, cfg);
                    string rows = SagRun_DatasetRows(contacts, string.IsNullOrEmpty(strSession) ? "trace" : strSession, bHeader);
                    int nF = 0, nH = 0, nC = 0, nX = 0, nR = 0;
                    foreach (CFloorContact c in contacts)
                    {
                        if (!c.Detected || c.JointAct == null || c.JointAct.Length < 4) continue;   // SagComp_FloorDatasetRows 와 같은 거르기
                        nR++;
                        if (c.Kind == "F") nF++; else if (c.Kind == "H") nH++; else if (c.Kind == "C") nC++; else if (c.Kind == "X") nX++;
                    }
                    strStats = string.Format(CultureInfo.InvariantCulture, "하강 {0} · 행 {1} (F {2}, H {3}, C {4}, X {5})", contacts.Count, nR, nF, nH, nC, nX);
                    return rows;
                }
                catch (Exception ex) { strStats = "★오류: " + SagRun_Msg(ex); return ""; }
            }

            /// <summary>데이터셋에 행 덧붙이기 — 파일이 없거나 머리줄(session,…)이 없으면 머리줄과 함께, 있으면 머리줄을 뺀 행만 (UTF-8 BOM 없음, CRLF).
            /// 머리줄 없음 = BOM·빈 줄만 있는 파일 (메모장으로 만든 빈 파일 등 — 10-03 검토: 길이만 보고 머리줄 없이 덧붙여 모든 행을 읽지 못했다).
            /// strRows 는 SagComp_FloorDatasetRows 형식 (머리줄이 있어도 된다) — q 칸이 있는 행의 pitch 칸은 관절 합 q2+q3+q4 로 맞춰 적는다
            /// (SagRun_DatasetRows 와 같은 규약, 이미 맞춘 행은 그대로). 반환 = 덧붙인 행 수 (실패 −1). 예외 없음.</summary>
            public static int SagRun_AppendDataset(string strDatasetPath, string strRows)
            {
                try
                {
                    if (string.IsNullOrEmpty(strDatasetPath)) return -1;
                    string[] lines = SagRun_PitchAsJointSum(strRows ?? "", false).Replace("\r", "").Split('\n');
                    string head = null; var body = new List<string>();
                    foreach (string ln in lines)
                    {
                        if (ln.Trim().Length == 0) continue;
                        if (ln.StartsWith("session,")) { if (head == null) head = ln; continue; }
                        body.Add(ln);
                    }
                    if (head == null) head = SagComp_FloorDatasetRows(null, "", true).TrimEnd('\r', '\n');
                    bool bExists = File.Exists(strDatasetPath);
                    bool bNew = !bExists || new FileInfo(strDatasetPath).Length == 0 || !SagRun_HasDatasetHeader(strDatasetPath);
                    var sb = new StringBuilder();
                    if (bExists)
                    {
                        // 끝에 줄바꿈이 없으면 붙인다 (머리줄을 새로 쓸 때도 — 앞 줄(BOM 만 있는 줄 등)에 붙지 않게)
                        using (var fs = new FileStream(strDatasetPath, FileMode.Open, FileAccess.Read))
                        {
                            if (fs.Length > 0) { fs.Seek(-1, SeekOrigin.End); if (fs.ReadByte() != '\n') sb.Append("\r\n"); }
                        }
                    }
                    if (bNew) sb.Append(head).Append("\r\n");
                    foreach (string ln in body) sb.Append(ln).Append("\r\n");
                    string dir = Path.GetDirectoryName(strDatasetPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    File.AppendAllText(strDatasetPath, sb.ToString(), new UTF8Encoding(false));
                    return body.Count;
                }
                catch { return -1; }
            }

            // 데이터셋 파일에 머리줄(session,…)이 있나 — BOM·앞뒤 빈칸을 떼고 본다 (읽지 못하면 있다고 보고 덧붙이기에 맡긴다 — 읽기 실패는 거기서 드러난다)
            private static bool SagRun_HasDatasetHeader(string strPath)
            {
                try
                {
                    using (var fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false), true))
                    {
                        string ln;
                        while ((ln = sr.ReadLine()) != null)
                            if (ln.Trim().TrimStart('﻿').Trim().StartsWith("session,")) return true;
                    }
                    return false;
                }
                catch { return true; }
            }

            /// <summary>새 측정 회차 이름 — "MMdd_HHmmss" (그 시각), 데이터셋에 이미 있으면 뒤에 b, c, … (두 상태·묶음 구분이 session 칸에 기댄다)</summary>
            public static string SagRun_NewSession(string strDatasetPath, DateTime t)
            {
                string s = t.ToString("MMdd_HHmmss", CultureInfo.InvariantCulture);
                try
                {
                    if (string.IsNullOrEmpty(strDatasetPath) || !File.Exists(strDatasetPath)) return s;
                    var used = new List<string>();
                    foreach (string ln in File.ReadAllLines(strDatasetPath)) { int c = ln.IndexOf(','); if (c > 0) used.Add(ln.Substring(0, c).Trim()); }
                    if (!used.Contains(s)) return s;
                    for (char ch = 'b'; ch <= 'z'; ch++) if (!used.Contains(s + ch)) return s + ch;
                    return s + "_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
                }
                catch { return s; }
            }

            // ────────────────────────────────────────────────────────────
            // 공용 도우미
            // ────────────────────────────────────────────────────────────

            // 로그 — 줄마다 시각을 붙여 파일(UTF-8 BOM 없음, strFile = null 이면 안 씀)에 덧붙이고 inner 를 부른다. 둘 다 예외를 삼킨다
            //   (10-02 14:54 로그 대리자 예외가 실측을 끊었다 — 작업 쪽 로그 대리자가 던져도 작업은 이어진다)
            private static CSim2Real.DLog SagRun_SafeLog(CSim2Real.DLog inner, string strFile)
            {
                object o = new object();
                return new CSim2Real.DLog(delegate(string s)
                {
                    if (!string.IsNullOrEmpty(strFile))
                    {
                        try
                        {
                            lock (o)
                            {
                                string dir = Path.GetDirectoryName(strFile);
                                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                File.AppendAllText(strFile, DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  " + (s ?? "") + "\r\n", new UTF8Encoding(false));
                            }
                        }
                        catch { }
                    }
                    if (inner != null) { try { inner(s); } catch { } }
                });
            }

            // 바닥 궤적 버퍼를 lock 안에서 꺼내고 비운다 (증분 저장 — 긴 측정에서 버퍼를 매번 통째로 복사하지 않게).
            //   버퍼를 비우면 다음 줄이 머리줄을 다시 쓰므로, 이미 머리줄을 썼으면(bHeaderDone) 첫 머리줄을 떼어 낸다 → 한 파일에 머리줄 하나
            private string SagRun_TraceTake(ref bool bHeaderDone)
            {
                string s = SagRun_TraceTakeRaw();
                if (s.Length == 0) return "";
                if (s.StartsWith("r,seg,"))
                {
                    if (bHeaderDone) { int nl = s.IndexOf('\n'); s = nl >= 0 ? s.Substring(nl + 1) : ""; }
                    else bHeaderDone = true;
                }
                return s;
            }
            // 바닥 궤적 버퍼를 꺼내고 비운다 (머리줄 그대로 — 버퍼가 비어 있다 처음 쓰는 줄이 머리줄이다: 누름 하나 = 머리줄 + 그 누름의 줄)
            private string SagRun_TraceTakeRaw()
            {
                string s;
                lock (m_sbSagFloor) { s = m_sbSagFloor.ToString(); m_sbSagFloor.Length = 0; }
                return s;
            }
            // 궤적 조각(머리줄 포함일 수 있음)을 파일에 덧붙인다 — 그 파일에 이미 머리줄을 썼으면(bHeaderDone) 조각의 머리줄을 떼어 낸다 (한 파일에 머리줄 하나).
            //   쓰기 실패는 예외 (작업이 잡아 멈춘다 — 궤적 없이 재면 데이터셋이 빈다)
            private static void SagRun_TraceAppend(string strPath, string strRaw, ref bool bHeaderDone)
            {
                if (string.IsNullOrEmpty(strPath) || string.IsNullOrEmpty(strRaw)) return;
                string s = strRaw;
                if (s.StartsWith("r,seg,"))
                {
                    if (bHeaderDone) { int nl = s.IndexOf('\n'); s = nl >= 0 ? s.Substring(nl + 1) : ""; }
                    else bHeaderDone = true;
                }
                if (s.Length > 0) File.AppendAllText(strPath, s, new UTF8Encoding(false));
            }

            // 관절 모델만 (측정용 모델) — 새로 읽어 바닥 보정 끔 (GP·보정표·첫닿음 모델은 남아도 꺼짐). 관절 모델이 없으면 null
            private static CSagComp SagRun_LoadJointOnly(string strPath, out string why)
            {
                why = "";
                try
                {
                    if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) { why = "모델 파일 없음: " + strPath; return null; }
                    CSagComp m = CSagComp.Load(strPath);
                    if (m == null || !m.HasJoint) { why = "관절 모델이 없음 — ⑨ 처짐보상으로 먼저"; return null; }
                    m.FloorMapEnabled = false;
                    return m;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return null; }
            }

            // 바닥 보정이 실제로 켜지는 모델인가 — 출력 훅(SagComp_JointOutModel)과 같은 조건 (첫닿음 모델 fcon=1, 아니면 GP, 아니면 반경 보정표; flooron)
            private static bool SagRun_FloorActive(CSagComp m)
            {
                if (m == null) return false;
                bool bFc = m.HasFc && m.Fc.Enabled;
                return (bFc || (m.HasGp ? m.Gp.Enabled : m.HasFloorMap)) && m.FloorMapEnabled;
            }

            // 데이터셋 한 행 (머리줄 이름으로 읽은 것)
            private class CSagDsRow
            {
                public string Session = "", Group = "", Kind = "";
                public float X = float.NaN, Y = float.NaN, Pitch = float.NaN, R = float.NaN, FkZ = float.NaN, CmdZ = float.NaN, Fc = float.NaN, FcCmdZ = float.NaN;
                public float[] A = null, Q = null;                  // 실측·이상 관절각 (PosIDs 3개 + 손목), 칸이 비면 NaN
                public bool Hold;
            }
            // 데이터셋 CSV 읽기 — 머리줄 이름으로 칸을 찾는다 (q1..q4 또는 q11..q14, a 도 같음). 이어 붙인 파일의 머리줄은 칸을 다시 읽고,
            //   머리줄보다 칸이 적은 줄은 건너뛴다(nBroken). 옛 16칸 머리줄 뒤에 새 20칸 행이 붙어 있으면 kind,fc,fccmdz,hold 칸을 늘려 읽는다
            private static List<CSagDsRow> SagRun_ReadDataset(string strCsv, out string why, out int nBroken)
            {
                why = ""; nBroken = 0;
                var rows = new List<CSagDsRow>();
                if (string.IsNullOrEmpty(strCsv)) { why = "빈 데이터셋"; return rows; }
                var ci = CultureInfo.InvariantCulture;
                string[] lines = strCsv.Replace("\r", "").Split('\n');
                List<string> hd = null; int nBase = 0;
                int iS = -1, iG = -1, iX = -1, iY = -1, iP = -1, iR = -1, iFk = -1, iCz = -1, iK = -1, iFc = -1, iFcc = -1, iH = -1;
                int[] iA = null, iQ = null;
                Func<string[], int, float> F = (f, k) => { float v; return (k >= 0 && k < f.Length && float.TryParse(f[k].Trim(), NumberStyles.Float, ci, out v)) ? v : float.NaN; };
                Func<string[], int, string> S = (f, k) => (k >= 0 && k < f.Length) ? f[k].Trim() : "";
                for (int li = 0; li < lines.Length; li++)
                {
                    string ln = li == 0 ? lines[li].TrimStart('﻿') : lines[li];
                    if (ln.Trim().Length == 0) continue;
                    string[] f = ln.Split(',');
                    if (f[0].Trim() == "session")
                    {
                        hd = new List<string>(); foreach (string h in f) hd.Add(h.Trim());
                        nBase = hd.Count;
                        if (hd.IndexOf("fc") < 0 && hd.Count == 16 && hd.IndexOf("fkz") == 6) hd.AddRange(new string[] { "kind", "fc", "fccmdz", "hold" });
                        iS = hd.IndexOf("session"); iG = hd.IndexOf("group"); iX = hd.IndexOf("x"); iY = hd.IndexOf("y"); iP = hd.IndexOf("pitch"); iR = hd.IndexOf("r");
                        iFk = hd.IndexOf("fkz"); iCz = hd.IndexOf("cmdz"); iK = hd.IndexOf("kind"); iFc = hd.IndexOf("fc"); iFcc = hd.IndexOf("fccmdz"); iH = hd.IndexOf("hold");
                        iA = new int[] { hd.IndexOf("a1"), hd.IndexOf("a2"), hd.IndexOf("a3"), hd.IndexOf("a4") };
                        if (Array.IndexOf(iA, -1) >= 0) iA = new int[] { hd.IndexOf("a11"), hd.IndexOf("a12"), hd.IndexOf("a13"), hd.IndexOf("a14") };
                        iQ = new int[] { hd.IndexOf("q1"), hd.IndexOf("q2"), hd.IndexOf("q3"), hd.IndexOf("q4") };
                        if (Array.IndexOf(iQ, -1) >= 0) iQ = new int[] { hd.IndexOf("q11"), hd.IndexOf("q12"), hd.IndexOf("q13"), hd.IndexOf("q14") };
                        continue;
                    }
                    if (hd == null || f.Length < nBase) { nBroken++; continue; }
                    var r = new CSagDsRow
                    {
                        Session = S(f, iS), Group = S(f, iG), Kind = S(f, iK),
                        X = F(f, iX), Y = F(f, iY), Pitch = F(f, iP), R = F(f, iR), FkZ = F(f, iFk), CmdZ = F(f, iCz), Fc = F(f, iFc), FcCmdZ = F(f, iFcc),
                        Hold = S(f, iH) == "1"
                    };
                    r.A = new float[] { F(f, iA[0]), F(f, iA[1]), F(f, iA[2]), F(f, iA[3]) };
                    r.Q = new float[] { F(f, iQ[0]), F(f, iQ[1]), F(f, iQ[2]), F(f, iQ[3]) };
                    rows.Add(r);
                }
                if (hd == null) why = "머리줄(session,…) 없음";
                else if (nBroken > 0) why = string.Format(ci, "칸이 모자란 줄 {0} 건너뜀", nBroken);
                return rows;
            }
            // 데이터셋 CSV 쓰기 — SagComp_FloorDatasetRows 와 같은 머리줄·서식 (빈 값은 빈칸)
            private static string SagRun_DatasetCsv(List<CSagDsRow> rows)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder(SagComp_FloorDatasetRows(null, "", true));
                if (rows == null) return sb.ToString();
                Func<float, string, string> V = (f, fmt) => (float.IsNaN(f) || float.IsInfinity(f)) ? "" : f.ToString(fmt, ci);
                Func<float[], int, string> J = (a, k) => (a != null && k < a.Length) ? V(a[k], "F3") : "";
                foreach (CSagDsRow r in rows)
                {
                    sb.Append(string.Join(",", new string[] {
                        r.Session, r.Group, V(r.X, "F2"), V(r.Y, "F2"), V(r.Pitch, "F1"), V(r.R, "F1"), V(r.FkZ, "F3"), V(r.CmdZ, "F3"),
                        J(r.A, 0), J(r.A, 1), J(r.A, 2), J(r.A, 3), J(r.Q, 0), J(r.Q, 1), J(r.Q, 2), J(r.Q, 3),
                        r.Kind, V(r.Fc, "F3"), V(r.FcCmdZ, "F3"), r.Hold ? "1" : "0" }));
                    sb.Append("\r\n");
                }
                return sb.ToString();
            }

            // 데이터셋 행 글(SagComp_FloorDatasetRows 형식)의 pitch 칸 → 관절 합 q2+q3+q4 (F1). 칸은 머리줄 이름으로 찾는다 (글 안의 머리줄, 없으면
            //   SagComp_FloorDatasetRows 머리줄; q12..q14 이름도). q 가 비거나 NaN 인 행: bRyFallback 이면 180 − pitch (pitch ≤ 90 = Ry 로 적힌 행 —
            //   SagComp_FloorDatasetRows 가 막 만든 글), 아니면 그대로 (규약을 모르는 남의 행). 줄바꿈·나머지 칸은 그대로 둔다.
            //   180 − pitch 대체값은 q 칸 없는 옛 행(OMX 데이터 — 수직 관절 합 90)만이다 — 새 행은 늘 q 합 (로봇 정의의 downsum 과 무관하게 정확)
            private static string SagRun_PitchAsJointSum(string strRows, bool bRyFallback)
            {
                if (string.IsNullOrEmpty(strRows)) return strRows ?? "";
                var ci = CultureInfo.InvariantCulture;
                int iP = -1, i2 = -1, i3 = -1, i4 = -1;
                Action<string[]> Head = h =>
                {
                    var l = new List<string>(); foreach (string x in h) l.Add(x.Trim().TrimStart('﻿'));
                    iP = l.IndexOf("pitch"); i2 = l.IndexOf("q2"); i3 = l.IndexOf("q3"); i4 = l.IndexOf("q4");
                    if (i2 < 0 || i3 < 0 || i4 < 0) { i2 = l.IndexOf("q12"); i3 = l.IndexOf("q13"); i4 = l.IndexOf("q14"); }
                };
                Head(SagComp_FloorDatasetRows(null, "", true).TrimEnd('\r', '\n').Split(','));
                string[] lines = strRows.Split('\n');
                var sb = new StringBuilder(strRows.Length + 16);
                for (int li = 0; li < lines.Length; li++)
                {
                    string ln = lines[li];
                    bool bCr = ln.EndsWith("\r");
                    string body = bCr ? ln.Substring(0, ln.Length - 1) : ln;
                    string[] f = body.Split(',');
                    if (f[0].Trim().TrimStart('﻿') == "session") Head(f);
                    else if (iP >= 0 && i2 >= 0 && i3 >= 0 && i4 >= 0 && f.Length > Math.Max(iP, Math.Max(i2, Math.Max(i3, i4))))
                    {
                        double q2, q3, q4, p, sum = double.NaN;
                        if (double.TryParse(f[i2].Trim(), NumberStyles.Float, ci, out q2) && double.TryParse(f[i3].Trim(), NumberStyles.Float, ci, out q3)
                            && double.TryParse(f[i4].Trim(), NumberStyles.Float, ci, out q4)) sum = q2 + q3 + q4;
                        if ((double.IsNaN(sum) || double.IsInfinity(sum)) && bRyFallback && double.TryParse(f[iP].Trim(), NumberStyles.Float, ci, out p) && p <= 90.01) sum = 180.0 - p;
                        if (!double.IsNaN(sum) && !double.IsInfinity(sum)) { f[iP] = sum.ToString("F1", ci); body = string.Join(",", f); }
                    }
                    if (li > 0) sb.Append('\n');
                    sb.Append(body);
                    if (bCr) sb.Append('\r');
                }
                return sb.ToString();
            }

            // 접촉 CSV (측정 floor_live.csv, 시험 test.csv 공통) — 머리줄 / 한 줄 (note 의 쉼표는 ';')
            private static string SagRun_ContactCsvHeader()
            {
                return "tag,mode,k,pitch,yaw,r,x,y,zs,zm,detected,safety,fitok,cmdz,cmdz_eff,fd_knee,fkz,base,slope_press,fitrms,lift_cmd,lift_fk,lag_at,load_at,ticks,pe_air,pe_knee,wrist_hold,q11,q12,q13,q14,a11,a12,a13,a14,kind,fc,fccmdz,note\r\n";
            }
            private static string SagRun_ContactCsvRow(string strTag, string strMode, int k, float pitch, float yaw, float zs, float zm, CFloorContact c)
            {
                var ci = CultureInfo.InvariantCulture;
                Func<float, string, string> V = (f, fmt) => (float.IsNaN(f) || float.IsInfinity(f)) ? "" : f.ToString(fmt, ci);
                if (c == null) c = new CFloorContact { CmdZ = float.NaN, FkZ = float.NaN };
                Func<float[], int, string> J = (a, i) => (a != null && i < a.Length) ? V(a[i], "F3") : "";
                return string.Join(",", new string[] {
                    (strTag ?? "").Replace(",", ";"), (strMode ?? "").Replace(",", ";"), k.ToString(ci), V(pitch, "F1"), V(yaw, "F1"), V(c.R, "F1"), V(c.X, "F2"), V(c.Y, "F2"),
                    V(zs, "F2"), V(zm, "F2"), c.Detected ? "1" : "0", c.Safety ? "1" : "0", c.FitOk ? "1" : "0",
                    V(c.CmdZ, "F3"), V(c.CmdZEff, "F3"), V(c.FdAtKnee, "F3"), V(c.FkZ, "F3"), V(c.BaseErr, "F3"), V(c.SlopePress, "F3"), V(c.FitRms, "F3"),
                    V(c.KneeCmdLift, "F3"), V(c.FkLift, "F3"), V(c.LagAtPress, "F2"), V(c.LoadDeltaAtPress, "F1"), c.Ticks.ToString(ci),
                    V(c.PitchErrDeg, "F2"), V(c.PitchErrKnee, "F2"), V(c.WristHoldDeg, "F2"),
                    J(c.JointCmd, 0), J(c.JointCmd, 1), J(c.JointCmd, 2), J(c.JointCmd, 3), J(c.JointAct, 0), J(c.JointAct, 1), J(c.JointAct, 2), J(c.JointAct, 3),
                    (c.Kind ?? "").Replace(",", ";"), V(c.FcZ, "F3"), V(c.FcCmdZ, "F3"),
                    (c.Note ?? "").Replace(",", ";").Replace("\r", " ").Replace("\n", " ") }) + "\r\n";
            }
        }
    }
}
