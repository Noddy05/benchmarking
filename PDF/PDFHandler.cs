using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Benchmarking;
using System.Collections.Generic;
using System;
using PdfSharp.Pdf.Annotations;

namespace PDF
{
    public enum Alignment
    {
        bottomLeft,  bottomCenter,  bottomRight,
        centerLeft,  center,        centerRight,
        topLeft,     topCenter,     topRight,
    }

    class PDFHandler
    {
        private static PdfDocument document;
        private static PdfPage page;
        private static XGraphics gfx;

        private static int yOffset = 0;
        private static int xOffset = 0;
        private static int yMargin = 60;
        private static int xMargin = 60;
        private static int pageIndex = -1;
        private static List<PdfPage> pages = new List<PdfPage>();

        private static XFont bodyFont = new XFont("Franklin Gothic Book", 12); 
        private static XFont titleFont = new XFont("Franklin Gothic Demi Cond", 22);
        private static XFont subtitleFont = new XFont("Franklin Gothic Demi Cond", 14);
        private static XBrush bodyBrush = XBrushes.Black;
        private static XBrush titleBrush = new XSolidBrush(XColor.FromArgb(68, 114, 196));

        public static void NewPDF()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            document = new PdfDocument();
            document.Info.Title = "Benchmarking Report";
            document.Info.Author = "Benchmarking Profiler | Noah D. Dirksen";
            document.Info.CreationDate = DateTime.Now;

            NewPage();
        }

        public static void RequireRect(ref XRect rect, bool relative = true, int lineDistance = 0)
        {
            if (relative)
            {
                rect.Y += yOffset;
                rect.X += xOffset;
                rect.Width += 0;
            }

            if (rect.Y + rect.Height - yMargin > page.Height - yMargin * 2)
            {
                NewPage();
            }

            if (rect.X + rect.Width - xMargin > page.Width - xMargin * 2)
            {
                yOffset += lineDistance;
                xOffset = 0;
            }
        }
        public static void RequireRect(XRect rect, bool relative = true, int lineDistance = 0)
        {
            if (relative)
            {
                rect.X += xOffset;
                rect.Width += 0;
            }

            if (rect.Y + rect.Height - yMargin > page.Height - yMargin * 2)
            {
                NewPage();
            }

            if (rect.X + rect.Width - xMargin > page.Width - xMargin * 2)
            {
                yOffset += lineDistance;
                xOffset = 0;
            }
        }

        private static void NewPage()
        {
            page = document.AddPage();
            yOffset = 0;
            xOffset = 0;
            gfx = XGraphics.FromPdfPage(page);
            pageIndex++;
            pages.Add(page);
        }

        private static XStringFormat StringFormat(Alignment alignment)
        {
            XStringFormat format = new XStringFormat();
            format.Alignment = (XStringAlignment)((int)alignment % 3);
            format.LineAlignment = (XLineAlignment)((int)alignment / 3);

            return format;
        }

        private static void WriteLine(string text, XRect rect, XFont font, XBrush brush, 
            Alignment alignment, int lineOffset, bool changeOffset = true, bool reference = false)
        {
            if(!reference)
                RequireRect(rect);
            else
                RequireRect(ref rect);
            XStringFormat textFormat = StringFormat(alignment);
            XRect newRect = new XRect(rect.X, rect.Y + yOffset, rect.Width, rect.Height);

            gfx.DrawString(text, font, brush, rect, textFormat);

            if (changeOffset)
                yOffset += (int)newRect.Height + lineOffset;

            xOffset = 0;
        }

        private static void Write(string text, XRect rect, XFont font, XBrush brush,
            Alignment alignment, int lineOffset, bool changeOffset = true)
        {
            RequireRect(ref rect);
            XStringFormat textFormat = StringFormat(alignment);

            gfx.DrawString(text, font, brush, rect, textFormat);

            if (changeOffset)
                xOffset = (int)rect.Width;
        }

        public static void Generate(BenchmarkData data)
        {
            Generate(new BenchmarkData[] { data });
        }

        public static void Generate(BenchmarkData[] data)
        {
            #region Contents
            //XRect rect = new XRect(xMargin, yMargin, page.Width - xMargin * 2, page.Height - yMargin * 2);
            WriteLine("Benchmarking report", new XRect(xMargin, yMargin, page.Width - xMargin * 2, 20),
                titleFont, titleBrush, Alignment.center, 25);

            WriteLine("Contents", new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 10), 
                subtitleFont, titleBrush, Alignment.centerLeft, 10);

            int introductionConentY = yOffset;
            WriteLine("Introduction", new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 10),
                bodyFont, bodyBrush, Alignment.centerLeft, 2);

            int structDataConentY = yOffset;
            for(int i = 0; i < data.Length; i++)
            {
                XRect rect = new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 10);

                WriteLine($"Data analysis of {data[i].title}", rect,
                    bodyFont, bodyBrush, Alignment.centerLeft, 2);
            }

            int comparedDataConentY = yOffset;
            WriteLine($"Compared data", new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 10),
                bodyFont, bodyBrush, Alignment.centerLeft, 2);

            int conclusionConentY = yOffset;
            WriteLine("Conclusion", new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 10),
                bodyFont, bodyBrush, Alignment.centerLeft, 2);

            NewPage();
            #endregion
            
            #region Introduction Page
            WriteLine("Introduction", new XRect(xMargin, yMargin, page.Width - xMargin * 2, 10),
                subtitleFont, titleBrush, Alignment.centerLeft, 10);

            string text = $"Benchmarking test of {data[0].title}";
            if(data.Length > 1)
            {
                if(data.Length > 2)
                {
                    text += ", ";
                }
                else
                {
                    text += " and ";
                }
            }

            XSize size = gfx.MeasureString(text, bodyFont);
            Write(text, new XRect(xMargin, yMargin, size.Width, 10),
                bodyFont, bodyBrush, Alignment.centerLeft, 0, false);
            xOffset += (int)size.Width;

            for (int i = 1; i < data.Length; i++)
            {
                text = $"{data[i].title}";
                if (data.Length > i + 1)
                {
                    if (data.Length > i + 2)
                    {
                        text += ", ";
                    }
                    else
                    {
                        text += " and ";
                    }
                }
                else
                {
                    text += ".";
                }

                size = gfx.MeasureString(text, bodyFont);
                XRect horizontalRect = new XRect(xMargin, yMargin, size.Width, 10);

                RequireRect(horizontalRect, true, 10);
                Write(text, new XRect(xMargin, yMargin, size.Width, 10),
                    bodyFont, bodyBrush, Alignment.centerLeft, 0, false);
                xOffset += (int)size.Width;
            }

            #endregion

            #region Data Pages

            for (int i = 0; i < data.Length; i++)
            {
                NewPage();

                WriteLine($"Data analysis of {data[i].title}", new XRect(xMargin, yMargin, page.Width - xMargin * 2, 10),
                    subtitleFont, titleBrush, Alignment.centerLeft, 10);

                WriteLine($"Graph {1 + i * 2} shows the execution time(ms) as a function of your codes' iterations.", 
                    new XRect(xMargin, yMargin, size.Width, 10),
                    bodyFont, bodyBrush, Alignment.centerLeft, 3, true, true);

                WriteLine($"Graph {2 + i * 2} shows the time distribution of your code.",
                    new XRect(xMargin, yMargin, size.Width, 10),
                    bodyFont, bodyBrush, Alignment.centerLeft, 3, true, true);

                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(233, 239, 242)), 
                    new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 250));

                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 4, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 4, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 2, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 2, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 4 * 3, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 4 * 3, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 4 * 3),
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 4 * 3));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 4),
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 4));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 2),
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 2));

                int n = data[i].runs;

                float max = 0;
                float[] xData = new float[n];
                float[] yData = new float[n];
                for (int j = 0; j < n; j++)
                {
                    xData[j] = j + 1;
                    max += data[i].testData.executionTimes[j];
                    yData[j] = max;
                }

                Graph graph = GraphData(xData, yData);
                max -= graph.b;
                float maxPoints = max;
                if (max < graph.a * n)
                    max = graph.a * n;

                gfx.DrawString("0", bodyFont, bodyBrush, 
                    new XPoint(xMargin, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString($"{MathF.Round(graph.b)}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250), StringFormat(Alignment.centerRight));

                gfx.DrawString(MathF.Round(Interpolate(n, 0, 0)).ToString(), bodyFont, bodyBrush, 
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(n, 0, 0.5f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 2, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(n, 0, 0.75f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 4, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(n, 0, 0.25f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 4 * 3, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));



                gfx.DrawString(MathF.Round(Interpolate(max, graph.b, 0f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset), StringFormat(Alignment.centerRight));

                gfx.DrawString(MathF.Round(Interpolate(max, graph.b, 0.5f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 2), StringFormat(Alignment.centerRight));

                gfx.DrawString(MathF.Round(Interpolate(max, graph.b, 0.25f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 4), StringFormat(Alignment.centerRight));

                gfx.DrawString(MathF.Round(Interpolate(max, graph.b, 0.75f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 4 * 3), StringFormat(Alignment.centerRight));

                gfx.DrawLine(XPens.Red, new XPoint(xMargin, yMargin + yOffset + 250), 
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 - 250 * (graph.a * n) / max));

                float total = 0;
                float radius = 4;
                for(int j = 0; j < n; j++)
                {
                    total += data[i].testData.executionTimes[j];
                    gfx.DrawEllipse(XPens.Black, XBrushes.Red, new XRect(
                        Interpolate(xMargin, (float)page.Width - xMargin, (float)(j + 1) / n) - radius / 2, 
                        yMargin + yOffset + 250 - 250 * (total - graph.b) / max - radius / 2, radius, radius));
                }

                string function = $"{graph.a}x + {graph.b}";
                if (graph.a == 0)
                {
                    function = $"{graph.b}";
                }
                if (graph.b < 0)
                {
                    function = $"{graph.a}x - {-graph.b}";
                    if(graph.a == 0)
                    {
                        function = $"{graph.b}";
                    }
                }
                else if (graph.b == 0)
                {
                    function = $"{graph.a}x";
                    if (graph.a == 0)
                    {
                        function = "0";
                    }
                }

                gfx.DrawString($"Graph {i * 2 + 1}, f(x) = {function}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"R  = {graph.r2}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 1), StringFormat(Alignment.centerLeft));
                gfx.DrawString($"2", new XFont(bodyFont.FontFamily.ToString(), 8), bodyBrush,
                    new XPoint(xMargin - 12 + 7, yMargin + yOffset + 300 + 14 * 1 - 2), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"Sum(x) = {graph.xSum}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 3), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"Sum(y) = {graph.ySum}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 4), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"Sum(x  ) = {graph.xSqSum}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 5), StringFormat(Alignment.centerLeft));
                gfx.DrawString($"2", new XFont(bodyFont.FontFamily.ToString(), 8), bodyBrush,
                    new XPoint(xMargin - 12 + 32, yMargin + yOffset + 300 + 14 * 5 - 2), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"Sum(y  ) = {graph.ySqSum}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 6), StringFormat(Alignment.centerLeft));
                gfx.DrawString($"2", new XFont(bodyFont.FontFamily.ToString(), 8), bodyBrush,
                    new XPoint(xMargin - 12 + 32, yMargin + yOffset + 300 + 14 * 6 - 2), StringFormat(Alignment.centerLeft));

                gfx.DrawString($"Sum(xy) = {graph.xySum}", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 7), StringFormat(Alignment.centerLeft));

                float averageTime = MathF.Round(graph.ySum / graph.xSum);
                gfx.DrawString($"Average execution time (pr. iteration): {averageTime}ms.", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 9), StringFormat(Alignment.centerLeft));
                gfx.DrawString($"{MathF.Round(graph.r2 * 1000) / 10}% of the " +
                    $"variation in y is explained by the variation in x.", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 10), StringFormat(Alignment.centerLeft));
                gfx.DrawString($"Leaving {MathF.Round((1 - graph.r2) * 1000) / 10}% of the variation in y unexplained.", 
                    bodyFont, bodyBrush, new XPoint(xMargin - 12, yMargin + yOffset + 300 + 14 * 11), StringFormat(Alignment.centerLeft));

                

                NewPage();

                Dictionary<int, int> distribution = new Dictionary<int, int>();
                for(int j = 0; j < data[i].runs; j++)
                {
                    if (distribution.ContainsKey(data[i].testData.executionTimes[j]))
                    {
                        distribution[data[i].testData.executionTimes[j]]++;
                    }
                    else
                    {
                        distribution.Add(data[i].testData.executionTimes[j], 1);
                    }
                }

                List<int> timeKeys = new List<int>();
                List<int> timeValues = new List<int>();
                foreach (KeyValuePair<int, int> stat in distribution)
                {
                    timeKeys.Add(stat.Key);
                    timeValues.Add(stat.Value);
                }
                int[] sortedTimeKeys = timeKeys.ToArray();
                int[] sortedTimeValues = timeValues.ToArray();

                InsertionSort(sortedTimeKeys);
                InsertionSort(sortedTimeValues);

                gfx.DrawString($"{sortedTimeKeys[0]}", bodyFont, bodyBrush,
                    new XPoint(xMargin, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(233, 239, 242)),
                    new XRect(xMargin, yMargin + yOffset, page.Width - xMargin * 2, 250));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeKeys[sortedTimeKeys.Length - 1], sortedTimeKeys[0], 0)).ToString(), bodyFont, bodyBrush,
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeKeys[sortedTimeKeys.Length - 1], sortedTimeKeys[0], 0.25f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 4 * 3, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeKeys[sortedTimeKeys.Length - 1], sortedTimeKeys[0], 0.5f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 2, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeKeys[sortedTimeKeys.Length - 1], sortedTimeKeys[0], 0.75f)).ToString(), bodyFont, bodyBrush,
                    new XPoint(xMargin + (page.Width - xMargin * 2) / 4, yMargin + yOffset + 250 + 12), StringFormat(Alignment.center));


                gfx.DrawString(MathF.Round(Interpolate(sortedTimeValues[sortedTimeValues.Length - 1] / 
                    (float)data[i].runs * 100, 0, 0.25f)).ToString() + "%", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 4), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeValues[sortedTimeValues.Length - 1] / 
                    (float)data[i].runs * 100, 0, 0.75f)).ToString() + "%", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 4 * 3), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeValues[sortedTimeValues.Length - 1] / 
                    (float)data[i].runs * 100, 0, 0.5f)).ToString() + "%", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 / 2), StringFormat(Alignment.center));

                gfx.DrawString(MathF.Round(Interpolate(sortedTimeValues[sortedTimeValues.Length - 1] / 
                    (float)data[i].runs * 100, 0, 0f)).ToString() + "%", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250 * 0), StringFormat(Alignment.center));

                gfx.DrawString($"0%", bodyFont, bodyBrush,
                    new XPoint(xMargin - 12, yMargin + yOffset + 250), StringFormat(Alignment.center));

                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 4, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 4, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 2, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 2, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin + (page.Width - 2 * xMargin) / 4 * 3, yMargin + yOffset),
                    new XPoint(xMargin + (page.Width - 2 * xMargin) / 4 * 3, yMargin + yOffset + 250));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 4 * 3),
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 4 * 3));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 4), 
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 4));
                gfx.DrawLine(XPens.LightGray, new XPoint(xMargin, yMargin + yOffset + 250 / 2),
                    new XPoint(page.Width - xMargin, yMargin + yOffset + 250 / 2));




                XVector lastPoint = new XVector(0, 0);
                for (int j = 0; j < sortedTimeKeys.Length; j++)
                {
                    XVector vector = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                            (float)(sortedTimeKeys[j] - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                            sortedTimeKeys[0])) - radius / 2, yMargin + yOffset + 250 - 250 * ((float)distribution[sortedTimeKeys[j]]
                            / sortedTimeValues[sortedTimeValues.Length - 1]) - radius / 2);

                    XVector vector2 = vector;
                    if (j + 1 < sortedTimeKeys.Length)
                        vector2 = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                            (float)(sortedTimeKeys[j + 1] - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                            sortedTimeKeys[0])), yMargin + yOffset + 250 - 250 * ((float)distribution[sortedTimeKeys[j + 1]]
                            / sortedTimeValues[sortedTimeValues.Length - 1]));


                    
                    if (j > 0 && sortedTimeKeys[j - 1] != sortedTimeKeys[j] - 1)
                    {
                        XVector ground = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                            (float)(sortedTimeKeys[j] - 1 - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                            sortedTimeKeys[0])), yMargin + yOffset + 250);

                        gfx.DrawLine(XPens.Black, new XPoint(vector.X + radius / 2, vector.Y + radius / 2),
                            new XPoint(ground.X, ground.Y));

                        gfx.DrawLine(XPens.Black, new XPoint(lastPoint.X, lastPoint.Y),
                            new XPoint(ground.X, ground.Y));
                    }

                    if (j + 1 < sortedTimeKeys.Length)
                    {
                        if (sortedTimeKeys[j + 1] == sortedTimeKeys[j] + 1)
                        {
                            gfx.DrawLine(XPens.Black, new XPoint(vector.X + radius / 2, vector.Y + radius / 2), 
                                new XPoint(vector2.X, vector2.Y));
                        }
                        else
                        {
                            XVector ground = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                                (float)(sortedTimeKeys[j] + 1 - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                                sortedTimeKeys[0])), yMargin + yOffset + 250);

                            gfx.DrawLine(XPens.Black, new XPoint(vector.X + radius / 2, vector.Y + radius / 2),
                                new XPoint(ground.X, ground.Y));
                            lastPoint = ground;
                        }
                    }

                }

                for (int j = 0; j < sortedTimeKeys.Length; j++)
                {
                    XVector vector = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                            (float)(sortedTimeKeys[j] - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                            sortedTimeKeys[0])) - radius / 2, yMargin + yOffset + 250 - 250 * ((float)distribution[sortedTimeKeys[j]]
                            / sortedTimeValues[sortedTimeValues.Length - 1]) - radius / 2);

                    XVector vector2 = vector;
                    if (j + 1 < sortedTimeKeys.Length)
                        vector2 = new XVector(Interpolate(xMargin, (float)page.Width - xMargin,
                            (float)(sortedTimeKeys[j + 1] - sortedTimeKeys[0]) / (sortedTimeKeys[sortedTimeKeys.Length - 1] -
                            sortedTimeKeys[0])), yMargin + yOffset + 250 - 250 * ((float)distribution[sortedTimeKeys[j + 1]]
                            / sortedTimeValues[sortedTimeValues.Length - 1]));

                    gfx.DrawEllipse(XPens.Black, XBrushes.Red, new XRect(
                        vector.X, vector.Y, radius, radius));
                }

                yOffset += 300;

                List<int> types = new List<int>();
                List<int> notTypes = new List<int>();
                for (int j = 0; j < sortedTimeKeys.Length; j++)
                {
                    if (distribution[sortedTimeKeys[j]] == sortedTimeValues[sortedTimeValues.Length - 1])
                    {
                        types.Add(sortedTimeKeys[j]);
                    }
                    if (distribution[sortedTimeKeys[j]] == sortedTimeValues[0])
                    {
                        notTypes.Add(sortedTimeKeys[j]);
                    }
                }

                string frequent = "";
                for (int j = 0; j < types.Count; j++)
                {
                    if (j > 0)
                    {
                        if (j < types.Count - 1)
                            frequent += ", " + types[j];
                        else
                            frequent += " and " + types[j];
                    }
                    else
                        frequent += types[j];
                }

                string infrequent = "";
                for (int j = 0; j < notTypes.Count; j++)
                {
                    if (j > 0)
                    {
                        if (j < notTypes.Count - 1)
                            infrequent += ", " + notTypes[j];
                        else
                            infrequent += " and " + notTypes[j];
                    }
                    else
                        infrequent += notTypes[j];
                }

                gfx.DrawString($"Most frequently occuring number is {frequent}ms, " +
                $"occuring {sortedTimeValues[sortedTimeValues.Length - 1]} " + 
                (sortedTimeValues[sortedTimeValues.Length - 1] != 1 ? "times." : "time."), bodyFont, bodyBrush,
                new XPoint(xMargin - 12, yMargin + yOffset), StringFormat(Alignment.centerLeft));

                yOffset += 12;
                gfx.DrawString($"Least frequently occuring number is {infrequent}ms, " +
                $"occuring {sortedTimeValues[0]} " + (sortedTimeValues[0] != 1 ? "times." : "time."), bodyFont, bodyBrush,
                new XPoint(xMargin - 12, yMargin + yOffset), StringFormat(Alignment.centerLeft));
            }
            #endregion

            document.Save("../../../BenchmarkResults/BenchmarkingResult.pdf");
        }

        static float Interpolate(float a, float b, float t)
            => a + (b - a) * t;

        //Not my code
        static void InsertionSort(int[] arr)
        {
            int i, key, j;
            for (i = 1; i < arr.Length; i++)
            {
                key = arr[i];
                j = i - 1;

                // Move elements of arr[0..i-1],  
                // that are greater than key, to one 
                // position ahead of their 
                // current position
                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j = j - 1;
                }
                arr[j + 1] = key;
            }
        }

        public static Graph GraphData(float[] xData, float[] yData)
        {
            int length = xData.Length;

            float xSum = 0;
            float ySum = 0;
            float xySum = 0;
            float xSqSum = 0;
            float ySqSum = 0;
            for (int i = 0; i < xData.Length; i++)
            {
                xSum += xData[i];
                ySum += yData[i];
                xySum += xData[i] * yData[i];
                xSqSum += MathF.Pow(xData[i], 2);
                ySqSum += MathF.Pow(yData[i], 2);
            }
            float n = xData.Length;
            float a = (n * xySum - xSum * ySum) / (n * xSqSum - MathF.Pow(xSum, 2));
            float b = (ySum - a * xSum) / n;
            float r = (n * xySum - xSum * ySum) /
            (MathF.Sqrt(n * xSqSum - MathF.Pow(xSum, 2)) * MathF.Sqrt(n * ySqSum - MathF.Pow(ySum, 2)));

            float R2 = MathF.Pow(r, 2);

            return new Graph
            {
                xSum = xSum,
                ySum = ySum,
                xySum = xySum,
                xSqSum = xSqSum,
                ySqSum = ySqSum,
                n = n,
                a = a,
                b = b,
                r = r,
                r2 = R2
            };
        }
    }
}