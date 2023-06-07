using DanTagsEditor.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using DanTagsEditor.MVVM.View;
using Forms = System.Windows.Forms;

namespace DanTagsEditor.MVVM.ViewModel
{
    public class ModifyTagsViewModel : ObservableObject
    {
        public TextBox ModifiedTagsTextBox { get; set; } = new TextBox();
        public TextBox UploadImageTextBox { get; set; } = new TextBox();
        public CheckBox MultipleImagesCB { get; set; } = new CheckBox();
        public ClearableTextBox ClearableTextBox { get; set; } = new ClearableTextBox();
        public RelayCommand CopyToClipboardCommand { get; }
        public RelayCommand UploadImageCommand { get; }
        public RelayCommand SelectFolderCommand { get; }
        public RelayCommand SelectTextFileCommand { get; }
        public RelayCommand ClearTxtBoxesCommand { get; set; }
        public List<List<string>> TagsLists { get => _TagsLists; set => _TagsLists = value; }

        public string ToolTipText
        {
            get { return _toolTipText; }
            set
            {
                _toolTipText = value;
                OnPropertyChanged(nameof(ToolTipText));
            }
        }
        public string TextFileText
        {
            get => _textFileText;
            set
            {
                _textFileText = value;
                OnPropertyChanged(nameof(TextFileText));
            }
        }

        private string _toolTipText = "";
        private string _textFileText = "";
        private MediaPlayer _mediaPlayer = new();
        private List<List<string>> _TagsLists = new();

        private const string _defaultImageExt = ".png";
        private const string _filterString = "Image files(*.bmp, *.jpg, *jpeg, *.png)|*.bmp;*.jpg;*.png;*.jpeg";
        private const string _textExt = ".txt";

        private bool _canDownloadFile = false;

        public ModifyTagsViewModel()
        {
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
            UploadImageCommand = new RelayCommand(UploadImage);
            SelectFolderCommand = new RelayCommand(OpenFolderCommand);
            SelectTextFileCommand = new RelayCommand(SelectTextCommand);
            ClearTxtBoxesCommand = new RelayCommand(ClearTextBoxes);

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

        private void ClearTextBoxes(object obj)
        {
            ModifiedTagsTextBox.Clear();
            UploadImageTextBox.Clear();
            ClearableTextBox.clearableTextBox.Clear();
        }

        /// <summary>
        /// A Command to copy the contents to clipboard
        /// </summary>
        /// <param name="obj"></param>
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

        /// <summary>
        /// A command to prompt the user to select an image file
        /// </summary>
        /// <param name="obj"></param>
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
                string[] imagePaths = openFileDialog.FileNames;
                string[] tagGroups = ModifiedTagsTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (imagePaths.Length != tagGroups.Length)
                {
                    MessageBox.Show("Number of selected images does not match the number of tag groups.");
                    return;
                }

                for (int i = 0; i < imagePaths.Length; i++)
                {
                    string imagePath = imagePaths[i];
                    string imageName = Path.GetFileName(imagePath);
                    UploadImageTextBox.Text += imageName + Environment.NewLine;

                    string textContents = tagGroups[i];
                    CreateTextFileFromImage(imagePath, textContents);
                }
            }
        }

        /// <summary>
        /// A Command to prompt the user to select a text file
        /// containing the tags
        /// </summary>
        /// <param name="obj"></param>
        private void SelectTextCommand(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Multiselect = false,
                Filter = "Text File(*.txt, *.json)|*.txt;*.json",
                Title = "Select Text File...",
            };

            bool? response = openFileDialog.ShowDialog();

            if (response == true)
            {
                ClearableTextBox.DanTags = openFileDialog.FileName;
                ClearableTextBox.clearableTextBox.Text = openFileDialog.FileName;

                // mark it as check automatically, because if the user selects a file
                // presumably the file contains more than one tag group
                MultipleImagesCB.IsChecked = true;
                ReadFileContents(openFileDialog.FileName);
            }
        }

        private void ReadFileContents(string fileName)
        {

            try
            {
                string contents = File.ReadAllText(fileName);
                ClearableTextBox.clearableTextBox.Text = contents;
                ClearableTextBox.DanTags = contents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error Ocurred While Trying Reading the File Contents. {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// A Command to prompt the user to select a folder containing the source images.
        /// </summary>
        /// <param name="obj"></param>
        private void OpenFolderCommand(object obj)
        {
            string folderPath = SelectFolder();

            ProcessImagesInFolder(folderPath);
        }

        private void ProcessImagesInFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                // Folder path is empty or null
                return;

            // Split the Tag groups
            string[] tagGroups = ModifiedTagsTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (tagGroups.Length == 0)
            {
                MessageBox.Show("Before creating text files, paste the tags on the first textbox or select a text file" +
                    " with the tags, and then press Submit...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UploadImageTextBox.Text = folderPath;

            // Get all files in the folder with image extensions
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            string[] imageFiles = Directory.GetFiles(folderPath)
                                           .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                                           .OrderBy(file => File.GetCreationTime(file))
                                           .ToArray();
            
            // Sort the image files by creation date
            //Array.Sort(imageFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            if (imageFiles.Length > _TagsLists.Count)
                MessageBox.Show("There are more images in the folder than tag groups entered. " +
                    "Some images will not have a corresponding text file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Process each image file
            for (int i = 0; i < Math.Min(imageFiles.Length, tagGroups.Length); i++)
            {
                string imagePath = imageFiles[i];
                string imageName = Path.GetFileName(imagePath);
                CreateTextFileFromImage(imageFiles[i], tagGroups[i]);
            }
            MessageBox.Show("Text File Created Successfully. Check Your Directory", "Text File Created", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string SelectFolder()
        {
            Forms.FolderBrowserDialog folderBrowserDialog = new Forms.FolderBrowserDialog
            {
                Description = "Select Images Source Folder...",
                ShowNewFolderButton = true,
            };
            var result = folderBrowserDialog.ShowDialog();

            if (result == Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                //string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                //string[] imageFiles = Directory.GetFiles(folderPath, "*.*") // Get all files
                //    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower())) // Filter by file extension
                //    .OrderBy(file => File.GetCreationTime(file)) // Sort by creation date in ascending order
                //    .ToArray();

                //// Sort the image files by creation date
                //Array.Sort(imageFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                return folderPath;
            }

            return string.Empty;
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
            string textFilePath = Path.Combine(path1: Path.GetDirectoryName(imagePath)!, imageName + _textExt);

            if (File.Exists(textFilePath))
                MessageBox.Show("Text file already exists. Will override it", "Override Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            try
            {
                // Write the text contents to the text file
                File.WriteAllText(textFilePath, textContents);

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

        private void PlaySound(string filePath)
        {
            _mediaPlayer.Open(new Uri(filePath, UriKind.Absolute));
        }
    }
}
