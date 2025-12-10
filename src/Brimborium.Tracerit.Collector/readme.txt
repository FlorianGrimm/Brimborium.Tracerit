Installation

mkdir C:\prgs\Brimborium.Tracerit.Collector
REM @You: Copy the files into this folder.

REM Create the folder for the log files
IF NOT EXIST c:\data\. ( mkdir c:\data )
IF NOT EXIST c:\data\LogFiles\. ( mkdir c:\data\LogFiles )

REM create the windows service - using a windows service virtual account - so no password is needed.
sc query Tracerit
IF NOT "%ERRORLEVEL%" == "0" (
sc create Tracerit type= own start= delayed-auto binPath= "C:\prgs\Brimborium.Tracerit.Collector\Brimborium.Tracerit.Collector.exe" obj= "NT SERVICE\Tracerit"
)

REM grant rights for the folders
icacls "C:\prgs\Brimborium.Tracerit.Collector" /grant "NT SERVICE\Tracerit":(R) /T

icacls "c:\data\LogFiles" /grant "NT SERVICE\Tracerit":(M) /T

sc start Tracerit


REM netsh advfirewall firewall add rule name="Tracerit" dir=in action=allow program="C:\prgs\Brimborium.Tracerit.Collector\Brimborium.Tracerit.Collector.exe" service="Tracerit" description="Tracerit" protocol=TCP localport=4319

REM sc query Tracerit

REM sc stop Tracerit


C:\prgs\Brimborium.Tracerit.Collector>sc start Tracerit
[SC] StartService FAILED 5:

Access is denied.

https://learn.microsoft.com/en-us/defender-endpoint/attack-surface-reduction-rules-deployment-implement#customize-attack-surface-reduction-rules

Use Group Policy to exclude files and folders
On your Group Policy management computer, open the Group Policy Management Console. Right-click the Group Policy Object you want to configure and select Edit.

In the Group Policy Management Editor, go to Computer configuration and select Administrative templates.

Expand the tree to Windows components > Microsoft Defender Antivirus > Microsoft Defender Exploit Guard > Attack surface reduction.

Double-click the Exclude files and paths from Attack surface reduction Rules setting and set the option to Enabled. Select Show and enter each file or folder in the Value name column. Enter 0 in the Value column for each item.