﻿using FreeSql;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OvOv.FreeSql.Repository
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionalAttribute : DynamicProxyAttribute, IActionFilter
    {
        public Propagation Propagation { get; set; } = Propagation.Requierd;
        public IsolationLevel? IsolationLevel { get; set; }

        [DynamicProxyFromServices]
        UnitOfWorkManager _uowManager;
        IUnitOfWork _uow;

        public override Task Before(DynamicProxyBeforeArguments args) => OnBefore(_uowManager);
        public override Task After(DynamicProxyAfterArguments args) => OnAfter(args.Exception);

        //这里是为了 controller 
        public void OnActionExecuting(ActionExecutingContext context) => OnBefore(context.HttpContext.RequestServices.GetService(typeof(UnitOfWorkManager)) as UnitOfWorkManager);
        public void OnActionExecuted(ActionExecutedContext context) => OnAfter(context.Exception);


        Task OnBefore(UnitOfWorkManager uowm)
        {
            _uow = uowm.Begin(this.Propagation, this.IsolationLevel);
            return Task.FromResult(false);
        }
        Task OnAfter(Exception ex)
        {
            try
            {
                if (ex == null) _uow.Commit();
                else _uow.Rollback();
            }
            finally
            {
                _uow.Dispose();
            }
            return Task.FromResult(false);
        }
    }
}