using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Anagram
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Début du programme");
            Console.WriteLine("Chargement de la liste des mots existants");

            ILookup<int, char[]> wordsLookup = GetWordsLookup();

            while (true)
            {
                Console.WriteLine("Saisissez un ou plusieurs mots et appuyez sur entrée");

                string input = Console.ReadLine();
                string cleanInput = input.RemovePunctuation().ToLower();

                IReadOnlyDictionary<char, LetterOccurences> data = GetInputData(cleanInput);

                IEnumerable<char[]> scannedWords = wordsLookup[cleanInput.Length];

                ICollection<string> results = new List<string>();
                foreach (char[] chars in scannedWords)
                {
                    if (ScanWord(data, chars))
                        results.Add(new string(chars));

                    foreach (var kvp in data)
                        kvp.Value.Reset();
                }

                Console.WriteLine();

                if (results.Any())
                {
                    Console.WriteLine("Liste des anagrammes:");
                    Console.WriteLine();

                    foreach (string result in results)
                        Console.WriteLine($"    {result}");

                }
                else
                    Console.WriteLine("Oups, aucun résultat trouvé :'(");

                Console.WriteLine(); 
            }
        }

        private static bool ScanWord(IReadOnlyDictionary<char, LetterOccurences> data, char[] chars)
        {
            foreach (char c in chars)
            {
                if (data.TryGetValue(c, out LetterOccurences occurence) && occurence.RemoveIfPossible())
                    continue;
                else
                    return false;
            }

            return true;
        }        

        private static ILookup<int, char[]> GetWordsLookup()
        {
            string[] allWords = File.ReadAllLines("words.txt");
            ILookup<int, char[]> wordsLookup = allWords
                .Select(str => str.RemoveDiacritics().ToLower())
                .ToLookup(
                    str => str.Length,
                    str => str.ToCharArray()
                );

            return wordsLookup;
        }

        private static string RemoveDiacritics(this string input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));

            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string result = sb.ToString().Normalize(NormalizationForm.FormC);
            return result;
        }

        private static string RemovePunctuation(this string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach(char c in input)
            {
                if (char.IsLetter(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private static IReadOnlyDictionary<char, LetterOccurences> GetInputData(string input)
        {
            IReadOnlyDictionary<char, LetterOccurences> wordData = input
                .ToCharArray()
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => new LetterOccurences(g.Count()));
            return wordData;
        }

        private class LetterOccurences
        {
            public LetterOccurences(int total)
            {
                this.total = total;
                current = total;
            }

            public bool RemoveIfPossible()
            {
                if (current > 0)
                {
                    current--;
                    return true;
                }
                else
                    return false;
            }

            public void Reset() => current = total;

            private readonly int total;
            private int current;
        }
    }
}