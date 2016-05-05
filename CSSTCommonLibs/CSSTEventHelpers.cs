using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using TRFCommonLibs;

namespace CSSTCommonLibs
{
    public class CSSTUserLoginRequestedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTUserLogoutRequestedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTClientLoadedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTSaveSessionRequestedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTSessionInsertConfirmedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTStageChangedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTDataAnalysisRequestedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTDataAnalysisResultReadyEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTDataSavedEvent : PubSubEvent<TRFEventArg> { }
    public class CSSTTerminationRequestedEvent : PubSubEvent<TRFEventArg> { }

    public class CSSTEventHelpers
    {
        private static readonly IEventAggregator _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        #region CSSTUserLoginRequestedEven
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to CSSTClientService</param>
        public static void SendCSSTUserLoginRequestedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {

            CSSTEventHelpers._eventAggregator.GetEvent<CSSTUserLoginRequestedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = receivers ?? new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTClientService"] },
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTUserLoginRequestedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTUserLoginRequestedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTUserLoginRequestedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTUserLoginRequestedEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTSaveSessionRequestedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to CSSTClientService, CSSTDatabaseController</param>
        public static void SendCSSTSaveSessionRequestedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTSaveSessionRequestedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = receivers ?? new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTClientService"], CSSTGlobalVariables.Event.Receiver["CSSTDatabaseController"] },
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTSaveSessionRequestedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTSaveSessionRequestedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTSaveSessionRequestedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTSaveSessionRequestedEvent>().Unsubscribe(token);
        }
        #endregion

        #region SendCSSTSessionInsertConfirmedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to CSSTClientService, CSSTDatabaseController</param>
        public static void SendCSSTSessionInsertConfirmedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {

            CSSTEventHelpers._eventAggregator.GetEvent<CSSTSessionInsertConfirmedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = receivers ?? new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTServerService"] },
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTSessionInsertConfirmedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTSessionInsertConfirmedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTSessionInsertConfirmedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTSessionInsertConfirmedEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTStageChangedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to everywhere</param>
        public static void SendCSSTStageChangedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTStageChangedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = receivers ?? new List<Guid>() { },
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTStageChangedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTStageChangedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTStageChangedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTStageChangedEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTDataAnalysisRequestedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to CSSTDataAnalysis</param>
        public static void SendCSSTDataAnalysisRequestedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisRequestedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = receivers ?? new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTDataAnalysis"] },
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTDataAnalysisRequestedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisRequestedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTDataAnalysisRequestedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisRequestedEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTDataAnalysisResultReadyEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to CSSTServerService</param>
        public static void SendCSSTDataAnalysisResultReadyEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            List<Guid> tempReveicers = new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTServerService"] };
            if (receivers != null)
                tempReveicers.AddRange(receivers);
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisResultReadyEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = tempReveicers,
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTDataAnalysisResultReadyEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisResultReadyEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTDataAnalysisResultReadyEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataAnalysisResultReadyEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTDataSavedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to everywhere</param>
        public static void SendCSSTDataSavedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            List<Guid> tempReveicers = new List<Guid>() { };
            if (receivers != null)
                tempReveicers.AddRange(receivers);
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataSavedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = tempReveicers,
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTDataSavedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataSavedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTDataSavedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTDataSavedEvent>().Unsubscribe(token);
        }
        #endregion

        #region CSSTTerminationRequestedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <param name="receivers">Default send to everywhere</param>
        public static void SendCSSTTerminationRequestedEvent(Guid sender, object payload, List<Guid> receivers = null)
        {
            List<Guid> tempReveicers = new List<Guid>() { CSSTGlobalVariables.Event.Receiver["CSSTClientService"] };
            if (receivers != null)
                tempReveicers.AddRange(receivers);
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTTerminationRequestedEvent>()
                    .Publish(new TRFEventArg
                    {
                        payload = payload,
                        receivers = tempReveicers,
                        sender = sender
                    });
        }
        public static SubscriptionToken ReceiveCSSTTerminationRequestedEvent(Action<TRFEventArg> action, Predicate<TRFEventArg> filter = null, ThreadOption thread = ThreadOption.BackgroundThread, bool keepSubscriberReferenceAlive = true)
        {
            return CSSTEventHelpers._eventAggregator.GetEvent<CSSTTerminationRequestedEvent>()
                .Subscribe(action, thread, keepSubscriberReferenceAlive, filter);
        }
        public static void DisposeCSSTTerminationRequestedEvent(SubscriptionToken token)
        {
            CSSTEventHelpers._eventAggregator.GetEvent<CSSTTerminationRequestedEvent>().Unsubscribe(token);
        }
        #endregion
    }
}
