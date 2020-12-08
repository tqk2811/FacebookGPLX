using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Media.Images;
namespace FacebookGPLX.Common
{
  public static class ImageHelper
  {
    static readonly Random random = new Random();
    public static Bitmap DrawGPLX(this Bitmap bitMap,string Name,string DateOfBirth)
    {
      var files = Directory.GetFiles(Extensions.ExeFolderPath + "\\Embryo", "*.png");
      if (files.Length == 0) throw new Exception("Không tìm thấy phôi");

      Bitmap result = (Bitmap)Bitmap.FromFile(files[random.Next(files.Length)]);
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
