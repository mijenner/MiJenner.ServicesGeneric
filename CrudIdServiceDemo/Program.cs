using MiJenner.ServicesGeneric;

namespace CrudIdServiceDemo
{
    public class Program
    {
        static ICrudIdService<Item> crudIdService;
        static List<Item> items;
        static List<Item> readItems;

        static async Task Main(string[] args)
        {
            crudIdService = new CrudIdServiceInMemory<Item>();

            // Add one object: 
            try
            {
                await crudIdService.CreateAsync(new Item() { Id = Guid.NewGuid(), Name = "Bent", Description = "BenBen", WantNotifications = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Add 11 more objects: 
            try
            {
                UploadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Fetch one by id: 
            var item2 = items[2];
            try
            {
                var item2read = await crudIdService.ReadByIdAsync(item2.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Read all items: 
            try
            {
                readItems = (await crudIdService.ReadAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Update a single item: 
            var item3 = items[3];
            item3.Name = "John Doe";
            try
            {
                await crudIdService.UpdateAsync(item3);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Delete async 
            var item4 = items[4];
            await crudIdService.DeleteAsync(item4);

            // Delete all: 
            await crudIdService.DeleteAllAsync();
        }

        static void UploadData()
        {
            var items2 = new List<Item>
{
    new Item { Id = Guid.NewGuid(), Name = "Item 1", Description = "Description for Item 1", WantNotifications = true },
    new Item { Id = Guid.NewGuid(), Name = "Item 2", Description = "Description for Item 2", WantNotifications = false },
    new Item { Id = Guid.NewGuid(), Name = "Item 3", Description = "Description for Item 3", WantNotifications = true },
    new Item { Id = Guid.NewGuid(), Name = "Item 4", Description = "Description for Item 4", WantNotifications = false },
    new Item { Id = Guid.NewGuid(), Name = "Item 5", Description = "Description for Item 5", WantNotifications = true },
    new Item { Id = Guid.NewGuid(), Name = "Item 6", Description = "Description for Item 6", WantNotifications = false },
    new Item { Id = Guid.NewGuid(), Name = "Item 7", Description = "Description for Item 7", WantNotifications = true },
    new Item { Id = Guid.NewGuid(), Name = "Item 8", Description = "Description for Item 8", WantNotifications = false },
    new Item { Id = Guid.NewGuid(), Name = "Item 9", Description = "Description for Item 9", WantNotifications = true },
    new Item { Id = Guid.NewGuid(), Name = "Item 10", Description = "Description for Item 10", WantNotifications = false },
    new Item { Id = Guid.NewGuid(), Name = "Item 11", Description = "Description for Item 11", WantNotifications = true },
};

            foreach (var item in items2)
            {
                crudIdService.CreateExistingIdAsync(item);
            }

        }
    }
}
