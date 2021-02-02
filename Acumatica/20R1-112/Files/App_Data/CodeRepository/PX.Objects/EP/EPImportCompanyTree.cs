using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.SM;
using PX.Objects.EP;
using PX.Objects.CR;

namespace PX.TM
{
    [Serializable]
    public class CompanyTreeMaint : PXGraph<CompanyTreeMaint>
    {
        [Serializable]
        [PXHidden]
        public partial class SelectedNode : IBqlTable
        {
            #region WorkGroupID
            public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
            protected int? _WorkGroupID;
            [PXInt()]
            [PXUIField(Visible = false)]
            public virtual int? WorkGroupID
            {
                get
                {
                    return this._WorkGroupID;
                }
                set
                {
                    this._WorkGroupID = value;
                }
            }
            #endregion            
        }

        [Serializable]
        [PXHidden]
        public partial class SelectedParentNode : IBqlTable
        {
            #region WorkGroupID
            public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
            protected int? _WorkGroupID;
            [PXInt()]
            [PXUIField(DisplayName = "Move to")]
            public virtual int? WorkGroupID
            {
                get
                {
                    return this._WorkGroupID;
                }
                set
                {
                    this._WorkGroupID = value;
                }
            }
            #endregion
        }       

        public PXFilter<SelectedNode> SelectedFolders;
        public PXSelectOrderBy<EPCompanyTreeMaster, OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>> Folders;
        public PXSelect<EPCompanyTreeMaster, Where<EPCompanyTreeMaster.workGroupID, Equal<Current<EPCompanyTreeMaster.workGroupID>>>> CurrentWorkGroup;
        public PXSelectJoin<EPCompanyTreeMember,
            LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>,
            LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                And<EPEmployeePosition.isActive, Equal<True>>>>>,
            Where<EPCompanyTreeMember.workGroupID, Equal<Current<EPCompanyTreeMaster.workGroupID>>>,
            OrderBy<Asc<EPEmployee.acctCD>>>
        Members;

        public PXSelectOrderBy<EPCompanyTreeMaster, OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>> ParentFolders;
        public PXFilter<SelectedParentNode> SelectedParentFolders;

        [PXDBGuid(IsKey = true)]
        [PXSelector(typeof(Search2<Users.pKID, LeftJoin<EPEmployee, On<Users.pKID, Equal<EPEmployee.userID>>, LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>>),
            typeof(Users.username),
            typeof(EPEmployee.acctCD),
            typeof(EPEmployee.acctName),
            typeof(EPEmployeePosition.positionID),
            typeof(EPEmployee.departmentID),
            SubstituteKey = typeof(Users.username), CacheGlobal = true)]
        [PXRestrictor(typeof(Where<EPEmployee.acctCD, IsNotNull>), Objects.EP.Messages.UserWithoutEmployee, typeof(Users.username))]
        [PXUIField(DisplayName = "User", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]       
        protected virtual void EPCompanyTreeMember_UserID_CacheAttached(PXCache sender) { }


        #region Ctors

        public CompanyTreeMaint()
        {
            PXUIFieldAttribute.SetEnabled<EPEmployee.acctCD>(Caches[typeof(EPEmployee)], null, false);
            PXUIFieldAttribute.SetDisplayName<EPCompanyTree.parentWGID>(Caches[typeof(EPCompanyTree)], "Move to");
        }
        #endregion

        #region delegates
        protected virtual IEnumerable folders(
        [PXInt]
            int? workGroupID
    )
        {
            if (workGroupID == null)
            {
                yield return new EPCompanyTreeMaster()
                {
                    WorkGroupID = 0,
                    Description = PXSiteMap.RootNode.Title
                };
            }
            else
            {


                foreach (EPCompanyTreeMaster item in PXSelect<EPCompanyTreeMaster,
                    Where<EPCompanyTreeMaster.parentWGID,
                        Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                    OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>>.Select(this, workGroupID))
                {
                    if (!string.IsNullOrEmpty(item.Description))
                        yield return item;
                }
            }
        }

        protected virtual IEnumerable currentWorkGroup()
        {
            if (Folders.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<EPCompanyTreeMaster.description>(Caches[typeof(EPCompanyTreeMaster)], null, Folders.Current.ParentWGID != null);
                PXUIFieldAttribute.SetEnabled<EPCompanyTreeMaster.parentWGID>(Caches[typeof(EPCompanyTreeMaster)], null, Folders.Current.ParentWGID != null);
                Caches[typeof(EPCompanyTreeMaster)].AllowInsert = Folders.Current.ParentWGID != null;
                Caches[typeof(EPCompanyTreeMaster)].AllowDelete = Folders.Current.ParentWGID != null;
                Caches[typeof(EPCompanyTreeMaster)].AllowUpdate = Folders.Current.ParentWGID != null;

                this.Actions["MoveWorkGroup"].SetEnabled(Folders.Current.ParentWGID != null);
                this.Actions["Up"].SetEnabled(Folders.Current.ParentWGID != null);
                this.Actions["Down"].SetEnabled(Folders.Current.ParentWGID != null);
                this.Actions["DeleteWorkGroup"].SetEnabled(Folders.Current.ParentWGID != null);

                foreach (EPCompanyTreeMaster item in PXSelect<EPCompanyTreeMaster,
                Where<EPCompanyTreeMaster.workGroupID, Equal<Required<EPCompanyTreeMaster.workGroupID>>>>.
                Select(this, Folders.Current.WorkGroupID))
                {
                    yield return item;
                }
            }
        }

        protected virtual IEnumerable members()
        {
            Caches[typeof(EPCompanyTreeMember)].AllowInsert = Folders.Current.ParentWGID != null && Folders.Current != null;
            Caches[typeof(EPCompanyTreeMember)].AllowDelete = Folders.Current.ParentWGID != null;
            Caches[typeof(EPCompanyTreeMember)].AllowUpdate = Folders.Current.ParentWGID != null;

            if (Folders.Current != null)
            {
                foreach (var res in
                         PXSelectJoin<EPCompanyTreeMember,
                     LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>,
                     LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                         And<EPEmployeePosition.isActive, Equal<True>>>,
                     LeftJoin<Users, On<Users.pKID, Equal<EPCompanyTreeMember.userID>>>>>,
                     Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>>>.Select(this, Folders.Current.WorkGroupID))
                {
                    EPCompanyTreeMember _companyTreeMember = res[typeof(EPCompanyTreeMember)] as EPCompanyTreeMember;
                    EPEmployee _employeer = res[typeof(EPEmployee)] as EPEmployee;
                    Users _user = res[typeof(Users)] as Users;
                    EPEmployeePosition _ePEmployeeposision = res[typeof(EPEmployeePosition)] as EPEmployeePosition;

                    if (_companyTreeMember.UserID != null )
                    {
                        if (_user == null)
                        {
                            this.Caches[typeof(EPCompanyTreeMember)].RaiseExceptionHandling<EPCompanyTreeMember.userID>(_companyTreeMember, null, new PXSetPropertyException(PX.Objects.EP.Messages.UserDeleted, PXErrorLevel.Warning));
                        }
                        else if (_user.State != Users.state.Active && _user.State != Users.state.Online)
                        {
                            this.Caches[typeof(EPCompanyTreeMember)].RaiseExceptionHandling<EPCompanyTreeMember.userID>(_companyTreeMember, null, new PXSetPropertyException(PX.Objects.EP.Messages.UserIsInactive, PXErrorLevel.Warning));
                        }
                        if (_employeer == null)
                        {
                            this.Caches[typeof(EPEmployee)].RaiseExceptionHandling<EPEmployee.acctCD>(_employeer, null, new PXSetPropertyException(PX.Objects.EP.Messages.EmployeeDeleted, PXErrorLevel.Warning));
                        }

                        else if (_employeer.Status == null)
                        {
                            this.Caches[typeof(EPEmployee)].RaiseExceptionHandling<EPEmployee.acctCD>(_employeer, null, new PXSetPropertyException(PX.Objects.EP.Messages.Employeedeattach, PXErrorLevel.Warning));
                        }

                        else if (_employeer.Status != BAccount.status.Active)
                        {
                            this.Caches[typeof(EPEmployee)].RaiseExceptionHandling<EPEmployee.acctCD>(_employeer, null, new PXSetPropertyException(PX.Objects.EP.Messages.EmployeeIsInactive, PXErrorLevel.Warning));
                        }
                    }
                    yield return new PXResult<EPCompanyTreeMember, EPEmployee, EPEmployeePosition>(_companyTreeMember, _employeer, _ePEmployeeposision);
                }
            }
        }

        protected virtual IEnumerable parentFolders(
           [PXInt]
            int? workGroupID
       )
        {
            if (workGroupID == null)
            {
                yield return new EPCompanyTreeMaster()
                {
                    WorkGroupID = 0,
                    Description = PXSiteMap.RootNode.Title
                };
            }
            else
            {
                foreach (EPCompanyTreeMaster item in PXSelect<EPCompanyTreeMaster,
                    Where<EPCompanyTreeMaster.parentWGID,
                        Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                    OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>>.Select(this, workGroupID))
                {
                    if (!string.IsNullOrEmpty(item.Description) &&
                        item.WorkGroupID != SelectedFolders.Current.WorkGroupID)
                        yield return item;
                }
            }
        }
        #endregion

        #region Action
        public PXSave<SelectedNode> Save;
        public PXCancel<SelectedNode> Cancel;

        public PXAction<SelectedNode> AddWorkGroup;
        [PXUIField(DisplayName = "Add WorkGroup", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Enabled = true)]
        [PXButton()]
        public virtual IEnumerable addWorkGroup(PXAdapter adapter)
        {
            EPCompanyTreeMaster current = Folders.Current;
            if (current == null)
            {
                current = Folders.Select();
            }                        
            if (current != null)
            {
                int ParentID = (int)current.WorkGroupID;
                EPCompanyTreeMaster inserted = (EPCompanyTreeMaster)Caches[typeof(EPCompanyTreeMaster)].CreateInstance();
                inserted.Description = PX.Objects.CR.Messages.New;
                inserted.ParentWGID = ParentID;

                inserted = Caches[typeof(EPCompanyTreeMaster)].Insert(inserted) as EPCompanyTreeMaster;
                if (inserted != null)
                {
                    inserted.TempChildID = inserted.WorkGroupID;
                    inserted.TempParentID = ParentID;
                    EPCompanyTreeMaster previous = PXSelect<EPCompanyTreeMaster,
                        Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                        OrderBy<Desc<EPCompanyTreeMaster.sortOrder>>>.SelectSingleBound(this, null, ParentID);

                    int sortOrder = (int)previous.SortOrder;
                    sortOrder = sortOrder + 1;
                    inserted.SortOrder = previous != null ? sortOrder : 1;

                    Folders.Cache.ActiveRow = inserted;

                    PXSelect<EPCompanyTreeMaster,
                        Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                        OrderBy<Desc<EPCompanyTreeMaster.sortOrder>>>.Clear(this);
                }                               
            }
            return adapter.Get();
        }

            

        public PXAction<SelectedNode> DeleteWorkGroup;
        [PXUIField(DisplayName = "Delete WorkGroup", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete, Enabled = true)]
        [PXButton()]
        public virtual IEnumerable deleteWorkGroup(PXAdapter adapter)
        {
            if (Folders.Current != null)
            {
                VerifyRecurringBeforeDelete(Folders.Current);
                Caches[typeof(EPCompanyTreeMaster)].Delete(Folders.Current);
            }
            return adapter.Get();
        }



        public PXAction<SelectedNode> down;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageUrl = "~/Icons/NavBar/xps_collapse.gif", DisabledImageUrl = "~/Icons/NavBar/xps_collapseD.gif")]
        public virtual IEnumerable Down(PXAdapter adapter)
        {
            EPCompanyTreeMaster curr = Folders.Current;
            EPCompanyTreeMaster next = PXSelect<EPCompanyTreeMaster,
                Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>,
                    And<EPCompanyTreeMaster.sortOrder, Greater<Required<EPCompanyTreeMaster.parentWGID>>>>,
                OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>>.SelectSingleBound(this, null, Folders.Current.ParentWGID,
                    Folders.Current.SortOrder);

            if (next != null && curr != null)
            {
                int temp = (int)curr.SortOrder;
                curr.SortOrder = next.SortOrder;
                next.SortOrder = temp;
                Caches[typeof(EPCompanyTreeMaster)].Update(next);
                Caches[typeof(EPCompanyTreeMaster)].Update(curr);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> up;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageUrl = "~/Icons/NavBar/xps_expand.gif", DisabledImageUrl = "~/Icons/NavBar/xps_expandD.gif")]
        public virtual IEnumerable Up(PXAdapter adapter)
        {
            EPCompanyTreeMaster curr = Folders.Current;
            EPCompanyTreeMaster prev = PXSelect<EPCompanyTreeMaster,
                Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>,
                    And<EPCompanyTreeMaster.sortOrder, Less<Required<EPCompanyTreeMaster.parentWGID>>>>,
                OrderBy<Desc<EPCompanyTreeMaster.sortOrder>>>.SelectSingleBound(this, null, Folders.Current.ParentWGID,
                    Folders.Current.SortOrder);

            if (prev != null && curr != null)
            {
                int temp = (int)curr.SortOrder;
                curr.SortOrder = prev.SortOrder;
                prev.SortOrder = temp;
                Caches[typeof(EPCompanyTreeMaster)].Update(prev);
                Caches[typeof(EPCompanyTreeMaster)].Update(curr);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> viewEmployee;
        [PXUIField(DisplayName = "View Employee", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual void ViewEmployee()
        {
            if (Members.Current.UserID != null)
            {
                EmployeeMaint graph = CreateInstance<EmployeeMaint>();
                graph.Employee.Current = graph.Employee.Search<EPEmployee.userID>(Members.Current.UserID);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
        }
        
        public PXAction<SelectedNode> MoveWorkGroup;
        [PXUIField(DisplayName = "Move WorkGroup", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton()]
        public virtual IEnumerable moveWorkGroup(PXAdapter adapter)
        {
            EPCompanyTreeMaster cur = Caches[typeof(EPCompanyTreeMaster)].Current as EPCompanyTreeMaster;
            SelectedFolders.Current.WorkGroupID = Folders.Current.WorkGroupID;
            if (cur != null)
            {
                if(SelectedParentFolders.View.Answer == WebDialogResult.None)
                    SelectedParentFolders.Current.WorkGroupID = cur.ParentWGID;
                if (SelectedParentFolders.AskExt() == WebDialogResult.OK)
                {
                    cur.ParentWGID = SelectedParentFolders.Current.WorkGroupID;
                    cur.TempParentID = SelectedParentFolders.Current.WorkGroupID;


                    EPCompanyTreeMaster previous = PXSelect<EPCompanyTreeMaster,
                                            Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                                            OrderBy<Desc<EPCompanyTreeMaster.sortOrder>>>.SelectSingleBound(this, null, SelectedParentFolders.Current.WorkGroupID);

                    if (previous != null)
                    {
                        int sortOrder = (int)previous.SortOrder;
                        cur.SortOrder = sortOrder + 1;                         
                    }
                    else
                    {
                        cur.SortOrder = 1;
                    }
                    cur = Caches[typeof(EPCompanyTreeMaster)].Update(cur) as EPCompanyTreeMaster;
                    PXSelect<EPCompanyTreeMaster,
                     Where<EPCompanyTreeMaster.parentWGID,
                        Equal<Required<EPCompanyTreeMaster.parentWGID>>>,
                     OrderBy<Asc<EPCompanyTreeMaster.sortOrder>>>.Clear(this);
                    Folders.Cache.ActiveRow = cur;
                    Folders.View.RequestRefresh();
                }
            }            
            
            return adapter.Get();                
        }
        #endregion

        #region Event Handler     

       
        protected virtual void EPCompanyTreeMaster_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
            if (row == null) return;
            insertHRecords(row);
        }

        protected virtual void EPCompanyTreeMaster_Description_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (String.IsNullOrEmpty((string)e.NewValue))
            {
                e.NewValue = Messages.EmptyData;
            }

            foreach (EPCompanyTreeMaster item in PXSelect
                <EPCompanyTreeMaster, Where<EPCompanyTreeMaster.description, Equal<Required<EPCompanyTreeMaster.description>>>>.
                Select(this, e.NewValue))
            {
                if (item != null && (item.Description == PX.Objects.CR.Messages.New || item.Description == Messages.EmptyData))
                    throw new PXSetPropertyException(Messages.DublicateEmptyData, "Description");
                if (item != null && item.Description == e.NewValue.ToString())
                    throw new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded, "Description");
            }            
        }

        protected virtual void EPCompanyTreeMaster_Description_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = e.Row as EPCompanyTreeMaster;
            if (row == null)
                return;

            foreach (EPCompanyTreeMaster item in PXSelect
                    <EPCompanyTreeMaster, Where<EPCompanyTreeMaster.description, Equal<Required<EPCompanyTreeMaster.description>>>>.
                Select(this, e.NewValue))
            {
                if (item != null && item.Description == e.NewValue.ToString())
                {
                    if (this.IsImport)
                    {
                        sender.SetStatus(row, PXEntryStatus.Held);
                        deleteHRecords(row.WorkGroupID);
                    }
                }
            }
        }

        protected virtual void EPCompanyTreeMaster_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster;
            if (row == null || row.WorkGroupID == null) return;

            deleteHRecords(row.WorkGroupID);
            deleteRecurring(row);
        }

        protected virtual void EPCompanyTreeMaster_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            EPCompanyTreeMaster row = e.Row as EPCompanyTreeMaster, oldRow = e.OldRow as EPCompanyTreeMaster;
            if (row == null || oldRow == null) return;
            if (row.ParentWGID != oldRow.ParentWGID)
            {
                updateHRecordParent(row);
            }            
        }

        public override void Persist()
        {
            base.Persist();
            foreach (EPCompanyTreeMaster item in Caches[typeof(EPCompanyTreeMaster)].Cached)
            {
                if (item.TempParentID < 0)
                {
                    foreach (EPCompanyTreeMaster item2 in Caches[typeof(EPCompanyTreeMaster)].Cached)
                    {
                        if (item2.TempChildID == item.TempParentID)
                        {
                            item.ParentWGID = item2.WorkGroupID;
                            item.TempParentID = item2.WorkGroupID;
                            Caches[typeof(EPCompanyTreeMaster)].SetStatus(item, PXEntryStatus.Updated);
                        }
                    }
                }
            }
            base.Persist();
            Members.View.RequestRefresh();
        }

        public virtual void EPCompanyTreeMaster_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
        }

        protected virtual void EPCompanyTreeMember_Active_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPCompanyTreeMember row = (EPCompanyTreeMember)e.Row;
            if (row == null) return;

            string res = "";            

             foreach (EPAssignmentMap map in PXSelectJoin<EPAssignmentMap,
                                    InnerJoin<EPAssignmentRoute, On<EPAssignmentRoute.assignmentMapID, Equal<EPAssignmentMap.assignmentMapID>>>,
                                    Where<EPAssignmentRoute.ownerID, Equal<Required<EPAssignmentRoute.ownerID>>>>.
                                    Select(this, row.UserID))
            {
                if (map == null)
                    throw new PXException(Objects.EP.Messages.WorkgroupIsInUse);

                string MapType = "";
                if (map.MapType == EPMapType.Assignment)
                {
                    MapType = Objects.EP.Messages.Assignment;
                }
                else
                {
                    MapType = Objects.EP.Messages.Approval;
                }

                EPEmployee Employee = PXSelect<EPEmployee,
                Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, row.UserID);
                if(Employee != null)
                    res = res + string.Format(Objects.EP.Messages.EmployeeIsInUseAtAssignmentMapPart1, Employee.AcctName, MapType, map.Name) + "\n";                
            }

            if (!String.IsNullOrEmpty(res))
                sender.RaiseExceptionHandling<EPCompanyTreeMember.userID>(row, row.UserID, new PXSetPropertyException(res + "\n" + Objects.EP.Messages.EmployeeIsInUseAtAssignmentMapPart2, PXErrorLevel.RowWarning));
        }            

        protected virtual void EPCompanyTreeMember_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            UpdateOwner(e.Row);            
        }

        protected virtual void EPCompanyTreeMember_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            UpdateOwner(e.Row);
        }

        private void UpdateOwner(object row)
        {
            EPCompanyTreeMember curr = (EPCompanyTreeMember)row;

            if (curr.IsOwner == true)
            {
                foreach (EPCompanyTreeMember item in Members.Select(curr.WorkGroupID))
                {
                    if (item.IsOwner != true || item.UserID == curr.UserID) continue;

                    item.IsOwner = false;
                    Members.Cache.Update(item);
                }
            }
            Members.View.RequestRefresh();
        }

        private bool VerifyRecurringBeforeDelete(EPCompanyTreeMaster map)
        {
            if (map != null)
            {
                foreach (EPCompanyTreeMaster child in PXSelect<EPCompanyTreeMaster,
                     Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>>
                      .Select(this, map.WorkGroupID))
                {
                    VerifyRecurringBeforeDelete(child);
                }
            }
            return VerifyForUsing(map);
        }

        private bool VerifyForUsing(EPCompanyTreeMaster row)
        {
            string res = "";
            foreach (EPAssignmentMap map in PXSelectJoin<EPAssignmentMap,
                      InnerJoin<EPRule, On<EPRule.assignmentMapID, Equal<EPAssignmentMap.assignmentMapID>>>,
                        Where<EPRule.workgroupID, Equal<Required<EPRule.workgroupID>>>>.
                            Select(this, row.WorkGroupID))
            {
                if (map == null)
                    throw new PXException(Objects.EP.Messages.WorkgroupIsInUse);

                string MapType = "";
                if (map.MapType == EPMapType.Assignment)
                {
                    MapType = Objects.EP.Messages.Assignment;
                }
                else
                {
                    MapType = Objects.EP.Messages.Approval;
                }
                res = res + string.Format(Objects.EP.Messages.WorkgroupIsInUseAtAssignmentMap, row.Description, MapType, map.Name) + "\n";
            }
            if (!String.IsNullOrEmpty(res))
                throw new PXException(res);

            return true;
        }

        private void deleteRecurring(EPCompanyTreeMaster map)
        {
            if (map != null)
            {
                foreach (EPCompanyTreeMaster child in PXSelect<EPCompanyTreeMaster,
                                                 Where<EPCompanyTreeMaster.parentWGID, Equal<Required<EPCompanyTreeMaster.parentWGID>>>>
                                             .Select(this, map.WorkGroupID))
                    deleteRecurring(child);                
                Caches[typeof(EPCompanyTreeMaster)].Delete(map);                
            }            
        }
        #endregion

        #region Extended Tree
        public PXSelect<EPCompanyTreeH> treeH;
        public PXSelect<EPCompanyTreeH,
            Where<EPCompanyTreeH.workGroupID, Equal<Required<EPCompanyTreeH.workGroupID>>>,
            OrderBy<Desc<EPCompanyTreeH.parentWGLevel>>> parents;
        public PXSelect<EPCompanyTreeH,
            Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>>,
            OrderBy<Asc<EPCompanyTreeH.workGroupLevel>>> childrens;

        private void insertHRecords(EPCompanyTreeMaster record)
        {
            int level = -1;
            foreach (EPCompanyTreeH item in parents.Select(record.ParentWGID))
            {
                if (level == -1)
                    level = item.WorkGroupLevel.GetValueOrDefault() + 1;
                EPCompanyTreeH ins = (EPCompanyTreeH)treeH.Cache.CreateCopy(item);
                ins.WorkGroupID = record.WorkGroupID;
                ins.WorkGroupLevel = level;
                ins.WaitTime += record.WaitTime;
                treeH.Cache.Insert(ins);
            }
            EPCompanyTreeH self = new EPCompanyTreeH();
            self.WorkGroupID = record.WorkGroupID;
            self.WorkGroupLevel = level;
            self.ParentWGID = record.WorkGroupID;
            self.ParentWGLevel = level;
            self.WaitTime = 0;
            treeH.Cache.Insert(self);
        }

        private void deleteHRecords(int? workGroupID)
        {
            foreach (EPCompanyTreeH item in parents.Select(workGroupID))
                treeH.Cache.Delete(item);
        }

        private void updateHRecordParent(EPCompanyTreeMaster item)
        {
            PXResultset<EPCompanyTreeH> newParents = parents.Select(item.ParentWGID);
            int parentLevel = -1;
            if (newParents.Count > 0)
            {
                EPCompanyTreeH newParent = newParents[newParents.Count - 1];
                parentLevel = newParent.WorkGroupLevel.GetValueOrDefault();
            }

            int levelDelta = 0;
            EPCompanyTreeH root = null;
            foreach (EPCompanyTreeH child in childrens.Select(item.WorkGroupID))
            {
                if (root == null)
                {
                    root = (EPCompanyTreeH)treeH.Cache.CreateCopy(child);
                    levelDelta = parentLevel + 1 - child.WorkGroupLevel.GetValueOrDefault();
                }

                foreach (EPCompanyTreeH parent in parents.Select(child.WorkGroupID))
                {
                    if (parent.WorkGroupID != parent.ParentWGID &&
                            parent.ParentWGLevel < root.WorkGroupLevel)
                    {
                        treeH.Cache.Delete(parent);
                    }
                    else
                    {
                        EPCompanyTreeH upd = (EPCompanyTreeH)treeH.Cache.CreateCopy(parent);
                        upd.WorkGroupLevel += levelDelta;
                        upd.ParentWGLevel += levelDelta;
                        treeH.Cache.Update(upd);
                    }
                }

                foreach (EPCompanyTreeH parent in newParents)
                {
                    EPCompanyTreeH ins = (EPCompanyTreeH)treeH.Cache.CreateCopy(parent);
                    ins.WorkGroupID = child.WorkGroupID;
                    ins.WorkGroupLevel = child.WorkGroupLevel;
                    ins.WaitTime += child.WaitTime + item.WaitTime;
                    treeH.Cache.Insert(ins);
                }
            }
        }
        #endregion
      
    }
}



