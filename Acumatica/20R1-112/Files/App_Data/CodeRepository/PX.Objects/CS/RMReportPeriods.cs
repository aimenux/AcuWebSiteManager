using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CS
{
    public class RMReportPeriods<T>
    {
        private List<MasterFinPeriod> _finPeriods;
        private string _perWildcard;

        public RMReportPeriods(PXGraph graph)
        {
            _perWildcard = new string(RMReportConstants.DefaultWildcardChar, 6);
			var res = PXSelectOrderBy<MasterFinPeriod, OrderBy<Asc<MasterFinPeriod.startDate>>>.Select(graph).ToList();
			_finPeriods = new List<MasterFinPeriod>(graph.Caches[typeof(MasterFinPeriod)].Cached.Cast<MasterFinPeriod>());
        }

        public string PerWildcard
        {
            get
            {
                return _perWildcard;
            }
        }

        public List<MasterFinPeriod> FinPeriods
        {
            get
            {
                return _finPeriods;
            }
        }

        public List<T> GetPeriodsForBeginningBalanceAmount(RMDataSourceGL dsGL, Dictionary<string, T> periodsForKey, bool limitToStartYear, out bool takeLast)
        {
            takeLast = false;

            if (dsGL.StartPeriod != null)
            {
                string per = RMReportWildcard.EnsureWildcard(dsGL.StartPeriod, _perWildcard);
                if (!per.Contains(_perWildcard))
                {
                    per = GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
                    string mostRecentPeriod;

                    if (limitToStartYear)
                    {
                        // For profit and loss accounts, we only look into same fiscal year as the end period.
                        mostRecentPeriod = GetMostRecentPeriodInList(periodsForKey, per.Substring(0, 4), per);
                    }
                    else
                    {
                        // For other type of accounts, we retrive the most recent row, no matter what is the year
                        mostRecentPeriod = GetMostRecentPeriodInList(periodsForKey, per);
                    }

                    if (String.IsNullOrEmpty(mostRecentPeriod))
                    {
                        return null;
                    }
                    else
                    {
                        takeLast = per != mostRecentPeriod;
                        return new List<T> { periodsForKey[mostRecentPeriod] };
                    }
                }
                else
                {
                    return periodsForKey.Values.ToList<T>();
                }
            }
            else
            {
                return periodsForKey.Values.ToList<T>();
            }
        }

        public List<T> GetPeriodsForEndingBalanceAmount(RMDataSourceGL dsGL, Dictionary<string, T> periodsForKey, bool limitToEndYear)
        {
            if (dsGL.EndPeriod != null)
            {
                string per = RMReportWildcard.EnsureWildcard(dsGL.EndPeriod, _perWildcard);
                if (!per.Contains(RMReportConstants.DefaultWildcardChar))
                {
                    per = GetFinPeriod(per, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
                    string mostRecentPeriod;

                    if (limitToEndYear)
                    {
                        // For profit and loss accounts, we only look into same fiscal year as the end period.
                        mostRecentPeriod = GetMostRecentPeriodInList(periodsForKey, per.Substring(0, 4), per);
                    }
                    else
                    {
                        // For other type of accounts, we retrive the most recent row, no matter what is the year
                        mostRecentPeriod = GetMostRecentPeriodInList(periodsForKey, per);
                    }

                    if (String.IsNullOrEmpty(mostRecentPeriod))
                    {
                        return null;
                    }
                    else
                    {
                        return new List<T> { periodsForKey[mostRecentPeriod] };
                    }
                }
                else
                {
                    return periodsForKey.Values.ToList<T>();
                }
            }
            else
            {
                return periodsForKey.Values.ToList<T>();
            }
        }

        public List<T> GetPeriodsForRegularAmount(RMDataSourceGL dsGL, Dictionary<string, T> periodsForKey)
        {
            if (dsGL.StartPeriod != null)
            {
                var periods = new List<T>();

                string per = RMReportWildcard.EnsureWildcard(dsGL.StartPeriod, _perWildcard);
                if (!per.Contains(_perWildcard) && dsGL.StartPeriodOffset != null && dsGL.StartPeriodOffset != 0 || dsGL.StartPeriodYearOffset != 0)
                {
                    per = GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
                }
                if (dsGL.EndPeriod == null)
                {
                    if (!per.Contains(_perWildcard))
                    {
                        T v;
                        if (periodsForKey.TryGetValue(per, out v))
                        {
                            periods.Add(v);
                        }
                    }
                    else
                    {
                        foreach (var p in periodsForKey)
                        {
                            if (RMReportWildcard.IsLike(per, p.Key))
                            {
                                periods.Add(p.Value);
                            }
                        }
                    }
                }
                else
                {
                    string toper = RMReportWildcard.EnsureWildcard(dsGL.EndPeriod, _perWildcard);
                    if (!toper.Contains(RMReportConstants.DefaultWildcardChar) && dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0 || dsGL.EndPeriodYearOffset != 0)
                    {
                        toper = GetFinPeriod(toper, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
                    }
                    toper = toper.Replace(RMReportConstants.DefaultWildcardChar, '9');

                    foreach (var p in periodsForKey)
                    {
                        if (RMReportWildcard.IsBetween(per, toper, p.Key))
                        {
                            periods.Add(p.Value);
                        }
                    }
                }

                return periods;
            }
            else
            {
                return periodsForKey.Values.ToList<T>();
            }
        }

        private static string GetMostRecentPeriodInList(Dictionary<string, T> list, string minPeriod, string maxPeriod)
        {
            string mostRecentPeriod = String.Empty;
            foreach (string p in list.Keys)
            {
                if (String.Compare(p, minPeriod) >= 0 && String.Compare(p, maxPeriod) <= 0 && String.Compare(p, mostRecentPeriod) > 0)
                {
                    mostRecentPeriod = p;
                }
            }
            return mostRecentPeriod;
        }

        private static string GetMostRecentPeriodInList(Dictionary<string, T> list, string maxPeriod)
        {
            string mostRecentPeriod = String.Empty;
            foreach (string p in list.Keys)
            {
                if (String.Compare(p, maxPeriod) <= 0 && String.Compare(p, mostRecentPeriod) > 0)
                {
                    mostRecentPeriod = p;
                }
            }
            return mostRecentPeriod;
        }

        /// <summary>
        /// Returns the financial period("{0:0000}{1:00}") by the current period("{0:0000}{1:00}") and offsets.
        /// Note: In different years can be a different number of financial periods.
        /// </summary>		
        public string GetFinPeriod(string period, short? yearOffset, short? periodOffset)
        {
            string result = period;
            if (!string.IsNullOrEmpty(period) && period.Length == 6)
            {
                string year = period.Substring(0, 4);
                string periodNbr = period.Substring(4);
                // apply year offset
                if (yearOffset != null && yearOffset != 0)
                {
                    string resYear = (int.Parse(year) + yearOffset).ToString();
                    List<MasterFinPeriod> finPeriodsResYear = _finPeriods.Where(f => string.Compare(f.FinYear, resYear.ToString()) == 0).ToList();
                    if (finPeriodsResYear != null && finPeriodsResYear.Count > 0)
                    {
                        string resPeriodNbr = finPeriodsResYear.Last().PeriodNbr;
                        if (string.Compare(resPeriodNbr, periodNbr) < 0)
                            result = String.Format("{0:0000}{1:00}", resYear, resPeriodNbr);
                        else
                            result = String.Format("{0:0000}{1:00}", resYear, periodNbr);
                    }
                    else // if there is no year
                    {
                        result = String.Format("{0:0000}{1:00}", resYear, 1);
                    }
                }
                // apply period offset
                if (periodOffset != null && periodOffset != 0)
                {
                    short currentIndex = 0;
                    foreach (MasterFinPeriod p in _finPeriods)
                    {
                        if (!string.Equals(p.FinPeriodID, result))
                            currentIndex++;
                        else
                            break;
                    }
                    currentIndex = (short)(currentIndex + periodOffset);
                    if (currentIndex < 0) // if there is no year
                    {
                        result = String.Format("{0:0000}{1:00}", int.Parse(year) - 1, periodNbr);
                    }
                    else
                    {
                        short i = 0;
                        foreach (MasterFinPeriod p in _finPeriods)
                        {
                            if (i == currentIndex)
                            {
                                result = p.FinPeriodID;
                                break;
                            }
                            else
                                i++;
                        }
                    }
                }
            }
            return result;
        }
    }
}
