# Member 3: completing Update

## Read these files in order

1. `Animal.cs`: the immutable record shared by all features.
2. `IAnimalRepository.cs`: the team's proposed storage contract.
3. `MainForm.cs`: `SearchAnimal`, `LoadForEditing`, `SaveChanges`, `ClearEditor`.
4. `AnimalRules.cs`: common validation and the single classification method.
5. `TextAnimalRepository.Update.cs`: **your main implementation task**.

The form demonstrates the Update flow. It does not prove file updates are correct. `PracticeAnimalRepository.Update` only changes a list; replacing that behavior with a real file rewrite is your responsibility.

## Connections

```text
Grid click / Search -> LoadForEditing -> same input boxes and edit mode
Save Changes -> AnimalRules.TryCreate -> repository.Update(originalId, replacement)
             -> ClearEditor + RefreshAnimals
Clear -> empty inputs and Add mode; no write
```

The original ID identifies a record, not its row number. Never use the grid as the authoritative source for a rewrite. `SaveChanges` does not run an Add-only uniqueness check. Failed writes keep entered values and edit mode. A successful write followed by a failed refresh is reported separately.

## Implement the file rewrite

Agree on parsing/serialization helpers with Member 2. Keep `Update(string originalId, Animal replacement)` so the form connects without changes.

1. Reject ID changes. Match with `StringComparison.OrdinalIgnoreCase`.
2. Read the latest file from `filePath`. If it disappears during editing, fail; do not rebuild it from a stale grid.
3. Find exactly one valid matching record. For missing/duplicate IDs, fail without writing.
4. Replace its line with seven correctly ordered fields. Preserve all other valid records and their positions. Retaining their original raw lines avoids unwanted normalization/reclassification. The brief permits omission of skipped malformed lines during rewriting; keeping them is also acceptable.
5. Write the replacement to a uniquely named temporary UTF-8 file in the same folder. Close/flush it before replacement.
6. Replace the original only after the temporary write succeeds. `File.Replace` is an option for an existing destination. Never delete the original first or truncate it before preparing the replacement.
7. Clean up a leftover temporary file in `finally` without hiding the original failure. Let expected file exceptions reach the form's friendly handler.

If the team supports external edits while the editor is open, agree on stale-update detection. At minimum, reading the latest file at save time must preserve unrelated changes already present then.

## Suggested actual work sessions

These are future milestones, not a claim of completed work. Commit meaningful changes on real separate days.

| Session | Your work | Example message |
| --- | --- | --- |
| Day 1 | Understand and adapt search/edit flow; agree on team contracts | `Refine animal search and edit mode integration` |
| Day 2 | Implement real rewrite; verify record preservation | `Implement safe animal file updates` |
| Day 3 | Test file failures, fix integration problems, record results | `Verify update errors and preserve unrelated records` |

Use `feature/update-animal`. Check `git config user.name` and `git config user.email` before committing. Reuse the draft PR into `main`; mark it ready only after file mode and the acceptance checks work. Today's scaffold alone cannot satisfy your contribution requirement.

## Walkthrough and live demo

Explain how grid/search reach one edit method, why `editingId` is stored separately, how shared rules work, and why the replacement file is prepared first. In file mode, change 79 to 80, show Release-Ready / Pre-Release Camp, compare file records, then restart to prove persistence. Include invalid input and a failed file write. Prepare at least three minutes of meaningful speaking and be able to explain and modify every method you adopt.
