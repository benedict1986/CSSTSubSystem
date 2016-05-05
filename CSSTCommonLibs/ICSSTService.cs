using System.ServiceModel;
using TRFCommonLibs;

namespace CSSTCommonLibs
{
    [ServiceContract(CallbackContract = typeof(ICSSTServiceCallback))]
    public interface ICSSTService
    {
        [OperationContract(IsOneWay = true)]
        void SendCSSTUserLoginRequest(TRFUser trfUser);
        [OperationContract(IsOneWay = true)]
        void SendCSSTSession(CSSTSession session);
        [OperationContract(IsOneWay = true)]
        void SendBodyDataFrame(TRFBody3D body3D, TRFBody2D body2D = null);
        [OperationContract(IsOneWay = true)]
        void SendTerminateCSST();

        [OperationContract(IsOneWay = true)]
        void RequestCSSTClientWindowClose();
    }
}
