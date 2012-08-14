using System;
using System.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace TalesOfVesperiaUtils.Formats.Packages
{
    public class MainForm : Form
    {
        // Fields
        private ToolStripMenuItem aboutToolStripMenuItem;
        private string[] args;
        public static uint BLOCK_SIZE = 0x800;
        private const int CB_CDROMSECTOR = 0x800;
        private const int CB_GAPMAXLBA = 0x7a120;
        private const int CB_GROUPSECTOR = 0x400;
        private const int CB_LAYER1LBA = 0x1b3880;
        private const int CB_XDVDFSLBA = 0x1fb20;
        private ColumnHeader columnHeaderAttrb;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnHeaderSector;
        private ColumnHeader columnHeaderSize;
        private ColumnHeader columnHeaderStatus;
        private IContainer components;
        private ContextMenuStrip contextMenuStrip;
        private ContextMenuStrip contextMenuStripMulti;
        public const uint CREATE_ALWAYS = 2;
        public const uint CREATE_NEW = 1;
        private string dirprefix;
        private ToolStrip driveToolStrip;
        private ToolStripComboBox driveToolStripComboBox;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem extractFilesToolStripMenuItem;
        private ToolStripMenuItem extractFileToolStripMenuItem;
        private ToolStripMenuItem extractToolStripMenuItem;
        private const uint FILE_ANY_ACCESS = 0;
        private const uint FILE_DEVICE_UNKNOWN = 0x22;
        public const uint FILE_SHARE_DELETE = 4;
        public const uint FILE_SHARE_READ = 1;
        public const uint FILE_SHARE_WRITE = 2;
        private ToolStripMenuItem fileToolStripMenuItem;
        private FolderBrowserDialog folderBrowserDialog;
        private FileStream fs;
        public const uint GENERIC_ALL = 0x10000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        private uint global_offset;
        private ImageList imageList;
        private ImageList imageListTreeView;
        public const uint INVALID_HANDLE_VALUE = uint.MaxValue;
        public const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        public const uint IOCTL_STORAGE_LOAD_MEDIA = 0x2d480c;
        public const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x2d4804;
        private string isofile;
        private ListView listView;
        private uint magic_number;
        private MenuStrip menuStrip;
        private const uint METHOD_BUFFERED = 0;
        private const uint METHOD_IN_DIRECT = 1;
        private const uint METHOD_NEITHER = 3;
        private const uint METHOD_OUT_DIRECT = 2;
        public const uint OPEN_ALWAYS = 4;
        public const uint OPEN_EXISTING = 3;
        private ToolStripButton openDvdToolStripButton;
        private OpenFileDialog openFileDialog;
        private OpenFileDialog openFileDialogReplace;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem questionToolStripMenuItem;
        private ToolStripMenuItem replaceFileToolStripMenuItem;
        private uint rootsector;
        private uint rootsize;
        private SaveFileDialog saveFileDialog;
        private SaveFileDialog saveFileDialogIso;
        private SafeFileHandle sfh;
        private SplitContainer splitContainer;
        private SplitContainer splitContainerView;
        private StatusStrip statusStrip;
        private ToolStripButton stopToolStripButton;
        private TextBox textBoxLog;
        private ToolStrip toolStrip;
        private ToolStripButton toolStripButtonExport;
        private ToolStripButton toolStripButtonExtract;
        private ToolStripButton toolStripButtonOpen;
        private ToolStripContainer toolStripContainer;
        private ToolStripSeparator toolStripMenuItemSeparator;
        private ToolStripStatusLabel toolStripStatusLabel;
        private ToolStripStatusLabel toolStripStatusLabelGael360;
        private ToolStripStatusLabel toolStripStatusLabelMN;
        private ToolStripStatusLabel toolStripStatusLabelSeparator;
        private ulong totalsize;
        private TreeView treeView;
        public const uint TRUNCATE_EXISTING = 5;
        private ToolStripMenuItem viewToolStripMenuItem;
        private const int wxMN_ERROR_NO_DVD = -4;
        private const int wxMN_ERROR_NO_DVD9 = -8;
        private const int wxMN_ERROR_NO_MEDIA = -2;
        private const int wxMN_ERROR_TO_SMALL = -16;
        public static uint XDVDFS_SECTOR_XBOX1 = 0x30600;
        public static uint XDVDFS_SECTOR_XBOX360 = 0x1fb20;
        public static uint XDVDFS_SECTOR_XBOX360_LAYER1 = 0x1b3880;

        // Methods
        public MainForm(string[] args)
        {
            this.args = args;
            this.InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //new AboutBox().ShowDialog();
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int CloseHandle(IntPtr hObject);
        private void comboFocus(object sender, EventArgs e)
        {
            this.driveToolStrip.Focus();
            this.toolStripStatusLabelMN.Text = "";
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string FileName, uint DesiredAccess, uint ShareMode, IntPtr lpSecurityAttributes, uint CreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        public static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((((DeviceType << 0x10) | (Access << 14)) | (Function << 2)) | Method);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int DeviceIoControl(IntPtr hDevice, int dwIoControlCode, byte[] lpInBuffer, int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, ref int lpBytesReturned, IntPtr lpOverlapped);
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ejectDrive(char c)
        {
            byte[] cmd = new byte[6];
            cmd[0] = 0x1b;
            cmd[1] = 1;
            cmd[4] = 2;
            this.send(c, cmd);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialogIso.ShowDialog() == DialogResult.OK)
            {
                this.saveFile(this.saveFileDialogIso.FileName);
            }
        }

        private static string ExtractExtention(string fullname)
        {
            int num = fullname.LastIndexOf('.');
            return fullname.Substring(num + 1, (fullname.Length - num) - 1);
        }

        private void extractFile(ListViewItem listViewItem)
        {
            this.extractFile(listViewItem, this.folderBrowserDialog.SelectedPath + @"\" + listViewItem.Text);
        }

        private void extractFile(ListViewItem listViewItem, string fullname)
        {
            string text = listViewItem.Text;
            this.dirprefix = "";
            uint num = this.HexToUInt(listViewItem.SubItems[this.columnHeaderAttrb.Index].Text);
            uint sector = Convert.ToUInt32(listViewItem.SubItems[this.columnHeaderSector.Index].Text);
            uint size = Convert.ToUInt32(listViewItem.SubItems[this.columnHeaderSize.Index].Text);
            uint num4 = size / BLOCK_SIZE;
            uint num5 = size - (num4 * BLOCK_SIZE);
            if ((num & 0x10) == 0x10)
            {
                listViewItem.SubItems[this.columnHeaderStatus.Index].Text = "In progress...";
                this.extractFolder(sector, size, fullname);
                listViewItem.SubItems[this.columnHeaderStatus.Index].Text = "Done";
            }
            else
            {
                FileStream stream;
                BinaryWriter writer;
                try
                {
                    Directory.CreateDirectory(ExtractFolder(fullname));
                }
                catch (IOException exception)
                {
                    this.textBoxLog.AppendText("Error : " + exception.Message.ToString() + "\r\n");
                }
                try
                {
                    stream = new FileStream(fullname, FileMode.Create, FileAccess.Write, FileShare.None);
                    writer = new BinaryWriter(stream);
                }
                catch (IOException exception2)
                {
                    this.textBoxLog.AppendText("Error : " + exception2.Message.ToString() + "\r\n");
                    return;
                }
                byte[] data = new byte[BLOCK_SIZE];
                for (uint i = sector; i < (sector + num4); i++)
                {
                    this.readBlock(this.fs, i + this.global_offset, data, 1);
                    writer.Write(data, 0, (int)BLOCK_SIZE);
                    string str = string.Format("{0}%", (100 * (i - sector)) / num4);
                    if (str != listViewItem.SubItems[this.columnHeaderStatus.Index].Text)
                    {
                        listViewItem.SubItems[this.columnHeaderStatus.Index].Text = str;
                        Application.DoEvents();
                    }
                }
                if (num5 != 0)
                {
                    this.readBlock(this.fs, (sector + this.global_offset) + num4, data, 1);
                    writer.Write(data, 0, (int)num5);
                }
                listViewItem.SubItems[this.columnHeaderStatus.Index].Text = "Done";
                writer.Close();
                stream.Close();
                stream.Dispose();
            }
        }

        private void extractFile(uint sector, uint size, string fullname)
        {
            FileStream stream;
            BinaryWriter writer;
            ExtractFilename(fullname);
            uint num = size / 0x800;
            uint num2 = size - (num * 0x800);
            try
            {
                Directory.CreateDirectory(ExtractFolder(fullname));
            }
            catch (IOException exception)
            {
                this.textBoxLog.AppendText("Error : " + exception.Message.ToString() + "\r\n");
            }
            try
            {
                stream = new FileStream(fullname, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new BinaryWriter(stream);
            }
            catch (IOException exception2)
            {
                this.textBoxLog.AppendText("Error : " + exception2.Message.ToString() + "\r\n");
                return;
            }
            byte[] data = new byte[BLOCK_SIZE];
            for (uint i = sector; i < (sector + num); i++)
            {
                this.readBlock(this.fs, i + this.global_offset, data, 1);
                writer.Write(data, 0, (int)BLOCK_SIZE);
                Application.DoEvents();
            }
            if (num2 != 0)
            {
                this.readBlock(this.fs, (sector + this.global_offset) + num, data, 1);
                writer.Write(data, 0, Convert.ToInt16(num2));
            }
            writer.Close();
            stream.Close();
            stream.Dispose();
        }

        private static string ExtractFilename(string fullname)
        {
            int num = fullname.LastIndexOf('\\');
            return fullname.Substring(num + 1, (fullname.Length - num) - 1);
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedItems.Count == 1)
            {
                if (!this.isFolder(this.listView.SelectedItems[0]))
                {
                    this.saveFileDialog.FileName = this.listView.SelectedItems[0].Text;
                    if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        this.extractFile(this.listView.SelectedItems[0], this.saveFileDialog.FileName);
                    }
                }
                else if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    this.extractFile(this.listView.SelectedItems[0], this.folderBrowserDialog.SelectedPath + @"\" + this.listView.SelectedItems[0].Text);
                }
            }
            else if ((this.listView.SelectedItems.Count > 1) && (this.folderBrowserDialog.ShowDialog() == DialogResult.OK))
            {
                for (int i = 0; i < this.listView.SelectedItems.Count; i++)
                {
                    this.extractFile(this.listView.SelectedItems[i], this.folderBrowserDialog.SelectedPath + @"\" + this.listView.SelectedItems[i].Text);
                }
            }
        }

        private void extractFolder(uint sector, uint size, string folder)
        {
            byte[] buffer;
            this.textBoxLog.AppendText(string.Format("extracting folder {0} @ {1:X} ({2:X})\r\n", folder, sector, size));
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (IOException exception)
            {
                this.textBoxLog.AppendText("Error : " + exception.Message.ToString() + "\r\n");
            }
            int num = (int)(size / BLOCK_SIZE);
            if (((size / BLOCK_SIZE) * BLOCK_SIZE) < size)
            {
                num++;
            }
            try
            {
                buffer = new byte[num * BLOCK_SIZE];
                this.readBlock(this.fs, sector + this.global_offset, buffer, num);
            }
            catch (IOException exception2)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception2.Message));
                return;
            }
            this.extractFolderFiles(buffer, 0, this.fs, folder);
        }

        private static string ExtractFolder(string fullname)
        {
            return fullname.Substring(0, fullname.LastIndexOf('\\') + 1);
        }

        private void extractFolderFiles(byte[] data, uint offset, FileStream fp, string folder)
        {
            Application.DoEvents();
            if (offset > data.Length)
            {
                this.textBoxLog.AppendText("bad offset!\r\n");
            }
            else
            {
                uint num = this.getword(data, offset);
                uint num2 = this.getword(data, offset + 2);
                uint sector = this.getint(data, offset + 4);
                uint size = this.getint(data, offset + 8);
                byte num1 = data[(int)((IntPtr)(offset + 12))];
                if (sector == uint.MaxValue)
                {
                    this.textBoxLog.AppendText("error\r\n");
                }
                if (num > 0)
                {
                    this.extractFolderFiles(data, num * 4, fp, folder);
                }
                if ((data[(int)((IntPtr)(offset + 12))] & 0x10) == 0x10)
                {
                    this.textBoxLog.AppendText(string.Format("subfolder : {0}\r\n", this.printentry(data, offset, folder)));
                    this.extractFolder(sector, size, folder + @"\" + this.printentry(data, offset, folder));
                }
                else
                {
                    this.textBoxLog.AppendText(string.Format("subfile : {0} @ 0x{1:X}, size 0x{2:X} in {3}\r\n", new object[] { this.printentry(data, offset, folder), sector, size, folder }));
                    this.extractFile(sector, size, folder + @"\" + this.printentry(data, offset, folder));
                }
                if (num2 > 0)
                {
                    this.extractFolderFiles(data, num2 * 4, fp, folder);
                }
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.treeView.SelectedNode = this.treeView.Nodes[0];
            Application.DoEvents();
            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < this.listView.Items.Count; i++)
                {
                    this.extractFile(this.listView.Items[i]);
                    Application.DoEvents();
                }
            }
        }

        private void fillSPTDB(byte[] cmd, ref SPTDB sptdb)
        {
            sptdb.length = 0x2c;
            sptdb.ScsiStatus = 0;
            sptdb.PathId = 0;
            sptdb.TargetId = 0;
            sptdb.Lun = 0;
            sptdb.SenseInfoLength = 0x23;
            sptdb.SenseInfoOffset = 0x2c;
            sptdb.TimeOutValue = 10;
            sptdb.cdb = new byte[0xff];
            sptdb.Fill = new byte[4];
            sptdb.SenseBuffer = new byte[0x23];
            for (int i = 0; i < cmd.Length; i++)
            {
                sptdb.cdb[i] = cmd[i];
            }
            sptdb.CdbLength = 6;
            sptdb.DataIn = 1;
            sptdb.DataTransfertLength = 0;
            sptdb.DataBuffer = 0;
        }

        [DllImport("kernel32.dll", EntryPoint = "GetDriveTypeA")]
        public static extern uint GetDriveType(string nDrive);
        private uint getint(byte[] data, uint offset)
        {
            if ((offset + 4) > data.Length)
            {
                return 0;
            }
            uint num = (uint)((data[(int)((IntPtr)(offset + 3))] & 0xff) << 0x18);
            num |= (uint)((data[(int)((IntPtr)(offset + 2))] & 0xff) << 0x10);
            num |= (uint)((data[(int)((IntPtr)(offset + 1))] & 0xff) << 8);
            return (num | ((uint)(data[offset] & 0xff)));
        }

        [DllImport("wxMN.dll", SetLastError = true)]
        private static extern int getMN(char c);
        [DllImport("kernel32.dll", EntryPoint = "GetVolumeInformationA")]
        public static extern long GetVolumeInformation(string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize, int lpVolumeSerialNumber, int lpMaximumComponentLength, uint lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, int nFileSystemNameSize);
        private uint getword(byte[] data, uint offset)
        {
            if ((offset + 2) > data.Length)
            {
                return 0;
            }
            uint num = (uint)((data[(int)((IntPtr)(offset + 1))] & 0xff) << 8);
            return (num | ((uint)(data[offset] & 0xff)));
        }

        private int HexToInt(string hex)
        {
            if (hex.Substring(0, 2).ToLower() == "0x")
            {
                hex = hex.Substring(2);
            }
            try
            {
                return int.Parse(hex, NumberStyles.HexNumber);
            }
            catch (FormatException exception)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception.Message));
                return 0;
            }
        }

        private uint HexToUInt(string hex)
        {
            if (hex.Substring(0, 2).ToLower() == "0x")
            {
                hex = hex.Substring(2);
            }
            try
            {
                return uint.Parse(hex, NumberStyles.HexNumber);
            }
            catch (FormatException exception)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception.Message));
                return 0;
            }
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItemSeparator = new ToolStripSeparator();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.questionToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.statusStrip = new StatusStrip();
            this.toolStripStatusLabel = new ToolStripStatusLabel();
            this.toolStripStatusLabelMN = new ToolStripStatusLabel();
            this.toolStripStatusLabelSeparator = new ToolStripStatusLabel();
            this.toolStripStatusLabelGael360 = new ToolStripStatusLabel();
            this.openFileDialog = new OpenFileDialog();
            this.folderBrowserDialog = new FolderBrowserDialog();
            this.imageListTreeView = new ImageList(this.components);
            this.imageList = new ImageList(this.components);
            this.contextMenuStrip = new ContextMenuStrip(this.components);
            this.viewToolStripMenuItem = new ToolStripMenuItem();
            this.extractFileToolStripMenuItem = new ToolStripMenuItem();
            this.replaceFileToolStripMenuItem = new ToolStripMenuItem();
            this.saveFileDialog = new SaveFileDialog();
            this.saveFileDialogIso = new SaveFileDialog();
            this.contextMenuStripMulti = new ContextMenuStrip(this.components);
            this.extractFilesToolStripMenuItem = new ToolStripMenuItem();
            this.openFileDialogReplace = new OpenFileDialog();
            this.toolStripContainer = new ToolStripContainer();
            this.splitContainer = new SplitContainer();
            this.splitContainerView = new SplitContainer();
            this.treeView = new TreeView();
            this.listView = new ListView();
            this.columnHeaderName = new ColumnHeader();
            this.columnHeaderAttrb = new ColumnHeader();
            this.columnHeaderSector = new ColumnHeader();
            this.columnHeaderSize = new ColumnHeader();
            this.columnHeaderStatus = new ColumnHeader();
            this.textBoxLog = new TextBox();
            this.toolStrip = new ToolStrip();
            this.driveToolStrip = new ToolStrip();
            this.driveToolStripComboBox = new ToolStripComboBox();
            this.toolStripButtonOpen = new ToolStripButton();
            this.toolStripButtonExtract = new ToolStripButton();
            this.toolStripButtonExport = new ToolStripButton();
            this.openDvdToolStripButton = new ToolStripButton();
            this.stopToolStripButton = new ToolStripButton();
            this.openToolStripMenuItem = new ToolStripMenuItem();
            this.extractToolStripMenuItem = new ToolStripMenuItem();
            this.exportToolStripMenuItem = new ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.contextMenuStripMulti.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.splitContainerView.Panel1.SuspendLayout();
            this.splitContainerView.Panel2.SuspendLayout();
            this.splitContainerView.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.driveToolStrip.SuspendLayout();
            base.SuspendLayout();
            this.menuStrip.Items.AddRange(new ToolStripItem[] { this.fileToolStripMenuItem, this.questionToolStripMenuItem });
            this.menuStrip.Location = new Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new Size(0x27b, 0x18);
            this.menuStrip.TabIndex = 0;
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.openToolStripMenuItem, this.extractToolStripMenuItem, this.exportToolStripMenuItem, this.toolStripMenuItemSeparator, this.exitToolStripMenuItem });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(0x23, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.toolStripMenuItemSeparator.Name = "toolStripMenuItemSeparator";
            this.toolStripMenuItemSeparator.Size = new Size(0x90, 6);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(0x93, 0x16);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);
            this.questionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.aboutToolStripMenuItem });
            this.questionToolStripMenuItem.Name = "questionToolStripMenuItem";
            this.questionToolStripMenuItem.Size = new Size(0x18, 20);
            this.questionToolStripMenuItem.Text = "?";
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(0x8a, 0x16);
            this.aboutToolStripMenuItem.Text = "About wx360";
            this.aboutToolStripMenuItem.Click += new EventHandler(this.aboutToolStripMenuItem_Click);
            this.statusStrip.Items.AddRange(new ToolStripItem[] { this.toolStripStatusLabel, this.toolStripStatusLabelMN, this.toolStripStatusLabelSeparator, this.toolStripStatusLabelGael360 });
            this.statusStrip.Location = new Point(0, 0x146);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new Size(0x27b, 0x16);
            this.statusStrip.TabIndex = 1;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new Size(560, 0x11);
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.Text = "wx360";
            this.toolStripStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.toolStripStatusLabelMN.Name = "toolStripStatusLabelMN";
            this.toolStripStatusLabelMN.Size = new Size(10, 0x11);
            this.toolStripStatusLabelMN.Text = " ";
            this.toolStripStatusLabelSeparator.BorderSides = ToolStripStatusLabelBorderSides.All;
            this.toolStripStatusLabelSeparator.BorderStyle = Border3DStyle.Sunken;
            this.toolStripStatusLabelSeparator.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelSeparator.Name = "toolStripStatusLabelSeparator";
            this.toolStripStatusLabelSeparator.Size = new Size(4, 0x11);
            this.toolStripStatusLabelGael360.Name = "toolStripStatusLabelGael360";
            this.toolStripStatusLabelGael360.Size = new Size(0x2e, 0x11);
            this.toolStripStatusLabelGael360.Text = "Gael360";
            this.openFileDialog.Filter = "x360 ISO (*.iso;*.360)|*.iso;*.360|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Select x360 ISO file";
            this.imageListTreeView.ImageStream = (ImageListStreamer)manager.GetObject("imageListTreeView.ImageStream");
            this.imageListTreeView.TransparentColor = Color.Fuchsia;
            this.imageListTreeView.Images.SetKeyName(0, "cd.bmp");
            this.imageListTreeView.Images.SetKeyName(1, "VSFolder_closed.bmp");
            this.imageListTreeView.Images.SetKeyName(2, "VSFolder_open.bmp");
            this.imageList.ImageStream = (ImageListStreamer)manager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Fuchsia;
            this.imageList.Images.SetKeyName(0, "VSFolder_closed.bmp");
            this.imageList.Images.SetKeyName(1, "VSProject_genericfile.bmp");
            this.imageList.Images.SetKeyName(2, "VSProject_form.bmp");
            this.imageList.Images.SetKeyName(3, "ico120-16.ico");
            this.imageList.Images.SetKeyName(4, "InsertPictureHS.png");
            this.contextMenuStrip.Items.AddRange(new ToolStripItem[] { this.viewToolStripMenuItem, this.extractFileToolStripMenuItem, this.replaceFileToolStripMenuItem });
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new Size(0x84, 70);
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new Size(0x83, 0x16);
            this.viewToolStripMenuItem.Text = "View";
            this.viewToolStripMenuItem.Click += new EventHandler(this.viewToolStripMenuItem_Click);
            this.extractFileToolStripMenuItem.Name = "extractFileToolStripMenuItem";
            this.extractFileToolStripMenuItem.Size = new Size(0x83, 0x16);
            this.extractFileToolStripMenuItem.Text = "Extract File";
            this.extractFileToolStripMenuItem.Click += new EventHandler(this.extractFileToolStripMenuItem_Click);
            this.replaceFileToolStripMenuItem.Name = "replaceFileToolStripMenuItem";
            this.replaceFileToolStripMenuItem.Size = new Size(0x83, 0x16);
            this.replaceFileToolStripMenuItem.Text = "Replace File";
            this.replaceFileToolStripMenuItem.Click += new EventHandler(this.replaceFileToolStripMenuItem_Click);
            this.saveFileDialog.Filter = "All files (*.*)|*.*";
            this.saveFileDialogIso.Filter = "x360 ISO (*.iso;*.360)|*.iso;*.360|All Files (*.*)|*.*";
            this.contextMenuStripMulti.Items.AddRange(new ToolStripItem[] { this.extractFilesToolStripMenuItem });
            this.contextMenuStripMulti.Name = "contextMenuStripMulti";
            this.contextMenuStripMulti.Size = new Size(0x86, 0x1a);
            this.extractFilesToolStripMenuItem.Name = "extractFilesToolStripMenuItem";
            this.extractFilesToolStripMenuItem.Size = new Size(0x85, 0x16);
            this.extractFilesToolStripMenuItem.Text = "Extract Files";
            this.extractFilesToolStripMenuItem.Click += new EventHandler(this.extractFileToolStripMenuItem_Click);
            this.openFileDialogReplace.Filter = "All files (*.*)|*.*";
            this.toolStripContainer.ContentPanel.Controls.Add(this.splitContainer);
            this.toolStripContainer.ContentPanel.Size = new Size(0x27b, 0x115);
            this.toolStripContainer.Dock = DockStyle.Fill;
            this.toolStripContainer.Location = new Point(0, 0x18);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.Size = new Size(0x27b, 0x12e);
            this.toolStripContainer.TabIndex = 8;
            this.toolStripContainer.Text = "toolStripContainer1";
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.driveToolStrip);
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.FixedPanel = FixedPanel.Panel2;
            this.splitContainer.Location = new Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = Orientation.Horizontal;
            this.splitContainer.Panel1.Controls.Add(this.splitContainerView);
            this.splitContainer.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainer.Size = new Size(0x27b, 0x115);
            this.splitContainer.SplitterDistance = 0xd0;
            this.splitContainer.TabIndex = 8;
            this.splitContainerView.Dock = DockStyle.Fill;
            this.splitContainerView.FixedPanel = FixedPanel.Panel1;
            this.splitContainerView.Location = new Point(0, 0);
            this.splitContainerView.Name = "splitContainerView";
            this.splitContainerView.Panel1.Controls.Add(this.treeView);
            this.splitContainerView.Panel2.Controls.Add(this.listView);
            this.splitContainerView.Size = new Size(0x27b, 0xd0);
            this.splitContainerView.SplitterDistance = 0xb6;
            this.splitContainerView.TabIndex = 0;
            this.treeView.Dock = DockStyle.Fill;
            this.treeView.FullRowSelect = true;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageListTreeView;
            this.treeView.Location = new Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new Size(0xb6, 0xd0);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new TreeViewEventHandler(this.treeView_AfterSelect);
            this.listView.Columns.AddRange(new ColumnHeader[] { this.columnHeaderName, this.columnHeaderAttrb, this.columnHeaderSector, this.columnHeaderSize, this.columnHeaderStatus });
            this.listView.Dock = DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.Location = new Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new Size(0x1c1, 0xd0);
            this.listView.SmallImageList = this.imageList;
            this.listView.TabIndex = 5;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = View.Details;
            this.listView.MouseClick += new MouseEventHandler(this.listView_MouseClick);
            this.listView.ColumnClick += new ColumnClickEventHandler(this.listView_ColumnClick);
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 150;
            this.columnHeaderAttrb.Text = "Attrb";
            this.columnHeaderAttrb.Width = 50;
            this.columnHeaderSector.Text = "Sector";
            this.columnHeaderSector.TextAlign = HorizontalAlignment.Right;
            this.columnHeaderSector.Width = 80;
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.TextAlign = HorizontalAlignment.Right;
            this.columnHeaderSize.Width = 80;
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 50;
            this.textBoxLog.Dock = DockStyle.Fill;
            this.textBoxLog.Font = new Font("Lucida Console", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.textBoxLog.Location = new Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = ScrollBars.Both;
            this.textBoxLog.Size = new Size(0x27b, 0x41);
            this.textBoxLog.TabIndex = 3;
            this.toolStrip.Dock = DockStyle.None;
            this.toolStrip.Items.AddRange(new ToolStripItem[] { this.toolStripButtonOpen, this.toolStripButtonExtract, this.toolStripButtonExport });
            this.toolStrip.Location = new Point(3, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(0x51, 0x19);
            this.toolStrip.TabIndex = 7;
            this.driveToolStrip.Dock = DockStyle.None;
            this.driveToolStrip.Items.AddRange(new ToolStripItem[] { this.driveToolStripComboBox, this.openDvdToolStripButton, this.stopToolStripButton });
            this.driveToolStrip.Location = new Point(0x54, 0);
            this.driveToolStrip.Name = "driveToolStrip";
            this.driveToolStrip.Size = new Size(0xfb, 0x19);
            this.driveToolStrip.TabIndex = 11;
            this.driveToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.driveToolStripComboBox.Name = "driveToolStripComboBox";
            this.driveToolStripComboBox.Size = new Size(160, 0x19);
            this.driveToolStripComboBox.SelectedIndexChanged += new EventHandler(this.comboFocus);
            this.toolStripButtonOpen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //this.toolStripButtonOpen.Image = Resources.openHS;
            this.toolStripButtonOpen.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new Size(0x17, 0x16);
            this.toolStripButtonOpen.Text = "Open";
            this.toolStripButtonOpen.Click += new EventHandler(this.openToolStripMenuItem_Click);
            this.toolStripButtonExtract.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButtonExtract.Enabled = false;
            //this.toolStripButtonExtract.Image = Resources.ShowAllCommentsHS;
            this.toolStripButtonExtract.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonExtract.Name = "toolStripButtonExtract";
            this.toolStripButtonExtract.Size = new Size(0x17, 0x16);
            this.toolStripButtonExtract.Text = "Extract all files";
            this.toolStripButtonExtract.Click += new EventHandler(this.extractToolStripMenuItem_Click);
            this.toolStripButtonExport.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButtonExport.Enabled = false;
            //this.toolStripButtonExport.Image = Resources.saveHS;
            this.toolStripButtonExport.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonExport.Name = "toolStripButtonExport";
            this.toolStripButtonExport.Size = new Size(0x17, 0x16);
            this.toolStripButtonExport.Text = "Export XDVDFS";
            this.toolStripButtonExport.Click += new EventHandler(this.exportToolStripMenuItem_Click);
            this.openDvdToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //this.openDvdToolStripButton.Image = Resources.cd;
            this.openDvdToolStripButton.ImageTransparentColor = Color.Magenta;
            this.openDvdToolStripButton.Name = "openDvdToolStripButton";
            this.openDvdToolStripButton.Size = new Size(0x17, 0x16);
            this.openDvdToolStripButton.Text = "Explore";
            this.openDvdToolStripButton.Click += new EventHandler(this.openDvdToolStripButton_Click);
            this.stopToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //this.stopToolStripButton.Image = Resources.StopHS;
            this.stopToolStripButton.ImageTransparentColor = Color.Magenta;
            this.stopToolStripButton.Name = "stopToolStripButton";
            this.stopToolStripButton.Size = new Size(0x17, 0x16);
            this.stopToolStripButton.Text = "Stop drive";
            this.stopToolStripButton.Click += new EventHandler(this.stopToolStripButton_Click);
            //this.openToolStripMenuItem.Image = Resources.openHS;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new Size(0x93, 0x16);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new EventHandler(this.openToolStripMenuItem_Click);
            this.extractToolStripMenuItem.Enabled = false;
            //this.extractToolStripMenuItem.Image = Resources.ShowAllCommentsHS;
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new Size(0x93, 0x16);
            this.extractToolStripMenuItem.Text = "Extract all files";
            this.extractToolStripMenuItem.Click += new EventHandler(this.extractToolStripMenuItem_Click);
            this.exportToolStripMenuItem.Enabled = false;
            //this.exportToolStripMenuItem.Image = Resources.saveHS;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new Size(0x93, 0x16);
            this.exportToolStripMenuItem.Text = "Export XDVDFS";
            this.exportToolStripMenuItem.Click += new EventHandler(this.exportToolStripMenuItem_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x27b, 0x15c);
            base.Controls.Add(this.toolStripContainer);
            base.Controls.Add(this.statusStrip);
            base.Controls.Add(this.menuStrip);
            base.Icon = (Icon)manager.GetObject("$this.Icon");
            base.MainMenuStrip = this.menuStrip;
            base.Name = "MainForm";
            this.Text = "wx360";
            base.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
            base.Load += new EventHandler(this.MainForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.contextMenuStripMulti.ResumeLayout(false);
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            this.splitContainerView.Panel1.ResumeLayout(false);
            this.splitContainerView.Panel2.ResumeLayout(false);
            this.splitContainerView.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.driveToolStrip.ResumeLayout(false);
            this.driveToolStrip.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool isFolder(ListViewItem listViewItem)
        {
            return (this.HexToInt(listViewItem.SubItems[this.columnHeaderAttrb.Index].Text) == 0x10);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //this.listView.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        private void listView_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (this.listView.SelectedItems.Count == 1))
            {
                if (this.isFolder(this.listView.SelectedItems[0]))
                {
                    this.contextMenuStripMulti.Show(this.listView, e.X, e.Y);
                }
                else
                {
                    this.contextMenuStrip.Show(this.listView, e.X, e.Y);
                }
            }
            else if ((e.Button == MouseButtons.Right) && (this.listView.SelectedItems.Count > 1))
            {
                this.contextMenuStripMulti.Show(this.listView, e.X, e.Y);
            }
        }

        private void loadDrive(char c)
        {
            byte[] cmd = new byte[6];
            cmd[0] = 0x1b;
            cmd[1] = 1;
            cmd[4] = 3;
            this.send(c, cmd);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.fs != null)
            {
                this.fs.Close();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.toolStripStatusLabel.Text = Application.ProductName + " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " BETA";
            this.magic_number = 0;
            StringBuilder lpVolumeNameBuffer = new StringBuilder(0x100);
            StringBuilder lpFileSystemNameBuffer = new StringBuilder(0x100);
            int lpVolumeSerialNumber = 0;
            int lpMaximumComponentLength = 0;
            uint lpFileSystemFlags = 0;
            this.toolStripStatusLabel.Text = string.Format("{0} - {1}", Application.ProductName, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            string[] logicalDrives = Directory.GetLogicalDrives();
            for (int i = 0; i < logicalDrives.Length; i++)
            {
                lpVolumeNameBuffer.Length = 0;
                lpFileSystemNameBuffer.Length = 0;
                GetVolumeInformation(logicalDrives[i], lpVolumeNameBuffer, lpVolumeNameBuffer.Capacity, lpVolumeSerialNumber, lpMaximumComponentLength, lpFileSystemFlags, lpFileSystemNameBuffer, lpFileSystemNameBuffer.Capacity);
                if (GetDriveType(logicalDrives[i]) == 5)
                {
                    if (lpFileSystemNameBuffer.Length > 0)
                    {
                        this.driveToolStripComboBox.Items.Add(string.Format("{0} {1} [{2}]", logicalDrives[i], lpVolumeNameBuffer, lpFileSystemNameBuffer));
                    }
                    else
                    {
                        this.driveToolStripComboBox.Items.Add(string.Format("{0} {1}", logicalDrives[i], lpVolumeNameBuffer));
                    }
                }
            }
            if (logicalDrives.Length > 0)
            {
                this.driveToolStripComboBox.SelectedIndex = 0;
            }
            if (this.args.Length > 0)
            {
                this.OpenDVD(this.args[0][0], Convert.ToUInt32(this.args[1]));
            }
        }

        private void OpenDVD(char c, uint mz)
        {
            try
            {
                this.sfh = CreateFile(string.Format(@"\\.\{0}:", c), 0x80000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (this.fs != null)
                {
                    this.fs.Close();
                }
                this.fs = new FileStream(this.sfh, FileAccess.Read);
            }
            catch (IOException exception)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception.Message));
            }
            this.magic_number = mz;
            this.toolStripStatusLabelMN.Text = this.magic_number.ToString();
            this.replaceFileToolStripMenuItem.Enabled = false;
            this.OpenFile(string.Format("{0}:", c), false);
        }

        private void openDvdToolStripButton_Click(object sender, EventArgs e)
        {
            int num = getMN(this.driveToolStripComboBox.Text[0]);
            switch (num)
            {
                case -2:
                    this.textBoxLog.AppendText("Error : no media loaded\r\n");
                    return;

                case -4:
                    this.textBoxLog.AppendText("Error : the loaded media isn't a DVD\r\n");
                    return;

                case -8:
                    this.textBoxLog.AppendText("Error : the loaded media isn't a DVD9\r\n");
                    return;

                case -16:
                    this.textBoxLog.AppendText("Error : hotswap DVD is too small\r\n\r\n");
                    return;

                case -1:
                    this.textBoxLog.AppendText("Unknow error\r\n\r\n");
                    return;
            }
            this.magic_number = (uint)num;
            this.textBoxLog.AppendText(string.Format("Found magic number : {0}\r\n", this.magic_number));
            this.toolStripStatusLabelMN.Text = this.magic_number.ToString();
            this.OpenDVD(this.driveToolStripComboBox.Text[0], this.magic_number);
        }

        private void OpenFile(string FileName, bool canExport)
        {
            TreeNode node;
            this.extractToolStripMenuItem.Enabled = false;
            this.toolStripButtonExtract.Enabled = false;
            this.exportToolStripMenuItem.Enabled = false;
            this.toolStripButtonExport.Enabled = false;
            this.listView.BeginUpdate();
            this.listView.Items.Clear();
            this.listView.EndUpdate();
            this.textBoxLog.Clear();
            this.Text = "wx360 - " + ExtractFilename(FileName);
            this.dirprefix = "";
            byte[] data = new byte[0x800];
            this.readBlock(this.fs, 0x20, data, 1);
            if (this.ReadByte(data).Substring(0, 20) == "MICROSOFT*XBOX*MEDIA")
            {
                this.rootsector = this.getint(data, 20);
                this.rootsize = this.getint(data, 0x18);
                this.global_offset = 0;
                this.extractToolStripMenuItem.Enabled = true;
                this.toolStripButtonExtract.Enabled = true;
                this.exportToolStripMenuItem.Enabled = false;
                this.toolStripButtonExport.Enabled = false;
            }
            else
            {
                this.global_offset = XDVDFS_SECTOR_XBOX360;
                this.readBlock(this.fs, this.global_offset + 0x20, data, 1);
                if (this.ReadByte(data).Substring(0, 20) == "MICROSOFT*XBOX*MEDIA")
                {
                    this.rootsector = this.getint(data, 20);
                    this.rootsize = this.getint(data, 0x18);
                    this.extractToolStripMenuItem.Enabled = true;
                    this.toolStripButtonExtract.Enabled = true;
                    this.exportToolStripMenuItem.Enabled = canExport;
                    this.toolStripButtonExport.Enabled = canExport;
                }
                else
                {
                    this.global_offset = XDVDFS_SECTOR_XBOX1;
                    this.readBlock(this.fs, this.global_offset + 0x20, data, 1);
                    if (this.ReadByte(data).Substring(0, 20) == "MICROSOFT*XBOX*MEDIA")
                    {
                        this.rootsector = this.getint(data, 20);
                        this.rootsize = this.getint(data, 0x18);
                        this.extractToolStripMenuItem.Enabled = true;
                        this.toolStripButtonExtract.Enabled = true;
                        this.exportToolStripMenuItem.Enabled = canExport;
                        this.toolStripButtonExport.Enabled = canExport;
                    }
                    else
                    {
                        MessageBox.Show("Error : not a XBOX ISO");
                        this.fs.Close();
                        return;
                    }
                }
            }
            if (this.rootsize < BLOCK_SIZE)
            {
                this.rootsize = BLOCK_SIZE;
            }
            byte[] buffer2 = new byte[this.rootsize];
            this.listView.BeginUpdate();
            this.listView.Items.Clear();
            this.treeView.BeginUpdate();
            this.treeView.Nodes.Clear();
            this.readBlock(this.fs, this.rootsector + this.global_offset, buffer2, (int)(this.rootsize / BLOCK_SIZE));
            this.totalsize = 0L;
            string text = ExtractFilename(FileName);
            if (text.LastIndexOf(".") > 0)
            {
                text = text.Substring(0, text.LastIndexOf("."));
            }

            node = new TreeNode(text)
            {
                ImageIndex = 0,
                SelectedImageIndex = 0,
                Tag = string.Format("{0}|{1}", this.rootsector, this.rootsize),
            };
            node.ToolTipText = node.Tag.ToString();
            this.treeView.Nodes.Add(node);
            this.Parse(buffer2, 0, this.fs, this.dirprefix, node, true, true);
            this.textBoxLog.AppendText(string.Format("rootsector     0x{0:X8} ({0:G})\r\n", this.rootsector));
            this.textBoxLog.AppendText(string.Format("rootsize       0x{0:X8} ({0:G})\r\n", this.rootsize));
            this.textBoxLog.AppendText(string.Format("totalsize      0x{0:X8} ({0:G})\r\n", this.totalsize));
            this.textBoxLog.AppendText(string.Format("global_offset  0x{0:X8} ({0:G})\r\n", this.global_offset));
            this.listView.EndUpdate();
            this.treeView.EndUpdate();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.isofile = this.openFileDialog.FileName;
                try
                {
                    if (this.fs != null)
                    {
                        this.fs.Close();
                    }
                    this.fs = new FileStream(this.isofile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (IOException exception)
                {
                    this.textBoxLog.AppendText("Error : " + exception.Message.ToString() + "\r\n");
                    return;
                }
                this.magic_number = 0;
                this.toolStripStatusLabelMN.Text = this.magic_number.ToString();
                this.replaceFileToolStripMenuItem.Enabled = true;
                this.OpenFile(this.isofile, true);
            }
        }

        private void Parse(byte[] data, uint offset, FileStream fp, string dirprefix, TreeNode tn, bool addfiles, bool addsub)
        {
            if (offset > data.Length)
            {
                this.textBoxLog.AppendText("bad offset!\r\n");
            }
            else
            {
                if (this.getint(data, offset + 4) == uint.MaxValue)
                {
                    this.textBoxLog.AppendText("error\r\n");
                }
                uint num2 = this.getword(data, offset);
                uint num3 = this.getword(data, offset + 2);
                if (num2 > 0)
                {
                    this.Parse(data, num2 * 4, fp, dirprefix, tn, addfiles, addsub);
                }
                uint num4 = data[(int)((IntPtr)(offset + 12))];
                if ((data[(int)((IntPtr)(offset + 12))] & 0x10) == 0x10)
                {
                    uint num5 = this.getint(data, offset + 4);
                    uint num6 = this.getint(data, offset + 8);
                    if ((num5 > 0) && (num6 > 0))
                    {
                        string str;
                        if (num6 != ((num6 / BLOCK_SIZE) * BLOCK_SIZE))
                        {
                            num6 = ((num6 / BLOCK_SIZE) * BLOCK_SIZE) + BLOCK_SIZE;
                        }
                        byte[] buffer = new byte[num6];
                        if (dirprefix == null)
                        {
                            str = this.printentry(data, offset, dirprefix);
                        }
                        else
                        {
                            str = dirprefix + @"\" + this.printentry(data, offset, dirprefix);
                        }
                        if (addsub)
                        {
                            TreeNode node = new TreeNode(this.printentry(data, offset, dirprefix))
                            {
                                //Tag = num5,
                                ImageIndex = 1,
                                SelectedImageIndex = 2,
                                Tag = string.Format("{0}|{1}", num5, num6),
                                ToolTipText = tn.Tag.ToString()
                            };
                            tn.Nodes.Add(node);
                            this.readBlock(fp, num5 + this.global_offset, buffer, (int)(num6 / BLOCK_SIZE));
                            this.Parse(buffer, 0, fp, str, node, false, addsub);
                        }
                    }
                }
                else
                {
                    this.totalsize += this.getint(data, offset + 8);
                }
                if (addfiles)
                {
                    ListViewItem item = new ListViewItem(this.printentry(data, offset, dirprefix));
                    item.SubItems.Add(string.Format("0x{0:X2}", num4));
                    item.SubItems.Add(string.Format("{0}", this.getint(data, offset + 4)));
                    item.SubItems.Add(string.Format("{0}", this.getint(data, offset + 8)));
                    item.SubItems.Add("");
                    switch (num4)
                    {
                        case 0x10:
                        case 0x30:
                            item.ImageIndex = 0;
                            break;

                        default:
                            if (ExtractExtention(item.Text).ToLower() == "xex")
                            {
                                item.ImageIndex = 2;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "wmv")
                            {
                                item.ImageIndex = 3;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "wma")
                            {
                                item.ImageIndex = 3;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "wav")
                            {
                                item.ImageIndex = 3;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "mp3")
                            {
                                item.ImageIndex = 3;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "bmp")
                            {
                                item.ImageIndex = 4;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "jpg")
                            {
                                item.ImageIndex = 4;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "gif")
                            {
                                item.ImageIndex = 4;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "png")
                            {
                                item.ImageIndex = 4;
                            }
                            else if (ExtractExtention(item.Text).ToLower() == "tga")
                            {
                                item.ImageIndex = 4;
                            }
                            else
                            {
                                item.ImageIndex = 1;
                            }
                            break;
                    }
                    this.listView.Items.Add(item);
                }
                if (num3 > 0)
                {
                    this.Parse(data, num3 * 4, fp, dirprefix, tn, addfiles, addsub);
                }
            }
        }

        private string printentry(byte[] data, uint offset, string dirprefix)
        {
            StringBuilder builder = new StringBuilder();
            int num = data[(int)((IntPtr)(offset + 13))];
            for (int i = 0; i < num; i++)
            {
                builder.Append((char)data[(int)((IntPtr)((offset + 14) + i))]);
            }
            return builder.ToString();
        }

        private int readBlock(FileStream fp, uint LBA, byte[] data, int num)
        {
            long num2 = 0L;
            if (LBA >= (XDVDFS_SECTOR_XBOX360 + XDVDFS_SECTOR_XBOX360_LAYER1))
            {
                num2 = this.magic_number;
            }
            fp.Seek((LBA + num2) * BLOCK_SIZE, SeekOrigin.Begin);
            fp.Read(data, 0, (int)(BLOCK_SIZE * num));
            return 0;
        }

        private string ReadByte(byte[] data)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append((char)data[i]);
            }
            return builder.ToString();
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialogReplace.ShowDialog() == DialogResult.OK)
            {
                uint sector = Convert.ToUInt32(this.listView.SelectedItems[0].SubItems[this.columnHeaderSector.Index].Text);
                uint num2 = Convert.ToUInt32(this.listView.SelectedItems[0].SubItems[this.columnHeaderSize.Index].Text);
                FileInfo info = new FileInfo(this.openFileDialogReplace.FileName);
                if (info.Length > num2)
                {
                    MessageBox.Show(string.Format("This file is too large!\r\nThe size of the new file must be less or equal to {0}!", num2), "wx360 - Replace file", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    this.writeFile(sector, num2, this.openFileDialogReplace.FileName, this.listView.SelectedItems[0]);
                }
            }
        }

        private void saveFile(string targetfile)
        {
            FileStream stream;
            BinaryReader reader;
            BinaryWriter writer;
            uint num = this.global_offset;
            uint num2 = 0;
            uint num3 = 0;
            try
            {
                reader = new BinaryReader(this.fs);
                num2 = (uint)((ulong)reader.BaseStream.Length / ((ulong)BLOCK_SIZE));
                num3 = ((uint)reader.BaseStream.Length) - (num2 * BLOCK_SIZE);
            }
            catch (IOException exception)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception.Message));
                return;
            }
            try
            {
                stream = new FileStream(targetfile, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new BinaryWriter(stream);
            }
            catch (IOException exception2)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception2.Message));
                return;
            }
            catch (UnauthorizedAccessException exception3)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception3.Message));
                return;
            }
            byte[] buffer = new byte[BLOCK_SIZE];
            ToolStripStatusLabel label = new ToolStripStatusLabel();
            ToolStripProgressBar bar = new ToolStripProgressBar
            {
                Style = ProgressBarStyle.Blocks
            };
            label.Text = "0%";
            bar.Value = 0;
            this.statusStrip.Items.Add(label);
            this.statusStrip.Items.Add(bar);
            while (num < num2)
            {
                try
                {
                    reader.BaseStream.Seek((long)(num * BLOCK_SIZE), SeekOrigin.Begin);
                    reader.Read(buffer, 0, (int)BLOCK_SIZE);
                    writer.Write(buffer, 0, (int)BLOCK_SIZE);
                }
                catch (IOException exception4)
                {
                    this.textBoxLog.AppendText(string.Format("{0}\r\n", exception4.Message));
                    return;
                }
                catch (UnauthorizedAccessException exception5)
                {
                    this.textBoxLog.AppendText(string.Format("{0}\r\n", exception5.Message));
                    return;
                }
                num++;
                bar.Value = (int)((100 * (num - this.global_offset)) / (num2 - this.global_offset));
                label.Text = bar.Value.ToString() + "%";
                Application.DoEvents();
            }
            reader.BaseStream.Seek((long)(num * BLOCK_SIZE), SeekOrigin.Begin);
            reader.Read(buffer, 0, (int)num3);
            writer.Write(buffer, 0, (int)num3);
            reader.Close();
            writer.Close();
            stream.Close();
            label.Text = "100%";
            bar.Value = 100;
            Application.DoEvents();
            label.Dispose();
            bar.Dispose();
        }

        private void send(char c, byte[] cmd)
        {
            SafeFileHandle handle;
            try
            {
                handle = CreateFile(string.Format(@"\\.\{0}:", c), 0xc0000000, 1, IntPtr.Zero, 3, 0, IntPtr.Zero);
            }
            catch (IOException)
            {
                return;
            }
            SPTDB sptdb = new SPTDB();
            this.fillSPTDB(cmd, ref sptdb);
            try
            {
                if (this.sendCommand(handle.DangerousGetHandle(), sptdb) == -1)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Win32Exception)
            {
            }
            handle.Close();
            handle.Dispose();
        }

        private int sendCommand(IntPtr hDevice, SPTDB sptdb)
        {
            int cb = Marshal.SizeOf(sptdb);
            byte[] destination = new byte[cb];
            byte[] buffer2 = new byte[cb];
            IntPtr ptr = Marshal.AllocHGlobal(cb);
            Marshal.StructureToPtr(sptdb, ptr, false);
            Marshal.Copy(ptr, destination, 0, cb);
            Marshal.Copy(ptr, buffer2, 0, cb);
            int lpBytesReturned = 0;
            if (DeviceIoControl(hDevice, 0x4d014, destination, destination.Length, buffer2, buffer2.Length, ref lpBytesReturned, IntPtr.Zero) == 0)
            {
                return -1;
            }
            return lpBytesReturned;
        }

        [DllImport("shell32.dll")]
        public static extern int ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
        private void slowDrive(char c)
        {
            byte[] cmd = new byte[5];
            cmd[0] = 0x1b;
            cmd[1] = 1;
            cmd[4] = 0x20;
            this.send(c, cmd);
        }

        private void spinDrive(char c)
        {
            byte[] cmd = new byte[5];
            cmd[0] = 0x1b;
            cmd[1] = 1;
            cmd[4] = 1;
            this.send(c, cmd);
        }

        private void stopDrive(char c)
        {
            byte[] cmd = new byte[5];
            cmd[0] = 0x1b;
            cmd[1] = 1;
            cmd[4] = 0x30;
            this.send(c, cmd);
        }

        private void stopToolStripButton_Click(object sender, EventArgs e)
        {
            this.stopDrive(this.driveToolStripComboBox.Text[0]);
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.IsSelected)
            {
                string str = e.Node.Tag.ToString();
                uint num = Convert.ToUInt32(str.Substring(0, str.IndexOf('|')));
                uint num2 = Convert.ToUInt32(str.Substring(str.IndexOf('|') + 1));
                if (num2 < BLOCK_SIZE)
                {
                    num2 = BLOCK_SIZE;
                }
                byte[] data = new byte[num2];
                this.listView.BeginUpdate();
                this.listView.Items.Clear();
                this.readBlock(this.fs, num + this.global_offset, data, (int)(num2 / BLOCK_SIZE));
                this.Parse(data, 0, this.fs, this.dirprefix, e.Node, true, false);
                this.listView.EndUpdate();
            }
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedItems.Count >= 1)
            {
                string fullname = Path.GetTempPath() + @"\" + this.listView.SelectedItems[0].Text;
                this.extractFile(this.listView.SelectedItems[0], fullname);
                ShellExecute(base.Handle, "open", fullname, "", "", 1);
            }
        }

        private void writeFile(uint sector, uint size, string inputfile, ListViewItem listViewItem)
        {
            FileStream stream;
            FileStream stream2;
            BinaryReader reader;
            BinaryWriter writer;
            try
            {
                stream2 = new FileStream(inputfile, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new BinaryReader(stream2);
            }
            catch (IOException exception)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception.Message));
                return;
            }
            try
            {
                stream = new FileStream(this.isofile, FileMode.Open, FileAccess.Write, FileShare.Read);
                writer = new BinaryWriter(stream);
            }
            catch (IOException exception2)
            {
                this.textBoxLog.AppendText(string.Format("{0}\r\n", exception2.Message));
                return;
            }
            int num = (int)((ulong)reader.BaseStream.Length / ((ulong)BLOCK_SIZE));
            int count = (int)(((ulong)reader.BaseStream.Length) - (ulong)(num * BLOCK_SIZE));
            int num3 = (int)(size - reader.BaseStream.Length);
            reader.BaseStream.Seek(0L, SeekOrigin.Begin);
            writer.BaseStream.Seek((long)((sector + this.global_offset) * BLOCK_SIZE), SeekOrigin.Begin);
            byte[] buffer = new byte[BLOCK_SIZE];
            for (int i = 0; i < num; i++)
            {
                buffer = reader.ReadBytes((int)BLOCK_SIZE);
                writer.Write(buffer, 0, (int)BLOCK_SIZE);
                string str = string.Format("{0}%", (100 * i) / num);
                if (str != listViewItem.SubItems[this.columnHeaderStatus.Index].Text)
                {
                    listViewItem.SubItems[this.columnHeaderStatus.Index].Text = str;
                    Application.DoEvents();
                }
            }
            buffer = reader.ReadBytes(count);
            writer.Write(buffer, 0, count);
            listViewItem.SubItems[this.columnHeaderStatus.Index].Text = "Padding...";
            Application.DoEvents();
            for (int j = 0; j < num3; j++)
            {
                writer.Write(0);
                Application.DoEvents();
            }
            listViewItem.SubItems[this.columnHeaderStatus.Index].Text = "Done";
            Application.DoEvents();
            reader.Close();
            stream2.Close();
            writer.Close();
            stream.Close();
        }

        // Nested Types
        public enum DriveTypes : uint
        {
            DRIVE_CDROM = 5,
            DRIVE_FIXED = 3,
            DRIVE_NO_ROOT_DIR = 1,
            DRIVE_RAMDISK = 6,
            DRIVE_REMOTE = 4,
            DRIVE_REMOVABLE = 2,
            DRIVE_UNKNOWN = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SPTDB
        {
            public short length;
            public byte ScsiStatus;
            public byte PathId;
            public byte TargetId;
            public byte Lun;
            public byte CdbLength;
            public byte SenseInfoLength;
            public byte DataIn;
            public int DataTransfertLength;
            public int TimeOutValue;
            public int DataBuffer;
            public int SenseInfoOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10, ArraySubType = UnmanagedType.AsAny)]
            public byte[] cdb;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.AsAny)]
            public byte[] Fill;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x23, ArraySubType = UnmanagedType.AsAny)]
            public byte[] SenseBuffer;
        }

        private enum SPTIDirection
        {
            SCSI_IOCTL_DATA_OUT,
            SCSI_IOCTL_DATA_IN,
            SCSI_IOCTL_DATA_UNSPECIFIED
        }
    }
}
