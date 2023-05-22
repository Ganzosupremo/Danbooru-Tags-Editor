using DanbooruTagsEditor.Core;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        }

        public void CopyToClipboard(object obj)
        {
            string textToCopy = ModifiedTagsTextBox.Text;
            if (!string.IsNullOrEmpty(textToCopy))
            {
                Clipboard.SetText(textToCopy);
                ToolTipText = "Text copied to clipboard.";
            }
            else
                ToolTipText = "No text to copy.";
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
