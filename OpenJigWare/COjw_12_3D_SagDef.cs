// ================================================================
// C3d — 로봇 정의 (공구 끝 · 꺾는 손목 · 바닥으로 수직)  2026-10-04
//   측정·실습이 쓰는 로봇 정의를 3D 와 대조하고 3D 에 보여 준다. 정의의 정본은 <urdf>\sagarm.txt 의 7키
//   (robot·func·posids·wrist·holdids·tcplen·downsum — CSagArmSetup, CSagArmCfg). 다른 곳(모델 파일·실습 상태·⑨ 설정)은 채워 쓰거나 대조만 한다.
//   공구 끝(TCP) = IK 수식 func 체인의 끝 — 첫 '!' 바로 앞 DH 줄 ('!' 가 없으면 체인 마지막 DH 줄, Init=1 로 다시 시작하면 마지막 체인).
//     OMX follower 의 공구 끝은 손목 14 원점에서 공구 쪽 103.2mm (60mm 는 손목 롤 15 에서 잰 값).
//   꺾는 손목 = 공구를 바닥으로 수직으로 꺾는 관절 (공구 끝 앞 마지막 관절). 공구축 = 손목 원점 → 공구 끝.
//   바닥으로 수직 = 공구축이 3D 의 −Z — 계산은 관절 합 posids[1] + posids[2] + wrist = downsum (OMX 90, 미리 정해 두는 값).
//     3D 에서 잰 값과 0.5° 넘게 다르면 저장도 작업도 하지 않는다.
//   고정 규약: 아래 = 3D 의 −Z · 바닥은 3D 의 XY 평면과 평행 (높이는 바닥 찾기가 잰다) · 로봇 베이스는 수평 · 펜은 손목 원점 → 공구 끝 직선 위
//     · 공구 피치 Ry 90 = 이 정의의 수직 (Ry < 90 이면 관절 합 = downsum + (90 − Ry)).
//   정의 확인 SagDef_Check (V0~V9) — 계획 기구학 묶음(지금 3D 의 수식 글로 만든 C3d, COjw_12_3D_SagRun.cs)에서 관절 한계 없이 관절값 사본에 직접 써서
//     계산한다 (3D·묶음의 관절값을 바꾸지 않는다 — 어느 스레드에서 불러도 된다). 결과는 묶음 차례·정의·3D 의 그 수식 객체마다 캐시한다 (최근 8개).
//     작업 입구(SagArm_Open)·⑨ 입구(SagComp_Survey 등)·바닥 누름 입구(SagComp_FloorPress·FloorProbe)·계획·학습·실습 단계가 부른다.
//   3D 표시 ToolMark_* — 공구 끝(빨강 공)·공구축(손목 원점 → 공구 끝 — 초록 = 수직, 파랑 = 기울어짐)·손목 원점(주황 공)·수직 기준(회색 선),
//     손목 부품은 주황 (Second 색). 그리기는 쌍둥이 그리기 훅(SagTwin_DrawGhost) 맨 앞에서 — COjw_12_3D.cs 는 고치지 않는다.
//   공개 함수는 예외를 밖으로 던지지 않는다.
// ================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Tao.OpenGl;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            // ────────────────────────────────────────────────────────────
            // 상수·형
            // ────────────────────────────────────────────────────────────

            /// <summary>정의 확인 허용 — 길이 (mm): V5 공구 끝 거리, V8 3D 와 계획 기구학의 공구 끝 차</summary>
            public const float SAGDEF_TOL_MM = 0.5f;
            /// <summary>정의 확인 허용 — 각 (deg): V4 펜·V6 관절 합 모형·V7 수직 관절 합·V9 시험 IK, 모델·실습 상태의 downsum 대조</summary>
            public const float SAGDEF_TOL_DEG = 0.5f;
            /// <summary>V5 — 자세마다 손목 원점 → 공구 끝 길이의 흩어짐 한도 (mm)</summary>
            public const float SAGDEF_LEN_SPREAD_MM = 0.1f;
            /// <summary>V6 — 손목 원점이 베이스 축에서 이만큼 안이면 그 자세는 뺀다 (mm, 팔 평면 방향이 정해지지 않는다)</summary>
            public const float SAGDEF_MIN_R_MM = 5f;
            /// <summary>3D 표시 — 공구선이 수직에서 이만큼 안이면 초록, 넘으면 파랑 (deg, 경고색은 쓰지 않는다 — Ry 70 작업도 있다)</summary>
            public const float TOOLMARK_VERT_DEG = 1.5f;
            // V4·V5·V6·V7 자세 (요, 피치 1, 피치 2, 손목 — 정의의 관절에 그대로)
            private static readonly float[][] SAGDEF_POSES = { new float[] { 0, 0, 0, 0 }, new float[] { 0, 0, 40, 50 }, new float[] { 0, 0, 40, 70 },
                                                               new float[] { 20, 30, -20, 40 }, new float[] { -30, 10, 10, 100 } };
            // V8 자세 (지금 3D 확인 bFull 의 세 자세)
            private static readonly float[][] SAGDEF_V8_POSES = { new float[] { 0, 0, 40, 50 }, new float[] { 20, 30, -20, 80 }, new float[] { -30, 10, 10, 70 } };
            // V9 — 시험 점 자세의 피치 1·2 후보 (차례로, 공구 끝 반경 ≥ 30mm 인 첫 자세), 요
            private static readonly float[][] SAGDEF_V9_P12 = { new float[] { 0, 40 }, new float[] { -30, 60 }, new float[] { 30, 30 } };
            private static readonly float[] SAGDEF_V9_YAWS = { 0, 20 };
            // 3D 수식이 바뀐 뒤 계획 기구학을 아직 맞추지 않음 (작업·학습 중이면 끝난 뒤 — 스크립트는 UI 스레드에서 SagRun_Prepare). 화면 글이라 함수 이름은 넣지 않는다
            private const string SAGDEF_WHY_CHANGED = "계획 기구학이 지금 3D 모델과 다름 (작업·학습 중이면 끝난 뒤 맞춘다)";

            /// <summary>정의 확인 실패 종류 — None 통과, NoKin 계획 기구학 없음 (앱을 다시 켜기), NoModel 3D 에 수식이 하나도 없음,
            /// Def 정의가 3D 와 맞지 않음 (정의 바꾸기), Model 3D 와 계획 기구학이 다름, Changed 3D 수식이 바뀐 뒤 계획 기구학을 다시 만들지 않음</summary>
            public enum ESagDefFail { None, NoKin, NoModel, Def, Model, Changed }

            /// <summary>정의 확인 결과 (SagDef_Check)</summary>
            public class CSagDefCheck
            {
                public bool Ok; public ESagDefFail Kind; public string Why = "";          // Why = 첫 실패의 쉬운 한국어 한 줄
                public List<string> Items = new List<string>();                          // "V1 맞음 …" / "V3 ★…" 줄 (툴팁·보고서)
                public float TcpLenMm = float.NaN, TcpLenSpreadMm = float.NaN, DownSumMeasDeg = float.NaN, SpreadDeg = float.NaN,
                             LateralDeg = float.NaN, PenOffDeg = float.NaN, IkTiltDeg = float.NaN, KinDiffMm = float.NaN;
                public int[] ChainIDs = new int[0], Formulas = new int[0];               // 공구 끝 체인의 관절 (처음 나온 차례), 3D 에서 글이 있는 수식 슬롯
                public string TcpLine = "", KinSig = ""; public bool BuiltIn; public int Gen;
                public string Summary = "";   // 쉬운 말: "공구 끝 = 수식 0 끝 · 꺾는 손목 = 14번 · 바닥으로 수직 = 3D 와 맞음"
                public string Detail = "";    // 숫자: 공구 끝 줄 글, 손목→끝 거리, 잰 수직 합(흩어짐), 펜, 시험 IK, 기구학 표시
                /// <summary>사본 (Items 는 새 목록) — 캐시된 결과를 바꾸지 않게</summary>
                internal CSagDefCheck Copy() { var r = (CSagDefCheck)MemberwiseClone(); r.Items = new List<string>(Items); return r; }
            }

            // ────────────────────────────────────────────────────────────
            // 정의 확인
            // ────────────────────────────────────────────────────────────

            private readonly object m_oSagDefCache = new object();
            private readonly List<KeyValuePair<string, CSagDefCheck>> m_lstSagDefCache = new List<KeyValuePair<string, CSagDefCheck>>();
            private const int SAGDEF_CACHE_MAX = 8;
            // 캐시 비우기 (계획 기구학 묶음을 새로 만들 때)
            private void SagDef_CacheClear() { lock (m_oSagDefCache) m_lstSagDefCache.Clear(); }

            /// <summary>로봇 정의 확인 (V0~V9) — c 가 null 이면 기본값(OMX follower). 계획 기구학 묶음에서 관절 한계 없이 관절값 사본으로 계산한다
            /// (3D·묶음의 관절값을 바꾸지 않는다, 어느 스레드에서도 된다 — UI(STA) 스레드에서 부르면 바뀐 수식 글로 묶음을 먼저 새로 만든다).
            /// V0 묶음 있음·3D 수식이 묶음과 같은 기구학 (글이 같거나 해석된 DH 행이 같음) · V1 관절 번호 (posids 3 + 손목이 서로 다르고 0~253, holdids 는 움직이는 관절·공구 끝 체인과 겹치지 않음)
            /// · V2 3D 에 수식 func 의 글이 있고 체인에 행이 있음 · V3 체인 관절이 처음 나온 차례로 정확히 [posids0, posids1, posids2, wrist] (손목 뒤 관절 없음)
            /// · V4 펜이 손목 원점 → 공구 끝 직선 위 (0.5°) · V5 손목 원점 → 공구 끝 길이 일정 (0.1mm), tcplen 과 0.5mm 안 (NaN 이면 생략)
            /// · V6 공구 방향이 관절 합 모형 (D = 관절 합 − 공구 각 + 90 일정, 팔 평면 안 — 0.5°) · V7 잰 수직 관절 합 D̄ 가 downsum 과 0.5° 안 (NaN 이면 생략)
            /// · V8 3D 와 계획 기구학의 공구 끝 차 0.5mm 안 · V9 수직 자세 시험 점을 관절 합 IK 로 풀고 그 공구가 −Z 에서 0.5° 안. 캐시. 예외 없음.</summary>
            public CSagDefCheck SagDef_Check(CSagArmCfg c)
            {
                using (SagRun_KinUse()) return SagDef_CheckBody(c);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private CSagDefCheck SagDef_CheckBody(CSagArmCfg c)
            {
                try
                {
                    if (c == null) c = new CSagArmCfg();
                    CSagRunKin st = SagRun_KinState();
                    if (st == null) return SagDef_Fail0(ESagDefFail.NoKin, m_strSagRunKinErr.Length > 0 ? m_strSagRunKinErr : SAGRUN_NO_KIN, null);   // 만들거나 바꾸지 못했으면 그 까닭
                    if (!SagRun_SameKinematics(st)) return SagDef_Fail0(ESagDefFail.Changed, SAGDEF_WHY_CHANGED, st);   // 글이 바뀌었어도 DH 행이 같으면 그대로
                    string key = SagDef_Key(st, c);
                    lock (m_oSagDefCache)
                        foreach (KeyValuePair<string, CSagDefCheck> kv in m_lstSagDefCache) if (kv.Key == key) return kv.Value;
                    CSagDefCheck r = SagDef_Compute(st, c);
                    lock (m_oSagDefCache)
                    {
                        m_lstSagDefCache.Add(new KeyValuePair<string, CSagDefCheck>(key, r));
                        while (m_lstSagDefCache.Count > SAGDEF_CACHE_MAX) m_lstSagDefCache.RemoveAt(0);
                    }
                    return r;
                }
                catch (Exception ex)
                {
                    var r = new CSagDefCheck { Kind = ESagDefFail.Def, Why = "★오류: " + SagRun_Msg(ex) };
                    r.Items.Add("★" + r.Why); r.Summary = "★로봇 정의를 확인하지 못함"; return r;
                }
            }
            // 캐시 키 — 묶음 차례 + 정의 + 3D 의 그 수식 객체 (같은 글로 다시 해석해 객체가 바뀌어도 다시 본다 — V8)
            private string SagDef_Key(CSagRunKin st, CSagArmCfg c)
            {
                var ci = CultureInfo.InvariantCulture;
                object dh3 = (m_CHeader != null && m_CHeader.pDhParamAll != null && c.FuncNumber >= 0 && c.FuncNumber < m_CHeader.pDhParamAll.Length) ? m_CHeader.pDhParamAll[c.FuncNumber] : null;
                int n3 = dh3 == null ? -1 : ((CDhParamAll)dh3).GetCount();
                return string.Format(ci, "{0}|f{1}|p{2}|w{3}|h{4}|t{5:R}|d{6:R}|o{7}:{8}", st.Gen, c.FuncNumber, SagRun_Ints(c.PosIDs), c.WristID, SagRun_Ints(c.HoldIDs),
                                     c.TcpLenMm, c.DownSumDeg, dh3 == null ? 0 : RuntimeHelpers.GetHashCode(dh3), n3);
            }
            private CSagDefCheck SagDef_Fail0(ESagDefFail k, string why, CSagRunKin st)
            {
                var r = new CSagDefCheck { Kind = k, Why = why, Gen = st == null ? 0 : st.Gen, BuiltIn = st != null && st.BuiltIn, Formulas = SagDef_Formulas() };
                r.Items.Add("V0 ★" + why);
                r.Summary = k == ESagDefFail.NoKin ? "★계획 기구학 없음 — 앱을 다시 켜십시오" : "★계획 기구학이 지금 3D 모델과 다름";   // 탭 다시 열기 안내는 앱이 종류별로 붙인다
                return r;
            }
            // 본체 — 묶음 st 에서 V1~V9 (첫 실패가 Why·Kind, 뒤 확인도 할 수 있는 만큼 해서 Items 에)
            private CSagDefCheck SagDef_Compute(CSagRunKin st, CSagArmCfg c)
            {
                var ci = CultureInfo.InvariantCulture;
                var r = new CSagDefCheck { Gen = st.Gen, BuiltIn = st.BuiltIn, Formulas = SagDef_Formulas() };
                Action<ESagDefFail, string, string> Fail = (k, item, why) => { r.Items.Add(item + " ★" + why); if (r.Kind == ESagDefFail.None) { r.Kind = k; r.Why = why; } };
                Action<string> Pass = s => r.Items.Add(s);
                int f = c.FuncNumber, w = c.WristID;
                int[] pos = c.PosIDs;
                // ── V1 관절 번호 ──
                bool bIds = c.IdsOk;
                if (bIds) foreach (int id in c.MoveIDs) if (id < 0 || id > 253) bIds = false;
                if (!bIds) { Fail(ESagDefFail.Def, "V1", "관절 번호가 겹치거나 0~253 밖 (베이스·피치 2개·손목 — posids " + SagRun_Ints(pos) + ", wrist " + w.ToString(ci) + ")"); return SagDef_Finish(r, c); }
                int[] mv = c.MoveIDs;
                int[] hold = c.HoldIDs ?? new int[0];
                foreach (int h in hold)
                    if (Array.IndexOf(mv, h) >= 0) { Fail(ESagDefFail.Def, "V1", string.Format(ci, "토크만 관절 {0}번이 움직이는 관절과 겹침", h)); return SagDef_Finish(r, c); }
                // ── V2 수식 ──
                string[] a3 = m_CHeader != null ? m_CHeader.pstrKinematics : null;
                if (r.Formulas.Length == 0) { Fail(ESagDefFail.NoModel, "V2", "3D 에 로봇 모델(수식)이 없음"); return SagDef_Finish(r, c); }
                string txt = (a3 != null && f >= 0 && f < a3.Length) ? a3[f] : null;
                if (string.IsNullOrEmpty(txt))
                {
                    CDhParamAll dh3 = (m_CHeader != null && m_CHeader.pDhParamAll != null && f >= 0 && f < m_CHeader.pDhParamAll.Length) ? m_CHeader.pDhParamAll[f] : null;
                    if (dh3 != null && dh3.GetCount() > 0) Fail(ESagDefFail.Def, "V2", string.Format(ci, "수식 {0} 은 여러 체인을 합친 전체 수식 — 체인 하나의 끝을 고르십시오", f));
                    else Fail(ESagDefFail.Def, "V2", string.Format(ci, "수식 {0} 이 3D 에 없음 (있는 수식: {1})", f, SagRun_Ints(r.Formulas)));
                    return SagDef_Finish(r, c);
                }
                CDhParamAll dh = (st.Kin != null && st.Kin.m_CHeader != null && f < st.Kin.m_CHeader.pDhParamAll.Length) ? st.Kin.m_CHeader.pDhParamAll[f] : null;
                int nRow = dh == null ? 0 : dh.GetCount();
                if (nRow <= 0) { Fail(ESagDefFail.Def, "V2", string.Format(ci, "수식 {0} 의 DH 줄을 읽지 못함 (공구 끝 체인이 비었음)", f)); return SagDef_Finish(r, c); }
                r.ChainIDs = SagDef_ChainOf(dh);
                r.TcpLine = SagDef_TcpLine(f);
                r.KinSig = SagRun_KinSig(st, f);
                Pass(string.Format(ci, "V2 맞음 — 수식 {0}: DH {1}줄, 공구 끝 줄 '{2}'", f, nRow, r.TcpLine));
                // ── V3 체인 ──
                int[] ch = r.ChainIDs;
                string strCh = SagRun_Ints(ch);
                int iw = Array.IndexOf(ch, w);
                int[] exp = { pos[0], pos[1], pos[2], w };
                string[] names = { "베이스(요)", "피치 관절", "피치 관절", "꺾는 손목" };
                string v3 = null;
                if (iw < 0) v3 = string.Format(ci, "꺾는 손목 {0}번이 공구 끝 체인에 없음 (체인: {1} — '!' 뒤 관절은 공구 끝을 움직이지 않음)", w, strCh);
                else if (iw < ch.Length - 1) v3 = string.Format(ci, "손목 {0}번과 공구 끝 사이에 관절 {1}번 (체인: {2})", w, ch[iw + 1], strCh);
                else
                {
                    for (int k = 0; k < 3 && v3 == null; k++) if (Array.IndexOf(ch, exp[k]) < 0) v3 = string.Format(ci, "{0} {1}번이 공구 끝 체인에 없음 (체인: {2})", names[k], exp[k], strCh);
                    for (int k = 0; k < ch.Length && v3 == null; k++) if (Array.IndexOf(exp, ch[k]) < 0) v3 = string.Format(ci, "공구 끝 체인에 정의 밖 관절 {0}번 (체인: {1})", ch[k], strCh);
                    if (v3 == null && (ch.Length != 4 || ch[0] != exp[0] || ch[1] != exp[1] || ch[2] != exp[2] || ch[3] != exp[3]))
                        v3 = string.Format(ci, "체인 순서가 베이스→피치→피치→손목이 아님 (체인: {0})", strCh);
                }
                if (v3 == null) foreach (int h in hold) if (Array.IndexOf(ch, h) >= 0) { v3 = string.Format(ci, "토크만 관절 {0}번이 공구 끝 체인 안에 있음 (움직이면 공구 끝이 움직임)", h); break; }
                if (v3 != null) { Fail(ESagDefFail.Def, "V3", v3); return SagDef_Finish(r, c); }
                Pass(string.Format(ci, "V1·V3 맞음 — 체인 {0} = 베이스 {1} → 피치 {2}·{3} → 손목 {4}{5}", strCh, pos[0], pos[1], pos[2], w,
                    hold.Length > 0 ? " (토크만 " + SagRun_Ints(hold) + " 은 체인 밖)" : ""));
                // ── V4~V7 (자세 5개) ──
                int iW = -1;
                for (int i = 0; i < nRow; i++) { CDhParam p = dh.GetData(i); if (p.nAxisNum == w) { iW = i; break; } }
                int iLen = -1;                                                       // 마지막 '길이 있는' 줄 (펜 조각)
                for (int i = nRow - 1; i >= 0; i--)
                {
                    CDhParam p = dh.GetData(i);
                    bool bPris = p.nAxisNum >= 0 && (p.nAxisDir == 2 || p.nAxisDir == 3);
                    if (Math.Abs(p.dA) > 1e-6 || Math.Abs(p.dD) > 1e-6 || bPris) { iLen = i; break; }
                }
                double penMax = 0, lenMin = double.MaxValue, lenMax = double.MinValue, lenSum = 0, latMax = 0;
                var lstD = new List<double>();
                int nLen = 0;
                foreach (float[] q in SAGDEF_POSES)
                {
                    double[] ad = SagDef_Ad(c, q);
                    List<double[]> pts; double[] zT;
                    SagDef_ChainPts(dh, ad, out pts, out zT);
                    double[] W = pts[iW], T = pts[nRow];
                    double vx = T[0] - W[0], vy = T[1] - W[1], vz = T[2] - W[2], L = Math.Sqrt(vx * vx + vy * vy + vz * vz);
                    // V4 — 펜 조각 (공구 끝 − 그 줄 앞 위치) 과 공구축 (공구 끝 − 손목 원점) 의 각
                    if (iLen >= 0 && iLen > iW)
                    {
                        double[] P = pts[iLen];
                        double px = T[0] - P[0], py = T[1] - P[1], pz = T[2] - P[2], pl = Math.Sqrt(px * px + py * py + pz * pz);
                        double dw = Math.Sqrt((P[0] - W[0]) * (P[0] - W[0]) + (P[1] - W[1]) * (P[1] - W[1]) + (P[2] - W[2]) * (P[2] - W[2]));
                        if (dw > 1e-6 && pl > 1e-9 && L > 1e-9)
                        {
                            double cs = (px * vx + py * vy + pz * vz) / (pl * L);
                            penMax = Math.Max(penMax, Math.Acos(Math.Max(-1.0, Math.Min(1.0, cs))) * 180.0 / Math.PI);
                        }
                    }
                    // V5
                    lenMin = Math.Min(lenMin, L); lenMax = Math.Max(lenMax, L); lenSum += L; nLen++;
                    // V6·V7 — 손목 원점의 수평 방향 û (베이스 축 위면 뺀다), 공구 각 φ = atan2(−v·z, v·û) (0 바깥 수평, 90 아래, 180 안쪽 수평)
                    double ur = Math.Sqrt(W[0] * W[0] + W[1] * W[1]);
                    if (ur < SAGDEF_MIN_R_MM || L < 1e-9) continue;
                    double ux = W[0] / ur, uy = W[1] / ur;
                    double phi = Math.Atan2(-vz, vx * ux + vy * uy) * 180.0 / Math.PI;
                    double S = (double)q[1] + q[2] + q[3];
                    double D = S - phi + 90.0;
                    if (lstD.Count > 0) { while (D - lstD[0] > 180) D -= 360; while (D - lstD[0] <= -180) D += 360; }
                    lstD.Add(D);
                    double lat = Math.Asin(Math.Min(1.0, Math.Abs(vx * (-uy) + vy * ux) / L)) * 180.0 / Math.PI;
                    latMax = Math.Max(latMax, lat);
                }
                r.PenOffDeg = (float)penMax;
                if (nLen > 0) { r.TcpLenMm = (float)(lenSum / nLen); r.TcpLenSpreadMm = (float)(lenMax - lenMin); }
                if (penMax > SAGDEF_TOL_DEG)
                    Fail(ESagDefFail.Def, "V4", string.Format(ci, "펜이 손목→공구 끝 직선에서 {0:F1}° 벗어남 — '바닥으로 수직' 을 관절 합으로 정할 수 없음", penMax));
                else Pass(string.Format(ci, "V4 맞음 — 펜이 손목→공구 끝 직선 위 ({0:F2}°)", penMax));
                if (!(r.TcpLenSpreadMm <= SAGDEF_LEN_SPREAD_MM))
                    Fail(ESagDefFail.Def, "V5", string.Format(ci, "손목→공구 끝 거리가 자세마다 다름 ({0:F1}~{1:F1}mm — 손목 뒤에 움직이는 관절?)", lenMin, lenMax));
                else if (!float.IsNaN(c.TcpLenMm) && !(Math.Abs(r.TcpLenMm - c.TcpLenMm) <= SAGDEF_TOL_MM))
                    Fail(ESagDefFail.Def, "V5", string.Format(ci, "손목→공구 끝 {0:F1} mm ≠ 정의 {1:0.0} mm", r.TcpLenMm, c.TcpLenMm));
                else Pass(string.Format(ci, "V5 맞음 — 손목→공구 끝 {0:F1}mm (흩어짐 {1:F2}mm{2})", r.TcpLenMm, r.TcpLenSpreadMm,
                                         float.IsNaN(c.TcpLenMm) ? ", 정의 거리 확인 안 함" : string.Format(ci, ", 정의 {0:0.0}", c.TcpLenMm)));
                if (lstD.Count < 3)
                    Fail(ESagDefFail.Def, "V6", string.Format(ci, "관절 합 모형을 볼 자세가 모자람 ({0}개 — 손목 원점이 베이스 축 위)", lstD.Count));
                else
                {
                    double sum = 0; foreach (double d in lstD) sum += d;
                    double mean = sum / lstD.Count, spread = 0;
                    foreach (double d in lstD) spread = Math.Max(spread, Math.Abs(d - mean));
                    while (mean > 180) mean -= 360; while (mean <= -180) mean += 360;
                    r.DownSumMeasDeg = (float)mean; r.SpreadDeg = (float)spread; r.LateralDeg = (float)latMax;
                    if (spread > SAGDEF_TOL_DEG)
                        Fail(ESagDefFail.Def, "V6", string.Format(ci, "공구 방향이 관절 합으로 설명되지 않음 (최대 {0:F1}° — 피치 관절·손목 축이 평행하지 않거나 방향이 반대)", spread));
                    else if (latMax > SAGDEF_TOL_DEG)
                        Fail(ESagDefFail.Def, "V6", string.Format(ci, "공구가 팔 평면 밖으로 {0:F1}° (베이스 축과 팔 평면이 어긋남)", latMax));
                    else Pass(string.Format(ci, "V6 맞음 — 공구 방향 = 관절 합 모형 (흩어짐 {0:F2}°, 팔 평면 밖 {1:F2}°)", spread, latMax));
                    if (!float.IsNaN(c.DownSumDeg) && !(Math.Abs(mean - c.DownSumDeg) <= SAGDEF_TOL_DEG))
                        Fail(ESagDefFail.Def, "V7", string.Format(ci, "3D 에서 수직 = 관절 합 {0:F1}° ≠ 정의 {1:0.###}°", mean, c.DownSumDeg));
                    else Pass(string.Format(ci, "V7 맞음 — 3D 에서 수직 = 관절 합 {0:F2}°{1}", mean, float.IsNaN(c.DownSumDeg) ? " (정의 값 없음 — 잰 값을 씀)" : string.Format(ci, " (정의 {0:0.###})", c.DownSumDeg)));
                }
                // ── V8 3D 와 계획 기구학 ──
                {
                    double worst = 0;
                    foreach (float[] q in SAGDEF_V8_POSES)
                    {
                        double[] a1 = SagDef_Ad(c, q), a2 = SagDef_Ad(c, q);
                        float x1, y1, z1, x2, y2, z2;
                        SagFc_Fk(f, a1, -1, out x1, out y1, out z1);
                        st.Kin.SagFc_Fk(f, a2, -1, out x2, out y2, out z2);
                        double d = Math.Sqrt((double)(x1 - x2) * (x1 - x2) + (double)(y1 - y2) * (y1 - y2) + (double)(z1 - z2) * (z1 - z2));
                        if (double.IsNaN(d) || double.IsInfinity(d)) d = double.MaxValue;
                        worst = Math.Max(worst, d);
                    }
                    r.KinDiffMm = (float)Math.Min(worst, 1e9);
                    if (worst > SAGDEF_TOL_MM)
                        Fail(ESagDefFail.Model, "V8", worst >= 1e6 ? "3D 와 계획 기구학의 공구 끝을 비교하지 못함"                       // 안내(탭 다시 열기)는 앱이 종류별로 붙인다
                                                                  : string.Format(ci, "3D 와 계획 기구학의 공구 끝이 {0:F1} mm 다름", worst));
                    else Pass(string.Format(ci, "V8 맞음 — 3D 와 계획 기구학의 공구 끝 차 {0:F2}mm", worst));
                }
                // ── V9 수직 자세 시험 점 ──
                {
                    float d = float.IsNaN(c.DownSumDeg) ? r.DownSumMeasDeg : c.DownSumDeg;
                    string v9 = null; double tiltMax = 0; int nTry = 0;
                    if (float.IsNaN(d)) v9 = "수직 관절 합을 정할 수 없어 시험 IK 를 하지 못함";
                    else
                    {
                        CSagArmCfg c2 = c.Clone(); c2.DownSumDeg = d;
                        foreach (float yaw in SAGDEF_V9_YAWS)
                        {
                            double[] T = null;
                            foreach (float[] p12 in SAGDEF_V9_P12)
                            {
                                double[] ad = SagDef_Ad(c, new float[] { yaw, p12[0], p12[1], d - p12[0] - p12[1] });
                                float tx, ty, tz;
                                st.Kin.SagFc_Fk(f, ad, -1, out tx, out ty, out tz);
                                if (Math.Sqrt((double)tx * tx + (double)ty * ty) >= 30) { T = new double[] { tx, ty, tz }; break; }
                            }
                            if (T == null) { v9 = string.Format(ci, "수직 자세 시험 점을 찾지 못함 (요 {0:0}, 공구 끝이 베이스 축 가까이)", yaw); break; }
                            nTry++;
                            float bend;
                            float[] qs = SagArm_IkSum(st.Kin, c2, SagRun_Seed(st.Kin, c2, yaw, 90f), (float)T[0], (float)T[1], (float)T[2], d, out bend);
                            if (qs == null) { v9 = string.Format(ci, "수직 자세 시험 점을 IK 로 풀지 못함 (요 {0:0}, ({1:F0}, {2:F0}, {3:F0}))", yaw, T[0], T[1], T[2]); break; }
                            double[] a = SagDef_Ad(c, qs);
                            List<double[]> pts; double[] zT;
                            SagDef_ChainPts(dh, a, out pts, out zT);
                            double[] W = pts[iW], E = pts[nRow];
                            double vx = E[0] - W[0], vy = E[1] - W[1], vz = E[2] - W[2], L = Math.Sqrt(vx * vx + vy * vy + vz * vz);
                            double tilt = L > 1e-9 ? Math.Acos(Math.Max(-1.0, Math.Min(1.0, -vz / L))) * 180.0 / Math.PI : 180.0;
                            tiltMax = Math.Max(tiltMax, tilt);
                        }
                    }
                    if (v9 == null) r.IkTiltDeg = (float)tiltMax;
                    if (v9 == null && tiltMax > SAGDEF_TOL_DEG) v9 = string.Format(ci, "시험 점 IK 의 공구가 수직에서 {0:F1}° 기욺", tiltMax);
                    if (v9 != null) Fail(ESagDefFail.Def, "V9", v9);
                    else Pass(string.Format(ci, "V9 맞음 — 수직 자세 시험 점 {0}개를 관절 합 {1:0.##} IK 로 풂, 공구 기울기 {2:F2}°", nTry, d, tiltMax));
                }
                return SagDef_Finish(r, c);
            }
            // 끝 — Ok·요약·숫자 글
            private CSagDefCheck SagDef_Finish(CSagDefCheck r, CSagArmCfg c)
            {
                var ci = CultureInfo.InvariantCulture;
                r.Ok = r.Kind == ESagDefFail.None;
                int[] pos = c.PosIDs ?? new int[0];
                string strSum = pos.Length == 3 ? string.Format(ci, "{0}+{1}+{2}", pos[1], pos[2], c.WristID) : "관절 합";
                if (r.Ok)
                    r.Summary = string.Format(ci, "공구 끝 = 수식 {0} 끝 · 꺾는 손목 = {1}번 · 바닥으로 수직 = 3D 와 맞음", c.FuncNumber, c.WristID);
                else if (r.Kind == ESagDefFail.NoModel) r.Summary = "★3D 에 로봇 모델 없음";
                else r.Summary = "★로봇 정의 확인 안 됨 — " + r.Why;
                var parts = new List<string>();
                if (r.TcpLine.Length > 0)
                    parts.Add(string.Format(ci, "공구 끝 = 수식 {0} 끝 '{1}'{2}", c.FuncNumber, r.TcpLine,
                        float.IsNaN(r.TcpLenMm) ? "" : string.Format(ci, " — 손목 {0} 축에서 {1:F1} mm", c.WristID, r.TcpLenMm)));
                if (!float.IsNaN(r.DownSumMeasDeg))
                    parts.Add(string.Format(ci, "바닥으로 수직 = 관절 합 {0} = {1}° (3D {2:F2}°, 흩어짐 {3:F2}°)", strSum,
                        float.IsNaN(c.DownSumDeg) ? "—" : c.DownSumDeg.ToString("0.###", ci), r.DownSumMeasDeg, r.SpreadDeg));
                var nums = new List<string>();
                if (!float.IsNaN(r.PenOffDeg)) nums.Add(string.Format(ci, "펜 {0:F2}°", r.PenOffDeg));
                if (!float.IsNaN(r.IkTiltDeg)) nums.Add(string.Format(ci, "시험 IK {0:F2}°", r.IkTiltDeg));
                if (r.KinSig.Length > 0) nums.Add("기구학 " + r.KinSig);
                if (r.BuiltIn) nums.Add("★3D 에 수식 없음 — 내장 OMX");
                if (nums.Count > 0) parts.Add(string.Join(" · ", nums.ToArray()));
                r.Detail = string.Join(" / ", parts.ToArray());
                return r;
            }
            // 정의의 관절에 자세 q (요, 피치 1, 피치 2, 손목) 를 직접 쓴 관절값 사본 (다른 관절 0 — 관절 한계 없음, 지금 3D 자세와 무관)
            private static double[] SagDef_Ad(CSagArmCfg c, float[] q)
            {
                var ad = new double[256];
                int[] ids = c.MoveIDs;
                for (int k = 0; k < 4 && k < q.Length; k++) if (ids[k] >= 0 && ids[k] < ad.Length) ad[ids[k]] = q[k];
                return ad;
            }
            // 체인의 관절 (nAxisNum ≥ 0) — 처음 나온 차례
            private static int[] SagDef_ChainOf(CDhParamAll dh)
            {
                var l = new List<int>();
                int n = dh == null ? 0 : dh.GetCount();
                for (int i = 0; i < n; i++) { CDhParam p = dh.GetData(i); if (p.nAxisNum >= 0 && !l.Contains(p.nAxisNum)) l.Add(p.nAxisNum); }
                return l.ToArray();
            }

            /// <summary>DH 체인의 줄마다 위치 — pts[0] = 원점 (첫 줄 앞), pts[i + 1] = i 번째 줄 뒤 (마지막 = 공구 끝), zT = 끝 좌표계 Z 축.
            /// SagFc_Fk 와 같은 CMath.CalcT·CalcMatrix 차례 (관절 i 의 원점 = 그 관절이 처음 나오는 줄 앞 위치). 관절값 사본 ad 그대로 (한계 없음)</summary>
            private static void SagDef_ChainPts(CDhParamAll dh, double[] ad, out List<double[]> pts, out double[] zT)
            {
                pts = new List<double[]>(); zT = new double[] { 0, 0, 1 };
                double[,] adR;
                Ojw.CMath.CalcT(0, 0, 0, 0, out adR);
                pts.Add(new double[] { adR[0, 3], adR[1, 3], adR[2, 3] });
                int n = dh == null ? 0 : dh.GetCount();
                for (int i = 0; i < n; i++)
                {
                    CDhParam p = dh.GetData(i);
                    double dAng = (p.nAxisNum >= 0 && p.nAxisNum < ad.Length) ? ad[p.nAxisNum] : 0;
                    double dTheta = p.dTheta, dA = p.dA, dD = p.dD;
                    if ((p.nAxisDir < 2) || (p.nAxisDir >= 4)) dTheta += (((p.nAxisNum >= 0) ? dAng : 0) * (((p.nAxisDir == 0) || (p.nAxisDir == 4)) ? 1.0f : -1.0f));
                    else dD += (((p.nAxisNum >= 0) ? dAng : 0) * ((p.nAxisDir == 2) ? 1.0f : -1.0f));
                    double[,] aT;
                    Ojw.CMath.CalcT(dA, p.dAlpha, dD, dTheta, out aT);
                    Ojw.CMath.CalcMatrix(4, adR, aT, out adR);
                    pts.Add(new double[] { adR[0, 3], adR[1, 3], adR[2, 3] });
                }
                zT = new double[] { adR[0, 2], adR[1, 2], adR[2, 2] };
            }

            /// <summary>3D 에서 수식 글이 있는 슬롯 번호 (공구 끝 후보). 앱이 합쳐 둔 글 없는 '전체 수식' 슬롯은 저절로 빠진다. 예외 없음</summary>
            public int[] SagDef_Formulas()
            {
                var l = new List<int>();
                try
                {
                    string[] a = m_CHeader != null ? m_CHeader.pstrKinematics : null;
                    if (a != null) for (int i = 0; i < a.Length; i++) if (!string.IsNullOrEmpty(a[i])) l.Add(i);
                }
                catch { }
                return l.ToArray();
            }
            /// <summary>수식 nFunc 의 공구 끝 체인 관절 (처음 나온 차례 — 계획 기구학 묶음, '!' 뒤 관절은 없다). 없으면 빈 배열. 예외 없음</summary>
            public int[] SagDef_ChainIDs(int nFunc)
            {
                using (SagRun_KinUse()) return SagDef_ChainIDsBody(nFunc);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private int[] SagDef_ChainIDsBody(int nFunc)
            {
                try
                {
                    CSagRunKin st = SagRun_KinState();
                    if (st == null || st.Kin == null || nFunc < 0 || nFunc >= st.Kin.m_CHeader.pDhParamAll.Length) return new int[0];
                    return SagDef_ChainOf(st.Kin.m_CHeader.pDhParamAll[nFunc]);
                }
                catch { return new int[0]; }
            }
            /// <summary>3D 의 수식 nFunc 글에서 공구 끝 DH 줄의 원문 — 해석기와 같은 전처리 ('//' 뒤 자름, 빈칸 지움, 빈 줄·'@'·해석 안 되는 줄 건너뜀,
            /// 'XYZ=' 줄 건너뜀, '!' 에서 멈춤 — 다음 Init=1 줄에서 다시, Init=1 이면 체인 다시 시작) — '!' 가 없으면 체인 마지막 DH 줄.
            /// 예: [0,60,0,0],[-1,0,0],[2,0]. 없으면 "". 예외 없음</summary>
            public string SagDef_TcpLine(int nFunc)
            {
                try
                {
                    string[] a = m_CHeader != null ? m_CHeader.pstrKinematics : null;
                    string txt = (a != null && nFunc >= 0 && nFunc < a.Length) ? a[nFunc] : null;
                    if (string.IsNullOrEmpty(txt)) return "";
                    string last = "";
                    bool bStop = false;
                    foreach (string raw in txt.Replace("\r", "").Split('\n'))
                    {
                        string ln = raw; int c = ln.IndexOf("//"); if (c >= 0) ln = ln.Substring(0, c);
                        ln = ln.Replace(" ", "").Trim();
                        if (ln.Length == 0) continue;
                        string dt = ln.Replace("[", "").Replace("]", "");
                        if (dt.ToUpperInvariant().StartsWith("XYZ=")) continue;
                        if (dt[0] == '!') { bStop = true; continue; }
                        CDhParam p;
                        bool bParse = CKinematics.CForward.StringLine_To_Class_DHParam(dt, out p);
                        if (bStop) { if (bParse && p.nInit == 1) bStop = false; else continue; }
                        if (!bParse) continue;
                        last = ln;
                    }
                    return last;
                }
                catch { return ""; }
            }
            /// <summary>정의 서명 "f0|p11,12,13|w14|d90|k1A2B3C4D" (공구 끝 수식·관절·수직 관절 합·지금 3D 의 그 수식 기구학 표시 — 이름·holdids·tcplen 은 넣지 않는다).
            /// 실습 상태(def·eyedef)가 1단계와 같은 정의인지 대조한다. 예외 없음</summary>
            public string SagDef_Sig(CSagArmCfg c)
            {
                using (SagRun_KinUse()) return SagDef_SigBody(c);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private string SagDef_SigBody(CSagArmCfg c)
            {
                try
                {
                    if (c == null) c = new CSagArmCfg();
                    string k = SagRun_KinSig(SagRun_KinState(), c.FuncNumber);
                    return SagDef_SigText(c, k);
                }
                catch { return ""; }
            }
            private static string SagDef_SigText(CSagArmCfg c, string strKin)
            {
                var ci = CultureInfo.InvariantCulture;
                return string.Format(ci, "f{0}|p{1}|w{2}|d{3}|k{4}", c.FuncNumber, SagRun_Ints(c.PosIDs), c.WristID, c.DownSumDeg.ToString("0.###", ci), strKin ?? "");
            }
            // 내장 OMX 정의의 서명 (기구학 표시 = 내장 OMX 수식 0) — 정의 칸이 없는 옛 실습 상태는 이 정의로 했다
            private static string SagDef_OmxSig() { return SagDef_SigText(new CSagArmCfg(), s_strOmxKinSig ?? ""); }
            /// <summary>공구 피치 Ry → 관절 합 (표시용) — Ry 90 = downsum, Ry &lt; 90 = downsum + (90 − Ry). c 가 null 이면 downsum 90</summary>
            public static float SagDef_SumFor(CSagArmCfg c, float fRy) { return SagArm_SumFor(c, fRy); }
            /// <summary>모델(결과·적용 중 파일)을 만든 기구학(kinsig, 빈 값 = 내장 OMX)이 지금 3D 의 그 수식과 다르면 경고 한 줄, 같으면 "" (앱 표시용, 막지 않는다). 예외 없음</summary>
            public string SagDef_ModelKinWarn(CSagComp m)
            {
                using (SagRun_KinUse()) return SagDef_ModelKinWarnBody(m);   // 계획 기구학을 쓰는 동안 묶음을 바꾸지 않는다 (SagRun_KinUse)
            }
            private string SagDef_ModelKinWarnBody(CSagComp m)
            {
                try
                {
                    if (m == null) return "";
                    CSagRunKin st = SagRun_KinState();
                    if (st == null) return "";
                    string now = SagRun_KinSig(st, m.FuncNumber), mk = SagRun_KinSigNorm(m.KinSig);
                    if (now.Length > 0 && string.Equals(now, mk, StringComparison.Ordinal)) return "";
                    return "★모델을 만든 기구학과 지금 3D 가 다름 (모델 " + (mk.Length > 0 ? mk : "?") + " ≠ 지금 3D " + (now.Length > 0 ? now : "없음") + ") — 다시 학습 권장";
                }
                catch { return ""; }
            }

            /// <summary>⑨ 처짐보상 측정·검증 입구 확인 — cfg 의 func·posids·wrist, 토크만 관절 없음, 공구 끝 거리 확인 안 함, 수직 관절 합 = cfg.DownSumDeg
            /// (NaN 이면 3D 에서 잰 값을 쓴다 — V7 생략) 로 SagDef_Check. 그리고 공구 피치 RyDeg &gt; 90.01 이면 거절, &lt; 89.99 이면 예전 경로 규칙
            /// (PlayXyz·CalcInv_Tool — 기운 공구의 손목 갈래는 시드가 정한다): 정의의 수직 관절 합이 90 에서 0.5° 안 (OMX) 이면 통과 + Items 에 ★ 한 줄, 아니면 거절. 예외 없음</summary>
            public CSagDefCheck SagDef_CheckSurvey(CSagSurveyCfg cfg)
            {
                try
                {
                    if (cfg == null) cfg = new CSagSurveyCfg();
                    var c = new CSagArmCfg { FuncNumber = cfg.FuncNumber, PosIDs = cfg.PosIDs == null ? null : (int[])cfg.PosIDs.Clone(), WristID = cfg.WristID,
                                             HoldIDs = new int[0], TcpLenMm = float.NaN, DownSumDeg = cfg.DownSumDeg, DefName = string.IsNullOrEmpty(cfg.Robot) ? "" : cfg.Robot };
                    CSagDefCheck chk = SagDef_Check(c);
                    if (!chk.Ok) return chk;
                    string why;
                    bool bWarn;
                    if (!SagDef_LegacyRy(cfg.RyDeg, float.IsNaN(cfg.DownSumDeg) ? chk.DownSumMeasDeg : cfg.DownSumDeg, out why, out bWarn))
                    {
                        CSagDefCheck r = chk.Copy();
                        r.Ok = false; r.Kind = ESagDefFail.Def; r.Why = why; r.Items.Add("Ry ★" + why); r.Summary = "★로봇 정의 확인 안 됨 — " + why;
                        return r;
                    }
                    if (bWarn) { CSagDefCheck r = chk.Copy(); r.Items.Add("Ry ★" + why); return r; }
                    return chk;
                }
                catch (Exception ex) { var r = new CSagDefCheck { Kind = ESagDefFail.Def, Why = "★오류: " + SagRun_Msg(ex) }; return r; }
            }
            // 예전 경로(PlayXyz·CalcInv_Tool — ⑨, 바닥 탐침, SumIk=false 누름)의 공구 피치 규칙 — Ry > 90.01 거절, Ry < 89.99 면 수직 관절 합 d 가 90 에서 0.5° 안이면
            //   통과 + 경고 (bWarn — 기운 공구의 손목 갈래는 시드가 정한다), 아니면 거절 (그 경로는 이 정의의 기운 자세를 보장하지 못한다). Ry 90 은 V7 이 같은 자세를 보장
            private static bool SagDef_LegacyRy(float fRy, float d, out string why, out bool bWarn)
            {
                why = ""; bWarn = false;
                var ci = CultureInfo.InvariantCulture;
                if (float.IsNaN(fRy) || fRy > 90.01f) { why = string.Format(ci, "공구 피치 Ry {0:0.#} 는 못 감 (Ry ≤ 90)", fRy); return false; }
                if (fRy >= 89.99f) return true;
                if (!float.IsNaN(d) && Math.Abs(d - 90f) <= SAGDEF_TOL_DEG)
                { why = string.Format(ci, "예전 경로 — 기운 공구(Ry {0:0.#})의 손목 갈래는 시드가 정함", fRy); bWarn = true; return true; }
                why = string.Format(ci, "예전 경로는 이 정의(수직 관절 합 {0:0.##})의 기운 자세(Ry {1:0.#})를 보장하지 못함 — 관절 합 경로(SumIk)로", d, fRy);
                return false;
            }
            // 바닥 누름·탐침 설정 → 정의 (func·posids·wrist·downsum, 토크만 관절 없음, 공구 끝 거리 확인 안 함)
            private static CSagArmCfg SagDef_FromProbe(CFloorProbeCfg p)
            {
                return new CSagArmCfg { FuncNumber = p.FuncNumber, PosIDs = p.PosIDs == null ? null : (int[])p.PosIDs.Clone(), WristID = p.WristID,
                                        HoldIDs = new int[0], TcpLenMm = float.NaN, DownSumDeg = p.DownSumDeg };
            }
            // 바닥 누름·탐침 입구 확인 — 정의 확인 (내장 OMX 대비책 없음) + 예전 경로(bLegacy)면 공구 피치 규칙. 실패면 why (부른 쪽이 "★로봇 정의: " + why 로 거절)
            private bool SagDef_ProbeGate(CFloorProbeCfg p, bool bLegacy, CSim2Real.DLog L, out string why)
            {
                why = "";
                if (p == null) { why = "설정 없음"; return false; }
                CSagArmCfg c = SagDef_FromProbe(p);
                if (!SagDef_Gate(c, false, L, out why)) return false;
                if (!bLegacy) { why = ""; return true; }
                bool bWarn;
                if (!SagDef_LegacyRy(p.RyDeg, p.DownSumDeg, out why, out bWarn)) return false;
                if (bWarn && L != null) L("★" + why);
                why = "";
                return true;
            }

            // 작업·계획 입구용 — 정의 확인 (캐시). bAllowBuiltIn 이 참이고 3D 에 수식이 하나도 없으면 (묶음 = 내장 OMX) 통과하고 why = ★ 알림 (L 에도 — 로봇을 움직이지 않는
            //   학습·계획만 이것을 쓴다). 실패면 false + why (확인 문구). 통과면 why = "" (내장 OMX 알림만 예외)
            private bool SagDef_Gate(CSagArmCfg c, bool bAllowBuiltIn, CSim2Real.DLog L, out string why)
            {
                why = "";
                try
                {
                    if (bAllowBuiltIn)
                    {
                        CSagRunKin st = SagRun_KinState();
                        if (st != null && st.BuiltIn && SagDef_Formulas().Length == 0)
                        {
                            why = "★3D 에 수식이 없어 내장 OMX 기구학으로 계산 (실물 작업은 하지 않음)";
                            if (L != null) L(why);
                            return true;
                        }
                    }
                    CSagDefCheck chk = SagDef_Check(c);
                    if (chk.Ok) return true;
                    why = chk.Why;
                    return false;
                }
                catch (Exception ex) { why = "★오류: " + SagRun_Msg(ex); return false; }
            }

            // ────────────────────────────────────────────────────────────
            // 3D 표시 — 공구 끝·공구축·꺾는 손목
            // ────────────────────────────────────────────────────────────

            private volatile CSagArmCfg m_ToolMarkCfg = null;        // 표시할 정의 (사본 — null 이면 표시 안 함)
            private volatile bool m_bToolMarkShow = false;
            private readonly object m_oToolMark = new object();      // 표시 칸 두 개를 함께 바꾼다 (UI 스레드 ToolMark_Set · 작업 스레드 ToolMark_Hold/Release)

            /// <summary>3D 표시 켬·끔 (UI 스레드에서) — c 의 func·wrist·posids·downsum 으로 공구 끝(빨강 공)·공구축(손목 원점 → 공구 끝, 초록 = 수직 / 파랑 = 기울어짐)
            /// ·손목 원점(주황 공)·수직 기준(회색 선)을 그린다. 손목 부품(GroupA = GroupB = 손목 ID)은 주황 (Second 색 — 이 표시만 쓴다).
            /// 그리기는 다음 그리기부터. 정의를 바꾸면 다시 부를 것</summary>
            public void ToolMark_Set(CSagArmCfg c, bool bShow)
            {
                try
                {
                    lock (m_oToolMark)
                    {
                        m_ToolMarkCfg = c == null ? null : c.Clone();
                        m_bToolMarkShow = bShow && c != null;
                    }
                    ClearColor_Second();
                    if (bShow && c != null && c.WristID > 0) AddColor_Second(c.WristID, c.WristID, 0, Color.Orange);
                }
                catch { }
            }
            // 작업 스레드에서 3D 표시를 잠깐 켠다 (SagPrac_EyeJob 이 묻는 동안) — 그리기 칸만 바꾼다 (손목 부품의 주황 Second 색은 UI 스레드 ToolMark_Set 의 몫).
            //   반환 = 되돌릴 때 쓸 표 (ToolMark_Release). c 가 null 이면 아무것도 하지 않는다
            private object ToolMark_Hold(CSagArmCfg c)
            {
                lock (m_oToolMark)
                {
                    var h = new CToolMarkHold { Prev = m_ToolMarkCfg, PrevShow = m_bToolMarkShow, Mine = c == null ? null : c.Clone() };
                    if (h.Mine != null) { m_ToolMarkCfg = h.Mine; m_bToolMarkShow = true; }
                    return h;
                }
            }
            // ToolMark_Hold 되돌리기 — 그사이 UI 가 ToolMark_Set 으로 바꿨으면 (앱이 묻는 동안 켜고 끝나면 탭에 맞춘다) 그대로 둔다
            private void ToolMark_Release(object o)
            {
                var h = o as CToolMarkHold;
                if (h == null || h.Mine == null) return;
                lock (m_oToolMark)
                {
                    if (!ReferenceEquals(m_ToolMarkCfg, h.Mine)) return;
                    m_ToolMarkCfg = h.Prev; m_bToolMarkShow = h.PrevShow;
                }
            }
            private sealed class CToolMarkHold { public CSagArmCfg Prev, Mine; public bool PrevShow; }
            /// <summary>3D 표시가 켜져 있나</summary>
            public bool ToolMark_Show { get { return m_bToolMarkShow && m_ToolMarkCfg != null; } }

            /// <summary>3D 표시 계산만 (GL 없음 — 헤드리스 시험·표시 글용). afData = 관절값 (null 이면 지금 3D 의 GetData), 정의 = ToolMark_Set 의 것 (없으면 기본값).
            /// wrist·tcp = 손목 원점·공구 끝 (3D 의 수식 — 화면과 같은 자세), tiltDeg = 공구축이 −Z 에서 기운 각, sumDeg = 관절 합 posids[1]+posids[2]+wrist.
            /// 반환 = 공구선이 있다 (손목이 공구 끝 체인 안 — 아니면 tcp 만 맞고 wrist = tcp, tilt NaN). 3D 관절값을 바꾸지 않는다. 예외 없음</summary>
            public bool ToolMark_Geometry(float[] afData, out float[] wrist, out float[] tcp, out float tiltDeg, out float sumDeg)
            {
                wrist = new float[3]; tcp = new float[3]; tiltDeg = float.NaN; sumDeg = float.NaN;
                try
                {
                    CSagArmCfg c = m_ToolMarkCfg ?? new CSagArmCfg();
                    var ad = new double[256];
                    for (int i = 0; i < ad.Length; i++) ad[i] = afData != null ? (i < afData.Length ? afData[i] : 0) : GetData(i);
                    float tx, ty, tz, wx, wy, wz;
                    SagFc_Fk(c.FuncNumber, ad, -1, out tx, out ty, out tz);
                    tcp = new float[] { tx, ty, tz };
                    if (c.PosIDs != null && c.PosIDs.Length == 3) sumDeg = (float)(ad[c.PosIDs[1]] + ad[c.PosIDs[2]] + ad[c.WristID]);
                    bool bChain = false;
                    CDhParamAll dh = (m_CHeader != null && m_CHeader.pDhParamAll != null && c.FuncNumber >= 0 && c.FuncNumber < m_CHeader.pDhParamAll.Length) ? m_CHeader.pDhParamAll[c.FuncNumber] : null;
                    if (dh != null && Array.IndexOf(SagDef_ChainOf(dh), c.WristID) >= 0) bChain = true;
                    if (!bChain) { wrist = (float[])tcp.Clone(); return false; }
                    SagFc_Fk(c.FuncNumber, ad, c.WristID, out wx, out wy, out wz);
                    wrist = new float[] { wx, wy, wz };
                    double vx = tx - wx, vy = ty - wy, vz = tz - wz, L = Math.Sqrt(vx * vx + vy * vy + vz * vz);
                    if (L < 0.01) { wrist = (float[])tcp.Clone(); return false; }
                    tiltDeg = (float)(Math.Acos(Math.Max(-1.0, Math.Min(1.0, -vz / L))) * 180.0 / Math.PI);
                    return true;
                }
                catch { return false; }
            }

            // 그리기 — 쌍둥이 그리기 훅(SagTwin_DrawGhost) 맨 앞. 픽 프레임·관절값 없음·표시 꺼짐이면 그리지 않는다.
            //   깊이 검사를 끄고 (공구 끝이 그리퍼 메시 안에 있다) 손목 원점 주황 공 r6, 공구 끝 빨강 공 r4, 공구선(초록 = 수직 1.5° 안, 파랑 = 기울어짐),
            //   손목 원점에서 −Z 로 공구 길이만큼 회색 선. 손목이 체인 밖이면 공구 끝 공만
            private void ToolMark_Draw(float[] afData)
            {
                try
                {
                    if (!ToolMark_Show || m_bPickMode || afData == null) return;
                    float[] w, t; float tilt, sum;
                    bool bLine = ToolMark_Geometry(afData, out w, out t, out tilt, out sum);
                    InitPosAngle();
                    Gl.glPushAttrib(Gl.GL_ENABLE_BIT | Gl.GL_LINE_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                    try
                    {
                        Gl.glDisable(Gl.GL_DEPTH_TEST);
                        OjwBall_Outside(true, Color.Red, 1.0f, 4f, 12, 0, 0, 0, t[0], t[1], t[2]);
                        if (bLine)
                        {
                            OjwBall_Outside(true, Color.Orange, 1.0f, 6f, 12, 0, 0, 0, w[0], w[1], w[2]);
                            double L = Math.Sqrt((double)(t[0] - w[0]) * (t[0] - w[0]) + (double)(t[1] - w[1]) * (t[1] - w[1]) + (double)(t[2] - w[2]) * (t[2] - w[2]));
                            Gl.glDisable(Gl.GL_LIGHTING);
                            Gl.glLineWidth(3.0f);
                            OjwLine(tilt <= TOOLMARK_VERT_DEG ? Color.LimeGreen : Color.SteelBlue, 1.0f, w[0], w[1], w[2], t[0], t[1], t[2], 0, 0, 0, 0, 0, 0);
                            Gl.glLineWidth(1.5f);
                            OjwLine(Color.Gray, 1.0f, w[0], w[1], w[2], w[0], w[1], (float)(w[2] - L), 0, 0, 0, 0, 0, 0);
                        }
                    }
                    finally { Gl.glPopAttrib(); }
                }
                catch { }                                                        // 표시는 그리기를 막지 않는다
            }
        }
    }
}
