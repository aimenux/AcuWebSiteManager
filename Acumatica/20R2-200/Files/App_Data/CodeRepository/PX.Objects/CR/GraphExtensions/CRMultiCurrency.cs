using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;

namespace PX.Objects.Extensions.MultiCurrency.CR
{
    public abstract class CRMultiCurrencyGraph<TGraph, TPrimary> : MultiCurrencyGraph<TGraph, TPrimary>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
    {
        protected override CurySourceMapping GetCurySourceMapping()
        {
            return new CurySourceMapping(typeof(Customer));
        }

        public PXSelect<CRSetup> crCurrency;
        protected PXSelectExtension<CurySource> SourceSetup => new PXSelectExtension<CurySource>(crCurrency);

        protected virtual CurySourceMapping GetSourceSetupMapping()
        {
            return new CurySourceMapping(typeof(CRSetup)) { CuryID = typeof(CRSetup.defaultCuryID), CuryRateTypeID = typeof(CRSetup.defaultRateTypeID) };
        }

        protected override CurySource CurrentSourceSelect()
        {
            CurySource settings = base.CurrentSourceSelect();
            if (settings == null)
                return SourceSetup.Select();
            if (settings.CuryID == null || settings.CuryRateTypeID == null)
            {
                CurySource setup = SourceSetup.Select();
                settings = (CurySource)CurySource.Cache.CreateCopy(settings);
                settings.CuryID = settings.CuryID ?? setup?.CuryID;
                settings.CuryRateTypeID = settings.CuryRateTypeID ?? setup?.CuryRateTypeID;
            }
            return settings;
        }

		protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.moduleCode> e)
		{
			e.NewValue = "CR";
		}
	}
}
