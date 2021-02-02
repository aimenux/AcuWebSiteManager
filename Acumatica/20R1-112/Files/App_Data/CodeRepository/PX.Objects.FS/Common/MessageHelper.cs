using PX.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.FS
{
    public static class MessageHelper
    {
        public class ErrorInfo
        {
            public int? SOID;
            public int? AppointmentID;
            public string ErrorMessage;
            public bool HeaderError;
        }

        public static int GetRowMessages(PXCache cache, object row, List<string> errors, List<string> warnings, bool includeRowInfo)
        {
            if (cache == null || row == null)
            {
                return 0;
            }

            int errorCount = 0;
            PXFieldState fieldState;

            foreach (string field in cache.Fields)
            {
                try
                {
                    fieldState = (PXFieldState)cache.GetStateExt(row, field);
                }
                catch
                {
                    fieldState = null;
                }

                if (fieldState != null && fieldState.Error != null)
                {
                    if (errors != null)
                    {
                        if (fieldState.ErrorLevel != PXErrorLevel.RowWarning
                            && fieldState.ErrorLevel != PXErrorLevel.Warning
                            && fieldState.ErrorLevel != PXErrorLevel.RowInfo
                        )
                        {
                            errors.Add(fieldState.Error);
                            errorCount++;
                        }
                    }

                    if (warnings != null)
                    {
                        if (fieldState.ErrorLevel == PXErrorLevel.RowWarning
                            || fieldState.ErrorLevel == PXErrorLevel.Warning
                            || (fieldState.ErrorLevel == PXErrorLevel.RowInfo && includeRowInfo == true)
                        )
                        {
                            warnings.Add(fieldState.Error);
                        }
                    }
                }
            }

            return errorCount;
        }

        public static string GetRowMessage(PXCache cache, IBqlTable row, bool getErrors, bool getWarnings)
        {
            List<string> errors = null;
            List<string> warnings = null;

            if (getErrors)
            {
                errors = new List<string>();
            }
            if (getWarnings)
            {
                warnings = new List<string>();
            }

            GetRowMessages(cache, row, errors, warnings, false);

            StringBuilder messageBuilder = new StringBuilder();

            if (errors != null)
            {
                foreach (string message in errors)
                {
                    if (messageBuilder.Length > 0)
                    {
                        messageBuilder.Append(Environment.NewLine);
                    }

                    messageBuilder.Append(message);
                }
            }

            if (warnings != null)
            {
                foreach (string message in warnings)
                {
                    if (messageBuilder.Length > 0)
                    {
                        messageBuilder.Append(Environment.NewLine);
                    }

                    messageBuilder.Append(message);
                }
            }

            return messageBuilder.ToString();
        }

        public static List<ErrorInfo> GetErrorInfo<TranType, TranExtensionType>(PXCache headerCache, IBqlTable headerRow, PXSelectBase<TranType> detailView)
            where TranType : class, IBqlTable, new()
            where TranExtensionType : PXCacheExtension<TranType>, IPostDocLineExtension
        {
            List<ErrorInfo> errorList = new List<ErrorInfo>();
            ErrorInfo errorInfo = null;

            string headerErrorMessage = MessageHelper.GetRowMessage(headerCache, headerRow, true, false);

            if (string.IsNullOrEmpty(headerErrorMessage) == false)
            {
                errorInfo = new ErrorInfo()
                {
                    HeaderError = true,
                    SOID = null,
                    AppointmentID = null,
                    ErrorMessage = headerErrorMessage
                };

                errorList.Add(errorInfo);
            }

            foreach (TranType row in detailView.Select())
            {
                string errorMessage = MessageHelper.GetRowMessage(detailView.Cache, row, true, false);

                if (string.IsNullOrEmpty(errorMessage) == false)
                {
                    TranExtensionType rowExtension = detailView.Cache.GetExtension<TranExtensionType>(row);

                    errorInfo = new ErrorInfo()
                    {
                        HeaderError = false,
                        SOID = rowExtension.SOID,
                        AppointmentID = rowExtension.AppointmentID,
                        ErrorMessage = errorMessage + ", "
                    };

                    errorList.Add(errorInfo);
                }
            }

            return errorList;
        }

        public static List<ErrorInfo> GetErrorInfo<TranType>(PXCache headerCache, IBqlTable headerRow, PXSelectBase<TranType> detailView)
            where TranType : class, IBqlTable, new()
        {
            List<ErrorInfo> errorList = new List<ErrorInfo>();
            ErrorInfo errorInfo = null;

            string headerErrorMessage = MessageHelper.GetRowMessage(headerCache, headerRow, true, false);

            if (string.IsNullOrEmpty(headerErrorMessage) == false)
            {
                errorInfo = new ErrorInfo()
                {
                    HeaderError = true,
                    SOID = null,
                    AppointmentID = null,
                    ErrorMessage = headerErrorMessage
                };

                errorList.Add(errorInfo);
            }

            foreach (TranType row in detailView.Select())
            {
                string errorMessage = MessageHelper.GetRowMessage(detailView.Cache, row, true, false);

                if (string.IsNullOrEmpty(errorMessage) == false)
                {
                    errorInfo = new ErrorInfo()
                    {
                        HeaderError = false,
                        SOID = null,
                        AppointmentID = null,
                        ErrorMessage = errorMessage + ", "
                    };

                    errorList.Add(errorInfo);
                }
            }

            return errorList;
        }
    }

    public static class StringExtensionMethods
    {
        public static string EnsureEndsWithDot(this string str)
        {
            if (str == null) return string.Empty;

            str = str.Trim();

            if (str == string.Empty) return str;

            if (!str.EndsWith(".")) return str + ".";

            return str;
        }
    }
}
