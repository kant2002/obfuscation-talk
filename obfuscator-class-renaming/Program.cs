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

    // If the type is public or protected, skip it to avoid breaking external references
    var renamePublicTypes = false;
    if (renamePublicTypes)
    {
        if (type.IsPublic || type.IsNestedFamily || type.IsNestedFamily || type.IsNestedAssembly)
            continue;
    }

    // Rename types
    type.Name = "Class" + typeCode.ToString(CultureInfo.InvariantCulture);
    typeCode++;

    // Rename methods
    int methodCode = 0;
    foreach (var method in type.Methods)
    {
        if (method.Name == ".ctor")
            continue;
        if (method.IsPublic || method.IsFamily)
            continue;
        method.Name = "Method" + methodCode.ToString(CultureInfo.InvariantCulture);
        Console.WriteLine($"Renamed method {method.Name} in type {type.Name}");
        methodCode++;
    }

    // Rename fields
    int fieldCode = 0;
    foreach (var field in type.Fields)
    {
        if (field.IsPublic || field.IsFamily)
            continue;
        field.Name = "Field" + fieldCode.ToString(CultureInfo.InvariantCulture);
        fieldCode++;
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
