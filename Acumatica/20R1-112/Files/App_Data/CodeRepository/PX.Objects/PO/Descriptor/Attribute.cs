using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.CM;
using System.Linq;
using PX.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.PO
{
    /// <summary>
    /// Extension of the string PXStringList attribute, which allows to define list <br/>
    /// of hidden(possible) values and their lables. Usually, this list must be wider, then list of <br/> 
    /// enabled values - which mean that UI control may display more values then user is allowed to select in it<br/>
    /// </summary>
	public class PXStringListExtAttribute : PXStringListAttribute
	{
		#region State
		protected string[] _HiddenValues;
		protected string[] _HiddenLabels;
		protected string[] _HiddenLabelsLocal;
		#endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="allowedValues">List of the string values, that user can select in the UI</param>
        /// <param name="allowedLabels">List of the labels for these values</param>
        /// <param name="hiddenValues">List of the string values, that may appear in the list. Normally, it must contain all the values from allowedList </param>
        /// <param name="hiddenLabels">List of the labels for these values</param>
		public PXStringListExtAttribute(string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels)
		{
			_HiddenValues = hiddenValues;
			_HiddenLabels = hiddenLabels;
			_HiddenLabelsLocal = null;
			_ExclusiveValues = false;
		}

		protected PXStringListExtAttribute(Tuple<string, string>[] allowedPairs, Tuple<string, string>[] hiddenPairs)
			: this(
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray(),
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray()
				) {}

	    public override void  CacheAttached(PXCache sender)
		{
			TryLocalize(sender);
 			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, OnFieldUpdating);
		}
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if (_HiddenLabelsLocal == null)
			{
				if (!System.Globalization.CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture) && 
					_HiddenLabels != null && _HiddenValues != null)
				{
					_HiddenLabelsLocal = new string[_HiddenLabels.Length];

					_HiddenLabels.CopyTo(_HiddenLabelsLocal, 0);
					if (_BqlTable != null)
					{
						for (int i = 0; i < _HiddenLabels.Length; i++)
						{
							string value = PXUIFieldAttribute.GetNeutralDisplayName(sender, _FieldName) + " -> " + _HiddenLabels[i];
							string temp = PXLocalizer.Localize(value, _BqlTable.FullName);
							if (!string.IsNullOrEmpty(temp) && temp != value)
								_HiddenLabelsLocal[i] = temp;
						}
					}
				}
				else
					_HiddenLabelsLocal = _HiddenLabels;
			}

			if (e.Row != null && e.ReturnValue != null && IndexAllowedValue((string)e.ReturnValue) < 0)
			{
				int index = IndexValue((string)e.ReturnValue);
				if (index >= 0)
					e.ReturnValue = _HiddenLabelsLocal != null ? _HiddenLabelsLocal[index] : _HiddenLabels[index];
			}
		}

		protected int IndexAllowedValue(string value)
		{
			if (_AllowedValues != null)
				for (int i = 0; i < _AllowedValues.Length; i++)
					if (string.Compare(_AllowedValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected int IndexValue(string value)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
					if (string.Compare(_HiddenValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected string SearchValueByName(string name)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
				{
					if (_HiddenLabelsLocal != null && string.Compare(_HiddenLabelsLocal[i], name, true) == 0)
						return _HiddenValues[i];
					if (_HiddenLabels != null && string.Compare(_HiddenLabels[i], name, true) == 0)
						return _HiddenValues[i];
				}
			return null;
		}

		#region IPXFieldUpdatingSubscriber Members

		protected virtual void OnFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if(e.NewValue != null)
			{
				if(IndexValue((string)e.NewValue) != -1) return;
				e.NewValue = SearchValueByName((string) e.NewValue);
			}
		}

		#endregion
	}

    /// <summary>
    /// Specialized PXStringList attribute for PO Order Line types.<br/>
    /// Provides a list of possible values for line types depending upon InventoryID <br/>
    /// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
    /// are different. If item is changed and old value is not compatible with inventory item <br/>
    /// - it will defaulted to the applicable value.<br/>
    /// <example>
    /// [POLineTypeList(typeof(POLine.inventoryID))]
    /// </example>
    /// </summary>
	public class POLineTypeListAttribute : PXStringListExtAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber
	{
		#region State
		protected Type _inventoryID;
		#endregion

		#region Ctor
		/// <summary>
		/// Ctor, short version. List of allowed values is defined as POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description  
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
		public POLineTypeListAttribute(Type inventoryID)
			: this(
				inventoryID,
				new[]
				{
					Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
					Pair(POLineType.NonStock, Messages.NonStockItem),
					Pair(POLineType.Service, Messages.Service),
					Pair(POLineType.Freight, Messages.Freight),
					Pair(POLineType.Description, Messages.Description),
				})
		{ }

		protected POLineTypeListAttribute(Type inventoryID, Tuple<string, string>[] allowedPairs)
			: this(
				inventoryID,
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray()
				)
		{ }

		/// <summary>
		/// Ctor. Shorter version. User may define a list of allowed values and their descriptions
		/// List for hidden values is defined as POLineType.GoodsForInventory, POLineType.GoodsForSalesOrder, 
		/// POLineType.GoodsForReplenishment, POLineType.GoodsForDropShip, POLineType.NonStockForDropShip, 
		/// POLineType.NonStockForSalesOrder, POLineType.NonStock, POLineType.Service, 
		/// POLineType.Freight, POLineType.Description - it includes all the values for the POLine types. 
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
		/// <param name="allowedValues">List of allowed values. </param>
		/// <param name="allowedLabels">List of allowed values labels. Will be shown in the combo-box in UI</param>        
		public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels)
			: this(
				inventoryID,
				allowedValues,
				allowedLabels,
				new[]
				{
					Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
					Pair(POLineType.GoodsForSalesOrder, Messages.GoodsForSalesOrder),
                    Pair(POLineType.GoodsForServiceOrder, Messages.GoodsForServiceOrder),
					Pair(POLineType.GoodsForReplenishment, Messages.GoodsForReplenishment),
					Pair(POLineType.GoodsForDropShip, Messages.GoodsForDropShip),
					Pair(POLineType.NonStockForDropShip, Messages.NonStockForDropShip),
					Pair(POLineType.NonStockForSalesOrder, Messages.NonStockForSalesOrder),
                    Pair(POLineType.NonStockForServiceOrder, Messages.NonStockForServiceOrder),
					Pair(POLineType.NonStock, Messages.NonStockItem),
					Pair(POLineType.Service, Messages.Service),
					Pair(POLineType.Freight, Messages.Freight),
					Pair(POLineType.Description, Messages.Description),
				}
				)
		{ }

		protected POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels, Tuple<string, string>[] hiddenPairs)
			: this(
				inventoryID,
				allowedValues,
				allowedLabels,
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray())
		{ }

		protected POLineTypeListAttribute(Type inventoryID, Tuple<string, string>[] allowedPairs, Tuple<string, string>[] hiddenPairs)
			: this(
				inventoryID,
				allowedPairs.Select(t => t.Item1).ToArray(),
				allowedPairs.Select(t => t.Item2).ToArray(),
				hiddenPairs.Select(t => t.Item1).ToArray(),
				hiddenPairs.Select(t => t.Item2).ToArray())
		{ }

		/// <summary>
		/// Ctor. Full version. User may define a list of allowed values and their descriptions, and a list of hidden values.
		/// </summary>
		/// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field of the row</param>
		/// <param name="allowedValues">List of allowed values. </param>
		/// <param name="allowedLabels"> Labels for the allowed values. List should have the same size as the list of the values</param>
		/// <param name="hiddenValues"> List of possible values for the control. Must include all the values from the allowedValues list</param>
		/// <param name="hiddenLabels"> Labels for the possible values. List should have the same size as the list of the values</param>
		public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels, hiddenValues, hiddenLabels)
		{
			_inventoryID = inventoryID;
		} 
		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryID.Name, InventoryIDUpdated);
		}


		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (e.Row != null && e.NewValue != null )
			{
				if (inventoryID != null)
				{
					InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);
					InventoryItem nonStock = item != null && item.StkItem == false ? item : null;

					if (nonStock != null && nonStock.KitItem == true)
					{
						INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, nonStock.InventoryID);
						if (component != null) nonStock = null;
					}

					if ((nonStock != null && !POLineType.IsNonStock((string) e.NewValue)) ||
							(nonStock == null && !POLineType.IsStock((string)e.NewValue)))
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				}
				/*
				else if((string)e.NewValue != POLineType.Freight && (string) e.NewValue != POLineType.Description)
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				*/
				if(IndexValue((string)e.NewValue) == -1)
					throw new PXSetPropertyException(Messages.UnsupportedLineType);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (inventoryID != null)
			{
				InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);

				if (item != null && item.StkItem != true)
				{
					if (item.KitItem == true)
					{
						INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, item.InventoryID);

						if (component != null)
							e.NewValue = POLineType.GoodsForInventory;
						return;
					}
					e.NewValue = POLineType.GoodsForInventory;
					if(item.NonStockReceipt != null)
						e.NewValue = item.NonStockReceipt == true
							? POLineType.NonStock
							: POLineType.Service;
				}
				else
					e.NewValue = POLineType.GoodsForInventory;
				e.Cancel = true;
			}
		}

    	protected virtual void InventoryIDUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValuePending(e.Row, _FieldName).IsIn(null, PXCache.NotSetValue))
			{
			sender.SetDefaultExt(e.Row, _FieldName);
		}	
		}	
		#endregion		
	}

	public class POLineTypeList2Attribute : PXStringListAttribute, IPXFieldDefaultingSubscriber
	{
		protected Type docTypeField;
		protected Type inventoryIDField;


		public POLineTypeList2Attribute(Type docType, Type inventoryID) : base(
			new[]
			{
				Pair(POLineType.GoodsForInventory, Messages.GoodsForInventory),
				Pair(POLineType.GoodsForSalesOrder, Messages.GoodsForSalesOrder),
                Pair(POLineType.GoodsForServiceOrder, Messages.GoodsForServiceOrder),
				Pair(POLineType.GoodsForReplenishment, Messages.GoodsForReplenishment),
				Pair(POLineType.GoodsForDropShip, Messages.GoodsForDropShip),
				Pair(POLineType.NonStockForDropShip, Messages.NonStockForDropShip),
				Pair(POLineType.NonStockForSalesOrder, Messages.NonStockForSalesOrder),
                Pair(POLineType.NonStockForServiceOrder, Messages.NonStockForServiceOrder),
				Pair(POLineType.NonStock, Messages.NonStockItem),
				Pair(POLineType.Service, Messages.Service),
				Pair(POLineType.Freight, Messages.Freight),
				Pair(POLineType.Description, Messages.Description),
			})
		{
			this.docTypeField = docType;
			this.inventoryIDField = inventoryID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), sender.GetField(inventoryIDField), InventoryIDUpdated);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (ShowFilteredComboValues(sender, e))
			{
				string retValue = (string)e.ReturnValue;

				string docType = null;
				if (docTypeField != null)
					docType = (string)sender.GetValue(e.Row, sender.GetField(docTypeField));

				int? inventoryID = (int?)sender.GetValue(e.Row, sender.GetField(inventoryIDField));

				Tuple<List<string>, List<string>, string> valueslables = PopulateValues(sender, docType, inventoryID);

				if (string.IsNullOrEmpty(retValue) || valueslables.Item1.Contains(retValue))
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, _FieldName, false, 1, null, valueslables.Item1.ToArray(), valueslables.Item2.ToArray(), true, valueslables.Item3);
					((PXStringState)e.ReturnState).Enabled = valueslables.Item1.Count > 1;
				}
				else
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, _FieldName, false, 1, null,
						this._AllowedValues, this._AllowedLabels,
						true, retValue);
					((PXStringState)e.ReturnState).Enabled = false;
				}
			}
			else
			{
				base.FieldSelecting(sender, e);
			}
		}

		protected virtual bool ShowFilteredComboValues(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
				return false;

			if (string.IsNullOrEmpty(sender.Graph.PrimaryView))//This is in the context of a report - return all possible values do that filter can be populated.
				return false;

			return true;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string docType = null;
			if (docTypeField != null)
				docType = (string)sender.GetValue(e.Row, sender.GetField(docTypeField));
			int? inventoryID = (int?)sender.GetValue(e.Row, sender.GetField(inventoryIDField));

			Tuple<List<string>, List<string>, string> valueslables = PopulateValues(sender, docType, inventoryID);

			e.NewValue = valueslables.Item3;
		}

		private string LocaleLabel(string value, string neutralLabel)
		{
			if (_AllowedValues != null && _AllowedLabels != null && _AllowedValues.Length == _AllowedLabels.Length)
			{
				for (int i = 0; i < _AllowedValues.Length; i++)
					if (_AllowedValues[i] == value)
						return _AllowedLabels[i];
			}

			return neutralLabel;
		}

	/// <summary>
		/// Populate list of available LineTypes based on current state of entity.
		/// </summary>
		/// <returns>
		/// Item1 - List of available values
		/// Item2 - List of available labels
		/// Item3 - default value.
		/// </returns>
		protected virtual Tuple<List<string>, List<string>, string> PopulateValues(PXCache sender, string docType, int? inventoryID)
		{
			string defaultValue = null;
			List<string> values = new List<string>();
			List<string> labels = new List<string>();

			InventoryItem item = null;
			if (inventoryID != null)
				item = InventoryItem.PK.Find(sender.Graph, inventoryID);

			if (item != null)
			{
				bool stockItem = false;

				if (item.StkItem == true)
				{
					stockItem = true;
				}
				else if (item.KitItem == true)
				{
					INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, item.InventoryID);
					if (component != null)
					{
						stockItem = true;
					}
				}

				if (stockItem)
				{
					if (docType == POOrderType.DropShip)
					{
						values.Add(POLineType.GoodsForDropShip);
						labels.Add(LocaleLabel(POLineType.GoodsForDropShip, Messages.GoodsForDropShip));
					}
					else
					{
						values.Add(POLineType.GoodsForInventory);
						labels.Add(LocaleLabel(POLineType.GoodsForInventory, Messages.GoodsForInventory));
					}
				}
				else
				{
					if (docType == POOrderType.DropShip)
					{
						if (item.NonStockReceipt == true && item.NonStockShip == true)
						{
							values.Add(POLineType.NonStockForDropShip);
							labels.Add(LocaleLabel(POLineType.NonStockForDropShip, Messages.NonStockForDropShip));
						}
						else if (item.NonStockReceipt == true)
						{
							values.Add(POLineType.NonStock);
							labels.Add(LocaleLabel(POLineType.NonStock, Messages.NonStockItem));
						}
						else
						{
							values.Add(POLineType.Service);
							labels.Add(Messages.Service);
						}
					}
					else
					{
						if (item.NonStockReceipt == true)
						{
							values.Add(POLineType.NonStock);
							labels.Add(LocaleLabel(POLineType.NonStock, Messages.NonStockItem));
						}
						else
						{
							values.Add(POLineType.Service);
							labels.Add(LocaleLabel(POLineType.Service, Messages.Service));
						}
					}
				}
			}
			else
			{
				values.Add(POLineType.Service);
				labels.Add(LocaleLabel(POLineType.Service, Messages.Service));

				values.Add(POLineType.Description);
				labels.Add(LocaleLabel(POLineType.Description, Messages.Description));

				values.Add(POLineType.Freight);
				labels.Add(LocaleLabel(POLineType.Freight, Messages.Freight));
			}


			if (values.Count > 0)
				defaultValue = values[0];


			return new Tuple<List<string>, List<string>, string>(values, labels, defaultValue);
		}

		protected virtual void InventoryIDUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValuePending(e.Row, _FieldName).IsIn(null, PXCache.NotSetValue))
			{
			sender.SetDefaultExt(e.Row, _FieldName);
		}
	}
	}

	/// <summary>
	/// Specialized PXStringList attribute for Receipt Line types.<br/>
	/// Provides a list of possible values for line types depending upon InventoryID <br/>
	/// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
	/// are different. If item is changed and old value is not compatible with inventory item <br/>
	/// - it will defaulted to the applicable value.<br/>
	/// <example>
	/// [POReceiptLineTypeList(typeof(POLine.inventoryID))]
	/// </example>
	/// </summary>
	public class POReceiptLineTypeListAttribute : POLineTypeListAttribute
	{
		public POReceiptLineTypeListAttribute(Type inventoryID)
			:base(
			inventoryID,
			new string[] { POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight },
			new string[] { Messages.GoodsForInventory, Messages.NonStockItem, Messages.Service, Messages.Freight })
		{
			
		}
	}

    /// <summary>
    /// Specialized for POOrder version of the VendorAttribute, which defines a list of vendors, <br/>
    /// which may be used in the PO Order (for example, employee are filtered <br/>
    /// out for all order types except Transfer ) <br/>
    /// Depends upon POOrder current. <br/>
    /// <example>
    /// [POVendor()]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Vendor.status, IsNull, 
							Or<Vendor.status, Equal<BAccount.status.active>, 
				  		    Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
	public class POVendorAttribute : VendorAttribute
	{
		public POVendorAttribute()
			: base(typeof(Search<BAccountR.bAccountID,
				Where<Vendor.type, NotEqual<BAccountType.employeeType>>>))
		{
		}
	}

    /// <summary>
    /// Specialized for PO version of the Address attribute.<br/>
    /// Uses POAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for POShipAddress(inherited from POAddress) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POAddress <br/>
    /// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>    
    /// Depends upon row instance. <br/>
    /// <example>
    ///[POShipAddress(typeof(Select2<Address,
    ///               LeftJoin<Location, On<Address.bAccountID, Equal<Location.bAccountID>,
    ///                And<Address.addressID, Equal<Location.defAddressID>,
    ///                And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
    ///                And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
    ///                And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
    ///                LeftJoin<INSite, On<Address.addressID, Equal<INSite.addressID>,
    ///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
    ///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
    ///                LeftJoin<POShipAddress, On<POShipAddress.bAccountID, Equal<Address.bAccountID>,
    ///                    And<POShipAddress.bAccountAddressID, Equal<Address.addressID>,
    ///                    And<POShipAddress.revisionID, Equal<Address.revisionID>,
    ///                    And<POShipAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
    ///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
    /// </example>
    /// </summary>	
	public class POShipAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects POShipAddress as a POAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>		
		public POShipAddressAttribute(Type SelectType)
			: base(typeof(POShipAddress.addressID), typeof(POShipAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<POShipAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<POShipAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}			
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && (string)sender.GetValue<POOrder.shipDestType>(e.Row) != POShipDestType.Site)
			{
				var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
				if (errors != null && errors.Count > 0)
					return;
			}

			base.RowPersisting(sender, e);
		}

		public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
		{
			int? siteID = (int?)sender.GetValue<POOrder.siteID>(DocumentRow);
			string shipDestType = (string)sender.GetValue<POOrder.shipDestType>(DocumentRow);

			bool issitebranch = false;
			if (siteID != null && shipDestType == POShippingDestination.Site)
				issitebranch = true;

			PXView view = CreateAddressView(sender, DocumentRow, issitebranch);

			int startRow = -1;
			int totalRows = 0;
			bool addressFind = false;
			foreach (PXResult res in view.Select(null, new object[] { siteID }, null, null, null, null, ref startRow, 1, ref totalRows))
			{
				addressFind = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
				break;
			}

			if (!addressFind && !_Required)
				this.ClearRecord(sender, DocumentRow);

			if (!addressFind && _Required && issitebranch)
				throw new SharedRecordMissingException();
		}

		protected virtual PXView CreateAddressView(PXCache sender, object DocumentRow, bool issitebranch)
		{
			if (issitebranch)
			{
				BqlCommand altSelect = BqlCommand.CreateInstance(
										typeof(SelectFrom<Address>.
										InnerJoin<INSite>.
											On<INSite.FK.Address.And<INSite.siteID.IsEqual<@P.AsInt>>>.
										LeftJoin<POShipAddress>.
											On<POShipAddress.bAccountID.IsEqual<Address.bAccountID>.
												And<POShipAddress.bAccountAddressID.IsEqual<Address.addressID>.
												And<POShipAddress.revisionID.IsEqual<Address.revisionID>.
												And<POShipAddress.isDefaultAddress.IsEqual<boolTrue>>>>>.
										Where<boolTrue.IsEqual<boolTrue>>));

				return sender.Graph.TypedViews.GetView(altSelect, false);
			}
			else
			{
				return sender.Graph.TypedViews.GetView(_Select, false);
			}
		}
	}

    /// <summary>
    /// Specialized for PO version of the Address attribute.<br/>
    /// Uses POAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for PORemitAddress(inherited POAddess) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POAddress <br/>
    /// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>    
    /// Depends upon row instance.
    /// <example>
    /// [PORemitAddress(typeof(Select2<BAccount2,
    ///		InnerJoin<Location, On<Location.bAccountID, Equal<BAccount2.bAccountID>>,
    ///		InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
    ///		LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>, 
    ///			And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
    ///			And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
    ///		Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]		
    /// </example>
    /// </summary>
	public class PORemitAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects PORemitAddress as a POAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
		public PORemitAddressAttribute(Type SelectType)
			: base(typeof(PORemitAddress.addressID), typeof(PORemitAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<PORemitAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, Row);
		}
		
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PORemitAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}			
		}


	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORShipContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.    
    ///<example>
    ///[POShipContactAttribute(typeof(Select2<Contact,
    ///                LeftJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
    ///                    And<Contact.contactID, Equal<Location.defContactID>,
    ///                    And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
    ///                    And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
    ///                    And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
    ///                LeftJoin<INSite, On<Contact.contactID, Equal<INSite.contactID>,
    ///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
    ///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
    ///                LeftJoin<POShipContact, On<POShipContact.bAccountID, Equal<Contact.bAccountID>,
    ///                    And<POShipContact.bAccountContactID, Equal<Contact.contactID>,
    ///                    And<POShipContact.revisionID, Equal<Contact.revisionID>,
    ///                    And<POShipContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
    ///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
    ///</example>    
    ///</summary>		
	public class POShipContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects POShipContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>		
		public POShipContactAttribute(Type SelectType)
			: base(typeof(POShipContact.contactID), typeof(POShipContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<POShipContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && (string)sender.GetValue<POOrder.shipDestType>(e.Row) != POShipDestType.Site)
			{
				var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
				if (errors != null && errors.Count > 0)
					return;
			}

			base.RowPersisting(sender, e);
		}

		public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object AddressRow)
		{
			int? siteID = (int?)sender.GetValue<POOrder.siteID>(DocumentRow);
			string shipDestType = (string)sender.GetValue<POOrder.shipDestType>(DocumentRow);
			bool issitebranch = false;
			if (siteID != null && shipDestType == POShippingDestination.Site)
				issitebranch = true;

			PXView view = CreateContactView(sender, DocumentRow, issitebranch);

			int startRow = -1;
			int totalRows = 0;
			bool contactFound = false;

			foreach (PXResult res in view.Select(null, new object[] { siteID }, null, null, null, null, ref startRow, 1, ref totalRows))
			{
				contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, AddressRow, res);
				break;
			}

			if (!contactFound && !_Required)
				this.ClearRecord(sender, DocumentRow);

			if (!contactFound && _Required && issitebranch)
				throw new SharedRecordMissingException();

		}

		protected virtual PXView CreateContactView(PXCache sender, object DocumentRow, bool issitebranch)
		{
			if (issitebranch)
			{
				BqlCommand altSelect = BqlCommand.CreateInstance(
										typeof(SelectFrom<Contact>.
										InnerJoin<INSite>.
											On<INSite.FK.Contact.And<INSite.siteID.IsEqual<@P.AsInt>>>.
										LeftJoin<POShipContact>.
											On<POShipContact.bAccountID.IsEqual<Contact.bAccountID>.
												And<POShipContact.bAccountContactID.IsEqual<Contact.contactID>.
												And<POShipContact.revisionID.IsEqual<Contact.revisionID>.
												And<POShipContact.isDefaultContact.IsEqual<boolTrue>>>>>.
										Where<boolTrue.IsEqual<boolTrue>>));

				return sender.Graph.TypedViews.GetView(altSelect, false);
			}
			else
			{
				return sender.Graph.TypedViews.GetView(_Select, false);
			}
		}
	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORemitContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.
    /// <example>
    /// [APContact(typeof(Select2<Location,
    ///		InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
    ///		InnerJoin<Contact, On<Contact.contactID, Equal<Location.remitContactID>, 
    ///		    And<Where<Contact.bAccountID, Equal<Location.bAccountID>, 
    ///		    Or<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
    ///		LeftJoin<APContact, On<APContact.vendorID, Equal<Contact.bAccountID>, 
    ///		    And<APContact.vendorContactID, Equal<Contact.contactID>, 
    ///		    And<APContact.revisionID, Equal<Contact.revisionID>, 
    ///		    And<APContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
    ///		Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, 
    ///		And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
    /// </example>
    /// </summary>
	public class PORemitContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects PORemitContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>		
		public PORemitContactAttribute(Type SelectType)
			: base(typeof(PORemitContact.contactID), typeof(PORemitContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<PORemitContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}

		}

	}

	public class POTaxAttribute : TaxAttribute
	{
		#region CuryRetainageAmt
		protected abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt> { }
		protected Type CuryRetainageAmt = typeof(curyRetainageAmt);
		protected string _CuryRetainageAmt
		{
			get
			{
				return CuryRetainageAmt.Name;
			}
		}
		#endregion

		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }
		protected override bool AskRecalculationOnCalcModeChange { get => false; set => base.AskRecalculationOnCalcModeChange = value; }

		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}

		public POTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POOrder.curyOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyLineTotal);
			this.DocDate = typeof(POOrder.orderDate);
            this.CuryTranAmt = typeof(POLine.curyExtCost);
            this.GroupDiscountRate = typeof(POLine.groupDiscountRate);
			this.CuryTaxTotal = typeof(POOrder.curyTaxTotal);
			this.CuryDiscTot = typeof(POOrder.curyDiscTot);
			this.TaxCalcMode = typeof(POOrder.taxCalcMode);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyExtCost), typeof(SumCalc<POOrder.curyLineTotal>)));
		}

		public override int CompareTo(object other)
		{
			return this.SortOrder.CompareTo(((POTaxAttribute)other).SortOrder);
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			// Normally, CuryTranAmt is reduced by CuryRetainageAmt for each POLine.
			// In case when "Retain Taxes" flag is disabled in APSetup - we should calculate
			// taxable amount based on the full CuryTranAmt amount, this why we should add 
			// CuryRetainageAmt to the CuryTranAmt value.
			//
			decimal curyRetainageAmt = IsRetainedTaxes(sender.Graph)
				? 0m
				: (decimal)(sender.GetValue(row, _CuryRetainageAmt) ?? 0m);

			decimal curyTranAmt = (base.GetCuryTranAmt(sender, row) ?? 0m);
			decimal? value = (curyTranAmt + curyRetainageAmt) *
				(decimal?)sender.GetValue(row, _GroupDiscountRate) *
				(decimal?)sender.GetValue(row, _DocumentDiscountRate);

			return PXDBCurrencyAttribute.Round(sender, row, (decimal)value, CMPrecision.TRANCURY);
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLine.lineNbr>>(graph, new object[] { row, ((POOrderEntry)graph).Document.Current }, taxchk, parameters);
		}

		protected List<object> SelectTaxes<Where, LineNbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where LineNbr : IBqlOperand
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			var taxesWithRevisions = 
				PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<
						Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>,
						Or<TaxRev.taxType, Equal<TaxType.sales>, 
							And<
								Where<Tax.reverseTax, Equal<boolTrue>,
									Or<Tax.taxType, Equal<CSTaxType.use>, 
									Or<Tax.taxType, Equal<CSTaxType.withholding>,
									Or<Tax.taxType, Equal<CSTaxType.perUnit>>>>>>>>>,
					And<Current<POOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters);


			foreach (PXResult<Tax, TaxRev> record in taxesWithRevisions)
			{
				Tax adjdTax = AdjustTaxLevel(graph, (Tax)record);
				tail[((Tax)record).TaxID] = new PXResult<Tax, TaxRev>(adjdTax, (TaxRev)record);
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (POTax record in PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.lineNbr, Equal<LineNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<POTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;
							Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
							ret.Insert(idx, new PXResult<POTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (POTax record in PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.lineNbr, Less<intMax>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((POTax)(PXResult<POTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& taxByCalculationLevelComparer.Compare((PXResult<POTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;
							Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
							ret.Insert(idx, new PXResult<POTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (POTaxTran record in PXSelect<POTaxTran,
						Where<POTaxTran.orderType, Equal<Current<POOrder.orderType>>,
							And<POTaxTran.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<POTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;
							Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
							ret.Insert(idx, new PXResult<POTaxTran, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<POLine,
								Where<POLine.orderType, Equal<Current<POOrder.orderType>>,
									And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<POLine>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}

		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (SortOrder == 0)
				base.DefaultTaxes(sender, row, DefaultExisting);
		}

		protected override void ClearTaxes(PXCache sender, object row)
		{
			if (SortOrder == 0)
				base.ClearTaxes(sender, row);
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType)
		{
			decimal curyTranAmt = (base.GetCuryTranAmt(sender, row) ?? 0m);
			decimal? groupDiscount = (decimal?)sender.GetValue(row, _GroupDiscountRate);
			decimal? docDiscount = (decimal?)sender.GetValue(row, _DocumentDiscountRate);
			decimal? value = curyTranAmt * groupDiscount * docDiscount;

			return (decimal)value;
		}

		public override void CacheAttached(PXCache sender)
		{
            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>(); 
            
            if (this.EnableTaxCalcOn(sender.Graph))
			{
				base.CacheAttached(sender);
				sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), _CuryTaxTotal, POOrder_CuryTaxTot_FieldUpdated);
                sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), typeof(POOrder.curyDiscTot).Name, POOrder_CuryDiscTot_FieldUpdated);
            }
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

        protected virtual void POOrder_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            bool calc = true;
            TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
            if (taxZone != null && taxZone.IsExternal == true)
                calc = false;

            this._ParentRow = e.Row;
            CalcTotals(sender, e.Row, calc);
            this._ParentRow = null;
        }

		virtual protected bool EnableTaxCalcOn(PXGraph aGraph) 
		{
			return (aGraph is POOrderEntry);
		}


		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted)
			{
				decimal doc_CuryWhTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryWhTaxTotal) ?? 0m);

				if (object.Equals(CuryWhTaxTotal, doc_CuryWhTaxTotal) == false)
				{
					ParentSetValue(sender.Graph, _CuryWhTaxTotal, CuryWhTaxTotal);
				}
			}
		}

		protected override void _CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal curyTaxTotal, 
			decimal curyInclTaxTotal, 
			decimal curyWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			//Note: POTaxAttribute is called first and cannot rely on the fields that will be calculated by other POXXTaxAttributes (based on the SortOrder)

			decimal curyDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot)??0m);
			decimal curyLineTotal = CalcLineTotal(sender, row);
			decimal curyDocTotal = curyLineTotal + curyTaxTotal - curyInclTaxTotal - curyDiscountTotal;

			decimal docCuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);
			decimal docCuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);//_CuryDocBal is actually curyOrderTotal

			if (curyTaxTotal != docCuryTaxTotal)
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, curyTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal) && curyDocTotal != docCuryDocBal)
			{
				ParentSetValue(sender.Graph, _CuryDocBal, curyDocTotal);
			}
		}

		protected virtual void POOrder_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			if (taxZone != null && taxZone.IsExternal == true)
			{
				decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);

				CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0);
			}
		}

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m);

            if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
            {
                if (Math.Abs(CuryTaxableAmt - CuryLineTotal) < 0.00005m)
                {
                    CuryTaxableAmt -= CuryDiscountTotal;
                }
            }
        }

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(APSetup)];
			APSetup apsetup = cache.Current as APSetup;
			POOrder document = ParentRow(graph) as POOrder;

			return
				PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				document?.RetainageApply == true &&
				apsetup?.RetainTaxes == true;
		}
	}

	public class POUnbilledTaxAttribute : POTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 1;
			}
		}

		public POUnbilledTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(POTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(POTax.curyUnbilledTaxAmt).Name;

			this.CuryDocBal = typeof(POOrder.curyUnbilledOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyUnbilledLineTotal);
			this.CuryTaxTotal = typeof(POOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(POOrder.orderDate);
			this.CuryDiscTot = typeof(POOrder.curyDiscTot);

			this.CuryTranAmt = typeof(POLine.curyUnbilledAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POLine.curyUnbilledAmt), typeof(SumCalc<POOrder.curyUnbilledLineTotal>));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		#region Per Unit Taxes
		protected override string TaxableQtyFieldNameForTaxDetail => nameof(POTax.UnbilledTaxableQty);
		#endregion

		//Only base attribute should re-default taxes
		protected override void ReDefaultTaxes(PXCache cache, List<object> details)
		{
		}
		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
		}
		
		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			// base behavior to avoid doubling of retainage (it is already in POLine.curyOpenAmt)
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			return false;
		}

		protected override void _CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal curyOpenTaxTotal, 
			decimal curyOpenInclTaxTotal, 
			decimal curyOpenWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			//POOpenTaxAttribute can rely on POTaxAttribute beeen called at this point in time (since the SortOrder is applied)

			decimal curyDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);
			decimal curyOpenLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);//_CuryLineTotal is actually a curyOpenLineTotal in this context.
			decimal curyLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;

			decimal curyOpenDiscTotal = CalcUnbilledDiscTotal(sender, row, curyLineTotal, curyDiscountTotal, curyOpenLineTotal);

			decimal curyOpenDocTotal = curyOpenLineTotal + curyOpenTaxTotal - curyOpenInclTaxTotal - curyOpenDiscTotal;
			
			decimal docCuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);//_CuryTaxTotal is actually curyOpenTaxTotal
			decimal docCuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);//_CuryDocBal is actually curyOpenOrderTotal

			if (curyOpenTaxTotal != docCuryTaxTotal)
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, curyOpenTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal) && curyOpenDocTotal != docCuryDocBal)
			{
				ParentSetValue(sender.Graph, _CuryDocBal, curyOpenDocTotal);
			}
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
			decimal curyUnbilledLineTotal = (decimal?)ParentGetValue<POOrder.curyUnbilledLineTotal>(sender.Graph) ?? 0m;

			if (curyUnbilledLineTotal != 0m && CuryTaxableAmt != 0m && curyUnbilledLineTotal == CuryTaxableAmt)
			{
				decimal curyLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;
				decimal curyDiscountTotal = (decimal?)ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m;

				CuryTaxableAmt -= CalcUnbilledDiscTotal(sender, row, curyLineTotal, curyDiscountTotal, curyUnbilledLineTotal);
			}
		}

		protected virtual decimal CalcUnbilledDiscTotal(PXCache sender, object row, decimal curyLineTotal, decimal curyDiscTotal, decimal curyUnbilledLineTotal)
		{
			return (Math.Abs(curyLineTotal - curyUnbilledLineTotal) < 0.00005m)
				? curyDiscTotal
				: (curyLineTotal == 0 ? 0 : PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), curyUnbilledLineTotal * curyDiscTotal / curyLineTotal));
		}
	}


	public class POUnbilledTaxRAttribute : POUnbilledTaxAttribute
	{
		public POUnbilledTaxRAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(POLineUOpen.curyUnbilledAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POLineUOpen.curyUnbilledAmt), typeof(SumCalc<POOrder.curyUnbilledLineTotal>));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<POLineUOpen.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
		
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryOpenAmt_FieldUpdated);
		}

		override protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return (aGraph is POOrderEntry || aGraph is POReceiptEntry || aGraph is AP.APReleaseProcess);
		} 

		public virtual void CuryOpenAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}

	public class LSPOReceiptLine : LSSelect<POReceiptLine, POReceiptLineSplit,
		Where<POReceiptLineSplit.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>
	{
		#region State
		protected virtual bool IsLSEntryEnabled(object row)
		{
			POReceiptLine line = row as POReceiptLine;

			if (line != null && line.IsLSEntryBlocked == true)
				return false;

			if (line == null) return true;

			if (line.IsStockItem())
				return true;

			if (line.LineType == POLineType.GoodsForDropShip)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, line.InventoryID);

				if (((INLotSerClass)item).RequiredForDropship == true)
					return true;
			}

			return false;
		}

        protected string _OrigOrderQtyField = "OrigOrderQty";
        protected string _OpenOrderQtyField = "OpenOrderQty";
		#endregion
		#region Ctor
		public LSPOReceiptLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(POReceiptLine.receiptQty);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.subItemID>(POReceiptLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.locationID>(POReceiptLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.invtMult>(POReceiptLineSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.lotSerialNbr>(POReceiptLineSplit_LotSerialNbr_FieldDefaulting);
			graph.RowUpdated.AddHandler<POReceipt>(POReceipt_RowUpdated);
            graph.FieldUpdated.AddHandler<POReceiptLine.receiptQty>(POReceiptLine_ReceiptQty_FieldUpdated);
            graph.FieldSelecting.AddHandler(typeof(POReceiptLine), _OrigOrderQtyField, OrigOrderQty_FieldSelecting);
            graph.FieldSelecting.AddHandler(typeof(POReceiptLine), _OpenOrderQtyField, OpenOrderQty_FieldSelecting);
		}
        #endregion

        #region Implementation
        public override POReceiptLine CloneMaster(POReceiptLine item)
		{
			POReceiptLine copy = base.CloneMaster(item);
			copy.POType = null;
			copy.PONbr = null;
			copy.POLineNbr = null;

			return copy;
		}
        
        public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            base.Master_Qty_FieldVerifying(sender, e);
            
            VerifyReceiptedQty(sender, (POReceiptLine)e.Row, e.NewValue, false);
        }

        public virtual bool VerifyReceiptedQty(PXCache sender, POReceiptLine row, object value, bool persisting)
        {
            bool istransfer = row.ReceiptType == POReceiptType.TransferReceipt;
            if (istransfer && row.MaxTransferBaseQty.HasValue)
            {
                decimal? max = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>
                    (sender, row, row.MaxTransferBaseQty.Value, INPrecision.QUANTITY);
                if ((decimal?)value > max)
                {
                    if(persisting)
                        throw new PXRowPersistingException(typeof(POReceiptLine.receiptQty).Name, row.ReceiptQty, CS.Messages.Entry_LE, new object[] { max });
                    
                    sender.RaiseExceptionHandling<POReceiptLineSplit.qty>(row, row.ReceiptQty, new PXSetPropertyException<INTran.qty>(CS.Messages.Entry_LE, PXErrorLevel.Error, max));
                    return false;
                }
            }
            return true;
        }

        protected virtual void POReceipt_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<POReceipt.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<POReceipt.hold>(e.Row) == false)
			{
				PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];

				foreach (POReceiptLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(POReceipt)))
				{
					if (IsLSEntryEnabled(item) && Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
					{
						cache.RaiseExceptionHandling<POReceiptLine.receiptQty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						cache.MarkUpdated(item);
					}
				}
			}
		}
		protected virtual void POReceiptLine_ReceiptQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row != null && row.ReceiptQty != (Decimal?)e.OldValue)
				sender.RaiseFieldUpdated<POReceiptLine.baseReceiptQty>(e.Row, row.BaseReceiptQty);
		}
		
		public override bool IsTrackSerial(PXCache sender, ILSDetail row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item == null)
				return false;

			if (((POReceiptLineSplit)row).LineType == POLineType.GoodsForDropShip)
			{
				return ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered;
			}
			else
			{
				return INLotSerialNbrAttribute.IsTrackSerial(item, row.TranType, row.InvtMult);
			}
		}

        public override void Detail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            base.Detail_RowPersisting(sender, e);
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row) && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				PXCache cache = sender.Graph.Caches[typeof(POReceipt)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(POReceipt)) ?? cache.Current;

				bool? OnHold = (bool?)cache.GetValue<POReceipt.hold>(doc);

				if (OnHold == false && Math.Abs((decimal)((POReceiptLine)e.Row).BaseQty) >= 0.0000005m && (((POReceiptLine)e.Row).UnassignedQty >= 0.0000005m || ((POReceiptLine)e.Row).UnassignedQty <= -0.0000005m))
				{
					if (sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(e.Row, ((POReceiptLine)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(POReceiptLine.receiptQty).Name, ((POReceiptLine)e.Row).Qty, Messages.BinLotSerialNotAssigned);
					}
				}
			}
            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
                VerifyReceiptedQty(sender, (POReceiptLine)e.Row, ((POReceiptLine)e.Row).ReceiptQty, true);
                
            base.Master_RowPersisting(sender, e);
		}
        
        protected virtual void OrigOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
            POReceiptLine row = e.Row as POReceiptLine;

			if (row != null && row.PONbr != null)
			{
                POLineR orig_line = PXSelect<POLineR,
                        Where<POLineR.orderType, Equal<Required<POLineR.orderType>>,
                        And<POLineR.orderNbr, Equal<Required<POLineR.orderNbr>>,
                        And<POLineR.lineNbr, Equal<Required<POLineR.lineNbr>>>>>>
                        .Select(sender.Graph, row.POType, row.PONbr, row.POLineNbr);

				if (orig_line != null && row.InventoryID == orig_line.InventoryID)
				{
					if (string.Equals(((POReceiptLine)e.Row).UOM, orig_line.UOM) == false)
					{
						decimal BaseOrderQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty, INPrecision.QUANTITY);
						e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOrderQty, INPrecision.QUANTITY);
					}
					else
					{
						e.ReturnValue = orig_line.OrderQty;
					}
				}
			}

            if (row != null && row.OrigRefNbr != null)
            {
                INTran orig_line = PXSelect<INTran, Where<INTran.tranType, Equal<INTranType.transfer>,
                    And<INTran.refNbr, Equal<Current<POReceiptLine.origRefNbr>>,
                    And<INTran.lineNbr, Equal<Current<POReceiptLine.origLineNbr>>,
                    And<INTran.docType, Equal<Current<POReceiptLine.origDocType>>>>>>>.SelectSingleBound(_Graph, new object[] { (POReceiptLine)e.Row });

                //is it needed at all? UOM conversion seems to be right thing to do. Also must it be origQty or origleftqty?
                if (orig_line != null)
                {
                    //if (string.Equals(row.UOM, orig_line.UOM) == false)
                    //{
                    //    decimal BaseOpenQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.Qty, INPrecision.QUANTITY);
                    //    e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOpenQty, INPrecision.QUANTITY);
                    //}
                    //else
                    {
                        e.ReturnValue = orig_line.Qty;
                    }
                }
            }

			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, ((CommonSetup)_Graph.Caches[typeof(CommonSetup)].Current).DecPlQty, _OrigOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
			((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OrigOrderQty);
			((PXFieldState)e.ReturnState).Enabled = false;
		}

        protected virtual void OpenOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            POReceiptLine row = e.Row as POReceiptLine;

            if (row != null && row.PONbr != null)
            {
                POLineR orig_line = PXSelect<POLineR,
						Where<POLineR.orderType, Equal<Required<POLineR.orderType>>,
						And<POLineR.orderNbr, Equal<Required<POLineR.orderNbr>>,
						And<POLineR.lineNbr, Equal<Required<POLineR.lineNbr>>>>>>
						.Select(sender.Graph, row.POType, row.PONbr, row.POLineNbr);

                if (orig_line != null && row.InventoryID == orig_line.InventoryID)
                {
					decimal? openQty;
                    if (string.Equals(((POReceiptLine)e.Row).UOM, orig_line.UOM) == false)
                    {
                        decimal BaseOpenQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty - (decimal)orig_line.ReceivedQty, INPrecision.QUANTITY);
						openQty = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOpenQty, INPrecision.QUANTITY);
                    }
                    else
                    {
						openQty = orig_line.OrderQty - orig_line.ReceivedQty;
                    }
					e.ReturnValue = (openQty < 0m) ? 0m : openQty;
                }
            }

            if (row != null && row.OrigRefNbr != null)
            {
                INTransitLineStatus origlinestat = 
					PXSelect<INTransitLineStatus,
					Where<INTransitLineStatus.transferNbr, Equal<Current<POReceiptLine.origRefNbr>>,
						And<INTransitLineStatus.transferLineNbr, Equal<Current<POReceiptLine.origLineNbr>>>>>
					.SelectSingleBound(_Graph, new object[] { (POReceiptLine)e.Row });

                if (origlinestat != null)
                {                    
                    decimal BaseOpenQty = origlinestat.QtyOnHand.Value - ((row.Released ?? false) ? 0 : row.BaseReceiptQty.GetValueOrDefault());
                    e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOpenQty, INPrecision.QUANTITY);   
                }
            }

			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, ((CommonSetup)_Graph.Caches[typeof(CommonSetup)].Current).DecPlQty, _OpenOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
            ((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OpenOrderQty);
            ((PXFieldState)e.ReturnState).Enabled = false;
        }

		public override IEnumerable GenerateLotSerial(PXAdapter adapter)
		{
			if (MasterCache.Current != null && IsLSEntryEnabled((POReceiptLine)MasterCache.Current))
				return base.GenerateLotSerial(adapter);
			return adapter.Get();
		}

		public override IEnumerable BinLotSerial(PXAdapter adapter)
		{
			if (MasterCache.Current != null)
			{
				if (!IsLSEntryEnabled((POReceiptLine)MasterCache.Current))
				{
					throw new PXSetPropertyException(IN.Messages.BinLotSerialEntryDisabled);
				}
				View.AskExt(true);
			}
			return adapter.Get();
		}

		protected override void Master_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			bool lsEntryEnabled = IsLSEntryEnabled((POReceiptLine)e.Row) && ((POReceiptLine)e.Row).Released != true;
			var splitCache = sender.Graph.Caches[typeof(POReceiptLineSplit)];
			splitCache.AllowInsert = lsEntryEnabled;
			splitCache.AllowUpdate = lsEntryEnabled;
			splitCache.AllowDelete = lsEntryEnabled;

			sender.Adjust<POLotSerialNbrAttribute>(e.Row).For<POReceiptLine.lotSerialNbr>(a => a.ForceDisable = !lsEntryEnabled);
		}

		protected override bool IsLotSerOptionsEnabled(PXCache sender, LotSerOptions opt)
		{
			return base.IsLotSerOptionsEnabled(sender, opt) &&
				((POReceipt)sender.Graph.Caches<POReceipt>().Current)?.Released != true;
		}

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<POReceiptLine.locationID>(e.Row, null);
				sender.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<POReceiptLine.expireDate>(e.Row, null);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				using (ResolveNotDecimalUnitErrorRedirectorScope<POReceiptLineSplit.qty>(e.Row))
					base.Master_RowUpdated(sender, e);
			}
			else
			{
				sender.SetValue<POReceiptLine.locationID>(e.Row, null);
				sender.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<POReceiptLine.expireDate>(e.Row, null);

				POReceiptLine row = (POReceiptLine)e.Row;
				POReceiptLine oldRow = (POReceiptLine)e.OldRow;

				if (row != null && oldRow != null && row.InventoryID != oldRow.InventoryID)			
				{
					RaiseRowDeleted(sender, oldRow);
				}
			}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.Master_RowDeleted(sender, e);
			}
		}

		public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (IsTrackSerial(sender, (ILSDetail)e.Row))
			{
				base.Detail_Qty_FieldVerifying(sender, e);
			}
			else
			{
				VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(POReceiptLineSplit.qty).Name);
			}
		}

		public override POReceiptLineSplit Convert(POReceiptLine item)
		{
			using (InvtMultScope<POReceiptLine> ms = new InvtMultScope<POReceiptLine>(item))
			{
				POReceiptLineSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}


		public virtual void POReceiptLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((POReceiptLineSplit)e.Row).BaseQty != 0m && ((POReceiptLineSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<POReceiptLineSplit.locationID>(sender, e.Row);
				}
			}
		}

		public virtual void POReceiptLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				e.NewValue = ((POReceiptLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void POReceiptLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				e.NewValue = ((POReceiptLine)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void POReceiptLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<POReceiptLine> ms = new InvtMultScope<POReceiptLine>((POReceiptLine)cache.Current))
				{
					e.NewValue = ((POReceiptLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void POReceiptLineSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((POReceiptLineSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((POReceiptLineSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<POReceiptLineSplit.invtMult>(e.Row, out InvtMult);
				}

				INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (POReceiptLineSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<POReceiptLineSplit>(sender, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			if (row is POReceiptLine)
			{
				sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetValueExt<POReceiptLine.inventoryID>(row), sender.GetValueExt<POReceiptLine.subItemID>(row), sender.GetValueExt<POReceiptLine.siteID>(row), sender.GetValueExt<POReceiptLine.locationID>(row), sender.GetValue<POReceiptLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<POReceiptLineSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetValueExt<POReceiptLineSplit.inventoryID>(row), sender.GetValueExt<POReceiptLineSplit.subItemID>(row), sender.GetValueExt<POReceiptLineSplit.siteID>(row), sender.GetValueExt<POReceiptLineSplit.locationID>(row), sender.GetValue<POReceiptLineSplit.lotSerialNbr>(row)));
			}
		}

		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
            POReceipt receipt = (POReceipt)PXParentAttribute.SelectParent(sender, e.Row, typeof(POReceipt));

            IQtyAllocated availability = AvailabilityFetch(sender, (POReceiptLine)e.Row, 
                (receipt?.Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent) | AvailabilityFetchMode.TryOptimize);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((POReceiptLine)e.Row).InventoryID);

				availability.QtyOnHand = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
				availability.QtyAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
				availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
				availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
						sender.GetValue<POReceiptLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));
									

				AvailabilityCheck(sender, (POReceiptLine)e.Row, availability);
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		protected int _detailsRequested = 0;

		public override IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster row, AvailabilityFetchMode fetchMode)
		{
			if (row == null)
				return null;
			if (fetchMode.HasFlag(AvailabilityFetchMode.TryOptimize) && _detailsRequested++ == 5)
			{
				//package loading and caching
				var select = new PXSelectReadonly2<POReceiptLine,
					InnerJoin<INSiteStatus,
						On<POReceiptLine.FK.INSiteStatus>,
					LeftJoin<INLocationStatus,
						On<POReceiptLine.FK.INLocationStatus>,
					LeftJoin<INLotSerialStatus,
						On<POReceiptLine.FK.INLotSerialStatus>>>>,
					Where<POReceiptLine.receiptType, Equal<Current<POReceipt.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>>(sender.Graph);
				using (new PXFieldScope(select.View, typeof(INSiteStatus), typeof(INLocationStatus), typeof(INLotSerialStatus)))
				{
					foreach(PXResult<POReceiptLine, INSiteStatus, INLocationStatus, INLotSerialStatus> res in select.Select())
					{
						INSiteStatus siteStatus = res;
						INLocationStatus locationStatus = res;
						INLotSerialStatus lotSerialStatus = res;

						INSiteStatus.PK.StoreCached(sender.Graph, siteStatus);
						if (locationStatus.LocationID != null)
							INLocationStatus.PK.StoreCached(sender.Graph, locationStatus);
						if (lotSerialStatus?.LotSerialNbr != null)
							IN.INLotSerialStatus.PK.StoreCached(sender.Graph, lotSerialStatus);
					}
				}
			}
			return base.AvailabilityFetch(sender, row, fetchMode);
		}

		public override void DefaultLotSerialNbr(PXCache sender, POReceiptLineSplit row)
		{
			if (row.ReceiptType == POReceiptType.TransferReceipt)
				row.AssignedNbr = null;
			else
				base.DefaultLotSerialNbr(sender, row);
		}
		#endregion

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, POReceiptLine Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<boolTrue>>>();
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(Row.LotSerialNbr))
						cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}
		}

		protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
		{
			POReceiptLine line = row as POReceiptLine;
			if (line != null && line.LineType == POLineType.GoodsForDropShip 
				&& lotSerClass != null && lotSerClass.LotSerTrack != null && lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered )
			{
				return INLotSerTrack.Mode.Create;
			}
			else
			{
				return base.GetTranTrackMode(row, lotSerClass);
			}
		}
	}

	public class POReceiptLineSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public POReceiptLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			this._ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<POReceipt.receiptDate, POReceipt.hold>(e.Row, e.OldRow))
			{
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				foreach (POReceiptLineSplit split in PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.Hold = (bool?)sender.GetValue<POReceipt.hold>(e.Row);
							plan.PlanDate = (DateTime?)sender.GetValue<POReceipt.receiptDate>(e.Row);
						}
					}
				}

				foreach(INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<POReceipt.noteID>>>>.Select(sender.Graph))
				{
					plan.Hold = (bool?)sender.GetValue<POReceipt.hold>(e.Row);
					plan.PlanDate = (DateTime?)sender.GetValue<POReceipt.receiptDate>(e.Row);

					plancache.MarkUpdated(plan);
				}
			}
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			POReceiptLineSplit split_Row = (POReceiptLineSplit)orig_Row;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			DateTime? receiptDate = (DateTime?)cache.GetValue(cache.Current, this._ParentOrderDate.Name);
			POReceiptLine parent = null;
			if (plan_Row.IsTemporary != true)
			{
				parent = (POReceiptLine)PXParentAttribute.SelectParent(sender, orig_Row, typeof(POReceiptLine));
				if (parent == null)
					return null;
			}

			plan_Row.BAccountID = parent?.VendorID;
			if (split_Row.PONbr != null)
			{
				plan_Row.OrigPlanLevel = INPlanLevel.Site;
			}

			switch (split_Row.LineType)
			{
				case POLineType.GoodsForInventory:
				case POLineType.GoodsForReplenishment:
                case POLineType.GoodsForManufacturing:
                    if (parent != null && parent.OrigTranType == INTranType.Transfer)
                    {                       
                        if (plan_Row.OrigNoteID == null)
		                    plan_Row.OrigNoteID = parent.OrigNoteID;

	                    plan_Row.OrigPlanLevel =
		                    (parent.OrigToLocationID != null ? INPlanLevel.Location : INPlanLevel.Site)
		                    | (parent.OrigIsLotSerial == true ? INPlanLevel.LotSerial : INPlanLevel.Site);

	                    plan_Row.PlanType = parent.OrigIsFixedInTransit == true ? INPlanConstants.Plan45 : INPlanConstants.Plan43;
                    }
                    else
                    {
                        plan_Row.PlanType = INPlanConstants.Plan71;
                        plan_Row.Reverse = split_Row.ReceiptType == POReceiptType.POReturn;
                    }
					break;
				case POLineType.GoodsForSalesOrder:
					if (split_Row.ReceiptType == POReceiptType.POReceipt)
					{
						plan_Row.PlanType = INPlanConstants.Plan77;
					}
					else
					{
						throw new PXException();
					}
					break;
                case POLineType.GoodsForServiceOrder:
                    if (split_Row.ReceiptType == POReceiptType.POReceipt)
                    {
                        plan_Row.PlanType = INPlanConstants.PlanF9;
                    }
                    else
                    {
                        throw new PXException();
                    }
                    break;
				case POLineType.GoodsForDropShip:
					if (split_Row.ReceiptType == POReceiptType.POReceipt)
					{
						plan_Row.PlanType = INPlanConstants.Plan75;
					}
					else
					{
						throw new PXException();
					} 
					break;
				default:
					return null;
			}

            plan_Row.OrigPlanType = split_Row.OrigPlanType;
			plan_Row.InventoryID= split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = receiptDate;
			plan_Row.PlanQty = split_Row.BaseQty;

			plan_Row.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}
	
		#endregion
	}

    /// <summary>
    /// Specialized for POLine version of the CrossItemAttribute.<br/> 
    /// Providing an Inventory ID selector for the field, it allows also user <br/>
    /// to select both InventoryID and SubItemID by typing AlternateID in the control<br/>
    /// As a result, if user type a correct Alternate id, values for InventoryID, SubItemID, <br/>
    /// and AlternateID fields in the row will be set.<br/>
    /// In this attribute, InventoryItems with a status inactive, markedForDeletion,<br/>
    /// noPurchase and noRequest are filtered out. It also fixes  INPrimaryAlternateType parameter to VPN <br/>    
    /// This attribute may be used in combination with AlternativeItemAttribute on the AlternateID field of the row <br/>
    /// <example>
    /// [POLineInventoryItem(Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>), IN.Messages.ItemCannotPurchase)]
	public class POLineInventoryItemAttribute : CrossItemAttribute
	{
	
        /// <summary>
        /// Default ctor
        /// </summary>
		public POLineInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), INPrimaryAlternateType.VPN)
		{
		}
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class POReceiptLineInventoryAttribute : CrossItemAttribute
	{
		public POReceiptLineInventoryAttribute(Type receiptType)
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), INPrimaryAlternateType.VPN)
		{
			var condition = BqlTemplate.OfCondition<
				Where<Current2<BqlPlaceholder.A>, Equal<POReceiptType.transferreceipt>,
					Or<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>>>
				.Replace<BqlPlaceholder.A>(receiptType)
				.ToType();

			_Attributes.Add(new PXRestrictorAttribute(condition, IN.Messages.ItemCannotPurchase));
		}
	}

	#region POOpenPeriod
	public class POOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public POOpenPeriodAttribute(Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool useMasterOrganizationIDByDefault = false,
			Type masterFinPeriodIDType = null)
			: base(
				typeof(Search<FinPeriod.finPeriodID,
							Where<FinPeriod.iNClosed, Equal<False>,
									And<FinPeriod.aPClosed, Equal<False>,
									And<FinPeriod.status, Equal<FinPeriod.status.open>>>>>),
				sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
				masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		public POOpenPeriodAttribute(Type SourceType)
			: this(SourceType, null)
		{
		}

		public POOpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion

		#region Implementation

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError)
			{
				if (finPeriod.APClosed == true)
				{
					result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: AP.Messages.FinancialPeriodClosedInAP);
				}

				if (finPeriod.INClosed == true)
				{
					result.Aggregate(HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: IN.Messages.FinancialPeriodClosedInIN));
				}
			}
			
			return result;
		}

		#endregion
	}
	#endregion

    /// <summary>
    /// This attribute defines, if the vendor and it's location specified 
    /// are the preffered Vendor for the inventory item. May be placed on field of boolean type, 
    /// to display this information dynamically 
    /// <example>
    /// [PODefaultVendor(typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID))]
    /// </example>
    /// </summary>
	public class PODefaultVendor : PXEventSubscriberAttribute, IPXRowSelectingSubscriber
	{
		private Type inventoryID;
		private Type vendorID;
		private Type vendorLocationID;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Field which contains inventory id, for which Vendor/location is checked for beeng a preferred Vendor</param>
        /// <param name="vendorID">Must be IBqlField. Field which contains VendorID of the vendor checking for beeng a preferred Vendor</param>
        /// <param name="vendorLocationID">Must be IBqlField. Field which contains VendorLocationID of the vendor checking for beeng a preferred Vendor</param>
		public PODefaultVendor(Type inventoryID, Type vendorID, Type vendorLocationID)
		{
			this.inventoryID = inventoryID;
			this.vendorID    = vendorID;
			this.vendorLocationID = vendorLocationID;
		}

		#region IPXRowSelectingSubscriber Members
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			object itemID = sender.GetValue(e.Row, this.inventoryID.Name);
			object vendorID = sender.GetValue(e.Row, this.vendorID.Name);
			object vendorLocationID = sender.GetValue(e.Row, this.vendorLocationID.Name);

			if (itemID == null || vendorID == null) return;
						
			using (new PXConnectionScope())
			{
				InventoryItem item = InventoryItem.PK.Find(sender.Graph, (int?)itemID);
				sender.SetValue(e.Row, _FieldName, item != null && object.Equals(item.PreferredVendorID, vendorID) && object.Equals(item.PreferredVendorLocationID, vendorLocationID));	
			}												
		}
		#endregion
	}

	public class POXRefUpdate : PXEventSubscriberAttribute
	{
		public POXRefUpdate(Type inventoryID, Type subItem, Type vendorID)
		{
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

		}
	}

	public class POVendorInventorySelect<Table, Join, Where, PrimaryType> : PXSelectJoin<Table, Join, Where>
		where Table : POVendorInventory, new()
		where Join : class, PX.Data.IBqlJoin, new()
		where Where : PX.Data.IBqlWhere, new()
		where PrimaryType : class, IBqlTable, new()
	{
		protected const string _UPDATEVENDORPRICE_COMMAND = "UpdateVendorPrice";
		protected const string _UPDATEVENDORPRICE_VIEW    = "VendorInventory$UpdatePrice";

		public POVendorInventorySelect(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected); 
			
			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);				
		}

		public POVendorInventorySelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected);

			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);
		}

		private void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			var uiAtt = new PXUIFieldAttribute { DisplayName = PXMessages.LocalizeNoPrefix(displayName)};
			graph.Actions[name] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new Type[] { typeof(PrimaryType) }), 
				new object[] { graph, name, handler, uiAtt });
		}

		protected virtual InventoryItem ReadInventory(object current)
		{
			return InventoryItem.PK.Find(this._Graph, ((Table)current).InventoryID);
		}

		protected virtual void OnRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			Table current = e.Row as Table;
			if (current == null) return;
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup setup = (INSetup)cache.Graph.Caches[typeof(INSetup)].Current;
				if (setup != null && setup.UseInventorySubItem == true)
				{
					InventoryItem item = ReadInventory(current);
					if (item != null && item.DefaultSubItemID == null && item.StkItem == true)
						current.OverrideSettings = true;
				}
			}
			if (!cache.Graph.IsCopyPasteContext)
			{
			UpdateXRef(current);			
		}
		}

		protected virtual void OnParentRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;
				PXUIFieldAttribute.SetVisible<POVendorInventory.overrideSettings>
					(sender.Graph.Caches[typeof(POVendorInventory)], null, setup.UseInventorySubItem == true);
			}
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Table row = (Table)e.Row;
			PXUIFieldAttribute.SetVisible<POVendorInventory.curyID>(sender, null,
				PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				if (row == null) return;
				INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;

				InventoryItem item = ReadInventory(row);

				bool isEnabled =
					row.OverrideSettings == true ||
					item == null ||
					setup.UseInventorySubItem != true ||
					item.DefaultSubItemID == row.SubItemID;
				PXUIFieldAttribute.SetEnabled<POVendorInventory.overrideSettings>(sender, row, setup.UseInventorySubItem == true && item != null && item.StkItem == true);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.addLeadTimeDays>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.eRQ>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.lotSize>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.maxOrdQty>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdFreq>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdQty>(sender, row, isEnabled);
				PXUIFieldAttribute.SetEnabled<POVendorInventory.subItemID>(sender, row, item != null && item.StkItem == true);
			}		
		}
		
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Table current = e.Row as Table;
			Table old     = e.OldRow as Table;
			if (current == null) return;
			
			InventoryItem item = ReadInventory(current);
	
			if (item != null && item.DefaultSubItemID != null && item.DefaultSubItemID == current.SubItemID)
			{				
				foreach (POVendorInventory vi in
					PXSelect<POVendorInventory,
					Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
						And2<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
									 Or<Where<Required<POVendorInventory.vendorLocationID>, IsNull, And<POVendorInventory.vendorLocationID, IsNull>>>>,
						And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.subItemID, NotEqual<Required<POVendorInventory.subItemID>>,
						And<POVendorInventory.overrideSettings, Equal<boolFalse>>>>>>>
						.Select(sender.Graph, current.VendorID, current.VendorLocationID, current.VendorLocationID, current.InventoryID, current.SubItemID))
				{
					if (vi.RecordID == current.RecordID) continue;
					POVendorInventory rec = PXCache<POVendorInventory>.CreateCopy(vi);								
					rec.AddLeadTimeDays = current.AddLeadTimeDays;
					rec.ERQ = current.ERQ;
					rec.VLeadTime = current.VLeadTime;
					rec.LotSize = current.LotSize;
					rec.MaxOrdQty = current.MaxOrdQty;
					rec.MinOrdFreq = current.MinOrdFreq;
					rec.MinOrdQty = current.MinOrdQty;
					sender.Update(rec);			
				}				
			}

			if (!sender.Graph.IsCopyPasteContext && !IsEqualByItemXRef(current, old))
			{
				if (!ExistRelatedPOVendorInventory(old)) DeleteXRef(old);
				UpdateXRef(current);
			}
		}

		protected virtual void OnRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = (Table)e.Row;

			if (!ExistRelatedPOVendorInventory(row)) DeleteXRef(row);
		}

		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if(e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				InventoryItem item = ReadInventory(e.Row);
				PXDefaultAttribute.SetPersistingCheck<POVendorInventory.subItemID>(sender, e.Row, item == null || item.StkItem == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		static bool IsEqualByItemXRef(Table op1, Table op2)
		{
			return (op1.VendorID == op2.VendorID
				&& op1.InventoryID == op2.InventoryID
				&& op1.SubItemID == op2.SubItemID
				&& op1.VendorInventoryID == op2.VendorInventoryID);
		}

		private void DeleteXRef(Table doc)
		{
			PXCache cache = _Graph.Caches[typeof(INItemXRef)];
			if (doc.InventoryID.HasValue && doc.SubItemID.HasValue && doc.VendorID.HasValue
						&& !String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>,
										And<INItemXRef.alternateType, Equal<INAlternateType.vPN>>>>>>>.										
										Select(_Graph, doc.VendorInventoryID, doc.InventoryID, doc.SubItemID, doc.VendorID))
				{
					cache.Delete(it);
				}
			}
		}

		private bool ExistRelatedPOVendorInventory(Table doc)
		{
			return PXSelect<POVendorInventory,
				Where<POVendorInventory.vendorInventoryID, Equal<Required<POVendorInventory.vendorInventoryID>>,
					And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
					And<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
					And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
					And<POVendorInventory.recordID, NotEqual<Required<POVendorInventory.recordID>>>>>>>>.
				SelectWindowed(_Graph, 0, 1, doc.VendorInventoryID, doc.InventoryID, doc.SubItemID, doc.VendorID, doc.RecordID).
				Any();
		}

		private void UpdateXRef(Table doc)
		{
			PXCache cache = _Graph.Caches[typeof(INItemXRef)];
			if (doc.InventoryID.HasValue && doc.SubItemID.HasValue && doc.VendorID.HasValue
						&& !String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				INItemXRef itemXRef = null;
				INItemXRef globalXRef = null;
				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<Where2<Where<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>,
											And<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>>>,
										Or<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>
										>>>>>>>>, OrderBy<Asc<INItemXRef.alternateType>>>.
										Select(_Graph, doc.VendorInventoryID, doc.InventoryID, doc.SubItemID, doc.VendorID))
				{
					if (it.AlternateType == INAlternateType.VPN)
					{
						itemXRef = it;
					}
					else
					{
						if (globalXRef == null)
							globalXRef = it;
					}
				}
				if (itemXRef == null)
				{
					if (globalXRef == null)
					{
						itemXRef = new INItemXRef();
						Copy(itemXRef, doc);
						itemXRef = (INItemXRef)cache.Insert(itemXRef);
					}
				}
				else
				{
					INItemXRef itemXRef2 = (INItemXRef)cache.CreateCopy(itemXRef);
					Copy(itemXRef2, doc);
					itemXRef = (INItemXRef)cache.Update(itemXRef2);
				}
			}
		}

		static void Copy(INItemXRef dest, Table src)
		{
			dest.InventoryID = src.InventoryID;
			if(PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			dest.SubItemID = src.SubItemID;
			dest.BAccountID = src.VendorID;
			dest.AlternateType = INAlternateType.VPN;
			dest.AlternateID = src.VendorInventoryID;						
			dest.UOM = src.PurchaseUnit;
		}
	}


    /// <summary>
    /// Specialized for Landed Cost version of VendorAttribute.
    /// Displayes only Vendors having LandedCostVendor = true.
    /// Employee and non-active vendors are filtered out
    /// <example>
    /// [LandedCostVendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<AP.Vendor.landedCostVendor,Equal<boolTrue>>), Messages.VendorIsNotLandedCostVendor)]
	public class LandedCostVendorActiveAttribute : AP.VendorNonEmployeeActiveAttribute
	{
        /// <summary>
        /// Default ctor.
        /// </summary>
		public LandedCostVendorActiveAttribute()
			: base()
		{
		}

		//public override void Verify(PXCache sender, Vendor item)
		//{
		//    if (item.LandedCostVendor == false)
		//    {
		//        throw new PXException(Messages.VendorIsNotLandedCostVendor);
		//    }
		//}
	}

	public class POLocationAvailAttribute : LocationAvailAttribute
	{
		public POLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType)
			: base(InventoryType, SubItemType, SiteIDType, TranType, InvtMultType)
		{
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row == null) return;

			if (POLineType.IsStock(row.LineType) && row.POType != null && row.PONbr != null && row.POLineNbr != null)
			{
				POLine poLine = PXSelect<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
						And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(sender.Graph, row.POType, row.PONbr, row.POLineNbr);

				if (poLine != null && poLine.TaskID != null)
				{
					INLocation selectedLocation = PXSelect<INLocation, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.taskID, Equal<Required<INLocation.taskID>>>>>.Select(sender.Graph, row.SiteID, poLine.TaskID);

					if (selectedLocation != null )
					{
						e.NewValue = selectedLocation.LocationID;
						return;
					}
					else
					{
						e.NewValue = null;
						return;
					}
				}
			}
			
			base.FieldDefaulting(sender, e);
		}
	}

	public class POLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
		public POLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
			: base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
		{
		}

		public POLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType)
			: base(InventoryType, SubItemType, LocationType)
		{
		}

		protected override bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			POReceiptLineSplit split = row as POReceiptLineSplit;

			if (split != null && split.LineType == POLineType.GoodsForDropShip)
			{
				return true;
			}
			else
			{
				return base.IsTracked(row, lotSerClass, tranType, invMult);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			string lineType = (string)sender.GetValue<POReceiptLineSplit.lineType>(e.Row);

			if (lineType == POLineType.GoodsForDropShip)
			{
				if ( ((INLotSerClass)item).RequiredForDropship == true && IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
				{
					((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
				}
			}
			else
			{
				base.RowPersisting(sender, e);
			}


			if (IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
			{
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}
	}

	public class POExpireDateAttribute : INExpireDateAttribute
	{
		public POExpireDateAttribute(Type InventoryType)
			: base(InventoryType)
		{
		}


		protected override bool IsTrackExpiration(PXCache sender, ILSMaster row)
		{
			string lineType = (string) sender.GetValue<POReceiptLineSplit.lineType>(row);
			
			if (lineType == POLineType.GoodsForDropShip)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

				if (item == null)
					return false;
				else
					return ((INLotSerClass)item).LotSerTrackExpiration == true;
			}
			else
			{
				return base.IsTrackExpiration(sender, row);
			}

			
		}
	}

	public class POReports : PX.SM.ReportUtils
	{
		public const string PurchaseOrderReportID = "PO641000";
	}
}
