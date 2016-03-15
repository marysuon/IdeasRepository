using System;
using System.Linq;
using Microsoft.AspNet.Mvc.Filters;

namespace IdeasRepository.Models
{
    public class MultipleButtonsAttribute : ActionFilterAttribute
    {
        public Type EnumType { get; set; }

        public MultipleButtonsAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            foreach (var key in filterContext.HttpContext.Request.Form.Keys)
            {
                if (Enum.IsDefined(EnumType, key))
                {
                    var pDesc = filterContext.ActionDescriptor.Parameters
                        .FirstOrDefault(x => x.ParameterType == EnumType);
                    filterContext.ActionArguments[pDesc.Name] = Enum.Parse(EnumType, key);
                    break;
                }
            }
        }
    }

    public enum ButtonType
    {
        Delete,
        Restore,
        Archive
    }
}
