using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using scratchql.Cli;

namespace scratchql.Cli.Tests;

public class REPLTests
{
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
        Assert.That(output.ToString(), Is.EqualTo(CLIConfig.databaseName + ">"));
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

        Assert.That(output.ToString(), Is.EqualTo(CLIConfig.databaseName + ">" + "\n" + CLIConfig.databaseName + ">"));
    }

    [Test]
    public void QuitCommandExits()
    {
        var quitCommand = ".quit\n";
        var (sut, _) = ReplFactory.WithInput(quitCommand);

        sut.Run();
        Assert.That(sut.State, Is.EqualTo(eReplState.ExitRequested));
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

