# Baobab Ridge Wildlife Rehabilitation Records System

PRG2782 Project 2026 — C# Windows Forms, .NET 8, UTF-8 text files, Git and GitHub.

Repository: [KeanuVermeulenBC/PRG281-Project](https://github.com/KeanuVermeulenBC/PRG281-Project).
Status reviewed on **19 October 2026** against `main` at `a5e465b` and the pushed feature branches.

## Current project status

The application can add animals and find/update them by ID. Saved changes persist in animals.txt after restarting.

**The group application is still in progress.** Victor's DataGridView is pending, so records are not displayed on startup, grid row selection is unavailable, and the grid refresh connection is still a placeholder. Dirk's Delete button and file method are merged, but deleting through the normal UI requires Victor's grid. A first Summary implementation exists only on `feature/generate-summary`.

| Feature | Owner | Verified repository state | Remaining integration |
| --- | --- | --- | --- |
| Add | Keanu | Input validation, duplicate-ID check and append-only saving are implemented on main | Refresh Victor's grid after saving |
| View | Johan Victor | Reserved blank panel; shared file-reading helper is available | Implement startup display, columns, read-only selection, row clicks and refresh |
| Update | Renier | Exact case-insensitive search, edit mode, shared validation/classification and safe file replacement work on main | Connect Victor's row click to LoadForEditing and verify actual grid refresh |
| Delete | Dirk | Button, selection check, Yes/No confirmation and file method are merged through PR #2 | Select and refresh records through Victor's grid; finish end-to-end verification |
| Summary | Dirk | Separate branch has a button, total count and average-age prototype | Finish all required figures/report formatting, write summary.txt, handle empty data and merge through a PR |

The current review changes only Member 3's Update implementation, Update test diagnostics and documentation. Other members' feature implementations are preserved.

## Team

| Member | Name | Student number | Responsibility |
| --- | --- | --- | --- |
| 1 | Keanu Vermeulen | 604066 | Animal model, classification, shared rules and Add |
| 2 | Johan Victor Liebenberg | AWAITING CONFIRMATION | DataGridView / View and related loading integration |
| 3 | Renier Van Rooyen | 604214 | Update, edit mode and file rewriting |
| 4 | Dirk Geldenhuys | 603556 | Delete, Summary and repository documentation |

Victor's full name was confirmed by the team. His student number still needs to be supplied; it has not been guessed.

## Build and run

Requirements: Windows, a .NET 8 SDK or compatible newer SDK, or Visual Studio with the **.NET desktop development** workload and .NET 8 support. No storage-related NuGet packages or database are used.

1. Clone or pull the repository's main branch.
2. Open `BaobabRidge.sln`.
3. Set `BaobabRidge` as the startup project, build and press F5.

From the repository folder:

```powershell
dotnet build BaobabRidge.sln
dotnet run --project BaobabRidge
dotnet run --project FrameworkChecks
```

The normal application always uses the real animals.txt file. There is no practice-mode switch. File paths are based on the executable directory, so running from a different working directory does not change the data location.

## Using the implemented features

**Add:** enter Animal ID, Name, Species, Age and Recovery Score, then click Add. Valid IDs are normalized to uppercase. A successful append shows the calculated status/housing and clears the input boxes. Invalid or duplicate input is rejected.

**Update:** search for an exact ID, such as `wr-0002`. Search ignores case and surrounding spaces; partial IDs do not match. The animal loads into the same inputs, its ID becomes read-only, Add is disabled and Save Changes is enabled. Correct the editable fields and save, or click Clear to cancel editing. An unsuccessful search leaves any current edit intact.

Save Changes uses the shared validation and classification rules, updates only the target record and returns to Add mode after success. A failed write keeps the entered values available for retry. The top label provides field errors, file errors and save feedback.

**Delete:** the button currently asks for a grid selection because the grid is not yet present. Once Victor's grid is connected, Dirk's handler will ask for Yes/No confirmation showing the selected ID and name.

## Member 3 requirement check

| Update requirement | State |
| --- | --- |
| Exact ID search, ignoring case | Implemented and tested |
| Loading an animal into the common input boxes | Implemented; LoadForEditing is ready for Victor's row-click handler |
| ID read-only, Add disabled, Save enabled, Clear restores Add mode | Implemented and tested |
| Same validation/classification as Add, with no Add-only uniqueness check | Implemented and tested |
| Status/housing recalculated even when the score is unchanged | Implemented and tested |
| Only the target record rewritten; other records/order preserved | Implemented and tested |
| Refresh actual DataGridView after save | Connection exists; verification depends on Victor's implementation |
| Load an animal by clicking an actual grid row | Pending Victor's grid and event connection |
| Presentation and explanation of the code | Each student must prepare and demonstrate this personally |

Renier's search/save responsibilities are implemented. Full Feature 3 acceptance still requires the two real-grid checks above; the project should not be described as fully complete before those pass.

## Shared code and integration

| File | Purpose |
| --- | --- |
| Animal.cs | Properties, constructor and seven-field ToString format |
| AnimalRules.cs | Shared input validation and the single classification method |
| FileHandler.cs | File loading, Add, Update and Delete methods |
| MainForm.cs | Form state, event handlers, feedback and connection methods |
| MainForm.Designer.cs | Controls and event wiring |
| Program.cs | Starts the application |
| FrameworkChecks/Program.cs | Checks using isolated temporary data |

The application follows the saved lecture examples: ordinary classes, explicit types, named events, loops and a FileHandler. See [coding references](docs/CODING-STYLE.md).

Victor's reserved area is `pnlAnimalsView`. His grid should bind Animal properties in this order: `Id`, `Name`, `Species`, `Age`, `RecoveryScore`, `Status`, `HousingUnit`.

Add, Update and Delete already call `RefreshAnimals()` after successful saves. That method is currently a TODO for Victor; it does not load or bind a grid. His row-click event should call `LoadForEditing(Animal)`. Existing method signatures and other members' code have been preserved.

## Data files

### animals.txt

The file is stored beside the executable. The repository's starter file contains eight fictional records covering all five statuses and is copied to the output folder with **Copy if newer**.

Format: one record per line, UTF-8, seven pipe-separated fields, no header:

```text
WR-0002|Naledi|Serval|3|79|Recovering|Outdoor Enclosure
```

Field order: `ID|Name|Species|Age|Score|Status|Housing Unit`.

- ID: WR- followed by four digits, unique when adding and immutable after saving.
- Name and Species: trimmed, 1–30 characters, no pipe or line break.
- Age and Score: whole numbers from 0 to 100.
- Status and Housing Unit: calculated by the shared method, never typed by the user.

| Score | Status | Housing Unit |
| --- | --- | --- |
| 0–19 | Critical | Intensive Care Unit |
| 20–39 | Serious | High-Dependency Ward |
| 40–59 | Stable | Recovery Ward |
| 60–79 | Recovering | Outdoor Enclosure |
| 80–100 | Release-Ready | Pre-Release Camp |

The shared reader creates an empty missing file and skips malformed lines, returning their count. Search reports this count once per read. The startup grid notification still needs Victor's integration. Update preserves unrelated raw lines, including malformed lines, and writes the replacement to a temporary UTF-8 file before replacing the original.

To inspect saved changes, open the animals.txt beside the built executable, rather than the starter file in the source folder. A newer source starter file may be copied over the output copy during a later build, so keep a separate copy of any data you need to retain.

### summary.txt — still to complete

The summary branch currently displays only total animals and average age. It does not yet meet the full report specification and is not merged into main.

The completed feature must display and write total animals, average age, average recovery score and counts for all five statuses, including zeros. It must overwrite summary.txt beside the executable using the exact layout in brief section 5.2, timestamp `yyyy-MM-dd HH:mm`, and invariant-culture averages with two decimal places. Empty averages must be N/A. Generated summary.txt is excluded from Git.

## Checks and known limitations

The current 64 checks cover shared rules, classification boundaries, input errors, exact search, edit mode, Add/Update persistence, unchanged unrelated records, duplicate/missing targets, read-only files and a Windows file lock. They operate on temporary fixtures and do not change the application's animals.txt.

The tests do **not** establish completion of Victor's grid, Delete's confirmation flow or the unfinished Summary report. Run those end-to-end checks after the responsible members integrate their work.

The application uses a single-user text-file design. Concurrent writers are not supported. Add appends; Update prepares a temporary replacement; Delete currently uses its owner's direct rewrite implementation. Expected file errors are caught and shown as friendly feedback.

Review [Member 3's walkthrough](docs/MEMBER-3.md), [acceptance checks](docs/ACCEPTANCE.md) and [Victor's handoff](docs/VICTOR-DATAGRID.md).

## Git evidence — snapshot before this review commit

Audit baseline: main `a5e465b`, checked 19 October 2026. Only pushed history is visible here. Dates below use South African time and exclude merge-only commits when counting meaningful feature work.

| Member | Non-merge commits visible | Distinct dates visible | Assessment of volume/dates |
| --- | --- | --- | --- |
| Renier | 3 on main | 12, 13, 15 October | At least three commits on three days already present |
| Keanu | 4 on main | 11, 13, 14, 15 October | Volume/dates present; README-only message does not meet the verb-first message rule |
| Dirk | 1 on main, plus 1 on the Summary branch | 18, 19 October | More meaningful commits and a third actual date are still needed |
| Johan Victor | No identifiable pushed commits in fetched refs | Not yet visible | Local/unpushed work cannot be assessed; push actual work under the correct identity |

At this baseline, main has **11 commits including 3 merge commits** (8 non-merge commits). The unmerged Summary branch adds one non-merge commit. The brief requires at least **15 total**, at least three per member, work across at least three actual days and a merged PR per member. This review adds a new commit on 19 October; it does not create or backdate contributions for anyone else.

- Renier's [Update PR #1](https://github.com/KeanuVermeulenBC/PRG281-Project/pull/1) is merged.
- Dirk's [Delete PR #2](https://github.com/KeanuVermeulenBC/PRG281-Project/pull/2) is merged.
- Keanu's Add commits were merged locally with original authorship; no GitHub Add PR was found in the two PRs returned at this audit.
- No View or Summary PR was visible at the audit.

Commit counts alone do not prove all Git requirements: messages must start with a verb, contributions must be meaningful, and feature work must follow the branch/PR rules. The existing initial commit contains only a README rather than the blank-project setup requested by the brief; the history has been preserved.

## Before submission

- Complete and integrate View, row selection and refresh, then verify Add/Update/Delete together.
- Complete and merge the full Summary report and verify the exact output and empty case.
- Confirm Victor's student number.
- Recheck actual commits/dates, meaningful messages, all five feature branches and each member's merged PR.
- Build and run from a fresh clone, confirm starter data beside the executable, and test file errors.
- Make sure main contains the demonstrated code and the lecturer can access the repository.
- Each member should prepare at least three minutes of meaningful presentation contribution and understand the code they present.

The supplied brief sets submission for **20 October 2026 at 23:59 South African time**. Later main commits are not counted under that brief.

This repository includes AI-assisted code and documentation. Review it, understand it and follow the lecturer's rules on permitted assistance and attribution.
