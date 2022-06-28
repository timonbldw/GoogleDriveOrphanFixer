using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;

namespace GoogleDriveOrphanFixer
{
    class DriveFileHelper
    {
        DriveHelper drive;
        DriveService service;
        private string rootFolder;

        public DriveFileHelper(DriveHelper drive)
        {
            this.drive = drive;
            // Create Google Drive Activity API service.
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = drive.Credential,
                ApplicationName = drive.ApplicationName,
            });

            rootFolder = service.Files.Get("root").Execute().Id;
        }

        public List<string> ScanRootDirectory()
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = drive.PageSize;
            listRequest.Fields = "nextPageToken, files(id, name, parents)";
            // only look for files/folders in the root folder as this is where orphans are being moved
            listRequest.Q = $"'{rootFolder}' in parents";
            FileList response;

            List<string> possibleOrphans = new List<string>();
            int i = 1;
            do
            {
                Console.Write("\rScanning root directory... Page {0}", i);
                response = listRequest.Execute();
                IList<Google.Apis.Drive.v3.Data.File> files = response.Files;
                foreach (var file in files)
                {
                    if (file.Parents.Contains(rootFolder))
                    {
                        possibleOrphans.Add("items/" + file.Id);
                    }
                }
                // check next page
                listRequest.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null && response.NextPageToken != "");
            Console.WriteLine();
            return possibleOrphans;
        }

        public void MoveOrphanFilesToParent(string globalRestoredFolderId, List<DriveItem> orphansToMove)
        {

            // if the user did not move the folders elsewhere, the orphans were moved to the root folder,
            // we therefore need to remove them from there...
            if (globalRestoredFolderId == null)
            {
                //var test = service.Files.Get("root").Execute();
                globalRestoredFolderId = rootFolder;
            }

            for (int i = 0; i < orphansToMove.Count; i++)
            {
                Console.Write("\rMoving orphan back into parent folder... {0}/{1}", i + 1, orphansToMove.Count);
                FilesResource.UpdateRequest updateRequest = service.Files.Update(new Google.Apis.Drive.v3.Data.File(), orphansToMove[i].id.Replace("items/", ""));
                updateRequest.AddParents = orphansToMove[i].parent_id.Replace("items/", "");
                updateRequest.RemoveParents = globalRestoredFolderId;

                var response = updateRequest.Execute();
            }
            Console.WriteLine();
        }

        public List<string> GetFilesFromFolderById(string parent_id)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = String.Format("\"{0}\" in parents", parent_id.Replace("items/", ""));
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.PageSize = drive.PageSize;


            FileList response;

            List<string> fileIds = new List<string>();

            int i = 1;
            do
            {
                Console.Write("\rFetching files from folder... Page {0}", i);
                try
                {
                    response = listRequest.Execute();
                    IList<Google.Apis.Drive.v3.Data.File> files = response.Files;

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            //Console.WriteLine("{0} ({1})", file.Name, file.Id);
                            fileIds.Add("items/" + file.Id);
                        }
                    }
                    else
                    {
                        break;
                    }

                    listRequest.PageToken = response.NextPageToken;
                }
                catch (Google.GoogleApiException e)
                {
                    Console.WriteLine("\nERROR: Could not find folder id! " + e.Message);
                    return fileIds;
                }
            } while (response.NextPageToken != null && response.NextPageToken != "");
            Console.WriteLine();
            return fileIds;
        }
    }
}