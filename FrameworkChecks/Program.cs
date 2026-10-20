using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
            CheckLoadingFailures();
            CheckDeleteAndSummary();
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

    // Verifies the real grid, Add/Search/Update connections and editor state after each save.
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
            DataGridView grid = (DataGridView)Find(form, "dataGrid");
            Check(!id.ReadOnly && id.Text == "" && add.Enabled && !save.Enabled, "Startup starts in Add mode");
            Check(grid.Rows.Count == 3 && grid.SelectedRows.Count == 0 && grid.CurrentCell == null, "Startup shows records without editing one");
            Check(grid.Parent == Find(form, "pnlAnimalsView") && grid.Dock == DockStyle.Fill, "Grid uses its resizable panel");
            Check(grid.ReadOnly && !grid.AllowUserToAddRows && !grid.AllowUserToDeleteRows
                && !grid.MultiSelect && grid.SelectionMode == DataGridViewSelectionMode.FullRowSelect, "Grid editing restrictions");
            string[] headers = { "Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit" };
            for (int i = 0; i < headers.Length; i++)
            {
                Check(grid.Columns[i].HeaderText == headers[i]
                    && grid.Columns[i].SortMode == DataGridViewColumnSortMode.NotSortable, "Header and file order " + i);
            }

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
            Check(id.Text == "" && !id.ReadOnly && add.Enabled && !save.Enabled
                && grid.SelectedRows.Count == 0 && grid.CurrentCell == null, "Save returns to empty Add mode");
            Check(((Animal)grid.Rows[1].DataBoundItem).RecoveryScore == 80
                && ((Animal)grid.Rows[1].DataBoundItem).Status == "Release-Ready", "Update refreshes displayed classification");

            Find(form, "txtSearchId").Text = "WR-0004";
            ((Button)Find(form, "btnSearch")).PerformClick();
            Check(id.Text == "WR-0004" && save.Enabled, "Search can load the next animal");
            ((Button)Find(form, "btnClear")).PerformClick();
            Check(id.Text == "" && !save.Enabled && Find(form, "txtName").Text == ""
                && Find(form, "txtSearchId").Text == "", "Clear empties editor");

            grid.CurrentCell = grid.Rows[2].Cells[0];
            grid.Rows[2].Selected = true;
            // Raise the real CellClick event without changing the application's visibility.
            typeof(DataGridView).GetMethod("OnCellClick", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(grid, new object[] { new DataGridViewCellEventArgs(0, 2) });
            Check(id.Text == "WR-0004" && id.ReadOnly && !add.Enabled && save.Enabled, "Row click loads the shared editor");
            ((Button)Find(form, "btnClear")).PerformClick();
            Check(grid.SelectedRows.Count == 0 && grid.CurrentCell == null && id.Text == "", "Clear removes the selected animal");
            byte[] beforeDelete = File.ReadAllBytes(filePath);
            ((Button)Find(form, "btnDelete")).PerformClick();
            Check(SameBytes(beforeDelete, File.ReadAllBytes(filePath))
                && Find(form, "lblMessage").Text.Contains("Select one animal"), "Delete after Clear asks for selection");

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
                && Find(form, "lblMessage").Text.Contains("Added WR-0009"), "Add success returns to Add mode");
            Check(grid.Rows.Count == 4 && ((Animal)grid.Rows[3].DataBoundItem).Id == "WR-0009"
                && grid.CurrentCell == null, "Add refreshes grid with no duplicates or automatic edit");

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

    // Exercises missing, empty, malformed, locked and missing-folder data through the real form.
    private static void CheckLoadingFailures()
    {
        File.Delete(filePath);
        using (MainForm form = OpenTestForm(filePath))
        {
            Check(File.Exists(filePath) && ((DataGridView)Find(form, "dataGrid")).Rows.Count == 0,
                "Startup creates an empty missing file");
        }

        ResetFile();
        File.AppendAllText(filePath, "malformed\nWR-7777|Bad|Cat|letters|50|Stable|Recovery Ward\n");
        using (MainForm form = OpenTestForm(filePath))
        {
            string message = Find(form, "lblMessage").Text;
            Check(((DataGridView)Find(form, "dataGrid")).Rows.Count == 3
                && message.Contains("Skipped 2 malformed line(s).")
                && message.IndexOf("Skipped") == message.LastIndexOf("Skipped"), "Startup reports one skipped-line total");
            Find(form, "txtSearchId").Text = "WR-0002";
            ((Button)Find(form, "btnSearch")).PerformClick();
            Find(form, "txtRecoveryScore").Text = "80";
            ((Button)Find(form, "btnSaveChanges")).PerformClick();
            Check(Find(form, "lblMessage").Text.Contains("Saved WR-0002")
                && Find(form, "lblMessage").Text.Contains("Skipped 2 malformed line(s)."), "Save retains the refresh skipped count");

            MethodInfo refresh = typeof(MainForm).GetMethod("RefreshAnimals", BindingFlags.Instance | BindingFlags.NonPublic);
            refresh.Invoke(form, null);
            refresh.Invoke(form, null);
            Check(((DataGridView)Find(form, "dataGrid")).Rows.Count == 3
                && !Find(form, "btnSaveChanges").Enabled, "Repeated refresh does not duplicate rows or enter edit mode");

            Find(form, "txtName").Text = "Unsaved input";
            using (FileStream locked = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                bool loaded = (bool)refresh.Invoke(form, null);
                Check(!loaded && Find(form, "lblMessage").Text.Contains("Cannot access")
                    && Find(form, "txtName").Text == "Unsaved input"
                    && ((DataGridView)Find(form, "dataGrid")).Rows.Count == 3,
                    "Failed refresh preserves the grid and inputs with friendly feedback");
            }
        }

        using (FileStream locked = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        using (MainForm form = OpenTestForm(filePath))
        {
            Check(Find(form, "lblMessage").Text.Contains("Cannot access")
                && Find(form, "btnAdd").Enabled, "Locked startup stays open with friendly feedback");
        }
        using (MainForm form = OpenTestForm(Path.Combine(testFolder, "missing-folder", "animals.txt")))
        {
            Check(Find(form, "lblMessage").Text.Contains("data folder was not found"), "Missing folder is handled on startup");
        }
    }

    // Verifies the real Delete confirmation and the required saved report, using temporary data.
    private static void CheckDeleteAndSummary()
    {
        ResetFile();
        string summaryPath = Path.Combine(testFolder, "summary.txt");
        using (MainForm form = OpenTestForm(filePath))
        {
            DataGridView grid = (DataGridView)Find(form, "dataGrid");
            grid.CurrentCell = grid.Rows[1].Cells[0];
            typeof(DataGridView).GetMethod("OnCellClick", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(grid, new object[] { new DataGridViewCellEventArgs(0, 1) });
            byte[] before = File.ReadAllBytes(filePath);
            ConfirmDelete(form, false);
            Check(SameBytes(before, File.ReadAllBytes(filePath)) && grid.Rows.Count == 3
                && Find(form, "txtId").Text == "WR-0002", "Delete No preserves file, grid and editor");
            ConfirmDelete(form, true);
            string[] after = File.ReadAllLines(filePath);
            Check(after.Length == 2 && after[0] == sampleLines[0] && after[1] == sampleLines[2],
                "Delete Yes removes only the selected record");
            Check(grid.Rows.Count == 2 && grid.CurrentCell == null && Find(form, "txtId").Text == ""
                && Find(form, "btnAdd").Enabled && !Find(form, "btnSaveChanges").Enabled,
                "Delete refreshes the grid and clears the deleted animal from editing");

            grid.CurrentCell = grid.Rows[0].Cells[0];
            File.SetAttributes(filePath, FileAttributes.ReadOnly);
            try
            {
                before = File.ReadAllBytes(filePath);
                ConfirmDelete(form, true);
                Check(SameBytes(before, File.ReadAllBytes(filePath))
                    && Find(form, "lblMessage").Text.Contains("Access denied"), "Read-only Delete fails safely");
            }
            finally { File.SetAttributes(filePath, FileAttributes.Normal); }
            using (FileStream locked = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                ConfirmDelete(form, true);
                Check(Find(form, "lblMessage").Text.Contains("Cannot access") && grid.Rows.Count == 2,
                    "Locked Delete remains friendly and leaves the grid unchanged");
            }
            Check(SameBytes(before, File.ReadAllBytes(filePath)), "Locked Delete preserves original bytes");
        }

        ResetFile();
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("af-ZA");
            using (MainForm form = OpenTestForm(filePath))
            {
                Button summary = (Button)Find(form, "btnGenerateSummary");
                Check(Find(form, "txtSummary") is TextBox && ((TextBox)Find(form, "txtSummary")).ReadOnly,
                    "Summary results use a persistent read-only form control");
                summary.PerformClick();
                string text = File.ReadAllText(summaryPath, Encoding.UTF8);
                Check(text == Find(form, "txtSummary").Text, "Saved Summary matches the displayed report");
                string[] lines = text.Replace("\r\n", "\n").Split('\n');
                DateTime generated;
                Check(lines.Length == 14 && lines[0] == "BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE"
                    && lines[1] == "ANIMAL SUMMARY REPORT", "Summary uses required title and line count");
                Check(DateTime.TryParseExact(lines[2].Substring("Generated: ".Length), "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out generated)
                    && Math.Abs((DateTime.Now - generated).TotalMinutes) < 2, "Summary timestamp uses minutes and current local time");
                string expected = string.Join(Environment.NewLine, new string[] {
                    "----------------------------------------",
                    "Total animals:            3",
                    "Average age (years):      6.33",
                    "Average recovery score:   61.00",
                    "----------------------------------------",
                    "Animals per status:",
                    "  Critical:               1",
                    "  Serious:                0",
                    "  Stable:                 0",
                    "  Recovering:             1",
                    "  Release-Ready:          1" });
                Check(string.Join(Environment.NewLine, lines, 3, 11) == expected,
                    "Summary uses exact layout, two decimal averages and all five status counts");

                File.AppendAllText(filePath, "malformed\n");
                summary.PerformClick();
                Check(Find(form, "lblMessage").Text.Contains("Skipped 1 malformed line(s).")
                    && File.ReadAllText(summaryPath).Contains("Total animals:            3"), "Summary skips malformed lines with feedback");
                byte[] savedReport = File.ReadAllBytes(summaryPath);
                File.SetAttributes(summaryPath, FileAttributes.ReadOnly);
                try
                {
                    summary.PerformClick();
                    Check(Find(form, "lblMessage").Text.Contains("Access denied"), "Read-only Summary output is handled");
                }
                finally { File.SetAttributes(summaryPath, FileAttributes.Normal); }
                using (FileStream locked = new FileStream(summaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    summary.PerformClick();
                    Check(Find(form, "lblMessage").Text.Contains("Cannot access"), "Locked Summary output is handled");
                }
                Check(SameBytes(savedReport, File.ReadAllBytes(summaryPath)), "Failed report saves preserve the previous report");
                using (FileStream locked = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    summary.PerformClick();
                    Check(Find(form, "lblMessage").Text.Contains("Cannot access"), "Locked Summary input is handled");
                }

                File.WriteAllText(filePath, string.Empty);
                summary.PerformClick();
                text = File.ReadAllText(summaryPath);
                Check(text == Find(form, "txtSummary").Text && text.Contains("Total animals:            0")
                    && text.Contains("Average age (years):      N/A")
                    && text.Contains("Average recovery score:   N/A"), "Empty Summary displays and saves zero total and N/A averages");
                string[] statuses = { "Critical", "Serious", "Stable", "Recovering", "Release-Ready" };
                foreach (string status in statuses)
                {
                    Check(text.Contains(("  " + status + ":").PadRight(26) + "0"), "Empty Summary includes zero for " + status);
                }
                Check(text.Split(new string[] { "ANIMAL SUMMARY REPORT" }, StringSplitOptions.None).Length == 2,
                    "Regenerating Summary overwrites instead of appending");
            }
        }
        finally { CultureInfo.CurrentCulture = originalCulture; }
    }

    // Answers only this test thread's Confirm Delete dialog; a timer runs inside the modal message loop.
    private static void ConfirmDelete(MainForm form, bool yes)
    {
        bool answered = false;
        bool correctDetails = false;
        Animal selected = (Animal)((DataGridView)Find(form, "dataGrid")).CurrentRow.DataBoundItem;
        int ticks = 0;
        using (Timer timer = new Timer())
        {
            timer.Interval = 50;
            timer.Tick += delegate
            {
                ticks++;
                EnumThreadWindows(GetCurrentThreadId(), delegate(IntPtr window, IntPtr state)
                {
                    StringBuilder title = new StringBuilder(128);
                    GetWindowText(window, title, title.Capacity);
                    if (title.ToString() == "Confirm Delete")
                    {
                        StringBuilder message = new StringBuilder(512);
                        GetWindowText(GetDlgItem(window, 65535), message, message.Capacity);
                        correctDetails = message.ToString().Contains(selected.Id) && message.ToString().Contains(selected.Name);
                        answered = true;
                        timer.Stop();
                        SendMessage(GetDlgItem(window, yes ? 6 : 7), 0x00F5, IntPtr.Zero, IntPtr.Zero);
                    }
                    return true;
                }, IntPtr.Zero);
                if (ticks > 100) timer.Stop();
            };
            timer.Start();
            ((Button)Find(form, "btnDelete")).PerformClick();
        }
        Check(answered && correctDetails, "Delete confirmation identifies the selected animal");
    }

    // Windows dialog helpers are used only by the checks, never by the application.
    private delegate bool WindowCallback(IntPtr window, IntPtr state);
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint thread, WindowCallback callback, IntPtr state);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder text, int size);
    [DllImport("user32.dll")] private static extern IntPtr GetDlgItem(IntPtr dialog, int id);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    // Opens an off-screen form backed by the supplied temporary fixture.
    private static MainForm OpenTestForm(string path)
    {
        MainForm form = new MainForm(new FileHandler(path));
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-10000, -10000);
        form.ShowInTaskbar = false;
        form.Show();
        Application.DoEvents();
        return form;
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
