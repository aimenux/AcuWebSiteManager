using System.Collections.Generic;
using PX.Common;

namespace PX.Objects.AM
{
    public static class MachineScheduleHelper
    {
        /// <summary>
        /// Does the list of machine schedule detail all contain the same <see cref="AMMachSchdDetail.SchdTime"/> value
        /// </summary>
        public static bool HasSameSchdTime(this List<AMMachSchdDetail> value)
        {
            if (value == null)
            {
                return false;
            }

            if (value.Count() <= 1)
            {
                return true;
            }

            for (var i = 1; i < value.Count; i++)
            {
                if (!value[i - 1].SchdTime.GetValueOrDefault().Equals(value[i].SchdTime.GetValueOrDefault()))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<AMMachSchdDetail> GetHasSchdTime(this List<AMMachSchdDetail> value)
        {
            if (value == null)
            {
                yield break;
            }

            foreach (var machSchdDetail in value)
            {
                if ((machSchdDetail?.SchdTime ?? 0) > 0)
                {
                    yield return machSchdDetail;
                }
            }
        }

        public static MachineCalendarHelper FindCalendarByMachineId(this List<MachineCalendarHelper> value, AMMachSchdDetail machSchdDetail)
        {
            return FindCalendarByMachineId(value, machSchdDetail?.MachID);
        }

        public static MachineCalendarHelper FindCalendarByMachineId(this List<MachineCalendarHelper> value, string machineId)
        {
            if (value == null || string.IsNullOrWhiteSpace(machineId))
            {
                return null;
            }

            foreach (var machineCalendarHelper in value)
            {
                if (machineCalendarHelper?.Machine?.MachID.Equals(machineId) == true)
                {
                    return machineCalendarHelper;
                }
            }

            return null;
        }
    }
}