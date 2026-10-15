# Baobab Ridge Wildlife Rehabilitation Records System

PRG2782 Project 2026. The GitHub repository is named `PRG281-Project`.

## Current status

**Member 3's Update feature saves to the real animals.txt file.** Search by ID or click a row, change an animal's details and click Save Changes. The file is updated, the grid reloads and the form returns to Add mode. Closing and reopening the application keeps saved changes.

The project also contains the shared model/rules, basic file loading and eight sample records needed to run and test Update. Add, Delete and Summary are still the other members' work. The Add button currently explains that it has not been implemented.

This is AI-assisted code. Review and understand it and follow the lecturer's rules on permitted assistance and attribution. It is not a claim of independently completed assessed work.

## Build and run

Use Windows and Visual Studio with the .NET desktop development workload and .NET 8 support (or a compatible newer SDK).

1. Clone or pull `main`.
2. Open `BaobabRidge.sln`.
3. Set `BaobabRidge` as the startup project and press F5.

From the repository folder:

```powershell
dotnet build BaobabRidge.sln
dotnet run --project BaobabRidge
dotnet run --project FrameworkChecks
```

There is no practice mode or `--file` switch anymore. The program always uses `animals.txt` beside its executable. The starter file is copied there with **Copy if newer**.

## Simple code structure

| File | Purpose |
| --- | --- |
| `Animal.cs` | Normal class with properties, constructor and a pipe-delimited ToString method |
| `AnimalRules.cs` | Shared validation and one if/else classification method |
| `FileHandler.cs` | ReadAnimals, parsing and safe UpdateAnimal file rewriting |
| `MainForm.cs` | Named button/grid events, input boxes, form state and friendly messages |
| `MainForm.Designer.cs` | Control layout and event connections |
| `Program.cs` | Starts the form |

The structure follows the class, List, FileHandler and text-file examples in the saved lecture materials. See [coding references](docs/CODING-STYLE.md). The application uses explicit types, ordinary methods and loops; it has no repository interface, records, lambda handlers or LINQ.

## Feature ownership

| Owner | Current support | Work still to complete |
| --- | --- | --- |
| Member 1 | Animal model, shared validation/classification, labelled inputs | Review shared rules; implement Add, uniqueness and append-only saving |
| Member 2 | Basic loading, grid, skipped-line notice, sample data and file-error messages | Review View/loading/error handling and extend the team's checks |
| Member 3 | Search, grid selection, edit state, validation and real file update | Understand the code, rehearse the demo and integrate with other features |
| Member 4 | Shared data access and UI helpers available | Implement Delete and Summary; finish team documentation |

## Team details

| Role | Name | Student number |
| --- | --- | --- |
| Member 1 | Keanu Vermeulen | 604066 |
| Member 2 | TO COMPLETE | TO COMPLETE |
| Member 3 | TO COMPLETE | TO COMPLETE |
| Member 4 | TO COMPLETE | TO COMPLETE |

## File formats

`animals.txt` is UTF-8 with seven pipe-separated fields per line, no header:
`ID|Name|Species|Age|Score|Status|Housing Unit`.
The starter file has eight fictional animals covering all five statuses. Loading skips malformed lines and reports their total once. Updates preserve unrelated lines, including malformed lines, and replace only the target record.

`summary.txt` is still to be implemented by Member 4. It must be generated beside the executable, overwritten each time, and use the exact brief section 5.2 layout: `yyyy-MM-dd HH:mm`, invariant-culture two-decimal averages, all five status counts and `N/A` averages for empty data. It is excluded from Git.

## Validation and limitations

The checks exercise real temporary files, classification boundaries, invalid input, first/middle/last record preservation, reopening, missing/duplicate IDs, malformed lines, read-only files, a Windows replacement lock and real form events. See [acceptance checks](docs/ACCEPTANCE.md).

This is a single-user text-file application. Saving rereads current data, but simultaneous writers during the read/replace operation are not supported. Close other writers while saving. A locked file produces a friendly error. The temporary-file replacement protects the original against a failed write; it is not a multi-user database transaction.

## Team workflow

Read [Member 3's guide](docs/MEMBER-3.md) and [integration notes](docs/INTEGRATION.md). This simplification was requested directly on `main`. Future assessed feature work should still follow the brief's feature-branch/PR requirements. Delete and Summary need separate feature branches even if Member 4 owns both.

The team needs at least 15 meaningful commits overall, at least three per member and work on at least three actual days, plus a merged PR per member. Four members making three commits each is only 12. Use correct identities and real dates. The existing first commit has only a README rather than the blank-project setup described in the brief; preserve history and clarify that discrepancy with the lecturer.
