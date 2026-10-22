# Blackboard — Spectre.Console adapter

**Status:** technology-oriented folder only; no Spectre.Console dependency or runtime package has yet been added.

This folder groups **independent component projects** rendered through Spectre.Console; it is not itself an aggregate project or required NuGet package. For example, the future `FileBrowser` project may depend on Spectre.Console while an application can install **only that one component**. It should not include the overall UnderstandMe settings application or expose translation-provider internals.

Initial planned component: [FileBrowser](FileBrowser/README.md).

Other candidates, as actual reuse appears: separately packageable editable field groups, selection menus, status/footer shortcuts, validation-error navigation, and confirmation dialogs. Do not create a project or dependency solely to fill a category. Favor shared display conventions and independently injectable localized labels.

See [Blackboard's architecture](../Docs/Architecture/en.md).
