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
using System.Text.RegularExpressions;

namespace DanbooruTagsEditor.MVVM.ViewModel
{
    public class ModifyTagsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TextBox ModifiedTagsTextBox { get; set; }
        public TextBox UploadImageTextBox { get; set; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand DownloadFileCommand { get; }

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
        private const string _defaultImageExt = ".png";
        private const string _filterString = "Image files(*.bmp, *.jpg, *jpeg, *.png)|*.bmp;*.jpg;*.png;*.jpeg";
        private const string _textExt = ".txt";

        private bool _canDownloadFile = false;

        public ModifyTagsViewModel()
        {
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
            UploadImageCommand = new RelayCommand(UploadImage);
            DownloadFileCommand = new RelayCommand(DownloadFile);

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

        public void UploadImage(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = _defaultImageExt,
                Multiselect = true,
                Filter = _filterString,
                Title = "Select Image(s)...",
            };

            bool? response = openFileDialog.ShowDialog();
            
            if (response == true)
            {
                foreach (string imagePath in openFileDialog.FileNames)
                {
                    string imageName = Path.GetFileName(imagePath);
                    UploadImageTextBox.Text += imageName + Environment.NewLine;

                    string textContents = ModifiedTagsTextBox.Text;
                    CreateTextFileFromImage(imagePath, textContents);
                }
            }
        }

        private void CreateTextFileFromImage(string imagePath, string textContents)
        {
            if (string.IsNullOrEmpty(textContents))
            {
                MessageBox.Show("No Tags To Write To File. Cannot Proceed...", "No Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                UploadImageTextBox.Clear();
                return;
            }

            string imageName = Path.GetFileNameWithoutExtension(imagePath);
            string textFilePath = Path.Combine(Path.GetDirectoryName(imagePath), imageName + _textExt);

            if (File.Exists(textFilePath))
            {
                MessageBox.Show("Text file already exists. Check Your Directory");
                _canDownloadFile = true;
                return;
            }

            try
            {
                // Write the text contents to the text file
                File.WriteAllText(textFilePath, textContents);

                MessageBox.Show("Text File Created Successfully. Check Your Directory", "Text File Created", MessageBoxButton.OK, MessageBoxImage.Information);
                _canDownloadFile = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creating the text file: " + ex.Message);
            }
        }

        public void DownloadFile(object obj)
        {
            if (!_canDownloadFile) return;

            string[] uploadedFileNames = UploadImageTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (uploadedFileNames.Length == 0)
            {
                MessageBox.Show("Please upload an image first.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = _textExt,
                AddExtension = true,
                Title = "Save Text Files...",
                FileName = "", // Clear the default file name
            };

            bool? success = saveFileDialog.ShowDialog();

            if (success == true)
            {
                try
                {
                    string[] selectedFilePaths = saveFileDialog.FileNames;

                    //if (selectedFilePaths.Length != uploadedFileNames.Length)
                    //{
                    //    MessageBox.Show("Please select a destination for each uploaded image.");
                    //    return;
                    //}

                    for (int i = 0; i < selectedFilePaths.Length; i++)
                    {
                        string textFilePath = selectedFilePaths[i];
                        string textContents = ModifiedTagsTextBox.Text;

                        File.WriteAllText(textFilePath, textContents);

                        MessageBox.Show("Text file downloaded successfully.", "Download Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while downloading the text file: " + ex.Message);
                }


                //try
                //{
                //    string textFilePath = saveFileDialog.FileName;
                //    File.WriteAllText(textFilePath, ModifiedTagsTextBox.Text);
                //    MessageBox.Show("Text file downloaded successfully.", "Download Succesful", MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("An error occurred while downloading the text file: " + ex.Message);
                //}
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
