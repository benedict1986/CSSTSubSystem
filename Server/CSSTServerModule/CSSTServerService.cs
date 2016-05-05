using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using CSSTCommonLibs;
using System.Runtime.InteropServices;
using System.Threading;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using Prism.Events;
using TRFCommonLibs;
using CSSTStages = CSSTCommonLibs.CSSTGlobalVariables.CSSTStages;
namespace CSSTServerModule
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [Guid("D643A2F7-3327-4DAE-8909-4F0CA2187FDC")]
    public class CSSTServerService : ICSSTService
    {
        private TRFUser _loginTrfUser;
        private readonly Guid _guid = typeof (CSSTServerService).GUID;
        private readonly SubscriptionToken _csstSessionInsertConfirmedEventST;
        private readonly SubscriptionToken _trfUserUpdateConfirmedEventST;
        private readonly SubscriptionToken _trfUserInsertConfirmedEventST;
        private readonly SubscriptionToken _csstDataAnalysisResultReadyEventST;
        private readonly SubscriptionToken _trfSaveMotionDataFinishedEventST;
        private readonly CSSTServiceChannelManager _csstServiceChannelManager;
        private TRFKeyValuePair<CSSTStages, CSSTStages> _csstStage;
        private bool _isCountDownTerminated;
        private CSSTSession _csstSession;
        private CSSTGlobalVariables.MotionDatPurpose _motionDatPurpose;

        private bool _isTRFUserSaved;
        private bool _isCSSTSessionSaved;
        private bool _isMotionDataSaved;

        public CSSTServerService()
        {
            this._csstServiceChannelManager = new CSSTServiceChannelManager();
            this._csstStage = new TRFKeyValuePair<CSSTStages, CSSTStages>(CSSTStages.Logout, CSSTStages.Logout);
            this._csstSessionInsertConfirmedEventST = CSSTEventHelpers.ReceiveCSSTSessionInsertConfirmedEvent(this.CSSTSessionInsertConfirmed,
                arg => arg.receivers.Contains(this._guid));
            this._trfUserUpdateConfirmedEventST =
                TRFEventHelpers.ReceiveTRFUserUpdateConfirmedEvent(this.TRFUserUpdateConfirmed,
                    arg => arg.receivers.Contains(this._guid));
            this._trfUserInsertConfirmedEventST = TRFEventHelpers.ReceiveTRFUserInsertConfirmedEvent(this.TRFUserInsertConfirmed, arg => arg.receivers.Contains(this._guid));
            this._csstDataAnalysisResultReadyEventST = CSSTEventHelpers.ReceiveCSSTDataAnalysisResultReadyEvent(this.CSSTDataAnalysisResultReady,
                arg => arg.receivers.Contains(this._guid));
            this._trfSaveMotionDataFinishedEventST = TRFEventHelpers.ReceiveTRFSaveMotionDataFinishedEvent(this.TRFSaveMotionDataFinished,
                arg => arg.receivers.Contains(this._guid));


            this._isCountDownTerminated = false;
            this._isTRFUserSaved = false;
            this._isCSSTSessionSaved = false;
            this._isMotionDataSaved = false;
        }

        ~CSSTServerService()
        {
            CSSTEventHelpers.DisposeCSSTSessionInsertConfirmedEvent(this._csstSessionInsertConfirmedEventST);
            TRFEventHelpers.DisposeTRFUserUpdateConfirmedEvent(this._trfUserUpdateConfirmedEventST);
            TRFEventHelpers.DisposeTRFUserInsertConfirmedEvent(this._trfUserInsertConfirmedEventST);
            CSSTEventHelpers.DisposeCSSTDataAnalysisResultReadyEvent(this._csstDataAnalysisResultReadyEventST);
            TRFEventHelpers.DisposeTRFSaveMotionDataFinishedEvent(this._trfSaveMotionDataFinishedEventST);
        }

        public void SendCSSTSession(CSSTSession session)
        {
            this._csstSession = session;
            this._csstSession.trfUser.motion3D = new List<TRFBody3D>();
            this._csstSession.trfUser.motion2D = new List<TRFBody2D>();
            this.ChangeCSSTStage(CSSTStages.CSSTSessionDataReceived);
            this._motionDatPurpose = CSSTGlobalVariables.MotionDatPurpose.Calibration;
            this.CountDownThread(3, this.CSSTCalibrationBeginningAction, this.CSSTCalibrationRunningAction, this.CSSTCalibrationEnddingAction);
        }

        public void SendCSSTUserLoginRequest(TRFUser trfUser)
        {
            this._loginTrfUser = trfUser;
            this._csstServiceChannelManager.CSSTChannelRegister(trfUser.id, OperationContext.Current.GetCallbackChannel<ICSSTServiceCallback>());
            this.ChangeCSSTStage(CSSTStages.Login);
        }

        public void SendBodyDataFrame(TRFBody3D body3D, TRFBody2D body2D = null)
        {
            body3D.SetPurpose((sbyte)this._motionDatPurpose);
            body3D.DecomposeMyBodyObject();
            this._csstSession.trfUser.motion3D.Add(body3D);
            if (body2D != null)
            {
                body2D.SetPurpose((sbyte)this._motionDatPurpose);
                body2D.DecomposeMyBodyObject();
                this._csstSession.trfUser.motion2D.Add(body2D);
            }

            //Console.WriteLine(this._csstSession.trfUser.motion3D.Count);
        }

        public void SendTerminateCSST()
        {
            this._isCountDownTerminated = true;
        }

        public void RequestCSSTClientWindowClose()
        {
            this._csstServiceChannelManager.CSSTChannelUnRegister(this._loginTrfUser.id);
            this._isCountDownTerminated = false;
            this._csstSession = null;
            this._motionDatPurpose = CSSTGlobalVariables.MotionDatPurpose.Calibration;
            this._csstStage = new TRFKeyValuePair<CSSTStages, CSSTStages>(CSSTStages.Logout, CSSTStages.Logout);
            this._isTRFUserSaved = false;
            this._isCSSTSessionSaved = false;
            this._isMotionDataSaved = false;
            this._loginTrfUser = null;
        }
        #region Event Handlers
        private void CSSTSessionInsertConfirmed(TRFEventArg e)
        {
            this._isCSSTSessionSaved = true;
            if (this._isTRFUserSaved == true && this._isMotionDataSaved == true)
            {
                this.CSSTDataSaved();
            }
            //this._csstServiceChannelManager.NotifyCSSTSessionInsertConfirmation(this._loginTrfUser.id, (bool)e.payload);
        }

        private void TRFUserUpdateConfirmed(TRFEventArg e)
        {
            bool updateResult = (bool)e.payload;
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id,
                updateResult ? new TRFMessage("User has been updated.") : new TRFMessage("Fail to update user."));

            this._isTRFUserSaved = true;
            if (this._isCSSTSessionSaved == true && this._isMotionDataSaved == true)
            {
                this.CSSTDataSaved();
            }
        }

        private void TRFUserInsertConfirmed(TRFEventArg e)
        {
            if ((int) e.payload == 0)
                this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id,
                    new TRFMessage("Fail to insert user information."));
            else
            {
                this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id,
                    new TRFMessage("A new user has been inserted."));
                this._csstSession.trfUserId = (int) e.payload;
                CSSTEventHelpers.SendCSSTSaveSessionRequestedEvent(this._guid, this._csstSession);

                this._isTRFUserSaved = true;
                if (this._isCSSTSessionSaved == true && this._isMotionDataSaved == true)
                {
                    this.CSSTDataSaved();
                }
            }
        }

        private void CSSTDataAnalysisResultReady(TRFEventArg e)
        {
            if ((CSSTAnalysisResult) e.payload == null)
            {
                this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id,
                    new TRFMessage("Fail to analyse data. Please do the test again."));
                this.ChangeCSSTStage(CSSTStages.CSSTFinished);
                return;
            }

            this._csstServiceChannelManager.NotifyCSSTAnalysisResult(this._loginTrfUser.id, (CSSTAnalysisResult)e.payload);

            TRFEventHelpers.SendTRFSaveMotionDataRequestedEvent(this._guid, new ArrayList() {this._csstSession.dataTableName, this._csstSession.trfUser.motion3D, this._csstSession.trfUser.motion2D});
            if (this._csstSession.trfUser.trfUserOperation == TRFGlobalVariables.TRFUser.Operation.Insert)
            {
                TRFEventHelpers.SendTRFUserInsertRequestedEvent(this._guid, this._csstSession.trfUser);
            }
            else if(this._csstSession.trfUser.trfUserOperation == TRFGlobalVariables.TRFUser.Operation.Update)
            {
                TRFEventHelpers.SendTRFUserUpdateRequestedEvent(this._guid, this._csstSession.trfUser);
                CSSTEventHelpers.SendCSSTSaveSessionRequestedEvent(this._guid, this._csstSession);
            }
        }

        private void TRFSaveMotionDataFinished(TRFEventArg e)
        {
            this._isMotionDataSaved = true;
            if (this._isCSSTSessionSaved == true && this._isTRFUserSaved == true)
            {
                this.CSSTDataSaved();
            }
        }
        #endregion

        private void ChangeCSSTStage(CSSTStages stage)
        {
            this._csstStage.key = this._csstStage.value;
            this._csstStage.value = stage;
            this._csstServiceChannelManager.NotifyCSSTStage(this._loginTrfUser.id, this._csstStage);
        }

        #region Count Down Thread
        private void CountDownThread(int duration, Action<int> beginningAction, Action<int> runningAction, Action<int> enddingAction, int interval = 1000)
        {
            ArrayList parameters = new ArrayList { duration, interval, beginningAction, runningAction, enddingAction };
            (new Thread(this.CountDownThreadFunction)).Start(parameters);
        }

        private void CountDownThreadFunction(object parameters)
        {
            ArrayList param = (ArrayList)parameters;
            int duration = (int)param[0];
            int interval = (int)param[1];
            Action<int> beginningAction = (Action<int>)param[2];
            Action<int> runningAction = (Action<int>)param[3];
            Action<int> enddingAction = (Action<int>)param[4];

            beginningAction(duration);
            Thread.Sleep(interval);
            int i = duration - 1;
            while (i > 0)
            {
                runningAction(i);
                i--;
                Thread.Sleep(interval);
                if (this._isCountDownTerminated)
                    return;
            }
            enddingAction(duration);
        }

        #region CSST Calibration
        private void CSSTCalibrationBeginningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage("Please stand straight in front of the chair for " + time + " seconds", DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTCalibrationRunningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage(time.ToString(), DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTCalibrationEnddingAction(int time)
        {
            this.ChangeCSSTStage(CSSTStages.CSSTCalibrated);
            this._motionDatPurpose = CSSTGlobalVariables.MotionDatPurpose.Preparation;
            this.CountDownThread(3, this.CSSTPreparationBeginningAction, this.CSSTPreparationRunningAction, this.CSSTPreparationEnddingAction);
        }
        #endregion

        #region Preparation
        private void CSSTPreparationBeginningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage("Please sit straight in the chair for " + time + " seconds", DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTPreparationRunningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage(time.ToString(), DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTPreparationEnddingAction(int time)
        {
            this.ChangeCSSTStage(CSSTStages.CSSTStarted);
            this._motionDatPurpose = CSSTGlobalVariables.MotionDatPurpose.Assessment;
            this.CountDownThread(10, this.CSSTAssessmentBeginningAction, this.CSSTAssessmentRunningAction, this.CSSTAssessmentEnddingAction);
        }
        #endregion

        #region Assessment
        private void CSSTAssessmentBeginningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage("Please sit straight in the chair for " + time + " seconds", DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTAssessmentRunningAction(int time)
        {
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage(time.ToString(), DateTime.Now, TRFGlobalVariables.Message.Presentation.Hybrid));
        }

        private void CSSTAssessmentEnddingAction(int time)
        {
            this.ChangeCSSTStage(CSSTStages.CSSTStopped);
            this._isCountDownTerminated = false;
            this._csstSession.endDateTime = DateTime.Now;

            #region Get testing data

            //Matrix<double> bodyMatrix = MatlabReader.Read<double>("ServerUserModules\\T1.mat");
            //List<TRFBody3D> bodies = new List<TRFBody3D>();
            //for (int i = 0; i < bodyMatrix.RowCount; i++)
            //{
            //    TRFBody3D body = new TRFBody3D();
            //    body._timestamp = DateTime.Now;
            //    body._spineBaseX = bodyMatrix[i, 0];
            //    body._spineBaseY = bodyMatrix[i, 1];
            //    body._spineBaseZ = bodyMatrix[i, 2];
            //    body._spineMidX = bodyMatrix[i, 3];
            //    body._spineMidY = bodyMatrix[i, 4];
            //    body._spineMidZ = bodyMatrix[i, 5];
            //    body._spineShoulderX = bodyMatrix[i, 6];
            //    body._spineShoulderY = bodyMatrix[i, 7];
            //    body._spineShoulderZ = bodyMatrix[i, 8];
            //    body._neckX = bodyMatrix[i, 9];
            //    body._neckY = bodyMatrix[i, 10];
            //    body._neckZ = bodyMatrix[i, 11];
            //    body._headX = bodyMatrix[i, 12];
            //    body._headY = bodyMatrix[i, 13];
            //    body._headZ = bodyMatrix[i, 14];
            //    body._shoulderLeftX = bodyMatrix[i, 15];
            //    body._shoulderLeftY = bodyMatrix[i, 16];
            //    body._shoulderLeftZ = bodyMatrix[i, 17];
            //    body._elbowLeftX = bodyMatrix[i, 18];
            //    body._elbowLeftY = bodyMatrix[i, 19];
            //    body._elbowLeftZ = bodyMatrix[i, 20];
            //    body._wristLeftX = bodyMatrix[i, 21];
            //    body._wristLeftY = bodyMatrix[i, 22];
            //    body._wristLeftZ = bodyMatrix[i, 23];
            //    body._handLeftX = bodyMatrix[i, 24];
            //    body._handLeftY = bodyMatrix[i, 25];
            //    body._handLeftZ = bodyMatrix[i, 26];
            //    body._handTipLeftX = bodyMatrix[i, 27];
            //    body._handTipLeftY = bodyMatrix[i, 28];
            //    body._handTipLeftZ = bodyMatrix[i, 29];
            //    body._thumbLeftX = bodyMatrix[i, 30];
            //    body._thumbLeftY = bodyMatrix[i, 31];
            //    body._thumbLeftZ = bodyMatrix[i, 32];
            //    body._shoulderRightX = bodyMatrix[i, 33];
            //    body._shoulderRightY = bodyMatrix[i, 34];
            //    body._shoulderRightZ = bodyMatrix[i, 35];
            //    body._elbowRightX = bodyMatrix[i, 36];
            //    body._elbowRightY = bodyMatrix[i, 37];
            //    body._elbowRightZ = bodyMatrix[i, 38];
            //    body._wristRightX = bodyMatrix[i, 39];
            //    body._wristRightY = bodyMatrix[i, 40];
            //    body._wristRightZ = bodyMatrix[i, 41];
            //    body._handRightX = bodyMatrix[i, 42];
            //    body._handRightY = bodyMatrix[i, 43];
            //    body._handRightZ = bodyMatrix[i, 44];
            //    body._handTipRightX = bodyMatrix[i, 45];
            //    body._handTipRightY = bodyMatrix[i, 46];
            //    body._handTipRightZ = bodyMatrix[i, 47];
            //    body._thumbRightX = bodyMatrix[i, 48];
            //    body._thumbRightY = bodyMatrix[i, 49];
            //    body._thumbRightZ = bodyMatrix[i, 50];
            //    body._hipLeftX = bodyMatrix[i, 51];
            //    body._hipLeftY = bodyMatrix[i, 52];
            //    body._hipLeftZ = bodyMatrix[i, 53];
            //    body._kneeLeftX = bodyMatrix[i, 54];
            //    body._kneeLeftY = bodyMatrix[i, 55];
            //    body._kneeLeftZ = bodyMatrix[i, 56];
            //    body._ankleLeftX = bodyMatrix[i, 57];
            //    body._ankleLeftY = bodyMatrix[i, 58];
            //    body._ankleLeftZ = bodyMatrix[i, 59];
            //    body._footLeftX = bodyMatrix[i, 60];
            //    body._footLeftY = bodyMatrix[i, 61];
            //    body._footLeftZ = bodyMatrix[i, 62];
            //    body._hipRightX = bodyMatrix[i, 63];
            //    body._hipRightY = bodyMatrix[i, 64];
            //    body._hipRightZ = bodyMatrix[i, 65];
            //    body._kneeRightX = bodyMatrix[i, 66];
            //    body._kneeRightY = bodyMatrix[i, 67];
            //    body._kneeRightZ = bodyMatrix[i, 68];
            //    body._ankleRightX = bodyMatrix[i, 69];
            //    body._ankleRightY = bodyMatrix[i, 70];
            //    body._ankleRightZ = bodyMatrix[i, 71];
            //    body._footRightX = bodyMatrix[i, 72];
            //    body._footRightY = bodyMatrix[i, 73];
            //    body._footRightZ = bodyMatrix[i, 74];
            //    body._purpose = (sbyte)bodyMatrix[i, 75];
            //    body.AssembleMyBodyObject();
            //    bodies.Add(body);
            //}
            //this._csstSession.trfUser.motion3D = bodies;
            #endregion


            CSSTEventHelpers.SendCSSTDataAnalysisRequestedEvent(this._guid, this._csstSession.trfUser);
        }
        #endregion
        #endregion

        public void CSSTDataSaved()
        {
            this.ChangeCSSTStage(CSSTStages.CSSTFinished);
            this._csstServiceChannelManager.NotifyTRFMessage(this._loginTrfUser.id, new TRFMessage("CSST data has been saved. You can start a new session."));
        }
    }
}
