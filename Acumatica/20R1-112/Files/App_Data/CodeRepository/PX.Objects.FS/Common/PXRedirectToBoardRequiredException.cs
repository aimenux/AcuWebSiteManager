using PX.Data;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.FS
{
    public class PXRedirectToBoardRequiredException : PXRedirectToUrlException
    {
        private static string BuildUrl(string baseBoardUrl, KeyValuePair<string, string>[] args)
        {
            StringBuilder boardUrl = new StringBuilder(@"~\");
            boardUrl.Append(baseBoardUrl);

            if (args != null && args.Length > 0)
            {
                boardUrl.Append("?");

                KeyValuePair<string, string> kvp;

                for (int i = 0; i < args.Length; i++)
                {
                    kvp = args[i];
                    boardUrl.Append(kvp.Key);
                    boardUrl.Append("=");
                    boardUrl.Append(kvp.Value);

                    if (i != args.Length - 1)
                    {
                        boardUrl.Append("&");
                    }
                }
            }

            return boardUrl.ToString();
        }

        public PXRedirectToBoardRequiredException(string baseBoardUrl, KeyValuePair<string, string>[] parameters, WindowMode windowMode = WindowMode.NewWindow, bool supressFrameset = true)
            : base(BuildUrl(baseBoardUrl, parameters), windowMode, supressFrameset, null)
        {
        }
    }
}
