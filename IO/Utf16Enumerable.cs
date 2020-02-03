using System;
using System.Collections;
using System.Collections.Generic;

namespace IO
{
	/// <summary>
	/// Provides a streaming UTF-16 source over a streaming UTF-32 source
	/// </summary>
	public sealed class Utf16Enumerable : IEnumerable<char>
	{
		IEnumerable<int> _input;
		/// <summary>
		/// Constructs a new instance of a UTF-32 enumerator
		/// </summary>
		/// <param name="input">The input source</param>
		public Utf16Enumerable(IEnumerable<int> input)
		{
			if (null == input)
				throw new ArgumentNullException("input");
			_input = input;
		}
		/// <summary>
		/// Retrieves an enumeration of UTF-16 values
		/// </summary>
		/// <returns>A new enumerator</returns>
		public IEnumerator<char> GetEnumerator()
		{
			return new _Enumerator(_input.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		private sealed class _Enumerator : IEnumerator<char>
		{
			const int _BeforeStart = -2;
			const int _AfterEnd = -1;
			const int _Disposed = -3;
			const int _Enumerating = 0;
			const int _EnumerateLow = 1;
			string _current;
			int _state;
			IEnumerator<int> _input;
			public _Enumerator(IEnumerator<int> input)
			{
				_input = input;
				_state = _BeforeStart;
				_current = null;
			}

			public char Current {
				get {
					if (0 > _state)
					{
						switch (_state)
						{
							case _BeforeStart:
								throw new InvalidOperationException("The cursor is before the start of the enumeration");
							case _AfterEnd:
								throw new InvalidOperationException("The cursor is after the end of the enumeration");
						}
						_CheckDisposed();
					}
					else if (_EnumerateLow == _state)
						return _current[1];
					return _current[0];
				}
			}
			void _CheckDisposed()
			{
				if (_Disposed == _state)
					throw new ObjectDisposedException("Utf16Enumerator");
			}
			object IEnumerator.Current { get { return Current; } }

			public void Dispose()
			{
				if (_Disposed != _state)
				{
					_input.Dispose();
					_state = _Disposed;
				}
			}

			public bool MoveNext()
			{
				if (0 > _state)
				{
					switch (_state)
					{
						case _AfterEnd:
							return false;
						case _BeforeStart:
							if (!_input.MoveNext())
							{
								_state = _AfterEnd;
								return false;
							}
							_state = _Enumerating;
							_current = char.ConvertFromUtf32(_input.Current);
							return true;
						default:
							_CheckDisposed();
							return false;
					}
				}
				if(_EnumerateLow != _state && 2 == _current.Length)
				{
					_state = _EnumerateLow;
					return true;
				}
				_state = _Enumerating;
				if (!_input.MoveNext())
				{
					_state = _AfterEnd;
					return false;
				}
				_current = char.ConvertFromUtf32(_input.Current);
				return true;
			}

			public void Reset()
			{
				_CheckDisposed();
				_input.Reset();
				_state = _BeforeStart;
			}
		}
	}
}
