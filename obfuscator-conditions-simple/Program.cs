using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text;

if (args.Length < 1)
{
    Console.WriteLine("obfuscator-string-encoding mdfile targetfile");
    return;
}

var assemblyFile = args[0];
var targetFile = args[1];
ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD module = ModuleDefMD.Load(assemblyFile, modCtx);
foreach (var type in module.Types)
{
    foreach (var method in type.Methods)
    {
        if (!method.HasBody)
            continue;
        for (int i = 2; i < method.Body.Instructions.Count; i++)
        {
            var instr = method.Body.Instructions[i];
            if (instr.IsConditionalBranch()
                && (method.Body.Instructions[i - 1].IsLdarg() || method.Body.Instructions[i - 1].IsLdloc()
                || method.Body.Instructions[i - 2].IsLdarg() || method.Body.Instructions[i - 2].IsLdloc()))
            {
                var nextInstruction = method.Body.Instructions[i + 1];
                // ldc.r8 1
                var const1 = new Instruction(
                    OpCodes.Ldc_R8,
                    1.0);
                method.Body.Instructions.Insert(i - 2, const1);
                // ldc.r8 1
                var const1_2 = new Instruction(
                    OpCodes.Ldc_R8,
                    1.0);
                method.Body.Instructions.Insert(i - 1, const1_2);
                // call Math::Log(double)
                var mathLog = new Instruction(
                    OpCodes.Call,
                    module.Import(typeof(Math).GetMethod("Log", [typeof(double)])));
                method.Body.Instructions.Insert(i, mathLog);
                // call Math::Log(double)
                var breqNext = new Instruction(
                    OpCodes.Beq_S,
                    nextInstruction);
                method.Body.Instructions.Insert(i + 1, breqNext);
                i = i + 4; // Skip the instructions we just added
            }
        }
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
