# Baobab Ridge Wildlife Rehabilitation Records System

**PRG2782 Project 2026**

A C# Windows Forms application for managing animal records at Baobab Ridge Wildlife Rehabilitation Centre. Staff can add, view, update and delete animal records, and generate a summary report. Each animal's recovery score determines its status and housing unit.

[GitHub repository](https://github.com/KeanuVermeulenBC/PRG282-Project)

## Team members

| Name | Student number | Main responsibility |
| --- | --- | --- |
| Keanu Vermeulen | 604066 | Animal model, shared classification rules and Add |
| Johan Victor Liebenberg | 604957 | View, DataGridView, file loading and error handling |
| Renier Van Rooyen | 604214 | Update, edit mode and saving changes to the file |
| Dirk Geldenhuys | 603556 | Delete, Summary report and repository documentation |

## Features

- **Add:** saves a new animal after checking its details and unique ID.
- **View:** loads animals into a read-only grid when the application starts. The grid refreshes after adding, updating or deleting a record.
- **Update:** loads an animal by clicking its row or searching for its exact ID. Changes are validated and its classification is recalculated before saving.
- **Delete:** asks for Yes/No confirmation showing the selected animal's ID and name before removing it.
- **Generate Summary:** displays and saves the total animals, average age, average recovery score and counts for all five statuses.

Missing animals.txt files are created automatically. Invalid file lines are skipped with a message showing how many were skipped. File access errors are shown as friendly messages.

## Build and run

You need Windows and the .NET 8 SDK. For Visual Studio, install the **.NET desktop development** workload with .NET 8 support.

### Visual Studio

1. Clone or download this repository.
2. Open `BaobabRidge.sln`.
3. Set `BaobabRidge` as the startup project.
4. Build the solution and press **F5**.

### Command line

```powershell
git clone https://github.com/KeanuVermeulenBC/PRG282-Project.git
cd PRG282-Project
dotnet build BaobabRidge.sln
dotnet run --project BaobabRidge
```

No database or external service is needed.

## How to use it

1. **Add an animal:** enter its ID, name, species, age and recovery score, then click **Add**.
2. **Edit an animal:** click its grid row or enter its ID and click **Search**. Search ignores upper/lower case. Edit the details and click **Save Changes**. The ID cannot be changed.
3. **Cancel editing:** click **Clear** to empty the inputs and return to Add mode without saving.
4. **Delete an animal:** select its grid row, click **Delete**, then choose **Yes** to remove it or **No** to cancel.
5. **Create a report:** click **Generate Summary**. The report appears below the buttons and is saved to `summary.txt`. Scroll the report box to see all results.

A successful Add or Update clears the inputs. The message at the top shows feedback and any errors.

## Validation and classification

| Field | Rule |
| --- | --- |
| Animal ID | `WR-` followed by four digits, for example `WR-0001`; must be unique when adding |
| Name and species | 1-30 characters; no pipe character (`\|`) or line break |
| Age and recovery score | Whole numbers from 0 to 100 |
| Status and housing unit | Calculated automatically from the recovery score |

Add and Update use the same validation and classification methods.

| Recovery score | Status | Housing unit |
| --- | --- | --- |
| 0-19 | Critical | Intensive Care Unit |
| 20-39 | Serious | High-Dependency Ward |
| 40-59 | Stable | Recovery Ward |
| 60-79 | Recovering | Outdoor Enclosure |
| 80-100 | Release-Ready | Pre-Release Camp |

## Data files

Both files are stored beside the running executable.

### animals.txt

Stores one animal per line in UTF-8, with seven fields separated by `|` and no header. The field order is `ID|Name|Species|Age|Score|Status|Housing Unit`.

Example record:

```text
WR-0002|Naledi|Serval|3|79|Recovering|Outdoor Enclosure
```

The repository includes **eight starter animals covering all five statuses**. The project uses **Copy if newer** to copy the starter file to the build output folder.

Add appends a new record. Update changes only the matching record, and Delete removes only the selected record.

### summary.txt

Created or overwritten whenever **Generate Summary** is clicked. The UTF-8 report includes the centre name, report title, timestamp, total animals, both averages and all five status counts.

The timestamp uses `yyyy-MM-dd HH:mm`. Averages use two decimal places with a full stop. Statuses with no animals show `0`. If there are no animals, the total and all status counts are `0`, and both averages are `N/A`.

This generated file is excluded from Git.

## Project files

| File | Purpose |
| --- | --- |
| `Animal.cs` | Stores an animal's details |
| `AnimalRules.cs` | Validates inputs and calculates classification |
| `FileHandler.cs` | Reads and writes animal records and saves reports |
| `MainForm.cs` | Handles buttons, the grid and form behaviour |
| `MainForm.Designer.cs` | Defines the form layout and controls |
| `FrameworkChecks/Program.cs` | Checks the application using temporary test files |

Application source files are in the `BaobabRidge` folder.

## Testing

Run the automated checks on Windows:

```powershell
dotnet run --project FrameworkChecks
```

The verified version passes **113 checks**, including validation, classification boundaries, saved changes, grid refresh, Delete confirmation, report formatting, empty data and file errors. The checks use temporary files and do not change the application's animal records. A fresh GitHub clone was also built successfully with zero warnings and zero errors.

## Known limitations

- The application is intended for one user at a time; simultaneous file changes by multiple users are not supported.
- The source starter file and the running application's data file are separate copies. Saved changes appear in the output folder. A newer starter file can replace that copy during a build, so back up important records.
