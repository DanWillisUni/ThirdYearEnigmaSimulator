using SharedCL;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Models
{
    public class SingleRotorResult
    {
        public SingleRotorResult(string attemptPlainText, double rating, int targetRotorIndex, RotorModel rm)
        {
            attemptedPlainText = attemptPlainText;
            score = rating;
            rotorIndex = targetRotorIndex;
            this.rm = rm;
        }
        public string attemptedPlainText { get; set; }
        public double score { get; set; }
        public int rotorIndex { get; set; }
        public RotorModel rm {get;set;}
    }
}
