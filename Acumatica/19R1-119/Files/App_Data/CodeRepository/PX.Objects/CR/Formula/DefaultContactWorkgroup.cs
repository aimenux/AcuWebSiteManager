using System;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.TM;

namespace PX.Objects.CR
{
	class DefaultContactWorkgroup<ClassID> : BqlFormulaEvaluator<ClassID>, IBqlOperand
		where ClassID: IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			string classID = (string)pars[typeof(ClassID)];
			CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.Select(cache.Graph, classID);
			if (cls == null) return null;

			PXSelectBase<EPCompanyTreeMember> cmd = new PXSelectJoin<EPCompanyTreeMember, 
				InnerJoin<EPCompanyTreeH, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTreeH.workGroupID>>>, 
				Where<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>(cache.Graph);
			if (cls.DefaultWorkgroupID != null && cls.OwnerIsCreatedUser != true)
			{
				return cls.DefaultWorkgroupID;
			}
			else if (cls.DefaultWorkgroupID != null && cls.OwnerIsCreatedUser == true)
			{
				cmd.WhereAnd<Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>>>();
				EPCompanyTreeMember m = cmd.SelectSingle(cache.Graph.Accessinfo.UserID, cls.DefaultWorkgroupID);
				return m.With(_=>_.WorkGroupID);
			}
			else if (cls.DefaultWorkgroupID == null && cls.OwnerIsCreatedUser == true && cls.DefaultOwnerWorkgroup == true)
			{
				EPCompanyTreeMember m = cmd.SelectSingle(cache.Graph.Accessinfo.UserID);
				return m.With(_ => _.WorkGroupID);
			}
			else
			{
				return null;
			}
		}
	}
}
