namespace scratchql.Core.Parser;

public class Lexer
{
    private string _input;
    private readonly Dictionary<string, eTokenType> tokenMapper = new Dictionary<string, eTokenType>(){
            { "SELECT", eTokenType.Select},
            { "INSERT", eTokenType.Insert},
            { "UPDATE", eTokenType.Update},
            { "DELETE", eTokenType.Delete},
            { "CREATE", eTokenType.Create},
            { "DROP", eTokenType.Drop},
            { "TABLE", eTokenType.Table},
            { "FROM", eTokenType.From},
            { "WHERE", eTokenType.Where},
            { "AND", eTokenType.And},
            { "OR", eTokenType.Or},
            { "NOT", eTokenType.Not},
            { "INTO", eTokenType.Into},
            { "VALUES", eTokenType.Values},
            { "NULL", eTokenType.Null},
            { "SET", eTokenType.Set}
        };

    public Lexer(string input)
    {
        _input = input;
    }
    public List<Token> Tokenize()
    {
        eTokenType tokenType;
        tokenMapper.TryGetValue(_input.Split("")[0].ToUpper(), out tokenType);

        return new List<Token>()
        {
            new(tokenType, _input.Split("")[0], 1),
            new(eTokenType.Eof, "", 2)
        };
    }
}
