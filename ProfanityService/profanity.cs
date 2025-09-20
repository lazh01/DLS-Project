using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProfanityService
{
    public class Profanity
    {
        private readonly Database _database;

        public Profanity(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Checks if the text contains any profanities from the database.
        /// </summary>
        /// <param name="text">The text request containing text.</param>
        /// <returns>True if any profanities are found, false otherwise.</returns>
        public async Task<bool> ContainsProfanityAsync(ProfanityRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return false;

            // Get all words from the database
            List<string> profanities = await Database.GetAllWordsAsync();

            if (profanities.Count == 0)
                return false;

            // Normalize the text to lowercase for case-insensitive matching
            string normalizedText = request.Text.ToLower();

            // Optional: split text into words (better than simple substring)
            string[] wordsInText = Regex.Matches(normalizedText, @"\w+")
                                        .Select(m => m.Value)
                                        .ToArray();

            // Check if any word from the database exists in the text
            foreach (var badWord in profanities)
            {
                if (Array.Exists(wordsInText, w => w.Equals(badWord.ToLower(), StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;
        }
    }
}