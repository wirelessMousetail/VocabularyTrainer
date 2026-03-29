# VocabularyTrainer Architecture

Technical architecture documentation for the VocabularyTrainer application.

## Table of Contents

1. [Overview](#overview)
2. [Architecture Principles](#architecture-principles)
3. [Layer Structure](#layer-structure)
4. [Component Diagram](#component-diagram)
5. [Key Design Patterns](#key-design-patterns)
6. [Data Flow](#data-flow)
7. [State Management](#state-management)
8. [Extension Points](#extension-points)

---

## Overview

VocabularyTrainer is a cross-platform desktop application built on .NET 8.0. The architecture follows a layered approach with clear separation between presentation, business logic, and data persistence layers.

### Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: Avalonia UI (MVVM, cross-platform)
- **Language**: C# 12
- **Data Storage**: CSV (vocabulary), JSON (settings)
- **Platform**: Windows 10+, macOS 10.15+, Linux

### Core Components

```
┌─────────────────────────────────────────────────────┐
│                 ApplicationService                  │
│          (Application Orchestrator)                 │
└──────────────┬──────────────────────────────────────┘
               │
       ┌───────┴──────────────┐
       │                      │
┌──────▼───────┐  ┌───────────▼───────┐
│  UI Layer    │  │   Service Layer   │
│              │  │                   │
│ - QuizView   │  │ - QuizService     │
│ - OptionsView│  │ - Settings        │
└──────────────┘  │ - WordList        │
                  └───────────┬───────┘
                              │
              ┌───────────────┴───────────┐
              │                           │
       ┌──────▼────────┐  ┌──────────────▼──────────┐
       │  Model Layer  │  │  Infrastructure Layer    │
       │               │  │                          │
       │ - Domain      │  │ - TrayIconService        │
       │ - Data        │  └──────────────────────────┘
       └───────────────┘
```

---

## Architecture Principles

### 1. Separation of Concerns
- **UI Layer**: Handles user interaction and presentation only
- **Service Layer**: Contains business logic and orchestration
- **Model Layer**: Defines domain entities and data structures
- **Infrastructure Layer**: Manages system integration (tray icon, timers)

### 2. Dependency Direction
Dependencies flow inward:
```
UI → Services → Models
Infrastructure → Services → Models
```

Models have no dependencies on other layers, making them portable and testable.

### 3. Immutability Where Possible
- `AppSettings` and `QuizConfiguration` use `init` accessors
- Promotes thread-safety and predictable state
- Reduces bugs from accidental mutations

### 4. Single Responsibility
Each class has a focused, well-defined purpose:
- `QuizService`: Creates quizzes
- `QuizPresenter`: Handles quiz interaction logic
- `WordListService`: Manages vocabulary persistence
- `SettingsService`: Manages application settings

### 5. Interface Segregation
Interfaces are used where abstraction adds value:
- `IQuizPresenter`: Allows for different quiz presentation strategies
- Enables testing and future extensibility

---

## Layer Structure

### Model Layer (`Models/`)

Domain entities representing core concepts:

**Domain Models:**
- `WordEntry`: Represents a vocabulary word with question, answer, weight data, and group
- `WeightData`: Encapsulates weight and streak tracking
- `Quiz`: Data carrier for a single quiz (question, correct answer, options, word)
- `QuizSession`: Bundles quiz, presenter, and configuration

**Configuration Models:**
- `AppSettings`: Root application settings
- `QuizConfiguration`: Quiz-specific settings (options count, direction, auto-close, etc.)

**Enums:**
- `WordGroup`: Noun, Verb, Other
- `QuizDirection`: Direct, Reverse, Random
- `QuizResult`: Pending, Correct, Wrong, MaxAttemptsReached

**Characteristics:**
- Plain C# objects (POCOs)
- No external dependencies
- Immutable where appropriate (using `init` accessors or constructors)

### Service Layer (`Services/`)

Business logic and orchestration:

**Core Services:**

1. `QuizService`: Creates quiz sessions
   - Selects words using weight-based probability
   - Generates multiple-choice options from same word group
   - Applies quiz direction (Direct, Reverse, Random)
   - Uses `Quiz` (`Models/Quiz.cs`) as a data carrier (question, correct answer, options)

2. `WordListService`: Manages application vocabulary
   - Loads and merges the precompiled word list into the managed word list during startup
   - Persists the managed list with progress data
   - Handles CSV parsing and serialization

3. `SettingsService`: Manages application settings
   - Loads settings from JSON during startup
   - Provides immutable settings access
   - Updates and persists settings changes

4. `WordWeightStrategy`: Implements word weight evaluation algorithms
   - Implements weight increase/decrease algorithms
   - Calculates ticket counts for word selection

**Presenter Pattern:**

5. **`IQuizPresenter` / `QuizPresenter`**
   - Handles quiz interaction logic
   - Tracks answer attempts
   - Applies weight updates via strategy
   - Returns quiz results

**Utilities:**

6. **`CsvWordRepository`**
   - Loads precompiled CSV word lists
   - Simple two-column format parser

7. **`WordGrouping`**
   - Static utility for detecting word groups
   - Dutch-specific grammar rules (detects "de/het" for nouns, "-en" for verbs)

**Application Orchestrator:**

8. **`ApplicationService`**
   - Main application orchestrator
   - Manages quiz timer (AutoReset=false)
   - Coordinates services and fires UI events (`QuizRequested`, `OptionsRequested`, `ExitRequested`)

### UI Layer

**Views (Avalonia MVVM):**

1. **`QuizView` / `QuizViewModel`**
   - Displays quiz questions and multiple-choice options
   - Handles user answer selection
   - Shows result feedback
   - Auto-closes on correct answers

2. **`OptionsView` / `OptionsViewModel`**
   - Settings configuration UI
   - Input fields for intervals, counts, and direction
   - Saves changes via `SettingsService`

**Application Entry Point:**

3. **`App` (`App.axaml.cs`)**
   - Avalonia application entry point
   - Wires services to views via events
   - Dispatches UI work to the UI thread

### Infrastructure Layer

**System Integration:**

1. **`TrayIconService`**
   - Creates and manages system tray icon
   - Provides context menu (Pause, Resume, Options, Exit)
   - Handles icon disposal

---

## Component Diagram

### High-Level Component Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                    ApplicationService                       │
│                                                             │
│  ┌──────────────┐   ┌──────────────┐  ┌─────────────────┐   │
│  │   Timer      │   │TrayIconSvc   │  │ SettingsService │   │
│  └──────┬───────┘   └──────┬───────┘  └────────┬────────┘   │
│         │                  │                   │            │
│         │ (on tick)        │ (on action)       │ (get)      │
│         ▼                  ▼                   ▼            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              ShowQuiz() / OpenOptions()              │   │
│  └──────────────┬───────────────────────────────────────┘   │
└─────────────────┼───────────────────────────────────────────┘
                  │ (events → App.axaml.cs → Dispatcher)
         ┌────────┴────────┐
         │                 │
    ┌────▼────┐      ┌─────▼──────┐
    │QuizView │      │OptionsView │
    └────┬────┘      └─────┬──────┘
         │                 │
         │                 │ (updates)
         │                 ▼
    ┌────▼───────────────────────────┐
    │       QuizSession              │
    │  ┌────────────────────────┐    │
    │  │  Quiz (data)           │    │
    │  │  QuizPresenter (logic) │────┼──► WordWeightStrategy
    │  │  QuizConfiguration     │    │
    │  └────────────────────────┘    │
    └───────┬────────────────────────┘
            │
            │ (on answer)
            ▼
    ┌──────────────────┐
    │ WordListService  │
    │   (saves)        │
    └──────────────────┘
```

### Service Dependencies

```
QuizService
  ├─► WordWeightStrategy (calculates tickets)
  └─► WordEntry (domain model)

QuizPresenter
  ├─► WordWeightStrategy (registers correct/mistake)
  ├─► WordListService (saves changes)
  └─► Quiz (data)

WordListService
  ├─► CsvWordRepository (loads precompiled)
  ├─► WordEntry (domain model)
  └─► File I/O (saves managed list)

SettingsService
  ├─► AppSettings (domain model)
  └─► File I/O (JSON serialization)
```

---

## Key Design Patterns

### 1. Presenter Pattern (MVP)

**Intent**: Separate presentation logic from UI concerns

**Implementation:**
```csharp
public interface IQuizPresenter
{
    void OnAnswerSelected(string selectedAnswer);
    QuizResult GetResult();
    string GetCorrectAnswer();
}

public class QuizPresenter : IQuizPresenter
{
    private readonly Quiz _quiz;
    private readonly WordWeightStrategy _strategy;
    private readonly WordListService _wordListService;
    // ... interaction logic
}
```

**Benefits:**
- Business logic testable without UI
- Clear separation of concerns
- UI becomes thin adapter

### 2. Strategy Pattern

**Intent**: Encapsulate weight calculation algorithms

**Implementation:**
```csharp
public class WordWeightStrategy
{
    public void RegisterCorrect(WordEntry word) { /* algorithm */ }
    public void RegisterMistake(WordEntry word) { /* algorithm */ }
    public int CalculateTickets(WordEntry word) { /* algorithm */ }
}
```

**Benefits:**
- Weight algorithms can be swapped
- Easy to test in isolation
- Single responsibility

### 3. Service Layer Pattern

**Intent**: Centralize business logic in cohesive services

**Examples:**
- `QuizService`: Quiz creation orchestration
- `WordListService`: Vocabulary persistence
- `SettingsService`: Configuration management

**Benefits:**
- Clear API boundaries
- Reusable across UI components
- Simplifies testing

### 4. Repository Pattern

**Intent**: Abstract data access

**Implementation:**
```csharp
public class CsvWordRepository
{
    public List<WordEntry> Load(string path) { /* CSV parsing */ }
}
```

**Benefits:**
- Data source can be replaced (CSV → Database)
- Testable with mock repositories

### 5. Immutable Objects

**Intent**: Prevent accidental state mutations

**Implementation:**
```csharp
public class AppSettings
{
    public int QuizIntervalSeconds { get; init; } = 300;
    public QuizConfiguration QuizConfiguration { get; init; } = new();
}
```

**Benefits:**
- Thread-safe by default
- Predictable state
- Easier reasoning about code flow

### 6. Dependency Injection (Manual)

**Intent**: Provide dependencies explicitly

**Implementation:**
```csharp
public ApplicationService(SettingsService settingsService)
{
    _settingsService = settingsService;
    _wordListService = new WordListService(precompiledPath, managedPath);
    _quizService = new QuizService(words, weightStrategy);
}
```

**Benefits:**
- Clear dependency graph
- Testable with mock services
- No hidden dependencies

---

## Data Flow

### Application Startup Flow

```
1. Program.Main() / App.OnFrameworkInitializationCompleted()
   │
   ├─► Create SettingsService (loads appsettings.json)
   │
   └─► Create ApplicationService
       │
       ├─► Create WordListService
       │   ├─► Load Data/words.csv (precompiled)
       │   ├─► Load appdata.csv (managed)
       │   └─► Merge and save to appdata.csv
       │
       ├─► Create QuizService (with words + WeightStrategy)
       │
       ├─► Initialize TrayIconService
       │
       ├─► Start quiz timer
       │
       └─► Show first quiz immediately
```

### Quiz Flow

```
1. Timer tick → ApplicationService.ShowQuiz()
   │
2. QuizService.CreateQuizSession()
   │
   ├─► SelectWordByWeight() [ticket-based selection]
   │   │
   │   ├─► Build ticket pool (1 + weight tickets per word)
   │   └─► Random selection from pool
   │
   ├─► CreateQuiz()
   │   │
   │   ├─► Select options from same word group
   │   ├─► Apply quiz direction (Direct/Reverse/Random)
   │   └─► Shuffle options
   │
   └─► Create QuizPresenter + QuizSession
       │
3. QuizRequested event → App dispatches → Display QuizView
   │
4. User clicks answer button
   │
   ├─► QuizPresenter.OnAnswerSelected()
   │   │
   │   ├─► Check if correct
   │   │
   │   ├─► If correct:
   │   │   ├─► WordWeightStrategy.RegisterCorrect()
   │   │   └─► WordListService.SaveWords()
   │   │
   │   └─► If wrong:
   │       ├─► WordWeightStrategy.RegisterMistake()
   │       └─► WordListService.SaveWords()
   │
5. QuizView shows result
   │
   └─► Auto-close (if correct) or allow retry (if wrong)
```

### Settings Update Flow

```
1. User opens OptionsView
   │
2. OptionsViewModel loads from settings
   │
   └─► SettingsService.GetSettings()
       │
3. User modifies values
   │
4. User clicks Save
   │
   ├─► SettingsService.UpdateSettings()
   │   │
   │   ├─► Create new immutable AppSettings
   │   └─► Save to appsettings.json
   │
   └─► ApplicationService.ApplySettings()
       │
       └─► Update timer interval
```

### Word List Merge Flow

```
On Startup:

1. WordListService.LoadAndMerge()
   │
   ├─► Load Data/words.csv via CsvWordRepository
   │   └─► Returns List<WordEntry> (question, answer only)
   │
   ├─► Load appdata.csv
   │   └─► Returns List<WordEntry> (with weight, streak, group)
   │
   ├─► Merge logic:
   │   │
   │   ├─► For each word in precompiled:
   │   │   │
   │   │   ├─► If exists in managed → skip (keep existing progress)
   │   │   └─► If new → add to managed (weight=0, streak=0, detect group)
   │   │
   │   └─► Save merged list to appdata.csv
   │
   └─► Return merged word list
```

---

## State Management

### Application State

**Location**: `ApplicationService`

**State Variables:**
- `_isPaused`: Boolean flag for pause/resume state
- `_nextQuizTimer`: System.Timers.Timer for quiz scheduling

**State Transitions:**
```
Running ──[Pause]──► Paused
  ▲                     │
  └────[Resume]─────────┘
```

### Quiz State

**Location**: `QuizPresenter`

**State Variables:**
- `_result`: Current quiz result (Pending, Correct, Wrong, MaxAttemptsReached)
- `_attemptCount`: Number of attempts made

**State Transitions:**
```
Pending ──[Correct Answer]──► Correct
   │
   └──[Wrong Answer]──► Wrong ──[Max Attempts]──► MaxAttemptsReached
          ▲               │
          └───[Retry]─────┘
```

### Word State

**Location**: `WordEntry` → `WeightData`

**State Variables:**
- `Weight`: 0-100 (difficulty weight)
- `CorrectStreak`: Number of consecutive correct answers

**State Transitions:**
```
Weight:
  Wrong Answer → weight = (weight * 3) + 1 (cap at 100)
  Correct Answer (streak ≤ 5) → weight = weight - 1
  Correct Answer (streak > 5) → weight = weight * 0.5

CorrectStreak:
  Correct Answer → streak++
  Wrong Answer → streak = 0
```

### Settings State

**Location**: `SettingsService._currentSettings`

**Characteristics:**
- Immutable `AppSettings` object
- Updated via `UpdateSettings()` (creates new instance)
- Persisted to `appsettings.json` on every update

---

## Extension Points

### Adding New Quiz Types

**Current**: Multiple choice quizzes
**Extension Point**: Create new `IQuizPresenter` implementation

```csharp
public class FillInBlankPresenter : IQuizPresenter
{
    public void OnAnswerTyped(string typedAnswer) { /* ... */ }
    // ...
}
```

Update `QuizService.CreateQuizSession()` to return appropriate presenter based on configuration.

### Adding New Word Selection Strategies

**Current**: Weight-based ticket selection
**Extension Point**: Create new selection method in `QuizService`

```csharp
private WordEntry SelectWordByLastSeen()
{
    // Select least recently seen word
}
```

Add configuration to `QuizConfiguration` to choose strategy.

### Adding New Weight Algorithms

**Current**: Exponential increase, linear/exponential decrease
**Extension Point**: Create `IWeightStrategy` interface

```csharp
public interface IWeightStrategy
{
    void RegisterCorrect(WordEntry word);
    void RegisterMistake(WordEntry word);
    int CalculateTickets(WordEntry word);
}

public class ExponentialWeightStrategy : IWeightStrategy { /* ... */ }
public class LinearWeightStrategy : IWeightStrategy { /* ... */ }
```

### Adding New Word Sources

**Current**: CSV files
**Extension Point**: Create new repository implementation

```csharp
public class DatabaseWordRepository
{
    public List<WordEntry> Load(string connectionString) { /* ... */ }
}
```

Update `WordListService` to accept repository as dependency.

### Adding New Language Support

**Current**: Dutch-specific word grouping
**Extension Point**: Create language-specific `IWordGroupingStrategy`

```csharp
public interface IWordGroupingStrategy
{
    WordGroup DetectGroup(string word);
}

public class DutchWordGrouping : IWordGroupingStrategy { /* ... */ }
public class FrenchWordGrouping : IWordGroupingStrategy { /* ... */ }
```

Configure strategy based on language setting.

### Adding Statistics/Analytics

**Extension Point**: Create `StatisticsService`

```csharp
public class StatisticsService
{
    public QuizStatistics GetStatistics(DateTime from, DateTime to);
    public List<WordEntry> GetMostDifficultWords(int count);
    public double GetAccuracyRate();
}
```

Track quiz history in separate data store (CSV or database).

### Adding Notification System

**Extension Point**: Create `INotificationService`

```csharp
public interface INotificationService
{
    void ShowToast(string message);
    void ShowBadge(int count);
}

public class WindowsNotificationService : INotificationService { /* ... */ }
```

Inject into `ApplicationService` for milestone notifications.

---

## Testing Strategy

### Unit Testing

**Highly Testable Components:**
- `WordWeightStrategy`: Pure logic, no dependencies
- `WordGrouping`: Static utility, deterministic
- `QuizPresenter`: Business logic with mockable dependencies
- `SettingsService`: File I/O can be mocked

**Example:**
```csharp
[Fact]
public void RegisterMistake_IncreasesWeight()
{
    var strategy = new WordWeightStrategy();
    var word = WordEntryFixture.Make("test", "test", weight: 10);

    strategy.RegisterMistake(word);

    word.WeightData.Weight.Should().Be(31); // (10 * 3) + 1
}
```

### Integration Testing

**Test Scenarios:**
- Quiz flow from creation to answer submission
- Word list merge on startup
- Settings persistence and reload

### Manual Testing

**Critical Paths:**
- System tray integration
- Timer functionality
- UI responsiveness
- Auto-close behavior

---

## Performance Considerations

### Memory Usage

**Optimizations:**
- Word list loaded once on startup (~10KB for 100 words)
- Immutable objects prevent defensive copying
- Minimal object allocations during quiz flow

**Current Footprint**: ~50 MB RAM (mostly .NET runtime overhead)

### CPU Usage

**Optimizations:**
- Ticket pool built only during word selection (O(n × weight))
- No continuous background processing
- Timer-based event model (not polling)

**Current Usage**: Near-zero when idle, <1% during quiz

### Disk I/O

**Operations:**
- Startup: Load 2 CSV files + 1 JSON file
- Per Quiz: Save 1 CSV file (appdata.csv, ~10-50 KB)
- Settings Change: Save 1 JSON file (appsettings.json, <1 KB)

**Optimizations:**
- Buffered file writes
- Minimal file access frequency

### Scalability

**Word List Size:**
- Tested with: Up to 1000 words
- Ticket pool complexity: O(total_weight_sum)
- Practical limit: ~10,000 words before noticeable delay

**Improvement Opportunity**: Cache ticket pool and invalidate only on weight changes

---

## Security Considerations

### Data Privacy

- All data stored locally (no network transmission)
- No telemetry or analytics
- CSV files human-readable and user-controlled

### File Access

- Read/write only to application directory
- No elevated privileges required
- No access to system files

### Input Validation

- CSV parsing handles malformed files gracefully
- JSON deserialization uses default settings (no custom converters)
- User input limited to numeric settings (validated by UI controls)

---

## Future Architecture Improvements

### 1. Dependency Injection Container

Replace manual DI with container (e.g., Microsoft.Extensions.DependencyInjection):
```csharp
services.AddSingleton<ISettingsService, SettingsService>();
services.AddScoped<IQuizService, QuizService>();
```

### 2. Event-Driven Architecture

Replace direct method calls with events:
```csharp
public event EventHandler<QuizCompletedEventArgs> QuizCompleted;
```

Benefits: Decoupling, easier extension

### 3. Database Storage

Migrate from CSV to SQLite:
- Better query performance for large word lists
- Support for quiz history tracking
- Atomic transactions

### 4. Configuration System

Use `IOptions<T>` pattern for settings:
```csharp
public class QuizService
{
    public QuizService(IOptions<AppSettings> settings) { /* ... */ }
}
```

### 5. Logging Framework

Add structured logging (e.g., Serilog):
```csharp
_logger.LogInformation("Quiz completed: {Result}", result);
```

### 6. Asynchronous I/O

Use async file operations:
```csharp
public async Task SaveWordsAsync()
{
    await File.WriteAllLinesAsync(_managedPath, lines);
}
```

Benefits: Non-blocking UI during saves

---

## Conclusion

VocabularyTrainer's architecture balances simplicity with maintainability. The clear layering, use of established patterns, and minimal dependencies make it approachable for contributors while providing solid foundations for future enhancements.

**Key Strengths:**
- Clear separation of concerns
- Testable business logic
- Immutable domain models
- Extensible through well-defined interfaces

**Areas for Growth:**
- Formalized dependency injection
- Comprehensive test coverage
- Enhanced error handling and logging
- Database migration for scalability
