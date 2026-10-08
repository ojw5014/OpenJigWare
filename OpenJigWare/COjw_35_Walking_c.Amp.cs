using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace OpenJigWare
{
    public partial class Ojw
    {
        /// <summary>
        /// COjwWalking_c — AMP-RL 참조 모션 제작 계층.
        ///   UrdfModel   : URDF 파싱 + 전체 링크 FK (시뮬 자산과 같은 기구학)
        ///   LegChain/IK : 롤-피치-피치-피치-롤 다리에 대한 평발 구속 IK (미지수 3, 발목은 항등식) — 정확 FK 기반 DLS 수렴
        ///   Rig         : 엔진 프레임(발 목표 mm + 채널 도) → URDF 관절값(rad). 채널 부호는 FK 프로브로 자동 결정
        ///   AmpExporter : 반복 사이클 → 관절공간 주기 3차 스플라인 재샘플 → 지지발 고정 루트 역산 → npz / csv / json
        ///   Npz         : numpy .npz(무압축 zip + .npy) 읽기/쓰기 — 외부 의존 없음
        ///   SelfTest    : 골든·FK 규약(기존 참조 npz 대조)·IK 왕복·평발·연속성·시간 측정
        /// 좌표 규약: 세계/베이스 x=전진, y=+왼쪽, z=+위 (m, rad). 엔진축 X=측방(+왼쪽), Y=수직(+위), Z=전진 (mm).
        /// </summary>
        public sealed partial class COjwWalking_c
        {
            #region 수학 (double, 할당 최소)
            public struct V3
            {
                public double X, Y, Z;
                public V3(double x, double y, double z) { X = x; Y = y; Z = z; }
                public static V3 operator +(V3 a, V3 b) { return new V3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
                public static V3 operator -(V3 a, V3 b) { return new V3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
                public static V3 operator *(V3 a, double s) { return new V3(a.X * s, a.Y * s, a.Z * s); }
                public double Len() { return Math.Sqrt(X * X + Y * Y + Z * Z); }
                public static double Dot(V3 a, V3 b) { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }
                public override string ToString() { return String.Format(CultureInfo.InvariantCulture, "({0:F4},{1:F4},{2:F4})", X, Y, Z); }
            }
            /// <summary>쿼터니언 (w,x,y,z) — Isaac Lab MotionLoader 의 wxyz 규약과 같다.</summary>
            public struct Q4
            {
                public double W, X, Y, Z;
                public Q4(double w, double x, double y, double z) { W = w; X = x; Y = y; Z = z; }
                public static Q4 Identity { get { return new Q4(1, 0, 0, 0); } }
                public static Q4 AxisAngle(V3 axis, double ang)
                {
                    double n = axis.Len(); if (n < 1e-12) return Identity;
                    double s = Math.Sin(ang * 0.5) / n;
                    return new Q4(Math.Cos(ang * 0.5), axis.X * s, axis.Y * s, axis.Z * s);
                }
                public static Q4 Rpy(double r, double p, double y) { return Q4.AxisAngle(new V3(0, 0, 1), y) * Q4.AxisAngle(new V3(0, 1, 0), p) * Q4.AxisAngle(new V3(1, 0, 0), r); }
                public static Q4 operator *(Q4 a, Q4 b)
                {
                    return new Q4(
                        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
                        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W);
                }
                public Q4 Conj() { return new Q4(W, -X, -Y, -Z); }
                public Q4 Normalized() { double n = Math.Sqrt(W * W + X * X + Y * Y + Z * Z); if (n < 1e-15) return Identity; return new Q4(W / n, X / n, Y / n, Z / n); }
                public V3 Rotate(V3 v)
                {
                    // v' = q v q*
                    double tx = 2 * (Y * v.Z - Z * v.Y), ty = 2 * (Z * v.X - X * v.Z), tz = 2 * (X * v.Y - Y * v.X);
                    return new V3(v.X + W * tx + (Y * tz - Z * ty), v.Y + W * ty + (Z * tx - X * tz), v.Z + W * tz + (X * ty - Y * tx));
                }
                public V3 AxisZ() { return Rotate(new V3(0, 0, 1)); }
                public V3 AxisX() { return Rotate(new V3(1, 0, 0)); }
                public double Yaw() { return Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Y * Y + Z * Z)); }
                /// <summary>이 회전에서 요 성분만 남긴 것.</summary>
                public Q4 YawOnly() { return Q4.AxisAngle(new V3(0, 0, 1), Yaw()); }
                public override string ToString() { return String.Format(CultureInfo.InvariantCulture, "({0:F4},{1:F4},{2:F4},{3:F4})", W, X, Y, Z); }
            }
            public struct Pose
            {
                public Q4 R; public V3 T;
                public Pose(Q4 r, V3 t) { R = r; T = t; }
                public static Pose Identity { get { return new Pose(Q4.Identity, new V3(0, 0, 0)); } }
                public V3 Apply(V3 p) { return R.Rotate(p) + T; }
                public static Pose operator *(Pose a, Pose b) { return new Pose(a.R * b.R, a.T + a.R.Rotate(b.T)); }
                public Pose Inverse() { Q4 c = R.Conj(); return new Pose(c, c.Rotate(T) * -1.0); }
            }
            #endregion

            #region URDF 모델 + FK
            public sealed class UrdfJoint
            {
                public string Name, Parent, Child, Type;
                public V3 Xyz, Axis;
                public Q4 Rot;                 // origin rpy
                public double Lower = double.NaN, Upper = double.NaN;
                public int Dof = -1;           // revolute/continuous 만 번호를 갖는다
                public bool IsRevolute { get { return Type == "revolute" || Type == "continuous"; } }
            }

            /// <summary>URDF 를 읽어 링크 트리와 FK 를 제공한다. 외부 의존 없음(System.Xml).</summary>
            public sealed class UrdfModel
            {
                public string RobotName = "";
                public readonly List<string> Links = new List<string>();
                public readonly List<UrdfJoint> Joints = new List<UrdfJoint>();
                public readonly List<string> DofNames = new List<string>();
                private readonly Dictionary<string, UrdfJoint> m_parentJoint = new Dictionary<string, UrdfJoint>();
                private readonly Dictionary<string, UrdfJoint> m_jointByName = new Dictionary<string, UrdfJoint>();
                private readonly Dictionary<string, int> m_linkIndex = new Dictionary<string, int>();
                private readonly Dictionary<string, int> m_dofIndex = new Dictionary<string, int>();
                private string[] m_order;       // 토폴로지 순서(부모 먼저)
                public string BaseLink = "";

                public static UrdfModel Load(string path)
                {
                    UrdfModel m = new UrdfModel();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    XmlElement robot = doc.DocumentElement;
                    m.RobotName = robot.GetAttribute("name");
                    foreach (XmlNode n in robot.SelectNodes("link")) { string nm = ((XmlElement)n).GetAttribute("name"); m.m_linkIndex[nm] = m.Links.Count; m.Links.Add(nm); }
                    foreach (XmlNode n in robot.SelectNodes("joint"))
                    {
                        XmlElement e = (XmlElement)n;
                        UrdfJoint j = new UrdfJoint();
                        j.Name = e.GetAttribute("name"); j.Type = e.GetAttribute("type");
                        j.Parent = ((XmlElement)e.SelectSingleNode("parent")).GetAttribute("link");
                        j.Child = ((XmlElement)e.SelectSingleNode("child")).GetAttribute("link");
                        XmlElement o = e.SelectSingleNode("origin") as XmlElement;
                        double[] xyz = ParseVec(o != null ? o.GetAttribute("xyz") : "", 3), rpy = ParseVec(o != null ? o.GetAttribute("rpy") : "", 3);
                        j.Xyz = new V3(xyz[0], xyz[1], xyz[2]); j.Rot = Q4.Rpy(rpy[0], rpy[1], rpy[2]);
                        XmlElement a = e.SelectSingleNode("axis") as XmlElement;
                        double[] ax = ParseVec(a != null ? a.GetAttribute("xyz") : "1 0 0", 3);
                        j.Axis = new V3(ax[0], ax[1], ax[2]);
                        XmlElement lim = e.SelectSingleNode("limit") as XmlElement;
                        if (lim != null) { j.Lower = ParseD(lim.GetAttribute("lower"), double.NaN); j.Upper = ParseD(lim.GetAttribute("upper"), double.NaN); }
                        if (j.IsRevolute) { j.Dof = m.DofNames.Count; m.m_dofIndex[j.Name] = j.Dof; m.DofNames.Add(j.Name); }
                        m.Joints.Add(j); m.m_jointByName[j.Name] = j; m.m_parentJoint[j.Child] = j;
                    }
                    foreach (string l in m.Links) if (!m.m_parentJoint.ContainsKey(l)) { m.BaseLink = l; break; }
                    // 토폴로지 순서
                    List<string> order = new List<string>(); HashSet<string> done = new HashSet<string>();
                    order.Add(m.BaseLink); done.Add(m.BaseLink);
                    bool progress = true;
                    while (progress && order.Count < m.Links.Count)
                    {
                        progress = false;
                        foreach (UrdfJoint j in m.Joints) if (!done.Contains(j.Child) && done.Contains(j.Parent)) { order.Add(j.Child); done.Add(j.Child); progress = true; }
                    }
                    m.m_order = order.ToArray();
                    return m;
                }
                private static double[] ParseVec(string s, int n)
                {
                    double[] r = new double[n];
                    if (string.IsNullOrEmpty(s)) return r;
                    string[] p = s.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < n && i < p.Length; i++) r[i] = ParseD(p[i], 0);
                    return r;
                }
                private static double ParseD(string s, double def) { double v; return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : def; }

                public int DofCount { get { return DofNames.Count; } }
                public int DofIndex(string jointName) { int i; return m_dofIndex.TryGetValue(jointName, out i) ? i : -1; }
                public UrdfJoint Joint(string name) { UrdfJoint j; return m_jointByName.TryGetValue(name, out j) ? j : null; }
                public UrdfJoint ParentJoint(string link) { UrdfJoint j; return m_parentJoint.TryGetValue(link, out j) ? j : null; }
                public int LinkIndex(string link) { int i; return m_linkIndex.TryGetValue(link, out i) ? i : -1; }
                public bool HasLink(string link) { return m_linkIndex.ContainsKey(link); }

                /// <summary>베이스에서 link 까지의 회전 관절 이름(부모→자식 순).</summary>
                public List<string> ChainTo(string link)
                {
                    List<string> chain = new List<string>();
                    string cur = link;
                    while (true) { UrdfJoint j = ParentJoint(cur); if (j == null) break; if (j.IsRevolute) chain.Add(j.Name); cur = j.Parent; }
                    chain.Reverse();
                    return chain;
                }

                /// <summary>전 링크 FK. poses 길이 = Links.Count (링크 인덱스 순). q 는 Dof 순.</summary>
                public void Fk(double[] q, Pose[] poses)
                {
                    poses[m_linkIndex[BaseLink]] = Pose.Identity;
                    for (int i = 1; i < m_order.Length; i++)
                    {
                        string link = m_order[i];
                        UrdfJoint j = m_parentJoint[link];
                        Pose parent = poses[m_linkIndex[j.Parent]];
                        Q4 rot = j.Rot;
                        if (j.Dof >= 0) rot = rot * Q4.AxisAngle(j.Axis, q[j.Dof]);
                        poses[m_linkIndex[link]] = new Pose(parent.R * rot, parent.T + parent.R.Rotate(j.Xyz));
                    }
                }
                /// <summary>한 링크만 (체인 경로만 계산).</summary>
                public Pose LinkPose(string link, double[] q)
                {
                    List<UrdfJoint> chain = new List<UrdfJoint>();
                    string cur = link;
                    while (true) { UrdfJoint j = ParentJoint(cur); if (j == null) break; chain.Add(j); cur = j.Parent; }
                    Pose p = Pose.Identity;
                    for (int i = chain.Count - 1; i >= 0; i--)
                    {
                        UrdfJoint j = chain[i];
                        Q4 rot = j.Rot; if (j.Dof >= 0) rot = rot * Q4.AxisAngle(j.Axis, q[j.Dof]);
                        p = new Pose(p.R * rot, p.T + p.R.Rotate(j.Xyz));
                    }
                    return p;
                }
            }
            #endregion

            #region 다리 체인 + 평발 구속 IK
            /// <summary>롤-피치-피치-피치-롤(힙롤, 힙피치, 무릎, 발목피치, 발목롤) 다리. 발 = 발목롤 관절의 자식 링크.</summary>
            public sealed class LegChain
            {
                public string Name, FootLink;
                public string[] JointNames = new string[5];
                public int[] Dof = new int[5];
                public double[] AxisSign = new double[5];   // 롤: axis.x 부호, 피치: axis.y 부호
                public V3 HomeFoot;                          // 영자세 발 링크 원점(베이스 좌표)
                public double Side;                          // +1 왼쪽(y>0), -1 오른쪽
                public bool Analytic;                        // 패턴 일치 여부
                public double KneeMin = double.NaN, KneeMax = double.NaN;
                internal double[] seedSign = null;           // 해석 시드의 관절 부호(첫 풀이에서 결정)
                internal double[] lastQ = new double[5];
                internal bool hasLast = false;
                public int Iterations;                       // 마지막 IK 반복 수
                public double LastError;                     // 마지막 IK 위치 오차(m)
            }

            /// <summary>발목롤 관절 이름(예: "T10")으로 다리 체인을 만든다.</summary>
            public static LegChain BuildLegChain(UrdfModel m, string ankleRollJoint, string name)
            {
                UrdfJoint aj = m.Joint(ankleRollJoint);
                if (aj == null) throw new ArgumentException("joint not found: " + ankleRollJoint);
                LegChain leg = new LegChain(); leg.Name = name; leg.FootLink = aj.Child;
                List<string> chain = m.ChainTo(aj.Child);
                if (chain.Count < 5) throw new ArgumentException("leg chain too short: " + ankleRollJoint);
                // 마지막 5개
                for (int i = 0; i < 5; i++) { leg.JointNames[i] = chain[chain.Count - 5 + i]; leg.Dof[i] = m.DofIndex(leg.JointNames[i]); }
                UrdfJoint[] js = new UrdfJoint[5]; for (int i = 0; i < 5; i++) js[i] = m.Joint(leg.JointNames[i]);
                bool ok = IsRoll(js[0]) && IsPitch(js[1]) && IsPitch(js[2]) && IsPitch(js[3]) && IsRoll(js[4]);
                leg.Analytic = ok;
                leg.AxisSign[0] = Math.Sign(js[0].Axis.X); leg.AxisSign[1] = Math.Sign(js[1].Axis.Y); leg.AxisSign[2] = Math.Sign(js[2].Axis.Y);
                leg.AxisSign[3] = Math.Sign(js[3].Axis.Y); leg.AxisSign[4] = Math.Sign(js[4].Axis.X);
                leg.KneeMin = js[2].Lower; leg.KneeMax = js[2].Upper;
                double[] q0 = new double[m.DofCount];
                leg.HomeFoot = m.LinkPose(leg.FootLink, q0).T;
                leg.Side = (leg.HomeFoot.Y >= 0) ? 1.0 : -1.0;
                return leg;
            }
            private static bool IsRoll(UrdfJoint j) { return Math.Abs(j.Axis.X) > 0.9 && Math.Abs(j.Axis.Y) < 0.1 && Math.Abs(j.Axis.Z) < 0.1; }
            private static bool IsPitch(UrdfJoint j) { return Math.Abs(j.Axis.Y) > 0.9 && Math.Abs(j.Axis.X) < 0.1 && Math.Abs(j.Axis.Z) < 0.1; }

            /// <summary>
            /// 평발 구속 IK. 미지수 = (힙롤, 힙피치, 무릎). 발목피치 = -(a1 q1 + a2 q2)/a3, 발목롤 = -(a0 q0)/a4 (발바닥이 베이스 수평면과 평행).
            /// 정확 FK + 수치 야코비안 DLS. 반환: 수렴 여부(오차 &lt; tol).
            /// </summary>
            public static bool SolveLegIk(UrdfModel m, LegChain leg, V3 target, double[] q, int maxIter, double tolM)
            {
                double[] u = new double[3];
                if (leg.hasLast) { u[0] = leg.lastQ[0]; u[1] = leg.lastQ[1]; u[2] = leg.lastQ[2]; }
                else SeedAnalytic(m, leg, target, q, u);
                double err = 0; int it;
                double lambda = 1e-4;
                for (it = 0; it < maxIter; it++)
                {
                    ApplyLeg(leg, u, q);
                    V3 p = m.LinkPose(leg.FootLink, q).T;
                    V3 e = target - p; err = e.Len();
                    if (err < tolM) break;
                    // 수치 야코비안 3x3
                    double[,] J = new double[3, 3]; double h = 1e-5;
                    for (int k = 0; k < 3; k++)
                    {
                        double save = u[k]; u[k] = save + h; ApplyLeg(leg, u, q);
                        V3 pp = m.LinkPose(leg.FootLink, q).T; u[k] = save;
                        J[0, k] = (pp.X - p.X) / h; J[1, k] = (pp.Y - p.Y) / h; J[2, k] = (pp.Z - p.Z) / h;
                    }
                    // (J^T J + λI) du = J^T e
                    double[,] A = new double[3, 3]; double[] b = new double[3];
                    for (int r = 0; r < 3; r++) { for (int c = 0; c < 3; c++) { double s = 0; for (int k = 0; k < 3; k++) s += J[k, r] * J[k, c]; A[r, c] = s + ((r == c) ? lambda : 0); } double t = 0; for (int k = 0; k < 3; k++) t += J[k, r] * (k == 0 ? e.X : (k == 1 ? e.Y : e.Z)); b[r] = t; }
                    double[] du = Solve3(A, b);
                    double step = 1.0; double maxd = Math.Max(Math.Abs(du[0]), Math.Max(Math.Abs(du[1]), Math.Abs(du[2])));
                    if (maxd > 0.5) step = 0.5 / maxd;
                    u[0] += du[0] * step; u[1] += du[1] * step; u[2] += du[2] * step;
                }
                ApplyLeg(leg, u, q);
                leg.Iterations = it; leg.LastError = err;
                bool ok = err < tolM * 10;
                if (ok) { leg.lastQ[0] = u[0]; leg.lastQ[1] = u[1]; leg.lastQ[2] = u[2]; leg.hasLast = true; }
                else leg.hasLast = false;
                return err < tolM;
            }
            private static void ApplyLeg(LegChain leg, double[] u, double[] q)
            {
                q[leg.Dof[0]] = u[0]; q[leg.Dof[1]] = u[1]; q[leg.Dof[2]] = u[2];
                q[leg.Dof[3]] = -(leg.AxisSign[1] * u[1] + leg.AxisSign[2] * u[2]) / leg.AxisSign[3];
                q[leg.Dof[4]] = -(leg.AxisSign[0] * u[0]) / leg.AxisSign[4];
            }
            private static double[] Solve3(double[,] A, double[] b)
            {
                double[,] M = new double[3, 4];
                for (int r = 0; r < 3; r++) { for (int c = 0; c < 3; c++) M[r, c] = A[r, c]; M[r, 3] = b[r]; }
                for (int c = 0; c < 3; c++)
                {
                    int piv = c; for (int r = c + 1; r < 3; r++) if (Math.Abs(M[r, c]) > Math.Abs(M[piv, c])) piv = r;
                    if (piv != c) for (int k = 0; k < 4; k++) { double t = M[c, k]; M[c, k] = M[piv, k]; M[piv, k] = t; }
                    double d = M[c, c]; if (Math.Abs(d) < 1e-18) d = 1e-18;
                    for (int r = 0; r < 3; r++) if (r != c) { double f = M[r, c] / d; for (int k = c; k < 4; k++) M[r, k] -= f * M[c, k]; }
                }
                return new double[] { M[0, 3] / M[0, 0], M[1, 3] / M[1, 1], M[2, 3] / M[2, 2] };
            }
            /// <summary>해석 시드: 롤 = atan2(측방, 수직), 평면 2링크 (L1, L2 는 URDF 원점 거리). 관절 부호 조합은 FK 오차 최소로 결정.</summary>
            private static void SeedAnalytic(UrdfModel m, LegChain leg, V3 target, double[] q, double[] u)
            {
                double[] q0 = new double[q.Length]; Array.Copy(q, q0, q.Length);
                for (int i = 0; i < 5; i++) q0[leg.Dof[i]] = 0;
                V3 hip = m.LinkPose(m.Joint(leg.JointNames[1]).Child, q0).T;      // 힙피치 관절 원점(자식 링크 원점)
                V3 hipRoll = m.LinkPose(m.Joint(leg.JointNames[0]).Child, q0).T;
                V3 knee = m.LinkPose(m.Joint(leg.JointNames[2]).Child, q0).T;
                V3 ank = m.LinkPose(m.Joint(leg.JointNames[3]).Child, q0).T;
                V3 foot = m.LinkPose(leg.FootLink, q0).T;
                double L1 = (knee - hip).Len(), L2 = (ank - knee).Len(), L3 = (foot - ank).Len();
                V3 d = target - hipRoll;
                double roll = Math.Atan2(d.Y, -d.Z);
                double hEff = Math.Sqrt(d.Y * d.Y + d.Z * d.Z) - (hip - hipRoll).Len() - L3;
                double x = d.X;
                double D = Math.Sqrt(x * x + hEff * hEff);
                double maxD = L1 + L2 - 1e-6; if (D > maxD) D = maxD; if (D < 1e-6) D = 1e-6;
                double kneeG = Math.PI - Math.Acos(Clamp((L1 * L1 + L2 * L2 - D * D) / (2 * L1 * L2), -1, 1));
                double psi = Math.Acos(Clamp((L1 * L1 + D * D - L2 * L2) / (2 * L1 * D), -1, 1));
                double phi = Math.Atan2(-x, hEff);
                double hipG = psi - phi;
                if (leg.seedSign == null)
                {
                    // 4가지 부호 조합 중 FK 오차 최소 + 무릎 한계 안
                    double best = double.MaxValue; double[] bestS = new double[] { 1, 1, 1 };
                    double[] sr = new double[] { 1, -1 };
                    foreach (double s0 in sr) foreach (double s1 in sr) foreach (double s2 in sr)
                    {
                        double[] uu = new double[] { roll * s0 * leg.AxisSign[0], hipG * s1, kneeG * s2 };
                        ApplyLeg(leg, uu, q0);
                        double kq = q0[leg.Dof[2]];
                        if (!double.IsNaN(leg.KneeMin) && (kq < leg.KneeMin - 0.05 || kq > leg.KneeMax + 0.05)) continue;
                        double e = (m.LinkPose(leg.FootLink, q0).T - target).Len();
                        if (e < best) { best = e; bestS = new double[] { s0, s1, s2 }; }
                    }
                    leg.seedSign = bestS;
                }
                u[0] = roll * leg.seedSign[0] * leg.AxisSign[0]; u[1] = hipG * leg.seedSign[1]; u[2] = kneeG * leg.seedSign[2];
            }
            private static double Clamp(double v, double lo, double hi) { return v < lo ? lo : (v > hi ? hi : v); }
            #endregion

            #region Rig — 엔진 프레임 → URDF 관절
            public sealed class RigConfig
            {
                public string UrdfPath = "";
                /// <summary>내보낼 바디 이름 (npz body_names). 순서: 베이스, 오른발, 왼발. v3 USD 는 link_15/link_21.</summary>
                public string ExportBaseName = "base_link", ExportFootNameR = "link_15", ExportFootNameL = "link_21";
                /// <summary>발 링크 원점에서 밑창까지(m). 발 프레임 -z 방향.</summary>
                public double FootSoleOffset = 0.029;
                /// <summary>엔진 +X(측방) 를 세계 +y(왼쪽)로 보낼 때 +1.</summary>
                public double LateralSign = 1.0;
                public bool ApplyHipSpread = true, ApplyAnkleSway = false, ApplyAnkleTilt = true, ApplyHipTilt = true, ApplyArms = true, ApplyWaist = true, ApplyExtraCommand = true, ApplyNeck = true;
                /// <summary>NaN 이면 엔진의 고관절 숙이기 값을 쓴다(도). 값을 주면 덮어쓴다.</summary>
                public double HipTiltOverrideDeg = double.NaN;
                /// <summary>true: 지지발 평발 구속으로 루트 자세를 역산(물리 정합). false: 루트 자세 = 단위(요만 유지), 기존 ref_to_amp 와 동일.</summary>
                public bool RootFromStanceFoot = true;
                // 의미 채널 부호 덮어쓰기(0 = FK 프로브 자동). +1/-1 로 주면 그 값을 쓴다.
                public int SignHipSpread = 0, SignAnkleSway = 0, SignAnkleTilt = 0, SignHipTilt = 0, SignArmUp = 0, SignArmWing = 0, SignElbow = 0, SignWaist = 0;
                public int IkMaxIter = 40; public double IkTolM = 1e-6;
            }

            public sealed class Rig
            {
                public readonly RigConfig Cfg;
                public readonly UrdfModel Model;
                public LegChain LegR, LegL;
                public string ArmUpJointR, ArmUpJointL, ArmWingJointR, ArmWingJointL, ElbowJointR, ElbowJointL, WaistJoint, NeckJoint, HipTiltJointR, HipTiltJointL, AnkleTiltJointR, AnkleTiltJointL, HipSpreadJointR, HipSpreadJointL, AnkleSwayJointR, AnkleSwayJointL;
                public double sHipSpreadR = 1, sHipSpreadL = 1, sAnkleSwayR = 1, sAnkleSwayL = 1, sAnkleTiltR = 1, sAnkleTiltL = 1, sHipTiltR = 1, sHipTiltL = 1, sArmUpR = 1, sArmUpL = 1, sArmWingR = 1, sArmWingL = 1, sElbowR = 1, sElbowL = 1, sWaist = 1;
                public string FootLinkR, FootLinkL;
                public readonly StringBuilder Log = new StringBuilder();
                private readonly Pose[] m_poses;
                private readonly double[] m_qTmp;
                private readonly List<KeyValuePair<string, double>> m_extra = new List<KeyValuePair<string, double>>();
                private string m_extraSrc = null;

                public Rig(RigConfig cfg, COjwWalking_c engine)
                {
                    Cfg = cfg;
                    Model = UrdfModel.Load(cfg.UrdfPath);
                    m_poses = new Pose[Model.Links.Count];
                    m_qTmp = new double[Model.DofCount];
                    // 관절 이름은 엔진 파라미터의 모터 ID 에서 온다 (T{id})
                    string ankR = J(engine, P_ANKROLL_R), ankL = J(engine, P_ANKROLL_L);
                    if (ankR == null || ankL == null) throw new InvalidOperationException("발목(W) ID 가 비어 있어 다리 체인을 만들 수 없다.");
                    LegR = BuildLegChain(Model, ankR, "R"); LegL = BuildLegChain(Model, ankL, "L");
                    FootLinkR = LegR.FootLink; FootLinkL = LegL.FootLink;
                    HipSpreadJointR = J(engine, P_HIPROLL_R); HipSpreadJointL = J(engine, P_HIPROLL_L);
                    AnkleSwayJointR = J(engine, P_ANKROLL_R); AnkleSwayJointL = J(engine, P_ANKROLL_L);
                    AnkleTiltJointR = J(engine, P_ANKTILT_R); AnkleTiltJointL = J(engine, P_ANKTILT_L);
                    HipTiltJointR = J(engine, P_HIPTILT_R); HipTiltJointL = J(engine, P_HIPTILT_L);
                    ArmUpJointR = J(engine, P_ARMUP_R); ArmUpJointL = J(engine, P_ARMUP_L);
                    ArmWingJointR = J(engine, P_ARMW_R); ArmWingJointL = J(engine, P_ARMW_L);
                    WaistJoint = J(engine, P_WAIST); NeckJoint = J(engine, P_NECK);
                    Log.AppendFormat("urdf={0} dof={1} links={2} base={3}\r\n", Model.RobotName, Model.DofCount, Model.Links.Count, Model.BaseLink);
                    Log.AppendFormat("legR={0} [{1}] analytic={2} home={3}\r\n", LegR.FootLink, string.Join(",", LegR.JointNames), LegR.Analytic, LegR.HomeFoot);
                    Log.AppendFormat("legL={0} [{1}] analytic={2} home={3}\r\n", LegL.FootLink, string.Join(",", LegL.JointNames), LegL.Analytic, LegL.HomeFoot);
                    ResolveSigns();
                }
                private static string J(COjwWalking_c e, int idx)
                {
                    string s = e.GetData_Str(1, idx);
                    if (string.IsNullOrEmpty(s)) return null;
                    s = s.Trim();
                    return (s.Length > 0 && (s[0] == 'T' || s[0] == 't')) ? "T" + s.Substring(1) : "T" + s;
                }

                private double Probe(string joint, double delta, Func<double[], double> metric)
                {
                    int d = Model.DofIndex(joint); if (d < 0) return 0;
                    double[] q = new double[Model.DofCount];
                    double m0 = metric(q); q[d] = delta; double m1 = metric(q);
                    return m1 - m0;
                }
                private int Pick(int over, double probeDelta) { if (over != 0) return over; return probeDelta > 0 ? 1 : -1; }
                private void ResolveSigns()
                {
                    string handR = EndOfChainFromJoint(ElbowJointR ?? ArmUpJointR), handL = EndOfChainFromJoint(ElbowJointL ?? ArmUpJointL);
                    // 팔꿈치 관절: 덧붙임 명령어의 T{id} 중 팔 체인에 속하는 것 (없으면 팔Up 체인의 끝 관절)
                    ElbowJointR = FindElbow(ArmUpJointR); ElbowJointL = FindElbow(ArmUpJointL);
                    handR = EndOfChainFromJoint(ArmUpJointR); handL = EndOfChainFromJoint(ArmUpJointL);
                    double dq = 0.1;
                    // 다리 벌리기: +δ → 발이 바깥(미드라인에서 멀어짐)
                    if (HipSpreadJointR != null) sHipSpreadR = Pick(Cfg.SignHipSpread, Probe(HipSpreadJointR, dq, qq => Model.LinkPose(FootLinkR, qq).T.Y * LegR.Side));
                    if (HipSpreadJointL != null) sHipSpreadL = Pick(Cfg.SignHipSpread, Probe(HipSpreadJointL, dq, qq => Model.LinkPose(FootLinkL, qq).T.Y * LegL.Side));
                    // 발목 Sway: +δ → 발바닥 법선이 미드라인 쪽으로 기움 (= 발이 고정이면 몸이 지지발 바깥쪽으로 기움)
                    if (AnkleSwayJointR != null) sAnkleSwayR = Pick(Cfg.SignAnkleSway, Probe(AnkleSwayJointR, dq, qq => Model.LinkPose(FootLinkR, qq).R.AxisZ().Y * (-LegR.Side)));
                    if (AnkleSwayJointL != null) sAnkleSwayL = Pick(Cfg.SignAnkleSway, Probe(AnkleSwayJointL, dq, qq => Model.LinkPose(FootLinkL, qq).R.AxisZ().Y * (-LegL.Side)));
                    // 발목 Tilt: +δ → 발끝 아래 (법선이 +x 로 기움)
                    if (AnkleTiltJointR != null) sAnkleTiltR = Pick(Cfg.SignAnkleTilt, Probe(AnkleTiltJointR, dq, qq => Model.LinkPose(FootLinkR, qq).R.AxisZ().X));
                    if (AnkleTiltJointL != null) sAnkleTiltL = Pick(Cfg.SignAnkleTilt, Probe(AnkleTiltJointL, dq, qq => Model.LinkPose(FootLinkL, qq).R.AxisZ().X));
                    // 고관절 숙이기: +δ → 발이 뒤로(-x) = 몸통이 앞으로
                    if (HipTiltJointR != null) sHipTiltR = Pick(Cfg.SignHipTilt, -Probe(HipTiltJointR, dq, qq => Model.LinkPose(FootLinkR, qq).T.X));
                    if (HipTiltJointL != null) sHipTiltL = Pick(Cfg.SignHipTilt, -Probe(HipTiltJointL, dq, qq => Model.LinkPose(FootLinkL, qq).T.X));
                    // 팔Up: +δ → 손이 앞(+x). 팔W: +δ → 손이 바깥(|y| 증가). 팔꿈치: +δ → 손이 위(+z)
                    if (ArmUpJointR != null && handR != null) sArmUpR = Pick(Cfg.SignArmUp, Probe(ArmUpJointR, dq, qq => Model.LinkPose(handR, qq).T.X));
                    if (ArmUpJointL != null && handL != null) sArmUpL = Pick(Cfg.SignArmUp, Probe(ArmUpJointL, dq, qq => Model.LinkPose(handL, qq).T.X));
                    if (ArmWingJointR != null && handR != null) sArmWingR = Pick(Cfg.SignArmWing, Probe(ArmWingJointR, dq, qq => Math.Abs(Model.LinkPose(handR, qq).T.Y)));
                    if (ArmWingJointL != null && handL != null) sArmWingL = Pick(Cfg.SignArmWing, Probe(ArmWingJointL, dq, qq => Math.Abs(Model.LinkPose(handL, qq).T.Y)));
                    if (ElbowJointR != null && handR != null) sElbowR = Pick(Cfg.SignElbow, Probe(ElbowJointR, dq, qq => Model.LinkPose(handR, qq).T.Z));
                    if (ElbowJointL != null && handL != null) sElbowL = Pick(Cfg.SignElbow, Probe(ElbowJointL, dq, qq => Model.LinkPose(handL, qq).T.Z));
                    // 허리: +δ → 오른손이 앞으로(+x) = 반시계(좌회전)
                    if (WaistJoint != null && handR != null) sWaist = Pick(Cfg.SignWaist, Probe(WaistJoint, dq, qq => Model.LinkPose(handR, qq).T.X));
                    Log.AppendFormat("signs: hipSpread R{0} L{1} | ankleSway R{2} L{3} | ankleTilt R{4} L{5} | hipTilt R{6} L{7} | armUp R{8} L{9} | armWing R{10} L{11} | elbow R{12}({13}) L{14}({15}) | waist {16}\r\n",
                        sHipSpreadR, sHipSpreadL, sAnkleSwayR, sAnkleSwayL, sAnkleTiltR, sAnkleTiltL, sHipTiltR, sHipTiltL, sArmUpR, sArmUpL, sArmWingR, sArmWingL, sElbowR, ElbowJointR, sElbowL, ElbowJointL, sWaist);
                }
                private string EndOfChainFromJoint(string joint)
                {
                    if (joint == null) return null;
                    UrdfJoint j = Model.Joint(joint); if (j == null) return null;
                    string cur = j.Child;
                    while (true)
                    {
                        UrdfJoint next = null;
                        foreach (UrdfJoint jj in Model.Joints) if (jj.Parent == cur) { next = jj; break; }
                        if (next == null) return cur;
                        cur = next.Child;
                    }
                }
                private string FindElbow(string shoulderJoint)
                {
                    if (shoulderJoint == null) return null;
                    UrdfJoint j = Model.Joint(shoulderJoint); if (j == null) return null;
                    string cur = j.Child; string last = null;
                    while (true)
                    {
                        UrdfJoint next = null;
                        foreach (UrdfJoint jj in Model.Joints) if (jj.Parent == cur) { next = jj; break; }
                        if (next == null) break;
                        if (next.IsRevolute) last = next.Name;
                        cur = next.Child;
                    }
                    return last;
                }

                /// <summary>덧붙임 명령어("T13 70 T14 70")를 (관절, 도) 목록으로.</summary>
                private void ParseExtra(string src)
                {
                    if (src == m_extraSrc) return;
                    m_extraSrc = src; m_extra.Clear();
                    if (string.IsNullOrEmpty(src)) return;
                    string[] tok = src.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i + 1 < tok.Length; i += 2)
                    {
                        string t = tok[i]; double v;
                        if ((t[0] == 'T' || t[0] == 't' || t[0] == 'P' || t[0] == 'p') && double.TryParse(tok[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            m_extra.Add(new KeyValuePair<string, double>("T" + t.Substring(1), v));
                    }
                }

                /// <summary>
                /// 프레임 → 관절값(rad, Model.DofNames 순). 발 목표 = 영자세 발 + 엔진 오프셋. 반환: 양다리 IK 수렴 여부.
                /// </summary>
                public bool Solve(Frame f, string extraCommand, double[] q)
                {
                    Array.Clear(q, 0, q.Length);
                    bool ok = true;
                    V3 tR = LegR.HomeFoot + new V3(f.FootR_Z * 0.001, Cfg.LateralSign * f.FootR_X * 0.001, f.FootR_Y * 0.001);
                    V3 tL = LegL.HomeFoot + new V3(f.FootL_Z * 0.001, Cfg.LateralSign * f.FootL_X * 0.001, f.FootL_Y * 0.001);
                    ok &= SolveLegIk(Model, LegR, tR, q, Cfg.IkMaxIter, Cfg.IkTolM);
                    ok &= SolveLegIk(Model, LegL, tL, q, Cfg.IkMaxIter, Cfg.IkTolM);
                    double D2R = Math.PI / 180.0;
                    if (Cfg.ApplyHipSpread) { Add(q, HipSpreadJointR, sHipSpreadR * f.HipSpreadR * D2R); Add(q, HipSpreadJointL, sHipSpreadL * f.HipSpreadL * D2R); }
                    if (Cfg.ApplyAnkleSway) { Add(q, AnkleSwayJointR, sAnkleSwayR * f.AnkleSwayR * D2R); Add(q, AnkleSwayJointL, sAnkleSwayL * f.AnkleSwayL * D2R); }
                    if (Cfg.ApplyAnkleTilt) { Add(q, AnkleTiltJointR, sAnkleTiltR * f.AnkleTiltR * D2R); Add(q, AnkleTiltJointL, sAnkleTiltL * f.AnkleTiltL * D2R); }
                    if (Cfg.ApplyHipTilt)
                    {
                        double tilt = double.IsNaN(Cfg.HipTiltOverrideDeg) ? f.HipTilt : Cfg.HipTiltOverrideDeg;
                        Add(q, HipTiltJointR, sHipTiltR * tilt * D2R); Add(q, HipTiltJointL, sHipTiltL * tilt * D2R);
                    }
                    if (Cfg.ApplyArms)
                    {
                        Set(q, ArmUpJointR, sArmUpR * f.ArmUpR * D2R); Set(q, ArmUpJointL, sArmUpL * f.ArmUpL * D2R);
                        Set(q, ArmWingJointR, sArmWingR * f.ArmWingR * D2R); Set(q, ArmWingJointL, sArmWingL * f.ArmWingL * D2R);
                    }
                    if (Cfg.ApplyWaist) Set(q, WaistJoint, sWaist * f.Waist * D2R);
                    if (Cfg.ApplyNeck) Set(q, NeckJoint, f.Neck * D2R);
                    if (Cfg.ApplyExtraCommand)
                    {
                        ParseExtra(extraCommand);
                        foreach (KeyValuePair<string, double> kv in m_extra)
                        {
                            double s = 1;
                            if (kv.Key == ElbowJointR) s = sElbowR; else if (kv.Key == ElbowJointL) s = sElbowL;
                            Set(q, kv.Key, s * kv.Value * D2R);
                        }
                    }
                    return ok;
                }
                private void Add(double[] q, string joint, double v) { if (joint == null) return; int d = Model.DofIndex(joint); if (d >= 0) q[d] += v; }
                private void Set(double[] q, string joint, double v) { if (joint == null) return; int d = Model.DofIndex(joint); if (d >= 0) q[d] = v; }

                public void Fk(double[] q, Pose[] poses) { Model.Fk(q, poses); }
                public Pose[] NewPoseBuffer() { return new Pose[Model.Links.Count]; }
                /// <summary>발바닥 기울기(도): 발 링크 z축과 베이스 z축 사이 각.</summary>
                public double SoleTiltDeg(Pose foot) { double c = Clamp(foot.R.AxisZ().Z, -1, 1); return Math.Acos(c) * 180.0 / Math.PI; }
            }
            #endregion

            #region 주기 3차 스플라인 (균일 간격, 주기)
            /// <summary>균일 간격 주기 3차 스플라인. 사용자 검증 레시피(gen_ref_gait.resample) 와 같은 수식: 순환 삼중대각 M 계.</summary>
            public sealed class PeriodicCubic
            {
                private readonly double[] y, M; private readonly double h; private readonly int n;
                public PeriodicCubic(double[] knots, double spacing)
                {
                    y = (double[])knots.Clone(); n = y.Length; h = spacing; M = new double[n];
                    // A M = r,  A[i,i-1]=1, A[i,i]=4, A[i,i+1]=1 (순환)
                    double[,] A = new double[n, n + 1];
                    for (int i = 0; i < n; i++)
                    {
                        A[i, (i - 1 + n) % n] += 1; A[i, i] += 4; A[i, (i + 1) % n] += 1;
                        A[i, n] = 6.0 / (h * h) * (y[(i - 1 + n) % n] - 2.0 * y[i] + y[(i + 1) % n]);
                    }
                    // 가우스 소거 (n ≤ 수백)
                    for (int c = 0; c < n; c++)
                    {
                        int piv = c; for (int r = c + 1; r < n; r++) if (Math.Abs(A[r, c]) > Math.Abs(A[piv, c])) piv = r;
                        if (piv != c) for (int k = 0; k <= n; k++) { double t = A[c, k]; A[c, k] = A[piv, k]; A[piv, k] = t; }
                        double d = A[c, c];
                        for (int r = 0; r < n; r++) if (r != c && A[r, c] != 0) { double f = A[r, c] / d; for (int k = c; k <= n; k++) A[r, k] -= f * A[c, k]; }
                    }
                    for (int i = 0; i < n; i++) M[i] = A[i, n] / A[i, i];
                }
                /// <summary>t 는 [0, n·h) 를 주기로 순환. 값과 1계 도함수.</summary>
                public void Eval(double t, out double val, out double der)
                {
                    double period = n * h;
                    t = t % period; if (t < 0) t += period;
                    int i0 = (int)Math.Floor(t / h); if (i0 >= n) i0 = n - 1;
                    double u = t - i0 * h;
                    int i1 = (i0 + 1) % n;
                    double y0 = y[i0], y1 = y[i1], m0 = M[i0], m1 = M[i1];
                    double b = (y1 - y0) / h - h * (2.0 * m0 + m1) / 6.0;
                    val = y0 + b * u + 0.5 * m0 * u * u + (m1 - m0) / (6.0 * h) * u * u * u;
                    der = b + m0 * u + (m1 - m0) / (2.0 * h) * u * u;
                }
            }
            #endregion

            #region AmpExporter — 반복 사이클 → AMP 참조 모션
            public sealed class ExportOptions
            {
                public double Fps = 100;
                public int Cycles = 4;
                public bool Spline = true;                  // false: 선형 보간(옛 거동, 가속도 계단 발생)
                /// <summary>스플라인 매듭 간격(엔진 스텝 단위). 1 = 모든 스텝. 3 이면 23 ms 스텝에서 69 ms 매듭(사용자 검증 레시피 70 ms 와 유사) — 모서리 가속도 완화. 프레임 수가 나누어떨어져야 한다.</summary>
                public int KnotStride = 1;
                public string OutDir = "";
                public string Name = "walk_c";
                public bool WriteNpz = true, WriteCsv = true, WriteJson = true, WriteDebugCsv = true;
                /// <summary>CSV 열 순서(관절 이름). null 이면 T1..T{n} 숫자순.</summary>
                public string[] CsvColumns = null;
            }

            public sealed class ClipResult
            {
                public string Name; public int Frames; public double Fps, Duration, Speed, RootZMean, RootZMin, RootZMax, RootPitchDegMean, MaxJointVel, MaxJointAcc, FootClearance, MaxSoleTiltDeg, MaxStanceSoleTiltDeg, LoopGapRad, IkMaxErrMm;
                public int StanceSwitches; public bool IkOk;
                public string NpzPath, CsvPath, JsonPath;
                public readonly List<string> Warnings = new List<string>();
                public override string ToString()
                {
                    return String.Format(CultureInfo.InvariantCulture,
                        "{0}: {1} frames @{2:F2}fps {3:F3}s | speed {4:F4} m/s | rootZ {5:F4} (min {6:F4} max {7:F4}) pitch {8:F1}deg | qd max {9:F2} rad/s qdd max {10:F0} rad/s2 | clearance {11:F1}mm | sole tilt max {12:F2}deg (stance {18:F2}) | loop gap {13:F5} rad | ik err max {14:F3}mm ok={15} | stance switches {16}{17}",
                        Name, Frames, Fps, Duration, Speed, RootZMean, RootZMin, RootZMax, RootPitchDegMean, MaxJointVel, MaxJointAcc, FootClearance, MaxSoleTiltDeg, LoopGapRad, IkMaxErrMm, IkOk, StanceSwitches,
                        Warnings.Count > 0 ? " | WARN: " + string.Join("; ", Warnings.ToArray()) : "", MaxStanceSoleTiltDeg);
                }
            }

            /// <summary>
            /// 반복 모드 1사이클(2 게이트)을 관절공간에서 풀고, 주기 스플라인으로 fps 재샘플, 지지발 고정으로 루트를 역산해 AMP 파일을 쓴다.
            /// </summary>
            public static ClipResult ExportRepeatCycle(COjwWalking_c engine, Rig rig, ExportOptions opt)
            {
                ClipResult res = new ClipResult(); res.Name = opt.Name; res.Fps = opt.Fps;
                int n = engine.FrameCount;
                double stepSec = engine.FrameMs(1, 2) / 1000.0;
                if (n <= 0 || stepSec <= 0) throw new InvalidOperationException("frame count / speed time invalid");
                double period = n * stepSec;
                int dof = rig.Model.DofCount;
                // 1) 엔진 프레임 → 관절 (정수 step)
                double[][] Q = new double[n][];
                Frame f = new Frame();
                string extra = engine.GetData_Str(1, P_EXTRA);
                bool ikOk = true; double ikErr = 0;
                rig.LegR.hasLast = false; rig.LegL.hasLast = false;
                for (int k = 0; k < n; k++)
                {
                    engine.Evaluate(1, k + 1, f);
                    Q[k] = new double[dof];
                    bool ok = rig.Solve(f, extra, Q[k]);
                    ikOk &= ok; ikErr = Math.Max(ikErr, Math.Max(rig.LegR.LastError, rig.LegL.LastError));
                }
                res.IkOk = ikOk; res.IkMaxErrMm = ikErr * 1000.0;
                if (!ikOk) res.Warnings.Add("IK not converged on some frames");
                // 2) 스플라인/선형 → 샘플. 마지막 프레임이 정확히 사이클 경계(첫 프레임과 같은 위상)에 오도록 dt 를 미세 조정한다.
                double total = period * opt.Cycles;
                int frames = (int)Math.Round(total * opt.Fps) + 1;
                double dt = total / (frames - 1);
                double fpsActual = 1.0 / dt;
                res.Fps = fpsActual;
                double[][] q = new double[frames][], qd = new double[frames][];
                PeriodicCubic[] sp = new PeriodicCubic[dof];
                int ks = (opt.KnotStride >= 1 && n % opt.KnotStride == 0) ? opt.KnotStride : 1;
                if (opt.Spline)
                {
                    int nk = n / ks;
                    for (int d = 0; d < dof; d++) { double[] knots = new double[nk]; for (int k = 0; k < nk; k++) knots[k] = Q[k * ks][d]; sp[d] = new PeriodicCubic(knots, stepSec * ks); }
                }
                for (int j = 0; j < frames; j++)
                {
                    double t = j * dt; q[j] = new double[dof]; qd[j] = new double[dof];
                    for (int d = 0; d < dof; d++)
                    {
                        if (opt.Spline) { double v, dv; sp[d].Eval(t, out v, out dv); q[j][d] = v; qd[j][d] = dv; }
                        else
                        {
                            double tt = t % period; int i0 = (int)Math.Floor(tt / stepSec); if (i0 >= n) i0 = n - 1; double u = (tt - i0 * stepSec) / stepSec; int i1 = (i0 + 1) % n;
                            q[j][d] = Q[i0][d] * (1 - u) + Q[i1][d] * u; qd[j][d] = (Q[i1][d] - Q[i0][d]) / stepSec;
                        }
                    }
                }
                // 지지발 힌트: 출력 프레임 시각 → 엔진 step → 스윙 다리 (양발지지는 직전 유지)
                int[] hint = new int[frames]; Frame hf = new Frame(); int last = 2;
                for (int j = 0; j < frames; j++)
                {
                    double tt = (j * dt) % period; int k = (int)Math.Floor(tt / stepSec) + 1; if (k > n) k = n; if (k < 1) k = 1;
                    engine.Evaluate(1, k, hf);
                    if (hf.SwingLeg == 1) last = 2; else if (hf.SwingLeg == 2) last = 1;
                    hint[j] = last;
                }
                return FinishClip(rig, q, qd, frames, dt, opt, res, period, n, stepSec, ks, engine.ToStrings(1), hint);
            }

            /// <summary>관절 궤적(q, qd) → FK, 지지발 고정 루트 역산, 통계, 파일 쓰기. ExportRepeatCycle 과 반걸음 내보내기가 공유한다.</summary>
            private static ClipResult FinishClip(Rig rig, double[][] q, double[][] qd, int frames, double dt, ExportOptions opt, ClipResult res, double period, int n, double stepSec, int knotStride, string[] paramsRepeat, int[] stanceHint)
            {
                int dof = rig.Model.DofCount;
                // 3) FK + 루트 역산
                int nb = 3;
                string[] bodyNames = new string[] { rig.Cfg.ExportBaseName, rig.Cfg.ExportFootNameR, rig.Cfg.ExportFootNameL };
                int iBase = rig.Model.LinkIndex(rig.Model.BaseLink), iFR = rig.Model.LinkIndex(rig.FootLinkR), iFL = rig.Model.LinkIndex(rig.FootLinkL);
                Pose[] poses = rig.NewPoseBuffer();
                V3[,] bp = new V3[frames, nb]; Q4[,] br = new Q4[frames, nb];
                Pose[] rawFootR = new Pose[frames], rawFootL = new Pose[frames];
                for (int j = 0; j < frames; j++) { rig.Fk(q[j], poses); rawFootR[j] = poses[iFR]; rawFootL[j] = poses[iFL]; }
                V3 soleOff = new V3(0, 0, -rig.Cfg.FootSoleOffset);
                int stancePrev = -1; int switches = 0;
                V3 anchorXY = new V3(0, 0, 0);   // 지지발 밑창 세계 xy
                Pose root = Pose.Identity;
                double zsum = 0, zmin = double.MaxValue, zmax = double.MinValue, pitchSum = 0, maxTilt = 0, maxStanceTilt = 0;
                double[] rawSoleZ = new double[frames];
                for (int j = 0; j < frames; j++)
                {
                    V3 soleR = rawFootR[j].Apply(soleOff), soleL = rawFootL[j].Apply(soleOff);
                    int stance;
                    if (stanceHint != null && j < stanceHint.Length && stanceHint[j] > 0) stance = (stanceHint[j] == 1) ? 0 : 1;   // 생성기가 아는 지지발 (계단에서는 "낮은 발" 규칙이 틀린다)
                    else
                    {
                        stance = (soleR.Z <= soleL.Z) ? 0 : 1;
                        if (stancePrev >= 0 && stance != stancePrev && Math.Abs(soleR.Z - soleL.Z) < 1e-4) stance = stancePrev;   // 동률이면 유지
                    }
                    Pose stanceFoot = (stance == 0) ? rawFootR[j] : rawFootL[j];
                    // 루트 자세: 평발 구속이면 지지발 회전의 역(요 제외)
                    Q4 rootR = Q4.Identity;
                    if (rig.Cfg.RootFromStanceFoot)
                    {
                        Q4 fr = stanceFoot.R;                        // 베이스 기준 발 회전
                        Q4 yaw = fr.YawOnly();
                        rootR = (yaw * fr.Conj()).Normalized();      // rootR * fr = yaw-only  → 발이 수평
                    }
                    V3 soleRootFrame = rootR.Rotate(stanceFoot.Apply(soleOff));
                    if (stancePrev < 0) { anchorXY = new V3(0, 0, 0); }
                    else if (stance != stancePrev)
                    {
                        // 지지 전환: 새 지지발의 현재 세계 위치(높이 포함)를 앵커로 승계 — 계단이면 지면 높이가 그대로 이어진다
                        Pose newFoot = (stance == 0) ? rawFootR[j - 1] : rawFootL[j - 1];
                        V3 prevSoleWorld = root.Apply(newFoot.Apply(soleOff));
                        anchorXY = new V3(prevSoleWorld.X, prevSoleWorld.Y, prevSoleWorld.Z);
                        switches++;
                    }
                    V3 rootT = new V3(anchorXY.X - soleRootFrame.X, anchorXY.Y - soleRootFrame.Y, anchorXY.Z - soleRootFrame.Z);
                    root = new Pose(rootR, rootT);
                    stancePrev = stance;
                    // 바디
                    Pose pB = root, pR = root * rawFootR[j], pL = root * rawFootL[j];
                    bp[j, 0] = pB.T; br[j, 0] = pB.R; bp[j, 1] = pR.T; br[j, 1] = pR.R; bp[j, 2] = pL.T; br[j, 2] = pL.R;
                    zsum += pB.T.Z; if (pB.T.Z < zmin) zmin = pB.T.Z; if (pB.T.Z > zmax) zmax = pB.T.Z;
                    double pitch = Math.Asin(Clamp(-pB.R.AxisX().Z, -1, 1)) * 180.0 / Math.PI; pitchSum += pitch;
                    double tilt = Math.Max(rig.SoleTiltDeg(pR), rig.SoleTiltDeg(pL)); if (tilt > maxTilt) maxTilt = tilt;
                    double stTilt = rig.SoleTiltDeg((stance == 0) ? pR : pL); if (stTilt > maxStanceTilt) maxStanceTilt = stTilt;
                    rawSoleZ[j] = Math.Max(pR.Apply(soleOff).Z, pL.Apply(soleOff).Z);
                }
                // 4) 속도 (중앙차분, 가장자리 한쪽차분 — np.gradient 와 같은 규약)
                V3[,] blv = new V3[frames, nb], bav = new V3[frames, nb];
                for (int b = 0; b < nb; b++)
                {
                    for (int j = 0; j < frames; j++)
                    {
                        int j0 = Math.Max(0, j - 1), j1 = Math.Min(frames - 1, j + 1); double span = (j1 - j0) * dt; if (span <= 0) span = dt;
                        blv[j, b] = (bp[j1, b] - bp[j0, b]) * (1.0 / span);
                        Q4 dq = (br[j1, b] * br[j0, b].Conj()).Normalized();
                        double sn = Math.Sqrt(dq.X * dq.X + dq.Y * dq.Y + dq.Z * dq.Z);
                        if (sn > 1e-9) { double ang = 2.0 * Math.Atan2(sn, Math.Abs(dq.W)); double sg = dq.W >= 0 ? 1 : -1; bav[j, b] = new V3(dq.X / sn * ang / span * sg, dq.Y / sn * ang / span * sg, dq.Z / sn * ang / span * sg); }
                        else bav[j, b] = new V3(0, 0, 0);
                    }
                }
                // 5) 통계
                double maxV = 0, maxA = 0;
                for (int j = 0; j < frames; j++) for (int d = 0; d < dof; d++) { maxV = Math.Max(maxV, Math.Abs(qd[j][d])); if (j > 0) maxA = Math.Max(maxA, Math.Abs((qd[j][d] - qd[j - 1][d]) / dt)); }
                double clearance = 0; for (int j = 0; j < frames; j++) clearance = Math.Max(clearance, rawSoleZ[j]);
                double loopGap = 0; for (int d = 0; d < dof; d++) loopGap = Math.Max(loopGap, Math.Abs(q[frames - 1][d] - q[0][d]));
                res.Frames = frames; res.Duration = (frames - 1) * dt; res.Speed = (bp[frames - 1, 0].X - bp[0, 0].X) / res.Duration;
                res.RootZMean = zsum / frames; res.RootZMin = zmin; res.RootZMax = zmax; res.RootPitchDegMean = pitchSum / frames;
                res.MaxJointVel = maxV; res.MaxJointAcc = maxA; res.FootClearance = clearance * 1000.0; res.MaxSoleTiltDeg = maxTilt; res.MaxStanceSoleTiltDeg = maxStanceTilt; res.LoopGapRad = loopGap; res.StanceSwitches = switches;
                if (maxA > 150) res.Warnings.Add(String.Format(CultureInfo.InvariantCulture, "joint accel {0:F0} rad/s2 exceeds servo band (~48-139)", maxA));
                if (period > 0 && loopGap > 0.02) res.Warnings.Add("loop not closed");
                // 6) 파일
                if (!string.IsNullOrEmpty(opt.OutDir)) Directory.CreateDirectory(opt.OutDir);
                string[] dofNames = rig.Model.DofNames.ToArray();
                if (opt.WriteNpz)
                {
                    res.NpzPath = Path.Combine(opt.OutDir, opt.Name + ".npz");
                    NpzWriter w = new NpzWriter();
                    w.AddScalarF8("fps", 1.0 / dt);
                    w.AddStrings("dof_names", dofNames);
                    w.AddStrings("body_names", bodyNames);
                    w.AddF4("dof_positions", frames, dof, (i, d) => (float)q[i][d]);
                    w.AddF4("dof_velocities", frames, dof, (i, d) => (float)qd[i][d]);
                    w.AddF4_3(  "body_positions", frames, nb, 3, (i, b, k) => (float)Comp(bp[i, b], k));
                    w.AddF4_3("body_rotations", frames, nb, 4, (i, b, k) => (float)CompQ(br[i, b], k));
                    w.AddF4_3("body_linear_velocities", frames, nb, 3, (i, b, k) => (float)Comp(blv[i, b], k));
                    w.AddF4_3("body_angular_velocities", frames, nb, 3, (i, b, k) => (float)Comp(bav[i, b], k));
                    w.Save(res.NpzPath);
                }
                if (opt.WriteCsv)
                {
                    res.CsvPath = Path.Combine(opt.OutDir, opt.Name + ".csv");
                    string[] cols = opt.CsvColumns ?? SortedTNames(dofNames);
                    using (StreamWriter sw = new StreamWriter(res.CsvPath, false, new UTF8Encoding(false)))
                    {
                        sw.Write("time"); foreach (string c in cols) { sw.Write(","); sw.Write(c); } sw.Write("\n");
                        int[] idx = new int[cols.Length]; for (int c = 0; c < cols.Length; c++) idx[c] = rig.Model.DofIndex(cols[c]);
                        for (int j = 0; j < frames; j++)
                        {
                            sw.Write((j * dt).ToString("F3", CultureInfo.InvariantCulture));
                            for (int c = 0; c < cols.Length; c++) { sw.Write(","); sw.Write((idx[c] >= 0 ? q[j][idx[c]] : 0.0).ToString("F5", CultureInfo.InvariantCulture)); }
                            sw.Write("\n");
                        }
                    }
                }
                if (opt.WriteDebugCsv)
                {
                    string p = Path.Combine(opt.OutDir, opt.Name + "_bodies.csv");
                    using (StreamWriter sw = new StreamWriter(p, false, new UTF8Encoding(false)))
                    {
                        sw.Write("time,root_x,root_y,root_z,root_qw,root_qx,root_qy,root_qz,footR_x,footR_y,footR_z,footL_x,footL_y,footL_z,phase\n");
                        for (int j = 0; j < frames; j++)
                        {
                            double ph = ((j * dt) % period) / period;
                            sw.Write(String.Format(CultureInfo.InvariantCulture, "{0:F3},{1:F5},{2:F5},{3:F5},{4:F5},{5:F5},{6:F5},{7:F5},{8:F5},{9:F5},{10:F5},{11:F5},{12:F5},{13:F5},{14:F4}\n",
                                j * dt, bp[j, 0].X, bp[j, 0].Y, bp[j, 0].Z, br[j, 0].W, br[j, 0].X, br[j, 0].Y, br[j, 0].Z, bp[j, 1].X, bp[j, 1].Y, bp[j, 1].Z, bp[j, 2].X, bp[j, 2].Y, bp[j, 2].Z, ph));
                        }
                    }
                }
                if (opt.WriteJson)
                {
                    res.JsonPath = Path.Combine(opt.OutDir, opt.Name + ".json");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("{\n");
                    sb.AppendFormat(CultureInfo.InvariantCulture, "  \"name\": \"{0}\", \"fps\": {1}, \"frames\": {2}, \"cycles\": {3}, \"cycle_period_s\": {4:F5}, \"engine_steps_per_cycle\": {5}, \"step_ms\": {6},\n", opt.Name, opt.Fps, frames, opt.Cycles, period, n, stepSec * 1000.0);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "  \"speed_mps\": {0:F5}, \"root_z_mean\": {1:F5}, \"root_pitch_deg_mean\": {2:F3}, \"max_joint_vel\": {3:F4}, \"max_joint_acc\": {4:F2}, \"foot_clearance_mm\": {5:F2}, \"max_sole_tilt_deg\": {6:F3}, \"ik_ok\": {7}, \"ik_max_err_mm\": {8:F4},\n", res.Speed, res.RootZMean, res.RootPitchDegMean, res.MaxJointVel, res.MaxJointAcc, res.FootClearance, res.MaxSoleTiltDeg, res.IkOk ? "true" : "false", res.IkMaxErrMm);
                    sb.AppendFormat("  \"spline\": {0}, \"knot_stride\": {4}, \"root_from_stance_foot\": {1}, \"lateral_sign\": {2}, \"foot_sole_offset\": {3}, \"max_stance_sole_tilt_deg\": {5},\n", opt.Spline ? "true" : "false", rig.Cfg.RootFromStanceFoot ? "true" : "false", rig.Cfg.LateralSign, rig.Cfg.FootSoleOffset.ToString(CultureInfo.InvariantCulture), knotStride, res.MaxStanceSoleTiltDeg.ToString("F3", CultureInfo.InvariantCulture));
                    sb.Append("  \"dof_names\": [\"").Append(string.Join("\",\"", dofNames)).Append("\"],\n");
                    sb.Append("  \"body_names\": [\"").Append(string.Join("\",\"", bodyNames)).Append("\"],\n");
                    sb.Append("  \"params_repeat\": [");
                    string[] pr = paramsRepeat ?? new string[0];
                    for (int i = 0; i < pr.Length; i++) { if (i > 0) sb.Append(","); sb.Append("\"").Append(pr[i].Replace("\"", "'")).Append("\""); }
                    sb.Append("],\n  \"rig_log\": \"").Append(rig.Log.ToString().Replace("\\", "/").Replace("\"", "'").Replace("\r\n", " | ")).Append("\"\n}\n");
                    File.WriteAllText(res.JsonPath, sb.ToString(), new UTF8Encoding(false));
                }
                return res;
            }
            private static double Comp(V3 v, int k) { return k == 0 ? v.X : (k == 1 ? v.Y : v.Z); }
            private static double CompQ(Q4 q, int k) { return k == 0 ? q.W : (k == 1 ? q.X : (k == 2 ? q.Y : q.Z)); }
            private static string[] SortedTNames(string[] names)
            {
                List<string> l = new List<string>(names);
                l.Sort(delegate(string a, string b)
                {
                    int ia, ib; bool ta = int.TryParse(a.TrimStart('T', 't'), out ia), tb = int.TryParse(b.TrimStart('T', 't'), out ib);
                    if (ta && tb) return ia.CompareTo(ib); if (ta) return -1; if (tb) return 1; return string.CompareOrdinal(a, b);
                });
                return l.ToArray();
            }

            public enum LadderMode { Stride = 0, StepTime = 1, Auto = 2 }

            /// <summary>
            /// 목표 속도 사다리. Stride: 전진량(보폭)만 조정. StepTime: 속도시간(ms/step)만 조정(관절 속도 증가).
            /// Auto(기본): 보폭을 키우다 IK 가 실패(다리 도달 한계, 웅크림 30 mm 에서 한 발 ±85 mm)하면 그 직전 보폭으로 되돌리고 나머지는 스텝 시간으로 맞춘다.
            /// 반복 모드 값만 바꾸며 끝나면 원래 값으로 되돌린다.
            /// </summary>
            public static List<ClipResult> ExportSpeedLadder(COjwWalking_c engine, Rig rig, ExportOptions opt, double[] targetSpeeds, out string log)
            {
                return ExportSpeedLadder(engine, rig, opt, targetSpeeds, LadderMode.Auto, out log);
            }
            public static List<ClipResult> ExportSpeedLadder(COjwWalking_c engine, Rig rig, ExportOptions opt, double[] targetSpeeds, LadderMode mode, out string log)
            {
                List<ClipResult> results = new List<ClipResult>();
                StringBuilder sb = new StringBuilder();
                string baseName = opt.Name; string strideOrig = engine.GetData_Str(1, P_STRIDE); string stepOrig = engine.GetData_Str(1, P_SPEED);
                double stride0 = ParseF(strideOrig), step0 = ParseF(stepOrig);
                foreach (double target in targetSpeeds)
                {
                    double stride = stride0, step = step0, lastOkStride = stride0;
                    engine.SetData(1, P_STRIDE, strideOrig); engine.SetData(1, P_SPEED, stepOrig);
                    ClipResult r = null; bool useStep = (mode == LadderMode.StepTime);
                    for (int iter = 0; iter < 6; iter++)
                    {
                        ExportOptions o2 = new ExportOptions(); o2.Fps = opt.Fps; o2.Cycles = opt.Cycles; o2.Spline = opt.Spline; o2.KnotStride = opt.KnotStride; o2.OutDir = opt.OutDir; o2.WriteNpz = opt.WriteNpz; o2.WriteCsv = opt.WriteCsv; o2.WriteJson = opt.WriteJson; o2.WriteDebugCsv = opt.WriteDebugCsv; o2.CsvColumns = opt.CsvColumns;
                        o2.Name = String.Format(CultureInfo.InvariantCulture, "{0}_v{1:F2}", baseName, target);
                        r = ExportRepeatCycle(engine, rig, o2);
                        sb.AppendFormat(CultureInfo.InvariantCulture, "target {0:F3}: stride {1:F1} step {2:F1} ms -> speed {3:F4} ik={4}\r\n", target, stride, step, r.Speed, r.IkOk);
                        if (r.Speed <= 1e-6) break;
                        if (mode == LadderMode.Auto && !useStep && !r.IkOk)
                        {
                            // 도달 한계: 마지막 성공 보폭으로 복귀하고 스텝 시간으로 전환
                            stride = lastOkStride; useStep = true;
                            engine.SetData(1, P_STRIDE, stride.ToString("F2", CultureInfo.InvariantCulture));
                            sb.AppendFormat(CultureInfo.InvariantCulture, "  reach limit -> stride back to {0:F1}, adjusting step time\r\n", stride);
                            continue;
                        }
                        if (r.IkOk) lastOkStride = stride;
                        if (Math.Abs(r.Speed - target) < 0.002 && r.IkOk) break;
                        double ratio = target / r.Speed;
                        if (!useStep)
                        {
                            stride = stride * ratio;
                            engine.SetData(1, P_STRIDE, stride.ToString("F2", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            step = step / ratio; if (step < 5) step = 5;
                            engine.SetData(1, P_SPEED, step.ToString("F1", CultureInfo.InvariantCulture));
                        }
                    }
                    results.Add(r);
                }
                engine.SetData(1, P_STRIDE, strideOrig); engine.SetData(1, P_SPEED, stepOrig);
                log = sb.ToString();
                return results;
            }
            #endregion

            #region Npz — numpy .npz 쓰기/읽기 (무압축 zip)
            public sealed class NpzWriter
            {
                private readonly List<KeyValuePair<string, byte[]>> m_entries = new List<KeyValuePair<string, byte[]>>();
                public delegate float F2(int i, int j);
                public delegate float F3(int i, int j, int k);
                public void AddScalarF8(string name, double v) { byte[] data = BitConverter.GetBytes(v); m_entries.Add(new KeyValuePair<string, byte[]>(name + ".npy", Npy("<f8", "()", data))); }
                public void AddStrings(string name, string[] s)
                {
                    int maxLen = 1; foreach (string x in s) maxLen = Math.Max(maxLen, x.Length);
                    byte[] data = new byte[s.Length * maxLen * 4];
                    for (int i = 0; i < s.Length; i++) for (int c = 0; c < s[i].Length; c++) { int code = s[i][c]; int o = (i * maxLen + c) * 4; data[o] = (byte)(code & 0xff); data[o + 1] = (byte)((code >> 8) & 0xff); }
                    m_entries.Add(new KeyValuePair<string, byte[]>(name + ".npy", Npy("<U" + maxLen, "(" + s.Length + ",)", data)));
                }
                public void AddF4(string name, int n, int m, F2 f)
                {
                    byte[] data = new byte[n * m * 4]; int o = 0;
                    for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) { byte[] b = BitConverter.GetBytes(f(i, j)); data[o++] = b[0]; data[o++] = b[1]; data[o++] = b[2]; data[o++] = b[3]; }
                    m_entries.Add(new KeyValuePair<string, byte[]>(name + ".npy", Npy("<f4", "(" + n + ", " + m + ")", data)));
                }
                public void AddF4_3(string name, int n, int m, int k, F3 f)
                {
                    byte[] data = new byte[n * m * k * 4]; int o = 0;
                    for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) for (int l = 0; l < k; l++) { byte[] b = BitConverter.GetBytes(f(i, j, l)); data[o++] = b[0]; data[o++] = b[1]; data[o++] = b[2]; data[o++] = b[3]; }
                    m_entries.Add(new KeyValuePair<string, byte[]>(name + ".npy", Npy("<f4", "(" + n + ", " + m + ", " + k + ")", data)));
                }
                private static byte[] Npy(string descr, string shape, byte[] data)
                {
                    string header = "{'descr': '" + descr + "', 'fortran_order': False, 'shape': " + shape + ", }";
                    int baseLen = 10 + header.Length + 1;
                    int pad = (64 - (baseLen % 64)) % 64;
                    header = header + new string(' ', pad) + "\n";
                    byte[] h = Encoding.ASCII.GetBytes(header);
                    byte[] outb = new byte[10 + h.Length + data.Length];
                    outb[0] = 0x93; outb[1] = (byte)'N'; outb[2] = (byte)'U'; outb[3] = (byte)'M'; outb[4] = (byte)'P'; outb[5] = (byte)'Y'; outb[6] = 1; outb[7] = 0;
                    outb[8] = (byte)(h.Length & 0xff); outb[9] = (byte)((h.Length >> 8) & 0xff);
                    Buffer.BlockCopy(h, 0, outb, 10, h.Length); Buffer.BlockCopy(data, 0, outb, 10 + h.Length, data.Length);
                    return outb;
                }
                public void Save(string path)
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        List<long> offsets = new List<long>(); List<uint> crcs = new List<uint>();
                        foreach (KeyValuePair<string, byte[]> e in m_entries)
                        {
                            byte[] name = Encoding.ASCII.GetBytes(e.Key); uint crc = Crc32(e.Value);
                            offsets.Add(fs.Position); crcs.Add(crc);
                            bw.Write(0x04034b50u); bw.Write((ushort)20); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0x21);
                            bw.Write(crc); bw.Write((uint)e.Value.Length); bw.Write((uint)e.Value.Length); bw.Write((ushort)name.Length); bw.Write((ushort)0);
                            bw.Write(name); bw.Write(e.Value);
                        }
                        long cdStart = fs.Position;
                        for (int i = 0; i < m_entries.Count; i++)
                        {
                            byte[] name = Encoding.ASCII.GetBytes(m_entries[i].Key);
                            bw.Write(0x02014b50u); bw.Write((ushort)20); bw.Write((ushort)20); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0x21);
                            bw.Write(crcs[i]); bw.Write((uint)m_entries[i].Value.Length); bw.Write((uint)m_entries[i].Value.Length);
                            bw.Write((ushort)name.Length); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write(0u); bw.Write((uint)offsets[i]);
                            bw.Write(name);
                        }
                        long cdEnd = fs.Position;
                        bw.Write(0x06054b50u); bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)m_entries.Count); bw.Write((ushort)m_entries.Count); bw.Write((uint)(cdEnd - cdStart)); bw.Write((uint)cdStart); bw.Write((ushort)0);
                    }
                }
                private static uint[] s_crcTable;
                internal static uint Crc32(byte[] data)
                {
                    if (s_crcTable == null)
                    {
                        uint[] t = new uint[256];
                        for (uint i = 0; i < 256; i++) { uint c = i; for (int k = 0; k < 8; k++) c = ((c & 1) != 0) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1); t[i] = c; }
                        s_crcTable = t;
                    }
                    uint crc = 0xFFFFFFFFu;
                    foreach (byte b in data) crc = s_crcTable[(crc ^ b) & 0xff] ^ (crc >> 8);
                    return crc ^ 0xFFFFFFFFu;
                }
            }

            /// <summary>npz 읽기(무압축/deflate). 배열은 double[] 로 평탄화, shape 별도. 문자열 배열은 string[].</summary>
            public sealed class NpzReader
            {
                public readonly Dictionary<string, double[]> Arrays = new Dictionary<string, double[]>();
                public readonly Dictionary<string, int[]> Shapes = new Dictionary<string, int[]>();
                public readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>();
                public static NpzReader Load(string path)
                {
                    NpzReader r = new NpzReader();
                    byte[] all = File.ReadAllBytes(path);
                    // EOCD
                    int eocd = -1; for (int i = all.Length - 22; i >= 0; i--) if (BitConverter.ToUInt32(all, i) == 0x06054b50u) { eocd = i; break; }
                    if (eocd < 0) throw new InvalidDataException("not a zip");
                    int count = BitConverter.ToUInt16(all, eocd + 10); int cdOff = (int)BitConverter.ToUInt32(all, eocd + 16);
                    int p = cdOff;
                    for (int e = 0; e < count; e++)
                    {
                        if (BitConverter.ToUInt32(all, p) != 0x02014b50u) throw new InvalidDataException("bad central dir");
                        int method = BitConverter.ToUInt16(all, p + 10); int csize = (int)BitConverter.ToUInt32(all, p + 20); int usize = (int)BitConverter.ToUInt32(all, p + 24);
                        int nlen = BitConverter.ToUInt16(all, p + 28), xlen = BitConverter.ToUInt16(all, p + 30), clen = BitConverter.ToUInt16(all, p + 32);
                        int loff = (int)BitConverter.ToUInt32(all, p + 42);
                        string name = Encoding.ASCII.GetString(all, p + 46, nlen);
                        p += 46 + nlen + xlen + clen;
                        int lnlen = BitConverter.ToUInt16(all, loff + 26), lxlen = BitConverter.ToUInt16(all, loff + 28);
                        int dataOff = loff + 30 + lnlen + lxlen;
                        byte[] raw;
                        if (method == 0) { raw = new byte[csize]; Buffer.BlockCopy(all, dataOff, raw, 0, csize); }
                        else if (method == 8) { using (MemoryStream ms = new MemoryStream(all, dataOff, csize)) using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress)) { raw = new byte[usize]; int got = 0; while (got < usize) { int k = ds.Read(raw, got, usize - got); if (k <= 0) break; got += k; } } }
                        else throw new InvalidDataException("unsupported zip method " + method);
                        r.ParseNpy(name.EndsWith(".npy") ? name.Substring(0, name.Length - 4) : name, raw);
                    }
                    return r;
                }
                private void ParseNpy(string key, byte[] b)
                {
                    int major = b[6]; int hlen = (major == 1) ? BitConverter.ToUInt16(b, 8) : (int)BitConverter.ToUInt32(b, 8); int hstart = (major == 1) ? 10 : 12;
                    string header = Encoding.ASCII.GetString(b, hstart, hlen);
                    string descr = Between(header, "'descr': '", "'"); string shape = Between(header, "'shape': (", ")");
                    List<int> dims = new List<int>(); foreach (string s in shape.Split(',')) { string t = s.Trim(); if (t.Length > 0) dims.Add(int.Parse(t, CultureInfo.InvariantCulture)); }
                    int total = 1; foreach (int d in dims) total *= d;
                    int off = hstart + hlen;
                    Shapes[key] = dims.ToArray();
                    if (descr.StartsWith("<f4") || descr.StartsWith("=f4")) { double[] a = new double[total]; for (int i = 0; i < total; i++) a[i] = BitConverter.ToSingle(b, off + i * 4); Arrays[key] = a; }
                    else if (descr.StartsWith("<f8") || descr.StartsWith("=f8")) { double[] a = new double[total]; for (int i = 0; i < total; i++) a[i] = BitConverter.ToDouble(b, off + i * 8); Arrays[key] = a; }
                    else if (descr.StartsWith("<U") || descr.StartsWith("=U"))
                    {
                        int n = int.Parse(descr.Substring(2), CultureInfo.InvariantCulture); string[] s = new string[total];
                        for (int i = 0; i < total; i++) { StringBuilder sb = new StringBuilder(); for (int c = 0; c < n; c++) { int code = BitConverter.ToInt32(b, off + (i * n + c) * 4); if (code == 0) break; sb.Append((char)code); } s[i] = sb.ToString(); }
                        Strings[key] = s;
                    }
                    else if (descr.StartsWith("<i4") || descr.StartsWith("<i8"))
                    {
                        bool i8 = descr.StartsWith("<i8"); double[] a = new double[total];
                        for (int i = 0; i < total; i++) a[i] = i8 ? (double)BitConverter.ToInt64(b, off + i * 8) : BitConverter.ToInt32(b, off + i * 4); Arrays[key] = a;
                    }
                }
                private static string Between(string s, string a, string b) { int i = s.IndexOf(a); if (i < 0) return ""; i += a.Length; int j = s.IndexOf(b, i); return j < 0 ? s.Substring(i) : s.Substring(i, j - i); }
            }
            #endregion

            #region SelfTest
            public sealed class SelfTestResult { public bool Pass = true; public readonly StringBuilder Log = new StringBuilder(); public void Line(string s) { Log.Append(s).Append("\r\n"); } public void Fail(string s) { Pass = false; Line("FAIL " + s); } }

            /// <summary>
            /// 자체 검증. refNpz 가 있으면 기존 참조 모션(dof_positions)으로 FK 를 돌려 발 위치(루트 상대)를 대조한다 — 이것이 sim 규약 검증이다.
            /// </summary>
            public static SelfTestResult RunSelfTest(COjwWalking_c engine, Rig rig, string refNpzPath)
            {
                SelfTestResult r = new SelfTestResult();
                // 1) 골든
                string rep; int mm = engine.GoldenCompare(out rep); r.Line(rep); if (mm != 0) r.Fail("golden mismatch " + mm);
                // 2) 엔진 프레임 요약
                int n = engine.FrameCount; Frame f = new Frame();
                r.Line(String.Format("engine: gate={0} frames/cycle={1} step={2}ms cycle={3:F3}s", engine.GateSize, n, engine.FrameMs(1, 2), n * engine.FrameMs(1, 2) / 1000.0));
                // 3) IK 왕복 + 평발
                double[] q = new double[rig.Model.DofCount]; Pose[] poses = rig.NewPoseBuffer();
                double maxErr = 0, maxTilt = 0; int notConv = 0; int iters = 0;
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                string extra = engine.GetData_Str(1, P_EXTRA);
                rig.LegR.hasLast = false; rig.LegL.hasLast = false;
                for (int k = 1; k <= n; k++)
                {
                    engine.Evaluate(1, k, f);
                    // 순수 IK(가산 채널 전부 끔)로 위치 오차와 평발을 검사한다. 가산 채널은 의도적으로 발을 움직이므로 오차 측정에서 제외.
                    bool saveTilt = rig.Cfg.ApplyAnkleTilt, saveHip = rig.Cfg.ApplyHipTilt, saveSway = rig.Cfg.ApplyAnkleSway, saveSpread = rig.Cfg.ApplyHipSpread;
                    rig.Cfg.ApplyAnkleTilt = false; rig.Cfg.ApplyHipTilt = false; rig.Cfg.ApplyAnkleSway = false; rig.Cfg.ApplyHipSpread = false;
                    bool ok = rig.Solve(f, extra, q);
                    rig.Cfg.ApplyAnkleTilt = saveTilt; rig.Cfg.ApplyHipTilt = saveHip; rig.Cfg.ApplyAnkleSway = saveSway; rig.Cfg.ApplyHipSpread = saveSpread;
                    if (!ok) notConv++;
                    iters += rig.LegR.Iterations + rig.LegL.Iterations;
                    rig.Fk(q, poses);
                    V3 tR = rig.LegR.HomeFoot + new V3(f.FootR_Z * 0.001, rig.Cfg.LateralSign * f.FootR_X * 0.001, f.FootR_Y * 0.001);
                    V3 tL = rig.LegL.HomeFoot + new V3(f.FootL_Z * 0.001, rig.Cfg.LateralSign * f.FootL_X * 0.001, f.FootL_Y * 0.001);
                    V3 pR = poses[rig.Model.LinkIndex(rig.FootLinkR)].T, pL = poses[rig.Model.LinkIndex(rig.FootLinkL)].T;
                    maxErr = Math.Max(maxErr, Math.Max((pR - tR).Len(), (pL - tL).Len()));
                    maxTilt = Math.Max(maxTilt, Math.Max(rig.SoleTiltDeg(poses[rig.Model.LinkIndex(rig.FootLinkR)]), rig.SoleTiltDeg(poses[rig.Model.LinkIndex(rig.FootLinkL)])));
                }
                sw.Stop();
                r.Line(String.Format(CultureInfo.InvariantCulture, "ik: max pos err {0:F4} mm, not converged {1}/{2}, avg iters/leg {3:F1}, {4:F1} us/frame(2 legs+FK)", maxErr * 1000, notConv, n, iters / (2.0 * n), sw.Elapsed.TotalMilliseconds * 1000.0 / n));
                r.Line(String.Format(CultureInfo.InvariantCulture, "flat-foot (pure IK): max sole tilt {0:F4} deg", maxTilt));
                if (maxErr > 1e-4) r.Fail("ik position error > 0.1 mm");
                if (maxTilt > 0.01) r.Fail("sole not flat");
                // 4) 체중이동 방향: 오른발 스윙 중 루트가 왼발(지지) 쪽(+y)으로 가는가
                {
                    double sumY = 0; int cnt = 0;
                    for (int k = 1; k <= n; k++) { engine.Evaluate(1, k, f); if (f.SwingLeg == 1) { sumY += -(rig.Cfg.LateralSign * f.FootL_X); cnt++; } }
                    // 루트 y (지지발 기준) = -(발 오프셋 y) → 왼발 지지 시 루트가 +y 로 가야 한다
                    double meanRootY = cnt > 0 ? sumY / cnt : 0;
                    r.Line(String.Format(CultureInfo.InvariantCulture, "weight shift: during right swing, root lateral offset from stance(left) foot = {0:+0.0;-0.0} mm (expect + = toward left)", meanRootY));
                    if (cnt > 0 && meanRootY < 0) r.Fail("sway direction: body moves away from stance foot — set RigConfig.LateralSign = -1");
                }
                // 5) 기존 참조 npz 와 FK 규약 대조
                if (!string.IsNullOrEmpty(refNpzPath) && File.Exists(refNpzPath))
                {
                    try
                    {
                        NpzReader np = NpzReader.Load(refNpzPath);
                        string[] dn = np.Strings["dof_names"], bn = np.Strings["body_names"];
                        int[] shp = np.Shapes["dof_positions"]; double[] dp = np.Arrays["dof_positions"]; double[] bpA = np.Arrays["body_positions"]; int[] bshp = np.Shapes["body_positions"];
                        double[] brA = np.Arrays["body_rotations"];
                        int nf = shp[0], nd = shp[1], nbod = bshp[1];
                        int iBase = Array.IndexOf(bn, rig.Cfg.ExportBaseName), iFR = Array.IndexOf(bn, rig.Cfg.ExportFootNameR), iFL = Array.IndexOf(bn, rig.Cfg.ExportFootNameL);
                        if (iBase < 0 || iFR < 0 || iFL < 0) r.Line("ref npz: body names not found (" + string.Join(",", bn) + ")");
                        else
                        {
                            int[] map = new int[nd]; for (int d = 0; d < nd; d++) map[d] = rig.Model.DofIndex(dn[d]);
                            double eMax = 0, eSum = 0, eMaxFlip = 0, eSumFlip = 0; int cnt = 0; int step = Math.Max(1, nf / 60);
                            for (int fi = 0; fi < nf; fi += step)
                            {
                                Array.Clear(q, 0, q.Length);
                                for (int d = 0; d < nd; d++) if (map[d] >= 0) q[map[d]] = dp[fi * nd + d];
                                rig.Fk(q, poses);
                                V3 rb = new V3(bpA[(fi * nbod + iBase) * 3], bpA[(fi * nbod + iBase) * 3 + 1], bpA[(fi * nbod + iBase) * 3 + 2]);
                                Q4 rq = new Q4(brA[(fi * nbod + iBase) * 4], brA[(fi * nbod + iBase) * 4 + 1], brA[(fi * nbod + iBase) * 4 + 2], brA[(fi * nbod + iBase) * 4 + 3]);
                                Q4 rinv = rq.Conj();
                                V3 fr = new V3(bpA[(fi * nbod + iFR) * 3], bpA[(fi * nbod + iFR) * 3 + 1], bpA[(fi * nbod + iFR) * 3 + 2]);
                                V3 fl = new V3(bpA[(fi * nbod + iFL) * 3], bpA[(fi * nbod + iFL) * 3 + 1], bpA[(fi * nbod + iFL) * 3 + 2]);
                                V3 relR = rinv.Rotate(fr - rb), relL = rinv.Rotate(fl - rb);
                                V3 myR = poses[rig.Model.LinkIndex(rig.FootLinkR)].T, myL = poses[rig.Model.LinkIndex(rig.FootLinkL)].T;
                                double e = Math.Max((relR - myR).Len(), (relL - myL).Len()); eMax = Math.Max(eMax, e); eSum += e; cnt++;
                                // 가설: 참조의 위치만 180° 요 회전됨 (x,y 부호 반전)
                                V3 flR = new V3(-relR.X, -relR.Y, relR.Z), flL = new V3(-relL.X, -relL.Y, relL.Z);
                                double ef = Math.Max((flR - myR).Len(), (flL - myL).Len()); eMaxFlip = Math.Max(eMaxFlip, ef); eSumFlip += ef;
                            }
                            r.Line(String.Format(CultureInfo.InvariantCulture, "ref npz FK check ({0}): feet-rel-root error max {1:F2} mm mean {2:F2} mm | after 180deg-yaw hypothesis max {3:F2} mm mean {4:F2} mm ({5} frames, sim joints -> my URDF FK)",
                                Path.GetFileName(refNpzPath), eMax * 1000, eSum / Math.Max(1, cnt) * 1000, eMaxFlip * 1000, eSumFlip / Math.Max(1, cnt) * 1000, cnt));
                            if (eMax <= 0.020) r.Line("  -> joint convention matches reference npz (residual = URDF vs USD geometry)");
                            else if (eMaxFlip <= 0.020) r.Line("  -> joints match only after flipping x,y: the reference npz positions were yaw-rotated 180deg (ref_to_amp.py '+x align') but its quaternions were rotated as xyzw while Isaac gives wxyz, so root orientation and positions disagree there. Residual = URDF(v2) vs USD(v3) geometry. Diagnostic only.");
                            else r.Line("  -> WARNING: neither hypothesis matches within 20 mm; check URDF version vs USD");
                        }
                    }
                    catch (Exception ex) { r.Line("ref npz check skipped: " + ex.Message); }
                }
                // 6) 스트리머 타이밍
                {
                    Streamer st = new Streamer(engine); st.StartRepeat();
                    int N = 5000; sw.Restart();
                    for (int i = 0; i < N; i++) st.Advance(0.04f);
                    sw.Stop();
                    r.Line(String.Format(CultureInfo.InvariantCulture, "streamer: Advance {0:F2} us/tick, cycles {1}", sw.Elapsed.TotalMilliseconds * 1000.0 / N, st.CyclesCompleted));
                    sw.Restart(); for (int i = 0; i < N; i++) engine.Evaluate(1, (i % n) + 1, f); sw.Stop();
                    r.Line(String.Format(CultureInfo.InvariantCulture, "kernel: Evaluate {0:F2} us/frame", sw.Elapsed.TotalMilliseconds * 1000.0 / N));
                }
                r.Line(r.Pass ? "SELFTEST PASS" : "SELFTEST FAIL");
                return r;
            }
            #endregion
        }
    }
}
