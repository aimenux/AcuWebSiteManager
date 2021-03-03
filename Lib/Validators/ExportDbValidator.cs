using FluentValidation;
using Lib.Helpers;

namespace Lib.Validators
{
    public class ExportDbValidator : AbstractValidator, IExportDbValidator
    {
        public ExportDbValidator(IDatabaseHelper databaseHelper)
        {
            RuleFor(x => x.ServerName)
                .NotEmpty();

            RuleFor(x => x.DatabaseName)
                .NotEmpty()
                .Must((request, _) =>
                {
                    var serverName = request.ServerName;
                    var databaseName = request.DatabaseName;
                    return databaseHelper.IsDatabaseExists(serverName, databaseName);
                }).WithMessage((request, name) => $"Database [{name}] does not exist !");

            RuleFor(x => x.BacPacFilePath)
                .NotEmpty();
        }
    }
}