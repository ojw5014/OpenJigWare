// ================================================================
// C3d — 처짐 보정 실습  2026-10-04 (10-15: 두 단계 — 학습 + 적용 예제)
//   MakeUrdf 로봇팔 세팅 탭의 "처짐 보정 실습" 페이지가 버튼마다 부르는 절차. 계산·판정·파일은 모두 여기 — 앱은 부르고 그림만 그린다.
//   10-15 두 단계: [1. 학습] = SagPrac_LearnJob — 한 번 펼치고 한 번 원위치하는 작업 하나 (적용 중 보정·공구 기울기 보정은 끝까지 끔):
//     (a) 기준 오차 (아래 옛 1단계와 같은 누름 — 바닥 찾기 → 점마다 첫닿음 명령 높이) → (b) 공중 관절 처짐 → 관절 모델 →
//     (c) 바닥 첫닿음 데이터 (방금 만든 관절 모델만 켬 — 학습 목표가 '관절 모델을 켠 자세의 첫닿음 FK') → 로봇 데이터셋 →
//     원위치 → (d) 학습 → 결과 파일 + 기준 점에서의 예상 (보정 켬 = 기준 − 결과 모델이 끝을 올리는 양, predict.csv) → 적용 권장 여부 (ApplyAdvised).
//     같은 작업의 기준이 곧 기준이라 옛 2·3단계의 '시작 확인' (가운데 점 다시 누르기) 은 없다. 몇 점을 못 재도 남은 점이 충분하면 이어 가고 수를 알린다.
//     앱은 끝에 "이 보정을 적용할까요?" 를 묻고 SagPrac_Apply (기본 [예] — 권장이 아니면 까닭과 [아니오]).
//     두 번째 버튼 [적용 예제 (파이썬)] 은 선택 — 앱이 예제 실행기를 띄운다 (실행기·예제는 python_kit — 적용 중 모델을 SagComp_Load 로 읽어 PlayXyz·PlayXyzPath 로 그린다).
//     첫닿음 그럴듯함 (SagPrac_Plausible, 10-15): 바닥은 평평하므로 첫닿음 순간의 FK·명령 높이는 반경을 따라 매끄럽다 — 벗어나면 추세 높이를
//     가운데로 한 창으로 한 번 다시, 그래도 벗어나면 뺀다. 수직 눈 확인은 어느 작업도 요구하지 않는다 (선택 — '로봇 정의' 페이지, SagPrac_EyeJob).
//   10-15 검토 고침:
//     · (d) 학습은 이번 회차 (c 가 덧붙인 session) 행만 쓴다 — 누적 데이터셋에는 바닥이 오늘보다 +1.0 ~ +3.0mm 높은 옛 회차와 바닥이 아닌 데서 걸린 행 (r245 fc 66.0)
//       이 섞여 있어, 모두로 배운 결과는 보정 켬 첫닿음이 Ref 보다 낮았다 (오전 3단계 실측 −4.0 ~ −4.8mm, 다음 빠르게 학습 흉내 −3.3mm — 펜이 PaperZ 에서 뜬다).
//       높이 기준 (c0·Ref) 은 이번 회차의 첫닿음 명령 높이 (관절 모델 켬) 에 맞춘다 (C3d.CSagFcFit.CmdLevel). 행은 지금처럼 누적 데이터셋에 덧붙인다.
//       데이터셋·손목 유지 켬 행·기준 높이 고정은 sagarm.txt 의 learndataset·learnusehold·learnref 대로 (SagPrac_LearnSet).
//     · 적용 권장에 예상 첫닿음 − Ref 평균 (SAGPRAC_ADVISE_OFFSET_MM)·Ref RMS (SAGPRAC_ADVISE_RMS_MM) 조건 — 적용 묻기 글도 DLL (SagPrac_ApplyAskText).
//     · 바닥 데이터가 학습에 필요한 수 (LearnMinData 6 = 학습이 쓰는 행 수) 가 될 수 없으면 남은 점을 누르지 않고 멈춘다.
//     · 기준에서 FK 는 추세 안이고 명령 높이만 벗어난 첫닿음은 다시 누르지 않고 뺀다. 다시 누를 창은 방금 닿았을 수 있는 높이보다 낮게 시작하지 않는다 (SagPrac_PlausPress).
//     · 바닥 데이터의 예상 높이는 내려가는 방향으로 관절 모델이 올리는 양을 뺀다 (SagPrac_Predict 와 같다 — 방향 0 은 먼 쪽에서 실제보다 12mm 까지 낮았다),
//       다시 누름 창 시작은 그 단계에서 받아들인 첫닿음 명령 높이의 추세 아래로 잡지 않는다.
//     · 적용 권장은 [1. 학습] 이 낸 판정 하나 — 진행 상태 (advised·advicewhy) 에 남겨 그래프·안내·적용 예제 모델 고르기가 따른다 (SagPrac_ChartData).
//     · 적용은 작업이 돌려준 결과 파일과 진행 상태를 맞춰 본다 (SagPrac_Apply — 상태를 쓰지 못했으면 옛 결과를 적용하지 않는다).
//   옛 네 단계 (아래 — 함수는 그대로 남는다, 실습 페이지는 쓰지 않는다. 옛 state.txt 의 step 1~4 는 그대로 읽힌다: 1 = 기준만, 2 이상 = 결과 있음):
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
//      첫닿음 FK 높이가 1단계 FK 추세 (그 반경 — 10-15, 점이 모자라면 같은 점의 1단계 FK) 와 FkTolMm 넘게 다르면 바닥이 아닌 데서 걸린 것으로 보고 뺀다 (SagPrac_FkGate)
//   4. 적용 (SagPrac_Apply) — 3단계에서 흩어짐이 충분히 줄었을 때만 (ApplyOk) SagRun_Activate (백업 + 적용). 앱이 시작할 때 적용 중 파일을 읽어 그 뒤 모든 동작이 보정된다
//   진행 상태는 <urdf>\sagcomp_practice\state.txt — 앱을 다시 켜도 이어서 (버튼 차례: 2 는 1 뒤, 3 은 2 뒤, 4 는 3 뒤).
//   로봇 정의 (10-04, COjw_12_3D_SagDef.cs): 1단계가 정의 서명(def)을 상태에 적고 2·3·4단계는 지금 정의와 대조한다 (다르면 1단계부터 — 앱을 다시 켜거나
//   sagarm.txt 를 밖에서 고쳐도).
//   수직 눈 확인만 하는 작업 (SagPrac_EyeJob, 10-05) — 펼치기 → 묻기 → 기록 → 원위치, 바닥은 누르지 않고 실습 진행(단계·폴더·결과)은 그대로.
//   10-15: 눈 확인은 선택 사항 — 실습·③ 측정·⑤ 닿기 시험은 기록이 없어도 시작한다 (SagPrac_EyeGate 는 로그 한 줄만). 1단계도 묻지 않는다.
//     예전에는 1단계가 끝날 때 상태를 다시 쓰며 눈 확인 기록의 공구 기울기 서명(eyetilt)을 빠뜨려, 기울기 보정이 걸린 로봇은 1단계를 할 때마다
//     눈 확인이 '확인 전' 으로 돌아갔다 (10-15 실측 — 2단계가 늘 거절됨). 상태를 새로 쓰는 길은 이제 눈 확인 칸을 모두 넘겨받는다 (SagPrac_StateKeepEye).
//   10-13: 눈 확인은 베이스 가까운 자리(CSagPracCfg.EyeRadius 130)에서 — 먼 자리는 중력 처짐이 펜을 기울여 맞는 로봇도 수직이 아니게 보였다.
//     실물은 묻기 전에 엔코더로 공구를 수직으로 붙잡고 (손목 수직 유지 — 엔코더로 보이는 처짐을 없앤다), [아니오] 면 손목을 조금씩 기울이는
//     미세 조정 (DSagEyeFine). 1단계와 눈 확인만이 같은 길 (SagPrac_EyeAt).
//   10-15: [수직임] 은 손목 영점을 고치지 않는다 — 그때의 엔코더 공구 기울기 m 으로 '손목 공구 기울기 보정' τ = −m 을 배운다
//     (엔코더 밖 기울기 — 영점·펜 장착·브래킷 등: 처짐 보정 모델의 항 CSagComp.WristTilt*, 출력 단이 손목 명령에서 τ 를 뺀다).
//     먼 자리도 재면 (CSagPracCfg.EyeFar, 반경 EyeFarRadius) 두 점으로 자세 항 b12 까지 맞춘다 (SagPrac_EyeTiltFit — 두 자리 차가 1° 안이면 일정).
//     배운 항이 지금 걸린 항과 같으면 (EyeTiltSameDeg) 기록만, 다르면 제안 (EyeAns "기울기", res.EyeTiltFit) — 앱이 묻고 (기본 [아니오])
//     SagRun_TiltApply (적용 중 모델에 넣기 — 백업 + 1초 전환, 모델이 없으면 기울기만 있는 모델) 뒤 SagPrac_EyeRecord.
//     기울기 보정이 걸려 있으면 눈 확인도 그 보정을 건 채 본다 (자동 수직 유지 기준 = 수직 − τ) — [예] 면 그 보정을 확인한 것.
//     눈 확인 기록에는 그때의 기울기 보정 서명도 남는다 (state.txt eyetilt) — 보정을 바꾸거나 지우면 다시 해야 한다.
//   그래프 자료는 SagPrac_ChartData (반경·전·후 높이·기준선) — 앱은 그리기만 한다.
// 꺾는 손목 없음 (10-13, CSagArmCfg.WristID = −1): 단계는 같다. 점은 피치 90 행만 (위치만 IK — 공구 피치는 자세마다 바뀐다), 수직 눈 확인은
//   해당 없음 (입구는 로그 한 줄 남기고 통과, 눈 확인만 작업은 거절), 손목 수직 유지는 꺼진다. 첫닿음은 팔꿈치 잔차·보조 판정 (CSagFtLive.NoWrist).
// 3D 만 (bDry): 실물 없이 가상 로봇(관절 처짐 c·지레 + 데드밴드, 끝 휨, 가상 바닥 — CSagPracCfg.Sim*)으로 같은 순서를 계산하고 3D 로 보여 준다.
//   기준 점은 3D 팔이 점마다 내려가는 모습을 보이고, 관절 처짐·바닥 데이터는 계산만. 결과·데이터셋은 그 실습 폴더 안에만 (로봇 데이터셋·결과 목록에 섞지 않는다).
//   3D 만 실습은 적용하지 않는다. 실물 실습과 3D 만 실습은 서로 이어지지 않는다.
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
                // ── 3단계 첫닿음 높이 확인 ──
                public float FkTolMm = 8;                      // 첫닿음 FK 높이가 1단계 FK 추세(그 반경, SagPrac_TrendAt — 점이 모자라면 같은 점의 1단계 FK)와 이만큼 넘게 다르면
                                                               //   바닥이 아닌 데서 걸린 것으로 보고 뺀다 (0 = 보지 않음). 보정을 켜면 자세가 조금 달라 학습의 PlausFkMm 보다 넓다
                // ── 첫닿음 그럴듯함 (10-15, SagPrac_Plausible) — 바닥은 평평하다: 첫닿음 순간의 엔코더 FK 높이와 명령 높이(보정 끔)는 반경을 따라 매끄럽다 (요와 무관) ──
                //   10-15 실측 1단계 다섯 회차 (닿은 점 61개, 누른 순서대로 그때까지 받아들인 점의 추세 — 반경 가까운 6점의 Theil–Sen): 맞는 점 |FK 차| ≤ 3.2mm,
                //   |명령 차| ≤ 5.0mm. 바닥이 아닌 데서 걸린 점 FK 차 +9.5 ~ +53mm, 명령 차 +11.4 ~ +56mm (r235 FK 56~58 · r190 요 ±20 FK 65 · r150 요 20 FK 88.5,
                //   r250 명령 66.8 은 FK 는 맞고 명령만 +12.4) — 벗어난 점은 문턱의 1.24 배 넘게, 맞는 점은 문턱의 1/1.71 안
                public float PlausFkMm = 5;                    // FK 높이가 잰 점들의 추세와 이만큼 넘게 다르면 (0 = 보지 않음)
                public float PlausTouchMm = 7;                 // 명령 높이 (기준 — 보정 끔) 가 이만큼 넘게 다르면 (0 = 보지 않음). 처짐이 먼 쪽에서 빨리 늘어 FK 보다 넓다
                public float PlausSlope = 0.35f;               // 잰 점이 1~2개일 때 — 가장 가까운 점 값 ± (문턱 + 반경 차 × 이 값) (mm/mm, 명령 높이 기울기 실측 0.13~0.27)
                public float PlausExtrapPerMm = 0.1f;          // 잰 반경 범위 밖으로 넘겨짚을 때 문턱을 반경 1mm 당 이만큼 넓힌다
                public int PlausNear = 6;                      // 추세는 반경이 가까운 점 이만큼으로 맞춘다 (휘어진 추세에도)
                public int PlausMinPts = 3;                    // 추세(Theil–Sen)는 점이 이만큼 이상이고 반경이 둘 이상일 때 — 아니면 가장 가까운 점 띠
                // ── [1. 학습] (10-15) ──
                public int LearnMinData = 6;                   // 새 바닥 첫닿음이 이만큼은 돼야 학습한다 — 첫닿음 학습이 쓸 수 있는 행 6개 이상을 요구한다 (SagComp_FitFloorFc).
                                                               //   학습은 이번 회차 행만 쓰므로 (CSagLearnCfg.Sessions 2) 같은 수. 남은 점을 다 받아도 모자라면 바닥 데이터를 거기서 멈추고 까닭을 알린다
                                                               //   (잰 행은 데이터셋에 덧붙인다)
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
                // ── 수직 눈 확인 (10-13) — 베이스 가까운 자리에서 본다 (먼 자리는 중력 처짐이 펜을 기울여 맞는 로봇도 '수직 아님' 으로 보인다) ──
                public float EyeRadius = 130;                  // 눈 확인 자리 반경 (mm, 50~300) — 기본 = 가장 가까운 실습 반경
                public float EyeYaw = 0;                       // 그 요 (deg)
                public float EyeHoldTolDeg = 0.2f;             // 자동 수직 유지 — 엔코더 공구 기울기(실측 관절 합 − 수직 관절 합)를 이 안으로 손목 명령을 고친다
                public float EyeHoldMaxDeg = 8f;               //   손목 명령에 더하는 각 상한 (측정 누름의 손목 유지와 같다)
                public int EyeHoldIter = 8;                    //   반복 상한 (재기 → 고치기 → 기다리기)
                public int EyeHoldWaitMs = 300;                //   고친 뒤 서보가 자리 잡을 시간
                public float EyeFineMaxDeg = 10f;              // 손 미세 조정 δ 합 상한 (손목 명령, ±)
                public int EyeFineMoveMs = 800;                // 미세 조정 한 번의 관절 보간 시간 (천천히)
                public float EyeFarRadius = 250;               // 관절 모델 예측 기울기를 보일 먼 자리 (가장 먼 실습 반경) — EyeFar 면 그 자리에서도 눈 확인
                public bool EyeFar = false;                    // 먼 자리(EyeFarRadius, 같은 요·같은 높이 규칙)에서도 눈 확인 — 두 점으로 공구 기울기의 자세 항을 맞춘다 (10-15, 기본 끔)
                public float EyeTiltConstTolDeg = 1.0f;        // 두 자리 τ 차가 이 안이면 일정한 기울기 (평균) — 넘으면 자세 항 b12
                public float EyeTiltSameDeg = 0.2f;            // 배운 기울기가 지금 걸린 항과 잰 자리마다 이 안이면 바꾸지 않고 기록만
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
                public bool Rejected;                  // 그럴듯함 확인으로 뺀 점 (10-15 — 잰 값은 비고에)
                internal bool Redone;                  // 그럴듯함 확인으로 이미 한 번 다시 눌렀다
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
                public string EyeTilt = "";            // 그때 걸린 손목 공구 기울기 보정 서명 (CSagComp.WristTiltSig — 빈 값 = 옛 기록 = "none"). 지금과 다르면 다시 묻는다 (10-15)
                public int Advised = -1;               // [1. 학습] 의 적용 권장 (CSagPracResult.ApplyAdvised — 1 권장, 0 권장 아님, −1 기록 없음 = 옛 상태). 그래프·안내·적용 예제가 따른다 (10-15)
                public string AdviceWhy = "";          //   권장하지 않는 까닭 (CSagPracResult.AdviceWhy)
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
                public string EyeAns = "";             // 수직 눈 확인 답 ("" 묻지 않음 / 예 / 아니오 (미세 조정 창 없음) / 기울기 (공구 기울기 보정 제안 — 10-15) / 취소 / 중단 / 생략 — 묻는 곳 없음 / 3D 만)
                // 수직 눈 확인 (10-13) — 엔코더 공구 기울기 = 실측 관절 합 − 정의의 수직 관절 합 (deg, + = 공구 끝이 베이스 쪽, 3D 만 NaN)
                public float EyeTiltBefore = float.NaN, EyeTiltAfter = float.NaN;   // 자동 수직 유지 전·후
                public float EyeHoldDeg = float.NaN;           // 자동 수직 유지가 손목 명령에 더한 각
                public float EyeFarTiltDeg = float.NaN;        // 먼 자리(EyeFarRadius)의 처짐 기울기 예측 (적용 중 관절 모델 — 없으면 NaN)
                public float EyeOffsetDeg = float.NaN;         // [수직임] 때 손 미세 조정 δ (손목 명령)
                // 손목 공구 기울기 보정 제안 (10-15, EyeAns "기울기" — 앱이 묻고 SagRun_TiltApply → SagPrac_EyeRecord. 3D 만은 미리 보기) — 맞춘 결과는 늘 채운다 (예 포함)
                public CSagTiltFit EyeTiltFit = null;
                // 손목 영점 고침 (10-13 ~ 10-14 — 10-15 부터 쓰지 않는다: [수직임] 은 공구 기울기 보정을 배운다. 칸은 옛 앱과의 호환으로 남김)
                public int EyeFixAxis = -1;                    // 손목 ID
                public float EyeFixDeg = float.NaN;            // 고칠 각 = [수직임] 때 엔코더로 본 공구 기울기 (3D 만은 δ) — 같은 틱에서 3D 각이 이만큼 준다
                public int EyeFixOld, EyeFixNew;               // 영점 틱 (앞 → 뒤, SagPrac_WristZeroPlan)
                public float EyeFixApplied = float.NaN;        // 틱으로 반올림된 고침 (deg)
                public string EyeFixWhy = "";                  // 고침을 세우지 못한 까닭 (영점 범위·리밋) 또는 45° 경고
                // [1. 학습] (10-15)
                public List<CSagPracPoint> Data = new List<CSagPracPoint>();   // (c) 바닥 첫닿음 점 (data.csv)
                public int NBaseMiss, NBaseOut, NBaseRedo;     // 기준: 재지 못한 점 (닿지 않음·옮기지 못함 …), 그럴듯함으로 뺀 점, 다시 눌러 받아들인 점
                public int NDataOut, NDataRedo;                // 바닥 데이터: 그럴듯함으로 뺀 점, 다시 눌러 받아들인 점
                public double SpreadPred = double.NaN, ReductionPred = double.NaN, RmsPred = double.NaN;   // 기준 점에서 결과 모델의 예상 (보정 켬) — 흩어짐·줄어든 비율(%)·Ref RMS
                public double SpreadPredBase = double.NaN;     // 같은 점(예상한 점)의 보정 끔 흩어짐 — ReductionPred 의 '전' (SpreadBefore 는 받아들인 기준 점 전부)
                public double PredOffset = double.NaN;         // 예상 첫닿음 − Ref 평균 (mm, 10-15) — 0 이 아니면 Ref(= PaperZ) 에서 펜이 뜨거나(−) 누른다(+)
                public int NPred;                              // 예상한 점 수
                public bool ApplyAdvised;                      // 적용 권장 (예상 흩어짐이 SAGPRAC_APPLY_MIN_RED % 이상 줄고, 예상 첫닿음 − Ref 평균 ≤ SAGPRAC_ADVISE_OFFSET_MM,
                                                               //   Ref RMS ≤ SAGPRAC_ADVISE_RMS_MM, 교차검증 ≤ SAGPRAC_ADVISE_CV_MM) — 앱의 기본 답
                public string AdviceWhy = "";                  // 권장하지 않는 까닭 (권장이면 "")
                public string DatasetSession = "";             // (c) 가 데이터셋에 덧붙인 회차 (session 칸) — 학습이 이 회차 행만 쓴다 (10-15)
                public string LearnSetText = "";               // 학습 설정 한 줄 (sagarm.txt 의 learndataset·learnusehold·learnref 반영 — 10-15)
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
                public bool ApplyOk;                    // 효과가 적용할 만하다 — [1. 학습] 의 예상이면 그 작업의 권장 (진행 상태 advised), 아니면 같은 조건 (같은 점 3개 이상, 흩어짐이
                                                        //   SAGPRAC_APPLY_MIN_RED % 이상 줄어듦, Ref 모델·'후' − Ref 평균·RMS·학습 교차검증 — 10-15, 옛 3단계 잰 값) — 권장
                public string ApplyWhy = "";            // 적용할 만하지 않은 까닭 (결과가 있을 때만 — 앱이 안내 줄에 보인다)
                public bool AfterPredicted;             // '후' 가 잰 값이 아니라 결과 모델의 예상 ([1. 학습] 의 predict.csv — 10-15). false = 옛 3단계 잰 값 (after.csv)
                public double Offset = double.NaN, OffsetRms = double.NaN;   // '후' − 기준선 Ref 평균·RMS (mm, 결과 모델의 Ref 일 때만 — 10-15): 권장은 ±SAGPRAC_ADVISE_OFFSET_MM, ≤ SAGPRAC_ADVISE_RMS_MM
                public string BeforeLabel = "", AfterLabel = "";   // 범례 ("보정 끔 (학습 때 잰 값)" / "보정 켬 (예상)" …)
            }
            /// <summary>적용 권장 조건 — 기준 점에서 흩어짐이 이 비율(%) 이상 줄어야 한다 (예상 또는 옛 3단계 잰 값). 줄지 않았거나 늘어난 보정은 권장하지 않는다</summary>
            public const double SAGPRAC_APPLY_MIN_RED = 10.0;
            /// <summary>적용 권장 조건 — 학습의 묶음 교차검증 오차 (이번 회차 교차검증이 있으면 둘 중 큰 쪽) 가 이 값(mm) 이하 (넘으면 데이터가 고르지 않다 — 10-15 실측 1.9mm)</summary>
            public const double SAGPRAC_ADVISE_CV_MM = 4.0;
            /// <summary>적용 권장 조건 (10-15) — 기준 점의 예상 첫닿음 (보정 켬) − Ref 평균이 이 값(mm) 안. Ref = 앱·예제가 종이 높이로 쓰는 값이라 벗어난 만큼 펜이 뜨거나(−) 누른다.
            /// 10-15 검토: 옛 회차까지 절편 하나로 배운 결과는 −3.3mm (다음 빠르게 학습 흉내, 오늘 기준 4회차 −2.9 ~ −3.3) — 흩어짐은 81% 줄어 '권장' 이었다.
            /// 이번 회차만 + 명령 높이 기준으로 배우면 같은 4회차 −0.1 ~ +0.6mm</summary>
            public const double SAGPRAC_ADVISE_OFFSET_MM = 1.5;
            /// <summary>적용 권장 조건 (10-15) — 기준 점의 예상 첫닿음의 Ref RMS 가 이 값(mm) 이하. 기준 첫닿음 자체가 같은 자리에서 흩어지고 (10-15 오후 4회차 점마다 표준편차
            /// 0.25~1.0mm) 평균 1.5mm 까지 허용하므로 sqrt(1.5² + 1.3²) ≈ 2 — 맞는 학습의 예상은 1.0~1.2mm (같은 4회차)</summary>
            public const double SAGPRAC_ADVISE_RMS_MM = 2.0;

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
                            case "eyetilt": s.EyeTilt = v; break;
                            case "advised": if (int.TryParse(v, NumberStyles.Integer, ci, out n)) s.Advised = n > 0 ? 1 : 0; break;
                            case "advicewhy": s.AdviceWhy = v; break;
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
                    if (!string.IsNullOrEmpty(s.EyeTilt)) sb.Append("eyetilt=").Append(One(s.EyeTilt)).Append("\r\n");   // 손목 공구 기울기 보정 서명 (10-15)
                    if (s.Advised >= 0)                                                  // [1. 학습] 의 적용 권장 — 그래프·안내·적용 예제가 같은 판정을 쓴다 (10-15)
                    {
                        sb.Append("advised=").Append(s.Advised > 0 ? "1" : "0").Append("\r\n");
                        sb.Append("advicewhy=").Append(One(s.AdviceWhy)).Append("\r\n");
                    }
                    string p = Path.Combine(dir, SAGPRAC_STATE), tmp = p + ".tmp";
                    File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
                    if (File.Exists(p)) File.Replace(tmp, p, null); else File.Move(tmp, p);
                    return true;
                }
                catch (Exception ex) { if (L != null) L("★실습 상태를 쓰지 못함: " + SagRun_Msg(ex)); return false; }
            }
            // 새 실습의 상태 — 수직 눈 확인 기록(eyedef·eyemap·eyetime·eyetilt)은 지금 상태에서 넘겨받는다 (로봇·정의·영점·기울기 보정마다 한 번 — 실습 진행과 별개).
            //   10-15: 예전 1단계는 eyetilt 를 빠뜨려 기울기 보정이 걸린 로봇의 눈 확인을 지웠다 — 상태를 새로 쓰는 길은 모두 이것으로
            private static CSagPracState SagPrac_StateKeepEye(string strUrdfDir)
            {
                CSagPracState old = SagPrac_LoadState(strUrdfDir);
                return new CSagPracState { EyeDef = old.EyeDef, EyeMap = old.EyeMap, EyeTime = old.EyeTime, EyeTilt = old.EyeTilt };
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
            // [1. 학습] (10-15) — 한 번 펼치고 한 번 원위치하는 작업 하나
            // ────────────────────────────────────────────────────────────

            /// <summary>[1. 학습] (블로킹 — 작업 스레드). 적용 중 보정·공구 기울기 보정은 처음부터 끝까지 끈다 (옮기기 모델 없음):
            /// (a) 기준 오차 — 가운데 점에서 바닥 찾기 → 점마다 첫닿음 명령 높이 (보정 끔 — 3D 가 믿는 높이, SagPrac_LearnBase) → before.csv
            /// (b) 공중 관절 처짐 (바닥 위 SurveyClearMm, ⑨ 와 같은 표본·학습) → 관절 모델 joint_model.txt (실습 폴더 — 적용 중 파일은 건드리지 않는다)
            /// (c) 바닥 첫닿음 데이터 (방금 만든 관절 모델만 켬 — 첫닿음 모델의 학습 목표가 '관절 모델을 켠 자세의 첫닿음 FK' 다) → 로봇 데이터셋
            ///     <urdf>\floor_dataset_fc.csv 에 덧붙임 (3D 만: 실습 폴더 learn\dataset_3d.csv)
            /// 원위치 뒤 (d) 학습 SagRun_Learn (바탕 = (b) 의 관절 모델, 이번 회차 행만 — 높이 기준은 이번 회차 첫닿음 명령 높이, sagarm.txt 의 learn* 설정 대로 — 10-15)
            /// → 결과 파일 (실물 <urdf>\sagcomp_results, 3D 만 실습 폴더 안) + 보고서,
            /// 기준 점에서 결과 모델의 예상 (보정 켬 — SagPrac_Predict) → predict.csv, 적용 권장 (ApplyAdvised·AdviceWhy — 흩어짐·예상 첫닿음 − Ref·교차검증).
            /// 첫닿음마다 그럴듯함 확인 (SagPrac_Plausible — 기준은 FK·명령 높이, 데이터는 FK 높이가 기준 FK 추세와) — 벗어나면 추세 높이를 가운데로 한 창으로
            /// 한 번 다시, 그래도 벗어나면 뺀다. 몇 점을 못 재도 (닿지 않음·뺌·옮기지 못함 — 연달아 두 번이면 멈춤) 남은 점이 충분하면 (기준 3, 관절 처짐 4,
            /// 바닥 LearnMinData) 이어 가고 수를 알린다. 같은 작업의 기준이 곧 기준이라 옛 2·3단계의 '시작 확인' 은 없다. 수직 눈 확인은 묻지 않는다 (로그 한 줄).
            /// 새 실습 폴더 <urdf>\sagcomp_practice\<yyyyMMdd_HHmmss>[_3d] — before.csv·predict.csv·data.csv·learn_summary.txt·joint_model.txt, 작업 기록 learn\
            /// (run.log·press_trace.csv·data_trace.csv·floor_dataset_run.csv·rest_pose.txt). 진행 상태: 결과가 생기면 step=2 (옛 '2단계 끝' 과 같은 뜻 — 적용 전),
            /// 기준만 재고 멈추지 않은 채 실패하면 step=1. 수직 눈 확인 기록은 넘겨받는다. strMotorMapKey = 지금 모터 매핑 키 (관절 모델·결과 파일의 motormap). 예외 없음.</summary>
            public CSagPracResult SagPrac_LearnJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_LearnJobBody(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, strMotorMapKey, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_LearnJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                        string strMotorMapKey, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 1, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                CSagComp joint = null;
                CSagLearnCfg lset = new CSagLearnCfg();
                bool bBaseOk = false, bDataOk = false;
                List<CSagPracPoint> known = new List<CSagPracPoint>();
                try
                {
                    armCfg = SagDef_Eff(armCfg);                            // 사본 — 수직 관절 합이 자동이면 3D FK 로 잰 값 (못 재면 정의 확인이 거절)
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 폴더를 만들거나 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
                    List<CSagPracPoint> pts = SagPrac_MakePoints(cfg, bPrecise);
                    if (pts.Count < 3) return SagPrac_Refuse(res, "기준 점이 3개 미만 — 설정의 반경 확인", L);
                    res.Points = pts;
                    res.Data = SagPrac_DataPoints(cfg, bPrecise);
                    if (res.Data.Count < Math.Max(1, cfg.LearnMinData))
                        return SagPrac_Refuse(res, string.Format(ci, "바닥 데이터 점이 {0}개 — 학습에 {1}개 이상 필요 (설정의 반경 확인)", res.Data.Count, Math.Max(1, cfg.LearnMinData)), L);
                    lset = SagPrac_LearnSet(strUrdfDir);                     // sagarm.txt 의 learndataset·learnusehold·learnref (10-15 — 예전에는 무시했다)
                    int iMid = SagPrac_MidIndex(pts);
                    DateTime t0 = DateTime.Now;
                    res.FolderDir = SagPrac_NewFolder(strUrdfDir, t0, bDry);
                    res.RunDir = Path.Combine(res.FolderDir, "learn");
                    Directory.CreateDirectory(res.RunDir);
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    L((bDry ? "3D 만 — " : "") + "1. 학습 시작 — " + t0.ToString("yyyy-MM-dd HH:mm:ss", ci) + " (" + (bPrecise ? "정밀" : "빠르게") + ") · " + SagPrac_LearnPlanText(cfg, bPrecise));
                    L("적용 중 보정·공구 기울기 보정은 끝까지 끔 — 기준은 3D 가 믿는 높이 그대로, 바닥 데이터만 이 작업이 만든 관절 모델을 켬");
                    if (armCfg.NoWrist) L(SAGPRAC_EYE_NA + " — 꺾는 손목이 없어 공구 기울기는 자세마다 바뀌고, 첫닿음은 팔꿈치 잔차·명령−FK 로 봅니다");
                    else SagPrac_EyeNote(strUrdfDir, bDry ? null : conn, armCfg, bDry, L);
                    res.DatasetPath = bDry ? Path.Combine(res.RunDir, "dataset_3d.csv") : SagRun_Resolve(strUrdfDir, lset.DatasetPath, SAGRUN_FC_DATASET);
                    res.LearnSetText = SagPrac_LearnSetText(lset, res.DatasetPath, bDry);
                    L(res.LearnSetText);
                    // 3D 만 — 가상 로봇의 닿는 높이 (보정 없음 — 명령, FK)
                    var sim = new Dictionary<int, float[]>();
                    if (bDry)
                    {
                        foreach (CSagPracPoint p in pts)
                        {
                            float z, fk; float[] qi, qa;
                            if (SagPrac_SimTouch(kin, armCfg, cfg, p.X, p.Y, p.Pitch, null, out z, out fk, out qi, out qa)) sim[p.Idx] = new float[] { z, fk };
                        }
                        if (!sim.ContainsKey(pts[iMid].Idx)) return SagPrac_Refuse(res, "가상 로봇 계산 실패 (IK) — 반경 확인", L);
                        L(string.Format(ci, "3D 만 — 가상 로봇: 바닥 FK {0:0.#}mm, 관절 처짐 c {1}, 끝 휨 {2:0.#}mm (실제 로봇과 다름 — 미리 보기)", cfg.SimFloorZ,
                            string.Join("/", Array.ConvertAll(cfg.SimJc, v => v.ToString("0.##", ci))), cfg.SimExtMm));
                    }
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    SagArm_SetMoveModel(arm, null, L);                       // 옮기기도 보정 없이 (기준 창·옮기는 높이는 보정 없는 닿는 높이로 잡는다)
                    var run = SagPrac_NewRun(arm, cfg, res.RunDir, L, stop);
                    CSagPracPoint mid = pts[iMid];
                    if (!SagArm_Unfold(arm, mid.X, mid.Y, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else
                    {
                        // ── (a) 기준 오차 ──
                        L(string.Format(ci, "진행 (a) 기준 오차 — 점 {0}개, 보정 끔", pts.Count));
                        known = SagPrac_LearnBase(run, pts, iMid, sim, res);
                        bBaseOk = !run.Stopped && run.Why.Length == 0 && known.Count >= 3;
                        if (!run.Stopped && run.Why.Length == 0 && known.Count < 3)
                            run.Why = string.Format(ci, "기준 점이 {0}개만 재짐 (3개 이상 필요 — 뺀 점 {1}, 재지 못한 점 {2})", known.Count, res.NBaseOut, pts.Count - known.Count - res.NBaseOut);
                        // ── (b) 공중 관절 처짐 ──
                        float hMax = float.MinValue; foreach (CSagPracPoint p in known) hMax = Math.Max(hMax, p.Touch);
                        float zS = bBaseOk ? Math.Min(armCfg.SafeZ, hMax + cfg.SurveyClearMm) : float.NaN;
                        if (bBaseOk && zS < hMax + cfg.SurveyClearMinMm)
                            run.Why = string.Format(ci, "관절 처짐 측정 높이 z={0:0.#} 가 바닥(기준 닿는 높이 최고 {1:0.#}) 위 {2:0} 이 안 됨 — 옮기는 높이(safez) 확인", zS, hMax, cfg.SurveyClearMinMm);
                        if (bBaseOk && !run.Stopped && run.Why.Length == 0)
                        {
                            List<float[]> svy = SagPrac_SurveyPoints(armCfg, cfg, bPrecise, zS, hMax, L);
                            if (svy.Count < 4) run.Why = "관절 처짐 측정 점이 4개 미만 (IK·옮기는 높이)";
                            else
                            {
                                L(string.Format(ci, "진행 (b) 공중 관절 처짐 — {0}점, 높이 z={1:0.#} (바닥 위 {2:0})", svy.Count, zS, zS - hMax));
                                if (!bDry && !float.IsNaN(arm.CurZLow)) arm.CurZLow = Math.Min(arm.CurZLow, zS);   // 측정 높이는 바닥 위 SurveyClearMinMm 이상 (위에서 확인)
                                List<CSagSample> samples = bDry ? SagPrac_SimSurvey(kin, armCfg, cfg, svy) : SagPrac_Survey(run, svy);
                                run.LastKey = null;                               // 다음 누름은 들어 옮긴다 (공중 측정 자리에서)
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
                                        else { joint = chk; joint.FloorMapEnabled = false; joint.Predictive = false; L("관절 모델: " + res.JointPath); }
                                    }
                                }
                            }
                        }
                        // ── (c) 바닥 첫닿음 데이터 (관절 모델 켬 — 1차 보정: 예측 보정 끔, 옛 회차 데이터셋의 첫닿음 명령 높이와 같은 조건 10-16) ──
                        if (joint != null && !run.Stopped && run.Why.Length == 0)
                        {
                            L(string.Format(ci, "진행 (c) 바닥 첫닿음 데이터 — {0}점, 방금 만든 관절 모델 켬", res.Data.Count));
                            SagArm_SetMoveModel(arm, joint, L);
                            if (bDry) bDataOk = SagPrac_SimData(kin, armCfg, cfg, joint, res.Data, known, res.DatasetPath, res, L);
                            else
                            {
                                SagPrac_FloorData(run, joint, res.Data, known, res.DatasetPath, res);
                                bDataOk = res.DatasetRowsAdded >= Math.Max(1, cfg.LearnMinData);
                                if (!bDataOk && !run.Stopped && run.Why.Length == 0)
                                    run.Why = string.Format(ci, "새 바닥 첫닿음이 {0}개 (학습에 {1}개 이상 필요 — 뺀 점 {2})", res.DatasetRowsAdded, Math.Max(1, cfg.LearnMinData), res.NDataOut);
                            }
                        }
                    }
                    res.Stopped = res.Stopped || run.Stopped;
                    if (run.Why.Length > 0) res.Why = SagRun_WhyJoin(res.Why, run.Why);
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                if (!res.Ran) return res;
                // ── (d) 학습 (원위치 뒤 — 로봇은 쉬고 있다) → 예상 ──
                List<CSagPracPoint> pred = null;
                try
                {
                    if (bDataOk && joint != null && !res.Stopped && res.Why.Length == 0 && !SagRun_Stopped(stop))
                    {
                        L("진행 (d) 학습 — 바탕 관절 모델 " + Path.GetFileName(res.JointPath) + ", 데이터 " + Path.GetFileName(res.DatasetPath) + " 의 이번 회차 " + res.DatasetSession);
                        // 이번 바닥에 맞춘 학습 (10-15 검토) — 데이터셋에는 바닥 높이가 다른 옛 회차가 섞여 있다 (오늘 바닥보다 +1.0 ~ +3.0mm): 이번 회차 행만 (Sessions 2 —
                        //   실측 교차검증으로 고름, CSagLearnCfg.Sessions), 높이 기준은 이번 회차의 첫닿음 명령 높이 (CmdLevel). 손목 유지 켬 행·기준 높이 고정은 sagarm.txt 대로
                        //   (3D 만은 가상 로봇 바닥이라 learnref 를 쓰지 않는다)
                        var lc = new CSagLearnCfg { DatasetPath = res.DatasetPath, BaseModelPath = res.JointPath, UseHold = lset.UseHold, RefMm = bDry ? float.NaN : lset.RefMm,
                                                    CurSession = res.DatasetSession, Sessions = 2, PlausMm = cfg.PlausFkMm, CmdLevel = true };
                        string outDir = bDry ? res.FolderDir : strUrdfDir;            // 3D 만은 실습 폴더 안 sagcomp_results (결과 목록·적용에 섞지 않는다)
                        res.Learn = SagRun_Learn(lc, outDir, strMotorMapKey, L, stop);
                        if (res.Learn != null && res.Learn.Ok) res.ResultPath = res.Learn.ModelPath;
                        else if (res.Learn != null && res.Learn.Stopped) res.Stopped = true;
                        else res.Why = SagRun_WhyJoin(res.Why, "학습 실패 — " + (res.Learn != null ? res.Learn.Why : ""));
                    }
                    if (!string.IsNullOrEmpty(res.ResultPath))
                    {
                        CSagComp mRes = SagRun_LoadQuiet(res.ResultPath);
                        C3d kin = SagRun_Kin();
                        if (mRes != null && mRes.HasJoint && kin != null)
                        {
                            mRes.Enabled = true; mRes.FloorMapEnabled = true; if (mRes.HasFc) mRes.Fc.Enabled = true;
                            res.Ref = mRes.HasFc ? mRes.Fc.Ref : double.NaN;
                            pred = SagPrac_Predict(kin, SagDef_Eff(armCfg), cfg == null ? new CSagPracCfg() : cfg, mRes, known, bDry, res);
                        }
                    }
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★학습 오류: " + SagRun_Msg(ex)); L("★학습 오류: " + SagRun_Msg(ex)); }
                SagPrac_FinishLearn(res, strUrdfDir, bPrecise, armCfg, known, pred, bBaseOk, L);
                return res;
            }

            // (a) 기준 — 바닥 찾기 (실물 — 3D 만은 가상 바닥) → 가운데 점 (그 높이로 창, 다시 재지 못하면 바닥 찾기 값) → 나머지 점 (잰 점으로 넘겨짚은 창, SagPrac_Order).
            //   첫닿음마다 그럴듯함 (SagPrac_PlausPress — 기준이라 명령 높이도), 끝에 받아들인 점을 하나씩 빼고 다시 본다 (SagPrac_PlausSweep — 벗어나면 한 번 다시 누름).
            //   옮기지 못한 점은 그 점만 뺀다 (SagPrac_SkipFail). sim = 3D 만 가상 로봇 (명령, FK). 반환 = 받아들인 점. 멈춤·실패는 run.Stopped·run.Why, 수는 res.NBase*
            private List<CSagPracPoint> SagPrac_LearnBase(CSagPracRun run, List<CSagPracPoint> pts, int iMid, Dictionary<int, float[]> sim, CSagPracResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagPracCfg cfg = run.Cfg;
                bool bDry = run.Arm.IsDry;
                var known = new List<CSagPracPoint>();
                CSagPracPoint mid = pts[iMid];
                int nFailRow = 0, nRedo = 0, nOut = 0;
                float est, estFk = float.NaN;
                if (bDry)
                {
                    est = sim[mid.Idx][0]; estFk = sim[mid.Idx][1];
                    run.L(string.Format(ci, "진행 바닥 찾기 — 3D 만: 가상 바닥에 닿는 높이 z={0:F1}", est));
                }
                else
                {
                    float top = Math.Min(cfg.FindStartZ, run.Arm.Cfg.SafeZ - 10);
                    run.L(string.Format(ci, "진행 바닥 찾기 — 가운데 점 r={0:0.#} 에서 z={1:0.#} 부터 천천히({2:0.#}mm/s) 내려가며 첫닿음에서 멈춤", mid.R, top, cfg.DescentMmPerSec));
                    est = SagPrac_FindFloor(run, mid, top, out estFk);
                    if (run.Stopped || run.Why.Length > 0) return known;
                    if (float.IsNaN(est))
                    {
                        run.Why = "바닥을 찾지 못함 — " + (mid.Note.Length > 0 ? mid.Note : "한도까지 첫닿음 없음") + " (바닥이 너무 낮거나 팔이 닿지 않는 자리)";
                        return known;
                    }
                    run.L(string.Format(ci, "바닥 찾음: 첫닿음 명령 z={0:F1} (FK {1:F1}) → 그 높이로 창(위 {2:0}, 아래 {3:0})을 잡아 가운데 점을 잼", est, estFk, cfg.AboveMm, cfg.BelowMm));
                }
                res.FloorFoundZ = est;
                mid.Est = est; mid.Note = "";
                ESagPracPress o = SagPrac_PressAt(run, mid, est, est, 0, null, false, bDry ? est : float.NaN);
                if (o == ESagPracPress.Stop) return known;
                if (o == ESagPracPress.Fail && !SagPrac_SkipFail(run, mid, ref nFailRow)) return known;
                if (bDry && !float.IsNaN(mid.Touch)) mid.Fk = estFk;
                if (float.IsNaN(mid.Touch) && !bDry)
                {
                    string w = mid.Note;
                    mid.Touch = est; mid.Fk = estFk; mid.Kind = "F"; mid.Note = "바닥 찾기 값" + (w.Length > 0 ? " (창으로 다시 재지 못함 — " + w + ")" : "");
                    run.L(string.Format(ci, "  r={0:0.#} 요 {1:0.#}: 창으로 다시 재지 못해 바닥 찾기 값 {2:F1} (FK {3:F1}) 을 씀", mid.R, mid.Yaw, est, estFk));
                }
                if (float.IsNaN(mid.Touch)) { run.Why = "가운데 점에서 첫닿음을 재지 못함 (" + mid.Note + ")"; return known; }
                known.Add(mid);
                run.L(string.Format(ci, "진행 기준 1/{0} — r={1:0.#} 요 {2:0.#}: 닿은 높이 {3:F1}", pts.Count, mid.R, mid.Yaw, mid.Touch));
                List<CSagPracPoint> order = SagPrac_Order(pts, iMid);
                int nDone = 1;
                foreach (CSagPracPoint p in order)
                {
                    if (ReferenceEquals(p, mid)) continue;
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "점 사이"); break; }
                    float u; float estP = SagPrac_Estimate(known, p.R, p.Yaw, cfg.UnknownMm, out u);
                    if (float.IsNaN(estP)) { p.Note = "예상 높이 없음 — 뺌"; continue; }
                    p.Est = estP;
                    float[] sv = null;
                    if (bDry && !sim.TryGetValue(p.Idx, out sv)) { p.Note = "가상 로봇 계산 실패 — 뺌"; continue; }
                    o = SagPrac_PressAt(run, p, estP, estP, u, null, false, bDry ? sv[0] : float.NaN);
                    if (o == ESagPracPress.Stop) break;
                    if (o == ESagPracPress.Fail) { if (SagPrac_SkipFail(run, p, ref nFailRow)) continue; break; }
                    if (o == ESagPracPress.Ok) nFailRow = 0;
                    if (bDry && !float.IsNaN(p.Touch)) p.Fk = sv[1];
                    nDone++;
                    bool bOk = false;
                    if (!float.IsNaN(p.Touch))
                    {
                        CFloorPlanRow rr;
                        o = SagPrac_PlausPress(run, p, known, true, float.NaN, null, false, ref nRedo, ref nOut, out bOk, out rr);
                        if (o == ESagPracPress.Stop) break;
                        if (o == ESagPracPress.Fail && !SagPrac_SkipFail(run, p, ref nFailRow)) break;
                    }
                    if (bOk) known.Add(p);
                    run.L(string.Format(ci, "진행 기준 {0}/{1} — r={2:0.#} 요 {3:0.#}: {4}", nDone, pts.Count, p.R, p.Yaw,
                        float.IsNaN(p.Touch) ? (p.Rejected ? "뺌 (바닥이 아닌 데서 걸림)" : "닿지 않음 (" + p.Note + ")") : string.Format(ci, "닿은 높이 {0:F1} (예상 {1:F1})", p.Touch, estP)));
                }
                // 끝 확인 — 받아들인 점을 하나씩 빼고 나머지 추세로 다시 (처음 몇 점은 이웃이 적어 넓게 봤다)
                if (!run.Stopped && run.Why.Length == 0)
                {
                    foreach (KeyValuePair<CSagPracPoint, CSagPracPlaus> kv in SagPrac_PlausSweep(known, cfg, true))
                    {
                        CSagPracPoint p = kv.Key;
                        known.Remove(p);
                        run.L(string.Format(ci, "  ★끝 확인 — r={0:0.#} 요 {1:0.#}: 나머지 점의 추세에서 벗어남 ({2})", p.R, p.Yaw, kv.Value.Why));
                        if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "끝 확인"); SagPrac_Reject(p, "끝 확인 — " + kv.Value.Why, run.L); nOut++; break; }
                        bool bOk; CFloorPlanRow rr;
                        o = SagPrac_PlausPress(run, p, known, true, float.NaN, null, false, ref nRedo, ref nOut, out bOk, out rr);
                        if (bOk) known.Add(p);
                        if (o == ESagPracPress.Stop) break;
                        if (o == ESagPracPress.Fail && !SagPrac_SkipFail(run, p, ref nFailRow)) break;
                    }
                }
                res.NBaseRedo = nRedo; res.NBaseOut = nOut;
                return known;
            }

            // 기준 점에서 결과 모델의 예상 (보정 켬) — 받아들인 기준 점마다: 실물은 결과 모델이 그 자세에서 끝을 올리는 양 (SagPrac_Lift — 관절 + 바닥 보정) 만큼
            //   첫닿음 명령 높이가 내려간다 (예상 z = 기준 − z 에서 올리는 양 — 두 번 되풀이, 내려가는 방향의 데드밴드 항까지. 보정이 맞으면 모두 Ref 근처),
            //   3D 만은 가상 로봇을 결과 모델로 다시 계산 (SagPrac_SimTouch). res 의 SpreadPred·ReductionPred·RmsPred·NPred, 반환 = 점 목록 (기준 순서, Touch = 예상, Est = 기준)
            private List<CSagPracPoint> SagPrac_Predict(C3d kin, CSagArmCfg armCfg, CSagPracCfg cfg, CSagComp mRes, List<CSagPracPoint> known, bool bDry, CSagPracResult res)
            {
                var outL = new List<CSagPracPoint>();
                var vb = new List<double>(); var va = new List<double>();
                foreach (CSagPracPoint b in res.Points)
                {
                    var q = new CSagPracPoint { Idx = b.Idx, R = b.R, Yaw = b.Yaw, X = b.X, Y = b.Y, Pitch = b.Pitch, Est = b.Touch };
                    outL.Add(q);
                    if (float.IsNaN(b.Touch) || !known.Contains(b)) { q.Note = "기준 없음"; continue; }
                    float z = float.NaN, fk = float.NaN;
                    if (bDry)
                    {
                        float[] qi, qa;
                        if (!SagPrac_SimTouch(kin, armCfg, cfg, b.X, b.Y, b.Pitch, mRes, out z, out fk, out qi, out qa)) z = float.NaN;
                    }
                    else
                    {
                        // z = 기준 − (닿는 자리 z 에서 올리는 양) — 두 번 되풀이 (올리는 양은 높이에 따라 천천히 바뀐다), 내려가는 방향 (데드밴드 항까지)
                        z = b.Touch - SagPrac_Lift(kin, armCfg, mRes, b.X, b.Y, b.Touch, b.Pitch, true);
                        z = b.Touch - SagPrac_Lift(kin, armCfg, mRes, b.X, b.Y, z, b.Pitch, true);
                    }
                    if (float.IsNaN(z) || float.IsInfinity(z)) { q.Note = "예상 못 함"; continue; }
                    q.Touch = z; q.Fk = fk; q.Kind = "P";
                    vb.Add(b.Touch); va.Add(z);
                }
                res.NPred = va.Count;
                if (va.Count >= 2)
                {
                    double rf = double.IsNaN(res.Ref) ? SagPrac_Mean(va) : res.Ref;
                    res.SpreadPred = SagPrac_Spread(va);
                    double sb = SagPrac_Spread(vb);
                    res.SpreadPredBase = sb;                                       // '전' 도 같은 점으로 (10-15)
                    res.ReductionPred = sb > 1e-6 ? (1.0 - res.SpreadPred / sb) * 100.0 : double.NaN;
                    res.RmsPred = SagPrac_Rms(va, rf);
                    res.PredOffset = double.IsNaN(res.Ref) ? double.NaN : SagPrac_Mean(va) - res.Ref;   // 펜이 Ref 에서 뜨는(−)·누르는(+) 양 (10-15)
                }
                return outL;
            }

            // [1. 학습] 끝 — before.csv·predict.csv·data.csv·learn_summary.txt, 적용 권장, 진행 상태 (결과가 생기면 step=2, 기준만 됐으면 step=1 — 멈췄으면 그대로)
            private void SagPrac_FinishLearn(CSagPracResult res, string strUrdfDir, bool bPrecise, CSagArmCfg armCfg, List<CSagPracPoint> known, List<CSagPracPoint> pred,
                                             bool bBaseOk, CSim2Real.DLog L)
            {
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    var v = new List<double>();
                    foreach (CSagPracPoint p in known) v.Add(p.Touch);
                    res.NTouched = v.Count;
                    res.NBaseMiss = Math.Max(0, res.Points.Count - res.NTouched - res.NBaseOut);
                    if (v.Count >= 2) { res.SpreadBefore = SagPrac_Spread(v); res.RmsBefore = SagPrac_Rms(v, double.IsNaN(res.Ref) ? SagPrac_Mean(v) : res.Ref); }
                    res.CsvPath = Path.Combine(res.FolderDir, "before.csv");
                    SagRun_WriteText(res.CsvPath, SagPrac_PointsCsv(res.Points));
                    if (pred != null) SagRun_WriteText(Path.Combine(res.FolderDir, "predict.csv"), SagPrac_PointsCsv(pred));
                    if (res.Data != null && res.Data.Count > 0 && !res.Dry) SagRun_WriteText(Path.Combine(res.FolderDir, "data.csv"), SagPrac_PointsCsv(res.Data));
                    res.Ok = !res.Stopped && res.Why.Length == 0 && !string.IsNullOrEmpty(res.ResultPath) && File.Exists(res.ResultPath);
                    if (!res.Stopped && res.Why.Length == 0 && !res.Ok) res.Why = "결과 파일이 생기지 않음";
                    // 적용 권장 — 예상 흩어짐이 충분히 줄고, 예상 첫닿음이 Ref 에 오고 (Ref = 종이 높이 — 벗어나면 펜이 뜨거나 누른다, 10-15 검토), 교차검증이 작아야
                    CSagLearnResult lr = res.Learn;
                    if (res.Ok)
                    {
                        double cvA = SagPrac_AdviceCv(lr);
                        if (res.NPred < 3) res.AdviceWhy = string.Format(ci, "예상할 수 있는 기준 점이 {0}개 — 3개 이상이어야 권장", res.NPred);
                        else if (double.IsNaN(res.ReductionPred) || res.ReductionPred < SAGPRAC_APPLY_MIN_RED)
                            res.AdviceWhy = string.Format(ci, "기준 점에서 예상 흩어짐이 {0} ({1}) — {2:0}% 이상 줄어야 권장", (!double.IsNaN(res.ReductionPred) && res.ReductionPred < 0) ? "늘어남" : "충분히 줄지 않음",
                                                          SagPrac_SpreadText(res.SpreadPredBase, res.SpreadPred, res.ReductionPred), SAGPRAC_APPLY_MIN_RED);
                        else if (double.IsNaN(res.PredOffset) || Math.Abs(res.PredOffset) > SAGPRAC_ADVISE_OFFSET_MM)
                            res.AdviceWhy = string.Format(ci, "기준 점의 예상 첫닿음이 Ref 와 평균 {0:+0.0;-0.0}mm — ±{1:0.0}mm 안이어야 권장 (Ref 높이로 그리면 펜이 {2})",
                                                          res.PredOffset, SAGPRAC_ADVISE_OFFSET_MM, double.IsNaN(res.PredOffset) ? "어디 닿을지 모름" : (res.PredOffset < 0 ? "그만큼 뜬다" : "그만큼 누른다"));
                        else if (double.IsNaN(res.RmsPred) || res.RmsPred > SAGPRAC_ADVISE_RMS_MM)
                            res.AdviceWhy = string.Format(ci, "기준 점의 예상 첫닿음이 Ref 에서 RMS {0:F1}mm — {1:0.0}mm 이하여야 권장 (자리마다 펜 높이가 고르지 않다)", res.RmsPred, SAGPRAC_ADVISE_RMS_MM);
                        else if (!double.IsNaN(cvA) && cvA > SAGPRAC_ADVISE_CV_MM)
                            res.AdviceWhy = string.Format(ci, "학습 교차검증 오차 {0:F1}mm 가 {1:0.#}mm 를 넘음 — 바닥 데이터가 고르지 않음 (정밀로 다시 학습 권장)", cvA, SAGPRAC_ADVISE_CV_MM);
                        res.ApplyAdvised = res.AdviceWhy.Length == 0;
                    }
                    string strBase = res.NTouched >= 2
                        ? string.Format(ci, "기준 오차: 흩어짐 {0:F1}mm ({1}점, {2:F1}~{3:F1}){4}{5}{6}", res.SpreadBefore, res.NTouched, SagPrac_Min(v), SagPrac_Max(v),
                                        res.NBaseOut > 0 ? string.Format(ci, " · 뺀 점 {0}", res.NBaseOut) : "", res.NBaseMiss > 0 ? string.Format(ci, " · 재지 못한 점 {0}", res.NBaseMiss) : "",
                                        float.IsNaN(res.FloorFoundZ) ? "" : string.Format(ci, " · 바닥 찾기 z={0:F1}", res.FloorFoundZ))
                        : "기준 오차: 잴 수 없음 (닿은 점 부족)";
                    string strLearn = string.Format(ci, "학습 데이터: 관절 처짐 {0}점 · 바닥 {1}점 (데이터셋 +{2}행{3})", res.SurveySamples, res.DataRows, res.DatasetRowsAdded,
                                                    res.NDataOut > 0 ? string.Format(ci, ", 뺀 점 {0}", res.NDataOut) : "") +
                                      (res.Ok ? " → " + Path.GetFileNameWithoutExtension(res.ResultPath) + ((lr != null && !double.IsNaN(lr.CvRmse)) ? string.Format(ci, " · CV {0:F2}mm", lr.CvRmse) : "") +
                                                ((lr != null && !double.IsNaN(lr.CvCur)) ? string.Format(ci, " · 이번 회차 CV {0:F2}mm", lr.CvCur) : "") : "");
                    string strPred = double.IsNaN(res.SpreadPred) ? "" : "예상 (보정 켬): " + SagPrac_SpreadText(res.SpreadPredBase, res.SpreadPred, res.ReductionPred) +
                                     (double.IsNaN(res.PredOffset) ? "" : string.Format(ci, " · 첫닿음 − Ref 평균 {0:+0.0;-0.0}mm", res.PredOffset));
                    res.Line = res.Ok ? (res.Dry ? "3D 만 · " : "") + "학습 끝 — " + (strPred.Length > 0 ? strPred : strBase) + " · " + Path.GetFileNameWithoutExtension(res.ResultPath)
                                     : (res.Stopped ? "1. 학습 중단" : "1. 학습 실패 — " + res.Why);
                    var sb = new StringBuilder();
                    sb.AppendLine(res.Dry ? "=== 처짐 보정 실습 1. 학습 (3D 만 — 가상 로봇) ===" : "=== 처짐 보정 실습 1. 학습 ===");
                    sb.AppendLine("실습 폴더: " + res.FolderDir);
                    sb.AppendLine("보정: 적용 중 보정·공구 기울기 보정 끔 — 기준은 3D 가 믿는 높이, 바닥 데이터만 이 작업의 관절 모델을 켬");
                    sb.AppendLine("첫닿음 그럴듯함: FK·명령 높이가 잰 점들의 추세(반경)에서 벗어나면 추세 높이 창으로 한 번 다시, 그래도 벗어나면 뺌");
                    sb.AppendLine("(a) " + strBase + (res.NBaseRedo > 0 ? string.Format(ci, " · 다시 눌러 받아들인 점 {0}", res.NBaseRedo) : ""));
                    sb.AppendLine("     r     요   예상     닿은 높이   FK      판정  비고");
                    foreach (CSagPracPoint p in res.Points)
                        sb.AppendLine(string.Format(ci, "  {0,5:0.#} {1,5:0.#}  {2,6}  {3,9}  {4,6}  {5,-4}  {6}", p.R, p.Yaw, SagPrac_V(p.Est), SagPrac_V(p.Touch), SagPrac_V(p.Fk),
                            p.Kind.Length > 0 ? p.Kind : "—", p.Note));
                    sb.AppendLine(string.Format(ci, "(b) 공중 관절 처짐: {0}점 → {1}", res.SurveySamples, string.IsNullOrEmpty(res.JointPath) ? "(없음)" : res.JointPath));
                    if (res.JointLine.Length > 0) sb.AppendLine("    " + res.JointLine.Replace("\r\n", "\r\n    "));
                    sb.AppendLine(string.Format(ci, "(c) 바닥 첫닿음: {0}점 → 데이터셋 +{1}행 ({2}){3}{4}", res.DataRows, res.DatasetRowsAdded, res.DatasetPath ?? "",
                        res.NDataOut > 0 ? string.Format(ci, " · 뺀 점 {0}", res.NDataOut) : "", res.NDataRedo > 0 ? string.Format(ci, " · 다시 눌러 받아들인 점 {0}", res.NDataRedo) : ""));
                    foreach (CSagPracPoint p in res.Data ?? new List<CSagPracPoint>())
                        if (p.Rejected || (p.Note.Length > 0 && float.IsNaN(p.Touch))) sb.AppendLine(string.Format(ci, "    r{0:0.#} 요 {1:0.#}: {2}", p.R, p.Yaw, p.Note));
                    sb.AppendLine("(d) 학습: " + (lr == null ? "하지 않음" : (lr.Ok ? lr.Line : "실패 — " + lr.Why)));
                    if (res.LearnSetText.Length > 0) sb.AppendLine("    " + res.LearnSetText);
                    if (res.DatasetSession.Length > 0) sb.AppendLine("    이번 회차 " + res.DatasetSession + " 의 행만 학습 (옛 회차는 바닥 높이가 달라 쓰지 않음 — 데이터셋에는 남는다)");
                    if (lr != null && lr.Ok) sb.AppendLine("    결과 파일: " + lr.ModelPath + "\r\n    보고서: " + lr.ReportPath);
                    if (strPred.Length > 0)
                    {
                        sb.AppendLine(strPred + string.Format(ci, " — 기준 점 {0}개에서 결과 모델로 계산 ({1})", res.NPred, res.Dry ? "가상 로봇" : "보정 켬 첫닿음 = 기준 − 결과 모델이 끝을 올리는 양"));
                        if (!double.IsNaN(res.Ref)) sb.AppendLine(string.Format(ci, "기준선 Ref {0:F1}mm — Ref RMS {1:F2} → 예상 {2:F2}mm, 예상 첫닿음 − Ref 평균 {3:+0.00;-0.00}mm", res.Ref, res.RmsBefore, res.RmsPred, res.PredOffset));
                    }
                    if (res.Ok) sb.AppendLine(res.ApplyAdvised ? string.Format(ci, "적용 권장 — 예상 흩어짐이 충분히 줄고 예상 첫닿음이 Ref 근처 (평균 {0:+0.0;-0.0}mm, RMS {1:F1}mm)", res.PredOffset, res.RmsPred)
                                                           : "★적용 권장하지 않음 — " + res.AdviceWhy);
                    if (res.UncheckedAxes.Length > 0) sb.AppendLine("★방향 확인 안 된 축: ID " + SagRun_Ints(res.UncheckedAxes));
                    if (!res.Dry) sb.AppendLine("원위치: " + (res.RestOk ? "휴지 자세 안" : "★휴지 자세와 차가 큼 또는 확인 못 함"));
                    if (res.Why.Length > 0) sb.AppendLine("★" + res.Why);
                    sb.AppendLine(res.Line);
                    res.Report = sb.ToString();
                    res.ReportPath = Path.Combine(res.FolderDir, "learn_summary.txt");
                    SagRun_WriteText(res.ReportPath, res.Report);
                    if (res.Ok || (bBaseOk && !res.Stopped))
                    {
                        CSagPracState st = SagPrac_StateKeepEye(strUrdfDir);          // 수직 눈 확인 기록은 넘겨받는다 (eyetilt 까지)
                        st.Folder = Path.GetFileName(res.FolderDir); st.Dry = res.Dry; st.Precise = bPrecise; st.Def = SagDef_Sig(armCfg); st.Applied = false;
                        st.Step = res.Ok ? 2 : 1;
                        st.Line1 = strBase;
                        if (res.Ok) { st.JointPath = res.JointPath ?? ""; st.ResultPath = res.ResultPath; st.Line2 = strLearn; st.Line3 = strPred; }
                        // 적용 권장 — 이 판정 하나를 그래프·안내·적용 예제 모델 고르기가 따른다 (10-15 검토: 그래프 쪽 규칙에 교차검증·Ref 모델 조건이 없어
                        //   '권장하지 않음' 이 뜬 학습 결과도 [적용 예제] 가 실물용 (learned) 으로 넘겼다)
                        if (res.Ok) { st.Advised = res.ApplyAdvised ? 1 : 0; st.AdviceWhy = res.ApplyAdvised ? "" : res.AdviceWhy; }
                        SagPrac_SaveState(strUrdfDir, st, L);
                    }
                    L(res.Line);
                    if (res.Ok && !res.ApplyAdvised) L("★적용 권장하지 않음 — " + res.AdviceWhy);
                }
                catch (Exception ex) { if (L != null) L("★학습 정리 오류: " + SagRun_Msg(ex)); }
            }
            // 적용 권장의 교차검증 값 — 묶음 교차검증과 이번 회차 교차검증 중 큰 쪽 (수가 아닌 쪽은 빼고, 둘 다 없으면 NaN)
            private static double SagPrac_AdviceCv(CSagLearnResult lr)
            {
                if (lr == null) return double.NaN;
                double a = lr.CvRmse, b = lr.CvCur;
                if (double.IsNaN(a) || double.IsInfinity(a)) return (double.IsNaN(b) || double.IsInfinity(b)) ? double.NaN : b;
                if (double.IsNaN(b) || double.IsInfinity(b)) return a;
                return Math.Max(a, b);
            }
            // 결과 파일 머리 주석의 학습 교차검증 (mm) — '# 첫닿음 모델 학습: … 묶음 교차검증 0.929mm' 와 '# 이번 바닥: … 이번 회차 교차검증 0.83mm' 중 큰 쪽
            //   (SagPrac_AdviceCv 와 같은 값 — 학습이 적는 줄). 진행 상태에 [1. 학습] 의 권장 기록이 없을 때 그래프 권장이 쓴다 (10-15). 없거나 못 읽으면 NaN
            private static double SagPrac_ResultCv(string strPath)
            {
                double cv = double.NaN;
                try
                {
                    if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath)) return cv;
                    const string KEY = "교차검증 ";
                    foreach (string raw in File.ReadAllLines(strPath, Encoding.UTF8))
                    {
                        string ln = raw.Trim();
                        if (!ln.StartsWith("#")) continue;
                        for (int i = ln.IndexOf(KEY, StringComparison.Ordinal); i >= 0; i = ln.IndexOf(KEY, i + KEY.Length, StringComparison.Ordinal))
                        {
                            int a = i + KEY.Length, b = ln.IndexOf("mm", a, StringComparison.Ordinal);
                            double v;
                            if (b > a && double.TryParse(ln.Substring(a, b - a).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v) && !double.IsInfinity(v))
                                cv = double.IsNaN(cv) ? v : Math.Max(cv, v);
                        }
                    }
                }
                catch { }
                return cv;
            }
            // 학습 설정 — urdf 폴더 sagarm.txt 의 learndataset·learnusehold·learnref (있으면 읽기만, 없으면 기본값 — 파일을 만들지 않는다. 10-15: [1. 학습] 도 이 값을 따른다)
            private static CSagLearnCfg SagPrac_LearnSet(string strUrdfDir)
            {
                try
                {
                    if (!string.IsNullOrEmpty(strUrdfDir) && File.Exists(Path.Combine(strUrdfDir, CSagArmSetup.FILE))) return CSagArmSetup.Load(strUrdfDir).Learn;
                }
                catch { }
                return new CSagLearnCfg();
            }
            // 학습 설정 한 줄 (로그·보고서)
            private static string SagPrac_LearnSetText(CSagLearnCfg lset, string strDataset, bool bDry)
            {
                var ci = CultureInfo.InvariantCulture;
                if (lset == null) lset = new CSagLearnCfg();
                return "학습 설정 (sagarm.txt): 데이터셋 " + (bDry ? "실습 폴더 안 (3D 만)" : strDataset) +
                       " · 손목 유지 켬 행 " + (lset.UseHold ? "넣음" : "뺌") +
                       " · 기준 Ref " + ((bDry || float.IsNaN(lset.RefMm)) ? "학습 평균" + (bDry && !float.IsNaN(lset.RefMm) ? " (3D 만은 learnref 를 쓰지 않음)" : "") : string.Format(ci, "고정 {0:0.###}mm (learnref)", lset.RefMm)) +
                       " · 이번 회차 행만, 높이 기준 = 이번 회차 첫닿음 명령 높이";
            }

            /// <summary>[1. 학습] 끝에 앱이 보일 적용 묻기 글 (10-15) — 예상 흩어짐 (같은 기준 점의 보정 끔 → 보정 켬, 줄어듦·늘어남), 예상 첫닿음 − Ref (펜이 Ref 에서 뜨거나 누르는 양)·RMS,
            /// 교차검증, 권장하지 않으면 까닭. 앱은 이 글에 묻는 줄만 붙인다. 예외 없음</summary>
            public static string SagPrac_ApplyAskText(CSagPracResult res)
            {
                var ci = CultureInfo.InvariantCulture;
                if (res == null) return "";
                try
                {
                    var sb = new StringBuilder();
                    sb.Append("학습이 끝났습니다 — ").Append(Path.GetFileNameWithoutExtension(res.ResultPath ?? "")).Append("\r\n\r\n");
                    if (!double.IsNaN(res.SpreadPred))
                        sb.Append(string.Format(ci, "기준 점 {0}개에서 높이 흩어짐 {1}\r\n", res.NPred, SagPrac_SpreadText(res.SpreadPredBase, res.SpreadPred, res.ReductionPred).Replace("흩어짐 ", "")));
                    if (!double.IsNaN(res.PredOffset))
                        sb.Append(string.Format(ci, "예상 첫닿음 − Ref (종이 높이 {0:F1}mm): 평균 {1:+0.0;-0.0}mm, RMS {2:F1}mm — {3}\r\n", res.Ref, res.PredOffset, res.RmsPred,
                            Math.Abs(res.PredOffset) <= SAGPRAC_ADVISE_OFFSET_MM ? string.Format(ci, "±{0:0.0}mm 안", SAGPRAC_ADVISE_OFFSET_MM)
                                                                                  : (res.PredOffset < 0 ? "Ref 높이에서 펜이 뜬다" : "Ref 높이에서 펜이 누른다")));
                    CSagLearnResult lr = res.Learn;
                    if (lr != null && !double.IsNaN(lr.CvRmse))
                        sb.Append(string.Format(ci, "학습 교차검증 오차 {0:F1}mm{1}\r\n", lr.CvRmse, double.IsNaN(lr.CvCur) ? "" : string.Format(ci, " (이번 회차 {0:F1}mm)", lr.CvCur)));
                    if (!res.ApplyAdvised) sb.Append("\r\n★권장하지 않음 — ").Append(res.AdviceWhy).Append("\r\n");
                    return sb.ToString();
                }
                catch { return ""; }
            }

            /// <summary>[1. 학습] 안내 한 줄 (확인창용) — 점 수와 대략 걸리는 시간 (분, 실물 기준 — 10-15 실측: 기준 점 약 0.4분, 바닥 데이터 약 0.4분)</summary>
            public static string SagPrac_LearnPlanText(CSagPracCfg cfg, bool bPrecise)
            {
                if (cfg == null) cfg = new CSagPracCfg();
                var ci = CultureInfo.InvariantCulture;
                int n1 = SagPrac_MakePoints(cfg, bPrecise).Count;
                int ns = ((bPrecise ? cfg.PreciseSurveyRadii : cfg.QuickSurveyRadii) ?? new float[0]).Length * 2 - 1;
                if (bPrecise) ns *= 2;
                int nd = SagPrac_DataPoints(cfg, bPrecise).Count;
                return string.Format(ci, "기준 {0}점 + 공중 관절 처짐 약 {1}점 + 바닥 데이터 {2}점 + 학습 — 약 {3:0}분", n1, ns, nd,
                    Math.Ceiling(0.6 + 0.7 + n1 * 0.4 + ns * 0.06 + nd * 0.4 + 0.5 + 0.4));
            }

            // ────────────────────────────────────────────────────────────
            // 1. 오차 재기 (옛 1단계 — 실습 페이지는 [1. 학습] 을 쓴다)
            // ────────────────────────────────────────────────────────────

            /// <summary>1단계 오차 재기 (블로킹 — 작업 스레드). 보정을 모두 끈 채 점마다 첫닿음 명령 높이를 잰다 (헤더 설명).
            /// 새 실습 폴더 <urdf>\sagcomp_practice\<yyyyMMdd_HHmmss>[_3d] 를 만들고 before.csv·before_summary.txt, 작업 기록은 그 안 1_error\.
            /// 성공하면 진행 상태를 1단계로 (전 실습의 2·3·4 단계는 지운다 — 수직 눈 확인 기록은 넘겨받는다). 실물은 연결이 있어야 한다 (없으면 움직이지 않고 거절). 예외 없음.
            /// 10-15: 실습 페이지는 이 함수 대신 [1. 학습] SagPrac_LearnJob 을 쓴다 (옛 스크립트를 위해 남김)</summary>
            public CSagPracResult SagPrac_ErrorJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_ErrorJob(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, null, log, stop);
            }
            /// <summary>1단계 오차 재기 — 옛 겹함수 (eyeAsk 는 쓰지 않는다 — 10-15 부터 수직 눈 확인은 선택 사항이라 묻지 않고 로그 한 줄)</summary>
            public CSagPracResult SagPrac_ErrorJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_ErrorJob(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, eyeAsk, null, log, stop);
            }
            /// <summary>1단계 오차 재기 — 옛 겹함수 (eyeAsk·eyeFine 은 쓰지 않는다: 10-15 부터 수직 눈 확인은 선택 사항 — '로봇 정의' 페이지의 SagPrac_EyeJob.
            /// 예전에는 펼친 자리에서 묻고, 배운 공구 기울기 보정이 지금 것과 다르면 누르지 않고 원위치했다 — 10-15 실측: 눈 확인을 할 때마다 기울기가 0.2° 넘게 달라
            /// 1단계가 바닥을 누르지 않고 돌아왔다)</summary>
            public CSagPracResult SagPrac_ErrorJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                   DSagEyeAsk eyeAsk, DSagEyeFine eyeFine, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_ErrorJobBody(conn, armCfg, cfg, strUrdfDir, bPrecise, bDry, eyeAsk, eyeFine, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_ErrorJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bPrecise, bool bDry,
                                                        DSagEyeAsk eyeAsk, DSagEyeFine eyeFine, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 1, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    armCfg = SagDef_Eff(armCfg);                            // 사본 — 수직 관절 합이 자동이면 3D FK 로 잰 값 (10-13, 못 재면 정의 확인이 거절)
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 폴더를 만들거나 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
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
                    if (armCfg.NoWrist) L(SAGPRAC_EYE_NA + " — 묻지 않음. 꺾는 손목이 없어 공구 기울기는 자세마다 바뀌고, 첫닿음은 팔꿈치 잔차·명령−FK 로 봅니다 (손목 잔차보다 늦게 잡힐 수 있음)");
                    else SagPrac_EyeNote(strUrdfDir, bDry ? null : conn, armCfg, bDry, L);   // 수직 눈 확인 — 선택 사항 (묻지 않고 로그 한 줄)
                    // 3D 만 — 가상 로봇의 닿는 높이 (보정 없음 — 명령, FK)
                    var sim = new Dictionary<int, float[]>();
                    if (bDry)
                    {
                        foreach (CSagPracPoint p in pts)
                        {
                            float z, fk; float[] qi, qa;
                            if (SagPrac_SimTouch(kin, armCfg, cfg, p.X, p.Y, p.Pitch, null, out z, out fk, out qi, out qa)) sim[p.Idx] = new float[] { z, fk };
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
                        // 바닥 찾기 + 가운데 점 + 나머지 점 — [1. 학습] 의 기준과 같은 누름·그럴듯함 확인 (10-15, SagPrac_LearnBase)
                        res.Points = pts;
                        SagPrac_LearnBase(run, pts, iMid, sim, res);
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
                        CSagPracState st = SagPrac_StateKeepEye(strUrdfDir);          // 수직 눈 확인 기록은 넘겨받는다 (기울기 보정 서명 eyetilt 까지 — 10-15)
                        st.Folder = Path.GetFileName(res.FolderDir); st.Dry = res.Dry; st.Precise = bPrecise; st.Step = 1; st.Line1 = res.Line; st.Def = SagDef_Sig(armCfg);
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
                float fk;
                return SagPrac_FindFloor(run, mid, top, out fk);
            }
            //   fk = 그 첫닿음의 엔코더 FK 높이 (못 찾았으면 NaN)
            private float SagPrac_FindFloor(CSagPracRun run, CSagPracPoint mid, float top, out float fk)
            {
                fk = float.NaN;
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
                fk = mid.Fk;
                mid.Touch = mid.Fk = float.NaN; mid.Kind = "";                 // 가운데 점 값은 이어지는 창으로 다시 잰다
                return z;
            }

            // ── 수직 눈 확인 (10-04) — 영점 오차(실물 펜이 기욺)는 엔코더로 볼 수 없어 새 로봇·정의·영점의 첫 누름 전에 사람이 한 번 본다 ──
            /// <summary>수직 눈 확인 답 — 예 (수직), 아니오 (기울어짐 — 누르지 않고 원위치), 중단</summary>
            public enum ESagEyeAns { Yes, No, Stop }

            /// <summary>수직 눈 확인이 배운 손목 공구 기울기 (10-15) — τ = C + B12·(g12 − Ref12) + B13·(g13 − Ref13) (deg, + = 공구 끝이 베이스 쪽, g = −L 중력 지레).
            /// 잰 자리마다 τ = −m (m = 실물 펜이 수직일 때의 엔코더 공구 기울기)</summary>
            public class CSagTiltFit
            {
                public double C, B12, B13, Ref12, Ref13;
                public float TauNear = float.NaN, TauFar = float.NaN;   // 잰 τ (가까운·먼 자리 — 먼 자리를 안 쟀으면 NaN)
                public float RNear = float.NaN, RFar = float.NaN;       // 그 반경 (mm)
                public double G12Near = double.NaN, G13Near = double.NaN, G12Far = double.NaN, G13Far = double.NaN;
                public string Why = "";                                 // 맞춤 판정 (일정 / 자세 항 / 자세 항을 정할 수 없음 …)
                /// <summary>자세 항이 있다 (b12·b13 중 하나라도 0 이 아님)</summary>
                public bool HasPose { get { return B12 != 0 || B13 != 0; } }
                /// <summary>그 지레에서 τ (deg)</summary>
                public double TauAt(double g12, double g13) { return C + B12 * (g12 - Ref12) + B13 * (g13 - Ref13); }
                /// <summary>쉬운 말 — "공구 기울기 보정 +5.2° (일정)" / "… +5.2° (가까운 자리 기준, 먼 자리 +6.4° — 자세에 따라 바뀜)"</summary>
                public string Text()
                {
                    var ci = CultureInfo.InvariantCulture;
                    if (!HasPose) return "공구 기울기 보정 " + C.ToString("+0.0;-0.0;0.0", ci) + "° (일정)";
                    return string.Format(ci, "공구 기울기 보정 {0:+0.0;-0.0;0.0}° (가까운 자리 기준, 먼 자리 {1:+0.0;-0.0;0.0}° — 자세에 따라 바뀜, b12 {2:+0.00;-0.00;0.00})",
                        TauAt(G12Near, G13Near), TauAt(G12Far, G13Far), B12);
                }
                /// <summary>모델에 넣는다 (켬). 자세 항이 없으면 상수만</summary>
                public void PutTo(CSagComp m)
                {
                    if (m == null) return;
                    m.WristTiltOn = true; m.WristTiltC = C; m.WristTiltKeys = true;
                    if (HasPose) { m.WristTiltB = new double[] { B12, B13 }; m.WristTiltRef = new double[] { Ref12, Ref13 }; }
                    else { m.WristTiltB = null; m.WristTiltRef = null; }
                }
                /// <summary>잰 자리 τ 의 큰 쪽 절대값 (deg) — 관절 보정 상한과 견준다</summary>
                public double MaxAbsTau()
                {
                    double a = Math.Abs(TauAt(G12Near, G13Near));
                    if (!double.IsNaN(G12Far)) a = Math.Max(a, Math.Abs(TauAt(G12Far, G13Far)));
                    return a;
                }
            }
            /// <summary>손목 공구 기울기 맞춤 (10-15, 순수 계산) — 가까운 자리의 m(엔코더 공구 기울기, 실물 펜이 수직일 때)·지레 g12/g13, 먼 자리도 쟀으면 그것까지.
            /// τ = −m. 한 점이면 C = τ 가까운 자리. 두 점이면 차 |τ먼 − τ가까운| ≤ fConstTol 이면 일정 (C = 두 점 평균), 넘으면 자세 항 b12 = 차 ÷ (g12먼 − g12가까운),
            /// C = τ가까운, Ref = 가까운 자리 지레 (b13 은 두 반경만으로 정할 수 없어 0 — 상수에 섞인다). 지레 차가 0.2 미만이면 자세 항을 정할 수 없어 평균. 예외 없음</summary>
            public static CSagTiltFit SagPrac_EyeTiltFit(float mNear, double g12Near, double g13Near, float rNear, float mFar, double g12Far, double g13Far, float rFar, float fConstTol)
            {
                var ci = CultureInfo.InvariantCulture;
                var f = new CSagTiltFit { RNear = rNear, G12Near = g12Near, G13Near = g13Near, Ref12 = g12Near, Ref13 = g13Near };
                f.TauNear = -mNear;
                f.C = f.TauNear;
                if (float.IsNaN(mFar) || double.IsNaN(g12Far))
                {
                    f.Why = string.Format(ci, "가까운 자리 (r{0:0}) 한 점 — 일정한 기울기", rNear);
                    return f;
                }
                f.TauFar = -mFar; f.RFar = rFar; f.G12Far = g12Far; f.G13Far = g13Far;
                double d = f.TauFar - f.TauNear, dg = g12Far - g12Near;
                if (Math.Abs(d) <= fConstTol)
                {
                    f.C = (f.TauNear + f.TauFar) / 2.0;
                    f.Why = string.Format(ci, "두 자리 차 {0:+0.00;-0.00;0.00}° (≤ {1:0.0#}°) — 일정한 기울기 (두 점 평균)", d, fConstTol);
                    return f;
                }
                if (Math.Abs(dg) < 0.2)
                {
                    f.C = (f.TauNear + f.TauFar) / 2.0;
                    f.Why = string.Format(ci, "두 자리 차 {0:+0.00;-0.00;0.00}° 이지만 지레 차 {1:0.###} 가 작아 자세 항을 정할 수 없음 — 두 점 평균", d, dg);
                    return f;
                }
                f.B12 = d / dg; f.B13 = 0;
                f.Why = string.Format(ci, "두 자리 차 {0:+0.00;-0.00;0.00}° (> {1:0.0#}°) — 자세 항 b12 = {2:+0.000;-0.000} (지레 g12 {3:0.###} → {4:0.###})", d, fConstTol, f.B12, g12Near, g12Far);
                return f;
            }
            // 기울기 보정 서명·쉬운 말 — 모델이 없으면 "none" / "공구 기울기 보정 없음"
            private static string SagPrac_TiltSigOf(CSagComp m) { return m == null ? "none" : m.WristTiltSig(); }
            private static string SagPrac_TiltTextOf(CSagComp m) { return m == null ? "공구 기울기 보정 없음" : m.WristTiltText(); }
            /// <summary>적용 중 모델의 손목 공구 기울기 보정 — 쉬운 말 ("공구 기울기 보정 +5.2° (일정)" / "공구 기울기 보정 없음") — 정의 페이지 줄. 예외 없음</summary>
            public string SagPrac_TiltText() { try { return SagPrac_TiltTextOf(m_SagComp); } catch { return ""; } }
            /// <summary>적용 중 모델에 손목 공구 기울기 보정이 있다</summary>
            public bool SagPrac_HasTilt { get { CSagComp m = m_SagComp; return m != null && m.HasWristTilt; } }
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
                    if (arm.NoWrist) { text = SAGPRAC_EYE_NA; return false; }   // 꺾는 손목 없음 — '바닥으로 수직' 자세가 없어 묻지 않는다 (10-13)
                    eyeDef = SagDef_Sig(arm);
                    eyeMap = SagPrac_EyeMapKey(conn, arm);
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    bool bDef = st.EyeDef.Length > 0 && string.Equals(st.EyeDef, eyeDef, StringComparison.Ordinal);
                    bool bMap = eyeMap.Length == 0 || string.Equals(st.EyeMap, eyeMap, StringComparison.Ordinal);
                    bool bTilt = string.Equals(string.IsNullOrEmpty(st.EyeTilt) ? "none" : st.EyeTilt, SagPrac_TiltSigOf(m_SagComp), StringComparison.Ordinal);   // 공구 기울기 보정을 바꾸거나 지웠으면 다시 (10-15)
                    if (bDef && bMap && bTilt) { text = "수직 눈 확인 끝" + (st.EyeTime.Length > 0 ? " (" + st.EyeTime + ")" : ""); return false; }
                    return true;
                }
                catch { return true; }
            }
            /// <summary>꺾는 손목 없는 팔의 수직 눈 확인 (10-13) — 공구 기울기가 자세마다 바뀌어 '바닥으로 수직' 자세가 없다. 입구는 이 글을 로그에 남기고 통과</summary>
            public const string SAGPRAC_EYE_NA = "수직 눈 확인 해당 없음 (꺾는 손목 없음)";
            /// <summary>옛 거절 문구 (10-15 부터 쓰지 않는다 — 수직 눈 확인은 선택 사항, 어느 작업도 이것으로 거절하지 않는다). 옛 앱·스크립트와의 호환으로 남김</summary>
            public const string SAGPRAC_EYE_FIRST = "수직 눈 확인 전 (이 로봇·정의·모터 영점으로 처음 한 번) — 로봇팔 세팅 탭 '로봇 정의' 페이지의 [수직 눈 확인] 으로 먼저: " +
                                                    "팔을 베이스 가까운 자리 위 옮기는 높이에서 수직 자세로 펼쳐 실물 펜이 바닥에 수직인지 묻는다 (바닥은 누르지 않고 실습 진행은 그대로)";
            /// <summary>눈 확인을 하지 않고 시작할 때 작업 로그에 남기는 한 줄 (10-15)</summary>
            public const string SAGPRAC_EYE_OPTIONAL = "수직 눈 확인 안 함 — 선택 사항 ('로봇 정의' 페이지의 [수직 눈 확인] 으로 언제든)";
            // 실물 바닥 작업 입구 (③ 측정 SagRun_MeasureJob·⑤ 닿기 시험 SagRun_TestJob·실습 작업) — 움직이기 전에. 3D 만이면 통과.
            //   로봇 정의 확인 (SagDef_Gate — 정의가 틀렸으면 그 까닭으로 거절) 만 관문이다. 수직 눈 확인은 10-15 부터 선택 사항 — 기록이 없거나 지금과 다르면
            //   (SagPrac_EyeNeeded — 정의·모터 영점·공구 기울기 보정 서명) 로그에 한 줄 남기고 통과한다 (SagPrac_EyeNote).
            //   예전에는 여기서 거절했다 (SAGPRAC_EYE_FIRST) — 10-15 실측: 1단계가 눈 확인 기록의 기울기 서명을 지워 2단계가 늘 거절됐다.
            //   꺾는 손목 없는 팔은 눈 확인이 해당 없음 — L 에 그 글을 남기고 통과 (10-13)
            private bool SagPrac_EyeGate(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, bool bDry, CSim2Real.DLog L, out string why)
            {
                why = "";
                if (bDry) return true;
                if (!SagDef_Gate(arm, false, null, out why)) { why = "로봇 정의 — " + why; return false; }
                if (arm != null && arm.NoWrist)
                {
                    if (L != null) L(SAGPRAC_EYE_NA + " — 공구 기울기가 자세마다 바뀌어 '바닥으로 수직' 자세가 없음, 묻지 않고 통과");
                    why = ""; return true;
                }
                SagPrac_EyeNote(strUrdfDir, conn, arm, false, L);
                why = "";
                return true;
            }
            // 수직 눈 확인 — 선택 사항 (10-15): 기록이 없거나 지금과 다르면 로그 한 줄 (SAGPRAC_EYE_OPTIONAL), 기록이 맞으면 그 글 한 줄. 3D 만·손목 없음은 아무것도 남기지 않는다
            private void SagPrac_EyeNote(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, bool bDry, CSim2Real.DLog L)
            {
                try
                {
                    if (bDry || L == null || (arm != null && arm.NoWrist)) return;
                    string t;
                    if (SagPrac_EyeNeeded(strUrdfDir, conn, arm, out t)) L(SAGPRAC_EYE_OPTIONAL);
                    else L(t);
                }
                catch { }
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
            //   strTimeKeep = 기록 시각 (null = 지금 — .ojw 에서 되살릴 때는 그 기록의 시각 그대로). eyeTilt = 그때 걸린 손목 공구 기울기 보정 서명 (10-15, "none" = 없음)
            private static bool SagPrac_SaveEye(string strUrdfDir, string eyeDef, string eyeMap, string eyeTilt, CSim2Real.DLog L, string strTimeKeep = null)
            {
                string strTime = string.IsNullOrEmpty(strTimeKeep) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : strTimeKeep;
                Func<string, string> One = v => (v ?? "").Replace("\r", " ").Replace("\n", " ");
                try
                {
                    string p = Path.Combine(SagPrac_Dir(strUrdfDir), SAGPRAC_STATE);
                    if (string.IsNullOrEmpty(eyeTilt)) eyeTilt = "none";
                    if (!File.Exists(p)) return SagPrac_SaveState(strUrdfDir, new CSagPracState { EyeDef = One(eyeDef), EyeMap = One(eyeMap), EyeTime = strTime, EyeTilt = One(eyeTilt) }, L);
                    byte[] raw = File.ReadAllBytes(p);
                    bool bBom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
                    string text = new UTF8Encoding(false).GetString(raw, bBom ? 3 : 0, raw.Length - (bBom ? 3 : 0));
                    var vals = new Dictionary<string, string> { { "eyedef", One(eyeDef) }, { "eyemap", One(eyeMap) }, { "eyetime", strTime }, { "eyetilt", One(eyeTilt) } };
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
                    foreach (string k in new[] { "eyedef", "eyemap", "eyetime", "eyetilt" })
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
            // ── 수직 눈 확인 — 가까운 자리·엔코더 수직 유지·손 미세 조정 (10-13) ──
            //   예전에는 가운데 점(r190) 위에서 물었다 — 팔이 펴진 만큼 중력 처짐이 펜을 기울여 맞는 로봇도 '수직 아님' 으로 보였고, [아니오] 는 영점 탓으로 안내했다.
            //   이제 베이스 가까운 자리(CSagPracCfg.EyeRadius — 기본 130)에서, 엔코더로 공구를 수직으로 붙잡은 뒤(손목 수직 유지 — 엔코더로 보이는 처짐을 없앤다) 묻는다.
            //   [아니오] 면 손목을 조금씩 기울이는 미세 조정 (DSagEyeFine) — [수직임] 때의 엔코더 기울기로 손목 공구 기울기 보정을 배운다 (10-15 — 앱이 묻고 넣는다).
            /// <summary>손목 미세 조정 요청 (수직 눈 확인 [아니오] 뒤) — None (기다림 끝, 요청 없음), Nudge (손목 명령을 fStepDeg 만큼 더 — ±1° 까지), Zero (δ 를 0 으로 되돌림),
            /// Vertical ([수직임] — 지금 실물 펜이 바닥에 수직), Cancel ([취소] — 기록 없이 원위치), Stop ([중단])</summary>
            public enum ESagEyeFineReq { None, Nudge, Zero, Vertical, Cancel, Stop }
            /// <summary>미세 조정 창에 보일 상태 (작업 스레드가 채워 넘긴다 — 앱은 그리기만)</summary>
            public class CSagEyeFineView
            {
                public int WristID;                          // 꺾는 손목 ID
                public float OffsetDeg;                      // 지금 손 미세 조정 δ (손목 명령, deg — + = 공구 끝이 베이스 쪽으로)
                public float MaxDeg = 10f;                   // δ 상한 (±)
                public float TiltDeg = float.NaN;            // 지금 엔코더 공구 기울기 (실측 관절 합 − 정의의 수직 관절 합, deg — 3D 만·못 읽으면 NaN)
                public bool Dry;                             // 3D 만 (미리 보기 — 기록·적용 없음)
                public bool Busy;                            // 움직이는 중 — 단추를 끄고 요청을 꺼내지 않는다 (바로 None)
                public bool Done;                            // 끝 — 창을 닫는다 (돌려받는 값은 보지 않는다)
                public string Text = "", Note = "";          // 상태 한 줄 · 알림 (한도·관절 한계 …)
            }
            /// <summary>손목 미세 조정 창 — 작업 스레드에서 부른다 (앱은 UI 스레드로 Invoke 해 처음이면 창을 띄우고, 다음부터 상태를 고치고, v.Done 이면 닫는다).
            /// v.Busy 가 아니면 사용자의 다음 요청을 nWaitMs 까지 기다려 돌려준다 (없으면 None — 그사이 작업이 중단 요청을 본다). fStepDeg = Nudge 의 손목 각 (±)</summary>
            public delegate ESagEyeFineReq DSagEyeFine(CSagEyeFineView v, int nWaitMs, out float fStepDeg);
            // 눈 확인 한 번의 끝 — 예 (기록함 / 3D 만 미리 보기 끝), 기울기 (공구 기울기 보정 제안 — 기록하지 않음), 취소, 아니오 (미세 조정 창 없음), 중단, 실패
            private enum ESagEyeOut { Yes, Tilt, Cancel, No, Stop, Fail }   // Tilt = 공구 기울기 보정 제안 (10-15 — 기록하지 않음, 앱이 묻고 적용 뒤 기록)

            /// <summary>손목 영점 고침 계획 (10-13, 순수 계산) — 실물 펜이 바닥에 수직일 때 엔코더로 본 공구 기울기 fTiltDeg (= 실측 관절 합 − 정의의 수직 관절 합,
            /// 3D 만은 손 미세 조정 δ) 만큼 손목의 3D 각이 줄도록 새 영점 틱 = 영점 + 방향 × round(틱/회전 × 기울기 ÷ 각/회전) — '현재 위치를 0 으로' 와 같은 확인
            /// (CArmConn_t.PlanZero: 0~4095, 리밋의 틱 창). 같은 틱에서 새 해석의 3D 각 = 앞 각 − fDegApplied (틱으로 반올림한 고침, deg). 그러면 같은 실물 자세에서
            /// 엔코더 관절 합이 정의의 수직 관절 합이 된다. 틱이 그대로면 고칠 것 없음 (nNewCenter = 영점). m 이 null 이면 기본 세팅 (방향 정·영점 2048·4096/360).
            /// 반환 false = 못 세움 (why — 영점 범위·리밋). 45° 넘게 바뀌면 true 와 why (경고). 예외 없음.
            /// 10-15: 수직 눈 확인은 이것을 쓰지 않는다 (공구 기울기 보정을 배운다 — SagPrac_EyeTiltFit). 계산 도구로 남김</summary>
            public static bool SagPrac_WristZeroPlan(CArmMotor_t m, float fTiltDeg, out int nNewCenter, out float fDegApplied, out string why)
            {
                nNewCenter = 0; fDegApplied = float.NaN; why = "";
                try
                {
                    if (m == null) m = CArmMotor_t.CreateDefault(0);
                    nNewCenter = m.nCenter_Evd;
                    if (float.IsNaN(fTiltDeg) || float.IsInfinity(fTiltDeg)) { why = "기울기가 수가 아님"; return false; }
                    string w;
                    int n;
                    if (!CArmConn_t.PlanZero(m, m.nCenter_Evd, -fTiltDeg, out n, out w)) { why = w; return false; }   // 지금 영점 틱이 3D (−기울기) 가 되는 영점 = 영점 + 방향 × 기울기 틱
                    nNewCenter = n;
                    fDegApplied = (float)((double)(n - m.nCenter_Evd) * m.fMechAngle / m.nMechMove * m.DirSign);
                    why = w ?? "";
                    return true;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
            }

            /// <summary>수직 눈 확인 기록 — 앱이 확인한 결과를 적용한 뒤 부른다 (10-15: 공구 기울기 보정을 적용 중 모델에 넣은 뒤 — SagRun_TiltApply):
            /// 지금 정의 서명(SagDef_Sig)·지금 연결의 모터 매핑 키·지금 적용 중 모델의 기울기 보정 서명으로 state.txt 의 eyedef·eyemap·eyetime·eyetilt 만 쓴다
            /// (실습 진행은 그대로). conn·모터 세팅이 없으면 쓰지 않고 false. 예외 없음</summary>
            public bool SagPrac_EyeRecord(string strUrdfDir, CArmConn_t conn, CSagArmCfg arm, CSim2Real.DLog log, out string why)
            {
                why = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    if (string.IsNullOrEmpty(strUrdfDir)) { why = "urdf 폴더 없음"; return false; }
                    if (conn == null || conn.Motors == null) { why = "연결의 모터 세팅 없음"; return false; }
                    if (arm != null && arm.NoWrist) { why = SAGPRAC_EYE_NA; return false; }
                    string eyeDef = SagDef_Sig(arm), eyeMap = SagPrac_EyeMapKey(conn, SagDef_Eff(arm));
                    if (eyeDef.Length == 0 || eyeMap.Length == 0) { why = "정의 서명·모터 매핑 키를 만들지 못함"; return false; }
                    if (!SagPrac_SaveEye(strUrdfDir, eyeDef, eyeMap, SagPrac_TiltSigOf(m_SagComp), L)) { why = "state.txt 에 쓰지 못함"; return false; }
                    L("수직 눈 확인 기록 — " + SagPrac_TiltTextOf(m_SagComp) + " (실습 진행은 그대로)");
                    return true;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
            }

            // ── .ojw 에 담는 수직 눈 확인 기록 (2026-10-15 — 디자이너 파일 확장 구역 eyeRecord) ──
            /// <summary>.ojw 에 담을 수직 눈 확인 기록 글 — state.txt 의 eyedef·eyemap·eyetime 세 줄 (CRLF). 기록이 없으면 null. 예외 없음</summary>
            public static string SagPrac_EyeRecordText(string strUrdfDir)
            {
                try
                {
                    if (string.IsNullOrEmpty(strUrdfDir)) return null;
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    if (!st.Exists || string.IsNullOrEmpty(st.EyeDef)) return null;
                    Func<string, string> One = v => (v ?? "").Replace("\r", " ").Replace("\n", " ");
                    return "eyedef=" + One(st.EyeDef) + "\r\n" + "eyemap=" + One(st.EyeMap) + "\r\n" + "eyetime=" + One(st.EyeTime) + "\r\n" +
                           "eyetilt=" + One(string.IsNullOrEmpty(st.EyeTilt) ? "none" : st.EyeTilt) + "\r\n";
                }
                catch { return null; }
            }
            /// <summary>눈 확인 기록 글 읽기 (SagPrac_EyeRecordText 꼴 — key=value 줄, // 줄은 설명). eyedef 가 없으면 false. 예외 없음</summary>
            public static bool SagPrac_ParseEyeRecord(string strText, out string eyeDef, out string eyeMap, out string eyeTime)
            {
                string t;
                return SagPrac_ParseEyeRecord(strText, out eyeDef, out eyeMap, out eyeTime, out t);
            }
            /// <summary>SagPrac_ParseEyeRecord 와 같고 기울기 보정 서명(eyetilt — 없으면 "none")도</summary>
            public static bool SagPrac_ParseEyeRecord(string strText, out string eyeDef, out string eyeMap, out string eyeTime, out string eyeTilt)
            {
                eyeDef = ""; eyeMap = ""; eyeTime = ""; eyeTilt = "none";
                try
                {
                    foreach (string raw in (strText ?? "").Split('\n'))
                    {
                        string ln = raw.Trim();
                        if (ln.Length == 0 || ln.StartsWith("//")) continue;
                        int eq = ln.IndexOf('='); if (eq <= 0) continue;
                        string k = ln.Substring(0, eq).Trim().ToLowerInvariant(), v = ln.Substring(eq + 1).Trim();
                        if (k == "eyedef") eyeDef = v; else if (k == "eyemap") eyeMap = v; else if (k == "eyetime") eyeTime = v; else if (k == "eyetilt" && v.Length > 0) eyeTilt = v;
                    }
                    return eyeDef.Length > 0;
                }
                catch { return false; }
            }
            /// <summary>.ojw 에서 온 수직 눈 확인 기록 되살리기 (UI 스레드) — 기록의 정의 서명이 지금 정의의 서명(SagDef_Sig)과 같고, 기록의 모터 매핑 키가
            /// set (불러온 모터 세팅 — 움직이는 관절만, SagPrac_EyeRecord 와 같은 키)의 키와 같고, 기록의 공구 기울기 보정 서명이 지금 적용 중 모델의 것과 같을 때만
            /// state.txt 의 eyedef·eyemap·eyetime·eyetilt 를 쓴다 (시각은 기록 그대로, 실습 진행은 그대로). 하나라도 다르면 쓰지 않는다 — 눈 확인은 다시 해야 한다 (false, why).
            /// 이미 같은 기록이면 쓰지 않고 true. 예외 없음</summary>
            public bool SagPrac_EyeRestore(string strUrdfDir, CSagArmCfg arm, CArmMotorSet_t set, string strRecordText, CSim2Real.DLog log, out string why)
            {
                why = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    string d, m, t, tl;
                    if (!SagPrac_ParseEyeRecord(strRecordText, out d, out m, out t, out tl)) { why = "눈 확인 기록 없음"; return false; }
                    if (string.IsNullOrEmpty(strUrdfDir)) { why = "urdf 폴더 없음"; return false; }
                    if (arm == null) arm = new CSagArmCfg();
                    if (arm.NoWrist) { why = SAGPRAC_EYE_NA; return false; }
                    if (set == null) { why = "모터 세팅 없음"; return false; }
                    string strDef = SagDef_Sig(arm);
                    int[] taken;
                    CArmMotorSet_t sub = set.MergeFor(SagDef_Eff(arm).MoveIDs, out taken);
                    string strMap = (sub == null) ? "" : (sub.MappingKey() ?? "");
                    if (strDef.Length == 0 || !string.Equals(strDef, d, StringComparison.Ordinal)) { why = "다른 로봇 정의로 한 눈 확인 (기록 " + d + " / 지금 " + strDef + ")"; return false; }
                    if (strMap.Length == 0 || !string.Equals(strMap, m, StringComparison.Ordinal)) { why = "다른 모터 영점·방향으로 한 눈 확인 (" + CArmMotorSet_t.DescribeMappingDiff(m, strMap) + ")"; return false; }
                    string strTilt = SagPrac_TiltSigOf(m_SagComp);
                    if (!string.Equals(strTilt, tl, StringComparison.Ordinal)) { why = "다른 공구 기울기 보정으로 한 눈 확인 (기록 " + tl + " / 지금 " + strTilt + ")"; return false; }
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    if (string.Equals(st.EyeDef, d, StringComparison.Ordinal) && string.Equals(st.EyeMap, m, StringComparison.Ordinal) &&
                        string.Equals(string.IsNullOrEmpty(st.EyeTilt) ? "none" : st.EyeTilt, tl, StringComparison.Ordinal)) { why = "이미 같은 기록"; return true; }
                    if (!SagPrac_SaveEye(strUrdfDir, d, m, tl, L, t)) { why = "state.txt 에 쓰지 못함"; return false; }
                    L("수직 눈 확인 기록 되살림 (.ojw" + (t.Length > 0 ? ", " + t : "") + ") — 같은 정의·모터 영점");
                    return true;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
            }

            // 묻는 글 (지금 자리 — 눈 확인 자리, r·z). nMode: 1 = 실습 1단계 ([예] 뒤 바닥 찾기), 0 = 눈 확인만 ([예] 면 기록하고 원위치),
            //   2 = 먼 자리 (10-15 — 두 점으로 공구 기울기의 자세 항을 맞춘다). bDry = 3D 만 미리 보기 (기록 없음). bFine = [아니오] 뒤 손 미세 조정 창이 있다.
            //   bFarNext = [예] 뒤 먼 자리로 옮겨 한 번 더 본다. fTilt·strTilt = 지금 걸린 손목 공구 기울기 보정 (τ, 쉬운 말).
            //   숫자는 res (자동 수직 유지 전·후 엔코더 기울기, 유지 각, 관절 모델 예측). 기울어 보이는 까닭을 단정하지 않는다 (엔코더 밖 기울기 — 영점·펜 장착·브래킷 등)
            private string SagPrac_EyePrompt(CSagArmCfg c, int nMode, bool bDry, double r, float z, CSagPracResult res, float fNearTilt, float fFarR, bool bFine,
                                             float fTilt, string strTilt, bool bFarNext)
            {
                var ci = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                if (bDry) sb.Append("수직 눈 확인 — 3D 만 미리 보기 (실물은 움직이지 않음, 기록·적용하지 않음)\r\n\r\n");
                else if (nMode == 2) sb.Append("수직 눈 확인 — 먼 자리 (두 자리로 공구 기울기가 자세에 따라 바뀌는지 봅니다)\r\n\r\n");
                else if (nMode == 0) sb.Append("수직 눈 확인 (이 로봇·정의·모터 영점·공구 기울기 보정으로 한 번 — 바닥은 누르지 않음)\r\n\r\n");
                else sb.Append("수직 눈 확인 (이 로봇·정의로 처음 한 번)\r\n\r\n");
                sb.Append(string.Format(ci, "{6}팔이 {7} (반경 {0:0}, 높이 {1:0}) 에서 '바닥으로 수직' 자세로 멈춰 있습니다 (관절 합 {2}+{3}+{4} = {5:0.#}°).\r\n",
                    r, z, c.PosIDs[1], c.PosIDs[2], c.WristID, c.DownSumDeg, bDry ? "3D " : "", nMode == 2 ? "먼 자리" : "베이스 가까운 자리"));
                if (Math.Abs(fTilt) >= 0.005f)
                    sb.Append(string.Format(ci, "지금 걸린 {0} — 이 자리 τ {1:+0.00;-0.00}°: 손목 명령에서 τ 를 뺀 자리를 '수직' 으로 봅니다.\r\n", strTilt, fTilt));
                if (bDry) sb.Append("실물이면 여기서 엔코더로 공구를 붙잡은 뒤 묻습니다 (3D 만은 엔코더가 없어 생략).\r\n");
                else if (!float.IsNaN(res.EyeTiltBefore) && !float.IsNaN(res.EyeTiltAfter))
                {
                    string strRef = Math.Abs(fTilt) >= 0.005f ? "수직 − τ" : "수직";
                    if (Math.Abs(res.EyeHoldDeg) < 0.005f || float.IsNaN(res.EyeHoldDeg))
                        sb.Append(string.Format(ci, "엔코더 공구 기울기 {0:+0.00;-0.00;0.00}° — 이미 {1} (손목 그대로).\r\n", res.EyeTiltAfter, strRef));
                    else
                        sb.Append(string.Format(ci, "엔코더로 공구를 붙잡았습니다 (기준 = {3}) — 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}° → {1:+0.00;-0.00;0.00}° (손목 명령 {2:+0.0;-0.0;0.0}°).\r\n",
                            res.EyeTiltBefore, res.EyeTiltAfter, res.EyeHoldDeg, strRef));
                }
                else sb.Append("★엔코더 공구 기울기를 읽지 못해 자동 수직 유지를 하지 못했습니다 (그대로 보고 답하십시오).\r\n");
                if (!float.IsNaN(res.EyeFarTiltDeg))
                    sb.Append(string.Format(ci, "적용 중 관절 모델의 예상 처짐 기울기 (보정 없이 엔코더로 보일 값): 이 자리 {0}, 먼 자리 (반경 {1:0}) 약 {2:0.0}° — 관절 모델이 켜져 있으면 대부분 메웁니다.\r\n",
                        float.IsNaN(fNearTilt) ? "—" : "약 " + Math.Abs(fNearTilt).ToString("0.0", ci) + "°", fFarR, Math.Abs(res.EyeFarTiltDeg)));
                else if (nMode != 2) sb.Append("팔을 멀리 뻗을수록 중력 처짐이 공구를 기울입니다 — 그래서 먼저 베이스 가까운 자리에서 봅니다.\r\n");
                sb.Append("3D 에는 공구 끝(빨강)·공구축(초록 선)·꺾는 손목(주황)이 보입니다.\r\n\r\n");
                if (bDry)
                {
                    sb.Append(bFarNext ? "[예] 이 자리 미리 보기 끝 — 3D 를 먼 자리로 옮깁니다\r\n" : "[예] 미리 보기 끝 — 3D 를 원위치합니다\r\n");
                    sb.Append(bFine ? "[아니오] 손목 미세 조정 창을 미리 봅니다 (공구 기울기 보정은 계산만 — 적용·기록하지 않음)" : "[아니오] 미리 보기 끝 — 3D 를 원위치합니다");
                    return sb.ToString();
                }
                sb.Append("실물 펜도 바닥에 수직입니까? 옆과 앞에서 보십시오.\r\n\r\n");
                if (nMode == 2) sb.Append("[예] 수직 — 이 자리 기울기를 재고 두 자리로 맞춥니다\r\n");
                else if (bFarNext) sb.Append("[예] 수직 — 이 자리 기울기를 재고 먼 자리로 옮겨 한 번 더 봅니다\r\n");
                else sb.Append(nMode == 0 ? "[예] 수직 — 기록하고 원위치합니다 (선택 사항 — 실습·측정은 기록 없이도 할 수 있음)\r\n"
                                          : "[예] 수직 — 기록하고 바닥 찾기를 이어 갑니다\r\n");
                sb.Append(bFine ? "[아니오] 기울어짐 — 손목 미세 조정 창이 열립니다: 손목을 조금씩 기울여 실물 펜을 수직으로 맞추고 [수직임] " +
                                  "(그 기울기를 '공구 기울기 보정' 으로 배웁니다 — 엔코더 밖 기울기: 영점·펜 장착·브래킷 등. 넣을지는 끝난 뒤 묻습니다)"
                                : "[아니오] 기울어짐 — 기록하지 않고 원위치합니다");
                return sb.ToString();
            }

            // 엔코더 공구 기울기 (실물 — 정착 읽기) = 실측 관절 합 (피치 둘 + 손목) − 기준 관절 합 fRefSum (deg). 못 읽으면 NaN.
            //   기준 = 정의의 수직 관절 합이면 '바닥에 수직에서 기운 각', 지금 3D 관절 합이면 '명령에서 벗어난 각' (자동 수직 유지가 쓴다)
            private static float SagPrac_EyeTiltRead(CSagArm arm, float fRefSum)
            {
                try
                {
                    if (arm == null || arm.Scene == null) return float.NaN;
                    string w;
                    Dictionary<int, float> d = arm.Scene.ReadDeg(out w);
                    if (d == null) return float.NaN;
                    CSagArmCfg c = arm.Cfg;
                    float a1, a2, a3;
                    if (!d.TryGetValue(c.PosIDs[1], out a1) || !d.TryGetValue(c.PosIDs[2], out a2) || !d.TryGetValue(c.WristID, out a3)) return float.NaN;
                    return (float)((double)a1 + a2 + a3 - fRefSum);
                }
                catch { return float.NaN; }
            }

            // 자동 수직 유지 (실물) — 손목 수직 유지(측정 누름과 같은 출력 훅 SagComp_WristHold*)를 켜고 자리 잡기 반복 (SagComp_WristSettleCore — 재기 → 손목 명령 고치기 →
            //   같은 자세를 보내며(출력 틱이 흘러야 실물에 들어간다) 기다리기, |기울기| ≤ EyeHoldTolDeg 면 끝, 상한 EyeHoldMaxDeg·반복 EyeHoldIter).
            //   기울기 = 실측 관절 합 − 지금 3D 관절 합 (자세가 정의의 수직이므로 '바닥에 수직에서 기운 각'). 놓지 않는다 — 부른 쪽 (1단계는 바닥 찾기 전, 눈 확인만은 SagArm_Close).
            //   전·후 기울기·유지 각은 res 에, 로그 한 줄. 중단 요청·한 틱 관절 변화 막이가 걸리면 그 자리에서 멈춘다
            private void SagPrac_EyeHold(CSagArm arm, CSagPracCfg cfg, CSagPracResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop)
            {
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                int[] ids = c.MoveIDs;
                var pc = new CFloorProbeCfg { WristHoldTolDeg = cfg.EyeHoldTolDeg, WristHoldMaxDeg = cfg.EyeHoldMaxDeg, WristSettleIter = Math.Max(1, cfg.EyeHoldIter),
                                              WristSettleWaitMs = Math.Max(50, cfg.EyeHoldWaitMs), WristSettleGain = 0.8f, WristSettleMaxStepDeg = 1f };   // 한 번에 1° 까지 (천천히)
                Func<double> read = delegate()
                {
                    // 기준 = 지금 3D 이상 합 − τ (손목 공구 기울기 보정이 걸려 있으면 — 10-15). 유지가 출력 단의 −τ 를 지우지 않고 실물 공구가 수직인 자리를 붙잡는다
                    float sumCmd = GetData(c.PosIDs[1]) + GetData(c.PosIDs[2]) + GetData(c.WristID) - SagComp_TiltNow();
                    return SagPrac_EyeTiltRead(arm, sumCmd);
                };
                Action send = delegate()
                {
                    var v = new List<float>();
                    foreach (int id in ids) { v.Add(id); v.Add(GetData(id)); }
                    Play(300, 0, v.ToArray());                                       // 같은 자세를 보낸다 — 손목 유지 값이 출력 틱에 실려 실물에 들어간다
                };
                Func<bool> halt = delegate() { return SagRun_Stopped(stop) || SagArm_GuardWhy(arm).Length > 0 || !SagArm_SinkOk(arm, null); };
                float dp0, dp1; int nFix;
                L(string.Format(ci, "진행 수직 눈 확인 — 엔코더로 공구를 수직으로 붙잡는 중 (허용 ±{0:0.0#}°, 손목 상한 ±{1:0.#}°)", cfg.EyeHoldTolDeg, cfg.EyeHoldMaxDeg));
                SagComp_WristHoldBegin(c.WristID);
                SagComp_WristSettleCore(read, send, halt, pc, out dp0, out dp1, out nFix);
                res.EyeTiltBefore = dp0; res.EyeTiltAfter = dp1; res.EyeHoldDeg = SagComp_WristHoldValue();
                if (float.IsNaN(dp0)) L("★엔코더 공구 기울기를 읽지 못함 (또는 10° 넘음 — 읽기 이상·큰 영점 차) — 자동 수직 유지 안 함");
                else L(string.Format(ci, "자동 수직 유지: 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}° → {1:+0.00;-0.00;0.00}° (손목 명령 {2:+0.00;-0.00;0.00}°, 고침 {3}번{4})",
                    dp0, dp1, res.EyeHoldDeg, nFix, Math.Abs(dp1) <= cfg.EyeHoldTolDeg + 1e-4f ? "" : " — ★허용 밖 (상한·반복 끝)"));
            }

            // 관절 모델 예측 (적용 중 모델의 관절 부분 — 시작 때 모델 arm.PrevModel, 켜져 있고 관절 모델이 있을 때만): 보정 없이 그 자세로 두면 엔코더로 보일 공구 기울기
            //   = −(피치 둘 + 손목 관절 보정 합) (deg). 이 자리(q0)와 먼 자리(반경 EyeFarRadius, 같은 요·높이 — 못 풀면 10mm 씩 내려 본다). 없으면 NaN
            private float SagPrac_EyeFarTilt(CSagArm arm, CSagPracCfg cfg, float[] q0, float z, out float fNear)
            {
                fNear = float.NaN;
                try
                {
                    CSagComp m = arm.PrevModel;
                    if (m == null || !m.Enabled || !m.HasJoint) return float.NaN;
                    CSagComp mj = m.JointOnlyCopy();
                    mj.ClearWristTilt();                                             // 처짐만 (손목 공구 기울기 보정은 빼고 — 사본의 칸만 비운다)
                    CSagArmCfg c = arm.Cfg;
                    int[] ids = c.MoveIDs;
                    Func<float[], float> Tilt = delegate(float[] q)
                    {
                        float[] corr = SagComp_PredictJointCorr(mj, ids, q, null);
                        return corr == null ? float.NaN : -(corr[1] + corr[2] + corr[3]);
                    };
                    fNear = Tilt(q0);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return float.NaN;
                    double a = cfg.EyeYaw * Math.PI / 180.0;
                    float xf = (float)(cfg.EyeFarRadius * Math.Cos(a)), yf = (float)(cfg.EyeFarRadius * Math.Sin(a));
                    for (int i = 0; i < 8; i++)
                    {
                        float bend;
                        float[] qf = SagArm_IkSum(kin, c, SagRun_Seed(kin, c, cfg.EyeYaw, 90f), xf, yf, z - 10f * i, c.DownSumDeg, out bend);
                        if (qf != null) return Tilt(qf);
                    }
                    return float.NaN;
                }
                catch { return float.NaN; }
            }

            // 눈 확인 한 번 (10-13 — 실습 1단계·눈 확인만 같은 길, 작업 스레드). arm = 펼친 손잡이 (옮기는 높이·바닥으로 수직 자세 — 어느 자리든)
            //   ① 눈 확인 자리로: 베이스 가까운 반경 EyeRadius·요 EyeYaw 위 옮기는 높이 (SagArm_MoveSafeCore — 다리 사이 중단, 바닥은 누르지 않는다. 이미 그 자리면 그대로)
            //      ★먼 자리(가운데 점 r190 등)는 팔이 펴진 만큼 중력 처짐이 펜을 기울여 맞는 로봇도 수직이 아니게 보였다
            //   ② 높이(바닥 찾기 시작 높이 이상)·관절 합(정의의 수직 관절 합)이 맞아야 묻는다
            //   ③ 실물: 자동 수직 유지 (SagPrac_EyeHold — 엔코더로 보이는 처짐을 없앤다, 기준 = 수직 − 걸린 공구 기울기 τ). 3D 만은 엔코더가 없어 생략
            //   ④ 관절 모델이 있으면 이 자리·먼 자리의 처짐 기울기 예측 (참고)
            //   ⑤ 묻기 (eyeAsk) — 예: 그 자리 엔코더 공구 기울기 m / 아니오: 손 미세 조정 (eyeFine — 없으면 기록 없이 끝) → [수직임] 때의 m / 중단
            //   ⑥ cfg.EyeFar 면 먼 자리에서 같은 일 (SagPrac_EyeFarAt) ⑦ 맞춤·판정 (SagPrac_EyeConclude — 같으면 기록, 다르면 공구 기울기 보정 제안)
            //   묻는 동안 3D 표시를 이 정의로 켠다 (ToolMark_Hold — 끝나면 되돌림). 손목 유지는 놓지 않는다 (부른 쪽이 — 1단계는 바닥 찾기 전, 눈 확인만은 SagArm_Close)
            private ESagEyeOut SagPrac_EyeAt(CSagArm arm, CSagPracCfg cfg, DSagEyeAsk eyeAsk, DSagEyeFine eyeFine, int nMode, string strUrdfDir, string eyeDef, string eyeMap,
                                             CSagPracResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop, out string why)
            {
                why = "";
                var ci = CultureInfo.InvariantCulture;
                if (cfg == null) cfg = new CSagPracCfg();
                CSagArmCfg c = arm.Cfg;
                bool bDry = arm.IsDry;
                double ar = cfg.EyeYaw * Math.PI / 180.0;
                float fEx = (float)(cfg.EyeRadius * Math.Cos(ar)), fEy = (float)(cfg.EyeRadius * Math.Sin(ar));
                float x, y, z, p;
                SagArm_Tcp(c, out x, out y, out z, out p);
                if (Math.Sqrt((double)(x - fEx) * (x - fEx) + (double)(y - fEy) * (y - fEy)) > 1.0)
                {
                    L(string.Format(ci, "진행 수직 눈 확인 — 베이스 가까운 자리 r={0:0.#} 요 {1:0.#} 위로 옮김 (먼 자리는 처짐이 펜을 기울인다)", cfg.EyeRadius, cfg.EyeYaw));
                    if (!SagArm_MoveSafeCore(arm, fEx, fEy, c.SafeZ, 90, float.NaN, float.NaN, L, stop))
                    {
                        if (arm.StopSeen) return ESagEyeOut.Stop;
                        why = SagRun_WhyJoin("눈 확인 자리로 옮기지 못함", arm.LastFail);
                        return ESagEyeOut.Fail;
                    }
                    SagArm_Tcp(c, out x, out y, out z, out p);
                }
                double r = Math.Sqrt((double)x * x + (double)y * y);
                float top = Math.Min(cfg.FindStartZ, c.SafeZ - 10);
                float sum = SagArm_SumNow(c);
                if (!(z >= top - 0.5f)) { why = string.Format(ci, "눈 확인 자리의 높이 z={0:0.#} 가 바닥 찾기 시작 높이 {1:0.#} 보다 낮음 — 묻지 않고 원위치", z, top); return ESagEyeOut.Fail; }
                if (!(Math.Abs(sum - c.DownSumDeg) <= SAGDEF_TOL_DEG)) { why = string.Format(ci, "눈 확인 자세의 관절 합 {0:F1}° 가 정의 {1:0.###}° 와 다름 — 묻지 않고 원위치", sum, c.DownSumDeg); return ESagEyeOut.Fail; }
                if (SagRun_Stopped(stop)) return ESagEyeOut.Stop;
                float[] q0 = SagArm_QNow(c);
                if (!bDry) SagPrac_EyeHold(arm, cfg, res, L, stop);
                else L("3D 만 — 엔코더가 없어 자동 수직 유지는 하지 않음 (미리 보기)");
                if (SagRun_Stopped(stop)) return ESagEyeOut.Stop;
                string g = SagArm_GuardWhy(arm);
                if (g.Length > 0) { why = g; return ESagEyeOut.Fail; }
                float fNear;
                res.EyeFarTiltDeg = SagPrac_EyeFarTilt(arm, cfg, q0, z, out fNear);
                if (!float.IsNaN(res.EyeFarTiltDeg))
                    L(string.Format(ci, "관절 모델 예측 처짐 기울기 — 이 자리 r={0:0} {1}, 먼 자리 r={2:0} {3:+0.00;-0.00;0.00}° (보정 없이 엔코더로 보일 기울기 = −관절 보정 합)", r,
                        float.IsNaN(fNear) ? "—" : fNear.ToString("+0.00;-0.00;0.00", ci) + "°", cfg.EyeFarRadius, res.EyeFarTiltDeg));
                object mark = ToolMark_Hold(c);
                try
                {
                    if (eyeAsk == null)
                    {
                        res.EyeAns = bDry ? "3D 만" : "생략";
                        L(string.Format(ci, "{0}수직 자세 (r{1:0}, z{2:0}) — 묻는 곳 없음, 기록하지 않음", bDry ? "3D 만 — " : "★", r, z));
                        return bDry ? ESagEyeOut.Yes : ESagEyeOut.No;
                    }
                    float fTilt = SagComp_TiltNow();
                    string strTilt = SagPrac_TiltTextOf(arm.PrevModel);
                    bool bFar = cfg.EyeFar;
                    L(string.Format(ci, "진행 수직 눈 확인 — 대답을 기다림 (r{0:0}, z{1:0}{2})", r, z, Math.Abs(fTilt) >= 0.005f ? string.Format(ci, ", 공구 기울기 보정 τ {0:+0.00;-0.00}°", fTilt) : ""));
                    ESagEyeAns ans;
                    try { ans = eyeAsk(SagPrac_EyePrompt(c, nMode, bDry, r, z, res, fNear, cfg.EyeFarRadius, eyeFine != null, fTilt, strTilt, bFar)); }
                    catch (Exception ex) { L("★수직 눈 확인 오류: " + SagRun_Msg(ex)); ans = ESagEyeAns.Stop; }
                    if (ans == ESagEyeAns.Stop) { res.EyeAns = "중단"; return ESagEyeOut.Stop; }
                    float mNear;
                    if (ans == ESagEyeAns.Yes)
                    {
                        mNear = bDry ? 0f : SagPrac_EyeTiltRead(arm, c.DownSumDeg);       // 실물 펜이 수직일 때의 엔코더 공구 기울기 (= −τ)
                        L(string.Format(ci, "수직 눈 확인: 예 (r{0:0}){1}", r, float.IsNaN(mNear) ? " — ★엔코더 공구 기울기를 읽지 못함" : string.Format(ci, " — 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}°", mNear)));
                    }
                    else
                    {
                        if (eyeFine == null)
                        {
                            if (bDry) { res.EyeAns = "3D 만"; L("3D 만 — 미리 보기 끝 (기록하지 않음)"); return ESagEyeOut.Yes; }
                            res.EyeAns = "아니오";
                            why = "실물 펜이 수직이 아님 (확인: 아니오) — 미세 조정 창이 없어 기록하지 않음";
                            return ESagEyeOut.No;
                        }
                        ESagEyeOut oF = SagPrac_EyeFine(arm, cfg, eyeFine, q0, res, L, stop, out why, out mNear);
                        if (oF != ESagEyeOut.Yes) return oF;
                    }
                    // 먼 자리 (선택 — 두 점으로 자세 항)
                    float mFar = float.NaN; float[] qFar = null; double rFar = double.NaN;
                    if (bFar && !float.IsNaN(mNear))
                    {
                        string whyFar;
                        ESagEyeOut oFar = SagPrac_EyeFarAt(arm, cfg, eyeAsk, eyeFine, res, L, stop, out mFar, out qFar, out rFar, out whyFar);
                        if (oFar == ESagEyeOut.Stop) { res.EyeAns = "중단"; return ESagEyeOut.Stop; }
                        if (oFar != ESagEyeOut.Yes)
                        {
                            L("★먼 자리 눈 확인 " + (oFar == ESagEyeOut.Cancel ? "취소" : "못 함") + (whyFar.Length > 0 ? " (" + whyFar + ")" : "") + " — 가까운 자리 결과만으로");
                            mFar = float.NaN; qFar = null;
                        }
                    }
                    return SagPrac_EyeConclude(arm, cfg, q0, r, mNear, qFar, rFar, mFar, strUrdfDir, eyeDef, eyeMap, res, L, out why);
                }
                finally { ToolMark_Release(mark); }
            }

            // 먼 자리 눈 확인 (10-15, cfg.EyeFar) — 가까운 자리를 마친 뒤: 손목 유지를 놓고 (옮기는 동안 빠진다) 먼 자리 (반경 EyeFarRadius, 같은 요) 위
            //   옮기는 높이로 (IK·경로가 안 되면 10mm 씩 낮춰 바닥 찾기 시작 높이까지 — 가까운 자리와 같은 높이 규칙), 자동 수직 유지 → 묻기
            //   ([예] = 그 자리 엔코더 공구 기울기, [아니오] = 미세 조정 → [수직임] 때의 기울기). mFar = 실물 펜이 수직일 때의 엔코더 공구 기울기, qFar = 그 자리 이상 자세.
            //   반환 Yes / Cancel / Stop / Fail (why)
            private ESagEyeOut SagPrac_EyeFarAt(CSagArm arm, CSagPracCfg cfg, DSagEyeAsk eyeAsk, DSagEyeFine eyeFine, CSagPracResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop,
                                                out float mFar, out float[] qFar, out double rFar, out string why)
            {
                mFar = float.NaN; qFar = null; rFar = double.NaN; why = "";
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                bool bDry = arm.IsDry;
                double ar = cfg.EyeYaw * Math.PI / 180.0;
                float fx = (float)(cfg.EyeFarRadius * Math.Cos(ar)), fy = (float)(cfg.EyeFarRadius * Math.Sin(ar));
                float top = Math.Min(cfg.FindStartZ, c.SafeZ - 10);
                try { SagComp_WristHoldRelease(); } catch { }                      // 가까운 자리의 손목 유지 — 옮기는 동안 빠진다 (먼 자리에서 다시 잡는다)
                L(string.Format(ci, "진행 수직 눈 확인 — 먼 자리 r={0:0.#} 요 {1:0.#} 위로 옮김 (두 자리로 공구 기울기가 자세에 따라 바뀌는지 봄)", cfg.EyeFarRadius, cfg.EyeYaw));
                bool bMoved = false;
                for (float zt = c.SafeZ; zt >= top - 0.01f; zt -= 10f)
                {
                    if (SagRun_Stopped(stop)) return ESagEyeOut.Stop;
                    if (SagArm_MoveSafeCore(arm, fx, fy, zt, 90, float.NaN, float.NaN, L, stop)) { bMoved = true; break; }
                    if (arm.StopSeen) return ESagEyeOut.Stop;
                    string gw = SagArm_GuardWhy(arm);
                    if (gw.Length > 0) { why = gw; return ESagEyeOut.Fail; }
                    L(string.Format(ci, "먼 자리 z={0:0} 로 옮기지 못함 ({1}) — 10mm 낮춰 봄", zt, arm.LastFail));
                }
                if (!bMoved) { why = SagRun_WhyJoin("먼 자리로 옮기지 못함", arm.LastFail); return ESagEyeOut.Fail; }
                float x, y, z, p;
                SagArm_Tcp(c, out x, out y, out z, out p);
                rFar = Math.Sqrt((double)x * x + (double)y * y);
                float sum = SagArm_SumNow(c);
                if (!(Math.Abs(sum - c.DownSumDeg) <= SAGDEF_TOL_DEG)) { why = string.Format(ci, "먼 자리 자세의 관절 합 {0:F1}° 가 정의 {1:0.###}° 와 다름", sum, c.DownSumDeg); return ESagEyeOut.Fail; }
                qFar = SagArm_QNow(c);
                var rf = new CSagPracResult { Dry = bDry };
                if (!bDry) SagPrac_EyeHold(arm, cfg, rf, L, stop);
                if (SagRun_Stopped(stop)) return ESagEyeOut.Stop;
                string g = SagArm_GuardWhy(arm);
                if (g.Length > 0) { why = g; return ESagEyeOut.Fail; }
                float fTilt = SagComp_TiltNow();
                ESagEyeAns ans;
                try { ans = eyeAsk(SagPrac_EyePrompt(c, 2, bDry, rFar, z, rf, float.NaN, cfg.EyeFarRadius, eyeFine != null, fTilt, SagPrac_TiltTextOf(arm.PrevModel), false)); }
                catch (Exception ex) { L("★수직 눈 확인 오류: " + SagRun_Msg(ex)); ans = ESagEyeAns.Stop; }
                if (ans == ESagEyeAns.Stop) return ESagEyeOut.Stop;
                if (ans == ESagEyeAns.Yes)
                {
                    mFar = bDry ? 0f : SagPrac_EyeTiltRead(arm, c.DownSumDeg);
                    L(string.Format(ci, "먼 자리 눈 확인: 예 (r{0:0}){1}", rFar, float.IsNaN(mFar) ? " — ★엔코더 공구 기울기를 읽지 못함" : string.Format(ci, " — 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}°", mFar)));
                    if (float.IsNaN(mFar)) { why = "먼 자리 엔코더 공구 기울기를 읽지 못함"; return ESagEyeOut.Fail; }
                    return ESagEyeOut.Yes;
                }
                if (eyeFine == null) { why = "먼 자리 — 미세 조정 창이 없음"; return ESagEyeOut.Cancel; }
                return SagPrac_EyeFine(arm, cfg, eyeFine, qFar, rf, L, stop, out why, out mFar);
            }

            // 눈 확인 끝 (10-15) — 잰 엔코더 공구 기울기 m (실물 펜이 수직일 때) 로 손목 공구 기울기 τ = −m 을 맞추고 (먼 자리도 쟀으면 두 점 — SagPrac_EyeTiltFit),
            //   지금 걸린 항 (적용 중 모델 arm.PrevModel) 과 잰 자리마다 EyeTiltSameDeg 안이면 바꿀 것 없이 기록 (예), 아니면 제안 (EyeAns "기울기", res.EyeTiltFit — 기록하지 않는다:
            //   앱이 묻고 SagRun_TiltApply 뒤 SagPrac_EyeRecord). m 을 읽지 못했으면 기울기는 그대로 두고 기록만. 3D 만은 계산만 (미리 보기)
            private ESagEyeOut SagPrac_EyeConclude(CSagArm arm, CSagPracCfg cfg, float[] qNear, double rNear, float mNear, float[] qFar, double rFar, float mFar,
                                                   string strUrdfDir, string eyeDef, string eyeMap, CSagPracResult res, CSim2Real.DLog L, out string why)
            {
                why = "";
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                bool bDry = arm.IsDry;
                CSagComp mAct = arm.PrevModel;                                    // 적용 중 모델 (기울기 보정을 넣을 곳 — 없으면 기울기만 있는 모델을 만든다)
                string strSig = SagPrac_TiltSigOf(mAct);
                if (float.IsNaN(mNear))
                {
                    if (bDry) { res.EyeAns = "3D 만"; L("3D 만 — 미리 보기 끝 (기록하지 않음)"); return ESagEyeOut.Yes; }
                    res.EyeAns = "예";
                    if (!SagPrac_SaveEye(strUrdfDir, eyeDef, eyeMap, strSig, L)) { why = "수직 눈 확인 기록을 쓰지 못함 — 다시 하십시오"; return ESagEyeOut.Fail; }
                    L("수직 눈 확인: 예 — 기록함 (★엔코더 공구 기울기를 읽지 못해 공구 기울기 보정은 그대로: " + SagPrac_TiltTextOf(mAct) + ")");
                    return ESagEyeOut.Yes;
                }
                int[] ids = c.MoveIDs;
                double g12n, g13n, g12f = double.NaN, g13f = double.NaN;
                if (!SagComp_TiltLevers(mAct, c, ids, qNear, out g12n, out g13n)) { g12n = 0; g13n = 0; }
                bool bFar = qFar != null && !float.IsNaN(mFar) && SagComp_TiltLevers(mAct, c, ids, qFar, out g12f, out g13f);
                CSagTiltFit fit = SagPrac_EyeTiltFit(mNear, g12n, g13n, (float)rNear, bFar ? mFar : float.NaN, g12f, g13f, (float)rFar, cfg.EyeTiltConstTolDeg);
                res.EyeTiltFit = fit;
                Func<double, double, double> Old = (a, b) => (mAct != null && mAct.HasWristTilt) ? mAct.WristTiltAt(a, b) : 0.0;
                double d = Math.Abs(fit.TauAt(g12n, g13n) - Old(g12n, g13n));
                if (bFar) d = Math.Max(d, Math.Abs(fit.TauAt(g12f, g13f) - Old(g12f, g13f)));
                L(string.Format(ci, "공구 기울기 맞춤: τ 가까운 자리 {0:+0.00;-0.00;0.00}° (r{1:0}){2} — {3} → {4} · 지금 걸린 보정과 차 최대 {5:0.00}°",
                    fit.TauNear, rNear, bFar ? string.Format(ci, ", 먼 자리 {0:+0.00;-0.00;0.00}° (r{1:0})", fit.TauFar, rFar) : "", fit.Why, fit.Text(), d));
                if (d <= cfg.EyeTiltSameDeg)
                {
                    if (bDry) { res.EyeAns = "3D 만"; L("3D 만 — 지금 걸린 공구 기울기 보정과 같음 — 미리 보기 끝 (기록하지 않음)"); return ESagEyeOut.Yes; }
                    res.EyeAns = "예";
                    if (!SagPrac_SaveEye(strUrdfDir, eyeDef, eyeMap, strSig, L)) { why = "수직 눈 확인 기록을 쓰지 못함 — 다시 하십시오"; return ESagEyeOut.Fail; }
                    L("수직 눈 확인: 수직 — 기록함 (" + SagPrac_TiltTextOf(mAct) + " 그대로 — 실습 진행은 그대로)");
                    return ESagEyeOut.Yes;
                }
                res.EyeAns = "기울기";
                L((bDry ? "3D 만 미리 보기 — " : "") + "공구 기울기 보정 제안: " + fit.Text() + " (지금 " + SagPrac_TiltTextOf(mAct) + ")" +
                  (bDry ? " — 적용·기록하지 않음" : " — 원위치 뒤 앱이 묻고 넣음 (아직 기록하지 않음)"));
                return ESagEyeOut.Tilt;
            }

            // 손 미세 조정 (실물·3D 만 — 작업 스레드). 창(eyeFine)이 요청을 주면: Nudge = 손목 명령 δ 를 ±(1° 까지) — 합 ±EyeFineMaxDeg, 관절 한계 안,
            //   관절 보간 EyeFineMoveMs (SagArm_PlayQ — 한 틱 관절 변화 막이·싱크·도착 확인 그대로), 움직인 뒤 엔코더 기울기를 다시 읽어 보인다. Zero = δ 를 0 으로.
            //   Vertical ([수직임]): 그때 엔코더로 본 공구 기울기 (3D 만은 δ) 를 mVertical 로 돌려준다 (Yes — 기록·맞춤은 부른 쪽 SagPrac_EyeConclude, 10-15).
            //   Cancel = 기록 없이, Stop·중단 요청 = 중단. 창은 늘 닫는다 (v.Done). 끝나면 δ 를 0 자리로 되돌린다 (중단·실패 제외 — 원위치가 그 자리에서)
            private ESagEyeOut SagPrac_EyeFine(CSagArm arm, CSagPracCfg cfg, DSagEyeFine eyeFine, float[] q0, CSagPracResult res, CSim2Real.DLog L, CSim2Real.DIsStopped stop,
                                               out string why, out float mVertical)
            {
                why = "";
                mVertical = float.NaN;
                var ci = CultureInfo.InvariantCulture;
                CSagArmCfg c = arm.Cfg;
                bool bDry = arm.IsDry;
                int nMove = Math.Max(300, (int)(Math.Max(300, cfg.EyeFineMoveMs) / Math.Max(0.1f, arm.Speed)));
                var v = new CSagEyeFineView { WristID = c.WristID, MaxDeg = cfg.EyeFineMaxDeg, Dry = bDry };
                float off = 0, step;
                ESagEyeOut o = ESagEyeOut.Cancel;
                if (!bDry) v.TiltDeg = SagPrac_EyeTiltRead(arm, c.DownSumDeg);
                L("진행 수직 눈 확인 — 손목 미세 조정 (±0.1·0.5°, [수직임]·[취소])");
                try
                {
                    while (true)
                    {
                        if (SagRun_Stopped(stop)) { res.EyeAns = "중단"; o = ESagEyeOut.Stop; L("중단 요청 — 손목 미세 조정에서 멈춤, 원위치"); break; }
                        string gw = SagArm_GuardWhy(arm);                             // 장면의 막이 — 엔코더 읽기가 멈춤/실패 등 (10-16) — 더 묻지 않고 원위치
                        if (gw.Length > 0) { why = gw; o = ESagEyeOut.Fail; L("★" + gw + " — 손목 미세 조정에서 멈춤, 원위치"); break; }
                        v.OffsetDeg = off; v.Busy = false; v.Text = SagPrac_EyeFineText(v);
                        ESagEyeFineReq rq;
                        try { rq = eyeFine(v, 250, out step); }
                        catch (Exception ex) { L("★미세 조정 창 오류: " + SagRun_Msg(ex)); rq = ESagEyeFineReq.Stop; step = 0; }
                        if (rq == ESagEyeFineReq.None) continue;
                        v.Note = "";
                        if (rq == ESagEyeFineReq.Stop) { res.EyeAns = "중단"; o = ESagEyeOut.Stop; L("중단 — 손목 미세 조정에서 멈춤, 원위치"); break; }
                        if (rq == ESagEyeFineReq.Cancel) { res.EyeAns = "취소"; o = ESagEyeOut.Cancel; L("수직 눈 확인 — 미세 조정 취소 (기록하지 않음), 원위치"); break; }
                        if (rq == ESagEyeFineReq.Nudge || rq == ESagEyeFineReq.Zero)
                        {
                            if (rq == ESagEyeFineReq.Nudge && (float.IsNaN(step) || float.IsInfinity(step) || Math.Abs(step) > 1.0001f)) { v.Note = "★한 번에 1° 까지"; continue; }
                            float want = rq == ESagEyeFineReq.Zero ? 0f : (float)Math.Round(off + step, 3);
                            if (Math.Abs(want) > cfg.EyeFineMaxDeg + 1e-4f) { v.Note = string.Format(ci, "★한도 ±{0:0.#}° — 더 기울이지 않음", cfg.EyeFineMaxDeg); continue; }
                            if (Math.Abs(want - off) < 1e-4f) continue;
                            float[] q = (float[])q0.Clone();
                            q[3] = q0[3] + want;
                            string wl = SagArm_LimitWhy(c, q);
                            if (wl.Length > 0) { v.Note = "★" + wl + " — 움직이지 않음"; continue; }
                            v.Busy = true; v.OffsetDeg = off; v.Text = SagPrac_EyeFineText(v) + string.Format(ci, " — 움직이는 중 (δ → {0:+0.0;-0.0;0.0}°)", want);
                            try { float s0; eyeFine(v, 0, out s0); } catch { }
                            if (!SagArm_PlayQ(arm, "손목 미세 조정", q, nMove, 150, L)) { why = SagRun_WhyJoin("손목 미세 조정 실패", arm.LastFail); o = ESagEyeOut.Fail; break; }
                            off = want;
                            if (!bDry) v.TiltDeg = SagPrac_EyeTiltRead(arm, c.DownSumDeg);
                            L(string.Format(ci, "손목 미세 조정 δ = {0:+0.0;-0.0;0.0}°{1}", off, float.IsNaN(v.TiltDeg) ? "" : string.Format(ci, " — 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}°", v.TiltDeg)));
                            continue;
                        }
                        if (rq != ESagEyeFineReq.Vertical) continue;
                        // [수직임] — 그때의 엔코더 공구 기울기 (3D 만은 δ)
                        float tilt = bDry ? off : SagPrac_EyeTiltRead(arm, c.DownSumDeg);
                        if (float.IsNaN(tilt)) { v.Note = "★엔코더를 읽지 못함 — 다시 [수직임] 을 누르십시오"; continue; }
                        res.EyeOffsetDeg = off;
                        mVertical = tilt;
                        L(string.Format(ci, "{0}[수직임] — 손목 미세 조정 δ {1:+0.0;-0.0;0.0}°, 엔코더 공구 기울기 {2:+0.00;-0.00;0.00}°", bDry ? "3D 만 — " : "", off, tilt));
                        o = ESagEyeOut.Yes;
                        break;
                    }
                }
                finally
                {
                    v.Done = true; v.Busy = false;
                    try { float s1; eyeFine(v, 0, out s1); } catch { }
                }
                if (o != ESagEyeOut.Stop && o != ESagEyeOut.Fail && Math.Abs(off) > 1e-4f)
                {
                    if (SagArm_PlayQ(arm, "손목 미세 조정 되돌림", q0, nMove, 150, L)) L("손목 미세 조정 되돌림 (δ = 0)");
                }
                return o;
            }
            // 미세 조정 창 상태 한 줄
            private static string SagPrac_EyeFineText(CSagEyeFineView v)
            {
                var ci = CultureInfo.InvariantCulture;
                return string.Format(ci, "손목(ID {0}) 미세 조정 δ = {1:+0.0;-0.0;0.0}° (한도 ±{2:0.#}°){3} · + = 공구 끝이 베이스 쪽으로", v.WristID, v.OffsetDeg, v.MaxDeg,
                    v.Dry ? " · 3D 만" : (float.IsNaN(v.TiltDeg) ? " · 엔코더 기울기 —" : string.Format(ci, " · 엔코더 공구 기울기 {0:+0.00;-0.00;0.00}°", v.TiltDeg)));
            }

            // ── 수직 눈 확인만 (10-05) — 이미 쓰던 설치가 실습 1단계를 다시 하지 않고 기록을 남기는 길 ──
            private const string SAGPRAC_EYE_OWNER = "수직 눈 확인";
            /// <summary>수직 눈 확인만 — 실물 (SagPrac_EyeJob(conn, armCfg, strUrdfDir, false, eyeAsk, log, stop) 와 같다)</summary>
            public CSagPracResult SagPrac_EyeJob(CArmConn_t conn, CSagArmCfg armCfg, string strUrdfDir, DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_EyeJob(conn, armCfg, null, strUrdfDir, false, eyeAsk, null, log, stop);
            }
            /// <summary>수직 눈 확인만 — 기본 자리 (CSagPracCfg 기본값), 미세 조정 창 없음 (아래 겹함수와 같다)</summary>
            public CSagPracResult SagPrac_EyeJob(CArmConn_t conn, CSagArmCfg armCfg, string strUrdfDir, bool bDry, DSagEyeAsk eyeAsk, CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                return SagPrac_EyeJob(conn, armCfg, null, strUrdfDir, bDry, eyeAsk, null, log, stop);
            }
            /// <summary>수직 눈 확인만 (블로킹 — 작업 스레드, 10-05 · 10-13). 이미 쓰던 설치가 실습 1단계를 다시 하지 않고 눈 확인 기록을 남기는 길
            /// (10-15 부터 선택 사항 — 어느 작업도 이 기록을 요구하지 않는다. 공구 기울기 보정을 배우는 길).
            /// 순서: 작업 입구 확인 (다른 작업 중·urdf 폴더·연결) → 로봇 정의 확인 (SagDef_Gate — 틀리면 그 까닭, 수직 관절 합은 자동이면 3D FK 로 잰 값) →
            /// 실물인데 묻는 곳(eyeAsk)이 없으면 움직이지 않고 거절 → SagArm_Open (높이·정의·연결·잠금·휴지 자세·토크 시작 상태 — 다른 작업과 같은 관문) →
            /// 펼치기 (SagArm_Unfold — 베이스 가까운 눈 확인 자리 cfg.EyeRadius·EyeYaw 위, 옮기는 높이, 바닥으로 수직 자세) → 눈 확인 한 번 (SagPrac_EyeAt —
            /// 실물은 엔코더로 공구를 수직으로 붙잡고(자동 수직 유지 — 기준 = 수직 − 걸린 공구 기울기 보정) 묻는다: 예 / 아니오 = 손 미세 조정 (eyeFine) → [수직임].
            /// cfg.EyeFar 면 먼 자리에서도 같은 일. 잰 엔코더 기울기로 손목 공구 기울기 보정을 맞춰 (10-15) 지금 걸린 보정과 같으면 기록, 다르면 제안
            /// (res.EyeTiltFit, EyeAns "기울기" — 기록하지 않음, 앱이 묻고 SagRun_TiltApply 뒤 SagPrac_EyeRecord), [취소] = 기록 없음) → SagArm_Close
            /// (손목 유지 놓기·원위치·토크 시작 상태·잠금 놓기). 바닥을 누르는 함수는 부르지 않는다. 3D 만 (bDry): 3D 팔만 같은 자리로 움직여 보이고 미세 조정·기울기 맞춤까지
            /// 미리 보기 (기록·적용 없음). cfg = 눈 확인 자리·유지·미세 조정 설정 (null = 기본값). 작업 기록은 <urdf>\sagcomp_runs\<시각>_eye[_3d]\ (run.log, rest_pose.txt).
            /// 결과: Step 0, Ok = 실물은 기록함 (3D 만은 미리 보기를 마침), EyeAns, EyeTilt*·EyeHoldDeg·EyeFarTiltDeg·EyeOffsetDeg·EyeTiltFit, Why, Stopped, Ran, RestOk, RunDir, Line. 예외 없음.</summary>
            public CSagPracResult SagPrac_EyeJob(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bDry, DSagEyeAsk eyeAsk, DSagEyeFine eyeFine,
                                                 CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                using (SagRun_KinUse()) return SagPrac_EyeJobBody(conn, armCfg, cfg, strUrdfDir, bDry, eyeAsk, eyeFine, log, stop);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagPracResult SagPrac_EyeJobBody(CArmConn_t conn, CSagArmCfg armCfg, CSagPracCfg cfg, string strUrdfDir, bool bDry, DSagEyeAsk eyeAsk, DSagEyeFine eyeFine,
                                                      CSim2Real.DLog log, CSim2Real.DIsStopped stop)
            {
                var res = new CSagPracResult { Step = 0, Dry = bDry };
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                CSagArm arm = null;
                var ci = CultureInfo.InvariantCulture;
                try
                {
                    armCfg = SagDef_Eff(armCfg);                            // 사본 — 수직 관절 합이 자동이면 3D FK 로 잰 값 (10-13, 못 재면 정의 확인이 거절)
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 폴더를 만들거나 움직이기 전에 (작업 입구 SagArm_Open 도 본다)
                    if (armCfg.NoWrist) return SagPrac_Refuse(res, SAGPRAC_EYE_NA + " — 공구 기울기가 자세마다 바뀌어 '바닥으로 수직' 자세가 없음 (실물 작업 입구는 이 확인 없이 통과)", L);
                    if (!bDry && eyeAsk == null) return SagPrac_Refuse(res, "묻는 곳 없음 — 수직 눈 확인은 사람이 실물 펜을 보고 답해야 함 (움직이지 않음)", L);
                    if (!(cfg.EyeRadius >= 50 && cfg.EyeRadius <= 300) || float.IsNaN(cfg.EyeYaw) || Math.Abs(cfg.EyeYaw) > 180)
                        return SagPrac_Refuse(res, string.Format(ci, "눈 확인 자리 (반경 {0}, 요 {1}) 가 범위 밖 — 반경 50~300, 요 ±180", cfg.EyeRadius, cfg.EyeYaw), L);
                    string eyeDef, eyeMap, eyeText;
                    bool bNeed = SagPrac_EyeNeededCore(strUrdfDir, bDry ? null : conn, armCfg, out eyeDef, out eyeMap, out eyeText);
                    double ay = cfg.EyeYaw * Math.PI / 180.0;
                    float fEx = (float)(cfg.EyeRadius * Math.Cos(ay)), fEy = (float)(cfg.EyeRadius * Math.Sin(ay));
                    DateTime t0 = DateTime.Now;
                    res.RunDir = SagRun_NewRunDir(strUrdfDir, t0, bDry ? "eye_3d" : "eye");
                    L = SagRun_SafeLog(log, Path.Combine(res.RunDir, "run.log"));
                    L((bDry ? "3D 만 — " : "") + "수직 눈 확인 시작 — " + t0.ToString("yyyy-MM-dd HH:mm:ss", ci) + (bDry ? " (미리 보기 — 기록하지 않음)" : (bNeed ? " (확인 전)" : " (" + eyeText + " — 다시 확인)")));
                    L(string.Format(ci, "팔을 베이스 가까운 자리 r={0:0.#} 요 {1:0.#} 위 옮기는 높이에서 '바닥으로 수직' 자세로 펼쳐 묻습니다 — 바닥은 누르지 않고 실습 진행은 그대로", cfg.EyeRadius, cfg.EyeYaw));
                    res.UncheckedAxes = SagArm_UncheckedAxes(bDry ? null : conn, armCfg);
                    arm = SagArm_Open(conn, armCfg, res.RunDir, SAGPRAC_EYE_OWNER, bDry, L, out why);
                    if (arm == null) return SagPrac_Refuse(res, why, L);
                    res.Ran = true;
                    if (!SagArm_Unfold(arm, fEx, fEy, float.NaN, L, stop))
                    {
                        res.Stopped = SagRun_Stopped(stop) || arm.StopSeen;
                        if (!res.Stopped) res.Why = SagRun_WhyJoin("펼치기 실패", arm.LastFail);
                    }
                    else if (SagRun_Stopped(stop)) { res.Stopped = true; res.EyeAns = "중단"; L("중단 요청 — 수직 눈 확인 전에 멈춤, 원위치로 돌아가 토크를 시작 상태로"); }
                    else
                    {
                        string whyEye;
                        ESagEyeOut o = SagPrac_EyeAt(arm, cfg, eyeAsk, eyeFine, 0, strUrdfDir, eyeDef, eyeMap, res, L, stop, out whyEye);
                        if (o == ESagEyeOut.Stop) { res.Stopped = true; res.EyeAns = "중단"; L("중단 요청 — 수직 눈 확인에서 멈춤, 원위치로 돌아가 토크를 시작 상태로"); }
                        else if (o == ESagEyeOut.Fail || o == ESagEyeOut.No) res.Why = whyEye;
                        if (res.Why.Length > 0) L("★" + res.Why);
                    }
                }
                catch (Exception ex) { res.Why = SagRun_WhyJoin(res.Why, "★오류: " + SagRun_Msg(ex)); L("★오류: " + SagRun_Msg(ex)); }
                finally { if (arm != null) res.RestOk = SagArm_Close(arm, L); }
                SagPrac_FinishEye(res, L);
                return res;
            }
            // 눈 확인만 — 끝 줄 (Ok = 실물은 기록함, 3D 만은 미리 보기를 마침 — 공구 기울기 보정 미리 보기 포함). 시작하지 않았으면 (SagPrac_Refuse 가 줄을 씀 — 아니면 오류 까닭으로) 그대로
            private static void SagPrac_FinishEye(CSagPracResult res, CSim2Real.DLog L)
            {
                if (!res.Ran)
                {
                    if (res.Line.Length == 0 && res.Why.Length > 0) { res.Line = "시작하지 않음 — " + res.Why; res.Report = res.Line; }
                    return;
                }
                bool bTilt = res.EyeAns == "기울기";
                res.Ok = !res.Stopped && res.Why.Length == 0 && (res.Dry || res.EyeAns == "예");   // 3D 만은 미리 보기를 마쳤으면 (어느 답이든 — 기록·적용 없음)
                string strRest = res.Dry ? "" : (res.RestOk ? "" : " (★원위치 확인 못 함)");
                string strFit = (res.EyeTiltFit != null) ? res.EyeTiltFit.Text() : "";
                if (res.Dry) res.Line = res.Ok ? (bTilt ? "3D 만 · " + strFit + " 미리 보기 (적용·기록하지 않음)"
                                                        : (res.EyeAns == "취소" ? "3D 만 · 미세 조정 취소 — 기록하지 않음" : "3D 만 · 수직 자세 미리 보기 — 기록하지 않음"))
                                              : (res.Stopped ? "수직 눈 확인 (3D 만) 중단" : "수직 눈 확인 (3D 만) 실패 — " + res.Why);
                else if (res.Ok) res.Line = "수직 눈 확인 끝 — 기록함 (실습 진행은 그대로)" + strRest;
                else if (res.Stopped) res.Line = "수직 눈 확인 중단 — 기록하지 않음" + strRest;
                else if (bTilt) res.Line = "수직 눈 확인 — " + strFit + " 제안 — 넣은 뒤 기록 (아직 기록하지 않음)" + strRest;
                else if (res.EyeAns == "취소") res.Line = "수직 눈 확인 취소 — 기록하지 않음" + strRest;
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
                    why = "학습을 다른 로봇 정의로 했음 (그때 내장 OMX, 지금 " + now + ") — 1. 학습부터";
                    return false;
                }
                if (string.Equals(was, now, StringComparison.Ordinal)) return true;
                why = "학습을 다른 로봇 정의로 했음 (그때 " + was + ", 지금 " + now + ") — 1. 학습부터";
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
            /// 수직 눈 확인은 요구하지 않는다 (10-15 — 선택 사항, 로그 한 줄). 10-15: 실습 페이지는 SagPrac_LearnJob 을 쓴다 (옛 스크립트를 위해 남김). 예외 없음.</summary>
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
                    armCfg = SagDef_Eff(armCfg);                            // 사본 — 수직 관절 합이 자동이면 3D FK 로 잰 값 (10-13, 못 재면 정의 확인이 거절)
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
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, L, out why)) return SagPrac_Refuse(res, why, L);   // 실물 — 로봇 정의 확인 (수직 눈 확인은 선택 — 로그 한 줄, 10-15)
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
                                    else { joint = chk; joint.FloorMapEnabled = false; joint.Predictive = false; L("관절 모델: " + res.JointPath); }
                                }
                            }
                        }
                        // ── (b) 바닥 첫닿음 측정 (관절 모델 켬 — 1차 보정: 예측 보정 끔, 옛 회차 데이터셋의 첫닿음 명령 높이와 같은 조건 10-16) ──
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
                        L("진행 학습 — 바탕 관절 모델 " + Path.GetFileName(res.JointPath) + ", 데이터 " + Path.GetFileName(dsPath) + " 의 이번 회차 " + res.DatasetSession);
                        // 이번 바닥에 맞춘 학습 (10-15 검토 — [1. 학습] 과 같다): 이번 회차 행만, 높이 기준 = 이번 회차 첫닿음 명령 높이
                        var lc = new CSagLearnCfg { DatasetPath = dsPath, BaseModelPath = res.JointPath, UseHold = true, RefMm = float.NaN,
                                                    CurSession = res.DatasetSession, Sessions = 2, PlausMm = cfg.PlausFkMm, CmdLevel = true };
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
                        st.Advised = -1; st.AdviceWhy = "";                             // 새 결과 — [1. 학습] 의 권장은 이 결과의 것이 아니다
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
                int nFailRow = 0;
                for (int i = 0; i < pts.Count; i++)
                {
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "관절 처짐 점 사이"); break; }
                    float[] p = pts[i];
                    float x = (float)(p[0] * Math.Cos(p[1] * Math.PI / 180.0)), y = (float)(p[0] * Math.Sin(p[1] * Math.PI / 180.0));
                    if (!SagArm_MoveSafeCore(arm, x, y, p[2], 90, p[2], float.NaN, run.L, run.Stop))
                    {
                        if (arm.StopSeen) { SagPrac_StopLog(run, "관절 처짐 점으로 옮기던 다리를 마친 자리"); break; }
                        string wf = SagRun_WhyJoin(string.Format(ci, "관절 처짐 측정 점으로 못 감 (r{0:0.#} z{1:0.#})", p[0], p[2]), arm.LastFail);
                        if (SagArm_GuardWhy(arm).Length > 0 || ++nFailRow >= 2) { run.Why = wf; break; }   // 안전 막이·연달아 두 번이면 멈춤 (10-15 — 한 점은 건너뜀)
                        run.L("  ★" + wf + " — 이 점은 건너뜀");
                        continue;
                    }
                    nFailRow = 0;
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
                if (cmd.Length > ids.Length) { float[] t = new float[ids.Length]; Array.Copy(cmd, t, ids.Length); cmd = t; }   // 꺾는 손목 없음 — 4칸 계산 자리에서 움직이는 관절만 (10-13)
                if (act.Length > ids.Length) { float[] t = new float[ids.Length]; Array.Copy(act, t, ids.Length); act = t; }
                if (dir != null && dir.Length > ids.Length) { float[] t = new float[ids.Length]; Array.Copy(dir, t, ids.Length); dir = t; }
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
                                       DownSumDeg = arm.DownSumDeg, KinSig = SagRun_KinSig(SagRun_KinState(), arm.FuncNumber),   // 로봇 정의 (이름·수직 관절 합·기구학 표시)
                                       MaxAbsJointDeg = CSagComp.LEARN_MAX_JOINT_DEG };                                          // 관절 보정 상한 12° (10-16 — 결과 모델이 이어받는다)
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
            //   한도 = (1단계 높이 − 관절 모델이 올리는 양 — 내려가는 방향, SagPrac_Predict 와 같다) − 아래. 누름 모델 = 관절 모델, 손목 유지 DataWristHold, 첫닿음에서 멈춤. 첫닿음 행 → 데이터셋에 덧붙임.
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
                int nDone = 0, nFailRow = 0, nRedo = 0, nOut = 0;
                int nNeed = Math.Max(1, run.Cfg.LearnMinData);
                for (int di = 0; di < data.Count; di++)
                {
                    CSagPracPoint p = data[di];
                    if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, "점 사이"); break; }
                    // 일찍 멈춤 (10-15) — 남은 점을 모두 받아들여도 학습에 필요한 수가 안 되면 더 누르지 않는다 (잰 행은 아래에서 덧붙인다)
                    int nGood = 0; for (int k = 0; k < di; k++) if (!float.IsNaN(data[k].Touch)) nGood++;
                    if (nGood + (data.Count - di) < nNeed)
                    {
                        string w = string.Format(ci, "바닥 첫닿음이 학습에 필요한 {0}개가 될 수 없음 — 받아들인 점 {1}, 남은 점 {2} (그럴듯함으로 뺀 점 {3}, 재지 못한 점 {4}) — 남은 점은 누르지 않고 멈춤",
                                                 nNeed, nGood, data.Count - di, nOut, di - nGood - nOut);
                        run.L("★" + w);
                        run.Why = SagRun_WhyJoin(run.Why, w);
                        break;
                    }
                    float u; float h = SagPrac_Estimate(known, p.R, p.Yaw, run.Cfg.UnknownMm, out u);
                    if (float.IsNaN(h)) { p.Note = "예상 높이 없음 — 뺌"; continue; }
                    // 관절 모델이 끝을 올리는 양 — 내려가는 방향 (데드밴드 항까지, SagPrac_Predict 와 같다 — 첫닿음은 내려가며 잰다).
                    //   10-15 검토: 방향 0 으로 낸 양 (먼 쪽 14~18mm) 은 실제 (h − 관절 모델 켬 첫닿음, r215~230 1.9~4.1mm) 보다 커 예상이 첫닿음보다 12mm 까지 낮았다 —
                    //   공중에서 걸린 점의 다시 누름 창 시작 (예상 + 12) 이 첫닿음과 같거나 아래 (10-04 r230 −3.0mm, 시작까지는 감시 없이 내려간다), 첫 누름 한도는 첫닿음 −17 ~ −27mm.
                    //   내려가는 방향 (6~10mm) 이면 같은 두 작업에서 다시 누름 시작이 첫닿음 +6.4 ~ +13.7mm, 한도 −10 ~ −18mm
                    float lift = SagPrac_Lift(kin, run.Arm.Cfg, joint, p.X, p.Y, h, p.Pitch, true);
                    float exp = h - Math.Max(0f, lift);
                    p.Est = exp;
                    CFloorPlanRow row;
                    ESagPracPress o = SagPrac_PressAt(run, p, h, exp, u, joint, run.Cfg.DataWristHold, float.NaN, out row);
                    if (o == ESagPracPress.Stop) break;
                    if (o == ESagPracPress.Fail) { if (SagPrac_SkipFail(run, p, ref nFailRow)) continue; break; }   // 한 점을 옮기지 못하면 그 점만 뺀다 (10-15)
                    if (o != ESagPracPress.Ok) continue;
                    nFailRow = 0;
                    nDone++;
                    if (!float.IsNaN(p.Touch))
                    {
                        // 그럴듯함 (10-15) — 첫닿음 FK 가 기준 점의 FK 추세와 맞나 (명령 높이는 관절 모델을 켜 기준과 다르다 — 보지 않음). 벗어나면 예상 높이 창으로 한 번 다시.
                        //   다시 누름 창 시작은 이 단계에서 이미 받아들인 첫닿음 (관절 모델 켬) 명령 높이의 추세 아래로 잡지 않는다 (10-15 검토 — 관절 모델의 예상이 빗나가도
                        //   시작이 닿는 높이 아래로 가지 않게. 받아들인 점이 없으면 예상만)
                        float eOn; string hOn;
                        float onTrend = SagPrac_TrendAt(data.FindAll(k => !ReferenceEquals(k, p) && !float.IsNaN(k.Touch)), p.R, false, run.Cfg, out eOn, out hOn);
                        bool bOk; CFloorPlanRow r2;
                        o = SagPrac_PlausPress(run, p, known, false, exp, joint, run.Cfg.DataWristHold, ref nRedo, ref nOut, out bOk, out r2, onTrend);
                        if (r2 != null) row = r2;
                        if (o == ESagPracPress.Stop) break;
                        if (o == ESagPracPress.Fail && !SagPrac_SkipFail(run, p, ref nFailRow)) break;
                    }
                    if (row != null) { row.PredZ = exp; rows.Add(row); }
                    run.L(string.Format(ci, "진행 바닥 {0}/{1} — r={2:0.#} 요 {3:0.#}: {4}", nDone, data.Count, p.R, p.Yaw,
                        float.IsNaN(p.Touch) ? "닿지 않음 (" + p.Note + ")" : string.Format(ci, "첫닿음 {0} 명령 {1:F1} FK {2:F1}", p.Kind.Length > 0 ? p.Kind : "—", p.Touch, p.Fk)));
                }
                res.DataRows = nDone; res.NDataRedo = nRedo; res.NDataOut = nOut;
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
                res.DatasetSession = strRound;                                    // 학습은 이 회차 행만 쓴다 (10-15)
                string rowsCsv = SagRun_DatasetRows(cs, strRound, true);
                SagRun_WriteText(Path.Combine(run.RunDir, "floor_dataset_run.csv"), rowsCsv);
                int nAdd = cs.Count > 0 ? SagRun_AppendDataset(dsPath, rowsCsv) : 0;
                if (nAdd < 0) { run.L("★데이터셋에 덧붙이지 못함: " + dsPath); run.Why = SagRun_WhyJoin(run.Why, "★데이터셋에 덧붙이지 못함"); nAdd = 0; }
                res.DatasetRowsAdded = nAdd;
                run.L(string.Format(ci, "바닥 첫닿음: 누른 점 {0} → 첫닿음 {1} (재지 못함 {2} — 그 가운데 그럴듯함으로 뺀 점 {6}) — 데이터셋 +{3}행 (회차 {4}, {5})", nDone, cs.Count, nDone - cs.Count,
                    nAdd, strRound, Path.GetFileName(dsPath), nOut));
                return nAdd > 0;
            }

            // ────────────────────────────────────────────────────────────
            // 3. 효과 보기
            // ────────────────────────────────────────────────────────────

            /// <summary>3단계 효과 보기 (블로킹 — 작업 스레드). 2단계를 마친 실습만. 결과 파일(strResultPath — null 이면 진행 상태의 결과 파일)을 켠 채
            /// 1단계와 같은 점을 같은 순서로 눌러 첫닿음 명령 높이를 잰다 (손목 유지 끔 — 1단계와 같은 조건, 옮기기는 결과 파일의 관절 모델).
            /// 먼저 가운데 점 바닥이 1단계와 같은지 본다 (보정 끔). after.csv, 전·후 흩어짐·Ref 기준 RMS·점별 값 (실습 폴더 after_summary.txt),
            /// 결과 파일 옆 <결과>_test.txt 에 [실습] 한 줄. 시험 동안 쌍둥이 = 결과 모델. 수직 눈 확인은 요구하지 않는다 (10-15 — 선택 사항). 10-15: 실습 페이지는 쓰지 않는다. 예외 없음.</summary>
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
                    armCfg = SagDef_Eff(armCfg);                            // 사본 — 수직 관절 합이 자동이면 3D FK 로 잰 값 (10-13, 못 재면 정의 확인이 거절)
                    cfg = (cfg == null ? new CSagPracCfg() : cfg).Clone();
                    string why;
                    if (!SagRun_JobPrecheck(conn, bDry, strUrdfDir, out why)) return SagPrac_Refuse(res, why, L);
                    C3d kin = SagRun_Kin();
                    if (kin == null) return SagPrac_Refuse(res, SAGRUN_NO_KIN, L);
                    if (!SagDef_Gate(armCfg, false, null, out why)) return SagPrac_Refuse(res, "로봇 정의 — " + why, L);   // 계산하기 전에 (작업 입구 SagArm_Open 도 본다)
                    st = SagPrac_StateFor(strUrdfDir, 2, bDry, out before, out why);
                    if (st == null) return SagPrac_Refuse(res, why, L);
                    if (!SagPrac_DefMatch(st, armCfg, out why)) return SagPrac_Refuse(res, why, L);   // 1단계와 같은 로봇 정의인가 (state.txt def)
                    if (!SagPrac_EyeGate(strUrdfDir, conn, armCfg, bDry, L, out why)) return SagPrac_Refuse(res, why, L);   // 실물 — 로봇 정의 확인 (수직 눈 확인은 선택 — 로그 한 줄, 10-15)
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
                                if (!bDry && o == ESagPracPress.Ok) SagPrac_FkGate(p, b, known, cfg, L);   // 바닥이 아닌 데서 걸린 첫닿음은 뺀다 (1단계 FK 추세와 — 10-15)
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
            // 3단계 첫닿음 높이 확인 — 같은 점·같은 바닥이면 첫닿음 순간의 엔코더 FK 높이는 1단계와 거의 같다 (10-04 실측: 네 점 ±1mm).
            //   fTol 넘게 다르면 바닥이 아닌 데서 검출기가 걸린 것으로 보고 뺀다 (10-04 r250: FK 74.6, 1단계 46.4 — 공중에서 보조 판정이 걸림).
            //   뺀 점은 Touch NaN (흩어짐에서 빠지고 수가 모자라면 3단계 실패), FK 는 after.csv 에 남긴다
            //   10-15: 기준은 1단계 받아들인 점들의 FK 추세 (SagPrac_TrendAt — 학습의 그럴듯함과 같은 추세, 넘겨짚기 여유 포함) — 같은 점의 1단계 값 한 개보다 덜 흔들린다.
            //   1단계 점이 없으면 (또는 추세를 못 내면) 예전처럼 같은 점의 1단계 FK. 문턱은 FkTolMm (보정을 켜 자세가 조금 달라 학습의 PlausFkMm 보다 넓다)
            private static void SagPrac_FkGate(CSagPracPoint p, CSagPracPoint b, List<CSagPracPoint> beforeKnown, CSagPracCfg cfg, CSim2Real.DLog L)
            {
                if (cfg == null) cfg = new CSagPracCfg();
                float fTol = cfg.FkTolMm;
                if (p == null || fTol <= 0 || float.IsNaN(p.Touch) || float.IsNaN(p.Fk)) return;
                float ext; string how;
                float refFk = SagPrac_TrendAt(beforeKnown, p.R, true, cfg, out ext, out how);
                string strRef = "1단계 FK 추세";
                if (float.IsNaN(refFk)) { ext = 0; refFk = b == null ? float.NaN : b.Fk; strRef = "1단계 FK"; }
                if (float.IsNaN(refFk)) return;
                float d = p.Fk - refFk, tol = fTol + ext;
                if (Math.Abs(d) <= tol) return;
                var ci = CultureInfo.InvariantCulture;
                string w = string.Format(ci, "첫닿음 FK {0:F1} 이 {4} {1:F1} 와 {2:+0.0;-0.0}mm 다름 (허용 ±{3:0.#}) — 바닥이 아닌 데서 걸린 것으로 보고 뺌", p.Fk, refFk, d, tol, strRef);
                p.Touch = float.NaN; p.Kind = "";
                p.Note = SagRun_WhyJoin(p.Note, w);
                if (L != null) L(string.Format(ci, "  ★r={0:0.#} 요 {1:0.#}: {2}", p.R, p.Yaw, w));
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
                        st.Advised = -1; st.AdviceWhy = "";                             // 잰 효과 (after.csv) 가 생겼다 — 권장은 그래프 규칙이 잰 값으로 다시 낸다
                        SagPrac_SaveState(strUrdfDir, st, L);
                    }
                    L(res.Line);
                }
                catch (Exception ex) { if (L != null) L("★3단계 정리 오류: " + SagRun_Msg(ex)); }
            }

            // ────────────────────────────────────────────────────────────
            // 4. 적용
            // ────────────────────────────────────────────────────────────

            /// <summary>적용 (UI 스레드 — SagRun_Activate 규칙). 결과가 있는 실물 실습 (진행 상태 step ≥ 2 — [1. 학습] 끝, 옛 2·3단계 끝) 의 결과 파일을 적용한다
            /// (적용 중 파일을 바꿔 끼움 + 1초 전환 — 지난 파일은 SagRun_Activate 가 스스로 보관한다). 3D 만 실습은 적용하지 않는다.
            /// 효과 (예상 또는 옛 3단계 잰 값 — SagPrac_ChartData 의 ApplyOk·ApplyWhy) 는 권장일 뿐 막지 않는다 (10-15 — 앱이 묻는 기본 답을 정한다).
            /// 앱은 시작할 때 적용 중 파일을 읽으므로 그 뒤 모든 동작이 보정된다. 반환 = 적용됨. 예외 없음.</summary>
            public bool SagPrac_Apply(string strUrdfDir, string strMotorMapKey, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                return SagPrac_Apply(strUrdfDir, null, strMotorMapKey, log, out strBackupPath, out report);
            }
            /// <summary>적용 — strResultPath = 방금 끝난 작업이 돌려준 결과 파일 (CSagPracResult.ResultPath, 10-15 검토). 진행 상태의 결과 파일이 그것과 같은 파일이어야 적용한다
            /// (상태를 쓰지 못했으면 진행 상태는 옛 결과를 가리킨다 — 옛 결과를 적용하지 않는다). null·빈 값 = 진행 상태의 결과 (옛 겹함수). 나머지는 위와 같다</summary>
            public bool SagPrac_Apply(string strUrdfDir, string strResultPath, string strMotorMapKey, CSim2Real.DLog log, out string strBackupPath, out string report)
            {
                strBackupPath = null; report = "";
                CSim2Real.DLog L = SagRun_SafeLog(log, null);
                try
                {
                    CSagPracState st = SagPrac_LoadState(strUrdfDir);
                    if (!string.IsNullOrEmpty(strResultPath) && (string.IsNullOrEmpty(st.ResultPath) || !SagRun_SamePath(st.ResultPath, strResultPath)))
                    {
                        report = "진행 상태의 결과 파일 (" + (string.IsNullOrEmpty(st.ResultPath) ? "없음" : Path.GetFileName(st.ResultPath)) + ") 이 이번 학습의 결과 (" + Path.GetFileName(strResultPath) +
                                 ") 와 다릅니다 — 진행 상태를 쓰지 못했을 수 있어 적용하지 않았습니다 (결과 파일은 고급 페이지 ⑥ 에서 골라 적용할 수 있습니다)";
                        L("★" + report);
                        return false;
                    }
                    if (st.Step < 2) { report = "1. 학습을 먼저 하십시오"; return false; }
                    if (st.Dry) { report = "3D 만 실습의 결과(가상 로봇)는 실물에 적용하지 않습니다 — [3D만] 을 끄고 실물로 1. 학습"; return false; }
                    string whyDef;                                                       // 학습 때와 같은 로봇 정의인가 — 정의 = <urdf>\sagarm.txt (없으면 기본값, 파일은 만들지 않는다)
                    if (!SagPrac_DefMatch(st, SagPrac_DefOf(strUrdfDir), out whyDef)) { report = whyDef; return false; }
                    if (string.IsNullOrEmpty(st.ResultPath) || !File.Exists(st.ResultPath)) { report = "결과 파일이 없음: " + st.ResultPath; return false; }
                    CSagPracChart ch = SagPrac_ChartData(strUrdfDir);                    // 효과 — 권장이 아니면 로그에만 (막지 않는다)
                    if (!ch.ApplyOk && ch.ApplyWhy.Length > 0) L("★적용 권장 아님 — " + ch.ApplyWhy + " (사용자가 적용을 고름)");
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

            /// <summary>그래프 자료 — 진행 상태의 실습 폴더 before.csv (전 — 보정 끔, 잰 값) 와 '후': 옛 3단계를 마쳤으면 after.csv (잰 값), 아니고 결과가 있으면
            /// predict.csv ([1. 학습] 의 예상 — AfterPredicted, 10-15). 점 순서는 before.csv 그대로. 기준선 = 결과 파일의 Ref — 없으면 '전' 평균.
            /// ApplyOk·ApplyWhy = 적용 권장 — '후' 가 [1. 학습] 의 예상이면 그 작업이 낸 판정 (진행 상태 advised·advicewhy — SagPrac_FinishLearn 한 곳, 10-15 검토).
            /// 그 기록이 없는 옛 상태·옛 3단계 잰 값은 같은 조건으로 다시 낸다: 같은 점 3개 이상, 흩어짐 SAGPRAC_APPLY_MIN_RED % 이상 줄어듦, 결과 모델에 Ref (첫닿음 모델) 가 있고
            /// '후' − Ref 평균 ±SAGPRAC_ADVISE_OFFSET_MM 안·RMS ≤ SAGPRAC_ADVISE_RMS_MM, 결과 파일 머리 주석의 학습 교차검증 ≤ SAGPRAC_ADVISE_CV_MM. 없으면 빈 자료. 예외 없음.</summary>
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
                    List<CSagPracPoint> a = st.Step >= 3 ? SagPrac_ReadPoints(Path.Combine(st.FolderDir, "after.csv")) : null;   // 옛 3단계 — 잰 값
                    if (a == null && st.Step >= 2) { a = SagPrac_ReadPoints(Path.Combine(st.FolderDir, "predict.csv")); ch.AfterPredicted = a != null; }   // [1. 학습] — 예상
                    ch.BeforeLabel = "보정 끔 (학습 때 잰 값)";
                    ch.AfterLabel = ch.AfterPredicted ? "보정 켬 (예상)" : "보정 켬 (잰 값)";
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
                        bool bRefModel = m != null && m.HasFc;
                        if (bRefModel && va.Count > 0)                                   // 보정 켬 첫닿음 − 기준선 (10-15 — 펜이 Ref 에서 뜨는(−)·누르는(+) 양)
                        {
                            ch.Offset = SagPrac_Mean(va) - ch.RefZ;
                            ch.OffsetRms = SagPrac_Rms(va, ch.RefZ);
                        }
                        ch.Line = (ch.AfterPredicted ? "예상 — " : "") + SagPrac_SpreadText(ch.SpreadBefore, ch.SpreadAfter, ch.Reduction) +
                                  (double.IsNaN(ch.Offset) ? "" : string.Format(ci, " · 첫닿음 − Ref {0:+0.0;-0.0}mm", ch.Offset));
                        double cvRes = double.NaN;
                        if (va.Count < 3) ch.ApplyWhy = string.Format(ci, "전·후 둘 다 있는 점이 {0}개 — 3개 이상이어야 권장", va.Count);
                        else if (double.IsNaN(ch.Reduction) || ch.Reduction < SAGPRAC_APPLY_MIN_RED)
                            ch.ApplyWhy = string.Format(ci, "보정을 켜도 흩어짐이 {0}{2} — {1:0}% 이상 줄어야 권장합니다. 1. 학습을 다시 (정밀 권장)",
                                (!double.IsNaN(ch.Reduction) && ch.Reduction < 0) ? "늘어남" : "충분히 줄지 않음", SAGPRAC_APPLY_MIN_RED, ch.AfterPredicted ? " (예상)" : "");
                        else if (!bRefModel)                                             // [1. 학습] 과 같다 — Ref 가 없으면 Ref 높이로 그릴 때 펜이 어디 닿을지 모른다 (10-15 검토)
                            ch.ApplyWhy = m == null ? "결과 파일을 읽지 못함 — 1. 학습을 다시"
                                                    : "결과 모델에 기준선 Ref (첫닿음 모델) 가 없음 — Ref 높이로 그리면 펜이 어디 닿을지 모름. 1. 학습을 다시";
                        else if (double.IsNaN(ch.Offset) || Math.Abs(ch.Offset) > SAGPRAC_ADVISE_OFFSET_MM)
                            ch.ApplyWhy = string.Format(ci, "보정 켬 첫닿음이 Ref 와 평균 {0:+0.0;-0.0}mm{2} — ±{1:0.0}mm 안이어야 권장합니다 (Ref 높이에서 펜이 {3}). 1. 학습을 다시",
                                ch.Offset, SAGPRAC_ADVISE_OFFSET_MM, ch.AfterPredicted ? " (예상)" : "", ch.Offset < 0 ? "뜬다" : "누른다");
                        else if (double.IsNaN(ch.OffsetRms) || ch.OffsetRms > SAGPRAC_ADVISE_RMS_MM)
                            ch.ApplyWhy = string.Format(ci, "보정 켬 첫닿음이 Ref 에서 RMS {0:F1}mm{2} — {1:0.0}mm 이하여야 권장합니다. 1. 학습을 다시 (정밀 권장)",
                                ch.OffsetRms, SAGPRAC_ADVISE_RMS_MM, ch.AfterPredicted ? " (예상)" : "");
                        else if (!double.IsNaN(cvRes = SagPrac_ResultCv(st.ResultPath)) && cvRes > SAGPRAC_ADVISE_CV_MM)   // [1. 학습] 과 같은 교차검증 조건 (결과 파일 머리 주석의 값)
                            ch.ApplyWhy = string.Format(ci, "학습 교차검증 오차 {0:F1}mm 가 {1:0.#}mm 를 넘음 — 바닥 데이터가 고르지 않음 (정밀로 다시 학습 권장)", cvRes, SAGPRAC_ADVISE_CV_MM);
                        // [1. 학습] 의 예상이면 그 작업이 낸 권장 (진행 상태 advised·advicewhy) 을 따른다 — 판정은 SagPrac_FinishLearn 한 곳 (10-15 검토).
                        //   기록이 없는 옛 상태와 옛 3단계 잰 값 (after.csv) 만 위 규칙 (같은 조건)
                        if (ch.AfterPredicted && st.Advised >= 0)
                            ch.ApplyWhy = st.Advised > 0 ? "" : (st.AdviceWhy.Length > 0 ? st.AdviceWhy : "1. 학습이 적용을 권장하지 않음");
                        ch.ApplyOk = ch.ApplyWhy.Length == 0;
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
            //   fAbove = 창 시작을 예상 + 이 값으로 (NaN = 위 규칙 — 그럴듯함 다시 누름은 MinAboveMm: 예상 ± 12 로 추세 높이를 가운데로, 10-15)
            private ESagPracPress SagPrac_PressAt(CSagPracRun run, CSagPracPoint p, float expHi, float expLo, float u, CSagComp model, bool bHold, float fDry, out CFloorPlanRow rowOut,
                                                  float fAbove = float.NaN)
            {
                var ci = CultureInfo.InvariantCulture;
                ESagPracPress o = SagPrac_PressOnce(run, p, expHi, expLo, u, model, bHold, fDry, out rowOut, fAbove);
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
            private ESagPracPress SagPrac_PressOnce(CSagPracRun run, CSagPracPoint p, float expHi, float expLo, float u, CSagComp model, bool bHold, float fDry, out CFloorPlanRow rowOut,
                                                    float fAbove = float.NaN)
            {
                rowOut = null;
                var ci = CultureInfo.InvariantCulture;
                CSagPracCfg cfg = run.Cfg;
                if (float.IsNaN(u) || u < 0) u = 0;
                float zMin = expLo - cfg.BelowMm - u;
                var cand = new List<float>();
                foreach (float a in (float.IsNaN(fAbove) ? new float[] { cfg.AboveMm + u, cfg.AboveMm, cfg.MinAboveMm } : new float[] { Math.Max(fAbove, cfg.MinAboveMm), cfg.MinAboveMm }))
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
                    List<CSagPracTouch> ts = SagPrac_TouchFromTrace(raw, arm.Cfg.NoWrist);          // 손목 없음 = 팔꿈치 잔차·보조 판정 (FloorPress 의 실시간 멈춤과 같은 검출)
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
            //   꺾는 손목이 없는 팔 (NoWrist, 10-13): d14 가 없다. 멈춤 = 보조 판정(위와 같음) + 팔꿈치 잔차 −(a13 − q13) 기울기 (창 4mm, 0.22°/mm 3틱,
            //   오름 0.5°, 하강 시작 4mm 는 보지 않음). 첫닿음 = 팔꿈치 잔차 경첩 (시작 2mm 건너뜀), 못 맞추면 명령−FK 차(−벌어짐) 경첩.
            //   실물 OMX 궤적(손목은 굳힌 것으로 FK 를 다시 낸 것)으로 오프라인 확인 — 공중 멈춤 0, d14 첫닿음과 중앙 |차| 1.3mm·p90 11mm (늦게 잡는 쪽).
            //   팔꿈치 잔차는 XL430 데드밴드로 약해 d14 보다 늦다 — 손목 없는 실제 팔로 다시 확인해야 한다
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
                // 꺾는 손목 없음 (10-13) — 잔차 칸(d14 자리)에 팔꿈치 잔차 −(a13 − q13) 를 넣는다 (Residual). 손목 로봇은 쓰지 않는다
                public bool NoWrist = false;
                public float NwWindowMm = 4f, NwRiseDeg = 0.5f, NwSkipMm = 4f;   // 팔꿈치 잔차 기울기 창·오름·하강 시작에서 보지 않는 길이
                public float NwOnsetSkipMm = 2f;       // 첫닿음 꺾임 맞춤에서 하강 시작 이만큼은 빼고 본다 (시작부터 닿음 = 꺾임이 시작 + 이것 + 2mm 안)
                public string OnsetHow = "";           // 첫닿음 꺾임을 낸 신호 (손목 없음: "팔꿈치 잔차" / "명령−FK")
                /// <summary>시작부터 닿음으로 보는 길이 — 꺾임이 하강 시작 이만큼 안 (손목 2mm, 손목 없음 NwOnsetSkipMm + 2mm)</summary>
                public float StartMm { get { return NoWrist ? NwOnsetSkipMm + 2f : 2f; } }
                /// <summary>Add 의 잔차 값 — 손목 로봇 d14 = a14 − q14 (q·a 넷째 칸), 꺾는 손목 없음 = −(a13 − q13) (셋째 칸, 닿으면 커지게 부호를 뒤집음)</summary>
                public static float Residual(float[] q, float[] a, bool bNoWrist)
                {
                    if (!bNoWrist) return (q.Length > 3 && a.Length > 3) ? a[3] - q[3] : float.NaN;
                    return (q.Length > 2 && a.Length > 2 && !float.IsNaN(q[2]) && !float.IsNaN(a[2])) ? -(a[2] - q[2]) : float.NaN;
                }
                internal readonly List<float[]> S = new List<float[]>();   // 명령, FK, 명령−FK, 어깨 부하, d14 (손목 없음: 팔꿈치 잔차)
                private int m_nRep; private float m_fDmin = float.NaN;
                public bool Add(float cz, float fz, float load12, float d14)
                {
                    if (Fired) return true;
                    if (float.IsNaN(cz) || float.IsNaN(fz) || float.IsInfinity(cz) || float.IsInfinity(fz)) return false;
                    S.Add(new float[] { cz, fz, cz - fz, load12, d14 });
                    float cz0 = S[0][0];
                    float dNow = Med(4, S.Count - 3, S.Count);
                    if (!float.IsNaN(dNow) && (!NoWrist || cz0 - cz >= NwSkipMm)) m_fDmin = float.IsNaN(m_fDmin) ? dNow : Math.Min(m_fDmin, dNow);
                    string why = null;
                    var ci = CultureInfo.InvariantCulture;
                    if (NoWrist)
                    {
                        if (cz0 - cz >= NwWindowMm + NwSkipMm)
                        {
                            double sx = 0, sy = 0, sxx = 0, sxy = 0; int n = 0;
                            for (int i = S.Count - 1; i >= 0; i--)
                            {
                                if (S[i][0] > cz + NwWindowMm || S[i][0] > cz0 - NwSkipMm) continue;
                                if (float.IsNaN(S[i][4])) continue;
                                sx += S[i][0]; sy += S[i][4]; sxx += S[i][0] * S[i][0]; sxy += S[i][0] * S[i][4]; n++;
                            }
                            double rt = double.NaN;
                            if (n >= 5) { double d = n * sxx - sx * sx; if (d > 1e-9) rt = -(n * sxy - sx * sy) / d; }
                            m_nRep = (!double.IsNaN(rt) && rt >= RateDegPerMm) ? m_nRep + 1 : 0;
                            if (m_nRep >= Repeat && !float.IsNaN(m_fDmin) && dNow - m_fDmin >= NwRiseDeg) why = string.Format(ci, "팔꿈치 잔차 {0:F2}°/mm, +{1:F2}°", rt, dNow - m_fDmin);
                        }
                    }
                    else if (cz0 - cz >= WindowMm)
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
                    if (NoWrist) return OnsetNoWrist(out k, out idx);
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
                // 꺾는 손목 없음 — 팔꿈치 잔차 경첩, 못 맞추면 명령−FK 차를 뒤집은 값(닿으면 벌어짐이 줄어 커진다) 경첩. 하강 시작 NwOnsetSkipMm 는 빼고 본다
                private bool OnsetNoWrist(out float k, out int idx)
                {
                    idx = -1;
                    k = Hinge(4, 1f, NwOnsetSkipMm); OnsetHow = "팔꿈치 잔차";
                    if (float.IsNaN(k)) { k = Hinge(2, -1f, NwOnsetSkipMm); OnsetHow = "명령−FK"; }
                    if (float.IsNaN(k)) { OnsetHow = ""; return false; }
                    double bd = double.MaxValue;
                    for (int i = 0; i <= FireIndex; i++) { double dd = Math.Abs(S[i][0] - k); if (dd < bd) { bd = dd; idx = i; } }
                    return true;
                }
                // 경첩 맞춤 (Onset 과 같은 식) — 값 = fSign·S[i][col], 멈춘 자리 위 10mm 안, 하강 시작 fSkip 아래 표본. 꺾임 = 멈춘 자리 위 0.25~8mm. 못 맞추면 NaN
                private float Hinge(int col, float fSign, float fSkip)
                {
                    float czf = S[FireIndex][0], top = S[0][0];
                    var pts = new List<float[]>();
                    for (int i = 0; i <= FireIndex; i++)
                        if (S[i][0] <= czf + 10f && S[i][0] <= top - fSkip && !float.IsNaN(S[i][col])) pts.Add(new float[] { S[i][0], fSign * S[i][col] });
                    double best = double.MaxValue; float k = float.NaN;
                    for (float kk = czf + 0.25f; kk <= Math.Min(czf + 8f, top - fSkip) + 1e-4f; kk += 0.25f)
                    {
                        int nUp = 0, nDn = 0; double sx = 0, sxx = 0, sy = 0, sxy = 0;
                        foreach (float[] p in pts)
                        {
                            if (p[0] >= kk) nUp++; else nDn++;
                            double uu = Math.Max(0.0, kk - p[0]); sx += uu; sxx += uu * uu; sy += p[1]; sxy += uu * p[1];
                        }
                        if (nUp < 4 || nDn < 3) continue;
                        int N = pts.Count; double det = N * sxx - sx * sx;
                        if (det <= 1e-9) continue;
                        double m = (N * sxy - sx * sy) / det, b = (sy - m * sx) / N;
                        if (m <= 0) continue;
                        double se = 0; foreach (float[] p in pts) { double r = p[1] - b - m * Math.Max(0.0, kk - p[0]); se += r * r; }
                        double rms = Math.Sqrt(se / N);
                        if (rms < best - 1e-9) { best = rms; k = kk; }
                    }
                    return k;
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
                return SagPrac_TouchFromTrace(strTraceCsv, false);
            }
            /// <summary>바닥 궤적 다시 읽기 — bNoWrist = 꺾는 손목 없는 팔의 검출 (CSagFtLive.NoWrist: 팔꿈치 잔차·보조 판정, 10-13). false 면 위와 같다</summary>
            public static List<CSagPracTouch> SagPrac_TouchFromTrace(string strTraceCsv, bool bNoWrist)
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
                            ft = new CSagFtLive { NoWrist = bNoWrist }; fds.Clear(); qs.Clear(); acts.Clear();
                        }
                        lastTick = tick; lastR = f[0];
                        if (ft.Fired) continue;                                       // 실시간이면 여기서 멈췄다 — 그 뒤 표본은 보지 않는다
                        float fd = F(f, "fd"); if (float.IsNaN(fd)) fd = 0;
                        float cz = F(f, "cz"), fz = F(f, "fz");
                        float[] q = { F(f, "q1"), F(f, "q2"), F(f, "q3"), F(f, "q4") }, a = { F(f, "a1"), F(f, "a2"), F(f, "a3"), F(f, "a4") };
                        float wh = F(f, "wh"); if (!float.IsNaN(wh)) cur.Hold = true;
                        float d14 = bNoWrist ? CSagFtLive.Residual(q, a, true) : ((float.IsNaN(a[3]) || float.IsNaN(q[3])) ? float.NaN : a[3] - q[3]);
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
                t.StartContact = ft.S[0][0] - k < ft.StartMm;
                if (ft.NoWrist && ft.OnsetHow.Length > 0) t.Why = SagRun_WhyJoin(t.Why, "꺾임: " + ft.OnsetHow);
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

            // ────────────────────────────────────────────────────────────
            // 첫닿음 그럴듯함 (10-15)
            //   바닥은 평평하다 — 첫닿음 순간의 엔코더 FK 높이 (= 바닥 + 엔코더 밖 휨) 와 명령 높이 (보정 끔 = FK + 관절 처짐) 는 반경을 따라 매끄럽고 요와 무관하다
            //   (10-15 실측: 요 ±20 줄도 요 0 줄과 1~2mm 안). 첫닿음 검출기는 하강 시작 13~16mm 아래 (보조 판정이 처음 걸릴 수 있는 자리) 에서
            //   출발 걸림 (데드밴드 — 명령−FK 차가 줄고 어깨 부하가 풀림) 을 닿음으로 잘못 보곤 했다: 요 줄의 첫 점·'시작부터 닿음' 뒤 창을 올린 다시 누름,
            //   그리고 그 잘못된 값으로 넘겨짚은 이웃 점. 그래서 잰 점들로 추세를 맞춰 벗어난 첫닿음은 받아들이지 않는다 — 넘겨짚기에도 쓰지 않는다.
            // ────────────────────────────────────────────────────────────

            /// <summary>첫닿음 그럴듯함 확인 결과 (SagPrac_Plausible)</summary>
            public class CSagPracPlaus
            {
                public bool Ok = true;                                    // 받아들인다 (볼 것이 없어도 true)
                public float PredFk = float.NaN, PredTouch = float.NaN;   // 그 반경의 추세 — FK 높이·명령 높이
                public float DFk = float.NaN, DTouch = float.NaN;         // 잰 값 − 추세
                public float TolFk = float.NaN, TolTouch = float.NaN;     // 그 점의 문턱 (넘겨짚기 여유 포함)
                public string How = "";                                   // 추세를 낸 점 ("추세 6점 (r160~235)" / "가까운 점 r190" / "점 없음")
                public string Why = "";                                   // 벗어난 까닭 (Ok 면 "")
                /// <summary>문턱에 견준 크기 (가장 큰 쪽 |차| ÷ 문턱 — 1 넘으면 벗어남)</summary>
                public double Ratio
                {
                    get
                    {
                        double q = 0;
                        if (!float.IsNaN(DFk) && TolFk > 0) q = Math.Max(q, Math.Abs(DFk) / TolFk);
                        if (!float.IsNaN(DTouch) && TolTouch > 0) q = Math.Max(q, Math.Abs(DTouch) / TolTouch);
                        return q;
                    }
                }
            }

            /// <summary>잰 점(known — Touch 가 수인 점)으로 반경 r 의 추세 (bFk = 첫닿음 FK 높이, 아니면 명령 높이). 반경이 가까운 cfg.PlausNear 점이
            /// cfg.PlausMinPts 점 이상이고 반경이 둘 이상이면 Theil–Sen 직선 (두 점 기울기의 중앙값, 절편 = (값 − 기울기·반경) 의 중앙값 — 벗어난 점 몇 개에 끌리지 않는다),
            /// tolExtra = 잰 반경 범위 밖 거리 × PlausExtrapPerMm. 아니면 가장 가까운 점의 값, tolExtra = 반경 차 × PlausSlope. 점이 없으면 NaN.
            /// 요는 보지 않는다 (요 ±20 도 같은 추세 — 10-15 실측). 예외 없음</summary>
            public static float SagPrac_TrendAt(List<CSagPracPoint> known, float r, bool bFk, CSagPracCfg cfg, out float tolExtra, out string how)
            {
                tolExtra = 0; how = "점 없음";
                try
                {
                    if (cfg == null) cfg = new CSagPracCfg();
                    var pts = new List<float[]>();
                    if (known != null)
                        foreach (CSagPracPoint k in known)
                        {
                            if (k == null || float.IsNaN(k.Touch)) continue;
                            float v = bFk ? k.Fk : k.Touch;
                            if (float.IsNaN(v) || float.IsInfinity(v)) continue;
                            pts.Add(new float[] { k.R, v });
                        }
                    if (pts.Count == 0) return float.NaN;
                    pts.Sort((a, b) => { int c = Math.Abs(a[0] - r).CompareTo(Math.Abs(b[0] - r)); return c != 0 ? c : a[0].CompareTo(b[0]); });
                    int nNear = Math.Max(Math.Max(2, cfg.PlausMinPts), cfg.PlausNear);
                    if (pts.Count > nNear) pts.RemoveRange(nNear, pts.Count - nNear);
                    float rMin = float.MaxValue, rMax = float.MinValue;
                    foreach (float[] p in pts) { rMin = Math.Min(rMin, p[0]); rMax = Math.Max(rMax, p[0]); }
                    var ci = CultureInfo.InvariantCulture;
                    if (pts.Count >= Math.Max(2, cfg.PlausMinPts) && rMax - rMin >= 1f)
                    {
                        var sl = new List<double>();
                        for (int i = 0; i < pts.Count; i++)
                            for (int j = i + 1; j < pts.Count; j++)
                                if (Math.Abs(pts[j][0] - pts[i][0]) >= 0.5f) sl.Add((pts[j][1] - pts[i][1]) / (double)(pts[j][0] - pts[i][0]));
                        double b = SagPrac_Median(sl);
                        var c0 = new List<double>(); foreach (float[] p in pts) c0.Add(p[1] - b * p[0]);
                        double a = SagPrac_Median(c0);
                        tolExtra = Math.Max(0f, Math.Max(rMin - r, r - rMax)) * cfg.PlausExtrapPerMm;
                        how = string.Format(ci, "추세 {0}점 (r{1:0}~{2:0})", pts.Count, rMin, rMax);
                        return (float)(a + b * r);
                    }
                    tolExtra = Math.Abs(pts[0][0] - r) * cfg.PlausSlope;
                    how = string.Format(ci, "가까운 점 r{0:0}", pts[0][0]);
                    return pts[0][1];
                }
                catch { return float.NaN; }
            }

            /// <summary>첫닿음 하나가 그럴듯한가 (10-15) — p 의 FK 높이 (bTouch 면 명령 높이도) 가 잰 점들 (known — p 는 빼고) 의 추세 (SagPrac_TrendAt) 에서
            /// cfg.PlausFkMm (명령 PlausTouchMm) + 넘겨짚기 여유 안이면 Ok. 값이 없는 쪽·문턱 0 인 쪽은 보지 않는다. 잰 점이 없으면 Ok. 예외 없음</summary>
            public static CSagPracPlaus SagPrac_Plausible(List<CSagPracPoint> known, CSagPracPoint p, CSagPracCfg cfg, bool bTouch)
            {
                var res = new CSagPracPlaus();
                try
                {
                    if (cfg == null) cfg = new CSagPracCfg();
                    if (p == null || float.IsNaN(p.Touch)) return res;
                    var others = new List<CSagPracPoint>();
                    if (known != null) foreach (CSagPracPoint k in known) if (!ReferenceEquals(k, p)) others.Add(k);
                    var ci = CultureInfo.InvariantCulture;
                    float ef, et; string hf, ht;
                    res.PredFk = SagPrac_TrendAt(others, p.R, true, cfg, out ef, out hf);
                    res.PredTouch = SagPrac_TrendAt(others, p.R, false, cfg, out et, out ht);
                    res.How = float.IsNaN(res.PredTouch) ? hf : ht;
                    var why = new List<string>();
                    if (cfg.PlausFkMm > 0 && !float.IsNaN(p.Fk) && !float.IsNaN(res.PredFk))
                    {
                        res.DFk = p.Fk - res.PredFk; res.TolFk = cfg.PlausFkMm + ef;
                        if (Math.Abs(res.DFk) > res.TolFk) why.Add(string.Format(ci, "FK {0:F1} 이 추세 {1:F1} 와 {2:+0.0;-0.0}mm (허용 ±{3:0.#})", p.Fk, res.PredFk, res.DFk, res.TolFk));
                    }
                    if (bTouch && cfg.PlausTouchMm > 0 && !float.IsNaN(res.PredTouch))
                    {
                        res.DTouch = p.Touch - res.PredTouch; res.TolTouch = cfg.PlausTouchMm + et;
                        if (Math.Abs(res.DTouch) > res.TolTouch) why.Add(string.Format(ci, "명령 {0:F1} 이 추세 {1:F1} 와 {2:+0.0;-0.0}mm (허용 ±{3:0.#})", p.Touch, res.PredTouch, res.DTouch, res.TolTouch));
                    }
                    if (why.Count > 0) { res.Ok = false; res.Why = string.Join(", ", why.ToArray()) + " — " + res.How; }
                }
                catch { }
                return res;
            }

            // 끝 확인 — 받아들인 점마다 나머지 점의 추세로 (하나 빼고) 다시 본다. 문턱에 견줘 가장 많이 벗어난 점부터 하나씩 빼고 다시 (벗어난 점이 이웃의 추세를 끌지 않게).
            //   판단할 점이 PlausMinPts 보다 적어지면 멈춘다. 반환 = 벗어난 점과 그 확인 (뺀 순서)
            private static List<KeyValuePair<CSagPracPoint, CSagPracPlaus>> SagPrac_PlausSweep(List<CSagPracPoint> known, CSagPracCfg cfg, bool bTouch)
            {
                var outL = new List<KeyValuePair<CSagPracPoint, CSagPracPlaus>>();
                if (cfg == null) cfg = new CSagPracCfg();
                var cur = new List<CSagPracPoint>(known);
                while (cur.Count >= Math.Max(2, cfg.PlausMinPts) + 1)
                {
                    CSagPracPoint worst = null; CSagPracPlaus wc = null; double wq = 1.0;
                    foreach (CSagPracPoint p in cur)
                    {
                        CSagPracPlaus c = SagPrac_Plausible(cur, p, cfg, bTouch);
                        if (c.Ok) continue;
                        double q = c.Ratio;
                        if (q > wq) { wq = q; worst = p; wc = c; }
                    }
                    if (worst == null) break;
                    cur.Remove(worst);
                    outL.Add(new KeyValuePair<CSagPracPoint, CSagPracPlaus>(worst, wc));
                }
                return outL;
            }

            /// <summary>잰 점 (before.csv 꼴 — 점 순서·값 그대로) 을 [1. 학습] 기준과 같은 순서·규칙으로 다시 본다 (10-15 오프라인 확인용 — 다시 누름은 없으니 벗어난 점은 뺀다):
            /// 가운데 점부터 (SagPrac_MidIndex·SagPrac_Order) 점마다 그때까지 받아들인 점으로 확인 → 끝에 하나씩 빼고 다시 확인 (SagPrac_PlausSweep).
            /// 반환 = 뺀 점 (사본 — Note 에 까닭), accepted = 받아들인 점 (사본). 입력은 바꾸지 않는다. 예외 없음</summary>
            public static List<CSagPracPoint> SagPrac_PlausReplay(List<CSagPracPoint> pts, CSagPracCfg cfg, bool bTouch, out List<CSagPracPoint> accepted)
            {
                accepted = new List<CSagPracPoint>();
                var outL = new List<CSagPracPoint>();
                try
                {
                    if (cfg == null) cfg = new CSagPracCfg();
                    if (pts == null || pts.Count == 0) return outL;
                    var cp = new List<CSagPracPoint>();
                    foreach (CSagPracPoint p in pts)
                        cp.Add(new CSagPracPoint { Idx = p.Idx, R = p.R, Yaw = p.Yaw, X = p.X, Y = p.Y, Pitch = p.Pitch, Est = p.Est, Touch = p.Touch, Fk = p.Fk, Kind = p.Kind, Note = "" });
                    int iMid = SagPrac_MidIndex(cp);
                    foreach (CSagPracPoint p in SagPrac_Order(cp, iMid))
                    {
                        if (float.IsNaN(p.Touch)) continue;
                        CSagPracPlaus c = SagPrac_Plausible(accepted, p, cfg, bTouch);
                        if (c.Ok) accepted.Add(p);
                        else { p.Note = "벗어남 (누를 때): " + c.Why; outL.Add(p); }
                    }
                    foreach (KeyValuePair<CSagPracPoint, CSagPracPlaus> kv in SagPrac_PlausSweep(accepted, cfg, bTouch))
                    {
                        accepted.Remove(kv.Key);
                        kv.Key.Note = "벗어남 (끝 확인): " + kv.Value.Why;
                        outL.Add(kv.Key);
                    }
                }
                catch { }
                return outL;
            }

            // 첫닿음 하나를 확인하고 (SagPrac_Plausible) 벗어나면 추세 높이 창 (시작 = 아래 기준 높이 + MinAboveMm, 한도 = 예상 − BelowMm) 으로 한 번 다시 누른다 — 그래도 벗어나면 뺀다.
            //   bTouch = 명령 높이도 본다 (기준 — 보정 끔). fExp = 다시 누를 창의 예상 명령 높이 (NaN = 추세의 명령 높이, 그것도 없으면 p.Est). model·bHold = 누름 모델.
            //   10-15 검토:
            //   · 기준에서 FK 는 추세 안이고 명령 높이만 벗어났으면 다시 누르지 않고 기준에서 뺀다 (잰 값은 비고에). FK 가 맞으니 끝은 바닥에 닿았다 — 명령만 벗어난 것은 그 누름에서
            //     관절이 평소와 다르게 처졌기 때문이다 (10-15 12:36 r250: FK 50.3 = 추세 안, 명령 66.8 = 추세 +12.4. 궤적: 어깨 ID12 명령−실측이 하강 내내 3.3~3.8° — 다른 회차의
            //     r250 은 1.8~2.3° — 이고 41mm 하강 동안 실측 31.55° 그대로, 데드밴드 반대쪽에 걸린 채. 앞 누름이 r235 공중에서 걸린 높은 값으로 넘겨짚은 창까지 들어 올린 뒤).
            //     그 명령 높이는 자세의 처짐이 아니라 그때의 관절 상태라 기준 (흩어짐·예상·넘겨짚기) 에 넣지 않는다. 추세 창으로 다시 누르면 시작 (추세 + 12 = 66.4) 이
            //     방금 닿은 높이 66.8 보다 낮아, 시작점까지 (감시 없이) 바닥을 누른다
            //   · 다시 누를 창의 시작은 방금 닿았을 수 있는 높이보다 낮게 잡지 않는다: max(예상, 기준이면 첫 누름의 예상 높이 p.Est, 첫닿음 명령 높이 — FK 가 없거나 추세보다
            //     높지 않으면 바닥에 닿았을 수 있다, fHiFloor) + MinAboveMm. 한도는 그대로. 바닥 데이터의 첫 누름 예상 (보정 끔 높이 h) 은 넣지 않는다 — 관절 모델을 켠 닿는
            //     높이 (예상 = h − 내려가는 방향으로 관절 모델이 올리는 양, 10-15 실측 h − 첫닿음 1.9~4.1mm) 보다 높아, 시작 아래 13~16mm 의 출발 걸림 (10-15 실측: 공중에서
            //     걸린 첫닿음 5개 모두 시작 아래 13.2~15.9mm, 바닥 첫닿음은 21.8mm 아래부터) 이 바닥 바로 위에 와 FK 그럴듯함을 지난 채 데이터를 흐린다.
            //     그 높이는 첫 누름이 감시하며 지나온 공중이다. 바닥 데이터는 대신 fHiFloor = 그 단계에서 이미 받아들인 첫닿음 (관절 모델 켬) 명령 높이의 추세 (SagPrac_TrendAt,
            //     NaN = 받아들인 점 없음) — 관절 모델의 예상이 빗나가도 시작이 닿는 높이 아래로 가지 않는다
            //   이미 한 번 다시 눌렀거나 3D 만이면 다시 누르지 않고 뺀다. bOk = 받아들임 (p 에 값). 반환 = 다시 누름의 결과 (다시 누르지 않았으면 Ok) — Stop·Fail 은 부른 쪽이 본다
            private ESagPracPress SagPrac_PlausPress(CSagPracRun run, CSagPracPoint p, List<CSagPracPoint> known, bool bTouch, float fExp, CSagComp model, bool bHold,
                                                     ref int nRedo, ref int nOut, out bool bOk, out CFloorPlanRow rowOut, float fHiFloor = float.NaN)
            {
                bOk = false; rowOut = null;
                var ci = CultureInfo.InvariantCulture;
                if (float.IsNaN(p.Touch)) return ESagPracPress.Ok;
                CSagPracPlaus c = SagPrac_Plausible(known, p, run.Cfg, bTouch);
                if (c.Ok) { bOk = true; return ESagPracPress.Ok; }
                string first = string.Format(ci, "첫닿음 {0:F1} (FK {1}) 벗어남 — {2}", p.Touch, SagPrac_V(p.Fk), c.Why);
                bool bFkIn = !float.IsNaN(c.DFk) && !float.IsNaN(c.TolFk) && Math.Abs(c.DFk) <= c.TolFk;      // FK 는 추세 안 — 끝은 바닥에 닿았다
                if (bTouch && bFkIn)
                {
                    run.L(string.Format(ci, "  ★r={0:0.#} 요 {1:0.#}: {2} — FK 는 추세 안 (바닥에 닿음), 명령 높이만 벗어남", p.R, p.Yaw, first));
                    SagPrac_Reject(p, first + string.Format(ci, " — FK 는 추세 안 ({0:+0.0;-0.0}mm, 허용 ±{1:0.#}): 바닥에 닿았고 명령 높이만 벗어남 (이 누름에서 관절이 평소와 다르게 처짐)" +
                                                                " — 다시 누르지 않고 기준에서 뺌", c.DFk, c.TolFk), run.L);
                    nOut++;
                    return ESagPracPress.Ok;
                }
                run.L(string.Format(ci, "  ★r={0:0.#} 요 {1:0.#}: {2} — 바닥이 아닌 데서 걸린 것으로 봄", p.R, p.Yaw, first));
                float exp = !float.IsNaN(fExp) ? fExp : (!float.IsNaN(c.PredTouch) ? c.PredTouch : p.Est);
                if (p.Redone || run.Arm.IsDry || float.IsNaN(exp)) { SagPrac_Reject(p, first, run.L); nOut++; return ESagPracPress.Ok; }
                if (SagRun_Stopped(run.Stop)) { SagPrac_StopLog(run, string.Format(ci, "r={0:0.#} 다시 누르기 전", p.R)); SagPrac_Reject(p, first, run.L); nOut++; return ESagPracPress.Stop; }
                // 창 시작의 기준 높이 — 방금 닿았을 수 있는 높이·첫 누름의 예상보다 낮게 잡지 않는다 (한도는 예상 − BelowMm 그대로)
                float hi = exp;
                if (bTouch && !float.IsNaN(p.Est)) hi = Math.Max(hi, p.Est);                           // 기준: 첫 누름의 예상 높이
                if (float.IsNaN(c.DFk) || c.DFk <= c.TolFk) hi = Math.Max(hi, p.Touch);                 // FK 가 없거나 추세보다 높지 않다 — 그 첫닿음은 바닥이었을 수 있다
                bool bOnFloor = !float.IsNaN(fHiFloor) && !float.IsInfinity(fHiFloor) && fHiFloor > hi + 0.05f;
                if (bOnFloor) hi = fHiFloor;                                                            // 바닥 데이터: 이 단계에서 받아들인 첫닿음 추세
                p.Redone = true; p.Tries++;
                p.Touch = p.Fk = float.NaN; p.Kind = ""; p.Note = ""; p.StartContact = p.NoTouch = false; p.TouchQ = p.TouchA = null;
                run.L(string.Format(ci, "  r={0:0.#} 요 {1:0.#}: 한 번 더 — 창 시작 {2:F1} ({3:F1} + {4:0}{5}), 한도 {6:F1} (예상 {7:F1} − {8:0})", p.R, p.Yaw,
                    hi + run.Cfg.MinAboveMm, hi, run.Cfg.MinAboveMm,
                    bOnFloor ? " — 이 단계에서 받아들인 첫닿음 추세보다 낮게 잡지 않음" : (hi > exp + 0.05f ? " — 방금 닿았을 수 있는 높이·첫 예상보다 낮게 잡지 않음" : " — 예상 높이"),
                    exp - run.Cfg.BelowMm, exp, run.Cfg.BelowMm));
                CFloorPlanRow row;
                ESagPracPress o = SagPrac_PressAt(run, p, hi, exp, 0, model, bHold, float.NaN, out row, run.Cfg.MinAboveMm);
                if (row != null) rowOut = row;
                if (o != ESagPracPress.Ok || float.IsNaN(p.Touch))
                {
                    string w = (o == ESagPracPress.Ok || o == ESagPracPress.Skip) ? p.Note : (o == ESagPracPress.Stop ? "중단" : "옮기지 못함");
                    SagPrac_Reject(p, first + " · 다시 누름: " + (w.Length > 0 ? w : "첫닿음 없음"), run.L);
                    nOut++;
                    return o == ESagPracPress.Skip ? ESagPracPress.Ok : o;
                }
                CSagPracPlaus c2 = SagPrac_Plausible(known, p, run.Cfg, bTouch);
                if (c2.Ok)
                {
                    bOk = true; nRedo++;
                    p.Note = SagRun_WhyJoin(string.Format(ci, "다시 누름 (처음 {0} 은 벗어남)", first), p.Note);
                    run.L(string.Format(ci, "  r={0:0.#} 요 {1:0.#}: 다시 누름 첫닿음 {2:F1} (FK {3}) — 받아들임", p.R, p.Yaw, p.Touch, SagPrac_V(p.Fk)));
                    return o;
                }
                SagPrac_Reject(p, first + string.Format(ci, " · 다시 누름 {0:F1} (FK {1}) 도 벗어남 — {2}", p.Touch, SagPrac_V(p.Fk), c2.Why), run.L);
                nOut++;
                return o;
            }
            // 그럴듯함으로 뺀 점 — 값은 비우고 (흩어짐·넘겨짚기·데이터셋에서 빠진다) 까닭·잰 값은 비고에
            private static void SagPrac_Reject(CSagPracPoint p, string why, CSim2Real.DLog L)
            {
                p.Touch = p.Fk = float.NaN; p.Kind = ""; p.Rejected = true; p.TouchQ = p.TouchA = null;
                p.Note = SagRun_WhyJoin("뺌 — " + why, p.Note);
                if (L != null) L(string.Format(CultureInfo.InvariantCulture, "  ★r={0:0.#} 요 {1:0.#}: 뺌 — {2}", p.R, p.Yaw, why));
            }
            // 옮기기 실패 (ESagPracPress.Fail) 를 그 점만 빼고 넘어갈 수 있나 (10-15 — 몇 점을 못 재도 작업을 이어 간다). 3D 만·멈춤·안전 막이 (한 틱 관절 변화 막이)·
            //   연달아 두 번이면 아니다 (run.Why 그대로 — 부른 쪽이 멈춘다). 넘어가면 점 비고에 까닭, run.Why 를 비우고 다음 점은 들어 옮긴다
            private static bool SagPrac_SkipFail(CSagPracRun run, CSagPracPoint p, ref int nFailRow)
            {
                if (run.Arm.IsDry || run.Stopped || SagRun_Stopped(run.Stop)) return false;
                if (SagArm_GuardWhy(run.Arm).Length > 0) return false;
                if (++nFailRow >= 2) { run.Why = SagRun_WhyJoin(run.Why, "연달아 두 점을 옮기지 못함"); return false; }
                string w = run.Why;
                p.Touch = p.Fk = float.NaN; p.Kind = ""; p.TouchQ = p.TouchA = null;
                p.Note = SagRun_WhyJoin(p.Note, "옮기지 못함 — 뺌 (" + w + ")");
                run.L(string.Format(CultureInfo.InvariantCulture, "  ★r={0:0.#} 요 {1:0.#}: 옮기지 못함 ({2}) — 이 점은 빼고 다음으로", p.R, p.Yaw, w));
                run.Why = ""; run.LastKey = null;
                return true;
            }
            private static double SagPrac_Median(List<double> v)
            {
                if (v == null || v.Count == 0) return double.NaN;
                double[] a = v.ToArray(); Array.Sort(a);
                return (a.Length % 2 == 1) ? a[a.Length / 2] : 0.5 * (a[a.Length / 2 - 1] + a[a.Length / 2]);
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
            //   −20 ~ 40 으로 자른다. 계산 못 하면 0.
            //   bDescend (10-15, 학습의 예상) = 내려가는 방향 (z + 10 에서 z 로 — 관절 변화 부호) 을 넣어 관절 모델의 데드밴드 항까지 (첫닿음은 내려가며 잰다 — 방향 0 이면
            //   데드밴드만큼 덜 올린다: 3D 만 가상 로봇의 보정 켬 첫닿음과 견줘 방향 0 은 평균 −3.8mm·최대 5.2mm, 내려가는 방향을 넣으면 −0.7mm·최대 1.6mm)
            private static float SagPrac_Lift(C3d kin, CSagArmCfg arm, CSagComp m, float x, float y, float z, float pitch, bool bDescend = false)
            {
                try
                {
                    if (kin == null || m == null) return 0;
                    float bend;
                    double[] ad = SagRun_Seed(kin, arm, (float)(Math.Atan2(y, x) * 180.0 / Math.PI), pitch);
                    float[] q = SagArm_IkSum(kin, arm, ad, x, y, z, SagArm_SumFor(arm, pitch), out bend);
                    if (q == null) return 0;
                    float[] sg = null;
                    if (bDescend)
                    {
                        float[] qUp = SagArm_IkSum(kin, arm, ad, x, y, z + 10, SagArm_SumFor(arm, pitch), out bend);
                        if (qUp != null) { sg = new float[q.Length]; for (int k = 0; k < q.Length && k < qUp.Length; k++) sg[k] = Math.Sign(q[k] - qUp[k]); }
                    }
                    float[] corr = SagPrac_Corr4(kin, m, arm, q, sg);
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
            // 계산 자리 4칸(위치 3 + 손목) q 의 관절 보정 — 손목 로봇은 SagComp_PredictJointCorr 그대로, 꺾는 손목 없음은 움직이는 관절 3개로 묻고 넷째 칸 0 (10-13)
            private static float[] SagPrac_Corr4(C3d kin, CSagComp m, CSagArmCfg arm, float[] q, float[] sg)
            {
                int[] ids = arm.MoveIDs;
                if (ids.Length == q.Length) return kin.SagComp_PredictJointCorr(m, ids, q, sg);
                float[] qn = new float[ids.Length], sn = sg == null ? null : new float[ids.Length];
                for (int k = 0; k < ids.Length; k++) { qn[k] = q[k]; if (sn != null) sn[k] = sg[k]; }
                float[] c = kin.SagComp_PredictJointCorr(m, ids, qn, sn);
                if (c == null) return null;
                float[] r = new float[q.Length];
                for (int k = 0; k < c.Length && k < r.Length; k++) r[k] = c[k];
                return r;
            }
            private static float[] SagPrac_SimAct(C3d kin, CSagArmCfg arm, CSagPracCfg cfg, float[] sent, float[] sigma)
            {
                float[] lev = kin.SagFc_Levers(arm.FuncNumber, arm.QIDs, sent, kin.SagFc_MotCopy());   // 4칸 (손목 없음 칸의 지레는 0)
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
                            float[] corr = SagPrac_Corr4(kin, m, arm, q, sg);
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
                res.DatasetSession = "sim_" + DateTime.Now.ToString("MMdd_HHmmss", ci);
                int n = SagRun_AppendDataset(dsPath, SagRun_DatasetRows(cs, res.DatasetSession, true));
                res.DatasetRowsAdded = Math.Max(0, n);
                if (L != null) L(string.Format(ci, "3D 만 — 가상 바닥 첫닿음 {0}점 → {1} ({2}행)", cs.Count, Path.GetFileName(dsPath), res.DatasetRowsAdded));
                return res.DatasetRowsAdded >= Math.Max(1, cfg.LearnMinData);
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
