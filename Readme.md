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

### Missing Scripts Finder

Allow to find objects with Missing Script references in both prefabs and scenes.

Just open the window from the Content Extensions/Missing Script Finder menu entry
and either click find in Assets or find in Current Scene

### Package Manager Check

Separate assembly that allow to check if a package is part of the project on first
import and add it if it's missing. Used by the Content Team when distributing a 
full project on the Asset Stores that requires packages (as the Asset Store Tool 
don't include yet the Packages Manifest).

Require to be placed in an editor folder with a file somewhere (ideally next to it)
called `PackageImportList.txt` that containt a list of the package of the form :
`com.unity.package@version`.

e.g
```com.unity.postprocessing@2.0.3-preview
com.unity.cinemachine@2.1.12
com.unity.probuilder@3.0.3
com.unity.textmeshpro@1.2.2
```

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
