using System.ServiceModel;
using TRFCommonLibs;
using CSSTStages = CSSTCommonLibs.CSSTGlobalVariables.CSSTStages;
namespace CSSTCommonLibs
{
    [ServiceContract]
    public interface ICSSTServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotifyTRFMessage(TRFMessage message);
        [OperationContract(IsOneWay = true)]
        void NotifyCSSTSessionInsertConfirmation(bool insertResult);

        [OperationContract(IsOneWay = true)]
        void NotifyCSSTStage(TRFKeyValuePair<CSSTStages, CSSTStages> stage);
        [OperationContract(IsOneWay = true)]
        void NotifyCSSTAnalysisResult(CSSTAnalysisResult result);
        [OperationContract(IsOneWay = true)]
        void NotifyCSSTDataSaved(bool result);
    }
}
