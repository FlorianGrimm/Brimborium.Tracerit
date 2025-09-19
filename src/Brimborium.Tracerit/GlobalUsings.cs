global using global::System;
global using global::System.Collections;
global using global::System.Collections.Concurrent;
global using global::System.Collections.Generic;
global using global::System.Collections.Immutable;
global using global::System.Diagnostics;
global using global::System.Diagnostics.CodeAnalysis;
global using global::System.Linq;
global using global::System.Runtime.CompilerServices;
global using global::System.Text;
global using global::System.Threading.Tasks;

global using global::Microsoft.Extensions.Logging;
global using global::Microsoft.Extensions.Primitives;
global using global::Microsoft.Extensions.Configuration;
global using global::Microsoft.Extensions.DependencyInjection;
global using global::Microsoft.Extensions.DependencyInjection.Extensions;
global using global::Microsoft.Extensions.Options;

global using global::Brimborium.Tracerit;
global using global::Brimborium.Tracerit.Condition;
global using global::Brimborium.Tracerit.DataAccessor;
global using global::Brimborium.Tracerit.Expression;
global using global::Brimborium.Tracerit.Logger;
global using global::Brimborium.Tracerit.Service;
global using global::Brimborium.Tracerit.Utility;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Brimborium.Tracerit.Test")]
//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Brimborium.Tracerit.AspNetCore")]
