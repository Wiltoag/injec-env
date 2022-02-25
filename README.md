# injec-env

Console program for Windows to edit user environnement variables temporarily (in `HKCU`, so no admins rights needed).


## How to use

First, make sure the files are **NOT** in an admin rights protected folder.

To use it, create a config file called `rules.json` next to the `Injec Env.exe`. If you run the .exe without it, it will create an example for you.

Once done, just start the .exe and it will do its magic. To stop, write `exit` or Ctrl+C. **DO NOT CLOSE THE WINDOW OR KILL THE PROCESS** ; doing so will likely not give enough time to revert the changes.

## Dynamic values

Values will be written differently :

- If you want to set a constant value that is **not** a path, prefix it with `:` (example : `":some string"` will be saved as `some string`)
- You can set a relative path, it will be saved as an absolute path depending on the current working directory.
- You can set a kinda relative path : If you give an absolute path, but with no letter (`/absolute/path/to/folder`), the engine will use the disk letter of the current working directory (ex: `E:/absolute/path/to/folder`).
- You can, obviously, set absolute paths (ex:`C:/path/to/stuff`).

Paths will be normalized by replacing `/` to `\\`.
