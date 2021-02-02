namespace PX.Objects.PR.AUF
{
	public class VerRecord : AufRecord
	{
		public VerRecord(string versionNumber) : base(AufRecordType.Ver)
		{
			_VersionNumber = versionNumber;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				_VersionNumber,
				AufConstants.VendorName,
				AufConstants.SourceProgram
			};

			return FormatLine(lineData);
		}

		private string _VersionNumber;
	}
}
