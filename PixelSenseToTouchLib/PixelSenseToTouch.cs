using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InputSimulatorStandard;
using Microsoft.Surface.Core;
using TCD.System.TouchInjection;

namespace PixelSenseToTouchLib
{
    public class PixelSenseToTouch : IDisposable
    {
        public ContactTarget ContactTarget { get; set; }
        private const int NumberOfSimultaniousTouches = 20;

        private NativeWindow window;
#if DEBUG
        public StringBuilder debuginfo;
#endif
        private InputSimulator inputSimulator;
        private PointerTouchInfo touch = new PointerTouchInfo();
        private Dictionary<uint, PointerTouchInfo> registeredTouches;
        private HashSet<uint> injectedIds;
        private readonly object touchLock = new object();

        public void Init()
        {
            this.registeredTouches = new Dictionary<uint, PointerTouchInfo>();
            this.injectedIds = new HashSet<uint>();
#if DEBUG
            this.debuginfo = new StringBuilder();
#endif
            this.touch.PointerInfo.pointerType = PointerInputType.TOUCH;
            this.touch.Orientation = 90;
            this.touch.Pressure = 32000;
            this.touch.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;

            System.Diagnostics.Debug.Write($"{DateTime.Now}: Create native window with handle... ");
            this.window = new NativeWindow();
            this.window.CreateHandle(new CreateParams());
            System.Diagnostics.Debug.WriteLine($"[OK]");

            // Create a target for surface input
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Create contact target... ");
            this.ContactTarget = new ContactTarget(IntPtr.Zero, EventThreadChoice.OnBackgroundThread);
            this.ContactTarget.EnableInput();
            System.Diagnostics.Debug.WriteLine($"[OK]");

            // Init the InputSimulator
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Init input simulaOr... ");
            this.inputSimulator = new InputSimulator();
            System.Diagnostics.Debug.WriteLine($"[OK]");

            // Initialize the TouchInjector
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Init touch injector... ");
            bool s = TouchInjector.InitializeTouchInjection(NumberOfSimultaniousTouches, TouchFeedback.DEFAULT);
            if (s) System.Diagnostics.Debug.WriteLine($"[OK]");
            else
            {
                System.Diagnostics.Debug.WriteLine($"[FAILED]");
                return;
            }

            // Setup event handlers
            this.InitEventHandlers();
        }

        public void InitEventHandlers()
        {
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Setting up event handlers... ");
            this.ContactTarget.ContactAdded += this.HandleAdd;
            this.ContactTarget.ContactChanged += this.HandleChange;
            this.ContactTarget.ContactRemoved += this.HandleRemove;
            this.ContactTarget.ContactTapGesture += this.HandleTap;
            this.ContactTarget.ContactHoldGesture += this.HandleHold;

            System.Diagnostics.Debug.WriteLine($"[OK]");
        }

        public void RemoveEventHandlers()
        {
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Removing event handlers... ");
            this.ContactTarget.ContactAdded -= this.HandleAdd;
            this.ContactTarget.ContactChanged -= this.HandleChange;
            this.ContactTarget.ContactRemoved -= this.HandleRemove;
            this.ContactTarget.ContactTapGesture -= this.HandleTap;
            this.ContactTarget.ContactHoldGesture -= this.HandleHold;
            System.Diagnostics.Debug.WriteLine($"[OK]");
        }

        private void TransformToSimulatorCoords(ref float x, ref float y)
        {
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;
            x = (65535.0f * (x / w)) + 0.5f;
            y = (65535.0f * (y / h)) + 0.5f;
        }

        private void UpdateTouches(uint touchIdFromLatestEvent)
        {
            lock (touchLock)
            {
                var arr = this.registeredTouches.Values.ToArray();
                // For each registered touch, check whether we already injected the touch.
                // If not, we flag that the touch has been injected.
                // If yes, we make sure that the point flag is set to UPDATE instead of NEW
                
                // Why do we do this? We want to leverage the functionality provided by the Surface SDK 1.0 for
                // raising events on tap and hold. We don't want to inject any touch events when a user
                // touches the screen with one finger and triggers the ContactAdded-event.
                // However, we *do* need to remember that a touch event occurred, for the situation
                // that a user touches the screen with an additional finger. In that situation, when the
                // user would then initiate a pinch event, that event would fail if we don't remember
                // the first finger and inject that event anyway.
                // What happens if we don't use this approach? the user *can* pinch, but only if the first
                // finger moves around a little bit to trigger a ContactChange event, before touching the screen
                // with another finger, wait and then initiate the pinch movement.
                for (var i = 0; i < arr.Length; i++)
                {
                    var id = arr[i].PointerInfo.PointerId;
                    if (!this.injectedIds.Contains(id))
                    {
                        this.injectedIds.Add(id);
                    }
                    else
                    {
                        arr[i].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                    }
                    if (id == touchIdFromLatestEvent) continue;
                }
                TouchInjector.InjectTouchInput(this.registeredTouches.Count, arr);
            }
        }

        private uint SetPointerInfo(Contact contact)
        {
            var x = contact.X;
            var y = contact.Y;
            var id = (uint)(contact.Id % NumberOfSimultaniousTouches);

            this.touch.PointerInfo.PointerId = id;

            this.touch.PointerInfo.PtPixelLocation.X = (int)x;
            this.touch.PointerInfo.PtPixelLocation.Y = (int)y;
            this.touch.ContactArea.left = (int)contact.Bounds.Left;
            this.touch.ContactArea.right = (int)contact.Bounds.Right;
            this.touch.ContactArea.top = (int)contact.Bounds.Top;
            this.touch.ContactArea.bottom = (int)contact.Bounds.Bottom;

            this.touch.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            System.Diagnostics.Debug.WriteLine($"New! {id}, {contact.CenterX},{contact.CenterY}\n");

            if (this.registeredTouches.ContainsKey(id))
            {
                this.registeredTouches.Remove(id);
                this.registeredTouches.Add(id, this.touch);
            }
            else
            {
                this.registeredTouches.Add(id, this.touch);
            }

            return id;
        }

        private void HandleAdd(object sender, ContactEventArgs e)
        {
            var contact = e.Contact;
            if (!contact.IsFingerRecognized) return;

            this.SetPointerInfo(contact);

            // Do *not* update touches on add! We only store the info for when another touch is detected! Then we might need to support pinching and stuff
        }

        private void HandleChange(object sender, ContactEventArgs e)
        {
            var contact = e.Contact;
            if (!contact.IsFingerRecognized) return;

            var id = this.SetPointerInfo(contact);

            this.UpdateTouches(id);
        }

        private void HandleRemove(object sender, ContactEventArgs e)
        {
            var contact = e.Contact;
            if (!e.Contact.IsFingerRecognized) return;

            var x = contact.X;
            var y = contact.Y;
            var id = (uint)(contact.Id % NumberOfSimultaniousTouches);

            this.touch.PointerInfo.PointerFlags = PointerFlags.UP;
            this.touch.PointerInfo.PointerId = id;

            this.touch.PointerInfo.PtPixelLocation.X = (int)x;
            this.touch.PointerInfo.PtPixelLocation.Y = (int)y;
            if (this.registeredTouches.ContainsKey(id)) this.registeredTouches.Remove(id);
            if (this.injectedIds.Contains(id)) this.injectedIds.Remove(id);
#if DEBUG
            this.debuginfo.Append($"Weg! {id}, {contact.CenterX},{contact.CenterY}\n");
#endif

            this.UpdateTouches(id);
        }

        private void HandleTap(object sender, ContactEventArgs e)
        {
            var contact = e.Contact;

            var x = contact.CenterX;
            var y = contact.CenterY;
            var id = (uint)(contact.Id % NumberOfSimultaniousTouches);
            this.TransformToSimulatorCoords(ref x, ref y);

            if (this.registeredTouches.ContainsKey(id)) this.registeredTouches.Remove(id);
            if (this.injectedIds.Contains(id)) this.injectedIds.Remove(id);

            this.inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(x, y).LeftButtonDown().Sleep(50).LeftButtonUp();
        }

        private void HandleHold(object sender, ContactEventArgs e)
        {
            var contact = e.Contact;

            var x = contact.CenterX;
            var y = contact.CenterY;
            var id = (uint)(contact.Id % NumberOfSimultaniousTouches);
            this.TransformToSimulatorCoords(ref x, ref y);

            if (this.registeredTouches.ContainsKey(id)) this.registeredTouches.Remove(id);
            if (this.injectedIds.Contains(id)) this.injectedIds.Remove(id);

            this.inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(x, y).RightButtonClick();
        }

        public void CleanUp()
        {
            System.Diagnostics.Debug.Write($"{DateTime.Now}: Start disposing resources... ");
            this.ContactTarget?.Dispose();
            System.Diagnostics.Debug.Write($"ContactTarget;");

            this.window?.DestroyHandle();
            System.Diagnostics.Debug.Write($"Window;");
        }

        void IDisposable.Dispose()
        {
            this.CleanUp();
        }
    }
}
