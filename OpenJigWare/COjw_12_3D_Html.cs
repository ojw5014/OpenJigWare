// ================================================================
// C3d — HTML 생성·구문 하이라이팅 (2026-09-03 MakeUrdf/Main.cs 에서 이관)
//
// 왜 옮겼나: 이 함수들은 UI 를 하나도 건드리지 않는 **순수 문자열 함수**인데
//   앱에 있었다. 사용자 원칙 — "중요 기능은 모두 OpenJigWare.dll 에" — 위반이고,
//   같은 문법(URDF/USDA/EDH)을 앱과 DLL 이 각각 해석하는 구조는 이미
//   실사고 2건을 냈다(COjw_12_3D.cs 의 Edh_* 이관 주석 참조).
//
// 이관 원칙: **로직은 한 줄도 바꾸지 않았다.** 바꾼 것은 셋뿐이다 —
//   ① private → private static,  ② List<DhGroup> → List<CDhGroup>,
//   ③ GenerateFormulaHtml 이 파일을 쓰던 것을 문자열 반환으로 (파일·브라우저는 앱 몫).
// ================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            /// <summary>HTML 이스케이프 (외부 공개판).</summary>
            public static string Html_Escape(string s) { return Esc(s); }

            /// <summary>USDA 한 줄 → HTML 구문 하이라이팅.</summary>
            public static string Html_ColorizeUsdaLine(string line) { return ColorizeUsdaLine(line); }

            /// <summary>XML(URDF/MJCF) 한 줄 → HTML 구문 하이라이팅.</summary>
            public static string Html_ColorizeXmlLine(string line) { return ColorizeXmlLine(line); }

            private static string Esc(string s)
            {
                return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
            }
            /// <summary>USDA 한 줄을 HTML 구문 하이라이팅 (심플 토큰 스캐너).</summary>
            private static string ColorizeUsdaLine(string line)
            {
                // 대문자로 시작하는 USDA 키워드 및 타입 집합
                var kwSet = new HashSet<string> {
                    "def","over","class","prepend","append","uniform","varying","rel","add","delete","reorder",
                    "custom","references","payload","subLayers","defaultPrim","metersPerUnit","upAxis",
                    "apiSchemas","physics","drive"
                };
                var tySet = new HashSet<string> {
                    "Xform","Sphere","Cylinder","Cube","Cone","Mesh","Camera","Scope","Material","Shader",
                    "PhysicsScene","PhysicsFixedJoint","PhysicsRevoluteJoint","PhysicsPrismaticJoint","PhysicsMeshCollisionAPI",
                    "PhysicsRigidBodyAPI","PhysicsMassAPI","PhysicsArticulationRootAPI",
                    "float","double","int","token","string","bool","asset","point3f","vector3f","color3f","quatf","matrix4d",
                    "double3","float3","float4","true","false"
                };
                StringBuilder sb = new StringBuilder();
                int pos = 0;
                int len = line.Length;
                while (pos < len)
                {
                    char ch = line[pos];
                    // 주석 # ... 끝까지
                    if (ch == '#')
                    {
                        sb.AppendFormat("<span class=\"cmt\">{0}</span>", Esc(line.Substring(pos)));
                        break;
                    }
                    // 문자열 리터럴 "..." (에스케이프 미지원 — USDA 실무 충분)
                    if (ch == '"')
                    {
                        int end = line.IndexOf('"', pos + 1);
                        if (end < 0) end = len - 1;
                        sb.AppendFormat("<span class=\"st\">{0}</span>", Esc(line.Substring(pos, end + 1 - pos)));
                        pos = end + 1;
                        continue;
                    }
                    // asset 리터럴 @...@
                    if (ch == '@')
                    {
                        int end = line.IndexOf('@', pos + 1);
                        if (end < 0) end = len - 1;
                        sb.AppendFormat("<span class=\"st\">{0}</span>", Esc(line.Substring(pos, end + 1 - pos)));
                        pos = end + 1;
                        continue;
                    }
                    // 숫자 (±소수 포함)
                    if (char.IsDigit(ch) || (ch == '-' && pos + 1 < len && (char.IsDigit(line[pos + 1]) || line[pos + 1] == '.')))
                    {
                        int start = pos;
                        if (ch == '-') pos++;
                        while (pos < len && (char.IsDigit(line[pos]) || line[pos] == '.' || line[pos] == 'e' || line[pos] == 'E' || line[pos] == '+' || line[pos] == '-'))
                        {
                            // -는 지수부에서만
                            if (line[pos] == '-' && (pos == start || (line[pos-1] != 'e' && line[pos-1] != 'E'))) break;
                            pos++;
                        }
                        sb.AppendFormat("<span class=\"num\">{0}</span>", Esc(line.Substring(start, pos - start)));
                        continue;
                    }
                    // 식별자
                    if (char.IsLetter(ch) || ch == '_')
                    {
                        int start = pos;
                        while (pos < len && (char.IsLetterOrDigit(line[pos]) || line[pos] == '_' || line[pos] == ':')) pos++;
                        string tok = line.Substring(start, pos - start);
                        string cls;
                        if (kwSet.Contains(tok)) cls = "kw";
                        else if (tySet.Contains(tok)) cls = "ty";
                        else if (tok.Contains(":")) cls = "at";   // xformOp:translate 같은 attribute 경로
                        else cls = "at";
                        sb.AppendFormat("<span class=\"{0}\">{1}</span>", cls, Esc(tok));
                        continue;
                    }
                    // 구두점 / 공백
                    if ("(){}[],=/<>".IndexOf(ch) >= 0)
                    {
                        sb.AppendFormat("<span class=\"pnc\">{0}</span>", Esc(ch.ToString()));
                    }
                    else
                    {
                        sb.Append(Esc(ch.ToString()));
                    }
                    pos++;
                }
                return sb.ToString();
            }
            /// <summary>XML 한 줄을 HTML 구문 하이라이팅</summary>
            private static string ColorizeXmlLine(string line)
            {
                StringBuilder sb = new StringBuilder();
                int pos = 0;
                int len = line.Length;

                while (pos < len)
                {
                    // 주석: <!-- ... -->
                    if (pos + 3 < len && line[pos] == '<' && line[pos + 1] == '!'
                        && line[pos + 2] == '-' && line[pos + 3] == '-')
                    {
                        int end = line.IndexOf("-->", pos + 4);
                        if (end < 0) end = len - 3;
                        string comment = line.Substring(pos, end + 3 - pos);
                        sb.AppendFormat("<span class=\"cmt\">{0}</span>", Esc(comment));
                        pos = end + 3;
                        continue;
                    }

                    // XML 선언: <?xml ... ?>
                    if (pos + 1 < len && line[pos] == '<' && line[pos + 1] == '?')
                    {
                        int end = line.IndexOf("?>", pos + 2);
                        if (end < 0) end = len - 2;
                        string decl = line.Substring(pos, end + 2 - pos);
                        sb.AppendFormat("<span class=\"kw\">{0}</span>", Esc(decl));
                        pos = end + 2;
                        continue;
                    }

                    // 닫는 태그: </tagname>
                    if (pos + 1 < len && line[pos] == '<' && line[pos + 1] == '/')
                    {
                        int end = line.IndexOf('>', pos + 2);
                        if (end < 0) end = len - 1;
                        string tagName = line.Substring(pos + 2, end - pos - 2).Trim();
                        sb.AppendFormat("<span class=\"pnc\">&lt;/</span><span class=\"tag\">{0}</span><span class=\"pnc\">&gt;</span>",
                            Esc(tagName));
                        pos = end + 1;
                        continue;
                    }

                    // 여는 태그: <tagname attr="val" ...> 또는 <tagname ... />
                    if (line[pos] == '<' && pos + 1 < len && char.IsLetter(line[pos + 1]))
                    {
                        // 태그 이름 추출
                        int nameStart = pos + 1;
                        int nameEnd = nameStart;
                        while (nameEnd < len && !char.IsWhiteSpace(line[nameEnd])
                               && line[nameEnd] != '>' && line[nameEnd] != '/')
                            nameEnd++;

                        string tagName = line.Substring(nameStart, nameEnd - nameStart);
                        sb.AppendFormat("<span class=\"pnc\">&lt;</span><span class=\"tag\">{0}</span>", Esc(tagName));

                        pos = nameEnd;

                        // 속성 파싱
                        while (pos < len && line[pos] != '>' && !(line[pos] == '/' && pos + 1 < len && line[pos + 1] == '>'))
                        {
                            // 공백 건너뛰기
                            if (char.IsWhiteSpace(line[pos]))
                            {
                                sb.Append(line[pos]);
                                pos++;
                                continue;
                            }

                            // 속성이름=값
                            if (char.IsLetter(line[pos]) || line[pos] == '_')
                            {
                                int aStart = pos;
                                while (pos < len && line[pos] != '=' && line[pos] != '>'
                                       && !char.IsWhiteSpace(line[pos]) && line[pos] != '/')
                                    pos++;
                                string attrName = line.Substring(aStart, pos - aStart);
                                sb.AppendFormat("<span class=\"atn\">{0}</span>", Esc(attrName));

                                // = 부호
                                if (pos < len && line[pos] == '=')
                                {
                                    sb.Append("<span class=\"pnc\">=</span>");
                                    pos++;

                                    // 값 (따옴표)
                                    if (pos < len && (line[pos] == '"' || line[pos] == '\''))
                                    {
                                        char q = line[pos];
                                        int vEnd = line.IndexOf(q, pos + 1);
                                        if (vEnd < 0) vEnd = len - 1;
                                        string val = line.Substring(pos, vEnd + 1 - pos);
                                        sb.AppendFormat("<span class=\"atv\">{0}</span>", Esc(val));
                                        pos = vEnd + 1;
                                    }
                                }
                                continue;
                            }

                            // 기타 문자
                            sb.Append(Esc(line[pos].ToString()));
                            pos++;
                        }

                        // 자기닫기 /> 또는 >
                        if (pos < len && line[pos] == '/' && pos + 1 < len && line[pos + 1] == '>')
                        {
                            sb.Append("<span class=\"pnc\">/&gt;</span>");
                            pos += 2;
                        }
                        else if (pos < len && line[pos] == '>')
                        {
                            sb.Append("<span class=\"pnc\">&gt;</span>");
                            pos++;
                        }
                        continue;
                    }

                    // 텍스트 내용 (태그 밖)
                    int nextTag = line.IndexOf('<', pos);
                    if (nextTag < 0) nextTag = len;
                    if (nextTag > pos)
                    {
                        sb.Append(Esc(line.Substring(pos, nextTag - pos)));
                        pos = nextTag;
                    }
                    else
                    {
                        sb.Append(Esc(line[pos].ToString()));
                        pos++;
                    }
                }

                return sb.ToString();
            }
            /// <summary>수학 수식에 구문 하이라이팅 적용</summary>
            private static string ColorizeFormulaExpr(string expr)
            {
                if (string.IsNullOrEmpty(expr)) return "";

                StringBuilder sb = new StringBuilder();
                int pos = 0;
                int len = expr.Length;

                while (pos < len)
                {
                    char ch = expr[pos];

                    // C( 또는 S( → 함수 하이라이팅
                    if ((ch == 'C' || ch == 'S') && pos + 1 < len && expr[pos + 1] == '(')
                    {
                        sb.AppendFormat("<span class=\"fn\">{0}</span>", ch);
                        pos++;
                        continue;
                    }

                    // t 뒤에 숫자 → 모터 변수
                    if (ch == 't' && pos + 1 < len && char.IsDigit(expr[pos + 1]))
                    {
                        sb.Append("<span class=\"var\">t");
                        pos++;
                        while (pos < len && char.IsDigit(expr[pos]))
                        {
                            sb.Append(expr[pos]);
                            pos++;
                        }
                        sb.Append("</span>");
                        continue;
                    }

                    // 숫자 (정수/소수)
                    if (char.IsDigit(ch))
                    {
                        sb.Append("<span class=\"num\">");
                        while (pos < len && (char.IsDigit(expr[pos]) || expr[pos] == '.'))
                        {
                            sb.Append(expr[pos]);
                            pos++;
                        }
                        sb.Append("</span>");
                        continue;
                    }

                    // 그 외: HTML 이스케이프 후 출력
                    if (ch == '<') sb.Append("&lt;");
                    else if (ch == '>') sb.Append("&gt;");
                    else if (ch == '&') sb.Append("&amp;");
                    else sb.Append(ch);
                    pos++;
                }

                return sb.ToString();
            }
            /// <summary>4x4 행렬을 HTML 테이블로 렌더링</summary>
            private static void WriteMatrixHtml(StringBuilder h, List<string[]> rows)
            {
                h.AppendLine("<div class=\"mtx-wrap\">");
                h.Append("<span class=\"bracket\">[</span>");
                h.AppendLine("<table class=\"mtx\">");
                foreach (string[] row in rows)
                {
                    h.Append("<tr>");
                    foreach (string cell in row)
                        h.AppendFormat("<td>{0}</td>", ColorizeFormulaExpr(cell));
                    h.AppendLine("</tr>");
                }
                h.AppendLine("</table>");
                h.AppendLine("<span class=\"bracket\">]</span>");
                h.AppendLine("</div>");
            }
            /// <summary>행렬 체인 파싱 + 개별 T행렬/곱셈 과정 HTML 생성</summary>
            private static void GenerateMatrixSections(StringBuilder h, string matrixChain)
            {
                if (string.IsNullOrEmpty(matrixChain)) return;

                string[] chunks = matrixChain.Split(new[] { "==============" }, StringSplitOptions.RemoveEmptyEntries);

                bool wroteTMatrixHeader = false;
                bool wroteMultHeader = false;
                int tMatrixIdx = 0;
                int multIdx = 0;

                foreach (string chunk in chunks)
                {
                    string[] cLines = chunk.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    string label = "";
                    List<string[]> rows = new List<string[]>();

                    foreach (string rawLine in cLines)
                    {
                        string line = rawLine.Trim();
                        if (line.Length == 0) continue;
                        if (line.StartsWith("//"))
                        {
                            label = line.TrimStart('/').TrimEnd('/').Trim();
                            continue;
                        }

                        // 행 파싱: 콤마 또는 3+공백 구분
                        List<string> cells = new List<string>();
                        if (line.Contains(","))
                        {
                            foreach (string c in line.Split(','))
                            {
                                string t = c.Trim();
                                if (t.Length > 0) cells.Add(t);
                            }
                        }
                        else
                        {
                            foreach (string c in line.Split(new[] { "   " }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                string t = c.Trim();
                                if (t.Length > 0) cells.Add(t);
                            }
                        }
                        if (cells.Count > 0)
                            rows.Add(cells.ToArray());
                    }

                    if (rows.Count == 0) continue;

                    // 섹션 분류
                    if (label.Contains("Start"))
                    {
                        // 항등 행렬 - 건너뜀
                        continue;
                    }
                    else if (label.StartsWith("--") && !label.Contains("First"))
                    {
                        // 개별 T 행렬
                        if (!wroteTMatrixHeader)
                        {
                            h.AppendLine("<h2>3. \uac1c\ubcc4 DH \ubcc0\ud658 \ud589\ub82c (T Matrices)</h2>");
                            h.AppendLine("<p style=\"color:#888;font-size:13px;margin-bottom:8px\">\uac01 DH \ud30c\ub77c\ubbf8\ud130 \ud55c \uc904\uc774 \ud558\ub098\uc758 4\u00d74 \ubcc0\ud658 \ud589\ub82c\uc744 \ub9cc\ub4ed\ub2c8\ub2e4.</p>");
                            wroteTMatrixHeader = true;
                        }
                        h.AppendFormat("<h3>T<sub>{0}</sub></h3>\n", tMatrixIdx);
                        WriteMatrixHtml(h, rows);
                        tMatrixIdx++;
                    }
                    else
                    {
                        // 곱셈 과정
                        if (!wroteMultHeader)
                        {
                            h.AppendLine("<h2>4. \ud589\ub82c \uacf1\uc148 \uacfc\uc815 (Matrix Multiplication)</h2>");
                            h.AppendLine("<p style=\"color:#888;font-size:13px;margin-bottom:8px\">\ubcc0\ud658 \ud589\ub82c\uc744 \uc21c\uc11c\ub300\ub85c \uacf1\ud558\uc5ec \ucd5c\uc885 \uacb0\uacfc\ub97c \ub9cc\ub4ed\ub2c8\ub2e4: T<sub>final</sub> = T<sub>0</sub> \u00d7 T<sub>1</sub> \u00d7 ... \u00d7 T<sub>n</sub></p>");
                            wroteMultHeader = true;
                        }

                        string stepLabel;
                        if (label.Contains("First"))
                            stepLabel = "T<sub>0</sub> \u00d7 T<sub>1</sub>";
                        else
                        {
                            int stepNum;
                            if (int.TryParse(label.Trim(), out stepNum))
                                stepLabel = string.Format("(\u2026) \u00d7 T<sub>{0}</sub>", stepNum + 1);
                            else
                                stepLabel = "Step " + multIdx;
                        }
                        h.AppendFormat("<h3>{0} =</h3>\n", stepLabel);
                        WriteMatrixHtml(h, rows);
                        multIdx++;
                    }
                }
            }
            /// <summary>DH 파라미터 텍스트 → HTML 테이블 (그룹 구분 포함)</summary>
            private static void GenerateDhParamTable(StringBuilder h, string dhText, List<CDhGroup> groups)
            {
                string[] grpColors = { "#569cd6", "#4ec9b0", "#dcdcaa", "#c586c0", "#ce9178", "#6a9955" };
                string[] lines = dhText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                h.AppendLine("<table class=\"dh\">");
                // 헤더: Index | 핵심 DH 파라미터(파란) | 확장 파라미터(주황) | 설명
                h.Append("<tr>");
                h.Append("<th class=\"idx-col\">#</th>");
                h.Append("<th class=\"dh-core\">A</th>");
                h.Append("<th class=\"dh-core\">D</th>");
                h.Append("<th class=\"dh-core\">\u03b8</th>");
                h.Append("<th class=\"dh-core\">\u03b1</th>");
                h.Append("<th class=\"dh-ext dh-sep\">Axis</th>");
                h.Append("<th class=\"dh-ext\">Dir</th>");
                h.Append("<th class=\"dh-ext\">Init</th>");
                h.Append("<th class=\"dh-ext\">Type</th>");
                h.Append("<th class=\"dh-ext\">Num</th>");
                h.Append("<th>\uc124\uba85</th>");
                h.AppendLine("</tr>");

                int idx = 0;
                int grpIdx = 0;
                bool firstDataLine = true;
                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("//")) continue;
                    if (line.StartsWith("@"))
                    {
                        h.AppendFormat("<tr><td class=\"idx-col\">{0}</td><td class=\"dh-core\" colspan=\"4\" style=\"text-align:left;color:#ce9178\">STL: {1}</td><td class=\"dh-ext dh-sep\" colspan=\"5\"></td><td style=\"color:#6a9955\">\ubaa8\ub378 \ud30c\uc77c</td></tr>\n", idx, Esc(line));
                        idx++;
                        continue;
                    }

                    // 주석 제거 후 파싱
                    string s = line;
                    int commentPos = s.IndexOf("//");
                    string comment = "";
                    if (commentPos >= 0) { comment = s.Substring(commentPos + 2).Trim(); s = s.Substring(0, commentPos); }
                    s = s.Replace("[", "").Replace("]", "").Trim();
                    string[] parts = s.Split(',');

                    string sA = "0", sD = "0", sTheta = "0", sAlpha = "0";
                    string sAxis = "-1", sDir = "0", sInit = "0";
                    string sType = "", sNum = "";
                    if (parts.Length >= 4)
                    {
                        sA = parts[0].Trim(); sD = parts[1].Trim();
                        sTheta = parts[2].Trim(); sAlpha = parts[3].Trim();
                    }
                    if (parts.Length >= 7)
                    {
                        sAxis = parts[4].Trim(); sDir = parts[5].Trim(); sInit = parts[6].Trim();
                    }
                    if (parts.Length >= 8) sType = parts[7].Trim();
                    if (parts.Length >= 9) sNum = parts[8].Trim();

                    float fA, fD, fTh, fAl; int nAx, nDir, nInit;
                    float.TryParse(sA, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fA);
                    float.TryParse(sD, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fD);
                    float.TryParse(sTheta, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fTh);
                    float.TryParse(sAlpha, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fAl);
                    int.TryParse(sAxis, out nAx); int.TryParse(sDir, out nDir); int.TryParse(sInit, out nInit);

                    // Init=1일 때 그룹 구분 행 삽입
                    if (nInit == 1 && !firstDataLine && groups.Count > 1)
                    {
                        grpIdx++;
                        string gc = grpColors[(grpIdx) % grpColors.Length];
                        string gName = (grpIdx < groups.Count) ? groups[grpIdx].Name : string.Format("\uadf8\ub8f9 {0}", grpIdx + 1);
                        h.AppendFormat("<tr class=\"grp-header\"><td colspan=\"11\" style=\"color:{0}\">\u25b6 {1}</td></tr>\n", gc, Esc(gName));
                    }
                    if (firstDataLine && groups.Count > 1)
                    {
                        string gc = grpColors[0];
                        h.AppendFormat("<tr class=\"grp-header\"><td colspan=\"11\" style=\"color:{0}\">\u25b6 {1}</td></tr>\n", gc, Esc(groups[0].Name));
                    }
                    firstDataLine = false;

                    // 설명 생성
                    string desc = "";
                    if (nInit == 1) desc = "\uc88c\ud45c \ucd08\uae30\ud654 (Init)";
                    else if (nAx >= 0 && fA == 0 && fD == 0 && fTh == 0 && fAl == 0)
                        desc = string.Format("\ubaa8\ud130 {0}\ubc88", nAx);
                    else
                    {
                        List<string> parts2 = new List<string>();
                        if (nAx >= 0) parts2.Add(string.Format("\ubaa8\ud130{0}", nAx));
                        if (fA != 0) parts2.Add(string.Format("X\uc774\ub3d9 {0}mm", fA));
                        if (fD != 0) parts2.Add(string.Format("Z\uc774\ub3d9 {0}mm", fD));
                        if (fTh != 0) parts2.Add(string.Format("Z\ud68c\uc804 {0}\u00b0", fTh));
                        if (fAl != 0) parts2.Add(string.Format("X\ud68c\uc804 {0}\u00b0", fAl));
                        desc = string.Join(", ", parts2.ToArray());
                    }
                    if (comment.Length > 0) desc = comment;

                    // Init=1 행은 강조
                    string initStyle = (nInit == 1) ? " style=\"color:#ff9900;font-weight:bold\"" : "";
                    string axisColor = (nAx >= 0) ? " style=\"color:#dcdcaa\"" : "";
                    string trClass = (nInit == 1 && idx > 0) ? " class=\"grp-sep\"" : "";
                    h.AppendFormat("<tr{0}><td class=\"idx-col\">{1}</td><td class=\"dh-core\">{2}</td><td class=\"dh-core\">{3}</td><td class=\"dh-core\">{4}</td><td class=\"dh-core\">{5}</td>" +
                        "<td class=\"dh-ext dh-sep\"{6}>{7}</td><td class=\"dh-ext\">{8}</td><td class=\"dh-ext\"{9}>{10}</td><td class=\"dh-ext\">{11}</td><td class=\"dh-ext\">{12}</td>" +
                        "<td style=\"color:#6a9955;text-align:left\">{13}</td></tr>\n",
                        trClass, idx, sA, sD, sTheta, sAlpha, axisColor, sAxis, sDir, initStyle, sInit, sType, sNum, Esc(desc));
                    idx++;
                }
                h.AppendLine("</table>");
            }
                /// <summary>EDH 텍스트 + 체인 그룹 → FK 수식 분석 HTML 문서 (문자열 반환).
                /// ★파일 쓰기·브라우저 열기는 호출자(앱) 몫이다 — 이 함수는 순수하다.</summary>
                public static string Edh_BuildFormulaHtml(string dhText, List<CDhGroup> groups)
            {
                // 그룹 색상 배열
                string[] grpColors = { "#569cd6", "#4ec9b0", "#dcdcaa", "#c586c0", "#ce9178", "#6a9955" };

                StringBuilder h = new StringBuilder();
                h.AppendLine("<!DOCTYPE html><html lang=\"ko\"><head><meta charset=\"UTF-8\">");
                h.AppendLine("<title>FK \uc218\uc2dd \ubd84\uc11d</title>");
                h.AppendLine("<style>");
                h.AppendLine("*{margin:0;padding:0;box-sizing:border-box}");
                h.AppendLine("body{background:#1e1e1e;color:#d4d4d4;font-family:'Segoe UI','맑은 고딕',sans-serif;padding:24px 32px;line-height:1.6}");
                h.AppendLine("h1{color:#569cd6;font-size:24px;margin-bottom:4px}");
                h.AppendLine(".subtitle{color:#888;font-size:13px;margin-bottom:24px}");
                h.AppendLine("h2{color:#c586c0;font-size:18px;margin-top:32px;padding-bottom:6px;border-bottom:1px solid #333}");
                h.AppendLine("h3{color:#4ec9b0;font-size:15px;margin:16px 0 8px}");
                h.AppendLine(".legend{background:#252526;padding:16px 20px;border-radius:8px;margin:12px 0;display:inline-block}");
                h.AppendLine(".legend table{border-collapse:collapse}");
                h.AppendLine(".legend td{padding:3px 16px 3px 0;vertical-align:top}");
                h.AppendLine(".legend .sym{font-family:Consolas,monospace;font-weight:bold;min-width:100px}");
                h.AppendLine("table.dh{border-collapse:collapse;margin:12px 0}");
                h.AppendLine("table.dh th{background:#333;color:#9cdcfe;padding:6px 14px;text-align:center;font-size:13px}");
                h.AppendLine("table.dh th.dh-core{background:#2a3a4a;color:#4fc1ff;font-weight:bold}");
                h.AppendLine("table.dh th.dh-ext{background:#3a2a2a;color:#e0a070}");
                h.AppendLine("table.dh td{padding:5px 14px;text-align:right;border:1px solid #444;font-family:Consolas,monospace;font-size:13px}");
                h.AppendLine("table.dh td.dh-core{background:rgba(42,58,74,0.25)}");
                h.AppendLine("table.dh td.dh-ext{background:rgba(58,42,42,0.25)}");
                h.AppendLine("table.dh td.dh-sep{border-left:2px solid #666}");
                h.AppendLine("table.dh th.dh-sep{border-left:2px solid #666}");
                h.AppendLine("table.dh tr:nth-child(even) td.dh-core{background:rgba(42,58,74,0.4)}");
                h.AppendLine("table.dh tr:nth-child(even) td.dh-ext{background:rgba(58,42,42,0.4)}");
                h.AppendLine("table.dh tr:nth-child(even) td{background:#252526}");
                h.AppendLine("table.dh td.idx-col{background:#333;color:#9cdcfe;font-weight:bold;text-align:center}");
                h.AppendLine("table.dh tr.grp-sep td{border-top:3px solid #666}");
                h.AppendLine("table.dh tr.grp-header td{background:#2a2a3a;text-align:left;padding:8px 14px;font-weight:bold;font-size:14px;border:none}");
                h.AppendLine("table.mtx{border-collapse:collapse;margin:8px 0;display:inline-block}");
                h.AppendLine("table.mtx td{padding:4px 10px;text-align:center;font-family:Consolas,monospace;font-size:13px;min-width:60px;border:1px solid #444;background:#252526}");
                h.AppendLine(".bracket{color:#ffd700;font-size:280%;vertical-align:middle;line-height:1;padding:0 4px}");
                h.AppendLine(".mtx-wrap{display:inline-flex;align-items:center;margin:8px 0;flex-wrap:wrap}");
                h.AppendLine(".mtx-label{color:#808080;font-style:italic;margin-right:8px;font-size:14px}");
                h.AppendLine(".op{color:#d4d4d4;font-size:20px;padding:0 12px;vertical-align:middle}");
                h.AppendLine(".eq{color:#d4d4d4;font-size:20px;padding:0 12px;vertical-align:middle}");
                h.AppendLine(".fn{color:#569cd6;font-weight:bold}");
                h.AppendLine(".var{color:#dcdcaa}");
                h.AppendLine(".num{color:#b5cea8}");
                h.AppendLine(".result-box{background:#2d2d2d;border-left:3px solid #569cd6;padding:16px 20px;margin:12px 0;font-family:Consolas,monospace;font-size:15px;line-height:2.0;overflow-x:auto}");
                h.AppendLine(".formula-label{color:#c586c0;font-weight:bold;font-size:16px;min-width:30px;display:inline-block}");
                h.AppendLine(".xyz-box{background:#2d2d2d;padding:14px 20px;margin:10px 0;font-family:Consolas,monospace;font-size:15px;line-height:1.8;overflow-x:auto;border-radius:6px}");
                h.AppendLine(".xyz-box.x-box{border-left:4px solid #ff6b6b}");
                h.AppendLine(".xyz-box.y-box{border-left:4px solid #6bff6b}");
                h.AppendLine(".xyz-box.z-box{border-left:4px solid #6b9fff}");
                h.AppendLine(".xyz-label{font-weight:bold;font-size:18px;margin-right:8px}");
                h.AppendLine(".xyz-label.x-label{color:#ff6b6b}");
                h.AppendLine(".xyz-label.y-label{color:#6bff6b}");
                h.AppendLine(".xyz-label.z-label{color:#6b9fff}");
                h.AppendLine(".dir-box{background:#2d2d2d;border-left:3px solid #4ec9b0;padding:16px 20px;margin:12px 0;font-family:Consolas,monospace;font-size:14px;line-height:1.8}");
                h.AppendLine(".dir-label{color:#4ec9b0;font-weight:bold;min-width:160px;display:inline-block}");
                h.AppendLine(".step-desc{color:#6a9955;font-style:italic;margin:4px 0 8px 0;font-size:13px}");
                h.AppendLine(".grp-section{border:1px solid #444;border-radius:10px;padding:20px 24px;margin:20px 0;background:#1a1a1a}");
                h.AppendLine(".grp-title{font-size:17px;font-weight:bold;margin-bottom:12px;padding-bottom:6px;border-bottom:2px solid}");
                h.AppendLine("</style></head><body>");

                // ── 제목 ──
                h.AppendLine("<h1>Forward Kinematics \uc218\uc2dd \ubd84\uc11d</h1>");
                int totalGroups = groups.Count;
                if (totalGroups > 1)
                    h.AppendFormat("<div class=\"subtitle\">DH \ud30c\ub77c\ubbf8\ud130\ub85c\ubd80\ud130 \uc704\uce58/\ubc29\ud5a5 \uc218\uc2dd\uc744 \uc720\ub3c4\ud569\ub2c8\ub2e4 \u2014 {0}\uac1c \uadf8\ub8f9 (Init=1 \uae30\uc900 \ubd84\ub9ac)</div>\n", totalGroups);
                else
                    h.AppendLine("<div class=\"subtitle\">DH \ud30c\ub77c\ubbf8\ud130\ub85c\ubd80\ud130 \uc704\uce58/\ubc29\ud5a5 \uc218\uc2dd\uc744 \uc720\ub3c4\ud569\ub2c8\ub2e4</div>");

                // ── 1. 범례 ──
                h.AppendLine("<h2>1. \ubc94\ub840 (Legend)</h2>");

                // 수학 기호 범례
                h.AppendLine("<h3>\uc218\ud559 \uae30\ud638</h3>");
                h.AppendLine("<div class=\"legend\"><table>");
                h.AppendLine("<tr><td class=\"sym\"><span class=\"fn\">C</span>(\u03b8)</td><td>= cos(\u03b8) \u2014 \ucf54\uc0ac\uc778 \ud568\uc218</td></tr>");
                h.AppendLine("<tr><td class=\"sym\"><span class=\"fn\">S</span>(\u03b8)</td><td>= sin(\u03b8) \u2014 \uc0ac\uc778 \ud568\uc218</td></tr>");
                h.AppendLine("<tr><td class=\"sym\"><span class=\"var\">t0</span>, <span class=\"var\">t1</span>, ...</td><td>= \ubaa8\ud130 0, \ubaa8\ud130 1, ... \uc758 \uad00\uc808 \uac01\ub3c4 (Degree, 0~360\u00b0)</td></tr>");
                h.AppendLine("<tr><td class=\"sym\"><span class=\"var\">-t0</span></td><td>= \uc5ed\ubc29\ud5a5 \ubaa8\ud130 (Dir=1\uc77c \ub54c, \uac01\ub3c4\uc5d0 -1\uc744 \uacf1\ud568)</td></tr>");
                h.AppendLine("</table></div>");

                // DH 핵심 파라미터 범례
                h.AppendLine("<h3>DH \ud575\uc2ec \ud30c\ub77c\ubbf8\ud130 (A, D, \u03b8, \u03b1)</h3>");
                h.AppendLine("<div class=\"legend\" style=\"border-left:3px solid #4fc1ff\"><table>");
                h.AppendLine("<tr><td class=\"sym\">A</td><td>\ub9c1\ud06c \uae38\uc774 (X\ucd95 \uc774\ub3d9, mm) \u2014 <span style=\"color:#ff6b6b\">\ube68\uac04\ucd95</span></td></tr>");
                h.AppendLine("<tr><td class=\"sym\">D</td><td>\ub9c1\ud06c \uc624\ud504\uc14b (Z\ucd95 \uc774\ub3d9, mm) \u2014 <span style=\"color:#6b9fff\">\ud30c\ub780\ucd95</span></td></tr>");
                h.AppendLine("<tr><td class=\"sym\">\u03b8 (Theta)</td><td>\uad00\uc808 \uac01\ub3c4 (Z\ucd95 \ud68c\uc804, \u00b0) \u2014 \ubaa8\ud130\uac00 \uc5f0\uacb0\ub418\uba74 \ubaa8\ud130 \uac01\ub3c4\ub85c \ub300\uce58</td></tr>");
                h.AppendLine("<tr><td class=\"sym\">\u03b1 (Alpha)</td><td>\ub9c1\ud06c \ube44\ud2c0\ub9bc (X\ucd95 \ud68c\uc804, \u00b0)</td></tr>");
                h.AppendLine("</table></div>");

                // 확장 파라미터 범례
                h.AppendLine("<h3>\ud655\uc7a5 \ud30c\ub77c\ubbf8\ud130 (Axis, Dir, Init, Type, Num)</h3>");
                h.AppendLine("<div class=\"legend\" style=\"border-left:3px solid #e0a070\"><table>");
                h.AppendLine("<tr><td class=\"sym\">Axis</td><td>\uc5f0\uacb0\ub41c \ubaa8\ud130 \ubc88\ud638 (-1: \uc5c6\uc74c, 0~: \ubaa8\ud130 \ubc88\ud638)</td></tr>");
                h.AppendLine("<tr><td class=\"sym\">Dir</td><td>\ubaa8\ud130 \ubc29\ud5a5: 0=\uc815\ubc29\ud5a5 Z\ud68c\uc804, 1=\uc5ed\ubc29\ud5a5 Z\ud68c\uc804, 2=\uc815\ubc29\ud5a5 Z\uc774\ub3d9, 3=\uc5ed\ubc29\ud5a5 Z\uc774\ub3d9, 4=\uc815\ubc29\ud5a5 \ubc14\ud034\ud68c\uc804, 5=\uc5ed\ubc29\ud5a5 \ubc14\ud034\ud68c\uc804</td></tr>");
                h.AppendLine("<tr><td class=\"sym\">Init</td><td>\uc88c\ud45c \ucd08\uae30\ud654 \ud50c\ub798\uadf8: <b>1</b>=\uc6d0\uc810\uc73c\ub85c \ub9ac\uc14b(\uc0c8 \uccb4\uc778 \uc2dc\uc791), 0=\uc774\uc804 \uccb4\uc778 \uc774\uc5b4\uc11c \uacc4\uc18d</td></tr>");
                h.AppendLine("<tr><td class=\"sym\">Type</td><td>\ud53c\ud0b9 \ud0c0\uc785: 0=\uc5c6\uc74c, <b>1</b>=\ubaa8\ud130\uadf8\ub8f9(\uac19\uc740 \uadf8\ub8f9\ub07c\ub9ac \ud568\uaed8 \uc120\ud0dd), <b>2</b>=IK\ubc88\ud638(\uc5ed\uae30\uad6c\ud559 \ub300\uc0c1 \uc9c0\uc815)</td></tr>");
                h.AppendLine("<tr><td class=\"sym\">Num</td><td>\uadf8\ub8f9/IK \ubc88\ud638: Type\uc5d0 \ub530\ub77c \ubaa8\ud130\uadf8\ub8f9 \ubc88\ud638 \ub610\ub294 IK \uc778\ub371\uc2a4 (0\ubd80\ud130 \uc2dc\uc791)</td></tr>");
                h.AppendLine("</table></div>");

                // ── 2. DH 파라미터 테이블 ──
                h.AppendLine("<h2>2. DH \ud30c\ub77c\ubbf8\ud130</h2>");
                GenerateDhParamTable(h, dhText, groups);

                // ── 3~7. 그룹별 수식 섹션 ──
                int sectionNum = 3;
                for (int gi = 0; gi < groups.Count; gi++)
                {
                    CDhGroup grp = groups[gi];
                    string grpColor = grpColors[gi % grpColors.Length];

                    if (totalGroups > 1)
                    {
                        // 그룹 컨테이너
                        h.AppendFormat("<div class=\"grp-section\" style=\"border-color:{0}\">\n", grpColor);
                        h.AppendFormat("<div class=\"grp-title\" style=\"color:{0};border-bottom-color:{0}\">\u25b6 {1}</div>\n", grpColor, Esc(grp.Name));
                    }

                    // 개별 T행렬 + 곱셈 과정
                    if (!string.IsNullOrEmpty(grp.MatrixChain))
                    {
                        h.AppendFormat("<h2>{0}. \uac1c\ubcc4 T\ud589\ub82c + \uacf1\uc148 \uacfc\uc815 ({1})</h2>\n", sectionNum, Esc(grp.Name));
                        GenerateMatrixSections(h, grp.MatrixChain);
                        sectionNum++;
                    }

                    // X,Y,Z 수식
                    h.AppendFormat("<h2>{0}. \ucd5c\uc885 \uc704\uce58 \uc218\uc2dd ({1})</h2>\n", sectionNum, Esc(grp.Name));
                    h.AppendLine("<p style=\"color:#888;font-size:13px;margin-bottom:8px\">\ucd5c\uc885 \ubcc0\ud658 \ud589\ub82c\uc758 4\ubc88\uc9f8 \uc5f4\uc5d0\uc11c X, Y, Z \uc704\uce58\ub97c \ucd94\ucd9c\ud569\ub2c8\ub2e4.</p>");
                    if (grp.XyzFormulas != null && grp.XyzFormulas.Length >= 3)
                    {
                        h.AppendLine("<div class=\"xyz-box x-box\">");
                        h.AppendFormat("<span class=\"xyz-label x-label\">X</span> <span style=\"color:#d4d4d4;font-size:16px\">=</span> {0}", ColorizeFormulaExpr(grp.XyzFormulas[0]));
                        h.AppendLine("</div>");
                        h.AppendLine("<div class=\"xyz-box y-box\">");
                        h.AppendFormat("<span class=\"xyz-label y-label\">Y</span> <span style=\"color:#d4d4d4;font-size:16px\">=</span> {0}", ColorizeFormulaExpr(grp.XyzFormulas[1]));
                        h.AppendLine("</div>");
                        h.AppendLine("<div class=\"xyz-box z-box\">");
                        h.AppendFormat("<span class=\"xyz-label z-label\">Z</span> <span style=\"color:#d4d4d4;font-size:16px\">=</span> {0}", ColorizeFormulaExpr(grp.XyzFormulas[2]));
                        h.AppendLine("</div>");
                    }
                    else
                    {
                        h.AppendLine("<p style=\"color:#f44\">\uc218\uc2dd\uc744 \ucd94\ucd9c\ud560 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4.</p>");
                    }
                    sectionNum++;

                    // 방향 벡터
                    if (grp.XyzFormulas != null && grp.XyzFormulas.Length >= 6)
                    {
                        h.AppendFormat("<h2>{0}. \ub05d\uc810 \ubc29\ud5a5 \ubca1\ud130 ({1})</h2>\n", sectionNum, Esc(grp.Name));
                        h.AppendLine("<div class=\"dir-box\">");
                        h.AppendFormat("<span class=\"dir-label\">X\ucd95 \ubc29\ud5a5\ubca1\ud130 = </span>{0}<br>", Esc(grp.XyzFormulas[3]));
                        h.AppendFormat("<span class=\"dir-label\">Y\ucd95 \ubc29\ud5a5\ubca1\ud130 = </span>{0}<br>", Esc(grp.XyzFormulas[4]));
                        h.AppendFormat("<span class=\"dir-label\">Z\ucd95 \ubc29\ud5a5\ubca1\ud130 = </span>{0}", Esc(grp.XyzFormulas[5]));
                        h.AppendLine("</div>");
                        sectionNum++;
                    }

                    if (totalGroups > 1)
                        h.AppendLine("</div>"); // grp-section 닫기
                }

                // ── DH 공식 참고 ──
                h.AppendFormat("<h2>{0}. DH \ubcc0\ud658 \ud589\ub82c \uacf5\uc2dd (Reference)</h2>\n", sectionNum);
                h.AppendLine("<p style=\"color:#888;font-size:13px;margin-bottom:12px\">\uac01 DH \ud30c\ub77c\ubbf8\ud130(A, D, \u03b8, \u03b1)\ub85c\ubd80\ud130 4\u00d74 \ub3d9\ucc28 \ubcc0\ud658 \ud589\ub82c T\ub97c \ub9cc\ub4dc\ub294 \uacf5\uc2dd:</p>");
                h.AppendLine("<div class=\"result-box\" style=\"border-left-color:#4ec9b0;font-size:14px;line-height:1.8\">");
                h.AppendLine("T = Rot<sub>z</sub>(\u03b8) \u00b7 Trans<sub>z</sub>(D) \u00b7 Trans<sub>x</sub>(A) \u00b7 Rot<sub>x</sub>(\u03b1)<br><br>");
                h.AppendLine("<table class=\"mtx\" style=\"font-size:13px\">");
                h.AppendLine("<tr><td><span class=\"fn\">C</span>\u03b8</td><td>-<span class=\"fn\">S</span>\u03b8\u00b7<span class=\"fn\">C</span>\u03b1</td><td><span class=\"fn\">S</span>\u03b8\u00b7<span class=\"fn\">S</span>\u03b1</td><td>A\u00b7<span class=\"fn\">C</span>\u03b8</td></tr>");
                h.AppendLine("<tr><td><span class=\"fn\">S</span>\u03b8</td><td><span class=\"fn\">C</span>\u03b8\u00b7<span class=\"fn\">C</span>\u03b1</td><td>-<span class=\"fn\">C</span>\u03b8\u00b7<span class=\"fn\">S</span>\u03b1</td><td>A\u00b7<span class=\"fn\">S</span>\u03b8</td></tr>");
                h.AppendLine("<tr><td>0</td><td><span class=\"fn\">S</span>\u03b1</td><td><span class=\"fn\">C</span>\u03b1</td><td>D</td></tr>");
                h.AppendLine("<tr><td>0</td><td>0</td><td>0</td><td>1</td></tr>");
                h.AppendLine("</table>");
                h.AppendLine("</div>");

                h.AppendLine("<div style=\"margin-top:32px;color:#555;font-size:12px;border-top:1px solid #333;padding-top:8px\">");
                h.AppendLine("Generated by MakeUrdf \u2014 OpenJigWare FK Formula Analyzer</div>");
                h.AppendLine("</body></html>");

                    return h.ToString();
            }
        }
    }
}
