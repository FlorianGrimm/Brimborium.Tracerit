# Brimborium.Tracerit Classes

## Core Data Types

### TracorDataRecord

The `TracorDataRecord` class represents a record of trace data, including properties and identifiers. It provides methods for managing and accessing trace data properties. Implements `ITracorData` and `ReferenceCountObject` for efficient memory pooling.

### TracorDataProperty

The `TracorDataProperty` struct represents a property in trace data, including its name, type, and value. It provides methods for accessing and converting the property value based on its type (String, Integer, Boolean, Enum, Level, Double, DateTime, DateTimeOffset, Duration, Uuid, Any).

### TracorDataCollection

A collection of `ITracorData` items for trace data management. Respects reference counting for proper memory management through `IReferenceCountObject`.

### TracorIdentifier

A record struct representing a unique identifier for a trace point, containing `SourceProvider`, `Scope`, and `Message` properties.

### TracorIdentifierCache

Provides caching for `TracorIdentifier` child instances to improve performance and reduce memory allocation.

## Configuration Options

### TracorOptions

Main configuration options for Tracor, including `IsEnabled`, `IsEmergencyLogging`, and `ApplicationName` properties. Allows setting up application stopping callbacks for proper flush handling.

### TracorBulkSinkOptions

Base options for bulk sink implementations, including `FlushPeriod` and resource configuration.

### TracorFileSinkOptions

Configuration for file-based trace sinks, including directory paths, file naming patterns, period, compression settings, and cleanup options.

### TracorHttpSinkOptions

Configuration for HTTP-based trace sinks, including `TargetUrl` and compression settings.

### TracorLoggerOptions

Configuration options for the Tracor logger provider, including minimum `LogLevel`.

### TracorActivityListenerOptions

Configuration for the activity listener, controlling which activity sources are monitored and whether start/stop events are enabled.

## Interfaces

### ITracorData

Core interface representing trace data that can be inspected for properties and values. Provides methods for accessing properties by name and converting to property lists.

### ITracorCollectivePublisher

Publisher interface for the collective sink with subscription support for multiple sinks.

### ITracorCollectiveSink

Collective sink interface that receives and processes all trace events from multiple sources.

### ITracorServiceSink

Core tracing interface for capturing and processing trace data with scope and level filtering.

### ITracorSink / ITracorSink<TCategoryName>

Category-scoped interface for checking enabled state and tracing private/public events.

### ITracorValidator

Validates trace data against defined expressions and manages validation paths.

### ITracorValidatorPath

Represents a validation path that tracks the progress of validation expressions.

### ITracorActivityListener

Listener interface for tracing activities from various activity sources.

### ITracorScopedFilter / ITracorScopedFilterFactory

Filter interfaces for checking if specific log levels are enabled for a source and creating scoped filters.

## Sink Implementations

### TracorCollectivePublisher

Default implementation of `ITracorCollectivePublisher` that manages multiple subscribed sinks and distributes trace events.

### TracorCollectiveFileSink

File-based collective sink that writes trace data to JSON Lines files with support for periodic rotation and compression.

### TracorCollectiveHttpSink

HTTP-based collective sink that sends trace data to a remote endpoint.

### TracorValidator

Implementation of `ITracorValidator` that validates trace data against defined expressions.

## Logger Integration

### TracorLoggerProvider

Provides logger instances that integrate with the Tracor tracing system. Creates `TracorLogger` instances that capture logging events as trace data.

## Filter System

### TracorScopedFilterFactory

Produces instances of `ITracorScopedFilter` classes based on configured providers and filter rules.

### TracorScopedFilterOptions

Configuration options for scoped filtering, including minimum log level and filter rules.

### TracorScopedFilterRule

Represents a single filter rule with category name, source name, log level, and optional filter function.

### TracorScopedFilterSource / PublicTracorScopedFilterSource / PrivateTracorScopedFilterSource

Source identifiers for scoped filtering (public and private trace sources).

## Extension Methods

### TracorServiceCollectionExtensions

Extension methods for adding Tracor services to the dependency injection container, including `AddTracor`, `AddEnabledTracor`, `AddDisabledTracor`, and `AddTracorInstrumentation<T>`.

## Utility Classes

### TracorDataRecordPool

A reference count pool for `TracorDataRecord` instances to reduce memory allocation.

### TracorEmergencyLogging

Emergency logging service for debugging Tracor itself by writing to `System.Console`.

### EqualityComparerTracorIdentifier

Case-insensitive equality comparer for `TracorIdentifier` instances.

### MatchEqualityComparerTracorIdentifier

Partial equality comparer for `TracorIdentifier` that only compares non-empty expected properties.

