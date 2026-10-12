# Team integration contract

Agree on this proposal together before parallel feature work. It is not a claim that other members have accepted the design.

| Owner | Files / methods | Connection |
| --- | --- | --- |
| Member 1 | `Animal.cs`, `AnimalRules.cs`, Add | Keep property names. Add and Update both use `TryCreate`/`Classify`; only Add checks uniqueness. |
| Member 2 | `TextAnimalRepository.Load`, parse/serialize helpers, starter file | Return file-order records and skipped count; display errors in the form. |
| Member 3 | `TextAnimalRepository.Update.cs`, Update form methods | Use shared rules, current file contents, original ID. |
| Member 4 | Delete, Summary, README | Reuse repository/path and refresh/clear hooks; separate branches for Delete and Summary. |

## Avoid merge conflicts

`TextAnimalRepository` and `MainForm` are partial classes. Put each member's handlers in files such as `MainForm.Add.cs`, `MainForm.Delete.cs`, `MainForm.Summary.cs` and `TextAnimalRepository.Delete.cs`. Move a placeholder once; never duplicate a method with the same signature. Coordinate layout changes in `MainForm.Designer.cs`. Do not create four MainForms or incompatible Animal classes.

`Program` constructs one repository instance. File mode uses `Path.Combine(AppContext.BaseDirectory, "animals.txt")`. Share this instance and path; do not use a working-directory-relative or user-specific path. The practice adapter is temporary.

## Shared rules

- Member 1 reviews/owns the provisional model and rules before connecting Add.
- `TryCreate` trims and validates, reports the first field issue, and always calls the single `Classify` method. Do not copy the score bands elsewhere.
- IDs are immutable. Update requires one target; Add checks current-file uniqueness.
- Grid bindings: `Id`, `Name`, `Species`, `Age`, `RecoveryScore`, `Status`, `HousingUnit`.
- UTF-8 file fields follow the same order. Never serialize grid display text or introduce a header.
- `Load` returns malformed-line count without UI messages. `RefreshAnimals` displays one aggregate warning per load.
- Preserve unrelated valid records' stored status/housing. Recalculate only added/edited records.
- Repository failures must throw. Do not swallow them and let the form falsely report success.

## Connect the other features

**Add:** replace the placeholder click message, call shared validation, perform Add-only uniqueness checking, append, show classification, refresh and clear after success.

**View:** implement Load, create an empty missing file, count skipped invalid rows and supply eight starter records spanning at least four statuses. The grid/startup hooks are ready.

**Delete:** add a button, require a selected row, confirm Yes/No with ID and name, then delete. On success, clear if `editingId` matches and refresh. Cancelling changes nothing.

**Summary:** add persistent labels/read-only textboxes and a button, load current file data, calculate counts/averages and write the exact section 5.2 report beside the executable. Use invariant culture and empty-case N/A averages.

Before submission, make file mode the default, complete member information, remove practice-only UI, and run acceptance checks from a fresh clone.
