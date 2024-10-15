using MiJenner.ServicesGeneric;

namespace TestCrudIdService
{
    public class Item : IHasGuid  
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool WantNotifications { get; set; }
    }
}
