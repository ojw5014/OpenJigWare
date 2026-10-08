using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenJigWare
{
    /// <summary>
    /// EDH 원문 엄격 구문 검증기.
    /// MakeDHSkeleton_Urdf 의 관대한 파서와 달리, 필드 수·숫자 형식·괄호 구조를
    /// 사전 검사하여 오타가 조용히 다른 로봇으로 변환되는 것(silent failure)을 차단한다.
    /// 문법 근거: 내장 예제 E1~E13 전 라인 (2026-07-02 전수 조사).
    ///   1) 빈 줄 / "//" 주석            → 통과
    ///   2) "!"                          → 통과 (지시자)
    ///   3) "#N"                         → 함수 라인 (N=정수)
    ///   4) "$name, f, f, ..."           → 지시자 ($camera, $mech 등, 이후 전부 숫자)
    ///   5) "@..." 외형 라인             → 그룹 1 = 파일명(접두사 ?,?!,/,/! 허용) 또는 #N,
    ///                                     이후 토큰·그룹 전부 숫자 (그룹 끝 빈 토큰 1개 허용,
    ///                                     E10 "[0,7.5,]" 관례), 괄호 없는 구형 1그룹 허용(E6)
    ///   6) "[a,d,th,al],[axis,dir,init(,type,num)](,[g,ik])" 코어 DH
    ///                                   → 그룹1=4필드, 그룹2=3|5필드, 그룹3(선택)=2필드, 전부 숫자
    /// </summary>
    public static class CEdhValidator
    {
        /// <summary>전체 EDH 텍스트 검증. 오류 목록 반환(비면 통과).</summary>
        public static List<string> Validate(string edhText)
        {
            List<string> errors = new List<string>();
            if (edhText == null) { errors.Add("입력이 null"); return errors; }
            string[] lines = edhText.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i].Trim();
                if (raw.Length == 0) continue;
                // 인라인 주석 제거
                int c = raw.IndexOf("//", StringComparison.Ordinal);
                string s = (c >= 0) ? raw.Substring(0, c).Trim() : raw;
                if (s.Length == 0) continue;

                string err = ValidateLine(s);
                if (err != null)
                    errors.Add(string.Format("라인 {0}: {1} | \"{2}\"", i + 1, err,
                        (raw.Length > 60) ? raw.Substring(0, 60) + "..." : raw));
            }
            return errors;
        }

        private static string ValidateLine(string s)
        {
            if (s == "!") return null;
            char c0 = s[0];
            if (c0 == '$') return ValidateDollar(s);
            if (c0 == '#') return ValidateFunc(s);
            if (c0 == '@') return ValidateAt(s);
            if (c0 == '[') return ValidateCore(s);
            return "알 수 없는 라인 형식";
        }

        // ── "#N" ───────────────────────────────────────────
        private static string ValidateFunc(string s)
        {
            int n;
            if (!int.TryParse(s.Substring(1).Trim(), NumberStyles.Integer,
                              CultureInfo.InvariantCulture, out n))
                return "#함수 번호가 정수가 아님";
            return null;
        }

        // ── "$name, f, f, ..." ─────────────────────────────
        //   기본 규칙: 이름 뒤 인자는 전부 숫자 ($camera 등).
        //   예외 (P1 물리 지시자 — 문자열 인자 허용):
        //     $servo, <축ID|T축ID>, <모델명>[, <메시파일명>]
        //     $jointrange, <축ID|T축ID>, <하한deg>, <상한deg>
        //     $extramass, <메시파일명>, <그램>
        private static string ValidateDollar(string s)
        {
            string[] tok = s.Split(',');
            string head = tok[0].Trim();          // "$camera" 등
            if (head.Length < 2) return "$지시자 이름 없음";
            for (int i = 1; i < head.Length; i++)
                if (!char.IsLetterOrDigit(head[i]) && head[i] != '_')
                    return "$지시자 이름에 허용되지 않는 문자";
            if (tok.Length < 2) return "$지시자 인자 없음";

            string name = head.Substring(1).ToLowerInvariant();
            if (name == "servo")
            {
                if (tok.Length < 3 || tok.Length > 4)
                    return string.Format("$servo 인자 수 {0} (2 또는 3이어야 함)", tok.Length - 1);
                if (!IsAxisToken(tok[1])) return "$servo 1번째 인자가 축ID(정수 또는 T정수)가 아님";
                string err = CheckNameChars(tok[2], "-_.");
                if (err != null) return "$servo 모델명: " + err;
                if (tok.Length == 4)
                {
                    err = CheckNameChars(tok[3], "-_.+");
                    if (err != null) return "$servo 메시파일명: " + err;
                }
                return null;
            }
            if (name == "jointrange")
            {
                if (tok.Length != 4)
                    return string.Format("$jointrange 인자 수 {0} (3이어야 함)", tok.Length - 1);
                if (!IsAxisToken(tok[1])) return "$jointrange 1번째 인자가 축ID(정수 또는 T정수)가 아님";
                if (!IsFloat(tok[2])) return "$jointrange 하한이 숫자가 아님";
                if (!IsFloat(tok[3])) return "$jointrange 상한이 숫자가 아님";
                return null;
            }
            if (name == "footcontact")
            {
                // $footcontact, 반길이, 반폭, 반두께, x, y, z [, roll, pitch, yaw [, color]]   (mm/deg)
                if (tok.Length != 7 && tok.Length != 10 && tok.Length != 11)
                    return string.Format("$footcontact 인자 수 {0} (6 또는 9 또는 10 이어야 함)", tok.Length - 1);
                for (int i = 1; i <= 9 && i < tok.Length; i++)
                    if (!IsFloat(tok[i])) return string.Format("$footcontact {0}번째 인자가 숫자가 아님", i);
                for (int i = 1; i <= 3; i++)
                {
                    double dv;
                    if (double.TryParse(tok[i], System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out dv) && dv <= 0)
                        return string.Format("$footcontact {0}번째(반치수)는 0보다 커야 함", i);
                }
                return null;
            }
            if (name == "extramass")
            {
                if (tok.Length != 3)
                    return string.Format("$extramass 인자 수 {0} (2여야 함)", tok.Length - 1);
                string err = CheckNameChars(tok[1], "-_.+");
                if (err != null) return "$extramass 메시파일명: " + err;
                if (!IsFloat(tok[2])) return "$extramass 그램이 숫자가 아님";
                return null;
            }

            for (int i = 1; i < tok.Length; i++)
                if (!IsFloat(tok[i])) return string.Format("$지시자 {0}번째 인자가 숫자가 아님", i);
            return null;
        }

        /// <summary>축 토큰: "13" 또는 "T13" (대소문자 무관).</summary>
        private static bool IsAxisToken(string s)
        {
            string t = s.Trim();
            if (t.Length == 0) return false;
            if (t[0] == 'T' || t[0] == 't') t = t.Substring(1);
            int n;
            return int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= 0;
        }

        /// <summary>영숫자 + 지정 특수문자만 허용하는 이름 검사. 통과 시 null.</summary>
        private static string CheckNameChars(string s, string extra)
        {
            string t = s.Trim();
            if (t.Length == 0) return "비어 있음";
            for (int i = 0; i < t.Length; i++)
            {
                char ch = t[i];
                if (!char.IsLetterOrDigit(ch) && extra.IndexOf(ch) < 0)
                    return string.Format("허용되지 않는 문자 '{0}'", ch);
            }
            return null;
        }

        // ── 코어 DH ────────────────────────────────────────
        private static string ValidateCore(string s)
        {
            List<List<string>> g;
            string err = ParseGroups(s, out g);
            if (err != null) return err;
            if (g.Count < 2 || g.Count > 3)
                return string.Format("코어 DH 그룹 수 {0} (2 또는 3이어야 함)", g.Count);
            if (g[0].Count != 4) return string.Format("DH 필드 수 {0} (4여야 함)", g[0].Count);
            if (g[1].Count != 3 && g[1].Count != 5)
                return string.Format("확장 필드 수 {0} (3 또는 5여야 함)", g[1].Count);
            if (g.Count == 3 && g[2].Count != 2)
                return string.Format("세 번째 그룹 필드 수 {0} (2여야 함)", g[2].Count);
            for (int i = 0; i < g.Count; i++)
                for (int j = 0; j < g[i].Count; j++)
                    if (!IsFloat(g[i][j]))
                        return string.Format("그룹 {0}의 {1}번째 필드가 숫자가 아님", i + 1, j + 1);
            return null;
        }

        // ── "@" 외형 라인 ──────────────────────────────────
        private static string ValidateAt(string s)
        {
            string body = s.Substring(1).Trim();
            if (body.Length == 0) return "@ 뒤에 내용 없음";
            if (body[0] != '[') body = "[" + body;   // 구형: 여는 괄호 생략 (E6)

            List<List<string>> g;
            string err = ParseGroups(body, out g);
            if (err != null) return err;
            if (g.Count < 1 || g.Count > 6)
                return string.Format("@ 그룹 수 {0} (1~6이어야 함)", g.Count);

            // 그룹 1: [이름(, 숫자...)]
            List<string> g1 = g[0];
            if (g1.Count < 1 || g1.Count > 5)
                return string.Format("@ 그룹1 필드 수 {0} (1~5여야 함)", g1.Count);
            string name = g1[0].Trim();
            // 트랙 접두사 제거
            foreach (string pre in new string[] { "?!", "/!", "?", "/" })
                if (name.StartsWith(pre, StringComparison.Ordinal))
                { name = name.Substring(pre.Length); break; }
            if (name.Length == 0) return "@ 이름이 비어 있음";
            if (name[0] == '#')
            {
                int prim;
                if (!int.TryParse(name.Substring(1), NumberStyles.Integer,
                                  CultureInfo.InvariantCulture, out prim))
                    return "@ 기본 도형 번호(#N)가 정수가 아님";
            }
            else
            {
                for (int i = 0; i < name.Length; i++)
                {
                    char ch = name[i];
                    if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_' && ch != '-' && ch != '+')
                        return string.Format("@ 파일명에 허용되지 않는 문자 '{0}'", ch);
                }
            }
            for (int j = 1; j < g1.Count; j++)
                if (!IsFloat(g1[j]))
                    return string.Format("@ 그룹1의 {0}번째 필드가 숫자가 아님", j + 1);

            // 그룹 2~N: 전부 숫자 (그룹 끝 빈 토큰 1개 허용)
            for (int i = 1; i < g.Count; i++)
            {
                List<string> gi = g[i];
                if (gi.Count < 1 || gi.Count > 8)
                    return string.Format("@ 그룹{0} 필드 수 {1} (1~8이어야 함)", i + 1, gi.Count);
                for (int j = 0; j < gi.Count; j++)
                {
                    string t = gi[j].Trim();
                    if (t.Length == 0)
                    {
                        if (j == gi.Count - 1) continue;   // 끝 빈 토큰 허용 (E10)
                        return string.Format("@ 그룹{0} 중간에 빈 필드", i + 1);
                    }
                    if (!IsFloat(t))
                        return string.Format("@ 그룹{0}의 {1}번째 필드가 숫자가 아님", i + 1, j + 1);
                }
            }
            return null;
        }

        // ── 공통: "[...],[...],..." 스캐너 ─────────────────
        private static string ParseGroups(string s, out List<List<string>> groups)
        {
            groups = new List<List<string>>();
            int i = 0, n = s.Length;
            while (true)
            {
                while (i < n && char.IsWhiteSpace(s[i])) i++;
                if (i >= n || s[i] != '[')
                    return string.Format("위치 {0}: '['가 와야 함", i + 1);
                int close = s.IndexOf(']', i + 1);
                if (close < 0) return "닫는 ']' 없음";
                string inner = s.Substring(i + 1, close - i - 1);
                List<string> tok = new List<string>(inner.Split(','));
                groups.Add(tok);
                i = close + 1;
                while (i < n && char.IsWhiteSpace(s[i])) i++;
                if (i >= n) return null;                    // 정상 종료
                if (s[i] != ',')
                    return string.Format("위치 {0}: 그룹 사이에 ','가 와야 함", i + 1);
                i++;                                        // ',' 소비 후 다음 그룹
            }
        }

        private static bool IsFloat(string s)
        {
            double d;
            return double.TryParse(s.Trim(), NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out d);
        }
    }
}
