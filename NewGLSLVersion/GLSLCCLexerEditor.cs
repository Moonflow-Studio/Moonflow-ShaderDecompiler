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
                        }

                        using (var leftView = new EditorGUILayout.ScrollViewScope(leftScroll))
                        {
                            leftScroll = leftView.scrollPosition;
                            EditorGUILayout.TextArea(originalText);
                        }
                    }
                }

                if (_lexer.tokens == null) return;
                using (var rightView = new EditorGUILayout.ScrollViewScope(rightScroll))
                {
                    rightScroll = rightView.scrollPosition;
                    int index = 0;
                    foreach (var line in _decompileCore.originLines)
                    {
                        GUI.backgroundColor = GetLineTypeColor(line.lineType);
                        EditorGUILayout.BeginHorizontal("box");
                        GUILayout.Button(GetLineTypeString(line.lineType), GUILayout.Width(30));
                        if(line.isSelfCalculate)
                            EditorGUILayout.LabelField("[SelfCalculate]", GUILayout.Width(100));
                        foreach (var hTokens in line.hTokens)
                        {
                            GUI.color = GetTokenTypeColor(hTokens.token.type);
                            if(GUILayout.Button(hTokens.token.tokenString, GUILayout.Width(GetTokenTypeWidth(hTokens.token.type)))){}
                        }
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
        }

        private Color GetTokenTypeColor(GLSLLexer.TokenType type)
        {
            switch (type)
            {
                case GLSLLexer.TokenType.symbol: return Color.white;
                case GLSLLexer.TokenType.space: return Color.gray;
                case GLSLLexer.TokenType.macros: return Color.magenta;
                case GLSLLexer.TokenType.number: return Color.yellow;
                case GLSLLexer.TokenType.tempDeclarRegex: return Color.cyan;
                case GLSLLexer.TokenType.storageClass: return Color.grey;
                case GLSLLexer.TokenType.precise: return Color.blue;
                case GLSLLexer.TokenType.inputModifier: return new Color(0.8f, 0.5f, 0.2f);
                case GLSLLexer.TokenType.logicalOperator: return new Color(0.2f, 0.5f, 0.58f);
                case GLSLLexer.TokenType.semanticRegex: return new Color(0.7f, 0.7f, 0.5f);
                case GLSLLexer.TokenType.instrFunc: return new Color(0.5f, 0.8f, 0.5f);
                case GLSLLexer.TokenType.dataType: return new Color(0.8f, 0.5f, 0.6f);
                case GLSLLexer.TokenType.name: return new Color(0.3f, 0.8f, 0.5f);
                case GLSLLexer.TokenType.partOfName: return new Color(0.2f, 0.6f, 0.3f);
                case GLSLLexer.TokenType.unknown: return Color.red;
            }
            return Color.black;
        }
        private float GetTokenTypeWidth(GLSLLexer.TokenType type)
        {
            switch (type)
            {
                case GLSLLexer.TokenType.symbol: return 20;
                case GLSLLexer.TokenType.space: return 0;
                case GLSLLexer.TokenType.macros: return 75;
                case GLSLLexer.TokenType.number: return 100;
                case GLSLLexer.TokenType.tempDeclarRegex: return 100;
                case GLSLLexer.TokenType.storageClass: return 50;
                case GLSLLexer.TokenType.precise: return 50;
                case GLSLLexer.TokenType.inputModifier: return 50;
                case GLSLLexer.TokenType.semanticRegex: return 120;
                case GLSLLexer.TokenType.instrFunc: return 100;
                case GLSLLexer.TokenType.dataType: return 50;
                case GLSLLexer.TokenType.name: return 150;
                case GLSLLexer.TokenType.partOfName: return 100;
                case GLSLLexer.TokenType.unknown: return 150;
            }
            return 100;
        }
        private Color GetLineTypeColor(GLSLCCDecompileCore.LineType type)
        {
            if(type == GLSLCCDecompileCore.LineType.others) return Color.black;
            return Color.white;
        }
        private string GetLineTypeString(GLSLCCDecompileCore.LineType type)
        {
            switch (type)
            {
                case GLSLCCDecompileCore.LineType.macro: return "[M]";
                case GLSLCCDecompileCore.LineType.uniformDeclaration: return "[U]";
                case GLSLCCDecompileCore.LineType.inoutDeclaration: return "[I]";
                case GLSLCCDecompileCore.LineType.tempDeclaration: return "[T]";
                case GLSLCCDecompileCore.LineType.logic: return "[L]";
                case GLSLCCDecompileCore.LineType.calculate: return "[C]";
                case GLSLCCDecompileCore.LineType.others: return "[O]";
            }
            return "Unknown";
        }

    }
}