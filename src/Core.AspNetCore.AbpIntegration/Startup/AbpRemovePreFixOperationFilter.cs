using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Web.Startup
{
    public class AbpRemovePreFixOperationFilter : RemovePreFixOperationFilter, IOperationFilter
    {
        protected override string Prefix => "ApiServicesApp";
    }
}
