using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HelperSharedLibrary
{
    public static class CredentialsValidator
    {
        public static readonly char[] UsernameAllowedSymbols = { '.', '-', '@', '_' };

        private static HashSet<string> tempEmailDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void SetBlockedEmailDomains(IEnumerable<string> domains)
        {
            tempEmailDomains = domains != null
                ? new HashSet<string>(domains, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static void BasicValidation(string toValidate, ref CredentialsDenyState refCredentialsDenyState, int minCharLenght, int maxCharLength = 20)
        {
            refCredentialsDenyState = CredentialsDenyState.IsValid;

            if (string.IsNullOrEmpty(toValidate))
                refCredentialsDenyState |= CredentialsDenyState.IsEmpty;

            if (toValidate == null)
                return;

            if (toValidate.Length < minCharLenght)
                refCredentialsDenyState |= CredentialsDenyState.IsTooShort;

            if (toValidate.Length > maxCharLength)
                refCredentialsDenyState |= CredentialsDenyState.IsTooLong;
        }

        public static bool IsValidUsername(string username, ref CredentialsDenyState refCredentialsDenyState)
        {
            refCredentialsDenyState = CredentialsDenyState.IsValid;
            BasicValidation(username, ref refCredentialsDenyState, 3, 20);

            if (refCredentialsDenyState.HasFlag(CredentialsDenyState.IsEmpty))
                return false;

            if (username.Any(x => !char.IsLetter(x) && !char.IsNumber(x) && !UsernameAllowedSymbols.Contains(x)))
                refCredentialsDenyState |= CredentialsDenyState.IsContainingInvalidChars;

            return refCredentialsDenyState == CredentialsDenyState.IsValid;
        }

        public static bool IsValidUsername(string username)
        {
            var tempCredentialsDenyState = CredentialsDenyState.IsValid;
            return IsValidUsername(username, ref tempCredentialsDenyState);
        }

        public static bool IsValidEmail(string email, ref CredentialsDenyState refCredentialsDenyState)
        {
            refCredentialsDenyState = CredentialsDenyState.IsValid;
            email = email ?? string.Empty;

            var localPart = email.Contains('@') ? email.Split('@')[0] : email;
            var domain = email.Contains('@') ? email.Split('@')[1] : string.Empty;

            BasicValidation(localPart, ref refCredentialsDenyState, 3, 64);

            if (refCredentialsDenyState.HasFlag(CredentialsDenyState.IsEmpty))
                return false;

            if (!email.Contains('@'))
                refCredentialsDenyState |= CredentialsDenyState.IsLeftingRequiredChars;

            var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.Match(email, pattern).Success)
                refCredentialsDenyState |= CredentialsDenyState.IsNotUsingEmailStructure;

            if (tempEmailDomains.Contains(domain))
                refCredentialsDenyState |= CredentialsDenyState.IsNotValidDomain;

            return refCredentialsDenyState == CredentialsDenyState.IsValid;
        }

        public static bool IsValidEmail(string email)
        {
            var tempCredentialsDenyState = CredentialsDenyState.IsValid;
            return IsValidEmail(email, ref tempCredentialsDenyState);
        }

        public static bool IsValidPassword(string password, ref CredentialsDenyState refCredentialsDenyState)
        {
            refCredentialsDenyState = CredentialsDenyState.IsValid;
            password = password ?? string.Empty;
            BasicValidation(password, ref refCredentialsDenyState, 8, 30);

            bool hasSymbol = password.Any(char.IsSymbol);
            bool hasPunctuation = password.Any(char.IsPunctuation);
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);

            if (!hasLower || !hasUpper || !(hasSymbol || hasPunctuation))
                refCredentialsDenyState |= CredentialsDenyState.IsLeftingRequiredChars;

            return refCredentialsDenyState == CredentialsDenyState.IsValid;
        }

        public static bool IsValidPassword(string password)
        {
            var tempCredentialsDenyState = CredentialsDenyState.IsValid;
            return IsValidPassword(password, ref tempCredentialsDenyState);
        }

        public static bool IsValidConfirmPassword(string confirmPassword, string password, ref CredentialsDenyState refCredentialsDenyState)
        {
            refCredentialsDenyState = CredentialsDenyState.IsValid;

            if (!string.Equals(password, confirmPassword))
                refCredentialsDenyState |= CredentialsDenyState.IsNotMatching;

            return refCredentialsDenyState == CredentialsDenyState.IsValid;
        }

        public static bool IsValidConfirmPassword(string confirmPassword, string password)
        {
            var tempCredentialsDenyState = CredentialsDenyState.IsValid;
            return IsValidConfirmPassword(confirmPassword, password, ref tempCredentialsDenyState);
        }

        /// <summary>
        /// Registration UI has no username field. Username is derived from the email local-part.
        /// </summary>
        public static bool IsValidToSignUp(string email, string password, string confirmPassword, bool acceptedTerms, bool acceptedDataTreatment)
        {
            if (!acceptedTerms || !acceptedDataTreatment)
                return false;

            if (!IsValidEmail(email) || !IsValidPassword(password) || !IsValidConfirmPassword(confirmPassword, password))
                return false;

            return IsValidUsername(DeriveUsernameFromEmail(email));
        }

        public static string DeriveUsernameFromEmail(string email)
        {
            var localPart = string.IsNullOrEmpty(email)
                ? string.Empty
                : (email.Contains('@') ? email.Split('@')[0] : email);

            var builder = new StringBuilder();
            foreach (var character in localPart)
            {
                if (char.IsLetterOrDigit(character) || UsernameAllowedSymbols.Contains(character))
                    builder.Append(character);
            }

            var username = builder.ToString();
            if (username.Length > 20)
                username = username.Substring(0, 20);

            if (username.Length < 3)
                username = (username + "user").Substring(0, 3);

            return username;
        }

        public static string WithUsernameRetrySuffix(string username)
        {
            var prefix = username ?? string.Empty;
            if (prefix.Length > 16)
                prefix = prefix.Substring(0, 16);

            var suffix = UnityEngine.Random.Range(1000, 10000).ToString();
            return prefix + suffix;
        }

        [Flags]
        public enum CredentialsDenyState
        {
            IsValid = 0,
            IsEmpty = 1 << 0,
            IsTooShort = 1 << 1,
            IsTooLong = 1 << 2,
            IsContainingInvalidChars = 1 << 3,
            IsLeftingRequiredChars = 1 << 4,
            IsNotUsingEmailStructure = 1 << 5,
            IsNotValidDomain = 1 << 6,
            IsNotMatching = 1 << 7,
        }
    }
}
