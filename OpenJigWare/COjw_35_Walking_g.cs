using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OpenJigWare
{
    public partial class Ojw
    {
        /// <summary>
        /// Original WalkingMaker equations with a reusable numeric output buffer.
        /// Included in OpenJigWare.dll as Ojw.COjwWalking_g, alongside the legacy COjwWalking.
        /// A single control thread owns each instance. Prepare after edits, before timed evaluation.
        /// Historical F19 string concatenation and constant neck output are intentionally retained.
        /// </summary>
        public sealed partial class COjwWalking_g
        {
            public struct FootTarget
            {
                public int FunctionId;
                public string IdText;
                // Original engine axes: lateral, vertical, forward. Relative to the caller's FK pose.
                public float X, Y, Z;
                public bool Enabled { get { return IdText != null; } }
            }

            public struct JointCommand
            {
                public int MotorId;
                public string IdText;
                public bool Additive;
                public float Value;
            }

            /// <summary>Caller-owned buffer; EvaluateFrame overwrites it. No history is retained.</summary>
            public sealed class Frame
            {
                public FootTarget Right, Left;
                public readonly JointCommand[] Joints = new JointCommand[16];
                public int JointCount, Mode, Step, FrameCount, Time, Delay, RepeatCount;
                public int SwingLeg; // -1 double support, 0 right swing, 1 left swing
                public float SwingProgress, Tilt;
                public bool MirrorBeforeGroup, MirrorAfterGroup, Valid;
                public string TimeText, DelayText, ExtraCommand, TrailingCommand;

                internal void Clear()
                {
                    Valid = false; JointCount = 0; Right = new FootTarget(); Left = new FootTarget();
                    SwingLeg = -1; SwingProgress = 0; Tilt = 0;
                    ExtraCommand = TrailingCommand = null;
                }

                public string ToCommandString()
                {
                    if (!Valid) return string.Empty;
                    StringBuilder sb = new StringBuilder(512);
                    sb.Append("E\t1\t");
                    AppendFoot(sb, Right); AppendFoot(sb, Left);
                    sb.Append("S\t").Append(TimeText).Append("\tD\t").Append(DelayText).Append('\t');
                    for (int i = 0; i < JointCount; i++)
                    {
                        JointCommand j = Joints[i];
                        sb.Append(j.Additive ? 'P' : 'T').Append(j.IdText).Append('\t').Append(j.Value).Append('\t');
                    }
                    if (MirrorBeforeGroup) sb.Append("X\t-1\t");
                    sb.Append("G\t").Append(Mode + 1).Append('\t');
                    if (MirrorAfterGroup) sb.Append("X\t-1\t");
                    if (RepeatCount > 1)
                        sb.Append("@SET_COMMAND,1\t@SET_DATA0,").Append(FrameCount - 1)
                          .Append("\t@SET_DATA1,").Append(RepeatCount).Append('\t');
                    if (!string.IsNullOrEmpty(TrailingCommand)) sb.Append(TrailingCommand).Append('\t');
                    if (ExtraCommand != null) sb.Append(ExtraCommand.Replace(' ', '\t'));
                    return sb.ToString();
                }

                private static void AppendFoot(StringBuilder sb, FootTarget f)
                {
                    if (f.Enabled) sb.Append('I').Append(f.IdText).Append('\t').Append(f.X)
                        .Append('\t').Append(f.Y).Append('\t').Append(f.Z).Append('\t');
                }
            }

            // Original spreadsheet parameter order: C# index = Making!B row - 4.
                public string[] m_pstrParams = new string[] {
                    "첫프레임 속도(종료시 제외)",
                    "Sway 고정(발들어 올리는 시점)",
                    "1점지지 시작구간",
                    "시작점(Sway Sequence Offset)",
                    "1점지지 Step 수",
                    "2점지지 Step 수",
                    "Sway",
                    "발들어 올리는 높이",
                    "전진량",
                    "수식 Leg(Right)",
                    "수식 Leg(Left)",
                    "전진성 타원(앞)",
                    "전진성 타원(뒤)",
                    "상체 좌우기울임(상체회전각)",
                    "",
                    "무게중심(+전진)",
                    "걸음새를 위한 앉기",
                    "다리벌리기 ",
                    "들어올리는 다리 벌리기",
                    "엉치(W) ID(Right)",
                    "엉치(W) ID(Left)",
                    "발목(W) ID(Right)",
                    "발목(W) ID(Left)",
                    "발목SwayOffset",
                    "속도시간",
                    "딜레이",
                    "Sway Shift",
                    "팔Up ID(Right)",
                    "팔Up ID(Left)",
                    "팔Up Shift",
                    "팔Up 동작각",
                    "팔W ID(Right)",
                    "팔W ID(Left)",
                    "팔W Shift",
                    "팔W 동작각",
                    "걸음시 궤적높이(Elastic)",
                    "평행회전각(대각이동)",
                    "슬립 카운터(1점지지보다 숫자가 낮아야 한다.)",
                    "",
                    "고관절 Tilt ID(Right)",
                    "고관절 Tilt ID(Left)",
                    "고관절 숙이기",
                    "발목 Tilt ID(Right)",
                    "발목 Tilt ID(Left)",
                    "뜨는 발바닥 롤링 각",
                    "발목 숙이기",
                    "고관절 회전을 이용한 회전각",
                    "고관절 Pan(오른다리) ID",
                    "고관절 Pan(왼다리) ID",
                    "덧붙임명령어",
                    "허리ID",
                    "허리 동작각",
                    "목 ID",
                    "목 동작각"
                };
                private string[] m_pstrParams_Value_Start = new string[] {
                "500",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                ""
            };
                private string[] m_pstrParams_Value_Ing = new string[] {
                "",
                "1",
                "0",
                "0",
                "4",
                "2",
                "10",
                "50",
                "40",
                "0",
                "1",
                "60",
                "30",
                "0",
                "",
                "5",
                "10",
                "0",
                "2",
                "5",
                "7",
                "10",
                "12",
                "4",
                "50",
                "0",
                "0",
                "1",
                "3",
                "0",
                "0",
                "2",
                "4",
                "-5",
                "0",
                "0",
                "0",
                "0",
                "",
                "",
                "",
                "10",
                "",
                "",
                "-10",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                ""
            };
                private string[] m_pstrParams_Value_End = new string[] {
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                ""
            };

            private const float DEG_TO_RAD = (float)(Math.PI / 180.0);
            private readonly string[,] raw = new string[3, 100];
            private readonly WalkingParamCache[] prepared = new WalkingParamCache[3];
            private readonly List<float>[] tiltHistory = { new List<float>(), new List<float>(), new List<float>() };
            private readonly List<float> allTilt = new List<float>();
            private readonly Frame stringBuffer = new Frame();
            private int walkingCount = 1, streamMode, streamStep;
            private bool mirror;
            private Ojw.CGrid grid;
            // Lazy: numeric-only use does not construct a WinForms control.
            public Ojw.CGrid m_COjwGrid { get { return grid ?? (grid = new Ojw.CGrid()); } }

            private sealed class WalkingParamCache
            {
                public float[] Values, RepeatValues;
                public string[] Names;
                public int[] Ids;
                public int Frames;
                public float SinDirection, CosDirection;
                    public float swayMode, swayAmplitude, slipSwayAmplitude, swaySequenceOffset, swayShift, supportStartOffset, singleSupport, doubleSupport, gateLength;
                    public float rightSlip, leftSlip, fAT, fAU, fBG;
                    public float fD50, liftHeight, nominalFootSpacing, effectiveFootSpacing, rightBodyTiltOffset, leftBodyTiltOffset;
                    public float crouch, repeatCrouch, phaseCrouch;
                    public float stride, forwardEllipse, backwardEllipse, fE17, fE18, forwardWeightShift;
                    public float fDK, fDQ, fDR, directionDegrees, directionRadians;
                    public float fEQ, fER, fBM, fBN;
                    public float fDM, fDN, fDU, waistAmplitude, neckAmplitude;

            }

            public COjwWalking_g()
            {
                for (int i = 0; i < 100; i++)
                {
                    raw[1, i] = i < m_pstrParams_Value_Ing.Length ? m_pstrParams_Value_Ing[i] : "";
                    raw[0, i] = i < m_pstrParams_Value_Start.Length ? m_pstrParams_Value_Start[i] : "";
                    raw[2, i] = i < m_pstrParams_Value_End.Length ? m_pstrParams_Value_End[i] : "";
                }
            }

            private static void CheckMode(int mode)
            {
                if (mode < 0 || mode > 2) throw new ArgumentOutOfRangeException("mode");
            }
            private static void CheckIndex(int index)
            {
                if (index < 0 || index >= 100) throw new ArgumentOutOfRangeException("index");
            }

            // Empty start/end values inherit repeat parameters; explicit overrides survive later edits.
            public string GetData_Str(int mode, int index)
            {
                CheckMode(mode); CheckIndex(index);
                return mode != 1 && string.IsNullOrEmpty(raw[mode, index]) ? raw[1, index] : raw[mode, index];
            }
            public void SetData(int mode, int index, string value)
            {
                CheckMode(mode); CheckIndex(index); value = value ?? "";
                if (raw[mode, index] == value) return;
                raw[mode, index] = value;
                InvalidateParamCache(mode);
            }
            public int SetData_Str(int mode, string[] values)
            {
                CheckMode(mode);
                if (values == null) throw new ArgumentNullException("values");
                if (values.Length > 100) throw new ArgumentException("At most 100 parameters are supported.");
                int changed = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i] ?? "";
                    if (raw[mode, i] != value) { SetData(mode, i, value); changed++; }
                }
                return changed;
            }
            public void InvalidateParamCache(int mode = -1)
            {
                if (mode == -1 || mode == 1)
                    for (int i = 0; i < 3; i++) prepared[i] = null;
                else { CheckMode(mode); prepared[mode] = null; }
            }

            private static readonly int[] IdIndices = { 9, 10, 19, 20, 21, 22, 27, 28, 31, 32, 39, 40, 42, 43, 47, 48, 50, 52 };
            private static readonly int[] NumericIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 15, 16, 17, 18, 23, 24, 25, 26, 29, 30, 33, 34, 35, 36, 37, 41, 44, 45, 46, 51, 53 };
            private static float ReadNumber(string value, int index)
            {
                if (string.IsNullOrWhiteSpace(value)) return 0;
                float result;
                if ((!float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result)
                    && !float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    || float.IsNaN(result) || float.IsInfinity(result))
                    throw new ArgumentException("Invalid walking parameter " + index + ": " + value);
                return result;
            }
            private float[] ReadValues(int mode)
            {
                float[] result = new float[100];
                foreach (int i in NumericIndices) result[i] = ReadNumber(GetData_Str(mode, i), i);
                float s = result[4], d = result[5];
                if (s < 1 || d < 0 || s != Math.Floor(s) || d != Math.Floor(d) || (double)s + d > int.MaxValue / 2)
                    throw new ArgumentException("Support steps must be integers: single >= 1, double >= 0.");
                if (Math.Abs(result[37]) >= s)
                    throw new ArgumentException("The absolute slip count must be less than single-support steps.");
                if (result[0] < 0 || result[24] < 0 || result[25] < 0
                    || result[0] != Math.Floor(result[0]) || result[24] != Math.Floor(result[24]) || result[25] != Math.Floor(result[25])
                    || (double)result[0] > int.MaxValue || (double)result[24] > int.MaxValue || (double)result[25] > int.MaxValue)
                    throw new ArgumentException("Frame time and delay must be nonnegative integer milliseconds.");
                return result;
            }

            /// <summary>Cold path. Call after parameter updates to avoid parsing on the next tick.</summary>
            public void Prepare()
            {
                // Build all three before publication; an invalid mode cannot publish a partial replacement.
                WalkingParamCache[] next = new WalkingParamCache[3];
                for (int i = 0; i < 3; i++) next[i] = prepared[i] ?? BuildCache(i);
                for (int i = 0; i < 3; i++) prepared[i] = next[i];
            }
            private WalkingParamCache BuildCache(int nMode)
            {
                WalkingParamCache cache = new WalkingParamCache();
                cache.Values = ReadValues(nMode); cache.RepeatValues = ReadValues(1);
                cache.Names = new string[100]; cache.Ids = new int[100];
                for (int i = 0; i < 100; i++) { cache.Names[i] = GetData_Str(nMode, i); cache.Ids[i] = -1; }
                foreach (int i in IdIndices)
                {
                    if (string.IsNullOrWhiteSpace(cache.Names[i])) { cache.Names[i] = null; continue; }
                    int id;
                    if (!int.TryParse(cache.Names[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out id) || id < 0)
                        throw new ArgumentException("Invalid function/motor ID at parameter " + i);
                    cache.Ids[i] = id;
                }
                cache.Frames = checked(2 * (int)(cache.RepeatValues[4] + cache.RepeatValues[5]));

                    cache.swayMode = cache.Values[1];
                    cache.swayAmplitude = -cache.Values[6];
                    cache.slipSwayAmplitude = cache.swayAmplitude * 0.5f;
                    cache.swaySequenceOffset = cache.Values[3];
                    cache.swayShift = -cache.Values[26];
                    cache.supportStartOffset = -cache.Values[2];
                    cache.singleSupport = cache.Values[4];
                    cache.doubleSupport = cache.Values[5];
                    cache.gateLength = cache.singleSupport + cache.doubleSupport;
                    float fAA_temp = cache.Values[37];
                    cache.leftSlip = -fAA_temp;
                    cache.rightSlip = (fAA_temp > 0 ? fAA_temp : 0.0f);
                    cache.leftSlip = (cache.leftSlip > 0 ? cache.leftSlip : 0.0f);
                    cache.fAT = cache.Values[23];
                    cache.fAU = cache.fAT * 0.5f;
                    cache.fBG = cache.Values[18];
                    cache.fD50 = cache.Values[46];
                    cache.liftHeight = cache.Values[7];
                    cache.nominalFootSpacing = 72;
                    cache.effectiveFootSpacing = Ojw.CConvert.StrToFloat(cache.Names[17] + cache.nominalFootSpacing);
                    float fB17 = cache.Values[13];
                    cache.rightBodyTiltOffset = cache.effectiveFootSpacing / 2 * (float)Math.Tan(fB17 * DEG_TO_RAD);
                    cache.leftBodyTiltOffset = -cache.rightBodyTiltOffset;
                    cache.crouch = cache.Values[16];
                    cache.repeatCrouch = cache.RepeatValues[16];
                    cache.phaseCrouch = cache.crouch;
                    cache.stride = cache.Values[8];
                    cache.forwardEllipse = cache.Values[11];
                    cache.backwardEllipse = cache.Values[12];
                    cache.fE17 = -fB17;
                    cache.fE18 = fB17;
                    cache.forwardWeightShift = cache.Values[15];
                    cache.fDK = -cache.Values[17];
                    cache.fDQ = cache.Values[33];
                    cache.fDR = cache.Values[34];
                    cache.directionDegrees = cache.Values[36] * ((nMode != 1) ? -1 : 1);
                    cache.directionRadians = cache.directionDegrees * DEG_TO_RAD;
                    cache.fEQ = cache.Values[45];
                    cache.fER = cache.Values[44];
                    cache.fBM = cache.Values[47];
                    cache.fBN = cache.Values[48];
                    cache.fDM = -cache.Values[29];
                    float fCG_val = cache.Values[8];
                    float fB34 = cache.Values[30];
                    cache.fDN = (fCG_val >= 0) ? fB34 : -fB34;
                    cache.fDU = cache.Values[35];
                    cache.waistAmplitude = cache.Values[51];
                    cache.neckAmplitude = cache.Values[53];
                cache.SinDirection = (float)Math.Sin(cache.directionRadians);
                cache.CosDirection = (float)Math.Cos(cache.directionRadians);
                return cache;
            }

            public int SetWalkingCount(int count)
            {
                count = Math.Max(1, count);
                if (walkingCount == count) return 0;
                walkingCount = count; return 1;
            }
            public int GetWalkingCount() { return walkingCount; }
            public void SetMirror(bool value) { mirror = value; }
            public bool GetMirror() { return mirror; }
            public int Size_Gate(int mode) { CheckMode(mode); float[] p = ReadValues(mode); return (int)(p[4] + p[5]); }
            public int Size_Frame(int mode) { return Size_Gate(mode) * (mode == 1 ? 2 : 1); }

            /// <summary>Allocation-free after Prepare, with a reused Frame. One-based legacy step numbering.</summary>
            public void EvaluateFrame(int nStart_0_Repeat_1_End_2, int nStep, Frame target)
            {
                if (target == null) throw new ArgumentNullException("target");
                target.Clear(); CheckMode(nStart_0_Repeat_1_End_2);
                int nMode = nStart_0_Repeat_1_End_2;
                WalkingParamCache cache = prepared[nMode];
                if (cache == null) { Prepare(); cache = prepared[nMode]; }
                int nSize_Frame = cache.Frames;
                if (nStep < 1 || nStep > nSize_Frame) throw new ArgumentOutOfRangeException("nStep");
                int i = nStep;
                // Main phase/trajectory variables have semantic names. Remaining fXX aliases
                // match the original spreadsheet; see README for the migration boundary.

                    float enabledWeight;
                    float fL;
                    float stepPosition;
                    float swayPosition;
                    float swayMode = cache.swayMode;
                    float swayAmplitude = cache.swayAmplitude;
                    float slipSwayAmplitude = cache.slipSwayAmplitude;
                    float activeSwayAmplitude;
                    float swaySequenceOffset = cache.swaySequenceOffset;
                    float swayShift = cache.swayShift;
                    float sway;
                    float zeroBasedStep;
                    float supportStartOffset = cache.supportStartOffset;
                    float singleSupport = cache.singleSupport;
                    float doubleSupport = cache.doubleSupport;
                    float gateLength = cache.gateLength;
                    float rightSlip = cache.rightSlip;
                    float leftSlip = cache.leftSlip;
                    float gatePosition;
                    float transitionPosition;
                    float swingStep;
                    float gateIndex;
                    float legDirection;
                    float isSwing;
                    float signedSwing;
                    float rightSwing;
                    float fAK;
                    float rightSwingStep;
                    float rightSlipStep;
                    float leftSwing;
                    float fAO;
                    float leftSwingStep;
                    float leftSlipStep;
                    float rightGate;
                    float leftGate;
                    float fAT = cache.fAT;
                    float fAU = cache.fAU;
                    float fAV;
                    float fAW;
                    float fAX;
                    float fF25 = 1.0f;
                    float fD25 = 1.0f;
                    float fAY = fF25;
                    float fAZ = fD25;
                    float fBA;
                    float fBB;
                    float fBC;
                    float fF26 = 1.0f;
                    float fD26 = 1.0f;
                    float fBD = fF26;
                    float fBE = fD26;
                    float fBF;
                    float fBG = cache.fBG;
                    float fD23 = -1;
                    float fBH = fD23;
                    float fD24 = -1;
                    float fBJ = fD24;
                    float fD50 = cache.fD50;
                    float fD51 = fD50;
                    float fBR = fD50;
                    float fBS = fD51;
                    float liftHeight = cache.liftHeight;
                    float nominalFootSpacing = cache.nominalFootSpacing;
                    float effectiveFootSpacing = cache.effectiveFootSpacing;
                    float rightBodyTiltOffset = cache.rightBodyTiltOffset;
                    float leftBodyTiltOffset = cache.leftBodyTiltOffset;
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        leftBodyTiltOffset = rightBodyTiltOffset;
                        rightBodyTiltOffset = -leftBodyTiltOffset;
                    }
                    float crouch = cache.crouch;
                    float fCC = rightBodyTiltOffset;
                    float repeatCrouch = cache.repeatCrouch;
                    float phaseCrouch = cache.phaseCrouch;
                    float blendedCrouch;
                    float fCD = leftBodyTiltOffset;
                    float fCE = crouch + fCC;
                    float fCF = crouch + fCD;
                    float stride = cache.stride;
                    float fCH = stride;
                    float fCI = stride;
                    float forwardEllipse = cache.forwardEllipse;
                    float backwardEllipse = cache.backwardEllipse;
                    float fE17 = cache.fE17;
                    float fE18 = cache.fE18;
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fE17 *= -1.0f;
                        fE18 *= -1.0f;
                    }

                    float forwardWeightShift = cache.forwardWeightShift;
                    float fDA = -stride;
                    float fDB = stride;
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fDA = fDB = 0.0f;
                    }
                    float fDK = cache.fDK;
                    float fDL = -fDK;
                    float fDQ = cache.fDQ;
                    float fDR = cache.fDR;
                    float fD13 = 1;
                    float fE13 = 1;
                    float fF13 = 1;
                    float fD14 = 1;
                    float fE14 = 1;
                    float fF14 = 1;
                    float directionDegrees = cache.directionDegrees;
                    float directionRadians = cache.directionRadians;
                    float fDY = fD13;
                    float fDZ = fE13;
                    float fEA = fF13;
                    float fEE = fD14;
                    float fEF = fE14;
                    float fEG = fF14;
                    float fEH = fDL;
                    float fEQ = cache.fEQ;
                    float fER = cache.fER;
                    float fBI;
                    float fBK;
                    float fBL;
                    float fBM = cache.fBM;
                    float fBN = cache.fBN;
                    float fBO;
                    float fBP;
                    float fBQ;
                    float fBT;
                    float fBU;
                    float fBX;
                    float rightLift;
                    float fBZ;
                    float leftLift;
                    float fCJ;
                    float fCK;
                    float fCL;
                    float fCM;
                    float fCP;
                    float fCQ;
                    float fCR;
                    float fCS;
                    float fCT;
                    float fCU;
                    float fCV;
                    float fCW;
                    float fDC;
                    float fDD;
                    float fDE;
                    float fDG;
                    float fDH;
                    float fDI;
                    float fDM = cache.fDM;
                    float fDN = cache.fDN;
                    float fDO;
                    float fDP;
                    float fDS;
                    float fDT;
                    float fDU = cache.fDU;
                    float fDV;
                    float fEB = fDK;
                    float fEC;
                    float fED;
                    float fEI;
                    float fEJ;
                    float rightX;
                    float rightY;
                    float rightZ;
                    float leftX;
                    float leftY;
                    float leftZ;
                    float fES;
                    float fET;
                    float waistAmplitude = cache.waistAmplitude;
                    float waistAngle;
                    float neckAmplitude = cache.neckAmplitude;

                    enabledWeight = (i <= nSize_Frame ? 1.0f : 0.0f);
                    fL = i;
                    stepPosition = i * enabledWeight;
                    // 1. Gate, support flags, and slip-adjusted swing phase.
                    zeroBasedStep = stepPosition - 1.0f;
                    gatePosition = ((float)((int)zeroBasedStep % (int)gateLength) + 1.0f) * enabledWeight;
                    isSwing = (((gatePosition - (doubleSupport + supportStartOffset)) <= singleSupport && gatePosition > (doubleSupport + supportStartOffset)) ? 1 : 0);
                    swingStep = (gatePosition - (doubleSupport + supportStartOffset)) * isSwing;
                    gateIndex = (float)Math.Round((zeroBasedStep + 0.001f) / gateLength - 0.5f, 0);
                    legDirection = (float)Math.Pow(-1.0f, gateIndex) * enabledWeight;
                    signedSwing = isSwing * legDirection;
                    rightSwing = (signedSwing > 0 ? 1 : 0);
                    leftSwing = (signedSwing < 0 ? 1 : 0);
                    leftSwingStep = (leftSwing == 0 ? 0 : swingStep);
                    rightSwingStep = (rightSwing == 0 ? 0 : swingStep);
                    leftGate = (legDirection < 0 ? 1.0f : 0.0f);
                    rightGate = (legDirection > 0 ? 1.0f : 0.0f);
                    fAK = (rightGate == 0 ? 0 : gatePosition);
                    fAO = (leftGate == 0 ? 0 : gatePosition);
                    swayPosition = (swayMode == 1 ? (stepPosition >= gateLength ? gateLength : fAK - rightSwingStep) + (stepPosition >= gateLength * 2.0f ? gateLength : fAO - leftSwingStep) : fAK - rightSwingStep + fAO - leftSwingStep + (rightSwingStep + leftSwingStep > 0 ? 1.0f : 0.0f) + enabledWeight * gateIndex * gateLength);
                    activeSwayAmplitude = (rightSlip + leftSlip != 0 ? ((stepPosition <= gateLength && leftSlip != 0) ? swayAmplitude : slipSwayAmplitude) * rightGate + ((stepPosition > gateLength && rightSlip != 0) ? swayAmplitude : slipSwayAmplitude) * leftGate : swayAmplitude * enabledWeight);
                    sway = (float)(activeSwayAmplitude * (float)Math.Sin((((swayMode == 0 ? stepPosition : swayPosition) - swaySequenceOffset) / (gateLength) * 180.0f) / 180.0f * Math.PI)) + swayShift;
                    transitionPosition = gateIndex * gateLength + legDirection * gatePosition + gateIndex;
                    rightSlipStep = (float)Math.Round((rightSwingStep > rightSlip ? (rightSwingStep - rightSlip) / (singleSupport - rightSlip) * singleSupport : 0), 3);
                    leftSlipStep = (float)Math.Round((leftSwingStep > leftSlip ? (leftSwingStep - leftSlip) / (singleSupport - leftSlip) * singleSupport : 0.0f), 3);
                    // 2. Ankle/hip waves and swing lift.
                    fAV = (rightSlip + leftSlip != 0 ? ((stepPosition <= gateLength && leftSlip != 0) ? fAT : fAU) * rightGate + ((stepPosition > gateLength && rightSlip != 0) ? fAT : fAU) * leftGate : fAT * enabledWeight);
                    fAW = fAV * (float)Math.Sin(((swayMode == 0 ? fAK : swayPosition) / gateLength * 180.0f) / 180.0f * Math.PI);
                    fAX = fAV * (float)Math.Sin((rightSwingStep / singleSupport * 180.0f) / 180.0f * Math.PI);
                    fBA = (fAY == 1 ? fAW : fAX) * fAZ;
                    fBB = fAV * (float)Math.Sin((fAO / gateLength * 180.0f) / 180.0f * Math.PI);
                    fBC = fAV * (float)Math.Sin((leftSwingStep / singleSupport * 180.0f) / 180.0f * Math.PI);
                    fBF = (fBD == 1 ? fBB : fBC) * fBE;
                    float swingSinR = (float)Math.Sin((rightSlipStep / singleSupport * 180.0f) / 180.0f * (float)Math.PI);
                    float swingSinL = (float)Math.Sin((leftSlipStep / singleSupport * 180.0f) / 180.0f * (float)Math.PI);
                    fBI = fBG * swingSinR * fBH;
                    fBK = fBG * swingSinL * fBJ;
                    fBL = (transitionPosition - (doubleSupport + supportStartOffset)) * enabledWeight;
                    fBO = (rightGate + rightSwing + leftGate + leftSwing - 1.0f) * legDirection;
                    fBP = (fBL < 0 ? 0 : (fBL > singleSupport ? singleSupport : fBL));
                    fBQ = (fBO < 0 ? fBP + 1.0f : fBP);
                    fBT = (fBR * fBQ / singleSupport);
                    fBU = (fBS * fBQ / singleSupport);
                    fBX = rightSwing * liftHeight * enabledWeight;
                    rightLift = (float)Math.Round(fBX * swingSinR, 3);
                    fBZ = leftSwing * liftHeight * enabledWeight;
                    leftLift = (float)Math.Round(fBZ * swingSinL, 3);
                    // 3. Forward progression and fore/aft ellipse blend.
                    fCJ = (stepPosition > (gateLength + supportStartOffset) ? stride * 2.0f : stride / singleSupport * rightSwingStep * 2.0f);
                    fCK = (stepPosition > (gateLength + supportStartOffset + doubleSupport + singleSupport) ? stride * 2.0f : stride / singleSupport * leftSwingStep * 2.0f);
                    fCL = fCH * (stepPosition / (gateLength * 2.0f) * 2.0f);
                    fCM = fCI * (stepPosition / (gateLength * 2.0f) * 2.0f);
                    fCP = forwardEllipse * swingSinR;
                    fCQ = backwardEllipse * swingSinR;
                    fCR = rightSlipStep / gateLength * 2.0f;
                    fCS = (fCR * fCP - (2.0f - fCR) * fCQ);
                    fCT = forwardEllipse * swingSinL;
                    fCU = backwardEllipse * swingSinL;
                    fCV = leftSlipStep / gateLength * 2.0f;
                    fCW = (fCV * fCT - (2.0f - fCV) * fCU);
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        forwardWeightShift = cache.Values[15];
                        float fCZ_1 = cache.RepeatValues[15];
                        forwardWeightShift = (forwardWeightShift + (fCZ_1 - forwardWeightShift) / gateLength * transitionPosition) * enabledWeight;
                    }
                    fDC = (fCJ - fCL) + fCS;
                    fDD = fDC * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDA;
                    fDE = fDC;
                    fDG = (fCK - fCM) + fCW;
                    fDH = fDG * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDB;
                    fDI = fDG;
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fDN = (cache.Values[8] >= 0) ? cache.Values[30] : -cache.Values[30];
                        float fDN_1 = (cache.RepeatValues[8] >= 0) ? cache.RepeatValues[30] : -cache.RepeatValues[30];
                        fDN = (fDN + (fDN_1 - fDN) / gateLength * transitionPosition) * enabledWeight;
                    }
                    fDO = fDN * (float)Math.Sin((stepPosition / (gateLength * 2.0f) * 180.0f) / 180.0f * (float)Math.PI) - fDN / 2.0f;
                    fDP = -fDO;
                    fDS = (float)Math.Abs(fDR * (float)Math.Sin((stepPosition / (gateLength) * 180.0f) / 180.0f * (float)Math.PI)) + fDQ;
                    fDT = fDS;
                    fDV = -(float)Math.Abs(fDU * (float)Math.Sin((stepPosition / (gateLength) * 180.0f) / 180.0f * (float)Math.PI)) * (fDU >= 0 ? 1.0f : -1.0f);
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        blendedCrouch = (phaseCrouch + (repeatCrouch - phaseCrouch) / gateLength * transitionPosition) * enabledWeight;
                        fCE = blendedCrouch + fCC;
                        fCF = blendedCrouch + fCD;
                    }
                    fEC = rightLift + fCE + fDV;
                    fED = (float)Math.Round(fDD, 3);
                    fEI = leftLift + fCF + fDV;
                    fEJ = (float)Math.Round(fDH, 3);
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        float fDK_0 = -cache.RepeatValues[17];
                        float fDK_1 = -cache.Values[17];
                        fDK = (fDK_1 + (fDK_0 - fDK_1) / singleSupport * fBQ) * enabledWeight;
                        fDL = -fDK;
                        fEB = fDK;
                        fEH = fDL;
                        float fDM_0 = -cache.RepeatValues[29];
                        float fDM_1 = -cache.Values[29];
                        fDM = (fDM_1 + (fDM_0 - fDM_1) / gateLength * transitionPosition) * enabledWeight;
                        float fDQ_0 = cache.RepeatValues[33];
                        float fDQ_1 = cache.Values[33];
                        fDQ = (fDQ_1 + (fDQ_0 - fDQ_1) / gateLength * transitionPosition) * enabledWeight;
                        float fEQ_0 = cache.RepeatValues[45];
                        float fEQ_1 = cache.Values[45];
                        fEQ = (fEQ_1 + (fEQ_0 - fEQ_1) / gateLength * transitionPosition) * enabledWeight;
                    }
                    // 4. Direction transform into the original engine coordinate convention.
                    rightX = sway + (fEB + cache.SinDirection * fED) * fDY;
                    rightY = fEC * fDZ;
                    rightZ = (cache.CosDirection * fED) * fEA - forwardWeightShift;
                    leftX = sway + (fEH + cache.SinDirection * fEJ) * fEE;
                    leftY = fEI * fEF;
                    leftZ = (cache.CosDirection * fEJ) * fEG - forwardWeightShift;
                    fES = (rightSwingStep > 0 ? fER * (float)Math.Sin((rightSwingStep / singleSupport * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    fET = (leftSwingStep > 0 ? fER * (float)Math.Sin((leftSwingStep / singleSupport * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    if (nStart_0_Repeat_1_End_2 == 1)
                        waistAngle = waistAmplitude * (float)Math.Sin(((stepPosition / gateLength * 180.0f) + 90.0f) / 180.0f * Math.PI);
                    else
                        waistAngle = waistAmplitude * (float)Math.Sin((-stepPosition / (gateLength * 2.0f) * 180.0f) / 180.0f * Math.PI);
                target.Mode = nMode; target.Step = nStep; target.FrameCount = nSize_Frame;
                target.Right = Foot(cache, 9, rightX, rightY, rightZ);
                target.Left = Foot(cache, 10, leftX, leftY, leftZ);
                int timeIndex = nMode == 0 && nStep == 1 ? 0 : 24;
                target.Time = (int)cache.Values[timeIndex]; target.Delay = (int)cache.Values[25];
                target.TimeText = cache.Names[timeIndex]; target.DelayText = cache.Names[25];
                target.SwingLeg = rightSwing > 0 ? 0 : leftSwing > 0 ? 1 : -1;
                target.SwingProgress = target.SwingLeg == 0 ? rightSlipStep / singleSupport : target.SwingLeg == 1 ? leftSlipStep / singleSupport : 0;
                // Preserve command order: FK/IK first, then additive/absolute joints, mirrors, extras.
                Joint(target, cache, 22, true, fBA + fE17); Joint(target, cache, 21, true, fBF + fE18);
                Joint(target, cache, 19, true, fBI); Joint(target, cache, 20, true, fBK);
                Joint(target, cache, 27, false, fDO + fDM); Joint(target, cache, 28, false, fDP + fDM);
                Joint(target, cache, 31, false, fDS + fDQ); Joint(target, cache, 32, false, fDT + fDQ);
                Joint(target, cache, 42, true, fES); Joint(target, cache, 43, true, fET);
                float tilt = 0;
                // Historical left-ID guard is retained for output compatibility.
                if (cache.Ids[40] >= 0)
                    tilt = nMode == 1 ? cache.Values[41] : (cache.Values[41] + (cache.RepeatValues[41] - cache.Values[41]) / gateLength * transitionPosition) * enabledWeight;
                target.Tilt = tilt;
                Joint(target, cache, 39, true, tilt); Joint(target, cache, 40, true, tilt);
                Joint(target, cache, 47, true, fBT); Joint(target, cache, 48, true, fBU);
                Joint(target, cache, 50, true, waistAngle); Joint(target, cache, 52, true, neckAmplitude);
                target.MirrorBeforeGroup = nMode != 1; target.MirrorAfterGroup = mirror;
                target.RepeatCount = nMode == 1 && nStep == 1 ? walkingCount : 1;
                target.ExtraCommand = cache.Names[49]; target.TrailingCommand = cache.Names[54];
                target.Valid = true;
            }

            private static FootTarget Foot(WalkingParamCache p, int index, float x, float y, float z)
            {
                if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y) || float.IsNaN(z) || float.IsInfinity(z))
                    throw new ArithmeticException("Nonfinite foot target.");
                return new FootTarget { FunctionId = p.Ids[index], IdText = p.Names[index], X = x, Y = y, Z = z };
            }
            private static void Joint(Frame frame, WalkingParamCache p, int index, bool add, float value)
            {
                if (p.Ids[index] < 0) return;
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArithmeticException("Nonfinite joint command.");
                frame.Joints[frame.JointCount++] = new JointCommand { MotorId = p.Ids[index], IdText = p.Names[index], Additive = add, Value = value };
            }

            public string MakeWalkingMotion(int mode, int step)
            {
                EvaluateFrame(mode, step, stringBuffer); return stringBuffer.ToCommandString();
            }
            public List<Frame> GenerateFrames(int count = 1)
            {
                Prepare(); allTilt.Clear(); foreach (List<float> list in tiltHistory) list.Clear();
                int n = prepared[1].Frames;
                List<Frame> result = new List<Frame>(count > 0 ? 3 * n : 2 * n);
                for (int mode = 0; mode < 3; mode++)
                {
                    if (mode == 1 && count <= 0) continue;
                    for (int step = 1; step <= n; step++)
                    {
                        Frame f = new Frame(); EvaluateFrame(mode, step, f); result.Add(f);
                        if (mode == 1 || (mode == 0 && step <= prepared[mode].gateLength) || (mode == 2 && step > prepared[mode].gateLength))
                        { allTilt.Add(f.Tilt); tiltHistory[mode].Add(f.Tilt); }
                    }
                }
                return result;
            }
            public List<string> GenerateWalking(int count = 1)
            {
                List<Frame> frames = GenerateFrames(count); List<string> text = new List<string>(frames.Count);
                foreach (Frame frame in frames) text.Add(frame.ToCommandString()); return text;
            }
            public float GetTilt(int mode, int step) { CheckMode(mode); return tiltHistory[mode][step]; }
            public float GetTilt(int step) { return allTilt[step]; }
            public int RecordedTiltCount { get { return allTilt.Count; } }
            public void GetString_Init() { streamMode = streamStep = 0; }
            public int GetStatus() { return streamMode; }
            public string GetString_Walk()
            {
                if (prepared[streamMode] == null) Prepare();
                int n = prepared[streamMode].Frames, gap = streamMode == 1 ? 0 : n / 2;
                streamStep++;
                string s = MakeWalkingMotion(streamMode, streamMode == 2 ? gap + streamStep : streamStep);
                if (streamStep >= n - gap) { streamStep = 0; streamMode = (streamMode + 1) % 3; }
                return s;
            }

            public bool ParamSave() { return SaveParameters(System.IO.Path.Combine(Application.StartupPath, "WalkingParam.dat")); }
            public bool ParamLoad() { return LoadParameters(System.IO.Path.Combine(Application.StartupPath, "WalkingParam.dat")); }
            public bool SaveParameters(string path)
            {
                Ojw.CFile file = new Ojw.CFile(); file.Clear();
                for (int i = 0; i < 100; i++) file.Add(string.Format("{0}:{1}:{2}", raw[0, i], raw[1, i], raw[2, i]));
                return file.Save(path);
            }
            public bool LoadParameters(string path)
            {
                Ojw.CFile file = new Ojw.CFile(); if (file.Load(path) <= 0) return false;
                for (int i = 0; i < Math.Min(100, file.Get_Count()); i++)
                {
                    string[] values = file.GetData(i).Split(':');
                    if (values.Length == 3) for (int mode = 0; mode < 3; mode++) SetData(mode, i, values[mode]);
                }
                return true;
            }

                public void Grid_Init_Param(Control ctrlHandle,
                    Color cLineColor,
                    String strHeader_Param, int nWidth_Param, Color cBack_Param, Color cFont_Param,
                    String strHeader_Start, int nWidth_Start, Color cBack_Start, Color cFont_Start,
                    String strHeader_Repeat, int nWidth_Repeat, Color cBack_Repeat, Color cFont_Repeat,
                    String strHeader_End, int nWidth_End, Color cBack_End, Color cFont_End
                    )
                {
                    string[] pstrTitle = new string[] { strHeader_Param, strHeader_Start, strHeader_Repeat, strHeader_End };
                    int[] pnWidth = new int[] { nWidth_Param, nWidth_Start, nWidth_Repeat, nWidth_End };
                    int[] pnType = new int[] { 0, 0, 0, 0 };
                    m_COjwGrid.Create(ctrlHandle, 0, 0, ctrlHandle.Width, ctrlHandle.Height, pstrTitle, pnWidth, pnType);
                    DataGridView OjwGrid = m_COjwGrid.GetHandle();

                    OjwGrid.RowHeadersDefaultCellStyle.ForeColor = cLineColor;


                    Font fnt = new Font(OjwGrid.Font.Name, OjwGrid.Font.Size, FontStyle.Bold);
                    int nPos = 0;
                    //OjwGrid.Font = new Font(OjwGrid.Font.Name, OjwGrid.Columns[nPos].DefaultCellStyle.Font.Size, FontStyle.Bold);
                    OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Param;// Color.Black;
                    OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Param;// Color.FromArgb(255, 251, 243); //Color.Orange;
                    nPos++;

                    //Font fnt = new Font(OjwGrid.Columns[nPos].DefaultCellStyle.Font.Name, OjwGrid.Columns[nPos].DefaultCellStyle.Font.SizeInPoints, System.Drawing.FontStyle.Bold);
                    OjwGrid.Columns[nPos].DefaultCellStyle.Font = fnt;
                    OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Start;// Color.Red;// Color.Yellow;
                    OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Start;//Color.Red;
                    nPos++;

                    OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Repeat;// Color.Blue;// Color.Black;
                    OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Repeat;// Color.White;
                    nPos++;


                    //OjwGrid.Columns[nPos].HeaderCell.Style.ForeColor = Color.Blue;
                    //OjwGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Blue;



                    OjwGrid.Columns[nPos].DefaultCellStyle.Font = fnt;
                    OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_End;// Color.Green;// Color.Violet; //Color.Yellow;
                    OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_End;//Color.Violet;
                    nPos++;

                    m_COjwGrid.Grid_Add(0, m_pstrParams.Length);

                    //m_COjwGrid.GetHandle(). = Color.FromArgb(235, 236, 239);
                    m_COjwGrid.GetHandle().GridColor = Color.FromArgb(235, 236, 239);

                    for (int j = 0; j < m_pstrParams.Length; j++)
                    {
                        m_COjwGrid.Grid_Set(0, j, m_pstrParams[j]);
                        if (j < m_pstrParams_Value_Start.Length) m_COjwGrid.Grid_Set(1, j, m_pstrParams_Value_Start[j]);
                        if (j < m_pstrParams_Value_Ing.Length) m_COjwGrid.Grid_Set(2, j, m_pstrParams_Value_Ing[j]);
                        if (j < m_pstrParams_Value_End.Length) m_COjwGrid.Grid_Set(3, j, m_pstrParams_Value_End[j]);
                    }
                }
        }
    }
}
