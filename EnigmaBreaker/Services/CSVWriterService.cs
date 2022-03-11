using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EnigmaBreaker.Services
{
    public class CSVWriterService<T>
    {
        private readonly ILogger<CSVWriterService<T>> _logger;
        public CSVWriterService(ILogger<CSVWriterService<T>> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Writes all the data into a CSV file
        /// </summary>
        /// <param name="dir">Directory to write to</param>
        /// <param name="fileName">File name (without extension)</param>
        /// <param name="all">Data to export</param>
        public void writeToFile(string dir,string fileName,List<T> all)
        {
            _logger.LogDebug("Writing to Dir: " + dir);
            _logger.LogDebug("Writing to fileName: " + fileName);
            _logger.LogDebug("Object Count: " + all.Count);
            var lines = getStringOutput(all);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(dir, fileName +".csv")))
            {
                foreach(string line in lines)
                {
                    outputFile.WriteLine(line);
                }                
            }
            _logger.LogDebug("Finished writing to file");
        }

        public List<string> getStringOutput(List<T> all)
        {
            List<string> output = new List<string>();
            var headers = typeof(T).GetProperties();
            string headerLine = "";
            for (var i = 0; i < headers.Length; i++)
            {
                DisplayNameAttribute dp = headers[i].GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                string nameToAdd = dp != null ? dp.DisplayName : headers[i].Name;

                headerLine += nameToAdd;
                _logger.LogDebug(nameToAdd);
                if (i != headers.Length - 1)
                {
                    headerLine += ",";
                }
            }
            output.Add(headerLine);
            foreach (T data in all)
            {
                //_logger.LogDebug("Object" + Convert.ToString(Regex.Matches(output, "\n").Count));
                output.Add(ToCsv(data, headers));
            }
            _logger.LogDebug(Convert.ToString(output.Count - 1) + " objects found");
            return output;
        }
        public virtual string ToCsv(T obj,System.Reflection.PropertyInfo[] properties)
        {
            string output = ""; 
            for (var i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(obj);
                string s = value != null ? value.ToString() : "null";                
                _logger.LogDebug(properties[i].Name + ": " + s);
                output += s;
                if (i != properties.Length - 1)
                {
                    output += ",";
                }
            }
            return output;
        }
    }
}
