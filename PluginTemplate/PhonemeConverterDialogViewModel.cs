using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using Yamaha.VOCALOID.VDM;
using Yamaha.VOCALOID.VOCALOID5;
using Yamaha.VOCALOID.VOCALOID5.MusicalEditor;
using Yamaha.VOCALOID.VOCALOID5.TrackEditor;
using Yamaha.VOCALOID.VSM;
using System.Linq;
using System.ComponentModel;
using System.Windows;

namespace PhonemeConverter
{
    public class PhonemeConverterDialogViewModel : INotifyPropertyChanged
    {
        MainWindow MainWindow;
        TrackEditorDivision TrackEditorDivision;
        TrackEditorViewModel TrackEditorViewModel;
        MusicalEditorDivision MusicalEditorDivision;
        MusicalEditorViewModel MusicalEditor;
        VoiceBank DefaultVoicebank;

        ConversionConfig Config;
        public bool Valid;

        public event PropertyChangedEventHandler PropertyChanged;


        public string FilePath { get; set; } = "";
        private string _filename;
        public string Filename { get => _filename; set { _filename = value; NotifyPropertyChanged("Filename"); } }
        private string _status;
        public string Status { get => _status; set { _status = value; NotifyPropertyChanged("Status"); } }

        private bool _enabled;
        public bool Enabled { get => _enabled; set { _enabled = value; NotifyPropertyChanged("Enabled"); } }

        public Window window;


        public PhonemeConverterDialogViewModel()
        {
            Status = "Choose a file then choose your option.";
            Filename = "No file selected.";
            Enabled = true;

            // If you need to find the name of a specific property you can rely on DNSpy or the visual studio WPF debugger to find it
            MainWindow = App.Current.MainWindow as MainWindow;
            MusicalEditorDivision = MainWindow.FindName("xMusicalEditorDiv") as MusicalEditorDivision;
            MusicalEditor = MusicalEditorDivision.DataContext as MusicalEditorViewModel;
            TrackEditorDivision = MainWindow.FindName("xTrackEditorDiv") as TrackEditorDivision;
            TrackEditorViewModel = TrackEditorDivision.DataContext as TrackEditorViewModel;
            DefaultVoicebank = App.DatabaseManager.DefaultVoiceBank;
        }

        private void DisplayError(string message, bool nextState)
        {
            MessageBoxDeliverer.GeneralError(window, message);
            Enabled = nextState;
        }
        public void SelectFile()
        {
            Enabled = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Phoneme converter configuration file | *.json";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Please select a converter...";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                try
                {
                    ConversionConfig config = Betterloid.Betterloid.DeserializeObject<ConversionConfig>(File.ReadAllText(FilePath));
                    if (config != null && !string.IsNullOrEmpty(config.Name) && config.PhonemeEquivalents != null)
                    {
                        Config = config;
                        Valid = true;
                        Filename = config.Name;
                    }
                    else
                    {
                        Config = null;
                        Filename = "Invalid file";
                        Valid = false;
                    }
                }
                catch
                {
                    Config = null;
                    Filename = "Invalid file";
                    Valid = false;
                }
            }
            Enabled = true;
        }

        private void ProcessNotes(ref List<WIVSMNote> notes)
        {
            Enabled = false;
            using (Transaction transaction = new Transaction(TrackEditorViewModel.Sequence))
            {
                foreach (var note in notes)
                {
                    PhonemeConversion conversion = new PhonemeConversion(note.Phonemes);
                    string phonemes;
                    if (Config.WordMode)
                    {
                        phonemes = conversion.GetConvertedWord(Config.PhonemeEquivalents);
                    }
                    else
                    {
                        phonemes = conversion.GetConvertedPhonemes(Config.PhonemeEquivalents);
                    }
                    if (string.IsNullOrEmpty(phonemes) || string.IsNullOrWhiteSpace(phonemes))
                    {
                        continue;
                    }
                    note.IsProtected = false;
                    if (!note.SetPhonemes(phonemes, true))
                    {
                        DisplayError("An error occurred when setting the note " + note.Phonemes + " !" + " : " + Enum.GetName(typeof(VSMResult), note.LastError), false);
                    }
                }

            }
            Enabled = true;
        }

        void ProcessParts(List<WIVSMPart> parts)
        {
            int Processed = 0;
            foreach (WIVSMPart part in parts)
            {
                if (part.GetType() != typeof(WIVSMMidiPart))
                {
                    continue;
                }
                WIVSMMidiPart midipart = part as WIVSMMidiPart;
                VoiceBank voicebank = App.DatabaseManager.GetVoiceBankByCompID(midipart.VoiceBankID);
                if (Config.TargetLanguage != "*" && midipart.LangIDFromVoiceBank != (int)Config.GetLanguageID())
                {
                    DisplayError($"The language from the singer {voicebank.Name} ({ Enum.GetName(typeof(VSMLanguageID), ((VSMLanguageID)voicebank.LangID))}) does not correspond to the language from the converter ({Enum.GetName(typeof(VSMLanguageID), Config.GetLanguageID())}) this part will be skipped !", false);
                    continue;
                }
                if (Config.TargetVoicebank != "*" && voicebank.Name != Config.TargetVoicebank)
                {
                    DisplayError($"The voicebank from {midipart.Name} ({voicebank.Name} doesn't correspond to the expected target voicebank ({Config.TargetVoicebank}, this part will be skipped !", false);
                    continue;
                }
                var notes = midipart.Notes;
                if (notes.Count > 0)
                {
                    ProcessNotes(ref notes);
                    Processed++;
                }
            }
            if (Processed == 0)
            {
                Status = "Nothing found to process.";
            }
            else
            {
                Status = $"All done. Processed {Processed} parts.";
            }
            Enabled = true;
        }


        public void UpdateSelectedTracksPhonemes()
        {
            Enabled = false;
            if (!Valid)
            {
                DisplayError("The file you selected is invalid !", true);
                return;
            }
            var selectedTracks = TrackEditorViewModel.Sequence.SelectedTracks;
            if (selectedTracks.Count == 0)
            {
                DisplayError("No tracks selected !", true);
                return;
            }
            foreach (var track in selectedTracks)
            {
                List<WIVSMPart> parts = track.Parts;
                ProcessParts(parts);
            }
        }


        public void UpdateSelectedPartsPhonemes()
        {
            Enabled = false;
            if (!Valid)
            {
                DisplayError("The file you selected is invalid !", true);
                return;
            }
            var parts = TrackEditorViewModel.Sequence.SelectedParts;
            if (parts.Count == 0)
            {
                DisplayError("No vocaloid part selected !", true);
                return;
            }
            ProcessParts(parts);
        }

        public void UpdateSelectedNotesPhonemes()
        {
            Enabled = false;
            if (!Valid)
            {
                DisplayError("The file you selected is invalid !", true);
                return;
            }
            WIVSMMidiPart activePart = MusicalEditor.ActivePart;
            if (activePart == null)
            {
                DisplayError("No active part selected !", true);
                return;
            }
            VoiceBank voicebank = App.DatabaseManager.GetVoiceBankByCompID(activePart.VoiceBankID);
            if (Config.TargetLanguage != "*" && activePart.LangIDFromVoiceBank != (int)Config.GetLanguageID())
            {
                DisplayError($"The language from the singer {voicebank.Name} ({ Enum.GetName(typeof(VSMLanguageID), ((VSMLanguageID)voicebank.LangID))}) does not correspond to the language from the converter ({Enum.GetName(typeof(VSMLanguageID),Config.GetLanguageID())}) this part will be skipped !", true);
                return;
            }
            if (Config.TargetVoicebank != "*" && voicebank.Name != Config.TargetVoicebank)
            {
                DisplayError($"The voicebank from {activePart.Name} ({voicebank.Name} doesn't correspond to the expected target voicebank ({Config.TargetVoicebank}, this part will be skipped !", true);
                return;
            }

            List<WIVSMNote> selectedNotes = activePart.Notes.Where(m => m.IsSelected).ToList();
            if (selectedNotes.Count == 0)
            {
                Status = "Nothing found to process.";
                Enabled = true;
                return;
            }
            ProcessNotes(ref selectedNotes);
            Status = $"All done. Processed {selectedNotes.Count} notes.";
            Enabled = true;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
