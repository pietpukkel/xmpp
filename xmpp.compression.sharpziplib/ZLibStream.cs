// ZLibStream.cs
//
//XMPP .NET Library Copyright (C) 2008 Dieter Lunn
//
//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 3 of the License, or (at your option)
//any later version.
//
//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//
//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using xmpp.attributes;
using xmpp.logging;

namespace xmpp.compression.sharpziplib
{
	[Compression("zlib", typeof(ZLibStream))]
	public class ZLibStream : Stream
	{
		private Stream _innerStream;
		private Inflater _in;
		private Deflater _out;
		private byte[] _inBuff;
		private byte[] _outBuff;

		public ZLibStream(Stream inner, Inflater inflater, int buffSize)
		{
			_innerStream = inner;
			_in = inflater;
			_inBuff = new byte[buffSize];
			_outBuff = _inBuff;
			_out = new Deflater();
		}
		
		public ZLibStream(Stream inner, Inflater inflater) : this(inner, inflater, 4096)
		{
		}
		
		public ZLibStream(Stream inner) : this(inner, new Inflater())
		{
		}
		
		public override bool CanRead {
			get { return _innerStream.CanRead; }
		}

		public override bool CanWrite {
			get { return _innerStream.CanWrite; }
		}
		
		public override bool CanSeek {
			get { return false; }
		}
		
		public override long Length {
			get { return _inBuff.Length; }
		}
		
		public override long Position {
			get { return _innerStream.Position; }
			set { throw new NotSupportedException(); }
		}
		
		public override void Flush ()
		{
			_innerStream.Flush();
		}
		
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			_out.SetInput(buffer, offset, count);
			
			while (!_out.IsNeedingInput)
			{
				int avail = _out.Deflate(_outBuff, 0, _outBuff.Length);
				_innerStream.Write(_outBuff, 0, avail);
			}
		}

		public override void WriteByte (byte value)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback cback, object state)
		{
			throw new NotSupportedException();
		}

		public override void Close ()
		{
			base.Close ();
		}
		
		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback cback, object state)
		{
			_outBuff = buffer;
			return _innerStream.BeginRead(_inBuff, 0, _inBuff.Length, cback, state);
		}

		public override int EndRead (IAsyncResult async_result)
		{
			int avail = _innerStream.EndRead(async_result);
			
			Logger.Debug(this, _inBuff);
			
			_in.SetInput(_inBuff, _inBuff.Length - avail, avail);
			
			_in.Inflate(_outBuff, 0, _outBuff.Length);
			
			Logger.Debug(this, _outBuff);
			
			return avail;
		}
	}
}
