//using System;
//using System.Collections.Generic;
//using System.Text;
//using PX.Data;

//namespace PX.Objects.CR
//{
//    public class CRAttributeMaint : PXGraph<CRAttributeMaint, CSAttribute>
//    {
//        public PXSelect<CSAttribute> Attributes;
//        public PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Current<CSAttribute.attributeID>>>> CurrentAttribute;
//        [PXImport(typeof(CSAttribute))]
//        public PXSelect<CRAttributeDetails, Where<CRAttributeDetails.parameterID, Equal<Current<CSAttribute.attributeID>>>> AttributeDetails;

//        protected virtual void CRAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
//        {
//            SetControlsState(e.Row as CSAttribute, sender);
//        }

//        protected virtual void CRAttributeDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
//        {
//            CRAttributeDetails row = e.Row as CRAttributeDetails;

//            if (row != null && CurrentAttribute.Current != null)
//            {
//                row.ParameterID = CurrentAttribute.Current.CRAttributeID;
//            }
//        }

//        protected virtual void CRAttribute_ControlType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
//        {
//            SetControlsState(e.Row as CSAttribute, sender);
//        }

//        private void SetControlsState(CSAttribute row, PXCache cache)
//        {
//            if (row != null)
//            {
//                AttributeDetails.Cache.AllowDelete = row.ControlType == CSAttribute.Combo;
//                AttributeDetails.Cache.AllowUpdate = row.ControlType == CSAttribute.Combo;
//                AttributeDetails.Cache.AllowInsert = row.ControlType == CSAttribute.Combo;

//                if (cache.GetStatus(row) == PXEntryStatus.Notchanged)
//                {
//                    CRAnswers ans = PXSelect<CRAnswers, Where<CRAnswers.parameterID, Equal<Required<CRAnswers.parameterID>>>>.SelectWindowed(this, 0, 1, row.AttributeID);
//                    CSAttributeGroup group = PXSelect<CSAttributeGroup, Where<CSAttributeGroup.attributeID, Equal<Required<CSAttribute.attributeID>>>>.SelectWindowed(this, 0, 1, row.AttributeID);

//                    bool enabled = (ans == null && group == null);

//                    PXUIFieldAttribute.SetEnabled<CSAttribute.controlType>(cache, row, enabled);
//                    cache.AllowDelete = enabled;
//                    AttributeDetails.Cache.AllowDelete = enabled;
//                }

//                if (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.CheckBox)
//                {
//                    PXUIFieldAttribute.SetEnabled<CSAttribute.entryMask>(cache, row, false);
//                    PXUIFieldAttribute.SetEnabled<CSAttribute.regExp>(cache, row, false);
//                }
//                else
//                {
//                    PXUIFieldAttribute.SetEnabled<CSAttribute.entryMask>(cache, row, true);
//                    PXUIFieldAttribute.SetEnabled<CSAttribute.regExp>(cache, row, true);
//                }
//            }
//        }

//        public override void Persist()
//        {
//            if (Attributes.Current != null)
//            {
//                string old = Attributes.Current.List;
//                Attributes.Current.List = null;
//                foreach (CRAttributeDetails det in AttributeDetails.Select())
//                {
//                    if (!String.IsNullOrEmpty(det.ValueID))
//                    {
//                        if (Attributes.Current.List == null)
//                        {
//                            Attributes.Current.List = det.ValueID + ';' + det.Description ?? "";
//                        }
//                        else
//                        {
//                            Attributes.Current.List = Attributes.Current.List + ',' + det.ValueID + ';' + det.Description ?? "";
//                        }
//                    }
//                }
//                if (!String.Equals(old, Attributes.Current.List) && Attributes.Cache.GetStatus(Attributes.Current) == PXEntryStatus.Notchanged)
//                {
//                    Attributes.Cache.SetStatus(Attributes.Current, PXEntryStatus.Updated);
//                }
//            }
//            base.Persist();
//        }
//    }
//}
