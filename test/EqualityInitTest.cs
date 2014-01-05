using NUnit.Framework;
using System;

namespace Praeclarum.Bind.Test
{
	[TestFixture]
	public class EqualityInitTest
	{
		[SetUp]
		public void SetUp ()
		{
			Binding.Error += message => Console.WriteLine (message);
		}

		[Test]
		public void LocalLeftInit ()
		{
			var left = "";
			var right = "hello";

			Binding.Create (() => left == right);

			Assert.AreEqual (left, right);
			Assert.AreEqual (left, "hello");
		}

		[Test]
		public void LocalRightInit ()
		{
			var left = "hello";
			var right = "";

			Binding.Create (() => left == right);

			Assert.AreEqual (left, right);
			Assert.AreEqual (left, "");
		}

		class TestObject
		{
			public int State { get; set; }
		}

		[Test]
		public void LocalLeftObjectInit ()
		{
			TestObject left = null;
			TestObject right = new TestObject ();

			Binding.Create (() => left == right);

			Assert.AreSame (left, right);
			Assert.IsNotNull (left);
		}

		[Test]
		public void LocalRightObjectInit ()
		{
			TestObject left = new TestObject ();
			TestObject right = null;

			Binding.Create (() => left == right);

			Assert.AreSame (left, right);
			Assert.IsNull (left);
		}

		[Test]
		public void LocalAndPropInit ()
		{
			var left = 69;
			TestObject right = new TestObject {
				State = 42,
			};

			Binding.Create (() => left == right.State);

			Assert.AreEqual (left, right.State);
			Assert.AreEqual (left, 42);
		}

		[Test]
		public void PropAndLocalInit ()
		{
			TestObject left = new TestObject {
				State = 42,
			};
			var right = 1001;

			Binding.Create (() => left.State == right);

			Assert.AreEqual (left.State, right);
			Assert.AreEqual (left.State, 1001);
		}

		static int Method() { return 33; }

		[Test]
		public void LocalAndMethodInit ()
		{
			var left = 0;

			Binding.Create (() => left == Method ());

			Assert.AreEqual (left, 33);
		}

		[Test]
		public void MethodAndLocalInit ()
		{
			var right = 42;

			Binding.Create (() => Method () == right);

			Assert.AreEqual (right, 33);
		}
	}
}

























