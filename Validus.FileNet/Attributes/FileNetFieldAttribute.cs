using System;

namespace Validus.FileNet
{
	public class FileNetFieldAttribute : Attribute
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public bool IsSystem { get; set; }
		public bool IsReadOnly { get; set; }
	}
}