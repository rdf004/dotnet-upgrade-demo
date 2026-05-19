using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ContosoCommerce.Api.ModelBinders
{
    public class DateTimeModelBinder
        : IModelBinder
    {
        private static readonly string[]
            Formats =
            {
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "MM/dd/yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "dd-MMM-yyyy",
                "yyyy-MM-dd HH:mm:ss"
            };

        public Task BindModelAsync(
            ModelBindingContext context)
        {
            if (context == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(context));
            }

            var modelName =
                context.ModelName;
            var valueResult =
                context.ValueProvider
                    .GetValue(modelName);

            if (valueResult
                == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            context.ModelState.SetModelValue(
                modelName, valueResult);

            var value =
                valueResult.FirstValue;
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (DateTime.TryParseExact(
                value, Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
            {
                context.Result =
                    ModelBindingResult
                        .Success(parsed);
                return Task.CompletedTask;
            }

            if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles
                    .AdjustToUniversal,
                out var fallback))
            {
                context.Result =
                    ModelBindingResult
                        .Success(fallback);
                return Task.CompletedTask;
            }

            context.ModelState
                .TryAddModelError(
                    modelName,
                    string.Format(
                        "Cannot convert"
                        + " '{0}' to DateTime",
                        value));

            return Task.CompletedTask;
        }
    }

    public class DateTimeModelBinderProvider
        : IModelBinderProvider
    {
        public IModelBinder GetBinder(
            ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(context));
            }

            if (context.Metadata.ModelType
                == typeof(DateTime)
                || context.Metadata.ModelType
                    == typeof(DateTime?))
            {
                return
                    new DateTimeModelBinder();
            }

            return null;
        }
    }
}
