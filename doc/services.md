# Brimborium.Tracerit Services

This document lists the services registered in the dependency injection container and their purposes.

## Core Tracing Services

### ITracorServiceSink / TracorServiceSink

The core tracing service for capturing and processing trace data. Handles scope and level filtering, converts traced values to `TracorDataRecord` instances, and publishes them through the collective publisher.

- **Enabled implementation**: `TracorServiceSink` - Processes trace data through validators and handles reference counting.
- **Disabled implementation**: `DisabledTracorServiceSink` - No-op implementation that disposes values but performs no actual tracing.

### ITracorSink\<TCategoryName\> / TracorSink

Category-scoped tracing interface bound to a specific base scope. Created transiently via `ITracorSink<T>` for type-safe category tracing. Checks enabled state and routes trace events (private/public) through the service sink.

### ITracorCollectivePublisher / TracorCollectivePublisher

Publisher service that manages multiple subscribed sinks and distributes trace events to all of them. Implements the pub/sub pattern for trace data distribution.

### ITracorCollectiveSink

Interface implemented by sinks that receive all trace events. Multiple implementations can be registered and subscribed to the publisher:
- `TracorCollectiveFileSink` - Writes to JSON Lines files
- `TracorCollectiveHttpSink` - Sends to remote HTTP endpoint
- `TracorValidator` - Validates trace data against expressions

## Validation Services

### ITracorValidator / TracorValidator

Validates trace data against defined expressions and manages validation paths. Used for testing scenarios to verify expected trace sequences. Also registered as `ITracorCollectiveSink` to receive trace events.

- **Disabled implementation**: `DisabledTracorValidator` - No-op implementation for production.

## Data Conversion Services

### ITracorDataConvertService / TracorDataConvertService

Service for converting arbitrary objects to trace data properties. Manages type-specific converters for both public and private trace data. Supports reflection-based conversion when enabled.

### LateTracorDataConvertService

Deferred initialization service for `ITracorDataConvertService` that resolves converters lazily.

## Activity Listener Services

### ITracorActivityListener

Listener service for tracing activities from `System.Diagnostics.ActivitySource`. Provides methods to start/stop listening and manage activity source subscriptions.

- **Enabled implementation**: `EnabledTracorActivityListener` - Monitors activity sources and converts activities to trace data.
- **Disabled implementation**: `DisabledTracorActivityListener` - No-op implementation.

## Filter Services

### ITracorScopedFilterFactory / TracorScopedFilterFactory

Factory service that creates `ITracorScopedFilter` instances based on configured providers and filter rules. Determines which log levels are enabled for specific categories and sources.

### ITracorScopedFilter

Scoped filter instances that check if specific log levels are enabled for a source. Created by the factory for each category.

## Logger Integration Services

### TracorLoggerProvider

`ILoggerProvider` implementation that creates `TracorLogger` instances integrating Microsoft.Extensions.Logging with the Tracor tracing system.

## Memory Management Services

### TracorDataRecordPool

Reference count pool for `TracorDataRecord` instances. Reduces memory allocation by reusing record objects.

### TracorMemoryPoolManager

Manages memory pools for efficient buffer allocation in trace data handling.

## Sink Services

### TracorCollectiveFileSink

File-based collective sink service that writes trace data to JSON Lines files with support for periodic rotation, compression, and cleanup.

### TracorCollectiveHttpSink

HTTP-based collective sink service that sends trace data to a remote endpoint with optional compression.

## Diagnostic Services

### TracorEmergencyLogging

Emergency logging service for debugging Tracor itself by writing to `System.Console`. Activated via `TracorOptions.IsEmergencyLogging`.

## Server Services (Brimborium.Tracerit.Server)

### ITracorCollector / TracorCollectorService

Collector service for storing and retrieving trace data records. Maintains a bounded queue of trace records with support for partial retrieval by named clients.

### ITracorCollectorHttpService / TracorCollectorHttpService

HTTP service for receiving trace data over HTTP POST requests. Deserializes JSON trace data and pushes to the collector.

### TracorCollectorToPublisherService

Bridge service that forwards collected trace data to the `ITracorCollectivePublisher` for distribution to sinks.

## Service Registration

Services are registered using extension methods on `IServiceCollection`:

- `AddTracor()` - Adds core Tracor services with configuration
- `AddEnabledTracor()` - Adds enabled tracing services
- `AddDisabledTracor()` - Adds disabled (no-op) tracing services
- `AddTracorInstrumentation<T>()` - Registers instrumentation types
- `AddTracorActivityListener()` - Adds activity listener services
- `AddTracorLogger()` - Adds logger provider integration
- `AddFileTracorCollectiveSinkDefault()` - Adds file sink with default configuration
- `AddHttpTracorCollectiveSink()` - Adds HTTP sink

