using System.Globalization;
using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Metadata-driven provider and storage registration screens.</summary>
public sealed partial class ConfiguratorApp
{
    private sealed record ComponentMenuChoice(string Label, string? Id, bool IsNew = false);
    private sealed record OptionMenuChoice(string Label, SettingsFieldDescriptor? Field);

    private async Task EditComponentsAsync(
        ConfiguratorSession session, SettingsComponentKind kind, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            Write(T(kind == SettingsComponentKind.TranslationBackend
                ? "component.providers" : "component.storages") + "\n");

            var entries = session.ListComponents(kind)
                .Select(x => new ComponentMenuChoice(x.Id + " (" + x.Type + ")", x.Id))
                .ToList();
            entries.Add(new(T("component.add"), null, true));
            entries.Add(new(T("menu.back"), null));
            var selected = await new SelectionPrompt<ComponentMenuChoice>()
                .Title(Markup.Escape(T("component.pick")))
                .PageSize(Math.Clamp(entries.Count, 3, 15))
                .UseConverter(x => Markup.Escape(x.Label))
                .AddChoices(entries)
                .AddCancelResult(entries[^1])
                .ShowAsync(_console, cancellationToken);

            if (selected.IsNew)
            {
                await AddComponentAsync(session, kind, cancellationToken);
                continue;
            }
            if (selected.Id is null)
                return;
            await EditComponentAsync(session, kind, selected.Id, cancellationToken);
        }
    }

    private async Task AddComponentAsync(
        ConfiguratorSession session, SettingsComponentKind kind, CancellationToken cancellationToken)
    {
        var schemas = ConfiguratorSession.GetAvailableTypes(kind);
        if (schemas.Count == 0)
        {
            Write(T("component.none") + "\n");
            await WaitForReturnAsync(cancellationToken);
            return;
        }
        var available = schemas.Cast<SettingsComponentDescriptor?>().Append(null).ToArray();
        var schema = await new SelectionPrompt<SettingsComponentDescriptor?>()
            .Title(Markup.Escape(T("component.type")))
            .PageSize(Math.Clamp(available.Length, 3, 15))
            .UseConverter(x => x is null ? Markup.Escape(T("menu.back")) : Markup.Escape(x.DisplayName))
            .AddChoices(available)
            .AddCancelResult(null)
            .ShowAsync(_console, cancellationToken);
        if (schema is null)
            return;

        var id = await new TextPrompt<string>(Markup.Escape(T("component.id")))
            .ShowAsync(_console, cancellationToken);
        try
        {
            session.AddComponent(kind, id, schema.Type);
            await EditComponentAsync(session, kind, id.Trim(), cancellationToken);
        }
        catch (ArgumentException ex)
        {
            Write(T("component.invalid") + ex.Message + "\n");
            await WaitForReturnAsync(cancellationToken);
        }
    }

    private async Task EditComponentAsync(
        ConfiguratorSession session, SettingsComponentKind kind, string id, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var component = session.GetComponent(kind, id);
            var schema = ConfiguratorSession.FindDescriptor(kind, component.Type);
            if (schema is null)
            {
                Write(T("component.none") + "\n");
                await WaitForReturnAsync(cancellationToken);
                return;
            }
            _console.Clear();
            Write(id + " (" + schema.DisplayName + ")\n");

            var commands = schema.Fields
                .Select(x => new OptionMenuChoice(OptionLabel(component.Options, x), x))
                .ToList();
            commands.Add(new(T("component.properties"), null));
            commands.Add(new(T("component.remove"), null));
            commands.Add(new(T("menu.back"), null));

            var chosen = await new SelectionPrompt<OptionMenuChoice>()
                .Title(Markup.Escape(T("component.options")))
                .PageSize(Math.Clamp(commands.Count, 3, 16))
                .UseConverter(x => Markup.Escape(x.Label))
                .AddChoices(commands)
                .AddCancelResult(commands[^1])
                .ShowAsync(_console, cancellationToken);

            if (chosen.Field is not null)
            {
                await EditOptionAsync(session, kind, id, chosen.Field, cancellationToken);
                continue;
            }
            if (chosen == commands[^1])
                return;
            if (chosen == commands[^2])
            {
                if (await Confirm("component.confirm.remove", cancellationToken))
                {
                    session.RemoveComponent(kind, id);
                    return;
                }
                continue;
            }
            await EditComponentPropertiesAsync(session, kind, id, cancellationToken);
        }
    }

    private string OptionLabel(
        IReadOnlyDictionary<string, JsonElement> options, SettingsFieldDescriptor field)
    {
        var label = _strings["component.field." + field.Key];
        if (label == "component.field." + field.Key)
            label = field.Label;
        if (!options.TryGetValue(field.Key, out var value))
            return label + ": " + T("component.unset");
        if (field.Kind == SettingsFieldKind.SecretReference)
            return label + ": " + T("component.hidden");
        return label + ": " + (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString());
    }

    private async Task EditOptionAsync(
        ConfiguratorSession session, SettingsComponentKind kind, string id,
        SettingsFieldDescriptor field, CancellationToken cancellationToken)
    {
        // Type/limits/choices come only from the shared descriptor.
        // Secret references are never echoed or used as visible defaults.
        var current = session.GetComponent(kind, id).Options;
        _console.Clear();
        var fieldLabel = _strings["component.field." + field.Key];
        if (fieldLabel == "component.field." + field.Key)
            fieldLabel = field.Label;
        Write(fieldLabel + "\n" + field.Help + "\n");
        if (field.Kind == SettingsFieldKind.SecretReference)
            Write(T("component.secret.help") + "\n");

        var commands = new[] { T("component.change"), T("component.clear"), T("menu.back") };
        var choice = await new SelectionPrompt<string>()
            .Title(Markup.Escape(T("component.option.action")))
            .AddChoices(commands)
            .AddCancelResult(commands[^1])
            .ShowAsync(_console, cancellationToken);
        if (choice == commands[^1])
            return;

        try
        {
            if (choice == commands[1])
            {
                session.RemoveOption(kind, id, field.Key);
                return;
            }

            var value = await ReadOptionAsync(field, current.GetValueOrDefault(field.Key),
                current.ContainsKey(field.Key), cancellationToken);
            if (value.HasValue)
                session.SetOption(kind, id, field.Key, value.Value);
        }
        catch (ArgumentException ex)
        {
            Write(T("component.invalid") + ex.Message + "\n");
            await WaitForReturnAsync(cancellationToken);
        }
    }

    private async Task<JsonElement?> ReadOptionAsync(
        SettingsFieldDescriptor field, JsonElement oldValue, bool hasOld, CancellationToken cancellationToken)
    {
        if (field.Kind is SettingsFieldKind.Choice or SettingsFieldKind.Boolean)
        {
            var choices = field.Kind == SettingsFieldKind.Boolean
                ? new[] { "true", "false" }
                : field.Choices!.ToArray();
            var selected = await new SelectionPrompt<string>()
                .Title(Markup.Escape(T("component.choose")))
                .AddChoices(choices)
                .ShowAsync(_console, cancellationToken);
            return field.Kind == SettingsFieldKind.Boolean
                ? JsonSerializer.SerializeToElement(selected == "true")
                : JsonSerializer.SerializeToElement(selected);
        }

        var input = new TextPrompt<string>(Markup.Escape(T("component.value")));
        if (field.Kind == SettingsFieldKind.SecretReference)
        {
            input.Secret();
        }
        else if (hasOld)
        {
            input.DefaultValue(oldValue.ValueKind == JsonValueKind.String
                ? oldValue.GetString()! : oldValue.ToString());
        }

        var raw = await input.ShowAsync(_console, cancellationToken);
        if (field.Kind == SettingsFieldKind.Integer)
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                throw new ArgumentException(T("component.invalid.integer"));
            return JsonSerializer.SerializeToElement(number);
        }

        return JsonSerializer.SerializeToElement(raw.Trim());
    }

    private async Task EditComponentPropertiesAsync(
        ConfiguratorSession session, SettingsComponentKind kind, string id, CancellationToken cancellationToken)
    {
        if (kind == SettingsComponentKind.TranslationBackend)
        {
            var provider = session.Draft.Translation.Providers.First(x => x.Id == id);
            var next = await new SelectionPrompt<bool>()
                .Title(Markup.Escape(T("component.enabled")))
                .UseConverter(value => Markup.Escape(T(value ? "component.yes" : "component.no")))
                .AddChoices(true, false)
                .ShowAsync(_console, cancellationToken);
            session.SetProviderEnabled(id, next);
            return;
        }

        var storage = session.Draft.Localization.Storages.First(x => x.Id == id);
        var lang = await new TextPrompt<string>(Markup.Escape(T("component.source.language")))
            .DefaultValue(storage.SourceLanguage)
            .ShowAsync(_console, cancellationToken);
        var dictionaryless = await new SelectionPrompt<bool>()
            .Title(Markup.Escape(T("component.setvaluefromkey")))
            .UseConverter(value => Markup.Escape(T(value ? "component.yes" : "component.no")))
            .AddChoices(true, false)
            .ShowAsync(_console, cancellationToken);
        session.SetStorageProperties(id, lang, dictionaryless);
    }
}
