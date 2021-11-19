using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHandler
{
    public class EnigmaConfiguration
    {
        public static EnigmaModel mergeEnigmaConfiguration(EnigmaModel enigmaModel)
        {
            string filePath = "currentConfig.json";
            EnigmaModel currentSave = getCurrentSave(filePath);
            if (enigmaModel.rotors?.Count > 0)
            {
                currentSave.rotors = enigmaModel.rotors;
            }
            if (enigmaModel.reflector != null)
            {
                currentSave.reflector = enigmaModel.reflector;
            }
            if (enigmaModel.plugboard?.Count > 0)
            {
                currentSave.plugboard = enigmaModel.plugboard;
            }
            System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(currentSave));
            return currentSave;
        }
        public static EnigmaModel getCurrentSave(string filePath)
        {
            EnigmaModel currentSave = new EnigmaModel();
            if (System.IO.File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    currentSave = JsonConvert.DeserializeObject<EnigmaModel>(r.ReadToEnd());
                }
            }
            return currentSave;
        }
    }
}
