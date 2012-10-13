<Query Kind="Program">
  <Reference Relative="Mercurial.Net.Tests\bin\Debug\Mercurial.Net.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net.Tests\bin\Debug\Mercurial.Net.dll</Reference>
  <Reference Relative="Mercurial.Net.Tests\bin\Debug\nunit.framework.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net.Tests\bin\Debug\nunit.framework.dll</Reference>
  <Reference Relative="Mercurial.Net.Tests\bin\Debug\Mercurial.Net.Tests.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net.Tests\bin\Debug\Mercurial.Net.Tests.dll</Reference>
  <Namespace>Mercurial.Tests</Namespace>
</Query>

void Main()
{
	var source = new AllFilesInProjectsAreUtf8Encoded();
    var allFiles = source.AllSourceFilesInProjectsInFolder(Path.GetDirectoryName(Util.CurrentQueryPath));
    
    foreach (var filename in allFiles)
    {
        var bytes = File.ReadAllBytes(filename);
        if (bytes.Length >= 2)
        {
            if (bytes[0] == 0xfe && bytes[1] == 0xff)
            {
                FixFile(filename, Encoding.Unicode);
                continue;
            }
            if (bytes[0] == 0xff && bytes[1] == 0xfe)
            {
                FixFile(filename, Encoding.Unicode);
                continue;
            }
        }
        if (bytes.Length >= 3)
        {
            if (bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
            {
                FixFile(filename, Encoding.UTF8);
                continue;
            }
        }
        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff)
            {
                FixFile(filename, Encoding.UTF32);
                continue;
            }
            if (bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0 && bytes[3] == 0)
            {
                FixFile(filename, Encoding.UTF32);
                continue;
            }
        }
    }
}

public static void FixFile(string filename, Encoding encoding)
{
    Debug.WriteLine(filename);
    string contents = encoding.GetString(File.ReadAllBytes(filename));
    byte[] bytes = Encoding.UTF8.GetBytes(contents).Skip(3).ToArray();
    File.WriteAllBytes(filename, bytes);
}