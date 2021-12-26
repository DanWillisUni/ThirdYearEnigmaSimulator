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
        public int[] encode(int[] input, EnigmaModel em)
        {
            int[] r = new int[input.Length];
            for(int i = 0;i< input.Length;i++)
            {
                em = stepRotors(em);
                r[i] = encodeOneChar(em, input[i]);
            }
            return r;
        }
        public string encode(string input,EnigmaModel em) 
        { 
            string formattedInput = Regex.Replace(input.ToUpper(), @"[^A-Z]", string.Empty);
            int[] ciphertextArr = new int[formattedInput.Length];
            for (int i = 0; i < formattedInput.Length; i++)
            {
                ciphertextArr[i] = Convert.ToInt16(formattedInput[i]) - 65;
            }
            int[] charArr = encode(ciphertextArr, em);
            string r = "";
            foreach(int c in charArr)
            {
                r += Convert.ToChar(c + 65);
            }
            return r;
        }
        public int encodeOneChar(EnigmaModel em, int current)
        {
            current = plugboardSwap(em.plugboard, current);
            foreach (RotorModel r in em.rotors.Reverse<RotorModel>())
            {
                current = rotorEncode(r, current);
            }
            current = rotorEncode(em.reflector, current);
            foreach (RotorModel r in em.rotors)
            {
                current = rotorEncodeInverse(r, current);
            }
            current = plugboardSwap(em.plugboard, current);
            return current;
        }
        public int plugboardSwap(Dictionary<int, int> plugboard, int input)
        {
            foreach (KeyValuePair<int, int> entry in plugboard)
            {
                if (entry.Key.Equals(input))
                {
                    return entry.Value;
                }
                if (entry.Value.Equals(input))
                {
                    return entry.Key;
                }
            }
            return input;
        }
        public int rotorEncode(RotorModel rm, int input)
        {
            int OrderIndex = input + rm.rotation - rm.ringOffset;
            char encodedChar = rm.rotor.order[mod26(OrderIndex)];
            int encodedCharRotated = Convert.ToInt32(encodedChar) - 65 - rm.rotation + rm.ringOffset;
            return mod26(encodedCharRotated);
        }
        public int rotorEncodeInverse(RotorModel rm, int input)
        {
            int charRotated = input + rm.rotation - rm.ringOffset;
            int encodedCharNumber = rm.rotor.order.IndexOf(Convert.ToChar(mod26(charRotated) + 65));
            return mod26(encodedCharNumber - rm.rotation + rm.ringOffset);
        }
        public static int mod26(int x)
        {
            int r = x % 26;
            return r < 0 ? r + 26 : r;
        }
        public EnigmaModel stepRotors(EnigmaModel em)
        {
            em.rotors[em.rotors.Count - 1].rotation = (em.rotors[em.rotors.Count - 1].rotation + 1) % 26;
            for (int i = em.rotors.Count - 1; i >= 1; i--)
            {
                RotorModel r = em.rotors[i];
                if (r.rotor.turnoverNotchA == mod26(r.rotation - 1))
                {
                    em.rotors[i - 1].rotation = (em.rotors[i - 1].rotation + 1) % 26;
                }
                else
                {
                    break;
                }
            }
            return em;
        }
    }
}
