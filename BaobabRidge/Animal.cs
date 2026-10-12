namespace BaobabRidge;

// Shared contract: Member 1 owns this model. Agree on property names before changing it.
public sealed record Animal(string Id, string Name, string Species, int Age,
    int RecoveryScore, string Status, string HousingUnit);

public sealed record AnimalInput(string Id, string Name, string Species,
    string Age, string RecoveryScore);

public sealed record ValidationIssue(string Field, string Message);

public sealed record AnimalLoadResult(IReadOnlyList<Animal> Animals, int SkippedLines);
