using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Vocabulary;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Vocabulary;

public class WordListServiceTests : IDisposable
{
    private readonly string _precompiledPath = Path.GetTempFileName();
    private readonly string _managedPath = Path.GetTempFileName();

    public void Dispose()
    {
        File.Delete(_precompiledPath);
        if (File.Exists(_managedPath)) File.Delete(_managedPath);
    }

    private WordListService Build() => new(_precompiledPath, _managedPath);

    // ── No managed file ───────────────────────────────────────────────────────

    [Fact]
    public void LoadAndMerge_CreatesManagedFile_WhenAbsent()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog", "kat;cat"]);
        File.Delete(_managedPath);

        Build().LoadAndMerge();

        File.Exists(_managedPath).Should().BeTrue();
    }

    [Fact]
    public void LoadAndMerge_ReturnsAllPrecompiledWords_WhenManagedAbsent()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog", "kat;cat"]);
        File.Delete(_managedPath);

        var result = Build().LoadAndMerge();

        result.Should().HaveCount(2);
        result.Should().Contain(w => w.Question == "hond");
        result.Should().Contain(w => w.Question == "kat");
    }

    // ── Merge logic ───────────────────────────────────────────────────────────

    [Fact]
    public void LoadAndMerge_PreservesProgress_FromManagedFile()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog"]);
        File.WriteAllLines(_managedPath, ["hond;dog;50;3;Other"]);

        var result = Build().LoadAndMerge();

        result.Should().HaveCount(1);
        result[0].WeightData.Weight.Should().Be(50);
        result[0].WeightData.CorrectStreak.Should().Be(3);
        result[0].Group.Should().Be(WordGroup.Other);
    }

    [Fact]
    public void LoadAndMerge_MergesNewWords_FromPrecompiled()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog", "kat;cat"]);
        File.WriteAllLines(_managedPath, ["hond;dog;0;0;Other"]);

        var result = Build().LoadAndMerge();

        result.Should().HaveCount(2);
        result.Should().Contain(w => w.Question == "kat");
    }

    [Fact]
    public void LoadAndMerge_NewWords_GetInitialWeight()
    {
        File.WriteAllLines(_precompiledPath, ["de hond;dog"]);
        File.WriteAllLines(_managedPath, []);

        var result = new WordListService(_precompiledPath, _managedPath).LoadAndMerge();

        result.Should().HaveCount(1);
        result[0].WeightData.Weight.Should().Be(WordWeightStrategy.MaxWeight / 2);
        result[0].WeightData.CorrectStreak.Should().Be(WordWeightStrategy.LinearStreakThreshold);
    }

    [Fact]
    public void LoadAndMerge_DoesNotDuplicateWords_WhenAllAlreadyInManaged()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog"]);
        File.WriteAllLines(_managedPath, ["hond;dog;10;2;Other"]);

        var result = Build().LoadAndMerge();

        result.Should().HaveCount(1);
    }

    // ── Partial columns ───────────────────────────────────────────────────────

    [Fact]
    public void LoadAndMerge_UsesDefaultProgress_WhenManagedHasPartialColumns()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog"]);
        File.WriteAllLines(_managedPath, ["hond;dog"]); // no weight/streak/group columns

        var result = Build().LoadAndMerge();

        result[0].WeightData.Weight.Should().Be(0);
        result[0].WeightData.CorrectStreak.Should().Be(0);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(";dog")]   // empty question
    [InlineData("hond;")] // empty answer
    public void LoadAndMerge_Throws_WhenManagedHasEmptyQuestionOrAnswer(string line)
    {
        File.WriteAllLines(_precompiledPath, []);
        File.WriteAllLines(_managedPath, [line]);

        Build().Invoking(s => s.LoadAndMerge())
            .Should().Throw<InvalidDataException>();
    }

    // ── Whitespace trimming ───────────────────────────────────────────────────

    [Fact]
    public void LoadAndMerge_PreservesProgress_WhenManagedCsvHasWhitespace()
    {
        // arrange
        File.WriteAllLines(_precompiledPath, ["de hond;dog"]);
        File.WriteAllLines(_managedPath, [" de hond ; dog ; 5 ; 2 ; Noun"]);

        // act
        var result = new WordListService(_precompiledPath, _managedPath).LoadAndMerge();

        // assert
        result.Should().HaveCount(1);
        result[0].WeightData.Weight.Should().Be(5);
        result[0].WeightData.CorrectStreak.Should().Be(2);
    }

    // ── SaveWords ─────────────────────────────────────────────────────────────

    [Fact]
    public void SaveWords_PersistsProgress()
    {
        File.WriteAllLines(_precompiledPath, ["hond;dog"]);
        File.Delete(_managedPath);

        var service = Build();
        var words = service.LoadAndMerge();
        words[0].WeightData.Weight = 42;
        words[0].WeightData.CorrectStreak = 7;
        service.SaveWords();

        var reloaded = Build().LoadAndMerge();
        reloaded[0].WeightData.Weight.Should().Be(42);
        reloaded[0].WeightData.CorrectStreak.Should().Be(7);
    }
}
