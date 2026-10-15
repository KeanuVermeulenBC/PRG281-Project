# Update acceptance checks

Run `dotnet run --project FrameworkChecks` on Windows. It uses a unique temporary folder and does not change the application's animals.txt. No test packages are required. DataGridView checks are pending Victor's implementation.

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
- Add appends one record without altering existing records, returns to Add mode and rejects duplicate IDs.
- Real form startup, exact search, validation focus, failed-save state, successful save and Clear without a grid dependency.

## Manual team demo checks

1. Add a new animal and verify its record is appended to animals.txt.
2. Load WR-0002 by lowercase ID search. After Victor adds the grid, verify row clicks reach the same editor.
3. Search a partial or missing ID while editing. Your current edit should remain.
4. Change 79 to 80 and save. Confirm Release-Ready / Pre-Release Camp.
5. Compare animals.txt before and after: only the target record changes.
6. Restart and confirm the saved value remains.
7. Try invalid inputs and verify both the field-specific feedback and cursor position.
8. Use a copy of your data to test an empty file, missing folder and file-in-use error.
9. After Victor adds the grid, verify seven columns, startup loading, read-only whole-row selection and refresh after Add/Update.
10. Repeat with the other members' features after integration.

Opening a file in a text editor may not lock it. The automated runner uses an actual Windows file handle to test replacement failure. Concurrent file writers are outside the scope of this application.
