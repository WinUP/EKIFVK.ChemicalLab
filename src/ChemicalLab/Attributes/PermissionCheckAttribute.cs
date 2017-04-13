using System;
using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using EKIFVK.ChemicalLab.Controllers;

namespace EKIFVK.ChemicalLab.Attributes {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class VerifyAttribute : ActionFilterAttribute {
        private readonly string _permission;

        public VerifyAttribute(string permission) {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!(context.Controller is VerifiableController)) {
                throw new NotSupportedException("VerifyAttribute only supports Controller that inherts VerifiableController");
            }
            var controller = (VerifiableController) context.Controller;
            if (!controller.Verifier.Check(controller.CurrentUser, _permission, out var result, controller.CurrentAddress)) {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Result =
                    controller.Json(new Hashtable {{"data", ""}, {"message", controller.Verifier.ToString(result)}});
            }
            base.OnActionExecuting(context);
        }
    }
}