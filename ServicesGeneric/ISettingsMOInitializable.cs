namespace MiJenner.ServicesGeneric
{
    /// <summary>
    /// Interface to handle sensitive data in settings objects.
    /// Classes implementing this interface can securely populate 
    /// sensitive properties and ensure that such data is excluded 
    /// from stored JSON files.
    /// </summary>
    public interface ISettingsMOInitializable
    {
        /// <summary>
        /// Populates sensitive properties that should not be serialized 
        /// or stored in JSON. This could involve fetching data from secure 
        /// storage or an external secure API.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Creates a copy of the object with sensitive data removed. 
        /// This method ensures sensitive fields are not written to the JSON file.
        /// The developer should create a deep copy of the object, replacing or 
        /// excluding sensitive fields as needed.
        /// </summary>
        /// <returns>A copy of the object with sensitive data removed.</returns>
        object CopyForStorage();
    }
}
