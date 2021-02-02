using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api.Payroll;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Payroll.Data;
using WSTaxType = PX.Payroll.Data.PRTaxType;

namespace PX.Objects.PR
{
	public class PRTaxMaintenance : PXGraph<PRTaxMaintenance>
	{
		public PRTaxMaintenance()
		{
			Taxes.AllowInsert = false;
			Taxes.AllowDelete = false;
			TaxAttributes.AllowInsert = false;
			TaxAttributes.AllowDelete = false;
			CompanyAttributes.AllowInsert = false;
			CompanyAttributes.AllowDelete = false;

			Employees.SetProcessDelegate(list => AssignEmployeeTaxes(list));
		}

		public override bool IsDirty
		{
			get
			{
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID);
				if (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted)
				{
					foreach (KeyValuePair<Type, PXCache> pair in Caches)
					{
						if (Views.Caches.Contains(pair.Key) && pair.Value.IsDirty)
						{
							return true;
						}
					}
				}
				return base.IsDirty;
			}
		}

		#region Views
		public PXFilter<FakeDac> FakeView;

		public SelectFrom<PRTaxUpdateHistory>.View TaxUpdateHistory;

		public SelectFrom<PRTaxCode>.View Taxes;
		public SelectFrom<PRTaxCode>
			.Where<PRTaxCode.taxID.IsEqual<PRTaxCode.taxID.FromCurrent>>.View CurrentTax;

		public PRAttributeDefinitionSelect<
			PRTaxCodeAttribute,
			SelectFrom<PRTaxCodeAttribute>
				.Where<PRTaxCodeAttribute.taxID.IsEqual<PRTaxCode.taxID.FromCurrent>>
				.OrderBy<PRTaxCodeAttribute.sortOrder.Asc>,
			PRTaxCode,
			Payroll.Data.PRTax,
			Payroll.TaxTypeAttribute> TaxAttributes;

		public PXFilter<PRCompanyTaxAttributeFilter> CompanyAttributeFilter;
		public PREmployeeAttributeDefinitionSelect<
			PRCompanyTaxAttribute,
			SelectFrom<PRCompanyTaxAttribute>
				.LeftJoin<PRTaxCode>.On<PRTaxCode.taxState.IsEqual<PRCompanyTaxAttribute.state>>
				.Where<PRCompanyTaxAttributeFilter.filterStates.FromCurrent.IsNotEqual<True>
					.Or<PRCompanyTaxAttribute.state.IsEqual<LocationConstants.Federal>>
					.Or<PRTaxCode.taxID.IsNotNull>>
				.OrderBy<PRCompanyTaxAttribute.state.Asc, PRCompanyTaxAttribute.sortOrder.Asc>,
			PRCompanyTaxAttributeFilter,
			PRCompanyTaxAttributeFilter.filterStates,
			PRTaxCode,
			PRTaxCode.taxState> CompanyAttributes;
		public SelectFrom<PRCompanyTaxAttribute>
			.LeftJoin<PRTaxCode>.On<PRTaxCode.taxState.IsEqual<PRCompanyTaxAttribute.state>>
			.Where<PRCompanyTaxAttributeFilter.filterStates.FromCurrent.IsNotEqual<True>
				.Or<PRCompanyTaxAttribute.state.IsEqual<LocationConstants.Federal>>
				.Or<PRTaxCode.taxID.IsNotNull>>.View FilteredCompanyAttributes;

		public SelectFrom<Address>
			.LeftJoin<PRLocation>.On<PRLocation.addressID.IsEqual<Address.addressID>>
			.LeftJoin<PREmployee>.On<PREmployee.defAddressID.IsEqual<Address.addressID>>
			.Where<PRLocation.locationID.IsNotNull
				.Or<PREmployee.bAccountID.IsNotNull>>.View Addresses;

		public InvokablePXProcessing<PREmployee> Employees;
		#endregion Views

		#region Data view delegates
		public virtual IEnumerable taxes()
		{
			UpdateTaxesCustomInfo customInfo = PXLongOperation.GetCustomInfo(this.UID) as UpdateTaxesCustomInfo;
			if (customInfo?.ClearTaxCache == true)
			{
				Taxes.Cache.Clear();
				customInfo.ClearTaxCache = false;
			}

			bool taxesCached = Taxes.Cache.Cached.Any_();
			List<object> taxList = new PXView(this, false, Taxes.View.BqlSelect).SelectMulti();
			if (!taxesCached)
			{
				ValidateTaxAttributes(taxList.Select(x => (PRTaxCode)x).ToList());
			}

			if (customInfo?.NewTaxes.Any() == true)
			{
				customInfo.NewTaxes.ForEach(x =>
				{
					Taxes.Insert(x);
					taxList.Add(x);
				});
				ValidateTaxAttributes(Taxes.Cache.Inserted.Cast<PRTaxCode>().ToList());
				customInfo.NewTaxes.Clear();
			}
			return taxList;
		}
		#endregion

		#region Actions
		public PXSave<FakeDac> Save;
		public PXCancel<FakeDac> Cancel;

		public PXAction<FakeDac> UpdateTaxes;
		[PXUIField(DisplayName = "Update Taxes", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable updateTaxes(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, delegate ()
			{
				PRTaxUpdateHistory updateHistory = TaxUpdateHistory.SelectSingle();
				bool insertNew = false;
				if (updateHistory == null)
				{
					updateHistory = new PRTaxUpdateHistory();
					insertNew = true;
				}
				updateHistory.LastCheckTime = DateTime.UtcNow;
				updateHistory.ServerTaxDefinitionTimestamp = new PayrollUpdateClient().GetTaxDefinitionTimestamp();

				PXPayrollAssemblyScope.UpdateTaxDefinition();
				List<PRTaxCode> newTaxes = CreateTaxesForAllLocations();

				updateHistory.LastUpdateTime = DateTime.UtcNow;
				TaxUpdateHistory.Update(updateHistory);
				TaxUpdateHistory.Cache.Persist(insertNew ? PXDBOperation.Insert : PXDBOperation.Update);
				TaxUpdateHistory.Cache.Clear();

				PXLongOperation.SetCustomInfo(new UpdateTaxesCustomInfo(newTaxes));
			});

			return adapter.Get();
		}

		public PXAction<FakeDac> AssignTaxesToEmployees;
		[PXUIField(DisplayName = "Assign Taxes to Employees", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXProcessButton]
		public virtual IEnumerable assignTaxesToEmployees(PXAdapter adapter)
		{
			PXLongOperation.ClearStatus(this.UID);
			return Employees.Invoke(adapter);
		}

		public PXAction<PRTaxCode> ViewTaxDetails;
		[PXUIField(DisplayName = "Tax Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewTaxDetails(PXAdapter adapter)
		{
			CurrentTax.AskExt();
			return adapter.Get();
		}
		#endregion Actions

		#region Events
		public virtual void _(Events.RowSelected<FakeDac> e)
		{
			AssignTaxesToEmployees.SetEnabled(!Taxes.Cache.Inserted.Any_() && !TaxAttributes.Cache.IsDirty && !CompanyAttributes.Cache.IsDirty);
		}

		public virtual void _(Events.RowSelected<PRTaxCode> e)
		{
			if (e.Row == null)
			{
				return;
			}

			SetTaxCodeError(e.Cache, e.Row);
		}

		public virtual void _(Events.RowPersisting<PRTaxCodeAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError)
			{
				e.Cache.RaiseExceptionHandling<PRTaxCodeAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequired, PXErrorLevel.RowError));
			}
		}

		public virtual void _(Events.RowPersisting<PRCompanyTaxAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError)
			{
				e.Cache.RaiseExceptionHandling<PRCompanyTaxAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequiredAndNotOverridable, PXErrorLevel.RowError));
			}
		}
		#endregion Events

		#region Helpers
		private List<PRTaxCode> CreateTaxesForAllLocations()
		{
			var payrollService = new PayrollTaxClient();
			List<Address> addresses = Addresses.Select().FirstTableItems.ToList();
			TaxLocationHelpers.UpdateAddressLocationCodes(addresses, payrollService);

			List<PRTaxCode> existingTaxes = Taxes.Select().FirstTableItems.ToList();
			List<PRTaxCode> newTaxes = new List<PRTaxCode>();
			foreach (WSTaxType taxType in payrollService.GetAllLocationTaxTypes(addresses)
				.Distinct(new TaxTypeEqualityComparer())
				.Where(x => !existingTaxes.Any(y => y.TaxUniqueCode == x.UniqueTaxID)))
			{
				if (taxType.IsImplemented)
				{
					newTaxes.Add(CreateTax(taxType));
				}
				else
				{
					PXTrace.WriteWarning(Messages.TaxTypeIsNotImplemented, taxType.TaxID);
				}
			}

			return newTaxes;
		}

		private PRTaxCode CreateTax(WSTaxType taxType)
		{
			string taxCD = taxType.TaxID.Replace('_', ' ');
			string taxJurisdiction = TaxJurisdiction.GetTaxJurisdiction(taxType.TaxJurisdiction);
			string stateAbbr = taxJurisdiction == TaxJurisdiction.Federal ? LocationConstants.FederalStateCode : PRState.FromLocationCode(int.Parse(taxType.LocationCode.Split('-')[0])).Abbr;
			if (taxJurisdiction != TaxJurisdiction.Federal)
			{
				taxCD = string.Join(" ", stateAbbr, taxCD);
				if (taxJurisdiction == TaxJurisdiction.Local)
				{
					string localTaxId = taxType.LocationCode.Split('-')[1];
					if (localTaxId == "000")
					{
						localTaxId = taxType.SchoolDistrictCode;
					}
					taxCD = string.Join(" ", taxCD, localTaxId);
				}
				else if (taxJurisdiction == TaxJurisdiction.Municipal)
				{
					taxCD = string.Join(" ", taxCD, taxType.LocationCode.Split('-')[2]);
				}
				else if (taxJurisdiction == TaxJurisdiction.SchoolDistrict)
				{
					taxCD = string.Join(" ", taxCD, taxType.SchoolDistrictCode);
				}
			}

			PRTaxCode newTaxCode = new PRTaxCode()
			{
				TaxCD = taxCD
			};
			newTaxCode = Taxes.Insert(newTaxCode);

			newTaxCode.TaxCategory = TaxCategory.GetTaxCategory(taxType.TaxCategory);
			newTaxCode.TypeName = taxType.TypeName;
			newTaxCode.TaxUniqueCode = taxType.UniqueTaxID;
			if (taxJurisdiction != TaxJurisdiction.Federal)
			{
				newTaxCode.TaxState = stateAbbr;
			}

			int descriptionLength = Taxes.Cache.GetAttributesOfType<PXDBStringAttribute>(newTaxCode, nameof(PRTaxCode.description)).First().Length;
			newTaxCode.Description = taxType.Description.Length > descriptionLength ? taxType.Description.Substring(0, descriptionLength) : taxType.Description;
			return newTaxCode;
		}

		private void ValidateTaxAttributes(List<PRTaxCode> taxes)
		{
			foreach (PRTaxCode taxCodeWithError in GetTaxAttributeErrors(taxes).Where(x => x.ErrorLevel != null && x.ErrorLevel != (int?)PXErrorLevel.Undefined))
			{
				SetTaxCodeError(Taxes.Cache, taxCodeWithError);
			}
		}

		private void SetTaxCodeError(PXCache cache, PRTaxCode taxCode)
		{
			(string previousErrorMsg, PXErrorLevel previousErrorLevel) = PXUIFieldAttribute.GetErrorWithLevel<PRTaxCode.taxCD>(cache, taxCode);
			bool previousErrorIsRelated = previousErrorMsg == Messages.ValueBlankAndRequired || previousErrorMsg == Messages.NewTaxSetting;

			if (taxCode.ErrorLevel == (int?)PXErrorLevel.RowError)
			{
				PXUIFieldAttribute.SetError(cache, taxCode, nameof(taxCode.TaxCD), Messages.ValueBlankAndRequired, taxCode.TaxCD, false, PXErrorLevel.RowError);
			}
			else if ((taxCode.ErrorLevel == (int?)PXErrorLevel.RowWarning || cache.GetStatus(taxCode) == PXEntryStatus.Inserted) &&
				(previousErrorLevel != PXErrorLevel.RowError || previousErrorIsRelated))
			{
				PXUIFieldAttribute.SetError(cache, taxCode, nameof(taxCode.TaxCD), Messages.NewTaxSetting, taxCode.TaxCD, false, PXErrorLevel.RowWarning);
			}
			else if (taxCode.ErrorLevel == (int?)PXErrorLevel.Undefined && previousErrorIsRelated)
			{
				PXUIFieldAttribute.SetError(cache, taxCode, nameof(taxCode.TaxCD), "", taxCode.TaxCD, false, PXErrorLevel.Undefined);
			}
		}

		private IEnumerable<PRTaxCode> GetTaxAttributeErrors(List<PRTaxCode> taxes)
		{
			PRTaxCode restoreCurrent = Taxes.Current;
			try
			{
				foreach (PRTaxCode taxCode in taxes)
				{
					Taxes.Current = taxCode;
					foreach (PRTaxCodeAttribute taxAttribute in TaxAttributes.Select().FirstTableItems)
					{
						// Raising FieldSelecting on PRTaxCodeAttribute will set error on the attribute and propagate
						// the error/warning to the tax code
						object value = taxAttribute.Value;
						TaxAttributes.Cache.RaiseFieldSelecting<PRTaxCodeAttribute.value>(taxAttribute, ref value, false);
					}

					yield return taxCode;
				}
			}
			finally
			{
				Taxes.Current = restoreCurrent;
			}
		}

		protected static void AssignEmployeeTaxes(List<PREmployee> list)
		{
			PREmployeePayrollSettingsMaint employeeGraph = PXGraph.CreateInstance<PREmployeePayrollSettingsMaint>();
			foreach (PREmployee employee in list)
			{
				try
				{
					employeeGraph.CurrentPayrollEmployee.Current = employee;
					employeeGraph.ImportTaxesProc(true);
					employeeGraph.Persist();
				}
				catch
				{
					PXProcessing.SetError(list.IndexOf(employee), Messages.CantAssignTaxesToEmployee);
				}
			}
		}
		#endregion Helpers

		#region Helper classes
		private class TaxTypeEqualityComparer : IEqualityComparer<WSTaxType>
		{
			public bool Equals(WSTaxType x, WSTaxType y)
			{
				return x.UniqueTaxID == y.UniqueTaxID;
			}

			public int GetHashCode(WSTaxType obj)
			{
				return obj.UniqueTaxID.GetHashCode();
			}
		}

		[PXHidden]
		public class FakeDac : IBqlTable { }

		private class UpdateTaxesCustomInfo
		{
			public List<PRTaxCode> NewTaxes;
			public bool ClearTaxCache = true;

			public UpdateTaxesCustomInfo(List<PRTaxCode> newTaxes)
			{
				NewTaxes = newTaxes;
			}
		}
		
		public class InvokablePXProcessing<TTable> : PXProcessing<TTable>
			where TTable : class, IBqlTable, new()
		{
			public InvokablePXProcessing(PXGraph graph) : base(graph) { }

			public IEnumerable Invoke(PXAdapter adapter)
			{
				return ProcessAll(adapter);
			}
		}
		#endregion Helper classes
	}

	[PXHidden]
	[Serializable]
	public class PRCompanyTaxAttributeFilter : IBqlTable
	{
		#region FilterStates
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Only show attributes for states that have tax codes set up")]
		public bool? FilterStates { get; set; }
		public abstract class filterStates : PX.Data.BQL.BqlBool.Field<filterStates> { }
		#endregion
	}
}
