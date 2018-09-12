namespace LargeFaceListTool
{
    public class Settings
    {
        public static string AzureWebJobsStorage { get; set; } = "";
        public static string FaceAPIKey { get; set; } = "";
        public static string AppInsightsKey { get; set; } = "";
        public static string Zone { get; set; } = "";
        public static string LargePersonGroupId { get; set; } = "";
        public static string LargeFaceListId { get; set; } = "";
        public static string ImageFolderPath { get; set; } = "";
        public static string MetadataFolderPath { get; set; } = "";
        public static string FindSimilarFolderPath { get; set; } = "";
    }
}