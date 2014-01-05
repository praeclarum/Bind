Bind allows you to bind two object graphs together so that their values remain in sync.

This is especially useful when creating UI code where you want to display model objects.

	using Praeclarum.Bind;

	class PersonViewController : UIViewController
	{
		UILabel nameLabel;

		Person person;

		public override void ViewDidLoad ()
		{
			Binding.Create (() => nameLabel.Text == person.Name);
		}
	}
