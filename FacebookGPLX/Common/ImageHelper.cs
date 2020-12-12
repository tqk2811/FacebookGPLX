using System;
using System.Drawing;
using System.IO;
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
      using (Font font = new Font("Arial", 14, FontStyle.Bold))
      {
        using (Graphics graphics = Graphics.FromImage(result))
        {
          graphics.DrawImage(bitMap, 8, 76, 142, 175);//149 250
          DrawCenterWidthPoint(graphics, new Point(365, 103), font, Name);
          DrawCenterWidthPoint(graphics, new Point(365, 125), font, DateOfBirth);
          //graphics.DrawText(new Point(297, 103), Name, font, Color.Black, 255);
          //graphics.DrawText(new Point(325, 125), DateOfBirth, font, Color.Black, 255);
        }
      }
      return result;
    }

    static void DrawCenterWidthPoint(Graphics graphics,Point point_center,Font font,string text)
    {
      SizeF textSize = graphics.MeasureString(text, font);
      graphics.DrawText(new Point(point_center.X - (int)textSize.Width / 2, point_center.Y), text, font, Color.Black, 255);
    }
  }
}
