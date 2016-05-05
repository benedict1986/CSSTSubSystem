namespace CSSTServerModule
{
    /// <summary>
    /// Interaction logic for CSSTServerView.xaml
    /// </summary>
    public partial class CSSTServerView
    {
        public CSSTServerView()
        {
            this.InitializeComponent();
            this.DataContext = new CSSTServerViewModel();
        }
    }
}
