//=====================================================================
// sim2real — 실측을 모델에 싣는 절차 (라이브러리)
//
// 시뮬에서는 되는데 실물은 넘어지는 이유는 대개 모델이 실물보다 관대하기 때문이다.
// 그 격차를 좁히는 절차를 세 단계로 고정한다:
//
//   ① 관절 방향 (CSignCalib)  — 매달림. 모터각 <-> 모델각 부호를 실측한다.
//   ② 관절 리미트 (CMechLimit) — 매달림. 실제 가동범위를 잰다.
//   ③ 반영 (CApply)           — 부호를 적용해 URDF 공간 $jointrange 로 기입한다.
//
// ★순서가 중요하다. 리미트는 모터각으로 나오고, URDF 로 옮기려면 부호가 있어야 한다.
//   sign < 0 인 축은 lo/hi 가 뒤바뀐다. 2026-07-27 에 이 순서를 안 지켜
//   5축(T2/T4/T5/T7/T17)이 부호가 뒤집힌 채 URDF 에 실렸다.
//
// UI 는 콜백으로 분리한다 — 이 파일은 폼을 모른다.
//=====================================================================
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
        public class CSim2Real
        {
            //=========================================================
            // 공통 설정
            //=========================================================
            public class CCfg
            {
                public int nPort = 8;
                public int nBaud = 1000000;
                public int[] anIDs = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
                public int nIdOffset = 0;         // T번호 = 모터ID - offset
                public int nPwm = 400;            // Goal PWM 제한 (885 만점)
                public float fMinVolt = 10.5f;
            }

            /// <summary>진행 로그 / 사용자 확인 / 3D 표시를 바깥에서 받는다.</summary>
            public delegate void DLog(string strMsg);
            public delegate void DShow3d(int nT, float fDeg);
            public delegate bool DIsStopped();

            //=========================================================
            // 사용 설명서 — MakeUrdf "설명서" 버튼이 이 텍스트를 띄운다
            //=========================================================
            public static string GetGuideText()
            {
                return
"■ sim2real 측정 순서 — 기초 운용법\r\n" +
"\r\n" +
"준비: 로봇 USB 연결 + 전원 ON. 버튼을 누르면 확인창(자세 안내)이\r\n" +
"뜨고, [예]를 누르면 로그 패널이 열리며 자동 진행됩니다.\r\n" +
"\r\n" +
"순서는 로봇 자세 기준 3단계입니다.\r\n" +
"\r\n" +
"─── 1단계: 매달림 (발이 땅에서 떨어지게) ───────────────\r\n" +
"\r\n" +
" ① 관절방향   시뮬 관절 ↔ 실물 모터의 부호(방향) 자동 대응\r\n" +
" ② 관절리미트 각 관절을 천천히 끝까지 밀어 기구 한계각 실측\r\n" +
" ⑤ 서보동정   모터 성능표 — 최대속도·마찰·EEPROM (약 15분)\r\n" +
"\r\n" +
"─── 2단계: 기립 (두 발로 세우고, 잡아줄 준비) ──────────\r\n" +
"\r\n" +
" ⑥ 전도경계   4방향으로 기울여 넘어지는 각도 실측 (약 10분)\r\n" +
"              ★로그 끝의 $footcontact 제안줄을 복사해서\r\n" +
"                EDH 텍스트의 발 링크 뒤에 붙여넣기 (수동 1회)\r\n" +
" ④ 기립처짐   자중으로 처지는 양 → 강성(kp) 확인\r\n" +
" ⑦ 전원측정   무릎 펌프 25회로 배터리 전압 강하 (약 2분)\r\n" +
" ⑧ 부하속도   다리 6관절 큰 스텝(20/40도)의 유효 슬루 —\r\n" +
"              체중 부하가 걸린 실속도 + 스텝 순간 전압.\r\n" +
"              ★전도 위험: 사람 보조 하에. 넘어지면 다시\r\n" +
"              세우면 자동 재개. 모드 3종: 기립(반체중) /\r\n" +
"              한발 지지(전체중) / 매달림(무부하 대조)\r\n" +
"\r\n" +
"─── 3단계: 내보내기 (로봇 없어도 됨) ───────────────────\r\n" +
"\r\n" +
" 1. ★Fixed Base 체크 해제 확인\r\n" +
"    (켜져 있으면 URDF 가 땅에 용접된 채 나갑니다)\r\n" +
" 2. ③ sim2real 버튼 → USD Export\r\n" +
"\r\n" +
"완료 = urdf\\sim2real\\ 폴더에 json 5개 + URDF/MJCF/USD.\r\n" +
"이 폴더를 통째로 학습(시뮬레이션) 쪽에 넘기면 인수인계 끝.\r\n" +
"\r\n" +
"─── 공통 조작 ──────────────────────────────────────────\r\n" +
"\r\n" +
" · 진행 중 빨간 [중단] = 즉시 정지 / 완료되면 초록 [닫기]\r\n" +
" · \"측정 완료 — 이제 전원을 꺼도 됩니다\" = 정상 종료 신호\r\n" +
" · 측정값이 이미 있는 축은 건너뛰고 기존 값을 보존하므로,\r\n" +
"   실패한 단계만 다시 돌려도 됩니다.\r\n" +
" · 산출물: urdf\\sim2real\\  (joint_map / mech_limits /\r\n" +
"   servo_ident / tiptest_pitch / power_profile /\r\n" +
"   load_vmax_stand .json)\r\n";
            }

            //=========================================================
            // 저수준 — 읽기
            //=========================================================
            public static float Deg(int nTick) { return (nTick - 2048) * 0.087890625f; }

            /// <summary>단축 읽기. 연속 2회가 tol 이내면 확정, 아니면 마지막 값.
            /// ★완전 일치를 요구하지 말 것 — 정지 중에도 엔코더가 1~2틱 흔들려서
            ///   매번 "읽기 실패"가 되어 도구가 아예 안 돈다(실제로 두 번 겪었다).</summary>
            public static int Read(Ojw.CProtocol com, int id, int addr, int len, int tol)
            {
                int prev = int.MinValue, v = int.MinValue;
                for (int t = 0; t < 4; t++)
                {
                    com.SyncRead_With_Address(addr, len, id); Thread.Sleep(10);
                    v = (len == 4) ? com.GetMap_Int(id, addr) : (short)com.GetMap_Short(id, addr);
                    if (prev != int.MinValue && Math.Abs(v - prev) <= tol) return v;
                    prev = v;
                }
                return v;
            }

            /// <summary>전축 일괄 읽기(SyncRead 한 번). 실패하면 null 과 사유.</summary>
            public static Dictionary<int, float> ReadAll(Ojw.CProtocol com, int[] anIDs, out string strWhy)
            {
                int[] anTick = ReadAllTicks(com, anIDs, out strWhy);
                return (anTick != null) ? ToDeg(anIDs, anTick) : null;
            }

            /// <summary>ReadAll 의 본체 — 틱(서보 위치 값) 그대로, anIDs 순. 연속 두 번이 8틱 안이면 확정(최대 6회),
            /// 정착은 안 됐어도 값이 정상이면 마지막 정상 값. 전부 실패하면 null 과 사유.
            /// 틱 0·-1·범위 밖은 무응답으로 본다 (모터 세팅의 리밋을 틱 [1, 4094] 안으로 두는 이유)</summary>
            public static int[] ReadAllTicks(Ojw.CProtocol com, int[] anIDs, out string strWhy)
            {
                const int TOL = 8;                     // 0.7도 — 정지 지터 허용
                strWhy = "";
                int[] anPrev = null, anGood = null;
                for (int t = 0; t < 6; t++)
                {
                    com.SyncRead_With_Address(132, 4, anIDs); Thread.Sleep(18);
                    int[] anCur = new int[anIDs.Length];
                    var lstBad = new List<int>();
                    for (int i = 0; i < anIDs.Length; i++)
                    {
                        anCur[i] = com.GetMap_Int(anIDs[i], 132);
                        if (anCur[i] == 0 || anCur[i] == -1 || anCur[i] < -8192 || anCur[i] > 12288)
                            lstBad.Add(anIDs[i]);
                    }
                    if (lstBad.Count > 0)
                    {
                        strWhy = "무응답/이상값 ID: " + string.Join(",", lstBad.ConvertAll(x => x.ToString()).ToArray());
                        anPrev = null;
                        continue;
                    }
                    anGood = anCur;
                    if (anPrev != null)
                    {
                        int nWorst = 0, nWorstId = 0;
                        for (int i = 0; i < anCur.Length; i++)
                        {
                            int d2 = Math.Abs(anCur[i] - anPrev[i]);
                            if (d2 > nWorst) { nWorst = d2; nWorstId = anIDs[i]; }
                        }
                        if (nWorst <= TOL) return anCur;
                        strWhy = string.Format("아직 정착 안 됨 (T{0} 이 {1}틱 = {2:F1}도 변동)",
                                               nWorstId, nWorst, nWorst * 0.0879f);
                    }
                    anPrev = anCur;
                }
                // 정착은 안 됐어도 값 자체가 정상이면 쓴다 (움직이는 중일 수 있다)
                return anGood;
            }

            private static Dictionary<int, float> ToDeg(int[] anIDs, int[] anTick)
            {
                var d = new Dictionary<int, float>();
                for (int i = 0; i < anIDs.Length; i++) d[anIDs[i]] = Deg(anTick[i]);
                return d;
            }

            public static void SetPwm(Ojw.CProtocol com, int[] anIDs, int nPwm)
            {
                foreach (int id in anIDs)
                { com.Send(id, 3, 100, (byte)(nPwm & 0xFF), (byte)((nPwm >> 8) & 0xFF)); Thread.Sleep(4); }
            }

            /// <summary>열기 + 토크 ON + 자세 검증. 실패하면 null (아무것도 명령하지 않는다).</summary>
            public static Ojw.CProtocol Open(CCfg cfg, DLog log, out Dictionary<int, float> dicPose)
            {
                dicPose = null;
                var com = new Ojw.CProtocol();
                if (!com.Open(cfg.nPort, cfg.nBaud))
                { if (log != null) log(string.Format("COM{0} 열기 실패", cfg.nPort)); return null; }
                foreach (int id in cfg.anIDs) { com.SetParam(id); com.SetParam_Protocol(id, 2); }

                // ★0V 를 "저전압"으로 보고하면 안 된다 — 0V 는 물리적으로 불가능하고,
                //   실제로는 **그 ID 가 응답하지 않는다**는 뜻이다(오타 ID/보레이트 불일치/포트 점유).
                //   2026-08-28: 존재하지 않는 ID 24 를 물었더니 "전압 낮음 — 충전 후 재시도" 가 떴다.
                //   멀쩡한 배터리(11.9V)를 두고 충전하러 갈 뻔한 메시지였다. 재시도로 지터도 함께 흡수.
                int nRawVolt = Read(com, cfg.anIDs[0], 144, 2, 3);
                float v = (nRawVolt & 0xFFFF) * 0.1f;
                if (nRawVolt <= 0 || v < 1.0f)
                {
                    // 0V 는 물리적으로 불가능 — 전원이 아니라 통신이 실패한 것이다
                    if (log != null)
                        log(string.Format("전압 읽기 실패 (ID {0} 무응답) — 배터리가 아니라 통신 문제입니다. "
                                        + "포트를 다른 프로그램이 잡고 있는지, ID/보레이트가 맞는지 확인하십시오.",
                                          cfg.anIDs[0]));
                    com.Close(); return null;
                }
                if (log != null) log(string.Format("버스 전압 {0:F1} V", v));
                if (v < cfg.fMinVolt)
                { if (log != null) log("전압 낮음 — 충전 후 재시도 (결과가 오염된다)"); com.Close(); return null; }

                foreach (int id in cfg.anIDs) { com.Send(id, 3, 64, (byte)1); Thread.Sleep(5); }

                string strWhy;
                dicPose = ReadAll(com, cfg.anIDs, out strWhy);
                if (dicPose == null)
                {
                    if (log != null) log("위치 읽기 실패 — 아무것도 명령하지 않고 중단. " + strWhy);
                    com.Close(); return null;
                }
                float fMax = 0;
                foreach (var kv in dicPose) fMax = Math.Max(fMax, Math.Abs(kv.Value));
                if (fMax > 175f)
                {
                    // 복귀 명령 자체가 사고를 일으킨다 — 아무것도 하지 않고 나간다
                    if (log != null) log(string.Format("비정상 자세({0:F0}도) — 명령 없이 중단. Recover 로 점검하십시오", fMax));
                    com.Close(); return null;
                }
                SetPwm(com, cfg.anIDs, cfg.nPwm);
                return com;
            }

            /// <summary>전축 0도 정렬 (현재각을 먼저 걸어 점프 방지 후 저속 램프).</summary>
            public static bool HomeAll(Ojw.CProtocol com, CCfg cfg, Dictionary<int, float> dicCur, DLog log)
            {
                float fMax = 0;
                foreach (var kv in dicCur) fMax = Math.Max(fMax, Math.Abs(kv.Value));
                com.Command_Clear();
                foreach (var kv in dicCur) com.Command_Set(kv.Key, kv.Value);
                com.Move_NoWait(200, 0); Thread.Sleep(300);
                int nMs = Math.Max(1500, (int)(fMax / 20.0f * 1000));
                com.Command_Clear();
                foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                com.Move_NoWait(nMs, 0); Thread.Sleep(nMs + 400);

                string strWhy;
                var chk = ReadAll(com, cfg.anIDs, out strWhy);
                float fOff = 0;
                if (chk != null) foreach (var kv in chk) fOff = Math.Max(fOff, Math.Abs(kv.Value));
                if (log != null) log(string.Format("정렬 후 최대 |각| {0:F1}도", fOff));
                return (chk != null && fOff <= 4.0f);
            }

            //=========================================================
            // ① 관절 방향 — joint_map.json
            //=========================================================
            public enum ESignAns { Same, Opposite, Again, Skip, Abort }

            /// <summary>한 관절을 움직인 뒤 사용자에게 방향을 묻는다.</summary>
            public delegate ESignAns DAskDir(int nT, int nId, float fSimDeg, int nSign,
                                             bool bKnown, int nDone, int nTotal);

            public class CSignCfg : CCfg
            {
                public float fSweep = 15.0f;      // 확인용 스윕 각
                public int nMoveMs = 450;
                public bool bHome = false;        // 방향만 볼 거면 0도 정렬은 불필요
                public Dictionary<int, float> Sweep = new Dictionary<int, float>();
            }

            /// <summary>부호 검사 본체. 저장된 값을 **적용해서** 움직이므로 재실행이 검증이 된다.</summary>
            public static bool SignCalib(CSignCfg cfg, string strDir,
                                         DAskDir ask, DShow3d show3d, DLog log,
                                         Dictionary<int, int> signs, out bool bAborted, out int nConfirmed)
            {
                bAborted = false; nConfirmed = 0;
                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                var dicCur0 = new Dictionary<int, float>(dicCur);

                try
                {
                    // ★0도 정렬은 기본으로 하지 않는다. 이건 방향 검사지 정밀 검사가 아니다.
                    //   지금 자세가 어디든 거기서 스윕하면 방향은 그대로 보이고,
                    //   전축 정렬은 시간도 걸리고 매달림에서 중력과 싸우다 실패해 검사 자체를 막는다.
                    if (cfg.bHome)
                    {
                        if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("정렬 실패 — 중단"); return false; }
                        string w2; dicCur = ReadAll(com, cfg.anIDs, out w2);
                        if (dicCur == null) { if (log != null) log("정렬 후 읽기 실패 — 중단"); return false; }
                    }
                    if (show3d != null)
                        foreach (int id in cfg.anIDs) show3d(id - cfg.nIdOffset, 0f);

                    int nIdx = 0;
                    foreach (int id in cfg.anIDs)
                    {
                        nIdx++;
                        int nT = id - cfg.nIdOffset;
                        float fSw = cfg.Sweep.ContainsKey(id) ? cfg.Sweep[id] : cfg.fSweep;
                        float fBase = dicCur.ContainsKey(id) ? dicCur[id] : 0f;
                        bool bKnown = signs.ContainsKey(id);
                        int nSign = bKnown ? signs[id] : +1;
                        // 이미 한쪽으로 많이 가 있으면 반대로 흔든다 (한계 회피). 3D 도 같은 부호로 간다.
                        float fDir = (Math.Abs(fBase + nSign * fSw) > 110f) ? -1f : +1f;

                        ESignAns ans;
                        while (true)
                        {
                            float fSim = fDir * fSw;
                            if (show3d != null) show3d(nT, fSim);
                            Thread.Sleep(120);
                            com.Command_Clear();
                            com.Command_Set(id, fBase + nSign * fSim);     // real = sign * sim
                            com.Move_NoWait(cfg.nMoveMs, 0);
                            Thread.Sleep(cfg.nMoveMs + 120);

                            ans = ask(nT, id, fSim, nSign, bKnown, nIdx, cfg.anIDs.Length);

                            if (show3d != null) show3d(nT, 0f);
                            com.Command_Clear();
                            com.Command_Set(id, fBase);
                            com.Move_NoWait(cfg.nMoveMs, 0);
                            Thread.Sleep(cfg.nMoveMs + 120);

                            if (ans == ESignAns.Opposite) { nSign = -nSign; bKnown = true; continue; }
                            if (ans != ESignAns.Again) break;
                        }
                        if (ans == ESignAns.Abort) { bAborted = true; break; }
                        if (ans == ESignAns.Skip) continue;
                        signs[id] = nSign;
                        nConfirmed++;
                        SaveJointMap(Path.Combine(strDir, "joint_map.partial.json"), cfg.anIDs, cfg.nIdOffset, signs);
                    }
                }
                finally
                {
                    try
                    {
                        com.Command_Clear();
                        foreach (int id in cfg.anIDs)
                            com.Command_Set(id, dicCur0.ContainsKey(id) ? dicCur0[id] : 0f);
                        com.Move_NoWait(1500, 0); Thread.Sleep(1700);
                        SetPwm(com, cfg.anIDs, 885);
                        com.Close();
                    }
                    catch { }
                }
                return true;
            }

            //=========================================================
            // ② 관절 리미트 — mech_limits.json
            //=========================================================
            public class CMechCfg : CCfg
            {
                public float fStep = 1.0f;
                public int nStepMs = 80;
                public float fErrThr = 2.5f;
                public int nLoadThr = 350;
                public float fCap = 125.0f;
                // ★중력 보상 — 근위 관절은 아래 관절을 접어 중력을 빼고 재야 한다 (R10).
                //   링크 길이가 비슷하면 아래관절 = 시험관절 x 2 가 최적(이등변삼각형).
                //   부호는 관절별 실측이 정본 — 좌우 무릎이 서로 반대다.
                public Dictionary<int, int> FoldJoint = new Dictionary<int, int>() { { 6, 15 }, { 8, 16 } };
                public Dictionary<int, float> FoldRatio = new Dictionary<int, float>() { { 6, -2.058f }, { 8, +2.058f } };
                public Dictionary<int, int> Mirror = new Dictionary<int, int>() {
                    {1,3},{3,1},{2,4},{4,2},{5,7},{7,5},{6,8},{8,6},
                    {9,11},{11,9},{10,12},{12,10},{13,14},{14,13},{15,16},{16,15} };
            }

            public class CLimit { public float lo, hi; public string q; }

            /// <summary>하중 곡선 모양 — 벽(스톱)인가 완만한 상승(중력)인가.</summary>
            private static string Classify(List<float[]> h)
            {
                if (h.Count < 6) return "unknown";
                int cut = Math.Max(1, h.Count - 3);
                float bMax = 0, first = 0, bAvg = 0;
                for (int i = 0; i < cut; i++) { bAvg += h[i][3]; if (h[i][3] > bMax) bMax = h[i][3]; }
                bAvg /= cut;
                int k = Math.Min(5, cut);
                for (int i = 0; i < k; i++) first += h[i][3];
                first /= k;
                float end = h[h.Count - 1][3];
                if (bMax < 150 && end > 2.0f * Math.Max(60f, bMax)) return "wall";
                if (bAvg - first > 60) return "gravity";
                return "mixed";
            }

            private static float Sweep(Ojw.CProtocol com, CMechCfg cfg, int id, float fSign,
                                       bool bFold, int nMirror, List<float[]> hist,
                                       DLog log, DIsStopped stop)
            {
                int nFold = 0; float fRatio = 0;
                if (bFold && cfg.FoldJoint.ContainsKey(id))
                { nFold = cfg.FoldJoint[id]; fRatio = cfg.FoldRatio[id]; }

                float g = 0f, fLast = 0f;
                bool bPartial = false;
                while (Math.Abs(g) < cfg.fCap)
                {
                    if (stop != null && stop()) break;
                    g += fSign * cfg.fStep;
                    float k = 0f;
                    com.Command_Clear();
                    com.Command_Set(id, g);
                    if (nFold > 0)
                    {
                        k = fRatio * g;
                        // 아래 관절 가동범위를 넘으면 **중단하지 말고 클램프**한다.
                        // 완전보상은 ±56도까지지만, 그 너머도 최대로 접어두면 잔여 중력이 작다.
                        float kLim = 113.5f;
                        if (k < -kLim || k > kLim)
                        {
                            k = (k < 0) ? -kLim : kLim;
                            if (!bPartial && log != null)
                            { log(string.Format("    (부분보상 시작 — 아래관절 {0:F1}도 고정)", k)); bPartial = true; }
                        }
                        com.Command_Set(nFold, k);
                    }
                    if (nMirror > 0) com.Command_Set(nMirror, g);
                    com.Move_NoWait(cfg.nStepMs, 0);
                    Thread.Sleep(cfg.nStepMs);

                    int p = Read(com, id, 132, 4, 6);
                    int l = Read(com, id, 126, 2, 40);
                    float pos = Deg(p);
                    if (p == 0 || p == -1 || Math.Abs(pos - fLast) > 30f)
                    { if (log != null) log(string.Format("    비정상 읽기(틱 {0}) — 명령 없이 중단", p)); return fLast; }

                    float err = Math.Abs(g - pos);
                    int load = Math.Abs(l);
                    hist.Add(new float[] { g, pos, err, load });
                    if (err > cfg.fErrThr || load > cfg.nLoadThr)
                    {
                        if (log != null)
                            log(string.Format("    정지 {0,6:F1}도 (오차 {1:F1}, 하중 {2}){3}",
                                              g, err, load, bFold ? " [중력보상]" : ""));
                        com.Command_Clear(); com.Command_Set(id, fLast);
                        if (nFold > 0) com.Command_Set(nFold, fRatio * fLast);
                        if (nMirror > 0) com.Command_Set(nMirror, fLast);
                        com.Move_NoWait(700, 0); Thread.Sleep(900);
                        return fLast;
                    }
                    fLast = pos;
                }
                com.Command_Clear(); com.Command_Set(id, 0f);
                if (nFold > 0) com.Command_Set(nFold, 0f);
                if (nMirror > 0) com.Command_Set(nMirror, 0f);
                com.Move_NoWait(1800, 0); Thread.Sleep(2100);
                return float.NaN;
            }

            private static void Zero(Ojw.CProtocol com, int id, int nFold)
            {
                com.Command_Clear();
                com.Command_Set(id, 0f);
                if (nFold > 0) com.Command_Set(nFold, 0f);
                com.Move_NoWait(1500, 0); Thread.Sleep(1700);
            }

            public static bool MechLimit(CMechCfg cfg, string strDir, DLog log, DIsStopped stop,
                                         Dictionary<int, CLimit> outLimits)
            {
                // ★기존 결과를 먼저 불러온다. 한두 축만 다시 재는 일이 흔한데,
                //   그때 나머지가 파일에서 지워지면 안 된다 (T17 재측정에서 실제로 16축이 날아갔다).
                int nKept = 0;
                foreach (var kvOld in LoadMechLimits(Path.Combine(strDir, "mech_limits.json")))
                    if (!outLimits.ContainsKey(kvOld.Key)) { outLimits[kvOld.Key] = kvOld.Value; nKept++; }
                BackupMechLimits(Path.Combine(strDir, "mech_limits.json"), "before_drive", log);

                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    if (nKept > 0 && log != null)
                        log(string.Format("기존 결과 {0}축 불러옴 — 이번에 재는 축만 갱신됩니다", nKept));
                    if (log != null) log(string.Format("PWM 제한 {0}/885 ({1}%)", cfg.nPwm, cfg.nPwm * 100 / 885));
                    if (log != null) log("전축 0도 정렬...");
                    // 다른 관절이 걸쳐 있으면 그 간섭을 한계로 오인한다
                    if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("정렬 실패 — 중단"); return false; }

                    // ★진행 상황을 항상 눈에 보이게 한다.
                    //   10분짜리 측정인데 관절이 1도씩 천천히 도니 멈춘 것처럼 보인다.
                    //   실제로 사용자가 "끝난 줄 알고" 전원을 끈 적이 있다 (2026-07-27).
                    int nIdx = 0;
                    foreach (int id in cfg.anIDs)
                    {
                        nIdx++;
                        if (stop != null && stop()) { if (log != null) log("사용자 중단"); break; }
                        int nT = id - cfg.nIdOffset;
                        bool bFold = cfg.FoldJoint.ContainsKey(id);
                        int nFold = bFold ? cfg.FoldJoint[id] : 0;
                        if (log != null)
                            log(string.Format("── [{0}/{1}] T{2} {3}  (남은 축 {4}개, 대략 {5}분)",
                                              nIdx, cfg.anIDs.Length, nT, bFold ? "(아래관절 보상)" : "",
                                              cfg.anIDs.Length - nIdx,
                                              Math.Max(1, (int)((cfg.anIDs.Length - nIdx) * 0.6))));

                        var hA = new List<float[]>();
                        float fHi = Sweep(com, cfg, id, +1f, bFold, 0, hA, log, stop);
                        string qA = float.IsNaN(fHi) ? "no_limit" : Classify(hA);
                        Zero(com, id, nFold);

                        var hB = new List<float[]>();
                        float fLo = Sweep(com, cfg, id, -1f, bFold, 0, hB, log, stop);
                        string qB = float.IsNaN(fLo) ? "no_limit" : Classify(hB);
                        Zero(com, id, nFold);

                        string q = (qA == "wall" || qB == "wall") ? "trusted" : "gravity_suspect";
                        // ★중력 지문 — 기구 스톱이면 두 방향 정지각의 중점이 0 근처여야 한다.
                        //   매달린 평형 방향으로 치우쳐 있으면 그건 중력 한계다.
                        if (!float.IsNaN(fHi) && !float.IsNaN(fLo))
                        {
                            float mid = (fHi + fLo) / 2f;
                            if (Math.Abs(mid) > 8f)
                            {
                                q = "gravity_suspect";
                                if (log != null)
                                    log(string.Format("   ★중점 {0:F1}도 — 중력 한계 의심 (스톱이면 0 근처여야 한다)", mid));
                            }
                        }
                        if (bFold) q = "gravity_free";

                        // 자기충돌 재검증 — 좁으면 미러 관절 동반 구동으로 다시
                        if (!bFold && cfg.Mirror.ContainsKey(id) &&
                            !float.IsNaN(fHi) && !float.IsNaN(fLo) && (fHi < 35f || fLo > -35f))
                        {
                            int nMir = cfg.Mirror[id];
                            if (log != null) log(string.Format("   좁음 — T{0} 동반 구동으로 자기충돌 재검증", nMir - cfg.nIdOffset));
                            var h2 = new List<float[]>();
                            float f2 = Sweep(com, cfg, id, +1f, false, nMir, h2, log, stop);
                            com.Command_Clear(); com.Command_Set(id, 0f); com.Command_Set(nMir, 0f);
                            com.Move_NoWait(1500, 0); Thread.Sleep(1700);
                            var h3 = new List<float[]>();
                            float f3 = Sweep(com, cfg, id, -1f, false, nMir, h3, log, stop);
                            com.Command_Clear(); com.Command_Set(id, 0f); com.Command_Set(nMir, 0f);
                            com.Move_NoWait(1500, 0); Thread.Sleep(1700);
                            bool bWider = (!float.IsNaN(f2) && f2 > fHi + 4f) || (!float.IsNaN(f3) && f3 < fLo - 4f);
                            if (bWider)
                            {
                                if (log != null)
                                    log(string.Format("   ★자기충돌 — 협조에서 열림 ({0:F1}~{1:F1} → {2:F1}~{3:F1}). "
                                                    + "limit 이 아니라 충돌형상으로 처리할 값", fLo, fHi, f3, f2));
                                if (!float.IsNaN(f2)) fHi = f2;
                                if (!float.IsNaN(f3)) fLo = f3;
                                q = "self_collision_excluded";
                            }
                        }

                        // ★안전상한(cap)까지 갔으면 "제한 없음"이지 "cap 이 한계"가 아니다.
                        //   그대로 두면 ±125 가 실제 스톱인 것처럼 URDF 에 실린다.
                        if (float.IsNaN(fHi) || float.IsNaN(fLo))
                        {
                            q = "no_limit";
                            if (log != null)
                                log(string.Format("   상한 {0:F0}도 까지 무간섭 — 제한 없음으로 기록 (URDF 에 싣지 않음)", cfg.fCap));
                        }
                        // ★가동범위가 비정상적으로 좁으면 측정 실패로 본다.
                        //   T17(몸통 요)이 0.00~4.83 으로 나온 적이 있다 — 매단 끈이 몸통을 잡고 있었다.
                        //   그 값이 URDF 에 들어가면 사실상 못 도는 관절이 된다.
                        else if (Math.Abs(fHi - fLo) < 10.0f)
                        {
                            q = "measure_failed";
                            if (log != null)
                                log(string.Format("   ★가동범위 {0:F1}도 뿐 — 측정 실패로 표시 (거치·간섭 확인 후 재측정)",
                                                  Math.Abs(fHi - fLo)));
                        }

                        var e = new CLimit();
                        e.lo = float.IsNaN(fLo) ? -cfg.fCap : fLo;
                        e.hi = float.IsNaN(fHi) ? cfg.fCap : fHi;
                        e.q = q;
                        outLimits[id] = e;
                        if (log != null) log(string.Format("   T{0}: {1:F2} ~ {2:F2}  [{3}]", nT, e.lo, e.hi, q));
                        SaveMechLimits(Path.Combine(strDir, "mech_limits.json"), cfg.anIDs, cfg.nIdOffset, cfg.nPwm, outLimits);
                    }
                }
                finally
                {
                    try
                    {
                        com.Command_Clear();
                        foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                        com.Move_NoWait(2500, 0); Thread.Sleep(2800);
                        SetPwm(com, cfg.anIDs, 885);
                        com.Close();
                    }
                    catch { }
                }
                return true;
            }

            //=========================================================
            // ②-b 수동 한계 학습 — 토크를 풀고 **사람이 손으로** 끝까지 돌린다
            //=========================================================
            /// <summary>★토크를 풀고 사람이 손으로 돌린 범위를 관절 한계로 잡는다.
            ///
            /// 구동 측정(MechLimit)의 고질병은 "서보가 멈춘 각"과 "기구가 막힌 각"을 구별 못 하는 것이다.
            /// 저PWM 에서는 중력·마찰이 먼저 이겨 `gravity_suspect` 가 붙고, PWM 을 올리면 이번엔
            /// 기구를 때린다. 손으로 돌리면 **사람이 벽을 직접 느끼므로** 그 구별이 필요 없다.
            ///
            /// 절차: 대상 축만 토크 OFF → 사람이 양쪽 끝까지 천천히 → 최소/최대 기록 → 토크 복구.
            /// ★토크 복구 전에 목표위치를 **현재 위치로** 써 넣는다. 안 그러면 마지막 목표(0도)로
            ///   전속 복귀하며 손·기구를 친다.
            /// 대상 외 축은 건드리지 않는다 (매달린 로봇이 무너지지 않게).</summary>
            /// <param name="nSeconds">학습 시간(초). 그동안 손으로 돌리면 된다.</param>
            public static bool TeachLimit(CMechCfg cfg, string strDir, DLog log, DIsStopped stop,
                                          Dictionary<int, CLimit> outLimits, int nSeconds)
            {
                // 기존 결과를 먼저 불러온다 — 이번에 배우는 축만 갱신 (MechLimit 과 같은 규약)
                int nKept = 0;
                foreach (var kvOld in LoadMechLimits(Path.Combine(strDir, "mech_limits.json")))
                    if (!outLimits.ContainsKey(kvOld.Key)) { outLimits[kvOld.Key] = kvOld.Value; nKept++; }
                // ★덜 민 채로 배운 값이 정본을 덮어쓸 수 있다 — 손대기 전에 사본부터
                BackupMechLimits(Path.Combine(strDir, "mech_limits.json"), "before_teach", log);

                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;

                var dicLo = new Dictionary<int, float>();
                var dicHi = new Dictionary<int, float>();
                try
                {
                    if (nKept > 0 && log != null)
                        log(string.Format("기존 결과 {0}축 불러옴 — 이번에 배우는 축만 갱신됩니다", nKept));

                    foreach (int id in cfg.anIDs)
                    {
                        float f0 = dicCur.ContainsKey(id) ? dicCur[id] : 0f;
                        dicLo[id] = f0; dicHi[id] = f0;
                    }

                    // ── 토크 OFF (대상 축만) ──
                    foreach (int id in cfg.anIDs) { com.Send(id, 3, 64, (byte)0); Thread.Sleep(5); }
                    if (log != null)
                    {
                        log("토크를 풀었습니다 — 이제 손으로 양쪽 끝까지 천천히 돌리십시오.");
                        log(string.Format("   대상 축: {0} / 학습 {1}초",
                            string.Join(",", Array.ConvertAll(cfg.anIDs, x => "T" + (x - cfg.nIdOffset))), nSeconds));
                    }

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    int nTick = 0;
                    while (sw.Elapsed.TotalSeconds < nSeconds)
                    {
                        if (stop != null && stop()) { if (log != null) log("사용자 중단"); break; }
                        string strWhy;
                        var cur = ReadAll(com, cfg.anIDs, out strWhy);
                        if (cur == null) { Thread.Sleep(20); continue; }   // 한 번 실패는 넘어간다
                        foreach (var kv in cur)
                        {
                            if (kv.Value < dicLo[kv.Key]) dicLo[kv.Key] = kv.Value;
                            if (kv.Value > dicHi[kv.Key]) dicHi[kv.Key] = kv.Value;
                        }
                        // 진행이 보이게 — 멈춘 것처럼 보이면 사람이 전원을 끈다 (2026-07-27 실사고)
                        if (++nTick % 10 == 0 && log != null)
                        {
                            var sbP = new StringBuilder();
                            foreach (int id in cfg.anIDs)
                                sbP.AppendFormat("T{0} {1,7:F1} [{2,7:F1}~{3,-7:F1}]  ",
                                                 id - cfg.nIdOffset, cur.ContainsKey(id) ? cur[id] : 0f,
                                                 dicLo[id], dicHi[id]);
                            log(string.Format("  {0,3}초 남음  {1}",
                                              (int)(nSeconds - sw.Elapsed.TotalSeconds), sbP.ToString()));
                        }
                        Thread.Sleep(20);
                    }

                    foreach (int id in cfg.anIDs)
                    {
                        int nT = id - cfg.nIdOffset;
                        var e = new CLimit();
                        e.lo = dicLo[id];
                        e.hi = dicHi[id];
                        e.q = "hand_taught";
                        outLimits[id] = e;
                        if (log != null) log(string.Format("   T{0}: {1:F2} ~ {2:F2}  [hand_taught]", nT, e.lo, e.hi));
                    }
                    SaveMechLimits(Path.Combine(strDir, "mech_limits.json"),
                                   cfg.anIDs, cfg.nIdOffset, cfg.nPwm, outLimits);
                }
                finally
                {
                    try
                    {
                        // ★튕김 방지 — 목표를 현재 위치로 먼저 써 넣고 토크를 켠다.
                        //   (토크만 켜면 마지막 목표=0도 로 전속 복귀해 손을 친다)
                        string strWhy2;
                        var fin = ReadAll(com, cfg.anIDs, out strWhy2);
                        com.Command_Clear();
                        foreach (int id in cfg.anIDs)
                            com.Command_Set(id, (fin != null && fin.ContainsKey(id)) ? fin[id] : 0f);
                        com.Move_NoWait(100, 0); Thread.Sleep(120);
                        foreach (int id in cfg.anIDs) { com.Send(id, 3, 64, (byte)1); Thread.Sleep(5); }
                        if (log != null) log("토크 복구 (현재 위치 유지 — 0도로 튕기지 않습니다)");
                        SetPwm(com, cfg.anIDs, 885);
                        com.Close();
                    }
                    catch { }
                }
                return true;
            }

            /// <summary>모터각 리미트를 URDF 각으로 환산한다. ★sign&lt;0 이면 lo/hi 가 뒤바뀐다.</summary>
            public static void ToUrdfRange(CLimit e, int nSign, out float fLo, out float fHi)
            {
                if (nSign < 0) { fLo = -e.hi; fHi = -e.lo; }
                else { fLo = e.lo; fHi = e.hi; }
            }

            /// <summary>좌우 대칭쌍의 리미트가 서로 맞는지 본다 — 손으로 배운 값의 유일한 자기검증.
            ///
            /// URDF 각(sign 적용)으로 환산하면 대칭쌍은 거의 같은 범위가 나와야 한다.
            /// ★한계: 좌우를 **나란히** 덜 밀면 이 검사도 통과한다. 2026-08-28 팔꿈치가 그랬다 —
            ///   좌우 차 1.3도로 통과했지만 둘 다 40도 모자랐고, 기존값이 정답이었다.
            ///   통과 = "좌우가 일관됨" 이지 "벽까지 갔음" 이 아니다.</summary>
            /// <returns>허용치를 넘은 쌍의 수 (0 이면 전부 일치)</returns>
            public static int MirrorCheck(CMechCfg cfg, Dictionary<int, CLimit> lim,
                                          Dictionary<int, int> signs, float fTolDeg, DLog log)
            {
                if (log != null) log("── 좌우 대칭 검사 (URDF 각) ──");
                var seen = new Dictionary<int, bool>();
                int nBad = 0, nPair = 0;
                var lstIds = new List<int>(lim.Keys);
                lstIds.Sort();
                foreach (int id in lstIds)
                {
                    int nT = id - cfg.nIdOffset;
                    if (seen.ContainsKey(nT)) continue;
                    if (!cfg.Mirror.ContainsKey(nT)) continue;
                    int nT2 = cfg.Mirror[nT];
                    int id2 = nT2 + cfg.nIdOffset;
                    if (!lim.ContainsKey(id2)) continue;
                    seen[nT] = true; seen[nT2] = true;

                    int s1 = signs.ContainsKey(id) ? signs[id] : +1;
                    int s2 = signs.ContainsKey(id2) ? signs[id2] : +1;
                    float lo1, hi1, lo2, hi2;
                    ToUrdfRange(lim[id], s1, out lo1, out hi1);
                    ToUrdfRange(lim[id2], s2, out lo2, out hi2);
                    float dLo = Math.Abs(lo1 - lo2), dHi = Math.Abs(hi1 - hi2);
                    bool bBad = (dLo > fTolDeg || dHi > fTolDeg);
                    if (bBad) nBad++;
                    nPair++;
                    if (log != null)
                        log(string.Format("  T{0,-2}/T{1,-2}  {2,8:F2}~{3,-8:F2} vs {4,8:F2}~{5,-8:F2}"
                                        + "   차 {6,6:F2}/{7,-6:F2}  {8}",
                                          nT, nT2, lo1, hi1, lo2, hi2, dLo, dHi, bBad ? "★어긋남" : "OK"));
                }
                if (log != null)
                {
                    var sbSolo = new StringBuilder();
                    foreach (int id in lstIds)
                        if (!cfg.Mirror.ContainsKey(id - cfg.nIdOffset))
                            sbSolo.AppendFormat("T{0} ", id - cfg.nIdOffset);
                    if (sbSolo.Length > 0) log("  단독축(짝 없음, 검사 불가): " + sbSolo.ToString().Trim());
                    log(string.Format("  대칭쌍 {0}쌍 중 {1}쌍 어긋남 (허용 {2:F1}도)", nPair, nBad, fTolDeg));
                }
                return nBad;
            }

            //=========================================================
            // JSON 입출력 (외부 라이브러리 없이 — .NET 4.0 대상)
            //=========================================================
            public static Dictionary<int, int> LoadJointMap(string strPath)
            {
                var d = new Dictionary<int, int>();
                if (!File.Exists(strPath)) return d;
                try
                {
                    string t = File.ReadAllText(strPath);
                    int i = 0;
                    while (true)
                    {
                        int a = t.IndexOf("\"real_id\"", i);
                        if (a < 0) break;
                        int b = t.IndexOf("\"sign\"", a);
                        if (b < 0) break;
                        int nId = IntAfter(t, a + 9);
                        int nSg = IntAfter(t, b + 6);
                        if (nId > 0 && (nSg == 1 || nSg == -1)) d[nId] = nSg;
                        i = b + 6;
                    }
                }
                catch { }
                return d;
            }

            public static void SaveJointMap(string strPath, int[] anIDs, int nIdOffset, Dictionary<int, int> signs)
            {
                var sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"note\": \"OpenJigWare sim2real 관절 방향 실측. zero_deg=0(all-0=기립). "
                            + "read sim_deg = sign*real_deg; write real_deg = sign*sim_deg. "
                            + "sign=-1 은 EDH Dir 이 실물과 반대라는 뜻.\",");
                sb.AppendLine("  \"source\": \"Ojw.CSim2Real.SignCalib\",");
                sb.AppendLine("  \"id_offset\": " + nIdOffset + ",");
                sb.Append("  \"sim_joint_order\": [");
                for (int i = 0; i < anIDs.Length; i++)
                    sb.Append((i > 0 ? ", " : "") + "\"T" + (anIDs[i] - nIdOffset) + "\"");
                sb.AppendLine("],");
                sb.AppendLine("  \"map\": [");
                int n = 0;
                for (int i = 0; i < anIDs.Length; i++)
                {
                    int id = anIDs[i];
                    if (!signs.ContainsKey(id)) continue;
                    if (n++ > 0) sb.AppendLine(",");
                    sb.Append(string.Format(CultureInfo.InvariantCulture,
                        "    {{ \"sim_idx\": {0}, \"sim_name\": \"T{1}\", \"real_id\": {2}, "
                      + "\"sign\": {3}, \"zero_deg\": 0.0, \"identified\": true }}",
                        i, id - nIdOffset, id, signs[id]));
                }
                sb.AppendLine();
                sb.AppendLine("  ]");
                sb.AppendLine("}");
                File.WriteAllText(strPath, sb.ToString(), new UTF8Encoding(false));
            }

            public static Dictionary<int, CLimit> LoadMechLimits(string strPath)
            {
                var d = new Dictionary<int, CLimit>();
                if (!File.Exists(strPath)) return d;
                try
                {
                    string t = File.ReadAllText(strPath);
                    int i = 0;
                    while (true)
                    {
                        int a = t.IndexOf("\"real_id\"", i);
                        if (a < 0) break;
                        int nId = IntAfter(t, a + 9);
                        int bLo = t.IndexOf("\"lo\"", a), bHi = t.IndexOf("\"hi\"", a), bQ = t.IndexOf("\"quality\"", a);
                        if (bLo < 0 || bHi < 0) break;
                        var e = new CLimit();
                        e.lo = FloatAfter(t, bLo + 4);
                        e.hi = FloatAfter(t, bHi + 4);
                        e.q = "";
                        if (bQ > 0)
                        {
                            int q1 = t.IndexOf('"', bQ + 9);
                            int q2 = (q1 > 0) ? t.IndexOf('"', q1 + 1) : -1;
                            if (q2 > q1) e.q = t.Substring(q1 + 1, q2 - q1 - 1);
                        }
                        if (nId > 0) d[nId] = e;
                        i = bHi + 4;
                    }
                }
                catch { }
                return d;
            }

            public static void SaveMechLimits(string strPath, int[] anIDs, int nIdOffset, int nPwm,
                                              Dictionary<int, CLimit> lim)
            {
                var sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"note\": \"OpenJigWare sim2real 관절 리미트 (모터각 공간, deg). "
                            + "URDF 각으로 바꾸려면 joint_map.json 의 sign 을 곱한다(sign<0 이면 lo/hi 가 뒤바뀐다). "
                            + "quality: hand_taught=토크를 풀고 손으로 배운 값(정본) / "
                            + "gravity_free=중력보상 측정(신뢰) / trusted=벽형 스톱 / "
                            + "gravity_suspect=중력 한계일 수 있음 / self_collision_excluded=미러 협조값 / "
                            + "no_limit=상한까지 무간섭\",");
                sb.AppendLine("  \"source\": \"Ojw.CSim2Real.MechLimit\",");
                sb.AppendLine("  \"pwm_limit\": " + nPwm + ",");
                sb.AppendLine("  \"limits\": {");
                int n = 0;
                // ★anIDs 로 돌면 "이번에 잰 축" 만 남고 나머지가 파일에서 사라진다.
                //   MechLimit 이 기존 결과를 불러와 lim 에 합쳐두는데, 저장이 그 병합을 버렸다
                //   (T17 만 재측정했더니 16축이 날아간 사고 — 불러오기만 고치고 저장은 그대로였다.
                //    2026-08-28 재발, 백업 덕에 발견). lim 에 든 전부를 ID 순으로 기록한다.
                var lstIds = new List<int>(lim.Keys);
                lstIds.Sort();
                foreach (int id in lstIds)
                {
                    if (!lim.ContainsKey(id)) continue;
                    if (n++ > 0) sb.AppendLine(",");
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "    \"T{0}\": {{ \"real_id\": {1}, \"lo\": {2:F2}, \"hi\": {3:F2}, \"quality\": \"{4}\" }}",
                        id - nIdOffset, id, lim[id].lo, lim[id].hi, lim[id].q);
                }
                sb.AppendLine();
                sb.AppendLine("  }");
                sb.AppendLine("}");
                File.WriteAllText(strPath, sb.ToString(), new UTF8Encoding(false));
            }

            /// <summary>측정 전 mech_limits.json 사본을 남긴다.
            ///
            /// 덜 민 채로 배운 값이나 잘못 잰 값이 원본을 덮어써도 되돌릴 수 있다.
            /// 2026-08-28 에 이 백업으로 "1축만 재측정하면 나머지 16축이 사라지는" 저장 버그를 발견했다.</summary>
            /// <returns>만든 백업 경로 (원본이 없으면 null)</returns>
            public static string BackupMechLimits(string strPath, string strTag, DLog log)
            {
                try
                {
                    if (!File.Exists(strPath)) return null;
                    string strBak = Path.Combine(Path.GetDirectoryName(strPath),
                        string.Format("{0}.{1}_{2}.json", Path.GetFileNameWithoutExtension(strPath),
                                      strTag, DateTime.Now.ToString("MMdd_HHmmss")));
                    File.Copy(strPath, strBak, true);
                    if (log != null) log("측정 전 백업: " + Path.GetFileName(strBak));
                    return strBak;
                }
                catch (Exception ex)
                {
                    if (log != null) log("백업 실패(그대로 진행): " + ex.Message);
                    return null;
                }
            }

            private static int IntAfter(string t, int nPos)
            {
                int i = nPos;
                while (i < t.Length && (t[i] == ':' || t[i] == ' ' || t[i] == '\t')) i++;
                int s = i;
                if (i < t.Length && (t[i] == '-' || t[i] == '+')) i++;
                while (i < t.Length && t[i] >= '0' && t[i] <= '9') i++;
                int v;
                if (i == s || !int.TryParse(t.Substring(s, i - s), out v)) return 0;
                return v;
            }

            private static float FloatAfter(string t, int nPos)
            {
                int i = nPos;
                while (i < t.Length && (t[i] == ':' || t[i] == ' ' || t[i] == '\t')) i++;
                int s = i;
                if (i < t.Length && (t[i] == '-' || t[i] == '+')) i++;
                while (i < t.Length && ((t[i] >= '0' && t[i] <= '9') || t[i] == '.')) i++;
                float v;
                if (i == s || !float.TryParse(t.Substring(s, i - s), NumberStyles.Float,
                                              CultureInfo.InvariantCulture, out v)) return 0f;
                return v;
            }

            //=========================================================
            // ③ 반영 — 모터각 리미트 x 부호 -> URDF 각 $jointrange
            //=========================================================
            /// <summary>$jointrange 줄 뒤 주석에 lock 이 붙었으면 사람이 확정한 값 —
            /// ③ 이 측정값으로 덮어쓰지 않는다. 다시 열려면 그 주석의 lock 만 지우면 된다.</summary>
            /// <summary>"$jointrange, T13, ..." 에서 13 을 뽑는다 (정렬용, 못 읽으면 9999).</summary>
            private static int TNumOf(string strLine)
            {
                try
                {
                    string[] tk = strLine.Split(',');
                    if (tk.Length < 2) return 9999;
                    string t = tk[1].Trim();
                    if (t.Length < 2 || (t[0] != 'T' && t[0] != 't')) return 9999;
                    int v;
                    return int.TryParse(t.Substring(1), out v) ? v : 9999;
                }
                catch { return 9999; }
            }

            public static bool IsLockedRange(string strLine)
            {
                if (string.IsNullOrEmpty(strLine)) return false;
                int i = strLine.IndexOf("//");
                if (i < 0) return false;
                return strLine.Substring(i).ToLower().Contains("lock");
            }

            /// <summary>EDH 텍스트의 $jointrange 줄을 실측값으로 갈아끼운다.
            /// 돌려주는 값 = 기입한 축 수. strReport 에 변환 내역이 담긴다.
            /// 주석에 lock 이 붙은 줄은 건드리지 않는다 (IsLockedRange).</summary>
            public static int ApplyJointRange(string strDir, ref string strEdh, int nIdOffset,
                                              out string strReport)
            {
                var sb = new StringBuilder();
                var signs = LoadJointMap(Path.Combine(strDir, "joint_map.json"));
                var mech = LoadMechLimits(Path.Combine(strDir, "mech_limits.json"));
                strReport = "";
                if (signs.Count == 0) { strReport = "① 관절방향 결과(joint_map.json)가 없습니다."; return 0; }
                if (mech.Count == 0) { strReport = "② 관절리미트 결과(mech_limits.json)가 없습니다."; return 0; }

                // ★기존 $jointrange 를 먼저 읽어 둔다.
                //   건너뛴 축의 기존 값까지 지우면 URDF 가 ±180 플레이스홀더로 되돌아간다.
                //   2026-07-31 humanoid 회신3 §9-1 이 "수 회 재발"이라 지목한 함정이 바로 이것이고,
                //   실제로 전달본 URDF 에서 팔 6축(T1/T2/T13/T3/T4/T14)이 ±180 으로 회귀해 있었다.
                //   측정값이 없으면 **기존 값을 그대로 살려 둔다** — 지우는 것보다 낫다.
                var dicOld = new Dictionary<int, string>();
                foreach (string raw0 in strEdh.Replace("\r\n", "\n").Split('\n'))
                {
                    string s0 = raw0.TrimStart();
                    if (!s0.StartsWith("$jointrange")) continue;
                    string[] tk = s0.Split(',');
                    if (tk.Length < 4) continue;
                    string strT = tk[1].Trim();
                    if (strT.Length < 2 || (strT[0] != 'T' && strT[0] != 't')) continue;
                    int nOldT;
                    if (!int.TryParse(strT.Substring(1), out nOldT)) continue;
                    dicOld[nOldT] = raw0.TrimEnd();
                }

                var lstLine = new List<string>();
                var lstKept = new List<string>();
                int nSkip = 0, nPreserved = 0, nOverwrote = 0;
                foreach (var kv in mech)
                {
                    int id = kv.Key;
                    string strWhySkip = null;
                    // ★사람이 잠근 줄은 측정값보다 세다. 측정은 "사람이 민 만큼"만 알고,
                    //   기능 클램프(발날 딛기 금지 같은)는 애초에 기구 한계가 아니다.
                    if (dicOld.ContainsKey(id - nIdOffset) && IsLockedRange(dicOld[id - nIdOffset]))
                        strWhySkip = "사람이 잠금 (// lock)";
                    else if (!signs.ContainsKey(id)) strWhySkip = "부호 없음";
                    else if (kv.Value.q == "no_limit") strWhySkip = "제한 없음";
                    else if (kv.Value.q == "measure_failed") strWhySkip = "측정 실패";
                    else if (kv.Value.q == "self_collision_excluded") strWhySkip = "충돌형상 처리 대상";
                    if (strWhySkip != null)
                    {
                        nSkip++;
                        int nT = id - nIdOffset;
                        if (dicOld.ContainsKey(nT))
                        {
                            lstKept.Add(dicOld[nT]);
                            dicOld.Remove(nT);
                            nPreserved++;
                            sb.AppendFormat("  T{0,-3} 건너뜀({1}) — 기존 값 보존\r\n", nT, strWhySkip);
                        }
                        else
                            sb.AppendFormat("  T{0,-3} 건너뜀({1}) — 기존 값도 없음 (URDF 는 ±180 이 된다)\r\n",
                                            nT, strWhySkip);
                        continue;
                    }
                    string strOldLine = dicOld.ContainsKey(id - nIdOffset) ? dicOld[id - nIdOffset] : null;
                    dicOld.Remove(id - nIdOffset);       // 측정값으로 교체 — 옛 줄이 중복되지 않게
                    float lo = kv.Value.lo, hi = kv.Value.hi;
                    // ★sign<0 이면 lo/hi 가 뒤바뀐다 — 이걸 놓쳐 5축이 반대로 실렸었다
                    if (signs[id] < 0) { float t = lo; lo = -hi; hi = -t; }
                    string strWarn = (kv.Value.q == "gravity_suspect") ? "   // 중력 한계 의심 — 재측정 권장" : "";
                    lstLine.Add(string.Format(CultureInfo.InvariantCulture,
                        "$jointrange, T{0}, {1:F2}, {2:F2}{3}", id - nIdOffset, lo, hi, strWarn));
                    sb.AppendFormat("  T{0,-3} 모터 {1,8:F2}~{2,-8:F2} sign {3:+0;-0} -> URDF {4,8:F2}~{5:F2}  [{6}]\r\n",
                                    id - nIdOffset, kv.Value.lo, kv.Value.hi, signs[id], lo, hi, kv.Value.q);
                    // ★사람이 손질해 둔 값을 말없이 덮어쓰지 않게 차이를 보고한다.
                    //   2026-08-28 팔꿈치가 그랬다 — 덜 민 학습값(-125.00)이 확정값(-140.06)을 되돌릴 뻔했다.
                    //   측정은 사람이 민 만큼만 알고, EDH 값은 사람이 판단한 결과다. 충돌하면 사람이 정본이다.
                    if (strOldLine != null)
                    {
                        string[] tkO = strOldLine.Split(',');
                        float oLo, oHi;
                        if (tkO.Length >= 4
                            && float.TryParse(tkO[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out oLo)
                            && float.TryParse(tkO[3].Split('/')[0].Trim(), NumberStyles.Float,
                                              CultureInfo.InvariantCulture, out oHi)
                            && (Math.Abs(oLo - lo) > 5.0f || Math.Abs(oHi - hi) > 5.0f))
                        {
                            nOverwrote++;
                            sb.AppendFormat("      ★기존 {0:F2}~{1:F2} 를 덮어씁니다 — 사람이 정한 값이었다면 되돌리십시오\r\n",
                                            oLo, oHi);
                        }
                    }
                }
                if (lstLine.Count == 0) { strReport = "반영할 값이 없습니다."; return 0; }

                const string MARK = "// ── 실측 관절한계 (sim2real";
                const string MARK2 = "// ── 기존 값 유지";
                var keep = new List<string>();
                int nRemoved = 0;
                foreach (string raw in strEdh.Replace("\r\n", "\n").Split('\n'))
                {
                    string s = raw.TrimStart();
                    if (s.StartsWith("$jointrange")) { nRemoved++; continue; }
                    // 이전 실행이 남긴 표제 — 쌓이지 않게 지운다 (MARK2 를 빠뜨려 매 실행마다 한 줄씩 늘어났다)
                    if (s.StartsWith(MARK) || s.StartsWith(MARK2)) continue;
                    keep.Add(raw);
                }
                // mech_limits 에 아예 없는 축의 기존 줄도 살린다 (예: T3 — 측정 대상이 아니었다)
                foreach (var kvOld in dicOld) { lstKept.Add(kvOld.Value); nPreserved++; }
                // 끝의 빈 줄은 버린다 — 안 그러면 ③ 을 누를 때마다 EDH 가 한 줄씩 길어진다
                while (keep.Count > 0 && keep[keep.Count - 1].Trim().Length == 0) keep.RemoveAt(keep.Count - 1);

                var outSb = new StringBuilder();
                outSb.AppendLine(MARK + ", 모터각->URDF 부호 적용) ──");
                foreach (string s in lstLine) outSb.AppendLine(s);
                if (lstKept.Count > 0)
                {
                    outSb.AppendLine(MARK2 + " — lock 이거나 측정값이 없는 축 (지우면 URDF 가 ±180 으로 회귀한다) ──");
                    lstKept.Sort(delegate(string x, string y) { return TNumOf(x).CompareTo(TNumOf(y)); });
                    foreach (string s in lstKept) outSb.AppendLine(s);
                }
                foreach (string s in keep) outSb.AppendLine(s);
                strEdh = outSb.ToString();

                strReport = string.Format(
                    "$jointrange {0}축 기입 / {1}축 기존값 보존 (건너뜀 {2}축, 원본 {3}줄 재구성)\r\n{4}\r\n{5}",
                    lstLine.Count, nPreserved, nSkip, nRemoved,
                    nOverwrote > 0
                        ? string.Format("★{0}축은 기존 값과 5도 넘게 다릅니다 — 아래에서 확인하십시오", nOverwrote)
                        : "",
                    sb.ToString());
                return lstLine.Count;
            }

            //=========================================================
            // ④ 기립 사지탈 처짐 — stand_droop.csv
            //
            // 왜 필요한가:
            //   ①②와 오전의 강성 측정은 **전부 매달림**에서 했다. 기립은 두 다리와
            //   지면이 닫힌 사슬이라 하중 경로가 아예 다르다. 매달림에서 잰 서보 법칙이
            //   기립에서도 통하는지는 따로 확인해야 한다.
            //   (2026-07-27 오후: compliance.csv 에서 롤 축 3개는 이미 확인됐다 —
            //    T7 0.91배 / T10 1.01배 / T12 0.86배. 사지탈은 미확인이었다.)
            //
            // 무엇을 재는가:
            //   사지탈 평행사변형으로 골반을 앞뒤로 평행이동시키며
            //   T6/T8(힙피치)·T9/T11(발목피치)의 명령·실제·하중을 기록한다.
            //   처짐/하중 기울기가 매달림값(0.01454 deg/load)과 같은지 본다.
            //
            // ★안전
            //   · 패턴은 새로 유추하지 않는다. 좌우대칭 후보 2개만 시험하고
            //     몸통이 안 기우는 쪽을 고른다 (R7 — 비대칭 조합은 내부응력만 만든다).
            //   · 명령 전에 기구한계표로 클램프한다 (R9).
            //   · 전도경계(실측 전방 16.5도)의 절반까지만 간다.
            //   · PWM 은 운전값 그대로 써야 한다. 낮추면 토크 포화가 처짐으로 둔갑한다(R10).
            //     대신 하중 상한과 자이로 감시로 막는다.
            //=========================================================
            public class CStandCfg : CCfg
            {
                public int nImuId = 200;              // CM-550
                public float fMaxLean = 8.0f;         // 평행이동 (시뮬 ±12 까지 무전도)
                public float fStep = 2.0f;
                public float fBowMax = 4.0f;          // 숙임 (시뮬 전방 11 / 후방 8 에서 전도)
                public float fBowStep = 1.0f;
                public bool bSquat = false;           // ★기본 끔 — 시뮬에서 10도에 전도한다
                public float fSquatMax = 4.0f;
                public float fSquatStep = 1.0f;
                public int nMoveMs = 900;
                public int nSettleMs = 800;
                public int nLoadAbort = 400;          // 이 하중을 넘으면 즉시 후퇴
                public float fGyroAbort = 25.0f;
                public float fPitchAbort = 20.0f;     // 몸통 pitch 이탈 상한 (전도 전 경고 신호)
                public float fMechMargin = 1.5f;
                public CStandCfg() { nPwm = 885; }    // ★운전 조건 — 낮추면 측정이 오염된다
            }

            private static void Imu(Ojw.CProtocol com, int nId,
                                    out float fRoll, out float fPitch, out float fGr, out float fGp)
            {
                com.SyncRead_With_Address(102, 12, nId); Thread.Sleep(12);
                fRoll = com.GetMap_Short(nId, 102) * 0.01f;
                fPitch = com.GetMap_Short(nId, 104) * 0.01f;
                fGr = com.GetMap_Short(nId, 108) * 0.01f;
                fGp = com.GetMap_Short(nId, 110) * 0.01f;
            }

            private static Dictionary<int, int> ReadLoads(Ojw.CProtocol com, int[] anIDs)
            {
                var d = new Dictionary<int, int>();
                com.SyncRead_With_Address(126, 2, anIDs); Thread.Sleep(14);
                foreach (int id in anIDs) d[id] = (short)com.GetMap_Short(id, 126);
                return d;
            }

            /// <summary>패턴 x L 을 기구한계로 클램프해 인가. 걸린 관절이 있으면 strClamp 로 알린다.</summary>
            private static void ApplyPat(Ojw.CProtocol com, CStandCfg cfg,
                                         Dictionary<int, CLimit> mech, Dictionary<int, float> pat,
                                         float fL, int nMs, out string strClamp)
            {
                strClamp = "";
                com.Command_Clear();
                foreach (int id in cfg.anIDs)
                {
                    int nT = id - cfg.nIdOffset;
                    float v = pat.ContainsKey(nT) ? pat[nT] * fL : 0f;
                    CLimit lim;
                    if (mech.TryGetValue(nT, out lim) && lim != null &&
                        lim.q != "no_limit" && lim.q != "measure_failed")
                    {
                        float lo = lim.lo + cfg.fMechMargin, hi = lim.hi - cfg.fMechMargin;
                        if (v < lo) { v = lo; strClamp = "T" + nT + " 하한"; }
                        else if (v > hi) { v = hi; strClamp = "T" + nT + " 상한"; }
                    }
                    com.Command_Set(id, v);
                }
                com.Move_NoWait(nMs, 0);
            }

            /// <summary>기립 사지탈 처짐 측정. 로봇은 **두 발로 서 있어야** 한다.</summary>
            public static bool StandDroop(CStandCfg cfg, string strDir, DLog log, DIsStopped stop,
                                          out string strReport)
            {
                strReport = "";
                const float HANG_SLOPE = 0.01454f;      // 매달림 실측 (legdroop)
                int[] anWatch = new int[] { 6, 8, 9, 11, 15, 16 };

                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    var mech = LoadMechLimits(Path.Combine(strDir, "mech_limits.json"));
                    if (log != null)
                        log(string.Format("기구한계 {0}축 적용 (R9 — 명령 전 클램프)", mech.Count));

                    if (!HomeAll(com, cfg, dicCur, log))
                    { if (log != null) log("0도 정렬 실패 — 중단"); return false; }

                    float r0, p0, g0, gp0;
                    Imu(com, cfg.nImuId, out r0, out p0, out g0, out gp0);
                    if (log != null) log(string.Format("기준 자세 roll {0:F2} pitch {1:F2}", r0, p0));

                    // ── 평행사변형 판별 (겸 기립 확인) ──────────────────
                    //   ★이 시험이 '서 있는지' 검사도 겸한다. 매달려 있으면 발이 지면에
                    //     안 눌리므로 어느 패턴을 줘도 몸통 pitch 가 거의 안 변한다.
                    var cands = new Dictionary<string, Dictionary<int, float>>();
                    cands["all_same"] = new Dictionary<int, float> { { 9, +1 }, { 11, +1 }, { 6, +1 }, { 8, +1 } };
                    cands["hip_opp"] = new Dictionary<int, float> { { 9, +1 }, { 11, +1 }, { 6, -1 }, { 8, -1 } };

                    string strBest = null, strClamp;
                    float fBestAbs = 1e9f, fMaxAbs = 0f;
                    Dictionary<int, float> patBest = null;
                    if (log != null) log("사지탈 평행사변형 판별 (+5도, 좌우대칭 후보만)");
                    foreach (var kv in cands)
                    {
                        if (stop != null && stop()) { if (log != null) log("중단"); return false; }
                        ApplyPat(com, cfg, mech, kv.Value, 0f, 800, out strClamp); Thread.Sleep(1100);
                        float ra, pa, ga, gpa; Imu(com, cfg.nImuId, out ra, out pa, out ga, out gpa);
                        ApplyPat(com, cfg, mech, kv.Value, 5f, 900, out strClamp); Thread.Sleep(1300);
                        float rb, pb, gb, gpb; Imu(com, cfg.nImuId, out rb, out pb, out gb, out gpb);
                        float dP = pb - pa;
                        if (log != null)
                            // ★{1,+7:F2} 는 안 된다 — 정렬 지정자에는 부호를 못 쓴다(FormatException).
                            //   부호를 보이려면 서식 쪽에 "+0.00;-0.00" 으로 준다.
                            log(string.Format("  {0,-9} 몸통 pitch {1,7:+0.00;-0.00}도", kv.Key, dP));
                        fMaxAbs = Math.Max(fMaxAbs, Math.Abs(dP));
                        if (Math.Abs(dP) < fBestAbs) { fBestAbs = Math.Abs(dP); strBest = kv.Key; patBest = kv.Value; }
                    }
                    ApplyPat(com, cfg, mech, patBest, 0f, 900, out strClamp); Thread.Sleep(1500);

                    if (fMaxAbs < 0.15f)
                    {
                        // 두 패턴 모두 몸통이 안 움직였다 = 지면 반력이 없다
                        if (log != null)
                        {
                            log(string.Format("★몸통 pitch 변화가 최대 {0:F2}도 뿐 — 서 있지 않은 것 같습니다.", fMaxAbs));
                            log("  이 측정은 두 발로 **지면에 세워둔 상태**에서만 뜻이 있습니다. 중단합니다.");
                        }
                        return false;
                    }
                    if (log != null) log(string.Format("→ 채택 {0} (몸통 |dPitch| {1:F2}도 = 평행이동)", strBest, fBestAbs));

                    // ── 스윕 대상 패턴들 ────────────────────────────────
                    //  ★한 패턴으로는 원하는 축을 다 못 싣는다 (시뮬 예측, kp 5.5 · 마찰 0.036):
                    //     평행이동 = 골반만 전후. 발목에 하중 124 까지, **힙은 31 뿐**.
                    //     숙임     = 몸통이 기움. 힙에 실리지만 전방 11 / 후방 8 에서 전도.
                    //     스쿼트   = 힙·무릎·발목 전부 165 까지. 다만 10도에서 전도 —— 기본 끔.
                    //  아라베스크가 요구하는 건 지지 힙피치라, 평행이동만 재면 정작 필요한 축이 빈다.
                    var patBow = (strBest == "all_same") ? cands["hip_opp"] : cands["all_same"];
                    var lstRun = new List<object[]>();
                    lstRun.Add(new object[] { "평행이동", patBest, cfg.fMaxLean, cfg.fStep });
                    lstRun.Add(new object[] { "숙임", patBow, cfg.fBowMax, cfg.fBowStep });
                    if (cfg.bSquat)
                    {
                        // 스쿼트 = 숙임에서 발목만 뒤집고 무릎을 2배로 (허벅지 67.8 ≈ 정강이 69.0)
                        var patSq = new Dictionary<int, float>();
                        foreach (var kv in patBow) patSq[kv.Key] = (kv.Key == 9 || kv.Key == 11) ? -kv.Value : kv.Value;
                        patSq[15] = +2f; patSq[16] = +2f;
                        lstRun.Add(new object[] { "스쿼트", patSq, cfg.fSquatMax, cfg.fSquatStep });
                    }

                    var csv = new StringBuilder("pattern,dir,lean,joint,cmd,pos,droop,load,clamp\n");
                    var pts = new Dictionary<string, List<float[]>>();
                    bool bAbort = false;

                    foreach (object[] run in lstRun)
                    {
                        if (bAbort) break;
                        string strPat = (string)run[0];
                        var pat = (Dictionary<int, float>)run[1];
                        float fMax = (float)run[2], fStp = (float)run[3];
                        int nSteps = Math.Max(1, (int)(fMax / fStp));
                        if (log != null)
                        {
                            log("");
                            log(string.Format("━━ {0} 패턴  (±{1:F0}도, {2:F0}도 스텝) ━━", strPat, fMax, fStp));
                        }
                        foreach (float fDir in new float[] { +1f, -1f })
                        {
                            if (bAbort) break;
                            if (log != null) log(string.Format("── {0} ──", fDir > 0 ? "전방(+)" : "후방(-)"));
                            for (int s = 1; s <= nSteps; s++)
                            {
                                if (stop != null && stop()) { bAbort = true; break; }
                                float fL = fDir * fStp * s;
                                ApplyPat(com, cfg, mech, pat, fL, cfg.nMoveMs, out strClamp);
                                Thread.Sleep(cfg.nMoveMs + cfg.nSettleMs);

                                float rr, pp, gr, gp; Imu(com, cfg.nImuId, out rr, out pp, out gr, out gp);
                                if (Math.Abs(gr) > cfg.fGyroAbort || Math.Abs(gp) > cfg.fGyroAbort)
                                {
                                    if (log != null)
                                        log(string.Format("  ★전도 조짐 (gyro R{0:F0} P{1:F0}) — 즉시 후퇴", gr, gp));
                                    bAbort = true; break;
                                }
                                // 자이로는 이미 넘어가는 중에야 뜬다. 몸통 각 이탈이 더 이른 신호다.
                                if (Math.Abs(pp - p0) > cfg.fPitchAbort)
                                {
                                    if (log != null)
                                        log(string.Format("  ★몸통 pitch {0:F1}도 이탈 — 전도 직전, 후퇴", pp - p0));
                                    bAbort = true; break;
                                }

                                string strWhy;
                                var pos = ReadAll(com, cfg.anIDs, out strWhy);
                                var lod = ReadLoads(com, cfg.anIDs);
                                if (pos == null)
                                { if (log != null) log("  위치 읽기 실패 — " + strWhy); continue; }

                                var sbLine = new StringBuilder();
                                foreach (int nT in anWatch)
                                {
                                    int id = nT + cfg.nIdOffset;
                                    if (!pos.ContainsKey(id)) continue;
                                    float fCmd = pat.ContainsKey(nT) ? pat[nT] * fL : 0f;
                                    float fPos = pos[id];
                                    int nLoad = Math.Abs(lod.ContainsKey(id) ? lod[id] : 0);
                                    if (nLoad > cfg.nLoadAbort)
                                    {
                                        if (log != null)
                                            log(string.Format("  ★T{0} 하중 {1} — 간섭/내부응력 의심, 즉시 후퇴", nT, nLoad));
                                        bAbort = true; break;
                                    }
                                    csv.AppendFormat(CultureInfo.InvariantCulture,
                                        "{0},{1},{2:F1},T{3},{4:F2},{5:F2},{6:F3},{7},{8}\n",
                                        strPat, fDir > 0 ? "fwd" : "bwd", fL, nT, fCmd, fPos, fCmd - fPos, nLoad, strClamp);
                                    if (nLoad >= 25)
                                    {
                                        string k = strPat + "|" + nT;
                                        if (!pts.ContainsKey(k)) pts[k] = new List<float[]>();
                                        pts[k].Add(new float[] { Math.Abs(fCmd - fPos), nLoad });
                                    }
                                    sbLine.AppendFormat("T{0} {1,5:F2}/{2,3}  ", nT, fCmd - fPos, nLoad);
                                }
                                if (bAbort) break;
                                if (log != null)
                                    log(string.Format("  lean {0,5:F1} 몸통{1,6:F2}  {2}{3}", fL, pp - p0, sbLine.ToString(),
                                                      strClamp == "" ? "" : "[" + strClamp + "]"));
                            }
                            ApplyPat(com, cfg, mech, pat, 0f, 1200, out strClamp); Thread.Sleep(1600);
                        }
                    }

                    // ── 정리 ────────────────────────────────────────────
                    com.Command_Clear();
                    foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                    com.Move_NoWait(1500, 0); Thread.Sleep(1800);

                    Directory.CreateDirectory(strDir);
                    File.WriteAllText(Path.Combine(strDir, "stand_droop.csv"),
                                      csv.ToString(), new UTF8Encoding(false));

                    var sb = new StringBuilder();
                    sb.AppendLine("기립 사지탈 처짐 — 매달림 법칙이 기립에서도 통하는가");
                    sb.AppendLine(string.Format("  평행이동 ±{0:F0} / 숙임 ±{1:F0}{2} / PWM {3} / 판별 {4}",
                                                cfg.fMaxLean, cfg.fBowMax,
                                                cfg.bSquat ? string.Format(" / 스쿼트 +{0:F0}", cfg.fSquatMax) : "",
                                                cfg.nPwm, strBest));
                    sb.AppendLine();
                    int nOk = 0;
                    foreach (object[] run in lstRun)
                    {
                        string strPat = (string)run[0];
                        sb.AppendLine(string.Format("  [{0}]", strPat));
                        sb.AppendLine("    축    n   deg/load   매달림대비");
                        foreach (int nT in anWatch)
                        {
                            string k = strPat + "|" + nT;
                            var L = pts.ContainsKey(k) ? pts[k] : new List<float[]>();
                            if (L.Count < 3)
                            {
                                sb.AppendLine(string.Format("    T{0,-3} {1,3}   (하중 25 미만 — 이 패턴은 이 축을 안 싣는다)", nT, L.Count));
                                continue;
                            }
                            double sxy = 0, sxx = 0;
                            foreach (float[] v in L) { sxy += v[1] * v[0]; sxx += (double)v[1] * v[1]; }
                            float fSlope = (float)(sxy / sxx);
                            sb.AppendLine(string.Format("    T{0,-3} {1,3}   {2:F5}    {3:F2}배", nT, L.Count, fSlope, fSlope / HANG_SLOPE));
                            nOk++;
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine("  1.0배에 가까우면 매달림에서 잰 kp 를 기립에도 그대로 써도 된다.");
                    sb.AppendLine("  크게 벗어나면 시뮬 kp 를 자세별로 나눠야 한다는 뜻이다.");
                    sb.AppendLine();
                    sb.AppendLine("  -> " + Path.Combine(strDir, "stand_droop.csv"));
                    strReport = sb.ToString();
                    if (log != null) { log(""); log(strReport); }
                    return (nOk > 0 && !bAbort);
                }
                finally
                {
                    SetPwm(com, cfg.anIDs, 885);
                    com.Close();
                }
            }

            //=========================================================
            // ⑤ 서보 동정 (매달림) — servo_ident.json + eeprom_dump.csv
            //
            //   sim2real 을 MakeUrdf 하나로 끝내기 위한 통합 측정 (2026-07-31):
            //   (1) EEPROM 전체 덤프 — 모델(스톨토크)·Drive Mode·한계 레지스터
            //   (2) 속도 상한 — Profile Velocity 0 계단 + addr126 len10 연속 채록,
            //       평탄부(3-표본 중앙값 최대). 방향별 = 사지 자중 비대칭이 담긴다.
            //   (3) 브레이크어웨이 — Goal 1틱 증분으로 움직임 시작 오차각.
            //       τ = 오차틱 × (Kp640/128)/885 × 스톨(모델별).  백래시 = 반전 히스테리시스 − 2×BA.
            //
            //   ★PWM 은 운전값(885) — 낮추면 속도·마찰이 아니라 토크 포화를 잰다(R10).
            //   ★힙롤(T5/T7)은 단독구동 시 반대다리 충돌 — 안쪽 행정을 6도로 제한.
            //=========================================================
            public class CIdentCfg : CCfg
            {
                public float fStepCap = 60f;          // 계단 행정 상한
                public int nCapMs = 800;              // 계단 채록 시간
                public CIdentCfg() { nPwm = 885; }
            }

            private static readonly int[,] EEPROM_REG = {
                { 0, 2 }, { 6, 1 }, { 7, 1 }, { 8, 1 }, { 9, 1 }, { 10, 1 }, { 11, 1 },
                { 12, 1 }, { 13, 1 }, { 20, 4 }, { 24, 4 }, { 31, 1 }, { 32, 2 }, { 34, 2 },
                { 36, 2 }, { 44, 4 }, { 48, 4 }, { 52, 4 }, { 63, 1 } };
            private static readonly string[] EEPROM_NAME = {
                "model", "firmware", "id", "baud", "return_delay", "drive_mode", "operating_mode",
                "shadow_id", "protocol", "homing_offset", "moving_threshold", "temp_limit",
                "max_volt", "min_volt", "pwm_limit", "vel_limit", "max_pos", "min_pos", "shutdown" };

            /// <summary>모델번호(addr 0) → 정식 모델명 (ROBOTIS). 미등록이면 null.</summary>
            public static string ModelNameOf(int nModel)
            {
                switch (nModel)
                {
                    case 1060: return "XL430-W250";
                    case 1070: return "XC430-W150";
                    case 1080: return "XC430-W240";
                    case 1090: return "2XL430-W250";
                    case 1160: return "2XC430-W250";
                    case 1020: return "XM430-W350";
                    case 1030: return "XM430-W210";
                    case 1190: return "XL330-M077";
                    case 1200: return "XL330-M288";
                }
                return null;
            }

            /// <summary>모델번호 → 스톨토크 [N·m]. ★서보 스펙 DB(COjwUrdfPhysics_t)가 정본이다.
            ///
            /// ★2026-09-03 정정 (답신 18 §1) — 종전에는 여기 `1080 ? 1.9 : 1.4` 가 박혀 있었다.
            ///   1.4 는 XL430 값이라 2XC430(1.6)·XC430-W240 축에 그대로 쓰면 안 되고,
            ///   1.9 는 XC430-W240 의 **12 V** 표기값이라 나머지(11.1 V 기준)와 전압 기준이 섞였다.
            ///   그 결과 2026-08-01 load_vmax_*.json 의 load_pu→N·m 환산이
            ///   힙·발목 14 % 과소 / 무릎 11 % 과대였다. 기존 파일은 재환산해서 쓸 것.</summary>
            private static float StallOf(int nModel)
            {
                string strName = ModelNameOf(nModel);
                if (strName != null)
                {
                    Ojw.C3d.COjwUrdfPhysics_t.ServoModelSpec spec =
                        Ojw.C3d.COjwUrdfPhysics_t.GetServoModel(strName);
                    if (spec != null) return (float)spec.StallTorqueNm;
                }
                return 1.4f;   // 미상 기종 — XL430 급으로 보수 가정
            }

            /// <summary>모델번호 → 스톨토크의 기준 전압 [V] (0 = 미상). 파일에 같이 적어
            /// "이 N·m 이 몇 V 기준인가"를 다시는 잃어버리지 않게 한다.</summary>
            private static float StallRefVoltOf(int nModel)
            {
                string strName = ModelNameOf(nModel);
                if (strName != null)
                {
                    Ojw.C3d.COjwUrdfPhysics_t.ServoModelSpec spec =
                        Ojw.C3d.COjwUrdfPhysics_t.GetServoModel(strName);
                    if (spec != null) return (float)spec.VoltageV;
                }
                return 0f;
            }

            /// <summary>맵에 이미 읽혀 있는 addr 144(전압)·146(온도)로 최저/최고를 갱신한다.
            /// ★추가 통신 없음 — 호출 전의 SyncRead 창이 146 까지 덮고 있어야 한다.
            /// 저전압 작업(답신 18)에서 "이 트라이얼이 몇 V 에서 났는가"를 사후에 감사하기 위한 것.
            /// 공급기가 전류 제한(CC)으로 주저앉거나 배터리가 흐르면 여기서 잡힌다.</summary>
            private static void TrackVT(Ojw.CProtocol com, int id, ref float fVmin, ref int nTmax)
            {
                float v = (com.GetMap_Short(id, 144) & 0xFFFF) * 0.1f;
                if (v > 0.5f && v < fVmin) fVmin = v;
                int t = com.GetMap(id, 146);
                if (t > nTmax && t < 120) nTmax = t;
            }

            private static float VoltOf(Ojw.CProtocol com, int id)
            {
                com.SyncRead_With_Address(144, 2, id); Thread.Sleep(20);
                com.SyncRead_With_Address(144, 2, id); Thread.Sleep(20);
                return (com.GetMap_Short(id, 144) & 0xFFFF) * 0.1f;
            }

            public static bool ServoIdent(CIdentCfg cfg, string strDir, DLog log, DIsStopped stop,
                                          out string strReport)
            {
                strReport = "";
                const float VEL_UNIT = 1.374f;    // 0.229rpm/LSB
                const float TICK = 0.087890625f;
                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    var mech = LoadMechLimits(Path.Combine(strDir, "mech_limits.json"));
                    float fV0 = VoltOf(com, cfg.anIDs[0]);

                    // ── (1) EEPROM ──────────────────────────────────
                    if (log != null) log("── EEPROM 덤프 (읽기 전용) ──");
                    var eeprom = new Dictionary<int, long[]>();
                    var csvE = new StringBuilder("id");
                    for (int r = 0; r < EEPROM_NAME.Length; r++) csvE.Append(',').Append(EEPROM_NAME[r]);
                    csvE.Append('\n');
                    foreach (int id in cfg.anIDs)
                    {
                        var vals = new long[EEPROM_NAME.Length];
                        csvE.Append(id);
                        for (int r = 0; r < EEPROM_NAME.Length; r++)
                        {
                            int addr = EEPROM_REG[r, 0], len = EEPROM_REG[r, 1];
                            long v = long.MinValue;
                            for (int t = 0; t < 4; t++)
                            {
                                com.SyncRead_With_Address(addr, len, id); Thread.Sleep(8);
                                long x = (len == 4) ? (long)com.GetMap_Int(id, addr)
                                       : (len == 2) ? (long)(com.GetMap_Short(id, addr) & 0xFFFF)
                                       : (long)(com.GetMap_Short(id, addr) & 0xFF);
                                if (v != long.MinValue && x == v) break;
                                v = x;
                            }
                            vals[r] = v;
                            csvE.Append(',').Append(v);
                        }
                        eeprom[id] = vals;
                        csvE.Append('\n');
                    }
                    Directory.CreateDirectory(strDir);
                    File.WriteAllText(Path.Combine(strDir, "eeprom_dump.csv"), csvE.ToString(), new UTF8Encoding(false));
                    if (log != null) log(string.Format("  {0}축 × {1}필드 -> eeprom_dump.csv", cfg.anIDs.Length, EEPROM_NAME.Length));

                    if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("0도 정렬 실패 — 중단"); return false; }

                    // ── (2)(3) 축별 vmax + 브레이크어웨이 ──────────────
                    var sbJ = new StringBuilder();
                    sbJ.AppendLine("{");
                    sbJ.AppendLine("  \"note\": \"OpenJigWare MakeUrdf ⑤ 서보동정 (매달림). vmax: ProfileVel0 계단 평탄부 [deg/s], tau_break: 정지 브레이크어웨이 [N·m], backlash [deg]. 방향별 차이 = 사지 자중. ★vmin_V/tmax_C 는 그 축의 전 트라이얼 중 최저 전압[addr144]·최고 온도[addr146] — meta.voltage_V(시작 무부하)와 크게 벌어지면 그 축은 전원이 주저앉은 상태에서 측정된 것이다. stall_Nm 은 stall_ref_V 기준값 (tau(V)=stall_Nm*V/stall_ref_V).\",");
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"meta\": {{ \"voltage_V\": {0:F1}, \"pwm\": {1}, \"posture\": \"hang\" }},\r\n", fV0, cfg.nPwm);

                    int k = 0;
                    var rep = new StringBuilder();
                    foreach (int id in cfg.anIDs)
                    {
                        if (stop != null && stop()) { if (log != null) log("중단"); return false; }
                        k++;
                        int nT = id - cfg.nIdOffset;
                        int nModel = eeprom.ContainsKey(id) ? (int)eeprom[id][0] : 0;
                        float fStall = StallOf(nModel);

                        // 행정: 기구한계 안쪽 3도, 캡. 힙롤은 안쪽 6도 제한(단독구동 충돌).
                        float lo = -cfg.fStepCap, hi = cfg.fStepCap;
                        CLimit lim;
                        if (mech.TryGetValue(nT, out lim) && lim != null && lim.q != "no_limit")
                        { lo = Math.Max(lo, lim.lo + 3f); hi = Math.Min(hi, lim.hi - 3f); }
                        if (nT == 5) lo = Math.Max(lo, -6f);
                        if (nT == 7) hi = Math.Min(hi, 6f);

                        float[] afV = new float[2];
                        float[] afTgt = { hi, lo };
                        // ★이 축의 전 트라이얼(무부하 vmax + 브레이크어웨이 + 백래시) 통틀어
                        //   최저 전압·최고 온도. 저전압 작업의 감사 지표 (답신 18 §5).
                        float fVminAx = 99f; int nTmaxAx = 0;
                        for (int d = 0; d < 2; d++)
                        {
                            com.Send(id, 3, 112, (byte)0, (byte)0, (byte)0, (byte)0); Thread.Sleep(5);
                            com.Command_Clear(); com.Command_Set(id, afTgt[d]);
                            com.Move_NoWait(0, 0);
                            var vels = new List<float>();
                            long l0 = Environment.TickCount;
                            while (Environment.TickCount - l0 < cfg.nCapMs)
                            {
                                // 126~146 한 방: vel(128)/pos(132) + ★전압(144)·온도(146)
                                //   10 → 21 바이트. 같은 SyncRead 라 왕복 횟수는 그대로다.
                                com.SyncRead_With_Address(126, 21, id); Thread.Sleep(2);
                                vels.Add(Math.Abs(com.GetMap_Int(id, 128) * VEL_UNIT));
                                TrackVT(com, id, ref fVminAx, ref nTmaxAx);
                                if (Math.Abs(Deg(com.GetMap_Int(id, 132)) - afTgt[d]) < 2f) break;
                            }
                            float fMax = 0;
                            for (int i = 1; i + 1 < vels.Count; i++)
                            {
                                float a = vels[i - 1], b = vels[i], c = vels[i + 1];
                                fMax = Math.Max(fMax, Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c)));
                            }
                            afV[d] = fMax;
                            com.Command_Clear(); com.Command_Set(id, 0f);
                            com.Move_NoWait(1000, 0); Thread.Sleep(1300);
                        }

                        // 브레이크어웨이 (양방향) + 백래시
                        float[] afBA = new float[2], afBL = new float[2];
                        for (int d = 0; d < 2; d++)
                        {
                            int sg = (d == 0) ? +1 : -1;
                            int p0 = Read(com, id, 132, 4, 2);
                            int goal = p0, nBreak = -1;
                            for (int i = 1; i <= 45; i++)
                            {
                                goal = p0 + sg * i;
                                com.Send(id, 3, 116, (byte)(goal & 0xFF), (byte)((goal >> 8) & 0xFF),
                                         (byte)((goal >> 16) & 0xFF), (byte)((goal >> 24) & 0xFF));
                                Thread.Sleep(110);
                                // 132~146 (4 → 15): 위치에 전압·온도를 얹는다 — 왕복 횟수 그대로
                                com.SyncRead_With_Address(132, 15, id); Thread.Sleep(6);
                                TrackVT(com, id, ref fVminAx, ref nTmaxAx);
                                if (Math.Abs(com.GetMap_Int(id, 132) - p0) >= 3) { nBreak = i; break; }
                            }
                            afBA[d] = (nBreak > 0) ? nBreak * TICK : float.NaN;
                            int nRev = -1;
                            if (nBreak > 0)
                            {
                                int pTop = Read(com, id, 132, 4, 2);
                                for (int i = 1; i <= 90; i++)
                                {
                                    goal -= sg;
                                    com.Send(id, 3, 116, (byte)(goal & 0xFF), (byte)((goal >> 8) & 0xFF),
                                             (byte)((goal >> 16) & 0xFF), (byte)((goal >> 24) & 0xFF));
                                    Thread.Sleep(110);
                                    com.SyncRead_With_Address(132, 15, id); Thread.Sleep(6);
                                    TrackVT(com, id, ref fVminAx, ref nTmaxAx);
                                    if ((pTop - com.GetMap_Int(id, 132)) * sg >= 3) { nRev = i; break; }
                                }
                            }
                            afBL[d] = (nRev > 0) ? Math.Max(0f, nRev * TICK - 2f * afBA[d]) : float.NaN;
                            com.Send(id, 3, 116, (byte)(p0 & 0xFF), (byte)((p0 >> 8) & 0xFF),
                                     (byte)((p0 >> 16) & 0xFF), (byte)((p0 >> 24) & 0xFF));
                            Thread.Sleep(300);
                        }

                        float fTauP = float.IsNaN(afBA[0]) ? float.NaN : (afBA[0] / TICK) * 5f / 885f * fStall;
                        float fTauN = float.IsNaN(afBA[1]) ? float.NaN : (afBA[1] / TICK) * 5f / 885f * fStall;
                        // ★stall_Nm 은 stall_ref_V 기준값 — τ_break 도 그 전압 기준이다.
                        //   전압 환산: τ(V) = τ × V / stall_ref_V (답신 18 §1)
                        sbJ.AppendFormat(CultureInfo.InvariantCulture,
                            "  \"T{0}\": {{ \"servo_id\": {1}, \"model\": {2}, \"model_name\": \"{11}\", " +
                            "\"stall_Nm\": {3:F2}, \"stall_ref_V\": {12:F1}, " +
                            "\"vmax_pos_dps\": {4:F0}, \"vmax_neg_dps\": {5:F0}, \"excursion_deg\": [{6:F0}, {7:F0}], " +
                            "\"tau_break_pos_Nm\": {8}, \"tau_break_neg_Nm\": {9}, \"backlash_deg\": {10}, " +
                            "\"vmin_V\": {13:F1}, \"tmax_C\": {14} }},\r\n",
                            nT, id, nModel, fStall, afV[0], afV[1], hi, lo,
                            float.IsNaN(fTauP) ? "null" : fTauP.ToString("F4", CultureInfo.InvariantCulture),
                            float.IsNaN(fTauN) ? "null" : fTauN.ToString("F4", CultureInfo.InvariantCulture),
                            float.IsNaN(afBL[0]) ? "null" : Math.Max(afBL[0], afBL[1]).ToString("F3", CultureInfo.InvariantCulture),
                            ModelNameOf(nModel) ?? "unknown", StallRefVoltOf(nModel),
                            (fVminAx > 98f) ? 0f : fVminAx, nTmaxAx);
                        string strLine = string.Format(
                            "[{0}/{1}] T{2,-3} vmax {3,4:F0}/{4,-4:F0} dps  τ_break {5:F3}/{6:F3}  BL {7:F2}  {8:F1}V/{9}°C{10}",
                            k, cfg.anIDs.Length, nT, afV[0], afV[1], fTauP, fTauN, Math.Max(afBL[0], afBL[1]),
                            (fVminAx > 98f) ? 0f : fVminAx, nTmaxAx,
                            (hi < 25f || lo > -25f) ? "  (행정 짧음 — 참고치)" : "");
                        rep.AppendLine(strLine);
                        if (log != null) log(strLine);
                    }
                    sbJ.AppendFormat(CultureInfo.InvariantCulture, "  \"voltage_end_V\": {0:F1}\r\n}}\r\n", VoltOf(com, cfg.anIDs[0]));
                    File.WriteAllText(Path.Combine(strDir, "servo_ident.json"), sbJ.ToString(), new UTF8Encoding(false));

                    com.Command_Clear();
                    foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                    com.Move_NoWait(1500, 0); Thread.Sleep(1800);
                    strReport = rep.ToString() + "\r\n-> servo_ident.json / eeprom_dump.csv";
                    return true;
                }
                finally { com.Close(); }
            }

            //=========================================================
            // ⑧ 부하 vmax (기립) — load_vmax_stand.json  (회신9 / R7)
            //
            //   v15 실기 전방 전도의 원인: sim 이 무부하 vmax(324~416dps)를
            //   부하에서도 성립한다고 가정 → 대진폭 고속 습관 학습 → 실물은
            //   체중 부하에서 유효 슬루 급락(추종 오차 56~67도)으로 전도.
            //   기립(체중 부하) 상태에서 다리 사지탈 6관절에 큰 스텝을 주고
            //   실행각 슬루를 채록한다. 스텝 20/40도 × 양방향 = speed-torque
            //   커브의 두 점. 같은 함수를 매달림으로 돌리면 무부하 대조가 된다.
            //   ★전도 위험 — 프레임 지지/사람 보조 전제 (전도 측정이 아니므로
            //     R14 무관 — 잡아도 슬루 신호는 오염되지 않는다).
            //=========================================================
            public class CLoadVmaxCfg : CStandCfg
            {
                public int[] anLegT = { 6, 8, 15, 16, 9, 11 };  // 힙피치/무릎/발목피치
                public float[] afSteps = { 20f, 40f };
                public int nCapMs = 1500;                       // 트라이얼 샘플링 상한
                public bool bStanding = true;                   // false = 매달림 무부하 대조
                public bool bOneFoot = false;                   // 기립일 때: 한발 지지(전체중) 모드 (회신10)
                public float fStanceTol = 5f;                   // 차렷 안정 판정 폭 [deg]
                public float fMinStroke = 12f;                  // 클램프 후 이보다 짧으면 스킵
            }

            private static float ClampMech(Dictionary<int, CLimit> mech, int nT, float fV)
            {
                CLimit lim;
                if (mech.TryGetValue(nT, out lim) && lim != null && lim.q != "no_limit" && lim.q != "measure_failed")
                    return Math.Max(lim.lo + 5f, Math.Min(lim.hi - 5f, fV));
                return fV;
            }

            /// <summary>한발 모드: 반대 다리 접기/펴기 (힙+무릎). bApply=false 면 0 복귀.
            /// ★굽힘 방향은 ① 관절방향(joint_map sign)으로 sim→servo 변환한다.
            ///   sim + = 힙/무릎 굽힘 (웅크림 setpoint T6/T8/T15/T16 전부 + 로 검증된 규약).
            ///   기구한계로 방향을 추정하면 안 된다 — 무릎이 no_limit 라서 기본 부호가
            ///   나가 좌무릎이 역굽힘된 실사고(2026-08-01).</summary>
            private static void LiftLeg(Ojw.CProtocol com, CLoadVmaxCfg cfg, Dictionary<int, CLimit> mech,
                                        Dictionary<int, int> signs, int nHipT, int nKneeT, bool bApply)
            {
                float fHip = 0f, fKnee = 0f;
                if (bApply)
                {
                    int nIdH = nHipT + cfg.nIdOffset, nIdK = nKneeT + cfg.nIdOffset;
                    int sH = signs.ContainsKey(nIdH) ? signs[nIdH] : +1;
                    int sK = signs.ContainsKey(nIdK) ? signs[nIdK] : +1;
                    fHip = ClampMech(mech, nHipT, sH * 25f);     // real = sign × sim(+25 굽힘)
                    fKnee = ClampMech(mech, nKneeT, sK * 45f);
                }
                com.Command_Clear();
                com.Command_Set(nHipT + cfg.nIdOffset, fHip);
                com.Command_Set(nKneeT + cfg.nIdOffset, fKnee);
                com.Move_NoWait(1200, 0); Thread.Sleep(1600);
            }

            /// <summary>기립 시행 사이의 자세 안정 게이트. 스텝을 넣으면 로봇이
            /// 넘어지는 게 정상이므로(R7 은 전도 측정이 아님), 사용자가 다시
            /// 세울 때까지 기다렸다가 차렷 ±tol 에서 1.5초 안정되면 진행한다.</summary>
            private static bool WaitStance(Ojw.CProtocol com, CLoadVmaxCfg cfg, float r0, float p0,
                                           DLog log, DIsStopped stop)
            {
                long lT0 = Environment.TickCount; long lOk = -1; bool bAsked = false;
                while (Environment.TickCount - lT0 < 180000)
                {
                    if (stop != null && stop()) return false;
                    float rr, pp, g1, g2; Imu(com, cfg.nImuId, out rr, out pp, out g1, out g2);
                    bool bOk = Math.Abs(rr - r0) < cfg.fStanceTol && Math.Abs(pp - p0) < cfg.fStanceTol
                            && Math.Abs(g1) < 8f && Math.Abs(g2) < 8f;
                    if (!bOk)
                    {
                        lOk = -1;
                        if (!bAsked)
                        { if (log != null) log("★로봇을 지지 자세로 다시 세워 주세요 — 1.5초 안정되면 자동 진행"); bAsked = true; }
                    }
                    else if (lOk < 0) lOk = Environment.TickCount;
                    else if (Environment.TickCount - lOk > 1500) return true;
                    Thread.Sleep(60);
                }
                if (log != null) log("180초 내 안정 자세를 얻지 못해 중단");
                return false;
            }

            public static bool LoadVmax(CLoadVmaxCfg cfg, string strDir, DLog log, DIsStopped stop,
                                        out string strReport)
            {
                strReport = "";
                const float VEL_UNIT = 1.374f;
                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    var mech = LoadMechLimits(Path.Combine(strDir, "mech_limits.json"));
                    if (mech.Count == 0) { if (log != null) log("mech_limits.json 없음 — 먼저 ② (R9)"); return false; }
                    var signs = LoadJointMap(Path.Combine(strDir, "joint_map.json"));
                    if (cfg.bOneFoot && signs.Count == 0)
                    { if (log != null) log("joint_map.json 없음 — 한발 모드는 ① 관절방향이 선행돼야 합니다 (다리 접는 방향에 씀)"); return false; }
                    string strPost = cfg.bStanding ? (cfg.bOneFoot ? "onefoot" : "stand") : "hang";
                    float fV0 = VoltOf(com, cfg.anIDs[0]);

                    if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("0도 정렬 실패 — 중단"); return false; }
                    Thread.Sleep(600);

                    // 차렷 IMU 기준 (1초 평균). 매달림이면 이게 곧 '매달림 차렷 pitch'
                    // — 접지 의존(-4~-2도 변동)을 제거한 순수 기하값 (답신 9 약속).
                    float r0 = 0, p0 = 0;
                    {
                        float fSr = 0, fSp = 0; int nN = 0;
                        for (int i = 0; i < 20; i++)
                        {
                            float rr, pp, g1, g2; Imu(com, cfg.nImuId, out rr, out pp, out g1, out g2);
                            fSr += rr; fSp += pp; nN++; Thread.Sleep(40);
                        }
                        r0 = fSr / nN; p0 = fSp / nN;
                        if (log != null) log(string.Format("차렷 IMU ({0}): roll {1:+0.00;-0.00}  pitch {2:+0.00;-0.00}",
                            cfg.bStanding ? "기립 기준" : "★매달림 — 순수 기하값", r0, p0));
                    }

                    var sbJ = new StringBuilder();
                    sbJ.AppendLine("{");
                    sbJ.AppendLine("  \"note\": \"MakeUrdf ⑧ 부하 vmax (R7/회신10). ProfileVel0 스텝의 평탄부 슬루 [deg/s] + 평탄부 하중 [pu, x stall_Nm = N·m] + 트라이얼 최저 순간전압 [V, addr144] + 최고 온도 [C, addr146] + Hardware Error [addr70]. 항목: [vmax_dps, load_pu, target_deg, vmin_V, tmax_C, hw_err] / null=행정부족 스킵. ★stall_Nm 은 stall_ref_V 기준값이다 (전압 환산은 tau(V)=stall_Nm*V/stall_ref_V) — 2026-08-01 이전 파일은 stall_Nm 이 기종/전압 기준이 섞여 있었으니 재환산할 것.\",");
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"meta\": {{ \"posture\": \"{0}\", \"voltage_V\": {1:F1}, \"pwm\": {2}, \"steps_deg\": [{3:F0}, {4:F0}], " +
                        "\"att_roll_deg\": {5:F2}, \"att_pitch_deg\": {6:F2} }},\r\n",
                        strPost, fV0, cfg.nPwm, cfg.afSteps[0], cfg.afSteps.Length > 1 ? cfg.afSteps[1] : cfg.afSteps[0], r0, p0);

                    var csv = new StringBuilder("T,step,dir,t_ms,pos_deg,vel_dps,load,volt_V,temp_C\n");
                    var rep = new StringBuilder();
                    int k = 0;

                    // 한발 모드: 반대발을 접어 지지다리에 전체중 → 지지쪽 3관절 측정, 좌우 교대
                    int[][] anGroups = (cfg.bStanding && cfg.bOneFoot)
                        ? new int[][] { new int[] { 6, 15, 9 }, new int[] { 8, 16, 11 } }
                        : new int[][] { cfg.anLegT };
                    int nTotal = 0; foreach (var g in anGroups) nTotal += g.Length;

                    for (int nG = 0; nG < anGroups.Length; nG++)
                    {
                    // 한발 모드는 시행 단위로 접었다 편다 — 넘어지면 발이 자동으로
                    // 펴진 상태에서 양발 차렷으로 다시 세우기만 하면 재개된다.
                    if (cfg.bStanding && cfg.bOneFoot && log != null)
                        log(nG == 0
                            ? "── 오른다리(전체중) 그룹: T6/T15/T9 — 시행마다 왼발을 접었다 폅니다"
                            : "── 왼다리(전체중) 그룹: T8/T16/T11 — 시행마다 오른발을 접었다 폅니다");
                    foreach (int nT in anGroups[nG])
                    {
                        if (stop != null && stop()) { if (log != null) log("중단"); return false; }
                        k++;
                        int id = nT + cfg.nIdOffset;
                        int nModel = Read(com, id, 0, 2, 0);
                        float fStall = StallOf(nModel);
                        CLimit lim; mech.TryGetValue(nT, out lim);

                        sbJ.AppendFormat(CultureInfo.InvariantCulture,
                            "  \"T{0}\": {{ \"servo_id\": {1}, \"model\": {2}, \"model_name\": \"{3}\", \"stall_Nm\": {4:F2}, \"stall_ref_V\": {5:F1}",
                            nT, id, nModel, ModelNameOf(nModel) ?? "unknown", fStall, StallRefVoltOf(nModel));
                        var repLine = new StringBuilder(string.Format("[{0}/{1}] T{2,-3}", k, nTotal, nT));

                        foreach (float fStep in cfg.afSteps)
                        {
                            for (int d = 0; d < 2; d++)
                            {
                                float fSg = (d == 0) ? +1f : -1f;
                                string strKey = string.Format(CultureInfo.InvariantCulture, "s{0:F0}_{1}", fStep, d == 0 ? "pos" : "neg");
                                float fTgt = fSg * fStep;
                                if (lim != null && lim.q != "no_limit" && lim.q != "measure_failed")
                                { fTgt = Math.Max(lim.lo + 3f, Math.Min(lim.hi - 3f, fTgt)); }
                                if (Math.Abs(fTgt) < cfg.fMinStroke)
                                {
                                    sbJ.AppendFormat(CultureInfo.InvariantCulture, ", \"{0}\": null", strKey);
                                    continue;
                                }

                                if (cfg.bStanding)
                                {
                                    if (cfg.bOneFoot)   // 발 펴기 — 양발 차렷으로 세울 수 있게
                                        LiftLeg(com, cfg, mech, signs, nG == 0 ? 8 : 6, nG == 0 ? 16 : 15, false);
                                    if (!WaitStance(com, cfg, r0, p0, log, stop)) return false;
                                    if (cfg.bOneFoot)
                                    {
                                        if (log != null) log(string.Format("  {0}발 접기 — 체중은 지지다리에, 잡을 준비!",
                                            nG == 0 ? "왼" : "오른"));
                                        LiftLeg(com, cfg, mech, signs, nG == 0 ? 8 : 6, nG == 0 ? 16 : 15, true);
                                    }
                                    if (log != null) log(string.Format("  다음: T{0} {1}{2:F0}° 스텝 — 1초 뒤!",
                                        nT, fSg > 0 ? "+" : "-", fStep));
                                    Thread.Sleep(1000);
                                }

                                com.Send(id, 3, 112, (byte)0, (byte)0, (byte)0, (byte)0); Thread.Sleep(5);
                                com.Command_Clear(); com.Command_Set(id, fTgt);
                                com.Move_NoWait(0, 0);
                                var vels = new List<float>(); var loads = new List<int>(); var volts = new List<float>();
                                var temps = new List<int>();
                                long l0 = Environment.TickCount;
                                while (Environment.TickCount - l0 < cfg.nCapMs)
                                {
                                    // 126~146 한 방: load(126)/vel(128)/pos(132)/★순간전압(144, 회신10 ②)
                                    //   ★2026-09-03: 20→21 바이트로 늘려 **온도(146)** 를 같은 타임스탬프에 담는다.
                                    //     회신 13 §2-2 / 회신 20 §4-3 — 전압과 발열이 같이 움직여서, 같이 안 받으면
                                    //     저전압 대역의 이름을 잘못 붙인다. 1바이트라 샘플링 비용은 사실상 0.
                                    com.SyncRead_With_Address(126, 21, id); Thread.Sleep(2);
                                    float fVel = Math.Abs(com.GetMap_Int(id, 128) * VEL_UNIT);
                                    int nLoad = (short)com.GetMap_Short(id, 126);
                                    float fPos = Deg(com.GetMap_Int(id, 132));
                                    float fVolt = (com.GetMap_Short(id, 144) & 0xFFFF) * 0.1f;
                                    int nTemp = com.GetMap(id, 146);
                                    vels.Add(fVel); loads.Add(Math.Abs(nLoad)); volts.Add(fVolt); temps.Add(nTemp);
                                    csv.AppendFormat(CultureInfo.InvariantCulture, "{0},{1:F0},{2},{3},{4:F2},{5:F1},{6},{7:F1},{8}\n",
                                        nT, fStep, fSg > 0 ? "+" : "-", Environment.TickCount - l0, fPos, fVel, nLoad, fVolt, nTemp);
                                    if (Math.Abs(fPos - fTgt) < 2f) break;
                                }
                                // ★셧다운 감지 — Hardware Error(addr 70). 샘플마다 읽으면 대역폭을 먹으므로
                                //   트라이얼 끝에 한 번만. 저전압 하한 지점에서 0 이 아니면 "약해진 것"이 아니라
                                //   "꺼진 것"이고, 그 트라이얼의 슬루·하중은 물리값이 아니다.
                                int nHwErr = Read(com, id, 70, 2, 0) & 0xFF;

                                float fMax = 0;
                                for (int i = 1; i + 1 < vels.Count; i++)
                                {
                                    float a = vels[i - 1], b = vels[i], c = vels[i + 1];
                                    fMax = Math.Max(fMax, Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c)));
                                }
                                var plat = new List<int>();
                                for (int i = 0; i < vels.Count; i++) if (vels[i] >= 0.7f * fMax) plat.Add(loads[i]);
                                plat.Sort();
                                float fLoad = (plat.Count > 0) ? plat[plat.Count / 2] * 0.001f : 0f;
                                float fVmin = 99f;
                                foreach (float v in volts) if (v > 0.5f && v < fVmin) fVmin = v;
                                if (fVmin > 98f) fVmin = 0f;
                                int nTmax = 0;
                                foreach (int t in temps) if (t > nTmax && t < 120) nTmax = t;

                                // [vmax_dps, load_pu, target_deg, vmin_V, tmax_C, hw_err]
                                sbJ.AppendFormat(CultureInfo.InvariantCulture,
                                    ", \"{0}\": [{1:F0}, {2:F3}, {3:F0}, {4:F1}, {5}, {6}]", strKey, fMax, fLoad, fTgt, fVmin, nTmax, nHwErr);
                                repLine.AppendFormat("  {0}{1:F0}°:{2,3:F0}dps/{3:F2}/{4:F1}V/{5}°C{6}",
                                    fSg > 0 ? "+" : "-", fStep, fMax, fLoad, fVmin, nTmax,
                                    nHwErr != 0 ? "/★HW" + nHwErr : "");
                                if (nHwErr != 0 && log != null)
                                    log(string.Format("  ★T{0} Hardware Error 0x{1:X2} — 저전압 셧다운 의심, 이 트라이얼 값은 물리값이 아닙니다", nT, nHwErr));

                                // 차렷 복귀 (다음 시행은 WaitStance 게이트가 지킨다)
                                com.Command_Clear(); com.Command_Set(id, 0f);
                                com.Move_NoWait(900, 0); Thread.Sleep(1300);
                                if (stop != null && stop()) { if (log != null) log("중단"); return false; }
                            }
                        }
                        sbJ.AppendLine(" },");
                        rep.AppendLine(repLine.ToString());
                        if (log != null) log(repLine.ToString());
                    }
                    if (cfg.bStanding && cfg.bOneFoot)
                    {
                        if (log != null) log("접은 발을 폅니다 — 잡아 주세요");
                        LiftLeg(com, cfg, mech, signs, nG == 0 ? 8 : 6, nG == 0 ? 16 : 15, false);
                    }
                    }
                    sbJ.AppendFormat(CultureInfo.InvariantCulture, "  \"voltage_end_V\": {0:F1}\r\n}}\r\n", VoltOf(com, cfg.anIDs[0]));
                    Directory.CreateDirectory(strDir);
                    File.WriteAllText(Path.Combine(strDir, "load_vmax_" + strPost + ".json"), sbJ.ToString(), new UTF8Encoding(false));
                    File.WriteAllText(Path.Combine(strDir, "load_vmax_" + strPost + "_raw.csv"), csv.ToString(), new UTF8Encoding(false));

                    com.Command_Clear();
                    foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                    com.Move_NoWait(1500, 0); Thread.Sleep(1800);
                    strReport = rep.ToString() + "\r\n-> load_vmax_" + strPost + ".json / _raw.csv";
                    return true;
                }
                finally { com.Close(); }
            }

            //=========================================================
            // ⑥ 전도경계 (기립) — tiptest_pitch.json
            //
            //   좌우(롤 평행사변형) + 전후(사지탈 평행사변형) 를 차렷에서 잰다.
            //   0.5도씩 키우며 전도 개시를 잡아 즉시 후퇴 (실제로 넘기지 않음).
            //   ★결과에서 유효 접지면을 역산해 $footcontact 제안값을 만든다:
            //     반폭 = h_COM × tan(θ_좌우),  앞뒤 = h_COM × tan(θ) 로 전/후 각각,
            //     반길이 = (앞+뒤)/2,  중심 오프셋 = (뒤−앞)/2 (발목 뒤쪽 +).
            //=========================================================
            public class CTipCfg : CStandCfg
            {
                public float fCapRoll = 24f;
                public float fCapPitchF = 20f;        // 전방 (실측 16.5 부근)
                public float fCapPitchB = 30f;        // 후방 (실측 26.5 부근)
                public float fComHeight = 0.20f;      // COM 높이 [m] — footcontact 역산용
            }

            private static float TipProbe(Ojw.CProtocol com, CTipCfg cfg, Dictionary<int, CLimit> mech,
                                          Dictionary<int, float> pat, float fSign, float fCap, bool bPitch,
                                          float fRoll0, float fPitch0, DLog log, DIsStopped stop,
                                          StringBuilder csv, string strTag)
            {
                string strClamp;
                int nHit = 0; float fPrev = 0;
                for (float L = 0; L <= fCap + 0.01f; L += 0.5f)
                {
                    if (stop != null && stop()) return float.NaN;
                    // 기구한계 선검사 (R9)
                    foreach (var kv in pat)
                    {
                        CLimit lim;
                        if (!mech.TryGetValue(kv.Key, out lim) || lim == null || lim.q == "no_limit") continue;
                        float v = kv.Value * fSign * L;
                        if (v < lim.lo + 1.5f || v > lim.hi - 1.5f)
                        {
                            if (log != null) log(string.Format("  ■ T{0} 기구한계 — lean {1:F1} 정지 (전도 아님)", kv.Key, fSign * L));
                            ApplyPat(com, cfg, mech, pat, 0f, 900, out strClamp); Thread.Sleep(1400);
                            return Math.Max(0f, L - 0.5f);
                        }
                    }
                    ApplyPat(com, cfg, mech, pat, fSign * L, cfg.nMoveMs, out strClamp);
                    long lEnd = Environment.TickCount + cfg.nMoveMs + cfg.nSettleMs;
                    while (Environment.TickCount < lEnd)
                    {
                        float rr, pp, g1, g2; Imu(com, cfg.nImuId, out rr, out pp, out g1, out g2);
                        float gAx = bPitch ? g2 : g1;
                        if (Math.Abs(gAx) > 25f || Math.Abs((bPitch ? pp - fPitch0 : rr - fRoll0)) > (bPitch ? 9f : 6f))
                        {
                            if (log != null) log(string.Format("  ★급속 전도 (lean {0:F1}) — 즉시 후퇴", fSign * L));
                            ApplyPat(com, cfg, mech, pat, 0f, 500, out strClamp); Thread.Sleep(900);
                            return Math.Max(0f, L - 0.5f);
                        }
                        Thread.Sleep(25);
                    }
                    float r, p, gr, gp; Imu(com, cfg.nImuId, out r, out p, out gr, out gp);
                    float d = bPitch ? (p - fPitch0) : (r - fRoll0);
                    float g0 = bPitch ? gp : gr;
                    csv.AppendFormat(CultureInfo.InvariantCulture, "{0},{1:F1},{2:F2},{3:F2},{4:F1},{5:F1}\n",
                                     strTag, fSign * L, r, p, gr, gp);
                    if (log != null) log(string.Format("  lean {0,5:F1}  d{1} {2,6:F2}  gyro {3,5:F1}",
                                                       fSign * L, bPitch ? "P" : "R", d, g0));
                    // 축별 문턱: 앞뒤는 평탄·잡음 커서 '커지는 중'일 때만 (TipTest 실측 교훈)
                    bool bRising = Math.Abs(d) > Math.Abs(fPrev) + 0.15f;
                    bool bTip = Math.Abs(g0) > 8f
                                || (Math.Abs(d) > (bPitch ? 4.5f : 2.5f) && (bRising || !bPitch))
                                || (Math.Abs(d) > 1.2f && Math.Abs(d) > Math.Abs(fPrev) + 0.5f);
                    fPrev = d;
                    if (bTip) nHit++; else nHit = 0;
                    if (nHit >= 2)
                    {
                        if (log != null) log(string.Format("  ★전도 개시 (lean {0:F1}) — 후퇴", fSign * L));
                        ApplyPat(com, cfg, mech, pat, 0f, 600, out strClamp); Thread.Sleep(1000);
                        return Math.Max(0f, L - 0.5f);
                    }
                }
                if (log != null) log(string.Format("  상한 {0:F0} 까지 전도 없음", fCap));
                ApplyPat(com, cfg, mech, pat, 0f, 1200, out strClamp); Thread.Sleep(1600);
                return float.NaN;
            }

            public static bool TipBoundary(CTipCfg cfg, string strDir, DLog log, DIsStopped stop,
                                           out string strReport)
            {
                strReport = "";
                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    var mech = LoadMechLimits(Path.Combine(strDir, "mech_limits.json"));
                    if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("정렬 실패 — 중단"); return false; }

                    // 기준선 + 자세 게이트
                    float sr = 0, sp = 0; int n = 0;
                    long lEnd0 = Environment.TickCount + 1500;
                    while (Environment.TickCount < lEnd0)
                    { float r, p, g1, g2; Imu(com, cfg.nImuId, out r, out p, out g1, out g2); sr += r; sp += p; n++; Thread.Sleep(40); }
                    float fR0 = sr / n, fP0 = sp / n;
                    if (log != null) log(string.Format("차렷 기준 roll {0:F2} pitch {1:F2}", fR0, fP0));
                    if (Math.Abs(fP0) > 5.5f || Math.Abs(fR0) > 4.0f)
                    { if (log != null) log("★반듯한 기립이 아닙니다 — 고쳐 세운 뒤 재실행"); return false; }

                    var csv = new StringBuilder("dir,lean,roll,pitch,gyroR,gyroP\n");
                    var patRoll = new Dictionary<int, float> { { 5, -1f }, { 7, -1f }, { 10, -1f }, { 12, -1f } };
                    var patPitch = new Dictionary<int, float> { { 9, +1f }, { 11, +1f }, { 6, -1f }, { 8, -1f } };

                    if (log != null) log("── 좌우(롤) ──");
                    float tR = TipProbe(com, cfg, mech, patRoll, +1f, cfg.fCapRoll, false, fR0, fP0, log, stop, csv, "R+");
                    Thread.Sleep(1500);
                    float tL = TipProbe(com, cfg, mech, patRoll, -1f, cfg.fCapRoll, false, fR0, fP0, log, stop, csv, "R-");
                    Thread.Sleep(1500);
                    if (log != null) log("── 전후(피치) ──");
                    float tF = TipProbe(com, cfg, mech, patPitch, +1f, cfg.fCapPitchF, true, fR0, fP0, log, stop, csv, "F");
                    Thread.Sleep(1500);
                    float tB = TipProbe(com, cfg, mech, patPitch, -1f, cfg.fCapPitchB, true, fR0, fP0, log, stop, csv, "B");

                    com.Command_Clear();
                    foreach (int id in cfg.anIDs) com.Command_Set(id, 0f);
                    com.Move_NoWait(1500, 0); Thread.Sleep(1800);

                    Directory.CreateDirectory(strDir);
                    File.WriteAllText(Path.Combine(strDir, "tiptest_raw.csv"), csv.ToString(), new UTF8Encoding(false));

                    // ── $footcontact 역산 ────────────────────────────
                    float h = cfg.fComHeight;
                    float fLat = float.IsNaN(tR) || float.IsNaN(tL) ? float.NaN
                               : h * 1000f * (float)Math.Tan(Math.Min(tR, tL) * Math.PI / 180.0);
                    float fFront = float.IsNaN(tF) ? float.NaN : h * 1000f * (float)Math.Tan(tF * Math.PI / 180.0);
                    float fBack = float.IsNaN(tB) ? float.NaN : h * 1000f * (float)Math.Tan(tB * Math.PI / 180.0);

                    var sb = new StringBuilder();
                    sb.AppendLine("전도경계 (차렷, deg):");
                    sb.AppendFormat("  좌우 {0} / {1}   전방 {2}   후방 {3}\r\n",
                        float.IsNaN(tR) ? ">" + cfg.fCapRoll : tR.ToString("F1"),
                        float.IsNaN(tL) ? ">" + cfg.fCapRoll : tL.ToString("F1"),
                        float.IsNaN(tF) ? ">" + cfg.fCapPitchF : tF.ToString("F1"),
                        float.IsNaN(tB) ? ">" + cfg.fCapPitchB : tB.ToString("F1"));
                    if (!float.IsNaN(fLat) && !float.IsNaN(fFront) && !float.IsNaN(fBack))
                    {
                        float fHalfX = (fFront + fBack) / 2f;
                        float fOffX = -(fBack - fFront) / 2f;      // 뒤가 넓으면 중심은 뒤(−x)
                        sb.AppendLine();
                        sb.AppendFormat("유효 접지면 역산 (COM 높이 {0:F0}mm 기준):\r\n", h * 1000f);
                        sb.AppendFormat("  반폭 {0:F0}mm  반길이 {1:F0}mm  중심 x {2:F0}mm\r\n", fLat, fHalfX, fOffX);
                        sb.AppendLine("  → EDH 의 발 링크 DH 라인 '바로 뒤'에 넣으십시오 (양발 각각):");
                        sb.AppendFormat("  $footcontact, 2.5, {0:F0}, {1:F0}, <발바닥z>, 0, {2:F0}\r\n",
                                        fLat, fHalfX, fOffX);
                    }

                    // json (병합 없이 단순 기록 — 자세 확장은 파일 누적으로)
                    var sbJ = new StringBuilder();
                    sbJ.AppendLine("{");
                    sbJ.AppendLine("  \"note\": \"MakeUrdf ⑥ 전도경계 (차렷). lean = 관절 명령각. tiptest_raw.csv 에 곡선.\",");
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"attention\": {{ \"roll_right\": {0}, \"roll_left\": {1}, \"forward\": {2}, \"backward\": {3} }},\r\n",
                        float.IsNaN(tR) ? "null" : tR.ToString("F1", CultureInfo.InvariantCulture),
                        float.IsNaN(tL) ? "null" : tL.ToString("F1", CultureInfo.InvariantCulture),
                        float.IsNaN(tF) ? "null" : tF.ToString("F1", CultureInfo.InvariantCulture),
                        float.IsNaN(tB) ? "null" : tB.ToString("F1", CultureInfo.InvariantCulture));
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"imu_zero\": {{ \"roll\": {0:F2}, \"pitch\": {1:F2} }},\r\n", fR0, fP0);
                    sbJ.AppendFormat(CultureInfo.InvariantCulture, "  \"com_height_m\": {0:F3}\r\n}}\r\n", h);
                    File.WriteAllText(Path.Combine(strDir, "tiptest_pitch.json"), sbJ.ToString(), new UTF8Encoding(false));

                    strReport = sb.ToString() + "\r\n-> tiptest_pitch.json / tiptest_raw.csv";
                    if (log != null) { log(""); log(strReport); }
                    return true;
                }
                finally { com.Close(); }
            }

            //=========================================================
            // ⑦ 전원 프로파일 (기립) — power_profile.json
            //   웅크림 펌프(보행급 부하)로 버스/다리말단 전압 강하를 잰다.
            //=========================================================
            public static bool PowerProfile(CStandCfg cfg, string strDir, int nCycles,
                                            DLog log, DIsStopped stop, out string strReport)
            {
                strReport = "";
                Dictionary<int, float> dicCur;
                var com = Open(cfg, log, out dicCur);
                if (com == null) return false;
                try
                {
                    if (!HomeAll(com, cfg, dicCur, log)) { if (log != null) log("정렬 실패 — 중단"); return false; }
                    int idBus = cfg.anIDs[0];
                    int idLeg = (Array.IndexOf(cfg.anIDs, 9 + cfg.nIdOffset) >= 0) ? 9 + cfg.nIdOffset : cfg.anIDs[cfg.anIDs.Length - 1];
                    var crouch = new Dictionary<int, float> {
                        { 6, -14.32f }, { 8, -14.32f }, { 15, 28.65f }, { 16, -28.65f },
                        { 9, -14.32f }, { 11, -14.32f } };

                    Action<Dictionary<int, float>, int> pose = delegate(Dictionary<int, float> p, int ms)
                    {
                        com.Command_Clear();
                        foreach (int id in cfg.anIDs)
                            com.Command_Set(id, (p != null && p.ContainsKey(id - cfg.nIdOffset)) ? p[id - cfg.nIdOffset] : 0f);
                        com.Move_NoWait(ms, 0);
                    };

                    float vIdle = 99, vLoad = 99, vLegIdle = 99, vLegLoad = 99;
                    var csv = new StringBuilder("t_ms,phase,volt_bus,volt_leg\n");
                    long l0 = Environment.TickCount;
                    Action<string, bool> rec = delegate(string ph, bool bLoad)
                    {
                        float v1 = VoltOf(com, idBus), v9 = VoltOf(com, idLeg);
                        csv.AppendFormat(CultureInfo.InvariantCulture, "{0},{1},{2:F2},{3:F2}\n",
                                         Environment.TickCount - l0, ph, v1, v9);
                        if (bLoad) { vLoad = Math.Min(vLoad, v1); vLegLoad = Math.Min(vLegLoad, v9); }
                        else { vIdle = Math.Min(vIdle, v1); vLegIdle = Math.Min(vLegIdle, v9); }
                    };

                    if (log != null) log("무부하 기준선 6s ...");
                    long lEnd = Environment.TickCount + 6000;
                    while (Environment.TickCount < lEnd) { rec("idle", false); Thread.Sleep(90); }
                    if (log != null) log(string.Format("웅크림 펌프 {0}회 ...", nCycles));
                    for (int c = 1; c <= nCycles; c++)
                    {
                        if (stop != null && stop()) break;
                        pose(crouch, 500);
                        lEnd = Environment.TickCount + 600;
                        while (Environment.TickCount < lEnd) { rec("down", true); Thread.Sleep(70); }
                        pose(null, 500);
                        lEnd = Environment.TickCount + 600;
                        while (Environment.TickCount < lEnd) { rec("up", true); Thread.Sleep(70); }
                        float rr, pp, g1, g2; Imu(com, cfg.nImuId, out rr, out pp, out g1, out g2);
                        if (Math.Abs(rr) > 15f || Math.Abs(pp) > 15f)
                        { if (log != null) log("★tilt 이탈 — 중단"); break; }
                        if (c % 5 == 0 && log != null) log(string.Format("  [{0}/{1}]", c, nCycles));
                    }
                    pose(null, 1200); Thread.Sleep(1500);

                    Directory.CreateDirectory(strDir);
                    File.WriteAllText(Path.Combine(strDir, "power_profile.csv"), csv.ToString(), new UTF8Encoding(false));
                    var sbJ = new StringBuilder();
                    sbJ.AppendLine("{");
                    sbJ.AppendLine("  \"note\": \"MakeUrdf ⑦ 전원 프로파일. 웅크림 펌프 = 보행급 부하. min 전압 [V].\",");
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"idle_min\": {{ \"bus\": {0:F2}, \"leg_end\": {1:F2} }},\r\n", vIdle, vLegIdle);
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"load_min\": {{ \"bus\": {0:F2}, \"leg_end\": {1:F2} }},\r\n", vLoad, vLegLoad);
                    sbJ.AppendFormat(CultureInfo.InvariantCulture,
                        "  \"sag_bus_V\": {0:F2}, \"sag_leg_V\": {1:F2}\r\n}}\r\n",
                        vIdle - vLoad, vLegIdle - vLegLoad);
                    File.WriteAllText(Path.Combine(strDir, "power_profile.json"), sbJ.ToString(), new UTF8Encoding(false));

                    strReport = string.Format(
                        "무부하 {0:F2}/{1:F2} -> 부하 최저 {2:F2}/{3:F2} (버스/다리말단)\r\n강하 {4:F2}/{5:F2} V\r\n-> power_profile.json",
                        vIdle, vLegIdle, vLoad, vLegLoad, vIdle - vLoad, vLegIdle - vLegLoad);
                    if (log != null) { log(""); log(strReport); }
                    return true;
                }
                finally { com.Close(); }
            }
        }

        /// <summary>교육 실행기용 장면·실물 브리지 (2026-08-26) — 파이썬(pythonnet)에 'scene' 으로
        /// 노출되는 클래스. 화면의 3D(c3d)를 건네고, 실물 로봇 통신을 CProtocol 위에 현장 절차로
        /// 묶어 제공한다: 포트 사전 확인 → 프로토콜 2.0 설정 → 토크 실측 확인(스테일 재읽기) →
        /// 정착 재시도 일괄 읽기(CSim2Real.ReadAll) → 3D 동기화 → 모션 스텝마다 스트리밍(SendPose: 렌더 틱 20ms /
        /// 자가 펌프 ≈26ms, 실제 틱 간격을 실측해 Profile Velocity 를 정한다 — CProtocol.Move_Stream).
        /// ※ 메서드명이 소문자인 이유 = 파이썬 예제 표기(scene.open/torqon/syncread/close) 그대로
        /// ※ 모터 세팅(SetMotors, COjw_44_ArmMotor.cs)을 붙이면 방향·영점·틱/회전·리밋을 그 세팅으로 해석한다 (부호 배열은 무시).
        ///   붙이지 않으면 예전 경로 그대로 — 버스로 나가는 명령과 읽는 값이 같다 (파이썬 실행기·CreateOmx 사용자)</summary>
        public class CScene_t
        {
            private readonly C3d m_C3d;
            private readonly int[] m_anIDs;
            private readonly int[] m_anSign;      // 시뮬(DH)각 → 실물 모터각 부호 (현장 보정용)
            private CProtocol m_Com = null;       // null = 시뮬레이션 전용
            private bool m_bTorque = false;
            // ── 모터 세팅 경로 (2026-10-02) — 세팅을 붙이지 않으면 m_aMot = null 이고 예전 경로를 탄다 ──
            private volatile CArmMotor_t[] m_aMot = null;   // IDs 순 복제본. null = 예전 경로(부호 배열 + 2048 기준)
            private float[] m_afHoldSim = null;              // 실물이 붙잡고 있는 3D 자세(보정 얹기 전) — 토크 ON 정렬·SendPose 마다 갱신
            private float[] m_afGoalSim = null;              // 실물에 걸린 목표(3D 각, 보정·리밋 반영) — 토크 ON 정렬 직후는 정렬 자세
            private readonly Action<int> m_Sink;             // 생성자가 등록한 싱크 (Release 비교용)
            private readonly Action<int> m_PrevSink;         // 그 전에 걸려 있던 싱크 — Release 가 되돌린다
            private bool m_bReleased = false;
            private bool m_bWarnUnaligned = false;           // "자세를 몰라 보내지 않음" 로그를 한 번만

            /// <summary>c3dRef = 화면의 3D, anMotorIDs = 실물 모터 ID 목록, anSign = 관절별 부호(+1/-1)</summary>
            public CScene_t(C3d c3dRef, int[] anMotorIDs, int[] anSign)
            {
                m_C3d = c3dRef;
                m_anIDs = (int[])anMotorIDs.Clone();
                m_anSign = (int[])anSign.Clone();
                // 모션 스텝마다 실물 스트리밍 — 렌더 틱이든 자가 펌프든 스텝 시점에 전송된다
                m_PrevSink = m_C3d.MoveJoints_GetMotionSink();   // Release 가 되돌릴 이전 싱크
                m_Sink = SendPose;
                m_C3d.MoveJoints_SetMotionSink(m_Sink);
            }

            public C3d c3d { get { return m_C3d; } }
            public bool IsConnected { get { return m_Com != null; } }
            public bool IsTorqueOn { get { return m_Com != null && m_bTorque; } }

            /// <summary>OMX Follower 모델을 내장 생성 — 3D 창 없이(VS Code 등 단독 실행) 기구학·
            /// 모션(자가 펌프)·실물 통신까지 실행기와 동일하게 동작한다. 예제 상단의
            /// 'scene = Ojw.CScene_t.CreateOmx()' 폴백이 이 함수를 쓴다 (관절 전용 DH — 렌더 불필요)</summary>
            public static CScene_t CreateOmx()
            {
                var c3d = new C3d();
                c3d.m_CHeader.pstrKinematics[0] = string.Join("\r\n", new[]
                {
                    "// *OMX Follower (Joints Only)",
                    "[0,0,90,90],[-1,0,0]",
                    "[0,0,0,-90],[-1,0,0]",
                    "[0,13,0,0],[-1,0,0]",
                    "[0,40,0,0],[-1,0,0]",
                    "[0,0,0,0],[11,0,0] // - Axis11",
                    "[0,0,90,0],[-1,0,0]",
                    "[0,0,0,90],[-1,0,0]",
                    "[0,0,90,0],[-1,0,0]",
                    "[44.5,0,0,0],[-1,0,0]",
                    "[0,0,0,0],[12,0,0] // - Axis12",
                    "[113.15,0,0,0],[-1,0,0]",
                    "[0,0,90,0],[-1,0,0]",
                    "[41.5,0,0,0],[-1,0,0]",
                    "[0,0,0,0],[13,0,0] // - Axis13",
                    "[162,0,0,0],[-1,0,0]",
                    "[0,0,0,0],[14,0,0] // - Axis14",
                    "[43.2,0,0,0],[-1,0,0]",
                    "[0,0,90,90],[-1,0,0]",
                    "[0,60,0,0],[-1,0,0],[2,0]",
                    "!",
                    "[0,-60,0,0],[-1,0,0]",
                    "[0,0,0,0],[15,0,0] // - Axis15",
                    "[0,15,-90,90],[-1,0,0]",
                    "[-7.5,0,0,0],[-1,0,0]",
                    "[0,0,90,0],[16,0,0] // - Axis16",
                    "[60,0,0,0],[-1,0,0]",
                    "[-60,0,0,0],[-1,0,0]",
                    "[0,0,0,0],[16,1,0] // - Axis16",
                    "[7.5,0,-90,0],[-1,0,0]",
                    "[10.80,0,0,0],[-1,0,0]",
                    "[0,0,90,0],[16,1,0] // - Axis16",
                    "[60,0,0,0],[-1,0,0]",
                    "[-60,0,0,0],[-1,0,0]",
                    "[0,0,0,0],[16,0,0] // - Axis16",
                    "[10.8,0,90,90],[-1,0,0]",
                });
                c3d.CheckForward();
                return new CScene_t(c3d, new[] { 11, 12, 13, 14, 15, 16 }, new[] { 1, 1, 1, 1, 1, 1 });
            }

            /// <summary>로그 — 앱에서는 등록된 로그 창(printf), 단독 실행에서는 콘솔에도 출력</summary>
            private static void Log(string strFmt, params object[] aArgs)
            {
                string s = aArgs.Length > 0 ? string.Format(strFmt, aArgs) : strFmt;
                Ojw.printf("{0}", s);
                try { Console.Write(s); } catch { }
            }

            public void open(int nPort = 4, int nBaud = 1000000)
            {
                close();
                // 포트 존재를 먼저 확인 — 없는 포트로 Open 하면 내부가 에러 덤프를 쏟아낸다
                string[] astrPorts = System.IO.Ports.SerialPort.GetPortNames();
                if (Array.FindIndex(astrPorts, s => s.Equals("COM" + nPort, StringComparison.OrdinalIgnoreCase)) < 0)
                {
                    Log("[COMM] COM{0} 포트가 없습니다 — 현재 연결된 포트: {1}\r\n",
                               nPort, astrPorts.Length > 0 ? string.Join(", ", astrPorts) : "(없음)");
                    Log("[COMM] 시뮬레이션만 계속합니다\r\n");
                    return;
                }
                var com = new CProtocol();
                if (!com.Open(nPort, nBaud))
                {
                    Log("[COMM] COM{0} 열기 실패(다른 프로그램 사용 중?) — 시뮬레이션만 계속합니다\r\n", nPort);
                    return;
                }
                CArmMotor_t[] aMot = m_aMot;
                for (int i = 0; i < m_anIDs.Length; i++)
                {
                    // XL-430 = 프로토콜 2.0. 모터 세팅이 붙어 있으면 그 실물 ID (지금은 축 번호와 같다 — SetMotors 가 확인).
                    // CProtocol 은 전송층 기본값(X 프리셋·방향 정·리밋 0)으로만 둔다 — 방향·영점·리밋은 CScene_t 가 세팅으로 처리
                    int id = (aMot != null) ? aMot[i].nMotorID : m_anIDs[i];
                    com.SetParam(id); com.SetParam_Protocol(id, 2);
                }
                if (!float.IsNaN(m_fStreamEarly)) com.Stream_Early = m_fStreamEarly;              // 스트리밍 horizon 배율(설정했으면)
                m_Com = com;
                Log("[COMM] COM{0} 연결 ({1} bps) — torqon() 하면 실물이 함께 움직입니다\r\n", nPort, nBaud);
            }

            public void close()
            {
                if (m_Com == null) return;
                ProfileAccel_Restore();
                try { m_Com.Close(); } catch { }
                m_Com = null;
                m_bTorque = false;
                Log("[COMM] 연결 해제\r\n");
            }

            public void torqon() { Torque(true); }
            public void torqoff() { Torque(false); }

            /// <summary>torqon 과 같은데 붙잡은 자세를 모름으로 둔 채 켠다 (CArmConn_t.TorqueOn — 이미 토크를 붙잡은 서보나 토크 상태를 모르는 서보가 있어
            /// 곧 서보 목표로 이어받거나 움직이기 전 확인이 맞출 때). 3D 는 실물 자세로 정렬하고 그 자세(IDs 순 3D 각)를 돌려준다 (미연결·읽기 실패면 null).
            /// 켜고 확인 읽기를 마칠 때까지(약 170ms) 붙잡은 자세가 없어 SendPose 가 보내지 않는다 — 그사이 다른 스레드의 3D 모션이 처진 지금 위치를
            /// 붙잡고 있던 서보로 보내 팔이 처짐만큼 떨어지지 않게 (10-03 검토). 부른 쪽이 AdoptTorqueOnGoal·HoldFromRealGoal 로 붙잡은 자세를 정한다</summary>
            internal float[] TorqueOnUnheld()
            {
                float[] a;
                TorqueCore(true, true, out a);
                return a;
            }

            private void Torque(bool bOn)
            {
                float[] a;
                TorqueCore(bOn, false, out a);
            }
            // 토크 켜기·끄기 본체 — bUnheld 면 켤 때 정렬한 자세를 붙잡은 자세로 삼지 않고 afAligned 로 돌려준다 (TorqueOnUnheld)
            private void TorqueCore(bool bOn, bool bUnheld, out float[] afAligned)
            {
                afAligned = null;
                if (m_Com == null)
                {
                    Log("[SIM] 통신 미연결 — 토크 {0} 은 실물 연결(open) 후에 의미가 있습니다\r\n", bOn ? "ON" : "OFF");
                    return;
                }
                if (bOn)
                {
                    // 켜기 전에 실물 자세로 3D 를 먼저 정렬 — 토크 ON 순간의 점프 방지 (읽기에 실패해도 토크는 켠다 — 예전 그대로)
                    m_afHoldSim = null; m_afGoalSim = null;          // 정렬에 성공해야 다시 생긴다 (모르면 null)
                    afAligned = SyncFromReal();
                    if (afAligned != null && !bUnheld) { m_afHoldSim = afAligned; m_afGoalSim = (float[])afAligned.Clone(); }
                }
                if (bOn) m_C3d.SagComp_JointReset();   // 처짐 보상의 관절 이동 방향 추적을 실물 자세에서 새로 시작
                if (bOn) ProfileAccel_Apply(); else ProfileAccel_Restore();
                foreach (int id in m_anIDs)
                { m_Com.Send(id, 3, 64, (byte)(bOn ? 1 : 0)); System.Threading.Thread.Sleep(5); }   // addr 64 = Torque Enable
                m_bTorque = bOn;
                m_Com.Stream_Reset();   // 스트리밍 추정 위치를 방금 읽은 실물 자세(m_afMot_Pose)에서 다시 시작
                // 쓰기 직후의 첫 읽기는 스테일일 수 있어 두 번 읽고 실측으로 확인한다
                System.Threading.Thread.Sleep(100);
                m_Com.SyncRead_With_Address(64, 1, m_anIDs); System.Threading.Thread.Sleep(30);
                m_Com.SyncRead_With_Address(64, 1, m_anIDs); System.Threading.Thread.Sleep(30);
                var lstBad = new List<int>();
                foreach (int id in m_anIDs)
                    if ((m_Com.GetMap_Short(id, 64) & 0xFF) != (bOn ? 1 : 0)) lstBad.Add(id);
                if (lstBad.Count > 0)
                    Log("[COMM] 주의 — 토크 {0} 미확인 ID: {1} (전원·배선 점검)\r\n",
                               bOn ? "ON" : "OFF", string.Join(",", lstBad.ConvertAll(x => x.ToString()).ToArray()));
                else
                    Log("[COMM] 전 모터 토크 {0} 확인\r\n", bOn ? "ON" : "OFF");
            }

            // ── 재부팅 (오류 지우기, 10-16) ──
            private const int REBOOT_FIRST_MS = 200;        // 보낸 뒤 첫 확인까지 (재부팅 중에는 대답이 없다 — 일찍 물으면 대기시간 초과만 쌓인다)
            private const int REBOOT_WAIT_MS = 3000;        // 다시 대답하기를 기다리는 최대 시간 (보통 0.5~2초)
            /// <summary>전 서보 재부팅 (REBOOT 0x08 브로드캐스트 — CProtocol.Reboot) — 하드웨어 오류(주소 70: 과부하·과열·전압·엔코더 등, 서보가 스스로 토크를 끈 까닭)를 지운다.
            /// 오류가 없어도 된다. 재부팅하면 서보 토크가 꺼지고 RAM(토크·목표·Profile 등)이 기본값으로 돌아간다 — 팔이 떨어질 수 있으니 받친 뒤 부른다.
            /// 보낸 뒤 모델번호(주소 0 — 연결 때 응답 확인과 같은 읽기)로 장면의 전 ID 가 다시 대답할 때까지 기다린다 (최대 3초).
            /// 장면은 토크 OFF·붙잡은 자세 모름으로 하고 처짐 보정의 출력 단 상태를 비운다 (서보가 붙잡은 것이 없다). 다시 움직이려면 torqon().
            /// 반환 = 전 ID 가 다시 대답했다. 미연결·3D 모션 중이면 보내지 않고 false</summary>
            public bool reboot() { return RebootCore(-1) > 0; }
            /// <summary>reboot 와 같고 ID 하나만 (장면의 모터 ID — REBOOT 를 그 ID 로 보내고 그 ID 가 다시 대답할 때까지 기다린다).
            /// 장면은 토크 OFF 로 둔다 (다른 축은 서보가 붙잡은 그대로 — 다시 움직이려면 torqon())</summary>
            public bool reboot(int id) { return RebootCore(id) > 0; }
            /// <summary>재부팅 본체 — nId = -1 이면 전 서보 (브로드캐스트). 반환: -1 = 보내지 않음 (미연결·장면 밖 ID·3D 모션 중),
            /// 0 = 보냈는데 다시 대답하지 않은 ID 가 있음, 1 = 전부 다시 대답함 (CArmConn_t.Reboot 가 보냈는지를 가린다)</summary>
            internal int RebootCore(int nId)
            {
                if (m_Com == null)
                {
                    Log("[SIM] 통신 미연결 — 재부팅은 실물 연결(open) 후에 의미가 있습니다\r\n");
                    return -1;
                }
                if (nId >= 0 && Array.IndexOf(m_anIDs, nId) < 0)
                {
                    Log("[COMM] 재부팅 — ID {0} 는 이 장면의 모터가 아닙니다 (모터: {1})\r\n", nId, string.Join(",", Array.ConvertAll(m_anIDs, x => x.ToString())));
                    return -1;
                }
                if (m_C3d.MoveJoints_IsMoving())
                {
                    Log("[COMM] 재부팅 거부 — 3D 모션 중 (끝난 뒤 다시)\r\n");
                    return -1;
                }
                // 먼저 장면을 토크 OFF·붙잡은 자세 모름으로 — 재부팅하는 동안 SendPose 가 보내지 않게
                m_bTorque = false;
                m_afHoldSim = null; m_afGoalSim = null; m_afLastSent = null;
                m_Com.Reboot((nId < 0) ? 254 : nId);            // REBOOT 0x08 (프로토콜 2) — CProtocol 의 토크 표시(m_abMot)도 지운다
                int nT0 = Environment.TickCount;
                // 다시 대답할 때까지 — 모델번호 읽기 (응답 없는 ID 는 ReadReg 가 빼고 돌려준다)
                List<int> lstLeft = new List<int>((nId < 0) ? m_anIDs : new int[] { nId });
                System.Threading.Thread.Sleep(REBOOT_FIRST_MS);
                while (true)
                {
                    string w;
                    Dictionary<int, long> d = ReadReg(0, 2, out w);
                    if (d != null) lstLeft.RemoveAll(x => d.ContainsKey(x));
                    if (lstLeft.Count == 0 || unchecked(Environment.TickCount - nT0) >= REBOOT_WAIT_MS) break;
                    System.Threading.Thread.Sleep(100);
                }
                int nMs = unchecked(Environment.TickCount - nT0);
                ProfileAccel_Restore();                           // 토크 끄기와 같이 — 토크 ON 때 바꿔 둔 Profile Acceleration 을 원래 값으로 (안 바꿨으면 아무것도 안 함)
                m_C3d.SagComp_JointReset();                       // 서보에 걸린 처짐 보정이 없다 — 걸린 보정 없음에서 다시 (토크 켤 때도 다시 한다)
                m_Com.Stream_Reset();
                string strWho = (nId < 0) ? "전 모터" : "ID " + nId;
                if (lstLeft.Count == 0)
                    Log("[COMM] 재부팅 — {0} 다시 대답함 ({1:F1}초) · 토크 OFF — 다시 움직이려면 torqon()\r\n", strWho, nMs / 1000f);
                else
                    Log("[COMM] 주의 — 재부팅 뒤 대답 없는 ID: {0} ({1:F1}초 기다림 — 전원·배선 점검) · 토크 OFF\r\n",
                        string.Join(",", lstLeft.ConvertAll(x => x.ToString()).ToArray()), nMs / 1000f);
                return (lstLeft.Count == 0) ? 1 : 0;
            }

            public void syncread() { SyncFromReal(); }

            /// <summary>syncread 본체 — 실물 자세로 3D 를 맞추고 그 자세(IDs 순 3D 각, 3D 리밋으로 자르기 전)를 돌려준다.
            /// 미연결·읽기 실패면 null (3D 는 그대로). 모터 세팅이 붙어 있으면 틱을 세팅으로 해석한다</summary>
            private float[] SyncFromReal()
            {
                if (m_Com == null)
                {
                    Log("[SIM] SyncRead(시뮬) — 현재 관절각:\r\n");
                    foreach (int id in m_anIDs) Log("   ID {0}: {1:F1}도\r\n", id, m_C3d.GetData(id));
                    return null;
                }
                string strWhy;
                float[] afDeg = new float[m_anIDs.Length];
                CArmMotor_t[] aMot = m_aMot;
                if (aMot == null)
                {
                    var dicDeg = CSim2Real.ReadAll(m_Com, m_anIDs, out strWhy);   // 정착 재시도 포함 일괄 읽기
                    if (dicDeg == null) { Log("[COMM] SyncRead 실패: {0}\r\n", strWhy); return null; }
                    for (int i = 0; i < m_anIDs.Length; i++) afDeg[i] = dicDeg[m_anIDs[i]] * m_anSign[i];
                }
                else
                {
                    int[] anTick = ReadTicks(out strWhy);                           // 모터 세팅 경로 — 틱을 세팅으로 해석
                    if (anTick == null) { Log("[COMM] SyncRead 실패: {0}\r\n", strWhy); return null; }
                    for (int i = 0; i < m_anIDs.Length; i++) afDeg[i] = aMot[i].TickToSim(anTick[i]);
                }
                Log("[COMM] SyncRead — 실물 관절각:\r\n");
                for (int i = 0; i < m_anIDs.Length; i++)
                {
                    Log("   ID {0}: {1:F1}도\r\n", m_anIDs[i], afDeg[i]);
                    m_C3d.SetData(m_anIDs[i], afDeg[i]);   // 3D 를 실물 자세로 동기화
                }
                return afDeg;
            }

            /// <summary>실물 관절각 일괄 읽기 (부호 적용, 정착 재시도 포함) — 3D 는 건드리지 않는다.
            /// 처짐 측정(C3d.SagComp_Survey) 등 "명령 대 실측" 비교가 필요한 곳에서 쓴다. 실패 시 null 과 사유.</summary>
            public Dictionary<int, float> ReadDeg(out string strWhy)
            {
                strWhy = "";
                if (m_Com == null) { strWhy = "통신 미연결"; return null; }
                CArmMotor_t[] aMot = m_aMot;
                if (aMot != null)
                {
                    // 모터 세팅 경로 — 정착 읽은 틱을 세팅으로 해석
                    int[] anTick = ReadTicks(out strWhy);
                    if (anTick == null) return null;
                    var dicSim = new Dictionary<int, float>();
                    for (int i = 0; i < m_anIDs.Length; i++) dicSim[m_anIDs[i]] = aMot[i].TickToSim(anTick[i]);
                    return dicSim;
                }
                MarkPosition();                                                    // 예전 경로도 응답 없는 ID 의 지난 값을 쓰지 않게 (10-16)
                var dicDeg = CSim2Real.ReadAll(m_Com, m_anIDs, out strWhy);
                if (dicDeg == null) { EncWatch_Bad(ENC_STALE_N, false, strWhy); return null; }
                EncWatch_Fresh();
                var dicOut = new Dictionary<int, float>();
                for (int i = 0; i < m_anIDs.Length; i++) dicOut[m_anIDs[i]] = dicDeg[m_anIDs[i]] * m_anSign[i];
                return dicOut;
            }

            /// <summary>실물 관절각 빠른 읽기 — SyncRead 한 번(정착 재시도 없음, 부호 적용). 주행 중 표본화용.
            /// 스트림 직후의 첫 응답은 스테일일 수 있으니 호출자가 두 번 읽고 뒤 것을 쓴다. 실패 시 null.</summary>
            public Dictionary<int, float> ReadDegQuick(out string strWhy)
            {
                strWhy = "";
                if (m_Com == null) { strWhy = "통신 미연결"; return null; }
                CArmMotor_t[] aMot = m_aMot;
                if (aMot != null)
                {
                    // 모터 세팅 경로 — 응답 없는 ID 의 지난 값을 쓰지 않도록 수신 맵을 -1 로 채우고 읽는다
                    MarkPosition();
                    m_Com.SyncRead_With_Address(132, 4, m_anIDs); System.Threading.Thread.Sleep(12);
                    var dicSim = new Dictionary<int, float>();
                    for (int i = 0; i < m_anIDs.Length; i++)
                    {
                        int t = m_Com.GetMap_Int(m_anIDs[i], 132);
                        if (t == 0 || t == -1 || t < -8192 || t > 12288) { strWhy = "ID " + m_anIDs[i] + " 이상값"; EncWatch_Bad(1, false, strWhy); return null; }
                        dicSim[m_anIDs[i]] = aMot[i].TickToSim(t);
                    }
                    EncWatch_Fresh();
                    return dicSim;
                }
                MarkPosition();                                                    // 예전 경로도 응답 없는 ID 의 지난 값을 쓰지 않게 (10-16)
                m_Com.SyncRead_With_Address(132, 4, m_anIDs); System.Threading.Thread.Sleep(12);
                var dicOut = new Dictionary<int, float>();
                for (int i = 0; i < m_anIDs.Length; i++)
                {
                    int t = m_Com.GetMap_Int(m_anIDs[i], 132);
                    if (t == 0 || t == -1 || t < -8192 || t > 12288) { strWhy = "ID " + m_anIDs[i] + " 이상값"; EncWatch_Bad(1, false, strWhy); return null; }
                    dicOut[m_anIDs[i]] = CSim2Real.Deg(t) * m_anSign[i];
                }
                EncWatch_Fresh();
                return dicOut;
            }

            // ── 스트리밍 설정 (09-28 실측) ──
            // 떨림(부들거림)의 원인: Move_Stream 이 "다음 틱 전에 도착" 하도록 속도 한도를 주면 XL430 은 직사각형 속도 프로파일
            // (Profile Acceleration 0)로 달려 목표에 먼저 닿아 즉시 멈추고, 다음 틱에 다시 출발한다 → 초당 ~25회.
            // Stream_Early 를 1 보다 크게(기본 1.3, CProtocol 기본값) 주면 서보가 멈추기 전에 다음 목표가 온다.
            // Profile Acceleration 은 100·200 raw 로 시험했을 때 이득이 없어 기본은 건드리지 않는다(−1).
            private float m_fStreamEarly = float.NaN;                  // NaN = CProtocol 기본값(1.3) 그대로
            /// <summary>스트리밍 horizon 배율 (CProtocol.Stream_Early). 1 보다 크면 서보가 멈추기 전에 다음 목표가 온다.
            /// 연결 전에 설정해도 open() 때 적용된다.</summary>
            public float StreamEarly
            {
                get { return m_Com != null ? m_Com.Stream_Early : (float.IsNaN(m_fStreamEarly) ? 1.3f : m_fStreamEarly); }
                set { m_fStreamEarly = value; if (m_Com != null) m_Com.Stream_Early = value; }
            }
            /// <summary>토크 ON 때 서보 Profile Acceleration(주소 108, 1 raw = 214.577 rev/min² ≈ 21.5 deg/s²)에 쓸 값.
            /// −1(기본) = 서보 값을 건드리지 않는다. 쓰면 원래 값을 저장해 두었다가 토크 OFF·close 때 되돌린다
            /// (RAM 영역이라 전원을 끄면 원래대로 — 다른 프로그램에 남기지 않기 위해 되돌린다).</summary>
            public int ProfileAccelRaw = -1;
            private Dictionary<int, int> m_dicAccelSaved = null;

            private void ProfileAccel_Apply()
            {
                if (m_Com == null || ProfileAccelRaw < 0 || m_dicAccelSaved != null) return;
                try
                {
                    for (int r = 0; r < 2; r++) { m_Com.SyncRead_With_Address(108, 4, m_anIDs); System.Threading.Thread.Sleep(25); }   // 첫 응답은 스테일일 수 있다
                    var saved = new Dictionary<int, int>();
                    foreach (int id in m_anIDs) saved[id] = m_Com.GetMap_Int(id, 108);
                    byte[] b = BitConverter.GetBytes(ProfileAccelRaw);
                    foreach (int id in m_anIDs) { m_Com.Send(id, 3, 108, b); System.Threading.Thread.Sleep(5); }
                    m_dicAccelSaved = saved;
                    Log("[COMM] Profile Acceleration {0} raw 적용 (원래 값 저장 — 토크 OFF 때 복원)\r\n", ProfileAccelRaw);
                }
                catch (Exception ex) { Log("[COMM] Profile Acceleration 설정 실패: {0}\r\n", ex.Message); }
            }
            private void ProfileAccel_Restore()
            {
                if (m_Com == null || m_dicAccelSaved == null) return;
                try
                {
                    foreach (var kv in m_dicAccelSaved) { m_Com.Send(kv.Key, 3, 108, BitConverter.GetBytes(kv.Value)); System.Threading.Thread.Sleep(5); }
                    Log("[COMM] Profile Acceleration 원래 값으로 복원\r\n");
                }
                catch (Exception ex) { Log("[COMM] Profile Acceleration 복원 실패: {0}\r\n", ex.Message); }
                m_dicAccelSaved = null;
            }

            // ── 스트리밍·떨림 진단 (09-28) ──
            private float[] m_afLastSent = null;
            /// <summary>모터 ID 목록 (생성 때 준 순서)</summary>
            public int[] IDs { get { return (int[])m_anIDs.Clone(); } }
            /// <summary>직전 SendPose 가 실제로 보낸 각도 (IDs 순, 처짐 보정 포함, 부호 적용 전 — 모터 세팅 경로는 리밋까지 반영한 3D 각). 아직 없으면 null</summary>
            public float[] LastSentDeg { get { return m_afLastSent == null ? null : (float[])m_afLastSent.Clone(); } }
            /// <summary>직전 스트리밍 틱의 horizon(ms) — CProtocol.Move_Stream 이 정한 "다음 틱 도착" 시간</summary>
            public float StreamHorizonMs { get { return m_Com == null ? 0f : m_Com.Stream_LastHorizon_ms; } }

            /// <summary>서보 레지스터 112~135 (24바이트) 를 한 번의 SyncRead 로 읽는다 — 서보가 실제로 받은 값과 실제 상태.
            /// 반환 [IDs 순][8] = { Profile Velocity raw, Goal Position tick, Realtime Tick(ms), Moving | Moving Status&lt;&lt;8,
            ///                      Present PWM raw, Present Load raw, Present Velocity raw, Present Position tick } (부호 미적용 원시값).
            /// nWaitMs = 요청 후 응답을 기다리는 시간. 실패 시 null. (XL430/X 시리즈 프로토콜 2.0 컨트롤 테이블 기준)
            /// 10-16: 읽기 전에 수신 맵의 112~135 를 표지값으로 채운다 — 대답하지 않은 ID 가 있으면 null ("무응답 ID") (예전에는 수신 맵에 남은 지난 값을
            /// 그대로 돌려줘, 버스가 대답을 멈춘 [1. 학습] 이 몇 분 전 누름의 목표·부하와 시작 때 읽은 휴지 자세로 첫닿음을 판정했다).
            /// 전 축의 24바이트가 직전 읽기와 비트까지 같고 실시간 틱(주소 120 — 서보의 ms 시계)이 0 이 아니면 새로 읽은 값이 아니다 → null ("읽은 값이 그대로").
            /// 작업 중이면 엔코더 읽기 감시에 알린다 (EncoderTrip)</summary>
            public int[][] ReadServoBlock(int nWaitMs, out string strWhy)
            {
                strWhy = "";
                if (m_Com == null) { strWhy = "통신 미연결"; return null; }
                foreach (int id in m_anIDs) BlockMark(id);
                m_Com.SyncRead_With_Address(112, 24, m_anIDs);
                if (nWaitMs > 0) System.Threading.Thread.Sleep(nWaitMs);
                var res = new int[m_anIDs.Length][];
                var lstBad = new List<int>();
                byte[] raw = new byte[m_anIDs.Length * BLOCK_LEN];
                bool bClock = false;                                               // 실시간 틱이 흐르는 서보 (0 이 아닌 값) 가 있다
                for (int i = 0; i < m_anIDs.Length; i++)
                {
                    int id = m_anIDs[i];
                    if (m_Com.GetMap_Int(id, 132) == BLOCK_MARK_POS) { lstBad.Add(id); continue; }
                    int nBase = id * CProtocol._SIZE_MAP + BLOCK_ADDR;
                    if (nBase >= 0 && nBase + BLOCK_LEN <= m_Com.m_abyMap.Length) Array.Copy(m_Com.m_abyMap, nBase, raw, i * BLOCK_LEN, BLOCK_LEN);
                    res[i] = new int[]
                    {
                        m_Com.GetMap_Int(id, 112), m_Com.GetMap_Int(id, 116), m_Com.GetMap_Short(id, 120) & 0xFFFF,
                        m_Com.GetMap_Short(id, 122) & 0xFFFF, m_Com.GetMap_Short(id, 124), m_Com.GetMap_Short(id, 126),
                        m_Com.GetMap_Int(id, 128), m_Com.GetMap_Int(id, 132)
                    };
                    if (res[i][2] != 0) bClock = true;
                }
                if (lstBad.Count > 0)
                {
                    strWhy = "무응답 ID: " + string.Join(",", lstBad.ConvertAll(x => x.ToString()).ToArray());
                    EncWatch_Bad(1, false, strWhy);
                    return null;
                }
                bool bSame;
                lock (m_oEnc)
                {
                    bSame = m_abyEncLastBlk != null && m_abyEncLastBlk.Length == raw.Length;
                    for (int k = 0; bSame && k < raw.Length; k++) if (raw[k] != m_abyEncLastBlk[k]) bSame = false;
                    m_abyEncLastBlk = raw;
                }
                if (bSame)
                {
                    strWhy = "읽은 값이 그대로 (서보 블록 전체가 직전 읽기와 비트까지 같음)";
                    // 실시간 틱까지 같다 — 새로 읽은 값이 아니다: 읽기 실패로 센다 (값을 돌려주지 않는다).
                    //   시계가 없는 버스(버스 흉내 등)는 값은 돌려주고 '그대로' 로만 센다 (명령이 움직였을 때만 건다)
                    EncWatch_Bad(1, !bClock, strWhy);
                    if (bClock) return null;
                    return res;
                }
                EncWatch_Fresh();
                return res;
            }
            private const int BLOCK_ADDR = 112, BLOCK_LEN = 24;
            private const int BLOCK_MARK_POS = int.MaxValue;                       // 표지값 — 위치(132)에 이 값이 남아 있으면 대답하지 않은 것 (위치 틱에서 나오지 않는 값)
            // 서보 블록 표지 — 112~131 은 0xFF, 132~135 는 BLOCK_MARK_POS
            private void BlockMark(int nId)
            {
                byte[] map = m_Com.m_abyMap;
                int nBase = nId * CProtocol._SIZE_MAP + BLOCK_ADDR;
                if (nId < 0 || nBase < 0 || nBase + BLOCK_LEN > map.Length) return;
                for (int b = 0; b < 20; b++) map[nBase + b] = 0xFF;
                RegMark(nId, 132, 4, BLOCK_MARK_POS);
            }
            /// <summary>ReadServoBlock 원시값의 부호(sim→실물) — 위치·속도·목표 변환에 쓴다.
            /// 모터 세팅이 붙어 있으면 그 축의 방향 (부호 배열은 무시) — 틱을 각으로 바꿀 때는 영점까지 보는 RawToSim 을 쓸 것</summary>
            public int SignOf(int nIndex)
            {
                CArmMotor_t[] aMot = m_aMot;
                if (aMot != null) return (nIndex >= 0 && nIndex < aMot.Length) ? aMot[nIndex].DirSign : 1;
                return (nIndex >= 0 && nIndex < m_anSign.Length) ? m_anSign[nIndex] : 1;
            }

            /// <summary>실물 상태 진단 — 토크·에러·목표틱·현재틱을 버스에서 직접 읽는다</summary>
            public void diag()
            {
                if (m_Com == null) { Log("[DIAG] 통신 미연결\r\n"); return; }
                for (int r = 0; r < 2; r++)   // 통신 직후의 첫 응답은 스테일일 수 있어 두 번 읽는다
                {
                    m_Com.SyncRead_With_Address(64, 1, m_anIDs); System.Threading.Thread.Sleep(25);   // Torque Enable
                    m_Com.SyncRead_With_Address(70, 1, m_anIDs); System.Threading.Thread.Sleep(25);   // Hardware Error
                    m_Com.SyncRead_With_Address(116, 4, m_anIDs); System.Threading.Thread.Sleep(25);  // Goal Position
                    m_Com.SyncRead_With_Address(132, 4, m_anIDs); System.Threading.Thread.Sleep(25);  // Present Position
                }
                Log("[DIAG] ID: 토크 / 에러 / 목표틱 / 현재틱\r\n");
                foreach (int id in m_anIDs)
                    Log("   ID {0}: {1} / {2} / {3} / {4}\r\n", id,
                               m_Com.GetMap_Short(id, 64) & 0xFF, m_Com.GetMap_Short(id, 70) & 0xFF,
                               m_Com.GetMap_Int(id, 116), m_Com.GetMap_Int(id, 132));
            }

            /// <summary>현재 3D 관절각 전체를 실물로 전송 — 모션 스텝(렌더 틱/자가 펌프)마다 호출
            /// (연결 + 토크 ON 일 때만 동작하므로 무조건 불러도 안전).
            /// nTimeMs 는 첫 틱에만 쓰는 horizon 힌트 — 이후는 CProtocol.Move_Stream 이 실제 틱 간격을 실측해
            /// "다음 틱에 도착"하는 Profile Velocity 를 쓴다 (고정 40ms 로 쓰면 실물이 밀리다 구간 끝에서 급가속 — 2026-09-22 실측)</summary>
            public void SendPose(int nTimeMs)
            {
                if (m_Com == null || !m_bTorque) return;
                CArmMotor_t[] aMot = m_aMot;                   // 한 번만 읽는다 (세팅 교체와 경합 방지)
                if (aMot != null)
                {
                    // 모터 세팅 경로 — 붙잡은 자세를 모르면(토크 켤 때 자세·토크 상태를 읽지 못함, 처짐 보정 중 꺼진 축만 다시 켬) 보내지 않는다.
                    // 3D 가 실물과 맞는지 모르는 채 보내면 그 차이만큼 한 틱에 튄다. 움직이기 전 확인(CArmConn_t.EnsureSynced)이 서보 목표로 맞추면 다시 보낸다
                    if (m_afHoldSim == null)
                    {
                        if (!m_bWarnUnaligned) { m_bWarnUnaligned = true; Log("[COMM] 실물이 붙잡은 자세를 몰라 보내지 않음 (토크 켤 때 읽기 실패·토크 상태 모름·처짐 보정 중 꺼진 축만 다시 켬) — 움직이기 전 확인이 서보 목표로 맞추면 보냄\r\n"); }
                        return;
                    }
                    m_bWarnUnaligned = false;
                }
                float[] afDeg = new float[m_anIDs.Length];
                for (int i = 0; i < m_anIDs.Length; i++) afDeg[i] = m_C3d.GetData(m_anIDs[i]);
                float[] afHold = (float[])afDeg.Clone();       // 보정 얹기 전 = 실물이 붙잡을 3D 자세
                // 모터 세팅 경로 — 붙잡은 자세도 리밋으로 자른 값 — 3D 가 리밋 밖으로 나가 있어도(3D 리밋이 장면 리밋보다 넓었던 경우) 실물은 리밋에 서 있으니
                // 그 자세를 기억한다 (움직이기 전 확인이 3D 를 그리로 되돌려 어긋남이 풀린다 — 리밋 밖 자세를 기억하면 영영 거부된다)
                if (aMot != null) for (int i = 0; i < afHold.Length && i < aMot.Length; i++) afHold[i] = aMot[i].Clamp(afHold[i]);
                // 한 틱 관절 변화 막이 (작업 중에만) — 3D 자세가 한 틱에 건너뛰었으면 보정을 얹기 전에 멈춘다 (방향 추적·보정 전환을 건드리지 않게)
                if (m_bStepGuard)
                {
                    string strTrip = StepGuard_Over(m_afHoldSim, afHold, "3D 자세");
                    if (strTrip != null) { StepGuard_Trip(strTrip); return; }
                }
                m_C3d.SagComp_JointOut(m_anIDs, afDeg);   // 처짐 보상(관절 모델) — 실물 명령에만 얹는다, 모델 없으면 방향 추적만
                if (aMot == null)
                {
                    if (m_bStepGuard)
                    {
                        string strTrip = StepGuard_Over(m_afGoalSim, afDeg, "명령");
                        if (strTrip != null) { StepGuard_Trip(strTrip); return; }
                        EncWatch_Cmd(m_afGoalSim, afDeg);          // 엔코더 읽기 감시 — 마지막으로 새로 읽은 뒤 명령이 움직인 양
                    }
                    m_afLastSent = afDeg;                      // 진단용 — 보정까지 얹어 실제로 보낸 각도(부호 적용 전)
                    m_afHoldSim = afHold; m_afGoalSim = afDeg;
                    m_Com.Command_Clear();
                    for (int i = 0; i < m_anIDs.Length; i++)
                        m_Com.Command_Set(m_anIDs[i], afDeg[i] * m_anSign[i]);
                    m_Com.Move_Stream(nTimeMs);
                    m_lStepGuardLastTs = System.Diagnostics.Stopwatch.GetTimestamp();
                    return;
                }
                // 모터 세팅 경로 — 보정까지 얹은 최종 명령을 리밋으로 자르고(보정이 리밋 밖으로 밀어내지 못하게) 세팅으로 틱을 만든다.
                // CProtocol 은 기본 파라미터(전송층)라 CalcEvd2Angle(틱) 을 넘기면 안에서 CalcAngle2Evd 로 같은 틱이 나간다.
                int[] anTick = ComputeOutTicks(aMot, afDeg);
                if (m_bStepGuard)
                {
                    string strTrip = StepGuard_Over(m_afGoalSim, afDeg, "명령");   // 보정·리밋까지 얹은 최종 명령도 (보정 전환 이상 등)
                    if (strTrip != null) { StepGuard_Trip(strTrip); return; }
                    EncWatch_Cmd(m_afGoalSim, afDeg);              // 엔코더 읽기 감시 — 마지막으로 새로 읽은 뒤 명령이 움직인 양 (리밋으로 자른 최종 명령)
                }
                m_afLastSent = afDeg;                          // 보정·리밋까지 반영한 값
                m_afHoldSim = afHold; m_afGoalSim = afDeg;
                m_Com.Command_Clear();
                for (int i = 0; i < m_anIDs.Length; i++)
                    m_Com.Command_Set(m_anIDs[i], m_Com.CalcEvd2Angle(m_anIDs[i], anTick[i]));
                m_Com.Move_Stream(nTimeMs);
                m_lStepGuardLastTs = System.Diagnostics.Stopwatch.GetTimestamp();
            }

            // ── 한 틱 관절 변화 막이 (로봇팔 세팅 작업 동안만, 10-03) ──
            //   작업(C3d.SagArm_Open … SagArm_Close)이 켜고 끈다. 켜져 있으면 SendPose 가 보내기 전에 이번 틱 3D 자세(보정 전)와 최종 명령(보정·리밋 뒤)을
            //   직전 전송과 비교해, 어느 축이든 한도 = max(최소 한도, 각속도 한도 × 직전 전송 뒤 시간(최대 0.1초)) 를 넘으면 보내지 않는다:
            //   모션을 멈추고(MoveJoints_Stop) 3D 를 실물이 붙잡은 자세로 되돌린다 — 3D 와 실물이 어긋난 채 남지 않게 (다음 동작이 거기서 이어진다).
            //   까닭은 StepGuardTrip 에 남는다 — 작업이 다리·누름마다 보고 멈춘다. 경로 IK 가 리치 경계에서 다른 해로 건너뛰는 것
            //   (10-03 검토: 버스 흉내에서 한 틱 14~46°) 같은 불연속이 실물로 나가지 않게 하는 마지막 울타리다.
            //   꺼져 있으면 예전과 같다 (조그·재생·⑨ 등 작업 밖 기능은 이 막이를 쓰지 않는다)
            private volatile bool m_bStepGuard = false;
            private float m_fStepGuardMinDeg = 3f, m_fStepGuardDegPerSec = 60f;
            private long m_lStepGuardLastTs = 0;                 // 직전 전송 시각 (Stopwatch 눈금) — 0 = 모름
            private volatile string m_strStepGuardTrip = null;
            /// <summary>한 틱 관절 변화 막이가 걸렸으면 그 까닭 (아니면 null) — 막이를 켤 때·StepGuard_ClearTrip 이 지운다</summary>
            public string StepGuardTrip { get { return m_strStepGuardTrip; } }
            /// <summary>한 틱 관절 변화 막이가 켜져 있나 (로봇팔 세팅 작업 중)</summary>
            public bool StepGuardOn { get { return m_bStepGuard; } }
            /// <summary>한 틱 관절 변화 막이 켜기 (작업 시작) — fMinDeg = 틱 하나의 최소 한도(°), fDegPerSec = 직전 전송 뒤 시간에 비례하는 한도(°/s, 시간은 0.1초까지)</summary>
            internal void StepGuard_Begin(float fMinDeg, float fDegPerSec)
            {
                m_fStepGuardMinDeg = (fMinDeg > 0) ? fMinDeg : 3f;
                m_fStepGuardDegPerSec = (fDegPerSec > 0) ? fDegPerSec : 60f;
                m_strStepGuardTrip = null;
                EncWatch_Reset();                                  // 엔코더 읽기 감시도 이 작업부터 새로 (10-16)
                m_bStepGuard = true;
            }
            /// <summary>한 틱 관절 변화 막이 끄기 (작업 끝) — 엔코더 읽기 감시도 같이 꺼진다</summary>
            internal void StepGuard_End() { m_bStepGuard = false; }
            /// <summary>걸린 까닭 지우기 — 다음 동작(원위치 등)을 그 동작의 결과로 보려고. 엔코더 읽기 감시의 셈은 그대로 둔다
            /// (원위치 뒤 확인 읽기도 실패하면 다시 건다 — 새로 읽혀야 셈이 0 으로)</summary>
            internal void StepGuard_ClearTrip() { m_strStepGuardTrip = null; lock (m_oEnc) m_bEncTripped = false; }

            // ── 엔코더 읽기 감시 (작업 중에만 — 한 틱 관절 변화 막이와 같이 켜고 끈다, 10-16) ──
            //   10-16 실기: 수직 눈 확인·공구 기울기 보정 뒤 [1. 학습] 의 펼치기 동안 버스가 대답을 멈췄는데 (시작 때 읽기는 정상, 끝의 원위치 확인 읽기도 실패)
            //   ReadServoBlock 이 수신 맵에 남은 지난 값 — 08:30 누름 마지막 틱의 목표·부하·PWM 과 시작 때 읽은 휴지 자세 — 을 그대로 돌려줘
            //   누름 84틱 내내 실측 a1..a4 가 휴지 자세에 비트까지 멈춰 있었다 (명령은 5.7° 움직임). 첫닿음 검출기가 멈춘 FK 로 '시작부터 닿아 있었음' 을 냈고,
            //   원위치는 읽지 못한 채 한 번 더 움직이고 토크를 끈 것으로 적었다.
            //   그래서 읽기마다 본다: 마지막으로 새로 읽힌 뒤 보낸 명령(SendPose — 리밋까지 얹은 최종 명령)의 관절각 변화 합이 ENC_CMD_DEG 넘었는데
            //   새 값이 아닌 읽기 (무응답 — 수신 맵 표지값 / 서보 블록 전체가 직전 읽기와 비트까지 같음) 가 ENC_STALE_N 번 잇달았으면,
            //   또는 명령과 상관없이 실패가 ENC_FAIL_HARD_N 번 잇달았으면 건다: 모션을 멈추고(MoveJoints_Stop) 까닭을 막이 걸림(StepGuardTrip)에 남긴다 —
            //   작업이 다리·누름마다 보고 멈추고, 원위치(SagArm_Close)는 할 수 있는 데까지 간다. 정착 읽기(ReadTicks·ReadDeg — 안에서 여러 번 시도)의 실패는
            //   한 번에 ENC_STALE_N 번으로 친다. 서보 블록이 실시간 틱(ms 시계)까지 그대로면 읽기 실패로 센다 (시계가 없는 버스 흉내는 '그대로' 로만 — 명령이 움직였을 때만).
            //   위치만 같은 것은 세지 않는다 — 바닥을 누른 뒤 들어 올릴 때 정지 마찰로 명령이 4.8° 움직이는 동안 엔코더가
            //   그대로인 것이 정상이다 (10-15 기록, 그때도 목표·부하·실시간 틱은 틱마다 바뀐다)
            /// <summary>엔코더 읽기가 멈춤/실패로 걸렸을 때의 까닭 머리 (StepGuardTrip·EncoderTrip 이 이것으로 시작한다)</summary>
            public const string ENC_TRIP_HEAD = "엔코더 읽기가 멈춤/실패 — 연결을 다시 하십시오";
            internal const int ENC_STALE_N = 5;                   // 잇달아 새 값이 아닌 읽기 수
            internal const float ENC_CMD_DEG = 3f;                // 그동안 명령이 움직인 관절각 합 (deg)
            internal const int ENC_FAIL_HARD_N = 10;              // 명령이 움직이지 않아도 이만큼 잇달아 실패하면 (무게 포함 — 정착 읽기 실패 두 번)
            private readonly object m_oEnc = new object();
            private int m_nEncFail = 0, m_nEncSame = 0;           // 잇달은 실패(무게 포함)·같은 블록 수
            private int m_nEncFailCalls = 0;                      // 잇달은 실패한 읽기 호출 수 (까닭 글에 — 무게 없이)
            private double m_dEncCmd = 0;                         // 마지막으로 새로 읽힌 뒤 명령이 움직인 관절각 합 (deg)
            private byte[] m_abyEncLastBlk = null;                // 직전에 읽은 서보 블록 (IDs × 24바이트)
            private bool m_bEncTripped = false;                   // 이번 걸림을 이미 알렸다 (ClearTrip 이 푼다)
            private volatile string m_strEncTrip = null;          // 이 작업에서 마지막으로 건 까닭 (작업 시작 때 지운다)
            private string m_strEncLastWhy = "";
            /// <summary>이 작업(막이를 켠 뒤)에서 엔코더 읽기 감시가 건 마지막 까닭 — 없으면 null. ClearTrip 이 지우지 않는다 (원위치 확인이 까닭을 적는다)</summary>
            public string EncoderTrip { get { return m_strEncTrip; } }
            /// <summary>마지막으로 새로 읽힌 뒤 잇달은 '새 값이 아닌 읽기' 수 (실패는 무게 포함) — 진단용</summary>
            public int EncoderStaleCount { get { lock (m_oEnc) return m_nEncFail + m_nEncSame; } }
            private void EncWatch_Reset()
            {
                lock (m_oEnc) { m_nEncFail = 0; m_nEncSame = 0; m_nEncFailCalls = 0; m_dEncCmd = 0; m_abyEncLastBlk = null; m_bEncTripped = false; m_strEncLastWhy = ""; }
                m_strEncTrip = null;
            }
            // 새로 읽혔다 — 셈과 명령 변화를 0 으로
            private void EncWatch_Fresh()
            {
                lock (m_oEnc) { m_nEncFail = 0; m_nEncSame = 0; m_nEncFailCalls = 0; m_dEncCmd = 0; }
            }
            // 보낸 명령의 변화 (직전 걸린 목표 → 이번 최종 명령) — 작업 중에만 (SendPose)
            private void EncWatch_Cmd(float[] afPrev, float[] afNow)
            {
                if (!m_bStepGuard || afPrev == null || afNow == null || afPrev.Length != afNow.Length) return;
                double s = 0;
                for (int i = 0; i < afNow.Length; i++)
                {
                    double d = Math.Abs((double)afNow[i] - afPrev[i]);
                    if (!double.IsNaN(d) && !double.IsInfinity(d)) s += d;
                }
                lock (m_oEnc) m_dEncCmd += s;
            }
            // 새 값이 아닌 읽기 — nWeight = 실패 무게 (정착 읽기는 ENC_STALE_N), bSame = 블록이 그대로. 작업 중에만 세고, 넘으면 건다
            private void EncWatch_Bad(int nWeight, bool bSame, string strWhy)
            {
                if (!m_bStepGuard) return;
                string strTrip = null;
                lock (m_oEnc)
                {
                    if (bSame) m_nEncSame++; else { m_nEncFail += Math.Max(1, nWeight); m_nEncFailCalls++; }
                    if (!string.IsNullOrEmpty(strWhy)) m_strEncLastWhy = strWhy;
                    int n = m_nEncFail + m_nEncSame;
                    bool bMove = n >= ENC_STALE_N && m_dEncCmd >= ENC_CMD_DEG;
                    bool bHard = m_nEncFail >= ENC_FAIL_HARD_N;
                    if (!m_bEncTripped && (bMove || bHard))
                    {
                        m_bEncTripped = true;
                        var ci = CultureInfo.InvariantCulture;
                        string strCmd = bMove ? string.Format(ci, "명령이 {0:F1}° 움직이는 동안 ", m_dEncCmd) : "";
                        int nCalls = m_nEncFailCalls + m_nEncSame;
                        string strN = nCalls > 1 ? nCalls.ToString(ci) + "번 잇달아 " : "";   // 정착 읽기 한 번은 안에서 여러 번 시도한 것 — 수를 적지 않는다
                        strTrip = (m_nEncSame > 0 && m_nEncSame >= m_nEncFailCalls)
                            ? string.Format(ci, "{0} ({1}읽은 값이 {2}그대로 — 버스가 새 값을 주지 않음, 전원·배선·USB 확인)", ENC_TRIP_HEAD, strCmd, strN)
                            : string.Format(ci, "{0} ({1}새 값을 {2}읽지 못함 — {3}, 전원·배선·USB 확인)", ENC_TRIP_HEAD, strCmd, strN, m_strEncLastWhy);
                    }
                }
                if (strTrip == null) return;
                m_strEncTrip = strTrip;
                m_strStepGuardTrip = strTrip;                     // 작업이 다리·누름마다 보는 걸림 (SagArm_GuardWhy·누름의 새 걸림)
                try { m_C3d.MoveJoints_Stop(); } catch { }        // 지금 모션을 멈춘다 — 이 읽기가 싱크 안이면 이 틱이 마지막
                Log("[COMM] ★{0} — 모션을 멈춤\r\n", strTrip);
            }
            // 직전 값과 이번 값의 축마다 차 — 한도를 넘는 가장 큰 축의 까닭 (넘지 않으면 null). 직전 값을 모르면 보지 않는다
            private string StepGuard_Over(float[] afPrev, float[] afNow, string strWhat)
            {
                if (afPrev == null || afNow == null || afPrev.Length != afNow.Length) return null;
                double dt = 0.1;
                long t0 = m_lStepGuardLastTs;
                if (t0 != 0)
                {
                    dt = (System.Diagnostics.Stopwatch.GetTimestamp() - t0) / (double)System.Diagnostics.Stopwatch.Frequency;
                    if (dt > 0.1) dt = 0.1;
                    if (dt < 0) dt = 0;
                }
                float fLim = Math.Max(m_fStepGuardMinDeg, (float)(m_fStepGuardDegPerSec * dt));
                int iw = -1; float dw = 0f;
                for (int i = 0; i < afNow.Length; i++)
                {
                    float d = Math.Abs(afNow[i] - afPrev[i]);
                    if (d > fLim && d > dw) { dw = d; iw = i; }
                }
                if (iw < 0) return null;
                return string.Format(CultureInfo.InvariantCulture, "한 틱 관절 변화 막이: ID {0} {1} {2:F1}° (한도 {3:F1}°)", m_anIDs[iw], strWhat, dw, fLim);
            }
            // 걸림 — 보내지 않고 모션을 멈춘 뒤 3D 를 붙잡은 자세로 되돌린다 (모션 스레드에서 — 싱크 안)
            private void StepGuard_Trip(string strWhy)
            {
                m_strStepGuardTrip = strWhy;
                m_C3d.MoveJoints_Stop();
                float[] h = m_afHoldSim;
                if (h != null && h.Length == m_anIDs.Length)
                    for (int i = 0; i < m_anIDs.Length; i++) m_C3d.SetData(m_anIDs[i], h[i]);
                Log("[COMM] ★{0} — 보내지 않고 모션을 멈춤, 3D 를 붙잡은 자세로 되돌림\r\n", strWhy);
            }

            // ── 모터 세팅 경로 (2026-10-02, COjw_44_ArmMotor.cs) ──
            // 3D 각 ↔ 틱 변환을 세팅(방향·영점·틱/회전·각/회전·리밋)으로 한다. CProtocol 은 전송층(기본 파라미터)으로만 쓴다.
            // 실물 ID 는 축 번호와 같아야 한다 (SetMotors 가 확인 — 버스 동작은 IDs 를 그대로 쓴다).
            // 세팅을 붙이지 않은 장면에서는 아래 계산 함수도 예전 식(부호 배열 + 2048 기준, 리밋 없음)을 따른다
            private const float OUTSIDE_TOL_DEG = 1f;      // 실물 값이 리밋 밖인지 볼 때의 여유 (CArmConn_t.OutsideTolDeg 기본값과 같다) — 이보다 밖이면 늘 거부
            /// <summary>틱 하나의 각 (각/회전 ÷ 틱/회전) — 세팅이 이상하면 0</summary>
            private static float TickTolDeg(CArmMotor_t m)
            {
                return (m != null && m.nMechMove > 0 && m.fMechAngle > 0f) ? m.fMechAngle / m.nMechMove : 0f;
            }
            /// <summary>쓰이는 리밋 밖으로 나간 거리 (도) — 안이면 0</summary>
            private static float OutsideDeg(CArmMotor_t m, float f)
            {
                float fLo = m.LimitLo, fHi = m.LimitHi;
                if (!float.IsNaN(fLo) && f < fLo) return fLo - f;
                if (!float.IsNaN(fHi) && f > fHi) return f - fHi;
                return 0f;
            }
            /// <summary>토크 ON 에서 세팅을 바꿀 때 — 실물에 걸린 값(붙잡은 자세·걸린 목표. fOld 는 지금 세팅의 해석, fNew 는 새 세팅의 해석)을
            /// 새 리밋이 지금 리밋보다 틱 하나 넘게 더 자르거나, 새 리밋 밖으로 OUTSIDE_TOL_DEG 넘게 나가 있으면 true. 그러면 다음 전송(또는 3D 리밋으로
            /// 잘린 다음 모션의 첫 틱)이 그만큼 한 번에 튄다 — 1° 안쪽이라도 받지 않는다 (DH 를 다시 해석해 리밋을 붙잡은 자세 바로 안쪽으로 좁힌 경우 등).
            /// 지금 리밋으로도 똑같이 잘리는 값(손으로 리밋 바로 밖에 둔 채 토크를 켠 경우 등)은 이 변경 때문에 튀는 것이 아니라 막지 않는다</summary>
            private static bool ClampGrows(CArmMotor_t mOld, float fOld, CArmMotor_t mNew, float fNew)
            {
                float dNew = OutsideDeg(mNew, fNew);
                if (dNew > OUTSIDE_TOL_DEG) return true;
                float dOld = (mOld == null) ? 0f : OutsideDeg(mOld, fOld);
                return dNew > Math.Max(TickTolDeg(mNew), dOld);
            }

            /// <summary>모터 세팅이 붙어 있나</summary>
            public bool HasMotors { get { return m_aMot != null; } }

            /// <summary>모터 세팅 붙이기·바꾸기. IDs 의 축이 모두 세트에 있어야 하고, 프로토콜 2·위치 제어·틱/회전·각/회전 &gt; 0·실물 ID = 축이어야 하며,
            /// 쓰이는 리밋이 비어 있으면(하한 &gt; 상한 — 수동 리밋과 EDH $jointrange 가 겹치지 않음) 거부한다. 3D 모션 중이면 거부한다. 매핑(방향·영점·틱/회전·각/회전·실물 ID)이 같으면 리밋만 바뀐 것 — 바꿔 끼우고 끝 (버스를 쓰지 않는다).
            /// 단 토크 ON 이면 새 리밋이 실물이 붙잡은 자세나 걸린 목표(보정 포함)를 지금보다 틱 하나 넘게 더 자르거나 1° 넘게 밖에 두면 거부한다
            /// (ClampGrows) — 다음 전송이 리밋으로 잘리면서 움직이지 않던 축이 한 틱에 튄다.
            /// 매핑이 바뀌었고 연결 중이면 실물은 움직이지 않고 3D 를 새 해석으로 옮긴다:
            ///   토크 OFF — 틱을 읽어 새 해석으로 SetData (읽기 실패면 거부)
            ///   토크 ON  — 처짐 보정이 켜져 있으면 거부 (보정량만큼 움찔). 아니면 실물에 걸린 목표(마지막으로 보낸 틱 — 보정을 끈 직후라 1초 페이드가
            ///              아직 안 돌았으면 보정이 남아 있다)를 새 해석으로 다시 써서 3D·붙잡은 자세로 삼는다 → 다음 전송 틱이 지금 목표와 같다.
            ///              새 각이 리밋 밖이면 거부. 걸린 목표를 모르면(토크 켤 때 정렬 실패 — 그 뒤 보낸 명령 없음) 틱을 읽어 옮긴다
            ///   끝으로 처짐 보정 방향 추적·페이드와 스트리밍 추정을 리셋한다 (남아 있던 보정은 3D 로 들어갔으니 걸린 보정 없음에서 시작).
            ///   미연결이면 3D 는 그대로 둔다 (해석할 실물이 없다).
            /// 받아들였는데 알릴 것이 있으면 true 와 strWhy (남아 있던 보정만큼 3D 가 옮겨짐 등)</summary>
            public bool SetMotors(CArmMotorSet_t set, out string strWhy)
            {
                strWhy = "";
                if (set == null) { strWhy = "모터 세팅 없음"; return false; }
                CArmMotor_t[] aNew = new CArmMotor_t[m_anIDs.Length];
                for (int i = 0; i < m_anIDs.Length; i++)
                {
                    CArmMotor_t m = set.Get(m_anIDs[i]);
                    string strP = "ID " + m_anIDs[i] + ": ";
                    if (m == null) { strWhy = strP + "모터 세팅이 없음"; return false; }
                    if (m.nProtocolVersion != 2) { strWhy = strP + "프로토콜 2 만"; return false; }
                    if (m.nMotorControlType != 0) { strWhy = strP + "위치 제어(0)만"; return false; }
                    if (m.nMechMove <= 0 || !(m.fMechAngle > 0f)) { strWhy = strP + "틱/회전·각/회전은 0 보다 커야 함"; return false; }
                    if (m.nMotorDir != 0 && m.nMotorDir != 1) { strWhy = strP + "방향은 0·1 만"; return false; }
                    if (m.nMotorID != m_anIDs[i]) { strWhy = strP + "실물 ID 가 축 번호와 다름 — 다음 단계 기능"; return false; }
                    float fLoEff = m.LimitLo, fHiEff = m.LimitHi;   // 수동 ∩ EDH 가 비면 하한 > 상한 — 그대로 쓰면 모든 명령이 상한으로 잘린다
                    if (!float.IsNaN(fLoEff) && !float.IsNaN(fHiEff) && fLoEff > fHiEff)
                    { strWhy = strP + "리밋이 비어 있음 (하한 " + CArmMotorSet_t.Fmt(fLoEff) + "° > 상한 " + CArmMotorSet_t.Fmt(fHiEff) + "° — 수동 리밋과 EDH $jointrange 가 겹치지 않음)"; return false; }
                    aNew[i] = m.Clone();
                }
                if (m_C3d.MoveJoints_IsMoving()) { strWhy = "3D 모션 중 — 끝난 뒤 다시"; return false; }
                CArmMotor_t[] aOld = m_aMot;
                if (aOld == null) aOld = LegacyMotors();
                bool bSame = true;
                for (int i = 0; i < aNew.Length; i++) if (!aNew[i].SameMapping(aOld[i])) { bSame = false; break; }
                // 실물이 붙잡은 자세(보정 얹기 전)와 걸린 목표(마지막으로 보낸 값 — 보정·리밋 자르기 포함) — 토크 ON 이고 알 때만
                float[] afHold = m_afHoldSim, afGoal = m_afGoalSim;
                bool bHeld = m_Com != null && m_bTorque && afHold != null && afHold.Length == m_anIDs.Length;
                if (bHeld && (afGoal == null || afGoal.Length != m_anIDs.Length)) afGoal = afHold;
                if (bSame)
                {
                    // 리밋·이름 등만 바뀜 — 버스를 쓰지 않는다. 토크 ON 이면 붙잡은 자세·걸린 목표가 새 리밋 안에 있어야 한다
                    if (bHeld)
                        for (int i = 0; i < aNew.Length; i++)
                        {
                            if (ClampGrows(aOld[i], afHold[i], aNew[i], afHold[i]))
                            { strWhy = aNew[i].OutsideText(afHold[i]) + " — 실물이 붙잡은 자세가 새 리밋 밖이라 바꾸지 않음 (토크를 끄고 바꾸거나 범위를 넓히십시오)"; return false; }
                            if (ClampGrows(aOld[i], afGoal[i], aNew[i], afGoal[i]))
                            { strWhy = aNew[i].OutsideText(afGoal[i]) + " — 실물에 걸린 목표(처짐 보정 포함)가 새 리밋 밖이라 바꾸지 않음 (보정을 끄거나 범위를 넓히십시오)"; return false; }
                        }
                    m_aMot = aNew;
                    return true;
                }
                string strNote = "";
                if (m_Com != null)
                {
                    float[] afNew = new float[m_anIDs.Length];
                    if (!bHeld)
                    {
                        int[] anTick = ReadTicks(out strWhy);
                        if (anTick == null) { strWhy = "읽기 실패 — 세팅을 바꾸지 않음: " + strWhy; return false; }
                        for (int i = 0; i < afNew.Length; i++) afNew[i] = aNew[i].TickToSim(anTick[i]);
                    }
                    else
                    {
                        C3d.CSagComp sag = m_C3d.SagComp_Get();
                        if (sag != null && sag.Enabled)
                        { strWhy = "처짐 보정이 켜져 있음 — 보정을 끈 뒤 바꾸십시오 (토크 ON 에서 해석이 바뀌면 보정량만큼 움찔한다. 끈 뒤에는 실물을 그대로 두고 3D 만 옮긴다)"; return false; }
                        var lstLeft = new List<string>();
                        for (int i = 0; i < afNew.Length; i++)
                        {
                            int nGoalTick = aOld[i].SimToTick(afGoal[i]);          // 실물에 걸린 목표 틱 — 다음 전송 틱이 이것과 같게
                            afNew[i] = aNew[i].TickToSim(nGoalTick);
                            if (ClampGrows(aOld[i], aOld[i].TickToSim(nGoalTick), aNew[i], afNew[i])) { strWhy = aNew[i].OutsideText(afNew[i]) + " — 새 해석이면 리밋 밖이라 바꾸지 않음"; return false; }
                            int nHoldTick = aOld[i].SimToTick(afHold[i]);
                            if (Math.Abs(nGoalTick - nHoldTick) > 1)
                                lstLeft.Add(string.Format(CultureInfo.InvariantCulture, "ID {0} {1:+0.0;-0.0}°", m_anIDs[i], aOld[i].TickToSim(nGoalTick) - aOld[i].TickToSim(nHoldTick)));
                        }
                        if (lstLeft.Count > 0)
                            strNote = "실물에 아직 걸려 있던 보정(또는 리밋 자르기)만큼 3D 를 옮김 (" + string.Join(", ", lstLeft.ToArray()) + ") — 실물은 그대로";
                        m_afGoalSim = (float[])afNew.Clone();
                        m_afLastSent = Remap(aOld, aNew, m_afLastSent);
                        m_afHoldSim = (float[])afNew.Clone();
                    }
                    for (int i = 0; i < afNew.Length; i++) m_C3d.SetData(m_anIDs[i], afNew[i]);
                    m_C3d.SagComp_JointReset();
                    m_Com.Stream_Reset();
                }
                if (Array.IndexOf(m_anSign, -1) >= 0) Log("[COMM] 부호 배열은 무시 — 방향은 모터 세팅이 정본\r\n");
                m_aMot = aNew;
                strWhy = strNote;
                if (strNote.Length > 0) Log("[COMM] " + strNote + "\r\n");
                return true;
            }
            /// <summary>세트의 매핑(방향·영점·틱/회전·각/회전·실물 ID)이 지금 붙은 세팅과 축마다 같은가 — 같으면 SetMotors 는 리밋만 바꿔 끼우고
            /// 버스·로그를 쓰지 않는다 (CArmConn_t 가 헤더 잠금 안에서 리밋만 바꿀 때 먼저 본다). 세팅이 없거나 축이 빠졌으면 false</summary>
            internal bool SameMappingAs(CArmMotorSet_t set)
            {
                CArmMotor_t[] aMot = m_aMot;
                if (aMot == null || set == null) return false;
                for (int i = 0; i < m_anIDs.Length && i < aMot.Length; i++)
                {
                    CArmMotor_t m = set.Get(m_anIDs[i]);
                    if (m == null || !m.SameMapping(aMot[i])) return false;
                }
                return true;
            }
            /// <summary>예전 경로(부호 배열 + 2048 기준)를 세팅으로 쓴 것 — 세팅을 처음 붙일 때 3D 를 옮기는 비교 기준</summary>
            private CArmMotor_t[] LegacyMotors()
            {
                CArmMotor_t[] a = new CArmMotor_t[m_anIDs.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = CArmMotor_t.CreateDefault(m_anIDs[i]);
                    a[i].nMotorDir = (m_anSign[i] < 0) ? 1 : 0;
                }
                return a;
            }
            private static float[] Remap(CArmMotor_t[] aOld, CArmMotor_t[] aNew, float[] af)
            {
                if (af == null || af.Length != aNew.Length) return null;
                float[] r = new float[af.Length];
                for (int i = 0; i < af.Length; i++) r[i] = aNew[i].TickToSim(aOld[i].SimToTick(af[i]));
                return r;
            }

            /// <summary>서보 틱 → 3D 각 (IDs 순 색인). 세팅이 있으면 TickToSim, 없으면 예전 식 CSim2Real.Deg(틱) × 부호</summary>
            public float RawToSim(int nIndex, int nTick)
            {
                CArmMotor_t[] aMot = m_aMot;
                if (aMot != null && nIndex >= 0 && nIndex < aMot.Length) return aMot[nIndex].TickToSim(nTick);
                return CSim2Real.Deg(nTick) * SignOf(nIndex);
            }
            /// <summary>서보 속도 raw(Present Velocity, 0.229 rpm 단위) → 3D 각속도(도/s). 예전 식 raw × 0.229 × 6 × 부호 에
            /// 세팅의 비율 (각/회전 ÷ 틱/회전) ÷ (360 ÷ 4096) 을 곱한다 — 기본값이면 예전 식과 같다</summary>
            public float RawVelToSim(int nIndex, int nRaw)
            {
                CArmMotor_t[] aMot = m_aMot;
                if (aMot == null || nIndex < 0 || nIndex >= aMot.Length) return nRaw * 0.229f * 6f * SignOf(nIndex);
                CArmMotor_t m = aMot[nIndex];
                float v = nRaw * 0.229f * 6f * m.DirSign;
                if (m.nMechMove != 4096 || m.fMechAngle != 360f) v *= (m.fMechAngle / m.nMechMove) / (360f / 4096f);
                return v;
            }
            /// <summary>보낼 틱 (IDs 순) — SendPose 와 같은 본체. 세팅이 있으면 afSimDeg 를 쓰이는 리밋으로 자른 값으로 바꾸고 SimToTick 틱을 돌려준다.
            /// 세팅이 없으면(예전 경로) 리밋 없이 CProtocol 기본 변환과 같은 틱을 계산만 한다 (afSimDeg 는 그대로). 길이가 다르면 null</summary>
            public int[] ComputeOutTicks(float[] afSimDeg)
            {
                if (afSimDeg == null || afSimDeg.Length != m_anIDs.Length) return null;
                CArmMotor_t[] aMot = m_aMot;
                if (aMot != null) return ComputeOutTicks(aMot, afSimDeg);
                int[] an = new int[afSimDeg.Length];
                for (int i = 0; i < an.Length; i++) an[i] = (int)Math.Round(((4096f * (afSimDeg[i] * m_anSign[i])) / 360f) * 1f + 2048f);
                return an;
            }
            private static int[] ComputeOutTicks(CArmMotor_t[] aMot, float[] afDeg)
            {
                int[] an = new int[aMot.Length];
                for (int i = 0; i < aMot.Length; i++)
                {
                    afDeg[i] = aMot[i].Clamp(afDeg[i]);           // 보정 뒤 최종 리밋
                    an[i] = aMot[i].SimToTick(afDeg[i]);
                }
                return an;
            }
            /// <summary>쓰이는 리밋으로 자른 3D 각 — 세팅이 없으면 그대로</summary>
            public float ClampSim(int nIndex, float fSim)
            {
                CArmMotor_t[] aMot = m_aMot;
                return (aMot != null && nIndex >= 0 && nIndex < aMot.Length) ? aMot[nIndex].Clamp(fSim) : fSim;
            }

            /// <summary>전 축 틱 정착 읽기 (IDs 순, CSim2Real.ReadAllTicks) — 응답 없는 ID 의 지난 값을 쓰지 않도록 수신 맵을 -1 로 채우고 읽는다.
            /// 실패하면 null 과 사유. 작업 중이면 엔코더 읽기 감시에 알린다 (정착 읽기는 안에서 여러 번 시도하므로 실패 한 번을 ENC_STALE_N 번으로 친다 — 10-16)</summary>
            public int[] ReadTicks(out string strWhy)
            {
                strWhy = "";
                if (m_Com == null) { strWhy = "통신 미연결"; return null; }
                MarkPosition();
                int[] an = CSim2Real.ReadAllTicks(m_Com, m_anIDs, out strWhy);
                if (an == null) EncWatch_Bad(ENC_STALE_N, false, strWhy); else EncWatch_Fresh();
                return an;
            }
            private void MarkPosition() { foreach (int id in m_anIDs) RegMark(id, 132, 4, -1L); }   // -1 = ReadAll 이 무응답으로 보는 값

            /// <summary>레지스터 하나를 전 ID 에서 읽는다 — ID 마다 두 번 연속 같은 값이 나와야 확정 (쓰기 직후 스테일 대비, 최대 4회).
            /// 응답 없는 ID 를 알아보도록 읽을 때마다 수신 맵을 표지값(1바이트 0xFF, 2바이트 0xFFFF, 4바이트 0x7FFFFFFF)으로 채운다 —
            /// 그 값 자체는 무응답으로 본다(읽는 레지스터에서 나오지 않는 값). 값: 1·2바이트는 부호 없는 값, 4바이트는 부호 있는 int.
            /// 확정 못 한 ID 는 빠지고 strWhy 에 적힌다. 미연결·주소 오류면 null</summary>
            public Dictionary<int, long> ReadReg(int nAddr, int nLen, out string strWhy) { return ReadReg(nAddr, nLen, 0, out strWhy); }
            /// <summary>ReadReg 와 같고, 연속 두 번의 차가 nTol 안이면 확정한다 (전압·온도처럼 조금씩 흔들리는 값용 — CSim2Real.Read 의 tol 과 같은 뜻).
            /// 확정값은 뒤에 읽은 값</summary>
            public Dictionary<int, long> ReadReg(int nAddr, int nLen, int nTol, out string strWhy)
            {
                strWhy = "";
                if (m_Com == null) { strWhy = "통신 미연결"; return null; }
                if ((nLen != 1 && nLen != 2 && nLen != 4) || nAddr < 0 || nAddr + nLen > CProtocol._SIZE_MAP) { strWhy = "주소·길이 오류"; return null; }
                if (nTol < 0) nTol = 0;
                long lMark = RegSentinel(nLen);
                var dicOut = new Dictionary<int, long>();
                var dicPrev = new Dictionary<int, long>();
                for (int t = 0; t < 4 && dicOut.Count < m_anIDs.Length; t++)
                {
                    var lst = new List<int>();
                    foreach (int id in m_anIDs) if (!dicOut.ContainsKey(id)) lst.Add(id);
                    foreach (int id in lst) RegMark(id, nAddr, nLen, lMark);
                    m_Com.SyncRead_With_Address(nAddr, nLen, lst.ToArray()); System.Threading.Thread.Sleep(8);
                    foreach (int id in lst)
                    {
                        long v = RegValue(id, nAddr, nLen);
                        if (v == lMark) { dicPrev.Remove(id); continue; }   // 이번엔 응답 없음
                        long p;
                        if (dicPrev.TryGetValue(id, out p) && Math.Abs(p - v) <= nTol) dicOut[id] = v;
                        else dicPrev[id] = v;
                    }
                }
                if (dicOut.Count < m_anIDs.Length)
                {
                    var lstBad = new List<int>();
                    foreach (int id in m_anIDs) if (!dicOut.ContainsKey(id)) lstBad.Add(id);
                    strWhy = "무응답/불안정 ID: " + string.Join(",", lstBad.ConvertAll(x => x.ToString()).ToArray());
                }
                return dicOut;
            }
            private void RegMark(int nId, int nAddr, int nLen, long lValue)
            {
                byte[] map = m_Com.m_abyMap;
                int nBase = nId * CProtocol._SIZE_MAP + nAddr;
                if (nId < 0 || nBase < 0 || nBase + nLen > map.Length) return;
                for (int b = 0; b < nLen; b++) map[nBase + b] = (byte)((lValue >> (8 * b)) & 0xFF);
            }
            private long RegValue(int nId, int nAddr, int nLen)
            {
                if (nLen == 1) return m_Com.GetMap(nId, nAddr);
                if (nLen == 2) return m_Com.GetMap_Short(nId, nAddr) & 0xFFFF;
                return m_Com.GetMap_Int(nId, nAddr);
            }
            private static long RegSentinel(int nLen) { return (nLen == 1) ? 0xFFL : ((nLen == 2) ? 0xFFFFL : (long)int.MaxValue); }

            /// <summary>실물이 붙잡고 있는 3D 자세 (IDs 순, 처짐 보정 얹기 전) — 토크 ON 정렬과 SendPose 마다 갱신. 모르면 null (복사본)</summary>
            public float[] HoldSimDeg { get { float[] a = m_afHoldSim; return (a == null) ? null : (float[])a.Clone(); } }
            /// <summary>실물에 걸린 목표 (IDs 순 3D 각, 보정·리밋 반영) — 정렬 직후는 정렬 자세, 그 뒤는 SendPose 가 보낸 값. 모르면 null</summary>
            internal float[] GoalSimDeg { get { float[] a = m_afGoalSim; return (a == null) ? null : (float[])a.Clone(); } }
            /// <summary>붙잡은 자세를 모름으로 — 실물로 보내지 않고(SendPose), 움직이기 전에 CArmConn_t.EnsureSynced 가 서보 목표(못 읽으면 실측)로 맞춘다
            /// (토크를 켤 때 어떤 축이 이미 켜져 있었는지 확인하지 못한 경우 — 정렬한 지금 위치는 하중만큼 처져 있을 수 있다)</summary>
            internal void ForgetHold() { m_afHoldSim = null; m_afGoalSim = null; }
            /// <summary>토크 ON 인데 정렬을 못 했던 경우 — 아직 명령을 보낸 적이 없으니 실측 자세를 붙잡은 자세·목표로 삼는다 (CArmConn_t.EnsureSynced)</summary>
            internal void HoldFromReal(float[] afSim) { HoldFromRealGoal(afSim, afSim); }
            /// <summary>HoldFromReal 과 같고 붙잡은 자세(3D, 보정 얹기 전)와 걸린 목표(서보 목표 — 지난 보정이 들어 있을 수 있다)를 따로 준다
            /// (목표가 null 이면 붙잡은 자세와 같다). CArmConn_t.EnsureSynced 의 서보 목표 이어받기는 둘 다 서보 목표 (10-03 — 되짚지 않는다)</summary>
            internal void HoldFromRealGoal(float[] afHold, float[] afGoal)
            {
                if (afHold == null || afHold.Length != m_anIDs.Length) return;
                if (afGoal == null || afGoal.Length != m_anIDs.Length) afGoal = afHold;
                m_afHoldSim = (float[])afHold.Clone();
                m_afGoalSim = (float[])afGoal.Clone();
            }
            /// <summary>서보가 이미 토크 ON 으로 붙잡고 있을 때(연결 해제 때 토크를 유지했던 경우 등) 그 상태를 이어받는다 — 버스에 쓰지 않는다
            /// (CArmConn_t.Connect). afHeld = 서보가 붙잡은 목표 (IDs 순 3D 각 — Goal Position 을 세팅으로 해석한 값): 3D·붙잡은 자세·걸린 목표를
            /// 그것으로 맞추고 토크 ON 으로 표시, 처짐 보정 방향 추적과 스트리밍 추정을 리셋한다 → 다음 전송 틱 = 지금 목표 틱.
            /// afHeld 가 null 이면 붙잡은 자세 모름 — 보내지 않고(SendPose), 움직이기 전에 EnsureSynced 가 실측으로 맞춘다</summary>
            internal void AdoptTorqueOn(float[] afHeld) { AdoptTorqueOnGoal(afHeld, afHeld); }
            /// <summary>AdoptTorqueOn 과 같고 3D·붙잡은 자세(afHold — 보정 얹기 전)와 걸린 목표(afGoal — 서보 목표, 지난 보정이 들어 있을 수 있다)를 따로 준다.
            /// CArmConn_t 는 둘 다 서보 목표로 준다 (10-03 — 되짚지 않는다). 처짐 보정이 걸려 있었으면 그 전에 보정을 페이드 없이 끈다
            /// (C3d.SagComp_OffForAdopt) → 다음 전송 틱 = 지금 목표 틱, 그 뒤에도 보정이 얹히지 않는다</summary>
            internal void AdoptTorqueOnGoal(float[] afHold, float[] afGoal)
            {
                if (m_Com == null) return;
                if (afHold != null && afHold.Length == m_anIDs.Length)
                {
                    if (afGoal == null || afGoal.Length != m_anIDs.Length) afGoal = afHold;
                    for (int i = 0; i < m_anIDs.Length; i++) m_C3d.SetData(m_anIDs[i], afHold[i]);
                    m_afHoldSim = (float[])afHold.Clone();
                    m_afGoalSim = (float[])afGoal.Clone();
                }
                else { m_afHoldSim = null; m_afGoalSim = null; }
                m_afLastSent = null;
                m_C3d.SagComp_JointReset();
                m_bTorque = true;
                m_Com.Stream_Reset();
                Log("[COMM] 서보가 붙잡은 목표를 이어받음 — 다음 전송 틱 = 지금 목표 틱\r\n");
            }
            /// <summary>처짐 보정이 실물 명령에 걸려 있거나 다음 출력 틱에 걸린다 — 관절 모델이 켜져 있거나, 직전 출력 틱에 얹은 관절 보정·전환 기준
            /// (C3d.SagComp_HasOffsets — 끈 뒤 페이드, 전환 씨앗으로 심은 보정 포함)·손목 유지가 있거나, 마지막으로 보낸 목표가 붙잡은 자세와
            /// 다르다 (보정을 끈 뒤 1초 페이드가 남은 경우 등 — 보정이 없으면 둘은 같은 값이다). 버스를 쓰지 않는다</summary>
            internal bool SagCompInPlay()
            {
                C3d.CSagComp m = m_C3d.SagComp_Get();
                if (m != null && m.Enabled && (m.HasJoint || m.HasWristTilt)) return true;   // 손목 공구 기울기 보정만 있어도 출력 단에 얹힌다 (10-15)
                foreach (float f in m_C3d.SagComp_LastJointCorr) if (f != 0f) return true;
                if (m_C3d.SagComp_WristHoldNow != 0f) return true;
                if (m_C3d.SagComp_HasOffsets) return true;
                float[] h = m_afHoldSim, g = m_afGoalSim;
                if (h != null && g != null && h.Length == g.Length)
                    for (int i = 0; i < h.Length; i++) if (Math.Abs(g[i] - h[i]) > 1e-3f) return true;
                return false;
            }
            /// <summary>토크가 꺼진 축만 켠다 (CArmConn_t.TorqueOn — 토크 ON 으로 아는데 꺼진 서보가 섞여 있을 때: 연결 때 일부 축만 토크 ON 이던 상태를
            /// 이어받았거나, 전원이 잠깐 끊기거나 오류로 서보가 스스로 꺼진 경우). 이미 토크 ON 으로 알고 있는 장면에서 부른다.
            /// afPose = IDs 순 지금 자세 (3D 각, 정착 읽기를 세팅으로 해석) — 켜는 축의 3D 를 그 값으로 맞추고, 붙잡은 자세·걸린 목표도 그 값으로 한다
            /// (이미 켜진 축은 붙잡은 자세·걸린 목표 그대로 → 다음 전송 틱 = 켜진 축은 지금 목표, 켜는 축은 지금 위치).
            /// 처짐 보정이 걸려 있거나 막 걸려 있었으면 (SagCompInPlay, 10-03): 보정 전환을 다시 건다 (C3d.SagComp_SeedFrom) — 켜진 축은 실물에 걸린 보정에서
            /// 그대로 이어 가고(두 번 얹히지 않는다), 켜는 축은 지금(처진) 위치에서 보정 0 으로 시작해 다음 동작의 출력 틱에 걸쳐 1초 동안 들어온다
            /// → 다음 전송 틱 = 서보 목표 (전환이 끝나 있었어도 켠 축에 보정이 한 번에 얹히지 않는다).
            /// afPose 가 null 이거나 붙잡은 자세를 원래 몰랐으면 붙잡은 자세를 모름으로 둔다 — 보내지 않고(SendPose), 움직이기 전에
            /// CArmConn_t.EnsureSynced 가 전 축을 서보 목표로 맞춘다.
            /// CArmConn_t.TorqueOnRest 는 처짐 보정이 걸려 있고 켜져 있던 축이 있으면 이것을 부르기 전에 보정을 페이드 없이 끄고 켜져 있던 축을 서보 목표로
            /// 맞춘다 (10-03 — 그때는 위의 전환 씨앗을 쓰지 않는다: 켜져 있던 축의 다음 틱 = 서보 목표, 켜는 축 = 지금 위치, 보정 없음).
            /// 전 축이 꺼져 있었으면(붙잡고 있던 서보 없음) 보정을 끄지 않고 위의 전환 씨앗으로 켠다 (토크 켜기와 같다 — 보정은 0 부터)</summary>
            internal void TorqueOnAxes(int[] anIds, float[] afPose)
            {
                if (m_Com == null || anIds == null || anIds.Length == 0) return;
                bool bPose = afPose != null && afPose.Length == m_anIDs.Length;
                float[] afHold = m_afHoldSim, afGoal = m_afGoalSim;
                bool bKeep = bPose && afHold != null && afHold.Length == m_anIDs.Length;
                bool bComp = bKeep && SagCompInPlay();                // 켜기 전에 본다 (전환 기준을 심을지)
                float[] h = null, g = null;
                if (bKeep)
                {
                    h = (float[])afHold.Clone();
                    g = (afGoal != null && afGoal.Length == m_anIDs.Length) ? (float[])afGoal.Clone() : (float[])afHold.Clone();
                }
                var lstIdx = new List<int>();
                for (int i = 0; i < m_anIDs.Length && bPose; i++)
                {
                    if (Array.IndexOf(anIds, m_anIDs[i]) < 0) continue;
                    if (bKeep) { h[i] = afPose[i]; g[i] = afPose[i]; }
                    m_C3d.SetData(m_anIDs[i], afPose[i]);          // 켜는 축의 3D = 지금 자세 (붙잡은 자세를 모름으로 둘 때도 보여 주기는 맞게 — 보내지는 않는다)
                    lstIdx.Add(i);
                }
                m_afHoldSim = h; m_afGoalSim = g;
                if (bComp && lstIdx.Count > 0)
                {
                    // 켜는 축: 지금 위치에서 보정 0 부터 (방향 추적도 그 자세에서 새로), 켜져 있던 축: 걸린 보정에서 이어서 — 다음 틱 = 서보 목표
                    int[] sid = new int[lstIdx.Count]; float[] sh = new float[lstIdx.Count], sf = new float[lstIdx.Count], ss = new float[lstIdx.Count];
                    for (int k = 0; k < lstIdx.Count; k++) { sid[k] = m_anIDs[lstIdx[k]]; sh[k] = afPose[lstIdx[k]]; }
                    m_C3d.SagComp_SeedFrom(sid, sh, sf, ss);
                }
                ProfileAccel_Apply();
                var lstOn = new List<int>();
                foreach (int id in anIds)
                {
                    if (Array.IndexOf(m_anIDs, id) < 0) continue;
                    m_Com.Send(id, 3, 64, (byte)1); System.Threading.Thread.Sleep(5);   // addr 64 = Torque Enable — 꺼진 축에만
                    lstOn.Add(id);
                }
                m_bTorque = true;
                m_Com.Stream_Reset();
                Log("[COMM] 토크 꺼진 축만 켬: ID {0} — 켜진 축은 붙잡은 목표 그대로\r\n", string.Join(",", lstOn.ConvertAll(x => x.ToString()).ToArray()));
            }

            /// <summary>연결을 닫고(close — 토크는 그대로), 모션 싱크가 아직 이 장면의 것이면 생성 전에 걸려 있던 싱크로 되돌린다
            /// (다른 것이 바꿔 놓았으면 건드리지 않는다). 되돌릴 싱크가 이미 Release 된 장면의 것이면 그 장면이 기억한 이전 싱크로 거슬러 올라간다</summary>
            public void Release()
            {
                close();
                m_bReleased = true;
                Action<int> cur = m_C3d.MoveJoints_GetMotionSink();
                if (cur == null || cur != m_Sink) return;
                Action<int> prev = m_PrevSink;
                for (int n = 0; n < 64 && prev != null; n++)
                {
                    CScene_t sc = prev.Target as CScene_t;
                    if (sc == null || !sc.m_bReleased) break;
                    prev = sc.m_PrevSink;
                }
                m_C3d.MoveJoints_SetMotionSink(prev);
            }
        }
    }
}
