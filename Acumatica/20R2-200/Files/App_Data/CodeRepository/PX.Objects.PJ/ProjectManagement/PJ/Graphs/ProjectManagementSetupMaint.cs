using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Infrastructure;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.AccuWeatherModels;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.WeatherBitModels;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Services;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Api;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
	public class ProjectManagementSetupMaint : PXGraph<ProjectManagementSetupMaint>
	{
		public PXSave<ProjectManagementSetup> Save;
		public PXCancel<ProjectManagementSetup> Cancel;

		public SelectFrom<ProjectManagementSetup>.View ProjectManagementSetup;
		public SelectFrom<ProjectIssueType>.View ProjectIssueTypes;
		public SelectFrom<DailyFieldReportCopyConfiguration>.View DailyFieldReportCopyConfiguration;
		public SelectFrom<WeatherIntegrationSetup>.View WeatherIntegrationSetup;
		public SelectFrom<PJSubmittalType>.View SubmittalTypes;

		public PXAction<WeatherIntegrationSetup> TestConnection;

		[PXButton]
		[PXUIField(DisplayName = "Test Connection")]
		public virtual IEnumerable testConnection(PXAdapter adapter)
		{
			var weatherIntegrationSetup = WeatherIntegrationSetup.Current;
			if (weatherIntegrationSetup.WeatherApiService == null)
			{
				WeatherIntegrationSetup.Cache.RaiseException<WeatherIntegrationSetup.weatherApiService>(
					weatherIntegrationSetup, WeatherIntegrationMessages.WeatherApiServiceMustBeSelected);
			}
			else
			{
				Persist();
				PXLongOperation.StartOperation(this, SendTestRequest);
			}
			return adapter.Get();
		}

		public virtual void _(Events.RowSelected<ProjectManagementSetup> args)
		{
			if (args.Row is ProjectManagementSetup projectManagementSetup)
			{
				PXUIFieldAttribute.SetEnabled<ProjectManagementSetup.calendarId>(args.Cache,
					args.Row, DoesCalculationTypeEqualBusinessDays(projectManagementSetup));
			}
		}

		public virtual void _(Events.RowPersisting<ProjectManagementSetup> args)
		{
			if (args.Row is ProjectManagementSetup projectManagementSetup)
			{
				ValidateCalendar(args.Cache, projectManagementSetup);
			}
		}

		public virtual void _(Events.RowInserted<ProjectManagementSetup> args)
		{
			DailyFieldReportCopyConfiguration.Insert(new DailyFieldReportCopyConfiguration());
			WeatherIntegrationSetup.Insert(new WeatherIntegrationSetup());
		}

		public virtual void _(Events.RowDeleting<ProjectIssueType> args)
		{
			if (args.Row is ProjectIssueType projectIssueType &&
				IsProjectIssueTypeInUse(projectIssueType.ProjectIssueTypeId))
			{
				throw new Exception(ProjectManagementMessages.ValueUsedInProjectIssue);
			}
		}

		public virtual void _(Events.FieldUpdated<DailyFieldReportCopyConfiguration,
			DailyFieldReportCopyConfiguration.isConfigurationEnabled> args)
		{
			var configuration = args.Row;
			if (configuration != null && !(bool)args.NewValue)
			{
				var cache = args.Cache;
				foreach (var field in cache.BqlFields)
				{
					cache.RaiseFieldDefaulting(field.Name, configuration, out var defaultValue);
					cache.SetValue(configuration, field.Name, defaultValue);
				}
			}
		}

		public virtual void _(Events.FieldUpdated<WeatherIntegrationSetup,
			WeatherIntegrationSetup.weatherApiService> args)
		{
			args.Cache.SetValueExt<WeatherIntegrationSetup.weatherApiKey>(args.Row, string.Empty);
			args.Cache.SetValueExt<WeatherIntegrationSetup.requestParametersType>(args.Row, string.Empty);
		}

		public virtual void _(Events.RowSelected<WeatherIntegrationSetup> args)
		{
			var weatherSetting = args.Row;
			if (weatherSetting == null)
			{
				return;
			}
			var isConfigurationEnabled = weatherSetting.IsConfigurationEnabled.GetValueOrDefault();
			PXUIFieldAttribute.SetEnabled(args.Cache, weatherSetting, isConfigurationEnabled);
			TestConnection.SetEnabled(isConfigurationEnabled);
			PXUIFieldAttribute.SetEnabled<WeatherIntegrationSetup.isConfigurationEnabled>(args.Cache, weatherSetting);
		}

		public virtual void _(Events.RowSelected<PJSubmittalType> e)
		{
			object row = e.Row;
			PXCache cache = e.Cache;
			PXUIFieldAttribute.SetEnabled(cache, row, cache.GetStatus(row) == PXEntryStatus.Inserted);
		}

		public virtual void _(Events.FieldUpdated<WeatherIntegrationSetup,
			WeatherIntegrationSetup.isWeatherProcessingLogEnabled> args)
		{
			args.Row.WeatherProcessingLogTerm =
				WeatherIntegrationConstants.WeatherIntegrationSetup.DefaultWeatherProcessingLogTerm;
		}

		public virtual void _(Events.RowPersisted<WeatherIntegrationSetup> args)
		{
			var weatherSetting = args.Row;
			if (weatherSetting == null)
			{
				return;
			}
			UpdateAutomationSchedule(weatherSetting.IsWeatherProcessingLogEnabled);
		}

		private void UpdateAutomationSchedule(bool? isWeatherProcessingLogEnabled)
		{
			var schedule = new AutomationScheduleDataProvider(this)
				.GetAutomationSchedule(ScreenIds.ClearDfrWeatherProcessingLog);
			schedule.IsActive = isWeatherProcessingLogEnabled;
			if (isWeatherProcessingLogEnabled == true)
			{
				schedule.NextRunDate = Accessinfo.BusinessDate;
			}
			var cache = this.Caches<AUSchedule>();
			cache.Update(schedule);
			cache.PersistUpdated(schedule);
		}

		private static void ValidateCalendar(PXCache cache,
			ProjectManagementSetup projectManagementSetup)
		{
			if (DoesCalculationTypeEqualBusinessDays(projectManagementSetup) &&
				projectManagementSetup.CalendarId.IsNullOrEmpty())
			{
				var message = string.Format(SharedMessages.FieldIsEmpty, ProjectManagementLabels.CalendarId);
				cache.RaiseExceptionHandling<ProjectManagementSetup.calendarId>(projectManagementSetup, null,
					new PXSetPropertyException(message));
			}
		}

		private static bool DoesCalculationTypeEqualBusinessDays(ProjectManagementSetup projectManagementSetup)
		{
			return projectManagementSetup.AnswerDaysCalculationType ==
				AnswerDaysCalculationTypeAttribute.BusinessDays;
		}

		private void SendTestRequest()
		{
			var baseUrl = GetBaseUrl();
			var request = new Request(baseUrl);
			switch (WeatherIntegrationSetup.Current.WeatherApiService)
			{
				case WeatherApiService.OpenWeatherMap:
					request.Execute<OpenWeatherMapResponse>();
					break;
				case WeatherApiService.WeatherBit:
					request.Execute<WeatherBitResponse>();
					break;
				default:
					request.Execute<List<AccuWeatherResponse>>();
					break;
			}
		}

		private string GetBaseUrl()
		{
			string baseUrl;
			switch (WeatherIntegrationSetup.Current.WeatherApiService)
			{
				case WeatherApiService.OpenWeatherMap:
					baseUrl = WeatherIntegrationConstants.TestConnectionUrls.OpenWeatherMap;
					break;
				case WeatherApiService.WeatherBit:
					baseUrl = WeatherIntegrationConstants.TestConnectionUrls.WeatherBit;
					break;
				default:
					baseUrl = WeatherIntegrationConstants.TestConnectionUrls.AccuWeather;
					break;
			}

			return string.Format(baseUrl, WeatherIntegrationSetup.Current.WeatherApiKey);
		}

		private bool IsProjectIssueTypeInUse(int? projectIssueTypeId)
		{
			return SelectFrom<ProjectIssue>
				.Where<ProjectIssue.projectIssueTypeId.IsEqual<P.AsInt>>.View.Select(this, projectIssueTypeId).Any();
		}
	}
}