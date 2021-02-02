namespace PX.Objects.PJ.RequestsForInformation.Descriptor
{
    public static class RequestForInformationConstants
    {
        public static class Print
        {
            public const string RequestForInformationId = "RFIID";
            public const string EmailId = "EmailId";
        }

        public static class EmailTemplate
        {
	        public const string NotificationName = "E-Mail Request For Information (v2)";

	        public const int IndexNotFound = -1;

	        public const string ResponseNoteTag =
		        "<p class=\"richp\">To respond to the RFI, reply to this email without changing the subject.</p>";

	        public const string RecipientNotesTag =
		        "<p class=\"richp\" style=\"text-align: left;\">{0}</p><p class=\"richp\"><br></p>";

	        public const string LogoTag =
		        "<p class=\"richp\" style=\"text-align: left;\"><img src=\"{0}\" objtype=\"file\" data-convert=\"view\" embedded=\"true\" title=\"{1}\"><br></p>";
        }
	}
}