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
using Azure.Storage.Blobs;

namespace EmployeeFunction
{
    public class CsvtoJson
    {
        public Stream ConvertCsvFileToJsonObject(string path)
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

            return new MemoryStream(Encoding.Default.GetBytes(JsonConvert.SerializeObject(listObjResult)));
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


        public void WriteJsonSerialisedMessageToFile(string filename,List<Dictionary<string, string>> jsonSerializedMessagesList, ILogger log)
        {
            string filepath = Path.Combine(Path.GetTempPath(), filename);
            Stream Content = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite);

            using (StreamWriter writer = new StreamWriter(Content))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, jsonSerializedMessagesList);
                jsonWriter.Flush();
                //file.Content.Close();   
            }
        }

        public void LoadJson(string filepath)
        {
            //dynamic items;
            //using (StreamReader r = new StreamReader(filepath))
            //{
            //    string json = r.ReadToEnd();
            //    items = JsonConvert.DeserializeObject(json);
            //}
            //return items;
            string json = File.ReadAllText(filepath);
            Dictionary<string, string> json_Dictionary =JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach (var item in json_Dictionary)
            {
                // parse here
                
            }
        }
    }



}
