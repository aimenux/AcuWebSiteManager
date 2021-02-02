using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	public abstract class CrewTimeEntryGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		public PXSelect<EPTimeActivitiesSummary> ActivitiesSummary;
		public PXSelect<EPWeeklyCrewTimeActivity> CrewActivitiesDocument;
	}
}
