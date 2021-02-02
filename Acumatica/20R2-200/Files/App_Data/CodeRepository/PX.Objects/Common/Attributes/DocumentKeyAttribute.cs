using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Attributes
{
	public class DocumentKeyAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber
	{
		protected Type StringListAttributeType;
		protected Dictionary<string, string> ValuesLabels;
		protected Dictionary<string, string> LabelsValues;

		public DocumentKeyAttribute(Type listAttributeType)
		{
			if (!typeof(PXStringListAttribute).IsAssignableFrom(listAttributeType))
			{
				throw new PXArgumentException(nameof(listAttributeType));
			}

			StringListAttributeType = listAttributeType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			ValuesLabels = ((PXStringListAttribute)Activator.CreateInstance(StringListAttributeType))
				.ValueLabelDic
				.ToDictionary(pair => pair.Key, pair => PXMessages.LocalizeNoPrefix(pair.Value)?.Trim());
			LabelsValues = ValuesLabels
				.ToDictionary(pair => pair.Value, pair => pair.Key);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string innerValue = (string)e.ReturnValue;
			if (innerValue == null) return;

			string[] splitted = innerValue.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (splitted == null || splitted.Length != 2)
			{
				throw new ArgumentOutOfRangeException();
			}

			e.ReturnValue = $"{ValuesLabels[splitted[0]]} {splitted[1]}";
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			string outerValue = (string)e.NewValue;
			if (outerValue == null) return;

			string[] splitted = outerValue.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (splitted == null || splitted.Length < 2)
			{
				throw new ArgumentOutOfRangeException();
			}

			string refNumber = splitted[splitted.Length - 1];
			string displayDocType = outerValue.Substring(0, outerValue.Length - refNumber.Length).Trim();

			e.NewValue = $"{LabelsValues[displayDocType]} {refNumber}";
		}
	}

	public class DocumentSelectorAttribute : PXSelectorAttribute
	{
		public DocumentSelectorAttribute(Type type) : base(type)
		{
		}

		public DocumentSelectorAttribute(Type type, params Type[] fieldList) : base(type, fieldList)
		{
		}
 
		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			object newValue = e.NewValue;
			sender.Graph.Caches[BqlCommand.GetItemType(_SubstituteKey)].RaiseFieldUpdating(_SubstituteKey.Name, null, ref newValue);
			e.NewValue = newValue;
			base.SubstituteKeyFieldUpdating(sender, e);
		}

		public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.SubstituteKeyFieldSelecting(sender, e);
			object returnValue = e.ReturnValue;
			sender.Graph.Caches[BqlCommand.GetItemType(_SubstituteKey)].RaiseFieldSelecting(_SubstituteKey.Name, null, ref returnValue, false);
			e.ReturnValue = returnValue;

		}
	}
}
