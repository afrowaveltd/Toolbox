using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Default implementation that resolves runtime error descriptors from catalog definitions.
/// </summary>
public sealed class ErrorDescriptorResolver : IErrorDescriptorResolver
{
    private readonly IErrorDefinitionResolver _definitionResolver;
    private readonly IErrorDescriptorFactory _descriptorFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDescriptorResolver"/> class.
    /// </summary>
    /// <param name="definitionResolver">Error definition resolver.</param>
    /// <param name="descriptorFactory">Error descriptor factory.</param>
    public ErrorDescriptorResolver(
        IErrorDefinitionResolver definitionResolver,
        IErrorDescriptorFactory descriptorFactory)
    {
        _definitionResolver = definitionResolver
            ?? throw new ArgumentNullException(nameof(definitionResolver));
        _descriptorFactory = descriptorFactory
            ?? throw new ArgumentNullException(nameof(descriptorFactory));
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> CreateById(
        ErrorCatalogContext? context,
        string errorId)
    {
        Response<ErrorDefinition> definitionResponse =
            _definitionResolver.FindById(context, errorId);

        return CreateDescriptorResponse(definitionResponse);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> CreateByName(
        ErrorCatalogContext? context,
        string errorName)
    {
        Response<ErrorDefinition> definitionResponse =
            _definitionResolver.FindByName(context, errorName);

        return CreateDescriptorResponse(definitionResponse);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> CreateByCode(
        ErrorCatalogContext? context,
        int code)
    {
        Response<ErrorDefinition> definitionResponse =
            _definitionResolver.FindByCode(context, code);

        return CreateDescriptorResponse(definitionResponse);
    }

    private Response<ErrorDescriptor> CreateDescriptorResponse(
        Response<ErrorDefinition> definitionResponse)
    {
        if (!definitionResponse.IsSuccess)
        {
            return Response<ErrorDescriptor>.WithStatus(
                Response<ErrorDescriptor>.Fail(
                    code: GetFirstIssueCode(
                        definitionResponse,
                        "ErrorDefinitionResolveFailed"),
                    message: GetResponseMessage(
                        definitionResponse,
                        "Error definition resolving failed.")),
                definitionResponse.Status);
        }

        if (definitionResponse.Data is null)
        {
            return Response<ErrorDescriptor>.Invalid(
                code: "ResolvedErrorDefinitionIsNull",
                message: "Error definition resolver returned success, but definition is null.");
        }

        ErrorDescriptor descriptor =
            _descriptorFactory.Create(definitionResponse.Data);

        return Response<ErrorDescriptor>.Ok(descriptor);
    }

    private static string GetFirstIssueCode(
        Response<ErrorDefinition> response,
        string fallbackCode)
    {
        return response.Issues.Count > 0
            ? response.Issues[0].Code
            : fallbackCode;
    }

    private static string GetResponseMessage(
        Response<ErrorDefinition> response,
        string fallbackMessage)
    {
        return string.IsNullOrWhiteSpace(response.Message)
            ? fallbackMessage
            : response.Message;
    }
}