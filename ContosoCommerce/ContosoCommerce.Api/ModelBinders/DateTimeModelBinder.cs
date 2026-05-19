using System;
using System.Globalization;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace ContosoCommerce.Api.ModelBinders
{
    /// <summary>
    /// Custom model binder for DateTime parsing.
    /// Uses Thread.CurrentThread.CurrentCulture
    /// for culture-aware date parsing (a pattern
    /// that is problematic in .NET 8).
    /// </summary>
    public class DateTimeModelBinder
        : IModelBinder
    {
        private static readonly string[]
            Formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "yyyy-MM-dd HH:mm:ss"
            };

        public bool BindModel(
            HttpActionContext actionContext,
            ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(
                    "bindingContext");
            }

            var valueResult = bindingContext
                .ValueProvider
                .GetValue(
                    bindingContext.ModelName);

            if (valueResult == null)
            {
                return false;
            }

            var rawValue =
                valueResult.RawValue
                    as string;

            if (string.IsNullOrWhiteSpace(
                rawValue))
            {
                return false;
            }

            var culture = Thread
                .CurrentThread
                .CurrentCulture;

            DateTime result;
            if (DateTime.TryParseExact(
                rawValue,
                Formats,
                culture,
                DateTimeStyles.None,
                out result))
            {
                bindingContext.Model = result;
                return true;
            }

            if (DateTime.TryParse(
                rawValue,
                culture,
                DateTimeStyles.None,
                out result))
            {
                bindingContext.Model = result;
                return true;
            }

            bindingContext.ModelState
                .AddModelError(
                    bindingContext.ModelName,
                    string.Format(
                        "Cannot parse '{0}' "
                        + "as a date.",
                        rawValue));
            return false;
        }
    }
}
