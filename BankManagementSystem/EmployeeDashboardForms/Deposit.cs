using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        // Replaced static field with instance field to avoid state inconsistency
        // in multi-instance cloud deployments.
        private decimal lastDepositAmount;

        public Deposit()
        {
            InitializeComponent();
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out lastDepositAmount);

            // Replaced File.WriteAllText with hard-coded Windows path (D:\DepositCache\last.txt)
            // with Azure Blob Storage upload using BlobClient.UploadAsync.
            // The storage account URL and container name are read from environment variables
            // for cloud-native, credential-free access via Workload Identity.
            string storageAccountUrl = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_URL")
                ?? throw new InvalidOperationException(
                    "Azure Storage account URL is not configured. " +
                    "Set the 'AZURE_STORAGE_ACCOUNT_URL' environment variable.");

            string containerName = Environment.GetEnvironmentVariable("DEPOSIT_CACHE_CONTAINER")
                ?? "deposit-cache";

            string blobName = "last.txt";

            var blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl),
                new Azure.Identity.DefaultAzureCredential());
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(lastDepositAmount.ToString()));
            _ = blobClient.UploadAsync(stream, overwrite: true);
        }
    }
}
