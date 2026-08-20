using Jellyfin.Plugin.Kindle.Api;
using Xunit;

namespace Jellyfin.Plugin.Kindle.Tests;

public class LocalizationTests
{
    [Theory]
    [InlineData("de")]
    [InlineData("de-DE")]
    [InlineData("de_AT")]
    [InlineData("DE")]
    public void GetMergedStrings_ResolvesRegionalAndCasedLanguageTags(string tag)
    {
        var strings = KindleResourceController.GetMergedStrings(tag);

        Assert.Equal("An Reader senden", strings["button.send"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kl")]
    [InlineData("../../etc/passwd")]
    [InlineData("en")]
    public void GetMergedStrings_FallsBackToEnglish(string? tag)
    {
        var strings = KindleResourceController.GetMergedStrings(tag);

        Assert.Equal("Send to reader", strings["button.send"]);
    }

    [Fact]
    public void GetMergedStrings_ReturnsEveryKeyForATranslatedLanguage()
    {
        // Overlaying onto English guarantees a complete table even if a translation
        // is missing keys, so the UI can never render a raw key.
        var english = KindleResourceController.GetMergedStrings("en");
        var german = KindleResourceController.GetMergedStrings("de");

        Assert.Equal(english.Count, german.Count);
        Assert.All(english.Keys, key => Assert.True(german.ContainsKey(key), $"missing: {key}"));
    }

    [Fact]
    public void GetMergedStrings_CoversAllThreeFrontends()
    {
        var strings = KindleResourceController.GetMergedStrings("de");

        Assert.Contains("button.send", strings.Keys);
        Assert.Contains("admin.smtpSection", strings.Keys);
        Assert.Contains("user.title", strings.Keys);
    }
}
