using SkiaSharp;

namespace ACP_MiniProyecto
{
    public class GraphGenerator
    {

        private SKPoint GraphCenter;
        public required Point[] Points;
        public required string XAxisLabel;
        public required string YAxisLabel;
        public required string[] RowHeaders;


        /// <summary>   
        /// Generates the bitmap image given the canvas that is going to be drawn.
        /// CircleGraph? if it should draw a cartesian plane with negative Y
        /// </summary>
        public SKBitmap DrawCartesianPlane(int width, int height, bool circleGraph = true)
        {
            SKBitmap bitmap = new(width, height);

            using (SKCanvas canvas = new(bitmap))
            {
                DrawCartesianPlane(canvas, width, height, circleGraph);
            }

            return bitmap;
        }

        /// <summary>   
        /// Responsible for drawing the canvas given a width and height,
        /// including its labels.
        /// CircleGraph? if it should draw a cartesian plane with negative Y
        /// </summary>
        private void DrawCartesianPlane(SKCanvas canvas, int width, int height, bool circleGraph)
        {
            using SKPaint paint = new();

            SKPaint tickPaint = new()
            {
                StrokeWidth = 1,
            };

            canvas.Clear(SKColors.White);

            // Set graph center
            GraphCenter = new SKPoint(width / 2, height / 2);

            DrawGrid(canvas, width, height);

            if (circleGraph)
            {
                canvas.DrawLine(0, height / 2, width, height / 2, paint);
                canvas.DrawLine(width / 2, 0, width / 2, height, paint);
                DrawDoublePointedArrow(canvas, paint, new SKPoint(0, height / 2), new SKPoint(width, height / 2));
                DrawDoublePointedArrow(canvas, paint, new SKPoint(width / 2, 0), new SKPoint(width / 2, height));

                // Draw X-axis labels
                for (float x = -2; x <= 2; x += 0.2f)
                {
                    float labelX = MapValue(x, -2, 2, 0, width);
                    canvas.DrawText(x.ToString("0.0"), labelX, height / 2 + 20, paint);
                    canvas.DrawLine(labelX, height / 2 - 3, labelX, height / 2 + 3, tickPaint);
                }

                // Draw Y-axis labels
                for (float y = -2; y <= 2; y += 0.2f)
                {
                    float labelY = MapValue(y, -2, 2, height, 0);
                    canvas.DrawText(y.ToString("0.0"), width / 2 - 30, labelY, paint);
                    canvas.DrawLine(width / 2 - 3, labelY, width / 2 + 3, labelY, tickPaint);
                }

                float radius = (float)(width / 4.00);
                DrawCircle(canvas, radius);
            }
            else
            {
                SKPaint linePaint = new()
                {
                    IsAntialias = true,
                    StrokeWidth = 2,
                };

                canvas.DrawLine(3, 3, 3, height - 3, linePaint);
                canvas.DrawLine(3, height - 3, width, height - 3, linePaint);

                // Draw X-axis labels
                float start = -2.0f;
                float end = 2.0f;
                for (float x = -2; x <= 2; x += 0.2f)
                {
                    float labelX = MapValue(x, start, end, 0, width);
                    canvas.DrawText(x.ToString("0.0"), labelX, height - 5, paint);
                    canvas.DrawLine(labelX, height, labelX, height - 7, tickPaint);
                }
                // Draw Y-axis labels
                for (float y = -2; y <= 2; y += 0.2f)
                {
                    float labelY = height - MapValue(y, start, end, height, 0);
                    canvas.DrawText(y.ToString("0.0"), 5, labelY, paint);
                    canvas.DrawLine(3, labelY, 5, labelY, tickPaint);
                }
            }

            DrawPoints(canvas, width, height, circleGraph);

            SKPaint labelPaint = new()
            {
                StrokeWidth = 2,
                FakeBoldText = true,
                TextSize = 18,
                IsAntialias = true,
            };

            // Labels for the sides, woohoo
            canvas.DrawText(XAxisLabel, width / 2 + 10, height - 20, labelPaint);
            canvas.DrawText(YAxisLabel, 20, height / 2 + 10, labelPaint);
        }

        /// <summary>   
        /// Draws double pointed arrow in the canvas
        /// </summary>
        private void DrawDoublePointedArrow(SKCanvas canvas, SKPaint paint, SKPoint start, SKPoint end)
        {
            canvas.DrawLine(start, end, paint);
            DrawArrow(canvas, paint, start, end, 10);
            DrawArrow(canvas, paint, end, start, 10);
        }

        /// <summary>   
        /// Draws an arrow in the canvas
        /// </summary>
        private void DrawArrow(SKCanvas canvas, SKPaint paint, SKPoint start, SKPoint end, float size)
        {
            float length = (float)Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            float unitX = (end.X - start.X) / length;
            float unitY = (end.Y - start.Y) / length;

            SKPoint point1 = new(end.X - size * unitX - size * unitY, end.Y - size * unitY + size * unitX);
            SKPoint point2 = new(end.X - size * unitX + size * unitY, end.Y - size * unitY - size * unitX);

            canvas.DrawLine(end, point1, paint);
            canvas.DrawLine(end, point2, paint);
        }

        private void DrawPoints(SKCanvas canvas, int width, int height, bool withLine = true)
        {
            int rows = Accord.Math.Matrix.Rows(Points);

            Random rnd = new();

            for (int i = 0; i < rows; i++)
            {

                byte red = (byte)rnd.Next(200);
                byte green = (byte)rnd.Next(200);
                byte blue = (byte)rnd.Next(200);

                SKColor color = new(red, green, blue);

                SKPaint paint = new()
                {
                    Color = color,
                    IsAntialias = true,
                    StrokeWidth = 2,
                    FakeBoldText = true,
                    TextSize = 14,
                };

                double x = Points[i].X;
                double y = Points[i].Y;
                string? pointName = Points[i].PointName;

                SKPoint point = new((float)(width / 2 + x * (width / 4)), (float)(height / 2 - y * (height / 4)));

                SKPaint pointPaint = new()
                {
                    Color = color,
                    IsAntialias = true,
                    StrokeWidth = 12,
                    StrokeCap = SKStrokeCap.Round,
                };

                if (withLine)
                {
                    canvas.DrawLine(GraphCenter, point, paint);
                }

                canvas.DrawPoint(point, pointPaint);

                if (point.X + 50 > width)
                {
                    point.X -= 80;
                }

                if (point.Y - 50 <= 0)
                {
                    point.Y += 20;
                }

                if (pointName != null)
                {
                    canvas.DrawText(pointName, point.X + 10, point.Y, paint);
                }

            }
        }

        private float MapValue(float value, float start1, float end1, float start2, float end2)
        {
            return start2 + (value - start1) * (end2 - start2) / (end1 - start1);
        }

        private void DrawCircle(SKCanvas canvas, float radius)
        {
            float scaleX = canvas.LocalClipBounds.Width / 2;
            float scaleY = canvas.LocalClipBounds.Height / 2;

            SKPaint paint = new()
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash([10, 10], 0)
            };

            if (scaleX > scaleY)
            {
                float scaledRadius = radius * scaleY / scaleX;
                float centerX = canvas.LocalClipBounds.Left + scaleX;
                float centerY = canvas.LocalClipBounds.Top + canvas.LocalClipBounds.Height / 2;
                canvas.DrawOval(new SKRect(centerX - radius, centerY - scaledRadius, centerX + radius, centerY + scaledRadius), paint);
            }
            else
            {
                float scaledRadius = radius * scaleX / scaleY;
                float centerX = canvas.LocalClipBounds.Left + canvas.LocalClipBounds.Width / 2;
                float centerY = canvas.LocalClipBounds.Top + scaleY;
                canvas.DrawOval(new SKRect(centerX - scaledRadius, centerY - radius, centerX + scaledRadius, centerY + radius), paint);
            }
        }

        private void DrawGrid(SKCanvas canvas, int width, int height)
        {
            using SKPaint paint = new()
            {
                Color = SKColors.LightGray,
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke
            };

            for (int x = 0; x <= width; x += width / 40)
            {
                canvas.DrawLine(x, 0, x, height, paint);
            }

            for (int y = 0; y <= height; y += height / 40)
            {
                canvas.DrawLine(0, y, width, y, paint);
            }
        }

    }
}
