using System.IO;
using System.Text;
using Moonflow;
using UnityEditor;
using UnityEngine;

public class DrawcallAnalyzer
{
    private CBufferAnalyzer _cbufferAnalyzer;
    private HLSLAnalyzer _hlslAnalyzer = new HLSLAnalyzer();
    private MeshInstaller _meshInstaller = new MeshInstaller();
    private string _drawcallFolderPath;
    private string _captureFolderPath;
    private string _translatedPath;

    public void Setup(string drawcallFolderPath)
    {
        _cbufferAnalyzer = ScriptableObject.CreateInstance<CBufferAnalyzer>();
        _drawcallFolderPath = drawcallFolderPath;
        //Create new folder {_drawcallIndex}_Translated for translated files
        _translatedPath = _drawcallFolderPath + "/Translated";
        Directory.CreateDirectory(_translatedPath);
        Debug.Log($"Create Translated Folder:{_translatedPath}");
        AssetDatabase.SaveAssets();
        AnalyzeResources();
    }

    public void Save()
    {
        // get relative path of _translatedPath
        string relativePath = _translatedPath.Substring(Application.dataPath.Length + 1);
        AssetDatabase.CreateAsset(_cbufferAnalyzer, "Assets/"+relativePath + "/CBufferAnalyzer.asset");
    }
    private void AnalyzeResources()
    {
        //get all files from _drawcallFolderPath
        string[] files = Directory.GetFiles(_drawcallFolderPath);
        Debug.Log($"Find {files.Length} files in {_drawcallFolderPath}");
        foreach (string file in files)
        {
            if (file.EndsWith(".txt"))
            {
                if (file.StartsWith("CBuffer"))
                {
                    _cbufferAnalyzer.AddResource(file);
                }
                else if (file.EndsWith("_output.txt"))
                {
                    _hlslAnalyzer.AddResource(file);
                }
                else if (file.EndsWith("VertexIndices.txt") || file.EndsWith("VertexInputData.txt"))
                {
                    _meshInstaller.AddResource(file);
                }
            }else if (file.EndsWith(".png"))
            {
                _hlslAnalyzer.AddResource(file);
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
