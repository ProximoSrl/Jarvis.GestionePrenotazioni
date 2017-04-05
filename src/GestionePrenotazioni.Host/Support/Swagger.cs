using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace GestionePrenotazioni.Host.Support
{

    public interface IExamplesProvider
    {
        object GetExamples();
    }

    public class ExamplesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            SetRequestModelExamples(operation, schemaRegistry, apiDescription);
            SetResponseModelExamples(operation, schemaRegistry, apiDescription);
        }

        private static void SetRequestModelExamples(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var requestAttributes = apiDescription.GetControllerAndActionAttributes<SwaggerRequestExamplesAttribute>();

            foreach (var attr in requestAttributes)
            {
                var schema = schemaRegistry.GetOrRegister(attr.ResponseType);

                var request = operation.parameters.FirstOrDefault(p => p.@in == "body" && p.schema.@ref == schema.@ref);

                if (request != null)
                {
                    var provider = (IExamplesProvider)Activator.CreateInstance(attr.ExamplesProviderType);

                    var parts = schema.@ref.Split('/');
                    var name = parts.Last();

                    var definitionToUpdate = schemaRegistry.Definitions[name];

                    if (definitionToUpdate != null)
                    {
                        definitionToUpdate.example = ((dynamic)FormatAsJson(provider))["application/json"];
                    }
                }
            }
        }

        private static void SetResponseModelExamples(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var responseAttributes = apiDescription.GetControllerAndActionAttributes<SwaggerResponseExamplesAttribute>();

            foreach (var attr in responseAttributes)
            {
                var schema = schemaRegistry.GetOrRegister(attr.ResponseType);

                var response =
                    operation.responses.FirstOrDefault(
                        x => x.Value != null && x.Value.schema != null && x.Value.schema.@ref == schema.@ref);

                if (response.Equals(default(KeyValuePair<string, Response>)) == false)
                {
                    if (response.Value != null)
                    {
                        var provider = (IExamplesProvider)Activator.CreateInstance(attr.ExamplesProviderType);
                        response.Value.examples = FormatAsJson(provider);
                    }
                }
            }
        }

        private static object FormatAsJson(IExamplesProvider provider)
        {
            var examples = new Dictionary<string, object>()
            {
                {
                    "application/json", provider.GetExamples()
                }
            };

            return ConvertToCamelCase(examples);
        }

        private static object ConvertToCamelCase(Dictionary<string, object> examples)
        {
            var jsonString = JsonConvert.SerializeObject(examples, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return JsonConvert.DeserializeObject(jsonString);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SwaggerRequestExamplesAttribute : Attribute
    {
        public SwaggerRequestExamplesAttribute(Type responseType, Type examplesProviderType)
        {
            ResponseType = responseType;
            ExamplesProviderType = examplesProviderType;
        }

        public Type ExamplesProviderType { get; private set; }

        public Type ResponseType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerResponseExamplesAttribute : Attribute
    {
        public SwaggerResponseExamplesAttribute(Type responseType, Type examplesProviderType)
        {
            ResponseType = responseType;
            ExamplesProviderType = examplesProviderType;
        }

        public Type ResponseType { get; set; }
        public Type ExamplesProviderType { get; set; }
    }
}
