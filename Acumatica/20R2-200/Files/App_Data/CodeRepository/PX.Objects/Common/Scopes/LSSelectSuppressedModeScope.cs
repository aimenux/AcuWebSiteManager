using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.Common.Scopes
{
	/// <summary>
	/// Represents a scope for suppressing of the <see cref="LSSelect{TLSMaster,TLSDetail,Where}"/> major internal logic
	/// </summary>
	public sealed class LSSelectSuppressedModeScope<TLSMaster, TLSDetail, Where> : IDisposable
		where TLSMaster : class, IBqlTable, ILSPrimary, new()
		where TLSDetail : class, IBqlTable, ILSDetail, new()
		where Where : IBqlWhere, new()
	{
		private readonly bool _initSuppressedMode;
		private readonly LSSelect<TLSMaster, TLSDetail, Where> _lsselect;

		public LSSelectSuppressedModeScope(LSSelect<TLSMaster, TLSDetail, Where> lsselect)
		{
			_lsselect = lsselect;
			_initSuppressedMode = lsselect.SuppressedMode;
			
			lsselect.SuppressedMode = true;
		}

		void IDisposable.Dispose()
		{
			_lsselect.SuppressedMode = _initSuppressedMode;
		}
	}
}
