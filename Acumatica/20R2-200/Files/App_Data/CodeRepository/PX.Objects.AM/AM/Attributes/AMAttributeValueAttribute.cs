using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;

namespace PX.Objects.AM.Attributes
{
    public class AMAttributeValueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        private Type _AttributeIDField;
        private Type _IsRequiredField;
        public int AnswerValueLength = 60;
        public Type AnswerValueField;

        public AMAttributeValueAttribute(Type attributeIDField)
        {
            _AttributeIDField = attributeIDField;
        }

        public AMAttributeValueAttribute(Type attributeIDField, Type isRequiredField)
        {
            _AttributeIDField = attributeIDField;
            _IsRequiredField = isRequiredField;
        }

        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            string answerValueFieldName = AnswerValueField == null ? "Value" : AnswerValueField.Name;
            if (e.Row != null)
            {
                var attributeID = (string)sender.GetValue(e.Row, _AttributeIDField.Name);
                bool? isRequired = _IsRequiredField == null ? false : (bool?)sender.GetValue(e.Row, _IsRequiredField.Name);

                CSAttribute question = new PXSelect<CSAttribute>(sender.Graph).Search<CSAttribute.attributeID>(attributeID);
                PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
                    Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
                    OrderBy<Asc<CSAttributeDetail.sortOrder>>>.Select(sender.Graph, attributeID);

                int required = -1;
                if(isRequired.GetValueOrDefault())
                {
                    required = 1;
                }

                if (options.Count > 0)
                {
                    //ComboBox:

                    List<string> allowedValues = new List<string>();
                    List<string> allowedLabels = new List<string>();

                    foreach (CSAttributeDetail option in options)
                    {
                        allowedValues.Add(option.ValueID);
                        allowedLabels.Add(option.Description);
                    }

                    string mask = question != null ? question.EntryMask : null;

                    e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength, true, answerValueFieldName, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
                    if (question.ControlType == CSAttribute.MultiSelectCombo)
                    {
                        ((PXStringState)e.ReturnState).MultiSelect = true;
                    }
                }
                else if (question != null)
                {
                    if (question.ControlType.Value == CSAttribute.CheckBox)
                    {
                        e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required, null, null, false, answerValueFieldName, null, null, null, PXErrorLevel.Undefined, true, true, null, PXUIVisibility.Visible, null, null, null);
                    }
                    else if (question.ControlType.Value == CSAttribute.Datetime)
                    {
                        e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(DateTime), false, false,
                            required, null, null, false, answerValueFieldName, null, null, null, PXErrorLevel.Undefined,
                            true, true, null, PXUIVisibility.Visible, null, null, null);
                    }
                    else
                    {
                        //TextBox:
                        string mask = question != null ? question.EntryMask : null;

                        e.ReturnState = PXStringState.CreateInstance(e.ReturnState, AnswerValueLength, null, answerValueFieldName, false, required, mask, null, null, true, null);
                    }
                }

            }
        }
    }
}
