using Newtonsoft.Json;
using System.Net;

namespace WebApiClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello, World!");
            BuildOpenApiClient("https://devsleep.persante.com/ScoringApi/swagger/v1/swagger.json");
        }

        private static void BuildOpenApiClient(string schemaUrl)
        {
            //var json= new WebClient().DownloadString(schemaUrl);
            var json = File.ReadAllText("TestData/scoring-api-swagger.json");
            var schema = OpenApiSerializer.FromJson(json);
            var toJson = OpenApiSerializer.ToJson(schema);
            var nsJson = JsonConvert.SerializeObject(schema, Formatting.Indented);
        }
    }
}