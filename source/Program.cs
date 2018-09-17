using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LargeFaceListTool
{
    class Program
    {
        private static TelemetryClient telemetryClient = null;

        static void Main(string[] args) 
        { 
            telemetryClient = new TelemetryClient { InstrumentationKey = Settings.AppInsightsKey };
            int option = 0; 

            do 
            { 
                try
                {
                    Console.WriteLine($"Settings required:");
                
                    var tLargeFaceListId = (Settings.LargeFaceListId == string.Empty) ? "PENDING CONFIGURATION!" : Settings.LargeFaceListId;
                    Console.WriteLine($"- LargeFaceListId: {tLargeFaceListId}");

                    var tImageFolderPath = (Settings.ImageFolderPath == string.Empty) ? "PENDING CONFIGURATION!" : Settings.ImageFolderPath;
                    Console.WriteLine($"- ImageFolderPath: {tImageFolderPath}");

                    var tMetadataFolderPath = (Settings.MetadataFolderPath == string.Empty) ? "PENDING CONFIGURATION!" : Settings.MetadataFolderPath;
                    Console.WriteLine($"- MetadataFolderPath: {tMetadataFolderPath}");

                    var tFindSimilarFolderPath = (Settings.FindSimilarFolderPath == string.Empty) ? "PENDING CONFIGURATION!" : Settings.FindSimilarFolderPath;
                    Console.WriteLine($"- FindSimilarFolderPath: {tFindSimilarFolderPath}");

                    Console.WriteLine();
                    Console.WriteLine("[ 1 ] Create large face list");
                    Console.WriteLine("[ 2 ] Assign large face list");
                    Console.WriteLine("[ 3 ] List of large face list");
                    Console.WriteLine("[ 4 ] Delete all large face lists");
                    Console.WriteLine("[ 5 ] Set ImageFolderPath, MetadataFolderPath and FindSimilarFolderPath for training");
                    Console.WriteLine("[ 6 ] Add faces to large face list");
                    Console.WriteLine("[ 7 ] Train large face list");
                    Console.WriteLine("[ 8 ] Find similar faces in all large lists");
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
                            SetParametersForTraining();                         
                            break; 
                        case 6: 
                            ValidateAddFaceToLargeFaceList();                         
                            break; 
                        case 7:
                            TrainLargeFaceList();
                            break;
                        case 8:
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
            bool result = await FaceHelper.LargeFaceList.CreateLargeFaceListAsync(largeFaceListId, name, userData);
            
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
            List<ListOfLargeFaceList> faceList = await FaceHelper.LargeFaceList.ListOfLargeFaceListAsync();
            foreach(ListOfLargeFaceList lfl in faceList)
                Console.WriteLine($"{lfl.largeFaceListId}");

            Console.WriteLine($"Task ended");
        } 

        private async static void DeleteAllLargeFaceListsAsync() 
        { 
            List<ListOfLargeFaceList> faceList = await FaceHelper.LargeFaceList.ListOfLargeFaceListAsync();
            foreach(ListOfLargeFaceList lfl in faceList)
            {
                bool res = await FaceHelper.LargeFaceList.DeleteLargeFaceListAsync(lfl.largeFaceListId);
                if (res)
                    Console.WriteLine($"{lfl.largeFaceListId} - Deleted!");
            }
            
            Console.WriteLine($"Task ended");
        } 

        private static void SetParametersForTraining() 
        {
            Console.WriteLine($"Image folder path: ");
            Settings.ImageFolderPath = Console.ReadLine();
            Console.WriteLine($"Assigned ImageFolderPath: {Settings.ImageFolderPath}");

            Console.WriteLine($"Metadata folder path: ");
            Settings.MetadataFolderPath = Console.ReadLine();
            Console.WriteLine($"Assigned MetadataFolderPath: {Settings.MetadataFolderPath}");

             Console.WriteLine($"FindSimilar folder path: ");
            Settings.FindSimilarFolderPath = Console.ReadLine();
            Console.WriteLine($"Assigned FindSimilarFolderPath: {Settings.FindSimilarFolderPath}");

            Console.WriteLine($"Task ended");
        }

        private static void ValidateAddFaceToLargeFaceList() 
        {
            Console.WriteLine($"Confirm you want to add faces to large face list with the following settings: ");

            var tLargeFaceListId = (Settings.LargeFaceListId == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.LargeFaceListId;
            Console.WriteLine($"- LargeFaceListId: {tLargeFaceListId}");

            var tImageFolderPath = (Settings.ImageFolderPath == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.ImageFolderPath;
            Console.WriteLine($"- ImageFolderPath: {tImageFolderPath}");

            var tMetadataFolderPath = (Settings.MetadataFolderPath == string.Empty) ? "PENDING TO CONFIGURE!" : Settings.MetadataFolderPath;
            Console.WriteLine($"- MetadataFolderPath: {tMetadataFolderPath}");

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

        private static void AddFaceToLargeFaceListAsync()
        {
            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("AddFaceToLargeFaceListAsync"))
            {
                try
                {
                    if(!Directory.Exists(Settings.ImageFolderPath))
                        throw new Exception("There was a problem validating the image folder");

                    if(!Directory.Exists(Settings.MetadataFolderPath))
                        throw new Exception("There was a problem validating the metadata folder");

                    List<string> imageList = Directory.GetFiles(Settings.ImageFolderPath,"*.jpg").ToList();
                    List<string> metadataList = Directory.GetFiles(Settings.MetadataFolderPath,"*.json").ToList();
                    
                    if(imageList.Count != metadataList.Count)
                        throw new Exception("There was a problem validating the correlation between the number of image and metadata files");

                    telemetryClient.TrackTrace($"ImageFolderPath: {Settings.ImageFolderPath}");
                    telemetryClient.TrackTrace($"MetadataFolderPath: {Settings.MetadataFolderPath}");

                    int processed = 0;
                    if(Parallel.ForEach(imageList, (s) => {

                        try
                        {
                            System.IO.FileInfo imageFile = new System.IO.FileInfo(s);

                            var noExtension = imageFile.Name.Replace(imageFile.Extension, "");
                            var metadataFileFullPath = metadataList.Find(x=>x.Contains($"{noExtension}.json"));
                            System.IO.FileInfo metadataFile = new System.IO.FileInfo(metadataFileFullPath);
                            
                            var json = System.IO.File.ReadAllTextAsync(metadataFile.FullName, new System.Threading.CancellationToken()).Result;
                            Metadata metadata = JsonConvert.DeserializeObject<Metadata>(json);

                            var imageBytes = File.ReadAllBytes(imageFile.FullName);
                            var stream = new System.IO.MemoryStream(imageBytes);
                            var imageUri = StorageHelper.UploadFileAsync(stream, $"{imageFile.Name}", "images", Settings.AzureWebJobsStorage, "image/jpeg").Result;

                            AddFaceToList resultFaceToList = FaceHelper.LargeFaceList.AddFaceToLargeFaceListAsync(imageUri, metadata.id).Result;

                            if (resultFaceToList == null)
                                return;
                            
                            Console.WriteLine($"PersistedFaceId: {resultFaceToList.persistedFaceId}");
                            telemetryClient.TrackTrace($"PersistedFaceId: {resultFaceToList.persistedFaceId}");
                            
                            processed++;
                        }
                        catch (System.IO.FileNotFoundException ex)
                        {
                            Console.WriteLine(ex.Message);
                            telemetryClient.TrackException(ex);
                            return;
                        }
                    }).IsCompleted)
                    {
                        Console.WriteLine($"Processed: {processed}");
                        telemetryClient.TrackTrace($"Processed: {processed}");
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    telemetryClient.TrackException(ex);
                }

                telemetryClient.StopOperation(operation);
            }
        }

        private async static void TrainLargeFaceList()
        {
            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("TrainLargeFaceList"))
            {
                int timeIntervalInMilliseconds = 5000;

                telemetryClient.TrackTrace($"LargeFaceListId: {Settings.LargeFaceListId}");
                telemetryClient.TrackTrace($"Interval: {timeIntervalInMilliseconds} ms");
                
                await FaceHelper.LargeFaceList.LargeFaceListTrainAsync();

                while (true)
                {
                    Console.WriteLine($"Working");
                    System.Threading.Tasks.Task.Delay(timeIntervalInMilliseconds).Wait();
                    var status = await FaceHelper.LargeFaceList.GetLargeFaceListTrainingStatusAsync();

                    if (status == "running")
                    {
                        Console.WriteLine($"{status}");
                        telemetryClient.TrackTrace($"{status}");
                        continue;
                    }
                    else if (status == "succeeded")
                    {
                        Console.WriteLine($"{status}");
                        telemetryClient.TrackTrace($"{status}");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"There was an error training the large face list, status: {status}");
                        telemetryClient.TrackTrace($"There was an error training the large face list, status: {status}");
                        break;
                    }
                }

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

        private async static void FindSimilarFacesInAllLargeFaceListsAsync()
        {
            using (var operation = telemetryClient.StartOperation<RequestTelemetry>("FindSimilarFacesInLargeFaceList"))
            {
                var filePath = Path.Combine(Settings.FindSimilarFolderPath, "input.jpg");
                var imageBytes = File.ReadAllBytes(filePath);
                var stream = new System.IO.MemoryStream(imageBytes);
                var imageUri = StorageHelper.UploadFileAsync(stream, $"input.jpg", "uploads", Settings.AzureWebJobsStorage, "image/jpeg").Result;

                List<JObject> list = FaceHelper.Face.DetectFacesAsync(imageUri).Result;
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
                    var detectedFaceId = list.First()["faceId"].ToString();

                    List<FindSimilar> facesFoundInAllFaceLists = new List<FindSimilar>();
                    List<ListOfLargeFaceList> faceList = await FaceHelper.LargeFaceList.ListOfLargeFaceListAsync();

                    int processed = 0;
                    if(Parallel.ForEach(faceList, (s) => {
                        try
                        {
                            Settings.LargeFaceListId = s.largeFaceListId;
                            Console.WriteLine($"Find similar faces in LargeFaceListId: {s.largeFaceListId}");
                            telemetryClient.TrackTrace($"Find similar faces in LargeFaceListId: {s.largeFaceListId}");

                            List<FindSimilar> similarFaces = FaceHelper.Face.FindSimilarFacesAsync(detectedFaceId, 10).Result;

                            foreach(FindSimilar fs in similarFaces)
                            {
                                facesFoundInAllFaceLists.Add(fs);
                            }

                            processed++;
                        }
                        catch (System.IO.FileNotFoundException ex)
                        {
                            Console.WriteLine(ex.Message);
                            telemetryClient.TrackException(ex);
                            return;
                        }
                    }).IsCompleted)
                    {
                        if(Parallel.ForEach(facesFoundInAllFaceLists, (fs) => {
                            try
                            {
                                GetFaceFromList face = FaceHelper.LargeFaceList.GetFaceInLargeFaceListAsync(fs.persistedFaceId).Result;

                                Console.WriteLine($"PersistedFaceId: {fs.persistedFaceId}, UserData: {face.userData}, Confidence: {fs.confidence}");
                                telemetryClient.TrackTrace($"PersistedFaceId: {fs.persistedFaceId}, UserData: {face.userData}, Confidence: {fs.confidence}");
                            }
                            catch (System.IO.FileNotFoundException ex)
                            {
                                Console.WriteLine(ex.Message);
                                telemetryClient.TrackException(ex);
                                return;
                            }
                        }).IsCompleted)
                        {
                            Console.WriteLine($"Processed lists: {processed}");
                            telemetryClient.TrackTrace($"Processed lists: {processed}");
                        }
                    }
                }

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
