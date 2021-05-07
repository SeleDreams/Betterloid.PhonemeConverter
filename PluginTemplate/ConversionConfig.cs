using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yamaha.VOCALOID.VSM;

namespace PhonemeConverter
{
    public class ConversionConfig
    {
        public string Name { get; set; } = "NO NAME";
        public bool WordMode { get; set; } = false;
        public string TargetLanguage { get; set; } = "*";
        public string TargetVoicebank { get; set; } = "*";
        public Dictionary<string, string> PhonemeEquivalents { get; set; } = null;

        public VSMLanguageID GetLanguageID()
        {
            switch (TargetLanguage)
            {
                case "EN":
                    return VSMLanguageID.English;
                case "JP":
                    return VSMLanguageID.Japanese;
                case "SP":
                    return VSMLanguageID.Spanish;
                case "KO":
                    return VSMLanguageID.Korean;
                case "CH":
                    return VSMLanguageID.Chinese;
            }
            return VSMLanguageID.Japanese;
        }
    }
}
