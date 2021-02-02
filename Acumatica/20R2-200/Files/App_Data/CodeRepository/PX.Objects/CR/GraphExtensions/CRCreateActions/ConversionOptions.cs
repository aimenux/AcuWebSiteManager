using PX.Data;
using System;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public class ConversionResult
	{
		// false if it was Existing, or conversion skipped for any reason
		// default true
		public bool Converted { get; set; } = true;
		internal PXGraph Graph { get; set; }
	}

	/// <exclude/>
	public class ConversionResult<TTarget> : ConversionResult
		where TTarget : class, IBqlTable, INotable, new()
	{
		public TTarget Entity { get; set; }
	}

	/// <exclude/>
	public abstract class ConversionOptions<TTargetGraph, TTarget>
		where TTargetGraph : PXGraph, new()
		where TTarget : class, IBqlTable, INotable, new()
	{
		// get currents from cache, and then return in in dispose
		public Func<IDisposable> HoldCurrentsCallback { get; set; }
	}

	/// <exclude/>
	public class ContactConversionOptions : ConversionOptions<ContactMaint, Contact>
	{
		public PXGraph GraphWithRelation { get; set; }
	}

	/// <exclude/>
	public class AccountConversionOptions : ConversionOptions<BusinessAccountMaint, BAccount>
	{
		public Contact ExistingContact { get; set; }
	}

	/// <exclude/>
	public class OpportunityConversionOptions : ConversionOptions<OpportunityMaint, CROpportunity>
	{
		// hack to skip automation changing OverrideRefContact field, or changed by Contact/Account creation
		public bool? ForceOverrideContact { get; set; }
	}
}
