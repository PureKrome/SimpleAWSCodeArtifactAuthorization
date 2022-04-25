using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static SimpleExec.Command;

namespace SimpleAWSCodeArtifactAuthorization
{
    class Program
    {
        public static void Main(string[] args)
        {
            var workingDirectory = args != null &&
                args.Any()
                    ? args[0]
                    : null;

            // Check if the nuget.config file exists.
            const string nugetConfigFileName = "nuget.config";

            var path = string.IsNullOrWhiteSpace(workingDirectory)
                ? Path.Combine(Environment.CurrentDirectory, nugetConfigFileName)
                : Path.Combine(workingDirectory, nugetConfigFileName);

            if (!File.Exists(path))
            {
                // No file, so lets create a new one.

                Console.WriteLine(" ** No nuget.config file found. Creating a new one");

                File.WriteAllText(path, "<configuration></configuration>");
            }

            try
            {
                Run("dotnet", 
                    "nuget remove source togorv/cosmos-nuget --configfile nuget.config", 
                    workingDirectory,
                    handleExitCode: _ => true ); // Always return true, regardless of errors.

                Console.WriteLine();

                AuthenticateWithAWSCodeArtifact(workingDirectory);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to run. Error: {exception.Message}");
            }
        }

        private static void AuthenticateWithAWSCodeArtifact(string workingDirectory = null)
        {
            var shellTextOutput = Read("aws", "codeartifact get-authorization-token --domain togorv --profile cosmos-nuget-read");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var token = JsonSerializer.Deserialize(
                shellTextOutput,
                typeof(Token),
                new SourceGenerationContext(options)) as Token;

            if (string.IsNullOrWhiteSpace(token.AuthorizationToken))
            {
                Console.WriteLine("*** Failed to authenticate with AWS CodeArtifact!!");
                return;
            }

            var argument = $"nuget add source https://togorv-499883392463.d.codeartifact.us-east-1.amazonaws.com/nuget/cosmos-nuget/v3/index.json --configfile nuget.config --name togorv/cosmos-nuget --username aws --store-password-in-clear-text --password {token.AuthorizationToken}";

            Run("dotnet", argument, workingDirectory);
        }
    }
}
