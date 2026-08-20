using Jellyfin.Plugin.Kindle.Configuration;
using Xunit;

namespace Jellyfin.Plugin.Kindle.Tests;

public class PluginConfigurationTests
{
    [Fact]
    public void Migrate_LeavesAFreshInstallOnAuto()
    {
        var config = new PluginConfiguration();

        config.Migrate();

        Assert.Equal(SmtpSecurity.Auto, config.SecurityMode);
        Assert.Equal(PluginConfiguration.LatestConfigVersion, config.ConfigVersion);
    }

    [Fact]
    public void Migrate_RoutesPort465ToImplicitTls()
    {
        // The bug this migration exists for: the old UseSsl flag only ever selected
        // STARTTLS, so a port 465 setup could never establish a session.
        var config = new PluginConfiguration { SmtpHost = "mail.gmx.net", SmtpPort = 465, UseSsl = true };

        config.Migrate();

        Assert.Equal(SmtpSecurity.SslOnConnect, config.SecurityMode);
    }

    [Fact]
    public void Migrate_KeepsStartTlsForPort587()
    {
        var config = new PluginConfiguration { SmtpHost = "smtp.gmail.com", SmtpPort = 587, UseSsl = true };

        config.Migrate();

        Assert.Equal(SmtpSecurity.StartTls, config.SecurityMode);
    }

    [Fact]
    public void Migrate_MapsDisabledSslToAuto()
    {
        var config = new PluginConfiguration { SmtpHost = "localhost", SmtpPort = 25, UseSsl = false };

        config.Migrate();

        Assert.Equal(SmtpSecurity.Auto, config.SecurityMode);
    }

    [Fact]
    public void Migrate_DoesNotOverwriteAnAlreadyMigratedChoice()
    {
        var config = new PluginConfiguration
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            UseSsl = true,
            Security = (int)SmtpSecurity.None,
            ConfigVersion = PluginConfiguration.LatestConfigVersion
        };

        config.Migrate();

        Assert.Equal(SmtpSecurity.None, config.SecurityMode);
    }

    [Fact]
    public void Migrate_IsIdempotent()
    {
        var config = new PluginConfiguration { SmtpHost = "mail.gmx.net", SmtpPort = 465, UseSsl = true };

        config.Migrate();
        var afterFirst = config.SecurityMode;
        config.Migrate();

        Assert.Equal(afterFirst, config.SecurityMode);
    }

    [Fact]
    public void SecurityMode_FallsBackToAutoForAnOutOfRangeValue()
    {
        var config = new PluginConfiguration { Security = 99 };

        Assert.Equal(SmtpSecurity.Auto, config.SecurityMode);
    }

    [Fact]
    public void SetUserEmail_StoresTrimmedAndReadsBack()
    {
        var config = new PluginConfiguration();

        config.SetUserEmail("user-1", "  name@kindle.com  ");

        Assert.Equal("name@kindle.com", config.GetUserEmail("user-1"));
    }

    [Fact]
    public void SetUserEmail_WithNullRemovesTheEntry()
    {
        var config = new PluginConfiguration();
        config.SetUserEmail("user-1", "name@kindle.com");

        config.SetUserEmail("user-1", null);

        Assert.Null(config.GetUserEmail("user-1"));
    }

    [Fact]
    public void SetUserEmail_KeepsOtherUsersIntact()
    {
        var config = new PluginConfiguration();

        config.SetUserEmail("user-1", "one@kindle.com");
        config.SetUserEmail("user-2", "two@kindle.com");
        config.SetUserEmail("user-1", null);

        Assert.Null(config.GetUserEmail("user-1"));
        Assert.Equal("two@kindle.com", config.GetUserEmail("user-2"));
    }

    [Fact]
    public void GetUserEmail_TreatsAStoredBlankAsUnset()
    {
        var config = new PluginConfiguration { UserKindleEmailsJson = "{\"user-1\":\"   \"}" };

        Assert.Null(config.GetUserEmail("user-1"));
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("{\"unterminated\":")]
    [InlineData("")]
    public void GetUserEmail_SurvivesACorruptedConfigurationFile(string json)
    {
        // A hand-edited or truncated plugin XML must not take every endpoint down.
        var config = new PluginConfiguration { UserKindleEmailsJson = json };

        Assert.Null(config.GetUserEmail("user-1"));
    }
}
