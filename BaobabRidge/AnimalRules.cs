using System;
using System.Globalization;

namespace BaobabRidge
{
    // Member 1 and Member 3 share these rules; Add also needs a separate uniqueness check.
    public static class AnimalRules
    {
        // Checks the five input strings and creates a classified animal when they are valid.
        public static bool TryCreate(string id, string name, string species, string ageText,
            string scoreText, out Animal animal, out string invalidField, out string errorMessage)
        {
            animal = null;
            invalidField = string.Empty;
            errorMessage = string.Empty;
            id = id.Trim().ToUpperInvariant();
            name = name.Trim();
            species = species.Trim();
            int age;
            int score;

            if (!ValidId(id))
            {
                invalidField = "Id";
                errorMessage = "Animal ID must be WR- followed by exactly four digits.";
            }
            else if (!ValidText(name))
            {
                invalidField = "Name";
                errorMessage = "Name must contain 1-30 characters and no pipe or line break.";
            }
            else if (!ValidText(species))
            {
                invalidField = "Species";
                errorMessage = "Species must contain 1-30 characters and no pipe or line break.";
            }
            else if (!TryNumber(ageText, out age))
            {
                invalidField = "Age";
                errorMessage = "Age must be a whole number between 0 and 100.";
            }
            else if (!TryNumber(scoreText, out score))
            {
                invalidField = "RecoveryScore";
                errorMessage = "Recovery Score must be a whole number between 0 and 100.";
            }
            else
            {
                animal = new Animal(id, name, species, age, score);
                ClassifyAnimal(animal);
                return true;
            }
            return false;
        }

        // Assigns the exact status and housing for the score; Add and Update use this same method.
        public static void ClassifyAnimal(Animal animal)
        {
            int score = animal.RecoveryScore;
            if (score < 0 || score > 100)
            {
                throw new ArgumentOutOfRangeException("RecoveryScore");
            }

            if (score <= 19)
            {
                animal.Status = "Critical";
                animal.HousingUnit = "Intensive Care Unit";
            }
            else if (score <= 39)
            {
                animal.Status = "Serious";
                animal.HousingUnit = "High-Dependency Ward";
            }
            else if (score <= 59)
            {
                animal.Status = "Stable";
                animal.HousingUnit = "Recovery Ward";
            }
            else if (score <= 79)
            {
                animal.Status = "Recovering";
                animal.HousingUnit = "Outdoor Enclosure";
            }
            else
            {
                animal.Status = "Release-Ready";
                animal.HousingUnit = "Pre-Release Camp";
            }
        }

        // Checks the prefix and each of the four digits without a regular expression.
        private static bool ValidId(string id)
        {
            if (id.Length != 7 || !id.StartsWith("WR-", StringComparison.Ordinal))
            {
                return false;
            }
            for (int i = 3; i < id.Length; i++)
            {
                if (id[i] < '0' || id[i] > '9')
                {
                    return false;
                }
            }
            return true;
        }

        // Keeps text within the required length and on a single pipe-separated record.
        private static bool ValidText(string text)
        {
            return text.Length >= 1 && text.Length <= 30
                && !text.Contains("|") && !text.Contains("\r") && !text.Contains("\n");
        }

        // Accepts digits only, then checks the required numeric range.
        private static bool TryNumber(string text, out int number)
        {
            text = text.Trim();
            number = 0;
            if (text == string.Empty)
            {
                return false;
            }
            foreach (char digit in text)
            {
                if (digit < '0' || digit > '9')
                {
                    return false;
                }
            }
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out number)
                && number >= 0 && number <= 100;
        }
    }
}
