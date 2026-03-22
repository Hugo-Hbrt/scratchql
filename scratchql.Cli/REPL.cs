namespace scratchql.Cli;

public class Repl
{
    TextWriter _outputStream;
    TextReader _inputStream;
    public eReplState State { get; private set; }
    public Repl(TextReader input, TextWriter output)
    {
        _outputStream = output ?? throw new Exception("Output interface can't be null");
        _inputStream = input ?? throw new Exception("Input interface can't be null");
    }

    public void Run()
    {
        while (State != eReplState.Stopped)
        {
            _outputStream.Write(CLIConfig.databaseName + "> ");

            var input = _inputStream.ReadLine();

            if (input == null)
            {
                State = eReplState.Stopped;
                return;
            }

            if (input == string.Empty)
            {
                _outputStream.WriteLine("");
                continue;
            }

            if (input == ".quit" || input == ".exit")
            {
                State = eReplState.ExitRequested;
                return;
            }

            if (input == ".help")
            {
                PrintHelp();
            }

            if (input.StartsWith("."))
            {
                _outputStream.Write("Unknown command: " + input);
            }

            _outputStream.Write("Executing: " + input);
            _outputStream.Write("\n");
        }
    }

    private void PrintHelp()
    {
        _outputStream.WriteLine("Help commands :");
        _outputStream.WriteLine(".quit / .exit : quit CLI");
        _outputStream.WriteLine(".help : show this help message");
    }
}
