## MiJenner Services Generic 
Offers a simple CRUD service for quick and dirty testing of apps needing one. 

# Model 
Model must be simple, a list of objects. Two requirements: (a) Model must inherit from IHasGuid, and (b) it must have a property called Id of type Guid. Example: 

```cs
public class Item : IHasGuid 
{
   public Guid Id { get; set; }
   public required string Name { get; set; }
   public string? Description { get; set; }
}
```

# Interface
Interface: ICrudIdService 

# Implementation 
Implementation: CrudIdServiceInMemory 

# Example usage 
After having created a Model as shown above, then you can create a list of these objects using: 

```cs
ICrudIdService<Item> crudIdService = new CrudIdInMemory<Item>();
```
