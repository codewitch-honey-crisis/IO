using IO;
using System;
using System.IO;

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
			var u32 = new Utf32Enumerable(test);
			Console.Write("Enum Utf32Enumerable: ");
			foreach(var uch in u32)
			{
				// console will mangle, but 
				// do it anyway
				var str = char.ConvertFromUtf32(uch);
				Console.Write(str);
			}
			Console.WriteLine();
			Console.WriteLine();
			Console.Write("Enum Utf16Enumerable: ");
			var u16 = new Utf16Enumerable(u32);
			foreach (var ch in u16)
			{
				Console.Write(ch);
			}
			Console.WriteLine();
			Console.WriteLine();

			var reader = new StringReader("This is a demo of TextReaderEnumerable");
			foreach (char ch in TextReaderEnumerable.FromReader(reader))
				Console.Write(ch);
			Console.WriteLine();
			Console.WriteLine();

		}
	}
}
