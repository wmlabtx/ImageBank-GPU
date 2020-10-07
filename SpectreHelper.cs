﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace ImageBank
{
    public static class SpectreHelper
    {
        public static bool Compute(Bitmap bitmap, out ColorLAB[] spectre)
        {
            Contract.Requires(bitmap != null && bitmap.Width > 0 && bitmap.Height > 0);
            spectre = null;
            using (var bitmap8x8 = Helper.ResizeBitmap(bitmap, 8, 8)) {
                bitmap8x8.Save("bitmap8x8.png", ImageFormat.Png);
                using (var mat8x8 = BitmapConverter.ToMat(bitmap8x8)) {
                    mat8x8.GetArray<Vec3b>(out var rgbpixels);
                    spectre = new ColorLAB[rgbpixels.Length];
                    for (var i = 0; i < rgbpixels.Length; i++) {
                        var colorRGB = new ColorRGB(rgbpixels[i].Item2, rgbpixels[i].Item1, rgbpixels[i].Item0);
                        spectre[i] = new ColorLAB(colorRGB);
                    }
                }
            }

            return true;
        }

        public static float GetDistance(ColorLAB[] x, ColorLAB[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            var list = new List<Tuple<int, int, float>>();
            var xoffset = 0;
            while (xoffset < x.Length) {
                var yoffset = 0;
                while (yoffset < y.Length) {
                    var distance = x[xoffset].CIEDE2000(y[yoffset]);
                    list.Add(new Tuple<int, int, float>(xoffset, yoffset, distance));
                    yoffset++;
                }

                xoffset++;
            }

            list = list.OrderBy(e => e.Item3).ToList();
            var distances = new List<float>();
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                distances.Add(mind);
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            var sum = 0f;
            var count = 0f;
            var k = 1f;
            for (var i = 0; i < distances.Count; i++) {
                sum += distances[i] * k;
                count += k;
                k *= 0.9f;
            }

            var avgdistance = sum / count;
            return avgdistance;
        }

        public static byte[] ToBuffer(ColorLAB[] spectre)
        {
            Contract.Requires(spectre != null);
            var fb = new float[spectre.Length * 3];
            for (var i = 0; i < spectre.Length; i++) {
                fb[i * 3] = spectre[i].L;
                fb[i * 3 + 1] = spectre[i].A;
                fb[i * 3 + 2] = spectre[i].B;
            }

            var b = new byte[fb.Length * sizeof(float)];
            Buffer.BlockCopy(fb, 0, b, 0, b.Length);
            return b;
        }

        public static ColorLAB[] FromBuffer(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            var fb = new byte[buffer.Length / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, fb, 0, buffer.Length);
            var spectre = new ColorLAB[fb.Length / 3];
            for (var i = 0; i < spectre.Length; i++) {
                spectre[i] = new ColorLAB(fb[i * 3], fb[i * 3 + 1], fb[i * 3 + 2]);
            }
            
            return spectre;
        }
    }
}
