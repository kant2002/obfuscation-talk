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
dotnet run --project obfuscator-class-renaming\obfuscator-class-renaming.csproj hello-properties\bin\Release\net11.0\hello-properties.dll hello-properties\bin\Release\net11.0\hello-properties.obfuscated-classname.dll
dotnet run --project obfuscator-properties-removal\obfuscator-properties-removal.csproj hello-properties\bin\Release\net11.0\hello-properties.dll hello-properties\bin\Release\net11.0\hello-properties.obfuscated.dll
dotnet run --project obfuscator-string-encoding\obfuscator-string-encoding.csproj hello-obfuscation\bin\Release\net11.0\hello-obfuscation.dll hello-obfuscation\bin\Release\net11.0\hello-obfuscation.obfuscated-encoding.dll
dotnet run --project obfuscator-string-encryption\obfuscator-string-encryption.csproj hello-obfuscation\bin\Release\net11.0\hello-obfuscation.dll hello-obfuscation\bin\Release\net11.0\hello-obfuscation.obfuscated-encryption.dll
dotnet run --project obfuscator-conditions-simple\obfuscator-conditions-simple.csproj hello-conditions\bin\Release\net11.0\hello-conditions.dll hello-conditions\bin\Release\net11.0\hello-conditions.obfuscated.dll
```