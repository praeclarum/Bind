# Praeclarum.Bind

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

# Installation

Bind can be included in your project by simply including `Bind.cs` in your project. It will work in any .NET 4.5 project.

Bind will not work in Windows Store apps because Microsoft broke System.Reflection's API. Write to your local evangelist and tell them to fix it.


