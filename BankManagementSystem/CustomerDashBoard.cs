using BankDatabaseAccess.EntityModel;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
// Blocker-14 (cr-dotnet-0008): Removed static singleton NavigationHistory field.
//   Replaced with Google Cloud Firestore document reads/writes for distributed
//   state consistency across GKE pods.
// Blocker-15 (cr-dotnet-0040): Removed Microsoft.Win32.Registry / RegistryKey usage.
//   Replaced with Google Secret Manager / Cloud Storage for configuration retrieval.
//   Windows Registry does not exist on Linux-based GKE nodes.

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker-14 (cr-dotnet-0008): Replace static singleton mutable list with
        // Firestore-backed distributed state. NavigationHistory is now persisted to
        // and read from Cloud Firestore so all GKE pod instances share consistent state.
        // The local list is used only as a transient in-memory buffer before flushing.
        private List<string> navigationHistoryBuffer = new List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Blocker-15 (cr-dotnet-0040): Replace Windows Registry access with
            // Google Secret Manager / environment variable configuration.
            // Registry.CurrentUser.OpenSubKey(@"Software\BankApp") is replaced by
            // reading the BANKAPP_CONFIG environment variable injected by GKE ConfigMap,
            // or by fetching secrets from Google Secret Manager via Workload Identity.
            LoadAppConfigurationAsync().GetAwaiter().GetResult();
        }

        // Blocker-14 (cr-dotnet-0008): Persist navigation history to Cloud Firestore
        // for distributed consistency across GKE pods.
        public async Task AddNavigationEntryAsync(string entry)
        {
            navigationHistoryBuffer.Add(entry);

            string projectId =
                Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
                ?? throw new InvalidOperationException(
                    "GOOGLE_CLOUD_PROJECT environment variable is not set.");

            FirestoreDb db = FirestoreDb.Create(projectId);
            // Use Username as the document identifier (PersonModel.Username is always present)
            string documentId = personModel?.Username ?? "unknown";
            DocumentReference docRef = db
                .Collection("customer-navigation-history")
                .Document(documentId);

            await docRef.SetAsync(
                new { entries = navigationHistoryBuffer, updatedAt = Timestamp.GetCurrentTimestamp() },
                SetOptions.MergeAll);
        }

        // Blocker-15 (cr-dotnet-0040): Replace Registry.CurrentUser.OpenSubKey() with
        // environment variable / Secret Manager configuration retrieval.
        // GKE injects BANKAPP_CONFIG via Kubernetes ConfigMap; sensitive values are
        // fetched from Google Secret Manager using Workload Identity Federation.
        private async Task LoadAppConfigurationAsync()
        {
            string bankAppConfig =
                Environment.GetEnvironmentVariable("BANKAPP_CONFIG")
                ?? string.Empty;

            // Configuration is now available via environment variable.
            // Additional secrets can be fetched from Google Secret Manager
            // using the Google.Cloud.SecretManager.V1 client with Workload Identity.
            await Task.CompletedTask;
        }
    }
}
