using System;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("[{ConfigResultsID}][{ConfigurationID}:{Revision}:{AttributeLineNbr}]")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationResultAttribute)]
    public class AMConfigResultsAttribute : IBqlTable, IRuleValid
	{
		#region ConfigResultsID
		public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }

		protected int? _ConfigResultsID;
		[PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfigurationResults.configResultsID))]
        [PXParent(typeof(Select<AMConfigurationResults, Where<AMConfigurationResults.configResultsID, Equal<Current<configResultsID>>>>))]
        public virtual int? ConfigResultsID
		{
			get
			{
				return this._ConfigResultsID;
			}
			set
			{
				this._ConfigResultsID = value;
			}
		}
        #endregion
        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

        protected string _ConfigurationID;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
        public virtual string ConfigurationID
        {
            get
            {
                return this._ConfigurationID;
            }
            set
            {
                this._ConfigurationID = value;
            }
        }
        #endregion
        #region Revision
        public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

        protected string _Revision;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.revision))]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual string Revision
        {
            get
            {
                return this._Revision;
            }
            set
            {
                this._Revision = value;
            }
        }
        #endregion
        #region AttributeLineNbr
        public abstract class attributeLineNbr : PX.Data.BQL.BqlInt.Field<attributeLineNbr> { }

		protected int? _AttributeLineNbr;
		[PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Attribute Line Nbr", Visible = false, Enabled = false)]
        public virtual int? AttributeLineNbr
		{
			get
			{
				return this._AttributeLineNbr;
			}
			set
			{
				this._AttributeLineNbr = value;
			}
		}
        #endregion
        #region AttributeID
        public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }

        protected string _AttributeID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Attribute ID", Enabled = false)]
        [PXSelector(typeof(CSAttribute.attributeID), ValidateValue = false)]
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
        #region Parent
        public abstract class parent : PX.Data.BQL.BqlString.Field<parent> { }
        [PXDBString(IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Parent", Visible = false, Enabled = false)]
        public virtual string Parent { get; set; }
        #endregion
        #region Value
        public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		protected string _Value;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
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
        #region Enabled
        public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }

        protected bool? _Enabled;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Enabled",Enabled = false)]
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
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Required", Enabled = false)]
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
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Visible", Enabled = false)]
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
        #region RuleValid
        public abstract class ruleValid : PX.Data.BQL.BqlBool.Field<ruleValid> { }


        [PXDBBool]
        [PXDefault(true)]
        public virtual bool? RuleValid { get; set; }
        #endregion

        #region System Fields
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

        #endregion
    }
}