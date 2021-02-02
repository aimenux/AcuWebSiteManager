using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	/// <summary>
	/// Represents a tax category. The head of a master-detail aggregate of a set of taxes. 
	/// The class is used to define a set of taxes that can be applied to a document on data entry pages.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(TaxCategoryMaint))]
	[PXCacheName(Messages.TaxCategory)]
	public partial class TaxCategory : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxCategory>.By<taxCategoryID>
		{
			public static TaxCategory Find(PXGraph graph, string taxCategoryID) => FindBy(graph, taxCategoryID);
		}
		#endregion

		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;

		/// <summary>
		/// The tax category ID. This is the key field, which can be specified by the user.
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Tax Category ID",Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<TaxCategory.taxCategoryID>), CacheGlobal = true)]
		[PX.Data.EP.PXFieldDescription]
		[PXReferentialIntegrityCheck]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the tax category, which can be specified by the user.
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
		#region TaxCatFlag
		public abstract class taxCatFlag : PX.Data.BQL.BqlBool.Field<taxCatFlag> { }
		protected Boolean? _TaxCatFlag;

		/// <summary>
		/// "Exclude Listed Taxes" flag. Specifies how the taxes that are included in the category should be applied to the document line.
		/// <value>
		/// <c>false</c>: Only the taxes of the category that are intersected with the taxes of the tax zone should be applied to the document line.
		/// <c>true</c>: All taxes of the tax zone except the taxes of the category should be applied to the document line.
		/// </value>
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exclude Listed Taxes")]
		public virtual Boolean? TaxCatFlag
		{
			get
			{
				return this._TaxCatFlag;
			}
			set
			{
				this._TaxCatFlag = value;
			}
		}
		#endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		/// <summary>
		/// Indicates (if set to <c>true</>) that the tax category is active.
		/// </summary>
		[PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        #endregion
		#region Exempt
		public abstract class exempt : IBqlField { }

		/// <summary>
		/// Indicates (if set to <c>true</>) that this is exempt tax category .
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exempt", FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual bool? Exempt { get; set; }
		#endregion
        #region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(TaxCategory.taxCategoryID),
			Selector = typeof(Search<TaxCategory.taxCategoryID>))]
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
