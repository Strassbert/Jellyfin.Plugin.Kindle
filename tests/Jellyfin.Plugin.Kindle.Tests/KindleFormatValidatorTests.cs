using Jellyfin.Plugin.Kindle;
using Xunit;

namespace Jellyfin.Plugin.Kindle.Tests;

public class KindleFormatValidatorTests
{
    [Theory]
    [InlineData(".epub")]
    [InlineData("epub")]
    [InlineData(".EPUB")]
    [InlineData(".Pdf")]
    [InlineData(".azw3")]
    public void IsCompatible_AcceptsSupportedFormatsRegardlessOfCaseOrLeadingDot(string extension)
    {
        Assert.True(KindleFormatValidator.IsCompatible(extension));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(".exe")]
    [InlineData(".cbz")]
    [InlineData(".mp4")]
    public void IsCompatible_RejectsEverythingElse(string? extension)
    {
        Assert.False(KindleFormatValidator.IsCompatible(extension));
    }

    [Fact]
    public void MaxFileSizeBytes_LeavesRoomForEncodingOverhead()
    {
        // Base64 inflates an attachment by roughly a third. The previous bare 50 MB
        // file check let messages through that the provider always rejected.
        var limit = KindleFormatValidator.MaxFileSizeBytes(50);

        Assert.True(limit < 50L * 1024 * 1024, "the file limit must be below the message limit");
        Assert.True(limit > 30L * 1024 * 1024, "the file limit must still be usable");
    }

    [Fact]
    public void MaxFileSizeBytes_ScalesWithTheConfiguredLimit()
    {
        Assert.Equal(2 * KindleFormatValidator.MaxFileSizeBytes(25), KindleFormatValidator.MaxFileSizeBytes(50), 1L);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void MaxFileSizeBytes_FallsBackForNonsenseConfiguration(int configured)
    {
        Assert.Equal(KindleFormatValidator.MaxFileSizeBytes(50), KindleFormatValidator.MaxFileSizeBytes(configured));
    }
}
