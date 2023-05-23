using DanbooruTagsEditor.Core;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NAudio.Wave;

namespace DanbooruTagsEditor.MVVM.ViewModel
{
    public class ModifyTagsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TextBox ModifiedTagsTextBox { get; set; }
        public ICommand CopyToClipboardCommand { get; }

        public string ToolTipText
        {
            get { return _toolTipText; }
            set
            {
                _toolTipText = value;
                OnPropertyChanged(nameof(ToolTipText));
            }
        }

        private string _toolTipText;
        private MediaPlayer _mediaPlayer;

        public ModifyTagsViewModel()
        {
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);

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

        private void MediaFailed(object sender, ExceptionEventArgs e)
        {
            var ex = e.ErrorException;
            MessageBox.Show($"MEDIA FAILED: {ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
        }

        private void MediaOpened(object sender, EventArgs e)
        {
            _mediaPlayer.Play();
        }

        public void CopyToClipboard(object obj)
        {
            string textToCopy = ModifiedTagsTextBox.Text;
            if (!string.IsNullOrEmpty(textToCopy))
            {
                PlaySound(filePath: @"pack://siteoforigin:,,,/Audio/ButtonClick4.mp3");
                Clipboard.SetText(textToCopy);
                ToolTipText = "Text copied to clipboard.";
            }
            else 
            {
                PlaySound(filePath: @"pack://siteoforigin:,,,/Audio/QuickFart.mp3");
                ToolTipText = "No text to copy.";
            }
        }

        private void PlaySound(string filePath)
        {
            _mediaPlayer.Open(new Uri(filePath, UriKind.Absolute));
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
