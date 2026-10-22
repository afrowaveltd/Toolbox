# Blackboard — Spectre.Console adapter

**Status:** future component grouping; no Spectre.Console dependency or implementation has yet been added.

This folder will contain terminal widgets rendered through Spectre.Console. It should not include the overall UnderstandMe settings application or expose translation-provider internals.

Initial planned component: [FileBrowser](FileBrowser/README.md).

Other candidates, as actual reuse appears: editable field groups, selection menus, status/footer shortcuts, validation-error navigation, and confirmation dialogs. Favor shared display conventions and independently injectable localized labels.

See [Blackboard's architecture](../Docs/Architecture/en.md).
