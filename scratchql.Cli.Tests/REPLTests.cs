using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using scratchql.Cli;

namespace scratchql.Cli.Tests;

public class REPLTests
{
    private const string ReplCommandPrompt = CLIConfig.databaseName + "> ";
    [SetUp]
    public void Setup()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void PromptsDatabaseEngineName()
    {
        var (sut, output) = ReplFactory.WithInput("");

        sut.Run();
        Assert.That(output.ToString(), Is.EqualTo(ReplCommandPrompt));
    }

    [Test]
    public void StopsWhenEOF()
    {
        var (sut, _) = ReplFactory.WithInput("");

        sut.Run();
        Assert.That(sut.State, Is.EqualTo(eReplState.Stopped));
    }

    [Test]
    public void IgnoreWhenEmptyInput_And_RePrompts()
    {
        var emptyInput = "\n";
        var (sut, output) = ReplFactory.WithInput(emptyInput);

        sut.Run();

        Assert.That(output.ToString(), Is.EqualTo(ReplCommandPrompt + "\n" + ReplCommandPrompt));
    }

    [Test]
    public void QuitCommandExits()
    {
        var quitCommand = ".quit\n";
        var (sut, _) = ReplFactory.WithInput(quitCommand);

        sut.Run();
        Assert.That(sut.State, Is.EqualTo(eReplState.ExitRequested));
    }

    [Test]
    public void ExitCommandExits()
    {
        var quitCommand = ".exit\n";
        var (sut, _) = ReplFactory.WithInput(quitCommand);

        sut.Run();
        Assert.That(sut.State, Is.EqualTo(eReplState.ExitRequested));
    }

    [Test]
    public void UnknownMetaCommandPrintsError()
    {
        var unknownCommand = ".foobar\n";
        var (sut, output) = ReplFactory.WithInput(unknownCommand);

        sut.Run();
        var outputString = output.ToString();

        Assert.That(outputString!.StartsWith(ReplCommandPrompt + "Unknown command: " + unknownCommand));
    }

    [Test]
    public void HelpPrintsHelpOutput()
    {
        var helpCommand = ".help\n";
        var (sut, output) = ReplFactory.WithInput(helpCommand);

        sut.Run();
        var outputString = output.ToString();

        Assert.That(outputString!.Contains(".quit"));
        Assert.That(outputString!.Contains(".exit"));
        Assert.That(outputString!.Contains(".help"));
    }

    [Test]
    public void NonMetaInputEchoesAsSQLExecution()
    {
        var anyCommand = "SELECT * FROM Table1\n";

        var (sut, output) = ReplFactory.WithInput(anyCommand);

        sut.Run();

        var outputString = output.ToString();
        Assert.That(outputString!.Contains("Executing: " + anyCommand));
    }


    internal class ReplFactory
    {
        public static (Repl, TextWriter) WithInput(string inputCommand)
        {
            var _output = new StringWriter();
            var _input = new StringReader(inputCommand);

            return (new Repl(_input, _output), _output);
        }
    }
}

