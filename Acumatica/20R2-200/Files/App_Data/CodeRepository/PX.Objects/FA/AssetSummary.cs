using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PO;

namespace PX.Objects.FA
{
    [PX.Objects.GL.TableAndChartDashboardType]
	public class AssetSummary : PXGraph<AssetSummary>
	{
		public PXFilter<AssetFilter> Filter;
		[PXFilterable]
		public PXSelectJoin<FixedAsset, LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
				LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FADetails.assetID>,
												And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>>>,
				Where<FixedAsset.recordType, Equal<FARecordType.assetType>, And<FixedAsset.status, NotEqual<FixedAssetStatus.reversed>>>> assets;

		public PXCancel<AssetFilter> Cancel;

		public PXFilter<DisposeParams> DispParams;

		public PXSetup<Company> company;

		protected virtual IEnumerable Assets()
		{
		   AssetFilter filter = Filter.Current;

		   assets.Cache.AllowInsert = false;
		   assets.Cache.AllowDelete = false;
		   assets.Cache.AllowUpdate = false;

		   PXSelectBase<FixedAsset> cmd = new PXSelectJoin<FixedAsset, 
			   LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>, 
			   LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FADetails.assetID>, 
												And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>>>,
				Where<FixedAsset.recordType, Equal<FARecordType.assetType>, And<FixedAsset.status, NotEqual<FixedAssetStatus.reversed>>>>(this);
		    
		   if (filter.ClassID != null)
		   {
		       cmd.WhereAnd<Where<FixedAsset.classID, Equal<Current<AssetFilter.classID>>>>();
		   }
			if (filter.AssetTypeID != null)
		   {
				cmd.WhereAnd<Where<FixedAsset.assetTypeID, Equal<Current<AssetFilter.assetTypeID>>>>();
		   }
		   if (filter.PropertyType != null)
		   {
		       cmd.WhereAnd<Where<FADetails.propertyType, Equal<Current<AssetFilter.propertyType>>>>();
		   }
		   if (filter.Condition != null)
		   {
		       cmd.WhereAnd<Where<FADetails.condition, Equal<Current<AssetFilter.condition>>>>();
		   }
		   if (filter.ReceiptNbr != null)
		   {
		       cmd.WhereAnd<Where<FADetails.receiptNbr, Equal<Current<AssetFilter.receiptNbr>>>>();
		   }
		   if (filter.PONumber != null)
		   {
		       cmd.WhereAnd<Where<FADetails.pONumber, Equal<Current<AssetFilter.pONumber>>>>();
		   }
		   if (filter.BillNumber != null)
		   {
		       cmd.WhereAnd<Where<FADetails.billNumber, Equal<Current<AssetFilter.billNumber>>>>();
		   }
		   if (filter.BranchID != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.locationID, Equal<Current<AssetFilter.branchID>>>>();
		   }
		   if (filter.BuildingID != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.buildingID, Equal<Current<AssetFilter.buildingID>>>>();
		   }
		   if (filter.Floor != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.floor, Equal<Current<AssetFilter.floor>>>>();
		   }
		   if (filter.Room != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.room, Equal<Current<AssetFilter.room>>>>();
		   }
		   if (filter.EmployeeID != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.employeeID, Equal<Current<AssetFilter.employeeID>>>>();
		   }
		   if (filter.Department != null)
		   {
		       cmd.WhereAnd<Where<FALocationHistory.department, Equal<Current<AssetFilter.department>>>>();
		   }
		   if (filter.AcqDateFrom != null)
		   {
			   cmd.WhereAnd<Where<FADetails.depreciateFromDate, GreaterEqual<Current<AssetFilter.acqDateFrom>>>>();
		   }
		   if (filter.AcqDateTo != null)
		   {
			   cmd.WhereAnd<Where<FADetails.depreciateFromDate, LessEqual<Current<AssetFilter.acqDateTo>>>>();
		   }
           int startRow = PXView.StartRow;
           int totalRows = 0;

           List<object> list = cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
           PXView.StartRow = 0;
           return list;
        }

		#region Button Dispose
		public PXAction<AssetFilter> Dispose;
		[PXUIField(DisplayName = "Dispose", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible=false)]
		[PXButton]
		public virtual IEnumerable dispose(PXAdapter adapter)
		{
			if (WebDialogResult.OK == DispParams.AskExt())
			{
				DisposeParams prm = DispParams.Current;

				//TODO: Implement dispose process
				throw new NotImplementedException();

			}
			return adapter.Get();
		}
		#endregion
	}

	[Serializable]
	public partial class AssetFilter:IBqlTable
	{
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		protected Int32? _ClassID;
		[PXDBInt]
		[PXSelector(typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
					SubstituteKey = typeof(FAClass.assetCD),
					DescriptionField = typeof(FAClass.description))]
		[PXUIField(DisplayName = "Asset Class")]
		public virtual Int32? ClassID
		{
			get
			{
				return _ClassID;
			}
			set
			{
				_ClassID = value;
			}
		}
		#endregion
		#region AssetTypeID
		public abstract class assetTypeID : PX.Data.BQL.BqlString.Field<assetTypeID> { }
		protected string _AssetTypeID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<FAType.assetTypeID>), DescriptionField = typeof(FAType.description))]
		[PXUIField(DisplayName = "Asset Type")]
		public virtual string AssetTypeID
		{
			get
			{
				return _AssetTypeID;
			}
			set
			{
				_AssetTypeID = value;
			}
		}
		#endregion
		#region PropertyType
		public abstract class propertyType : PX.Data.BQL.BqlString.Field<propertyType> { }
		protected String _PropertyType;
		[PXDBString(2, IsFixed = true)]
		[FADetails.propertyType.List]
		[PXUIField(DisplayName = "Property Type")]
		public virtual String PropertyType
		{
			get
			{
				return _PropertyType;
			}
			set
			{
				_PropertyType = value;
			}
		}
		#endregion
		#region Condition
		public abstract class condition : PX.Data.BQL.BqlString.Field<condition> { }
		protected String _Condition;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Condition")]
		[FADetails.condition.List]
		public virtual String Condition
		{
			get
			{
				return _Condition;
			}
			set
			{
				_Condition = value;
			}
		}
		#endregion

		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[POReceiptType.RefNbr(typeof(POReceipt.receiptNbr), Filterable = true)]
		[PXUIField(DisplayName = "Receipt Nbr.")]
		public virtual String ReceiptNbr
		{
			get
			{
				return _ReceiptNbr;
			}
			set
			{
				_ReceiptNbr = value;
			}
		}
		#endregion
		#region PONumber
		public abstract class pONumber : PX.Data.BQL.BqlString.Field<pONumber> { }
		protected String _PONumber;
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search4<FADetails.pONumber, Where<FADetails.pONumber, IsNotNull>, Aggregate<GroupBy<FADetails.pONumber>>>), typeof(FADetails.pONumber))]
		[PXUIField(DisplayName = "PO Number")]
		public virtual String PONumber
		{
			get
			{
				return _PONumber;
			}
			set
			{
				_PONumber = value;
			}
		}
		#endregion
		#region BillNumber
		public abstract class billNumber : PX.Data.BQL.BqlString.Field<billNumber> { }
		protected String _BillNumber;
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search4<FADetails.billNumber, Where<FADetails.billNumber, IsNotNull>, Aggregate<GroupBy<FADetails.billNumber>>>), typeof(FADetails.billNumber))]
		[PXUIField(DisplayName = "Bill Number")]
		public virtual String BillNumber
		{
			get
			{
				return _BillNumber;
			}
			set
			{
				_BillNumber = value;
			}
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch()]
        [PXRestrictor(typeof(Where<True, Equal<True>>), GL.Messages.BranchInactive, ReplaceInherited = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region BuildingID
		public abstract class buildingID : PX.Data.BQL.BqlInt.Field<buildingID> { }
		protected Int32? _BuildingID;
		[PXDBInt]
		[PXSelector(typeof(Search<Building.buildingID, Where<Building.branchID, Equal<Current<AssetFilter.branchID>>>>), SubstituteKey = typeof(Building.buildingCD))]
		[PXUIField(DisplayName = "Building")]
		public virtual Int32? BuildingID
		{
			get
			{
				return _BuildingID;
			}
			set
			{
				_BuildingID = value;
			}
		}
		#endregion
		#region Floor
		public abstract class floor : PX.Data.BQL.BqlString.Field<floor> { }
		protected String _Floor;
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Floor")]
		public virtual String Floor
		{
			get
			{
				return _Floor;
			}
			set
			{
				_Floor = value;
			}
		}
		#endregion
		#region Room
		public abstract class room : PX.Data.BQL.BqlString.Field<room> { }
		protected String _Room;
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Room")]
		public virtual String Room
		{
			get
			{
				return _Room;
			}
			set
			{
				_Room = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected int? _EmployeeID;
		[PXDBInt]
		[PXSelector(typeof(EPEmployee.bAccountID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Custodian")]
		public virtual int? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
		#endregion
		#region Department
		public abstract class department : PX.Data.BQL.BqlString.Field<department> { }
		protected String _Department;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEmployee.departmentID, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>))]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department", Required = false)]
		public virtual String Department
		{
			get
			{
				return _Department;
			}
			set
			{
				_Department = value;
			}
		}
		#endregion

		#region AcqDateFrom
		public abstract class acqDateFrom : PX.Data.BQL.BqlDateTime.Field<acqDateFrom> { }
		protected DateTime? _AcqDateFrom;
		[PXDate]
		[PXUIField(DisplayName = "Placed-in-Service Date From")]
		public virtual DateTime? AcqDateFrom
		{
			get
			{
				return _AcqDateFrom;
			}
			set
			{
				_AcqDateFrom = value;
			}
		}
		#endregion
		#region AcqDateTo
		public abstract class acqDateTo : PX.Data.BQL.BqlDateTime.Field<acqDateTo> { }
		protected DateTime? _AcqDateTo;
		[PXDate]
		[PXUIField(DisplayName = "To")]
		public virtual DateTime? AcqDateTo
		{
			get
			{
				return _AcqDateTo;
			}
			set
			{
				_AcqDateTo = value;
			}
		}
		#endregion
	}
}