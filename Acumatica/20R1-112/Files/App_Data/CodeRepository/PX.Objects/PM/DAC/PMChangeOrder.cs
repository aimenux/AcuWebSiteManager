using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.TM;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PM
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ChangeOrder)]
	[PXPrimaryGraph(typeof(ChangeOrderEntry))]
	[Serializable]
	[PXEMailSource]
	public class PMChangeOrder : PX.Data.IBqlTable, IAssign
	{
		public const string FieldClass = "CHANGEORDER";
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}

		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(PMChangeOrder.refNbr), DescriptionField = typeof(PMChangeOrder.description))]
		[AutoNumber(typeof(Search<PMSetup.changeOrderNumbering>), typeof(AccessInfo.businessDate))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectNbr
		public abstract class projectNbr : PX.Data.BQL.BqlString.Field<projectNbr> {
			public const int Length = 15;
		}
		/// <summary>
		/// Gets or sets description
		/// </summary>
		[PXDBString(projectNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Revenue Change Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ProjectNbr
		{
			get;
			set;
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		[PXForeignReference(typeof(Field<classID>.IsRelatedTo<PMChangeOrderClass.classID>))]
		[PXDBString(PMChangeOrderClass.classID.Length, IsUnicode = true, InputMask = "")]
		[PXDefault(typeof(Search<PMSetup.defaultChangeOrderClassID>))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PMChangeOrderClass.classID, Where<PMChangeOrderClass.isActive, Equal<True>>>), DescriptionField = typeof(PMChangeOrderClass.description))]
		public virtual String ClassID
		{
			get;
			set;
		}
		#endregion				
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// Gets or sets description
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region Text
		public abstract class text : PX.Data.BQL.BqlString.Field<text> { }
		protected String _Text;
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Details")]
		public virtual String Text
		{
			get
			{
				return this._Text;
			}
			set
			{
				this._Text = value;
				_plainText = null;
			}
		}
		#endregion
		#region DescriptionAsPlainText
		public abstract class descriptionAsPlainText : PX.Data.BQL.BqlString.Field<descriptionAsPlainText> { }

		private string _plainText=null;
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false)]
		public virtual String DescriptionAsPlainText
		{
			get
			{
				return _plainText ?? (_plainText = PX.Data.Search.SearchService.Html2PlainText(this.Text));
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[ChangeOrderStatus.List()]
		[PXDefault(ChangeOrderStatus.OnHold)]
		[PXUIField(DisplayName = "Status", Required = true, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold")]
		[PXDefault(true)]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		protected Boolean? _Approved;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approved", Enabled = false)]
		public virtual Boolean? Approved
		{
			get
			{
				return this._Approved;
			}
			set
			{
				this._Approved = value;
			}
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		protected bool? _Rejected = false;
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Rejected", Enabled = false)]
		public bool? Rejected
		{
			get
			{
				return _Rejected;
			}
			set
			{
				_Rejected = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[Project(typeof(Where<PMProject.changeOrderWorkflow, Equal<True>>), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXFormula(typeof(Selector<projectID, PMProject.customerID>))]
		[Customer(DescriptionField = typeof(Customer.acctName), Enabled = false)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Change Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region CompletionDate
		public abstract class completionDate : PX.Data.BQL.BqlDateTime.Field<completionDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Approval Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CompletionDate
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "External Reference Nbr.")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		[PXDBString(refNbr.Length, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Orig. CO Ref. Nbr.")]
		public virtual String OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ReverseStatus
		public abstract class reverseStatus : PX.Data.BQL.BqlString.Field<reverseStatus> { }
		[PXDBString(1, IsFixed = true)]
		[ChangeOrderReverseStatus.List()]
		[PXDefault(ChangeOrderReverseStatus.None)]
		[PXUIField(DisplayName = "Reverse Status", Enabled = false)]
		public virtual String ReverseStatus
		{
			get;
			set;
		}
		#endregion

		#region CostTotal
		public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost Budget Change Total")]
		public virtual Decimal? CostTotal
		{
			get;
			set;
		}
		#endregion
		#region RevenueTotal
		public abstract class revenueTotal : PX.Data.BQL.BqlDecimal.Field<revenueTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revenue Budget Change Total")]
		public virtual Decimal? RevenueTotal
		{
			get;
			set;
		}
		#endregion
		#region CommitmentTotal
		public abstract class commitmentTotal : PX.Data.BQL.BqlDecimal.Field<commitmentTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commitments Change Total")]
		public virtual Decimal? CommitmentTotal
		{
			get;
			set;
		}
		#endregion
		#region GrossMarginAmount
		public abstract class grossMarginAmount : PX.Data.BQL.BqlDecimal.Field<grossMarginAmount> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin Amount")]
		public virtual Decimal? GrossMarginAmount
		{
			[PXDependsOnFields(typeof(revenueTotal), typeof(costTotal))]
			get
			{
				return RevenueTotal - CostTotal;
			}

		}
		#endregion
		#region GrossMarginPct
		public abstract class grossMarginPct : PX.Data.BQL.BqlDecimal.Field<grossMarginPct> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin %")]
		public virtual Decimal? GrossMarginPct
		{
			[PXDependsOnFields(typeof(revenueTotal), typeof(costTotal))]
			get
			{
				if (RevenueTotal != 0)
				{
					return 100 * (RevenueTotal - CostTotal) / RevenueTotal;
				}
				else
					return 0;
			}
		}
		#endregion

		
		#region ChangeRequestCostTotal
		public abstract class changeRequestCostTotal : PX.Data.BQL.BqlDecimal.Field<changeRequestCostTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Request Cost Total", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual Decimal? ChangeRequestCostTotal
		{
			get;
			set;
		}
		#endregion
		#region ChangeRequestLineTotal
		public abstract class changeRequestLineTotal : PX.Data.BQL.BqlDecimal.Field<changeRequestLineTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Request Line Total", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual Decimal? ChangeRequestLineTotal
		{
			get;
			set;
		}
		#endregion
		#region ChangeRequestMarkupTotal
		public abstract class changeRequestMarkupTotal : PX.Data.BQL.BqlDecimal.Field<changeRequestMarkupTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Request Markup Total", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual Decimal? ChangeRequestMarkupTotal
		{
			get;
			set;
		}
		#endregion
		#region ChangeRequestPriceTotal
		public abstract class changeRequestPriceTotal : PX.Data.BQL.BqlDecimal.Field<changeRequestPriceTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Request Price Total", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual Decimal? ChangeRequestPriceTotal
		{
			get;
			set;
		}
		#endregion


		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;

		/// <summary>
		/// The workgroup that is responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;

		/// <summary>
		/// The <see cref="EPEmployee">Employee</see> responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDBGuid()]
		[PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXOwnerSelector(typeof(PMChangeOrder.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region DelayDays
		public abstract class delayDays : PX.Data.BQL.BqlInt.Field<delayDays> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Contract Time Change, Days")]
		public virtual Int32? DelayDays
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion

		#region IsCostVisible
		public abstract class isCostVisible : PX.Data.BQL.BqlBool.Field<isCostVisible> { }
		[PXBool()]
		[PXUIField(DisplayName = "Visible Cost", Enabled = false)]
		[PXUnboundDefault(true)]
		public virtual Boolean? IsCostVisible
		{
			get;
			set;
		}
		#endregion
		#region IsRevenueVisible
		public abstract class isRevenueVisible : PX.Data.BQL.BqlBool.Field<isRevenueVisible> { }
		[PXBool()]
		[PXUIField(DisplayName = "Visible Revenue", Enabled = false)]
		[PXUnboundDefault(true)]
		public virtual Boolean? IsRevenueVisible
		{
			get;
			set;
		}
		#endregion
		#region IsDetailsVisible
		public abstract class isDetailsVisible : PX.Data.BQL.BqlBool.Field<isDetailsVisible> { }
		[PXBool()]
		[PXUIField(DisplayName = "Visible Details", Enabled = false)]
		[PXUnboundDefault(true)]
		public virtual Boolean? IsDetailsVisible
		{
			get;
			set;
		}
		#endregion
		#region IsChangeRequestVisible
		public abstract class isChangeRequestVisible : PX.Data.BQL.BqlBool.Field<isChangeRequestVisible> { }
		[PXBool()]
		[PXUIField(DisplayName = "2-Tier Change Management", Enabled = false)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? IsChangeRequestVisible
		{
			get;
			set;
		}
		#endregion

		#region Attributes
		public abstract class attributes : CR.BqlAttributes.Field<attributes> { }

		[CR.CRAttributesField(typeof(PMChangeOrder.classID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.PM, "{0} - {1}", new Type[] { typeof(PMChangeOrder.refNbr), typeof(PMChangeOrder.projectID) },
			new Type[] { typeof(PMChangeOrder.description) },
			NumberFields = new Type[] { typeof(PMChangeOrder.refNbr) },
			Line1Format = "{0:d}{1}", Line1Fields = new Type[] { typeof(PMChangeOrder.date), typeof(PMChangeOrder.status) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(PMChangeOrder.description) }
		)]
		[PXNote(DescriptionField = typeof(PMChangeOrderClass.description))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class ChangeOrderStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { OnHold, PendingApproval, Open, Closed, Rejected },
				new string[] { Messages.OnHold, Messages.PendingApproval, Messages.Open, Messages.Closed, Messages.Rejected })
			{; }
		}
		public const string OnHold = "H";
		public const string PendingApproval = "A";
		public const string Open = "O";
		public const string Closed = "C";
		public const string Rejected = "R";

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) {; }
		}
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class ChangeOrderReverseStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, Reversed, Reversal },
				new string[] { Messages.None, Messages.Reversed, Messages.Reversing })
			{; }
		}
		public const string None = "N";
		public const string Reversed = "X";
		public const string Reversal = "R";

		public class reversed : PX.Data.BQL.BqlString.Constant<reversed>
		{
			public reversed() : base(Reversed) {; }
		}

		public class reversal : PX.Data.BQL.BqlString.Constant<reversal>
		{
			public reversal() : base(Reversal) {; }
		}

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(None) {; }
		}
	}
}
