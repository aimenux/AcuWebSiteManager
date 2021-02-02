using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using CommonMessages = PX.Objects.CN.Common.Descriptor.SharedMessages;

namespace PX.Objects.CN.Common.Services
{
	/// <summary>
	/// Adjusts UI controls for common attributes.
	/// </summary>
	public class AttributeGroupDataProvider
	{
		private readonly PXGraph graph;
		private CSAttribute attribute;
		private List<CSAttributeDetail> attributeDetails;

		public AttributeGroupDataProvider(PXGraph graph)
		{
			this.graph = graph;
		}

		/// <summary>
		/// Configures the type of control for default value field on adding an attribute. Should be used on
		/// FieldSelecting event for <see cref="CSAttributeGroup.defaultValue"/>.
		/// </summary>
		public PXFieldState GetNewReturnState(object returnState, CSAttributeGroup attributeGroup)
		{
			attribute = GetAttribute(attributeGroup);
			attributeDetails = GetAttributeDetails(attributeGroup);
			var requiredFieldState = attributeGroup.Required.GetValueOrDefault()
				? 1
				: -1;
			return GetNeededReturnState(returnState, requiredFieldState);
		}

		private PXFieldState GetNeededReturnState(object returnState, int requiredFieldState)
		{
			return NeedToGetStringStateForComboAttribute()
				? GetStringStateForComboAttribute(returnState, requiredFieldState)
				: GetReturnStateForOtherAttributes(returnState, requiredFieldState);
		}

		private PXFieldState GetReturnStateForOtherAttributes(object returnState, int requiredFieldState)
		{
			switch (attribute?.ControlType.GetValueOrDefault())
			{
				case CSAttribute.CheckBox:
					return GetFieldState(returnState, requiredFieldState);
				case CSAttribute.Datetime:
					return GetDateState(returnState, requiredFieldState);
				default:
					return GetStringState(returnState, requiredFieldState);
			}
		}

		private bool NeedToGetStringStateForComboAttribute()
		{
			return attributeDetails.Any() && (attribute == null || attribute.ControlType == CSAttribute.Combo ||
				attribute.ControlType == CSAttribute.MultiSelectCombo);
		}

		private static PXFieldState GetFieldState(object returnState, int requiredFieldState)
		{
			return PXFieldState.CreateInstance(returnState, typeof(bool), false, false, requiredFieldState,
				null, null, false, CommonMessages.DefaultValue, null, null, null,
				PXErrorLevel.Undefined, true, true, null, PXUIVisibility.Visible);
		}

		private PXFieldState GetDateState(object returnState, int requiredFieldState)
		{
			return PXDateState.CreateInstance(returnState, CommonMessages.DefaultValue, false,
				requiredFieldState, attribute.EntryMask, attribute.EntryMask, null, null);
		}

		private PXFieldState GetStringState(object returnState, int requiredFieldState)
		{
			return PXStringState.CreateInstance(returnState, 60, null, CommonMessages.DefaultValue, false,
				requiredFieldState, attribute.EntryMask, null, null, true, null);
		}

		private PXFieldState GetStringStateForComboAttribute(object returnState, int requiredFieldState)
		{
			var stringState = (PXStringState)GetStringState(returnState, requiredFieldState);
			UpdateStringState(stringState);
			return stringState;
		}

		private void UpdateStringState(PXStringState stringState)
		{
			if (attribute.ControlType == CSAttribute.MultiSelectCombo)
			{
				stringState.MultiSelect = true;
			}
			stringState.Length = CSAttributeDetail.ParameterIdLength;
			stringState.AllowedLabels = attributeDetails.Select(x => x.Description).ToArray();
			stringState.AllowedValues = attributeDetails.Select(x => x.ValueID).ToArray();
			stringState.IsUnicode = true;
		}

		private CSAttribute GetAttribute(CSAttributeGroup attributeGroup)
		{
			return new PXSelect<CSAttribute>(graph).Search<CSAttribute.attributeID>(attributeGroup.AttributeID);
		}

		private List<CSAttributeDetail> GetAttributeDetails(CSAttributeGroup attributeGroup)
		{
			var query = new PXSelect<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>(graph);
			return query.Select(attributeGroup.AttributeID).FirstTableItems.ToList();
		}
	}
}