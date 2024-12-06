using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonflow
{
    
    public class CaptureAnalyzer : EditorWindow
    {
        private string _capturePath;
        private DrawcallAnalyzer[] _drawcallAnalyzers;
        private List<HLSLAnalyzer> _hlslAnalyzers;
        private Dictionary<ShaderCodeIdPair, ShaderCodePair> _shaderCodePairs = new Dictionary<ShaderCodeIdPair, ShaderCodePair>();
        
        public Dictionary<int, BufferDeclaration> cbuffers;
        private Vector2Int _drawcallRange;
        [MenuItem("Tools/Moonflow/Utility/Capture Analyzer")]
        public static void ShowWindow()
        {
            var _ins = GetWindow<CaptureAnalyzer>("Capture Analyzer");
            _ins.minSize = new Vector2(200, 300);
            _ins.maxSize = new Vector2(200, 300);
            _ins._drawcallRange = new Vector2Int(0, 20000);
        }

        private void OnDestroy()
        {
            
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(_capturePath);
            if (GUILayout.Button("Select Folder"))
            {
                _capturePath = EditorUtility.OpenFolderPanel("Select RenderDoc Capture Folder", "", "");
            }
            EditorGUILayout.Space();
            _drawcallRange = EditorGUILayout.Vector2IntField("Drawcall Range", _drawcallRange);
            if (GUILayout.Button("Analyze Data"))
            {
                AnalyzeData(_capturePath);
                Debug.Log("Finish!!");
            }

            // using (new EditorGUILayout.VerticalScope("box"))
            // {
            //     using (new EditorGUILayout.HorizontalScope())
            //     {
            //         if (GUILayout.Button("Setup HLSL File"))
            //         {
            //             Debug.Log("starting analyze hlsl code");
            //             SetupHLSLFile();
            //             Debug.Log("Finish!!");
            //         }
            //
            //         if (GUILayout.Button("Analyze HLSL"))
            //         {
            //             Debug.Log("starting analyze hlsl code");
            //             AnalyzeHLSL();
            //             Debug.Log("Finish!!");
            //         }
            //     }
            //
            //     if (_hlslAnalyzers != null)
            //     {
            //         for (int i = 0; i < _hlslAnalyzers.Count; i++)
            //         {
            //             using (new EditorGUILayout.HorizontalScope())
            //             {
            //                 EditorGUILayout.LabelField("Vertex Shader:", _hlslAnalyzers[i].shaderCodePair.id.vsid, GUILayout.Width(200));
            //                 EditorGUILayout.LabelField("Pixel Shader:", _hlslAnalyzers[i].shaderCodePair.id.psid,GUILayout.Width(200));
            //                 if (GUILayout.Button("Analyze"))
            //                 {
            //                     _hlslAnalyzers[i].Analyze();
            //                 }
            //             }
            //         }
            //     }
            // }
            

            if (GUILayout.Button("Save"))
            {
                Save();
            }

            if (GUILayout.Button("Load"))
            {
                Load();
            }
        }

        private void Load()
        {
            foreach (var drawcall in _drawcallAnalyzers)
            {
                drawcall.Translate();
            }
        }

        private void AnalyzeData(string capturePath)
        {
            _shaderCodePairs = new Dictionary<ShaderCodeIdPair, ShaderCodePair>();
            _capturePath = capturePath;
            if (Directory.Exists(_capturePath))
            {
                //读取子文件夹列表
                string[] subFolders = Directory.GetDirectories(capturePath);
                _drawcallAnalyzers = new DrawcallAnalyzer[subFolders.Length];
                Debug.Log($"Recognize {subFolders.Length} drawcall in captures");
                for (int i = 0; i < subFolders.Length; i++)
                {
                    string correctFolder = subFolders[i].Replace('\\', '/');
                    Debug.Log($"Analyze {correctFolder}");
                    _drawcallAnalyzers[i] = new DrawcallAnalyzer();
                    string[] folderSplit = correctFolder.Split('/');
                    int drawcallIndex = int.Parse(folderSplit[^1]);
                    if(drawcallIndex >= _drawcallRange.x && drawcallIndex <= _drawcallRange.y)
                        _drawcallAnalyzers[i].Setup(correctFolder, this);
                }
            }
            else
            {
                Debug.LogError("Capture folder is not found!");
            }

            Debug.Log("Finish setup drawcall data");
        }

        private void SetupHLSLFile()
        {
            _hlslAnalyzers = new List<HLSLAnalyzer>();
            foreach (var codepair in _shaderCodePairs)
            {
                var _HLSLAnalyzer = new HLSLAnalyzer();
                _HLSLAnalyzer.Setup(this, codepair.Value);
                _hlslAnalyzers.Add(_HLSLAnalyzer);
            }
        }
        private void AnalyzeHLSL()
        {
            for (var index = 0; index < _hlslAnalyzers.Count; index++)
            {
                AnalyzeHLSL(index);
            }
        }

        private void AnalyzeHLSL(int index)
        {
            _hlslAnalyzers[index].Analyze();
        }

        private void Save()
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < _drawcallAnalyzers.Length; i++)
            {
                _drawcallAnalyzers[i].Save();
            }

            // for (int i = 0; i < _hlslAnalyzers.Count; i++)
            // {
            //     _hlslAnalyzers[i].SaveAsFile(_capturePath);
            // }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        public void AddShaderFile(ShaderCodePair pair)
        {
            _shaderCodePairs.TryAdd(pair.id, pair);
        }
    }
}