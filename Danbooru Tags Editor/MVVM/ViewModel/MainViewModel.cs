using Danbooru_Tags_Editor.Core;

namespace Danbooru_Tags_Editor.MVVM.ViewModel
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
