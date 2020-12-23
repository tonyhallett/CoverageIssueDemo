using FileSynchronization.FineCodeCoverage.Engine.FileSynchronization;
using FileSynchronization.FineCodeCoverage.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FileSynchronization
{
    class Program
    {
		//a) Run tests with both frameworks in Debug - generates work folder
		//b) Run this in Debug
		static void Main(string[] args)
        {
			var replacesNetFramework = FileSynchronizationUtil.ReplacesDll(true);
			var replacesNetCore = FileSynchronizationUtil.ReplacesDll(false);
			if(!replacesNetFramework && !replacesNetCore)
            {
				var comparableNetFrameworkComparable = FileSynchronizationUtil.GetDllComparableFile(true);
				var comparableNetCoreComparable = FileSynchronizationUtil.GetDllComparableFile(true);
				var same = FileComparer.Singleton.Equals(comparableNetFrameworkComparable, comparableNetCoreComparable);
				throw new Exception($"Surely one of them should have been replaced - issue due to same hashcode: {same}");
            }
		}
    }
	namespace FineCodeCoverage.Engine.Utilities
	{
		internal class FileComparer : IEqualityComparer<ComparableFile>
		{
			public static FileComparer Singleton { get; } = new FileComparer();

			public int GetHashCode(ComparableFile file) => file.GetHashCode();

			public bool Equals(ComparableFile file, ComparableFile otherFile) => file.Equals(otherFile);
		}

		internal class ComparableFile : IEquatable<ComparableFile>
		{
			private readonly int hashCode;

			public FileInfo FileInfo { get; }

			public string RelativePath { get; }

			public override int GetHashCode() => hashCode;

			public bool Equals(ComparableFile other) => hashCode.Equals(other.hashCode);

			public ComparableFile(FileInfo fileInfo, string relativePath)
			{
				FileInfo = fileInfo;
				RelativePath = relativePath;
				var ticks = FileInfo.LastWriteTimeUtc.Ticks;
				hashCode = string.Format("{0}|{1}|{2}", RelativePath, FileInfo.Length, ticks).GetHashCode();
			}
		}
	}
	namespace FineCodeCoverage.Engine.FileSynchronization
	{
		internal static partial class FileSynchronizationUtil
		{
			
			private static string DebugFolderPath
            {
                get
                {
					var thisDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
					var root = thisDir.Parent.Parent.Parent.Parent.Parent;
					return Path.Combine(root.FullName,"Test", "bin", "Debug");
				}
            }
			private static string GetSourceFolder(bool netFrameworkSource)
            {
				return Path.Combine(DebugFolderPath, netFrameworkSource ? "net472" : "netcoreapp3.1");
			}
			public static ComparableFile GetDllComparableFile(bool netFrameworkSource)
            {
				var sourceFolder = GetSourceFolder(netFrameworkSource);
				var srceDir = new DirectoryInfo(Path.GetFullPath(sourceFolder) + '\\');
				var fi = new FileInfo(Path.Combine(sourceFolder, "CoverageIssueDemo.dll"));
				return new ComparableFile(fi, fi.FullName.Substring(srceDir.FullName.Length));
			} 
			public static bool ReplacesDll(bool netFrameworkSource)
            {
				var debugFolder = DebugFolderPath;
				var workFolder = Path.Combine(debugFolder, "fine-code-coverage");
				var sourceFolder = Path.Combine(debugFolder, netFrameworkSource ? "net472" : "netcoreapp3.1");

				var filesToCopy = Synchronize(sourceFolder, workFolder);
				var fileNamesToCopy = filesToCopy.Select(f => f.RelativePath);
				return fileNamesToCopy.Contains("CoverageIssueDemo.dll");
                
            }

			
			public static List<ComparableFile>  Synchronize(string sourceFolder, string destinationFolder)
			{
				var logs = new List<string>();
				var srceDir = new DirectoryInfo(Path.GetFullPath(sourceFolder) + '\\');
				var destDir = new DirectoryInfo(Path.GetFullPath(destinationFolder) + '\\');

				// file lists

				var srceFiles = srceDir.GetFiles("*", SearchOption.AllDirectories).Select(fi => new ComparableFile(fi, fi.FullName.Substring(srceDir.FullName.Length)));
				var destFiles = destDir.GetFiles("*", SearchOption.AllDirectories).Select(fi => new ComparableFile(fi, fi.FullName.Substring(destDir.FullName.Length)));
				
				// copy to dest
				var filesToCopy = srceFiles.Except(destFiles, FileComparer.Singleton).ToList();


				return filesToCopy;
				
			}
		}
	}
}
