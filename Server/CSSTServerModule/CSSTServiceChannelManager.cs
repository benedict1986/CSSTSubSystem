using System;
using System.Collections.Generic;
using System.Linq;
using CSSTCommonLibs;
using TRFCommonLibs;
using CSSTStages = CSSTCommonLibs.CSSTGlobalVariables.CSSTStages;
namespace CSSTServerModule
{
    public class CSSTServiceChannelManager
    {
        private readonly Dictionary<int, ICSSTServiceCallback> _csstServiceCallbackChannelList;
        private readonly object _csstServiceCallbackObj;

        public CSSTServiceChannelManager()
        {
            this._csstServiceCallbackChannelList = new Dictionary<int, ICSSTServiceCallback>();
            this._csstServiceCallbackObj = new object();
        }

        public void CSSTChannelRegister(int id, ICSSTServiceCallback csstServiceCallback)
        {
            lock (this._csstServiceCallbackObj)
            {
                string message;
                if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                {
                    message = "User " + id + " already exists";
                }
                else
                {
                    this._csstServiceCallbackChannelList.Add(id, csstServiceCallback);
                    message = "User " + id + " just registered.";
                }
            }
        }

        public void CSSTChannelUnRegister(int id)
        {
            lock (this._csstServiceCallbackObj)
            {
                string message;
                if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                {
                    this._csstServiceCallbackChannelList.Remove(id);
                    message = "User " + id + " has been removed.";
                }
                else
                {
                    message = "User " + id + " does not exist.";
                }
            }
        }

        public void NotifyCSSTSessionInsertConfirmation(int id, bool insertResult)
        {
            lock (this._csstServiceCallbackObj)
            {
                try
                {
                    lock (this._csstServiceCallbackObj)
                    {
                        if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                        {
                            this._csstServiceCallbackChannelList[id].NotifyCSSTSessionInsertConfirmation(insertResult);
                        }
                        else
                        {
                            TRFGlobalVariables.SystemLogger.Error("Cannot send csst session insert result to user " + id + ". Channel cannot be found");
                        }
                    }
                }
                catch (Exception e)
                {
                    TRFGlobalVariables.SystemLogger.Error(e.Message);
                }
            }
        }

        public void NotifyCSSTStage(int id, TRFKeyValuePair<CSSTStages, CSSTStages> stage)
        {
            lock (this._csstServiceCallbackObj)
            {
                try
                {
                    lock (this._csstServiceCallbackObj)
                    {
                        if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                        {
                            this._csstServiceCallbackChannelList[id].NotifyCSSTStage(stage);
                        }
                        else
                        {
                            TRFGlobalVariables.SystemLogger.Error("Cannot send csst stage to user " + id + ". Channel cannot be found");
                        }
                    }
                }
                catch (Exception e)
                {
                    TRFGlobalVariables.SystemLogger.Error(e.Message);
                }
            }
        }

        public void NotifyTRFMessage(int id, TRFMessage message)
        {
            lock (this._csstServiceCallbackObj)
            {
                try
                {
                    lock (this._csstServiceCallbackObj)
                    {
                        if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                        {
                            this._csstServiceCallbackChannelList[id].NotifyTRFMessage(message);
                        }
                        else
                        {
                            TRFGlobalVariables.SystemLogger.Error("Cannot send message to user " + id + ". Channel cannot be found");
                        }
                    }
                }
                catch (Exception e)
                {
                    TRFGlobalVariables.SystemLogger.Error(e.Message);
                }
            }
        }

        public void NotifyCSSTAnalysisResult(int id, CSSTAnalysisResult result)
        {
            lock (this._csstServiceCallbackObj)
            {
                try
                {
                    lock (this._csstServiceCallbackObj)
                    {
                        if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                        {
                            this._csstServiceCallbackChannelList[id].NotifyCSSTAnalysisResult(result);
                        }
                        else
                        {
                            TRFGlobalVariables.SystemLogger.Error("Cannot send result to user " + id + ". Channel cannot be found");
                        }
                    }
                }
                catch (Exception e)
                {
                    TRFGlobalVariables.SystemLogger.Error(e.Message);
                }
            }
        }

        public void NotifyCSSTDataSaved(int id, bool result)
        {
            lock (this._csstServiceCallbackObj)
            {
                try
                {
                    lock (this._csstServiceCallbackObj)
                    {
                        if (this._csstServiceCallbackChannelList.Keys.Contains(id))
                        {
                            this._csstServiceCallbackChannelList[id].NotifyCSSTDataSaved(result);
                        }
                        else
                        {
                            TRFGlobalVariables.SystemLogger.Error("Cannot send result to user " + id + ". Channel cannot be found");
                        }
                    }
                }
                catch (Exception e)
                {
                    TRFGlobalVariables.SystemLogger.Error(e.Message);
                }
            }
        }
    }
}
