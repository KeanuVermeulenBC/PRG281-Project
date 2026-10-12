namespace BaobabRidge;

internal static class Program
{
    // Starts the framework in practice mode unless --file is supplied.
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        bool practiceMode = !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        IAnimalRepository repository = practiceMode
            ? new PracticeAnimalRepository()
            : new TextAnimalRepository(Path.Combine(AppContext.BaseDirectory, "animals.txt"));
        Application.Run(new MainForm(repository, practiceMode));
    }
}
