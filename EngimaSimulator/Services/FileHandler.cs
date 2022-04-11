using Newtonsoft.Json;
using SharedCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EngimaSimulator.Services
{
    public class FileHandler
    {
        /// <summary>
        /// Merges the Enigma model passed in with the current save 
        /// 
        /// Keeps all attributes of the Enigma model passed in that have value
        /// If there is any values that are null then use the values from in the file
        /// </summary>
        /// <param name="enigmaModel">Enigma model to prioritize</param>
        /// <param name="filePath">File path to look and save in</param>
        /// <returns>The full save enigma model</returns>
        public static EnigmaModel mergeEnigmaConfiguration(EnigmaModel enigmaModel, string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            file.Directory.Create(); // If the directory already exists, this method does nothing.

            EnigmaModel currentSave = getCurrentSave(filePath); //Get the current save of the file
            if (enigmaModel.rotors?.Count > 0)//if the rotors are not null or empty
            {
                currentSave.rotors = enigmaModel.rotors;//set the current save to the rotors passed in
            }
            if (enigmaModel.reflector != null)//if the reflector is not null
            {
                currentSave.reflector = enigmaModel.reflector;//set the current save reflector to the one passed in
            }
            if (enigmaModel.plugboard?.Count > 0)//if the plugboard is not null or empty
            {
                currentSave.plugboard = enigmaModel.plugboard;//set the current save plugboard to the plugboard passed in
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(currentSave));//write to file
            return currentSave;//return the merged model
        }
        /// <summary>
        /// Gets the enigma model that is serilized in the file
        /// </summary>
        /// <param name="filePath">File path to look in</param>
        /// <returns>The enigma model from the file</returns>
        public static EnigmaModel getCurrentSave(string filePath)
        {
            EnigmaModel r = new EnigmaModel();//set return model to a new model
            if (System.IO.File.Exists(filePath))//if the file exists
            {
                using (StreamReader sr = new StreamReader(filePath))//create a stream reader of the file
                {
                    r = JsonConvert.DeserializeObject<EnigmaModel>(sr.ReadToEnd());//deserialize the file contents
                }
            }
            return r;//return the enigma model
        }
        /// <summary>
        /// Overwrites the data in the file with the new enigma model
        /// </summary>
        /// <param name="enigmaModel">Enigma model to go in file</param>
        /// <param name="filePath">Filepath to write to</param>
        public static void overwrite(EnigmaModel enigmaModel, string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(enigmaModel));//write a serilised enigma model to the file
        }
    }
}
