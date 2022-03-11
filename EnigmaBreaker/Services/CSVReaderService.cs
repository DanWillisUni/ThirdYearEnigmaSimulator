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
        public List<T> readFromFile(string dir, string fileName)
        {
            _logger.LogDebug("Reading from Dir: " + dir);
            _logger.LogDebug("Reading from fileName: " + fileName);
            List<string> headers = new List<string>();
            List<T> r = new List<T>(); 
            using (StreamReader reader = new StreamReader(Path.Combine(dir, fileName + ".csv")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    _logger.LogDebug("Line: " + line);
                    var values = line.Split(',').ToList();
                    if (headers.Count > 0)
                    {
                        r.Add(convertToT(headers, values));
                    }
                    else
                    {
                        headers = values;
                    }  
                }
            }
            _logger.LogDebug("Finished reading file");
            return r;
        }

        private T convertToT(List<string> headers,List<string> values)
        {
            _logger.LogDebug("ConvertTo");
            T r = (T)Activator.CreateInstance(typeof(T));
            var allProperties = typeof(T).GetProperties();
            for (int i = 0;i< headers.Count; i++)
            {
                foreach(var pi in allProperties)
                {                    
                    DisplayNameAttribute dp = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                    string nameToCheck = dp != null? dp.DisplayName: pi.Name;

                    if (headers[i] == nameToCheck)
                    {
                        _logger.LogDebug(nameToCheck + ":" + values[i]);
                        pi.SetValue(r, values[i]);                                               
                        break;
                    }
                }                
            }
            return r;
        }
    }
}
