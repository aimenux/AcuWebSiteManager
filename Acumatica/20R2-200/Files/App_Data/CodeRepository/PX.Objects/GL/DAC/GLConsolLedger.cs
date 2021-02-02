namespace PX.Objects.GL
{
	using System;
	using PX.Data;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLConsolLedger)]
	public partial class GLConsolLedger : PX.Data.IBqlTable
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
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
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
		#region PostInterCompany
		public abstract class postInterCompany : PX.Data.BQL.BqlBool.Field<postInterCompany> { }
		protected Boolean? _PostInterCompany;

		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		[PXDBBool()]
		[PXUIField(DisplayName = "Generates Inter-Branch Transactions", Enabled = false)]
		public virtual Boolean? PostInterCompany
		{
			get
			{
				return this._PostInterCompany;
			}
			set
			{
				this._PostInterCompany = value;
			}
		}
		#endregion
		#region BalanceType
		public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }

		/// <summary>
		/// The type of balance of the ledger in the source company.
		/// </summary>
		/// <value>
		/// For more info see the <see cref="Ledger.BalanceType"/> field.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[LedgerBalanceType.List]
		[PXUIField(DisplayName = "Balance Type", Enabled = false)]
		public virtual String BalanceType { get; set; }
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


	[PXProjection(typeof(Select<GLConsolLedger>))]
	public class GLConsolLedger2 : IBqlTable
	{
		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlInt.Field<setupID> { }
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true, BqlField = typeof(GLConsolLedger.setupID))]
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
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(GLConsolLedger.ledgerCD))]
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
		#region BalanceType
		public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(GLConsolLedger.balanceType))]
		public virtual String BalanceType { get; set; }
		#endregion
	}
}
