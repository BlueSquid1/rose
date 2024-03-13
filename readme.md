# Requirements
- Windows client must have RDP services installed
- Windows client must have registery key Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\fAllowUnlistedRemotePrograms set to 1.

# Client
## Setup
Build dependancies can be automatically installed by running:
```
# Install choco
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install VS Build tools
choco install visualstudio2022buildtools

# Install .NET Framework 4.7.2 SDK, .NET SDK and .NET 8.0 Runtime (Long Term Support)
& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\setup.exe' modify --installPath "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools" --config ".\client\rose.vsconfig" --quiet
```

## To build run
```
cd client
MSBuild.exe /property:Configuration=Release
```
# Server
## Setup
- Install XCode
