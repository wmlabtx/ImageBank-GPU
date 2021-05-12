﻿using OpenCvSharp;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) { 
                _hashList.Add(img.Hash, img);
                _imgList.Add(img.Name, img);
            }
        }

        private void Add(Img img, Mat akazedescriptors, Mat akazemirrordescriptors)
        {
            AddToMemory(img);
            SqlAdd(img, akazedescriptors, akazemirrordescriptors);
        }
    }
}
