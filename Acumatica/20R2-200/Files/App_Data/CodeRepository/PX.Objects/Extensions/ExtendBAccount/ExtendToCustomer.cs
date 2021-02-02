using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.GraphExtensions.ExtendBAccount
{		
	public abstract class ExtendToCustomerGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		[PXHidden]
		public class SourceAccount : SourceAccount<ExtendToCustomerGraph<TGraph, TPrimary>>
		{
		}
		/// <summary>A class that defines the default mapping of the <see cref="Document" /> mapped cache extension to a DAC.</summary>
		protected class SourceAccountMapping : IBqlMapping
		{
			/// <exclude />
			protected Type _extension = typeof(SourceAccount);
			/// <exclude />
			public Type Extension => _extension;

			/// <exclude />
			protected Type _table;
			/// <exclude />
			public Type Table => _table;

			/// <summary>Creates the default mapping of the <see cref="SourceAccount" /> mapped cache extension to the specified table.</summary>
			/// <param name="table">A DAC.</param>
			public SourceAccountMapping(Type table)
			{
				_table = table;
			}
			/// <exclude />
			public Type AcctCD = typeof(SourceAccount.acctCD);
			/// <exclude />
			public Type Type = typeof(SourceAccount.type);
			/// <exclude />
			public Type LocaleName = typeof(SourceAccount.localeName);

		}
		/// <summary>Returns the mapping of the <see cref="SourceAccount" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
		/// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
		/// <example>
		///   <code title="Example" description="The following code shows the method that overrides the GetSourceAccountMapping() method in the implementation class. The method returns the default mapping of the SourceAccount mapped cache extension to the CROpportunity DAC." lang="CS">
		/// protected override SourceAccountMapping GetSourceAccountMapping()
		/// {
		///       return new SourceAccountMapping(typeof(CROpportunity));
		/// }</code>
		/// </example>
		protected abstract SourceAccountMapping GetSourceAccountMapping();
		/// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
		public PXSelectExtension<SourceAccount> Accounts;

		public PXAction<TPrimary> extendToCustomer;
		[PXUIField(DisplayName = AP.Messages.ExtendToCustomer, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToCustomer(PXAdapter adapter)
		{
			SourceAccount source = this.Accounts.Current;
			BAccount bacct = (BAccount)this.Accounts.Cache.GetMain(source);
			if (source != null && 
			    source.Type != BAccountType.CustomerType && 
			    source.Type != BAccountType.CombinedType)
			{
				Base.Actions["Save"].Press();
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				bacct.COrgBAccountID = null; //for invoke defaulting in Extend<>
				Customer customer = (Customer)graph.BAccount.Cache.Extend<BAccount>(bacct);
				graph.BAccount.Current = customer;
				customer.NoteID = source.NoteID;
				customer.CreatedByID = bacct.CreatedByID;

				customer.Type = source.Type == BAccountType.VendorType
					? BAccountType.CombinedType
					: BAccountType.CustomerType;
				string locationType = source.Type == BAccountType.VendorType
					? LocTypeList.CombinedLoc
					: LocTypeList.CustomerLoc;

				customer.LocaleName = source?.LocaleName;
				var defLocationExt = graph.GetExtension<AR.CustomerMaint.DefLocationExt>();
				CRLocation defLocation = defLocationExt.DefLocation.Select();
				defLocationExt.DefLocation.Cache.RaiseRowSelected(defLocation);

				if (defLocation.CTaxZoneID == null)
				{
					defLocationExt.DefLocation.Cache.SetDefaultExt<CRLocation.cTaxZoneID>(defLocation);
				}
				defLocationExt.InitLocation(defLocation, locationType, bacct.IsBranch == true);
				defLocation = defLocationExt.DefLocation.Update(defLocation);

				var locationDetails = graph.GetExtension<AR.CustomerMaint.LocationDetailsExt>();
				foreach (CRLocation iLoc in locationDetails.Locations.Select())
				{
					if (iLoc.LocationID != defLocation.LocationID)
					{
						defLocationExt.InitLocation(iLoc, locationType, bacct.IsBranch == true);
						locationDetails.Locations.Update(iLoc);
					}
				}
				graph.Caches[typeof(CSAnswers)].Clear();
				graph.Caches[typeof(CSAnswers)].ClearQueryCache();

				if (!Base.IsContractBasedAPI)
					throw new PXRedirectRequiredException(graph, "Edit Customer");

				graph.Save.Press();
			}
			return adapter.Get();
		}
		public PXAction<TPrimary> viewCustomer;
		[PXUIField(DisplayName = AR.Messages.ViewCustomer, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			SourceAccount source = this.Accounts.Current;
			if (source != null && (source.Type == BAccountType.CustomerType || source.Type == BAccountType.CombinedType))
			{
				Base.Actions["Save"].Press();
				AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AR.Customer.acctCD>(source.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Customer");
			}
			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<TPrimary> e)
		{
			PXEntryStatus baccountStatus = e.Cache.GetStatus(e.Row);
			SourceAccount source = Accounts.Current;
			if (source == null) return;

			BAccount baccount = (BAccount)Accounts.Cache.GetMain(source);

			bool isVendorPersisted = true;
			if (baccountStatus != PXEntryStatus.Inserted && baccount?.Type == BAccountType.VendorType)
			{
				CA.Light.Vendor vendor = SelectFrom<CA.Light.Vendor>
					.Where<CA.Light.Vendor.bAccountID.IsEqual<@P.AsInt>>
					.View
					.ReadOnly
					.SelectSingleBound(Base, null, baccount.BAccountID);
				isVendorPersisted = vendor != null;
			}

			extendToCustomer.SetEnabled(
				!(source.Type == BAccountType.OrganizationType
					|| source.Type == BAccountType.CustomerType
					|| source.Type == BAccountType.CombinedType)
				&& baccountStatus != PXEntryStatus.Inserted
				&& isVendorPersisted);
			viewCustomer.SetEnabled((source.Type == BAccountType.CustomerType || source.Type == BAccountType.CombinedType) && baccountStatus != PXEntryStatus.Inserted);

		}
	}

	public abstract class OrganizationUnitExtendToCustomer<TGraph, TPrimary> : ExtendToCustomerGraph<TGraph, TPrimary>
	where TGraph : PXGraph, IActionsMenuGraph
	where TPrimary : class, IBqlTable, new()
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.ActionsMenuItem.AddMenuAction(viewCustomer, nameof(BranchMaint.ChangeID), false);
			Base.ActionsMenuItem.AddMenuAction(extendToCustomer, nameof(BranchMaint.ChangeID), false);
		}
	}
}
