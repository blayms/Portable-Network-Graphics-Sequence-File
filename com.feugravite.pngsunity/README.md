
# Portable Network Graphics Sequence Unity Engine Package
### An official Unity Engine support for PNGS file format made by the developer (Blayms)!
> [More about Portable Network Graphics Sequence here](https://github.com/blayms/Portable-Network-Graphics-Sequence-File)

In short, PNGS files behave similarly to *.gif, but heavily rely on *.png pixel data instead, which allows solid transparency support straight out of the box! — *.apng analog made by Blayms.

## Unity Package Features

- **Native object wrapper specifically designed for Unity Engine**
Obviously, to add support for these files, the wrappers had to be included, which extend and express the possibilities of that file format.
  - **Custom asset importer**
    - PngSequenceTextureFileUnity - can be imported if no metadata that defines Unity type was found in a file
    - PngSequenceSpriteFileUnity - can be imported only if metadata `unityType=sprite` was detected
  - **Custom inspectors**
    - PNGS asset inspector also contains a preview window with a little player
  - **Custom icons**
    - Made by Blayms 
- **A RenderTexture player (PngSequenceBasicPlayer)**
Plays *.pngs files straight up on a RenderTexture, which can be used on things like UI or Materials (e.g., for meshes)

- **A SpriteRenderer player (PngSequenceSpritePlayer)**
Plays *.pngs files straight up on a SpriteRenderer, which can be used in 2D or even 2.5D games

## How to Install
The only way to install a Unity Engine package is through Git and UPM (Unity Package Manager)
This method requires [Git](https://git-scm.com/downloads) to be installed on your machine!

1. Open Unity Package Manager inside the engine
2. Click on a "+" icon that is near a thing labeled "Sort Names (asc)"
3. Select "Install package from a git URL..."
4. Paste this link
`https://github.com/blayms/Portable-Network-Graphics-Sequence-File.git?path=/com.feugravite.pngsunity`
5. You're good to go!
