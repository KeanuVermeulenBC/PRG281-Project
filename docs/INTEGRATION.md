# Team integration notes

Everyone uses the same Animal properties, AnimalRules methods, FileHandler and five input boxes.

| Owner | Existing connection |
| --- | --- |
| Keanu / Add | btnAdd_Click validates, calls AddAnimal, clears the editor and refreshes |
| Victor / View | ConfigureGrid sets up the grid; RefreshAnimals loads data and handles file errors |
| Renier / Update | Search and grid CellClick call LoadForEditing; Save calls UpdateAnimal, clears and refreshes |
| Dirk / Delete | Existing handler uses dataGrid.CurrentRow, confirms removal, calls DeleteAnimal and refreshes |
| Dirk / Summary | Prototype is on feature/generate-summary and still needs completion/merge |

## Grid and editor

- dataGrid is created once and docked inside pnlAnimalsView.
- Animal DisplayName attributes supply the exact seven headers.
- Columns cannot be sorted, preserving file order. Direct grid edits/add/delete are disabled.
- Only CellClick opens an edit. Programmatic refresh does not reopen the first row.
- RefreshAnimals reads a fresh list before replacing the grid source, clears selection and reports the skipped count. It returns false after friendly I/O/access feedback.
- ClearEditor empties inputs, clears grid selection/current cell and restores Add mode.
- Add/Update/Delete already call RefreshAnimals. Keep that connection when completing other features.

## Shared rules and file methods

TryCreate validates and classifies the supplied strings. Add separately checks uniqueness. Update uses the saved original ID and does not perform Add's uniqueness check.

ReadAnimals returns a List<Animal> in file order plus a skipped-line count. Animal.ToString returns the seven pipe-separated fields. Keep the same properties/format for all features.

UpdateAnimal reads current raw lines, finds exactly one ID, changes its line and writes a same-folder temporary file. It moves the completed file over the original after writing succeeds and cleans up on failure. Add and Delete retain their existing owner implementations.

FileHandler owns the path beside the executable. New file operations must use the form's friendly exception-handling pattern rather than showing raw stack traces.

## Completing Summary

Merge current main into the Summary branch and resolve imports/layout using the completed grid/editor connections. The prototype uses CultureInfo, so retain the appropriate System.Globalization import in the file containing its completed handler. Finish the report specification before merging it to main. Do not duplicate the classification bands or create a separate Animal type.
