using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using PX.Data;

namespace PX.Objects.FS
{
    public class WFStageMaint : PXGraph<WFStageMaint>
    {
        private const int RootNodeID = 0;
        private FSWFStage fsWFStageRow_Current;

        #region DACFilter
        [Serializable]
        public class WFStageFilter : IBqlTable
        {
            #region WFID
            public abstract class wFID : PX.Data.BQL.BqlInt.Field<wFID> { }

            [PXDBInt(IsKey = true)]
            [PXUIField(DisplayName = "Service Order Type")]
            [FSSelectorWorkflow]
            public virtual int? WFID { get; set; }
            #endregion
            #region Descr
            public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

            [PXDBString(60, IsUnicode = true)]
            [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, IsReadOnly = true)]
            public virtual string Descr { get; set; }
            #endregion
        }
        #endregion
        #region NodeSelection
        [Serializable]
        [DebuggerDisplay("WFID={WFID} ParentWFStageID={ParentWFStageID} WFStageID={WFStageID}")]
        public partial class SelectedNode : IBqlTable
        {
            #region WFID
            public abstract class wFID : PX.Data.BQL.BqlInt.Field<wFID> { }

            [PXInt]
            public virtual int? WFID { get; set; }
            #endregion
            #region ParentWFStageID
            public abstract class parentWFStageID : PX.Data.BQL.BqlInt.Field<parentWFStageID> { }

            [PXInt]
            public virtual int? ParentWFStageID { get; set; }
            #endregion
            #region WFStageID

            public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

            [PXInt]
            public virtual int? WFStageID { get; set; }
            #endregion 
        }
        #endregion
        #region ActionButtons

        public PXCancel<WFStageFilter> Cancel;
        public PXSave<WFStageFilter> Save;
        public PXInsert<WFStageFilter> Insert;
        public PXDelete<WFStageFilter> Delete;

        public PXAction<WFStageFilter> up;
        public PXAction<WFStageFilter> down;

        #endregion
        #region Selects

        public PXFilter<WFStageFilter> Filter;
        public PXFilter<SelectedNode> NodeFilter;

        public PXSelect<FSWFStage, 
                        Where<
                            FSWFStage.wFID, Equal<Current<SelectedNode.wFID>>>> 
                        CurrentItem;

        public PXSelect<FSWFStage,
                        Where<
                            FSWFStage.parentWFStageID, Equal<Argument<int?>>>, 
                            OrderBy<Asc<FSWFStage.sortOrder>>> 
                        Nodes;

        public PXSelect<FSWFStage, 
                        Where<
                            FSWFStage.parentWFStageID, Equal<Argument<int?>>>, 
                            OrderBy<Asc<FSWFStage.sortOrder>>> 
                        Items;

        #endregion
        #region TreeDelegates
        protected virtual IEnumerable nodes([PXInt] int? parent)
        {
            List<FSWFStage> list = new List<FSWFStage>();

            if (!parent.HasValue)
            {
                FSWFStage fsWFStageRow = new FSWFStage();
                fsWFStageRow.WFStageID = RootNodeID;
                fsWFStageRow.WFID = NodeFilter.Current.WFID;

                FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType, 
                                               Where<
                                                     FSSrvOrdType.srvOrdTypeID, Equal<Required<FSSrvOrdType.srvOrdTypeID>>>>
                                               .Select(this, fsWFStageRow.WFID);
                if (fsSrvOrdTypeRow != null)
                {
                    fsWFStageRow.WFStageCD = fsSrvOrdTypeRow.SrvOrdType;
                    list.Add(fsWFStageRow);
                }
            }
            else
            {
                PXResultset<FSWFStage> bqlResultSet;
                
                bqlResultSet = PXSelect<FSWFStage, 
                               Where<
                                    FSWFStage.wFID, Equal<Current<SelectedNode.wFID>>,
                                    And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>>
                               .Select(this, parent);
                
                foreach (FSWFStage fsWFStageRow in bqlResultSet)
                {
                    list.Add(fsWFStageRow);
                }
            }

            return list;
        }

        protected virtual IEnumerable items([PXInt] int? parent)
        {
            NodeFilter.Current.ParentWFStageID = (parent == null) ? RootNodeID : parent;

            PXResultset<FSWFStage> bqlResultSet;

            if (parent == null || parent == RootNodeID)
            {
                bqlResultSet  = PXSelect<FSWFStage,
                                Where<
                                    FSWFStage.wFID, Equal<Current<SelectedNode.wFID>>,
                                    And<FSWFStage.parentWFStageID, Equal<Current<SelectedNode.parentWFStageID>>>>>
                                .Select(this);
            }
            else
            {
                bqlResultSet = PXSelect<FSWFStage,
                               Where<
                                    FSWFStage.wFID, Equal<Current<SelectedNode.wFID>>,
                                    And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>>
                               .Select(this, parent);
            }
            
            return bqlResultSet;
        }
        #endregion
        #region Tree Selector Entity

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
        public virtual IEnumerable Up(PXAdapter adapter)
        {
            IList<FSWFStage> routes = GetSortedItems();

            int? selectedItem = CurrentItem.Current.WFStageID;
            int currentItemIndex = 0;
            for (int i = 0; i < routes.Count; i++)
            {
                if (routes[i].WFStageID == selectedItem)
                {
                    currentItemIndex = i;
                }

                routes[i].SortOrder = i + 1;
                Items.Update(routes[i]);
            }

            if (currentItemIndex > 0)
            {
                routes[currentItemIndex].SortOrder--;
                routes[currentItemIndex - 1].SortOrder++;

                Items.Update(routes[currentItemIndex]);
                Items.Update(routes[currentItemIndex - 1]);
            }

            return adapter.Get();
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
        public virtual IEnumerable Down(PXAdapter adapter)
        {
            IList<FSWFStage> routes = GetSortedItems();

            int? selectedItem = CurrentItem.Current.WFStageID;
            int currentItemIndex = 0;
            for (int i = 0; i < routes.Count; i++)
            {
                if (routes[i].WFStageID == selectedItem)
                {
                    currentItemIndex = i;
                }

                routes[i].SortOrder = i + 1;
                Items.Update(routes[i]);
            }

            if (currentItemIndex < routes.Count - 1)
            {
                routes[currentItemIndex].SortOrder++;
                routes[currentItemIndex + 1].SortOrder--;

                Items.Update(routes[currentItemIndex]);
                Items.Update(routes[currentItemIndex + 1]);
            }

            return adapter.Get();
        }
        #endregion
        #region EventsHandlers_FSWFStage

        protected virtual void FSWFStage_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            FSWFStage fsWFStageRow_New = (FSWFStage)e.NewRow;
            FSWFStage fsWFStageRow_Old = (FSWFStage)e.Row;
            if (fsWFStageRow_New.SortOrder != fsWFStageRow_Old.SortOrder && e.ExternalCall)
            {
                if (fsWFStageRow_Old.SortOrder < fsWFStageRow_New.SortOrder)
                {
                    UpdateSequence(sender, fsWFStageRow_New, fsWFStageRow_Old.SortOrder + 1, fsWFStageRow_New.SortOrder, -1);
                }
                else
                {
                    UpdateSequence(sender, fsWFStageRow_New, fsWFStageRow_New.SortOrder, fsWFStageRow_Old.SortOrder, +1);
                }

                this.Filter.View.RequestRefresh();
            }
        }

        private void UpdateSequence(PXCache sender, FSWFStage fsWFStageRow_Route, int? from, int? to, int step)
        {
            foreach (FSWFStage fsWFStageRow 
                                in PXSelect<FSWFStage,
                                   Where<
                                        FSWFStage.wFID, Equal<Required<FSWFStage.wFID>>, 
                                        And<FSWFStage.wFStageID, NotEqual<Required<FSWFStage.wFStageID>>,
                                        And<FSWFStage.sortOrder, Between<Required<FSWFStage.sortOrder>, Required<FSWFStage.sortOrder>>>>>>
                                   .Select(this, fsWFStageRow_Route.WFID, fsWFStageRow_Route.WFStageID, from, to))
            {
                fsWFStageRow.SortOrder += step;
            }            
        }

        protected virtual void FSWFStage_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            FSWFStage fsWFStageRow = e.Row as FSWFStage;
            if (fsWFStageRow != null && Filter.Current != null)
            {
                IEnumerable list = items(NodeFilter.Current.ParentWFStageID);

                int maxSequence = 0;
                foreach (PXResult<FSWFStage> bqlResult in list)
                {
                    FSWFStage fsWFStageRow_Item = (FSWFStage)bqlResult;

                    if (fsWFStageRow_Item.SortOrder.Value > maxSequence)
                    {
                        maxSequence = fsWFStageRow_Item.SortOrder.Value;
                    }
                }

                fsWFStageRow.SortOrder = maxSequence + 1;
                fsWFStageRow.ParentWFStageID = NodeFilter.Current.ParentWFStageID;
                fsWFStageRow.WFStageCD = fsWFStageRow.WFStageCD.Trim();
                fsWFStageRow.WFID = Filter.Current.WFID;

                string strErrMess = ValidateItemName(fsWFStageRow.WFStageCD, fsWFStageRow.ParentWFStageID);
                if (strErrMess != string.Empty)
                {
                    sender.RaiseExceptionHandling<FSWFStage.wFStageCD>(e.Row, fsWFStageRow.WFStageCD, new PXSetPropertyException(strErrMess, PXErrorLevel.Error));
                    e.Cancel = true;
                }
            }
        }

        protected virtual void FSWFStage_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            FSWFStage fsWFStageRow = e.Row as FSWFStage;
            if (fsWFStageRow != null && Filter.Current != null)
            {
                NodeFilter.Current.ParentWFStageID = fsWFStageRow.ParentWFStageID;
            }
        }

        protected virtual void FSWFStage_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            FSWFStage fsWFStageRow = e.Row as FSWFStage;
            if (fsWFStageRow == null) 
            {
                return;
            }

            if (e.Operation == PXDBOperation.Insert &&
                    NodeFilter.Current.WFID == fsWFStageRow.WFID &&
                        (NodeFilter.Current.ParentWFStageID == fsWFStageRow.WFStageID 
                            || NodeFilter.Current.ParentWFStageID == null))
            {
                fsWFStageRow_Current = fsWFStageRow;
            }
        }

        protected virtual void FSWFStage_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            FSWFStage fsWFStageRow = (FSWFStage)e.Row;
            if (fsWFStageRow == fsWFStageRow_Current && e.TranStatus == PXTranStatus.Completed)
            {                
                NodeFilter.Current.WFID = fsWFStageRow.WFID;
                NodeFilter.Current.WFStageID = fsWFStageRow.WFStageID;
            }
        }

        protected virtual void FSWFStage_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            FSWFStage fsWFStageRow = e.Row as FSWFStage;
            if (fsWFStageRow == null || fsWFStageRow.WFStageID == null)
            {
                return;
            } 

            deleteRecurring(fsWFStageRow);
        }

        private void deleteRecurring(FSWFStage fsWFStageRow)
        {
            if (fsWFStageRow != null)
            {
                foreach (FSWFStage child in PXSelect<FSWFStage,
                                            Where<
                                                FSWFStage.wFID, Equal<Current<WFStageFilter.wFID>>,
                                                And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.wFStageID>>>>>
                                            .Select(this, fsWFStageRow.WFStageID))
                {
                    deleteRecurring(child);
                }

                Items.Cache.Delete(fsWFStageRow);
            }
        }

        #endregion
        #region Validations
        private string ValidateItemName(string name, int? parentSSID)
        {
            FSWFStage fsWFStageRow = PXSelect<FSWFStage, 
                                     Where<
                                        FSWFStage.wFID, Equal<Current<WFStageFilter.wFID>>,
                                        And<FSWFStage.wFStageCD, Equal<Required<FSWFStage.wFStageCD>>,
                                        And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>>>
                                     .Select(this, name, parentSSID);
            if (fsWFStageRow != null)
            {
                return TX.Error.ID_ALREADY_USED_SAME_LEVEL;
            }
            
            FSWFStage fsWFStageRow_Parent = PXSelect<FSWFStage, 
                                            Where<
                                                FSWFStage.wFID, Equal<Current<WFStageFilter.wFID>>,
                                                And<FSWFStage.wFStageCD, Equal<Required<FSWFStage.wFStageCD>>,
                                                And<FSWFStage.wFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>>>
                                            .Select(this, name, parentSSID);
            if (fsWFStageRow_Parent != null)
            {
                return TX.Error.ID_ALREADY_USED_PARENT;
            }

            return string.Empty;
        }
        #endregion
        #region EventsHandlers_Filter
        protected virtual void WFStageFilter_WFID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            WFStageFilter row = e.Row as WFStageFilter;
            if (row != null)
            {
                NodeFilter.Current.WFID = row.WFID;
            }
        }

        protected virtual void WFStageFilter_WFID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue != null)
            {
                FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType, 
                                     Where<
                                         FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                                     .Select(this, e.NewValue);
                if (fsSrvOrdTypeRow != null)
                {
                    Filter.Current.Descr = fsSrvOrdTypeRow.Descr;
                }
            }
        }

        protected virtual void WFStageFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            WFStageFilter wfStageFilterRow = (WFStageFilter)e.Row;
            Items.AllowInsert = wfStageFilterRow.WFID != null;
        }

        #endregion
        #region Utils

        private static int Comparison(FSWFStage fsWFStageRow_X, FSWFStage fsWFStageRow_Y)
        {
            return fsWFStageRow_X.SortOrder.Value.CompareTo(fsWFStageRow_Y.SortOrder.Value);
        }

        private IList<FSWFStage> GetSortedItems()
        {
            List<FSWFStage> routes = new List<FSWFStage>();

            PXResultset<FSWFStage> bqlResultSet = this.Items.Select(NodeFilter.Current.ParentWFStageID);

            foreach (FSWFStage fsWFStageRow in bqlResultSet)
            {
                routes.Add(fsWFStageRow);                
            }

            routes.Sort(Comparison); 
            return routes;
        }

        #endregion
    }
}