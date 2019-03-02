using System;
using System.Linq;
using System.Text.RegularExpressions;
using Steam4NET;

namespace SteamBulkActivatorCLI
{
    public enum EPurchaseResultDetail : int
    {
        k_EPurchaseResultNoDetail = 0,
        k_EPurchaseResultAVSFailure = 1,
        k_EPurchaseResultInsufficientFunds = 2,
        k_EPurchaseResultContactSupport = 3,
        k_EPurchaseResultTimeout = 4,
        k_EPurchaseResultInvalidPackage = 5,
        k_EPurchaseResultInvalidPaymentMethod = 6,
        k_EPurchaseResultInvalidData = 7,
        k_EPurchaseResultOthersInProgress = 8,
        k_EPurchaseResultAlreadyPurchased = 9,
        k_EPurchaseResultWrongPrice = 10,
        k_EPurchaseResultFraudCheckFailed = 11,
        k_EPurchaseResultCancelledByUser = 12,
        k_EPurchaseResultRestrictedCountry = 13,
        k_EPurchaseResultBadActivationCode = 14,
        k_EPurchaseResultDuplicateActivationCode = 15,
        k_EPurchaseResultUseOtherPaymentMethod = 16,
        k_EPurchaseResultUseOtherFundingSource = 17,
        k_EPurchaseResultInvalidShippingAddress = 18,
        k_EPurchaseResultRegionNotSupported = 19,
        k_EPurchaseResultAcctIsBlocked = 20,
        k_EPurchaseResultAcctNotVerified = 21,
        k_EPurchaseResultInvalidAccount = 22,
        k_EPurchaseResultStoreBillingCountryMismatch = 23,
        k_EPurchaseResultDoesNotOwnRequiredApp = 24,
        k_EPurchaseResultCanceledByNewTransaction = 25,
        k_EPurchaseResultForceCanceledPending = 26,
        k_EPurchaseResultFailCurrencyTransProvider = 27,
        k_EPurchaseResultFailedCyberCafe = 28,
        k_EPurchaseResultNeedsPreApproval = 29,
        k_EPurchaseResultPreApprovalDenied = 30,
        k_EPurchaseResultWalletCurrencyMismatch = 31,
        k_EPurchaseResultEmailNotValidated = 32,
        k_EPurchaseResultExpiredCard = 33,
        k_EPurchaseResultTransactionExpired = 34,
        k_EPurchaseResultWouldExceedMaxWallet = 35,
        k_EPurchaseResultMustLoginPS3AppForPurchase = 36,
        k_EPurchaseResultCannotShipToPOBox = 37,
        k_EPurchaseResultTooManyActivationAttempts = 53
    };

    public class Utils
    {
        public static Random Random = new Random();

        public static string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-M-d HH-mm-ss");
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static string GetRandomCDKey()
        {
            return $"{RandomString(5)}-{RandomString(5)}-{RandomString(5)}";
        }

        public static string GetFriendlyEPurchaseResultDetailMsg(EPurchaseResultDetail result)
        {
            switch(result)
            {
                case EPurchaseResultDetail.k_EPurchaseResultNoDetail:
                    return "Success";

                case EPurchaseResultDetail.k_EPurchaseResultAlreadyPurchased:
                    return "Already registered";

                case EPurchaseResultDetail.k_EPurchaseResultRestrictedCountry:
                    return "Key restricted country";

                case EPurchaseResultDetail.k_EPurchaseResultBadActivationCode:
                    return "Bad activation key";

                case EPurchaseResultDetail.k_EPurchaseResultDuplicateActivationCode:
                    return "Duplicate activation code";

                case EPurchaseResultDetail.k_EPurchaseResultRegionNotSupported:
                    return "Region not supported";

                case EPurchaseResultDetail.k_EPurchaseResultInvalidAccount:
                    return "Invalid account";

                case EPurchaseResultDetail.k_EPurchaseResultDoesNotOwnRequiredApp:
                    return "Does not own required app to register this key";

                case EPurchaseResultDetail.k_EPurchaseResultTooManyActivationAttempts:
                    return "Too many activation attempts. Try again later.";

                default:
                    return result.ToString();
            }
        }
    }
}
