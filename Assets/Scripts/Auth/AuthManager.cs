using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HelperSharedLibrary;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

namespace ProDomino.Authentication
{
    [Serializable]
    public class SignUpCredentialsPayload
    {
        public string username;
        public string email;
        public string password;
    }

    public readonly struct SignUpResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public bool UsernameTaken { get; }

        public SignUpResult(bool success, string errorMessage = null, bool usernameTaken = false)
        {
            Success = success;
            ErrorMessage = errorMessage;
            UsernameTaken = usernameTaken;
        }

        public static SignUpResult Ok() => new SignUpResult(true);
        public static SignUpResult Fail(string message, bool usernameTaken = false) => new SignUpResult(false, message, usernameTaken);
    }

    /// <summary>
    /// Credentials signup from client-old-code, without the old AuthUI prefab.
    /// UGS anonymous session → encrypted Cloud Code SignUpWithCredentials → local username/password sign-in.
    /// Firebase JSLib session is skipped on this UI branch (same fallback as the old non-WebGL path).
    /// </summary>
    public static class AuthManager
    {
        public static bool IsAlreadyInitialized => UnityServices.State == ServicesInitializationState.Initialized;
        public static bool IsUGSAuthenticated => IsAlreadyInitialized && AuthenticationService.Instance.IsSignedIn;

        public static bool IsUserAuthenticatedWithCredentials =>
            IsUGSAuthenticated && !AuthenticationService.Instance.IsAnonymous;

        public static async Task EnsureInitializedAsync()
        {
            if (IsAlreadyInitialized)
                return;

            await UnityServices.InitializeAsync();
        }

        public static async Task<SignUpResult> SignUpWithCredentialsAsync(string username, string email, string password)
        {
            var first = await SignUpOnceAsync(username, email, password);
            if (first.Success || !first.UsernameTaken)
                return first;

            var retryUsername = CredentialsValidator.WithUsernameRetrySuffix(username);
            if (!CredentialsValidator.IsValidUsername(retryUsername) || retryUsername == username)
                return first;

            Debug.Log($"SignUp username '{username}' taken, retrying as '{retryUsername}'");
            return await SignUpOnceAsync(retryUsername, email, password);
        }

        private static async Task<SignUpResult> SignUpOnceAsync(string username, string email, string password)
        {
            try
            {
                await EnsureInitializedAsync();

                if (!IsAlreadyInitialized)
                    return SignUpResult.Fail("Unity services are not initialized.");

                if (IsUserAuthenticatedWithCredentials)
                    return SignUpResult.Fail("Already authenticated with credentials.");

                if (!IsUGSAuthenticated)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    if (!IsUGSAuthenticated)
                        return SignUpResult.Fail("Failed to sign in anonymously.");
                }

                var playerId = AuthenticationService.Instance.PlayerId;
                var accessToken = AuthenticationService.Instance.AccessToken;
                var derivedKey = SecurityHelper.DeriveKey(playerId, accessToken);

                var payload = new SignUpCredentialsPayload
                {
                    username = username,
                    email = email,
                    password = password
                };

                var parametersJson = JsonUtility.ToJson(payload);
                var encryptedParameters = SecurityHelper.EncryptData(parametersJson, derivedKey);
                var encryptedParametersJson = JsonUtility.ToJson(encryptedParameters);

                await CloudCodeService.Instance.CallModuleEndpointAsync(
                    "Backend",
                    "SignUpWithCredentials",
                    new Dictionary<string, object>
                    {
                        { "parametersEncryptedJson", encryptedParametersJson }
                    });

                if (AuthenticationService.Instance.IsSignedIn)
                    AuthenticationService.Instance.SignOut();

                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

                Debug.Log("<color=cyan><b>SignUp</b> was completed successfully</color>");
                return SignUpResult.Ok();
            }
            catch (CloudCodeException exception)
            {
                var message = ParseCloudCodeMessage(exception);
                Debug.LogWarning($"SignUp failed Cloud Code: {exception.Message}");
                return SignUpResult.Fail(message, IsUsernameTakenMessage(message));
            }
            catch (AuthenticationException exception)
            {
                Debug.LogWarning($"SignUp failed UGS: {exception.Message}");
                return SignUpResult.Fail($"<b>*</b> {exception.Message}", IsUsernameTakenMessage(exception.Message));
            }
            catch (RequestFailedException exception)
            {
                Debug.LogWarning($"SignUp failed UGS request: {exception.Message}");
                return SignUpResult.Fail($"<b>*</b> {exception.Message}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SignUp failed Basic Exception: {exception.Message}");
                return SignUpResult.Fail("<b>*</b> Unknow exception.");
            }
        }

        private static string ParseCloudCodeMessage(CloudCodeException exception)
        {
            var message = $"Unknow exception. Error Code: {exception.ErrorCode}";
            var match = Regex.Match(exception.Message ?? string.Empty, @"Exception type: (\w+).*?Message: (.*)");
            if (match.Success)
            {
                var exceptionType = match.Groups[1].Value;
                var exceptionMessageExtracted = match.Groups[2].Value;
                if (exceptionType == "UGSException" || exceptionType == "FirebaseException")
                    message = $"<b>*</b> {exceptionMessageExtracted}";
            }

            return message;
        }

        private static bool IsUsernameTakenMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            if (message.IndexOf("USERNAME_EXISTS", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var mentionsUsername = message.IndexOf("username", StringComparison.OrdinalIgnoreCase) >= 0;
            var takenOrExists = message.IndexOf("already taken", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0;
            return mentionsUsername && takenOrExists;
        }

        public static string ToDisplayError(string message)
        {
            const string fallback = "Something went wrong while creating your account. Please try again.";
            if (string.IsNullOrWhiteSpace(message))
                return fallback;

            var stripped = Regex.Replace(message, "<.*?>", string.Empty).Trim();
            stripped = stripped.TrimStart('*').Trim();
            return string.IsNullOrEmpty(stripped) ? fallback : stripped;
        }
    }
}
