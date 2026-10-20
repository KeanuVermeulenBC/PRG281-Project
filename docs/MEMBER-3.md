# Member 3: Update walkthrough

## Read the code in this order

1. `Animal.cs`: properties, constructor and ToString.
2. `AnimalRules.cs`: TryCreate validation and ClassifyAnimal.
3. `MainForm.cs`: search, LoadForEditing, Save Changes and Clear.
4. `FileHandler.cs`: UpdateAnimal and the small loading/parsing helpers.

Update is implemented against the real file. Search, validation, edit state, classification and file rewriting are complete. The grid now connects to the editor; full Feature 3 verification still needs actual grid refresh after saving.

## What happens when the user edits

An exact case-insensitive ID search and grid selection both call `LoadForEditing`. That fills the five boxes, remembers `editingId`, makes the ID read-only, disables Add and enables Save Changes. An empty selection with no Animal is ignored.

Age and Recovery Score are whole numbers, so the editor uses ordinary conversions:

```csharp
txtAge.Text = animal.Age.ToString();
txtRecoveryScore.Text = animal.RecoveryScore.ToString();
```

`ToString()` converts a number to textbox text. No culture argument is needed for these valid 0-100 values. Decimal summary averages still need the brief's two-decimal, full-stop formatting. The shared file-format code is unchanged.

Save calls `AnimalRules.TryCreate`. It trims input, rejects invalid fields and creates a new Animal. The shared classification method fills its status/housing even when the score is unchanged. The form focuses the failing field and leaves the file untouched if validation fails. It does not perform Add's uniqueness check.

The form calls `fileHandler.UpdateAnimal(editingId, updatedAnimal)`. It clears the editor and calls the `RefreshAnimals` connection point only after the write succeeds. That method is currently a placeholder for Victor's grid; it performs no loading or binding. If saving fails, the original file and the user's entered values remain available. Clear cancels editing without saving.

Feedback appears in the label at the top of the form. Search-not-found changes the feedback but leaves the current edit alone.

The form can start in edit mode if the grid selects an existing animal. Clear returns it to empty Add mode. Tests use Clear to establish that state rather than assuming startup must be blank.

## File rewriting, step by step

1. Check that the replacement ID matches the original.
2. Validate the replacement through the shared rules.
3. Use File.ReadAllLines to read the latest file into a string array.
4. Use a for loop to remember the matching line index. A second match is rejected immediately; missing or malformed targets also fail without writing.
5. Replace only that array element with the new animal's ToString result.
6. Use File.WriteAllLines to write a unique temporary file in the same folder.
7. Use File.Replace after writing finishes. Clean up a leftover temporary file if the operation fails.

The array contains raw unchanged lines for every other animal. Rewriting a list built from the grid could lose records or overwrite unrelated changes made after the grid was loaded, which is why the file is read again at save time.

File.Replace is the one extra safety step beyond the lecture's direct WriteAllLines example: it avoids truncating the only data file before the new content has been written. It can fail for locked files or permissions; the form catches those failures. Concurrent writers are not supported.

## Live demo

1. Start the application and search for `wr-0002`.
2. Verify Naledi's score is 79. If a prior demo saved 80, set it back to 79 first.
3. Change the score to 80 and save.
4. Show Release-Ready / Pre-Release Camp in the feedback label and the cleared editor.
5. Open the output animals.txt: only WR-0002 should differ.
6. Restart the program and search again to prove persistence.
7. Try a decimal age and explain the field-specific message.
8. Set a temporary test file read-only and explain why a failed save keeps the entered values.

## Your remaining personal work

As audited on 19 October, your three non-merge commits dated 12, 13 and 15 October meet the minimum volume/date requirement, and Update PR #1 is merged. This does not replace the presentation requirement or the final grid integration check. Other members must establish their own contributions on actual dates.

Understand and explain each adopted method. Agree on names/signatures with the team, review their integration changes and run the checks yourself. Prepare at least three minutes of meaningful presentation contribution. Make genuine incremental changes under your own identity on actual separate days; generated commits do not replace demonstrating your own understanding or the lecturer's contribution requirements.
