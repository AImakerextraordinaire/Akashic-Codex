using System;
using System.Runtime.InteropServices;
using System.Threading;
using AkashicCodex;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AkashicCodex
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodemuseAIPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class CodemuseAIPackage : AsyncPackage
    {
        public const string PackageGuidString = "dbdf122d-50f1-4905-8131-1bde2191b2b2";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                await AIAutomationCommand.InitializeAsync(this);
                await VSProjectManager.InitializeAsync(this);

                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                ActivityLog.LogInformation("AkadhicCodexPackage", "Initialization successful.");
            }
            catch (Exception ex)
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                ActivityLog.LogError("AkadhicCodexPackage", $"Initialization failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}

