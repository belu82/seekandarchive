using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace SeekAndArchive
{
    class Program
    {
        static List<FileInfo> FoundFiles;
        static List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        static List<DirectoryInfo> archiveDirs;
        static void Main(string[] args)
        {

            string fileName = args[0];
            string directoryName = args[1];
            FoundFiles = new List<FileInfo>();

            DirectoryInfo rootDir = new DirectoryInfo(directoryName);

            if (!rootDir.Exists)
            {
                Console.WriteLine("The specified directory does not exist!");
                return;
            }

            RecursiveSearch(FoundFiles, fileName, rootDir);
            Console.WriteLine("Found{0} files.", FoundFiles.Count);

            foreach(FileInfo fil in FoundFiles)
            {
                Console.WriteLine("{0}", fil.FullName);
            }
            Console.ReadKey();

            archiveDirs = new List<DirectoryInfo>();
            for (int i =0;i<FoundFiles.Count; i++)
            {
                archiveDirs.Add(Directory.CreateDirectory("archive" + i.ToString()));
            }
            foreach (FileInfo fil in FoundFiles)
            {
                FileSystemWatcher newWatcher = new FileSystemWatcher(fil.DirectoryName, fil.Name);
                newWatcher.Changed += new FileSystemEventHandler(WatcherChanged);
                newWatcher.EnableRaisingEvents = true;
                watchers.Add(newWatcher);
            }
        }

        public static void RecursiveSearch(List<FileInfo> foundFiles, string fileName, DirectoryInfo currentDirectory)
        {
            foreach(FileInfo fil in currentDirectory.GetFiles())
            {
                if(fil.Name == fileName)
                {
                    foundFiles.Add(fil);
                }
            }
            foreach(DirectoryInfo dir in currentDirectory.GetDirectories())
            {
                RecursiveSearch(foundFiles, fileName, dir);
            }

        }
        public static void ArchiveFile(DirectoryInfo archiveDir, FileInfo fileToArchive)
        {
            FileStream input = fileToArchive.OpenRead();
            FileStream output = File.Create(archiveDir.FullName + @"" + fileToArchive.Name + ".gz");
            GZipStream Compressor = new GZipStream(output, CompressionMode.Compress);
                
            int b = input.ReadByte();

            while(b != -1)
            {
                Compressor.WriteByte((byte)b);

                b = input.ReadByte();
            }
            Compressor.Close();
            input.Close();
            output.Close();
        }

        public static void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            //find the the index of the changed file 
            FileSystemWatcher senderWatcher = (FileSystemWatcher)sender;
            int index = watchers.IndexOf(senderWatcher, 0);

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Console.WriteLine("{0} has been changes", e.FullPath);
             
                //now that we have the index, we can archive the file
                ArchiveFile(archiveDirs[index], FoundFiles[index]);
            }

            

        }

                          
    }

         
}