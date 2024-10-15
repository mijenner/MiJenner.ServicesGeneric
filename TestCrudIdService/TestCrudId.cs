using MiJenner.ServicesGeneric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestCrudIdService
{
    public class TestCrudId

    {
        private readonly CrudIdServiceInMemory<Item> _service;

        public TestCrudId()
        {
            _service = new CrudIdServiceInMemory<Item>();
        }

        [Fact, TestPriority(1)]
        public async Task CreateAsync_ShouldGenerateGuidAndAddItem()
        {
            // Arrange
            var item = new Item { Name = "Test Item", Description = "A test description", WantNotifications = true };

            // Act
            Guid id = await _service.CreateAsync(item);

            // Assert
            Assert.NotEqual(Guid.Empty, id);  // Ensure the ID was generated
            var storedItem = await _service.ReadByIdAsync(id);
            Assert.Equal(item.Name, storedItem.Name);  // Ensure item was added
        }

        [Fact, TestPriority(2)]
        public async Task CreateExistingIdAsync_ShouldAddItemWithExistingGuid()
        {
            // Arrange
            var item = new Item { Id = Guid.NewGuid(), Name = "Test Item", Description = "A test description", WantNotifications = true };

            // Act
            await _service.CreateExistingIdAsync(item);
            var storedItem = await _service.ReadByIdAsync(item.Id);

            // Assert
            Assert.Equal(item.Id, storedItem.Id);  // Ensure the same ID is used
        }

        [Fact, TestPriority(3)]
        public async Task CreateExistingIdAsync_ShouldThrowExceptionForDuplicateGuid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item1 = new Item { Id = id, Name = "Item 1" };
            var item2 = new Item { Id = id, Name = "Item 2" };  // Same ID

            // Act
            await _service.CreateExistingIdAsync(item1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateExistingIdAsync(item2));

            // Assert
            Assert.Equal($"Item with ID: {id} already exists\nUse overwrite if you really want to", exception.Message);
        }

        [Fact, TestPriority(4)]
        public async Task ReadByIdAsync_ShouldReturnItemIfExists()
        {
            // Arrange
            var item = new Item { Id = Guid.NewGuid(), Name = "Test Item" };
            await _service.CreateExistingIdAsync(item);

            // Act
            var storedItem = await _service.ReadByIdAsync(item.Id);

            // Assert
            Assert.NotNull(storedItem);
            Assert.Equal(item.Name, storedItem.Name);
        }

        [Fact, TestPriority(5)]
        public async Task ReadByIdAsync_ShouldThrowExceptionIfItemDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ReadByIdAsync(nonExistentId));
            Assert.Equal($"No item found with ID: {nonExistentId}", exception.Message);
        }

        [Fact, TestPriority(6)]
        public async Task ReadAllAsync_ShouldReturnAllItems()
        {
            // Arrange
            var item1 = new Item { Id = Guid.NewGuid(), Name = "Item 1" };
            var item2 = new Item { Id = Guid.NewGuid(), Name = "Item 2" };
            await _service.CreateExistingIdAsync(item1);
            await _service.CreateExistingIdAsync(item2);

            // Act
            var allItems = await _service.ReadAllAsync();

            // Assert
            Assert.Equal(2, allItems.Count());
        }

        [Fact, TestPriority(7)]
        public async Task UpdateAsync_ShouldUpdateExistingItem()
        {
            // Arrange
            var item = new Item { Id = Guid.NewGuid(), Name = "Original Name" };
            await _service.CreateExistingIdAsync(item);

            // Act
            item.Name = "Updated Name";
            await _service.UpdateAsync(item);
            var updatedItem = await _service.ReadByIdAsync(item.Id);

            // Assert
            Assert.Equal("Updated Name", updatedItem.Name);
        }

        [Fact, TestPriority(8)]
        public async Task UpdateAsync_ShouldThrowExceptionIfItemDoesNotExist()
        {
            // Arrange
            var nonExistentItem = new Item { Id = Guid.NewGuid(), Name = "Non-Existent Item" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(nonExistentItem));
            Assert.Equal($"Cannot update non-existing item - ID: {nonExistentItem.Id}\nUse CreateAsync instead", exception.Message);
        }

        [Fact, TestPriority(10)]
        public async Task DeleteAsync_ShouldRemoveItem()
        {
            // Arrange
            var item = new Item { Id = Guid.NewGuid(), Name = "Test Item" };
            await _service.CreateExistingIdAsync(item);

            // Act
            await _service.DeleteAsync(item);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ReadByIdAsync(item.Id));

            // Assert
            Assert.Equal($"No item found with ID: {item.Id}", exception.Message);
        }

        [Fact, TestPriority(11)]
        public async Task DeleteAllAsync_ShouldClearAllItems()
        {
            // Arrange
            var item1 = new Item { Id = Guid.NewGuid(), Name = "Item 1" };
            var item2 = new Item { Id = Guid.NewGuid(), Name = "Item 2" };
            await _service.CreateExistingIdAsync(item1);
            await _service.CreateExistingIdAsync(item2);

            // Act
            await _service.DeleteAllAsync();
            var allItems = await _service.ReadAllAsync();

            // Assert
            Assert.Empty(allItems);
        }
    }
}
