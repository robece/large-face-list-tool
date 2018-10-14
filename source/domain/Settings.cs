namespace LargeFaceListTool
{
    public class Settings
    {
        public static string AzureWebJobsStorage { get; set; } = "";
        public static string FaceAPIKey { get; set; } = "";
        public static string FaceAPIZone { get; set; } = "";
        public static string AppInsightsKey { get; set; } = "";
        public static string LargeFaceListId { get; set; } = "";
        public static string ImageFolderPath { get; set; } = "";
        public static string FindSimilarFolderPath { get; set; } = "";
        public static int AddFaceRetries { get; set; } = 0;
        public static int AddFaceTimeToSleepInMs { get; set; } = 0;
    }
}