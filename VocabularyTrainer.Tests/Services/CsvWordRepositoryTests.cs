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
    public void Load_SkipsLines_WithWrongColumnCount(string line) //todo change behavior: fail if not compliant (1 or more than 2 columns is not compliant, 2 columns if one of them empty - not compliant, completely empty line - compliant  
    {
        File.WriteAllLines(_tempFile, [line, "vis;fish"]);

        var result = _repo.Load(_tempFile);

        result.Should().HaveCount(1);
        result[0].Question.Should().Be("vis");
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
}
