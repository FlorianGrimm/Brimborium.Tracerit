# HowTo

## Sample

o sample\Sample\Program.cs

```csharp
        var tracorOptions = builder.Configuration.BindTracorOptionsDefault(new());
        var tracorEnabled = tracorOptions.IsEnabled || startupActions.Testtime;
        builder.Services.AddTracor(
            addEnabledServices: tracorEnabled,
            configureTracor: (tracorOptions) => {
                tracorOptions.SetOnGetApplicationStopping(static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
            },
            configureConvert: null,
            tracorScopedFilterSection: ""
            )
            .AddTracorActivityListener(
                enabled: tracorEnabled,
                configuration: default,
                configure: (options) => { 
                    //options.AllowAllActivitySource = true;
                })
            .AddTracorInstrumentation<SampleInstrumentation>()
            .AddTracorLogger()
            .AddFileTracorCollectiveSinkDefault(
               configuration: builder.Configuration,
               configure: (fileTracorOptions) => {
               })
            .AddTracorCollectiveHttpSink(
               configuration: builder.Configuration,
               configure: (tracorHttpSinkOptions) => {
                    //tracorHttpSinkOptions.TargetUrl = "http://localhost:8080/_api/tracerit/v1/collector.http";
                });
        ;
```

