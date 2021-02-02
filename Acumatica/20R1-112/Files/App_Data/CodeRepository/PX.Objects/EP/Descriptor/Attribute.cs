using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.SQLTree;
using PX.Data.Update.ExchangeService;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Interfaces;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.SM;
using PX.TM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web.Compilation;

namespace PX.TM
{
    #region PXOwnerSelectorAttribute

    using Objects.EP;
    /// <summary>
    /// Allows show employees for specified work group.
    /// </summary>
    /// <example>
    /// [PXOwnerSelector(typeof(MyDac.myField)]
    /// </example>
    public class PXOwnerSelectorAttribute : PXAggregateAttribute
    {
        #region SelectorAttribute

        public class OwnerSubstituteSelectorAttribute : PXSelectorAttribute
        {
            public OwnerSubstituteSelectorAttribute(Type type, params Type[] fieldList)
                : base(type, fieldList)
            {
            }

            public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
            {
                Guid value;
                if (e.NewValue != null && !GUID.TryParse(e.NewValue.ToString(), out value))
                {
                    base.SubstituteKeyFieldUpdating(sender, e);
                }
            }

            public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                base.SubstituteKeyFieldSelecting(sender, e);
            }





            public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
            {
                var origValue = sender.GetValue(e.Row, _FieldName);
                base.DescriptionFieldSelecting(sender, e, alias);
                if (origValue != null && e.ReturnValue == null)
                {
                    using (var del = new PXReadDeletedScope())
                    {
                        Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.SelectSingleBound(sender.Graph, new object[] { }, origValue);
                        e.ReturnValue = (user != null)
                            ? user.DisplayName
                            : origValue;

                        e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, null, null, alias
                            , null, null, PXLocalizer.Localize(Messages.OwnerNotFound, typeof(Messages).FullName), PXErrorLevel.Warning, false, null, null, PXUIVisibility.Visible, null, null, null);
                    }
                }
            }
        }
        #endregion
        #region State
        protected readonly int _SelAttrIndex;
        protected Type _workgroupType;
        #endregion

        #region DAC
        [PXProjection(typeof(Select<CREmployee, Where<CREmployee.userID, IsNotNull>>), Persistent = false)]
        [CRCacheIndependentPrimaryGraph(
            typeof(EmployeeMaint),
            typeof(Select<PX.Objects.EP.EPEmployee,
                Where<PX.Objects.EP.EPEmployee.bAccountID, Equal<Current<EPEmployee.bAccountID>>>>))]
        [CRCacheIndependentPrimaryGraph(
            typeof(AccessUsers),
            typeof(Select<Users,
                Where<Current<EPEmployee.bAccountID>, IsNull, And<Users.pKID, Equal<Current<EPEmployee.pKID>>>>>))]
        [CRCacheIndependentPrimaryGraph(
            typeof(EmployeeMaint),
            typeof(Where<Current<EPEmployee.bAccountID>, IsNull>))]
        [PXHidden]
        public class EPEmployee : IBqlTable
        {
            #region PKID
            public abstract class pKID : PX.Data.BQL.BqlGuid.Field<pKID> { }
            protected Guid? _PKID;
            [PXDBGuidMaintainDeleted(BqlField = typeof(CREmployee.userID))]
            [PXDefault]
            [PXUser]
            [PXUIField(Visibility = PXUIVisibility.Invisible)]
            public virtual Guid? PKID
            {
                get
                {
                    return this._PKID;
                }
                set
                {
                    this._PKID = value;
                }
            }
            #endregion
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBIdentity(BqlTable = typeof(CREmployee))]
            [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region AcctCD
            public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
            protected String _AcctCD;
            [PXDBString(30, IsUnicode = true, InputMask = "", BqlTable = typeof(CREmployee), IsKey = true)]
            [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String AcctCD
            {
                get
                {
                    return this._AcctCD;
                }
                set
                {
                    this._AcctCD = value;
                }
            }
            #endregion
            #region AcctName
            public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
            protected String _AcctName;
            [PXDBString(60, IsUnicode = true, BqlTable = typeof(CREmployee))]
            [PXUIField(DisplayName = "Employee Name")]
            public virtual String AcctName
            {
                get
                {
                    return this._AcctName;
                }
                set
                {
                    this._AcctName = value;
                }
            }
            #endregion
            #region DepartmentID
            public abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
            protected String _DepartmentID;
            [PXDBString(30, IsUnicode = true, BqlTable = typeof(CREmployee))]
            [PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
            [PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String DepartmentID
            {
                get
                {
                    return this._DepartmentID;
                }
                set
                {
                    this._DepartmentID = value;
                }
            }
            #endregion
            #region Status
            public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
            protected String _Status;
            [PXDBString(1, IsFixed = true, BqlField = typeof(CREmployee.status))]
            [PXUIField(DisplayName = "Status")]
            [BAccount.status.List()]
            [PXDefault(BAccount.status.Active)]
            public virtual String Status
            {
                get
                {
                    return this._Status;
                }
                set
                {
                    this._Status = value;
                }
            }
            #endregion
            #region DefContactID
            public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
            [PXDBInt(BqlField = typeof(CREmployee.defContactID))]
            [PXUIField(DisplayName = "Default Contact")]
            [PXSelector(typeof(Search<Contact.contactID, Where<Contact.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
            public virtual Int32? DefContactID
            {
                get;
                set;
            }
            #endregion
            #region ParentBAccountID
            public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
            [PXDBInt(BqlField = typeof(CREmployee.parentBAccountID))]
            [PXUIField(DisplayName = "Branch")]
            public virtual Int32? ParentBAccountID
            {
                get;
                set;
            }
			#endregion
			#region SupervisorID
			public abstract class supervisorID : IBqlField { }

			protected Int32? _SupervisorID;
			[PXDBInt(BqlField = typeof(CREmployee.supervisorID))]
			[PXEPEmployeeSelector]
			[PXUIField(DisplayName = "Reports to", Visibility = PXUIVisibility.Visible)]
			public virtual Int32? SupervisorID
			{
				get
				{
					return this._SupervisorID;
				}
				set
				{
					this._SupervisorID = value;
				}
			}
			#endregion
		}
		#endregion

		public PXOwnerSelectorAttribute() : this(null)
        {

        }
        public PXOwnerSelectorAttribute(Type workgroupType)
            : this(workgroupType, null)
        {
        }

        protected PXOwnerSelectorAttribute(Type workgroupType, Type search, bool validateValue = true, bool inquiryMode = false)
        {
            PXSelectorAttribute selector;
            _Attributes.Add(selector = new OwnerSubstituteSelectorAttribute(search ?? CreateSelect(workgroupType),
            typeof(EPEmployee.acctName), typeof(EPEmployee.acctCD),
            typeof(EPEmployee.departmentID)));
            _SelAttrIndex = _Attributes.Count - 1;

            selector.DescriptionField = typeof(EPEmployee.acctName);
            selector.SubstituteKey = typeof(EPEmployee.acctCD);
            selector.ValidateValue = validateValue;
            selector.CacheGlobal = true;
            _workgroupType = workgroupType;

            if (!inquiryMode)
            {
                _Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.status, IsNull, Or<EPEmployee.status, NotEqual<BAccount.status.inactive>>>), Objects.EP.Messages.InactiveEpmloyee, typeof(EPEmployee.acctCD), typeof(EPEmployee.status)));
            }
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (_workgroupType != null)
            {
                sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
                sender.Graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(_workgroupType), _workgroupType.Name, FieldVerifying);
            }
        }

        private static Type CreateSelect(Type workgroupType)
        {
            if (workgroupType == null)
                return typeof(Search<EPEmployee.pKID, Where<EPEmployee.acctCD, IsNotNull>>);

            return BqlCommand.Compose(
                            typeof(Search2<,,>), typeof(EPEmployee.pKID),
                            typeof(LeftJoin<,>), typeof(EPCompanyTreeMember),
                            typeof(On<,,>), typeof(EPCompanyTreeMember.userID), typeof(Equal<EPEmployee.pKID>),
                            typeof(And<,>), typeof(EPCompanyTreeMember.workGroupID), typeof(Equal<>), typeof(Optional<>), workgroupType,
                            typeof(Where<,,>),
                            typeof(Optional<>), workgroupType, typeof(IsNull),
                            typeof(Or<EPCompanyTreeMember.userID, IsNotNull>)
                            );
        }

        protected virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {

        }

        protected virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (e.Row != null)
            {
                int? WorkGroupID = (int?)e.NewValue;
                int? oldWorkGroupID = (int?)sender.GetValue(e.Row, _workgroupType.Name);
                string owner =
                    sender.GetValuePending(e.Row, _FieldName) as string ??
                    (sender.GetValueExt(e.Row, _FieldName) as PXFieldState)?.Value as string;

                var selector = _Attributes[_SelAttrIndex] as PXSelectorAttribute;
                if (selector != null)
                {
                    var copy_row = sender.CreateCopy(e.Row);
                    var fu = new PXFieldUpdatingEventArgs(copy_row, owner);
                    selector.SubstituteKeyFieldUpdating(sender, fu);
                    Guid? OwnerID = fu.NewValue as Guid?;

                    if (WorkGroupID != oldWorkGroupID && !BelongsToWorkGroup(sender.Graph, WorkGroupID, OwnerID))
                    {
                        sender.SetValue(copy_row, _FieldName, OwnerWorkGroup(sender.Graph, WorkGroupID));
                        sender.SetValuePending(e.Row, _FieldName, (string)(sender.GetValueExt(copy_row, _FieldName) as PXFieldState)?.Value);
                    }
                }
            }
        }
        public static bool BelongsToWorkGroup(PXGraph graph, int? WorkGroupID, Guid? OwnerID)
        {
            if (WorkGroupID == null && OwnerID != null) return true;
            if (WorkGroupID != null && OwnerID == null) return false;

            return PXSelect<EPCompanyTreeMember,
                    Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
                    And<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>>
                    .Select(graph, WorkGroupID, OwnerID).Count > 0;
        }

        public static Guid? OwnerWorkGroup(PXGraph graph, int? WorkGroupID)
        {
            EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
                Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
                    And<EPCompanyTreeMember.isOwner, Equal<Required<EPCompanyTreeMember.isOwner>>>>>
                .Select(graph, WorkGroupID, 1);
            return member != null ? member.UserID : null;
        }

        public static int? DefaultWorkgroup(PXGraph graph, Guid? userID)
        {
            PXSelectBase<EPCompanyTreeMember> cmd = new PXSelectJoin<EPCompanyTreeMember,
                  InnerJoin<EPCompanyTreeH, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTreeH.workGroupID>>>,
                  Where<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>(graph);
            EPCompanyTreeMember m = cmd.SelectSingle(userID ?? graph.Accessinfo.UserID);
            return m != null ? m.WorkGroupID : null;
        }
    }

    #endregion

    #region PXSubordinateOwnerSelectorAttribute

    /// <summary>
    /// Allows show employees which are subordinated or coworkers for current logined employee.
    /// </summary>
    /// <example>
    /// [PXSubordinateOwnerSelector]
    /// </example>
    public class PXSubordinateOwnerSelectorAttribute : PXOwnerSelectorAttribute
    {
        public PXSubordinateOwnerSelectorAttribute()
            : base(null, typeof(Search5<EPEmployee.pKID,
                LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.userID, Equal<EPEmployee.pKID>>>,
                Where<EPEmployee.pKID, Equal<Current<AccessInfo.userID>>,
                   Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>,
                Aggregate<GroupBy<EPEmployee.pKID>>>), false, true)
        {
        }
    }
    #endregion
}

namespace PX.Objects.EP
{
    #region PXSubordinateSelectorAttribute
    /// <summary>
    /// Allow show employees which are subordinated of coworkers for logined employee.
    /// You can specify additional filter for EPEmployee records.
    /// It's a 'BIZACCT' dimension selector.
    /// </summary>
    /// <example>
    /// [PXSubordinateSelector]
    /// </example>
    public class PXSubordinateSelectorAttribute : PXAggregateAttribute
    {
        public Type DescriptionField
        {
            get
            {
                return this.GetAttribute<PXSelectorAttribute>().DescriptionField;
            }
            set
            {
                this.GetAttribute<PXSelectorAttribute>().DescriptionField = value;
            }
        }

        public Type SubstituteKey
        {
            get
            {
                return this.GetAttribute<PXSelectorAttribute>().SubstituteKey;
            }
            set
            {
                this.GetAttribute<PXSelectorAttribute>().SubstituteKey = value;
            }
        }

        public PXSubordinateSelectorAttribute(Type where)
            : this("BIZACCT", GetCommand(where), true, true)
        {
        }

        protected PXSubordinateSelectorAttribute(string DimensionName, Type seach, bool validCombo, bool restrictInactiveEmployee)
        {
            PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
            attr.ValidComboRequired = validCombo;
            _Attributes.Add(attr);

            PXSelectorAttribute selattr = new PXSelectorAttribute(seach,
                typeof(CREmployee.acctCD),
                typeof(CREmployee.bAccountID), typeof(CREmployee.acctName),
                typeof(CREmployee.departmentID));
            selattr.SubstituteKey = typeof(CREmployee.acctCD);
            selattr.DescriptionField = typeof(CREmployee.acctName);

            _Attributes.Add(selattr);
            if (restrictInactiveEmployee)
                _Attributes.Add(new PXRestrictorAttribute(typeof(Where<CREmployee.status, NotEqual<BAccount.status.inactive>>), Objects.EP.Messages.InactiveEpmloyee, typeof(CREmployee.acctCD), typeof(CREmployee.status)));

        }

        public PXSubordinateSelectorAttribute()
            : this(null)
        {
        }

        private static Type GetCommand(Type where)
        {
            var whereType = typeof(Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                        Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>);
            if (where != null)
                whereType = BqlCommand.Compose(typeof(Where2<,>),
                    typeof(Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                        Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>),
                    typeof(And<>), where);
            return BqlCommand.Compose(typeof(Search5<,,,>), typeof(CREmployee.bAccountID),
                typeof(LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.userID, Equal<CREmployee.userID>>>),
                whereType,
                typeof(Aggregate<GroupBy<CREmployee.acctCD>>));
        }


        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating);
            sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
        }

        protected virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            CREmployee employee = PXSelect<CREmployee, Where<CREmployee.acctCD, Equal<Required<CREmployee.acctCD>>>>
                .SelectWindowed(sender.Graph, 0, 1, e.NewValue);
            if (employee != null)
            {
                e.NewValue = employee.BAccountID;
                e.Cancel = true;
            }
            else
            {
                PXFieldUpdating fu = this.GetAttribute<PXDimensionAttribute>().FieldUpdating;
                fu(sender, e);
                e.Cancel = false;

                fu = this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
                fu(sender, e);

            }
        }

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            if (typeof(ISubscriber) != typeof(IPXFieldUpdatingSubscriber) &&
                  typeof(ISubscriber) != typeof(IPXRowPersistingSubscriber) &&
                    typeof(ISubscriber) != typeof(IPXFieldDefaultingSubscriber))
            {
                base.GetSubscriber<ISubscriber>(subscribers);
            }
        }

        public static bool IsSubordinated(PXGraph graph, object employeeId)
        {
            var command = BqlCommand.CreateInstance(GetCommand(null));
            var filter = new[] { new PXFilterRow
                            {
                                DataField = typeof(CREmployee.bAccountID).Name,
                                Condition = PXCondition.EQ,
                                Value = employeeId
                            }};
            var view = new PXView(graph, true, command);
            int startRow = 0;
            int totalRows = 0;
            var res = view.Select(null, null, null, null, null, filter, ref startRow, 1, ref totalRows);
            return res.Return(_ => _.Count(), 0) > 0;
        }
    }

    #endregion

    #region PXEPEmployeeSelectorAttribute

    /// <summary>
    /// Dimension selector for EPEmployee.
    /// </summary>
    /// <example>
    /// [PXEPEmployeeSelector]
    /// </example>
    public class PXEPEmployeeSelectorAttribute : PXDimensionSelectorAttribute
    {
        public PXEPEmployeeSelectorAttribute()
            : base(
                "EMPLOYEE",
            typeof(Search<CR.Standalone.EPEmployee.bAccountID>),
            typeof(CR.Standalone.EPEmployee.acctCD),
                typeof(CR.Standalone.EPEmployee.bAccountID),
                typeof(CR.Standalone.EPEmployee.acctCD),
                typeof(CR.Standalone.EPEmployee.acctName),
            typeof(CR.Standalone.EPEmployee.departmentID))
        {
            DescriptionField = typeof(CR.Standalone.EPEmployee.acctName);
        }
    }

    #endregion

	#region PXEPEmployeeSelectorAttribute

	public class PXEPEPEmployeeSelectorAttribute : PXDimensionSelectorAttribute
	{
		public PXEPEPEmployeeSelectorAttribute()
			: base(
				"EMPLOYEE",
			typeof(Search<EP.EPEmployee.bAccountID>),
			typeof(EP.EPEmployee.acctCD),
				typeof(EP.EPEmployee.bAccountID),
				typeof(EP.EPEmployee.acctCD),
				typeof(EP.EPEmployee.acctName),
			typeof(EP.EPEmployee.departmentID))
		{
			DescriptionField = typeof(EP.EPEmployee.acctName);
		}
	}

	#endregion

    public class PXWorkgroupSelectorAttribute : PXSelectorAttribute
    {
        public PXWorkgroupSelectorAttribute()
            : this(null)
        {

        }
        public PXWorkgroupSelectorAttribute(Type rootWorkgroupID)
            : base(rootWorkgroupID == null
                            ? typeof(Search3<EPCompanyTree.workGroupID,
                                 LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>, And<EPCompanyTreeMember.isOwner, Equal<True>>>,
                               LeftJoin<CREmployee, On<CREmployee.userID, Equal<EPCompanyTreeMember.userID>>>>,
                                 OrderBy<Asc<EPCompanyTree.description, Asc<EPCompanyTree.workGroupID>>>>)
                       : BqlCommand.Compose(
                           typeof(Search2<,,,>), typeof(EPCompanyTree.workGroupID),
                                typeof(InnerJoin<EPCompanyTreeH, On<EPCompanyTreeH.workGroupID, Equal<EPCompanyTree.workGroupID>>,
                                             LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>, And<EPCompanyTreeMember.isOwner, Equal<True>>>,
                                             LeftJoin<CREmployee, On<CREmployee.userID, Equal<EPCompanyTreeMember.userID>>>>>),
                                typeof(Where2<,>),
                                    typeof(Where<,,>), typeof(Current<>), rootWorkgroupID, typeof(IsNotNull), typeof(And<,>), typeof(EPCompanyTreeH.parentWGID), typeof(Equal<>), typeof(Current<>), rootWorkgroupID,
                                typeof(Or<>),
                                    typeof(Where<,,>), typeof(Current<>), rootWorkgroupID, typeof(IsNull), typeof(And<,>), typeof(EPCompanyTreeH.parentWGID), typeof(Equal<>), typeof(EPCompanyTreeH.workGroupID),
                           typeof(OrderBy<
                               Asc<EPCompanyTree.description,
                                    Asc<EPCompanyTree.workGroupID>>>)), typeof(EPCompanyTree.description), typeof(CREmployee.acctCD), typeof(CREmployee.acctName))
        {
            SubstituteKey = typeof(EPCompanyTree.description);
        }
    }

    #region EPExpenceClaimSelectorAttribute

    /// <summary>
    /// Allow show expence claim records.
    /// </summary>
    /// <example>
    /// [EPExpenceClaimSelector]
    /// </example>
    public class EPExpenceClaimSelectorAttribute : PXSelectorAttribute
    {
        public EPExpenceClaimSelectorAttribute()
            : base(typeof(Search2<EPExpenseClaim.refNbr,
                    InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
                    Where<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
                         Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
                         Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
                         Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>,
                         Or<EPExpenseClaim.employeeID, WingmanUser<Current<AccessInfo.userID>>>>>>>, OrderBy<Desc<EPExpenseClaim.refNbr>>>)
                , typeof(EPExpenseClaim.docDate)
                , typeof(EPExpenseClaim.refNbr)
                , typeof(EPExpenseClaim.status)
                , typeof(EPExpenseClaim.docDesc)
                , typeof(EPExpenseClaim.curyDocBal)
                , typeof(EPExpenseClaim.curyID)
                , typeof(EPEmployee.acctName)
                , typeof(EPExpenseClaim.departmentID)
                )
        {
        }
    }

    #endregion

    #region EPExpenceReceiptSelectorAttribute

    /// <summary>
    /// Allow show expence receipt records.
    /// </summary>
    /// <example>
    /// [EPExpenceReceiptSelector]
    /// </example>
    public class EPExpenceReceiptSelectorAttribute : PXSelectorAttribute
    {
        public EPExpenceReceiptSelectorAttribute()
            : base(typeof(Search2<EPExpenseClaimDetails.claimDetailCD,
                    LeftJoin<EPExpenseClaim, On<EPExpenseClaim.refNbr, Equal<EPExpenseClaimDetails.refNbr>>,
                    InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaimDetails.employeeID>>>>,
                    Where<EPExpenseClaimDetails.createdByID, Equal<Current<AccessInfo.userID>>,
                         Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
                         Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
                         Or<EPExpenseClaimDetails.employeeID, WingmanUser<Current<AccessInfo.userID>>,
                         Or<EPExpenseClaimDetails.noteID, Approver<Current<AccessInfo.userID>>,
                        Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>>>,
                    OrderBy<Desc<EPExpenseClaimDetails.claimDetailCD>>>)
                    , typeof(EPExpenseClaimDetails.claimDetailCD)
                    , typeof(EPExpenseClaimDetails.expenseDate)
                    , typeof(EPExpenseClaimDetails.curyTranAmt)
                    , typeof(EPExpenseClaimDetails.curyID)
                    , typeof(EPExpenseClaimDetails.employeeID)
                    , typeof(EPExpenseClaimDetails.refNbr)
                    , typeof(EPExpenseClaimDetails.status)
                )
        {
        }
    }

    #endregion

    #region EPAcctSubDefault

    public class EPAcctSubDefault
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues
            {
                get
                {
                    return _AllowedValues;
                }
            }

            public string[] AllowedLabels
            {
                get
                {
                    return _AllowedLabels;
                }
            }

            public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
                : base(AllowedValues, AllowedLabels)
            {
            }
        }

        /// <summary>
        /// Specialized version of the string list attribute which represents <br/>
        /// the list of the possible sources of the segments for the sub-account <br/>
        /// defaulting in the AP transactions. <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
        {
            public ClassListAttribute()
                : base(new string[] { MaskEmployee, MaskItem, MaskCompany, MaskProject, MaskTask, MaskLocation }, new string[] { Messages.MaskEmployee, Messages.MaskItem, Messages.MaskCompany, PM.Messages.Project, Messages.Task, !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation })
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                _AllowedValues = new[] { MaskEmployee, MaskItem, MaskCompany, MaskProject, MaskTask, MaskLocation };
                _AllowedLabels = new[] { Messages.MaskEmployee, Messages.MaskItem, Messages.MaskCompany, PM.Messages.Project, Messages.Task, !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation };
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }
        }
        public const string MaskEmployee = "E";
        public const string MaskItem = "I";
        public const string MaskCompany = "C";
        public const string MaskProject = "P";
        public const string MaskTask = "T";
        public const string MaskLocation = "L";
    }


    #endregion


    #region SubAccountMaskAttribute

    /// <summary>
    /// Determine mask for sub accounts used in EP module.
    /// </summary>
    [PXDBString(30, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [EPAcctSubDefault.ClassList]
    public sealed class SubAccountMaskAttribute : AcctSubAttribute
    {
        private const string _DimensionName = "SUBACCOUNT";
        private const string _MaskName = "EPSETUPSALE";
        public SubAccountMaskAttribute()
            : base()
        {
            PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, EPAcctSubDefault.MaskEmployee, new EPAcctSubDefault.ClassListAttribute().AllowedValues, new EPAcctSubDefault.ClassListAttribute().AllowedLabels);
            attr.ValidComboRequired = false;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<EPAcctSubDefault.ClassListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (EPAcctSubDefault.ClassListAttribute)_Attributes.First(x => x.GetType() == typeof(EPAcctSubDefault.ClassListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
            where Field : IBqlField
        {
            try
            {
                return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new EPAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
            }
            catch (PXMaskArgumentException ex)
            {
                PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
                string fieldName = fields[ex.SourceIdx].Name;
                throw new PXMaskArgumentException(new EPAcctSubDefault.ClassListAttribute().AllowedValues[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
            }
        }
    }

    #endregion

    #region EPMessageType

    public sealed class EPMessageType
    {
        /// <summary>
        /// List of attendee message types.
        /// </summary>
        /// <example>
        /// [EPMessageTypeList]
        /// </example>
        public sealed class EPMessageTypeListAttribute : PXIntListAttribute
        {
            public EPMessageTypeListAttribute()
                : base(new int[] { Invitation, CancelInvitation },
                         new string[] { Messages.Invitation, Messages.CancelInvitation })
            {

            }
        }

        public const int Invitation = 1;
        public const int CancelInvitation = 2;

        public class invitation : PX.Data.BQL.BqlInt.Constant<invitation>
		{
            public invitation() : base(Invitation) { }
        }

        public class cancelInvitation : PX.Data.BQL.BqlInt.Constant<cancelInvitation>
		{
            public cancelInvitation() : base(CancelInvitation) { }
        }
    }

    #endregion

    #region EPWikiPageSelectorAttribute
    /// <summary>
    /// Allow show articles of certain wiki.
    /// </summary>
    /// <example>
    /// [EPWikiPageSelector]
    /// </example>
    public sealed class EPWikiPageSelectorAttribute : PXCustomSelectorAttribute
    {
        private readonly Type _wiki;

        public EPWikiPageSelectorAttribute() : this(null) { }

        public EPWikiPageSelectorAttribute(Type wiki) :
            base(typeof(WikiPageSimple.pageID))
        {
            _wiki = wiki;
            _ViewName = GenerateViewName();
        }

        protected override string GenerateViewName()
        {
            return string.Concat(base.GenerateViewName(), "_", _wiki == null ? null : _wiki.Name);
        }

        public IEnumerable GetRecords([PXDBGuid] Guid? wikiId)
        {
            if (wikiId != null || _wiki != null && BqlCommand.GetItemType(_wiki) != null)
            {
                var wikiCache = _Graph.Caches[BqlCommand.GetItemType(_wiki)];
                var id = wikiId;
                if (id == null && _wiki != null)
                {
                    var idValue = wikiCache.GetValue(wikiCache.Current, _wiki.Name);
                    if (idValue != null) id = GUID.CreateGuid(idValue.ToString());
                }
                if (id != null)
                    foreach (WikiPageSimple page in
                        PXSelect<WikiPageSimple, Where<WikiPageSimple.wikiID, Equal<Required<WikiPageSimple.wikiID>>>>.
                            Select(_Graph, id))
                    {
                        if (PXSiteMap.WikiProvider.GetAccessRights(page.PageID.Value) >= PXWikiRights.Select)
                        {
                            PXSiteMapNode node = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(page.PageID.Value);
                            page.Title = (node != null && !string.IsNullOrEmpty(node.Title)) ? node.Title : page.Name;
                            yield return page;
                        }
                    }
            }
        }
    }

    #endregion

    #region EPAllDaySupportDateAttribute
    public class EPAllDaySupportDateTimeAttribute : PXDBDateAndTimeAttribute
    {
        public Type AllDayField { get; set; }

        public override void Time_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            SetPreserveTime(sender, e.Row);
            base.Time_FieldSelecting(sender, e);
        }

        public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            SetPreserveTime(sender, e.Row);
            base.CommandPreparing(sender, e);
        }

        public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            SetPreserveTime(sender, e.Row);
            base.RowSelecting(sender, e);
        }

        private void SetPreserveTime(PXCache sender, object row)
        {
            if (AllDayField != null) PreserveTime = (bool?)sender.GetValue(row, sender.GetField(AllDayField)) != true;
        }
    }
    #endregion

    #region EPStartDateAttribute
    public class EPStartDateAttribute : EPAllDaySupportDateTimeAttribute
    {
        private string _DisplayFieldName;

        public EPStartDateAttribute()
        {
            _PreserveTime = true;
            base.UseTimeZone = true;
            base.WithoutDisplayNames = true;
        }

        public string DisplayName { get; set; }

        public bool IgnoreRequireTimeOnActivity { get; set; }

        public override bool UseTimeZone
        {
            get { return base.UseTimeZone; }
            set { }
        }

        public override void CacheAttached(PXCache sender)
        {
            if (!typeof(CRActivity).IsAssignableFrom(sender.GetItemType()))
                throw new ArgumentException(Messages.CRActivityIsExpected);

            _DisplayFieldName = _FieldName + "_Display";
            sender.Fields.Add(_DisplayFieldName);
            sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _DisplayFieldName, _DisplayFieldName_FieldSelecting);
            base.CacheAttached(sender);
        }

        private void _DisplayFieldName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
        {
            var info = string.Format("{0:g}", sender.GetValue(args.Row, _FieldOrdinal));
            string localDispName = DisplayName;
            if (!CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture))
                localDispName = PXLocalizer.Localize(DisplayName, sender.GetItemType().FullName);

            args.ReturnState = PXFieldState.CreateInstance(info, typeof(string), null, null, null, null, null,
                        null, _DisplayFieldName, null, localDispName, null, PXErrorLevel.Undefined, false,
                        true, true, PXUIVisibility.Visible, null, null, null);
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            this.InputMask = this.DisplayMask = RequireTimeOnActivity(sender) ? "g" : "d";
            base.FieldSelecting(sender, e);
        }

        public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            EPSetup setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetupSelect<EPSetup>(sender.Graph).SelectSingle();

            if (e.NewValue != null && (setup == null || setup.RequireTimes != true))
            {
                if (!IgnoreRequireTimeOnActivity)
                {
                    DateTime date = (DateTime)e.NewValue;
                    e.NewValue = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                }
            }
            base.FieldUpdating(sender, e);
        }

        protected virtual bool RequireTimeOnActivity(PXCache sender)
        {
            EPSetup setup = null;
            try
            {
                setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetupSelect<EPSetup>(sender.Graph).SelectSingle();
            }
            catch {/* SKIP */}
            return (setup != null ? setup.RequireTimes : null) ?? false;
        }
    }

    #endregion

    #region EPEndDayAttribute
    public sealed class EPEndDateAttribute : EPAllDaySupportDateTimeAttribute, IPXRowUpdatedSubscriber
    {
        private readonly Type ActivityClass;
        private readonly Type StartDate;
        private readonly Type TimeSpent;

        public EPEndDateAttribute(Type activityClass, Type startDate, Type timeSpent = null)
        {
            this.ActivityClass = activityClass;
            this.StartDate = startDate;
            this.TimeSpent = timeSpent;
            this.InputMask = "g";
            this.PreserveTime = true;
            this.WithoutDisplayNames = true;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            this.InputMask = this.DisplayMask = RequireTimeOnActivity(sender) ? "g" : "d";
            base.FieldSelecting(sender, e);
        }

        private bool RequireTimeOnActivity(PXCache sender)
        {
            EPSetup setup = null;
            try
            {
                setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetup<EPSetup>(sender.Graph).SelectSingle();
            }
            catch {/* SKIP */}
            return (setup != null ? setup.RequireTimes : null) ?? false;
        }

        #region IPXRowUpdatingSubscriber Members
        public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var side = (int?)sender.GetValue(e.Row, ActivityClass.Name);
            var startDate = (DateTime?)sender.GetValue(e.Row, StartDate.Name);
            var endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);
            var timeSpent = TimeSpent != null ? (int?)sender.GetValue(e.Row, TimeSpent.Name) : null;

            if (side != null && startDate != null)
            {
                DateTime? value = endDate;
                switch ((int)side)
                {
                    case CRActivityClass.Task:
                        if (startDate > endDate)
                            value = (DateTime)startDate;
                        break;
                    case CRActivityClass.Event:
                        if (startDate > endDate || endDate == null)
                            value = ((DateTime)startDate).AddMinutes(30);
                        else if (Object.Equals(sender.GetValue(e.OldRow, _FieldName), endDate))
                        {
                            var oldStartDate = (DateTime?)sender.GetValue(e.OldRow, StartDate.Name);
                            if (oldStartDate != null)
                                value = ((DateTime)endDate).AddTicks(((DateTime)startDate - (DateTime)oldStartDate).Ticks);
                        }
                        break;
                    default:
                        value = timeSpent != null ? (DateTime?)((DateTime)startDate).AddMinutes((int)timeSpent) : null;
                        break;
                }
                sender.SetValue(e.Row, _FieldName, value);
            }
        }
        #endregion
    }
    #endregion

    #region EPAllDayAttribute
    public sealed class EPAllDayAttribute : PXDBBoolAttribute, IPXRowUpdatedSubscriber, IPXRowSelectedSubscriber
    {
        private readonly Type StartDate;
        private readonly Type EndDate;
        public EPAllDayAttribute(Type startDate, Type endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        #region IPXRowUpdatedSubscriber Members
        public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            bool? oldAllDay = (bool?)sender.GetValue(e.OldRow, _FieldName);
            bool? allDay = (bool?)sender.GetValue(e.Row, _FieldName);
            DateTime? startDate = (DateTime?)sender.GetValue(e.Row, StartDate.Name);
            DateTime? endDate = (DateTime?)sender.GetValue(e.Row, EndDate.Name);
            DateTime? newStartDate = startDate;
            DateTime? newEndDate = endDate;
            if (allDay == true && startDate != null)
            {
                if (endDate == null || ((DateTime)endDate).Date < ((DateTime)startDate).Date)
                    newEndDate = ((DateTime)startDate).AddDays(1D);

                newStartDate = ((DateTime)startDate).Date;
                newEndDate = ((DateTime)newEndDate).Date;
            }

            if (allDay != true && allDay != oldAllDay)
            {
                newStartDate = ((DateTime)startDate).Date;
                newEndDate = ((DateTime)newEndDate).Date;
            }

            if (startDate != newStartDate || endDate != newEndDate)
            {
                sender.SetValue(e.Row, StartDate.Name, newStartDate);
                sender.SetValue(e.Row, EndDate.Name, newEndDate);
            }
        }
        #endregion

        #region IPXRowSelectedSubscriber Members

        public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            bool? allDay = (bool?)sender.GetValue(e.Row, _FieldName);
            PXFieldState startState = sender.GetStateExt(e.Row, typeof(CRActivity.startDate).Name + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX) as PXFieldState;
            PXFieldState endState = sender.GetStateExt(e.Row, typeof(CRActivity.endDate).Name + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX) as PXFieldState;
            if (startState != null)
                PXDBDateAndTimeAttribute.SetTimeEnabled<CRActivity.startDate>(sender, e.Row, startState.Enabled && allDay != true);
            if (endState != null)
                PXDBDateAndTimeAttribute.SetTimeEnabled<CRActivity.endDate>(sender, e.Row, endState.Enabled && allDay != true);
        }

        #endregion
    }
    #endregion

    #region PXInvitationStatusAttribute

    /// <summary>
    /// List of invitation statuses.
    /// </summary>
    /// <example>
    /// [PXInvitationStatus]
    /// </example>
    public class PXInvitationStatusAttribute : PXIntListAttribute
    {
        public const int NOTINVITED = 0;
        public const int INVITED = 1;
        public const int ACCEPTED = 2;
        public const int REJECTED = 3;
        public const int RESCHEDULED = 4;
        public const int CANCELED = 5;

        public PXInvitationStatusAttribute()
            : base(
                new[] { NOTINVITED, INVITED, ACCEPTED, REJECTED, RESCHEDULED, CANCELED },
                new[] { Messages.InvitationNotInvited,
                  Messages.InvitationInvited,
                  Messages.InvitationAccepted,
                  Messages.InvitationRejected,
                  Messages.InvitationRescheduled,
                  Messages.InvitationCanceled })
        {
        }
    }

    #endregion

    #region EPDependNoteList

    public class EPDependNoteList<Table, FRefNoteID, ParentTable> : PXSelect<Table>
        where Table : class, IBqlTable, new()
        where FRefNoteID : class, IBqlField
        where ParentTable : class, IBqlTable
    {
        protected PXView _History;

        public EPDependNoteList(PXGraph graph)
            : base(graph)
        {
            PXDBDefaultAttribute.SetSourceType<FRefNoteID>(graph.Caches[typeof(Table)], SourceNoteID);
            this.View = new PXView(graph, false, BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<,>), typeof(Table), ComposeWhere)));
            Init(graph);
        }
        public EPDependNoteList(PXGraph graph, Delegate handler)
            : base(graph, handler)
        {
            Init(graph);
        }

        protected Type SourceNoteID
        {
            get { return typeof(ParentTable).GetNestedType(EntityHelper.GetNoteField(typeof(ParentTable))); }
        }
        protected Type RefNoteID
        {
            get { return typeof(FRefNoteID); }
        }

        protected Type ComposeWhere
        {
            get { return BqlCommand.Compose(typeof(Where<,>), RefNoteID, typeof(Equal<>), typeof(Current<>), SourceNoteID); }
        }

        protected virtual void Init(PXGraph graph)
        {
            graph.RowInserted.AddHandler(BqlCommand.GetItemType(SourceNoteID), Source_RowInserted);
            graph.RowDeleted.AddHandler(BqlCommand.GetItemType(SourceNoteID), Source_RowDeleted);

            if (!graph.Views.Caches.Contains(typeof(Note)))
                graph.Views.Caches.Add(typeof(Note));

            PXCache source = graph.Caches[typeof(Table)];
            var viewName = $"EPDependNoteList_{graph.GetType().FullName}_{typeof(Table).FullName}_{typeof(ParentTable).FullName}_{typeof(FRefNoteID).FullName}_History";
            _History = CreateView(viewName, graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(Table),
                typeof(Where<,>), RefNoteID,
                typeof(Equal<>), typeof(Required<>), SourceNoteID));
        }

        protected virtual PXView CreateView(string viewName, PXGraph graph, Type command)
        {
            PXView view = new PXView(graph, false, BqlCommand.CreateInstance(command));
            graph.Views.Add(viewName, view);
            return view;
        }

        protected virtual void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Guid? NoteID = (Guid?)sender.GetValue(e.Row, SourceNoteID.Name);
            foreach (Table item in this._History.SelectMulti(NoteID))
                this.Cache.Delete(item);
        }

        protected virtual void Source_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            GetSourceNoteID(e.Row);
        }

        protected Guid? GetSourceNoteID(object source)
        {
            PXCache cache = this._Graph.Caches[source.GetType()];
            var noteCache = _Graph.Caches[typeof(Note)];
            var oldDirty = noteCache.IsDirty;
            var res = (Guid?)PXNoteAttribute.GetNoteID(cache, source, SourceNoteID.Name);
            noteCache.IsDirty = oldDirty;
            return res;
        }

        protected Guid? GetRefNoteID(object source)
        {
            PXCache cache = this._Graph.Caches[source.GetType()];
            return (Guid?)cache.GetValue(source, RefNoteID.Name);
        }
    }
    #endregion

    #region EPApprovalList

    public class ApprovalMap
    {
        public readonly int ID;
        public readonly int? NotificationID;

        public ApprovalMap(int assignmentMapID, int? notificationID)
        {
            ID = assignmentMapID;
            NotificationID = notificationID;
        }
    }

    [Serializable]
    public class RequestApproveException : PXException
    {
        public RequestApproveException() : base(Messages.Approve) { }

        public RequestApproveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class RequestRejectException : PXException
    {
        public RequestRejectException() : base(Messages.Reject) { }

        public RequestRejectException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ReasonRejectedException : PXException
    {
        public ReasonRejectedException() : base(Messages.Cancel) { }

        public ReasonRejectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

	public enum ApprovalResult {
		Approved,
		Rejected,
		PendingApproval,
		Submitted
	}

    public class EPApprovalList<SourceAssign, Approved, Rejected> : EPDependNoteList<EPApproval, EPApproval.refNoteID, SourceAssign>
        where SourceAssign : class, IAssign, IBqlTable, new()
        where Approved : class, IBqlField
        where Rejected : class, IBqlField
    {
        #region ReasonApproveRejectFilter
        [Serializable()]
        [PXHidden]
        public class ReasonApproveRejectFilter : IBqlTable
        {
            #region Reason
            public abstract class reason : IBqlField { }

            [PXString(IsUnicode = true)]
            [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Reason")]
            public virtual String Reason { get; set; }
            #endregion
        }
        #endregion

        #region State
        protected PXView _Pending;
        protected PXView _Find;
        protected PXView _FindOwner;
        protected PXView _FindWithWorkgroup;
        protected PXView _FindOwnerWithoutWorkgroup;
        protected PXView _ApprovedByOwnerOrApprover;
        protected PXView _ApprovedByWorkgroup;
        protected PXView _Rejected;
        protected PXView _Except;
        protected PXAction _Activity;
        protected PXFilter<ReasonApproveRejectFilter> _ReasonFilter;
		public bool SuppressApproval;
        #endregion

        #region Ctor
        public EPApprovalList(PXGraph graph, Delegate @delegate)
            : base(graph, @delegate)
        {
        }

        public EPApprovalList(PXGraph graph)
            : base(graph)
        {
        }

        protected override void Init(PXGraph graph)
        {
            Initialize(graph);
            base.Init(graph);
        }

        private void Initialize(PXGraph graph)
        {
            _Activity = graph.Actions["RegisterActivity"];
            var viewNamePrefix = $"EPApprovalList_{graph.GetType().FullName}_{typeof(SourceAssign).FullName}_{typeof(Approved).FullName}_{typeof(Rejected).FullName}";

            this.View = CreateView($"{viewNamePrefix}_View", graph, BqlCommand.Compose(
                typeof(Select2<,,>), typeof(EPApproval),
                typeof(LeftJoin<ApproverEmployee,
                        On<ApproverEmployee.userID, Equal<EPApproval.ownerID>>,
                    LeftJoin<ApprovedByEmployee,
                        On<ApprovedByEmployee.userID, Equal<EPApproval.approvedByID>>>>),
                typeof(Where<,,>),
                    typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Current<>), SourceNoteID,
                    typeof(And<EPApproval.isPreApproved, Equal<False>>)));

            _Pending = CreateView($"{viewNamePrefix}_Pending", graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(EPApproval),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.status, Equal<EPApprovalStatus.pending>>)));

            _Find = CreateView($"{viewNamePrefix}_Find", graph, BqlCommand.Compose(
                typeof(Select2<,,>), typeof(EPApproval),
                typeof(LeftJoin<EPRule,
                        On<EPRule.ruleID, Equal<EPApproval.ruleID>>>),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>,
                    And<EPApproval.workgroupID, Equal<Required<EPApproval.workgroupID>>,
                    And<EPApproval.ruleID, Equal<Required<EPApproval.ruleID>>>>>)));

            _FindOwner = CreateView($"{viewNamePrefix}_FindOwner", graph, BqlCommand.Compose(
                typeof(Select2<,,>), typeof(EPApproval),
                typeof(LeftJoin<EPRule,
                        On<EPRule.ruleID, Equal<EPApproval.ruleID>>>),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>,
                    And<EPApproval.ownerID, Equal<Required<EPApproval.ownerID>>,
                    And<EPApproval.ruleID, Equal<Required<EPApproval.ruleID>>>>>)));

            _FindWithWorkgroup = CreateView($"{viewNamePrefix}_FindWithWorkgroup", graph, BqlCommand.Compose(
                typeof(Select2<,,>), typeof(EPApproval),
                typeof(LeftJoin<EPRule,
                        On<EPRule.ruleID, Equal<EPApproval.ruleID>>>),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>,
                    And<EPApproval.workgroupID, Equal<Required<EPApproval.workgroupID>>>>)));

            _FindOwnerWithoutWorkgroup = CreateView($"{viewNamePrefix}_FindOwnerWithoutWorkgroup", graph, BqlCommand.Compose(
                typeof(Select2<,,>), typeof(EPApproval),
                typeof(LeftJoin<EPRule,
                        On<EPRule.ruleID, Equal<EPApproval.ruleID>>>),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>,
                    And<EPApproval.workgroupID, IsNull,
                    And<EPApproval.ownerID, Equal<Required<EPApproval.ownerID>>>>>)));

            _ApprovedByOwnerOrApprover = CreateView($"{viewNamePrefix}_ApprovedByOwner", graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(EPApproval),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.status, Equal<EPApprovalStatus.approved>, 
                    And<
						Where<EPApproval.ownerID, Equal<Required<EPApproval.ownerID>>, 
						Or<EPApproval.approvedByID, Equal<Required<EPApproval.ownerID>>>>>>)));

            _ApprovedByWorkgroup = CreateView($"{viewNamePrefix}_ApprovedByOwner", graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(EPApproval),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.status, Equal<EPApprovalStatus.approved>, 
                    And<EPApproval.workgroupID, Equal<Required<EPApproval.workgroupID>>>>)));

            _Rejected = CreateView($"{viewNamePrefix}_Rejected", graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(EPApproval),
                typeof(Where<,,>),
                typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.status, Equal<EPApprovalStatus.rejected>>)));

            _Except = CreateView($"{viewNamePrefix}_Except", graph, BqlCommand.Compose(
                typeof(Select<,>), typeof(EPApproval),
                typeof(Where<,,>), typeof(EPApproval.refNoteID),
                typeof(Equal<>), typeof(Required<>), SourceNoteID,
                typeof(And<EPApproval.ruleID, NotEqual<Required<EPApproval.ruleID>>,
                And<EPApproval.status, NotEqual<EPApprovalStatus.approved>>>)));

            graph.RowPersisted.AddHandler<SourceAssign>(OnPersisted);

            _ReasonFilter = new PXFilter<ReasonApproveRejectFilter>(graph);
            graph.ViewNames[_ReasonFilter.View] = "ReasonApproveRejectParams";
            graph.Views.Add("ReasonApproveRejectParams", _ReasonFilter.View);
        }


        #endregion

        #region Implementation

        public void Assign(SourceAssign source, IEnumerable<ApprovalMap> maps)
        {
            if (source == null) return;

            Reset(source);

			bool pendingApprove = false, pendingReject = false;
			if (!SuppressApproval)
			{
            foreach (ApprovalMap map in maps)
            {
                var am = PXSelectReadonly<
                       EPAssignmentMap,
                   Where<
								EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>
						.SelectSingleBound(_Graph, null, map.ID);

				try
				{
					DoMapReExecution(source, null, am, map.NotificationID, true);
				}           
				catch (RequestApproveException)
				{
					pendingApprove = true;
				}
				catch (RequestRejectException)
				{
					pendingReject = true;
				}
			}
			}

			if (Cache.Inserted.Count() == 0)
			{
				if (pendingReject)
				{
					_Graph.Caches[source.GetType()].SetValue<Approved>(source, false);
					_Graph.Caches[source.GetType()].SetValue<Rejected>(source, true);
				}
				else if (pendingApprove || SuppressApproval)
				{
					_Graph.Caches[source.GetType()].SetValue<Approved>(source, true);
					_Graph.Caches[source.GetType()].SetValue<Rejected>(source, false);
				}
			}
		}

        public void Assign(SourceAssign source, int? assignmentMapID, int? notification)
        {
            Reset(source);

            var map = PXSelectReadonly<
                   EPAssignmentMap,
               Where<
                   EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>.
               SelectSingleBound(_Graph, null, assignmentMapID);

            DoMapReExecution(source, null, map, notification);
        }

        public virtual void Reset(SourceAssign source)
        {
            foreach (EPApproval item in this._History.SelectMulti(GetSourceNoteID(source)))
                this.Cache.Delete(item);
        }

        public virtual bool Approve(SourceAssign source)
        {
            EPApproval item = (EPApproval)this._Rejected.SelectSingle(GetSourceNoteID(source));

            if (item != null)
                throw new PXException(Messages.CannotApproveRejectedItem);

            if (UpdateApproval(source, EPApprovalStatus.Approved))
            {
                RegisterActivity(source, PO.Messages.Approved);

                return true;
            }

            return false;
        }

        public virtual bool Reject(SourceAssign source)
        {
            if (UpdateApproval(source, EPApprovalStatus.Rejected))
            {
                RegisterActivity(source, PO.Messages.Rejected);

                return true;
            }

            return false;
        }

        public virtual void ClearPendingApproval(SourceAssign source)
        {
            foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
            {
                this.Cache.Delete(item);
            }
        }

        public virtual bool IsApprover(SourceAssign source)
        {
            bool result = true;

            foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
            {
                result = false;

                if (ValidateAccess(item.WorkgroupID, item.OwnerID))
                    return true;
            }

            return result;
        }
		
        public virtual bool IsApproved(SourceAssign source)
        {
			this._Pending.Cache.ClearQueryCacheObsolete();
			this._Pending.Clear();

            var sourceNoteId = GetSourceNoteID(source);
            EPApproval item = (EPApproval)this._Pending.SelectSingle(sourceNoteId);
            if (item != null) return false;

            item = (EPApproval)this._Rejected.SelectSingle(sourceNoteId);
            if (item != null) return false;

            return true;
        }

        public virtual bool IsRejected(SourceAssign source)
        {
            this._Rejected.Cache.ClearQueryCacheObsolete();
			this._Rejected.Clear();

            var sourceNoteId = GetSourceNoteID(source);
            EPApproval item = (EPApproval)this._Rejected.SelectSingle(sourceNoteId);
            if (item != null) return true;

            return false;
        }

		public virtual ApprovalResult GetResult(SourceAssign source)
		{
			var cache = _Graph.Caches[typeof(SourceAssign)];

			bool isApproved = (bool?)cache.GetValue<Approved>(source) == true;
			bool isRejected = (bool?)cache.GetValue<Rejected>(source) == true;

			if (isApproved)
				return ApprovalResult.Approved;
			
			if (isRejected)
				return ApprovalResult.Rejected;
			
			return ApprovalResult.PendingApproval;
		}

        private bool UpdateApproval(SourceAssign source, string status)
        {
            bool result = false;
            var stillPending = new List<EPApproval>();

            foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
            {
                if (ValidateAccess(item.WorkgroupID, item.OwnerID))
                {
                    EPRule rule = PXSelectReadonly<
                         EPRule,
                     Where<
                         EPRule.ruleID, Equal<Required<EPApproval.ruleID>>>>
                    .SelectSingleBound(_Graph, null, item.RuleID);

                    if (rule != null)
                    {
                        string reasonSettings = status.Equals(EPApprovalStatus.Approved) 
                            ? rule.ReasonForApprove 
                            : rule.ReasonForReject;

                        if (!reasonSettings.Equals(EPReasonSettings.NotRequired))
                        {
                            if (reasonSettings.Equals(EPReasonSettings.Required))
                            {
                                if (this._Graph.UnattendedMode || this._Graph.IsMobile)
                                    throw new PXException(Messages.RequireCommentForApproveReject);
                            }
                            if (!this._Graph.UnattendedMode)
                            {
                                PXCache cache = _ReasonFilter.Cache;
                                var current = _ReasonFilter.Current;

                                if (_ReasonFilter != null && current != null)
                                {
                                    if (_ReasonFilter.View.Answer == WebDialogResult.None || _ReasonFilter.View.Answer == WebDialogResult.No)
                                    {
                                        _ReasonFilter.Reset();
                                    }

                                    PXDefaultAttribute attr = cache?.GetAttributes<ReasonApproveRejectFilter.reason>().OfType<PXDefaultAttribute>().FirstOrDefault();
                                    if (attr != null)
                                    {
                                        attr.PersistingCheck = reasonSettings.Equals(EPReasonSettings.Required) 
                                            ? PXPersistingCheck.Null 
                                            : PXPersistingCheck.Nothing;

                                        if (attr.PersistingCheck == PXPersistingCheck.Null && string.IsNullOrWhiteSpace(current.Reason))
                                        {
                                            PXUIFieldAttribute.SetError(cache, current, nameof(ReasonApproveRejectFilter.Reason),
                                                $"The approval rule {rule.Name} requires that you enter a comment to complete this action for the selected document.");
                                        }
                                        else
                                        {
                                            PXUIFieldAttribute.SetError(cache, current, nameof(ReasonApproveRejectFilter.Reason), null, null, false, PXErrorLevel.Undefined);
                                        }

                                        if (_ReasonFilter.AskExtFullyValid((graph, viewName) => { }, DialogAnswerType.Positive))
                                        {
                                            item.Reason = current.Reason;
                                            _ReasonFilter.Reset();
                                        }
                                        else
                                        {
                                            throw new ReasonRejectedException();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    item.ApprovedByID = PXAccess.GetUserID();
                    item.ApproveDate = PXTimeZoneInfo.Now;
                    item.Status = status;
                    if (this.Cache.Update(item) != null)
                    {
                        result = true;

                        if (status == EPApprovalStatus.Approved)
                            OnApprovalApproved(source, item, item.AssignmentMapID, item.NotificationID);
                    }
                }

                if (item.Status == EPApprovalStatus.Pending)
                    stillPending.Add(item);
            }

            if (result && status == EPApprovalStatus.Rejected)
            {
                stillPending.ForEach(item => this.Cache.Delete(item));
				
				_Graph.Caches[source.GetType()].SetValue<Approved>(source, false);
				_Graph.Caches[source.GetType()].SetValue<Rejected>(source, true);
            }

            return result;
        }

        protected void OnApprovalApproved(SourceAssign source, EPApproval approval, int? assignmentMapID, int? notification)
        {
            EPRule rule = PXSelectReadonly<
                 EPRule,
             Where<
                 EPRule.ruleID, Equal<Required<EPApproval.ruleID>>>>.
             SelectSingleBound(_Graph, null, approval.RuleID);

            switch (rule?.ApproveType ?? EPApproveType.Wait)
            {
                case EPApproveType.Approve:     // Entire Doc

                    bool isApprovalLastInRule = PXSelectJoin<
                                EPApproval,
                            InnerJoin<EPRule,
                                On<EPRule.ruleID, Equal<EPApproval.ruleID>,
                                And<EPApproval.status, NotEqual<EPApprovalStatus.approved>,
                                And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>,
                                And<EPApproval.ruleID, Equal<Required<EPRule.ruleID>>>>>>>>.
                            SelectSingleBound(_Graph, null, GetSourceNoteID(source), approval.RuleID).Count == 0;

                    if (isApprovalLastInRule)
                    {
                        _Except.SelectMulti(GetSourceNoteID(source), rule.RuleID).ForEach(item => Cache.Delete(item));

						_Graph.Caches[source.GetType()].SetValue<Approved>(source, true);
						_Graph.Caches[source.GetType()].SetValue<Rejected>(source, false);
                    }

                    break;

                case EPApproveType.Complete:    // Step

                    EPAssignmentMap map = PXSelectReadonly<
                            EPAssignmentMap,
                        Where<
                            EPAssignmentMap.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>>>.
                        SelectSingleBound(_Graph, null, assignmentMapID);

                    isApprovalLastInRule = PXSelectJoin<
                                EPApproval,
                            InnerJoin<EPRule,
                                On<EPRule.ruleID, Equal<EPApproval.ruleID>,
                                And<EPApproval.status, NotEqual<EPApprovalStatus.approved>,
                                And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>,
                                And<EPApproval.ruleID, Equal<Required<EPRule.ruleID>>>>>>>>.
                            SelectSingleBound(_Graph, null, GetSourceNoteID(source), approval.RuleID).Count == 0;

                    if (isApprovalLastInRule)
                    {
                        _Except.SelectMulti(GetSourceNoteID(source), rule.RuleID).ForEach(item => Cache.Delete(item));
                    }

                    DoMapReExecution(source, approval, map, notification);

                    break;

                case EPApproveType.Wait:        // For others
                default:

                    map = PXSelectReadonly<
                            EPAssignmentMap,
                        Where<
                            EPAssignmentMap.assignmentMapID, Equal<Required<EPApproval.assignmentMapID>>>>.
                        SelectSingleBound(_Graph, null, assignmentMapID);

                    DoMapReExecution(source, approval, map, notification);

                    break;
            }
        }

        protected bool OnApprovalApprovedWithoutReExecution(SourceAssign source, EPApproval approval, int? assignmentMapID, int? notification)
        {
            EPRule rule = PXSelectReadonly<
                 EPRule,
             Where<
                 EPRule.ruleID, Equal<Required<EPApproval.ruleID>>>>.
             SelectSingleBound(_Graph, null, approval.RuleID);

            bool isApprovalLastInRule = PXSelectJoin<
                        EPApproval,
                    InnerJoin<EPRule,
                        On<EPRule.ruleID, Equal<EPApproval.ruleID>,
                        And<EPApproval.status, NotEqual<EPApprovalStatus.approved>,
                        And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>,
                        And<EPApproval.ruleID, Equal<Required<EPRule.ruleID>>>>>>>>.
                    SelectSingleBound(_Graph, null, GetSourceNoteID(source), approval.RuleID).Count == 0;

            switch (rule?.ApproveType ?? EPApproveType.Wait)
            {
                case EPApproveType.Approve:     // Entire Doc

                    if (isApprovalLastInRule)
                    {
                        _Except.SelectMulti(GetSourceNoteID(source), rule.RuleID).ForEach(item => Cache.Delete(item));

                        _Graph.Caches[source.GetType()].SetValue<Approved>(source, true);
                        _Graph.Caches[source.GetType()].SetValue<Rejected>(source, false);
                    }

                    return false;

                case EPApproveType.Complete:    // Step

                    if (isApprovalLastInRule)
                    {
                        _Except.SelectMulti(GetSourceNoteID(source), rule.RuleID).ForEach(item => Cache.Delete(item));
                    }

                    return true;

                case EPApproveType.Wait:        // For others
                default:

                    return true;
            }
        }

        protected void DoMapReExecution(SourceAssign source, EPApproval approved, EPAssignmentMap map, int? notification, bool isMultimap = false)
        {
            try
            {
                int i = 0;
                while (MapReExecutionInternal(source, approved, map, notification, i++))
                {
                }

                if (IsApproved(source))
                    throw new RequestApproveException();
            }
            catch (RequestApproveException)
			{
				if (isMultimap)
					throw;

                _Graph.Caches[source.GetType()].SetValue<Approved>(source, true);
                _Graph.Caches[source.GetType()].SetValue<Rejected>(source, false);
            }
            catch (RequestRejectException)
            {
				if (isMultimap)
					throw;

                _Graph.Caches[source.GetType()].SetValue<Approved>(source, false);
                _Graph.Caches[source.GetType()].SetValue<Rejected>(source, true);
            }
        }

        protected bool MapReExecutionInternal(SourceAssign source, EPApproval approved, EPAssignmentMap map, int? notification, int currentStepSequence)
        {
            bool goNextStep = true;
			EPRule step = null;

			if (map != null)
			{
				step = PXSelect<
						EPRule,
					Where<EPRule.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
						And<EPRule.sequence, Greater<Required<EPRule.sequence>>,
						And<EPRule.isActive, Equal<boolTrue>,
						And<EPRule.stepID, IsNull>>>>,
					OrderBy<
						Asc<EPRule.sequence>>>
					.SelectSingleBound(_Graph, null, map.AssignmentMapID, currentStepSequence);

	            if (step?.ExecuteStep == EPExecuteStep.IfNoApproversFoundatPreviousSteps)
	            {
					bool wasAnyApprovalsInDoc = PXSelect<
							EPApproval,
						Where<
							EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>
						.SelectSingleBound(_Graph, null, GetSourceNoteID(source)).Count != 0;

	                if (wasAnyApprovalsInDoc)
	                   return goNextStep;
	            }
			}
            using (new CRAssigmentScope(source))
            {
                if (map?.MapType == EPMapType.Legacy)
                {
                    source.WorkgroupID = approved?.WorkgroupID;
                    source.OwnerID = approved?.OwnerID;
                }

                foreach (var approveInfo in GetApproversFromNextStep(source, map, currentStepSequence))
                {
                    PXResult<EPApproval, EPRule> existing = null;

                    if (map != null)
                    {
                        if (map.MapType == EPMapType.Legacy)
                        {
                            if (approveInfo.WorkgroupID == null)
                            {
                                existing = (PXResult<EPApproval, EPRule>)_FindOwnerWithoutWorkgroup.SelectSingle(GetSourceNoteID(source), map.AssignmentMapID, approveInfo.OwnerID);
                            }
                            else
                            {
                                existing = (PXResult<EPApproval, EPRule>)_FindWithWorkgroup.SelectSingle(GetSourceNoteID(source), map.AssignmentMapID, approveInfo.WorkgroupID);
                            }
                        }
                        else if (approveInfo.WorkgroupID == null)
                        {
                            existing = (PXResult<EPApproval, EPRule>)_FindOwner.SelectSingle(GetSourceNoteID(source), map.AssignmentMapID, approveInfo.OwnerID, approveInfo.RuleID);
                        }
                        else
                        {
                            existing = (PXResult<EPApproval, EPRule>)_Find.SelectSingle(GetSourceNoteID(source), map.AssignmentMapID, approveInfo.WorkgroupID, approveInfo.RuleID);
                        }
                    }
                    else if (approved != null)
                    {
                        break;      // Simple Approve
                    }

                    EPApproval existingApproval = existing;
                    EPRule existingRule = existing;

                    if (existingApproval == null)
                    {
                        var item = new EPApproval
                        {
                            RefNoteID = GetSourceNoteID(source),
                            WorkgroupID = approveInfo.WorkgroupID,
                            OwnerID = approveInfo.OwnerID,
                            RuleID = approveInfo.RuleID,
                            StepID = approveInfo.StepID,
                            WaitTime = approveInfo.WaitTime,
                            Status = EPApprovalStatus.Pending,
                            AssignmentMapID = map?.AssignmentMapID,
                            NotificationID = notification
                        };

                        var alreadyApprovedApproval =
                            (item.OwnerID != null)
                                ? _ApprovedByOwnerOrApprover.SelectSingle(item.RefNoteID, item.OwnerID, item.OwnerID) as EPApproval
                                : null
                            ?? (item.WorkgroupID != null
								? _ApprovedByWorkgroup.SelectSingle(item.RefNoteID, item.WorkgroupID) as EPApproval
								: null);

                        if (alreadyApprovedApproval != null)
                        {
                            item.ApprovedByID = alreadyApprovedApproval.ApprovedByID;
                            item.ApproveDate = alreadyApprovedApproval.ApproveDate;
                            item.Status = EPApprovalStatus.Approved;
                            item.IsPreApproved = true;

                            goNextStep = OnApprovalApprovedWithoutReExecution(source, item, item.AssignmentMapID, item.NotificationID);
                        }
                        else
                        {
                            goNextStep = false;
                        }

                        item = (EPApproval)Cache.Insert(item);

                        if (item != null && alreadyApprovedApproval == null)
                        {
                            PXFieldState state = Cache.GetStateExt<EPApproval.ownerID>(item) as PXFieldState;

                            PXTrace.WriteInformation(Messages.TraceAssign, state?.Value ?? item.OwnerID, item.WorkgroupID);

                            if (approved != null)
                            {
                                if (ValidateAccess(item.WorkgroupID, item.OwnerID))
                                {
                                    item.ApprovedByID = PXAccess.GetUserID();
                                    item.ApproveDate = PXTimeZoneInfo.Now;
                                    item.Status = EPApprovalStatus.Approved;
                                    if (Cache.Update(item) != null)
                                        goNextStep = OnApprovalApprovedWithoutReExecution(source, item, item.AssignmentMapID, item.NotificationID);
                                }
                            }
                        }

                        if (map?.MapType == EPMapType.Legacy)
                        {
                            return false;
                        }
                    }
                    else if (existingApproval.Status == EPApprovalStatus.Approved && existingRule.ApproveType == EPApproveType.Complete)
                    {
                        bool isApprovalLastInRule = PXSelectJoin<
                                    EPApproval,
                                InnerJoin<EPRule,
                                    On<EPRule.ruleID, Equal<EPApproval.ruleID>,
                                    And<EPApproval.status, NotEqual<EPApprovalStatus.approved>,
                                    And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>,
                                    And<EPApproval.ruleID, Equal<Required<EPRule.ruleID>>>>>>>>.
                                SelectSingleBound(_Graph, null, GetSourceNoteID(source), existingApproval.RuleID).Count == 0;

                        if (isApprovalLastInRule)
                        {
                            _Except.SelectMulti(GetSourceNoteID(source), existingApproval.RuleID).ForEach(item => Cache.Delete(item));

                            return true;
                        }
                    }
                    else if (approved != null)
                    {
                        bool isApprovalLastInStep = PXSelect<
                                EPApproval,
                            Where<
                                EPApproval.status, NotEqual<EPApprovalStatus.approved>,
                                And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>,
								And<EPApproval.stepID, Equal<Required<EPApproval.stepID>>>>>>.
                            SelectSingleBound(_Graph, null, GetSourceNoteID(source), approveInfo.StepID).Count == 0;

                        goNextStep = isApprovalLastInStep;
                    }
                }
            }

            if (map == null || step == null) return false;

            bool wasAnyApprovalsInDocByStep = PXSelectJoin<
					EPApproval,
                InnerJoin<EPRule,
                    On<EPApproval.ruleID, Equal<EPRule.ruleID>,
                    And<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>>,
                Where<
                    EPRule.stepID, Equal<Required<EPRule.stepID>>>>.
                SelectSingleBound(_Graph, null, GetSourceNoteID(source), step.RuleID).Count != 0;

            bool isAnyNewApprovals = Cache.Inserted.Count() != 0;

            if (!wasAnyApprovalsInDocByStep && !isAnyNewApprovals)
            {
                switch (step.EmptyStepType)
                {
                    case EPEmptyStepType.Approve:
                        throw new RequestApproveException();

                    case EPEmptyStepType.Reject:
                        throw new RequestRejectException();

                    default:
                        goNextStep = true;
                        break;
                }
            }

            return goNextStep;
        }

        public bool ValidateAccess(SourceAssign source)
        {
            foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
            {
                if (ValidateAccess(item.WorkgroupID, item.OwnerID))
                    return true;
            }
            return false;
        }

        protected virtual IEnumerable<ApproveInfo> GetApproversFromNextStep(SourceAssign source, EPAssignmentMap map, int? currentStepSequence)
        {
            if (map == null) yield break;

            var processor = new EPAssignmentProcessor<SourceAssign>(_Graph);

            foreach (var approveInfo in processor.Approve(source, map, currentStepSequence))
            {
                yield return approveInfo;
            }
        }

        public virtual bool ValidateAccess(int? workgroup, Guid? ownerID)
        {
            if (workgroup == null && ownerID == null) return true;

            if (PXAccess.GetUserID() == ownerID) return true;

            EPCompanyTree wg =
                PXSelect<
                    EPCompanyTree,
                Where<
                    EPCompanyTree.workGroupID, Equal<Required<EPCompanyTree.workGroupID>>,
                    And<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>>.SelectWindowed(this._Graph, 0, 1, workgroup);
            return wg != null;
        }

        protected void RegisterActivity(object item, string summary)
        {
            if (_Activity != null)
            {
                PXAdapter adapter = new PXAdapter(new PXView.Dummy(this._Graph, new Select<SourceAssign>(), new List<object> { item }));
                adapter.Parameters = new object[] { summary };
                foreach (object r in _Activity.Press(adapter))
                {
                }
            }
        }

        protected virtual void OnPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.TranStatus == PXTranStatus.Completed && e.Operation != PXDBOperation.Delete)
            {
                var distinct = this._Pending.Cache.Inserted.Cast<EPApproval>().GroupBy(_ => _.OwnerID).Select(_ => _.First());

                foreach (EPApproval approval in distinct)
                {
                    if (approval.NotificationID != null && approval.Status == EPApprovalStatus.Pending /*&& approval.OwnerID != PXAccess.GetUserID()*/)
                    {
                        try
                        {
                            this._Pending.Cache.Current = approval;
                            var sender = TemplateNotificationGenerator.Create(this._Graph, e.Row, approval.NotificationID.Value);
                            sender.LinkToEntity = false;
                            sender.MassProcessMode = false;
                            sender.Send();
                        }
                        catch (Exception ex)
                        {
                            this._Pending.Cache.RaiseExceptionHandling<EPApproval.status>(approval, approval.Status,
                                new PXSetPropertyException(Messages.ApprovalNotificationError, PXErrorLevel.Warning, ex.Message));
                        }
                    }
                }

            }
        }
        #endregion
    }

    public sealed class ApproveInfo
    {
        public Guid? OwnerID;
        public int? WorkgroupID;
        public Guid? RuleID;
        public Guid? StepID;
        public int? WaitTime;
    }
    #endregion
	
	#region EPApprovalActionExtensionPersistent
	
	public class EPApprovalActionExtensionPersistent<SourceAssign, Approved, Rejected, ApprovalMapID, ApprovalNotificationID> 
		: EPApprovalActionExtension<SourceAssign, Approved, Rejected, ApprovalMapID, ApprovalNotificationID>
		where SourceAssign : class, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where ApprovalMapID : class, IBqlField
		where ApprovalNotificationID : class, IBqlField
	{
		protected override bool Persistent => true;

		#region Ctor
		public EPApprovalActionExtensionPersistent(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
		}

		public EPApprovalActionExtensionPersistent(PXGraph graph)
			: base(graph)
		{
		}
		#endregion
	}

	#endregion
	
	#region EPApprovalActionExtension

	public class EPApprovalActionExtension<SourceAssign, Approved, Rejected, ApprovalMapID, ApprovalNotificationID> 
		: EPApprovalAction<SourceAssign, Approved, Rejected>
		where SourceAssign : class, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where ApprovalMapID : class, IBqlField
		where ApprovalNotificationID : class, IBqlField
	{
		#region Ctor
		public EPApprovalActionExtension(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
		}

		public EPApprovalActionExtension(PXGraph graph)
			: base(graph)
		{
		}
		protected override void Initialize(PXGraph graph)
		{
			base.Initialize(graph);
			AddAction(graph, nameof(Submit), Submit);

			graph.RowSelected.AddHandler<SourceAssign>(RowSelected);
		}
		#endregion
		
		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			int? mapID;
			int? notificationID;

			var isApprovalConfigured = GetConfiguration(out mapID, out notificationID);
			
			_Graph.Actions[nameof(Approve)].SetVisible(isApprovalConfigured);
			_Graph.Actions[nameof(Reject)].SetVisible(isApprovalConfigured);
		}

		[PXUIField(DisplayName = Messages.Submit)]
		[PXButton]
		public virtual IEnumerable Submit(PXAdapter adapter)
		{
			foreach (var item in adapter.Get<SourceAssign>())
			{
				DoSubmit(item);
				
				StatusHandler?.Invoke(item);

				_Graph.Caches[typeof(SourceAssign)].Update(item);

				if (Persistent)
					_Graph.Persist();

				yield return item;
			}
		}

		protected void DoSubmit(SourceAssign item)
		{
			int? mapID;
			int? notificationID;

			if (GetConfiguration(out mapID, out notificationID))
			{
				Assign(item, mapID, notificationID);
			}
		}

		public override ApprovalResult GetResult(SourceAssign source)
		{
			var cache = _Graph.Caches[typeof(SourceAssign)];

			bool isApproved = (bool?)cache.GetValue<Approved>(source) == true;
			bool isRejected = (bool?)cache.GetValue<Rejected>(source) == true;

			if (isApproved)
				return ApprovalResult.Approved;
			
			if (isRejected)
				return ApprovalResult.Rejected;
			
			int? mapID;
			int? notificationID;

			if (GetConfiguration(out mapID, out notificationID))
				return ApprovalResult.PendingApproval;

			return ApprovalResult.Submitted;
		}

		protected bool GetConfiguration(out int? mapID, out int? notificationID)
		{
			mapID = null;
			notificationID = null;

			var setupCache = _Graph.Caches[typeof(ApprovalMapID).DeclaringType];
			var setup = setupCache?.Current;
			if (setup == null)
				return false;

			mapID = setupCache.GetValue<ApprovalMapID>(setup) as int?;
			notificationID = setupCache.GetValue<ApprovalNotificationID>(setup) as int?;
			if (mapID == null)
				return false;

			return true;
		}
	}

	#endregion

	#region EPApprovalAction
    public class EPApprovalAction<SourceAssign, Approved, Rejected> : EPApprovalList<SourceAssign, Approved, Rejected>
        where SourceAssign : class, IAssign, IBqlTable, new()
        where Approved : class, IBqlField
        where Rejected : class, IBqlField
    {
		public delegate void StatusDelegate(SourceAssign item);
		public virtual StatusDelegate StatusHandler { get; set; }

		protected virtual bool Persistent => false;

        #region Ctor
        public EPApprovalAction(PXGraph graph, Delegate @delegate)
            : base(graph, @delegate)
        {
            Initialize(graph);
        }

        public EPApprovalAction(PXGraph graph)
            : base(graph)
        {
            Initialize(graph);
        }
        protected virtual void Initialize(PXGraph graph)
        {
            AddAction(graph, nameof(Approve), Approve);
            AddAction(graph, nameof(Reject), Reject);
        }
        #endregion
		
		[Obsolete("Will be removed in 2018R1 version")]
        public virtual void SetEnabledActions(PXCache sender, SourceAssign row, bool enable)
        {
            if (enable && !IsApprover(row))
                enable = false;

            sender.Graph.Actions[nameof(Approve)].SetEnabled(enable);
            sender.Graph.Actions[nameof(Reject)].SetEnabled(enable);
        }
        protected virtual void AddAction(PXGraph graph, string name, PXButtonDelegate handler)
        {
            graph.Actions[name] =
                (PXAction)Activator.CreateInstance(
                typeof(PXNamedAction<>).MakeGenericType(
                new Type[] { BqlCommand.GetItemType(SourceNoteID) }),
                new object[] { graph, name, handler }
                );
        }

        [PXUIField(DisplayName = Messages.Approve)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Complete)]
        public virtual IEnumerable Approve(PXAdapter adapter)
        {
            foreach (SourceAssign item in adapter.Get<SourceAssign>())
            {
                try
                {
                    if (!Approve(item))
                        throw new PXSetPropertyException(Common.Messages.NotApprover);

                    StatusHandler?.Invoke(item);

                    _Graph.Caches[typeof(SourceAssign)].Update(item);

                    if (Persistent)
                        _Graph.Persist();
                }
                catch (ReasonRejectedException) { }

                yield return item;
            }
        }

        [PXUIField(DisplayName = Messages.Reject)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
        public virtual IEnumerable Reject(PXAdapter adapter)
        {
            foreach (SourceAssign item in adapter.Get<SourceAssign>())
            {
                try
                {
                    if (!Reject(item))
                        throw new PXSetPropertyException(Common.Messages.NotApprover);

                    StatusHandler?.Invoke(item);

                    _Graph.Caches[typeof(SourceAssign)].Update(item);

                    if (Persistent)
                        _Graph.Persist();
                }
                catch (ReasonRejectedException) { }

                yield return item;
            }
        }
    }
    #endregion

    #region EPApprovalAutomation

    public interface IAssignedMap
    {
        int? AssignmentMapID { get; set; }
        int? AssignmentNotificationID { get; set; }
        Boolean? IsActive { get; }
    }

    public class EPApprovalAutomation<SourceAssign, Approved, Rejected, Hold, SetupApproval> : EPApprovalList<SourceAssign, Approved, Rejected>
        where SourceAssign : class, IAssign, IBqlTable, new()
        where Approved : class, IBqlField
        where Rejected : class, IBqlField
        where Hold : class, IBqlField
        where SetupApproval : class, IBqlTable, new()
    {
        #region Ctor
        public EPApprovalAutomation(PXGraph graph, Delegate @delegate)
            : base(graph, @delegate)
        {
            Initialize(graph);
        }

        public EPApprovalAutomation(PXGraph graph)
            : base(graph)
        {
            Initialize(graph);
        }

        private void Initialize(PXGraph graph)
        {
            graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(typeof(Approved)), typeof(Approved).Name, Approved_FieldVerifying);
            graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(typeof(Rejected)), typeof(Rejected).Name, Rejected_FieldVerifying);
            graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(typeof(Approved)), typeof(Approved).Name, Approved_FieldUpdated);
            graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(typeof(Rejected)), typeof(Rejected).Name, Rejected_FieldUpdated);
            graph.FieldDefaulting.AddHandler(BqlCommand.GetItemType(typeof(Hold)), typeof(Hold).Name, Hold_FieldDefaulting);
            graph.FieldDefaulting.AddHandler(BqlCommand.GetItemType(typeof(Approved)), typeof(Approved).Name, Approved_FieldDefaulting);
            graph.Initialized += InitLastEvents;
        }

        private void InitLastEvents(PXGraph graph)
        {
            graph.RowUpdated.AddHandler<SourceAssign>(RowUpdated);
        }
        #endregion
        #region Implementation
        public virtual List<ApprovalMap> GetAssignedMaps(SourceAssign doc, PXCache cache)
        {
			if (!PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
			{
				return new List<ApprovalMap>();
			}

            PXResultset<SetupApproval> setups = PXSetup<SetupApproval>.SelectMultiBound(cache.Graph, new object[] { doc });

            int count = setups.Count;
            var list = new List<ApprovalMap>();
            for (int i = 0; i < count; i++)
            {
                SetupApproval setup = (SetupApproval)setups[i];
                IAssignedMap map = (IAssignedMap)setup;
                if (map.IsActive == true && map.AssignmentMapID != null)
                {
                    list.Add(new ApprovalMap(map.AssignmentMapID.Value, map.AssignmentNotificationID));
                }
            }
            return list;
        }
        protected virtual void Approved_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((bool?)e.NewValue == true && !this.IsApprover((SourceAssign)e.Row))
            {
                if (sender
                    .GetAttributesReadonly<Approved>(e.Row)
                    .OfType<PXUIFieldAttribute>()
                    .Any(attribute => attribute.Visible))
                {
                    PXUIFieldAttribute.SetError<Approved>(sender, e.Row, Common.Messages.NotApprover);
                }

                throw new PXSetPropertyException(Common.Messages.NotApprover);
            }
        }
        protected virtual void Rejected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((bool?)e.NewValue == true && !this.IsApprover((SourceAssign)e.Row))
            {
                e.NewValue = false;

                if (sender
                    .GetAttributesReadonly<Approved>(e.Row)
                    .OfType<PXUIFieldAttribute>()
                    .Any(attribute => attribute.Visible))
                {
                    PXUIFieldAttribute.SetError<Approved>(sender, e.Row, Common.Messages.NotApprover);
                }

                throw new PXSetPropertyException(Common.Messages.NotApprover);
            }
        }
        protected virtual void Approved_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SourceAssign doc = (SourceAssign)e.Row;
            if (e.Row != null &&
            (bool?)(cache.GetValue<Approved>(doc)) == true && (bool?)e.OldValue != true)
            {
                cache.SetValue<Approved>(doc, false);
                try
                {
                    if (this.Approve(doc))
                    {
                        cache.SetValue<Approved>(doc, this.IsApproved(doc));
                    }
                }
                catch (ReasonRejectedException) { }
            }
        }
        protected virtual void Rejected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SourceAssign doc = (SourceAssign)e.Row;
            if (e.Row != null &&
                (bool?)(cache.GetValue<Rejected>(doc)) == true && (bool?)e.OldValue != true)
            {
                cache.SetValue<Rejected>(doc, false);
                try
                {
                    if (this.Reject(doc))
                    {
                        cache.SetValue<Rejected>(doc, this.IsRejected(doc));
                    }
                }
                catch (ReasonRejectedException) { }
            }
        }
        protected virtual void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            SourceAssign doc = (SourceAssign)e.Row;
            SourceAssign olddoc = (SourceAssign)e.OldRow;
            if (olddoc == null || doc == null)
                return;
            bool? approved = (bool?)cache.GetValue<Approved>(doc);
            bool? hold = (bool?)cache.GetValue<Hold>(doc);
            bool? oldValue = (bool?)cache.GetValue<Hold>(olddoc);

            if (oldValue != null)
            {
                if (hold == true)
                {
                    if (oldValue == false)
                    {
                        this.ClearPendingApproval(doc);
                        cache.SetDefaultExt<Approved>(doc);
                        cache.SetDefaultExt<Rejected>(doc);
                    }
                }
                else if (oldValue == true && approved != true)
                {
                    List<ApprovalMap> maps = GetAssignedMaps(doc, cache);
                    if (maps.Any())
                    {
                        Assign(doc, maps);
                    }
                    else
                    {
                        cache.SetValue<Approved>(doc, true);
                        cache.SetValue<Rejected>(doc, false);
                    }
                }
            }
        }
        protected virtual void Hold_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SourceAssign doc = (SourceAssign)e.Row;
            if (GetAssignedMaps(doc, cache).Any())
            {
                cache.SetValue<Hold>(doc, true);
            }
        }
        protected virtual void Approved_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SourceAssign doc = (SourceAssign)e.Row;
            bool? hold = (bool?)cache.GetValue<Hold>(doc);
            if (hold != true && !GetAssignedMaps(doc, cache).Any())
            {
                cache.SetValue<Approved>(doc, true);
            }
        }
        #endregion
    }
    #endregion

    #region EPApprovalEnabledFlagAttribute

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EPRequireApprovalAttribute : PXDBBoolAttribute
    {

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            base.FieldSelecting(sender, e);

            if (!PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
            {
                sender.SetValue(e.Row, _FieldOrdinal, false);
            }
        }
    }


    #endregion

    #region EPAssignmentMapSelector
    public class EPAssignmentMapSelectorAttribute : PXCustomSelectorAttribute
    {
        public virtual int MapType { get; set; }

        public EPAssignmentMapSelectorAttribute()
            : base(typeof(EPAssignmentMap.assignmentMapID))
        {
            DescriptionField = typeof(EPAssignmentMap.name);
        }

        public IEnumerable GetRecords()
        {
            foreach (EPAssignmentMap map in PXSelect<EPAssignmentMap>.Select(_Graph))
            {
                if (map.AssignmentMapID >= 0 && ((map.MapType == null && MapType != EPMapType.Legacy) || (map.MapType != null && map.MapType != (int)MapType)))
                    continue;

                if (map.GraphType != null)
                {
                    var node = PXSiteMap.Provider.FindSiteMapNode(PXBuildManager.GetType(map.GraphType, false));
                    if (node != null)
                    {
                        yield return map;
                    }
                }
                else
                {
                    if (map.EntityType != null)
                    {
                        Type entityType = PXBuildManager.GetType(map.EntityType, false);
                        if (entityType != null)
                        {
                            Type graphType = EntityHelper.GetPrimaryGraphType(_Graph, entityType);

                            if (graphType != null)
                            {
                                var node = PXSiteMap.Provider.FindSiteMapNode(graphType);
                                if (node != null)
                                {
                                    yield return map;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region DayOfWeekAttribute

    /// <summary>
	/// List days of week.
	/// </summary>
	/// <example>
	/// [DayOfWeek]
	/// </example>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DayOfWeekAttribute : PXIntListAttribute
    {
        public override bool IsLocalizable { get { return false; } }

        public override void CacheAttached(PXCache sender)
        {
            this._AllowedValues = new[] { (int)DayOfWeek.Sunday,
                           (int)DayOfWeek.Monday,
                           (int)DayOfWeek.Tuesday,
                           (int)DayOfWeek.Wednesday,
                           (int)DayOfWeek.Thursday,
                           (int)DayOfWeek.Friday,
                           (int)DayOfWeek.Saturday };
            this._AllowedLabels = new[] { GetDayName(DayOfWeek.Sunday),
                           GetDayName(DayOfWeek.Monday),
                           GetDayName(DayOfWeek.Tuesday),
                           GetDayName(DayOfWeek.Wednesday),
                           GetDayName(DayOfWeek.Thursday),
                           GetDayName(DayOfWeek.Friday),
                           GetDayName(DayOfWeek.Saturday) };
            this._NeutralAllowedLabels = _AllowedLabels;

            base.CacheAttached(sender);
        }

        private static string GetDayName(DayOfWeek day)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(day);
        }
    }

    #endregion

    #region WorkTimeRemindDateAttribute

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WorkTimeRemindDateAttribute : PXRemindDateAttribute
    {
        private readonly Type _activityEmployeeBqlField;
        private int _activityEmployeeFieldOrigin;
        private PXGraph _graph;

        public WorkTimeRemindDateAttribute(Type isReminderOnBqlField, Type startDateBqlField, Type activityEmployeeBqlField)
            : base(isReminderOnBqlField, startDateBqlField)
        {
            _activityEmployeeBqlField = activityEmployeeBqlField;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            _activityEmployeeFieldOrigin = sender.GetFieldOrdinal(sender.GetField(_activityEmployeeBqlField));
            _graph = sender.Graph;
        }

        protected override DateTime? CalcCorrectValue(PXCache sender, object row)
        {
            _reversedRemindAt = 0;
            DateTime? result = base.CalcCorrectValue(sender, row);
            if (result != null && object.Equals(true, sender.GetValue(row, _isReminderOnFieldOrigin)))
            {
                var searchedCalendar = PXSelectJoin<CSCalendar,
                    InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>>,
                    Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
                    Select(_graph, sender.GetValue(row, _activityEmployeeFieldOrigin));
                if (searchedCalendar != null && searchedCalendar.Count != 0)
                {
                    var calendar = (CSCalendar)searchedCalendar[0][typeof(CSCalendar)];
                    var calendarTimeZone = string.IsNullOrEmpty(calendar.TimeZone) ?
                        PXTimeZoneInfo.Invariant :
                        PXTimeZoneInfo.FindSystemTimeZoneById(calendar.TimeZone);
                    var addTicks = 0L;
                    switch (result.Value.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            if (calendar.SunWorkDay == true) addTicks = calendar.SunStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Monday:
                            if (calendar.MonWorkDay == true) addTicks = calendar.MonStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Tuesday:
                            if (calendar.TueWorkDay == true) addTicks = calendar.TueStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Wednesday:
                            if (calendar.WedWorkDay == true) addTicks = calendar.WedStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Thursday:
                            if (calendar.ThuWorkDay == true) addTicks = calendar.ThuStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Friday:
                            if (calendar.FriWorkDay == true) addTicks = calendar.FriStartTime.Value.TimeOfDay.Ticks;
                            break;
                        case DayOfWeek.Saturday:
                            if (calendar.SatWorkDay == true) addTicks = calendar.SatStartTime.Value.TimeOfDay.Ticks;
                            break;
                    }
                    var utcTime = result.Value.Date.AddTicks(addTicks - calendarTimeZone.BaseUtcOffset.Ticks);
                    result = PXTimeZoneInfo.ConvertTimeFromUtc(utcTime, LocaleInfo.GetTimeZone());
                }
            }
            return result;
        }
    }

    #endregion

    #region EmployeeRawAttribute

    /// <summary>
    /// 'EMPLOYEE' dimension selector.
    /// </summary>
    /// <example>
    /// [EmployeeRaw]
    /// </example>
    public class EmployeeRawAttribute : AcctSubAttribute
    {
        #region EmployeeLogin

        [Serializable]
        [PXBreakInheritance]
        [PXHidden]
        public sealed class EmployeeLogin : Users
        {
            #region PKID

            public new abstract class pKID : PX.Data.BQL.BqlGuid.Field<pKID> { }

            #endregion

            #region Username

            public new abstract class username : PX.Data.BQL.BqlString.Field<username> { }

            [PXDBString(64, IsKey = true, IsUnicode = true)]
            [PXUIField(Visible = false)]
            public override string Username
            {
                get
                {
                    return base.Username;
                }
                set
                {
                    base.Username = value;
                }
            }

            #endregion

        }

        #endregion

        public const string DimensionName = "EMPLOYEE";

        public EmployeeRawAttribute()
        {
            Type searchType = typeof(Search2<EPEmployee.acctCD,
                LeftJoin<EmployeeLogin, On<EmployeeLogin.pKID, Equal<EPEmployee.userID>>,
                InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>,
                LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>>,
                Where<MatchWithBranch<GL.Branch.branchID>>>);

            PXDimensionSelectorAttribute attr;
            _Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(EPEmployee.acctCD),
                                    typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName),
                                    typeof(EPEmployee.status), typeof(EPEmployeePosition.positionID), typeof(EPEmployee.departmentID),
									typeof(EPEmployee.defLocationID), typeof(EmployeeLogin.username)));
            attr.DescriptionField = typeof(EPEmployee.acctName);
            _SelAttrIndex = _Attributes.Count - 1;
            this.Filterable = true;
        }
    }

    #endregion

    #region EPViewStatusAttribute

    public class EPViewStatusAttribute : PXIntListAttribute
    {
        public const int NOTVIEWED = 0;
        public const int VIEWED = 1;

        public sealed class NotViewed : PX.Data.BQL.BqlInt.Constant<NotViewed>
		{
            public NotViewed() : base(NOTVIEWED) { }
        }

        public sealed class Viewed : PX.Data.BQL.BqlInt.Constant<Viewed>
		{
            public Viewed() : base(VIEWED) { }
        }

        public EPViewStatusAttribute() : base(
            new[] { NOTVIEWED, VIEWED },
            new[] { Messages.EntityIsNotViewed, Messages.EntityIsViewed })
        { }
    }

    #endregion

    #region CurrentEmployeeByDefaultAttribute

    /// <summary>
    /// Allow determine current logined employee on 'field default' event.
    /// </summary>
    /// <example>
    /// [CurrentEmployeeByDefault]
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CurrentEmployeeByDefaultAttribute : PXDefaultAttribute
    {
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var employee = EmployeeMaint.GetCurrentEmployee(sender.Graph);
            if (employee != null && employee.TimeCardRequired == true)
                e.NewValue = employee.BAccountID;
        }
    }

    #endregion

    #region PXWeekSelectorAttribute

    /// <summary>
    /// Allow select weeks.</br>
    /// Shows start and end date of week.
    /// </summary>
    /// <example>
    /// [PXWeekSelector]
    /// </example>
    [Serializable]
    public class PXWeekSelectorAttribute : PXCustomSelectorAttribute, IPXFieldDefaultingSubscriber
    {
        [PXVirtual]
        [Serializable]
        [PXHidden]
        public partial class EPWeek : IBqlTable
        {
            #region WeekID

            public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

            [PXDBInt(IsKey = true)]
            [PXUIField(Visible = false)]
            public virtual Int32? WeekID { get; set; }

            #endregion

            #region FullNumber

            public abstract class fullNumber : PX.Data.BQL.BqlString.Field<fullNumber> { }

            private String _fullNumber;
            [PXString]
            [PXUIField(DisplayName = "Week")]
            public virtual String FullNumber
            {
                get
                {
                    Initialize();
                    return _fullNumber;
                }
            }

            #endregion

            #region Number

            public abstract class number : PX.Data.BQL.BqlInt.Field<number> { }

            private Int32? _number;
            [PXInt]
            [PXUIField(DisplayName = "Number", Visible = false)]
            public virtual Int32? Number
            {
                get
                {
                    Initialize();
                    return _number;
                }
            }

            #endregion

            #region StartDate

            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

            private DateTime? _startDate;
            [PXDate]
            [PXUIField(DisplayName = "Start")]
            public virtual DateTime? StartDate
            {
                get
                {
                    Initialize();
                    return _startDate;
                }
            }

            #endregion

            #region EndDate

            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

            private DateTime? _endDate;
            [PXDate]
            [PXUIField(DisplayName = "End", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? EndDate
            {
                get
                {
                    Initialize();
                    return _endDate;
                }
            }

            #endregion

            #region Description

            public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

            [PXString]
            [PXUIField(DisplayName = "Description")]
            public virtual String Description
            {
                [PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
                get
                {
                    CultureInfo culture = LocaleInfo.GetCulture();
                    if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
                    {
                        return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
                    }
                    else
                    {
                        return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
                    }
                }
            }

            #endregion

            #region ShortDescription

            public abstract class shortDescription : PX.Data.BQL.BqlString.Field<shortDescription> { }

            [PXString]
            [PXUIField(DisplayName = "Description", Visible = false)]
            public virtual String ShortDescription
            {
                [PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
                get
                {
                    CultureInfo culture = LocaleInfo.GetCulture();
                    if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
                    {
                        return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
                    }
                    else
                    {
                        return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
                    }
                }
            }

            #endregion

            private void Initialize()
            {
                if (_number != null && _fullNumber != null &&
                    _startDate != null && _endDate != null || WeekID == null)
                {
                    return;
                }

                var weekId = (int)WeekID;
                var year = weekId / 100;
                var week = weekId % 100;
                _number = week;
                _fullNumber = string.Format("{0}-{1:00}", year, week);

                var startDate = PX.Data.EP.PXDateTimeInfo.GetWeekStart(year, week);
                _startDate = startDate;
                _endDate = startDate.AddDays(7).AddTicks(-1L);
            }
        }

        private readonly Type _startDateBqlField;
        private readonly Type _timeSpentBqlField;

        private readonly bool _limited;
        private int _startDateOrdinal;
        private int _timeSpentOrdinal;

        public PXWeekSelectorAttribute()
            : base(typeof(EPWeek.weekID),
                typeof(EPWeek.number), typeof(EPWeek.startDate), typeof(EPWeek.endDate))
        {

        }


        public PXWeekSelectorAttribute(Type type, Type[] fieldList)
            : base(type, fieldList)
        {

        }

        public PXWeekSelectorAttribute(Type startDateBqlField, Type timeSpentBqlField)
            : this()
        {
            _startDateBqlField = startDateBqlField;
            _timeSpentBqlField = timeSpentBqlField;
            _limited = true;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.EmitColumnForDescriptionField(sender);

            base.CacheAttached(sender);

            if (_limited)
            {
                _startDateOrdinal = sender.GetFieldOrdinal(sender.GetField(_startDateBqlField));
                _timeSpentOrdinal = sender.GetFieldOrdinal(sender.GetField(_timeSpentBqlField));
            }
        }

        public override void DescriptionFieldCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select &&
            (e.Operation & PXDBOperation.Option) == PXDBOperation.External &&
            (e.Value == null || e.Value is string))
            {
                e.Cancel = true;
				e.Expr = SQLExpression.Null();
                e.DataLength = null;
                e.DataType = PXDbType.Unspecified;
                e.DataValue = null;
            }
        }

        protected IEnumerable GetRecords()
        {
            if (!_limited) return GetAllRecords();

            var cache = _Graph.Caches[_CacheType];
            var startDate = cache.GetValue(cache.Current, _startDateOrdinal);
            return GetRecordsByDate((DateTime?)startDate);
        }

        protected virtual IEnumerable GetAllRecords()
        {
            var pageSize = PXView.MaximumRows;
            if (pageSize < 1) pageSize = 52;

            var startIndex = PXView.StartRow;
            /*var pageNumber = pageSize / */
            PXView.StartRow = 0;

            int year = -1;
            int week = -1;
            if (PXView.Searches != null && PXView.Searches.Length > 0)
                for (int i = 0; i < PXView.Searches.Length; i++)
                {
                    var name = PXView.SortColumns[i].With(_ => _.ToLower());
                    if (string.IsNullOrEmpty(name)) continue;

                    var searchValue = PXView.Searches[i];
                    if (searchValue == null) continue;

                    if (typeof(EPWeek.weekID).Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var weekId = searchValue is int ? (int)searchValue : int.Parse((string)searchValue);
                        year = weekId / 100;
                        week = weekId % 100;
                        break;
                    }

                    var str = searchValue as string;
                    if (str != null && typeof(EPWeek.fullNumber).Name.Equals(name, StringComparison.OrdinalIgnoreCase) && str.Length > 6)
                    {
                        year = int.Parse(str.Substring(0, 4));
                        week = int.Parse(str.Substring(5, 2));
                        break;
                    }
                }

            if ((year == -1 || week == -1) && PXView.Filters != null && PXView.Filters.Length > 0)
            {
                var condArr = new[]
                                {
                                    PXCondition.EQ, PXCondition.BETWEEN, PXCondition.GE, PXCondition.IN,
                                    PXCondition.LE, PXCondition.LLIKE, PXCondition.LIKE, PXCondition.RLIKE
                                };
                foreach (PXFilterRow item in PXView.Filters)
                {
                    var name = item.DataField;
                    if (string.IsNullOrEmpty(name)) continue;
                    var searchValue = item.Value;
                    if (searchValue == null) continue;
                    var condition = item.Condition;
                    if (Array.IndexOf(condArr, condition) < 0) continue;

                    if (typeof(EPWeek.weekID).Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var weekId = searchValue is int ? (int)searchValue : int.Parse((string)searchValue);
                        year = weekId / 100;
                        week = weekId % 100;
                        break;
                    }

                    var str = searchValue as string;
                    if (str != null && typeof(EPWeek.fullNumber).Name.Equals(name, StringComparison.OrdinalIgnoreCase) && str.Length > 6)
                    {
                        year = int.Parse(str.Substring(0, 4));
                        week = int.Parse(str.Substring(5, 2));
                        break;
                    }
                }
            }

            if (year == -1 || week == -1)
            {
                var toDay = PXTimeZoneInfo.Now.Date;
                year = toDay.Year;
                week = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(toDay);
            }

            var currentDate = PX.Data.EP.PXDateTimeInfo.GetWeekStart(year, week).AddDays(startIndex * 7);
            if (currentDate.Year < 1901) yield break;

            var currentWeek = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(currentDate);
            int currentYear = currentDate.Year;

            if (currentWeek > 52)
            {
                if (currentDate.Day > 28)
                {
                    currentYear++;
                    currentWeek = 1;
                }
            }


            for (int i = 0; i < pageSize; i++)
            {
                yield return new EPWeek { WeekID = currentYear * 100 + currentWeek };

                currentDate = currentDate.AddDays(7D);
                if (currentWeek < 52)
                    currentWeek++;
                else
                {
                    currentWeek = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(currentDate);
                    if (currentWeek == 1)
                    {
                        currentYear++;
                    }
                }

                if (currentDate.Year > 9998) yield break;
            }
        }

        public static IEnumerable GetRecordsByDate(DateTime? startDate)
        {
            if (startDate == null) yield break;

            var date = (DateTime)startDate;
            var dateWeek = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(date);
            var utcDate = PXTimeZoneInfo.ConvertTimeToUtc(date, LocaleInfo.GetTimeZone());
            var utcDateWeek = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(utcDate);
            if (dateWeek != utcDateWeek)
            {
                if (date > utcDate)
                {
                    yield return new EPWeek { WeekID = utcDate.Year * 100 + utcDateWeek };
                    yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
                }
                else
                {
                    yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
                    yield return new EPWeek { WeekID = utcDate.Year * 100 + utcDateWeek };
                }
            }
            else
                yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (_limited)
            {
                var oldCurrent = sender.Current;
                sender.Current = e.Row;
                base.FieldVerifying(sender, e);
                sender.Current = oldCurrent;
            }
        }

        public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (!_limited) return;

            var startDate = sender.GetValue(e.Row, _startDateOrdinal);
            if (startDate != null)
            {
                var date = (DateTime)startDate;
                e.NewValue = GetWeekID(date);
            }
        }

        public static int GetWeekID(DateTime date)
        {
            int year = date.Year;
            int week = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(date.Date);

            if (week >= 52 && date.Month == 1)
            {
                year--;
            }

            if (week == 1 && date.Month == 12)
            {
                year++;
            }

            return year * 100 + week;
        }

        public static DateTime GetWeekStartDate(int weekId)
        {
            var year = weekId / 100;
            var weekNumber = weekId % 100;
            return PX.Data.EP.PXDateTimeInfo.GetWeekStart(year, weekNumber);
        }
    }

    #endregion

    #region PXWeekSelector2Attribute

    /// <summary>
    /// Allow select weeks.</br>
    /// Shows start and end date of week, fixed diapason only.
    /// </summary>
    /// <example>
    /// [PXWeekSelector2]
    /// </example>
    [Serializable]
    public class PXWeekSelector2Attribute : PXWeekSelectorAttribute
    {
        public class FullWeekList
        {

            private class Definition : IPrefetchable
            {
                private DateTime _stratDay = new DateTime(2005, 1, 1);
                private const int weekListCount = 1000;
                private List<EPWeekRaw> _weeks = new List<EPWeekRaw>(weekListCount);

                public void Prefetch()
                {
                    DateTime curentDay = _stratDay;
                    for (int i = 0; i < weekListCount; i++)
                    {
                        int year;
                        if (curentDay.AddDays(-3).Year < curentDay.Year && PX.Data.EP.PXDateTimeInfo.GetWeekNumber(curentDay) > 1)
                            year = curentDay.AddYears(-1).Year;
                        else
                            year = curentDay.Year;

                        EPWeek epWeek = new EPWeek { WeekID = year * 100 + PX.Data.EP.PXDateTimeInfo.GetWeekNumber(curentDay) };
                        EPWeekRaw rawWeek = EPWeekRaw.ToEPWeekRaw(epWeek);
                        _weeks.Add(rawWeek);
                        curentDay = curentDay.AddDays(7);
                    }
                }

                public static Definition Get()
                {
                    return PXDatabase.GetSlot<Definition>(typeof(FullWeekList).Name, typeof(EPSetup));
                }

                public List<EPWeekRaw> Weeks
                {
                    get { return _weeks; }
                }
            }

            public static List<EPWeekRaw> Weeks()
            {
                var def = Definition.Get();
                if (def == null) return new List<EPWeekRaw> { };
                return def.Weeks;
            }

        }

        public PXWeekSelector2Attribute() : base(typeof(EPWeekRaw.weekID),
                                                 new Type[]
                                                    {
                                                        typeof (EPWeekRaw.fullNumber),
                                                        typeof (EPWeekRaw.startDate),
                                                        typeof (EPWeekRaw.endDate)
                                                    }
                                                )
        {
			CacheGlobal = true;
		}

		protected static EPSetup GetEPSetup(PXGraph graph)
		{
			if (graph.Caches.ContainsKey(typeof(EPSetup)) && graph.Caches[typeof(EPSetup)].Current != null)
			{
				return graph.Caches[typeof(EPSetup)].Current as EPSetup;
			}
			else
			{
                return PXSetup<EPSetup>.SelectSingleBound(graph, null);
			}
		}

		protected override IEnumerable GetAllRecords()
        {
            if (IsCustomWeek(_Graph))
            {
                EPSetup setup = GetEPSetup(_Graph);
                var res = (PXResultset<EPCustomWeek>)PXSelect<EPCustomWeek>.Select(_Graph, null).AsEnumerable();
				List<EPWeekRaw> customWeeks = res.Select(_ => EPWeekRaw.ToEPWeekRaw(_)).ToList();
                //Fills weeks that were before usage of custom weeks so that older records can still display weeks correctly.
                List<EPWeekRaw> allWeeks = FullWeekList.Weeks().TakeWhile(x => x.WeekID != setup.FirstCustomWeekID).ToList();
                allWeeks.AddRange(customWeeks);
                return allWeeks;
            }
            else
                return FullWeekList.Weeks();
        }

        public static bool IsCustomWeek(PXGraph graph)
        {
			EPSetup setup = GetEPSetup(graph);
			return setup != null && setup.CustomWeek == true;
		}

		public static bool IsCustomWeek(PXGraph graph, int weekID)
		{
            EPSetup setup = GetEPSetup(graph);
            return setup != null && setup.CustomWeek == true && weekID > setup.FirstCustomWeekID;
		}

		private static EPCustomWeek GetCustomWeek(PXGraph graph, DateTime date)
        {
            EPCustomWeek customWeek = PXSelect<EPCustomWeek, Where<Required<EPCustomWeek.startDate>, Between<EPCustomWeek.startDate, EPCustomWeek.endDate>>>.SelectSingleBound(graph, null, date);
            if (customWeek == null)
                throw new PXException(Messages.CustomWeekNotFoundByDate, date);
            return customWeek;
        }

        private static EPCustomWeek GetCustomWeek(PXGraph graph, int weekID)
        {
            EPCustomWeek customWeek = PXSelect<EPCustomWeek, Where<EPCustomWeek.weekID, Equal<Required<EPCustomWeek.weekID>>>>.SelectSingleBound(graph, null, weekID);
            if (customWeek == null)
                throw new PXException(Messages.CustomWeekNotFound);
            return customWeek;
        }

        private static EPWeekRaw GetStandartWeek(PXGraph graph, DateTime date)
        {
            EPWeek epWeek = new EPWeek { WeekID = date.Year * 100 + PX.Data.EP.PXDateTimeInfo.GetWeekNumber(date) };
            return EPWeekRaw.ToEPWeekRaw(epWeek);
        }

        public static int GetWeekID(PXGraph graph, DateTime date)
        {
            if (IsCustomWeek(graph))
                return GetCustomWeek(graph, date).WeekID.Value;
            else
                return PXWeekSelectorAttribute.GetWeekID(date);
        }

        public static int GetNextWeekID(PXGraph graph, int weekID)
        {
            if (IsCustomWeek(graph, weekID))
                return GetWeekID(graph, GetCustomWeek(graph, weekID).EndDate.Value.AddDays(1d));
            else
                return PXWeekSelectorAttribute.GetWeekID(PXWeekSelectorAttribute.GetWeekStartDate(weekID).AddDays(7d));
        }

        public static int GetNextWeekID(PXGraph graph, DateTime date)
        {
            if (IsCustomWeek(graph))
                return GetWeekID(graph, GetCustomWeek(graph, date).EndDate.Value.AddDays(1d));
            else
                return PXWeekSelectorAttribute.GetWeekID(date.AddDays(7));
        }

        public static DateTime GetWeekStartDate(PXGraph graph, int weekId)
        {
            if (IsCustomWeek(graph, weekId))
                return GetCustomWeek(graph, weekId).StartDate.Value;
            else
                return PXWeekSelectorAttribute.GetWeekStartDate(weekId);
        }

        public static DateTime GetWeekEndDate(PXGraph graph, int weekId)
        {
            if (IsCustomWeek(graph, weekId))
                return GetCustomWeek(graph, weekId).EndDate.Value;
            else
                return PXWeekSelectorAttribute.GetWeekStartDate(weekId).AddDays(6d);
        }


        public class WeekInfo
        {
            private Dictionary<DayOfWeek, DayInfo> _days = new Dictionary<DayOfWeek, DayInfo>();

            public DayInfo Mon { get { return GetDayInfo(DayOfWeek.Monday); } }
            public DayInfo Tue { get { return GetDayInfo(DayOfWeek.Tuesday); } }
            public DayInfo Wed { get { return GetDayInfo(DayOfWeek.Wednesday); } }
            public DayInfo Thu { get { return GetDayInfo(DayOfWeek.Thursday); } }
            public DayInfo Fri { get { return GetDayInfo(DayOfWeek.Friday); } }
            public DayInfo Sat { get { return GetDayInfo(DayOfWeek.Saturday); } }
            public DayInfo Sun { get { return GetDayInfo(DayOfWeek.Sunday); } }

            private DayInfo GetDayInfo(DayOfWeek mDay)
            {
                if (_days.ContainsKey(mDay))
                    return _days[mDay];
                else
                    return new DayInfo(null);
            }

            public void AddDayInfo(DateTime date)
            {
                _days[date.DayOfWeek] = new DayInfo(date);
            }

            public Dictionary<DayOfWeek, DayInfo> Days
            {
                get { return _days; }
            }

            public bool IsValid(DateTime date)
            {
                foreach (DayInfo info in _days.Values)
                {
                    if (info.Enabled && info.Date.Value.Date == date.Date.Date)
                        return true;
                }

                return false;
            }
        }

        public class DayInfo
        {
            public DayInfo(DateTime? date)
            {
                _date = date;
            }

            private DateTime? _date;
            public DateTime? Date { get { return _date; } }
            public bool Enabled { get { return (_date != null); } }
        }

        public static WeekInfo GetWeekInfo(PXGraph graph, int weekId)
        {
            WeekInfo ret = new WeekInfo();
            for (DateTime date = GetWeekStartDate(graph, weekId); date <= GetWeekEndDate(graph, weekId); date = date.AddDays(1))
            {
                ret.AddDayInfo(date);
            }
            return ret;
        }

    }

    #endregion

    #region EPActivityDefaultWeekAttribute
    public class EPActivityDefaultWeekAttribute : PXDefaultAttribute
    {
        private readonly Type dateField;

        public EPActivityDefaultWeekAttribute(Type _dateField) : base()
        {
            dateField = _dateField;
            this.PersistingCheck = PXPersistingCheck.Nothing;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            if (!typeof(PMTimeActivity).IsAssignableFrom(sender.GetItemType()) && !typeof(CRPMTimeActivity).IsAssignableFrom(sender.GetItemType()))
                throw new PXArgumentException(EP.Messages.IncorrectUsingAttribute, new object[] { GetType().Name, typeof(PMTimeActivity).Name });
        }

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var field = sender.Graph.Caches[sender.GetItemType()].GetBqlField(dateField.Name);

            var dateValue = sender.GetValue(e.Row, field.Name);

            if (e.Row != null && dateValue != null)
            {
                if (PXWeekSelector2Attribute.IsCustomWeek(sender.Graph))
                {
                    try
                    {
                        e.NewValue = PXWeekSelector2Attribute.GetWeekID(sender.Graph, (DateTime)dateValue);
                    }
                    catch (PXException exception)
                    {
                        sender.RaiseExceptionHandling(FieldName, e.Row, true, exception);
                    }
                }
                else
                    e.NewValue = PXWeekSelector2Attribute.GetWeekID(sender.Graph, (DateTime)dateValue);
            }
            else
                e.NewValue = null;
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var field = sender.Graph.Caches[sender.GetItemType()].GetBqlField(dateField.Name);

            var dateValue = sender.GetValue(e.Row, field.Name);
            var weekValue = sender.GetValue(e.Row, FieldName);

            if (e.Row != null && dateValue != null && weekValue == null && PXWeekSelector2Attribute.IsCustomWeek(sender.Graph) && e.Operation != PXDBOperation.Delete)
            {
                try
                {
                    PXWeekSelector2Attribute.GetWeekID(sender.Graph, (DateTime)dateValue);
                }
                catch (PXException exception)
                {
                    sender.RaiseExceptionHandling(FieldName, e.Row, true, exception);
                }
            }

            base.RowPersisting(sender, e);
        }

    }
    #endregion


    #region EPActivityProjectDefaultAttribute
    public class EPActivityProjectDefaultAttribute : PM.ProjectDefaultAttribute
    {
        private readonly Type isBillableField;

        public EPActivityProjectDefaultAttribute(Type _isBillableField = null)
            : base(BatchModule.TA, typeof(Search<PM.PMProject.contractID, Where<PM.PMProject.nonProject, Equal<True>>>))
        {
            if (_isBillableField != null)
                isBillableField = _isBillableField;


        }

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            bool? isBillable = null;

            if (isBillableField != null)
                isBillable = (bool?)sender.GetValue(e.Row, isBillableField.Name);

            if (e.Row == null || isBillable != true || !PM.ProjectAttribute.IsPMVisible(BatchModule.TA))
                base.FieldDefaulting(sender, e);
        }

    }
    #endregion

    #region EPActiveProjectAttribute
    [PXDBInt()]
    [PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
    [PXAttributeFamily(typeof(AcctSubAttribute))]
    public class EPProjectAttribute : AcctSubAttribute
    {
        protected Type OwnerFieldType;
        public EPProjectAttribute(Type ownerFieldType)
        {
            this.OwnerFieldType = ownerFieldType;

            Type searchType = BuildSearchClause();

            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, searchType, typeof(PMProject.contractCD),
            typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.status));

            select.DescriptionField = typeof(PMProject.description);
            select.ValidComboRequired = true;
            select.CacheGlobal = true;

            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;

            Filterable = true;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            /*Visible =*/
            Enabled = ProjectAttribute.IsPMVisible(BatchModule.TA);
        }

        protected virtual Type BuildSearchClause()
        {
            Type SearchType =
                BqlCommand.Compose(
                typeof(Search2<,,,>),
                typeof(PMProject.contractID),
                BuildJoinClause(),
                BuildWhereClause(),
                BuildOrderByClause());

            return SearchType;
        }

        protected virtual Type BuildJoinClause()
        {
            Type staticJoinType = typeof(LeftJoin<EPEmployeeContract, On<EPEmployeeContract.contractID, Equal<PMProject.contractID>, And<EPEmployeeContract.employeeID, Equal<EPEmployee.bAccountID>>>>);

            Type JoinType =
                BqlCommand.Compose(
                typeof(LeftJoin<,,>),
                typeof(EPEmployee),
                typeof(On<,>),
                typeof(EPEmployee.userID),
                typeof(Equal<>),
                typeof(Current2<>),
                OwnerFieldType,
                staticJoinType);

            return JoinType;
        }

        protected virtual Type BuildWhereClause()
        {
            return typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>,
                And2<Where<PMProject.restrictToEmployeeList, Equal<False>, Or<EPEmployeeContract.employeeID, IsNotNull>>,
                And2<Match<Current<AccessInfo.userName>>,
                And<Where<PMProject.visibleInTA, Equal<True>, Or<PMProject.nonProject, Equal<True>>>>>>>);
        }

        protected virtual Type BuildOrderByClause()
        {
            return typeof(OrderBy<Desc<PMProject.contractCD>>);
        }
    }

    #endregion

    #region RateTypesAttribute
    public class RateTypesAttribute : PXStringListAttribute
    {
        public const string Hourly = "H";
        public const string Salary = "S";
        public const string SalaryWithExemption = "E";

        public RateTypesAttribute()
            : base(
            new[] { Hourly, Salary, SalaryWithExemption },
            new[] { Messages.Hourly, Messages.Salary, Messages.SalaryWithExemption })
        { }

        public class hourly : PX.Data.BQL.BqlString.Constant<hourly>
		{
            public hourly() : base(Hourly) { }
        }

        public class salary : PX.Data.BQL.BqlString.Constant<salary>
		{
            public salary() : base(Salary) { }
        }

        public class salaryWithExemption : PX.Data.BQL.BqlString.Constant<salaryWithExemption>
		{
            public salaryWithExemption() : base(SalaryWithExemption) { }
        }
    }
    #endregion

    #region FilterHeaderDescriptionAttribute

    public sealed class FilterHeaderDescriptionAttribute : PXDACDescriptionAttribute
    {
        public FilterHeaderDescriptionAttribute()
            : base(typeof(FilterHeader), new PXPrimaryGraphAttribute(typeof(PX.Objects.CS.CSFilterMaint)))
        { }
    }

    #endregion

    /// <summary>
    /// Time is displayed and modified in the timezone of the user/Employee.
    /// </summary>
    public class EPDBDateAndTimeAttribute : PXDBDateAndTimeAttribute
    {
        protected Type typeUserID;
        protected PXTimeZoneInfo timezone;

        public EPDBDateAndTimeAttribute(Type userID)
        {
            this.typeUserID = userID;
        }

        public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            InitTimeZone(sender, e.Row);

            base.CommandPreparing(sender, e);
        }
        public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            using (new PXConnectionScope())
            {
                InitTimeZone(sender, e.Row);
            }

            base.RowSelecting(sender, e);
        }

        protected virtual void InitTimeZone(PXCache sender, object row)
        {
            if (row == null) return;

            Guid? userID = (Guid?)sender.GetValue(row, sender.GetField(typeUserID));
            if (userID != null)
            {
                UserPreferences pref = PXSelect<UserPreferences, Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(sender.Graph, userID);
                if (pref != null && !string.IsNullOrEmpty(pref.TimeZone))
                {
                    timezone = PXTimeZoneInfo.FindSystemTimeZoneById(pref.TimeZone);
                    return;
                }
                EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(sender.Graph, userID);
                if (employee != null && employee.CalendarID != null)
                {
                    CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(sender.Graph, employee.CalendarID);
                    if (cal != null && !string.IsNullOrEmpty(cal.TimeZone))
                    {
                        timezone = PXTimeZoneInfo.FindSystemTimeZoneById(cal.TimeZone);
                    }
                }
            }
        }

        protected override PXTimeZoneInfo GetTimeZone()
        {
            if (timezone != null)
                return timezone;

            return base.GetTimeZone();
        }
    }

    #region PXSubordinateAndWingmenSelectorAttribute
    public class PXSubordinateAndWingmenSelectorAttribute : PXSubordinateSelectorAttribute
    {

        public PXSubordinateAndWingmenSelectorAttribute(Type where)
            : base("BIZACCT", GetCommand(where), true, true)
        {
        }

        public PXSubordinateAndWingmenSelectorAttribute()
            : this(null)
        {
        }

        private static Type GetCommand(Type where)
        {
            var whereType = typeof(Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                        Or<CREmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
                        Or<CREmployee.bAccountID, WingmanUser<Current<AccessInfo.userID>>>
                        >>);
            if (where != null)
                whereType = BqlCommand.Compose(typeof(Where2<,>),
                    typeof(Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                        Or<CREmployee.userID, OwnedUser<Current<AccessInfo.userID>>>>),
                    typeof(And<>), where);
            return BqlCommand.Compose(typeof(Search5<,,,>), typeof(CREmployee.bAccountID),
                typeof(LeftJoin<Users, On<Users.pKID, Equal<CREmployee.userID>>>),
                whereType,
                typeof(Aggregate<GroupBy<CREmployee.acctCD>>));
        }

    }
    #endregion

    #region PXSubordinateAndWingmenOwnerSelectorAttribute

    /// <summary>
    /// Allows show employees which are subordinated or wingman for current logined employee.
    /// </summary>
    /// <example>
    /// [PXSubordinateAndWingmenOwnerSelector]
    /// </example>
    public class PXSubordinateAndWingmenOwnerSelectorAttribute : PXOwnerSelectorAttribute
    {
        public PXSubordinateAndWingmenOwnerSelectorAttribute()
            : base(null, typeof(Search5<EPEmployee.pKID,
                LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.pKID>>>,
                Where<EPEmployee.pKID, Equal<Current<AccessInfo.userID>>,
                        Or<EPEmployee.pKID, OwnedUser<Current<AccessInfo.userID>>,
                        Or<EPEmployee.bAccountID, WingmanUser<Current<AccessInfo.userID>>>
                        >>,
                Aggregate<GroupBy<EPEmployee.pKID>>>), false)
        {
        }
    }
    #endregion

    #region BusinessDateDefaultAttribute

    /// <summary>
    /// Allow determine current business date with time part on 'field default' event.
    /// </summary>
    /// <example>
    /// [BusinessDateTimeDefault]
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BusinessDateTimeDefaultAttribute : PXDefaultAttribute
    {
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = sender.Graph.Accessinfo.BusinessDate?.AddMinutes(PXTimeZoneInfo.Now.Minute + PXTimeZoneInfo.Now.Hour * 60);
        }
    }

    #endregion

    #region  EPAssignmentMapPrimaryGraphAttribute

    public sealed class EPAssignmentMapPrimaryGraphAttribute : CRCacheIndependentPrimaryGraphListAttribute
    {
        public EPAssignmentMapPrimaryGraphAttribute() : base(
            new[]
            {
                typeof (EPApprovalMapMaint),
                typeof (EPAssignmentMapMaint),
                typeof (EPAssignmentMaint),		// legacy
				typeof (EPAssignmentAndApprovalMapEnq)
            },
            new[]
            {
                typeof (Select<EPAssignmentMap,
                    Where<EPAssignmentMap.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>,
                        And<EPAssignmentMap.mapType, Equal<EPMapType.approval>>>>),

                typeof (Select<EPAssignmentMap,
                    Where<EPAssignmentMap.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>,
                        And<EPAssignmentMap.mapType, Equal<EPMapType.assignment>>>>),

                typeof (Select<EPAssignmentMap,
                    Where<EPAssignmentMap.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>,
                        And<EPAssignmentMap.mapType, Equal<EPMapType.legacy>,
                        And<EPAssignmentMap.assignmentMapID, Greater<Zero>>>>>),	// To handle the cache.Insert() before the redirect

				typeof (Select<EPAssignmentMap>)
            })
        { }

        protected override void OnAccessDenied(Type graphType)
        {
            throw new AccessViolationException(CR.Messages.FormNoAccessRightsMessage(graphType));
        }
    }

    #endregion
    /// <summary>
    /// A helper for the approval mechanism.
    /// </summary>
    public static class EPApprovalHelper
    {
        public static string BuildEPApprovalDetailsString(PXCache sender, IApprovalDescription currentDocument)
        {
            var result = String.Empty;
            CashAccount ca = PXSelect<CashAccount>.Search<CashAccount.cashAccountID>(sender.Graph, currentDocument.CashAccountID).First();
            PaymentMethod pm = PXSelect<PaymentMethod>.Search<PaymentMethod.paymentMethodID>(sender.Graph, currentDocument.PaymentMethodID).First();
            CurrencyInfo ci = PXSelect<CurrencyInfo>.Search<CurrencyInfo.curyInfoID>(sender.Graph, currentDocument.CuryInfoID).First();

            string chargesString = (currentDocument.CuryChargeAmt == null || currentDocument.CuryChargeAmt == 0.0m)
                ? PXLocalizer.Localize(Common.Messages.NoCharges)
                : PXLocalizer.Localize(Common.Messages.Charges) + "=" +
                  PXCurrencyAttribute.Round(sender, currentDocument, currentDocument.CuryChargeAmt.Value,
                      CMPrecision.TRANCURY).ToString($"N{ci.BasePrecision ?? 4}");
            result = ca?.Descr + " (" + pm?.Descr + "; " + chargesString + ")";
            return result;
        }
    }
}