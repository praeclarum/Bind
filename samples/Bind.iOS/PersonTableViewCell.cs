using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Praeclarum.Bind;

namespace Bind.iOS.Sample
{
	public class PersonTableViewCell : UITableViewCell
	{
		public static readonly NSString Id = new NSString ("Person");

		Person person;
		public Person Person {
			get { return person; }
			set {
				person = value;
				PersonChanged (this, EventArgs.Empty);
			}
		}
		public event EventHandler PersonChanged = delegate {};

		public PersonTableViewCell ()
			: base (UITableViewCellStyle.Default, Id)
		{
			Binding.Create (() =>
				TextLabel.Text == Person.FirstName &&
				DetailTextLabel.Text == Person.LastName
			);
		}
	}
}
