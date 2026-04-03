using dnlib.DotNet;
using dnlib.DotNet.Emit;

if (args.Length < 1)
{
    Console.WriteLine("obfuscator-string-encoding mdfile targetfile");
    return;
}

var assemblyFile = args[0];
var targetFile = args[1];
ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD targetModule = ModuleDefMD.Load(assemblyFile, modCtx);

// Create a new type in the target assembly to hold the injected code
var decoderType = new TypeDefUser("Decoder", targetModule.CorLibTypes.Object.TypeDefOrRef);
targetModule.Types.Add(decoderType);
// Load template class
var targetDecoder = GetRuntimeTemplateType(typeof(Decoder).FullName);
// Inject template class content into target type in target assembly
var context = new InjectContext(targetModule);
var importer = new Importer(targetModule, ImporterOptions.TryToUseTypeDefs, new GenericParamContext(), context);
Inject((TypeDef)targetDecoder, (TypeDef)decoderType, targetModule);
// Perform assembly rewriting
foreach (var type in targetModule.Types)
{
    if (type == decoderType) continue;
    foreach (var method in type.Methods)
    {
        if (!method.HasBody)
            continue;
        for (int i = 0; i < method.Body.Instructions.Count; i++)
        {
            var instr = method.Body.Instructions[i];
            if (instr.OpCode == OpCodes.Ldstr)
            {
                var str = (string)instr.Operand;
                // Encode using obfuscation runtime
                var encodedStr = Decoder.EncodeString(str);
                instr.Operand = encodedStr;
                // Insert placing Decoder.DecodeString on the stack after the ldstr instruction
                var decodeStringSignature = 
                    MethodSig.CreateStatic(targetModule.CorLibTypes.String, targetModule.CorLibTypes.String);
                var decodeString = new Instruction(
                    OpCodes.Call,
                    importer.Import(decoderType.FindMethod("DecodeString", decodeStringSignature)));
                method.Body.Instructions.Insert(i + 1, decodeString);
                i = i + 1; // Skip the instruction we just added
            }
        }
    }
}

targetModule.Write(targetFile);
Console.WriteLine($"Rewritten metadata for the assembly {assemblyFile} saved to {targetFile}");

// Get runtime type from existing assembly.
TypeDef GetRuntimeTemplateType(string typeName)
{
    var runtimeModule = ModuleDefMD.Load(typeof(Program).Assembly.ManifestModule);
    return runtimeModule.Find(typeName, true);
}

// Inject type definition into new type
static IEnumerable<IDnlibDef> Inject(TypeDef typeDef, TypeDef newType, ModuleDef target)
{
    var ctx = new InjectContext(target);
    ctx.DefMap[typeDef] = newType;
    PopulateContext(typeDef, ctx);
    foreach (MethodDef method in typeDef.Methods)
        CopyMethodDef(method, ctx);
    return ctx.DefMap.Values.Except(new[] { newType }).OfType<IDnlibDef>();
}

static void CopyMethodDef(MethodDef methodDef, InjectContext ctx)
{
    var newMethodDef = ctx.Map(methodDef)?.ResolveMethodDefThrow();

    newMethodDef.Signature = ctx.Importer.Import(methodDef.Signature);
    newMethodDef.Parameters.UpdateParameterTypes();

    foreach (var paramDef in methodDef.ParamDefs)
        newMethodDef.ParamDefs.Add(new ParamDefUser(paramDef.Name, paramDef.Sequence, paramDef.Attributes));

    if (methodDef.ImplMap != null)
        newMethodDef.ImplMap = new ImplMapUser(new ModuleRefUser(ctx.TargetModule, methodDef.ImplMap.Module.Name), methodDef.ImplMap.Name, methodDef.ImplMap.Attributes);

    foreach (CustomAttribute ca in methodDef.CustomAttributes)
    {
        var newCa = new CustomAttribute((ICustomAttributeType)ctx.Importer.Import(ca.Constructor));
        foreach (var arg in ca.ConstructorArguments)
        {
            if (arg.Value is IType type)
                newCa.ConstructorArguments.Add(new CAArgument((TypeSig)ctx.Importer.Import(type)));
            else
                newCa.ConstructorArguments.Add(arg);
        }

        newMethodDef.CustomAttributes.Add(newCa);
    }

    if (methodDef.HasBody)
        CopyMethodBody(methodDef, ctx, newMethodDef);
}

static void CopyMethodBody(MethodDef methodDef, InjectContext ctx, MethodDef newMethodDef)
{
    newMethodDef.Body = new CilBody(methodDef.Body.InitLocals, new List<Instruction>(),
        new List<ExceptionHandler>(), new List<Local>())
    { MaxStack = methodDef.Body.MaxStack };

    var bodyMap = new Dictionary<object, object>();
    foreach (Local local in methodDef.Body.Variables)
    {
        var newLocal = new Local(ctx.Importer.Import(local.Type)) { Name = local.Name };
        newMethodDef.Body.Variables.Add(newLocal);
        bodyMap[local] = newLocal;
    }

    foreach (Instruction instr in methodDef.Body.Instructions)
    {
        var newInstr = new Instruction(instr.OpCode, instr.Operand);
        switch (newInstr.Operand)
        {
            case IType type:
                newInstr.Operand = ctx.Importer.Import(type);
                break;
            case IMethod method:
                newInstr.Operand = ctx.Importer.Import(method);
                break;
            case IField field:
                newInstr.Operand = ctx.Importer.Import(field);
                break;
        }

        newMethodDef.Body.Instructions.Add(newInstr);
        bodyMap[instr] = newInstr;
    }

    foreach (Instruction instr in newMethodDef.Body.Instructions)
    {
        if (instr.Operand != null && bodyMap.ContainsKey(instr.Operand))
            instr.Operand = bodyMap[instr.Operand];
    }

    newMethodDef.Body.SimplifyMacros(newMethodDef.Parameters);
}

static TypeDef PopulateContext(TypeDef typeDef, InjectContext ctx)
{
    var ret = ctx.Map(typeDef)?.ResolveTypeDef();
    if (ret is null)
    {
        ret = new TypeDefUser(typeDef.Namespace, typeDef.Name);
        ctx.DefMap[typeDef] = ret;
    }

    foreach (MethodDef method in typeDef.Methods)
    {
        var newMethodDef = new MethodDefUser(method.Name, null, method.ImplAttributes, method.Attributes);
        ctx.DefMap[method] = newMethodDef;
        ret.Methods.Add(newMethodDef);
    }

    return ret;
}

class InjectContext : ImportMapper
{
    public readonly Dictionary<IMemberRef, IMemberRef> DefMap = new Dictionary<IMemberRef, IMemberRef>();

    public readonly ModuleDef TargetModule;

    public InjectContext(ModuleDef target)
    {
        TargetModule = target;
        Importer = new Importer(target, ImporterOptions.TryToUseTypeDefs, new GenericParamContext(), this);
    }

    public Importer Importer { get; }

    /// <inheritdoc />
    public override ITypeDefOrRef? Map(ITypeDefOrRef source)
    {
        if (DefMap.TryGetValue(source, out var mappedRef))
            return mappedRef as ITypeDefOrRef;

        // check if the assembly reference needs to be fixed.
        if (source is TypeRef sourceRef)
        {
            var targetAssemblyRef = TargetModule.GetAssemblyRef(sourceRef.DefinitionAssembly.Name);
            if (!(targetAssemblyRef is null) && !string.Equals(targetAssemblyRef.FullName, source.DefinitionAssembly.FullName, StringComparison.Ordinal))
            {
                // We got a matching assembly by the simple name, but not by the full name.
                // This means the injected code uses a different assembly version than the target assembly.
                // We'll fix the assembly reference, to avoid breaking anything.
                var fixedTypeRef = new TypeRefUser(sourceRef.Module, sourceRef.Namespace, sourceRef.Name, targetAssemblyRef);
                return Importer.Import(fixedTypeRef);
            }
        }
        return null;
    }

    /// <inheritdoc />
    public override IMethod? Map(MethodDef source)
    {
        if (DefMap.TryGetValue(source, out var mappedRef))
            return mappedRef as IMethod;
        return null;
    }

    /// <inheritdoc />
    public override IField? Map(FieldDef source)
    {
        if (DefMap.TryGetValue(source, out var mappedRef))
            return mappedRef as IField;
        return null;
    }

    public override MemberRef? Map(MemberRef source)
    {
        if (DefMap.TryGetValue(source, out var mappedRef))
            return mappedRef as MemberRef;
        return null;
    }
}