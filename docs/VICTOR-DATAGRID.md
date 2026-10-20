# View implementation notes

Victor's DataGridView and Animal display-name attributes are retained and integrated with Add, Update and Delete.

## Flow

1. MainForm's constructor builds the controls and calls ConfigureGrid once.
2. MainForm_Load clears the editor and calls RefreshAnimals.
3. RefreshAnimals reads valid records, binds a fresh list in file order, clears selection and reports skipped lines. File errors are handled without closing the form.
4. A grid CellClick calls LoadForEditing, which is also used by ID Search.
5. Successful writes call the same refresh method. Rebinding never enters edit mode by itself.

The grid fills pnlAnimalsView and resizes with the form. It is read-only, uses whole-row single selection, disallows direct add/delete and disables column sorting. Long values wrap into readable rows. The seven headers come from the model's DisplayName attributes.

## Why CellClick is used

SelectionChanged also runs when DataSource is rebound. Loading the editor from that event caused a successful Save to reopen the first animal and disable Add again. CellClick opens edits only after a deliberate row click. Clear and refresh both remove the selected/current row.

## Checks

FrameworkChecks covers the real startup grid, header order, restrictions, row-click connection, Clear, Add/Update refresh, skipped-line notice, locked startup, failed refresh and missing folders. The tests use temporary data; the group should still rehearse the live demonstration with the final project.
