using System;
using Microsoft.AspNetCore.Mvc.Filters;
using EKIFVK.ChemicalLab.Controllers;
using EKIFVK.ChemicalLab.Services.Verification;

namespace EKIFVK.ChemicalLab.Attributes {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class PermissionCheckAttribute : ActionFilterAttribute {
        private readonly string _permission;

        public PermissionCheckAttribute(string permission) {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!(context.Controller is VerifiableController)) {
                throw new NotSupportedException("PermissionCheckAttribute only supports Controller that inherts VerifiableController");
            }
            var controller = (VerifiableController) context.Controller;
            var result = controller.Verifier.Check(controller.CurrentUser, _permission, controller.CurrentAddress);
            if (result != VerificationResult.Pass) {
                context.Result = controller.RejectedResponse(result, _permission);
            }
            base.OnActionExecuting(context);
        }
    }
}