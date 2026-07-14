using System.Drawing.Drawing2D;

namespace NotificadorVendasSoftware;

public class SmoothPictureBox : PictureBox
{
    protected override void OnPaint(PaintEventArgs pe)
    {
        if (Image == null)
        {
            base.OnPaint(pe);
            return;
        }

        pe.Graphics.Clear(this.BackColor);

        pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        pe.Graphics.CompositingQuality = CompositingQuality.HighQuality;

        Rectangle destino = new Rectangle(0, 0, Width, Height);

        pe.Graphics.DrawImage(Image, destino);
    }
}