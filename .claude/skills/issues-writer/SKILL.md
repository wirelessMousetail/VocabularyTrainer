---
name: "issues-writer"
description: "Use when the user wants to produce a written GitHub issue — a bug report, feature request, or improvement proposal — for the project. Trigger on phrasings like \"write a story\", \"write an issue\", \"draft a ticket\", \"log this as a bug\", \"create a feature request\". Do not trigger when the user wants to implement or fix something directly in the code."
---

# Issue writer

## Instructions:
1. Read the issue description provided by the user and evaluate what needs to be changed.
2. If the description is too vague to write a meaningful issue — missing the core problem, the motivation, or what done looks like — ask the user one or two focused clarifying questions before proceeding. Do not guess or pad with placeholders.
3. Study the relevant code area to verify assumptions — check actual class/method names, existing behaviour, and any constraints — before writing.
4. Choose the correct format and write the issue:
   - **Feature or technical improvement:** use the structure below in section 4.1
   - **Bug fix:** use the structure below in section 4.2
5. Always include a short, descriptive **title** as the first line, prefixed with `# `.
6. If important information does not fit any proposed section, add an extra section with an appropriate name.
7. Write concisely. Do not pad sections or restate what the user already said. Each section should add information.
8. Print the issue as unrendered markdown so the user can copy-paste it into the tracker as-is.

### 4.1 Feature / technical improvement structure
```markdown
# <Title>

## What
<What is being added or changed — one or two sentences.>

## Why
<Why this feature is valuable: what problem it solves, how it improves the user's experience or makes the app more useful.>

## How
<High-level implementation steps. Focus on what needs to happen, not how to code it. Only mention specific files, classes, or methods if omitting them would make a step ambiguous or hide a non-obvious constraint.>
```

### 4.2 Bug fix structure
```markdown
# <Title>

## Expected behavior
WHEN <context>
AND <condition>
THEN <expected outcome>

## Actual behavior
<What actually happens instead.>

## Steps to reproduce
1. <Step>
2. <Step>
3. ...
```

## Rules:
* Do not attempt to create the issue in the tracker - you do not have access.
* Do not invent implementation details you could not verify from the code.

## Examples:

```markdown
# Convert repeated [Fact] tests to [Theory] with parameterized inputs

## What
Many unit tests repeat the same assertion logic with different input values using separate `[Fact]` methods. These should be converted to `[Theory]` with `[InlineData]` or `[MemberData]`.

## Why
Duplicated test structure increases maintenance cost: adding a new case means adding a new method, and a logic change requires updating multiple places. Parameterized tests make the intent clearer and make it trivial to add edge cases.

## How
1. Identify `[Fact]` tests that share the same assertion logic but differ only in input/output values.
2. Rewrite them as `[Theory]` with `[InlineData]` (scalar inputs) or `[MemberData]` / `TheoryData<>` (complex objects).
3. Ensure test names remain descriptive — xUnit appends parameter values to the display name automatically.
4. Do not parameterize tests where variation in setup or assertion logic makes a shared structure artificial.
```

```markdown
# Typing quiz accepts only the first correct answer when multiple exist

## Expected behavior
WHEN the quiz is in typing mode
AND the question has more than one correct answer
AND the user types any one of the correct answers
THEN the quiz marks the result as correct

## Actual behavior
The quiz marks the result as incorrect unless the user types the exact first answer stored in the CSV.

## Steps to reproduce
1. Start the quiz.
2. Wait for a question that has multiple accepted answers (or rig the app to show a specific word).
3. Type one of the correct answers that is not the first one and press Enter.
```

