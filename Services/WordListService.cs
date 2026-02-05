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

    public WordListService(string precompiledPath, string managedPath)
    {
        _precompiledPath = precompiledPath;
        _managedPath = managedPath;
        _repository = new CsvWordRepository();
    }

    public List<WordEntry> LoadAndMerge()
    {
        var precompiled = _repository.Load(_precompiledPath);
        
        if (!File.Exists(_managedPath))
        {
            // No managed list exists - create it from precompiled
            SaveManaged(precompiled);
            return precompiled;
        }

        var managed = _repository.Load(_managedPath);
        
        // Merge: add words from precompiled that don't exist in managed
        var managedQuestions = managed.Select(w => w.Question).ToHashSet();
        var newWords = precompiled.Where(w => !managedQuestions.Contains(w.Question)).ToList();
        
        if (newWords.Count > 0)
        {
            managed.AddRange(newWords);
            SaveManaged(managed);
        }

        return managed;
    }

    private void SaveManaged(List<WordEntry> words)
    {
        var lines = words.Select(w => $"{w.Question};{w.Answer}");
        File.WriteAllLines(_managedPath, lines);
    }
}
