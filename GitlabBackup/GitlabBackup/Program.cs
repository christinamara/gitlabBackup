using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace GitlabBackup
{
    class Program
    {
        static void Main(string[] args)
        {

            // Declare&Initialize variables
            string html = string.Empty;                                                                   //Holds the api call output in json format
            string apiAddress =
                @"https://gitlab.com/api/v4/projects?private_token=ByfnGGX1jFXt3joJhJCW;membership=true"; //api address with my (user specific) private token that WILL EXPIRE EVENTUALLY
                                                                                                          //TODO: Different authentication method that does not expire for the api call
            string cloneAndPull = @"C:\Users\cmara\Documents\cloneAndPull.bat ";                          //Absolute path to the cloneAndPull batch file that clones the given directory and pulls down changes
            string localBackupDirectory = @"C:\Users\cmara\Documents\git\";                               //path to directory you want your git project folders created & updated in
            string search = @"(https://gitlab.com/cmara/[a-zA-Z0-9/]+.git)";                              //regex string to search for in the raw json output from the api call

            //Call the API and store results as raw json string
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiAddress);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            // Get the strings that I think are addresses my projects based on the regex
            MatchCollection projects = Regex.Matches(html, search);

            foreach (Match project in projects)
            {
                // Find where the project name starts and stops in the project address,
                // because this is what the cloned copy will automatically be named
                //TODO: a more dynamic way to do this...
                int startIndex = project.Value.LastIndexOf('/') + 1;
                int endIndex = project.Value.LastIndexOf('.');

                //Execute the cloneAndPull batch file with parameter 1: the gitlab path to clone and pull from 2: the local path to the cloned project and parameter
                ExecuteCommand(cloneAndPull + project.Value + " " + localBackupDirectory + project.Value.Substring(startIndex, endIndex - startIndex));
            }


            // Displays the full json response
            // Console.WriteLine(html);

            // I want to console to stay open after execution so I can see output
            Console.ReadKey();
        }

        /** Executes a command in a seperate cmd prompt.
         *  More info: https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp
         *  @author steinar https://stackoverflow.com/users/282024/steinar
         */
        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
    }
}
