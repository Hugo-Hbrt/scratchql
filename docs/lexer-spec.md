# ClearDB — Lexer specification

| Field | Value |
|---|---|
| Version | 1.0 — initial |
| Status | Draft |
| Component | `ClearDB.Core / Parser / Lexer.cs` |
| Dependencies | None — pure C#, no NuGet packages |

---

## 1. Purpose

The Lexer (also called a tokenizer or scanner) is the first processing stage of the ClearDB query pipeline. It accepts a raw SQL string and produces a flat, ordered list of tokens. Each token pairs a type (keyword, identifier, literal, operator, punctuation, or end-of-input) with the original substring and its byte position in the input.

Downstream components — the Parser, and indirectly the Planner and Executor — consume only the token list. They never read the raw SQL string again. This clean boundary means the Lexer can be developed, tested, and replaced independently.

> **Design principle:** The Lexer has no knowledge of grammar. It never decides whether `SELECT` is in a valid position. It only recognises shapes: reserved words, names, numbers, strings, symbols. Grammar is the Parser's responsibility.

---

## 2. Scope

### 2.1 In scope (v1)

| Token type | Examples | Notes | In scope v1 |
|---|---|---|---|
| Keyword | `SELECT` `FROM` `WHERE` `INSERT` `INTO` `VALUES` `CREATE` `TABLE` `DROP` `AND` `OR` `NOT` `NULL` | Case-insensitive. Full list in §4. | Yes |
| Identifier | `users` `table_name` `col1` `_private` | Letter or underscore start; alphanumeric or underscore body. | Yes |
| Int literal | `0` `42` `1000` | Digits only. Sign handled as unary op by the parser. | Yes |
| Float literal | `3.14` `0.5` `100.0` | Digits, one dot, digits. | Yes |
| String literal | `'hello'` `'it''s'` | Single-quoted. Escaped by doubling: `''` → `'` | Yes |
| Operators | `=` `!=` `<` `>` `<=` `>=` `<>` `+` `-` `*` `/` | Two-character ops scanned as one token. | Yes |
| Punctuation | `,` `;` `(` `)` | Each character is its own token. | Yes |
| Star | `*` | Separate type — avoids ambiguity with multiply. | Yes |
| Unknown | `@` `#` `£` `~` | Unrecognised character: emit `Unknown`, do not throw. | Yes |
| EOF | *(end of input)* | Always the last token in the list. | Yes |

### 2.2 Out of scope (v1)

- Multi-line comments (`/* … */`)
- Single-line comments (`-- …`)
- Dollar-quoted strings (`$$…$$`)
- Bit-string and hex literals (`B'1010'`, `X'FF'`)
- Unicode escape sequences (`U&'\0041'`)
- Named parameters (`@param`, `:name`)
- Full operator set for future phases (`BETWEEN`, `LIKE`, `IS`, `IN`) — keywords are tokenized now, operator logic comes later

---

## 3. Token model

Each token is an immutable value type carrying three fields:

| Field | Type | Description |
|---|---|---|
| `Type` | `TokenType` (enum) | The category of this token (`Select`, `Identifier`, `IntLiteral`, `Equals`, etc.) |
| `Value` | `string` | The verbatim substring from the input. Always present, never null. |
| `Position` | `int` | Zero-based byte offset of the first character of this token in the input string. |

In C# the model is expressed as a record for structural equality — essential for clean assertions in unit tests:

```csharp
public enum TokenType
{
    // Keywords
    Select, Insert, Update, Delete, Create, Drop, Table,
    From, Where, And, Or, Not, Null, Into, Values, Set,
    // Literals & identifiers
    Identifier, IntLiteral, FloatLiteral, StringLiteral,
    // Operators
    Equals, NotEquals, LessThan, GreaterThan,
    LessThanOrEqual, GreaterThanOrEqual,
    Plus, Minus, Star, Slash,
    // Punctuation
    Comma, Semicolon, LeftParen, RightParen,
    // Control
    Unknown, Eof
}

public record Token(TokenType Type, string Value, int Position);
```

---

## 4. Keyword list (v1)

The following reserved words must be recognised case-insensitively. Any word not in this list that matches the identifier pattern is emitted as an `Identifier` token.

| | | | |
|---|---|---|---|
| `SELECT` | `INSERT` | `UPDATE` | `DELETE` |
| `CREATE` | `DROP` | `TABLE` | `FROM` |
| `WHERE` | `AND` | `OR` | `NOT` |
| `INTO` | `VALUES` | `NULL` | `SET` |

---

## 5. Behavioural rules

### 5.1 Whitespace

- Spaces, tabs (`\t`), carriage returns (`\r`), and newlines (`\n`) are silently skipped between tokens.
- Whitespace inside a string literal is preserved verbatim.
- The `Position` field always reflects the offset after whitespace is consumed.

### 5.2 Case sensitivity

- Keywords are case-insensitive. `SELECT`, `select`, and `Select` all produce `TokenType.Select`.
- Identifiers and string literal values preserve their original case.
- `Value` on a keyword token stores the original casing from the input, not a normalised form.

### 5.3 String literals

- Delimited by single quotes: `'hello world'`
- Escaped by doubling the quote character: `'it''s fine'` → value `it's fine`
- An unterminated string (reaching end of input before the closing quote) must throw `LexerException` with a descriptive message and the start position.
- `Value` is stored without the surrounding quote characters.

### 5.4 Numeric literals

- **Integer:** one or more decimal digits, no leading zeros except bare `0`.
- **Float:** digits, exactly one dot, digits. Leading or trailing dot (e.g. `.5` or `5.`) is not supported in v1.
- No sign prefix — a leading minus is tokenized as a separate `Minus` token; sign-folding is the parser's job.

### 5.5 Two-character operators

The following operator pairs must be scanned as a single token, not two:

```
!=   →  NotEquals
<=   →  LessThanOrEqual
>=   →  GreaterThanOrEqual
<>   →  NotEquals   (SQL-standard alternative)
```

A single `<` or `>` not followed by `=` or `>` is emitted as `LessThan` or `GreaterThan` respectively.

### 5.6 Unknown characters

- Any character not matched by the rules above is emitted as a single `Unknown` token.
- The Lexer must **not** throw on unknown characters.
- Processing continues normally after the unknown token.

### 5.7 EOF token

- The last element of the returned list is always a token of type `Eof`.
- Its `Value` is the empty string and its `Position` is the length of the input string.
- Empty input produces exactly one token: `Eof`.

### 5.8 Error handling

The Lexer throws `LexerException` (extends `Exception`) only for unrecoverable errors — currently only unterminated string literals. All other anomalies emit `Unknown` tokens. The exception message must include the zero-based position of the offending character.

---

## 6. Public API

The Lexer exposes a single public constructor and a single public method:

```csharp
namespace ClearDB.Core.Parser;

public sealed class Lexer
{
    public Lexer(string input);

    /// <summary>
    /// Tokenize the entire input and return all tokens including the
    /// terminal Eof. Never returns null. Never returns an empty list.
    /// </summary>
    /// <exception cref="LexerException">
    /// Thrown only for unterminated string literals.
    /// </exception>
    public List<Token> Tokenize();
}

public sealed class LexerException : Exception
{
    public int Position { get; }
    public LexerException(string message, int position)
        : base(message) => Position = position;
}
```

---

## 7. Required test cases

All tests live in `ClearDB.Tests / LexerTests.cs`. The suite must pass before the Lexer is considered complete. Tests are written using xUnit and follow the Arrange / Act / Assert pattern established during the REPL phase.

### 7.1 Happy-path tests

| Input | Expected tokens | Purpose |
|---|---|---|
| `"SELECT"` | `SELECT, EOF` | Single keyword |
| `"select"` | `SELECT, EOF` | Case insensitivity |
| `"SELECT *"` | `SELECT, STAR, EOF` | Star token |
| `"SELECT * FROM users"` | `SELECT, STAR, FROM, IDENT(users), EOF` | Simple query |
| `"WHERE id = 1"` | `WHERE, IDENT(id), EQ, INT(1), EOF` | Operator + int literal |
| `"WHERE id != 1"` | `WHERE, IDENT(id), NEQ, INT(1), EOF` | Two-char operator |
| `"WHERE id <> 1"` | `WHERE, IDENT(id), NEQ, INT(1), EOF` | SQL-standard not-equals |
| `"WHERE price <= 9.99"` | `WHERE, IDENT(price), LTE, FLOAT(9.99), EOF` | Float literal |
| `"WHERE name = 'Alice'"` | `WHERE, IDENT(name), EQ, STR(Alice), EOF` | String literal |
| `"'it''s'"` | `STR(it's), EOF` | Escaped single quote |
| `"  SELECT  *  "` | `SELECT, STAR, EOF` | Extra whitespace |
| `"(id, name)"` | `LPAREN, IDENT(id), COMMA, IDENT(name), RPAREN, EOF` | Punctuation |
| `""` *(empty string)* | `EOF` | Empty input |

### 7.2 Edge-case tests

| Input | Expected tokens | Purpose |
|---|---|---|
| `"SELECT@name"` | `SELECT, UNKNOWN(@), IDENT(name), EOF` | Unknown char does not throw |
| `"123abc"` | `INT(123), IDENT(abc), EOF` | Number then identifier |
| `"_col_1"` | `IDENT(_col_1), EOF` | Underscore-leading identifier |
| `"<="` | `LTE, EOF` | Two-char op at end of input |
| `"<"` | `LT, EOF` | Single `<` not followed by `=` |
| `"SELECT;\nFROM"` | `SELECT, SEMI, FROM, EOF` | Newline treated as whitespace |
| Position field check | Each token reports correct byte offset | Position tracking |

### 7.3 Error tests

| Input | Expected outcome | Purpose |
|---|---|---|
| `"'unterminated"` | throws `LexerException` | Unterminated string |
| `"'missing at EOF"` | `LexerException.Position = 0` | Position reported correctly |

> **TDD guidance:** Write each test first and watch it fail before implementing the feature. Start with the single-keyword test, implement just enough to pass it, then move to the next. The full test list above defines your implementation checklist.

---

## 8. Performance notes

The Lexer operates on strings up to a few kilobytes (typical SQL queries). O(n) single-pass scanning is the target — no backtracking, no lookahead beyond one character.

- Use a single integer cursor (`_pos`) rather than slicing strings in the inner loop.
- Use `ReadOnlySpan<char>` or string indexing directly — avoid allocating substrings until a token is committed.
- The returned `List<Token>` allocates one record per token. For v1 this is acceptable; a pooled approach can be added in a later optimisation pass.

---

## 9. What comes next

Once the Lexer passes all tests in §7, the next component is the Parser (Phase 1b). The Parser consumes the token list produced here and builds the Abstract Syntax Tree. No changes to the Lexer should be required to support the Parser — if a Parser test reveals a missing token type, add it to this spec first and update the Lexer accordingly.

> **Integration milestone:** After the Parser is complete, wire both into `Engine.Execute()` and run the `ClearDB.Cli` REPL. Typing `SELECT * FROM users;` should produce a parsed `SelectStatement` object and print its string representation — the first sign of life from the full pipeline.

---

*ClearDB project · Lexer spec v1.0*