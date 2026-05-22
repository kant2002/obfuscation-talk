using dnlib.DotNet.Emit;

var (expr, value) = Generator(3);
var body = new CilBody();
GenerateMethodBody(expr, body.Instructions);

Console.WriteLine("Expression");
Console.WriteLine("{0}", expr);
Console.WriteLine("Value");
Console.WriteLine("{0}", value);

Console.WriteLine("IL Code\n{0}", body);
for (var i = 0; i < body.Instructions.Count; i++)
{
    Console.WriteLine("{0}", body.Instructions[i]);
}

(Expr, int) Generator(int fuel)
{
    if (fuel == 0)
    {
        var value = Random.Shared.Next(100);
        return (new ConstInt32(value), value);
    }
    var op = Random.Shared.Next(5);
    var (left, leftValue) = Generator(fuel - 1);
    var (right, rightValue) = Generator(fuel - 1);
    switch (op)
    {
        case 0:
            return (new AddOperation(left, right), leftValue + rightValue);
        case 1:
            return (new SubOperation(left, right), leftValue - rightValue);
        case 2:
            return (new MulOperation(left, right), leftValue * rightValue);
        case 3:
            if (rightValue == 0)
                return Generator(fuel - 1);
            return (new DivOperation(left, right), leftValue / rightValue);
        case 4:
            if (rightValue == 0)
                return Generator(fuel - 1);
            return (new ModOperation(left, right), leftValue % rightValue);
    }
    return (new AddOperation(left, right), rightValue);
}

void GenerateMethodBody(Expr expr, IList<Instruction> il)
{
    switch (expr)
    {
        case ConstInt32 c:
            il.Add(Instruction.Create(OpCodes.Ldc_I4, c.Value));
            break;
        case AddOperation a:
            GenerateMethodBody(a.Left, il);
            GenerateMethodBody(a.Right, il);
            il.Add(Instruction.Create(OpCodes.Add));
            break;
        case SubOperation s:
            GenerateMethodBody(s.Left, il);
            GenerateMethodBody(s.Right, il);
            il.Add(Instruction.Create(OpCodes.Sub));
            break;
        case MulOperation m:
            GenerateMethodBody(m.Left, il);
            GenerateMethodBody(m.Right, il);
            il.Add(Instruction.Create(OpCodes.Mul));
            break;
        case DivOperation d:
            GenerateMethodBody(d.Left, il);
            GenerateMethodBody(d.Right, il);
            il.Add(Instruction.Create(OpCodes.Div));
            break;
        case ModOperation m:
            GenerateMethodBody(m.Left, il);
            GenerateMethodBody(m.Right, il);
            il.Add(Instruction.Create(OpCodes.Rem));
            break;
    }
}

record ConstInt32(int Value);
record AddOperation(Expr Left, Expr Right);
record SubOperation(Expr Left, Expr Right);
record MulOperation(Expr Left, Expr Right);
record DivOperation(Expr Left, Expr Right);
record ModOperation(Expr Left, Expr Right);
union Expr(ConstInt32, AddOperation, SubOperation, MulOperation, DivOperation, ModOperation);
