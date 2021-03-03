using FluentValidation;
using Lib.Models;

namespace Lib.Validators
{
    public abstract class AbstractValidator : AbstractValidator<Request>
    {
        protected bool IsValidXmlExtension(string file)
        {
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(".xml");
        }

        protected bool IsValidExeExtension(string file)
        {
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(".exe");
        }
    }
}
