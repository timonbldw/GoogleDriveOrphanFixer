using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.DriveActivity.v2;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using Google.Apis.Drive.v3;
using System.Runtime.InteropServices;


namespace GoogleDriveOrphanFixer
{
    class DriveItem
    {
        //public string curParent;
        public string id;
        public string title;
        public string parent_id;
        public string parent_title;
        public bool isFile;

        override public string ToString()
        {
            return String.Format("{0}|{1}|{2}|{3}|{4}", id, title, parent_id, parent_title, isFile);
            //return String.Format("{0}|{1}|{2}|{3}|{4}|{5}", id, title, parent_id, parent_title, curParent, isFile);
        }

        public static string Header()
        {
            return "id|title|parent_id|parent_title|isFile\r\n";
            //return "id|title|parent_id|parent_title|curParent|isFile\r\n";
        }

    }
    class DriveHelper
    {
        // Api Deklarieren
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);

        public int PageSize = 500;

        public string[] Scopes = { DriveActivityService.Scope.DriveActivityReadonly, DriveService.Scope.Drive };
        public string ApplicationName = "DriveOrphanFixer";
        public UserCredential Credential;

        public DriveHelper()
        {
            _init();
        }

        public DriveHelper(string[] scopes, string applicationName, int pageSize)
        {
            Scopes = scopes;
            ApplicationName = applicationName;
            PageSize = pageSize;
            _init();
        }

        private void _init()
        {
            try
            {
                using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    //Console.WriteLine("Credential file saved to: " + credPath);
                }
            } catch (FileNotFoundException e)
            {
                try
                {
                    ShellExecute(0, "open", "https://console.developers.google.com/henhouse/?pb=%5B%22hh-0%22,%22drive%22,null,%5B%5D,%22https:%2F%2Fdevelopers.google.com%22,null,%5B%5D,null,%22Enable%20the%20Drive%20API%22,1,null,%5B%5D,false,false,null,null,null,null,false,null,false,false,null,null,null,%22DESKTOP%22,null,%22Quickstart%22,true,%22Quickstart%22,null,null,false%5D", "", "", 5);
                } catch { } // if not supported on linux
                Console.WriteLine("ERROR: No credentials.json file found. {0}\n\n" +
                    "If the credentials.json file is missing or invalid you can create your own in the opened window. " +
                    "Simply click on the button in the bottom right of the opened browser window and download your credentials file.", e.Message);
                Console.ReadKey();
            }
        }
    }
}
