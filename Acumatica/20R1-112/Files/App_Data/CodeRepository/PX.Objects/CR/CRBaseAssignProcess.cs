using System;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.MassProcess;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public abstract class CRBaseMassProcess<TGraph, TPrimary> : PXGraph<TGraph>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new()
		where TPrimary : class, IBqlTable, new()
	{
		public PXCancel<TPrimary> Cancel;
		private readonly PXPrimaryGraphCollection primaryGraph;

		protected CRBaseMassProcess()
		{
			primaryGraph = new PXPrimaryGraphCollection(this);
			if (ProcessingDataMember == null)
				throw new PXException(Messages.IsNotProcessing, typeof(TGraph).FullName);

			foreach (Type table in ProcessingDataMember.View.BqlSelect.GetTables())
			{
				PXDBAttributeAttribute.Activate(Caches[table]);
			}

			ProcessingDataMember.SetParametersDelegate(delegate
				{
					bool result = AskParameters();
					Unload();
					TGraph process;
					using (new PXPreserveScope())
					{
						process = CreateInstance<TGraph>();
						foreach (var key in this.Caches.Keys)
						{
							var clonedCache = process.Caches[key];
						}
					}
					
					ProcessingDataMember.SetProcessDelegate(item =>
						{
							PXGraph graph = GetPrimaryGraph(item);
							if (graph == null)
								throw new PXException(ErrorMessages.CantDetermineGraphType);
                            process.ProccessItem(graph, item);
							graph.Actions.PressSave();
							PXCache cache = graph.Caches[typeof(TPrimary)];
							cache.RestoreCopy(item, cache.Current);					
						});
					return result;
				});

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(Caches[typeof(BAccount)], Messages.BAccountCD);
		}

		protected virtual PXGraph GetPrimaryGraph(TPrimary item)
		{
			return primaryGraph[item];
		}

		private PXProcessing<TPrimary> _items;
		public PXProcessing<TPrimary> ProcessingDataMember 
		{
			get
			{
				return _items ?? (_items = (typeof(TGraph).GetFields(BindingFlags.Public | BindingFlags.Instance)
														 .Where(
															 field =>
															 typeof(PXProcessing<TPrimary>).IsAssignableFrom(field.FieldType))
														 .Select(field => (PXProcessing<TPrimary>)field.GetValue(this)))
					.FirstOrDefault()); 
			}
		}

		public abstract void ProccessItem(PXGraph graph, TPrimary item);
		protected virtual bool AskParameters()
		{
			return true;
		}
	}
	
	public abstract class CRBaseAssignProcess<TGraph, TPrimary, TAssignmentMapField> : CRBaseMassProcess<TGraph, TPrimary>, IMassProcess<TPrimary>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new() 
		where TPrimary : class, IBqlTable, IAssign, new() 
		where TAssignmentMapField : IBqlField
	{
		protected EPAssignmentProcessor<TPrimary> processor;

		protected CRBaseAssignProcess()
		{
			processor = CreateInstance<EPAssignmentProcessor<TPrimary>>();
		}

		public override void ProccessItem(PXGraph graph, TPrimary item)
		{
			int? assingmentMapId = GetAssignmentMapId(graph);
			if (assingmentMapId == null)
				throw new PXException(Messages.AssignmentMapIdEmpty);

			PXCache cache = graph.Caches[typeof(TPrimary)];
		    if (!cache.GetItemType().IsAssignableFrom(typeof(TPrimary)))
		    {
                PXCache primary = graph.Views[graph.PrimaryView].Cache;
                PXCache itemCache = (PXCache)Activator.CreateInstance(typeof(PXCache<TPrimary>), primary.Graph);
		        object[] searches = new object[primary.Keys.Count];
		        string[] sortcolumns = new string[primary.Keys.Count];
		        for (int i = 0; i < primary.Keys.Count; i++)
		        {
		            searches[i] = itemCache.GetValue(item, primary.Keys[i]);
		            sortcolumns[i] = primary.Keys[i];
		        }
		        int startRow = 0, totalRows = 0;
                graph.Views[graph.PrimaryView].Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
		        item = (TPrimary) graph.Views[graph.PrimaryView].Cache.Current;
		    }

            TPrimary upd = (TPrimary)cache.CreateCopy(item);

			if (!processor.Assign(upd, assingmentMapId))
				throw new PXException(Messages.AssignmentError);
			cache.Update(upd);
		}

		private static int? GetAssignmentMapId(PXGraph graph)
		{
			BqlCommand search = (BqlCommand)Activator.CreateInstance(BqlCommand.Compose(typeof(Search<>), typeof(TAssignmentMapField)));
			PXView view = new PXView(graph, true, BqlCommand.CreateInstance(search.GetSelectType()));
			object row = view.SelectSingle();
			return row.With(_ => (int?)view.Cache.GetValue(_, ((IBqlSearch)search).GetField().Name));
		}
	}
}
