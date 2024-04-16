using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace P42.Utils
{
    public static class AssemblyExtensions
    {
        public static bool EmbeddedResourceExists(this Assembly assembly, string resourceId)
        {
            foreach (var res in assembly.GetManifestResourceNames())
                if (res == resourceId)
                    return true;
            return false;
        }

        public static void TryCopyResource(this Assembly assembly, string resourceId, string path)
        {
            if (assembly.GetManifestResourceStream(resourceId) is Stream stream)
            {
                WriteToFile(stream, path);
                stream.Dispose();
            }
        }


        public static void WriteToFile(Stream stream, string destinationFile, int bufferSize = 4096, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.ReadWrite)
        {
            using (var destinationFileStream = new FileStream(destinationFile, mode, access, share))
            {
                while (stream.Position < stream.Length)
                {
                    destinationFileStream.WriteByte((byte)stream.ReadByte());
                }
            }
        }
    }
}
