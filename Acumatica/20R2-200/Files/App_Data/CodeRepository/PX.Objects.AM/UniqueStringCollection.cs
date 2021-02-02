using System.Collections.Generic;

namespace PX.Objects.AM
{
    /// <summary>
    /// Collection class to maintain a small unique list of strings (avoiding empties). (Anything over 30 items in a collection can use a HashSet)
    /// </summary>
    public class UniqueStringCollection
    {
        private List<string> _uniqueList;

        public UniqueStringCollection()
        {
            _uniqueList = new List<string>();
        }

        /// <summary>
        /// Does the collection contain values
        /// </summary>
        public virtual bool HasValues
        {
            get
            {
                return _uniqueList.Count > 0;
            }
        }

        /// <summary>
        /// Add the string item if not currently in the list and not empty
        /// </summary>
        /// <param name="stringValue">String value to be added to the collection</param>
        /// <returns>True if the item was added</returns>
        public virtual bool Add(string stringValue)
        {
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                if (!Contains(stringValue))
                {
                    _uniqueList.Add(stringValue);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the current list
        /// </summary>
        /// <returns></returns>
        public List<string> List
        {
            get
            {
                return _uniqueList;
            }
        }

        public virtual bool Contains(string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue) && _uniqueList.Contains(stringValue);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _uniqueList.GetEnumerator();
        }

        public string this[int i]
        {
            get { return _uniqueList[i]; }
            set { _uniqueList[1] = value; }
        }
    }
}