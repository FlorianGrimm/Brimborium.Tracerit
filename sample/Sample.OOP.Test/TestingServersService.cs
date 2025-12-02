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
    private record ProcessServerToTest(Process Process, bool Own);
    private readonly TaskCompletionSource<ProcessServerToTest> _TcsRunningServerToTest = new();
    private Task _TaskRunServerToTest = Task.CompletedTask;
    private Task _TaskRunServerForReceiving = Task.CompletedTask;
    private ProcessServerToTest? _ProcessRunServerToTest;

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
        if (this._ProcessRunServerToTest?.Process is null) { return false; }
        if (!this._TcsRunningServerToTest.Task.IsCompleted) { return false; }
        if (this._ProcessRunServerToTest.Process.HasExited) { return false; }
        if (this._TaskRunServerToTest.IsCompleted) { return false; }
        return true;
    }

    public async ValueTask DisposeAsync() {
        if (this._ProcessRunServerToTest is not { } process) { return; }

        this._ProcessRunServerToTest = null;
        process.Process.Kill();

        await this._TaskRunServerToTest.ConfigureAwait(false);

        System.GC.SuppressFinalize(this);
    }

    public Task RunServerToTest() {
        var pathToCsproj = Sample.WebApp.Program.GetCsproj();
        return this.RunServerToTest(pathToCsproj);
    }

    public async Task RunServerToTest(string pathToCsproj) {
        try {
            var listProcess = System.Diagnostics.Process.GetProcessesByName("SampleForTesting");
            if (listProcess is { Length: > 0 }) {
                // already running
                this._TcsRunningServerToTest.SetResult(new(listProcess[0], false));
            } else {

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
                psi.ArgumentList.Add(System.Environment.ProcessId.ToString());

                var process = System.Diagnostics.Process.Start(psi);
                if (process is null) { throw new Exception("Cannot start process"); }
                if (process.HasExited) { throw new Exception("process died"); }
                this._ProcessRunServerToTest = new(process,true);

                //var taskReadStdOutError = Task.Run(async () => { await ReadStdOutError(); });
                process.OutputDataReceived += (sender, args) => {
                    var message = args.Data;
                    var output = TestContext.Current?.Output;
                    if (message is { }) {
                        output?.WriteLine(message);
                        //System.Console.Out.WriteLine(message);
                    }
                    if (message is { Length: > 0 }) {
                        if (message.Contains("Application started. Press Ctrl+C to shut down.")) {
                            this._TcsRunningServerToTest.TrySetResult(new(process, true));
                            /*
                            process.CancelOutputRead();
                            process.CancelErrorRead();
                            */
                        }
                        if (message.Contains("System.IO.IOException: Failed to bind to address")) {
                            throw new Exception(message);
                        }
                    }
                };
                process.ErrorDataReceived += (sender, args) => {
                    var message = args.Data;
                    var output = TestContext.Current?.Output;
                    if (message is { }) {
                        output?.WriteLine(message);
                        //System.Console.Out.WriteLine(message);
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                //await taskReadStdOutError;
                await process.WaitForExitAsync();
            }
#if false
            async Task ReadStdOutError() {
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
            }
#endif
        } catch (Exception ex) {
            this._TcsRunningServerToTest.TrySetException(ex);
        }
    }

    public async Task RunServerForReceiving() {
        await this.RunServerForReceiving((builder) => {
            var projectSourceCodeFolder = TestUtility.GetProjectSourceCodeFolder();
            builder.Configuration.AddJsonFile(
                path:System.IO.Path.Combine(projectSourceCodeFolder, @"appsettings.json"),
                optional: false);
            builder.Configuration.AddJsonFile(
                path:System.IO.Path.Combine(projectSourceCodeFolder, @"appsettings.Development.json"),
                optional: false);
        });
    }

    public async Task RunServerForReceiving(
        Action<WebApplicationBuilder> configWebApplicationBuilder
        ) {
        try {
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
            configWebApplicationBuilder(builder);
            builder.Services.AddLogging((loggingBuilder) => {
                loggingBuilder.AddSimpleConsole();
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