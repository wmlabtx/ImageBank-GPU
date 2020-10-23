﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageBank.Tests
{
    [TestClass()]
    public class SpectreHelperTests
    {
        [TestMethod()]
        public void ComputeTest()
        {
            var imagedata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            if (!SpectreHelper.Compute(bitmap, out var spectre)) {
                Assert.Fail();
            }
        }

        private ColorLAB[] GetDescriptors(string filename)
        {
            var imagedata = File.ReadAllBytes(filename);
            if (!Helper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            if (!SpectreHelper.Compute(bitmap, out var spectre)) {
                Assert.Fail();
            }

            return spectre;
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var baseline = GetDescriptors("org.jpg");
            var files = new[] {
                "org_png.jpg",
                "org_resized.jpg",
                "org_nologo.jpg", 
                "org_r10.jpg", 
                "org_r90.jpg", 
                "org_sim1.jpg",
                "org_sim2.jpg",
                "org_crop.jpg",
                "org_nosim1.jpg",
                "org_nosim2.jpg",
                "org_mirror.jpg"
            };

            var sb = new StringBuilder();
            foreach (var filename in files) {
                var descriptors = GetDescriptors(filename);
                var distance = SpectreHelper.GetDistance(baseline, descriptors);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: {distance:F4}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}