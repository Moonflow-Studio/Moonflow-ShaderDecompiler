using System.Collections.Generic;

namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLLexer
    {
        public List<GLSLToken> tokens;

        public enum GLSLTokenType
        {
            symbol,
            space,
            macros,
            number,
            tempDeclarRegex,
            storageClass,
            precise,
            inputModifier,
            semanticRegex,
            logicalOperator,
            instrFunc,
            dataType,
            name,
            unknown,
            partOfName
        }
        public static readonly char[] symbolStrings = new []{/*'#',*/ '(', ')', ',', ';', '{', '}', '=', '+', '-', '*', '/', '%', '&', '|', '^', '!', '~', '<', '>', '?', ':',/* '[', ']', '.'*/};
        public static readonly char[] spaceStrings = new []{' ', '\t', '\n', '\r', '\f'};
        public static readonly string[] macroStrings = new []{"#define", "#undef", "#if", "#ifdef", "#ifndef", "#else", "#elif", "#endif", "#include", "#pragma", "#line", "#version", "#extension"};
        public static readonly char[] numberChars = new []{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'e', '-'};
        public static readonly string tempDeclarStrings = "u_xlat";
        public static readonly string storageClassString = "inline";
        public static readonly string[] preciseStrings = new []{"lowp", "mediump", "highp"};

        #region Arguments
        public static readonly string[] InputModifierStrings = new []{"in", "out", "inout", "uniform"};

        public static readonly string[] SemanticsRegexStrings = new[]
        {
            "BINORMAL[0-9]", "BLENDINDICES[0-9]", "BLENDWEIGHT[0-9]", "COLOR[0-9]", "NORMAL[0-9]", "POSITION[0-9]",
            "POSITIONT", "PSIZE[0-9]", "TANGENT[0-9]", "TEXCOORD[0-9]", "TESSFACTOR[0-9]", "VFACE", "VPOS",
            "SV_ClipDistance[0-9]", "SV_CullDistance[0-9]", "SV_Coverage", "SV_Depth", "SV_DepthGreaterEqual",
            "SV_DepthLessEqual", "SV_DispatchThreadID", "SV_DomainLocation", "SV_GroupID", "SV_GroupIndex",
            "SV_GroupThreadID",
            "SV_GSInstanceID", "SV_InnerCoverage", "SV_InsideTessFactor[0-9]", "SV_InstanceID", "SV_IsFrontFace",
            "SV_OutputControlPointID", "SV_Position", "SV_PrimitiveID", "SV_RenderTargetArrayIndex", "SV_SampleIndex",
            "SV_StencilRef", "SV_Target[0-9]", "SV_TessFactor[0-9]", "SV_VertexID", "SV_ViewportArrayIndex"
        };
        #endregion

        public static readonly string[] logicalOperatorStrings = new[] {"if","else","discard","return","break","continue","for"};
        public static readonly string[] InstrinsicFunctionStrings = new[]
        {
            "abort", "abs", "acos", "all", "any", "asin", "asint", "asuint", "atan", "atan2", "ceil", "clamp", "clip",
            "cos", "cosh", "countbits", "cross", "ddx", "ddx_coarse", "ddx_fine", "ddy", "ddy_coarse", "ddy_fine",
            "degrees", "determinant", "distance", "dot", "dst", "errorf", "exp", "exp2", "faceforward", "firstbithigh",
            "firstbitlow", "floor", "fma", "fmod", "fract", "frexp", "fwidth", "inversesqrt","isfinite", "isinf", "isnan", "ldexp",
            "length", "lerp", "lit", "log", "log10", "log2", "mad", "max", "min", "modf", "msad4", "mul", "normalize",
            "pow", "radians", "rcp", "reflect", "refract", "reversebits", "round", "rsqrt", "saturate", "sign", "sin",
            "sincos", "sinh", "smoothstep", "sqrt", "step", "tan", "tanh", "texture", "transpose",
            "trunc", "InterlockedAdd", "InterlockedAnd", "InterlockedCompareExchange", "InterlockedCompareStore",
            "InterlockedExchange", "InterlockedMax", "InterlockedMin", "InterlockedOr", "InterlockedXor", "frac",
            "noise", "textureLod" 
        };

        public static readonly string[] dataTypes = new[]
        {
            //scalar type
            "bool", "int", "uint", "float", "double",
            //Vector type
            "vec2", "vec3", "vec4",
            "bvec2", "bvec3", "bvec4", "ivec2", "ivec3", "ivec4", "uvec2", "uvec3", "uvec4", "dvec2", "dvec3", "dvec4",
            //matrix
            "mat2", "mat3", "mat4",
            //sampler
            "sampler1D", "sampler2D", "sampler3D", "samplerCube", "sampler1DShadow", "sampler2DShadow",
            "samplerCubeShadow", "sampler1DArray", "sampler2DArray", "sampler1DArrayShadow", "sampler2DArrayShadow",
            "sampler2DMS", "sampler2DMSArray", "samplerCubeArray", "samplerCubeArrayShadow",
        };
        
        public static string ReadText(string path)
        {
            string text = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
            {
                text = sr.ReadToEnd();
            }
            return text;
        }
        
        public List<GLSLToken> MakeToken(string text)
        {
            string[] tokenStrings = SplitTextBySpace(text);
            List<GLSLToken> tokens = Tokenize(tokenStrings);
            // foreach (var token in tokens)
            // {
            //     UnityEngine.Debug.Log(token.symbol + " " + token.tokenString);
            // }

            return tokens;
        }

        private string[] SplitTextBySpace(string text)
        {
            List<string> tokenStrings = new List<string>();
            int index = 0;
            while (index < text.Length)
            {
                if (IsSymbol(text[index]) || IsSpace(text[index]))
                {
                    tokenStrings.Add(text[index].ToString());
                    index++;
                }
                else
                {
                    string tokenString = "";
                    while (index < text.Length && !IsSpace(text[index]) && !IsSymbol(text[index]))
                    {
                        tokenString += text[index];
                        index++;
                    }
                    tokenStrings.Add(tokenString);
                }
            }
            return tokenStrings.ToArray();
        }


        public List<GLSLToken> Tokenize(string[] tokenstrings)
        {
            List<GLSLToken> tokens = new List<GLSLToken>();
            bool isNegativeSymbol = false;
            bool lastIsNumber = false;
            for (int i = 0; i < tokenstrings.Length; i++)
            {
                string tokenString = tokenstrings[i];
                if (tokenString[0] == '-')
                {
                    if ((i + 1) < tokenstrings.Length && !IsSpace(tokenstrings[i + 1][0]))
                    {
                        if (i > 0 && lastIsNumber)
                        {
                            // tokens[^1].tokenString += "-";
                            lastIsNumber = true;
                        }
                        else
                        {
                            isNegativeSymbol = true;
                        }
                    }
                    // else
                    // {
                    //     tokens.Add(new GLSLToken(GLSLTokenType.symbol, tokenString));
                    //     lastIsNumber = false;
                    // }
                }
                else
                {
                    if (IsSymbol(tokenString[0]))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.symbol, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsSpace(tokenString[0]))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.space, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsMacro(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.macros, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsNumber(tokenString))
                    {
                        if (lastIsNumber)
                        {
                            tokens[^1].tokenString += tokenString;
                        }
                        else
                        {
                            tokens.Add(new GLSLToken(GLSLTokenType.number,
                                (isNegativeSymbol ? "-" : "") + tokenString));
                            isNegativeSymbol = false;
                            lastIsNumber = true;
                        }
                    }
                    else if (IsTempDeclar(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.tempDeclarRegex, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsStorageClass(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.storageClass, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsPrecise(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.precise, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsInputModifier(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.inputModifier, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsSemanticRegex(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.semanticRegex, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsLogicalOperator(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.logicalOperator, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsInstrinsicFunction(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.instrFunc, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsDataType(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.dataType, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsName(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.name, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else if (IsPartOfName(tokenString))
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.partOfName, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    else
                    {
                        tokens.Add(new GLSLToken(GLSLTokenType.unknown, tokenString, isNegativeSymbol));
                        lastIsNumber = false;
                    }
                    isNegativeSymbol = false;
                }
            }
            return tokens;
        }

        private bool IsLogicalOperator(string tokenString)
        {
            for (int i = 0; i < logicalOperatorStrings.Length; i++)
            {
                if (tokenString == logicalOperatorStrings[i]) return true;
            }
            return false;
        }

        private bool IsPartOfName(string tokenString)
        {
            //tokenString seems like .xyz
            // if (tokenString[0] == '.')
            // {
                for (int i = 1; i < tokenString.Length; i++)
                {
                    if (tokenString[i] == '_' 
                        || (tokenString[i] >= 'a' && tokenString[i] <= 'z') 
                        || (tokenString[i] >= 'A' && tokenString[i] <= 'Z') 
                        || (tokenString[i] >= '0' && tokenString[i] <= '9')
                        || tokenString[i] == '['|| tokenString[i] == ']'
                        || tokenString[i] == '.') continue;
                    else return false;
                }
                return true;
            // }
            // else
            // {
            //     return false;
            // }
        }

        private bool IsName(string tokenString)
        {
            if (tokenString[0] == '_' || (tokenString[0] >= 'a' && tokenString[0] <= 'z') || (tokenString[0] >= 'A' && tokenString[0] <= 'Z'))
            {
                for (int i = 1; i < tokenString.Length; i++)
                {
                    if (tokenString[i] == '_' 
                        || (tokenString[i] >= 'a' && tokenString[i] <= 'z') 
                        || (tokenString[i] >= 'A' && tokenString[i] <= 'Z') 
                        || (tokenString[i] >= '0' && tokenString[i] <= '9')
                        || tokenString[i] == '['|| tokenString[i] == ']') continue;
                    else return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsDataType(string tokenString)
        {
            for (int i = 0; i < dataTypes.Length; i++)
            {
                if (tokenString == dataTypes[i]) return true;
            }
            return false;
        }

        private bool IsInstrinsicFunction(string tokenString)
        {
            for (int i = 0; i < InstrinsicFunctionStrings.Length; i++)
            {
                if (tokenString == InstrinsicFunctionStrings[i]) return true;
            }
            return false;
        }

        private bool IsSemanticRegex(string tokenString)
        {
            for (int i = 0; i < SemanticsRegexStrings.Length; i++)
            {
                //SemanticsRegexStrings[i] is a regex string
                if (System.Text.RegularExpressions.Regex.IsMatch(tokenString, SemanticsRegexStrings[i])) return true;
            }
            return false;
        }

        private bool IsInputModifier(string tokenString)
        {
            for (int i = 0; i < InputModifierStrings.Length; i++)
            {
                if (tokenString == InputModifierStrings[i]) return true;
            }
            return false;
        }

        private bool IsPrecise(string tokenString)
        {
            for (int i = 0; i < preciseStrings.Length; i++)
            {
                if (tokenString == preciseStrings[i]) return true;
            }
            return false;
        }

        private bool IsStorageClass(string tokenString)
        {
            return tokenString == storageClassString;
        }

        private bool IsTempDeclar(string tokenString)
        {
            return tokenString.Contains(tempDeclarStrings);
        }

        private bool IsNumber(string tokenString)
        {
            //tokenstring seems like -0.0001
            if (tokenString[0] == '-' || tokenString[0] == '+')
            {
                for (int i = 1; i < tokenString.Length; i++)
                {
                    if (tokenString[i] == '.') continue;
                    for (int j = 0; j < numberChars.Length; j++)
                    {
                        if (tokenString[i] == numberChars[j]) break;
                        if (j == numberChars.Length - 1) return false;
                    }
                }
                return true;
            }
            else
            {
                for (int i = 0; i < tokenString.Length; i++)
                {
                    if (tokenString[i] == '.') continue;
                    for (int j = 0; j < numberChars.Length; j++)
                    {
                        if (tokenString[i] == numberChars[j]) break;
                        if (j == numberChars.Length - 1) return false;
                    }
                }
                return true;
            }
        }

        private bool IsMacro(string tokenString)
        {
            for (int i = 0; i < macroStrings.Length; i++)
            {
                if (tokenString == macroStrings[i]) return true;
            }
            return false;
        }


        private bool IsSpace(char tokenstrings)
        {
            for (int i = 0; i < spaceStrings.Length; i++)
            {
                if (tokenstrings == spaceStrings[i]) return true;
            }
            return false;
        }

        private bool IsSymbol(char tokenstrings)
        {
            for (int i = 0; i < symbolStrings.Length; i++)
            {
                if (tokenstrings == symbolStrings[i]) return true;
            }
            return false;
        }
    }
}