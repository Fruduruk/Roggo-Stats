using Discord_Bot.ExtensionMethods;

namespace Discord_Bot.Modules.RLStats.RecurringReports
{
    public static class RecurringReportsConstants
    {
        public const string CorruptedConfigMessage = "The config for this subscription is corrupted. This subscription process will be deleted. Start a new one.";
        public const string ProcessCanceledMessage = "The subscription process is successfully canceled.";
        #region Step messages
        public const string SubStepTimeMessage = "You have started the subscription process.\nI'm going to lead you through the configuration." +
            "\nTo proceed with this command begin with -p {answer}." +
            "\nTo cancel the process type in -p cancel" +
            "\n\nHow often do you want to get notified?\nPossible answers: d,w,m,y";

        public static string SubStepCompareMessage => "Do you want to compare it to last time?";
        public static string SubStepNamesMessage => "Who are the players?\nNames or steam ids separated by comma. No whitespaces.";
        public static string SubStepTogetherMessage => "Only show stats where these players played together?\nPossible answers: y/n";
        public static string SubStepSkipTogetherMessage => "Now you need to choose the stats you want to see.\nJust type in all the numbers separated by comma. No whitespaces.\nIf you want all of them just type in all.";
        public static string SubStepIndexesMessage => "Now you need to choose the stats you want to see.\nJust type in all the numbers separated by comma. No whitespaces.\nIf you want all of them just type in all.";
        #endregion

        public const string ExecuteSubStepTime = "proceed to subscription step time";
        public const string ExecuteSubStepNames = "proceed to subscription step names";
        public const string ExecuteSubStepTogether = "proceed to subscription step together";
        public const string ExecuteSubStepIndexes = "proceed to subscription step indexes";
        public const string ExecuteSubStepCompare = "proceed to subscription step compare";
    }

}