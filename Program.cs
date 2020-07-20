﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Components.WebAssembly.Runtime.TimeZone
{
    class Program
    {
        static void Main(string[] args)
        {
            var tzFolder = Path.Combine(Directory.GetCurrentDirectory(), "obj", "data", "output");
            if (!Directory.Exists(tzFolder))
            {
                throw new DirectoryNotFoundException("TZOutput file does not exist. Use run.sh to run this project");
            }

            // https://en.wikipedia.org/wiki/Tz_database#Area
            var areas = new[] { "Africa", "America", "Antarctica", "Arctic", "Asia", "Atlantic", "Australia", "Europe", "Indian", "Pacific" };
            var stream = new MemoryStream();
            var indices = new List<object[]>();

            foreach (var area in areas)
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(tzFolder, area));
                foreach (var entry in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    
                    var relativePath = entry.FullName.Substring(tzFolder.Length).Trim('/');
                    System.Console.WriteLine(entry.FullName);


                    indices.Add(new object[] { relativePath, entry.Length });
                    
                    using (var readStream = entry.OpenRead())
                        readStream.CopyTo(stream);
                }
            }

            stream.Position = 0;
            var options = new JsonSerializerOptions
                {
                    WriteIndented = false
                };

            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(indices);
            using (var timezoneFile = File.OpenWrite("dotnet.timezones.blat"))
            {
                var bytes = new byte[4];
                var magicBytes = Encoding.ASCII.GetBytes("talb");
                BinaryPrimitives.WriteInt32LittleEndian(bytes, jsonBytes.Length);
                timezoneFile.Write(magicBytes);
                timezoneFile.Write(bytes);
                timezoneFile.Write(jsonBytes);
                
                stream.CopyTo(timezoneFile);
            }
        }
    }
}
