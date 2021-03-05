using FluentValidation;
using Lib.Helpers;

namespace Lib.Validators
{
    public class ExportDbValidator : AbstractValidator, IExportDbValidator
    {
        public ExportDbValidator(IDatabaseHelper databaseHelper)
        {
            RuleFor(x => x.ServerName)
                .NotEmpty()
                .Must((request, _) => databaseHelper.IsServerExists(request))
                .WithMessage((request, name) => $"Server [{name}] does not exist !");

            RuleFor(x => x.DatabaseName)
                .NotEmpty()
                .Must((request, _) => databaseHelper.IsDatabaseExists(request))
                .WithMessage((request, name) => $"Database [{name}] does not exist !");

            RuleFor(x => x.BacPacFilePath)
                .NotEmpty();

            When(x => x.DatabaseUserName != null, () =>
            {
                RuleFor(x => x.DatabaseUserName).NotEmpty();
                RuleFor(x => x.DatabasePassword).NotEmpty();
            });

            When(x => x.DatabasePassword != null, () =>
            {
                RuleFor(x => x.DatabaseUserName).NotEmpty();
                RuleFor(x => x.DatabasePassword).NotEmpty();
            });
        }
    }
}