using System;

namespace Bind.iOS
{
	public class Person
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public string FullName { get { return FirstName + " " + LastName; } }

		public Person ()
		{
			FirstName = "";
			LastName = "";
		}
	}
}

