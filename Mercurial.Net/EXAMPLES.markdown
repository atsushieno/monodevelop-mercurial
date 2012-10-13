The examples that are shipped as part of the Mercurial.Net
[repository][1] requires [LINQPad][2] to run, a free tool made
by the author of [C# 40 in a Nutshell][3], a [O'Reilly][4]
book about C# 4.0, LINQ and various other topics.

Note that you need [LINQPad][2] version 4.27.1 or higher, as
the scripts here rely on a relatively new feature where the script
can discover its location on disk. This is used so that the scripts work against the
repository of Mercurial.Net automatically.

Ensure that you load up the solution file in Visual Studio 2010,
and do a full rebuild for the Debug target before testing any
of the examples. They rely on the binary produced as part of the
build output.

The examples numbered 100 and upwards are excercising the GUI client,
TortoiseHg.

  [1]: http://mercurialnet.codeplex.com/
  [2]: http://www.linqpad.net/
  [3]: http://www.albahari.com/nutshell/
  [4]: http://oreilly.com/