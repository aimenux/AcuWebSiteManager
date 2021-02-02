using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	[PXPrimaryGraph(new Type[] { typeof(EmployeeMaint) },
			new Type[] { typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPContractRate.employeeID>>>>) }
		)]
	[PXCacheName(Messages.EPContractRate)]
    [Serializable]
	public partial class EPContractRate : IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXParent(typeof(Select<EPEmployeeContract, Where<EPEmployeeContract.employeeID, Equal<Current<EPContractRate.employeeID>>, And<EPEmployeeContract.contractID, Equal<Current<EPContractRate.contractID>>>>>))]
		[PXCheckUnique(Where = typeof(Where<EPContractRate.earningType, Equal<Current<EPContractRate.earningType>>, And<EPContractRate.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXDBDefault(typeof(EPEmployeeContract.employeeID))]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region EarningType
		public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }
		protected string _EarningType;
		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
		[PXDefault()]
		[PXRestrictor(typeof(Where<EP.EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EP.EPEarningType.typeCD))]
		[PXSelector(typeof(EP.EPEarningType.typeCD))]
		[PXCheckUnique(Where = typeof(Where<EPContractRate.employeeID, Equal<Current<EPContractRate.employeeID>>, And<EPContractRate.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType
		{
			get
			{
				return this._EarningType;
			}
			set
			{
				this._EarningType = value;
			}

		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;

		[PXDBInt()]
		[PXCheckUnique(Where = typeof(Where<EPContractRate.employeeID, Equal<Current<EPContractRate.employeeID>>, And<EPContractRate.earningType, Equal<Current<EPContractRate.earningType>>>>))]
		[PXDBDefault(typeof(EPEmployeeContract.contractID))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		protected Int32? _LabourItemID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Labor Item")]
		[PXDefault]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? LabourItemID
		{
			get
			{
				return this._LabourItemID;
			}
			set
			{
				this._LabourItemID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedDateTime()]
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

		public static int? GetContractLaborClassID(PXGraph graph, PMTimeActivity activity)
		{
			EPContractRate matrix =
				PXSelectJoin<
					EPContractRate, 
				InnerJoin<CRCase, 
					On<CRCase.contractID, Equal<EPContractRate.contractID>>, 
				InnerJoin<CRActivityLink,
					On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>,
				InnerJoin<EPEmployee,
					On<EPContractRate.employeeID, Equal<EPEmployee.bAccountID>>>>>, 
				Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>, 
					And<EPContractRate.earningType, Equal<Required<PMTimeActivity.earningTypeID>>, 
					And<EPEmployee.userID, Equal<Required<PMTimeActivity.ownerID>>>>>>
				.Select(graph, new object[] {activity.RefNoteID, activity.EarningTypeID, activity.OwnerID});

			if (matrix == null)
				matrix =
					PXSelectJoin<
						EPContractRate, 
					InnerJoin<CRCase, 
						On<CRCase.contractID, Equal<EPContractRate.contractID>>,
					InnerJoin<CRActivityLink,
						On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>>, 
					Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>, 
						And<EPContractRate.earningType, Equal<Required<PMTimeActivity.earningTypeID>>, 
						And<EPContractRate.employeeID, IsNull>>>>
					.Select(graph, new object[] {activity.RefNoteID, activity.EarningTypeID});

			if (matrix != null)
				return matrix.LabourItemID;
			else
				return null;
		}

		public static int? GetProjectLaborClassID(PXGraph graph, int projectID, int employeeID, string earningType)
		{
			EPContractRate projectSettings = PXSelect
				<EPContractRate, Where<EPContractRate.contractID, Equal<Required<EPContractRate.contractID>>,
					And
						<EPContractRate.employeeID, Equal<Required<EPContractRate.employeeID>>,
							And<EPContractRate.earningType, Equal<Required<EPEarningType.typeCD>>>>>>.Select(graph, projectID, employeeID, earningType);

			return projectSettings != null ? projectSettings.LabourItemID : null;
		}

		public static void UpdateKeyFields(PXGraph graph, int? oldProjectID, int? oldEmployeeID, int? newProjectID, int? newEmployeeID)
		{
			PXResultset<EPContractRate> res = PXSelect<EPContractRate
									, Where<EPContractRate.contractID, Equal<Required<EPContractRate.contractID>>
										, And<EPContractRate.employeeID, Equal<Required<EPContractRate.employeeID>>>
										>
									>.Select(graph, oldProjectID, oldEmployeeID);
			foreach (EPContractRate contractRate in res)
			{
				contractRate.ContractID = newProjectID;
				contractRate.EmployeeID = newEmployeeID;
				graph.Caches[typeof (EPContractRate)].Update(contractRate);
			}
		}

	}
}
