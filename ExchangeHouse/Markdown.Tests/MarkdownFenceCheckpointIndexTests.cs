using System.Text;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownFenceCheckpointIndexTests
{
    [Fact]
    public async Task Code_fence_opened_on_previous_page_is_preserved_after_random_seek()
    {
        const string markdown =
            "# Introduction\n\n~~~~csharp\n" +
            "# inside code\n- also code\n~~~\n" +
            "~~~~\n# Outside\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 3 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(3, fences.PageCount);

            var last = await fences.ReadPageAsync(2);    // jump to end first
            var middle = await fences.ReadPageAsync(1);
            var first = await fences.ReadPageAsync(0);
            Assert.Equal('~', middle.Checkpoint.FenceMarker);
            Assert.Equal(4, middle.Checkpoint.FenceLength);
            Assert.Equal("csharp", middle.Checkpoint.CodeLanguage);
            Assert.Equal(0, middle.Checkpoint.FenceQuoteDepth);
            Assert.All(middle.Lines, line =>
                Assert.Equal(MarkdownStreamLineKind.CodeContent, line.Kind));
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose, last.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown, last.Lines[1].Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceOpen, first.Lines[2].Kind);
            Assert.Equal(markdown,
                first.Source.Content + middle.Source.Content + last.Source.Content);

            var ordered = first.Lines.Concat(middle.Lines).Concat(last.Lines).ToArray();
            long offset = 0;
            foreach (var line in ordered)
            {
                Assert.Equal(offset, line.Start);
                offset += line.Length;
            }
            Assert.Equal(markdown.Length, offset);
        });
    }

    [Fact]
    public async Task Backtick_fence_requires_matching_length_across_page_boundaries()
    {
        const string markdown = "````js\n" +
            "```\n" +
            "# still code\n" +
            "````\n" +
            "# heading\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                Assert.Single((await fences.ReadPageAsync(1)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                Assert.Single((await fences.ReadPageAsync(2)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose,
                Assert.Single((await fences.ReadPageAsync(3)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown,
                Assert.Single((await fences.ReadPageAsync(4)).Lines).Kind);
            Assert.Equal('\0', (await fences.ReadCheckpointAsync(4)).FenceMarker);
        });
    }

    [Fact]
    public async Task Quoted_fence_is_interrupted_by_unquoted_line_across_page_boundary()
    {
        const string markdown =
            "> ~~~bash\n" +
            "> echo safe\n" +
            "## Outside\n" +
            "> not code\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(1, (await fences.ReadCheckpointAsync(1)).FenceQuoteDepth);
            var page = await fences.ReadPageAsync(1);
            Assert.Equal(MarkdownStreamLineKind.Markdown, page.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown, page.Lines[1].Kind);
            Assert.Equal("## Outside", page.Lines[0].Content);
        });
    }

    [Fact]
    public async Task Utf8_bom_and_multibyte_characters_preserve_global_UTF16_offsets()
    {
        const string markdown = "# 🐈\n~~~cs\nřádek 🦋\n~~~\nEnd\n";
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
            .Concat(Encoding.UTF8.GetBytes(markdown)).ToArray();
        await WithBytesAsync(bytes, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            var middle = await fences.ReadPageAsync(1);
            Assert.Equal(markdown.IndexOf("řádek", StringComparison.Ordinal),
                middle.Checkpoint.Start);
            Assert.Equal(MarkdownStreamLineKind.CodeContent, middle.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose, middle.Lines[1].Kind);
            Assert.Equal(markdown, (await fences.ReadPageAsync(0)).Source.Content +
                middle.Source.Content + (await fences.ReadPageAsync(2)).Source.Content);
        });
    }

    [Fact]
    public async Task Index_uses_sparse_fixed_size_snapshots_and_is_deleted_on_dispose()
    {
        var markdown = "~~~bash\n" +
            string.Concat(Enumerable.Repeat("some code\n", 2_000)) + "~~~\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 10 }, tempDirectory: temp);
            var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            var checkpoint = Assert.Single(Directory.GetFiles(temp,
                "afrowave-markdown-fences-*.tmp"));
            Assert.Equal(16, fences.CheckpointStride);
            Assert.Equal(13, fences.SavedCheckpointCount);
            Assert.Equal(fences.SavedCheckpointCount * 512,
                new FileInfo(checkpoint).Length);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                (await fences.ReadPageAsync(150)).Lines[0].Kind);
            await fences.DisposeAsync();
            Assert.False(File.Exists(checkpoint));
            Assert.True(File.Exists(path));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => fences.ReadCheckpointAsync(0));
        });
    }

    [Fact]
    public async Task Nested_list_candidates_survive_page_seeks_and_keep_original_depth()
    {
        const string markdown =
            "- Parent\n" +
            "  - Child\n" +
            "    long paragraph\n" +
            "    continues\n" +
            "  - Sibling\n" +
            "- Root\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var snapshots = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            var last = await snapshots.ReadPageAsync(2);
            var middle = await snapshots.ReadPageAsync(1);
            var checkpoint = await snapshots.ReadCheckpointAsync(1);

            Assert.Equal(new[] { 0, 2 }, checkpoint.ListIndents);
            Assert.Equal(new[] { 2, 2 }, middle.Lines.Select(x => x.ListDepth));
            Assert.False(middle.Lines[0].IsParagraphContinuationCandidate);
            Assert.True(middle.Lines[1].IsParagraphContinuationCandidate);
            Assert.Equal(new[] { 2, 1 }, last.Lines.Select(x => x.ListDepth));
            Assert.Equal(markdown, (await snapshots.ReadPageAsync(0)).Source.Content +
                middle.Source.Content + last.Source.Content);
        });
    }

    [Fact]
    public async Task Quote_depth_and_contiguous_paragraph_candidate_resume_across_page()
    {
        const string markdown =
            "> A paragraph\n" +
            "> another line\n" +
            "> still the same quote\n" +
            "outside quote\n" +
            "outside again\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var snapshots = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            var checkpoint = await snapshots.ReadCheckpointAsync(1);
            Assert.Equal(1, checkpoint.PreviousQuoteDepth);
            Assert.True(checkpoint.PreviousParagraphCandidate);
            var second = await snapshots.ReadPageAsync(1);
            Assert.Equal(1, second.Lines[0].QuoteDepth);
            Assert.True(second.Lines[0].IsParagraphContinuationCandidate);
            Assert.Equal(0, second.Lines[1].QuoteDepth);
            Assert.False(second.Lines[1].IsParagraphContinuationCandidate);
            var third = await snapshots.ReadPageAsync(2);
            Assert.True(third.Lines[0].IsParagraphContinuationCandidate);
        });
    }

    [Fact]
    public async Task Blank_and_fence_boundaries_reset_paragraph_candidate()
    {
        const string markdown =
            "prose one\n" +
            "prose two\n" +
            "\n" +
            "new paragraph\n" +
            "~~~\n" +
            "code line\n" +
            "~~~\n" +
            "after code\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var snapshots = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            Assert.True(Assert.Single((await snapshots.ReadPageAsync(1)).Lines)
                .IsParagraphContinuationCandidate);
            Assert.False(Assert.Single((await snapshots.ReadPageAsync(3)).Lines)
                .IsParagraphContinuationCandidate);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                Assert.Single((await snapshots.ReadPageAsync(5)).Lines).Kind);
            Assert.False(Assert.Single((await snapshots.ReadPageAsync(7)).Lines)
                .IsParagraphContinuationCandidate);
        });
    }

    [Fact]
    public async Task Explicit_nested_list_continuations_keep_their_actual_owner_after_reverse_seek()
    {
        const string markdown =
            "- Parent\n" +
            "  - Child\n" +
            "    child details\n" +
            "  parent continuation\n" +
            "  - Sibling\n" +
            "    sibling details\n" +
            "- Last\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            var last = await index.ReadPageAsync(3);
            var second = await index.ReadPageAsync(1);
            var third = await index.ReadPageAsync(2);
            var first = await index.ReadPageAsync(0);

            Assert.Equal(markdown.IndexOf("- Last", StringComparison.Ordinal),
                last.Lines[0].ListOwnerStart);
            Assert.Equal(markdown.IndexOf("  - Child", StringComparison.Ordinal),
                second.Lines[0].ListOwnerStart);
            Assert.True(second.Lines[0].IsListContinuationCandidate);
            Assert.Equal(0L, second.Lines[1].ListOwnerStart);
            Assert.True(second.Lines[1].IsListContinuationCandidate);
            Assert.Equal(markdown.IndexOf("  - Sibling", StringComparison.Ordinal),
                third.Lines[1].ListOwnerStart);
            Assert.True(third.Lines[1].IsListContinuationCandidate);
            Assert.Equal(0L, first.Lines[0].ListOwnerStart);
            Assert.False(first.Lines[0].IsListContinuationCandidate);
            Assert.Equal(new[] { 0, 2 }, (await index.ReadCheckpointAsync(1))
                .ListOwners.Select(x => x.Indent));
        });
    }

    [Fact]
    public async Task Lazy_list_continuation_is_tentative_and_blank_breaks_it()
    {
        const string markdown =
            "- item\n" +
            "lazy prose\n" +
            "still lazy\n" +
            "\n" +
            "not owned\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            Assert.True(Assert.Single((await index.ReadPageAsync(1)).Lines)
                .IsListContinuationCandidate);
            Assert.True(Assert.Single((await index.ReadPageAsync(2)).Lines)
                .IsListContinuationCandidate);
            var plain = Assert.Single((await index.ReadPageAsync(4)).Lines);
            Assert.Null(plain.ListOwnerStart);
            Assert.False(plain.IsListContinuationCandidate);
        });
    }

    [Fact]
    public async Task Quote_depth_change_prevents_stealing_other_list_owners()
    {
        const string markdown =
            "> - quoted\n" +
            ">   continuation\n" +
            "- outside\n" +
            "  outside continuation\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            Assert.Equal(0L, (await index.ReadPageAsync(1)).Lines[0].ListOwnerStart);
            Assert.Equal(markdown.IndexOf("- outside", StringComparison.Ordinal),
                (await index.ReadPageAsync(3)).Lines[0].ListOwnerStart);
        });
    }

    [Fact]
    public async Task Random_access_pages_match_uninterrupted_scan_for_bounded_structure()
    {
        const string markdown =
            "# 🐈 Intro\r\n" +
            "- parent\n  - child\n    more prose\n    continuation\n" +
            "> quoted\n> quote continuation\n" +
            "> ~~~csharp\n> # code\n> ~~~\n" +
            "ordinary\nordinary continuation\n\n" +
            "~~~bash\n- literal item\n~~~\n" +
            "the end";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            var expected = new List<MarkdownStreamLine>();
            using (var complete = new StringReader(markdown))
                await foreach (var line in MarkdownStreamingReader.ScanAsync(complete))
                    expected.Add(line);

            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 3 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);

            // Request pages in reverse order, ensuring no shared cursor dependency.
            var actual = new MarkdownStreamLine[expected.Count];
            for (long page = index.PageCount; page-- > 0;)
            {
                var current = await index.ReadPageAsync(page);
                for (var i = 0; i < current.Lines.Count; i++)
                    actual[checked((int)(current.Source.FirstLine - 1 + i))] =
                        current.Lines[i];
            }

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Kind, actual[i].Kind);
                Assert.Equal(expected[i].Start, actual[i].Start);
                Assert.Equal(expected[i].RawText, actual[i].RawText);
                Assert.Equal(expected[i].QuoteDepth, actual[i].QuoteDepth);
                Assert.Equal(expected[i].ListDepth, actual[i].ListDepth);
                Assert.Equal(expected[i].OpenFenceStart, actual[i].OpenFenceStart);
                Assert.Equal(expected[i].CodeLanguage, actual[i].CodeLanguage);
                Assert.Equal(expected[i].IsParagraphContinuationCandidate,
                    actual[i].IsParagraphContinuationCandidate);
                Assert.Equal(expected[i].ParagraphStart, actual[i].ParagraphStart);
                Assert.Equal(expected[i].ListOwnerStart, actual[i].ListOwnerStart);
                Assert.Equal(expected[i].IsListContinuationCandidate,
                    actual[i].IsListContinuationCandidate);
            }
        });
    }

    [Fact]
    public async Task Rejects_too_small_checkpoint_quota_before_creating_file()
    {
        await WithMarkdownAsync("one\ntwo\n", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                MarkdownFenceCheckpointIndex.BuildAsync(
                    pages, tempDirectory: temp, maxIndexBytes: 512, checkpointStride: 1));
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-fences-*.tmp"));
        });
    }

    [Fact]
    public async Task Sparse_checkpoints_replay_owner_and_fence_state_on_either_side_of_boundaries()
    {
        var markdown = "- Root\n" +
            string.Concat(Enumerable.Repeat("  lazy owned continuation\n", 30)) +
            "  - Child\n" +
            string.Concat(Enumerable.Repeat("    nested continuation\n", 24)) +
            "  - Sibling\n" +
            "  ~~~cs\n" +
            string.Concat(Enumerable.Repeat("  # literal code\n", 16)) +
            "  ~~~\n" +
            "  back to root\n";

        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            var expected = new List<MarkdownStreamLine>();
            using (var full = new StringReader(markdown))
                await foreach (var line in MarkdownStreamingReader.ScanAsync(full))
                    expected.Add(line);

            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp, checkpointStride: 4);

            Assert.Equal(4, index.CheckpointStride);
            Assert.Equal(1 + (pages.PageCount - 1) / 4,
                index.SavedCheckpointCount);
            var file = Assert.Single(Directory.GetFiles(temp,
                "afrowave-markdown-fences-*.tmp"));
            Assert.Equal(index.SavedCheckpointCount * 512,
                new FileInfo(file).Length);

            var actual = new MarkdownStreamLine[expected.Count];
            for (long page = index.PageCount; page-- > 0;)
            {
                var snapshot = await index.ReadPageAsync(page);
                Assert.Equal(snapshot.Checkpoint.Start,
                    snapshot.Lines[0].Start);
                for (var i = 0; i < snapshot.Lines.Count; i++)
                    actual[checked((int)(snapshot.Source.FirstLine - 1 + i))] =
                        snapshot.Lines[i];
            }

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Start, actual[i].Start);
                Assert.Equal(expected[i].Kind, actual[i].Kind);
                Assert.Equal(expected[i].RawText, actual[i].RawText);
                Assert.Equal(expected[i].QuoteDepth, actual[i].QuoteDepth);
                Assert.Equal(expected[i].ListDepth, actual[i].ListDepth);
                Assert.Equal(expected[i].ListOwnerStart, actual[i].ListOwnerStart);
                Assert.Equal(expected[i].IsListContinuationCandidate,
                    actual[i].IsListContinuationCandidate);
                Assert.Equal(expected[i].IsParagraphContinuationCandidate,
                    actual[i].IsParagraphContinuationCandidate);
                Assert.Equal(expected[i].ParagraphStart, actual[i].ParagraphStart);
                Assert.Equal(expected[i].OpenFenceStart, actual[i].OpenFenceStart);
            }
        });
    }

    [Fact]
    public async Task Paragraph_start_is_preserved_through_sparse_replay_and_changes_with_list_owner()
    {
        const string markdown =
            "- parent\n" +
            "  parent paragraph A\n" +
            "  parent paragraph B\n" +
            "  - child\n" +
            "    child paragraph A\n" +
            "    child paragraph B\n" +
            "  parent again\n" +
            "  parent again continued\n" +
            "\n" +
            "  fresh paragraph after blank\n" +
            "  continuation after blank\n" +
            "\n" +
            "outside\n";

        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp, checkpointStride: 4);

            // Read out of order, reconstructing both saved and intermediate
            // checkpoints, without retaining a single previous page.
            var childContinued = Assert.Single((await index.ReadPageAsync(5)).Lines);
            var parentReturned = Assert.Single((await index.ReadPageAsync(6)).Lines);
            var parentContinued = Assert.Single((await index.ReadPageAsync(7)).Lines);
            var fresh = Assert.Single((await index.ReadPageAsync(9)).Lines);
            var afterFresh = Assert.Single((await index.ReadPageAsync(10)).Lines);
            var outside = Assert.Single((await index.ReadPageAsync(12)).Lines);
            var parentFirst = Assert.Single((await index.ReadPageAsync(1)).Lines);
            var parentSecond = Assert.Single((await index.ReadPageAsync(2)).Lines);

            Assert.True(parentSecond.IsParagraphContinuationCandidate);
            Assert.Equal(parentFirst.Start, parentSecond.ParagraphStart);
            Assert.Equal(parentFirst.ParagraphStart, parentSecond.ParagraphStart);
            Assert.Equal(markdown.IndexOf("    child paragraph A", StringComparison.Ordinal),
                childContinued.ParagraphStart);
            Assert.True(childContinued.IsParagraphContinuationCandidate);
            Assert.False(parentReturned.IsParagraphContinuationCandidate);
            Assert.Equal(parentReturned.Start, parentReturned.ParagraphStart);
            Assert.Equal(0L, parentReturned.ListOwnerStart);
            Assert.True(parentContinued.IsParagraphContinuationCandidate);
            Assert.Equal(parentReturned.Start, parentContinued.ParagraphStart);
            Assert.False(fresh.IsParagraphContinuationCandidate);
            Assert.Equal(fresh.Start, fresh.ParagraphStart);
            Assert.True(afterFresh.IsParagraphContinuationCandidate);
            Assert.Equal(fresh.Start, afterFresh.ParagraphStart);
            Assert.Null(outside.ListOwnerStart);
            Assert.False(outside.IsParagraphContinuationCandidate);
        });
    }

    [Fact]
    public async Task Quote_transition_and_fence_reset_paragraph_identity()
    {
        const string markdown =
            "unquoted first\n" +
            "unquoted second\n" +
            "> quoted one\n" +
            "> quoted two\n" +
            "~~~md\n" +
            "not a paragraph\n" +
            "~~~\n" +
            "outside again\n";

        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp, checkpointStride: 3);

            var line1 = Assert.Single((await index.ReadPageAsync(1)).Lines);
            var quoted = Assert.Single((await index.ReadPageAsync(2)).Lines);
            var quotedContinuation = Assert.Single((await index.ReadPageAsync(3)).Lines);
            var code = Assert.Single((await index.ReadPageAsync(5)).Lines);
            var final = Assert.Single((await index.ReadPageAsync(7)).Lines);

            Assert.Equal(0L, line1.ParagraphStart);
            Assert.True(line1.IsParagraphContinuationCandidate);
            Assert.Equal(quoted.Start, quoted.ParagraphStart);
            Assert.False(quoted.IsParagraphContinuationCandidate);
            Assert.Equal(quoted.Start, quotedContinuation.ParagraphStart);
            Assert.True(quotedContinuation.IsParagraphContinuationCandidate);
            Assert.Null(code.ParagraphStart);
            Assert.False(code.IsParagraphContinuationCandidate);
            Assert.Equal(final.Start, final.ParagraphStart);
            Assert.False(final.IsParagraphContinuationCandidate);
        });
    }

    [Fact]
    public async Task Blank_separated_list_paragraphs_and_siblings_keep_distinct_spacing_hints()
    {
        const string markdown =
            "- first item\n" +
            "  same paragraph\n" +
            "\n" +
            "  second paragraph\n" +
            "  continued paragraph\n" +
            "- second item\n" +
            "\n" +
            "- third item\n" +
            "\n" +
            "outside the list\n";

        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var sourcePages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var snapshots = await MarkdownFenceCheckpointIndex.BuildAsync(
                sourcePages, tempDirectory: temp, checkpointStride: 3);

            var expected = new List<MarkdownStreamLine>();
            using (var complete = new StringReader(markdown))
                await foreach (var line in MarkdownStreamingReader.ScanAsync(complete))
                    expected.Add(line);

            var actual = new MarkdownStreamLine[expected.Count];
            for (long page = snapshots.PageCount; page-- > 0;)
            {
                var current = await snapshots.ReadPageAsync(page);
                actual[checked((int)page)] = Assert.Single(current.Lines);
            }

            Assert.Equal(MarkdownListSpacingHint.None, actual[0].ListSpacingHint);
            Assert.Equal(MarkdownListSpacingHint.None, actual[1].ListSpacingHint);
            Assert.Equal(MarkdownListSpacingHint.BlankSeparatedParagraph,
                actual[3].ListSpacingHint);
            Assert.Equal(0L, actual[3].ListOwnerStart);
            Assert.False(actual[3].IsParagraphContinuationCandidate);
            Assert.Equal(actual[3].Start, actual[3].ParagraphStart);
            Assert.Equal(MarkdownListSpacingHint.None, actual[4].ListSpacingHint);
            Assert.True(actual[4].IsParagraphContinuationCandidate);
            Assert.Equal(MarkdownListSpacingHint.None, actual[5].ListSpacingHint);
            Assert.Equal(MarkdownListSpacingHint.BlankSeparatedItem,
                actual[7].ListSpacingHint);
            Assert.Equal(MarkdownListSpacingHint.None, actual[9].ListSpacingHint);
            Assert.Null(actual[9].ListOwnerStart);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].ListSpacingHint, actual[i].ListSpacingHint);
                Assert.Equal(expected[i].ListOwnerStart, actual[i].ListOwnerStart);
                Assert.Equal(expected[i].ParagraphStart, actual[i].ParagraphStart);
            }
        });
    }

    [Fact]
    public async Task Quoted_blank_continuation_is_tracked_without_crossing_quote_depth()
    {
        const string markdown =
            "> - quoted item\n" +
            ">\n" +
            ">   new quoted paragraph\n" +
            "\n" +
            "ordinary paragraph\n" +
            "\n" +
            "- new standalone item\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp, checkpointStride: 2);
            var quoted = Assert.Single((await index.ReadPageAsync(2)).Lines);
            var outside = Assert.Single((await index.ReadPageAsync(4)).Lines);
            var standalone = Assert.Single((await index.ReadPageAsync(6)).Lines);
            Assert.Equal(MarkdownListSpacingHint.BlankSeparatedParagraph,
                quoted.ListSpacingHint);
            Assert.Equal(0L, quoted.ListOwnerStart);
            Assert.Equal(MarkdownListSpacingHint.None, outside.ListSpacingHint);
            Assert.Null(outside.ListOwnerStart);
            Assert.Equal(MarkdownListSpacingHint.None, standalone.ListSpacingHint);
        });
    }

    [Fact]
    public async Task Consecutive_blank_lines_are_not_mistaken_for_contiguous_prose()
    {
        const string markdown =
            "- item\n" +
            "  first paragraph\n" +
            "\n" +
            "\n" +
            "  indented paragraph after blank lines\n" +
            "\n" +
            "unindented outside\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp, checkpointStride: 4);
            var indented = Assert.Single((await index.ReadPageAsync(4)).Lines);
            var outside = Assert.Single((await index.ReadPageAsync(6)).Lines);
            Assert.Equal(MarkdownListSpacingHint.BlankSeparatedParagraph,
                indented.ListSpacingHint);
            Assert.False(indented.IsParagraphContinuationCandidate);
            Assert.Equal(indented.Start, indented.ParagraphStart);
            Assert.Equal(MarkdownListSpacingHint.None, outside.ListSpacingHint);
            Assert.Null(outside.ListOwnerStart);
        });
    }

    [Fact]
    public async Task Sparse_quota_is_evaluated_on_record_count_not_page_count()
    {
        await WithMarkdownAsync(string.Concat(Enumerable.Repeat("word\n", 17)),
            async (path, temp) =>
            {
                await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                    new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
                await using var index = await MarkdownFenceCheckpointIndex.BuildAsync(
                    pages, tempDirectory: temp,
                    maxIndexBytes: 2 * 512, checkpointStride: 16);
                Assert.Equal(2, index.SavedCheckpointCount);
                await Assert.ThrowsAsync<InvalidDataException>(() =>
                    MarkdownFenceCheckpointIndex.BuildAsync(pages,
                        tempDirectory: temp, maxIndexBytes: 512,
                        checkpointStride: 16));
                Assert.Single(Directory.GetFiles(temp,
                    "afrowave-markdown-fences-*.tmp"));
            });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(257)]
    public async Task Sparse_stride_outside_valid_range_is_rejected(int stride)
    {
        await WithMarkdownAsync("line\n", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(
                path, tempDirectory: temp);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                MarkdownFenceCheckpointIndex.BuildAsync(pages, tempDirectory: temp,
                    checkpointStride: stride));
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-fences-*.tmp"));
        });
    }

    [Fact]
    public async Task Utf8_bom_only_document_has_no_phantom_line_or_checkpoint()
    {
        await WithBytesAsync([0xEF, 0xBB, 0xBF], async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            Assert.Equal(0, pages.LineCount);
            Assert.Equal(0, pages.PageCount);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(0, fences.PageCount);
        });
    }

    [Fact]
    public async Task An_empty_document_has_no_checkpoint_records()
    {
        await WithMarkdownAsync("", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(0, fences.PageCount);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => fences.ReadCheckpointAsync(0));
        });
    }

    private static Task WithMarkdownAsync(
        string markdown, Func<string, string, Task> assertion) =>
        WithBytesAsync(new UTF8Encoding(false).GetBytes(markdown), assertion);

    private static async Task WithBytesAsync(
        byte[] bytes, Func<string, string, Task> assertion)
    {
        var directory = Path.Combine(Path.GetTempPath(), "afrowave-fence-index-test-" +
            Guid.NewGuid().ToString("N"));
        var temp = Path.Combine(directory, "index");
        Directory.CreateDirectory(temp);
        var file = Path.Combine(directory, "source.md");
        await File.WriteAllBytesAsync(file, bytes);
        try { await assertion(file, temp); }
        finally { Directory.Delete(directory, true); }
    }
}
