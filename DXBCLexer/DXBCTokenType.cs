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
    Constant,            // l
    Absolute,            // abs
    
    //1 line operators
    Add,				// +
    And,				// &
    DerivRtx,			// ddx
    DerivRty,			// ddy
    Divide,				// /
    Dot,				// dp2 dp3 dp4
    Exp,				// exp
    Frac,				// frc
    Log,				// log
    MultiAdd,			// mad
    Max,                // max
    Min,                // min
    Move,				// mov
    
    //int operators
    IAdd,				// iadd
    IMulAdd,				// imad
    IMax,				// imax
    IMin,				// imin
    IMul,				// imul
    INegative,				// ineg
    IShifLeft,				// ishl
    IShifRight,				// ishr


    //logic operators
    Break,				// break
    BreakC,				// breakc
    Call,				// call
    CallC,				// callc
    Case,				// case
    Continue,			// continue
    ContinueC,			// continuec
    Default,			// default
    Discard,			// discard
    Else,				// else
    EndIf,				// endif
    EndLoop,			// endloop
    EndSwitch,			// endswitch
    IEqual,                // ieq
    If,					// if
    Label,				// label
    Loop,				// loop
    
    
    //Transfer operators
    FtoI,				// ftoi
    FtoU,				// ftou
    ItoF,				// itof
    UtoF,				// utof
    
    //Comparison operators
    GreatEqual,			// ge
    IGreatEqual,		// ige
    ILessThan,			// ilt
    LessThan,			// lt
    
    //sample operators
    Load,				// ld
    LoadFromArray,		// ld2dms
    LOD,				// lod
    
    
}