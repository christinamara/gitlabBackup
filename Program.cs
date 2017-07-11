using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace WebAPIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string html = string.Empty;
            string url = @"https://gitlab.com/api/v4/projects?private_token=ByfnGGX1jFXt3joJhJCW;membership=true";
            string cloneAndPull = @"C:\Users\cmara\Documents\cloneAndPull.bat ";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            string value = @"(https://gitlab.com/cmara/[a-zA-Z0-9/]+.git)";
            MatchCollection projects = Regex.Matches(html, @value);

            string git = "\"" + @"C:\Program Files (x86)\Git\bin\git.exe" + "\"";

            foreach (Match project in projects)
            {
                Console.WriteLine(project.Value);
                int startIndex = project.Value.LastIndexOf('/') + 1;
                int endIndex = project.Value.LastIndexOf('.');
                ExecuteCommand(cloneAndPull + project.Value + " " + "C:\\Users\\cmara\\Documents\\git\\" + project.Value.Substring(startIndex, endIndex - startIndex));
            }

            
            // Displays the json response
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
