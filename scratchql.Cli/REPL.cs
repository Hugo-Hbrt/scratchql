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
            _outputStream.Write(CLIConfig.databaseName + ">");

            var input = _inputStream.ReadLine();

            if (input == null)
            {
                State = eReplState.Stopped;
                return;
            }

            if (input == ".quit")
            {
                State = eReplState.ExitRequested;
                return;
            }

            _outputStream.Write("\n");
        }
    }
}
