using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mercurial.Hooks;

namespace Mercurial.Net.Tests.Hook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            File.WriteAllText(@"c:\temp\test.txt", "Test");
            try
            {
                IMercurialControllingHook controller = null;
                string hookName = args[0];
                args = args.Skip(1).ToArray();
                switch (hookName)
                {
                    case "commit":
                        controller = DumpHookInformation<MercurialCommitHook>(args);
                        break;

                    case "incoming":
                        controller = DumpHookInformation<MercurialIncomingHook>(args);
                        break;

                    case "changegroup":
                        controller = DumpHookInformation<MercurialChangeGroupHook>(args);
                        break;

                    case "precommit":
                        controller = DumpHookInformation<MercurialPreCommitHook>(args);
                        break;

                    case "outgoing":
                        controller = DumpHookInformation<MercurialPreOutgoingHook>(args);
                        break;

                    case "prechangegroup":
                        controller = DumpHookInformation<MercurialPreChangegroupHook>(args);
                        break;

                    case "prelistkeys":
                        controller = DumpHookInformation<MercurialPreListKeysHook>(args);
                        break;

                    case "preoutgoing":
                        controller = DumpHookInformation<MercurialPreOutgoingHook>(args);
                        break;

                    case "prepushkey":
                        controller = DumpHookInformation<MercurialPrePushKeyHook>(args);
                        break;

                    case "pretag":
                        controller = DumpHookInformation<MercurialPreTagHook>(args);
                        break;

                    case "pretxnchangegroup":
                        controller = DumpHookInformation<MercurialPreTransactionChangegroupHook>(args);
                        break;

                    case "pretxncommit":
                        controller = DumpHookInformation<MercurialPreTransactionCommitHook>(args);
                        break;

                    case "preupdate":
                        controller = DumpHookInformation<MercurialPreUpdateHook>(args);
                        break;

                    case "listkeys":
                        controller = DumpHookInformation<MercurialListKeysHook>(args);
                        break;

                    case "pushkey":
                        controller = DumpHookInformation<MercurialPushKeyHook>(args);
                        break;

                    case "tag":
                        controller = DumpHookInformation<MercurialTagHook>(args);
                        break;

                    case "update":
                        controller = DumpHookInformation<MercurialUpdateHook>(args);
                        break;

                    default:
                        if (hookName.StartsWith("pre-"))
                            controller = DumpHookInformation<MercurialPreCommandHook>(args);
                        else if (hookName.StartsWith("post-"))
                            controller = DumpHookInformation<MercurialPostCommandHook>(args);
                        else
                        {
                            Console.Error.WriteLine("unknown hook type: " + args[0]);
                            Environment.Exit(1);
                        }
                        break;
                }

                if (controller != null)
                {
                    if (args.Length == 1 && args[0] == "fail")
                    {
                        Console.Out.WriteLine("failing");
                        controller.TerminateHookAndCancelCommand(1, "failing hook " + hookName);
                    }
                    else if (args.Length == 1 && args[0] == "ok")
                        controller.TerminateHookAndProceed();
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.Error.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);

                    ex = ex.InnerException;
                }
                Environment.Exit(1);
            }
        }

        private static IMercurialControllingHook DumpHookInformation<T>(string[] args)
            where T : new()
        {
            var hook = new T();
            var writer = GetOutputWriter(args);
            for (int index = 0; index < args.Length; index++)
                writer.WriteLine("hook-arg.#" + index + "=" + args[index]);

            foreach (var property in typeof(T).GetProperties())
            {
                var value = property.GetValue(hook, null);
                var arguments = value as MercurialCommandHookArgumentsCollection;
                var dictionary = value as MercurialCommandHookDictionary;
                var list = value as MercurialCommandHookPatternCollection;
                if (arguments != null)
                {
                    for (int index = 0; index < arguments.Count; index++)
                        writer.WriteLine("arg.#{0}={1}", index, arguments[index]);
                }
                else if (dictionary != null)
                {
                    foreach (string key in dictionary.Keys)
                        writer.WriteLine("opt.{0}={1}", key, dictionary[key]);
                }
                else if (list != null)
                {
                    for (int index = 0; index < list.Count; index++)
                        writer.WriteLine("pattern.{0}={1}", index, list[index]);
                }
                else
                    writer.WriteLine("{0}:{1}", property.Name, value);
            }
            DumpEnvironmentVariables(args);
            return hook as IMercurialControllingHook;
        }

        private static TextWriter GetOutputWriter(string[] args)
        {
            TextWriter writer;
            if (args.Contains("fail"))
                writer = Console.Error;
            else
                writer = Console.Out;
            return writer;
        }

        private static void DumpEnvironmentVariables(string[] args)
        {
            TextWriter writer = GetOutputWriter(args);
            foreach (string name in Environment.GetEnvironmentVariables().Keys.Cast<string>().Where(name => name.StartsWith("HG_")))
                writer.WriteLine(name + "=" + Environment.GetEnvironmentVariable(name));
        }
    }
}
