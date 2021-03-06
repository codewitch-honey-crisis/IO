﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace IO
{
	/// <summary>
	/// Represents an <see cref="IEnumerable{Char}"/> instance over a file
	/// </summary>
#if IOLIB
	public
#endif
		sealed class FileReaderEnumerable : TextReaderEnumerable
	{
		/// <summary>
		/// Indicates whether or not the instance can create a reader
		/// </summary>
		protected override bool CanCreateReader => true;
		readonly string _filename;
		/// <summary>
		/// Constructs a new instance over a file
		/// </summary>
		public FileReaderEnumerable(string filename)
		{
			if (null == filename) throw new ArgumentNullException("filename");
			if (0 == filename.Length) throw new ArgumentException("The filename must not be empty.", "filename");
			_filename = filename;
		}
		/// <summary>
		/// Creates a text reader over a file
		/// </summary>
		/// <returns>A new text reader</returns>
		protected override TextReader CreateTextReader()
		{
			return File.OpenText(_filename);
		}
	}
	/// <summary>
	/// Represents an <see cref="IEnumerable{Char}"/> instance over stdin
	/// </summary>
#if IOLIB
	public
#endif 
	sealed class ConsoleReaderEnumerable : TextReaderEnumerable
	{
		/// <summary>
		/// Indicates whether or not the instance can create a reader
		/// </summary>
		protected override bool CanCreateReader => false;
		/// <summary>
		/// Constructs a new instance over stdin
		/// </summary>
		public ConsoleReaderEnumerable()
		{
		}
		/// <summary>
		/// Creates a text reader over stdin
		/// </summary>
		/// <returns>A new text reader</returns>
		protected override TextReader CreateTextReader()
		{
			return Console.In;
		}
	}
	/// <summary>
	/// Represents an <see cref="IEnumerable{Char}"/> instance over an URL
	/// </summary>
#if IOLIB
	public
#endif
	sealed class UrlReaderEnumerable : TextReaderEnumerable
	{
		/// <summary>
		/// Indicates whether or not the instance can create a reader
		/// </summary>
		protected override bool CanCreateReader => true;
		readonly string _url;
		/// <summary>
		/// Constructs a new instance over an URL
		/// </summary>
		/// <param name="url"></param>
		public UrlReaderEnumerable(string url)
		{
			if (null == url) throw new ArgumentNullException("url");
			if (0 == url.Length) throw new ArgumentException("The url must not be empty.", "url");
			_url = url;
		}
		/// <summary>
		/// Creates a new <see cref="TextReader"/> from the URL
		/// </summary>
		/// <returns>A text reader over the URL</returns>
		protected override TextReader CreateTextReader()
		{
			var wq = WebRequest.Create(_url);
			var wr = wq.GetResponse();
			return new StreamReader(wr.GetResponseStream());
		}
	}
	/// <summary>
	/// Represents an <see cref="IEnumerable{Char}"/> instance over a <see cref="TextReader" />
	/// </summary>
#if IOLIB
	public
#endif
	abstract class TextReaderEnumerable : IEnumerable<char>
	{
#region _OnceReaderEnumerable
		sealed class _OnceTextReaderEnumerable : TextReaderEnumerable
		{
			TextReader _reader;
			internal _OnceTextReaderEnumerable(TextReader reader)
			{
				_reader = reader;
			}
			protected override TextReader CreateTextReader()
			{
				if (null == _reader)
					throw new NotSupportedException("This method can only be called once.");
				var r = _reader;
				_reader = null;
				return r;
			}
			protected override bool CanCreateReader => false;
		}
#endregion
		/// <summary>
		/// Creates a new instance from an existing text reader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static TextReaderEnumerable FromReader(TextReader reader)
		{
			if (null == reader)
				throw new ArgumentNullException("reader");
			return new _OnceTextReaderEnumerable(reader);
		}
		/// <summary>
		/// Gets the enumerator for this instance
		/// </summary>
		/// <returns>An enumerator</returns>
		public IEnumerator<char> GetEnumerator()
		{
			return new TextReaderEnumerator(this);
		}
		/// <summary>
		/// Indicates whether or not the instance is capable of creating a reading
		/// </summary>
		protected abstract bool CanCreateReader { get; }
		/// <summary>
		/// Creates the <see cref="TextReader" /> source for this instance
		/// </summary>
		/// <returns></returns>
		protected abstract TextReader CreateTextReader();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
		sealed class TextReaderEnumerator : IEnumerator<char>
		{
			TextReaderEnumerable _outer;
			TextReader _reader;
			int _state;
			char _current;
			internal TextReaderEnumerator(TextReaderEnumerable outer)
			{
				_outer = outer;
				_reader = null;
				if (_outer.CanCreateReader)
					Reset();
				else
				{
					_state = -1;
					_reader = _outer.CreateTextReader(); // doesn't really recreate it
				}
			}

			public char Current {
				get {
					switch (_state)
					{
						case -3:
							throw new ObjectDisposedException(GetType().Name);
						case -2:
							throw new InvalidOperationException("The cursor is past the end of input.");
						case -1:
							throw new InvalidOperationException("The cursor is before the start of input.");
					}
					return _current;
				}
			}
			object IEnumerator.Current => Current;

			public void Dispose()
			{
				// Dispose of unmanaged resources.
				_Dispose(true);
				// Suppress finalization.
				GC.SuppressFinalize(this);
			}
			~TextReaderEnumerator()
			{
				_Dispose(false);
			}
			// Protected implementation of Dispose pattern.
			void _Dispose(bool disposing)
			{
				if (null == _reader)
					return;

				if (disposing)
				{
					_reader.Close();
					_reader = null;
					_state = -3;
				}

			}

			public bool MoveNext()
			{
				switch (_state)
				{
					case -3:
						throw new ObjectDisposedException(GetType().Name);
					case -2:
						return false;
				}
				int i = _reader.Read();
				if (-1 == _state &&
					((BitConverter.IsLittleEndian && '\uFEFF' == i) ||
						(!BitConverter.IsLittleEndian && '\uFFFE' == i))) // skip the byte order mark
					i = _reader.Read();
				_state = 0;
				if (-1 == i)
				{
					_state = -2;
					return false;
				}
				_current = unchecked((char)i);
				return true;
			}

			public void Reset()
			{
				// don't bother if we haven't moved.
				if (-1 == _state) return;
				try
				{

					// optimization for streamreader.
					var sr = _reader as StreamReader;
					if (null != sr && null != sr.BaseStream && sr.BaseStream.CanSeek && 0L == sr.BaseStream.Seek(0, SeekOrigin.Begin))
					{
						_state = -1;
						return;
					}
				}
				catch (IOException) { }
				if (!_outer.CanCreateReader)
					throw new NotSupportedException();
				_Dispose(true);
				_reader = _outer.CreateTextReader();
				_state = -1;
			}
		}
	}
}
