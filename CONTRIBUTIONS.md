# Contributing to VocabularyTrainer

## Build & Run

```bash
# Build
dotnet build

# Run
dotnet run
```

## Tests

```bash
# Run all tests
dotnet test

# Run tests matching a name pattern
dotnet test --filter "Name~CreateQuizSession"

# Run all tests in a class
dotnet test --filter "ClassName=QuizServiceTests"
```

All business logic must be covered by unit tests. When fixing a bug, include a regression test that fails before the fix and passes after.

## Pull Request Expectations

- Keep PRs focused on a single concern.
- Include tests for any business logic changes.
- Run `dotnet test` locally before submitting — all tests must pass.

## Word Grouping

Word group detection is implemented in `WordGrouping.cs` and is Dutch-specific. The rules (noun detection via "de"/"het" articles, verb detection via "-en" suffix) reflect Dutch grammar. Changes to grouping logic require Dutch grammar context to avoid breaking word selection.

## Architecture Note

ViewModels receive data and callbacks via constructor parameters — they do not reference services directly. `App.axaml.cs` is the only place that wires layers together. Keep UI, business logic, and data access strictly separated.
