# Changelog

## [Unreleased]

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
