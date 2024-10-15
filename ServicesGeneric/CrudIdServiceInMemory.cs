using System.Collections.Concurrent;

namespace MiJenner.ServicesGeneric
{
    public class CrudIdServiceInMemory<T> : ICrudIdService<T> where T : IHasGuid
    {
        private ConcurrentDictionary<Guid, T> items;

        public CrudIdServiceInMemory()
        {
            items = new ConcurrentDictionary<Guid, T>();
        }

        public Task<Guid> CreateAsync(T item)
        {
            item.Id = Guid.NewGuid();
            items[item.Id] = item;
            return Task.FromResult(item.Id);
        }

        public Task CreateExistingIdAsync(T item)
        {
            bool alreadyExists = Contains(item.Id).Result;
            if (alreadyExists)
            {
                throw new KeyNotFoundException($"Item with ID: {item.Id} already exists\nUse overwrite if you really want to");
            }

            items[item.Id] = item;
            return Task.CompletedTask;
        }

        public Task<T> ReadByIdAsync(Guid id)
        {
            bool alreadyExists = Contains(id).Result;
            if (alreadyExists)
            {
                return Task.FromResult(items[id]);
            }
            throw new KeyNotFoundException($"No item found with ID: {id}");
        }

        public Task<IEnumerable<T>> ReadAllAsync()
        {
            return Task.FromResult<IEnumerable<T>>(items.Values.ToList());
        }

        public Task UpdateAsync(T item)
        {
            bool alreadyExists = Contains(item.Id).Result;
            if (alreadyExists)
            {
                items[item.Id] = item;
                return Task.CompletedTask;
            }
            throw new KeyNotFoundException($"Cannot update non-existing item - ID: {item.Id}\nUse CreateAsync instead");
        }

        public Task DeleteAsync(T item)
        {
            items.TryRemove(item.Id, out _);
            return Task.CompletedTask;
        }

        public Task DeleteAllAsync()
        {
            items.Clear();
            return Task.CompletedTask;
        }

        public Task<bool> Contains(Guid id)
        {
            var res = items.ContainsKey(id);
            return Task.FromResult(res);
        }
    }
}
