# Update acceptance checks

Run `dotnet run --project FrameworkChecks` on Windows. It uses a unique temporary folder and does not change the application's animals.txt. No test packages are required. Reflection is used only in the check runner to exercise the private grid-click event; it is not part of the application workflow.

## Automated coverage

- Both edges of all five classification bands.
- Trimming, uppercase IDs, invalid ID format, blank/long/pipe text and invalid numbers.
- Missing file creation and malformed-line counting.
- First, middle and last record updates, preserving count, order and unrelated lines.
- Reopening through a new FileHandler confirms persistence.
- Reclassification even when the score is unchanged.
- Preservation of an unrelated edit made after the grid snapshot.
- Rejection of missing/duplicate IDs, changed IDs and invalid replacement fields.
- Read-only protection and a Windows file handle that allows reading but denies replacement.
- Original bytes preserved after failed writes and temporary-file cleanup.
- A file removed during editing is not silently recreated by Save.
- Real form startup, grid headers, exact search, grid click, validation focus, failed-save state, successful save/refresh and Clear.

## Manual team demo checks

1. Start with the eight supplied animals and confirm the seven grid columns.
2. Load WR-0002 by row click, then by lowercase ID search. Both should use the same inputs.
3. Search a partial or missing ID while editing. Your current edit should remain.
4. Change 79 to 80 and save. Confirm Release-Ready / Pre-Release Camp.
5. Compare animals.txt before and after: only the target record changes.
6. Restart and confirm the saved value remains.
7. Try invalid inputs and verify both the field-specific feedback and cursor position.
8. Use a copy of your data to test an empty file, missing folder and file-in-use error.
9. Repeat with the other members' features after integration.

Opening a file in a text editor may not lock it. The automated runner uses an actual Windows file handle to test replacement failure. Concurrent file writers are outside the scope of this application.
