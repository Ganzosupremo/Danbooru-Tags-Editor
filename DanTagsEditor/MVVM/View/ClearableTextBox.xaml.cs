using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DanTagsEditor.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClearableTextBox.xaml
    /// </summary>
    public partial class ClearableTextBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string ModifiedTags
        {
            get { return _Modifiedags; }
            set
            {
                _Modifiedags = value;
                OnPropertyChanged(nameof(ModifiedTags));
            }
        }
        public string DanTags
        {
            get { return _Tags; }
            set
            {
                _Tags = value;
                OnPropertyChanged(nameof(DanTags));
            }
        }

        private string _Tags = "";
        private string _Modifiedags = "";
        private MediaPlayer _mediaPlayer = new();

        public ClearableTextBox()
        {
            InitializeComponent();
            this.Loaded += ClearableTextBox_Loaded;

        }

        private void ClearableTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.MediaOpened += MediaOpened;
                _mediaPlayer.MediaFailed += MediaFailed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void MediaFailed(object? sender, ExceptionEventArgs e)
        {
            var ex = e.ErrorException;
            MessageBox.Show($"MEDIA FAILED: {ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
        }

        private void MediaOpened(object? sender, EventArgs e)
        {
            _mediaPlayer.Play();
        }

        //private void BtnClear_Click(object sender, RoutedEventArgs e)
        //{
        //    PlaySound(@"pack://siteoforigin:,,,/Audio/ButtonClick2.mp3");
        //    clearableTextBox.Clear();
        //    clearableTextBox.Focus();
        //}

        //private void PlaySound(string uri)
        //{
        //    _mediaPlayer.Open(new Uri(uri, UriKind.Absolute));
        //}

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(clearableTextBox.Text))
                TBPlaceholderText.Visibility = Visibility.Visible;
            else
                TBPlaceholderText.Visibility = Visibility.Hidden;
        }

        private void TBPlaceholderText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clearableTextBox.Focus();
        }
    }
}
