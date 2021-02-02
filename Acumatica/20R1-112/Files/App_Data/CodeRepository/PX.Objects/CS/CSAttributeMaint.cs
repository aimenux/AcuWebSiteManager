using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CS
{
	public class CSAttributeMaint : PXGraph<CSAttributeMaint, CSAttribute>
	{
		public PXSelect<CSAttribute> Attributes;
		public PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Current<CSAttribute.attributeID>>>> CurrentAttribute;
		[PXImport(typeof(CSAttribute))]
		public PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Current<CSAttribute.attributeID>>>, 
						OrderBy<Asc<CSAttributeDetail.sortOrder,
								Asc<CSAttributeDetail.valueID>>>> AttributeDetails;

		protected virtual void CSAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            var row = e.Row as CSAttribute;
			SetControlsState(row, sender);

			ValidateAttributeID(sender, row);

			if (row.ControlType == CSAttribute.GISelector)
			{
				if (!string.IsNullOrEmpty(row.ObjectName as string))
				{
					try
					{
						Type objType = System.Web.Compilation.PXBuildManager.GetType(row.ObjectName, true);
						PXCache objCache = this.Caches[objType];
						string[] fields = objCache.Fields
							.Where(f => objCache.GetBqlField(f) != null || f.EndsWith("_Attributes", StringComparison.OrdinalIgnoreCase))
							.Where(f => !objCache.GetAttributesReadonly(f).OfType<PXDBTimestampAttribute>().Any())
							.Where(f => !string.IsNullOrEmpty((objCache.GetStateExt(null, f) as PXFieldState)?.ViewName))
							.Where(f => f != "CreatedByID" && f != "LastModifiedByID")
							.ToArray();
						PXStringListAttribute.SetList<CSAttribute.fieldName>(sender, row, fields, fields);
					}
					catch { }
				}else PXStringListAttribute.SetList<CSAttribute.fieldName>(sender, row, Array.Empty<string>(), Array.Empty<string>());
			}
		}

		

		protected virtual void CSAttributeDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CSAttributeDetail;
			if(row != null)
			{
				CSAnswers ans = PXSelect<CSAnswers, 
					Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>, 
						And<CSAnswers.value, Equal<Required<CSAnswers.value>>>>>.
					SelectWindowed(this, 0, 1, row.AttributeID, row.ValueID);
				CSAttributeGroup group = PXSelect<CSAttributeGroup, 
					Where<CSAttributeGroup.attributeID, Equal<Required<CSAttribute.attributeID>>>>.
					SelectWindowed(this, 0, 1, row.AttributeID);
				if (ans != null && group != null)
					throw new PXSetPropertyException<CSAttributeDetail.attributeID>(Messages.AttributeDetailCanNotBeDeletedAsItUsed, PXErrorLevel.RowError);
			}
		}

		protected virtual void CSAttributeDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CSAttributeDetail row = e.Row as CSAttributeDetail;

			if (row != null && CurrentAttribute.Current != null)
			{
				row.AttributeID = CurrentAttribute.Current.AttributeID;
			}
		}

		protected virtual void CSAttribute_ControlType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetControlsState(e.Row as CSAttribute, sender);
		}

		protected virtual void CSAttributeDetail_ValueID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as CSAttributeDetail;
		    if (row == null) return;

            if (e.NewValue != null && e.NewValue.Equals(row.ValueID)) return;

			CSAnswers ans = PXSelect<CSAnswers,
				Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
					And<CSAnswers.value, Equal<Required<CSAnswers.value>>>>>.
				SelectWindowed(this, 0, 1, row.AttributeID, row.ValueID);
			CSAttributeGroup group = PXSelect<CSAttributeGroup,
				Where<CSAttributeGroup.attributeID, Equal<Required<CSAttribute.attributeID>>>>.
				SelectWindowed(this, 0, 1, row.AttributeID);
			if (ans != null && group != null)
				throw new PXSetPropertyException<CSAttributeDetail.valueID>(Messages.AttributeDetailIdCanNotBeChangedAsItUsed, PXErrorLevel.Error);
		}

		protected virtual void CSAttribute_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CSAttribute item = (CSAttribute)e.Row;
			CSAttribute olditem = (CSAttribute)e.OldRow;
			if (item.ControlType != CSAttribute.Text && olditem.ControlType == CSAttribute.Text)
			{
				item.RegExp = null;
				item.EntryMask = null;
			}
		}

		protected virtual void CSAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CSAttribute row = e.Row as CSAttribute;
			if (row != null)
			{
				if ( string.IsNullOrEmpty(row.Description))
				{
					if (sender.RaiseExceptionHandling<CSAttribute.description>(e.Row, row.Description, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(CSAttribute.description).Name)))
					{
						throw new PXSetPropertyException(typeof(CSAttribute.description).Name, null, Data.ErrorMessages.FieldIsEmpty, typeof(CSAttribute.description).Name);
					}
				}
				if (!ValidateAttributeID(sender, row))
				{
					var displayName = ((PXFieldState)sender.GetStateExt(row, typeof(CSAttribute.attributeID).Name)).DisplayName;
					if (string.IsNullOrEmpty(displayName)) displayName = typeof(CSAttribute.attributeID).Name;
					throw new PXSetPropertyException(
						string.Concat(displayName, ": ", PXUIFieldAttribute.GetError<CSAttribute.attributeID>(sender, row)));
				}
				if (row.ControlType == CSAttribute.GISelector)
				{
					if (string.IsNullOrEmpty(row.ObjectName))
					{
						throw new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, "Schema Object");
					}
					if (string.IsNullOrEmpty(row.FieldName))
					{
						throw new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, "Schema Field");
					}
				}
			}
		}

		private static bool ValidateAttributeID(PXCache sender, CSAttribute row)
		{
			if (row == null || string.IsNullOrEmpty(row.AttributeID)) return true;

			if (Char.IsDigit(row.AttributeID[0]))
			{
				PXUIFieldAttribute.SetWarning<CSAttribute.attributeID>(sender, row, Messages.CannotStartWithDigit);
				return false;
			}

			if (row.AttributeID.Contains(" "))
			{
				PXUIFieldAttribute.SetWarning<CSAttribute.attributeID>(sender, row, Messages.CannotContainEmptyChars);
				return false;
			}

			return true;
		}        

        private void SetControlsState(CSAttribute row, PXCache cache)
		{
			if (row != null)
			{
				AttributeDetails.Cache.AllowDelete = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);
				AttributeDetails.Cache.AllowUpdate = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);
				AttributeDetails.Cache.AllowInsert = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);

                CSAnswers ans = PXSelect<CSAnswers, Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>.SelectWindowed(this, 0, 1, row.AttributeID);
                CSAttributeGroup group = null;
                if(ans == null)
                    group = PXSelect<CSAttributeGroup, Where<CSAttributeGroup.attributeID, Equal<Required<CSAttribute.attributeID>>>>.SelectWindowed(this, 0, 1, row.AttributeID);
                bool enabled = (ans == null && group == null);
                PXUIFieldAttribute.SetEnabled<CSAttribute.controlType>(cache, row, enabled);
                cache.AllowDelete = enabled;

				PXUIFieldAttribute.SetEnabled<CSAttribute.entryMask>(cache, row, row.ControlType == CSAttribute.Text);
				PXUIFieldAttribute.SetEnabled<CSAttribute.regExp>(cache, row, row.ControlType == CSAttribute.Text);
			}
		}

		public override void Persist()
		{
			if (Attributes.Current != null)
			{
				if (!PXDBLocalizableStringAttribute.IsEnabled)
				{
					string old = Attributes.Current.List;
					Attributes.Current.List = null;
					foreach (CSAttributeDetail det in AttributeDetails.Select())
					{
						if (!String.IsNullOrEmpty(det.ValueID))
						{
							if (Attributes.Current.List == null)
							{
								Attributes.Current.List = det.ValueID + '\0' + det.Description ?? "";
							}
							else
							{
								Attributes.Current.List = Attributes.Current.List + '\t' + det.ValueID + '\0' + det.Description ?? "";
							}
						}
					}
					if (!String.Equals(old, Attributes.Current.List) && Attributes.Cache.GetStatus(Attributes.Current) == PXEntryStatus.Notchanged)
					{
						Attributes.Cache.SetStatus(Attributes.Current, PXEntryStatus.Updated);
					}
				}
				else
				{
					bool isImport = IsImport;
					bool isExport = IsExport;
					bool isCopyPasteContext = IsCopyPasteContext;
					IsImport = false;
					IsExport = false;
					IsCopyPasteContext = false;
					try
					{
						string[] languages = Attributes.Cache.GetValueExt(null, "ListTranslations") as string[];
						if (languages != null)
						{
							languages = new string[languages.Length];
							foreach (CSAttributeDetail det in AttributeDetails.Select())
							{
								if (!String.IsNullOrEmpty(det.ValueID))
								{
									string[] translations = AttributeDetails.Cache.GetValueExt(det, "DescriptionTranslations") as string[];
									for (int i = 0; i < languages.Length; i++)
									{
										if (languages[i] == null)
										{
											languages[i] = det.ValueID;
										}
										else
										{
											languages[i] = languages[i] + '\t' + det.ValueID;
										}
										string descr = det.Description;
										if (translations != null && translations.Any(_ => !String.IsNullOrEmpty(_)))
										{
											if (i < translations.Length && !String.IsNullOrEmpty(translations[i]))
											{
												descr = translations[i];
											}
											else
											{
												descr = translations.FirstOrDefault(_ => !String.IsNullOrEmpty(_));
											}
										}
										languages[i] = languages[i] + '\0' + descr ?? "";
									}
								}
							}
							Attributes.Cache.SetValueExt(Attributes.Current, "ListTranslations", languages);
						}
					}
					finally
					{
						IsImport = isImport;
						IsExport = isExport;
						IsCopyPasteContext = isCopyPasteContext;
					}
				}
			}
			base.Persist();
		}
	}
}
