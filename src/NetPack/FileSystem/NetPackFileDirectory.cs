using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using NetPack.File;

namespace NetPack.FileSystem
{
    public class NetPackFileDirectory
    {

        public NetPackFileDirectory(NetPackFileDirectory parentDir, string dirName)
        {
            Parent = parentDir;
            DirName = dirName;
            Subdirectories = new Dictionary<string, NetPackFileDirectory>();
            Files = new Dictionary<string, IFileInfo>();
        }

        /// <summary>
        /// Sub directories within this directory.
        /// </summary>
        public Dictionary<string, NetPackFileDirectory> Subdirectories { get; set; }

        public Dictionary<string, IFileInfo> Files { get; set; }

        public NetPackFileDirectory Parent { get; set; }

        public string DirName { get; set; }

        public NetPackFileDirectory EnsureSubDirectory(string subDir)
        {

            //string path;
            //if (Parent != null)
            //{
            //    path = SubPathInfo.Parse(Parent.DirName + "/" + subDir).ToString();
            //}
            //else
            //{
            //    path = SubPathInfo.Parse(subDir).ToString();
            //}

            NetPackFileDirectory dir;
            if (!Subdirectories.ContainsKey(subDir))
            {
                dir = new NetPackFileDirectory(this, subDir);
                Subdirectories.Add(subDir, dir);
            }
            else
            {
                dir = Subdirectories[subDir];
            }

            return dir;
        }

        public NetPackFileDirectory EnsureDirectory(string directory)
        {
            var pathInfo = SubPathInfo.Parse(directory);
            // navigate to root dir.
            var topLevelDir = this;
            while (topLevelDir.Parent != null)
            {
                topLevelDir = topLevelDir.Parent;
            }

            NetPackFileDirectory destinationDir = topLevelDir;
            foreach (var segment in pathInfo.DirectorySegements)
            {
                destinationDir = destinationDir.EnsureSubDirectory(segment);
            }
            return destinationDir;
        }

        public void AddFile(IFileInfo file)
        {
            var name = file.Name;

            if (Files.ContainsKey(name))
            {
                Files[name] = file;
            }
            else
            {
                Files.Add(name, file);
            }
        }



    }
}