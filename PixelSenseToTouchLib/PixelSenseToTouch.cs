using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Surface.Core;
using TCD.System.TouchInjection;
using Contact = Microsoft.Surface.Core.Contact;

namespace PixelSenseToTouchLib {
	public class PixelSenseToTouch : IDisposable {
		private ContactTarget contactTarget;
		private NativeWindow window;
		private List<PointerTouchInfo> registeredContacts;
		private bool singleTouch;

		public void Init(bool singleTouch = true) {
			this.registeredContacts = new List<PointerTouchInfo>();
			this.singleTouch = singleTouch;

			System.Diagnostics.Debug.Write($"{DateTime.Now}: Create native window with handle... ");
			this.window = new NativeWindow();
			this.window.CreateHandle(new CreateParams());
			System.Diagnostics.Debug.WriteLine($"[OK]");

			// Create a target for surface input
			System.Diagnostics.Debug.Write($"{DateTime.Now}: Create contact target... ");
			this.contactTarget = new ContactTarget(IntPtr.Zero, EventThreadChoice.OnBackgroundThread);
			this.contactTarget.EnableInput();
			System.Diagnostics.Debug.WriteLine($"[OK]");

			// Initialize the TouchInjector
			System.Diagnostics.Debug.Write($"{DateTime.Now}: Init touch injector... ");
			bool s = TouchInjector.InitializeTouchInjection();
			if (s) System.Diagnostics.Debug.WriteLine($"[OK]");
			else {
				System.Diagnostics.Debug.WriteLine($"[FAILED]");
				return;
			}

			// Setup event handlers
			this.InitEventHandlers();
		}

		public void InitEventHandlers() {
			System.Diagnostics.Debug.Write($"{DateTime.Now}: Setting up event handlers... ");
			this.contactTarget.ContactAdded += this.HandleContactAdded;
			this.contactTarget.ContactChanged += this.HandleContactChanged;
			this.contactTarget.ContactRemoved += this.HandleContactRemoved;
			System.Diagnostics.Debug.WriteLine($"[OK]");

			this.registeredContacts.Clear(); // Reset the registered contacts
		}

		public void RemoveEventHandlers() {
			System.Diagnostics.Debug.Write($"{DateTime.Now}: Removing event handlers... ");
			this.contactTarget.ContactAdded -= this.HandleContactAdded;
			this.contactTarget.ContactChanged -= this.HandleContactChanged;
			this.contactTarget.ContactRemoved -= this.HandleContactRemoved;
			System.Diagnostics.Debug.WriteLine($"[OK]");
		}

		private void HandleContactRemoved(object sender, ContactEventArgs e) {
			var contact = e.Contact;
			if (!contact.IsFingerRecognized) return; // Only handle finger contacts at the moment

			// Update the finger contact
			var contactToRemove = new PointerTouchInfo[1];
			contactToRemove[0] = MakePointerTouchInfo((int)contact.X, (int)contact.Y, 2, (uint)contact.Id % 255);
			contactToRemove[0].PointerInfo.PointerFlags = PointerFlags.UP;

			if (!this.singleTouch) {
				var registeredTouchToUpdate = this.registeredContacts.First(c => c.PointerInfo.PointerId == contact.Id % 255);
				registeredTouchToUpdate.PointerInfo.PointerFlags = PointerFlags.UP;

				this.registeredContacts.RemoveAll(c => c.PointerInfo.PointerId == contact.Id % 255);
				this.updateTouches();
			}
			else {
				bool success = TouchInjector.InjectTouchInput(1, contactToRemove);
			}
		}

		private void HandleContactChanged(object sender, ContactEventArgs e) {
			var contact = e.Contact;

			// Update the finger contact
			var contactToUpdate = new PointerTouchInfo[1];
			contactToUpdate[0] = MakePointerTouchInfo((int)contact.X, (int)contact.Y, 2, (uint)contact.Id % 255);
			contactToUpdate[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			if (!this.singleTouch) {
				this.registeredContacts.RemoveAll(c => c.PointerInfo.PointerId == contact.Id % 255);

				var registeredTouchToUpdateIndex = this.registeredContacts.FindIndex(c => c.PointerInfo.PointerId == contact.Id % 255);
				this.registeredContacts.Add(contactToUpdate[0]);
				this.updateTouches();
			}
			else {
				bool success = TouchInjector.InjectTouchInput(1, contactToUpdate);
			}
		}

		private void HandleContactAdded(object sender, ContactEventArgs e) {
			var contact = e.Contact;
			if (!contact.IsFingerRecognized) return; // Only handle finger contacts at the moment

			// Insert the finger contact
			var contactToInsert = new PointerTouchInfo[1];
			contactToInsert[0] = MakePointerTouchInfo((int)contact.X, (int)contact.Y, 2, (uint)contact.Id % 255);

			if (!this.singleTouch) {
				this.registeredContacts.Add(contactToInsert[0]);
				this.updateTouches();
				contactToInsert[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			}
			else {
				bool success = TouchInjector.InjectTouchInput(1, contactToInsert);
			}
		}

		private void updateTouches(object sender = null, EventArgs e = null) {
			bool success = TouchInjector.InjectTouchInput(this.registeredContacts.Count, this.registeredContacts.ToArray());
		}

		/// <summary>
		/// From TouchInjection example (see http://blog.mosthege.net/2012/08/19/windows-8-touchinjection-with-c/)
		/// </summary>
		private PointerTouchInfo MakePointerTouchInfo(int x, int y, int radius, uint id, uint orientation = 90, uint pressure = 32000) {
			PointerTouchInfo contact = new PointerTouchInfo();
			contact.PointerInfo.pointerType = PointerInputType.TOUCH;
			contact.TouchFlags = TouchFlags.NONE;
			contact.Orientation = orientation;
			contact.Pressure = pressure;
			contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
			contact.PointerInfo.PtPixelLocation.X = x;
			contact.PointerInfo.PtPixelLocation.Y = y;
			contact.PointerInfo.PointerId = id;
			contact.ContactArea.left = x - radius;
			contact.ContactArea.right = x + radius;
			contact.ContactArea.top = y - radius;
			contact.ContactArea.bottom = y + radius;
			return contact;
		}

		public void CleanUp() {
			System.Diagnostics.Debug.Write($"{DateTime.Now}: Start disposing resources... ");
			this.contactTarget?.Dispose();
			System.Diagnostics.Debug.Write($"ContactTarget;");

			this.window?.DestroyHandle();
			System.Diagnostics.Debug.Write($"Window;");
		}

		void IDisposable.Dispose() {
			this.CleanUp();
		}
	}
}
