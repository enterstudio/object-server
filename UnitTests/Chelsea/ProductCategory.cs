using System;

using Nichevo.ObjectServer;

namespace UnitTests.Chelsea
{
	[Table("ProductCategories", "Id", PrimaryKeyType.Identity)]
	public abstract class ProductCategory : ServerObject
	{
		public const int MaxNameLength = 50;

		[Column("id")]
		public abstract int Id
		{
			get;
		}

		[Column("name")]
		protected abstract string name
		{
			get;
			set;
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException("value", "Name cannot be null");

				value = value.Trim();

				if(value.Length == 0)
					throw new ArgumentException("Name cannot be an empty string");

				if(value.Length > MaxNameLength)
					throw new ArgumentException(String.Format("Name cannot be more than {0} characters", MaxNameLength));

				name = value;
			}
		}

		public string DisplayName
		{
			get
			{
				return Name.ToUpper();
			}
		}

		[Children(typeof(Product), "Category")]
		public abstract ServerObjectCollection Products
		{
			get;
		}
	}
}
