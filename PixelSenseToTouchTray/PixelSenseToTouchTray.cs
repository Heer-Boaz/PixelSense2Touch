using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelSenseToTouchLib;

namespace PixelSense2Touch {
	class PixelSense2TouchTray : ApplicationContext {
		private System.ComponentModel.IContainer components;
		private NotifyIcon notifyIcon;
		private ContextMenuStrip contextMenu;
		private ToolStripMenuItem stopMenuItem;
		private ToolStripMenuItem startMenuItem;
		private ToolStripMenuItem aboutMenuItem;
		private ToolStripMenuItem exitMenuItem;
		private PixelSenseToTouch pixelSenseToTouchProvider;

		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var oContext = new PixelSense2TouchTray();
			Application.Run(oContext);
		}

		public PixelSense2TouchTray() {
			this.components = new System.ComponentModel.Container();

			this.setupTrayIcon();
			this.setupTrayMenu();
			this.initPixelSenseToTouch();
		}

		private void setupTrayIcon() {
			this.notifyIcon = new NotifyIcon(this.components);
			this.notifyIcon.Icon = new System.Drawing.Icon("trayicon.ico");
			this.notifyIcon.Text = "PixelSense2Touch";
			this.notifyIcon.Visible = true;

			this.contextMenu = new ContextMenuStrip();
			this.stopMenuItem = new ToolStripMenuItem();
			this.startMenuItem = new ToolStripMenuItem();
			this.aboutMenuItem = new ToolStripMenuItem();
			this.exitMenuItem = new ToolStripMenuItem();

			this.notifyIcon.ContextMenuStrip = this.contextMenu;
		}

		private void initPixelSenseToTouch() {
			// Init PixelSenseToTouch provider
			this.pixelSenseToTouchProvider = new PixelSenseToTouch();
			this.pixelSenseToTouchProvider.Init();
		}

		private void setupTrayMenu() {
			this.startMenuItem.Text = "Start";
			this.startMenuItem.Click += new EventHandler(HandleStartRequest);
			this.startMenuItem.Enabled = false;
			this.contextMenu.Items.Add(this.startMenuItem);

			this.stopMenuItem.Text = "Stop";
			this.stopMenuItem.Click += new EventHandler(handleStopRequest);
			this.stopMenuItem.Enabled = true;
			this.contextMenu.Items.Add(this.stopMenuItem);

			this.aboutMenuItem.Text = "About";
			this.aboutMenuItem.Click += new EventHandler(HandleAboutRequest);
			this.contextMenu.Items.Add(this.aboutMenuItem);

			this.exitMenuItem.Text = "Exit";
			this.exitMenuItem.Click += new EventHandler(HandleExitRequest);
			this.contextMenu.Items.Add(this.exitMenuItem);
		}

		private void handleStopRequest(object sender, EventArgs e) {
			this.pixelSenseToTouchProvider.RemoveEventHandlers();
			this.stopMenuItem.Enabled = false;
			this.startMenuItem.Enabled = true;
		}

		private void HandleStartRequest(object sender, EventArgs e) {
			this.pixelSenseToTouchProvider.InitEventHandlers();
			this.startMenuItem.Enabled = false;
			this.stopMenuItem.Enabled = true;
		}

		private void HandleAboutRequest(object sender, EventArgs e) {
			// TODO: Remove hardcoded string here
			MessageBox.Show("Surface2Touch created by Boaz Pat-El (http://www.boazpatel.com).\nVersion: 1.0a (Aug 2017)\n\nSee https://github.com/Heer-Boaz/PixelSense2Touch for latest version and details.", "About Surface2Touch", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void HandleExitRequest(object sender, EventArgs e) {
			this.pixelSenseToTouchProvider.CleanUp();
			this.pixelSenseToTouchProvider = null; // Dispose the touch provider
			base.ExitThreadCore();
		}
	}
}
