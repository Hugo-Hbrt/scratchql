# CLI REPL Specification

## Overview

The CLI is an interactive REPL (Read-Eval-Print-Loop) that reads SQL statements and meta-commands from the user, processes them, and prints results.

## Behaviors

### Prompt display

- The REPL prints `scratchql> ` before waiting for each line of input.

### Input handling

- Empty or whitespace-only input is ignored — the REPL re-prompts without producing any output.
- `null` input (EOF / Ctrl+D) exits the REPL gracefully.

### Meta-commands

Meta-commands start with `.` and are handled before any SQL parsing.

| Command  | Behavior                                      |
|----------|-----------------------------------------------|
| `.quit`  | Exits the REPL.                               |
| `.exit`  | Exits the REPL.                               |
| `.help`  | Prints a list of available meta-commands.      |

- An unrecognized meta-command (e.g., `.foo`) prints: `Unknown command: .foo`

### SQL input (placeholder)

- Any input that does not start with `.` is treated as a SQL statement.
- Until the parser is implemented, the REPL echoes: `Executing: <input>`

## Design notes for testability

The REPL logic must be decoupled from `Console` so it can be unit-tested.

- The REPL class accepts a `TextReader` (input) and a `TextWriter` (output) via its constructor.
- In production (`Program.cs`), pass `Console.In` and `Console.Out`.
- In tests, pass `StringReader` and `StringWriter` to control input and assert on output.

## Test list (TDD backlog)

Work through these in order, one red-green-refactor cycle per item:

1. REPL prints prompt on start then exits on EOF.
2. REPL ignores empty input and re-prompts.
3. `.quit` exits the REPL.
4. `.exit` exits the REPL.
5. `.help` prints the list of available meta-commands.
6. Unknown meta-command (e.g., `.foo`) prints an error message.
7. Non-meta input echoes as a SQL execution placeholder.
