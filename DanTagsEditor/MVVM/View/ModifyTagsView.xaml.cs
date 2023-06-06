using DanTagsEditor.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for ModifyTagsView.xaml
    /// </summary>
    public partial class ModifyTagsView : UserControl
    {
        private List<string> _TagsList = new();
        private List<List<string>> _TagLists = new();
        private string _Tags = "";
        private MediaPlayer _mediaPlayer = new();
        private ModifyTagsViewModel _modifyTagsViewModel = new();

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

        private void MediaFailed(object? sender, ExceptionEventArgs e)
        {
            var ex = e.ErrorException;
            MessageBox.Show($"MEDIA FAILED: {ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
        }

        private void MediaOpened(object? sender, EventArgs e)
        {
            _mediaPlayer.Play();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (MultipleImagesCheckBox.IsChecked == true)
            {
                SubmitMultipleTagGroups();
            }
            else if (MultipleImagesCheckBox.IsChecked == false)
            {
                SubmitOneTagGroup();
            }
        }

        private void SubmitOneTagGroup()
        {
            if (string.IsNullOrEmpty(TagsTextBox.clearableTextBox.Text))
            {
                MessageBox.Show("Please provide the tags", "Warning No Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            PlaySound(@"pack://siteoforigin:,,,/Audio/ButtonClick3.mp3");

            _Tags = TagsTextBox.clearableTextBox.Text;
            _TagsList = SplitTagsList();
            ApplyRegexPatterns(_TagsList);

            TagsTextBox.ModifiedTags = string.Join(" ", _TagsList);
        }

        private void SubmitMultipleTagGroups()
        {
            if (string.IsNullOrEmpty(TagsTextBox.clearableTextBox.Text))
            {
                MessageBox.Show("Please provide the tags", "Warning No Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            PlaySound(@"pack://siteoforigin:,,,/Audio/ButtonClick3.mp3");

            _Tags = TagsTextBox.clearableTextBox.Text;
            _TagLists = SplitTagsLists();
            _modifyTagsViewModel.TagsLists = _TagLists;
            ApplyRegexPatterns(_TagLists);

            StringBuilder modifiedTagsBuilder = new();
            foreach (List<string> imageTags in _TagLists)
            {
                modifiedTagsBuilder.AppendLine(string.Join(" ", imageTags));
            }
            TagsTextBox.ModifiedTags = modifiedTagsBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// Splits the tag on the list
        /// </summary>
        /// <param name="i"></param>
        /// <returns>The list with the tags splitted on a new line.</returns>
        private List<string> SplitTagsList()
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

        /// <summary>
        /// Splits the List of tag lists
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The list of tag lists and the tags within are splitted on a new line.</returns>
        private List<List<string>> SplitTagsLists()
        {
            try
            {
                string[] imageTagsArray = _Tags.Split(new string[] { Environment.NewLine + "//" }, StringSplitOptions.RemoveEmptyEntries);
                List<List<string>> tagLists = new List<List<string>>();

                foreach (string imageTags in imageTagsArray)
                {
                    string[] tagArray = imageTags.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    tagLists.Add(new List<string>(tagArray));
                }

                return tagLists;
            }
            catch (Exception)
            {
                MessageBox.Show("Format Not Valid, Check the input.");
                return new List<List<string>>();
            }
        }

        private void ApplyRegexPatterns(List<string> tagList)
        {
            Regex questionRegex = QuestionSignRegex();
            Regex numbersRegex = NumbersRegex();

            for (int i = 0; i < tagList.Count; i++)
            {
                if (questionRegex.IsMatch(tagList[i]) && numbersRegex.IsMatch(tagList[i]))
                {
                    tagList[i] = questionRegex.Replace(tagList[i], "");
                    tagList[i] = numbersRegex.Replace(tagList[i], ",");
                }
                else
                    tagList[i] = $"Invalid Danbooru Tag: {tagList[i]}.";
            }
        }

        private void ApplyRegexPatterns(List<List<string>> tagLists)
        {
            Regex questionRegex = QuestionSignRegex();
            Regex numbersRegex = NumbersRegex();

            for (int i = 0; i < tagLists.Count; i++)
            {
                List<string> tagList = tagLists[i];

                for (int j = 0; j < tagList.Count; j++)
                {
                    if (questionRegex.IsMatch(tagList[j]) && numbersRegex.IsMatch(tagList[j]))
                    {
                        tagList[j] = questionRegex.Replace(tagList[j], "");
                        tagList[j] = numbersRegex.Replace(tagList[j], ",");
                    }
                    else
                        tagList[j] = $"Invalid Danbooru Tag: {tagList[j]}.";
                }
            }
        }

        private void PlaySound(string audioPath)
        {
            _mediaPlayer.Open(new Uri(audioPath));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ModifyTagsViewModel viewModel)
            {
                viewModel.ModifiedTagsTextBox = ModifiedTagsTextBox;
                viewModel.UploadImageTextBox = UploadImageTextBox;
                viewModel.ClearableTextBox = TagsTextBox;
                viewModel.MultipleImagesCB = MultipleImagesCheckBox;
                _modifyTagsViewModel = viewModel;
                _modifyTagsViewModel.TagsLists = _TagLists;
            }
        }

        [GeneratedRegex("^\\?\\s", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex QuestionSignRegex();
        
        [GeneratedRegex("(\\d+(\\.\\d+)?)([kM])?$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NumbersRegex();
    }
}
