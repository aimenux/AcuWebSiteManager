using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using PX.Data;
using PX.Common;
using PX.Data.Update;
using System.Reflection;
using PX.Data.Update.ExchangeService;
using PX.Data.Update.WebServices;
using Address = PX.Objects.CR.Address;
using PX.Data.RichTextEdit;
using System.Net.Mail;

namespace PX.Objects.CS.Email
{
	#region ExchangeBaseSyncCommand
	public abstract class ExchangeBaseSyncCommand : IDisposable
	{
		protected string OperationCode;
		protected MicrosoftExchangeSyncProvider Provider;
		protected Dictionary<string, List<string>> Errors = new Dictionary<string, List<string>>();

		public EMailSyncServer Account { get { return Provider.Account; } }
		public EMailSyncPolicy Policy { get { return Provider.Policy; } }
		public PXSyncCache Cache { get { return Provider.Cache; } }
		public PXExchangeServer Gate { get { return Provider.Gate; } }

		protected ExchangeBaseSyncCommand(MicrosoftExchangeSyncProvider provider, string operationCode)
		{
			Provider = provider;
			OperationCode = operationCode;
		}
		public virtual void Dispose()
		{ }

		#region Interface
		public abstract void ProcessSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);
		#endregion

		#region Support
		protected EMailSyncAccount GetSyncAccount(PXGraph graph, int? accID)
		{
			EMailSyncAccount result = PXSelectReadonly<EMailSyncAccount,
				Where<EMailSyncAccount.emailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>>
				.SelectSingleBound(graph, null, accID);
			return result;
		}

		protected void SetSyncAccount(PXGraph graph, int? accID, string operation, PXEmailSyncDirection.Directions direction, DateTime? date, string folderID, bool? hasErrors)
		{
			if (accID == null)
				return;
			
			EMailSyncAccount row = GetSyncAccount(graph, accID);
			if (row == null)
				return;

			PXCache cache = graph.Caches[typeof(EMailSyncAccount)];


			if (direction == PXEmailSyncDirection.Directions.Full)
			{
				if (date != null)
				{
					cache.SetValue(row, operation + PXEmailSyncDirection.Directions.Export + "Date", date ?? PXTimeZoneInfo.Now);
					cache.SetValue(row, operation + PXEmailSyncDirection.Directions.Import + "Date", date ?? PXTimeZoneInfo.Now);
				}

				if (folderID != null)
				{
					cache.SetValue(row, operation + PXEmailSyncDirection.Directions.Export + "Folder", folderID);
					cache.SetValue(row, operation + PXEmailSyncDirection.Directions.Import + "Folder", folderID);
				}
			}
			else
			{
				if (date != null)
					cache.SetValue(row, operation + direction + "Date", date);

				if (folderID != null)
					cache.SetValue(row, operation + direction + "Folder", folderID);
			}

			if (hasErrors == true)
				row.HasErrors = true;

			row.ToReinitialize = false;
			row.IsReset = false;

			cache.Update(row);
			cache.Persist(PXDBOperation.Insert);
			cache.Persist(PXDBOperation.Update);
			graph.Clear();
        }
		protected DateTime? GetDateTime(PXEmailSyncDirection.Directions direction, PXSyncMailbox mailbox)
		{
			DateTime? result;
			switch (direction)
			{
				case PXEmailSyncDirection.Directions.Export:
					result = mailbox.ExportPreset.Date;
					if (result == null) result = new DateTime(1901, 1, 1);
					break;
				case PXEmailSyncDirection.Directions.Import:
					result = mailbox.ImportPreset.Date;
					break;
				default:
					throw new Exception(Messages.WrongSyncDirection);
			}
			if (result != null)
			{
				if (result < new DateTime(1901, 1, 1)) result = new DateTime(1901, 1, 1);
				result = PXTimeZoneInfo.ConvertTimeToUtc(result.Value, LocaleInfo.GetTimeZone());
			}
			return result;
		}
		protected DateTime? GetDateTime(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			DateTime? result;
			switch (direction)
			{
				case PXEmailSyncDirection.Directions.Export:
					result = mailboxes.Any(m => m.ExportPreset.Date == null) ? null : mailboxes.Min(m => m.ExportPreset.Date);
					if (result == null) result = new DateTime(1901, 1, 1);
					break;
				case PXEmailSyncDirection.Directions.Import:
					result = mailboxes.Any(m => m.ImportPreset.Date == null) ? null : mailboxes.Min(m => m.ImportPreset.Date);
					break;
				default:
					throw new Exception(Messages.WrongSyncDirection);
			}
			if (result != null)
			{
				if (result < new DateTime(1901, 1, 1)) result = new DateTime(1901, 1, 1);
				result = PXTimeZoneInfo.ConvertTimeToUtc(result.Value, LocaleInfo.GetTimeZone());
			}
			return result;
		}
		protected DateTime ConvertDateTime(DateTime? acuDate)
		{
			if (acuDate == null) return new DateTime(1950, 1, 1);
			return PXTimeZoneInfo.ConvertTimeToUtc(acuDate.Value, LocaleInfo.GetTimeZone());
		}


		protected void PostSyncHandling(PXGraph graph, string operationCode, PXEmailSyncDirection.Directions direction, DateTime date, IEnumerable<PXSyncMailbox> mailboxes, IEnumerable<PXSyncResult> processed)
		{
			int allItems = 0, failedItems = 0;
			foreach (PXSyncResult item in processed)
			{
				allItems++;
				if (!item.Success)
				{
					failedItems++;

					string text;
					text = PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncItemError, operationCode, direction.ToString(), item.DisplayKey);
					text = Provider.CreateErrorMessage(false, text, item.Message, item.Error, item.Details);

					StoreError(item.Address, text);
				}
			}
			
			foreach (PXSyncMailbox mailbox in mailboxes)
			{
				bool hasErrors = HasErrors(mailbox.Address);
				if (Policy.SkipError == true || !hasErrors)
					SetSyncAccount(graph, mailbox.EmailAccountID, operationCode, direction, date, null, hasErrors);
			}
			
			Provider.LogInfo(null, Messages.EmailExchangeSyncResult, direction.ToString(), operationCode, mailboxes.Count(), allItems, failedItems);
		}

		protected PXSyncResult SafeOperation(string mailbox, string id, Guid? note, string key, string title, PXSyncItemStatus status, Action action, bool logOperation)
		{
			return SafeOperation(PXEmailSyncDirection.Directions.Import, mailbox, id, note, key, title, status, action, logOperation);
		}
		protected PXSyncResult SafeOperation(PXEmailSyncDirection.Directions direction, string mailbox, string id, Guid? note, string key, string title, PXSyncItemStatus status, Action action, bool logOperation)
		{
			PXSyncResult result;
			try
			{
				PXTimeTagAttribute.SyncScope scope = null;
				try
				{
					scope = new PXTimeTagAttribute.SyncScope();

					action();
				}
				finally
				{
					if (scope != null) scope.Dispose();
				}
				result = new PXSyncResult(OperationCode, direction, mailbox, id, note, key) { ActionTitle = title, ItemStatus = status };
			}
			catch (Exception ex)
			{
				result = new PXSyncResult(OperationCode, direction, mailbox, id, note, key, null, ex, null) { ActionTitle = title, ItemStatus = status };
			}

			if (logOperation) Provider.LogResult(result);
			return result;
		}
		protected PXSyncResult SafeOperation<ExchangeType>(PXExchangeResponce<ExchangeType> item, string id, Guid? note, string key, string title, PXSyncItemStatus status, Action success, Func<bool> failed = null)
			where ExchangeType : ItemType, new()
		{
			if (item?.Item?.ItemId != null)
				id = item.Item.ItemId.Id;

			if (item.Success) return SafeOperation(PXEmailSyncDirection.Directions.Export, item.Address, id, note, key, title, status, success, true);

			PXSyncResult result = null;
			bool bypassUnsuccessfull = false;
			if (failed != null)
			{
				result = SafeOperation(PXEmailSyncDirection.Directions.Export, item.Address, id, note, key, title, status, () => bypassUnsuccessfull = failed(), false);
			}

			result = bypassUnsuccessfull ? result : new PXSyncResult(OperationCode, PXEmailSyncDirection.Directions.Export, item.Address, id, note, key, item.Message, null, item.Details) { ActionTitle = title, ItemStatus = status };
			if (!bypassUnsuccessfull) Provider.LogResult(result);
			return result;
		}

		protected string ColumnByLambda<T, V>(Expression<Func<T, V>> exp)
		{
			string name = String.Empty;
			if (exp.Body is MemberExpression) name = ((MemberExpression)exp.Body).Member.Name;
			if (exp.Body is UnaryExpression) name = ((MemberExpression)(((UnaryExpression)exp.Body).Operand)).Member.Name;

			return name;
		}
		protected void MergeErrors(Dictionary<string, List<string>> dictionary)
		{
			foreach (KeyValuePair<string, List<string>> pair in dictionary)
			{
				List<string> errors;
				if (!Errors.TryGetValue(pair.Key, out errors))
					Errors[pair.Key] = errors = new List<string>();
				errors.AddRange(pair.Value);
			}
		}
		protected void StoreError(string address, Exception error)
		{
			StoreError(address, error.ToString());
		}
		protected void StoreError(string address, string error)
		{
			List<string> list;
			if (!Errors.TryGetValue(address, out list))
				Errors[address] = list = new List<string>();
			list.Add(error);
		}
		protected bool HasErrors(string address)
		{
			return Errors.ContainsKey(address);
		}
		#endregion

		#region Converting
		protected virtual void ExportInsertedItemProperty<T>(Expression<Func<T, Object>> exp, T item, object value, string exchTimezone = null, bool isAllDay = false)
			where T : ItemType, new()
		{
			if (value == null) return;

			string fieldName = ColumnByLambda(exp);
			Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> fieldInfo = Cache.FieldsCache(typeof(T), fieldName);

			if (value is DateTime)
			{
				PXTimeZoneInfo tzdest = PXTimeZoneInfo.FindSystemTimeZoneById(exchTimezone);
				
				if (isAllDay)
				{
					// move time to compensate the exchange user's timezone offset 
				// actually not to UTC: to the reversed tzdest
					value = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)value, tzdest ?? LocaleInfo.GetTimeZone());
				}

				if (fieldInfo.Item3 != null)
				{
					TimeZoneDefinitionType tzdef = tzdest == null ? null : Gate.GetTimeZone(tzdest.RegionId);

					if (tzdef != null)
					{
						value = PXTimeZoneInfo.ConvertTimeFromUtc((DateTime)value, tzdest);
						fieldInfo.Item3.SetValue(item, tzdef);
					}
				}
			}

			if (fieldInfo.Item1 != null)
				fieldInfo.Item1.SetValue(item, value);

			if (fieldInfo.Item2 != null)
				fieldInfo.Item2.SetValue(item, true);
		}
		protected virtual void ExportInsertedItemPropertyConditional<T>(Expression<Func<T, Object>> exp, T item, object value, Object condition)
			where T : ItemType, new()
		{
			if (condition == null) return;

			ExportInsertedItemProperty<T>(exp, item, value, null);
		}

		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemProperty<T>(Expression<Func<T, object>> exp, UnindexedFieldURIType uri, object value, string exchTimezone = null, bool isAllDay = false)
			where T : ItemType, new()
		{
			return ExportUpdatedItemProperty<T>(exp, new PathToUnindexedFieldType() { FieldURI = uri }, value, exchTimezone, isAllDay);
		}
		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemPropertyConditional<T>(Expression<Func<T, object>> exp, UnindexedFieldURIType uri, object value, object condition)
			where T : ItemType, new()
		{
			if (condition == null) return null;

			return ExportUpdatedItemProperty<T>(exp, new PathToUnindexedFieldType() { FieldURI = uri }, value);
		}
		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemProperty<T>(Expression<Func<T, object>> exp, DictionaryURIType uri, string fieldIndex, object value, object condition)
			where T : ItemType, new()
		{
			if (condition == null) return null;

			return ExportUpdatedItemProperty<T>(exp, new PathToIndexedFieldType() { FieldURI = uri, FieldIndex = fieldIndex }, value);
		}
		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemProperty<T>(Expression<Func<T, object>> exp, string tag, MapiPropertyTypeType type, object value, object condition)
			where T : ItemType, new()
		{
			if (condition == null) return null;

			return ExportUpdatedItemProperty<T>(exp, new PathToExtendedFieldType() { PropertyTag = tag, PropertyType = type }, value);
		}
		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemProperty<T>(Expression<Func<T, object>> exp, string tag, MapiPropertyTypeType type, object value)
			where T : ItemType, new()
		{
			if (value == null) return null;

			return ExportUpdatedItemProperty<T>(exp, new PathToExtendedFieldType() { PropertyTag = tag, PropertyType = type }, PXExchangeConversionHelper.GetExtendedProperties(Tuple.Create(tag, type, value)), null);
		}
		protected virtual IEnumerable<SetItemFieldType> ExportUpdatedItemProperty<T>(Expression<Func<T, object>> exp, BasePathToElementType uriPath, object value, string exchTimezone = null, bool isAllDay = false)
			where T : ItemType, new()
		{
			if (value == null) yield break;

			T item = new T();
			Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> fieldInfo = Cache.FieldsCache(typeof(T), ColumnByLambda(exp));

            if (value is DateTime)
			{
				PXTimeZoneInfo tzdest = PXTimeZoneInfo.FindSystemTimeZoneById(exchTimezone);

				if (isAllDay)
				{
					// move time to compensate the exchange user's timezone offset 
				// actually not to UTC: to the reversed tzdest
					value = PXTimeZoneInfo.ConvertTimeToUtc((DateTime) value, tzdest ?? LocaleInfo.GetTimeZone());
				}
				
				if (fieldInfo.Item3 != null)
				{
					TimeZoneDefinitionType tzdef = tzdest == null ? null : Gate.GetTimeZone(tzdest.RegionId);

					if (tzdef != null)
					{
						value = PXTimeZoneInfo.ConvertTimeFromUtc((DateTime)value, tzdest);

						UnindexedFieldURIType timezoneUri;
						UnindexedFieldURIType fieldUri = (uriPath as PathToUnindexedFieldType).FieldURI;
						if (Enum.TryParse<UnindexedFieldURIType>(fieldUri.ToString() + "TimeZone", out timezoneUri))
						{
							T timezoneItem = new T();
							fieldInfo.Item3.SetValue(timezoneItem, tzdef);
							yield return new SetItemFieldType() { Item = new PathToUnindexedFieldType() { FieldURI = timezoneUri }, Item1 = timezoneItem };
						}
					}
				}
			}

			if (fieldInfo.Item1 != null)
				fieldInfo.Item1.SetValue(item, value);

			if (fieldInfo.Item2 != null)
				fieldInfo.Item2.SetValue(item, true);

			yield return new SetItemFieldType() { Item = uriPath, Item1 = item };
		}

		protected virtual void ImportItemProperty<T, V>(Expression<Func<T, V>> exp, T item, Action<V> setter, bool merge = false, string exchTimezone = null, bool isAllDay = false)
			where T : ItemType, new()
		{
			string fieldName = ColumnByLambda(exp);
			Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> fieldInfo = Cache.FieldsCache(typeof(T), fieldName);

			if (fieldInfo.Item2 != null && !(bool)fieldInfo.Item2.GetValue(item)) return;

			object value = fieldInfo.Item1?.GetValue(item);

			if (value == null && merge) return;

			if (value is DateTime && isAllDay)
			{
				// move time to compensate the exchange user's timezone offset 
				PXTimeZoneInfo tzdest = PXTimeZoneInfo.FindSystemTimeZoneById(exchTimezone);

				// actually not from UTC: from the reversed tzdest
				value = PXTimeZoneInfo.ConvertTimeFromUtc((DateTime)value, tzdest ?? LocaleInfo.GetTimeZone());
			}
			setter((V)value);
		}
		
		protected virtual IEnumerable<DeleteItemFieldType> DeleteItemProperty<T>(Expression<Func<T, object>> exp, UnindexedFieldURIType uri)
			where T : ItemType, new()
		{
			return DeleteItemProperty<T>(exp, new PathToUnindexedFieldType() { FieldURI = uri });
		}
		protected virtual IEnumerable<DeleteItemFieldType> DeleteItemProperty<T>(Expression<Func<T, object>> exp, DictionaryURIType uri, string fieldIndex)
			where T : ItemType, new()
		{
			return DeleteItemProperty<T>(exp, new PathToIndexedFieldType() { FieldURI = uri, FieldIndex = fieldIndex });
		}
		protected virtual IEnumerable<DeleteItemFieldType> DeleteItemProperty<T>(Expression<Func<T, object>> exp, BasePathToElementType uriPath)
			where T : ItemType, new()
		{
			yield return new DeleteItemFieldType() { Item = uriPath };
		}
		#endregion
	}
	#endregion

	#region ExchangeBaseLogicSyncCommand
	public abstract class ExchangeBaseLogicSyncCommand<GraphType, TPrimary, ExchangeType> : ExchangeBaseSyncCommand
		where GraphType : PXGraph, new()
		where TPrimary : IBqlTable, new()
		where ExchangeType : ItemType, new()
	{
		#region Consts
		protected const string BrokenImageBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAOCAYAAAAmL5yKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAADNSURBVDhPYwCC/7HVB/+Hl+3/7527+79z2vb/9slbcWLL2I3/DcNW/tcJWPwfpJchueHI/wcvP//XD9uAF8PUINNavnP/M6Q2HUFRSArW9J71nyG66uD/y/ffk4xBLtDwmvafIbh4H1jgy/ffJGGQAeruk/4zBBTuJdsANbcJw8IA/4I9FBjQ/58BlPrINUDVpec/g3vWTrINUHZo/s/glLodzCEHK9s3gfIDw3+ruA3/jcJXATPIov9aPnPASRQfVnXp/q/i2AbUzPAfAPWz01dc928mAAAAAElFTkSuQmCC";
		protected const string ContentIdPrefix = "ac_file_id_";
		//This prefix not works with Exchange Server 2016
		protected const string ContentIdPrefixOld = "ac_file_id:";
		#endregion

		protected GraphType graph = PXGraph.CreateInstance<GraphType>();

		protected PXExchangeFindOptions DefFindOptions;
		protected bool Attachments { get { return DefFindOptions.HasFlag(PXExchangeFindOptions.IncludeAttachments); } }

		private UploadFileMaintenance uploader;
		protected UploadFileMaintenance Uploader
		{
			get
			{
				if (uploader == null)
				{
					uploader = new UploadFileMaintenance();
					uploader.IgnoreFileRestrictions = true;
				}
				return uploader;
			}
		}

		protected ExchangeBaseLogicSyncCommand(MicrosoftExchangeSyncProvider provider, string operationCode, PXExchangeFindOptions findOption)
			: base(provider, operationCode)
		{
			DefFindOptions = findOption;
			GraphType graph = PXGraph.CreateInstance<GraphType>();
		}

		protected void EnsureEnvironmentConfigured<T>(IEnumerable<PXSyncMailbox> mailboxes, params PXSyncFolderSpecification[] folders)
			where T : BaseFolderType, new()
		{
			Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

			for (int i = 0; i < mailboxes.Count(); i += Gate.ProcessPackageSize)
			{
				var enumerator = Gate.GetUsersTimeZones(mailboxes.Skip(i).Take(Gate.ProcessPackageSize).Select(_ => _.Address)).GetEnumerator();
				
				foreach (PXSyncMailbox mailbox in mailboxes.Skip(i).Take(Gate.ProcessPackageSize))
				{
					try
					{
						bool hasValue = enumerator.MoveNext();

						if (mailbox.Reinitialize || (mailbox.ExportPreset.Date == null && mailbox.ImportPreset.Date == null))
						{
							CategoryColor color;
							if (!Enum.TryParse(Policy.Color, out color))
								color = CategoryColor.Black;

							Gate.EnsureCategory(mailbox.Address, Policy.Category, color);
						}

						if (hasValue)
						{
							if (enumerator.Current == -1)
								throw new Exception(PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncInitError, mailbox.Address));

							mailbox.ExchangeTimeZone = PXTimeZoneInfo.FindSystemTimeZoneByOffset(-enumerator.Current * 60).Id;
						}
					}
					catch (Exception ex)
					{
						List<string> list;
						if (!errors.TryGetValue(mailbox.Address, out list))
							errors[mailbox.Address] = list = new List<string>();
						string error = Provider.CreateErrorMessage(true, PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncMailboxError, mailbox.Address), null, ex, null);
						Provider.LogError(mailbox.Address, ex, error);
						list.Add(error);
					}
				}
			}
			//if (errors.Count > 0) throw new PXExchangeSyncItemsException(errors);

			Func<PXSyncMailbox, IEnumerable<PXExchangeFolderDefinition>> multiplicator = delegate(PXSyncMailbox m)
			{
				bool singleFolder = folders.Count(f => !String.IsNullOrEmpty(f.Name)) == 1;
				bool singleImport = folders.Count(f => !String.IsNullOrEmpty(f.Name) && f.Direction.HasFlag(PXEmailSyncDirection.Directions.Import)) == 1;
				bool singleExport = folders.Count(f => !String.IsNullOrEmpty(f.Name) && f.Direction.HasFlag(PXEmailSyncDirection.Directions.Export)) == 1;

				List<PXExchangeFolderDefinition> result = new List<PXExchangeFolderDefinition>();
				if (!errors.ContainsKey(m.Address))
				{
					foreach (PXSyncFolderSpecification f in folders)
					{
						if (String.IsNullOrEmpty(f.Name)) 
							result.Add(new PXExchangeFolderDefinition(m.Address, f.Type, f.Name, null, Tuple.Create(m, f)));
						else if (singleFolder) 
							result.Add(new PXExchangeFolderDefinition(m.Address, f.Type, f.Name, m.ImportPreset.FolderID, Tuple.Create(m, f)));
						else if (singleImport && f.Direction.HasFlag(PXEmailSyncDirection.Directions.Import)) 
							result.Add(new PXExchangeFolderDefinition(m.Address, f.Type, f.Name, m.ImportPreset.FolderID, Tuple.Create(m, f)));
						else if (singleExport && f.Direction.HasFlag(PXEmailSyncDirection.Directions.Export)) 
							result.Add(new PXExchangeFolderDefinition(m.Address, f.Type, f.Name, m.ExportPreset.FolderID, Tuple.Create(m, f)));
						else 
							result.Add(new PXExchangeFolderDefinition(m.Address, f.Type, f.Name, null, Tuple.Create(m, f)));
					}
				}
				return result;
			};

			IEnumerable<PXExchangeFolderDefinition> definitions = mailboxes.SelectMany(multiplicator);
			foreach (PXOperationResult<PXExchangeFolderID> result in Gate.EnsureFolders<T>(definitions))
			{
				PXSyncMailbox mailbox = (result.Tag as Tuple<PXSyncMailbox, PXSyncFolderSpecification>)?.Item1;
				PXSyncFolderSpecification spec = (result.Tag as Tuple<PXSyncMailbox, PXSyncFolderSpecification>)?.Item2;
				if (spec == null || mailbox == null) continue;

				string foldername = spec.Type.ToString();
				if (!String.IsNullOrEmpty(spec.Name)) foldername += "\\" + spec.Name;

				if (result.Success)
				{
					PXExchangeFolderID folder = result.Result;

					List<PXSyncMovingCondition> movingRules = new List<PXSyncMovingCondition>();
					foreach (PXSyncMovingCondition moveTo in spec.MoveTo)
					{
						if (String.IsNullOrEmpty(spec.Name))
							continue;

						PXOperationResult<PXExchangeFolderID> subres = Gate.EnsureFolders<T>(new PXExchangeFolderDefinition(mailbox.Address, moveTo.ParentFolder, null, null)).FirstOrDefault();
						if (subres?.Result == null || (subres.Result.FolderID is FolderIdType)) continue;

						movingRules.AddIfNotEmpty(moveTo.InitialiseFolder(subres.Result.FolderID));
					}

					mailbox.Folders.Add(new PXSyncDirectFolder(folder.Address, folder.FolderID, spec.Direction, spec.Categorized, (movingRules.Count > 0) ? movingRules.ToArray() : null));
					
					if (folder.NeedUpdate)
					{
						var ID = folder.FolderID as FolderIdType;
						SetSyncAccount(graph, mailbox.EmailAccountID, OperationCode, spec.Direction, null, ID?.Id, null);
					}
				}
				else
				{
					List<string> list;
					if (!errors.TryGetValue(mailbox.Address, out list))
						errors[mailbox.Address] = list = new List<string>();
					string error = Provider.CreateErrorMessage(true, PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncFolderError, foldername, mailbox.Address), null, result.Error, null);
					Provider.LogError(mailbox.Address, result.Error, error);
					list.Add(error);
				}
			}

			if (errors.Count > 0) throw new PXExchangeSyncItemsException(errors);
		}

		#region Specific

		protected void DeleteReference(string address, Guid? noteID)
		{
			if (noteID == null) throw new PXException(Messages.EmailExchangeNoteIsNull);

			bool exist = false;
			foreach (PXDataRecord rec in PXDatabase.SelectMulti<EMailSyncReference>(
				new PXDataField<EMailSyncReference.serverID>(),
				new PXDataFieldValue<EMailSyncReference.serverID>(Account.AccountID),
				new PXDataFieldValue<EMailSyncReference.address>(address),
				new PXDataFieldValue<EMailSyncReference.noteID>(noteID)))
			{
				exist = true;
				break;
			}

			if (exist)
			{
				List<PXDataFieldRestrict> list = new List<PXDataFieldRestrict>();

				list.Add(new PXDataFieldRestrict<EMailSyncReference.serverID>(Account.AccountID));
				list.Add(new PXDataFieldRestrict<EMailSyncReference.address>(address));
				list.Add(new PXDataFieldRestrict<EMailSyncReference.noteID>(noteID));

				PXDatabase.Delete<EMailSyncReference>(list.ToArray());
			}
		}
		protected void SaveReference(string address, Guid? noteID, ItemIdType itemID, ItemIdType conversationID, string hash, bool? isSynchronized)
		{
			if (noteID == null) throw new PXException(Messages.EmailExchangeNoteIsNull);

			bool exist = false;
			foreach (PXDataRecord rec in PXDatabase.SelectMulti<EMailSyncReference>(
				new PXDataField<EMailSyncReference.binaryReference>(),
				new PXDataFieldValue<EMailSyncReference.serverID>(Account.AccountID),
				new PXDataFieldValue<EMailSyncReference.address>(address),
				new PXDataFieldValue<EMailSyncReference.noteID>(noteID)))
			{
				exist = true;
				break;
			}

			if (exist)
			{
				List<PXDataFieldParam> list = new List<PXDataFieldParam>();

				list.Add(new PXDataFieldAssign<EMailSyncReference.binaryReference>(itemID != null ? Encoding.UTF8.GetBytes(itemID.Id) : new byte[] { }));
				list.Add(new PXDataFieldAssign<EMailSyncReference.binaryChangeKey>(itemID != null ? Encoding.UTF8.GetBytes(itemID.ChangeKey) : new byte[] { }));
				if (hash != null) 
				list.Add(new PXDataFieldAssign<EMailSyncReference.hash>(hash));
				if (itemID == null || conversationID != null)
					list.Add(new PXDataFieldAssign<EMailSyncReference.conversation>(conversationID != null ? conversationID.Id : null));
				if (isSynchronized != null)
				list.Add(new PXDataFieldAssign<EMailSyncReference.isSynchronized>(isSynchronized));

				list.Add(new PXDataFieldRestrict<EMailSyncReference.serverID>(Account.AccountID));
				list.Add(new PXDataFieldRestrict<EMailSyncReference.address>(address));
				list.Add(new PXDataFieldRestrict<EMailSyncReference.noteID>(noteID));

				PXDatabase.Update<EMailSyncReference>(list.ToArray());
			}
			else
			{
				List<PXDataFieldAssign> list = new List<PXDataFieldAssign>();

				list.Add(new PXDataFieldAssign<EMailSyncReference.binaryReference>(itemID != null ? Encoding.UTF8.GetBytes(itemID.Id) : new byte[] { }));
				list.Add(new PXDataFieldAssign<EMailSyncReference.binaryChangeKey>(itemID != null ? Encoding.UTF8.GetBytes(itemID.ChangeKey) : new byte[] { }));
				list.Add(new PXDataFieldAssign<EMailSyncReference.conversation>(conversationID != null ? conversationID.Id : null));
				list.Add(new PXDataFieldAssign<EMailSyncReference.hash>(hash));
				list.Add(new PXDataFieldAssign<EMailSyncReference.serverID>(Account.AccountID));
				list.Add(new PXDataFieldAssign<EMailSyncReference.address>(address));
				list.Add(new PXDataFieldAssign<EMailSyncReference.noteID>(noteID));
				list.Add(new PXDataFieldAssign<EMailSyncReference.isSynchronized>(isSynchronized));

				PXDatabase.Insert<EMailSyncReference>(list.ToArray());
			}
		}
		protected bool ValidateItemHash(PXExchangeItem<ExchangeType> item)
		{
			if (item == null) throw new PXException(Messages.EmailExchangeNoteIsNull);
			if (DateTime.Now.Ticks > 0) return false;

			using (PXDataRecord rec = PXDatabase.SelectSingle<EMailSyncReference>(
				new PXDataField<EMailSyncReference.noteID>(),
				new PXDataField<EMailSyncReference.hash>(),
				new PXDataFieldValue<EMailSyncReference.serverID>(Account.AccountID),
				new PXDataFieldValue<EMailSyncReference.address>(item.Address),
				new PXDataFieldValue<EMailSyncReference.binaryReference>(item.Item.ItemId.Id)))
			{
				if (rec == null) return false;

				var reference = new EMailSyncReference();
				reference.ServerID = Account.AccountID;
				reference.Address = item.Address;
				reference.BinaryReference = item.Item.ItemId.Id;
				reference.NoteID = rec.GetGuid(0);
				reference.Hash = rec.GetString(1);

				bool result = reference.NoteID != null && reference.Hash != null && item.Hash == reference.Hash;
				if (result) Provider.LogInfo(item.Address, Messages.EmailExchangeHashIsnotChanged, item.Item.Subject);
				return result;
			}
		}
		
		protected virtual IEnumerable<PXExchangeItem<ExchangeType>> GetItems(IEnumerable<PXSyncMailbox> mailboxes, PXSyncItemSet exported, PXExchangeFindOptions options, params Tuple<string, MapiPropertyTypeType>[] extFields)
		{
			Func<PXExchangeItem<ExchangeType>, PXExchangeRequest<ExchangeType, ExchangeType>> converter = delegate(PXExchangeItem<ExchangeType> i)
			{
				return new PXExchangeRequest<ExchangeType, ExchangeType>(
					new PXExchangeFolderID(i.Address, i.Item.ParentFolderId), 
					new ExchangeType() { ItemId = i.Item.ItemId, Categories = i.Item.Categories }, 
					Guid.NewGuid().ToString(), 
					null,
					i.Attachments);
			};

			var ids = exported != null 
				? exported.ToHashSet()
				: new HashSet<string>();

			options = DefFindOptions | options;
			foreach (bool categorized in new bool[] { true, false })
			{
				DateTime? date = GetDateTime(PXEmailSyncDirection.Directions.Import, mailboxes);
				PXSyncDirectFolder[] folders = mailboxes.SelectMany(m => m.Folders.Where(f => f.Categorized == categorized && f.IsImport)).ToArray();
				if (folders == null || folders.Length <= 0) continue;


				List<PXExchangeRequest<ExchangeType, ExchangeType>> toCategorizing = new List<PXExchangeRequest<ExchangeType, ExchangeType>>();
				BasePathToElementType[] extPaths = extFields == null || extFields.Length <= 0 ? null : extFields.Select(e => new PathToExtendedFieldType() { PropertyTag = e.Item1, PropertyType = e.Item2 }).ToArray();
				foreach (PXExchangeItem<ExchangeType> item in Gate.FindItems<ExchangeType>(folders, PXExchangeFindOptions.Changed | options, categorized ? Policy.Category : null, date, extPaths, ids))
				{
					if (ids != null && ids.Contains(item.Item.ItemId.Id)) continue;
					if (ValidateItemHash(item)) continue;

					if (!categorized 
						&& ((options & PXExchangeFindOptions.HeadersOnly) != PXExchangeFindOptions.HeadersOnly)
						&& (item.Item.Categories == null || !item.Item.Categories.Contains(Policy.Category)))
					{
						toCategorizing.Add(converter(item));
						Provider.LogInfo(item.Address, Messages.EmailExchangeItemCategorizing, item.Item.Subject, Policy.Category);
					}
					ids.Add(item.Item.ItemId.Id);
					yield return item;
				}
				if (options.HasFlag(PXExchangeFindOptions.IncludeUncategorized) && (date != null))
				{
					foreach (PXExchangeItem<ExchangeType> item in Gate.FindItems<ExchangeType>(folders, PXExchangeFindOptions.Created | options, null, date, extPaths, ids))
					{
						if (ids != null && ids.Contains(item.Item.ItemId.Id)) continue;

						if (!ids.Contains(item.Item.ItemId.Id))
						{
							if (((options & PXExchangeFindOptions.HeadersOnly) != PXExchangeFindOptions.HeadersOnly)
								&& (item.Item.Categories == null || !item.Item.Categories.Contains(Policy.Category)))
							{
								toCategorizing.Add(converter(item));
								Provider.LogInfo(item.Address, Messages.EmailExchangeItemCategorizing, item.Item.Subject, Policy.Category);
							}

							ids.Add(item.Item.ItemId.Id);
							yield return item;
						}
					}
				}
				//categorizing new items
				if (toCategorizing.Count > 0)
				{
					Provider.LogVerbose(null, Messages.EmailExchangeCategorizing);
					Gate.CategorizeItems(Policy.Category, toCategorizing.ToArray()).ToArray();
				}
			}
		}

		protected virtual BqlCommand PrepareBqlCommand<TNote>(BqlCommand bqlCommand, PXSyncItemStatus status)
			where TNote : IBqlField
		{
			//Patching bql command
			bqlCommand = BqlCommand.AppendJoin<LeftJoin<EMailSyncReference,
				On<EMailSyncReference.noteID, Equal<TNote>, And<EMailSyncReference.serverID, Equal<Required<EMailSyncReference.serverID>>, And<EMailSyncReference.address, Equal<Required<EMailSyncReference.address>>>>>>>(bqlCommand);
			bqlCommand = BqlCommand.AppendJoin<LeftJoin<SyncTimeTag, On<SyncTimeTag.noteID, Equal<TNote>>>>(bqlCommand);

			switch (status)
			{
				case PXSyncItemStatus.Inserted:
					bqlCommand = bqlCommand.WhereAnd<
						Where<EMailSyncReference.binaryReference, IsNull>>();							// = no EMailSyncReference
					break;

				case PXSyncItemStatus.Updated:
					bqlCommand = bqlCommand.WhereAnd<
						Where2<Where<SyncTimeTag.timeTag, GreaterEqual<Required<SyncTimeTag.timeTag>>,
								Or<EMailSyncReference.isSynchronized, Equal<False>>>,
							And<EMailSyncReference.binaryReference, IsNotNull>>>();						// = some EMailSyncReference
					break;

				case PXSyncItemStatus.Unsynced:
					bqlCommand = bqlCommand.WhereAnd<
							Where<EMailSyncReference.isSynchronized, Equal<False>,
							And<EMailSyncReference.binaryReference, IsNotNull>>>();						// = some EMailSyncReference
					break;

				case PXSyncItemStatus.Deleted:
					bqlCommand = bqlCommand.WhereAnd<
						Where<EMailSyncReference.binaryReference, NotEqual<Empty>>>();					// = some EMailSyncReference and filled ref

					if (typeof(TPrimary) != typeof(Contact))
					{
						var type = typeof (TPrimary).GetNestedType(typeof (CRActivity.deletedDatabaseRecord).Name);

						bqlCommand = bqlCommand.WhereAnd(BqlCommand.Compose(
							typeof (Where<,>), type, typeof (Equal<>), typeof (True)
							));
					}

					break;
			}

			return bqlCommand;
		}
		protected virtual List<Func<PXSyncMailbox, Object>> PrepareParameters(BqlCommand bqlCommand)
		{
			List<Func<PXSyncMailbox, Object>> args = new List<Func<PXSyncMailbox, Object>>();

			IBqlParameter[] parameters = bqlCommand.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				Type type = parameters[i].GetReferencedType();
				if (String.Equals(type.Name, "ServerID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Account.AccountID);
				else if (String.Equals(type.Name, "Address", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => m.Address);
				else if (String.Equals(type.Name, "TimeTag", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => GetDateTime(PXEmailSyncDirection.Directions.Export, m));
				else if (String.Equals(type.Name, "Owner", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Cache.EmployeeCache[m.EmployeeID]);
				else if (String.Equals(type.Name, "OwnerID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Cache.EmployeeCache[m.EmployeeID]);
				else if (String.Equals(type.Name, "User", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Cache.EmployeeCache[m.EmployeeID]);
				else if (String.Equals(type.Name, "UserID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Cache.EmployeeCache[m.EmployeeID]);
				else if (String.Equals(type.Name, "WorkgroupID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => Cache.EmployeeCache[m.EmployeeID]);
				else if (String.Equals(type.Name, "BAccountID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => m.EmployeeID);
				else if (String.Equals(type.Name, "EmailAccountID", StringComparison.InvariantCultureIgnoreCase)) args.Add(m => m.EmailAccountID);
			}

			return args;
		}

		protected virtual IEnumerable<PXSyncItemBucket<TPrimary>> SelectItems<TNote>(PXGraph graph, BqlCommand bqlCommand, IEnumerable<PXSyncMailbox> mailboxes,
			PXSyncItemSet exceptionSet, PXSyncItemStatus status, bool withAttachments)
			where TNote : IBqlField
		{
			return SelectItems<TNote, IBqlTable>(graph, bqlCommand, mailboxes, exceptionSet, status, withAttachments);
		}
		protected virtual IEnumerable<PXSyncItemBucket<TPrimary, T2>> SelectItems<TNote, T2>(PXGraph graph, BqlCommand bqlCommand, IEnumerable<PXSyncMailbox> mailboxes,
			PXSyncItemSet exceptionSet, PXSyncItemStatus status, bool withAttachments)
			where TNote : IBqlField
			where T2 : IBqlTable
		{
			return SelectItems<TNote, T2, IBqlTable>(graph, bqlCommand, mailboxes, exceptionSet, status, withAttachments);
		}
		protected virtual IEnumerable<PXSyncItemBucket<TPrimary, T2, T3>> SelectItems<TNote, T2, T3>(PXGraph graph, BqlCommand bqlCommand, IEnumerable<PXSyncMailbox> mailboxes,
			PXSyncItemSet exceptionSet, PXSyncItemStatus status, bool withAttachments)
			where TNote : IBqlField
			where T2 : IBqlTable
			where T3 : IBqlTable
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			Func<TPrimary, Guid?> getNote = row => PXNoteAttribute.GetNoteID(cache, row, null);
			
			bqlCommand = PrepareBqlCommand<TNote>(bqlCommand, status);
			if (bqlCommand == null) yield break;

			foreach (PXSyncItemBucket<TPrimary, T2, T3> bucket in SelectItems<T2, T3>(graph, bqlCommand, mailboxes, exceptionSet, status))
			{
				TPrimary primary = bucket.Item1;
				EMailSyncReference reference = bucket.Reference;
				
				List<UploadFileWithData> attachments = new List<UploadFileWithData>();
				if (withAttachments && bucket.Status != PXSyncItemStatus.Deleted)
				{
					Guid? noteID = getNote(primary);
					if (noteID != null)
					{
						foreach (UploadFileWithData row in PXSelect<UploadFileWithData, Where<UploadFileWithData.noteID, Equal<Required<UploadFileWithData.noteID>>>>.Select(graph, noteID))
						{
							if (row.Data == null) continue;
							int size = 1024 * 1024 * 20;
							if (Account.SyncAttachmentSize != null && Account.SyncAttachmentSize > 0)
								size = Account.SyncAttachmentSize.Value * 1024;
							if (row.Data.Length > size)
							{
								Provider.LogWarning(bucket.Mailbox.Address, PX.Data.Update.Messages.ExchangeAttachmentTooBig, row.Name, Account.SyncAttachmentSize);
								continue;
							}
							attachments.Add(row);
						}
					}
				}
				if (withAttachments && attachments.Count > 0) bucket.Attachments = attachments.ToArray();

				yield return bucket;
			}
		}

		protected virtual IEnumerable<PXSyncItemBucket<TPrimary, T2, T3>> SelectItems<T2, T3>(PXGraph graph, BqlCommand bqlCommand, IEnumerable<PXSyncMailbox> mailboxes, PXSyncItemSet exceptionSet, PXSyncItemStatus status)
			where T2 : IBqlTable
			where T3 : IBqlTable
		{
			//TODO Need review field getting
			PXCache cache = graph.Caches[typeof(TPrimary)];
			Func<TPrimary, bool> getSynced = delegate(TPrimary row)
			{
				Object value = cache.GetValue(row, "Synchronize");
				return value != null && value is bool && ((bool)value);
			};

			var ids = exceptionSet != null
				? exceptionSet.ToHashSet()
				: new HashSet<string>();

			List<Func<PXSyncMailbox, Object>> args = PrepareParameters(bqlCommand);
			foreach (PXSyncMailbox mailbox in mailboxes)
			{
				//evaluate arguments using current mailbox
				Object[] arguments = args.Select(a => a(mailbox)).ToArray();

				if ((status & PXSyncItemStatus.Inserted) == PXSyncItemStatus.Inserted || (status & PXSyncItemStatus.Updated) == PXSyncItemStatus.Updated)
				{
					foreach (PXResult result in ExecuteSelect(mailbox, new PXView(graph, true, bqlCommand), arguments))
					{
						TPrimary primary = result.GetItem<TPrimary>();
						EMailSyncReference reference = result.GetItem<EMailSyncReference>();

						if (ids != null && ids.Contains(reference.BinaryReference)) continue;

						//if item is not marked for processing, than skip it
						if (!getSynced(primary)) continue;

						PXSyncItemStatus itemStatus = reference == null || String.IsNullOrEmpty(reference.BinaryReference) ? PXSyncItemStatus.Inserted : PXSyncItemStatus.Updated;
						if (status != PXSyncItemStatus.None 
							&& status != PXSyncItemStatus.Unsynced
							&& ((itemStatus & status) != status)) continue;

						yield return new PXSyncItemBucket<TPrimary, T2, T3>(mailbox, itemStatus, reference, result.GetItem<TPrimary>(), result.GetItem<T2>(), result.GetItem<T3>());
					}
				}

				if ((status & PXSyncItemStatus.Deleted) == PXSyncItemStatus.Deleted)
				{
					using (new PXReadDeletedScope(typeof(TPrimary) != typeof(Contact)))
					{
						foreach (PXResult result in ExecuteSelect(mailbox, new PXView(graph, true, bqlCommand), arguments))
						{
							TPrimary primary = result.GetItem<TPrimary>();
							EMailSyncReference reference = result.GetItem<EMailSyncReference>();

							if (ids != null && ids.Contains(reference.BinaryReference)) continue;

							//if item is not marked for processing, than skip it
							if (!getSynced(primary)) continue;

							yield return new PXSyncItemBucket<TPrimary, T2, T3>(mailbox, PXSyncItemStatus.Deleted, reference, result.GetItem<TPrimary>(), result.GetItem<T2>(), result.GetItem<T3>());
						}
					}
				}
			}
		}

		protected IEnumerable<PXResult> ExecuteSelect(PXSyncMailbox mailbox, PXView view, Object[] arguments)
		{
			view.Clear();
			IEnumerable<Object> list = null;
			try
			{
				list = view.SelectMulti(arguments);
			}
			catch (Exception ex)
			{
				Provider.LogError(mailbox.Address, ex);
				StoreError(mailbox.Address, ex);
				yield break;
			}

			foreach (PXResult result in view.SelectMulti(arguments))
			{
				yield return result;
			}
		}
		#endregion

		#region Interface
		protected abstract BqlCommand GetSelectCommand();
		public override void ProcessSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			try
			{
				ConfigureEnvironment(direction, mailboxes);
			}
			catch (PXExchangeSyncItemsException ex)
			{
				if(ex.Errors == null || ex.Errors.Count <=0) 
					throw;
				MergeErrors(ex.Errors);
			}

			try
			{
				//filter failed mailboxes
				List<PXSyncMailbox> toProcess = new List<PXSyncMailbox>();
				foreach (PXSyncMailbox mailbox in mailboxes)
				{
					if (!Errors.ContainsKey(mailbox.Address))
						toProcess.Add(mailbox);
				}

				PrepareSync(policy, direction, toProcess.ToArray());
			}
			catch (PXExchangeSyncItemsException ex)
			{
				if (ex.Errors == null || ex.Errors.Count <= 0)
					throw;
				MergeErrors(ex.Errors);
			}

			if (Errors.Count > 0) throw new PXExchangeSyncItemsException(Errors);
		}

		protected abstract void ExportImportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes);

        protected virtual void ExportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes, PXSyncItemSet excludes = null)
		{
			if (!mailboxes.Any()) return;

			List<PXSyncResult> exported = null;
			if ((direction & PXEmailSyncDirection.Directions.Export) == PXEmailSyncDirection.Directions.Export)
			{
				Provider.LogVerbose(null, Messages.EmailExchangeOperationStarted, PXEmailSyncDirection.Directions.Export, OperationCode);

				DateTime date = PXTimeZoneInfo.Now;
				exported = PrepareExport(mailboxes, excludes);
				PostSyncHandling(graph, OperationCode, PXEmailSyncDirection.Directions.Export, date, mailboxes, exported);

				Provider.LogVerbose(null, Messages.EmailExchangeOperationFinished, PXEmailSyncDirection.Directions.Export, OperationCode);
			}

			System.Threading.Thread.Sleep(500);

			List<PXSyncResult> imported;
			if ((direction & PXEmailSyncDirection.Directions.Import) == PXEmailSyncDirection.Directions.Import)
			{
				Provider.LogVerbose(null, Messages.EmailExchangeOperationStarted, PXEmailSyncDirection.Directions.Import, OperationCode);

				DateTime date = PXTimeZoneInfo.Now;
				imported = PrepareImport(mailboxes, new PXSyncItemSet(exported));
				PostSyncHandling(graph, OperationCode, PXEmailSyncDirection.Directions.Import, date, mailboxes, imported);

				Provider.LogVerbose(null, Messages.EmailExchangeOperationFinished, PXEmailSyncDirection.Directions.Import, OperationCode);
			}
		}

		protected virtual void ImportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes, PXSyncItemSet excludes = null)
		{
			if (!mailboxes.Any()) return;

			List<PXSyncResult> imported = null;
			if ((direction & PXEmailSyncDirection.Directions.Import) == PXEmailSyncDirection.Directions.Import)
			{
				Provider.LogVerbose(null, Messages.EmailExchangeOperationStarted, PXEmailSyncDirection.Directions.Import, OperationCode);

				DateTime date = PXTimeZoneInfo.Now;
				imported = PrepareImport(mailboxes, excludes);
				PostSyncHandling(graph, OperationCode, PXEmailSyncDirection.Directions.Import, date, mailboxes, imported);

				Provider.LogVerbose(null, Messages.EmailExchangeOperationFinished, PXEmailSyncDirection.Directions.Import, OperationCode);
			}

			System.Threading.Thread.Sleep(500);

			List<PXSyncResult> exported;
			if ((direction & PXEmailSyncDirection.Directions.Export) == PXEmailSyncDirection.Directions.Export)
			{
				Provider.LogVerbose(null, Messages.EmailExchangeOperationStarted, PXEmailSyncDirection.Directions.Export, OperationCode);

				DateTime date = PXTimeZoneInfo.Now;
				exported = PrepareExport(mailboxes, new PXSyncItemSet(imported));
				PostSyncHandling(graph, OperationCode, PXEmailSyncDirection.Directions.Export, date, mailboxes, exported);

				Provider.LogVerbose(null, Messages.EmailExchangeOperationFinished, PXEmailSyncDirection.Directions.Export, OperationCode);
			}
		}

		protected abstract void LastUpdated(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes);

		protected abstract void KeepBoth(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes);

		protected virtual void PrepareSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			switch (policy.Priority)
			{
				case PXSyncPriority.ExchangeCode:

					ImportFirst(policy, direction, mailboxes);

					break;

				case PXSyncPriority.LastUpdatedCode:

					ImportFirst(policy, direction, mailboxes.Where(_ => _.IsReset).ToArray());
					LastUpdated(policy, direction, mailboxes.Where(_ => !_.IsReset).ToArray());

					break;

				case PXSyncPriority.KeepBothCode:

					ImportFirst(policy, direction, mailboxes.Where(_ => _.IsReset).ToArray());
					KeepBoth(policy, direction, mailboxes.Where(_ => !_.IsReset).ToArray());

					break;

				case PXSyncPriority.AcumaticaCode:
				default:

					ImportFirst(policy, direction, mailboxes.Where(_ => _.IsReset).ToArray());
					ExportImportFirst(policy, direction, mailboxes.Where(_ => !_.IsReset).ToArray());

					break;
			}
		}
		protected abstract void ConfigureEnvironment(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);
		
		protected virtual List<PXSyncResult> PrepareExport(PXSyncMailbox[] mailboxes, PXSyncItemSet imported = null)
		{
			List<PXSyncResult> processed = new List<PXSyncResult>();

			processed.AddRange(ProcessSyncExport(mailboxes.Where(m => m.ExportPreset.Date == null).ToArray(), imported));
			processed.AddRange(ProcessSyncExport(mailboxes.Where(m => m.ExportPreset.Date != null).ToArray(), imported));

			return processed;
		}
		protected virtual List<PXSyncResult> PrepareImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported = null)
		{
			List<PXSyncResult> processed = new List<PXSyncResult>();

			processed.AddRange(ProcessSyncImport(mailboxes.Where(m => m.ImportPreset.Date == null).ToArray(), exported));
			processed.AddRange(ProcessSyncImport(mailboxes.Where(m => m.ImportPreset.Date != null).ToArray(), exported));

			// Messages will be done after processing stage
			if (typeof(ExchangeType) != typeof(MessageType))
				processed.AddRange(ProcessSyncExportUnsynced(mailboxes, exported));

			return processed;
		}

		protected abstract IEnumerable<PXSyncResult> ProcessSyncExport(PXSyncMailbox[] mailboxes, PXSyncItemSet imported);
		protected abstract IEnumerable<PXSyncResult> ProcessSyncExportUnsynced(PXSyncMailbox[] mailboxes, PXSyncItemSet exported);
		protected abstract IEnumerable<PXSyncResult> ProcessSyncImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported);

		protected virtual bool IsFullyFilled(ExchangeType item)
		{
			return true;
		}
		#endregion

		#region Attachments
		protected virtual bool SaveAttachmentsSync(PXGraph graph, TPrimary row, AttachmentType[] attachments)
		{
			return SaveAttachmentsSync(graph, row, attachments, out _);
		}
		protected virtual bool SaveAttachmentsSync(PXGraph graph, TPrimary row, AttachmentType[] attachments, out string imgName)
		{
			imgName = null;
			bool containsNewFiles = false;
			if (attachments == null || attachments.Length <= 0) return containsNewFiles;

			List<Guid> docs = new List<Guid>();
			foreach (AttachmentType attachment in attachments)
			{
				Byte[] content = null;
				if (attachment is FileAttachmentType) content = ((FileAttachmentType)attachment).Content;
				if (content == null) continue;

				// do not create new attachment if there are existing one
				if (TryParseAcumaticaContentId(attachment.ContentId, out Guid fileId)
					&& CheckFileExistance(fileId, out var file)
					&& CompareBinaryData(file.BinData, content))
				{
					// do nothing if files are equal (but add it to files)
					docs.Add(file.UID.Value);
					continue;
				}

				// check file by name
				// WARNING: if email contains more than one file with the same name, only first would be processed
				string fileName = GetAttachmentName(graph, row, attachment);
				if (CheckFileExistance(fileName, out file)
					&& CompareBinaryData(file.BinData, content))
				{
					continue;
				}

				// otherwise create new attachment
				var newFile = new FileInfo(fileName, null, content);

				containsNewFiles = true;

				Uploader.SaveFile(newFile, FileExistsAction.CreateVersion);

				if (newFile.UID != null)
					docs.Add(newFile.UID.Value);

				if (attachment.ContentType != null && attachment.ContentType.Contains("image"))
					imgName = newFile.Name;
			}
			if (docs.Count > 0) PXNoteAttribute.SetFileNotes(graph.Caches[row.GetType()], row, docs.ToArray());

			return containsNewFiles;
		}

		protected virtual AttachmentType[] ConvertAttachment(UploadFileWithData[] files, string photoName = null)
		{
			if (files == null || files.Length <= 0) return null;

			AttachmentType[] attachmets = new AttachmentType[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				UploadFileWithData file = files[i];
				attachmets[i] = new FileAttachmentType
				{
					Name = GetAttachmentName(file.Name),
					Size = file.Size ?? 0,
					SizeSpecified = true,
					Content = file.Data,
					ContentId = ConvertToAcumaticaContentId(file.FileID),
					IsContactPhoto = file.Name == photoName,
					IsContactPhotoSpecified = true,
				};
			}
			return attachmets;
		}

		protected string ConvertToAcumaticaContentId(Guid? fileId)
		{
			return ContentIdPrefix + fileId?.ToString("D") ?? "no_file";
		}

		protected bool TryParseAcumaticaContentId(string contentId, out Guid fileId)
		{
			contentId = contentId?.Trim();
			if (string.IsNullOrEmpty(contentId)
				|| !contentId.StartsWith(ContentIdPrefix, StringComparison.InvariantCulture))
			{
				return TryParseAcumaticaContentIdOld(contentId, out fileId);
			}
			contentId = contentId.Substring(ContentIdPrefix.Length, contentId.Length - ContentIdPrefix.Length);
			if (Guid.TryParse(contentId, out fileId))
				return true;
			return false;
		}

		protected bool TryParseAcumaticaContentIdOld(string contentId, out Guid fileId)
		{
			if (string.IsNullOrEmpty(contentId)
				|| !contentId.StartsWith(ContentIdPrefixOld, StringComparison.InvariantCulture))
			{
				fileId = default(Guid);
				return false;
			}
			contentId = contentId.Substring(ContentIdPrefixOld.Length, contentId.Length - ContentIdPrefixOld.Length);
			if (Guid.TryParse(contentId, out fileId))
				return true;
			return false;
		}

		protected string GetAttachmentFileID(string name)
		{
			const string prefix = "~/Frames/GetFile.ashx?fileID=";
			if (!name.StartsWith(prefix))
				return null;
			name = name.Substring(prefix.Length);
			return name;
		}

		protected string GetAttachmentName(string name)
		{
			var slash = name.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
			if (slash >= 0 && slash < name.Length - 1)
				name = name.Substring(slash + 1);

			slash = name.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
			if (slash >= 0 && slash < name.Length - 1)
				name = name.Substring(slash + 1);

			return name;
		}
		protected string GetAttachmentName(PXGraph graph, TPrimary row, AttachmentType attachment)
		{
			string title = OperationCode;
			PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(graph.GetType());
			if (node != null) title = node.Title;

			IEnumerable<object> values = graph.Caches[typeof(TPrimary)].Keys.Select(key => graph.Caches[typeof(TPrimary)].GetValue(row, key)).Where(value => value != null);
			string prefix = String.Join(" ", values);
			string caption = String.Format("{0} ({1})\\", title, prefix);

			List<string> existing = new List<string>();
			PXNoteState state = graph.Caches[typeof(TPrimary)].GetValueExt(row, "NoteFiles") as PXNoteState;
			if (state != null && state.Value != null && state.Value is string[])
			{
				foreach (string s in (state.Value as string[]))
				{
					if (String.IsNullOrEmpty(s)) continue;
					string[] parts = s.Split(new char[] { '$' }, StringSplitOptions.None);
					if (parts.Length > 1) existing.Add(parts[1]);
				}
			}

			if (!String.IsNullOrEmpty(attachment.ContentId) && attachment.ContentId.StartsWith(caption)) return attachment.ContentId;
			if (!String.IsNullOrEmpty(attachment.Name) && attachment.Name.StartsWith(caption)) return attachment.Name;
			string exist = existing.FirstOrDefault(e => (e == caption + attachment.ContentId) || (e == caption + attachment.Name));
			if (!String.IsNullOrEmpty(exist)) return exist;

			string name = attachment.Name;
			int slash = name.LastIndexOf("\\");
			if (slash >= 0 && slash < name.Length - 1) name = name.Substring(slash + 1);
			return caption + name;
		}
		protected string GetAttachmentNameByContentId(string ContentId)
		{
			if (TryParseAcumaticaContentId(ContentId, out Guid fileID))
			{
				if (CheckFileExistance(fileID, out var file))
				{
					return file.FullName;
				}
			}
			return null;
		}
		protected string ClearHtml(PXGraph graph, TPrimary row, string html, IEnumerable<AttachmentType> attachments)
		{
			if (String.IsNullOrEmpty(html)) return html;

			List<AttachmentType> atts = new List<AttachmentType>();
			if (attachments != null)
			{
				foreach (AttachmentType att in attachments)
				{
					//if (att.IsInline && att.IsInlineSpecified)
					atts.Add(att);
				}
			}

			Regex imgRegex = new Regex("(?<root><img.+?(?<src>src=[\"].+?[\"])[^>]*?>)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
			Regex aRegex = new Regex("(?<root><a.+?(?<href>href=[\"].+?[\"])[^>]*?>)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
			foreach (Match m in imgRegex.Matches(html).ToArray<Match>().Concat<Match>(aRegex.Matches(html).ToArray<Match>()))
			{
				if (m.Success)
				{
					bool isImage;
					Group maing = ExtractGroup(m, out isImage);
					Group rootg = m.Groups["root"];

					if (maing != null && maing.Success)
					{
						string src = maing.Value;
						src = src.Substring(src.IndexOf("\"", StringComparison.InvariantCultureIgnoreCase) + 1);
						src = src.Substring(0, src.LastIndexOf("\"", StringComparison.InvariantCultureIgnoreCase));

						if (!src.StartsWith("cid:")) continue;

						AttachmentType attachment = null;
						string filename = src.Replace("cid:", "");
						if (atts.Count == 1) attachment = atts[0];
						if (attachment == null && atts.Any(a => a.ContentId == filename)) attachment = atts.First(a => a.ContentId == filename);
						if (attachment != null)
						{
							filename = GetAttachmentNameByContentId(attachment.ContentId);
							if (String.IsNullOrEmpty(filename))
							{
								filename = GetAttachmentName(graph, row, attachment);
							}
						}

						string line = ParceHtmlTag(m.Value, filename, attachment != null, isImage);

						html = html.Replace(rootg.Value, line ?? rootg.Value);
					}
				}
			}

			return html;
		}
		protected string ClearHtml(PXGraph graph, TPrimary row, string html, bool creation, IEnumerable<UploadFileWithData> attachments)
		{
			if (String.IsNullOrEmpty(html)) return html;

			Regex imgRegex = new Regex("(?<root><img.+?(?<src>src=[\"].+?[\"]).+?(?<exch>exchange=[\"].+?[\"])*?>)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
			Regex aRegex = new Regex("(?<root><a.+?(?<href>href=[\"].+?[\"]).+?(?<exch>exchange=[\"].+?[\"])*?>)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
			foreach (Match m in imgRegex.Matches(html).ToArray<Match>().Concat<Match>(aRegex.Matches(html).ToArray<Match>()))
			{
				if (m.Success)
				{
					bool isImage = false;
					Group maing = ExtractGroup(m, out isImage);

					string src = null, srcOriginal = null;
					if (isImage && maing != null && maing.Success)
					{
						src = srcOriginal = maing.Value;
						src = src.Substring(src.IndexOf("\"", StringComparison.InvariantCultureIgnoreCase) + 1);
						src = src.Substring(0, src.LastIndexOf("\"", StringComparison.InvariantCultureIgnoreCase));
					}

					string exch = null;
					Group exchg = m.Groups["exch"];
					if (exchg != null && exchg.Success)
					{
						exch = exchg.Value;
						exch = exch.Substring(exch.IndexOf("\"", StringComparison.InvariantCultureIgnoreCase) + 1);
						exch = exch.Substring(0, exch.LastIndexOf("\"", StringComparison.InvariantCultureIgnoreCase));
					}

					if (!String.IsNullOrEmpty(exch))
					{
						html = html.Replace(exchg.Value, String.Empty);
					}

					if (!String.IsNullOrEmpty(src))
					{
						string filename = GetAttachmentFileID(src);
						UploadFileWithData file = null;
						if (!String.IsNullOrEmpty(filename))
						{
							file = attachments?.FirstOrDefault(f => String.Equals(f.FileID?.ToString(), filename, StringComparison.InvariantCultureIgnoreCase));
						}
						else
						{
							filename = GetAttachmentName(src);
							file = attachments?.FirstOrDefault(f => String.Equals(f.Name, src, StringComparison.InvariantCultureIgnoreCase));
						}

						if (creation)
						{
							if (file != null)
							{
								string cid = ConvertToAcumaticaContentId(file.FileID);
								String srcUpdated = srcOriginal.Replace(src, "cid:" + cid);
								html = html.Replace(srcOriginal, srcUpdated);
								//Updating title
								html = html.Replace(src, filename);

								file.ContentID = cid;
							}
						}
						else
						{
							if (!String.IsNullOrEmpty(exch))
							{
								html = html.Replace(src, exch);

								if (file != null)
									file.ContentID = exch.Replace("cid:", "");
							}
						}
					}
				}
			}

			return html;
		}
		protected System.Text.RegularExpressions.Group ExtractGroup(Match m, out bool image)
		{
			Group srcg = m.Groups["src"];
			Group hrefg = m.Groups["href"];

			Group group = null;
			if (srcg != null && srcg.Success) group = srcg;
			if (hrefg != null && hrefg.Success) group = hrefg;

			image = group == srcg;

			return group;
		}
		protected string ParceHtmlTag(string tag, string fileName, bool isFound, bool isImg)
		{
			int index = 0;
			var parsedTag = ParsedOpeningTag.Parse(tag, ref index);
			if (parsedTag == null) 
				return null;

			parsedTag.SetAttribute("exchange", parsedTag.GetAttribute(isImg ? "src" : "href"));
			if (isFound)
			{
				parsedTag.SetAttribute("objtype", "file");
				parsedTag.SetAttribute("data-convert", "view");
				parsedTag.SetAttribute("embedded", "true");

				if (isImg) parsedTag.SetAttribute("src", fileName);
				else parsedTag.SetAttribute("href", fileName);
			}
			else
			{
				if (isImg)
					parsedTag.SetAttribute("src", BrokenImageBase64);
				else
				{
					parsedTag.SetAttribute("data-convert-error", Messages.EmailExchangeBrokenLink);
					parsedTag.SetAttribute("href", "~/Frames/Error.aspx");
				}
			}

			return parsedTag.Value;
		}

		// check if file with same data already exist in database
		private bool CheckFileExistance(Guid fileId, out FileInfo file)
		{
			file = Uploader.GetFile(fileId);
			return file != null;
		}

		private bool CheckFileExistance(string fileName, out FileInfo file)
		{
			file = Uploader.GetFile(fileName);
			return file != null;
		}

		// simple check lengths, but perhaps should replace it with hashes checking
		protected bool CompareBinaryData(byte[] left, byte[] right)
		{
			return left?.Length == right?.Length;
		}
		#endregion
	}
	#endregion

	#region ExchangeActivitySyncCommand
	public abstract class ExchangeActivitySyncCommand<
		TGraph, 
		TExchangeType, 
		TActivity, 
		TActivitySyncronize,
		TActivityNoteID,
		TActivityOwner> : ExchangeBaseLogicSyncCommand<TGraph, TActivity, TExchangeType>
			where TGraph : PXGraph, new()
			where TExchangeType : ItemType, new()
			where TActivity : CRActivity, new()
			where TActivitySyncronize : class, IBqlField
			where TActivityNoteID : class, IBqlField
			where TActivityOwner : class, IBqlField
	{
		protected bool ExportInserted = true;
		protected bool ExportUpdated = true;
		protected bool ExportDeleted = true;
		protected bool ExportReowned = false;

		protected ExchangeActivitySyncCommand(MicrosoftExchangeSyncProvider provider, string operationCode, PXExchangeFindOptions findOption)
			: base(provider, operationCode, findOption | PXExchangeFindOptions.IncludePrivate)
		{
		}
		protected override void ExportImportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			// If no conflicts -> change direction (it increases the accuracy)
			// ============================================================
			// Get updated items from server
			IEnumerable<PXExchangeItem<TExchangeType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
			var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation

			// Get updated entities from DB
			BqlCommand cmd = GetSelectCommand();

			bool hasConflicts;
            using (new PXReadDeletedScope())
			{
				// Deleted items also make conflicts
				IEnumerable<PXSyncItemBucket<TActivity>> dbEntities = SelectItems<TActivityNoteID>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);

				Func<PXSyncItemBucket<TActivity>, bool> isModifiedOnBothSides = delegate (PXSyncItemBucket<TActivity> bucket)
				{
					var serverItem = serverItemsList.FirstOrDefault(_ => _.Item.ItemId.Id == bucket.Reference.BinaryReference);

					if (serverItem != null)
					{
						return serverItem.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
								|| bucket.Item1.DeletedDatabaseRecord == true;
					}
					return false;
				};

				hasConflicts = dbEntities.Where(isModifiedOnBothSides).Any();
			}

			if (hasConflicts)
			{
				ExportFirst(policy, direction, mailboxes);
			}
			else
			{
				ImportFirst(policy, direction, mailboxes);
			}

		}
		protected override void LastUpdated(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			PXSyncItemSet olderOnServer = null;
			if ((direction & PXEmailSyncDirection.Directions.Full) == PXEmailSyncDirection.Directions.Full)
			{
				// Get updated items from server
				IEnumerable<PXExchangeItem<TExchangeType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
				var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation


				// Get updated entities from DB
				BqlCommand cmd = GetSelectCommand();

				using (new PXReadDeletedScope())
				{
					// Deleted items also make conflicts
					IEnumerable<PXSyncItemBucket<TActivity>> dbEntities = SelectItems<TActivityNoteID>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);

					// Exclude from DB entities ones from server if server version has newer changes
					Func<PXSyncItemBucket<TActivity>, PXSyncItemID> converter = delegate (PXSyncItemBucket<TActivity> bucket)
					{
						// select ones to exclude: if server version is newer

						var serverItem = serverItemsList.FirstOrDefault(_ =>
							_.Item.ItemId.Id == bucket.Reference.BinaryReference
							&& _.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
						);

						if (serverItem != null && serverItem.Item.LastModifiedTime < PXTimeZoneInfo.ConvertTimeToUtc((DateTime)bucket.Item1.LastModifiedDateTime, LocaleInfo.GetTimeZone()))
						{
							return new PXSyncItemID(null, bucket.Reference.BinaryReference, null);
						}
						return new PXSyncItemID(null, null, null);
					};
					olderOnServer = new PXSyncItemSet(dbEntities.Select(converter).Where(_ => _.ItemID != null));
				}
			}

			// Now for import ready only that items that are newer than the items in DB
			// Make import, excluding older
			ImportFirst(policy, direction, mailboxes, olderOnServer);
		}

		protected override void KeepBoth(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			if ((direction & PXEmailSyncDirection.Directions.Export) == PXEmailSyncDirection.Directions.Export)
			{
				// Get updated items from server
				IEnumerable<PXExchangeItem<TExchangeType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
				var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation


				// Get updated entities from DB
				BqlCommand cmd = GetSelectCommand();

				IEnumerable<PXSyncItemBucket<TActivity>> dbEntities = SelectItems<TActivityNoteID>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);


				PXCache cache = graph.Caches[typeof(TActivity)];
				PXCache remCache = graph.Caches[typeof(CRReminder)];
				Action<PXSyncItemBucket<TActivity>> createDublicateInDB = delegate(PXSyncItemBucket<TActivity> bucket)
				{
					TActivity activity = bucket.Item1;
					TActivity newActivity = (TActivity)cache.CreateCopy(activity);
					newActivity.NoteID = null;
					
					newActivity = (TActivity)cache.Insert(newActivity);
					cache.Update(newActivity);
					cache.Persist(PXDBOperation.Insert);
					cache.Clear();


					// Reminder copy
					Dictionary<string, object> keys = new Dictionary<string, object>();
					keys["RefNoteID"] = activity.NoteID;
					remCache.Locate(keys);

					var current = (CRReminder)remCache.Current;
					var reminder = (CRReminder)remCache.CreateCopy(current);
					reminder.RefNoteID = newActivity.NoteID;
					reminder.ReminderDate = current.ReminderDate;
					remCache.Insert(reminder);
					remCache.Persist(PXDBOperation.Insert);
					remCache.Clear();
				};

				Func<PXSyncItemBucket<TActivity>, bool> isModifiedOnBothSides = delegate (PXSyncItemBucket<TActivity> bucket)
				{
					var serverItem = serverItemsList.FirstOrDefault(_ => 
						_.Item.ItemId.Id == bucket.Reference.BinaryReference
						&& _.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
					);

					if (serverItem != null)
					{
						return true;
					}
					return false;
				};

				// Create a Copy of all conflicted entities
				dbEntities.Where(isModifiedOnBothSides).ForEach(createDublicateInDB);
			}

			ImportFirst(policy, direction, mailboxes);
		}
		
		protected override IEnumerable<PXSyncResult> ProcessSyncExport(PXSyncMailbox[] mailboxes, PXSyncItemSet imported)
		{
			if (!mailboxes.Any()) yield break;

			PXCache cache = graph.Caches[typeof(TActivity)];
			BqlCommand cmd = GetSelectCommand();
			cmd = cmd.WhereAnd<Where<TActivitySyncronize, Equal<True>>>();
			
			if (ExportInserted)
			{
				foreach (PXExchangeResponce<TExchangeType> item in ExportActivityInserted(SelectItems<TActivityNoteID>(graph, cmd, mailboxes, imported, PXSyncItemStatus.Inserted, Attachments)))
				{
					PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

					yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Inserted, delegate
					{
						SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, true);
						PostProcessingSuccessfull(cache, PXSyncItemStatus.Inserted, item, tag);
					}, delegate
					{
						PostProcessingFailed(cache, PXSyncItemStatus.Inserted, item, tag);
						return false;
					});
				}
			}
			if (ExportUpdated)
			{
				foreach (PXExchangeResponce<TExchangeType> item in ExportActivityUpdated(SelectItems<TActivityNoteID>(graph, cmd, mailboxes, imported, PXSyncItemStatus.Updated, Attachments)))
				{
					PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

					yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Updated, delegate
					{
						SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, true);
						PostProcessingSuccessfull(cache, PXSyncItemStatus.Updated, item, tag);
					}, delegate
					{
						if (item.Code == ResponseCodeType.ErrorItemNotFound)
						{
							SaveReference(item.Address, tag.Row.NoteID, null, null, null, false);

							MarkUnsynced(tag);

							return true;
                        }
                        PostProcessingFailed(cache, PXSyncItemStatus.Updated, item, tag);
                        return false;
                    });
                }
            }
            if (ExportDeleted)
			{
				foreach (PXExchangeResponce<TExchangeType> item in ExportActivityDeleted(SelectItems<TActivityNoteID>(graph, cmd, mailboxes, imported, PXSyncItemStatus.Deleted, false)))
				{
					PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

					yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Deleted, delegate
					{
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, true);
					});
				}

				if (ExportReowned)
				{
					// only for tasks
					BqlCommand reoCmd = PXSelectJoin<TActivity,
						InnerJoin<EMailSyncReference, 
							On<EMailSyncReference.noteID, Equal<TActivityNoteID>>,
						InnerJoin<Contact, 
							On<Contact.eMail, Equal<EMailSyncReference.address>>,
						InnerJoin<EPEmployee, 
							On<EPEmployee.defContactID, Equal<Contact.contactID>, 
							And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>>,
						Where<CRActivity.classID, Equal<CRActivityClass.task>,
							And<EPEmployee.userID, NotEqual<TActivityOwner>, 
							And<EMailSyncReference.binaryReference, NotEqual<Empty>, 
							And<EMailSyncReference.address, Equal<Required<EMailSyncReference.address>>>>>>>.GetCommand();

					foreach (PXExchangeResponce<TExchangeType> item in ExportActivityDeleted(SelectItems<IBqlTable, IBqlTable>(graph, reoCmd, mailboxes, imported, PXSyncItemStatus.None)))
					{
						PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

						yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Deleted, delegate
						{
							SaveReference(item.Address, tag.Row.NoteID, null, null, null, true);
						});
					}
				}
			}
		}

		protected override IEnumerable<PXSyncResult> ProcessSyncExportUnsynced(PXSyncMailbox[] mailboxes, PXSyncItemSet exported)
		{
			if (!mailboxes.Any()) yield break;

			PXCache cache = graph.Caches[typeof(TActivity)];
			BqlCommand cmd = GetSelectCommandUnsynced();
			cmd = cmd.WhereAnd<Where<TActivitySyncronize, Equal<True>>>();
			
			foreach (PXExchangeResponce<TExchangeType> item in ExportActivityUnsynced(SelectItems<TActivityNoteID>(graph, cmd, mailboxes, exported, PXSyncItemStatus.Unsynced, false)))
			{
				PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

				yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Updated, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, true);
				}, delegate
				{
					if (item.Code == ResponseCodeType.ErrorItemNotFound)
					{
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, false);

						MarkUnsynced(tag);

						return true;
					}
					PostProcessingFailed(cache, PXSyncItemStatus.Updated, item, tag);
					return false;
				});
			}
		}
		protected override IEnumerable<PXSyncResult> ProcessSyncImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported)
		{
			if (!mailboxes.Any()) yield break;
			
			IEnumerable<PXExchangeItem<TExchangeType>> activities = GetItems(mailboxes, exported,
				PXExchangeFindOptions.None,
				Tuple.Create("0x3e4a", MapiPropertyTypeType.String));
			
			List<PXExchangeRequest<TExchangeType, ItemIdType>> toMove = new List<PXExchangeRequest<TExchangeType, ItemIdType>>();
			int i = 0;

			foreach (PXExchangeItem<TExchangeType> fitem in activities)
			{
				if (i++%100 == 0)
				{
					graph = PXGraph.CreateInstance<TGraph>();
					PXCache cache = graph.Caches[typeof(TActivity)];
					cache.AllowDelete = true;
					cache.AllowInsert = true;
					cache.AllowUpdate = true;
				}

				PXExchangeItem<TExchangeType> item = fitem;
				PXSyncMailbox mailbox = mailboxes.FirstOrDefault(m => String.Equals(m.Address, item.Address, StringComparison.InvariantCultureIgnoreCase));

				BqlCommand select = PXSelectJoin<TActivity,
						LeftJoin<EMailSyncReference,
							On<EMailSyncReference.noteID, Equal<TActivityNoteID>,
							And<EMailSyncReference.serverID, Equal<Required<EMailSyncReference.serverID>>,
							And<EMailSyncReference.address, Equal<Required<EMailSyncReference.address>>>>>>>.GetCommand();

				if (PreSyncRestrict(item.Item))
					continue;

				TActivity activity = null;
				EMailSyncReference reference = null;

				bool isSynchronized = true;
				
				if (item.Item.ExtendedProperty != null && item.Item.ExtendedProperty.Any(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a"))
				{
					select = select.WhereNew<Where<TActivityNoteID, Equal<Required<TActivityNoteID>>,
						And<TActivitySyncronize, Equal<True>>>>();

					var result = (PXResult<TActivity, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
						Account.AccountID, 
						item.Address, 
						new Guid(item.Item.ExtendedProperty.First(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a").Item.ToString()));

					activity = result;
					reference = result;
				}
				if (activity == null)
                {
					isSynchronized = false;

					select = select.WhereNew<Where<EMailSyncReference.binaryReference, Equal<Required<EMailSyncReference.binaryReference>>>>();

	                var result = (PXResult<TActivity, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
		                Account.AccountID,
		                item.Address,
		                item.Item.ItemId.Id);

					activity = result;
					reference = result;
				}
				
				if (activity != null && reference != null && reference.BinaryChangeKey == item.Item.ItemId.ChangeKey)
					continue;
				
				Guid? noteid = activity?.NoteID;
				PXSyncItemStatus status = activity == null ? PXSyncItemStatus.Inserted : PXSyncItemStatus.Updated;
				yield return SafeOperation(item.Address, item.Item.ItemId.Id, noteid, item.Item.Subject, null, status, delegate
				{
					ImportAction(mailbox, item.Item, ref activity);

					if (activity == null) return;
					noteid = activity.NoteID;

					//saving attachments
					if (SaveAttachmentsSync(graph, activity, item.Attachments))
						graph.Actions.PressSave();

					//saving reference
					SaveReference(item.Address, activity.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, isSynchronized);

					//collecting items that we have to move
					PXSyncDirectFolder folder = mailboxes.SelectMany(m => m.Folders).FirstOrDefault(f => (f.FolderID is FolderIdType) && ((FolderIdType)f.FolderID).Id == item.Item.ParentFolderId.Id);
					if (folder != null && folder.MoveToFolder != null && folder.MoveToFolder.Length > 0)
					{
						foreach (PXSyncMovingCondition mc in folder.MoveToFolder)
						{
							BaseFolderIdType folderid = mc.Evaluate(mailbox, item.Item);
							if (folderid != null)
							{
								toMove.Add(new PXExchangeRequest<TExchangeType, ItemIdType>(new PXExchangeFolderID(fitem.Address, folderid), fitem.Item.ItemId, Guid.NewGuid().ToString(), activity, item.Attachments));
								Provider.LogInfo(item.Address, Messages.EmailExchangeItemMoving, item.Item.Subject);
								break;
							}
						}
					}
				}, true).DoWith(r => new PXSyncResult(r, null, noteid, null, status));
				graph.Clear();
			}

			//moving processed items
			if (toMove.Count > 0)
			{
				Provider.LogVerbose(null, Messages.EmailExchangeMoving);
				foreach (PXExchangeResponce<TExchangeType> moved in Gate.MoveItems(toMove))
				{
					TActivity activity = moved.Tag as TActivity;

					SafeOperation(moved, moved.Item.ItemId.Id, activity.NoteID, activity.Subject, "Moving", PXSyncItemStatus.None, delegate
					{
						if (activity != null && moved.Item != null && moved.Item.ItemId != null)
							SaveReference(moved.Address, activity.NoteID, moved.Item.ItemId, moved.Item.ConversationId, null, null);
					});
				}
			}
		}

		protected virtual IEnumerable<PXExchangeResponce<TExchangeType>> ExportActivityInserted(IEnumerable<PXSyncItemBucket<TActivity>> inserted)
		{
			List<string> canceled = new List<string>();
			Func<PXSyncItemBucket<TActivity>, PXExchangeRequest<TExchangeType, ItemType>> converter = delegate(PXSyncItemBucket<TActivity> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				TActivity activity = bucket.Item1;
				TExchangeType item = new TExchangeType();

				ExportInsertedItemProperty(t => t.ExtendedProperty, item, PXExchangeConversionHelper.GetExtendedProperties(
					Tuple.Create("0x3e4a", MapiPropertyTypeType.String, (Object)activity.NoteID.ToString())));

				ExportInsertedItemProperty(t => t.Subject, item, activity.Subject);
				if (!String.IsNullOrEmpty(activity.Body))
					ExportInsertedItemProperty(t => t.Body, item, new BodyType() { BodyType1 = BodyTypeType.HTML, Value = ClearHtml(graph, activity, activity.Body, true, bucket.Attachments) });

				PXSyncTag tag = ExportInsertedAction(mailbox, item, activity);

				ExportInsertedItemProperty(t => t.Categories, item, new string[] { Policy.Category });

				if (tag != null && tag.CancelRequired) canceled.Add(bucket.ID);
				var request = new PXExchangeRequest<TExchangeType, ItemType>(mailbox.Folders.First(f => f.IsExport), item, bucket.ID, new PXSyncTag<TActivity>(activity, mailbox, bucket.Reference, tag), ConvertAttachment(bucket.Attachments));
				if (tag != null && tag.SendRequired) request.SendRequired = tag.SendRequired;
				if (tag != null && tag.SendSeparateRequired) request.SendSeparateRequired = tag.SendSeparateRequired;
				return request;
			};

			foreach (PXExchangeResponce<TExchangeType> item in Gate.CreateItems(inserted.Select(converter)))
			{
				if (item.Success && canceled.Contains(item.UID))
				{
					PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

					//just cancel item and leave it undeleted. Be aware of status of canceled item http://jira.acumatica.com/browse/AC-48407?filter=10121
					//PXExchangeRequest<ExchangeType, ItemType> cancelRequest =
					//	new PXExchangeRequest<ExchangeType, ItemType>(tag.Mailbox.Folders.First(f => f.IsExport), new ExchangeType() { ItemId = item.Item.ItemId }, item.UID, item.Tag);
					//foreach (PXExchangeResponce<ExchangeType> sub in Gate.CancelItems(cancelRequest))
					//	yield return sub;

					//delete canceled item
					PXExchangeRequest<TExchangeType, ItemType> deleteRequest =
						new PXExchangeRequest<TExchangeType, ItemType>(tag.Mailbox.Folders.First(f => f.IsExport), new TExchangeType() { ItemId = item.Item.ItemId }, item.UID, item.Tag);
					foreach (PXExchangeResponce<TExchangeType> sub in Gate.DeleteItems(deleteRequest))
						yield return sub;
				}
				else yield return item;
			}
		}
		protected virtual IEnumerable<PXExchangeResponce<TExchangeType>> ExportActivityUpdated(IEnumerable<PXSyncItemBucket<TActivity>> updated)
		{
			List<string> canceled = new List<string>();
			Func<PXSyncItemBucket<TActivity>, PXExchangeRequest<TExchangeType, ItemChangeType>> converter = delegate(PXSyncItemBucket<TActivity> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				TActivity activity = bucket.Item1;

				List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<TExchangeType>(t => t.ExtendedProperty, "0x3e4a", MapiPropertyTypeType.String, (Object)activity.NoteID.ToString()));
				
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.Subject, UnindexedFieldURIType.itemSubject, activity.Subject));
				if (!String.IsNullOrEmpty(activity.Body))
					updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.Body, UnindexedFieldURIType.itemBody, new BodyType() { BodyType1 = BodyTypeType.HTML, Value = ClearHtml(graph, activity, activity.Body, false, bucket.Attachments) }));

				PXSyncTag tag = ExportUpdatedAction(mailbox, null, activity, updates);
				if (tag != null && tag.SkipReqired) canceled.Add(bucket.ID);

				ItemChangeType change = new ItemChangeType() { Item = new ItemIdType() { Id = bucket.Reference.BinaryReference }, Updates = updates.ToArray() };

				if (tag != null && tag.CancelRequired) canceled.Add(bucket.ID);
				var request = new PXExchangeRequest<TExchangeType, ItemChangeType>(mailbox.Folders.First(f => f.IsExport), change, bucket.ID, new PXSyncTag<TActivity>(activity, mailbox, bucket.Reference, tag), ConvertAttachment(bucket.Attachments));
				if (tag != null && tag.SendRequired) request.SendRequired = tag.SendRequired;
				if (tag != null && tag.SendSeparateRequired) request.SendSeparateRequired = tag.SendSeparateRequired;
				return request;
			};

			foreach (PXExchangeResponce<TExchangeType> item in Gate.UpdateItems(updated.Select(converter)))
			{
				if (item.Success && canceled.Contains(item.UID))
				{
					PXSyncTag<TActivity> tag = item.Tag as PXSyncTag<TActivity>;

					//just cancel item and leave it undeleted. Be aware of status of canceled item http://jira.acumatica.com/browse/AC-48407?filter=10121
					//PXExchangeRequest<ExchangeType, ItemType> cancelRequest
					//	= new PXExchangeRequest<ExchangeType, ItemType>(tag.Mailbox.Folders.First(f => f.IsExport), new ExchangeType() { ItemId = item.Item.ItemId }, item.UID, item.Tag);
					//foreach (PXExchangeResponce<ExchangeType> sub in Gate.CancelItems(cancelRequest))
					//	yield return sub;

					//delete wit cancel canceled item
					PXExchangeRequest<TExchangeType, ItemType> deleteRequest =
						new PXExchangeRequest<TExchangeType, ItemType>(tag.Mailbox.Folders.First(f => f.IsExport), new TExchangeType() { ItemId = item.Item.ItemId }, item.UID, item.Tag);
					foreach (PXExchangeResponce<TExchangeType> sub in Gate.DeleteItems(deleteRequest))
						yield return sub;
				}
				else yield return item;
			}
		}
		protected virtual IEnumerable<PXExchangeResponce<TExchangeType>> ExportActivityUnsynced(IEnumerable<PXSyncItemBucket<TActivity>> updated)
		{
			Func<PXSyncItemBucket<TActivity>, PXExchangeRequest<TExchangeType, ItemChangeType>> converter = delegate (PXSyncItemBucket<TActivity> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				TActivity activity = bucket.Item1;

				List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<TExchangeType>(t => t.ExtendedProperty, "0x3e4a", MapiPropertyTypeType.String, (Object)activity.NoteID.ToString()));

				ItemChangeType change = new ItemChangeType() { Item = new ItemIdType() { Id = bucket.Reference.BinaryReference }, Updates = updates.ToArray() };

				var request = new PXExchangeRequest<TExchangeType, ItemChangeType>(mailbox.Folders.First(f => f.IsExport), change, bucket.ID, new PXSyncTag<TActivity>(activity, mailbox, bucket.Reference), ConvertAttachment(bucket.Attachments));

				return request;
			};
			
			return Gate.UpdateItems(updated.Select(converter));
		}
		protected virtual IEnumerable<PXExchangeResponce<TExchangeType>> ExportActivityDeleted(IEnumerable<PXSyncItemBucket<TActivity>> deleted)
		{
			Func<PXSyncItemBucket<TActivity>, PXExchangeRequest<TExchangeType, ItemType>> converter = delegate(PXSyncItemBucket<TActivity> bucket)
			{
				string id = Guid.NewGuid().ToString();
				PXSyncMailbox mailbox = bucket.Mailbox;
				TActivity activity = bucket.Item1;
				EMailSyncReference reference = bucket.Reference;

				TExchangeType item = new TExchangeType();
				item.ItemId = new ItemIdType() { Id = reference.BinaryReference };

				return new PXExchangeRequest<TExchangeType, ItemType>(mailbox.Folders.First(f => f.IsExport), item, id, new PXSyncTag<TActivity>(activity, mailbox, bucket.Reference));
			};

			return Gate.DeleteItems(deleted.Select(converter));
		}
		
		protected virtual void PostProcessingSuccessfull(PXCache cache, PXSyncItemStatus status, PXExchangeResponce<TExchangeType> item, PXSyncTag<TActivity> tag)
		{
		}
		protected virtual void PostProcessingFailed(PXCache cache, PXSyncItemStatus status, PXExchangeResponce<TExchangeType> item, PXSyncTag<TActivity> tag)
		{
		}
		protected virtual void MarkUnsynced(PXSyncTag<TActivity> tag)
		{
			var list = new List<PXDataFieldParam>
			{
				new PXDataFieldAssign<TActivitySyncronize>(false),
				new PXDataFieldRestrict<TActivityNoteID>(tag.Row.NoteID.Value)
			};
		
			PXDatabase.Update<TActivity>(list.ToArray());
		}
		protected void PrepareActivity(PXSyncMailbox mailbox, TExchangeType item, bool evaluateOwner, ref TActivity activity)
		{
			PXCache cache = graph.Caches[typeof(TActivity)];

			if (activity == null)
			{
				activity = new TActivity();
				activity = (TActivity)cache.Insert(activity);
			}
			else if (activity.Synchronize != true)
			{
				activity = null;
				return;
			}

			activity.Synchronize = true;
			activity.Subject = item.Subject ?? activity.Subject ?? Messages.EmailExchangeEmptySubject;

			if (item.Body != null)
				activity.Body = PX.Web.UI.RichStyle.RemoveViewStyle(item.Body.Value);

			if (evaluateOwner)
			{
				Guid owner;
				if (Cache.EmployeeCache.TryGetValue(mailbox.EmployeeID, out owner))
					activity.OwnerID = owner;
			}
			else activity.OwnerID = null;

			activity = (TActivity)cache.Update(activity);
		}
		protected void PostpareActivity(PXSyncMailbox mailbox, TExchangeType item, ref TActivity activity)
		{
			PXCache cache = graph.Caches[typeof(TActivity)];

			activity = (TActivity)cache.Update(activity);

			//presisting data
			graph.Actions.PressSave();

			//clearing html
			activity.Body = ClearHtml(graph, activity, activity.Body, item.Attachments);
			activity = (TActivity)cache.Update(activity);

			//presisting data
			graph.Actions.PressSave();
		}

		#region Interface
		protected abstract PXSyncTag ExportInsertedAction(PXSyncMailbox account, TExchangeType item, TActivity activity);
		protected abstract PXSyncTag ExportUpdatedAction(PXSyncMailbox account, TExchangeType item, TActivity activity, List<ItemChangeDescriptionType> updates);
		protected abstract PXSyncTag ImportAction(PXSyncMailbox account, TExchangeType item, ref TActivity activity);

		protected virtual bool PreSyncRestrict(TExchangeType item) { return false; }
		protected virtual BqlCommand GetSelectCommandUnsynced() { return GetSelectCommand(); }
		#endregion
	}
	#endregion

	#region ExchangeTasksSyncCommand
	public class ExchangeTasksSyncCommand : ExchangeActivitySyncCommand<CRTaskMaint, TaskType, CRActivity, CRActivity.synchronize, CRActivity.noteID, CRActivity.ownerID>
	{
		public ExchangeTasksSyncCommand(MicrosoftExchangeSyncProvider provider)
			: base(provider, PXEmailSyncOperation.TasksCode, PXExchangeFindOptions.IncludeAttachments)
		{
			ExportReowned = true;

			if ((Policy.TasksSkipCategory ?? false))
				DefFindOptions = DefFindOptions | PXExchangeFindOptions.IncludeUncategorized;
		}

		protected override void ConfigureEnvironment(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			EnsureEnvironmentConfigured<TasksFolderType>(mailboxes, new PXSyncFolderSpecification(Policy.TasksSeparated == true ? Policy.TasksFolder : null, DistinguishedFolderIdNameType.tasks));
		}
		protected override BqlCommand GetSelectCommand()
		{
			return PXSelectReadonly2<CRActivity,
				InnerJoin<Contact,
					On<CRActivity.ownerID, Equal<Contact.userID>>,
				InnerJoin<EPEmployee, 
					On<EPEmployee.defContactID, Equal<Contact.contactID>, 
					And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>,
				Where<CRActivity.classID, Equal<CRActivityClass.task>, 
					And<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>>.GetCommand();
		}

		protected override PXSyncTag ExportInsertedAction(PXSyncMailbox account, TaskType item, CRActivity activity)
		{
			ExportInsertedItemProperty(t => t.Status, item, PXExchangeConversionHelper.ParceActivityStatus(activity.UIStatus));
			ExportInsertedItemProperty(t => t.Importance, item, activity.Priority);
			ExportInsertedItemProperty(t => t.PercentComplete, item, activity.PercentCompletion);
			ExportInsertedItemProperty(t => t.StartDate, item, activity.StartDate, account.ExchangeTimeZone);
			ExportInsertedItemProperty(t => t.DueDate, item, activity.EndDate, account.ExchangeTimeZone);
			ExportInsertedItemProperty(t => t.CompleteDate, item, activity.CompletedDate, account.ExchangeTimeZone);

			var reminder = graph.Reminder.Current;

			ExportInsertedItemProperty(t => t.ReminderIsSet, item, reminder != null);
			if (reminder != null && reminder.ReminderDate != null)
			{
				ExportInsertedItemProperty(t => t.ReminderNextTime, item, reminder.ReminderDate);
				ExportInsertedItemProperty(t => t.ReminderDueBy, item, reminder.ReminderDate);
			}

			return null;
		}
		protected override PXSyncTag ExportUpdatedAction(PXSyncMailbox account, TaskType item, CRActivity activity, List<ItemChangeDescriptionType> updates)
		{
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.Status, UnindexedFieldURIType.taskStatus, PXExchangeConversionHelper.ParceActivityStatus(activity.UIStatus)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.Importance, UnindexedFieldURIType.itemImportance, PXExchangeConversionHelper.ParceActivityPriority(activity.Priority)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.StartDate, UnindexedFieldURIType.taskStartDate, activity.StartDate, account.ExchangeTimeZone));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.DueDate, UnindexedFieldURIType.taskDueDate, activity.EndDate, account.ExchangeTimeZone));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.CompleteDate, UnindexedFieldURIType.taskCompleteDate, activity.CompletedDate, account.ExchangeTimeZone));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<TaskType>(t => t.PercentComplete, UnindexedFieldURIType.taskPercentComplete,
                PXExchangeConversionHelper.ParceActivityStatus(activity.UIStatus) == TaskStatusType.Completed
                ? 100
                : activity.PercentCompletion));

			var reminder = graph.Reminder.Current;

			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.ReminderIsSet, UnindexedFieldURIType.itemReminderIsSet, reminder != null));
			if (reminder != null && reminder.ReminderDate != null)
			{
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.ReminderNextTime, UnindexedFieldURIType.itemReminderNextTime, reminder.ReminderDate));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.ReminderDueBy, UnindexedFieldURIType.itemReminderDueBy, reminder.ReminderDate));
			}

			return null;
		}
		protected override PXSyncTag ImportAction(PXSyncMailbox account, TaskType item, ref CRActivity activity)
		{
			PrepareActivity(account, item, true, ref activity);
			if (activity == null) return null;

			activity.ClassID = CRActivityClass.Task;

			CRActivity copy = activity;
			ImportItemProperty(t => t.Status, item, v => copy.UIStatus = PXExchangeConversionHelper.ParceActivityStatus(v));
			ImportItemProperty(t => t.Importance, item, v => copy.Priority = PXExchangeConversionHelper.ParceActivityPriority(v));
			ImportItemProperty(t => t.StartDate, item, v => copy.StartDate = v, false, account.ExchangeTimeZone);
			ImportItemProperty(t => t.DueDate, item, v => copy.EndDate = v, false, account.ExchangeTimeZone);
			ImportItemProperty(t => t.CompleteDate, item, v => copy.CompletedDate = v, false, account.ExchangeTimeZone);
			ImportItemProperty(t => t.PercentComplete, item, v => copy.PercentCompletion = (int)v);

			if (item.ReminderIsSet && item.ReminderNextTimeSpecified)
			{
				var reminder = (CRReminder)graph.Reminder.SelectSingle() ?? (CRReminder)graph.Reminder.Cache.Insert();
				reminder.RefNoteID = activity.NoteID;
				ImportItemProperty(t => t.ReminderNextTime, item, v => reminder.ReminderDate = v);

                graph.Caches[typeof(CRReminder)].RaiseFieldUpdated<CRReminder.reminderDate>(reminder, null);

                graph.Caches[typeof(CRReminder)].Update(reminder);
			}

			//saving activity
			PostpareActivity(account, item, ref activity);

			return null;
		}
	}
	#endregion
	#region ExchangeEventsSyncCommand
	public class ExchangeEventsSyncCommand : ExchangeActivitySyncCommand<EPEventMaint, CalendarItemType, CRActivity, CRActivity.synchronize, CRActivity.noteID, CRActivity.ownerID>
	{
		public ExchangeEventsSyncCommand(MicrosoftExchangeSyncProvider provider)
			: base(provider, PXEmailSyncOperation.EventsCode, PXExchangeFindOptions.IncludeAttachments)
		{
			if ((Policy.EventsSkipCategory ?? false))
				DefFindOptions = DefFindOptions | PXExchangeFindOptions.IncludeUncategorized;
		}

		protected override void ConfigureEnvironment(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			EnsureEnvironmentConfigured<CalendarFolderType>(mailboxes, new PXSyncFolderSpecification(Policy.EventsSeparated == true ? Policy.EventsFolder : null, DistinguishedFolderIdNameType.calendar));
		}
		protected override BqlCommand GetSelectCommand()
		{
			return PXSelectReadonly2<CRActivity,
				InnerJoin<Contact, 
					On<CRActivity.ownerID, Equal<Contact.userID>>,
				InnerJoin<EPEmployee, 
					On<EPEmployee.defContactID, Equal<Contact.contactID>,
					And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>,
				Where<CRActivity.classID, Equal<CRActivityClass.events>, 
					And<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>>.GetCommand();
		}

		protected override PXSyncTag ExportInsertedAction(PXSyncMailbox account, CalendarItemType item, CRActivity activity)
		{
			ExportInsertedItemProperty(t => t.Importance, item, PXExchangeConversionHelper.ParceActivityPriority(activity.Priority));
			ExportInsertedItemProperty(t => t.IsAllDayEvent, item, activity.AllDay);
			ExportInsertedItemProperty(t => t.Start, item, activity.StartDate, account.ExchangeTimeZone, activity.AllDay == true);
			ExportInsertedItemProperty(t => t.End, item, activity.EndDate, account.ExchangeTimeZone, activity.AllDay == true);
			ExportInsertedItemProperty(t => t.LegacyFreeBusyStatus, item, PXExchangeConversionHelper.ParceShowAs(activity.ShowAsID));

			var reminder = graph.Reminder.Current;

			ExportInsertedItemProperty(t => t.ReminderIsSet, item, reminder != null);
			if (reminder != null && reminder.ReminderDate != null)
			{
				ExportInsertedItemProperty(t => t.ReminderMinutesBeforeStart, item, ((int)((TimeSpan)(activity.StartDate - reminder.ReminderDate)).TotalMinutes).ToString());
			}

			if (activity.IsPrivate != true)
			{
				ExportInsertedItemProperty(t => t.Location, item, activity.Location);

				List<AttendeeType> attendees = new List<AttendeeType>();
				graph.Events.Current = activity;
				foreach (AllTypeAttendee ata in graph.AllAttendees.Select())
				{
					attendees.Add(new AttendeeType()
					{
						Mailbox = new EmailAddressType() { EmailAddress = ata.Email },
						ResponseType = PXExchangeConversionHelper.ParceAttendeeStatus(ata.Invitation),
						ResponseTypeSpecified = true
					});
				}
				if (attendees.Count > 0) item.RequiredAttendees = attendees.ToArray();
			}

			return new PXSyncTag() { SendRequired = item.RequiredAttendees != null && item.RequiredAttendees.Length > 0, CancelRequired = activity.UIStatus == ActivityStatusListAttribute.Canceled };
		}
		protected override PXSyncTag ExportUpdatedAction(PXSyncMailbox account, CalendarItemType item, CRActivity activity, List<ItemChangeDescriptionType> updates)
		{
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.Importance, UnindexedFieldURIType.itemImportance, PXExchangeConversionHelper.ParceActivityPriority(activity.Priority)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.IsAllDayEvent, UnindexedFieldURIType.calendarIsAllDayEvent, activity.AllDay));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.Start, UnindexedFieldURIType.calendarStart, activity.StartDate, account.ExchangeTimeZone, activity.AllDay == true));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.End, UnindexedFieldURIType.calendarEnd, activity.EndDate, account.ExchangeTimeZone, activity.AllDay == true));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.LegacyFreeBusyStatus, UnindexedFieldURIType.calendarLegacyFreeBusyStatus, PXExchangeConversionHelper.ParceShowAs(activity.ShowAsID)));

			var reminder = graph.Reminder.Current;

			updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.ReminderIsSet, UnindexedFieldURIType.itemReminderIsSet, reminder != null));

			if (reminder != null && reminder.ReminderDate != null)
			{
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.ReminderMinutesBeforeStart, UnindexedFieldURIType.itemReminderMinutesBeforeStart, ((int)((TimeSpan)(activity.StartDate - reminder.ReminderDate)).TotalMinutes).ToString()));
			}

			List<AttendeeType> attendees = new List<AttendeeType>();
			if (activity.IsPrivate != true)
			{
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<CalendarItemType>(t => t.Location, UnindexedFieldURIType.calendarLocation, activity.Location));

				graph.Events.Current = activity;
				foreach (AllTypeAttendee ata in graph.AllAttendees.Select())
				{
					attendees.Add(new AttendeeType()
					{
						Mailbox = new EmailAddressType() { EmailAddress = ata.Email },
						ResponseType = PXExchangeConversionHelper.ParceAttendeeStatus(ata.Invitation),
						ResponseTypeSpecified = true
					});
				}
				if (attendees.Count > 0)
				{
					updates.Add(new SetItemFieldType { Item = new PathToUnindexedFieldType() { FieldURI = UnindexedFieldURIType.calendarRequiredAttendees }, Item1 = new CalendarItemType() { RequiredAttendees = attendees.ToArray() } });
				}
			}

			return new PXSyncTag() { SendRequired = attendees.Count > 0, CancelRequired = activity.UIStatus == ActivityStatusListAttribute.Canceled };
		}
		protected override PXSyncTag ImportAction(PXSyncMailbox account, CalendarItemType item, ref CRActivity activity)
		{
			if (item.Organizer != null && item.Organizer.Item != null && string.Compare(item.Organizer.Item.EmailAddress, account.Address, StringComparison.CurrentCultureIgnoreCase) != 0) return null;

			PrepareActivity(account, item, true, ref activity);

			if (activity == null) return null;

			activity.ClassID = CRActivityClass.Event;

			CRActivity copy = activity;

			if (item.Sensitivity == SensitivityChoicesType.Private)
			{
				activity.IsPrivate = true;
				activity.Subject = Messages.EmailExchangePrivateEvent;
				activity.Body = Messages.EmailExchangePrivateEvent;
			}
			else
			{
				ImportItemProperty(t => t.Location, item, v => copy.Location = v);
			}

			ImportItemProperty(t => t.IsAllDayEvent, item, v => copy.AllDay = v);
			ImportItemProperty(t => t.Start, item, v => copy.StartDate = v, false, account.ExchangeTimeZone, copy.AllDay == true);
			ImportItemProperty(t => t.End, item, v => copy.EndDate = v, false, account.ExchangeTimeZone, copy.AllDay == true);
			ImportItemProperty(t => t.LegacyFreeBusyStatus, item, v => copy.ShowAsID = PXExchangeConversionHelper.ParceShowAs(v));
			ImportItemProperty(t => t.Importance, item, v => copy.Priority = PXExchangeConversionHelper.ParceActivityPriority(v));

			if (item.ReminderIsSet && item.ReminderNextTimeSpecified)
			{
				var reminder = (CRReminder)graph.Reminder.SelectSingle() ?? (CRReminder)graph.Reminder.Cache.Insert();
				reminder.RefNoteID = activity.NoteID;
				ImportItemProperty(t => t.ReminderNextTime, item, v => reminder.ReminderDate = v);

                graph.Caches[typeof(CRReminder)].RaiseFieldUpdated<CRReminder.reminderDate>(reminder, null);

                graph.Caches[typeof(CRReminder)].Update(reminder);
			}

			activity = (CRActivity)graph.Caches[typeof(CRActivity)].Update(copy);

			//TODO Need review.
			if (item.Sensitivity != SensitivityChoicesType.Private && (item.OptionalAttendees != null || item.RequiredAttendees != null))
			{
				PXCache attCache = graph.Attendees.Cache;
				PXCache allCache = graph.AllAttendees.Cache;
				PXCache otherCache = graph.OtherAttendees.Cache;
				allCache.AllowDelete = attCache.AllowDelete = otherCache.AllowDelete = true;
				allCache.AllowInsert = attCache.AllowInsert = otherCache.AllowInsert = true;
				allCache.AllowUpdate = attCache.AllowUpdate = otherCache.AllowUpdate = true;

				List<string> processed = new List<string>();
				List<AllTypeAttendee> attendees = graph.AllAttendeesAndOwner.Select().Select(a => (AllTypeAttendee)a).ToList();
				foreach (AttendeeType at in (item.OptionalAttendees ?? new AttendeeType[0]).Concat((item.RequiredAttendees ?? new AttendeeType[0])))
				{
					if (processed.Contains(at.Mailbox.EmailAddress)) continue;
					else processed.Add(at.Mailbox.EmailAddress);

					AllTypeAttendee attendee = attendees.FirstOrDefault(a => String.Equals(a.Email, at.Mailbox.EmailAddress, StringComparison.InvariantCultureIgnoreCase));
					Guid ownerID;
					if (attendee != null && Guid.TryParse(attendee.Key, out ownerID) && ownerID == activity.OwnerID)
						continue;
					if (String.IsNullOrEmpty(at.Mailbox.EmailAddress)) continue;

					Dictionary<string, Object> keys = new Dictionary<string, Object>();
					keys["EventNoteID"] = activity.NoteID;
					keys["Type"] = Cache.UsersCache.ContainsKey(at.Mailbox.EmailAddress) ? 1 : 0;
					keys["Key"] = attendee != null ? attendee.Key : ((Cache.UsersCache.ContainsKey(at.Mailbox.EmailAddress) ? Cache.UsersCache[at.Mailbox.EmailAddress].ToString() : Guid.NewGuid().ToString()));

					Dictionary<string, Object> values = new Dictionary<string, Object>();
					values["Name"] = Cache.UsersCache.ContainsKey(at.Mailbox.EmailAddress) ? keys["Key"] : at.Mailbox.Name;
					values["Email"] = at.Mailbox.EmailAddress;
					values["Invitation"] = PXExchangeConversionHelper.ParceAttendeeStatus(at.ResponseType);

					if (attendee == null)
					{
						Dictionary<string, Object> all = new Dictionary<string, Object>();
						all.AddRange(keys); all.AddRange(values);
						graph.ExecuteInsert("AllAttendeesAndOwner", all);

						keys["Key"] = all["Key"];
					}
					graph.ExecuteUpdate("AllAttendeesAndOwner", keys, values);
				}

				foreach (AllTypeAttendee at in attendees)
				{
					if (processed.Contains(at.Email)) continue;
					if (Cache.UsersCache.ContainsKey(at.Email) && Cache.UsersCache[at.Email] == activity.OwnerID) continue;

					Dictionary<string, Object> keys = new Dictionary<string, Object>();
					keys["EventNoteID"] = at.EventNoteID;
					keys["Key"] = at.Key;
					keys["Type"] = at.Type;

					Dictionary<string, Object> values = new Dictionary<string, Object>();
					values["Name"] = at.Name;
					values["Email"] = at.Email;
					values["Invitation"] = at.Invitation;

					graph.ExecuteDelete("AllAttendeesAndOwner", keys, values);
				}

				graph.Events.View.SetAnswer("key7", WebDialogResult.No);

				graph.AllAttendees.View.Answer = WebDialogResult.No;
				graph.AllAttendees.View.SetAnswer("key1", WebDialogResult.No);
				graph.AllAttendees.View.SetAnswer("key3", WebDialogResult.No);
				graph.AllAttendees.View.SetAnswer("key5", WebDialogResult.No);
				graph.AllAttendees.View.SetAnswer("key6", WebDialogResult.No);

				graph.AllAttendeesAndOwner.View.Answer = WebDialogResult.No;

				graph.ConfirmDeleteOtherAttendees.View.SetAnswer("key2", WebDialogResult.No);
			}

			//saving activity
			PostpareActivity(account, item, ref activity);

			return null;
		}
	}
	#endregion
	#region ExchangeEmailsSyncCommand
	public class ExchangeEmailsSyncCommand : ExchangeActivitySyncCommand<CREmailActivityMaint, MessageType, CRSMEmail, CRSMEmail.synchronize, CRSMEmail.noteID, CRSMEmail.ownerID>
	{
		public ExchangeEmailsSyncCommand(MicrosoftExchangeSyncProvider provider)
			: base(provider, PXEmailSyncOperation.EmailsCode, PXExchangeFindOptions.None)
		{
			ExportInserted = true;
			ExportUpdated = false;

			if ((Policy.EmailsAttachments ?? false))
				DefFindOptions = DefFindOptions | PXExchangeFindOptions.IncludeAttachments;
		}

		protected override void ConfigureEnvironment(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			EnsureEnvironmentConfigured<FolderType>(mailboxes,
				//Import Folders
				new PXSyncFolderSpecification(Policy.EmailsFolder, DistinguishedFolderIdNameType.inbox, PXEmailSyncDirection.Directions.Import, false,
					new PXSyncMovingMessageCondition(this, DistinguishedFolderIdNameType.inbox, true, false),
					new PXSyncMovingMessageCondition(this, DistinguishedFolderIdNameType.sentitems, false, true)),
				new PXSyncFolderSpecification(null, DistinguishedFolderIdNameType.inbox, PXEmailSyncDirection.Directions.Import, true),
				new PXSyncFolderSpecification(Policy.EmailsFolder, DistinguishedFolderIdNameType.sentitems, PXEmailSyncDirection.Directions.Import, false,
					new PXSyncMovingMessageCondition(this, DistinguishedFolderIdNameType.inbox, true, false),
					new PXSyncMovingMessageCondition(this, DistinguishedFolderIdNameType.sentitems, false, true)),
				new PXSyncFolderSpecification(null, DistinguishedFolderIdNameType.sentitems, PXEmailSyncDirection.Directions.Import, true),
				//Export Folders
				new PXSyncFolderSpecification(null, DistinguishedFolderIdNameType.sentitems, PXEmailSyncDirection.Directions.Export, true));
		}
		protected override List<PXSyncResult> PrepareImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported = null)
		{
			List<PXSyncResult> result = base.PrepareImport(mailboxes, exported);

			foreach (PXSyncResult item in result)
			{
				try
				{
					if (item.Success && item.NoteID != null)
					{
						foreach (SMEmail activity in PXSelect<SMEmail, Where<SMEmail.refNoteID, Equal<Required<SMEmail.refNoteID>>>>.Select(graph, item.NoteID))
						{
							PX.Data.EP.EMailMessageReceiver.ProcessMessage(activity);
						}
					}
				}
				catch (Exception ex)
				{
					item.Error = ex;
				}
			}

			result.AddRange(ProcessSyncExportUnsynced(mailboxes, exported));

			return result;
		}

		protected override void ExportImportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			// No ExportImportFirst sync for emails
			ExportFirst(policy, direction, mailboxes);
		}
		protected override void LastUpdated(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			// No BiDirectional sync for emails
			ExportFirst(policy, direction, mailboxes);
		}
		protected override void KeepBoth(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			// No KeepBoth sync for emails
			ExportFirst(policy, direction, mailboxes);
		}

		protected virtual void ProcessSyncImportExported(PXExchangeResponce<MessageType> exportedItem)
		{
			IEnumerable<PXExchangeItem<MessageType>> items = Gate.GetItems<MessageType>(false, false, 
				new BasePathToElementType[]
				{
					new PathToExtendedFieldType()
					{
						PropertyTag = "0x3e4a", PropertyType = MapiPropertyTypeType.String
					}
				},
				new PXExchangeItemID(exportedItem.Address, exportedItem.Item.ItemId, exportedItem.Item.DateTimeReceived));
			
			foreach (PXExchangeItem<MessageType> item in items)
			{
				SMEmail email = null;

				if (item.Item.ExtendedProperty != null && item.Item.ExtendedProperty.Any(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a"))
				{
					email = PXSelect<SMEmail, Where<SMEmail.refNoteID, Equal<Required<SMEmail.refNoteID>>>>
						.SelectSingleBound(graph, new Object[0], new Guid(item.Item.ExtendedProperty.First(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a").Item.ToString()));
				}

				if (email == null) return;

				List<PXDataFieldParam> list = new List<PXDataFieldParam>();

				list.Add(new PXDataFieldAssign<SMEmail.messageId>(item.Item.InternetMessageId));
				list.Add(new PXDataFieldRestrict<SMEmail.noteID>(email.NoteID));

				PXDatabase.Update<SMEmail>(list.ToArray());

				return;
			}
		}

		[Obsolete]
		public void SendMessage(PXSyncMailbox mailbox, IEnumerable<CRSMEmail> activities)
		{
			EnsureEnvironmentConfigured<FolderType>(new PXSyncMailbox[] { mailbox }, new PXSyncFolderSpecification(null, DistinguishedFolderIdNameType.sentitems, PXEmailSyncDirection.Directions.Export, true));

			List<PXSyncItemBucket<CRSMEmail>> buckets = new List<PXSyncItemBucket<CRSMEmail>>();
			foreach (CRSMEmail activity in activities)
			{
				EMailSyncReference reference = PXSelect<EMailSyncReference,
					Where<EMailSyncReference.serverID, Equal<Required<EMailSyncReference.serverID>>,
						And<EMailSyncReference.address, Equal<Required<EMailSyncReference.address>>,
						And<EMailSyncReference.noteID, Equal<Required<EMailSyncReference.noteID>>>>>>.SelectSingleBound(graph, null, Account.AccountID, mailbox.Address, activity.NoteID);
				if (reference == null) reference = new EMailSyncReference();

				PXSyncItemStatus status = String.IsNullOrEmpty(reference.BinaryReference) ? PXSyncItemStatus.Inserted : PXSyncItemStatus.Updated;
				PXSyncItemBucket<CRSMEmail> bucket = new PXSyncItemBucket<CRSMEmail>(mailbox, status, reference, activity);

				if (Attachments)
				{
					List<UploadFileWithData> attachments = new List<UploadFileWithData>();
					foreach (UploadFileWithData row in PXSelect<UploadFileWithData, Where<UploadFileWithData.noteID, Equal<Required<UploadFileWithData.noteID>>>>.Select(graph, activity.NoteID))
					{
						if (row.Data == null) continue;
						int size = 1024 * 1024 * 20;
						if (Account.SyncAttachmentSize != null && Account.SyncAttachmentSize > 0)
							size = Account.SyncAttachmentSize.Value * 1024;
						if (row.Data.Length > size) continue;
						attachments.Add(row);
					}
					bucket.Attachments = attachments.ToArray();
				}
				buckets.Add(bucket);
			}

			PXCache cache = graph.Caches[typeof(CRSMEmail)];
			foreach (PXExchangeResponce<MessageType> item in ExportActivityInserted(buckets.Where(b => b.Status == PXSyncItemStatus.Inserted)))
			{
				PXSyncTag<CRSMEmail> tag = item.Tag as PXSyncTag<CRSMEmail>;

				PXSyncResult result = SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Inserted, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, true);
					PostProcessingSuccessfull(cache, PXSyncItemStatus.Inserted, item, tag);
				}, delegate
				{
					PostProcessingFailed(cache, PXSyncItemStatus.Inserted, item, tag);
					return false;
				});
				if (!result.Success)
				{
					string text = PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncItemError, OperationCode, PXEmailSyncDirection.ExportCode, tag.Row.Subject);
					throw new Exception(Provider.CreateErrorMessage(true, text, item.Message, null, item.Details));
				}
			}

			foreach (PXExchangeResponce<MessageType> item in ExportActivityUpdated(buckets.Where(b => b.Status == PXSyncItemStatus.Updated)))
			{
				PXSyncTag<CRSMEmail> tag = item.Tag as PXSyncTag<CRSMEmail>;

				PXSyncResult result = SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.Subject, null, PXSyncItemStatus.Updated, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, true);
					PostProcessingSuccessfull(cache, PXSyncItemStatus.Updated, item, tag);
				}, delegate
				{
					if (item.Code == ResponseCodeType.ErrorItemNotFound)
					{
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, false);
						
						// TODO: refactor!
						PostProcessingFailed(cache, PXSyncItemStatus.Updated, item, tag);
						
						MarkUnsynced(tag);

						return true;
					}
					return false;
				});

				if (!result.Success)
				{
					string text = PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncItemError, OperationCode, PXEmailSyncDirection.ExportCode, tag.Row.Subject);
					throw new Exception(Provider.CreateErrorMessage(true, text, item.Message, null, item.Details));
				}
			}
		}

		protected override BqlCommand GetSelectCommand()
		{
			return PXSelectReadonly2<
					CRSMEmail,
				InnerJoin<Contact, 
					On<CRSMEmail.ownerID, Equal<Contact.userID>>,
				InnerJoin<EPEmployee, 
					On<EPEmployee.defContactID, Equal<Contact.contactID>, 
					And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>,
				Where<CRSMEmail.isIncome, NotEqual<True>,
					And<Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>,// Or<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.failed>>
					And<Where<CRSMEmail.mailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>>>>>>.GetCommand();

			//And<Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>, Or<CRSMEmail.mailAccountID, Like<Required<EMailSyncAccount.emailAccountID>>>>>>>>>.GetCommand();
		}
		protected override BqlCommand GetSelectCommandUnsynced()
		{
			return PXSelectReadonly2<
					CRSMEmail,
				InnerJoin<Contact, 
					On<CRSMEmail.ownerID, Equal<Contact.userID>>,
				InnerJoin<EPEmployee, 
					On<EPEmployee.defContactID, Equal<Contact.contactID>, 
					And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>,
				Where<CRSMEmail.mpstatus, Equal<MailStatusListAttribute.processed>,
					And<CRSMEmail.mailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>>>.GetCommand();
		}

		protected override PXSyncTag ExportInsertedAction(PXSyncMailbox account, MessageType item, CRSMEmail activity)
		{
			ExportInsertedItemPropertyConditional(t => t.From, item, new SingleRecipientType() { Item = PXExchangeConversionHelper.ParceAddress(account.Address) }, account.Address);
			ExportInsertedItemPropertyConditional(t => t.Sender, item, new SingleRecipientType() { Item = PXExchangeConversionHelper.ParceAddress(activity.MailFrom) }, activity.MailFrom);
			ExportInsertedItemPropertyConditional(t => t.ToRecipients, item, PXExchangeConversionHelper.ParceAddresses(activity.MailTo), activity.MailTo);
			ExportInsertedItemPropertyConditional(t => t.ReplyTo, item, PXExchangeConversionHelper.ParceAddresses(activity.MailReply), activity.MailReply);
			ExportInsertedItemPropertyConditional(t => t.CcRecipients, item, PXExchangeConversionHelper.ParceAddresses(activity.MailCc), activity.MailCc);
			ExportInsertedItemPropertyConditional(t => t.BccRecipients, item, PXExchangeConversionHelper.ParceAddresses(activity.MailBcc), activity.MailBcc);

			//SetItemConditional(t => t.ConversationId.Id, item, activity.Ticket, activity.Ticket);
			ExportInsertedItemProperty(t => t.Importance, item, PXExchangeConversionHelper.ParceActivityPriority(activity.Priority));

			return new PXSyncTag() { SendRequired = true, SendSeparateRequired = true };
		}
		protected override PXSyncTag ExportUpdatedAction(PXSyncMailbox account, MessageType item, CRSMEmail activity, List<ItemChangeDescriptionType> updates)
		{
			if (activity.MPStatus == MailStatusListAttribute.Processed || activity.MPStatus == MailStatusListAttribute.InProcess || activity.MPStatus == MailStatusListAttribute.PreProcess || (activity.IsArchived ?? false))
				return new PXSyncTag() { SkipReqired = true };

			updates.AddIfNotEmpty(ExportUpdatedItemPropertyConditional<MessageType>(t => t.From, UnindexedFieldURIType.messageFrom, new SingleRecipientType() { Item = PXExchangeConversionHelper.ParceAddress(account.Address) }, account.Address));
			updates.AddIfNotEmpty(ExportUpdatedItemPropertyConditional<MessageType>(t => t.Sender, UnindexedFieldURIType.messageFrom, new SingleRecipientType() { Item = PXExchangeConversionHelper.ParceAddress(activity.MailFrom) }, activity.MailFrom));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<MessageType>(t => t.ToRecipients, UnindexedFieldURIType.messageToRecipients, PXExchangeConversionHelper.ParceAddresses(activity.MailTo)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<MessageType>(t => t.ReplyTo, UnindexedFieldURIType.messageReplyTo, PXExchangeConversionHelper.ParceAddresses(activity.MailReply)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<MessageType>(t => t.CcRecipients, UnindexedFieldURIType.messageCcRecipients, PXExchangeConversionHelper.ParceAddresses(activity.MailCc)));
			updates.AddIfNotEmpty(ExportUpdatedItemProperty<MessageType>(t => t.BccRecipients, UnindexedFieldURIType.messageBccRecipients, PXExchangeConversionHelper.ParceAddresses(activity.MailBcc)));

			updates.AddIfNotEmpty(ExportUpdatedItemProperty<MessageType>(t => t.Importance, UnindexedFieldURIType.itemImportance, PXExchangeConversionHelper.ParceActivityPriority(activity.Priority)));

			return new PXSyncTag() { SendRequired = true, SendSeparateRequired = true };
		}
		protected override PXSyncTag ImportAction(PXSyncMailbox account, MessageType item, ref CRSMEmail activity)
		{
			if (String.IsNullOrWhiteSpace(item.From?.Item?.EmailAddress))
			{
				activity = null;
				return null;
			}

			PrepareActivity(account, item, false, ref activity);
			if (activity == null) return null;

			activity.MessageId = item.InternetMessageId;
			activity.MessageReference = item.References;
			activity.ClassID = CRActivityClass.Email;
			activity.MailAccountID = account.EmailAccountID;

			//calculating status
			activity.IsIncome = true;
			if (item.IsDraftSpecified && item.IsDraft)
				activity.MPStatus = MailStatusListAttribute.Draft;
			activity.IsIncome = EvaluateIncomming(account.Address, item) ?? false;

			PXCache cache = graph.Caches[typeof(CRSMEmail)];
			CRSMEmail copy = (CRSMEmail)cache.CreateCopy(activity);

			ImportItemProperty(t => t.From, item, v => copy.MailFrom = new MailAddress(v.Item.EmailAddress, v.Item.Name).ToString());
			ImportItemProperty(t => t.ToRecipients, item, v => copy.MailTo = PXExchangeConversionHelper.ParceAddresses(v));
			ImportItemProperty(t => t.ReplyTo, item, v => copy.MailReply = PXExchangeConversionHelper.ParceAddresses(v));
			ImportItemProperty(t => t.CcRecipients, item, v => copy.MailCc = PXExchangeConversionHelper.ParceAddresses(v));
			ImportItemProperty(t => t.BccRecipients, item, v => copy.MailBcc = PXExchangeConversionHelper.ParceAddresses(v));
			ImportItemProperty(t => t.Importance, item, v => copy.Priority = PXExchangeConversionHelper.ParceActivityPriority(v));
			ImportItemProperty(t => t.DateTimeReceived, item, v => copy.StartDate = v, false, account.ExchangeTimeZone);
			activity = (CRSMEmail)graph.Caches[typeof(CRSMEmail)].Update(copy);

			//saving activity
			PostpareActivity(account, item, ref activity);

			return null;
		}

		protected override void PostProcessingSuccessfull(PXCache cache, PXSyncItemStatus status, PXExchangeResponce<MessageType> item, PXSyncTag<CRSMEmail> tag)
		{
			tag.Row.Exception = null;
            tag.Row.StartDate = PXTimeZoneInfo.Now;
			tag.Row.MPStatus = MailStatusListAttribute.Processed;
			tag.Row.UIStatus = ActivityStatusAttribute.Completed;

			cache.Update(tag.Row);
			cache.Persist(PXDBOperation.Update);

			// TODO: refactor!
			ProcessSyncImportExported(item);
		}
		protected override void PostProcessingFailed(PXCache cache, PXSyncItemStatus status, PXExchangeResponce<MessageType> item, PXSyncTag<CRSMEmail> tag)
		{
			tag.Row.Exception = item.Message;
			tag.Row.MPStatus = MailStatusListAttribute.Failed;
			tag.Row.UIStatus = ActivityStatusAttribute.Canceled;

			cache.Update(tag.Row);
			cache.Persist(PXDBOperation.Update);
		}
		protected override void MarkUnsynced(PXSyncTag<CRSMEmail> tag)
		{
			var list = new List<PXDataFieldParam>
			{
				new PXDataFieldAssign<CRActivity.synchronize>(false),
				new PXDataFieldRestrict<CRActivity.noteID>(tag.Row.NoteID.Value)
			};

			PXDatabase.Update<CRActivity>(list.ToArray());
		}
		public bool? EvaluateIncomming(string mailbox, ItemType item)
		{
			// TODO: need refactor!!! 
			MessageType mesage = item as MessageType;

			if (mesage == null || (mesage.IsDraftSpecified && mesage.IsDraft) || String.IsNullOrWhiteSpace(mesage.From?.Item?.EmailAddress))
				return null;

			bool income = true;
			if (mesage.From == null || mesage.ReceivedBy == null)
				income = false;
			else if (mesage.From != null && String.Equals(mesage.From.Item.EmailAddress, mailbox, StringComparison.InvariantCultureIgnoreCase) 
					|| mesage.Sender != null && String.Equals(mesage.Sender.Item.EmailAddress, mailbox, StringComparison.InvariantCultureIgnoreCase))
				income = false;
			else
			{
				bool found = false;
				foreach (EmailAddressType addr in (mesage.ToRecipients ?? new EmailAddressType[0]).Concat(mesage.CcRecipients ?? new EmailAddressType[0]).Concat(mesage.BccRecipients ?? new EmailAddressType[0]))
				{
					if (String.Equals(addr.EmailAddress, mailbox, StringComparison.InvariantCultureIgnoreCase))
					{
						found = true;
						break;
					}
					if (addr.MailboxTypeSpecified && (addr.MailboxType == MailboxTypeType.PrivateDL || addr.MailboxType == MailboxTypeType.PublicDL))
					{
						string[] addresses = Gate.ExpandGroup(addr.EmailAddress);
						if (addresses != null && addresses.Contains(mailbox, StringComparer.InvariantCultureIgnoreCase))
						{
							found = true;
							break;
						}
					}
				}
				if (found) income = found;
			}

			return income;
		}

		protected override bool PreSyncRestrict(MessageType item)
		{
			if (item.IsDraftSpecified && item.IsDraft)
				return true;

			return false;
		}
	}
	#endregion

	#region ExchangeContactsSyncCommand
	public class ExchangeContactsSyncCommand : ExchangeBaseLogicSyncCommand<ContactMaint, Contact, ContactItemType>
	{
		public ExchangeContactsSyncCommand(MicrosoftExchangeSyncProvider provider)
			: base(provider, PXEmailSyncOperation.ContactsCode, PXExchangeFindOptions.IncludeAttachments)
		{
			if ((Policy.ContactsSkipCategory ?? false))
				DefFindOptions = DefFindOptions | PXExchangeFindOptions.IncludeUncategorized;
		}

		protected override void ConfigureEnvironment(PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			EnsureEnvironmentConfigured<ContactsFolderType>(mailboxes, new PXSyncFolderSpecification(Policy.ContactsSeparated == true ? Policy.ContactsFolder : null, DistinguishedFolderIdNameType.contacts));
		}
		protected override List<PXSyncResult> PrepareImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported = null)
		{
			List<PXSyncResult> processed = new List<PXSyncResult>();

			processed.AddRange(ProcessSyncImport(mailboxes.Where(m => m.ImportPreset.Date == null).ToArray(), exported));
			processed.AddRange(ProcessSyncImport(mailboxes.Where(m => m.ImportPreset.Date != null).ToArray(), exported));

			if (processed.Count > 0 && (PXEmailSyncDirection.Parse(Policy.ContactsDirection) & PXEmailSyncDirection.Directions.Export) == PXEmailSyncDirection.Directions.Export)
			{
				PXSyncItemSet items = new PXSyncItemSet(processed.Where(p => p.Success && p.ItemStatus != PXSyncItemStatus.Deleted).Select(p => p as PXSyncItemID));
				processed.AddRange(ProcessSyncExport(mailboxes, items));
			}

			processed.AddRange(ProcessSyncExportUnsynced(mailboxes, null));
			
			return processed;
		}

		protected override void ExportImportFirst(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			// If no conflicts -> change direction
			// ============================================================
			// Get updated items from server
			IEnumerable<PXExchangeItem<ContactItemType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
			var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation

			// Get updated entities from DB
			BqlCommand cmd = GetSelectCommand();

			bool hasConflicts;
			using (new PXReadDeletedScope())
			{
				IEnumerable<PXSyncItemBucket<Contact>> dbEntities = SelectItems<Contact.noteID>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);

				Func<PXSyncItemBucket<Contact>, bool> isModifiedOnBothSides = delegate (PXSyncItemBucket<Contact> bucket)
				{
					var serverItem = serverItemsList.FirstOrDefault(_ => 
						_.Item.ItemId.Id == bucket.Reference.BinaryReference
						&& _.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
					);

					if (serverItem != null)
					{
						return true;
					}
					return false;
				};

				hasConflicts = dbEntities.Where(isModifiedOnBothSides).Any();
			}

			if (hasConflicts)
			{
				ExportFirst(policy, direction, mailboxes);
			}
			else
			{
				ImportFirst(policy, direction, mailboxes);
			}

		}

		protected override void LastUpdated(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			PXSyncItemSet olderOnServer = null;
            if ((direction & PXEmailSyncDirection.Directions.Full) == PXEmailSyncDirection.Directions.Full)
			{
				// Get updated items from server
				IEnumerable<PXExchangeItem<ContactItemType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
				var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation


				// Get updated entities from DB
				BqlCommand cmd = GetSelectCommand();

				using (new PXReadDeletedScope())
				{
					// Deleted items also make conflicts
					IEnumerable<PXSyncItemBucket<Contact, Address, BAccount>> dbEntities = SelectItems<Contact.noteID, Address, BAccount>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);
					
					// Exclude from DB entities ones from server if server version has newer changes
					Func<PXSyncItemBucket<Contact, Address, BAccount>, PXSyncItemID> converter = delegate(PXSyncItemBucket<Contact, Address, BAccount> bucket)
						{
							// select ones to exclude: if server version is newer

							var serverItem = serverItemsList.FirstOrDefault(_ => 
								_.Item.ItemId.Id == bucket.Reference.BinaryReference
								&& _.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
							);

							if (serverItem != null && serverItem.Item.LastModifiedTime < PXTimeZoneInfo.ConvertTimeToUtc((DateTime)bucket.Item1.LastModifiedDateTime, LocaleInfo.GetTimeZone()))
							{
								return new PXSyncItemID(null, bucket.Reference.BinaryReference, null);
							}
							return new PXSyncItemID(null, null, null);
						};

					olderOnServer = new PXSyncItemSet(dbEntities.Select(converter).Where(_ => _.ItemID != null));
				}
			}

			// Now for import ready only that items that are newer than the items in DB
			// Make import, excluding older
			ImportFirst(policy, direction, mailboxes, olderOnServer);
		}

		protected override void KeepBoth(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, PXSyncMailbox[] mailboxes)
		{
			if ((direction & PXEmailSyncDirection.Directions.Full) == PXEmailSyncDirection.Directions.Full)
			{
				// Get updated items from server
				IEnumerable<PXExchangeItem<ContactItemType>> serverItems = GetItems(mailboxes, null, PXExchangeFindOptions.HeadersOnly);
				var serverItemsList = serverItems.ToList(); // To make only one Client-Server conversation


				// Get updated entities from DB
				BqlCommand cmd = GetSelectCommand();

				IEnumerable<PXSyncItemBucket<Contact, Address, BAccount>> dbEntities = SelectItems<Contact.noteID, Address, BAccount>(graph, cmd, mailboxes, null, PXSyncItemStatus.Updated, Attachments);


				PXCache cache = graph.Caches[typeof(Contact)];
				Action<PXSyncItemBucket<Contact, Address, BAccount>> createDublicateInDB = delegate (PXSyncItemBucket<Contact, Address, BAccount> bucket)
					{
						Contact contact = bucket.Item1;
						Contact newContact = (Contact)cache.CreateCopy(contact);
						newContact.NoteID = null;
						cache.Insert(newContact);
						cache.Persist(PXDBOperation.Insert);
						cache.Clear();
					};

				Func<PXSyncItemBucket<Contact, Address, BAccount>, bool> isModifiedOnBothSides = delegate (PXSyncItemBucket<Contact, Address, BAccount> bucket)
					{
						var serverItem = serverItemsList.FirstOrDefault(_ => 
							_.Item.ItemId.Id == bucket.Reference.BinaryReference
							&& _.Item.ItemId.ChangeKey != bucket.Reference.BinaryChangeKey
						);

						if (serverItem != null)
						{
							return true;
						}
						return false;
					};

				dbEntities.Where(isModifiedOnBothSides).ForEach(createDublicateInDB);
			}

			ImportFirst(policy, direction, mailboxes);
		}

		protected override IEnumerable<PXSyncResult> ProcessSyncExport(PXSyncMailbox[] mailboxes, PXSyncItemSet imported)
		{
			if (!mailboxes.Any()) yield break;

			PXCache cache = graph.Caches[typeof(Contact)];
			BqlCommand cmd = GetSelectCommand();

			foreach (PXExchangeResponce<ContactItemType> item in ExportContactsInserted(graph, SelectItems<Contact.noteID, Address, BAccount>(graph, cmd, mailboxes, imported, PXSyncItemStatus.Inserted, Attachments)))
			{
				PXSyncTag<Contact> tag = item.Tag as PXSyncTag<Contact>;

				yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.DisplayName, null, PXSyncItemStatus.Inserted, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, null, true);
				});
			}
			
			foreach (PXExchangeResponce<ContactItemType> item in ExportContactsUpdated(graph, SelectItems<Contact.noteID, Address, BAccount>(graph, cmd, mailboxes, imported, PXSyncItemStatus.Updated, Attachments)))
			{
				PXSyncTag<Contact> tag = item.Tag as PXSyncTag<Contact>;

				yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.DisplayName, null, PXSyncItemStatus.Updated, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, null, true);
				}, delegate
				{
					if (!item.Success && item.Code == ResponseCodeType.ErrorItemNotFound)
					{
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, false);

						var list = new List<PXDataFieldParam>
						{
							new PXDataFieldAssign<Contact.synchronize>(false),
							new PXDataFieldRestrict<Contact.contactID>(tag.Row.ContactID.Value)
						};

						PXDatabase.Update<Contact>(list.ToArray());

						return true;
					}
					return false;
				});
			}

			cmd = GetDeleteCommand();

			foreach (PXExchangeResponce<ContactItemType> item in ExportContactsDeleted(graph, SelectItems<Contact.noteID, BAccount>(graph, cmd, mailboxes, null, PXSyncItemStatus.Deleted, Attachments)))
			{
				PXSyncTag<Contact> tag = item.Tag as PXSyncTag<Contact>;
				
				yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.DisplayName, null, PXSyncItemStatus.Deleted, delegate
				{
					
					if (tag.Row.DeletedDatabaseRecord ?? false)
					{
						// is deleted record
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, true);
					}
					else
					{
						// is inactive record
						var list = new List<PXDataFieldParam>
						{
							new PXDataFieldAssign<Contact.synchronize>(false),
							new PXDataFieldRestrict<Contact.contactID>(tag.Row.ContactID.Value)
						};

						PXDatabase.Update<Contact>(list.ToArray());

						DeleteReference(item.Address, tag.Row.NoteID);
					}
				});
			}
		}
		protected override IEnumerable<PXSyncResult> ProcessSyncExportUnsynced(PXSyncMailbox[] mailboxes, PXSyncItemSet exported)
		{
			if (!mailboxes.Any()) yield break;

			PXCache cache = graph.Caches[typeof(Contact)];
			BqlCommand cmd = GetSelectCommand();
			
			foreach (PXExchangeResponce<ContactItemType> item in ExportContactsUnsynced(graph, SelectItems<Contact.noteID, Address, BAccount>(graph, cmd, mailboxes, exported, PXSyncItemStatus.Unsynced, Attachments)))
			{
				PXSyncTag<Contact> tag = item.Tag as PXSyncTag<Contact>;

				yield return SafeOperation(item, tag.Ref.BinaryReference, tag.Ref.NoteID, tag.Row.DisplayName, null, PXSyncItemStatus.Unsynced, delegate
				{
					SaveReference(item.Address, tag.Row.NoteID, item.Item.ItemId, item.Item.ConversationId, null, true);
				}, delegate
				{
					if (!item.Success && item.Code == ResponseCodeType.ErrorItemNotFound)
					{
						SaveReference(item.Address, tag.Row.NoteID, null, null, null, false);

						var list = new List<PXDataFieldParam>
						{
							new PXDataFieldAssign<Contact.synchronize>(false),
							new PXDataFieldRestrict<Contact.contactID>(tag.Row.ContactID.Value)
						};

						PXDatabase.Update<Contact>(list.ToArray());

						return true;
					}
					return false;
				});
			}
		}
		protected override IEnumerable<PXSyncResult> ProcessSyncImport(PXSyncMailbox[] mailboxes, PXSyncItemSet exported)
		{
			if (!mailboxes.Any()) yield break;

			PXCache cache = graph.Caches[typeof(Contact)];

			IEnumerable<PXExchangeItem<ContactItemType>> contacts = GetItems(mailboxes, exported, PXExchangeFindOptions.PlainText,
				Tuple.Create("0x3a45", MapiPropertyTypeType.String),
				Tuple.Create("0x3a4d", MapiPropertyTypeType.Short),
				Tuple.Create("0x3e4a", MapiPropertyTypeType.String));
			int i = 0;

			foreach (PXExchangeItem<ContactItemType> fitem in contacts)
			{
				if (i++ % 100 == 0)
				{
					graph = PXGraph.CreateInstance<ContactMaint>();
					cache = graph.Caches[typeof(Contact)];
					cache.AllowDelete = true;
					cache.AllowInsert = true;
					cache.AllowUpdate = true;
				}

				PXExchangeItem<ContactItemType> item = fitem;
				PXSyncMailbox mailbox = mailboxes.FirstOrDefault(m => String.Equals(m.Address, item.Address, StringComparison.InvariantCultureIgnoreCase));

				bool merge = false;

				BqlCommand select = PXSelectJoin<Contact,
					LeftJoin<EMailSyncReference,
						On<EMailSyncReference.noteID, Equal<Contact.noteID>,
						And<EMailSyncReference.serverID, Equal<Required<EMailSyncReference.serverID>>,
						And<EMailSyncReference.address, Equal<Required<EMailSyncReference.address>>>>>>>.GetCommand();

				Contact contact = null;
				EMailSyncReference reference = null;
				bool foundByNoteID = false;

				if (item.Item.ExtendedProperty != null && item.Item.ExtendedProperty.Any(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a"))
				{
					using (new PXReadDeletedScope())
					{
						select = select.WhereNew<Where<Contact.noteID, Equal<Required<Contact.noteID>>,
							And<Contact.synchronize, Equal<True>>>>();

						var result = (PXResult<Contact, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
							Account.AccountID,
							mailbox.Address,
							new Guid(item.Item.ExtendedProperty.First(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a").Item.ToString()));

						contact = result;
						reference = result;

						if (contact != null)
							foundByNoteID = true;
					}
				}
				if (!foundByNoteID)
				{
					using (new PXReadDeletedScope())
					{
						select = select.WhereNew<Where<EMailSyncReference.binaryReference, Equal<Required<EMailSyncReference.binaryReference>>>>();

						var result = (PXResult<Contact, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
							Account.AccountID,
							mailbox.Address,
							item.Item.ItemId.Id);

						contact = result;
						reference = result;
					}
				}
				
				if (contact != null && Policy.ContactsFilter != null)
				{
					if ((reference!= null && reference.BinaryChangeKey == item.Item.ItemId.ChangeKey) || (contact.DeletedDatabaseRecord ?? false))
						continue;

					if (Policy.ContactsFilter == PXEmailSyncContactsFilter.OwnerCode)
						select = select.WhereAnd<Where<Contact.ownerID, Equal<Required<Contact.ownerID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();
					else if (Policy.ContactsFilter == PXEmailSyncContactsFilter.WorkgroupCode)
						select = select.WhereAnd<Where<Contact.workgroupID, InMember<Required<Contact.userID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();

					Contact contactFiltered = null;
					if (foundByNoteID)
					{
						using (new PXReadDeletedScope())
						{
							var result = (PXResult<Contact, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
								Account.AccountID,
								mailbox.Address,
								new Guid(item.Item.ExtendedProperty.First(_ => _.ExtendedFieldURI.PropertyTag == "0x3e4a").Item.ToString()),
								Cache.EmployeeCache[mailbox.EmployeeID]);

							contactFiltered = result;
						}
					}
					else
					{
						using (new PXReadDeletedScope())
						{
							var result = (PXResult<Contact, EMailSyncReference>) new PXView(graph, true, select).SelectSingle(
								Account.AccountID,
								mailbox.Address,
								item.Item.ItemId.Id,
								Cache.EmployeeCache[mailbox.EmployeeID]);

							contactFiltered = result;
						}
					}

					//if we didn't find contact with filter, we should not update this contact (contact hs been reowned), but we can create new one.
					if (contactFiltered == null) contact = null;
				}
				else if (contact == null && (Policy.ContactsMerge ?? false) && GetEmailByType(item.Item.EmailAddresses, EmailAddressKeyType.EmailAddress1) != null)
				{
					contact = PXSelect<Contact,
						Where<Contact.eMail, Equal<Required<Contact.eMail>>,
						And<Where<Contact.contactType, Equal<ContactTypesAttribute.person>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>>
						.SelectSingleBound(graph, new Object[0], GetEmailByType(item.Item.EmailAddresses, EmailAddressKeyType.EmailAddress1));
					if (contact != null) merge = true;
				}
				
				Guid? noteid = contact == null ? Guid.Empty : contact.NoteID;
				string key = !String.IsNullOrWhiteSpace(item.Item.DisplayName)
					? item.Item.DisplayName
					: !String.IsNullOrWhiteSpace(GetEmailByType(item.Item.EmailAddresses, EmailAddressKeyType.EmailAddress1))
						? GetEmailByType(item.Item.EmailAddresses, EmailAddressKeyType.EmailAddress1)
						: item.Item.ItemId.Id;
				PXSyncItemStatus status = contact == null ? PXSyncItemStatus.Inserted : PXSyncItemStatus.Updated;
				yield return SafeOperation(mailbox.Address, item.Item.ItemId.Id, noteid, key, null, status, delegate
				{
					if (!IsFullyFilled(item.Item)) return;

					bool isNewRecord = false;
					if (contact == null)
					{
						contact = new Contact();
						contact = (Contact)cache.Insert(contact);

						if (!String.IsNullOrEmpty(Policy.ContactsClass))
							contact.ClassID = Policy.ContactsClass;

						isNewRecord = true;
					}
					else if (contact.Synchronize != true || contact.ContactType == ContactTypesAttribute.Employee) return;

					if (!contact.IsActive ?? true)
					{
						contact.Synchronize = false;

						contact = (Contact)cache.Update(contact);
						graph.Actions.PressSave();

						return;
					}

					contact.Synchronize = true;
					noteid = contact.NoteID;

					//Owner sync
					Guid owner;
					if (cache.GetStatus(contact) == PXEntryStatus.Inserted &&
						Cache.EmployeeCache.TryGetValue(mailbox.EmployeeID, out owner))
					{
						if (contact.OwnerID != owner)
						{
							contact.WorkgroupID = PXOwnerSelectorAttribute.DefaultWorkgroup(graph, owner);
							contact.OwnerID = owner;
						}
					}

					//Contact
					ImportContactSync(mailbox, contact, item.Item, merge);
					contact = (Contact)cache.Update(contact);
					bool modified = PXExchangeReflectionHelper.IsObjectModified<Contact>(cache, contact, false);

					//Address
					Address address = (Address)graph.AddressCurrent.Cache.Current;
					if (address == null) address = (Address)graph.AddressCurrent.View.SelectSingle();

					PhysicalAddressDictionaryEntryType itemAddress = 
						PXExchangeConversionHelper.GetValueByType(item.Item.PhysicalAddresses, PhysicalAddressKeyType.Business) 
						?? PXExchangeConversionHelper.GetValueByType(item.Item.PhysicalAddresses, PhysicalAddressKeyType.Home);

					ImportAddressSync(null, address, itemAddress, merge);
					address = (Address)graph.AddressCurrent.Cache.Update(address);
					if (!modified) modified = PXExchangeReflectionHelper.IsObjectModified<Contact>(graph.AddressCurrent.Cache, address, false);

					if (item.Item.Body != null)
					{
						if (PXNoteAttribute.GetNote(graph.Caches[typeof(Contact)], contact) != item.Item.Body.Value)
						{
							modified = true;
							PXNoteAttribute.SetNote(graph.Caches[typeof(Contact)], contact, PX.Web.UI.RichStyle.RemoveViewStyle(item.Item.Body.Value));
						}
					}

					contact = (Contact)cache.Update(contact);

					//saving data
					graph.Actions.PressSave();

					//saving attachments
					string fileName;
					if (SaveAttachmentsSync(graph, contact, item.Attachments, out fileName))
					{
						modified = true;
						if (!String.IsNullOrEmpty(fileName))
						{
							contact.Img = fileName;
							contact = (Contact)cache.Update(contact);
						}
					}

					//may be it was wrong idea.
					if (modified && mailboxes.Any(m => m.ExportPreset.Date != null))
					{
						var timeTag = PXTimeZoneInfo.ConvertTimeToUtc(mailboxes.Where(m => m.ExportPreset.Date != null).Max(m => m.ExportPreset.Date.Value), LocaleInfo.GetTimeZone());
						if(timeTag != null)
							PXTimeTagAttribute.UpdateTag<Contact.noteID>(cache, contact, timeTag);												
					}

					//saving data with attachments
					graph.Actions.PressSave();

					//saving reference
					SaveReference(mailbox.Address, contact.NoteID, item.Item.ItemId, item.Item.ConversationId, item.Hash, !(isNewRecord || !foundByNoteID));
				}, true).DoWith(r => new PXSyncResult(r, null, noteid, null, status) { Reprocess = merge });

				graph.Clear();
			}
		}

		protected override BqlCommand GetSelectCommand()
		{
			BqlCommand cmd = PXSelectReadonly2<Contact,
				InnerJoin<Address, 
					On<Contact.defAddressID, Equal<Address.addressID>>,
				LeftJoin<BAccount, 
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>,
				Where2<Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>,
					And<Where<Contact.synchronize, Equal<True>>>>>.GetCommand();
			if (Policy.ContactsFilter == PXEmailSyncContactsFilter.OwnerCode)
				cmd = cmd.WhereAnd<Where<Contact.ownerID, Equal<Required<Contact.ownerID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();
			if (Policy.ContactsFilter == PXEmailSyncContactsFilter.WorkgroupCode)
				cmd = cmd.WhereAnd<Where<Contact.workgroupID, InMember<Required<Contact.userID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();
			return cmd;
		}
		protected BqlCommand GetDeleteCommand()
		{
			BqlCommand cmd = PXSelectReadonly2<Contact,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
				Where2<
					Where2<
						Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
							Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>,
						And<Where<Contact.synchronize, Equal<True>, And<Contact.deletedDatabaseRecord, Equal<True>>>>>,
					Or<
						Where<Contact.isActive, Equal<False>, And<Contact.synchronize, Equal<True>, And<Contact.deletedDatabaseRecord, Equal<False>>>>>>
				>.GetCommand();
			if (Policy.ContactsFilter == PXEmailSyncContactsFilter.OwnerCode)
				cmd = cmd.WhereAnd<Where<Contact.ownerID, Equal<Required<Contact.ownerID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();
			if (Policy.ContactsFilter == PXEmailSyncContactsFilter.WorkgroupCode)
				cmd = cmd.WhereAnd<Where<Contact.workgroupID, InMember<Required<Contact.userID>>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>();
			return cmd;
		}

		protected virtual IEnumerable<PXExchangeResponce<ContactItemType>> ExportContactsInserted(ContactMaint graph, IEnumerable<PXSyncItemBucket<Contact, Address, BAccount>> inserted)
		{
			Func<PXSyncItemBucket<Contact, Address, BAccount>, PXExchangeRequest<ContactItemType, ItemType>> converter = delegate(PXSyncItemBucket<Contact, Address, BAccount> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				EMailSyncReference reference = bucket.Reference;
				Contact contact = bucket.Item1;
				Address address = bucket.Item2;
				BAccount baccount = bucket.Item3;

				ContactItemType item = new ContactItemType();
				ExportInsertedItemProperty(t => t.GivenName, item, contact.FirstName);
				ExportInsertedItemProperty(t => t.Surname, item, contact.LastName);
				ExportInsertedItemProperty(t => t.MiddleName, item, contact.MidName);
				ExportInsertedItemProperty(t => t.DisplayName, item, contact.DisplayName);
				ExportInsertedItemProperty(t => t.Subject, item, contact.DisplayName);
				ExportInsertedItemProperty(t => t.BusinessHomePage, item, GenerateLink(contact));
				ExportInsertedItemProperty(t => t.Birthday, item, contact.DateOfBirth, mailbox.ExchangeTimeZone);
				ExportInsertedItemProperty(t => t.SpouseName, item, contact.Spouse);
				ExportInsertedItemProperty(t => t.JobTitle, item, contact.Salutation);
				ExportInsertedItemProperty(t => t.CompanyName, item, baccount != null && baccount.AcctName != null ? baccount.AcctName : contact.FullName);

				ExportInsertedItemProperty(t => t.ExtendedProperty, item, PXExchangeConversionHelper.GetExtendedProperties(
					Tuple.Create("0x3a45", MapiPropertyTypeType.String, (Object)contact.Title),
					Tuple.Create("0x3a4d", MapiPropertyTypeType.Short, PXExchangeConversionHelper.ParceAcGender(contact.Gender)),
					Tuple.Create("0x3e4a", MapiPropertyTypeType.String, (Object)contact.NoteID.ToString())));

				ExportInsertedItemProperty(t => t.Categories, item, new string[] { Policy.Category });
				ExportInsertedItemProperty(t => t.FileAsMapping, item, String.IsNullOrEmpty(item.CompanyName) ? FileAsMappingType.LastCommaFirst : FileAsMappingType.LastCommaFirstCompany);
				ExportInsertedItemProperty(t => t.Body, item, new BodyType() { BodyType1 = BodyTypeType.Text, Value = PXNoteAttribute.GetNote(graph.Caches[typeof(Contact)], contact) ?? String.Empty });

				ExportInsertedItemPropertyConditional(t => t.EmailAddresses, item, new[] { new EmailAddressDictionaryEntryType() { Key = EmailAddressKeyType.EmailAddress1, Value = contact.EMail } }, contact.EMail);
				item.PhoneNumbers = new[]
				{
					PXExchangeConversionHelper.ParcePhone(contact.FaxType, contact.Fax),
					PXExchangeConversionHelper.ParcePhone(contact.Phone1Type, contact.Phone1),
					PXExchangeConversionHelper.ParcePhone(contact.Phone2Type, contact.Phone2),
					PXExchangeConversionHelper.ParcePhone(contact.Phone3Type, contact.Phone3),
				};

				string addressLines = String.Empty;
				foreach (string line in new string[] { address.AddressLine1, address.AddressLine2, address.AddressLine3 })
				{
					string converted = (line ?? String.Empty).Trim(' ', '\n', '\r');
					if (!String.IsNullOrEmpty(converted.Trim()))
					{
						addressLines += addressLines.Length > 0 ? Environment.NewLine + converted : converted;
					}
				}
				item.PhysicalAddresses = new PhysicalAddressDictionaryEntryType[]
				{
					new PhysicalAddressDictionaryEntryType( ) 
					{
						Key = PhysicalAddressKeyType.Business, 
						City = address.City, 
						CountryOrRegion = address.CountryID, 
						PostalCode = address.PostalCode, 
						State = address.State, 
						Street = addressLines
					}
				};

				string contactImage = bucket.Attachments == null ? null :
					(bucket.Attachments.FirstOrDefault(a => a.Name == contact.Img).With(a => a.Name)
					?? bucket.Attachments.FirstOrDefault(a => SitePolicy.AllowedImageTypesExt.Any(i => a.Name.EndsWith(i))).With(a => a.Name));

				return new PXExchangeRequest<ContactItemType, ItemType>(mailbox.Folders.First(f => f.IsExport), item, bucket.ID, new PXSyncTag<Contact>(contact, mailbox, reference), ConvertAttachment(bucket.Attachments, contactImage));
			};

			return Gate.CreateItems(inserted.Select(converter));
		}
		protected virtual IEnumerable<PXExchangeResponce<ContactItemType>> ExportContactsUpdated(ContactMaint graph, IEnumerable<PXSyncItemBucket<Contact, Address, BAccount>> updated)
		{
			Func<PXSyncItemBucket<Contact, Address, BAccount>, PXExchangeRequest<ContactItemType, ItemChangeType>> converter = delegate(PXSyncItemBucket<Contact, Address, BAccount> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				EMailSyncReference reference = bucket.Reference;
				Contact contact = bucket.Item1;
				Address address = bucket.Item2;
				BAccount baccount = bucket.Item3;

				List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.GivenName, UnindexedFieldURIType.contactsGivenName, contact.FirstName));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.Surname, UnindexedFieldURIType.contactsSurname, contact.LastName));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.MiddleName, UnindexedFieldURIType.contactsMiddleName, contact.MidName));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.JobTitle, UnindexedFieldURIType.contactsJobTitle, contact.Salutation));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.DisplayName, UnindexedFieldURIType.contactsDisplayName, contact.DisplayName));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.Subject, UnindexedFieldURIType.itemSubject, contact.DisplayName));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.BusinessHomePage, UnindexedFieldURIType.contactsBusinessHomePage, GenerateLink(contact)));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.Birthday, UnindexedFieldURIType.contactsBirthday, contact.DateOfBirth, mailbox.ExchangeTimeZone));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.SpouseName, UnindexedFieldURIType.contactsSpouseName, contact.Spouse));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.CompanyName, UnindexedFieldURIType.contactsCompanyName, baccount != null && baccount.AcctName != null ? baccount.AcctName : contact.FullName));

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.Body, UnindexedFieldURIType.itemBody,
					new BodyType() { BodyType1 = BodyTypeType.Text, Value = PXNoteAttribute.GetNote(graph.Caches[typeof(Contact)], contact) ?? String.Empty }));

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.ExtendedProperty, "0x3a45", MapiPropertyTypeType.String, (Object)contact.Title));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.ExtendedProperty, "0x3a4d", MapiPropertyTypeType.Short, PXExchangeConversionHelper.ParceAcGender(contact.Gender)));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.ExtendedProperty, "0x3e4a", MapiPropertyTypeType.String, (Object)contact.NoteID.ToString()));

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.EmailAddresses, DictionaryURIType.contactsEmailAddress, "EmailAddress1",
					new[] { new EmailAddressDictionaryEntryType() { Key = EmailAddressKeyType.EmailAddress1, Value = contact.EMail } }, contact.EMail));

				PhoneNumberDictionaryEntryType fax = PXExchangeConversionHelper.ParcePhone(contact.FaxType, contact.Fax);
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhoneNumbers, DictionaryURIType.contactsPhoneNumber, fax.Key.ToString(), new[] { fax }, contact.Fax));
				PhoneNumberDictionaryEntryType phone1 = PXExchangeConversionHelper.ParcePhone(contact.Phone1Type, contact.Phone1);
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhoneNumbers, DictionaryURIType.contactsPhoneNumber, phone1.Key.ToString(), new[] { phone1 }, contact.Phone1));
				PhoneNumberDictionaryEntryType phone2 = PXExchangeConversionHelper.ParcePhone(contact.Phone2Type, contact.Phone2);
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhoneNumbers, DictionaryURIType.contactsPhoneNumber, phone2.Key.ToString(), new[] { phone2 }, contact.Phone2));
				PhoneNumberDictionaryEntryType phone3 = PXExchangeConversionHelper.ParcePhone(contact.Phone3Type, contact.Phone3);
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhoneNumbers, DictionaryURIType.contactsPhoneNumber, phone3.Key.ToString(), new[] { phone3 }, contact.Phone3));

				//purge removed phones
				foreach (PhoneNumberDictionaryEntryType phone in PXExchangeConversionHelper.PurgePhones(fax, phone1, phone2, phone3))
					updates.AddIfNotEmpty(DeleteItemProperty<ContactItemType>(t => t.PhoneNumbers, DictionaryURIType.contactsPhoneNumber, phone.Key.ToString()));

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhysicalAddresses, DictionaryURIType.contactsPhysicalAddressCity, "Business",
					new[] { new PhysicalAddressDictionaryEntryType() { Key = PhysicalAddressKeyType.Business, City = address.City } }, address.City));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhysicalAddresses, DictionaryURIType.contactsPhysicalAddressCountryOrRegion, "Business",
					new[] { new PhysicalAddressDictionaryEntryType() { Key = PhysicalAddressKeyType.Business, CountryOrRegion = address.CountryID } }, address.CountryID));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhysicalAddresses, DictionaryURIType.contactsPhysicalAddressPostalCode, "Business",
					new[] { new PhysicalAddressDictionaryEntryType() { Key = PhysicalAddressKeyType.Business, PostalCode = address.PostalCode } }, address.PostalCode));
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhysicalAddresses, DictionaryURIType.contactsPhysicalAddressState, "Business",
					new[] { new PhysicalAddressDictionaryEntryType() { Key = PhysicalAddressKeyType.Business, State = address.State } }, address.State));

				string addressLines = String.Empty;
				foreach (string line in new string[] { address.AddressLine1, address.AddressLine2, address.AddressLine3 })
				{
					string converted = (line ?? String.Empty).Trim(' ', '\n', '\r');
					if (!String.IsNullOrEmpty(converted.Trim()))
					{
						addressLines += addressLines.Length > 0 ? Environment.NewLine + converted : converted;
					}
				}
				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.PhysicalAddresses, DictionaryURIType.contactsPhysicalAddressStreet, "Business",
					new[] { new PhysicalAddressDictionaryEntryType() { Key = PhysicalAddressKeyType.Business, Street = addressLines } }, addressLines));

				string contactImage = bucket.Attachments == null ? null :
					(bucket.Attachments.FirstOrDefault(a => a.Name == contact.Img).With(a => a.Name)
					?? bucket.Attachments.FirstOrDefault(a => SitePolicy.AllowedImageTypesExt.Any(i => a.Name.EndsWith(i))).With(a => a.Name));

				ItemIdType id = new ItemIdType() { Id = reference.BinaryReference };
				return new PXExchangeRequest<ContactItemType, ItemChangeType>(mailbox.Folders.First(f => f.IsExport),
					new ItemChangeType() { Item = id, Updates = updates.ToArray() }, bucket.ID, new PXSyncTag<Contact>(contact, mailbox, reference), ConvertAttachment(bucket.Attachments, contactImage));
			};

			return Gate.UpdateItems(updated.Select(converter));
		}
		protected virtual IEnumerable<PXExchangeResponce<ContactItemType>> ExportContactsUnsynced(ContactMaint graph, IEnumerable<PXSyncItemBucket<Contact, Address, BAccount>> updated)
		{
			Func<PXSyncItemBucket<Contact, Address, BAccount>, PXExchangeRequest<ContactItemType, ItemChangeType>> converter = delegate (PXSyncItemBucket<Contact, Address, BAccount> bucket)
			{
				PXSyncMailbox mailbox = bucket.Mailbox;
				EMailSyncReference reference = bucket.Reference;
				Contact contact = bucket.Item1;

				List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();

				updates.AddIfNotEmpty(ExportUpdatedItemProperty<ContactItemType>(t => t.ExtendedProperty, "0x3e4a", MapiPropertyTypeType.String, (Object)contact.NoteID.ToString()));

				string contactImage = bucket.Attachments == null ? null :
					(bucket.Attachments.FirstOrDefault(a => a.Name == contact.Img).With(a => a.Name)
					?? bucket.Attachments.FirstOrDefault(a => SitePolicy.AllowedImageTypesExt.Any(i => a.Name.EndsWith(i))).With(a => a.Name));

				ItemIdType id = new ItemIdType() { Id = reference.BinaryReference };
				return new PXExchangeRequest<ContactItemType, ItemChangeType>(mailbox.Folders.First(f => f.IsExport),
					new ItemChangeType() { Item = id, Updates = updates.ToArray() }, bucket.ID, new PXSyncTag<Contact>(contact, mailbox, reference), ConvertAttachment(bucket.Attachments, contactImage));
			};

			return Gate.UpdateItems(updated.Select(converter));
		}
		protected virtual IEnumerable<PXExchangeResponce<ContactItemType>> ExportContactsDeleted(ContactMaint graph, IEnumerable<PXSyncItemBucket<Contact, BAccount>> deleted)
		{
			Func<PXSyncItemBucket<Contact, BAccount>, PXExchangeRequest<ContactItemType, ItemType>> converter = delegate (PXSyncItemBucket<Contact, BAccount> bucket)
			{
				string id = Guid.NewGuid().ToString();
				PXSyncMailbox mailbox = bucket.Mailbox;
				Contact contact = bucket.Item1;
				EMailSyncReference reference = bucket.Reference;

				ContactItemType item = new ContactItemType();
				item.ItemId = new ItemIdType() { Id = reference.BinaryReference };

				return new PXExchangeRequest<ContactItemType, ItemType>(mailbox.Folders.First(f => f.IsExport), item, id, new PXSyncTag<Contact>(contact, mailbox, bucket.Reference));
			};

			return Gate.DeleteItems(deleted.Select(converter));
		}
		protected virtual string GenerateLink(Contact row)
		{
			if (!(Policy.ContactsGenerateLink ?? false)) return row.WebSite;

			PXCache cache = graph.Caches[typeof(Contact)];
			PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(typeof(ContactMaint));

			StringBuilder bld = new StringBuilder();
			bld.Append((Policy.LinkTemplate ?? String.Empty).TrimEnd('/'));
			bld.Append(PX.Common.PXUrl.ToAbsoluteUrl(node.Url));
			bld.Append(bld.ToString().Contains("?") ? "&" : "?");

			string company = PXAccess.GetCompanyName();
			if (!String.IsNullOrEmpty(company))
			{
				bld.Append("CompanyID=" + System.Web.HttpUtility.UrlEncode(company));
				bld.Append("&");
			}
			bld.Append("ScreenId=" + node.ScreenID);

			foreach (string key in cache.Keys)
			{
				string k = key;
				object v = cache.GetValue(row, key);
				if (v != null)
				{
					string val = v.ToString();
					if (val.Contains('\\')) val = val.Replace("\\", "%5C");

					bld.Append("&");
					bld.Append(k + "=" + System.Web.HttpUtility.UrlEncode(val));
				}
			}

			return bld.ToString();
		}
		protected virtual void ImportContactSync(PXSyncMailbox account, Contact contact, ContactItemType item, bool merge)
		{
			string lastname = !String.IsNullOrWhiteSpace(item.CompleteName.LastName) ? item.CompleteName.LastName : item.CompleteName.FullName;

			if (!merge) //we should keep original name from contact owner
			{
				if (!String.IsNullOrEmpty(lastname)) contact.LastName = lastname;
				if (!String.IsNullOrEmpty(item.CompleteName.FirstName)) contact.FirstName = item.CompleteName.FirstName;
				if (!String.IsNullOrEmpty(item.CompleteName.MiddleName)) contact.MidName = item.CompleteName.MiddleName;
			}
			if (!String.IsNullOrEmpty(item.CompleteName.Title) || !merge) contact.Title = item.CompleteName.Title;
			if (!String.IsNullOrEmpty(item.JobTitle) || !merge) contact.Salutation = item.JobTitle;

			if (contact.BAccountID == null && !String.IsNullOrEmpty(item.CompanyName)) ImportItemProperty(t => t.CompanyName, item, v => contact.FullName = v, merge);

			ImportItemProperty(t => t.Birthday, item, v => contact.DateOfBirth = v, merge, account.ExchangeTimeZone);
			ImportItemProperty(t => t.SpouseName, item, v => contact.Spouse = v, merge);
			ImportItemProperty(t => t.EmailAddresses, item, v => contact.EMail = GetEmailByType(v, EmailAddressKeyType.EmailAddress1), merge);
			if (!(Policy.ContactsGenerateLink ?? false) || graph.Caches[typeof(Contact)].GetStatus(contact) == PXEntryStatus.Inserted)
				ImportItemProperty(t => t.BusinessHomePage, item, v => contact.WebSite = v, merge);

			if (item.ExtendedProperty != null)
			{
				foreach (ExtendedPropertyType ext in item.ExtendedProperty)
				{
					switch (ext.ExtendedFieldURI.PropertyTag)
					{
						case "0x3a4d":
							contact.Gender = PXExchangeConversionHelper.ParceExGender(ext.Item);
							break;
						case "0x3a45":
							contact.Title = ext.Item as string;
							break;
					}
				}
			}

			//phones evaluation
			if (item.PhoneNumbers != null)
			{
				Int16 acucunter = 1;
				foreach (PhoneNumberDictionaryEntryType entry in item.PhoneNumbers)
				{
					string type, value;
					PhoneNumberDictionaryEntryType phone = entry;

					if (!PXExchangeConversionHelper.ParcePhone(phone, out type, out value)) continue;
					if (value == null && merge) continue;

					if (phone.Key == PhoneNumberKeyType.BusinessFax || phone.Key == PhoneNumberKeyType.OtherFax || phone.Key == PhoneNumberKeyType.HomeFax)
					{
						contact.FaxType = type;
						contact.Fax = value;
					}
					else
					{
						if (acucunter == 1)
						{
							contact.Phone1Type = type;
							contact.Phone1 = value;
						}
						else if (acucunter == 2)
						{
							contact.Phone2Type = type;
							contact.Phone2 = value;
						}
						else if (acucunter == 3)
						{
							contact.Phone3Type = type;
							contact.Phone3 = value;
						}
						acucunter++;
					}
				}
			}
		}
		protected virtual void ImportAddressSync(PXSyncMailbox account, Address address, PhysicalAddressDictionaryEntryType item, bool merge)
		{
			if (item != null)
			{ 
				if (!String.IsNullOrEmpty(item.City) || !merge) address.City = item.City;
				if (!String.IsNullOrEmpty(item.Street) || !merge)
				{
					address.AddressLine1 = null;
					address.AddressLine2 = null;
					address.AddressLine3 = null;
					if (!String.IsNullOrEmpty(item.Street))
					{
						string[] lines = item.Street.Split(new string[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
						if (lines.Length > 0)
						{
							address.AddressLine1 = lines[0];
							if (lines.Length == 2)
							{
								address.AddressLine2 = lines[1];
							}
							else address.AddressLine2 = String.Join(" ", lines.Skip(1).ToArray());
						}
					}
				}
				if (!String.IsNullOrEmpty(item.PostalCode)) 
					address.PostalCode = item.PostalCode;

				if (!String.IsNullOrEmpty(item.CountryOrRegion))
					address.CountryID = item.CountryOrRegion;

				if (!String.IsNullOrEmpty(item.State))
					address.State = item.State;
			}
			if (address.CountryID == null)
			{
				Address branchAddress = PXSelectJoin<Address,
					InnerJoin<CR.BAccount, On<CR.BAccount.defAddressID, Equal<Address.addressID>>,
					InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<BAccount.bAccountID>,
						And<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>>>.SelectSingleBound(graph, null, null);

				address.CountryID = branchAddress.CountryID;
			}
		}

		protected virtual string GetEmailByType(EmailAddressDictionaryEntryType[] types, EmailAddressKeyType key, bool checkEmpty = false)
		{
			if (types != null)
			{
				foreach (EmailAddressDictionaryEntryType item in types)
				{
					if (item.Key == key)
					{
						if (item.Value == null || item.Value.Contains("@")) return item.Value;
						if (!item.Value.Contains("/") && !item.Value.Contains("=")) return item.Value;
						return Gate.ResolveName(item.Value);
					}
				}
			}
			if (checkEmpty) throw new PXException(PXMessages.LocalizeFormatNoPrefix(Messages.EmailTypeNotFound, key.ToString()));
			return null;
		}

		protected override bool IsFullyFilled(ContactItemType item)
		{
			return item.CompleteName != null
					&& item.CompleteName != null;	// for the future 
		}
	}
	#endregion
}