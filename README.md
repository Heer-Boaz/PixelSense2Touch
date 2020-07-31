# Latest version binary download
https://github.com/Heer-Boaz/PixelSense2Touch/releases/tag/v1.0a

# PixelSense2Touch
App that adds touch support to Microsoft PixelSense 1.0 (Microsoft Surface) that runs on Windows 8+.

## Update - V2.0
The app is now able to handle multiple touches and translate those touches into
- Mouse clicks
- Right mouse clicks; place finger on screen for long time - longer than you would expect :-)
- Dragging
- Multi-touch events, such as pinching/zooming

## Prerequisites for running the app
- Microsoft PixelSense (Surface Table) 1.0
- Windows 8/10\*
- .NET Framework 4.7.2 run-time
- Microsoft Surface SDK 1.0 SP1
  - Be sure to download the SDK and place the DLLs in the outputfolder
  - You need the following assemblies:
    - Microsoft.Surface.Common.dll
    - Microsoft.Surface.Core.dll
    - Microsoft.Surface.Core.xml(?)
    - Microsoft.Surface.dll
    - Microsoft.Surface.xml(?)
    - Microsoft.Surface.Tools.dll

## Prerequisites for compiling the source
- VS2019
- .NET Framework 4.7.2 SDK
- Other libraries such as WPF
- Be sure to download the SDK and place the DLLs in folder *./PixelSense2Touch/lib*, as listed above. These files are copied to the output folder

\* See this post by Zac Bowden: https://www.windowscentral.com/windows-10-on-microsoft-surface-coffee-table and this post by Rajen: http://blog.rajenki.com/2014/02/modernizing-original-microsoft-surface/

---
## Steps to make this all work
1. Download binaries from GitHub.
2. Perform the steps as described in the prereqs-section!
3. Make sure that your PixelSense is running in *User Mode*, otherwise touch input will not be recognised.
4. Run the _Touch Input_ program that is installed as part of the Surface SDK and make sure it is running.
5. Run _PixelSenseToTouch.exe_.

When all is working, the app is currently able to translate finger (only) touches into mouse clicks, mouse holds (hold really long), drag and even pinching/zooming!

----
## Copyright / Licensing
PixelSense2Touch uses the following packages/libraries:
- TCD.System.TouchInjection by Michael (https://www.nuget.org/packages/TCD.System.TouchInjection/)
- InputSimulatorStandard (https://github.com/GregsStack/InputSimulatorStandard)
- Microsoft Surface SDK 1.0 SP1 (https://msdn.microsoft.com/en-us/library/ee804767(v=surface.10).aspx)