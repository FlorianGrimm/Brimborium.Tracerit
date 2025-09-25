cd /D "%~dp0"
dotnet restore "%~dp0Brimborium.Tracerit.sln"

cd /D "%~dp0src\Brimborium.Tracerit.Collector.Frontend"
CALL npm install

cd /D "%~dp0sample\samplefrontend"
CALL npm install

cd /D "%~dp0"
dotnet build "%~dp0Brimborium.Tracerit.sln"

@echo ~ fin ~