﻿using OpenCvSharp;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static object _sqllock = new object();
        private static SqlConnection _sqlConnection;

        private object _imglock = new object();
        private readonly SortedDictionary<string, Img> _imgList = new SortedDictionary<string, Img>();
        private readonly SortedDictionary<ulong, string> _hashList = new SortedDictionary<ulong, string>();

        private object _flannlock = new object();
        private bool _flannAvailable = false;
        private readonly FlannBasedMatcher _flannBasedMatcher = new FlannBasedMatcher(new LshIndexParams(12, 20, 2));
        private string[] _flannNames;

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var mc = _imgList.Min(e => e.Value.Counter);
                var cc = _imgList.Count(e => e.Value.Counter == mc);
                sb.Append($"{cc}:{mc}/");
                sb.Append($"{_imgList.Count}: ");
                return sb.ToString();
            }
        }

        private string GetNextToCheck()
        {
            lock (_imglock) {
                var zerolist = _imgList
                    .Where(e => e.Value.GetDescriptors() == null)
                    .Select(e => e.Value)
                    .OrderBy(e => e.LastCheck)
                    .ToArray();

                if (zerolist.Length == 0) {
                    zerolist = _imgList
                    .Where(e => e.Value.Name.Equals(e.Value.NextName, StringComparison.OrdinalIgnoreCase) || !_imgList.ContainsKey(e.Value.NextName))
                    .Select(e => e.Value)
                    .OrderBy(e => e.LastCheck)
                    .ToArray();
                }

                if (zerolist.Length == 0) {
                    zerolist = _imgList
                    .Select(e => e.Value)
                    .OrderBy(e => e.LastCheck)
                    .ToArray();
                }

                var nameX = zerolist.FirstOrDefault().Name;
                return nameX;
            }
        }

        private DateTime GetMinLastView()
        {
            lock (_imglock) {
                var min = (_imgList.Count == 0 ?
                    DateTime.Now :
                    _imgList.Min(e => e.Value.LastView))
                    .AddSeconds(-1);

                return min;
            }
        }
    }
}