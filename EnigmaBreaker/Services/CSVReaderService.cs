using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EnigmaBreaker.Services
{
    public class CSVReaderService<T>
    {
        private readonly ILogger<CSVReaderService<T>> _logger;
        public CSVReaderService(ILogger<CSVReaderService<T>> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Read from the file into the list of T
        /// </summary>
        /// <param name="dir">dir of the file</param>
        /// <param name="fileName">filename</param>
        /// <returns>List of T from the file</returns>
        public List<T> readFromFile(string dir, string fileName)
        {
            _logger.LogDebug("Reading from Dir: " + dir);
            _logger.LogDebug("Reading from fileName: " + fileName);
            List<string> headers = new List<string>();
            List<T> r = new List<T>(); 
            using (StreamReader reader = new StreamReader(Path.Combine(dir, fileName + ".csv")))//open the file
            {
                while (!reader.EndOfStream)//while there are more lines
                {
                    var line = reader.ReadLine();
                    _logger.LogDebug("Line: " + line);
                    var values = line.Split(',').ToList();
                    if (headers.Count > 0)
                    {
                        r.Add(convertToT(headers, values));//add items to list
                    }
                    else
                    {
                        headers = values;//convert top line to headers
                    }  
                }
            }
            _logger.LogDebug("Finished reading file");
            return r;
        }

        /// <summary>
        /// Convert a list of headers and values to object
        /// </summary>
        /// <param name="headers">Column header names</param>
        /// <param name="values">values of each column</param>
        /// <returns>New object</returns>
        private T convertToT(List<string> headers,List<string> values)
        {
            _logger.LogDebug("ConvertTo");
            T r = (T)Activator.CreateInstance(typeof(T));//create instance
            var allProperties = typeof(T).GetProperties();//get properties
            for (int i = 0;i< headers.Count; i++)//for each header
            {
                foreach(var pi in allProperties)//for each property
                {                    
                    DisplayNameAttribute dp = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();//get display attribute
                    string nameToCheck = dp != null? dp.DisplayName: pi.Name;//if there is a display attribute set to display name else property name

                    if (headers[i] == nameToCheck)//if header name matches the name
                    {
                        _logger.LogDebug(nameToCheck + ":" + values[i]);
                        pi.SetValue(r, values[i]); //set the value                                              
                        break;//break to move to the next header value
                    }
                }                
            }
            return r;
        }
    }
}
