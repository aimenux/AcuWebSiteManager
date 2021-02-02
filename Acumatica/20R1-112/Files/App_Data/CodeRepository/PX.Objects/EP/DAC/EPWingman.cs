using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.Wingman)]
	public partial class EPWingman : IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return _RecordID;
			}
			set
			{
				_RecordID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXDBDefault(typeof(EPEmployee.bAccountID))]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPWingman.employeeID>>>>))]
		public Int32? EmployeeID
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
		#region WingmanID
		public abstract class wingmanID : PX.Data.BQL.BqlInt.Field<wingmanID> { }
		protected Int32? _WingmanID;
		[PXDBInt()]
		[PXRestrictor(typeof(Where<EPEmployee.bAccountID, NotEqual<Current<EPWingman.employeeID>>>), null)]
		[PXEPEmployeeSelector]
		[PXCheckUnique(typeof(employeeID))]
		[PXUIField(DisplayName = "Delegate")]
		public Int32? WingmanID
		{
			get
			{
				return _WingmanID;
			}
			set
			{
				_WingmanID = value;
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
		[PXDBCreatedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}

	public class WingmanUser<UserID> : IBqlComparison
		where UserID : IBqlOperand, new()
	{
		private IBqlCreator _operand;
		private const string EMPLOYEERALIAS = "EMPLOYEERALIAS";
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = null;
			value = null;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			SQLExpression userID = null;
			status &= BqlCommand.AppendExpression<UserID>(ref userID, graph, info, selection, ref _operand);

			if (graph == null || !info.BuildExpression) return status;
			
			SimpleTable epEmployee = new SimpleTable<EPEmployee>(EMPLOYEERALIAS);

			exp = exp.In(new Query()
				.Select<EPWingman.employeeID>().From<EPWingman>()
					.InnerJoin(epEmployee)
					.On(new Column<EPEmployee.bAccountID>(epEmployee).EQ(new Column(typeof(EPWingman.wingmanID)))
						.And(new Column<EPEmployee.userID>(epEmployee).EQ(userID)))
					.Where(new SQLConst(1)
						.EQ(new SQLConst(1))));

			return status;
		}
	}
	
}
