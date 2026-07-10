using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Blocker-9  (cr-dotnet-0001): Removed hard-coded Windows path D:\DepositCache\last.txt.
//   Replaced with Cloud Storage bucket URI configured via environment variables.
// Blocker-11 (cr-dotnet-0002): Replaced File.WriteAllText() local write with
//   StorageClient.UploadObjectAsync() for durable GCS persistence.
// Blocker-13 (cr-dotnet-0003): Replaced System.IO.File API with Google Cloud Storage
//   StorageClient methods for cloud-native object storage.

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        // Removed static LastDepositAmount field — stateless pattern; value is
        // persisted to Cloud Storage instead of in-memory static state.

        public Deposit()
        {
            InitializeComponent();
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal lastDepositAmount;
            decimal.TryParse(AmountTextBox.Text, out lastDepositAmount);

            // Blocker-9  (cr-dotnet-0001): Replace hard-coded Windows path D:\DepositCache\last.txt
            //   with Cloud Storage URI configured via environment variables.
            // Blocker-11 (cr-dotnet-0002): Replace File.WriteAllText() with
            //   StorageClient.UploadObjectAsync() for durable GCS persistence.
            // Blocker-13 (cr-dotnet-0003): Replace System.IO.File API with StorageClient.
            UploadDepositCacheAsync(lastDepositAmount.ToString()).GetAwaiter().GetResult();
        }

        // Blocker-9, Blocker-11, Blocker-13 (cr-dotnet-0001/0002/0003):
        // Upload deposit cache entry to Google Cloud Storage instead of writing to a
        // local Windows file path. Workload Identity Federation grants pod-level
        // access to Cloud Storage without JSON key files.
        private async Task UploadDepositCacheAsync(string depositValue)
        {
            string bucketName =
                Environment.GetEnvironmentVariable("GCS_DEPOSIT_CACHE_BUCKET")
                ?? throw new InvalidOperationException(
                    "GCS_DEPOSIT_CACHE_BUCKET environment variable is not set.");
            string objectName =
                Environment.GetEnvironmentVariable("GCS_DEPOSIT_CACHE_OBJECT")
                ?? "last.txt";

            StorageClient storageClient = await StorageClient.CreateAsync();
            byte[] data = Encoding.UTF8.GetBytes(depositValue);
            using (MemoryStream stream = new MemoryStream(data))
            {
                await storageClient.UploadObjectAsync(
                    bucketName,
                    objectName,
                    "text/plain",
                    stream);
            }
        }
    }
}
