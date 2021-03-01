/*
 * This program serves a specific purpose.
 * 
 * If you are:
 * 1) hosting files on a ZFS partition 
 * 2) reading them on a .NET application...
 * 3) on a Windows machine 
 * 4) from a remote server share (either NFS or SMB)
 * 5) and you're seeing strange exception on a select few files
 * 
 * It may be because one of the date attributes (e.g. last write, last access, or creation)  is null or out of range and cannot be converted
 * to a W32Time value. This means you'll get an exception each time you try to retrieve
 * those properties from .NET Core on Windows
 * 
 * TODO: File an upstream fix for the actual .NET Core project.
 * 
 */
using System;
using System.IO;
using System.Linq;

namespace FileDateFix
{
    class Program
    {
        static int entryCount = 0;
        static int entryFixCount = 0;
        static void Main(string[] args)
        {

            if (args.Length > 0 && args[0] != null)
                enumFiles(args[0]);
            else
            {
                Console.WriteLine("No Action.\nSpecify a directory path");
                return;
            }

            Console.WriteLine("Total Files Scanned: {0}\nFixes Applied:{1}", entryCount, entryFixCount);

        }

        public static void enumFiles(string path)
        {

            foreach (var d in System.IO.Directory.GetFiles(path))
            {
                if (!CheckFileDateAttrib(d, FileDateAttr.Creation)) 
                    FixFileDateAttrib(d,FileDateAttr.Creation);
                
                if (!CheckFileDateAttrib(d, FileDateAttr.LastAccess)) 
                    FixFileDateAttrib(d,FileDateAttr.LastAccess);
                
                if (!CheckFileDateAttrib(d, FileDateAttr.LastWrite)) 
                    FixFileDateAttrib(d,FileDateAttr.LastWrite);

                entryCount++;
            }

            foreach (var d in System.IO.Directory.GetDirectories(path))
            {
                Console.Write(".");
                enumFiles(d);
            }

        }

        public enum FileDateAttr
        {
            LastAccess,
            Creation,
            LastWrite
        }
        public static bool CheckFileDateAttrib(string Path, FileDateAttr fileattr)
        {
            try
            {
                switch (fileattr)
                {
                    case FileDateAttr.Creation:     System.IO.File.GetCreationTimeUtc(Path);    break;
                    case FileDateAttr.LastAccess:   System.IO.File.GetLastAccessTimeUtc(Path);  break;
                    case FileDateAttr.LastWrite:    System.IO.File.GetLastWriteTimeUtc(Path);   break;
                }
                return true;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static void FixFileDateAttrib(string Path, FileDateAttr fileattr)
        {
            switch (fileattr)
            {
                case FileDateAttr.Creation: System.IO.File.SetCreationTimeUtc(Path, DateTime.UtcNow); break;
                case FileDateAttr.LastAccess: System.IO.File.SetLastAccessTimeUtc(Path, DateTime.UtcNow); break;
                case FileDateAttr.LastWrite: System.IO.File.SetLastWriteTimeUtc(Path, DateTime.UtcNow); break;
            }
            entryFixCount++;
            Console.WriteLine("Fixed {1}",Path);
        }
    }
}
