using DanTagsEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DanTagsEditor.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ModifyTagsViewCommand { get; set; }
        public RelayCommand CloseAppCommand { get; set; }
        public HomeViewModel HomeVM { get; set; }
        public ModifyTagsViewModel ModifyTagsVM { get; set; }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private object _currentView = new();
        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            ModifyTagsVM = new ModifyTagsViewModel();
            CloseAppCommand = new RelayCommand(CloseApp);

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            ModifyTagsViewCommand = new RelayCommand(o =>
            {
                CurrentView = ModifyTagsVM;
            });
        }

        private void CloseApp(object sender)
        {
            Application.Current.Shutdown();
        }
    }
}
