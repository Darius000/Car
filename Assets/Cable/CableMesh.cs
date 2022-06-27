using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cable
{


    [System.Serializable]
    internal class CableMesh
    {
        public int m_NumSegments;

        public int m_NumSides = 3;

        public float m_Width = 1f;

        public float m_TiledMaterial = 1;

        [SerializeField]
        List<Vector3> vertices = new List<Vector3>();

        [SerializeField]
        List<Vector2> uvs = new List<Vector2>();

        [SerializeField]
        List<int> triangles = new List<int>();

        [SerializeField]
        List<Vector3> normals = new List<Vector3>();

        [SerializeField]
        List<Vector4> tangents = new List<Vector4>();

        Mesh m_Mesh;

        private MeshFilter m_MeshFilter;

        private GameObject m_Parent;

        public CableMesh(int segments, int sides, float width, float tiling, GameObject parent)
        {
            m_NumSegments = segments;
            m_NumSides = sides;
            m_Width = width;
            m_TiledMaterial = tiling;

            m_MeshFilter = parent.AddComponent<MeshFilter>();

            m_Parent = parent;
        }

        public void BuildMesh(Vector3[] points)
        {
            m_Mesh = new Mesh();
            m_Mesh.name = "CableMesh";
            m_Mesh.MarkDynamic();

            CalculateVertices(points, ref vertices,  ref uvs, ref normals, ref tangents, ref triangles);

            m_Mesh.vertices = vertices.ToArray();
            m_Mesh.uv = uvs.ToArray();
            m_Mesh.triangles = triangles.ToArray();
            m_Mesh.normals = normals.ToArray();
            m_Mesh.tangents = tangents.ToArray();

            //m_Mesh.RecalculateNormals();
            m_Mesh.RecalculateTangents();

            m_MeshFilter.mesh = m_Mesh;
        }

        public void ReBuild(Vector3[] points)
        { 

            CalculateVertices(points, ref vertices, ref uvs, ref normals, ref tangents, ref triangles );

            m_Mesh.vertices = vertices.ToArray();
            m_Mesh.normals = normals.ToArray();
            m_Mesh.tangents = tangents.ToArray();

            //m_Mesh.RecalculateNormals();
            m_Mesh.RecalculateTangents();
            //m_Mesh.RecalculateBounds();
        }

        public Mesh GetMesh() { return m_Mesh; }

        int GetVertexCount()
        {
            return (m_NumSides + 1) * (m_NumSegments + 1);
        }

        int GetIndexCount()
        {
            return (m_NumSegments * m_NumSides *2) *3;
        }

        int GetVertexIndex(int alongIndex, int aroundIndex)
        {
            return (alongIndex * (m_NumSides+1)) + aroundIndex;
        }

        void CalculateVertices(Vector3[] points, ref List<Vector3> vertices, 
            ref List<Vector2> texCoord, ref List<Vector3> normals, ref List<Vector4> tangents, ref List<int> triangles)
        {
            vertices.Clear();
            texCoord.Clear();
            triangles.Clear();
            normals.Clear();
            tangents.Clear();

            int numPoints = points.Length;
            int segmentCount = points.Length  -1;

            int numRings = m_NumSides + 1;

            for(int pointIdx = 0; pointIdx < numPoints; pointIdx++)
            {
                Vector3 offset = m_Parent.transform.position;

                int prevIndex = Mathf.Max(0, pointIdx - 1);
                int nextIndex = Mathf.Min(pointIdx + 1, numPoints - 1);

                Vector3 forwardDir = ((points[nextIndex] - offset) - (points[prevIndex] - offset)).normalized;
                Vector3 rightDir =  Vector3.Cross(new Vector3(0, 1 ,0), forwardDir).normalized;
                Vector3 upDir = Vector3.Cross(rightDir, forwardDir).normalized;

                float AlongFrac = (float)pointIdx / (float)segmentCount;

                for(int vertexIdx = 0; vertexIdx < numRings; vertexIdx++)
                {
                    float AroundFrac = (float)vertexIdx / (float)m_NumSides;
                    float radAngle = 2f * Mathf.PI * AroundFrac;

                    Vector3 outDir = (Mathf.Cos(radAngle) * upDir) + (Mathf.Sin(radAngle) * rightDir);

                    var position = (points[pointIdx] - offset) + (outDir * .5f * m_Width);
                    var uv = new Vector2(AlongFrac * m_TiledMaterial, AroundFrac);

                    vertices.Add(position);
                    uvs.Add(uv);
                    normals.Add(outDir);
                    tangents.Add(forwardDir);
                }

                CalulateTriangles(segmentCount, m_NumSides, ref triangles);
            }
        }

        void CalulateTriangles(int segments, int numSides, ref List<int> triangles)
        {
            for (int segIdx = 0; segIdx < segments; segIdx++)
            {
                for (int sideIdx = 0; sideIdx < numSides; sideIdx++)
                {
                    int tl = GetVertexIndex(segIdx, sideIdx);
                    int bl = GetVertexIndex(segIdx, sideIdx + 1);
                    int tr = GetVertexIndex(segIdx + 1, sideIdx);
                    int br = GetVertexIndex(segIdx + 1, sideIdx + 1);

                    triangles.Add(tl);
                    triangles.Add(bl);
                    triangles.Add(tr);

                    triangles.Add(tr);
                    triangles.Add(bl);
                    triangles.Add(br);
                }
            }

        }
    }
}
