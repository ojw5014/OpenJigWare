// ================================================================
// C3d — 처짐 쌍둥이 (sim2real)  2026-10-02
//
// 실물로 보낸(시뮬만이면 보냈을) 명령 + 처짐 모델이 예측한 관절 오차 = "실물이 있을 자세".
//   · 보정 켬 → 보낸 명령에 보정이 들어 있어 쌍둥이 ≈ 목표 (보정한 만큼 시뮬도 보정된다)
//   · 보정 끔 → 쌍둥이가 실물처럼 처진다
// 예측은 보정과 **같은 모델을 방향만 반대로** 쓴다 — 관절 모델(중력 c·L·(1+κ), 데드밴드 h·σ, 지연 τ·ω) +
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
            private bool m_bSagTwinPass = false;                      // 그리기: 쌍둥이 패스 중 (OjwDraw_Class 가 색·선택 처리를 바꾼다)
            private readonly object m_oSagTwin = new object();
            private int m_nSagTwinNoteSeq = 0, m_nSagTwinUsedSeq = 0; // 출력 단이 명령을 기록한 횟수 / 예측에 쓴 횟수
            private int[] m_anSagTwinIds = null;                      // 출력 단이 마지막으로 기록한 명령 (ids 순)
            private float[] m_afSagTwinIdeal = null, m_afSagTwinCmd = null;
            private float[] m_afSagTwinLastIdeal = null;              // 멈춘 상태 갱신: 직전 계산 때의 이상 자세
            private CSagComp m_SagTwinLastModel = null;
            private readonly Dictionary<int, float> m_dicSagTwinSigma = new Dictionary<int, float>();   // 쌍둥이 전용 매끈한 이동 방향 (출력 단과 같은 식)
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
                    if (m_SagTwinModel == null) m_afSagTwinDelta = null;
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
                for (int i = 0; i < ids.Length && i < afOut.Length; i++)
                {
                    float v = GetData(ids[i]);
                    if (d != null && ids[i] >= 0 && ids[i] < d.Length) v += d[ids[i]];
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

            // 출력 단(SagComp_JointOut 끝)이 실제로 보낸 명령을 기록 — 복사만 한다
            private void SagTwin_NoteCommand(int[] ids, float[] ideal, float[] cmd)
            {
                lock (m_oSagTwin)
                {
                    m_anSagTwinIds = (int[])ids.Clone(); m_afSagTwinIdeal = ideal; m_afSagTwinCmd = (float[])cmd.Clone();
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
                if (m == null || !m.HasJoint) { m_afSagTwinDelta = null; return; }
                int[] ids = null; float[] ideal = null, cmd = null;
                float[] shOmega = null, shAlpha = null, shDir = null;          // 그림자 출력 단의 방향·속도 추적 (그림자일 때만 — 실물 추적 대신)
                lock (m_oSagTwin)
                {
                    if (m_nSagTwinNoteSeq != m_nSagTwinUsedSeq && m_anSagTwinIds != null)
                    { ids = m_anSagTwinIds; ideal = m_afSagTwinIdeal; cmd = m_afSagTwinCmd; m_nSagTwinUsedSeq = m_nSagTwinNoteSeq; }
                }
                if (ids == null)
                {
                    // 그림자 출력 단 — 실물로는 나가지 않는다. 보정(켜져 있으면)·전환·방향·속도 추적을 그림자 상태 사본으로 돌린다
                    //   (SagComp_JointOutShadow — 실물 출력 단의 상태는 바꾸지 않는다, 10-03)
                    ids = (int[])m.JointIDs.Clone();
                    ideal = new float[ids.Length];
                    for (int i = 0; i < ids.Length; i++) ideal[i] = GetData(ids[i]);
                    cmd = (float[])ideal.Clone();
                    SagComp_JointOutShadow(ids, cmd, out shOmega, out shAlpha, out shDir);
                    lock (m_oSagTwin) m_nSagTwinUsedSeq = m_nSagTwinNoteSeq;   // 방금 그림자가 남긴 기록은 다시 쓰지 않는다
                }
                SagTwin_Predict(m, ids, ideal, cmd, shOmega, shAlpha, shDir);
                float[] li = new float[m.JointIDs.Length];
                for (int j = 0; j < li.Length; j++) { int k = Array.IndexOf(ids, m.JointIDs[j]); li[j] = k >= 0 ? ideal[k] : GetData(m.JointIDs[j]); }
                m_afSagTwinLastIdeal = li; m_SagTwinLastModel = m;
            }

            // 쌍둥이 = 보낸 명령 + 예측 오차 → 스냅샷 (쌍둥이 − 이상) 발행.
            //   shOmega·shAlpha·shDir = 그림자 출력 단의 방향·속도 추적 (ids 순, 그림자일 때) — null 이면 실물 출력 단의 추적 (실물 스트리밍)
            private void SagTwin_Predict(CSagComp m, int[] ids, float[] ideal, float[] cmd, float[] shOmega, float[] shAlpha, float[] shDir)
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
                // ① 엔코더가 보는 처짐 — 보낸 자세의 중력 지레, 이상 자세의 이동 방향
                float[] lev = SagComp_JointLevers(m.FuncNumber, m.JointIDs, pc);
                float[] twin = new float[nj];
                for (int j = 0; j < nj; j++)
                {
                    int id = m.JointIDs[j];
                    int ki = Array.IndexOf(ids, id);
                    float sg = SagTwin_Sigma(m, j, id, pi[j], (shDir != null && ki >= 0 && ki < shDir.Length) ? shDir[ki] : float.NaN);
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
                m_afSagTwinDelta = delta;
            }

            // 쌍둥이 전용 이동 방향 σ (−1..+1) — 출력 단(SagComp_JointOutModel)과 같은 식, 보정을 꺼도 돈다.
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
                        sg += (pose - prev) / (float)Math.Max(0.2, m.SigmaRamp * m.Jh[j]);
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
                        if (c.nName >= 0 && c.nName < afData.Length) c.fAngle = afData[c.nName] + (c.nName < d.Length ? d[c.nName] : 0f);
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
