# Baobab Ridge Wildlife Rehabilitation Records System

PRG2782 Project 2026. The GitHub repository is currently named `PRG281-Project`.

## Framework status

This is an **AI-assisted learning scaffold for Member 3**, not a completed project or submission. Review it, understand it and follow your lecturer's rules on permitted assistance. The brief says submitting AI-generated code or text as your own is misconduct. This scaffold does not establish assessed contribution or satisfy the three-day commit requirement.

The Windows Forms application targets .NET 8 and uses no storage packages. It starts in **practice mode**: three example animals are held in memory. Click a row or search by ID, edit values, save a practice change and clear the editor. Closing the app discards these changes. Practice data is not the required starter `animals.txt`.

**Real file loading, appending, updating and deleting are deliberately unfinished.** Their methods throw explanatory exceptions handled by the form. Add displays an integration message. Delete and Summary controls still need to be created by Member 4.

## Build and run

On Windows, use Visual Studio with the .NET desktop development workload and .NET 8 support (or a compatible newer SDK). Switch to `feature/update-animal` while the draft is under review. Open `BaobabRidge.sln`, set `BaobabRidge` as startup project, build and press F5.

From the repository folder:

```powershell
dotnet build BaobabRidge.sln
dotnet run --project BaobabRidge
dotnet run --project FrameworkChecks
```

After implementing the production repository methods, run file mode:

```powershell
dotnet run --project BaobabRidge -- --file
```

Before submission, make file mode the default and remove the temporary practice adapter.

## Team details to complete

| Role | Name | Student number |
| --- | --- | --- |
| Member 1: model, shared rules, Add | TO COMPLETE | TO COMPLETE |
| Member 2: View, loading, errors, starter data | TO COMPLETE | TO COMPLETE |
| Member 3: Update | TO COMPLETE | TO COMPLETE |
| Member 4: Delete, Summary, documentation | TO COMPLETE | TO COMPLETE |

## Feature status

| Feature | Present | Remaining |
| --- | --- | --- |
| Add | Labelled inputs and integration button | Uniqueness, append, success flow |
| View | Read-only grid, exact columns, refresh hook, practice data | UTF-8 loader, skipped lines, missing file, starter records |
| Update | Search, edit state, shared validation, practice save | Safe file rewrite and file-based verification |
| Delete | Repository contract | Selection, confirmation, removal, editor reset |
| Summary | Integration guidance | Controls, calculations, exact report output |

## File formats

`animals.txt` belongs beside the executable and uses UTF-8, no header and no blank lines. Fields are `ID|Name|Species|Age|Score|Status|Housing Unit`, one record per line. Member 2 must add `BaobabRidge/animals.txt` with at least eight valid records spanning at least four statuses. The project already specifies Copy if newer (`PreserveNewest`) for that future file.

`summary.txt` is generated beside the executable, overwritten for every report and excluded from Git. Follow brief section 5.2 exactly: `yyyy-MM-dd HH:mm`, invariant-culture two-decimal averages, all five statuses, and `N/A` averages for an empty file.

## Start here

- [Member 3 completion guide](docs/MEMBER-3.md)
- [Team integration contract](docs/INTEGRATION.md)
- [Acceptance checklist](docs/ACCEPTANCE.md)

## Git requirements still to satisfy

The repository's existing first commit contains only a README; the brief asks for a blank WinForms project, `.gitignore` and README in that first commit. This scaffold preserves existing history. Ask the lecturer how to handle that discrepancy; do not rewrite history to pretend it began differently.

The brief requires one branch for each of the five features. Member 4 should use separate `feature/delete-animal` and `feature/summary-report` branches and PRs. The team needs at least 15 meaningful commits overall, at least three per member, work across at least three actual days and a merged PR per member. Three commits each from four members total only 12. Use actual dates and correct contributor identities; do not backdate commits. Keep `main` buildable and merge reviewed work through PRs.
