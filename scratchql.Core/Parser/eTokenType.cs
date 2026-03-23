namespace scratchql.Core.Parser;

public enum eTokenType
{
    Select, Eof,
    Insert,
    Update,
    Delete,
    Create,
    Drop,
    Table,
    From,
    Where,
    And,
    Or,
    Not,
    Into,
    Values,
    Null,
    Set,
    Equal
}