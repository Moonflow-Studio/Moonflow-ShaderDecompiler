using System.Collections.Generic;
using System.IO;
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

    public void Translate()
    {
    }
}
