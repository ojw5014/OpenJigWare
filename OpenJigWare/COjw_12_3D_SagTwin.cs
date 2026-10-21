// ================================================================
// C3d — 처짐 쌍둥이 (sim2real)  2026-10-02
//
// 실물로 보낸(시뮬만이면 보냈을) 명령 + 처짐 모델이 예측한 관절 오차 = "실물이 있을 자세".
//   · 보정 켬 → 보낸 명령에 보정이 들어 있어 쌍둥이 ≈ 목표 (보정한 만큼 시뮬도 보정된다)
//   · 보정 끔 → 쌍둥이가 실물처럼 처진다
// 예측은 보정과 **같은 모델을 방향만 반대로** 쓴다 — 관절 모델(중력 c·L·(1+κ), 데드밴드 h·σ — 10-16 부터 보낸 데드밴드 항의 놀이, 지연 τ·ω) +
//   엔코더 밖 처짐(첫닿음 모델 / 바닥 GP / 반경 보정표, 이상 자세에서 "끝을 올릴 양" dz → 실물은 dz 만큼 낮다).
//   보정의 켬·끔 플래그(Enabled, FloorMapEnabled, LagEnabled)는 "보정을 할지" 일 뿐이라 예측에는 쓰지 않는다.
//   fcon 은 바닥 모델 선택(첫닿음 모델 ↔ GP·보정표)이라 출력 단과 같게 따른다.
//
// 계산은 모션 스레드에서만 한다 — 지레·FK 계산이 3D 관절값을 잠깐 바꿨다 되돌리기 때문 (화면·전송은 스냅샷만 읽는다).
//   · 실물 스트리밍 중 : CScene_t.SendPose → SagComp_JointOut 이 보낸 명령을 기록 → MoveJoints_Step 끝에서 예측
//   · 시뮬만(싱크 없음·실물 미연결·토크 꺼짐) : MoveJoints_Step 끝에서 출력 단을 그림자로 돌려 "보냈을 명령" 을 만든다
//     (그림자는 출력 단 상태의 사본으로 돈다 — 실물 출력 단의 직전 보정·전환·추적은 바꾸지 않는다, SagComp_JointOutShadow 10-03)
//   · 멈춰 있을 때(마우스 드래그·재생) : 그리기 직전, 싱크가 없고 움직임이 없을 때만 UI 스레드에서
//   (첫닿음 모델의 준비·특징은 예외 — 관절값 사본으로 계산해 3D 관절값을 바꾸지 않으므로 SagTwin_SetModel 을 부른 스레드에서 한다. COjw_12_3D_SagFc.cs)
// 표시 : SagTwin_Show = 3D 화면에 반투명 팔 / SagTwin_GetDeg = 외부 시뮬 전송용 (CSimBridge.SendJointsTwin)
//        SagTwin_ToolInfo·ToolText (10-16) = 쌍둥이 자세를 공구 끝까지 FK 한 공구 각(바닥과, 90 = 수직)·기운 쪽·끝 오차 — 앱이 낮은 주기로 읽어 보인다
// 예측 보정 (10-16, CSagComp.Predictive) : 출력 단이 1차 보정을 이 쌍둥이 식으로 다듬어 (SagComp_PredRefine) 보정 켬이면 쌍둥이 끝 = 목표, 공구 = 3D 의 공구 각이 된다
//        (전환 1초 동안·항이 MaxAbsJointDeg 에 잘린 자세·지연 항은 예외)
// 모델 : SagComp_Load / SagComp_Set 이 쌍둥이 모델도 함께 바꾼다. 보정을 꺼도(SagComp_Clear·Enable(false)) 쌍둥이 모델은 남는다
//        — 그래야 "보정 끔 = 처짐" 이 보인다. 다른 로봇으로 바꿀 때는 SagTwin_SetModel(null).
// 한계 : 손목 서보 동적 지연(하강 중 q4≈60° 에서 +1.5~3.4°, 10-02 측정)은 아직 모델에 없다. 엔코더 밖 처짐은 첫닿음 모델(fcon=1)이면
//        작업 범위 전체에서 예측하고, 바닥 GP 만 있으면 아는 자세(바닥 접촉 띠)에서만 0 이 아니다 (SagTwin_ExternalDz).
// ================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using Tao.OpenGl;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            /// <summary>3D 화면에 처짐 쌍둥이(실물이 있을 자세)를 반투명 팔로 겹쳐 그린다</summary>
            public bool SagTwin_Show = false;
            /// <summary>화면에 그리지 않아도 쌍둥이를 계산한다 (외부 시뮬 전송용)</summary>
            public bool SagTwin_Compute = false;
            /// <summary>쌍둥이 색·투명도 — 고정값으로 쓸 것 (메시 표시 목록 캐시의 키라 바꿀 때마다 메시를 다시 굽는다)</summary>
            public Color SagTwin_Color = Color.FromArgb(255, 140, 26);
            public float SagTwin_Alpha = 0.35f;

            private CSagComp m_SagTwinModel = null;                   // 예측 전용 모델 — 보정을 꺼도 유지
            private volatile float[] m_afSagTwinDelta = null;         // 쌍둥이 − 이상 (모터 번호 색인 0..255), 계산할 때마다 새 배열로 바꿔 끼운다
            private volatile float[] m_afSagTwinPose = null;          // 쌍둥이 자세 그 자체 (모터 번호 색인, 계산에 쓴 이상 자세 + 차이 — 계산하지 않은 칸은 NaN).
                                                                      //   10-15: 화면이 '지금 3D 관절값 + 직전 틱의 차이' 를 그리면 이상 자세가 한 틱 앞서 나간 프레임에서
                                                                      //   쌍둥이가 보정 전·후 자세를 오갔다 — 차이와 같은 틱의 이상 자세로 만든 자세를 한 번에 바꿔 끼워 그린다
            private bool m_bSagTwinPass = false;                      // 그리기: 쌍둥이 패스 중 (OjwDraw_Class 가 색·선택 처리를 바꾼다)
            private readonly object m_oSagTwin = new object();
            private int m_nSagTwinNoteSeq = 0, m_nSagTwinUsedSeq = 0; // 출력 단이 명령을 기록한 횟수 / 예측에 쓴 횟수
            private int[] m_anSagTwinIds = null;                      // 출력 단이 마지막으로 기록한 명령 (ids 순)
            private float[] m_afSagTwinIdeal = null, m_afSagTwinCmd = null;
            private float[] m_afSagTwinDb = null;                     // 그 명령에 든 데드밴드 항 w·h·σ (ids 순, 10-16 — 없으면 null)
            private float[] m_afSagTwinLastIdeal = null;              // 멈춘 상태 갱신: 직전 계산 때의 이상 자세
            private CSagComp m_SagTwinLastModel = null;
            private readonly Dictionary<int, float> m_dicSagTwinSigma = new Dictionary<int, float>();   // 쌍둥이 전용 데드밴드 상태 (놀이 — SagTwin_Sigma)
            private readonly Dictionary<int, float> m_dicSagTwinPrev = new Dictionary<int, float>();
            private const int SAG_TWIN_MAX_ID = 256;

            private bool SagTwin_Wanted { get { return (SagTwin_Show || SagTwin_Compute) && m_SagTwinModel != null; } }

            /// <summary>쌍둥이 예측 모델 지정 (null = 없음 → 쌍둥이 = 보낸 명령). SagComp_Load/Set 이 자동으로 부른다.</summary>
            public void SagTwin_SetModel(CSagComp m)
            {
                if (m != null && m.HasJoint) SagFc_EnsureReady(m);          // 첫닿음 모델은 여기서 준비 (모션 스레드의 첫 예측에서 하지 않게) — 관절값 사본 계산이라 어느 스레드에서도 된다
                lock (m_oSagTwin)
                {
                    m_SagTwinModel = (m != null && m.HasJoint) ? m : null;
                    m_dicSagTwinSigma.Clear(); m_dicSagTwinPrev.Clear();
                    m_afSagTwinLastIdeal = null;
                    if (m_SagTwinModel == null) { m_afSagTwinDelta = null; m_afSagTwinPose = null; m_SagTwinSnap = null; }
                }
            }
            public CSagComp SagTwin_GetModel() { return m_SagTwinModel; }
            /// <summary>쌍둥이가 계산돼 있는가 (모델이 있고 한 번 이상 예측했다)</summary>
            public bool SagTwin_Ready { get { return m_SagTwinModel != null && m_afSagTwinDelta != null; } }

            /// <summary>쌍둥이 관절각 (deg) = 이상(3D) + 예측 차이. 쌍둥이가 없으면 이상 자세를 채우고 false.</summary>
            public bool SagTwin_GetDeg(int[] ids, float[] afOut)
            {
                if (ids == null || afOut == null) return false;
                float[] d = m_afSagTwinDelta;
                float[] tp = m_afSagTwinPose;                       // 같은 틱의 쌍둥이 자세 (있는 칸은 그대로 — 지금 3D 값 + 직전 차이를 섞지 않는다)
                for (int i = 0; i < ids.Length && i < afOut.Length; i++)
                {
                    int id = ids[i];
                    if (tp != null && id >= 0 && id < tp.Length && !float.IsNaN(tp[id])) { afOut[i] = tp[id]; continue; }
                    float v = GetData(id);
                    if (d != null && id >= 0 && id < d.Length) v += d[id];
                    afOut[i] = v;
                }
                return d != null && m_SagTwinModel != null;
            }
            /// <summary>쌍둥이 − 이상 (deg, ids 순) — 진단용. 없으면 0</summary>
            public float[] SagTwin_GetDelta(int[] ids)
            {
                float[] r = new float[ids == null ? 0 : ids.Length];
                float[] d = m_afSagTwinDelta;
                if (d != null) for (int i = 0; i < r.Length; i++) if (ids[i] >= 0 && ids[i] < d.Length) r[i] = d[ids[i]];
                return r;
            }

            // 출력 단(SagComp_JointOut 끝)이 실제로 보낸 명령을 기록 — 복사만 한다 (db = 그 명령에 든 데드밴드 항, ids 순 — 없으면 null)
            private void SagTwin_NoteCommand(int[] ids, float[] ideal, float[] cmd, float[] db)
            {
                lock (m_oSagTwin)
                {
                    m_anSagTwinIds = (int[])ids.Clone(); m_afSagTwinIdeal = ideal; m_afSagTwinCmd = (float[])cmd.Clone(); m_afSagTwinDb = db;
                    m_nSagTwinNoteSeq++;
                }
            }

            // MoveJoints_Step 끝 (싱크 다음) — 모션 스레드
            private void SagTwin_AfterStep()
            {
                if (!SagTwin_Wanted) return;
                SagTwin_Update();
            }

            // 멈춘 상태 갱신 — OjwDraw 의 그리기 직전 (UI 스레드). 싱크가 있거나 움직이는 중이면 모션 스레드가 맡는다
            private void SagTwin_IdleUpdate()
            {
                if (!SagTwin_Wanted || m_MotionSink != null || MoveJoints_IsMoving()) return;
                CSagComp m = m_SagTwinModel;
                if (m == null) return;
                float[] cur = new float[m.JointIDs.Length];
                for (int j = 0; j < cur.Length; j++) cur[j] = GetData(m.JointIDs[j]);
                float[] last = m_afSagTwinLastIdeal;
                if (last != null && m == m_SagTwinLastModel && last.Length == cur.Length && m_afSagTwinDelta != null)
                {
                    bool bSame = true;
                    for (int j = 0; j < cur.Length && bSame; j++) if (Math.Abs(cur[j] - last[j]) > 1e-4f) bSame = false;
                    if (bSame) return;
                }
                SagTwin_Update();
            }

            // 기록된 명령(실물 스트리밍)이 있으면 그것으로, 없으면 출력 단을 그림자로 돌려 예측
            private void SagTwin_Update()
            {
                CSagComp m = m_SagTwinModel;
                if (m == null || !m.HasJoint) { m_afSagTwinDelta = null; m_afSagTwinPose = null; m_SagTwinSnap = null; return; }
                int[] ids = null; float[] ideal = null, cmd = null, db = null;
                float[] shOmega = null, shAlpha = null, shDir = null;          // 그림자 출력 단의 방향·속도 추적 (그림자일 때만 — 실물 추적 대신)
                lock (m_oSagTwin)
                {
                    if (m_nSagTwinNoteSeq != m_nSagTwinUsedSeq && m_anSagTwinIds != null)
                    { ids = m_anSagTwinIds; ideal = m_afSagTwinIdeal; cmd = m_afSagTwinCmd; db = m_afSagTwinDb; m_nSagTwinUsedSeq = m_nSagTwinNoteSeq; }
                }
                if (ids == null)
                {
                    // 그림자 출력 단 — 실물로는 나가지 않는다. 보정(켜져 있으면)·전환·방향·속도 추적을 그림자 상태 사본으로 돌린다
                    //   (SagComp_JointOutShadow — 실물 출력 단의 상태는 바꾸지 않는다, 10-03)
                    ids = (int[])m.JointIDs.Clone();
                    ideal = new float[ids.Length];
                    for (int i = 0; i < ids.Length; i++) ideal[i] = GetData(ids[i]);
                    cmd = (float[])ideal.Clone();
                    SagComp_JointOutShadow(ids, cmd, out shOmega, out shAlpha, out shDir, out db);
                    lock (m_oSagTwin) m_nSagTwinUsedSeq = m_nSagTwinNoteSeq;   // 방금 그림자가 남긴 기록은 다시 쓰지 않는다
                }
                SagTwin_Predict(m, ids, ideal, cmd, shOmega, shAlpha, shDir, db);
                float[] li = new float[m.JointIDs.Length];
                for (int j = 0; j < li.Length; j++) { int k = Array.IndexOf(ids, m.JointIDs[j]); li[j] = k >= 0 ? ideal[k] : GetData(m.JointIDs[j]); }
                m_afSagTwinLastIdeal = li; m_SagTwinLastModel = m;
            }

            // 쌍둥이 = 보낸 명령 + 예측 오차 → 스냅샷 (쌍둥이 − 이상) 발행. 출력 단의 예측 보정(SagComp_PredRefine)은 ①②③ 과 같은 식을 쓴다 — 고치면 둘 다
            //   (지연 항은 예측 보정에 넣지 않는다 — 1차 보정과 같이 앞서 보내기 켬일 때만 메운다. 데드밴드는 예측 보정이 출력 단의 σ 를 쓰고, 쌍둥이는 보낸 명령의
            //    놀이를 쓴다 — 한 방향으로 가는 동안은 같은 값, 반전 직후 데드밴드를 넘는 동안만 쌍둥이가 실물의 멈춤을 보인다 10-16)
            //   shOmega·shAlpha·shDir = 그림자 출력 단의 방향·속도 추적 (ids 순, 그림자일 때) — null 이면 실물 출력 단의 추적 (실물 스트리밍)
            //   db = 출력 단이 이 틱에 보낸 데드밴드 항 w·h·σ (ids 순, 10-16 — 보정 끔·관절 모델 없음이면 null)
            private void SagTwin_Predict(CSagComp m, int[] ids, float[] ideal, float[] cmd, float[] shOmega, float[] shAlpha, float[] shDir, float[] db)
            {
                var delta = new float[SAG_TWIN_MAX_ID];
                for (int i = 0; i < ids.Length && i < ideal.Length && i < cmd.Length; i++)
                    if (ids[i] >= 0 && ids[i] < SAG_TWIN_MAX_ID) delta[ids[i]] = cmd[i] - ideal[i];              // 보정분 (보정 끔이면 0)
                int nj = m.JointIDs.Length;
                float[] pc = new float[nj], pi = new float[nj];
                for (int j = 0; j < nj; j++)
                {
                    int k = Array.IndexOf(ids, m.JointIDs[j]);
                    pc[j] = k >= 0 ? cmd[k] : GetData(m.JointIDs[j]);
                    pi[j] = k >= 0 ? ideal[k] : GetData(m.JointIDs[j]);
                }
                // ① 엔코더가 보는 처짐 — 보낸 자세의 중력 지레, 데드밴드는 이상 자세 + 보낸 데드밴드 항의 놀이 (10-16 — sigmatake=0 이면 예전: 이상 자세의 이동 방향)
                float[] lev = SagComp_JointLevers(m.FuncNumber, m.JointIDs, pc);
                float[] twin = new float[nj];
                bool bPlay = m.SigmaTakeDeg > 0;
                for (int j = 0; j < nj; j++)
                {
                    int id = m.JointIDs[j];
                    int ki = Array.IndexOf(ids, id);
                    float v = pi[j] + ((bPlay && db != null && ki >= 0 && ki < db.Length) ? db[ki] : 0f);
                    float sg = SagTwin_Sigma(m, j, id, v, (shDir != null && ki >= 0 && ki < shDir.Length) ? shDir[ki] : float.NaN);
                    double e = (1.0 + m.FlexGain) * m.Jc[j] * lev[j] - m.Jh[j] * sg;     // = −JointCorrStatic
                    if (m.HasJointLag)
                    {
                        float om = 0, al = 0;
                        if (shOmega != null && shAlpha != null && ki >= 0 && ki < shOmega.Length && ki < shAlpha.Length) { om = shOmega[ki]; al = shAlpha[ki]; }
                        else lock (m_dicSagDir) { m_dicSagOmega.TryGetValue(id, out om); m_dicSagAlpha.TryGetValue(id, out al); }
                        e += m.PredictJointLag(j, om, al);                                   // = −τ·ω − κ·α (실물은 뒤처진다)
                    }
                    twin[j] = (float)(pc[j] + e);
                    if (id >= 0 && id < SAG_TWIN_MAX_ID) delta[id] += (float)e;
                }
                // ② 엔코더 밖 처짐 — 이상 자세에서 끝을 올릴 양 dz → 실물 끝은 dz 만큼 낮다 → 관절 12·13·14 로 (반경·피치 유지)
                //    관절각 환산은 보정과 같은 자리(이상 자세)에서 — 보정 켬이면 보정분과 정확히 상쇄된다 (r=250 dz≈20mm 에서 보낸 자세로
                //    환산하면 손목이 0.67° 어긋났다)
                if (m.PosIDs != null && m.PosIDs.Length >= 3)
                {
                    double dz = SagTwin_ExternalDz(m, ids, ideal);
                    if (Math.Abs(dz) > 1e-3)
                    {
                        int[] fid = { m.PosIDs[1], m.PosIDs[2], m.WristID };
                        float[] fq = new float[3];
                        for (int k = 0; k < 3; k++) { int i = Array.IndexOf(ids, fid[k]); fq[k] = i >= 0 ? ideal[i] : GetData(fid[k]); }
                        double[] dq;
                        if (SagComp_FloorJointDelta(m.FuncNumber, fid, fq, -dz, out dq))
                            for (int k = 0; k < 3; k++) if (fid[k] >= 0 && fid[k] < SAG_TWIN_MAX_ID) delta[fid[k]] += (float)dq[k];
                    }
                }
                // ③ 엔코더 밖 공구 기울기 (10-15) — 실물 공구는 엔코더가 보는 손목 자리보다 τ 만큼 더 기운다 (+ = 끝이 베이스 쪽). 보정 켬이면 보낸 명령에
                //    −τ 가 들어 있어 (①의 cmd − ideal) 둘이 상쇄된다 — 쌍둥이 공구가 수직, 보정 끔이면 τ 만큼 기운 공구
                if (m.HasWristTilt && m.WristID >= 0 && m.WristID < SAG_TWIN_MAX_ID) delta[m.WristID] += SagComp_TiltOf(m, ids, ideal);
                // 쌍둥이 자세 — 이 계산에 쓴 이상 자세 + 차이 (ids 밖 모델 관절은 지금 3D 값 — 계산에 쓴 값과 같다). 나머지 칸은 NaN (화면 = 지금 3D 값)
                var pose = new float[SAG_TWIN_MAX_ID];
                for (int i = 0; i < pose.Length; i++) pose[i] = float.NaN;
                for (int i = 0; i < ids.Length && i < ideal.Length; i++)
                    if (ids[i] >= 0 && ids[i] < SAG_TWIN_MAX_ID) pose[ids[i]] = ideal[i] + delta[ids[i]];
                int[] extra = { m.PosIDs != null && m.PosIDs.Length >= 3 ? m.PosIDs[1] : -1, m.PosIDs != null && m.PosIDs.Length >= 3 ? m.PosIDs[2] : -1, m.WristID };
                foreach (int id in m.JointIDs) if (id >= 0 && id < SAG_TWIN_MAX_ID && float.IsNaN(pose[id])) pose[id] = GetData(id) + delta[id];
                foreach (int id in extra) if (id >= 0 && id < SAG_TWIN_MAX_ID && float.IsNaN(pose[id])) pose[id] = GetData(id) + delta[id];
                // 공구 읽기용 (10-16) — 같은 틱의 이상 자세 (쌍둥이 자세가 있는 칸만: 쌍둥이 − 차이)
                var idealPose = new float[SAG_TWIN_MAX_ID];
                for (int i = 0; i < idealPose.Length; i++) idealPose[i] = float.IsNaN(pose[i]) ? float.NaN : pose[i] - delta[i];
                m_SagTwinSnap = new CSagTwinSnap { Pose = pose, Ideal = idealPose, Model = m };
                m_afSagTwinPose = pose;
                m_afSagTwinDelta = delta;
            }

            // ── 쌍둥이 공구 읽기 (10-16) ──
            //   쌍둥이 자세와 같은 틱의 이상 자세를 함께 바꿔 끼운다 (화면 쪽이 둘을 따로 읽어 다른 틱이 섞이지 않게)
            private sealed class CSagTwinSnap { public float[] Pose, Ideal; public CSagComp Model; }
            private volatile CSagTwinSnap m_SagTwinSnap = null;
            /// <summary>쌍둥이 공구 읽기 (10-16) — 쌍둥이 자세(실물이 있을 자세)를 공구 끝까지 FK (관절값 사본 — 3D 관절값을 바꾸지 않는다, 어느 스레드에서도 된다).
            /// fAngleDeg = 공구 축(꺾는 손목 원점 → 공구 끝)이 바닥과 이루는 각 (deg, 90 = 수직 — 로봇 정의의 수직 관절 합과 같은 기준),
            /// fTiltDeg = 수직에서 기운 각 (deg, + = 공구 끝이 베이스 쪽, − = 바깥쪽), fTcpErrMm = 쌍둥이 끝과 같은 틱 이상 자세(3D) 끝의 거리 (mm),
            /// fIdealAngleDeg = 이상 자세(3D)의 공구 각. 꺾는 손목 없음이면 각 셋은 NaN (끝 오차만). 쌍둥이가 없으면(표시·계산 꺼짐, 모델 없음) false. 예외 없음</summary>
            public bool SagTwin_ToolInfo(out float fAngleDeg, out float fTiltDeg, out float fTcpErrMm, out float fIdealAngleDeg)
            {
                fAngleDeg = fTiltDeg = fTcpErrMm = fIdealAngleDeg = float.NaN;
                try
                {
                    CSagTwinSnap s = m_SagTwinSnap;
                    CSagComp m = m_SagTwinModel;
                    if (s == null || m == null || !ReferenceEquals(s.Model, m) || m_afSagTwinDelta == null) return false;
                    double[] adT = SagFc_MotCopy(), adI = (double[])adT.Clone();
                    for (int id = 0; id < s.Pose.Length && id < adT.Length; id++)
                    {
                        if (!float.IsNaN(s.Pose[id])) SagFc_Set(adT, id, s.Pose[id]);
                        if (!float.IsNaN(s.Ideal[id])) SagFc_Set(adI, id, s.Ideal[id]);
                    }
                    float tx, ty, tz, ix, iy, iz;
                    SagFc_Fk(m.FuncNumber, adT, -1, out tx, out ty, out tz);
                    SagFc_Fk(m.FuncNumber, adI, -1, out ix, out iy, out iz);
                    fTcpErrMm = (float)Math.Sqrt((double)(tx - ix) * (tx - ix) + (double)(ty - iy) * (ty - iy) + (double)(tz - iz) * (tz - iz));
                    if (m.WristID >= 0)
                    {
                        float wx, wy, wz, vx, vy, vz;
                        SagFc_Fk(m.FuncNumber, adT, m.WristID, out wx, out wy, out wz);
                        SagFc_Fk(m.FuncNumber, adI, m.WristID, out vx, out vy, out vz);
                        double t;
                        fAngleDeg = (float)SagTwin_FloorAngle(tx - wx, ty - wy, tz - wz, tx, ty, out t);
                        fTiltDeg = (float)t;
                        fIdealAngleDeg = (float)SagTwin_FloorAngle(ix - vx, iy - vy, iz - vz, ix, iy, out t);
                    }
                    return !float.IsNaN(fTcpErrMm);
                }
                catch { fAngleDeg = fTiltDeg = fTcpErrMm = fIdealAngleDeg = float.NaN; return false; }
            }
            /// <summary>쌍둥이 공구 읽기 한 줄 (10-16) — "예측 공구 각 89.9° · 끝 오차 0.2 mm" (기울면 "(끝이 베이스 쪽)"·"(끝이 바깥쪽)", 3D 자세가 수직이 아니면
            /// "(3D 75.0°)" 를 붙인다. 꺾는 손목 없음이면 "예측 끝 오차 0.2 mm"). 쌍둥이가 없으면 "". 예외 없음</summary>
            public string SagTwin_ToolText()
            {
                float a, t, e, ai;
                if (!SagTwin_ToolInfo(out a, out t, out e, out ai)) return "";
                var ci = System.Globalization.CultureInfo.InvariantCulture;
                if (float.IsNaN(a)) return string.Format(ci, "예측 끝 오차 {0:0.0} mm", e);
                string dir = Math.Abs(t) < 0.05f ? "" : (t > 0 ? " (끝이 베이스 쪽)" : " (끝이 바깥쪽)");
                string ideal = (!float.IsNaN(ai) && Math.Abs(ai - 90f) >= 0.5f) ? string.Format(ci, " (3D {0:0.0}°)", ai) : "";
                return string.Format(ci, "예측 공구 각 {0:0.0}°{1}{2} · 끝 오차 {3:0.0} mm", a, dir, ideal, e);
            }
            // 공구 축 (vx,vy,vz — 손목 원점 → 끝) 이 바닥과 이루는 각 (deg, 0..90) 과 수직에서 기운 각 tilt (deg, + = 끝이 베이스 쪽 — 끝 (px,py) 의 반경 방향 성분 부호)
            private static double SagTwin_FloorAngle(double vx, double vy, double vz, double px, double py, out double tilt)
            {
                double vh = Math.Sqrt(vx * vx + vy * vy);
                double ang = Math.Atan2(-vz, vh) * 180.0 / Math.PI;
                double r = Math.Sqrt(px * px + py * py);
                double vr = r > 1e-6 ? (vx * px + vy * py) / r : 0;
                tilt = Math.Atan2(-vz, vr) * 180.0 / Math.PI - 90.0;
                return ang;
            }

            // 쌍둥이 전용 데드밴드 상태 σ (−1..+1), 보정을 꺼도 돈다. 실물 관절 = 보낸 명령 − h·σ.
            //   10-16 (sigmatake > 0): pose = 이상 자세 + 출력 단이 보낸 데드밴드 항 (w·h·σ), 폭 h — 서보 데드밴드의 놀이 연산자 (보낸 데드밴드 항이 2h 를 건너야
            //     실물이 반전해 따라 움직인다). 보정 끔이면 pose = 이상 자세라 예전 식(sigmaramp 1)과 같고, 한 방향으로 가는 동안은 출력 단의 σ 와 같은 값 —
            //     보정 켬이면 출력 단의 데드밴드 넘기(SagComp_SigmaStep)가 남긴 반전 자리의 멈춤만 보인다.
            //     (보낸 명령 전체가 아니라 이 몫만 쓴다 — 바닥·중력 보정이 자세에 따라 바뀌어 명령이 이상 자세와 반대로 가는 곳(바닥 높이 r160→180 의 12번 등)은
            //      출력 단도 이상 자세의 이동 방향으로 보정하므로 쌍둥이도 같은 가정 — 예측 보정 격자 잔차가 그대로)
            //   sigmatake = 0: 예전 — pose = 이상 자세, 폭 sigmaramp·h (출력 단의 예전 식과 같다).
            //   처음 σ = 출력 단이 추적한 방향 — dir0 (그림자 출력 단의 방향, NaN 이면 실물 출력 단의 방향)
            private float SagTwin_Sigma(CSagComp m, int j, int id, float pose, float dir0)
            {
                lock (m_oSagTwin)
                {
                    float sg, prev;
                    if (!m_dicSagTwinSigma.TryGetValue(id, out sg))
                    {
                        if (!float.IsNaN(dir0)) sg = dir0;
                        else { int d; lock (m_dicSagDir) m_dicSagDir.TryGetValue(id, out d); sg = d; }
                    }
                    else if (m_dicSagTwinPrev.TryGetValue(id, out prev))
                    {
                        sg += (pose - prev) / (float)(m.SigmaTakeDeg > 0 ? Math.Max(1e-3, m.Jh[j]) : Math.Max(0.2, m.SigmaRamp * m.Jh[j]));
                        if (sg > 1f) sg = 1f; if (sg < -1f) sg = -1f;
                    }
                    m_dicSagTwinSigma[id] = sg; m_dicSagTwinPrev[id] = pose;
                    return sg;
                }
            }

            // 엔코더 밖 처짐: 이상 자세에서 끝을 올릴 양 (mm) — 출력 단과 같은 우선순위: fcon=1 이면 첫닿음 모델(COjw_12_3D_SagFc.cs),
            //   아니면 바닥 GP(데이터 밖은 0), 그것도 없으면 반경 보정표
            private double SagTwin_ExternalDz(CSagComp m, int[] ids, float[] ideal)
            {
                CSagFc fc = m.Fc;                                   // 한 번만 읽는다 (쓰고 있는 모델을 다시 학습하면 다른 스레드가 바꿔 끼운다)
                bool bFc = fc != null && fc.HasData && fc.Enabled;  // = m.HasFc && m.Fc.Enabled
                if (!bFc && !m.HasGp && !m.HasFloorMap) return 0;
                int[] fid = { m.PosIDs[1], m.PosIDs[2], m.WristID };
                float[] fq = new float[3];
                for (int k = 0; k < 3; k++) { int i = Array.IndexOf(ids, fid[k]); fq[k] = i >= 0 ? ideal[i] : GetData(fid[k]); }
                double r0, z0;
                SagComp_TcpRZ(m.FuncNumber, fid, fq, out r0, out z0);
                if (bFc) return SagTwin_FcDz(m, fc, ids, ideal, fq, r0, z0);
                if (m.HasGp)
                {
                    double[] gx = new double[m.Gp.FeatIDs.Length];
                    for (int k = 0; k < gx.Length; k++) { int i = Array.IndexOf(ids, m.Gp.FeatIDs[k]); gx[k] = i >= 0 ? ideal[i] : GetData(m.Gp.FeatIDs[k]); }
                    double std;
                    return m.Gp.Correction(gx, r0, out std);
                }
                return m.FloorDzAt(r0);
            }

            // 그리기 — OjwDraw 의 메인 루프 뒤. 픽 프레임에서는 그리지 않는다 (쌍둥이가 마우스 선택을 가로채지 않게)
            private void SagTwin_DrawGhost(float[] afData)
            {
                ToolMark_Draw(afData);   // 같은 그리기 훅 — 공구 끝·공구축·손목 표시 (COjw_12_3D_SagDef.cs)
                if (!SagTwin_Show || m_bPickMode || afData == null) return;
                float[] d = m_afSagTwinDelta;
                if (d == null || m_SagTwinModel == null) return;
                float[] tp = m_afSagTwinPose;                         // 같은 틱의 쌍둥이 자세 — 있는 칸은 그대로 그린다 (SagTwin_GetDeg 와 같다)
                int nCnt = OjwDispAll.GetCount();
                long lTrack = m_lTmrTrack; m_lTmrTrack = 0;      // 바퀴·트랙 회전이 두 번 누적되지 않게
                InitPosAngle();
                m_bSagTwinPass = true;
                Gl.glDepthMask(Gl.GL_FALSE);                      // 반투명 — 원래 팔을 가리지 않게
                try
                {
                    for (int i = 0; i < nCnt; i++)
                    {
                        COjwDisp c = OjwDispAll.GetData(i);
                        if (c == null) continue;
                        if (c.nName >= 0 && c.nName < afData.Length)
                            c.fAngle = (tp != null && c.nName < tp.Length && !float.IsNaN(tp[c.nName])) ? tp[c.nName] : afData[c.nName] + (c.nName < d.Length ? d[c.nName] : 0f);
                        m_nDrawClass_Pos = i;
                        OjwDraw_Class(c);
                    }
                }
                finally
                {
                    Gl.glDepthMask(Gl.GL_TRUE);
                    m_bSagTwinPass = false;
                    m_lTmrTrack = lTrack;
                    for (int i = 0; i < nCnt; i++)
                    {
                        COjwDisp c = OjwDispAll.GetData(i);
                        if (c != null && c.nName >= 0 && c.nName < afData.Length) c.fAngle = afData[c.nName];
                    }
                }
            }
        }
    }
}
