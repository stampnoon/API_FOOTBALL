using RestSharp;

namespace Football_API.Models
{
    public class API
    {
        public static string apiKEY = "6c5f1952camsha2f1f94a1215a7fp1832fdjsn21cc3ec4de1a";

        public string callAPIService(string URL)
        {
            var client = new RestClient(URL);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", apiKEY);
            IRestResponse response = client.Execute(request);
            string JsonStr = response.Content.ToString();
            if (!JsonStr.StartsWith("["))
            {
                JsonStr = "[" + JsonStr;
            }
            if (!JsonStr.EndsWith("]"))
            {
                JsonStr = JsonStr + "]";
            }
            return JsonStr;
        }
    }

}

