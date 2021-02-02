using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public static class TreeWFStageHelper
    {
        public class TreeWFStageView : PXSelectOrderBy<FSWFStage, OrderBy<Asc<FSWFStage.sortOrder>>>
        {
            public TreeWFStageView(PXGraph graph) : base(graph)
            {
            }

            public TreeWFStageView(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public static IEnumerable treeWFStages(PXGraph graph, string srvOrdType, int? wFStageID)
        {
            if (wFStageID == null)
            {
                wFStageID = 0;
            }

            PXResultset<FSWFStage> fsWFStageSet = PXSelectJoin<FSWFStage,
                                                  InnerJoin<FSSrvOrdType,
                                                  On<
                                                      FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                                                  Where<
                                                      FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>,
                                                      And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>,
                                                  OrderBy<
                                                      Asc<FSWFStage.sortOrder>>>
                                                  .Select(graph, srvOrdType, wFStageID);

            foreach (FSWFStage fsWFStageRow in fsWFStageSet)
            {
                yield return fsWFStageRow;
            }
        }

        private static void EnableDisableActions(PXCache cache, 
                                                 FSWFStage fsWFStageRow,
                                                 Dictionary<string, PXAction> headerActions)
        {
            PXAction completeAction, closeAction, cancelAction, reopenAction;

            headerActions.TryGetValue(typeof(FSWFStage.allowComplete).Name, out completeAction);
            headerActions.TryGetValue(typeof(FSWFStage.allowClose).Name, out closeAction);
            headerActions.TryGetValue(typeof(FSWFStage.allowCancel).Name, out cancelAction);
            headerActions.TryGetValue(typeof(FSWFStage.allowReopen).Name, out reopenAction);

            EnableDisableActionsByWorkflowStage(cache, fsWFStageRow, completeAction, cancelAction, closeAction, reopenAction);
        }

        public static void EnableDisableDocumentByWorkflowStage(FSWFStage fsWFStageRow, 
                                                                List<PXCache> caches, 
                                                                Dictionary<string, PXAction> headerActions,
                                                                List<PXAction> detailsActions,
                                                                List<PXView> views)
        {
            if (fsWFStageRow == null)
            {
                return;
            }

            bool allowUpdate = fsWFStageRow.AllowModify.Value;
            string wfsFieldName;

            foreach (PXCache cache in caches)
            {
                EnableDisableActions(cache, fsWFStageRow, headerActions);
                wfsFieldName = cache.GetItemType().IsAssignableFrom(typeof(FSAppointment)) ? typeof(FSAppointment.wFStageID).Name : typeof(FSServiceOrder.wFStageID).Name;

                if (allowUpdate == false)
                {
                    foreach (Type field in cache.BqlFields)
                    {
                        if (!cache.BqlKeys.Contains(field) && field.Name != wfsFieldName)
                        {
                            PXUIFieldAttribute.SetEnabled(cache, cache.Current, field.Name, allowUpdate);
                        }
                    }
                }
            }

            foreach (PXAction action in detailsActions)
            {
                action.SetEnabled(allowUpdate && action.GetEnabled());
            }

            foreach (PXView view in views)
            {
                view.AllowInsert = allowUpdate;
                view.AllowUpdate = allowUpdate;
                view.AllowDelete = allowUpdate;
            }
        }

        public static void EnableDisableActionsByWorkflowStage(PXCache cache,
                                                               FSWFStage fsWFStageRow,
                                                               PXAction actionComplete,
                                                               PXAction actionCancel,
                                                               PXAction actionClose,
                                                               PXAction actionReopen)
        {
            if (fsWFStageRow == null)
            {
                return;
            }

            if (fsWFStageRow.AllowComplete == false)
            {
                actionComplete?.SetEnabled(fsWFStageRow.AllowComplete.Value);
            }

            if (fsWFStageRow.AllowCancel == false)
            {
                actionCancel?.SetEnabled(fsWFStageRow.AllowCancel.Value);
            }

            if (fsWFStageRow.AllowClose == false)
            {
                actionClose?.SetEnabled(fsWFStageRow.AllowClose.Value);
            }

            if (fsWFStageRow.AllowReopen == false)
            {
                actionReopen?.SetEnabled(fsWFStageRow.AllowReopen.Value);
            }

            cache.AllowDelete &= (bool)fsWFStageRow.AllowDelete;
        }
    }
}
