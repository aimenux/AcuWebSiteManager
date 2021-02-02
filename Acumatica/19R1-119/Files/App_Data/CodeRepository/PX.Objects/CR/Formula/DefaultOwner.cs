using System;
using System.Collections.Generic;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	class DefaultContactOwner<ClassID> : BqlFormulaEvaluator<ClassID>, IBqlOperand
		where ClassID: IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			string classID = (string)pars[typeof(ClassID)];
			CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.Select(cache.Graph, classID);
			if (cls == null) return null;
			
			var rec = (PXResult<Users, Contact, BAccountR>)
				PXSelectJoin<Users,
					InnerJoin<Contact, On<Contact.userID, Equal<Users.pKID>>,
					InnerJoin<BAccountR, On<Contact.contactID, Equal<BAccountR.defContactID>, And<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
					Where<Users.pKID, Equal<Current<AccessInfo.userID>>>>.SelectSingleBound(cache.Graph,null, null);
			BAccountR bAccount = rec;
			return bAccount != null && !string.IsNullOrEmpty(bAccount.AcctCD) && cls.OwnerIsCreatedUser == true
				? bAccount.AcctCD
				: null;
		}
	}
}
