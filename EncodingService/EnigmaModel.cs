using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedCL
{
    public class EnigmaModel
    {
        public EnigmaModel()
        {
            rotors = new List<RotorModel>();
            plugboard = new Dictionary<int, int>();
        }
        public EnigmaModel(List<RotorModel> rotors, RotorModel reflector, Dictionary<int, int> plugboard)
        {
            this.rotors = rotors;
            this.reflector = reflector;
            this.plugboard = plugboard;
        }
        public List<RotorModel> rotors { get; set; }//left to right
        public RotorModel reflector { get; set; }
        public Dictionary<int, int> plugboard { get; set; }
    }
}
