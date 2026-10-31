# Setter.Tests — Implementation status

## 2026-10-31 — documentation baseline

- Confirmed existing `Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.csproj` with xUnit command/output/regression cases.
- Added English README, topic documentation and independent continuation status; no test behavior changed in this documentation increment.
- Check [Setter implementation status](../Setter/IMPLEMENTATION_STATUS.md) for exact functional milestones and follow-up tasks.

## 2026-10-31 — README mirror preserved during link audit

- The existing test `SetterReadmeTests.EnglishReadmeSource_MatchesRootReadme` requires exact equality of the two Setter English README copies.
- Changed both files to identical canonical GitHub topic links rather than different relative paths. This maintains working links at both locations and preserves the test contract without modifying test or runtime code. CI verification pending.
