namespace MiJenner.ServicesGeneric
{
    public interface ICrudIdService<T>
    {
        Task<Guid> CreateAsync(T item);      // Create with service-generated unique ID
        Task CreateExistingIdAsync(T item);  // Create with existing unique ID
        Task<T> ReadByIdAsync(Guid id);      // Read item by unique ID
        Task<IEnumerable<T>> ReadAllAsync(); // Read all items
        Task UpdateAsync(T updatedItem);     // Update an item 
        Task DeleteAsync(T item);            // Delete an item by unique ID
        Task DeleteAllAsync();               // Delete all items 
        Task<bool> Contains(Guid id);        // Contains, returns true or false. 
    }
}
