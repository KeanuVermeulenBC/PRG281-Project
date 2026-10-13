# Member 3: Update walkthrough

## Read the code in this order

1. `Animal.cs`: properties, constructor and ToString.
2. `AnimalRules.cs`: TryCreate validation and ClassifyAnimal.
3. `MainForm.cs`: search, grid click, LoadForEditing, Save Changes and Clear.
4. `FileHandler.cs`: UpdateAnimal and the small loading/parsing helpers.

Update is implemented against the real file. The old practice repository, interface and unfinished TextAnimalRepository files have been removed.

## What happens when the user edits

Both a grid click and an exact case-insensitive ID search call `LoadForEditing`. That fills the same five boxes, remembers `editingId`, makes the ID read-only, disables Add and enables Save Changes.

Save calls `AnimalRules.TryCreate`. It trims input, rejects invalid fields and creates a new Animal. The shared classification method fills its status/housing even when the score is unchanged. The form focuses the failing field and leaves the file untouched if validation fails. It does not perform Add's uniqueness check.

The form calls `fileHandler.UpdateAnimal(editingId, updatedAnimal)`. It clears the editor and refreshes the grid only after the write succeeds. If saving fails, the original file and the user's entered values remain available. Clear cancels editing without saving.

Feedback appears in the label at the top of the form. Search-not-found changes the feedback but leaves the current edit alone.

## File rewriting, step by step

1. Check that the replacement ID matches the original.
2. Validate the replacement through the shared rules.
3. Use File.ReadAllLines to read the latest file into a string array.
4. Use a for loop to find exactly one matching ID. Missing, duplicate or malformed targets fail without writing.
5. Replace only that array element with the new animal's ToString result.
6. Use File.WriteAllLines to write a unique temporary file in the same folder.
7. Use File.Replace after writing finishes. Clean up a leftover temporary file if the operation fails.

The array contains raw unchanged lines for every other animal. Rewriting a list built from the grid could lose records or overwrite unrelated changes made after the grid was loaded, which is why the file is read again at save time.

File.Replace is the one extra safety step beyond the lecture's direct WriteAllLines example: it avoids truncating the only data file before the new content has been written. It can fail for locked files or permissions; the form catches those failures. Concurrent writers are not supported.

## Live demo

1. Start the application and search for `wr-0002`.
2. Verify Naledi's score is 79. If a prior demo saved 80, set it back to 79 first.
3. Change the score to 80 and save.
4. Show Release-Ready / Pre-Release Camp and the cleared editor.
5. Open the output animals.txt: only WR-0002 should differ.
6. Restart the program and search again to prove persistence.
7. Try a decimal age and explain the field-specific message.
8. Set a temporary test file read-only and explain why a failed save keeps the entered values.

## Your remaining personal work

Understand and explain each adopted method. Agree on names/signatures with the team, review their integration changes and run the checks yourself. Prepare at least three minutes of meaningful presentation contribution. Make genuine incremental changes under your own identity on actual separate days; generated commits do not replace demonstrating your own understanding or the lecturer's contribution requirements.
