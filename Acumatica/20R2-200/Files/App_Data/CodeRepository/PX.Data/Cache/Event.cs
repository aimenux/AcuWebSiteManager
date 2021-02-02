// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.ComponentModel;


namespace PX.Data
{
    /// <summary>
    ///   <para>The delegate for the <tt>RowUpdating</tt> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>Required. The cache object that generated the event.</para>
    /// </param>
    /// <param name="e">
    ///   <para>Required. The instance of the <see cref="PXRowUpdatingEventArgs">PXRowUpdatingEventArgs</see> type
    ///   that holds data for the <tt>RowUpdating</tt> event.</para>
    /// </param>
    /// <remarks>
    ///   <para>The <tt>RowUpdating</tt> event is generated before the data record is actually updated
    ///   in the <tt>PXCache</tt> object during an update initiated in either of the following cases:</para>
    ///   <list type="bullet">
    ///     <item>In the UI or via Web Service API.</item>
    ///     <item>When the following <tt>PXCache</tt> class methods were invoked:<ul><li><tt>Update(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>RowUpdating</tt> event handler is used to evaluate a data record that is being updated
    ///   and to cancel the update operation if the data record does not
    /// fit the business logic requirements.</para>
    ///   <para>The following execution order is used for the <tt>RowUpdating</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowUpdating(
    ///     PXCache sender, 
    ///     PXRowUpdatingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowUpdating&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to evaluate the data record that is being updated, cancel the update operation, and show a message box." lang="CS">
    /// public class APPaymentEntry : APDataEntryGraph&lt;APPaymentEntry, APPayment&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void APAdjust_RowUpdating(PXCache sender, 
    ///                                                 PXRowUpdatingEventArgs e)
    ///     {
    ///         APAdjust adj = (APAdjust)e.Row;
    ///         if (_IsVoidCheckInProgress == false &amp;&amp; adj.Voided == true)
    ///         {
    ///             throw new PXException(ErrorMessages.CantUpdateRecord);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to evaluate the data record that is being updated, cancel the update operation, and show a message box." lang="CS">
    /// public class APPaymentEntry : APDataEntryGraph&lt;APPaymentEntry, APPayment&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowUpdating&lt;APAdjust&gt; e)
    ///     {
    ///         APAdjust adj = e.Row;
    ///         if (_IsVoidCheckInProgress == false &amp;&amp; adj.Voided == true)
    ///         {
    ///             throw new PXException(ErrorMessages.CantUpdateRecord);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to evaluate the data record that is being updated, cancel the update operation, and show the warning or error indication near the input control for one field or multiple fields." lang="CS">
    /// protected virtual void INLotSerClass_RowUpdating(PXCache sender,
    ///                                                  PXRowUpdatingEventArgs e)
    /// {
    ///     INLotSerClass row = (INLotSerClass) e.NewRow;
    ///     if (row.LotSerTrackExpiration != true &amp;&amp;
    ///         row.LotSerIssueMethod == INLotSerIssueMethod.Expiration)
    ///     {
    ///         sender.RaiseExceptionHandling&lt;INLotSerClass.lotSerIssueMethod&gt;(
    ///             row, null,
    ///             new PXSetPropertyException(
    ///                 Messages.LotSerTrackExpirationInvalid,
    ///                 typeof(INLotSerClass.lotSerIssueMethod).Name));
    ///         e.Cancel = true;
    ///     }
    /// }</code>
    ///   <code title="Example3b" description="The following code uses the generic event handler approach to evaluate the data record that is being updated, cancel the update operation, and show the warning or error indication near the input control for one field or multiple fields." lang="CS">
    /// protected virtual void _(Events.RowUpdating&lt;INLotSerClass&gt; e)
    /// {
    ///     INLotSerClass row = e.NewRow;
    ///     if (row.LotSerTrackExpiration != true &amp;&amp;
    ///         row.LotSerIssueMethod == INLotSerIssueMethod.Expiration)
    ///     {
    ///         e.Cache.RaiseExceptionHandling&lt;INLotSerClass.lotSerIssueMethod&gt;(
    ///             row, null,
    ///             new PXSetPropertyException(
    ///                 Messages.LotSerTrackExpirationInvalid,
    ///                 typeof(INLotSerClass.lotSerIssueMethod).Name));
    ///         e.Cancel = true;
    ///     }
    /// }</code>
    /// </example>
    public delegate void PXRowUpdating(PXCache sender, PXRowUpdatingEventArgs e);

    /// <summary>Provides data for the <tt>RowUpdating</tt> event.</summary>
    /// <seealso cref="PXRowUpdating"/>
    public sealed class PXRowUpdatingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private readonly object _NewRow;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="newrow"></param>
        /// <param name="externalCall"></param>
        public PXRowUpdatingEventArgs(object row, object newrow, bool externalCall)
        {
            _Row = row;
            _NewRow = newrow;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the original DAC object that is being updated.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the updated copy of the DAC object that is going to be
        /// merged with the original one.</summary>
        public object NewRow
        {
            get
            {
                return _NewRow;
            }
        }

        /// <summary>
        /// Returns <tt>true</tt> if the update of the DAC object has been initiated from the UI
        /// or through the Web Service API; otherwise, it returns <tt>false</tt>.
        /// </summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowUpdated</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowUpdatedEventArgs">PXRowUpdatedEventArgs</see> type that holds data for the <tt>RowUpdated</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowUpdated</tt> event is generated after the data record has been successfully updated
    ///   in the <tt>PXCache</tt> object in one of the following cases:</para>
    ///   <list type="bullet">
    ///     <item>The update is initiated in the UI or through the Web Service API.</item>
    ///     <item>One of the following methods of the <tt>PXCache</tt> class is invoked:
    ///         <ul><li><tt>Update(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <div class="WidgetContent" contenteditable="false" style="BORDER-TOP: white 2px solid; BORDER-BOTTOM: white 1px solid; MARGIN-TOP: 2px" data-widget-type="Note Box" data-source-widget-type="Note Box" data-widget-layout="block">
    ///     <div class="i-box i-box-note">
    ///         The updating of a data record is executed only when there is a data record
    ///         with the same values of the data access class (DAC) key fields, either in the
    ///         <tt>PXCache</tt> object or in the database. Otherwise, the process of inserting the data record is started.
    ///     </div>
    ///   </div>
    ///   <para>The <tt>RowUpdated</tt> event handler is used to implement the business logic of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Update of the master data record in a many-to-one relationship</item>
    ///     <item>Insertion or update of the detail data records in a one-to-many relationship</item>
    ///     <item>Update of the related data record in a one-to-one relationship</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowUpdated</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowUpdated(PXCache sender, 
    ///                                           PXRowUpdatedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowUpdated&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code updates the detail data records in a one-to-many relationship by using the classic event handler approach." lang="CS">
    /// public class DraftScheduleMaint : PXGraph&lt;DraftScheduleMaint, DRSchedule&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void DRSchedule_RowUpdated(PXCache sender, 
    ///                                                  PXRowUpdatedEventArgs e)
    ///     {
    ///         DRSchedule row = e.Row as DRSchedule;
    ///         if (!sender.ObjectsEqual&lt;DRSchedule.documentType, DRSchedule.refNbr, 
    ///                                  DRSchedule.lineNbr, DRSchedule.bAccountID, 
    ///                                  DRSchedule.finPeriodID, 
    ///                                  DRSchedule.docDate&gt;(e.Row, e.OldRow))
    ///         {
    ///             foreach (DRScheduleDetail detail in Components.Select())
    ///             {
    ///                 detail.Module = row.Module;
    ///                 detail.DocumentType = row.DocumentType;
    ///                 detail.DocType = row.DocType;
    ///                 detail.RefNbr = row.RefNbr;
    ///                 detail.LineNbr = row.LineNbr;
    ///                 detail.BAccountID = row.BAccountID;
    ///                 detail.FinPeriodID = row.FinPeriodID;
    ///                 detail.DocDate = row.DocDate;
    ///                 Components.Update(detail);
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code updates the detail data records in a one-to-many relationship by using the generic event handler approach." lang="CS">
    /// public class DraftScheduleMaint : PXGraph&lt;DraftScheduleMaint, DRSchedule&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowUpdated&lt;DRSchedule&gt; e)
    ///     {
    ///         DRSchedule row = e.Row;
    ///         if (!e.Cache.ObjectsEqual&lt;DRSchedule.documentType, DRSchedule.refNbr, 
    ///                                  DRSchedule.lineNbr, DRSchedule.bAccountID, 
    ///                                  DRSchedule.finPeriodID, 
    ///                                  DRSchedule.docDate&gt;(e.Row, e.OldRow))
    ///         {
    ///             foreach (DRScheduleDetail detail in Components.Select())
    ///             {
    ///                 detail.Module = row.Module;
    ///                 detail.DocumentType = row.DocumentType;
    ///                 detail.DocType = row.DocType;
    ///                 detail.RefNbr = row.RefNbr;
    ///                 detail.LineNbr = row.LineNbr;
    ///                 detail.BAccountID = row.BAccountID;
    ///                 detail.FinPeriodID = row.FinPeriodID;
    ///                 detail.DocDate = row.DocDate;
    ///                 Components.Update(detail);
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code updates the master data record in a many-to-one relationship by using the classic event handler approach." lang="CS">
    /// public class ARInvoiceEntry : ARDataEntryGraph&lt;ARInvoiceEntry, ARInvoice&gt;, 
    ///                               PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void ARTran_RowUpdated(PXCache sender, 
    ///                                              PXRowUpdatedEventArgs e)
    ///     {
    ///         ARTran row = (ARTran)e.Row;
    ///         ARTran oldRow = (ARTran)e.OldRow;
    ///         if (Document.Current != null &amp;&amp; 
    ///             IsExternalTax == true &amp;&amp;
    ///             !sender.ObjectsEqual&lt;ARTran.accountID, ARTran.inventoryID, 
    ///                                  ARTran.tranDesc,
    ///                                  ARTran.tranAmt, ARTran.tranDate, 
    ///                                  ARTran.taxCategoryID&gt;(e.Row, e.OldRow))
    ///         {
    ///             ARInvoice copy = Document.Current;
    ///             copy.IsTaxValid = false;
    ///             Document.Update(copy);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code updates the master data record in a many-to-one relationship by using the generic event handler approach." lang="CS">
    /// public class ARInvoiceEntry : ARDataEntryGraph&lt;ARInvoiceEntry, ARInvoice&gt;, 
    ///                               PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowUpdated&lt;ARTran&gt; e)
    ///     {
    ///         ARTran row = e.Row;
    ///         ARTran oldRow = e.OldRow;
    ///         if (Document.Current != null &amp;&amp; 
    ///             IsExternalTax == true &amp;&amp;
    ///             !e.Cache.ObjectsEqual&lt;ARTran.accountID, ARTran.inventoryID, 
    ///                                  ARTran.tranDesc,
    ///                                  ARTran.tranAmt, ARTran.tranDate, 
    ///                                  ARTran.taxCategoryID&gt;(e.Row, e.OldRow))
    ///         {
    ///             ARInvoice copy = Document.Current;
    ///             copy.IsTaxValid = false;
    ///             Document.Update(copy);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowUpdated(PXCache sender, PXRowUpdatedEventArgs e);

    /// <summary>Provides data for the <tt>RowUpdated</tt> event.</summary>
    /// <seealso cref="PXRowUpdated"/>
    public sealed class PXRowUpdatedEventArgs : EventArgs
    {
        private readonly object _Row;
        private readonly object _OldRow;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="oldRow"></param>
        /// <param name="externalCall"></param>
        public PXRowUpdatedEventArgs(object row, object oldRow, bool externalCall)
        {
            _Row = row;
            _OldRow = oldRow;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the DAC object that has been updated.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the copy of the original DAC object before the update operation.</summary>
        public object OldRow
        {
            get
            {
                return _OldRow;
            }
        }

        /// <summary>
        /// Returns <tt>true</tt> if the DAC object has been updated from the UI or through the Web Service API;
        /// otherwise, it returns <tt>false</tt>.
        /// </summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowInserting</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowInsertingEventArgs">PXRowInsertingEventArgs</see> type
    /// that holds data for the <tt>RowInserting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowInserting</tt> event is trigged before the new data record is inserted into the <tt>PXCache</tt> object
    ///   as a result of one of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Insertion initiated in the UI or through the Web Service API</item>
    ///     <item>Invocation of either of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>RowInserting</tt> event handler is used to perform the following actions:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>Evaluation of the data record that is being inserted</item>
    ///     <item>Cancellation of the insertion by throwing an exception</item>
    ///     <item>Assignment of the default values to the fields of the data record that is being inserted</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowInserting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowInserting(PXCache sender, 
    ///                                             PXRowInsertingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowInserting&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to evaluate the data record that is being inserted and cancel the insertion." lang="CS">
    /// public class CashAccountMaint : PXGraph&lt;CashAccountMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void PaymentMethodAccount_RowInserting(
    ///         PXCache sender, 
    ///         PXRowInsertingEventArgs e)
    ///     {
    ///         PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
    ///         if (row.PaymentMethodID != null)
    ///             foreach (PaymentMethodAccount it in Details.Select())
    ///                 if (!object.ReferenceEquals(row, it) &amp;&amp; 
    ///                     it.PaymentMethodID == row.PaymentMethodID)
    ///                     throw new PXException(
    ///                         Messages.DuplicatedPaymentMethodForCashAccount, 
    ///                         row.PaymentMethodID);
    ///         if (row.APIsDefault == true &amp;&amp; 
    ///             String.IsNullOrEmpty(row.PaymentMethodID))
    ///             throw new PXException(ErrorMessages.FieldIsEmpty, 
    ///                                   typeof(PaymentMethodAccount.
    ///                                       paymentMethodID).Name);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to evaluate the data record that is being inserted and cancel the insertion." lang="CS">
    /// public class CashAccountMaint : PXGraph&lt;CashAccountMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowInserting&lt;PaymentMethodAccount&gt; e)
    ///     {
    ///         PaymentMethodAccount row = e.Row;
    ///         if (row.PaymentMethodID != null)
    ///             foreach (PaymentMethodAccount it in Details.Select())
    ///                 if (!object.ReferenceEquals(row, it) &amp;&amp; 
    ///                     it.PaymentMethodID == row.PaymentMethodID)
    ///                     throw new PXException(
    ///                         Messages.DuplicatedPaymentMethodForCashAccount, 
    ///                         row.PaymentMethodID);
    ///         if (row.APIsDefault == true &amp;&amp; 
    ///             String.IsNullOrEmpty(row.PaymentMethodID))
    ///             throw new PXException(ErrorMessages.FieldIsEmpty, 
    ///                                   typeof(PaymentMethodAccount.
    ///                                       paymentMethodID).Name);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to assign the default field values to the data record that is being inserted." lang="CS">
    /// public class MyCaseDetailsMaint : PXGraph&lt;MyCaseDetailsMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void EPActivity_RowInserting(PXCache sender, 
    ///                                                    PXRowInsertingEventArgs e)
    ///     {
    ///         EPActivity row = e.Row as EPActivity;
    ///         if (Case.Current != null)
    ///         {
    ///             row.StartDate = PXTimeZoneInfo.Now;
    ///             row.RefNoteID = Case.Current.NoteID;
    ///             row.ClassID = CRActivityClass.Activity;
    ///             row.IsExternal = true;
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code uses the generic event handler approach to assign the default field values to the data record that is being inserted." lang="CS">
    /// public class MyCaseDetailsMaint : PXGraph&lt;MyCaseDetailsMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowInserting&lt;EPActivity&gt; e)
    ///     {
    ///         EPActivity row = e.Row;
    ///         if (Case.Current != null)
    ///         {
    ///             row.StartDate = PXTimeZoneInfo.Now;
    ///             row.RefNoteID = Case.Current.NoteID;
    ///             row.ClassID = CRActivityClass.Activity;
    ///             row.IsExternal = true;
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowInserting(PXCache sender, PXRowInsertingEventArgs e);

    /// <summary>Provides data for the <tt>RowInserting</tt> event.</summary>
    /// <seealso cref="PXRowInserting"/>
    public sealed class PXRowInsertingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="externalCall"></param>
        public PXRowInsertingEventArgs(object row, bool externalCall)
        {
            _Row = row;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the DAC object that is being inserted.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }

        /// <summary>
        /// Returns <tt>true</tt> if a DAC object was inserted from the UI or by using the Web API Service; otherwise, it returns <tt>false</tt>.
        /// </summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    /// The delegate for the <tt>RowInserted</tt> event.
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowInsertedEventArgs">PXRowInsertedEventArgs</see> type that holds data for the <tt>RowInserted</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowInserted</tt> event is generated after a new data record has been successfully inserted
    ///   into the <tt>PXCache</tt> as a result of one of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Insertion initiated in the UI or through the Web Service API</item>
    ///     <item>Invocation of any of the following <tt>PXCache</tt> class methods:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>RowInserted</tt> event handler is used to implement the business logic for
    ///   the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Insertion of the detail data records in a one-to-many relationship</item>
    ///     <item>Update of the master data record in a many-to-one relationship</item>
    ///     <item>Insertion or update of the related data record in a one-to-one relationship</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowInserted</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowInserted(PXCache sender, 
    ///                                            PXRowInsertedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowInserted&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code inserts the detail data records in a one-to-many relationship by using the classic event handler approach." lang="CS">
    /// public class VendorClassMaint : PXGraph&lt;VendorClassMaint&gt;
    /// {
    ///     ...
    ///     
    ///     public virtual void VendorClass_RowInserted(PXCache sender, 
    ///                                                 PXRowInsertedEventArgs e)
    ///     {
    ///         VendorClass row = (VendorClass)e.Row;
    ///         if (row == null || row.VendorClassID == null) return;
    ///         
    ///         foreach (APNotification n in PXSelect&lt;
    ///             APNotification,
    ///             Where&lt;APNotification.sourceCD, 
    ///                   Equal&lt;APNotificationSource.vendor&gt;&gt;&gt;.
    ///             Select(this))
    ///         {
    ///             NotificationSource source = new NotificationSource();
    ///             source.SetupID = n.SetupID;
    ///             NotificationSources.Insert(source);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code inserts the detail data records in a one-to-many relationship by using the generic event handler approach." lang="CS">
    /// public class VendorClassMaint : PXGraph&lt;VendorClassMaint&gt;
    /// {
    ///     ...
    ///     
    ///     public virtual void _(Events.RowInserted&lt;VendorClass&gt; e)
    ///     {
    ///         VendorClass row = e.Row;
    ///         if (row == null || row.VendorClassID == null) return;
    ///         
    ///         foreach (APNotification n in PXSelect&lt;
    ///             APNotification,
    ///             Where&lt;APNotification.sourceCD, 
    ///                   Equal&lt;APNotificationSource.vendor&gt;&gt;&gt;.
    ///             Select(this))
    ///         {
    ///             NotificationSource source = new NotificationSource();
    ///             source.SetupID = n.SetupID;
    ///             NotificationSources.Insert(source);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code updates the master data record in a many-to-one relationship by using the classic event handler approach." lang="CS">
    /// public class InventoryItemMaint : PXGraph&lt;InventoryItemMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void POVendorInventory_RowInserted(
    ///         PXCache sender, 
    ///         PXRowInsertedEventArgs e)
    ///     {
    ///         POVendorInventory current = e.Row as POVendorInventory;
    ///         if (current.IsDefault == true &amp;&amp; current.VendorID != null &amp;&amp; 
    ///             current.VendorLocationID != null &amp;&amp; current.SubItemID != null &amp;&amp;
    ///             this.Item.Current.PreferredVendorLocationID != 
    ///             current.VendorLocationID)
    ///         {
    ///             InventoryItem upd = Item.Current;
    ///             upd.PreferredVendorID = current.IsDefault == true ? 
    ///                                                          current.VendorID : 
    ///                                                          null;
    ///             upd = this.Item.Update(upd);
    ///             upd.PreferredVendorLocationID = current.IsDefault == 
    ///                 true ? current.VendorLocationID : null;
    ///             Item.Update(upd);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code updates the master data record in a many-to-one relationship by using the generic event handler approach." lang="CS">
    /// public class InventoryItemMaint : PXGraph&lt;InventoryItemMaint&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowInserted&lt;POVendorInventory&gt; e)
    ///     {
    ///         POVendorInventory current = e.Row;
    ///         if (current.IsDefault == true &amp;&amp; current.VendorID != null &amp;&amp; 
    ///             current.VendorLocationID != null &amp;&amp; current.SubItemID != null &amp;&amp;
    ///             this.Item.Current.PreferredVendorLocationID != 
    ///             current.VendorLocationID)
    ///         {
    ///             InventoryItem upd = Item.Current;
    ///             upd.PreferredVendorID = current.IsDefault == true ? 
    ///                                                          current.VendorID : 
    ///                                                          null;
    ///             upd = this.Item.Update(upd);
    ///             upd.PreferredVendorLocationID = current.IsDefault == 
    ///                 true ? current.VendorLocationID : null;
    ///             Item.Update(upd);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowInserted(PXCache sender, PXRowInsertedEventArgs e);

    /// <summary>Provides data for the <tt>RowInserted</tt> event.</summary>
    /// <seealso cref="PXRowInserted"/>
    public sealed class PXRowInsertedEventArgs : EventArgs
    {
        private readonly object _Row;
        private readonly object _NewRow;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="externalCall"></param>
        public PXRowInsertedEventArgs(object row, bool externalCall)
        {
            _Row = row;
            _ExternalCall = externalCall;
        }

        internal PXRowInsertedEventArgs(object row, object newRow, bool externalCall)
        {
            _Row = row;
            _NewRow = newRow;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the DAC object that has been inserted.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }

        internal object NewRow
        {
            get
            {
                return _NewRow;
            }
        }

        /// <summary>
        /// Returns <tt>true</tt> if the DAC object has been inserted in the UI or through the Web Service API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>The delegate for the <tt>RowDeleting</tt> event.</summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowDeletingEventArgs">PXRowDeletingEventArgs</see> type
    /// that holds data for the <tt>RowDeleting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowDeleting</tt> event is generated for a data record that is being deleted
    ///   from the <tt>PXCache</tt> object after its status has been set to
    /// <tt>Deleted</tt> or <tt>InsertedDeleted</tt>, but the data record can still be reverted
    /// to the previous state by canceling the deletion. The status of
    /// the data record is set to <tt>Deleted</tt> or <tt>InsertedDeleted</tt> as a result of either of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Deletion initiated in the UI or through the Web Service API.</item>
    ///     <item>Invocation of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Delete(object)</tt></li><li><tt>Delete(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">When a data record is deleted that has already been stored
    ///     in the database (and, hence, exists in
    ///     both the database and the <tt>PXCache</tt> object), the status of the data record is set to <tt>Deleted</tt>.
    ///     For a data record that has not yet been stored in the
    ///     database but was only inserted in the <tt>PXCache</tt> object, the status of the data record is set to <tt>InsertedDeleted</tt>.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The <tt>RowDeleting</tt> event handler is used to evaluate the data record that is marked as
    ///   <tt>Deleted</tt> or <tt>InsertedDeleted</tt> and cancel the deletion if
    /// it is required by the business logic.</para>
    ///   <para>The following execution order is used for the <tt>RowDeleting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowDeleting(PXCache sender, 
    ///                                            PXRowDeletingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowDeleting&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to evaluate the data record that is being deleted and cancel the deletion by throwing an exception." lang="CS">
    /// public class VendorMaint : BusinessAccountGraphBase&lt;
    ///     VendorR, VendorR, 
    ///      Where&lt;BAccount.type, 
    ///            Equal&lt;BAccountType.vendorType&gt;,
    ///            Or&lt;BAccount.type, 
    ///               Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void Vendor_RowDeleting(PXCache sender, 
    ///                                               PXRowDeletingEventArgs e)
    ///     {
    ///         Vendor row = e.Row as Vendor;
    ///  
    ///         TX.Tax tax = PXSelect&lt;
    ///             TX.Tax, 
    ///             Where&lt;TX.Tax.taxVendorID, 
    ///                   Equal&lt;Current&lt;Vendor.bAccountID&gt;&gt;&gt;&gt;.
    ///             Select(this);
    ///         if (tax != null)
    ///             throw new PXException(Messages.TaxVendorDeleteErr);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to evaluate the data record that is being deleted and cancel the deletion by throwing an exception." lang="CS">
    /// public class VendorMaint : BusinessAccountGraphBase&lt;
    ///     VendorR, VendorR, 
    ///      Where&lt;BAccount.type, 
    ///            Equal&lt;BAccountType.vendorType&gt;,
    ///            Or&lt;BAccount.type, 
    ///               Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowDeleting&lt;Vendor&gt; e)
    ///     {
    ///         Vendor row = e.Row;
    ///  
    ///         TX.Tax tax = PXSelect&lt;
    ///             TX.Tax, 
    ///             Where&lt;TX.Tax.taxVendorID, 
    ///                   Equal&lt;Current&lt;Vendor.bAccountID&gt;&gt;&gt;&gt;.
    ///             Select(this);
    ///         if (tax != null)
    ///             throw new PXException(Messages.TaxVendorDeleteErr);
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowDeleting(PXCache sender, PXRowDeletingEventArgs e);

    /// <summary>Provides data for the <tt>RowDeleting</tt> event.</summary>
    /// <seealso cref="PXRowDeleting"/>
    public sealed class PXRowDeletingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="externalCall"></param>
        public PXRowDeletingEventArgs(object row, bool externalCall)
        {
            _Row = row;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the DAC object that has been marked as <tt>Deleted</tt>.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }

        /// <summary>Returns <tt>true</tt> if the DAC object has been marked as <tt>Deleted</tt> in the UI or
        /// through the Web Service API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>The delegate for the <tt>RowDeleted</tt> event.</summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowDeletedEventArgs">PXRowDeletedEventArgs</see>
    /// type that holds data for the <tt>RowDeleted</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowDeleted</tt> event is generated for a data record that is being deleted from the <tt>PXCache</tt>
    ///   object—that is, a data record whose status has been successfully set to <tt>Deleted</tt> or <tt>InsertedDeleted</tt>
    ///   as a result of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Deletion initiated in the UI or through the Web Service API</item>
    ///     <item>Invocation of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Delete(object)</tt></li><li><tt>Delete(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">When a data record is deleted that has already been stored in the database
    ///     (and, hence, exists in both the database and the <tt>PXCache</tt> object),
    ///     the status of the data record is set to <tt>Deleted</tt>. For a data record that has not yet been stored in the database
    ///     but has only been inserted in the <tt>PXCache</tt> object, the status of the data record is set to <tt>InsertedDeleted</tt>.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The <tt>RowDeleted</tt> event handler is used to implement the business logic of the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Deletion of the detail data records in a one-to-many relationship</item>
    ///     <item>Update of the master data record in a many-to-one relationship</item>
    ///     <item>Deletion or update of the related data record in a one-to-one relationship</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowDeleted</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowDeleted(PXCache sender, 
    ///                                           PXRowDeletedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowDeleted&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code deletes detail data records in a one-to-many relationship by using the classic event handler approach." lang="CS">
    /// public class CashTransferEntry : PXGraph&lt;CashTransferEntry, CATransfer&gt;
    /// {
    ///     ...
    ///     
    ///     public virtual void CATransfer_RowDeleted(PXCache sender, 
    ///                                               PXRowDeletedEventArgs e)
    ///     {
    ///         foreach (CATran item in TransferTran.Select())
    ///             TransferTran.Delete(item);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code deletes detail data records in a one-to-many relationship by using the generic event handler approach." lang="CS">
    /// public class CashTransferEntry : PXGraph&lt;CashTransferEntry, CATransfer&gt;
    /// {
    ///     ...
    ///     
    ///     public virtual void _(Events.RowDeleted&lt;CATransfer&gt; e)
    ///     {
    ///         foreach (CATran item in TransferTran.Select())
    ///             TransferTran.Delete(item);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code updates the master data record in a many-to-one relationship by using the classic event handler approach." lang="CS">
    /// public class INSiteMaint : PXGraph&lt;INSiteMaint, INSite&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void INLocation_RowDeleted(PXCache sender, 
    ///                                                  PXRowDeletedEventArgs e)
    ///     {
    ///         INLocation l = (INLocation)e.Row;
    ///         if (site.Current == null || l == null ||
    ///             site.Cache.GetStatus(site.Current) == PXEntryStatus.Deleted) 
    ///             return;
    ///  
    ///         INSite s = site.Current;
    ///         if (s.DropShipLocationID == l.LocationID)
    ///             s.DropShipLocationID = null;
    ///         if (s.ReceiptLocationID == l.LocationID)
    ///             s.ReceiptLocationID = null;
    ///         if (s.ShipLocationID == l.LocationID)
    ///             s.ShipLocationID = null;
    ///         if (s.ReturnLocationID == l.LocationID)
    ///             s.ReturnLocationID = null;
    ///         site.Update(s);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code updates the master data record in a many-to-one relationship by using the generic event handler approach." lang="CS">
    /// public class INSiteMaint : PXGraph&lt;INSiteMaint, INSite&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowDeleted&lt;INLocation&gt; e)
    ///     {
    ///         INLocation l = e.Row;
    ///         if (site.Current == null || l == null ||
    ///             site.Cache.GetStatus(site.Current) == PXEntryStatus.Deleted) 
    ///             return;
    ///  
    ///         INSite s = site.Current;
    ///         if (s.DropShipLocationID == l.LocationID)
    ///             s.DropShipLocationID = null;
    ///         if (s.ReceiptLocationID == l.LocationID)
    ///             s.ReceiptLocationID = null;
    ///         if (s.ShipLocationID == l.LocationID)
    ///             s.ShipLocationID = null;
    ///         if (s.ReturnLocationID == l.LocationID)
    ///             s.ReturnLocationID = null;
    ///         site.Update(s);
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowDeleted(PXCache sender, PXRowDeletedEventArgs e);

    /// <summary>Provides data for the <tt>RowDeleted</tt> event.</summary>
    /// <seealso cref="PXRowDeleted"/>
    public sealed class PXRowDeletedEventArgs : EventArgs
    {
        private readonly object _Row;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="externalCall"></param>
        public PXRowDeletedEventArgs(object row, bool externalCall)
        {
            _Row = row;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the DAC object that has been marked as <tt>Deleted</tt>.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }

        /// <summary>Returns <tt>true</tt> if the DAC object has been marked as <tt>Deleted</tt> in the UI or
        /// through the Web Services API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowSelected</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowSelectedEventArgs">PXRowSelectedEventArgs</see> type that holds data for the <tt>RowSelected</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowSelected</tt> event is generated in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>To display a data record in the UI</item>
    ///     <item>To execute the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Locate(IDictionary)</tt></li><li><tt>Insert()</tt></li><li><tt>Insert()</tt></li><li><tt>Insert(IDictionary)</tt></li><li><tt>Update(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>Delete(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">Avoid executing BQL statements in a <tt>RowSelected</tt> event handler,
    ///     because this execution may cause performance degradation caused by multiple invocations
    ///     of the <tt>RowSelected</tt> event for a single data record.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The <tt>RowSelected</tt> event handler is used to do the following:</para>
    ///   <list type="bullet">
    ///     <item>Implement the UI presentation logic</item>
    ///     <item>Set up the processing operation on a processing screen (which is a type of UI screen
    ///     that allows the execution of a long-running operation on multiple data records at once)</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowSelected</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowSelected(PXCache sender, 
    ///                                            PXRowSelectedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowSelected&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code sets the UI properties for input controls at run time by using the classic event handler approach." lang="CS">
    /// public class VendorMaint :
    ///      BusinessAccountGraphBase&lt;VendorR, VendorR,
    ///          Where&lt;BAccount.type, Equal&lt;BAccountType.vendorType&gt;,
    ///              Or&lt;BAccount.type, Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void Vendor_RowSelected(PXCache sender,
    ///                                               PXRowSelectedEventArgs e)
    ///     {
    ///         Vendor row = (Vendor)e.Row;
    ///         if (row == null) return;
    ///  
    ///         bool isNotInserted = !(sender.GetStatus(row) ==
    ///                                PXEntryStatus.Inserted);
    ///         PXUIFieldAttribute.SetVisible&lt;VendorBalanceSummary.depositsBalance&gt;(
    ///             VendorBalance.Cache, null, isNotInserted);
    ///         PXUIFieldAttribute.SetVisible&lt;VendorBalanceSummary.balance&gt;(
    ///             VendorBalance.Cache, null, isNotInserted);
    ///         PXUIFieldAttribute.SetEnabled&lt;Vendor.taxReportFinPeriod&gt;(
    ///             sender, null,
    ///             row.TaxPeriodType != PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod);
    ///         PXUIFieldAttribute.SetEnabled&lt;Vendor.taxReportPrecision&gt;(
    ///             sender, null, row.TaxUseVendorCurPrecision != true);
    ///     }
    ///  
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code sets the UI properties for input controls at run time by using the generic event handler approach." lang="CS">
    /// public class VendorMaint :
    ///      BusinessAccountGraphBase&lt;VendorR, VendorR,
    ///          Where&lt;BAccount.type, Equal&lt;BAccountType.vendorType&gt;,
    ///              Or&lt;BAccount.type, Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void _(Events.RowSelected&lt;Vendor&gt; e)
    ///     {
    ///         Vendor row = e.Row;
    ///         if (row == null) return;
    ///  
    ///         bool isNotInserted = !(e.Cache.GetStatus(row) ==
    ///                                PXEntryStatus.Inserted);
    ///         PXUIFieldAttribute.SetVisible&lt;VendorBalanceSummary.depositsBalance&gt;(
    ///             VendorBalance.Cache, null, isNotInserted);
    ///         PXUIFieldAttribute.SetVisible&lt;VendorBalanceSummary.balance&gt;(
    ///             VendorBalance.Cache, null, isNotInserted);
    ///         PXUIFieldAttribute.SetEnabled&lt;Vendor.taxReportFinPeriod&gt;(
    ///             e.Cache, null,
    ///             row.TaxPeriodType != PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod);
    ///         PXUIFieldAttribute.SetEnabled&lt;Vendor.taxReportPrecision&gt;(
    ///             e.Cache, null, row.TaxUseVendorCurPrecision != true);
    ///     }
    ///  
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code sets the UI properties for actions by using the classic event handler approach." lang="CS">
    /// public class APAccess : PX.SM.BaseAccess
    /// {
    ///     ...
    ///     
    ///     protected virtual void RelationGroup_RowSelected(PXCache sender, 
    ///                                                      PXRowSelectedEventArgs e)
    ///     {
    ///         PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
    ///         if (group != null)
    ///         {
    ///             if (String.IsNullOrEmpty(group.GroupName))
    ///             {
    ///                 Save.SetEnabled(false);
    ///                 Vendor.Cache.AllowInsert = false;
    ///             }
    ///             else
    ///             {
    ///                 Save.SetEnabled(true);
    ///                 Vendor.Cache.AllowInsert = true;
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code sets the UI properties for actions by using the generic event handler approach." lang="CS">
    /// public class APAccess : PX.SM.BaseAccess
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowSelected&lt;RelationGroup&gt; e)
    ///     {
    ///         RelationGroup group = e.Row;
    ///         if (group != null)
    ///         {
    ///             if (String.IsNullOrEmpty(group.GroupName))
    ///             {
    ///                 Save.SetEnabled(false);
    ///                 Vendor.Cache.AllowInsert = false;
    ///             }
    ///             else
    ///             {
    ///                 Save.SetEnabled(true);
    ///                 Vendor.Cache.AllowInsert = true;
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example4a" description="The following code sets up the processing operation on a processing screen by using the classic event handler approach." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class APIntegrityCheck : PXGraph&lt;APIntegrityCheck&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void APIntegrityCheckFilter_RowSelected(
    ///         PXCache sender, 
    ///         PXRowSelectedEventArgs e)
    ///     {
    ///         APIntegrityCheckFilter filter = Filter.Current;
    ///  
    ///         APVendorList.SetProcessDelegate&lt;APReleaseProcess&gt;(
    ///             delegate(APReleaseProcess re, Vendor vend)
    ///             {
    ///                 re.Clear(PXClearOption.PreserveTimeStamp);
    ///                 re.IntegrityCheckProc(vend, filter.FinPeriodID);
    ///             }
    ///         );
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example4b" description="The following code sets up the processing operation on a processing screen by using the generic event handler approach." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class APIntegrityCheck : PXGraph&lt;APIntegrityCheck&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowSelected&lt;APIntegrityCheckFilter&gt; e)
    ///     {
    ///         APIntegrityCheckFilter filter = Filter.Current;
    ///  
    ///         APVendorList.SetProcessDelegate&lt;APReleaseProcess&gt;(
    ///             delegate(APReleaseProcess re, Vendor vend)
    ///             {
    ///                 re.Clear(PXClearOption.PreserveTimeStamp);
    ///                 re.IntegrityCheckProc(vend, filter.FinPeriodID);
    ///             }
    ///         );
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowSelected(PXCache sender, PXRowSelectedEventArgs e);

    /// <summary>Provides data for the <tt>RowSelected</tt> event.</summary>
    /// <seealso cref="PXRowSelected"/>
    public sealed class PXRowSelectedEventArgs : EventArgs
    {
        private readonly object _Row;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="row"></param>
        public PXRowSelectedEventArgs(object row)
        {
            _Row = row;
        }

        /// <summary>Returns the DAC object that is being processed.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
    }

    /// <summary>The delegate for the <tt>CommandPreparing</tt> event.</summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXCommandPreparingEventArgs">PXCommandPreparingEventArgs</see> type
    /// that contains data for the <tt>CommandPreparing</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>CommandPreparing</tt> event is generated each time the Acumatica data access layer prepares a database-specific SQL
    /// statement for a <tt>SELECT</tt>, <tt>INSERT</tt>, <tt>UPDATE</tt>, or <tt>DELETE</tt> operation. This event is raised
    /// for every data access class (DAC) field placed in the <tt>PXCache</tt> object.
    /// By using the <tt>CommandPreparing</tt> event subscriber, you can alter the property values of the
    /// <tt>PXCommandPreparingEventArgs.FieldDescription</tt> object that is used in the generation of an SQL statement.</para>
    ///   <para>The <tt>CommandPreparing</tt> event handler is used in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>To exclude a DAC field from a <tt>SELECT</tt>, <tt>INSERT</tt>, or <tt>UPDATE</tt> operation</item>
    ///     <item>To replace a DAC field from a <tt>SELECT</tt> operation with a custom SQL statement</item>
    ///     <item>To transform a DAC field value submitted to the server for an <tt>INSERT</tt>, <tt>UPDATE</tt>, or <tt>DELETE</tt> operation</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>CommandPreparing</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldType_CommandPreparing(
    ///   PXCache sender, 
    ///   PXCommandPreparingEventArgs e) 
    /// { 
    ///   ... 
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.CommandPreparing&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code excludes a DAC field from the <tt>UPDATE</tt> operation by using the classic event handler approach." lang="CS">
    /// public class APReleaseProcess : PXGraph&lt;APReleaseProcess&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void APRegister_FinPeriodID_CommandPreparing(
    ///         PXCache sender, 
    ///         PXCommandPreparingEventArgs e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) == PXDBOperation.Update)
    ///         {
    ///             e.ExcludeFromInsertUpdate();
    ///         }
    ///     }
    /// }</code>
    ///   <code title="Example2b" description="The following code excludes a DAC field from the <tt>UPDATE</tt> operation by using the generic event handler approach." lang="CS">
    /// public class APReleaseProcess : PXGraph&lt;APReleaseProcess&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.CommandPreparing&lt;APRegister.FinPeriodID&gt; e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) == PXDBOperation.Update)
    ///         {
    ///             e.Args.ExcludeFromInsertUpdate();
    ///         }
    ///     }
    /// }</code>
    ///   <code title="Example3a" description="The following code transforms the DAC field value during <tt>INSERT</tt> and <tt>UPDATE</tt> operations by using a redefinition of the attribute method." lang="CS">
    /// public class PXDBCryptStringAttribute : PXDBStringAttribute, 
    ///                                         IPXFieldVerifyingSubscriber, 
    ///                                         IPXRowUpdatingSubscriber, 
    ///                                         IPXRowSelectingSubscriber
    /// {
    ///     ...
    ///  
    ///     public override void CommandPreparing(PXCache sender, 
    ///                                           PXCommandPreparingEventArgs e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) == PXDBOperation.Insert ||
    ///             (e.Operation &amp; PXDBOperation.Command) == PXDBOperation.Update)
    ///         {
    ///             string value = (string)sender.GetValue(e.Row, _FieldOrdinal);
    ///  
    ///             e.Value = !string.IsNullOrEmpty(value) ?
    ///                       Convert.ToBase64String(
    ///                           Encrypt(Encoding.Unicode.GetBytes(value))) :
    ///                       null;
    ///         }
    ///         base.CommandPreparing(sender, e);
    ///     }
    ///  
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e);

    /// <summary>Provides data for the <tt>CommandPreparing</tt> event.</summary>
    /// <seealso cref="PXCommandPreparing"/>
    public sealed class PXCommandPreparingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The nested class that provides information about the field
        /// required for the SQL statement generation.
        /// </summary>
        [Serializable]
        public sealed class FieldDescription : ICloneable, IEquatable<FieldDescription>
        {
            /// <summary>
            /// The type of the DAC objects placed in the cache.
            /// </summary>
            public readonly Type BqlTable;
            /// <summary>The <tt>PXDbType</tt> value of the DAC field being
            /// used during the current operation.</summary>
            public PXDbType DataType;
            /// <summary>
            /// The storage size of the DAC field.
            /// </summary>
            public int? DataLength;
            /// <summary>
            /// The value stored in the DAC field.
            /// </summary>
            public object DataValue;
            /// <summary>
            /// The value indicating that the DAC field being used during the
            /// <tt>UPDATE</tt> or <tt>DELETE</tt> operation is placed in the <tt>WHERE</tt> clause.
            /// </summary>
            public readonly bool IsRestriction;

            /// <summary>The SQL tree expression of the field.</summary>
            public SQLTree.SQLExpression Expr;

            internal bool IsExcludedFromUpdate { get; private set; }

            internal FieldDescription(Type bqlTable, SQLTree.SQLExpression expr, PXDbType dataType, int? dataLength, object dataValue, bool isRestriction, bool isExcludedFromUpdate = false)
            {
                BqlTable = bqlTable;
                DataType = dataType;
                DataLength = dataLength;
                DataValue = dataValue;
                IsRestriction = isRestriction;
                IsExcludedFromUpdate = isExcludedFromUpdate;
                Expr = expr;
            }

            public object Clone()
            {
                return new FieldDescription(BqlTable, Expr, DataType, DataLength, DataValue, IsRestriction, IsExcludedFromUpdate);
            }

            public bool Equals(FieldDescription other)
            {
                if (other == null)
                    return false;
                if (Object.ReferenceEquals(this, other))
                    return true;
                return BqlTable == other.BqlTable
                       && Expr == other.Expr
                       && DataType == other.DataType
                       && DataLength == other.DataLength
                       && (ReferenceEquals(DataValue, other.DataValue) || DataValue != null && DataValue.Equals(other.DataValue))
                       && IsRestriction == other.IsRestriction
                       && IsExcludedFromUpdate == other.IsExcludedFromUpdate;
            }
        }
        /// <summary>Initializes and returns an object that contains a DAC field description
        /// that was used during the current operation.</summary>
        public FieldDescription GetFieldDescription()
        {
            return new FieldDescription(_BqlTable, Expr, _DataType, _DataLength, _DataValue, _IsRestriction, _isExcludedFromUpdate);
        }
        public void FillFromFieldDescription(FieldDescription fDescr)
        {
            if (fDescr == null)
                throw new ArgumentNullException("fDescr");
            _BqlTable = fDescr.BqlTable;
            _DataLength = fDescr.DataLength;
            _DataType = fDescr.DataType;
            _DataValue = fDescr.DataValue;
            Expr = fDescr.Expr;
            _IsRestriction = fDescr.IsRestriction;
            _isExcludedFromUpdate = fDescr.IsExcludedFromUpdate;
            Expr = fDescr.Expr;
        }
        private readonly object _Row;
        private object _Value;
        private readonly PXDBOperation _Operation;
        private readonly Type _Table;
        private Type _BqlTable;
        private PXDbType _DataType = PXDbType.Unspecified;
        private int? _DataLength;
        private object _DataValue;
        private bool _IsRestriction;
        /// <summary>The SQL dialect of the command.</summary>
        public readonly ISqlDialect SqlDialect;
        private bool _isExcludedFromUpdate = false;
        /// <summary>The SQL tree expression of the command.</summary>
        public SQLTree.SQLExpression Expr;

        /// <summary>
        /// Initializes an instance of the <tt>PXCommandPreparingEventArgs</tt> class.
        /// </summary>
        /// <param name="row">The data record.</param>
        /// <param name="value">The field value.</param>
        /// <param name="operation">The type of the database operation.</param>
        /// <param name="table">The DAC type of the data record.</param>
        /// <param name="dialect">The SQL dialect.</param>
        public PXCommandPreparingEventArgs(object row, object value, PXDBOperation operation, Type table, ISqlDialect dialect = null)
        {
            _Row = row;
            _Value = value;
            _Operation = operation;
            _Table = table;
            SqlDialect = dialect;
        }

        public void ExcludeFromInsertUpdate()
        {
            _isExcludedFromUpdate = true;
        }

        public bool IsSelect()
        {
            return (_Operation & PXDBOperation.Command) == PXDBOperation.Select;
        }

        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the current DAC field value or sets the value for the DAC field.</summary>
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
        /// <summary>Returns the type of the current database operation.</summary>
        public PXDBOperation Operation
        {
            get
            {
                return _Operation;
            }
        }
        /// <summary>Returns the type of the DAC objects placed in the cache.</summary>
        public Type Table
        {
            get
            {
                return _Table;
            }
        }
        /// <summary>Returns or sets the DAC type that is being used during the current operation.</summary>
        public Type BqlTable
        {
            get
            {
                return _BqlTable;
            }
            set
            {
                _BqlTable = value;
            }
        }

        /// <summary>Returns or sets the <tt>PXDbType</tt> value of the DAC field being
        /// used during the current operation.</summary>
        public PXDbType DataType
        {
            get
            {
                return _DataType;
            }
            set
            {
                _DataType = value;
            }
        }
        /// <summary>Returns or sets the number of characters in the DAC field being
        /// used during the current operation.</summary>
        public int? DataLength
        {
            get
            {
                return _DataLength;
            }
            set
            {
                _DataLength = value;
            }
        }
        /// <summary>Returns or sets the DAC field value being used during the
        /// current operation.</summary>
        public object DataValue
        {
            get
            {
                return _DataValue;
            }
            set
            {
                _DataValue = value;
            }
        }
        /// <summary>Returns or sets the value indicating that the DAC field
        /// being used during the <tt>UPDATE</tt> or <tt>DELETE</tt> operation is placed in the <tt>WHERE</tt> clause.</summary>
        public bool IsRestriction
        {
            get
            {
                return _IsRestriction;
            }
            set
            {
                _IsRestriction = value;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowSelecting</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowSelectingEventArgs">PXRowSelectingEventArgs</see> type
    /// that holds data for the <tt>RowSelecting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowSelecting</tt> event is generated for each retrieved data record
    ///   when the result of a BQL statement is processed. For a BQL statement that
    /// contains a <tt>JOIN</tt> clause, the <tt>RowSelecting</tt> event is raised for every joined data access class (DAC).</para>
    ///   <para>The <tt>RowSelecting</tt> event handler can be used for the following purposes:</para>
    ///   <list type="bullet">
    ///     <item>To calculate DAC field values that are not bound to specific database columns</item>
    ///     <item>To modify the logic that converts the database record to the DAC</item>
    ///   </list>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">We recommend that you not use <tt>RowSelecting</tt> event handlers
    ///     in the application code. However, if you use these event handlers in the application code
    ///     and you execute additional BQL statements within a <tt>RowSelecting</tt> event
    ///     handler, you should use a separate connection scope for the execution. The connection scope
    ///     that is used to retrieve the data that generated
    ///     the <tt>RowSelecting</tt> event is still busy; therefore, no other operations can be performed
    ///     on this connection scope.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The following execution order is used for the <tt>RowSelecting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowSelecting(PXCache sender, 
    ///                                             PXRowSelectingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowSelecting&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to calculate a DAC field value that is not bound to a specific column in a database table." lang="CS">
    /// public class LocationMaint : 
    ///     LocationMaintBase&lt;Location, Location, 
    ///                       Where&lt;Location.bAccountID, 
    ///                             Equal&lt;Optional&lt;Location.bAccountID&gt;&gt;&gt;&gt;
    /// {
    ///  
    ///     ...
    ///     
    ///     protected virtual void Location_RowSelecting(PXCache sender, 
    ///                                                  PXRowSelectingEventArgs e)
    ///     {
    ///         Location record = (Location)e.Row;
    ///         if (record != null)
    ///             record.IsARAccountSameAsMain = 
    ///                 !object.Equals(record.LocationID, record.CARAccountLocationID);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to calculate a DAC field value that is not bound to a specific column in a database table." lang="CS">
    /// public class LocationMaint : 
    ///     LocationMaintBase&lt;Location, Location, 
    ///                       Where&lt;Location.bAccountID, 
    ///                             Equal&lt;Optional&lt;Location.bAccountID&gt;&gt;&gt;&gt;
    /// {
    ///  
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowSelecting&lt;Location&gt; e)
    ///     {
    ///         Location record = e.Row;
    ///         if (record != null)
    ///             record.IsARAccountSameAsMain = 
    ///                 !object.Equals(record.LocationID, record.CARAccountLocationID);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to execute an additional BQL statement to calculate a DAC field value that is not bound to a specific column in a database table." lang="CS">
    /// public class SOInvoiceEntry : ARInvoiceEntry
    /// {
    ///     ...
    ///     
    ///     protected virtual void ARInvoice_RowSelecting(PXCache sender, 
    ///                                                   PXRowSelectingEventArgs e)
    ///     {
    ///         ARInvoice row = (ARInvoice)e.Row;
    ///         if (row != null &amp;&amp; !String.IsNullOrEmpty(row.DocType) 
    ///                         &amp;&amp; !String.IsNullOrEmpty(row.RefNbr))
    ///         {
    ///             row.IsCCPayment = false;
    ///             using (new PXConnectionScope())
    ///             {
    ///                 if (PXSelectJoin&lt;
    ///                     CustomerPaymentMethodC,
    ///                     InnerJoin&lt;
    ///                         CA.PaymentMethod,
    ///                         On&lt;CA.PaymentMethod.paymentMethodID, 
    ///                            Equal&lt;CustomerPaymentMethodC.paymentMethodID&gt;&gt;,
    ///                         InnerJoin&lt;
    ///                             SOInvoice,
    ///                             On&lt;SOInvoice.pMInstanceID, 
    ///                                Equal&lt;CustomerPaymentMethodC.pMInstanceID&gt;&gt;&gt;&gt;,
    ///                     Where&lt;SOInvoice.docType, 
    ///                           Equal&lt;Required&lt;SOInvoice.docType&gt;&gt;,
    ///                           And&lt;SOInvoice.refNbr, 
    ///                               Equal&lt;Required&lt;SOInvoice.refNbr&gt;&gt;,
    ///                               And&lt;CA.PaymentMethod.paymentType, 
    ///                                   Equal&lt;CA.PaymentMethodType.creditCard&gt;,
    ///                               And&lt;CA.PaymentMethod.aRIsProcessingRequired, 
    ///                                   Equal&lt;True&gt;&gt;&gt;&gt;&gt;&gt;.
    ///                     Select(this, row.DocType, row.RefNbr).Count &gt; 0)
    ///                 {
    ///                     row.IsCCPayment = true;
    ///                 }
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code uses the generic event handler approach to execute an additional BQL statement to calculate a DAC field value that is not bound to a specific column in a database table." lang="CS">
    /// public class SOInvoiceEntry : ARInvoiceEntry
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowSelecting&lt;ARInvoice&gt; e)
    ///     {
    ///         ARInvoice row = e.Row;
    ///         if (row != null &amp;&amp; !String.IsNullOrEmpty(row.DocType) 
    ///                         &amp;&amp; !String.IsNullOrEmpty(row.RefNbr))
    ///         {
    ///             row.IsCCPayment = false;
    ///             using (new PXConnectionScope())
    ///             {
    ///                 if (PXSelectJoin&lt;
    ///                     CustomerPaymentMethodC,
    ///                     InnerJoin&lt;
    ///                         CA.PaymentMethod,
    ///                         On&lt;CA.PaymentMethod.paymentMethodID, 
    ///                            Equal&lt;CustomerPaymentMethodC.paymentMethodID&gt;&gt;,
    ///                         InnerJoin&lt;
    ///                             SOInvoice,
    ///                             On&lt;SOInvoice.pMInstanceID, 
    ///                                Equal&lt;CustomerPaymentMethodC.pMInstanceID&gt;&gt;&gt;&gt;,
    ///                     Where&lt;SOInvoice.docType, 
    ///                           Equal&lt;Required&lt;SOInvoice.docType&gt;&gt;,
    ///                           And&lt;SOInvoice.refNbr, 
    ///                               Equal&lt;Required&lt;SOInvoice.refNbr&gt;&gt;,
    ///                               And&lt;CA.PaymentMethod.paymentType, 
    ///                                   Equal&lt;CA.PaymentMethodType.creditCard&gt;,
    ///                               And&lt;CA.PaymentMethod.aRIsProcessingRequired, 
    ///                                   Equal&lt;True&gt;&gt;&gt;&gt;&gt;&gt;.
    ///                     Select(this, row.DocType, row.RefNbr).Count &gt; 0)
    ///                 {
    ///                     row.IsCCPayment = true;
    ///                 }
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example4a" description="The following code converts the database table value to the DAC field value by using a redefinition of the attribute method." lang="CS">
    /// public class PXDBCryptStringAttribute : PXDBStringAttribute, 
    ///                                         IPXFieldVerifyingSubscriber, 
    ///                                         IPXRowUpdatingSubscriber, 
    ///                                         IPXRowSelectingSubscriber
    /// {
    ///     ...
    ///     
    ///     public override void RowSelecting(PXCache sender, 
    ///                                       PXRowSelectingEventArgs e)
    ///     {
    ///         base.RowSelecting(sender, e);
    ///         if (e.Row == null || sender.GetStatus(e.Row) 
    ///                           != PXEntryStatus.Notchanged) return;
    ///         string value = (string)sender.GetValue(e.Row, _FieldOrdinal);
    ///         string result = string.Empty;
    ///         if (!string.IsNullOrEmpty(value))
    ///         {
    ///             try
    ///             {
    ///                 result = Encoding.
    ///                     Unicode.
    ///                     GetString(Decrypt(Convert.FromBase64String(value)));
    ///             }
    ///             catch (Exception)
    ///             {
    ///                 try
    ///                 {
    ///                     result = Encoding.Unicode.
    ///                         GetString(Convert.FromBase64String(value));
    ///                 }
    ///                 catch (Exception)
    ///                 {
    ///                     result = value;
    ///                 }
    ///             }
    ///         }
    ///         sender.SetValue(e.Row, _FieldOrdinal, 
    ///                         result.Replace("\0", string.Empty));
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowSelecting(PXCache sender, PXRowSelectingEventArgs e);

    /// <summary>Provides data for the <tt>RowSelecting</tt> event.</summary>
    /// <seealso cref="PXRowSelecting"/>
    public sealed class PXRowSelectingEventArgs : CancelEventArgs
    {
        private object _Row;
        private readonly PXDataRecord _Record;
        private int _Position;
        private readonly bool _IsReadOnly;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="record"></param>
        /// <param name="position"></param>
        /// <param name="isReadOnly"></param>
        public PXRowSelectingEventArgs(object row, PXDataRecord record, int position, bool isReadOnly)
        {
            _Row = row;
            _Record = record;
            _Position = position;
            _IsReadOnly = isReadOnly;
        }

        /// <summary>Returns the DAC object that is being processed.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
            internal set
            {
                _Row = value;
            }
        }
        /// <summary>Returns the processed data record in the result set.</summary>
        public PXDataRecord Record
        {
            get
            {
                return _Record;
            }
        }
        /// <summary>Returns or sets the index of the processed column in the result set.</summary>
        public int Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
            }
        }
        /// <summary>Returns the value indicating whether the DAC object is read-only.</summary>
        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowPersisting</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowPersistingEventArgs">PXRowPersistingEventArgs</see> type
    /// that holds data for the <tt>RowPersisting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowPersisting</tt> event is generated in the process of committing changes to the database
    ///   for every data record whose status is <tt>Inserted</tt>, <tt>Updated</tt>, or <tt>Deleted</tt> before
    ///   the corresponding changes for the data record are committed to the database. The committing of changes to a database
    ///   is initiated by invoking the <tt>Actions.PressSave()</tt> method of the business logic controller (BLC).
    ///   While processing this method, the Acumatica data access layer commits first every inserted
    /// data record, then every updated data record, and finally every deleted data record.</para>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">Avoid executing additional BQL statements
    ///     in a <tt>RowPersisting</tt> event handler. When the <tt>RowPersisting</tt>
    ///     event is raised, the associated transaction scope is busy saving the changes,
    ///     and any other operation performed within this transaction scope may cause
    ///     performance degradation and deadlocks.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The <tt>RowPersisting</tt> event handler is used to do the following:</para>
    ///   <list type="bullet">
    ///     <item>Validate the data record before it has been committed to the database</item>
    ///     <item>Cancel the committing of the data record by throwing an exception</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowPersisting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowPersisting(PXCache sender, 
    ///                                              PXRowPersistingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowPersisting&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to validate the data record before it is committed to the database." lang="CS">
    /// public class CCProcessingCenterMaint : PXGraph&lt;CCProcessingCenterMaint, 
    ///                                                CCProcessingCenter&gt;, 
    ///                                        IProcessingCenterSettingsStorage
    /// {
    ///     ...
    ///     
    ///     protected virtual void CCProcessingCenter_RowPersisting(
    ///         PXCache sender, 
    ///         PXRowPersistingEventArgs e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) != PXDBOperation.Delete &amp;&amp; 
    ///              e.Row != null &amp;&amp; 
    ///              (bool)((CCProcessingCenter)e.Row).IsActive &amp;&amp; 
    ///              string.IsNullOrEmpty(((CCProcessingCenter)e.Row).
    ///                  ProcessingTypeName))
    ///         {
    ///             throw new PXRowPersistingException(
    ///                 typeof(CCProcessingCenter.processingTypeName).Name, 
    ///                 null, 
    ///                 ErrorMessages.FieldIsEmpty, 
    ///                 typeof(CCProcessingCenter.processingTypeName).Name);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to validate the data record before it is committed to the database." lang="CS">
    /// public class CCProcessingCenterMaint : PXGraph&lt;CCProcessingCenterMaint, 
    ///                                                CCProcessingCenter&gt;, 
    ///                                        IProcessingCenterSettingsStorage
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowPersisting&lt;CCProcessingCenter&gt; e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) != PXDBOperation.Delete &amp;&amp; 
    ///              e.Row != null &amp;&amp; 
    ///              (bool)e.Row.IsActive &amp;&amp; 
    ///              string.IsNullOrEmpty(e.Row.ProcessingTypeName))
    ///         {
    ///             throw new PXRowPersistingException(
    ///                 typeof(CCProcessingCenter.processingTypeName).Name, 
    ///                 null, 
    ///                 ErrorMessages.FieldIsEmpty, 
    ///                 typeof(CCProcessingCenter.processingTypeName).Name);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to show a message box, as well as the warning and error indications near the input control for one field or multiple fields." lang="CS">
    /// protected virtual void APInvoice_RowPersisting(PXCache sender,
    ///                                                PXRowPersistingEventArgs e)
    /// {
    ///     APInvoice doc = (APInvoice)e.Row;
    ///     if (doc.PaySel == true &amp;&amp; doc.PayDate == null)
    ///     {
    ///         sender.RaiseExceptionHandling&lt;APInvoice.payDate&gt;(
    ///             doc, null,
    ///             new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
    ///                                        typeof(APInvoice.payDate).Name));
    ///     }
    ///     if (doc.PaySel == true &amp;&amp; doc.PayDate != null &amp;&amp;
    ///         ((DateTime)doc.DocDate).CompareTo((DateTime)doc.PayDate) &gt; 0)
    ///     {
    ///         sender.RaiseExceptionHandling&lt;APInvoice.payDate&gt;(
    ///             e.Row, doc.PayDate,
    ///             new PXSetPropertyException(Messages.ApplDate_Less_DocDate,
    ///                                        PXErrorLevel.RowError,
    ///                                        typeof(APInvoice.payDate).Name));
    ///     }
    /// }</code>
    ///   <code title="Example3b" description="The following code uses the generic event handler approach to show a message box, as well as the warning and error indications near the input control for one field or multiple fields." lang="CS">
    /// protected virtual void _(Events.RowPersisting&lt;APInvoice&gt; e)
    /// {
    ///     APInvoice doc = e.Row;
    ///     if (doc.PaySel == true &amp;&amp; doc.PayDate == null)
    ///     {
    ///         e.Cache.RaiseExceptionHandling&lt;APInvoice.payDate&gt;(
    ///             doc, null,
    ///             new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
    ///                                        typeof(APInvoice.payDate).Name));
    ///     }
    ///     if (doc.PaySel == true &amp;&amp; doc.PayDate != null &amp;&amp;
    ///         ((DateTime)doc.DocDate).CompareTo((DateTime)doc.PayDate) &gt; 0)
    ///     {
    ///         e.Cache.RaiseExceptionHandling&lt;APInvoice.payDate&gt;(
    ///             e.Row, doc.PayDate,
    ///             new PXSetPropertyException(Messages.ApplDate_Less_DocDate,
    ///                                        PXErrorLevel.RowError,
    ///                                        typeof(APInvoice.payDate).Name));
    ///     }
    /// }</code>
    ///   <code title="Example4a" description="The following code uses the classic event handler approach to cancel the operation of committing a data record." lang="CS">
    /// public class CampaignMemberMassProcess : PXGraph&lt;CampaignMemberMassProcess&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void Contact_RowPersisting(PXCache sender, 
    ///                                                  PXRowPersistingEventArgs e)
    ///     {
    ///         e.Cancel = true;
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example4b" description="The following code uses the generic event handler approach to cancel the operation of committing a data record." lang="CS">
    /// public class CampaignMemberMassProcess : PXGraph&lt;CampaignMemberMassProcess&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.RowPersisting&lt;Contact&gt; e)
    ///     {
    ///         e.Cancel = true;
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowPersisting(PXCache sender, PXRowPersistingEventArgs e);

    /// <summary>Provides data for the <tt>RowPersisting</tt> event.</summary>
    /// <seealso cref="PXRowPersisting"/>
    public sealed class PXRowPersistingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private readonly PXDBOperation _Operation;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="row"></param>
        public PXRowPersistingEventArgs(PXDBOperation operation, object row)
        {
            _Row = row;
            _Operation = operation;
        }

        /// <summary>Returns the DAC object that is being committed to the database.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the <tt>PXDBOperation</tt> of the current commit operation.</summary>
        public PXDBOperation Operation
        {
            get
            {
                return _Operation;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>RowPersisted</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="e">Required. The instance of the <see cref="PXRowPersistedEventArgs">PXRowPersistedEventArgs</see> type
    /// that holds data for the <tt>RowPersisted</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>RowPersisted</tt> event is generated in the process of committing changes to the database
    ///   for every data record whose status is <tt>Inserted</tt>, <tt>Updated</tt>,
    /// or <tt>Deleted</tt>. The <tt>RowPersisted</tt> event is generated in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>When the data record has been committed to the database and the status of the transaction scope
    ///     (indicated in the <tt>e.TranStatus</tt> field) is <tt>Open</tt></item>
    ///     <item>When the status of the transaction scope has been changed to <tt>Completed</tt>, indicating successful committing,
    ///     or <tt>Aborted</tt>, indicating that a database error has occurred and changes to the database have been dropped</item>
    ///   </list>
    ///   <para>The <tt>Actions.PressSave()</tt> method of the business logic controller (graph)
    ///   initiates the committing of changes to a database. While processing this method,
    /// the Acumatica data access layer commits first every inserted data record, then every updated data record,
    /// and finally every deleted data record.</para>
    ///   <innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    ///     <innovasys:widgetproperty layout="block" name="Content">Avoid executing additional BQL statements in a
    ///     <tt>RowPersisted</tt> event handler when the status of the transaction scope is <tt>Open</tt>.
    ///     When the <tt>RowPersisted</tt> event is raised with this status, the associated transaction scope is busy saving the changes,
    ///     and any other operation performed within this transaction scope may cause performance degradation and deadlocks.</innovasys:widgetproperty>
    ///   </innovasys:widget>
    ///   <para>The <tt>RowPersisted</tt> event handler is used to perform the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Retrieval of data generated by the database.</item>
    ///     <item>Restoring of data access class (DAC) field values if the status of the transaction scope
    ///     is <tt>Aborted</tt> indicating that changes have not been saved. Note that in this case
    ///     the DAC fields do not revert to any previous state automatically but are left
    ///     by the Acumatica data access layer in the state they were in before
    ///     the committing was initiated.</item>
    ///     <item>Validation of the data record while committing it to the database.</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>RowPersisted</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_RowPersisted(PXCache sender, 
    ///                                             PXRowPersistedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.RowPersisted&lt;DACType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the redefinition of the attribute method to retrieve data generated by the database." lang="CS">
    /// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | 
    ///                 AttributeTargets.Class | AttributeTargets.Method)]
    /// public class PXDBIdentityAttribute : PXDBFieldAttribute, 
    ///                                      IPXFieldDefaultingSubscriber, 
    ///                                      IPXRowSelectingSubscriber, 
    ///                                      IPXCommandPreparingSubscriber, 
    ///                                      IPXFieldUpdatingSubscriber, 
    ///                                      IPXFieldSelectingSubscriber, 
    ///                                      IPXRowPersistedSubscriber, 
    ///                                      IPXFieldVerifyingSubscriber
    /// {
    ///     ...
    ///     
    ///     public virtual void RowPersisted(PXCache sender, 
    ///                                      PXRowPersistedEventArgs e)
    ///     {
    ///         if ((e.Operation &amp; PXDBOperation.Command) == PXDBOperation.Insert)
    ///         {
    ///             if (e.TranStatus == PXTranStatus.Open)
    ///             {
    ///                 if (_KeyToAbort == null)
    ///                     _KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);
    ///                 if (_KeyToAbort &lt; 0)
    ///                 {
    ///                     int? id = 
    ///                         Convert.ToInt32(PXDatabase.SelectIdentity(_BqlTable, _FieldName));
    ///                     if ((id ?? 0m) == 0m)
    ///                     {
    ///                         PXDataField[] pars = 
    ///                             new PXDataField[sender.Keys.Count + 1];
    ///                         pars[0] = new PXDataField(_DatabaseFieldName);
    ///                         for (int i = 0; i &lt; sender.Keys.Count; i++)
    ///                         {
    ///                             string name = sender.Keys[i];
    ///                             PXCommandPreparingEventArgs.
    ///                                 FieldDescription description = null;
    ///                             sender.RaiseCommandPreparing(
    ///                                 name, e.Row, 
    ///                                 sender.GetValue(e.Row, name), 
    ///                                 PXDBOperation.Select, 
    ///                                 _BqlTable, out description);
    ///                             if (description != null &amp;&amp;
    ///                                     description.Expr != null &amp;&amp; 
    ///                                 description.IsRestriction)
    ///                             {
    ///                                 pars[i + 1] = new PXDataFieldValue(
    ///                                     description.Expr, 
    ///                                     description.DataType, 
    ///                                     description.DataLength, 
    ///                                     description.DataValue);
    ///                             }
    ///                         }
    ///                         using (PXDataRecord record = 
    ///                             PXDatabase.SelectSingle(_BqlTable, pars))
    ///                         {
    ///                             if (record != null)
    ///                                 id = record.GetInt32(0);
    ///                         }
    ///                     }
    ///                     sender.SetValue(e.Row, _FieldOrdinal, id);
    ///                 }
    ///                 else
    ///                     _KeyToAbort = null;
    ///             }
    ///             else if (e.TranStatus == PXTranStatus.Aborted &amp;&amp; 
    ///                      _KeyToAbort != null)
    ///             {
    ///                 sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
    ///                 _KeyToAbort = null;
    ///             }
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses a redefinition of the attribute method to restore the values of a DAC field if the commitment failed—resulting in the &lt;tt&gt;Aborted&lt;/tt&gt; status of the transaction scope." lang="CS">
    /// public class AddressRevisionIDAttribute : PXEventSubscriberAttribute, 
    ///                                           IPXRowPersistingSubscriber, 
    ///                                           IPXRowPersistedSubscriber
    /// {
    ///     ...
    ///     
    ///     public virtual void RowPersisted(PXCache sender, 
    ///                                      PXRowPersistedEventArgs e)
    ///     {
    ///         if (e.TranStatus == PXTranStatus.Aborted &amp;&amp; 
    ///             (e.Operation == PXDBOperation.Insert || e.Operation == 
    ///              PXDBOperation.Update))
    ///         {
    ///             int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal);
    ///             revision--;
    ///             sender.SetValue(e.Row, _FieldOrdinal, revision);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXRowPersisted(PXCache sender, PXRowPersistedEventArgs e);

    /// <summary>Provides data for the <tt>RowPersisted</tt> event.</summary>
    /// <seealso cref="PXRowPersisted"/>
    public sealed class PXRowPersistedEventArgs : EventArgs
    {
        private readonly object _Row;
        private readonly PXDBOperation _Operation;
        private readonly PXTranStatus _TranStatus;
        private readonly Exception _Exception;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="operation"></param>
        /// <param name="tranStatus"></param>
        /// <param name="exception"></param>
        public PXRowPersistedEventArgs(object row, PXDBOperation operation, PXTranStatus tranStatus, Exception exception)
        {
            _Row = row;
            _Operation = operation;
            _TranStatus = tranStatus;
            _Exception = exception;
        }

        /// <summary>Returns the DAC object that has been committed to the database.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the status of the transaction scope associated with the
        /// current commitment.</summary>
        public PXTranStatus TranStatus
        {
            get
            {
                return _TranStatus;
            }
        }
        /// <summary>Returns the <tt>PXDBOperation</tt> value, indicating the type of
        /// the current commitment.</summary>
        public PXDBOperation Operation
        {
            get
            {
                return _Operation;
            }
        }
        /// <summary>Returns the <tt>Exception</tt> object, which is thrown while changes are
        /// committed to the database.</summary>
        public Exception Exception
        {
            get
            {
                return _Exception;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>FieldSelecting</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXFieldSelectingEventArgs">PXFieldSelectingEventArgs</see> type
    /// that holds data for the <tt>FieldSelecting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>FieldSelecting</tt> event is generated in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>When the external representation—the way the value should be displayed in the UI—of
    ///     a data access class field (DAC) value is requested
    ///     from the UI or through the Web Service API.</item>
    ///     <item>When any of the following methods of the <tt>PXCache</tt> class initiates the assigning the default value to a field:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li></ul></item>
    ///     <item>While a field is updated in the <tt>PXCache</tt> object, and this action is initiated
    ///     by any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Update(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li></ul></item>
    ///     <item>While a DAC field value is requested through any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>GetValueInt(object, string)</tt></li><li><tt>GetValueIntField(object)</tt></li><li><tt>GetValueExt(object, string)</tt></li><li><tt>GetValueExt&lt;Field&gt;(object)</tt></li><li><tt>GetValuePending(object, string)</tt></li><li><tt>ToDictionary(object)</tt></li><li><tt>GetStateExt(object, string)</tt></li><li><tt>GetStateExt&lt;Field&gt;(object)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>FieldSelecting</tt> event handler is used to perform the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Conversion of the internal presentation of a DAC field (the data field value of a DAC instance)
    ///     to the external presentation (the value displayed in the UI)</item>
    ///     <item>Conversion of the values of multiple DAC fields to a single external presentation</item>
    ///     <item>Provision of additional information to set up a DAC field input control or cell presentation</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>FieldSelecting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    ///   <para></para>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_FieldSelecting(
    ///   PXCache sender, 
    ///   PXFieldSelectingEventArgs e) 
    /// { 
    ///   ... 
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.FieldSelecting&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.FieldSelecting&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code converts the DAC field value to its external presentation by using a redefinition of the attribute method." lang="CS">
    /// public class PXTimeSpanLongAttribute : PXIntAttribute
    /// {
    ///     ...
    ///     
    ///     public override void FieldSelecting(PXCache sender, 
    ///                                         PXFieldSelectingEventArgs e)
    ///     {
    ///         if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
    ///         {
    ///             string inputMask = this.inputMask ?? 
    ///                                _inputMasks[(int)this._Format];
    ///             int lenght = this.inputMask != null ? _maskLenght : 
    ///                                            _lengths[(int)this._Format];
    ///             inputMask = PXMessages.LocalizeNoPrefix(inputMask);
    ///             e.ReturnState = PXStringState.CreateInstance(
    ///                 e.ReturnState, 
    ///                 lenght, 
    ///                 null, 
    ///                 _FieldName, 
    ///                 _IsKey, 
    ///                 null, 
    ///                 String.IsNullOrEmpty(inputMask) ? null : inputMask, 
    ///                 null, null, null, null);
    ///         }
    ///         if (e.ReturnValue != null)
    ///         {
    ///             TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
    ///             int hours = 
    ///                 (this._Format == TimeSpanFormatType.LongHoursMinutes) ?
    ///                 span.Days * 24 + span.Hours :  span.Hours;
    ///             e.ReturnValue = string.Format(_outputFormats[(int)this._Format], 
    ///                                           span.Days, hours, span.Minutes);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code calculates the external value of a DAC field by using the classic event handler approach." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class RevalueAPAccounts : PXGraph&lt;RevalueAPAccounts&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void RevalueFilter_TotalRevalued_FieldSelecting(
    ///         PXCache sender, 
    ///         PXFieldSelectingEventArgs e)
    ///     {
    ///         if (e.Row == null) return;
    ///  
    ///         decimal val = 0m;
    ///         foreach (RevaluedAPHistory res in APAccountList.Cache.Updated)
    ///             if ((bool)res.Selected)
    ///                 val += (decimal)res.FinPtdRevalued;
    ///         e.ReturnValue = val;
    ///         e.Cancel = true;
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code calculates the external value of a DAC field by using the generic event handler approach." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class RevalueAPAccounts : PXGraph&lt;RevalueAPAccounts&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldSelecting&lt;RevalueFilter.TotalRevalued&gt; e)
    ///     {
    ///         if (e.Row == null) return;
    ///  
    ///         decimal val = 0m;
    ///         foreach (RevaluedAPHistory res in APAccountList.Cache.Updated)
    ///             if ((bool)res.Selected)
    ///                 val += (decimal)res.FinPtdRevalued;
    ///         e.Args.ReturnValue = val;
    ///         e.Args.Cancel = true;
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example4a" description="The following code defines the mask for the input control or the cell presentation of a DAC field by using a redefinition of the attribute method." lang="CS">
    /// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | 
    ///                 AttributeTargets.Class | AttributeTargets.Method)]
    /// public class PXDBStringWithMaskAttribute : PXDBStringAttribute, 
    ///                                            IPXFieldSelectingSubscriber 
    /// {
    ///     ...
    ///     
    ///     public override void FieldSelecting(PXCache sender, 
    ///                                         PXFieldSelectingEventArgs e)
    ///     {
    ///         if (e.Row == null) return;
    ///  
    ///         string mask = this.FindMask(sender, e.Row);
    ///         if (!string.IsNullOrEmpty(mask))
    ///             e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 
    ///                                                          _Length, 
    ///                                                          null, 
    ///                                                          _FieldName, 
    ///                                                          _IsKey, 
    ///                                                          null, 
    ///                                                          mask, 
    ///                                                          null, null, null, 
    ///                                                          null);
    ///         else
    ///             base.FieldSelecting(sender, e);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example6a" description="The following code defines the lists of values and labels for the PXDropDown input control of the DAC field by using a redefinition of the attribute method." lang="CS">
    /// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | 
    ///                 AttributeTargets.Parameter | AttributeTargets.Method)]
    /// [PXAttributeFamily(typeof(PXBaseListAttribute))]
    /// public class PXStringListAttribute : PXEventSubscriberAttribute, 
    ///                                      IPXFieldSelectingSubscriber
    /// {
    ///     ...
    ///     
    ///     public virtual void FieldSelecting(PXCache sender, 
    ///                                        PXFieldSelectingEventArgs e)
    ///     {
    ///         if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
    ///         {
    ///             string[] values = _AllowedValues;
    ///             e.ReturnState = PXStringState.CreateInstance(
    ///                 e.ReturnState, null, null, _FieldName,
    ///                 null, -1, null, values, _AllowedLabels,
    ///                 _ExclusiveValues, null);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXFieldSelecting(PXCache sender, PXFieldSelectingEventArgs args);

    /// <summary>Provides data for the <tt>FieldSelecting</tt> event.</summary>
    /// <seealso cref="PXFieldSelecting"/>
    public sealed class PXFieldSelectingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private object _ReturnValue;
        private bool _IsAltered;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="returnValue"></param>
        /// <param name="isAltered"></param>
        /// <param name="externalCall"></param>
        public PXFieldSelectingEventArgs(object row, object returnValue, bool isAltered, bool externalCall)
        {
            _Row = row;
            _ReturnValue = returnValue;
            _IsAltered = isAltered;
            _ExternalCall = externalCall;
        }
        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns or sets the data used to set up the DAC field input control
        /// or cell presentation.</summary>
        public object ReturnState
        {
            get
            {
                return _ReturnValue;
            }
            set
            {
                _ReturnValue = value;
            }
        }
        /// <summary>Returns or sets the value indicating whether the
        /// <tt>ReturnState</tt> property should be created for each data
        /// record.</summary>
        public bool IsAltered
        {
            get
            {
                return _IsAltered;
            }
            set
            {
                _IsAltered = value;
            }
        }
        /// <summary>Returns or sets the external presentation of the value of the
        /// DAC field.</summary>
        public object ReturnValue
        {
            get
            {
                PXFieldState state = _ReturnValue as PXFieldState;
                if (state == null)
                {
                    return _ReturnValue;
                }
                else
                {
                    return state.Value;
                }
            }
            set
            {
                PXFieldState state = _ReturnValue as PXFieldState;
                if (state == null)
                {
                    _ReturnValue = value;
                }
                else
                {
                    state.Value = value;
                }
            }
        }
        /// <summary>Returns <tt>true</tt> if the current DAC field has been
        /// selected in the UI or through the Web Service API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>FieldDefaulting</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXFieldDefaultingEventArgs">PXFieldDefaultingEventArgs</see>
    /// type that holds data for a <tt>FieldDefaulting</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>FieldDefaulting</tt> event is generated in either of the following cases:</para>
    ///   <list type="bullet">
    ///     <item>A new record is inserted into the <tt>PXCache</tt> object by user action in the user interface or via the Web API.</item>
    ///     <item>Any of the following methods of the <tt>PXCache</tt> class initiates the assigning the default value to a field:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li><li><tt>SetDefaultExt(object, string)</tt></li><li><tt>SetDefaultExt(object)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>FieldDefaulting</tt> event handler is used to generate and assign the default value to a data access class (DAC) field.</para>
    ///   <para>The following execution order is used for the <tt>FieldDefaulting</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_FieldDefaulting(
    ///     PXCache sender, 
    ///     PXFieldDefaultingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.FieldDefaulting&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.FieldDefaulting&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The example below shows how to generate a default value for a DAC field by using the classic event handler approach." lang="CS">
    /// public class POOrderEntry : PXGraph&lt;POOrderEntry, POOrder&gt;, 
    ///                             PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void POOrder_ExpectedDate_FieldDefaulting(
    ///         PXCache sender, 
    ///         PXFieldDefaultingEventArgs e)
    ///     {
    ///         POOrder row = (POOrder)e.Row;
    ///         Location vendorLocation = this.location.Current;
    ///         if (row != null &amp;&amp; row.OrderDate.HasValue)
    ///         {
    ///             int offset = (vendorLocation != null ? 
    ///                          (int)(vendorLocation.VLeadTime ?? 0) : 0);
    ///             e.NewValue = row.OrderDate.Value.AddDays(offset);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The example below shows how to generate a default value for a DAC field by using the generic event handler approach." lang="CS">
    /// public class POOrderEntry : PXGraph&lt;POOrderEntry, POOrder&gt;, 
    ///                             PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldDefaulting&lt;POOrder.ExpectedDate&gt; e)
    ///     {
    ///         POOrder row = (POOrder)e.Row;
    ///         Location vendorLocation = this.location.Current;
    ///         if (row != null &amp;&amp; row.OrderDate.HasValue)
    ///         {
    ///             int offset = (vendorLocation != null ? 
    ///                          (int)(vendorLocation.VLeadTime ?? 0) : 0);
    ///             e.NewValue = row.OrderDate.Value.AddDays(offset);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs args);

    /// <summary>Provides data for the <tt>FieldDefaulting</tt> event.</summary>
    /// <seealso cref="PXFieldDefaulting"/>
    public sealed class PXFieldDefaultingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private object _NewValue;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row">The current DAC object.</param>
        public PXFieldDefaultingEventArgs(object row)
        {
            _Row = row;
        }
        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns or sets the default value for a DAC field.</summary>
        public object NewValue
        {
            get
            {
                return _NewValue;
            }
            set
            {
                _NewValue = value;
            }
        }

    }

    /// <summary>
    ///   <para>The delegate for the <tt>FieldUpdating</tt> event.</para>
    ///   <para></para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXFieldUpdatingEventArgs">PXFieldUpdatingEventArgs</see>
    /// type that holds data for the <tt>FieldUpdating</tt> event.</param>
    /// <remarks>
    ///   <para>In the following cases, the <tt>FieldUpdating</tt> event is generated for a data access class (DAC) field
    ///   before the field is updated:</para>
    ///   <list type="bullet">
    ///     <item>For each DAC field value that is received
    ///     from the UI or through the Web Service
    ///     API when a data record is being inserted or updated.</item>
    ///     <item>For each DAC key field value in the process
    ///     of deleting a data record when the deletion is initiated
    ///     from the UI or through the Web Service API.</item>
    ///     <item>When any of the following methods of the <tt>PXCache</tt> class
    ///     initiates the assigning of the default value to a field:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li><li><tt>SetDefaultExt(object, string)</tt></li><li><tt>SetDefaultExt&lt;Field&gt;(object)</tt></li></ul></item>
    ///     <item>When any of the following methods of the <tt>PXCache</tt> class
    ///     initiates the updating of a field:
    ///         <ul><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>SetValueExt(object, string, object)</tt></li><li><tt>SetValueExt&lt;Field&gt;(object, object)</tt></li><li><tt>SetValuePending(object, string, object)</tt></li><li><tt>SetValuePending&lt;Field&gt;(object, object)</tt></li></ul></item>
    ///     <item>When the conversion of the external DAC key field presentation
    ///     to the internal field value is initiated by the following <tt>PXCache</tt> class methods:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>FieldUpdating</tt> event handler is used when either or both of the following occur:</para>
    ///   <list type="bullet">
    ///     <item>The external presentation of a DAC field (the value displayed in the UI) differs from the value stored in the DAC.</item>
    ///     <item>The storage of values is spread among multiple DAC fields (database columns).</item>
    ///   </list>
    ///   <para>In both cases, the application should implement both the <tt>FieldUpdating</tt> and
    ///   <tt>FieldSelecting</tt> events.</para>
    ///   <para>The following execution order is used for the <tt>FieldUpdating</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_FieldUpdating(
    ///     PXCache sender, 
    ///     PXFieldUpdatingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.FieldUpdating&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.FieldUpdating&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code spreads the external presentation of a field among multiple DAC fields by using the classic event handler approach." lang="CS">
    /// protected void Batch_ManualStatus_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
    /// {
    ///     Batch batch = (Batch)e.Row;
    ///     if (batch != null &amp;&amp; e.NewValue != null)
    ///     {
    ///         switch ((string)e.NewValue)
    ///         {
    ///             case "H":
    ///                 batch.Hold = true;
    ///                 batch.Released = false;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "B":
    ///                 batch.Hold = false;
    ///                 batch.Released = false;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "U":
    ///                 batch.Hold = false;
    ///                 batch.Released = true;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "P":
    ///                 batch.Hold = false;
    ///                 batch.Released = true;
    ///                 batch.Posted = true;
    ///                 break;
    ///         }
    ///    }
    /// }
    ///  
    /// protected void Batch_ManualStatus_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
    /// {
    ///     Batch batch = (Batch)e.Row;
    ///     if (batch != null)
    ///     {
    ///         if (batch.Hold == true)
    ///         {
    ///             e.ReturnValue = "H";
    ///         }
    ///         else if (batch.Released != true)
    ///         {
    ///             e.ReturnValue = "B";
    ///         }
    ///         else if (batch.Posted != true)
    ///         {
    ///             e.ReturnValue = "U";
    ///         }
    ///         else
    ///         {
    ///             e.ReturnValue = "P";
    ///         }
    ///     }
    /// }</code>
    ///   <code title="Example3b" description="The following code spreads the external presentation of a field among multiple DAC fields by using the classic event handler approach." lang="CS">
    /// protected void _(Events.FieldUpdating&lt;Batch.ManualStatus&gt; e)
    /// {
    ///     Batch batch = (Batch)e.Row;
    ///     if (batch != null &amp;&amp; e.NewValue != null)
    ///     {
    ///         switch ((string)e.NewValue)
    ///         {
    ///             case "H":
    ///                 batch.Hold = true;
    ///                 batch.Released = false;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "B":
    ///                 batch.Hold = false;
    ///                 batch.Released = false;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "U":
    ///                 batch.Hold = false;
    ///                 batch.Released = true;
    ///                 batch.Posted = false;
    ///                 break;
    ///             case "P":
    ///                 batch.Hold = false;
    ///                 batch.Released = true;
    ///                 batch.Posted = true;
    ///                 break;
    ///         }
    ///    }
    /// }
    ///  
    /// protected void _(Events.FieldSelecting&lt;Batch.ManualStatus&gt; e)
    /// {
    ///     Batch batch = (Batch)e.Row;
    ///     if (batch != null)
    ///     {
    ///         if (batch.Hold == true)
    ///         {
    ///             e.ReturnValue = "H";
    ///         }
    ///         else if (batch.Released != true)
    ///         {
    ///             e.ReturnValue = "B";
    ///         }
    ///         else if (batch.Posted != true)
    ///         {
    ///             e.ReturnValue = "U";
    ///         }
    ///         else
    ///         {
    ///             e.ReturnValue = "P";
    ///         }
    ///     }
    /// }</code>
    /// </example>
    public delegate void PXFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs args);

    /// <summary>Provides data for the <tt>FieldUpdating</tt> event.</summary>
    /// <seealso cref="PXFieldUpdating"/>
    public sealed class PXFieldUpdatingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private object _NewValue;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="newValue"></param>
        public PXFieldUpdatingEventArgs(object row, object newValue)
        {
            _Row = row;
            _NewValue = newValue;
        }
        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns or sets the internal DAC field value.</summary>
        public object NewValue
        {
            get
            {
                return _NewValue;
            }
            set
            {
                _NewValue = value;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>FieldVerifying</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXFieldVerifyingEventArgs">PXFieldVerifyingEventArgs</see>
    /// type that holds data for the <tt>PXFieldVerifying</tt> event.</param>
    /// <remarks>
    ///   <para>The system generates the <tt>FieldVerifying</tt> event for each data access class (DAC) field
    ///   of a data record that is inserted or updated in the
    /// <tt>PXCache</tt> object in the following processes:</para>
    ///   <list type="bullet">
    ///     <item>Insertion or update that is initiated in the UI
    ///     or through the Web Service API.</item>
    ///     <item>Assignment of the default value to the DAC field that is initiated by any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li><li><tt>SetDefaultExt(object, string)</tt></li><li><tt>SetDefaultExt&lt;Event&gt;(object)</tt></li></ul></item>
    ///     <item>A DAC field update that is initiated by any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Update(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>SetValueExt(object, string, object)</tt></li><li><tt>SetValueExt&lt;Field&gt;(object, object)</tt></li></ul></item>
    ///     <item>Validation of a DAC key field value when the validation is initiated by any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Locate(IDictionary)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>FieldVerifying</tt> event handler is used to perform the following actions:</para>
    ///   <list type="bullet">
    ///     <item>Implementation of the business logic associated with the validation of the DAC field value before the value is assigned to the DAC field.</item>
    ///     <item>Cancellation of the assigning of a value by throwing an exception of the <tt>PXSetPropertyException</tt>
    ///     type if the value does not meet the requirements.</item>
    ///     <item>Conversion of the external presentation of a DAC field value to the internal presentation
    ///     and implementation of the associated business logic. The internal presentation is the value stored in a DAC instance.</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>FieldVerifying</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_FieldVerifying(
    ///     PXCache sender, 
    ///     PXFieldVerifyingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.FieldVerifying&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.FieldVerifying&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code validates the new value of a DAC field by using the classic event handler approach." lang="CS">
    /// public class APPaymentEntry : APDataEntryGraph&lt;APPaymentEntry, APPayment&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void APPayment_AdjDate_FieldVerifying(
    ///         PXCache sender, 
    ///         PXFieldVerifyingEventArgs e)
    ///     {
    ///         if ((bool)((APPayment)e.Row).VoidAppl == false &amp;&amp; 
    ///             vendor.Current != null &amp;&amp; (bool)vendor.Current.Vendor1099)
    ///         {
    ///             string Year1099 = ((DateTime)e.NewValue).Year.ToString();
    ///             AP1099Year year = PXSelect&lt;
    ///                 AP1099Year, 
    ///                 Where&lt;AP1099Year.finYear, 
    ///                       Equal&lt;Required&lt;AP1099Year.finYear&gt;&gt;&gt;&gt;.
    ///                 Select(this, Year1099);
    ///             if (year != null &amp;&amp; year.Status != "N")
    ///                 throw new PXSetPropertyException(
    ///                     Messages.AP1099_PaymentDate_NotIn_OpenYear);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code validates the new value of a DAC field by using the generic event handler approach." lang="CS">
    /// public class APPaymentEntry : APDataEntryGraph&lt;APPaymentEntry, APPayment&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldVerifying&lt;APPayment.AdjDate&gt; e)
    ///     {
    ///         if ((bool)((APPayment)e.Row).VoidAppl == false &amp;&amp; 
    ///             vendor.Current != null &amp;&amp; (bool)vendor.Current.Vendor1099)
    ///         {
    ///             string Year1099 = ((DateTime)e.NewValue).Year.ToString();
    ///             AP1099Year year = PXSelect&lt;
    ///                 AP1099Year, 
    ///                 Where&lt;AP1099Year.finYear, 
    ///                       Equal&lt;Required&lt;AP1099Year.finYear&gt;&gt;&gt;&gt;.
    ///                 Select(this, Year1099);
    ///             if (year != null &amp;&amp; year.Status != "N")
    ///                 throw new PXSetPropertyException(
    ///                     Messages.AP1099_PaymentDate_NotIn_OpenYear);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to validate the external presentation of a DAC field value and convert it to the internal presentation if the validation succeeds." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class CAReconEnq : PXGraph&lt;CAReconEnq&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void CashAccountFilter_CashAccountID_FieldVerifying(
    ///         PXCache sender, 
    ///         PXFieldVerifyingEventArgs e)
    ///     {
    ///         CashAccountFilter createReconFilter = (CashAccountFilter)e.Row;
    ///         if (!e.NewValue is string) return;
    ///         CashAccount acct = 
    ///             PXSelect&lt;CashAccount, 
    ///                      Where&lt;CashAccount.accountCD, 
    ///                            Equal&lt;Required&lt;CashAccount.accountCD&gt;&gt;&gt;&gt;.
    ///                 Select(this, (string)e.NewValue);
    ///         if (acct != null &amp;&amp; acct.Reconcile != true)
    ///             throw new PXSetPropertyException(Messages.CashAccounNotReconcile);
    ///         e.NewValue = acct.AccountID;
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3b" description="The following code uses the generic event handler approach to validate the external presentation of a DAC field value and convert it to the internal presentation if the validation succeeds." lang="CS">
    /// [TableAndChartDashboardType]
    /// public class CAReconEnq : PXGraph&lt;CAReconEnq&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldVerifying&lt;CashAccountFilter.CashAccountID&gt; e)
    ///     {
    ///         CashAccountFilter createReconFilter = (CashAccountFilter)e.Row;
    ///         if (!e.NewValue is string) return;
    ///         CashAccount acct = 
    ///             PXSelect&lt;CashAccount, 
    ///                      Where&lt;CashAccount.accountCD, 
    ///                            Equal&lt;Required&lt;CashAccount.accountCD&gt;&gt;&gt;&gt;.
    ///                 Select(this, (string)e.NewValue);
    ///         if (acct != null &amp;&amp; acct.Reconcile != true)
    ///             throw new PXSetPropertyException(Messages.CashAccounNotReconcile);
    ///         e.NewValue = acct.AccountID;
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs args);

    /// <summary>Provides data for the <tt>FieldVerifying</tt> event.</summary>
    /// <seealso cref="PXFieldVerifying"/>
    public sealed class PXFieldVerifyingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private object _NewValue;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="newValue"></param>
        /// <param name="externalCall"></param>
        public PXFieldVerifyingEventArgs(object row, object newValue, bool externalCall)
        {
            _Row = row;
            _NewValue = newValue;
            _ExternalCall = externalCall;
        }
        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns or sets the new value of the current DAC field.</summary>
        public object NewValue
        {
            get
            {
                return _NewValue;
            }
            set
            {
                _NewValue = value;
            }
        }
        /// <summary>Returns <tt>true</tt> if the new value of the current DAC
        /// field has been received from the UI or through the Web Service
        /// API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <tt>FieldUpdated</tt> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXFieldUpdatedEventArgs">PXFieldUpdatedEventArgs</see>
    /// type that holds data for the <tt>FieldUpdated</tt> event.</param>
    /// <remarks>
    ///   <para>In the following cases, the <tt>FieldUpdated</tt> event is generated after a data access class (DAC) field is actually updated:</para>
    ///   <list type="bullet">
    ///     <item>For each DAC field value that is received
    ///     from the UI or through the Web Service API when a data record is
    ///     inserted or updated in the <tt>PXCache</tt> object.</item>
    ///     <item>For each DAC key field value in the process of deleting a data record
    ///     from the <tt>PXCache</tt> object when the deletion is initiated from the UI or
    ///     through the Web Service API.</item>
    ///     <item>When any of the following methods of the <tt>PXCache</tt> class
    ///     initiates the assigning of a default value to a field:
    ///         <ul><li><tt>Insert()</tt></li><li><tt>Insert(object)</tt></li><li><tt>Insert(IDictionary)</tt></li><li><tt>SetDefaultExt(object, string)</tt></li><li><tt>SetDefaultExt&lt;Event&gt;(object)</tt></li></ul></item>
    ///     <item>When the field update is initiated
    ///     by any of the following methods of the <tt>PXCache</tt> class:
    ///         <ul><li><tt>Update(object)</tt></li><li><tt>SetValueExt(object, string, object)</tt></li><li><tt>SetValueExt&lt;Field&gt;(object, object)</tt></li></ul></item>
    ///     <item>During the validation of the DAC key field value
    ///     that is initiated by any of the following <tt>PXCache</tt> class methods:
    ///         <ul><li><tt>Locate(IDictionary)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>Delete(IDictionary, IDictionary)</tt></li></ul></item>
    ///   </list>
    ///   <para>The <tt>FieldUpdated</tt> event handler is used to implement the business logic
    ///   related to the changes to the value of the DAC field in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>To update the related fields of a data record containing a modified field or assigning default values to these fields</item>
    ///     <item>To update any of the following:
    ///         <ul><li>The detail data records in a one-to-many relationship</li>
    ///         <li>The related data records in a one-to-one relationship</li>
    ///         <li>The master data records in a many-to-one relationship</li></ul></item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>FieldUpdated</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Attribute event handlers are executed.</item>
    ///     <item>Graph event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_FieldUpdated(
    ///     PXCache sender, 
    ///     PXFieldUpdatedEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.FieldUpdated&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.FieldUpdated&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to update the related field values of the current data record, assign them the default values, or perform both actions." lang="CS">
    /// public class APInvoiceEntry : APDataEntryGraph&lt;APInvoiceEntry,
    ///                               APInvoice&gt;, 
    ///                               PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void APTran_UOM_FieldUpdated(
    ///         PXCache sender, 
    ///         PXFieldUpdatedEventArgs e)
    ///     {
    ///         APTran tran = (APTran)e.Row;
    ///         sender.SetDefaultExt&lt;APTran.unitCost&gt;(tran);
    ///         sender.SetDefaultExt&lt;APTran.curyUnitCost&gt;(tran);
    ///         sender.SetValue&lt;APTran.unitCost&gt;(tran, null);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to update the related field values of the current data record, assign them the default values, or perform both actions." lang="CS">
    /// public class APInvoiceEntry : APDataEntryGraph&lt;APInvoiceEntry,
    ///                               APInvoice&gt;, 
    ///                               PXImportAttribute.IPXPrepareItems
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldUpdated&lt;APTran.UOM&gt; e)
    ///     {
    ///         APTran tran = (APTran)e.Row;
    ///         e.Cache.SetDefaultExt&lt;APTran.unitCost&gt;(tran);
    ///         e.Cache.SetDefaultExt&lt;APTran.curyUnitCost&gt;(tran);
    ///         e.Cache.SetValue&lt;APTran.unitCost&gt;(tran, null);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code updates the related data records by using the classic event handler approach." lang="CS">
    /// public class ARCashSaleEntry : ARDataEntryGraph&lt;ARCashSaleEntry,
    ///                                                 ARCashSale&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void ARCashSale_ProjectID_FieldUpdated(
    ///         PXCache sender, 
    ///         PXFieldUpdatedEventArgs e)
    ///     {
    ///         ARCashSale row = e.Row as ARCashSale;
    ///  
    ///         foreach (ARTran tran in Transactions.Select())
    ///             Transactions.Cache.SetDefaultExt&lt;ARTran.projectID&gt;(tran);
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code updates the related data records by using the generic event handler approach." lang="CS">
    /// public class ARCashSaleEntry : ARDataEntryGraph&lt;ARCashSaleEntry,
    ///                                                 ARCashSale&gt;
    /// {
    ///     ...
    ///     
    ///     protected virtual void _(Events.FieldUpdated&lt;ARCashSale.ProjectID&gt; e)
    ///     {
    ///         ARCashSale row = e.Row as ARCashSale;
    ///  
    ///         foreach (ARTran tran in Transactions.Select())
    ///             Transactions.Cache.SetDefaultExt&lt;ARTran.projectID&gt;(tran);
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs args);

    /// <summary>Provides data for the <tt>FieldUpdated</tt> event.</summary>
    /// <seealso cref="PXFieldUpdated"/>
    public sealed class PXFieldUpdatedEventArgs : EventArgs
    {
        private readonly object _Row;
        private readonly object _OldValue;
        private readonly bool _ExternalCall;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="oldValue"></param>
        /// <param name="externalCall"></param>
        public PXFieldUpdatedEventArgs(object row, object oldValue, bool externalCall)
        {
            _Row = row;
            _OldValue = oldValue;
            _ExternalCall = externalCall;
        }

        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns the previous value of the current DAC field.</summary>
        public object OldValue
        {
            get
            {
                return _OldValue;
            }
        }
        /// <summary>Returns <tt>true</tt> if the new value of the
        /// current DAC field has been changed in the UI or through the Web
        /// Service API; otherwise, it returns <tt>false</tt>.</summary>
        public bool ExternalCall
        {
            get
            {
                return _ExternalCall;
            }
        }
    }

    /// <summary>
    ///   <para>The delegate for the <code>ExceptionHandling</code> event.</para>
    /// </summary>
    /// <param name="sender">Required. The cache object that raised the event.</param>
    /// <param name="args">Required. The instance of the <see cref="PXExceptionHandlingEventArgs">PXExceptionHandlingEventArgs</see>
    /// type that holds data for the <tt>ExceptionHandling</tt> event.</param>
    /// <remarks>
    ///   <para>The <tt>ExceptionHandling</tt> event is generated in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>The <tt>PXSetPropertyException</tt> exception is thrown while the system is processing
    ///     a data access class (DAC) field value received from the UI or
    ///     through the Web Service API when a data record
    ///     is being inserted or updated in the <tt>PXCache</tt> object.</item>
    ///     <item>The <tt>PXSetPropertyException</tt> exception is thrown while the system is processing
    ///     DAC key field values when the deletion of a data record from the <tt>PXCache</tt> object
    ///     is initiated in the UI or through the Web Service API.</item>
    ///     <item>The <tt>PXSetPropertyException</tt> exception is thrown when the system is assigning the default value to a field
    ///     or updating the value when the assignment or update is initiated by any
    ///     of the following methods of the <tt>PXCache</tt> class:
    ///     <ul><li><tt>Insert(IDictionary)</tt></li><li><tt>SetDefaultExt(object, string)</tt></li><li><tt>SetDefaultExt&lt;Field&gt;(object)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>SetValueExt(object, string, object)</tt></li><li><tt>SetValueExt&lt;Field&gt;(object, object</tt></li></ul></item>
    ///     <item>The <tt>PXSetPropertyException</tt> exception is thrown while the system is converting
    ///     the external DAC key field presentation to the internal field value initiated by any of the following methods
    ///     of the <tt>PXCache</tt> class:
    ///     <ul><li><tt>Locate(IDictionary)</tt></li><li><tt>Update(IDictionary, IDictionary)</tt></li><li><tt>Delete(IDictionary, IDictionary)</tt></li></ul></item>
    ///     <item>The <tt>PXCommandPreparingException</tt>, <tt>PXRowPersistingException</tt>, or <tt>PXRowPersistedException</tt>
    ///     exception is thrown when an inserted, updated, or deleted data record is saved in the database.</item>
    ///   </list>
    ///   <para>The <code>ExceptionHandling</code> event handler is used to do the following:</para>
    ///   <list type="bullet">
    ///     <item>Catch and handle the exceptions mentioned above (the platform rethrows all unhandled exceptions)</item>
    ///     <item>Implement non-standard handling of the exceptions mentioned above</item>
    ///   </list>
    ///   <para>The following execution order is used for the <tt>ExceptionHandling</tt> event handlers:</para>
    ///   <list type="number">
    ///     <item>Graph event handlers are executed.</item>
    ///     <item>If <tt>e.Cancel</tt> is <tt>false</tt>, attribute event handlers are executed.</item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <code title="Example1a" description="According to the naming convention for graph event handlers in Acumatica Framework, the classic event handler must have the following signature." lang="CS">
    /// protected virtual void DACName_FieldName_ExceptionHandling(
    ///     PXCache sender, 
    ///     PXExceptionHandlingEventArgs e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1b" description="The generic event handler must have the following signature." lang="CS">
    /// protected virtual void _(Events.ExceptionHandling&lt;DACType, FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example1c" description="The generic event handler signature can contain only the field parameter; it can omit the DAC parameter if this field belongs to the DAC. Following is the signature with only one parameter." lang="CS">
    /// protected virtual void _(Events.ExceptionHandling&lt;FieldType&gt; e)
    /// {
    ///     ...
    /// }</code>
    ///   <code title="Example2a" description="The following code uses the classic event handler approach to handle an exception on a DAC field and set the field value." lang="CS">
    /// public class APVendorBalanceEnq : PXGraph&lt;APVendorBalanceEnq&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void APHistoryFilter_AccountID_ExceptionHandling(
    ///         PXCache sender, 
    ///         PXExceptionHandlingEventArgs e)
    ///     {
    ///         APHistoryFilter header = e.Row as APHistoryFilter;
    ///         if (header != null)
    ///         {
    ///             e.Cancel = true;
    ///             header.AccountID = null;
    ///         }
    ///     }
    ///  
    ///     ...
    /// }</code>
    ///   <code title="Example2b" description="The following code uses the generic event handler approach to handle an exception on a DAC field and set the field value." lang="CS">
    /// public class APVendorBalanceEnq : PXGraph&lt;APVendorBalanceEnq&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void _(Events.ExceptionHandling&lt;APHistoryFilter.AccountID&gt; e)
    ///     {
    ///         APHistoryFilter header = e.Row as APHistoryFilter;
    ///         if (header != null)
    ///         {
    ///             e.Cancel = true;
    ///             header.AccountID = null;
    ///         }
    ///     }
    ///  
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the classic event handler approach to alter an exception on a DAC field by setting its description." lang="CS">
    /// public class CustomerMaint : 
    ///     BusinessAccountGraphBase&lt;Customer, Customer, 
    ///                              Where&lt;BAccount.type,
    ///                                    Equal&lt;BAccountType.customerType&gt;,
    ///                                    Or&lt;BAccount.type, 
    ///                                       Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void Customer_CustomerClassID_ExceptionHandling(
    ///         PXCache sender, 
    ///         PXExceptionHandlingEventArgs e)
    ///     {
    ///         PXSetPropertyException ex = e.Exception as PXSetPropertyException;
    ///         if (ex != null)
    ///         {
    ///             ex.SetMessage(ex.Message + System.Environment.NewLine + 
    ///                           System.Environment.NewLine + 
    ///                           "Stack Trace:" + System.Environment.NewLine + 
    ///                           ex.StackTrace);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    ///   <code title="Example3a" description="The following code uses the generic event handler approach to alter an exception on a DAC field by setting its description." lang="CS">
    /// public class CustomerMaint : 
    ///     BusinessAccountGraphBase&lt;Customer, Customer, 
    ///                              Where&lt;BAccount.type,
    ///                                    Equal&lt;BAccountType.customerType&gt;,
    ///                                    Or&lt;BAccount.type, 
    ///                                       Equal&lt;BAccountType.combinedType&gt;&gt;&gt;&gt;
    /// {
    ///     ...
    ///  
    ///     protected virtual void _(Events.ExceptionHandling&lt;Customer.CustomerClassID&gt; e)
    ///     {
    ///         PXSetPropertyException ex = e.Exception as PXSetPropertyException;
    ///         if (ex != null)
    ///         {
    ///             ex.SetMessage(ex.Message + System.Environment.NewLine + 
    ///                           System.Environment.NewLine + 
    ///                           "Stack Trace:" + System.Environment.NewLine + 
    ///                           ex.StackTrace);
    ///         }
    ///     }
    ///     
    ///     ...
    /// }</code>
    /// </example>
    public delegate void PXExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs args);

    /// <summary>Provides data for the <tt>ExceptionHandling</tt> event.</summary>
    /// <seealso cref="PXExceptionHandling"/>
    public sealed class PXExceptionHandlingEventArgs : CancelEventArgs
    {
        private readonly object _Row;
        private object _NewValue;
        private readonly Exception _Exception;

        /// <summary>
        /// Initializes a new instance of the class with the provided values.
        /// </summary>
        /// <param name="row">The data record.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="exception">The exception instance.</param>
        public PXExceptionHandlingEventArgs(object row, object newValue, Exception exception)
        {
            _Row = row;
            _NewValue = newValue;
            _Exception = exception;
        }
        /// <summary>Returns the current DAC object.</summary>
        public object Row
        {
            get
            {
                return _Row;
            }
        }
        /// <summary>Returns or sets the values of the DAC field. By default, this property
        /// contains one of the following groups of values:<ul>
        /// <li>The values that are generated in the process of assigning the default value to a DAC field</li>
        /// <li>The values that are passed as new values when a field is updated</li>
        /// <li>The values that are entered in the UI or through the Web Service API</li>
        /// <li>The values that are received with the <tt>PXCommandPreparingException</tt>,
        /// <tt>PXRowPersistingException</tt>, or <tt>PXRowPersistedException</tt> exception</li>
        /// </ul></summary>
        public object NewValue
        {
            get
            {
                return _NewValue;
            }
            set
            {
                _NewValue = value;
            }
        }
        /// <summary>Returns the initial exception that caused the event to be generated.</summary>
        public Exception Exception
        {
            get
            {
                return _Exception;
            }
        }
    }
}
