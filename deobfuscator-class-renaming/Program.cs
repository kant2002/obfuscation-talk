using dnlib.DotNet;
using System.Globalization;

if (args.Length < 1)
{
    Console.WriteLine("deobfuscator-class-renaming mdfile targetfile");
    return;
}

var assemblyFile = args[0];
var targetFile = args[1];
ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD module = ModuleDefMD.Load(assemblyFile, modCtx);
foreach (var type in module.Types)
{
    if (type.Name == "<Module>")
        continue;

    // Rename types
    if (type.BaseType.FullName == "Avalonia.Controls.Window")
        type.Name = type.Name + "Window";
    if (type.BaseType.FullName == "Avalonia.Application")
        type.Name = type.Name + "App";
    if (type.BaseType.FullName == "Avalonia.Controls.UserControl")
        type.Name = type.Name + "UserControl";

    // Rename fields
    foreach (var field in type.Fields)
    {
        if (field.FieldType.FullName == "Avalonia.Controls.Button")
            field.Name = field.Name + "Button";
        if (field.FieldType.FullName == "Avalonia.Controls.Label")
            field.Name = field.Name + "Label";
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
