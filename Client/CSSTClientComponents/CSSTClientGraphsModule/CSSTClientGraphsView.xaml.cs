namespace CSSTClientGraphsModule
{
    /// <summary>
    /// Interaction logic for CSSTClientGraphsView.xaml
    /// </summary>
    public partial class CSSTClientGraphsView
    {
        public CSSTClientGraphsView()
        {
            this.InitializeComponent();
            this.DataContext = new CSSTClientGraphsViewModel();
        }
    }
}
