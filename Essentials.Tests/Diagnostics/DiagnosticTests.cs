using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

/// <summary>
/// Tests for the <see cref="DiagnosticHint"/> class.
/// </summary>
public class DiagnosticHintTests
{
    [Fact]
    public void DefaultConstruction_HasExpectedDefaults()
    {
        // Act
        var hint = new DiagnosticHint();

        // Assert
        Assert.Equal(DiagnosticHintKind.Note, hint.Kind);
        Assert.Equal(string.Empty, hint.Message);
    }

    [Theory]
    [InlineData(DiagnosticHintKind.Note)]
    [InlineData(DiagnosticHintKind.Help)]
    [InlineData(DiagnosticHintKind.Suggestion)]
    [InlineData(DiagnosticHintKind.Example)]
    public void InitWithKind_SetsKind(DiagnosticHintKind kind)
    {
        // Act
        var hint = new DiagnosticHint { Kind = kind };

        // Assert
        Assert.Equal(kind, hint.Kind);
    }

    [Fact]
    public void InitWithMessage_SetsMessage()
    {
        // Act
        var hint = new DiagnosticHint { Message = "Consider using async/await" };

        // Assert
        Assert.Equal("Consider using async/await", hint.Message);
    }

    [Fact]
    public void CompleteHint_AllPropertiesSet()
    {
        // Act
        var hint = new DiagnosticHint
        {
            Kind = DiagnosticHintKind.Suggestion,
            Message = "Use 'var' for implicit typing"
        };

        // Assert
        Assert.Equal(DiagnosticHintKind.Suggestion, hint.Kind);
        Assert.Equal("Use 'var' for implicit typing", hint.Message);
    }
}

/// <summary>
/// Tests for the <see cref="DiagnosticLocation"/> class.
/// </summary>
public class DiagnosticLocationTests
{
    [Fact]
    public void DefaultConstruction_AllPropertiesNull()
    {
        // Act
        var location = new DiagnosticLocation();

        // Assert
        Assert.Null(location.Source);
        Assert.Null(location.Line);
        Assert.Null(location.Column);
        Assert.Null(location.Offset);
    }

    [Fact]
    public void InitWithSource_SetsSource()
    {
        // Act
        var location = new DiagnosticLocation { Source = "Program.cs" };

        // Assert
        Assert.Equal("Program.cs", location.Source);
    }

    [Fact]
    public void InitWithLine_SetsLine()
    {
        // Act
        var location = new DiagnosticLocation { Line = 42 };

        // Assert
        Assert.Equal(42, location.Line);
    }

    [Fact]
    public void InitWithColumn_SetsColumn()
    {
        // Act
        var location = new DiagnosticLocation { Column = 15 };

        // Assert
        Assert.Equal(15, location.Column);
    }

    [Fact]
    public void InitWithOffset_SetsOffset()
    {
        // Act
        var location = new DiagnosticLocation { Offset = 1234L };

        // Assert
        Assert.Equal(1234L, location.Offset);
    }

    [Fact]
    public void CompleteLocation_AllPropertiesSet()
    {
        // Act
        var location = new DiagnosticLocation
        {
            Source = "src/Services/UserService.cs",
            Line = 85,
            Column = 12,
            Offset = 2456L
        };

        // Assert
        Assert.Equal("src/Services/UserService.cs", location.Source);
        Assert.Equal(85, location.Line);
        Assert.Equal(12, location.Column);
        Assert.Equal(2456L, location.Offset);
    }
}

/// <summary>
/// Tests for the <see cref="DiagnosticSpan"/> class.
/// </summary>
public class DiagnosticSpanTests
{
    [Fact]
    public void DefaultConstruction_HasExpectedDefaults()
    {
        // Act
        var span = new DiagnosticSpan();

        // Assert
        Assert.NotNull(span.Start);
        Assert.Null(span.End);
        Assert.Null(span.Label);
    }

    [Fact]
    public void InitWithStart_SetsStart()
    {
        // Arrange
        var start = new DiagnosticLocation { Line = 10, Column = 5 };

        // Act
        var span = new DiagnosticSpan { Start = start };

        // Assert
        Assert.Same(start, span.Start);
        Assert.Equal(10, span.Start.Line);
    }

    [Fact]
    public void InitWithEnd_SetsEnd()
    {
        // Arrange
        var end = new DiagnosticLocation { Line = 15, Column = 20 };

        // Act
        var span = new DiagnosticSpan { End = end };

        // Assert
        Assert.Same(end, span.End);
        Assert.Equal(15, span.End.Line);
    }

    [Fact]
    public void InitWithLabel_SetsLabel()
    {
        // Act
        var span = new DiagnosticSpan { Label = "problematic code" };

        // Assert
        Assert.Equal("problematic code", span.Label);
    }

    [Fact]
    public void CompleteSpan_AllPropertiesSet()
    {
        // Arrange
        var start = new DiagnosticLocation { Source = "test.cs", Line = 10, Column = 5 };
        var end = new DiagnosticLocation { Source = "test.cs", Line = 10, Column = 25 };

        // Act
        var span = new DiagnosticSpan
        {
            Start = start,
            End = end,
            Label = "variable declaration"
        };

        // Assert
        Assert.Equal(10, span.Start.Line);
        Assert.Equal(5, span.Start.Column);
        Assert.Equal(10, span.End!.Line);
        Assert.Equal(25, span.End.Column);
        Assert.Equal("variable declaration", span.Label);
    }
}

/// <summary>
/// Tests for the <see cref="DiagnosticInfo"/> class.
/// </summary>
public class DiagnosticInfoTests
{
    [Fact]
    public void DefaultConstruction_HasExpectedDefaults()
    {
        // Act
        var diagnostic = new DiagnosticInfo();

        // Assert
        Assert.Equal(string.Empty, diagnostic.Code);
        Assert.Equal(string.Empty, diagnostic.Message);
        Assert.Null(diagnostic.Details);
        Assert.Equal(IssueSeverity.Information, diagnostic.Severity);
        Assert.Null(diagnostic.Location);
        Assert.NotNull(diagnostic.Spans);
        Assert.Empty(diagnostic.Spans);
        Assert.NotNull(diagnostic.Hints);
        Assert.Empty(diagnostic.Hints);
        Assert.NotNull(diagnostic.Metadata);
        Assert.True(diagnostic.Metadata.IsEmpty);
    }

    [Fact]
    public void InitWithCode_SetsCode()
    {
        // Act
        var diagnostic = new DiagnosticInfo { Code = "CS0246" };

        // Assert
        Assert.Equal("CS0246", diagnostic.Code);
    }

    [Fact]
    public void InitWithMessage_SetsMessage()
    {
        // Act
        var diagnostic = new DiagnosticInfo { Message = "Type or namespace not found" };

        // Assert
        Assert.Equal("Type or namespace not found", diagnostic.Message);
    }

    [Fact]
    public void InitWithDetails_SetsDetails()
    {
        // Act
        var diagnostic = new DiagnosticInfo { Details = "Check using directives and references" };

        // Assert
        Assert.Equal("Check using directives and references", diagnostic.Details);
    }

    [Theory]
    [InlineData(IssueSeverity.Information)]
    [InlineData(IssueSeverity.Warning)]
    [InlineData(IssueSeverity.Error)]
    public void InitWithSeverity_SetsSeverity(IssueSeverity severity)
    {
        // Act
        var diagnostic = new DiagnosticInfo { Severity = severity };

        // Assert
        Assert.Equal(severity, diagnostic.Severity);
    }

    [Fact]
    public void InitWithLocation_SetsLocation()
    {
        // Arrange
        var location = new DiagnosticLocation { Source = "Program.cs", Line = 42 };

        // Act
        var diagnostic = new DiagnosticInfo { Location = location };

        // Assert
        Assert.Same(location, diagnostic.Location);
        Assert.Equal("Program.cs", diagnostic.Location.Source);
    }

    [Fact]
    public void InitWithSpans_SetsSpans()
    {
        // Arrange
        var spans = new List<DiagnosticSpan>
        {
            new() { Label = "span1" },
            new() { Label = "span2" }
        };

        // Act
        var diagnostic = new DiagnosticInfo { Spans = spans };

        // Assert
        Assert.Equal(2, diagnostic.Spans.Count);
        Assert.Equal("span1", diagnostic.Spans[0].Label);
        Assert.Equal("span2", diagnostic.Spans[1].Label);
    }

    [Fact]
    public void InitWithHints_SetsHints()
    {
        // Arrange
        var hints = new List<DiagnosticHint>
        {
            new() { Kind = DiagnosticHintKind.Help, Message = "Add using directive" },
            new() { Kind = DiagnosticHintKind.Example, Message = "using System.Linq;" }
        };

        // Act
        var diagnostic = new DiagnosticInfo { Hints = hints };

        // Assert
        Assert.Equal(2, diagnostic.Hints.Count);
        Assert.Equal(DiagnosticHintKind.Help, diagnostic.Hints[0].Kind);
        Assert.Equal(DiagnosticHintKind.Example, diagnostic.Hints[1].Kind);
    }

    [Fact]
    public void InitWithMetadata_SetsMetadata()
    {
        // Arrange
        var metadata = new MetadataBag();
        metadata["ruleId"] = "CA1001";

        // Act
        var diagnostic = new DiagnosticInfo { Metadata = metadata };

        // Assert
        Assert.Same(metadata, diagnostic.Metadata);
        Assert.Equal("CA1001", diagnostic.Metadata["ruleId"]);
    }

    [Fact]
    public void CompleteDiagnostic_AllPropertiesSet()
    {
        // Arrange
        var location = new DiagnosticLocation { Source = "Service.cs", Line = 100, Column = 15 };
        var spans = new List<DiagnosticSpan>
        {
            new() { Start = new DiagnosticLocation { Line = 100, Column = 15 }, Label = "variable" }
        };
        var hints = new List<DiagnosticHint>
        {
            new() { Kind = DiagnosticHintKind.Suggestion, Message = "Use meaningful name" }
        };
        var metadata = new MetadataBag();
        metadata["category"] = "naming";

        // Act
        var diagnostic = new DiagnosticInfo
        {
            Code = "IDE0001",
            Message = "Name does not match convention",
            Details = "Variable names should use camelCase",
            Severity = IssueSeverity.Warning,
            Location = location,
            Spans = spans,
            Hints = hints,
            Metadata = metadata
        };

        // Assert
        Assert.Equal("IDE0001", diagnostic.Code);
        Assert.Equal("Name does not match convention", diagnostic.Message);
        Assert.Equal("Variable names should use camelCase", diagnostic.Details);
        Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Service.cs", diagnostic.Location!.Source);
        Assert.Single(diagnostic.Spans);
        Assert.Single(diagnostic.Hints);
        Assert.Equal("naming", diagnostic.Metadata["category"]);
    }

    [Fact]
    public void ImplementsIHasCode()
    {
        // Arrange
        var diagnostic = new DiagnosticInfo { Code = "TEST001" };

        // Act
        var hasCode = diagnostic as Afrowave.Toolbox.Essentials.Interfaces.IHasCode;

        // Assert
        Assert.NotNull(hasCode);
        Assert.Equal("TEST001", hasCode.Code);
    }

    [Fact]
    public void ImplementsIHasSeverity()
    {
        // Arrange
        var diagnostic = new DiagnosticInfo { Severity = IssueSeverity.Error };

        // Act
        var hasSeverity = diagnostic as Afrowave.Toolbox.Essentials.Interfaces.IHasSeverity;

        // Assert
        Assert.NotNull(hasSeverity);
        Assert.Equal(IssueSeverity.Error, hasSeverity.Severity);
    }

    [Fact]
    public void ImplementsIHasMetadata()
    {
        // Arrange
        var metadata = new MetadataBag();
        metadata["key"] = "value";
        var diagnostic = new DiagnosticInfo { Metadata = metadata };

        // Act
        var hasMetadata = diagnostic as Afrowave.Toolbox.Essentials.Interfaces.IHasMetadata;

        // Assert
        Assert.NotNull(hasMetadata);
        Assert.Same(metadata, hasMetadata.Metadata);
    }
}
