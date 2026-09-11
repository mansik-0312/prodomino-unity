using HelperSharedLibrary;
using NUnit.Framework;
using ProDomino.Authentication;

public class CredentialsValidatorTests
{
    [Test]
    public void IsValidEmail_AcceptsStandardAddress()
    {
        Assert.IsTrue(CredentialsValidator.IsValidEmail("player.one@example.com"));
    }

    [Test]
    public void IsValidEmail_RejectsShortLocalPart()
    {
        Assert.IsFalse(CredentialsValidator.IsValidEmail("ab@example.com"));
    }

    [Test]
    public void IsValidPassword_RequiresLengthCaseAndSymbol()
    {
        Assert.IsFalse(CredentialsValidator.IsValidPassword("short"));
        Assert.IsFalse(CredentialsValidator.IsValidPassword("nouppercase1!"));
        Assert.IsFalse(CredentialsValidator.IsValidPassword("NOLOWERCASE1!"));
        Assert.IsFalse(CredentialsValidator.IsValidPassword("NoSymbol1234"));
        Assert.IsTrue(CredentialsValidator.IsValidPassword("ValidPass1!"));
    }

    [Test]
    public void IsValidConfirmPassword_RequiresMatch()
    {
        Assert.IsTrue(CredentialsValidator.IsValidConfirmPassword("ValidPass1!", "ValidPass1!"));
        Assert.IsFalse(CredentialsValidator.IsValidConfirmPassword("ValidPass1!", "ValidPass2!"));
    }

    [Test]
    public void DeriveUsernameFromEmail_SanitizesTruncatesAndPads()
    {
        Assert.AreEqual("player.one", CredentialsValidator.DeriveUsernameFromEmail("player.one@example.com"));
        Assert.AreEqual("abcdefghijklmnopqrst", CredentialsValidator.DeriveUsernameFromEmail("abcdefghijklmnopqrstuvwxyz@example.com"));
        Assert.AreEqual("use", CredentialsValidator.DeriveUsernameFromEmail("!!!@example.com"));
        Assert.IsTrue(CredentialsValidator.IsValidUsername(CredentialsValidator.DeriveUsernameFromEmail("ok.user@example.com")));
    }

    [Test]
    public void WithUsernameRetrySuffix_StaysWithinUsernameRules()
    {
        var result = CredentialsValidator.WithUsernameRetrySuffix("abcdefghijklmnopqrstuvwxyz");
        Assert.LessOrEqual(result.Length, 20);
        Assert.GreaterOrEqual(result.Length, 3);
        Assert.IsTrue(CredentialsValidator.IsValidUsername(result));
    }

    [Test]
    public void IsValidToSignUp_RequiresFieldsCheckboxesAndDerivedUsername()
    {
        Assert.IsFalse(CredentialsValidator.IsValidToSignUp("player.one@example.com", "ValidPass1!", "ValidPass1!", false, true));
        Assert.IsFalse(CredentialsValidator.IsValidToSignUp("player.one@example.com", "ValidPass1!", "ValidPass1!", true, false));
        Assert.IsFalse(CredentialsValidator.IsValidToSignUp("bad", "ValidPass1!", "ValidPass1!", true, true));
        Assert.IsFalse(CredentialsValidator.IsValidToSignUp("player.one@example.com", "ValidPass1!", "Mismatch1!", true, true));
        Assert.IsTrue(CredentialsValidator.IsValidToSignUp("player.one@example.com", "ValidPass1!", "ValidPass1!", true, true));
    }

    [Test]
    public void ToDisplayError_StripsRichTextAndFallsBack()
    {
        Assert.AreEqual("Username is already taken", AuthManager.ToDisplayError("<b>*</b> Username is already taken"));
        Assert.AreEqual(
            "Something went wrong while creating your account. Please try again.",
            AuthManager.ToDisplayError(null));
    }
}
