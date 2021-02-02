using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	/// <exclude/>
	public class CROpportunityClassMaint : PXGraph<CROpportunityClassMaint, CROpportunityClass>
	{
		[PXViewName(Messages.OpportunityClass)]
		public PXSelect<CROpportunityClass>
			OpportunityClass;

		[PXHidden]
		public PXSelect<CROpportunityClass,
			Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunityClass.cROpportunityClassID>>>>
			OpportunityClassProperties;

        [PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CROpportunityClass, CROpportunity> Mapping;

        [PXHidden]
		public PXSelect<CRSetup>
			Setup;

        [PXViewName(Messages.OpportunityClassProbabilities)]
        public PXSelectOrderBy<CROpportunityProbability,
            OrderBy<Asc<CROpportunityProbability.sortOrder, Asc<CROpportunityProbability.probability, Asc<CROpportunityProbability.stageCode>>>>>
            OpportunityProbabilities;
        protected virtual IEnumerable opportunityProbabilities()
        {
            List<string> activeProbabilities = new List<string>();
            foreach (CROpportunityClassProbability probability in OpportunityClassActiveProbabilities.Select())
            {
                if (probability.ClassID == OpportunityClass.Current.CROpportunityClassID)
                    activeProbabilities.Add(probability.StageID);
            }

            foreach (CROpportunityProbability probability in
                PXSelectOrderBy<CROpportunityProbability,
                    OrderBy<Asc<CROpportunityProbability.sortOrder, Asc<CROpportunityProbability.probability, Asc<CROpportunityProbability.stageCode>>>>>.
                        Select(this))
            {
                probability.IsActive = activeProbabilities.Contains(probability.StageCode);
                yield return new PXResult<CROpportunityProbability>(probability);
            }
        }

        [PXHidden]
        public PXSelect<CROpportunityClassProbability>
            OpportunityClassActiveProbabilities;

        protected virtual void CROpportunityClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CROpportunityClass;
			if (row == null) return;
			
			CRSetup s = Setup.Select();
			if (s != null && s.DefaultOpportunityClassID == row.CROpportunityClassID)
			{
				s.DefaultOpportunityClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityClass, CROpportunityClass.defaultOwner> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == e.OldValue)
				return;

			row.DefaultAssignmentMapID = null;
		}

		protected virtual void CROpportunityClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CROpportunityClass;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void CROpportunityClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CROpportunityClass;
			if (row == null) return;
			
			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

        protected virtual void CROpportunityClass_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            // init default list of opportunity stages
            CROpportunityProbability s = OpportunityProbabilities.Select();
            if (s == null)
            {
                InitOpportunityProbabilities();
            }

            // activate all stages for the first class
            int classesCount = OpportunityClass.Select().Count;
            if ( classesCount == 1)
            {
                ActivateAllOpportunityClassProbabilities();
            }
        }

        protected virtual void CROpportunityClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            // checking, that at least one stage is selected while inserting or saving opportunity class
            if (OpportunityClass.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
            {
                CROpportunityClass opp = OpportunityClass.Current;
                foreach (CROpportunityClassProbability s in OpportunityClassActiveProbabilities.Select())
                {
                    if (s.ClassID == opp.CROpportunityClassID)
                        return;
                }
                OpportunityClass.Cache.RaiseExceptionHandling<CROpportunityClass.cROpportunityClassID>(opp, opp.CROpportunityClassID, new PXSetPropertyException(Messages.ClassMustHaveActiveStages, PXErrorLevel.Error));
            }
        }

        private bool CanDelete(CROpportunityClass row)
		{
			if (row != null)
			{
				CROpportunity c = PXSelect<CROpportunity, 
					Where<CROpportunity.classID, Equal<Required<CROpportunity.classID>>>>.
					SelectWindowed(this, 0, 1, row.CROpportunityClassID);
				if (c != null)
				{
					return false;
				}
			}

			return true;
		}

        #region Opportunity Probabilities
        public virtual void CROpportunityProbability_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CROpportunityProbability probability = e.Row as CROpportunityProbability;
            if (probability == null) return;
            bool allowEdit = sender.GetStatus(e.Row) == PXEntryStatus.Inserted || sender.GetStatus(e.Row) == PXEntryStatus.InsertedDeleted;
            PXUIFieldAttribute.SetEnabled<CROpportunityProbability.stageCode>(sender, probability, allowEdit);
        }
        public virtual void CROpportunityProbability_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            if (!e.ExternalCall) return;

            CROpportunityProbability probability = e.Row as CROpportunityProbability;
            if (probability == null) return;

            // checking if the opportunities with such class exist
            Standalone.CROpportunity opp = PXSelect<Standalone.CROpportunity,
                    Where<Standalone.CROpportunity.stageID, Equal<Required<CROpportunityProbability.stageCode>>>>.
                    SelectWindowed(this, 0, 1, probability.StageCode);
            if (opp != null)
            {
                throw new PXException(Messages.StageCannotBeDeleted, probability.Name);
            }

            // checking if another classes with such state exists
            List<string> classesWithActiveProbability = new List<string>();
            foreach (CROpportunityClassProbability activeProbabilityInAnotherClass in PXSelect<CROpportunityClassProbability,
                        Where<CROpportunityClassProbability.stageID, Equal<Required<CROpportunityProbability.stageCode>>,
                        And<CROpportunityClassProbability.classID, NotEqual<Current<CROpportunityClass.cROpportunityClassID>>>>>.
                    Select(this, probability.StageCode))
            {
                classesWithActiveProbability.Add(activeProbabilityInAnotherClass.ClassID);
            }
            if (classesWithActiveProbability.Count > 0)
            {
                throw new PXException(Messages.StageIsActiveInClasses, string.Join(", ", classesWithActiveProbability));
            }            

            // ask before deleting stage - because it is used in every class
            if (OpportunityProbabilities.Ask(Messages.Warning, Messages.StageWillBeDeletedFromAllClasses, MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
        public virtual void CROpportunityProbability_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            UpdateOpportunityClassProbability(sender, (CROpportunityProbability)e.Row);
        }
        public virtual void CROpportunityProbability_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            UpdateOpportunityClassProbability(sender, (CROpportunityProbability)e.Row);

            CROpportunityProbability oldProbability = e.OldRow as CROpportunityProbability;
            CROpportunityProbability newProbability = e.Row as CROpportunityProbability;

            if (newProbability == null || oldProbability == null) return;
            if (newProbability.Name != oldProbability.Name || newProbability.Probability != oldProbability.Probability || newProbability.SortOrder != oldProbability.SortOrder)
            {
                OpportunityProbabilities.Cache.RaiseExceptionHandling<CROpportunityProbability.stageCode>(newProbability, newProbability.StageCode, new PXSetPropertyException(Messages.StageWillBeChangedInEveryClass, PXErrorLevel.RowWarning));
            }
        }
        public virtual void CROpportunityProbability_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CROpportunityProbability probability = e.Row as CROpportunityProbability;
            if (probability == null || OpportunityClass.Current == null) return;
            foreach (CROpportunityClassProbability link in PXSelect<CROpportunityClassProbability, Where<CROpportunityClassProbability.stageID, Equal<Required<CROpportunityProbability.stageCode>>>>.Select(this, probability.StageCode))
            {
                OpportunityClassActiveProbabilities.Delete(link);
            }

        }
        public virtual void CROpportunityProbability_Probability_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CROpportunityProbability probability = e.Row as CROpportunityProbability;
            if (probability.SortOrder == null)
                probability.SortOrder = probability.Probability;
        }

        private void UpdateOpportunityClassProbability(PXCache sender, CROpportunityProbability probability)
        {
            if (OpportunityClass.Current == null) return;

            CROpportunityClassProbability link = PXSelect<CROpportunityClassProbability,
                    Where<CROpportunityClassProbability.classID, Equal<Current<CROpportunityClass.cROpportunityClassID>>,
                        And<CROpportunityClassProbability.stageID, Equal<Required<CROpportunityProbability.stageCode>>>>>.
                    SelectWindowed(this, 0, 1, probability.StageCode);

            if (probability.IsActive == true && link == null)
            {
                CROpportunityClassProbability newLink = OpportunityClassActiveProbabilities.Insert();
                newLink.ClassID = OpportunityClass.Current.CROpportunityClassID;
                newLink.StageID = probability.StageCode;
                OpportunityClassActiveProbabilities.Update(newLink);
            }
            else if (probability.IsActive == false && link != null)
            {
                OpportunityClassActiveProbabilities.Delete(link);
            }
        }

        private void InitOpportunityProbabilities()
        {
            using (ReadOnlyScope scope = new ReadOnlyScope(OpportunityProbabilities.Cache))
            {
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "L", Name = Messages.StageProspect, Probability = 0, SortOrder = 0 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "N", Name = Messages.StageNurture, Probability = 5, SortOrder = 5 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "P", Name = Messages.StageQualify, Probability = 10, SortOrder = 10 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "Q", Name = Messages.StageDevelop, Probability = 20, SortOrder = 20 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "V", Name = Messages.StageSolution, Probability = 40, SortOrder = 40 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "A", Name = Messages.StageProof, Probability = 60, SortOrder = 60 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "R", Name = Messages.StageClose, Probability = 80, SortOrder = 80 });
                OpportunityProbabilities.Insert(new CROpportunityProbability() { StageCode = "W", Name = Messages.StageDeploy, Probability = 100, SortOrder = 100 });
            }
        }

        private void ActivateAllOpportunityClassProbabilities()
        {
            if (OpportunityClass.Current == null)
                return;

            using (ReadOnlyScope scope = new ReadOnlyScope(OpportunityClassActiveProbabilities.Cache))
            {
                string classID = OpportunityClass.Current.CROpportunityClassID;
                foreach (CROpportunityProbability probability in OpportunityProbabilities.Select())
                {
                    OpportunityClassActiveProbabilities.Insert(new CROpportunityClassProbability() { ClassID = classID, StageID = probability.StageCode });
                }
            }
        }
        #endregion
    }
}
