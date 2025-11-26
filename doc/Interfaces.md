# Interfaces

## Core Tracing Interfaces

### ITracorData
Represents trace data that can be inspected for properties and values.

### ITracorData\<TValue\>
Represents trace data with a strongly-typed original value.

### ITracorDataAccessor
Provides access to properties of objects for tracing purposes.

### ITracorDataAccessor\<T\>
Provides strongly-typed access to properties of objects for tracing purposes.

### ITracorDataAccessorFactory
Factory interface for creating trace data from objects.

### ITracorDataAccessorFactory\<T\>
Generic factory interface for creating trace data from strongly-typed objects.

### ITracorDataSelfAccessor
Interface for objects that can convert themselves for tracing.

### ITracorDataConvertService
Service interface for converting trace data between different formats and property lists.

## Sink Interfaces

### ITracorSink
Represents the tracor sink for capturing and routing trace data with public/private scope support.

### ITracorSink\<TCategoryName\>
Generic tracor sink interface bound to a specific category type.

### ITracorServiceSink
Represents the core tracing interface for capturing and processing trace data.

### ITracorCollectiveSink
Collective sink interface that receives and processes all trace events.

### ITracorCollectivePublisher
Publisher interface for the collective sink with subscription support for multiple sinks.

## Conversion Interfaces

### ITracorConvertObjectToListProperty
Converts objects to a list of tracor data properties.

### ITracorConvertValueToListProperty\<T\>
Converts strongly-typed values to a list of tracor data properties.

### ITracorConvertSelfToListProperty
Interface for objects that can convert themselves to a list of tracor data properties.

## Filtering Interfaces

### ITracorScopedFilter
Filter interface for checking if a specific log level is enabled for a source.

### ITracorScopedFilter\<TCategoryName\>
Generic scoped filter bound to a specific category type.

### ITracorScopedFilterBuilder
Builder interface for configuring scoped filters with service collection access.

### ITracorScopedFilterFactory
Factory interface for creating scoped filters by category name.

### ITracorScopedFilterSource
Interface representing a source for scoped filter configuration.

### ITracorScopedFilterSourceConfiguration
Interface for accessing the configuration of a filter source.

### ITracorScopedFilterSourceConfiguration\<T\>
Generic configuration interface bound to a specific type.

### ITracorScopedFilterConfigurationFactory
Factory interface for retrieving configuration by provider type.

## Validation Interfaces

### ITracorValidator
Validates trace data against defined expressions and manages validation paths.

### ITracorValidatorPath
Represents a validation path that tracks the progress of validation expressions.

### IValidatorExpression
Represents a validation expression that can process trace events and determine success or failure.

### IExpressionCondition
Represents a condition that can be evaluated against trace data to determine if it matches specific criteria.

### IExpressionCondition\<T\>
Represents a strongly-typed condition that can be evaluated against trace data.

## Activity Listener Interfaces

### ITracorActivityListener
Listener interface for tracing activities from various activity sources. Provides methods to start/stop listening and manage activity source subscriptions.

## Diagnostics Interfaces

### IActivitySourceResolver
Resolves an ActivitySource from a service provider.

### IInstrumentation
Base interface for instrumentation that provides access to an ActivitySource.

## Builder Interfaces

### ITracorBuilder
Builder interface for configuring Tracor services with access to the service collection.

## Utility Interfaces

### IReferenceCountObject
Interface for reference-counted poolable objects with automatic pool return on dispose.

### IReferenceCountObject\<T\>
Generic interface for reference-counted objects wrapping a value of type T.

### IReferenceCountPool
Base pool interface for accepting returned reference-counted objects.

### IReferenceCountPool\<T\>
Generic pool interface for renting reference-counted objects.

## Server/Collector Interfaces

### ITracorCollector
Collector interface for storing and retrieving trace data records.

### ITracorCollectorHttpService
Service interface for receiving trace data over HTTP.

### IController
Interface for ASP.NET Core endpoint controllers to map their endpoints.

### IResponseWrapper
Marker interface for API response wrappers.

### IResponseFailed
Interface for failed API responses containing an error message.
