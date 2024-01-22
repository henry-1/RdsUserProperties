# RdsUserProperties
Powershell Module to manage RDS Settings of an AD User Object

Ths **src** directory contains the source code of the project.

The **RdsUserProperties** directory must be copied to 
 C:\Windows\System32\WindowsPowerShell\v1.0\Modules

 Folder Structure for Module Path
 ```
RdsUserProperties
| - RdsUserProperties.psd1
| - RdsUserProperties.psm1
|
└─── bin
     | - RdsUserProperties.dll
     | - RdsUserProperties.pdb


 ```
# Cmdlet Usage
```
Get-Help Get-RdsUserProperties
```
```
Get-RdsUserProperties [-Identity] <string> [-ServerName] <string>  [<CommonParameters>]
```

```
Set-RdsUserProperties [-Identity] <string> [-ServerName] <string> [[-TerminalServicesProfilePath] <string>] [[-TerminalServicesHomeDrive] <string>] [[-TerminalServicesHomeDirectory] <string>] [[-AllowLogon] <bool>]
```