Tool Collection
===============

This repository is used to gather together all the little editor script & tools
that we write for different projects.

**Note : all those tools are WIP and experimental and sometime not the most stable.
Use at your own risk. Be sure that your project is on source control so if some
delete files you can recovers them!**

Each live in its own folder under the Assets folder.

Through the **Package Designer** tool (see wiki for documentation) each tool can
be easily export to a unity Package to be imported by itself inside a projects

*Longer term goal is to have a build computer somewhere exporting those package
automatically from time to time & upload them somewhere so people can just go
and download the unitypackage directly*

## List of included tools

### Reference Finder

A tool to find all reference to a given function used in Unity Event inside the
editor. Allow to check before removing a function that seemed used nowhere that
it is not referenced by a Unity Event (and so would only fail during execution)

### Package Designer

The Package Designer is a tool that allow to define package from assets, save
that configuration as asset in the project and export them into a unitypackage
through a single click.

It also allow to reorganize said assets in a different hierarchy than the one
they are in the project before export (e.g you may want to group mesh, material
  and textures of a character inside a folder with the character name instead
  of in 3 differents mesh/material/texture folder in the root of the package)

See the Wiki for more documentation

### Preset Importer

The Preset Importer allow to define preset to apply on import to file that
match a given filter.

One example is to make a preset to import all file having `_Normal` in their
filename as Normal.

See Wiki for more info on how to use
