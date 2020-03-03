﻿using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Delete(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    Helper.DeleteToRecycleBin(img.File);
                    _imgList.Remove(id);
                }
            }

            SqlDelete(id);
            ResetRefers(id);
        }

        private void ResetRefers(int id)
        {
            lock (_imglock) {
                _imgList
                    .Values
                    .Where(e => e.NextId == id)
                    .ToList()
                    .ForEach(e => e.LastId = 0);
            }
        }
    }
}