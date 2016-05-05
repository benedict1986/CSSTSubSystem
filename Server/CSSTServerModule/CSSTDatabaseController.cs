using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSSTCommonLibs;
using Prism.Events;
using TRFCommonLibs;

namespace CSSTServerModule
{
    [Guid("80E0CAC7-D14B-4A84-983B-6DBDDF87EA6E")]
    public class CSSTDatabaseController
    {
        private readonly Guid _guid = typeof(CSSTDatabaseController).GUID;
        private readonly SubscriptionToken _csstSessionReadyEventST;

        public CSSTDatabaseController()
        {
            this._csstSessionReadyEventST = CSSTEventHelpers.ReceiveCSSTSaveSessionRequestedEvent(this.CSSTSessionReady,
                arg => arg.receivers.Contains(this._guid));
        }

        ~CSSTDatabaseController()
        {
            CSSTEventHelpers.DisposeCSSTSaveSessionRequestedEvent(this._csstSessionReadyEventST);
        }

        #region Event Handlers

        private void CSSTSessionReady(TRFEventArg e)
        {
            TRFDatabaseAdapter da = new TRFDatabaseAdapter(CSSTGlobalVariables.CSSTDatabase.DatabaseName.CSSTMain);
            int id;
            bool insertResult = da.InsertDatabaseTable(new List<object>() {e.payload}, out id, CSSTGlobalVariables.CSSTDatabase.DataTableName.CSSTSession, CSSTGlobalVariables.CSSTDatabase.DatabaseName.CSSTMain);
            CSSTEventHelpers.SendCSSTSessionInsertConfirmedEvent(this._guid, insertResult);
        }
        #endregion
    }
}
