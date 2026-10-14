using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace BaobabRidge
{
    public partial class MainForm : Form
    {
        private FileHandler fileHandler;
        private string editingId = string.Empty;
        private int skippedLines;

        // Opens the normal application using animals.txt beside the executable.
        public MainForm() : this(new FileHandler())
        {
        }

        // Allows automated checks to open the same form using a temporary data file.
        public MainForm(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            InitializeComponent();
        }

        // Starts in Add mode and loads the current file into the grid.
        private void MainForm_Load(object sender, EventArgs e)
        {
            ClearEditor();
            RefreshAnimals();
        }

        // Reloads records without adding duplicate grid rows; returns false if reading fails.
        private bool RefreshAnimals()
        {
            try
            {
                List<Animal> animals = fileHandler.ReadAnimals(out skippedLines);
                gridAnimals.DataSource = animals;
                gridAnimals.ClearSelection();
                gridAnimals.CurrentCell = null;
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

        // Reloads the grid without discarding unsaved editor values.
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshAnimals();
        }

        // Loads the clicked animal into the common editor.
        private void gridAnimals_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Animal animal = gridAnimals.Rows[e.RowIndex].DataBoundItem as Animal;
                if (animal != null)
                {
                    LoadForEditing(animal);
                    ShowMessage("Editing " + animal.Id + ".");
                }
            }
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

        // Makes grid selection and ID search use the same input boxes and form state.
        private void LoadForEditing(Animal animal)
        {
            editingId = animal.Id;
            txtId.Text = animal.Id;
            txtName.Text = animal.Name;
            txtSpecies.Text = animal.Species;
            txtAge.Text = animal.Age.ToString(CultureInfo.InvariantCulture);
            txtRecoveryScore.Text = animal.RecoveryScore.ToString(CultureInfo.InvariantCulture);
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
                ShowMessage("The update was saved, but the grid could not reload. Check the file and click Reload.");
            }
        }

        // Clears the editor without writing anything to the file.
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearEditor();
            ShowMessage("Editor cleared. Select an animal or search by Animal ID.");
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
            gridAnimals.ClearSelection();
            gridAnimals.CurrentCell = null;
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
