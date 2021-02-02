using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;

namespace PX.Objects.WZ
{
    [PXLocalizable(Messages.Prefix)]
    public static class Messages
    {

        public const string Prefix = "WZ Error";

        public const string Pending = "Pending";
        public const string Active = "In Progress";
        public const string Suspend = "Suspended";
        public const string Completed = "Completed";
        public const string Skipped = "Skipped";
        public const string Disabled = "Disabled";
        public const string Open = "Open";


        public const string Article = "Article";
        public const string Screen = "Screen";

        public const string TaskAlreadyInSuccessorList = "This task already in successor list";
        public const string TaskAlreadyInPredecessorList = "This task already in predecessor list";

        public const string CannotBeCompletedWileOpenTasks = "Task cannot be completed because there are still Open/In Progress subtasks.";

        public const string SuccessorCycleError = "This task cannot be successor for current task because of cycle reference";
        public const string PredecessorCycleError = "This task cannot be predecessor for current task because of cycle reference";

		public const string DeleteTaskHeader = "Delete task";
        public const string AllChildTasksWillBeDeleted = "All child tasks will be deleted. Are you sure you want to delete this task?";
        public const string ScreenTaskCannotHaveChilds = "Task of Screen type cannot have childs";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
        public const string ScreenTaskMustHaveID = "Screen ID must have a value in screen tasks.";

        public const string NewTask = "New Task";

        public const string ScenarioRootLocationError = "Only one Scenario can be located at the Sitemap Root.";
        public const string ScenarioCannotBeEditedInStatus = "A scenario in the '{0}' status cannot be edited";
        public const string NoAccessRightsToWizardArticle = "You have no access rights to the Wizard Article Screen WZ.20.15.10";

        public const string BackToScenario = "Back to Scenario";
		public const string ScenarioAlreadyUsed = "The scenario is already used in the {0} schedule.";
		public const string ScenarioReferenceIsNotValid = "The scenario reference is not valid.";
		public const string WizardScenarioName = "Wizard Scenario";
    }
}
