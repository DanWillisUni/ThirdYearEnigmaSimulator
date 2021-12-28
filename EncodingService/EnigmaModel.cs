using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedCL
{
    public class EnigmaModel
    {
        public List<RotorModel> rotors { get; set; }//left to right
        public RotorModel reflector { get; set; }
        public Dictionary<int, int> plugboard { get; set; }
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

        public static EnigmaModel randomizeEnigma(int maxRotor = 5,int maxReflector = 3,int maxPlugboard = 10)
        {
            Random rnd = new Random();
            int l = rnd.Next(maxRotor);
            int m = rnd.Next(maxRotor);
            int r = rnd.Next(maxRotor);
            while(m == l)
            {
                m = rnd.Next(maxRotor);
            }
            while (r == l || r == m)
            {
                r = rnd.Next(maxRotor);
            }
            Dictionary<int, int> plugboard = new Dictionary<int, int>();
            int plugboardNumber = rnd.Next(maxPlugboard);
            List<int> containedNumbers = new List<int>();
            for(int i = 0;i <= plugboardNumber; i++)
            {
                int a = rnd.Next(26);
                int b = rnd.Next(26);             
                while (a==b || containedNumbers.Contains(a) || containedNumbers.Contains(b))
                {
                    a = rnd.Next(26);
                    b = rnd.Next(26);
                }
                plugboard.Add(a, b);
                containedNumbers.Add(a);
                containedNumbers.Add(b);
            }

            string Rotorjson = @"[
      {
        'name': 'I',
        'order': 'EKMFLGDQVZNTOWYHXUSPAIBRCJ',
        'turnoverNotchA': 7

      },
      {
        'name': 'II',
        'order': 'AJDKSIRUXBLHWTMCQGZNPYFVOE',
        'turnoverNotchA': 25
      },
      {
        'name': 'III',
        'order': 'BDFHJLCPRTXVZNYEIWGAKMUSQO',
        'turnoverNotchA': 11
      },
      {
        'name': 'IV',
        'order': 'ESOVPZJAYQUIRHXLNFTGKDCMWB',
        'turnoverNotchA': 6
      },
      {
        'name': 'V',
        'order': 'VZBRGITYUPSDNHLXAWMJQOFECK',
        'turnoverNotchA': 1
      }
    ]";
            List<Rotor> rotors = JsonConvert.DeserializeObject<List<Rotor>>(Rotorjson);
            string Reflectorjson = @"[      
      {
                'name': 'A',
        'order': 'EJMZALYXVBWFCRQUONTSPIKHGD'
      },
      {
                'name': 'B',
        'order': 'YRUHQSLDPXNGOKMIEBFZCWVJAT'
      },
      {
                'name': 'C',
        'order': 'FVPJIAOYEDRZXWGCTKUQSBNMHL'
      }
    ]";
            List<Rotor> reflectors = JsonConvert.DeserializeObject<List<Rotor>>(Reflectorjson);
            int reflectorStartIndex = 0;
            if (maxReflector == 1)
            {
                reflectorStartIndex = 1;
            }
            reflectors = reflectors.GetRange(reflectorStartIndex, maxReflector);

            List<RotorModel> emRotors = new List<RotorModel>();
            emRotors.Add(new RotorModel(rotors[l], rnd.Next(26), rnd.Next(26)));
            emRotors.Add(new RotorModel(rotors[m], rnd.Next(26), rnd.Next(26)));
            emRotors.Add(new RotorModel(rotors[r], rnd.Next(26), rnd.Next(26)));

            EnigmaModel rEM = new EnigmaModel(emRotors, new RotorModel(reflectors[rnd.Next(maxReflector)]), plugboard);
            return rEM;
        }
    }
}
