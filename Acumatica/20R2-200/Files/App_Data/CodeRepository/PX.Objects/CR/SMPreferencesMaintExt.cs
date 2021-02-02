using System;
using System.Globalization;
using System.IO;
using System.Web;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Collections;
using PX.Common.Cryptography;
using PX.Common.Collection;
using System.Text;
using System.Security.Permissions;
using PX.Data.EP;
using PX.Mail;
using PX.Api;
using System.Collections.Generic;
using System.Reflection;
using MailSender = PX.Common.Mail.MailSender;
using PX.Web.UI;
using PX.SM;
namespace PX.Objects.CR
{
	public class PreferencesGeneralMaintExt : PXGraphExtension<PX.SM.PreferencesGeneralMaint>
	{
		[PXOverride]
		public virtual void UpdatePersonDisplayNames(string PersonDisplayNameFormat)
		{
			using (var tran = new PXTransactionScope())
			{
				switch (PersonDisplayNameFormat)
				{
					case PersonNameFormatsAttribute.WESTERN:
						SetContactsWesternOrder();
						break;

					case PersonNameFormatsAttribute.EASTERN:
						SetContactsEasternOrder();
						break;

					case PersonNameFormatsAttribute.LEGACY:
						SetContactsLegacyOrder();
						break;

					case PersonNameFormatsAttribute.EASTERN_WITH_TITLE:
						SetContactsEasternWithTitleOrder();
						break;
				}

				UpdateBAccounts();

				tran.Complete();
			}

			PXDatabase.ClearCompanyCache();
		}

		protected virtual void SetContactsWesternOrder()
		{
			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<IsNull<Add<Contact.firstName, Space>, Empty>, Contact.lastName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull>>>());
		}
		protected virtual void SetContactsEasternOrder()
		{
			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Contact.lastName, IsNull<Add<CommaSpace, Contact.firstName>, Empty>>>,
				Select<Contact, Where<Contact.lastName, IsNotNull>>>());
		}
		protected virtual void SetContactsLegacyOrder()
		{
			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName,
					Contact.lastName>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNull,
					And<Contact.firstName, IsNull,
					And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.lastName, CommaSpace>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNull,
					And<Contact.firstName, IsNull,
					And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.lastName, CommaSpace>, Contact.firstName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNull,
					And<Contact.firstName, IsNotNull,
					And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<
					Contact.lastName, CommaSpace>, Contact.firstName>, Space>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNull,
					And<Contact.firstName, IsNotNull,
					And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.lastName, CommaSpace>, Contact.title>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNotNull,
					And<Contact.firstName, IsNull,
					And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<
					Contact.lastName, Space>, Contact.midName>, CommaSpace>, Contact.title>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNotNull,
					And<Contact.firstName, IsNull,
					And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<
					Contact.lastName, Space>, Contact.firstName>, CommaSpace>, Contact.title>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNotNull,
					And<Contact.firstName, IsNotNull,
					And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<Add<Add<
					Contact.lastName, Space>, Contact.firstName>, Space>, Contact.midName>, CommaSpace>, Contact.title>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
					And<Contact.title, IsNotNull,
					And<Contact.firstName, IsNotNull,
					And<Contact.midName, IsNotNull>>>>>>());
		}
		protected virtual void SetContactsEasternWithTitleOrder()
		{
			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName,
				Contact.lastName>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNull,
				And<Contact.firstName, IsNull,
				And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.lastName, CommaSpace>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNull,
				And<Contact.firstName, IsNull,
				And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.lastName, CommaSpace>, Contact.firstName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNull,
				And<Contact.firstName, IsNotNull,
				And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<
					Contact.lastName, CommaSpace>, Contact.firstName>, Space>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNull,
				And<Contact.firstName, IsNotNull,
				And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<
					Contact.title, Space>, Contact.lastName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNotNull,
				And<Contact.firstName, IsNull,
				And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add
				<Contact.title, Space>, Contact.lastName>, CommaSpace>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNotNull,
				And<Contact.firstName, IsNull,
				And<Contact.midName, IsNotNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<
					Contact.title, Space>, Contact.lastName>, CommaSpace>, Contact.firstName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNotNull,
				And<Contact.firstName, IsNotNull,
				And<Contact.midName, IsNull>>>>>>());

			PXDatabase.Update(new PXGraph(),
				new Update<Set<Contact.displayName, Add<Add<Add<Add<Add<Add<
					Contact.title, Space>, Contact.lastName>, CommaSpace>, Contact.firstName>, Space>, Contact.midName>>,
				Select<Contact, Where<Contact.lastName, IsNotNull,
				And<Contact.title, IsNotNull,
				And<Contact.firstName, IsNotNull,
				And<Contact.midName, IsNotNull>>>>>>());
		}
		protected virtual void UpdateBAccounts()
		{
			PXDatabase.Update(new PXGraph(),
				new Update<Set<BAccount.acctName, Contact.displayName>,
				Select2<BAccount,
				InnerJoin<Contact, On<Contact.contactID, Equal<BAccount.defContactID>>>,
				Where<BAccount.type, Equal<BAccountType.employeeType>>>>());
		}
	}
}