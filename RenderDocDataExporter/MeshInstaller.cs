using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonflow
{
    public class MeshInstaller : IResourceReceiver
    {
        // private string _drawcallIndex;
        public int instanceCount;
        public PRSReverseAux.PRS[] prs;
        public Material mat;
        private int _diffuseIndex;
        private Mesh _mesh;
        private List<Vector4>[] _vertexDataList;
        private int[] _vertexDataChannel;
        private List<int> _vertexIndices = new List<int>();
        private string _path;
        private string _savePath;
        private int _startVertexOffset = 0;
        private static readonly int BASE_MAP = Shader.PropertyToID("_BaseMap");

        public void AddResource(string path)
        {
            _path = path;
            if (path.EndsWith("VertexIndices.txt"))
            {
                _vertexIndices = new List<int>();
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        int index = int.Parse(line);
                        _vertexIndices.Add(index);
                    }
                }
            }else if (path.EndsWith("VertexInputData.txt"))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                {
                    int lineIndex = 0;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (lineIndex == 0)
                        {
                            var title = line.Split(' ');
                            _vertexDataList = new List<Vector4>[title.Length-1];
                            _vertexDataChannel = new int[title.Length - 1];
                        }
                        else if (lineIndex == 1)
                        {
                            instanceCount = int.Parse(line);
                        }
                        else if (lineIndex == 2)
                        {
                            
                        }
                        else if(lineIndex >= 3)
                        {
                            var attrSplit = line.Split("  ");
                            if (lineIndex == 3)
                            {
                                try
                                {
                                    _startVertexOffset = int.Parse(attrSplit[0]);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }
                            }
                            for (int i = 1; i < attrSplit.Length; i++)
                            {
                                var attr = attrSplit[i].Remove(attrSplit[i].Length-1,1).Remove(0,1).Split(", ");
                                if (lineIndex == 3)
                                {
                                    try
                                    {
                                        _vertexDataChannel[i - 1] = attr.Length;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        throw;
                                    }
                                    
                                }
                                Vector4 data = Vector4.zero;
                                if (attr.Length > 0)
                                {
                                    data.x = float.Parse(attr[0]);
                                    if (attr.Length > 1)
                                    {
                                        data.y = float.Parse(attr[1]);
                                        if (attr.Length > 2)
                                        {
                                            data.z = float.Parse(attr[2]);
                                            if (attr.Length > 3)
                                                data.w = float.Parse(attr[3]);
                                        }
                                    }
                                }
                                if (_vertexDataList[i - 1] == null)
                                    _vertexDataList[i - 1] = new List<Vector4>();
                                _vertexDataList[i - 1].Add(data);
                            }
                        }
                        lineIndex++;
                    }
                }
            }
        }

        public void SetDiffuseIndex(int index)
        {
            _diffuseIndex = index;
        }

        private bool MakeMesh()
        {
            if (_vertexDataChannel == null)
            {
                Debug.LogError($"{_path} cannot find vertex datachannel");
            }
            int findObjPos = -1;
            int findUV = -1;
            int findecolor = -1;
            for (int i = 0; i < _vertexDataChannel.Length; i++)
            {
                if (_vertexDataChannel[i] == 2 && findUV < 0)
                {
                    findUV = i;
                }

                if (_vertexDataChannel[i] == 3 && findObjPos < 0)
                {
                    findObjPos = i;
                }

                if (_vertexDataChannel[i] == 4 && findecolor < 0)
                {
                    findecolor = i;
                }

                if (findObjPos >= 0 && findUV >= 0)
                {
                    break;
                }
            }

            if (findObjPos < 0)
            {
                Debug.LogError($"{_path} cannot combine to mesh because no object position(no 3 channel) attribute could be found");
                return false;
            }
            
            _mesh = new Mesh();
            var verticesArray = _vertexDataList[findObjPos].ToArray();
            Vector3[] verticesArray3 = new Vector3[verticesArray.Length];
            for (int i = 0; i < verticesArray3.Length; i++)
            {
                verticesArray3[i] = verticesArray[i];
            }
            _mesh.SetVertices(verticesArray3, 0, verticesArray3.Length, MeshUpdateFlags.DontValidateIndices);
            
            try
            {
                for (int i = 0; i < _vertexIndices.Count; i++)
                {
                    _vertexIndices[i] -= _startVertexOffset;
                }
                // _mesh.SetTriangles(_vertexIndices, 0,true,0);
                _mesh.SetIndices(_vertexIndices,MeshTopology.Triangles,0,true,0);
                // _mesh.triangles = _vertexIndices.ToArray();
                // _mesh.SetIndexBufferParams(_vertexIndices.Count, IndexFormat.UInt32);
                // _mesh.SetIndexBufferData(_vertexIndices, 0, 0, _vertexIndices.Count, MeshUpdateFlags.DontValidateIndices);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError($"Error Drawcall {_path}");
                return false;
            }
            _mesh.RecalculateNormals();
            if (findUV > 0)
            {
                var uvArray = _vertexDataList[findUV].ToArray();
                Vector2[] uvArray2 = new Vector2[uvArray.Length];
                for (int i = 0; i < uvArray2.Length; i++)
                {
                    uvArray2[i] = new Vector2(uvArray[i].x, 1 - uvArray[i].y);
                }
                _mesh.uv = uvArray2;
            }

            if (findecolor > 0)
            {
                var colorArray = _vertexDataList[findecolor].ToArray();
                Color[] color4 = new Color[colorArray.Length];
                for (int i = 0; i < color4.Length; i++)
                {
                    color4[i] = new Color(colorArray[i].x, colorArray[i].y, colorArray[i].z, colorArray[i].w);
                }

                _mesh.colors = color4;
            }

            return true;
        }

        public void SaveMesh(string relativePath, bool blendMode, CullMode cullMode)
        {
            if (MakeMesh())
            {
                _savePath = "Assets/" + relativePath + "/mesh.asset";
                Shader lit = Shader.Find("Universal Render Pipeline/Unlit");
                mat = new Material(lit);
                if (blendMode)
                {
                    mat.SetFloat("_Surface", 1);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.SetFloat("_Blend", 1);
                    mat.SetFloat("_DstBlend", 10);
                    mat.SetFloat("_DstBlendAlpha", 10);
                    mat.SetFloat("_SrcBlend", 5);
                    mat.SetFloat("_SrcBlendAlpha", 1);
                    mat.renderQueue = 3000;
                }
                else
                {
                    mat.SetFloat("_Surface", 0);
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.SetFloat("_DstBlend", 0);
                    mat.SetFloat("_DstBlendAlpha", 0);
                    mat.SetFloat("_SrcBlend", 1);
                    mat.SetFloat("_SrcBlendAlpha", 1);
                    mat.renderQueue = 2000;
                }

                switch (cullMode)
                {
                    case CullMode.Back:
                    {
                        mat.SetFloat("_Cull", 2);
                        break;
                    }
                    case CullMode.Front:
                    {
                        mat.SetFloat("_Cull", 1);
                        break;
                    }
                    case CullMode.Off:
                    {
                        mat.SetFloat("_Cull", 0);
                        break;
                    }
                }
                AssetDatabase.CreateAsset(mat, "Assets/" + relativePath + "/material.mat");
                AssetDatabase.CreateAsset(_mesh, _savePath);
            }
        }

        public void SetMatrix(Matrix4x4 prsMatrix)
        {
            prs = new PRSReverseAux.PRS[1];
            prs[0] = PRSReverseAux.GetPRS(prsMatrix);
        }

        public void SetMatrixes(Matrix4x4[] prsMatrix)
        {
            prs = new PRSReverseAux.PRS[prsMatrix.Length];
            for (int i = 0; i < prs.Length; i++)
            {
                prs[i] = PRSReverseAux.GetPRS(prsMatrix[i]);
            }
        }

        
        // public void SetDrawcall(string drawcallIndex)
        // {
        //     _drawcallIndex = drawcallIndex;
        // }
        public Mesh GetMeshFile()
        {
            return AssetDatabase.LoadAssetAtPath<Mesh>(_savePath);
        }
    }
}