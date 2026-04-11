using System;
using System.Collections.Generic;
using System.IO;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Vocabulary;

/// <summary>
/// Loads vocabulary words from a semicolon-delimited CSV file.
/// Expected format per line: <c>question;answer</c> (no header row).
/// </summary>
public class CsvWordRepository
{
    /// <summary>
    /// Reads all words from the CSV file at <paramref name="path"/>.
    /// Blank lines are skipped. Word group is auto-detected via <see cref="WordGrouping.Detect"/>.
    /// </summary>
    /// <param name="path">Path to the CSV file.</param>
    /// <returns>A list of <see cref="WordEntry"/> objects with default weight and streak values.</returns>
    /// <exception cref="FormatException">Thrown when a line does not contain exactly two non-empty semicolon-separated fields.</exception>
    public List<WordEntry> Load(string path)
    {
        var result = new List<WordEntry>();

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(';');
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                throw new FormatException($"Invalid line in CSV: '{line}'");

            result.Add(new WordEntry(parts[0].Trim(), parts[1].Trim()));
        }

        return result;
    }
}
