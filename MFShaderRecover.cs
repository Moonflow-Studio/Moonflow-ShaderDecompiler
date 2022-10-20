
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moonflow.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace moonflow_system.Tools.MFUtilityTools
{
    public class MFShaderRecover:EditorWindow
    {
        public static MFShaderRecover window;
        public static string[] inlineOperation = new[]
        {
            "l",
            "abs"
        };

        //DX version
        public static string[] Operation = new[]
        {
            /*00*/"add", "and",
            /*02*/"break", "breakc",
            /*04*/"call", "callc", "case", "continue", "continuec", "cut",
            /*10*/"default", "deriv_rtx", "deriv_rty", "discard", "div", "dp2", "dp3", "dp4",
            /*18*/"else", "emit", "emitThenCut", "endif", "endloop", "endswitch", "eq", "exp",
            /*26*/"frc", "ftoi", "ftou",
            /*29*/"gather4", "ge",
            /*31*/"iadd", "ieq", "if", "ige", "ilt", "imad", "imax", "imin", "imul", "ine", "ineg", "ishl", "ishr", "itof",
            /*45*/"label", "ld", "ld2dms", "lod", "log", "loop", "lt", 
            /*52*/"mad", "max", "min", "mov", "movc", "mul", 
            /*58*/"ne", "nop", "not", 
            /*61*/"or", 
            /*62*/"resinfo", "ret", "retc",  "round", "round_ne", "round_ni", "round_pi", "round_z", "rsq",
            /*71*/"sample", "sample_b", "sample_c", "sample_c_lz", "sample_d", "sample_l", "sampleinfo", "samplepos", "sincos", "sqrt", "switch", 
            /*82*/"udiv", "uge", "ult", "umad", "umax", "umin", "umul", "ushr", "utof", 
            /*91*/"xor"
        };

        public static string[] enhanceOperation = new[]
        {
            "lerp", 
            "linearstep", 
            "smoothstep",
            "normalize",
            "pow4",
            "matrixMultiply",
            "clamp"
        };
        public enum ShaderType
        {
            Unknown,
            Vertex,
            Geometry,
            Pixel
        }
        public Object oriVertexFile;
        public Object oriFragmentFile;
        public string vertResultText;
        public string fragResultText;
        private ShaderData _resultData;
        private string _original;
        private string[] _definitions;
        // private singleLine[] _resultAlgorithms;
        private ShaderType _type;
        private string[] _definitionTypes;
        // private List<string[]> _properties;
        private int _cbufferCount;
        private shaderPropDefinition[] _tb;
        private shaderPropDefinition[] _tbVert;
        private shaderPropDefinition[] _tbFrag;
        private string _resultProperties;
        private Queue<SingleLine> enhanceAnalyze;
        private string[] _temp;
        private Vector2 _textVertScroll;
        private Vector2 _textFragScroll;
        private Vector2[] _propScroll;
        private Vector2 _vertVarScroll;
        private Vector2 _pixelVarScroll;
        private bool showBuffer;
        private bool _arranged = false;
        private static readonly string[] DEFINITION_TYPE_VERTEX = new[] {"globalFlags", "constantbuffer", "input", "output_siv", "output", "sampler", "resource_texture2d", "resource_texture3d", "temps"};//dcl_input_sgv未知定义
        private static readonly string[] DEFINITION_TYPE_PIXEL = new[] {"globalFlags", "constantbuffer", "input_ps", "input_ps_siv", "output", "sampler", "resource_texture2d", "resource_texture3d", "temps"};
        [MenuItem("Moonflow/Tools/ShaderRecover")]
        public static void ShowWindow()
        {
            if (!window) window = GetWindow<MFShaderRecover>("Shader重构（临时）");
            window.InitResultData();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("该算法需要移除计算行部分由于if/else产生的行序号与运算符之间多余的空格，否则无法分析", new GUIStyle(){fontStyle = FontStyle.Bold, normal = new GUIStyleState(){textColor = Color.white}});
            using (new GUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 50;
                using (new EditorGUILayout.VerticalScope())
                {
                    // EditorGUILayout.LabelField("当前文件类型", ShowType());
                    using (var view = new EditorGUILayout.ScrollViewScope(_textVertScroll, new GUIStyle(){fixedWidth = 600}))
                    {
                        _textVertScroll = view.scrollPosition;
                        vertResultText = EditorGUILayout.TextArea(vertResultText);
                    }
                }
                using (var side = new EditorGUILayout.VerticalScope("Wizard Box"))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        oriVertexFile = EditorGUILayout.ObjectField("Vertex Shader", oriVertexFile, typeof(Object), false) as UnityEngine.Object;
                        oriFragmentFile = EditorGUILayout.ObjectField("Fragment Shader", oriFragmentFile, typeof(Object), false) as UnityEngine.Object;
                        using (new EditorGUILayout.HorizontalScope("box"))
                        {
                            if (GUILayout.Button("还原"))
                            {
                                InitResultData();
                                _type = ShaderType.Vertex;
                                if (ReadShaderFile(oriVertexFile))
                                {
                                    DoRecover(ref _resultData.vert);
                                }

                                _type = ShaderType.Pixel;
                                if (ReadShaderFile(oriFragmentFile))
                                {
                                    DoRecover(ref _resultData.frag);
                                }
                            }
                            if (GUILayout.Button("刷新显示"))
                            {
                                if (_arranged)
                                {
                                    if (oriVertexFile != null)
                                    {
                                        ChangeOperationDisplay(ref _resultData.vert);
                                        CombineDisplay(ref _resultData.vert);
                                        PrintResultData(ref _resultData.vert, ref vertResultText, false);
                                    }

                                    if (oriFragmentFile != null)
                                    {
                                        ChangeOperationDisplay(ref _resultData.frag);
                                        CombineDisplay(ref _resultData.frag);
                                        PrintResultData(ref _resultData.frag, ref fragResultText, false);
                                    }
                                }
                                else
                                {
                                    if (oriVertexFile != null)
                                    {
                                        PrintResultData(ref _resultData.vert, ref vertResultText);
                                    }
                                    if (oriFragmentFile != null)
                                    {
                                        PrintResultData(ref _resultData.frag, ref fragResultText);
                                    }
                                }
                            }
                            if (GUILayout.Button("清空"))
                            {
                                _arranged = false;
                                InitResultData();
                                if (oriVertexFile != null)
                                {
                                    PrintResultData(ref _resultData.vert, ref vertResultText);
                                }
                                if (oriFragmentFile != null)
                                {
                                    PrintResultData(ref _resultData.frag, ref fragResultText);
                                }
                            }
                        }

                        using (new EditorGUILayout.HorizontalScope("box"))
                        {
                            if (GUILayout.Button("智能函数识别"))
                            {
                                if (oriVertexFile != null)
                                {
                                    _type = ShaderType.Vertex;
                                    MergeExceedFunction(ref _resultData.vert);
                                    PrintResultData(ref _resultData.vert, ref vertResultText);
                                }

                                if (oriFragmentFile != null)
                                {
                                    _type = ShaderType.Pixel;
                                    MergeExceedFunction(ref _resultData.frag);
                                    PrintResultData(ref _resultData.frag, ref fragResultText);
                                }
                            }
                            if (GUILayout.Button("寄存器转换分隔"))
                            {
                                if (oriVertexFile != null)
                                {
                                    _type = ShaderType.Vertex;
                                    TempVariableTransSplit(ref _resultData.vert);
                                    PrintResultData(ref _resultData.vert, ref vertResultText);
                                }

                                if (oriFragmentFile != null)
                                {
                                    _type = ShaderType.Pixel;
                                    TempVariableTransSplit(ref _resultData.frag);
                                    PrintResultData(ref _resultData.frag, ref fragResultText);
                                }
                            }

                            if (GUILayout.Button("运算符替换"))
                            {
                                _arranged = true;
                                if (oriVertexFile != null)
                                {
                                    ChangeOperationDisplay(ref _resultData.vert);
                                    PrintResultData(ref _resultData.vert, ref vertResultText, false);
                                }

                                if (oriFragmentFile != null)
                                {
                                    ChangeOperationDisplay(ref _resultData.frag);
                                    PrintResultData(ref _resultData.frag, ref fragResultText, false);
                                }
                            }

                            if (GUILayout.Button("行合并"))
                            {
                                if (oriVertexFile != null)
                                {
                                    CombineDisplay(ref _resultData.vert);
                                    PrintResultData(ref _resultData.vert, ref vertResultText, false);
                                }

                                if (oriFragmentFile != null)
                                {
                                    CombineDisplay(ref _resultData.frag);
                                    PrintResultData(ref _resultData.frag, ref fragResultText, false);
                                }
                            }
                            if (GUILayout.Button("生成临时变量"))
                            {
                                if (oriVertexFile != null)
                                {
                                    _type = ShaderType.Vertex;
                                    MakeTempVariable(ref _resultData.vert);
                                    PrintResultData(ref _resultData.vert, ref vertResultText);
                                }
                                if (oriFragmentFile != null)
                                {
                                    _type = ShaderType.Pixel;
                                    MakeTempVariable(ref _resultData.frag);
                                    PrintResultData(ref _resultData.frag, ref fragResultText);
                                }
                            }
                        }
                    }
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        if (GUILayout.Button(showBuffer ? "Constant Buffer" : "Temp Variable", new GUIStyle("FrameBox"){fixedHeight = 40, alignment = TextAnchor.UpperCenter}))
                        {
                            showBuffer = !showBuffer;
                        }

                        if (showBuffer)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (_propScroll == null || _propScroll.Length != _resultData.properties.Count)
                                {
                                    _propScroll = new Vector2[_resultData.properties.Count];
                                }
                                for (int i = 0; i < _resultData.properties.Count; i++)
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        using (var view = new EditorGUILayout.ScrollViewScope(_propScroll[i], false, true))
                                        {
                                            _propScroll[i] = view.scrollPosition;
                                            EditorGUILayout.LabelField($"Constant Buffer{i}", new GUIStyle("HelpBox"){fixedHeight = 20, alignment = TextAnchor.UpperCenter});
                                            for (int j = 0; j < _resultData.properties[i].Count; j++)
                                            {
                                                _resultData.properties[i][j].name = EditorGUILayout.TextField(_resultData.properties[i][j].name);
                                                // EditorGUILayout.LabelField(resultData.attribute[i].type, resultData.attribute[i].def);
                                            }
                                        
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (var view = new EditorGUILayout.ScrollViewScope(_vertVarScroll, false, true))
                                {
                                    _vertVarScroll = view.scrollPosition;
                                    using (new EditorGUILayout.VerticalScope("box"))
                                    {
                                        EditorGUILayout.LabelField("Vertex", new GUIStyle("FrameBox"){fixedHeight = 25, alignment = TextAnchor.UpperCenter});
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            using (new EditorGUILayout.VerticalScope())
                                            {
                                                for (int i = 0; i < _resultData.tempVertexVar.Count; i++)
                                                {
                                                    EditorGUILayout.LabelField(_resultData.tempVertexVar[i].channel);
                                                }
                                            }
                                            using (new EditorGUILayout.VerticalScope())
                                            {
                                                for (int i = 0; i < _resultData.tempVertexVar.Count; i++)
                                                {
                                                    _resultData.tempVertexVar[i].linkedVar.name =
                                                        EditorGUILayout.TextField(_resultData.tempVertexVar[i].linkedVar
                                                            .name);
                                                }
                                            }
                                        }
                                    }
                                }
                                using (var view = new EditorGUILayout.ScrollViewScope(_pixelVarScroll, false, true))
                                {
                                    _pixelVarScroll = view.scrollPosition;
                                    using (new EditorGUILayout.VerticalScope("box"))
                                    {
                                        EditorGUILayout.LabelField("Pixel", new GUIStyle("FrameBox"){fixedHeight = 25, alignment = TextAnchor.UpperCenter});
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            using (new EditorGUILayout.VerticalScope())
                                            {
                                                for (int i = 0; i < _resultData.tempPixelVar.Count; i++)
                                                {
                                                    EditorGUILayout.LabelField(_resultData.tempPixelVar[i].channel);
                                                }
                                            }
                                            using (new EditorGUILayout.VerticalScope())
                                            {
                                                for (int i = 0; i < _resultData.tempPixelVar.Count; i++)
                                                {
                                                    _resultData.tempPixelVar[i].linkedVar.name =
                                                        EditorGUILayout.TextField(_resultData.tempPixelVar[i].linkedVar
                                                            .name);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // EditorGUILayout.LabelField("Constant Buffer", new GUIStyle("FrameBox"){fixedHeight = 30, alignment = TextAnchor.UpperCenter});
                        
                        // EditorGUILayout.TextArea(_resultProperties);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            EditorGUILayout.LabelField("Attribute", new GUIStyle("FrameBox"){fixedHeight = 30, alignment = TextAnchor.UpperCenter});
                            for (int i = 0; i < _resultData.attribute.Count; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope("box"))
                                {
                                    _resultData.attribute[i].name = EditorGUILayout.TextField(_resultData.attribute[i].name);
                                    EditorGUILayout.LabelField(_resultData.attribute[i].type, _resultData.attribute[i].def);
                                }
                            }
                            // EditorGUILayout.TextArea(_resultAttribute);
                        }
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            EditorGUILayout.LabelField("v2f", new GUIStyle("FrameBox"){fixedHeight = 30, alignment = TextAnchor.UpperCenter});
                            for (int i = 0; i < _resultData.v2f.Count; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope("box"))
                                {
                                    _resultData.v2f[i].name = EditorGUILayout.TextField(_resultData.v2f[i].name);
                                    EditorGUILayout.LabelField(_resultData.v2f[i].type, _resultData.v2f[i].def);
                                }
                            }
                            // EditorGUILayout.TextArea(_resultv2f);
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            EditorGUILayout.LabelField("gbuffer",
                                new GUIStyle("FrameBox") { fixedHeight = 30, alignment = TextAnchor.UpperCenter });
                            for (int i = 0; i < _resultData.gbuffer.Count; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope("box"))
                                {
                                    _resultData.gbuffer[i].name =
                                        EditorGUILayout.TextField(_resultData.gbuffer[i].name);
                                    EditorGUILayout.LabelField(_resultData.gbuffer[i].type, _resultData.gbuffer[i].def);
                                }
                            }
                            // EditorGUILayout.TextArea(_resultGbuffer);
                        }
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            EditorGUILayout.LabelField("tex",
                                new GUIStyle("FrameBox") { fixedHeight = 30, alignment = TextAnchor.UpperCenter });
                            for (int i = 0; i < _resultData.gbuffer.Count; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope("box"))
                                {
                                    _resultData.tex[i].name =
                                        EditorGUILayout.TextField(_resultData.tex[i].name);
                                    EditorGUILayout.LabelField(_resultData.tex[i].type, _resultData.tex[i].def);
                                }
                            }
                            // EditorGUILayout.TextArea(_resultGbuffer);
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    // EditorGUILayout.LabelField("当前文件类型", ShowType());
                    using (var view = new EditorGUILayout.ScrollViewScope(_textFragScroll, new GUIStyle(){fixedWidth = 600}))
                    {
                        _textFragScroll = view.scrollPosition;
                        fragResultText = EditorGUILayout.TextArea(fragResultText);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "设置Shader还原参数");
            }
            
        }

        private void InitResultData()
        {
            _resultData = new ShaderData();
            _resultData.properties = new List<List<shaderPropDefinition>>();
            _resultData.attribute = new List<shaderPropDefinition>();
            _resultData.v2f = new List<shaderPropDefinition>();
            _resultData.gbuffer = new List<shaderPropDefinition>();
            _resultData.tex = new List<shaderPropDefinition>();
            _resultData.tempVertexVar = new List<shaderPropUsage>();
            _resultData.tempPixelVar = new List<shaderPropUsage>();
        }

        private bool ReadShaderFile(Object file)
        {
            try
            {
                _original = File.ReadAllText(AssetDatabase.GetAssetPath(file));
                return true;
            }
            catch (Exception e)
            {
                if (_type == ShaderType.Vertex)
                {
                    MFDebug.DialogError("无法读取Vertex Shader原文件");
                }
                else if (_type == ShaderType.Pixel)
                {
                    MFDebug.DialogError("无法读取Pixel Shader原文件");
                }
                else
                {
                    MFDebug.DialogError("无法读取原文件");
                }
                return false;
            }
        }

        private void DoRecover(ref List<SingleLine> text)
        {
            if (_type == ShaderType.Vertex) _definitionTypes = DEFINITION_TYPE_VERTEX;
            else if (_type == ShaderType.Pixel) _definitionTypes = DEFINITION_TYPE_PIXEL;
            else
            {
                MFDebug.DialogError("暂不支持");
            }

            if (_type == ShaderType.Vertex)
            {
                _tb = _tbVert;
            }else/* if (_type == ShaderType.Pixel)*/
            {
                _tb = _tbFrag;
            }

            SplitStructAndAlgorithm(ref text);
            CleanPrefix(ref text);
            SplitDefination(/*ref text*/);
            AnalyzeAlgorithm(ref text);
            // OptimizeAlgorithm();
            if (_type == ShaderType.Vertex)
            {
                PrintResultData(ref text, ref vertResultText);
            }else if (_type == ShaderType.Pixel)
            {
                PrintResultData(ref text, ref fragResultText);
            }
        }

        // private void OptimizeAlgorithm(ref List<SingleLine> lines)
        // {
        //     // singleLine[] lines = new singleLine[] { };
        //     // if (_type == ShaderType.Vertex) lines = _resultData.vert;
        //     // if (_type == ShaderType.Pixel) lines = _resultData.frag;
        //     //算法合并
        //     
        //     //赋值切割
        //     TempVariableTransSplit(ref lines);
        //     //临时变量替换
        //     MakeTempVariable(ref lines);
        //     //运算符转换
        //     //赋值合并
        // }
        public void ChangeOperationDisplay(ref List<SingleLine> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.elipsised || line.empty) continue;
                switch (line.opIndex)
                {
                    case 0:
                    {
                        if (line.localVar[1].negative)
                        {
                            line.str = $"({line.localVar[0].GetDisplayVar()} - {line.localVar[1].GetDisplayVar(false)})";
                        }
                        else
                        {
                            line.str = $"({line.localVar[0].GetDisplayVar()} + {line.localVar[1].GetDisplayVar()})";
                        }
                    }
                        break;
                    case 1: line.str = $"({line.localVar[0].GetDisplayVar()} && {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 13: line.str = $"(clip({line.result.GetDisplayVar()}))";
                        break;
                    case 14: line.str = $"({line.localVar[0].GetDisplayVar()} / {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 15: line.str = $"(dot({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}))";
                        break;
                    case 16: line.str = $"(dot({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}))";
                        break;
                    case 17: line.str = $"(dot({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}))";
                        break;
                    case 25: line.str = $"(exp({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 26: line.str = $"(frac({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 27: line.str = $"(round({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 30: line.str = $"({line.localVar[0].GetDisplayVar()} >= {line.localVar[1].GetDisplayVar()})";
                        break;
                    // case 31~44(int)
                    case 49: line.str = $"(log2({line.localVar[0].GetDisplayVar()})";
                        break;
                    case 51: line.str = $"({line.localVar[0].GetDisplayVar()} < {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 52:
                        line.str =
                            $"({line.localVar[0].GetDisplayVar()} * {line.localVar[1].GetDisplayVar()} + {line.localVar[2].GetDisplayVar()})";
                        break;
                    case 53:
                        line.str = $"(max({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}))";
                        break;
                    case 54:
                        line.str = $"(min({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}))";
                        break;
                    case 55: line.str = $"{line.localVar[0].GetDisplayVar()}";
                        break;
                    case 56: line.str = $"{line.localVar[0].GetDisplayVar()} ? {line.localVar[1].GetDisplayVar()} : {line.localVar[2].GetDisplayVar()}";
                        break;
                    case 57: line.str = $"({line.localVar[0].GetDisplayVar()} * {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 58: line.str = $"({line.localVar[0].GetDisplayVar()} != {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 61: line.str = $"({line.localVar[0].GetDisplayVar()} | {line.localVar[1].GetDisplayVar()})";
                        break;
                    case 65: line.str = $"(round({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 67: line.str = $"(floor({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 68: line.str = $"(ceil({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 70: line.str = $"(1/sqrt({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 71:
                    case 72:
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                        line.str =
                            $"(SAMPLE_TEXTURE2D({line.localVar[1].linkedVar.name}, sampler_{line.localVar[1].linkedVar.name}, {line.localVar[0].GetDisplayVar()}))";
                        
                        break;
                    case 79: line.str = $"(sincos({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 80: line.str = $"(sqrt({line.localVar[0].GetDisplayVar()}))";
                        break;
                    // case 82~90(uncharted int)
                    case 100:
                        line.str =
                            $"(lerp({line.localVar[0].GetDisplayVar()}, {line.localVar[1].GetDisplayVar()}, {line.localVar[2].GetDisplayVar()}))";
                        break;
                    case 101:
                        // line.str = $"(linearstep())"
                        break;
                    case 102: line.str = $"(smoothstep({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 103: line.str = $"(normalize({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 104: line.str = $"(pow4({line.localVar[0].GetDisplayVar()}))";
                        break;
                    case 105:
                        line.str = $"(mul({line.localVar[1].linkedVar.type} {line.localVar[1].linkedVar.name}, " +
                                   $"{line.localVar[0].GetDisplayVar()}" +
                                   $"))";
                        break;
                    case 106: //clamp
                        break;
                }
                line.opArranged = true;
            }
        }
        public void CombineDisplay(ref List<SingleLine> lines)
        {
            SingleLine last = lines[lines.Count-1];
            for (int i = lines.Count - 2; i >= 0; i--)
            {
                var line = lines[i];
                if (last.empty || last.localVar == null || last.elipsised)
                {
                    last = line;
                    continue;
                }
                if (line.elipsised || line.empty || line.result == null || !line.opArranged) continue;
                
                bool push = true;
                try
                {
                    for (int j = 0; j < last.localVar.Length; j++)
                    {
                        if (ReferenceEquals(last.localVar[j].linkedVar, line.result.linkedVar) && last.localVar[j].channel == line.result.channel && last.opArranged && line.opArranged && (last.opIndex<71 || last.opIndex>78)/*&& last.combineState!=2*/)
                        {
                            string replaced = line.result.GetDisplayVar();
                            last.str = last.str.Replace(replaced, line.str);
                            line.combineState = 1;
                            last.combineState = 2;
                            push = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                if(push)last = line;
            }
        }

        private void MergeExceedFunction(ref List<SingleLine> lines)
        {
            MFShaderExceedFunctionModule exceed = new MFShaderExceedFunctionModule();
            exceed.Merge_Smoothstep(ref lines);
            exceed.Merge_Lerp(ref lines);
            exceed.Merge_Pow4(ref lines);
            exceed.Merge_Clamp(ref lines);
            exceed.Merge_MatrixMultiply(ref lines, ref _resultData.properties);
        }

        private void MakeTempVariable(ref List<SingleLine> lines)
        {
            Queue<SingleLine> sameResult = new Queue<SingleLine>();
            shaderPropUsage lastResult = lines[0].result;
            for (int i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.elipsised) continue;
                if (line.empty)
                {
                    MakeNewTempVar(lastResult, sameResult);
                }else if (lastResult != null)
                {
                    if (line.result != null)
                    {
                        if (!ReferenceEquals(line.result.linkedVar, lastResult.linkedVar) ||
                            !line.result.channel.Equals(lastResult.channel))
                        {
                            MakeNewTempVar(lastResult, sameResult);
                        }   
                    }
                }
                else
                {
                    sameResult.Enqueue(line);
                }

                lastResult = line.result;
            }
            void MakeNewTempVar(shaderPropUsage shaderPropUsage, Queue<SingleLine> singleLines)
            {
                shaderPropUsage temp = new shaderPropUsage();
                temp.additional = true;
                if (_type == ShaderType.Vertex)
                {
                    try
                    {
                        temp.channel = "float" + shaderPropUsage.channel.Length;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    temp.linkedVar = new shaderPropDefinition()
                    {
                        name = "vTemp_" + _resultData.tempVertexVar.Count,
                    };
                    _resultData.tempVertexVar.Add(temp);
                }
                else if (_type == ShaderType.Pixel)
                {
                    temp.channel = "float" + shaderPropUsage.channel.Length;
                    temp.linkedVar = new shaderPropDefinition()
                    {
                        name = "pTemp_" + _resultData.tempPixelVar.Count,
                    };
                    _resultData.tempPixelVar.Add(temp);
                }

                foreach (var l in singleLines)
                {
                    l.result = temp;
                }

                singleLines.Clear();
            }
        }
        
        private void TempVariableTransSplit(ref List<SingleLine> lines)
        {
            SingleLine last = lines[0];
            
            for (int i = 1; i < lines.Count;i++)
            {
                bool needSplit = false;
                bool forceEnd = false;
                SingleLine thisone = lines[i];
                if(thisone.result == null) continue;
                if (thisone.elipsised) continue;
                
                //上一行结果与该行不同
                if (!ReferenceEquals(thisone.result.linkedVar, last.result.linkedVar) ||
                    thisone.result.channel != last.result.channel)
                {
                    //找是不是被该行引用了
                    for (int j = 0; j < thisone.localVar.Length; j++)
                    {
                        //如果被引用了
                        if (ReferenceEquals(thisone.localVar[j].linkedVar, last.result.linkedVar))
                        {
                            //找引用的通道是不是对的上
                            if (IncludedChannelDeliver(last.result.channel, thisone.localVar[j].channel))
                            {
                                //对的上就找下文有没有再次引用
                                for (int k = i + 1; k < lines.Count; k++)
                                {
                                    var line = lines[k];
                                    //如果已经被重新赋值看看通道对不对，对的就完全结束
                                    if (line.result != null &&
                                        ReferenceEquals(line.result.linkedVar, last.result.linkedVar))
                                    {
                                        if (last.result.channel == line.result.channel)
                                        {
                                            forceEnd = true;
                                            break;
                                        }
                                    }
                                    //还没被重新赋值就找是不是重新被引用了而且通道一样
                                    if (line.localVar != null)
                                    {
                                        for (int l = 0; l < line.localVar.Length; l++)
                                        {
                                            if (ReferenceEquals(line.localVar[l].linkedVar, last.result.linkedVar))
                                            {
                                                //重新被引用了而且通道一样而且没有被重新赋值说明需要分离
                                                if (IncludedChannelDeliver(last.result.channel,
                                                        line.localVar[l].channel))
                                                {
                                                    needSplit = true;
                                                    forceEnd = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (forceEnd) break;
                                }
                                if (forceEnd) break;
                            }
                        }
                    }
                    if(!forceEnd)needSplit = true;
                    last = lines[i];
                }
                if (needSplit)
                {
                    lines.Insert(i, new SingleLine(){empty = true});
                    i++;
                    last = lines[i];
                }
            }
        }

        private void AnalyzeAlgorithm(ref List<SingleLine> target)
        {
            // singleLine[] lines = new singleLine[] { };
            // if (_type == ShaderType.Vertex) lines = _resultData.vert;
            // if (_type == ShaderType.Pixel) lines = _resultData.frag;
            for (int i = 0; i < target.Count; i++)
            {
                var singleLine = target[i];
                singleLine.lineIndex = i;
                singleLine.opIndex = -1;
                ProcessSingleLine(ref singleLine);
                target[i] = singleLine;
            }
        }

        private void ProcessSingleLine(ref SingleLine singleLine)
        {
            string[] split = singleLine.str.Split(' ', 2);
            singleLine.selfCal = false;
            singleLine.elipsised = false;

            for (var index = 0; index < Operation.Length; index++)
            {
                var Op = Operation[index];
                if (split[0].Contains(Op))
                {
                    singleLine.opIndex = index;
                }

                if (split[0].Contains("_sat"))
                {
                    singleLine.saturate = true;
                }
                
            }

            if (split.Length > 1)
            {
                Regex r = new Regex(@"\(.*?\)");
                var matches = r.Matches(split[1]);
                for (int i = 0; i < matches.Count; i++)
                {
                    split[1] = split[1].Replace(matches[i].Value, matches[i].Value.Replace(" ", ""));
                }

                split = split[1].Split(", ", StringSplitOptions.RemoveEmptyEntries);
                singleLine.localVar = new shaderPropUsage[split.Length - 1];
                for (int i = 0; i < split.Length; i++)
                {
                    LinkUsage(split, i, ref singleLine);
                }

                RefreshSingleline(ref singleLine);
            }

            void LinkUsage(string[] strings, int i, ref SingleLine singleline)
            {
                shaderPropUsage target = new shaderPropUsage(){channel = "", linkedVar = null, negative = false, inlineOp = -1, additional = false};
                string text = strings[i];
                if (text[0] == '-')
                {
                    target.negative = true;
                    text = text.Substring(1, text.Length - 1);
                }
                if (text.Contains("("))
                {
                    string[] inPropSplit = text.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < inlineOperation.Length; j++)
                    {
                        if (inPropSplit[0] == inlineOperation[j])
                        {
                            target.inlineOp = j;
                        }
                    }
                    text = inPropSplit[1];
                }

                string[] singlesplit = null;
                if (target.inlineOp != 0)
                {
                    singlesplit = text.Split(".");
                    target = DiscernPropSource(singlesplit, target, text);
                    if (singlesplit.Length > 1)
                    {
                        char[] d = singlesplit[singlesplit.Length - 1].Replace("\r", "").Distinct().ToArray();
                        target.channel = string.Join("", d);
                    }
                }
                else
                {
                    target = MakeNewConstant(singleline, text);
                    // singlesplit = text.Split(",");
                    // target.linkedVar = new shaderPropDefinition()
                    // {
                    //     name = $"float{singlesplit.Length.ToString()}",
                    //     type = singlesplit.Length.ToString()
                    // };
                    // target.channel = $"{text}";
                    // target.additional = true;
                }
                
                if (target.linkedVar != null)
                {
                    if (singlesplit.Length > 1 && string.IsNullOrEmpty(target.channel))
                    {
                        char[] d = singlesplit[singlesplit.Length - 1].Replace("\r", "").Distinct().ToArray();
                        target.channel = string.Join("", d);
                    }
                    
                    if (i == 0)
                    {
                        singleline.result = target;
                    }
                    else
                    {
                        singleline.localVar[i - 1] = target;
                        if (target.linkedVar.name == singleline.result.linkedVar.name)
                        {
                            // target.channel.Contains(singleline.result.channel)   
                            if (IncludedChannelDeliver(target.channel, singleline.result.channel))
                            {
                                singleline.selfCal = true;
                            }
                        }
                    }
                }
                else
                {
                    singleline.localVar[i - 1] = target;
                }
            }
        }

        private shaderPropUsage MakeNewConstant(SingleLine singleline, string text)
        {
            int resultCount = singleline.result.channel.Length;
            var singlesplit = text.Split(",");
            bool allSame = true;
            if (resultCount == singlesplit.Length && singlesplit.Length>1)
            {
                for (int i = 1; i < singlesplit.Length; i++)
                {
                    if (singlesplit[i] != singlesplit[0])
                    {
                        allSame = false;
                        break;
                    }
                }
            }

            if (allSame)
            {
                return new shaderPropUsage()
                {
                    negative = singlesplit[0][0] == '-',
                    channel = singlesplit[0].Replace("-",""),
                    inlineOp = 0,
                };
            }
            else
            {
                return new shaderPropUsage()
                {
                    channel = text,
                    inlineOp = 0,
                    additional = true
                };
            }
            // var linked = new shaderPropDefinition()
            // {
            //     name = $"float{singlesplit.Length.ToString()}",
            //     type = singlesplit.Length.ToString()
            // };
            // target.channel = $"{text}";
            // target.additional = true;
        }


        private bool IncludedChannelDeliver(string from, string to, bool completion = false)
        {
            return from == to;
            // char[] fromChar = from.ToCharArray();
            
            // if (completion)
            // {
            //     //xyw -> xyxw
            // }
            // else
            // {
            //     //xyxw -> xyw
            //     if (fromChar.Length != 1 && fromChar.Length != 4)
            //     {
            //         
            //     }
            //     if (fromChar.Length == 1)
            //     {
            //         return to[0].Equals(from[0]);
            //     }
            //     else
            //     {
            //         char[] toChar = to.ToCharArray();
            //         bool allIncluded = true;
            //         for (int i = 0; i < toChar.Length; i++)
            //         {
            //             try
            //             {
            //                 switch (toChar[i])
            //                 {
            //                     case 'x': allIncluded &= fromChar[0] == 'x';
            //                         break;
            //                     case 'y': allIncluded &= fromChar[1] == 'y';
            //                         break;
            //                     case 'z': allIncluded &= fromChar[2] == 'z';
            //                         break;
            //                     case 'w': allIncluded &= fromChar[3] == 'w';
            //                         break;
            //                 }
            //             }
            //             catch (Exception e)
            //             {
            //                 Console.WriteLine(e);
            //                 throw;
            //             }
            //         
            //         }
            //         return allIncluded;
            //     }
            // }

            return false;
        }

        private void RefreshSingleline(ref SingleLine singleLine)
        {
            // if (singleLine.result != null && singleLine.result.linkedVar != null)
            // {
            //     string op = singleLine.opIndex < 100 ? Operation[singleLine.opIndex]: enhanceOperation[singleLine.opIndex - 100];
            //     if (singleLine.result.additional)
            //     {
            //         singleLine.str = $"{singleLine.result.channel} {singleLine.result.linkedVar.name} => {op} ";
            //     }
            //     else
            //     {
            //         singleLine.str =
            //             $"{singleLine.result.linkedVar.name}.{singleLine.result.channel} => {op} ";
            //     }
            // }
            // else
            // {
            //     singleLine.str = "ret";
            // }

            singleLine.str = "";
            if (singleLine.localVar != null)
            {
                for (int i = 0; i < singleLine.localVar.Length; i++)
                {
                    try
                    {
                        singleLine.str += " " + singleLine.localVar[i].GetDisplayVar();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    // if (singleLine.localVar[i].negative)
                    // {
                    //     singleLine.str += "-";
                    // }
                    //
                    // if (singleLine.localVar[i].inlineOp == 0)
                    // {
                    //     if (Convert.ToInt16(singleLine.localVar[i].linkedVar.type) == 1)
                    //     {
                    //         singleLine.str += $"({singleLine.localVar[i].channel.TrimEnd('0')})";
                    //     }
                    //     else
                    //     {
                    //         singleLine.str += $"{singleLine.localVar[i].linkedVar.name}({singleLine.localVar[i].channel})" ;
                    //     }
                    // }
                    // else
                    // {
                    //     singleLine.str += singleLine.localVar[i].linkedVar.name + "." + singleLine.localVar[i].channel;
                    // }
                }
            }

            // singleLine.str += ";\n";
        }

        private shaderPropUsage DiscernPropSource(string[] singlesplit, shaderPropUsage target, string text)
        {
            switch (singlesplit[0][0])
            {
                case 'r': //temp buffer
                    target.linkedVar = _tb[Convert.ToInt16(singlesplit[0].Split("r")[1])];
                    break;
                case 'c': //constant buffer text ->cb0[10].xy
                    string[] cbsplit = text.Split(new[] { "cb", "[", "]." }, StringSplitOptions.RemoveEmptyEntries);
                    int bufferIndex = Convert.ToInt16(cbsplit[cbsplit.Length>3?1:0]);

                    string[] serialSplit = text.Split(new[] { "[", "]." }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        int indexInBuffer =
                            Convert.ToInt16(serialSplit[1]);
                        target.linkedVar = _resultData.properties[bufferIndex][indexInBuffer];
                    }
                    catch (Exception e)
                    {
                        char[] d = serialSplit[2].Distinct().ToArray();
                        target = new shaderPropUsage()
                        {
                            linkedVar = new shaderPropDefinition()
                            {
                                name = $"props_{bufferIndex.ToString()}_[{serialSplit[1]}]",
                            },
                            channel = string.Join("", d),
                            inlineOp = -1
                        };
                    }

                    break;
                case 't': //tex
                    string splitTexDef = singlesplit[0].Replace("t", "");
                    try
                    {
                        target.linkedVar = _resultData.tex[Convert.ToInt16(splitTexDef)];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    break;
                case 's': //sampler 忽略
                    target.linkedVar = new shaderPropDefinition() { name = $"sampler_{singlesplit[0].Replace("s", "")}" };
                    break;
                case 'v': //input vs:attribute ps: v2f
                    if (_type == ShaderType.Vertex)
                    {
                        try
                        {
                            target.linkedVar = _resultData.attribute[Convert.ToInt16(singlesplit[0].Split("v")[1])];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                    else if (_type == ShaderType.Pixel)
                    {
                        target.linkedVar = _resultData.v2f[Convert.ToInt16(singlesplit[0].Split("v")[1])];
                    }

                    break;
                case 'o': //output vs: v2f ps:gbuffer
                    if (_type == ShaderType.Vertex)
                    {
                        target.linkedVar = _resultData.v2f[Convert.ToInt16(singlesplit[0].Split("o")[1])];
                    }
                    else if (_type == ShaderType.Pixel)
                    {
                        target.linkedVar = _resultData.gbuffer[Convert.ToInt16(singlesplit[0].Split("o")[1])];
                    }
                    break;
            }
            return target;
        }

        private void SplitDefination(/*ref List<SingleLine> target*/)
        {
            // if (_type == ShaderType.Vertex) _definitionTypes = DEFINITION_TYPE_VERTEX;
            // else if (_type == ShaderType.Pixel) _definitionTypes = DEFINITION_TYPE_PIXEL;
            // else
            // {
            //     MFDebug.DialogError("暂不支持");
            // }
            string[] temp;
            _cbufferCount = 0;
            for (int i = 0; i < _definitions.Length; i++)
            {
                temp = Regex.Split(_definitions[i], " ");
                CheckDefination(temp);
            }
        }

        private void CheckDefination(string[] tuple)
        {
            if (tuple[0].Equals(_definitionTypes[0]))
            {
                //重新排序标记，可以不译
            }
            if (tuple[0].Equals(_definitionTypes[1]))
            {
                //Constant Buffer
                MakeDef_Constant(tuple[1]);
                _cbufferCount++;
            }
            if (tuple[0].Contains(_definitionTypes[2]))
            {
                //input / input_ps(永远少于vert的output) / input_sgv(没找到资料)
                MakeDef_Input(tuple[1]);
            }
            if (tuple[0].Equals(_definitionTypes[3]))
            {
                // output_siv / input_ps_siv -> SV_POSITION
                MakeDef_SVPosition(tuple[1]);
            }
            if (tuple[0].Equals(_definitionTypes[4]))
            {
                //output -> v2f(vert)/gbuffer(frag)
                MakeDef_Output(tuple[1]);
            }
            if (tuple[0].Equals(_definitionTypes[5]))
            {
                //Sampler，可以不译
            }
            if (tuple[0].Equals(_definitionTypes[6]))
            {
                //texture2d
                MakeDef_Tex(tuple[2].Replace("\r", "").Replace("\n", ""), "2");
            }
            if (tuple[0].Equals(_definitionTypes[7]))
            {
                //texture2d
                MakeDef_Tex(tuple[2].Replace("\r", "").Replace("\n", ""), "3");
            }

            if (tuple[0].Equals(_definitionTypes[8]))
            {
                //重新排序标记，可以不译
                _tb = new shaderPropDefinition[Convert.ToInt16(tuple[1])];
                for (int i = 0; i < _tb.Length; i++)
                {
                    _tb[i] = new shaderPropDefinition()
                    {
                        def = "xyzw",
                        name = $"r{i}",
                        type = ""
                    };
                }
            }
        }

        private void MakeDef_Tex(string name, string demension)
        {
            _resultData.tex ??= new List<shaderPropDefinition>();
            var hasSame = _resultData.tex.Any(t => t.name.Equals(name));
            if(!hasSame)_resultData.tex.Add(new shaderPropDefinition(){name = name, type = $"Texture{demension}D"});
        }

        private void MakeDef_Output(string text)
        {
            switch (_type)
            {
                case ShaderType.Vertex:
                {
                    if (_resultData.gbuffer == null)
                    {
                        _resultData.gbuffer = new List<shaderPropDefinition>();
                    }
                    _resultData.v2f ??= new List<shaderPropDefinition>();
                    // Regex rIndex = new Regex("v().");
                    string temp = text.Split('o')[1];
                    string index = temp.Split('.')[0];
                    // index = text.Split('v')[1].Split('.')[0];
                    // string[] temp = text.Split('.');
                    _resultData.v2f.Add(new shaderPropDefinition()
                    {
                        type = $"float{(temp.Split(new []{'.', '\r'}, StringSplitOptions.RemoveEmptyEntries)[1]).Length.ToString()}",
                        name = $"v2f_{index}",
                        def = $"TEXCOORD{index}",
                        
                    });
                    break;
                }
                case ShaderType.Pixel:
                {
                    if (_resultData.gbuffer == null)
                    {
                        _resultData.gbuffer = new List<shaderPropDefinition>();
                    }
                    string temp = text.Split('o')[1];
                    string index = temp.Split('.')[0];
                    
                    _resultData.gbuffer.Add(new shaderPropDefinition()
                    {
                        type = $"float{(temp.Split(new []{'.', '\r'}, StringSplitOptions.RemoveEmptyEntries)[1]).Length.ToString()}",
                        name = $"gbuffer_{index}",
                        def = $"",
                        
                    });
                    break;
                }
                // default: break;
                case ShaderType.Unknown:
                    break;
                case ShaderType.Geometry:
                    break;
                default:
                    break;
            }
        }

        private void MakeDef_SVPosition(string text)
        {
            switch (_type)
            {
                case ShaderType.Vertex:
                {
                    if (_resultData.v2f == null) _resultData.v2f = new List<shaderPropDefinition>();
                    string temp = text.Split('o')[1];
                    string index = temp.Split('.')[0];
                    _resultData.v2f.Add(new shaderPropDefinition()
                    {
                        type = "float4",
                        name = $"v2f_{index}_sv_position",
                        def = "SV_POSITION",
                    });
                    break;
                }
                case ShaderType.Unknown:
                    break;
                case ShaderType.Geometry:
                    break;
                case ShaderType.Pixel:
                    break;
                default:
                    break;
            }
        }

        private void MakeDef_Input(string text)
        {
            switch (_type)
            {
                case ShaderType.Vertex:
                {
                    if (_resultData.attribute == null) _resultData.attribute = new List<shaderPropDefinition>();
                    string temp = text.Split('v')[1];
                    string index = temp.Split('.')[0];
                    _resultData.attribute.Add(new shaderPropDefinition()
                    {
                        type = $"float{(temp.Split(new []{'.', '\r'}, StringSplitOptions.RemoveEmptyEntries)[1]).Length.ToString()}",
                        name = $"attr_{index}",
                        def = $"TEXCOORD{index}",
                        
                    });
                    break;
                }
                case ShaderType.Pixel:
                {
                    if (_resultData.v2f == null)
                    {
                        _resultData.v2f = new List<shaderPropDefinition>();
                        Regex rIndex = new Regex("v().");
                        string index = rIndex.Match(text).Value;
                        string[] temp = text.Split('.');
                        _resultData.v2f.Add(new shaderPropDefinition()
                        {
                            type = $"float{(temp.Length-1).ToString()}",
                            name = $"attr_{index}",
                            def = $"TEXCOORD{index}",
                        
                        });
                    }
                    break;
                }
                default: break;
            }
        }

        private void MakeDef_Constant(string text)
        {
            string[] split1 = text.Split('[');
            split1 = split1[1].Split(']');
            // Regex r = new Regex("[()],");
            // var result = r.Match(text);
            var newPropertiesList = new List<shaderPropDefinition>();
            for (int i = 0; i < Convert.ToInt32(split1[0]); i++)
            {
                var newDef = new shaderPropDefinition()
                {
                    type = "",
                    name = $"props_{_cbufferCount.ToString()}_{i}",
                    def = ""
                };
                newPropertiesList.Add(newDef);
            }

            if (_resultData.properties == null) _resultData.properties = new List<List<shaderPropDefinition>>();
            string[] index = text.Split('[');
            index = index[0].Split("cb");
            if(_resultData.properties.Count < Convert.ToInt16(index[1]+1))
                _resultData.properties.Add(newPropertiesList);
        }

        private void CleanPrefix(ref List<SingleLine> target)
        {
            List<string> tempDef = _definitions.ToList();
            if (tempDef[0].Contains("vs"))
            {
                _type = ShaderType.Vertex;
            }else if (tempDef[0].Contains("ps"))
            {
                _type = ShaderType.Pixel;
            }
            tempDef.RemoveAt(0);
            _definitions = tempDef.ToArray();
            string[] tempSplit;
            for (int i = 0; i < target.Count; i++)
            {
                tempSplit = Regex.Split(target[i].str, ": ");
                if (tempSplit.Length == 2)
                {
                    target[i].str = tempSplit[1];
                }
            }
        }

        private void PrintResultData(ref List<SingleLine> target, ref string text, bool defaultMode = true)
        {
            text = "";
            if (target?.Count > 0)
            {
                for (int i = 0; i < target.Count; i++)
                {
                    var singleLine = target[i];
                    if (singleLine.empty)
                    {
                        text += "\n";
                        continue;
                    }
                    if(defaultMode)RefreshSingleline(ref singleLine);
                    if (!singleLine.elipsised && singleLine.combineState!=1)
                    {
                        if (singleLine.result != null && singleLine.result.linkedVar != null)
                        {
                            if (defaultMode)
                            {
                                string op = singleLine.opIndex < 100 ? Operation[singleLine.opIndex]: enhanceOperation[singleLine.opIndex - 100];
                                if (singleLine.result.additional)
                                {
                                    text += $"{singleLine.result.channel} {singleLine.result.linkedVar.name} => {op} ";
                                }
                                else
                                {
                                    text +=
                                        $"{singleLine.result.linkedVar.name}.{singleLine.result.channel} => {op} ";
                                }
                            }
                            else
                            {
                                text += $"{singleLine.result.GetDisplayVar()} = ";
                            }
                        }
                        else
                        {
                            text += "ret";
                        }
                        text += singleLine.str + ";\n";
                    }
                    else
                    {
                        // text += "\n";
                    }
                    target[i] = singleLine;
                }
            }
        }

        private string ShowType()
        {
            if (_type == null) return "Unknown";
            switch (_type)
            {
                case ShaderType.Vertex: return "Vertex Shader";
                case ShaderType.Geometry: return "Geometry Shader";
                case ShaderType.Pixel: return "Pixel Shader";
            }
            return "Unknown";
        }

        private void SplitStructAndAlgorithm(ref List<SingleLine> target)
        {
            // _temp = Regex.Split(original, "0: ");
            _temp = _original.Split("0: ", 2);
            _definitions = Regex.Split(_temp[0], "      dcl_");
            var m = Regex.Split(_temp[1], "\n");
            target = new List<SingleLine>();
            for (int i = 0; i < m.Length; i++)
            {
                var newTarget = new SingleLine();
                newTarget.str = m[i];
                target.Add(newTarget);
            }
        }
    }

    public struct ShaderData
    {
        public List<List<shaderPropDefinition>> properties;
        public List<shaderPropDefinition> attribute;
        // public List<shaderPropDefinition> v2g;
        // public List<shaderPropDefinition> g2f;
        public List<shaderPropDefinition> v2f;
        public List<shaderPropDefinition> gbuffer;
        public List<shaderPropDefinition> tex;
        public List<SingleLine> vert;
        public string geometry;
        public List<SingleLine> frag;
        public List<shaderPropUsage> tempVertexVar;
        public List<shaderPropUsage> tempPixelVar;
    }

    public class shaderPropDefinition
    {
        public string type;
        public string name;
        public string def;
    }

    public class SingleLine
    {
        public int lineIndex;
        public string str;
        public shaderPropUsage result;
        public int opIndex;
        public bool selfCal;
        public bool saturate;
        public bool elipsised;
        public int combineState;//0:没动 1:省略了 2：合并了
        
        public bool empty;
        public shaderPropUsage[] localVar;
        public bool opArranged;
    }

    public class shaderPropUsage
    {
        public shaderPropDefinition linkedVar;
        public string channel;
        public bool negative;
        public int inlineOp;
        public bool additional;

        public string GetDisplayVar(bool needNegative = true)
        {
            string result = "";
            if (negative && needNegative) result += "-";
            if (inlineOp == -1)
            {
                result += $"{linkedVar.name}.{channel}";
            }else if (inlineOp == 0)
            {
                if (linkedVar == null)
                {
                    if (channel.Contains('.'))
                    {
                        result += channel.TrimEnd('0').TrimEnd('.');
                    }
                    else
                    {
                        result += channel;
                    }
                }
                else
                {
                    string[] split = channel.Split(",");
                    var single= "";
                    for (int i = 0; i < split.Length; i++)
                    {
                        split[i] = split[i].TrimEnd('0').TrimEnd('.');
                        single += $"{split[i]}";
                        if (i < split.Length - 1)
                        {
                            single += ",";
                        }
                    }
                    // channel += ")";
                    result += $"{linkedVar.name}({single})";
                }
            }else if (inlineOp == 1)
            {
                result += $"abs({channel})";
            }

            return result;
        }
    }
}