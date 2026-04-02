var world = new World();
world.ComplexName = "Universe";
Console.WriteLine($"Hello, {world.Name}!");
Console.WriteLine($"Hello, {world.ComplexName}!");

class World
{
    public string Name { get; set; } = "World";
    public string ComplexName
    { 
        get
        {
            Console.WriteLine($"Getting ComplexName, which is {field}");
            return field;
        }
        set
        {
            field = value;
            Console.WriteLine($"Set ComplexName to {value}");   
        }
     } = "World";
}