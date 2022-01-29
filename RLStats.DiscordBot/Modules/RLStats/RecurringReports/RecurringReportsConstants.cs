using Discord_Bot.ExtensionMethods;

namespace Discord_Bot.Modules.RLStats.RecurringReports
{
    public static class RecurringReportsConstants
    {
        public const string CorruptedConfigMessage = "The config for this subscription is corrupted. This subscription process will be deleted. Start a new one.";
        public const string ProcessCanceledMessage = "The subscription process is successfully canceled.";
        #region Step messages
        public const string SubStepOneMessage = "You have started the subscription process.\nI'm going to lead you through the configuration." +
            "\nTo proceed with this command begin with -p {answer}." +
            "\nTo cancel the process type in -p cancel" +
            "\n\nHow often do you want to get notified?\nPossible answers: d,w,m,y";

        public static string SubStepTwoMessage(string time) => $"You have chosen this time: {time.Adverbify()}" +
            $"\n\nWho are the players?\nNames or steam ids separated by comma. No whitespaces.";

        public static string SubStepThreeMessage(string[] namesAndIds) => $"You have chosen these names and steam ids: {string.Join(' ', namesAndIds)}" +
            $"\n\nOnly show stats where these players played together?\nPossible answers: y/n";

        public static string SubStepFourMessage(bool together) => $"You have chosen: {together}" +
            $"\n\nNow you need to choose the stats you want to see.\nJust type in all the numbers separated by comma. No whitespaces.\nIf you want all of them just type in all.";
        #endregion

        /// <summary>
        /// Proceeding method remark
        /// </summary>
        public const string ProceedingMethod = "093wmasoiwej,,na9w3..pnfc_iosdnpvaiouebg";
        public const string ExecuteSubStepOne = "proceed to subscription step one";
        public const string ExecuteSubStepTwo = "proceed to subscription step two";
        public const string ExecuteSubStepThree = "proceed to subscription step three";
        public const string ExecuteSubStepFour = "proceed to subscription step four";
    }

}