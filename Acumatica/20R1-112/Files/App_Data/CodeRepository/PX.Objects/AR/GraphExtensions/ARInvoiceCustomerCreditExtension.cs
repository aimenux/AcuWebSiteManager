using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Extensions.CustomerCreditHold;

namespace PX.Objects.AR.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the AR invoice credit helper functionality.</summary>	
	public class ARInvoiceCustomerCreditExtension : CustomerCreditExtension<ARInvoiceEntry>
	{
		#region Mapping	

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(ARInvoice))
			{
				CustomerID = typeof(ARInvoice.customerID),
				Hold = typeof(ARInvoice.creditHold),
				Released = typeof(ARInvoice.released),
				Status = typeof(ARInvoice.status)
			};
		}

		#endregion

		#region State
		private Dictionary<Type, List<PXRowUpdated>> _preupdatedevents = new Dictionary<Type, List<PXRowUpdated>>();
		#endregion

		#region Implementation
		protected override bool? GetHoldValue(PXCache sender, object Row)
		{
			ARInvoice row = Document.Cache.GetMain((Document)Row) as ARInvoice;

			return (row?.Hold == true || row?.CreditHold == true);
		}

		protected override bool? GetCreditCheckError(PXCache sender, object Row)
		{
			if (sender.Graph.GetType() == typeof(SO.SOInvoiceEntry) || sender.Graph.GetType().BaseType == typeof(SO.SOInvoiceEntry))
			{
				PX.Objects.SO.SOSetup soSetup = PXSetup<PX.Objects.SO.SOSetup>.Select(sender.Graph);
				return soSetup?.CreditCheckError;
			}
			else
			{
				ARSetup arSetup = PXSetup<ARSetup>.Select(sender.Graph);
				return arSetup?.CreditCheckError;
			}
		}

		protected override void _(Events.RowUpdated<Document> e)
		{
			ARInvoice row = Document.Cache.GetMain((Document)e.Row) as ARInvoice;
			ARInvoice oldRow = Document.Cache.GetMain((Document)e.OldRow) as ARInvoice;

			if (row == null) return;

			if (e.Row != null && e.OldRow != null)
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			if (_InternalCall)
			{
				return;
			}
			List<PXRowUpdated> evLst;
			if (_preupdatedevents.TryGetValue(typeof(ARInvoice), out evLst))
			{
				foreach (var ev in evLst)
				{
					PXRowUpdatedEventArgs arInvoiceEventErgs = new PXRowUpdatedEventArgs(row, oldRow, e.ExternalCall);
					ev.Invoke(e.Cache, arInvoiceEventErgs);
				}
			}

			if (oldRow != null && ((row.CreditHold != oldRow.CreditHold && row.CreditHold == false && row.Hold == false)))
			{
				object DocumentBal = row.OrigDocAmt;

				e.Cache.SetValue<ARInvoice.approvedCredit>(row, true);
				e.Cache.SetValue<ARInvoice.approvedCreditAmt>(row, DocumentBal);
			}

			base._(e);
		}

		//We cannot create a specialized CustomerCreditExtension for SOInvoiceEntry as it will cause execution of both ARInvoiceEntry and SOInvoiceEntry extensions for SOInvoiceEntry graph. 
		protected virtual void _(Events.RowUpdated<SO.SOInvoice> e)
		{
			if (e.Row == null || e.OldRow == null) return;

			if (!e.Cache.ObjectsEqual<SO.SOInvoice.isCCCaptured>(e.Row, e.OldRow) && (bool?)e.Cache.GetValue<SO.SOInvoice.isCCCaptured>(e.Row) == true)
			{
				ARInvoice ardoc = (ARInvoice)e.Cache.Graph.Caches[typeof(ARInvoice)].Current;

				if (ardoc != null)
				{
					object DocumentBal = e.Cache.Graph.Caches[typeof(ARInvoice)].GetValue<ARInvoice.origDocAmt>(ardoc);

					e.Cache.SetValue<ARInvoice.approvedCredit>(ardoc, true);
					e.Cache.SetValue<ARInvoice.approvedCreditAmt>(ardoc, DocumentBal);
				}
			}
		}

		public virtual void AppendPreUpdatedEvent(Type entity, PXRowUpdated del)
		{
			List<PXRowUpdated> evLst;
			if (!_preupdatedevents.TryGetValue(entity, out evLst))
				evLst = new List<PXRowUpdated>();
			evLst.Add(del);
			_preupdatedevents[entity] = evLst;
		}
		public virtual void RemovePreUpdatedEvent(Type entity, PXRowUpdated del)
		{
			List<PXRowUpdated> evLst;
			if (!_preupdatedevents.TryGetValue(entity, out evLst))
				return;
			evLst.Remove(del);
		}

		protected override decimal? GetDocumentBalance(PXCache cache, object Row)
		{
			ARInvoice row = Document.Cache.GetMain((Document)Row) as ARInvoice;

			decimal? DocumentBal = 0m;
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				DocumentBal = accumbal.UnreleasedBal;
			}

			PXCache sender = cache.Graph.Caches[typeof(ARInvoice)];

			if (DocumentBal > 0m && row.ApprovedCredit == true)
			{
				if (row.ApprovedCreditAmt >= row.OrigDocAmt)
				{
					DocumentBal = 0m;
				}
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			ARInvoice row = Document.Cache.GetMain((Document)Row) as ARInvoice;

			if (OnAdminHold)
			{
				sender.RaiseExceptionHandling<ARInvoice.hold>(row, true, new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				object oldRow = sender.CreateCopy(row);
				sender.SetValueExt<ARInvoice.status>(row, null);
				sender.SetValueExt<ARInvoice.creditHold>(row, false);
				sender.SetValueExt<ARInvoice.hold>(row, true);
				sender.RaiseRowUpdated(row, oldRow);
			}
			else
			{
				base.PlaceOnHold(sender, row, false);
			}

			sender.SetValue<ARInvoice.approvedCredit>(row, false);
			sender.SetValue<ARInvoice.approvedCreditAmt>(row, 0m);
		}

		protected virtual void _(Events.RowInserted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected virtual void _(Events.RowDeleted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		public override void UpdateARBalances(PXCache cache, object newRow, object oldRow)
		{
			if (oldRow != null)
			{
				ARInvoice oldARRow = Document.Cache.GetMain((Document)oldRow) as ARInvoice;
				ARReleaseProcess.UpdateARBalances(cache.Graph, (ARInvoice)oldARRow, -((ARInvoice)oldARRow).OrigDocAmt);
			}

			if (newRow != null)
			{
				ARInvoice newARRow = Document.Cache.GetMain((Document)newRow) as ARInvoice;
				ARReleaseProcess.UpdateARBalances(cache.Graph, (ARInvoice)newARRow, ((ARInvoice)newARRow).OrigDocAmt);
			}
		}

		protected virtual void _(Events.FieldUpdated<ARInvoice, ARInvoice.hold> eventArgs)
		{
			if (eventArgs.Row?.Hold == true && eventArgs.Row.Released != true)
				eventArgs.Cache.SetValue<ARInvoice.creditHold>(eventArgs.Row, false);
		}
		#endregion
	}
}