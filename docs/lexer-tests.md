# Lexer — Test List

Comprehensive test list derived from [lexer-spec.md](lexer-spec.md).

---

## Keywords

1. Single keyword `SELECT` → `Select, Eof`
2. Lowercase keyword `select` → `Select, Eof` (case insensitivity)
3. Mixed-case keyword `SeLeCt` → `Select, Eof`
4. Keyword `Value` preserves original casing (e.g. `select` not stored as `SELECT`)
5. Every keyword in the §4 table tokenizes correctly: `INSERT`, `UPDATE`, `DELETE`, `CREATE`, `DROP`, `TABLE`, `FROM`, `WHERE`, `AND`, `OR`, `NOT`, `INTO`, `VALUES`, `NULL`, `SET`

## Identifiers

6. Simple identifier `users` → `Identifier(users), Eof`
7. Underscore-leading identifier `_col_1` → `Identifier(_col_1), Eof`
8. Identifier with digits `col1` → `Identifier(col1), Eof`
9. Identifier preserves original case `MyTable` → `Identifier(MyTable)`
10. Word not in keyword list is an identifier, e.g. `BETWEEN` → `Identifier(BETWEEN), Eof`

## Integer literals

11. Single digit `0` → `IntLiteral(0), Eof`
12. Multi-digit `42` → `IntLiteral(42), Eof`
13. Digits followed by identifier `123abc` → `IntLiteral(123), Identifier(abc), Eof`

## Float literals

14. Standard float `3.14` → `FloatLiteral(3.14), Eof`
15. Float starting with zero `0.5` → `FloatLiteral(0.5), Eof`
16. Float with trailing zero `100.0` → `FloatLiteral(100.0), Eof`

## String literals

17. Simple string `'hello'` → `StringLiteral(hello), Eof` (quotes stripped from value)
18. Escaped quote `'it''s'` → `StringLiteral(it's), Eof`
19. String with whitespace `'hello world'` → whitespace preserved in value
20. Empty string `''` → `StringLiteral(), Eof`

## Operators — single character

21. `=` → `Equals, Eof`
22. `<` → `LessThan, Eof`
23. `>` → `GreaterThan, Eof`
24. `+` → `Plus, Eof`
25. `-` → `Minus, Eof`
26. `/` → `Slash, Eof`

## Operators — two character

27. `!=` → `NotEquals, Eof`
28. `<=` → `LessThanOrEqual, Eof`
29. `>=` → `GreaterThanOrEqual, Eof`
30. `<>` → `NotEquals, Eof` (SQL-standard alternative)
31. `<` at end of input → `LessThan, Eof` (not consumed as two-char op)
32. `!` alone (not followed by `=`) → decide: `Unknown`?

## Star

33. `*` in `SELECT *` → `Select, Star, Eof` (separate Star type)

## Punctuation

34. `,` → `Comma, Eof`
35. `;` → `Semicolon, Eof`
36. `(` → `LeftParen, Eof`
37. `)` → `RightParen, Eof`
38. `(id, name)` → `LeftParen, Identifier(id), Comma, Identifier(name), RightParen, Eof`

## Whitespace handling

39. Empty input `""` → `Eof` only
40. Whitespace-only `"   "` → `Eof` only
41. Extra whitespace between tokens `"  SELECT  *  "` → `Select, Star, Eof`
42. Tab and newline as whitespace `"SELECT;\nFROM"` → `Select, Semicolon, From, Eof`
43. Carriage return `\r` treated as whitespace

## Unknown characters

44. `@` → `Unknown(@), Eof`
45. `#` → `Unknown(#), Eof`
46. Unknown char between tokens `SELECT@name` → `Select, Unknown(@), Identifier(name), Eof` — does not throw, processing continues

## EOF token

47. EOF `Value` is the empty string
48. EOF `Position` equals the length of the input
49. EOF is always the last token

## Position tracking

50. First token starts at position 0
51. After whitespace, position reflects post-whitespace offset (e.g. `"  SELECT"` → position 2)
52. Each token in a multi-token input reports correct byte offset
53. Two-char operator position points to first character

## Error handling

54. Unterminated string `'unterminated` → throws `LexerException`
55. `LexerException.Position` reports the position of the opening quote (0 for `'missing at EOF`)
56. `LexerException` message is descriptive

## Composite / integration

57. `SELECT * FROM users` → `Select, Star, From, Identifier(users), Eof`
58. `WHERE id = 1` → `Where, Identifier(id), Equals, IntLiteral(1), Eof`
59. `WHERE id != 1` → `Where, Identifier(id), NotEquals, IntLiteral(1), Eof`
60. `WHERE price <= 9.99` → `Where, Identifier(price), LessThanOrEqual, FloatLiteral(9.99), Eof`
61. `WHERE name = 'Alice'` → `Where, Identifier(name), Equals, StringLiteral(Alice), Eof`
