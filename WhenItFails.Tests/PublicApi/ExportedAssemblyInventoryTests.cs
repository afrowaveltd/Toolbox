using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit.Abstractions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ExportedAssemblyInventoryTests(ITestOutputHelper output)
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.DeclaredOnly;

    [Fact]
    public void CompiledAssembly_EnumeratesExportedTypesAndPublicMembers()
    {
        Assembly assembly = typeof(IErrorCatalogRuntime).Assembly;
        Type[] types = assembly.GetExportedTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(types);
        Assert.Contains(typeof(IErrorCatalogRuntime), types);
        Assert.Contains(
            typeof(Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions),
            types);
        Assert.All(types, type => Assert.True(type.IsVisible));

        string report = BuildReport(assembly, types);
        string? requestedPath = Environment.GetEnvironmentVariable(
            "AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT");

        if (!string.IsNullOrWhiteSpace(requestedPath))
        {
            string path = Path.GetFullPath(requestedPath);
            string? directory = Path.GetDirectoryName(path);
            Assert.True(directory is not null && Directory.Exists(directory),
                "The report output directory must already exist.");

            File.WriteAllText(path, report,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            output.WriteLine("Report: " + path);
        }

        output.WriteLine("Exported types: " + types.Length);
        output.WriteLine("Interfaces: " + types.Count(type => type.IsInterface));
        output.WriteLine("Enums: " + types.Count(type => type.IsEnum));
        output.WriteLine("Classes: " + types.Count(type => type.IsClass &&
            !typeof(Delegate).IsAssignableFrom(type)));
    }

    [Fact]
    public void CurrentAssembly_ExportsAllNewOptionalObservationAndSnapshotTypes()
    {
        Type[] exported = typeof(IErrorCatalogRuntime).Assembly.GetExportedTypes();

        foreach (Type type in new[]
                 {
                     typeof(IErrorCatalogRuntimePublicationReader),
                     typeof(IErrorCatalogRuntimeActivationReader),
                     typeof(IErrorCatalogRuntimeCombinedObservationReader),
                     typeof(IErrorCatalogRuntimeSupportingObservationReader),
                     typeof(IErrorCatalogRuntimeFullObservationReader),
                     typeof(ErrorCatalogCombinedSnapshot),
                     typeof(ErrorSupportingCatalogsSnapshot),
                     typeof(ErrorCatalogPublishedSupportingCatalogsSnapshot),
                     typeof(ErrorCatalogCompletedSupportingCatalogsSnapshot),
                     typeof(ErrorCatalogFullSnapshot),
                     typeof(ErrorCatalogCompletedFullSnapshot),
                     typeof(ErrorOwnerCatalogSnapshot),
                     typeof(ErrorCodeGroupCatalogSnapshot),
                     typeof(ErrorProfileCatalogSnapshot),
                     typeof(ErrorProfileDefinitionSnapshot),
                     typeof(ErrorSupportingCatalogsSnapshotExtensions),
                     typeof(ErrorCatalogPublishedSupportingCatalogsSnapshotExtensions)
                 })
        {
            Assert.Contains(type, exported);
            Assert.True(type.IsVisible, type.FullName);
        }
    }

    [Fact]
    public void CurrentAssembly_InternalCaptureHelpersAreNotPublicApiEntries()
    {
        foreach (Type extension in new[]
                 {
                     typeof(ErrorCatalogCombinedSnapshotExtensions),
                     typeof(ErrorSupportingCatalogsSnapshotExtensions)
                 })
        {
            Assert.Contains(extension.GetMethods(
                    BindingFlags.NonPublic | BindingFlags.Static |
                    BindingFlags.DeclaredOnly),
                method => method.Name == "CaptureFromContext");

            Assert.DoesNotContain(extension.GetMethods(Declared),
                method => method.Name == "CaptureFromContext");
        }
    }

    [Fact]
    public void CurrentInventory_ListsEveryExportedTypeExactlyOnceInStableOrder()
    {
        Assembly assembly = typeof(IErrorCatalogRuntime).Assembly;
        Type[] types = assembly.GetExportedTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        string report = BuildReport(assembly, types);
        Assert.Equal(report, BuildReport(assembly, types));
        Assert.Contains("Exported types: " + types.Length, report);

        int previous = -1;
        foreach (Type type in types)
        {
            string heading = "## " + TypeName(type) + Environment.NewLine;
            int offset = report.IndexOf(heading, StringComparison.Ordinal);
            Assert.True(offset > previous,
                "Missing or out-of-order type heading: " + TypeName(type));
            Assert.Equal(offset, report.LastIndexOf(heading, StringComparison.Ordinal));
            previous = offset;
        }

        Assert.Contains("## " + TypeName(typeof(ErrorCatalogFullSnapshot)) +
            Environment.NewLine, report);
        Assert.Contains("## " +
            TypeName(typeof(IErrorCatalogRuntimeFullObservationReader)) +
            Environment.NewLine, report);
    }

    private static string BuildReport(Assembly assembly, Type[] types)
    {
        StringBuilder text = new();
        text.AppendLine("# WhenItFails compiled public API inventory");
        text.AppendLine();
        text.AppendLine("Assembly: " + assembly.GetName().Name);
        text.AppendLine("Version: " + assembly.GetName().Version);
        text.AppendLine("Exported types: " + types.Length);
        text.AppendLine();
        text.AppendLine("Based on compiled Assembly.GetExportedTypes().");
        text.AppendLine("Members listed under each type are declared, not inherited.");
        text.AppendLine();

        foreach (Type type in types)
        {
            text.AppendLine("## " + TypeName(type));
            text.AppendLine();
            text.AppendLine("Kind: " +
                (type.IsInterface ? "interface" : type.IsEnum ? "enum" :
                 type.IsValueType ? "struct" :
                 typeof(Delegate).IsAssignableFrom(type) ? "delegate" :
                 type.IsAbstract && type.IsSealed ? "static class" :
                 type.IsAbstract ? "abstract class" :
                 type.IsSealed ? "sealed class" : "class"));

            if (type.BaseType is { } parent && parent != typeof(object))
                text.AppendLine("Base: " + TypeName(parent));

            Type[] interfaces = type.GetInterfaces()
                .OrderBy(TypeName, StringComparer.Ordinal).ToArray();
            if (interfaces.Length > 0)
                text.AppendLine("Interfaces: " +
                    string.Join(", ", interfaces.Select(TypeName)));

            text.AppendLine();
            text.AppendLine("~~~text");

            if (type.IsEnum)
            {
                text.AppendLine("underlying: " +
                    TypeName(Enum.GetUnderlyingType(type)));
                foreach (FieldInfo field in type.GetFields(
                             BindingFlags.Public | BindingFlags.Static |
                             BindingFlags.DeclaredOnly)
                         .OrderBy(field => field.Name, StringComparer.Ordinal))
                    text.AppendLine(field.Name + " = " +
                        Convert.ToString(field.GetRawConstantValue(),
                            CultureInfo.InvariantCulture));
            }
            else
            {
                IEnumerable<string> signatures =
                    type.GetConstructors(Declared).Select(
                        ctor => "ctor " + TypeName(type) +
                            "(" + Arguments(ctor.GetParameters()) + ")")
                    .Concat(type.GetFields(Declared).Select(
                        field => "field " +
                            (field.IsLiteral ? "const " :
                             field.IsStatic ? "static " : "") +
                            TypeName(field.FieldType) + " " + field.Name +
                            (field.IsLiteral ? " = " + Convert.ToString(
                                field.GetRawConstantValue(),
                                CultureInfo.InvariantCulture) : "")))
                    .Concat(type.GetProperties(Declared).Select(property =>
                    {
                        MethodInfo? setter = property.SetMethod;
                        string accessors =
                            (property.GetMethod?.IsPublic == true ? "get; " : "") +
                            (setter?.IsPublic == true
                                ? setter.ReturnParameter.GetRequiredCustomModifiers()
                                    .Contains(typeof(IsExternalInit))
                                    ? "init;" : "set;" : "");
                        return "property " + TypeName(property.PropertyType) +
                            " " + property.Name +
                            (property.GetIndexParameters().Length == 0 ? "" :
                                "[" + Arguments(property.GetIndexParameters()) + "]") +
                            " { " + accessors.Trim() + " }";
                    }))
                    .Concat(type.GetEvents(Declared).Select(
                        item => "event " + TypeName(item.EventHandlerType!) +
                            " " + item.Name))
                    .Concat(type.GetMethods(Declared)
                        .Where(method => !method.IsSpecialName ||
                            method.Name.StartsWith("op_", StringComparison.Ordinal))
                        .Select(method => "method " +
                            (method.IsStatic ? "static " : "") +
                            TypeName(method.ReturnType) + " " + method.Name +
                            (method.IsGenericMethodDefinition
                                ? "<" + string.Join(", ",
                                    method.GetGenericArguments().Select(a => a.Name)) + ">"
                                : "") +
                            "(" + Arguments(method.GetParameters()) + ")" +
                            (method.IsDefined(typeof(ExtensionAttribute), false)
                                ? " [extension]" : "")));

                foreach (string signature in signatures.OrderBy(
                             value => value, StringComparer.Ordinal))
                    text.AppendLine(signature);
            }

            text.AppendLine("~~~");
            text.AppendLine();
        }

        return text.ToString();
    }

    private static string Arguments(ParameterInfo[] parameters) =>
        string.Join(", ", parameters.Select(parameter =>
        {
            Type type = parameter.ParameterType;
            string modifier = parameter.IsOut ? "out " :
                type.IsByRef ? "ref " : "";
            if (type.IsByRef) type = type.GetElementType()!;

            string optional = parameter.IsOptional
                ? " = " + (parameter.HasDefaultValue
                    ? FormatDefault(parameter.DefaultValue, type) : "default")
                : "";

            return modifier + TypeName(type) + " " + parameter.Name + optional;
        }));

    private static string FormatDefault(object? value, Type type)
    {
        if (value is null || value == Missing.Value || value == DBNull.Value)
            return type.IsValueType ? "default(" + TypeName(type) + ")" : "null";

        return value switch
        {
            string s => System.Text.Json.JsonSerializer.Serialize(s),
            bool b => b ? "true" : "false",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default"
        };
    }

    private static string TypeName(Type type)
    {
        if (type.IsByRef || type.IsPointer)
            return TypeName(type.GetElementType()!) +
                (type.IsPointer ? "*" : "&");

        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return TypeName(underlying) + "?";

        if (type.IsArray)
            return TypeName(type.GetElementType()!) +
                "[" + new string(',', type.GetArrayRank() - 1) + "]";

        if (type.IsGenericParameter)
            return type.Name;

        if (type.IsGenericType)
        {
            string name = type.GetGenericTypeDefinition().FullName!
                .Replace('+', '.');
            int tick = name.IndexOf((char)96);
            if (tick >= 0) name = name[..tick];

            return name + "<" +
                string.Join(", ", type.GetGenericArguments().Select(TypeName)) + ">";
        }

        return type.FullName?.Replace('+', '.') ?? type.Name;
    }
}
