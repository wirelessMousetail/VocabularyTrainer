---
name: "issues-writer"
description: "Writes issues, bugs, user stories for the project. Use it when user asks to create or write a new issue for a new feature, technical improvenent or a bug fix"
---

# Issue writer

## Instructions:
1. Read the issue description provided by the user and evaluate what needs to be changed
2. Make sure that you have all related information about the project in your context. Study project files and documentation, if required
3. Write the issue in the proper format:
   3.1 If you creating a user story for a new feature or technical improvement, then use the structure:
```markdown
## WHAT
## WHY
## HOW
```
   3.2 If you creating a bug fix, then use the structure: 
```markdown
### Expected behavior 
### Actual behavior
### Steps to reproduce
```
4. If some important information does not fall under any of proposed sections, create an additional section and give it a proper name.
5. Print the text of the issue to the output


## Rules:
* Print created issue as unrendered markdown text, so the user can copypaste to the tracker it as is
* Do not attempt to create the issue in the tracker - you do not have the access

## Examples:
```markdown
## What
  Many unit tests repeat the same logic with different input values using separate `[Fact]` methods. These should be converted to `[Theory]` with `[InlineData]` or `[MemberData]`.

## Why
  Duplicated test structure increases maintenance cost: adding a new case means adding a new method, and a logic change requires updating multiple places. Parameterized tests make the intent clearer — "this behaviour holds for all these inputs" — and make it trivial to add edge cases.

## How
  1. Identify `[Fact]` tests that share the same assertion logic but differ only in input/output values
  2. Rewrite them as `[Theory]` with `[InlineData]` (for simple scalar inputs) or `[MemberData]` / `[ClassData]` (for complex objects)
  3. Ensure test names remain descriptive — xUnit appends parameter values to the display name automatically
  4. Do not parameterize tests where variation in setup or assertion logic makes a shared structure artificial
```

```markdown
### Expected behavior
WHEN the quiz in the typing mode
AND the question has more than one correct answer
AND the user has typed the correct answer
THEN the quiz responds that result is correct

### Actual behavior
the quiz responds that result is incorrect

### Steps to reproduce
1. Start the quiz
2. Wait for the question which has more than one correct answer (use test or "rig" the app to ask you a specific question) 
3. Type one of correct answers and press Enter
```

