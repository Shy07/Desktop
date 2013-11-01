using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpaqueTaskbar;

namespace Desktop
{
    public partial class Form1 : Form
    {
        [DllImport("shell32.dll")]
        static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);
        [DllImport("shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);
        [DllImport("shell32.dll")]
        static extern int SHUpdateRecycleBinIcon();

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct SHQUERYRBINFO
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(4)]
            public long i64Size;
            [FieldOffset(12)]
            public long i64NumItems;
        }
        //     No dialog box confirming the deletion of the objects will be displayed.
        const int SHERB_NOCONFIRMATION = 0x00000001;
        //     No dialog box indicating the progress will be displayed. 
        const int SHERB_NOPROGRESSUI = 0x00000002;
        //     No sound will be played when the operation is complete. 
        const int SHERB_NOSOUND = 0x00000004;

        bool taskbarTransparency;

        public Form1()
        {
            InitializeComponent();
            API.FindTaskbars();
            API.DisableTaskbarTransparency();
            taskbarTransparency = true;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                Process.Start("C:\\Users\\" + Environment.UserName + "\\Links\\Desktop.lnk");
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Users\\" + Environment.UserName + "\\Links\\Desktop.lnk");
        }

        private void emptyRecycleBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SHQUERYRBINFO sqrbi = new SHQUERYRBINFO();
            sqrbi.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
            if (SHQueryRecycleBin(null, ref sqrbi) == 0)
            {
                if (sqrbi.i64NumItems <= 0)
                {
                    MessageBox.Show("The operation failed. Recycle bin is empty.");
                    return;
                }
            }
            int rt = SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOPROGRESSUI);
            if (rt != 0)
                MessageBox.Show("The operation failed. Error code: " + rt.ToString() + ".");
        }

        private void disbaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (taskbarTransparency)
            {
                API.FindTaskbars();
                API.EnableTaskbarTransparency();
                disbaleToolStripMenuItem.Text = "DisableTaskbarTransparency";
                taskbarTransparency = false;
            }
            else
            {
                API.FindTaskbars();
                API.DisableTaskbarTransparency();
                disbaleToolStripMenuItem.Text = "EnableTaskbarTransparency";
                taskbarTransparency = true;
            }
        }
    }
}
