using System.Collections.Generic;

namespace PX.Objects.CN.JointChecks.AP.Models
{
    public class JointCheckPrintModel
    {
        private JointCheckPrintModel()
        {
        }

        public List<int?> InternalJointPayeeIds
        {
            get;
            set;
        }

        public List<string> ExternalJointPayeeNames
        {
            get;
            set;
        }

        public string JointPayeeNames
        {
            get;
            set;
        }

        public bool IsMultilinePrintMode
        {
            get;
            set;
        }

        public static JointCheckPrintModel Create(bool isMultiline)
        {
            return new JointCheckPrintModel
            {
                InternalJointPayeeIds = new List<int?>(),
                ExternalJointPayeeNames = new List<string>(),
                JointPayeeNames = string.Empty,
                IsMultilinePrintMode = isMultiline
            };
        }
    }
}