using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that resolves error definitions from a loaded catalog context.
/// </summary>
public sealed class ErrorDefinitionResolver : IErrorDefinitionResolver
{
    /// <inheritdoc />
    public Response<ErrorDefinition> FindById(
        ErrorCatalogContext? context,
        string errorId)
    {
        Response<ErrorDefinition>? validationResponse =
            ValidateContextAndTextValue(
                context,
                errorId,
                "ErrorIdIsEmpty",
                "Error id is empty.");

        if (validationResponse is not null)
        {
            return validationResponse;
        }

        string normalizedErrorId = TextKeyNormalizer.NormalizeKey(errorId);

        ErrorDefinition? errorDefinition =
            context!.ErrorCatalog.FindById(normalizedErrorId);

        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFoundById",
                message: $"Error definition with id '{errorId}' was not found.");
        }

        return Response<ErrorDefinition>.Ok(errorDefinition);
    }

    /// <inheritdoc />
    public Response<ErrorDefinition> FindByName(
        ErrorCatalogContext? context,
        string errorName)
    {
        Response<ErrorDefinition>? validationResponse =
            ValidateContextAndTextValue(
                context,
                errorName,
                "ErrorNameIsEmpty",
                "Error name is empty.");

        if (validationResponse is not null)
        {
            return validationResponse;
        }

        string normalizedErrorName = TextKeyNormalizer.NormalizeKey(errorName);

        ErrorDefinition? errorDefinition =
            context!.ErrorCatalog.FindByName(normalizedErrorName);

        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFoundByName",
                message: $"Error definition with name '{errorName}' was not found.");
        }

        return Response<ErrorDefinition>.Ok(errorDefinition);
    }

    /// <inheritdoc />
    public Response<ErrorDefinition> FindByCode(
        ErrorCatalogContext? context,
        int code)
    {
        if (context is null)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogContextIsNull",
                message: "Error catalog context is null.");
        }

        if (context.ErrorCatalog is null)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogIsNull",
                message: "Error catalog context does not contain an error catalog.");
        }

        if (code <= 0)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCodeIsInvalid",
                message: "Error code must be greater than zero.");
        }

        ErrorDefinition? errorDefinition =
            context.ErrorCatalog.FindByCode(code);

        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFoundByCode",
                message: $"Error definition with code '{code}' was not found.");
        }

        return Response<ErrorDefinition>.Ok(errorDefinition);
    }

    private static Response<ErrorDefinition>? ValidateContextAndTextValue(
        ErrorCatalogContext? context,
        string value,
        string emptyValueCode,
        string emptyValueMessage)
    {
        if (context is null)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogContextIsNull",
                message: "Error catalog context is null.");
        }

        if (context.ErrorCatalog is null)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogIsNull",
                message: "Error catalog context does not contain an error catalog.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Response<ErrorDefinition>.Invalid(
                code: emptyValueCode,
                message: emptyValueMessage);
        }

        return null;
    }
}