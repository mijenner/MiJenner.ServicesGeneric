using MiJenner.ServicesGeneric;

namespace CrudIdServiceDemo
{
    public class Item : IHasGuid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool WantNotifications { get; set; }
    }
}
