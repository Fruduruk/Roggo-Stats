using Discord_Bot.ExtensionMethods;

namespace Discord_Bot.Modules.RLStats.RecurringReports
{
    public static class RecurringReportsConstants
    {
        public const string CorruptedConfigMessage = "The config for this subscription is corrupted. This subscription process will be deleted. Start a new one.";

        public const string SubStepOneMessage = "You have started the subscription process.\nI'm going to lead you through the configuration.\n\nHow often do you want to get notified?\nPossible answers: d,w,m,y";

        public static string SubStepTwoMessage(string time) => $"You have chosen this time: {time.Adverbify()}" +
            $"\n\nWho are the players?\nNames or steam ids separated by comma";

        public static string SubStepThreeMessage(string[] namesAndIds) => $"You have chosen these names and steam ids: {string.Join(' ', namesAndIds)}" +
            $"\n\nOnly show stats where these players played together?\nPossible answers: y/n";

        public static string SubStepFourMessage(bool together) => $"You have chosen: {together}" +
            $"\n\n";

        public const string ExecuteSubStepOne = "proceed to subscription step one";
        public const string ExecuteSubStepTwo = "proceed to subscription step two";
        public const string ExecuteSubStepThree = "proceed to subscription step three";
        public const string ExecuteSubStepFour = "proceed to subscription step four";
        public const string ExecuteSubStepFive = "proceed to subscription step five";
        public const string ExecuteSubStepSix = "proceed to subscription step six";
    }

}