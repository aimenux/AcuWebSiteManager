using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Diagnostics;

namespace PX.Objects.IN
{
	public class INKitSpecMaint : PXGraph<INKitSpecMaint, INKitSpecHdr>
	{ 

		#region Public Members


		public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

		public PXSelect<INKitSpecStkDet,
			Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>,
			OrderBy<Asc<INKitSpecStkDet.kitInventoryID, Asc<INKitSpecStkDet.revisionID, Asc<INKitSpecStkDet.lineNbr>>>>> StockDet;

		public PXSelect<INKitSpecNonStkDet,
			Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>,
			OrderBy<Asc<INKitSpecNonStkDet.kitInventoryID, Asc<INKitSpecNonStkDet.revisionID, Asc<INKitSpecNonStkDet.lineNbr>>>>> NonStockDet;

		#endregion Public Members        
		protected virtual void INKitSpecHdr_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INKitSpecHdr row = e.Row as INKitSpecHdr;
			if (row != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.KitInventoryID);
				if (item != null)
				{
					PXUIFieldAttribute.SetEnabled<INKitSpecHdr.kitSubItemID>(sender, row, item.StkItem == true);
                }
			}
		}

	    protected virtual void INKitSpecHdr_IsNonStock_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
	    {
	        INKitSpecHdr row = e.Row as INKitSpecHdr;
	        if (row != null)
	        {
	            InventoryItem item = InventoryItem.PK.Find(this, row.KitInventoryID);
	            if (item != null)
	            {
	                row.IsNonStock = item.StkItem != true;
	                e.ReturnValue = item.StkItem != true;
	            }
	        }
	    }


	    protected virtual void INKitSpecHdr_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecHdr row = e.Row as INKitSpecHdr;
            if (row != null && e.Operation == PXDBOperation.Insert)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.KitInventoryID);
				if (item.StkItem != true)
				{
					INKitSpecHdr existing =
						PXSelectReadonly<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>
							.Select(this);
					if (existing != null)
					{
						if (sender.RaiseExceptionHandling<INKitSpecHdr.revisionID>(e.Row, row.RevisionID,
							new PXSetPropertyException(Messages.SingleRevisionForNS)))
						{
							throw new PXRowPersistingException(typeof (INKitSpecHdr.revisionID).Name, null, Messages.SingleRevisionForNS);
						}
					}
				}
			}
		}

		protected virtual void INKitSpecStkDet_CompInventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
					Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
					And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
					And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
					And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(e.NewValue, row.CompSubItemID);

				if (res.Count > 0)
				{
					InventoryItem component = InventoryItem.PK.Find(this, row.CompInventoryID);
					var ex = new PXSetPropertyException(Messages.KitItemMustBeUniqueAccrosSubItems);
					ex.ErrorValue = component?.InventoryCD;
					RaiseOnKitNotUniqueException(e, ex);
				}
								
				InventoryItem kit = InventoryItem.PK.Find(this, Hdr.Current.KitInventoryID);
				if (kit != null)
				{
					INLotSerClass kitLotSerClass = INLotSerClass.PK.Find(this, kit.LotSerClassID) ?? new INLotSerClass();

					InventoryItem component = InventoryItem.PK.Find(this, (int?)e.NewValue);
					if (component != null)
					{
						INLotSerClass compLotSerClass = INLotSerClass.PK.Find(this, component.LotSerClassID) ?? new INLotSerClass();

						//Serial number components are valid only for serial numbered kit validation:
						if (kit.StkItem == true && kitLotSerClass.LotSerTrack != INLotSerTrack.SerialNumbered && compLotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							var ex = new PXSetPropertyException(Messages.SNComponentInSNKit);
							ex.ErrorValue = component.InventoryCD;
							RaiseSNComponentInSNKitException(e, ex);
						}

						//Manually assigned components are not supported in kits. 
						if (kit.StkItem != true && compLotSerClass.IsManualAssignRequired == true)
						{
							var ex = new PXSetPropertyException(Messages.WhenUsedComponentInKit);
							ex.ErrorValue = component.InventoryCD;

							RaiseUnassignedComponentInKitException(e, ex);
						}
					}
				}
			}
		}

		protected virtual void INKitSpecStkDet_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				InventoryItem component = InventoryItem.PK.Find(this, row.CompInventoryID);
				INLotSerClass lsClass = INLotSerClass.PK.Find(this, component?.LotSerClassID);
				if(lsClass != null 
					&& lsClass.LotSerTrack == INLotSerTrack.SerialNumbered
					&& !string.Equals(component.BaseUnit, (string) e.NewValue, StringComparison.InvariantCultureIgnoreCase) )
				{
					var ex = new PXSetPropertyException(Messages.SerialNumberedComponentMustBeInBaseUnitOnly, component.BaseUnit);
					ex.ErrorValue = e.NewValue;
					RaiseSerialTrackedComponentIsNotInBaseUnitException(e, ex);
				}
			}
		}

		/// <summary>
		///  Raised during the INKitSpecStkDet_CompInventoryID verification if component Item is not unique for the given Kit accross Component ID and Subitem combinations.
		/// </summary>
		public virtual void RaiseOnKitNotUniqueException(PXFieldVerifyingEventArgs e, PXSetPropertyException ex)
		{
			throw ex;
		}

		/// <summary>
		///  Raised during the INKitSpecStkDet_CompInventoryID verification if component is not SerialNumbered and Kit is SerialNumbered.
		/// </summary>
		public virtual void RaiseSNComponentInSNKitException(PXFieldVerifyingEventArgs e, PXSetPropertyException ex)
		{
			throw ex;
		}

		/// <summary>
		/// Raised during the INKitSpecStkDet_CompInventoryID verification if component's LotSerialClass.IsUnassigned property is True
		/// </summary>
		public virtual void RaiseUnassignedComponentInKitException(PXFieldVerifyingEventArgs e, PXSetPropertyException ex)
		{
			throw ex;
		}

		/// <summary>
		/// Raised during the INKitSpecStkDet_UOM_FieldVerifying verification if component is serial numbered and the UOM used is not the Base Unit.
		/// </summary>
		public virtual void RaiseSerialTrackedComponentIsNotInBaseUnitException(PXFieldVerifyingEventArgs e, PXSetPropertyException ex)
		{
			throw ex;
		}

		protected virtual void INKitSpecStkDet_CompSubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
				Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
				And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(row.CompInventoryID, e.NewValue);

				if (res.Count > 0)
				{
					INSubItem subitem = INSubItem.PK.Find(this, (int?)e.NewValue);
					var ex = new PXSetPropertyException(Messages.KitItemMustBeUniqueAccrosSubItems);
					ex.ErrorValue = subitem?.SubItemCD;
					throw ex;
				}
			}
		}
		
		protected virtual void INKitSpecStkDet_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INKitSpecStkDet row = e.NewRow as INKitSpecStkDet;
			if (row != null)
			{
				if (row.AllowQtyVariation == true)
				{
					if(	((row.MinCompQty != null) && (row.DfltCompQty < row.MinCompQty))
						|| ((row.MaxCompQty != null) && (row.DfltCompQty > row.MaxCompQty))	)
					{
                        throw new PXSetPropertyException(Messages.DfltQtyShouldBeBetweenMinAndMaxQty);
					}
				}

				//if (row.KitInventoryID == row.CompInventoryID)
				//{
				//   throw new PXSetPropertyException(Messages.KitMayNotIncludeItselfAsComponentPart);
				//}
			}
		}

		protected virtual void INKitSpecStkDet_CompInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.CompInventoryID);
				if (item != null)
				{
					row.UOM = item.BaseUnit;
				}
			}
		}

        
		protected virtual void INKitSpecStkDet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
            if (row == null)
                return;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
					Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
					And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
					And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
					And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(row.CompInventoryID, row.CompSubItemID);

				if (res.Count > 1)
				{
					InventoryItem item = InventoryItem.PK.Find(this, row.CompInventoryID);
					if (sender.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(e.Row, item.InventoryCD, new PXException(Messages.KitItemMustBeUniqueAccrosSubItems)))
					{
						throw new PXRowPersistingException(typeof(INKitSpecStkDet.compInventoryID).Name, item.InventoryCD, Messages.KitItemMustBeUniqueAccrosSubItems);
					}
				}
			
		}


		protected virtual void INKitSpecNonStkDet_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INKitSpecNonStkDet row = e.NewRow as INKitSpecNonStkDet;
			if (row != null)
			{
				if (row.AllowQtyVariation == true)
				{
					if (((row.MinCompQty != null) && (row.DfltCompQty < row.MinCompQty))
						|| ((row.MaxCompQty != null) && (row.DfltCompQty > row.MaxCompQty)))
					{
						throw new PXSetPropertyException(typeof(INKitSpecNonStkDet.dfltCompQty).Name, null, Messages.DfltQtyShouldBeBetweenMinAndMaxQty);
					}
				}

				if (row.KitInventoryID == row.CompInventoryID)
				{
                    throw new PXSetPropertyException(typeof(INKitSpecNonStkDet.compInventoryID).Name, null, Messages.KitMayNotIncludeItselfAsComponentPart);
				}
			}
		}       

		protected virtual void INKitSpecNonStkDet_CompInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
            if (row != null)
            {
                InventoryItem item = InventoryItem.PK.Find(this, row.CompInventoryID);
                if (item != null)
                {
                    row.UOM = item.BaseUnit;
                }
            }
		}

		protected virtual void INKitSpecNonStkDet_CompInventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecNonStkDet> select = new PXSelect<INKitSpecNonStkDet,
					Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
					And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
					And<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>>>>(this);

				PXResultset<INKitSpecNonStkDet> res = select.Select(e.NewValue);

				if (res.Count > 0)
				{
					InventoryItem item = InventoryItem.PK.Find(this, (int?)e.NewValue);
					var ex = new PXSetPropertyException(Messages.KitItemMustBeUnique);
					ex.ErrorValue = item?.InventoryCD;
					throw ex;
				}
			}
		}

		protected virtual void INKitSpecNonStkDet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
            if (row == null)
                return;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

				PXSelectBase<INKitSpecNonStkDet> select = new PXSelect<INKitSpecNonStkDet,
					Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
					And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
					And<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>>>>(this);

				PXResultset<INKitSpecNonStkDet> res = select.Select(row.CompInventoryID);

				if (res.Count > 1)
				{
					InventoryItem item = InventoryItem.PK.Find(this, row.CompInventoryID);
					if (sender.RaiseExceptionHandling<INKitSpecNonStkDet.compInventoryID>(e.Row, item.InventoryCD, new PXException(Messages.KitItemMustBeUnique)))
					{
						throw new PXRowPersistingException(typeof(INKitSpecNonStkDet.compInventoryID).Name, item.InventoryCD, Messages.KitItemMustBeUnique);
					}
				}
			}

        [NonStockItem(DisplayName = "Component ID")]
        [PXDefault()]
        [PXRestrictor(typeof(Where<InventoryItem.inventoryID, NotEqual<Current<INKitSpecHdr.kitInventoryID>>>), Messages.UsingKitAsItsComponent)]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolFalse>>), Messages.NonStockKitInKit)]
        protected virtual void INKitSpecNonStkDet_CompInventoryID_CacheAttached(PXCache sender)
        { }
		
	}
}
