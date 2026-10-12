using System.Globalization;
using System.Text.RegularExpressions;

namespace BaobabRidge;

// Provisional shared implementation for Member 1 to review and own.
public static class AnimalRules
{
    // Maps the score to the exact status and housing strings required by the brief.
    public static (string Status, string HousingUnit) Classify(int score)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score));
        if (score <= 19) return ("Critical", "Intensive Care Unit");
        if (score <= 39) return ("Serious", "High-Dependency Ward");
        if (score <= 59) return ("Stable", "Recovery Ward");
        if (score <= 79) return ("Recovering", "Outdoor Enclosure");
        return ("Release-Ready", "Pre-Release Camp");
    }

    // Validates common Add/Update fields and always recalculates classification.
    // Member 1 must perform the Add-only uniqueness check against the latest file separately.
    public static bool TryCreate(AnimalInput input, out Animal? animal, out ValidationIssue? issue)
    {
        animal = null;
        issue = null;
        string id = input.Id.Trim().ToUpperInvariant();
        string name = input.Name.Trim();
        string species = input.Species.Trim();
        if (!Regex.IsMatch(id, @"\AWR-[0-9]{4}\z"))
            issue = new("Id", "Animal ID must be WR- followed by exactly four digits.");
        else if (!ValidText(name))
            issue = new("Name", "Name must contain 1-30 characters and no pipe or line break.");
        else if (!ValidText(species))
            issue = new("Species", "Species must contain 1-30 characters and no pipe or line break.");
        else if (!TryNumber(input.Age, out _))
            issue = new("Age", "Age must be a whole number between 0 and 100.");
        else if (!TryNumber(input.RecoveryScore, out _))
            issue = new("RecoveryScore", "Recovery Score must be a whole number between 0 and 100.");
        if (issue != null) return false;

        TryNumber(input.Age, out int age);
        TryNumber(input.RecoveryScore, out int score);
        var classification = Classify(score);
        animal = new(id, name, species, age, score, classification.Status, classification.HousingUnit);
        return true;
    }

    // Keeps a text field within the one-record-per-line storage contract.
    private static bool ValidText(string value) => value.Length is >= 1 and <= 30
        && value.IndexOfAny(new[] { '|', '\r', '\n' }) < 0;

    // Accepts trimmed ASCII digits only, with no decimals, signs or culture-dependent separators.
    private static bool TryNumber(string value, out int number)
    {
        string trimmed = value.Trim();
        number = 0;
        return Regex.IsMatch(trimmed, @"\A[0-9]+\z")
            && int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out number)
            && number <= 100;
    }
}
