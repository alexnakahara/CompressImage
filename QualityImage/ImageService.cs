using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace QualityImage
{
    public class ImageService
    {
        public ImageService()
        {
            quality = 20L;
        }
        public long quality { get; set; }

        public void CoreCompact(Stream stream, string path)
        {
            var filename = Path.Combine(path, "coredrawing.jpg");
            if (File.Exists(filename))
                File.Delete(filename);


            using var image = new Bitmap(System.Drawing.Image.FromStream(stream));
            using var graphics = Graphics.FromImage(image);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(image, 0, 0);
            graphics.Flush(FlushIntention.Sync);

            using var ms = new FileStream(filename, FileMode.CreateNew);
            var qualityParamId = Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
            var codec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(codecx => codecx.FormatID == ImageFormat.Jpeg.Guid);
            image.Save(ms, codec, encoderParameters);
        }

        public void MagicCompact(Stream stream, string path)
        {
            var filename = Path.Combine(path, "magicscaler.jpg");
            if (File.Exists(filename))
                File.Delete(filename);

            //using var image = new Bitmap(System.Drawing.Image.FromStream(stream));

            var settings = new PhotoSauce.MagicScaler.ProcessImageSettings()
            {
                //ResizeMode = PhotoSauce.MagicScaler.CropScaleMode.Max,
                SaveFormat = PhotoSauce.MagicScaler.FileFormat.Jpeg,
                JpegQuality = (int)quality,
                JpegSubsampleMode = PhotoSauce.MagicScaler.ChromaSubsampleMode.Subsample420
                //Interpolation = PhotoSauce.MagicScaler.InterpolationSettings.Cubic
            };

            using var ms = new FileStream(filename, FileMode.CreateNew);

            PhotoSauce.MagicScaler.MagicImageProcessor.ProcessImage(stream, ms, settings);
        }

        public void SharpCompactJPG(Stream stream, string path)
        {
            var filename = Path.Combine(path, "sharper.jpg");
            if (File.Exists(filename))
                File.Delete(filename);

            using var ms = new FileStream(filename, FileMode.CreateNew);

            var image = SixLabors.ImageSharp.Image.Load(stream);
            image.Metadata.ExifProfile = null;
            image.SaveAsJpeg(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
            {
                Quality = (int)quality,
                Subsample = SixLabors.ImageSharp.Formats.Jpeg.JpegSubsample.Ratio420
            });
        }

        public void SharpCompactPNG(Stream stream, string path)
        {
            var filename = Path.Combine(path, "sharper.png");
            if (File.Exists(filename))
                File.Delete(filename);

            using var ms = new FileStream(filename, FileMode.CreateNew);

            var image = SixLabors.ImageSharp.Image.Load(stream);
            image.Metadata.ExifProfile = null;

            image.SaveAsPng(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder()
            {
                CompressionLevel = (int)quality / 10,
                ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,
            });
        }
    }
}
