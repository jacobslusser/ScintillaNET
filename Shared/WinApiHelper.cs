 using System;
 using System.Runtime.InteropServices;

 namespace ScintillaNET; 

 internal static class WinApiHelpers
 {
     internal static IntPtr SetWindowLongPtr(this IntPtr hWnd, int nIndex, IntPtr dwNewLong)
     {
         if (Environment.Is64BitProcess)
         {
             return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
         }

         return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
     }

     internal static IntPtr GetWindowLongPtr(this IntPtr hWnd, int nIndex)
     {
         return GetWindowLong(hWnd, nIndex);
     }

     [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
     private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

     [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
     private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

     [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
     private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

     internal const long WS_EX_LAYOUTRTL = 0x00400000L;
     internal const int GWL_EXSTYLE = -20;
 }