using System;
using System.Collections.Generic;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class V1ProcessingDTOGenerator
	{
		public static V1.ProcessingResult GetProcessingResult(V2.ProcessingResult inputResult)
		{
			if (inputResult == null) throw new ArgumentNullException(nameof(inputResult));

			V1.ProcessingResult result = new V1.ProcessingResult()
			{
				AuthorizationNbr = inputResult.AuthorizationNbr,
				CcvVerificatonStatus = ToV1(inputResult.CcvVerificatonStatus),
				ExpireAfterDays = inputResult.ExpireAfterDays,
				isAuthorized = true,
				PCResponse = inputResult.ResponseText,
				PCResponseCode = inputResult.ResponseCode,
				PCResponseReasonCode = inputResult.ResponseReasonCode,
				PCResponseReasonText = inputResult.ResponseReasonText,
				PCTranNumber = inputResult.TransactionNumber,
				ResultFlag = V1.CCResultFlag.None,
				TranStatus = V1.CCTranStatus.Approved
			};
			return result;
		}

		public static V1.ProcessingResult GetProcessingResult(string error)
		{
			//assuming this is called only for V2.CCErrorSource.PaymentGateway
			V1.ProcessingResult result = new V1.ProcessingResult()
			{
				isAuthorized = false,
				ErrorSource = V1.CCErrors.CCErrorSource.ProcessingCenter,
				ErrorText = error
			};
			return result;
		}

		public static void ApiResponseSetSuccess(V1.APIResponse apiResponse)
		{
			apiResponse.isSucess = true;
			apiResponse.ErrorSource = V1.CCErrors.CCErrorSource.None;
			apiResponse.Messages = null;
		}

		public static void ApiResponseSetError(V1.APIResponse apiResponse, V2.CCProcessingException e)
		{
			apiResponse.ErrorSource = V1.CCErrors.CCErrorSource.ProcessingCenter;
			apiResponse.isSucess = false;
			apiResponse.Messages["Exception"] = e.Message;
		}

		public static V2.SettingsValue ToV2(V1.ISettingsDetail detail)
		{
			V2.SettingsValue result = new V2.SettingsValue()
			{
				DetailID = detail.DetailID,
				Value = detail.Value
			};
			return result;
		}

		public static V1.CCErrors GetCCErrors(string v2Result)
		{
			V1.CCErrors result = new V1.CCErrors()
			{
				source = string.IsNullOrEmpty(v2Result) ? V1.CCErrors.CCErrorSource.None : V1.CCErrors.CCErrorSource.Internal,
				ErrorMessage = v2Result
			};
			return result;
		}

		public static void FillV1Settings(IList<V1.ISettingsDetail> v1List, IEnumerable<V2.SettingsDetail> v2List)
		{
			foreach (var v2Setting in v2List)
			{
				var v1Setting = new PluginSettingDetail()
				{
					DetailID = v2Setting.DetailID,
					Descr = v2Setting.Descr,
					Value = v2Setting.DefaultValue,
					IsEncryptionRequired = v2Setting.IsEncryptionRequired,
					ControlType = ToV1(v2Setting.ControlType)
				};
				List<KeyValuePair<string, string>> comboList = null;
				if (v2Setting.ComboValues != null)
				{
					comboList = new List<KeyValuePair<string, string>>(v2Setting.ComboValues);
				}
				v1Setting.SetComboValues(comboList);
				v1List.Add(v1Setting);
			}
		}

		public static int? ToV1(V2.SettingsControlType controlType)
		{
			switch (controlType)
			{
				case V2.SettingsControlType.CheckBox:
					return 3;
				case V2.SettingsControlType.Combo:
					return 2;
				case V2.SettingsControlType.Text:
					return 1;
				default:
					return null;
			}
		}

		public static V1.CCTranType ToV1(V2.CCTranType tranTypeV2)
		{
			switch (tranTypeV2)
			{
				case V2.CCTranType.AuthorizeAndCapture:
					return V1.CCTranType.AuthorizeAndCapture;
				case V2.CCTranType.AuthorizeOnly:
					return V1.CCTranType.AuthorizeOnly;
				case V2.CCTranType.PriorAuthorizedCapture:
					return V1.CCTranType.PriorAuthorizedCapture;
				case V2.CCTranType.CaptureOnly:
					return V1.CCTranType.CaptureOnly;
				case V2.CCTranType.Credit:
					return V1.CCTranType.Credit;
				case V2.CCTranType.Void:
					return V1.CCTranType.Void;
				case V2.CCTranType.VoidOrCredit:
					return V1.CCTranType.VoidOrCredit;
				default:
					throw new PXException(CCProcessingBase.Messages.UnexpectedTranType, tranTypeV2);
			}
		}

		public static V1.CCTranStatus ToV1(V2.CCTranStatus status)
		{
			V1.CCTranStatus v1result = V1.CCTranStatus.Unknown;
			switch (status)
			{
				case V2.CCTranStatus.Approved:
					v1result = V1.CCTranStatus.Approved;
					break;
				case V2.CCTranStatus.Declined:
					v1result = V1.CCTranStatus.Declined;
					break;
				case V2.CCTranStatus.Error:
					v1result = V1.CCTranStatus.Error;
					break;
				case V2.CCTranStatus.HeldForReview:
					v1result = V1.CCTranStatus.HeldForReview;
					break;
				default:
					v1result = V1.CCTranStatus.Unknown;
					break;
			}
			return v1result; 
		}

		public static V1.CcvVerificationStatus ToV1(V2.CcvVerificationStatus status)
		{
			V1.CcvVerificationStatus v1result;
			switch (status)
			{
				case V2.CcvVerificationStatus.Match:
					v1result = V1.CcvVerificationStatus.Match;
					break;
				case V2.CcvVerificationStatus.NotMatch:
					v1result = V1.CcvVerificationStatus.NotMatch;
					break;
				case V2.CcvVerificationStatus.NotProcessed:
					v1result = V1.CcvVerificationStatus.NotProcessed;
					break;
				case V2.CcvVerificationStatus.ShouldHaveBeenPresent:
					v1result = V1.CcvVerificationStatus.ShouldHaveBeenPresent;
					break;
				case V2.CcvVerificationStatus.IssuerUnableToProcessRequest:
					v1result = V1.CcvVerificationStatus.IssuerUnableToProcessRequest;
					break;
				case V2.CcvVerificationStatus.RelyOnPreviousVerification:
					v1result = V1.CcvVerificationStatus.RelyOnPreviousVerification;
					break;
				case V2.CcvVerificationStatus.Unknown:
				default:
					v1result = V1.CcvVerificationStatus.Unknown;
					break;
			}
			return v1result;
		}
	}
}
