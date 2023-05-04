namespace moonflow_system.Tools.MFUtilityTools.DXBCLexer;

public struct DXBCToken
{
    public DXBCTokenType Type;
    public int Index;
    public int Line;
    public int LineIndex;
    public int Length;

    public string Cut(string content) => Length <= 0 ? "" : content.Substring(Index, Length);
}