// ================================================================
// C3d — 디자이너 파일(.ojw) 텍스트 형식 · 확장 구역  2026-10-15
//   STL 에 바이너리·텍스트 두 가지가 있듯이 .ojw 도 두 가지다. 내용은 같다.
//   · 바이너리 = 예전 그대로 ('OJW01.09.00' / 'DHF1.x' 로 시작). FileSave(파일, 헤더) 의 기본 — 다른 앱·옛 DLL 이 읽는다.
//   · 텍스트 = UTF-8 XML (<ojw format="text">). 버전이 없다 — 더할 것은 새 요소·속성으로 더하고, 읽는 쪽은
//     모르는 요소·속성을 무시한다 (다시 쓸 때는 그대로 남긴다). 빠진 요소·속성 = 기본값.
//     lib= 는 쓴 라이브러리 버전 (참고용 — 읽기는 보지 않는다).
//   읽기는 확장자가 아니라 내용으로 가른다 (OjwFile_IsText — BOM·공백 뒤 첫 글자가 '<' 면 텍스트).
//     FileOpen / FileOpen_Without_Event 가 맨 앞에서 갈라 텍스트면 OjwText_Read 로 헤더를 채우고, 그 뒤 처리(3D·모션·이벤트)는 바이너리와 같다.
//     텍스트로 읽은 헤더는 같은 내용의 바이너리를 읽은 헤더와 칸마다 같다 — 고정 폭 문자열(제목·모델 이름 21, 모터 이름 32)은 바이너리처럼
//     널로 채우고, 설명은 "", 수식 바이트(암호 칸)는 바이너리 읽기와 같은 바이트, 부수 효과(m_lstMotorsEn, m_CRobotis 모델)도 같다.
//     다른 점: strVersion 은 "OJW" + 지금 버전 (텍스트에는 버전이 없다 — 바이너리의 최신 칸을 모두 담는다), eFileFormat = Text.
//   쓰기는 FileSave(파일, 헤더, EOjwFileFormat.Text) 또는 FileSave_Text. 값의 출처는 바이너리 FileSave 와 같다
//     (배경색·시점·위치·배율은 지금 화면 값, 모터·슬롯·그룹·Json·m_lstMotorsEn 은 m_CHeader·이 컨트롤, 나머지는 넘긴 헤더).
//     파일 전체를 메모리에서 만든 뒤 한 번에 쓴다 (만들다 실패하면 있던 파일은 그대로). 실수는 다시 읽어 비트까지 같은 글 (안 되면 0x 비트).
//   확장 구역 — 헤더의 Ext (COjwFileExt): 이름 붙은 글. 텍스트는 <ext><section name="…">, 바이너리는 끝 꼬리 블록
//     ('OJWX' + 길이 4바이트 + 같은 꼴의 XML + 'XE'). 바이너리 읽기는 Json 칸 뒤를 보지 않으므로 옛 읽기(이 DLL 의 두 읽기 함수·바꾸기 전 DLL)는
//     꼬리를 무시한다 (2026-10-15 확인). 확장 구역이 없으면 꼬리도 없다 — 바이트가 예전과 같다.
//     꼬리에는 구역과 모르는 최상위 요소만 실린다 (모르는 속성은 텍스트 형식으로 저장할 때만 남는다).
//     MakeUrdf 모터 세팅 .ojw 의 덧붙임 4가지: armMotors (방향 확인·출처), robotDef (sagarm.txt), eyeRecord (수직 눈 확인 기록), sagComp (적용 중 처짐 보정 모델).
//   형식 보기
//     <?xml version="1.0" encoding="utf-8"?>
//     <ojw format="text" lib="2.0.8.0">
//       <header nDefaultFunctionNumber="-1" nModelNum="0" strTitle="" strModelName="" cBackColor="#FFA9A9A9" nMotorCnt="6" … />
//       <motors>
//         <motor axis="11" nMotorID="11" nMotorDir="0" fLimit_Up="0" fLimit_Down="0" nCenter_Evd="2048" … />   기본값과 같은 축은 쓰지 않는다
//       </motors>
//       <slots>
//         <slot index="0" pnSecret="0" pnType="0" pbPython="0" pstrGroupName=""><kinematics nl="crlf"><![CDATA[…]]></kinematics></slot>
//       </slots>
//       <drawModel nl="crlf"><![CDATA[…]]></drawModel>
//       <motorsEnabled>…</motorsEnabled>
//       <groups><group nNumber="1" strName="…" /></groups>
//       <json nQr="0" … />
//       <ext><section name="armMotors" nl="crlf"><![CDATA[…]]></section></ext>
//     </ojw>
//   글 칸: 그대로 쓸 수 없는 글(줄바꿈·제어 문자·XML 에 못 쓰는 글자)은 속성 이름 뒤 "_b64" 로 (UTF-16 base64 — 그대로 되살린다).
//     긴 글은 CDATA — nl="crlf" 면 줄 끝이 CRLF (XML 은 줄 끝을 LF 로 읽으므로 되돌린다), enc="b64" 면 base64, enc="secret" 면 암호 칸 바이트(base64).
// ================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class C3d
        {
            // ────────────────────────────────────────────────────────────
            // 형식 · 확장 구역
            // ────────────────────────────────────────────────────────────

            /// <summary>디자이너 파일(.ojw) 형식 — Binary = 예전 바이너리 (FileSave 의 기본 — 다른 앱·옛 DLL 이 읽는다), Text = UTF-8 XML (버전 없이 늘린다)</summary>
            public enum EOjwFileFormat { Binary = 0, Text = 1 }

            /// <summary>확장 구역 — MakeUrdf 모터 세팅의 방향 확인·출처 (CArmMotorSet_t)</summary>
            public const string OJWEXT_ARMMOTORS = "armMotors";
            /// <summary>확장 구역 — 로봇 정의·로봇팔 설정 (&lt;urdf&gt;\sagarm.txt 글)</summary>
            public const string OJWEXT_ROBOTDEF = "robotDef";
            /// <summary>확장 구역 — 수직 눈 확인 기록 (실습 상태 state.txt 의 eyedef·eyemap·eyetime 줄)</summary>
            public const string OJWEXT_EYERECORD = "eyeRecord";
            /// <summary>확장 구역 — 적용 중 처짐 보정 모델 (&lt;urdf&gt;\sagcomp_model.txt 글)</summary>
            public const string OJWEXT_SAGCOMP = "sagComp";

            /// <summary>디자이너 파일의 확장 구역 — 이름 붙은 글 (텍스트 형식 &lt;ext&gt;, 바이너리 끝 꼬리 블록).
            /// 텍스트 형식에서 이 DLL 이 모르는 요소·속성도 여기에 담아 두고 다시 쓸 때 그대로 남긴다 (바이너리 꼬리에는 모르는 최상위 요소만)</summary>
            public class COjwFileExt
            {
                private readonly List<string> m_lstName = new List<string>();
                private readonly Dictionary<string, string> m_dicText = new Dictionary<string, string>(StringComparer.Ordinal);
                internal readonly List<string> lstUnknownXml = new List<string>();                                                     // 모르는 최상위 요소 (OuterXml)
                internal readonly Dictionary<string, List<KeyValuePair<string, string>>> dicUnknownAttr = new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.Ordinal);   // 요소 키 → 모르는 속성
                internal readonly Dictionary<string, List<string>> dicUnknownChild = new Dictionary<string, List<string>>(StringComparer.Ordinal);   // 요소 키 → 모르는 자식 요소

                /// <summary>구역 이름 (넣은 순서)</summary>
                public string[] Names { get { return m_lstName.ToArray(); } }
                /// <summary>구역 수</summary>
                public int Count { get { return m_lstName.Count; } }
                /// <summary>그 이름의 구역이 있나</summary>
                public bool Has(string strName) { return strName != null && m_dicText.ContainsKey(strName); }
                /// <summary>구역 글 — 없으면 null</summary>
                public string Get(string strName)
                {
                    string s;
                    return (strName != null && m_dicText.TryGetValue(strName, out s)) ? s : null;
                }
                /// <summary>구역 글을 넣거나 바꾼다 (null = 지움)</summary>
                public void Set(string strName, string strText)
                {
                    if (string.IsNullOrEmpty(strName)) return;
                    if (strText == null)
                    {
                        if (m_dicText.Remove(strName)) m_lstName.Remove(strName);
                        return;
                    }
                    if (!m_dicText.ContainsKey(strName)) m_lstName.Add(strName);
                    m_dicText[strName] = strText;
                }
                /// <summary>구역 지우기</summary>
                public void Remove(string strName) { Set(strName, null); }
                /// <summary>텍스트 형식에서 읽은 모르는 요소·속성이 있나 (다시 쓸 때 남긴다)</summary>
                public bool HasUnknown { get { return lstUnknownXml.Count > 0 || dicUnknownAttr.Count > 0 || dicUnknownChild.Count > 0; } }
                /// <summary>구역도 모르는 것도 없다</summary>
                public bool IsEmpty { get { return m_lstName.Count == 0 && !HasUnknown; } }
                // 바이너리 끝 꼬리에 실을 것이 있나 (구역·모르는 최상위 요소)
                internal bool HasTail { get { return m_lstName.Count > 0 || lstUnknownXml.Count > 0; } }
                /// <summary>모두 지움 (구역·모르는 것)</summary>
                public void Clear()
                {
                    m_lstName.Clear(); m_dicText.Clear();
                    lstUnknownXml.Clear(); dicUnknownAttr.Clear(); dicUnknownChild.Clear();
                }
                /// <summary>깊은 복사본</summary>
                public COjwFileExt Clone()
                {
                    COjwFileExt c = new COjwFileExt();
                    foreach (string n in m_lstName) c.Set(n, m_dicText[n]);
                    c.lstUnknownXml.AddRange(lstUnknownXml);
                    foreach (KeyValuePair<string, List<KeyValuePair<string, string>>> kv in dicUnknownAttr) c.dicUnknownAttr[kv.Key] = new List<KeyValuePair<string, string>>(kv.Value);
                    foreach (KeyValuePair<string, List<string>> kv in dicUnknownChild) c.dicUnknownChild[kv.Key] = new List<string>(kv.Value);
                    return c;
                }
                internal void KeepAttr(string strKey, string strName, string strValue)
                {
                    List<KeyValuePair<string, string>> l;
                    if (!dicUnknownAttr.TryGetValue(strKey, out l)) { l = new List<KeyValuePair<string, string>>(); dicUnknownAttr[strKey] = l; }
                    l.Add(new KeyValuePair<string, string>(strName, strValue));
                }
                internal void KeepChild(string strKey, string strXml)
                {
                    List<string> l;
                    if (!dicUnknownChild.TryGetValue(strKey, out l)) { l = new List<string>(); dicUnknownChild[strKey] = l; }
                    l.Add(strXml);
                }
                internal bool HasKept(string strKey) { return dicUnknownAttr.ContainsKey(strKey) || dicUnknownChild.ContainsKey(strKey); }
            }

            // ────────────────────────────────────────────────────────────
            // 가르기 · 쓰기 입구
            // ────────────────────────────────────────────────────────────

            /// <summary>디자이너 파일 내용이 텍스트 형식인가 — UTF-8/UTF-16 BOM 과 공백을 건너뛴 첫 글자가 '&lt;' (바이너리는 'OJW'·'DHF' 로 시작). 확장자는 보지 않는다</summary>
            public static bool OjwFile_IsText(byte[] b)
            {
                if (b == null || b.Length == 0) return false;
                int i = 0;
                if (b.Length >= 2 && ((b[0] == 0xFF && b[1] == 0xFE) || (b[0] == 0xFE && b[1] == 0xFF)))
                {
                    bool bLe = (b[0] == 0xFF);
                    for (i = 2; i + 1 < b.Length; i += 2)
                    {
                        int c = bLe ? (b[i] | (b[i + 1] << 8)) : ((b[i] << 8) | b[i + 1]);
                        if (c == ' ' || c == '\t' || c == '\r' || c == '\n') continue;
                        return c == '<';
                    }
                    return false;
                }
                if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) i = 3;
                while (i < b.Length && (b[i] == ' ' || b[i] == '\t' || b[i] == '\r' || b[i] == '\n')) i++;
                return i < b.Length && b[i] == '<';
            }
            /// <summary>파일의 형식 (앞부분만 읽는다). 없거나 못 읽으면 Binary</summary>
            public static EOjwFileFormat OjwFile_GetFormat(string strFileName)
            {
                try
                {
                    using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] b = new byte[(int)Math.Min(fs.Length, 4096)];
                        int n = fs.Read(b, 0, b.Length);
                        if (n < b.Length) Array.Resize(ref b, n);
                        return OjwFile_IsText(b) ? EOjwFileFormat.Text : EOjwFileFormat.Binary;
                    }
                }
                catch { return EOjwFileFormat.Binary; }
            }

            /// <summary>텍스트 형식으로 저장 — FileSave(파일, 헤더, EOjwFileFormat.Text) 와 같다</summary>
            public void FileSave_Text(String strFileName, COjwDesignerHeader CHeader)
            {
                FileSave(strFileName, CHeader, EOjwFileFormat.Text);
            }
            /// <summary>형식을 골라 저장. Binary = FileSave(파일, 헤더) 와 같다 (헤더의 확장 구역이 있으면 끝 꼬리 블록이 붙는다).
            /// 실패 까닭은 GetFileSave_Error ("" = 끝까지 씀)</summary>
            public void FileSave(String strFileName, COjwDesignerHeader CHeader, EOjwFileFormat eFormat)
            {
                if (eFormat == EOjwFileFormat.Text) OjwText_Save(strFileName, CHeader);
                else FileSave(strFileName, CHeader, (String)null);
            }

            private String m_strFileOpen_Warn = String.Empty;
            /// <summary>마지막 텍스트 형식 읽기의 경고·실패 까닭 ("" = 없음) — 읽지 못한 값(기본값으로 둠)·모르는 축 번호 등</summary>
            public String GetFileOpen_Warn() { return m_strFileOpen_Warn; }

            // 텍스트 형식 쓰기 — 전체를 메모리에서 만든 뒤 한 번에 쓴다 (만들다 실패하면 있던 파일은 그대로)
            private void OjwText_Save(String strFileName, COjwDesignerHeader CHeader)
            {
                m_strFileSave_Error = String.Empty;
                if (String.IsNullOrEmpty(strFileName))
                {
                    m_strFileSave_Error = "File Save Error - Null FileName";
                    Ojw.CMessage.Write_Error(m_strFileSave_Error);
                    return;
                }
                if (CHeader == null) CHeader = m_CHeader;
                byte[] abyFile;
                try { abyFile = OjwText_Build(CHeader); }
                catch (Exception ex)
                {
                    m_strFileSave_Error = ex.GetType().Name + ": " + ex.Message;
                    try { Ojw.CMessage.Write_Error("File Save Error - " + strFileName + " (" + m_strFileSave_Error + ") — 파일은 그대로"); } catch { }
                    return;
                }
                try { File.WriteAllBytes(strFileName, abyFile); }
                catch (Exception ex)
                {
                    m_strFileSave_Error = ex.GetType().Name + ": " + ex.Message;
                    try { Ojw.CMessage.Write_Error("File Save Error - " + strFileName + " (" + m_strFileSave_Error + ")"); } catch { }
                    return;
                }
                try { FileSave_RebuildDh(); } catch { }        // 바이너리 FileSave 와 같은 뒤처리 (쓴 뒤의 DH 해석 — 실패해도 파일은 다 썼다)
            }

            // 텍스트 형식 전체 (UTF-8, BOM 없음, CRLF). 출처는 바이너리 FileSave 와 같다
            private byte[] OjwText_Build(COjwDesignerHeader CHeader)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                COjwDesignerHeader H = m_CHeader;                                     // 모터·슬롯·그룹·Json — 바이너리 FileSave 와 같이 m_CHeader
                COjwFileExt ext = (CHeader.Ext != null) ? CHeader.Ext : new COjwFileExt();
                #region 바이너리 FileSave 와 같은 쓰기 전 정리 (Guide 칸)
                for (int i = 0; i < 256; i++)
                {
                    if (H.pSMotorInfo[i].nGuide_RingDir == 0) H.pSMotorInfo[i].nGuide_RingDir = 1;
                    if (H.pSMotorInfo[i].afGuide_Pos == null)
                    {
                        H.pSMotorInfo[i].afGuide_Pos = new float[6];
                        H.pSMotorInfo[i].anGuide_Off_IDs = new int[6];
                        H.pSMotorInfo[i].anGuide_Off_Dir = new int[6];
                    }
                }
                #endregion 바이너리 FileSave 와 같은 쓰기 전 정리 (Guide 칸)

                MemoryStream ms = new MemoryStream();
                XmlWriterSettings st = new XmlWriterSettings();
                st.Encoding = new UTF8Encoding(false);
                st.Indent = true;
                st.IndentChars = "  ";
                st.NewLineChars = "\r\n";
                st.NewLineHandling = NewLineHandling.None;                         // 글 칸은 그대로 (줄 끝은 nl= 로 따로 적는다)
                st.CheckCharacters = true;
                using (XmlWriter w = XmlWriter.Create(ms, st))
                {
                    COjwXw x = new COjwXw(w);
                    w.WriteStartDocument();
                    w.WriteComment(" OpenJigWare 디자이너 파일 (텍스트 형식) - 모르는 요소와 속성은 읽을 때 무시하고 다시 쓸 때 남긴다. 빠진 것은 기본값 ");
                    x.Start("ojw");
                    x.Attr("format", "text");
                    x.Attr("lib", OjwText_LibVersion());
                    x.KeptAttr(ext, "root");

                    #region header
                    x.Start("header");
                    x.Attr("nDefaultFunctionNumber", CHeader.nDefaultFunctionNumber.ToString(ci));
                    x.Attr("nModelNum", CHeader.nModelNum.ToString(ci));
                    x.StrFixed("strTitle", CHeader.strTitle, 21);
                    x.StrFixed("strModelName", CHeader.strModelName, 21);
                    x.Attr("cBackColor", OjwText_C2S(GetBackColor()));            // 바이너리처럼 지금 화면 값
                    x.Attr("nMotorCnt", CHeader.nMotorCnt.ToString(ci));
                    x.Attr("SInitAngle.pan", OjwText_F2S(GetPan()));
                    x.Attr("SInitAngle.tilt", OjwText_F2S(GetTilt()));
                    x.Attr("SInitAngle.swing", OjwText_F2S(GetSwing()));
                    float fX, fY, fZ;
                    GetPos_Display(out fX, out fY, out fZ);
                    x.Attr("SInitPos.x", OjwText_F2S(fX));
                    x.Attr("SInitPos.y", OjwText_F2S(fY));
                    x.Attr("SInitPos.z", OjwText_F2S(fZ));
                    x.Attr("fInitScale", OjwText_F2S(GetScale()));
                    x.Attr("nWheelCounter_2", CHeader.nWheelCounter_2.ToString(ci));
                    x.Attr("nWheelCounter_3", CHeader.nWheelCounter_3.ToString(ci));
                    x.Attr("nWheelCounter_4", CHeader.nWheelCounter_4.ToString(ci));
                    x.KeptAttr(ext, "header");
                    x.KeptChild(ext, "header");
                    x.End();
                    #endregion header

                    #region motors — 기본값과 다른 축만 (모르는 것이 남은 축도)
                    x.Start("motors");
                    x.KeptAttr(ext, "motors");
                    COjwDesignerHeader hDef = OjwText_DefaultHeader();
                    for (int i = 0; i < 256; i++)
                    {
                        List<KeyValuePair<string, string>> lst = OjwText_FieldAttrs(H.pSMotorInfo[i], s_afiMotor, "strNickName", 32);
                        string strKey = "motor:" + i.ToString(ci);
                        if (!ext.HasKept(strKey) && OjwText_SameAttrs(lst, OjwText_FieldAttrs(hDef.pSMotorInfo[i], s_afiMotor, "strNickName", 32))) continue;
                        x.Start("motor");
                        x.Attr("axis", i.ToString(ci));
                        foreach (KeyValuePair<string, string> kv in lst) x.Attr(kv.Key, kv.Value);
                        x.KeptAttr(ext, strKey);
                        x.KeptChild(ext, strKey);
                        x.End();
                    }
                    x.KeptChild(ext, "motors");
                    x.End();
                    #endregion motors

                    #region slots — 무엇이든 있는 슬롯만 (그룹 이름·수식·역기구학 — 암호 칸은 바이너리와 같은 암호 바이트)
                    x.Start("slots");
                    x.KeptAttr(ext, "slots");
                    for (int i = 0; i < 512; i++)
                    {
                        string strG = H.pstrGroupName[i] ?? String.Empty, strK = H.pstrKinematics[i] ?? String.Empty, strI = H.pstrInverseKinematics[i] ?? String.Empty;
                        string strKey = "slot:" + i.ToString(ci);
                        bool bAny = H.pnSecret[i] != 0 || H.pnType[i] != 0 || H.pbPython[i] || strG.Length > 0 || strK.Length > 0 || strI.Length > 0;
                        if (!bAny && !ext.HasKept(strKey)) continue;
                        bool bSecret = (H.pnSecret[i] > 0);
                        x.Start("slot");
                        x.Attr("index", i.ToString(ci));
                        x.Attr("pnSecret", H.pnSecret[i].ToString(ci));
                        x.Attr("pnType", H.pnType[i].ToString(ci));
                        x.Attr("pbPython", H.pbPython[i] ? "1" : "0");
                        x.Str("pstrGroupName", strG);
                        x.KeptAttr(ext, strKey);
                        if (bSecret)
                        {
                            if (strK.Length > 0) x.Secret("kinematics", CEncryption.Encryption(true, Encoding.Default.GetBytes(strK)));
                            if (strI.Length > 0) x.Secret("inverse", CEncryption.Encryption(true, Encoding.Default.GetBytes(strI)));
                        }
                        else
                        {
                            if (strK.Length > 0) x.Text("kinematics", null, null, strK);
                            if (strI.Length > 0) x.Text("inverse", null, null, strI);
                        }
                        x.KeptChild(ext, strKey);
                        x.End();
                    }
                    x.KeptChild(ext, "slots");
                    x.End();
                    #endregion slots

                    if (!String.IsNullOrEmpty(CHeader.strDrawModel)) x.Text("drawModel", null, null, CHeader.strDrawModel);
                    if (!String.IsNullOrEmpty(CHeader.strComment)) x.Text("comment", null, null, CHeader.strComment);

                    #region motorsEnabled (m_lstMotorsEn)
                    StringBuilder sbEn = new StringBuilder();
                    for (int i = 0; i < m_lstMotorsEn.Count; i++) { if (i > 0) sbEn.Append(','); sbEn.Append(m_lstMotorsEn[i].ToString(ci)); }
                    x.Start("motorsEnabled");
                    w.WriteString(sbEn.ToString());
                    x.End();
                    #endregion motorsEnabled (m_lstMotorsEn)

                    #region groups (lstGroupInfo)
                    x.Start("groups");
                    x.KeptAttr(ext, "groups");
                    if (H.lstGroupInfo != null)
                        foreach (SGroupInfo_t g in H.lstGroupInfo)
                        {
                            x.Start("group");
                            x.Attr("nNumber", g.nNumber.ToString(ci));
                            x.Str("strName", g.strName);
                            x.End();
                        }
                    x.KeptChild(ext, "groups");
                    x.End();
                    #endregion groups (lstGroupInfo)

                    #region json
                    x.Start("json");
                    foreach (KeyValuePair<string, string> kv in OjwText_FieldAttrs(H.SJson, s_afiJson, null, 0)) x.Attr(kv.Key, kv.Value);
                    x.KeptAttr(ext, "json");
                    x.KeptChild(ext, "json");
                    x.End();
                    #endregion json

                    OjwText_WriteExt(x, ext);
                    foreach (string strXml in ext.lstUnknownXml) x.Raw(strXml);
                    x.End();
                    w.WriteEndDocument();
                }
                return ms.ToArray();
            }

            // <ext> — 구역마다 <section name="…"> 글 (텍스트 형식과 바이너리 꼬리가 같이 쓴다)
            private static void OjwText_WriteExt(COjwXw x, COjwFileExt ext)
            {
                if (ext.Count == 0 && !ext.HasKept("ext")) return;
                x.Start("ext");
                x.KeptAttr(ext, "ext");
                foreach (string strName in ext.Names) x.Text("section", "name", strName, ext.Get(strName));
                x.KeptChild(ext, "ext");
                x.End();
            }

            /// <summary>쓴 라이브러리 버전 (lib= — 참고용)</summary>
            private static string OjwText_LibVersion()
            {
                try { return typeof(Ojw).Assembly.GetName().Version.ToString(); }
                catch { return ""; }
            }

            // ────────────────────────────────────────────────────────────
            // 읽기
            // ────────────────────────────────────────────────────────────

            // 텍스트 형식 → 헤더. 바이너리 읽기와 같은 헤더(같은 기본값·고정 폭 문자열 널 채움)와 같은 부수 효과 (m_lstMotorsEn, m_CRobotis 모델).
            //   형식이 아니면 null 과 까닭. 읽지 못한 값은 기본값 그대로 두고 m_strFileOpen_Warn 에 남긴다
            private COjwDesignerHeader OjwText_Read(byte[] byteData, out string strWhy)
            {
                strWhy = "";
                m_strFileOpen_Warn = String.Empty;
                List<string> lstWarn = new List<string>();
                CultureInfo ci = CultureInfo.InvariantCulture;
                try
                {
                    XmlDocument doc = OjwText_Parse(byteData);
                    XmlElement root = doc.DocumentElement;
                    if (root == null || root.Name != "ojw") { strWhy = "텍스트 형식 .ojw 가 아님 (최상위 요소 <ojw> 가 없음)"; m_strFileOpen_Warn = strWhy; return null; }

                    COjwDesignerHeader h = new COjwDesignerHeader();
                    h.strVersion = _STR_EXT.ToUpper() + _STR_EXT_VERSION.ToUpper();
                    h.nVersion = OjwText_VersionNumber(_STR_EXT_VERSION);
                    h.eFileFormat = EOjwFileFormat.Text;
                    #region 바이너리로 읽은 빈 값과 같게 — 고정 폭 문자열은 널 채움, 설명 ""
                    h.strTitle = OjwText_PadFixed("", 21);
                    h.strModelName = OjwText_PadFixed("", 21);
                    h.strComment = "";
                    for (int i = 0; i < 256; i++) h.pSMotorInfo[i].strNickName = OjwText_PadFixed("", 32);
                    #endregion 바이너리로 읽은 빈 값과 같게 — 고정 폭 문자열은 널 채움, 설명 ""
                    bool[] abSecretBytes = new bool[512];                          // 암호 칸 바이트를 파일에서 받은 슬롯 (나머지는 글의 바이트)
                    bool[] abInvBytes = new bool[512];
                    List<int> lstEn = new List<int>();

                    OjwText_KeepUnknownAttr(h.Ext, "root", root, new string[] { "format", "lib" });
                    foreach (XmlNode nd in root.ChildNodes)
                    {
                        XmlElement e = nd as XmlElement;
                        if (e == null) continue;
                        switch (e.Name)
                        {
                            case "header": OjwText_ReadHeader(h, e, lstWarn); break;
                            case "motors":
                                OjwText_KeepUnknownAttr(h.Ext, "motors", e, null);
                                foreach (XmlNode nm in e.ChildNodes)
                                {
                                    XmlElement em = nm as XmlElement;
                                    if (em == null) continue;
                                    if (em.Name != "motor") { h.Ext.KeepChild("motors", em.OuterXml); continue; }
                                    int nAxis;
                                    if (!OjwText_S2I(em.GetAttribute("axis"), out nAxis) || nAxis < 0 || nAxis >= 256) { lstWarn.Add("motor axis='" + em.GetAttribute("axis") + "' — 0~255 밖이거나 수가 아님, 건너뜀"); continue; }
                                    object o = h.pSMotorInfo[nAxis];
                                    o = OjwText_ReadFields(em, o, s_afiMotor, "strNickName", 32, lstWarn, "motor " + nAxis.ToString(ci));
                                    SMotorInfo_t m = (SMotorInfo_t)o;
                                    if (m.nGuide_RingDir == 0) m.nGuide_RingDir = 1;                     // 바이너리 읽기와 같다
                                    m.afGuide_Pos = OjwText_AtLeast(m.afGuide_Pos, 6);
                                    m.anGuide_Off_IDs = OjwText_AtLeast(m.anGuide_Off_IDs, 6);
                                    m.anGuide_Off_Dir = OjwText_AtLeast(m.anGuide_Off_Dir, 6);
                                    h.pSMotorInfo[nAxis] = m;
                                    OjwText_KeepUnknown(h.Ext, "motor:" + nAxis.ToString(ci), em, OjwText_KnownNames(s_afiMotor, "axis"), null);
                                }
                                break;
                            case "slots":
                                OjwText_KeepUnknownAttr(h.Ext, "slots", e, null);
                                foreach (XmlNode ns in e.ChildNodes)
                                {
                                    XmlElement es = ns as XmlElement;
                                    if (es == null) continue;
                                    if (es.Name != "slot") { h.Ext.KeepChild("slots", es.OuterXml); continue; }
                                    int k;
                                    if (!OjwText_S2I(es.GetAttribute("index"), out k) || k < 0 || k >= 512) { lstWarn.Add("slot index='" + es.GetAttribute("index") + "' — 0~511 밖이거나 수가 아님, 건너뜀"); continue; }
                                    OjwText_ReadSlot(h, k, es, abSecretBytes, abInvBytes, lstWarn);
                                }
                                break;
                            case "drawModel": h.strDrawModel = OjwText_ReadText(e, lstWarn); break;
                            case "comment": h.strComment = OjwText_ReadText(e, lstWarn); break;
                            case "motorsEnabled":
                                foreach (string t in e.InnerText.Split(','))
                                {
                                    if (t.Trim().Length == 0) continue;
                                    int n;
                                    if (OjwText_S2I(t, out n)) lstEn.Add(n); else lstWarn.Add("motorsEnabled '" + t.Trim() + "' 을 읽지 못함");
                                }
                                break;
                            case "groups":
                                OjwText_KeepUnknownAttr(h.Ext, "groups", e, null);
                                foreach (XmlNode ng in e.ChildNodes)
                                {
                                    XmlElement eg = ng as XmlElement;
                                    if (eg == null) continue;
                                    if (eg.Name != "group") { h.Ext.KeepChild("groups", eg.OuterXml); continue; }
                                    SGroupInfo_t g = new SGroupInfo_t();
                                    int n;
                                    if (OjwText_S2I(eg.GetAttribute("nNumber"), out n)) g.nNumber = n; else lstWarn.Add("group nNumber 를 읽지 못함");
                                    g.strName = OjwText_ReadStr(eg, "strName", -1, lstWarn) ?? "";
                                    h.lstGroupInfo.Add(g);
                                }
                                break;
                            case "json":
                                {
                                    object o = h.SJson;
                                    o = OjwText_ReadFields(e, o, s_afiJson, null, 0, lstWarn, "json");
                                    COjwDesignerHeader.SJson_t j = (COjwDesignerHeader.SJson_t)o;
                                    j.afScene_Position = OjwText_AtLeast(j.afScene_Position, 6);
                                    j.afCamera_Position = OjwText_AtLeast(j.afCamera_Position, 6);
                                    j.afRobot_Position = OjwText_AtLeast(j.afRobot_Position, 6);
                                    if (j.strBleName == null) j.strBleName = "";
                                    h.SJson = j;
                                    OjwText_KeepUnknown(h.Ext, "json", e, OjwText_KnownNames(s_afiJson, null), null);
                                    break;
                                }
                            case "ext": OjwText_ReadExt(h.Ext, e, lstWarn); break;
                            default: h.Ext.lstUnknownXml.Add(e.OuterXml); break;     // 이 DLL 이 모르는 요소 — 다시 쓸 때 그대로
                        }
                    }
                    #region 바이너리 읽기와 같은 수식 바이트 (암호화일 수도 있는 원본 바이트)
                    for (int i = 0; i < 512; i++)
                    {
                        if (h.pstrGroupName[i] == null) h.pstrGroupName[i] = "";
                        if (h.pstrKinematics[i] == null) h.pstrKinematics[i] = "";
                        if (h.pstrInverseKinematics[i] == null) h.pstrInverseKinematics[i] = "";
                        if (!abSecretBytes[i]) h.pSEncryptKinematics_encryption[i].byteEncryption = Encoding.Default.GetBytes(h.pstrKinematics[i]);
                        if (!abInvBytes[i]) h.pSEncryptInverseKinematics_encryption[i].byteEncryption = Encoding.Default.GetBytes(h.pstrInverseKinematics[i]);
                    }
                    #endregion 바이너리 읽기와 같은 수식 바이트 (암호화일 수도 있는 원본 바이트)

                    #region 부수 효과 — 바이너리 읽기와 같다 (켜진 모터 목록, 모터 모델)
                    m_lstMotorsEn.Clear();
                    m_lstMotorsEn.AddRange(lstEn);
#if !_MONSTER_LIB
                    for (int i = 0; i < 256; i++)
                        if (h.pSMotorInfo[i].nHwMotor_Index > 0) m_CRobotis.SetParam(i, h.pSMotorInfo[i].nHwMotor_Index);   // 모델명이 지정되어 있다면 앞의것들 무시 (바이너리 읽기와 같음)
#endif
                    #endregion 부수 효과 — 바이너리 읽기와 같다 (켜진 모터 목록, 모터 모델)
                    m_strFileOpen_Warn = String.Join("\r\n", lstWarn.ToArray());
                    return h;
                }
                catch (Exception ex)
                {
                    strWhy = "텍스트 형식 .ojw 를 읽지 못함: " + ex.Message;
                    m_strFileOpen_Warn = strWhy;
                    return null;
                }
            }

            private static XmlDocument OjwText_Parse(byte[] byteData)
            {
                XmlReaderSettings st = new XmlReaderSettings();
                st.DtdProcessing = DtdProcessing.Prohibit;
                st.XmlResolver = null;
                st.IgnoreComments = true;
                st.IgnoreProcessingInstructions = true;
                st.CheckCharacters = true;
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;                                       // CDATA·글 칸의 공백 그대로
                doc.XmlResolver = null;
                using (MemoryStream ms = new MemoryStream(byteData))
                using (XmlReader r = XmlReader.Create(ms, st))
                    doc.Load(r);
                return doc;
            }

            private static readonly string[] s_astrHeaderAttr = {
                "nDefaultFunctionNumber", "nModelNum", "strTitle", "strTitle_b64", "strModelName", "strModelName_b64", "cBackColor", "nMotorCnt",
                "SInitAngle.pan", "SInitAngle.tilt", "SInitAngle.swing", "SInitPos.x", "SInitPos.y", "SInitPos.z", "fInitScale",
                "nWheelCounter_2", "nWheelCounter_3", "nWheelCounter_4" };
            private static void OjwText_ReadHeader(COjwDesignerHeader h, XmlElement e, List<string> lstWarn)
            {
                int n; float f; Color c;
                if (OjwText_AttrI(e, "nDefaultFunctionNumber", lstWarn, out n)) h.nDefaultFunctionNumber = n;
                if (OjwText_AttrI(e, "nModelNum", lstWarn, out n)) h.nModelNum = n;
                string s = OjwText_ReadStr(e, "strTitle", 21, lstWarn); if (s != null) h.strTitle = s;
                s = OjwText_ReadStr(e, "strModelName", 21, lstWarn); if (s != null) h.strModelName = s;
                if (e.HasAttribute("cBackColor")) { if (OjwText_S2C(e.GetAttribute("cBackColor"), out c)) h.cBackColor = c; else lstWarn.Add("header cBackColor 를 읽지 못함"); }
                if (OjwText_AttrI(e, "nMotorCnt", lstWarn, out n)) h.nMotorCnt = n;
                if (OjwText_AttrF(e, "SInitAngle.pan", lstWarn, out f)) h.SInitAngle.pan = f;
                if (OjwText_AttrF(e, "SInitAngle.tilt", lstWarn, out f)) h.SInitAngle.tilt = f;
                if (OjwText_AttrF(e, "SInitAngle.swing", lstWarn, out f)) h.SInitAngle.swing = f;
                if (OjwText_AttrF(e, "SInitPos.x", lstWarn, out f)) h.SInitPos.x = f;
                if (OjwText_AttrF(e, "SInitPos.y", lstWarn, out f)) h.SInitPos.y = f;
                if (OjwText_AttrF(e, "SInitPos.z", lstWarn, out f)) h.SInitPos.z = f;
                if (OjwText_AttrF(e, "fInitScale", lstWarn, out f)) h.fInitScale = f;
                if (OjwText_AttrI(e, "nWheelCounter_2", lstWarn, out n)) h.nWheelCounter_2 = n;
                if (OjwText_AttrI(e, "nWheelCounter_3", lstWarn, out n)) h.nWheelCounter_3 = n;
                if (OjwText_AttrI(e, "nWheelCounter_4", lstWarn, out n)) h.nWheelCounter_4 = n;
                OjwText_KeepUnknown(h.Ext, "header", e, new List<string>(s_astrHeaderAttr), null);
            }
            private static readonly string[] s_astrSlotAttr = { "index", "pnSecret", "pnType", "pbPython", "pstrGroupName", "pstrGroupName_b64" };
            private static readonly string[] s_astrSlotChild = { "kinematics", "inverse" };
            private static void OjwText_ReadSlot(COjwDesignerHeader h, int k, XmlElement e, bool[] abSecretBytes, bool[] abInvBytes, List<string> lstWarn)
            {
                int n;
                string strW = "slot " + k.ToString(CultureInfo.InvariantCulture);
                if (OjwText_AttrI(e, "pnSecret", lstWarn, out n)) h.pnSecret[k] = n;
                if (OjwText_AttrI(e, "pnType", lstWarn, out n)) h.pnType[k] = n;
                if (e.HasAttribute("pbPython")) { bool b; if (OjwText_S2B(e.GetAttribute("pbPython"), out b)) h.pbPython[k] = b; else lstWarn.Add(strW + " pbPython 을 읽지 못함"); }
                string s = OjwText_ReadStr(e, "pstrGroupName", -1, lstWarn); if (s != null) h.pstrGroupName[k] = s;
                foreach (XmlNode nd in e.ChildNodes)
                {
                    XmlElement c = nd as XmlElement;
                    if (c == null) continue;
                    bool bK = (c.Name == "kinematics"), bI = (c.Name == "inverse");
                    if (!bK && !bI) continue;
                    if (c.GetAttribute("enc") == "secret")
                    {
                        // 암호 칸 — 바이너리와 같은 바이트 (글 = 그 바이트를 Encoding.Default 로 읽은 것 — 바이너리 읽기와 같다)
                        byte[] ab;
                        try { ab = Convert.FromBase64String(OjwText_InnerText(c).Trim()); }
                        catch { lstWarn.Add(strW + " " + c.Name + " (secret) base64 를 읽지 못함 — 빈 글"); continue; }
                        if (bK) { h.pstrKinematics[k] = Encoding.Default.GetString(ab); h.pSEncryptKinematics_encryption[k].byteEncryption = ab; abSecretBytes[k] = true; }
                        else { h.pstrInverseKinematics[k] = Encoding.Default.GetString(ab); h.pSEncryptInverseKinematics_encryption[k].byteEncryption = ab; abInvBytes[k] = true; }
                        continue;
                    }
                    string t = OjwText_ReadText(c, lstWarn);
                    if (bK) h.pstrKinematics[k] = t; else h.pstrInverseKinematics[k] = t;
                }
                OjwText_KeepUnknown(h.Ext, "slot:" + k.ToString(CultureInfo.InvariantCulture), e, new List<string>(s_astrSlotAttr), new List<string>(s_astrSlotChild));
            }
            private static void OjwText_ReadExt(COjwFileExt ext, XmlElement e, List<string> lstWarn)
            {
                OjwText_KeepUnknownAttr(ext, "ext", e, null);
                foreach (XmlNode nd in e.ChildNodes)
                {
                    XmlElement c = nd as XmlElement;
                    if (c == null) continue;
                    if (c.Name != "section" || !c.HasAttribute("name")) { ext.KeepChild("ext", c.OuterXml); continue; }
                    ext.Set(c.GetAttribute("name"), OjwText_ReadText(c, lstWarn));
                }
            }

            // ────────────────────────────────────────────────────────────
            // 바이너리 끝 꼬리 블록 — 'OJWX' + 길이(4) + XML(UTF-8) + 'XE'
            // ────────────────────────────────────────────────────────────

            // 바이너리 FileSave 가 Json 칸 뒤에 — 확장 구역(또는 모르는 최상위 요소)이 있을 때만 (없으면 아무것도 쓰지 않는다 — 바이트가 예전과 같다)
            private static void OjwText_WriteBinaryTail(Stream fs, COjwDesignerHeader CHeader)
            {
                if (CHeader == null || CHeader.Ext == null || !CHeader.Ext.HasTail) return;
                byte[] abyXml = OjwText_BuildTail(CHeader.Ext);
                fs.Write(new byte[] { (byte)'O', (byte)'J', (byte)'W', (byte)'X' }, 0, 4);
                fs.Write(BitConverter.GetBytes(abyXml.Length), 0, 4);
                fs.Write(abyXml, 0, abyXml.Length);
                fs.WriteByte((byte)'X');
                fs.WriteByte((byte)'E');
            }
            private static byte[] OjwText_BuildTail(COjwFileExt ext)
            {
                MemoryStream ms = new MemoryStream();
                XmlWriterSettings st = new XmlWriterSettings();
                st.Encoding = new UTF8Encoding(false);
                st.Indent = true;
                st.IndentChars = "  ";
                st.NewLineChars = "\r\n";
                st.NewLineHandling = NewLineHandling.None;
                st.CheckCharacters = true;
                using (XmlWriter w = XmlWriter.Create(ms, st))
                {
                    COjwXw x = new COjwXw(w);
                    w.WriteStartDocument();
                    x.Start("ojw");
                    x.Attr("format", "ext");
                    x.Attr("lib", OjwText_LibVersion());
                    OjwText_WriteExt(x, ext);
                    foreach (string strXml in ext.lstUnknownXml) x.Raw(strXml);
                    x.End();
                    w.WriteEndDocument();
                }
                return ms.ToArray();
            }
            // 바이너리 읽기가 Json 칸까지 읽은 뒤 (nPos = 그 끝) — 꼬리가 있으면 확장 구역을 헤더에. 못 읽어도 본문은 그대로 (조용히 넘어간다)
            private static void OjwText_ReadBinaryTail(byte[] b, int nPos, COjwDesignerHeader h)
            {
                try
                {
                    if (b == null || h == null || nPos < 0 || nPos + 8 > b.Length) return;
                    if (b[nPos] != 'O' || b[nPos + 1] != 'J' || b[nPos + 2] != 'W' || b[nPos + 3] != 'X') return;
                    int n = BitConverter.ToInt32(b, nPos + 4);
                    if (n <= 0 || (long)nPos + 8 + n > b.Length) return;
                    byte[] x = new byte[n];
                    Array.Copy(b, nPos + 8, x, 0, n);
                    XmlDocument doc = OjwText_Parse(x);
                    XmlElement root = doc.DocumentElement;
                    if (root == null || root.Name != "ojw") return;
                    List<string> lstWarn = new List<string>();
                    foreach (XmlNode nd in root.ChildNodes)
                    {
                        XmlElement e = nd as XmlElement;
                        if (e == null) continue;
                        if (e.Name == "ext") OjwText_ReadExt(h.Ext, e, lstWarn);
                        else h.Ext.lstUnknownXml.Add(e.OuterXml);
                    }
                }
                catch { }
            }

            // ────────────────────────────────────────────────────────────
            // 구조체 칸 ↔ 속성 (리플렉션 — SMotorInfo_t·SJson_t 에 칸이 늘면 저절로 따라간다)
            // ────────────────────────────────────────────────────────────

            private static readonly FieldInfo[] s_afiMotor = OjwText_Fields(typeof(SMotorInfo_t));
            private static readonly FieldInfo[] s_afiJson = OjwText_Fields(typeof(COjwDesignerHeader.SJson_t));
            private static FieldInfo[] OjwText_Fields(Type t)
            {
                FieldInfo[] a = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                Array.Sort(a, delegate(FieldInfo p, FieldInfo q) { return p.MetadataToken.CompareTo(q.MetadataToken); });   // 선언 순서
                return a;
            }
            private static List<string> OjwText_KnownNames(FieldInfo[] afi, string strExtra)
            {
                List<string> l = new List<string>();
                if (strExtra != null) l.Add(strExtra);
                foreach (FieldInfo fi in afi) { l.Add(fi.Name); if (fi.FieldType == typeof(string)) l.Add(fi.Name + "_b64"); }
                return l;
            }
            private static COjwDesignerHeader s_hdrDefault = null;
            // 헤더 기본값 (생성자 그대로) — 기본값과 같은 축은 쓰지 않으려고 (읽을 때 빠진 축 = 이 값)
            private static COjwDesignerHeader OjwText_DefaultHeader()
            {
                if (s_hdrDefault == null) s_hdrDefault = new COjwDesignerHeader();
                return s_hdrDefault;
            }
            // 구조체의 칸 → (이름, 글) — 글 칸은 그대로 쓸 수 있으면 이름, 아니면 이름_b64. strFixed = 고정 폭 문자열 칸 (끝 널을 떼고 쓴다)
            private static List<KeyValuePair<string, string>> OjwText_FieldAttrs(object boxed, FieldInfo[] afi, string strFixed, int nWidth)
            {
                List<KeyValuePair<string, string>> l = new List<KeyValuePair<string, string>>();
                foreach (FieldInfo fi in afi)
                {
                    object v = fi.GetValue(boxed);
                    if (fi.FieldType == typeof(string))
                    {
                        string s = (string)v;
                        if (strFixed != null && fi.Name == strFixed) l.Add(OjwText_FixedPair(fi.Name, s, nWidth));
                        else l.Add(OjwText_StrPair(fi.Name, s));
                        continue;
                    }
                    string t = OjwText_ValueText(v, fi.FieldType);
                    if (t != null) l.Add(new KeyValuePair<string, string>(fi.Name, t));
                }
                return l;
            }
            private static bool OjwText_SameAttrs(List<KeyValuePair<string, string>> a, List<KeyValuePair<string, string>> b)
            {
                if (a.Count != b.Count) return false;
                for (int i = 0; i < a.Count; i++) if (a[i].Key != b[i].Key || a[i].Value != b[i].Value) return false;
                return true;
            }
            // 속성 → 구조체 칸 (있는 것만 — 없으면 기본값 그대로). 반환 = 바꾼 상자
            private static object OjwText_ReadFields(XmlElement e, object boxed, FieldInfo[] afi, string strFixed, int nWidth, List<string> lstWarn, string strWhere)
            {
                foreach (FieldInfo fi in afi)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string s = OjwText_ReadStr(e, fi.Name, (strFixed != null && fi.Name == strFixed) ? nWidth : -1, lstWarn);
                        if (s != null) fi.SetValue(boxed, s);
                        continue;
                    }
                    if (!e.HasAttribute(fi.Name)) continue;
                    object v;
                    if (OjwText_ParseValue(e.GetAttribute(fi.Name), fi.FieldType, out v)) fi.SetValue(boxed, v);
                    else lstWarn.Add(strWhere + " " + fi.Name + "='" + e.GetAttribute(fi.Name) + "' 을 읽지 못함 — 기본값");
                }
                return boxed;
            }
            private static T[] OjwText_AtLeast<T>(T[] a, int n)
            {
                if (a == null) return new T[n];
                if (a.Length >= n) return a;
                T[] b = new T[n];
                Array.Copy(a, b, a.Length);
                return b;
            }

            // ────────────────────────────────────────────────────────────
            // 모르는 것 남기기
            // ────────────────────────────────────────────────────────────

            private static void OjwText_KeepUnknownAttr(COjwFileExt ext, string strKey, XmlElement e, ICollection<string> known)
            {
                foreach (XmlAttribute a in e.Attributes)
                {
                    if (a.Name == "xmlns" || a.Name.StartsWith("xmlns:", StringComparison.Ordinal)) continue;
                    if (known != null && known.Contains(a.Name)) continue;
                    ext.KeepAttr(strKey, a.Name, a.Value);
                }
            }
            private static void OjwText_KeepUnknown(COjwFileExt ext, string strKey, XmlElement e, ICollection<string> knownAttr, ICollection<string> knownChild)
            {
                OjwText_KeepUnknownAttr(ext, strKey, e, knownAttr);
                foreach (XmlNode nd in e.ChildNodes)
                {
                    XmlElement c = nd as XmlElement;
                    if (c == null) continue;
                    if (knownChild != null && knownChild.Contains(c.Name)) continue;
                    ext.KeepChild(strKey, c.OuterXml);
                }
            }

            // ────────────────────────────────────────────────────────────
            // 쓰기 도우미 (속성 이름 중복 막기 · 글 칸 · CDATA)
            // ────────────────────────────────────────────────────────────

            private sealed class COjwXw
            {
                private readonly XmlWriter W;
                private readonly HashSet<string> m_hs = new HashSet<string>(StringComparer.Ordinal);
                public COjwXw(XmlWriter w) { W = w; }
                public void Start(string strName) { W.WriteStartElement(strName); m_hs.Clear(); }
                public void End() { W.WriteEndElement(); }
                public void Attr(string strName, string strValue)
                {
                    if (strValue == null || !m_hs.Add(strName)) return;
                    W.WriteAttributeString(strName, strValue);
                }
                public void Str(string strName, string s)
                {
                    KeyValuePair<string, string> kv = OjwText_StrPair(strName, s);
                    Attr(kv.Key, kv.Value);
                }
                public void StrFixed(string strName, string s, int nWidth)
                {
                    KeyValuePair<string, string> kv = OjwText_FixedPair(strName, s, nWidth);
                    Attr(kv.Key, kv.Value);
                }
                public void KeptAttr(COjwFileExt ext, string strKey)
                {
                    List<KeyValuePair<string, string>> l;
                    if (ext == null || !ext.dicUnknownAttr.TryGetValue(strKey, out l)) return;
                    foreach (KeyValuePair<string, string> kv in l) Attr(kv.Key, kv.Value);
                }
                public void KeptChild(COjwFileExt ext, string strKey)
                {
                    List<string> l;
                    if (ext == null || !ext.dicUnknownChild.TryGetValue(strKey, out l)) return;
                    foreach (string s in l) Raw(s);
                }
                // 남겨 둔 요소 (OuterXml) 를 그대로 다시 쓴다
                public void Raw(string strXml)
                {
                    XmlReaderSettings st = new XmlReaderSettings();
                    st.ConformanceLevel = ConformanceLevel.Fragment;
                    st.DtdProcessing = DtdProcessing.Prohibit;
                    st.XmlResolver = null;
                    using (XmlReader r = XmlReader.Create(new StringReader(strXml), st))
                    {
                        r.MoveToContent();
                        while (!r.EOF)
                        {
                            if (r.NodeType == XmlNodeType.Element) W.WriteNode(r, true);
                            else r.Read();
                        }
                    }
                }
                // 글 요소 — CDATA (줄 끝 nl="crlf"), 그대로 쓸 수 없으면 enc="b64". strAttr = 앞에 붙일 속성 하나 (없으면 null)
                public void Text(string strElem, string strAttr, string strAttrValue, string s)
                {
                    s = s ?? "";
                    Start(strElem);
                    if (strAttr != null) Attr(strAttr, strAttrValue);
                    int nNl = OjwText_NlMode(s);
                    if (nNl < 0 || !OjwText_ValidXml(s, false))
                    {
                        Attr("enc", "b64");
                        W.WriteString(OjwText_B64(s));
                    }
                    else
                    {
                        if (nNl == 1) Attr("nl", "crlf");
                        int nStart = 0, k;
                        while ((k = s.IndexOf("]]>", nStart, StringComparison.Ordinal)) >= 0)   // CDATA 안의 "]]>" 는 두 덩이로 나눈다
                        {
                            W.WriteCData(s.Substring(nStart, k + 2 - nStart));
                            nStart = k + 2;
                        }
                        W.WriteCData(s.Substring(nStart));
                    }
                    End();
                }
                // 암호 칸 바이트 — enc="secret" (base64)
                public void Secret(string strElem, byte[] ab)
                {
                    Start(strElem);
                    Attr("enc", "secret");
                    W.WriteString(Convert.ToBase64String(ab ?? new byte[0]));
                    End();
                }
            }

            // 글 속성 쌍 — 그대로 쓸 수 있으면 (이름, 글), 아니면 (이름_b64, base64)
            private static KeyValuePair<string, string> OjwText_StrPair(string strName, string s)
            {
                s = s ?? "";
                if (OjwText_ValidXml(s, true)) return new KeyValuePair<string, string>(strName, s);
                return new KeyValuePair<string, string>(strName + "_b64", OjwText_B64(s));
            }
            // 고정 폭 글 속성 쌍 — 끝 널을 떼고 쓴다 (읽을 때 바이너리처럼 다시 채운다). 떼고 채운 것이 원래와 다르면(널 뒤에 글자 등) 그대로 _b64
            private static KeyValuePair<string, string> OjwText_FixedPair(string strName, string s, int nWidth)
            {
                s = s ?? "";
                string t = s.TrimEnd('\0');
                if (OjwText_ValidXml(t, true) && (t == s || OjwText_PadFixed(t, nWidth) == s)) return new KeyValuePair<string, string>(strName, t);
                return new KeyValuePair<string, string>(strName + "_b64", OjwText_B64(s));
            }
            // 고정 폭 문자열을 바이너리 읽기처럼 — Encoding.Default 바이트 수가 폭보다 작으면 남는 바이트만큼 널을 붙인다 (넘으면 그대로 — 텍스트는 자르지 않는다)
            private static string OjwText_PadFixed(string s, int nWidth)
            {
                s = s ?? "";
                int n = Encoding.Default.GetByteCount(s);
                return (n < nWidth) ? s + new string('\0', nWidth - n) : s;
            }
            // 글 속성 읽기 — 이름_b64 가 있으면 그대로, 이름이 있으면 (고정 폭이면 널 채움), 둘 다 없으면 null (기본값 그대로)
            private static string OjwText_ReadStr(XmlElement e, string strName, int nFixedWidth, List<string> lstWarn)
            {
                if (e.HasAttribute(strName + "_b64"))
                {
                    try { return OjwText_FromB64(e.GetAttribute(strName + "_b64")); }
                    catch { lstWarn.Add(e.Name + " " + strName + "_b64 를 읽지 못함 — 기본값"); return null; }
                }
                if (!e.HasAttribute(strName)) return null;
                string s = e.GetAttribute(strName);
                return (nFixedWidth > 0) ? OjwText_PadFixed(s, nFixedWidth) : s;
            }
            // 글 요소 읽기 — enc="b64" 면 base64, 아니면 CDATA(있으면 그것만)·글. XML 이 LF 로 읽은 줄 끝을 nl="crlf" 면 CRLF 로
            private static string OjwText_ReadText(XmlElement e, List<string> lstWarn)
            {
                string s = OjwText_InnerText(e);
                if (e.GetAttribute("enc") == "b64")
                {
                    try { return OjwText_FromB64(s.Trim()); }
                    catch { lstWarn.Add(e.Name + " base64 를 읽지 못함 — 빈 글"); return ""; }
                }
                s = s.Replace("\r\n", "\n");
                if (e.GetAttribute("nl") == "crlf") s = s.Replace("\n", "\r\n");
                return s;
            }
            private static string OjwText_InnerText(XmlElement e)
            {
                bool bCData = false;
                foreach (XmlNode nd in e.ChildNodes) if (nd.NodeType == XmlNodeType.CDATA) { bCData = true; break; }
                if (!bCData) return e.InnerText;
                StringBuilder sb = new StringBuilder();
                foreach (XmlNode nd in e.ChildNodes) if (nd.NodeType == XmlNodeType.CDATA) sb.Append(nd.Value);
                return sb.ToString();
            }
            // 줄 끝 — 0 = CR 없음, 1 = 모든 CR 이 CRLF 이고 모든 LF 앞에 CR, -1 = 섞임 (그대로 되살릴 수 없어 base64)
            private static int OjwText_NlMode(string s)
            {
                if (s.IndexOf('\r') < 0) return 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '\r' && (i + 1 >= s.Length || s[i + 1] != '\n')) return -1;
                    if (s[i] == '\n' && (i == 0 || s[i - 1] != '\r')) return -1;
                }
                return 1;
            }
            // XML 1.0 에 그대로 쓸 수 있나 (bAttr = 속성 — 탭·줄바꿈도 안 됨: 읽을 때 공백으로 바뀐다)
            private static bool OjwText_ValidXml(string s, bool bAttr)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '\t' || c == '\n' || c == '\r') { if (bAttr) return false; continue; }
                    if (c < 0x20) return false;
                    if (char.IsHighSurrogate(c))
                    {
                        if (i + 1 >= s.Length || !char.IsLowSurrogate(s[i + 1])) return false;
                        i++;
                        continue;
                    }
                    if (char.IsLowSurrogate(c)) return false;
                    if (c == '￾' || c == '￿') return false;
                }
                return true;
            }
            private static string OjwText_B64(string s) { return Convert.ToBase64String(Encoding.Unicode.GetBytes(s ?? "")); }
            private static string OjwText_FromB64(string s) { return Encoding.Unicode.GetString(Convert.FromBase64String((s ?? "").Trim())); }

            // ────────────────────────────────────────────────────────────
            // 값 ↔ 글 (실수는 다시 읽어 비트까지 같은 글 — 안 되면 0x 비트)
            // ────────────────────────────────────────────────────────────

            private static string OjwText_ValueText(object v, Type t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                if (t == typeof(int)) return ((int)v).ToString(ci);
                if (t == typeof(float)) return OjwText_F2S((float)v);
                if (t == typeof(double)) return OjwText_D2S((double)v);
                if (t == typeof(bool)) return ((bool)v) ? "1" : "0";
                if (t == typeof(short) || t == typeof(byte) || t == typeof(long) || t == typeof(uint) || t == typeof(ushort) || t == typeof(sbyte)) return Convert.ToString(v, ci);
                if (t == typeof(Color)) return OjwText_C2S((Color)v);
                if (v == null) return null;
                if (t == typeof(float[])) return OjwText_Join((float[])v, OjwText_F2S);
                if (t == typeof(int[])) return OjwText_Join((int[])v, delegate(int n) { return n.ToString(ci); });
                if (t == typeof(double[])) return OjwText_Join((double[])v, OjwText_D2S);
                return null;                                                         // 모르는 형 — 쓰지 않는다
            }
            private static bool OjwText_ParseValue(string s, Type t, out object v)
            {
                v = null;
                CultureInfo ci = CultureInfo.InvariantCulture;
                if (t == typeof(int)) { int n; if (!OjwText_S2I(s, out n)) return false; v = n; return true; }
                if (t == typeof(float)) { float f; if (!OjwText_S2F(s, out f)) return false; v = f; return true; }
                if (t == typeof(double)) { double d; if (!OjwText_S2D(s, out d)) return false; v = d; return true; }
                if (t == typeof(bool)) { bool b; if (!OjwText_S2B(s, out b)) return false; v = b; return true; }
                if (t == typeof(Color)) { Color c; if (!OjwText_S2C(s, out c)) return false; v = c; return true; }
                if (t == typeof(short) || t == typeof(byte) || t == typeof(long) || t == typeof(uint) || t == typeof(ushort) || t == typeof(sbyte))
                {
                    try { v = Convert.ChangeType(long.Parse(s.Trim(), NumberStyles.Integer, ci), t, ci); return true; }
                    catch { return false; }
                }
                if (t == typeof(float[]) || t == typeof(int[]) || t == typeof(double[]))
                {
                    List<string> lst = new List<string>();
                    foreach (string p in s.Split(',')) if (p.Trim().Length > 0) lst.Add(p.Trim());
                    if (t == typeof(float[])) { float[] a = new float[lst.Count]; for (int i = 0; i < a.Length; i++) if (!OjwText_S2F(lst[i], out a[i])) return false; v = a; return true; }
                    if (t == typeof(int[])) { int[] a = new int[lst.Count]; for (int i = 0; i < a.Length; i++) if (!OjwText_S2I(lst[i], out a[i])) return false; v = a; return true; }
                    double[] ad = new double[lst.Count]; for (int i = 0; i < ad.Length; i++) if (!OjwText_S2D(lst[i], out ad[i])) return false; v = ad; return true;
                }
                return false;
            }
            private static string OjwText_Join<T>(T[] a, Func<T, string> f)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < a.Length; i++) { if (i > 0) sb.Append(','); sb.Append(f(a[i])); }
                return sb.ToString();
            }
            private static string OjwText_F2S(float f)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                int nBits = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
                if (!float.IsNaN(f) && !float.IsInfinity(f) && nBits != unchecked((int)0x80000000))      // NaN·무한·−0 은 비트로
                {
                    foreach (string strFmt in new string[] { "R", "G9" })
                    {
                        string s = f.ToString(strFmt, ci);
                        float g;
                        if (float.TryParse(s, NumberStyles.Float, ci, out g) && BitConverter.ToInt32(BitConverter.GetBytes(g), 0) == nBits) return s;
                    }
                }
                return "0x" + nBits.ToString("X8", ci);
            }
            private static bool OjwText_S2F(string s, out float f)
            {
                f = 0f;
                s = (s ?? "").Trim();
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    uint u;
                    if (!uint.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out u)) return false;
                    f = BitConverter.ToSingle(BitConverter.GetBytes(u), 0);
                    return true;
                }
                return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
            }
            private static string OjwText_D2S(double d)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                long nBits = BitConverter.DoubleToInt64Bits(d);
                if (!double.IsNaN(d) && !double.IsInfinity(d) && nBits != unchecked((long)0x8000000000000000L))
                {
                    foreach (string strFmt in new string[] { "R", "G17" })
                    {
                        string s = d.ToString(strFmt, ci);
                        double g;
                        if (double.TryParse(s, NumberStyles.Float, ci, out g) && BitConverter.DoubleToInt64Bits(g) == nBits) return s;
                    }
                }
                return "0x" + nBits.ToString("X16", ci);
            }
            private static bool OjwText_S2D(string s, out double d)
            {
                d = 0.0;
                s = (s ?? "").Trim();
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    ulong u;
                    if (!ulong.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out u)) return false;
                    d = BitConverter.Int64BitsToDouble(unchecked((long)u));
                    return true;
                }
                return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
            }
            // 정수 — 10진 또는 0x 16진
            private static bool OjwText_S2I(string s, out int n)
            {
                n = 0;
                s = (s ?? "").Trim();
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    uint u;
                    if (!uint.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out u)) return false;
                    n = unchecked((int)u);
                    return true;
                }
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out n);
            }
            private static bool OjwText_S2B(string s, out bool b)
            {
                s = (s ?? "").Trim();
                b = false;
                if (s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase)) { b = true; return true; }
                if (s == "0" || s.Equals("false", StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            }
            // 색 — "#AARRGGBB" (쓰기), 읽기는 "#RRGGBB"(불투명)·정수도
            private static string OjwText_C2S(Color c) { return "#" + c.ToArgb().ToString("X8", CultureInfo.InvariantCulture); }
            private static bool OjwText_S2C(string s, out Color c)
            {
                c = Color.Empty;
                s = (s ?? "").Trim();
                uint u;
                if (s.StartsWith("#", StringComparison.Ordinal))
                {
                    string h = s.Substring(1);
                    if ((h.Length != 8 && h.Length != 6) || !uint.TryParse(h, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out u)) return false;
                    if (h.Length == 6) u |= 0xFF000000;
                    c = Color.FromArgb(unchecked((int)u));
                    return true;
                }
                int n;
                if (!OjwText_S2I(s, out n)) return false;
                c = Color.FromArgb(n);
                return true;
            }
            private static bool OjwText_AttrI(XmlElement e, string strName, List<string> lstWarn, out int n)
            {
                n = 0;
                if (!e.HasAttribute(strName)) return false;
                if (OjwText_S2I(e.GetAttribute(strName), out n)) return true;
                lstWarn.Add(e.Name + " " + strName + "='" + e.GetAttribute(strName) + "' 을 읽지 못함 — 기본값");
                return false;
            }
            private static bool OjwText_AttrF(XmlElement e, string strName, List<string> lstWarn, out float f)
            {
                f = 0f;
                if (!e.HasAttribute(strName)) return false;
                if (OjwText_S2F(e.GetAttribute(strName), out f)) return true;
                lstWarn.Add(e.Name + " " + strName + "='" + e.GetAttribute(strName) + "' 을 읽지 못함 — 기본값");
                return false;
            }
            // "01.09.00" → 10900 (바이너리 읽기와 같은 셈 — 숫자만 이어 붙인다)
            private static int OjwText_VersionNumber(string strVersion)
            {
                int n = 0;
                foreach (char c in strVersion ?? "") if (c >= '0' && c <= '9') n = n * 10 + (c - '0');
                return n;
            }
        }
    }
}
