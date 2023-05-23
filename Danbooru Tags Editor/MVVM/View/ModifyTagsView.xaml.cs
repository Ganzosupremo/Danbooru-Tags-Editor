using DanbooruTagsEditor.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DanbooruTagsEditor.MVVM.View
{
    /// <summary>
    /// Interaction logic for ModifyTagsView.xaml
    /// </summary>
    public partial class ModifyTagsView : UserControl
    {
        private List<string> _TagsList = new List<string>();
        private string _Tags = "";
        private MediaPlayer _mediaPlayer;

        public ModifyTagsView()
        {
            InitializeComponent();
            this.Loaded += ModifyTagsView_Loaded;
        }

        private void ModifyTagsView_Loaded(object sender, RoutedEventArgs e)
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

        private void MediaFailed(object sender, ExceptionEventArgs e)
        {
            var ex = e.ErrorException;
            MessageBox.Show($"MEDIA FAILED: {ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
        }

        private void MediaOpened(object sender, EventArgs e)
        {
            _mediaPlayer.Play();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TagsTextBox.CSTextBox.Text))
            {
                MessageBox.Show("Please provide the tags", "Warning No Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            PlaySound(@"pack://siteoforigin:,,,/Audio/ButtonClick3.mp3");

            _Tags = TagsTextBox.CSTextBox.Text;
            _TagsList = SplitTags();
            ApplyRegexPatterns(_TagsList);

            TagsTextBox.ModifiedTags = string.Join(" ", _TagsList);
        }

        private List<string> SplitTags()
        {
            try
            {
                string[] tagArray = _Tags.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(tagArray);
            }
            catch (Exception)
            {
                MessageBox.Show("Format Not Valid, Check the input.");
                return new List<string>();
            }
        }

        private void ApplyRegexPatterns(List<string> tagList)
        {
            Regex questionRegex = new Regex("^\\?\\s", RegexOptions.IgnoreCase);
            Regex numbersRegex = new Regex("(\\d+(\\.\\d+)?)([kM])?$", RegexOptions.IgnoreCase);

            for (int i = 0; i < tagList.Count; i++)
            {
                if (questionRegex.IsMatch(tagList[i]) && numbersRegex.IsMatch(tagList[i]))
                {
                    tagList[i] = questionRegex.Replace(tagList[i], "");
                    tagList[i] = numbersRegex.Replace(tagList[i],",");
                }
                else
                    tagList[i] = $"Invalid Danbooru Tag: {tagList[i]}.";
            }
        }

        private void PlaySound(string audioPath)
        {
            _mediaPlayer.Open(new Uri(audioPath, UriKind.Absolute));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ModifyTagsViewModel viewModel)
            {
                viewModel.ModifiedTagsTextBox = ModifiedTagsTextBox;
            }
        }
    }
}
