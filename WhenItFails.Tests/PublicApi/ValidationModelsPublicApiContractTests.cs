using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ValidationModelsPublicApiContractTests
{
    [Fact]
    public void ValidationResult_PreservesPublishedPropertiesAndMethodSignatures()
    {
        Type type = typeof(ErrorCatalogValidationResult);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(2, properties.Length);
        AssertGetterOnlyProperty(
            type,
            "Issues",
            typeof(IReadOnlyList<ErrorCatalogValidationIssue>));
        AssertGetterOnlyProperty(type, "IsValid", typeof(bool));

        MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        Assert.Equal(4, methods.Length);

        MethodInfo addIssue = RequireMethod(
            type,
            "AddIssue",
            typeof(void),
            typeof(ErrorCatalogValidationIssue));

        Assert.Single(addIssue.GetParameters());

        foreach (string methodName in new[]
                 {
                     "AddError",
                     "AddWarning",
                     "AddInformation"
                 })
        {
            MethodInfo method = RequireMethod(
                type,
                methodName,
                typeof(void),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string));

            ParameterInfo[] parameters = method.GetParameters();

            Assert.Equal(
                new[] { "code", "message", "errorId", "errorName", "path" },
                parameters.Select(parameter => parameter.Name).ToArray());

            Assert.All(
                parameters.Take(2),
                parameter => Assert.False(parameter.IsOptional));

            Assert.All(
                parameters.Skip(2),
                parameter =>
                {
                    Assert.True(parameter.IsOptional);
                    Assert.True(parameter.HasDefaultValue);
                    Assert.Null(parameter.DefaultValue);
                });
        }

        NullabilityInfoContext nullability = new();
        PropertyInfo issues = type.GetProperty("Issues")!;

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(issues).ReadState);
    }

    [Fact]
    public void ValidationIssue_PreservesPublishedShapeAndNullableFields()
    {
        Type type = typeof(ErrorCatalogValidationIssue);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(6, properties.Length);

        AssertGetSetProperty(type, "Severity", typeof(ErrorCatalogValidationSeverity));
        AssertGetSetProperty(type, "Code", typeof(string));
        AssertGetSetProperty(type, "Message", typeof(string));
        AssertGetSetProperty(type, "ErrorId", typeof(string));
        AssertGetSetProperty(type, "ErrorName", typeof(string));
        AssertGetSetProperty(type, "Path", typeof(string));

        NullabilityInfoContext nullability = new();

        foreach (string name in new[] { "Code", "Message" })
        {
            Assert.Equal(
                NullabilityState.NotNull,
                nullability.Create(type.GetProperty(name)!).ReadState);
        }

        foreach (string name in new[] { "ErrorId", "ErrorName", "Path" })
        {
            Assert.Equal(
                NullabilityState.Nullable,
                nullability.Create(type.GetProperty(name)!).ReadState);
        }
    }

    [Fact]
    public void ValidationSeverity_PreservesExactlyThreePublishedValues()
    {
        Type type = typeof(ErrorCatalogValidationSeverity);

        Assert.True(type.IsPublic);
        Assert.True(type.IsEnum);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(type));

        Assert.Equal(
            new[]
            {
                ErrorCatalogValidationSeverity.Information,
                ErrorCatalogValidationSeverity.Warning,
                ErrorCatalogValidationSeverity.Error
            },
            Enum.GetValues<ErrorCatalogValidationSeverity>());

        Assert.Equal(0, (int)ErrorCatalogValidationSeverity.Information);
        Assert.Equal(1, (int)ErrorCatalogValidationSeverity.Warning);
        Assert.Equal(2, (int)ErrorCatalogValidationSeverity.Error);
    }

    [Fact]
    public void ValidationResult_RecomputesValidityWhenAnExistingIssueChangesSeverity()
    {
        ErrorCatalogValidationResult result = new();
        ErrorCatalogValidationIssue issue = new()
        {
            Severity = ErrorCatalogValidationSeverity.Warning
        };

        result.AddIssue(issue);

        Assert.True(result.IsValid);
        Assert.Same(issue, Assert.Single(result.Issues));

        issue.Severity = ErrorCatalogValidationSeverity.Error;
        Assert.False(result.IsValid);

        issue.Severity = ErrorCatalogValidationSeverity.Information;
        Assert.True(result.IsValid);
    }

    private static PropertyInfo AssertGetterOnlyProperty(
        Type type,
        string name,
        Type expectedType)
    {
        PropertyInfo property = RequireProperty(type, name, expectedType);
        Assert.Null(property.SetMethod);
        return property;
    }

    private static PropertyInfo AssertGetSetProperty(
        Type type,
        string name,
        Type expectedType)
    {
        PropertyInfo property = RequireProperty(type, name, expectedType);
        Assert.True(property.SetMethod?.IsPublic == true);
        return property;
    }

    private static PropertyInfo RequireProperty(
        Type type,
        string name,
        Type expectedType)
    {
        PropertyInfo? property = type.GetProperty(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.NotNull(property);
        Assert.Equal(expectedType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);
        return property;
    }

    private static MethodInfo RequireMethod(
        Type type,
        string name,
        Type expectedReturnType,
        params Type[] parameters)
    {
        MethodInfo? method = type.GetMethod(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly,
            binder: null,
            types: parameters,
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(expectedReturnType, method.ReturnType);
        return method;
    }
}
