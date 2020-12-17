using PInvoke;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookGPLX.Common
{
  public static class ChromeLocationHelper
  {
    private static int base_TitleAndTaskbar = 30;
    private static RECT RECT;
    private static int window_width = 0;
    private static int window_height = 0;

    public static void Init()
    {
      IntPtr desktop = User32.GetDesktopWindow();
      User32.GetWindowRect(desktop, out RECT);
      window_height = (RECT.bottom - base_TitleAndTaskbar) / 2;
      window_width = RECT.right / 2;
    }

    public static Rectangle GetPos(int index)
    {
      if (index < 4)
      {
        Rectangle rectangle = new Rectangle();
        rectangle.X = index % 2 * window_width;
        rectangle.Y = index / 2 * window_height;
        rectangle.Width = window_width;
        rectangle.Height = window_height;
        return rectangle;
      }
      else return new Rectangle() { Width = 800, Height = 500 };
    }
  }
}