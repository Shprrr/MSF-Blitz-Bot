namespace MSFBlitzBot
{
    public class ViewModelLocator
    {
        public static OverlayViewModel OverlayViewModel { get; private set; } = new OverlayViewModel();
        public static BlitzViewModel BlitzViewModel { get; private set; } = new BlitzViewModel();
    }
}
