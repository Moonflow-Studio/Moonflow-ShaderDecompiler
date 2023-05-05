namespace moonflow_system.Tools.MFUtilityTools.DXBCLexer;

public enum DXBCTokenType
{
    // inline operators
    LeftParen,			// (
    RightParen,			// )
    LeftBracket,		// [
    RightBracket,		// ]
    Colon,				// :
    Comma,				// ,
    
    // 0 cost operators
    Constant,           // l
    Absolute,           // abs
    Zero,               // _z
    NotZero,            // _nz
    Indexable,          // _indexable
    
    //1 line operators
    Add,				// add
    And,				// and
    DDX,			    // deriv_rtx
    DDY,			    // deriv_rty
    Divide,				// div
    Dot,				// dp2 dp3 dp4
    Exp,				// exp
    Frac,				// frc
    Log,				// log
    MultiAdd,			// mad
    Max,                // max
    Min,                // min
    Move,				// mov
    MoveC,				// movc
    Multiply,			// mul
    Negate,				// neg
    Power,				// pow
    Floor,              // round_ni
    Round,              // round_ne
    Ceil,               // round_pi
    Trunc,              // round_z
    Rsq,				// rsq
    Sincos,				// sincos
    Sqrt,				// sqrt
    

    //int operators
    IAdd,				// iadd
    IMulAdd,			// imad
    IMax,				// imax
    IMin,				// imin
    IMul,				// imul
    INegative,			// ineg
    IShifLeft,			// ishl
    IShifRight,			// ishr

    //uint operators
    UDive,				// udiv
    UMultiAdd,			// umad
    UMax,				// umax
    UMin,				// umin
    UMulti,				// umul
    UShiftRight,		// ushr
    

    //logic operators
    Break,				// break
    BreakCondition,	    // breakc
    Call,				// call
    CallCondition,		// callc
    Case,				// case
    Continue,			// continue
    ContinueCondition,	// continuec
    Default,			// default
    Discard,			// discard
    Else,				// else
    EndIf,				// endif
    EndLoop,			// endloop
    EndSwitch,			// endswitch
    IEqual,             // ieq
    If,					// if
    Label,				// label
    Loop,				// loop
    Not,				// not
    Or,					// or
    Ret,				// ret
    RetC,				// retc
    Switch,				// switch
    
    
    //Transfer operators
    FtoI,				// ftoi
    FtoU,				// ftou
    ItoF,				// itof
    UtoF,				// utof
    
    //Comparison operators
    Equal,				// eq
    GreatEqual,			// ge
    IGreatEqual,		// ige
    ILessThan,			// ilt
    LessThan,			// lt
    UGreatEqual,		// uge
    ULessThan,			// ult
    Xor,				// xor
    
    //sample operators
    Load,				// ld
    LoadFromArray,		// ld2dms
    LOD,				// lod
    Sample,				// sample
    Bias,			    // _b
    Cmp,			    // _c
    LevelZero,	        // _c_lz
    Deriv,		        // _d
    Lod,		        // _l
    SampleInfo,		    // sampleinfo
    SamplePos,		    // samplepos
    
    //declare part
    Dcl,				// dcl
    GlobalFlags,		// _globalFlags
    ConstantBuffer,		// _constantbuffer
    ImmediateConstantBuffer,	// _immediateConstantbuffer
    Input,				// _input
    Output,				// _output
    Sampler,			// _sampler
    Resource,			// _resource
    Texture2D,          // _texture2d
    TextureCube,        // _texturecube
    Buffer,             // _buffer
    Temps,				// _temps
    VertexShader,		// _vs
    PixelShader,		// _ps
    IndexableTemp,		// _indexableTemp
    IndexRange,			// _indexRange
    SV,                 // _sv
    Depth,              // odepth
    SIV,                // _siv
    UAV,                // _uav
    SGV,                // _sgv
    OutputTopology,     // OutputTopology
    
    
    


}