using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;
using PX.Common.Mail;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.SM;
using PX.Data;
using PX.Data.Update;
using System.Reflection;
using PX.Data.Update.ExchangeService;
using PX.Data.Update.WebServices;

namespace PX.Objects.CS.Email
{
	#region PXItemID
	public class PXSyncItemID
	{
		public readonly string Address;
		public readonly string ItemID;
		public readonly Guid? NoteID;

		public PXSyncItemID(string address, string itemID, Guid? noteID)
		{
			Address = address;
			ItemID = itemID;
			NoteID = noteID;
		}

		public override int GetHashCode()
		{
			return ItemID.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj != null && obj is PXSyncItemID)
			{
				return String.Equals(ItemID, ((PXSyncItemID)obj).ItemID);
			}
			return false;
		}
		public override string ToString()
		{
			return ItemID;
		}

		public virtual bool NeedProcess( )
		{
			return false;
		}
	}
	public sealed class PXSyncItemSet
	{
		private Dictionary<Int32, PXSyncItemID> byItem = new Dictionary<Int32, PXSyncItemID>();
		private Dictionary<Int32, List<PXSyncItemID>> byNote = new Dictionary<Int32, List<PXSyncItemID>>();
		
		public Int32 Count 
		{
			get 
			{
				return byItem.Count; 
			}
		}
		public PXSyncItemID this[string id]
		{
			get
			{
				PXSyncItemID item = null;
				if (!String.IsNullOrEmpty(id) && byItem.TryGetValue(id.GetHashCode(), out item))
					return item;
				return null;
			}
			set
			{
				if (id != null) byItem[id.GetHashCode()] = value;
			}
		}
		public List<PXSyncItemID> this[Guid? id]
		{
			get
			{
				List<PXSyncItemID> item = null;
				if (id != null && byNote.TryGetValue(id.GetHashCode(), out item))
					return item;
				return null;
			}
			set
			{
				if (id != null)
				{
					List<PXSyncItemID> list = null;
					if (!byNote.TryGetValue(id.GetHashCode(), out list))
						byNote[id.GetHashCode()] = list = new List<PXSyncItemID>();
					list.AddRange(value);
				}
			}
		}

		
		public PXSyncItemSet(IEnumerable<PXSyncItemID> collection)
		{
			if (collection != null)
			{
				foreach (PXSyncItemID item in collection)
				{
					Add(item);
				}
			}
		}

		public bool NeedProcessItem(string address, string itemid, Guid? noteid)
		{
			if (itemid == null || noteid == null) return false;

			List<PXSyncItemID> list = null;
			if (!byNote.TryGetValue(noteid.GetHashCode( ), out list)) return false;
			if (list == null || list.Count <= 0) return false;

			if (list.Any(i => i.NeedProcess( ))) return true;
			
			if (list.First().Address == address) return false;
			return true;
		}

		public void Add(PXSyncItemID item)
		{
			if (item == null || item.ItemID == null) return;
			if (byItem.ContainsKey(item.ItemID.GetHashCode())) return;

			byItem[item.ItemID.GetHashCode()] = item;

			List<PXSyncItemID> list = null;
			if (!byNote.TryGetValue(item.NoteID.GetHashCode( ), out list))
				byNote[item.NoteID.GetHashCode()] = list = new List<PXSyncItemID>();
			list.Add(item);
		}
		public void Clear()
		{
			byItem.Clear();
			byNote.Clear();
		}

		public bool Contains(PXSyncItemID item)
		{
			if(Contains(item.ItemID)) return true;
			else if(item.NoteID != null && Contains(item.NoteID.Value)) return true;
			return false;
		}
		public bool Contains(string code)
		{
			return !String.IsNullOrEmpty(code) && byItem.ContainsKey(code.GetHashCode());
		}
		public bool Contains(Guid? code)
		{
			return code != null && byNote.ContainsKey(code.GetHashCode());
		}

		public HashSet<string> ToHashSet()
		{
			return new HashSet<string>(byItem.Values.Select(i => i.ItemID));
		}
	}
	#endregion
	#region PXSyncDirectFolder
	public class PXSyncFolderSpecification
	{
		public readonly string Name;
		public readonly bool Categorized;
		public readonly DistinguishedFolderIdNameType Type;
		public readonly PXEmailSyncDirection.Directions Direction;
		public readonly PXSyncMovingCondition[] MoveTo;

		public PXSyncFolderSpecification(string name, DistinguishedFolderIdNameType type)
			: this(name, type, PXEmailSyncDirection.Directions.Full, true)
		{
		}
		public PXSyncFolderSpecification(string name, DistinguishedFolderIdNameType type, PXEmailSyncDirection.Directions direction, bool categorized, params PXSyncMovingCondition[] moveTo)
		{
			Name = name;
			Type = type;
			Categorized = categorized;
			Direction = direction;
			MoveTo = moveTo;
		}
	}
	public class PXSyncDirectFolder : PXExchangeFolderID
	{
		public readonly bool Categorized;
		public readonly PXEmailSyncDirection.Directions Direction;
		public readonly PXSyncMovingCondition[] MoveToFolder;

		public bool IsExport { get { return (Direction & PXEmailSyncDirection.Directions.Export) == PXEmailSyncDirection.Directions.Export; } }
		public bool IsImport { get { return (Direction & PXEmailSyncDirection.Directions.Import) == PXEmailSyncDirection.Directions.Import; } }

		public PXSyncDirectFolder(string mailbox, BaseFolderIdType folder, PXEmailSyncDirection.Directions direction, bool categorized, PXSyncMovingCondition[] moveTo)
			: base(mailbox, folder)
		{
			Direction = direction;
			Categorized = categorized;
			MoveToFolder = moveTo;
		}
	}
	#endregion
	#region PXSyncMailbox
	public class PXSyncMailboxPreset
	{
		public DateTime? Date;
		public string FolderID;

		public PXSyncMailboxPreset(DateTime? date, string folder)
		{
			Date = date;
			FolderID = folder;
		}
	}

	public class PXSyncMailbox : PXExchangeItemBase
	{
		public string ExchangeTimeZone;
		public readonly Int32 EmployeeID;
		public readonly Int32? EmailAccountID;
		public readonly bool IsIncomingProcessing;

		public bool Reinitialize;
		public bool IsReset;
		public PXSyncMailboxPreset ExportPreset;
		public PXSyncMailboxPreset ImportPreset;

		public List<PXSyncDirectFolder> Folders = new List<PXSyncDirectFolder>();

		public PXSyncMailbox(string mailbox, Int32 employee, Int32? emailAccountID, PXSyncMailboxPreset exportPreset, PXSyncMailboxPreset importPreset, bool isIncomingProcessing)
			: base(mailbox)
		{
			EmployeeID = employee;
			EmailAccountID = emailAccountID;

			ExportPreset = exportPreset;
			ImportPreset = importPreset;
			IsIncomingProcessing = isIncomingProcessing;
		}
	}
	#endregion
	#region PXSyncTag
	public class PXSyncTag
	{
		public bool SkipReqired;
		public bool SendRequired;
		public bool SendSeparateRequired;
		public bool CancelRequired;
	}
	public class PXSyncTag<T> : PXSyncTag
	{
		public T Row;
		public EMailSyncReference Ref;
		public PXSyncMailbox Mailbox;

		public PXSyncTag(T row, PXSyncMailbox mailbox, EMailSyncReference reference, PXSyncTag tag = null)
		{
			Row = row;
			Ref = reference;
			Mailbox = mailbox;
			if (tag != null)
			{
				SkipReqired = tag.SkipReqired;
				CancelRequired = tag.CancelRequired;
				SendRequired = tag.SendRequired;
				SendSeparateRequired = tag.SendSeparateRequired;
			}
		}
	}
	#endregion
	#region PXExchangeItemBucket
	[Flags]
	public enum PXSyncItemStatus
	{
		Inserted = 1,
		Updated = 2,
		Unsynced = 3,
		Deleted = 4,
		None = 7
	}
	public class PXSyncItemBucket<T1>
		where T1 : IBqlTable
	{
		public readonly string ID;
		public readonly PXSyncMailbox Mailbox;
		public readonly PXSyncItemStatus Status;
		public readonly EMailSyncReference Reference;

		public UploadFileWithData[] Attachments;

		public readonly T1 Item1;

		public PXSyncItemBucket(PXSyncMailbox mailbox, PXSyncItemStatus status, EMailSyncReference reference, T1 item1)
		{
			ID = Guid.NewGuid().ToString();

			Item1 = item1;
			Mailbox = mailbox;
			Status = status;
			Reference = reference;
		}
	}
	public class PXSyncItemBucket<T1, T2> : PXSyncItemBucket<T1>
		where T1 : IBqlTable
		where T2 : IBqlTable
	{
		public readonly T2 Item2;

		public PXSyncItemBucket(PXSyncMailbox mailbox, PXSyncItemStatus status, EMailSyncReference reference, T1 item1, T2 item2)
			: base(mailbox, status, reference, item1)
		{
			Item2 = item2;
		}
	}
	public class PXSyncItemBucket<T1, T2, T3> : PXSyncItemBucket<T1, T2>
		where T1 : IBqlTable
		where T2 : IBqlTable
		where T3 : IBqlTable
	{
		public readonly T3 Item3;

		public PXSyncItemBucket(PXSyncMailbox mailbox, PXSyncItemStatus status, EMailSyncReference reference, T1 item1, T2 item2, T3 item3)
			: base(mailbox, status, reference, item1, item2)
		{
			Item3 = item3;
		}
	}
	#endregion
	#region PXSyncResult
	public class PXSyncResult : PXSyncItemID
	{
		//Operation Status
		public string DisplayKey;
		public string ActionTitle;
		public string OperationTitle;
		public PXEmailSyncDirection.Directions Direction;
		public PXSyncItemStatus ItemStatus;

		//Error Handlong
		public Exception Error;
		public string Message;
		public string[] Details;
		public bool Success
		{
			get { return Error == null && String.IsNullOrEmpty(Message); }
		}

		//Runtime properties
		public DateTime Date;
		public bool Reprocess;

		public PXSyncResult(string operation, PXEmailSyncDirection.Directions direction, string mailbox, string id, Guid? note, string key)
			: base(mailbox, id, note)
		{
			DisplayKey = key;
			Date = DateTime.UtcNow;
			OperationTitle = operation;
			Direction = direction;
		}
		public PXSyncResult(string operation, PXEmailSyncDirection.Directions direction, string mailbox, string id, Guid? note, string key, string message, Exception error, string[] details)
			: this(operation, direction, mailbox, id, note, key)
		{
			Error = error;
			Message = message;
			Details = details;
		}

		public PXSyncResult(PXSyncResult result, string id = null, Guid? note = null, string key = null, PXSyncItemStatus? status = null)
			: this(result.OperationTitle, result.Direction, result.Address, id ?? result.ItemID, note ?? result.NoteID, key ?? result.DisplayKey)
		{
			ItemStatus = result.ItemStatus;
			DisplayKey = result.DisplayKey;
			ActionTitle = result.ActionTitle;
			OperationTitle = result.OperationTitle;
			Direction = result.Direction;

			Error = result.Error;
			Message = result.Message;
			Details = result.Details;

			Date = result.Date;
			Reprocess = result.Reprocess;
		}

		public override bool NeedProcess()
		{
			return Reprocess;
		}
	}
	#endregion
	#region PXSyncMovingCondition
	public abstract class PXSyncMovingCondition
	{
		public readonly DistinguishedFolderIdNameType ParentFolder;
		protected readonly BaseFolderIdType FolderId;

		public PXSyncMovingCondition(DistinguishedFolderIdNameType parent)
		{
			ParentFolder = parent;
		}
		protected PXSyncMovingCondition(DistinguishedFolderIdNameType parent, BaseFolderIdType folder)
		{
			ParentFolder = parent;
			FolderId = folder;
		}

		public abstract BaseFolderIdType Evaluate(PXSyncMailbox mailbox, ItemType item);
		public abstract PXSyncMovingCondition InitialiseFolder(BaseFolderIdType folder);
	}
	public class PXSyncMovingMessageCondition :  PXSyncMovingCondition
	{
		protected bool Incomming;
		protected bool Outgoing;
		protected ExchangeEmailsSyncCommand Command;

		public PXSyncMovingMessageCondition(ExchangeEmailsSyncCommand command, DistinguishedFolderIdNameType parent, bool incomming, bool outgoing)
			: base (parent)
		{
			Incomming = incomming;
			Outgoing = outgoing;
			Command = command;
		}
		protected PXSyncMovingMessageCondition(ExchangeEmailsSyncCommand command, DistinguishedFolderIdNameType parent, BaseFolderIdType folder, bool incomming, bool outgoing)
			: base(parent, folder)
		{
			Incomming = incomming;
			Outgoing = outgoing;
			Command = command;
		}

		public override BaseFolderIdType Evaluate(PXSyncMailbox mailbox, ItemType item)
		{
			bool? income = Command.EvaluateIncomming(mailbox.Address, item);
			if (income == null) return null;

			if (Incomming && (income ?? false)) return FolderId;
			if (Outgoing && !(income ?? false)) return FolderId;

			return null;
		}
		public override PXSyncMovingCondition InitialiseFolder(BaseFolderIdType folder)
		{
			return new PXSyncMovingMessageCondition(Command, ParentFolder, folder, Incomming, Outgoing);
		}
	}
	#endregion
	#region PXSyncCache
	public class PXSyncCache
	{
		private Object _locker = new Object();

		private Dictionary<string, Guid> _UsersCache;
		public Dictionary<string, Guid> UsersCache
		{
			get
			{
				if (_UsersCache == null)
				{
					lock (_locker)
					{
						_UsersCache = new Dictionary<string, Guid>(StringComparer.InvariantCultureIgnoreCase);
						foreach (PXDataRecord rec in PXDatabase.SelectMulti<Users>(new PXDataField<Users.email>(), new PXDataField<Users.pKID>()))
						{
							if (String.IsNullOrEmpty(rec.GetString(0))) continue;
							_UsersCache[rec.GetString(0)] = rec.GetGuid(1).Value;
						}
					}
				}
				return _UsersCache;
			}
		}
		private Dictionary<Int32, Guid> _EmployeeCache;
		public Dictionary<Int32, Guid> EmployeeCache
		{
			get
			{
				if (_EmployeeCache == null)
				{
					lock (_locker)
					{
						_EmployeeCache = new Dictionary<Int32, Guid>();
						foreach (PXDataRecord rec in PXDatabase.SelectMulti<EPEmployee>(new PXDataField<EPEmployee.bAccountID>(), new PXDataField<EPEmployee.userID>()))
						{
							if (rec.GetInt32(0) == null || rec.GetGuid(1) == null) continue;
							_EmployeeCache[rec.GetInt32(0).Value] = rec.GetGuid(1).Value;
						}
					}
				}
				return _EmployeeCache;
			}
		}
		private Dictionary<string, Tuple<string, List<Tuple<string, string>>>> _CountryCache;
		public Dictionary<string, Tuple<string, List<Tuple<string, string>>>> CountryCache
		{
			get
			{
				if (_CountryCache == null)
				{
					lock (_locker)
					{
						_CountryCache = new Dictionary<string, Tuple<string, List<Tuple<string, string>>>>(StringComparer.InvariantCultureIgnoreCase);
						foreach (PXDataRecord rec in PXDatabase.SelectMulti<Country>(new PXDataField<Country.countryID>(), new PXDataField<Country.description>()))
						{
							if (rec.GetString(0) == null) continue;
							_CountryCache[rec.GetString(0)] = Tuple.Create(rec.GetString(1), new List<Tuple<string, string>>());
						}
						foreach (PXDataRecord rec in PXDatabase.SelectMulti<State>(new PXDataField<State.countryID>(), new PXDataField<State.stateID>(), new PXDataField<State.name>()))
						{
							if (rec.GetString(0) == null || rec.GetString(1) == null) continue;

							Tuple<string, List<Tuple<string, string>>> details;
							if (_CountryCache.TryGetValue(rec.GetString(0), out details))
							{
								details.Item2.Add(Tuple.Create(rec.GetString(1), rec.GetString(2)));
							}
						}
					}
				}
				return _CountryCache;
			}
		}

		private Dictionary<string, Guid> _ContactsCache;
		public Guid? ContactsCache(string email)
		{
			if (String.IsNullOrEmpty(email)) return null;

			lock (_locker)
			{
				if (_ContactsCache == null) _ContactsCache = new Dictionary<string, Guid>();

				Guid result;
				if (_ContactsCache.TryGetValue(email, out result)) return result;

				using (PXDataRecord record = PXDatabase.SelectSingle<Contact>(new PXDataField<Contact.noteID>(),
					new PXDataFieldValue<Contact.eMail>(email),
					new PXDataFieldValue<Contact.contactType>(ContactTypesAttribute.Person)))
				{
					if (record != null && record.GetGuid(0) != null)
					{
						_ContactsCache[email] = record.GetGuid(0).Value;
						return record.GetGuid(0);
					}
				}
			}
			return null;
		}

		private Dictionary<Type, Dictionary<string, Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo>>> _FieldsCache;
		public Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> FieldsCache(Type type, string field)
		{
			lock (_locker)
			{
				if (_FieldsCache == null) _FieldsCache = new Dictionary<Type, Dictionary<string, Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo>>>();

				Dictionary<string, Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo>> typeDescr;
				if (!_FieldsCache.TryGetValue(type, out typeDescr))
				{
					_FieldsCache[type] = typeDescr = new Dictionary<string, Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo>>();
					foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
					{
						if (!pi.CanRead || !pi.CanWrite) continue;

						bool specified = pi.Name.EndsWith("Specified") && pi.Name.Length > 9 && pi.PropertyType == typeof(bool);
						bool timezone = pi.Name.EndsWith("TimeZone") && pi.Name.Length > 8 && pi.PropertyType == typeof(TimeZoneDefinitionType);
						bool timezoneid = pi.Name.EndsWith("TimeZoneId") && pi.Name.Length > 8 && pi.PropertyType == typeof(string);
						string fname = pi.Name;
						if (specified) fname = pi.Name.Substring(0, pi.Name.Length - 9);
						if (timezone) fname = pi.Name.Substring(0, pi.Name.Length - 8);
						if (timezoneid) fname = pi.Name.Substring(0, pi.Name.Length - 10);

						Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> fieldDescr;
						if (!typeDescr.TryGetValue(fname, out fieldDescr))
							typeDescr[fname] = fieldDescr = new Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo>(null, null, null, null);

						if (specified) fieldDescr.Item2 = pi;
						else if (timezone) fieldDescr.Item3 = pi;
						else if (timezoneid) fieldDescr.Item4 = pi;
						else fieldDescr.Item1 = pi;
					}
				}

				Gang<PropertyInfo, PropertyInfo, PropertyInfo, PropertyInfo> result;
				typeDescr.TryGetValue(field, out result);
				return result;
			}
		}
	}
	#endregion

	#region PXExchangeConversionHelper
	public static class PXExchangeConversionHelper
	{
		public static ExtendedPropertyType[] GetExtendedProperties(params Tuple<string, MapiPropertyTypeType, Object>[] def)
		{
			List<ExtendedPropertyType> properties = new List<ExtendedPropertyType>();
			for (int i = 0; i < def.Length; i++)
			{
				Tuple<string, MapiPropertyTypeType, Object> item = def[i];
				if (item.Item3 == null) continue;

				properties.Add(new ExtendedPropertyType()
				{
					ExtendedFieldURI = new PathToExtendedFieldType()
					{
						PropertyTag = item.Item1,
						PropertyType = item.Item2,
					},
					Item = item.Item3
				});
			}

			if (properties.Count <= 0) return null;
			return properties.ToArray();
		}

		public static PhysicalAddressDictionaryEntryType GetValueByType(PhysicalAddressDictionaryEntryType[] types, PhysicalAddressKeyType key, bool checkEmpty = false)
		{
			if (types != null)
			{
				foreach (PhysicalAddressDictionaryEntryType item in types)
				{
					if (item.Key == key) return item;
				}
			}
			if (checkEmpty) throw new PXException(PXMessages.LocalizeFormatNoPrefix(Messages.EmailTypeNotFound, key.ToString()));
            return null;
		}

		public static TaskStatusType ParceActivityStatus(string status)
		{
			switch (status)
			{
				case ActivityStatusListAttribute.Approved:
					return TaskStatusType.NotStarted;
				case ActivityStatusListAttribute.Canceled:
					return TaskStatusType.Completed;
				case ActivityStatusListAttribute.Completed:
					return TaskStatusType.Completed;
				case ActivityStatusListAttribute.Draft:
					return TaskStatusType.NotStarted;
				case ActivityStatusListAttribute.InProcess:
					return TaskStatusType.InProgress;
				case ActivityStatusListAttribute.Open:
					return TaskStatusType.NotStarted;
				case ActivityStatusListAttribute.PendingApproval:
					return TaskStatusType.NotStarted;
				case ActivityStatusListAttribute.Rejected:
					return TaskStatusType.Deferred;
				case ActivityStatusListAttribute.Released:
					return TaskStatusType.Completed;

				default:
					return TaskStatusType.NotStarted;
			}
		}
		public static string ParceActivityStatus(TaskStatusType status)
		{
			switch (status)
			{
				case TaskStatusType.NotStarted:
					return ActivityStatusListAttribute.Open;
				case TaskStatusType.Completed:
					return ActivityStatusListAttribute.Completed;
				case TaskStatusType.InProgress:
					return ActivityStatusListAttribute.InProcess;
				case TaskStatusType.Deferred:
					return ActivityStatusListAttribute.Rejected;
				case TaskStatusType.WaitingOnOthers:
					return ActivityStatusListAttribute.InProcess;
				default:
					return ActivityStatusListAttribute.Draft;
			}
		}
		public static ImportanceChoicesType ParceActivityPriority(int? priority)
		{
			if (priority == null) return ImportanceChoicesType.Normal;

			switch (priority)
			{
				case 0:
					return ImportanceChoicesType.Low;
				case 1:
					return ImportanceChoicesType.Normal;
				case 2:
					return ImportanceChoicesType.High;

				default:
					return ImportanceChoicesType.Normal;
			}
		}
		public static int ParceActivityPriority(ImportanceChoicesType priority)
		{
			switch (priority)
			{
				case ImportanceChoicesType.Low:
					return 0;
				case ImportanceChoicesType.Normal:
					return 1;
				case ImportanceChoicesType.High:
					return 2;
				default:
					return 1;
			}
		}
		public static ResponseTypeType ParceAttendeeStatus(int? status)
		{
			if (status == null) return ResponseTypeType.Unknown;

			switch (status)
			{
				case PXInvitationStatusAttribute.NOTINVITED:
					return ResponseTypeType.Unknown;
				case PXInvitationStatusAttribute.INVITED:
					return ResponseTypeType.NoResponseReceived;
				case PXInvitationStatusAttribute.ACCEPTED:
					return ResponseTypeType.Accept;
				case PXInvitationStatusAttribute.REJECTED:
					return ResponseTypeType.Decline;
				case PXInvitationStatusAttribute.RESCHEDULED:
					return ResponseTypeType.Accept;
				case PXInvitationStatusAttribute.CANCELED:
					return ResponseTypeType.Decline;

				default:
					return ResponseTypeType.Unknown;
			}
		}
		public static int ParceAttendeeStatus(ResponseTypeType status)
		{
			switch (status)
			{
				case ResponseTypeType.Accept:
					return PXInvitationStatusAttribute.ACCEPTED;
				case ResponseTypeType.Decline:
					return PXInvitationStatusAttribute.REJECTED;
				case ResponseTypeType.NoResponseReceived:
					return PXInvitationStatusAttribute.INVITED;
				case ResponseTypeType.Organizer:
					return PXInvitationStatusAttribute.ACCEPTED;
				case ResponseTypeType.Tentative:
					return PXInvitationStatusAttribute.ACCEPTED;
				case ResponseTypeType.Unknown:
					return PXInvitationStatusAttribute.NOTINVITED;

				default:
					return PXInvitationStatusAttribute.NOTINVITED;
			}
		}
		public static LegacyFreeBusyType ParceShowAs(int? status)
		{
			if (status == null) return LegacyFreeBusyType.NoData;

			switch (status)
			{
				case 2:
					return LegacyFreeBusyType.Busy;
				case 1:
					return LegacyFreeBusyType.Free;
				case 3:
					return LegacyFreeBusyType.OOF;
				default:
					return LegacyFreeBusyType.NoData;
			}
		}
		public static int ParceShowAs(LegacyFreeBusyType status)
		{
			switch (status)
			{
				case LegacyFreeBusyType.Busy:
					return 2;
				case LegacyFreeBusyType.Free:
					return 1;
				case LegacyFreeBusyType.OOF:
					return 3;

				default:
					return 1;
			}
		}

		public static Object ParceAcGender(string status)
		{
			if (status == null) return "0";

			switch (status)
			{
				case GendersAttribute.Male:
					return "2";
				case GendersAttribute.Female:
					return "1";
				default:
					return "0";
			}
		}
		public static string ParceExGender(Object status)
		{
			if (status == null) return null; 

			switch (status.ToString())
			{
				case "2":
					return GendersAttribute.Male;
				case "1":
					return GendersAttribute.Female;
				case "0":
					return null;

				default:
					return null;
			}
		}

		public static PhoneNumberDictionaryEntryType ParcePhone(string type, string value)
		{
			PhoneNumberKeyType key = PhoneNumberKeyType.CompanyMainPhone;
			switch (type)
			{
				case PhoneTypesAttribute.Business1:
					key = PhoneNumberKeyType.BusinessPhone;
					break;
				case PhoneTypesAttribute.Business2:
					key = PhoneNumberKeyType.BusinessPhone2;
					break;
				case PhoneTypesAttribute.Business3:
					key = PhoneNumberKeyType.OtherTelephone;
					break;
				case PhoneTypesAttribute.BusinessAssistant1:
					key = PhoneNumberKeyType.AssistantPhone;
					break;
				case PhoneTypesAttribute.BusinessFax:
					key = PhoneNumberKeyType.BusinessFax;
					break;
				case PhoneTypesAttribute.HomeFax:
					key = PhoneNumberKeyType.HomeFax;
					break;
				case PhoneTypesAttribute.Cell:
					key = PhoneNumberKeyType.MobilePhone;
					break;
				case PhoneTypesAttribute.Home:
					key = PhoneNumberKeyType.HomePhone;
					break;
			}

			PhoneNumberDictionaryEntryType phone = new PhoneNumberDictionaryEntryType();
			phone.Key = key;
			phone.Value = value;
			return phone;
		}
		public static bool ParcePhone(PhoneNumberDictionaryEntryType phone, out string type, out string value)
		{
			value = phone.Value;
			type = PhoneTypesAttribute.Cell;
			switch (phone.Key)
			{
				case PhoneNumberKeyType.BusinessPhone:
				case PhoneNumberKeyType.PrimaryPhone:
					type = PhoneTypesAttribute.Business1;
					break;
				case PhoneNumberKeyType.BusinessPhone2:
					type = PhoneTypesAttribute.Business2;
					break;
				case PhoneNumberKeyType.OtherTelephone:
					type = PhoneTypesAttribute.Business3;
					break;
				case PhoneNumberKeyType.AssistantPhone:
					type = PhoneTypesAttribute.BusinessAssistant1;
					break;
				case PhoneNumberKeyType.HomeFax:
					type = PhoneTypesAttribute.HomeFax;
					break;
				case PhoneNumberKeyType.BusinessFax:
					type = PhoneTypesAttribute.BusinessFax;
					break;
				case PhoneNumberKeyType.MobilePhone:
					type = PhoneTypesAttribute.Cell;
					break;
				case PhoneNumberKeyType.HomePhone:
					type = PhoneTypesAttribute.Home;
					break;
				case PhoneNumberKeyType.OtherFax:
				case PhoneNumberKeyType.HomePhone2:
				case PhoneNumberKeyType.CarPhone:
				case PhoneNumberKeyType.CompanyMainPhone:
				case PhoneNumberKeyType.Isdn:
				case PhoneNumberKeyType.Pager:
				case PhoneNumberKeyType.RadioPhone:
				case PhoneNumberKeyType.Telex:
				case PhoneNumberKeyType.Callback:
					return false;
			}

			return true;
		}
		public static IEnumerable<PhoneNumberDictionaryEntryType> PurgePhones(params PhoneNumberDictionaryEntryType[] existingTypes)
		{
			foreach (PhoneNumberKeyType type in new PhoneNumberKeyType[] { PhoneNumberKeyType.BusinessPhone, PhoneNumberKeyType.BusinessPhone2, PhoneNumberKeyType.OtherTelephone,
				PhoneNumberKeyType.AssistantPhone, PhoneNumberKeyType.BusinessFax, PhoneNumberKeyType.MobilePhone, PhoneNumberKeyType.HomePhone, PhoneNumberKeyType.HomeFax })
			{
				if (!existingTypes.Any(t => t.Key == type && !String.IsNullOrEmpty(t.Value)))
				{
					PhoneNumberDictionaryEntryType phone = new PhoneNumberDictionaryEntryType();
					phone.Key = type;
					phone.Value = null;
					yield return phone;
				}
			}
		}

		public static EmailAddressType ParceAddress(string address)
		{
			if (String.IsNullOrEmpty(address)) return null;
			foreach (MailAddress item in EmailParser.ParseAddresses(address))
			{
				return new EmailAddressType() { EmailAddress = item.Address, Name = item.DisplayName };
			}
			return null;
		}
		public static EmailAddressType[] ParceAddresses(string address)
		{
			if (String.IsNullOrEmpty(address)) return null;

			List<EmailAddressType> addresses = new List<EmailAddressType>();
			foreach (MailAddress item in EmailParser.ParseAddresses(address))
			{
				addresses.Add(new EmailAddressType() { EmailAddress = item.Address, Name = item.DisplayName });
			}
			return addresses.ToArray();
		}
		public static string ParceAddresses(EmailAddressType[] addresses)
		{
			if (addresses == null || addresses.Length <= 0) return null;
			return PXDBEmailAttribute.ToString(addresses.Select(_ => new MailAddress(_.EmailAddress, _.Name)));
		}
	}
	#endregion
}
