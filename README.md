# Praeclarum.Bind

Bind gives you easy two-way data binding between properties of objects. These objects can be UI elements, plain old data, or complex model objects, whatever.

Values are automatically updated if the object classes implement property changed events.

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

Bind can be included in your project by simply including `Bind.cs` in your project. It will work in any .NET 4.5 project.

There is even a Portable Class Library version of the project that works on Profile 78 (which includes everything except Silverlight 5).




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


#### Complex Equality Binding

Sometimes you will want to bind a transformation or composition of data.

Consider the case of displaying a person's full name and allowing them to enter that data using text boxes:

    class PersonViewController : UIViewController
    {
        UITextField firstNameEdit;
        UITextField lastNameEdit;
        UILabel fullNameLabel;

        Person person;

        public override void ViewDidLoad ()
        {
            Binding.Create (() => firstNameEdit.Text == person.FirstName);
            Binding.Create (() => lastNameEdit.Text == person.LastName);
            Binding.Create (() => fullNameLabel.Text == person.LastName + ", " + person.FirstName);
            Binding.Create (() => Title == person.LastName + ", " + person.FirstName);
        }
    }

Here we have bound `fullNameLabel.Text` and the view controller's `Title` to a complex expression involving two variables. When either of these values change, the text will be automatically updated.

Complex expression disrupt two-way databinding - updates will only flow from the complex side to the simple side.

Bindings with complex expressions on both sides are meaningless. (Technically, they define a algebraic loop that must be solved. I haven't implemented this and probably never will.)


#### Change Tracking

You must call `Invalidate` to update a binding if the value comes from an object that does not implement [Automatic Change Tracking][].

Invalidate takes a lambda returning the property that changed:

    Binding.Invalidate (() => obj.Property);


#### Automatic Change Tracking

Objects that implement the `INotifyPropertyChanged` interface or that has any of these events:

* *Property*Changed (where *Property* is the name of the property that causes the event)
* EditingDidEnd
* ValueChanged
* Changed

will be automatically change tracked.





#### Unbinding

Call `Unbind` on the object returned from `Binding.Create` to cancel the binding.



## Error Handling

If Bind runs into problems, it will raise the static event `Binding.Error`. The default behavior is to write a message to the debug console - binding errors do not raise exceptions.

If you want to debug these errors, create a global event handler and set a debug point:

    Bind.Error += message => {
        // Set a breakpoint here
    };

