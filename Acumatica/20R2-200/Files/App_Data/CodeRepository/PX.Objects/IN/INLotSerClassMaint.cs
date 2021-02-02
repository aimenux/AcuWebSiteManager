using System;
using System.Collections;
using PX.Data;
using System.Linq;
using PX.Objects.SO;
using PX.Objects.PO;

namespace PX.Objects.IN
{
	public class INLotSerClassMaint : PXGraph<INLotSerClassMaint, INLotSerClass>
	{
		private const string lotSerNumValueFieldName = nameof(INLotSerClassLotSerNumVal.LotSerNumVal);

		public PXSelect<INLotSerClass> lotserclass;
		public PXSelect<INLotSerSegment, Where<INLotSerSegment.lotSerClassID, Equal<Current<INLotSerClass.lotSerClassID>>>> lotsersegments;
		public PXSelect<INLotSerClassLotSerNumVal, Where<INLotSerClassLotSerNumVal.lotSerClassID, Equal<Current<INLotSerClass.lotSerClassID>>>> lotSerNumVal;

		public INLotSerClassMaint()
		{
			lotserclass.Cache.Fields.Add(lotSerNumValueFieldName);
			FieldSelecting.AddHandler(typeof(INLotSerClass), lotSerNumValueFieldName, LotSerNumValueFieldSelecting);
			FieldUpdating.AddHandler(typeof(INLotSerClass), lotSerNumValueFieldName, LotSerNumValueFieldUpdating);
		}

		private void SetParentChanged(PXCache sender, object Row)
		{
			object parent = PXParentAttribute.SelectParent(sender, Row);

			if (parent != null && lotserclass.Cache.GetStatus(parent) == PXEntryStatus.Notchanged)
			{
				lotserclass.Cache.SetStatus(parent, PXEntryStatus.Updated);
			}
		}

		protected virtual void INLotSerClass_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INLotSerClass row = (INLotSerClass) e.NewRow;
            INLotSerClass oldrow = (INLotSerClass)e.Row;
			if (oldrow != null && row.LotSerAssign != oldrow.LotSerAssign)
				HasInventoryItemsInUse(row);
			if (row.LotSerTrackExpiration != true && row.LotSerIssueMethod == INLotSerIssueMethod.Expiration 
                && row.LotSerTrack != INLotSerTrack.NotNumbered && row.LotSerAssign != INLotSerAssign.WhenUsed)
			{
                throw new PXSetPropertyException(Messages.LotSerTrackExpirationInvalid, typeof(INLotSerClass.lotSerIssueMethod).Name);
			}
		}

		protected virtual void HasInventoryItemsInUse(INLotSerClass item)
        {
			InventoryItem inventoryUse = PXSelectReadonly<InventoryItem,
				Where<InventoryItem.lotSerClassID, Equal<Required<InventoryItem.lotSerClassID>>>>.
				SelectWindowed(this, 0, 1, item.LotSerClassID);
			if(inventoryUse != null)
                throw new PXSetPropertyException(Messages.LotSerAssignCannotBeChanged, inventoryUse.InventoryCD);
        }

		protected virtual void INLotSerClass_LotSerTrack_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INLotSerClass row = (INLotSerClass)e.Row;
			if (row != null && row.LotSerTrack == INLotSerTrack.NotNumbered)
			{
				row.RequiredForDropship = false;
				foreach (INLotSerSegment segment in this.lotsersegments.Select())
					this.lotsersegments.Delete(segment);
			}
			if (row.LotSerTrack != INLotSerTrack.SerialNumbered || row.AutoNextNbr != true)
			{
				row.AutoSerialMaxCount = 0;
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		protected virtual void INLotSerClass_LotSerIssueMethod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected virtual void INLotSerClass_IsManualAssignRequired_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass row = (INLotSerClass)e.Row;
			if (row == null)
				return;

			if (row.IsManualAssignRequired != null && e.NewValue != null && row.IsManualAssignRequired != (bool?)e.NewValue)
				HasOpenDocuments(sender, row, row.LotSerIssueMethod);
		}

		protected virtual void HasOpenDocuments(PXCache sender, INLotSerClass row, string oldValue)
		{
			foreach (PXResult<InventoryItem, PX.Objects.SO.Table.SOShipLineSplit> openShipmentSplit in PXSelectJoin<InventoryItem,
				InnerJoin<PX.Objects.SO.Table.SOShipLineSplit, On<PX.Objects.SO.Table.SOShipLineSplit.inventoryID, Equal<InventoryItem.inventoryID>, And<PX.Objects.SO.Table.SOShipLineSplit.confirmed, Equal<False>>>>,
				Where<InventoryItem.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.SelectSingleBound(this, null, row.LotSerClassID))
			{
				InventoryItem item = (InventoryItem)openShipmentSplit;
				PX.Objects.SO.Table.SOShipLineSplit shipmentLineSplit = (PX.Objects.SO.Table.SOShipLineSplit)openShipmentSplit;

				throw new PXException(
					row.LotSerAssign == INLotSerAssign.WhenReceived
						? Messages.LotSerIssueMethodCannotBeChangedShipment
						: Messages.LotSerAutoNextNbrCannotBeChangedShipment,
					shipmentLineSplit.ShipmentNbr,
					item.InventoryCD);
			}
		}

		protected virtual void INLotSerClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass row = (INLotSerClass)e.Row;
			if (row != null)
			{
				INItemClass classUse = PXSelectReadonly<INItemClass, 
					Where<INItemClass.lotSerClassID, Equal<Required<INItemClass.lotSerClassID>>>>.
					SelectWindowed(this, 0, 1, row.LotSerClassID);
				InventoryItem itemUse = PXSelectReadonly<InventoryItem, 
					Where<InventoryItem.lotSerClassID, Equal<Required<InventoryItem.lotSerClassID>>>>.
					SelectWindowed(this, 0, 1, row.LotSerClassID);

				PXUIFieldAttribute.SetEnabled<INLotSerClass.lotSerTrack>(sender, row, classUse == null && itemUse == null);
				PXUIFieldAttribute.SetEnabled<INLotSerClass.lotSerTrackExpiration>(sender, row, classUse == null && itemUse == null);

				bool enable = row.LotSerTrack != INLotSerTrack.NotNumbered;

				this.lotsersegments.Cache.AllowInsert = enable;
				this.lotsersegments.Cache.AllowUpdate = enable;
				this.lotsersegments.Cache.AllowDelete = enable;

				PXUIFieldAttribute.SetEnabled<INLotSerClassLotSerNumVal.lotSerNumVal>(lotSerNumVal.Cache, lotSerNumVal.Current, enable && row.LotSerNumShared == true);
				PXUIFieldAttribute.SetEnabled<INLotSerClass.autoNextNbr>(sender, row, enable);
				PXUIFieldAttribute.SetEnabled<INLotSerClass.lotSerNumShared>(sender, row, enable);
				PXUIFieldAttribute.SetEnabled<INLotSerClass.requiredForDropship>(sender, row, row.LotSerTrack != INLotSerTrack.NotNumbered);
				PXUIFieldAttribute.SetEnabled<INLotSerClass.autoSerialMaxCount>(sender, row, enable && row.AutoNextNbr == true && row.LotSerTrack == INLotSerTrack.SerialNumbered);
				PXUIFieldAttribute.SetVisible<INLotSerClass.lotSerIssueMethod>(sender, row, row.LotSerAssign == INLotSerAssign.WhenReceived);
			}
		}

		protected virtual void INLotSerSegment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SetParentChanged(sender, e.Row);
		}
		
		protected virtual void INLotSerSegment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SetParentChanged(sender, e.Row);
		}

		protected virtual void INLotSerSegment_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SetParentChanged(sender, e.Row);
		}

		protected virtual void INLotSerSegment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerSegment row = e.Row as INLotSerSegment;
			if (row == null) return;
			if (row.SegmentType == INLotSerSegmentType.NumericVal)
				row.SegmentValue = lotSerNumVal.Current?.LotSerNumVal;
			PXUIFieldAttribute.SetEnabled<INLotSerSegment.segmentValue>(sender, row,
				row.SegmentType == INLotSerSegmentType.FixedConst || row.SegmentType == INLotSerSegmentType.DateConst);
		}

		#region LotSerNumVal events
		protected virtual void LotSerNumValueFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			lotSerNumVal.Current = (INLotSerClassLotSerNumVal)lotSerNumVal.View.SelectSingleBound(new object[] { e.Row });
			e.ReturnState = lotSerNumVal.Cache.GetStateExt<INLotSerClassLotSerNumVal.lotSerNumVal>(lotSerNumVal.Current);
		}

		protected virtual void LotSerNumValueFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			INLotSerClass lsClass = (INLotSerClass)e.Row;
			if (lsClass == null)
				return;
			var newNumValue = (string)e.NewValue;
			var currentNumValue = (INLotSerClassLotSerNumVal)lotSerNumVal.View.SelectSingleBound(new object[] { lsClass });
			var oldNumValue = currentNumValue?.LotSerNumVal;
			if(!sender.ObjectsEqual(oldNumValue, newNumValue))
			{
				if (lsClass.LotSerTrack != INLotSerTrack.NotNumbered)
				{
					if (currentNumValue == null)
					{
						currentNumValue = lotSerNumVal.Insert(new INLotSerClassLotSerNumVal
						{
							LotSerNumVal = newNumValue
						});
					}
					else
					{
						if (string.IsNullOrWhiteSpace(newNumValue))
						{
							lotSerNumVal.Delete(currentNumValue);
						}
						else
						{
							var copy = (INLotSerClassLotSerNumVal)lotSerNumVal.Cache.CreateCopy(currentNumValue);
							copy.LotSerNumVal = newNumValue;
							lotSerNumVal.Cache.Update(copy);
						}
					}
				}
			}
		}

		#endregion

		protected virtual void INLotSerClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INLotSerClass cls = (INLotSerClass) e.Row;
			if(cls == null)
				return;

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (cls.LotSerTrack != INLotSerTrack.NotNumbered && cls.LotSerNumShared == true)
				{
					var fieldState = (PXStringState)sender.GetValueExt(cls, lotSerNumValueFieldName);
					if(fieldState == null || fieldState.Value == null)
					{
						var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, fieldState.DisplayName);
						PXUIFieldAttribute.SetError<INLotSerClassLotSerNumVal.lotSerNumVal>(lotSerNumVal.Cache, null, exception.Message);
					}
				}
			}
		}

		public override void Persist()
		{
			foreach (INLotSerClass lsclass in lotserclass.Cache.Inserted)
			{
				lsclass.LotSerFormatStr = INLotSerialNbrAttribute.MakeFormatStr(lotserclass.Cache, lsclass);

                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.AutoNextNbr == true)
				{
					int NumbericCount = 0;
					foreach (INLotSerSegment lssegment in lotsersegments.View.SelectMultiBound(new object[] { lsclass }))
					{
						if (lssegment.SegmentType == INLotSerSegmentType.NumericVal)
						{
							NumbericCount++;
						}
					}

					if (NumbericCount == 0)
					{
						throw new PXException(Messages.NumericLotSerSegmentNotExists, Messages.NumericVal);
					}
					else if (NumbericCount > 1)
					{
						throw new PXException(Messages.NumericLotSerSegmentMultiple, Messages.NumericVal);
					}
				}
			}

			foreach (INLotSerClass lsclass in lotserclass.Cache.Updated)
			{
				lsclass.LotSerFormatStr = INLotSerialNbrAttribute.MakeFormatStr(lotserclass.Cache, lsclass);

                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.AutoNextNbr == true)
				{
					int NumbericCount = 0;
					foreach (INLotSerSegment lssegment in lotsersegments.View.SelectMultiBound(new object[] { lsclass }))
					{
						if (lssegment.SegmentType == INLotSerSegmentType.NumericVal)
						{
							NumbericCount++;
						}
					}

					if (NumbericCount == 0)
					{
						throw new PXException(Messages.NumericLotSerSegmentNotExists, Messages.NumericVal);
					}
					else if (NumbericCount > 1)
					{
						throw new PXException(Messages.NumericLotSerSegmentMultiple, Messages.NumericVal);
					}
				}
			}

			base.Persist();
		}
	}
}
