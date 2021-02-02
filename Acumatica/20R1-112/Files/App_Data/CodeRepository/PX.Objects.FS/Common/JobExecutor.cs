using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PX.Objects.FS.ParallelProcessing
{
    public enum StepStatus
    {
        WaitingRun,
        Running,
        Done
    }

    public enum StepProcessingType
    {
        WaitStepCompletionBeforeNextStep,
        Independent,
        WaitCompletionOfAllPreviousStepsBeforeRun
    }

    public class GroupWithSteps
    {
        public int? GroupID;
        public List<Step> StepList;

        public GroupWithSteps(int? groupID)
        {
            GroupID = groupID;
            StepList = new List<Step>();
        }
    }

    public abstract class BaseShared
    {
        public abstract void Clear();

        public abstract void Dispose();
    }

    public delegate void StepMethodDelegate(MethodParm parm);

    public delegate void CheckStepResultMethodDelegate(MethodParm parm);

    public class ExecutionContext
    {
        private bool _parentFinished = true;
        private object _parentLongOperationKey = null;
        public List<Guid> childLongOperations = new List<Guid>();
        public string CompanyName;
        public string UserNameForLoginScope;
        public int? BranchID;
        public DateTime? BusinessDate;
        public string ScreenID;

        public bool ParentFinished
        {
            get
            {
                if (this._parentFinished == true)
                {
                    return true;
                }

                if (this._parentLongOperationKey == null)
                {
                    return false;
                }

                TimeSpan timespan;
                Exception message;
                PXLongRunStatus status = PXLongOperation.GetStatus(this._parentLongOperationKey, out timespan, out message);

                if (status == PXLongRunStatus.InProcess)
                {
                    return false;
                }
                else
                {
                    this._parentFinished = true;
                    
                    return true;
                }
            }
        }

        public void SaveCurrentContextValues(string companyName, object parentLongOperationKey)
        {
            this._parentFinished = false;
            this._parentLongOperationKey = parentLongOperationKey;

            this.CompanyName = companyName;
            this.UserNameForLoginScope = PXDatabase.Companies.Length > 0 ? PXAccess.GetUserName() + "@" + this.CompanyName : PXAccess.GetUserName();

            this.BranchID = PXContext.GetBranchID();
            this.BusinessDate = PXContext.GetBusinessDate();
            this.ScreenID = PXContext.GetScreenID();
        }

        public object GetParentLongOperationKey()
        {
            return this._parentLongOperationKey;
        }

        public void SetContextValues()
        {
            PXContext.SetBranchID(this.BranchID);
            PXContext.SetBusinessDate(this.BusinessDate);
            PXContext.SetScreenID(this.ScreenID);
        }
    }

    public abstract class MethodParm
    {
        public bool? IsGenerateInvoiceScreen;
        protected ExecutionContext _executionContext;

        public Step MyStep { get; private set; }

        public Exception Exception { get; set; }

        public MethodParm(Step myStep, ExecutionContext executionContext)
        {
            this.MyStep = myStep;
            this._executionContext = executionContext;
            Exception = null;
            IsGenerateInvoiceScreen = true;
        }
    }

    public class Processor
    {
        public const int WAIT_TIME_IN_MILLISECONDS = 100;

        private object _longOperationKey = null;
        private Thread _thread = null;
        private bool _singleThread = false;

        public StepGroup CurrentStepGroup { get; set; }

        public Processor(bool singleThread)
        {
            _singleThread = singleThread;
        }

        public bool IsIdle
        {
            get
            {
                if (this._longOperationKey == null)
                {
                    return true;
                }
                else
                {
                    if (this._thread != null)
                    {
                        if (this._thread.IsAlive == true)
                        {
                            return false;
                        }
                        else
                        {
                            this._thread = null;
                        }
                    }

                    TimeSpan timespan;
                    Exception message;
                    PXLongRunStatus status = PXLongOperation.GetStatus(this._longOperationKey, out timespan, out message);

                    if (status == PXLongRunStatus.InProcess)
                    {
                        return false;
                    }
                    else
                    {
                        PXLongOperation.ClearStatus(this._longOperationKey);
                        this._longOperationKey = null;
                        return true;
                    }
                }
            }
        }

        public void RunStepGroupAsync(StepGroup stepGroup)
        {
            if (this._longOperationKey != null)
            {
                throw new PXException(TX.Error.PROCESSOR_ALREADY_RUNNING_A_PROCESS);
            }

            if (stepGroup.Processor != null)
            {
                throw new PXException(TX.Error.STEP_GROUP_ALREADY_ASSIGNED_TO_PROCESSOR);
            }

            this.CurrentStepGroup = stepGroup;
            stepGroup.Processor = this;

            if (_singleThread == false)
            {
                this._longOperationKey = Guid.NewGuid();

                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "Main - " + this.CurrentStepGroup.MainContext.GetParentLongOperationKey().ToString();
                }

                this._thread = new Thread(
                    () =>
                    {
                        RunStepGroup();
                    });

                this._thread.Start();
            }
            else
            {
                RunStepGroup();
            }
        }

        public void RunStepGroup()
        {
            Guid? longOperationKey = null;

            if (this._longOperationKey != null)
            {
                longOperationKey =  (Guid)this._longOperationKey;
            }

            if (_singleThread == false)
            {
                using (var ls = new PXLoginScope(CurrentStepGroup.MainContext.UserNameForLoginScope))
                {
                    PXSessionContextFactory.AuthenticateRequest();
                    PXLogin.SetBranchID(CurrentStepGroup.MainContext.BranchID);
                    CurrentStepGroup.MainContext.SetContextValues();
                    ls.Keep();
                }

                lock (CurrentStepGroup.MainContext.childLongOperations)
                {
                    if (longOperationKey != null)
                    {
                        CurrentStepGroup.MainContext.childLongOperations.Add((Guid)longOperationKey);
                    }
                }

                PXLongOperation.StartOperation(
                    longOperationKey,
                    () =>
                    {
                        if (Thread.CurrentThread.Name == null)
                        {
                            Thread.CurrentThread.Name = "Child - " + longOperationKey.ToString();
                        }

                        CurrentStepGroup.RunAllSteps(longOperationKey);
                    });
            }
            else
            {
                CurrentStepGroup.RunAllSteps(longOperationKey);
            }
        }

        public void OnLongOperationCompleted()
        {
            if (this.CurrentStepGroup == null)
            {
                throw new PXException(TX.Error.PROCESSOR_DOES_NOT_HAVE_A_STEP_GROUP_ASSIGNED);
            }

            this.CurrentStepGroup.RaiseOnStepMethodCompleted();

            if (this.CurrentStepGroup != null)
            {
                this.CurrentStepGroup.Processor = null;
                this.CurrentStepGroup = null;
            }
        }
    }

    public class StepGroup
    {
        public abstract class StepGroupShared : BaseShared
        {
        }

        private int? _indexCurrentStep;

        public StepGroupShared Shared { get; private set; }

        public List<Step> StepList { get; set; }

        public bool AllStepsExecuted { get; private set; }

        public Processor Processor { get; set; }

        public Job ParentJob { get; private set; }

        public ExecutionContext MainContext { get; private set; }

        public StepGroup(StepGroupShared shared, Job parentJob, ExecutionContext mainContext)
        {
            this.Shared = shared;
            this.ParentJob = parentJob;
            this.MainContext = mainContext;
            this.StepList = new List<Step>();
        }

        public Step NextStep()
        {
            int newIndex;

            if (this.AllStepsExecuted == true)
            {
                return null;
            }

            if (this._indexCurrentStep == null)
            {
                newIndex = 0;
            }
            else
            {
                newIndex = (int)this._indexCurrentStep + 1;
            }

            if (newIndex >= this.StepList.Count)
            {
                this.AllStepsExecuted = true;
                this._indexCurrentStep = newIndex;

                return null;
            }

            Step nextStep = this.StepList[newIndex];

            foreach (Step previousStep in nextStep.ParentJob.StepList)
            {
                if (previousStep == nextStep)
                {
                    break;
                }
                else
                {
                    if (previousStep.MyGroup != this)
                    {
                        if (previousStep.ProcessingType == StepProcessingType.WaitStepCompletionBeforeNextStep
                            && previousStep.IsCompleted == false)
                        {
                            return null;
                        }

                        if (nextStep.ProcessingType == StepProcessingType.WaitCompletionOfAllPreviousStepsBeforeRun
                            && previousStep.IsCompleted == false)
                        {
                            return null;
                        }
                    }
                }
            }

            this._indexCurrentStep = newIndex;

            return this.StepList[(int)this._indexCurrentStep];
        }

        public void RunAllSteps(Guid? longOperationKey)
        {
            if (Processor == null)
            {
                throw new PXException(TX.Error.STEP_GROUP_DOES_NOT_HAVE_A_PROCESSOR_ASSIGNED);
            }

            Step currentStep;

            try
            {
                while (this.AllStepsExecuted == false)
                {
                    if (this.MainContext.ParentFinished == true)
                    {
                        return;
                    }

                    currentStep = this.NextStep();

                    if (currentStep == null)
                    {
                        if (this.AllStepsExecuted == false)
                        {
                            Thread.Sleep(Processor.WAIT_TIME_IN_MILLISECONDS);
                        }
                    }
                    else
                    {
                        currentStep.RunStepMethod();
                    }
                }
            }
            catch (Exception e)
            {
                lock (this.ParentJob)
                {
                    if (this.ParentJob.Exception == null)
                    {
                        this.ParentJob.Exception = e;
                    }
                }

                foreach (Step step in this.StepList)
                {
                    if (step.Status == StepStatus.WaitingRun
                        || step.Status == StepStatus.Running)
                    {
                        step.SetStepError(e, StepStatus.Running);
                    }
                }

                this.AllStepsExecuted = true;

                lock (this.ParentJob.Shared)
                {
                    if (longOperationKey != null)
                    {
                        this.ParentJob.Shared.AbortedTasks.Add((Guid)longOperationKey);
                    }
                }
            }
        }

        public void RaiseOnStepMethodCompleted()
        {
            if (Processor == null)
            {
                throw new PXException(TX.Error.STEP_GROUP_DOES_NOT_HAVE_A_PROCESSOR_ASSIGNED);
            }

            if (this.AllStepsExecuted == false)
            {
                throw new PXException(TX.Error.STEP_GROUP_STILLS_RUNNING);
            }

            foreach (Step step in this.StepList)
            {
                step.OnStepMethodCompleted();
            }

            Processor.CurrentStepGroup = null;
            Processor = null;
        }
    }

    public class JobExecutor<StepGroupSharedType>
        where StepGroupSharedType : StepGroup.StepGroupShared, new()
    {
        public List<Job> JobList;
        public List<Processor> ProcessorList;
        public ExecutionContext MainContext;

        private bool RunAllSync { get; set; }

        private List<StepGroup> StepGroupList { get; set; }

        public JobExecutor(bool isMassProcess = false) : this(GetProcessorCount(), isMassProcess)
        {
        }

        public static int GetProcessorCount()
        {
            //// For each new parallel process it is reserved one more thread in case of
            //// some graph creates a new thread by calling PXLongOperation.StartOperation
            int processorCount = PXLongOperation.ThreadPoolSize / 2;

            return processorCount;
        }

        protected static bool IS_PARALLEL_PROCESSING_ACTIVE { get { return  WebConfig.ParallelProcessingDisabled == false && WebConfig.EnableAutoNumberingInSeparateConnection; } }

        public JobExecutor(int processorCount, bool isMassProcess = false)
        {
            if (IS_PARALLEL_PROCESSING_ACTIVE == false)
            {
                processorCount = 1;
            }

            if (processorCount < 1)
            {
                processorCount = 1;
            }

            this.ProcessorList = new List<Processor>();

            if (processorCount < 2)
            {
                this.RunAllSync = true;
            }
            else
            {
                this.RunAllSync = false;
            }

            for (int i = 0; i < processorCount; i++)
            {
                var processor = new Processor(!(isMassProcess || processorCount > 1));

                this.ProcessorList.Add(processor);
            }

            this.JobList = new List<Job>();
            this.MainContext = new ExecutionContext();
        }

        public void ExecuteJobs(int? branchID, string companyName, object parentLongOperationKey)
        {
            this.MainContext.SaveCurrentContextValues(companyName, parentLongOperationKey);

            //Changing context branch id before launching long run operations
            PXContext.SetBranchID(branchID);

            try
            {
                this.ExecuteJobs_MaximizeJobsInParallel();
            }
            catch (ThreadAbortException)
            {
                foreach (Guid childLongOperation in this.MainContext.childLongOperations)
                {
                    var r = PXLongOperation.GetTaskList().FirstOrDefault(_ => _.Key == childLongOperation.ToString());

                    if (r != null)
                    {
                        PXLongOperation.AsyncAbort(r.NativeKey);
                    }
                }
            }
            finally
            {
                //Restoring default branch id
                PXContext.SetBranchID(this.MainContext.BranchID);
            }
        }

        private void ExecuteJobs_MaximizeJobsInParallel()
        {
            this.DivideStepsIntoGroups();

            StepGroup currentStepGroup = null;
            Processor currentProcessor = null;
            bool resultCheckingPending = false;
            bool allIsDone = false;

            while (allIsDone == false)
            {
                if (this.MainContext.ParentFinished == true)
                {
                    return;
                }

                allIsDone = true;
                currentStepGroup = null;
                currentProcessor = null;

                foreach (StepGroup stepGroup in this.StepGroupList)
                {
                    if (stepGroup.AllStepsExecuted == false)
                    {
                        allIsDone = false;

                        if (stepGroup.Processor == null)
                        {
                            currentStepGroup = stepGroup;
                            break;
                        }
                    }
                }

                if (allIsDone == false)
                {
                    if (currentStepGroup == null)
                    {
                        Thread.Sleep(Processor.WAIT_TIME_IN_MILLISECONDS);
                    }
                    else
                    {
                        foreach (Processor processor in this.ProcessorList)
                        {
                            if (processor.IsIdle == true)
                            {
                                currentProcessor = processor;
                                break;
                            }
                        }

                        if (currentProcessor != null)
                        {
                            if (currentProcessor.CurrentStepGroup != null)
                            {
                                currentProcessor.OnLongOperationCompleted();
                            }

                            currentProcessor.RunStepGroupAsync(currentStepGroup);
                            resultCheckingPending = true;
                        }
                        else
                        {
                            Thread.Sleep(Processor.WAIT_TIME_IN_MILLISECONDS);
                        }
                    }
                }

                foreach (StepGroup stepGroup in this.StepGroupList)
                {
                    if (stepGroup.Processor != null && stepGroup.Processor.IsIdle == true)
                    {
                        stepGroup.Processor.OnLongOperationCompleted();
                    }
                }
            }

            while (resultCheckingPending == true)
            {
                if (this.MainContext.ParentFinished == true)
                {
                    return;
                }

                resultCheckingPending = false;
                currentProcessor = null;

                foreach (Processor processor in this.ProcessorList)
                {
                    if (processor.IsIdle == false)
                    {
                        resultCheckingPending = true;
                    }
                    else
                    {
                        if (processor.CurrentStepGroup != null)
                        {
                            processor.OnLongOperationCompleted();
                            currentProcessor = processor;
                        }
                    }
                }

                if (currentProcessor == null && resultCheckingPending == true)
                {
                    Thread.Sleep(Processor.WAIT_TIME_IN_MILLISECONDS);
                }
            }
        }

        private void DivideStepsIntoGroups()
        {
            this.StepGroupList = new List<StepGroup>();

            int stepsCount = this.GetStepsCount();

            if (stepsCount == 0)
            {
                return;
            }
            
            int maxGroupSize = stepsCount / this.ProcessorList.Count;

            if (maxGroupSize == 0)
            {
                maxGroupSize = 1;
            }

            StepGroup currentStepGroup;

            if (this.RunAllSync == true)
            {
                foreach (Job job in this.JobList)
                {
                    currentStepGroup = new StepGroup(new StepGroupSharedType(), job, this.MainContext);
                    this.StepGroupList.Add(currentStepGroup);

                    foreach (Step step in job.StepList)
                    {
                        currentStepGroup.StepList.Add(step);
                        step.MyGroup = currentStepGroup;
                    }
                }
            }
            else
            {
                foreach (Job job in this.JobList)
                {
                    Dictionary<int?, int?> SOIDGroupDic = new Dictionary<int?, int?>();
                    List<GroupWithSteps> GroupWithStepList = new List<GroupWithSteps>();

                    foreach (Step step in job.StepList.Where(x => x.ProcessingType == StepProcessingType.Independent))
                    {
                        List<int?> currentSOIDList = step.docLines.Select(x => x.fsServiceOrder.SOID).Distinct().ToList();

                        GroupWithSteps groupWithSteps = new GroupWithSteps(GroupWithStepList.Count + 1);

                        groupWithSteps.StepList.Add(step);
                        GroupWithStepList.Add(groupWithSteps);

                        foreach (int? soid in currentSOIDList)
                        {
                            int? currentGroup;

                            if (SOIDGroupDic.TryGetValue(soid, out currentGroup))
                            {
                                for(int i = 0; i < SOIDGroupDic.Count(); i++)
                                {
                                    KeyValuePair<int?, int?> pairSOIDGroup = SOIDGroupDic.ElementAt(i);

                                    if (pairSOIDGroup.Value == currentGroup)
                                    {
                                        SOIDGroupDic[pairSOIDGroup.Key] = groupWithSteps.GroupID;
                                    }
                                }

                                var previuosGroupWithSteps = GroupWithStepList.Find(x => x.GroupID == currentGroup);
                                groupWithSteps.StepList = groupWithSteps.StepList.Union(previuosGroupWithSteps.StepList).ToList();
                                previuosGroupWithSteps.StepList.Clear();
                            }
                            else
                            {
                                SOIDGroupDic.Add(soid, groupWithSteps.GroupID);
                            }
                        }
                    }

                    GroupWithStepList = GroupWithStepList.Where(x => x.StepList.Count() > 1).ToList();

                    foreach (GroupWithSteps groupWithSteps in GroupWithStepList)
                    {
                        currentStepGroup = new StepGroup(new StepGroupSharedType(), job, this.MainContext);
                        this.StepGroupList.Add(currentStepGroup);

                        foreach (Step step in groupWithSteps.StepList)
                        {
                            step.MyGroup = currentStepGroup;
                            currentStepGroup.StepList.Add(step);
                        }
                    }
                }

                int groupSize = 0;
                int currentStepIndex;
                bool findNextStep = true;

                while (findNextStep == true)
                {
                    findNextStep = false;

                    foreach (Job job in this.JobList)
                    {
                        currentStepGroup = null;
                        currentStepIndex = 0;

                        foreach (Step step in job.StepList)
                        {
                            currentStepIndex++;

                            if (step.MyGroup == null)
                            {
                                if (currentStepGroup == null)
                                {
                                    currentStepGroup = new StepGroup(new StepGroupSharedType(), job, this.MainContext);
                                    this.StepGroupList.Add(currentStepGroup);

                                    groupSize = 0;
                                }

                                currentStepGroup.StepList.Add(step);
                                step.MyGroup = currentStepGroup;

                                groupSize++;

                                findNextStep = true;

                                if (groupSize == maxGroupSize
                                    || (step.ProcessingType == StepProcessingType.WaitStepCompletionBeforeNextStep
                                        && (job.StepList.Count - currentStepIndex) > maxGroupSize))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public int GetStepsCount()
        {
            int count = 0;

            foreach (Job job in this.JobList)
            {
                foreach (Step step in job.StepList)
                {
                    step.MyGroup = null;
                    count++;
                }
            }

            return count;
        }
    }

    public class Job
    {
        public class JobShared : BaseShared
        {
            public override void Clear()
            {
            }

            public override void Dispose()
            {
            }

            public List<Guid> AbortedTasks = new List<Guid>();
        }

        public List<Step> StepList;
        public Exception Exception;
        public bool ExceptionProcessed;

        public bool Canceled
        {
            get
            {
                if (this.Exception != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public JobShared Shared { get; private set; }

        public Job()
            : this(new JobShared())
        {
        }

        public Job(JobShared shared)
        {
            this.Shared = shared;
            this.StepList = new List<Step>();
            this.Exception = null;
            this.ExceptionProcessed = false;
        }
    }

    public class Step
    {
        private StepStatus _status;

        public StepStatus Status
        {
            get
            {
                return this._status;
            }
        }

        private StepProcessingType _processingType;

        public StepProcessingType ProcessingType
        {
            get
            {
                return this._processingType;
            }
        }

        public List<DocLineExt> docLines;
        public StepMethodDelegate StepMethod;
        public CheckStepResultMethodDelegate CheckStepResultMethod;

        public MethodParm Parm { get; set; }

        public string Name { get; set; }

        public StepGroup MyGroup { get; set; }

        public Job ParentJob { get; private set; }

        public Step(string name, StepProcessingType processingType, Job parentJob)
        {
            this.Name = name;
            this._processingType = processingType;
            this.ParentJob = parentJob;
            this._status = StepStatus.WaitingRun;
            this.StepMethod = null;
            this.CheckStepResultMethod = null;
        }

        public bool IsCompleted
        {
            get
            {
                if (this.ParentJob.Canceled == true)
                {
                    return true;
                }

                return this.Status == StepStatus.Done ? true : false;
            }
        }

        public void RunStepMethod()
        {
            if (this._status != StepStatus.WaitingRun)
            {
                throw new PXException(TX.Error.INVALID_STEP_STATUS_RUNNING_STEPMETHOD, this.Name);
            }

            this._status = StepStatus.Running;

            if (this.ParentJob.Canceled == true)
            {
                return;
            }

            try
            {
                this.StepMethod(this.Parm);
            }
            catch (Exception e)
            {
                this.SetStepError(e, this._status);

                if (e is ThreadAbortException)
                {
                    throw e;
                }
            }
        }

        public void OnStepMethodCompleted()
        {
            if (this._status != StepStatus.Running)
            {
                throw new PXException(TX.Error.INVALID_STEP_STATUS_RUNNING_ONSTEPMETHODCOMPLETED, this.Name);
            }

            if (this.CheckStepResultMethod != null)
            {
                this.CheckStepResultMethod(this.Parm);
            }

            this._status = StepStatus.Done;
        }

        public void SetStepError(Exception e, StepStatus newStatus)
        {
            if (this._status != StepStatus.WaitingRun
                    && this._status != StepStatus.Running)
            {
                throw new PXException(TX.Error.INVALID_STEP_STATUS_RUNNING_SETERROR, this.Name);
            }

            this._status = newStatus;

            if (this.Parm.Exception == null)
            {
                this.Parm.Exception = e;
            }
        }
    }
}
