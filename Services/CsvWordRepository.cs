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
            if (parts.Length != 2)
                continue;

            result.Add(new WordEntry(parts[0], parts[1]));
        }

        return result;
    }
}
