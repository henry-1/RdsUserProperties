# RdsUserProperties
Powershell Module to manage RDS Settings of an AD User Object

Ths **src** directory contains the source code of the project.

The **RdsUserProperties** directory must be copied to 
 C:\Windows\System32\WindowsPowerShell\v1.0\Modules

 Folder Structure for Module Path
 ```
RdsUserProperties
| - RdsUserProperties.psd1
|
└─── bin
     | - RdsUserProperties.dll
     | - RdsUserProperties.dll-Help.xml


 ```
# Cmdlet Usage
```
Get-Command -Module RdsUserProperties
```
```
Get-Help Get-RdsUserProperties
```
```
Get-RdsUserProperties -Identity <string> [-Password <string>] [-ServerName <string>] [-UserName <string>] [<CommonParameters>]
```

```
Set-RdsUserProperties -Identity <string> [-AllowLogon <bool>] [-Password <string>] [-ServerName <string>] [-TerminalServicesHomeDirectory <string>] [-TerminalServicesHomeDrive <string>] [-TerminalServicesProfilePath <string>] [-UserName <string>] [<CommonParameters>]
```