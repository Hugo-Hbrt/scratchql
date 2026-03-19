namespace scratchql.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        var repl = new Repl(Console.In, Console.Out);
        repl.Run();
    }
}