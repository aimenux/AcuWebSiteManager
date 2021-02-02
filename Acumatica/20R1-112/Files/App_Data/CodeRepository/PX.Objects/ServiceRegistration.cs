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
				.RegisterType<PM.CostCodeManager>()
				.As<PM.ICostCodeManager>();

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
			builder.RegisterType<CbApiWorkflowApplicator.CaseApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.OpportunityApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.LeadApplicator>().SingleInstance().AsSelf();

			builder
				.RegisterType<CN.Common.Services.NumberingSequenceUsage>()
				.As<CN.Common.Services.INumberingSequenceUsage>();

			builder
				.RegisterType<AdvancedAuthenticationRestrictor>()
				.As<IAdvancedAuthenticationRestrictor>()
				.SingleInstance();
		}
	}
}
