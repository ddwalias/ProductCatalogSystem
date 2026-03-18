using FluentAssertions;
using ProductCatalogSystem.Server.Common;

namespace ProductCatalogSystem.Server.Tests;

public sealed class PagingOptionsTests
{
    [Fact]
    public void Normalize_ShouldClampPageSize_AndTrimCursor()
    {
        var result = PagingOptions.Normalize("  cursor-token  ", 500);

        result.Cursor.Should().Be("cursor-token");
        result.PageSize.Should().Be(PagingOptions.MaxPageSize);
    }

    [Fact]
    public void Normalize_ShouldDefaultMissingCursorAndPageSize()
    {
        var result = PagingOptions.Normalize(null, 0);

        result.Cursor.Should().BeNull();
        result.PageSize.Should().Be(PagingOptions.DefaultPageSize);
    }
}
