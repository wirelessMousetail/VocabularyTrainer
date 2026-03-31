# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run
dotnet run

# Test (all)
dotnet test

# Test (single test by name pattern)
dotnet test --filter "Name~CreateQuizSession"

# Test (all tests in a class)
dotnet test --filter "ClassName=QuizServiceTests"
```

## Architecture

Tray app that fires periodic multiple-choice vocabulary quizzes. No main window — `ShutdownMode = OnExplicitShutdown`.

**Stack:** .NET 8 / C# 12, Avalonia UI (cross-platform MVVM), CSV + JSON storage, xUnit + FluentAssertions tests.

### Event-driven flow

`App.axaml.cs` wires `ApplicationService` events to UI creation on `Dispatcher.UIThread.Post`:
- `QuizRequested` → create `QuizViewModel` + `QuizView`
- `OptionsRequested` → create `OptionsViewModel` + `OptionsView`
- `ExitRequested` → shutdown

Quiz timer (`AutoReset=false`) is manually restarted in `OnQuizCompleted()` after the window closes.

### Quiz lifecycle

1. Timer fires → `ApplicationService.ShowQuiz()`
2. `QuizService.CreateQuizSession()` — selects word by weight (ticket pool: `1 + weight` tickets each), generates options from same `WordGroup`, applies `QuizDirection`
3. `QuizRequested` event → `App.axaml.cs` creates view
4. User answers → `QuizPresenter.OnAnswerSelected()` → `WordWeightStrategy` updates weight → `WordListService.SaveWords()`
5. Correct: auto-close timer starts → window closes → `OnQuizCompleted()` → timer restarts

### Weight algorithm (`WordWeightStrategy`)

- **Mistake:** `weight = (weight * 3) + 1`, capped at 100; streak → 0
- **Correct (streak ≤ 5):** `weight = weight - 1` (min 0)
- **Correct (streak > 5):** `weight = weight * 0.5` (min 0)

### Data files

- `Data/words.csv` — precompiled vocabulary, 2 columns: `question;answer`
- `appdata.csv` — runtime-managed list with progress, 5 columns: `question;answer;weight;streak;group` (auto-created by merging on startup)
- `appsettings.json` — user settings (JSON)

On startup `WordListService.LoadAndMerge()` merges the two CSVs: new words from `Data/words.csv` get weight=0, streak=0, auto-detected group; existing words retain progress.

### Word groups (Dutch-specific, `WordGrouping.cs`)

- **Noun:** starts with `"de "` or `"het "`
- **Verb:** ends with `"en"`
- **Other:** everything else

### Key service responsibilities

| Service | Responsibility |
|---|---|
| `ApplicationService` | Orchestrates timer, coordinates all services, fires events |
| `QuizService` | Creates `QuizSession` (word selection + options generation) |
| `QuizPresenter` | Processes answers, updates weights, persists via `WordListService` |
| `WordListService` | CSV load/merge/save |
| `WordWeightStrategy` | Weight calculation algorithms |
| `SettingsService` | `appsettings.json` load/save; `AppSettings` is immutable (`init`) |
| `CsvWordRepository` | Raw CSV parsing (reused by both services) |
| `TrayIconService` | System tray icon and context menu |

### Models

`AppSettings` and `QuizConfiguration` use `init` accessors (immutable after construction). `WordEntry` and `QuizSession` are constructor-immutable (get-only properties). `WeightData` is mutable (`set`) — intentionally mutated in place by `WordWeightStrategy`. `Quiz` lives in `Services/Quiz.cs`, not `Models/`.

### Test fixtures

\`WordEntryFixture.Make(question, answer, group?, weight?, streak?)\` creates test `WordEntry` instances. Tests inline-construct services (no shared `Build()` helper).

## Development guidelines

**Test coverage:** All business logic must be covered by unit tests. Every service method with non-trivial logic (weight calculation, word selection, CSV parsing, merge behaviour, answer evaluation) needs a corresponding test. When fixing a bug, add a regression test that fails before the fix and passes after.

**Layer separation:** Keep UI, business logic, and data access strictly separated. ViewModels must not reference services directly — they receive data and callbacks via constructor parameters. Services must not reference ViewModels or Avalonia types. `App.axaml.cs` is the only place that wires layers together.

**Single Responsibility Principle:** Each class should have one reason to change. Prefer splitting a class when it starts owning multiple distinct concerns (e.g. data loading AND transformation AND persistence). Deviation is acceptable only when the split would produce trivially thin classes or when tight coupling between concerns makes separation artificial.
