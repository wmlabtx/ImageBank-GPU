﻿using System;
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

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=30";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public void UpdateLastView(string id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.LastView = DateTime.Now;
                }
            }
        }

        public void UpdateCounter(string id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.Counter += 1;
                }
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

        private DateTime GetMinLastCheck()
        {
            lock (_imglock) {
                var min = (_imgList.Count == 0 ?
                    DateTime.Now :
                    _imgList.Min(e => e.Value.LastCheck))
                    .AddSeconds(-1);

                return min;
            }
        }

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var mc = _imgList.Min(e => e.Value.Counter);
                var cc = _imgList.Count(e => e.Value.Counter == mc);
                var md = _imgList.Where(e => e.Value.Counter == mc).Min(e => e.Value.Distance);
                var cd = _imgList.Where(e => e.Value.Counter == mc).Count(e => e.Value.Distance == md);
                sb.Append($"{cc}:{mc}/");
                sb.Append($"{cd}:{md:F2}/");
                sb.Append($"{_imgList.Count}: ");
                return sb.ToString();
            }
        }

        private string GetNextToCheck()
        {
            lock (_imglock) {
                var idX = string.Empty;
                var minlc = DateTime.MaxValue;
                //var scope = _imgList.OrderBy(e => e.Value.LastView).Take(100).ToArray();
                foreach (var e in /*scope*/_imgList) {
                    if (e.Value.LastCheck < minlc) {
                        idX = e.Value.Id;
                        minlc = e.Value.LastCheck;
                    }
                }

                return idX;
            }
        }
    }
}