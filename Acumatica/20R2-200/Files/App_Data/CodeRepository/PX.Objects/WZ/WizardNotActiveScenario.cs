using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using PX.Data;

namespace PX.Objects.WZ
{
    public class WizardNotActiveScenario : PXGraph<WizardNotActiveScenario>
    {
        public PXSelect<WZScenario> Scenario;
        
    }
}
