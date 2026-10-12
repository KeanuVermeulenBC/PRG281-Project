namespace BaobabRidge;

public sealed partial class TextAnimalRepository
{
    // TODO MEMBER 3: implement durable replacement; the UI already calls this method.
    public void Update(string originalId, Animal replacement)
    {
        // 1. Reject a replacement whose ID differs from originalId (case-insensitive).
        // 2. Read the latest file, NOT the possibly stale grid snapshot.
        // 3. Find exactly one matching ID. If missing or duplicated, fail without writing.
        // 4. Replace only that record, preserving the order/content of all other records.
        // 5. Write UTF-8 to a unique temporary file in the SAME directory as filePath.
        // 6. Close the writer, then replace the original only after the write succeeds.
        // 7. Clean up the temporary file on failure without masking the original error.
        // Never use append for Update, or truncate animals.txt before preparing its replacement.
        // File exceptions go to MainForm's friendly error handler; never swallow a failed write.
        throw new NotImplementedException(
            "Member 3: implement the safe file rewrite in TextAnimalRepository.Update.cs. " +
            "No file has been changed.");
    }
}
