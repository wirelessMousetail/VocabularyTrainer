using System;
using System.Collections.Generic;
using System.IO;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class CsvWordRepository
{
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
