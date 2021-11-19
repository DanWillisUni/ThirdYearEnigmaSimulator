using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class EnigmaModel
    {
        public EnigmaModel()
        {
            rotors = new List<RotorModel>();
            plugboard = new Dictionary<char, char>();
        }
        public List<RotorModel> rotors { get; set; }//left to right
        public RotorModel reflector { get; set; }
        public Dictionary<char,char> plugboard { get; set; }        
    }
}
