using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Prism.Commands;
using TRFCommonLibs;
using System.Runtime.InteropServices;

namespace CSSTServerModule
{
    [Guid("0CD917A2-004F-4679-81DA-8F878F2BCF30")]
    public class CSSTServerViewModel : TRFNotifyPropertyChanged
    {
        private readonly Guid _guid = typeof (CSSTServerViewModel).GUID;
        private readonly CSSTServerModel _csstServerModel;  // model
        private CSSTServiceHost _csstServiceHost;   //Customised service host

        /// <summary>
        /// To display content on Start/Stop button
        /// </summary>
        public string startStopButtonContent
        {
            get { return this._csstServerModel.startStopButtonContent; }
            set
            {
                if (this._csstServerModel.startStopButtonContent == value) return;
                this._csstServerModel.startStopButtonContent = value;
                this.NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// To handle start/stop button click event
        /// </summary>
        public DelegateCommand<object> startStopButtonClick { get; private set; }

        public CSSTServerViewModel()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CSSTAssemblyResolveHandler;

            this._csstServerModel = new CSSTServerModel();
            this.startStopButtonClick = new DelegateCommand<object>(this.RunStartStopButtonClickCommand);
        }

        private Assembly CSSTAssemblyResolveHandler(object sender, ResolveEventArgs e)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.
            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = "";

            var objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            if (arrReferencedAssmbNames.Any(strAssmbName => strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",", StringComparison.Ordinal)) == e.Name.Substring(0, e.Name.IndexOf(",", StringComparison.Ordinal))))
            {
                strTempAssmbPath = Environment.CurrentDirectory + "\\ServerUserModules\\";
                if (strTempAssmbPath.EndsWith("\\")) strTempAssmbPath += "\\";
                strTempAssmbPath += e.Name.Substring(0, e.Name.IndexOf(",", StringComparison.Ordinal)) + ".dll";
            }
            //Load the assembly from the specified path.
            Assembly myAssembly = null;
            if (!string.IsNullOrWhiteSpace(strTempAssmbPath))
                myAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return myAssembly;
        }

        #region Start/Stop Button
        private void RunStartStopButtonClickCommand(object obj)
        {
            string message = "";
            try
            {
                if (this._csstServiceHost == null || this._csstServiceHost.State != CommunicationState.Opened)
                {
                    this._csstServiceHost = new CSSTServiceHost(typeof(CSSTServerService));
                    this._csstServiceHost.Open();
                    message = "CSST service has been turned on.";
                    TRFEventHelpers.SendTRFStatusIndicatorChangedEvent(this._guid, new TRFStatusIndicator() {moduleName = typeof(CSSTServerModule).Name, status = true, tip = typeof(CSSTServerModule).Name });

                }
                else // if service host already exists and it is opened, then close it.
                {
                    this._csstServiceHost.Close();
                    message = "CSST service has been turned off.";
                    TRFEventHelpers.SendTRFStatusIndicatorChangedEvent(this._guid, new TRFStatusIndicator() { moduleName = typeof(CSSTServerModule).Name, status = false, tip = typeof(CSSTServerModule).Name });
                }
            }
            catch (Exception e)
            {
                message = "Cannot turn on CSST service. " + e.Message;
            }
            //Record log
            TRFGlobalVariables.SystemLogger.Info(message);
            //Display message in status bar
            TRFEventHelpers.SendTRFMessageReadyEvent(this._guid, new TRFMessage(message));
        }
        #endregion
    }
}
