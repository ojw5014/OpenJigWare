using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenJigWare
{
    partial class Ojw
    {
    public class StlBoundingBox
    {
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;

        // 삼각형 정점 데이터 (9개씩: v0x,v0y,v0z, v1x,v1y,v1z, v2x,v2y,v2z)
        public List<float> Vertices = new List<float>();

        public float CenterX { get { return (MinX + MaxX) / 2; } }
        public float CenterY { get { return (MinY + MaxY) / 2; } }
        public float CenterZ { get { return (MinZ + MaxZ) / 2; } }

        /// <summary>
        /// 바운딩 박스의 6면 중심점 반환
        /// face: 0=+X, 1=-X, 2=+Y, 3=-Y, 4=+Z, 5=-Z
        /// </summary>
        public void GetFaceCenter(int face, out float x, out float y, out float z)
        {
            x = CenterX; y = CenterY; z = CenterZ;
            switch (face)
            {
                case 0: x = MaxX; break;  // +X
                case 1: x = MinX; break;  // -X
                case 2: y = MaxY; break;  // +Y
                case 3: y = MinY; break;  // -Y
                case 4: z = MaxZ; break;  // +Z
                case 5: z = MinZ; break;  // -Z
            }
        }

        public string GetFaceName(int face)
        {
            switch (face)
            {
                case 0: return "+X";
                case 1: return "-X";
                case 2: return "+Y";
                case 3: return "-Y";
                case 4: return "+Z";
                case 5: return "-Z";
                default: return "";
            }
        }

        public bool IsValid
        {
            get { return MinX <= MaxX && MinY <= MaxY && MinZ <= MaxZ; }
        }

        /// <summary>
        /// 지정된 바운딩 박스 면에 있는 삼각형 정점들의 중심(centroid) 계산.
        /// 같은 2D 평면에 존재하는 도형의 실제 기하학적 중심을 반환.
        /// face: 0=+X, 1=-X, 2=+Y, 3=-Y, 4=+Z, 5=-Z
        /// </summary>
        public bool GetFaceGeometryCentroid(int face, out float cx, out float cy, out float cz)
        {
            cx = cy = cz = 0;

            if (Vertices.Count < 9) return false;

            // 바운딩 박스 크기 기반 허용 오차 (0.1%)
            float dimX = MaxX - MinX;
            float dimY = MaxY - MinY;
            float dimZ = MaxZ - MinZ;
            float maxDim = Math.Max(dimX, Math.Max(dimY, dimZ));
            if (maxDim <= 0) return false;
            float tolerance = maxDim * 0.001f;
            if (tolerance < 0.001f) tolerance = 0.001f;

            // 면의 기준값과 축 인덱스
            float faceVal;
            int axisIdx; // 0=X, 1=Y, 2=Z
            switch (face)
            {
                case 0: faceVal = MaxX; axisIdx = 0; break;
                case 1: faceVal = MinX; axisIdx = 0; break;
                case 2: faceVal = MaxY; axisIdx = 1; break;
                case 3: faceVal = MinY; axisIdx = 1; break;
                case 4: faceVal = MaxZ; axisIdx = 2; break;
                case 5: faceVal = MinZ; axisIdx = 2; break;
                default: return false;
            }

            float sumX = 0, sumY = 0, sumZ = 0;
            int count = 0;
            int numTris = Vertices.Count / 9;

            for (int i = 0; i < numTris; i++)
            {
                int baseIdx = i * 9;

                // 3개 정점 모두 해당 면 평면 위에 있는지 확인
                bool allOnFace = true;
                for (int v = 0; v < 3 && allOnFace; v++)
                {
                    float val = Vertices[baseIdx + v * 3 + axisIdx];
                    allOnFace = Math.Abs(val - faceVal) <= tolerance;
                }

                if (allOnFace)
                {
                    for (int v = 0; v < 3; v++)
                    {
                        sumX += Vertices[baseIdx + v * 3];
                        sumY += Vertices[baseIdx + v * 3 + 1];
                        sumZ += Vertices[baseIdx + v * 3 + 2];
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                cx = sumX / count;
                cy = sumY / count;
                cz = sumZ / count;
                return true;
            }

            return false;
        }
    }

    public static class StlParser
    {
        public static StlBoundingBox Parse(string filePath)
        {
            // FileShare.ReadWrite → C3d가 파일을 잡고 있어도 읽기 가능
            byte[] data;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }

            // ASCII 형식 판별: "solid"로 시작하고 "facet" 포함
            if (data.Length > 84)
            {
                string sample = Encoding.ASCII.GetString(data, 0, Math.Min(1024, data.Length));
                if (sample.TrimStart().StartsWith("solid") && sample.Contains("facet"))
                    return ParseAscii(Encoding.ASCII.GetString(data));
            }

            return ParseBinary(data);
        }

        private static StlBoundingBox ParseBinary(byte[] data)
        {
            var bb = new StlBoundingBox();
            bb.MinX = bb.MinY = bb.MinZ = float.MaxValue;
            bb.MaxX = bb.MaxY = bb.MaxZ = float.MinValue;

            if (data.Length < 84) return bb;

            int triCount = BitConverter.ToInt32(data, 80);
            int offset = 84;

            for (int i = 0; i < triCount && offset + 50 <= data.Length; i++)
            {
                offset += 12; // 법선 벡터 건너뛰기
                for (int v = 0; v < 3; v++)
                {
                    float x = BitConverter.ToSingle(data, offset); offset += 4;
                    float y = BitConverter.ToSingle(data, offset); offset += 4;
                    float z = BitConverter.ToSingle(data, offset); offset += 4;
                    bb.Vertices.Add(x);
                    bb.Vertices.Add(y);
                    bb.Vertices.Add(z);
                    UpdateBB(bb, x, y, z);
                }
                offset += 2; // attribute byte count
            }

            return bb;
        }

        private static StlBoundingBox ParseAscii(string text)
        {
            var bb = new StlBoundingBox();
            bb.MinX = bb.MinY = bb.MinZ = float.MaxValue;
            bb.MaxX = bb.MaxY = bb.MaxZ = float.MinValue;

            string[] lines = text.Split('\n');
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (!line.StartsWith("vertex")) continue;

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    float x, y, z;
                    if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                    {
                        bb.Vertices.Add(x);
                        bb.Vertices.Add(y);
                        bb.Vertices.Add(z);
                        UpdateBB(bb, x, y, z);
                    }
                }
            }

            return bb;
        }

        /// <summary>
        /// 지정된 면의 삼각형들만 추출하여 별도 바이너리 STL 파일로 저장.
        /// 반환값: 추출된 삼각형 개수 (0이면 해당 면에 삼각형 없음)
        /// </summary>
        public static int WriteFaceStl(StlBoundingBox bbox, int face, string outputPath)
        {
            if (bbox.Vertices.Count < 9) return 0;

            float dimX = bbox.MaxX - bbox.MinX;
            float dimY = bbox.MaxY - bbox.MinY;
            float dimZ = bbox.MaxZ - bbox.MinZ;
            float maxDim = Math.Max(dimX, Math.Max(dimY, dimZ));
            if (maxDim <= 0) return 0;
            float tolerance = maxDim * 0.001f;
            if (tolerance < 0.001f) tolerance = 0.001f;

            float faceVal;
            int axisIdx;
            switch (face)
            {
                case 0: faceVal = bbox.MaxX; axisIdx = 0; break;
                case 1: faceVal = bbox.MinX; axisIdx = 0; break;
                case 2: faceVal = bbox.MaxY; axisIdx = 1; break;
                case 3: faceVal = bbox.MinY; axisIdx = 1; break;
                case 4: faceVal = bbox.MaxZ; axisIdx = 2; break;
                case 5: faceVal = bbox.MinZ; axisIdx = 2; break;
                default: return 0;
            }

            // 면 위의 삼각형 인덱스 수집
            var faceTriIndices = new List<int>();
            int numTris = bbox.Vertices.Count / 9;
            for (int i = 0; i < numTris; i++)
            {
                int baseIdx = i * 9;
                bool allOnFace = true;
                for (int v = 0; v < 3 && allOnFace; v++)
                {
                    float val = bbox.Vertices[baseIdx + v * 3 + axisIdx];
                    allOnFace = Math.Abs(val - faceVal) <= tolerance;
                }
                if (allOnFace)
                    faceTriIndices.Add(i);
            }

            if (faceTriIndices.Count == 0) return 0;

            // 바이너리 STL 쓰기
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(new byte[80]); // header
                bw.Write(faceTriIndices.Count);
                foreach (int idx in faceTriIndices)
                {
                    int baseIdx = idx * 9;
                    bw.Write(0f); bw.Write(0f); bw.Write(0f); // normal
                    for (int vi = 0; vi < 9; vi++)
                        bw.Write(bbox.Vertices[baseIdx + vi]);
                    bw.Write((ushort)0); // attribute
                }
            }
            return faceTriIndices.Count;
        }

        /// <summary>
        /// 지정된 면 위의 삼각형 인덱스 목록 반환
        /// </summary>
        public static List<int> GetFaceTriangleIndices(StlBoundingBox bbox, int face)
        {
            var result = new List<int>();
            if (bbox.Vertices.Count < 9) return result;

            float dimX = bbox.MaxX - bbox.MinX;
            float dimY = bbox.MaxY - bbox.MinY;
            float dimZ = bbox.MaxZ - bbox.MinZ;
            float maxDim = Math.Max(dimX, Math.Max(dimY, dimZ));
            if (maxDim <= 0) return result;
            float tolerance = maxDim * 0.001f;
            if (tolerance < 0.001f) tolerance = 0.001f;

            float faceVal;
            int axisIdx;
            switch (face)
            {
                case 0: faceVal = bbox.MaxX; axisIdx = 0; break;
                case 1: faceVal = bbox.MinX; axisIdx = 0; break;
                case 2: faceVal = bbox.MaxY; axisIdx = 1; break;
                case 3: faceVal = bbox.MinY; axisIdx = 1; break;
                case 4: faceVal = bbox.MaxZ; axisIdx = 2; break;
                case 5: faceVal = bbox.MinZ; axisIdx = 2; break;
                default: return result;
            }

            int numTris = bbox.Vertices.Count / 9;
            for (int i = 0; i < numTris; i++)
            {
                int baseIdx = i * 9;
                bool allOnFace = true;
                for (int v = 0; v < 3 && allOnFace; v++)
                {
                    float val = bbox.Vertices[baseIdx + v * 3 + axisIdx];
                    allOnFace = Math.Abs(val - faceVal) <= tolerance;
                }
                if (allOnFace)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        /// 면 위의 삼각형들을 연결된 그룹(클러스터)으로 분류.
        /// 꼭짓점을 공유하는 삼각형들은 같은 클러스터로 묶임.
        /// 각 클러스터 = 원본 삼각형 인덱스 리스트.
        /// </summary>
        public static List<List<int>> ClusterFaceTriangles(StlBoundingBox bbox, int face)
        {
            List<int> faceTriIndices = GetFaceTriangleIndices(bbox, face);
            if (faceTriIndices.Count == 0) return new List<List<int>>();

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            int n = faceTriIndices.Count;
            int[] parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;

            // vertex hash → 처음 등장한 클러스터 내 인덱스
            var vertexToIdx = new Dictionary<long, int>();

            for (int ci = 0; ci < n; ci++)
            {
                int triIdx = faceTriIndices[ci];
                int baseIdx = triIdx * 9;
                for (int v = 0; v < 3; v++)
                {
                    float x = bbox.Vertices[baseIdx + v * 3];
                    float y = bbox.Vertices[baseIdx + v * 3 + 1];
                    float z = bbox.Vertices[baseIdx + v * 3 + 2];
                    long key = SnapKey(x, y, z, snapGrid);

                    int existing;
                    if (vertexToIdx.TryGetValue(key, out existing))
                        UF_Union(parent, ci, existing);
                    else
                        vertexToIdx[key] = ci;
                }
            }

            // 루트별 그룹화
            var groups = new Dictionary<int, List<int>>();
            for (int ci = 0; ci < n; ci++)
            {
                int root = UF_Find(parent, ci);
                if (!groups.ContainsKey(root))
                    groups[root] = new List<int>();
                groups[root].Add(faceTriIndices[ci]);
            }
            return new List<List<int>>(groups.Values);
        }

        private static long SnapKey(float x, float y, float z, float grid)
        {
            long ix = (long)Math.Round(x / grid);
            long iy = (long)Math.Round(y / grid);
            long iz = (long)Math.Round(z / grid);
            return (ix * 1000003L + iy) * 1000003L + iz;
        }

        private static int UF_Find(int[] p, int i)
        {
            while (p[i] != i) { p[i] = p[p[i]]; i = p[i]; }
            return i;
        }

        private static void UF_Union(int[] p, int a, int b)
        {
            int ra = UF_Find(p, a), rb = UF_Find(p, b);
            if (ra != rb) p[ra] = rb;
        }

        /// <summary>
        /// 특정 삼각형 인덱스 목록만 바이너리 STL로 저장
        /// </summary>
        public static int WriteClusterStl(StlBoundingBox bbox, List<int> triIndices, string outputPath)
        {
            if (triIndices == null || triIndices.Count == 0) return 0;

            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(new byte[80]);
                bw.Write(triIndices.Count);
                foreach (int idx in triIndices)
                {
                    int baseIdx = idx * 9;
                    bw.Write(0f); bw.Write(0f); bw.Write(0f);
                    for (int vi = 0; vi < 9; vi++)
                        bw.Write(bbox.Vertices[baseIdx + vi]);
                    bw.Write((ushort)0);
                }
            }
            return triIndices.Count;
        }

        /// <summary>
        /// 삼각형 인덱스 목록의 정점 중심(centroid) 계산
        /// </summary>
        public static void GetClusterCentroid(StlBoundingBox bbox, List<int> triIndices,
            out float cx, out float cy, out float cz)
        {
            cx = cy = cz = 0;
            if (triIndices == null || triIndices.Count == 0) return;

            int count = 0;
            foreach (int triIdx in triIndices)
            {
                int baseIdx = triIdx * 9;
                for (int v = 0; v < 3; v++)
                {
                    cx += bbox.Vertices[baseIdx + v * 3];
                    cy += bbox.Vertices[baseIdx + v * 3 + 1];
                    cz += bbox.Vertices[baseIdx + v * 3 + 2];
                    count++;
                }
            }
            if (count > 0) { cx /= count; cy /= count; cz /= count; }
        }

        private static void UpdateBB(StlBoundingBox bb, float x, float y, float z)
        {
            if (x < bb.MinX) bb.MinX = x;
            if (x > bb.MaxX) bb.MaxX = x;
            if (y < bb.MinY) bb.MinY = y;
            if (y > bb.MaxY) bb.MaxY = y;
            if (z < bb.MinZ) bb.MinZ = z;
            if (z > bb.MaxZ) bb.MaxZ = z;
        }

        // =====================================================
        // 원형 윤곽선(Circular Contour) 감지
        // 바운딩박스 면의 경계선에서 원형 특징(구멍, 보스 등)을 찾음
        // =====================================================

        /// <summary>
        /// 바운딩박스 면의 원형 윤곽선 정보
        /// </summary>
        public class CircularContour
        {
            public int Face;                    // 바운딩박스 면 (0-5)
            public float CenterX, CenterY, CenterZ; // 3D 중심
            public float Radius;                // 평균 반지름
            public float Circularity;           // 원형도 (0~1, 높을수록 원에 가까움)
            public List<int> RimTriIndices;     // 윤곽선 인접 삼각형 인덱스

            public override string ToString()
            {
                return string.Format("Face{0} R={1:F1} C={2:F2} ({3}tri)",
                    Face, Radius, Circularity, RimTriIndices != null ? RimTriIndices.Count : 0);
            }
        }

        /// <summary>
        /// 모든 바운딩박스 면에서 원형 윤곽선(구멍, 보스 등)을 감지.
        /// 면의 삼각형 경계선을 추적하여 폐곡선을 만들고 원형도를 계산.
        /// </summary>
        public static List<CircularContour> FindCircularContours(StlBoundingBox bbox)
        {
            var result = new List<CircularContour>();
            if (bbox.Vertices.Count < 9) return result;

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            if (maxDim <= 0) return result;
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            for (int face = 0; face < 6; face++)
            {
                var faceTris = GetFaceTriangleIndices(bbox, face);
                if (faceTris.Count == 0) continue;

                // 면의 법선축과 접선축
                int normalAxis;
                switch (face)
                {
                    case 0: case 1: normalAxis = 0; break;
                    case 2: case 3: normalAxis = 1; break;
                    default: normalAxis = 2; break;
                }
                int ax1 = (normalAxis + 1) % 3;
                int ax2 = (normalAxis + 2) % 3;

                // 정점 키 → 3D 위치, 에지 카운트
                var vertexPos = new Dictionary<long, float[]>();
                var edgeCnt = new Dictionary<long, Dictionary<long, int>>();

                foreach (int triIdx in faceTris)
                {
                    int b = triIdx * 9;
                    long[] keys = new long[3];
                    for (int v = 0; v < 3; v++)
                    {
                        float x = bbox.Vertices[b + v * 3];
                        float y = bbox.Vertices[b + v * 3 + 1];
                        float z = bbox.Vertices[b + v * 3 + 2];
                        keys[v] = SnapKey(x, y, z, snapGrid);
                        if (!vertexPos.ContainsKey(keys[v]))
                            vertexPos[keys[v]] = new float[] { x, y, z };
                    }

                    for (int e = 0; e < 3; e++)
                    {
                        long ka = Math.Min(keys[e], keys[(e + 1) % 3]);
                        long kb = Math.Max(keys[e], keys[(e + 1) % 3]);
                        if (ka == kb) continue;

                        if (!edgeCnt.ContainsKey(ka))
                            edgeCnt[ka] = new Dictionary<long, int>();
                        int cnt;
                        edgeCnt[ka].TryGetValue(kb, out cnt);
                        edgeCnt[ka][kb] = cnt + 1;
                    }
                }

                // 경계 에지 (count==1): 인접 그래프 생성
                var adj = new Dictionary<long, List<long>>();
                foreach (var outer in edgeCnt)
                {
                    long ka = outer.Key;
                    foreach (var inner in outer.Value)
                    {
                        if (inner.Value != 1) continue;
                        long kb = inner.Key;
                        if (!adj.ContainsKey(ka)) adj[ka] = new List<long>();
                        if (!adj.ContainsKey(kb)) adj[kb] = new List<long>();
                        adj[ka].Add(kb);
                        adj[kb].Add(ka);
                    }
                }

                // 폐곡선 추적
                var visited = new HashSet<long>();
                foreach (long startV in adj.Keys)
                {
                    if (visited.Contains(startV)) continue;

                    var loop = new List<long>();
                    long cur = startV;
                    long prev = -1;
                    bool closed = false;

                    while (true)
                    {
                        visited.Add(cur);
                        loop.Add(cur);

                        long next = -1;
                        foreach (long nb in adj[cur])
                        {
                            if (nb != prev && !visited.Contains(nb))
                            {
                                next = nb;
                                break;
                            }
                        }

                        if (next < 0)
                        {
                            // 시작점으로 돌아갈 수 있으면 폐곡선
                            if (loop.Count >= 3 && adj[cur].Contains(startV))
                                closed = true;
                            break;
                        }
                        prev = cur;
                        cur = next;
                    }

                    if (!closed || loop.Count < 6) continue;

                    // 3D 중심 계산
                    float cx = 0, cy = 0, cz = 0;
                    foreach (long key in loop)
                    {
                        float[] p = vertexPos[key];
                        cx += p[0]; cy += p[1]; cz += p[2];
                    }
                    cx /= loop.Count; cy /= loop.Count; cz /= loop.Count;

                    // 2D 평면 위의 반지름/원형도 계산
                    float[] dists = new float[loop.Count];
                    float meanR = 0;
                    float cAx1 = 0, cAx2 = 0;
                    foreach (long key in loop)
                    {
                        float[] p = vertexPos[key];
                        cAx1 += p[ax1]; cAx2 += p[ax2];
                    }
                    cAx1 /= loop.Count; cAx2 /= loop.Count;

                    for (int i = 0; i < loop.Count; i++)
                    {
                        float[] p = vertexPos[loop[i]];
                        float d1 = p[ax1] - cAx1;
                        float d2 = p[ax2] - cAx2;
                        dists[i] = (float)Math.Sqrt(d1 * d1 + d2 * d2);
                        meanR += dists[i];
                    }
                    meanR /= loop.Count;

                    if (meanR < snapGrid * 2) continue; // 너무 작은 윤곽선 제외

                    float variance = 0;
                    for (int i = 0; i < loop.Count; i++)
                    {
                        float diff = dists[i] - meanR;
                        variance += diff * diff;
                    }
                    variance /= loop.Count;
                    float stddev = (float)Math.Sqrt(variance);
                    float circularity = (meanR > 0) ? 1.0f - stddev / meanR : 0;

                    // 윤곽선 인접 삼각형 수집 (경계 에지를 가진 삼각형)
                    var loopVerts = new HashSet<long>(loop);
                    var rimTris = new List<int>();
                    foreach (int triIdx in faceTris)
                    {
                        int b = triIdx * 9;
                        int onLoop = 0;
                        for (int v = 0; v < 3; v++)
                        {
                            float x = bbox.Vertices[b + v * 3];
                            float y = bbox.Vertices[b + v * 3 + 1];
                            float z = bbox.Vertices[b + v * 3 + 2];
                            long key = SnapKey(x, y, z, snapGrid);
                            if (loopVerts.Contains(key)) onLoop++;
                        }
                        if (onLoop >= 2) rimTris.Add(triIdx);
                    }

                    var contour = new CircularContour();
                    contour.Face = face;
                    contour.CenterX = cx;
                    contour.CenterY = cy;
                    contour.CenterZ = cz;
                    contour.Radius = meanR;
                    contour.Circularity = circularity;
                    contour.RimTriIndices = rimTris;
                    result.Add(contour);
                }
            }

            return result;
        }
        // =====================================================
        // Fusion 360 스타일 면 선택: 레이캐스트 + 법선 기반 면 확장
        // =====================================================

        /// <summary>
        /// Möller-Trumbore 레이-삼각형 교차 검사.
        /// 히트 시 거리 t >= 0 반환, 미스 시 -1 반환.
        /// </summary>
        public static float RayTriangleIntersect(
            float ox, float oy, float oz,
            float dx, float dy, float dz,
            float v0x, float v0y, float v0z,
            float v1x, float v1y, float v1z,
            float v2x, float v2y, float v2z)
        {
            const float EPSILON = 1e-7f;
            float e1x = v1x - v0x, e1y = v1y - v0y, e1z = v1z - v0z;
            float e2x = v2x - v0x, e2y = v2y - v0y, e2z = v2z - v0z;

            float hx = dy * e2z - dz * e2y;
            float hy = dz * e2x - dx * e2z;
            float hz = dx * e2y - dy * e2x;

            float a = e1x * hx + e1y * hy + e1z * hz;
            if (a > -EPSILON && a < EPSILON) return -1;

            float f = 1.0f / a;
            float sx = ox - v0x, sy = oy - v0y, sz = oz - v0z;
            float u = f * (sx * hx + sy * hy + sz * hz);
            if (u < 0 || u > 1) return -1;

            float qx = sy * e1z - sz * e1y;
            float qy = sz * e1x - sx * e1z;
            float qz = sx * e1y - sy * e1x;

            float v = f * (dx * qx + dy * qy + dz * qz);
            if (v < 0 || u + v > 1) return -1;

            float t = f * (e2x * qx + e2y * qy + e2z * qz);
            return t >= 0 ? t : -1;
        }

        /// <summary>
        /// 메시의 모든 삼각형에 대해 레이캐스트. 가장 가까운 히트 삼각형 인덱스 반환 (-1 = 미스).
        /// </summary>
        public static int RaycastMesh(StlBoundingBox bbox,
            float ox, float oy, float oz,
            float dx, float dy, float dz)
        {
            int numTris = bbox.Vertices.Count / 9;
            int bestIdx = -1;
            float bestT = float.MaxValue;

            for (int i = 0; i < numTris; i++)
            {
                int b = i * 9;
                float t = RayTriangleIntersect(ox, oy, oz, dx, dy, dz,
                    bbox.Vertices[b], bbox.Vertices[b + 1], bbox.Vertices[b + 2],
                    bbox.Vertices[b + 3], bbox.Vertices[b + 4], bbox.Vertices[b + 5],
                    bbox.Vertices[b + 6], bbox.Vertices[b + 7], bbox.Vertices[b + 8]);
                if (t >= 0 && t < bestT)
                {
                    bestT = t;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        /// <summary>
        /// 삼각형의 법선 벡터 계산 (정규화됨)
        /// </summary>
        public static void GetTriangleNormal(StlBoundingBox bbox, int triIdx,
            out float nx, out float ny, out float nz)
        {
            int b = triIdx * 9;
            float e1x = bbox.Vertices[b + 3] - bbox.Vertices[b];
            float e1y = bbox.Vertices[b + 4] - bbox.Vertices[b + 1];
            float e1z = bbox.Vertices[b + 5] - bbox.Vertices[b + 2];
            float e2x = bbox.Vertices[b + 6] - bbox.Vertices[b];
            float e2y = bbox.Vertices[b + 7] - bbox.Vertices[b + 1];
            float e2z = bbox.Vertices[b + 8] - bbox.Vertices[b + 2];

            nx = e1y * e2z - e1z * e2y;
            ny = e1z * e2x - e1x * e2z;
            nz = e1x * e2y - e1y * e2x;

            float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len > 1e-10f) { nx /= len; ny /= len; nz /= len; }
            else { nx = ny = nz = 0; }
        }

        /// <summary>
        /// 히트된 삼각형에서 시작하여 동일 면(coplanar) 삼각형들을 BFS로 탐색.
        /// 에지 공유 + 법선 유사(threshold 이내) 조건으로 면 확장.
        /// Fusion 360 스타일 면 선택.
        /// </summary>
        public static List<int> FloodFillFace(StlBoundingBox bbox, int startTriIdx, float normalToleranceDeg = 15f)
        {
            int numTris = bbox.Vertices.Count / 9;
            if (startTriIdx < 0 || startTriIdx >= numTris)
                return new List<int>();

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            // 각 삼각형의 꼭짓점 키 계산
            long[][] triVertKeys = new long[numTris][];
            for (int i = 0; i < numTris; i++)
            {
                int b = i * 9;
                triVertKeys[i] = new long[3];
                for (int v = 0; v < 3; v++)
                    triVertKeys[i][v] = SnapKey(bbox.Vertices[b + v * 3],
                        bbox.Vertices[b + v * 3 + 1], bbox.Vertices[b + v * 3 + 2], snapGrid);
            }

            // 에지 → 삼각형 인덱스 맵 (nested dict으로 overflow 방지)
            var edgeMap = new Dictionary<long, Dictionary<long, List<int>>>();
            for (int i = 0; i < numTris; i++)
            {
                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(triVertKeys[i][e], triVertKeys[i][(e + 1) % 3]);
                    long kb = Math.Max(triVertKeys[i][e], triVertKeys[i][(e + 1) % 3]);
                    if (ka == kb) continue;

                    Dictionary<long, List<int>> inner;
                    if (!edgeMap.TryGetValue(ka, out inner))
                    {
                        inner = new Dictionary<long, List<int>>();
                        edgeMap[ka] = inner;
                    }
                    List<int> tris;
                    if (!inner.TryGetValue(kb, out tris))
                    {
                        tris = new List<int>();
                        inner[kb] = tris;
                    }
                    tris.Add(i);
                }
            }

            // 시작 삼각형의 법선
            float snx, sny, snz;
            GetTriangleNormal(bbox, startTriIdx, out snx, out sny, out snz);

            float cosThreshold = (float)Math.Cos(normalToleranceDeg * Math.PI / 180.0);

            // BFS
            var result = new List<int>();
            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            queue.Enqueue(startTriIdx);
            visited.Add(startTriIdx);

            while (queue.Count > 0)
            {
                int curr = queue.Dequeue();
                result.Add(curr);

                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(triVertKeys[curr][e], triVertKeys[curr][(e + 1) % 3]);
                    long kb = Math.Max(triVertKeys[curr][e], triVertKeys[curr][(e + 1) % 3]);
                    if (ka == kb) continue;

                    Dictionary<long, List<int>> inner;
                    if (!edgeMap.TryGetValue(ka, out inner)) continue;
                    List<int> neighbors;
                    if (!inner.TryGetValue(kb, out neighbors)) continue;

                    foreach (int nb in neighbors)
                    {
                        if (nb == curr || visited.Contains(nb)) continue;

                        float nnx, nny, nnz;
                        GetTriangleNormal(bbox, nb, out nnx, out nny, out nnz);
                        float dot = snx * nnx + sny * nny + snz * nnz;

                        if (dot >= cosThreshold)
                        {
                            visited.Add(nb);
                            queue.Enqueue(nb);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 면 그룹 내 내부 경계 루프(구멍)에 인접한 삼각형만 분리.
        /// 클릭한 삼각형이 내부 경계에 인접하면 해당 구멍 루프의 인접 삼각형만 반환.
        /// 아니면 원본 faceTris를 그대로 반환.
        /// </summary>
        public static List<int> FilterHoleNearbyTris(StlBoundingBox bbox, List<int> faceTris,
            int clickedTriIdx, float normalToleranceDeg = 15f)
        {
            if (faceTris.Count < 6) return faceTris;

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            // 면 그룹 내 삼각형 집합
            var faceSet = new HashSet<int>(faceTris);

            // 각 삼각형의 꼭짓점 키
            int numTris = bbox.Vertices.Count / 9;
            var triVertKeys = new Dictionary<int, long[]>();
            foreach (int tri in faceTris)
            {
                int b = tri * 9;
                long[] keys = new long[3];
                for (int v = 0; v < 3; v++)
                    keys[v] = SnapKey(bbox.Vertices[b + v * 3],
                        bbox.Vertices[b + v * 3 + 1], bbox.Vertices[b + v * 3 + 2], snapGrid);
                triVertKeys[tri] = keys;
            }

            // 전체 메시의 에지 → 삼각형 맵 (내부 경계 판별용)
            var globalEdgeMap = new Dictionary<long, Dictionary<long, List<int>>>();
            for (int i = 0; i < numTris; i++)
            {
                int b = i * 9;
                long[] keys = new long[3];
                for (int v = 0; v < 3; v++)
                    keys[v] = SnapKey(bbox.Vertices[b + v * 3],
                        bbox.Vertices[b + v * 3 + 1], bbox.Vertices[b + v * 3 + 2], snapGrid);
                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(keys[e], keys[(e + 1) % 3]);
                    long kb = Math.Max(keys[e], keys[(e + 1) % 3]);
                    if (ka == kb) continue;
                    Dictionary<long, List<int>> inner;
                    if (!globalEdgeMap.TryGetValue(ka, out inner))
                    { inner = new Dictionary<long, List<int>>(); globalEdgeMap[ka] = inner; }
                    List<int> tris;
                    if (!inner.TryGetValue(kb, out tris))
                    { tris = new List<int>(); inner[kb] = tris; }
                    tris.Add(i);
                }
            }

            // 면 그룹 내에서 내부 경계 에지 찾기:
            // 면 그룹 내 1회만 등장하는 에지 중에서 전체 메시에서는 2회 등장하는 에지 = 내부 경계 (구멍 경계)
            var faceEdgeCnt = new Dictionary<long, Dictionary<long, int>>();
            foreach (int tri in faceTris)
            {
                long[] keys = triVertKeys[tri];
                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(keys[e], keys[(e + 1) % 3]);
                    long kb = Math.Max(keys[e], keys[(e + 1) % 3]);
                    if (ka == kb) continue;
                    Dictionary<long, int> ci;
                    if (!faceEdgeCnt.TryGetValue(ka, out ci))
                    { ci = new Dictionary<long, int>(); faceEdgeCnt[ka] = ci; }
                    int cnt; ci.TryGetValue(kb, out cnt); ci[kb] = cnt + 1;
                }
            }

            // 내부 경계 에지에 인접한 정점 키 수집
            var innerBoundaryVertKeys = new HashSet<long>();
            foreach (var outer in faceEdgeCnt)
            {
                long ka = outer.Key;
                foreach (var inner in outer.Value)
                {
                    if (inner.Value != 1) continue; // 면 내에서 1회 등장 = 경계 에지
                    long kb = inner.Key;
                    // 전체 메시에서 이 에지가 2회 등장하면 내부 경계 (다른 면과 공유)
                    Dictionary<long, List<int>> ge;
                    if (globalEdgeMap.TryGetValue(ka, out ge))
                    {
                        List<int> geTris;
                        if (ge.TryGetValue(kb, out geTris) && geTris.Count >= 2)
                        {
                            innerBoundaryVertKeys.Add(ka);
                            innerBoundaryVertKeys.Add(kb);
                        }
                    }
                }
            }

            if (innerBoundaryVertKeys.Count == 0) return faceTris; // 내부 경계 없음

            // 클릭한 삼각형이 내부 경계에 인접한지 확인
            if (!triVertKeys.ContainsKey(clickedTriIdx)) return faceTris;
            long[] clickKeys = triVertKeys[clickedTriIdx];
            bool clickNearHole = false;
            for (int v = 0; v < 3; v++)
            {
                if (innerBoundaryVertKeys.Contains(clickKeys[v]))
                { clickNearHole = true; break; }
            }
            if (!clickNearHole) return faceTris; // 클릭이 구멍 근처가 아님

            // 내부 경계에 인접한 삼각형만 BFS로 수집
            // 시작: 클릭한 삼각형에서, 내부 경계 정점을 공유하는 삼각형들만 탐색
            var holeTris = new List<int>();
            var visitedTris = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(clickedTriIdx);
            visitedTris.Add(clickedTriIdx);

            while (queue.Count > 0)
            {
                int curr = queue.Dequeue();
                // 이 삼각형이 내부 경계 정점을 포함하는지 확인
                long[] curKeys = triVertKeys[curr];
                bool touchesBoundary = false;
                for (int v = 0; v < 3; v++)
                {
                    if (innerBoundaryVertKeys.Contains(curKeys[v]))
                    { touchesBoundary = true; break; }
                }
                if (!touchesBoundary) continue; // 경계와 무관한 삼각형은 건너뜀

                holeTris.Add(curr);

                // 인접 삼각형 탐색 (에지 공유)
                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(curKeys[e], curKeys[(e + 1) % 3]);
                    long kb = Math.Max(curKeys[e], curKeys[(e + 1) % 3]);
                    if (ka == kb) continue;
                    Dictionary<long, List<int>> inner;
                    if (!globalEdgeMap.TryGetValue(ka, out inner)) continue;
                    List<int> neighbors;
                    if (!inner.TryGetValue(kb, out neighbors)) continue;
                    foreach (int nb in neighbors)
                    {
                        if (visitedTris.Contains(nb) || !faceSet.Contains(nb)) continue;
                        visitedTris.Add(nb);
                        queue.Enqueue(nb);
                    }
                }
            }

            return holeTris.Count >= 3 ? holeTris : faceTris;
        }

        /// <summary>
        /// 면 그룹의 경계 에지(외곽선) 꼭짓점 추출.
        /// face 내 삼각형에서 1회만 등장하는 에지 = 경계.
        /// 반환: float[6] 배열 리스트 (x0,y0,z0, x1,y1,z1)
        /// </summary>
        public static List<float[]> GetBoundaryEdges(StlBoundingBox bbox, List<int> faceTris)
        {
            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            var edgeCount = new Dictionary<long, Dictionary<long, int>>();
            var edgeVerts = new Dictionary<long, Dictionary<long, float[]>>();

            foreach (int triIdx in faceTris)
            {
                int b = triIdx * 9;
                long[] vk = new long[3];
                float[][] vp = new float[3][];
                for (int v = 0; v < 3; v++)
                {
                    vp[v] = new float[] {
                        bbox.Vertices[b + v * 3],
                        bbox.Vertices[b + v * 3 + 1],
                        bbox.Vertices[b + v * 3 + 2]
                    };
                    vk[v] = SnapKey(vp[v][0], vp[v][1], vp[v][2], snapGrid);
                }

                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(vk[e], vk[(e + 1) % 3]);
                    long kb = Math.Max(vk[e], vk[(e + 1) % 3]);
                    if (ka == kb) continue;

                    Dictionary<long, int> ci;
                    if (!edgeCount.TryGetValue(ka, out ci))
                    { ci = new Dictionary<long, int>(); edgeCount[ka] = ci; }
                    int cnt; ci.TryGetValue(kb, out cnt); ci[kb] = cnt + 1;

                    Dictionary<long, float[]> vi;
                    if (!edgeVerts.TryGetValue(ka, out vi))
                    { vi = new Dictionary<long, float[]>(); edgeVerts[ka] = vi; }
                    if (!vi.ContainsKey(kb))
                        vi[kb] = new float[] {
                            vp[e][0], vp[e][1], vp[e][2],
                            vp[(e + 1) % 3][0], vp[(e + 1) % 3][1], vp[(e + 1) % 3][2]
                        };
                }
            }

            var result = new List<float[]>();
            foreach (var outer in edgeCount)
                foreach (var inner in outer.Value)
                    if (inner.Value == 1)
                        result.Add(edgeVerts[outer.Key][inner.Key]);
            return result;
        }

        /// <summary>
        /// 경계 에지를 얇은 삼각형 스트립으로 STL 파일 작성.
        /// 법선 방향으로 살짝 들어올려 z-fighting 방지.
        /// </summary>
        public static int WriteEdgeHighlightStl(StlBoundingBox bbox, List<int> faceTris,
            string filePath, float faceNx, float faceNy, float faceNz)
        {
            var edges = GetBoundaryEdges(bbox, faceTris);
            if (edges.Count == 0) return 0;

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float thickness = maxDim * 0.004f;
            float lift = maxDim * 0.002f;

            using (var bw = new System.IO.BinaryWriter(System.IO.File.Create(filePath)))
            {
                bw.Write(new byte[80]);
                bw.Write((uint)(edges.Count * 2));

                foreach (var edge in edges)
                {
                    float x0 = edge[0] + faceNx * lift;
                    float y0 = edge[1] + faceNy * lift;
                    float z0 = edge[2] + faceNz * lift;
                    float x1 = edge[3] + faceNx * lift;
                    float y1 = edge[4] + faceNy * lift;
                    float z1 = edge[5] + faceNz * lift;

                    float dx = x1 - x0, dy = y1 - y0, dz = z1 - z0;
                    float px = dy * faceNz - dz * faceNy;
                    float py = dz * faceNx - dx * faceNz;
                    float pz = dx * faceNy - dy * faceNx;
                    float plen = (float)Math.Sqrt(px * px + py * py + pz * pz);
                    if (plen > 1e-10f)
                    { px = px / plen * thickness; py = py / plen * thickness; pz = pz / plen * thickness; }

                    bw.Write(faceNx); bw.Write(faceNy); bw.Write(faceNz);
                    bw.Write(x0); bw.Write(y0); bw.Write(z0);
                    bw.Write(x1); bw.Write(y1); bw.Write(z1);
                    bw.Write(x0 + px); bw.Write(y0 + py); bw.Write(z0 + pz);
                    bw.Write((ushort)0);

                    bw.Write(faceNx); bw.Write(faceNy); bw.Write(faceNz);
                    bw.Write(x1); bw.Write(y1); bw.Write(z1);
                    bw.Write(x1 + px); bw.Write(y1 + py); bw.Write(z1 + pz);
                    bw.Write(x0 + px); bw.Write(y0 + py); bw.Write(z0 + pz);
                    bw.Write((ushort)0);
                }
            }
            return edges.Count;
        }

        /// <summary>
        /// 선택된 면(faceTris)의 경계선에서 원형 특징(구멍, 보스 등)을 찾음.
        /// 경계 에지를 폐곡선으로 추적하고 원형도 계산.
        /// 원형도가 가장 높은 특징의 중심 좌표 반환 (없으면 null).
        /// </summary>
        public static float[] FindCircularFeatureOnFace(StlBoundingBox bbox, List<int> faceTris,
            float minCircularity = 0.7f)
        {
            if (faceTris.Count < 3) return null;

            float maxDim = Math.Max(bbox.MaxX - bbox.MinX,
                            Math.Max(bbox.MaxY - bbox.MinY, bbox.MaxZ - bbox.MinZ));
            float snapGrid = maxDim * 0.0005f;
            if (snapGrid < 0.0005f) snapGrid = 0.0005f;

            // 정점 키 → 3D 위치, 에지 카운트
            var vertexPos = new Dictionary<long, float[]>();
            var edgeCnt = new Dictionary<long, Dictionary<long, int>>();

            foreach (int triIdx in faceTris)
            {
                int b = triIdx * 9;
                long[] keys = new long[3];
                for (int v = 0; v < 3; v++)
                {
                    float x = bbox.Vertices[b + v * 3];
                    float y = bbox.Vertices[b + v * 3 + 1];
                    float z = bbox.Vertices[b + v * 3 + 2];
                    keys[v] = SnapKey(x, y, z, snapGrid);
                    if (!vertexPos.ContainsKey(keys[v]))
                        vertexPos[keys[v]] = new float[] { x, y, z };
                }

                for (int e = 0; e < 3; e++)
                {
                    long ka = Math.Min(keys[e], keys[(e + 1) % 3]);
                    long kb = Math.Max(keys[e], keys[(e + 1) % 3]);
                    if (ka == kb) continue;

                    Dictionary<long, int> ci;
                    if (!edgeCnt.TryGetValue(ka, out ci))
                    { ci = new Dictionary<long, int>(); edgeCnt[ka] = ci; }
                    int cnt; ci.TryGetValue(kb, out cnt); ci[kb] = cnt + 1;
                }
            }

            // 경계 에지 (count==1): 인접 그래프 생성
            var adj = new Dictionary<long, List<long>>();
            foreach (var outer in edgeCnt)
            {
                long ka = outer.Key;
                foreach (var inner in outer.Value)
                {
                    if (inner.Value != 1) continue;
                    long kb = inner.Key;
                    if (!adj.ContainsKey(ka)) adj[ka] = new List<long>();
                    if (!adj.ContainsKey(kb)) adj[kb] = new List<long>();
                    adj[ka].Add(kb);
                    adj[kb].Add(ka);
                }
            }

            // 폐곡선 추적 → 원형도 계산
            var visited = new HashSet<long>();
            float bestCircularity = 0;
            float[] bestCenter = null;

            foreach (long startV in adj.Keys)
            {
                if (visited.Contains(startV)) continue;

                var loop = new List<long>();
                long cur = startV;
                long prev = -1;
                bool closed = false;

                while (true)
                {
                    visited.Add(cur);
                    loop.Add(cur);

                    long next = -1;
                    foreach (long nb in adj[cur])
                    {
                        if (nb != prev && !visited.Contains(nb))
                        {
                            next = nb;
                            break;
                        }
                    }

                    if (next < 0)
                    {
                        if (loop.Count >= 6 && adj[cur].Contains(startV))
                            closed = true;
                        break;
                    }
                    prev = cur;
                    cur = next;
                }

                if (!closed || loop.Count < 6) continue;

                // 3D 중심 계산
                float cx = 0, cy = 0, cz = 0;
                foreach (long key in loop)
                {
                    float[] p = vertexPos[key];
                    cx += p[0]; cy += p[1]; cz += p[2];
                }
                cx /= loop.Count; cy /= loop.Count; cz /= loop.Count;

                // 3D 거리로 원형도 계산
                float meanR = 0;
                float[] dists = new float[loop.Count];
                for (int i = 0; i < loop.Count; i++)
                {
                    float[] p = vertexPos[loop[i]];
                    float dx = p[0] - cx, dy = p[1] - cy, dz = p[2] - cz;
                    dists[i] = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    meanR += dists[i];
                }
                meanR /= loop.Count;

                if (meanR < snapGrid * 2) continue;

                float variance = 0;
                for (int i = 0; i < loop.Count; i++)
                {
                    float diff = dists[i] - meanR;
                    variance += diff * diff;
                }
                variance /= loop.Count;
                float stddev = (float)Math.Sqrt(variance);
                float circularity = (meanR > 0) ? 1.0f - stddev / meanR : 0;

                if (circularity >= minCircularity && circularity > bestCircularity)
                {
                    bestCircularity = circularity;
                    bestCenter = new float[] { cx, cy, cz };
                }
            }

            return bestCenter;
        }

        // ========================================================================
        // snap.html / snap.js 포팅 — STL 기하 분석 (평면 클러스터링)
        //   FloodFillFace 와 달리 인접 무관 같은 평면이면 모두 한 면으로 묶음.
        //   소형 디테일 / 떨어진 영역 까지 같은 평면으로 인식.
        // ========================================================================

        /// <summary>STL 의 모든 삼각형을 법선+오프셋 양자화로 평면 그룹화한 결과.</summary>
        public class PlaneCluster
        {
            public float Nx, Ny, Nz;       // 면적 가중 평균 법선
            public float Offset;            // 면적 가중 평균 오프셋 (n·c)
            public float Area;              // 총 면적
            public List<int> TriIndices = new List<int>();
        }

        public class StlPlaneAnalysis
        {
            public List<PlaneCluster> Planes;
            public int[] TriToPlane;         // 삼각형 idx → planes idx (-1 이면 작은 면 또는 퇴화)
        }

        // snap.js 와 동일한 파라미터 (snap만드는법.md §3)
        private const float PLANE_MIN_AREA   = 12.0f;   // mm² (작은 평면 제외)
        private const float NORMAL_QUANT     = 12.0f;   // 법선 양자화 단계 (≈5°)
        private const float OFFSET_QUANT     = 0.5f;    // mm 오프셋 양자화

        /// <summary>
        /// 모든 삼각형을 (법선 방향, 오프셋) 으로 양자화해 해시 버킷에 모음.
        /// 같은 평면 위 삼각형은 인접하지 않아도 같은 PlaneCluster.
        /// snap.js analyze() 의 §4.2~4.3 포팅.
        /// </summary>
        public static StlPlaneAnalysis AnalyzePlanes(StlBoundingBox bbox)
        {
            int numTris = bbox.Vertices.Count / 9;
            int[] triToPlaneRaw = new int[numTris];
            for (int i = 0; i < numTris; i++) triToPlaneRaw[i] = -1;

            List<PlaneCluster> planes = new List<PlaneCluster>();
            Dictionary<string, int> planeMap = new Dictionary<string, int>();

            for (int t = 0; t < numTris; t++)
            {
                int b = t * 9;
                float v0x = bbox.Vertices[b],     v0y = bbox.Vertices[b + 1], v0z = bbox.Vertices[b + 2];
                float v1x = bbox.Vertices[b + 3], v1y = bbox.Vertices[b + 4], v1z = bbox.Vertices[b + 5];
                float v2x = bbox.Vertices[b + 6], v2y = bbox.Vertices[b + 7], v2z = bbox.Vertices[b + 8];

                // 법선 (정규화 전 cross = 2*면적)
                float e1x = v1x - v0x, e1y = v1y - v0y, e1z = v1z - v0z;
                float e2x = v2x - v0x, e2y = v2y - v0y, e2z = v2z - v0z;
                float crx = e1y * e2z - e1z * e2y;
                float cry = e1z * e2x - e1x * e2z;
                float crz = e1x * e2y - e1y * e2x;
                float clen = (float)Math.Sqrt(crx * crx + cry * cry + crz * crz);
                if (clen < 1e-10f) continue;   // 퇴화 삼각형
                float area = clen * 0.5f;
                float nx = crx / clen, ny = cry / clen, nz = crz / clen;

                // 무게중심
                float cx = (v0x + v1x + v2x) / 3.0f;
                float cy = (v0y + v1y + v2y) / 3.0f;
                float cz = (v0z + v1z + v2z) / 3.0f;
                float off = nx * cx + ny * cy + nz * cz;

                // 양자화 키 (snap.js §4.2 와 동일)
                int kx = (int)Math.Round(nx * NORMAL_QUANT);
                int ky = (int)Math.Round(ny * NORMAL_QUANT);
                int kz = (int)Math.Round(nz * NORMAL_QUANT);
                int koff = (int)Math.Round(off / OFFSET_QUANT);
                string key = kx + "_" + ky + "_" + kz + "_" + koff;

                int idx;
                if (!planeMap.TryGetValue(key, out idx))
                {
                    idx = planes.Count;
                    planes.Add(new PlaneCluster());
                    planeMap[key] = idx;
                }
                PlaneCluster pl = planes[idx];
                pl.TriIndices.Add(t);
                pl.Area += area;
                // 면적 가중 누적 → 나중에 평균
                pl.Nx += nx * area;
                pl.Ny += ny * area;
                pl.Nz += nz * area;
                pl.Offset += off * area;
                triToPlaneRaw[t] = idx;
            }

            // 면적 가중 평균 정규화
            for (int p = 0; p < planes.Count; p++)
            {
                PlaneCluster pl = planes[p];
                if (pl.Area > 0)
                {
                    float invA = 1.0f / pl.Area;
                    pl.Nx *= invA; pl.Ny *= invA; pl.Nz *= invA;
                    pl.Offset *= invA;
                    float nlen = (float)Math.Sqrt(pl.Nx * pl.Nx + pl.Ny * pl.Ny + pl.Nz * pl.Nz);
                    if (nlen > 1e-9f)
                    { pl.Nx /= nlen; pl.Ny /= nlen; pl.Nz /= nlen; }
                }
            }

            // 작은 평면 제외 + triToPlane 재매핑 (snap.js §4.3)
            int[] oldToNew = new int[planes.Count];
            List<PlaneCluster> bigPlanes = new List<PlaneCluster>();
            for (int p = 0; p < planes.Count; p++)
            {
                if (planes[p].Area >= PLANE_MIN_AREA)
                {
                    oldToNew[p] = bigPlanes.Count;
                    bigPlanes.Add(planes[p]);
                }
                else oldToNew[p] = -1;
            }
            int[] triToPlane = new int[numTris];
            for (int t = 0; t < numTris; t++)
            {
                int old = triToPlaneRaw[t];
                triToPlane[t] = (old >= 0) ? oldToNew[old] : -1;
            }

            return new StlPlaneAnalysis { Planes = bigPlanes, TriToPlane = triToPlane };
        }

        // ========================================================================
        // snap.js §4.4~4.8 포팅 — 원(구멍/보스) 검출
        //   1) 정점 용접 (위상 복원)
        //   2) 에지 인접 → 특징에지 (이면각 > 60°)
        //   3) 정점 인접 그래프 → DFS 컴포넌트
        //   4) 각 컴포넌트에 대해 fitCircleLoop (PCA + Kasa)
        //   5) 중복 제거 + 외곽 실루엣 필터
        // ========================================================================

        public class CircleFit
        {
            public float Cx, Cy, Cz;       // 원 중심 (STL native local)
            public float Ax, Ay, Az;       // 원 축 (단위벡터)
            public float Radius;
            public float CircVariation;    // 변동계수 (std/mean)
        }

        private const float CIRC_WELD_EPS  = 0.05f;
        private const float CIRC_FEAT_DIH  = 1.04719755f;   // 60° in rad
        private const float CIRC_CV_TOL    = 0.22f;
        private const float CIRC_PLANAR    = 0.18f;
        private const float CIRC_RMIN      = 0.6f;
        private const float CIRC_RMAX      = 80.0f;
        private const int   CIRC_MINPTS    = 5;

        /// <summary>snap.js analyze() §4.4~4.8 — STL 의 모든 원(구멍/보스) 검출.</summary>
        public static List<CircleFit> AnalyzeCircles(StlBoundingBox bbox)
        {
            int numTris = bbox.Vertices.Count / 9;
            if (numTris == 0) return new List<CircleFit>();

            // === 1) 정점 용접 ===
            Dictionary<long, int> vmap = new Dictionary<long, int>();
            List<float[]> V = new List<float[]>();
            int[][] tris = new int[numTris][];
            float[][] tN = new float[numTris][];

            for (int t = 0; t < numTris; t++)
            {
                int b = t * 9;
                int[] ids = new int[3];
                for (int v = 0; v < 3; v++)
                {
                    float x = bbox.Vertices[b + v * 3];
                    float y = bbox.Vertices[b + v * 3 + 1];
                    float z = bbox.Vertices[b + v * 3 + 2];
                    long kx = (long)Math.Round(x / CIRC_WELD_EPS) & 0x1FFFFFL;
                    long ky = (long)Math.Round(y / CIRC_WELD_EPS) & 0x1FFFFFL;
                    long kz = (long)Math.Round(z / CIRC_WELD_EPS) & 0x1FFFFFL;
                    long key = (kx << 42) | (ky << 21) | kz;
                    int id;
                    if (!vmap.TryGetValue(key, out id))
                    {
                        id = V.Count;
                        V.Add(new float[] { x, y, z });
                        vmap[key] = id;
                    }
                    ids[v] = id;
                }
                if (ids[0] == ids[1] || ids[1] == ids[2] || ids[0] == ids[2])
                {
                    tris[t] = null; tN[t] = null;
                    continue;
                }
                tris[t] = ids;
                float[] a = V[ids[0]], bv = V[ids[1]], cv = V[ids[2]];
                float e1x = bv[0] - a[0], e1y = bv[1] - a[1], e1z = bv[2] - a[2];
                float e2x = cv[0] - a[0], e2y = cv[1] - a[1], e2z = cv[2] - a[2];
                float nx = e1y * e2z - e1z * e2y;
                float ny = e1z * e2x - e1x * e2z;
                float nz = e1x * e2y - e1y * e2x;
                float nl = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (nl > 1e-10f) tN[t] = new float[] { nx / nl, ny / nl, nz / nl };
                else tN[t] = new float[] { 0, 1, 0 };
            }

            // === 2) 에지 인접 ===
            Dictionary<long, List<int>> edgeMap = new Dictionary<long, List<int>>();
            for (int t = 0; t < numTris; t++)
            {
                if (tris[t] == null) continue;
                int[] tr = tris[t];
                for (int e = 0; e < 3; e++)
                {
                    int i = tr[e], j = tr[(e + 1) % 3];
                    long ka = Math.Min(i, j);
                    long kb = Math.Max(i, j);
                    long ekey = (ka << 32) | (kb & 0xFFFFFFFFL);
                    List<int> list;
                    if (!edgeMap.TryGetValue(ekey, out list))
                    {
                        list = new List<int>();
                        edgeMap[ekey] = list;
                    }
                    list.Add(t);
                }
            }

            // === 3) 특징에지 → 정점 인접 그래프 ===
            Dictionary<int, HashSet<int>> adj = new Dictionary<int, HashSet<int>>();
            foreach (KeyValuePair<long, List<int>> kv in edgeMap)
            {
                long key = kv.Key;
                int i = (int)(key >> 32);
                int j = (int)(key & 0xFFFFFFFFL);
                List<int> l = kv.Value;
                bool feat = false;
                if (l.Count == 1) feat = true;
                else if (l.Count == 2)
                {
                    float[] n0 = tN[l[0]], n1 = tN[l[1]];
                    float dot = n0[0] * n1[0] + n0[1] * n1[1] + n0[2] * n1[2];
                    if (dot > 1) dot = 1; else if (dot < -1) dot = -1;
                    float angle = (float)Math.Acos(dot);
                    if (angle > CIRC_FEAT_DIH) feat = true;
                }
                if (feat)
                {
                    if (!adj.ContainsKey(i)) adj[i] = new HashSet<int>();
                    adj[i].Add(j);
                    if (!adj.ContainsKey(j)) adj[j] = new HashSet<int>();
                    adj[j].Add(i);
                }
            }

            // === 4) 컴포넌트 추출 (DFS) + fitCircleLoop ===
            HashSet<int> seen = new HashSet<int>();
            List<CircleFit> rawCircles = new List<CircleFit>();
            foreach (int start in adj.Keys)
            {
                if (seen.Contains(start)) continue;
                List<int> comp = new List<int>();
                Stack<int> stack = new Stack<int>();
                stack.Push(start); seen.Add(start);
                while (stack.Count > 0)
                {
                    int x = stack.Pop();
                    comp.Add(x);
                    foreach (int y in adj[x])
                    {
                        if (!seen.Contains(y)) { seen.Add(y); stack.Push(y); }
                    }
                }
                if (comp.Count < CIRC_MINPTS) continue;
                List<float[]> pts = new List<float[]>();
                foreach (int idx in comp) pts.Add(V[idx]);
                CircleFit c = FitCircleLoop(pts);
                if (c != null) rawCircles.Add(c);
            }

            // === 5) 중복 제거 + 외곽 실루엣 필터 ===
            List<CircleFit> uniq = DedupeCircles(rawCircles);
            List<CircleFit> holes = FilterOuterSilhouettes(uniq, V);
            return holes;
        }

        private static CircleFit FitCircleLoop(List<float[]> pts)
        {
            int n = pts.Count;
            if (n < CIRC_MINPTS) return null;
            // 중심 평균
            double cx = 0, cy = 0, cz = 0;
            for (int i = 0; i < n; i++) { cx += pts[i][0]; cy += pts[i][1]; cz += pts[i][2]; }
            cx /= n; cy /= n; cz /= n;
            // 공분산 행렬
            double[,] M = new double[3, 3];
            for (int i = 0; i < n; i++)
            {
                double dx = pts[i][0] - cx, dy = pts[i][1] - cy, dz = pts[i][2] - cz;
                M[0, 0] += dx * dx; M[1, 1] += dy * dy; M[2, 2] += dz * dz;
                M[0, 1] += dx * dy; M[0, 2] += dx * dz; M[1, 2] += dy * dz;
            }
            M[1, 0] = M[0, 1]; M[2, 0] = M[0, 2]; M[2, 1] = M[1, 2];
            // 평면 법선 (최소 고유벡터)
            double[] axis = PlaneNormalPCA(M);
            double ax = axis[0], ay = axis[1], az = axis[2];
            // 평면 기저 u, v
            double ux, uy, uz;
            if (Math.Abs(ax) > 0.9) { ux = 0; uy = 1; uz = 0; }
            else { ux = 1; uy = 0; uz = 0; }
            double udota = ux * ax + uy * ay + uz * az;
            ux -= udota * ax; uy -= udota * ay; uz -= udota * az;
            double ulen = Math.Sqrt(ux * ux + uy * uy + uz * uz);
            if (ulen < 1e-9) return null;
            ux /= ulen; uy /= ulen; uz /= ulen;
            double vx = ay * uz - az * uy;
            double vy = az * ux - ax * uz;
            double vz = ax * uy - ay * ux;
            // 2D 투영 + 평면 이탈
            double planeDev = 0;
            double[][] p2 = new double[n][];
            for (int i = 0; i < n; i++)
            {
                double dx = pts[i][0] - cx, dy = pts[i][1] - cy, dz = pts[i][2] - cz;
                double doff = dx * ax + dy * ay + dz * az;
                double aDev = Math.Abs(doff);
                if (aDev > planeDev) planeDev = aDev;
                p2[i] = new double[] { dx * ux + dy * uy + dz * uz, dx * vx + dy * vy + dz * vz };
            }
            double[] fit = FitCircle2D(p2);
            if (fit == null) return null;
            double r = fit[2];
            if (r < CIRC_RMIN || r > CIRC_RMAX) return null;
            // 변동계수
            double sumR = 0;
            for (int i = 0; i < n; i++)
            {
                double rdx = p2[i][0] - fit[0], rdy = p2[i][1] - fit[1];
                sumR += Math.Sqrt(rdx * rdx + rdy * rdy);
            }
            double mean = sumR / n;
            double sumVar = 0;
            for (int i = 0; i < n; i++)
            {
                double rdx = p2[i][0] - fit[0], rdy = p2[i][1] - fit[1];
                double rd = Math.Sqrt(rdx * rdx + rdy * rdy);
                sumVar += (rd - mean) * (rd - mean);
            }
            double std = Math.Sqrt(sumVar / n);
            double cv = std / mean;
            if (cv > CIRC_CV_TOL) return null;
            if (planeDev / r > CIRC_PLANAR) return null;
            float centerX = (float)(cx + ux * fit[0] + vx * fit[1]);
            float centerY = (float)(cy + uy * fit[0] + vy * fit[1]);
            float centerZ = (float)(cz + uz * fit[0] + vz * fit[1]);
            return new CircleFit
            {
                Cx = centerX, Cy = centerY, Cz = centerZ,
                Ax = (float)ax, Ay = (float)ay, Az = (float)az,
                Radius = (float)mean,
                CircVariation = (float)cv
            };
        }

        private static double[] PlaneNormalPCA(double[,] M)
        {
            double a00 = M[0, 0], a11 = M[1, 1], a22 = M[2, 2];
            double a01 = M[0, 1], a02 = M[0, 2], a12 = M[1, 2];
            double p1 = a01 * a01 + a02 * a02 + a12 * a12;
            double[] eigs;
            if (p1 < 1e-18) eigs = new double[] { a00, a11, a22 };
            else
            {
                double q = (a00 + a11 + a22) / 3.0;
                double p2 = (a00 - q) * (a00 - q) + (a11 - q) * (a11 - q) + (a22 - q) * (a22 - q) + 2 * p1;
                double p = Math.Sqrt(p2 / 6.0);
                double b00 = (a00 - q) / p, b11 = (a11 - q) / p, b22 = (a22 - q) / p;
                double b01 = a01 / p, b02 = a02 / p, b12 = a12 / p;
                double detB = b00 * (b11 * b22 - b12 * b12) - b01 * (b01 * b22 - b12 * b02) + b02 * (b01 * b12 - b11 * b02);
                double rclamp = detB / 2.0;
                if (rclamp > 1) rclamp = 1; else if (rclamp < -1) rclamp = -1;
                double phi = Math.Acos(rclamp) / 3.0;
                double e1 = q + 2 * p * Math.Cos(phi);
                double e3 = q + 2 * p * Math.Cos(phi + 2 * Math.PI / 3.0);
                double e2 = 3 * q - e1 - e3;
                eigs = new double[] { e1, e2, e3 };
            }
            double lam = Math.Min(eigs[0], Math.Min(eigs[1], eigs[2]));
            double[] r0 = { a00 - lam, a01, a02 };
            double[] r1 = { a01, a11 - lam, a12 };
            double[] r2 = { a02, a12, a22 - lam };
            double[][] cands = new double[3][];
            cands[0] = new double[] { r0[1] * r1[2] - r0[2] * r1[1], r0[2] * r1[0] - r0[0] * r1[2], r0[0] * r1[1] - r0[1] * r1[0] };
            cands[1] = new double[] { r0[1] * r2[2] - r0[2] * r2[1], r0[2] * r2[0] - r0[0] * r2[2], r0[0] * r2[1] - r0[1] * r2[0] };
            cands[2] = new double[] { r1[1] * r2[2] - r1[2] * r2[1], r1[2] * r2[0] - r1[0] * r2[2], r1[0] * r2[1] - r1[1] * r2[0] };
            double[] best = cands[0];
            double bl = Math.Sqrt(best[0] * best[0] + best[1] * best[1] + best[2] * best[2]);
            for (int i = 1; i < 3; i++)
            {
                double l = Math.Sqrt(cands[i][0] * cands[i][0] + cands[i][1] * cands[i][1] + cands[i][2] * cands[i][2]);
                if (l > bl) { bl = l; best = cands[i]; }
            }
            if (bl < 1e-12) return new double[] { 0, 0, 1 };
            return new double[] { best[0] / bl, best[1] / bl, best[2] / bl };
        }

        private static double[] FitCircle2D(double[][] pts)
        {
            int n = pts.Length;
            double Sx = 0, Sy = 0, Sxx = 0, Syy = 0, Sxy = 0;
            double Sxz = 0, Syz = 0, Sz = 0;
            for (int i = 0; i < n; i++)
            {
                double x = pts[i][0], y = pts[i][1], z = x * x + y * y;
                Sx += x; Sy += y; Sxx += x * x; Syy += y * y;
                Sxy += x * y; Sxz += x * z; Syz += y * z; Sz += z;
            }
            double[,] A = { { Sxx, Sxy, Sx }, { Sxy, Syy, Sy }, { Sx, Sy, n } };
            double[] b = { Sxz, Syz, Sz };
            double[] s = Solve3(A, b);
            if (s == null) return null;
            double cx = s[0] / 2, cy = s[1] / 2;
            double r2 = cx * cx + cy * cy + s[2];
            if (r2 < 0) return null;
            return new double[] { cx, cy, Math.Sqrt(r2) };
        }

        private static double[] Solve3(double[,] A, double[] b)
        {
            double[,] M = new double[3, 4];
            for (int i = 0; i < 3; i++)
            {
                M[i, 0] = A[i, 0]; M[i, 1] = A[i, 1]; M[i, 2] = A[i, 2]; M[i, 3] = b[i];
            }
            for (int i = 0; i < 3; i++)
            {
                int piv = i;
                for (int r = i + 1; r < 3; r++) if (Math.Abs(M[r, i]) > Math.Abs(M[piv, i])) piv = r;
                if (piv != i)
                {
                    for (int c = 0; c < 4; c++) { double tmp = M[i, c]; M[i, c] = M[piv, c]; M[piv, c] = tmp; }
                }
                if (Math.Abs(M[i, i]) < 1e-12) return null;
                for (int r = 0; r < 3; r++)
                {
                    if (r == i) continue;
                    double f = M[r, i] / M[i, i];
                    for (int c = i; c < 4; c++) M[r, c] -= f * M[i, c];
                }
            }
            return new double[] { M[0, 3] / M[0, 0], M[1, 3] / M[1, 1], M[2, 3] / M[2, 2] };
        }

        private static List<CircleFit> DedupeCircles(List<CircleFit> circles)
        {
            List<CircleFit> uniq = new List<CircleFit>();
            for (int i = 0; i < circles.Count; i++)
            {
                CircleFit c = circles[i];
                bool dup = false;
                for (int j = 0; j < uniq.Count; j++)
                {
                    CircleFit u = uniq[j];
                    float dx = c.Cx - u.Cx, dy = c.Cy - u.Cy, dz = c.Cz - u.Cz;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    float ddot = Math.Abs(c.Ax * u.Ax + c.Ay * u.Ay + c.Az * u.Az);
                    if (Math.Abs(c.Radius - u.Radius) < 0.4f && d < 0.6f && ddot > 0.95f)
                    { dup = true; break; }
                }
                if (!dup) uniq.Add(c);
            }
            return uniq;
        }

        private static List<CircleFit> FilterOuterSilhouettes(List<CircleFit> circles, List<float[]> V)
        {
            List<CircleFit> holes = new List<CircleFit>();
            for (int k = 0; k < circles.Count; k++)
            {
                CircleFit c = circles[k];
                float maxLat = 0;
                for (int i = 0; i < V.Count; i++)
                {
                    float[] p = V[i];
                    float rx = p[0] - c.Cx, ry = p[1] - c.Cy, rz = p[2] - c.Cz;
                    float dot = rx * c.Ax + ry * c.Ay + rz * c.Az;
                    float latX = rx - dot * c.Ax;
                    float latY = ry - dot * c.Ay;
                    float latZ = rz - dot * c.Az;
                    float lat = (float)Math.Sqrt(latX * latX + latY * latY + latZ * latZ);
                    if (lat > maxLat) maxLat = lat;
                }
                if (c.Radius < 0.85f * maxLat) holes.Add(c);
            }
            return holes;
        }
    }

        // ================================================================
        // STL 면 피킹 — 월드 레이 → 가장 가까운 삼각형 (2026-09-03 MakeUrdf 에서 이관)
        //
        // 왜 옮겼나: MakeUrdf 의 TrySelectStlFace(클릭)와 TryHoverStlFace(호버)가
        //   **같은 레이캐스트를 각각 한 벌씩** 갖고 있었다(공통 95줄, 변수명만 다름).
        //   선택과 호버가 서로 다른 판정을 갖는 구조라, 한쪽만 고치면 조용히 어긋난다
        //   — "마우스를 올렸을 때 빛나는 면과 클릭했을 때 잡히는 면이 다르다"가 여기서 난다.
        //   ⇒ 골격을 여기 하나로 두고 둘 다 이걸 부른다. 원칙(기능은 DLL) 준수 + 중복 제거.
        // ================================================================

        /// <summary>STL 면 피킹 결과. StepIndex &lt; 0 이면 히트 없음.</summary>
        public class StlPickResult
        {
            public int StepIndex = -1;          // 히트한 DH 스텝 번호
            public int TriangleIndex = -1;      // 그 STL 안의 삼각형 번호
            public float Distance = float.MaxValue;   // 레이 원점부터의 거리 (STL 간 비교용)
            public StlBoundingBox Bbox;         // 히트한 STL 의 파싱 결과
            public float OffX, OffY, OffZ;      // 그 스텝의 STL 배치 오프셋
            public float Pan, Tilt, Swing;      // 그 스텝의 STL 배치 회전
            public bool Hit { get { return StepIndex >= 0; } }
        }

        /// <summary>월드 레이로 모든 STL 스텝을 훑어 가장 가까운 삼각형을 찾는다.
        ///
        /// 좌표 변환 — 렌더링이 DH_chain · Translate(off) · Rz(swing) · Rx(tilt) · Ry(pan) · vertex
        /// 이므로, 레이는 그 역순으로 STL 로컬로 내린다:
        ///   Ry(-pan) · Rx(-tilt) · Rz(-swing) · (Re^T · (ray - endpoint) - offset)
        /// </summary>
        /// <param name="builder">DH 빌더 (스텝·엔드포인트 자세 제공)</param>
        /// <param name="stlDir">STL 파일이 있는 폴더 (앱의 stl 폴더)</param>
        public static StlPickResult PickStlFace(DhBuilder builder, string stlDir,
            double wox, double woy, double woz, double wdx, double wdy, double wdz)
        {
            StlPickResult best = new StlPickResult();
            if (builder == null) return best;

            for (int i = 0; i < builder.StepCount; i++)
            {
                DhStep step = builder.GetStep(i);
                if (step.Action != BuildAction.Stl) continue;

                // 신형 DH 형식 (`[ ]` 포함) 도 파싱 가능하도록 괄호 제거 후 split.
                string dhLine = step.DhLines.TrimStart('@').Replace("[", "").Replace("]", "");
                string[] parts = dhLine.Split(',');
                if (parts.Length < 1) continue;
                string stlFilename = parts[0].Trim();

                string stlPath = System.IO.Path.Combine(stlDir ?? "", stlFilename);
                StlBoundingBox bbox;
                try { bbox = StlParser.Parse(stlPath); }
                catch { continue; }
                if (!bbox.IsValid) continue;

                float offX = 0, offY = 0, offZ = 0;
                float stlPan = 0, stlTilt = 0, stlSwing = 0;
                if (parts.Length >= 8)
                {
                    float.TryParse(parts[5], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out offX);
                    float.TryParse(parts[6], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out offY);
                    float.TryParse(parts[7], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out offZ);
                }
                if (parts.Length >= 11)
                {
                    float.TryParse(parts[8], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out stlPan);
                    float.TryParse(parts[9], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out stlTilt);
                    float.TryParse(parts[10], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out stlSwing);
                }

                double epx, epy, epz;
                builder.GetEndpointPosition(i, out epx, out epy, out epz);
                double[,] Re = builder.GetEndpointRotation(i);

                double dxw = wox - epx, dyw = woy - epy, dzw = woz - epz;

                // Re^T * (ray - endpoint) - offset → endpoint 로컬 좌표
                float eox = (float)(Re[0, 0] * dxw + Re[1, 0] * dyw + Re[2, 0] * dzw) - offX;
                float eoy = (float)(Re[0, 1] * dxw + Re[1, 1] * dyw + Re[2, 1] * dzw) - offY;
                float eoz = (float)(Re[0, 2] * dxw + Re[1, 2] * dyw + Re[2, 2] * dzw) - offZ;

                float edx = (float)(Re[0, 0] * wdx + Re[1, 0] * wdy + Re[2, 0] * wdz);
                float edy = (float)(Re[0, 1] * wdx + Re[1, 1] * wdy + Re[2, 1] * wdz);
                float edz = (float)(Re[0, 2] * wdx + Re[1, 2] * wdy + Re[2, 2] * wdz);

                // STL 회전 역변환: Rm^T * (endpoint_local),  Rm = Rz(swing)·Rx(tilt)·Ry(pan)
                float lox = eox, loy = eoy, loz = eoz;
                float ldx = edx, ldy = edy, ldz = edz;
                if (stlSwing != 0 || stlTilt != 0 || stlPan != 0)
                {
                    double[,] Rm = DhBuilder.StlLocalRotation(stlPan, stlTilt, stlSwing);
                    lox = (float)(Rm[0, 0] * eox + Rm[1, 0] * eoy + Rm[2, 0] * eoz);
                    loy = (float)(Rm[0, 1] * eox + Rm[1, 1] * eoy + Rm[2, 1] * eoz);
                    loz = (float)(Rm[0, 2] * eox + Rm[1, 2] * eoy + Rm[2, 2] * eoz);
                    ldx = (float)(Rm[0, 0] * edx + Rm[1, 0] * edy + Rm[2, 0] * edz);
                    ldy = (float)(Rm[0, 1] * edx + Rm[1, 1] * edy + Rm[2, 1] * edz);
                    ldz = (float)(Rm[0, 2] * edx + Rm[1, 2] * edy + Rm[2, 2] * edz);
                }

                int hitTri = StlParser.RaycastMesh(bbox, lox, loy, loz, ldx, ldy, ldz);
                if (hitTri < 0) continue;

                // 히트 거리 계산 (여러 STL 간 비교용)
                int b = hitTri * 9;
                float t = StlParser.RayTriangleIntersect(lox, loy, loz, ldx, ldy, ldz,
                    bbox.Vertices[b], bbox.Vertices[b + 1], bbox.Vertices[b + 2],
                    bbox.Vertices[b + 3], bbox.Vertices[b + 4], bbox.Vertices[b + 5],
                    bbox.Vertices[b + 6], bbox.Vertices[b + 7], bbox.Vertices[b + 8]);

                if (t < best.Distance)
                {
                    best.Distance = t;
                    best.StepIndex = i;
                    best.TriangleIndex = hitTri;
                    best.Bbox = bbox;
                    best.OffX = offX; best.OffY = offY; best.OffZ = offZ;
                    best.Pan = stlPan; best.Tilt = stlTilt; best.Swing = stlSwing;
                }
            }
            return best;
        }

    } // partial class Ojw
}
