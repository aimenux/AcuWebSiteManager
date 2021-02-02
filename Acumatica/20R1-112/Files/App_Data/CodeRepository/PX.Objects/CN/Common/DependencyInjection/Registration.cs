using Autofac;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.CL.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.Creators;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using ProjectDataProvider = PX.Objects.CN.Common.Services.DataProviders.ProjectDataProvider;

namespace PX.Objects.CN.Common.DependencyInjection
{
	public class Registration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);
			builder.RegisterType<CacheService>().As<ICacheService>();
			builder.RegisterType<LienWaiverCreator>().As<ILienWaiverCreator>();
			builder.RegisterType<ComplianceAttributeTypeDataProvider>().As<IComplianceAttributeTypeDataProvider>();
			builder.RegisterType<LienWaiverDataProvider>().As<ILienWaiverDataProvider>();
			builder.RegisterType<LienWaiverTransactionRetriever>().As<ILienWaiverTransactionRetriever>();
			builder.RegisterType<LienWaiverTypeDeterminator>().As<ILienWaiverTypeDeterminator>();
			builder.RegisterType<LienWaiverConfirmationService>().As<ILienWaiverConfirmationService>();
			builder.RegisterType<ProjectDataProvider>().As<IProjectDataProvider>();
			builder.RegisterType<ProjectTaskDataProvider>().As<IProjectTaskDataProvider>();
			builder.RegisterType<BusinessAccountDataProvider>().As<IBusinessAccountDataProvider>();
			builder.RegisterType<LienWaiverGenerationKeyCreator>().As<ILienWaiverGenerationKeyCreator>();
			builder.RegisterType<LienWaiverJointPayeesProvider>().As<ILienWaiverJointPayeesProvider>();
			builder.RegisterType<LienWaiverTransactionsProvider>().As<ILienWaiverTransactionsProvider>();
			builder.RegisterType<LienWaiverReportCreator>().As<ILienWaiverReportCreator>();
			builder.RegisterType<PrintEmailLienWaiverBaseService>().As<IPrintEmailLienWaiverBaseService>();
			builder.RegisterType<PrintLienWaiversService>().As<IPrintLienWaiversService>();
			builder.RegisterType<EmailLienWaiverService>().As<IEmailLienWaiverService>();
			builder.RegisterType<RecipientEmailDataProvider>().As<IRecipientEmailDataProvider>();
			builder.RegisterType<EmployeeDataProvider>().As<IEmployeeDataProvider>();
		}
	}
}