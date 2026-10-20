using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class LocalizationPlaceholderValidatorTests
{
    [Theory]
    [InlineData("Hello {name}, you have {count} messages", "Máte {count} zpráv, {name}", true)]
    [InlineData("{name} + {name} = {count}", "{count} od {name} a {name}", true)]
    [InlineData("{name} and {name}", "{name}", false)]
    [InlineData("{name}", "{name} {name}", false)]
    [InlineData("{name} {count}", "{name} {amount}", false)]
    [InlineData("{name}", "{Name}", false)]
    [InlineData("{name}", "{{name}}", false)]
    [InlineData("{{name}} {name}", "{{literal}} {name}", true)]
    [InlineData("{{{name}}}", "{{{name}}}", true)]
    [InlineData("Plain text", "Jiný prostý text", true)]
    [InlineData("Plain text", "Překlad s {added}", false)]
    [InlineData("{user_name1}", "{user_name1}", true)]
    [InlineData("{user_name1}", "{user-name1}", false)]
    [InlineData("{name}", "{name", false)]
    [InlineData("{name}", "{name}}", false)]
    [InlineData("{name}", "{na{me}", false)]
    [InlineData("{name}", "{ }", false)]
    [InlineData("{name", "{name", false)]
    public void Candidates_preserve_named_placeholder_multiset_and_valid_syntax(
        string source, string candidate, bool expected)
    {
        Assert.Equal(expected, LocalizationPlaceholderValidator.HasMatchingPlaceholders(source, candidate));
    }

    [Theory]
    [InlineData("Hello {name}", true)]
    [InlineData("Hello {{name}}", true)]
    [InlineData("I have {broken", false)]
    [InlineData("{name}}", false)]
    public void Source_placeholder_syntax_is_checked_separately(string template, bool expected)
    {
        Assert.Equal(expected, LocalizationPlaceholderValidator.HasValidPlaceholderSyntax(template));
    }

    [Fact]
    public void Placeholder_validation_requires_non_null_templates()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LocalizationPlaceholderValidator.HasMatchingPlaceholders(null!, "candidate"));
        Assert.Throws<ArgumentNullException>(() =>
            LocalizationPlaceholderValidator.HasMatchingPlaceholders("source", null!));
    }
}
