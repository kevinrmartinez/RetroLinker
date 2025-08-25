# RetroLinker
A helper for creating shortcuts of games that use Libretro's emulator frontend RetroArch. It aims to help set most of the parameters that RetroArch takes and writes it to a shortcut file for you, along with an icon of your choice. Currently compatible with the Windows shortcut (.lnk) and Linux desktop entry (.desktop)[(FreeDesktop spec)](https://specifications.freedesktop.org/desktop-entry-spec/latest/).

As Windows Shortcuts can only use .ico for its icons, RetroLinker comes ready for converting .png, .jpeg and even .svg images into .ico files, as well to extract .ico files from other Windows executable files (.exe only for now), that way you'll have more freedom on your icon choices!

## Features
- Let the user create a custom link per game for desktop integration.
- In Windows, the user can select .PNGs, .JPGs or .SVGs and convert them to .ICOs to use them on their links.
- Has a list of recommended output directories for better desktop integration.
- In Linux, it auto assigns executable permissions to the link for a seamless experience.
- For faster link creation, commonly used icons and Configs files can be stored via the app settings.
- Ready to be translated in multiple languages.

## Compatible RetroArch parameters
- core*
  - with content
  - contentless
- rom soft-patching
- custom Config file
- appending Config files
- fullscreen
- verbose
- accessibility
- load menu on error

## Captures
![Main Application Window](MainApp.png)

![Application Window - Mario World Example](MainApp_Ex01.png)

![Application Window - Touhou Example](MainApp_Ex02.png)

![Application Window - 2048 Example](MainApp_Ex03.png)

In the following capture, we can see that the example from before are now part of the Games category on the system apps menu.
![Examples added to System Applications](SystemMenu_Ex01.png)

## Planned RetroArch parameters
- core subsystems
- log-file
- environment variables (all)

## Restrictions
- This app won't ask for admin permissions; reading and writing to paths without the proper permissions will simple result in an error.
- This app won't check if the core really exist, or if the executable is really RetroArch's, so user miss-inputs will likely create an undesirable link.   

***This app is currently work-in-progress.***