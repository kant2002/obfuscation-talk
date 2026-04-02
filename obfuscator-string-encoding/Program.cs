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
        // PInvoke methods does not have body. 
        // Abstract methods too does not have body.
        // So we skip these cases
        if (!method.HasBody)
            continue;
        for (int i = 0; i < method.Body.Instructions.Count; i++)
        {
            var instr = method.Body.Instructions[i];
            // Detect ldstr
            if (instr.OpCode == OpCodes.Ldstr)
            {
                var str = (string)instr.Operand;
                var encodedStr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
                instr.Operand = encodedStr;
                // Insert placing Encoding.UTF8 on the stack before the ldstr instruction
                var encoding = new Instruction(
                    OpCodes.Call,
                    module.Import(typeof(Encoding).GetProperty("UTF8", []).GetGetMethod()));
                method.Body.Instructions.Insert(i, encoding);
                // Insert placing Convert.FromBase64String on the stack before the ldstr instruction
                var fromBase64String = new Instruction(
                    OpCodes.Call,
                    module.Import(typeof(Convert).GetMethod("FromBase64String", [typeof(string)])));
                method.Body.Instructions.Insert(i + 2, fromBase64String);
                // Insert placing Encoding.GetString on the stack before the ldstr instruction
                var getString = new Instruction(
                    OpCodes.Call,
                    module.Import(typeof(Encoding).GetMethod("GetString", [typeof(byte[])])));
                method.Body.Instructions.Insert(i + 3, getString);
                i = i + 3; // Skip the instructions we just added
            }
        }
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");
