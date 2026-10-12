using BaobabRidge;
using System.Reflection;

internal static class Program
{
    private static int checks;

    // Runs dependency-free checks; production file tests must be added once the TODO is implemented.
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            CheckRules();
            CheckPracticeUpdates();
            ApplicationConfiguration.Initialize();
            CheckForm(args.FirstOrDefault());
            Console.WriteLine($"PASS: {checks} framework assertions. Production persistence remains unimplemented.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: {ex.Message}");
            return 1;
        }
    }

    // Checks both edges of every classification band and field validation failures.
    private static void CheckRules()
    {
        var cases = new[]
        {
            (0, "Critical", "Intensive Care Unit"), (19, "Critical", "Intensive Care Unit"),
            (20, "Serious", "High-Dependency Ward"), (39, "Serious", "High-Dependency Ward"),
            (40, "Stable", "Recovery Ward"), (59, "Stable", "Recovery Ward"),
            (60, "Recovering", "Outdoor Enclosure"), (79, "Recovering", "Outdoor Enclosure"),
            (80, "Release-Ready", "Pre-Release Camp"), (100, "Release-Ready", "Pre-Release Camp")
        };
        foreach (var (score, status, housing) in cases)
            Check(AnimalRules.Classify(score) == (status, housing), $"Classification at {score}");
        AnimalInput valid = new(" wr-0002 ", " Naledi ", " Serval ", " 3 ", " 80 ");
        Check(AnimalRules.TryCreate(valid, out Animal? animal, out _), "Trimmed input accepted");
        Check(animal!.Id == "WR-0002" && animal.Name == "Naledi" && animal.Status == "Release-Ready", "Normalization and classification");
        var invalid = new[]
        {
            (valid with { Id = "WR-123" }, "Id"),
            (valid with { Name = " " }, "Name"),
            (valid with { Name = new string('a', 31) }, "Name"),
            (valid with { Species = "Ser|val" }, "Species"),
            (valid with { Species = "Serval\ncat" }, "Species"),
            (valid with { Age = "1.5" }, "Age"),
            (valid with { Age = "-1" }, "Age"),
            (valid with { RecoveryScore = "101" }, "RecoveryScore"),
            (valid with { RecoveryScore = "abc" }, "RecoveryScore"),
            (valid with { RecoveryScore = "" }, "RecoveryScore")
        };
        foreach (var (input, field) in invalid)
            Check(!AnimalRules.TryCreate(input, out var rejected, out var issue)
                && rejected == null && issue!.Field == field, $"Reject invalid {field}");
    }

    // Verifies replacement identity, order and preservation in the practice adapter only.
    private static void CheckPracticeUpdates()
    {
        var repository = new PracticeAnimalRepository();
        var before = repository.Load().Animals;
        AnimalRules.TryCreate(new("WR-0002", "Naledi", "Serval", "3", "80"), out var updated, out _);
        repository.Update("wr-0002", updated!);
        var after = repository.Load().Animals;
        Check(after.Count == before.Count && after[0] == before[0] && after[2] == before[2], "Unrelated records preserved");
        Check(after[1].RecoveryScore == 80 && after[1].Status == "Release-Ready", "Boundary update");
        Check(before[1].RecoveryScore == 79, "Old snapshot unchanged");
        bool rejected = false;
        try { repository.Update("WR-0002", updated! with { Id = "WR-9999" }); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected && repository.Load().Animals.SequenceEqual(after), "ID change rejected without mutation");
    }

    // Inspects real WinForms controls without modal dialogs or changing any data file.
    private static void CheckForm(string? previewPath)
    {
        using var form = new MainForm(new PracticeAnimalRepository(), true);
        // Create a real off-screen window so binding/layout match a running application.
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-10000, -10000);
        form.ShowInTaskbar = false;
        form.Show();
        Application.DoEvents();
        var grid = Find<DataGridView>(form, "gridAnimals");
        var id = Find<TextBox>(form, "txtId");
        var add = Find<Button>(form, "btnAdd");
        var save = Find<Button>(form, "btnSaveChanges");
        Check(grid.Rows.Count == 3 && grid.SelectedRows.Count == 0, "Startup loads records without selecting a row");
        Check(!id.ReadOnly && add.Enabled && !save.Enabled, "Initial Add mode");
        Check(grid.ReadOnly && !grid.AllowUserToAddRows && !grid.AllowUserToDeleteRows
            && grid.SelectionMode == DataGridViewSelectionMode.FullRowSelect, "Read-only whole-row grid");
        Check(grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).SequenceEqual(
            new[] { "Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit" }), "Column order");
        Find<TextBox>(form, "txtSearchId").Text = " wr-0002 ";
        Invoke(form, "SearchAnimal");
        Check(id.Text == "WR-0002" && id.ReadOnly && !add.Enabled && save.Enabled, "Search loads edit mode");
        Check(Find<TextBox>(form, "txtRecoveryScore").Text == "79", "Search loads existing score");
        Invoke(form, "ClearEditor");
        Check(id.Text == "" && !id.ReadOnly && add.Enabled && !save.Enabled, "Clear restores Add mode");
        Check(Find<TextBox>(form, "txtName").Text == "" && Find<TextBox>(form, "txtSearchId").Text == "", "Clear empties fields");
        if (previewPath != null)
        {
            form.PerformLayout();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
            bitmap.Save(previewPath);
        }
    }

    // Resolves a named control so checks exercise the actual shared form.
    private static T Find<T>(Form form, string name) where T : Control =>
        (T)form.Controls.Find(name, true).Single();

    // Invokes a form action without a visible window or simulated user input.
    private static void Invoke(MainForm form, string name) =>
        typeof(MainForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null);

    // Fails immediately with the scenario name rather than silently skipping an assertion.
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        checks++;
    }
}
