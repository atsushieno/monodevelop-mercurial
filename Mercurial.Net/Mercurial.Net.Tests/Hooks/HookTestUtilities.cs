using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Mercurial.Net.Tests.Hook;

namespace Mercurial.Tests.Hooks
{
    public static class HookTestUtilities
    {
        public static int Count(this string text, string textToCountOccurancesOf)
        {
            int originalLength = text.Length;
            text = text.Replace(textToCountOccurancesOf, string.Empty);
            int replacedLength = text.Length;

            return (originalLength - replacedLength) / textToCountOccurancesOf.Length;
        }

        public static void SetHook(this Repository repository, string hookName, params string[] arguments)
        {
            arguments = new string[] { hookName }.Concat(arguments ?? new string[0]).ToArray();
            string argumentString = string.Join(" ", arguments.Select(s => "\"" + s + "\"").ToArray());
            string[] lines;
            string hgrcPath = Path.Combine(Path.Combine(repository.Path, ".hg"), "hgrc");
            if (File.Exists(hgrcPath))
                lines = File.ReadAllLines(hgrcPath);
            else
                lines = new string[0];

            string hookCommand = string.Format(
                CultureInfo.InvariantCulture, "{0}=\"{1}\" {2}", 
                hookName,
                typeof(Program).Assembly.Location,
                argumentString);
            var modifiedLines = new List<string>();
            bool inHooks = false;
            bool hookAdded = false;
            foreach (string line in lines)
            {
                if (inHooks)
                {
                    if (line.StartsWith("["))
                    {
                        if (!hookAdded)
                        {
                            modifiedLines.Add(hookCommand);
                            hookAdded = true;
                        }
                        modifiedLines.Add(line);
                        inHooks = false;
                    }
                    else
                    {
                        if (line.StartsWith(hookName + "="))
                        {
                            modifiedLines.Add(hookCommand);
                            hookAdded = true;
                        }
                        else
                            modifiedLines.Add(line);
                    }
                }
                else
                {
                    if (line == "[hooks]")
                    {
                        modifiedLines.Add(line);
                        inHooks = true;
                    }
                }
            }
            if (!hookAdded)
            {
                if (!inHooks)
                    modifiedLines.Add("[hooks]");

                modifiedLines.Add(hookCommand);
            }

            File.WriteAllLines(hgrcPath, modifiedLines.ToArray());
        }
    }
}
