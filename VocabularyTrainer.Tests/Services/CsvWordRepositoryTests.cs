using FluentAssertions;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class CsvWordRepositoryTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly CsvWordRepository _repo = new();

    public void Dispose() => File.Delete(_tempFile);

    [Fact]
    public void Load_ParsesQuestionsAndAnswers()
    {
        File.WriteAllLines(_tempFile, ["hond;dog", "kat;cat"]);

        var result = _repo.Load(_tempFile);

        result.Should().HaveCount(2);
        result[0].Question.Should().Be("hond");
        result[0].Answer.Should().Be("dog");
        result[1].Question.Should().Be("kat");
        result[1].Answer.Should().Be("cat");
    }

    [Theory]
    [InlineData("hond")]           // too few columns
    [InlineData("hond;dog;extra")] // too many columns
    [InlineData(";dog")]           // empty question
    [InlineData("hond;")]          // empty answer
    public void Load_ThrowsFormatException_ForNonCompliantLines(string line)
    {
        File.WriteAllLines(_tempFile, [line, "vis;fish"]);

        var act = () => _repo.Load(_tempFile);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Load_SkipsBlankLines()
    {
        File.WriteAllLines(_tempFile, ["", "   ", "hond;dog"]);

        var result = _repo.Load(_tempFile);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void Load_ReturnsEmpty_ForEmptyFile()
    {
        File.WriteAllText(_tempFile, "");

        _repo.Load(_tempFile).Should().BeEmpty();
    }

    [Fact]
    public void Load_Throws_WhenFileDoesNotExist()
    {
        // arrange
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");

        // act
        var act = () => _repo.Load(missingPath);

        // assert
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void Load_TrimsWhitespace_FromQuestionAndAnswer()
    {
        // arrange
        File.WriteAllLines(_tempFile, [" de hond ; dog "]);

        // act
        var result = _repo.Load(_tempFile);

        // assert
        result.Should().HaveCount(1);
        result[0].Question.Should().Be("de hond");
        result[0].Answer.Should().Be("dog");
    }
}
