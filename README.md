# injec-env

Console program for Windows to edit user environnement variables temporarily (in `HKCU`, so no admins rights needed). For when you want to have portable softwares on an USB stick that instantly work using the CLI, for example, or set that damn `JAVA_HOME` using a jdk on an USB stick.


## How to use

First, make sure the files are **NOT** in an admin rights protected folder.

To use it, create a config file called `rules.json` next to the `Injec Env.exe`. If you run the .exe without it, it will create an example for you.

Once done, just start the .exe and it will do its magic. To stop, write `exit` or Ctrl+C. **DO NOT CLOSE THE WINDOW OR KILL THE PROCESS** ; doing so will likely not give enough time to revert the changes.

It is possible to :
- Set a new variable, which is deleted once the program stops.
- Set an existing variable, which is reverted once the program stops.
- Add values to the `Path` variable.
- Does not revert values if the said values has changed while the program was active.

## Dynamic values

Values will be written differently :

- If you want to set a constant value that is **not** a path, prefix it with `:` (example : `":some string"` will be saved as `some string`)
- You can set a relative path, it will be saved as an absolute path depending on the current working directory.
- You can set a kinda relative path : If you give an absolute path, but with no letter (`/absolute/path/to/folder`), the engine will use the disk letter of the current working directory (ex: `E:/absolute/path/to/folder`).
- You can, obviously, set absolute paths (ex:`C:/path/to/stuff`).

Paths will be normalized by replacing `/` to `\\`.

# Last warning ⚠

The program runs on [NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0), so you WILL require, at least, the NET 6.0 runtime (Desktop not needed).

[A version of NET 6.0 runtime compatible](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.2-windows-x64-binaries).

To start the program, you will have to execute the `.dll` using the `dotnet` command. For example :
```cmd
cd "path/to/Injec Env"
path/to/NET6/dotnet.exe "Injec Env.dll"
```
