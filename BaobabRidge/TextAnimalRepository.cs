namespace BaobabRidge;

// Production adapter. Deliberately unfinished: this is where the team connects its file work.
public sealed partial class TextAnimalRepository : IAnimalRepository
{
    private readonly string filePath;

    // Receives the single application data path; tests may supply a temporary file instead.
    public TextAnimalRepository(string filePath) => this.filePath = filePath;

    // TODO MEMBER 2: create a missing file, parse valid UTF-8 lines, count skipped lines.
    public AnimalLoadResult Load() => throw new NotImplementedException(
        "Member 2: implement TextAnimalRepository.Load before using file mode.");

    // TODO MEMBER 1: check current IDs for uniqueness, then append a single serialized line.
    public void Add(Animal animal) => throw new NotImplementedException(
        "Member 1: implement append-only Add.");

    // TODO MEMBER 4: re-read the file and safely remove only the confirmed ID.
    public void Delete(string id) => throw new NotImplementedException(
        "Member 4: implement Delete after the confirmation dialog.");
}
