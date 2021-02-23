using System.Collections.Generic;
using Lib.Models;

namespace Lib.Helpers
{
    public interface IConsoleHelper
    {
        void RenderTitle(string text);
        void RenderFile(string filepath);
        void RenderTable(Request request);
        void RenderTable(ICollection<SiteDetails> details);
    }
}
