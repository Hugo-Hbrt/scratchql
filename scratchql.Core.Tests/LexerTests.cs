using scratchql.Core.Parser;
using TokenList = System.Collections.Generic.List<(scratchql.Core.Parser.eTokenType, string)>;
namespace scratchql.Core.Tests;

public class LexerTests
{

    [TestFixture]
    public class Keywords
    {
        private static IEnumerable<TestCaseData> AllKeywords()
        {
            Dictionary<string, eTokenType> keywords = new Dictionary<string, eTokenType> {
            {"SELECT", eTokenType.Select},
            {"INSERT", eTokenType.Insert},
            {"UPDATE", eTokenType.Update},
            {"DELETE", eTokenType.Delete},
            {"CREATE", eTokenType.Create},
            {"DROP", eTokenType.Drop},
            {"TABLE", eTokenType.Table},
            {"FROM", eTokenType.From},
            {"WHERE", eTokenType.Where},
            {"AND", eTokenType.And},
            {"OR", eTokenType.Or},
            {"NOT", eTokenType.Not},
            {"INTO", eTokenType.Into},
            {"VALUES", eTokenType.Values},
            {"NULL", eTokenType.Null},
            {"SET", eTokenType.Set}};

            foreach (var entry in keywords)
            {
                yield return new TestCaseData(entry.Key, entry.Value);
            }
        }

        [Test]
        public void SingleKeyword()
        {
            var input = "SELECT";
            TokenList expected;
            TokenList tokens;

            var lexer = new Lexer(input);

            TokenizeToTuple(input, eTokenType.Select, lexer, out expected, out tokens);

            Assert.That(tokens, Is.EqualTo(expected));
        }

        [TestCase("SeLECt", eTokenType.Select)]
        [TestCase("CreATE", eTokenType.Create)]
        [TestCase("create", eTokenType.Create)]
        public void MixedCaseKeyword(string input, eTokenType expectedToken)
        {
            var lexer = new Lexer(input);
            TokenList expected;
            TokenList tokens;

            TokenizeToTuple(input, expectedToken, lexer, out expected, out tokens);

            Assert.That(tokens, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(AllKeywords))]
        public void KeywordGetsTokenizeCorrectly(string input, eTokenType expectedToken)
        {
            var lexer = new Lexer(input);

            TokenList expected;
            TokenList tokens;
            TokenizeToTuple(input, expectedToken, lexer, out expected, out tokens);

            Assert.That(tokens, Is.EqualTo(expected));
        }

    }

    [TestFixture]
    public class Identifiers
    {
        [Test]
        public void SimpleIdentifier()
        {
            var input = "users\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Identifier, "users", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void IdentifierWithLeadingUnderscore()
        {
            var input = "_col_test\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Identifier, "_col_test", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void IdentifierWithDigits()
        {
            var input = "col1\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Identifier, "col1", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void PreservesIdentifierCase()
        {
            var input = "Col1SnakeCase\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Identifier, "Col1SnakeCase", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void WordNotInKeyWordIsIdentifier()
        {
            var input = "BETWEEN\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Identifier, "BETWEEN", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class IntegerLiterals
    {
        [Test]
        public void SingleDigit()
        {
            var input = "1\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.IntLiteral, "1", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [TestCase(42)]
        [TestCase(122)]
        [TestCase(9765)]
        [TestCase(1097821)]
        public void MultiDigit(int inputInteger)
        {
            var input = inputInteger.ToString() + "\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.IntLiteral, inputInteger.ToString(), 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void DigitFollowedByIdentifier()
        {
            var input = "123abc\n";

            TokenList expected = new TokenList()
            {
                (eTokenType.IntLiteral, "123"),
                (eTokenType.Identifier, "abc"),
                (eTokenType.Eof, ""),
            };

            var lexer = new Lexer(input);

            TokenList output = lexer.Tokenize().Select(t => (t.type, t.source)).ToList();

            Assert.That(output, Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class FloatLiterals
    {
        [TestCase("3.14")]
        [TestCase("0.5")]
        [TestCase("100.0")]
        [TestCase("10293.8187325")]
        public void CanParseFloats(string inputFloat)
        {
            var input = inputFloat + "\n";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.FloatLiteral, inputFloat, 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class PositionTracking
    {
        [Test]
        public void FirstTokenStartsAtPosition0()
        {
            var input = "SELECT";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Select, "SELECT", 0);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void PositionReflectAfterWhitespace()
        {
            var input = "  SELECT";

            var lexer = new Lexer(input);
            Token expected = new Token(eTokenType.Select, "SELECT", 2);

            Assert.That(lexer.Tokenize()[0], Is.EqualTo(expected));
        }

        [Test]
        public void EachTokenGetsCorrectPosition()
        {
            var input = "SELECT FROM";

            var lexer = new Lexer(input);
            var expected = new List<Token>()
            {
                new(eTokenType.Select, "SELECT", 0),
                new(eTokenType.From, "FROM", 7),
                new(eTokenType.Eof, "", 11),
            };

            Assert.That(lexer.Tokenize(), Is.EqualTo(expected));
        }
    }
    private static void TokenizeToTuple(string input, eTokenType expectedToken, Lexer lexer, out TokenList expected, out TokenList output)
    {
        expected = new TokenList
            {
                (expectedToken, input),
                (eTokenType.Eof, "")
            };

        output = lexer.Tokenize().Select(t => (t.type, t.source)).ToList();
    }
}
