# RetroLinker
A helper for creating shortcuts of games that use libretro's emulator frontend RetroArch. It aims to help set most of the parameters that RetroArch takes and writes it to a shortcut file, along with shortcut an icon of your choice. Currently compatible with Windows Shortcut (.lnk) and linux desktop entry (.desktop).

As Windows Shortcuts can only use .ico for its icons, RetroLinker comes ready for converting .png, .jpeg and .svg images into .icos, as well to extract .icos from other Windows executable files (.exe only for now), that way you'll have more freedom on your icon choices!

## Compatible Retroarch parameters
- core*
- - with content
- - contentless
- custom Config file
- fullscreen
- verbose
- accesability

## Planned Retroarch parameters
- core subsystems
- rom softpatching
- append config

***This app is currently work-in-progress.***