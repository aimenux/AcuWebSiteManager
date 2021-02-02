using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CT;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;
using System.Diagnostics;
using PX.Objects.AR;
using PX.Objects.SO;
using CommonServiceLocator;

namespace PX.Objects.PM
{

	#region Project Selectors

	/// <summary>
	/// Selector Attribute that displays all Projects including Templates.
	/// Selector also has <see cref="WarnIfCompleted"/> property for field verification.
	/// This Attribute also contains static Util methods.
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]
	[PXAttributeFamily(typeof(AcctSubAttribute))]
	public class ProjectAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber
	{
		public const string DimensionName = "PROJECT";
		public const string DimensionNameTemplate = "TMPROJECT";


		public ProjectAttribute()
		{
			WarnIfCompleted = true;

			Type SearchType =
					BqlCommand.Compose(
					typeof(Search2<,,>),
					typeof(PMProject.contractID),
					typeof(LeftJoin<,,>),
							typeof(Customer),
							typeof(On<,>),
							typeof(Customer.bAccountID),
							typeof(Equal<>),
							typeof(PMProject.customerID),
					typeof(LeftJoin<,>),
							typeof(ContractBillingSchedule),
							typeof(On<,>),
							typeof(ContractBillingSchedule.contractID),
							typeof(Equal<>),
							typeof(PMProject.contractID),
					typeof(Where2<,>),
					typeof(Where<,,>),
					typeof(PMProject.baseType),
					typeof(Equal<CT.CTPRType.project>),
					typeof(Or<,>),
					typeof(PMProject.baseType),
					typeof(Equal<CT.CTPRType.projectTemplate>),
					typeof(And<Match<Current<AccessInfo.userName>>>)
					);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, SearchType, typeof(PMProject.contractCD),
				typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.customerID), typeof(Customer.acctName),
				typeof(PMProject.locationID), typeof(PMProject.status), typeof(PMProject.approverID), typeof(PMProject.startDate),
				typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), typeof(PMProject.curyID));
			select.DescriptionField = typeof(PMProject.description);

			select.ValidComboRequired = true;
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
			Filterable = true;
		}

		/// <summary>
		/// Creates an instance of ProjectAttribute.
		/// </summary>
		/// <param name="where">BQL Where 
		/// </param>
		public ProjectAttribute(Type where)
		{
			WarnIfCompleted = true;

			Type SearchType =
					BqlCommand.Compose(
					typeof(Search2<,,>),
					typeof(PMProject.contractID),
					typeof(LeftJoin<,,>),
							typeof(Customer),
							typeof(On<,>),
							typeof(Customer.bAccountID),
							typeof(Equal<>),
							typeof(PMProject.customerID),
					typeof(LeftJoin<,>),
							typeof(ContractBillingSchedule),
							typeof(On<,>),
							typeof(ContractBillingSchedule.contractID),
							typeof(Equal<>),
							typeof(PMProject.contractID),
					typeof(Where2<,>),
					typeof(Where<,,>),
					typeof(PMProject.baseType),
					typeof(Equal<CT.CTPRType.project>),
					typeof(Or<,>), 
					typeof(PMProject.baseType),
					typeof(Equal<CT.CTPRType.projectTemplate>),
					typeof(And2<,>),
					typeof(Match<Current<AccessInfo.userName>>),
					typeof(And<>),
					where);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, SearchType, typeof(PMProject.contractCD),
				typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.customerID), typeof(Customer.acctName),
				typeof(PMProject.locationID), typeof(PMProject.status), typeof(PMProject.approverID), typeof(PMProject.startDate),
				typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), typeof(PMProject.curyID));
			select.DescriptionField = typeof(PMProject.description);

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		/// <summary>
		/// If True a Warning will be shown if the Project selected is Completed.
		/// Default = True
		/// </summary>
		///
		public bool WarnIfCompleted { get; set; }

		/// <summary>
		/// Composes VisibleInModule Type to be used in BQL
		/// </summary>
		/// <param name="Module">Module</param>
		public static Type ComposeVisibleIn(string Module)
		{
			Type visibleInModule;
			switch (Module)
			{
				case BatchModule.GL:
					visibleInModule = typeof(PMProject.visibleInGL);
					break;
				case BatchModule.AP:
					visibleInModule = typeof(PMProject.visibleInAP);
					break;
				case BatchModule.AR:
					visibleInModule = typeof(PMProject.visibleInAR);
					break;
				case BatchModule.SO:
					visibleInModule = typeof(PMProject.visibleInSO);
					break;
				case BatchModule.PO:
					visibleInModule = typeof(PMProject.visibleInPO);
					break;
				case BatchModule.IN:
					visibleInModule = typeof(PMProject.visibleInIN);
					break;
				case BatchModule.CA:
					visibleInModule = typeof(PMProject.visibleInCA);
					break;
				case BatchModule.CR:
					visibleInModule = typeof(PMProject.visibleInCR);
					break;
				default:
					throw new ArgumentOutOfRangeException("Module", Module, Messages.ProjectAttributeNotSupport);
			}

			return visibleInModule;
		}

		/// <summary>
		/// Returns True if the given module is integrated with PM.
		/// </summary>
		/// <remarks>Always returns True if Module is null or an empty string.</remarks>
		public static bool IsPMVisible(string module)
		{
			if (string.IsNullOrEmpty(module))
				return true;

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
				return false;

			IProjectSettingsManager psm = ServiceLocator.Current.GetInstance<IProjectSettingsManager>();
			return psm.IsPMVisible(module);
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
		public static bool IsAutonumbered(PXGraph graph, string dimensionID)
		{
			return DimensionMaint.IsAutonumbered(graph, dimensionID, false);
		}

		#region IPXFieldVerifyingSubscriber Members

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
			if (project != null)
			{
				if (WarnIfCompleted && project.IsCompleted == true)
				{
					sender.RaiseExceptionHandling(FieldName, e.Row, e.NewValue,
						new PXSetPropertyException(Warnings.ProjectIsCompleted, PXErrorLevel.Warning));
				}
			}
		}

		#endregion

	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), Messages.InactiveContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), Messages.CompleteContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), Messages.CancelledContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>, 
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]	
	public class ActiveProjectOrContractBaseAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber
	{
		protected Type customerField;

		public ActiveProjectOrContractBaseAttribute() : this(null) { }

		public ActiveProjectOrContractBaseAttribute(Type customerField)
		{
			this.customerField = customerField;

			PXDimensionSelectorAttribute select;

			if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>() && customerField != null)
			{

				List<Type> command = new List<Type>();

				command.AddRange(new[] {
				typeof(Search2<,,>),
					typeof(PMProject.contractID),
					typeof(LeftJoin<,>),
							typeof(Customer),
							typeof(On<,>),
							typeof(Customer.bAccountID),
							typeof(Equal<>),
							typeof(PMProject.customerID),
						});

				command.AddRange(
					new[]
					{

						typeof(Where<,,>),
						typeof (PMProject.nonProject),
						typeof (Equal<>),
						typeof (True),
						typeof (Or2<,>),
						typeof (Where2<,>),
						typeof (Where<,,>),
						typeof (PMProject.customerID),
						typeof (Equal<>),
						typeof (Current<>),
						customerField,
						typeof (And<,,>),
						typeof (PMProject.restrictProjectSelect),
						typeof (Equal<>),
						typeof (PMRestrictOption.customerProjects),
						typeof (Or<,,>),
						typeof (PMProject.restrictProjectSelect),
						typeof (Equal<>),
						typeof (PMRestrictOption.allProjects),
						typeof (Or<,>),
						typeof(PMProject.baseType),
						typeof(Equal<CT.CTPRType.contract>),
						typeof (Or<,>),
						typeof (Current<>),
						customerField,
						typeof (IsNull),
						typeof (And2<,>),
						typeof (Match<Current<AccessInfo.userName>>),
						typeof (Or<,>),
						typeof (PMProject.nonProject),
						typeof (Equal<>),
						typeof (True)
					});

				select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName,
				BqlCommand.Compose(command.ToArray())
				, typeof(PMProject.contractCD), typeof(PMProject.contractCD), typeof(PMProject.description),
				 typeof(PMProject.status), typeof(PMProject.customerID), typeof(Customer.acctName), typeof(PMProject.curyID));
			}
			else
			{
				select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName,
				typeof(Search2<PMProject.contractID,
				LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>,
				Where<PMProject.nonProject, Equal<True>, Or<Match<Current<AccessInfo.userName>>>>>)
				, typeof(PMProject.contractCD), typeof(PMProject.contractCD), typeof(PMProject.description),
				typeof(PMProject.status), typeof(PMProject.customerID), typeof(Customer.acctName), typeof(PMProject.curyID));
			}

			select.DescriptionField = typeof(PMProject.description);
			select.ValidComboRequired = true;
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
			if (customerField != null && project != null && project.NonProject != true)
			{
				int? customerID = (int?)sender.GetValue(e.Row, customerField.Name);

				if (customerID != project.CustomerID)
				{
					sender.RaiseExceptionHandling(FieldName, e.Row, e.NewValue,
						new PXSetPropertyException(Warnings.ProjectCustomerDontMatchTheDocument, PXErrorLevel.Warning));
				}
			}
		}
	}
	
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
	[PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), Messages.CompleteContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>,
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
	public class ProjectBaseAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber
	{
		protected Type customerField;

		public ProjectBaseAttribute():this(null)
		{

		}

		public ProjectBaseAttribute(Type customerField)
		{
			this.customerField = customerField;

			PXDimensionSelectorAttribute select;

			if (customerField == null)
			{
				select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName,
						typeof(Search2<PMProject.contractID,
						LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>,
						Where<PMProject.baseType, Equal<CTPRType.project>,
						And2<Match<Current<AccessInfo.userName>>, Or<PMProject.nonProject, Equal<True>>>>>)
						, typeof(PMProject.contractCD), typeof(PMProject.contractCD), typeof(PMProject.description),
						typeof(PMProject.status), typeof(PMProject.customerID), typeof(Customer.acctName), typeof(PMProject.curyID));
			}
			else
			{
				List<Type> command = new List<Type>();

				command.AddRange(new[] {
				typeof(Search2<,,>),
					typeof(PMProject.contractID),
					typeof(LeftJoin<,>),
							typeof(Customer),
							typeof(On<,>),
							typeof(Customer.bAccountID),
							typeof(Equal<>),
							typeof(PMProject.customerID),
						});

				command.AddRange(
					new[]
					{
						typeof(Where<,,>),
						typeof(PMProject.baseType),
						typeof(Equal<>),
						typeof(CT.CTPRType.project),
						typeof (And2<,>),
						typeof (Where2<,>),
						typeof (Where<,,>),
						typeof (PMProject.customerID),
						typeof (Equal<>),
						typeof (Current<>),
						customerField,
						typeof (And<,,>),
						typeof (PMProject.restrictProjectSelect),
						typeof (Equal<>),
						typeof (PMRestrictOption.customerProjects),
						typeof (Or<,>),
						typeof (PMProject.restrictProjectSelect),
						typeof (Equal<>),
						typeof (PMRestrictOption.allProjects),
						typeof (Or<,>),
						typeof (Current<>),
						customerField,
						typeof (IsNull),
						typeof (And2<,>),
						typeof (Match<Current<AccessInfo.userName>>),
						typeof (Or<,>),
						typeof (PMProject.nonProject),
						typeof (Equal<>),
						typeof (True)
					});

				select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName,
				BqlCommand.Compose(command.ToArray())
				, typeof(PMProject.contractCD), typeof(PMProject.contractCD), typeof(PMProject.description),
				typeof(PMProject.status), typeof(PMProject.customerID), typeof(Customer.acctName), typeof(PMProject.curyID));
			}

			select.DescriptionField = typeof(PMProject.description);
			select.ValidComboRequired = true;
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
		}
		
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{

			if (customerField == null)
				return;

			PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
			if (project != null && project.NonProject != true)
			{
				int? customerID = (int?)sender.GetValue(e.Row, customerField.Name);

				if (customerID != project.CustomerID)
				{
					sender.RaiseExceptionHandling(FieldName, e.Row, e.NewValue,
						new PXSetPropertyException(Warnings.ProjectCustomerDontMatchTheDocument, PXErrorLevel.Warning));
				}
			}
		}
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), Messages.InactiveContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), Messages.CompleteContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>, 
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.visibleInGL, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
	public class ActiveProjectOrContractForGLAttribute : ActiveProjectOrContractBaseAttribute
	{

		public Type AccountFieldType { get; set; }
		public ActiveProjectOrContractForGLAttribute():base(null)
		{
			Filterable = true;
		}

		#region IPXFieldVerifyingSubscriber Members

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
			if (project != null && project.NonProject != true)
			{
				if (project.BaseType == CT.CTPRType.Project)
				{
					if (project != null && project.NonProject != true && project.BaseType == CT.CTPRType.Project &&
						AccountFieldType != null)
					{
						Account account =
							PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph,
								sender.GetValue(e.Row, AccountFieldType.Name));
						if (account != null && account.AccountGroupID == null)
						{
							var newRow = sender.CreateCopy(e.Row);
							sender.SetValue(newRow, _FieldName, e.NewValue);
							e.NewValue = sender.GetStateExt(newRow, _FieldName);
							throw new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Error, account.AccountCD);
						}
					}
				}
			}
		}
		#endregion
	}

	[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), Messages.InactiveContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.visibleInCA, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
	public class CAActiveProjectAttribute : ProjectBaseAttribute
	{
		public Type AccountFieldType { get; set; }

		public CAActiveProjectAttribute() : base(null)
		{
			AccountFieldType = typeof(CA.CASplit.accountID);
			Filterable = true;
		}

		#region IPXFieldVerifyingSubscriber Members
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
			if (project != null && project.NonProject != true && project.BaseType == PM.PMProject.ProjectBaseType.Project && AccountFieldType != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, sender.GetValue(e.Row, AccountFieldType.Name));
				if (account != null && account.AccountGroupID == null)
				{
					var ex = new PXSetPropertyException(Messages.NoAccountGroup, PXErrorLevel.Error, account.AccountCD);
					ex.ErrorValue = project.ContractCD;
					throw ex;
				}
			}
		}
		#endregion
	}

	#region EPTimeCardActiveProjectAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<PMProject.isCancelled,  NotEqual<True>>), Messages.CompleteContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>,
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.visibleInTA, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
	public class EPTimeCardProjectAttribute : AcctSubAttribute
	{
		public EPTimeCardProjectAttribute()
		{
			Type searchType = typeof(Search2<PMProject.contractID,
			LeftJoin<EPEmployeeContract, On<EPEmployeeContract.contractID, Equal<PMProject.contractID>, And<EPEmployeeContract.employeeID, Equal<Current<EPTimeCard.employeeID>>>>,
			LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<PMProject.customerID>>,
			LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>>>,
			Where<PMProject.baseType, Equal<CT.CTPRType.project>,
			And2<Where<PMProject.restrictToEmployeeList, Equal<False>, Or<EPEmployeeContract.employeeID, IsNotNull>>,
			And<Match<Current<AccessInfo.userName>>>>>,
			OrderBy<Desc<PMProject.contractCD>>>);


			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, searchType, typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.customerID), typeof(BAccountR.acctName), typeof(PMProject.locationID), 
			typeof(PMProject.status), typeof(PMProject.approverID), typeof(PMProject.startDate), typeof(ContractBillingSchedule.lastDate),
			typeof(ContractBillingSchedule.nextDate), typeof(PMProject.curyID));
			select.DescriptionField = typeof(PMProject.description);
			select.ValidComboRequired = true;
			
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
			CacheGlobal = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			PXGraph graph = sender.Graph;
			if (graph == null)
				throw new ArgumentNullException("graph");

			Visible = Enabled = ProjectAttribute.IsPMVisible(BatchModule.TA);
		}
	}

	#endregion 

	
	#region EPEquipmentActiveProjectAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), Messages.InactiveContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), Messages.CompleteContract, typeof(PMProject.contractCD))]
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>,
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
	public class EPEquipmentActiveProjectAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber
	{
		public EPEquipmentActiveProjectAttribute()
		{
			Type searchType = typeof(Search2<PMProject.contractID,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>,
			LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>>,
			Where<PMProject.baseType, Equal<CT.CTPRType.project>,
			And<Match<Current<AccessInfo.userName>>>>,
			OrderBy<Desc<PMProject.contractCD>>>);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, searchType, typeof(PMProject.contractCD), 
				typeof(PMProject.contractCD), typeof(PMProject.description),typeof(PMProject.customerID), typeof(Customer.acctName), 
				typeof(PMProject.locationID), typeof(PMProject.status),typeof(PMProject.approverID), typeof(PMProject.startDate), 
				typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), typeof(PMProject.curyID));
			select.DescriptionField = typeof(PMProject.description);
			select.ValidComboRequired = true;
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			PXGraph graph = sender.Graph;
			if (graph == null)
				throw new ArgumentNullException("graph");

			Visible = Enabled = ProjectAttribute.IsPMVisible(BatchModule.TA);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
            if (e.Row != null && e.NewValue != null)
            {
                PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(sender.Graph, e.NewValue);
                if (project != null && project.RestrictToResourceList == true)
                {
                    PMProject allowedForProject = PXSelectJoin<PMProject, LeftJoin<EPEquipmentRate,
                                                            On<EPEquipmentRate.projectID, Equal<PMProject.contractID>,
                                                            And<EPEquipmentRate.equipmentID, Equal<Current<EPEquipmentTimeCard.equipmentID>>>>>,
                                                            Where<EPEquipmentRate.projectID, Equal<Required<PMProject.contractID>>>>.Select(sender.Graph, project.ContractID);
                    if (allowedForProject == null)
                        throw new PXSetPropertyException(EP.Messages.ProjectIsNotAvailableForEquipment);
                }
            }
	    }
	}

	#endregion 

	#endregion

	#region Project Task Selectors
	/// <summary>
	/// Displays all Tasks for the given Project. Task is mandatory if Project is set.
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = ProjectTaskAttribute.DisplayNameText, Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
	public class ProjectTaskAttribute : AcctSubAttribute, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber
	{
		public const string DisplayNameText = "Project Task";
		public const string DimensionName = "PROTASK";
		Type projectIDField;
		string module;

		/// <summary>
		/// If True allows TaskID to be null if ProjectID is a Contract.
		/// </summary>
		///
		public bool AllowNullIfContract { get; set; }

		public bool AllowNull { get; set; }

		/// <summary>
		/// Field is always enebled even if project is null or invalid.
		/// </summary>
		public bool AlwaysEnabled { get; set; }

		public ProjectTaskAttribute(Type projectID):this(projectID, (Type)null)
		{
			Filterable = true;
		}

		public ProjectTaskAttribute(Type projectID, Type Where)
		{
			if (projectID == null)
				throw new ArgumentNullException("projectID");

			projectIDField = projectID;


			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Current<>),
				projectID);

			if (Where != null)
			{
				SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Current<>),
				projectID,
				typeof(And<>),
				Where);
			}
			

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(PMTask.taskCD),
				typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status));
			select.DescriptionField = typeof(PMTask.description);
			select.ValidComboRequired = true;
			

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
		}

		public ProjectTaskAttribute(Type projectID, string Module)
		{
			if (projectID == null)
				throw new ArgumentNullException("projectID");

			if (string.IsNullOrEmpty(Module))
				throw new ArgumentNullException("Module");

			projectIDField = projectID;
			module = Module;

			Type visibleInModule;
			switch (Module)
			{
				case BatchModule.GL:
					visibleInModule = typeof(PMTask.visibleInGL);
					break;
				case BatchModule.AP:
					visibleInModule = typeof(PMTask.visibleInAP);
					break;
				case BatchModule.AR:
					visibleInModule = typeof(PMTask.visibleInAR);
					break;
				case BatchModule.SO:
					visibleInModule = typeof(PMTask.visibleInSO);
					break;
				case BatchModule.PO:
					visibleInModule = typeof(PMTask.visibleInPO);
					break;
				case BatchModule.IN:
					visibleInModule = typeof(PMTask.visibleInIN);
					break;
				case BatchModule.TA:
					visibleInModule = typeof(PMTask.visibleInTA);
					break;
				case BatchModule.CA:
					visibleInModule = typeof(PMTask.visibleInCA);
					break;					
				case BatchModule.CR:
					visibleInModule = typeof(PMTask.visibleInCR);
					break;
				default:
					throw new ArgumentOutOfRangeException("Module", Module, Messages.ProjectTaskAttributeNotSupport);
			}

			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,,>),
				visibleInModule,
				typeof(Equal<True>),
				typeof(And<,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Current<>),
				projectID);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(PMTask.taskCD),
				typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status));
			select.DescriptionField = typeof(PMTask.description);
			select.ValidComboRequired = true;
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;

			Filterable = true;
		}

		#region IPXRowPersistingSubscriber Members

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			//rule: Task is mandatory only if Project is not an X project.

			int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
			int? taskID = (int?)sender.GetValue(e.Row, FieldOrdinal);

			if (projectID != null && !ProjectDefaultAttribute.IsNonProject(projectID) && taskID == null && e.Operation != PXDBOperation.Delete)
			{
				if (AllowNull) return;

				if (AllowNullIfContract)
				{
					//projectID may be contract and task is not required.
					PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(sender.Graph, projectID);
					if (project.BaseType == CT.CTPRType.Contract)
					{
						return;
					}
				}

				if (sender.RaiseExceptionHandling(FieldName, e.Row, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, FieldName)))
				{
					throw new PXRowPersistingException(FieldName, null, Data.ErrorMessages.FieldIsEmpty, FieldName);
				}

			}
		}

		#endregion

		#region IPXFieldSelectingSubscriber Members

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXFieldState ss = e.ReturnState as PXFieldState;
			if (ss != null && e.Row != null && !AlwaysEnabled)
			{
				PMProject project = null;
				if (e.Row != null)
				{
					project = PXSelectorAttribute.Select(sender, e.Row, projectIDField.Name) as PMProject;

					if (project == null)
					{
						//Can be that Selector Attribute is not defined for the given field. Fallback to slower method:
						int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
						project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, projectID);

					}
				}

                ss.Enabled = (project != null && (project.BaseType == CT.CTPRType.Project|| project.BaseType == CT.CTPRType.ProjectTemplate) && project.NonProject != true && sender.GetValue(e.Row, projectIDField.Name) != null) || (sender.Graph.IsImport && !sender.Graph.IsMobile);
			}
			ss.Visible = ProjectAttribute.IsPMVisible( module);
		}
		#endregion

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), projectIDField.Name, OnProjectUpdated);
		}
		protected PMTask GetDefaultTask(PXGraph graph, int? projectID)
		{
			if (projectID == null) return null;
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.isDefault, Equal<True>>>>.Select(graph, projectID);

			return task;
		}

		protected virtual void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object taskCD = (sender.GetValuePending(e.Row, _FieldName) as string) ?? (sender.GetValueExt(e.Row, _FieldName) as PXSegmentedState).Value;
			object projectID = sender.GetValue(e.Row, projectIDField.Name);

			if (taskCD != null && taskCD != PXCache.NotSetValue)
			{
				try
				{

					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, projectID, taskCD);

					if (task != null && !ProjectDefaultAttribute.IsNonProject((int?)projectID))
					{
						sender.SetValueExt(e.Row, _FieldName, task.TaskID);
						object val = sender.GetValue(e.Row, _FieldName);
						if (task.TaskID != (int?)val)
						{
							PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
							sender.SetValueExt(e.Row, _FieldName, null);
						}
					}
					else
					{
						object pendingValue = null;
						if (ProjectDefaultAttribute.IsNonProject((int?)projectID))
						{
							object projectCD = sender.GetValuePending(e.Row, projectIDField.Name);
							if (projectCD != null && projectCD != PXCache.NotSetValue)
							{
								PMProject project = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Required<PMProject.contractCD>>>>.Select(sender.Graph, projectCD);
								if (project != null && project.NonProject != true)
								{
									task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, project.ContractID, taskCD);
									if (task != null)
									{
										pendingValue = task.TaskCD;
									}
								}
							}
							else if (projectCD == PXCache.NotSetValue)
							{
								object val = sender.GetValue(e.Row, _FieldName);
								if (val != null)
								{
									sender.SetValue(e.Row, _FieldName, null);
								}
							}
						}

						if (pendingValue == null)
						{
							sender.SetValuePending(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskCD);
						}
						else
						{
							sender.SetValuePending(e.Row, _FieldName, pendingValue);
						}
					}
				}
				catch (PXException ex)
				{
					if (sender.Graph.UnattendedMode)
					{
						throw ex;
					}
					else
					{
						sender.SetValuePending(e.Row, _FieldName, null);
					}
				}
			}
			else
			{
				sender.SetValue(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskID);
			}
		}

	}



	/// <summary>
	/// Attribute for TaskCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.Visible)]
	public class ProjectTaskRawAttribute : AcctSubAttribute
	{
		public ProjectTaskRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(ProjectTaskAttribute.DimensionName);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}


	/// <summary>
	/// Task Selector that displays all Tasks for the given Project. Task Field is Disabled if a Non-Project is selected; otherwise mandatory.
	/// Task Selector always work in pair with Project Selector. When the Project Selector displays a valid Project Task field becomes mandatory.
	/// When This selector is used in pair with <see cref="ActiveProjectOrContractAttribute"/> and a Contract is selected Task is no longer mandatory.
	/// If Completed Task is selected an error will be displayed - Completed Task cannot be used in DataEntry.
	/// </summary>
	/// 
	[PXDBInt()]
	[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
    public class BaseProjectTaskAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber, IPXFieldVerifyingSubscriber
	{
		readonly Type projectIDField;
		public Type NeedTaskValidationField { get; set; }
	    readonly string module;

		protected int _ModuleRestrictorAttrIndex = -1;
		protected Type _visibleInModule;

		public bool AllowCompleted { get; set; }

		public bool AllowCanceled { get; set; }

		public bool AllowInactive { get; set; } = true;


		public BaseProjectTaskAttribute(Type projectID)
		{
			if (projectID == null)
				throw new ArgumentNullException("projectID");

			projectIDField = projectID;

			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Optional<>),
				projectID
				);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectTaskAttribute.DimensionName, SearchType, typeof(PMTask.taskCD),
				typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status));
			select.DescriptionField = typeof(PMTask.description);
			select.ValidComboRequired = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
			Filterable = true;
		}

		public BaseProjectTaskAttribute(Type projectID, string Module): this(projectID)
		{
			if (string.IsNullOrEmpty(Module))
				throw new ArgumentNullException("Module");

			module = Module;

			switch (Module)
			{
				case BatchModule.GL:
					_visibleInModule = typeof(PMTask.visibleInGL);
					break;
				case BatchModule.AP:
					_visibleInModule = typeof(PMTask.visibleInAP);
					break;
				case BatchModule.AR:
					_visibleInModule = typeof(PMTask.visibleInAR);
					break;
				case BatchModule.SO:
					_visibleInModule = typeof(PMTask.visibleInSO);
					break;
				case BatchModule.PO:
					_visibleInModule = typeof(PMTask.visibleInPO);
					break;
				case BatchModule.IN:
					_visibleInModule = typeof(PMTask.visibleInIN);
					break;
				case BatchModule.CA:
					_visibleInModule = typeof(PMTask.visibleInCA);
					break;						
				case BatchModule.CR:
					_visibleInModule = typeof(PMTask.visibleInCR);
					break;
				default:
					throw new ArgumentOutOfRangeException("Module", Module, Messages.ProjectTaskAttributeNotSupport);
			}
			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(typeof(Where<,>), _visibleInModule, typeof(Equal<True>)), PXMessages.LocalizeFormatNoPrefixNLA(Messages.TaskInvisibleInModule, "{0}", PXMessages.LocalizeNoPrefix(module)), typeof(PMTask.taskCD)));
			_ModuleRestrictorAttrIndex = _Attributes.Count - 1;
		}

		public BaseProjectTaskAttribute(Type projectID, Type Module): this(projectID)
		{
			if (Module == null)
				throw new ArgumentNullException("Module");
			
            Type VisibleType =
				BqlCommand.Compose(
				typeof(Where<,,>),
				typeof(PMTask.visibleInGL),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,,>),
				typeof(Optional<>),
				Module,
				typeof(Equal<>),
				typeof(GL.BatchModule.moduleGL),
				typeof(Or<,,>),
				typeof(PMTask.visibleInAR),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,,>),
				typeof(Optional<>),
				Module,
				typeof(Equal<>),
				typeof(GL.BatchModule.moduleAR),
				typeof(Or<,,>),
				typeof(PMTask.visibleInAP),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,,>),
				typeof(Optional<>),
				Module,
				typeof(Equal<>),
				typeof(GL.BatchModule.moduleAP),
				typeof(Or<,,>),
				typeof(PMTask.visibleInCA),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,,>),
				typeof(Optional<>),
				Module,
				typeof(Equal<>),
				typeof(GL.BatchModule.moduleCA),
				typeof(Or<,,>),
				typeof(PMTask.visibleInIN),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,>),
				typeof(Optional<>),
				Module,
				typeof(Equal<>),
				typeof(GL.BatchModule.moduleIN)
				);

            _Attributes.Add(new PXRestrictorAttribute(typeof(Where<PMTask.isCompleted, NotEqual<True>>), Messages.CompletedTask, typeof(PMTask.taskCD)));
            _Attributes.Add(new PXRestrictorAttribute(VisibleType, Messages.InvisibleTask, typeof(PMTask.taskCD)));

		}

		#region IPXRowPersistingSubscriber Members

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				//rule: Task is mandatory only if Project is not an X project.
				int? taskID = (int?)sender.GetValue(e.Row, FieldOrdinal);
				Boolean? needsTaskValidation = true;
				if (this.NeedTaskValidationField != null)
					needsTaskValidation = (Boolean?)sender.GetValue(e.Row, this.NeedTaskValidationField.Name);

				int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
				PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, projectID);

				if (project != null && project.NonProject != true && taskID == null && project.BaseType == CT.CTPRType.Project && (needsTaskValidation == true))
				{
					if (sender.RaiseExceptionHandling(FieldName, e.Row, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, FieldName)))
					{
						throw new PXRowPersistingException(FieldName, null, Data.ErrorMessages.FieldIsEmpty, FieldName);
					}
				}
			}
		}

		#endregion

		#region IPXFieldVerifyingSubscriber Members
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null && e.NewValue is Int32)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.taskID>(sender.Graph, e.NewValue);
				if (task != null)
				{
					if (!AllowCompleted && task.IsCompleted == true && !CanPostToInactiveTask())
					{
						var ex = new PXTaskIsCompletedException(task.TaskID, PM.Messages.NoPermissionForInactiveTasks);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}

					if (!AllowCanceled && task.IsCancelled == true && !CanPostToInactiveTask())
					{
						var ex = new PXSetPropertyException(PM.Messages.NoPermissionForInactiveTasks);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}

					if (!AllowInactive && task.IsActive != true && !CanPostToInactiveTask())
					{
						var ex = new PXSetPropertyException(PM.Messages.NoPermissionForInactiveTasks);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
				}
			}
		}
		#endregion

		#region IPXFieldSelectingSubscriber Members

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
            PXFieldState ss = e.ReturnState as PXFieldState;
			Boolean? needsTaskValidation = true;
            if (ss != null  )
            {
				ss.Visible = ProjectAttribute.IsPMVisible( module);
				if (e.Row != null)
				{
					PMProject rec = (PMProject)PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectSingleBound(sender.Graph, null, sender.GetValue(e.Row, projectIDField.Name));

					if (this.NeedTaskValidationField != null)
						needsTaskValidation = (Boolean?)sender.GetValue(e.Row, this.NeedTaskValidationField.Name);
					ss.Enabled = (needsTaskValidation == true) && rec != null && rec.NonProject != true && rec.BaseType == CT.CTPRType.Project;
				}				
            }
		}
		#endregion

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(),projectIDField.Name, OnProjectUpdated);
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);
			if (_ModuleRestrictorAttrIndex != -1 && !string.IsNullOrEmpty(module))
			{
				PXRestrictorAttribute restrictor = (PXRestrictorAttribute) _Attributes[_ModuleRestrictorAttrIndex];
				restrictor.Message = PXMessages.LocalizeFormat(Messages.TaskInvisibleInModule, "{0}", PXUIFieldAttribute.GetDisplayName(sender.Graph.Caches[typeof(PMTask)], _visibleInModule.Name));
			}
		}

		protected PMTask GetDefaultTask(PXGraph graph, int? projectID)
		{
			if (projectID == null) return null;
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.isDefault, Equal<True>>>>.Select(graph, projectID);

			return task;
		}

		protected virtual void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{     
			object taskCD = (sender.GetValuePending(e.Row, _FieldName) as string) ?? (sender.GetValueExt(e.Row, _FieldName) as PXSegmentedState).Value;
			object projectID = sender.GetValue(e.Row, projectIDField.Name);

			if (taskCD != null && taskCD != PXCache.NotSetValue)
			{
				try
				{
					
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, projectID, taskCD);

					if (task != null && !ProjectDefaultAttribute.IsNonProject((int?)projectID))
					{
						sender.SetValueExt(e.Row, _FieldName, task.TaskID);
						object val = sender.GetValue(e.Row, _FieldName);
						if (task.TaskID != (int?)val)
						{
							PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
							sender.SetValueExt(e.Row, _FieldName, null);
						}
					}
					else
					{
						object pendingValue = null;
						if (ProjectDefaultAttribute.IsNonProject((int?)projectID))
						{
							object projectCD = sender.GetValuePending(e.Row, projectIDField.Name);
							if (projectCD != null && projectCD != PXCache.NotSetValue)
							{
								PMProject project = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Required<PMProject.contractCD>>>>.Select(sender.Graph, projectCD);
								if (project != null && project.NonProject != true)
								{
									task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, project.ContractID, taskCD);
									if (task != null)
									{
										pendingValue = task.TaskCD;
									}
								}
							}
							else if (projectCD == PXCache.NotSetValue)
							{
								object val = sender.GetValue(e.Row, _FieldName);
								if (val != null)
								{
									sender.SetValue(e.Row, _FieldName, null);
								}
							}
						}

						if (pendingValue == null)
						{
							sender.SetValuePending(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskCD);
						}
						else
						{
							sender.SetValuePending(e.Row, _FieldName, pendingValue);
						}
					}
				}
				catch(PXException ex)
				{
					if (sender.Graph.UnattendedMode)
					{
						throw ex;
					}
					else
					{
						sender.SetValuePending(e.Row, _FieldName, null);
					}
				}
			}
			else
			{
				sender.SetValue(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskID);
			}
		}

		protected virtual bool CanPostToInactiveTask()
		{
			return !string.IsNullOrEmpty(PredefinedRoles.ProjectAccountant)
					&& System.Web.Security.Roles.IsUserInRole(PXAccess.GetUserName(), PredefinedRoles.ProjectAccountant);
		}
	}

	public class PXTaskIsCompletedException : PXSetPropertyException
	{
		protected int? taskID;
		public PXTaskIsCompletedException(int? taskID):this(taskID, PM.Messages.ProjectTaskIsCompleted)
		{
		}

		public PXTaskIsCompletedException(int? taskID, string message) : base(message)
		{
			this.TaskID = taskID;
		}

		public PXTaskIsCompletedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context):base(info, context){ }

		public int? TaskID
		{
			get
			{
				return taskID;
			}

			protected set
			{
				taskID = value;
			}
		}
	}

	/// <summary>
	/// Task Selector that displays all active Tasks for the given Project. Task Field is Disabled if a Non-Project is selected; otherwise mandatory.
	/// Task Selector always work in pair with Project Selector. When the Project Selector displays a valid Project Task field becomes mandatory.
	/// If Completed Task is selected an error will be displayed - Completed Task cannot be used in DataEntry.
	/// 
	/// Task is mandatory only if the Freight amount is greater then zero.
	/// </summary>
	/// 
	public class SOFreightDetailTask : ActiveProjectTaskAttribute
	{
		protected Type curyTotalFreightAmtField;
		public SOFreightDetailTask(Type projectID, Type curyTotalFreightAmt):base(projectID, BatchModule.SO)
		{
			this.curyTotalFreightAmtField = curyTotalFreightAmt;
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				decimal? amt = (decimal?)sender.GetValue(e.Row, curyTotalFreightAmtField.Name);

				if (amt > 0)
					base.RowPersisting(sender, e);
			}
		}	
	}


	public class EPBaseAllowProjectTaskAttribute : AcctSubAttribute
	{
		protected Type projectIDField;
		protected string module;
	
		public EPBaseAllowProjectTaskAttribute(Type projectID)
		{
			if (projectID == null)
				throw new ArgumentNullException("projectID");

			projectIDField = projectID;

			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Optional<>),
				projectID
				);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectTaskAttribute.DimensionName, SearchType, typeof(PMTask.taskCD),
				typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status));
			select.DescriptionField = typeof(PMTask.description);
			select.ValidComboRequired = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}		

		#region IPXRowPersistingSubscriber Members

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			//rule: Task is mandatory only if Project is not an X project.
			int? taskID = (int?)sender.GetValue(e.Row, FieldOrdinal);

			int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
			PMProject contract = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(sender.Graph, projectID);

			if (contract != null && contract.NonProject != true && taskID == null && contract.BaseType == CT.CTPRType.Project)
			{
				if (sender.RaiseExceptionHandling(FieldName, e.Row, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, FieldName)))
				{
					throw new PXRowPersistingException(FieldName, null, Data.ErrorMessages.FieldIsEmpty, FieldName);
				}
			}

		}

		#endregion

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), projectIDField.Name, OnProjectUpdated);
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);
		}
		protected PMTask GetDefaultTask(PXGraph graph, int? projectID)
		{
			if (projectID == null) return null;
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.isDefault, Equal<True>>>>.Select(graph, projectID);

			return task;
		}

		protected virtual void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object taskCD = (sender.GetValuePending(e.Row, _FieldName) as string) ?? (sender.GetValueExt(e.Row, _FieldName) as PXSegmentedState).Value;
			object projectID = sender.GetValue(e.Row, projectIDField.Name);

			if (taskCD != null && taskCD != PXCache.NotSetValue)
			{
				try
				{

					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, projectID, taskCD);

					if (task != null && !ProjectDefaultAttribute.IsNonProject( (int?)projectID))
						sender.SetValueExt(e.Row, _FieldName, task.TaskID);
					else
						sender.SetValuePending(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskCD);
				}
				catch (PXException)
				{
					sender.SetValuePending(e.Row, _FieldName, null);
				}
			}
			else
			{
				sender.SetValue(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskID);
			}
		}
	}
	

	[PXDBInt()]
	[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.Visible)]
	public class EPExpenseAllowProjectTaskAttribute : EPBaseAllowProjectTaskAttribute, IPXFieldSelectingSubscriber
	{				
		public EPExpenseAllowProjectTaskAttribute(Type projectID, string Module)
			: base(projectID)
		{
			if (string.IsNullOrEmpty(Module))
				throw new ArgumentNullException("Module");

			module = Module;

			Type visibleInModule;
			switch (Module)
			{
				case BatchModule.EA:
					visibleInModule = typeof(PMTask.visibleInEA);
					break;
				default:
					throw new ArgumentOutOfRangeException("Module", Module, Messages.ProjectTaskAttributeNotSupport);
			}

			Filterable = true;
			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(typeof(Where<,>), visibleInModule, typeof(Equal<True>)), PXMessages.LocalizeFormatNoPrefixNLA(Messages.TaskInvisibleInModule, "{0}", PXMessages.LocalizeNoPrefix(module)), typeof(PMTask.taskCD)));
		}

		#region IPXFieldSelectingSubscriber Members

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXFieldState ss = e.ReturnState as PXFieldState;
			if (ss != null)
			{
				PMSetup setup = PXSelect<PMSetup>.Select(sender.Graph);

				ss.Visible = ProjectAttribute.IsPMVisible(module);
				if (e.Row != null)
				{
					PMProject rec = (PMProject)PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectSingleBound(sender.Graph, null, sender.GetValue(e.Row, projectIDField.Name));

					ss.Enabled = rec != null && rec.NonProject != true && rec.BaseType == CT.CTPRType.Project;
				}
			}
		}
		#endregion

	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.Visible)]
	// Restrictor is not applicable cause Verifying must be switch on/off in runtime (implemented in graph)
	//[PXRestrictor(typeof(Where<PMTask.isActive, Equal<True>, And<PMTask.isCancelled, NotEqual<True>>>), Messages.InactiveTask, typeof(PMTask.taskCD))]
	public class EPTimecardProjectTaskAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber //, IPXFieldVerifyingSubscriber - verifying is done in graph
	{
		readonly Type projectIDField;
		readonly string module;

		public bool AllowNull { get; set; }

		public EPTimecardProjectTaskAttribute(Type projectID)
		{
			if (projectID == null)
				throw new ArgumentNullException("projectID");

			projectIDField = projectID;

			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(PMTask.taskID),
				typeof(Where<,>),
				typeof(PMTask.projectID),
				typeof(Equal<>),
				typeof(Optional<>),
				projectID
				);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectTaskAttribute.DimensionName, SearchType, typeof(PMTask.taskCD),
				typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status));
			select.DescriptionField = typeof(PMTask.description);
			select.ValidComboRequired = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
			Filterable = true;
		}

		public EPTimecardProjectTaskAttribute(Type projectID, string Module)
			: this(projectID)
		{
			if (string.IsNullOrEmpty(Module))
				throw new ArgumentNullException("Module");

			module = Module;

			Type visibleInModule;
			switch (Module)
			{
				case BatchModule.GL:
					visibleInModule = typeof(PMTask.visibleInGL);
					break;
				case BatchModule.AP:
					visibleInModule = typeof(PMTask.visibleInAP);
					break;
				case BatchModule.AR:
					visibleInModule = typeof(PMTask.visibleInAR);
					break;
				case BatchModule.SO:
					visibleInModule = typeof(PMTask.visibleInSO);
					break;
				case BatchModule.PO:
					visibleInModule = typeof(PMTask.visibleInPO);
					break;
				case BatchModule.TA:
					visibleInModule = typeof(PMTask.visibleInTA);
					break;
				case BatchModule.IN:
					visibleInModule = typeof(PMTask.visibleInIN);
					break;
				case BatchModule.CA:
					visibleInModule = typeof(PMTask.visibleInCA);
					break;							
				case BatchModule.CR:
					visibleInModule = typeof(PMTask.visibleInCR);
					break;
				default:
					throw new ArgumentOutOfRangeException("Module", Module, Messages.ProjectTaskAttributeNotSupport);
			}

			Filterable = true;
			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(typeof(Where<,>), visibleInModule, typeof(Equal<True>)), PXMessages.LocalizeFormatNoPrefixNLA(Messages.TaskInvisibleInModule, "{0}", PXMessages.LocalizeNoPrefix(module)), typeof(PMTask.taskCD)));
		}
		
		#region IPXRowPersistingSubscriber Members

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				if (AllowNull) return;

				//rule: Task is mandatory only if Project is not an X project.
				int? taskID = (int?)sender.GetValue(e.Row, FieldOrdinal);

				int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
				PMProject contract = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(sender.Graph, projectID);

				if (contract != null && contract.NonProject != true && taskID == null && contract.BaseType == CT.CTPRType.Project)
				{
					if (sender.RaiseExceptionHandling(FieldName, e.Row, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, FieldName)))
					{
						throw new PXRowPersistingException(FieldName, null, Data.ErrorMessages.FieldIsEmpty, FieldName);
					}
				}
			}
		}

		#endregion

		#region IPXFieldSelectingSubscriber Members

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXFieldState ss = e.ReturnState as PXFieldState;
			if (ss != null)
			{
				ss.Visible = ProjectAttribute.IsPMVisible( module);
				if (e.Row != null)
				{
					PMProject project = PXSelectorAttribute.Select(sender, e.Row, projectIDField.Name) as PMProject;

					if (project == null)
					{
						//Can be that Selector Attribute is not defined for the given field. Fallback to slower method:
						int? projectID = (int?)sender.GetValue(e.Row, projectIDField.Name);
						project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, projectID);

					}
					
					ss.Enabled = project != null && project.NonProject != true && project.BaseType == CT.CTPRType.Project;
				}
			}
		}
		#endregion

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), projectIDField.Name, OnProjectUpdated);
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);
		}
		protected PMTask GetDefaultTask(PXGraph graph, int? projectID)
		{
			if (projectID == null) return null;
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.isDefault, Equal<True>>>>.Select(graph, projectID);

			return task;
		}

		protected virtual void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object taskCD = (sender.GetValuePending(e.Row, _FieldName) as string) ?? (sender.GetValueExt(e.Row, _FieldName) as PXSegmentedState).Value;
			object projectID = sender.GetValue(e.Row, projectIDField.Name);

			if (taskCD != null && taskCD != PXCache.NotSetValue)
			{
				try
				{
					

					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(sender.Graph, projectID, taskCD);

					if (task != null && !ProjectDefaultAttribute.IsNonProject((int?)projectID))
					{
						sender.SetValueExt(e.Row, _FieldName, task.TaskID);
						object val = sender.GetValue(e.Row, _FieldName);
						if (task.TaskID != (int?)val)
						{
							sender.SetValuePending(e.Row, _FieldName, null);
							sender.SetValue(e.Row, _FieldName, null);
						}
					}
					else
					{
						 sender.SetValuePending(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskCD);
					}

				}
				catch (PXException)
				{
					sender.SetValuePending(e.Row, _FieldName, null);
				}
			}
			else
			{
				sender.SetValue(e.Row, _FieldName, GetDefaultTask(sender.Graph, (int?)projectID)?.TaskID);
			}
		}
	}

	[PXRestrictor(typeof(Where<PMTask.isActive, Equal<True>, And<PMTask.isCancelled, NotEqual<True>>>), Messages.InactiveTask, typeof(PMTask.taskCD))]
	public class ActiveProjectTaskAttribute : BaseProjectTaskAttribute
	{
		public ActiveProjectTaskAttribute(Type projectID) : base(projectID)
		{
			Filterable = true;
		}
		public ActiveProjectTaskAttribute(Type projectID, string Module) : base(projectID, Module)
		{
			Filterable = true;
		}
		public ActiveProjectTaskAttribute(Type projectID, Type Module) : base(projectID, Module)
		{
			Filterable = true;
		}
	}

	[PXRestrictor(typeof(Where<PMTask.isCancelled, Equal<False>>), Messages.ProjectTaskIsCanceled, typeof(PMTask.taskCD))]
	[PXRestrictor(typeof(Where<PMTask.isCompleted, Equal<False>>), Messages.ProjectTaskIsCompleted, typeof(PMTask.taskCD))]
	public class ActiveOrInPlanningProjectTaskAttribute : BaseProjectTaskAttribute
	{
		public ActiveOrInPlanningProjectTaskAttribute(Type projectID) : base(projectID)
		{
			Filterable = true;
		}

		public ActiveOrInPlanningProjectTaskAttribute(Type projectID, string Module) : base(projectID, Module)
		{
			Filterable = true;
		}
	}
	#endregion

	public class PMAddressAttribute : AddressAttribute
	{
		protected Type customerID;

		/// <summary>
		/// Internaly, it expects PMAddress as a IAddress type.
		/// </summary>
		/// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
		/// a source Address record from which AR address is defaulted and for selecting default version of POAddress, <br/>
		/// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
		/// if it exists - so it must include both records. See example above. <br/>
		/// </param>
		public PMAddressAttribute(Type SelectType, Type customerID)
			: base(typeof(PMAddress.addressID), typeof(PMAddress.isDefaultBillAddress), SelectType)
		{
			this.customerID = customerID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PMAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PMAddress, PMAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (customerID != null)
			{
				int? val = (int?) sender.GetValue(e.Row, customerID.Name);
				if (val != null)
				{
					base.RowInserted(sender, e);
				}
			}		
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PMAddress, PMAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PMAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<PMAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class PMShippingAddressAttribute : AddressAttribute
	{
		protected Type customerID;

		public PMShippingAddressAttribute(Type SelectType, Type customerID)
			: base(typeof(PMShippingAddress.addressID), typeof(PMShippingAddress.isDefaultBillAddress), SelectType)
		{
			this.customerID = customerID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PMShippingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PMShippingAddress, PMShippingAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (customerID != null)
			{
				int? val = (int?)sender.GetValue(e.Row, customerID.Name);
				if (val != null)
				{
					base.RowInserted(sender, e);
				}
			}
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PMShippingAddress, PMShippingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PMShippingAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMShippingAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<PMShippingAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class PMContactAttribute : ContactAttribute
	{
		protected Type customerID;

		/// <summary>
		/// Ctor. Internaly, it expects PMContact as a IContact type
		/// </summary>
		/// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/>
		/// a source Contact record from which AR Contact is defaulted and for selecting version of PMContact, <br/>
		/// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
		/// - so it must include both records. See example above. <br/>
		/// </param>
		public PMContactAttribute(Type SelectType, Type customerID)
			: base(typeof(PMContact.contactID), typeof(PMContact.isDefaultContact), SelectType)
		{
			this.customerID = customerID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PMContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (customerID != null)
			{
				int? val = (int?)sender.GetValue(e.Row, customerID.Name);
				if (val != null)
				{
					base.RowInserted(sender, e);
				}
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<PMContact, PMContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<PMContact, PMContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<PMContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	public class PMShippingContactAttribute : ContactAttribute
	{
		protected Type customerID;

		public PMShippingContactAttribute(Type SelectType, Type customerID)
			: base(typeof(PMShippingContact.contactID), typeof(PMShippingContact.isDefaultContact), SelectType)
		{
			this.customerID = customerID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PMShippingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (customerID != null)
			{
				int? val = (int?)sender.GetValue(e.Row, customerID.Name);
				if (val != null)
				{
					base.RowInserted(sender, e);
				}
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<PMShippingContact, PMShippingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<PMShippingContact, PMShippingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<PMShippingContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMShippingContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	/// <summary>
	/// Defaults ProjectID to the Non-Project if the module supplied is either null or not not intergated with Project Management i.e. PMSetup.VisibleInXX = False.
	/// When Search is supplied ProjectID is defaulted with the value returned by that search.
	/// Selector also contains static Util methods.
	/// </summary>
	public class ProjectDefaultAttribute : PXDefaultAttribute
	{
		protected readonly string module;
		public Type AccountType {get; set;}

        /// <summary>
        /// Forces user to explicitly set the Project irrespective of the AccountType settings.
        /// </summary>
        public bool ForceProjectExplicitly { get; set; }

	    public ProjectDefaultAttribute()
			:this(null)
		{			
		}

		public ProjectDefaultAttribute(string module)
		{
			this.module = module;

		}
		
		public ProjectDefaultAttribute(string module, Type search) : this(module, search, null) { }
		public ProjectDefaultAttribute(string module, Type search, Type account)
			:base(search)
		{
			this.module = module;
			this.AccountType = account;
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);
			if (e.NewValue == null)
			{
				if (IsImporting(sender,e.Row) || IsDefaultNonProject(sender, e.Row) || !IsAccountGroupSpecified(sender, e.Row))
				{
					PMProject prj = PXSelect<PMProject, Where<PMProject.nonProject, Equal<True>>>.SelectSingleBound(sender.Graph, null, null);
					e.NewValue = prj != null ? prj.ContractCD : null;
				}
			}
			else
			{
				if (IsAccountGroupSpecified(sender, e.Row))
					e.NewValue = null;
			}

		}

		protected virtual bool IsDefaultNonProject(PXCache sender, object row)
		{
			return module == null || !ProjectAttribute.IsPMVisible( module);
		}

		protected  virtual bool IsImporting(PXCache sender, object row)
		{
			return sender.GetValuePending(row, PXImportAttribute.ImportFlag) != null;
		}

		/// <summary>
		/// When Account has no AccountGroup associated with it the only valid value for the Project is a Non-Project.
		/// </summary>
		protected bool IsAccountGroupSpecified(PXCache sender, object row)
		{
		    if (ForceProjectExplicitly)
		        return true;

			if (AccountType == null)
				return false;
			else
			{
				object accountID = sender.GetValue(row, AccountType.Name);

				if ( accountID == null )
				{
					return false;
				}
				else
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, accountID);
					if ( account == null )
						return false;
					else
						return account.AccountGroupID != null;
				}
			}			
		}


		/// <summary>
		/// Returns the Non-Project ID.
		/// Non-Project is stored in the table as a row with <see cref="PMProject.nonProject"/>=1.
		/// </summary>
		public static int? NonProject()
		{
			IProjectSettingsManager psm = ServiceLocator.Current.GetInstance<IProjectSettingsManager>();
			return psm.NonProjectID;
		}
				
		/// <summary>
		/// Returns true if the given ID is a Non-Project ID; oterwise false.
		/// </summary>
		public static bool IsNonProject(int? projectID)
		{
			return projectID == NonProject();
		}

		public static bool IsProject(PXGraph graph, int? projectID)
		{
			PMProject project;
			return IsProject(graph, projectID, out project);
		}

		public static bool IsProject(PXGraph graph, int? projectID, out PMProject project)
		{
			project = null;

			if (projectID == null)
				return false;

			if (projectID == NonProject())
				return false;

			PMProject rec = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectWindowed(graph, 0, 1, projectID);

			if (rec.BaseType != CT.CTPRType.Project)
				return false;
			else
				project = rec;
			
			return true;
		}

		public static bool IsBillableProject(PXGraph graph, int? projectID)
		{
			if (projectID == null)
				return false;

			PMProject rec = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectWindowed(graph, 0, 1, projectID);

			if (rec == null)
				return false;

			if (rec.BaseType != CT.CTPRType.Project)
				return false;

			if (rec.NonProject == true)
				return false;

			return rec.CustomerID != null;
		}
	}

	/// <summary>
	/// Project Default Attribute specific for PO Module. Defaulting of ProjectID field in PO depends on the LineType.
	/// If Line type is of type Non-Stock, Freight or Service ProjectID is defaulted depending on the setting in PMSetup (same as <see cref="ProjectDefaultAttribute"/>). 
	/// For all other type of lines Project is defaulted with Non-Project.
	/// </summary>
	public class POProjectDefaultAttribute : ProjectDefaultAttribute
	{
		protected readonly Type lineType;
		
		public POProjectDefaultAttribute(Type lineType)
			:base(BatchModule.PO)
		{
			this.lineType = lineType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(lineType, lineType.Name,
				(cache,e)=>
			 {
			  bool isDefault = IsDefaultNonProject(cache, e.Row);
				if (!isDefault && 
					IsNonProject((int?)cache.GetValuePending(e.Row,_FieldName)))
					cache.SetValuePending(e.Row, _FieldName, null);

				if(isDefault && cache.GetValuePending(e.Row, _FieldName) == null)
					 cache.SetDefaultExt(e.Row, _FieldName);
			});
		}

		protected override bool IsDefaultNonProject(PXCache sender, object row)
		{
			string poLineType = (string)sender.GetValue(row, lineType.Name);	
			switch (poLineType)
			{
				case PO.POLineType.NonStock:
				case PO.POLineType.Freight:
				case PO.POLineType.Service:
                    return base.IsDefaultNonProject(sender, row);

				default:
					return true;
			}			
		}

	}

    /// <summary>
    /// Project Default Attribute specific for GL Module. Defaulting of ProjectID field in GL depends on the Ledger type. 
    /// Budget and Report Ledgers do not require Project and hense it is always defaulted with Non-Project for these ledgers.
    /// </summary>
    public class GLProjectDefaultAttribute : ProjectDefaultAttribute
    {
        protected readonly Type ledgerType;
    	public GLProjectDefaultAttribute(Type ledgerType)
            : base(BatchModule.GL)
        {
            this.ledgerType = ledgerType;
		}

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldUpdated.AddHandler(ledgerType, ledgerType.Name,
                (cache, e) =>
                {
                    bool isDefault = IsDefaultNonProject(cache, e.Row);
                    if (!isDefault &&
                        IsNonProject((int?)cache.GetValuePending(e.Row, _FieldName)))
                        cache.SetValuePending(e.Row, _FieldName, null);
                    if (isDefault && cache.GetValuePending(e.Row, _FieldName) == null)
                        cache.SetDefaultExt(e.Row, _FieldName);
                });
		}

        protected override bool IsDefaultNonProject(PXCache sender, object row)
        {
            object ledgerID = sender.GetValue(row, ledgerType.Name);
            Ledger ledger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(sender.Graph, ledgerID);

			if (ledger != null && (ledger.BalanceType == LedgerBalanceType.Report || ledger.BalanceType == LedgerBalanceType.Budget))
			{
				return true;
			}
			else
				return base.IsDefaultNonProject(sender, row);

        }



    }

	/// <summary>
	/// Displays all AccountGroups sorted by SortOrder.
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.Visible)]
	public class AccountGroupAttribute : AcctSubAttribute
	{
		public const string DimensionName = "ACCGROUP";
		protected Type showGLAccountGroups;
			

		public AccountGroupAttribute():this(typeof(Where<PMAccountGroup.groupID, IsNotNull>))
		{	
		}

		public AccountGroupAttribute(Type WhereType)
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,,>),
				typeof(PMAccountGroup.groupID),
				WhereType,
				typeof(OrderBy<Asc<PMAccountGroup.sortOrder>>)
				);
			
			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(PMAccountGroup.groupCD),
				typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive));
			select.DescriptionField = typeof(PMAccountGroup.description);
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

	}


	/// <summary>
	/// Base attribute for AccountGroupCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.Visible)]
	public class AccountGroupRawAttribute : AcctSubAttribute
	{
		public AccountGroupRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(AccountGroupAttribute.DimensionName);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	/// <summary>
	/// Inventory Selector that allows to specify an OTHER InventoryID saving 0 in the table.
	/// </summary>  
    [Serializable]
	public class PMInventorySelectorAttribute : PXDimensionSelectorAttribute
    {
		#region State
		public const string DimensionName = "INVENTORY";
		#endregion

		#region Ctor
        public PMInventorySelectorAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public PMInventorySelectorAttribute(Type searchType)
		: this(searchType, typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}		

		public PMInventorySelectorAttribute(Type searchType, Type substituteKey, Type descriptionField)
			: base(DimensionName, searchType, substituteKey)
		{
			CacheGlobal = true;
			DescriptionField = descriptionField;
		}
		#endregion

        public const string EmptyComponentCD = "<N/A>";
        public static int EmptyInventoryID
        {
			get
            {
				IProjectSettingsManager psm = ServiceLocator.Current.GetInstance<IProjectSettingsManager>();
				return psm.EmptyInventoryID;
            }
            }
        }

   	/// <summary>
	/// Same as INUnit with support for Inventory=<NA> 
	/// </summary>
	[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
	[PXUIField(DisplayName = "UOM", Visibility = PXUIVisibility.Visible)]
	public class PMUnitAttribute : AcctSub2Attribute
            {
		public PMUnitAttribute(Type InventoryType)
			: base()
            {
			PMUnitSelectorAttrubute attr = new PMUnitSelectorAttrubute(InventoryType);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
            }
        }

	public class PMUnitSelectorAttrubute : PXCustomSelectorAttribute
        {
		protected Type inventory;
		public PMUnitSelectorAttrubute(Type inventory):base(typeof(INUnit.fromUnit))
            {
			this.inventory = inventory;
			
            }

		protected virtual IEnumerable GetRecords()
         {
			object current = null;
			if (PXView.Currents != null && PXView.Currents.Length > 0)
            {
				current = PXView.Currents[0];
            }
			else
			{
				current = _Graph.Caches[_CacheType].Current;
        }

			int? inventoryID = (int?)_Graph.Caches[_CacheType].GetValue(current, inventory.Name);
                       
			if (inventoryID == null || inventoryID == PMInventorySelectorAttribute.EmptyInventoryID)
        {
				var selectGloabalUnits = new PXSelectGroupBy<INUnit, 
					Where<INUnit.unitType, Equal<INUnitType.global>>, 
					Aggregate<GroupBy<INUnit.fromUnit>>>(this._Graph);
			
				return selectGloabalUnits.Select();
            }
            else
        {
				var selectItemUnits = new PXSelectGroupBy<INUnit,
					Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>>>,
					Aggregate<GroupBy<INUnit.fromUnit>>>(this._Graph);

				return selectItemUnits.Select(inventoryID);
            }
			
        }
    }



	/// <summary>
	/// Attribute for Subaccount field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class PMSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "PMSETUP";
		public PMSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, PMAcctSubDefault.MaskSource, new PMAcctSubDefault.SubListAttribute().AllowedValues, new PMAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new PMAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new PMAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}

			
		}
	}

	
	public class PMAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) {}
			public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) {}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute() : base(
				new[]
				{
					Pair(MaskSource, Messages.MaskSource),
					Pair(AllocationStep, Messages.AllocationStep),
					Pair(ProjectDefault, Messages.ProjectDefault),
					Pair(TaskDefault, Messages.TaskDefault),
				}) {}
		}

		public const string MaskSource = "S";
		public const string AllocationStep = "A";
		public const string ProjectDefault = "P";
		public const string TaskDefault = "T";
	}

	/// <summary>
	/// Attribute for Subaccount field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// Used in PM Billing to create a subaccount mask.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Sales Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName, Required = true)]
	public sealed class PMBillSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "PMBILL";
		public PMBillSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, AcctSubDefault.BillingRule, new AcctSubDefault.BillingSubListAttribute().AllowedValues, new AcctSubDefault.BillingSubListAttribute().AllowedLabels);

			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new AcctSubDefault.BillingSubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new AcctSubDefault.BillingSubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	[PXString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Sales Subaccount Mask", Visibility = PXUIVisibility.Visible, Visible = false, FieldClass = _DimensionName, Required = true)]
	public sealed class PMBillBudgetSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "PMBILLBUDGET";
		public PMBillBudgetSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, AcctSubDefault.BillingRule, new AcctSubDefault.BillingBudgetSubListAttribute().AllowedValues, new AcctSubDefault.BillingBudgetSubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new AcctSubDefault.BillingBudgetSubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new AcctSubDefault.BillingBudgetSubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class PMRecurentBillSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private string _MaskName = "PMRECBILL";
		public PMRecurentBillSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, AcctSubDefault.RecurentBilling, new AcctSubDefault.RecurentBillingSubListAttribute().AllowedValues, new AcctSubDefault.RecurentBillingSubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new AcctSubDefault.RecurentBillingSubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[fields[ex.SourceIdx].DeclaringType];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new AcctSubDefault.RecurentBillingSubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}
	
	
	/// <summary>
	/// Attribute for Subaccount field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// Used in PM Billing to create a subaccount mask for Expenses.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Combine Expense Sub. From", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "PMSETUP";
		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, AcctSubDefault.Inventory, new AcctSubDefault.SubListAttribute().AllowedValues, new AcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new AcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new AcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}


		}
	}

	public class AcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) { }
			public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute() : base(
				new[]
				{
					Pair(Inventory, Messages.AccountSource_InventoryItem),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
					Pair(Employee, Messages.AccountSource_Employee),
				}) {}
		}

		public class BillingSubListAttribute : CustomListAttribute
		{
			public BillingSubListAttribute() : base(
				new[]
				{
					Pair(BillingRule, Messages.AccountSource_BillingRule),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
					Pair(Employee, Messages.AccountSource_Employee),
					Pair(Source, Messages.AccountSource_SourceTransaction),
					Pair(Inventory, Messages.AccountSource_InventoryItem),
					Pair(Customer, Messages.AccountSource_Customer),
					Pair(Branch, Messages.AccountSource_Branch),
				}) {}
		}

		public class BillingBudgetSubListAttribute : CustomListAttribute
		{
			public BillingBudgetSubListAttribute()
				: base(new string[] { BillingRule, Project, Task, Inventory, Customer, Branch }, 
					  new string[] { Messages.AccountSource_BillingRule, Messages.AccountSource_Project, Messages.AccountSource_Task,
									Messages.AccountSource_InventoryItem, Messages.AccountSource_Customer, Messages.AccountSource_Branch})
			{
			}
		}

		public class RecurentBillingSubListAttribute : CustomListAttribute
		{
			public RecurentBillingSubListAttribute() : base(
				new[]
				{
					Pair(RecurentBilling, Messages.AccountSource_RecurentBillingItem),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
				}) {}
		}

		public const string Source = "S";
		public const string BillingRule = "B";
		public const string RecurentBilling = "B";
		public const string Inventory = "I";
		public const string Customer = "C";
		public const string Project = "P";
		public const string Task = "T";
		public const string Employee = "E";
		public const string Branch = "R";
	}

	public class PMBudgetAccumAttribute : PXAccumulatorAttribute
	{
		public PMBudgetAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMBudget item = (PMBudget)row;
			columns.Update<PMBudget.curyDraftChangeOrderAmount>(item.CuryDraftChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.draftChangeOrderAmount>(item.DraftChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.draftChangeOrderQty>(item.DraftChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyChangeOrderAmount>(item.CuryChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.changeOrderAmount>(item.ChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.changeOrderQty>(item.ChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyInvoicedAmount>(item.CuryInvoicedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.invoicedAmount>(item.InvoicedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.actualQty>(item.ActualQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyActualAmount>(item.CuryActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.actualAmount>(item.ActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedQty>(item.CommittedQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyCommittedAmount>(item.CuryCommittedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedAmount>(item.CommittedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedOrigQty>(item.CommittedOrigQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyCommittedOrigAmount>(item.CuryCommittedOrigAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedOrigAmount>(item.CommittedOrigAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedCOQty>(item.CommittedCOQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyCommittedCOAmount>(item.CuryCommittedCOAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedCOAmount>(item.CommittedCOAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedOpenQty>(item.CommittedOpenQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyCommittedOpenAmount>(item.CuryCommittedOpenAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedOpenAmount>(item.CommittedOpenAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedReceivedQty>(item.CommittedReceivedQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedInvoicedQty>(item.CommittedInvoicedQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyCommittedInvoicedAmount>(item.CuryCommittedInvoicedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.committedInvoicedAmount>(item.CommittedInvoicedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyRetainedAmount>(item.CuryRetainedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.retainedAmount>(item.RetainedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.curyDraftRetainedAmount>(item.CuryDraftRetainedAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMBudget.draftRetainedAmount>(item.DraftRetainedAmount, PXDataFieldAssign.AssignBehavior.Summarize);

			columns.Update<PMBudget.description>(item.Description, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<PMBudget.type>(item.Type, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<PMBudget.uOM>(item.UOM, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<PMBudget.curyUnitRate>(item.CuryUnitRate, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<PMBudget.rate>(item.Rate, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<PMBudget.curyInfoID>(item.CuryInfoID, PXDataFieldAssign.AssignBehavior.Initialize);

			return true;
		}

		
	}

	[PXDBInt]
	[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
	public class PMAccountAttribute : AcctSubAttribute
	{		
		public PMAccountAttribute()
		{
			/*
			 
			SEARCH Account.accountID
			WHERE 
			Account.active = True
			AND 
			(
			  (
				( Current_PMAccountGroup.type = AccountType.asset OR Current_PMAccountGroup.type = AccountType.liability )
				AND 
				( Account.type = AccountType.asset OR Account.type = AccountType.liability )
			  )
			  OR
			  (
				( Current_PMAccountGroup.type = AccountType.expense OR Current_PMAccountGroup.type = AccountType.income )
				AND 
				( Account.type = AccountType.expense OR Account.type = AccountType.income )
			  )
			)
			 
			 */

			Type SearchType = typeof(Search<Account.accountID,
				Where2<Match<Current<AccessInfo.userName>>,
				And<Account.active, Equal<boolTrue>,
				And2<
					Where2<Where<Current<PMAccountGroup.type>, Equal<AccountType.asset>,
							Or<Current<PMAccountGroup.type>, Equal<AccountType.liability>>>,
						And<Where<Account.type, Equal<AccountType.asset>,
							Or<Account.type, Equal<AccountType.liability>>>>>,
					Or2<Where<Current<PMAccountGroup.type>, Equal<AccountType.expense>,
						And<Account.type, In3<AccountType.expense, AccountType.income, AccountType.asset, AccountType.liability>>>,
					Or<Where<Current<PMAccountGroup.type>, Equal<AccountType.income>>>
						>>>>>);

			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(AccountAttribute.DimensionName, SearchType, typeof(Account.accountCD));
			attr.CacheGlobal = true;
			attr.DescriptionField = typeof(Account.description);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}
	}

    public class PMRecurringItemAccumAttribute : PXAccumulatorAttribute
    {
        public PMRecurringItemAccumAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;

			PMRecurringItemAccum item = (PMRecurringItemAccum)row;
            columns.Update<PMRecurringItemAccum.used>(item.Used, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<PMRecurringItemAccum.usedTotal>(item.UsedTotal, PXDataFieldAssign.AssignBehavior.Summarize);

            return true;
        }
    }

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class GroupTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Project, Messages.GroupTypes_Project),
					Pair(Task, Messages.GroupTypes_Task),
					Pair(AccountGroup, Messages.GroupTypes_AccountGroup),
					Pair(Equipment, Messages.GroupTypes_Equipment),
				}) {}
		}

		public const string Project = "PROJECT";
		public const string Task = "TASK";
		public const string AccountGroup = "ACCGROUP";
		public const string Transaction = "PROTRAN";
		public const string Equipment = "EQUIPMENT";

		public class ProjectType : PX.Data.BQL.BqlString.Constant<ProjectType>
		{
			public ProjectType() : base(GroupTypes.Project) { ;}
		}

		public class TaskType : PX.Data.BQL.BqlString.Constant<TaskType>
		{
			public TaskType() : base(GroupTypes.Task) { ;}
		}

		public class AccountGroupType : PX.Data.BQL.BqlString.Constant<AccountGroupType>
		{
			public AccountGroupType() : base(GroupTypes.AccountGroup) { ;}
		}

		public class TransactionType : PX.Data.BQL.BqlString.Constant<TransactionType>
		{
			public TransactionType() : base(GroupTypes.Transaction) { ;}
		}

		public class EquipmentType : PX.Data.BQL.BqlString.Constant<EquipmentType>
		{
			public EquipmentType() : base(GroupTypes.Equipment) { ;}
				
			
		}
	}

	#region projectType
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public sealed class projectType : PX.Data.BQL.BqlString.Constant<projectType>
	{
		public projectType()
			: base(typeof(PMProject).FullName)
		{
		}
	}
	#endregion

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMBillingFormat
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Detail, Messages.BillingFormat_Detail),
					Pair(Summary, Messages.BillingFormat_Summary),
					Pair(Progress, Messages.BillingFormat_Progress),
				})
			{ }
		}

		public const string Summary = "S";
		public const string Detail = "D";
		public const string Progress = "P";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class BudgetControlOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base
			(
				new string[] { Nothing, Warn },
				new string[] { Messages.BudgetControlOption_Nothing, Messages.BudgetControlOption_Warn }
			)
			{ }
		}

		public const string Nothing = "N";
		public const string Warn = "W";
	}
}
