# Voxelfronteirs
==========================

<!-- TOC -->
* [# VoxelFrontiers](#-VoxelFrontiers)
  * [What it is](#what-it-is)
  * [How to use](#how-to-use)
  * [Contributing](#contributing)
  * [License](#license)
  * [Code Of Conduct](#code-of-conduct)
<!-- TOC -->

What it is
-------
VoxelFrontiers is an engine designed to run the VoxeLibre (Originally) MineClone 2 Luanti (Fromally) Minetest Game.
After having developed for the VoxeLibre (formerly) Mineclone 2 team, and realizing that the Luanti (Originally) Minetest developers
were more than complacent to leave 5-10 year old issues unanswered, even after most of the 
developers had decided that it was a great idea, and had given their consent on the PR's 
that fixed said issues, that they would never merge the PRs. Instead, we watched them as a 
whole engage in blaming their engine's game developers "for shoddy, dumpster fire code" 
instead of making real and much needed inprovements to their engine.

So, I decided to step back from the main VoxeLibre (formerly) Mineclone 2 Development team, to work on this 
project. To give life to an engine that would help the game that I (and many others) love. 
And so, with a lot of prodding, and some pestering, from my good friend WBJ I decided to 
build this from just fanciful thoughts into a real, honest engine.

Initial versions of this project were created In Unity/C#. But, with the recent events, 
and Unity being a dumpster fire (as well as closed source, proprietary software) I have
moved the project over to the Godot engine 4.2 stable-mono-official (.NET Version). 
VoxeLibre (Originally) Mineclone 2 is open source, this engine's code is open source, and Godot is open source. 
So, it all came together as a perfect fit.

How to use
-------
To get started with using, developing, and contributing, these are the links that you will need. (Requires knowledge of DotNet, or a willingness to look up commands that you will be using, and potentially troubleshoot things along the way.)

https://codeberg.org/VoxelFrontiers/VoxelFrontiers (Engine Source code)
https://codeberg.org/VoxelFrontiers/i18nscan (Translation Program)
https://github.com/WeaselGames/godot_luaAPI/releases/tag/v2.1-beta11 (Godot Editor with LuaAPI built in.) DO NOT USE THE STANDARD GODOT EDITOR
See https://luaapi.weaselgames.info/v2.1/getting_started/dotnet/ for additional help.
This (https://codeberg.org/VoxelFrontiers/VoxelFrontiers/COMPILE.md) contains a condensed version of this.

The primary links that you need are the Engine Source code (which has instructions) and the Godot Editor link. Use the instructions on Engine source code page (the readme) and to know what to download and install. Note that beta9 is the latest version of the editor that this project used at the time of writing. The project will use the latest version of the editor, which matches the latest version of the Godot Engine (editor).
Note: you will need to use this specific editor, as the basic Godot editor does not have the LUA API built in. (You'll also want the required export templates too.)

You will need at least DotNet 7 installed. (Preferably have 7/8/9 installed.)

Feel free to ask questions in the ⁉-support or development channel.

Place Ideas in the dev-notes channel. (Soon there will be a suggestions channel for user made suggestions.) (edited)


If you get this style of error, the type namespace name 'LUAAPI' could not be found use this information to fix that:
https://luaapi.weaselgames.info/v2.1/getting_started/dotnet/


Without the local nuget packages set up properly, the project will not compile, and you will get many, many namespace / using directive missing errors, so make sure to follow the instructions in the getting_started/dotnet/ link.
if you have an IDE that handles Nuget Packages, you can verify that things are set up properly by going to the Manage Nuget Packages (or similar) spot, and looking at the installed packages. You should see something that looks like this:


Important!!! The standard Godot Editor will not work!!! Use the Lua Api version in the links above.


For people having trouble with creating the local nuget package source, you can do this instead.

* Remove the source that you created before, and replace it with this.
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
* Replace /path/to/editor/editor-mono/nuget_packages with the correct path to your local NuGet source.
* Save the nuget.config file.

With this configuration, you don't need to manually specify the package source in your project file. The NuGet restore process will automatically consider the sources listed in the nuget.config.

After doing this, do a `dotnet restore '/pathtoproject/example_project.csproj' -f -s LuaAPINugetSource` to restore the nuget packages for the project.


With this configuration, you don't need to manually specify the package source in your project file. The NuGet restore process will automatically consider the sources listed in the nuget.config.

After doing this, do a dotnet restore '/pathtoproject/example_project.csproj' -f -s LuaAPINugetSource to restore the nuget packages for the project.

Note that the nuget.config file cannot have any whitespace on the first line, before the <?xml tag.

Additionally, Windows users may have to set things up while in Administrator mode. Occassionally, certain files / directories are not made by DotNet like it should. So, you may have to make those directories. (It'll list the directory in the error messages.)
While that is not a fault of the project, nor the instructions, it can occur. Also, you may need to copy files from C:\Users\USERNAME\.nuget\packages\godot.net.sdk\4.2.0\Sdk replacing USERNAME with your windows user name.
Additionally, some versions of Windows like to mark downloaded files as inaccessible. So you may have to right click those files and select "unblock" from the files' properties.

For additional help / to ask questions, please feel free to ask questions in the ⁉-support or development channel.

https://discord.gg/5ZnuVhAAqF

Contributing
-------
At the current moment, we happily accept contributions, and they are needed. We also need people to test the engine on various systems. 
Please use the Issue Tracker to report the outcomes, so that if needed, we can look into it.
Please see [CONTRIBUTING.md](CONTRIBUTING.md) for more information.

For those that wish to support this project, we have set up a Ko-Fi support option. [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z8NGVMV). A single donation will get you access to the Apophis Software Lounge discord server, and Monthly Donations will give you access to the "Supporter" role, and access to a special Supporter channel!

License
-------
Please see [LICENSE.md](LICENSE.md) for information on the License of this software.

Code Of Conduct
-------
Please see [CODEOFCONDUCT.md](CODEOFCONDUCT.md) for more information.
