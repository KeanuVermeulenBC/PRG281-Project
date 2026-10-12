# Acceptance checks

`dotnet run --project FrameworkChecks` checks shared rules, the practice adapter and form state. It does **not** certify the unfinished production adapter. Record actual file-mode results when implementation is complete.

## Practice walkthrough

- Start: practice warning and three records, no auto-selected row.
- Click Naledi: ID read-only, Add disabled, Save enabled.
- Clear: empty boxes, ID editable, Add enabled, Save disabled.
- Search ` wr-0002 `: exact case-insensitive match; partial IDs do not match.
- Search missing ID during editing: the current edit must remain unchanged.
- Change 79 to 80: correct classification, one row changed, Add mode restored.
- Try decimal age, text score, blank name or pipe in species: no save, helpful field message/focus.
- Restart: practice changes disappear. This is expected, not a persistence pass.

## File-mode tests required after implementation

| Scenario | Expected result |
| --- | --- |
| Update first, middle, last record | Only target changes; same count/order; persists after restart |
| Cross 19/20, 39/40, 59/60, 79/80 both ways | Exact classification strings |
| Target has wrong stored status, score unchanged | Save recalculates target status/housing |
| Unrelated record has different stored status | It remains untouched |
| Invalid text/numbers including 101 and negative values | No file changes; field message/focus |
| Nothing selected or empty data | No unhandled error |
| Target removed since loading | No duplicate or new record; useful failure |
| Duplicate target ID | Reject ambiguous update without writing |
| Other record changed since grid load | Latest unrelated change survives |
| Read-only file or exclusive file lock | Friendly error; original bytes unchanged; edit retained |
| Missing folder or failed replacement write | No truncated original or false success |
| Mixed malformed/valid rows | One skip count on grid load; valid records survive |
| Non-ASCII names in assessor UTF-8 file | Text survives load/save |
| Different working directory | Uses file beside executable |
| Fresh clone build/run | No missing files or machine-specific paths |

Use temporary test files and compare before/after lines; for failed writes compare bytes. Opening a file in a text editor may not lock it: test with an exclusive file handle. Do not experiment on the team's only data copy.
