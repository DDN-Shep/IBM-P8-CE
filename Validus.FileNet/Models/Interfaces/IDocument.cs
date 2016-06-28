using System;

namespace Validus.FileNet
{
	public interface IDocument
	{
		Guid? ID { get; set; }

		ObjectStore Store { get; set; }

		DocumentClass Class { get; set; }

		byte[] Content { get; set; }

		string MimeType { get; set; }

		string Name { get; set; }

		string Title { get; set; }

		string CreatedBy { get; set; }

		string ModifiedBy { get; set; }

		DateTime? CreatedOn { get; set; }

		DateTime? ModifiedOn { get; set; }
	}
}