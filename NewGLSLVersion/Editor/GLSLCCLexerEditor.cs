using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLCCLexerEditor : EditorWindow
    {
        public static GLSLCCLexerEditor window;

        public UnityEngine.Object originalDoc;
        
        private string originalText;
        private Vector2 leftScroll;
        private Vector2 rightScroll;
        private Vector2 rightScroll1;
        
        private GLSLLexer _lexer;
        private GLSLCCDecompileCore _decompileCore;
        private SAILData _sailData;
        private int _rightViewIndex;
        private static readonly string[] rightViewTabStrings = new[] {"GLSL","SAIL"};
        [MenuItem("Tools/Moonflow/Tools/Misc/HLSLCCLexer #&L")]
        public static void ShowWindow()
        {
            if(!window)window = GetWindow<GLSLCCLexerEditor>("HLSLCCLexer");
        }

        private void OnEnable()
        {
            _decompileCore = new GLSLCCDecompileCore();
            _lexer = new GLSLLexer();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    originalDoc = EditorGUILayout.ObjectField("Original", originalDoc, typeof(Object), true);
                    if(originalDoc!= null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Read"))
                            {
                                originalText = GLSLLexer.ReadText(AssetDatabase.GetAssetPath(originalDoc));
                            }
                            if (GUILayout.Button("Make Token and split to lines"))
                            {
                                _lexer = new GLSLLexer();
                                _lexer.tokens = _lexer.MakeToken(originalText);
                                _decompileCore.originLines = _decompileCore.SplitToLines(ref _lexer.tokens);
                                for (var index = 0; index < _decompileCore.originLines.Count; index++)
                                {
                                    var line = _decompileCore.originLines[index];
                                    _decompileCore.Analyze(ref line);
                                    _decompileCore.originLines[index] = line;
                                }
                            }
                            
                            if(GUILayout.Button("Decompile"))
                            {
                                _sailData = GLSL2SAIL.TransToSAIL(ref _decompileCore.originLines);
                            }
                        }

                        using (var leftView = new EditorGUILayout.ScrollViewScope(leftScroll))
                        {
                            leftScroll = leftView.scrollPosition;
                            EditorGUILayout.TextArea(originalText);
                        }
                    }
                }

                if (_lexer.tokens == null) return;
                using (new EditorGUILayout.VerticalScope())
                {
                    _rightViewIndex = GUILayout.Toolbar(_rightViewIndex, rightViewTabStrings);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        switch (_rightViewIndex)
                        {
                            case 0:
                            {
                                using (var rightView = new EditorGUILayout.ScrollViewScope(rightScroll))
                                {
                                    rightScroll = rightView.scrollPosition;
                                    ShowGLSLTokenResult();
                                }
                                break;
                            }
                            case 1:
                            {
                                using (new EditorGUILayout.VerticalScope())
                                {
                                    if (_sailData != null)
                                    {
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            if (GUILayout.Button("Analyze"))
                                            {
                                                SAILAnalyze.Analyze(ref _sailData);
                                            }
                                        }
                                        using (var rightView = new EditorGUILayout.ScrollViewScope(rightScroll, GUILayout.Height(300)))
                                        {
                                            rightScroll = rightView.scrollPosition;
                                            GUI.color = Color.white;
                                            using (new EditorGUILayout.HorizontalScope())
                                            {
                                                ShowSAILDefinitionResult();
                                            }
                                        }
                                        using (var rightView = new EditorGUILayout.ScrollViewScope(rightScroll1))
                                        {
                                            rightScroll1 = rightView.scrollPosition;
                                            ShowSAILCalculateLineResult();
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            _sailData?.UpdateIntensity(Time.deltaTime);
        }

        private void ShowSAILDefinitionResult()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Input Variables");
                for (var index = 0; index < _sailData.inVar.Count; index++)
                {
                    var inVar = _sailData.inVar[index];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(inVar.tokenType.ToString().ToLower(), GUILayout.Width(100));
                        inVar.tokenString = EditorGUILayout.TextField(inVar.tokenString);
                        if (GUILayout.Button("High Light"))
                        {
                            inVar.Chosen();
                        }
                    }
                }
            }
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Output Variables");
                foreach (var outVar in _sailData.outVar)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(outVar.tokenType.ToString().ToLower(), GUILayout.Width(100));
                        outVar.tokenString = EditorGUILayout.TextField(outVar.tokenString);
                        if (GUILayout.Button("High Light"))
                        {
                            outVar.Chosen();
                        }
                    }
                }
            }
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Global Variables");
                foreach (var glbVar in _sailData.glbVar)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(glbVar.tokenType.ToString().ToLower(), GUILayout.Width(100));
                        glbVar.tokenString = EditorGUILayout.TextField(glbVar.tokenString);
                        if (GUILayout.Button("High Light"))
                        {
                            glbVar.Chosen();
                        }
                    }
                }
            }
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Temp Variables");
                foreach (var tempVar in _sailData.tempVar)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(tempVar.tokenType.ToString().ToLower(), GUILayout.Width(100));
                        tempVar.tokenString = EditorGUILayout.TextField(tempVar.tokenString);
                        if (GUILayout.Button("High Light"))
                        {
                            tempVar.Chosen();
                        }
                    }
                }
            }
        }

        private void ShowSAILCalculateLineResult()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                for (var index = 0; index < _sailData.calculationLines.Count; index++)
                {
                    var line = _sailData.calculationLines[index];
                    using (new EditorGUILayout.HorizontalScope("box"))
                    {
                        EditorGUILayout.LabelField(index.ToString() + (line.isSelfCalculate ? "[○]" : "    "), GUILayout.Width(50));
                        foreach (var hToken in line.hTokens)
                        {
                            float grey = 1 - 0.2f * hToken.layer;
                            GUI.backgroundColor = new Color(grey, grey, grey);
                            GUI.color = SAILEditorUtility.GetTokenTypeColor(hToken.token);
                            if (hToken.token != null)
                            {
                                string showString = (hToken.isNegative ? "-" : "") + hToken.token.ShowString();
                                // if (hToken.token is SAILPieceVariableToken pieceToken)
                                // {
                                //     showString = pieceToken.tokenString + "." + pieceToken.channel;
                                // }
                                if (GUILayout.Button(showString, GUILayout.Width(hToken.token.GetDisplaySize())))
                                {
                                }
                            }

                            GUI.color = Color.white;
                            GUI.backgroundColor = Color.white;
                        }
                    }
                }
            }
        }

        private void ShowGLSLTokenResult()
        {
            foreach (var line in _decompileCore.originLines)
            {
                GUI.backgroundColor = GetGLSLLineTypeColor(line.glslLineType);
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    GUILayout.Button(GetGLSLLineTypeString(line.glslLineType), GUILayout.Width(30));
                    foreach (var token in line.tokens)
                    {
                        GUI.color = GetGLSLTokenTypeColor(token.type);
                        if (GUILayout.Button(token.ShowString(), GUILayout.Width(token.GetDisplaySize())))
                        {
                        }
                    }
                }
                GUI.backgroundColor = Color.white;
            }
        }

        private Color GetGLSLTokenTypeColor(GLSLLexer.GLSLTokenType type)
        {
            switch (type)
            {
                case GLSLLexer.GLSLTokenType.symbol: return Color.white;
                case GLSLLexer.GLSLTokenType.space: return Color.gray;
                case GLSLLexer.GLSLTokenType.macros: return Color.magenta;
                case GLSLLexer.GLSLTokenType.number: return Color.yellow;
                case GLSLLexer.GLSLTokenType.tempDeclarRegex: return Color.cyan;
                case GLSLLexer.GLSLTokenType.storageClass: return Color.grey;
                case GLSLLexer.GLSLTokenType.precise: return new Color(0.2f, 0.6f, 0.9f);
                case GLSLLexer.GLSLTokenType.inputModifier: return new Color(0.8f, 0.5f, 0.2f);
                case GLSLLexer.GLSLTokenType.logicalOperator: return new Color(0.2f, 0.5f, 0.58f);
                case GLSLLexer.GLSLTokenType.semanticRegex: return new Color(0.7f, 0.7f, 0.5f);
                case GLSLLexer.GLSLTokenType.instrFunc: return new Color(0.5f, 0.8f, 0.5f);
                case GLSLLexer.GLSLTokenType.dataType: return new Color(0.8f, 0.5f, 0.6f);
                case GLSLLexer.GLSLTokenType.name: return new Color(0.3f, 0.8f, 0.5f);
                case GLSLLexer.GLSLTokenType.partOfName: return new Color(0.2f, 0.6f, 0.3f);
                case GLSLLexer.GLSLTokenType.unknown: return Color.red;
            }
            return Color.black;
        }
        private Color GetGLSLLineTypeColor(GLSLCCDecompileCore.GLSLLineType type)
        {
            if(type == GLSLCCDecompileCore.GLSLLineType.others) return Color.black;
            return Color.white;
        }
        private string GetGLSLLineTypeString(GLSLCCDecompileCore.GLSLLineType type)
        {
            switch (type)
            {
                case GLSLCCDecompileCore.GLSLLineType.macro: return "[M]";
                case GLSLCCDecompileCore.GLSLLineType.declaration: return "[D]";
                case GLSLCCDecompileCore.GLSLLineType.tempDeclaration: return "[T]";
                case GLSLCCDecompileCore.GLSLLineType.logic: return "[L]";
                case GLSLCCDecompileCore.GLSLLineType.calculate: return "[C]";
                case GLSLCCDecompileCore.GLSLLineType.others: return "[O]";
            }
            return "Unknown";
        }

    }
}