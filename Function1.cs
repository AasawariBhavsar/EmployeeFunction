using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Azure.Storage.Queues.Models;

namespace EmployeeFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Blob("input/Sample-Spreadsheet-10-rows.csv", FileAccess.Read ,Connection = "AzureWebJobsStorage")] Stream myBlob,
            ILogger log)
        {
            StreamReader reader=new StreamReader(myBlob);
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"BlobInput processed blob \n Size: {myBlob.Length} bytes \n {reader.ReadToEnd()} ");
        

        //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            string response=reader.ToString();

            return new OkObjectResult(response);
        }
    }
}
