using Kahoofection.Scripts;
using Kahoofection.Ressources;





namespace Kahoofection.Modules.Information
{
    internal class QuizIdAnswers
    {
        private static readonly string _currentSection = "QuizIdAnswers";



        internal static async Task Start()
        {
        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();

            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter a QuizId        ");
            Console.WriteLine("             \u001b[94m├─────────────────────   ");
            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, $"Prompted to provide a QuizId, waiting for an input.");

            (bool escapeKeyPressed, string lineContent) = await ConsoleHelper.ReadLine();
        }
    }
}
