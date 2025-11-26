// MIT - Florian Grimm

using System.Diagnostics;
using System.Threading.Tasks;
using Brimborium.Tracerit;
using Brimborium.Tracerit.Server;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample.WebApp;

public class TestingServersService : TUnit.Core.Interfaces.IAsyncInitializer, IAsyncDisposable {
    private readonly TaskCompletionSource<Process> _TcsRunningServerToTest = new();
    private Task _TaskRunServerToTest = Task.CompletedTask;
    private Task _TaskRunServerForReceiving = Task.CompletedTask;
    private Process? _ProcessRunServerToTest;

    private WebApplication? _AppServerForReceiving;
    private readonly TaskCompletionSource<WebApplication> _TcsServerForReceiving = new();

    public bool IsInitialized { get; private set; }

    public WebApplication GetApplication() => this._AppServerForReceiving ?? throw new InvalidOperationException("Application yet is not set");
    public IServiceProvider GetServices() => this.GetApplication().Services;

    private ITracorServiceSink? _Tracor;
    public ITracorServiceSink GetTracor() => this._Tracor
        ??= this.GetServices().GetRequiredService<ITracorServiceSink>();

    private ITracorValidator? _TracorValidator;
    public ITracorValidator GetTracorValidator() => this._TracorValidator
        ??= this.GetServices().GetRequiredService<ITracorValidator>();

    public async Task InitializeAsync() {
        if (this.IsInitialized) { return; }
        this.IsInitialized = true;
        try {
            this._TaskRunServerToTest = this.RunServerToTest().ContinueWith(t => {
                if (t.IsFaulted) {
                    string message = t.Exception.ToString();
                    if (message is { }) {
                        System.Console.Error.WriteLine(message);
                        var output = TestContext.Current?.Output;
                        output?.WriteLine(message);
                    }
                }
            });
            this._TaskRunServerForReceiving = this.RunServerForReceiving().ContinueWith(t => {
                if (t.IsFaulted) {
                    string message = t.Exception.ToString();
                    if (message is { }) {
                        System.Console.Error.WriteLine(message);
                        var output = TestContext.Current?.Output;
                        output?.WriteLine(message);
                    }
                }
            });
            await Task.Delay(100);
            this._ProcessRunServerToTest = await this._TcsRunningServerToTest.Task.ConfigureAwait(false);
            this._AppServerForReceiving = await this._TcsServerForReceiving.Task.ConfigureAwait(false);
        } catch (Exception error) {
            string message = error.ToString();
            if (message is { }) {
                System.Console.Error.WriteLine(message);
                var output = TestContext.Current?.Output;
                output?.WriteLine(message);
            }
            throw;
        }
    }


    public bool IsRunning() {
        if (this._ProcessRunServerToTest is null) { return false; }
        if (!this._TcsRunningServerToTest.Task.IsCompleted) { return false; }
        if (this._ProcessRunServerToTest.HasExited) { return false; }
        if (this._TaskRunServerToTest.IsCompleted) { return false; }
        return true;
    }

    public async ValueTask DisposeAsync() {
        if (this._ProcessRunServerToTest is not { } process) { return; }

        this._ProcessRunServerToTest = null;
        process.Kill();

        await this._TaskRunServerToTest.ConfigureAwait(false);

        System.GC.SuppressFinalize(this);
    }

    public Task RunServerToTest() {
        var pathToCsproj = Sample.WebApp.Program.GetCsproj();
        return this.RunServerToTest(pathToCsproj);
    }

    public async Task RunServerToTest(string pathToCsproj) {
        try {
            await Assert.That(System.IO.File.Exists(pathToCsproj)).IsTrue().Because(pathToCsproj);
            System.Diagnostics.ProcessStartInfo psi = new() {
                FileName = @"C:\Program Files\dotnet\dotnet.exe",
                WorkingDirectory = System.IO.Path.GetDirectoryName(pathToCsproj),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("run");
            psi.ArgumentList.Add("--no-build"); // this has an dependency to it
            psi.ArgumentList.Add("--project");
            psi.ArgumentList.Add(pathToCsproj);
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add("--ParentTestingProcess");
            psi.ArgumentList.Add(System.Diagnostics.Process.GetCurrentProcess().Id.ToString());

            var process = System.Diagnostics.Process.Start(psi);
            if (process is null) { throw new Exception("Cannot start process"); }
            if (process.HasExited) { throw new Exception("process died"); }
            this._ProcessRunServerToTest = process;

            bool foundStartingMessage = false;
            var taskStdOut = process.StandardOutput.ReadLineAsync();
            var taskStdError = process.StandardError.ReadLineAsync();
            while (!process.HasExited && !foundStartingMessage) {
                var task = await Task.WhenAny(taskStdOut, taskStdError);
                if (ReferenceEquals(task, taskStdOut)) {
                    var message = await task;
                    var output = TestContext.Current?.Output;
                    if (message is { }) {
                        output?.WriteLine(message);
                        System.Console.Out.WriteLine(message);
                    }
                    if (message is { Length: > 0 }) {
                        if (message.Contains("Application started. Press Ctrl+C to shut down.")) {
                            this._TcsRunningServerToTest.TrySetResult(process);
                            // foundStartingMessage = true;
                        }
                        if (message.Contains("System.IO.IOException: Failed to bind to address")) {
                            throw new Exception(message);
                        }
                    }

                    taskStdOut = process.StandardOutput.ReadLineAsync();
                } else if (ReferenceEquals(task, taskStdOut)) {
                    var message = await task;
                    if (message is { }) {
                        var output = TestContext.Current?.Output;
                        output?.WriteLine(message);
                        System.Console.Error.WriteLine(message);
                    }
                    taskStdError = process.StandardError.ReadLineAsync();
                } else {
                    await task;
                }
            }
            await process.WaitForExitAsync();
        } catch (Exception ex) {
            this._TcsRunningServerToTest.TrySetException(ex);
        }
    }

    public async Task RunServerForReceiving() {
        try {
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile(@"C:\github\FlorianGrimm\Brimborium.Tracerit-3\sample\Sample.OOP.Test\appsettings.json");
            builder.Configuration.AddJsonFile(@"C:\github\FlorianGrimm\Brimborium.Tracerit-3\sample\Sample.OOP.Test\appsettings.Development.json");
            builder.Services.AddLogging((loggingBuilder) => {
                loggingBuilder.AddConsole();
            });
            builder.Services.AddTracor(
                addEnabledServices: true,
                configureTracor: (tracorOptions) => { },
                configureConvert: (tracorDataConvertOptions) => { }
                ).
                AddTracorValidatorService(
                configure: (tracorValidatorServiceOptions) => {
                    // tracorValidatorServiceOptions.
                });
            builder.Services.AddTracorCollector();
            builder.AddTracorCollectorMinimal();
            builder.WebHost.UseKestrel();
            WebApplication app = builder.Build();
            app.MapGet("/", () => $"TestServer {System.DateTime.Now:o}");
            app.MapTracorControllerEndpoints();
            var taskRun = app.RunAsync();
            this._AppServerForReceiving = app;
            this._TcsServerForReceiving.TrySetResult(app);
            await taskRun;
        } catch (Exception error) {
            this._TcsServerForReceiving.TrySetException(error);
        }
    }
}