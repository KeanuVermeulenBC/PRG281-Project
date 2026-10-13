using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace BaobabRidge
{
    // Keeps text-file work separate from the form, following the FileHandler lecture.
    public class FileHandler
    {
        private string filePath;

        // Uses animals.txt beside the running application.
        public FileHandler()
        {
            filePath = Path.Combine(AppContext.BaseDirectory, "animals.txt");
        }

        // Allows checks to use a temporary file without changing the application's data.
        public FileHandler(string filePath)
        {
            this.filePath = filePath;
        }

        // Reads valid animals in file order and counts malformed lines for one UI notification.
        public List<Animal> ReadAnimals(out int skippedLines)
        {
            List<Animal> animals = new List<Animal>();
            skippedLines = 0;

            if (!File.Exists(filePath))
            {
                // Create an empty missing file, but do not hide missing-folder or permission errors.
                File.WriteAllText(filePath, string.Empty, Encoding.UTF8);
            }

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (string line in lines)
            {
                Animal animal;
                if (TryReadAnimal(line, out animal))
                {
                    animals.Add(animal);
                }
                else
                {
                    skippedLines++;
                }
            }
            return animals;
        }

        // Replaces one animal in the latest file while keeping every other line unchanged.
        public void UpdateAnimal(string originalId, Animal replacement)
        {
            if (!string.Equals(originalId, replacement.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("An animal's ID cannot be changed.");
            }

            Animal validatedAnimal;
            string invalidField;
            string errorMessage;
            if (!AnimalRules.TryCreate(replacement.Id, replacement.Name, replacement.Species,
                replacement.Age.ToString(CultureInfo.InvariantCulture),
                replacement.RecoveryScore.ToString(CultureInfo.InvariantCulture),
                out validatedAnimal, out invalidField, out errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            // Read the file again instead of rewriting a possibly outdated grid list.
            // Unlike ReadAnimals, this must fail if the file disappeared during editing.
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            int matchingIndex = -1;
            int matchCount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split('|');
                if (string.Equals(fields[0].Trim(), originalId, StringComparison.OrdinalIgnoreCase))
                {
                    matchingIndex = i;
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                throw new InvalidOperationException("This animal is no longer in the file. Reload before editing again.");
            }
            if (matchCount > 1)
            {
                throw new InvalidOperationException("The file contains this Animal ID more than once. Correct the duplicate IDs before saving.");
            }

            Animal existingAnimal;
            if (!TryReadAnimal(lines[matchingIndex], out existingAnimal))
            {
                throw new InvalidOperationException("This animal's file record is no longer valid. Reload before editing again.");
            }

            if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) != 0)
            {
                throw new UnauthorizedAccessException("The animals file is read-only.");
            }

            lines[matchingIndex] = validatedAnimal.ToString();
            string temporaryPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                // WriteAllLines closes the temporary file before replacing the original.
                // This extra step protects the original if writing fails partway through.
                File.WriteAllLines(temporaryPath, lines, new UTF8Encoding(false));
                File.Replace(temporaryPath, filePath, null);
            }
            finally
            {
                DeleteTemporaryFile(temporaryPath);
            }
        }

        // Returns true if the latest file already contains this ID (ignoring upper/lower case).
        public bool IDExists(string id)
        {
            if (!File.Exists(filePath))
            {
                // Create an empty missing file, but do not hide missing-folder or permission errors.
                return false;
            }
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] fields = line.Split('|');
                if (string.Equals(fields[0].Trim(), id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // Appends one new animal to the end of animals.txt without touching existing lines.
        public void AddAnimal(Animal newAnimal)
        {
            if (IDExists(newAnimal.Id))
            {
                throw new InvalidOperationException("Animal ID " + newAnimal.Id + " already exists.");
            }

            string text = newAnimal.ToString() + Environment.NewLine;

            if (File.Exists(filePath))
            {
                 string existingText = File.ReadAllText(filePath, Encoding.UTF8);

                if (existingText.Length > 0 && !existingText.EndsWith("\n"))
                {
                    text = Environment.NewLine + text;
                }

            }
            File.AppendAllText(filePath, text, new UTF8Encoding(false));
        }

        // Converts a seven-field line into an Animal without changing its stored classification.
        private bool TryReadAnimal(string line, out Animal animal)
        {
            animal = null;
            string[] fields = line.Split('|');
            if (fields.Length != 7 || fields[5].Trim() == string.Empty || fields[6].Trim() == string.Empty)
            {
                return false;
            }

            string invalidField;
            string errorMessage;
            if (!AnimalRules.TryCreate(fields[0], fields[1], fields[2], fields[3], fields[4],
                out animal, out invalidField, out errorMessage))
            {
                return false;
            }

            // Only a saved edit should correct classification; other file records must survive unchanged.
            animal.Status = fields[5];
            animal.HousingUnit = fields[6];
            return true;
        }

        // Removes a leftover temporary file without hiding the original save error.
        private void DeleteTemporaryFile(string temporaryPath)
        {
            try
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            catch (IOException)
            {
                // A locked temporary file can be removed later; the original exception still reaches the form.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup permissions must not replace the useful error from the failed save.
            }
        }
    }
}
