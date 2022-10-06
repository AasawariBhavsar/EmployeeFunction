using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Azure.Storage.Queues.Models;
using System.Net.Http;
using System.Linq;
using System.Reflection.Metadata;
using Azure.Identity;
using Azure.Storage.Blobs;
using static System.Net.WebRequestMethods;
using System.Formats.Asn1;
using System.Globalization; 
using System.Text;
using System.IO;
using Newtonsoft.Json;
using CsvHelper;
using System.Collections.Generic;
using Azure;
using JsonLogic.Net;


namespace EmployeeFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
    
            //[Blob("input/Sample-Spreadsheet-10-rows.csv", FileAccess.Read ,Connection = "AzureWebJobsStorage")] Stream myBlob,
            //[Blob("output/Sample-Spreadsheet-10-rows.csv",FileAccess.Write,Connection ="AzureWebJobsStorage")] Stream outBlob,
            ILogger log)
        {
           
            string filename = req.Headers.GetValues("filename").FirstOrDefault();
            log.LogInformation($"File Name : {filename}");
            string InputcontainerName = "input";
            string OutputcontainerName = "output";
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            var InputcontainerClient = blobServiceClient.GetBlobContainerClient(InputcontainerName);

            BlobClient InputblobClient = InputcontainerClient.GetBlobClient(filename);
            //Only convert CSV files
           



            if (await InputblobClient.ExistsAsync())
            {
                //var response = await InputblobClient.DownloadAsync();
               

               

                CsvtoJson csvtoJson=new CsvtoJson();
                csvtoJson.GetFileFromDataLake(InputcontainerName,filename,log);
                string filepath = Path.Combine(Path.GetTempPath(), filename);
                log.LogInformation(filepath);
                string feedData = System.IO.File.ReadAllText(filepath);
                log.LogInformation("File data : ", feedData);
                
                Stream jsondata=csvtoJson.ConvertCsvFileToJsonObject(filepath);
                //log.LogInformation(jsondata);

                var OutputcontainerClient = blobServiceClient.GetBlobContainerClient(OutputcontainerName);

                string json_filename = $"{filename.Split(".")[0]}.json";
                log.LogInformation(json_filename);

                BlobClient OutputblobClient = OutputcontainerClient.GetBlobClient(json_filename);
                await OutputblobClient.UploadAsync(jsondata);




                csvtoJson.GetFileFromDataLake(OutputcontainerName, json_filename, log);
                string json_filepath=Path.Combine(Path.GetTempPath(), json_filename);
                dynamic employees=csvtoJson.LoadJson(json_filepath);

                
                
                //using (var streamReader = new StreamReader(response.Value.Content))
                //{
                //    while (!streamReader.EndOfStream)
                //    {
                //        var line = await streamReader.ReadLineAsync();
                //        csv.Add(line.Split(","));

                //    }
                //}





            }











            //StreamReader reader=new StreamReader(myBlob);
            log.LogInformation("C# HTTP trigger function processed a request.");
            //log.LogInformation($"BlobInput processed blob \n Size: {myBlob.Length} bytes \n {reader.ReadToEnd()} ");
        

        //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //string response=reader.ToString();

            return new OkObjectResult("ok");
        }



        

    }


}
