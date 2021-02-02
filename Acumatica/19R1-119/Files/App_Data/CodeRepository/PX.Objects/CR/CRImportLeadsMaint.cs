using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	[Obsolete("Will be removed in 7.0 version")]
	public class CRImportLeadsMaint : PXGraph<CRImportLeadsMaint>
	{
		#region Processor

		private class Processor
		{
			private readonly Guid? _ownerId;
			private readonly CRImportLeadsMaint _sourceGraph;

			private LeadMaint _graph;

			private bool _preparied;

			public Processor(CRImportLeadsMaint sourceGraph)
			{
				_ownerId = (Guid?)EP.EmployeeMaint.GetCurrentEmployeeID(sourceGraph);
				_sourceGraph = sourceGraph;
			}

			public void ImportRecords(CRBatchLead item)
			{
				Prepare();

				var currentFilter = (CRImportLeadsFilter)_sourceGraph.Filter.Current;
				var graph = Graph;
				try
				{
					var row = (Contact)graph.Lead.Cache.CreateCopy(item);
					row.ContactID = null;
					row.ContactType = null;
					row.MajorStatus = currentFilter.MajorStatus;
					row.Status = currentFilter.Status;
					row.Resolution = currentFilter.Resolution;
					row.Source = currentFilter.Source;
					row.ClassID = currentFilter.ClassID;
					row.OwnerID = _ownerId;
					row = (Contact)graph.Lead.Cache.Insert(row);
					graph.Lead.Cache.Current = row;

					var newAddress = (Address)graph.AddressCurrent.View.SelectSingle();
					newAddress.AddressLine1 = item.AddressLine1;
					newAddress.AddressLine2 = item.AddressLine2;
					newAddress.AddressLine3 = item.AddressLine3;
					newAddress.AddressType = item.AddressType;
					newAddress.City = item.City;
					newAddress.CountryID = item.CountryID;
					newAddress.PostalCode = item.PostalCode;
					newAddress.State = item.State;
					graph.AddressCurrent.Cache.Update(newAddress);

					graph.Save.Press();
				}
				finally
				{
					graph.Clear();
				}
			}

			private void Prepare()
			{
				if (!_preparied)
				{
					if (_sourceGraph.Filter.Current == null)
						throw new Exception(string.Format("Current of cache '{0}' is not set.",
							_sourceGraph.Filter.Cache.GetItemType().GetLongName()));

					//PXLongOperation.SetCustomInfo(new Func<PXGraph>(() => _sourceGraph));

					_preparied = true;
				}
			}

			private LeadMaint Graph
			{
				get { return _graph ?? (_graph = PXGraph.CreateInstance<LeadMaint>()); }
			}
		}

		#endregion

		#region CRBatchLead
		[Serializable]
		public partial class CRBatchLead : Contact
		{
			#region DefAddressID

			public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }

			[PXDBInt]
			[PXDBLiteDefault(typeof(Address.addressID))]
			[PXUIField(Visible = false)]
			public override int? DefAddressID
			{
				get
				{
					return base.DefAddressID;
				}
				set
				{
					base.DefAddressID = value;
				}
			}

			#endregion

			#region AddressType
			public abstract class addressType : PX.Data.BQL.BqlString.Field<addressType> { }
			[PXString(2, IsFixed = true)]
			[PXDefault(CR.Address.AddressTypes.BusinessAddress)]
			[Address.AddressTypes.List]
			[PXUIField(DisplayName = "Address Type", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String AddressType { get; set; }
			#endregion

			#region AddressLine1
			public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
			[PXString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String AddressLine1 { get; set; }
			#endregion

			#region AddressLine2
			public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
			[PXString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Address Line 2")]
			public virtual String AddressLine2 { get; set; }
			#endregion

			#region AddressLine3
			public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
			[PXString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Address Line 3")]
			public virtual String AddressLine3 { get; set; }
			#endregion

			#region City
			public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

			[PXString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String City { get; set; }
			#endregion

			#region CountryID
			public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

			[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
			[PXDBString(100)]
			[PXUIField(DisplayName = "Country")]
			[Country]
			public virtual String CountryID { get; set; }
			#endregion

			#region State
			public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

			[PXString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "State")]
			[State(typeof(CRBatchLead.countryID))]
			public virtual String State { get; set; }

			#endregion

			#region PostalCode
			public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
			[PXString(20)]
			[PXUIField(DisplayName = "Postal Code")]
			[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(CRBatchLead.countryID))]
			public virtual String PostalCode { get; set; }
			#endregion
		}
		#endregion

		#region CRImportLeadsFilter
		[System.Serializable()]
		[PXHidden]
		public partial class CRImportLeadsFilter : PX.Data.IBqlTable
		{
			#region Lead MajorStatus
			public abstract class majorStatus : PX.Data.BQL.BqlInt.Field<majorStatus> { }

			[PXDBInt]
			[LeadMajorStatuses]
			[PXDefault(LeadMajorStatusesAttribute._HOLD, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(Visible = false)]
			public virtual Int32? MajorStatus { get; set; }
			#endregion

			#region Lead Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

			[PXDBString(1, IsFixed = true)]
			[PXDefault(LeadStatusesAttribute.New, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(Visible = false)]
			[LeadStatuses]
			public virtual String Status { get; set; }

			#endregion

			#region Lead Resolution

			public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

			[PXString(2, IsFixed = true)]
			[LeadResolutions]
			[PXUIField(Visible = false)]
			public virtual String Resolution { get; set; }

			#endregion

			#region Source
			public abstract class source : PX.Data.BQL.BqlString.Field<source> { }
			protected String _Source;
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Source")]
			[CRMSources(BqlField = typeof(Contact.source))]
			[PXDefault(CRMSourcesAttribute._PURCHASED_LIST, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual String Source
			{
				get
				{
					return this._Source;
				}
				set
				{
					this._Source = value;
				}
			}
			#endregion
			#region ClassID
			public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
			protected String _ClassID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Class ID")]
			[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
			public virtual String ClassID
			{
				get
				{
					return this._ClassID;
				}
				set
				{
					this._ClassID = value;
				}
			}
			#endregion
		}
		#endregion

		#region Fields

		[PXViewName(Messages.ImportSettings)]
		public PXFilter<CRImportLeadsFilter> 
			Filter;

		[PXImport(typeof(CRImportLeadsFilter))]
		[PXViewName(Messages.Items)]
		public PXFilteredProcessing<CRBatchLead, CRImportLeadsFilter> 
			Items;

		#endregion

		#region Ctors

		public CRImportLeadsMaint()
		{
			var processor = new Processor(this);
			Items.SetProcessDelegate(processor.ImportRecords);
			Actions["Schedule"].SetVisible(false);
			Actions.Move("Process", "Cancel");

			PXUIFieldAttribute.SetEnabled<CRBatchLead.title>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.firstName>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.lastName>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.salutation>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.fullName>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.isActive>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.status>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.webSite>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.eMail>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.phone1>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.phone2>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.phone3>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.fax>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.dateOfBirth>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.addressLine1>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.addressLine2>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.addressLine3>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.city>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.countryID>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.state>(Items.Cache, null);
			PXUIFieldAttribute.SetEnabled<CRBatchLead.postalCode>(Items.Cache, null);

			PXUIFieldAttribute.SetRequired<CRBatchLead.lastName>(Items.Cache, true);

			Items.Cache.AllowInsert = true;
			Items.Cache.AllowUpdate = true;
			Items.Cache.AllowDelete = true;
		}

		#endregion

		#region Actions

		public PXCancel<CRImportLeadsFilter> Cancel;

		#endregion

		#region Data Handlers

		public virtual IEnumerable items()
		{
			foreach (CRBatchLead item in Items.Cache.Inserted)
				yield return item;
			foreach (CRBatchLead item in Items.Cache.Updated)
				yield return item;
		}

		#endregion

		#region Event Handlers

		protected virtual void CRBatchLead_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
	}
}
