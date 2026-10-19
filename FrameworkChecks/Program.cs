using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BaobabRidge;

internal static class Program
{
    private static int checks;
    private static string testFolder;
    private static string filePath;
    private static readonly string[] sampleLines =
    {
        "WR-0001|Thabo|African Penguin|4|85|Release-Ready|Pre-Release Camp",
        "WR-0002|Naledi|Serval|3|79|Recovering|Outdoor Enclosure",
        "WR-0004|Zolá|Cape Vulture|12|19|Critical|Intensive Care Unit"
    };

    // Runs checks against a new temporary folder, never the user's animals.txt.
    [STAThread]
    private static int Main(string[] args)
    {
        testFolder = Path.Combine(Path.GetTempPath(), "BaobabRidgeChecks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testFolder);
        filePath = Path.Combine(testFolder, "animals.txt");
        try
        {
            CheckRules();
            CheckFileUpdates();
            ApplicationConfiguration.Initialize();
            string previewPath = string.Empty;
            if (args.Length > 0) previewPath = args[0];
            CheckForm(previewPath);
            Console.WriteLine("PASS: " + checks + " checks, including real file updates and form events.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex);
            return 1;
        }
        finally
        {
            // Delete only files in the unique temporary folder created by this run.
            foreach (string file in Directory.GetFiles(testFolder))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            Directory.Delete(testFolder);
        }
    }

    // Tests the two edges of all five score bands and common invalid input.
    private static void CheckRules()
    {
        int[] scores = { 0, 19, 20, 39, 40, 59, 60, 79, 80, 100 };
        string[] statuses = { "Critical", "Serious", "Stable", "Recovering", "Release-Ready" };
        string[] housing = { "Intensive Care Unit", "High-Dependency Ward", "Recovery Ward", "Outdoor Enclosure", "Pre-Release Camp" };
        for (int i = 0; i < scores.Length; i++)
        {
            Animal animal = CreateAnimal("WR-0002", scores[i]);
            Check(animal.Status == statuses[i / 2] && animal.HousingUnit == housing[i / 2], "Band edge " + scores[i]);
        }

        Animal valid;
        string field;
        string error;
        Check(AnimalRules.TryCreate(" wr-0002 ", " Naledi ", " Serval ", " 3 ", " 80 ",
            out valid, out field, out error), "Trimmed input");
        Check(valid.Id == "WR-0002" && valid.Name == "Naledi", "ID normalization");
        string[,] invalid =
        {
            { "WR-123", "Naledi", "Serval", "3", "80", "Id" },
            { "WR-１２３４", "Naledi", "Serval", "3", "80", "Id" },
            { "WR-0002", "", "Serval", "3", "80", "Name" },
            { "WR-0002", new string('a', 31), "Serval", "3", "80", "Name" },
            { "WR-0002", "Naledi", "Ser|val", "3", "80", "Species" },
            { "WR-0002", "Naledi", "Ser\nval", "3", "80", "Species" },
            { "WR-0002", "Naledi", "Serval", "1.5", "80", "Age" },
            { "WR-0002", "Naledi", "Serval", "-1", "80", "Age" },
            { "WR-0002", "Naledi", "Serval", "101", "80", "Age" },
            { "WR-0002", "Naledi", "Serval", "3", "abc", "RecoveryScore" },
            { "WR-0002", "Naledi", "Serval", "3", "101", "RecoveryScore" },
            { "WR-0002", "Naledi", "Serval", "3", "", "RecoveryScore" }
        };
        for (int row = 0; row < invalid.GetLength(0); row++)
        {
            bool accepted = AnimalRules.TryCreate(invalid[row, 0], invalid[row, 1], invalid[row, 2],
                invalid[row, 3], invalid[row, 4], out valid, out field, out error);
            Check(!accepted && valid == null && field == invalid[row, 5] && error.Length > 0, "Reject invalid input " + row);
        }
    }

    // Verifies persisted updates and that failure cases leave the original file untouched.
    private static void CheckFileUpdates()
    {
        FileHandler handler = new FileHandler(filePath);
        int skipped;
        Check(handler.ReadAnimals(out skipped).Count == 0 && File.Exists(filePath), "Missing file created empty");
        string[] ids = { "WR-0001", "WR-0002", "WR-0004" };
        for (int target = 0; target < ids.Length; target++)
        {
            ResetFile();
            Animal updated = CreateAnimal(ids[target], 80);
            handler.UpdateAnimal(ids[target].ToLowerInvariant(), updated);
            string[] after = File.ReadAllLines(filePath);
            Check(after.Length == sampleLines.Length, "No records lost or duplicated");
            for (int line = 0; line < after.Length; line++)
            {
                if (line == target) Check(after[line] == updated.ToString(), "Target persisted");
                else Check(after[line] == sampleLines[line], "Unrelated line preserved");
            }
            FileHandler reopened = new FileHandler(filePath);
            Check(reopened.ReadAnimals(out skipped)[target].RecoveryScore == 80, "Update survives reopening");
        }

        ResetFile();
        string[] changed = File.ReadAllLines(filePath);
        changed[0] = changed[0].Replace("Thabo", "Changed outside grid");
        changed[1] = changed[1].Replace("Recovering|Outdoor Enclosure", "Critical|Intensive Care Unit");
        File.WriteAllLines(filePath, changed, Encoding.UTF8);
        handler.UpdateAnimal("WR-0002", CreateAnimal("WR-0002", 79));
        string[] corrected = File.ReadAllLines(filePath);
        Check(corrected[0] == changed[0], "Latest unrelated external change survives");
        Check(corrected[1].EndsWith("79|Recovering|Outdoor Enclosure"), "Unchanged score still reclassified");

        ResetFile();
        File.AppendAllText(filePath, "bad line\nWR-7777|Bad|Cat|letters|50|Stable|Recovery Ward\n", Encoding.UTF8);
        Check(handler.ReadAnimals(out skipped).Count == 3 && skipped == 2, "Malformed lines counted");
        string[] beforeMalformed = File.ReadAllLines(filePath);
        handler.UpdateAnimal("WR-0002", CreateAnimal("WR-0002", 80));
        Check(File.ReadAllLines(filePath)[3] == beforeMalformed[3], "Unrelated malformed line preserved");

        ResetFile();
        CheckFailedUpdate(handler, "WR-0002", CreateAnimal("WR-9999", 80), "ID change rejected");
        CheckFailedUpdate(handler, "WR-9999", CreateAnimal("WR-9999", 80), "Missing ID rejected");
        Animal invalid = CreateAnimal("WR-0002", 80);
        invalid.Name = "bad|name";
        CheckFailedUpdate(handler, "WR-0002", invalid, "Invalid replacement rejected");
        File.AppendAllText(filePath, sampleLines[1].Replace("WR-0002", "wr-0002") + "\n");
        CheckFailedUpdate(handler, "WR-0002", CreateAnimal("WR-0002", 80), "Duplicate target rejected");

        ResetFile();
        File.SetAttributes(filePath, FileAttributes.ReadOnly);
        try
        {
            CheckFailedUpdate(handler, "WR-0002", CreateAnimal("WR-0002", 80), "Read-only file protected");
        }
        finally
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
        }

        ResetFile();
        using (FileStream locked = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            // Reading is allowed, but Windows denies replacement: tests failure after temporary writing.
            CheckFailedUpdate(handler, "WR-0002", CreateAnimal("WR-0002", 80), "Locked replacement protected");
        }
        string[] temporaryFiles = Directory.GetFiles(testFolder, "*.tmp");
        Check(temporaryFiles.Length == 0, "Failed replacement cleaned temporary file: " + string.Join(", ", temporaryFiles));
        File.Delete(filePath);
        bool missingRejected = false;
        try { handler.UpdateAnimal("WR-0002", CreateAnimal("WR-0002", 80)); }
        catch (FileNotFoundException) { missingRejected = true; }
        Check(missingRejected && !File.Exists(filePath), "Save does not recreate a disappeared file");
        ResetFile();
    }

    // Verifies Add/Search/Update buttons and error feedback while Victor's grid is pending.
    private static void CheckForm(string previewPath)
    {
        ResetFile();
        using (MainForm form = new MainForm(new FileHandler(filePath)))
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-10000, -10000);
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();
            TextBox id = (TextBox)Find(form, "txtId");
            Button add = (Button)Find(form, "btnAdd");
            Button save = (Button)Find(form, "btnSaveChanges");
            Check(!id.ReadOnly && add.Enabled && !save.Enabled, "Initial Add state");

            Find(form, "txtSearchId").Text = " wr-0002 ";
            ((Button)Find(form, "btnSearch")).PerformClick();
            Check(id.Text == "WR-0002" && id.ReadOnly && !add.Enabled && save.Enabled, "Search enters edit state");
            Find(form, "txtName").Text = "Unsaved edit";
            Find(form, "txtSearchId").Text = "WR-000";
            ((Button)Find(form, "btnSearch")).PerformClick();
            Check(Find(form, "txtName").Text == "Unsaved edit" && id.Text == "WR-0002"
                && Find(form, "lblMessage").Text.Contains("not found"), "Partial ID leaves editor unchanged");
            Find(form, "txtRecoveryScore").Text = "abc";
            byte[] before = File.ReadAllBytes(filePath);
            save.PerformClick();
            Check(SameBytes(before, File.ReadAllBytes(filePath)) && save.Enabled
                && form.ActiveControl == Find(form, "txtRecoveryScore"), "Invalid score preserves file and restores focus");

            Find(form, "txtRecoveryScore").Text = "80";
            File.SetAttributes(filePath, FileAttributes.ReadOnly);
            try
            {
                save.PerformClick();
                Check(SameBytes(before, File.ReadAllBytes(filePath)) && save.Enabled && id.ReadOnly
                    && Find(form, "txtName").Text == "Unsaved edit"
                    && Find(form, "lblMessage").Text.Contains("permissions"), "Failed save preserves edit and reports permissions");
            }
            finally { File.SetAttributes(filePath, FileAttributes.Normal); }

            save.PerformClick();
            Check(File.ReadAllLines(filePath)[1].Contains("|80|Release-Ready|Pre-Release Camp"), "UI saves real file");
            Check(id.Text == "" && !id.ReadOnly && add.Enabled && !save.Enabled, "Save resets editor without a grid");

            Find(form, "txtSearchId").Text = "WR-0004";
            ((Button)Find(form, "btnSearch")).PerformClick();
            Check(id.Text == "WR-0004" && save.Enabled, "Search can load the next animal");
            ((Button)Find(form, "btnClear")).PerformClick();
            Check(id.Text == "" && !save.Enabled && Find(form, "txtName").Text == ""
                && Find(form, "txtSearchId").Text == "", "Clear empties editor");

            string beforeAdd = File.ReadAllText(filePath);
            id.Text = "wr-0009";
            Find(form, "txtName").Text = " New animal ";
            Find(form, "txtSpecies").Text = "Serval";
            Find(form, "txtAge").Text = "2";
            Find(form, "txtRecoveryScore").Text = "20";
            add.PerformClick();
            string afterAdd = File.ReadAllText(filePath);
            Check(afterAdd.StartsWith(beforeAdd, StringComparison.Ordinal)
                && File.ReadAllLines(filePath)[3] == "WR-0009|New animal|Serval|2|20|Serious|High-Dependency Ward",
                "Keanu's Add appends without changing existing records");
            Check(id.Text == "" && add.Enabled && !save.Enabled
                && Find(form, "lblMessage").Text.Contains("Added WR-0009"), "Add success resets editor without a grid");

            id.Text = "wr-0009";
            Find(form, "txtName").Text = "Duplicate animal";
            Find(form, "txtSpecies").Text = "Serval";
            Find(form, "txtAge").Text = "2";
            Find(form, "txtRecoveryScore").Text = "20";
            add.PerformClick();
            Check(File.ReadAllText(filePath) == afterAdd
                && Find(form, "txtName").Text == "Duplicate animal"
                && Find(form, "lblMessage").Text.Contains("already exists"), "Duplicate Add still rejected");
            ((Button)Find(form, "btnClear")).PerformClick();

            if (previewPath != string.Empty)
            {
                form.PerformLayout();
                using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
                {
                    form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
                    bitmap.Save(previewPath);
                }
            }
        }
    }

    // Creates a valid replacement through the same shared method as the form.
    private static Animal CreateAnimal(string id, int score)
    {
        Animal animal;
        string field;
        string error;
        if (!AnimalRules.TryCreate(id, "Naledi", "Serval", "3", score.ToString(CultureInfo.InvariantCulture),
            out animal, out field, out error))
        {
            throw new Exception(error);
        }
        return animal;
    }

    // Restores a small fixture containing a non-ASCII name.
    private static void ResetFile()
    {
        File.WriteAllLines(filePath, sampleLines, new UTF8Encoding(false));
    }

    // Requires a failed update to leave the existing file byte-for-byte unchanged.
    private static void CheckFailedUpdate(FileHandler handler, string id, Animal animal, string description)
    {
        byte[] before = File.ReadAllBytes(filePath);
        bool failed = false;
        try { handler.UpdateAnimal(id, animal); }
        catch (InvalidOperationException) { failed = true; }
        catch (IOException) { failed = true; }
        catch (UnauthorizedAccessException) { failed = true; }
        Check(failed && SameBytes(before, File.ReadAllBytes(filePath)), description);
    }

    // Compares bytes without relying on LINQ.
    private static bool SameBytes(byte[] first, byte[] second)
    {
        if (first.Length != second.Length) return false;
        for (int i = 0; i < first.Length; i++)
        {
            if (first[i] != second[i]) return false;
        }
        return true;
    }

    // Finds one actual form control by its meaningful designer name.
    private static Control Find(Form form, string name)
    {
        return form.Controls.Find(name, true)[0];
    }

    // Stops immediately when an expected result is missing.
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        checks++;
    }
}
