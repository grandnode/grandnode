namespace Grand.Domain.Documents
{
    /// <summary>
    /// Represents a document type
    /// </summary>
    public partial class DocumentType : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
