namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLConsolAccount)]
	public partial class GLConsolAccount : PX.Data.IBqlTable
	{
		#region AccountCD
		public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
		protected String _AccountCD;
		[PXDefault()]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDimension(AccountAttribute.DimensionName)]
		public virtual String AccountCD
		{
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
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
