using System.Web.Http;
using ContosoCommerce.Api.ModelBinders;
using ContosoCommerce.Orders.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ContosoCommerce.Api.App_Start
{
    /// <summary>
    /// Configures Web API routes, formatters,
    /// message handlers, and model binders.
    /// </summary>
    public static class WebApiConfig
    {
        public static void Register(
            HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate:
                    "api/{controller}/{id}",
                defaults: new
                {
                    id = RouteParameter
                        .Optional
                });

            var json = config.Formatters
                .JsonFormatter;
            json.SerializerSettings
                .ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings
                .Formatting =
                    Formatting.Indented;
            json.SerializerSettings
                .NullValueHandling =
                    NullValueHandling.Ignore;
            json.SerializerSettings
                .Converters.Add(
                    new StringEnumConverter());

            config.Formatters.Remove(
                config.Formatters
                    .XmlFormatter);

            config.MessageHandlers.Add(
                new OrderNotificationHandler());

            config
                .BindParameter(
                    typeof(System.DateTime),
                    new DateTimeModelBinder());
            config
                .BindParameter(
                    typeof(System.DateTime?),
                    new DateTimeModelBinder());
        }
    }
}
