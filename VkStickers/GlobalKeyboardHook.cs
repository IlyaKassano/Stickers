﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using static VkStickers.WinApi;

namespace VkStickers
{
    class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public KeyboardState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public GlobalKeyboardHookEventArgs(
            LowLevelKeyboardInputEvent keyboardData,
            KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }

    //Based on https://gist.github.com/Stasonix
    class GlobalKeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

        public GlobalKeyboardHook()
        {
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            _user32LibraryHandle = LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }



            _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
            if (_windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _hookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;

        public const int WH_KEYBOARD_LL = 13;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

                EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                handler?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
