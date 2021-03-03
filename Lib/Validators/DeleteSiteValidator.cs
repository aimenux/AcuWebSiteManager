using System.IO;
using FluentValidation;

namespace Lib.Validators
{
    public class DeleteSiteValidator : AbstractValidator, IDeleteSiteValidator
    {
        public DeleteSiteValidator()
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
                    RuleFor(x => x.ConfigExeArguments).NotEmpty();
                    RuleFor(x => x.SiteDirectoryName).NotEmpty();
                    RuleForEach(x => x.DefaultDirectories).NotEmpty();
                    RuleForEach(x => x.AbsoluteDirectories).NotEmpty();
                    RuleFor(x => x.SiteVirtualDirectoryName).NotEmpty();
                    RuleFor(x => x.RootDirectory).NotEmpty().Must(Directory.Exists);
                });
        }
    }
}
