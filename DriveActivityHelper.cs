using Google.Apis.DriveActivity.v2;
using Google.Apis.DriveActivity.v2.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleDriveOrphanFixer
{
    class DriveActivityHelper
    {
        DriveHelper drive;
        DriveActivityService service;

        public DriveActivityHelper(DriveHelper drive)
        {
            this.drive = drive;
            // Create Google Drive Activity API service.
            service = new DriveActivityService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = drive.Credential,
                ApplicationName = drive.ApplicationName,
            });
        }

        public List<DriveItem> FindOrphanParent(List<string> orphans)
        {
            List<DriveItem> reparentedItems = new List<DriveItem>(orphans.Count);
            for (int i = 0; i < orphans.Count; i++)
            {
                Console.Write("\rFinding parents of orphan {0} of {1}... ", i, orphans.Count);
                try
                {
                    reparentedItems.Add(FindOrphanParent(orphans[i]));
                }
                catch (Google.GoogleApiException e)
                {
                    Console.WriteLine("ERROR: Could not retrieve activity data for orphan! " + e.Message);
                }
                catch (FileNotFoundException)
                {
                    Console.Write("No activity found!");
                }
                catch (InvalidOperationException)
                {
                    Console.Write("Not an orphan.");
                }
            }
            Console.WriteLine();
            return reparentedItems;
        }

        public DriveItem FindOrphanParent(string orphan_id)
        {
            QueryDriveActivityResponse response;
            QueryDriveActivityRequest requestData = new QueryDriveActivityRequest();

            requestData.ItemName = orphan_id;
            requestData.PageSize = drive.PageSize;

            do
            {
                ActivityResource.QueryRequest queryRequest = service.Activity.Query(requestData);
                response = queryRequest.Execute();
                IList<Google.Apis.DriveActivity.v2.Data.DriveActivity> activities = response.Activities;

                if (activities != null && activities.Count > 0)
                {
                    foreach (var activity in activities)
                    {
                        var move = activity.PrimaryActionDetail.Move;
                        // check that the item was moved from a standard folder to the MY_DRIVE_ROOT
                        if (move == null || move.RemovedParents == null || move.AddedParents == null) continue;

                        if (move.RemovedParents.Count > 1 || move.AddedParents.Count > 1)
                        {
                            Console.WriteLine("WARNING: Item has more than one removed or added parent. Only the first entries are used. This is untested.");
                        }

                        var removed = move.RemovedParents[0];
                        var added = move.AddedParents[0];
                        if (removed.DriveItem.DriveFolder == null || added.DriveItem.DriveFolder == null) continue;
                        // Item must be moved from STANDARD to ROOT folder NOT by the owner (i.e. current user)
                        if (removed.DriveItem.DriveFolder.Type == "STANDARD_FOLDER" &&
                            added.DriveItem.DriveFolder.Type == "MY_DRIVE_ROOT" &&
                            activity.Actors.Any(c => c.User.KnownUser.IsCurrentUser == null)
                            )
                        {
                            DriveItem item = new DriveItem
                            {
                                id = activity.Targets[0].DriveItem.Name,
                                title = activity.Targets[0].DriveItem.Title,
                                parent_id = activity.PrimaryActionDetail.Move.RemovedParents[0].DriveItem.Name,
                                parent_title = activity.PrimaryActionDetail.Move.RemovedParents[0].DriveItem.Title,
                                isFile = activity.Targets[0].DriveItem.DriveFile != null
                            };
                            return item;
                        }
                        else
                        {
                            // the folder was not moved from STANDARD -> ROOT or by owner himself
                            throw new InvalidOperationException("Not an orphan.");
                        }
                    }
                }
                else
                {
                    break;
                }

                // set next request to get next page
                requestData.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null && response.NextPageToken != "");
            throw new FileNotFoundException("No Activity found!");
        }
        public void GetSingleActivity(string item, bool getAllInfoInsideFolder = false)
        {
            QueryDriveActivityResponse response;
            QueryDriveActivityRequest requestData = new QueryDriveActivityRequest();
            if (getAllInfoInsideFolder)
            {
                requestData.AncestorName = item; // gets all activity inside the folder
            }
            else
            {
                requestData.ItemName = item; // gets all activity from the item
            }

            requestData.PageSize = drive.PageSize;
            ActivityResource.QueryRequest queryRequest = service.Activity.Query(requestData);
            response = queryRequest.Execute();

            // List activities.
            IList<Google.Apis.DriveActivity.v2.Data.DriveActivity> activities = response.Activities;
        }

    }
}