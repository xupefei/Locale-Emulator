Locale Emulator
===============

[![license](https://img.shields.io/github/license/xupefei/Locale-Emulator.svg)](https://www.gnu.org/licenses/lgpl-3.0.en.html)
[![AppVeyor](https://img.shields.io/appveyor/ci/xupefei/Locale-Emulator.svg)](https://ci.appveyor.com/project/xupefei/Locale-Emulator)
[![Github All Releases](https://img.shields.io/github/downloads/xupefei/Locale-Emulator/total.svg)](https://github.com/xupefei/Locale-Emulator/releases)
[![GitHub release](https://img.shields.io/github/release/xupefei/Locale-Emulator.svg)](https://github.com/xupefei/Locale-Emulator/releases/latest)

Yet Another System Region and Language Simulator

![LE interface](http://i.imgur.com/E4Gqyly.png)

## Download ##

Download available at <https://github.com/xupefei/Locale-Emulator/releases>.

For usage, please read <http://xupefei.github.io/Locale-Emulator/> (in English and 中文).

## Translate ##

If you want to help translating Locale Emulator, you can find all strings in

 -  `DefaultLanguage.xaml` in `LEGUI/Lang` folder.
 -  `DefaultLanguage.xml` in `LEContextMenuHandler/Lang` folder.

After you translated the above files into your language, please either create a pull request or submit a ticket to notify me about that.

## Build ##

 1. Clone the repo using Git.
 2. Install Microsoft Visual Studio 2015 / 2017.
 3. Open `LocaleEmulator.sln`.
 4. Perform Build action.

## Submit Issue ##

*Due to the limited effort, [we only accept issues under Windows 10 OS](https://github.com/xupefei/Locale-Emulator/wiki/Stopping-support-for-old-Windows-OS). If you are using another version of Windows, you may fix it by yourself.*

You can submit an issue if any application is not working (but you think should work) with Locale Emulator. Before submitting, please turn-off your antivirus and protection software and try again.

If you decide to submit a ticket, please indicate the following information in the issue:

 - Your system type (32 / 64 / 65536bit).
 - Name and company of the broken application.
 - Error message produced by Locale Emulator, including error number, application path, version of Locale Emulator and UAC information.
 -  Attach a screen capture if the application runs but not running correctly.

## Feature Request ##

You can also submit an issue if you have some wonderful ideas to improve Locale Emulator. Then I *may* accept it because I want to make Locale Emulator *small*. So if your idea is to add automatic translation function to Locale Emulator, please fork this repo and do it yourself.

## License ##

![enter image description here](http://www.gnu.org/graphics/lgplv3-147x51.png)

`LEContextMenuHandler` project use source codes from [Microsoft All-In-One Code Framework](http://blogs.msdn.com/b/onecode/) which is licensed by [Microsoft Public License](http://www.microsoft.com/en-us/openness/licenses.aspx#MPL).

[Flat icon set](commit/eae9fbc27f1a4c85986577202b61742c6287e10a) from [graphicex](http://graphicex.com/icon-and-logo/15983-flat-alphabet-in-9-colors-with-long-shadow-6913875.html).

All other source code files are licensed under [LGPL-3.0](https://opensource.org/licenses/LGPL-3.0).

If you want make any modification on these source codes while keeping new codes not protected by LGPL-3.0, please contact me for a sublicense instead.
