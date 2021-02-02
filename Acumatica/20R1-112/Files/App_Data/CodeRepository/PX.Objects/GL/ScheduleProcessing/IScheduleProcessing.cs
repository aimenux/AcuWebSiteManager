using System;

namespace PX.Objects.GL
{
	/// <summary>
	/// Represents a graph capable of processing recurring transactions
	/// according to the specified schedule.
	/// </summary>
	public interface IScheduleProcessing
	{
		void GenerateProc(Schedule schedule);
		void GenerateProc(Schedule schedule, short times, DateTime runDate);
	}
}
