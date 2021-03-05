using FluentValidation;
using Lib.Helpers;
using System.IO;

namespace Lib.Validators
{
    public class ImportDbValidator : AbstractValidator, IImportDbValidator
    {
        public ImportDbValidator(IDatabaseHelper databaseHelper)
        {
            RuleFor(x => x.ServerName)
                .NotEmpty()
                .Must((request, _) => databaseHelper.IsServerExists(request))
                .WithMessage((request, name) => $"Server [{name}] does not exist !");

            RuleFor(x => x.DatabaseName)
                .NotEmpty()
                .Must((request, _) => !databaseHelper.IsDatabaseExists(request))
                .WithMessage((request, name) => $"Database [{name}] already exists !");

            RuleFor(x => x.BacPacFilePath)
                .NotEmpty()
                .Must(File.Exists)
                .WithMessage(x => $"File [{x}] does not exist !");

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