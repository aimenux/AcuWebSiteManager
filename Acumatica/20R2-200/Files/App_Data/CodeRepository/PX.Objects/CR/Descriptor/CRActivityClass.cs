using PX.Data;

namespace PX.Objects.CR
{
	public class CRActivityClass : PXIntListAttribute
	{
		//NOTE: don't use 5 and 3. These numbers were used in old version. Or look at sql update script carefully.
		public const int Task = 0;
		public const int Event = 1;
		public const int Activity = 2;
		public const int Email = 4;
		public const int EmailRouting = -2;
		public const int OldEmails = -3;

		public CRActivityClass()
			: base(
				new[]
				{
					Task,
					Event,
					Activity,
					Email,
					EmailRouting,
					OldEmails
				},
				new[]
				{
					Data.EP.Messages.TaskClassInfo,
					Data.EP.Messages.EventClassInfo,
					Messages.ActivityClassInfo,
					Messages.EmailClassInfo,
					Messages.EmailResponse,
					Messages.EmailClassInfo
				})
		{
		}

		public class task : PX.Data.BQL.BqlInt.Constant<task>
		{
			public task() : base(Task) { }
		}

		public class events : PX.Data.BQL.BqlInt.Constant<events>
		{
			public events() : base(Event) { }
		}

		public class activity : PX.Data.BQL.BqlInt.Constant<activity>
		{
			public activity() : base(Activity) { }
		}

		public class email : PX.Data.BQL.BqlInt.Constant<email>
		{
			public email() : base(Email) { }
		}

		public class emailRouting : PX.Data.BQL.BqlInt.Constant<emailRouting>
		{
			public emailRouting() : base(EmailRouting) { }
		}

		public class oldEmails : PX.Data.BQL.BqlInt.Constant<oldEmails>
		{
			public oldEmails() : base(OldEmails) { }
		}
	}

	public class PMActivityClass : CRActivityClass
	{
		public const int TimeActivity = 8;

		public PMActivityClass()
		{
			var values = new int[_AllowedValues.Length + 1];
			_AllowedValues.CopyTo(values, 0);
			values[_AllowedValues.Length] = TimeActivity;
			_AllowedValues = values;
			
			var labels = new string[_AllowedLabels.Length + 1];
			_AllowedLabels.CopyTo(labels, 0);
			labels[_AllowedLabels.Length] = Messages.TimeActivity;
			_AllowedLabels = labels;
		}

		public class UI
		{
			public class timeActivity : PX.Data.BQL.BqlString.Constant<timeActivity>
			{
				public timeActivity() : base(Messages.TimeActivity) { }
			}
		}

		public class timeActivity : PX.Data.BQL.BqlInt.Constant<timeActivity>
		{
			public timeActivity() : base(TimeActivity) { }
		}
	}
}
