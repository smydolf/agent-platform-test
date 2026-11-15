---
name: file-header
description: Repo check of agent-platform-test (a test of the reviewer, smydolf/agent-platform#32). Every C# file that a change adds or edits starts with the header line of the orders team.
---

# Check: file header

Rule of this repo: every `.cs` file that the change adds or edits starts with this line as its first line:

```
// Orders team. See README.md.
```

For each `.cs` file in the diff (added or edited), read its first line at the branch tip. When the line is missing or different, report a `blocker` on line 1 of that file, with the title "The file has no orders team header".
Files that the change does not touch are out of scope.
