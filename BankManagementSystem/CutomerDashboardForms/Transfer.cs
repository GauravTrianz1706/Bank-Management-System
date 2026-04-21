using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        private readonly PersonModel sender;
        private readonly PersonModel receiver = new CustomerModel();

        // MSMQ dependency (cloud)
        private readonly MessageQueue auditQueue =
            new MessageQueue(@".\Private$\transfer");

        public Tansfer(PersonModel customer)
        {
            sender = customer;
            InitializeComponent();
        }

        private void TransferBtn_Click(object senderObj, EventArgs e)
        {
            auditQueue.Send("transfer");

            // Hard-coded path (cloud)
            File.WriteAllText(@"C:\BankState\transfer.txt", DateTime.Now.ToString());
        }
    }
}
