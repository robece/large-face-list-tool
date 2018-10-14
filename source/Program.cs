using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FaceClientSDK;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using FaceClientSDK.Domain.LargeFaceList;

namespace LargeFaceListTool
{
    class Program
    {
        private static TelemetryClient telemetryClient = null;

        private static void InitConfiguration()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddIniFile("config.ini", optional: false, reloadOnChange: true)
               .Build();

            Settings.AzureWebJobsStorage = config.GetSection("general:AzureWebJobsStorage").Value;
            Settings.FaceAPIKey = config.GetSection("general:FaceAPIKey").Value;
            Settings.FaceAPIZone = config.GetSection("general:FaceAPIZone").Value;
            Settings.AppInsightsKey = config.GetSection("general:AppInsightsKey").Value;
            Settings.ImageFolderPath = config.GetSection("general:ImageFolderPath").Value;
            Settings.FindSimilarFolderPath = config.GetSection("general:FindSimilarFolderPath").Value;
            Settings.AddFaceRetries = Convert.ToInt32(config.GetSection("general:AddFaceRetries").Value);
            Settings.AddFaceTimeToSleepInMs = Convert.ToInt32(config.GetSection("general:AddFaceTimeToSleepInMs").Value);

            APIReference.FaceAPIKey = Settings.FaceAPIKey;
            APIReference.FaceAPIZone = Settings.FaceAPIZone;
        }

        static void Main(string[] args) 
        {
            InitConfiguration();

            telemetryClient = new TelemetryClient { InstrumentationKey = Settings.AppInsightsKey };
            int option = 0; 

            do 
            { 
                try
                {
                    Console.WriteLine($"Settings required:");

                    var tLargeFaceListId = (Settings.LargeFaceListId == string.Empty) ? "PENDING CONFIGURATION!" : Settings.LargeFaceListId;
                    Console.WriteLine($"- LargeFaceListId: {tLargeFaceListId}");

                    Console.WriteLine();
                    Console.WriteLine("[ 1 ] Create large face list");
                    Console.WriteLine("[ 2 ] Assign large face list");
                    Console.WriteLine("[ 3 ] List of large face list");
                    Console.WriteLine("[ 4 ] Delete all large face lists");          
                    Console.WriteLine("[ 5 ] Add faces to large face list");
                    Console.WriteLine("[ 6 ] Train large face list");
                    Console.WriteLine("[ 7 ] Find similar faces in all large lists");
                    Console.WriteLine("[ 0 ] Terminate"); 
                    Console.WriteLine("-------------------------------------"); 
                    Console.Write("Select an option: "); 
                    option = Int32.Parse(Console.ReadLine()); 
                    switch (option) 
                    {
                        case 1: 
                            CreateLargeFaceListAsync();                         
                            break; 
                        case 2: 
                            AssignLargeFaceList();                         
                            break; 
                        case 3: 
                            ListOfLargeFaceListAsync();                         
                            break;
                        case 4: 
                            DeleteAllLargeFaceListsAsync();                         
                            break;                 
                        case 5: 
                            ValidateAddFaceToLargeFaceList();                         
                            break; 
                        case 6:
                            TrainLargeFaceList();
                            break;
                        case 7:
                            ValidateFindSimilarFacesInAllLargeFaceLists();
                            break;
                        default: 
                            Terminate(); 
                            break; 
                    }   
                }
                catch (System.Exception)
                {
                   Console.WriteLine($"Invalid option");
                   option = 1;
                } 
                finally{
                    Console.ReadKey(); 
                    Console.Clear(); 
                }
            } 
            while (option != 0); 
        }

        private async static void CreateLargeFaceListAsync() 
        { 
            var id = $"{Guid.NewGuid().ToString()}";
            var largeFaceListId = id;
            var name = id;
            var userData = id;
            bool result = await APIReference.Instance.LargeFaceList.CreateAsync(id, name, userData);

            if (result)
            {
                Settings.LargeFaceListId = id;
                Console.WriteLine($"Assigned LargeFaceList: {id}");  
            }
            
            Console.WriteLine($"Task ended");
        } 
 
        private static void AssignLargeFaceList() 
        { 
            Console.WriteLine($"LargeFaceListId: ");
            string id = Console.ReadLine();
            Settings.LargeFaceListId = id;
            Console.WriteLine($"Assigned LargeFaceList: {id}");
            
            Console.WriteLine($"Task ended");
        } 

        private async static void ListOfLargeFaceListAsync() 
        {
            List<ListResult> faceList = await APIReference.Instance.LargeFaceList.ListAsync("0","1000");
            foreach(ListResult lfl in faceList)
                Console.WriteLine($"{lfl.largeFaceListId}");

            Console.WriteLine($"Task ended");
        } 

        private async static void DeleteAllLargeFaceListsAsync() 
        {
            List<ListResult> faceList = await APIReference.Instance.LargeFaceList.ListAsync("0", "1000");
            foreach (ListResult lfl in faceList)
            {
                bool res = await APIReference.Instance.LargeFaceList.DeleteAsync(lfl.largeFaceListId);
                if (res)
                    Console.WriteLine($"{lfl.largeFaceListId} - Deleted!");
            }
            
            Console.WriteLine($"Task ended");
        } 

        private static void ValidateAddFaceToLargeFaceList() 
        {
            Console.WriteLine($"Confirm you want to add faces to large face list with the following settings: ");

            var tLargeFaceListId = (Settings.LargeFaceListId == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.LargeFaceListId;
            Console.WriteLine($"- LargeFaceListId: {tLargeFaceListId}");

            var tImageFolderPath = (Settings.ImageFolderPath == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.ImageFolderPath;
            Console.WriteLine($"- ImageFolderPath: {tImageFolderPath}");

            Console.WriteLine("Type [ yes | no ] to confirm:");
            string res = Console.ReadLine().ToLowerInvariant();
            if (res == "yes")
            {
                Console.WriteLine("Adding faces");
                AddFaceToLargeFaceListAsync();
            }
            else
            {
                Console.WriteLine("Operation not confirmed");
            }

            Console.WriteLine($"Task ended");
        }

        private static async void AddFaceToLargeFaceListAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var processed = 0;
            var IterationsToRetry = Settings.AddFaceRetries;
            var TimeToSleepForRetry = Settings.AddFaceTimeToSleepInMs;

            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("Add face to large face list"))
            {
                try
                {
                    if (!Directory.Exists(Settings.ImageFolderPath))
                        throw new Exception("There was a problem validating the images folder");

                    List<string> imageList = Directory.GetFiles(Settings.ImageFolderPath, "*.jpg").ToList();

                    telemetryClient.TrackTrace($"ImageFolderPath: {Settings.ImageFolderPath}");

                    foreach (string s in imageList)
                    {
                        for (int i = 0; i <= IterationsToRetry; i++)
                        {
                            FileInfo imageFile = null;

                            try
                            {
                                imageFile = new FileInfo(s);

                                var noExtension = imageFile.Name.Replace(imageFile.Extension, "");

                                Metadata metadata = new Metadata() { id = noExtension };

                                var imageBytes = File.ReadAllBytes(imageFile.FullName);
                                var stream = new MemoryStream(imageBytes);
                                var imageUri = await StorageHelper.UploadFileAsync(stream, $"{imageFile.Name}", "images", Settings.AzureWebJobsStorage, "image/jpeg");

                                AddFaceResult resultFaceToList = await APIReference.Instance.LargeFaceList.AddFaceAsync(Settings.LargeFaceListId, imageUri, metadata.id, string.Empty);

                                if (resultFaceToList == null)
                                    return;

                                processed++;

                                Console.WriteLine($"Processed: {processed}, PersistedFaceId: {resultFaceToList.persistedFaceId}");
                                telemetryClient.TrackTrace($"Processed: {processed}, PersistedFaceId: {resultFaceToList.persistedFaceId}");

                                break;
                            }
                            catch (Exception ex)
                            {
                                var message = string.Empty;
                                if (i > 0)
                                {
                                    message = $"Retry #{i} in image: {imageFile.Name}, Exception: {ex.Message}";
                                }
                                else
                                {
                                    message = $"There was an error in image: {imageFile.Name}, Exception: {ex.Message}";
                                }

                                Console.WriteLine(message);

                                List<Exception> exs = new List<Exception>();
                                exs.Add(ex);
                                AggregateException aex = new AggregateException(message,exs);
                                telemetryClient.TrackException(aex);
                                Task.Delay(TimeToSleepForRetry).Wait();
                            }
                        }
                    }

                    Console.WriteLine($"Processed: {processed}");
                    operation.Telemetry.Properties["ProcessedFaces"] = processed.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    telemetryClient.TrackException(ex);
                }

                stopwatch.Stop();
                telemetryClient.TrackEvent($"Add face to large face list - completed at {stopwatch.ElapsedMilliseconds} ms.");

                telemetryClient.StopOperation(operation);
            }
        }

        private async static void TrainLargeFaceList()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timeIntervalInMilliseconds = 3000;
            GetTrainingStatusResult status = null;

            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("Train large face list"))
            {
                try
                {
                    operation.Telemetry.Properties["LargeFaceListId"] = Settings.LargeFaceListId;
                    operation.Telemetry.Properties["Interval"] = $"{timeIntervalInMilliseconds} ms.";

                    await APIReference.Instance.LargeFaceList.TrainAsync(Settings.LargeFaceListId);

                    while (true)
                    {
                        Console.WriteLine($"Working");
                        Task.Delay(timeIntervalInMilliseconds).Wait();
                        status = await APIReference.Instance.LargeFaceList.GetTrainingStatusAsync(Settings.LargeFaceListId);

                        if (status.status == "running")
                        {
                            Console.WriteLine($"{status.status}");
                            telemetryClient.TrackTrace($"{status.status}");
                            continue;
                        }
                        else if (status.status == "succeeded")
                        {
                            Console.WriteLine($"{status.status}");
                            telemetryClient.TrackTrace($"{status.status}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"There was an error training the large face list, status: {status.status}");
                            telemetryClient.TrackTrace($"There was an error training the large face list, status: {status.status}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    telemetryClient.TrackException(ex);
                }

                operation.Telemetry.Properties["Status"] = status.status;

                stopwatch.Stop();
                telemetryClient.TrackEvent($"Train large face list - completed at {stopwatch.ElapsedMilliseconds} ms.");

                telemetryClient.StopOperation(operation);
            }

            Console.WriteLine($"Task ended");
        }

        private static void ValidateFindSimilarFacesInAllLargeFaceLists() 
        {
            Console.WriteLine($"Confirm you want to find similar faces in a large face list with the following settings: ");

            var tFindSimilarFolderPath = (Settings.FindSimilarFolderPath == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.FindSimilarFolderPath;
            Console.WriteLine($"- FindSimilarFolderPath: {tFindSimilarFolderPath}");

            Console.WriteLine("Type [ yes | no ] to confirm:");
            string res = Console.ReadLine().ToLowerInvariant();
            if (res == "yes")
            {
                Console.WriteLine("Finding similar faces");
                FindSimilarFacesInAllLargeFaceListsAsync();
            }
            else
            {
                Console.WriteLine("Operation not confirmed");
            }

            Console.WriteLine($"Task ended");
        }

        private static void FindSimilarFacesInAllLargeFaceListsAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var processed = 0;

            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("Find similar faces in all large face lists"))
            {
                try
                {
                    var filePath = Path.Combine(Settings.FindSimilarFolderPath, "input.jpg");
                    var imageBytes = File.ReadAllBytes(filePath);
                    var stream = new System.IO.MemoryStream(imageBytes);
                    var imageUri = StorageHelper.UploadFileAsync(stream, $"input.jpg", "uploads", Settings.AzureWebJobsStorage, "image/jpeg").Result;

                    List<FaceClientSDK.Domain.Face.DetectResult> list = APIReference.Instance.Face.DetectAsync(imageUri, "age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise", true, true).Result;
                    bool isValid = true;

                    if (list.Count == 0)
                    {
                        isValid = false;

                        //delete file from storage
                        var res = StorageHelper.DeleteFileAsync($"input.jpg", "uploads", Settings.AzureWebJobsStorage).Result;

                        Console.WriteLine($"no face detected!");
                        telemetryClient.TrackTrace($"no face detected!");
                    }

                    if (list.Count > 1)
                    {
                        isValid = false;

                        //delete file from storage
                        var res = StorageHelper.DeleteFileAsync($"input.jpg", "uploads", Settings.AzureWebJobsStorage).Result;

                        Console.WriteLine($"multiple faces detected!");
                        telemetryClient.TrackTrace($"multiple faces detected!");
                    }

                    if (isValid)
                    {
                        var detectedFaceId = list.First().faceId.ToString();

                        List<FaceClientSDK.Domain.Face.FindSimilarResult> facesFoundInAllFaceLists = new List<FaceClientSDK.Domain.Face.FindSimilarResult>();
                        List<ListResult> faceList = APIReference.Instance.LargeFaceList.ListAsync("0","1000").Result;

                        if (Parallel.ForEach(faceList, (s) => {
                            try
                            {
                                Settings.LargeFaceListId = s.largeFaceListId;
                                Console.WriteLine($"Find similar faces in LargeFaceListId: {s.largeFaceListId}");
                                telemetryClient.TrackTrace($"Find similar faces in LargeFaceListId: {s.largeFaceListId}");

                                List<FaceClientSDK.Domain.Face.FindSimilarResult> similarFaces = APIReference.Instance.Face.FindSimilarAsync(detectedFaceId, string.Empty, Settings.LargeFaceListId, new string[] { }, 10, "matchPerson").Result;

                                foreach (FaceClientSDK.Domain.Face.FindSimilarResult fs in similarFaces)
                                {
                                    facesFoundInAllFaceLists.Add(fs);
                                }

                                processed++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                telemetryClient.TrackException(ex);
                                return;
                            }
                        }).IsCompleted)
                        {
                            if (Parallel.ForEach(facesFoundInAllFaceLists, (fs) => {
                                try
                                {
                                    GetFaceResult face = APIReference.Instance.LargeFaceList.GetFaceAsync(Settings.LargeFaceListId, fs.persistedFaceId).Result;

                                    Console.WriteLine($"PersistedFaceId: {fs.persistedFaceId}, UserData: {face.userData}, Confidence: {fs.confidence}");
                                    telemetryClient.TrackTrace($"PersistedFaceId: {fs.persistedFaceId}, UserData: {face.userData}, Confidence: {fs.confidence}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    telemetryClient.TrackException(ex);
                                    return;
                                }
                            }).IsCompleted)
                            {
                                Console.WriteLine($"Processed lists: {processed}");
                                operation.Telemetry.Properties["ProcessedLists"] = processed.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    telemetryClient.TrackException(ex);
                }

                stopwatch.Stop();
                telemetryClient.TrackEvent($"Find similar faces in all large face lists - completed at {stopwatch.ElapsedMilliseconds} ms.");

                telemetryClient.StopOperation(operation);
            }
        }

        private static void Terminate() 
        { 
            Console.WriteLine(); 
            Console.WriteLine("Thanks for using large face list tool..."); 
        } 
    }
}
