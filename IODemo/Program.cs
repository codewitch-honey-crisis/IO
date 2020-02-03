using IO;
using System;


namespace IODemo
{
	class Program
	{
		static void Main()
		{
			// Lifetime is automatically managed
			var fr = new FileReaderEnumerable(@"..\..\Program.cs");
			
			// file open on enumeration start
			foreach (var ch in fr)
				Console.Write(ch);
			// file close when done
			Console.WriteLine();
			Console.WriteLine();

			var ur = new UrlReaderEnumerable(@"http://www.google.com");
			var i = 0;
			// url fetch on enumeration start
			foreach (var ch in ur)
			{
				if(79==i)
				{
					Console.Write("...");
					break;
				}
				Console.Write(ch);
				++i;
			}
			// url close on done
			Console.WriteLine();
			Console.WriteLine();

			// put in a string with a 21-bit unicode value
			var test = "This is a test \U0010FFEE";
			var uni = new Utf32Enumerable(test);
			foreach(var uch in uni)
			{
				// console will mangle, but 
				// do it anyway
				var str = char.ConvertFromUtf32(uch);
				Console.Write(str);
			}
			Console.WriteLine();
			Console.WriteLine();

		}
	}
}
