using System.Globalization;
using PX.Common;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;

namespace PX.Objects.CS
{
    using System;
    using PX.Data;
    using System.Diagnostics;
	using System.Collections.Generic;
	using System.Linq;

	public class CSAttributeSelectorAttribute : PXSelectorAttribute
	{
		public CSAttributeSelectorAttribute()
			: base(typeof(CSAttribute.attributeID))
		{
			DescriptionField = typeof(CSAttribute.description);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if ((sender.Graph.IsImport || sender.Graph.IsExport) && !sender.Graph.IsCopyPasteContext && e.ReturnValue is string)
			{
			   CRAttribute.Attribute attr;
				CRAttribute.Attribute attrDesc;
				if (CRAttribute.Attributes.TryGetValue((string)e.ReturnValue, out attr) &&
					CRAttribute.AttributesByDescr.TryGetValue(attr.Description, out attrDesc) &&
					attr.ID == attrDesc.ID)
				{
					e.ReturnValue = attr.Description;
				}
			}
		}

		public virtual void AttributeIDFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if ((sender.Graph.IsImport || sender.Graph.IsExport) && e.NewValue is string)
			{
				CRAttribute.Attribute attr;
				if (CRAttribute.AttributesByDescr.TryGetValue((string)e.NewValue, out attr) &&
						 !CRAttribute.Attributes.Contains((string)e.NewValue))
				{
					e.NewValue = attr.ID;
				}
				else if (CRAttribute.AttributesByDescr.TryGetValue((string)e.NewValue, out attr) && attr.ID != null && String.Equals((string)e.NewValue, attr.ID, StringComparison.InvariantCultureIgnoreCase))
				{
					e.NewValue = attr.ID;
				}
			}
		}

		public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
	    {
            object key = sender.GetValue(e.Row, _FieldOrdinal);
	        if (key != null)
	        {
	            e.ReturnValue = CRAttribute.Attributes[(string) key].With(attr => attr.Description);
	        }
	        //base.DescriptionFieldSelecting();                                     
	    }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, AttributeIDFieldUpdating);
		}
	}

    #region ValueValidationAttribute
    /// <summary>
    /// This is base class for attribute allows to provide a validation rules for the field.<br/>
    /// <example>
    /// [DynamicValueValidation(typeof(Search<PaymentMethodDetail.validRegexp, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>,
    ///								   And<PaymentMethodDetail.detailID, Equal<Current<VendorPaymentMethodDetail.detailID>>>>>))]
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public abstract class ValueValidationAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
    {

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>        
        protected ValueValidationAttribute()
        {
        }

        #endregion

        #region Implementation

        public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            string value = (string)e.NewValue;
            if (!string.IsNullOrEmpty(value))
			{
                string regexp = this.FindValidationRegexp(sender, e.Row);
                if (!this.ValidateValue(value, regexp))
                    throw new PXSetPropertyException(CA.Messages.ValueIsNotValid);

            }
			}

        protected abstract string FindValidationRegexp(PXCache sender, Object row);
        protected virtual bool ValidateValue(string val, string regex)
        {
            if (val == null || regex == null)
                return true;
            System.Text.RegularExpressions.Regex regexobject = new System.Text.RegularExpressions.Regex(regex);
            return regexobject.IsMatch(val);
        }
        #endregion
    }
    #endregion

    public class CSAttributeValueValidationAttribute : ValueValidationAttribute
    {
        #region state
        private readonly Type sourceAttribute;
        #endregion

        public CSAttributeValueValidationAttribute(Type sourceAttribute)
        {
            this.sourceAttribute = sourceAttribute;
        }

        protected override string FindValidationRegexp(PXCache sender, object row)
        {
            string id = (string)sender.GetValue(row, sourceAttribute.Name);
            return id == null ? null : CRAttribute.Attributes[id].With(attr=>attr.RegExp);
		}
	}

	[System.SerializableAttribute]
	[DebuggerDisplay("AttributeID={AttributeID} IsRequired={IsRequired}")]
	[PXCacheName(Messages.CSAnswers)]
	[PXPossibleRowsList(typeof(CSAttribute.description), typeof(CSAnswers.attributeID), typeof(CSAnswers.value))]
	public partial class CSAnswers : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<CSAnswers>.By<refNoteID, attributeID>
        {
            public static CSAnswers Find(PXGraph graph, Guid? refNoteID, string attributeID) => FindBy(graph, refNoteID, attributeID);
        }
        #endregion
        #region EntityNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        [PXDBGuid(IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField]
        public virtual Guid? RefNoteID { get; set; }
        #endregion


		#region AttributeID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		protected String _AttributeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Attribute")]
		[CSAttributeSelector]
		public virtual String AttributeID
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
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		
        [PXAttributeValue]
		[PXUIField(DisplayName = "Value")]
		[CSAttributeValueValidation(typeof(CSAnswers.attributeID))]
		[PXPersonalDataFieldAttribute.Value]
		public virtual String Value
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
		
		//#region EntityClassID
		//public abstract class entityClassID : PX.Data.BQL.BqlString.Field<entityClassID>
		//{
		//}
		//protected String _EntityClassID;
		//[PXDBString(30, IsUnicode = true, IsKey = true)]
		//[PXDefault()]
		//[PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		//public virtual String EntityClassID
		//{
		//	get
		//	{
		//		return this._EntityClassID;
		//	}
		//	set
		//	{
		//		this._EntityClassID = value;
		//	}
		//}
		//#endregion

		#region IsRequired
		public abstract class isRequired : PX.Data.BQL.BqlBool.Field<isRequired> { }
		protected bool? _IsRequired;
		[PXBool()]
		[PXUIField(DisplayName = "Required", IsReadOnly = true)]
		public virtual bool? IsRequired
		{
			get
			{
				return this._IsRequired;
			}
			set
			{
				this._IsRequired = value;
			}
		}
		#endregion
		#region AttributeCategory
		public abstract class attributeCategory : PX.Data.BQL.BqlBool.Field<attributeCategory> { }
		[PXString(1, IsUnicode = false, IsFixed = true)]
		[PXUIField(DisplayName = "Category", IsReadOnly = true, FieldClass = nameof(FeaturesSet.MatrixItem))]
		[CSAttributeGroup.attributeCategory.List]
		public virtual string AttributeCategory
		{
			get;
			set;
		}
		#endregion


		#region Order
		public abstract class order : PX.Data.BQL.BqlShort.Field<order> { }

		[PXShort]
		public Int16? Order { get; set; }
		#endregion
	}

    internal class PXAttributeValueAttribute : PXDBStringAttribute
    {
		private class ValuesWithSortOrder : IPrefetchable
		{
			public Dictionary<string, Dictionary<string, short>> AllValues { get; private set; }
			public List<string> MultiSelectAttributes { get; private set; }
			public void Prefetch()
			{
				MultiSelectAttributes = PXDatabase.SelectMulti<CSAttribute>(
					new PXDataField<CSAttribute.attributeID>(),
					new PXDataField<CSAttribute.controlType>())
					.Where(r => r.GetInt32(1) == 6)
					.Select(r => r.GetString(0)).ToList();

				var attributesWithValues = PXDatabase.SelectMulti<CSAttributeDetail>(
					new PXDataField<CSAttributeDetail.attributeID>(),
					new PXDataField<CSAttributeDetail.valueID>(),
					new PXDataField<CSAttributeDetail.sortOrder>())
					.Where(r => MultiSelectAttributes.Contains(r.GetString(0)))
					.Select(r => new CSAttributeDetail
					{
						AttributeID = r.GetString(0),
						ValueID = r.GetString(1),
						SortOrder = r.GetInt16(2)
					}).ToList();

				AllValues = new Dictionary<string, Dictionary<string, short>>();
				foreach (var attribute in MultiSelectAttributes)
				{
					Dictionary<string, short> valuesWithSortOrder = new Dictionary<string, short>();
					foreach (var attributeWithValues in attributesWithValues.Where(a => a.AttributeID == attribute))
					{
						valuesWithSortOrder[attributeWithValues.ValueID] = attributeWithValues.SortOrder ?? 0;
					}
					AllValues[attribute] = valuesWithSortOrder;
				}
			}
		}

		private Dictionary<string, Dictionary<string, short>> AttributesWithValues
		{
			get
			{
				return PXDatabase.GetSlot<ValuesWithSortOrder>(nameof(PXAttributeValueAttribute), typeof(CSAttribute), typeof(CSAttributeDetail)).AllValues;
			}
		}

		private List<string> MultiSelectAttributes
		{
			get
			{
				return PXDatabase.GetSlot<ValuesWithSortOrder>(nameof(PXAttributeValueAttribute), typeof(CSAttribute)).MultiSelectAttributes;
			}
		}

		public PXAttributeValueAttribute() : base(255)
        {
            IsUnicode = true;
        }

        public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if(e.NewValue is DateTime)
                e.NewValue = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture);
			var row = e.Row as CSAnswers;
			string attributeID = row?.AttributeID;
			if (!string.IsNullOrWhiteSpace(attributeID) && e.NewValue != null && MultiSelectAttributes.Contains(attributeID))
			{
				var valuesWithOrder = AttributesWithValues[attributeID];
				var realValues = (e.NewValue as string).Split(',').Where(val => !string.IsNullOrEmpty(val));
				e.NewValue = string.Join(",", valuesWithOrder.Where(val => realValues.Contains(val.Key))
					.OrderBy(val => val.Value).ThenBy(val => val.Key).Select(val => val.Key));
			}
            base.FieldUpdating(sender, e);
        }
    }
}
