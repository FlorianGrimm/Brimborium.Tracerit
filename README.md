# Brimborium.Tracerit

Utility for testing.

This library is not production ready.

Using Brimborium.Tracerit you can await for logs within your tests.
Their is no garantee, that the log does what it says

## Goal

Provide a way to validate the execution of code - in development, integration tests and production, if needed - e.g. to find a bug.

## Background

Logging, open telemetry and monitoring are usefull in production.
For development, testing at devopment time or search for bugs in production this library tries to provide possibilities to trace the execution of code.

The "normal" logging (Microsoft.Logging.ILogger) and Activity (System.Diagnostics.Activity) should be used.
Expensive logging (e.g. an big array) is not usefull and slow in execution and slows down the development.
Sometimes you shouln't log the data, because it is secret or personal data.
The Trace-method should help here. 
(Unclear how) The data should be filtered and collected in a way so it is usefull and not dangerous.

Brimborium.Tracerit.ITracor.Trace is used to trace state within a operation
The TracorLoggerProvider and TracorLogger is also used to collect traces.
- The traces can be collected and can be analyzed later (while developing).
- The traces can be used to validate the execution of code (while developing / integration tests).
- The traces can be used to find bugs in production (if the traces are collected in production).

# Roadmap

Build the library, test it.
Build samples - may be their is an open-source application, that can be used as sample.
Build tools to analyze the traces and generate rules.
Revisit the design.

# Open Questions

- How to avoid leaking of secret or personal data?
- How to extract the usefull data from the traces?
- Avoid persitence of traces while development - e.g. in memory only?
- Allow persitence of traces if needed - e.g. in integration tests?
- Avoid persitence of traces in production - e.g. only if needed? only if failed?
- Avoid performance impacts in production, but allow it if needed.
- Are the traces and rules validated with in the same application? Or different application? Or both?

# Todo

Using IConfiguration for everything - but allow code configuration too.
Samples.
Collector-Server/Service
Web-UI to analyze traces and generate rules

# Szenario WebApp

Test Implementation: A ASP.net core web app - with Angular frontend and TUnit.

- samplefrontend - The Angular frontend
- Sample - The server
- Sample.Test - testing with playwright; using one process for the test and the server.
- Sample.OOP.Test - testing with playwright in another process
- SampleForTesting - used by Sample.OOP.Test; implements dangerous things for production - usefull for testing, like another authentication.

Goal
Run the test with the test explorer.
Run the test with the console.
Debug the test and debug the server in one process.
Debug the test and debug the server in two processes.
Debug the test and run the server in two processes.

Using Brimborium.Tracerit you can await for Logs