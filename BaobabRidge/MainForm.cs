using System.Globalization;

namespace BaobabRidge;

public sealed partial class MainForm : Form
{
    private readonly IAnimalRepository repository;
    private string? editingId;
    private readonly bool practiceMode;

    // Builds the shared form and wires Member 3's editing workflow.
    public MainForm(IAnimalRepository repository, bool practiceMode)
    {
        this.repository = repository;
        this.practiceMode = practiceMode;
        InitializeComponent();
        lblMode.Text = practiceMode
            ? "PRACTICE MODE - edits are in memory only and reset when you close the app."
            : "FILE MODE - production repository methods must be completed by the team.";
        Shown += (_, _) => RunFileAction(RefreshAnimals);
        gridAnimals.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && gridAnimals.Rows[e.RowIndex].DataBoundItem is Animal animal)
                LoadForEditing(animal);
        };
        btnSearch.Click += (_, _) => SearchAnimal();
        btnSaveChanges.Click += (_, _) => SaveChanges();
        btnClear.Click += (_, _) => ClearEditor();
        btnRefresh.Click += (_, _) => RunFileAction(RefreshAnimals);
        btnAdd.Click += (_, _) => MessageBox.Show(this, "Member 1: connect Add here using AnimalRules.TryCreate, repository.Add, RefreshAnimals and ClearEditor.");
        ClearEditor();
    }

    // Reloads a fresh snapshot, with no duplicate rows or implicit selection/editing.
    private void RefreshAnimals()
    {
        AnimalLoadResult result = repository.Load();
        gridAnimals.DataSource = result.Animals.ToList();
        gridAnimals.ClearSelection();
        gridAnimals.CurrentCell = null;
        if (result.SkippedLines > 0)
            MessageBox.Show(this, $"Skipped {result.SkippedLines} malformed line(s).", "Load animals");
    }

    // Finds an exact ID from the current repository; a failed search leaves the editor unchanged.
    private void SearchAnimal()
    {
        RunFileAction(() =>
        {
            string id = txtSearchId.Text.Trim();
            if (id.Length == 0)
            {
                MessageBox.Show(this, "Enter an Animal ID to search for.");
                txtSearchId.Focus();
                return;
            }
            Animal? animal = repository.Load().Animals.FirstOrDefault(a =>
                string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
            if (animal == null)
            {
                MessageBox.Show(this, $"Animal ID {id} was not found.");
                return;
            }
            LoadForEditing(animal);
        });
    }

    // Sends either a clicked row or a search result to the same input boxes and edit state.
    private void LoadForEditing(Animal animal)
    {
        editingId = animal.Id;
        txtId.Text = animal.Id;
        txtName.Text = animal.Name;
        txtSpecies.Text = animal.Species;
        txtAge.Text = animal.Age.ToString(CultureInfo.InvariantCulture);
        txtRecoveryScore.Text = animal.RecoveryScore.ToString(CultureInfo.InvariantCulture);
        lblClassification.Text = $"{animal.Status} | {animal.HousingUnit}";
        txtId.ReadOnly = true;
        btnAdd.Enabled = false;
        btnSaveChanges.Enabled = true;
        txtName.Focus();
    }

    // Validates through Member 1's shared method and delegates storage to Member 3's adapter.
    private void SaveChanges()
    {
        if (editingId == null)
        {
            MessageBox.Show(this, "Select or search for an animal before saving.");
            return;
        }
        // Use the original ID as identity, never an editable textbox or a grid row number.
        var input = new AnimalInput(editingId, txtName.Text, txtSpecies.Text, txtAge.Text, txtRecoveryScore.Text);
        if (!AnimalRules.TryCreate(input, out Animal? updated, out ValidationIssue? issue))
        {
            MessageBox.Show(this, issue!.Message, "Check your input");
            Control field = issue.Field switch
            {
                "Id" => txtId, "Name" => txtName, "Species" => txtSpecies,
                "Age" => txtAge, _ => txtRecoveryScore
            };
            field.Focus();
            return;
        }
        if (!RunFileAction(() => repository.Update(editingId, updated!))) return;
        ClearEditor();
        // Report refresh failure separately: the storage operation has already succeeded.
        if (!RunFileAction(RefreshAnimals))
        {
            MessageBox.Show(this, "The update was saved, but the grid could not reload. Click Reload when the file is available.");
            return;
        }
        string scope = practiceMode ? "Updated for practice only (not saved to disk)." : "Animal updated.";
        MessageBox.Show(this, $"{scope}\n{updated!.Status} | {updated.HousingUnit}");
    }

    // Returns to Add mode with empty inputs; Member 1 and Member 4 reuse this after changes.
    private void ClearEditor()
    {
        editingId = null;
        foreach (TextBox field in new[] { txtId, txtName, txtSpecies, txtAge, txtRecoveryScore, txtSearchId })
            field.Clear();
        txtId.ReadOnly = false;
        btnAdd.Enabled = true;
        btnSaveChanges.Enabled = false;
        lblClassification.Text = "Status and housing are calculated from the recovery score.";
        gridAnimals.ClearSelection();
        gridAnimals.CurrentCell = null;
        txtId.Focus();
    }

    // Keeps expected storage errors and unfinished framework hooks from crashing the form.
    private bool RunFileAction(Action action)
    {
        try { action(); return true; }
        catch (NotImplementedException ex) { MessageBox.Show(this, ex.Message, "Framework task remaining"); }
        catch (FileNotFoundException) { MessageBox.Show(this, "The animals file was not found. Reload before trying again."); }
        catch (DirectoryNotFoundException) { MessageBox.Show(this, "The data folder was not found. Check the application folder."); }
        catch (UnauthorizedAccessException) { MessageBox.Show(this, "Permission was denied. Check whether the file is read-only and whether the folder allows writing."); }
        catch (IOException) { MessageBox.Show(this, "The file could not be accessed. Close other programs using it and check available disk space, then try again."); }
        catch (InvalidOperationException ex) { MessageBox.Show(this, ex.Message, "Could not complete the action"); }
        return false;
    }
}
