Locale Emulator
===============

[![license](https://img.shields.io/github/license/xupefei/Locale-Emulator.svg)](https://www.gnu.org/licenses/lgpl-3.0.en.html)
[![AppVeyor](https://img.shields.io/appveyor/ci/xupefei/Locale-Emulator.svg)](https://ci.appveyor.com/project/xupefei/Locale-Emulator)
[![Github All Releases](https://img.shields.io/github/downloads/xupefei/Locale-Emulator/total.svg)](https://github.com/xupefei/Locale-Emulator/releases)
[![GitHub release](https://img.shields.io/github/release/xupefei/Locale-Emulator.svg)](https://github.com/xupefei/Locale-Emulator/releases/latest)

Yet Another System Region and Language Simulator

![LE interface](https://i.imgur.com/E4Gqyly.png)

## Download ##

Download available at <https://github.com/xupefei/Locale-Emulator/releases>.

For usage, please read <https://xupefei.github.io/Locale-Emulator/> (in English and 中文).

## Translate ##

If you want to help translating Locale Emulator, you can find all strings in

 -  `DefaultLanguage.xaml` in `LEGUI/Lang` folder.
 -  `DefaultLanguage.xml` in `LEContextMenuHandler/Lang` folder.

After you translated the above files into your language, please inform me by creating a pull request.

## Build ##

 1. Clone the repo using Git.
 2. Install Microsoft Visual Studio 2015 / 2017.
 3. Open `LocaleEmulator.sln`.
 4. Perform Build action.
 5. Clone and build the core libraries: https://github.com/xupefei/Locale-Emulator-Core
 6. Copy LoaderDll.dll and LocaleEmulator.dll from Locale-Emulator-Core to Locale-Emulator build folder.

## License ##

![LGPL](https://www.gnu.org/graphics/lgplv3-147x51.png)

`LEContextMenuHandler` project use source codes from [Microsoft All-In-One Code Framework](https://blogs.msdn.com/b/onecode/) which is licensed by [Microsoft Public License](https://www.microsoft.com/en-us/openness/licenses.aspx#MPL).

[Flat icon set](commit/eae9fbc27f1a4c85986577202b61742c6287e10a) from [graphicex](https://graphicex.com/icon-and-logo/15983-flat-alphabet-in-9-colors-with-long-shadow-6913875.html).

All other source code files are licensed under [LGPL-3.0](https://opensource.org/licenses/LGPL-3.0).

If you want make any modification on these source codes while keeping new codes not protected by LGPL-3.0, please contact me for a sublicense instead.
