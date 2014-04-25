using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Bind.iOS.Sample
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//
			// Create a view controller showing a list of people
			//
			var personVC = new PersonViewController (new Person ());

			//
			// Create and populate the main window
			//
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.RootViewController = new UINavigationController (personVC);
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

