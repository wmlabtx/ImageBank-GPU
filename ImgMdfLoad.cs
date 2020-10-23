﻿using System;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Load(IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrHash}, "); // 2
            sb.Append($"{AppConsts.AttrLastView}, "); // 3
            sb.Append($"{AppConsts.AttrWidth}, "); // 4
            sb.Append($"{AppConsts.AttrHeigth}, "); // 5
            sb.Append($"{AppConsts.AttrSize}, "); // 6
            sb.Append($"{AppConsts.AttrColors}, "); // 7
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 8
            sb.Append($"{AppConsts.AttrLastAdded}, "); // 9
            sb.Append($"{AppConsts.AttrNextName}, "); // 10
            sb.Append($"{AppConsts.AttrDistance}, "); // 11
            sb.Append($"{AppConsts.AttrFamily} "); // 12
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetInt32(1);
                            var bhash = (byte[])reader[2];
                            var hash = BitConverter.ToUInt64(bhash, 0);
                            var lastview = reader.GetDateTime(3);
                            var width = reader.GetInt32(4);
                            var heigth = reader.GetInt32(5);
                            var size = reader.GetInt32(6);
                            var colors = (byte[])reader[7];
                            var lastcheck = reader.GetDateTime(8);
                            var lastadded = reader.GetDateTime(9);
                            var nextname = reader.GetString(10);
                            var distance = reader.GetFloat(11);
                            var family = reader.GetString(12);
                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                heigth: heigth,
                                size: size,
                                colors: colors,
                                folder: folder,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                lastadded: lastadded,
                                nextname: nextname,
                                distance: distance,
                                family: family
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }
            }

            progress.Report("Database loaded");
        }
    }
}