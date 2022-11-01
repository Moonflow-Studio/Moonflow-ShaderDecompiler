using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEstablish : MonoBehaviour
{
    public enum eInputSource
    {
        Null,
        TEXCOORD0,
        TEXCOORD1,
        TEXCOORD2,
        TEXCOORD3,
        TEXCOORD4,
        TEXCOORD5,
        TEXCOORD6,
        TEXCOORD7,
        TEXCOORD8,
        TEXCOORD9,
        TEXCOORD10,
        TEXCOORD11,
        TEXCOORD12,
        TEXCOORD13,
        TEXCOORD14,
        TEXCOORD15,
        SV_POSITION,
        in_NORMAL0,
        in_TANGENT0,
        COLOR,
        in_TEXCOORD0,
        in_TEXCOORD1,
        in_TEXCOORD2,
        in_TEXCOORD3,
        ATTRIBUTE0,
        ATTRIBUTE1,
        ATTRIBUTE2,
        ATTRIBUTE3,
        ATTRIBUTE4,
        ATTRIBUTE5,
        ATTRIBUTE6,
        ATTRIBUTE7,
        ATTRIBUTE8,
        ATTRIBUTE9,
        ATTRIBUTE10,
        ATTRIBUTE11,
        ATTRIBUTE12,
        ATTRIBUTE13,
        ATTRIBUTE14,
        ATTRIBUTE15,
        
    }
    public struct VertexData
    {
        public int m_index;
        public Vector3 m_position;
        public Vector3 m_normal;
        public Vector4 m_tangent;
        public Color m_color;
        public Vector2 m_uv0;
        public Vector2 m_uv1;
        public Vector2 m_uv2;
        public Vector2 m_uv3;
        public Vector2 m_uv4;


        public VertexData(string text, int positionIdx, int normalIdx, int tangentIdx, int colorIdx, int uv0Idx, int uv1Idx, int uv2Idx, int uv3Idx, int uv4Idx)
        {
            string[] cell = MeshEstablish.Split(text);

            m_index = int.Parse(cell[1]);

            m_position = (positionIdx > 0) ? new Vector3(float.Parse(cell[positionIdx]), float.Parse(cell[positionIdx + 1]), float.Parse(cell[positionIdx + 2])) : Vector3.zero;
            m_normal = (normalIdx > 0) ? new Vector3(float.Parse(cell[normalIdx]), float.Parse(cell[normalIdx + 1]), float.Parse(cell[normalIdx + 2])) : Vector3.zero;
            m_tangent = (tangentIdx > 0) ? new Vector4(float.Parse(cell[tangentIdx]), float.Parse(cell[tangentIdx + 1]), float.Parse(cell[tangentIdx + 2]), float.Parse(cell[tangentIdx + 3])) : Vector4.zero;
            m_color = (colorIdx > 0) ? new Color(float.Parse(cell[colorIdx]), float.Parse(cell[colorIdx + 1]), float.Parse(cell[colorIdx + 2]), float.Parse(cell[colorIdx + 3])) : Color.black;
            m_uv0 = (uv0Idx > 0) ? new Vector2(float.Parse(cell[uv0Idx]), 1 - float.Parse(cell[uv0Idx + 1])) : Vector2.zero;
            m_uv1 = (uv1Idx > 0) ? new Vector2(float.Parse(cell[uv1Idx]), 1 - float.Parse(cell[uv1Idx + 1])) : Vector2.zero;
            m_uv2 = (uv2Idx > 0) ? new Vector2(float.Parse(cell[uv2Idx]), 1 - float.Parse(cell[uv2Idx + 1])) : Vector2.zero;
            m_uv3 = (uv3Idx > 0) ? new Vector2(float.Parse(cell[uv3Idx]), 1 - float.Parse(cell[uv3Idx + 1])) : Vector2.zero;
            m_uv4 = (uv3Idx > 0) ? new Vector2(float.Parse(cell[uv4Idx]), 1 - float.Parse(cell[uv4Idx + 1])) : Vector2.zero;
        }
    }

    public TextAsset m_textAsset;

    public eInputSource m_position;
    public eInputSource m_normal;
    public eInputSource m_tangent;
    public eInputSource m_color;
    public eInputSource m_uv0;
    public eInputSource m_uv1;
    public eInputSource m_uv2;
    public eInputSource m_uv3;
    public eInputSource m_uv4;

    public Material m_material;


    public static string[] Split(string text)
    {
        return text.Replace(" ", "").Replace("\t", "").Split(',');
    }

    int GetInfoIdx(string[] firstLineCell, eInputSource inputSrc)
    {
        if (inputSrc == eInputSource.Null)
            return -1;
        for(int i = 1; i < firstLineCell.Length; i++)
        {
            if (firstLineCell[i] == inputSrc.ToString() + ".x")
                return i;
        }
        return -1;
    }

    [ContextMenu("Execute")]
    void Execute()
    {
        string[] lines = m_textAsset.text.Split('\n');
        Dictionary<int, VertexData> m_vertexData = new Dictionary<int, VertexData>();
        List<int> triangles = new List<int>();
        int minVertId = int.MaxValue;
        int maxVertId = int.MinValue;
        int vertCount = 0;

        string[] firstLineCell = Split(lines[0]);
        int positionIdx = GetInfoIdx(firstLineCell, m_position);
        int normalIdx = GetInfoIdx(firstLineCell, m_normal);
        int tangentIdx = GetInfoIdx(firstLineCell, m_tangent);
        int colorIdx = GetInfoIdx(firstLineCell, m_color);
        int uv0Idx = GetInfoIdx(firstLineCell, m_uv0);
        int uv1Idx = GetInfoIdx(firstLineCell, m_uv1);
        int uv2Idx = GetInfoIdx(firstLineCell, m_uv2);
        int uv3Idx = GetInfoIdx(firstLineCell, m_uv3);
        int uv4Idx = GetInfoIdx(firstLineCell, m_uv4);

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Length <= 1)
                continue;
            VertexData vd = new VertexData(lines[i], positionIdx, normalIdx, tangentIdx, colorIdx, uv0Idx, uv1Idx, uv2Idx, uv3Idx, uv4Idx);
            triangles.Add(vd.m_index);
            if (!m_vertexData.ContainsKey(vd.m_index))
                m_vertexData.Add(vd.m_index, vd);
            maxVertId = Mathf.Max(maxVertId, vd.m_index);
            minVertId = Mathf.Min(minVertId, vd.m_index);
        }
        vertCount = maxVertId - minVertId + 1;
        for(int i = 0; i < triangles.Count; i++)
        {
            triangles[i] -= minVertId;
        }

        Vector3[] verts = new Vector3[vertCount];
        Vector3[] norms = new Vector3[vertCount];
        Vector4[] tangs = new Vector4[vertCount];
        Color[] colors = new Color[vertCount];
        Vector2[] uv0 = new Vector2[vertCount];
        Vector2[] uv1 = new Vector2[vertCount];
        Vector2[] uv2 = new Vector2[vertCount];
        Vector2[] uv3 = new Vector2[vertCount];
        Vector2[] uv4 = new Vector2[vertCount];

        for(int i = minVertId; i <= maxVertId; i++)
        {
            VertexData vd = m_vertexData[i];
            verts[i - minVertId] = vd.m_position;
            norms[i - minVertId] = vd.m_normal;
            tangs[i - minVertId] = vd.m_tangent;
            colors[i - minVertId] = vd.m_color;
            uv0[i - minVertId] = vd.m_uv0;
            uv1[i - minVertId] = vd.m_uv1;
            uv2[i - minVertId] = vd.m_uv2;
            uv3[i - minVertId] = vd.m_uv3;
            uv4[i - minVertId] = vd.m_uv4;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        if (normalIdx > 0)
            mesh.normals = norms;
        if (tangentIdx > 0)
            mesh.tangents = tangs;
        if (colorIdx > 0)
            mesh.colors = colors;
        if (uv0Idx > 0)
            mesh.uv = uv0;
        if (uv1Idx > 0)
            mesh.uv2 = uv1;
        if (uv2Idx > 0)
            mesh.uv3 = uv2;
        if (uv3Idx > 0)
            mesh.uv4 = uv3;
        if (uv4Idx > 0)
            mesh.uv5 = uv4;

        GameObject go = new GameObject("Export");
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = m_material;
    }
}
