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
        
        private GLSLLexer _lexer;
        private GLSLCCDecompileCore _decompileCore;
        private SAILData _sailData;
        private int _rightViewIndex;
        private static readonly string[] rightViewTabStrings = new[] {"GLSL","SAIL"};
        [MenuItem("Moonflow/Tools/Editor/HLSLCCLexer #&L")]
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
                            }

                            if (GUILayout.Button("Analyze"))
                            {
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
                            if (_sailData != null)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    ShowSAILDefinitionResult();
                                }
                                using (var rightView = new EditorGUILayout.ScrollViewScope(rightScroll))
                                {
                                    rightScroll = rightView.scrollPosition;
                                    ShowSAILCalculateLineResult();
                                }
                            }
                            break;
                        }
                    }
                }
            }
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
                        EditorGUILayout.LabelField(inVar.tokenTypeName, GUILayout.Width(100));
                        inVar.tokenString = EditorGUILayout.TextField(inVar.tokenString);
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
                        EditorGUILayout.LabelField(outVar.tokenTypeName, GUILayout.Width(100));
                        outVar.tokenString = EditorGUILayout.TextField(outVar.tokenString);
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
                        EditorGUILayout.LabelField(glbVar.tokenTypeName, GUILayout.Width(100));
                        glbVar.tokenString = EditorGUILayout.TextField(glbVar.tokenString);
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
                        EditorGUILayout.LabelField(tempVar.tokenTypeName, GUILayout.Width(100));
                        tempVar.tokenString = EditorGUILayout.TextField(tempVar.tokenString);
                    }
                }
            }
        }

        private void ShowSAILCalculateLineResult()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                foreach (var line in _sailData.calculationLines)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    if (line.isSelfCalculate)
                        EditorGUILayout.LabelField("[SelfCalculate]", GUILayout.Width(100));
                    foreach (var hToken in line.hTokens)
                    {
                        float grey = 1 - 0.2f * hToken.layer;
                        GUI.backgroundColor = new Color(grey, grey, grey);
                        GUI.color = SAILEditorUtility.GetTokenTypeColor(hToken.token);
                        if (hToken.token != null)
                        {
                            if (GUILayout.Button(hToken.token.tokenString, GUILayout.Width(SAILEditorUtility.GetTokenTypeWidth(hToken.token))))
                            {
                            }
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void ShowGLSLTokenResult()
        {
            foreach (var line in _decompileCore.originLines)
            {
                GUI.backgroundColor = GetGLSLLineTypeColor(line.glslLineType);
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.Button(GetGLSLLineTypeString(line.glslLineType), GUILayout.Width(30));
                if (line.isSelfCalculate)
                    EditorGUILayout.LabelField("[SelfCalculate]", GUILayout.Width(100));
                foreach (var token in line.tokens)
                {
                    GUI.color = GetGLSLTokenTypeColor(token.type);
                    if (GUILayout.Button(token.tokenString, GUILayout.Width(GetGLSLTokenTypeWidth(token.type))))
                    {
                    }
                }

                EditorGUILayout.EndHorizontal();
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
        private float GetGLSLTokenTypeWidth(GLSLLexer.GLSLTokenType type)
        {
            switch (type)
            {
                case GLSLLexer.GLSLTokenType.symbol: return 20;
                case GLSLLexer.GLSLTokenType.space: return 0;
                case GLSLLexer.GLSLTokenType.macros: return 75;
                case GLSLLexer.GLSLTokenType.number: return 100;
                case GLSLLexer.GLSLTokenType.tempDeclarRegex: return 100;
                case GLSLLexer.GLSLTokenType.storageClass: return 50;
                case GLSLLexer.GLSLTokenType.precise: return 70;
                case GLSLLexer.GLSLTokenType.inputModifier: return 50;
                case GLSLLexer.GLSLTokenType.semanticRegex: return 120;
                case GLSLLexer.GLSLTokenType.instrFunc: return 100;
                case GLSLLexer.GLSLTokenType.dataType: return 50;
                case GLSLLexer.GLSLTokenType.name: return 150;
                case GLSLLexer.GLSLTokenType.partOfName: return 100;
                case GLSLLexer.GLSLTokenType.unknown: return 150;
            }
            return 100;
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