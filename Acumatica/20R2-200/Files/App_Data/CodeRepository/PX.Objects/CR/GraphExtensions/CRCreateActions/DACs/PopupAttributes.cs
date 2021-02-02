using System;
using PX.Data;
using PX.Data.MassProcess;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXHidden]
	[Serializable]
	[PXBreakInheritance]
	public class PopupAttributes : FieldValue
	{
		#region CacheName
		public new abstract class cacheName : PX.Data.BQL.BqlString.Field<cacheName> { }

		[PXString(IsKey = true)]
		public override string CacheName { get; set; }
		#endregion
	}
}