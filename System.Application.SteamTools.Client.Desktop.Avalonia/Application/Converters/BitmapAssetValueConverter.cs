﻿using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace System.Application.Converters
{
    public class BitmapAssetValueConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is string rawUri)
            {
                Uri uri;
                // Allow for assembly overrides
                if (File.Exists(rawUri))
                {
                    return new Bitmap(rawUri);
                }
                //在列表中使用此方法性能极差
                else if (rawUri.StartsWith("http://") || rawUri.StartsWith("https://")) 
                {
                    using var web = new WebClient();
                    var bt = web.DownloadData(rawUri);
                    using var stream = new MemoryStream(bt);
                    return new Bitmap(stream);
                }
                else if (rawUri.StartsWith("avares://"))
                {
                    uri = new Uri(rawUri);
                }
                else
                {
                    string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                    uri = new Uri($"avares://{assemblyName}{rawUri}");
                }
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets.Open(uri);
                return new Bitmap(asset);
            }
            else if (value is Stream s)
            {
                if (s.Position > 0)
                {
                    s.Position = 0;
                }
                return new Bitmap(s);
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}