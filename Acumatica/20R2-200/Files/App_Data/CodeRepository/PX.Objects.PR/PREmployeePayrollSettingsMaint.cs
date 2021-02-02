using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PREmployeePayrollSettingsMaint : PXGraph<PREmployeePayrollSettingsMaint>
	{
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

		public PREmployeePayrollSettingsMaint()
		{
			EmployeeTax.AllowInsert = false;
			EmployeeTax.AllowDelete = false;
		}
		#region Views
		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Vendor> DummyVendor;
		public PXSetup<PRSetup> PRSetup;
		public PXSelect<PREmployee> PayrollEmployee;
		public PXSelect<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployee.bAccountID>>>> CurrentPayrollEmployee;
		public PXSelect<Address, Where<Address.addressID, Equal<Current<PREmployee.defAddressID>>>> Address;
		public PXSelect<PREmployeeEarning, Where<PREmployeeEarning.bAccountID, Equal<Current<PREmployee.bAccountID>>>> EmployeeEarning;
		public PXSelect<PREmployeeDeduct,
			Where<PREmployeeDeduct.bAccountID, Equal<Current<PREmployee.bAccountID>>>,
			OrderBy<Asc<PREmployeeDeduct.sequence>>> EmployeeDeduction;
		public PXSelect<PREmployeeDeduct,
			Where<PREmployeeDeduct.bAccountID, Equal<Optional<PREmployee.bAccountID>>,
			And<PREmployeeDeduct.codeID, Equal<Optional<PREmployeeDeduct.codeID>>>>> CurrentDeduction;

		public PREmployeeAttributeValueSelect<
			PREmployeeAttribute,
			SelectFrom<PREmployeeAttribute>
				.Where<PREmployeeAttribute.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>
				.OrderBy<PREmployeeAttribute.isFederal.Desc, PREmployeeAttribute.state.Asc, PREmployeeAttribute.sortOrder.Asc>,
			PRCompanyTaxAttribute,
			SelectFrom<PRCompanyTaxAttribute>
				.Where<PRCompanyTaxAttribute.state.IsEqual<LocationConstants.Federal>>,
			SelectFrom<PREmployeeTax>.
				Where<PREmployeeTax.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>,
			PREmployeeTax.state,
			PREmployee> EmployeeAttributes;

		public PXSelect<PREmployeeTax,
				Where<PREmployeeTax.bAccountID,
					Equal<Current<PREmployee.bAccountID>>>> EmployeeTax;

		public PRAttributeValuesSelect<
				PREmployeeTaxAttribute,
				Select<PREmployeeTaxAttribute,
					Where<PREmployeeTaxAttribute.bAccountID,
						Equal<Current<PREmployee.bAccountID>>,
					And<PREmployeeTaxAttribute.taxID,
						Equal<Current<PREmployeeTax.taxID>>>>,
					OrderBy<Asc<PREmployeeAttribute.sortOrder>>>,
				PRTaxCodeAttribute,
				Select<PRTaxCodeAttribute,
					Where<PRTaxCodeAttribute.taxID,
						Equal<Current<PREmployeeTax.taxID>>>>,
				PRTaxCode,
				Select<PRTaxCode,
					Where<PRTaxCode.taxID,
						Equal<Optional<PREmployeeTax.taxID>>>>,
				Payroll.Data.PRTax,
				Payroll.TaxTypeAttribute> EmployeeTaxAttributes;

		public PXOrderedSelect<PREmployee, PREmployeeDirectDeposit,
			Where<PREmployeeDirectDeposit.bAccountID, Equal<Current<PREmployee.bAccountID>>>,
			OrderBy<Asc<PREmployeeDirectDeposit.sortOrder>>> EmployeeDirectDeposit;

		public SelectFrom<PREmployeePTOBank>
			.Where<PREmployeePTOBank.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>.View PTOBanks;

		public SelectFrom<PRPayment>.
			Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>.
				And<PRPayment.released.IsNotEqual<True>>.And<PRPayment.printed.IsNotEqual<True>>>.View ActiveEmployeePayments;

		public SelectFrom<PRPaymentOvertimeRule>.
			Where<PRPaymentOvertimeRule.paymentDocType.IsEqual<P.AsString>.
				And<PRPaymentOvertimeRule.paymentRefNbr.IsEqual<P.AsString>>>.View PaymentOvertimeRules;

		public SelectFrom<PREmployeeClassPTOBank>
			.InnerJoin<PRPTOBank>.On<PRPTOBank.bankID.IsEqual<PREmployeeClassPTOBank.bankID>>
			.Where<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>
				.And<PREmployee.usePTOBanksFromClass.FromCurrent.IsEqual<True>>
				.And<PREmployeeClassPTOBank.isActive.IsEqual<True>>
				.And<PRPTOBank.isActive.IsEqual<True>>>.View EmployeeClassPTOBanks;

		public SelectFrom<PRPaymentPTOBank>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentPTOBank.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentPTOBank.docType>>>
			.Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PRPayment.printed.IsEqual<False>>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPaymentPTOBank.bankID.IsEqual<P.AsString>>>.View EditablePaymentPTOBanks;

		public SelectFrom<EmploymentHistory>.View EmploymentHistory;

		public PXFilter<CreateEditPREmployeeFilter> CreateEditPREmployeeFilter;

		public SelectFrom<PREmployeeWorkLocation>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREmployeeWorkLocation.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.Where<PREmployeeWorkLocation.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PREmployee.locationUseDflt.FromCurrent.IsEqual<False>>>.View WorkLocations;
		#endregion

		#region Data View Delegates

		public IEnumerable pTOBanks()
		{
			PXView viewSelect = new PXView(this, true, PTOBanks.View.BqlSelect);
			var employeeBankIDs = new HashSet<string>();
			List<PREmployeePTOBank> queryResult = new List<PREmployeePTOBank>();
			PXResultset<PREmployeeClassPTOBank> employeeClassBanks = EmployeeClassPTOBanks.Select();

			foreach (PREmployeePTOBank record in PTOBanks.Cache.Cached)
			{
				if (!employeeBankIDs.Contains(record.BankID) &&
					(PTOBanks.Cache.GetStatus(record) == PXEntryStatus.Updated || PTOBanks.Cache.GetStatus(record) == PXEntryStatus.Inserted))
				{
					employeeBankIDs.Add(record.BankID);
					queryResult.Add(record);
				}
			}

			foreach (PREmployeePTOBank bank in viewSelect.SelectMulti().Where(x => !employeeBankIDs.Contains(((PREmployeePTOBank)x).BankID)))
			{
				var record = PTOBanks.Cache.Locate(bank) ?? bank;
				if (PTOBanks.Cache.GetStatus(record) != PXEntryStatus.Deleted)
				{
					employeeBankIDs.Add(bank.BankID);
					queryResult.Add(bank);
				}
			}

			if (PayrollEmployee.Current?.UsePTOBanksFromClass == true)
			{
				foreach (PREmployeeClassPTOBank bank in EmployeeClassPTOBanks.Select().FirstTableItems.Where(x => !employeeBankIDs.Contains(x.BankID)))
				{
					var newBank = new PREmployeePTOBank();
					PTOBanks.SetValueExt<PREmployeePTOBank.bankID>(newBank, bank.BankID);
					PTOBanks.SetValueExt<PREmployeePTOBank.useClassDefault>(newBank, true);
					PTOBanks.Cache.SetDefaultExt<PREmployeePTOBank.bAccountID>(newBank);
					PTOBanks.Cache.SetDefaultExt<PREmployeePTOBank.employeeClassID>(newBank);

					newBank = (PREmployeePTOBank)PTOBanks.Cache.Locate(newBank) ?? PTOBanks.Insert(newBank);
					queryResult.Add(newBank);
				}
			}

			foreach (PREmployeePTOBank record in queryResult)
			{
				bool hasEmployeeClassBank = employeeClassBanks.Any(x => ((PREmployeeClassPTOBank)x).BankID == record.BankID);
				PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.isActive>(PTOBanks.Cache, record, !hasEmployeeClassBank);
				PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.useClassDefault>(PTOBanks.Cache, record, hasEmployeeClassBank);
				record.AllowDelete = !hasEmployeeClassBank;

				if (record.StartDate != null)
				{
					PTOHelper.GetPTOHistory(this, Accessinfo.BusinessDate.Value, record.BAccountID.Value, record, out decimal accumulated, out decimal used, out decimal available);
					record.AccumulatedAmount = accumulated;
					record.UsedAmount = used;
					record.AvailableAmount = available;
				}
			}

			return queryResult;
		}

		public IEnumerable employeeDeduction()
		{
			PXView bqlSelect = new PXView(this, false, EmployeeDeduction.View.BqlSelect);
			PXResultset<PRDeductCode> activeDeductCodes = SelectFrom<PRDeductCode>.Where<PRDeductCode.isActive.IsEqual<True>>.View.Select(this);

			foreach (object result in bqlSelect.SelectMulti())
			{
				PREmployeeDeduct deduct = result as PREmployeeDeduct;
				if (deduct != null)
				{
					if (deduct.CodeID != null && !activeDeductCodes.FirstTableItems.Any(x => x.CodeID == deduct.CodeID))
					{
						deduct.IsActive = false;
						PXUIFieldAttribute.SetEnabled(EmployeeDeduction.Cache, deduct, false);
						EmployeeDeduction.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(
							deduct,
							deduct.CodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return deduct;
				}
			}
		}

		public IEnumerable employmentHistory()
		{
			int? employeeID = CurrentPayrollEmployee.Current.BAccountID;
			DateTime? effectiveDate = Accessinfo.BusinessDate;
			EmploymentDates employmentDates = EmploymentHistoryHelper.GetEmploymentDates(this, employeeID, effectiveDate);
			
			yield return new EmploymentHistory
			{
				HireDate = employmentDates.ContinuousHireDate,
				TerminationDate = employmentDates.TerminationDate
			};
		}

		public IEnumerable workLocations()
		{
			if (CurrentPayrollEmployee.Current.LocationUseDflt == true)
			{
				IEnumerable<PXResult<PREmployeeClassWorkLocation, PRLocation>> employeeClassWorkLocations = SelectFrom<PREmployeeClassWorkLocation>
					.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREmployeeClassWorkLocation.locationID>>
					.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>>.View.Select(this)
					.Select(x => (PXResult<PREmployeeClassWorkLocation, PRLocation>)x);

				return employeeClassWorkLocations.Select(x => new PXResult<PREmployeeWorkLocation, PRLocation>(
					new PREmployeeWorkLocation()
					{
						EmployeeID = CurrentPayrollEmployee.Current.BAccountID,
						IsDefault = ((PREmployeeClassWorkLocation)x).IsDefault,
						LocationID = ((PREmployeeClassWorkLocation)x).LocationID
					},
					x));
			}
			else
			{
				return new PXView(this, false, WorkLocations.View.BqlSelect).SelectMulti();
			}
		}

		#endregion Data View Delegates

		#region Actions
		public PXSave<PREmployee> Save;
		public PXCancel<PREmployee> Cancel;
		public PXAction<PREmployee> Insert;
		public PXDelete<PREmployee> Delete;
		public PXFirst<PREmployee> First;
		public PXPrevious<PREmployee> Prev;
		public PXNext<PREmployee> Next;
		public PXLast<PREmployee> Last;
		#endregion

		#region Buttons

		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected virtual IEnumerable insert(PXAdapter adapter)
		{
			if (CreateEditPREmployeeFilter.AskExt() == WebDialogResult.OK)
			{
				if (this.IsImport)
				{
					InsertForImport();
					return adapter.Get();
				}

				var epGraph = PXGraph.CreateInstance<EPEmployeeSelectGraph>();
				EPEmployee employee = epGraph.Employee.SelectSingle(CreateEditPREmployeeFilter.Current.BAccountID);
				if (employee != null)
				{
					var prGraph = PXGraph.CreateInstance<PREmployeePayrollSettingsMaint>();
					prGraph.Caches[typeof(EPEmployee)] = epGraph.Caches[typeof(EPEmployee)];
					prGraph.PayrollEmployee.Extend(employee);
					CreateEditPREmployeeFilter.Current.BAccountID = null;
					throw new PXRedirectRequiredException(prGraph, string.Empty);
				}
			}

			return adapter.Get();
		}

		protected virtual void InsertForImport()
		{
			var epGraph = PXGraph.CreateInstance<EPEmployeeSelectGraph>();
			EPEmployee employee = epGraph.Employee.SelectSingle(CreateEditPREmployeeFilter.Current.BAccountID);
			if (employee != null)
			{
				Caches[typeof(EPEmployee)] = epGraph.Caches[typeof(EPEmployee)];
				PREmployee prEmployee = PayrollEmployee.Extend(employee);
				prEmployee.EmployeeClassID = CreateEditPREmployeeFilter.Current.EmployeeClassID;
				prEmployee.PaymentMethodID = CreateEditPREmployeeFilter.Current.PaymentMethodID;
				prEmployee.CashAccountID = CreateEditPREmployeeFilter.Current.CashAccountID;
				Actions.PressSave();
			}
		}

		public PXAction<PREmployee> GarnishmentDetails;
		[PXButton]
		[PXUIField(DisplayName = "Garnishment Details")]
		public virtual void garnishmentDetails()
		{
			CurrentDeduction.AskExt();
		}

		public PXAction<PREmployee> DeletePTOBank;
		[PXButton]
		[PXUIField]
		public virtual void deletePTOBank()
		{
			PTOBanks.Delete(PTOBanks.Current);
		}

		public PXAction<PREmployee> EditEmployee;
		[PXUIField(DisplayName = "Edit Employee Record", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void editEmployee()
		{
			var graph = PXGraph.CreateInstance<EmployeeMaint>();
			graph.Employee.Current = CurrentPayrollEmployee.Current;
			PXRedirectHelper.TryRedirect(graph);
		}

		public PXAction<PREmployee> ImportTaxes;
		[PXUIField(DisplayName = "Import Taxes", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXButton]
		public virtual IEnumerable importTaxes(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => ImportTaxesProc(IsImport));
			return adapter.Get();
		}
		#endregion

		#region Events
		protected virtual void _(Events.FieldUpdated<PREmployee.usePTOBanksFromClass> e)
		{
			if (e.Row == null)
			{
				return;
			}
			var row = (PREmployee)e.Row;

			if (row.UsePTOBanksFromClass == false)
			{
				foreach (PREmployeePTOBank bank in PTOBanks.Select())
				{
					bank.UseClassDefault = false;
					PTOBanks.Update(bank);
				}
			}
			else
			{
				HashSet<string> employeeClassPTOBanks = EmployeeClassPTOBanks.Select().Select(x => ((PREmployeeClassPTOBank)x).BankID).ToHashSet();
				foreach (PREmployeePTOBank bank in PTOBanks.Select().Where(x => employeeClassPTOBanks.Contains(((PREmployeePTOBank)x).BankID)))
				{
					bank.IsActive = true;
					PTOBanks.Update(bank);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployee.empType> e)
		{
			PREmployee currentRow = e.Row as PREmployee;

			if (currentRow == null)
			{
				return;
			}

			string employeeType = e.NewValue as string;
			if (employeeType == EmployeeType.SalariedExempt && currentRow.ExemptFromOvertimeRules != true)
			{
				currentRow.ExemptFromOvertimeRulesUseDflt = false;
				currentRow.ExemptFromOvertimeRules = true;
			}
			else if (employeeType == EmployeeType.SalariedNonExempt && currentRow.ExemptFromOvertimeRules != false)
			{
				currentRow.ExemptFromOvertimeRulesUseDflt = false;
				currentRow.ExemptFromOvertimeRules = false;
			}
		}

		protected virtual void _(Events.RowInserting<PREmployee> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (CurrentPayrollEmployee.Current?.UnionID != null)
			{
				e.Row.UnionUseDflt = false;
				e.Row.UnionID = CurrentPayrollEmployee.Current.UnionID;
			}
		}

		protected virtual void PREmployeeEarning_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var payRatePrecision = (short?)PRSetup.GetValueExt<PRSetup.payRateDecimalPlaces>(PRSetup.Current);
			PXDBDecimalAttribute.SetPrecision(sender, "PayRate", payRatePrecision);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.getsRemainder> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null || row.GetsRemainder == false) return;

			//Ensure that we don't have more than one row with GetsRemainder checked.
			foreach (PREmployeeDirectDeposit rec in EmployeeDirectDeposit.Select())
			{
				if (rec.GetsRemainder == true && rec != row)
				{
					rec.GetsRemainder = false;
					EmployeeDirectDeposit.Update(rec);
					EmployeeDirectDeposit.View.RequestRefresh();
				}
			}
		}

		protected virtual void _(Events.RowInserting<PREmployeeDirectDeposit> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null)
			{
				return;
			}

			//Ensure that we have exactly one row with GetsRemainder checked.
			if (!EmployeeDirectDeposit.Select().FirstTableItems.Any(x => x.GetsRemainder == true))
			{
				row.GetsRemainder = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.amount> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null && row.Amount != null)
			{
				row.Percent = null;
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.percent> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null && row.Percent != null)
			{
				row.Amount = null;
			}
		}

		protected virtual void _(Events.FieldVerifying<PREmployeeDirectDeposit.percent> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null)
			{
				var newValue = (decimal?)e.NewValue ?? 0m;
				var oldValue = (decimal?)e.OldValue ?? 0m;
				decimal? total = newValue - oldValue;
				foreach (PREmployeeDirectDeposit ddRow in EmployeeDirectDeposit.Select())
				{
					total += ddRow.Percent ?? 0m;
				}

				if (total > 100)
				{
					PXUIFieldAttribute.SetError<PREmployeeDirectDeposit.percent>(e.Cache, row, Messages.TotalOver100Pct);
					e.NewValue = 0m;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployeeDeduct> e)
		{
			var row = e.Row as PREmployeeDeduct;

			// If row doesn't have a start date, let the DAC generate an error.
			if (row == null || row.StartDate == null)
			{
				return;
			}

			if (row.EndDate < row.StartDate)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(
					row,
					row.CodeID,
					new PXSetPropertyException(Messages.InconsistentDeductDate, PXErrorLevel.RowError, row.StartDate?.ToString("d"), row.EndDate?.ToString("d")));
			}

			foreach (PREmployeeDeduct deduction in EmployeeDeduction.SearchAll<Asc<PREmployeeDeduct.codeID>>(new object[] { row.CodeID }))
			{
				if (row.LineNbr != deduction.LineNbr &&
					(row.EndDate == null && deduction.EndDate == null ||
					row.StartDate <= deduction.EndDate && row.EndDate >= deduction.StartDate ||
					row.StartDate <= deduction.EndDate && row.EndDate == null ||
					row.EndDate >= deduction.StartDate && deduction.EndDate == null))
				{
					e.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(row,
						row.CodeID,
						new PXSetPropertyException(Messages.DuplicateEmployeeDeduct, PXErrorLevel.RowError, deduction.StartDate?.ToString("d"), deduction.EndDate?.ToString("d")));
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployeeEarning> e)
		{
			var row = e.Row as PREmployeeEarning;

			// If row doesn't have a start date, let the DAC generate an error.
			if (row == null || row.StartDate == null)
			{
				return;
			}

			if (row.EndDate < row.StartDate)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeEarning.typeCD>(
					row,
					row.TypeCD,
					new PXSetPropertyException(Messages.InconsistentEarningDate, PXErrorLevel.RowError, row.StartDate?.ToString("d"), row.EndDate?.ToString("d")));
			}

			foreach (PREmployeeEarning earning in EmployeeEarning.SearchAll<Asc<PREmployeeEarning.typeCD>>(new object[] { row.TypeCD }))
			{
				if (row.LineNbr != earning.LineNbr &&
					(row.EndDate == null && earning.EndDate == null ||
					row.StartDate <= earning.EndDate && row.EndDate >= earning.StartDate ||
					row.StartDate <= earning.EndDate && row.EndDate == null ||
					row.EndDate >= earning.StartDate && earning.EndDate == null))
				{
					e.Cache.RaiseExceptionHandling<PREmployeeEarning.typeCD>(row,
						row.TypeCD,
						new PXSetPropertyException(Messages.DuplicateEmployeeEarning, PXErrorLevel.RowError, earning.StartDate?.ToString("d"), earning.EndDate?.ToString("d")));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeePTOBank.bankID> e)
		{
			var row = (PREmployeePTOBank)e.Row;
			if (row?.BankID == null)
			{
				return;
			}

			IPTOBank bank = (PREmployeeClassPTOBank)SelectFrom<PREmployeeClassPTOBank>
				.Where<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>
				.And<PREmployeeClassPTOBank.bankID.IsEqual<P.AsString>>
				.And<PREmployeeClassPTOBank.isActive.IsEqual<True>>>.View.SelectWindowed(this, 0, 1, row.BankID);
			if (bank == null)
			{
				bank = (PRPTOBank)PXSelectorAttribute.Select<PREmployeePTOBank.bankID>(e.Cache, row);
				if (bank == null)
				{
					return;
				}
			}

			row.AccrualRate = bank.AccrualRate;
			row.AccrualLimit = bank.AccrualLimit;
			row.CarryoverType = bank.CarryoverType;
			row.CarryoverAmount = bank.CarryoverAmount;
			row.FrontLoadingAmount = bank.FrontLoadingAmount;
			row.StartDate = bank.StartDate;
		}

		public void _(Events.FieldVerifying<PREmployeePTOBank.bankID> e)
		{
			if (e.Row == null || e.NewValue == null)
			{
				return;
			}

			if (PXSelectorAttribute.Select<PREmployeePTOBank.bankID>(e.Cache, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException<PREmployeePTOBank.bankID>(ErrorMessages.ValueDoesntExist, nameof(PREmployeePTOBank.bankID), e.NewValue);
			}
		}

		public void _(Events.RowSelected<PREmployeePTOBank> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.bankID>(e.Cache, e.Row, e.Row.BankID == null);
		}

		protected virtual void _(Events.RowSelecting<PREmployee> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.StdWeeksPerYearUseDflt == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.stdWeeksPerYearUseDflt>(e.Row);

			if (e.Row.HoursPerWeek == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.hoursPerWeek>(e.Row);

			if (e.Row.CalendarIDUseDflt == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.calendarIDUseDflt>(e.Row);
		}

		protected virtual void _(Events.RowUpdated<PREmployee> e)
		{
			PREmployee oldRow = e.OldRow as PREmployee;
			PREmployee newRow = e.Row as PREmployee;

			if (oldRow.ExemptFromOvertimeRules != true && newRow.ExemptFromOvertimeRules == true && ActiveEmployeePayments.Select().Any_())
			{
				foreach (PRPayment payment in ActiveEmployeePayments.Select())
				{
					payment.ApplyOvertimeRules = false;
					ActiveEmployeePayments.Update(payment);

					PaymentOvertimeRules.Select(payment.DocType, payment.RefNbr).
						ForEach(paymentOvertimeRule => PaymentOvertimeRules.Delete(paymentOvertimeRule));
				}
			}
		}

		protected virtual void _(Events.RowDeleting<PREmployee> e)
		{
			if (e.Row != null)
			{
				// We shouldn't delete the base dacs records when deleting PREmployee
				PXTableAttribute tableAttr = e.Cache.Interceptor as PXTableAttribute;
				tableAttr.BypassOnDelete(typeof(EPEmployee), typeof(Vendor), typeof(BAccount));
				PXNoteAttribute.ForceRetain<PREmployee.noteID>(e.Cache);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeePTOBank.isActive> e)
		{
			PREmployeePTOBank row = e.Row as PREmployeePTOBank;
			if (row == null)
			{
				return;
			}

			if (!e.NewValue.Equals(true))
			{
				foreach (PRPaymentPTOBank paymentPTOBank in EditablePaymentPTOBanks.Select(row.BankID))
				{
					paymentPTOBank.IsActive = false;
					paymentPTOBank.AccrualAmount = 0m;
					EditablePaymentPTOBanks.Update(paymentPTOBank);
				}
			}
		}

		public virtual void _(Events.FieldVerifying<PREmployeeWorkLocation.isDefault> e)
		{
			if (!e.ExternalCall)
			{
				return;
			}

			bool? newValueBool = e.NewValue as bool?;
			bool requestRefresh = false;
			if (newValueBool == true)
			{
				WorkLocations.Select().FirstTableItems.Where(x => x.IsDefault == true).ForEach(x =>
				{
					x.IsDefault = false;
					WorkLocations.Update(x);
					requestRefresh = true;
				});
			}
			else if (newValueBool == false && !WorkLocations.Select().FirstTableItems.Any(x => x.IsDefault == true && !x.LocationID.Equals(e.Cache.GetValue<PREmployeeWorkLocation.locationID>(e.Row))))
			{
				e.NewValue = true;
			}

			if (requestRefresh)
			{
				WorkLocations.View.RequestRefresh();
			}
		}

		public virtual void _(Events.RowInserting<PREmployeeWorkLocation> e)
		{
			if (!WorkLocations.Select().FirstTableItems.Any(x => x.IsDefault == true))
			{
				e.Row.IsDefault = true;
			}
		}

		public virtual void _(Events.RowDeleted<PREmployeeWorkLocation> e)
		{
			IEnumerable<PREmployeeWorkLocation> remainingWorkLocations = WorkLocations.Select().FirstTableItems;
			if (!remainingWorkLocations.Any(x => x.IsDefault == true))
			{
				PREmployeeWorkLocation newDefault = remainingWorkLocations.FirstOrDefault();
				if (newDefault != null)
				{
					newDefault.IsDefault = true;
					WorkLocations.Update(newDefault);
					WorkLocations.View.RequestRefresh();
				}
			}
		}

		public virtual void _(Events.RowSelected<PREmployee> e)
		{
			WorkLocations.AllowInsert = e.Row.LocationUseDflt == false;
			WorkLocations.AllowUpdate = e.Row.LocationUseDflt == false;
			WorkLocations.AllowDelete = e.Row.LocationUseDflt == false;

			bool enableLocationUseDflt = e.Row.LocationUseDflt == true || 
				(PXSelectorAttribute.Select<PREmployee.employeeClassID>(e.Cache, e.Row) as PREmployeeClass)?.WorkLocationCount > 0;
			PXUIFieldAttribute.SetEnabled<PREmployee.locationUseDflt>(e.Cache, e.Row, enableLocationUseDflt);
			PXUIFieldAttribute.SetWarning<PREmployee.locationUseDflt>(e.Cache, e.Row, enableLocationUseDflt ? null : Messages.EmployeeClassHasNoWorkLocation);

			ImportTaxesCustomInfo customInfo = PXLongOperation.GetCustomInfo(this.UID) as ImportTaxesCustomInfo;
			if (customInfo?.ClearAttributeCache == true)
			{
				EmployeeTaxAttributes.Cache.Clear();
				EmployeeAttributes.Cache.Clear();
				customInfo.ClearAttributeCache = false;
			}

			if (customInfo?.TaxesToDelete.Any() == true && !EmployeeTax.Cache.Deleted.Any_())
			{
				customInfo.TaxesToDelete.ForEach(x => EmployeeTax.Delete(x));
				customInfo.TaxesToDelete.Clear();
			}

			if (customInfo?.TaxesToAdd.Any() == true && !EmployeeTax.Cache.Inserted.Any_())
			{
				customInfo.TaxesToAdd.ForEach(x => EmployeeTax.Insert(x));
				ValidateTaxAttributes();
				customInfo.TaxesToAdd.Clear();
			}

			if (!EmployeeTaxAttributes.Cache.Cached.Any_())
			{
				ValidateTaxAttributes();
			}

			if (!EmployeeAttributes.Cache.Cached.Any_())
			{
				ValidateEmployeeAttributes();
			}
		}

		public virtual void _(Events.RowSelected<PREmployeeTax> e)
		{
			if (e.Row == null)
			{
				return;
			}

			SetTaxSettingError<PREmployeeTax.taxID>(e.Cache, e.Row, e.Row.ErrorLevel);
		}

		public virtual void _(Events.RowPersisting<PREmployeeTaxAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeTaxAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequired, PXErrorLevel.RowError));
			}
		}

		public virtual void _(Events.RowPersisting<PREmployeeAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequired, PXErrorLevel.RowError));
			}
		}

		protected virtual void _(Events.RowPersisting<Address> e)
		{
			TaxLocationHelpers.AddressPersisting(e);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeAttribute.description> e)
		{
			PREmployeeAttribute row = e.Row as PREmployeeAttribute;
			if (!IsImport || row == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(row.SettingName))
			{
				EmployeeAttributes.SetSettingNameForDescription(row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeAttribute.state> e)
		{
			PREmployeeAttribute row = e.Row as PREmployeeAttribute;
			if (!IsImport || row == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(row.SettingName))
			{
				EmployeeAttributes.SetSettingNameForDescription(row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployeeAttribute.description> e)
		{
			if (IsImport)
			{
				// Acuminator disable once PX1070 UiPresentationLogicInEventHandlers
				// The import scenario engine needs this field to be enabled for "Import PR Employee Attributes".
				PXUIFieldAttribute.SetEnabled<PREmployeeAttribute.description>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployeeAttribute.state> e)
		{
			if (IsImport)
			{
				// Acuminator disable once PX1070 UiPresentationLogicInEventHandlers
				// The import scenario engine needs this field to be enabled for "Import PR Employee Attributes".
				PXUIFieldAttribute.SetEnabled<PREmployeeAttribute.state>(e.Cache, e.Row);
			}
		}
		#endregion Events

		#region Graph Overrides
		public override void Persist()
		{
			bool foundOne = true;
			foreach (PREmployeeDirectDeposit dd in EmployeeDirectDeposit.Select())
			{
				foundOne = false;
				if (dd.GetsRemainder == true)
				{
					foundOne = true;
					break;
				}
			}
			if (!foundOne)
			{
				throw new PXException(Messages.AtLeastOneRemainderDD);
			}


			try
			{
				base.Persist();
			}
			catch (PXOuterException ex)
			{
				throw new PXPrimaryDacOuterException(ex, PayrollEmployee.Cache, typeof(PREmployee));
			}
		}
		#endregion

		#region Helpers
		private void ValidateTaxAttributes()
		{
			foreach (PREmployeeTax taxCodeWithError in GetTaxAttributeErrors().Where(x => x.ErrorLevel != null && x.ErrorLevel != (int?)PXErrorLevel.Undefined))
			{
				SetTaxSettingError<PREmployeeTax.taxID>(EmployeeTax.Cache, taxCodeWithError, taxCodeWithError.ErrorLevel);
			}
		}

		private IEnumerable<PREmployeeTax> GetTaxAttributeErrors()
		{
			PREmployeeTax restoreCurrent = EmployeeTax.Current;
			try
			{
				foreach (PREmployeeTax taxCode in EmployeeTax.Select().FirstTableItems)
				{
					EmployeeTax.Current = taxCode;
					foreach (PREmployeeTaxAttribute taxAttribute in EmployeeTaxAttributes.Select().FirstTableItems)
					{
						// Raising FieldSelecting on PREmployeeTaxAttribute will set error on the attribute and propagate
						// the error/warning to the tax code
						object value = taxAttribute.Value;
						EmployeeTaxAttributes.Cache.RaiseFieldSelecting<PREmployeeTaxAttribute.value>(taxAttribute, ref value, false);
					}

					yield return taxCode;
				}
			}
			finally
			{
				EmployeeTax.Current = restoreCurrent;
			}
		}

		private void SetTaxSettingError<TErrorField>(PXCache cache, IBqlTable row, int? errorLevel) where TErrorField : IBqlField
		{
			(string previousErrorMsg, PXErrorLevel previousErrorLevel) = PXUIFieldAttribute.GetErrorWithLevel<TErrorField>(cache, row);
			bool previousErrorIsRelated = previousErrorMsg == Messages.ValueBlankAndRequired || previousErrorMsg == Messages.NewTaxSetting;

			if (errorLevel == (int?)PXErrorLevel.RowError)
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, Messages.ValueBlankAndRequired, cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.RowError);
			}
			else if ((errorLevel == (int?)PXErrorLevel.RowWarning || cache.GetStatus(row) == PXEntryStatus.Inserted) &&
				(previousErrorLevel != PXErrorLevel.RowError || previousErrorIsRelated))
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, Messages.NewTaxSetting, cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.RowWarning);
			}
			else if (errorLevel == (int?)PXErrorLevel.Undefined && previousErrorIsRelated)
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, "", cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.Undefined);
			}
		}

		private void ValidateEmployeeAttributes()
		{
			foreach (PREmployeeAttribute attribute in EmployeeAttributes.Select().FirstTableItems)
			{
				object value = attribute.Value;
				EmployeeAttributes.Cache.RaiseFieldSelecting<PREmployeeAttribute.value>(attribute, ref value, false);
				SetTaxSettingError<PREmployeeAttribute.value>(EmployeeAttributes.Cache, attribute, attribute.ErrorLevel);
				if (attribute.ErrorLevel == (int?)PXErrorLevel.RowError)
				{
					EmployeeAttributes.Cache.SetStatus(attribute, PXEntryStatus.Modified);
				}
			}
		}

		public void ImportTaxesProc(bool isMassProcess)
		{
			if (CurrentPayrollEmployee.Current == null)
			{
				return;
			}

			Address residenceAddress = Address.Current ?? Address.SelectSingle();
			List<Address> taxableAddresses = new List<Address>() { residenceAddress };
			if (CurrentPayrollEmployee.Current.LocationUseDflt == true)
			{
				taxableAddresses.AddRange(SelectFrom<Address>
					.InnerJoin<PRLocation>.On<PRLocation.addressID.IsEqual<Address.addressID>>
					.InnerJoin<PREmployeeClassWorkLocation>.On<PREmployeeClassWorkLocation.locationID.IsEqual<PRLocation.locationID>>
					.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>>.View.Select(this).FirstTableItems);
			}
			else
			{
				taxableAddresses.AddRange(WorkLocations.Select().ToList().Select(x => (Address)x[typeof(Address)]));
			}

			var payrollService = new PayrollTaxClient();
			IEnumerable<Address> addressesWithoutLocationCode = taxableAddresses.Where(x => string.IsNullOrEmpty(x.TaxLocationCode));
			if (TaxLocationHelpers.IsAddressedModified(Address.Cache, residenceAddress))
			{
				addressesWithoutLocationCode = addressesWithoutLocationCode.Union(new List<Address> { residenceAddress }, new TaxLocationHelpers.AddressEqualityComparer());
			}

			if (addressesWithoutLocationCode.Any())
			{
				TaxLocationHelpers.UpdateAddressLocationCodes(addressesWithoutLocationCode.ToList(), payrollService);
				Address.Cache.SetStatus(residenceAddress, PXEntryStatus.Notchanged);
			}

			HashSet<string> applicableTaxCodes = payrollService.GetAllLocationTaxTypes(taxableAddresses).Select(x => x.UniqueTaxID).ToHashSet();
			HashSet<int?> taxIDsToAdd = SelectFrom<PRTaxCode>.View.Select(this).FirstTableItems
				.Where(x => applicableTaxCodes.Contains(x.TaxUniqueCode))
				.Select(x => x.TaxID)
				.ToHashSet();

			List<PREmployeeTax> taxesToDelete = new List<PREmployeeTax>();
			foreach (PREmployeeTax employeeTax in EmployeeTax.Select().FirstTableItems)
			{
				if (!taxIDsToAdd.Contains(employeeTax.TaxID))
				{
					taxesToDelete.Add(employeeTax);
				}
				else
				{
					taxIDsToAdd.Remove(employeeTax.TaxID);
				}
			}

			List<PREmployeeTax> taxesToAdd = taxIDsToAdd.Select(x => new PREmployeeTax() { TaxID = x }).ToList();

			if (isMassProcess)
			{
				taxesToAdd.ForEach(x => EmployeeTax.Insert(x));
				taxesToDelete.ForEach(x => EmployeeTax.Delete(x));
			}
			else
			{
				PXLongOperation.SetCustomInfo(new ImportTaxesCustomInfo(taxesToAdd, taxesToDelete));
			}
		}

		protected virtual void _(Events.RowInserted<PREmployeeTax> e)
		{
			ValidateTaxAttributes();
			ValidateEmployeeAttributes();
		}
		#endregion Helpers

		#region Helper classes
		private class ImportTaxesCustomInfo
		{
			public List<PREmployeeTax> TaxesToAdd;
			public List<PREmployeeTax> TaxesToDelete;
			public bool ClearAttributeCache = true;

			public ImportTaxesCustomInfo(List<PREmployeeTax> taxesToAdd, List<PREmployeeTax> taxesToDelete)
			{
				TaxesToAdd = taxesToAdd;
				TaxesToDelete = taxesToDelete;
			}
		}
		#endregion Helper classes

		#region Address Lookup Extension
		/// <exclude/>
		public class PREmployeePayrollSettingsMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<PREmployeePayrollSettingsMaint, PREmployee, Address>
		{
			protected override string AddressView => nameof(Base.Address);
		}
		#endregion
	}

	[PXCacheName(Messages.EmploymentHistory)]
	public class EmploymentHistory : IBqlTable
	{
		#region HireDate
		public abstract class hireDate : PX.Data.BQL.BqlDateTime.Field<hireDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Hire Date", Enabled = false)]
		public DateTime? HireDate { get; set; }
		#endregion

		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Termination Date", Enabled = false)]
		public DateTime? TerminationDate { get; set; }
		#endregion
	}

	[PXCacheName(Messages.CreateEditPREmployeeFilter)]
	public class CreateEditPREmployeeFilter : IBqlTable
	{
		#region BAccountID
		[PXInt]
		[PXUIField(DisplayName = "Employee ID")]
		[PXDimensionSelector(EmployeeRawAttribute.DimensionName,
				typeof(Search2<CR.Standalone.EPEmployee.bAccountID,
							LeftJoin<PREmployee, On<CR.Standalone.EPEmployee.bAccountID,
								Equal<PREmployee.bAccountID>>>,
							Where<PREmployee.bAccountID, IsNull>>),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.bAccountID),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.acctName),
				typeof(CR.Standalone.EPEmployee.departmentID))]
		public virtual int? BAccountID { get; set; }
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		#endregion

		#region EmployeeClassID
		public abstract class employeeClassID : BqlString.Field<employeeClassID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visible = false)]
		[PXSelector(typeof(PREmployeeClass.employeeClassID))]
		public string EmployeeClassID { get; set; }
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.isActive, Equal<True>,
				And<PRxPaymentMethod.useForPR, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public virtual string PaymentMethodID { get; set; }
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[UnboundCashAccount(typeof(Search2<CashAccount.cashAccountID,
			InnerJoin<PaymentMethodAccount,
				On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<paymentMethodID>>,
					And<PRxPaymentMethodAccount.useForPR, Equal<True>>>>>,
			Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", DescriptionField = typeof(CashAccount.descr), Visible = false)]
		public virtual int? CashAccountID { get; set; }
		#endregion
	}

	[PXInt]
	public class UnboundCashAccountAttribute : CashAccountBaseAttribute
	{
		public UnboundCashAccountAttribute(Type search) : base(null, search)
		{
		}
	}

	[PXHidden]
	public class EPEmployeeSelectGraph : PXGraph<EPEmployeeSelectGraph>
	{
		public PXSelect<EPEmployee,
					Where<EPEmployee.bAccountID,
						Equal<Required<EPEmployee.bAccountID>>>> Employee;
	}
}