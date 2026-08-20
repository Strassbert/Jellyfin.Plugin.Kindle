using Jellyfin.Plugin.Kindle;
using Xunit;

namespace Jellyfin.Plugin.Kindle.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("name@kindle.com")]
    [InlineData("first.last+tag@sub.example.co.uk")]
    [InlineData("a@b.co")]
    public void IsPlausibleEmailAddress_AcceptsRealAddresses(string email)
    {
        Assert.True(Validation.IsPlausibleEmailAddress(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign.com")]
    [InlineData("@kindle.com")]
    [InlineData("two@at@kindle.com")]
    [InlineData("name@localhost")]
    [InlineData("name@.com")]
    [InlineData("name@com.")]
    [InlineData("name@ex..com")]
    [InlineData("name with space@kindle.com")]
    public void IsPlausibleEmailAddress_RejectsMalformed(string? email)
    {
        Assert.False(Validation.IsPlausibleEmailAddress(email));
    }

    [Fact]
    public void IsPlausibleEmailAddress_RejectsOverlyLongInput()
    {
        var email = new string('a', 250) + "@kindle.com";
        Assert.False(Validation.IsPlausibleEmailAddress(email));
    }

    [Fact]
    public void BuildAttachmentName_UsesItemNameAndExtension()
    {
        Assert.Equal("Der Steppenwolf.epub", Validation.BuildAttachmentName("Der Steppenwolf", ".epub"));
    }

    [Theory]
    [InlineData("A/B")]
    [InlineData("A\\B")]
    [InlineData("A\"B")]
    [InlineData("A\rB")]
    [InlineData("A\nB")]
    public void BuildAttachmentName_StripsCharactersThatCouldEscapeTheMimeHeader(string itemName)
    {
        var result = Validation.BuildAttachmentName(itemName, ".epub");

        Assert.DoesNotContain('/', result);
        Assert.DoesNotContain('\\', result);
        Assert.DoesNotContain('"', result);
        Assert.DoesNotContain('\r', result);
        Assert.DoesNotContain('\n', result);
        Assert.EndsWith(".epub", result);
    }

    [Fact]
    public void BuildAttachmentName_FallsBackWhenNameIsEmpty()
    {
        Assert.Equal("book.pdf", Validation.BuildAttachmentName(null, ".pdf"));
        Assert.Equal("book.pdf", Validation.BuildAttachmentName("   ", ".pdf"));
    }

    [Fact]
    public void BuildAttachmentName_NeverProducesABareExtension()
    {
        // Whatever the item is called, the attachment must carry a filename - a bare
        // ".epub" makes some clients drop the attachment entirely.
        Assert.Equal("___.epub", Validation.BuildAttachmentName("///", ".epub"));
        Assert.Equal("book.epub", Validation.BuildAttachmentName("\r\n", ".epub"));
    }

    [Fact]
    public void BuildAttachmentName_TruncatesLongTitles()
    {
        var result = Validation.BuildAttachmentName(new string('x', 400), ".epub");

        Assert.Equal(100 + ".epub".Length, result.Length);
        Assert.EndsWith(".epub", result);
    }
}
