# Acceptance checks

Run dotnet run --project FrameworkChecks on Windows. The 113 assertions use a unique temporary folder and do not change the application's animals.txt. Reflection and Windows dialog automation are confined to the test runner; they are not part of the product workflow.

## Automated coverage

- Both edges of every classification band; trimming and ID normalization.
- Invalid ID/text/age/score input, field feedback and focus.
- First/middle/last file updates, preserved record order, reopening and unchanged-score classification.
- Missing/duplicate targets, immutable IDs and invalid replacements.
- Original bytes preserved on read-only/locked-file failures and temporary-file cleanup.
- Missing-file creation, empty grid, malformed records and skipped-line totals.
- Exact grid headers, file order, read-only single-row selection and disabled direct row changes.
- Row click and Search reach the same edit state.
- Save/Add refresh the displayed records without duplicates or automatic editing.
- Clear resets inputs and removes selection; Delete after Clear asks for a selection.
- Locked startup, failed refresh and missing-folder errors remain friendly and preserve available data/inputs.
- Actual Delete No/Yes confirmation, correct ID/name, unchanged data on cancellation, one-record removal and grid/editor refresh.
- Read-only and locked Delete errors preserve the original file.
- Summary titles, aligned layout, minute-only timestamp, two-decimal averages under comma-decimal settings and all five status counts.
- Empty Summary has zero counts and N/A averages; repeated generation overwrites the file.
- Summary handles locked input, locked output and read-only output without crashing or changing the previous saved report.

## Final manual demonstration

1. Start with all eight supplied animals visible and the editor empty.
2. Add an animal and show classification, grid refresh and one validation error.
3. Load WR-0002 by lowercase ID search, then by row click.
4. Change 79 to 80 and save. Show Release-Ready / Pre-Release Camp, refreshed grid and empty Add mode.
5. Restart to prove persistence and compare file lines to show only the target changed.
6. Select an animal and test Delete No, then Yes. Confirm only one record is removed and the grid/editor remain consistent.
7. Generate Summary, then verify every figure on the form and exact summary.txt output, including empty data.
8. Demonstrate a read-only or genuinely locked file error using a copy of the data.
9. Repeat from a fresh clone and prepare the final source ZIP.

An ordinary text editor may not lock the file. The runner uses real Windows file handles to test failure cases. Concurrent external writers are outside the supported single-user design.
