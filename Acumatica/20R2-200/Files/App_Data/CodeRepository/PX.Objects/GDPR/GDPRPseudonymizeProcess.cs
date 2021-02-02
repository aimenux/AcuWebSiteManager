using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using PX.Common;
using PX.SM;
using PX.Data.Process;

namespace PX.Objects.GDPR
{
	public class GDPRPseudonymizeProcess : GDPRPersonalDataProcessBase
	{
		#region DACs

		[Serializable]
		[PXHidden]
		public partial class ObfuscateEntity : IBqlTable
		{
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

			[PXBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
			public virtual bool? Selected { get; set; }

			[PXUIField(DisplayName = "Contact ID", Enabled = false)]
			[PXInt(IsKey = true)]
			public virtual Int32? ContactID { get; set; }

			[PXStringList(
				new string[]
				{
					ContactTypesAttribute.Person,
					ContactTypesAttribute.SalesPerson,
					ContactTypesAttribute.BAccountProperty,
					ContactTypesAttribute.Employee,
					ContactTypesAttribute.Lead,
					"OP"
				},
				new string[]
				{
					Objects.CR.Messages.Person,
					Objects.CR.Messages.SalesPerson,
					Objects.CR.Messages.BAccountProperty,
					Objects.CR.Messages.Employee,
					Objects.CR.Messages.Lead,
					"Opportunity"
				})]
			[PXUIField(DisplayName = "Master Entity Type", Enabled = false)]
			[PXString(IsKey = true)]
			public virtual String ContactType { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Business Account")]
			public virtual String AcctCD { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Display Name", Enabled = false)]
			public virtual String DisplayName { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Middle Name", Enabled = false)]
			public virtual String MidName { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Last Name", Enabled = false)]
			public virtual String LastName { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Job Title", Enabled = false)]
			public virtual String Salutation { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Full Name", Enabled = false)]
			public virtual String FullName { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Email", Enabled = false)]
			public virtual String Email { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Web Site", Enabled = false)]
			public virtual String WebSite { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Fax", Enabled = false)]
			public virtual String Fax { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Phone 1", Enabled = false)]
			public virtual String Phone1 { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Phone 2", Enabled = false)]
			public virtual String Phone2 { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Phone 3", Enabled = false)]
			public virtual String Phone3 { get; set; }
			
			[PXBool]
			[PXUIField(DisplayName = "Deleted")]
			public virtual bool? Deleted { get; set; }
		}

		[Serializable]
		[PXHidden]
		public class ObfuscateType : IBqlTable
		{
			#region Search
			public abstract class search : PX.Data.BQL.BqlString.Field<search> { }

			[PXString]
			[PXUIField(DisplayName = "Search")]
			public virtual String Search { get; set; }
			#endregion

			#region MasterEntity
			public abstract class masterEntity : PX.Data.BQL.BqlString.Field<masterEntity> { }

			[PXStringList(
				new string[]
				{
					"CT",
					"OP"
				},
				new string[]
				{
					"Contact",
					"Opportunity"
				})]
			[PXUIField(DisplayName = "Master Entity")]
			[PXDefault("CT")]
			[PXString]
			public virtual String MasterEntity { get; set; }
			#endregion

			#region NoConsent
			public abstract class noConsent : PX.Data.BQL.BqlBool.Field<noConsent> { }

			[PXBool]
			[PXUIField(DisplayName = "No Consent")]
			[PXDefault(false)]
			public virtual bool? NoConsent { get; set; }
			#endregion

			#region ConsentExpired
			public abstract class consentExpired : PX.Data.BQL.BqlBool.Field<consentExpired> { }

			[PXBool]
			[PXUIField(DisplayName = "Consent Expired")]
			[PXDefault(false)]
			public virtual bool? ConsentExpired { get; set; }
			#endregion
		}

		#endregion

		#region ctor

		public GDPRPseudonymizeProcess()
		{
			GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.notPseudonymized);
			SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.Pseudonymized;

			SelectedItems.SetProcessDelegate(delegate(List<ObfuscateEntity> entries)
			{
				var graph = PXGraph.CreateInstance<GDPRPseudonymizeProcess>();

				Process(entries, graph);
			});

			SelectedItems.SetSelected<ObfuscateEntity.selected>();

			SelectedItems.SetProcessCaption(Messages.Pseudonymize);
			SelectedItems.SetProcessAllCaption(Messages.PseudonymizeAll);
		}

		#endregion

		#region Actions

		public PXCancel<ObfuscateType> Cancel;

		public PXAction<ObfuscateType> OpenContact;

		[PXButton]
		[PXUIField(DisplayName = Messages.OpenContact, Visible = false)]
		public virtual IEnumerable openContact(PXAdapter adapter)
		{
			var entity = this.Caches[typeof(ObfuscateEntity)].Current as ObfuscateEntity;

			if (entity == null)
				return adapter.Get();

			if (entity.Deleted == true)
			{
				throw new PXSetPropertyException(Messages.NavigateDeleted);
			}

			var primary = RemapToPrimary(new List<ObfuscateEntity> { entity });

			foreach (IBqlTable table in primary)
			{
				if (table is Contact)
				{
					var graph = PXGraph.CreateInstance<ContactMaint>();
					PXRedirectHelper.TryRedirect(graph, table, PXRedirectHelper.WindowMode.New);
				}
				else if (table is CRContact)
				{
					var opp = new PXSelect<
							CROpportunity,
							Where<CROpportunity.opportunityContactID, Equal<Required<CRContact.contactID>>>>(this)
						.SelectSingle((table as CRContact).ContactID);

					var graph = PXGraph.CreateInstance<OpportunityMaint>();
					PXRedirectHelper.TryRedirect(graph, opp, PXRedirectHelper.WindowMode.New);
				}
			}

			return adapter.Get();
		}

		#endregion
		
		#region Selects

		public PXFilter<ObfuscateType> Filter;

		public PXFilteredProcessing<ObfuscateEntity, ObfuscateType>
			SelectedItems;

		public virtual IEnumerable selectedItems()
		{
			List<ObfuscateEntity> detailsList = new List<ObfuscateEntity>();

			using (new PXReadDeletedScope())
			{
				List<string> fields = new List<string>()
				{
					nameof(BAccount) + "__" + nameof(BAccount.AcctCD),
					nameof(Contact.DisplayName),
					nameof(Contact.FirstName),
					nameof(Contact.MidName),
					nameof(Contact.LastName),
					nameof(Contact.Salutation),
					nameof(Contact.FullName),
					nameof(Contact.EMail),
					nameof(Contact.WebSite),
					nameof(Contact.Fax),
					nameof(Contact.Phone1),
					nameof(Contact.Phone2),
					nameof(Contact.Phone3),
				};

				foreach (PXResult<Contact, BAccount> contactBA in SelectContacts(fields))
				{
					var contact = (Contact)contactBA;
					var ba = (BAccount)contactBA;

					detailsList.Add(new ObfuscateEntity()
					{
						ContactID = contact.ContactID,
						ContactType = contact.ContactType,
						AcctCD = ba.AcctCD,

						DisplayName = contact.DisplayName,
						MidName = contact.MidName,
						LastName = contact.LastName,
						Salutation = contact.Salutation,
						FullName = contact.FullName,
						Email = contact.EMail,
						WebSite = contact.WebSite,
						Fax = contact.Fax,
						Phone1 = contact.Phone1,
						Phone2 = contact.Phone2,
						Phone3 = contact.Phone3,

						Deleted = contact.DeletedDatabaseRecord
					});
				}

				foreach (PXResult<CRContact, BAccount> contactBA in SelectOpportunities(fields))
				{
					var contact = (CRContact)contactBA;
					var ba = (BAccount)contactBA;

					detailsList.Add(new ObfuscateEntity()
					{
						ContactID = contact.ContactID,
						ContactType = "OP",
						AcctCD = ba.AcctCD,

						DisplayName = contact.DisplayName,
						MidName = contact.MidName,
						LastName = contact.LastName,
						Salutation = contact.Salutation,
						FullName = contact.FullName,
						Email = contact.Email,
						WebSite = contact.WebSite,
						Fax = contact.Fax,
						Phone1 = contact.Phone1,
						Phone2 = contact.Phone2,
						Phone3 = contact.Phone3,
					});
				}
				
				foreach (ObfuscateEntity detail in detailsList)
				{
					var located = SelectedItems.Cache.Locate(detail);
					
					if (located != null)
					{
						yield return located;
					}
					else
					{
						SelectedItems.Cache.SetStatus(detail, PXEntryStatus.Held);
						yield return detail;
					}
				}
			}

			SelectedItems.Cache.IsDirty = false;
		}

		protected virtual IEnumerable SelectContacts(List<string> fields)
		{
			ObfuscateType filter = Filter.Current;

			if (filter.MasterEntity == "OP")
				return new List<PXResult<Contact, BAccount>>();

			var view = new PXView(this, true, BqlCommand.CreateInstance(
				typeof(Select2<,,>),
				typeof(Contact),
				typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>),
				typeof(Where<,,>),
				typeof(ContactExt.pseudonymizationStatus), typeof(Equal<>), GetPseudonymizationStatus,
				typeof(Or<,>), typeof(ContactExt.pseudonymizationStatus), typeof(IsNull)));

			List<PXFilterRow> filters = new List<PXFilterRow>();

			foreach (PXFilterRow filterRow in PXView.Filters)
			{
				filters.Add(filterRow);
			}

			if (!String.IsNullOrWhiteSpace(filter.Search))
			{
				for (int i = 0; i < fields.Count; i++)
				{
					filters.Add(new PXFilterRow()
					{
						OrOperator = i == fields.Count - 1 ? false : true,
						OpenBrackets = i == 0 ? 1 : 0,
						DataField = fields[i],
						Condition = PXCondition.LIKE,
						Value = filter.Search,
						CloseBrackets = i == fields.Count - 1 ? 1 : 0
					});
				}
			}

			if (filter.ConsentExpired == true)
				view.WhereAnd<Where<Contact.consentExpirationDate, LessEqual<Now>>>();

			if (filter.NoConsent == true)
				view.WhereAnd<Where<Contact.consentDate, IsNull>>();

			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, null, null, filters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual IEnumerable SelectOpportunities(List<string> fields)
		{
			ObfuscateType filter = Filter.Current;
			
			if (filter.MasterEntity != "OP")
				return new List<PXResult<CRContact, BAccount>>();

			var view = new PXView(this, true, BqlCommand.CreateInstance(
				typeof(Select2<,,>),
				typeof(CRContact),
				typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRContact.bAccountID>>>),
				typeof(Where<,,>),
				typeof(CRContactExt.pseudonymizationStatus), typeof(Equal<>), GetPseudonymizationStatus,
				typeof(Or<,>), typeof(CRContactExt.pseudonymizationStatus), typeof(IsNull)));

			List<PXFilterRow> filters = new List<PXFilterRow>();

			foreach (PXFilterRow filterRow in PXView.Filters)
			{
				filters.Add(filterRow);
			}

			if (!String.IsNullOrWhiteSpace(filter.Search))
			{
				for (int i = 0; i < fields.Count; i++)
				{
					filters.Add(new PXFilterRow()
					{
						OrOperator = true,
						OpenBrackets = i == 0 ? 1 : 0,
						DataField = fields[i],
						Condition = PXCondition.LIKE,
						Value = filter.Search,
						CloseBrackets = i == fields.Count - 1 ? 1 : 0
					});
				}
			}
			
			if (filter.ConsentExpired == true)
				view.WhereAnd<Where<CRContact.consentExpirationDate, LessEqual<Now>>>();

			if (filter.NoConsent == true)
				view.WhereAnd<Where<CRContact.consentDate, IsNull>>();

			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, null, null, filters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		#endregion

		#region Public Methods

		public static void Process(IEnumerable<ObfuscateEntity> entities, GDPRPersonalDataProcessBase graph)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				using (new PXReadDeletedScope())
				{
					graph.ProcessImpl(RemapToPrimary(entities), true, null);

					ts.Complete();
				}
			}
		}

		#endregion

		#region Implementation

		protected static IEnumerable<IBqlTable> RemapToPrimary(IEnumerable<ObfuscateEntity> entities)
		{
			foreach (ObfuscateEntity entity in entities)
			{
				switch (entity.ContactType)
				{
					case ContactTypesAttribute.Person:
					case ContactTypesAttribute.SalesPerson:
					case ContactTypesAttribute.BAccountProperty:
					case ContactTypesAttribute.Employee:
					case ContactTypesAttribute.Lead:
						yield return new PXSelect<
								Contact,
							Where<
								Contact.contactID, Equal<Required<Contact.contactID>>>>(new PXGraph()).SelectSingle(entity.ContactID);
						break;
					case "OP":
						yield return new PXSelect<
								CRContact,
							Where<
								CRContact.contactID, Equal<Required<CRContact.contactID>>>>(new PXGraph()).SelectSingle(entity.ContactID);
						break;
				}
			}
		}

		protected override void TopLevelProcessor(string combinedKey, Guid? topParentNoteID, string info)
		{
			StoreRestorableSearchIndex(combinedKey, info);

			DeleteSearchIndex(topParentNoteID);
		}

		protected override void ChildLevelProcessor(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			StoreChildsValues(processingGraph, childTable, fields, childs, topParentNoteID);

			PseudonymizeChilds(processingGraph, childTable, fields, childs);

			WipeAudit(processingGraph, childTable, fields, childs);
		}

		protected bool StoreRestorableSearchIndex(string combinedKey, string info)
		{
			return PXDatabase.Insert<SMPersonalDataIndex>(
				new PXDataFieldAssign<SMPersonalDataIndex.combinedKey>(combinedKey),
				new PXDataFieldAssign<SMPersonalDataIndex.indexID>(Guid.NewGuid()),
				new PXDataFieldAssign<SMPersonalDataIndex.content>(info.ToString()),
				new PXDataFieldAssign<SMPersonalDataIndex.createdDateTime>(PXTimeZoneInfo.UtcNow)
			);
		}

		protected bool DeleteSearchIndex(Guid? topParentNoteID)
		{
			return PXDatabase.Delete<SearchIndex>(
				new PXDataFieldRestrict<SearchIndex.noteID>(topParentNoteID));
		}

		private void StoreChildsValues(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			foreach (object child in childs)
			{
				var childNoteID = processingGraph.Caches[childTable].GetValue(child, nameof(INotable.NoteID)) as Guid?;

				// TODO: merge into single INSERT
				foreach (PXPersonalDataFieldAttribute field in fields)
				{
					var value = processingGraph.Caches[childTable].GetValue(child, field.FieldName);
					if (value == null)
						continue;

					if (field.DefaultValue != null && (!value.GetType().IsAssignableFrom(field.DefaultValue.GetType()) || value.Equals(field.DefaultValue)))
						continue;

					List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();

					assigns.Add(new PXDataFieldAssign<SMPersonalData.table>(childTable.FullName));
					assigns.Add(new PXDataFieldAssign<SMPersonalData.field>(field.FieldName));
					assigns.Add(new PXDataFieldAssign<SMPersonalData.entityID>(childNoteID));
					assigns.Add(new PXDataFieldAssign<SMPersonalData.topParentNoteID>(topParentNoteID));
					assigns.Add(new PXDataFieldAssign<SMPersonalData.value>(value));
					assigns.Add(new PXDataFieldAssign<SMPersonalData.createdDateTime>(PXTimeZoneInfo.UtcNow));

					PXDatabase.Insert<SMPersonalData>(assigns.ToArray());
				}

				DeleteSearchIndex(childNoteID);
			}
		}

		protected void PseudonymizeChilds(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs)
		{
			foreach (object child in childs)
			{
				List<PXDataFieldParam> assigns = new List<PXDataFieldParam>();

				foreach (var field in fields)
				{
					object obfuscatedValue = GetObfuscateValue(childTable, field);

					assigns.Add(new PXDataFieldAssign(field.FieldName, obfuscatedValue));
				}

				assigns.Add(new PXDataFieldAssign(nameof(IPseudonymizable.PseudonymizationStatus), SetPseudonymizationStatus));

				List<PXDataFieldParam> restricts = new List<PXDataFieldParam>();

				foreach (string key in processingGraph.Caches[childTable].Keys)
				{
					restricts.Add(new PXDataFieldRestrict(key, processingGraph.Caches[childTable].GetValue(child, key)));
				}

				PXDatabase.Update(
					childTable,
					InterruptPseudonimyzationHandler(processingGraph.Caches[childTable], restricts)
						.Union(assigns)
						.Distinct(_ => _.Column.Name.ToLower())
						.Union(restricts)
						.ToArray()
				);

				DeleteSearchIndex(processingGraph.Caches[childTable].GetValue(child, nameof(INotable.NoteID)) as Guid?);
			}
		}

		private List<PXDataFieldParam> InterruptPseudonimyzationHandler(PXCache cache, List<PXDataFieldParam> restricts)
		{
			var returnAssigns = new List<PXDataFieldParam>();

			var extensions = cache
				.GetExtensionTypes()
				.Where(_ => typeof(IPostPseudonymizable).IsAssignableFrom(_));

			if (extensions == null || extensions.Count() == 0)
				return returnAssigns;

			foreach (var extension in extensions)
			{
				var handler = extension.GetMethod(nameof(IPostPseudonymizable.InterruptPseudonimyzationHandler));

				if (handler != null)
				{
					returnAssigns.AddRange(
						handler?.Invoke(Activator.CreateInstance(extension), new object[] { restricts }) as List<PXDataFieldParam>
						?? new List<PXDataFieldParam>()
					);
				}
			}
			
			return returnAssigns;
		}

		private object GetObfuscateValue(Type table, PXPersonalDataFieldAttribute field)
		{
			var tableStruct = PXDatabase.GetTableStructure(table.Name);

			var column = tableStruct.getColumnByName(field.FieldName);

			if (column.SystemType == typeof(string))
			{
				string returnVal = field.IsKey ? Guid.NewGuid().ToString() : "*****";

				return returnVal.Substring(0, Math.Min(column.Size, returnVal.Length));
			}

			return column.GetDefaultValue();
		}

		protected void WipeAudit(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs)
		{
			foreach (object child in childs)
			{
				string restrict = null;
				foreach (string key in processingGraph.Caches[childTable].Keys)
				{
					restrict += processingGraph.Caches[childTable].GetValue(child, key).ToString() + PXAuditHelper.SEPARATOR;
				}

				restrict = restrict.TrimEnd(PXAuditHelper.SEPARATOR);

				List<Tuple<long, long>> modifications = new List<Tuple<long, long>>();

				foreach (PXDataRecord record in PXDatabase.SelectMulti<AuditHistory>(
					new PXDataField<AuditHistory.batchID>(),
					new PXDataField<AuditHistory.changeID>(),
					new PXDataField<AuditHistory.modifiedFields>(),
					new PXDataFieldValue<AuditHistory.combinedKey>(restrict))
					.Where(_ => fields.Any(field => _.GetString(2).Contains(field.FieldName))))
				{
					modifications.Add(new Tuple<long, long>(record.GetInt64(0).Value, record.GetInt64(1).Value));
				}

				foreach (var modification in modifications)
				{
					PXDatabase.Delete<AuditHistory>(
						new PXDataFieldRestrict<AuditHistory.batchID>(modification.Item1),
						new PXDataFieldRestrict<AuditHistory.changeID>(modification.Item2)
					);
				}
			}
		}

		#endregion
	}
}
