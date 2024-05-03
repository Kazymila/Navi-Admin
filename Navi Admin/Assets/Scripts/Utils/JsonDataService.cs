using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

public static class JsonDataService
{
    public static string ToJson<T>(T data)
    {   // Convert the data to a JSON string
        return JsonConvert.SerializeObject(data, Formatting.Indented,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }

    public static bool SaveData<T>(string path, T data)
    {   // Save the data to a JSON file

        if (File.Exists(path))
        {   // If the file exists, delete it and create a new one
            try
            {
                File.Delete(path);
                using FileStream stream = File.Create(path);
                stream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(data));
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving data: " + e.Message);
                return false;
            }
        }
        else
        {   // If the file doesn't exist, create a new one
            try
            {
                using FileStream stream = File.Create(path);
                stream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving data: " + e.Message);
                return false;
            }
        }
    }

    public static T LoadData<T>(string path)
    {   // Load the data from a JSON file

        if (!File.Exists(path))
        {   // If the file doesn't exist, throw an exception
            Debug.LogError("Cannot load file. File not found: " + path);
            throw new FileNotFoundException("File not found: " + path);
        }
        try
        {   // Deserialize the JSON file and return the data
            T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading data: " + e.Message);
            throw e;
        }
    }
}
