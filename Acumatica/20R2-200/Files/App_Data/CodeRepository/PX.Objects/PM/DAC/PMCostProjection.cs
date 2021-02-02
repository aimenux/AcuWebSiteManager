using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CN.ProjectAccounting;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.CostProjection)]
	[PXPrimaryGraph(typeof(CostProjectionEntry))]
	[Serializable]
	[PXEMailSource]
	public class PMCostProjection : PX.Data.IBqlTable, IAssign
	{		
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[Project(typeof(Where<PMProject.baseType, Equal<PX.Objects.CT.CTPRType.project>, And<PMProject.nonProject, Equal<False>>>), IsKey = true, WarnIfCompleted = false)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID>
		{
		}
		[PXSelector(typeof(Search<PMCostProjection.revisionID, Where<PMCostProjection.projectID, Equal<Current<projectID>>>, OrderBy<Desc<PMCostProjection.revisionID>>>), DescriptionField = typeof(description))]
		[PXDBString(30, IsKey = true, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RevisionID
		{
			get;
			set;
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		[PXForeignReference(typeof(Field<classID>.IsRelatedTo<PMCostProjectionClass.classID>))]
		[PXDBString(PMCostProjectionClass.classID.Length, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PMCostProjectionClass.classID>), DescriptionField = typeof(PMCostProjectionClass.description))]
		[PXRestrictor(typeof(Where<PMCostProjectionClass.isActive, Equal<True>>), Messages.ClassIsInactive)]
		public virtual String ClassID
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, IsFixed = true)]
		[CostProjectionStatus.List()]
		[PXDefault(CostProjectionStatus.OnHold)]
		[PXUIField(DisplayName = "Status", Required = true, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold")]
		[PXDefault(true)]
		public virtual Boolean? Hold
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approved", Enabled = false)]
		public virtual Boolean? Approved
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Rejected", Enabled = false)]
		public bool? Rejected
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Revision Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedQuantity
		public abstract class totalBudgetedQuantity : PX.Data.BQL.BqlDecimal.Field<totalBudgetedQuantity> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Budgeted Quantity", Enabled = false)]
		public virtual Decimal? TotalBudgetedQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedAmount
		public abstract class totalBudgetedAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedAmount> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Budgeted Cost", Enabled = false)]
		public virtual Decimal? TotalBudgetedAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalActualQuantity
		public abstract class totalActualQuantity : PX.Data.BQL.BqlDecimal.Field<totalActualQuantity> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Actual Quantity", Enabled = false)]
		public virtual Decimal? TotalActualQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalActualAmount
		public abstract class totalActualAmount : PX.Data.BQL.BqlDecimal.Field<totalActualAmount> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Actual Amount", Enabled = false)]
		public virtual Decimal? TotalActualAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalUnbilledQuantity
		public abstract class totalUnbilledQuantity : PX.Data.BQL.BqlDecimal.Field<totalUnbilledQuantity> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Unbilled Quantity", Enabled = false)]
		public virtual Decimal? TotalUnbilledQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalUnbilledAmount
		public abstract class totalUnbilledAmount : PX.Data.BQL.BqlDecimal.Field<totalUnbilledAmount> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Unbilled Amount", Enabled = false)]
		public virtual Decimal? TotalUnbilledAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalQuantity
		public abstract class totalQuantity : PX.Data.BQL.BqlDecimal.Field<totalQuantity> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Quantity to Complete")]
		public virtual Decimal? TotalQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalAmount
		public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Cost to Complete")]
		public virtual Decimal? TotalAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalProjectedQuantity
		public abstract class totalProjectedQuantity : PX.Data.BQL.BqlDecimal.Field<totalProjectedQuantity> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Quantity at Completion")]
		public virtual Decimal? TotalProjectedQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalProjectedAmount
		public abstract class totalProjectedAmount : PX.Data.BQL.BqlDecimal.Field<totalProjectedAmount> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Cost at Completion")]
		public virtual Decimal? TotalProjectedAmount
		{
			get;
			set;
		}
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		
		/// <summary>
		/// The workgroup that is responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(PMProject.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		
		/// <summary>
		/// The <see cref="EPEmployee">Employee</see> responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDefault(typeof(PMProject.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Owner(typeof(PMChangeRequest.workgroupID))]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get; set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get; set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get; set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get; set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get; set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get; set;
		}
		#endregion
		#endregion

		#region TotalQuantityToComplete
		public abstract class totalQuantityToComplete : PX.Data.BQL.BqlDecimal.Field<totalQuantityToComplete> { }
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quantity To Complete", Enabled = false)]
		public virtual Decimal? TotalQuantityToComplete
		{
			[PXDependsOnFields(typeof(totalBudgetedQuantity), typeof(totalActualQuantity), typeof(totalUnbilledQuantity))]
			get { return Math.Max(0, TotalBudgetedQuantity.GetValueOrDefault() - (TotalActualQuantity.GetValueOrDefault() + TotalUnbilledQuantity.GetValueOrDefault() )); }
		}
		#endregion
		#region TotalAmountToComplete
		public abstract class totalAmountToComplete : PX.Data.BQL.BqlDecimal.Field<totalAmountToComplete> { }

		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost To Complete", Enabled = false)]
		public virtual Decimal? TotalAmountToComplete
		{
			[PXDependsOnFields(typeof(totalBudgetedAmount), typeof(totalActualAmount), typeof(totalUnbilledAmount))]
			get { return Math.Max(0, TotalBudgetedAmount.GetValueOrDefault() - (TotalActualAmount.GetValueOrDefault() + TotalUnbilledAmount.GetValueOrDefault())); }
		}
		#endregion
	}
}
