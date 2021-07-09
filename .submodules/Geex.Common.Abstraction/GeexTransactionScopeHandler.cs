﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Processing;

using Microsoft.Extensions.DependencyInjection;

namespace Geex.Common
{
    public class GeexTransactionScopeHandler : ITransactionScopeHandler
    {
        public virtual ITransactionScope Create(IRequestContext context)
        {
            return new GeexTransactionScope(context, new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
            }, TransactionScopeAsyncFlowOption.Enabled));
        }
    }

    public class GeexTransactionScope : ITransactionScope
    {
        public GeexTransactionScope(IRequestContext context, TransactionScope transaction)
        {
            this.Context = context;
            this.Transaction = transaction;
            this.UowServices = context.Services.GetServices<IUnitOfWork>();
        }

        public IEnumerable<IUnitOfWork> UowServices { get; set; }

        protected IRequestContext Context { get; }

        protected TransactionScope Transaction { get; }

        public void Complete()
        {
            bool flag;
            if (this.Context.Result is QueryResult result && result.Data != null)
            {
                IReadOnlyList<IError> errors = result.Errors;
                if (errors == null || errors.Count == 0)
                {
                    flag = true;
                    goto label_4;
                }
            }
            flag = false;
            label_4:
            if (!flag)
                return;
            foreach (var unitOfWork in UowServices)
            {
                unitOfWork.CommitAsync().Wait();
            }
            this.Transaction.Complete();
        }

        public void Dispose() => this.Transaction.Dispose();
    }

    public interface IUnitOfWork
    {
        Task CommitAsync();
    }

    public class WrapperUnitOfWork : IUnitOfWork
    {
        private readonly Func<Task> _task;

        public WrapperUnitOfWork(Func<Task> task)
        {
            _task = task;
        }
        public async Task CommitAsync()
        {
            await _task.Invoke();
        }
    }
}