using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class WeightFile
    {
        public string length { get; set; }
        public string IOC { get; set; }
        public string S { get; set; }
        public string BI { get; set; }
        public string TRI { get; set; }
        public string QUAD { get; set; }

        public Dictionary<string, double> weights { get
            {
                Dictionary<string, double> r = new Dictionary<string, double>();
                if (IOC != null)
                {
                    r.Add("IOC",Convert.ToDouble(IOC)/100);
                }
                if (S != null)
                {
                    r.Add("S", Convert.ToDouble(S)/100);
                }
                if (BI != null)
                {
                    r.Add("BI", Convert.ToDouble(BI)/100);
                }
                if (TRI != null)
                {
                    r.Add("TRI", Convert.ToDouble(TRI)/100);
                }
                if (QUAD != null)
                {
                    r.Add("QUAD", Convert.ToDouble(QUAD)/100);
                }
                return r; 
            }
        }

    }
}
