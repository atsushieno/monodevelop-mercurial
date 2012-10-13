using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    [Category("API")]
    public class AllFilesInProjectsAreUtf8Encoded
    {
        public static IEnumerable<string> AllFilesInProjectsInFolder(string sourceLocation)
        {
            return
                from filename in Directory.GetFiles(sourceLocation, "*.*", SearchOption.AllDirectories)
                let lowerFilename = filename.ToLower()
                where !lowerFilename.Contains(@"\.hg\")
                      && !lowerFilename.Contains(@"\help\")
                      && !lowerFilename.Contains(@"\bin\")
                      && !lowerFilename.Contains(@"\obj\")
                      && !lowerFilename.Contains(@"\packages\")
                      && !lowerFilename.Contains(@"\_resharper.")
                select filename;
        }

        public static IEnumerable<string> AllFilesInProjects()
        {
            return AllFilesInProjectsInFolder(GetSourceLocation());
        }

        private static string GetSourceLocation()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string sourceLocation = Path.GetFullPath(Path.Combine(assemblyLocation, "..\\..\\..\\..\\"));
            return sourceLocation;
        }

        public IEnumerable<string> AllSourceFilesInProjectsInFolder(string sourceLocation)
        {
            var re = new Regex(@"\.(cs|sln|linq|nuspec|xml|hgignore|markdown|csproj|tt)$", RegexOptions.IgnoreCase);

            return
                from filename in AllFilesInProjectsInFolder(sourceLocation)
                where re.Match(filename).Success
                select filename;
        }

        public IEnumerable<string> AllSourceFilesInProjects()
        {
            return AllSourceFilesInProjectsInFolder(GetSourceLocation());
        }

        public IEnumerable<string> AllUnknownExtensions()
        {
            var re = new Regex(@"\.(cs|csproj|sln|linq|nuspec|nunit|hgignore|hgtags|fxcop|stylecop|suo|user|snk|markdown|xml|resharper|shfbproj|cache|dll|ini|config|metaproj|tmp|proj|tt)$", RegexOptions.IgnoreCase);

            return
                (from filename in AllFilesInProjects()
                 where filename != null && !re.Match(filename).Success
                 let extension = Path.GetExtension(filename)
                 where !StringEx.IsNullOrWhiteSpace(extension)
                 let lowerExtension = extension.ToLower()
                 where lowerExtension.Length > 0
                 select lowerExtension).Distinct();
        }

        [Test]
        [TestCaseSource("AllUnknownExtensions")]
        public void EnsureNoUnknownFileExtensionsSlipsBy(string extension)
        {
            if (!StringEx.IsNullOrWhiteSpace(extension))
                Assert.Fail("unknown file extension {0}", extension);
        }

        [Test]
        [TestCaseSource("AllSourceFilesInProjects")]
        public void EnsureAllFilesInProjectsAreUtf8Encoded(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            if (bytes.Length >= 2)
            {
                if (bytes[0] == 0xfe && bytes[1] == 0xff)
                    Assert.Fail("File {0} is UTF-16, big-endian encoded", filename);
                if (bytes[0] == 0xff && bytes[1] == 0xfe)
                    Assert.Fail("File {0} is UTF-16, little-endian encoded", filename);
            }
            if (bytes.Length >= 3)
            {
                if (bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
                    Assert.Fail("File {0} is UTF-8-encoded, but with byte-order-mark", filename);
            }
            
            if (bytes.Length < 4)
                return;
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff)
                Assert.Fail("File {0} is UTF-32, big-endian encoded", filename);
            if (bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0 && bytes[3] == 0)
                Assert.Fail("File {0} is UTF-32, big-endian encoded", filename);
        }
    }
}