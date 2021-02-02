using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Feature Attribute")]
    public class AMFeatureAttribute : IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<AMFeatureAttribute>.By<featureID, lineNbr>
        {
            public static AMFeatureAttribute Find(PXGraph graph, string featureID, int lineNbr)
                => FindBy(graph, featureID, lineNbr);
        }

        public static class FK
        {
            public class Feature : AMFeature.PK.ForeignKeyOf<AMFeatureAttribute>.By<featureID> { }
        }
        #endregion

        #region FeatureID
        public abstract class featureID : PX.Data.BQL.BqlString.Field<featureID> { }

		protected string _FeatureID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(AMFeature.featureID))]
        [PXUIField(Visible = false)]
        [PXParent(typeof(Select<AMFeature, 
            Where<AMFeature.featureID, 
                Equal<Current<AMFeatureAttribute.featureID>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		protected int? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMFeature.lineCntrAttribute))]
        public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region AttributeID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }

		protected string _AttributeID;
		[PXDBString(10, IsUnicode = true, InputMask = "")] /*Add empty input mask to allow AMEmptySelectorValue to function correctly - Case # 101250*/
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Attribute ID")]
        [AMEmptySelectorValue(Messages.SelectorFormula)]
        [PXSelector(typeof(CSAttribute.attributeID))]
		public virtual string AttributeID
		{
			get
			{
				return this._AttributeID;
			}
			set
			{
				this._AttributeID = value;
			}
		}
		#endregion
		#region Label
		public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

		protected string _Label;
		[PXDBString(30, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Label")]
        [PXCheckUnique(typeof(AMFeatureAttribute.featureID))]
        public virtual string Label
		{
			get
			{
				return this._Label;
			}
			set
			{
				this._Label = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXDefault()]
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
        #region IsFormula
        public abstract class isFormula : PX.Data.BQL.BqlBool.Field<isFormula> { }

        [PXBool]
        [PXUIField(DisplayName = "Is Formula", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        public virtual bool? IsFormula
        {
            [PXDependsOnFields(typeof(AMFeatureAttribute.attributeID))]
            get
            {
                return string.IsNullOrEmpty(this.AttributeID);
            }
        }

        #endregion
        #region Enabled
        public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }

		protected bool? _Enabled;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Enabled")]
		public virtual bool? Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				this._Enabled = value;
			}
		}
		#endregion
		#region Required
		public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }

		protected bool? _Required;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required")]
		public virtual bool? Required
		{
			get
			{
				return this._Required;
			}
			set
			{
				this._Required = value;
			}
		}
		#endregion
		#region Visible
		public abstract class visible : PX.Data.BQL.BqlBool.Field<visible> { }

		protected bool? _Visible;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Visible")]
		public virtual bool? Visible
		{
			get
			{
				return this._Visible;
			}
			set
			{
				this._Visible = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		protected string _Value;
		[PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Default Value")]
        [AMAttributeValue(typeof(attributeID), typeof(required))]
        [DynamicValueValidation(typeof(Search<CSAttribute.regExp, Where<CSAttribute.attributeID, Equal<Current<attributeID>>>>))]
        public virtual string Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}
        #endregion

        #region Variable
        public abstract class variable : PX.Data.BQL.BqlString.Field<variable> { }

        protected string _Variable;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Variable")]
        [PXCheckUnique(typeof(AMFeatureAttribute.featureID))]
        public virtual string Variable
        {
            get
            {
                return this._Variable;
            }
            set
            {
                this._Variable = value;
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
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
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp()]
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