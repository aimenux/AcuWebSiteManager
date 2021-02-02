using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.GraphExtensions.ExtendBAccount
{
	public abstract class ExtendToVendorGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		[PXHidden]
		public class SourceAccount : SourceAccount<ExtendToVendorGraph<TGraph, TPrimary>>
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

		public PXAction<TPrimary> extendToVendor;
		[PXUIField(DisplayName = PX.Objects.AR.Messages.ExtendToVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToVendor(PXAdapter adapter)

		{
			SourceAccount source = this.Accounts.Current;
			BAccount bacct = (BAccount)this.Accounts.Cache.GetMain(source);			
			if (source != null &&
			    source.Type != BAccountType.VendorType &&
			    source.Type != BAccountType.CombinedType)
				
			{
				Base.Actions["Save"].Press();
				AP.VendorMaint graph = PXGraph.CreateInstance<AP.VendorMaint>();

				// Need to take TimeStamp from this because Current may have outdated tstamp
				// after PXUpdates in this.Persist().
				graph.TimeStamp = Base.TimeStamp;
				AP.VendorR vendor = (AP.VendorR)graph.BAccount.Cache.Extend<BAccount>(bacct);
				graph.BAccount.Current = vendor;
				vendor.NoteID = source.NoteID;
				vendor.CreatedByID = bacct.CreatedByID;
				vendor.Type = source.Type == BAccountType.CustomerType
					? BAccountType.CombinedType
					: BAccountType.VendorType;
				vendor.LocaleName = source.LocaleName;
				string locationType = source.Type == BAccountType.CustomerType
					? LocTypeList.CombinedLoc
					: LocTypeList.VendorLoc;

				var defLocationExt = graph.GetExtension<AP.VendorMaint.DefLocationExt>();
				CRLocation defLocation = defLocationExt.DefLocation.Select();
				defLocationExt.DefLocation.Cache.RaiseRowSelected(defLocation);

				CRLocation defaultLocation = CRLocation.PK.Find(Base, bacct.DefLocationID);
				if (bacct.IsBranch != true
					&& defLocation != null
					&& defaultLocation.CTaxZoneID != null)
				{
					defLocationExt.DefLocation.Cache.SetValueExt<Location.vTaxZoneID>(defLocation, defaultLocation.CTaxZoneID);
				}

				defLocationExt.InitLocation(defLocation, locationType, bacct.IsBranch == true);
				defLocation = defLocationExt.DefLocation.Update(defLocation);

				var locationDetails = graph.GetExtension<AP.VendorMaint.LocationDetailsExt>();
				foreach (CRLocation iLoc in locationDetails.Locations.Select())
				{
					if (iLoc.LocationID != defLocation.LocationID)
					{
						defLocationExt.InitLocation(iLoc, locationType, bacct.IsBranch == true);
						locationDetails.Locations.Update(iLoc);
					}
				}

				if (!Base.IsContractBasedAPI)
					throw new PXRedirectRequiredException(graph, CR.Messages.EditVendor);

				graph.Save.Press();
			}
			return adapter.Get();
		}
		public PXAction<TPrimary> viewVendor;
		[PXUIField(DisplayName = CR.Messages.ViewVendor, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			SourceAccount bacct = this.Accounts.Current;
			if (bacct != null && (bacct.Type == BAccountType.VendorType || bacct.Type == BAccountType.CombinedType))
			{
				Base.Actions["Save"].Press();
				AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AP.VendorR.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, CR.Messages.EditVendor);
			}
			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<TPrimary> e)
		{
			PXEntryStatus baccountStatus = e.Cache.GetStatus(e.Row);
			SourceAccount source = Accounts.Current;
			if (source == null) return;

			BAccount baccount = (BAccount)Accounts.Cache.GetMain(source);

			bool isCustomerPersisted = true;
			if(baccountStatus != PXEntryStatus.Inserted && baccount?.Type == BAccountType.CustomerType)
			{
				CA.Light.Customer customer = SelectFrom<CA.Light.Customer>
					.Where<CA.Light.Customer.bAccountID.IsEqual<@P.AsInt>>
					.View
					.ReadOnly
					.SelectSingleBound(Base, null, baccount.BAccountID);
				isCustomerPersisted = customer != null;
			}

			extendToVendor.SetEnabled(
				!(source.Type == BAccountType.OrganizationType 
					|| source.Type == BAccountType.VendorType 
					|| source.Type == BAccountType.CombinedType) 
				&& baccountStatus != PXEntryStatus.Inserted
				&& isCustomerPersisted);
			viewVendor.SetEnabled((source.Type == BAccountType.VendorType || source.Type == BAccountType.CombinedType) && baccountStatus != PXEntryStatus.Inserted);
		}
	}

	public abstract class OrganizationUnitExtendToVendor<TGraph, TPrimary> : ExtendToVendorGraph<TGraph, TPrimary>
		where TGraph : PXGraph, IActionsMenuGraph
		where TPrimary : class, IBqlTable, new()
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.ActionsMenuItem.AddMenuAction(viewVendor, nameof(BranchMaint.ChangeID), false);
			Base.ActionsMenuItem.AddMenuAction(extendToVendor, nameof(BranchMaint.ChangeID), false);
		}
	}

}
