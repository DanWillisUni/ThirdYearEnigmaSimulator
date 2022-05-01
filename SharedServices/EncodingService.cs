using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedCL
{
    public class EncodingService
    {
        public EncodingService()
        {
        }
        /// <summary>
        /// Encodes the text through the enigma model
        /// </summary>
        /// <param name="input">Integer array of text to encrypt</param>
        /// <param name="em">Enigma model</param>
        /// <returns></returns>
        public int[] encode(int[] input, EnigmaModel em)
        {
            int[] r = new int[input.Length];//set the return integer array to the length of the input
            for(int i = 0;i< input.Length;i++)//for each index of the input array
            {
                em = stepRotors(em);//step the rotors
                r[i] = encodeOneChar(em, input[i]);//set the return array at the same index to one charater encoded
            }
            return r;//return the array
        }
        /// <summary>
        /// Encodes the string with the enigma model
        /// </summary>
        /// <param name="input">string to encode</param>
        /// <param name="em">enigma model to encode with</param>
        /// <returns>string of ciphertext</returns>
        public string encode(string input,EnigmaModel em) 
        {
            int[] ciphertextArr = preProccessCiphertext(input);//convert the string to a int array
            int[] charArr = encode(ciphertextArr, em);//encode the integer array through the enigma model        
            return getStringFromIntArr(charArr);//convert the integer array back to a string and return
        }
        /// <summary>
        /// Encode a single char through the enigma model
        /// </summary>
        /// <param name="em">Enigma model to encode with</param>
        /// <param name="input">input integer</param>
        /// <returns></returns>
        public int encodeOneChar(EnigmaModel em, int input)
        {
            int current = plugboardSwap(em.plugboard, input);//do the plugboard swap
            foreach (RotorModel r in em.rotors.Reverse<RotorModel>())//for each rotor right to left
            {
                current = rotorEncode(r, current);//encode through the rotor
            }
            current = rotorEncode(em.reflector, current);//encode through the reflector
            foreach (RotorModel r in em.rotors)//for each rotor left to right
            {
                current = rotorEncodeInverse(r, current);//encode inverse
            }
            current = plugboardSwap(em.plugboard, current);//do the plugboard swap
            return current;//return the end result
        }
        /// <summary>
        /// Performs the plugboard swaps for one char
        /// </summary>
        /// <param name="plugboard">plugboard state</param>
        /// <param name="input">integer to represent char</param>
        /// <returns>the swapped integer</returns>
        public int plugboardSwap(Dictionary<int, int> plugboard, int input)
        {
            foreach (KeyValuePair<int, int> entry in plugboard)//for each pair in the dictionary
            {
                if (entry.Key.Equals(input))//if the key is equal to input
                {
                    return entry.Value;//return the value
                }
                if (entry.Value.Equals(input))//if the value is equal to the input
                {
                    return entry.Key;//return the key
                }
            }
            return input;//no swap performed so return the input
        }
        /// <summary>
        ///  Encode the char through the rotor one way
        /// </summary>
        /// <param name="rm">Rotor to encode through</param>
        /// <param name="input">input char</param>
        /// <returns>encoded char</returns>
        public int rotorEncode(RotorModel rm, int input)
        {
            int OrderIndex = input + rm.rotation - rm.ringOffset;//get the index to use
            char encodedChar = rm.rotor.order[mod26(OrderIndex)];//get the char value
            int encodedCharRotated = Convert.ToInt32(encodedChar) - 65 - rm.rotation + rm.ringOffset;//account for rotation and offset
            return mod26(encodedCharRotated);//return the mod 26
        }
        /// <summary>
        /// Encodes the char by the rotor inverse
        /// </summary>
        /// <param name="rm"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public int rotorEncodeInverse(RotorModel rm, int input)
        {
            int charRotated = input + rm.rotation - rm.ringOffset;//gets the char rotated
            int encodedCharNumber = rm.rotor.order.IndexOf(Convert.ToChar(mod26(charRotated) + 65));//gets in index of the char
            return mod26(encodedCharNumber - rm.rotation + rm.ringOffset);//mod 26 the account for rotation and offset
        }        
        /// <summary>
        /// Step the rotors
        /// </summary>
        /// <param name="em">input enigma model</param>
        /// <returns>The enigma model having stepped the rotors</returns>
        public EnigmaModel stepRotors(EnigmaModel em)
        {
            em.rotors[em.rotors.Count - 1].rotation = (em.rotors[em.rotors.Count - 1].rotation + 1) % 26;//turn over the right most rotor
            for (int i = em.rotors.Count - 1; i >= 1; i--)//for rotor right to left
            {
                RotorModel r = em.rotors[i];//set the rotor model
                if (r.rotor.turnoverNotchA == mod26(r.rotation - 1))//if the next rotor needs to turn
                {
                    em.rotors[i - 1].rotation = (em.rotors[i - 1].rotation + 1) % 26;//turn the next rotor
                }
                else
                {
                    break;//dont turn over any more rotors
                }
            }
            return em;//return the modified enigma model
        }

        #region helpers
        /// <summary>
        /// Modulus 26
        /// </summary>
        /// <param name="x">input</param>
        /// <returns>modulus 26 of the input</returns>
        public static int mod26(int x)
        {
            return (x + 26) % 26;
        }
        /// <summary>
        /// Convert int array to string
        /// </summary>
        /// <param name="input">input integer array</param>
        /// <returns>String equivilent of array</returns>
        public static string getStringFromIntArr(int[] input)
        {
            string r = "";
            foreach (int c in input)//for each value in the array
            {
                r += Convert.ToChar(c + 65);//convert it to a char and add it to the return string
            }
            return r;//return the string
        }
        /// <summary>
        /// convert the string to integer array
        /// </summary>
        /// <param name="text">text input</param>
        /// <returns>integer array of the equivilent</returns>
        public static int[] preProccessCiphertext(string text)
        {
            string formattedInput = Regex.Replace(text.ToUpper(), @"[^A-Z]", string.Empty);//remove any character that isnt a letter and uppercase
            int[] r = new int[formattedInput.Length];//create new integer array of the length of the formatted string
            for (int i = 0; i < formattedInput.Length; i++)//for each char
            {
                r[i] = Convert.ToInt16(formattedInput[i]) - 65;//set the integer array value at the same index to the int of the char
            }
            return r;//return the array
        }
        #endregion
    }
}
