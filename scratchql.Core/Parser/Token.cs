namespace scratchql.Core.Parser;

public record Token(eTokenType type, string source, int pos);
