 # PixelSense2Touch
App for converting input from the Microsoft PixelSense 1.0 (Microsoft Surface Table) to touch input for Windows 10. The aim is to allow a user to operate the PixelSense as she would a tablet.
In its current state, it is only able to recognize a single finger and some basic (i.e. tap and scroll) interactions.
Future work will need to also allow:
- Multitouch;
- Operations such as right-mouse clicks by prolonged tapping;
- Etc.

## Prerequisites
- Microsoft PixelSense (Surface Table) 1.0
- Windows 8/10\*
- .NET 4.5.2
- Microsoft Surface SDK 1.0 SP1

\* See this post by Zac Bowden: https://www.windowscentral.com/windows-10-on-microsoft-surface-coffee-table and this post by Rajen: http://blog.rajenki.com/2014/02/modernizing-original-microsoft-surface/

## Steps to make this work
1. Download binaries from GitHub.
2. Make sure that your PixelSense is running in *User Mode*, otherwise touch input will not be recognised.
2. Run the _Touch Input_ program that is installed as part of the Surface SDK and make sure it is running.
3. Run _PixelSenseToTouch.exe_.

When all is working, the app is currently able to recognize single touch events from a single finger.

----
## Copyright / Licensing
PixelSense2Touch uses the following packages/libraries:
- TCD.System.TouchInjection by Michael (https://www.nuget.org/packages/TCD.System.TouchInjection/)
- Microsoft Surface SDK 1.0 SP1 (https://msdn.microsoft.com/en-us/library/ee804767(v=surface.10).aspx)
