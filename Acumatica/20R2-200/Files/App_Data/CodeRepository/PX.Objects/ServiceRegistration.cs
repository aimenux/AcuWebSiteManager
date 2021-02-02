using System;
using Autofac;
using PX.Data;
using PX.Data.EP;
using PX.Data.RelativeDates;
using PX.Objects.GL.FinPeriods;
using PX.Objects.SM;
using PX.Objects.CM.Extensions;
using PX.Objects.EndpointAdapters;
using PX.Objects.FA;
using PX.Objects.PM;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Data.Search;
using PX.Objects.AP.InvoiceRecognition;

namespace PX.Objects
{
	public class ServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
			.RegisterType<FinancialPeriodManager>()
			.As<IFinancialPeriodManager>();

			builder
				.RegisterType<TodayBusinessDate>()
				.As<ITodayUtc>();

			builder.RegisterType<EP.NotificationProvider>()
				.As<INotificationSender>()
				.SingleInstance()
				.PreserveExistingDefaults();

			builder.RegisterType<EP.NotificationProvider>()
				.As<INotificationSenderWithActivityLink>()
				.SingleInstance()
				.PreserveExistingDefaults();

			builder
				.RegisterType<FinPeriodRepository>()
				.As<IFinPeriodRepository>();

			builder
				.RegisterType<FinPeriodUtils>()
				.As<IFinPeriodUtils>();

			builder
				.Register<Func<PXGraph, IPXCurrencyService>>(context
					=>
					{
						return (graph)
						=>
						{
							return new DatabaseCurrencyService(graph);
						};
					});

			builder
				.RegisterType<FABookPeriodRepository>()
				.As<IFABookPeriodRepository>();

			builder
				.RegisterType<FABookPeriodUtils>()
				.As<IFABookPeriodUtils>();

			builder
				.RegisterType<BudgetService>()
				.As<IBudgetService>();

			builder
				.RegisterType<UnitRateService>()
				.As<IUnitRateService>();

			builder
				.RegisterType<PM.ProjectSettingsManager>()
				.As<PM.IProjectSettingsManager>();

			builder
				.RegisterType<PM.CostCodeManager>()
				.As<PM.ICostCodeManager>();

			builder
				.RegisterType<PM.ProjectSettingsManager>()
				.As<PM.IProjectSettingsManager>();

			builder.RegisterType<DefaultEndpointImplCR>().AsSelf();
			builder.RegisterType<DefaultEndpointImplCR20>().AsSelf();
			builder.RegisterType<DefaultEndpointImplWorkFlow>().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.CaseApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.OpportunityApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.LeadApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.SalesOrderApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.ShipmentApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.SalesInvoiceApplicator>().SingleInstance().AsSelf();

			builder
				.RegisterType<CN.Common.Services.NumberingSequenceUsage>()
				.As<CN.Common.Services.INumberingSequenceUsage>();

			builder
				.RegisterType<AdvancedAuthenticationRestrictor>()
				.As<IAdvancedAuthenticationRestrictor>()
				.SingleInstance();

			builder
				.RegisterType<PXEntitySearchEnriched>()
				.As<IEntitySearchService>();

			builder
				.RegisterType<APInvoiceEmailProcessor>()
				.SingleInstance()
				.ActivateOnApplicationStart(EmailProcessorManager.Register);
		}
	}
}
