using Customization;
using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    internal static class UpgradeHelper
    {
        public static void WriteInfo(string infoMsg, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(infoMsg))
            {
                return;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName(infoMsg, args);
#endif
            PXTrace.WriteInformation(infoMsg, args);
        }

        public static void WriteInfo(CustomizationPlugin plugin, string infoMsg, params object[] args)
        {
            if (plugin != null && !string.IsNullOrWhiteSpace(infoMsg))
            {
                WriteCstInfoOnly(plugin, infoMsg, args);
                return;
            }
            WriteInfo(infoMsg, args);
        }

        public static void WriteCstInfoOnly(CustomizationPlugin plugin, string infoMsg, params object[] args)
        {
#if DEBUG
            if (!string.IsNullOrWhiteSpace(infoMsg))
            {
                AMDebug.TraceWriteMethodName(infoMsg, args);
            }
#endif
            if (plugin != null && !string.IsNullOrWhiteSpace(infoMsg))
            {
                plugin.WriteLog(string.Format(infoMsg, args));
            }
        }
    }
}