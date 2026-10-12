namespace BaobabRidge;

// Every feature uses this same instance. No form should invent its own data path or cached list.
public interface IAnimalRepository
{
    // Returns a fresh snapshot in file order and the number of skipped malformed lines.
    AnimalLoadResult Load();
    // Appends one unique, validated animal without rewriting existing lines (Member 1).
    void Add(Animal animal);
    // Replaces exactly one existing ID, without changing other records (Member 3).
    void Update(string originalId, Animal replacement);
    // Removes exactly one existing ID after the UI has confirmed the action (Member 4).
    void Delete(string id);
}
