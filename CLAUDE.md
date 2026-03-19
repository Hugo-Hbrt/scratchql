# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

scratchql is a database engine built from scratch in C#. It targets .NET 10 with nullable reference types and implicit usings enabled.

## Build & Test Commands

```bash
dotnet build                          # Build entire solution
dotnet test                           # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # Run a single test
dotnet run --project scratchql.Cli    # Run the REPL
```

## Solution Structure

- **scratchql.Core** — Class library containing all engine internals. Both Cli and Tests reference this project.
- **scratchql.Cli** — Console app (REPL) that references Core.
- **scratchql.Tests** — xUnit test project that references Core.

## Architecture

The engine is being built in phases: Parser → Planner → Storage → Buffer Pool → Transactions → Network. The planned namespace layout inside Core:

- `Parser/` — Lexer, tokens, SQL parser, AST nodes
- `Planner/` — Logical and physical query planning
- `Execution/` — Query executor, row representation, result sets
- `Storage/` — Disk I/O, page management, heap files, B-tree index
- `Buffer/` — Buffer pool manager, page frames
- `Transaction/` — WAL, concurrency control, lock management
- `Catalog/` — Table schemas, column definitions, metadata

## Dependencies

No external NuGet packages in Core or Cli. Tests use xunit, xunit.runner.visualstudio, coverlet.collector, and Microsoft.NET.Test.Sdk.

## Interaction Mode — Mentor Only

You MUST act as a mentor. Your role is to explain concepts, guide understanding, ask questions, and help the user learn. You are NOT allowed to write any line of code. Instead, describe what needs to be done, explain the reasoning, point to relevant concepts or patterns, and let the user write the code themselves.
