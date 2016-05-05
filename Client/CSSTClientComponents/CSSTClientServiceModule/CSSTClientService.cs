using System;
using System.Runtime.InteropServices;
using System.ServiceModel;
using CSSTCommonLibs;

using Prism.Events;
using TRFCommonLibs;

namespace CSSTClientServiceModule
{
    [Guid("EA8711A2-AB9D-4333-AB37-B5DCB895A5F1")]
    public class CSSTClientService
    {
        private readonly Guid _guid = typeof (CSSTClientService).GUID;
        private readonly SubscriptionToken _csstSaveSessionRequestedEventST;
        private readonly SubscriptionToken _csstUserLoginRequestedEventST;
        private readonly SubscriptionToken _trfKinectBodyDataFrameReadyEventST;
        private readonly SubscriptionToken _csstTerminationRequestedEventST;
        private readonly SubscriptionToken _trfClientWindowClosingEventST;
        private readonly ICSSTService _csstServiceProxy;
        private bool _isServerAlive;  
        public CSSTClientService()
        {
            CSSTServiceCallback csstServiceCallback = new CSSTServiceCallback();
            TRFDuplexChannelFactory<ICSSTService>.configutatyionPath = "ClientUserModules\\CSSTClientComponents\\CSSTClientServiceModule.config";
            TRFDuplexChannelFactory<ICSSTService> channel = new TRFDuplexChannelFactory<ICSSTService>();
            this._csstServiceProxy = channel.CreateChannel(new InstanceContext(csstServiceCallback));

            this._csstSaveSessionRequestedEventST = CSSTEventHelpers.ReceiveCSSTSaveSessionRequestedEvent(this.CSSTSessionReady,
                arg => arg.receivers.Contains(this._guid));
            this._csstUserLoginRequestedEventST = CSSTEventHelpers.ReceiveCSSTUserLoginRequestedEvent(this.CSSTUserLoginRequested,
                arg => arg.receivers.Contains(this._guid));
            this._trfKinectBodyDataFrameReadyEventST = TRFEventHelpers.ReceiveTRFKinectBodyDataFrameReadyEvent(this.TRFKinectBodyDataFrameReady, arg => arg.receivers.Contains(this._guid));
            this._csstTerminationRequestedEventST = CSSTEventHelpers.ReceiveCSSTTerminationRequestedEvent(this.CSSTTerminationRequested,
                arg => arg.receivers.Contains(this._guid));
            this._trfClientWindowClosingEventST = TRFEventHelpers.ReceiveTRFClientWindowClosingEvent(this.TRFClientWindowClosing);
        }

        ~CSSTClientService()
        {
            CSSTEventHelpers.DisposeCSSTSaveSessionRequestedEvent(this._csstSaveSessionRequestedEventST);
            CSSTEventHelpers.DisposeCSSTUserLoginRequestedEvent(this._csstUserLoginRequestedEventST);
            TRFEventHelpers.DisposeTRFKinectBodyNumberChangedEvent(this._trfKinectBodyDataFrameReadyEventST);
            CSSTEventHelpers.DisposeCSSTTerminationRequestedEvent(this._csstTerminationRequestedEventST);
        }

        #region Event Handlers

        private void CSSTSessionReady(TRFEventArg e)
        {
            try
            {
                this._csstServiceProxy.SendCSSTSession((CSSTSession)e.payload);
                this._isServerAlive = true;
            }
            catch
            {
                this._isServerAlive = false;
                // ignored
            }
        }

        private void CSSTUserLoginRequested(TRFEventArg e)
        {
            try
            {
                this._csstServiceProxy.SendCSSTUserLoginRequest((TRFUser) e.payload);
                this._isServerAlive = true;
            }
            catch
            {
                this._isServerAlive = false;
                // ignored
            }

        }

        private void TRFKinectBodyDataFrameReady(TRFEventArg e)
        {
            try
            {
                this._csstServiceProxy.SendBodyDataFrame((TRFBody3D)e.payload);
                this._isServerAlive = true;
            }
            catch
            {
                this._isServerAlive = false;
                // ignored
            }
        }

        private void CSSTTerminationRequested(TRFEventArg e)
        {
            try
            {
                this._csstServiceProxy.SendTerminateCSST();
                this._isServerAlive = true;
            }
            catch
            {
                this._isServerAlive = false;
                // ignored
            }
        }

        private void TRFClientWindowClosing(TRFEventArg e)
        {
            try
            {
                this._csstServiceProxy.RequestCSSTClientWindowClose();
                this._isServerAlive = true;
            }
            catch
            {
                this._isServerAlive = false;
                // ignored
            }
        }
        #endregion
    }
}
