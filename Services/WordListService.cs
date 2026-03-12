using System.Collections.Generic;
using System.IO;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Service responsible for managing vocabulary word lists, including loading, merging, and persisting word data.
/// </summary>
public class WordListService
{
    private readonly string _precompiledPath;
    private readonly string _managedPath;
    private readonly CsvWordRepository _repository;
    private List<WordEntry> _words;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordListService"/> class.
    /// </summary>
    /// <param name="precompiledPath">Path to the precompiled word list CSV file (Data/words.csv).</param>
    /// <param name="managedPath">Path to the managed word list CSV file with progress tracking (appdata.csv).</param>
    public WordListService(string precompiledPath, string managedPath)
    {
        _precompiledPath = precompiledPath;
        _managedPath = managedPath;
        _repository = new CsvWordRepository();
        _words = new List<WordEntry>();
    }

    /// <summary>
    /// Loads and merges the precompiled and managed word lists.
    /// New words from the precompiled list are added to the managed list with default progress values.
    /// Existing words in the managed list retain their progress data (weight, streak, group).
    /// </summary>
    /// <returns>The merged list of <see cref="WordEntry"/> objects.</returns>
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

    /// <summary>
    /// Saves the current word list with updated progress data to the managed CSV file.
    /// </summary>
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

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
                throw new InvalidDataException($"Invalid word list: empty question or answer found in '{_managedPath}'. Please fix or delete the file and restart.");

            int.TryParse(parts.Length > 2 ? parts[2] : null, out var weight);
            int.TryParse(parts.Length > 3 ? parts[3] : null, out var streak);

            WordGroup? group = null;
            if (parts.Length > 4 && Enum.TryParse<WordGroup>(parts[4], out var parsedGroup))
                group = parsedGroup;

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
