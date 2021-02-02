using System;
using System.Collections;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;

using PX.Objects.AR.BQL;
using PX.Objects.AR.Overrides.ScheduleMaint;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARScheduleMaint : ScheduleMaintBase<ARScheduleMaint, ARScheduleProcess>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        [AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search2<
			Schedule.scheduleID,
				LeftJoin<ARRegisterAccess, 
					On<ARRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<ARRegisterAccess.scheduled, Equal<True>,
					And<Not<Match<ARRegisterAccess, Current<AccessInfo.userName>>>>>>>,
			Where<
				Schedule.module, Equal<BatchModule.moduleAR>,
	            And<ARRegisterAccess.docType, IsNull>>>))]
        [PXDefault]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#endregion
		#endregion

		public PXSelect<Customer> Customers;
		public PXSelect<
			ARRegister, 
			Where<
				ARRegister.scheduleID, Equal<Current<Schedule.scheduleID>>, 
				And<ARRegister.scheduled, Equal<False>>>> 
			Document_History;

		public PXSelect<
			DocumentSelection, 
			Where<
				DocumentSelection.scheduleID, Equal<Current<Schedule.scheduleID>>,
				And<DocumentSelection.scheduled, Equal<True>>>>
			Document_Detail;

		public PXSelect<ARBalances> arbalances;

		public ARScheduleMaint()
		{
			ARSetup arSetup = ARSetup.Current;

			Document_History.Cache.AllowDelete = false;
			Document_History.Cache.AllowInsert = false;
			Document_History.Cache.AllowUpdate = false;

			Schedule_Header.Join<LeftJoin<
				ARRegisterAccess,
					On<ARRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<ARRegisterAccess.scheduled, Equal<True>,
					And<Not<Match<ARRegisterAccess, Current<AccessInfo.userName>>>>>>>>();

			Schedule_Header.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleAR>,
				And<ARRegisterAccess.docType, IsNull>>>();
		}

		public PXSetup<ARSetup> ARSetup;
		public PXSetup<GLSetup> GLSetup;

		protected override string Module => BatchModule.AR;

		internal override bool AnyScheduleDetails() => Document_Detail.Any();

		protected virtual void DocumentSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if(e.Row == null) return;

			PXUIFieldAttribute.SetEnabled<DocumentSelection.docType>(sender, e.Row, ((DocumentSelection)e.Row).Scheduled != true);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.refNbr>(sender, e.Row, ((DocumentSelection)e.Row).Scheduled != true);
		}

		protected override void SetControlsState(PXCache cache, Schedule schedule)
		{
			base.SetControlsState(cache, schedule);

			bool isNotProcessed = schedule.LastRunDate == null;

			PXUIFieldAttribute.SetEnabled<Schedule.nextRunDate>(cache, schedule, isNotProcessed == false);
			PXUIFieldAttribute.SetEnabled<Schedule.formScheduleType>(cache, schedule, isNotProcessed);
			PXUIFieldAttribute.SetEnabled<Schedule.startDate>(cache, schedule, isNotProcessed);
			
			PXUIFieldAttribute.SetEnabled<DocumentSelection.customerID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.status>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDate>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.finPeriodID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyOrigDocAmt>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDesc>(Document_Detail.Cache, null, false);
		}

		protected virtual void DocumentSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			ARRegister documentAsRegister = e.Row as ARRegister;

			if (documentAsRegister != null && documentAsRegister.Voided == false && documentAsRegister.Scheduled == false)
			{
				ARReleaseProcess.UpdateARBalances(this, documentAsRegister, -documentAsRegister.OrigDocAmt);

				documentAsRegister.Scheduled = true;
				documentAsRegister.ScheduleID = Schedule_Header.Current.ScheduleID;

				ARReleaseProcess.UpdateARBalances(this, documentAsRegister, documentAsRegister.OrigDocAmt);
			}

			DocumentSelection document = e.Row as DocumentSelection;

			if (document != null && 
				!string.IsNullOrWhiteSpace(document.DocType) && 
				!string.IsNullOrWhiteSpace(document.RefNbr) && 
				PXSelectorAttribute.Select<DocumentSelection.refNbr>(cache, document) == null)
			{
				cache.RaiseExceptionHandling<DocumentSelection.refNbr>(
					document, 
					document.RefNbr, 
					new PXSetPropertyException(AP.Messages.ReferenceNotValid));

				Document_Detail.Cache.Remove(document);
			}
		}

        protected virtual void DocumentSelection_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            DocumentSelection document = (DocumentSelection)e.Row;

			if (document != null && !String.IsNullOrWhiteSpace(document.DocType) && !String.IsNullOrWhiteSpace(document.RefNbr))
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

					cache.RaiseExceptionHandling<DocumentSelection.refNbr>(document, document.RefNbr, new PXSetPropertyException(AP.Messages.ReferenceNotValid));
                }
            }
        }

		protected virtual void DocumentSelection_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DocumentSelection document = e.Row as DocumentSelection;

			if (document == null) return;

			RemoveApplications(document);
		}

		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (DocumentSelection document in PXSelect<
				DocumentSelection, 
				Where<DocumentSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>
				.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				if ((bool)document.Scheduled)
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
			foreach (DocumentSelection document in Document_Detail.Cache.Updated)
			{
				ARReleaseProcess.UpdateARBalances(this, document, -document.OrigDocAmt);

				if (document.Voided == false)
				{
					document.Scheduled = true;
					document.ScheduleID = Schedule_Header.Current.ScheduleID;

					Document_Detail.Cache.Update(document);
				}

				ARReleaseProcess.UpdateARBalances(this, document, document.OrigDocAmt);
			}

			foreach (DocumentSelection document in Document_Detail.Cache.Deleted)
			{
				ARReleaseProcess.UpdateARBalances(this, document, -document.OrigDocAmt);
				
				PXDBDefaultAttribute.SetDefaultForUpdate<DocumentSelection.scheduleID>(Document_Detail.Cache, document, false);

				document.Voided = true;
				document.OpenDoc = false;
				document.Scheduled = false;
				document.ScheduleID = null;

				Document_Detail.Cache.SetStatus(document, PXEntryStatus.Updated);
				Document_Detail.Cache.Update(document);

				ARReleaseProcess.UpdateARBalances(this, document, document.OrigDocAmt);
			}

			base.Persist();
		}

		public PXAction<Schedule> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Document_Detail.Current == null) return adapter.Get();

			ARInvoiceEntry graph = CreateInstance<ARInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(Document_Detail.Current.RefNbr, Document_Detail.Current.DocType);

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

			ARInvoiceEntry graph = CreateInstance<ARInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(Document_History.Current.RefNbr, Document_History.Current.DocType);

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
			ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();

			ARInvoice documentAsInvoice = PXSelect<
				ARInvoice,
				Where<
					ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
				.Select(invoiceEntry, document.DocType, document.RefNbr);

			invoiceEntry.Document.Current = documentAsInvoice;

			//Removing aggregates on curyAdjdWOAmt and curyAdjdAmt fields to avoid excessive update of ARInvoice record
			invoiceEntry.Adjustments.Cache.Adjust<PXFormulaAttribute>().For<ARAdjust2.curyAdjdWOAmt>(a =>
			{
				if (a.Aggregate.IsIn(typeof(SumCalc<ARPayment.curyWOAmt>), typeof(SumCalc<AR.ARInvoice.curyBalanceWOTotal>)))
				{
					a.Aggregate = null;
				}
			});
			invoiceEntry.Adjustments.Cache.Adjust<PXFormulaAttribute>().For<ARAdjust2.curyAdjdAmt>(a =>
			{
				if (a.Aggregate == typeof(SumCalc<AR.ARInvoice.curyPaymentTotal>))
				{
					a.Aggregate = null;
				}
			});

			invoiceEntry.Adjustments
				.Select()
				.RowCast<ARAdjust2>()
				.Where(application => application.Released != true)
				.ForEach(application => invoiceEntry.Adjustments.Delete(application));

			invoiceEntry.Adjustments_1
				.Select()
				.RowCast<ARAdjust>()
				.Where(application => application.Released != true)
				.ForEach(application => invoiceEntry.Adjustments_1.Delete(application));

			invoiceEntry.Save.Press();
		}

		#endregion
	}
}

namespace PX.Objects.AR.Overrides.ScheduleMaint
{
	[PXPrimaryGraph(null)]
    [PXCacheName(Messages.DocumentSelection)]
    [Serializable]
	public partial class DocumentSelection : ARRegister
	{
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<ARRegister.scheduleID>>>>), LeaveChildren = true)]
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
		[PXDefault(ARDocType.Invoice)]
		[PXStringList(
			new [] { ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.CreditMemo }, 
			new [] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo })]
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
		[PXSelector(typeof(Search2<
			ARRegister.refNbr, 
				LeftJoin<GLVoucher, 
					On<GLVoucher.refNoteID, Equal<ARRegister.noteID>, 
					And<FeatureInstalled<FeaturesSet.gLWorkBooks>>>>, 
			Where<
				ARRegister.docType, Equal<Optional<ARRegister.docType>>, 
				And<GLVoucher.refNbr, IsNull,
				And<IsSchedulable<ARRegister>>>>>),
			typeof(ARRegister.finPeriodID),
			typeof(ARRegister.refNbr),
			typeof(ARRegister.customerID),
			typeof(ARRegister.customerLocationID),
			typeof(ARRegister.status),
			typeof(ARRegister.curyID),
			typeof(ARRegister.curyOrigDocAmt))]
		public override String RefNbr
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
			typeof(ARRegister.docDate),
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.tranPeriodID),
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
		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
	}
}
