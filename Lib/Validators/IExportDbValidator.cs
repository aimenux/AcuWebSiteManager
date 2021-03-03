using FluentValidation;
using Lib.Models;

namespace Lib.Validators
{
    public interface IExportDbValidator : IValidator<Request>
    {
    }
}