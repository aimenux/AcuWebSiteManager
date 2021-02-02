using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class AleRecord : AufRecord
	{
		public AleRecord() : base(AufRecordType.Ale) { }

		public override string ToString()
		{
			List<object> lineData = new List<object>()
			{
				IsDesignatedGovernmentEntity == true ? "X" : null,
				IsAggregateGroupMember == true ? "X" : null,
				IsSelfInsured == true ? "X" : null,
				UsesCoeQualifyingOfferMethod == true ? "X" : null,
				AufConstants.UnusedField, // COE-Qualifying Offer Method Transition Relief
				AufConstants.UnusedField, // COE-Section 4980H Transition Relief
				UsesCoe98PctMethod == true ? "X" : null
			};

			for (int i = 0; i < 12; i++)
			{
				lineData.Add(MecIndicator == true ? "X" : null);
				lineData.Add(FteCount[i]);
				lineData.Add(EmployeeCount[i]);
				lineData.Add(IsAggregateGroupMember == true ? "X" : null);
				lineData.Add(AufConstants.UnusedField); // Transition Relief Indicator
			}

			lineData.Add(IsAuthoritativeTransmittal == true ? "X" : null);

			return FormatLine(lineData.ToArray());
		}

		public virtual bool? IsDesignatedGovernmentEntity { get; set; }
		public virtual bool? IsAggregateGroupMember { get; set; }
		public virtual bool? IsSelfInsured { get; set; }
		public virtual bool? UsesCoeQualifyingOfferMethod { get; set; }
		public virtual bool? UsesCoe98PctMethod { get; set; }
		public virtual bool? MecIndicator { get; set; }
		public virtual int[] FteCount { get; set; } = Enumerable.Repeat(0, 12).ToArray();
		public virtual int[] EmployeeCount { get; set; } = Enumerable.Repeat(0, 12).ToArray();
		public virtual bool? IsAuthoritativeTransmittal { get; set; }
	}
}
