using System;
using Autofac;
using PX.Data;
using PX.Data.EP;
using PX.Data.RelativeDates;
using PX.Objects.GL.FinPeriods;
using PX.Objects.SM;
using PX.Objects.CM.Extensions;
using PX.Objects.FA;

namespace PX.Objects
{
    public class ServiceRegistration: Module
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

		}
	}
}
