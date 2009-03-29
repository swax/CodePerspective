using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace XLibrary
{
    public class Win32
    {
        public const uint DIB_RGB_COLORS = 0;
        public const uint SRCCOPY = 0x00CC0020;
        public const uint BI_RGB = 0;
        public const int COLORONCOLOR = 3;
         

        public enum Bool
        {
            False = 0,
            True
        };

        public enum TernaryRasterOperations
        {
            SRCCOPY = 0x00CC0020, /* dest = source                   */
            SRCPAINT = 0x00EE0086, /* dest = source OR dest           */
            SRCAND = 0x008800C6, /* dest = source AND dest          */
            SRCINVERT = 0x00660046, /* dest = source XOR dest          */
            SRCERASE = 0x00440328, /* dest = source AND (NOT dest )   */
            NOTSRCCOPY = 0x00330008, /* dest = (NOT source)             */
            NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
            MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)     */
            MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest     */
            PATCOPY = 0x00F00021, /* dest = pattern                  */
            PATPAINT = 0x00FB0A09, /* dest = DPSnoo                   */
            PATINVERT = 0x005A0049, /* dest = pattern XOR dest         */
            DSTINVERT = 0x00550009, /* dest = (NOT dest)               */
            BLACKNESS = 0x00000042, /* dest = BLACK                    */
            WHITENESS = 0x00FF0062, /* dest = WHITE                    */
        };

        public enum PenStyles
        {
            PS_SOLID = 0,
            PS_DASH = 1,
            PS_DOT = 2,
            PS_DASHDOT = 3,
            PS_DASHDOTDOT = 4,
            PS_NULL = 5,
            PS_INSIDEFRAME = 6,
            PS_USERSTYLE = 7,
            PS_ALTERNATE = 8,
            PS_STYLE_MASK = 0x0000000F,
            PS_ENDCAP_ROUND = 0x00000000,
            PS_ENDCAP_SQUARE = 0x00000100,
            PS_ENDCAP_FLAT = 0x00000200,
            PS_ENDCAP_MASK = 0x00000F00,
            PS_JOIN_ROUND = 0x00000000,
            PS_JOIN_BEVEL = 0x00001000,
            PS_JOIN_MITER = 0x00002000,
            PS_JOIN_MASK = 0x0000F000,
            PS_COSMETIC = 0x00000000,
            PS_GEOMETRIC = 0x00010000,
            PS_TYPE_MASK = 0x000F0000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public UInt32 biSize;
            public Int32 biWidth;
            public Int32 biHeight;
            public Int16 biPlanes;
            public Int16 biBitCount;
            public UInt32 biCompression;
            public UInt32 biSizeImage;
            public Int32 biXPelsPerMeter;
            public Int32 biYPelsPerMeter;
            public UInt32 biClrUsed;
            public UInt32 biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER Header;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] Colors;
        }

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern Bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern Bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hObject, int width, int height);

        [DllImport("gdi32.dll")]
        public static extern bool LineTo(
            IntPtr hdc,    // device context handle
            int nXEnd,  // x-coordinate of line's ending point
            int nYEnd   // y-coordinate of line's ending point
            );


        [DllImport("user32.dll")]
        public static extern int FillRect(
          IntPtr hDC,           // handle to DC
          ref RECT lprc,  // rectangle
          IntPtr hbr         // handle to brush
        );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(
            PenStyles fnPenStyle,    // pen style
            int nWidth,        // pen width
            int crColor   // pen color
            );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(
            int crColor   // brush color value
            );

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern Bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
   
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int SetDIBitsToDevice(
              IntPtr hdc,                 // handle to DC
              int XDest,               // x-coord of destination upper-left corner
              int YDest,               // y-coord of destination upper-left corner 
              uint dwWidth,           // source rectangle width
              uint dwHeight,          // source rectangle height
              int XSrc,                // x-coord of source lower-left corner
              int YSrc,                // y-coord of source lower-left corner
              uint uStartScan,         // first scan line in array
              uint cScanLines,         // number of scan lines
              IntPtr lpvBits,           // array of DIB bits CONST VOID *lpvBits
              IntPtr lpbmi,             // bitmap information CONST BITMAPINFO *lpbmi
              uint fuColorUse          // RGB or palette indexes
            );

        [DllImportAttribute("gdi32.dll")]
        public static extern bool MoveToEx(
            IntPtr hdc,          // handle to device context
            int X,            // x-coordinate of new current position
            int Y,            // y-coordinate of new current position
            int oldp// pointer to old current position
            );


        static public uint MAKERGB(int r, int g, int b)
        {
            return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
        }

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
         public static  extern int StretchDIBits(
              IntPtr hdc,                      // handle to DC
              int XDest,                    // x-coord of destination upper-left corner
              int YDest,                    // y-coord of destination upper-left corner
              int nDestWidth,               // width of destination rectangle
              int nDestHeight,              // height of destination rectangle
              int XSrc,                     // x-coord of source upper-left corner
              int YSrc,                     // y-coord of source upper-left corner
              int nSrcWidth,                // width of source rectangle
              int nSrcHeight,               // height of source rectangle
              IntPtr lpBits,           // bitmap bits CONST VOID *lpBits
              ref BITMAPINFO lpBitsInfo, // bitmap data  CONST BITMAPINFO *lpBitsInfo
              uint iUsage,                  // usage options
              uint dwRop                   // raster operation code
            );

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            Int32 dwRop);
        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(
          IntPtr hdc,           // handle to DC
          int iStretchMode   // bitmap stretching mode
        );

        [DllImport("gdi32.dll")]
        public static extern bool Arc(
          IntPtr hdc,         // handle to device context
          int nLeftRect,   // x-coord of rectangle's upper-left corner
          int nTopRect,    // y-coord of rectangle's upper-left corner
          int nRightRect,  // x-coord of rectangle's lower-right corner
          int nBottomRect, // y-coord of rectangle's lower-right corner
          int nXStartArc,  // x-coord of first radial ending point
          int nYStartArc,  // y-coord of first radial ending point
          int nXEndArc,    // x-coord of second radial ending point
          int nYEndArc     // y-coord of second radial ending point
        );

        [DllImport("gdi32.dll")]
        public static extern bool Ellipse(
          IntPtr hdc,        // handle to DC
          int nLeftRect,  // x-coord of upper-left corner of rectangle
          int nTopRect,   // y-coord of upper-left corner of rectangle
          int nRightRect, // x-coord of lower-right corner of rectangle
          int nBottomRect // y-coord of lower-right corner of rectangle
        );

        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(
              IntPtr hDC,
              int xLeft, int yTop,
              int xRight, int yBottom);  
    }
}
