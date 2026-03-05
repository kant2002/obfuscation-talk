How to build obfuscator and deobfuscator
========================================

Material born out of desire of one [Almaty Hackerspace](https://blackice.kz/) resident to learn how to deobfuscate.

## How to replicate

```shell
dotnet build -c Release
cd ..\
# This is application for dumping metadata into CSV files. 
git clone https://github.com/kant2002/metadatadumper
cd MetadataDumper
dotnet run --project MetadataDumper\MetadataDumper.csproj ..\obfuscation-talk\hello-obfuscation\bin\Release\net11.0\hello-obfuscation.dll ..\obfuscation-talk\hello-obfuscation\metadata\
cd ..\obfuscation-talk
dotnet run --project obfuscator-class-renaming\obfuscator-class-renaming.csproj hello-obfuscation\bin\Release\net11.0\hello-obfuscation.dll hello-obfuscation\bin\Release\net11.0\hello-obfuscation.obfuscated.dll
```