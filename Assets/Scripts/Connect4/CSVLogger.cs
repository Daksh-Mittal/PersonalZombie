using System;
using System.IO;
using UnityEngine;

public class CSVLogger
{
    private string filePath;

    public CSVLogger(string fileName)
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // first column is always timestamp, then replace with your column labels
                writer.WriteLine("Timestamp,result");
            }
        }
        // print out where the file is stored... took me forever to find it otherwise
        Debug.Log($"CSV Logger initialized. Logging to: {filePath}");
    }

    // log a variable number of values (floats, strings, ints, bools) to a single row
    public void Log(params object[] values)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string[] stringValues = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                    stringValues[i] = "";
                else
                    stringValues[i] = values[i].ToString();
            }
            string row = timeStamp + "," + string.Join(",", stringValues);
            writer.WriteLine(row);
        }
    }
}