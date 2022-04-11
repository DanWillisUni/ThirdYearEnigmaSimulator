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
            rotors = new List<RotorModel>();//sets rotors to an empty list
            plugboard = new Dictionary<int, int>();//sets plugboard to an empty dictionary
        }
        /// <summary>
        /// Constructs a enigma model from the individual parts
        /// </summary>
        /// <param name="rotors">List of Rotor models</param>
        /// <param name="reflector">Reflector rotor model</param>
        /// <param name="plugboard">plugboard dictionary</param>
        public EnigmaModel(List<RotorModel> rotors, RotorModel reflector, Dictionary<int, int> plugboard)
        {
            this.rotors = rotors;
            this.reflector = reflector;
            this.plugboard = plugboard;
        }

        /// <summary>
        /// Creates an enigma model randomly
        /// 
        /// Uses the max Rotors as the maximum number of rotors to select from
        /// Use the max Reflector as the maximum number of reflectors
        /// </summary>
        /// <param name="maxRotor">Max rotor in physical configuration to go to</param>
        /// <param name="maxReflector">Max Reflector index to go up to in Physical configuration</param>
        /// <param name="maxPlugboard">max number of plugboard pairs</param>
        /// <param name="useExactPlugboard">is the number of plugboards used exactly the max plugboard</param>
        /// <returns>New Enigma model</returns>
        public static EnigmaModel randomizeEnigma(PhysicalConfiguration pc,int maxRotor = 5,int maxReflector = 3,int maxPlugboard = 10,bool useExactPlugboard = false)
        {
            Random rnd = new Random();
            int l = rnd.Next(maxRotor);//random left rotor
            int m = rnd.Next(maxRotor);//random right rotor         
            while(m == l)//while the left rotor equals middle
            {
                m = rnd.Next(maxRotor);//select new middle rotor
            }
            int r = rnd.Next(maxRotor);//randomly select right rotor
            while (r == l || r == m)//while the right rotor is equal to the left or middle
            {
                r = rnd.Next(maxRotor);//randomly select another right rotor
            }
            
            List<RotorModel> emRotors = new List<RotorModel>();
            emRotors.Add(new RotorModel(pc.rotors[l], rnd.Next(26), rnd.Next(26)));//add leftmost rotor and randomise offset and rotation
            emRotors.Add(new RotorModel(pc.rotors[m], rnd.Next(26), rnd.Next(26)));//add middle rotor and randomise offset and rotation
            emRotors.Add(new RotorModel(pc.rotors[r], rnd.Next(26), rnd.Next(26)));//add rightmost rotor and randomise offset and rotation

            List<Rotor> reflectors = pc.reflectors;
            int reflectorStartIndex = maxReflector == 1? 1:0;            
            reflectors = reflectors.GetRange(reflectorStartIndex, maxReflector);

            Dictionary<int, int> plugboard = new Dictionary<int, int>();//create a new dictionary
            int plugboardNumber = useExactPlugboard ? maxPlugboard : rnd.Next(maxPlugboard);//randomise the number of plugboard connections if required                       
            List<int> allowedNumbers = Enumerable.Range(0, 26).Take(26).ToList();//initilise allowed numbers for plugboard
            for (int i = 0; i < plugboardNumber; i++)//for each plugboard pair
            {
                int a = allowedNumbers[rnd.Next(allowedNumbers.Count)];//select random int in the list
                allowedNumbers.Remove(a);//remove the item from the list
                int b = allowedNumbers[rnd.Next(allowedNumbers.Count)];//select second random item from the list
                allowedNumbers.Remove(b);//remove the second item from the list
                plugboard.Add(a, b);//add both items to the plugboard dictionary
            }

            EnigmaModel rEM = new EnigmaModel(emRotors, new RotorModel(reflectors[rnd.Next(maxReflector)]), plugboard);//combine all elements together to get new Enigma model
            return rEM;
        }

        public string toStringRotors()
        {
            string r = this.reflector.rotor.name;
            foreach (RotorModel rotor in this.rotors)
            {
                r += "/";
                r += rotor.ToString();
            }
            return r;
        }
        public string toStringPlugboard()
        {
            string r = "";
            foreach (KeyValuePair<int, int> entry in this.plugboard)
            {
                if (entry.Key < entry.Value)
                {
                    r += Convert.ToChar(entry.Key + 65);
                    r += Convert.ToChar(entry.Value + 65);
                }
                else
                {
                    r += Convert.ToChar(entry.Value + 65);
                    r += Convert.ToChar(entry.Key + 65);
                }
                r += " ";
            }
            return r;
        }
        public override string ToString()
        {
            return this.toStringRotors() + "/" + this.toStringPlugboard();
        }
    }
}
