namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Dependency-free, source-preserving first-pass Markdown block scanner.
/// Includes conservative inline tokens for headings, paragraphs and list items. Not full CommonMark.
/// </summary>
public static class MarkdownParser
{
    /// <summary>Read Markdown without changing content, line endings or protected code text.</summary>
    public static MarkdownDocument Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var blocks = new List<MarkdownBlock>();
        var listRoots = new List<MutableListNode>();
        var listStack = new List<MutableListNode>();
        // Quoted lists have their own hierarchy and never become descendants
        // of an unrelated list outside the quote container.
        var quoteListRoots = new List<MutableListNode>();
        var quoteListStack = new List<MutableListNode>();
        var previousQuoteDepth = 0;
        int? previousQuoteOuterOwnerStart = null;
        var pendingBlankIndices = new List<int>();
        // Physical quote-prefix blank lines are deferred until a following
        // explicitly indented line proves ownership (never lazy across blanks).
        var pendingQuoteBlankIndices = new List<int>();
        var offset = 0;
        var inCode = false;
        var fenceMarker = '\0';
        var fenceLength = 0;
        var fenceStart = -1;
        string? fenceLanguage = null;
        string? fenceInfo = null;
        MutableListNode? fenceOwner = null;
        MutableListNode? fenceOuterOwner = null;
        // Zero means a normal Markdown fence. Positive depth means every code
        // line must explicitly retain that same blockquote prefix.
        var fenceQuoteDepth = 0;

        while (offset < source.Length)
        {
            var contentEnd = offset;
            while (contentEnd < source.Length && source[contentEnd] is not ('\r' or '\n'))
                contentEnd++;

            var next = contentEnd;
            if (next < source.Length && source[next] == '\r')
                next++;
            if (next < source.Length && source[next] == '\n')
                next++;

            var line = source[offset..contentEnd];
            var raw = source[offset..next];
            var level = 0;
            MarkdownBlockKind kind;
            string content = line;
            MarkdownListMarker? marker = null;
            MutableListNode? continuationOwner = null;
            var lazyContinuation = false;
            var codeClosing = false;
            var quoteDepth = 0;
            var isQuotedListLine = false;
            var protectedCodeLine = false;
            MutableListNode? quoteOuterOwner = null;
            var currentFenceStart = -1;
            string? currentLanguage = null;
            string? currentInfo = null;

            // A quote-owned fence cannot consume a line from another quote depth
            // or from the surrounding document. Reclassify the first such line
            // normally, rather than hiding all following Markdown as code.
            if (inCode && fenceQuoteDepth > 0 &&
                (!TryQuotedContainerLine(line, listStack, out var observedOuter,
                    out var observedQuoteDepth, out _) ||
                 observedQuoteDepth != fenceQuoteDepth ||
                 observedOuter?.Block.Start != fenceOuterOwner?.Block.Start))
            {
                inCode = false;
                fenceOwner = null;
                fenceOuterOwner = null;
                fenceStart = -1;
                fenceInfo = null;
                fenceLanguage = null;
                fenceQuoteDepth = 0;
            }

            // A quote blank keeps the visible '>' marker and quote depth. Do not
            // attach it eagerly: only an explicitly indented continuation at
            // the same quote depth can claim the pending separators.
            if (!inCode && pendingQuoteBlankIndices.Count > 0)
            {
                var sameQuote = TryQuotedContainerLine(line, listStack,
                    out var candidateOuter, out var candidateDepth,
                    out var candidateQuoteContent) &&
                    candidateDepth == previousQuoteDepth &&
                    candidateOuter?.Block.Start == previousQuoteOuterOwnerStart;
                if (!sameQuote || !string.IsNullOrWhiteSpace(candidateQuoteContent))
                {
                    var probe = new List<MutableListNode>(quoteListStack);
                    var startsNewItem = TryListItem(candidateQuoteContent,
                        out _, out _, out _, out _);
                    if (sameQuote && !startsNewItem &&
                        TryContinuation(candidateQuoteContent, probe,
                            out var quoteBlankOwner, out _) && quoteBlankOwner is not null)
                    {
                        foreach (var blankIndex in pendingQuoteBlankIndices)
                        {
                            var ownedBlank = blocks[blankIndex] with
                            {
                                ListBlankOwnerStart = quoteBlankOwner.Block.Start
                            };
                            blocks[blankIndex] = ownedBlank;
                            quoteBlankOwner.SeparatorBlocks.Add(ownedBlank);
                            quoteBlankOwner.ContentBlocks.Add(ownedBlank);
                        }
                    }
                    else
                    {
                        quoteListStack.Clear();
                    }
                    pendingQuoteBlankIndices.Clear();
                }
            }

            // Defer classification of blank separators until the next nonblank line.
            // Only a sufficiently indented paragraph can turn those blanks into list content.
            // Keeping the pending indices (not copying source strings) avoids repeated lookahead.
            if (!inCode && pendingBlankIndices.Count > 0 && !string.IsNullOrWhiteSpace(line))
            {
                var probe = new List<MutableListNode>(listStack);
                var isMarker = TryListItem(line, out _, out _, out _, out _);
                if (!isMarker && TryContinuation(line, probe, out var blankOwner, out _)
                    && blankOwner is not null)
                {
                    foreach (int blankIndex in pendingBlankIndices)
                    {
                        var ownedBlank = blocks[blankIndex] with
                        {
                            ListBlankOwnerStart = blankOwner.Block.Start
                        };
                        blocks[blankIndex] = ownedBlank;
                        blankOwner.SeparatorBlocks.Add(ownedBlank);
                        blankOwner.ContentBlocks.Add(ownedBlank);
                    }
                }
                else
                {
                    listStack.Clear();
                }
                pendingBlankIndices.Clear();
            }

            if (inCode)
            {
                protectedCodeLine = true;
                // Retain the list owner while scanning literal code: Markdown markers
                // inside a fence must never create list items, headings or quotes.
                continuationOwner = fenceOwner;
                if (fenceQuoteDepth > 0)
                {
                    // The depth was validated before the scan. Remove only the
                    // quote container and the owning list's content indentation;
                    // preserve all remaining literal code whitespace.
                    TryQuotedContainerLine(line, listStack, out quoteOuterOwner,
                        out quoteDepth, out content);
                    isQuotedListLine = true;
                    if (fenceOwner is not null)
                        content = StripIndentUpTo(content, fenceOwner.ContentIndentColumns);
                }
                else if (fenceOwner is not null)
                    content = StripIndentUpTo(line, fenceOwner.ContentIndentColumns);
                currentFenceStart = fenceStart;
                currentLanguage = fenceLanguage;
                currentInfo = fenceInfo;
                codeClosing = IsClosingFence(content, fenceMarker, fenceLength);
                kind = codeClosing ? MarkdownBlockKind.CodeFence : MarkdownBlockKind.CodeLine;
                if (codeClosing)
                    inCode = false;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                kind = MarkdownBlockKind.Blank;
            }
            else if (TryListItem(line, out var originalMarker, out var number,
                         out var indentColumns, out content) &&
                     (indentColumns <= 3 ||
                      (listStack.Count > 0 && indentColumns >= listStack[0].IndentColumns)))
            {
                while (listStack.Count > 0 && listStack[^1].IndentColumns >= indentColumns)
                    listStack.RemoveAt(listStack.Count - 1);

                // Keep hierarchy depth bounded for malformed/untrusted huge Markdown files.
                if (listStack.Count < 32)
                {
                    kind = MarkdownBlockKind.ListItem;
                    marker = new MarkdownListMarker(originalMarker, number.HasValue,
                        number, indentColumns, listStack.Count,
                        offset + line.Length - content.Length)
                    {
                        ContentIndentColumns = CountColumns(line, line.Length - content.Length)
                    };
                }
                else
                {
                    kind = MarkdownBlockKind.Paragraph;
                    content = line;
                }
            }
            else if (TryContinuation(line, listStack, out continuationOwner, out int contentIndex))
            {
                content = line[contentIndex..];
                if (TryFence(content, out fenceMarker, out fenceLength))
                {
                    kind = MarkdownBlockKind.CodeFence;
                    inCode = true;
                    fenceStart = offset;
                    fenceOwner = continuationOwner;
                    fenceOuterOwner = null;
                    fenceQuoteDepth = 0;
                    fenceInfo = ReadFenceInfo(content, fenceMarker, fenceLength);
                    fenceLanguage = ReadFenceLanguage(fenceInfo);
                    currentFenceStart = fenceStart;
                    currentLanguage = fenceLanguage;
                    currentInfo = fenceInfo;
                }
                else if (TryQuote(content, out quoteDepth, out var quoteContent))
                {
                    kind = MarkdownBlockKind.BlockQuote;
                    content = quoteContent;
                    quoteOuterOwner = continuationOwner;
                    continuationOwner = null;
                    isQuotedListLine = true;
                }
                else
                    kind = MarkdownBlockKind.Paragraph;
            }
            else if (TryLazyContinuation(line, listStack, out continuationOwner))
            {
                // A non-blank paragraph immediately after list content may continue
                // without indentation. Never cross a pending blank or a block starter.
                kind = MarkdownBlockKind.Paragraph;
                lazyContinuation = true;
            }
            else if (TryFence(line, out fenceMarker, out fenceLength))
            {
                kind = MarkdownBlockKind.CodeFence;
                inCode = true;
                fenceStart = offset;
                fenceOwner = null;
                fenceOuterOwner = null;
                fenceQuoteDepth = 0;
                fenceInfo = ReadFenceInfo(line, fenceMarker, fenceLength);
                fenceLanguage = ReadFenceLanguage(fenceInfo);
                currentFenceStart = fenceStart;
                currentLanguage = fenceLanguage;
                currentInfo = fenceInfo;
            }
            else if (TryQuote(line, out quoteDepth, out var quotedContent))
            {
                kind = MarkdownBlockKind.BlockQuote;
                content = quotedContent;
                isQuotedListLine = true;
            }
            else if (TryHeading(line, out level, out content))
            {
                kind = MarkdownBlockKind.Heading;
            }
            else
            {
                kind = MarkdownBlockKind.Paragraph;
            }

            if (isQuotedListLine && !protectedCodeLine)
            {
                // A quoted run is scoped to its particular outer list item.
                if (previousQuoteDepth != quoteDepth ||
                    previousQuoteOuterOwnerStart != quoteOuterOwner?.Block.Start)
                    quoteListStack.Clear();
                previousQuoteDepth = quoteDepth;
                previousQuoteOuterOwnerStart = quoteOuterOwner?.Block.Start;
                var quotedContent = content;

                if (TryListItem(quotedContent, out var quoteMarker, out var quoteNumber,
                        out var quoteIndent, out var quoteItemContent) &&
                    (quoteIndent <= 3 || (quoteListStack.Count > 0 &&
                                          quoteIndent >= quoteListStack[0].IndentColumns)))
                {
                    while (quoteListStack.Count > 0 &&
                           quoteListStack[^1].IndentColumns >= quoteIndent)
                        quoteListStack.RemoveAt(quoteListStack.Count - 1);

                    if (quoteListStack.Count < 32)
                    {
                        kind = MarkdownBlockKind.ListItem;
                        content = quoteItemContent;
                        marker = new MarkdownListMarker(quoteMarker, quoteNumber.HasValue,
                            quoteNumber, quoteIndent, quoteListStack.Count,
                            offset + line.Length - content.Length)
                        {
                            ContentIndentColumns = CountColumns(quotedContent,
                                quotedContent.Length - content.Length)
                        };
                    }
                    else
                    {
                        kind = MarkdownBlockKind.BlockQuote;
                        content = quotedContent;
                    }
                }
                else if (TryContinuation(quotedContent, quoteListStack,
                             out continuationOwner, out var quoteContentIndex))
                {
                    content = quotedContent[quoteContentIndex..];
                    if (TryFence(content, out fenceMarker, out fenceLength))
                    {
                        kind = MarkdownBlockKind.CodeFence;
                        inCode = true;
                        fenceStart = offset;
                        fenceQuoteDepth = quoteDepth;
                        fenceOwner = continuationOwner;
                        fenceOuterOwner = quoteOuterOwner;
                        fenceInfo = ReadFenceInfo(content, fenceMarker, fenceLength);
                        fenceLanguage = ReadFenceLanguage(fenceInfo);
                        currentFenceStart = fenceStart;
                        currentLanguage = fenceLanguage;
                        currentInfo = fenceInfo;
                    }
                    else
                        kind = MarkdownBlockKind.Paragraph;
                }
                else if (TryFence(quotedContent, out fenceMarker, out fenceLength))
                {
                    // A fence without the list's content indentation starts a
                    // quote-owned block, not an attachment to the preceding item.
                    quoteListStack.Clear();
                    kind = MarkdownBlockKind.CodeFence;
                    content = quotedContent;
                    inCode = true;
                    fenceStart = offset;
                    fenceQuoteDepth = quoteDepth;
                    fenceOwner = null;
                    fenceOuterOwner = quoteOuterOwner;
                    fenceInfo = ReadFenceInfo(content, fenceMarker, fenceLength);
                    fenceLanguage = ReadFenceLanguage(fenceInfo);
                    currentFenceStart = fenceStart;
                    currentLanguage = fenceLanguage;
                    currentInfo = fenceInfo;
                }
                else if (TryLazyContinuation(quotedContent, quoteListStack,
                             out continuationOwner))
                {
                    kind = MarkdownBlockKind.Paragraph;
                    content = quotedContent;
                    lazyContinuation = true;
                }
                else
                {
                    kind = MarkdownBlockKind.BlockQuote;
                    content = quotedContent;
                }

                // Unlike prose, a quote-prefixed blank can be a deferred list
                // separator. Keep the candidate stack only until the next
                // nonblank line decides if it belongs to this same quote level.
                if (kind == MarkdownBlockKind.BlockQuote &&
                    !string.IsNullOrWhiteSpace(content))
                    quoteListStack.Clear();
                // Ordinary quote prose remains an owned embedded block of
                // the outer item. An inner list retains its own owner metadata.
                if (kind == MarkdownBlockKind.BlockQuote &&
                    !string.IsNullOrWhiteSpace(content) && quoteOuterOwner is not null)
                    continuationOwner = quoteOuterOwner;
            }

            if (!isQuotedListLine)
            {
                quoteListStack.Clear();
                previousQuoteDepth = 0;
                previousQuoteOuterOwnerStart = null;
            }

            if (kind is not (MarkdownBlockKind.ListItem or MarkdownBlockKind.Blank)
                && continuationOwner is null && quoteOuterOwner is null)
                listStack.Clear();

            var inlines = kind is MarkdownBlockKind.Heading or MarkdownBlockKind.Paragraph
                or MarkdownBlockKind.ListItem or MarkdownBlockKind.BlockQuote
                ? MarkdownInlineParser.Parse(content, offset + line.Length - content.Length)
                : [];

            var block = new MarkdownBlock(kind, offset, next - offset, content, raw, level)
            {
                Inlines = inlines,
                ListMarker = marker,
                ListContinuationMarker = continuationOwner?.Block.ListMarker,
                ListContinuationOwnerStart = continuationOwner?.Block.Start,
                IsLazyListContinuation = lazyContinuation,
                QuoteDepth = quoteDepth,
                QuoteContainerListOwnerStart = quoteOuterOwner?.Block.Start,
                QuoteContainerListMarker = quoteOuterOwner?.Block.ListMarker,
                CodeFenceStart = currentFenceStart >= 0 ? currentFenceStart : null,
                CodeFenceInfo = currentInfo,
                CodeLanguage = currentLanguage,
                IsClosingCodeFence = codeClosing
            };
            blocks.Add(block);

            // The outer item retains an ordered flat view of its quoted
            // subtree, while QuoteListRoots exposes the quoted hierarchy.
            if (isQuotedListLine && quoteOuterOwner is not null &&
                continuationOwner != quoteOuterOwner)
            {
                quoteOuterOwner.EmbeddedBlocks.Add(block);
                quoteOuterOwner.ContentBlocks.Add(block);
            }

            if (marker is not null)
            {
                var item = new MutableListNode(block, marker.IndentColumns);
                var owningStack = isQuotedListLine ? quoteListStack : listStack;
                var owningRoots = isQuotedListLine ? quoteListRoots : listRoots;
                if (owningStack.Count > 0)
                    owningStack[^1].Children.Add(item);
                else
                    owningRoots.Add(item);
                owningStack.Add(item);
            }
            else if (continuationOwner is not null)
            {
                if (kind == MarkdownBlockKind.Paragraph)
                    continuationOwner.ContinuationBlocks.Add(block);
                else
                    continuationOwner.EmbeddedBlocks.Add(block);
                continuationOwner.ContentBlocks.Add(block);
            }
            else if (kind == MarkdownBlockKind.Blank && listStack.Count > 0)
            {
                pendingBlankIndices.Add(blocks.Count - 1);
            }
            else if (kind == MarkdownBlockKind.BlockQuote &&
                     isQuotedListLine && quoteListStack.Count > 0 &&
                     string.IsNullOrWhiteSpace(content))
            {
                pendingQuoteBlankIndices.Add(blocks.Count - 1);
            }

            if (codeClosing)
            {
                fenceOwner = null;
                fenceOuterOwner = null;
                fenceLanguage = null;
                fenceInfo = null;
                fenceStart = -1;
                fenceQuoteDepth = 0;
            }
            offset = next;
        }

        return new MarkdownDocument(source, blocks.AsReadOnly(),
            listRoots.Select(FreezeListNode).ToList().AsReadOnly(),
            quoteListRoots.Select(FreezeListNode).ToList().AsReadOnly());
    }

    // Normalize an explicit quote prefix that may follow the content indent
    // of a regular list item. Probe a stack copy to avoid changing ownership
    // while checking a candidate (including quoted code lines).
    private static bool TryQuotedContainerLine(string line,
        List<MutableListNode> outerStack, out MutableListNode? outerOwner,
        out int quoteDepth, out string content)
    {
        // An indented quote belongs to a surrounding list when its content
        // indentation matches. Probe that owner BEFORE accepting a standalone
        // quote with up to three source spaces (which would lose its owner).
        var probe = new List<MutableListNode>(outerStack);
        if (TryContinuation(line, probe, out var owner, out var index) &&
            TryQuote(line[index..], out quoteDepth, out content))
        {
            outerOwner = owner;
            return true;
        }

        outerOwner = null;
        if (TryQuote(line, out quoteDepth, out content))
            return true;

        quoteDepth = 0;
        content = line;
        return false;
    }

    private static string StripIndentUpTo(string line, int requiredColumns)
    {
        var columns = 0;
        var index = 0;
        while (index < line.Length && line[index] is ' ' or '\t' &&
               columns < requiredColumns)
        {
            columns += line[index] == '\t' ? 4 - columns % 4 : 1;
            index++;
        }
        // Lines that outdent the owning item are protected, not reclassified.
        return columns >= requiredColumns ? line[index..] : line;
    }

    private static string? ReadFenceInfo(string line, char marker, int length)
    {
        var info = line[(Indent(line) + length)..].Trim();
        // Backticks are forbidden in a backtick fence information string.
        // Retain the fence and its literal payload even if the hint is invalid.
        return info.Length > 0 && (marker != '`' || !info.Contains('`')) ? info : null;
    }

    private static string? ReadFenceLanguage(string? info)
    {
        if (info is null)
            return null;
        var length = 0;
        while (length < info.Length && !char.IsWhiteSpace(info[length]))
            length++;
        var hint = info[..length];
        // The source is kept verbatim; this bound applies only to the display hint.
        return hint.Length is > 0 and <= 64 ? hint : null;
    }

    private static bool TryQuote(string line, out int depth, out string text)
    {
        depth = 0;
        text = line;
        var index = Indent(line);
        if (index > 3 || index == line.Length || line[index] != '>')
            return false;
        while (index < line.Length && line[index] == '>' && depth < 32)
        {
            depth++;
            index++;
            if (index < line.Length && line[index] is ' ' or '\t')
                index++;
            // A nested quote may use > > or >>.
            if (index < line.Length && line[index] == '>')
                continue;
            break;
        }
        text = line[index..];
        return true;
    }

    private static int Indent(string line)
    {
        var index = 0;
        while (index < line.Length && line[index] == ' ' && index < 4)
            index++;
        return index;
    }

    private static bool TryFence(string line, out char marker, out int count)
    {
        marker = '\0';
        count = 0;
        var indent = Indent(line);
        if (indent > 3 || indent >= line.Length ||
            (line[indent] != '`' && line[indent] != '~'))
            return false;

        marker = line[indent];
        var index = indent;
        while (index < line.Length && line[index] == marker)
            index++;

        count = index - indent;
        return count >= 3;
    }

    private static bool IsClosingFence(string line, char marker, int requiredLength)
    {
        if (!TryFence(line, out var found, out var count) || found != marker || count < requiredLength)
            return false;

        var indent = Indent(line);
        return line[(indent + count)..].All(c => c is ' ' or '\t');
    }

    private static bool TryHeading(string line, out int level, out string content)
    {
        level = 0;
        content = line;
        var indent = Indent(line);
        if (indent > 3 || indent >= line.Length || line[indent] != '#')
            return false;

        var index = indent;
        while (index < line.Length && line[index] == '#')
            index++;

        level = index - indent;
        if (level is < 1 or > 6 || (index < line.Length && line[index] is not (' ' or '\t')))
        {
            level = 0;
            return false;
        }

        content = line[index..].TrimStart(' ', '\t');
        return true;
    }

    private static bool TryListItem(
        string line, out string originalMarker, out int? number,
        out int indentColumns, out string content)
    {
        originalMarker = string.Empty;
        number = null;
        content = line;
        indentColumns = 0;
        int index = 0;

        // Tabs advance to the next four-column stop. The physical source is untouched.
        while (index < line.Length && line[index] is ' ' or '\t')
        {
            indentColumns += line[index] == '\t' ? 4 - indentColumns % 4 : 1;
            if (indentColumns > 256)
                return false;
            index++;
        }
        if (index >= line.Length)
            return false;

        int markerStart = index;
        if (line[index] is '-' or '+' or '*')
            index++;
        else if (char.IsAsciiDigit(line[index]))
        {
            int digitStart = index;
            while (index < line.Length && char.IsAsciiDigit(line[index]) && index - digitStart < 10)
                index++;
            int digits = index - digitStart;
            if (digits is < 1 or > 9 || index >= line.Length ||
                line[index] is not ('.' or ')') ||
                !int.TryParse(line[digitStart..index], out int parsed))
                return false;
            number = parsed;
            index++;
        }
        else
            return false;

        if (index < line.Length && line[index] is not (' ' or '\t'))
            return false;
        originalMarker = line[markerStart..index];

        while (index < line.Length && line[index] is ' ' or '\t')
            index++;
        content = line[index..];
        return true;
    }

    private static int CountColumns(string line, int prefixLength)
    {
        int columns = 0;
        for (int index = 0; index < prefixLength; index++)
            columns += line[index] == '\t' ? 4 - columns % 4 : 1;
        return columns;
    }

    private static bool TryContinuation(string line, List<MutableListNode> stack,
        out MutableListNode? owner, out int contentIndex)
    {
        owner = null;
        contentIndex = 0;
        if (stack.Count == 0)
            return false;

        int columns = 0;
        while (contentIndex < line.Length && line[contentIndex] is ' ' or '\t')
        {
            columns += line[contentIndex] == '\t' ? 4 - columns % 4 : 1;
            if (columns > 256)
                return false;
            contentIndex++;
        }
        if (contentIndex == 0 || contentIndex == line.Length)
            return false;

        for (int index = stack.Count - 1; index >= 0; index--)
        {
            if (columns < stack[index].ContentIndentColumns)
                continue;
            owner = stack[index];
            if (index + 1 < stack.Count)
                stack.RemoveRange(index + 1, stack.Count - index - 1);
            return true;
        }
        return false;
    }

    // Conservative paragraph-only lazy continuation: no preceding blank and no
    // new block marker. Indented continuations and ancestor selection are handled
    // by TryContinuation before this method is called.
    private static bool TryLazyContinuation(string line, List<MutableListNode> stack,
        out MutableListNode? owner)
    {
        owner = null;
        if (stack.Count == 0 || string.IsNullOrWhiteSpace(line))
            return false;

        if (TryListItem(line, out _, out _, out _, out _) ||
            TryHeading(line, out _, out _) ||
            TryFence(line, out _, out _) ||
            IsThematicBreak(line))
            return false;

        var indent = Indent(line);
        if (indent < line.Length && line[indent] == '>')
            return false;

        // A lazy line belongs to the last active item, including a nested item.
        // Pending blank separators clear the stack before this branch is reached.
        owner = stack[^1];
        return true;
    }

    private static bool IsThematicBreak(string line)
    {
        var indent = Indent(line);
        if (indent > 3 || indent >= line.Length ||
            line[indent] is not ('-' or '*' or '_'))
            return false;

        var marker = line[indent];
        var count = 0;
        for (var i = indent; i < line.Length; i++)
        {
            if (line[i] == marker)
                count++;
            else if (line[i] is not (' ' or '\t'))
                return false;
        }
        return count >= 3;
    }

    private static MarkdownListItemNode FreezeListNode(MutableListNode item) =>
        new(item.Block, item.Children.Select(FreezeListNode).ToList().AsReadOnly(),
            item.ContinuationBlocks.AsReadOnly(),
            item.SeparatorBlocks.AsReadOnly(),
            item.ContentBlocks.AsReadOnly(),
            item.EmbeddedBlocks.AsReadOnly());

    private sealed class MutableListNode(MarkdownBlock block, int indentColumns)
    {
        public MarkdownBlock Block { get; } = block;
        public int IndentColumns { get; } = indentColumns;
        public int ContentIndentColumns { get; } = block.ListMarker!.ContentIndentColumns;
        public List<MutableListNode> Children { get; } = [];
        public List<MarkdownBlock> ContinuationBlocks { get; } = [];
        public List<MarkdownBlock> SeparatorBlocks { get; } = [];
        public List<MarkdownBlock> ContentBlocks { get; } = [];
        public List<MarkdownBlock> EmbeddedBlocks { get; } = [];
    }
}
