using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Represents a tax zone. The head of a master-detail aggregate of a set of taxes.
	/// The class is used to define a set of taxes that can be applied to a document on data entry pages.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxZone)]
	[PXPrimaryGraph(
		new Type[] { typeof(TaxZoneMaint)},
		new Type[] { typeof(Select<TaxZone, 
			Where<TaxZone.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>>)
		})]
	public partial class TaxZone : PX.Data.IBqlTable
	{
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// A key field, which can be specified by the user.
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Tax Zone ID",Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<TaxZone.taxZoneID, OrderBy<Asc<TaxZone.taxZoneID>>>), CacheGlobal = true)]
		[PX.Data.EP.PXFieldDescription]
		[PXReferentialIntegrityCheck]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;

		/// <summary>
		/// The description of the tax zone, which can be specified by the user.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PX.Data.EP.PXFieldDescription]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
		#region DfltTaxCategoryID
		public abstract class dfltTaxCategoryID : PX.Data.BQL.BqlString.Field<dfltTaxCategoryID> { }
		protected String _DfltTaxCategoryID;

		/// <summary>
		/// Default <see cref="TaxCategory"/>. It is used to set a tax category for document lines if there are no overriding defaults (e.g. by inventory item).
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Tax Category",Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID),DescriptionField = typeof(TaxCategory.descr))]
		public virtual String DfltTaxCategoryID
		{
			get
			{
				return this._DfltTaxCategoryID;
			}
			set
			{
				this._DfltTaxCategoryID = value;
			}
		}
		#endregion
		#region IsImported
		public abstract class isImported : PX.Data.BQL.BqlBool.Field<isImported> { }
		protected Boolean? _IsImported;

		/// <summary>
		/// The field was used on importing of tax configuration from Avalara files.
		/// </summary>
		[Obsolete("This property is obsolete and will be removed in Acumatica 8.0")]
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsImported
		{
			get
			{
				return this._IsImported;
			}
			set
			{
				this._IsImported = value;
			}
		}
		#endregion
		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }
		protected Boolean? _IsExternal;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax zone is used for the external tax provider.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="External Tax Provider")]
		public virtual Boolean? IsExternal
		{
			get
			{
				return this._IsExternal;
			}
			set
			{
				this._IsExternal = value;
			}
		}
		#endregion
		#region TaxPluginID
		public abstract class taxPluginID : PX.Data.BQL.BqlString.Field<taxPluginID> { }
		protected string _TaxPluginID;

		/// <summary>
		/// <see cref="PX.Objects.TX.TaxPlugin.taxPluginID"/> of a tax provider which will bu used. 
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Provider ID")]
		[PXSelector(typeof(TaxPlugin.taxPluginID),DescriptionField = typeof(TaxPlugin.description))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxPluginID
		{
			get
			{
				return this._TaxPluginID;
			}
			set
			{
				this._TaxPluginID = value;
			}
		}
		#endregion
		#region TaxVendorID
		public abstract class taxVendorID : PX.Data.BQL.BqlInt.Field<taxVendorID> { }
		protected Int32? _TaxVendorID;

		/// <summary>
		/// <see cref="PX.Objects.AP.Vendor.BAccountID"/> of a tax agency to which the tax zone belongs. 
		/// </summary>
		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<True>>>), DisplayName = "Tax Agency ID", DescriptionField = typeof(Vendor.acctName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? TaxVendorID
		{
			get
			{
				return this._TaxVendorID;
			}
			set
			{
				this._TaxVendorID = value;
			}
		}
		#endregion

		#region IsManualVATZone
		public abstract class isManualVATZone : PX.Data.BQL.BqlBool.Field<isManualVATZone> { }
		protected Boolean? _IsManualVATZone;

		/// <summary>
		/// The field marks that tax zone is used for manual VAT entry.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual VAT Entry")]
		public virtual Boolean? IsManualVATZone
		{
			get
			{
				return this._IsManualVATZone;
			}
			set
			{
				this._IsManualVATZone = value;
			}
		}
		#endregion
		#region ShowTaxTabExpr
		public abstract class showTaxTabExpr : PX.Data.BQL.BqlBool.Field<showTaxTabExpr> { }

		/// <summary>
		/// A service field. It is used to hide "Applicable Taxes" tab on TX206000 page.
		/// </summary>
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? ShowTaxTabExpr
		{
			get
			{
				return (!PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() || (this.IsManualVATZone != true)) && (this.IsExternal != true);
			}
		}
		#endregion
		#region ShowZipTabExpr
		public abstract class showZipTabExpr : PX.Data.BQL.BqlBool.Field<showZipTabExpr> { }

		/// <summary>
		/// A service field. It is used to hide "Zip Codes" tab on TX206000 page.
		/// </summary>
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? ShowZipTabExpr
		{
			get
			{
				return (!PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() || (this.IsManualVATZone != true));
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected String _TaxID;

		/// <summary>
		/// The field contains ID of a tax that would be used to create tax transactions in documents.
		/// It is relevant when the zone is used for manual VAT entry <see cref="IsManualVATZone"/>.
		/// </summary>
		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax ID")]
		[PXRestrictor(typeof(Where<Tax.reverseTax, Equal<False>>), Messages.NoReverseInManualVAT)]
		[PXRestrictor(typeof(Where<Tax.deductibleVAT, Equal<False>>), Messages.NoDeductibleInManualVAT)]
		[PXSelector(typeof(Search<Tax.taxID, Where<Tax.isExternal, Equal<False>, And<Tax.taxType, Equal<CSTaxType.vat>>>>), DescriptionField = typeof(Tax.descr))]
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(TaxZone.taxZoneID),
			Selector = typeof(Search<TaxZone.taxZoneID>))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
