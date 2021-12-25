using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class BreakerResult
    {
        public double score { get; set; }
        public EnigmaModel enigmaModel { get; set; }

        public BreakerResult(double score,EnigmaModel em)
        {
            this.score = score;
            enigmaModel = em;
        }        
    }
}
