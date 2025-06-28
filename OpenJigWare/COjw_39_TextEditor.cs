using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace OpenJigWare
{
    partial class Ojw
    {
        
        /////
        #region TextEditor Class

        #region 설명
    #if false
                                                                                                                                                                                                                                                                                                                                                                                                                                                        ////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////    
    // 1. 폰트 크기만 변경
    CTxt.SetFontSize(14f);    // 14pt로 변경
    CTxt.SetFontSize(18f);    // 18pt로 변경

    // 2. 폰트 종류만 변경
    CTxt.SetFontFamily("Arial");
    CTxt.SetFontFamily("Courier New");
    CTxt.SetFontFamily("Times New Roman");

    // 3. 폰트 종류와 크기 한번에 변경
    CTxt.SetFont("Arial", 16f);
    CTxt.SetFont("Consolas", 14f);
    CTxt.SetFont("Courier New", 12f);

    // 4. 현재 폰트 정보 가져오기
    float currentSize = CTxt.GetFontSize();
    string currentFamily = CTxt.GetFontFamily();
    Console.WriteLine($"현재 폰트: {currentFamily}, 크기: {currentSize}");

    // 5. Font 객체로 직접 설정 (기존 방법)
    CTxt.Font = new Font("Verdana", 15f, FontStyle.Regular);
    ////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////    
    // 1. 별도 Init 함수
    // >> csharp// 방법 1: 기본 생성자 + Init (새로운 방식)
    ////////////////////////////////////////////////////////////////
    CTextEditor CTxt = new CTextEditor();
    CTxt.Init(richTextEditor);
    
    ////////////////////////////////////////////////////////////////    
    // 방법 2: 생성자에서 바로 초기화 (기존 방식 - 호환성 유지)
    ////////////////////////////////////////////////////////////////    
    CTextEditor CTxt = new CTextEditor(richTextEditor);

    ////////////////////////////////////////////////////////////////    
    // 2. 외부에서 색상 및 효과 지정
    ////////////////////////////////////////////////////////////////    
    // >> 함수 설정
    // >> csharp// 함수들에 색상과 스타일 지정
    ////////////////////////////////////////////////////////////////    
    CTxt.SetColor_Function("sin, cos, tan", Color.Blue, FontStyle.Bold);
    CTxt.SetColor_Function("atan, atan2", Color.Red, FontStyle.Bold | FontStyle.Italic);
    CTxt.SetColor_Function("pow, sqrt, log", Color.Green, FontStyle.Bold);
    
    ////////////////////////////////////////////////////////////////    
    // 함수 제거
    ////////////////////////////////////////////////////////////////    
    CTxt.RemoveColor_Function("sin, cos");

    ////////////////////////////////////////////////////////////////    
    // 단어 설정
    // >> csharp// 특정 단어들에 색상 지정 (정확한 단어 매칭)
    ////////////////////////////////////////////////////////////////    
    CTxt.SetColor_Word("PI, E, true, false", Color.Purple, FontStyle.Bold);
    CTxt.SetColor_Word("const, var, let", Color.DarkBlue, FontStyle.Italic);
    
    
    ////////////////////////////////////////////////////////////////    
    // 변수 패턴 설정
    // >> csharp// 정규식 패턴으로 변수 설정
    ////////////////////////////////////////////////////////////////    
    CTxt.SetColor_Variable(@"\b[tv]\d+\b", Color.DarkViolet, FontStyle.Bold | FontStyle.Italic);
    CTxt.SetColor_Variable(@"\b[xy]\d+\b", Color.Orange, FontStyle.Italic); // x0, y1, x123 등
    CTxt.SetColor_Variable(@"\b[A-Z][A-Z0-9_]*\b", Color.Brown, FontStyle.Bold); // 상수명 패턴
    
    ////////////////////////////////////////////////////////////////    
    // 고급 사용 예제
    ////////////////////////////////////////////////////////////////    
    csharppublic partial class Form1 : Form
    {
        private CTextEditor CTxt;

        public Form1()
        {
            InitializeComponent();
        
            CTxt = new CTextEditor();
            CTxt.Init(richTextEditor);
        
            // 수학 함수들
            CTxt.SetColor_Function("sin, cos, tan, asin, acos, atan, atan2", Color.Blue, FontStyle.Bold);
            CTxt.SetColor_Function("pow, sqrt, log, log10, exp", Color.Green, FontStyle.Bold);
            CTxt.SetColor_Function("abs, floor, ceil, round", Color.DarkBlue, FontStyle.Bold);
        
            // 상수들
            CTxt.SetColor_Word("PI, E, true, false, null", Color.Purple, FontStyle.Bold);
        
            // 연산자들 (정규식으로)
            CTxt.SetColor_Variable(@"[+\-*/=<>!&|]", Color.Red, FontStyle.Bold);
        
            // 숫자들
            CTxt.SetColor_Variable(@"\b\d+\.?\d*\b", Color.DarkGreen, FontStyle.Regular);
        
            // 예제 텍스트
            CTxt.Text = "result = sin(PI * t0) + cos(v1) * pow(2.5, t2) + sqrt(abs(v0))";
        }
    }
    
    ////////////////////////////////////////////////////////////////    
    // 편의 함수들
    // >> csharp// 현재 설정 확인
    ////////////////////////////////////////////////////////////////    
    string[] functions = CTxt.GetFunctions();
    string[] patterns = CTxt.GetVariablePatterns();
    
    ////////////////////////////////////////////////////////////////    
    // 모든 설정 초기화
    ////////////////////////////////////////////////////////////////    
    CTxt.ClearAllHighlights();
    
    ////////////////////////////////////////////////////////////////    
    // 기본 설정으로 복원
    ////////////////////////////////////////////////////////////////    
    CTxt.ResetToDefaults();

    #endif
        #endregion 설명

        public class CTextEditor : IDisposable
        {
            // Windows API for preventing flicker
            [DllImport("user32.dll")]
            private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
            private const int WM_SETREDRAW = 11;

            private RichTextBox richTextBox;
            private Timer highlightTimer;
            private bool isHighlighting = false;
            private bool disposed = false;

            // 색상 설정 (외부에서 변경 가능)
            public Color FunctionColor { get; set; }
            public Color VariableColor { get; set; }
            public Color DefaultColor { get; set; }

            // 타이머 간격 설정 (외부에서 변경 가능)
            public int HighlightDelay { get; set; }

            // 함수 목록들을 저장하는 Dictionary (함수명 -> 색상+스타일)
            private Dictionary<string, Tuple<Color, FontStyle>> functionList;

            // 변수 패턴들을 저장하는 Dictionary (패턴 -> 색상+스타일)
            private Dictionary<string, Tuple<Color, FontStyle>> variablePatterns;

            // 기본 생성자
            public CTextEditor()
            {
                InitializeDefaults();
            }

            // RichTextBox를 받는 생성자 (호환성 유지)
            public CTextEditor(RichTextBox richTextBox)
            {
                if (richTextBox == null)
                    throw new ArgumentNullException("richTextBox");

                InitializeDefaults();
                Init(richTextBox);
            }

            private void InitializeDefaults()
            {
                // 색상 초기값 설정 (VS2010 호환)
                FunctionColor = Color.Blue;
                VariableColor = Color.DarkViolet;
                DefaultColor = Color.Black;
                HighlightDelay = 300;

                // 기본 함수 목록 초기화
                functionList = new Dictionary<string, Tuple<Color, FontStyle>>();

                // 기본 변수 패턴 초기화
                variablePatterns = new Dictionary<string, Tuple<Color, FontStyle>>();

                // 기본값들 설정
                SetDefaultFunctions();
                SetDefaultVariables();

                // 타이머 설정
                SetupTimer();
            }

            private void SetDefaultFunctions()
            {
                // 수학 함수들
                var mathFunctions = new string[] { 
                "sin", "cos", "tan", "asin", "acos", "atan", 
                "sqrt", "pow", "abs", "mod", "asin2", "acos2", "atan2", "round" 
            };
                foreach (var func in mathFunctions)
                {
                    functionList[func] = Tuple.Create(Color.Blue, FontStyle.Bold);
                }

                // 제어 함수들
                var controlFunctions = new string[] { "call", "if" };
                foreach (var func in controlFunctions)
                {
                    functionList[func] = Tuple.Create(Color.DarkBlue, FontStyle.Bold);
                }
            }

            private void SetDefaultVariables()
            {
                // 좌표 변수들 (x, y, z) - 빨간색, 굵게+기울임
                variablePatterns[@"\b[xyz]\b"] =
                    Tuple.Create(Color.Red, FontStyle.Bold | FontStyle.Italic);

                // t0-t256 변수들 - 보라색, 굵게+기울임
                variablePatterns[@"\bt(?:25[0-6]|2[0-4][0-9]|1[0-9][0-9]|[0-9]{1,2})\b"] =
                    Tuple.Create(Color.DarkViolet, FontStyle.Bold | FontStyle.Italic);

                // v0-v256 변수들 - 초록색, 굵게+기울임
                variablePatterns[@"\bv(?:25[0-6]|2[0-4][0-9]|1[0-9][0-9]|[0-9]{1,2})\b"] =
                    Tuple.Create(Color.DarkGreen, FontStyle.Bold | FontStyle.Italic);

                // // 주석 - 초록색, 기울임
                variablePatterns[@"//.*$"] =
                    Tuple.Create(Color.Green, FontStyle.Italic);
            }

            CAutoComplete m_CAutoComplete;// = new CAutoComplete(this);
            // 초기화 함수
            public void Init(RichTextBox richTextBox)
            {
                if (richTextBox == null)
                    throw new ArgumentNullException("richTextBox");

                // 기존 연결 해제
                if (this.richTextBox != null)
                {
                    this.richTextBox.TextChanged -= RichTextBox_TextChanged;
                    this.richTextBox.KeyDown -= RichTextBox_KeyDown;
                }

                this.richTextBox = richTextBox;
                InitializeEditor();

                // 2. 자동완성 기능 추가
                //m_CAutoComplete = new CAutoComplete(this);
            }

            private void InitializeEditor()
            {
                // 기본 폰트 설정
                if (richTextBox.Font.Name != "Consolas")
                {
                    richTextBox.Font = new Font("Consolas", richTextBox.Font.Size);
                }

                // 이벤트 연결
                richTextBox.TextChanged += RichTextBox_TextChanged;
                richTextBox.KeyDown += RichTextBox_KeyDown;

                // 초기 구문 강조 적용
                ApplySyntaxHighlighting();
            }

            private void SetupTimer()
            {
                highlightTimer = new Timer();
                highlightTimer.Interval = HighlightDelay;
                highlightTimer.Tick += HighlightTimer_Tick;
            }

            // 함수 색상 및 스타일 설정
            public void SetColor_Function(string functions, Color color, FontStyle style)
            {
                var funcArray = functions.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var func in funcArray)
                {
                    var trimmedFunc = func.Trim();
                    if (!string.IsNullOrEmpty(trimmedFunc))
                    {
                        functionList[trimmedFunc] = Tuple.Create(color, style);
                    }
                }

                ApplySyntaxHighlighting();
            }

            // 함수 색상 설정 (기본 스타일: Bold)
            public void SetColor_Function(string functions, Color color)
            {
                SetColor_Function(functions, color, FontStyle.Bold);
            }

            // 함수 설정 (기본 색상과 스타일 사용)
            public void SetColor_Function(string functions)
            {
                SetColor_Function(functions, FunctionColor, FontStyle.Bold);
            }

            // 함수 제거
            public void RemoveColor_Function(string functions)
            {
                var funcArray = functions.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var func in funcArray)
                {
                    var trimmedFunc = func.Trim();
                    if (!string.IsNullOrEmpty(trimmedFunc))
                    {
                        functionList.Remove(trimmedFunc);
                    }
                }

                ApplySyntaxHighlighting();
            }

            // 변수 패턴 색상 및 스타일 설정
            public void SetColor_Variable(string pattern, Color color, FontStyle style)
            {
                variablePatterns[pattern] = Tuple.Create(color, style);
                ApplySyntaxHighlighting();
            }

            // 변수 패턴 설정 (기본 스타일)
            public void SetColor_Variable(string pattern, Color color)
            {
                SetColor_Variable(pattern, color, FontStyle.Bold | FontStyle.Italic);
            }

            // 변수 패턴 설정 (기본 색상과 스타일)
            public void SetColor_Variable(string pattern)
            {
                SetColor_Variable(pattern, VariableColor, FontStyle.Bold | FontStyle.Italic);
            }

            // 변수 패턴 제거
            public void RemoveColor_Variable(string pattern)
            {
                variablePatterns.Remove(pattern);
                ApplySyntaxHighlighting();
            }

            // 단어별 개별 색상 설정 (정확한 단어 매칭)
            public void SetColor_Word(string words, Color color, FontStyle style)
            {
                var wordArray = words.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordArray)
                {
                    var trimmedWord = word.Trim();
                    if (!string.IsNullOrEmpty(trimmedWord))
                    {
                        // 단어 경계를 사용한 정확한 매칭 패턴
                        string pattern = @"\b" + Regex.Escape(trimmedWord) + @"\b";
                        variablePatterns[pattern] = Tuple.Create(color, style);
                    }
                }

                ApplySyntaxHighlighting();
            }

            // 단어 설정 (기본 스타일: Regular)
            public void SetColor_Word(string words, Color color)
            {
                SetColor_Word(words, color, FontStyle.Regular);
            }

            // 현재 설정된 함수들 가져오기
            public string[] GetFunctions()
            {
                var keys = new string[functionList.Count];
                functionList.Keys.CopyTo(keys, 0);
                return keys;
            }

            // 현재 설정된 변수 패턴들 가져오기
            public string[] GetVariablePatterns()
            {
                var keys = new string[variablePatterns.Count];
                variablePatterns.Keys.CopyTo(keys, 0);
                return keys;
            }

            // 모든 강조 설정 초기화
            public void ClearAllHighlights()
            {
                functionList.Clear();
                variablePatterns.Clear();
                ApplySyntaxHighlighting();
            }

            // 기본 설정으로 복원
            public void ResetToDefaults()
            {
                functionList.Clear();
                variablePatterns.Clear();
                SetDefaultFunctions();
                SetDefaultVariables();
                ApplySyntaxHighlighting();
                // 자동완성 기능 완전 제거
                if (m_CAutoComplete != null)
                {
                    m_CAutoComplete.Dispose();
                    m_CAutoComplete = null;
                }
            }

            private void RichTextBox_TextChanged(object sender, EventArgs e)
            {
                if (isHighlighting) return;

                // 타이머 재시작 (디바운싱)
                highlightTimer.Stop();
                highlightTimer.Interval = HighlightDelay; // 설정 반영
                highlightTimer.Start();
            }

            private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
            {
                // 특정 키에서 즉시 강조 적용
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter ||
                    e.KeyCode == Keys.Tab || e.KeyCode == Keys.OemSemicolon ||
                    e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Oemcomma)
                {
                    highlightTimer.Stop();
                    ApplySyntaxHighlighting();
                }
            }

            private void HighlightTimer_Tick(object sender, EventArgs e)
            {
                highlightTimer.Stop();
                ApplySyntaxHighlighting();
            }

            public void ApplySyntaxHighlighting()
            {
                if (isHighlighting || richTextBox == null || richTextBox.IsDisposed) return;
                isHighlighting = true;

                try
                {
                    // 화면 갱신 중단
                    SendMessage(richTextBox.Handle, WM_SETREDRAW, false, 0);

                    // 현재 커서 위치와 선택 영역 저장
                    int currentPosition = richTextBox.SelectionStart;
                    int currentLength = richTextBox.SelectionLength;

                    // 전체 텍스트를 기본 색상으로 설정
                    richTextBox.SelectAll();
                    richTextBox.SelectionColor = DefaultColor;
                    richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);

                    string text = richTextBox.Text;

                    // 구문 강조 적용
                    HighlightFunctions(text);
                    HighlightVariables(text);

                    // 커서 위치 복원
                    richTextBox.SelectionStart = Math.Min(currentPosition, richTextBox.Text.Length);
                    richTextBox.SelectionLength = 0;
                }
                finally
                {
                    // 화면 갱신 재개
                    SendMessage(richTextBox.Handle, WM_SETREDRAW, true, 0);
                    richTextBox.Invalidate();
                    isHighlighting = false;
                }
            }

            private void HighlightFunctions(string text)
            {
                foreach (var kvp in functionList)
                {
                    string func = kvp.Key;
                    var colorStyle = kvp.Value;
                    Color color = colorStyle.Item1;
                    FontStyle style = colorStyle.Item2;

                    // 함수명 + 괄호 패턴으로 매칭
                    string pattern = @"\b" + Regex.Escape(func) + @"(?=\s*\()";
                    MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    foreach (Match match in matches)
                    {
                        richTextBox.Select(match.Index, match.Length);
                        richTextBox.SelectionColor = color;
                        richTextBox.SelectionFont = new Font(richTextBox.Font, style);
                    }
                }
            }

            private void HighlightVariables(string text)
            {
                foreach (var kvp in variablePatterns)
                {
                    string pattern = kvp.Key;
                    var colorStyle = kvp.Value;
                    Color color = colorStyle.Item1;
                    FontStyle style = colorStyle.Item2;

                    MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    foreach (Match match in matches)
                    {
                        richTextBox.Select(match.Index, match.Length);
                        richTextBox.SelectionColor = color;
                        richTextBox.SelectionFont = new Font(richTextBox.Font, style);
                    }
                }
            }

            // 텍스트 설정/가져오기
            public string Text
            {
                get
                {
                    return richTextBox != null ? richTextBox.Text : "";
                }
                set
                {
                    if (richTextBox != null)
                    {
                        richTextBox.Text = value;
                        ApplySyntaxHighlighting();
                    }
                }
            }

            // 폰트 설정
            public Font Font
            {
                get
                {
                    return richTextBox != null ? richTextBox.Font : null;
                }
                set
                {
                    if (richTextBox != null)
                    {
                        richTextBox.Font = value;
                        ApplySyntaxHighlighting();
                    }
                }
            }

            // 폰트 크기 설정
            public void SetFontSize(float size)
            {
                if (richTextBox != null && richTextBox.Font != null)
                {
                    richTextBox.Font = new Font(richTextBox.Font.FontFamily, size, richTextBox.Font.Style);
                    ApplySyntaxHighlighting();
                }
            }

            // 폰트 종류 설정
            public void SetFontFamily(string fontFamily)
            {
                if (richTextBox != null && richTextBox.Font != null)
                {
                    try
                    {
                        richTextBox.Font = new Font(fontFamily, richTextBox.Font.Size, richTextBox.Font.Style);
                        ApplySyntaxHighlighting();
                    }
                    catch (ArgumentException)
                    {
                        // 폰트가 없는 경우 기본 폰트 사용
                        richTextBox.Font = new Font("Consolas", richTextBox.Font.Size, richTextBox.Font.Style);
                        ApplySyntaxHighlighting();
                    }
                }
            }

            // 폰트 크기와 종류를 한번에 설정
            public void SetFont(string fontFamily, float size)
            {
                if (richTextBox != null)
                {
                    try
                    {
                        richTextBox.Font = new Font(fontFamily, size);
                        ApplySyntaxHighlighting();
                    }
                    catch (ArgumentException)
                    {
                        // 폰트가 없는 경우 기본 폰트 사용
                        richTextBox.Font = new Font("Consolas", size);
                        ApplySyntaxHighlighting();
                    }
                }
            }

            // 폰트 크기 가져오기
            public float GetFontSize()
            {
                if (richTextBox != null && richTextBox.Font != null)
                    return richTextBox.Font.Size;
                return 12f;
            }

            // 폰트 종류 가져오기
            public string GetFontFamily()
            {
                if (richTextBox != null && richTextBox.Font != null && richTextBox.Font.FontFamily != null)
                    return richTextBox.Font.FontFamily.Name;
                return "Consolas";
            }

            // 수동으로 강조 적용
            public void RefreshHighlighting()
            {
                ApplySyntaxHighlighting();
            }

            // 색상 테마 설정
            public void SetColorTheme(Color functionColor, Color variableColor, Color defaultColor)
            {
                FunctionColor = functionColor;
                VariableColor = variableColor;
                DefaultColor = defaultColor;
                ApplySyntaxHighlighting();
            }

            // 파이썬 모드로 설정
            public void SetPythonMode(bool bEn = true)
            {
                if (bEn)
                {
                    ClearAllHighlights();

                    // 파이썬 키워드들
                    SetColor_Function("and, as, assert, break, class, continue, def, del", Color.Blue, FontStyle.Bold);
                    SetColor_Function("elif, else, except, exec, finally, for, from, global", Color.Blue, FontStyle.Bold);
                    SetColor_Function("if, import, in, is, lambda, not, or, pass, print", Color.Blue, FontStyle.Bold);
                    SetColor_Function("raise, return, try, while, with, yield", Color.Blue, FontStyle.Bold);
                    SetColor_Function("True, False, None", Color.Blue, FontStyle.Bold);

                    // 파이썬 내장 함수들
                    SetColor_Function("abs, all, any, bin, bool, chr, dict, dir, enumerate", Color.DarkCyan, FontStyle.Regular);
                    SetColor_Function("eval, filter, float, format, help, hex, id, input", Color.DarkCyan, FontStyle.Regular);
                    SetColor_Function("int, isinstance, len, list, map, max, min, next", Color.DarkCyan, FontStyle.Regular);
                    SetColor_Function("open, ord, pow, print, range, repr, round, set", Color.DarkCyan, FontStyle.Regular);
                    SetColor_Function("sorted, str, sum, super, tuple, type, vars, zip", Color.DarkCyan, FontStyle.Regular);

                    // 파이썬 패턴들
                    SetColor_Variable(@"""[^""]*""", Color.DarkRed, FontStyle.Regular);  // "문자열"
                    SetColor_Variable(@"'[^']*'", Color.DarkRed, FontStyle.Regular);     // '문자열'
                    SetColor_Variable(@"#.*$", Color.Green, FontStyle.Italic);           // # 주석
                    SetColor_Variable(@"\b\d+\.?\d*\b", Color.DarkMagenta, FontStyle.Regular);  // 숫자
                    SetColor_Variable(@"[+\-*/%=<>!&|^~]", Color.Red, FontStyle.Bold);  // 연산자
                    SetColor_Variable(@"\bself\b", Color.DarkGreen, FontStyle.Italic);   // self
                    SetColor_Variable(@"\bcls\b", Color.DarkGreen, FontStyle.Italic);    // cls
                    SetColor_Variable(@"__[a-zA-Z_][a-zA-Z0-9_]*__", Color.Magenta, FontStyle.Bold); // __init__ 등

                    // 2. 자동완성 기능 추가
                    m_CAutoComplete = new CAutoComplete(this);
                }
                else
                {
                    ResetToDefaults();
                }
            }

            // 수학 모드로 설정 (기본값으로 복원)
            public void SetMathMode()
            {
                ResetToDefaults();
            }

            // C# 모드로 설정
            public void SetCSharpMode()
            {
                ClearAllHighlights();

                // C# 키워드들
                SetColor_Function("abstract, as, base, bool, break, byte, case, catch", Color.Blue, FontStyle.Bold);
                SetColor_Function("char, checked, class, const, continue, decimal, default", Color.Blue, FontStyle.Bold);
                SetColor_Function("delegate, do, double, else, enum, event, explicit", Color.Blue, FontStyle.Bold);
                SetColor_Function("extern, false, finally, fixed, float, for, foreach", Color.Blue, FontStyle.Bold);
                SetColor_Function("goto, if, implicit, in, int, interface, internal", Color.Blue, FontStyle.Bold);
                SetColor_Function("is, lock, long, namespace, new, null, object, operator", Color.Blue, FontStyle.Bold);
                SetColor_Function("out, override, params, private, protected, public", Color.Blue, FontStyle.Bold);
                SetColor_Function("readonly, ref, return, sbyte, sealed, short, sizeof", Color.Blue, FontStyle.Bold);
                SetColor_Function("stackalloc, static, string, struct, switch, this", Color.Blue, FontStyle.Bold);
                SetColor_Function("throw, true, try, typeof, uint, ulong, unchecked", Color.Blue, FontStyle.Bold);
                SetColor_Function("unsafe, ushort, using, virtual, void, volatile, while", Color.Blue, FontStyle.Bold);

                // C# 패턴들
                SetColor_Variable(@"""[^""]*""", Color.DarkRed, FontStyle.Regular);  // "문자열"
                SetColor_Variable(@"'[^']*'", Color.DarkRed, FontStyle.Regular);     // '문자'
                SetColor_Variable(@"//.*$", Color.Green, FontStyle.Italic);          // // 주석
                SetColor_Variable(@"/\*[\s\S]*?\*/", Color.Green, FontStyle.Italic); // /* */ 주석
                SetColor_Variable(@"\b\d+\.?\d*[fFdDmM]?\b", Color.DarkMagenta, FontStyle.Regular); // 숫자
                SetColor_Variable(@"[+\-*/%=<>!&|^~]", Color.Red, FontStyle.Bold);  // 연산자
            }

            // JavaScript 모드로 설정
            public void SetJavaScriptMode()
            {
                ClearAllHighlights();

                // JavaScript 키워드들
                SetColor_Function("break, case, catch, class, const, continue, debugger", Color.Blue, FontStyle.Bold);
                SetColor_Function("default, delete, do, else, enum, export, extends", Color.Blue, FontStyle.Bold);
                SetColor_Function("false, finally, for, function, if, import, in", Color.Blue, FontStyle.Bold);
                SetColor_Function("instanceof, let, new, null, return, super, switch", Color.Blue, FontStyle.Bold);
                SetColor_Function("this, throw, true, try, typeof, undefined, var", Color.Blue, FontStyle.Bold);
                SetColor_Function("void, while, with, yield", Color.Blue, FontStyle.Bold);

                // JavaScript 내장 객체/함수들
                SetColor_Function("console, document, window, Math, Date, Array, Object", Color.DarkCyan, FontStyle.Regular);
                SetColor_Function("parseInt, parseFloat, isNaN, setTimeout, setInterval", Color.DarkCyan, FontStyle.Regular);

                // JavaScript 패턴들
                SetColor_Variable(@"""[^""]*""", Color.DarkRed, FontStyle.Regular);  // "문자열"
                SetColor_Variable(@"'[^']*'", Color.DarkRed, FontStyle.Regular);     // '문자열'
                SetColor_Variable(@"`[^`]*`", Color.DarkRed, FontStyle.Regular);     // `템플릿 문자열`
                SetColor_Variable(@"//.*$", Color.Green, FontStyle.Italic);          // // 주석
                SetColor_Variable(@"/\*[\s\S]*?\*/", Color.Green, FontStyle.Italic); // /* */ 주석
                SetColor_Variable(@"\b\d+\.?\d*\b", Color.DarkMagenta, FontStyle.Regular); // 숫자
                SetColor_Variable(@"[+\-*/%=<>!&|^~]", Color.Red, FontStyle.Bold);  // 연산자
            }

            // IDisposable 구현
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        // 이벤트 연결 해제
                        if (richTextBox != null && !richTextBox.IsDisposed)
                        {
                            richTextBox.TextChanged -= RichTextBox_TextChanged;
                            richTextBox.KeyDown -= RichTextBox_KeyDown;
                        }

                        // 타이머 해제
                        if (highlightTimer != null)
                        {
                            highlightTimer.Dispose();
                        }
                    }
                    disposed = true;
                }
            }

            // 소멸자
            ~CTextEditor()
            {
                Dispose(false);
            }
        }

        public class CAutoComplete : IDisposable
        {
            private CTextEditor textEditor;
            private ListBox autoCompleteList;
            private bool isShowing = false;
            private string currentWord = "";
            private int wordStartPosition = 0;

            // 파이썬 내장 객체들과 메서드들
            private Dictionary<string, string[]> pythonMembers;

            public CAutoComplete(CTextEditor editor)
            {
                textEditor = editor;
                InitializeAutoComplete();
                SetupPythonMembers();
            }

            private void InitializeAutoComplete()
            {
                // 자동완성 리스트박스 생성
                autoCompleteList = new ListBox();
                autoCompleteList.Visible = false;
                autoCompleteList.Font = new Font("Consolas", 9f);
                autoCompleteList.BackColor = Color.White;
                autoCompleteList.BorderStyle = BorderStyle.FixedSingle;
                autoCompleteList.Height = 120;
                autoCompleteList.Width = 200;

                // 이벤트 연결
                autoCompleteList.DoubleClick += AutoCompleteList_DoubleClick;
                autoCompleteList.KeyDown += AutoCompleteList_KeyDown;

                // 텍스트 에디터에 추가
                if (textEditor != null)
                {
                    var richTextBox = GetRichTextBox();
                    if (richTextBox != null)
                    {
                        richTextBox.Parent.Controls.Add(autoCompleteList);
                        autoCompleteList.BringToFront();

                        // 이벤트 연결
                        richTextBox.KeyDown += RichTextBox_KeyDown;
                        richTextBox.KeyPress += RichTextBox_KeyPress;
                        richTextBox.LostFocus += RichTextBox_LostFocus;
                    }
                }
            }

            private void SetupPythonMembers()
            {
                pythonMembers = new Dictionary<string, string[]>();

                // 문자열 메서드들
                pythonMembers["str"] = new string[] {
                "capitalize()", "casefold()", "center(width)", "count(sub)",
                "encode()", "endswith(suffix)", "find(sub)", "format(*args)",
                "index(sub)", "isalnum()", "isalpha()", "isdigit()", "islower()",
                "isnumeric()", "isspace()", "istitle()", "isupper()", "join(iterable)",
                "lower()", "lstrip()", "replace(old, new)", "rfind(sub)", "rindex(sub)",
                "rstrip()", "split(sep)", "splitlines()", "startswith(prefix)",
                "strip()", "swapcase()", "title()", "upper()", "zfill(width)"
            };

                // 리스트 메서드들
                pythonMembers["list"] = new string[] {
                "append(item)", "clear()", "copy()", "count(item)", "extend(iterable)",
                "index(item)", "insert(index, item)", "pop(index)", "remove(item)",
                "reverse()", "sort(key, reverse)"
            };

                // 딕셔너리 메서드들
                pythonMembers["dict"] = new string[] {
                "clear()", "copy()", "get(key, default)", "items()", "keys()",
                "pop(key, default)", "popitem()", "setdefault(key, default)",
                "update(other)", "values()"
            };

                // 파일 객체 메서드들
                pythonMembers["file"] = new string[] {
                "close()", "read(size)", "readline()", "readlines()", "seek(offset)",
                "tell()", "write(string)", "writelines(lines)", "flush()"
            };

                // 일반적인 변수 타입별 메서드
                pythonMembers["default"] = new string[] {
                "append()", "clear()", "copy()", "count()", "extend()", "index()",
                "insert()", "pop()", "remove()", "reverse()", "sort()",
                "upper()", "lower()", "strip()", "split()", "replace()",
                "find()", "startswith()", "endswith()"
            };
            }

            private RichTextBox GetRichTextBox()
            {
                // CTextEditor에서 내부 RichTextBox를 가져오는 방법
                // 실제로는 CTextEditor에 public 프로퍼티를 추가하는 것이 좋습니다
                var fields = textEditor.GetType().GetFields(
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(RichTextBox))
                    {
                        return (RichTextBox)field.GetValue(textEditor);
                    }
                }
                return null;
            }

            private void RichTextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == '.')
                {
                    // 점(.)을 입력했을 때 자동완성 시작
                    ShowAutoComplete();
                }
                else if (isShowing)
                {
                    if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '_')
                    {
                        // 문자 입력 시 필터링
                        FilterAutoComplete();
                    }
                    else if (e.KeyChar == '\b') // Backspace
                    {
                        // 백스페이스 시 필터링 갱신
                        HideAutoComplete();
                    }
                    else
                    {
                        // 다른 문자 입력 시 자동완성 숨김
                        HideAutoComplete();
                    }
                }
            }

            private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (isShowing)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Escape:
                            HideAutoComplete();
                            e.Handled = true;
                            break;

                        case Keys.Enter:
                        case Keys.Tab:
                            InsertSelectedItem();
                            e.Handled = true;
                            break;

                        case Keys.Up:
                            if (autoCompleteList.SelectedIndex > 0)
                                autoCompleteList.SelectedIndex--;
                            e.Handled = true;
                            break;

                        case Keys.Down:
                            if (autoCompleteList.SelectedIndex < autoCompleteList.Items.Count - 1)
                                autoCompleteList.SelectedIndex++;
                            e.Handled = true;
                            break;
                    }
                }
            }

            private void RichTextBox_LostFocus(object sender, EventArgs e)
            {
                // 포커스를 잃으면 자동완성 숨김 (약간의 지연을 두어 클릭 가능하게)
                Timer timer = new Timer();
                timer.Interval = 100;
                timer.Tick += (s, args) =>
                {
                    if (!autoCompleteList.Focused)
                        HideAutoComplete();
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }

            private void ShowAutoComplete()
            {
                var richTextBox = GetRichTextBox();
                if (richTextBox == null) return;

                // 현재 위치에서 변수명 추출
                string variableName = GetVariableNameBeforeDot(richTextBox);

                // 해당 변수의 타입에 맞는 메서드 목록 가져오기
                string[] members = GetMembersForVariable(variableName);

                if (members.Length > 0)
                {
                    // 리스트박스에 항목 추가
                    autoCompleteList.Items.Clear();
                    autoCompleteList.Items.AddRange(members);

                    // 위치 계산
                    Point caretPos = GetCaretPosition(richTextBox);
                    autoCompleteList.Location = new Point(caretPos.X, caretPos.Y + 20);

                    // 표시
                    autoCompleteList.Visible = true;
                    autoCompleteList.SelectedIndex = 0;
                    isShowing = true;

                    wordStartPosition = richTextBox.SelectionStart;
                    currentWord = "";
                }
            }

            private string GetVariableNameBeforeDot(RichTextBox richTextBox)
            {
                int dotPosition = richTextBox.SelectionStart - 1;
                int startPos = dotPosition - 1;

                // 변수명의 시작점 찾기 (문자, 숫자, _ 만 허용)
                while (startPos >= 0)
                {
                    char c = richTextBox.Text[startPos];
                    if (!char.IsLetterOrDigit(c) && c != '_')
                        break;
                    startPos--;
                }

                startPos++; // 실제 변수명 시작 위치

                if (startPos < dotPosition)
                {
                    return richTextBox.Text.Substring(startPos, dotPosition - startPos);
                }

                return "";
            }

            private string[] GetMembersForVariable(string variableName)
            {
                // 간단한 타입 추론 (실제로는 더 복잡한 분석이 필요)
                if (string.IsNullOrEmpty(variableName))
                    return pythonMembers["default"];

                // 변수명 기반 타입 추론
                if (variableName.ToLower().Contains("str") || variableName.ToLower().Contains("text") ||
                    variableName.ToLower().Contains("name"))
                    return pythonMembers["str"];

                if (variableName.ToLower().Contains("list") || variableName.ToLower().Contains("arr"))
                    return pythonMembers["list"];

                if (variableName.ToLower().Contains("dict") || variableName.ToLower().Contains("map"))
                    return pythonMembers["dict"];

                if (variableName.ToLower().Contains("file") || variableName.ToLower().Contains("f"))
                    return pythonMembers["file"];

                return pythonMembers["default"];
            }

            private Point GetCaretPosition(RichTextBox richTextBox)
            {
                Point pt = richTextBox.GetPositionFromCharIndex(richTextBox.SelectionStart);
                pt.X += richTextBox.Location.X;
                pt.Y += richTextBox.Location.Y;
                return pt;
            }

            private void FilterAutoComplete()
            {
                var richTextBox = GetRichTextBox();
                if (richTextBox == null) return;

                // 현재 입력중인 단어 가져오기
                currentWord = GetCurrentWord(richTextBox);

                // 필터링
                autoCompleteList.Items.Clear();
                string[] allMembers = GetMembersForVariable("");

                foreach (string member in allMembers)
                {
                    if (member.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                    {
                        autoCompleteList.Items.Add(member);
                    }
                }

                if (autoCompleteList.Items.Count > 0)
                {
                    autoCompleteList.SelectedIndex = 0;
                }
                else
                {
                    HideAutoComplete();
                }
            }

            private string GetCurrentWord(RichTextBox richTextBox)
            {
                int currentPos = richTextBox.SelectionStart;
                return richTextBox.Text.Substring(wordStartPosition, currentPos - wordStartPosition);
            }

            private void InsertSelectedItem()
            {
                if (autoCompleteList.SelectedItem != null)
                {
                    var richTextBox = GetRichTextBox();
                    if (richTextBox == null) return;

                    string selectedText = autoCompleteList.SelectedItem.ToString();

                    // 현재 입력된 부분 제거
                    int currentPos = richTextBox.SelectionStart;
                    if (currentWord.Length > 0)
                    {
                        richTextBox.Select(currentPos - currentWord.Length, currentWord.Length);
                        richTextBox.SelectedText = "";
                    }

                    // 선택된 항목 삽입
                    richTextBox.SelectedText = selectedText;

                    HideAutoComplete();
                }
            }

            private void AutoCompleteList_DoubleClick(object sender, EventArgs e)
            {
                InsertSelectedItem();
            }

            private void AutoCompleteList_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    InsertSelectedItem();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    HideAutoComplete();
                }
            }

            private void HideAutoComplete()
            {
                autoCompleteList.Visible = false;
                isShowing = false;
                currentWord = "";
            }

            public void Dispose()
            {
                if (autoCompleteList != null)
                {
                    autoCompleteList.Dispose();
                }
            }
        }
        #endregion TextEditor Class
    ///
    }
}
