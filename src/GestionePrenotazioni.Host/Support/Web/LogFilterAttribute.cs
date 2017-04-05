﻿using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Web.Http.Filters;

namespace GestionePrenotazioni.Host.Support.Web
{
    public class LogFilterAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public IExtendedLogger Logger { get; set; }

        public LogFilterAttribute(IExtendedLogger logger)
        {
            Logger = logger;
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //Logger.DebugFormat("Executing action: {0} on controller {1}",
            //    actionContext.ActionDescriptor.ActionName,
            //    actionContext.ActionDescriptor.ControllerDescriptor.ControllerName);

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                Dictionary<String, Object> extraProperties = new Dictionary<string, Object>();
                extraProperties["requri"] = actionExecutedContext.Request.RequestUri.ToString();
                if (actionExecutedContext.ActionContext.ActionArguments != null)
                {
                    foreach (var key in actionExecutedContext.ActionContext.ActionArguments.Keys)
                    {
                        var obj = actionExecutedContext.ActionContext.ActionArguments[key];
                        extraProperties[key] = obj;
                    }
                }
                Logger.ErrorFormat(
                    // @g.rossetti - commentato perchè andava in errore
                    //extraProperties,
                    actionExecutedContext.Exception,
                    "Executing action: {0} on controller {1} - Message: {2}",
                    actionExecutedContext.ActionContext.ActionDescriptor.ActionName,
                    actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    actionExecutedContext.Exception.Message);

            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}