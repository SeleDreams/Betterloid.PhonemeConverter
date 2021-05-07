using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonemeConverter
{
    public class PhonemeConversion
    {
        string phonemes;
        string[] splitPhoneme;

        public PhonemeConversion(string _phonemes)
        {
            phonemes = _phonemes;
            if (!string.IsNullOrEmpty(phonemes) && !string.IsNullOrWhiteSpace(phonemes))
            {
                splitPhoneme = phonemes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string GetConvertedWord(Dictionary<string,string> equivalences)
        {
            bool containsWord = equivalences.ContainsKey(phonemes);
            return containsWord ? equivalences[phonemes] : string.Empty;
        }

        public string GetConvertedPhonemes(Dictionary<string,string> equivalences)
        {
            if (splitPhoneme == null)
            {
                return string.Empty;
            }
            string result = string.Empty;
            for (int i = 0; i < splitPhoneme.Length;i++)
            {
                bool containsKey = equivalences.ContainsKey(splitPhoneme[i]);
                bool containsDefault = equivalences.ContainsKey("Default");
                result += containsKey ? equivalences[splitPhoneme[i]] + " " :  containsDefault ? 
                    equivalences["Default"] + " " : string.Empty;
            }
            return result;
        }

    }
}
