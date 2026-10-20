# Baobab Ridge Wildlife Rehabilitation Records System

PRG2782 Project 2026. A C# Windows Forms application for recording animals in rehabilitation, calculating their recovery classification and maintaining UTF-8 text-file records.

Repository: [KeanuVermeulenBC/PRG282-Project](https://github.com/KeanuVermeulenBC/PRG282-Project)

## Team

| Member | Name | Student number | Responsibility |
| --- | --- | --- | --- |
| 1 | Keanu Vermeulen | 604066 | Model, shared rules and Add |
| 2 | Johan Victor Liebenberg | 604957 | View, DataGridView and loading/error handling |
| 3 | Renier Van Rooyen | 604214 | Update, edit mode and file rewriting |
| 4 | Dirk Geldenhuys | 603556 | Delete, Summary and repository documentation |

## Feature status

| Feature | Current main branch |
| --- | --- |
| Add | Validates inputs, checks unique IDs, appends a record, shows classification and refreshes the grid |
| View | Loads valid records on startup, displays the seven required columns in file order, reports malformed lines and handles missing/locked files |
| Update | Loads by row click or exact case-insensitive ID search, validates edits, recalculates classification, safely replaces the target record and returns to Add mode |
| Delete | Confirms the selected ID/name, removes one record, refreshes the grid and clears the deleted animal from the editor |
| Summary | Displays and saves totals, two-decimal averages and all five status counts using the prescribed report layout; supports empty data |

## Requirements and setup

- Windows.
- .NET 8 SDK or a compatible newer SDK, or Visual Studio with the **.NET desktop development** workload and .NET 8 support.
- No database, storage NuGet package or external service is required.

Clone the repository and open the solution:

```powershell
git clone https://github.com/KeanuVermeulenBC/PRG282-Project.git
cd PRG282-Project
dotnet build BaobabRidge.sln
dotnet run --project BaobabRidge
```

In Visual Studio, open BaobabRidge.sln, set BaobabRidge as the startup project, build and press F5.

## Using the application

**Startup / View:** valid animals appear in a read-only grid in file order. Cells cannot be edited and rows cannot be added/deleted directly. The editor starts empty in Add mode. Malformed lines are skipped and one aggregate count appears in the feedback label.

**Add:** enter Animal ID, Name, Species, Age and Recovery Score, then click Add. A valid new animal is appended to animals.txt. The calculated status/housing are displayed, the grid reloads and the inputs clear. Invalid or duplicate inputs are rejected.

**Update:** click an animal's row or search for its exact ID. Search ignores case and surrounding spaces. The shared inputs enter edit mode: ID is read-only, Add is disabled and Save Changes is enabled. Save validates and recalculates status/housing, changes only that record, reloads the grid and clears the editor. Clear cancels editing without writing. A failed save keeps the entered values available.

**Delete:** click a row and press Delete. The confirmation names the animal's ID and name. No cancels; Yes removes the selected record. The grid reloads and the editor clears if it contained the deleted animal. With no selected animal, the application asks for a selection.

**Generate Summary:** click Generate Summary to calculate results from the current animals.txt. The read-only report box displays the results, and summary.txt is overwritten beside the executable. Scroll the report box to see all five status counts. Empty data produces zero counts and N/A averages.

The top feedback label shows validation errors, skipped-line totals, file errors and successful actions.

## Validation and classification

Inputs are trimmed before validation. IDs are normalized to uppercase.

| Field | Rule |
| --- | --- |
| Animal ID | WR- followed by exactly four digits; unique when adding; immutable when updating |
| Name / Species | 1-30 characters; no pipe character or line break |
| Age / Recovery Score | Whole numbers from 0 to 100 |
| Status / Housing Unit | Calculated, never entered manually |

Both Add and Update use AnimalRules.TryCreate and the same ClassifyAnimal method.

| Recovery score | Status | Housing unit |
| --- | --- | --- |
| 0-19 | Critical | Intensive Care Unit |
| 20-39 | Serious | High-Dependency Ward |
| 40-59 | Stable | Recovery Ward |
| 60-79 | Recovering | Outdoor Enclosure |
| 80-100 | Release-Ready | Pre-Release Camp |

## Data files

### animals.txt

- UTF-8, one animal per line, seven pipe-separated fields, no header.
- Field order: `ID|Name|Species|Age|Score|Status|Housing Unit`.
- Located beside the running executable, using AppContext.BaseDirectory.
- The repository includes eight fictional starter animals covering all five statuses.
- The project copies the starter file to the output directory with **Copy if newer**.

Example:

```text
WR-0002|Naledi|Serval|3|79|Recovering|Outdoor Enclosure
```

A missing file is created empty. Malformed lines are skipped during loading. Update preserves unrelated raw lines, including malformed lines, and writes a unique temporary file beside the original. Once writing succeeds, File.Move overwrites the destination with the completed file. Failed updates preserve the original and clean up the application's temporary file where permissions allow.

The source starter file and the executable's data file are separate copies. Inspect the output copy to see saved edits; keep a backup of important demo data before rebuilding with a newer starter file.

### summary.txt

Generated from the latest valid animals.txt records. Each generation overwrites the UTF-8 report using the layout in brief section 5.2. The timestamp uses local time in yyyy-MM-dd HH:mm format. Both averages use two decimals with a full stop regardless of regional settings; all five statuses are always listed. For empty data, counts are 0 and both averages are N/A.

Example for the supplied starter data (timestamp is illustrative):

```text
BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE
ANIMAL SUMMARY REPORT
Generated: 2026-10-20 21:45
----------------------------------------
Total animals:            8
Average age (years):      4.25
Average recovery score:   56.50
----------------------------------------
Animals per status:
  Critical:               1
  Serious:                2
  Stable:                 1
  Recovering:             2
  Release-Ready:          2
```

Generated summary.txt is excluded from Git; it is created when Generate Summary is clicked.

## Code organisation

| File | Purpose |
| --- | --- |
| Animal.cs | Animal properties, grid display names and pipe-separated record formatting |
| AnimalRules.cs | Shared validation and classification |
| FileHandler.cs | File loading, Add, Update, Delete and Summary saving |
| MainForm.cs | Event handlers, editor state, grid configuration/refresh and friendly feedback |
| MainForm.Designer.cs | Form layout and named controls |
| Program.cs | Application entry point |
| FrameworkChecks/Program.cs | Automated checks using isolated temporary files |

The code uses ordinary classes, explicit types, named events and loops, following the course's FileHandler and collections examples. Victor's grid and display-name attributes are retained. Grid CellClick opens the editor; programmatic binding does not. The shared RefreshAnimals method updates the same grid after every completed write.

## Verification

Run the checks on Windows:

```powershell
dotnet run --project FrameworkChecks
```

The current suite has 113 assertions covering classification boundaries, input errors, Add/Update persistence, other-record preservation, duplicate/missing IDs, read-only/locked files, Update temporary-file cleanup, exact grid headers, selection restrictions, row-click editing, refresh without duplicates, returning to Add mode, malformed-line notices and startup/refresh error handling. It also exercises the actual Delete Yes/No dialog and verifies Summary layout, averages under comma-decimal regional settings, all five status counts, empty data, overwriting and input/output file errors.

Checks use their own temporary fixtures and do not change the application's animals.txt. The latest Windows build passed with zero warnings and zero errors. All five features are integrated on main. The team must still rehearse the presentation using its final demo data.

See [acceptance checks](docs/ACCEPTANCE.md), [Update walkthrough](docs/MEMBER-3.md), [View implementation notes](docs/VICTOR-DATAGRID.md) and [integration notes](docs/INTEGRATION.md).

## Known limitations and submission

This is a single-user text-file application; concurrent writers are not supported. File errors receive friendly feedback. Update writes through a temporary file; Add appends and Delete retains its direct rewrite implementation. Keep a backup of important data. Restricted folder permissions may prevent cleanup of a temporary file after a failed update.

The history includes merged Update PR #1, Delete PR #2 and Summary PR #3. The repository exceeds 15 commits. Each member must independently check the required three meaningful commits on three actual days and a merged PR: Victor's visible work covers 19/20 October, and no Add/View PR was visible during the 20 October review. Dates and authors have not been rewritten.

Before submission, rehearse all five features and one file error, prepare the 20-minute presentation with at least three meaningful minutes per member, and submit the repository URL and final source ZIP through BCConnect. All four members' names and student numbers appear above.

This repository includes AI-assisted code and documentation. Team members must understand the adopted work and follow the lecturer's rules on permitted assistance and attribution.
