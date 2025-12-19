# BondInspector
BondInspector is a hacked together Fiddler inspector for Bond protocol based on the code written by dend in [dend/bond-reader](https://github.com/dend/bond-reader).

### Open the source project
The project references Fiddler.exe itself and it's expected to be already installed on the development computer, located at "%LOCALAPPDATA%\Programs\Fiddler\Fiddler.exe".
1) Project reference -%LOCALAPPDATA%\Programs\Fiddler\Fiddler.exe
2) Start external program - %LOCALAPPDATA%\Programs\Fiddler\Fiddler.exe
3) Post-build event copies the output assembly to user's profile - %USERPROFILE%\Documents\Fiddler2\Inspectors

Tune the project setting based on your locations.

### Installation
Build the code. Ensure "BondInspector.dll" assembly was copied to C:\Users\Jason\Documents\Fiddler2\Inspectors directory along with various Bond.dlls - it should happen automaticaly after build.

### Supported Fiddler versions
Tested with Fiddler classic v5.0.20253.3311 
