using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CodemuseAI
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodemuseAIPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class CodemuseAIPackage : AsyncPackage
    {
        public const string PackageGuidString = "dbdf122d-50f1-4905-8131-1bde2191b2b2";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Switch to the main thread for VS Shell operations.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // ✅ Initialize All Necessary Commands
            await AIAutomationCommand.InitializeAsync(this);
            await VSProjectManager.InitializeAsync(this); // <-- Missing before
        }
    }
}
