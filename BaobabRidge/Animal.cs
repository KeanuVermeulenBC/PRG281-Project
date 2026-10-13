using System.Globalization;

namespace BaobabRidge
{
    // Stores one animal, using ordinary properties and a constructor.
    public class Animal
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public int Age { get; set; }
        public int RecoveryScore { get; set; }
        public string Status { get; set; }
        public string HousingUnit { get; set; }

        // Creates an animal; classification is supplied by the one shared rules method.
        public Animal(string id, string name, string species, int age, int recoveryScore)
        {
            Id = id;
            Name = name;
            Species = species;
            Age = age;
            RecoveryScore = recoveryScore;
            Status = string.Empty;
            HousingUnit = string.Empty;
        }

        // Formats one record for animals.txt, as in the lecturer's ToString/file-writing example.
        public override string ToString()
        {
            return Id + "|" + Name + "|" + Species + "|"
                + Age.ToString(CultureInfo.InvariantCulture) + "|"
                + RecoveryScore.ToString(CultureInfo.InvariantCulture) + "|"
                + Status + "|" + HousingUnit;
        }
    }
}
