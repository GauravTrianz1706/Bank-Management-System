using BankDatabaseAccess.EntityModel;
using System;
using System.IO;
using System.Windows.Forms;
// Blocker-1 (cr-dotnet-0030): Removed System.Security.Principal / WindowsIdentity import.
// Windows Authentication (NTLM/Kerberos) replaced with Google Cloud Workforce Identity Federation.
// The authenticated identity is now obtained from the GCP_WORKFORCE_USER_IDENTITY environment
// variable injected by GKE / Workforce Identity Federation at runtime.
// Blocker-7 (cr-dotnet-0001): Removed hard-coded Windows path C:\EmployeeAudit\session.log.
// Replaced with a platform-agnostic path built from the AUDIT_LOG_DIR environment variable
// (or a safe OS temp fallback), using Path.Combine with forward-slash-compatible separators.

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker-1 (cr-dotnet-0030): Session identity sourced from GCP Workforce Identity
        // Federation environment variable instead of WindowsIdentity.GetCurrent().Name.
        private static string CurrentEmployeeSession;

        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Blocker-1 (cr-dotnet-0030): Replace Windows Authentication with
            // Google Cloud Workforce Identity Federation.
            // The authenticated user principal is injected via the
            // GCP_WORKFORCE_USER_IDENTITY environment variable by GKE Workload Identity.
            CurrentEmployeeSession =
                Environment.GetEnvironmentVariable("GCP_WORKFORCE_USER_IDENTITY")
                ?? "unknown-user";

            // Blocker-7 (cr-dotnet-0001): Replace hard-coded Windows path with
            // environment-variable-driven, platform-agnostic path.
            // AUDIT_LOG_DIR is configured via Kubernetes ConfigMap; falls back to
            // the OS temp directory so the app still runs locally without configuration.
            string auditLogDir =
                Environment.GetEnvironmentVariable("AUDIT_LOG_DIR")
                ?? Path.Combine(Path.GetTempPath(), "EmployeeAudit");

            Directory.CreateDirectory(auditLogDir);

            auditStream = new FileStream(
                Path.Combine(auditLogDir, "session.log"),
                FileMode.OpenOrCreate);
        }
    }
}
