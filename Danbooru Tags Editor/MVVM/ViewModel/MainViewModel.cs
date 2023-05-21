using DanbooruTagsEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanbooruTagsEditor.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {


        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ModifyTagsViewCommand { get; set; }
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

        private object _currentView;
        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            ModifyTagsVM = new ModifyTagsViewModel();
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
    }
}
