using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Azure.Identity;
using Azure;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;

namespace EmployeeFunction
{
    public class CsvtoJson
    {
        public string ConvertCsvFileToJsonObject(string path)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }


        public void GetFileFromDataLake(string containerDL,string filenameDL, ILogger log)
        {
            
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(connectionString);

            string container = containerDL;
            var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(container);

            string filepath = Path.Combine(Path.GetTempPath(), filenameDL);

            log.LogInformation(Path.GetTempPath());

            string filename = filenameDL;
           

            DataLakeFileClient fileClient;
            
            fileClient = fileSystemClient.GetFileClient(filename);
            
            try
            {
                log.LogInformation("GetFileFromDatalake: " + fileClient.Path);

                if (fileClient.Exists())
                {
                    Response<FileDownloadInfo> fileContents = fileClient.Read();
                    using (FileStream stream = File.OpenWrite(filepath))
                    {
                        fileContents.Value.Content.CopyTo(stream);
                        stream.Close();
                    }
                }
            }
            finally
            {
                //fileSystemClient.Delete();
            }

        }
    }
}
