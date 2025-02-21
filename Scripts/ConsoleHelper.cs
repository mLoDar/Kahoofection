using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Kahoofection.Ressources;



#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time





namespace Kahoofection.Scripts
{
    internal class ConsoleHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(nint hConsoleHandle, int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(nint hConsoleHandle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern nint GetStdHandle(int handle);



        internal static (bool ansiSupportEnabled, Exception occurredError) EnableAnsiSupport()
        {
            try
            {
                nint consoleHandle = GetStdHandle(-11);

                if (GetConsoleMode(consoleHandle, out int currentConsoleMode))
                {
                    SetConsoleMode(consoleHandle, currentConsoleMode | 0x0004);
                }
            }
            catch (Exception exception)
            {
                return (false, exception);
            }

            return (true, new Exception());
        }

        internal static async Task<(bool escapeKeyPressed, string lineContent)> ReadLine()
        {
            StringBuilder sb = new();

            int cursorStartPositionTop = Console.CursorTop;



            while (true)
            {
                await Task.Delay(10);

                if (Console.KeyAvailable == false)
                {
                    continue;
                }

                ConsoleKeyInfo pressedKey = Console.ReadKey(true);

                switch (pressedKey.Key)
                {
                    case ConsoleKey.Escape:
                        return (true, string.Empty);

                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return (false, sb.ToString());

                    case ConsoleKey.Backspace:
                        if (sb.Length > 0)
                        {
                            sb.Remove(sb.Length - 1, 1);

                            int cursorLeft = Console.CursorLeft;
                            int cursorTop = Console.CursorTop;

                            if (cursorLeft == 0 && cursorTop > cursorStartPositionTop)
                            {
                                Console.SetCursorPosition(Console.BufferWidth - 1, cursorTop - 1);
                                Console.Write(" ");
                                Console.SetCursorPosition(Console.BufferWidth - 1, cursorTop - 1);
                                break;
                            }
                            
                            Console.Write("\b \b");
                        }
                        break;

                    default:
                        if (char.IsControl(pressedKey.KeyChar) == false && sb.Length < 4094)
                        {
                            sb.Append(pressedKey.KeyChar);
                            Console.Write(pressedKey.KeyChar);
                        }
                        break;
                }
            }
        }

        internal static void ResetConsole()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 4);
        }

        internal static async Task DisplayInformation(string title, string description, ConsoleColor titleColor, int durationInSeconds = 3)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             ┌───");
            Console.Write("             │ ");
            Console.ForegroundColor = titleColor;
            Console.WriteLine(title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             ├───────────────");
            Console.WriteLine("             │ {0}", description);
            Console.WriteLine("             └───");

            if (durationInSeconds <= 0 || durationInSeconds > 10)
            {
                await Task.Delay(5000);
                return;
            }

            await Task.Delay(durationInSeconds * 1000);
        }

        internal static int GetCombinedAnsiSequenceLength(string input)
        {
            int combinedLength = 0;
            MatchCollection matches = RegexPatterns.AnsiSequence().Matches(input);
            
            foreach (Match match in matches)
            {
                combinedLength += match.Length;
            }

            return combinedLength;
        }

        internal static void ClearLine()
        {
            int cursorTop = Console.GetCursorPosition().Top;

            Console.SetCursorPosition(0, cursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, cursorTop);
        }
    }
}