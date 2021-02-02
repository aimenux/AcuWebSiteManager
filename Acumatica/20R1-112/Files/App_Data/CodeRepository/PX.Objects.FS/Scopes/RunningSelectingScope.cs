using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RunningSelectingScope<DAC> : IDisposable
        where DAC : IBqlTable
    {
        protected List<string> _GraphList;
        protected string _MyGraphSelecting;
        protected readonly RunningSelectingScope<DAC> _Previous;

        public RunningSelectingScope(PXGraph myGraph)
        {
            _MyGraphSelecting = myGraph.GetType().FullName;

            _Previous = PXContext.GetSlot<RunningSelectingScope<DAC>>();
            if (_Previous == null)
            {
                _GraphList = new List<string>();
            }
            else
            {
                _GraphList = new List<string>(_Previous._GraphList);
            }

            _GraphList.Add(_MyGraphSelecting);

            PXContext.SetSlot<RunningSelectingScope<DAC>>(this);
        }
        public void Dispose()
        {
            PXContext.SetSlot<RunningSelectingScope<DAC>>(_Previous);
        }
        public static bool IsRunningSelecting(PXGraph graph)
        {
            RunningSelectingScope<DAC> scope = PXContext.GetSlot<RunningSelectingScope<DAC>>();
            if (scope == null)
            {
                return false;
            }
            else
            {
                return scope._GraphList.Exists(e => e == graph.GetType().FullName);
            }
        }
    }
}
