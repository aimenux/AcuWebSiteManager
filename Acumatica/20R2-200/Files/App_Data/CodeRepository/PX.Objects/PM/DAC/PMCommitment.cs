using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.GL;
using System.Text;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.Commitment)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMCommitment : PX.Data.IBqlTable, IProjectFilter, IQuantify
	{		
		#region CommitmentID
		public abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
		protected Guid? _CommitmentID;
		[PXDefault]
		[PXDBGuid(IsKey =true)]
		public virtual Guid? CommitmentID
		{
			get
			{
				return _CommitmentID;
			}
			set
			{
				_CommitmentID = value;
			}
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(IsDetail = true, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		[PXDBString(1)]
		[PXDefault(PMCommitmentType.Internal)]
		[PMCommitmentType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDefault]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		[AccountGroup()]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXRestrictor(typeof(Where<PMProject.nonProject, Equal<False>>), PM.Messages.NonProjectCodeIsInvalid)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[ProjectBase]
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
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ActiveOrInPlanningProjectTask(typeof(PMCommitment.projectID))]
		[PXForeignReference(typeof(Field<projectTaskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}
		public int? TaskID
		{
			get { return ProjectTaskID; }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDBInt()]
		[PMInventorySelector]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(SkipVerification = true)]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Nbr")]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PMUnit(typeof(PMCommitment.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region OrigQty
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBQuantity]
		[PXUIField(DisplayName = "Original Committed Quantity", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? OrigQty
		{
			get;
			set;
		}
		#endregion
		#region OrigAmount
		public abstract class origAmount : PX.Data.BQL.BqlDecimal.Field<origAmount> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Original Committed Amount", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? OrigAmount
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount")]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		protected Decimal? _ReceivedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Received Quantity")]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region InvoicedQty
		public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
		protected Decimal? _InvoicedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Quantity")]
		public virtual Decimal? InvoicedQty
		{
			get
			{
				return this._InvoicedQty;
			}
			set
			{
				this._InvoicedQty = value;
			}
		}
		#endregion
		#region InvoicedAmount
		public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
		protected Decimal? _InvoicedAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount")]
		public virtual Decimal? InvoicedAmount
		{
			get
			{
				return this._InvoicedAmount;
			}
			set
			{
				this._InvoicedAmount = value;
			}
		}
		#endregion
		#region InvoicedIsReadonly
		public abstract class invoicedIsReadonly : PX.Data.BQL.BqlBool.Field<invoicedIsReadonly> { }
		protected bool? _InvoicedIsReadonly = false;
		[PXBool]
		[PXUnboundDefault(false)]
		public virtual bool? InvoicedIsReadonly
		{
			get
			{
				return _InvoicedIsReadonly;
			}
			set
			{
				_InvoicedIsReadonly = value;
			}
		}
		#endregion
		#region CommittedCOQty
		public abstract class committedCOQty : PX.Data.BQL.BqlDecimal.Field<committedCOQty> { }
		[PXQuantity]
		[PXUIField(DisplayName = "Committed CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOQty
		{
			[PXDependsOnFields(typeof(qty), typeof(origQty))]
			get
			{
				return this.Qty.GetValueOrDefault() - this.OrigQty.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedCOAmount
		public abstract class committedCOAmount : PX.Data.BQL.BqlDecimal.Field<committedCOAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Committed CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOAmount
		{
			[PXDependsOnFields(typeof(amount), typeof(origAmount))]
			get
			{
				return this.Amount.GetValueOrDefault() - this.OrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region ProjectCuryID
		public abstract class projectCuryID : PX.Data.BQL.BqlString.Field<projectCuryID> { }
		[PXFormula(typeof(Selector<projectID, PMProject.curyID>))]
		[PXString(5, IsUnicode = true)]
		[PXSelector(typeof(CurrencyList.curyID))]
		[PXUIField(DisplayName = "Project Currency", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ProjectMultiCurrency))]
		public virtual string ProjectCuryID
		{
			get;
			set;
		}
		#endregion

		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity")]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region OpenAmount
		public abstract class openAmount : PX.Data.BQL.BqlDecimal.Field<openAmount> { }
		protected Decimal? _OpenAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount")]
		public virtual Decimal? OpenAmount
		{
			get
			{
				return this._OpenAmount;
			}
			set
			{
				this._OpenAmount = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXUIField(DisplayName = "Related Document")]
		[PXRefNote()]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		public class PXRefNoteAttribute : Common.PXRefNoteBaseAttribute
		{
			public PXRefNoteAttribute()
				: base()
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				PXButtonDelegate del = delegate (PXAdapter adapter)
				{
					PXCache cache = adapter.View.Graph.Caches[typeof(PMCommitment)];
					if (cache.Current != null)
					{
						object val = cache.GetValueExt(cache.Current, _FieldName);

						PXLinkState state = val as PXLinkState;
						if (state != null)
						{
							helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
						}
						else
						{
							helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
						}
					}

					return adapter.Get();
				};

				string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
				sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(CommitmentInquiry.ProjectBalanceFilter)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
			}
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
	public static class PMCommitmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Internal, External },
				new string[] { Messages.CommitmentType_Internal, Messages.CommitmentType_External })
			{ }


		}

		public const string Internal = "I";
		public const string External = "E";
		public class internalType : PX.Data.BQL.BqlString.Constant<internalType>
		{
			public internalType() : base(Internal) {; }
		}
		public class externalType : PX.Data.BQL.BqlString.Constant<externalType>
		{
			public externalType() : base(External) {; }
		}

	}
}
