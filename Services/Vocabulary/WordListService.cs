using System.Collections.Generic;
using System.IO;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Vocabulary;

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
            // No managed list exists - create it from precompiled with initial weights
            _words = precompiled.Select(ApplyInitialWeight).ToList();
            SaveManaged();
            return _words;
        }

        _words = LoadManagedWords();

        bool changed = UpdateChangedAnswers(precompiled);
        changed |= AddNewWords(precompiled);

        if (changed)
            SaveManaged();

        return _words;
    }

    private bool UpdateChangedAnswers(List<WordEntry> precompiled)
    {
        var precompiledByQuestion = precompiled.ToDictionary(w => w.Question, w => w);
        bool changed = false;
        for (int i = 0; i < _words.Count; i++)
        {
            var managed = _words[i];
            if (precompiledByQuestion.TryGetValue(managed.Question, out var source) &&
                !string.Equals(managed.Answer, source.Answer, StringComparison.Ordinal))
            {
                _words[i] = new WordEntry(managed.Question, source.Answer, managed.WeightData, managed.Group);
                changed = true;
            }
        }
        return changed;
    }

    private bool AddNewWords(List<WordEntry> precompiled)
    {
        var existingQuestions = _words.Select(w => w.Question).ToHashSet();
        var newWords = precompiled
            .Where(w => !existingQuestions.Contains(w.Question))
            .Select(ApplyInitialWeight)
            .ToList();

        if (newWords.Count == 0)
            return false;

        _words.AddRange(newWords);
        return true;
    }

    private static WordEntry ApplyInitialWeight(WordEntry word)
    {
        var weightData = new WeightData(
            WordWeightStrategy.MaxWeight / 2,
            WordWeightStrategy.LinearStreakThreshold);
        return new WordEntry(word.Question, word.Answer, weightData, word.Group);
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

            var question = parts[0].Trim();
            var answer = parts[1].Trim();

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
