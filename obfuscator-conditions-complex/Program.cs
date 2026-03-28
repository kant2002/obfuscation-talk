using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text;

if (args.Length < 1)
{
    Console.WriteLine("obfuscator-conditions-complex mdfile targetfile");
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
        var flowGraph = new FlowGraph(method);
        if (flowGraph.BasicBlocks.Count == 1)
            continue;
        // ldc.r8 1
        var const1 = new Instruction(
            OpCodes.Ldc_R8,
            1.0);
        // ldc.r8 1
        var const1_2 = new Instruction(
            OpCodes.Ldc_R8,
            1.0);
        // call Math::Log(double)
        var mathLog = new Instruction(
            OpCodes.Call,
            module.Import(typeof(Math).GetMethod("Log", [typeof(double)])));
        // Beq_S
        var randomBB = Random.Shared.Next(flowGraph.BasicBlocks.Count - 1);
        var randomTarget = Random.Shared.Next(flowGraph.BasicBlocks.Count - 1);
        var fakeInstruction = flowGraph.BasicBlocks[randomTarget].Instructions[0];
        var breqNext = new Instruction(
            OpCodes.Beq_S,
            fakeInstruction);
        flowGraph.BasicBlocks.Insert(randomBB, new BasicBlock()
        {
            Instructions =
            {
                const1,
                const1_2,
                mathLog,
                breqNext
            }
        });
        flowGraph.BasicBlocks.Insert(randomBB + 1, new BasicBlock()
        {
            Instructions =
            {
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Pop),
            }
        });
        flowGraph.Save(method);
    }
}

module.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");

class BasicBlock
{
    public List<Instruction> Instructions { get; set; } 
        = new List<Instruction>();
}

class FlowGraph
{
    public List<BasicBlock> BasicBlocks { get; set; } 
        = new List<BasicBlock>();
    public FlowGraph(MethodDef method)
    {
        List<int> basicBlocksStart = new() { 0 };
        for (int i = 1; i < method.Body.Instructions.Count; i++)
        {
            var instr = method.Body.Instructions[i];
            if (instr.IsBr() || instr.IsConditionalBranch() || instr.OpCode == OpCodes.Ret)
            {
                if (instr.IsConditionalBranch())
                {
                    var instructionIndex = method.Body.Instructions.IndexOf((Instruction)instr.Operand);
                    basicBlocksStart.Add(instructionIndex);
                }

                if (i + 1 < method.Body.Instructions.Count)
                {
                    basicBlocksStart.Add(i + 1);
                    i++; // skip next instruction, since we already add it.
                    continue;
                }
            }
        }

        basicBlocksStart = basicBlocksStart.Distinct().ToList();
        basicBlocksStart.Sort();
        for (int i = 0; i < basicBlocksStart.Count; i++)
        {
            var block = new BasicBlock();
            var finish = i == basicBlocksStart.Count - 1 
                ? method.Body.Instructions.Count 
                : basicBlocksStart[i + 1];
            for (int j = basicBlocksStart[i]; j < finish; j++)
            {
                block.Instructions.Add(method.Body.Instructions[j]);
            }

            BasicBlocks.Add(block);
        }
    }

    public void Save(MethodDef method)
    {
        method.Body.Instructions.Clear();
        foreach (var block in BasicBlocks)
        {
            foreach (var instr in block.Instructions)
            {
                method.Body.Instructions.Add(instr);
            }
        }
    }
}