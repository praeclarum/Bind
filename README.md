# Praeclarum.Bind

Bind allows you to bind two object graphs together so that their values remain in sync. The binding is two-way, so any edits to either graph will be reflected in the other graph.

This is especially useful when creating UI code where you want to display and edit model values.

    using Praeclarum.Bind;

    class PersonViewController : UIViewController
    {
        UITextField nameEdit;

        Person person;

        public override void ViewDidLoad ()
        {
            Binding.Create (() => nameEdit.Text == person.Name);
        }
    }




## Installation

Bind can be included in your project by simply including `Bind.cs` in your project. It will work in any .NET 4 project.

(Bind will not work in Windows Store apps because Microsoft broke System.Reflection's API. Write to your local evangelist and tell them to fix it.)




## Usage

### Equality Binding

Equality binding is the simplest use of the library. Equality bindings are specified using the `==` operator in a call to `Binding.Create`:

    Binding.Create (() => left == right);

where `left` and `right` are two values.

This binding will attempt to keep the values of `left` and `right` in sync. That is, if `right` changes, so will `left`.

When initialized, the binding will attempt to assign `right` to `left`. If `left` is constant, then it will do the reverse, assign `left` to `right`.

`Left` and `right` can be any expression ranging from simple constants up to long object walks:
    
    Binding.Create (() => stateEdit.Text == person.Address.State);

When this binding is created, the value of `person.Address.State` is assigned to the edit control's `Text` property. If the user changes that text, the values will be written back to `person.Address.State`.

Bindings are symmetric, so you could just as well have written:

    Binding.Create (() => person.Address.State == stateEdit.Text);

Then only difference occurs at initialization: the `stateEdit.Text` value is assigned to the `person.Address.State` value instead of the other way around.




## Error Handling

If Bind runs into problems, it will raise the static event `Binding.Error`. The default behavior is to write a message to the debug console - binding errors do not raise exceptions.

If you want to debug these errors, create a global event handler and set a debug point:

    Bind.Error += message => {
        // Set a breakpoint here
    };

