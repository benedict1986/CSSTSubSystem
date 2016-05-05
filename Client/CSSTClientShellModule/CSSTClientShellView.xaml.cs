namespace CSSTClientShellModule
{
    /// <summary>
    /// Interaction logic for CSSTClientShellView.xaml
    /// </summary>
    public partial class CSSTClientShellView
    {
        public CSSTClientShellView()
        {
            this.InitializeComponent();
            this.DataContext = new CSSTClientShellViewModel();
        }
    }
}
