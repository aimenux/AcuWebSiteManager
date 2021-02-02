using System.IO;
using FluentValidation;
using Lib.Helpers;

namespace Lib.Validators
{
    public class CreateSiteValidator : AbstractSiteValidator, ICreateSiteValidator
    {
        public CreateSiteValidator(ISiteHelper siteHelper)
        {
            RuleFor(x => x.ConfigXmlFile)
                .NotEmpty()
                .Must(x => File.Exists(x) && IsValidXmlExtension(x))
                .WithMessage(x => $"File [{x.ConfigXmlFile}] does not exist or has not valid extension")
                .DependentRules(() =>
                {
                    RuleFor(x => x.ConfigExeFile)
                        .NotEmpty()
                        .Must(x => File.Exists(x) && IsValidExeExtension(x))
                        .WithMessage(x => $"File [{x.ConfigExeFile}] does not exist or has not valid extension");

                    RuleFor(x => x.Password).NotEmpty();
                    RuleFor(x => x.ServerName).NotEmpty();
                    RuleFor(x => x.AppPoolName).NotEmpty();
                    RuleFor(x => x.DatabaseName).NotEmpty();
                    RuleFor(x => x.ConfigExeArguments).NotEmpty();
                    RuleFor(x => x.SiteDirectoryName).NotEmpty();
                    RuleForEach(x => x.DefaultDirectories).NotEmpty();
                    RuleForEach(x => x.AbsoluteDirectories).NotEmpty();
                    RuleFor(x => x.SiteVirtualDirectoryName)
                        .NotEmpty()
                        .Must((request, siteName) => !siteHelper.IsSiteExists(request))
                        .WithMessage(x => $"Site [{x.SiteVirtualDirectoryName}] already exists");
                });
        }
    }
}
