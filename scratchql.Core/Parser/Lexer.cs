using System.Reflection.Metadata.Ecma335;

namespace scratchql.Core.Parser;

public class Lexer
{
    private const char StringDelimiter = '\'';
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
            { "SET", eTokenType.Set},
            { "=", eTokenType.Equal},
            { "<", eTokenType.LessThan},
            { ">", eTokenType.GreaterThan},
            { "+", eTokenType.Plus},
            { "-", eTokenType.Minus},
            { "/", eTokenType.Slash},
            { "!=", eTokenType.NotEquals},
            { "<=", eTokenType.LessThanOrEqual},
            { ">=", eTokenType.GreaterThanOrEqual},
            { "<>", eTokenType.NotEquals},
            { "*", eTokenType.Star},
            { ",", eTokenType.Comma},
            { ";", eTokenType.Semicolon},
            { "(", eTokenType.LeftParen},
            { ")", eTokenType.RightParen},
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
        eTokenType tokenType;
        if (!IsCommonToken(tokenSource, out tokenType))
        {
            if (IsIntLiteral(tokenSource))
            {
                tokenType = eTokenType.IntLiteral;
            }
            else if (IsFloatLiteral(tokenSource))
            {
                tokenType = eTokenType.FloatLiteral;
            }
            else if (IsStringLiteral(tokenSource))
            {
                tokenType = eTokenType.StringLiteral;
            }
            else if (IsIdentifier(tokenSource))
            {
                tokenType = eTokenType.Identifier;
            }
            else
            {
                tokenType = eTokenType.Unknown;
            }
        }

        return new Token(tokenType, tokenSource, left);
    }

    private bool IsCommonToken(string tokenSource, out eTokenType tokenType)
    {
        return tokenMapper.TryGetValue(tokenSource.ToUpper(), out tokenType);
    }

    private static bool IsIntLiteral(string tokenSource)
    {
        return tokenSource.All(c => char.IsDigit(c));
    }

    private static bool IsFloatLiteral(string tokenSource)
    {
        return tokenSource.All(c => char.IsDigit(c) || c == '.');
    }

    private static bool IsStringLiteral(string tokenSource)
    {
        return tokenSource.StartsWith(StringDelimiter);
    }

    private static bool IsIdentifier(string tokenSource)
    {
        return char.IsAsciiLetter(tokenSource.First()) || tokenSource.First() == '_';
    }

    private string ExtractToken(int left, int length)
    {
        char firstChar = _input[left];

        if (char.IsDigit(firstChar))
        {
            length = DelimitNumber(left, length);
        }
        else if (firstChar == StringDelimiter)
        {
            var (token, tokenLength) = DelimitLiteral(left, length);
            _pos += tokenLength;
            return token;
        }
        else if (IsPunctuation(firstChar))
        {
            length = 1;
            _pos += 1;
            return firstChar.ToString();
        }
        else
        {
            length = DelimitToken(left, length);
        }

        _pos += length;

        return _input.Substring(left, length);
    }

    private (string, int) DelimitLiteral(int left, int length)
    {
        string token = "";

        while (IsInInputLimit(left + length))
        {
            var c = _input[left + length];
            if (IsQuote(c))
            {
                if (IsEscaping(left + length))
                {
                    token += c;

                    // Skip this quote
                    length += 2;
                    continue;
                }

                length += 2;
            }
            else
            {
                token += c;

                length++;
                continue;
            }
        }

        return (StringDelimiter + token + StringDelimiter, length);
    }

    private bool IsEscaping(int charIndex)
    {
        if (charIndex >= _input.Length - 1)
        {
            return false;
        }

        var nextChar = _input[charIndex + 1];
        return nextChar == StringDelimiter;
    }

    private bool IsQuote(char c)
    {
        return c == StringDelimiter;
    }

    private bool IsInInputLimit(int index)
    {
        return index < _input.Length;
    }

    private int DelimitToken(int left, int length)
    {
        while (IsInInputLimit(left + length) && !char.IsWhiteSpace(_input[left + length]) && !IsPunctuation(_input[left + length]))
        {
            length++;
        }

        return length;
    }

    private bool IsPunctuation(char c)
    {
        return "(,;)".Contains(c);
    }

    private int DelimitNumber(int left, int length)
    {
        while (IsInInputLimit(left + length) && (char.IsDigit(_input[left + length]) || _input[left + length] == '.'))
        {
            length++;
        }

        return length;
    }
}
