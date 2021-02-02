using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CT;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.PMLaborCostRate)]
	[System.SerializableAttribute()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMLaborCostRate : PX.Data.IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1)]
		[PXDefault]
		[PMLaborCostRateType.List]
		[PXUIField(DisplayName = "Labor Rate Type")]
		public virtual string Type
		{
			get; set;
		}
		#endregion
		#region UnionID
		public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PXRestrictor(typeof(Where<PMUnion.isActive, Equal<True>>), Messages.InactiveUnion, typeof(PMUnion.unionID))]
		[PXSelector(typeof(Search<PMUnion.unionID>))]
		[PXDBString(PMUnion.unionID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Union Local")]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), WarnIfCompleted = false)]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		
		[ProjectTask(typeof(projectID), AllowNull = true)]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[EP.PXEPEmployeeSelector]
		[PXDBInt()]
		[PXUIField(DisplayName = "Employee")]
		[PXForeignReference(typeof(Field<employeeID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual Int32? EmployeeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Labor Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region EmploymentType
		public abstract class employmentType : PX.Data.BQL.BqlString.Field<employmentType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(EP.RateTypesAttribute.Hourly)]
		[PXUIField(DisplayName = "Type of Employment")]
		[EP.RateTypes]
		public virtual string EmploymentType
		{
			get;
			set;
		}
		#endregion
		#region RegularHours
		public abstract class regularHours : PX.Data.BQL.BqlDecimal.Field<regularHours> { }
		[PXDBDecimal(1)]
		[PXUIField(DisplayName = "Regular Hours per week")]
		public virtual decimal? RegularHours
		{
			get;
			set;
		}
		#endregion
		#region AnnualSalary
		public abstract class annualSalary : PX.Data.BQL.BqlDecimal.Field<annualSalary> { }
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Annual Rate")]
		public virtual decimal? AnnualSalary
		{
			get;
			set;
		}
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		[PXDefault]
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date")]
		public virtual DateTime? EffectiveDate
		{
			get;
			set;
		}
        #endregion

        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        [PXString(6, IsUnicode = true)]       
        [PXUIField(Visible = false, IsReadOnly = true)]
        public virtual String UOM
        {
            get;
            set;
        }
        #endregion

        #region Rate
        public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		protected decimal? _Rate;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Rate")]
		public virtual decimal? Rate
		{
			get
			{
				return this._Rate;
			}
			set
			{
				this._Rate = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXDefault(typeof(Search<GL.Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Nbr")]
		public virtual String ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMLaborCostRateType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(GetListBasedOnFeatures().ToArray())
			{ }

			public static List<Tuple<string, string>> GetListBasedOnFeatures()
			{
				List<Tuple<string, string>> list = new List<Tuple<string, string>>();
				list.Add(Pair(Employee, Messages.CostRateType_Employee));
				if (PXAccess.FeatureInstalled<FeaturesSet.construction>() || PXAccess.FeatureInstalled<FeaturesSet.payrollModule>())
				{
					list.Add(Pair(Union, Messages.CostRateType_Union));
					list.Add(Pair(Certified, Messages.CostRateType_Certified));
				}
				list.Add(Pair(Project, Messages.CostRateType_Project));
				list.Add(Pair(Item, Messages.CostRateType_Item));

				return list;
			}
		}

		public class FilterListAttribute : PXStringListAttribute
		{
			public FilterListAttribute() : base(GetListBasedOnFeatures().ToArray())
			{ }

			public static List<Tuple<string, string>> GetListBasedOnFeatures()
			{
				List<Tuple<string, string>> list = new List<Tuple<string, string>>();
				list.Add(Pair(All, Messages.CostRateType_All));
				list.Add(Pair(Employee, Messages.CostRateType_Employee));
				if (PXAccess.FeatureInstalled<FeaturesSet.construction>() || PXAccess.FeatureInstalled<FeaturesSet.payrollModule>())
				{
					list.Add(Pair(Union, Messages.CostRateType_Union));
					list.Add(Pair(Certified, Messages.CostRateType_Certified));
				}
				if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
				{
					list.Add(Pair(Project, Messages.CostRateType_Project));
				}
				list.Add(Pair(Item, Messages.CostRateType_Item));

				return list;
			}
		}

		public const string All = "A";
		public const string Employee = "E";
		public const string Union = "U";
		public const string Certified = "C";
		public const string Project = "P";
		public const string Item = "I";

		public class union : PX.Data.BQL.BqlString.Constant<union>
		{
			public union() : base(Union) {; }
		}
		public class certified : PX.Data.BQL.BqlString.Constant<certified>
		{
			public certified() : base(Certified) {; }
		}
		public class item : PX.Data.BQL.BqlString.Constant<item>
		{
			public item() : base(Item) {; }
		}
		public class employee : PX.Data.BQL.BqlString.Constant<employee>
		{
			public employee() : base(Employee) {; }
		}
		public class all : PX.Data.BQL.BqlString.Constant<all>
		{
			public all() : base(All) {; }
		}
	}
}
