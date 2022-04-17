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
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(dir, fileName +".csv")))//opens file
            {
                foreach(string line in lines)//for each line
                {
                    outputFile.WriteLine(line);//write line
                }                
            }
            _logger.LogDebug("Finished writing to file");
        }

        /// <summary>
        /// Converts list of objects to a list of lines to write to the file
        /// </summary>
        /// <param name="all">the list of objects</param>
        /// <returns>List of lines for the file</returns>
        public List<string> getStringOutput(List<T> all)
        {
            List<string> output = new List<string>();
            var headers = typeof(T).GetProperties();//set the headers as every property
            string headerLine = "";
            for (var i = 0; i < headers.Length; i++)//for eahc header
            {
                DisplayNameAttribute dp = headers[i].GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();//get display attribute
                string nameToAdd = dp != null ? dp.DisplayName : headers[i].Name;//get the name

                headerLine += nameToAdd;//add to the headerline
                _logger.LogDebug(nameToAdd);
                if (i != headers.Length - 1)
                {
                    headerLine += ",";//adding commas for csv
                }
            }
            output.Add(headerLine);//add headerline to return
            foreach (T data in all)//for each peice of data
            {
                //_logger.LogDebug("Object" + Convert.ToString(Regex.Matches(output, "\n").Count));
                output.Add(ToCsv(data, headers));//convert the data to a string and add it
            }
            _logger.LogDebug(Convert.ToString(output.Count - 1) + " objects found");
            return output;
        }
        /// <summary>
        /// Convert the data to a string
        /// </summary>
        /// <param name="obj">object to convert</param>
        /// <param name="properties">properties of object</param>
        /// <returns></returns>
        public virtual string ToCsv(T obj,System.Reflection.PropertyInfo[] properties)
        {
            string output = ""; 
            for (var i = 0; i < properties.Length; i++)//for eahc property
            {
                var value = properties[i].GetValue(obj);//get value
                string s = value != null ? value.ToString() : "null"; //convert value to string               
                _logger.LogDebug(properties[i].Name + ": " + s);
                output += s;//add the string to the return
                if (i != properties.Length - 1)
                {
                    output += ",";//add comma for csv
                }
            }
            return output;
        }
    }
}
