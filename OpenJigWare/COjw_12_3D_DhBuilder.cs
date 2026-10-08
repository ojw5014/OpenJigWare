using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
        public enum BuildAction
        {
            None,
            RedMove,      // X축 이동: A
            BlueMove,     // Z축 이동: D
            GreenMove,    // Y축 이동: 복합 3줄
            RedRotate,    // X축 회전: Alpha
            BlueRotate,   // Z축 회전: Theta
            GreenRotate,  // Y축 회전: 복합 3줄
            Motor,        // 모터 선언
            Init,         // 좌표 초기화
            Stl           // STL 모델 추가
        }

        public class DhStep
        {
            public BuildAction Action;
            public float Value;
            public int MotorNumber;
            public string Description;
            public string DhLines;
            public bool IsRaw;       // true = 텍스트에서 직접 임포트 (InjectGroupInfo 건너뜀)

            public override string ToString() { return Description; }
        }

        /// <summary>DH 텍스트 한 줄에 축 delta 를 적용한 결과 (±버튼·휠의 "선택 라인 편집" 경로).
        /// Inserted=false 면 그 줄을 NewLines[0] 으로 교체, true 면 그 줄 바로 뒤에 NewLines 를 끼운다.</summary>
        public class DhLineEdit
        {
            public bool Ok;             // false = 적용 불가 → 호출 측이 기존 동작 유지
            public bool Inserted;       // true = 새 줄 삽입 / false = 제자리 값 수정
            public string[] NewLines;   // 교체 줄(1개) 또는 삽입 줄들
            public int CarrierOffset;   // 삽입 시 값 운반 줄의 상대 위치 (Y축 합성은 가운데 = 1)
            public string FieldName;    // 바뀐 필드 표시명 (A / D / Θ / α / STL X …)
            public float OldValue;
            public float NewValue;
            public string Description;  // 삽입된 스텝 설명
            public string Reason;       // 삽입으로 간 사유 (모터 줄 / 수식 줄 / …)
        }

        public class DhBuilder
        {
            private List<DhStep> m_lstSteps = new List<DhStep>();

            // 오버레이: 특정 스텝 뒤에 임시 하이라이트 DH 라인 삽입
            private int m_nOverlayIdx = -1;
            private string m_strOverlay = "";

            public int StepCount { get { return m_lstSteps.Count; } }
            public DhStep GetStep(int index) { return m_lstSteps[index]; }

            public void SetOverlay(int afterStepIdx, string dhLines)
            {
                m_nOverlayIdx = afterStepIdx;
                m_strOverlay = dhLines;
            }

            public void ClearOverlay()
            {
                m_nOverlayIdx = -1;
                m_strOverlay = "";
            }

            public static DhStep CreateStep(BuildAction action, float value, int motorNum = -1)
            {
                DhStep step = new DhStep();
                step.Action = action;
                step.Value = value;
                step.MotorNumber = motorNum;

                int v = (int)Math.Round(value);
                string sv = v.ToString();

                switch (action)
                {
                    case BuildAction.RedMove:
                        step.Description = string.Format("빨간(X) {0}mm 이동", sv);
                        step.DhLines = string.Format("[{0},0,0,0],[-1,0,0]", sv);
                        break;
                    case BuildAction.BlueMove:
                        step.Description = string.Format("파란(Z) {0}mm 이동", sv);
                        step.DhLines = string.Format("[0,{0},0,0],[-1,0,0]", sv);
                        break;
                    case BuildAction.GreenMove:
                        step.Description = string.Format("초록(Y) {0}mm 이동", sv);
                        step.DhLines = string.Format(
                            "[0,0,90,0],[-1,0,0]\r\n[{0},0,0,0],[-1,0,0]\r\n[0,0,-90,0],[-1,0,0]", sv);
                        break;
                    case BuildAction.RedRotate:
                        step.Description = string.Format("빨간(X)축 {0}도 회전", sv);
                        step.DhLines = string.Format("[0,0,0,{0}],[-1,0,0]", sv);
                        break;
                    case BuildAction.BlueRotate:
                        step.Description = string.Format("파란(Z)축 {0}도 회전", sv);
                        step.DhLines = string.Format("[0,0,{0},0],[-1,0,0]", sv);
                        break;
                    case BuildAction.GreenRotate:
                        step.Description = string.Format("초록(Y)축 {0}도 회전", sv);
                        step.DhLines = string.Format(
                            "[0,0,0,90],[-1,0,0]\r\n[0,0,{0},0],[-1,0,0]\r\n[0,0,0,-90],[-1,0,0]", sv);
                        break;
                    case BuildAction.Motor:
                        step.Description = string.Format("모터 {0}번 추가", motorNum);
                        step.DhLines = string.Format("[0,0,0,0],[{0},0,0]", motorNum);
                        break;
                    case BuildAction.Init:
                        step.Description = "좌표 초기화";
                        step.DhLines = "[0,0,0,0],[-1,0,1]";
                        break;
                }
                return step;
            }

            public static DhStep CreateStlStep(string filename, float offX, float offY, float offZ,
                float pan = 0, float tilt = 0, float swing = 0)
            {
                DhStep step = new DhStep();
                step.Action = BuildAction.Stl;
                step.Value = 0;
                step.MotorNumber = -1;
                step.Description = string.Format("STL: {0}", System.IO.Path.GetFileName(filename));
                step.DhLines = string.Format(CultureInfo.InvariantCulture,
                    "@{0},-1,1.0,0,0,{1:F1},{2:F1},{3:F1},{4:F1},{5:F1},{6:F1}",
                    filename, offX, offY, offZ, pan, tilt, swing);
                return step;
            }

            /// <summary>
            /// STL DhLines에서 파라미터 파싱.
            /// 형식: @filename,-1,1.0,type,group,offX,offY,offZ,pan,tilt,swing
            /// </summary>
            public static bool ParseStlParams(string dhLines,
                out string filename, out float offX, out float offY, out float offZ,
                out float pan, out float tilt, out float swing)
            {
                filename = ""; offX = offY = offZ = pan = tilt = swing = 0;
                if (string.IsNullOrEmpty(dhLines) || !dhLines.TrimStart().StartsWith("@"))
                    return false;
                // 두 가지 DH 형식 모두 지원 (구형 / 신형 `[ ]` 그룹):
                //   구형: @filename,color,scale,type,num,offX,offY,offZ,pan,tilt,swing
                //   신형: @[filename,color,scale],[type,num],[offX,offY,offZ],[pan,tilt,swing]
                // 신형의 `[` `]` 가 split 결과에 남으면 float.TryParse 실패 → 0.
                // → 두 문자 모두 제거 후 split.
                string raw = dhLines.TrimStart('@').Replace("[", "").Replace("]", "");
                string[] parts = raw.Split(',');
                if (parts.Length < 11) return false;
                filename = parts[0].Trim();
                float.TryParse(parts[5].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out offX);
                float.TryParse(parts[6].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out offY);
                float.TryParse(parts[7].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out offZ);
                float.TryParse(parts[8].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out pan);
                float.TryParse(parts[9].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out tilt);
                float.TryParse(parts[10].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out swing);
                return true;
            }

            /// <summary>
            /// STL DhLines 재구성.
            /// </summary>
            public static string BuildStlDhLines(string filename,
                float offX, float offY, float offZ,
                float pan, float tilt, float swing)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "@{0},-1,1.0,0,0,{1:F1},{2:F1},{3:F1},{4:F1},{5:F1},{6:F1}",
                    filename, offX, offY, offZ, pan, tilt, swing);
            }

            public void AddStep(DhStep step)
            {
                m_lstSteps.Add(step);
            }

            public DhStep RemoveLastStep()
            {
                if (m_lstSteps.Count == 0) return null;
                DhStep last = m_lstSteps[m_lstSteps.Count - 1];
                m_lstSteps.RemoveAt(m_lstSteps.Count - 1);
                return last;
            }

            public void RemoveStepAt(int index)
            {
                if (index >= 0 && index < m_lstSteps.Count)
                    m_lstSteps.RemoveAt(index);
            }

            public void Clear()
            {
                m_lstSteps.Clear();
            }

            /// <summary>
            /// DH 텍스트를 파싱하여 스텝으로 임포트. 주석/빈줄은 건너뜀.
            /// </summary>
            public void ImportFromText(string dhText)
            {
                Clear();
                if (string.IsNullOrEmpty(dhText)) return;

                string[] lines = dhText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string raw in lines)
                {
                    string trimmed = raw.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("//"))
                        continue;

                    DhStep step = new DhStep();
                    step.IsRaw = true;
                    step.DhLines = trimmed;

                    if (trimmed.StartsWith("@"))
                    {
                        step.Action = BuildAction.Stl;
                        step.Description = "STL: " + ExtractStlFilename(trimmed);
                    }
                    else
                    {
                        float A, D, Theta, Alpha;
                        int Axis, Dir, Init;
                        ParseDhValues(trimmed, out A, out D, out Theta, out Alpha, out Axis, out Dir, out Init);

                        step.MotorNumber = (Axis >= 0) ? Axis : -1;

                        bool allZero = (A == 0 && D == 0 && Theta == 0 && Alpha == 0);
                        if (Init == 1 && allZero && Axis < 0)
                            step.Action = BuildAction.Init;
                        else if (Axis >= 0 && allZero)
                            step.Action = BuildAction.Motor;
                        else if (A != 0 && D == 0 && Theta == 0 && Alpha == 0 && Axis < 0)
                            { step.Action = BuildAction.RedMove;    step.Value = A; }
                        else if (D != 0 && A == 0 && Theta == 0 && Alpha == 0 && Axis < 0)
                            { step.Action = BuildAction.BlueMove;   step.Value = D; }
                        else if (Theta != 0 && A == 0 && D == 0 && Alpha == 0 && Axis < 0)
                            { step.Action = BuildAction.BlueRotate; step.Value = Theta; }
                        else if (Alpha != 0 && A == 0 && D == 0 && Theta == 0 && Axis < 0)
                            { step.Action = BuildAction.RedRotate;  step.Value = Alpha; }
                        else
                            step.Action = BuildAction.None;

                        step.Description = BuildImportDescription(A, D, Theta, Alpha, Axis, Dir, Init);
                    }

                    m_lstSteps.Add(step);
                }
            }

            /// <summary>DH 라인 인덱스 → 스텝 인덱스</summary>
            public int MapDhLineToStep(int dhLineIndex)
            {
                int dhLine = 0;
                for (int i = 0; i < m_lstSteps.Count; i++)
                {
                    int lineCount = CountValidDhLines(m_lstSteps[i].DhLines);
                    if (dhLineIndex < dhLine + lineCount) return i;
                    dhLine += lineCount;
                }
                return -1;
            }

            /// <summary>스텝 인덱스 → 첫 번째 DH 라인 인덱스</summary>
            public int MapStepToDhLine(int stepIndex)
            {
                int dhLine = 0;
                for (int i = 0; i < stepIndex && i < m_lstSteps.Count; i++)
                    dhLine += CountValidDhLines(m_lstSteps[i].DhLines);
                return dhLine;
            }

            private static int CountValidDhLines(string dhLines)
            {
                if (string.IsNullOrEmpty(dhLines)) return 1;
                string[] lines = dhLines.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                int count = 0;
                foreach (string l in lines)
                {
                    string t = l.Trim();
                    if (t.Length > 0 && !t.StartsWith("//")) count++;
                }
                return Math.Max(count, 1);
            }

            private static void ParseDhValues(string line, out float A, out float D,
                out float Theta, out float Alpha, out int Axis, out int Dir, out int Init)
            {
                A = D = Theta = Alpha = 0;
                Axis = -1; Dir = 0; Init = 0;

                string s = line;
                int commentPos = s.IndexOf("//");
                if (commentPos >= 0) s = s.Substring(0, commentPos);

                s = s.Replace("[", "").Replace("]", "").Trim();
                string[] parts = s.Split(',');

                if (parts.Length >= 4)
                {
                    float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out A);
                    float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out D);
                    float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out Theta);
                    float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out Alpha);
                }
                if (parts.Length >= 7)
                {
                    int.TryParse(parts[4].Trim(), out Axis);
                    int.TryParse(parts[5].Trim(), out Dir);
                    int.TryParse(parts[6].Trim(), out Init);
                }
            }

            private static string ExtractStlFilename(string line)
            {
                string s = line.TrimStart();
                if (s.StartsWith("@["))
                    s = s.Substring(2);
                else if (s.StartsWith("@"))
                    s = s.Substring(1);
                int comma = s.IndexOf(',');
                if (comma >= 0)
                    s = s.Substring(0, comma);
                return s.Trim().TrimEnd(']');
            }

            private static string BuildImportDescription(float A, float D, float Theta, float Alpha,
                int Axis, int Dir, int Init)
            {
                bool hasMotor = (Axis >= 0);
                bool hasInit = (Init == 1);
                bool allZero = (A == 0 && D == 0 && Theta == 0 && Alpha == 0);

                if (hasInit && allZero && !hasMotor)
                    return "좌표 초기화";
                if (hasMotor && allZero)
                    return string.Format("모터 {0}번", Axis);
                if (!hasMotor && !hasInit)
                {
                    if (A != 0 && D == 0 && Theta == 0 && Alpha == 0)
                        return string.Format("빨간(X) {0}mm 이동", FmtNum(A));
                    if (D != 0 && A == 0 && Theta == 0 && Alpha == 0)
                        return string.Format("파란(Z) {0}mm 이동", FmtNum(D));
                    if (Theta != 0 && A == 0 && D == 0 && Alpha == 0)
                        return string.Format("파란(Z)축 {0}도 회전", FmtNum(Theta));
                    if (Alpha != 0 && A == 0 && D == 0 && Theta == 0)
                        return string.Format("빨간(X)축 {0}도 회전", FmtNum(Alpha));
                }

                StringBuilder sb = new StringBuilder();
                if (hasMotor) sb.AppendFormat("모터{0}", Axis);
                if (hasInit) { if (sb.Length > 0) sb.Append(" "); sb.Append("초기화"); }
                if (A != 0) { if (sb.Length > 0) sb.Append(" "); sb.AppendFormat("A:{0}", FmtNum(A)); }
                if (D != 0) { if (sb.Length > 0) sb.Append(" "); sb.AppendFormat("D:{0}", FmtNum(D)); }
                if (Theta != 0) { if (sb.Length > 0) sb.Append(" "); sb.AppendFormat("θ:{0}", FmtNum(Theta)); }
                if (Alpha != 0) { if (sb.Length > 0) sb.Append(" "); sb.AppendFormat("α:{0}", FmtNum(Alpha)); }
                return sb.Length > 0 ? sb.ToString() : "(원점)";
            }

            private static string FmtNum(float v)
            {
                return (v == (int)v) ? ((int)v).ToString() : v.ToString("F1", CultureInfo.InvariantCulture);
            }

            public string GenerateFullDh()
            {
                StringBuilder sb = new StringBuilder();
                int currentGroup = -1;
                for (int i = 0; i < m_lstSteps.Count; i++)
                {
                    var step = m_lstSteps[i];
                    if (step.Action == BuildAction.Motor)
                        currentGroup = step.MotorNumber;

                    if (step.IsRaw)
                        sb.AppendLine(step.DhLines);
                    else
                        sb.AppendLine(InjectGroupInfo(step.DhLines, currentGroup));

                    // 오버레이 삽입 (해당 스텝 바로 뒤)
                    if (i == m_nOverlayIdx && !string.IsNullOrEmpty(m_strOverlay))
                        sb.AppendLine(m_strOverlay);
                }
                return sb.ToString();
            }

            public string GenerateFullDhWithPreview(DhStep tempStep)
            {
                StringBuilder sb = new StringBuilder();
                int currentGroup = -1;
                for (int i = 0; i < m_lstSteps.Count; i++)
                {
                    var step = m_lstSteps[i];
                    if (step.Action == BuildAction.Motor)
                        currentGroup = step.MotorNumber;

                    if (step.IsRaw)
                        sb.AppendLine(step.DhLines);
                    else
                        sb.AppendLine(InjectGroupInfo(step.DhLines, currentGroup));

                    if (i == m_nOverlayIdx && !string.IsNullOrEmpty(m_strOverlay))
                        sb.AppendLine(m_strOverlay);
                }
                if (tempStep != null)
                {
                    if (tempStep.Action == BuildAction.Motor)
                        currentGroup = tempStep.MotorNumber;
                    sb.AppendLine(InjectGroupInfo(tempStep.DhLines, currentGroup));
                }
                return sb.ToString();
            }

            // DH 라인에 PickGroup 정보 주입
            private string InjectGroupInfo(string dhLines, int groupNum)
            {
                bool hasAtLine = dhLines.IndexOf('@') >= 0;
                if (groupNum < 0 && !hasAtLine) return dhLines;

                string[] lines = dhLines.Split(new[] { "\r\n" }, StringSplitOptions.None);
                StringBuilder sb = new StringBuilder();
                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0) continue;

                    if (line.StartsWith("@"))
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length >= 5)
                        {
                            int pickGroup = groupNum >= 0 ? groupNum : 1;
                            parts[3] = "1";
                            parts[4] = pickGroup.ToString();
                            line = string.Join(",", parts);
                        }
                    }
                    else if (groupNum >= 0)
                    {
                        int lastBracket = line.LastIndexOf(']');
                        if (lastBracket >= 0)
                            line = line.Substring(0, lastBracket) + string.Format(",1,{0}]", groupNum);
                    }

                    if (sb.Length > 0) sb.Append("\r\n");
                    sb.Append(line);
                }
                return sb.ToString();
            }

            public int GetNextMotorNumber()
            {
                int maxMotor = -1;
                foreach (var step in m_lstSteps)
                {
                    if (step.Action == BuildAction.Motor && step.MotorNumber > maxMotor)
                        maxMotor = step.MotorNumber;
                }
                return maxMotor + 1;
            }

            // ========================================
            // 3x3 행렬 연산 (순방향 기구학용)
            // ========================================

            public static double[,] MatMul3x3(double[,] A, double[,] B)
            {
                double[,] C = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < 3; k++)
                            C[i, j] += A[i, k] * B[k, j];
                return C;
            }

            /// <summary>3×3 행렬 전치. 회전 행렬의 경우 R⁻¹ = Rᵀ.</summary>
            public static double[,] Transpose3x3(double[,] A)
            {
                double[,] T = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        T[i, j] = A[j, i];
                return T;
            }

            /// <summary>축(단위벡터) + 각도(rad) → 3×3 회전 행렬 (Rodrigues).</summary>
            public static double[,] AxisAngleRotation(double ax, double ay, double az, double rad)
            {
                double c = Math.Cos(rad), s = Math.Sin(rad), C = 1 - c;
                return new double[,]
                {
                    { c + ax*ax*C,        ax*ay*C - az*s,  ax*az*C + ay*s },
                    { ay*ax*C + az*s,     c + ay*ay*C,     ay*az*C - ax*s },
                    { az*ax*C - ay*s,     az*ay*C + ax*s,  c + az*az*C    }
                };
            }

            /// <summary>두 단위벡터 (from → to) 사이의 최소회전 (Rodrigues).
            /// from·to = 1 → identity; from·to = -1 → 180° flip (수직축 자동 선택).</summary>
            public static double[,] RotationFromVectors(double fx, double fy, double fz,
                                                        double tx, double ty, double tz)
            {
                double vx = fy * tz - fz * ty;
                double vy = fz * tx - fx * tz;
                double vz = fx * ty - fy * tx;
                double s = Math.Sqrt(vx * vx + vy * vy + vz * vz);
                double c = fx * tx + fy * ty + fz * tz;
                if (s < 1e-9)
                {
                    if (c > 0) return new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                    double bx = (Math.Abs(fx) > 0.9) ? 0 : 1;
                    double by = (Math.Abs(fx) > 0.9) ? 1 : 0;
                    double bz = 0;
                    double pdot = fx * bx + fy * by + fz * bz;
                    bx -= pdot * fx; by -= pdot * fy; bz -= pdot * fz;
                    double bl = Math.Sqrt(bx * bx + by * by + bz * bz);
                    bx /= bl; by /= bl; bz /= bl;
                    return new double[,]
                    {
                        { 2*bx*bx-1, 2*bx*by,   2*bx*bz },
                        { 2*bx*by,   2*by*by-1, 2*by*bz },
                        { 2*bx*bz,   2*by*bz,   2*bz*bz-1 }
                    };
                }
                double k = (1 - c) / (s * s);
                double[,] R = new double[3, 3];
                R[0, 0] = 1 - (vy * vy + vz * vz) * k;
                R[0, 1] = -vz + vx * vy * k;
                R[0, 2] = vy + vx * vz * k;
                R[1, 0] = vz + vx * vy * k;
                R[1, 1] = 1 - (vx * vx + vz * vz) * k;
                R[1, 2] = -vx + vy * vz * k;
                R[2, 0] = -vy + vx * vz * k;
                R[2, 1] = vx + vy * vz * k;
                R[2, 2] = 1 - (vx * vx + vy * vy) * k;
                return R;
            }

            /// <summary>R = Rz(swing) · Rx(tilt) · Ry(pan) 분해 → ZXY Euler 각 (degrees).
            /// R[2,1] = sin(tilt) 기반. tilt = ±90° (gimbal lock) 시 pan = 0 으로 swing 에 흡수.</summary>
            public static void ZXYEulerFromMatrix(double[,] R,
                out float pan, out float tilt, out float swing)
            {
                double sx = R[2, 1];
                if (sx > 1) sx = 1; else if (sx < -1) sx = -1;
                double t = Math.Asin(sx);
                double cx = Math.Cos(t);
                double p, sw;
                if (Math.Abs(cx) > 1e-6)
                {
                    p = Math.Atan2(-R[2, 0], R[2, 2]);
                    sw = Math.Atan2(-R[0, 1], R[1, 1]);
                }
                else
                {
                    p = 0;
                    sw = Math.Atan2(R[1, 0], R[0, 0]);
                }
                float k = (float)(180.0 / Math.PI);
                pan = (float)p * k;
                tilt = (float)t * k;
                swing = (float)sw * k;
            }

            public static double[,] RotX3(double deg)
            {
                double r = deg * Math.PI / 180.0;
                double c = Math.Cos(r), s = Math.Sin(r);
                return new double[,] { { 1, 0, 0 }, { 0, c, -s }, { 0, s, c } };
            }

            public static double[,] RotY3(double deg)
            {
                double r = deg * Math.PI / 180.0;
                double c = Math.Cos(r), s = Math.Sin(r);
                return new double[,] { { c, 0, s }, { 0, 1, 0 }, { -s, 0, c } };
            }

            public static double[,] RotZ3(double deg)
            {
                double r = deg * Math.PI / 180.0;
                double c = Math.Cos(r), s = Math.Sin(r);
                return new double[,] { { c, -s, 0 }, { s, c, 0 }, { 0, 0, 1 } };
            }

            /// <summary>
            /// STL 로컬 회전 행렬: Rm = Rz(swing) * Rx(tilt) * Ry(pan)
            /// OjwRotation 적용 순서와 동일.
            /// </summary>
            public static double[,] StlLocalRotation(float pan, float tilt, float swing)
            {
                return MatMul3x3(RotZ3(swing), MatMul3x3(RotX3(tilt), RotY3(pan)));
            }

            // ========================================
            // 순방향 기구학 (DH 체인 누적 계산)
            // ========================================

            /// <summary>
            /// DH 체인의 누적 회전 행렬 계산 (확정된 스텝만).
            /// upToStep이 -1이면 전체 스텝.
            /// </summary>
            public double[,] GetEndpointRotation(int upToStep = -1)
            {
                double[,] R = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                int limit = upToStep < 0 ? m_lstSteps.Count : Math.Min(upToStep, m_lstSteps.Count);

                for (int i = 0; i < limit; i++)
                {
                    DhStep step = m_lstSteps[i];
                    switch (step.Action)
                    {
                        case BuildAction.Init:
                            R = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                            break;
                        case BuildAction.RedRotate:
                            R = MatMul3x3(R, RotX3(step.Value));
                            break;
                        case BuildAction.BlueRotate:
                            R = MatMul3x3(R, RotZ3(step.Value));
                            break;
                        case BuildAction.GreenRotate:
                            R = MatMul3x3(R, RotY3(-step.Value));
                            break;
                    }
                }
                return R;
            }

            /// <summary>
            /// DH 체인의 누적 위치 계산 (순방향 기구학).
            /// upToStep이 -1이면 전체 스텝.
            /// </summary>
            public void GetEndpointPosition(int upToStep, out double px, out double py, out double pz)
            {
                double[,] R = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                px = py = pz = 0;
                int limit = upToStep < 0 ? m_lstSteps.Count : Math.Min(upToStep, m_lstSteps.Count);

                for (int i = 0; i < limit; i++)
                {
                    DhStep step = m_lstSteps[i];
                    switch (step.Action)
                    {
                        case BuildAction.Init:
                            R = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                            px = py = pz = 0;
                            break;
                        case BuildAction.RedMove:
                            px += R[0, 0] * step.Value;
                            py += R[1, 0] * step.Value;
                            pz += R[2, 0] * step.Value;
                            break;
                        case BuildAction.BlueMove:
                            px += R[0, 2] * step.Value;
                            py += R[1, 2] * step.Value;
                            pz += R[2, 2] * step.Value;
                            break;
                        case BuildAction.GreenMove:
                            px += R[0, 1] * step.Value;
                            py += R[1, 1] * step.Value;
                            pz += R[2, 1] * step.Value;
                            break;
                        case BuildAction.RedRotate:
                            R = MatMul3x3(R, RotX3(step.Value));
                            break;
                        case BuildAction.BlueRotate:
                            R = MatMul3x3(R, RotZ3(step.Value));
                            break;
                        case BuildAction.GreenRotate:
                            R = MatMul3x3(R, RotY3(-step.Value));
                            break;
                    }
                }
            }

            // ========================================
            // STL DH 라인 format 보존 / world<->local 변환 (Phase R1 통합)
            //   원본 ParseStlParams / BuildStlDhLines 는 위쪽 정의.
            //   아래는 MakeUrdf 가 사용해 오던 헬퍼들을 라이브러리로 흡수한 것.
            // ========================================

            /// <summary>DH 텍스트에서 @ 로 시작하는 STL 라인을 모두 제거.
            /// 체인 변환 라인은 유지 → STL 메시 숨기되 운동학 계산은 그대로.</summary>
            public static string StripStlLines(string dh)
            {
                if (string.IsNullOrEmpty(dh)) return dh;
                string[] lines = dh.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                StringBuilder sb = new StringBuilder();
                foreach (string line in lines)
                {
                    if (line.TrimStart().StartsWith("@")) continue;
                    if (sb.Length > 0) sb.Append("\r\n");
                    sb.Append(line);
                }
                return sb.ToString();
            }

            /// <summary>STL DH 라인의 offset + rotation 갱신, **신/구 format (color, bracket) 보존**.
            /// 기존 BuildStlDhLines 는 color 인자 X (-1 고정) + 구형 format 만 → 색/형식 손실.
            /// 이 메서드는 원본을 파싱한 뒤 같은 format 그대로 재구성.</summary>
            public static string RebuildStlDhFull(string originalDh,
                float ox, float oy, float oz, float pan, float tilt, float swing)
            {
                if (string.IsNullOrEmpty(originalDh)) return originalDh;
                bool isNewFormat = originalDh.Contains("],[") || originalDh.Contains("[");
                string raw = originalDh.TrimStart('@').Replace("[", "").Replace("]", "");
                string[] parts = raw.Split(',');
                if (parts.Length < 11) return originalDh;
                string filename = parts[0].Trim();
                string color    = parts[1].Trim();
                string scale    = parts[2].Trim();
                string type     = parts[3].Trim();
                string num      = parts[4].Trim();
                CultureInfo ci = CultureInfo.InvariantCulture;
                if (isNewFormat)
                    return string.Format(ci,
                        "@[{0},{1},{2}],[{3},{4}],[{5:F2},{6:F2},{7:F2}],[{8:F1},{9:F1},{10:F1}]",
                        filename, color, scale, type, num, ox, oy, oz, pan, tilt, swing);
                return string.Format(ci,
                    "@{0},{1},{2},{3},{4},{5:F2},{6:F2},{7:F2},{8:F1},{9:F1},{10:F1}",
                    filename, color, scale, type, num, ox, oy, oz, pan, tilt, swing);
            }

            /// <summary>STL DH 라인에 BuildAction 의 delta 를 적용, format 보존.
            /// drag preview / wheel step 등에서 사용.</summary>
            public static string MakeStlPreviewDhLinesFormatPreserved(
                string originalDh, BuildAction action, float delta)
            {
                string fn; float ox, oy, oz, pan, tilt, swing;
                if (!ParseStlParams(originalDh, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                    return originalDh;
                switch (action)
                {
                    case BuildAction.RedMove:     ox += delta; break;
                    case BuildAction.GreenMove:   oy += delta; break;
                    case BuildAction.BlueMove:    oz += delta; break;
                    case BuildAction.RedRotate:   tilt += delta; break;
                    case BuildAction.GreenRotate: pan += delta; break;
                    case BuildAction.BlueRotate:  swing += delta; break;
                }
                return RebuildStlDhFull(originalDh, ox, oy, oz, pan, tilt, swing);
            }

            // ──────────────────────────────────────────────────────────────────
            // 선택 라인 직접 편집 (축 ±버튼 / 휠)
            //   아무 줄도 선택하지 않았으면 앱이 지금까지처럼 체인 끝에 새 스텝을 붙이고,
            //   3D(또는 텍스트)에서 줄 하나를 고르면 그 줄의 값을 바로 증감한다.
            //   ★모터/수식/초기화 줄은 순수 [0,0,0,0] 유지가 규약 (적용 순서 미정의) —
            //     값을 넣지 않고 바로 뒤에 새 줄을 끼운다.
            // ──────────────────────────────────────────────────────────────────

            /// <summary>축 액션 → DH 첫 그룹 필드 인덱스 (A0 D1 Θ2 α3).
            /// Y축(초록)은 대응 필드가 없어 -1 — Θ 또는 α ±90 으로 감싼 3줄 합성이 규약.</summary>
            private static int GetDhFieldIndex(BuildAction action)
            {
                switch (action)
                {
                    case BuildAction.RedMove:    return 0;   // A     : X 이동
                    case BuildAction.BlueMove:   return 1;   // D     : Z 이동
                    case BuildAction.BlueRotate: return 2;   // Theta : Z 회전
                    case BuildAction.RedRotate:  return 3;   // Alpha : X 회전
                }
                return -1;
            }

            private static string DhFieldName(int field)
            {
                switch (field)
                {
                    case 0: return "A";
                    case 1: return "D";
                    case 2: return "Θ";
                    case 3: return "α";
                }
                return "?";
            }

            /// <summary>10번째 필드(각도 수식)를 가진 줄인가 — 수식 줄은 값을 건드리지 않는다.</summary>
            private static bool HasFormulaField(string dhLine)
            {
                if (string.IsNullOrEmpty(dhLine)) return false;
                string[] parts = dhLine.Replace("[", "").Replace("]", "").Split(',');
                return parts.Length >= 10 && parts[9].Trim().Length > 0;
            }

            /// <summary>Y축 합성(Θ 또는 α ±90 으로 감싼 3줄)의 가운데 = 값 운반 줄인지 확인.
            /// 맞으면 그 줄에서 Y 값을 담는 필드 인덱스, 아니면 -1.</summary>
            private static int GetGreenCarrierField(string[] lines, int index, BuildAction action)
            {
                if (lines == null || index <= 0 || index + 1 >= lines.Length) return -1;
                float pA, pD, pTh, pAl, nA, nD, nTh, nAl;
                int pAx, pDir, pIn, nAx, nDir, nIn;
                ParseDhValues(lines[index - 1], out pA, out pD, out pTh, out pAl, out pAx, out pDir, out pIn);
                ParseDhValues(lines[index + 1], out nA, out nD, out nTh, out nAl, out nAx, out nDir, out nIn);
                if (pAx >= 0 || nAx >= 0 || pA != 0 || nA != 0 || pD != 0 || nD != 0) return -1;

                if (action == BuildAction.GreenMove
                    && pTh == 90 && nTh == -90 && pAl == 0 && nAl == 0)
                    return 0;   // 감싼 프레임 안에서는 A 가 Y 이동
                if (action == BuildAction.GreenRotate
                    && pAl == 90 && nAl == -90 && pTh == 0 && nTh == 0)
                    return 2;   // 감싼 프레임 안에서는 Θ 가 Y 회전
                return -1;
            }

            /// <summary>DH 줄의 첫 `[ ]` 그룹(A,D,Θ,α) 중 한 필드만 교체 —
            /// 나머지 필드·그룹·수식·들여쓰기는 원문 그대로 둔다.</summary>
            private static bool ReplaceDhField(string body, int field, string value, out string result)
            {
                result = body;
                if (body == null || field < 0 || field > 3) return false;
                int open = body.IndexOf('[');
                int close = (open >= 0) ? body.IndexOf(']', open + 1) : -1;
                if (open >= 0 && close > open)
                {
                    string[] parts = body.Substring(open + 1, close - open - 1).Split(',');
                    if (parts.Length <= field) return false;
                    parts[field] = value;
                    result = body.Substring(0, open + 1) + string.Join(",", parts) + body.Substring(close);
                    return true;
                }
                // 괄호 없는 옛 형식: 앞 4개가 A,D,Θ,α
                string[] flat = body.Split(',');
                if (flat.Length <= field) return false;
                flat[field] = value;
                result = string.Join(",", flat);
                return true;
            }

            /// <summary>편집 결과 숫자 표기 — 정수면 정수로, 아니면 원값 유지 (F1 반올림 금지).</summary>
            private static string FmtEditVal(float v)
            {
                if (v == (float)Math.Round(v) && Math.Abs(v) < 1e7f)
                    return ((int)Math.Round(v)).ToString(CultureInfo.InvariantCulture);
                return v.ToString("0.######", CultureInfo.InvariantCulture);
            }

            private static string StlFieldName(BuildAction action)
            {
                switch (action)
                {
                    case BuildAction.RedMove:     return "X";
                    case BuildAction.GreenMove:   return "Y";
                    case BuildAction.BlueMove:    return "Z";
                    case BuildAction.RedRotate:   return "Tilt";
                    case BuildAction.GreenRotate: return "Pan";
                    case BuildAction.BlueRotate:  return "Swing";
                }
                return "?";
            }

            private static float StlFieldValue(BuildAction action,
                float ox, float oy, float oz, float pan, float tilt, float swing)
            {
                switch (action)
                {
                    case BuildAction.RedMove:     return ox;
                    case BuildAction.GreenMove:   return oy;
                    case BuildAction.BlueMove:    return oz;
                    case BuildAction.RedRotate:   return tilt;
                    case BuildAction.GreenRotate: return pan;
                    case BuildAction.BlueRotate:  return swing;
                }
                return 0;
            }

            /// <summary>제자리 수정이 불가능한 줄(모터/수식/초기화/지시자) — 바로 뒤에 새 줄을 끼운다.</summary>
            private static DhLineEdit MakeInsertEdit(BuildAction action, float delta, string reason)
            {
                DhLineEdit r = new DhLineEdit();
                DhStep s = CreateStep(action, delta);
                if (s == null || string.IsNullOrEmpty(s.DhLines)) return r;
                r.Ok = true;
                r.Inserted = true;
                r.NewLines = s.DhLines.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                r.CarrierOffset = (r.NewLines.Length >= 3) ? 1 : 0;   // Y축 합성은 가운데가 값 운반
                r.OldValue = 0;
                r.NewValue = (float)Math.Round(delta);
                r.FieldName = GetAxisName(action);
                r.Description = s.Description;
                r.Reason = reason;
                return r;
            }

            /// <summary>선택된 한 줄에 축 delta 를 적용한다.
            /// lines 는 전체 텍스트 줄 배열 (Y축 합성의 감싸기 줄을 앞뒤로 봐야 해서 필요), index 는 대상 줄.
            /// 반환값 Ok=false 면 호출 측이 기존 동작(체인 끝에 누적)을 그대로 하면 된다.</summary>
            public static DhLineEdit ApplyLineDelta(string[] lines, int index, BuildAction action, float delta)
            {
                DhLineEdit r = new DhLineEdit();
                if (lines == null || index < 0 || index >= lines.Length) return r;
                if (action == BuildAction.None || Math.Abs(delta) < 1e-4f) return r;

                string body = lines[index];
                string comment = "";
                int cpos = body.IndexOf("//");
                if (cpos >= 0) { comment = body.Substring(cpos); body = body.Substring(0, cpos); }
                string trimmed = body.Trim();

                // ① @STL 줄 — 모델 자체의 offset/회전을 증감 (드래그·수동이동과 같은 결과)
                if (trimmed.StartsWith("@"))
                {
                    string fn; float ox, oy, oz, pan, tilt, swing;
                    if (!ParseStlParams(trimmed, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                        return MakeInsertEdit(action, delta, "@줄 파싱 실패");
                    r.OldValue = StlFieldValue(action, ox, oy, oz, pan, tilt, swing);
                    r.NewValue = r.OldValue + delta;
                    r.FieldName = "STL " + StlFieldName(action);
                    r.Ok = true;
                    r.Inserted = false;
                    r.NewLines = new string[] {
                        MakeStlPreviewDhLinesFormatPreserved(trimmed, action, delta)
                        + (comment.Length > 0 ? " " + comment.Trim() : "") };
                    return r;
                }

                // ② 지시자($)/함수(#)/빈 줄 — 값이 없다 → 뒤에 새 줄
                if (trimmed.Length == 0) return MakeInsertEdit(action, delta, "빈 줄");
                if (trimmed.StartsWith("$")) return MakeInsertEdit(action, delta, "지시자 줄");
                if (trimmed.StartsWith("#")) return MakeInsertEdit(action, delta, "함수 줄");

                // ③ DH 줄
                float A, D, Th, Al; int Axis, Dir, Init;
                ParseDhValues(trimmed, out A, out D, out Th, out Al, out Axis, out Dir, out Init);
                if (Axis >= 0)             return MakeInsertEdit(action, delta, "모터 줄");
                if (HasFormulaField(trimmed)) return MakeInsertEdit(action, delta, "수식 줄");
                if (Init == 1)             return MakeInsertEdit(action, delta, "초기화 줄");

                int field = GetDhFieldIndex(action);
                if (field < 0)
                {
                    // Y축 — 이 줄이 Y 합성의 값 운반 줄이면 제자리 누적, 아니면 새 합성 3줄 삽입
                    field = GetGreenCarrierField(lines, index, action);
                    if (field < 0) return MakeInsertEdit(action, delta, "Y축 합성");
                }

                float oldVal = (field == 0) ? A : (field == 1) ? D : (field == 2) ? Th : Al;
                float newVal = oldVal + delta;
                string edited;
                if (!ReplaceDhField(body, field, FmtEditVal(newVal), out edited))
                    return MakeInsertEdit(action, delta, "필드 없음");

                r.Ok = true;
                r.Inserted = false;
                r.NewLines = new string[] { edited + comment };
                r.FieldName = DhFieldName(field);
                r.OldValue = oldVal;
                r.NewValue = newVal;
                return r;
            }

            // ================================================================
            // 선택 줄의 "현재 상태" 읽기/쓰기 (2026-09-11)
            //
            // ±버튼·휠은 **명령**(증감)이고, 이 API 는 **상태**(지금 값이 얼마인가)다.
            // 같은 줄을 두 방식으로 보게 되므로 해석은 반드시 한 곳 — 여기 — 에서만 한다.
            // 앱이 따로 파싱하면 "버튼이 올린 값"과 "칸에 보이는 값"이 어긋난다.
            // ================================================================

            /// <summary>선택된 한 줄의 현재 상태. Has* 는 그 축에 값이 실제로 있는지.</summary>
            public class DhLineState
            {
                public bool Ok;                 // 해석 성공 (빈 줄/$/# 이면 false)
                public bool IsStl;              // @STL 줄
                public bool HasMeta;            // joint/dir/init 을 가진 줄인가 (DH 줄만 true)
                public string StlFile = "";

                public float XMove, YMove, ZMove;         // RedMove / GreenMove / BlueMove
                public float XRotate, YRotate, ZRotate;   // RedRotate / GreenRotate / BlueRotate
                public bool HasXMove, HasYMove, HasZMove;
                public bool HasXRotate, HasYRotate, HasZRotate;

                public int Joint = -1;          // DH 의 Axis 필드 (모터 번호, -1 = 없음)
                public int Dir;
                public int Init;

                /// <summary>제자리 수정이 안 되는 줄이면 그 사유 (모터 줄/수식 줄/초기화 줄).
                /// 값 표시는 그대로 하되, 고치면 ±버튼과 똑같이 **새 줄이 삽입**된다.</summary>
                public string Note = "";

                /// <summary>축 → 현재 값.</summary>
                public float ValueOf(BuildAction a)
                {
                    switch (a)
                    {
                        case BuildAction.RedMove: return XMove;
                        case BuildAction.GreenMove: return YMove;
                        case BuildAction.BlueMove: return ZMove;
                        case BuildAction.RedRotate: return XRotate;
                        case BuildAction.GreenRotate: return YRotate;
                        case BuildAction.BlueRotate: return ZRotate;
                    }
                    return 0f;
                }

                /// <summary>축 → 그 값이 이 줄에 실제로 있는가.</summary>
                public bool HasValue(BuildAction a)
                {
                    switch (a)
                    {
                        case BuildAction.RedMove: return HasXMove;
                        case BuildAction.GreenMove: return HasYMove;
                        case BuildAction.BlueMove: return HasZMove;
                        case BuildAction.RedRotate: return HasXRotate;
                        case BuildAction.GreenRotate: return HasYRotate;
                        case BuildAction.BlueRotate: return HasZRotate;
                    }
                    return false;
                }
            }

            /// <summary>선택된 줄(index)의 현재 상태를 읽는다.
            ///
            /// @STL 줄 — 여섯 축이 모두 있다 (ApplyStlDelta 와 **같은 대응**):
            ///   X이동=offX  Y이동=offY  Z이동=offZ  X회전=tilt  Y회전=pan  Z회전=swing
            /// DH 줄 — X이동=A, Z이동=D, Z회전=Θ, X회전=α 는 항상 있고,
            ///   Y 는 합성 3줄의 **값 운반 줄**일 때만 있다(GetGreenCarrierField).
            /// 빈 줄 / $지시자 / #함수 는 Ok=false.</summary>
            public static DhLineState ReadLineState(string[] lines, int index)
            {
                DhLineState st = new DhLineState();
                if (lines == null || index < 0 || index >= lines.Length)
                { st.Note = "선택 없음"; return st; }

                string body = lines[index];
                int cpos = body.IndexOf("//");
                if (cpos >= 0) body = body.Substring(0, cpos);
                string trimmed = body.Trim();

                if (trimmed.Length == 0) { st.Note = "빈 줄"; return st; }
                if (trimmed.StartsWith("$")) { st.Note = "지시자 줄"; return st; }
                if (trimmed.StartsWith("#")) { st.Note = "함수 줄"; return st; }

                if (trimmed.StartsWith("@"))
                {
                    string fn; float ox, oy, oz, pan, tilt, swing;
                    if (!ParseStlParams(trimmed, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                    { st.Note = "@줄 파싱 실패"; return st; }
                    st.Ok = true; st.IsStl = true; st.HasMeta = false;
                    st.StlFile = fn;
                    st.XMove = ox; st.YMove = oy; st.ZMove = oz;
                    st.XRotate = tilt; st.YRotate = pan; st.ZRotate = swing;
                    st.HasXMove = st.HasYMove = st.HasZMove = true;
                    st.HasXRotate = st.HasYRotate = st.HasZRotate = true;
                    return st;
                }

                float A, D, Th, Al; int axis, dir, init;
                ParseDhValues(trimmed, out A, out D, out Th, out Al, out axis, out dir, out init);
                st.Ok = true; st.IsStl = false; st.HasMeta = true;
                st.Joint = axis; st.Dir = dir; st.Init = init;
                st.XMove = A; st.HasXMove = true;
                st.ZMove = D; st.HasZMove = true;
                st.ZRotate = Th; st.HasZRotate = true;
                st.XRotate = Al; st.HasXRotate = true;

                // Y 는 합성 프레임 안의 값 운반 줄일 때만 실체가 있다
                int fy = GetGreenCarrierField(lines, index, BuildAction.GreenMove);
                if (fy >= 0) { st.YMove = (fy == 0) ? A : D; st.HasYMove = true; }
                int fyr = GetGreenCarrierField(lines, index, BuildAction.GreenRotate);
                if (fyr >= 0) { st.YRotate = (fyr == 2) ? Th : Al; st.HasYRotate = true; }

                // 제자리 수정이 막히는 줄 — 값은 보여 주되 고치면 새 줄이 삽입된다
                if (axis >= 0) st.Note = "모터 줄";
                else if (HasFormulaField(trimmed)) st.Note = "수식 줄";
                else if (init == 1) st.Note = "초기화 줄";
                return st;
            }

            /// <summary>축의 값을 **절대값으로** 지정한다 (칸에 숫자를 직접 써 넣는 경로).
            /// 내부적으로는 (새값 − 현재값) 델타로 바꿔 ApplyLineDelta 를 그대로 탄다 —
            /// ±버튼과 완전히 같은 코드라 두 경로가 어긋날 수 없다.</summary>
            public static DhLineEdit SetLineValue(string[] lines, int index, BuildAction action, float newValue)
            {
                DhLineState st = ReadLineState(lines, index);
                float cur = st.Ok ? st.ValueOf(action) : 0f;
                return ApplyLineDelta(lines, index, action, newValue - cur);
            }

            /// <summary>DH 줄의 joint(Axis)/dir/init 을 교체한다. 나머지 필드·수식·주석·들여쓰기는 원문 유지.
            /// 두 번째 `[ ]` 그룹만 건드린다. @STL·빈 줄 등 메타가 없는 줄이면 false.</summary>
            public static bool SetDhMeta(string line, int joint, int dir, int init, out string result)
            {
                result = line;
                if (line == null) return false;
                string body = line, comment = "";
                int cpos = body.IndexOf("//");
                if (cpos >= 0) { comment = body.Substring(cpos); body = body.Substring(0, cpos); }
                string trimmed = body.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("@")
                    || trimmed.StartsWith("$") || trimmed.StartsWith("#")) return false;

                string sj = joint.ToString(CultureInfo.InvariantCulture);
                string sd = dir.ToString(CultureInfo.InvariantCulture);
                string si = init.ToString(CultureInfo.InvariantCulture);

                int open1 = body.IndexOf('[');
                int close1 = (open1 >= 0) ? body.IndexOf(']', open1 + 1) : -1;
                int open2 = (close1 >= 0) ? body.IndexOf('[', close1 + 1) : -1;
                int close2 = (open2 >= 0) ? body.IndexOf(']', open2 + 1) : -1;
                if (open2 >= 0 && close2 > open2)
                {
                    string[] parts = body.Substring(open2 + 1, close2 - open2 - 1).Split(',');
                    List<string> outp = new List<string>(parts);
                    while (outp.Count < 3) outp.Add("0");
                    outp[0] = sj; outp[1] = sd; outp[2] = si;
                    result = body.Substring(0, open2 + 1) + string.Join(",", outp.ToArray())
                           + body.Substring(close2) + comment;
                    return true;
                }

                // 괄호 없는 옛 형식: 5·6·7번째가 Axis,Dir,Init
                string[] flat = body.Split(',');
                if (flat.Length < 7) return false;
                flat[4] = sj; flat[5] = sd; flat[6] = si;
                result = string.Join(",", flat) + comment;
                return true;
            }

            /// <summary>STL DH 라인의 color 필드 (2번째 token) 를 정수로 파싱.</summary>
            public static int GetStlColorFromDhLines(string dh)
            {
                if (string.IsNullOrEmpty(dh)) return 0;
                string raw = dh.TrimStart('@').Replace("[", "").Replace("]", "");
                string[] parts = raw.Split(',');
                if (parts.Length < 2) return 0;
                int color = 0;
                int.TryParse(parts[1].Trim(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out color);
                return color;
            }

            /// <summary>STL color 의 채도 최대화된 보색 계산.
            /// 무채색 (R=G=B) 은 본색 밝기에 따라 흑/백 강제 — plane 모드 hover overlay 와 동일.</summary>
            public static System.Drawing.Color GetStlComplementColor(string dh)
            {
                int origColor = GetStlColorFromDhLines(dh);
                int rgb = origColor & 0xFFFFFF;
                int compRgb = 0xFFFFFF ^ rgb;
                int rC = (compRgb >> 16) & 0xFF;
                int gC = (compRgb >> 8) & 0xFF;
                int bC = compRgb & 0xFF;
                int maxC = Math.Max(rC, Math.Max(gC, bC));
                int minC = Math.Min(rC, Math.Min(gC, bC));
                if (maxC == minC)
                {
                    int origMax = Math.Max((rgb >> 16) & 0xFF,
                                  Math.Max((rgb >> 8) & 0xFF, rgb & 0xFF));
                    if (origMax > 127) { rC = gC = bC = 0; }
                    else { rC = gC = bC = 255; }
                }
                else if (maxC < 255)
                {
                    float k = 255.0f / maxC;
                    rC = Math.Min(255, (int)(rC * k));
                    gC = Math.Min(255, (int)(gC * k));
                    bC = Math.Min(255, (int)(bC * k));
                }
                return System.Drawing.Color.FromArgb(rC, gC, bC);
            }

            /// <summary>STL local (cx,cy,cz) → world 좌표 (또는 normal 방향).
            /// 변환: DH_endpoint(stepIdx) · Translate(stlOffset) · Rm(pan,tilt,swing) · v
            /// isNormal=true 면 translation 적용 X (방향 벡터 전용).</summary>
            public void StlLocalToWorld(int stepIdx,
                float cx, float cy, float cz, bool isNormal,
                out double wx, out double wy, out double wz)
            {
                wx = wy = wz = 0;
                DhStep step = GetStep(stepIdx);
                if (step.Action != BuildAction.Stl) return;
                string fn; float ox, oy, oz, pan, tilt, swing;
                if (!ParseStlParams(step.DhLines, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                    return;

                double[,] Rm = StlLocalRotation(pan, tilt, swing);
                double lx = Rm[0, 0] * cx + Rm[0, 1] * cy + Rm[0, 2] * cz;
                double ly = Rm[1, 0] * cx + Rm[1, 1] * cy + Rm[1, 2] * cz;
                double lz = Rm[2, 0] * cx + Rm[2, 1] * cy + Rm[2, 2] * cz;
                if (!isNormal) { lx += ox; ly += oy; lz += oz; }

                double epx, epy, epz;
                GetEndpointPosition(stepIdx, out epx, out epy, out epz);
                double[,] Re = GetEndpointRotation(stepIdx);
                wx = Re[0, 0] * lx + Re[0, 1] * ly + Re[0, 2] * lz;
                wy = Re[1, 0] * lx + Re[1, 1] * ly + Re[1, 2] * lz;
                wz = Re[2, 0] * lx + Re[2, 1] * ly + Re[2, 2] * lz;
                if (!isNormal) { wx += epx; wy += epy; wz += epz; }
            }

            /// <summary>world 좌표 delta → 해당 STL 의 DH endpoint 로컬 delta
            /// (= STL offset 에 직접 가산할 양).
            /// 변환: offset_delta = Re⁻¹ · world_delta  (Re 직교 → Reᵀ = Re⁻¹)
            /// STL offset 은 endpoint 로컬 (Rm 적용 전) 이라 Rm⁻¹ 불필요.</summary>
            public void WorldDeltaToStlOffsetDelta(int stepIdx,
                double dx, double dy, double dz,
                out float dox, out float doy, out float doz)
            {
                double[,] Re = GetEndpointRotation(stepIdx);
                dox = (float)(Re[0, 0] * dx + Re[1, 0] * dy + Re[2, 0] * dz);
                doy = (float)(Re[0, 1] * dx + Re[1, 1] * dy + Re[2, 1] * dz);
                doz = (float)(Re[0, 2] * dx + Re[1, 2] * dy + Re[2, 2] * dz);
            }

            // ========================================
            // 유틸리티
            // ========================================

            /// <summary>축 이름 표시 문자열 반환.</summary>
            public static string GetAxisName(BuildAction action)
            {
                switch (action)
                {
                    case BuildAction.RedMove: return "빨간(X) 이동";
                    case BuildAction.BlueMove: return "파란(Z) 이동";
                    case BuildAction.GreenMove: return "초록(Y) 이동";
                    case BuildAction.RedRotate: return "빨간(X) 회전";
                    case BuildAction.BlueRotate: return "파란(Z) 회전";
                    case BuildAction.GreenRotate: return "초록(Y) 회전";
                    default: return "";
                }
            }

            /// <summary>회전 액션인지 여부.</summary>
            public static bool IsRotation(BuildAction action)
            {
                return action == BuildAction.RedRotate
                    || action == BuildAction.BlueRotate
                    || action == BuildAction.GreenRotate;
            }

            /// <summary>
            /// 법선 벡터로부터 가장 가까운 바운딩박스 면 결정.
            /// 0=+X, 1=-X, 2=+Y, 3=-Y, 4=+Z, 5=-Z
            /// </summary>
            public static int DetermineBBFace(float nx, float ny, float nz)
            {
                float ax = Math.Abs(nx), ay = Math.Abs(ny), az = Math.Abs(nz);
                if (ax >= ay && ax >= az)
                    return nx > 0 ? 0 : 1;
                if (ay >= az)
                    return ny > 0 ? 2 : 3;
                return nz > 0 ? 4 : 5;
            }

            /// <summary>
            /// BuildAction에 따라 STL 파라미터에 delta 적용.
            /// 이동: offX/Y/Z, 회전: pan(Y)/tilt(X)/swing(Z)
            /// </summary>
            public static void ApplyStlDelta(DhStep step, BuildAction action, float delta)
            {
                string fn; float ox, oy, oz, pan, tilt, swing;
                if (!ParseStlParams(step.DhLines, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                    return;

                switch (action)
                {
                    case BuildAction.RedMove:     ox += delta; break;
                    case BuildAction.BlueMove:    oz += delta; break;
                    case BuildAction.GreenMove:   oy += delta; break;
                    case BuildAction.RedRotate:   tilt += delta; break;
                    case BuildAction.BlueRotate:  swing += delta; break;
                    case BuildAction.GreenRotate: pan += delta; break;
                }

                step.DhLines = BuildStlDhLines(fn, ox, oy, oz, pan, tilt, swing);
            }

            /// <summary>
            /// STL DhLines 문자열에 delta를 적용한 새 DhLines 반환.
            /// </summary>
            public static string GetStlPreviewDhLines(string dhLines, BuildAction action, float delta)
            {
                string fn; float ox, oy, oz, pan, tilt, swing;
                if (!ParseStlParams(dhLines, out fn, out ox, out oy, out oz, out pan, out tilt, out swing))
                    return dhLines;

                switch (action)
                {
                    case BuildAction.RedMove:     ox += delta; break;
                    case BuildAction.BlueMove:    oz += delta; break;
                    case BuildAction.GreenMove:   oy += delta; break;
                    case BuildAction.RedRotate:   tilt += delta; break;
                    case BuildAction.BlueRotate:  swing += delta; break;
                    case BuildAction.GreenRotate: pan += delta; break;
                }

                return BuildStlDhLines(fn, ox, oy, oz, pan, tilt, swing);
            }
        }
    }
}
