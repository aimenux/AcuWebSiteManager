using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
	[Serializable]
    [PXCacheName("Feature")]
    [PXPrimaryGraph(typeof(FeatureMaint))]
    public class AMFeature : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMFeature>.By<featureID>
        {
            public static AMFeature Find(PXGraph graph, string featureID)
                => FindBy(graph, featureID);
        }
        #endregion

        #region FeatureID
        public abstract class featureID : PX.Data.BQL.BqlString.Field<featureID> { }

		protected string _FeatureID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Feature ID")]
        [PXSelector(
            typeof(Search<AMFeature.featureID>),
            typeof(AMFeature.featureID),
            typeof(AMFeature.descr))]
		public virtual string FeatureID
		{
			get
			{
				return this._FeatureID;
			}
			set
			{
				this._FeatureID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
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
		#region ActiveFlg
		public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

		protected bool? _ActiveFlg;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? ActiveFlg
		{
			get
			{
				return this._ActiveFlg;
			}
			set
			{
				this._ActiveFlg = value;
			}
		}
		#endregion
		#region AllowNonInventoryOptions
		public abstract class allowNonInventoryOptions : PX.Data.BQL.BqlBool.Field<allowNonInventoryOptions> { }

		protected bool? _AllowNonInventoryOptions;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Non-Inventory Options")]
		public virtual bool? AllowNonInventoryOptions
		{
			get
			{
				return this._AllowNonInventoryOptions;
			}
			set
			{
				this._AllowNonInventoryOptions = value;
			}
		}
		#endregion
		#region DisplayOptionAttributes
		public abstract class displayOptionAttributes : PX.Data.BQL.BqlBool.Field<displayOptionAttributes> { }

		protected bool? _DisplayOptionAttributes;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Display Option Attributes", Visible = false)]
		public virtual bool? DisplayOptionAttributes
		{
			get
			{
				return this._DisplayOptionAttributes;
			}
			set
			{
				this._DisplayOptionAttributes = value;
			}
		}
		#endregion
		#region LineCntrAttribute
		public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

		protected int? _LineCntrAttribute;
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntrAttribute
		{
			get
			{
				return this._LineCntrAttribute;
			}
			set
			{
				this._LineCntrAttribute = value;
			}
		}
		#endregion
		#region LineCntrOption
		public abstract class lineCntrOption : PX.Data.BQL.BqlInt.Field<lineCntrOption> { }

		protected int? _LineCntrOption;
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntrOption
		{
			get
			{
				return this._LineCntrOption;
			}
			set
			{
				this._LineCntrOption = value;
			}
		}
        #endregion
        #region PrintResults
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        public abstract class printResults : PX.Data.BQL.BqlBool.Field<printResults> { }

        protected bool? _PrintResults;
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Print Results")]
        public virtual bool? PrintResults
        {
            get
            {
                return this._PrintResults;
            }
            set
            {
                this._PrintResults = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
        [PXDBCreatedByID]
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
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
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
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}