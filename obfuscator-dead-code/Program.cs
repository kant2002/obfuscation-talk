using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text;

if (args.Length < 1)
{
    Console.WriteLine("obfuscator-dead-code mdfile targetfile");
    return;
}

var assemblyFile = args[0];
var targetFile = args[1];
ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD module = ModuleDefMD.Load(assemblyFile, modCtx);
var random = new Random();
foreach (var type in module.Types)
{
    foreach (var method in type.Methods)
    {
        if (!method.HasBody)
            continue;
        var injectionPoint = random.Next(method.Body.Instructions.Count);

        var const1 = new Instruction(
            OpCodes.Ldc_R8,
            1.0);
        method.Body.Instructions.Insert(injectionPoint, const1);
        var pop = new Instruction(OpCodes.Pop);
        method.Body.Instructions.Insert(injectionPoint + 1, pop);
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
