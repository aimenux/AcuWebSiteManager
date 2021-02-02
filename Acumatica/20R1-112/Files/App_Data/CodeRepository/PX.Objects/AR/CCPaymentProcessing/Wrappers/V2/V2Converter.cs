using System;
using PX.Data;
using AutoMapper;
using PX.Objects.AR.CCPaymentProcessing.Common;
using V2 = PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class V2Converter
	{
		private static readonly IMapper Mapper;

		static V2Converter()
		{
			MapperConfiguration mapperConf = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<CCTranType, V2.CCTranType>()
				.AfterMap((i, v2) => {
					if (!Enum.IsDefined(v2.GetType(), v2))
					{
						throw new PXException(CCProcessingBase.Messages.UnexpectedTranType, i);
					}
				});
				cfg.CreateMap<V2.CCTranType, CCTranType>()
				.AfterMap((v2, o) => {
					if (!Enum.IsDefined(o.GetType(), o))
					{
						throw new PXException(CCProcessingBase.Messages.UnexpectedTranType, v2);
					}
				});
				cfg.CreateMap<V2.SettingsControlType, int?>()
				.ConvertUsing((V2.SettingsControlType i) => {
					switch (i)
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
				});
				cfg.CreateMap<V2.SettingsDetail, PluginSettingDetail>()
				.ForMember(dst => dst.Value, opts => opts.MapFrom(src => src.DefaultValue))
				.ForMember(dst => dst.ComboValuesCollection, opts => { opts.AllowNull(); opts.MapFrom(src => src.ComboValues); });
			});
			Mapper = mapperConf.CreateMapper();
		}

		public static CCTranType ConvertTranType(V2.CCTranType v2TranType)
		{
			return Mapper.Map<CCTranType>(v2TranType);
		}

		public static HostedFormResponse ConvertHostedFormResponse(V2.HostedFormResponse v2FormResponse)
		{
			return Mapper.Map<HostedFormResponse>(v2FormResponse);
		}

		public static V2.CCTranType ConvertTranTypeToV2(CCTranType tranType)
		{
			return Mapper.Map<V2.CCTranType>(tranType); 
		}

		public static V2.SettingsValue ConvertSettingDetailToV2(PluginSettingDetail detail)
		{
			return Mapper.Map<V2.SettingsValue>(detail);
		}

		public static TranProcessingResult ConvertTranProcessingResult(V2.ProcessingResult processingResult)
		{
			if (processingResult == null) throw new ArgumentNullException(nameof(processingResult));

			TranProcessingResult result = new TranProcessingResult()
			{
				AuthorizationNbr = processingResult.AuthorizationNbr,
				CcvVerificatonStatus = ConvertCvvStatus(processingResult.CcvVerificatonStatus),
				ExpireAfterDays = processingResult.ExpireAfterDays,
				Success = true,
				PCResponse = processingResult.ResponseText,
				PCResponseCode = processingResult.ResponseCode,
				PCResponseReasonCode = processingResult.ResponseReasonCode,
				PCResponseReasonText = processingResult.ResponseReasonText,
				PCTranNumber = processingResult.TransactionNumber,
				ResultFlag = CCResultFlag.None,
				TranStatus = CCTranStatus.Approved
			};
			return result;
		}

		public static PluginSettingDetail ConvertSettingsDetail(V2.SettingsDetail detail)
		{
			return Mapper.Map<PluginSettingDetail>(detail);
		}

		public static int? ConvertSettingsControlType(V2.SettingsControlType controlType)
		{
			return Mapper.Map<int?>(controlType);
		}

		public static CcvVerificationStatus ConvertCardVerificationStatus(V2.CcvVerificationStatus status)
		{
			CcvVerificationStatus ret = Mapper.Map<CcvVerificationStatus>(status);
			if (!Enum.IsDefined(typeof(CcvVerificationStatus), ret))
			{
				ret = CcvVerificationStatus.Unknown;
			}
			return ret;
		}

		public static CCTranStatus ConvertTranStatus(V2.CCTranStatus status)
		{
			CCTranStatus ret = Mapper.Map<CCTranStatus>(status);
			if (!Enum.IsDefined(typeof(CCTranStatus), ret))
			{
				ret = CCTranStatus.Unknown;
			}
			return ret;
		}

		public static CcvVerificationStatus ConvertCvvStatus(V2.CcvVerificationStatus status)
		{
			return Mapper.Map<CcvVerificationStatus>(status);
		}
	}
}
