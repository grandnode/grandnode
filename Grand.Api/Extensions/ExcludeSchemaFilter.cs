using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Grand.Api.Extensions
{
    public class ExcludeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            foreach (var item in context.SchemaRepository.Schemas)
            {
                if (IsRemoveKey(item.Key))
                    context.SchemaRepository.Schemas.Remove(item.Key);
                if (item.Key.Contains("Delta"))
                    context.SchemaRepository.Schemas.Remove(item.Key);
            }

        }
        private bool IsRemoveKey(string key)
        {
            var removeType = new List<string>();
            removeType.Add("Assembly");
            removeType.Add("CallingConventions");
            removeType.Add("ConstructorInfo");
            removeType.Add("CustomAttributeData");
            removeType.Add("CustomAttributeNamedArgument");
            removeType.Add("CustomAttributeTypedArgument");
            removeType.Add("EventAttributes");
            removeType.Add("EventInfo");
            removeType.Add("FieldAttributes");
            removeType.Add("FieldInfo");
            removeType.Add("GenericParameterAttributes");
            removeType.Add("ICustomAttributeProvider");
            removeType.Add("IntPtr");
            removeType.Add("LayoutKind");
            removeType.Add("MemberInfo");
            removeType.Add("MemberTypes");
            removeType.Add("MethodAttributes");
            removeType.Add("MethodBase");
            removeType.Add("MethodImplAttributes");
            removeType.Add("MethodInfo");
            removeType.Add("Module");
            removeType.Add("ModuleHandle");
            removeType.Add("ParameterAttributes");
            removeType.Add("ParameterInfo");
            removeType.Add("PropertyAttributes");
            removeType.Add("PropertyInfo");
            removeType.Add("RuntimeFieldHandle");
            removeType.Add("RuntimeMethodHandle");
            removeType.Add("RuntimeTypeHandle");
            removeType.Add("SecurityRuleSet");
            removeType.Add("StructLayoutAttribute");
            removeType.Add("Type");
            removeType.Add("TypeAttributes");
            removeType.Add("TypeInfo");

            return removeType.Contains(key);
        }
    }
}
