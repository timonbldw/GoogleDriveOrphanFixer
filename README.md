## Google Drive Orphan Fixer
Tool for locating orphaned files and folders in Google Drive and restoring their original folder structure.

## Purpose
When sharing files using Google Drive it may happen that a person that has editor permissions deletes a file or folder owned by someone else. *In this case Google Drive will delete the file for the shared person but it won't delete the file for the owner*. Instead, Google Drive marks the file (or folder) as **"unorganized"** - thus the file still exists but lost its parent folder. The file will not show up in Google Drive anymore but still takes up space. If files have been deleted separately from their folder it is almost impossible to restore large numbers of files manually. This is particularly the case when using a Google Drive client, such as the official Backup & Sync. When a person synchronizes a shared folder and later on deletes it directly from the hard drive, the client will not only mark a folder as *unorganized*, but rather each file turns into an orphan.
### Update June 2022
I noticed that instead of marking the files/folders as *unorganized*, deleted files and folder would show up in the owner's root directory instead. Version v2.0 is a somewhat quick and dirty fix for this.

### Am I affected?
#### As of 2022 (?)
If you have previously shared files or folders that were deleted by an editor and are now appearing in your root directory, you can use version 2.0 of this tool to recover the original folder structure.
#### Pre 2022 (?)
There is an easy and quick way to tell: simply open Google Drive and search for *"is:unorganized owner:me"*. Or use the following link: https://drive.google.com/drive/search?q=is:unorganized%20owner:me.
If any items show up, you either may want to delete them in case you don't need them anymore (since they are *not* in the trash, they still count towards your total used space), or you may wish to restore them to their original path - which is the purpose of this tool.
Use version v1.0 of the tool if this is the case and you want to recover the original folder structure.

## Usage
On the first start of the tool, your browser will open requiring your to login and allow the tool to access your Google Drive data as well its activity. The activity is required to determine from where a folder was (re-)moved from.

### Version v2.0
Currently this version of the tool only supports the case where shared files were deleted by an editor that is non-owner of the files resulting
in those files to show up in the root directory of the owner (that shared the files in the first place). Any file or folder that now appears
in the root folder can be moved back to its original location using this tool. The tool currently does not support the case when the files/folders
were already moved again by the owner after being "deleted" by the non-owner.

### Version v1.0
**Requirements**
- The original version v1.0 requires .NET Core 3.1 Runtime: [download](https://dotnet.microsoft.com/download/dotnet?utm_source=getdotnetcore)
  - Please note that if you don't have the .NET Core Runtime installed, the program will exit immediately without giving an error (unless started manually from the command line)
- I compiled a second (untested) version based on .NET Framework that should work without the need of any additional runtimes

Use this version if files appear as *"is:unorganized owner:me"*.
Two modi are supported:
1. Restore folder structure from files already moved from the *nirvana* into a single, large folder
	* This is the case when you have already manually moved some, or all files into another folder in your Drive
	* See the next paragraph for further information
2. Restore the folder structure directly
	* This is the most common case, where the tool attempts to restore all files directly

You can run both modi if you have already started to move some files.

#### Restoring from a folder (Version v1.0)
To restore files from a specific folder to their original folder structure the tool will require you to enter the id of this specific folder. To obtain the id you can simply login into your Google Drive via browser, open the folder and copy the id found in the URL, as in:
>`https://drive.google.com/drive/folders/{folder-id}`

E.g., suppose the link to the folder is `https://drive.google.com/drive/folders/13BivEqW7pKwdGA1zgrolXQjv--2FG6Ms`, the id of the folder is therefore `13BivEqW7pKwdGA1zgrolXQjv--2FG6Ms`

## Configuring the credentials.json
If the API did not yet get approved and the maximum amount of users is reached the credentials.json file will stop working. To fix this you can either set up your own OAuth login at the [Google Cloud Platform](https://console.cloud.google.com/apis/) and edit the credentials.json file or use the fast way and get your credentials by using the [Quickstart API](https://console.developers.google.com/henhouse/?pb=%5B%22hh-0%22,%22drive%22,null,%5B%5D,%22https:%2F%2Fdevelopers.google.com%22,null,%5B%5D,null,%22Enable%20the%20Drive%20API%22,1,null,%5B%5D,false,false,null,null,null,null,false,null,false,false,null,null,null,%22DESKTOP%22,null,%22Quickstart%22,true,%22Quickstart%22,null,null,false%5D) - simply click on create in the bottom right. The credentials.json file must be placed in the same folder as the executable.

# Download
You can find compiled binaries [here](https://github.com/timonbldw/GoogleDriveOrphanFixer/releases) or build from source.

Make sure to install the .NET Core 3.1 Runtime in case you use .NET Core version of v1.0!

### Compiling
Verison v2.0:
I switched to .NET Framework 4.6.1 since it should be preinstalled on windows machines.

Version v1.0:
The code is written in C# with .NET Core 3.1. You can use Visual Studio for compiling. Building for Linux is untested and probably needs code adjustments to work.
