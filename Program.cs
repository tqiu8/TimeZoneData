using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace Microsoft.AspNetCore.Components.WebAssembly.Runtime.TimeZone
{
    class Program
    {
        
        static (byte[] json_bytes, MemoryStream stream) enumerateData (string[] sub_folders, string output_folder) {
            var indices = new List<object[]>();
            var stream = new MemoryStream();

            foreach (var folder in sub_folders)
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(output_folder, folder));
                foreach (var entry in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    var relativePath = entry.FullName.Substring(output_folder.Length).Trim('/');
                    indices.Add(new object[] { relativePath, entry.Length});

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
            
            return (jsonBytes, stream);
        }

        static (byte[] json_bytes, MemoryStream stream) readTimeZone (string folder) {
            // https://en.wikipedia.org/wiki/Tz_database#Area
            var areas = new[] { "Africa", "America", "Antarctica", "Arctic", "Asia", "Atlantic", "Australia", "Europe", "Indian", "Pacific" };

            return enumerateData (areas, folder);
        }

        static (byte[] json_bytes, MemoryStream stream) readGeneralData (string input_folder) {
            var DirectoryInfo = new DirectoryInfo (input_folder);
            string[] sub_folders = DirectoryInfo.EnumerateFileSystemInfos().Select(f => f.Name).ToArray();

            return enumerateData (sub_folders, input_folder);
        }

        static void Main(string[] args)
        {
            string type = args[0];
            string input_dir = args[1];
            string data_name = args[2];
            (byte[] json_bytes, MemoryStream stream) data;
            
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "obj", "data", "output");
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException("Output directory does not exist. Use run.sh to run this project");
            }
            
            if (type == "timezone") {
                data = readTimeZone (folder);
            }
            else {
                data = readGeneralData (input_dir);
            }
            
            using (var file = File.OpenWrite(data_name))
            {
                var jsonBytes = data.json_bytes;
                var stream = data.stream;
                var bytes = new byte[4];
                var magicBytes = Encoding.ASCII.GetBytes("talb");
                BinaryPrimitives.WriteInt32LittleEndian(bytes, jsonBytes.Length);
                file.Write(magicBytes);
                file.Write(bytes);
                file.Write(jsonBytes);
                
                stream.CopyTo(file);
            }
            

            
        }
    }
}
