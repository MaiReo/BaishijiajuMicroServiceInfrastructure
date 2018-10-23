using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Core.Web.Startup
{
    public class RemovePreFixOperationFilter : IOperationFilter
    {
        protected virtual string Prefix { get; set; }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var prefix = Prefix ?? string.Empty;
            var appServiceName = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ?? string.Empty;
            prefix += appServiceName;
            if (operation.OperationId.StartsWith(prefix))
            {
                operation.OperationId = operation.OperationId.Remove(0, prefix.Length);
            }
            if (operation.OperationId.EndsWith("Post"))
            {
                operation.OperationId = operation.OperationId.Remove(operation.OperationId.LastIndexOf("Post"));
            }
            if (operation.OperationId.EndsWith("Get"))
            {
                operation.OperationId = operation.OperationId.Remove(operation.OperationId.LastIndexOf("Get"));
            }
            if (operation.OperationId.EndsWith("Put"))
            {
                operation.OperationId = operation.OperationId.Remove(operation.OperationId.LastIndexOf("Put"));
            }
            if (operation.OperationId.EndsWith("Delete"))
            {
                operation.OperationId = operation.OperationId.Remove(operation.OperationId.LastIndexOf("Delete"));
            }
        }
    }
}