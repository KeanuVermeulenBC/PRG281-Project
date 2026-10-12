namespace BaobabRidge;

// Temporary teaching adapter: nothing in this class reads or writes animals.txt.
public sealed class PracticeAnimalRepository : IAnimalRepository
{
    private readonly List<Animal> animals = new()
    {
        new("WR-0001", "Thabo", "African Penguin", 4, 85, "Release-Ready", "Pre-Release Camp"),
        new("WR-0002", "Naledi", "Serval", 3, 79, "Recovering", "Outdoor Enclosure"),
        new("WR-0004", "Zola", "Cape Vulture", 12, 19, "Critical", "Intensive Care Unit")
    };

    // Gives the form a fresh collection of immutable records for practice.
    public AnimalLoadResult Load() => new(animals.ToArray(), 0);

    // Leaves Add for its feature owner instead of presenting a fake completed feature.
    public void Add(Animal animal) => throw new NotImplementedException("Member 1 owns Add.");

    // Demonstrates replacement in memory only; restarting restores the three practice records.
    public void Update(string originalId, Animal replacement)
    {
        if (!string.Equals(originalId, replacement.Id, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("An animal's ID cannot be changed.");
        int index = animals.FindIndex(a => string.Equals(a.Id, originalId, StringComparison.OrdinalIgnoreCase));
        if (index < 0) throw new InvalidOperationException("This animal no longer exists. Reload the records.");
        animals[index] = replacement;
    }

    // Leaves Delete for its feature owner.
    public void Delete(string id) => throw new NotImplementedException("Member 4 owns Delete.");
}
