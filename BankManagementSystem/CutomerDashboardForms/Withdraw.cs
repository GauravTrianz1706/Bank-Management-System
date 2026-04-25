using System.Diagnostics;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Withdraw : Form
    {
        public Withdraw()
        {
            InitializeComponent();

            
            var sessionId = Process.GetCurrentProcess().SessionId;
        }

        
        private struct WithdrawalToken
        {
            public int Code;
        }
    }
}
