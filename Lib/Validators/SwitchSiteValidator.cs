using System.IO;
using FluentValidation;
using Lib.Helpers;

namespace Lib.Validators
{
    public class SwitchSiteValidator : AbstractSiteValidator, ISwitchSiteValidator
    {
        public SwitchSiteValidator(IDatabaseHelper databaseHelper)
        {
            RuleFor(x => x.ConfigXmlFile)
                .NotEmpty()
                .Must(File.Exists)
                .WithMessage(x => $"File [{x.ConfigXmlFile}] does not exist")
                .DependentRules(() =>
                {
                    RuleFor(x => x.ServerName).NotEmpty();
                    RuleFor(x => x.AppPoolName).NotEmpty();
                    RuleFor(x => x.DatabaseName).NotEmpty();
                    RuleFor(x => x.SwitchDatabaseName).NotEmpty();
                    RuleFor(x => x.ConfigExeArguments).NotEmpty();
                    RuleFor(x => x.SiteDirectoryName).NotEmpty();
                    RuleForEach(x => x.DefaultDirectories).NotEmpty();
                    RuleForEach(x => x.AbsoluteDirectories).NotEmpty();
                    RuleFor(x => x.SiteVirtualDirectoryName).NotEmpty();
                    RuleFor(x => x.RootDirectory).NotEmpty().Must(Directory.Exists);
                    RuleFor(x => x.SwitchDatabaseName).Must((request, _) =>
                    {
                        var serverName = request.ServerName;
                        var databaseName = request.SwitchDatabaseName;
                        return databaseHelper.IsDatabaseExists(serverName, databaseName);
                    }).WithMessage((request, name) => $"Database [{name}] does not exist !");
                });
        }
    }
}