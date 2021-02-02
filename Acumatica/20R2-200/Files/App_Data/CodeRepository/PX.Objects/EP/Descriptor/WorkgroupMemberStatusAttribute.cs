using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	public class WorkgroupMemberStatusAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber
	{
		public WorkgroupMemberStatusAttribute()
			: base(new (string, string)[] {
				(PermanentActive, Messages.PermanentActive),
				(PermanentInactive, Messages.PermanentInactive),
				(TemporaryActive, Messages.TemporaryActive),
				(TemporaryInactive, Messages.TemporaryInactive),
				(AdHoc, Messages.Adhoc) })
		{
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			var row = (EPTimeActivitiesSummary)e.Row;
			if (row?.IsMemberActive != null && row?.Status != null)
			{
				e.ReturnValue = GetStatus(row.IsMemberActive, row.Status);
			}

			if (e.ReturnValue == null)
			{
				e.ReturnValue = AdHoc;
			}
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{

			var row = (EPTimeActivitiesSummary)e.Row;
			if (row != null)
			{
				var member = SelectFrom<EPCompanyTreeMember>
							.Where<EPCompanyTreeMember.workGroupID.IsEqual<P.AsInt>
							.And<EPCompanyTreeMember.contactID.IsEqual<P.AsInt>>>.View.Select(sender.Graph, row.WorkgroupID, row.ContactID).TopFirst;
				e.NewValue = GetStatus(member?.Active, member?.MembershipType);
			}
		}

		private static string GetStatus(bool? isActive, string membershipType)
		{
			string status = AdHoc;
			if (isActive == true && membershipType == MembershipTypeListAttribute.Permanent)
			{
				status = PermanentActive;
			}
			else if (isActive == false && membershipType == MembershipTypeListAttribute.Permanent)
			{
				status = PermanentInactive;
			}
			else if (isActive == true && membershipType == MembershipTypeListAttribute.Temporary)
			{
				status = TemporaryActive;
			}
			else if (isActive == false && membershipType == MembershipTypeListAttribute.Temporary)
			{
				status = TemporaryInactive;
			}

			return status;
		}

		public const string PermanentActive = "PERMA";
		public class permanentActive : PX.Data.BQL.BqlString.Constant<permanentActive>
		{
			public permanentActive() : base(PermanentActive) { }
		}

		public const string PermanentInactive = "PERMI";
		public class permanentInactive : PX.Data.BQL.BqlString.Constant<permanentInactive>
		{
			public permanentInactive() : base(PermanentInactive) { }
		}

		public const string TemporaryActive = "TEMPA";
		public class temporaryActive : PX.Data.BQL.BqlString.Constant<temporaryActive>
		{
			public temporaryActive() : base(TemporaryActive) { }
		}

		public const string TemporaryInactive = "TEMPI";
		public class temporaryInactive : PX.Data.BQL.BqlString.Constant<temporaryInactive>
		{
			public temporaryInactive() : base(TemporaryInactive) { }
		}

		public const string AdHoc = "ADHOC";
		public class adHoc : PX.Data.BQL.BqlString.Constant<adHoc>
		{
			public adHoc() : base(AdHoc) { }
		}
	}
}
