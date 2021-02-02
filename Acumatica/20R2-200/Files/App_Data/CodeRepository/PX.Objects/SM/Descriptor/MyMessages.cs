using PX.Data;
using PX.Common;

namespace PX.SM
{
	[PXLocalizable(MyMessages.Prefix)]
	public static class MyMessages
	{
		public const string Prefix = "SM Error";

		#region Graph Names

		public const string MyProfileMaint = "My Profile Maintenance";

		#endregion

		#region Messages used by web site
		public const string ConfirmationSend = "Registration confirmation has been sent to your e-mail.";
		public const string RequestSubmitted = "Request submitted successfully.";
		public const string RequestSubmittedDescr = "Thank you for submitting your request.";
		public const string RequestExpired = "Your confirmation request has failed.";
		public const string RequestExpiredDescr = "Your request confirmation token has expired. Please, generate your request again.";

		public const string AccountHasNoEmail = "No e-mail address is specified in your account.";
		public const string AccountNotFound = "Account not found. Contact your system administrator.";
		public const string btnSearchText = "Search";
		public const string CheckSpelling = "Make sure all words are spelled correctly.";
		public const string EmailEmpty = "Please, enter your e-mail address.";
		public const string EnterQuery = "Enter your query:";
		public const string ErrorSendingEmail = "An error occurred while sending an e-mail.";
		public const string FullTextSearch = "Full-text search (slower)";
		public const string InterfaceLanguage = "Interface Language :";
		public const string InvalidLogin = "Please, enter your valid login.";
		public const string InvalidRecoveryAnswer = "Invalid recovery answer.";
		public const string lblCategories = "Categories :";
		public const string lblReplace = "Replace with :";
		public const string lblSearch = "Search :";
		public const string Login = "Please enter your login information.";
		public const string LoginNotFound = "The login you specified is incorrect.";
		public const string LoginRecovery = "Login recovery";
		public const string LoginWelcome = "Welcome to Acumatica";
		public const string IncorrectLoginSymbols = "Incorrect login or password.";
		public const string NewPasswordMustDiffer = "New password must differ from the old one.";
		public const string NothingFound = "We're sorry, no results found for";
		public const string Password = "Password :";
		public const string PasswordBlank = "New password can not be blank.";
		public const string PasswordNotConfirmed = "The password entered doesn't match confirmation.";
		public const string PasswordRecoveryDenied = "Password recovery feature is disabled for this account. Contact your system administrator.";
		public const string PasswordSent = "The e-mail containing instructions on further actions is sent to your address.";
		public const string SearchCaption = "Acumatica Site Search";
		public const string Search = "search...";

		public const string SearchReplace = "Search and replace";
		public const string SearchResults = "Results <b>{0}-{1}</b> of <b>{2}</b> for: <b><i>{3}</i></b> ({4} seconds)";
		public const string SearchTips = "Search tips";
		public const string SimplifyQuery = "Consider simplifying complex or wordy queries.";
		public const string SpecifySearchRequest = "Please, specify your search request.";
		public const string TryFullTextSearch = "Try using more popular keywords.";
		public const string ttipHelpReplace = "Replace matching text with.";
		public const string ttipHelpSearch = "Search for articles.";
		public const string Username = "Username :";
		public const string NotificationNotAvailable = "We're sorry! Notification service is unavailable at the moment, please try again later.";
		public const string MandatoryField = "This field is mandatory";
		public const string CompleteRequiredFields = "Complete all required fields";
		public const string Error404 = "<b>404:</b> We're sorry! The requested resource could not be accessed.<br />Please, try to repeat your request later or contact our technical staff.";

		public const string LoginBtn = "Login";
		public const string LogoutBtn = "Logout";
		public const string ContactUs = "Contact Us";
		public const string EmployeeContactWouldBeCleared = "Unable to link employee with guest user. Do you want to proceed operation and remove link?";
		public const string ExternalUserContactWouldBeCleared = "Unable to link contact with non guest user. Do you want to proceed operation and remove link?";
		public const string SendReceiveAll = "Send/Receive All";
		public const string ArticleDoesNotExist = "The article does not exist, or you don't have enough rights to see it.";
		public const string AppliesTo = "Applies to: ";
		public const string Article = "Article: ";
		public const string CreateDate = "Create Date: ";
		public const string LastModified = "Last Modified: ";
		public const string Views = "Views: ";
		public const string Rating = "Rating: ";
		public const string Category = "Category: ";
		#endregion

		public const string CannotFindFinancialPeriod = "Cannot determine current financial period";
		public const string CannotFindNextFinancialPeriod = "Cannot determine resulting financial period: shift value is too big";
		public const string Account = "Account";
	}
}
