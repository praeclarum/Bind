using System;
using MonoTouch.UIKit;

using Praeclarum.Bind;
using Praeclarum.UI;

namespace Bind.iOS.Sample
{
	public class PersonViewController : UIViewController
	{
		readonly Person person;

		UITextField firstNameEdit;
		UITextField lastNameEdit;
		UILabel fullNameLabel;

		public PersonViewController (Person person)
		{
			this.person = person;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Done,
				delegate {
					new UIAlertView ("", "Good job, " + person.FullName, null, "OK").Show ();
				});
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//
			// Create the UI
			//
			firstNameEdit = new UITextField {
				Placeholder = "First Name",
			};
			lastNameEdit = new UITextField {
				Placeholder = "Last Name",
			};
			fullNameLabel = new UILabel {
				Font = UIFont.PreferredHeadline,
			};

			View.AddSubviews (firstNameEdit, lastNameEdit, fullNameLabel);
			View.BackgroundColor = UIColor.White;

			//
			// Layout the UI
			//
			View.ConstrainLayout (() =>
				firstNameEdit.Frame.Left == View.Frame.Left + 10 &&
				firstNameEdit.Frame.Right == View.Frame.Right - 10 &&
				firstNameEdit.Frame.Top == View.Frame.Top + 100 &&

				lastNameEdit.Frame.Left == firstNameEdit.Frame.Left &&
				lastNameEdit.Frame.Right == firstNameEdit.Frame.Right &&
				lastNameEdit.Frame.Top == firstNameEdit.Frame.Bottom + 10 &&
			
				fullNameLabel.Frame.Left == firstNameEdit.Frame.Left &&
				fullNameLabel.Frame.Right == firstNameEdit.Frame.Right &&
				fullNameLabel.Frame.Top == lastNameEdit.Frame.Bottom + 20);

			//
			// Databind the UI
			//
			Binding.Create (() => firstNameEdit.Text == person.FirstName);
			Binding.Create (() => lastNameEdit.Text == person.LastName);
			Binding.Create (() => fullNameLabel.Text == person.LastName + ", " + person.FirstName);
			Binding.Create (() => Title == person.LastName + ", " + person.FirstName);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			firstNameEdit.BecomeFirstResponder ();
		}
	}
}














