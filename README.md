# Brimborium.Tracerit

Utility for testing 

# EventId

```powershell

&{ $ticks=[System.DateTime]::UtcNow.Ticks; (($ticks / [int]::MaxValue) -bxor ($ticks % [int]::MaxValue)).ToString() | Set-Clipboard }

```