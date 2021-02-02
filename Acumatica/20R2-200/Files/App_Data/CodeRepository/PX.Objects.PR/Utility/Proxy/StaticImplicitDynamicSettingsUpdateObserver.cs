using PX.Api.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
    public class PRStateObserver : IStaticImplicitDynamicSettingsUpdateObserver
    {
        private PRStateObserver() { }

        public void DynamicSettingsUpdated()
        {
            PRState.InitializeStateList();
        }
    }
}
