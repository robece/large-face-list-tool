using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LargeFaceListTool
{
    public class FaceHelper
    {
        public class Face
        {
            public static async Task<List<JObject>> DetectFacesAsync(String url)
            {
                dynamic body = new JObject();
                body.url = url;
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false", queryString);

                    List<JObject> result = new List<JObject>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<JObject>>(json);
                    }
                    return result;
                }
            }

            public static async Task<List<FindSimilar>> FindSimilarFacesAsync(String faceId, int maxNumOfCandidatesReturned)
            {
                dynamic body = new JObject();
                body.faceId = faceId;
                body.largeFaceListId = Settings.LargeFaceListId;
                body.maxNumOfCandidatesReturned = maxNumOfCandidatesReturned;
                body.mode = "matchPerson";
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/findsimilars", queryString);

                    List<FindSimilar> result = new List<FindSimilar>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<FindSimilar>>(json);
                    }
                    return result;
                }
            }
        }

        public class LargePersonGroupPerson
        {
            public static async Task<CreatePerson> AddPersonToLargePersonGroupAsync(String name)
            {
                dynamic body = new JObject();
                body.name = name;
                body.userData = "Person added to group";
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{Settings.LargePersonGroupId}/persons", queryString);

                    CreatePerson result = null;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<CreatePerson>(json);
                    }
                    return result;
                }
            }

            public static async Task<AddPersonFace> AddFaceToLargePersonGroupAsync(String url, String personId)
            {
                dynamic body = new JObject();
                body.url = url;
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{Settings.LargePersonGroupId}/persons/{personId}/persistedFaces", queryString);

                    AddPersonFace result = null;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<AddPersonFace>(json);
                    }
                    return result;
                }
            }

            public static async Task<List<PersonInGroupOfPerson>> ListOfPersonsInLargePersonGroupAsync()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.GetAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{Settings.LargePersonGroupId}/persons");

                    List<PersonInGroupOfPerson> result = new List<PersonInGroupOfPerson>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<PersonInGroupOfPerson>>(json);
                    }

                    return result;
                }
            }

            public static async Task<bool> DeletePersonInLargePersonGroupAsync(string personId)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.DeleteAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{Settings.LargePersonGroupId}/persons/{personId}");

                    bool result = false;
                    if (response.IsSuccessStatusCode)
                        result = true;

                    return result;
                }
            }
        }

        public class LargeFaceList
        {
            public static async Task<bool> CreateLargeFaceListAsync(string largeFaceListId, string name, string userData)
            {
                dynamic body = new JObject();
                body.name = name;
                body.userData = userData;
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PutAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{largeFaceListId}", queryString);

                    bool result = false;
                    if (response.IsSuccessStatusCode)
                    {
                       result = true;
                    }
                    return result;
                }
            }
            
            public static async Task<List<ListOfLargeFaceList>> ListOfLargeFaceListAsync()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.GetAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists?top=1000");

                    List<ListOfLargeFaceList> result = new List<ListOfLargeFaceList>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<ListOfLargeFaceList>>(json);
                    }
                    return result;
                }
            }
            
             public static async Task<bool> DeleteLargeFaceListAsync(string largeFaceListId)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.DeleteAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{largeFaceListId}");

                    bool result = false;
                    if (response.IsSuccessStatusCode)
                    {
                        result = true;
                    }
                    return result;
                }
            }

            public static async Task<AddFaceToList> AddFaceToLargeFaceListAsync(string url, string userData)
            {
                dynamic body = new JObject();
                body.url = url;
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                dynamic jUserData = new JObject();
                jUserData.personId = userData;
                var rUserData = JsonConvert.SerializeObject(jUserData);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{Settings.LargeFaceListId}/persistedFaces?userData={rUserData}", queryString);

                    AddFaceToList result = null;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<AddFaceToList>(json);
                    }
                    return result;
                }
            }

            public static async Task<bool> LargeFaceListTrainAsync()
            {
                dynamic body = new JObject();
                StringContent queryString = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.PostAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{Settings.LargeFaceListId}/train", queryString);

                    bool result = false;
                    if (response.IsSuccessStatusCode)
                        result = true;

                    return result;
                }
            }

            public static async Task<string> GetLargeFaceListTrainingStatusAsync()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.GetAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{Settings.LargeFaceListId}/training");

                    string result = string.Empty;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic obj = JObject.Parse(json) as JObject;
                        result = obj.status;
                    }

                    return result;
                }
            }

            public static async Task<List<FaceInFaceList>> ListOfFacesInLargeFaceListAsync()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.GetAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{Settings.LargeFaceListId}/persistedfaces");

                    List<FaceInFaceList> result = new List<FaceInFaceList>();
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<FaceInFaceList>>(json);
                    }

                    return result;
                }
            }

            public static async Task<bool> DeleteFaceInLargeFaceListAsync(string persistedFaceId)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.FaceAPIKey);
                    var response = await client.DeleteAsync($"https://{Settings.Zone}.api.cognitive.microsoft.com/face/v1.0/largefacelists/{Settings.LargeFaceListId}/persistedfaces/{persistedFaceId}");

                    bool result = false;
                    if (response.IsSuccessStatusCode)
                        result = true;

                    return result;
                }
            }
        }
    }
}