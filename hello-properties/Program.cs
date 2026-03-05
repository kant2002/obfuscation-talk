var world = new World();
Console.WriteLine($"Hello, {world.Name}!");

class World
{
    public string Name { get; set; } = "World";
}