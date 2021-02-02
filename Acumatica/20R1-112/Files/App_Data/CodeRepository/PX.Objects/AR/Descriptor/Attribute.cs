using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.BQLConstants;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CR;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using PX.Objects.EP;
using System.Collections;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.SM;
using PX.Common;

namespace PX.Objects.AR
{
	public class ARCommissionPeriodIDAttribute : PeriodIDAttribute, IPXFieldVerifyingSubscriber
	{
		protected int _SelAttrIndex = -1;

		public ARCommissionPeriodIDAttribute()
			:base(null, typeof(Search<ARSPCommissionPeriod.commnPeriodID>))
		{
			_Attributes.Add(new PXSelectorAttribute(typeof(Search<ARSPCommissionPeriod.commnPeriodID>)));
			_SelAttrIndex = _Attributes.Count - 1;
		}

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				if (_SelAttrIndex != -1)
					((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FormatPeriod((string)e.NewValue);
				throw;
			}
		}
		#endregion
	}

    /// <summary>
    /// Specialized for ARInvoice version of the InvoiceNbrAttribute.<br/>
    /// The main purpose of the attribute is poviding of the uniqueness of the RefNbr <br/>
    /// amoung  a set of  documents of the specifyed types (for example, each RefNbr of the ARInvoice <br/>
    /// the ARInvoices must be unique across all ARInvoices and AR Debit memos)<br/>
    /// This may be useful, if user has configured a manual numberin for ARInvoices  <br/>
    /// or needs  to create ARInvoice from another document (like SOOrder) allowing to type RefNbr <br/>
    /// for the to-be-created Invoice manually. To store the numbers, system using ARInvoiceNbr table, <br/>
    /// keyed uniquelly by DocType and RefNbr. A source document is linked to a number by NoteID.<br/>
    /// Attributes checks a number for uniqueness on FieldVerifying and RowPersisting events.<br/>
    /// </summary>
	public class ARInvoiceNbrAttribute : InvoiceNbrAttribute
	{
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected override Guid? GetNoteID(PXCache sender, PXRowPersistedEventArgs e)
		{
			Guid? val = (Guid?)sender.GetValue<ARInvoice.refNoteID>(e.Row);
			return (val != null) ? val : base.GetNoteID(sender, e);
		}

		protected override bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			Guid? val = (Guid?)sender.GetValue<ARInvoice.refNoteID>(e.Row);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete && val != null)
			{
				return false;
			}
			else
			{
				return base.DeleteOnUpdate(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			try
			{
				base.RowPersisted(sender, e);
			}
			catch (PXRowPersistedException)
			{
				if (e.Operation != PXDBOperation.Update || e.TranStatus != PXTranStatus.Open)
				{
					throw;
				}
			}
		}

		public ARInvoiceNbrAttribute()
			: base(typeof(ARInvoice.docType), typeof(ARInvoice.noteID))
		{
		}
	}

	public class InvoiceNbrAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber
	{
		protected Type _DocType;
		protected Type _NoteID;
		protected string[] _DocTypes;

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
			sender.Graph.Caches[typeof(Note)].IsDirty = false;
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Guid? RefNoteID = (Guid?)sender.GetValue(e.Row, _NoteID.Name);
			string DocType = GetDocType(sender, e.Row);

			if (string.IsNullOrEmpty((string)e.NewValue) == false)
			{
				ARInvoiceNbr dup = PXSelectReadonly<ARInvoiceNbr, Where<ARInvoiceNbr.docType, Equal<Required<ARInvoiceNbr.docType>>, And<ARInvoiceNbr.refNbr, Equal<Required<ARInvoiceNbr.refNbr>>, And<ARInvoiceNbr.refNoteID, NotEqual<Required<ARInvoiceNbr.refNoteID>>>>>>.Select(sender.Graph, DocType, e.NewValue, RefNoteID);
				if (dup != null)
				{
					throw new PXSetPropertyException(Messages.DuplicateInvoiceNbr);
				}
			}
		}

		protected virtual bool DeleteNumber(string DocType, string RefNbr, Guid? RefNoteID)
		{
			return PXDatabase.Delete<ARInvoiceNbr>(
				new PXDataFieldRestrict("DocType", DocType),
				new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, RefNbr, PXComp.NE),
				new PXDataFieldRestrict("RefNoteID", RefNoteID));
		}

		protected virtual bool SelectNumber(string DocType, string RefNbr, Guid? RefNoteID)
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<ARInvoiceNbr>(
				new PXDataField("RefNoteID"),
				new PXDataFieldValue("DocType", DocType),
				new PXDataFieldValue("RefNbr", RefNbr),
				new PXDataFieldValue("RefNoteID", RefNoteID)))
			{
				return record != null;
			}
		}

		protected virtual void InsertNumber(PXCache sender, string DocType, string RefNbr, Guid? RefNoteID)
		{
			ARInvoiceNbr record = new ARInvoiceNbr();
			PXCache cache = sender.Graph.Caches[typeof(ARInvoiceNbr)];

			List<PXDataFieldAssign> fields = new List<PXDataFieldAssign>();

			foreach (string field in cache.Fields)
			{
				object NewValue = null;
				switch (field)
				{
					case "DocType":
						NewValue = DocType;
						break;
					case "RefNbr":
						NewValue = RefNbr;
						break;
					case "RefNoteID":
						NewValue = RefNoteID;
						break;
					case "tstamp":
						continue;
					default:
						cache.RaiseFieldDefaulting(field, record, out NewValue);
						if (NewValue == null)
						{
							cache.RaiseRowInserting(record);
							NewValue = cache.GetValue(record, field);
						}
						break;
				}
				fields.Add(new PXDataFieldAssign(field, NewValue));
			}
			PXDatabase.Insert<ARInvoiceNbr>(fields.ToArray());
		}

		protected virtual bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete;
		}

		protected virtual Guid? GetNoteID(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (Guid?)sender.GetValue(e.Row, _NoteID.Name);
		}

		protected virtual string GetNumber(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (string)sender.GetValue(e.Row, _FieldName);
		}

		protected virtual string GetDocType(PXCache sender, object data)
		{
			string DocType = (string)sender.GetValue(data, _DocType.Name);

			switch (DocType)
			{
				case ARDocType.Invoice:
				case ARDocType.CashSale:
					return ARDocType.Invoice;
				case ARDocType.CreditMemo:
				case ARDocType.DebitMemo:
					return DocType;
				default:
					return null;
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Open)
			{
				Guid? RefNoteID = GetNoteID(sender, e);
				string DocType = GetDocType(sender, e.Row);
				string SetNumber = GetNumber(sender, e);
				bool Deleted = true;

				if (string.IsNullOrEmpty(DocType))
				{
					return;
				}

				//if (RefNoteID < 0L)
				//{
				//    throw new PXRowPersistedException(_FieldName, SetNumber, Messages.CannotSaveNotes);
				//}


				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				{
					if (DeleteOnUpdate(sender, e))
					{
						DeleteNumber(DocType, string.Empty, RefNoteID);
						SetNumber = string.Empty;
					}
					else
					{
						Deleted = DeleteNumber(DocType, SetNumber ?? string.Empty, RefNoteID);
					}
				}
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
				{
					if (string.IsNullOrEmpty((string)SetNumber) == false && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && Deleted || SelectNumber(DocType, SetNumber, RefNoteID) == false))
					{
						try
						{
							InsertNumber(sender, DocType, SetNumber, RefNoteID);
						}
						catch (PXDatabaseException)
						{
							throw new PXRowPersistedException(_FieldName, SetNumber, Messages.DuplicateInvoiceNbr);
						}
					}
				}
			}
		}

		public InvoiceNbrAttribute(Type DocType, Type NoteID)
		{
			_DocType = DocType;
			_NoteID = NoteID;
			_DocTypes = new ARDocType.SOEntryListAttribute().AllowedValues;
		}
	}

    /// <summary>
    /// Specialized for AR version of the Address attribute.<br/>
    /// Uses ARAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for ARAddress must be present in the graph <br/>
    /// Depends upon row instance. <br/>
    /// <example>
    ///[ARAddress(typeof(Select2<Customer,
    ///        InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>,
    ///         And<Location.locationID, Equal<Customer.defLocationID>>>,
    ///        InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>,
    ///         And<Address.addressID, Equal<Customer.defBillAddressID>>>,
    ///        LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>,
    ///         And<ARAddress.customerAddressID, Equal<Address.addressID>,
    ///         And<ARAddress.revisionID, Equal<Address.revisionID>,
    ///         And<ARAddress.isDefaultBillAddress, Equal<True>>>>>>>>,
    ///        Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
    /// </example>
    /// </summary>
	public class ARAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects ARAddress as a IAddress type.
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
        /// a source Address record from which AR address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
        public ARAddressAttribute(Type SelectType)
			: base(typeof(ARAddress.addressID), typeof(ARAddress.isDefaultBillAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<ARAddress, ARAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<ARAddress, ARAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<ARAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<ARAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class ARShippingAddressAttribute : AddressAttribute
	{
		public ARShippingAddressAttribute(Type SelectType)
			: base(typeof(ARShippingAddress.addressID), typeof(ARShippingAddress.isDefaultBillAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARShippingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<ARShippingAddress, ARShippingAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<ARShippingAddress, ARShippingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<ARShippingAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARShippingAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<ARShippingAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	/// <summary>
	/// Specialized for AR version of the Contact attribute.<br/>
	/// Uses ARContact tables for Contact versions storage <br/>
	/// Parameters ContactID, IsDefaultContact are assigned to the <br/>
	/// corresponded fields in the ARContact table. <br/>
	/// Cache for ARContact must be present in the graph <br/>
	/// Depends upon row instance.
	///<example>
	///[ARContact(typeof(Select2<Customer,
	///		InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>,
	///		    And<Location.locationID, Equal<Customer.defLocationID>>>,
	///		InnerJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
	///		    And<Contact.contactID, Equal<Customer.defBillContactID>>>,
	///		LeftJoin<ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>,
	///		    And<ARContact.customerContactID, Equal<Contact.contactID>,
	///		    And<ARContact.revisionID, Equal<Contact.revisionID>,
	///		    And<ARContact.isDefaultContact, Equal<True>>>>>>>>,
	///		Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
	///</example>
	///</summary>
	public class ARContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects ARContact as a IContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
        /// a source Contact record from which AR Contact is defaulted and for selecting version of ARContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>
		public ARContactAttribute(Type SelectType)
			: base(typeof(ARContact.contactID), typeof(ARContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<ARContact, ARContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<ARContact, ARContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<ARContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	public class ARShippingContactAttribute : ContactAttribute
	{
		public ARShippingContactAttribute(Type SelectType)
			: base(typeof(ARShippingContact.contactID), typeof(ARShippingContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARShippingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<ARShippingContact, ARShippingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<ARShippingContact, ARShippingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<ARShippingContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARShippingContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="AnyPeriodFilterableAttribute"/>.
	/// Displays any periods (active, closed, etc), maybe filtered.
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// Default columns list includes 'Active' and  'Closed in GL' and 'Closed in AP'  columns
	/// </summary>
	public class ARAnyPeriodFilterableAttribute : AnyPeriodFilterableAttribute
	{

		public ARAnyPeriodFilterableAttribute(Type aSearchType, Type aSourceType)
			: base(aSearchType, aSourceType)
		{

		}

		public ARAnyPeriodFilterableAttribute(Type aSourceType)
			: this(null, aSourceType)
		{

		}

		public ARAnyPeriodFilterableAttribute()
			: this(null)
		{

		}
	}

	public class ARAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				: base(AllowedValues, AllowedLabels)
			{
			}

		}
        /// <summary>
        /// Defines a list of the possible sources for the AR Documents sub-account defaulting: <br/>
        /// Namely: MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute()
                : base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson }, new string[] { !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation, Messages.MaskItem, Messages.MaskEmployee, Messages.MaskCompany, Messages.MaskSalesPerson })
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				_AllowedValues = new[] { MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson };
				_AllowedLabels = new[] { !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation, Messages.MaskItem, Messages.MaskEmployee, Messages.MaskCompany, Messages.MaskSalesPerson };
				_NeutralAllowedLabels = _AllowedLabels;

				base.CacheAttached(sender);
			}
		}
		public const string MaskLocation = "L";
		public const string MaskItem = "I";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
    [ARAcctSubDefault.ClassList]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private string _DimensionName = "SUBACCOUNT";
		private string _MaskName = "ARSETUP";
		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, ARAcctSubDefault.MaskLocation, new ARAcctSubDefault.ClassListAttribute().AllowedValues, new ARAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<ARAcctSubDefault.ClassListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (ARAcctSubDefault.ClassListAttribute)_Attributes.First(x => x.GetType() == typeof(ARAcctSubDefault.ClassListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new ARAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new ARAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

    /// <summary>
    /// Provides a UI Selector for Customers AcctCD as a string. <br/>
    /// Should be used where the definition of the AccountCD is needed - mostly, in a Customer DAC class.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "CUSTOMER".<br/>
    /// By default, search uses the following tables (linked) BAccount, Customer (strict), Contact, Address (optional).<br/>
    /// List of the Customers is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Customer.acctCD, Customer.acctName,<br/>
    ///	Customer.customerClassID, Customer.status, Contact.phone1, Address.city, Address.countryID, Contact.EMail<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example>
    /// [CustomerRaw(IsKey = true)]
    ///	</example>
    /// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
	public sealed class CustomerRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "CUSTOMER";
		public CustomerRawAttribute()
			: base()
		{

			Type SearchType = typeof(Search2<Customer.acctCD,
				LeftJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defContactID>>>,
				LeftJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defAddressID>>>>>,
				Where<Match<Current<AccessInfo.userName>>>>);
			_Attributes.Add(new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Customer.acctCD),
					typeof(Customer.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail)
				));
			_SelAttrIndex = _Attributes.Count - 1;
			this.DescriptionField = typeof(Customer.acctName);
			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
			this.Filterable = true;
		}
	}

	/// <summary>
	/// This is a specialized version of the Customer attribute.<br/>
	/// Displays only Active or OneTime customers<br/>
	/// See CustomerAttribute for detailed description. <br/>
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<Customer.status, IsNull,
                        Or<Customer.status, Equal<BAccount.status.active>,
                        Or<Customer.status, Equal<BAccount.status.oneTime>>>>), AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
	public class CustomerActiveAttribute : CustomerAttribute
	{
		public CustomerActiveAttribute(Type search, params Type[] fields)
			: base(search, fields)
		{
		}

		public CustomerActiveAttribute(Type search)
			:base(search)
		{
		}

		public CustomerActiveAttribute()
			: base ()
		{
		}
	}


    /// <summary>
    /// Provides a UI Selector for Customers.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "BIZACCT".<br/>
    /// By default, search uses the following tables (linked) BAccount, Customer (strict), Contact, Address (optional).<br/>
    /// List of the Vendors is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Customer.acctCD, Customer.acctName,<br/>
    ///	Customer.customerClassID, Customer.status, Contact.phone1, Address.city, Address.countryID<br/>
    ///	As most Dimention Selector - substitutes BAccountID by AcctCD.<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example>
    ///[Customer(typeof(Search<BAccountR.bAccountID,
    /// Where<Customer.smallBalanceAllow, Equal<True>,
    /// And<Where<Customer.status, Equal<BAccount.status.active>,
    /// Or<Customer.status, Equal<BAccount.status.oneTime>>>>>>),
    /// Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Customer.acctName))]
    ///	</example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
    [Serializable]
	public class CustomerAttribute : AcctSubAttribute
	{
		#region Override DACs

		[Serializable]
		[PXHidden]
		public partial class Location : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			protected Int32? _BAccountID;
			[PXDBInt(IsKey = true)]
			public virtual Int32? BAccountID
			{
				get
				{
					return this._BAccountID;
				}
				set
				{
					this._BAccountID = value;
				}
			}
			#endregion
			#region LocationID
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			protected Int32? _LocationID;
			[PXDBIdentity()]
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
			#region LocationCD
			public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
			protected String _LocationCD;
			[PXDBString(IsKey = true, IsUnicode = true)]
			public virtual String LocationCD
			{
				get
				{
					return this._LocationCD;
				}
				set
				{
					this._LocationCD = value;
				}
			}
			#endregion
			#region TaxRegistrationID
			public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
			protected String _TaxRegistrationID;
			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Registration ID")]
			public virtual String TaxRegistrationID
			{
				get
				{
					return this._TaxRegistrationID;
				}
				set
				{
					this._TaxRegistrationID = value;
				}
			}
			#endregion
			#region VShipTermsID
			public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Shipping Terms")]
			public virtual String VShipTermsID { get; set; }
			#endregion
			#region VLeadTime
            public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
            protected Int16? _VLeadTime;
            [PXDBShort(MinValue = 0, MaxValue = 100000)]
            [PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
            public virtual Int16? VLeadTime
            {
                get
                {
                    return this._VLeadTime;
                }
                set
                {
                    this._VLeadTime = value;
                }
            }
            #endregion
            #region VCarrierID
            public abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }
            protected String _VCarrierID;
            [PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
            [PXUIField(DisplayName = "Ship Via")]
            public virtual String VCarrierID
            {
                get
                {
                    return this._VCarrierID;
                }
                set
                {
                    this._VCarrierID = value;
                }
            }
            #endregion
		}

        [Serializable]
		[PXHidden(ServiceVisible = true)]
		public class Contact : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

			[PXDBInt]
			[CRContactBAccountDefault]
			public virtual Int32? BAccountID { get; set; }
			#endregion

			#region ContactID
			public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

			[PXDBIdentity(IsKey = true)]
			[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
			public virtual Int32? ContactID { get; set; }
			#endregion

			#region RevisionID
			public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

			[PXDBInt]
			[PXDefault(0)]
			[AddressRevisionID()]
			public virtual Int32? RevisionID { get; set; }
			#endregion

			#region Title
			public abstract class title : PX.Data.BQL.BqlString.Field<title> { }

			[PXDBString(50, IsUnicode = true)]
			[Titles]
			[PXUIField(DisplayName = "Title")]
			public virtual String Title { get; set; }
			#endregion

			#region FirstName
			public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "First Name")]
			public virtual String FirstName { get; set; }
			#endregion

			#region MidName
			public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }

			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Middle Name")]
			public virtual String MidName { get; set; }
			#endregion

			#region LastName
			public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

			[PXDBString(100, IsUnicode = true)]
			[PXUIField(DisplayName = "Last Name")]
			[CRLastNameDefault]
			public virtual String LastName { get; set; }
			#endregion

			#region Salutation

			public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Job Title")]
			[PXPersonalDataField]
			public virtual String Salutation { get; set; }
			#endregion

			#region Attention

			public abstract class attention : PX.Data.IBqlField { }

			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
			[PXPersonalDataField]
			public virtual String Attention { get; set; }
			#endregion

			#region Phone1
			public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
			[PhoneValidation()]
			public virtual String Phone1 { get; set; }
			#endregion

			#region Phone1Type
			public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 1")]
			[PhoneTypes]
			public virtual String Phone1Type { get; set; }
			#endregion

			#region Phone2
			public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 2")]
			[PhoneValidation()]
			public virtual String Phone2 { get; set; }
			#endregion

			#region Phone2Type
			public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 2")]
			[PhoneTypes]
			public virtual String Phone2Type { get; set; }
			#endregion

			#region Phone3
			public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 3")]
			[PhoneValidation()]
			public virtual String Phone3 { get; set; }
			#endregion

			#region Phone3Type
			public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 3")]
			[PhoneTypes]
			public virtual String Phone3Type { get; set; }
			#endregion

			#region WebSite
			public abstract class webSite : PX.Data.BQL.BqlString.Field<webSite> { }

			[PXDBWeblink]
			[PXUIField(DisplayName = "Web")]
			public virtual String WebSite { get; set; }
			#endregion

			#region Fax
			public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Fax")]
			[PhoneValidation()]
			public virtual String Fax { get; set; }
			#endregion

			#region EMail
			public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }

			[PXDBEmail]
			[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String EMail { get; set; }
			#endregion
		}
		#endregion

		public const string DimensionName = "BIZACCT";
        /// <summary>
        /// Default Ctor
        /// </summary>
		public CustomerAttribute()
			: this(typeof(Search<BAccountR.bAccountID>))
		{
		}
        /// <summary>
        /// Extended Ctor. User may define customised search.
        /// </summary>
        /// <param name="search">Must Be IBqlSearch, which returns BAccountID. Tables Customer,Contact, Address will be added automatically
        /// </param>
		public CustomerAttribute(Type search)
			: this(search,
					typeof(Customer.acctCD), typeof(Customer.acctName),
					typeof(Address.addressLine1), typeof(Address.addressLine2), typeof(Address.postalCode),
					typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID),
					typeof(Location.taxRegistrationID), typeof(Customer.curyID),
					typeof(Contact.attention),
					typeof(Customer.customerClassID), typeof(Customer.status))
		{
		}

        /// <summary>
        /// Extended ctor - full. User may define a select and the list of the fields.
        /// </summary>
        /// </summary>
        /// <param name="search">Must be IBqlSearch, which returns BAccountID. Tables Customer,Contact, Address will be added automatically </param>
	    /// <param name="fields">Msut be a list of IBqlFields from the tables, used in the search</param>
		public CustomerAttribute(Type search, params Type[] fields)
			: base()
		{
			Type searchType = search.GetGenericTypeDefinition();
			Type[] searchArgs = search.GetGenericArguments();

			Type cmd;
			if (searchType == typeof(Search<>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								typeof(Where<Customer.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search<,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1]);
			}
			else if (searchType == typeof(Search<,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2]);
			}
			else if (searchType == typeof(Search2<,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								typeof(Where<Customer.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search2<,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2]);
			}
			else if (searchType == typeof(Search2<,,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2],
								searchArgs[3]);
			}
			else
			{
				throw new PXArgumentException("search", ErrorMessages.ArgumentException);
			}

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, cmd, typeof(BAccountR.acctCD),
				typeof(BAccountR.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
			));
			attr.DescriptionField = typeof(Customer.acctName);
			attr.CacheGlobal = true;
	        attr.FilterEntity = typeof (Customer);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
			_fields = fields;
		}

		private readonly Type[] _fields;
		protected string[] _HeaderList = null;
		protected string[] _FieldList = null;

		public override void CacheAttached(PXCache sender)
		{
			//should go before standard code because of anonymous delegate in PXSelectorAttribute
			EmitColumnForCustomerField(sender);

			base.CacheAttached(sender);

			string name = _FieldName.ToLower();
			sender.Graph.FieldSelecting.RemoveHandler(sender.GetItemType(), name, GetAttribute<PXDimensionSelectorAttribute>().FieldSelecting);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), name, FieldSelecting);

		}

		protected virtual void PopulateFields(PXCache sender)
		{
			if (_FieldList == null)
			{
				_FieldList = new string[this._fields.Length];
				var _CacheList = new Type[this._fields.Length];

			    for (int i = 0; i < this._fields.Length; i++)
			    {
				    Type cacheType = BqlCommand.GetItemType(_fields[i]);
					_CacheList[i] = cacheType;
			        sender.Graph.Caches.AddCacheMapping(cacheType, cacheType);

					if (cacheType.IsAssignableFrom(typeof(BAccountR)) ||
					    _fields[i].Name == typeof(BAccountR.acctCD).Name ||
					    _fields[i].Name == typeof(BAccountR.acctName).Name)
				    {
						    _FieldList[i] = _fields[i].Name;
				    }
				    else
				    {
						    _FieldList[i] = cacheType.Name + "__" + _fields[i].Name;
				    }
			    }

				bool b = sender.Graph.Prototype.TryGetValue(out _HeaderList, "PopulateFields", _FieldList.GetHashCodeOfSequence(), sender.GetItemType());
				if (!b)
				{
					_HeaderList = new string[this._fields.Length];
					for (int i = 0; i < _FieldList.Length; i++)
					{
						PXCache cache = sender.Graph.Caches[_CacheList[i]];
						_HeaderList[i] = PXUIFieldAttribute.GetDisplayName(cache, _fields[i].Name);
					}
					sender.Graph.Prototype.SetValue(_HeaderList, "PopulateFields", _FieldList.GetHashCodeOfSequence(), sender.GetItemType());
				}
			}

			var attr = GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>();
			attr.SetColumns(_FieldList, _HeaderList);
		  
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (this.AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				PopulateFields(sender);
			}

			PXFieldSelecting handler = GetAttribute<PXDimensionSelectorAttribute>().FieldSelecting;
			handler(sender, e);
		}

		protected void EmitColumnForCustomerField(PXCache sender)
		{
			if (this.DescriptionField  == null)
				return;

			{
				string alias = _FieldName + "_" + typeof(Customer).Name + "_" + DescriptionField.Name;
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.Graph.FieldSelecting.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) =>
						 {
							 PopulateFields(s);
							 GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldSelecting(s, e, alias);
						 });

					sender.Graph.CommandPreparing.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) => GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldCommandPreparing(s, e));
				}
			}
			{
				string alias = _FieldName + "_description";
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.Graph.FieldSelecting.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) =>
						 {
							 PopulateFields(s);
							 GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldSelecting(s, e, alias);
						 });

					sender.Graph.CommandPreparing.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) => GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldCommandPreparing(s, e));
				}
			}
		}
	}


    [PXDBString(15, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "SalesPerson ID", Visibility = PXUIVisibility.Visible)]
	public sealed class SalesPersonRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "SALESPER";
		public SalesPersonRawAttribute()
			: base()
		{

			Type SearchType = typeof(Search<SalesPerson.salesPersonCD, Where<True, Equal<True>>>);
			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
					typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
				));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
		}
	}

    /// <summary>
    /// Provides UI selector of the Salespersons. <br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "SALESPER".<br/>
    ///	As most Dimention Selector - substitutes SalesPersonID by SalesPersonCD.<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Salesperson ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<SalesPerson.isActive, Equal<True>>), Messages.SalesPersonIsInactive)]
	public class SalesPersonAttribute : AcctSubAttribute
	{
		public const string DimensionName = "SALESPER";

        /// <summary>
        /// Default ctor. Shows all the salespersons
        /// </summary>
		public SalesPersonAttribute()
            : this(typeof(Where<True, Equal<True>>))
		{
		}
        /// <summary>
        /// Extended ctor. User can provide addtional where clause
        /// </summary>
        /// <param name="WhereType">Must be IBqlWhere type. Additional Where Clause</param>
		public SalesPersonAttribute(Type WhereType)
			: base()
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(SalesPerson.salesPersonID),
				/*typeof(LeftJoin<Contact, On<Contact.bAccountID, Equal<SalesPerson.bAccountID>, And<Contact.contactID, Equal<SalesPerson.contactID>>>,
							LeftJoin<Address, On<Address.bAccountID, Equal<SalesPerson.bAccountID>, And<Address.addressID, Equal<Contact.defAddressID>>>>>),*/
				WhereType);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
				typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
			));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;
            attr.CacheGlobal = true;
		}

        /// <summary>
        /// Extended ctor, full form. User can provide addtional Where and Join clause.
        /// </summary>
        /// <param name="WhereType">Must be IBqlWhere type. Additional Where Clause</param>
        /// <param name="JoinType">Must be IBqlJoin type. Defines Join Clause</param>
		public SalesPersonAttribute(Type WhereType, Type JoinType)
			: base()
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(SalesPerson.salesPersonID),
				JoinType,
				WhereType);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
				typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
			));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;
            attr.CacheGlobal = true;
        }

		public override bool DirtyRead
		{
			get
			{
				return (_SelAttrIndex == -1) ? false : ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead;
			}
			set
			{
				if (_SelAttrIndex != -1)
					((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead = value;
			}
		}
	}

	public class ARTaxAttribute : TaxAttribute
	{
		protected abstract class signedCuryTranAmt : PX.Data.BQL.BqlDecimal.Field<signedCuryTranAmt> { }

		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}

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
		#region PaymentsByLinesAllowed
		protected abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }
		protected Type PaymentsByLinesAllowed = typeof(paymentsByLinesAllowed);
		protected string _PaymentsByLinesAllowed
		{
			get
			{
				return PaymentsByLinesAllowed.Name;
			}
		}
		#endregion
		#region RetainageApply
		protected abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		protected Type RetainageApply = typeof(retainageApply);
		protected string _RetainageApply
		{
			get
			{
				return RetainageApply.Name;
			}
		}
		#endregion
		#region IsRetainageDocument
		protected abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
		protected Type IsRetainageDocument = typeof(isRetainageDocument);
		protected string _IsRetainageDocument
		{
			get
			{
				return IsRetainageDocument.Name;
			}
		}
		#endregion

		public ARTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType = null, Type parentBranchIDField = null)
			: this(ParentType, TaxType, TaxSumType, null, parentBranchIDField)
		{
		}

		public ARTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type TaxCalculationMode, Type parentBranchIDField)
			: base(ParentType, TaxType, TaxSumType, TaxCalculationMode, parentBranchIDField)
		{
			CuryTranAmt = typeof(ARTran.curyTranAmt);
			GroupDiscountRate = typeof(ARTran.groupDiscountRate);
			CuryLineTotal = typeof(ARInvoice.curyLineTotal);
			CuryDiscTot = typeof(ARInvoice.curyDiscTot);
			TaxCalcMode = typeof(ARInvoice.taxCalcMode);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARInvoice.curyLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<
			Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>,
				And<Where<ARTran.tranType, Equal<ARDocType.creditMemo>, Or<ARTran.tranType, Equal<ARDocType.cashReturn>>>>>>>,
					Minus<Sub<Sub<ARTran.curyTranAmt, Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.origGroupDiscountRate>, ARTran.origDocumentDiscountRate>>>,
						  Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>, ARTran.documentDiscountRate>>>>,
			Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>,
						  Sub<Sub<ARTran.curyTranAmt, Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.origGroupDiscountRate>, ARTran.origDocumentDiscountRate>>>,
						  Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>, ARTran.documentDiscountRate>>>>>,	
			decimal0>),
			typeof(SumCalc<ARSalesPerTran.curyCommnblAmt>)));
		}

		public override int CompareTo(object other)
		{
			return SortOrder.CompareTo(((ARTaxAttribute)other).SortOrder);
		}

		public override object Insert(PXCache cache, object item)
		{
			PXResultset<ARTax> currentDocumentTaxLinesRecords = PXSelect<ARTax,
				Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
					And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
					.Select(cache.Graph);

			List<object> taxLinesList = new List<object>(currentDocumentTaxLinesRecords.RowCast<ARTax>());

			PXRowInserted taxInsertedHandler = delegate (PXCache sender, PXRowInsertedEventArgs e)
			{
				taxLinesList.Add(e.Row);

				PXCache arRegisterCache = cache.Graph.Caches[typeof(ARRegister)];

				object taxTranType = arRegisterCache.GetValue<ARRegister.docType>(arRegisterCache.Current);
				object taxRefNbr = arRegisterCache.GetValue<ARRegister.refNbr>(arRegisterCache.Current);

				PXSelect<ARTax,
					Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
						And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
						.StoreCached(
							cache.Graph,
							new PXCommandKey(new object[] { taxTranType, taxRefNbr }),
							taxLinesList);
			};

			cache.Graph.RowInserted.AddHandler<ARTax>(taxInsertedHandler);

			try
			{
				return base.Insert(cache, item);
			}
			finally
			{
				cache.Graph.RowInserted.RemoveHandler<ARTax>(taxInsertedHandler);
			}
		}

		public override object Delete(PXCache cache, object item)
		{
			PXResultset<ARTax> currentDocumentTaxLinesRecords = PXSelect<ARTax,
				Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
					And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
					.Select(cache.Graph);

			List<object> taxLinesList = new List<object>(currentDocumentTaxLinesRecords.RowCast<ARTax>());

			PXRowDeleted taxDeletedHandler = delegate (PXCache sender, PXRowDeletedEventArgs e)
			{
				taxLinesList.Remove(e.Row);

				PXCache arRegisterCache = cache.Graph.Caches[typeof(ARRegister)];

				object taxTranType = arRegisterCache.GetValue<ARRegister.docType>(arRegisterCache.Current);
				object taxRefNbr = arRegisterCache.GetValue<ARRegister.refNbr>(arRegisterCache.Current);

				PXSelect<ARTax,
					Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
						And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
						.StoreCached(
							cache.Graph,
							new PXCommandKey(new object[] { taxTranType, taxRefNbr }),
							taxLinesList);
			};

			cache.Graph.RowDeleted.AddHandler<ARTax>(taxDeletedHandler);

			try
			{
				return base.Delete(cache, item);
			}
			finally
			{
				cache.Graph.RowDeleted.RemoveHandler<ARTax>(taxDeletedHandler);
			}
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			decimal? value = GetDocLineFinalAmtNoRounding(sender, row, TaxCalcType);
			return PXDBCurrencyAttribute.Round(sender, row, (decimal)value, CMPrecision.TRANCURY);
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType)
		{
			// Normally, CuryTranAmt is reduced by CuryRetainageAmt for each ARTran line.
			// In case when "Retain Taxes" flag is disabled in ARSetup - we should calculate
			// taxable amount based on the full CuryTranAmt amount, this why we should add 
			// CuryRetainageAmt to the CuryTranAmt value.
			//

			decimal curyRetainageAmt = IsRetainedTaxes(sender.Graph)
				? 0m
				: (decimal)(sender.GetValue(row, _CuryRetainageAmt) ?? 0m);

			decimal curyTranAmt = (base.GetCuryTranAmt(sender, row) ?? 0m);

			decimal combinedCuryTranAmt = curyTranAmt + curyRetainageAmt;

			decimal? value = combinedCuryTranAmt -
				(combinedCuryTranAmt - (combinedCuryTranAmt *
					(decimal?)sender.GetValue(row, _OrigGroupDiscountRate) *
					(decimal?)sender.GetValue(row, _OrigDocumentDiscountRate))) - 
				(combinedCuryTranAmt - (combinedCuryTranAmt *
					(decimal?)sender.GetValue(row, _GroupDiscountRate) *
					(decimal?)sender.GetValue(row, _DocumentDiscountRate)));

			return (decimal)value;
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARRegister document = e.Row as ARRegister;
			if (document == null) return;

			// Taxes for retainage invoice should not be recalculated
			// if date updated (See AC-135288 for details).
			// 
			bool isApplyRetainageCreditMemo = document.RetainageApply == true && document.DocType == ARDocType.CreditMemo;
			if (!isApplyRetainageCreditMemo && !document.IsChildRetainageDocument())
			{
				base.DateUpdated(sender, e);
			}
		}

		protected override void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<ARTran.curyTaxableAmt>(row, value);
		}

		protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<ARTran.curyTaxAmt>(row, value);
		}

		protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
		{
			ARTran row = child as ARTran;
			if (row != null)
			{
				row.CuryExtPrice = value;
				sender.Update(row);
			}
		}

		protected override string GetExtCostLabel(PXCache sender, object row)
		{
			return ((PXDecimalState)sender.GetValueExt<ARTran.curyExtPrice>(row)).DisplayName;
		}

		protected override bool AskRecalculate(PXCache sender, PXCache detailCache, object detail)
		{
			// We shouldn't ask recalculation if retainage feature is activated
			// because it is not possible to calculate correct taxable and 
			// tax amounts in this case.
			//
			return
				!PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				base.AskRecalculate(sender, detailCache, detail);
		}

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(ARSetup)];
			ARSetup arsetup = cache.Current as ARSetup;
			ARRegister document = ParentRow(graph) as ARRegister;

			return
				PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				document?.RetainageApply == true &&
				arsetup?.RetainTaxes == true;
		}

		protected virtual bool IsRoundingNeeded(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(ARSetup)];

			PXCache currencyInfoCache = graph.Caches[typeof(CurrencyInfo)];
			Currency currency = null;

			if (currencyInfoCache.Current != null)
			{
				currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(graph, (currencyInfoCache.Current as CurrencyInfo).CuryID);
			}

			bool allowRounding = false;

			if ((bool?)ParentGetValue(graph, _PaymentsByLinesAllowed) == true
			|| (bool?)ParentGetValue(graph, _RetainageApply) == true
			|| (bool?)ParentGetValue(graph, _IsRetainageDocument) == true)
			{
				allowRounding = false;
			}
			else if (currency?.UseARPreferencesSettings == false)
			{
				allowRounding = currency.ARInvoiceRounding != RoundingType.Currency;
			}
			else
			{
				allowRounding = ((ARSetup)cache.Current).InvoiceRounding != RoundingType.Currency;
			}

			return allowRounding;
		}

		protected virtual decimal? RoundAmount(PXGraph graph, decimal? value)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>())
			{
				return value;
			}

			PXCache currencyInfoCache = graph.Caches[typeof(CurrencyInfo)];
			Currency currency = null;

			if (currencyInfoCache.Current != null)
			{
				currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(graph, (currencyInfoCache.Current as CurrencyInfo).CuryID);
			}

			if (currency?.UseARPreferencesSettings == false)
			{
				value = ARReleaseProcess.RoundAmount(value, currency.ARInvoiceRounding, currency.ARInvoicePrecision);
			}
			else
			{
				PXCache setupCache = graph.Caches[typeof(ARSetup)];
				var setup = (ARSetup)setupCache.Current;
				value = ARReleaseProcess.RoundAmount(value, setup.InvoiceRounding, setup.InvoicePrecision);
			}

			return value;
		}

		protected virtual void ResetRoundingDiff(PXGraph graph)
		{
			base.ParentSetValue(graph, typeof(ARRegister.curyRoundDiff).Name, 0m);
			base.ParentSetValue(graph, typeof(ARRegister.roundDiff).Name, 0m);
		}

		protected override void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache parentCache = ParentCache(graph);

			if (parentCache.Current == null)
			{
				return;
			}

			PXCache cache = graph.Caches[typeof(ARSetup)];

			if (fieldname == _CuryDocBal && cache.Current != null && IsRoundingNeeded(graph))
			{
				decimal? oldval = (decimal?)value;
				value = RoundAmount(graph, (decimal?)value);

				decimal oldbaseval;
				decimal baseval;

				graph.Caches[typeof(CurrencyInfo)].ClearQueryCacheObsolete();
				PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)oldval, out oldbaseval);
				PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)value, out baseval);

				oldbaseval -= baseval;
				oldval -= (decimal?)value;

				base.ParentSetValue(graph, typeof(ARRegister.curyRoundDiff).Name, oldval);
				base.ParentSetValue(graph, typeof(ARRegister.roundDiff).Name, oldbaseval);
			}
			else
			{
				ResetRoundingDiff(graph);
			}

			base.ParentSetValue(graph, fieldname, value);
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
			decimal CuryLineTotal = (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
			decimal CuryDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);

			if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
			{
				if (Math.Abs(CuryTaxableAmt - CuryLineTotal) < 0.00005m)
				{
					CuryTaxableAmt -= CuryDiscountTotal;
				}
			}
		}

		public override IEnumerable<ITaxDetail> MatchesCategory(PXCache sender, object row, IEnumerable<ITaxDetail> zonetaxlist)
		{
			string taxcat = GetTaxCategory(sender, row);

			List<ITaxDetail> ret = new List<ITaxDetail>();

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return ret;
			}

			HashSet<string> cattaxlist;
			ARInvoiceEntry invoiceEntry = sender.Graph as ARInvoiceEntry;
			if (invoiceEntry != null)
			{
				if (!invoiceEntry.TaxesByTaxCategory.TryGetValue(cat.TaxCategoryID, out cattaxlist))
				{
					cattaxlist = new HashSet<string>();
					foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat))
					{
						cattaxlist.Add(detail.TaxID);
					}

					invoiceEntry.TaxesByTaxCategory.Add(cat.TaxCategoryID, cattaxlist);
				}
			}
			else
			{
				cattaxlist = new HashSet<string>();
				foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat))
				{
					cattaxlist.Add(detail.TaxID);
				}
			}

			foreach (ITaxDetail zoneitem in zonetaxlist)
			{
				bool zonematchestaxcat = cattaxlist.Contains(zoneitem.TaxID);
				if (cat.TaxCatFlag == false && zonematchestaxcat || cat.TaxCatFlag == true && !zonematchestaxcat)
				{
					ret.Add(zoneitem);
				}
			}
			return ret;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					int? linenbr = int.MinValue;

					if (row != null && row.GetType() == typeof(ARTran))
					{
						linenbr = (int?)graph.Caches[typeof(ARTran)].GetValue<ARTran.lineNbr>(row);
					}

					foreach (ARTax record in PXSelect<ARTax,
						Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
							And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						if (record.LineNbr == linenbr)
						{
							AppendTail<ARTax, Where>(graph, ret, record, row, parameters);
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (ARTax record in PXSelect<ARTax,
						Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
							And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						AppendTail<ARTax, Where>(graph, ret, record, row, parameters);
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (ARTaxTran record in PXSelect<ARTaxTran,
						Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
							And<ARTaxTran.tranType, Equal<Current<ARRegister.docType>>,
							And<ARTaxTran.refNbr, Equal<Current<ARRegister.refNbr>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						AppendTail<ARTaxTran, Where>(graph, ret, record, row, parameters);
					}
					return ret;
				default:
					return ret;
			}
		}

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<ARTran,
								Where<ARTran.tranType, Equal<Current<ARRegister.docType>>,
									And<ARTran.refNbr, Equal<Current<ARRegister.refNbr>>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<ARTran>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}

		protected virtual void AppendTail<T, W>(PXGraph graph, List<object> ret, T record, object row, params object[] parameters) where T : class, ITaxDetail, PX.Data.IBqlTable, new()
			where W : IBqlWhere, new()
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			PXSelectBase<Tax> select = new PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
				And<TaxRev.outdated, Equal<False>,
				And<TaxRev.taxType, Equal<TaxType.sales>,
				And<Tax.taxType, NotEqual<CSTaxType.withholding>,
				And<Tax.taxType, NotEqual<CSTaxType.use>,
				And<Tax.reverseTax, Equal<False>,
				And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where2<Where<Tax.taxID, Equal<Required<Tax.taxID>>>, And<W>>>(graph);

			List<object> newParams = new List<object>();
			newParams.Add(this.GetDocDate(ParentCache(graph), row));
			newParams.Add(record.TaxID);

			if (parameters != null)
			{
				newParams.AddRange(parameters);
			}

			foreach (PXResult<Tax, TaxRev> line in select.View.SelectMultiBound(new object[] { row }, newParams.ToArray()))
			{
				int idx;
				for (idx = ret.Count;
					(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<T, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;

				Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);

				ret.Insert(idx, new PXResult<T, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
			}
		}

		protected override void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);
			decimal CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (!Equals(CuryTaxTotal, doc_CuryTaxTotal))
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
			}
		}
	}

    /// <summary>
    /// Specialized for the ARPayment  version of the <see cref="CashTranIDAttribute"/><br/>
    /// Since CATran created from the source row, it may be used only the fields <br/>
    /// of ARPayment compatible DAC. <br/>
    /// The main purpuse of the attribute - to create CATran <br/>
    /// for the source row and provide CATran and source synchronization on persisting.<br/>
    /// CATran cache must exists in the calling Graph.<br/>
    /// </summary>
	public class ARCashTranIDAttribute : CashTranIDAttribute
	{
		[Obsolete(Common.InternalMessages.ConstructorIsObsoleteAndWillBeRemoved2018R2)]
		public ARCashTranIDAttribute(Type isMigrationModeEnabledSetupField)
			: base()
		{ }

		public ARCashTranIDAttribute()
			: base()
		{ }

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return ARCashTranIDAttribute.DefaultValues(sender, catran_Row, (ARPayment)orig_Row);
		}

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, ARPayment orig_Row)
		{
			ARPayment parentDoc = (ARPayment)orig_Row;
			if (parentDoc.DocType == ARDocType.CreditMemo ||
				parentDoc.DocType == ARDocType.SmallBalanceWO ||
				(parentDoc.Released == true) && (catran_Row.TranID != null) ||
				 parentDoc.CuryOrigDocAmt == null ||
				 parentDoc.IsMigratedRecord == true ||
				 parentDoc.CuryOrigDocAmt == 0m || (parentDoc.Released == false && parentDoc.Voided == true))
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryID = parentDoc.CuryID;
			
            catran_Row.CuryTranAmt = (ARPaymentType.DrCr(parentDoc.DocType)==DrCr.Credit ? -1 : 1) * parentDoc.CuryOrigDocAmt;
			if (catran_Row.CuryTranAmt < 0)
			{
				catran_Row.DrCr = DrCr.Credit;
			}
			else
			{
					catran_Row.DrCr = DrCr.Debit;
			}


            string[] voidedTypes = ARPaymentType.GetVoidedARDocType(parentDoc.DocType);

			catran_Row.TranDate = parentDoc.DocDate;
			catran_Row.TranDesc = parentDoc.DocDesc;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.ReferenceID = parentDoc.CustomerID;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
			catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;
			//This coping is required in one specfic case - when payment reclassification is made
			if (parentDoc.CARefTranID.HasValue)
			{
				catran_Row.RefTranAccountID = parentDoc.CARefTranAccountID;
				catran_Row.RefTranID = parentDoc.CARefTranID;
                catran_Row.RefSplitLineNbr = parentDoc.CARefSplitLineNbr;
			}

            foreach (string voidedType in voidedTypes)
            {
                ARPayment voidedDoc = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
                            And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>.Select(sender.Graph, parentDoc.RefNbr, voidedType);

                if (voidedDoc != null)
                {
                    catran_Row.VoidedTranID = voidedDoc.CATranID;
                    break;
				}
            }

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		protected override bool NeedPreventCashTransactionCreation(PXCache sender, object row)
		{
			ARPayment payment = row as ARPayment;
			if (payment != null && payment.CashAccountID == null)
				return true;

			return base.NeedPreventCashTransactionCreation(sender, row);
		}
	}

    /// <summary>
    /// Specialized for the ARCashSale  version of the <see cref="CashTranIDAttribute"/><br/>
    /// Since CATran created from the source row, it may be used only the fields <br/>
    /// of ARPayment compatible DAC. <br/>
    /// The main purpuse of the attribute - to create CATran <br/>
    /// for the source row and provide CATran and source synchronization on persisting.<br/>
    /// CATran cache must exists in the calling Graph.<br/>
    /// </summary>
	public class ARCashSaleCashTranIDAttribute : CashTranIDAttribute
	{
		[Obsolete(Common.InternalMessages.ConstructorIsObsoleteAndWillBeRemoved2018R2)]
		public ARCashSaleCashTranIDAttribute(Type isMigrationModeEnabledSetupField)
			: base()
		{ }

		public ARCashSaleCashTranIDAttribute()
			: base()
		{ }

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			ARCashSale parentDoc = (ARCashSale)orig_Row;
			if (parentDoc.Released == true || 
				parentDoc.CuryOrigDocAmt == null || 
				parentDoc.IsMigratedRecord == true ||
				parentDoc.CuryOrigDocAmt == 0m)
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryID = parentDoc.CuryID;

			switch (parentDoc.DocType)
			{
				case ARDocType.CashSale:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = DrCr.Debit;
					break;
				case ARDocType.CashReturn:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = DrCr.Credit;
					break;
				default:
					throw new PXException();
			}

			catran_Row.TranDate = parentDoc.DocDate;
			catran_Row.TranDesc = parentDoc.DocDesc;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.ReferenceID = parentDoc.CustomerID;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
			catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;

			return catran_Row;
		}
	}

    /// <summary>
    /// Specialized for AR version of the <see cref="OpenPeriodAttribut"/><br/>
    /// Selector. Provides  a list  of the active Fin. Periods, having ARClosed flag = false <br/>
    /// <example>
    /// [AROpenPeriod(typeof(ARRegister.docDate))]
    /// </example>
    /// </summary>
	public class AROpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor

		public AROpenPeriodAttribute()
			: this(null)
		{
		}

		public AROpenPeriodAttribute(Type sourceType,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool useMasterOrganizationIDByDefault = false,
			SelectionModesWithRestrictions selectionModeWithRestrictions = SelectionModesWithRestrictions.Undefined,
			Type[] sourceSpecificationTypes = null,
		    Type masterFinPeriodIDType = null)
			: base(typeof(Search<FinPeriod.finPeriodID,
					Where<FinPeriod.aRClosed, Equal<False>,
						And<FinPeriod.status, Equal<FinPeriod.status.open>>>>),
						sourceType,
						branchSourceType: branchSourceType,
						branchSourceFormulaType: branchSourceFormulaType,
						organizationSourceType: organizationSourceType,
						useMasterCalendarSourceType: useMasterCalendarSourceType,
						defaultType: defaultType,
						redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
						useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
						selectionModeWithRestrictions: selectionModeWithRestrictions,
						sourceSpecificationTypes: sourceSpecificationTypes,
			            masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		#endregion

		#region Implementation

		public static void DefaultFirstOpenPeriod(PXCache sender, string FieldName)
		{
			foreach (PeriodIDAttribute attr in sender.GetAttributesReadonly(FieldName).OfType<PeriodIDAttribute>())
			{
				attr.DefaultType = typeof(Search2<FinPeriod.finPeriodID,
					CrossJoin<GLSetup>,
					Where<FinPeriod.endDate, Greater<Current2<QueryParams.sourceDate>>,
						And2<Where<FinPeriod.status, Equal<FinPeriod.status.open>,
									Or<FinPeriod.status, Equal<FinPeriod.status.closed>>>,
							And<Where<GLSetup.restrictAccessToClosedPeriods, NotEqual<True>,
								Or<FinPeriod.aRClosed, Equal<False>>>>>>,
					OrderBy<Asc<FinPeriod.finPeriodID>>>);
			}
		}

		public static void DefaultFirstOpenPeriod<Field>(PXCache sender)
			where Field : IBqlField
		{
			DefaultFirstOpenPeriod(sender, typeof(Field).Name);
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError && finPeriod.ARClosed == true)
			{
				result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: Messages.FinancialPeriodClosedInAR);
			}

			return result;
		}

		#endregion
	}

	#region accountsReceivableModule

	public sealed class accountsReceivableModule : PX.Data.BQL.BqlString.Constant<accountsReceivableModule>
	{
		public accountsReceivableModule() : base(typeof(accountsReceivableModule).Namespace) { }
	}

	#endregion
	#region customerType
	public sealed class customerType : PX.Data.BQL.BqlString.Constant<customerType>
	{
		public customerType()
			: base(typeof(PX.Objects.AR.Customer).FullName)
		{
		}
	}
	#endregion

	#region ARRegisterCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ARRegisterCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public ARRegisterCacheNameAttribute(string name)
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var register = row as ARRegister;

			if (register == null)
				return base.GetName();

			string name = string.Empty;

			switch (register.DocType)
			{
				case ARDocType.CashReturn:
					name = Messages.CashReturn;
					break;
				case ARDocType.CashSale:
					name = Messages.CashSale;
					break;
				case ARDocType.CreditMemo:
					name = Messages.CreditMemo;
					break;
				case ARDocType.DebitMemo:
					name = Messages.DebitMemo;
					break;
				case ARDocType.FinCharge:
					name = Messages.FinCharge;
					break;
				case ARDocType.Invoice:
					name = Messages.Invoice;
					break;
				case ARDocType.Payment:
					name = Messages.Payment;
					break;
				case ARDocType.Prepayment:
					name = Messages.Prepayment;
					break;
				case ARDocType.Refund:
					name = Messages.Refund;
					break;
                case ARDocType.VoidRefund:
                    name = Messages.VoidRefund;
                    break;
                case ARDocType.SmallBalanceWO:
					name = Messages.SmallBalanceWO;
					break;
				case ARDocType.SmallCreditWO:
					name = Messages.SmallCreditWO;
					break;
				case ARDocType.VoidPayment:
					name = Messages.VoidPayment;
					break;
				default:
					name = Messages.Register;
					break;
			}

			return PXMessages.LocalizeNoPrefix(name);
		}
	}

	#endregion


	/// <summary>
	/// This attribute implements auto-generation of the next check sequential number for ARPayment Document<br/>
	/// according to the settings in the CashAccount and PaymentMethod. <br/>
	/// </summary>
	public class PaymentRefAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
	{
		protected Type _CashAccountID;
		protected Type _PaymentTypeID;
		protected Type _UpdateNextNumber;
		protected Type _IsMigratedRecord;
		protected Type _ClassType;
		protected string _TargetDisplayName;

		protected bool _UpdateCashManager = true;
		protected bool _AllowAskUpdateLastRefNbr = true;

		/// <summary>
		/// This flag defines wether the field is defaulted from the PaymentMethodAccount record by the next check number<br/>
		/// If it set to false - the field on which attribute is set will not be initialized by the next value.<br/>
		/// This flag doesn't affect persisting behavior - if user enter next number manually, it will be saved.<br/>
		/// </summary>
		public bool UpdateCashManager
		{
			get
			{
				return this._UpdateCashManager;
			}
			set
			{
				this._UpdateCashManager = value;
			}
		}

		public bool AllowAskUpdateLastRefNbr
		{
			get
			{
				return this._AllowAskUpdateLastRefNbr;
			}
			set
			{
				this._AllowAskUpdateLastRefNbr = value;
			}
		}

		private PaymentMethodAccount GetCashAccountDetail(PXCache sender, object row)
		{
			object CashAccountID = sender.GetValue(row, _CashAccountID.Name);
			object PaymentTypeID = sender.GetValue(row, _PaymentTypeID.Name);
			object Hold = false;

			if (_UpdateCashManager && CashAccountID != null && PaymentTypeID != null)
			{
				PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly<PaymentMethodAccount,
																Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
																	And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
																	And<PaymentMethodAccount.useForAR, Equal<True>>>>>(sender.Graph);
				PaymentMethodAccount det = cmd.Select(CashAccountID, PaymentTypeID);
				cmd.View.Clear();

				if (det != null && det.ARLastRefNbr == null)
				{
					det.ARLastRefNbr = string.Empty;
					det.ARLastRefNbrIsNull = true;
				}
				return det;
			}
			return null;
		}

		private void GetPaymentMethodSettings(PXCache sender, object row, out PaymentMethod aPaymentMethod, out PaymentMethodAccount aPMAccount)
		{
			aPaymentMethod = null;
			aPMAccount = null;
			object CashAccountID = sender.GetValue(row, _CashAccountID.Name);
			object PaymentTypeID = sender.GetValue(row, _PaymentTypeID.Name);
			object Hold = false;
			if (_UpdateCashManager && CashAccountID != null && PaymentTypeID != null)
			{
				PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly2<PaymentMethodAccount,
																	InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>>>,
																Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
																	And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
																	And<PaymentMethodAccount.useForAR, Equal<True>>>>>(sender.Graph);
				PXResult<PaymentMethodAccount, PaymentMethod> res = (PXResult<PaymentMethodAccount, PaymentMethod>)cmd.Select(CashAccountID, PaymentTypeID);
				aPaymentMethod = res;
				cmd.View.Clear();
				PaymentMethodAccount det = res;
				if (det != null && det.ARLastRefNbr == null)
				{
					det.ARLastRefNbr = string.Empty;
					det.ARLastRefNbrIsNull = true;
				}
				aPMAccount = det;
			}

		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(!this._AllowAskUpdateLastRefNbr)
			{
				return;
			}

			PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);
			if (det != null && det.ARAutoNextNbr == true)
			{
				string OldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);
				bool? isMigratedRecord = (bool?)sender.GetValue(e.Row, _IsMigratedRecord.Name);

				if (isMigratedRecord != true &&
					string.IsNullOrEmpty(OldValue) == false && 
					string.IsNullOrEmpty((string)e.NewValue) == false && 
					object.Equals(OldValue, e.NewValue) == false)
				{
					try
					{
						if (sender.Graph.Views.ContainsKey("Document") && sender.Graph.Views["Document"].CacheGetItemType() == sender.GetItemType())
						{
							WebDialogResult result = sender.Graph.Views["Document"].Ask(e.Row, Messages.AskConfirmation, Messages.AskUpdateLastRefNbr, MessageButtons.YesNo, MessageIcon.Question);
							if (result == WebDialogResult.Yes)
							{
								sender.SetValue(e.Row, this._UpdateNextNumber.Name, true);
							}
						}
					}
					catch (PXException ex)
					{
						if (ex is PXDialogRequiredException)
						{
							throw;
						}
					}
				}
			}

		}

		protected Type _Table = null;

		/// <summary>
		/// Defines a table, from where oldValue of the field is taken from.<br/>
		/// If not set - the table associated with the sender will be used<br/>
		/// </summary>
		public Type Table
		{
			get
			{
				return this._Table;
			}
			set
			{
				this._Table = value;
			}
		}

		protected virtual string GetOldField(PXCache sender, object Row)
		{
			List<PXDataField> fields = new List<PXDataField>();

			fields.Add(new PXDataField(_FieldName));

			foreach (string key in sender.Keys)
			{
				fields.Add(new PXDataFieldValue(key, sender.GetValue(Row, key)));
			}

			using (PXDataRecord OldRow = PXDatabase.SelectSingle(_Table, fields.ToArray()))
			{
				if (OldRow != null)
				{
					return OldRow.GetString(0);
				}
			}
			return null;
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object SetNumber = sender.GetValue(e.Row, _FieldOrdinal);
			bool updateNextNumber = ((Boolean?)sender.GetValue(e.Row, this._UpdateNextNumber.Name)) ?? false;

			PaymentMethodAccount det;
			PaymentMethod pm;
			GetPaymentMethodSettings(sender, e.Row, out pm, out det);

			if (det == null || pm == null)
			{
				return;
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				string NewNumber = AutoNumberAttribute.NextNumber(det.ARLastRefNbr);

				if (SetNumber != null)
				{
					if (det.ARAutoNextNbr == true && (object.Equals((string)SetNumber, NewNumber) || updateNextNumber))
					{
						try
						{
							PXDatabase.Update<PaymentMethodAccount>(
								new PXDataFieldAssign("ARLastRefNbr", SetNumber),
								new PXDataFieldRestrict("CashAccountID", det.CashAccountID),
								new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),
								new PXDataFieldRestrict("ARLastRefNbr", det.ARLastRefNbr),
								PXDataFieldRestrict.OperationSwitchAllowed);
						}
						catch (PXDbOperationSwitchRequiredException)
							{
								PXDatabase.Insert<PaymentMethodAccount>(
									new PXDataFieldAssign("CashAccountID", det.CashAccountID),
									new PXDataFieldAssign("PaymentMethodID", det.PaymentMethodID),
									new PXDataFieldAssign("UseForAR", det.UseForAR),
									new PXDataFieldAssign("ARLastRefNbr", NewNumber),
									new PXDataFieldAssign("ARAutoNextNbr", det.ARAutoNextNbr),
									new PXDataFieldAssign("ARIsDefault", det.ARIsDefault),
									new PXDataFieldAssign("UseForAP", det.UseForAP),
									new PXDataFieldAssign("APIsDefault", det.APIsDefault),
									new PXDataFieldAssign("APAutoNextNbr", det.APIsDefault));
							}
					}
				}
			}
		}

		void IPXFieldDefaultingSubscriber.FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PaymentMethod pm;
			PaymentMethodAccount det;
			GetPaymentMethodSettings(sender, e.Row, out pm, out det);
			e.NewValue = null;
			if (pm != null && det != null)
			{
				if (det.ARAutoNextNbr == true)
				{
					int i = 0;
					do
					{
						try
						{
							e.NewValue = AutoNumberAttribute.NextNumber(det.ARLastRefNbr, ++i);
						}
						catch (Exception)
						{
                            sender.RaiseExceptionHandling(_FieldName, e.Row, null, new AutoNumberException(CS.Messages.CantAutoNumberSpecific, _TargetDisplayName));
						}

						if (i > 100)
						{
							e.NewValue = null;
                            new AutoNumberException(CS.Messages.CantAutoNumberSpecific, _TargetDisplayName);
						}
					}
					while (PXSelect<CashAccountCheck, Where<CashAccountCheck.accountID, Equal<Current<PaymentMethodAccount.cashAccountID>>, And<CashAccountCheck.paymentMethodID, Equal<Current<PaymentMethodAccount.paymentMethodID>>, And<CashAccountCheck.checkNbr, Equal<Required<CashAccountCheck.checkNbr>>>>>>.SelectSingleBound(sender.Graph, new object[] { det }, new object[] { e.NewValue }).Count == 1);

					if (i > 1)
					{
						//will be persisted via Graph
						//det.APLastRefNbr = (string)e.NewValue;
						//sender.Graph.Caches[typeof(PaymentMethodAccount)].Update(det);
						sender.SetValue(e.Row, this._UpdateNextNumber.Name, true);
					}
				}
			}
		}

		public PaymentRefAttribute(Type CashAccountID, Type PaymentTypeID, Type UpdateNextNumber)
		{
			_CashAccountID = CashAccountID;
			_PaymentTypeID = PaymentTypeID;
			_UpdateNextNumber = UpdateNextNumber;
		}

		public PaymentRefAttribute(Type CashAccountID, Type PaymentTypeID, Type UpdateNextNumber, Type IsMigratedRecord)
		{
			_CashAccountID = CashAccountID;
			_PaymentTypeID = PaymentTypeID;
			_UpdateNextNumber = UpdateNextNumber;
			_IsMigratedRecord = IsMigratedRecord;
		}

		private void DefaultRef(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object oldValue = sender.GetValue(e.Row, _FieldName);
			sender.SetValue(e.Row, _FieldName, null);
			sender.SetDefaultExt(e.Row, _FieldName);
			if (sender.GetValue(e.Row, _FieldName) == null && sender.Graph.IsCopyPasteContext == false)
			{
				sender.SetValue(e.Row, _FieldName, oldValue);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ClassType = sender.GetItemType();
			if (_Table == null)
			{
				_Table = sender.BqlTable;
			}

			sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_CashAccountID), _CashAccountID.Name, DefaultRef);
			sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_PaymentTypeID), _PaymentTypeID.Name, DefaultRef);

			_TargetDisplayName = PXUIFieldAttribute.GetDisplayName<PaymentMethodAccount.aRLastRefNbr>(sender.Graph.Caches[typeof(PaymentMethodAccount)]);
		}

		/// <summary>
		/// Sets IsUpdateCashManager flag for each instances of the Attibute specifed on the on the cache.
		/// </summary>
		/// <typeparam name="Field"> field, on which attribute is set</typeparam>
		/// <param name="cache"></param>
		/// <param name="data">Row. If omited, Field is set as altered for the cache</param>
		/// <param name="isUpdateCashManager">Value of the flag</param>
		public static void SetUpdateCashManager<Field>(PXCache cache, object data, bool isUpdateCashManager)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PaymentRefAttribute)
				{
					((PaymentRefAttribute)attr).UpdateCashManager = isUpdateCashManager;
				}
			}
		}

		public static void SetAllowAskUpdateLastRefNbr<Field>(PXCache cache, bool AllowAskUpdateLastRefNbr)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PaymentRefAttribute)
				{
					((PaymentRefAttribute)attr).AllowAskUpdateLastRefNbr = AllowAskUpdateLastRefNbr;
				}
			}
		}
	}

    public class ARPaymentChargeCashTranIDAttribute : CashTranIDAttribute
    {
        protected bool _IsIntegrityCheck = false;

		[Obsolete(Common.InternalMessages.ConstructorIsObsoleteAndWillBeRemoved2018R2)]
		public ARPaymentChargeCashTranIDAttribute(Type isMigrationModeEnabledSetupField)
			: base()
		{ }

		public ARPaymentChargeCashTranIDAttribute()
			: base()
		{ }

        public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
        {
            ARPaymentChargeTran parentDoc = (ARPaymentChargeTran)orig_Row;

            if (parentDoc.Released == true || parentDoc.CuryTranAmt == null || parentDoc.CuryTranAmt == 0m || parentDoc.Consolidate == true || string.IsNullOrEmpty(parentDoc.EntryTypeID))
            {
                return null;
            }

            catran_Row.OrigModule = BatchModule.AR;
            catran_Row.OrigTranType = parentDoc.DocType;
            catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.IsPaymentChargeTran = true;
			catran_Row.OrigLineNbr = parentDoc.LineNbr;
            catran_Row.CashAccountID = parentDoc.CashAccountID;
            catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryTranAmt = parentDoc.GetCASign() * parentDoc.CuryTranAmt;
            catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
            catran_Row.DrCr = parentDoc.DrCr;
            SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);

            ARRegister register = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARPaymentChargeTran.docType>>,
				And<ARRegister.refNbr, Equal<Required<ARPaymentChargeTran.refNbr>>>>>.Select(sender.Graph, parentDoc.DocType, parentDoc.RefNbr);
			catran_Row.ReferenceID = register.CustomerID;
            catran_Row.TranDate = parentDoc.TranDate;
            catran_Row.TranDesc = parentDoc.TranDesc;
            catran_Row.Released = parentDoc.Released;

			CashAccount cashAccount = GetCashAccount(catran_Row, sender.Graph);
			catran_Row.CuryID = cashAccount.CuryID;
			SetCleared(catran_Row, cashAccount);

            string[] voidedTypes = ARPaymentType.GetVoidedARDocType(parentDoc.DocType);

            catran_Row.Hold = register.Hold;

            foreach (string voidedType in voidedTypes)
            {
                int sign = ARPaymentType.DrCr(voidedType) == ARPaymentType.DrCr(parentDoc.DocType) ? (-1) : (1);

                ARPaymentChargeTran voidedDoc = PXSelectReadonly<ARPaymentChargeTran, Where<ARPaymentChargeTran.refNbr, Equal<Required<ARRegister.refNbr>>,
                                               And<ARPaymentChargeTran.docType, Equal<Required<ARRegister.docType>>,
                                               And<ARPaymentChargeTran.lineNbr, Equal<Required<CATran.origLineNbr>>,
                                               And<ARPaymentChargeTran.curyTranAmt, Equal<Required<ARPaymentChargeTran.curyTranAmt>>>>>>>
                                               .Select(sender.Graph, register.RefNbr, voidedType, catran_Row.OrigLineNbr, sign * parentDoc.CuryTranAmt);

                if (voidedDoc != null)
                {
                    catran_Row.VoidedTranID = voidedDoc.CashTranID;
                    break;
                }
            }

            return catran_Row;
        }

        public static CATran DefaultValues<Field>(PXCache sender, object data)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
            {
                if (attr is ARPaymentChargeCashTranIDAttribute)
                {
                    ((ARPaymentChargeCashTranIDAttribute)attr)._IsIntegrityCheck = true;
                    return ((ARPaymentChargeCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
                }
            }
            return null;
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (_IsIntegrityCheck == false)
            {
                base.RowPersisting(sender, e);
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (_IsIntegrityCheck == false)
            {
                base.RowPersisted(sender, e);
            }
        }
    }

	public class ARCrossItemAttribute : IN.CrossItemAttribute
	{
		public ARCrossItemAttribute() : base(
			typeof(Search<IN.InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>),
			typeof(IN.InventoryItem.inventoryCD),
			typeof(IN.InventoryItem.descr),
			IN.INPrimaryAlternateType.CPN) {}
	}

    #region PXPriceCodeSelector
    /// <summary>
    /// For UsrDeletedDatabaseRecord support only
    /// </summary>
    public class PXPriceCodeSelectorAttribute : PXSelectorAttribute
    {
        public PXPriceCodeSelectorAttribute(Type type)
            : base(type)
        {
        }

        public PXPriceCodeSelectorAttribute(Type type, params Type[] fieldList)
            : base(type, fieldList)
        {
        }

        protected override bool IsReadDeletedSupported
        {
            get
            {
                return false;
            }
        }
    }

    #endregion

    public class ARSetupSelect : PXSetupSelect<ARSetup>
    {
        public ARSetupSelect(PXGraph graph)
            : base(graph)
        {
        }

        protected override void FillDefaultValues(ARSetup record)
        {
            record.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
        }
    }

	public class ARPaymentChargeSelect<PaymentTable, PaymentMethodID, CashAccountID, DocDate, TranPeriodID, PMInstanceID, WhereSelect> :
		AP.PaymentChargeSelect<PaymentTable, PaymentMethodID, CashAccountID, DocDate, TranPeriodID,
            ARPaymentChargeTran, ARPaymentChargeTran.entryTypeID, ARPaymentChargeTran.docType, ARPaymentChargeTran.refNbr, ARPaymentChargeTran.cashAccountID,
		    ARPaymentChargeTran.finPeriodID, ARPaymentChargeTran.tranDate, WhereSelect>
		where PaymentTable : class, IBqlTable, new()
		where PaymentMethodID : IBqlField
		where CashAccountID : IBqlField
		where DocDate : IBqlField
	    where TranPeriodID : IBqlField
        where PMInstanceID : IBqlField
		where WhereSelect : IBqlWhere, new()
	{
		public ARPaymentChargeSelect(PXGraph graph)
			: base(graph)
		{
		}

		protected override void RelatedFieldsDefaulting(PXCache sender, PaymentTable payment)
		{
			object newPMInstanceID;
			sender.RaiseFieldDefaulting<PMInstanceID>(payment, out newPMInstanceID);
			sender.SetValue<PMInstanceID>(payment, newPMInstanceID);
		}

		public void ReverseCharges(CM.IRegister oldDoc, CM.IRegister newDoc)
		{
			ReverseCharges(oldDoc, newDoc, ARPaymentType.DrCr(oldDoc.DocType) == ARPaymentType.DrCr(newDoc.DocType));
		}

		#region PaymentChargesValidation
		/// <summary>
		/// Check if the negative sign of the <see cref="ARPaymentChargeTran.CuryTranAmt"/> of the charge is allowed by document type.
		/// </summary>
		/// <param name="charge">The charge.</param>
		/// <returns/>
		protected override bool IsAllowedNegativeSign(ARPaymentChargeTran charge) => ARPaymentType.VoidAppl(charge.DocType);
		#endregion
	}

	public class ARReports : ReportUtils
	{
		public const string InvoiceMemoReportID = "AR641000";
		public const string DunningLetterReportID = "AR661000";
		public const string AREditDetailedReportID = "AR610500";
		public const string ARRegisterDetailedReportID = "AR622000";
	}
}
