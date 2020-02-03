using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IO
{
	/// <summary>
	/// Provides a streaming UTF-32 source over a streaming character source
	/// </summary>
	public sealed class Utf32Enumerable : IEnumerable<int>
	{
		IEnumerable<char> _input;
		/// <summary>
		/// Constructs a new instance of a UTF-32 enumerator
		/// </summary>
		/// <param name="input">The input source</param>
		public Utf32Enumerable(IEnumerable<char> input)
		{
			if (null == input)
				throw new ArgumentNullException("input");
			_input = input;
		}
		/// <summary>
		/// Retrieves an enumeration of UTF-32 values
		/// </summary>
		/// <returns>A new enumerator</returns>
		public IEnumerator<int> GetEnumerator()
		{
			return new _Enumerator(_input.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		private sealed class _Enumerator : IEnumerator<int>
		{
			const int _BeforeStart = -2;
			const int _AfterEnd = -1;
			const int _Disposed = -3;
			const int _Enumerating = 0;
			int _current;
			int _state;
			IEnumerator<char> _input;
			public _Enumerator(IEnumerator<char> input)
			{
				_input = input;
				_state = _BeforeStart;
				_current = -1;
			}

			public int Current { 
				get {
					if(0>_state)
					{
						switch(_state)
						{
							case _BeforeStart:
								throw new InvalidOperationException("The cursor is before the start of the enumeration");
							case _AfterEnd:
								throw new InvalidOperationException("The cursor is after the end of the enumeration");
						}
						_CheckDisposed();
					}
					return _current;
				}
			}
			void _CheckDisposed()
			{
				if (_Disposed == _state)
					throw new ObjectDisposedException("Utf32Enumerator");
			}
			object IEnumerator.Current { get { return Current; } }

			public void Dispose()
			{
				if(_Disposed!=_state)
				{
					_input.Dispose();
					_state = _Disposed;
				}
			}

			public bool MoveNext()
			{
				if(0>_state)
				{
					switch (_state)
					{
						case _AfterEnd:
							return false;
						case _BeforeStart:
							_state = _Enumerating;
							break;
						default:
							_CheckDisposed();
							return false;
					}
				}
				if(!_input.MoveNext())
				{
					_state = _AfterEnd;
					return false;
				}
				var chh = _input.Current;
				_current = chh;
				if(char.IsHighSurrogate(chh))
				{
					if (!_input.MoveNext())
						throw new IOException("Unexpected end of stream looking for Unicode surrogate.");
					_current = char.ConvertToUtf32(chh, _input.Current);
				}
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
