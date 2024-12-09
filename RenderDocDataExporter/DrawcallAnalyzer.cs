using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moonflow;
using UnityEditor;
using UnityEngine;

public class DrawcallAnalyzer
{
    private CBufferAnalyzer _cbufferAnalyzer;
    // private HLSLAnalyzer _hlslAnalyzer = new HLSLAnalyzer();
    private TextureAnalyzer _textureAnalyzer = new TextureAnalyzer();
    private MeshInstaller _meshInstaller = new MeshInstaller();
    private string _drawcallFolderPath;
    private string _captureFolderPath;
    private string _translatedPath;
    private CaptureAnalyzer _captureAnalyzer;
    private ShaderCodePair _shaderCodePair;
    private int _diffuseIndex;

    public void Setup(string drawcallFolderPath, CaptureAnalyzer captureAnalyzer)
    {
        _captureAnalyzer = captureAnalyzer;
        _cbufferAnalyzer = ScriptableObject.CreateInstance<CBufferAnalyzer>();
        _drawcallFolderPath = drawcallFolderPath;
        //Create new folder {_drawcallIndex}_Translated for translated files
        _translatedPath = _drawcallFolderPath + "/Translated";
        Directory.CreateDirectory(_translatedPath);
        Debug.Log($"Create Translated Folder:{_translatedPath}");
        AssetDatabase.SaveAssets();
        AnalyzeResources();
        //copy buffer linkers
        List<BufferLinker> linker = new List<BufferLinker>();
        foreach (var item in _cbufferAnalyzer.bufferLinkers)
            linker.Add(item.Clone() is BufferLinker ? (BufferLinker)item.Clone() : default);
        _shaderCodePair.bufferLinkersExample = _cbufferAnalyzer.bufferLinkers;
        _captureAnalyzer.AddShaderFile(_shaderCodePair);
        
    }

    public void Save()
    {
        // get relative path of _translatedPath
        string relativePath = _translatedPath.Substring(Application.dataPath.Length + 1);
        AssetDatabase.CreateAsset(_cbufferAnalyzer, "Assets/"+relativePath + "/CBuffer.asset");
        
        _meshInstaller.SaveMesh(relativePath/*, _textureAnalyzer*/);
    }
    private void AnalyzeResources()
    {
        //get all files from _drawcallFolderPath
        string[] files = Directory.GetFiles(_drawcallFolderPath);
        Debug.Log($"Find {files.Length} files in {_drawcallFolderPath}");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".txt"))
            {
                if (fileName.StartsWith("CBuffer"))
                {
                    _cbufferAnalyzer.AddResource(file);
                }
                else if (fileName.EndsWith("_output_hlsl.txt"))
                {
                    string[] split = fileName.Split("_");
                    if (split[0] == "vs")
                    {
                        _shaderCodePair.vsFilePath = file;
                        _shaderCodePair.id.vsid = split[1];
                    }
                    else if (split[0] == "ps")
                    {
                        _shaderCodePair.psFilePath = file;
                        _shaderCodePair.id.psid = split[1];
                    }
                }
                else if (fileName.EndsWith("VertexIndices.txt") || fileName.EndsWith("VertexInputData.txt"))
                {
                    // _meshInstaller.SetDrawcall(_drawcallFolderPath.Split('/')[^1]);
                    _meshInstaller.AddResource(file);
                }
            }
            else if (fileName.EndsWith(".png"))
            {
                _textureAnalyzer.AddResource(file);
            }
            else
            {
                //They are just a backup and will not be used in any part
            }
        }
    }

    public void Translate(List<HLSLAnalyzer> hlslAnalyzers)
    {
        foreach (var hlslAnalyzer in hlslAnalyzers)
        {
            if (hlslAnalyzer.shaderCodePair.id.vsid == _shaderCodePair.id.vsid && hlslAnalyzer.shaderCodePair.id.psid == _shaderCodePair.id.psid)
            {
                _diffuseIndex = hlslAnalyzer.diffuseResIndex;
            }
        }
        
        bool needInstanciate = false;
        for (int i = 0; i < _cbufferAnalyzer.buffers.Count; i++)
        {
            var data = _cbufferAnalyzer.buffers[i];
            if (data.dec.bufferName == "UnityPerDraw")
            {
                Matrix4x4 m = new Matrix4x4();
                try
                {
                    m.SetColumn(0,new Vector4(data.variables[0].sub[0].sub[0].value, data.variables[0].sub[0].sub[1].value, data.variables[0].sub[0].sub[2].value, data.variables[0].sub[0].sub[3].value));
                    m.SetColumn(1,new Vector4(data.variables[0].sub[1].sub[0].value, data.variables[0].sub[1].sub[1].value, data.variables[0].sub[1].sub[2].value, data.variables[0].sub[1].sub[3].value));
                    m.SetColumn(2,new Vector4(data.variables[0].sub[2].sub[0].value, data.variables[0].sub[2].sub[1].value, data.variables[0].sub[2].sub[2].value, data.variables[0].sub[2].sub[3].value));
                    m.SetColumn(3,new Vector4(data.variables[0].sub[3].sub[0].value, data.variables[0].sub[3].sub[1].value, data.variables[0].sub[3].sub[2].value, data.variables[0].sub[3].sub[3].value));
                    _meshInstaller.SetMatrix(m);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Debug.LogError(_drawcallFolderPath + "没有UnityPerDraw的Buffer或者识别错误导致没有读到obj2World矩阵");
                    return;
                }
                break;
            }
            else if (data.dec.bufferName == "UnityInstancingPerDraw")
            {
                needInstanciate = true;
                Matrix4x4[] matrix4X4s = new Matrix4x4[_meshInstaller.instanceCount];
                for (int j = 0; j < _meshInstaller.instanceCount; j++)
                {
                    Matrix4x4 m = new Matrix4x4();
                    try
                    {
                        //data.variables[0].    sub[j].     sub[0].     sub[0]. sub[0]
                        //     child0.          child0[0].  obj2world.  m1.     x
                        m.SetColumn(0,new Vector4(data.variables[0].sub[j].sub[0].sub[0].sub[0].value, data.variables[0].sub[j].sub[0].sub[0].sub[1].value, data.variables[0].sub[j].sub[0].sub[0].sub[2].value, data.variables[0].sub[j].sub[0].sub[0].sub[3].value));
                        m.SetColumn(1,new Vector4(data.variables[0].sub[j].sub[0].sub[1].sub[0].value, data.variables[0].sub[j].sub[0].sub[1].sub[1].value, data.variables[0].sub[j].sub[0].sub[1].sub[2].value, data.variables[0].sub[j].sub[0].sub[1].sub[3].value));
                        m.SetColumn(2,new Vector4(data.variables[0].sub[j].sub[0].sub[2].sub[0].value, data.variables[0].sub[j].sub[0].sub[2].sub[1].value, data.variables[0].sub[j].sub[0].sub[2].sub[2].value, data.variables[0].sub[j].sub[0].sub[2].sub[3].value));
                        m.SetColumn(3,new Vector4(data.variables[0].sub[j].sub[0].sub[3].sub[0].value, data.variables[0].sub[j].sub[0].sub[3].sub[1].value, data.variables[0].sub[j].sub[0].sub[3].sub[2].value, data.variables[0].sub[j].sub[0].sub[3].sub[3].value));
                        matrix4X4s[j] = m;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Debug.LogError(_drawcallFolderPath + "没有UnityPerDraw的Buffer或者识别错误导致没有读到obj2World矩阵");
                        return;
                    }
                }
                _meshInstaller.SetMatrixes(matrix4X4s);
            }
        }

        if (_textureAnalyzer != null && _diffuseIndex!=0)
        {
            if (_textureAnalyzer.textureDeclarations != null)
            {
                for (int i = 0; i < _textureAnalyzer.textureDeclarations.Count; i++)
                {
                    if (int.Parse(_textureAnalyzer.textureDeclarations[i].resourceIndex) == _diffuseIndex)
                    {
                        _meshInstaller.mat.SetTexture("_BaseMap", _textureAnalyzer.textureDeclarations[i].texture);
                    }
                }
            }
        }
        
        if (!needInstanciate)
        {
            GameObject go = new GameObject(_drawcallFolderPath.Split('/')[^1]);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            filter.mesh = _meshInstaller.GetMeshFile();
            go.transform.position = _meshInstaller.prs[0].position;
            go.transform.rotation = _meshInstaller.prs[0].rotation;
            go.transform.localScale = _meshInstaller.prs[0].scale;
            renderer.material = _meshInstaller.mat;
        }
        else
        {
            GameObject goParent = new GameObject(_drawcallFolderPath.Split('/')[^1]);
            for (int i = 0; i < _meshInstaller.instanceCount; i++)
            {
                GameObject go = new GameObject(_drawcallFolderPath.Split('/')[^1]);
                MeshFilter filter = go.AddComponent<MeshFilter>();
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                filter.mesh = _meshInstaller.GetMeshFile();
                go.transform.position = _meshInstaller.prs[i].position;
                go.transform.rotation = _meshInstaller.prs[i].rotation;
                go.transform.localScale = _meshInstaller.prs[i].scale;
                renderer.material = _meshInstaller.mat;
                go.transform.parent = goParent.transform;
            }
        }
        
        
    }
}
