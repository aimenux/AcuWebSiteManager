using Newtonsoft.Json;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class PRGovernmentReportingProcess : PXGraph<PRGovernmentReportingProcess>
	{
		public const string SessionIdSeparator = "----";

		public override bool IsDirty => false;

		private static HttpClient _HttpClient = null;

		static PRGovernmentReportingProcess()
		{
			_HttpClient = new HttpClient();
			_HttpClient.BaseAddress = new Uri(AatrixConfiguration.WebformsUrl);
			_HttpClient.DefaultRequestHeaders.Accept.Clear();
			_HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		#region Views
		public PXFilter<PRGovernmentReportingFilter> Filter;

		public SelectFrom<PRGovernmentReport>
			.OrderBy<PRGovernmentReport.state.Asc, PRGovernmentReport.formDisplayName.Asc>.View.ReadOnly Reports;
		public SelectFrom<PRGovernmentReport>.View CurrentReport;
		#endregion

		#region Actions
		public PXCancel<PRGovernmentReportingFilter> Cancel;

		public PXAction<PRGovernmentReportingFilter> ViewHistory;
		[PXUIField(DisplayName = "View History")]
		[PXButton]
		public virtual void viewHistory() { }

		public PXAction<PRGovernmentReportingFilter> RunReport;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual void runReport()
		{
			CurrentReport.AskExt();	
		}

		public PXAction<PRGovernmentReportingFilter> OnRunReportError;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable onRunReportError(PXAdapter adapter)
		{
			switch (adapter.CommandArguments)
			{
				case RunReportProcessingError.EinMissing:
					throw new PXException(Messages.AatrixReportEinMissing);

				case RunReportProcessingError.AatrixVendorIDMissing:
					throw new PXException(Messages.AatrixReportAatrixVendorIDMissing);

				case RunReportProcessingError.YearMissing:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.year>(
						CurrentReport.Current,
						null,
						new PXSetPropertyException(Messages.CantBeEmpty, PXErrorLevel.Error, Messages.Year));
					break;

				case RunReportProcessingError.QuarterMissing:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.quarter>(
						CurrentReport.Current,
						null,
						new PXSetPropertyException(Messages.CantBeEmpty, PXErrorLevel.Error, Messages.Quarter));
					break;

				case RunReportProcessingError.MonthMissing:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.month>(
						CurrentReport.Current,
						null,
						new PXSetPropertyException(Messages.CantBeEmpty, PXErrorLevel.Error, Messages.Month));
					break;

				case RunReportProcessingError.DateFromMissing:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.dateFrom>(
						CurrentReport.Current,
						null,
						new PXSetPropertyException(Messages.CantBeEmpty, PXErrorLevel.Error, Messages.DateFrom));
					break;

				case RunReportProcessingError.DateToMissing:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.dateTo>(
						CurrentReport.Current,
						null,
						new PXSetPropertyException(Messages.CantBeEmpty, PXErrorLevel.Error, Messages.DateTo));
					break;

				case RunReportProcessingError.DateInconsistent:
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.dateFrom>(
						CurrentReport.Current,
						CurrentReport.Current.DateFrom,
						new PXSetPropertyException(Messages.InconsistentDateRange, PXErrorLevel.Error));
					CurrentReport.Cache.RaiseExceptionHandling<PRGovernmentReport.dateTo>(
						CurrentReport.Current,
						CurrentReport.Current.DateTo,
						new PXSetPropertyException(Messages.InconsistentDateRange, PXErrorLevel.Error));
					break;

				case RunReportProcessingError.Exception:
				default:
					throw new PXException(Messages.AatrixReportProcessingError);
			}

			return adapter.Get();
		}

		public PXAction<PRGovernmentReportingFilter> StartAufGeneration;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable startAufGeneration(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, delegate ()
			{
				PRAufGenerationEngine.GenerateAuf(Filter.Current.OrgBAccountID.Value, CurrentReport.Current);
			});

			return adapter.Get();
		}

		public PXAction<PRGovernmentReportingFilter> OnUploadPermissionGranted;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable onUploadPermissionGranted(PXAdapter adapter)
		{
			string[] arguments = adapter.CommandArguments.Split(new string[] { SessionIdSeparator }, StringSplitOptions.RemoveEmptyEntries);
			string aatrixSessionID = arguments[0];

			if (arguments.Length > 1 && bool.TryParse(arguments[1], out bool downloadAuf))
			{
				Filter.Current.DownloadAuf = downloadAuf;
			}
			else
			{
				Filter.Current.DownloadAuf = null;
			}

			MultipartFormDataContent aatrixContent = GetContentForAatrixUpload(aatrixSessionID, out PXRedirectToFileException fileDownloadRedirect);
			if (aatrixContent != null)
			{
				PostAsync(
					AatrixConfiguration.GetEndpoint(AatrixOperation.UploadAuf),
					aatrixContent).Wait(AatrixConfiguration.TransactionTimeoutMs);
			}

			if (fileDownloadRedirect != null)
			{
				throw fileDownloadRedirect;
			}

			return adapter.Get();
		}

		public PXAction<PRGovernmentReportingFilter> OnNavigateAway;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable onNavigateAway(PXAdapter adapter)
		{
			PXLongOperation.AsyncAbort(this.UID);

			return adapter.Get();
		}
		#endregion Actions

		#region View delegates
		protected virtual IEnumerable reports()
		{
			if (!Reports.Cache.Inserted.Any_())
			{
				Task<AatrixAvailableForms> forms = GetAvailableFormsList();
				if (forms.Wait(AatrixConfiguration.TransactionTimeoutMs))
				{
					foreach (AatrixAvailableForms.AatrixForm form in forms.Result.Data)
					{
						Reports.Cache.Insert(new PRGovernmentReport()
						{
							FormName = form.FormName,
							FormDisplayName = form.DisplayName,
							Description = form.Description,
							State = form.State == "FE" ? LocationConstants.FederalStateCode : form.State,
							ReportingPeriod = GovernmentReportingPeriod.FromAatrixData(form.ReportPeriod),
							ReportType = form.FormType
						});
					}
				}
				else
				{
					throw new PXException(Messages.AatrixOperationTimeout, AatrixConfiguration.TransactionTimeoutMs);
				}
			}

			var reportList = new List<PRGovernmentReport>();
			foreach (PRGovernmentReport report in Reports.Cache.Inserted)
			{
				if ((Filter.Current.FederalOnly == false && (Filter.Current.State == null || Filter.Current.State == report.State) ||
					(Filter.Current.FederalOnly == true && report.State == LocationConstants.FederalStateCode)) &&
					(Filter.Current.ReportingPeriod == null || Filter.Current.ReportingPeriod == report.ReportingPeriod))
				{
					reportList.Add(report);
				}
			}

			return reportList;
		}

		protected virtual IEnumerable currentReport()
		{
			return new List<PRGovernmentReport>() { Reports.Current };
		}
		#endregion View delegates

		#region Helpers
		private async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent body)
		{
			HttpResponseMessage response = await _HttpClient.PostAsync(endpoint, body).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return response;
		}

		private async Task<AatrixAvailableForms> GetAvailableFormsList()
		{
			var data = new Dictionary<string, object>();
			data.Add("VendorCode", AatrixConfiguration.VendorID);
			HttpContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			HttpResponseMessage response = await PostAsync(
				AatrixConfiguration.GetEndpoint(AatrixOperation.GetAvailableFormsList),
				content).ConfigureAwait(false);

			string aatrixResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<AatrixAvailableForms>(aatrixResponse);
		}

		private MultipartFormDataContent GetContentForAatrixUpload(string aatrixSessionID, out PXRedirectToFileException fileDownloadRedirect)
		{
			fileDownloadRedirect = null;
			byte[] aufFileBytes = GenerateAufFile();
			if (aufFileBytes == null)
			{
				return null;
			}

			if (Filter.Current.DownloadAuf == true)
			{
				fileDownloadRedirect = new PXRedirectToFileException(new PX.SM.FileInfo(PRFileNames.Auf, null, aufFileBytes), true);
			}

			MultipartFormDataContent form = new MultipartFormDataContent();
			form.Add(new ByteArrayContent(aufFileBytes, 0, aufFileBytes.Length), "file", "aufFile");
			form.Add(new StringContent(CurrentReport.Current.FormName), "FormName");
			form.Add(new StringContent(aatrixSessionID), "WfSessionId");

			return form;
		}

		private byte[] GenerateAufFile()
		{ 
			PXLongOperation.WaitCompletion(this.UID);
			object longOperationReturn = PXLongOperation.GetCustomInfo(this.UID);

			if (longOperationReturn is PXException)
			{
				throw (PXException)longOperationReturn;
			}

			return longOperationReturn as byte[];
		}
		#endregion Helpers

		private class AatrixAvailableForms
		{
			public class AatrixForm
			{
				public string FormName { get; set; }
				public string DisplayName { get; set; }
				public string State { get; set; }
				public string FormType { get; set; }
				public string Signature { get; set; }
				public bool IsPaymentForm { get; set; }
				public string ReportPeriod { get; set; }
				public string DataBreakout { get; set; }
				public string Description { get; set; }
				public bool IsSupported { get; set; }
			}

			public IEnumerable<AatrixForm> Data { get; set; }
			public string Exception { get; set; }
			public string FriendlyErrorMessage { get; set; }
			public object MetaData { get; set; }
		}
	}

	public class RunReportProcessingError
	{
		public const string SetupError = "SetupError";
		public const string EinMissing = "EinMissing";
		public const string AatrixVendorIDMissing = "AatrixVendorIDMissing";
		public const string YearMissing = "YearMissing";
		public const string QuarterMissing = "QuarterMissing";
		public const string MonthMissing = "MonthMissing";
		public const string DateFromMissing = "DateFromMissing";
		public const string DateToMissing = "DateToMissing";
		public const string DateInconsistent = "DateInconsistent";

		// Unknown error
		public const string Exception = "Exception";
	}
}
