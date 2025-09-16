// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Logs;

/// <summary>
/// LoggerProvider is the entry point of the OpenTelemetry API. It provides access to <see cref="Logger"/>.
/// </summary>
public class LoggerProvider : BaseProvider {
    private static readonly NoopLogger NoopLogger = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerProvider"/> class.
    /// </summary>
    protected LoggerProvider() {
    }

    /// <summary>
    /// Gets a logger.
    /// </summary>
    /// <remarks><inheritdoc cref="Logger" path="/remarks"/></remarks>
    /// <returns><see cref="Logger"/> instance.</returns>
    public Logger GetLogger()
        => this.GetLogger(name: null, version: null);

    /// <summary>
    /// Gets a logger with the given name.
    /// </summary>
    /// <remarks><inheritdoc cref="Logger" path="/remarks"/></remarks>
    /// <param name="name">Optional name identifying the instrumentation library.</param>
    /// <returns><see cref="Logger"/> instance.</returns>
    public Logger GetLogger(string? name)
        => this.GetLogger(name, version: null);

    /// <summary>
    /// Gets a logger with the given name and version.
    /// </summary>
    /// <remarks><inheritdoc cref="Logger" path="/remarks"/></remarks>
    /// <param name="name">Optional name identifying the instrumentation library.</param>
    /// <param name="version">Optional version of the instrumentation library.</param>
    /// <returns><see cref="Logger"/> instance.</returns>
    public Logger GetLogger(string? name, string? version) {
        if (!this.TryCreateLogger(name, out var logger)) {
            return NoopLogger;
        }

        logger!.SetInstrumentationScope(version);

        return logger;
    }

    /// <summary>
    /// Try to create a logger with the given name.
    /// </summary>
    /// <remarks><inheritdoc cref="Logger" path="/remarks"/></remarks>
    /// <param name="name">Optional name identifying the instrumentation library.</param>
    /// <param name="logger"><see cref="Logger"/>.</param>
    /// <returns><see langword="true"/> if the logger was created.</returns>
    protected virtual bool TryCreateLogger(
        string? name,
        [NotNullWhen(true)] out Logger? logger) {
        logger = null;
        return false;
    }
}
