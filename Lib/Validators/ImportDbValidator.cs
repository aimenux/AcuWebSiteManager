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
                .Must((request, _) =>
                {
                    var serverName = request.ServerName;
                    return databaseHelper.IsServerExists(serverName);
                }).WithMessage((request, name) => $"Server [{name}] does not exist !");

            RuleFor(x => x.DatabaseName)
                .NotEmpty()
                .Must((request, _) =>
                {
                    var serverName = request.ServerName;
                    var databaseName = request.DatabaseName;
                    return !databaseHelper.IsDatabaseExists(serverName, databaseName);
                }).WithMessage((request, name) => $"Database [{name}] already exists !");

            RuleFor(x => x.BacPacFilePath)
                .NotEmpty()
                .Must(File.Exists)
                .WithMessage(x => $"File [{x}] does not exist !");
        }
    }
}