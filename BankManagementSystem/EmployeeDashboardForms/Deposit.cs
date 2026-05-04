using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.S3;
using Amazon.S3.Model;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        // Blocker 14 Fix: Remove static state - use instance field instead
        private decimal lastDepositAmount;

        // Blocker 11, 13 Fix: Replace local file operations with Amazon S3
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;

        public Deposit()
        {
            InitializeComponent();

            // Blocker 11, 13 Fix: Initialize Amazon S3 client for file operations
            s3Client = new AmazonS3Client();
            bucketName = Environment.GetEnvironmentVariable("DEPOSIT_CACHE_BUCKET") ?? "bank-deposit-cache";
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out lastDepositAmount);

            // Blocker 9 Fix: Replace hard-coded Windows path with S3 key
            // Blocker 11, 13 Fix: Replace File.WriteAllText with S3 PutObject
            string cacheKey = "deposit-cache/last.txt";
            
            // Write to S3 instead of local file system
            WriteToS3Async(cacheKey, lastDepositAmount.ToString()).Wait();
        }

        // Blocker 11, 13 Fix: Write content to S3 object
        private async Task WriteToS3Async(string key, string content)
        {
            try
            {
                // Ensure bucket exists
                await EnsureBucketExistsAsync();

                // Write to S3
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = content,
                    ContentType = "text/plain"
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to S3: {ex.Message}");
                MessageBox.Show($"Failed to save deposit information: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ensure S3 bucket exists
        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                await s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                });
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
            {
                // Bucket already exists, that's fine
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                s3Client?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
