using System.Text;

namespace Lox;

public class Program
{
    private static LoxLang lox = new();

    public static int Main(string[] args)
    {
        switch (args.Length)
        {
            case 1:
                return RunFile(args[0]);
                break;
            case 0:
                return RunPrompt();
                break;
            default:
                return 0;
        }
    }

    public static int RunFile(string path)
    {
        string contents = File.ReadAllText(path);

        Run(contents);

        if (lox.HasError) return 65;
        if (lox.HasRuntimeError) return 70;

        return 0;
    }

    public static int RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");

            StringBuilder buffer = new();
            while (true)
            {
                string? line = Console.ReadLine();

                if (line == null)
                    break;

                buffer.Append(line);

                if (line.Length == 0) break;
            }
            

            RunLine(buffer.ToString());
            buffer.Clear();

            if (lox.HasError) return 65;
            if (lox.HasRuntimeError) return 70;
        }

        return 0;
    }

    public static void RunLine(string source)
    {
        lox.Run(source);
    }

    public static void Run(string source)
    {
        lox.Reset();
        lox.Run(source);
    }
}

