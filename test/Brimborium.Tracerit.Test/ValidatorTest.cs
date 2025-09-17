namespace Brimborium.Tracerit.Test;

using static Brimborium.Tracerit.TracorExtension;

public class ValidatorTest {
    [Test]
    public async Task SequenceExpression001() {
        ServiceCollection serviceBuilder = new();
        serviceBuilder.AddLogging(options => {
            options.AddConsole();
        });
        serviceBuilder.AddTesttimeTracor(options => {
            options.AddTracorDataAccessorByType<Uri>(new TracorDataAccessorFactory<Uri>(new SystemUriTracorDataAccessor()));
            options.AddTracorDataAccessorByType<string>(new ValueAccessorFactory<string>());
        });
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracor tracor = serviceProvider.GetRequiredService<ITracor>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new SequenceExpression()
            .Add(new MatchExpression(
                condition: new PredicateTracorDataCondition(
                    static (data) =>
                        data.TryGetPropertyValue<string>("PathAndQuery", out var pathAndQuery)
                        && "/1" == pathAndQuery)
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
        tracor.Trace(new TracorIdentitfier("Logger", "something"), "something");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation1);
        tracor.Trace(new TracorIdentitfier("Logger", "else"), "else");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation2);
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
        serviceBuilder.AddTesttimeTracor(options => {
            options.AddTracorDataAccessorByType<Uri>(new TracorDataAccessorFactory<Uri>(new SystemUriTracorDataAccessor()));
            options.AddTracorDataAccessorByType<string>(new ValueAccessorFactory<string>());
        });
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracor tracor = serviceProvider.GetRequiredService<ITracor>();
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
        tracor.Trace(new TracorIdentitfier("Logger", "something"), "something");
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation1);
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.Trace(new TracorIdentitfier("Logger", "else"), "else");
        {
            var act = validatorPath.GetFinished((_) => true);
            await Assert.That(act).IsNull();
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation2);
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
        serviceBuilder.AddTesttimeTracor();
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracor tracor = serviceProvider.GetRequiredService<ITracor>();
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
        tracor.Trace(new TracorIdentitfier("Logger", "something"), "something");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation1);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(2);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation3);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(3);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "else"), "else");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation2);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(2);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(1);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation4);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(2);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation5);
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
        serviceBuilder.AddTesttimeTracor();
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        ITracor tracor = serviceProvider.GetRequiredService<ITracor>();
        ITracorValidator tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        var validatorPath = tracorValidator
        .Add(
            new FilterExpression<Uri>(
                label: "",
                condition: new EqualsTracorDataPropertyCondition<Uri, string>(
                    fnGetProperty: static (Uri value) => value.Host,
                    expectedValue: "def",
                    fnEquality: static (a, b) => a == b,
                    setGlobalState: "Host"
                    ),
                listChild: new FilterExpression<Uri>(
                    label:"",
                    condition:
                        new AlwaysCondition<Uri,string>(
                            fnGetProperty: static (value) =>value.Host,
                            setGlobalState: "Host2"),
                    listChild:[
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
        tracor.Trace(new TracorIdentitfier("Logger", "something"), "something");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation1);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation3);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "else"), "else");
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation2);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(1);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(0);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation4);
        {
            await Assert.That(validatorPath.GetListRunnging().Count).IsEqualTo(0);
            await Assert.That(validatorPath.GetListFinished().Count).IsEqualTo(1);
        }
        tracor.Trace(new TracorIdentitfier("Logger", "Page.Location"), uriPageLocation5);
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
