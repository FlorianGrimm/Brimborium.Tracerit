namespace Brimborium.Tracerit.Test;

using static Brimborium.Tracerit.TracorExtension;

public class ValidatorTest {
    [Test]
    public async Task SequenceExpression001() {
        ServiceCollection serviceBuilder = new();
        serviceBuilder.AddLogging(options => {
            options.AddConsole();
        });
        serviceBuilder.AddEnabledTracor(
            configureConvert: options => {
                options.AddTracorDataAccessorByTypePublic<Uri>(new BoundAccessorTracorDataFactory<Uri>(new SystemUriTracorDataAccessor()));
                options.AddTracorDataAccessorByTypePublic<string>(new ValueAccessorFactory<string>());
            });
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracorServiceSink tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new SequenceExpression()
            .Add(new MatchExpression(
                condition: new PredicateTracorDataCondition(
                    static (data) => data.IsEqualString("PathAndQuery", "/1"))
                )
            )
            .Add(new MatchExpression() {
                Condition = new PredicateTracorDataCondition(
                    static (data) =>
                        data.TryGetPropertyValue<string>("PathAndQuery", out var pathAndQuery)
                        && "/2" == pathAndQuery)
            }
            )
        );
        Uri uriPageLocation1 = new("https://localhost/1", UriKind.Absolute);
        Uri uriPageLocation2 = new("https://localhost/2", UriKind.Absolute);
        tracor.TracePublic("something", LogLevel.Information, "test", "something");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation1);
        tracor.TracePublic("else", LogLevel.Information, "test", "else");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation2);
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNotNull();
        }
    }

    [Test]
    public async Task SequenceExpression002() {
        ServiceCollection serviceBuilder = new();
        serviceBuilder.AddLogging(options => {
            options.AddConsole();
        });
        serviceBuilder.AddEnabledTracor(
            configureConvert: options => {
            options.AddTracorDataAccessorByTypePublic<Uri>(new BoundAccessorTracorDataFactory<Uri>(new SystemUriTracorDataAccessor()));
            options.AddTracorDataAccessorByTypePublic<string>(new ValueAccessorFactory<string>());
        });
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracorServiceSink tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new SequenceExpression()
            .Add(new MatchExpression() {
                Condition = new PredicateTracorDataCondition(
                    static (data) =>
                        data.TryGetPropertyValue<string>("PathAndQuery", out var pathAndQuery)
                        && "/1" == pathAndQuery)
            })
            .Add(new MatchExpression() {
                Condition = new PredicateTracorDataCondition(
                    static (data) =>
                        data.TryGetPropertyValue<string>("PathAndQuery", out var pathAndQuery)
                        && "/2" == pathAndQuery)
            })
        );
        Uri uriPageLocation1 = new("https://localhost/1", UriKind.Absolute);
        Uri uriPageLocation2 = new("https://localhost/2", UriKind.Absolute);
        tracor.TracePublic("something", LogLevel.Information, "test", "something");
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation1);
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.TracePublic("else", LogLevel.Information, "test", "else");
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation2);
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNotNull();
        }
    }

    [Test]
    public async Task GroupByExpression003() {
        ServiceCollection serviceBuilder = new();
        serviceBuilder.AddLogging(static options => {
            options.AddConsole();
        });
        serviceBuilder.AddEnabledTracor();
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracorServiceSink tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new GroupByExpression<string>() {
                PropertyName = "Host"
            }
            .Add(
                new SequenceExpression()
                + PredicateValue(static (Uri data) => "/1" == data.PathAndQuery).AsMatch()
                + PredicateValue(static (Uri data) => "/2" == data.PathAndQuery).AsMatch()
            )
        );
        Uri uriPageLocation1 = new("https://abc/1", UriKind.Absolute);
        Uri uriPageLocation2 = new("https://abc/2", UriKind.Absolute);
        Uri uriPageLocation3 = new("https://def/1", UriKind.Absolute);
        Uri uriPageLocation4 = new("https://def/2", UriKind.Absolute);
        Uri uriPageLocation5 = new("https://ghi/1", UriKind.Absolute);
        tracor.TracePublic("something", LogLevel.Information, "test", "something");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation1);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(2);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation3);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(3);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.TracePublic("else", LogLevel.Information, "test", "else");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation2);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(2);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(1);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation4);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(2);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation5);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(2);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(2);
        }
        {
            var act = validatorPath.GetFinished((s) => s.TryGetValue("Host", out var host) && host is string txtHost && "abc" == txtHost);
            await Assert.That(act).IsNotNull();
        }
        {
            var act = validatorPath.GetFinished((s) => s.TryGetValue("Host", out var host) && host is string txtHost && "def" == txtHost);
            await Assert.That(act).IsNotNull();
        }
        {
            var act = validatorPath.GetFinished((s) => s.TryGetValue("Host", out var host) && host is string txtHost && "ghi" == txtHost);
            await Assert.That(act).IsNull();
        }
    }


    [Test]
    public async Task FilterExpression004() {
        ServiceCollection serviceBuilder = new();
        serviceBuilder.AddLogging(static options => {
            options.AddConsole();
        });
        serviceBuilder.AddEnabledTracor();
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracorServiceSink tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new FilterExpression(
                label: "",
                condition: new EqualsTracorDataPropertyCondition<Uri, string>(
                    fnGetProperty: static (Uri value) => value.Host,
                    expectedValue: "def",
                    fnEquality: static (a, b) => a == b,
                    setGlobalState: "Host"
                    ),
                listChild: new FilterExpression(
                    label: "",
                    condition:
                        new AlwaysCondition<Uri, string>(
                            fnGetProperty: static (value) => value.Host,
                            setGlobalState: "Host2"),
                    listChild: [
                        EqualsValue(static (Uri data) => data.PathAndQuery, "/1").AsMatch(),
                        EqualsValue(static (Uri data) => data.PathAndQuery, "/2").AsMatch()]
                )
            )
        );
        Uri uriPageLocation1 = new("https://abc/1", UriKind.Absolute);
        Uri uriPageLocation2 = new("https://abc/2", UriKind.Absolute);
        Uri uriPageLocation3 = new("https://def/1", UriKind.Absolute);
        Uri uriPageLocation4 = new("https://def/2", UriKind.Absolute);
        Uri uriPageLocation5 = new("https://ghi/1", UriKind.Absolute);
        tracor.TracePublic("something", LogLevel.Information, "test", "something");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation1);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation3);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.TracePublic("else", LogLevel.Information, "test", "else");
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation2);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation4);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(0);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(1);
        }
        tracor.TracePublic("Page.Location", LogLevel.Information, "test", uriPageLocation5);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(0);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(1);
        }
        {
            var act = validatorPath.GetFinished((s) => s.TryGetValue("Host", out var host) && host is string txtHost && "def" == txtHost);
            await Assert.That(act).IsNotNull();
        }
        {
            var act = validatorPath.GetFinished((s) => s.TryGetValue("Host", out var host) && host is string txtHost && "ghi" == txtHost);
            await Assert.That(act).IsNull();
        }
    }
}
