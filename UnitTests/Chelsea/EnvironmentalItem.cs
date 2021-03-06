using System;

using Nichevo.ObjectServer;

namespace UnitTests.Chelsea
{
	[Table("EnvironmentalItems", "Id", PrimaryKeyType.Identity, DefaultOrder = "Order")]
	public abstract class EnvironmentalItem : ServerObject
	{
		public const int MaxTitleLength = 50;

		[Column("id")]
		public abstract int Id
		{
			get;
		}

		[Column("title")]
		protected abstract string title
		{
			get;
			set;
		}

		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException("value", "Title cannot be null");

				value = value.Trim();

				if(value.Length == 0)
					throw new ArgumentException("Title cannot be an empty string");

				if(value.Length > MaxTitleLength)
					throw new ArgumentException(String.Format("Title cannot be more than {0} characters", MaxTitleLength));

				title = value;
			}
		}

		[Column("content")]
		public abstract string Text
		{
			get;
			set;
		}

		[Column("ordering")]
		public abstract int Order
		{
			get;
			set;
		}
	}
}
