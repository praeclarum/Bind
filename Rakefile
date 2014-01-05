require "rake/clean"

CLEAN.include "*.xam"
CLEAN.include "xamarin-component"

COMPONENT = "Bind-1.0.xam"

file "xamarin-component/xamarin-component.exe" do
	puts "* Downloading xamarin-component..."
	mkdir "xamarin-component"
	sh "curl -L https://components.xamarin.com/submit/xpkg > xamarin-component.zip"
	sh "unzip -o -q xamarin-component.zip -d xamarin-component"
	sh "rm xamarin-component.zip"
end

task :default => "xamarin-component/xamarin-component.exe" do
	line = <<-END
	mono xamarin-component/xamarin-component.exe create-manually #{COMPONENT} \
		--name="Bind" \
		--summary="Generic data binding." \
		--publisher="Frank A. Krueger" \
		--website="http://github.com/praeclarum/Bind" \
		--details="Details.md" \
		--license="License.md" \
		--getting-started="GettingStarted.md" \
		--icon="icons/Bind_128x128.png" \
		--icon="icons/Bind_512x512.png" \
		--library="ios":"bin/Bind.iOS.dll" \
		--library="android":"bin/Bind.Android.dll" \
		--sample="iOS Sample. Demonstrates Bind on iOS.":"samples/Bind.iOS.sln" \
		--sample="Android Sample. Demonstrates Bind on Android":"samples/Bind.Android.sln"
		END
	puts "* Creating #{COMPONENT}..."
	puts line.strip.gsub "\t\t", "\\\n    "
	sh line, :verbose => false
	puts "* Created #{COMPONENT}"
end
