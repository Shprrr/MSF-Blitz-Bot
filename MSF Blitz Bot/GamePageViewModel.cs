namespace MSFBlitzBot
{
    public abstract class GamePageViewModel : ObservableObject
    {
        private string _pageName;

        public string PageName
        {
            get => _pageName;
            set
            {
                _pageName = value;
                RaisePropertyChanged(nameof(PageName));
            }
        }

        public abstract void Loop();

        public abstract void OnOpen();

        public abstract void OnClose();
    }
}
