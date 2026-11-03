using System.Text;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownPagingProfilesTests
{
    [Theory]
    [InlineData(MarkdownPagingProfile.Balanced, 40, 16)]
    [InlineData(MarkdownPagingProfile.Hdd, 64, 1)]
    [InlineData(MarkdownPagingProfile.Ssd, 64, 32)]
    [InlineData(MarkdownPagingProfile.LowRam, 8, 16)]
    public void Presets_have_explicit_validated_page_and_checkpoint_policies(
        MarkdownPagingProfile profile, int linesPerPage, int stride)
    {
        var settings = MarkdownPagingProfiles.For(profile);
        Assert.Equal(profile, settings.Profile);
        Assert.Equal(linesPerPage, settings.LinesPerPage);
        Assert.Equal(stride, settings.CheckpointStride);
        Assert.Equal(64 * 1024, settings.MaxLineBytes);
        Assert.Equal(256L * 1024 * 1024, settings.MaxSourceIndexBytes);
        Assert.Equal(256L * 1024 * 1024, settings.MaxCheckpointIndexBytes);
        Assert.Equal(linesPerPage, settings.CreateSourceOptions().LinesPerPage);
        settings.Validate();
    }

    [Fact]
    public void Caller_can_override_one_knob_without_modifying_preset()
    {
        var original = MarkdownPagingProfiles.For(MarkdownPagingProfile.Hdd);
        var custom = original with
        {
            LinesPerPage = 32,
            CheckpointStride = 4,
            MaxCheckpointIndexBytes = 1024
        };

        custom.Validate();
        Assert.Equal(32, custom.CreateSourceOptions().LinesPerPage);
        Assert.Equal(4, custom.CheckpointStride);
        Assert.Equal(1024, custom.MaxCheckpointIndexBytes);
        Assert.Equal(64, original.LinesPerPage);
        Assert.Equal(1, original.CheckpointStride);
        Assert.Equal(64, MarkdownPagingProfiles.For(MarkdownPagingProfile.Hdd).LinesPerPage);
    }

    [Theory]
    [InlineData(0, 16)]
    [InlineData(129, 16)]
    [InlineData(40, 0)]
    [InlineData(40, 257)]
    public void Invalid_page_or_checkpoint_limits_are_rejected(int lines, int stride)
    {
        var settings = MarkdownPagingProfiles.For(MarkdownPagingProfile.Balanced)
            with { LinesPerPage = lines, CheckpointStride = stride };
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Fact]
    public void Invalid_disk_quotas_and_unknown_profile_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MarkdownPagingProfiles.For((MarkdownPagingProfile)987));
        var normal = MarkdownPagingProfiles.For(MarkdownPagingProfile.Balanced);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (normal with { MaxCheckpointIndexBytes = 511 }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (normal with { MaxSourceIndexBytes = 7 }).Validate());
    }

    [Theory]
    [InlineData(MarkdownPagingProfile.Balanced)]
    [InlineData(MarkdownPagingProfile.Hdd)]
    [InlineData(MarkdownPagingProfile.Ssd)]
    [InlineData(MarkdownPagingProfile.LowRam)]
    public async Task All_profiles_recover_fence_context_when_opening_an_arbitrary_page(
        MarkdownPagingProfile profile)
    {
        var markdown = "~~~md\n" +
            string.Concat(Enumerable.Repeat("- inside code\n", 130)) +
            "~~~\n" +
            "- outside\n" +
            "  continuation\n";
        var root = Path.Combine(Path.GetTempPath(),
            "afrowave-profile-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "source.md");
        await File.WriteAllTextAsync(path, markdown, new UTF8Encoding(false));
        try
        {
            var settings = MarkdownPagingProfiles.For(profile);
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                settings.CreateSourceOptions(), tempDirectory: root);
            await using var checkpoints = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: root,
                maxIndexBytes: settings.MaxCheckpointIndexBytes,
                checkpointStride: settings.CheckpointStride);

            Assert.Equal(settings.CheckpointStride, checkpoints.CheckpointStride);
            var last = await checkpoints.ReadPageAsync(checkpoints.PageCount - 1);
            var middle = await checkpoints.ReadPageAsync(1);
            var first = await checkpoints.ReadPageAsync(0);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceOpen, first.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeContent, middle.Lines[0].Kind);
            Assert.Contains(last.Lines, x => x.Content == "  continuation");
            Assert.Equal(markdown, await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
