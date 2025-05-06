using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodemuseAI
{
    internal sealed class AIAutomationCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("dbdf122d-50f1-4905-8131-1bde2191b2b2");
        private readonly AsyncPackage package;

        private static readonly string[] CommandsRequiringInput =
        {
            "open_project", "create_project", "build_project",
            "run_project", "commit_project", "fetch_logs"
        };

        private AIAutomationCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            new AIAutomationCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => await ExecuteAsync(sender, e));
        }

        private async Task ExecuteAsync(object sender, EventArgs e)
        {
            try
            {
                string commandName = (sender as OleMenuCommand)?.Text;
                if (string.IsNullOrEmpty(commandName))
                {
                    ShowMessage("Invalid Command", "No command name detected.");
                    return;
                }

                string apiEndpoint = MapCommandToEndpoint(commandName);
                if (apiEndpoint == null)
                {
                    ShowMessage("Unknown Command", $"No API endpoint mapped for '{commandName}'.");
                    return;
                }

                string requestBody = null;
                if (Array.Exists(CommandsRequiringInput, cmd => cmd == apiEndpoint))
                {
                    string userInput = PromptForInput($"Enter project name for {commandName}:");
                    if (string.IsNullOrWhiteSpace(userInput))
                    {
                        ShowMessage("Operation Cancelled", "No project name provided.");
                        return;
                    }

                    // Different commands may require additional input
                    if (apiEndpoint == "commit_project")
                    {
                        string commitMessage = PromptForInput("Enter commit message:");
                        if (string.IsNullOrWhiteSpace(commitMessage))
                        {
                            ShowMessage("Operation Cancelled", "No commit message provided.");
                            return;
                        }
                        requestBody = JsonConvert.SerializeObject(new { project_name = userInput.Trim(), commit_message = commitMessage.Trim() });
                    }
                    else
                    {
                        requestBody = JsonConvert.SerializeObject(new { project_name = userInput.Trim() });
                    }
                }

                string responseText = await VSProjectManager.SendApiRequestAsync(apiEndpoint, requestBody);
                ShowMessage(commandName, responseText);
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Error executing command: {ex.Message}");
            }
        }

        private string MapCommandToEndpoint(string commandName)
        {
            return commandName switch
            {
                "Launch Visual Studio" => "launch_vs",
                "Open Project" => "open_project",
                "Create Project" => "create_project",
                "List Projects" => "list_projects",
                "Build Project" => "build_project",
                "Run Project" => "run_project",
                "Commit Project" => "commit_project",
                "Fetch Logs" => "fetch_logs",
                _ => null
            };
        }

        private string PromptForInput(string message)
        {
            InputDialog dialog = new InputDialog(message);
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.UserInput : null;
        }

        private void ShowMessage(string title, string message)
        {
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
