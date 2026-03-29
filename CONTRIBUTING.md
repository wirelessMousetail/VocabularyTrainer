# Contributing to VocabularyTrainer

Contributions are welcome. Please follow the process below to keep changes coordinated and avoid wasted effort.

## Reporting Bugs

Open an issue describing:
- What you expected to happen
- What actually happened
- Steps to reproduce
- OS and .NET version

## Suggesting Features

Open an issue describing the feature and the problem it solves. Wait for a maintainer response before writing any code — the feature may be out of scope or require design discussion first.

## Contributing Code

1. **Open an issue** describing the bug fix or improvement.
2. **Wait for the `accepted` label** to be added by the maintainer. This signals the change is in scope and you are clear to proceed.
3. **Fork the repository** and create a branch from `main`.
4. **Write your changes** and include tests for any business logic (see below).
5. **Open a pull request** referencing the issue (e.g. `Closes #42`).

Pull requests opened without a linked `accepted` issue will be closed without review.

## Code Requirements

- Run `dotnet test` locally — all tests must pass before submitting.
- Include a unit test for any business logic change. For bug fixes, include a regression test that fails before the fix and passes after.
- Keep PRs focused on a single concern. Unrelated changes belong in a separate issue and PR.
- Follow the existing layer separation: ViewModels receive data and callbacks via constructor and do not reference services directly. `App.axaml.cs` is the only place that wires layers together.

## Build & Test Commands

```bash
dotnet build
dotnet run
dotnet test

# Filter by test name pattern
dotnet test --filter "Name~CreateQuizSession"

# Filter by test class
dotnet test --filter "ClassName=QuizServiceTests"
```

## Word Grouping Note

`WordGrouping.cs` is Dutch-specific — it detects nouns via "de"/"het" articles and verbs via the "-en" suffix. Changes to grouping logic require Dutch grammar context to avoid breaking word selection behaviour.
