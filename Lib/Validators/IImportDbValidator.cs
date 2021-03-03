using FluentValidation;
using Lib.Models;

namespace Lib.Validators
{
    public interface IImportDbValidator : IValidator<Request>
    {
    }
}