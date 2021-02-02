using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PM
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMEmployeeRate)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMEmployeeRate : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		protected int? _RateDefinitionID;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateDefinitionID))]
		[PXParent(typeof(Select<PMRateSequence, Where<PMRateSequence.rateDefinitionID, Equal<Current<PMEmployeeRate.rateDefinitionID>>, And<PMRateSequence.rateCodeID, Equal<Current<PMEmployeeRate.rateCodeID>>>>>))]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateCodeID
		public abstract class rateCodeID : PX.Data.BQL.BqlString.Field<rateCodeID> { }
		protected String _RateCodeID;

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateCodeID))]
		public virtual String RateCodeID
		{
			get
			{
				return this._RateCodeID;
			}
			set
			{
				this._RateCodeID = value;
			}
		}
		#endregion
		#region EmployeeID

		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID")]
		[PXEPEmployeeSelectorAttribute]
		public virtual Int32? EmployeeID { get; set; }

		#endregion
	}
}
