# TODO
- ssh keys
- Investigate how to package and deploy an app on choco and brew
- make a CI server to build the ROSE app
- make ROSE server CLI to start/stop the server


# Use cases
- want to use mac terminal
- easy access to programs on windows "start menu"

# Alternative solutions
- Using RDP, open powershell as a remoteapp

# Requirements
- Windows client must have registery key Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\fAllowUnlistedRemotePrograms set to 1.
- Windows client must have TermService service started (auto starts by default).

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
Add-Content -Path $PROFILE -Value "`$env:Path += ';C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin'"
Add-Content -Path $PROFILE -Value "`$env:Path += ';C:\Program Files\dotnet\'"
```
then reload powershell.

## To build
```
cd client
dotnet restore
MSBuild.exe /property:Configuration=Release
```

# Server

## Setup
- Install XCode

## To build
- Build in XCode