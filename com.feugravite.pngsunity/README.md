# Portable Network Graphics Sequence File

This is a relatively simple file format I created in one day. It's structured similarly to *.gif files. The format was inspired by my old Baldi's Basics Plus mod codenamed "MLMBC" (Monday Left Me Broken Cat). This mod replaced all tile textures with MP4 files to animate the tiles.

Since Unity Engine doesn't support crisp, lossless compression for video files of any format and doesn't support GIFs either, I decided to create my own file format that stores a collection of *.png color bytes along with the duration of each "sequence."

## Fun Fact
Because this is a collection of PNG images, I named it "**Portable Network Graphics Sequence**" (*.pngs).
***.pngs (PNGs)** - can also serve as the plural form for PNG files!

## Unity Package

> Currently, the Unity Engine package is not finished, unstable. I do not recommend to use it

The only way to install a Unity Engine package is through Git and UPM (Unity Package Manager)
This method requires Git to be installed on your machine!

1. Open Unity Package Manager inside the engine
2. Click on a "+" icon that is near a thing labeled "Sort Names (asc)"
3. Select "Install package from a git URL..."
4. Paste this link
`https://github.com/blayms/Portable-Network-Graphics-Sequence-File.git?path=/com.feugravite.pngsunity`
5. You're good to go!

## Structure
![](https://file.garden/Z-1IetWhPAglb4Fv/pngsgithub.svg)
## Special Thanks
- [l4net by Krashan on NuGet](https://www.nuget.org/packages/lz4net/)