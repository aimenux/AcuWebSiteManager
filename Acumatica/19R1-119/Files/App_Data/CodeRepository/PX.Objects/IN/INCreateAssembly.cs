using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class MyKitAssemblyEntry : KitAssemblyEntry
	{
		protected virtual void INComponentTran_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((INComponentTran)e.Row).SiteID != null)
			{
				e.NewValue = sender.Graph.Caches[typeof(INKitRegister)].GetValue<INKitRegister.locationID>(sender.Graph.Caches[typeof(INKitRegister)].Current);//(val = sender.Graph.Caches[typeof(INKitRegister)].GetValueExt<INKitRegister.locationID>(sender.Graph.Caches[typeof(INKitRegister)].Current)) is PXFieldState ? ((PXFieldState)val).Value : val;
				if (e.NewValue != null)
				{
					e.Cancel = true;
				}
			}
		}
	}

	public class INCreateAssembly : PXGraph<INCreateAssembly>
	{
		public PXCancel<INCreateAssemblyFilter> Cancel;
		public PXFilter<INCreateAssemblyFilter> Filter;
		public PXFilteredProcessingJoin<INLocationStatus, INCreateAssemblyFilter, 
			InnerJoin<InventoryItem, On<INLocationStatus.FK.InventoryItem>>, 
			Where<INLocationStatus.siteID, Equal<Current<INCreateAssemblyFilter.siteID>>, 
			And<INLocationStatus.locationID, Equal<Current<INCreateAssemblyFilter.locationID>>, 
			And<Add<INLocationStatus.qtyOnHand, INLocationStatus.qtyINAssemblySupply>, Less<decimal0>, 
			And<InventoryItem.stkItem, Equal<boolTrue>, 
			And<InventoryItem.kitItem, Equal<boolTrue>>>>>>> Records;

		public PXSetup<INSetup> insetup;

		public INCreateAssembly()
		{
			Records.SetProcessDelegate<MyKitAssemblyEntry>(ProcessRecord);

			object record = insetup.Current;
		}

		public static void ProcessRecord(MyKitAssemblyEntry graph, INLocationStatus record)
		{
			graph.Setup.Current.RequireControlTotal = false;
			graph.Setup.Current.HoldEntry = false;

			INKitRegister doc = PXCache<INKitRegister>.CreateCopy(graph.Document.Insert(new INKitRegister()));
			doc.KitInventoryID = record.InventoryID;
			doc.SiteID = record.SiteID;
			doc.LocationID = record.LocationID;
			doc.Qty = Math.Abs((decimal)(record.QtyOnHand + record.QtyINAssemblySupply));

			graph.Document.Update(doc);

			graph.Save.Press();
		}
	}

    [Serializable]
	public partial class INCreateAssemblyFilter : IBqlTable 
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site()]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INCreateAssemblyFilter.siteID))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
	}
}
