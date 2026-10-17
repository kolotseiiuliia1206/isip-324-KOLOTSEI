using System;
using System.Collections.Generic;

namespace TextAnalyzer
{
    class Program
    {
        static List<string> history = new List<string>();
        static char[] vowels = { 'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю', 'я', 'a', 'e', 'i', 'o', 'u', 'y' };

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool work = true;

            while (work)
            {
                string text = GetText();
                string stats = Analyze(text);
                history.Add(stats);
                Console.WriteLine(stats);

                Console.WriteLine("\nПродолжить? (да/нет)");
                string ans = Console.ReadLine().ToLower();
                if (ans != "да" && ans != "д") work = false;
                else
                {
                    Console.WriteLine("Показать историю? (да/нет)");
                    if (Console.ReadLine().ToLower() == "да")
                    {
                        Console.WriteLine("\n=== История ===");
                        for (int i = 0; i < history.Count; i++)
                            Console.WriteLine(history[i] + "\n---\n");
                    }
                }
            }
        }

        static string GetText()
        {
            string text;
            do
            {
                Console.WriteLine("Введите текст (мин. 100 символов):");
                text = Console.ReadLine();
                if (text.Length < 100)
                    Console.WriteLine("Слишком коротко! Введено: " + text.Length + "\n");
            } while (text.Length < 100);
            return text;
        }

        static string Analyze(string text)
        {
            string[] words = text.Split(new[] { ' ', '.', ',', '!', '?', ':', ';', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string shortest = words[0], longest = words[0];
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Length < shortest.Length) shortest = words[i];
                if (words[i].Length > longest.Length) longest = words[i];
            }

            int sentences = 0;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '.' || text[i] == '!' || text[i] == '?') sentences++;

            int vowelCount = 0, consonantCount = 0;
            Dictionary<char, int> freq = new Dictionary<char, int>();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsLetter(text[i])) continue;
                char c = char.ToLower(text[i]);

                bool isVowel = false;
                for (int j = 0; j < vowels.Length; j++)
                    if (vowels[j] == c) { isVowel = true; break; }

                if (isVowel) vowelCount++;
                else consonantCount++;

                if (freq.ContainsKey(c)) freq[c]++;
                else freq[c] = 1;
            }

            string result = "Слов: " + words.Length + "\n";
            result += "Предложений: " + sentences + "\n";
            result += "Короткое: " + shortest + "\n";
            result += "Длинное: " + longest + "\n";
            result += "Гласных: " + vowelCount + ", Согласных: " + consonantCount + "\n";
            result += "Частота букв:\n";

            foreach (var pair in freq)
                result += "  " + pair.Key + ": " + pair.Value + "\n";

            return result;
        }
    }
}