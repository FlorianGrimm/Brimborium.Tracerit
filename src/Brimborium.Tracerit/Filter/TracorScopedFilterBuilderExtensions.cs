namespace Brimborium.Tracerit.Filter;

/// <summary>
/// Extension methods for setting up logging services in an <see cref="ITracorScopedFilterBuilder" />.
/// </summary>
public static class TracorScopedFilterBuilderExtensions {

    public static ITracorScopedFilterBuilder AddTracorScopedFilterSource<T>(
        this ITracorScopedFilterBuilder builder)
        where T : class, ITracorScopedFilterSource {
        builder.Services.AddSingleton<ITracorScopedFilterSource, T>();
        return builder;
    }

    public static ITracorScopedFilterBuilder AddTracorScopedFilterSource(
        this ITracorScopedFilterBuilder builder,
        params string[] listSourceName) {
        builder.Services
            .AddOptions<TracorScopedFilterFactoryOptions>()
            .Configure((options) => {
                options.ListSourceName.AddRange(listSourceName);
            });
        return builder;
    }

    public static OptionsBuilder<TracorScopedFilterFactoryOptions> AddOptions(
        this ITracorScopedFilterBuilder builder,
        Action<TracorScopedFilterFactoryOptions> configure
        ) {
        return builder.Services.AddOptions<TracorScopedFilterFactoryOptions>()
            .Configure(configure);
    }

    /// <summary>
    /// Adds services required to consume <see cref="ITracorScopedFilterConfigurationFactory"/> or <see cref="ITracorScopedFilterSourceConfiguration{T}"/>
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to register services on.</param>
    public static void AddTracorScopedFilterConfiguration(
        this ITracorScopedFilterBuilder builder,
        string tracorScopedFilterSection = "") {
        builder.Services.TryAddSingleton<ITracorScopedFilterConfigurationFactory, TracorScopedFilterSourceConfigurationFactory>();
        builder.Services.TryAddSingleton(typeof(ITracorScopedFilterSourceConfiguration<>), typeof(TracorScopedFilterSourceConfiguration<>));
        
        string sectionPath = (string.IsNullOrEmpty(tracorScopedFilterSection)) ? "Logging" : tracorScopedFilterSection;
        builder.Services.AddSingleton<TracorScopedFilterConfiguration>(
            (IServiceProvider serviceProvider) => new TracorScopedFilterConfiguration(
                serviceProvider.GetRequiredService<IConfiguration>().GetSection(sectionPath)));
    }

    /// <summary>
    /// Configures <see cref="TracorScopedFilterOptions" /> from an instance of <see cref="IConfiguration" />.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to use.</param>
    /// <param name="configuration">The <see cref="IConfiguration" /> to add.</param>
    /// <returns>The builder.</returns>
    public static ITracorScopedFilterBuilder AddTracorScopedFilterConfiguration(this ITracorScopedFilterBuilder builder, IConfiguration configuration) {
        builder.AddTracorScopedFilterConfiguration();

        builder.Services.AddSingleton<IConfigureOptions<TracorScopedFilterOptions>>(new TracorScopedFilterConfigureOptions(configuration));
        builder.Services.AddSingleton<IOptionsChangeTokenSource<TracorScopedFilterOptions>>(new ConfigurationChangeTokenSource<TracorScopedFilterOptions>(configuration));

        builder.Services.AddSingleton(new TracorScopedFilterConfiguration(configuration));

        return builder;
    }

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="filter">The filter to be added.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter(this ITracorScopedFilterBuilder builder, Func<string?, string?, LogLevel, bool> filter) =>
        builder.ConfigureFilter(options => options.AddFilter(filter));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="categoryLevelFilter">The filter to be added.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter(this ITracorScopedFilterBuilder builder, Func<string?, LogLevel, bool> categoryLevelFilter) =>
        builder.ConfigureFilter(
            options => options.AddFilter(categoryLevelFilter));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="categoryLevelFilter">The filter to be added.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter<T>(this ITracorScopedFilterBuilder builder, Func<string?, LogLevel, bool> categoryLevelFilter) where T : ITracorScopedFilterSource =>
        builder.ConfigureFilter(
            options => options.AddFilter<T>(categoryLevelFilter));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="levelFilter">The filter to be added.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter(this ITracorScopedFilterBuilder builder, Func<LogLevel, bool> levelFilter) =>
        builder.ConfigureFilter(
            options => options.AddFilter(levelFilter));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="levelFilter">The filter to be added.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter<T>(this ITracorScopedFilterBuilder builder, Func<LogLevel, bool> levelFilter) where T : ITracorScopedFilterSource =>
        builder.ConfigureFilter(
            options => options.AddFilter<T>(levelFilter));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="level">The level to filter.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter(this ITracorScopedFilterBuilder builder, string? category, LogLevel level) =>
        builder.ConfigureFilter(
            options => options.AddFilter(category, level));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="level">The level to filter.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter<T>(this ITracorScopedFilterBuilder builder, string? category, LogLevel level) where T : ITracorScopedFilterSource =>
        builder.ConfigureFilter(
            options => options.AddFilter<T>(category, level));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter(this ITracorScopedFilterBuilder builder, string? category, Func<LogLevel, bool> levelFilter) =>
        builder.ConfigureFilter(
            options => options.AddFilter(category, levelFilter));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static ITracorScopedFilterBuilder AddFilter<T>(this ITracorScopedFilterBuilder builder, string? category, Func<LogLevel, bool> levelFilter) where T : ITracorScopedFilterSource =>
        builder.ConfigureFilter(
            options => options.AddFilter<T>(category, levelFilter));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="filter">The filter function to apply.</param>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter(this TracorScopedFilterOptions builder, Func<string?, string?, LogLevel, bool> filter) =>
        AddRule(builder, filter: filter);

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="categoryLevelFilter">The filter function to apply.</param>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter(this TracorScopedFilterOptions builder, Func<string?, LogLevel, bool> categoryLevelFilter) =>
        AddRule(
            builder,
            filter: (type, name, level) => categoryLevelFilter(name, level));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="categoryLevelFilter">The filter function to apply.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter<T>(this TracorScopedFilterOptions builder, Func<string?, LogLevel, bool> categoryLevelFilter) where T : ITracorScopedFilterSource =>
        AddRule(
            builder,
            type: TracorScopedFilterSource.GetSourceNameFromType(typeof(T)),
            filter: (type, name, level) => categoryLevelFilter(name, level));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter(this TracorScopedFilterOptions builder, Func<LogLevel, bool> levelFilter) =>
        AddRule(
            builder,
            filter: (type, name, level) => levelFilter(level));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter<T>(this TracorScopedFilterOptions builder, Func<LogLevel, bool> levelFilter) where T : ITracorScopedFilterSource =>
        AddRule(
            builder,
            type: TracorScopedFilterSource.GetSourceNameFromType(typeof(T)),
            filter: (type, name, level) => levelFilter(level));

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="level">The level to filter.</param>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter(this TracorScopedFilterOptions builder, string? category, LogLevel level) =>
        AddRule(
            builder,
            category: category,
            level: level);

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="level">The level to filter.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter<T>(this TracorScopedFilterOptions builder, string? category, LogLevel level) where T : ITracorScopedFilterSource =>
        AddRule(
            builder,
            type: TracorScopedFilterSource.GetSourceNameFromType(typeof(T)),
            category: category,
            level: level);

    /// <summary>
    /// Adds a log filter to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="TracorScopedFilterOptions"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <returns>The <see cref="TracorScopedFilterOptions"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter(this TracorScopedFilterOptions builder, string? category, Func<LogLevel, bool> levelFilter) =>
        AddRule(
            builder,
            category: category,
            filter: (type, name, level) => levelFilter(level));

    /// <summary>
    /// Adds a log filter for the given <see cref="ITracorScopedFilterSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITracorScopedFilterBuilder"/> to add the filter to.</param>
    /// <param name="category">The category to filter.</param>
    /// <param name="levelFilter">The filter function to apply.</param>
    /// <typeparam name="T">The <see cref="ITracorScopedFilterSource"/> which this filter will be added for.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilterBuilder"/> so that additional calls can be chained.</returns>
    public static TracorScopedFilterOptions AddFilter<T>(
        this TracorScopedFilterOptions builder,
        string? category,
        Func<LogLevel, bool> levelFilter) where T : ITracorScopedFilterSource =>
        AddRule(
            builder,
            type: TracorScopedFilterSource.GetSourceNameFromType(typeof(T)),
            category: category,
            filter: (type, name, level) => levelFilter(level));

    private static ITracorScopedFilterBuilder ConfigureFilter(
        this ITracorScopedFilterBuilder builder,
        Action<TracorScopedFilterOptions> configureOptions) {
        builder.Services.Configure(configureOptions);
        return builder;
    }

    private static TracorScopedFilterOptions AddRule(TracorScopedFilterOptions options,
        string? type = null,
        string? category = null,
        LogLevel? level = null,
        Func<string?, string?, LogLevel, bool>? filter = null) {
        options.Rules.Add(new TracorScopedFilterRule(type, category, level, filter));
        return options;
    }
}
