namespace CSSTClientStatisticModule
{
    /// <summary>
    /// Interaction logic for CSSTClientStatisticView.xaml
    /// </summary>
    public partial class CSSTClientStatisticView
    {
        public CSSTClientStatisticView()
        {
            this.InitializeComponent();
            this.DataContext = new CSSTClientStatisticViewModel();
        }
    }
}
