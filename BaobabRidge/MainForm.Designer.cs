using System;
using System.Drawing;
using System.Windows.Forms;

namespace BaobabRidge
{

    public partial class MainForm
    {
        private DataGridView gridAnimals = new DataGridView();
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtSpecies = new TextBox();
        private TextBox txtAge = new TextBox();
        private TextBox txtRecoveryScore = new TextBox();
        private TextBox txtSearchId = new TextBox();
        private Button btnAdd = new Button();
        private Button btnSaveChanges = new Button();
        private Button btnClear = new Button();
        private Button btnSearch = new Button();
        private Button btnRefresh = new Button();
        private Label lblClassification = new Label();
        private Label lblMessage = new Label();

        // Creates the common controls; agree on layout changes to avoid team merge conflicts.
        private void InitializeComponent()
        {
            Text = "Baobab Ridge - Animal Records";
            ClientSize = new Size(1120, 690);
            MinimumSize = new Size(920, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 6 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            Controls.Add(layout);
            lblMessage.Dock = DockStyle.Fill;
            lblMessage.Name = "lblMessage";
            lblClassification.Name = "lblClassification";
            lblMessage.ForeColor = Color.DarkSlateBlue;
            layout.Controls.Add(lblMessage, 0, 0);

            FlowLayoutPanel search = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            search.Controls.Add(new Label { Text = "Search Animal ID", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            txtSearchId.Width = 160;
            txtSearchId.Name = "txtSearchId";
            search.Controls.Add(txtSearchId);
            ConfigureButton(btnSearch, "Search", "btnSearch");
            ConfigureButton(btnRefresh, "Reload", "btnRefresh");
            search.Controls.AddRange(new Control[] { btnSearch, btnRefresh });
            layout.Controls.Add(search, 0, 1);

            gridAnimals.Name = "gridAnimals";
            gridAnimals.Dock = DockStyle.Fill;
            gridAnimals.ReadOnly = true;
            gridAnimals.AllowUserToAddRows = false;
            gridAnimals.AllowUserToDeleteRows = false;
            gridAnimals.MultiSelect = false;
            gridAnimals.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridAnimals.AutoGenerateColumns = false;
            gridAnimals.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridAnimals.RowHeadersVisible = false;
            string[] properties = { "Id", "Name", "Species", "Age", "RecoveryScore", "Status", "HousingUnit" };
            string[] headers = { "Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit" };
            for (int i = 0; i < properties.Length; i++)
                gridAnimals.Columns.Add(new DataGridViewTextBoxColumn { Name = properties[i], DataPropertyName = properties[i], HeaderText = headers[i], SortMode = DataGridViewColumnSortMode.NotSortable });
            layout.Controls.Add(gridAnimals, 0, 2);

            TableLayoutPanel editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(0, 14, 0, 0) };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            AddField(editor, "Animal ID", txtId, "txtId", 0, 0);
            AddField(editor, "Name", txtName, "txtName", 0, 1);
            AddField(editor, "Species", txtSpecies, "txtSpecies", 0, 2);
            AddField(editor, "Age (years)", txtAge, "txtAge", 2, 0);
            AddField(editor, "Recovery Score", txtRecoveryScore, "txtRecoveryScore", 2, 1);
            layout.Controls.Add(editor, 0, 3);
            lblClassification.Dock = DockStyle.Fill;
            layout.Controls.Add(lblClassification, 0, 4);

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
            ConfigureButton(btnAdd, "Add", "btnAdd");
            ConfigureButton(btnSaveChanges, "Save Changes", "btnSaveChanges");
            ConfigureButton(btnClear, "Clear", "btnClear");
            actions.Controls.AddRange(new Control[] { btnAdd, btnSaveChanges, btnClear });
            layout.Controls.Add(actions, 0, 5);

            // Named events match the usual Visual Studio WinForms event-handler pattern.
            Load += MainForm_Load;
            gridAnimals.CellClick += gridAnimals_CellClick;
            btnSearch.Click += btnSearch_Click;
            btnSaveChanges.Click += btnSaveChanges_Click;
            btnClear.Click += btnClear_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnAdd.Click += btnAdd_Click;
        }

        // Adds a consistently labelled input to the shared editor.
        private static void AddField(TableLayoutPanel panel, string label, TextBox input, string name, int column, int row)
        {
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 6, 8, 0) }, column, row);
            input.Name = name;
            input.Dock = DockStyle.Top;
            panel.Controls.Add(input, column + 1, row);
        }

        // Gives action buttons enough room for readable labels.
        private static void ConfigureButton(Button button, string text, string name)
        {
            button.Text = text;
            button.Name = name;
            button.AutoSize = true;
            button.Height = 34;
            button.Padding = new Padding(8, 0, 8, 0);
        }
    }
}
