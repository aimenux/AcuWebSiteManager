using System;
using System.Diagnostics;
using System.IO;

namespace PX.Objects.AM
{
    /// <summary>
    /// Helper class to work with Acumatica ERP version number
    /// </summary>
    public static class ERPVersionHelper
    {
        public static readonly FileVersionInfo VersionInfo;
        public static readonly int UpdateVersion;
        public static readonly bool IsPreRelease;

        static ERPVersionHelper()
        {
            VersionInfo = GetFileVersionInfo("PX.Data.dll");
            var minorV = VersionInfo?.ProductMinorPart ?? 0;
            UpdateVersion = GetERPUpdateNumber(minorV);
            IsPreRelease = IsPreReleaseVersion(minorV);
#if DEBUG
            AMDebug.TraceWriteMethodName($"{VersionInfo?.ProductVersion ?? "***V Info Note Found***"}; UpdateVersion={UpdateVersion}; IsPreRelease={IsPreRelease}");
#endif
        }

        public static int GetERPUpdateNumber(int minorVersion)
        {
            var u = StripRVersion(minorVersion);

            // Preview build could be 091 (R1) or 191 (R2) for first preview
            if (u.BetweenInclusive(90, 99))
            {
                return u - 90;
            }

            return u;
        }

        public static bool IsPreReleaseVersion(int minorVersion)
        {
            var u = StripRVersion(minorVersion);
            return u.BetweenInclusive(90, 99);
        }

        /// <summary>
        /// Strip the ERP R* version from the value
        /// </summary>
        /// <returns></returns>
        public static int StripRVersion(int minorVersion)
        {
            if (minorVersion < 0 || minorVersion >= 1000)
            {
                return -1;
            }

            var m100 = minorVersion / 100;
            var u = minorVersion - m100 * 100;

            return u < 0 ? -1 : u;
        }

        private static FileVersionInfo GetFileVersionInfo(string dllFileName)
        {
            if (string.IsNullOrWhiteSpace(dllFileName))
            {
                return null;
            }

            try
            {
                //Here we just want to first load the PX.Data.dll
                var baseDir = GetGetExecutingAssemblyPath();
                if (string.IsNullOrWhiteSpace(baseDir))
                {
                    return null;
                }

                var fullFileName = Path.Combine(baseDir, dllFileName);
                if (!File.Exists(fullFileName))
                {
                    return null;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Getting FileVersionInfo for {fullFileName}");
#endif
                return FileVersionInfo.GetVersionInfo(fullFileName);
            }
#if DEBUG
            catch (Exception e)
            {
                AMDebug.TraceWriteMethodName(e.Message);
            }
#else
            catch {}
#endif

            return null;
        }

        private static string GetGetExecutingAssemblyPath()
        {
            var loc = AppDomain.CurrentDomain.BaseDirectory;
            return string.IsNullOrWhiteSpace(loc) ? null : Path.Combine(loc, "Bin");
        }
    }
}