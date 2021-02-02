using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Web.Compilation;
using PX.Objects.IN;
using PX.SM;
using PX.Data.Update;
using System.Security.Permissions;
using System.Reflection;
using PX.CarrierService;

namespace PX.Objects.CS
{
	public class CarrierMaint : PXGraph<CarrierMaint, Carrier>
	{
		public PXSelect<Carrier> Carrier;
		public PXSelect<Carrier, Where<Carrier.carrierID, Equal<Current<Carrier.carrierID>>>> CarrierCurrent;
		public PXSelect<FreightRate, Where<FreightRate.carrierID, Equal<Current<Carrier.carrierID>>>> FreightRates;
		public PXSelectJoin<CarrierPackage, 
			InnerJoin<CSBox, On<CSBox.boxID, Equal<CarrierPackage.boxID>> ,CrossJoin<CommonSetup>>,           
			Where<CarrierPackage.carrierID, Equal<Current<Carrier.carrierID>>>> CarrierPackages;
		public PXSelect<CSBox, Where<CSBox.activeByDefault, Equal<boolTrue>>> DefaultBoxes;
		

		
		protected virtual void FreightRate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FreightRate doc = (FreightRate)e.Row;

			if (doc.Weight < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.weight>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, typeof(FreightRate.weight).Name)))
				{
					throw new PXRowPersistingException(typeof(FreightRate.weight).Name, null, Messages.FieldShouldNotBeNegative, typeof(FreightRate.weight).Name);
				}
				e.Cancel = true;
			}
			if (doc.Volume < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.volume>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, typeof(FreightRate.volume).Name)))
				{
					throw new PXRowPersistingException(typeof(FreightRate.volume).Name, null, Messages.FieldShouldNotBeNegative, typeof(FreightRate.volume).Name);
				}
				e.Cancel = true;
			}
			if (doc.Rate < 0)
			{
				if (sender.RaiseExceptionHandling<FreightRate.rate>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, typeof(FreightRate.rate).Name)))
				{
					throw new PXRowPersistingException(typeof(FreightRate.rate).Name, null, Messages.FieldShouldNotBeNegative, typeof(FreightRate.rate).Name);
				}
				e.Cancel = true;
			}
		}

		protected virtual void Carrier_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Carrier row = e.Row as Carrier;
			if (row != null)
			{
				foreach (CSBox box in DefaultBoxes.Select())
				{
					CarrierPackage package = new CarrierPackage();
					package.CarrierID = row.CarrierID;
					package.BoxID = box.BoxID;

					CarrierPackages.Insert(package);
				}
				CarrierPackages.Cache.IsDirty = false;
			}
		}
				
		protected virtual void Carrier_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Carrier doc = (Carrier)e.Row;

			if (doc.BaseRate < 0)
			{
				if (sender.RaiseExceptionHandling<Carrier.baseRate>(e.Row, null, new PXSetPropertyException(Messages.FieldShouldNotBeNegative, typeof(Carrier.baseRate).Name)))
				{
					throw new PXRowPersistingException(typeof(Carrier.baseRate).Name, null, Messages.FieldShouldNotBeNegative, typeof(Carrier.baseRate).Name);
				}
				e.Cancel = true;
			}

			if (doc.IsExternal == true)
			{
				if (string.IsNullOrEmpty(doc.CarrierPluginID))
				{
					if (sender.RaiseExceptionHandling<Carrier.carrierPluginID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Carrier.carrierPluginID).Name)))
					{
						throw new PXRowPersistingException(typeof(Carrier.carrierPluginID).Name, null, ErrorMessages.FieldIsEmpty, typeof(Carrier.carrierPluginID).Name);
					}
					e.Cancel = true;
				}

				if (string.IsNullOrEmpty(doc.PluginMethod))
				{
					if (sender.RaiseExceptionHandling<Carrier.pluginMethod>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Carrier.pluginMethod).Name)))
					{
						throw new PXRowPersistingException(typeof(Carrier.pluginMethod).Name, null, ErrorMessages.FieldIsEmpty, typeof(Carrier.pluginMethod).Name);
					}
					e.Cancel = true;
				}
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.advancedFulfillment>() && doc.IsExternalShippingApplication == true && String.IsNullOrEmpty(doc.ShippingApplicationType))
			{
				if (sender.RaiseExceptionHandling<Carrier.shippingApplicationType>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Carrier.shippingApplicationType).Name)))
				{
					throw new PXRowPersistingException(typeof(Carrier.shippingApplicationType).Name, null, ErrorMessages.FieldIsEmpty, typeof(Carrier.shippingApplicationType).Name);
				}
			}
		}

        protected virtual void Carrier_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row != null)
            {
                PXUIFieldAttribute.SetVisible<Carrier.calcMethod>(sender, row, row.IsExternal != true);
                PXUIFieldAttribute.SetVisible<Carrier.baseRate>(sender, row, row.IsExternal != true);
				PXUIFieldAttribute.SetEnabled<Carrier.calcFreightOnReturn>(sender, row, row.IsExternal != true);

				PXUIFieldAttribute.SetVisible<Carrier.carrierPluginID>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.pluginMethod>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.confirmationRequired>(sender, row, row.IsExternal == true);
                PXUIFieldAttribute.SetVisible<Carrier.packageRequired>(sender, row, row.IsExternal == true);

				PXUIFieldAttribute.SetEnabled<Carrier.baseRate>(sender, row, row.CalcMethod != CarrierCalcMethod.Manual);

				if (PXAccess.FeatureInstalled<FeaturesSet.advancedFulfillment>())
	            {
		            // Shipping application integration is mutually exclusive with external carrier plug-in. You can't use both at the same time.
		            PXUIFieldAttribute.SetEnabled<Carrier.isExternal>(sender, row, row.IsExternalShippingApplication == false);
		            PXUIFieldAttribute.SetVisible<Carrier.returnLabel>(sender, row, row.IsExternal == true);
		            PXUIFieldAttribute.SetEnabled<Carrier.isExternalShippingApplication>(sender, row, row.IsExternal == false);
		            PXUIFieldAttribute.SetEnabled<Carrier.shippingApplicationType>(sender, row, row.IsExternal == false);
		            PXUIFieldAttribute.SetEnabled<Carrier.shippingApplicationType>(sender, row, row.IsExternalShippingApplication == true);
				}
            }
        }

        protected virtual void Carrier_CarrierPluginID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row == null) return;
            row.PluginMethod = null;
        }

        protected virtual void Carrier_CalcMethod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Carrier row = e.Row as Carrier;
            if (row.CalcMethod == CarrierCalcMethod.Manual) 
                row.BaseRate = 0;
        }

		protected virtual void Carrier_IsExternal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Carrier row = e.Row as Carrier;
			if (row != null)
				sender.SetDefaultExt<Carrier.calcFreightOnReturn>(row);
		}

		public static ICarrierService CreateCarrierService(PXGraph graph, string carrierID)
		{
			if (string.IsNullOrEmpty(carrierID))
				throw new ArgumentNullException();

			ICarrierService service = null;
			Carrier carrier = CS.Carrier.PK.Find(graph, carrierID);
			if (carrier != null && carrier.IsExternal == true && !string.IsNullOrEmpty(carrier.CarrierPluginID))
			{
				service = CarrierPluginMaint.CreateCarrierService(graph, carrier.CarrierPluginID);
                service.Method = carrier.PluginMethod;

            }

			return service;
		}
	}

	
    [Serializable]
	public class CarrierMethodSelectorAttribute : PXCustomSelectorAttribute
	{
		public CarrierMethodSelectorAttribute()
			: base(typeof(CarrierPluginMethod.code))
		{
			DescriptionField = typeof(CarrierPluginMethod.description);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = this._Graph.Caches[typeof(Carrier)];
			Carrier row = cache.Current as Carrier;
			if (row != null && row.IsExternal == true && !string.IsNullOrEmpty(row.CarrierPluginID))
			{
				ICarrierService cs = CarrierPluginMaint.CreateCarrierService(this._Graph, row.CarrierPluginID);

				foreach (CarrierMethod cm in cs.AvailableMethods)
				{
					CarrierPluginMethod cpm = new CarrierPluginMethod();
					cpm.Code = cm.Code;
					cpm.Description = cm.Description;

					yield return cpm;
				}
			}


		}

        [Serializable]
        [PXHidden]
		public partial class CarrierPluginMethod : IBqlTable
		{
			#region Code
			public abstract class code : PX.Data.BQL.BqlString.Field<code> { }
			protected String _Code;
			[PXDefault()]
			[PXString(50, IsUnicode = false, IsKey = true)]
			[PXUIField(DisplayName = "Method Code", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Code
			{
				get
				{
					return this._Code;
				}
				set
				{
					this._Code = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXString(255, IsUnicode = true)]
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

}
