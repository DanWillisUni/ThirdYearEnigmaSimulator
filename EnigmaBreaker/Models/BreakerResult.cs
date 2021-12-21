using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class BreakerResult
    {
        public string decodedText { get; set; }
        public double score { get; set; }
        public EnigmaModel enigmaModel { get; set; }

        public BreakerResult(string text,double score,EnigmaModel em)
        {
            decodedText = text;
            this.score = score;
            enigmaModel = em;
        }        
    }
}
