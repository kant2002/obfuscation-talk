using dnlib.DotNet;
using System.Globalization;

if (args.Length < 1)
{
    Console.WriteLine("obfuscator-class-renaming mdfile targetfile");
    return;
}

var assemblyFile = args[0];
var targetFile = args[1];
ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD module = ModuleDefMD.Load(assemblyFile, modCtx);
int typeCode = 0;
foreach (var type in module.Types)
{
    if (type.Name == "<Module>")
        continue;
    type.Name = "Class" + typeCode.ToString(CultureInfo.InvariantCulture);
    // Remove properties metadata
    type.Properties.Clear();
    // Remove events metadata
    type.Events.Clear();
    typeCode++;
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
