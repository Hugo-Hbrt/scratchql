using System.Reflection.Metadata.Ecma335;

namespace scratchql.Core.Parser;

public class Lexer
{
    private string _input;
    private int _pos = 0;
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
        var tokens = new List<Token>();
        _pos = 0;

        while (_pos < _input.Length - 1)
        {
            if (!char.IsWhiteSpace(_input[_pos]))
            {
                var token = ParseToken();
                tokens.Add(token);
            }
            else
            {
                _pos++;
            }
        }

        tokens.Add(new Token(eTokenType.Eof, "", _pos));
        return tokens;
    }

    private Token ParseToken()
    {
        int left = _pos;
        int length = 1;

        var tokenSource = ExtractToken(left, length);

        if (!tokenMapper.TryGetValue(tokenSource.ToUpper(), out eTokenType tokenType))
        {
            if (tokenSource.All(c => char.IsDigit(c)))
            {
                tokenType = eTokenType.IntLiteral;
            }
            else
            {
                tokenType = eTokenType.Identifier;
            }

        }

        return new Token(tokenType, tokenSource, left);
    }

    private string ExtractToken(int left, int length)
    {
        char firstChar = _input[left];

        if (char.IsDigit(firstChar))
        {
            length = DelimitNumber(left, length);
        }
        else
        {
            length = DelimitToken(left, length);
        }

        _pos += length;

        return _input.Substring(left, length);
    }

    private int DelimitToken(int left, int length)
    {
        while ((left + length) < _input.Length && !char.IsWhiteSpace(_input[left + length]))
        {
            length++;
        }

        return length;
    }

    private int DelimitNumber(int left, int length)
    {
        while ((left + length) < _input.Length && char.IsDigit(_input[left + length]))
        {
            length++;
        }

        return length;
    }
}
