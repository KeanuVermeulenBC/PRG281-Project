# Connecting the other members' work

Everyone uses the same Animal properties, AnimalRules methods, FileHandler instance and five input boxes.

| Member | Connection |
| --- | --- |
| 1: Add | Use AnimalRules.TryCreate, check unique IDs, append animal.ToString, then RefreshAnimals and ClearEditor |
| 2: View/errors | Review FileHandler.ReadAnimals / TryReadAnimal, RefreshAnimals, ShowFileError and starter animals.txt |
| 3: Update | Uses FileHandler.UpdateAnimal and named events in MainForm |
| 4: Delete/Summary | Add the remaining controls and FileHandler methods; reuse the current-file loader and form helpers |

## Shared methods

`AnimalRules.TryCreate(id, name, species, ageText, scoreText, out animal, out invalidField, out errorMessage)` is the shared input check. It calls `ClassifyAnimal`. Add must separately reject IDs already in the latest file. Update preserves the original ID.

`FileHandler.ReadAnimals(out skippedLines)` returns a List<Animal> in file order. It creates an empty missing file and counts malformed lines. It does not display messages itself.

`FileHandler.UpdateAnimal(originalId, replacement)` replaces one matching record using the latest file contents and fails on missing/ambiguous IDs. Keep that method signature when integrating.

`Animal.ToString()` returns exactly one pipe-separated record. Add should reuse it. Do not duplicate the score bands or the record format in feature handlers.

`RefreshAnimals()` reloads the grid and handles file errors. It returns false if reload failed. `ClearEditor()` resets edit state and the input boxes. `ShowMessage()` puts feedback in the top label.

## File path and errors

FileHandler owns the path `Path.Combine(AppContext.BaseDirectory, "animals.txt")`. The test constructor accepts a temporary path; the normal application never needs a user-specific path.

FileHandler lets storage exceptions reach the form. Every new file operation must be called inside the same try/catch pattern used by Search and Save. Do not report success or clear inputs after a failed write.

Add appends rather than rewrites. Delete should preserve every other record and use a safe temporary-file replacement. Summary uses current file data and writes summary.txt beside the executable.

## Form integration

Replace the placeholder `btnAdd_Click` for Add. MainForm is partial, so other members can put their event handlers in `MainForm.Add.cs`, `MainForm.Delete.cs` or `MainForm.Summary.cs` if useful. Remove the old placeholder before declaring the same handler elsewhere. Coordinate edits to the shared Designer file.

Delete must require a selected row and a Yes/No confirmation naming the ID and animal. After success, clear the editor if the deleted ID matches editingId, then refresh. Cancelling changes nothing.

Summary needs visible labels or read-only textboxes, all five status counts, invariant-culture averages, N/A when empty and the exact report layout from the brief.

Before submission, review all five features from a fresh clone, complete the member details, verify branch/PR requirements and make sure the GitHub main version matches the demo.
