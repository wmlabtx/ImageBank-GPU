﻿using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly Random _random = new Random();

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            var sb = new StringBuilder();
            var method = string.Empty;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        var r = 2; //  _random.Next(7);
                        imgX = null;
                        foreach (var e in _imgList) {
                            var eX = e.Value;
                            if (eX.Hash.Equals(eX.NextHash)) {
                                continue;
                            }

                            if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                                continue;
                            }

                            if (imgX != null &&
                                eX.LastView > eX.LastChanged) {
                                continue;
                            }

                            if (imgX != null) {
                                switch (r) {
                                    case 0:
                                        method = "CLR";
                                        if (imgX.ColorDistance <= eX.ColorDistance) {
                                            continue;
                                        }

                                        break;

                                    case 1:
                                        method = "ORB";
                                        if (imgX.OrbDistance <= eX.OrbDistance) {
                                            continue;
                                        }

                                        break;

                                    case 2:
                                        method = "PHS";
                                        if (imgX.PerceptiveDistance <= eX.PerceptiveDistance) {
                                            continue;
                                        }

                                        break;

                                    case 3:
                                        method = "MD5";
                                        if (imgX.Hash.CompareTo(eX.Hash) <= 0) {
                                            continue;
                                        }

                                        break;

                                    case 4:
                                        method = "LSV";
                                        if (imgX.LastView <= eX.LastView) {
                                            continue;
                                        }

                                        break;

                                    case 5:
                                        method = "DIM";
                                        if (imgX.Width * imgX.Height <= eX.Width * eX.Height) {
                                            continue;
                                        }

                                        break;

                                    case 6:
                                        method = "CFL";
                                        if (imgX.ColorDescriptors.Count(b => b != 0) >= eX.ColorDescriptors.Count(b => b != 0)) {
                                            continue;
                                        }

                                        break;
                                }
                            }

                            imgX = eX;
                            var imgY = eY;
                            nameX = imgX.Name;
                            nameY = imgY.Name;
                        }
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        progress.Report("No images to view");
                        return;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(nameX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(nameX);
                        progress.Report($"{nameX} deleted");
                        nameX = string.Empty;
                        continue;
                    }

                    imgX = AppVars.ImgPanel[0].Img;
                    AppVars.ImgPanel[1] = GetImgPanel(nameY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(nameY);
                        progress.Report($"{nameY} deleted");
                        nameX = string.Empty;
                        continue;
                    }

                    break;
                }

                var zerocounter = _imgList.Count(e => e.Value.LastView <= e.Value.LastChanged);
                sb.Append($"{zerocounter}/{_imgList.Count}: ");
                sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
                sb.Append($"{method} ");
                sb.Append($"p:{imgX.PerceptiveDistance}/o:{imgX.OrbDistance:F2}/c:{imgX.ColorDistance:F2} ");
            }

            progress.Report(sb.ToString());
        }
    }
}
