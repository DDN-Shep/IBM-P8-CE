using System;
using System.Runtime.Serialization;

namespace Validus.FileNet
{
	[Serializable, DataContract]
	public class Document : IDocument
	{
		[DataMember]
		public ObjectStore Store { get; set; }

		[DataMember]
		public DocumentClass Class { get; set; }

		[DataMember, FileNetField(ID = "ID", Name = "ID", IsSystem = true, IsReadOnly = true)]
		public Guid? ID { get; set; }

		[DataMember, FileNetField(IsSystem = true)]
		public byte[] Content { get; set; }

		[DataMember, FileNetField(ID = "MimeType", Name = "Mime Type", IsSystem = true)]
		public string MimeType { get; set; }

		[DataMember, FileNetField(ID = "Name", Name = "Name", IsSystem = true, IsReadOnly = true)]
		public string Name { get; set; }

		[DataMember, FileNetField(ID = "DocumentTitle", Name = "Document Title")]
		public string Title { get; set; }

		[DataMember, FileNetField(ID = "Creator", Name = "Created By", IsSystem = true, IsReadOnly = true)]
		public string CreatedBy { get; set; }

		[DataMember, FileNetField(ID = "LastModifier", Name = "Modified By", IsSystem = true, IsReadOnly = true)]
		public string ModifiedBy { get; set; }

		[DataMember, FileNetField(ID = "DateCreated", Name = "Created On", IsSystem = true, IsReadOnly = true)]
		public DateTime? CreatedOn { get; set; }

		[DataMember, FileNetField(ID = "DateLastModified", Name = "Modified On", IsSystem = true, IsReadOnly = true)]
		public DateTime? ModifiedOn { get; set; }

		public Document(ObjectStore objectStore = ObjectStore.Document,
						DocumentClass documentClass = DocumentClass.Document)
		{
			Store = objectStore;
			Class = documentClass;
		}
	}
}