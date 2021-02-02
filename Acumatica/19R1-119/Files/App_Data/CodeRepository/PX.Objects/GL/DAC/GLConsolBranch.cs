using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.GL
{
	[Serializable()]
	[PXCacheName(Messages.GLConsolBranch)]
	public partial class GLConsolBranch : IBqlTable 
	{
		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlInt.Field<setupID> { }
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region BranchCD
		public abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
		protected String _BranchCD;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")] //InputMask = "" for using dash ("-") character
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual String BranchCD
		{
			get
			{
				return this._BranchCD;
			}
			set
			{
				this._BranchCD = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual String LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
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
	}
}
