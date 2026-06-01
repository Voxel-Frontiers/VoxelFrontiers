Godot Lua API DotNet Notes
===============

<!-- TOC -->
* [Godot Lua API DotNet Notes](#godot-lua-api-dotnet-notes)
  * [Prerequisites](#prerequisites)
  * [Introduction](#introduction)
	  * [Helpful places:](#helpful-places)
  * [Commands](#commands)
	  * [Method 1](#method-1)
	  * [Method 2](#method-2)
  * [Troubleshooting](#troubleshooting)
<!-- TOC -->

Prerequisites
-------

You will need DotNet 7 (as of Dec 2023) installed to compile / use the software. Install the version for your OS. You 
will also need the Godot LUA Api Editor (mono version). The standard Godot Editor will not work, as it is missing the 
LUA API layer that this project requires.

To ensure that your dotnet works, use these two commands:

* `dotnet --list-sdks`
* `dotnet --list-runtimes`

This will show you the available runtime and sdks installed, and their locations. Make sure that you see the version 
required for the project. 

You will need to know what commands to use, and where to use them, so a familiarity with 
dotnet, or the ability to look them up online, is necessary. You will also need to know how to use a command prompt. For
windows users, you may need to know how to use an administrative command prompt.

Introduction
-------

When working with this version of the editor, or the extension addon, you will need to use specific versions of the
nuget packages for your project. This has been tested with v2.1-beta9.

For this document, we will use *nix style paths. If you are a Windows user, please convert lines like `/path/to/` to
`C:\path\to\` so that it will work with your system. MacOS users can use the *nix paths.

The editor builds have a file named `mono_instructions.txt` included in the zipped release.
The contents of this file are as follows:

```
If you have GodotSharp locally cached dotnet may use one of them instead. To clear local cache run the following command:
dotnet nuget locals all --clear

To add the local nuget source, please run the following command:
dotnet nuget add source /path/to/nuget_packages --name LuaAPINugetSource
```

What this is saying is that you will want to open up a command prompt and do the following steps to install the correct
nuget packages into your project. If you have downloaded the editor zip file, it comes with a directory named 
`nuget_packages` This is the directory that you will want to include into the second step in the instructions file. 
So, if you have extracted the Editor to `C:\Users\username\Godot` the correct path to use would be 
`C:\Users\username\Godot\nuget_packages`. Likewise, if you extracted the editor to `/home/username/Godot/` you would 
want to use `/home/username/Godot/nuget_packages`.

Make sure to enclose directories with spaces with either a `"` or `'` so that the path resolves correctly.

Additionally, you will want to execute a `dotnet restore` command to install the correct nuget packages. See
Commands below for an example on how to do this.

There are 2 methods of adding the nuget packages as a local source. Most users can use the main method. However, if you 
are one of the ones that cannot, use method 2. The `mono_instructions.txt` lightly outlines the main method.

#### Helpful places:
Your nuget directory is your home / user directory -> `/.nuget/packages`. So, for *nix, it would be 
`/home/username/.nuget/packages`. 
And your SDK location (on Windows) is in `"C:\Program Files\dotnet\sdk\7.0.404\Sdks"`. On *nix, it can either be 
located in the `/usr` directory, or in the user's home directory in the `.dotnet` directory. 

If you encounter issues, these locations will be needed (especially for windows users).

Commands
-------

#### Method 1

In your command prompt, execute these commands in the following order.

`dotnet nuget locals all --clear`  -- This command clears the local package directory, so if updating, copy the proper
packages from the archive, so that they are there before proceeding.

`dotnet nuget add source /path/to/nuget_packages --name LuaAPINugetSource` -- Only necessary if the source hasn't yet been added.

`dotnet restore '/pathtoproject/example_project.csproj' -f -s  LuaAPINugetSource`

This will set up the proper packages to work with the Editor / Add-on. Note: you may have to select the correct nuget
source within your IDE. If so, please use the `LuaAPINugetSource` option. Note that in the third command, we are using
the specific location (`-s <source>`) and we are forcing (`-f`) the restore. This is done to specifically use the custom
nuget packages. If you get errors, you can use `dotnet nuget list source` to list the sources. Also, there's a `remove`
command that will allow you to redo the source add. Note that putting a trailing `/` on the end of the path will generally
mess up the local source.

#### Method 2

In some cases the packages will fail to restore, and if that happens to you, this is something that you can do to try
to make it work. You will need to remove the existing `LuaAPINugetSource` that you made above, and then put this file
in your project directory. As it uses an absolute path, others will need to change it to their location, if they are
part of your team. (Like an open source project, or if they are compiling the code themselves.)

* Clear, then Remove the above created nuget source.
* Create a nuget.config file in the root of your project or solution (if it doesn't already exist).
* Open or create the nuget.config file, and add the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
	<add key="LuaAPINugetSource" value="/path/to/editor/editor-mono/nuget_packages" />
  </packageSources>
</configuration>
```

* Replace `/path/to/editor/editor-mono/nuget_packages` with the correct path to your local NuGet source. Note the
  lack of a trailing slash.
* Save the nuget.config file.
* Perform a `dotnet restore '/pathtoproject/example_project.csproj' -f -s  LuaAPINugetSource` to restore the packages.

With this configuration, the NuGet restore process will automatically consider the sources listed in the nuget.config.

Once you have done this, you will need to rebuild your project. You can do so either through your IDE or inside of the
Godot Editor. I highly recommend keeping this file handy, as you will need to use commands from here when you update the 
editor to new versions. If you can, it's advisable to change the nuget sources so that the local source is first in the
list (from within an IDE). This will make life easier. And then, with new projects, run the clear command, then restore 
the project in your IDE, or simply build the project in Godot. (Godot will perform a restore in the build process.) By 
having the local sources first in the list, this will ensure that they are put in, and that your project will work.

A note on the LuaAPI specific nuget packages: They are included in the Mono (DotNet) builds from the `Releases` tab on
the Github page.

Troubleshooting
------

The intention of this entire file is to help you get up and running. And for most users it will do that. However, as we 
recently experienced, that's not always the case. Especially for Windows Users. As you can see in the note below, 
Windows likes to block and prevent users from running things correctly. We encountered issues with unzipping the source 
code that was downloaded, DotNet not being able to install local nuget packages, etc. And, it's just not possible to 
cover all of the different scenarios that can occur. I have a hunch that it might be because of certain 
`Developer Options` not being turned on in the System Settings, but I cannot say for certain that is the only thing that 
should be changed / set.

```
Note: Windows users may have other issues with dotnet, files being marked unsafe because they originated from other
computers, not being done as an administrator, etc. Sadly, those are on the user to fix as it is beyond the scope of a
getting started file. In some instances, Windows users will have to manually copy files from their .nuget directory into 
the dotnet Sdks directory in order for dotnet to actually compile / run the project.
```

For Linux Users, there's 3-4 different ways to install DotNet, and each can cause you to rethink your life choices lol. 
The easiest way that we found, is the way that installs DotNet to the `~/.dotnet` directory. But, even so, you will 
have to set up the `.profile` or `.bashprofile` (or similar) and reboot, to make things work. Oddly, dotnet seems to 
work better on Linux than it does on Windows. Still, there may be "Some Assembly Required."

We may be able to help you, if you contact us through our discord. Use this link: https://discord.gg/5ZnuVhAAqF.

Lastly, if you change the paths without updating what uses them, you will break things. We recommend that you use a 
generic directory name for the Editor, like `Godot LuaApi` or something similar. That way, if you upgrade the editor, 
you are not breaking the path to the nuget local sources.  
