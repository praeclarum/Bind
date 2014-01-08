using NUnit.Framework;
using System;
using System.ComponentModel;

namespace Praeclarum.Bind.Test
{
	[TestFixture]
	public class ChangeTrackingTest
	{
		[SetUp]
		public void SetUp ()
		{
			Binding.Error += message => Console.WriteLine (message);
		}

		class PropertyChangedEventObject
		{
			string stringValue = "";

			public int StringValueChangedCount { get; private set; }

			EventHandler stringValueChanged; 
			public event EventHandler StringValueChanged 
			{ 
				add {
					stringValueChanged += value;
					StringValueChangedCount++;
				} 
				remove {
					stringValueChanged -= value;
					StringValueChangedCount--;
				}
			} 

			public string StringValue {
				get { return stringValue; }
				set {
					if (stringValue != value) {
						stringValue = value;
						if (stringValueChanged != null) {
							stringValueChanged (this, EventArgs.Empty);
						}
					}
				}
			}
		}

		[Test]
		public void PropertyChanged ()
		{
			var obj = new PropertyChangedEventObject {
				StringValue = "Hello",
			};
			var left = "";

			Binding.Create (() => left == obj.StringValue);

			Assert.AreEqual (1, obj.StringValueChangedCount);

			Assert.AreEqual (obj.StringValue, left);

			obj.StringValue = "Goodbye";

			Assert.AreEqual (obj.StringValue, left);
		}

		[Test]
		public void MultipleObjectPropertyChanged ()
		{
			var objA = new PropertyChangedEventObject {
				StringValue = "Hello",
			};
			var objB = new PropertyChangedEventObject {
				StringValue = "World",
			};
			var left = "";

			Binding.Create (() => left == objA.StringValue + ", " + objB.StringValue);

			Assert.AreEqual ("Hello, World", left);

			objA.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye, World", left);

			objB.StringValue = "Mars";

			Assert.AreEqual ("Goodbye, Mars", left);

		}

		[Test]
		public void MultiplePropertyChanged ()
		{
			var obj = new PropertyChangedEventObject {
				StringValue = "Hello",
			};
			var leftA = "";
			var leftB = "";

			Binding.Create (() => leftA == obj.StringValue);
			Binding.Create (() => leftB == obj.StringValue + "...");

			Assert.AreEqual (1, obj.StringValueChangedCount);

			Assert.AreEqual ("Hello", leftA);
			Assert.AreEqual ("Hello...", leftB);

			obj.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye", leftA);
			Assert.AreEqual ("Goodbye...", leftB);
		}

		[Test]
		public void RemoveMultiplePropertyChanged ()
		{
			var obj = new PropertyChangedEventObject {
				StringValue = "Hello",
			};
			var leftA = "";
			var leftB = "";

			var bA = Binding.Create (() => leftA == obj.StringValue);
			var bB = Binding.Create (() => leftB == obj.StringValue + "...");

			Assert.AreEqual (1, obj.StringValueChangedCount);

			Assert.AreEqual ("Hello", leftA);
			Assert.AreEqual ("Hello...", leftB);

			obj.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye", leftA);
			Assert.AreEqual ("Goodbye...", leftB);

			bA.Unbind ();

			Assert.AreEqual (1, obj.StringValueChangedCount);

			obj.StringValue = "Hello Again";

			Assert.AreEqual ("Goodbye", leftA);
			Assert.AreEqual ("Hello Again...", leftB);

			bB.Unbind ();

			Assert.AreEqual (0, obj.StringValueChangedCount);

			obj.StringValue = "Goodbye Again";

			Assert.AreEqual ("Goodbye", leftA);
			Assert.AreEqual ("Hello Again...", leftB);
		}


		[Test]
		public void RemovePropertyChanged ()
		{
			var obj = new PropertyChangedEventObject {
				StringValue = "Hello",
			};
			var left = "";

			var b = Binding.Create (() => left == obj.StringValue);

			Assert.AreEqual (1, obj.StringValueChangedCount);

			Assert.AreEqual (obj.StringValue, left);

			obj.StringValue = "Goodbye";

			Assert.AreEqual (obj.StringValue, left);

			b.Unbind ();

			Assert.AreEqual (0, obj.StringValueChangedCount);

			obj.StringValue = "Hello Again";

			Assert.AreEqual ("Goodbye", left);
		}

		class NotifyPropertyChangedEventObject : INotifyPropertyChanged
		{
			public int PropertyChangedCount { get; private set; }

			PropertyChangedEventHandler propertyChanged; 
			public event PropertyChangedEventHandler PropertyChanged 
			{ 
				add {
					propertyChanged += value;
					PropertyChangedCount++;
				} 
				remove {
					propertyChanged -= value;
					PropertyChangedCount--;
				}
			} 

			string stringValue = "";

			public string StringValue {
				get { return stringValue; }
				set {
					if (stringValue != value) {
						stringValue = value;
						if (propertyChanged != null) {
							propertyChanged (this, new PropertyChangedEventArgs ("StringValue"));
						}
					}
				}
			}

			int intValue = 0;

			public int IntValue {
				get { return intValue; }
				set {
					if (intValue != value) {
						intValue = value;
						if (propertyChanged != null) {
							propertyChanged (this, new PropertyChangedEventArgs ("IntValue"));
						}
					}
				}
			}
		}

		[Test]
		public void NotifyPropertyChanged ()
		{
			var obj = new NotifyPropertyChangedEventObject {
				StringValue = "Hello",
			};
			var left = "";

			Binding.Create (() => left == obj.StringValue);

			Assert.AreEqual (1, obj.PropertyChangedCount);

			Assert.AreEqual (obj.StringValue, left);

			obj.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye", left);
		}

		[Test]
		public void RemoveNotifyPropertyChanged ()
		{
			var obj = new NotifyPropertyChangedEventObject {
				StringValue = "Hello",
			};
			var left = "";

			var b = Binding.Create (() => left == obj.StringValue);

			Assert.AreEqual (1, obj.PropertyChangedCount);

			Assert.AreEqual (obj.StringValue, left);

			obj.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye", left);

			b.Unbind ();

			Assert.AreEqual (0, obj.PropertyChangedCount);

			obj.StringValue = "Hello Again";

			Assert.AreEqual ("Goodbye", left);
		}

		class NotEventHandlerObject
		{
			string stringValue = "";

			public int StringValueChangedCount { get; private set; }

			Action<string, string> stringValueChanged; 
			public event Action<string, string> StringValueChanged 
			{ 
				add {
					stringValueChanged += value;
					StringValueChangedCount++;
				} 
				remove {
					stringValueChanged -= value;
					StringValueChangedCount--;
				}
			} 

			public string StringValue {
				get { return stringValue; }
				set {
					if (stringValue != value) {
						var oldValue = stringValue;
						stringValue = value;
						if (stringValueChanged != null) {
							stringValueChanged (oldValue, stringValue);
						}
					}
				}
			}
		}

		[Test]
		public void NotifyNotEventHandler ()
		{
			var obj = new NotEventHandlerObject {
				StringValue = "Hello",
			};
			var left = "";

			var b = Binding.Create (() => left == obj.StringValue);

			Assert.AreEqual (1, obj.StringValueChangedCount);

			Assert.AreEqual (obj.StringValue, left);

			obj.StringValue = "Goodbye";

			Assert.AreEqual ("Goodbye", left);

			b.Unbind ();

			Assert.AreEqual (0, obj.StringValueChangedCount);

			obj.StringValue = "Hello Again";

			Assert.AreEqual ("Goodbye", left);
		}
	}
}

