using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CT;
using PX.Objects.GL;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	public class TemplateGlobalTaskListMaint : PXGraph<TemplateGlobalTaskListMaint>
	{
		#region DAC Attributes Override
		        
        #region PMTask
        [PXDBInt(IsKey=true)]
		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
        protected virtual void _(Events.CacheAttached<PMTask.projectID> e)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProjectTaskStatus.Active)]
        [PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
        protected virtual void _(Events.CacheAttached<PMTask.status> e)
        {
        }

        [Customer(DescriptionField = typeof(Customer.acctName), Visibility = PXUIVisibility.Invisible, Visible = false)]
        protected virtual void _(Events.CacheAttached<PMTask.customerID> e)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        protected virtual void _(Events.CacheAttached<PMTask.autoIncludeInPrj> e)
        {
        }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInGL>))]
		[PXUIField(DisplayName = "GL")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInGL> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAP>))]
		[PXUIField(DisplayName = "AP")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInAP> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAR>))]
		[PXUIField(DisplayName = "AR")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInAR> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCA>))]
		[PXUIField(DisplayName = "CA")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInCA> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCR>))]
		[PXUIField(DisplayName = "CRM")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInCR> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInTA>))]
		[PXUIField(DisplayName = "Time Entries")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInTA> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInEA>))]
		[PXUIField(DisplayName = "Expenses")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInEA> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInIN>))]
		[PXUIField(DisplayName = "IN")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInIN> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInSO>))]
		[PXUIField(DisplayName = "SO")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInSO> e) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInPO>))]
		[PXUIField(DisplayName = "PO")]
		protected virtual void _(Events.CacheAttached<PMTask.visibleInPO> e) { }

        #endregion

        #endregion

        #region Views/Selects

        public PXSavePerRow<PMTask> Save;
        public PXCancel<PMTask> Cancel;

        public PXSelectJoin<PMTask, InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTask.projectID>>>, Where<PMProject.nonProject, Equal<True>>> Tasks;
       
        #endregion

        #region Actions/Buttons

		public PXAction<PMTask> viewTask;
        [PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable ViewTask(PXAdapter adapter)
        {
            if (Tasks.Current != null)
            {
				TemplateGlobalTaskMaint graph = CreateInstance<TemplateGlobalTaskMaint>();
				graph.Task.Current = PMTask.PK.Find(this, Tasks.Current.TaskID);
				
                throw new PXPopupRedirectException(graph, Messages.ProjectTaskEntry + " - " + Messages.ViewTask, true);
            }
            return adapter.Get();
        }

        #endregion

		public TemplateGlobalTaskListMaint()
        {            
        }

        #region Event Handlers
		        

        #endregion



	}
}
