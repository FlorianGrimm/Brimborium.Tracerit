IF NOT EXIST c:\data\. ( mkdir c:\data )
IF NOT EXIST c:\data\LogFiles\. ( mkdir c:\data\LogFiles )

cd C:\prgs\Brimborium.Tracerit.Collector

sc query Tracerit
IF NOT "%ERRORLEVEL%" == "0" (
sc create Tracerit type= own start= delayed-auto binPath= "C:\prgs\Brimborium.Tracerit.Collector\Brimborium.Tracerit.Collector.exe" obj= "NT SERVICE\Tracerit"
)

icacls "C:\prgs\Brimborium.Tracerit.Collector" /grant "NT SERVICE\Tracerit":(R) /T

icacls "c:\data\LogFiles" /grant "NT SERVICE\Tracerit":(M) /T

sc start Tracerit


REM netsh advfirewall firewall add rule name="Tracerit" dir=in action=allow program="C:\prgs\Brimborium.Tracerit.Collector\Brimborium.Tracerit.Collector.exe" service="Tracerit" description="Tracerit" protocol=TCP localport=4319

REM sc query Tracerit

REM sc stop Tracerit