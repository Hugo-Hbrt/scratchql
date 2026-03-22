using System.Globalization;
using scratchql.Core.Parser;

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
            var inputString = "SELECT";
            var lexer = new Lexer(inputString);

            List<Token> expected = new List<Token>()
            {
                new(eTokenType.Select, "SELECT", 1),
                new(eTokenType.Eof, "", 2)
            };

            Assert.That(lexer.Tokenize(), Is.EqualTo(expected));
        }

        [Test]
        public void LowerCaseKeyword()
        {
            var inputString = "select";
            var lexer = new Lexer(inputString);

            List<Token> expected = new List<Token>()
            {
                new(eTokenType.Select, "select", 1),
                new(eTokenType.Eof, "", 2)
            };

            Assert.That(lexer.Tokenize(), Is.EqualTo(expected));
        }

        [TestCase("SeLECt", eTokenType.Select)]
        [TestCase("CreATE", eTokenType.Create)]
        public void MixedCaseKeyword(string input, eTokenType expectedToken)
        {
            var lexer = new Lexer(input);

            List<Token> expected = new List<Token>()
            {
                new(expectedToken, input, 1),
                new(eTokenType.Eof, "", 2)
            };

            Assert.That(lexer.Tokenize(), Is.EqualTo(expected));
        }


        [TestCaseSource(nameof(AllKeywords))]
        public void KeywordGetsTokenizeCorrectly(string input, eTokenType tokenType)
        {
            var lexer = new Lexer(input);

            List<Token> expected = new List<Token>()
            {
                new(tokenType, input, 1),
                new(eTokenType.Eof, "", 2)
            };

            Assert.That(lexer.Tokenize(), Is.EqualTo(expected));
        }
    }
}
