using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Models
{
    public class EnigmaModel
    {
        public List<RotorModel> rotors { get; set; }//left to right
        public RotorModel reflector { get; set; }
        public Dictionary<char,char> plugboard { get; set; }        
    }
}
