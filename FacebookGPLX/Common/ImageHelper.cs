using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Media.Images;
namespace FacebookGPLX.Common
{
  public static class ImageHelper
  {
    public static Bitmap DrawGPLX(this Bitmap bitMap,string Name,string DateOfBirth)
    {
      Bitmap result = new Bitmap(Properties.Resources.GPLX);
      using (Font font = new Font("Tahoma",10))
      {
        using (Graphics graphics = Graphics.FromImage(result))
        {
          graphics.DrawImage(bitMap, 8, 76, 142, 175);//149 250
          graphics.DrawText(new Point(257, 103), Name, font, Color.Black, 255);
          graphics.DrawText(new Point(285, 125), DateOfBirth, font, Color.Black, 255);
        }
      }
      return result;
    }
  }
}
