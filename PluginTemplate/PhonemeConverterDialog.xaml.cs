using System.Windows.Markup;
using Yamaha.VOCALOID.VOCALOID5;
using Yamaha.VOCALOID.VOCALOID5.Controls;
using Yamaha.VOCALOID.VOCALOID5.MusicalEditor;

namespace PhonemeConverter
{
    /// <summary>
    /// Interaction logic for ExampleDialog.xaml
    /// </summary>
    public partial class PhonemeConverterDialog : DialogBase
    {
        public PhonemeConverterDialogViewModel model { get; set; }
        public PhonemeConverterDialog()
        {
            model = new PhonemeConverterDialogViewModel();
            InitializeComponent();
            model.window = this;
            this.DataContext = model;
        }

        private void SelectedNotes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            model.UpdateSelectedNotesPhonemes();
            
        }

        private void SelectedParts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            model.UpdateSelectedPartsPhonemes();
        }


        private void SelectedTracks_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            model.UpdateSelectedTracksPhonemes();
        }

        private void SelectFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            model.SelectFile();
        }

    }
}
