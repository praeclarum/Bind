using System;
using MonoTouch.UIKit;

using Praeclarum.Bind;
using Praeclarum.UI;
using MonoTouch.CoreGraphics;

namespace Bind.iOS.Sample
{
	public class PersonViewController : UIViewController
	{
		readonly Person person;

		UITextField firstNameEdit;
		UITextField lastNameEdit;
		UILabel fullNameLabel;
		UIButton bindButton;

		Binding binding;

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
			bindButton = UIButton.FromType (UIButtonType.RoundedRect);
			bindButton.SetTitle ("Bind", UIControlState.Normal);
			bindButton.Selected = true;
			bindButton.TouchUpInside += HandleBindButton;

			firstNameEdit = new UITextField {
				BorderStyle = UITextBorderStyle.RoundedRect,
				Placeholder = "First Name",
				BackgroundColor = UIColor.White,
			};
			lastNameEdit = new UITextField {
				BorderStyle = UITextBorderStyle.RoundedRect,
				Placeholder = "Last Name",
				BackgroundColor = UIColor.White,
			};
			fullNameLabel = new UILabel {
				Font = UIFont.PreferredHeadline,
			};

			View.AddSubviews (bindButton, firstNameEdit, lastNameEdit, fullNameLabel);
			View.BackgroundColor = UIColor.FromWhiteAlpha (0.9f, 1);

			//
			// Layout the UI
			//
			View.ConstrainLayout (() =>
				bindButton.Frame.Top == View.Frame.Top + 80 &&
				bindButton.Frame.GetMidX () == View.Frame.GetMidX () &&

				firstNameEdit.Frame.Left == View.Frame.Left + 10 &&
				firstNameEdit.Frame.Right == View.Frame.Right - 10 &&
				firstNameEdit.Frame.Top == bindButton.Frame.Bottom + 10 &&

				lastNameEdit.Frame.Left == firstNameEdit.Frame.Left &&
				lastNameEdit.Frame.Right == firstNameEdit.Frame.Right &&
				lastNameEdit.Frame.Top == firstNameEdit.Frame.Bottom + 10 &&
			
				fullNameLabel.Frame.Left == firstNameEdit.Frame.Left &&
				fullNameLabel.Frame.Right == firstNameEdit.Frame.Right &&
				fullNameLabel.Frame.Top == lastNameEdit.Frame.Bottom + 20);

			//
			// Databind the UI
			//
			binding = CreateBinding ();
		}

		Binding CreateBinding ()
		{
			return Binding.Create (() => 
				firstNameEdit.Text == person.FirstName && 
				lastNameEdit.Text == person.LastName && 
				fullNameLabel.Text == "Full Name: " + person.LastName + ", " + person.FirstName && 
				Title == person.FirstName + " " + person.LastName);
		}

		void HandleBindButton (object sender, EventArgs e)
		{
			if (binding != null) {
				binding.Unbind ();
				binding = null;
				bindButton.Selected = false;
			} else {
				binding = CreateBinding ();
				bindButton.Selected = true;
			}
		}
	}
}














