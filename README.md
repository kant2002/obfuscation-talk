How to build obfuscator and deobfuscator
========================================

Material born out of desire of one [Almaty Hackerspace](https://blackice.kz/) resident to learn how to deobfuscate.

## Prerequiresites

- VS Code or any other .NET IDE
- [ILSpy VSCode](https://marketplace.visualstudio.com/items?itemName=icsharpcode.ilspy-vscode) extension for ILSpy integration in VSCode
- If you on Windows, better use [ILSpy](https://github.com/icsharpcode/ILSpy)
- .NET 11.0 SDK (Yes, I prefer bleeding edge, but you can use .NET 10.0 as well, just change the target framework in Directory.Build.props)

```
dotnet tool install --global MetadataDumper
```

or alternatively build from source

```shell
# This is application for dumping metadata into CSV files. 
git clone https://github.com/kant2002/metadatadumper
cd MetadataDumper
dotnet build
```

## How to replicate

Build target executables and obfuscators

```shell
dotnet build -c Release
```

Run metadata export

```shell
curl -O https://download.wikdict.com/dictionaries/sqlite/2_2024-03/en.sqlite3
dotnet tool exec MetadataDumper -- artifacts\bin\hello-obfuscation\release\hello-obfuscation.dll hello-obfuscation\metadata\
```

or if you want local
```shell
cd ..\
# This is application for dumping metadata into CSV files. 
git clone https://github.com/kant2002/metadatadumper
cd MetadataDumper
curl -O https://download.wikdict.com/dictionaries/sqlite/2_2024-03/en.sqlite3
dotnet run --project MetadataDumper\MetadataDumper.csproj ..\obfuscation-talk\artifacts\bin\release\hello-obfuscation.dll ..\obfuscation-talk\hello-obfuscation\metadata\
cd ..\obfuscation-talk
```

Run obfuscators

```shell
dotnet run --project obfuscator-class-renaming\obfuscator-class-renaming.csproj artifacts\bin\hello-obfuscation\release\hello-obfuscation.dll artifacts\bin\hello-obfuscation\release\hello-obfuscation.obfuscated.dll
dotnet run --project obfuscator-class-renaming\obfuscator-class-renaming.csproj artifacts\bin\hello-properties\release\hello-properties.dll artifacts\bin\hello-properties\release\hello-properties.obfuscated-classname.dll
dotnet run --project obfuscator-properties-removal\obfuscator-properties-removal.csproj artifacts\bin\hello-properties\release\hello-properties.dll artifacts\bin\hello-properties\release\hello-properties.obfuscated.dll
dotnet run --project obfuscator-string-encoding\obfuscator-string-encoding.csproj artifacts\bin\hello-obfuscation\release\hello-obfuscation.dll artifacts\bin\hello-obfuscation\release\hello-obfuscation.obfuscated-encoding.dll
dotnet run --project obfuscator-string-encryption\obfuscator-string-encryption.csproj artifacts\bin\hello-obfuscation\release\hello-obfuscation.dll artifacts\bin\hello-obfuscation\release\hello-obfuscation.obfuscated-encryption.dll
dotnet run --project obfuscator-conditions-simple\obfuscator-conditions-simple.csproj artifacts\bin\hello-conditions\release\hello-conditions.dll artifacts\bin\hello-conditions\release\hello-conditions.obfuscated.dll
dotnet run --project obfuscator-dead-code\obfuscator-dead-code.csproj artifacts\bin\hello-conditions\release\hello-conditions.dll artifacts\bin\hello-conditions\release\hello-conditions.obfuscated-dead.dll
```

## .NET Cheat Sheet

If you see .NET for a first time, here is a cheat sheet for you:

Create HelloWorld console application in the HelloWorld sub-directory

```
dotnet new console -n HelloWorld
```

Build the application

```
dotnet build
```

Build release application (slightly different IL codegen)

```
dotnet build -c Release
```

Run application

```
dotnet run
```

Run application with parameters 

```
dotnet run -- parameters goes here
```

Add dnlib package

```
dotnet add package dnlib
```