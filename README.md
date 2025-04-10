# Portable Network Graphics Sequence File

This is a relatively simple file format I created in one day. It's structured similarly to *.gif files. The format was inspired by my old Baldi's Basics Plus mod codenamed "MLMBC" (Monday Left Me Broken Cat). This mod replaced all tile textures with MP4 files to animate the tiles.

Since Unity Engine doesn't support crisp, lossless compression for video files of any format and doesn't support GIFs either, I decided to create my own file format that stores a collection of *.png color bytes along with the duration of each "sequence."

**Note:** I haven't included Unity Engine support yet, but it's coming soon!

## Fun Fact
Because this is a collection of PNG images, I named it "**Portable Network Graphics Sequence**" (*.pngs).

***.pngs (PNGs)** - can also serve as the plural form for PNG files!

## Structure
![](https://file.garden/Z-1IetWhPAglb4Fv/pngsgithub.svg)
## Special Thanks
- [l4net by Krashan on NuGet](https://www.nuget.org/packages/lz4net/)
