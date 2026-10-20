using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace BaobabRidge
{
    public partial class MainForm : Form
    {
        private FileHandler fileHandler;
        private string editingId = string.Empty;
        private int skippedLines;
        private DataGridView dataGrid = new DataGridView();

        // Opens the normal application using animals.txt beside the executable.
        public MainForm() : this(new FileHandler())
        {
        }

        // Allows automated checks to open the same form using a temporary data file.
        public MainForm(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            InitializeComponent();
            ConfigureGrid();
        }

        // Starts with an empty editor and loads the file using friendly error handling.
        private void MainForm_Load(object sender, EventArgs e)
        {
            ClearEditor();
            RefreshAnimals();
        }

        // Configures Victor's grid once; file data is loaded separately by RefreshAnimals.
        private void ConfigureGrid()
        {
            dataGrid.Name = "dataGrid";
            dataGrid.Dock = DockStyle.Fill;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.ReadOnly = true;
            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToDeleteRows = false;
            dataGrid.MultiSelect = false;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGrid.RowHeadersVisible = false;
            // DisplayName attributes on Animal provide the seven required column headers.
            dataGrid.DataBindingComplete += dataGrid_DataBindingComplete;
            dataGrid.CellClick += dataGrid_CellClick;
            pnlAnimalsView.Controls.Add(dataGrid);
        }

        // Keeps records in file order instead of allowing header clicks to sort them.
        private void dataGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        // Only a user's row click opens the editor; programmatic refreshes do not enter edit mode.
        private void dataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGrid.Rows.Count)
            {
                Animal selected = dataGrid.Rows[e.RowIndex].DataBoundItem as Animal;
                LoadForEditing(selected);
            }
        }

        // Loads a fresh snapshot, clears selection and reports skipped lines once per load.
        private bool RefreshAnimals()
        {
            try
            {
                // Read first so a failed reload leaves the currently displayed records available.
                List<Animal> animals = fileHandler.ReadAnimals(out skippedLines);
                dataGrid.DataSource = animals;
                dataGrid.ClearSelection();
                dataGrid.CurrentCell = null;
                ShowMessage("Loaded " + animals.Count + " animals." + SkippedMessage());
                return true;
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
            }
            return false;
        }

        // Uses a foreach loop to find one exact ID, ignoring upper/lower case.
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchId = txtSearchId.Text.Trim();
            if (searchId == string.Empty)
            {
                ShowMessage("Enter an Animal ID to search for.");
                txtSearchId.Focus();
                return;
            }

            try
            {
                List<Animal> animals = fileHandler.ReadAnimals(out skippedLines);
                foreach (Animal animal in animals)
                {
                    if (string.Equals(animal.Id, searchId, StringComparison.OrdinalIgnoreCase))
                    {
                        LoadForEditing(animal);
                        ShowMessage("Editing " + animal.Id + "." + SkippedMessage());
                        return;
                    }
                }
                // Do not clear or replace the current editor when there is no exact match.
                ShowMessage("Animal ID " + searchId + " was not found." + SkippedMessage());
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
            }
        }

        // Loads one animal into the editor, whether it came from Search or the grid.
        private void LoadForEditing(Animal animal)
        {
            // A blank grid row has no Animal to edit.
            if (animal == null)
            {
                return;
            }

            editingId = animal.Id;
            txtId.Text = animal.Id;
            txtName.Text = animal.Name;
            txtSpecies.Text = animal.Species;
            txtAge.Text = animal.Age.ToString();
            txtRecoveryScore.Text = animal.RecoveryScore.ToString();
            lblClassification.Text = animal.Status + " | " + animal.HousingUnit;
            txtId.ReadOnly = true;
            btnAdd.Enabled = false;
            btnSaveChanges.Enabled = true;
            txtName.Focus();
        }

        // Validates the edit, saves it to the real file and returns to Add mode after success.
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (editingId == string.Empty)
            {
                ShowMessage("Select or search for an animal before saving.");
                return;
            }

            Animal updatedAnimal;
            string invalidField;
            string errorMessage;
            // The stored original ID is the identity, even if a textbox is changed by other code.
            if (!AnimalRules.TryCreate(editingId, txtName.Text, txtSpecies.Text,
                txtAge.Text, txtRecoveryScore.Text, out updatedAnimal, out invalidField, out errorMessage))
            {
                ShowMessage(errorMessage);
                FocusField(invalidField);
                return;
            }

            try
            {
                fileHandler.UpdateAnimal(editingId, updatedAnimal);
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage(ex.Message);
                return;
            }

            // Clear only after saving succeeded; failed writes leave the edit available to retry.
            ClearEditor();
            if (RefreshAnimals())
            {
                ShowMessage("Saved " + updatedAnimal.Id + ": " + updatedAnimal.Status
                    + " | " + updatedAnimal.HousingUnit + "." + SkippedMessage());
            }
            else
            {
                ShowMessage("The update was saved, but the animal list could not reload. Check file access and reopen the application.");
            }
        }

        // Clears the editor without writing anything to the file.
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearEditor();
            ShowMessage("Editor cleared. Select an animal or search by Animal ID.");
        }

        // Calculates the full summary, shows it on the form and saves it to summary.txt.
        private void btnGenerateSummary_Click(object sender, EventArgs e)
        {
            string[] statusNames = { "Critical", "Serious", "Stable", "Recovering", "Release-Ready" };

            try
            {
                List<Animal> animals = fileHandler.ReadAnimals(out skippedLines);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                double totalAge = 0;
                double totalScore = 0;
                int[] statusCounts = new int[statusNames.Length];

                foreach (Animal animal in animals)
                {
                    totalAge += animal.Age;
                    totalScore += animal.RecoveryScore;
                    for (int i = 0; i < statusNames.Length; i++)
                    {
                        if (animal.Status == statusNames[i])
                        {
                            statusCounts[i]++;
                        }
                    }
                }

                // Empty data still has the full report, with N/A averages and zero counts.
                string averageAge = "N/A";
                string averageScore = "N/A";
                if (animals.Count > 0)
                {
                    averageAge = (totalAge / animals.Count).ToString("0.00", CultureInfo.InvariantCulture);
                    averageScore = (totalScore / animals.Count).ToString("0.00", CultureInfo.InvariantCulture);
                }

                // PadRight aligns values at the column shown in brief section 5.2.
                string summaryText = "BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE" + Environment.NewLine
                    + "ANIMAL SUMMARY REPORT" + Environment.NewLine
                    + "Generated: " + timestamp + Environment.NewLine
                    + "----------------------------------------" + Environment.NewLine
                    + "Total animals:".PadRight(26) + animals.Count + Environment.NewLine
                    + "Average age (years):".PadRight(26) + averageAge + Environment.NewLine
                    + "Average recovery score:".PadRight(26) + averageScore + Environment.NewLine
                    + "----------------------------------------" + Environment.NewLine
                    + "Animals per status:";
                for (int i = 0; i < statusNames.Length; i++)
                {
                    summaryText += Environment.NewLine + ("  " + statusNames[i] + ":").PadRight(26) + statusCounts[i];
                }

                txtSummary.Text = summaryText;
                fileHandler.SaveSummary(summaryText);
                ShowMessage("Summary generated and saved to summary.txt." + SkippedMessage());
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
            }
        }
        // Deletes the animal selected in the grid after a Yes/No confirmation.
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string selectedId;
            string selectedName;
            if (this.dataGrid.CurrentRow == null)
            {
                ShowMessage("Select one animal in the grid before deleting.");
                return;
            }

            //from here we can assume that a row is selected, so we extract the id and name for the rest
            Animal selectedAnimal = this.dataGrid.CurrentRow.DataBoundItem as Animal;
            selectedId = selectedAnimal.Id;
            selectedName = selectedAnimal.Name;


            DialogResult answer = MessageBox.Show(
                "Delete " + selectedId + " (" + selectedName + ")? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes)
            {
                ShowMessage("Delete cancelled. Nothing was changed.");
                return;
            }

            try
            {
                fileHandler.DeleteAnimal(selectedId);
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage(ex.Message);
                return;
            }

            // Clear the editor only if the deleted animal was the one loaded for editing.
            if (string.Equals(editingId, selectedId, StringComparison.OrdinalIgnoreCase))
            {
                ClearEditor();
            }

            if (RefreshAnimals())
            {
                ShowMessage("Deleted " + selectedId + " (" + selectedName + ")." + SkippedMessage());
            }
            else
            {
                ShowMessage("The animal was deleted, but the grid could not reload. Check the file and click Reload.");
            }
        }

        // Returns to Add mode; Add and Delete can reuse this after successful changes.
        private void ClearEditor()
        {
            editingId = string.Empty;
            txtId.Clear();
            txtName.Clear();
            txtSpecies.Clear();
            txtAge.Clear();
            txtRecoveryScore.Clear();
            txtSearchId.Clear();
            txtId.ReadOnly = false;
            btnAdd.Enabled = true;
            btnSaveChanges.Enabled = false;
            lblClassification.Text = "Status and housing are calculated from the recovery score.";
            dataGrid.ClearSelection();
            dataGrid.CurrentCell = null;
            txtId.Focus();
        }

        // Leaves Add with its feature owner while keeping the shared inputs ready.
        private void btnAdd_Click(object sender, EventArgs e)
        {
            Animal newAnimal;
            string invalidField;
            string errorMessage;

            if (!AnimalRules.TryCreate(txtId.Text, txtName.Text, txtSpecies.Text,
                txtAge.Text, txtRecoveryScore.Text, out newAnimal, out invalidField, out errorMessage))
            {
                ShowMessage(errorMessage);
                FocusField(invalidField);
                return;
            }

            try
            {
                fileHandler.AddAnimal(newAnimal);
            }
            catch (IOException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError(ex);
                return;
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage(ex.Message);
                txtId.Focus();
                return;
            }

            // Clear and reload only after the animal was written; a failed save keeps the typed values.
            ClearEditor();
            if (RefreshAnimals())
            {
                ShowMessage("Added " + newAnimal.Id + ": " + newAnimal.Status
                    + " | " + newAnimal.HousingUnit + "." + SkippedMessage());
            }
            else
            {
                ShowMessage("The added animal was saved, but the grid could not reload. Check the file and click Reload.");
            }
        }

        // Returns the cursor to the input that failed validation.
        private void FocusField(string field)
        {
            if (field == "Id") txtId.Focus();
            else if (field == "Name") txtName.Focus();
            else if (field == "Species") txtSpecies.Focus();
            else if (field == "Age") txtAge.Focus();
            else txtRecoveryScore.Focus();
        }

        // Shows feedback in a persistent label, without interrupting the editor with a dialog.
        private void ShowMessage(string message)
        {
            lblMessage.Text = message;
        }

        // Adds one aggregate malformed-line notice to the result of a load.
        private string SkippedMessage()
        {
            if (skippedLines > 0)
            {
                return " Skipped " + skippedLines + " malformed line(s).";
            }
            return string.Empty;
        }

        // Explains expected storage failures without displaying a stack trace.
        private void ShowFileError(Exception error)
        {
            if (error is FileNotFoundException)
            {
                ShowMessage("The animals file was not found. Reload before trying again.");
            }
            else if (error is DirectoryNotFoundException)
            {
                ShowMessage("The data folder was not found. Check the application folder.");
            }
            else if (error is UnauthorizedAccessException)
            {
                ShowMessage("Access denied. Check file read-only settings and folder permissions.");
            }
            else
            {
                ShowMessage("Cannot access the file. Close programs using it and check disk space, then try again.");
            }
        }
    }
}
