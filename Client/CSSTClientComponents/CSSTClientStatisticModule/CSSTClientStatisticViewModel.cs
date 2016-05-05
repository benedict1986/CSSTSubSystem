using System;
using CSSTCommonLibs;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using TRFCommonLibs;
using System.Runtime.InteropServices;

namespace CSSTClientStatisticModule
{
    [Guid("463297CB-3F65-4BEA-9BA7-5F973F00504C")]
    public class CSSTClientStatisticViewModel : TRFNotifyPropertyChanged
    {
        private readonly Guid _guid = typeof (CSSTClientStatisticViewModel).GUID;
        private readonly CSSTClientStatisticModel _csstClientStatisticModel;

        public string cycleNumber
        {
            get { return this._csstClientStatisticModel.cycleNumber; }
            set
            {
                if (value == null || this._csstClientStatisticModel.cycleNumber == value) return;
                this._csstClientStatisticModel.cycleNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        public string upTimeMeanStd
        {
            get { return this._csstClientStatisticModel.upTimeMeanStd + " Second"; }
            set
            {
                if (value == null || this._csstClientStatisticModel.upTimeMeanStd == value) return;
                this._csstClientStatisticModel.upTimeMeanStd = value;
                this.NotifyPropertyChanged();
            }
        }
        public string downTimeMeanStd
        {
            get { return this._csstClientStatisticModel.downTimeMeanStd + " Second"; }
            set
            {
                if (value == null || this._csstClientStatisticModel.downTimeMeanStd == value) return;
                this._csstClientStatisticModel.downTimeMeanStd = value;
                this.NotifyPropertyChanged();
            }
        }
        public string bodySwingMeanStd
        {
            get { return this._csstClientStatisticModel.bodySwingMeanStd + " Meter"; }
            set
            {
                if (value == null || this._csstClientStatisticModel.bodySwingMeanStd == value) return;
                this._csstClientStatisticModel.bodySwingMeanStd = value;
                this.NotifyPropertyChanged();
            }
        }
        public string armSwingMeanStd
        {
            get { return this._csstClientStatisticModel.armSwingMeanStd + " Meter"; }
            set
            {
                if (value == null || this._csstClientStatisticModel.armSwingMeanStd == value) return;
                this._csstClientStatisticModel.armSwingMeanStd = value;
                this.NotifyPropertyChanged();
            }
        }
        public string severityLevelLabel
        {
            get { return this._csstClientStatisticModel.severityLevelLabel; }
            set
            {
                if (value == null || this._csstClientStatisticModel.severityLevelLabel == value) return;
                this._csstClientStatisticModel.severityLevelLabel = value;
                this.NotifyPropertyChanged();
            }
        }
        public double severityLevelProgressBarValue
        {
            get { return this._csstClientStatisticModel.severityLevelProgressBarValue; }
            set
            {
                if (this._csstClientStatisticModel.severityLevelProgressBarValue.Equals(value)) return;
                this._csstClientStatisticModel.severityLevelProgressBarValue = value;
                this.NotifyPropertyChanged();
            }
        }
        public string healthyCycleNumber
        {
            get { return this._csstClientStatisticModel.healthyCycleNumber; }
            set
            {
                this._csstClientStatisticModel.healthyCycleNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        public string healthyUpTime {
            get { return this._csstClientStatisticModel.healthyUpTime; }
            set
            {
                this._csstClientStatisticModel.healthyUpTime = value;
                this.NotifyPropertyChanged();
            }
        }
        public string healthyDownTime {
            get { return this._csstClientStatisticModel.healthyDownTime; }
            set
            {
                this._csstClientStatisticModel.healthyDownTime = value;
                this.NotifyPropertyChanged();
            }
        }
        public string healthyBodySwing {
            get { return this._csstClientStatisticModel.healthyBodySwing; }
            set
            {
                this._csstClientStatisticModel.healthyBodySwing = value;
                this.NotifyPropertyChanged();
            }
        }
        public string healthyArmSwing {
            get { return this._csstClientStatisticModel.healthyArmSwing; }
            set
            {
                this._csstClientStatisticModel.healthyArmSwing = value;
                this.NotifyPropertyChanged();
            }
        }

        private readonly SubscriptionToken _csstDataAnalysisResultReadyEventST;
        public CSSTClientStatisticViewModel()
        {
            this._csstClientStatisticModel = new CSSTClientStatisticModel();
            this._csstDataAnalysisResultReadyEventST = CSSTEventHelpers.ReceiveCSSTDataAnalysisResultReadyEvent(this.CSSTDataAnalysisResultReady,
                arg => arg.receivers.Contains(this._guid));
        }

        ~CSSTClientStatisticViewModel()
        {
            CSSTEventHelpers.DisposeCSSTDataAnalysisResultReadyEvent(this._csstDataAnalysisResultReadyEventST);
        }

        #region Event Handlers

        private void CSSTDataAnalysisResultReady(TRFEventArg e)
        {
            CSSTAnalysisResult result = (CSSTAnalysisResult)e.payload;
            this.cycleNumber = result.cycleNumber.ToString();
            this.upTimeMeanStd = Math.Round(result.upTimeMean, 3) + "/" + Math.Round(result.upTimeStd, 3);
            this.downTimeMeanStd = Math.Round(result.downTimeMean, 3) + "/" + Math.Round(result.downTimeStd, 3);
            this.bodySwingMeanStd = Math.Round(result.bodySwingMean, 3) + "/" + Math.Round(result.bodySwingrStd, 3);
            this.armSwingMeanStd = Math.Round(result.armSwingMean, 3) + "/" + Math.Round(result.armSwingStd, 3);
            this.healthyCycleNumber = "4 ~ 6";
            this.healthyUpTime = result.healthyUpTimeBoundary.Item1 + " ~ " + result.healthyUpTimeBoundary.Item2;
            this.healthyDownTime = result.healthyDownTimeBoundary.Item1 + " ~ " + result.healthyDownTimeBoundary.Item2;
            this.healthyBodySwing = result.healthyBodySwingBoundary.Item1 + " ~ " + result.healthyBodySwingBoundary.Item2;
            this.healthyArmSwing = result.healthyArmSwingBoundary.Item1 + " ~ " + result.healthyArmSwingBoundary.Item2;
        }
        #endregion
    }
}
