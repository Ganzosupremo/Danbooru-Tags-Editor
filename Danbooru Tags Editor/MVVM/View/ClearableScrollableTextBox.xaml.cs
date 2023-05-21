using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DanbooruTagsEditor.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClearableScrollableTextBox.xaml
    /// </summary>
    public partial class ClearableScrollableTextBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _TagsList = new List<string>();


        private string _Tags = "";

        public string ModifiedTags
        {
            get { return _Tags; }
            set 
            { 
                _Tags = value;
                OnPropertyChanged(nameof(ModifiedTags));
            }
        }

        public ClearableScrollableTextBox()
        {
            InitializeComponent();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            CSTextBox.Clear();
            CSTextBox.Focus();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CSTextBox.Text))
            {
                MessageBox.Show("Please provide the tags", "Warning No Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _Tags = CSTextBox.Text;
            _TagsList = SplitTags();
            ApplyRegexPatterns(_TagsList);

            ModifiedTags = string.Join(Environment.NewLine, _TagsList);
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
                    tagList[i] = numbersRegex.Replace(tagList[i], ",");
                }
                else
                {
                    tagList[i] = "Invalid Danbooru Tag";
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
