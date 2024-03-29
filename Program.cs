﻿using System;
using System.Collections.Generic;

namespace GoogleDriveOrphanFixer
{
    class Program
    {
        private static string restoreFolderId;
        private enum Selection
        {
            RestoreFolder,
            Orphan,
            Exit
        }

        static void Main(string[] args)
        {
            Selection selection = Startup();
            Console.WriteLine("\nInitializing...");

            DriveHelper drive = new DriveHelper();
            DriveFileHelper fileHelper = new DriveFileHelper(drive);
            DriveActivityHelper activityHelper = new DriveActivityHelper(drive);

            List<DriveItem> filesToReparent = new List<DriveItem>();

            do
            {
                List<string> fileIDs = new List<string>();
                switch (selection)
                {
                    case Selection.RestoreFolder:
                        fileIDs = fileHelper.GetFilesFromFolderById(restoreFolderId); // finds all files that are in the given folder
                        break;
                    case Selection.Orphan:
                        fileIDs = fileHelper.ScanRootDirectory(); // finds all orphans
                        break;
                }

                try
                {
                    filesToReparent = activityHelper.FindOrphanParent(fileIDs);
                    // backup
                    //System.IO.File.WriteAllText("output.csv", DriveItem.Header() + string.Join("\r\n", (object[])filesToReparent.ToArray()));

                    fileHelper.MoveOrphanFilesToParent(restoreFolderId, filesToReparent);
                }
                catch (Exception e) { Console.WriteLine("Error: {0}", e.Message); }
            } while (filesToReparent.Count > 0);

            Console.Write("All tasks done. Press any key to exit.");
            Console.ReadKey();
        }

        private static Selection Startup()
        {
            try
            {
                Console.SetWindowSize(120, 30);
            }
            catch { } // if not supported on linux
            Console.WriteLine("========================================================================================================================");
            Console.WriteLine(_Center("Welcome to the Google Drive Orphan Fixer 2.0!"));
            Console.WriteLine(_Center("This tool will try restore the original folder structure of shared files that were removed by others and now appear in the root folder."));
            Console.WriteLine("\n");
            Console.WriteLine(_Center("Please make sure you have your credentials.json file saved in the same folder as this executable file!"));
            Console.WriteLine("\n");
            Console.WriteLine(_Center("Icon modified from Freepik from www.flaticon.com"));
            Console.WriteLine("========================================================================================================================\n");

            Console.WriteLine("Please select:\n" +
                "1. There are files in my root folder that were shared and are supposed to be somewhere else\n" +
                "2. Exit\n");

            ConsoleKeyInfo key;
            do
            {
                _ClearLine();
                Console.Write("Select: ");
                key = Console.ReadKey();
            } while (key.KeyChar < '1' || key.KeyChar > '2');

            switch (key.KeyChar)
            {
                //case '1':
                //    Console.WriteLine("\n\nIn order to identify your files, the id of the folder where you moved the files is required.\n" +
                //        "Hint: You can obtain the folder id by opening the folder in your browser and looking at the link.\n" +
                //        "Your link will have the following format: https://drive.google.com/drive/folders/{folder-id}\n\n");
                //        Console.Write("Please enter your folder id: ");

                //    restoreFolderId = Console.ReadLine();
                //    return Selection.RestoreFolder;
                case '1':
                    return Selection.Orphan;
                default:
                    Environment.Exit(0);
                    return Selection.Exit;
            }
        }

        static int _GetCenterPos(string text)
        {
            return (Console.WindowWidth / 2) - (text.Length / 2);
        }
        static string _Center(string text)
        {
            return _GetSpace(_GetCenterPos(text)) + text;
        }

        static string _GetSpace(int amount)
        {
            string ret = "";
            for (int i = 0; i < amount; i++)
            {
                ret += " ";
            }
            return ret;
        }

        static void _ClearLine()
        {
            Console.Write("\r{0}\r", _GetSpace(Console.WindowWidth));
        }
    }
}
