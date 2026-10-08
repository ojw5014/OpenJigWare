using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
        /// <summary>
        /// 3D 시뮬레이터 소켓 브리지 — Isaac Sim / PyBullet / MuJoCo 뷰어와 통신.
        ///
        /// 상대는 isaac_runner.py 의 TCP 서버(기본 127.0.0.1:4000). 프로토콜:
        ///   관절 명령 : "time_ms,delay,id1,val1,id2,val2,...;"
        ///   리셋      : "reset;" / "clear;"
        ///   조회      : "GETPOS[,robot_idx];"  =&gt;  "POS,id,angle,...;"
        ///
        /// 관절 id 는 뷰어 URDF 의 조인트 이름 T{id} 와 1:1 대응한다
        /// (예: id 11 =&gt; joint "T11"). 각도 단위는 도(deg), prismatic 은 mm.
        ///
        /// 서버가 "연결 1회 = 명령 1회" 방식이라 매 호출마다 connect-send-close 한다.
        /// ※ 주의: 호출 간격을 20ms 미만으로 두지 말 것 — TIME_WAIT 소켓 고갈 사고 이력.
        ///   연속 스트리밍은 100ms 정도로 스로틀하고 time_ms(보간시간)를 그 간격에 맞춘다.
        ///
        /// 앱(ojwSimul / MakeUrdf 등)은 이 함수들을 "사용만" 한다.
        /// </summary>
        public class CSimBridge
        {
            public const string DEFAULT_IP = "127.0.0.1";
            public const int DEFAULT_PORT = 4000;
            private const int REPLY_TIMEOUT_MS = 700;

            /// <summary>마지막 오류 메시지(진단용).</summary>
            public static string LastError = "";

            // ================================================================
            // payload 생성
            // ================================================================

            /// <summary>
            /// 관절 명령 문자열 생성: "timeMs,delayMs,id1,deg1,id2,deg2,...;"
            /// 각도는 InvariantCulture 고정 소수 2자리 — 한국 로캘의 소수점 콤마로
            /// 필드가 깨지는 것을 막는다.
            /// </summary>
            public static string BuildJointPayload(int nTimeMs, int nDelayMs, int[] anIds, float[] afDegs)
            {
                if (anIds == null || afDegs == null) return "";
                int n = Math.Min(anIds.Length, afDegs.Length);
                if (n <= 0) return "";

                StringBuilder sb = new StringBuilder();
                sb.Append(nTimeMs).Append(",").Append(nDelayMs);
                for (int i = 0; i < n; i++)
                {
                    sb.Append(",").Append(anIds[i]).Append(",")
                      .Append(afDegs[i].ToString("F2", CultureInfo.InvariantCulture));
                }
                sb.Append(";");
                return sb.ToString();
            }

            // ================================================================
            // 전송 (응답 없음)
            // ================================================================

            public static bool SendRaw(string strPayload)
            {
                return SendRaw(DEFAULT_IP, DEFAULT_PORT, strPayload);
            }

            /// <summary>payload 를 그대로 1회 전송(connect-send-close). 실패 시 false.</summary>
            public static bool SendRaw(string strIp, int nPort, string strPayload)
            {
                if (string.IsNullOrEmpty(strPayload)) { LastError = "empty payload"; return false; }
                CSocket sock = new CSocket();
                try
                {
                    if (sock.Connect(strIp, nPort) == false)
                    {
                        LastError = "connect failed (viewer not running?)";
                        return false;
                    }
                    bool bOk = sock.SendString(strPayload);
                    if (bOk == false) LastError = "send failed";
                    else LastError = "";
                    return bOk;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return false;
                }
                finally
                {
                    try { sock.DisConnect(); }
                    catch { }
                }
            }

            // ================================================================
            // 관절 전송
            // ================================================================

            /// <summary>기본 주소(127.0.0.1:4000)로 관절각 전송. nTimeMs = 뷰어 보간 시간.</summary>
            public static bool SendJoints(int nTimeMs, int[] anIds, float[] afDegs)
            {
                return SendJoints(DEFAULT_IP, DEFAULT_PORT, nTimeMs, 0, anIds, afDegs);
            }

            public static bool SendJoints(string strIp, int nPort, int nTimeMs, int nDelayMs,
                                          int[] anIds, float[] afDegs)
            {
                string strPayload = BuildJointPayload(nTimeMs, nDelayMs, anIds, afDegs);
                if (strPayload.Length == 0) { LastError = "no joints"; return false; }
                return SendRaw(strIp, nPort, strPayload);
            }

            /// <summary>뷰어 로봇 자세 리셋.</summary>
            public static bool SendReset()
            {
                return SendRaw(DEFAULT_IP, DEFAULT_PORT, "reset;");
            }

            // ================================================================
            // 조회 (요청 =&gt; 응답)
            // ================================================================

            /// <summary>요청을 보내고 같은 연결에서 세미콜론으로 끝나는 응답을 읽어 반환. 실패 시 빈 문자열.</summary>
            public static string Query(string strIp, int nPort, string strPayload)
            {
                CSocket sock = new CSocket();
                try
                {
                    if (sock.Connect(strIp, nPort) == false)
                    {
                        LastError = "connect failed (viewer not running?)";
                        return "";
                    }
                    if (sock.SendString(strPayload) == false)
                    {
                        LastError = "send failed";
                        return "";
                    }
                    string strRes = sock.GetStringUntil(SEMICOLON, REPLY_TIMEOUT_MS);
                    if (string.IsNullOrEmpty(strRes)) LastError = "no reply (timeout)";
                    else LastError = "";
                    return strRes;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return "";
                }
                finally
                {
                    try { sock.DisConnect(); }
                    catch { }
                }
            }

            private const char SEMICOLON = (char)59;   // ";"
            private const char COMMA = (char)44;       // ","

            /// <summary>뷰어의 현재 관절값 조회. 응답 "POS,id,angle,...;" 파싱.</summary>
            public static bool GetPos(out int[] anIds, out float[] afDegs)
            {
                return GetPos(DEFAULT_IP, DEFAULT_PORT, 0, out anIds, out afDegs);
            }

            public static bool GetPos(string strIp, int nPort, int nRobotIdx,
                                      out int[] anIds, out float[] afDegs)
            {
                anIds = null; afDegs = null;
                string strReq = (nRobotIdx <= 0) ? "GETPOS;"
                                                 : string.Format("GETPOS,{0};", nRobotIdx);
                string strRes = Query(strIp, nPort, strReq);
                if (string.IsNullOrEmpty(strRes)) return false;

                int nEnd = strRes.IndexOf(SEMICOLON);
                if (nEnd >= 0) strRes = strRes.Substring(0, nEnd);
                string[] p = strRes.Split(COMMA);
                if (p.Length < 3 || p[0].Trim().ToUpper() != "POS")
                {
                    LastError = "bad reply: " + strRes;
                    return false;
                }

                List<int> ids = new List<int>();
                List<float> degs = new List<float>();
                for (int i = 1; i + 1 < p.Length; i += 2)
                {
                    int id; float deg;
                    if (int.TryParse(p[i].Trim(), out id) == false) continue;
                    if (float.TryParse(p[i + 1].Trim(), NumberStyles.Float,
                                       CultureInfo.InvariantCulture, out deg) == false) continue;
                    ids.Add(id); degs.Add(deg);
                }
                if (ids.Count == 0) { LastError = "no joints in reply"; return false; }
                anIds = ids.ToArray();
                afDegs = degs.ToArray();
                return true;
            }

            // ================================================================
            // 연결 확인
            // ================================================================

            public static bool IsAlive() { return IsAlive(DEFAULT_IP, DEFAULT_PORT); }

            /// <summary>뷰어가 떠 있는지(포트가 열려 있는지) 짧게 확인.</summary>
            public static bool IsAlive(string strIp, int nPort)
            {
                CSocket sock = new CSocket();
                try
                {
                    return sock.Connect(strIp, nPort);
                }
                catch { return false; }
                finally
                {
                    try { sock.DisConnect(); }
                    catch { }
                }
            }
        }
    }
}
