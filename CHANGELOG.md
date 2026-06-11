# Changelog

## [1.2.0] - 2026-06-11

### Fixed

- **Subset/intersection answer filtering in multiple-choice options** — a distractor is now excluded when any of its comma-separated answer parts matches any part of the correct answer, preventing words like `beslissen ; to decide` from appearing as a wrong option alongside `besluiten ; to decide, to resolve`.

### Added

- **Alternatives shown after correct typing answer** — after typing a correct answer in typing-quiz mode, if the word has multiple accepted answers, the full answer string (showing all alternatives) is shown in the additional info area below the result.
- **Switch to options in typing mode** — a "Switch to options" button is now shown during typing-mode quizzes. Clicking it immediately applies a maximum weight penalty (100) to the word, closes the typing window, and opens a standard multiple-choice quiz for the same word. The switch applies only to the current question; the next quiz continues in typing mode.

## [1.1.0] - 2026-05-07

### Changed

- **Visual redesign — accent header layout** — visual design of all application windows was improved 

### Added

- **Answer additional info after correct answer** — in multiple-choice mode, when the correct answer contains parenthetical content (e.g. "the current (water, air, electricity)"), the full unstripped answer is shown as an additional info below the result message after the user answers correctly.

### Changed

- **Auto-focus input on typing quiz open** — the answer field is now focused automatically when the typing quiz window opens, so the user can start typing immediately without clicking.
- **Hyphen-insensitive answer matching** — hyphens are treated as spaces during answer normalization, so `after-school` and `after school` are accepted as equivalent.

- **Word list sanitization** — fixed typos, removed Cyrillic text, corrected separators, and improved phrasing across `Data/words.csv`. MC quiz options and correct-answer labels now strip parenthetical context (`(...)`) before display, matching typing mode behaviour.
- **Answer format validation** — `Data/words.csv` is validated on load; entries with Cyrillic characters, invalid punctuation, malformed brackets, or empty fields throw a `FormatException` at startup.
- **Answer update on merge** — when `Data/words.csv` contains a corrected answer for an existing word, `LoadAndMerge()` now updates the stored answer while preserving weight, streak, and group.
- **Parameterized unit tests** — converted repetitive `[Fact]` tests in `StringDistanceTests` and `AnswerParserTests` to `[Theory]` tests using `[InlineData]` and `[MemberData]`.

## [1.0.0] - 2026-04-20

### Added

- **Quiz direction** — configurable in the Options window: Direct (Dutch → English), Reverse (English → Dutch), or Random.
- **Typing mode** — new quiz mode where the user types the answer instead of selecting from options. Selectable in the Options window alongside Easy/Hard difficulty.
- **Letter hint reveal** — optional setting for typing mode. After each wrong attempt, correctly aligned letters are progressively revealed. A random bonus reveal fires when the alignment gate (contiguous match ≥ 3 chars) does not open. The last unrevealed character is always protected.
- **Wrong article detection** — in typing mode, typing the correct Dutch noun with the wrong article (de/het) gives a distinct "Wrong article!" result instead of a plain wrong answer.
- **Multiple accepted answers** — answers may contain comma-separated alternatives; parenthetical notes (e.g. hints in brackets) are stripped before evaluation.
- **Quiz difficulty levels** — Easy / Hard difficulty setting in the Options window. Hard mode uses string similarity (Jaro-Winkler) to select distractors that are lexically close to the correct answer.
- **Pause/resume from tray** — the quiz timer can be paused and resumed via the system tray context menu. The tray icon changes to a distinct paused icon while paused, and the tooltip shows the current timer state.
- **Initial weight for new words** — newly added words start with a non-zero weight so they appear in quizzes sooner.
- **Penalize wrong distractor** — when the user selects an incorrect option, both the asked word and the chosen distractor receive a weight penalty.
