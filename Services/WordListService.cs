using System.Collections.Generic;
using System.IO;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class WordListService
{
    private readonly string _precompiledPath;
    private readonly string _managedPath;
    private readonly CsvWordRepository _repository;
    private List<WordEntry> _words;

    public WordListService(string precompiledPath, string managedPath)
    {
        _precompiledPath = precompiledPath;
        _managedPath = managedPath;
        _repository = new CsvWordRepository();
        _words = new List<WordEntry>();
    }

    public List<WordEntry> LoadAndMerge()
    {
        var precompiled = _repository.Load(_precompiledPath);
        
        if (!File.Exists(_managedPath))
        {
            // No managed list exists - create it from precompiled
            _words = precompiled;
            SaveManaged();
            return _words;
        }

        _words = LoadManagedWords();
        
        // Merge: add words from precompiled that don't exist in managed
        var managedQuestions = _words.Select(w => w.Question).ToHashSet();
        var newWords = precompiled
            .Where(w => !managedQuestions.Contains(w.Question))
            .ToList();
        
        if (newWords.Count > 0)
        {
            _words.AddRange(newWords);
            SaveManaged();
        }

        return _words;
    }

    public void SaveWords()
    {
        SaveManaged();
    }

    private List<WordEntry> LoadManagedWords()
    {
        var lines = File.ReadAllLines(_managedPath);
        var words = new List<WordEntry>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(';');
            if (parts.Length < 2)
                continue;

            var question = parts[0];
            var answer = parts[1];
            var weight = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? int.Parse(parts[2]) : 0;
            var streak = parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]) ? int.Parse(parts[3]) : 0;
            
            // Parse WordGroup if present (backward compatible)
            WordGroup? group = null;
            if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
            {
                group = Enum.Parse<WordGroup>(parts[4]);
            }

            var weightData = new WeightData(weight, streak);
            var wordEntry = new WordEntry(question, answer, weightData, group);
            words.Add(wordEntry);
        }

        return words;
    }

    private void SaveManaged()
    {
        var lines = _words.Select(w => 
            $"{w.Question};{w.Answer};{w.WeightData.Weight};{w.WeightData.CorrectStreak};{w.Group}");
        File.WriteAllLines(_managedPath, lines);
    }
}
