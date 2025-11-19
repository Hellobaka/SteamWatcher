using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class Painting : IDisposable
    {
        public Painting(int width, int height)
        {
            Width = width;
            Height = height;
            MainSurface = SKSurface.Create(new SKImageInfo(width, height));
            MainCanvas.Clear(SKColors.White);

            CustomFont = CreateCustomFont();
        }

        private static SKTypeface CreateCustomFont()
        {
            // 路径 > 名称
            if (!string.IsNullOrEmpty(AppConfig.CustomFontPath))
            {
                // 自定义路径不支持粗体
                return SKTypeface.FromFile(AppConfig.CustomFontPath);
            }

            return !string.IsNullOrEmpty(AppConfig.CustomFont)
                ? SKTypeface.FromFamilyName(AppConfig.CustomFont) ?? SKTypeface.Default
                : SKTypeface.Default;
        }

        public static SKRect Anywhere { get; set; } = new SKRect { Right = int.MaxValue, Bottom = int.MaxValue };

        public enum TextAlign
        {
            Left,
            Center,
            Right
        }

        public float Height { get; set; }

        public float Width { get; set; }

        private static SKPaint AntialiasPaint { get; set; } = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
        };

        private static SKFontManager FontManager { get; set; } = SKFontManager.CreateDefault();

        private SKTypeface CustomFont { get; set; }

        private bool Disposing { get; set; }

        private SKCanvas MainCanvas => MainSurface.Canvas;

        private SKSurface MainSurface { get; set; }

        public void Clear(SKColor color)
        {
            MainCanvas.Clear(color);
        }

        public SKBitmap ConvertToBitmap(SKImage image)
        {
            return SKBitmap.FromImage(image);
        }

        /// <summary>
        /// 裁切图片为圆形
        /// </summary>
        /// <param name="image">欲进行裁切的图片</param>
        /// <param name="width">圆形直径</param>
        /// <returns></returns>
        public SKImage CreateCircularImage(SKImage image, float width)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            var imageInfo = new SKImageInfo((int)width, (int)width);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            using var path = new SKPath();
            path.AddCircle(width / 2, width / 2, width / 2, SKPathDirection.CounterClockwise);
            canvas.ClipPath(path, SKClipOperation.Intersect, true);

            var srcRect = new SKRect(0, 0, image.Width, image.Height);
            var destRect = new SKRect(0, 0, width, width);

            canvas.DrawImage(image, srcRect, destRect, AntialiasPaint);
            canvas.Flush();
            return surface.Snapshot();
        }

        public SKImage CreateCircularImage(string imagePath, float width)
        {
            return CreateCircularImage(LoadImage(imagePath), width);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 在指定位置绘制指定大小的图片
        /// </summary>
        /// <param name="imagePath">欲绘制图片的文件路径</param>
        /// <param name="rect">目标尺寸、大小</param>
        public void DrawImage(string imagePath, SKRect rect)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }
            using var imageStream = File.OpenRead(imagePath);
            using var codec = SKCodec.Create(imageStream);
            var bitmap = SKBitmap.Decode(codec);
            var image = SKImage.FromBitmap(bitmap);

            DrawImage(image, rect);
        }

        /// <summary>
        /// 在指定位置绘制指定大小的图片
        /// </summary>
        /// <param name="imagePath">欲绘制图片的文件路径</param>
        /// <param name="rect">目标位置、大小</param>
        public void DrawImage(SKImage image, SKRect rect)
        {
            MainCanvas.DrawImage(image, rect, AntialiasPaint);
        }

        public void DrawRectangle(SKRect rect, SKColor fillColor, SKColor strokeColor, float strokeWidth)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = fillColor
            };

            MainCanvas.DrawRect(rect, paint);

            if (strokeWidth == 0)
            {
                return;
            }
            using var strokePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                StrokeWidth = strokeWidth
            };
            MainCanvas.DrawRect(rect, strokePaint);
        }

        /// <summary>
        /// 相对区域坐标绘制
        /// </summary>
        /// <param name="text">欲绘制的文本</param>
        /// <param name="area">可绘制的区域</param>
        /// <param name="startPoint">起始相对坐标</param>
        /// <param name="color">文本颜色</param>
        /// <param name="customFont">自定义字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="isBold">是否粗体</param>
        /// <param name="wrap">是否自动换行</param>
        /// <param name="align">文本对齐方式</param>
        /// <returns>最后一个字符的右下角坐标</returns>
        public SKPoint DrawRelativeText(string text, SKRect area, SKPoint startPoint, SKColor color, float fontSize = 26, SKTypeface customFont = null, bool isBold = false, bool wrap = false, TextAlign align = TextAlign.Left)
        {
            return DrawText(text, area, new SKPoint { X = startPoint.X + area.Left, Y = startPoint.Y + area.Top }, color, fontSize, customFont, isBold, wrap, align);
        }

        /// <summary>
        /// 绘制文本，换行，自适应Fallback字体
        /// </summary>
        /// <param name="text">欲绘制的文本</param>
        /// <param name="area">可绘制的区域</param>
        /// <param name="startPoint">起始绝对坐标</param>
        /// <param name="color">文本颜色</param>
        /// <param name="customFont">自定义字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="align">文本对齐方式</param>
        /// <returns>最后一个字符的右下角坐标</returns>
        public SKPoint DrawText(string text, SKRect area, SKPoint startPoint, SKColor color, float fontSize = 26, SKTypeface customFont = null, bool isBold = false, bool wrap = false, TextAlign align = TextAlign.Left)
        {
            SKTypeface GetTypeface(SKTypeface baseFont, bool bold)
            {
                return baseFont != null && baseFont.IsBold == bold
                    ? baseFont
                    : SKTypeface.FromFamilyName(baseFont.FamilyName, bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            }

            var textElementEnumerator = StringInfo.GetTextElementEnumerator(text);
            float currentX = startPoint.X;
            float currentY = startPoint.Y + fontSize;
            float lineHeight = fontSize + 3;

            // 存储每行的信息用于对齐
            List<float> lineWidths = new List<float>();
            List<List<(string text, SKPaint paint, SKShaper shaper, float width)>> lineElements = new List<List<(string, SKPaint, SKShaper, float)>>();
            List<(string text, SKPaint paint, SKShaper shaper, float width)> currentLineElements = new List<(string, SKPaint, SKShaper, float)>();
            float currentLineWidth = 0;

            SKTypeface typeface = CustomFont != null ? GetTypeface(CustomFont, isBold) : null;

            // 单次遍历处理所有文本元素
            while (textElementEnumerator.MoveNext())
            {
                string textElement = textElementEnumerator.GetTextElement();

                // 处理换行
                if (textElement.Contains("\n"))
                {
                    if (currentLineElements.Count > 0)
                    {
                        lineWidths.Add(currentLineWidth);
                        lineElements.Add(currentLineElements);
                        currentLineElements = new List<(string, SKPaint, SKShaper, float)>();
                        currentLineWidth = 0;
                    }
                    continue;
                }

                // 字体处理
                var elementEnumerator = StringInfo.GetTextElementEnumerator(textElement);
                elementEnumerator.MoveNext();
                int codepoint = char.ConvertToUtf32(textElement, elementEnumerator.ElementIndex);

                if (customFont != null && customFont.ContainsGlyph(codepoint))
                {
                    typeface = customFont;
                }
                else if (typeface == null || !typeface.ContainsGlyph(codepoint))
                {
                    typeface = FontManager.MatchCharacter(codepoint);
                    if (typeface == null)
                    {
                        continue;
                    }
                }

                // 创建绘制对象
                var paint = new SKPaint
                {
                    Typeface = typeface,
                    TextSize = fontSize,
                    Color = color,
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    SubpixelText = true,
                    LcdRenderText = true,
                };

                var shaper = new SKShaper(typeface);
                var shapedText = shaper.Shape(textElement, paint);

                // 检查是否需要换行
                if (wrap && currentLineWidth + shapedText.Width > area.Right - area.Left)
                {
                    if (currentLineElements.Count > 0)
                    {
                        lineWidths.Add(currentLineWidth);
                        lineElements.Add(currentLineElements);
                        currentLineElements = new List<(string, SKPaint, SKShaper, float)>();
                        currentLineWidth = 0;
                    }
                }

                // 添加到当前行
                currentLineElements.Add((textElement, paint, shaper, shapedText.Width));
                currentLineWidth += shapedText.Width;
            }

            // 添加最后一行
            if (currentLineElements.Count > 0)
            {
                lineWidths.Add(currentLineWidth);
                lineElements.Add(currentLineElements);
            }

            // 绘制文本
            for (int lineIndex = 0; lineIndex < lineElements.Count; lineIndex++)
            {
                var line = lineElements[lineIndex];
                float lineWidth = lineWidths[lineIndex];

                // 计算对齐偏移
                float alignOffset = 0;
                if (align != TextAlign.Left)
                {
                    float availableWidth = area.Right - area.Left;
                    switch (align)
                    {
                        case TextAlign.Center:
                            alignOffset = (availableWidth - lineWidth) / 2;
                            break;
                        case TextAlign.Right:
                            alignOffset = availableWidth - lineWidth;
                            break;
                    }
                }

                currentX = area.Left + alignOffset;

                // 绘制当前行的所有元素
                foreach (var (textElement, paint, shaper, width) in line)
                {
                    // 检查边界
                    if (area.Bottom != 0 && currentY > area.Bottom)
                    {
                        break;
                    }

                    MainCanvas.DrawShapedText(textElement, currentX, currentY, paint);
                    currentX += width;
                }

                currentX = area.Left;
                currentY += lineHeight;

                // 检查是否超出底部边界
                if (area.Bottom != 0 && currentY > area.Bottom)
                {
                    break;
                }
            }

            return new SKPoint(currentX, currentY);
        }

        public SKImage LoadImage(string imagePath)
        {
            using var imageStream = File.OpenRead(imagePath);
            using var codec = SKCodec.Create(imageStream);
            var bitmap = SKBitmap.Decode(codec);
            return SKImage.FromBitmap(bitmap);
        }

        public SKImage LoadImageFromBuffer(byte[] buffer)
        {
            using var imageStream = new MemoryStream(buffer);
            using var codec = SKCodec.Create(imageStream);
            var bitmap = SKBitmap.Decode(codec);
            return SKImage.FromBitmap(bitmap);
        }

        public SKSize MeasureString(string text, float fontSize)
        {
            SKTypeface typeface;
            if (CustomFont != null && CustomFont.ContainsGlyphs(text))
            {
                typeface = CustomFont;
            }
            else
            {
                typeface = FontManager.MatchCharacter(text.First());
                if (typeface == null)
                {
                    return new();
                }
            }
            var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High,
            };
            var shaper = new SKShaper(typeface);
            var shapedText = shaper.Shape(text, paint);
            var metrics = paint.FontMetrics;
            return new SKSize { Width = shapedText.Width, Height = metrics.Descent - metrics.Ascent };
        }

        public void Padding(int top, int left, int right, int bottom, SKColor color)
        {
            var newSurface = SKSurface.Create(new SKImageInfo((int)(Width + left + right), (int)(Height + top + bottom)));
            SKCanvas canvas = newSurface.Canvas;
            canvas.Clear(color);

            MainSurface.Draw(canvas, top, left, AntialiasPaint);
            MainCanvas.Dispose();
            MainSurface.Dispose();

            Width += left + right;
            Height += top + bottom;

            MainSurface = newSurface;
        }

        public void Resize(int width, int height)
        {
            var newSurface = SKSurface.Create(new SKImageInfo(width, height));
            SKCanvas canvas = newSurface.Canvas;
            canvas.Clear(SKColors.White);

            MainSurface.Draw(canvas, 0, 0, AntialiasPaint);
            MainCanvas.Dispose();
            MainSurface.Dispose();

            Width = width;
            Height = height;

            MainSurface = newSurface;
        }

        public SKImage ResizeImage(SKImage image, int newWidth, int newHeight)
        {
            var scaledBitmap = new SKBitmap(newWidth, newHeight);

            using (var canvas = new SKCanvas(scaledBitmap))
            {
                var paint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High
                };

                var srcRect = new SKRect(0, 0, image.Width, image.Height);
                var destRect = new SKRect(0, 0, newWidth, newHeight);

                canvas.DrawImage(image, srcRect, destRect, paint);
            }

            return SKImage.FromBitmap(scaledBitmap);
        }

        public void RadiusBorder(float radius, float borderWidth = 0, SKColor? borderColor = null)
        {
            var path = new SKPath();
            path.AddRoundRect(new SKRect(0, 0, Width, Height), radius, radius);

            MainCanvas.Save();
            MainCanvas.ClipPath(path, SKClipOperation.Intersect, true);

            if (borderWidth > 0 && borderColor.HasValue)
            {
                using var paint = new SKPaint
                {
                    Color = borderColor.Value,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = borderWidth
                };
                MainCanvas.DrawPath(path, paint);
            }
        }

        public void DrawRainbowGradientBorder(float strokeWidth, float radius = 0)
        {
            SKColor[] colors =
            [
                SKColors.Red,
                SKColors.Orange,
                SKColors.Yellow,
                SKColors.Green,
                SKColors.Blue,
                SKColors.Indigo,
                SKColors.Violet
            ];
            DrawGradientBorder(colors, strokeWidth, radius);
        }

        public void DrawGradientBorder(SKColor[] colors, float strokeWidth, float radius = 0)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(Width, Height),
                    colors,
                    null,
                    SKShaderTileMode.Clamp)
            };
            var rect = new SKRect(
                strokeWidth / 2,
                strokeWidth / 2,
                Width - (strokeWidth / 2),
                Height - (strokeWidth / 2));

            if (radius > 0)
            {
                MainCanvas.DrawRoundRect(rect, radius, radius, paint);
            }
            else
            {
                MainCanvas.DrawRect(rect, paint);
            }
        }

        public void Save(string path)
        {
            using var image = MainSurface.Snapshot();
            using var data = image.Encode();

            File.WriteAllBytes(path, data.ToArray());
        }

        public SKImage SnapShot()
        {
            return MainSurface.Snapshot();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposing)
            {
                if (disposing)
                {
                    MainSurface.Dispose();
                }
                Disposing = true;
            }
        }
    }
}