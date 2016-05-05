namespace CSSTClientControlPanelModule
{
    /// <summary>
    /// Interaction logic for CSSTClientControlPanelView.xaml
    /// </summary>
    public partial class CSSTClientControlPanelView
    {
        public CSSTClientControlPanelView()
        {
            this.InitializeComponent();
            this.DataContext = new CSSTClientControlPanelViewModel();
        }
    }
}
