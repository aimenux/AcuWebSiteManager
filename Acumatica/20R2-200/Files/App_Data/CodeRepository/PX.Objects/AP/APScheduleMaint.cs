using System;
using System.Collections;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;

using PX.Objects.AP.BQL;
using PX.Objects.AP.Overrides.ScheduleMaint;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APScheduleMaint : ScheduleMaintBase<APScheduleMaint, APScheduleProcess>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        [AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search2<
			Schedule.scheduleID,
				LeftJoin<APRegisterAccess, 
					On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<APRegisterAccess.scheduled, Equal<True>,
					And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>,
            Where<
				Schedule.module, Equal<BatchModule.moduleAP>,
				And<APRegisterAccess.docType, IsNull>>>))]
        [PXDefault]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#endregion
		#endregion

		#region Views

		public PXSelect<Vendor> Vendors;
		public PXSelect<
			APRegister, 
			Where<
				APRegister.scheduleID, Equal<Current<Schedule.scheduleID>>, 
				And<APRegister.scheduled, Equal<False>>>> 
			Document_History;

		public PXSelect<
			DocumentSelection, 
			Where<
				DocumentSelection.scheduleID, Equal<Current<Schedule.scheduleID>>, 
				And<DocumentSelection.scheduled, Equal<True>>>> 
			Document_Detail;

		#endregion

		public APScheduleMaint()
		{
			APSetup apSetup = APSetup.Current;
			GLSetup glSetup = GLSetup.Current;

			Document_History.Cache.AllowDelete = false;
			Document_History.Cache.AllowInsert = false;
			Document_History.Cache.AllowUpdate = false;

			Schedule_Header.Join<LeftJoin<
				APRegisterAccess, 
					On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<APRegisterAccess.scheduled, Equal<True>,
					And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>>();

			Schedule_Header.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleAP>,
				And<APRegisterAccess.docType, IsNull>>>();
		}

		public PXSetup<APSetup> APSetup;
		public PXSetup<GLSetup> GLSetup;

		protected override string Module => BatchModule.AP;

		internal override bool AnyScheduleDetails() => Document_Detail.Any();

		protected virtual void DocumentSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DocumentSelection document = e.Row as DocumentSelection;

			if (document == null) return;

			PXUIFieldAttribute.SetEnabled<DocumentSelection.docType>(sender, document, document.Scheduled != true);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.refNbr>(sender, document, document.Scheduled != true);
		}

		protected override void SetControlsState(PXCache cache, Schedule schedule)
		{
			base.SetControlsState(cache, schedule);

			bool isNotProcessed = schedule.LastRunDate == null;

			PXUIFieldAttribute.SetEnabled<Schedule.nextRunDate>(cache, schedule, isNotProcessed == false);
			PXUIFieldAttribute.SetEnabled<Schedule.formScheduleType>(cache, schedule, isNotProcessed);
			PXUIFieldAttribute.SetEnabled<Schedule.startDate>(cache, schedule, isNotProcessed);

			PXUIFieldAttribute.SetEnabled<DocumentSelection.vendorID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.status>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDate>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.finPeriodID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyOrigDocAmt>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDesc>(Document_Detail.Cache, null, false);
		}

		protected virtual void DocumentSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			APRegister documentAsRegister = e.Row as APRegister;

			if (documentAsRegister != null && documentAsRegister.Voided == false)
			{
				documentAsRegister.ScheduleID = Schedule_Header.Current.ScheduleID;
				documentAsRegister.Scheduled = true;
			}

			DocumentSelection document = e.Row as DocumentSelection;

			if (document != null && 
				!string.IsNullOrWhiteSpace(document.DocType) && 
				!string.IsNullOrWhiteSpace(document.RefNbr) && 
				PXSelectorAttribute.Select<DocumentSelection.refNbr>(cache, document) == null)
			{
				cache.RaiseExceptionHandling<DocumentSelection.refNbr>(document, document.RefNbr, new PXSetPropertyException(Messages.ReferenceNotValid));

				Document_Detail.Cache.Remove(document);
			}
		}

        protected virtual void DocumentSelection_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            DocumentSelection document = e.Row as DocumentSelection;

			if (document != null && 
				!string.IsNullOrWhiteSpace(document.DocType) && 
				!string.IsNullOrWhiteSpace(document.RefNbr))
            {
                document = PXSelectReadonly<
					DocumentSelection, 
					Where<
						DocumentSelection.docType, Equal<Required<DocumentSelection.docType>>,
						And<DocumentSelection.refNbr, Equal<Required<DocumentSelection.refNbr>>>>>
					.Select(this, document.DocType, document.RefNbr);

				if (document != null)
                {
                    Document_Detail.Delete(document);
                    Document_Detail.Update(document);
                }
                else
                {
                    document = (DocumentSelection)e.Row;

                    Document_Detail.Delete(document);

					cache.RaiseExceptionHandling<DocumentSelection.refNbr>(document, document.RefNbr, new PXSetPropertyException(Messages.ReferenceNotValid));
                }
            }
        }

		protected virtual void DocumentSelection_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			DocumentSelection document = e.Row as DocumentSelection;

			if (document == null) return;

			RemoveApplications(document);
		}

		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (DocumentSelection document in PXSelect<
				DocumentSelection, 
				Where<
					DocumentSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>
				.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				if (document.Scheduled == true)
				{
					document.Voided = true;
					document.Scheduled = false;
				}

				document.ScheduleID = null;

				PXDBDefaultAttribute.SetDefaultForUpdate<DocumentSelection.scheduleID>(Document_Detail.Cache, document, false);

				Document_Detail.Cache.Update(document);
			}
		}
		
		public override void Persist()
		{
			foreach (DocumentSelection document in Document_Detail.Cache.Inserted)
			{
				// This case happens in Import Scenarios when rows have state "Inserted"
				// Move rows to collection Updated to set ScheduleID in next foreach 
				Document_Detail.Cache.SetStatus(document, PXEntryStatus.Updated); 
			}

			foreach (DocumentSelection document in Document_Detail.Cache.Updated)
			{
				if (document.Voided == false)
				{
					document.Scheduled = true;
					document.ScheduleID = Schedule_Header.Current.ScheduleID;

					Document_Detail.Cache.Update(document);
				}
			}

			foreach (DocumentSelection document in Document_Detail.Cache.Deleted)
			{
				PXDBDefaultAttribute.SetDefaultForUpdate<DocumentSelection.scheduleID>(Document_Detail.Cache, document, false);

				document.Voided = true;
				document.OpenDoc = false;
				document.Scheduled = false;
				document.ScheduleID = null;

				Document_Detail.Cache.SetStatus(document, PXEntryStatus.Updated);
				Document_Detail.Cache.Update(document);
			}

			base.Persist();
		}

		public PXAction<Schedule> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Document_Detail.Current == null) return adapter.Get();

			APInvoiceEntry graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<APInvoice.refNbr>(Document_Detail.Current.RefNbr, Document_Detail.Current.DocType);

			throw new PXRedirectRequiredException(graph, true, "Document")
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		public PXAction<Schedule> viewGenDocument;
        [PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ViewGenDocument(PXAdapter adapter)
		{
			if (Document_History.Current == null) return adapter.Get();

			APInvoiceEntry graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<APInvoice.refNbr>(Document_History.Current.RefNbr, Document_History.Current.DocType);

			throw new PXRedirectRequiredException(graph, true, "Generated Document")
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		#region Helper Methods

		/// <summary>
		/// Removes all application records associated with the specified
		/// document. This is required in order to prevent stuck application
		/// records after a document becomes scheduled.
		/// </summary>
		private void RemoveApplications(DocumentSelection document)
		{
			APInvoiceEntry invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();

			APInvoice documentAsInvoice = PXSelect<
				APInvoice,
				Where<
					APInvoice.docType, Equal<Required<APInvoice.docType>>,
					And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
				.Select(invoiceEntry, document.DocType, document.RefNbr);

			invoiceEntry.Document.Current = documentAsInvoice;

			invoiceEntry.Adjustments
				.Select()
				.RowCast<APInvoiceEntry.APAdjust>()
				.Where(application => application.Released != true)
				.ForEach(application => invoiceEntry.Adjustments.Delete(application));

			invoiceEntry.Save.Press();
		}

		#endregion
	}
}

namespace PX.Objects.AP.Overrides.ScheduleMaint
{
	[PXPrimaryGraph(null)]
    [Serializable]
	public partial class DocumentSelection : APRegister
	{
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<APRegister.scheduleID>>>>))]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(APDocType.Invoice)]
		[PXStringList(
			new [] { APDocType.Invoice, APDocType.Prepayment, APDocType.DebitAdj, APDocType.CreditAdj }, 
			new [] { Messages.Invoice, Messages.Prepayment, Messages.DebitAdj, Messages.CreditAdj })]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public override string DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<
			APRegister.refNbr,
			Where<
				APRegister.docType, Equal<Optional<APRegister.docType>>, 
				And<IsSchedulable<APRegister>>>>),
			typeof(APRegister.finPeriodID),
			typeof(APRegister.refNbr),
			typeof(APRegister.vendorID),
			typeof(APRegister.vendorLocationID),
			typeof(APRegister.status),
			typeof(APRegister.curyID),
			typeof(APRegister.curyOrigDocAmt))]
		public override string RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion

		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(
			typeof(DocumentSelection.docDate),
			branchSourceType: typeof(DocumentSelection.branchID),
			masterFinPeriodIDType: typeof(DocumentSelection.tranPeriodID),
			IsHeader = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region Rejected
		public new abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		#endregion

		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
	}
}
