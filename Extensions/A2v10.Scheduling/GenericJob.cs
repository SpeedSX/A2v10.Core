﻿using A2v10.Scheduling.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Scheduling;

internal class GenericJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    public GenericJob(ILogger<GenericJob> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        var handler = context.MergedJobDataMap.Get("HandlerType") as Type;
        var info = context.MergedJobDataMap.Get("JobInfo") as ScheduledJobInfo;
        if (info == null)
        {
            _logger.LogCritical("'JobInfo' not found");
            return;
        }
        if (handler == null)
        {
            _logger.LogCritical("Handler is null");
            return;
        }
        try
        {
            if (_serviceProvider.GetRequiredService(handler) is IScheduledJob schedulingJob)
                await schedulingJob.ExecuteAsync(info);
            else
                _logger.LogCritical("{handler} is not registered", handler);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Failed to execute {ex}", ex);
        }
    }
}
