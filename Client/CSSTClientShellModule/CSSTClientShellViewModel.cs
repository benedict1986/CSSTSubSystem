using TRFCommonLibs;

namespace CSSTClientShellModule
{
    public class CSSTClientShellViewModel : TRFNotifyPropertyChanged
    {
        private readonly CSSTClientShellModel _csstClientShellModel;
        public string tabHeader => this._csstClientShellModel.tabHeader;

        public CSSTClientShellViewModel()
        {
            this._csstClientShellModel = new CSSTClientShellModel();
        }
    }
}
