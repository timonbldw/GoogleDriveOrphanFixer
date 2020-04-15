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

        public DriveFileHelper(DriveHelper drive)
        {
            this.drive = drive;
            // Create Google Drive Activity API service.
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = drive.Credential,
                ApplicationName = drive.ApplicationName,
            });
        }

        public List<string> ScanForOrphans()
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = drive.PageSize;
            listRequest.Fields = "nextPageToken, files(id, name, parents)";
            FileList response;

            List<string> orphans = new List<string>();
            int i = 1;
            do
            {
                Console.Write("\rScanning for orphans... Page {0}", i);
                response = listRequest.Execute();
                IList<Google.Apis.Drive.v3.Data.File> files = response.Files;

                foreach (var file in files)
                {
                    try
                    {
                        if (file.Parents == null)
                        {
                            orphans.Add("items/" + file.Id);
                        }
                    }
                    catch { Console.WriteLine("Error reading file id from orphan"); }
                }
                // check next page
                listRequest.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null && response.NextPageToken != "");
            Console.WriteLine();
            return orphans;
        }

        public void MoveOrphanFilesToParent(string globalRestoredFolderId, List<DriveItem> orphansToMove)
        {
            for (int i = 0; i < orphansToMove.Count; i++)
            {
                Console.Write("\rMoving orphan back into parent folder... {0}/{1}", i+1, orphansToMove.Count);
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
                } catch (Google.GoogleApiException e)
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