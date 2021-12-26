using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class BreakerResult
    {
        public int[] text { get; set; }
        public double score { get; set; }
        public EnigmaModel enigmaModel { get; set; }

        public BreakerResult(int[] text,double score,EnigmaModel em)
        {
            this.text = text;
            this.score = score;
            enigmaModel = em;
        }        
    }
}
