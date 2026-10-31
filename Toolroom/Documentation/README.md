# Documentation link validator

A dependency-free Python 3 script checks local Markdown link file/directory targets across Toolbox README and `Docs/**/en.md` content. It is a **file-target audit**, not a full CommonMark renderer and not a validator for remote URLs, reference links or heading fragments.

```bash
python3 Toolroom/Documentation/Check-Markdown-Links.py
```

The command exits with a nonzero status when a referenced local path does not exist. It runs automatically on both push and pull request through the documentation job in GitHub Actions.

[Usage and scope](Docs/Usage/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md).
