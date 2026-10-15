# Connecting the other members' work

Everyone uses the same Animal properties, AnimalRules methods, FileHandler instance and five input boxes.

| Member | Connection |
| --- | --- |
| 1: Keanu / Add | Existing committed Add handler, IDExists and AddAnimal are retained |
| 2: Victor / View | Implement the DataGridView in pnlAnimalsView, startup loading, RefreshAnimals and row selection |
| 3: Update | Uses FileHandler.UpdateAnimal and named events in MainForm |
| 4: Delete/Summary | Add the remaining controls and FileHandler methods; reuse the current-file loader and form helpers |

## Shared methods

`AnimalRules.TryCreate(id, name, species, ageText, scoreText, out animal, out invalidField, out errorMessage)` is the shared input check. It calls `ClassifyAnimal`. Add must separately reject IDs already in the latest file. Update preserves the original ID.

`FileHandler.ReadAnimals(out skippedLines)` returns a List<Animal> in file order. It creates an empty missing file and counts malformed lines. It does not display messages itself.

`FileHandler.UpdateAnimal(originalId, replacement)` replaces one matching record using the latest file contents and fails on missing/ambiguous IDs. Keep that method signature when integrating.

`Animal.ToString()` returns exactly one pipe-separated record. Add should reuse it. Do not duplicate the score bands or the record format in feature handlers.

`RefreshAnimals()` is now an empty integration hook for Victor. It returns true until he implements the actual grid refresh; existing Add and Update calls remain in place. Victor's implementation should return false on a load failure. `ClearEditor()` resets edit state and inputs. `ShowMessage()` puts feedback in the top label.

## File path and errors

FileHandler owns the path `Path.Combine(AppContext.BaseDirectory, "animals.txt")`. The test constructor accepts a temporary path; the normal application never needs a user-specific path.

FileHandler lets storage exceptions reach the form. Every new file operation must be called inside the same try/catch pattern used by Search and Save. Do not report success or clear inputs after a failed write.

Add appends rather than rewrites. Delete should preserve every other record and use a safe temporary-file replacement. Summary uses current file data and writes summary.txt beside the executable.

## Form integration

Keep Keanu's completed `btnAdd_Click`, `IDExists` and `AddAnimal` methods. MainForm is partial, so Victor can place view handlers in `MainForm.View.cs` if useful. Coordinate edits to the shared Designer file and avoid declaring the same method twice.

Victor's [DataGridView handoff](VICTOR-DATAGRID.md) identifies the blank panel, startup hook, refresh hook and editor method. The existing ReadAnimals/TryReadAnimal helpers are retained because Search and Update depend on them; they do not display a grid.

Delete must require a selected row and a Yes/No confirmation naming the ID and animal. After success, clear the editor if the deleted ID matches editingId, then refresh. Cancelling changes nothing.

Summary needs visible labels or read-only textboxes, all five status counts, invariant-culture averages, N/A when empty and the exact report layout from the brief.

Before submission, review all five features from a fresh clone, complete the member details, verify branch/PR requirements and make sure the GitHub main version matches the demo.
