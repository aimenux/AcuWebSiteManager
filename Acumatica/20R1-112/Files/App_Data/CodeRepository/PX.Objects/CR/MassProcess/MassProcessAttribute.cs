using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{
	public abstract class MassProcessAttribute : PXViewExtensionAttribute
	{
		#region Fields

		protected const string _OPERATIONS_VIEWNAME = "Operations";

		private Type _lightSelect;

		private string _viewName;

		#endregion

		#region Public Methods

		public bool IsDefault { get; set; }

		public Type LightSelect
		{
			get { return _lightSelect; }
			set
			{
				if (_lightSelect == value) return;

				if (_lightSelect != null && !typeof (IBqlWhere).IsAssignableFrom(value) &&
				    !typeof (IBqlWhere).IsAssignableFrom(value))
				{
					var error = string.Format("Implementation of interface '{0}' or '{1}' is expected, but '{2}' was passed.",
					                          typeof (IBqlWhere).GetLongName(), typeof (IBqlSearch).GetLongName(), value.GetLongName());
					throw new Exception(error);
				}

				_lightSelect = value;
			}
		}

		public static void SetDefaultAction(PXGraph graph, string actionName)
		{
			PXCache cache = graph.Caches[typeof (OperationParam)];
			cache.SetValue(cache.Current, typeof (OperationParam.action).Name, actionName);
		}

		#endregion

		#region Protected Methods

		protected abstract string ActionName { get; }

		protected PXView View
		{
			get { return Graph.Views[_viewName]; }
		}

		protected PXGraph Graph { get; private set; }

		protected PXView Operations
		{
			get { return Graph.Views[_OPERATIONS_VIEWNAME]; }
		}

		protected string CurrentAction
		{
			get
			{
				var cache = Graph.Caches[typeof (OperationParam)];
				return (string) cache.GetValue(cache.Current, typeof (OperationParam.action).Name);
			}
			set
			{
				var cache = Graph.Caches[typeof (OperationParam)];
				cache.SetValueExt(cache.Current, typeof (OperationParam.action).Name, value);
			}
		}

		protected virtual List<PXCache> CachesForBackup
		{
			get { return new List<PXCache>{ View.Cache, View.Graph.Caches[typeof(CS.CSAnswers)] }; }
		}

		protected virtual bool AskAdditionalParameters(IEnumerable<object> targets)
		{
			return true;
		}

		protected abstract int ProcessAction(List<object> targets);		
		#endregion

		#region Initialization

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			CheckActionName();

			_viewName = viewName;

			Graph = graph;

			InitializeOperationsView();

			AttacheDataDelegate(viewName);

			IMassProcess massProcess = (IMassProcess) Graph;
			Action<List<object>> itemsProcessDelegate = massProcess.ItemsProcessDelegate;
			massProcess.ItemsProcessDelegate = list => Process(list, itemsProcessDelegate);
			Func<List<object>, bool> askProcessDelegate = massProcess.AskProcessDelegate;
			massProcess.AskProcessDelegate = list => Ask(list, askProcessDelegate);

			AppendOpperation();

			SetDefaultAction();
		}

		#endregion

		#region Private Methods

		private void AttacheDataDelegate(string viewName)
		{
			var processingSelector = (IPXProcessingWithCustomDelegate) Graph.GetType().GetField(viewName).GetValue(Graph);
			var prevSelectDelegate = processingSelector.CustomViewDelegate;
			processingSelector.CustomViewDelegate = ((PXSelectDelegate) (() =>
				{
					if (CurrentAction == null) CurrentAction = ActionName;
					return ItemsQuery(prevSelectDelegate);
				}));
		}

		private void SetDefaultAction()
		{
			if (!IsDefault) return;

			Graph.FieldDefaulting.AddHandler<OperationParam.action>((s, e) => e.NewValue = ActionName);
		}

		private void AppendOpperation()
		{
			var cache = Graph.Caches[typeof (OperationParam)];
			var field = typeof (OperationParam.action).Name;
			var state = cache.GetStateExt(null, field) as PXStringState;
			var actionName = ActionName;
			if (state == null || state.AllowedValues == null || !state.AllowedValues.Contains(actionName))
			{
				PXStringListAttribute.AppendList(cache, null, field, new[] {actionName}, new[] {actionName});
			}
		}

		private void InitializeOperationsView()
		{
			if (!Graph.Views.ContainsKey(_OPERATIONS_VIEWNAME))
			{
				Graph.Views.Add(_OPERATIONS_VIEWNAME, new PXFilter<OperationParam>(Graph).View);

				//move operation param cache in the beginning to correct "Process" action handling
				Graph.Views.Caches.Remove(typeof (OperationParam));
				Graph.Views.Caches.Insert(0, typeof (OperationParam));
			}
		}

		private void CheckActionName()
		{
			if (string.IsNullOrWhiteSpace(ActionName))
				throw new ArgumentNullException("ActionName");
		}

		private bool Ask(List<object> list, Func<List<object>, bool> prevAskDelegate)
		{
			if (CurrentAction == ActionName && list != null)
			{
				var selectedObjects = list.Where(l => ((IPXSelectable)l).Selected == true).ToList();
				return (AskAdditionalParameters(selectedObjects));				
			}
			return prevAskDelegate == null || prevAskDelegate.Invoke(list);
		}

		private void Process(List<object> list, Action<List<object>> prevProcessDelegate)
		{
			if (CurrentAction == ActionName && list != null)
			{
				var selectedObjects = list.Where(l => ((IPXSelectable) l).Selected == true).ToList();
				using (IUnitOfWork unitOfWork = new PXCacheTransaction(CachesForBackup))
				{
					PXDBAttributeAttribute.Activate(View.Cache);
					Graph.SelectTimeStamp();
					PXRedirectRequiredException redirect = null;
					int processed;
					try
					{
						processed = ProcessAction(selectedObjects);
					}
					catch (PXRedirectRequiredException ex)
					{
						redirect = ex;
						processed = 1;
					}					

					if (processed > 0) unitOfWork.Commit();
					
					if (redirect != null) throw redirect;

					View.Cache.Clear();
					if (processed != selectedObjects.Count)
						throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
				}
			}
			else if (prevProcessDelegate != null)
				prevProcessDelegate.Invoke(list);
		}

		private IEnumerable ItemsQuery(Delegate prevSelectDelegate)
		{
			if (ActionName == CurrentAction && LightSelect != null)
			{
				BqlCommand command = View.BqlSelect;
				if (typeof (IBqlWhere).IsAssignableFrom(LightSelect))
					command = View.BqlSelect.WhereAnd(LightSelect);
				if (typeof (IBqlSelect).IsAssignableFrom(LightSelect))
					command = BqlCommand.CreateInstance(LightSelect);
				return Graph.QuickSelect(command);
			}

			if (prevSelectDelegate != null)
				return ((PXSelectDelegate) prevSelectDelegate).Invoke();

			return Graph.QuickSelect(View.BqlSelect);
		}

		#endregion
	}
}
