# Changelog

## [Unreleased]

### Added

- **Quiz direction** — configurable in the Options window: Direct (Dutch → English), Reverse (English → Dutch), or Random.
- **Quiz difficulty levels** — Easy / Hard difficulty setting in the Options window. Hard mode uses string similarity (Jaro-Winkler) to select distractors that are lexically close to the correct answer.
- **Pause/resume from tray** — the quiz timer can be paused and resumed via the system tray context menu. The tray icon changes to a distinct paused icon while paused, and the tooltip shows the current timer state.
- **Initial weight for new words** — newly added words start with a non-zero weight so they appear in quizzes sooner.
- **Penalize wrong distractor** — when the user selects an incorrect option, both the asked word and the chosen distractor receive a weight penalty.
