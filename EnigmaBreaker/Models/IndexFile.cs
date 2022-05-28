using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class IndexFile
    {
        public List<indexFileItem> IndexFiles { get; set; }  
        public IndexFile(string fileName)
        {
            IndexFiles = new List<indexFileItem>();
            int count = 0;
            foreach (string line in System.IO.File.ReadLines(fileName))//for each line
            {
                if (count != 0)
                {
                    IndexFiles.Add(new indexFileItem(line));//create new item
                }
                count = count + 1;
            }
        }
    }

    public class indexFileItem
    {
        public int length { get; set; }
        public Dictionary<int, double> data { get; set; }
        public indexFileItem(string line)
        {
            var ls = line.Split(",");//split by comma
            length = Convert.ToInt16(ls[0]);//first item is the length
            data = new Dictionary<int, double>();//rest is the data
            for (int i = 1; i < ls.Length; i++)//for the rest
            {
                data.Add(i-1, Convert.ToDouble(ls[i]));//add to data dictionary
            }
        }
    }
}
