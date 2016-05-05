using System;
using System.Collections.Generic;
using CSSTCommonLibs;
using TRFCommonLibs;
using System.Runtime.InteropServices;
using CSSTStages = CSSTCommonLibs.CSSTGlobalVariables.CSSTStages;
namespace CSSTClientServiceModule
{
    [Guid("210D694B-E981-487B-A72D-80B44ED0A971")]
    public class CSSTServiceCallback : ICSSTServiceCallback
    {
        private readonly Guid _guid = typeof (CSSTServiceCallback).GUID;
        public void NotifyTRFMessage(TRFMessage message)
        {
            TRFEventHelpers.SendTRFMessageReadyEvent(this._guid, message);
        }
        public void NotifyCSSTSessionInsertConfirmation(bool insertResult)
        {
            //Console.WriteLine(insertResult);
        }

        public void NotifyCSSTStage(TRFKeyValuePair<CSSTStages, CSSTStages> stage)
        {
            CSSTEventHelpers.SendCSSTStageChangedEvent(this._guid, stage);
            if (stage.value == CSSTStages.CSSTSessionDataReceived)
            {
                TRFEventHelpers.SendTRFKinectStartStopRequestedEvent(this._guid, true, CSSTGlobalVariables.Event.Receiver["CSSTClientService"]);
            }
            else if (stage.value == CSSTStages.CSSTStopped)
                TRFEventHelpers.SendTRFKinectStartStopRequestedEvent(this._guid, false, new Guid());
        }

        public void NotifyCSSTAnalysisResult(CSSTAnalysisResult result)
        {
            CSSTEventHelpers.SendCSSTDataAnalysisResultReadyEvent(this._guid, result, new List<Guid> { CSSTGlobalVariables.Event.Receiver["CSSTClientGraphsViewModel"], CSSTGlobalVariables.Event.Receiver["CSSTClientStatisticViewModel"]});
        }

        public void NotifyCSSTDataSaved(bool result)
        {
            CSSTEventHelpers.SendCSSTDataSavedEvent(this._guid, result);
        }
    }
}
