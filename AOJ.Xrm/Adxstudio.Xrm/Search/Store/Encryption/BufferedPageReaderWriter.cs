/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.Linq;

	/// <summary>
	/// The buffered page reader writer.
	/// </summary>
	public abstract class BufferedPageReaderWriter : IBufferedPageReader, IBufferedPageWriter
	{
		/// <summary>
		/// The buffer pointer.
		/// </summary>
		protected int bufferPointer;

		/// <summary>
		/// The file pointer.
		/// </summary>
		protected long filePointer;

		/// <summary>
		/// The file size.
		/// </summary>
		protected long fileSize;

		/// <summary>
		/// The page buffer.
		/// </summary>
		protected byte[] pageBuffer;

		/// <summary>
		/// The page is changed.
		/// </summary>
		protected bool pageIsChanged;

		/// <summary>
		/// The page number.
		/// </summary>
		protected long pageNumber;

		/// <summary>
		/// Gets the length.
		/// </summary>
		public abstract long Length { get; }

		/// <summary>
		/// Gets the page size.
		/// </summary>
		public abstract int PageSize { get; }

		/// <summary>
		/// Clones the object.
		/// </summary>
		/// <returns>copy of instance</returns>
		public abstract object Clone();

		/// <summary>
		/// Gets the length.
		/// </summary>
		long IBufferedPageReader.Length
		{
			get
			{
				return Length;
			}
		}

		/// <summary>
		/// Gets the position.
		/// </summary>
		long IBufferedPageReader.Position
		{
			get
			{
				return filePointer;
			}
		}

		/// <summary>
		/// Gets the length.
		/// </summary>
		long IBufferedPageWriter.Length
		{
			get
			{
				return Length;
			}
		}

		/// <summary>
		/// Gets the position.
		/// </summary>
		long IBufferedPageWriter.Position
		{
			get
			{
				return filePointer;
			}
		}

		/// <summary>
		/// The dispose.
		/// </summary>
		public virtual void Dispose()
		{
			Flush();
		}

		/// <summary>
		/// The flush.
		/// </summary>
		public void Flush()
		{
			WriteCurrentPage();
		}

		/// <summary>
		/// The read byte.
		/// </summary>
		/// <returns>
		/// The <see cref="byte"/>.
		/// </returns>
		public byte ReadByte()
		{
			if (bufferPointer == PageSize)
			{
				pageNumber++;
				bufferPointer = 0;

				ReadCurrentPage();
			}

			var @byte = pageBuffer[bufferPointer++];
			filePointer++;

			return @byte;
		}

		/// <summary>
		/// The read bytes.
		/// </summary>
		/// <param name="destination">
		/// The destination.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		public void ReadBytes(byte[] destination, int offset, int length)
		{
			// TODO: Optimize with Buffer.BlockCopy
			for (var i = 0; i < length; i++)
			{
				var @byte = ReadByte();

				destination[offset + i] = @byte;
			}
		}

		/// <summary>
		/// The read page.
		/// </summary>
		/// <param name="pageNumber">
		/// The page number.
		/// </param>
		/// <param name="destination">
		/// The destination.
		/// </param>
		public abstract void ReadPage(long pageNumber, byte[] destination);

		/// <summary>
		/// The write byte.
		/// </summary>
		/// <param name="byte">
		/// The byte.
		/// </param>
		public void WriteByte(byte @byte)
		{
			if (bufferPointer == PageSize)
			{
				WriteCurrentPage();
				bufferPointer = 0;
				pageNumber++;
				ReadCurrentPage();
			}

			pageBuffer[bufferPointer++] = @byte;
			pageIsChanged = true;

			filePointer++;

			if (filePointer > fileSize)
			{
				fileSize = filePointer;
			}
		}

		/// <summary>
		/// The write bytes.
		/// </summary>
		/// <param name="source">
		/// The source.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		public void WriteBytes(byte[] source, int offset, int length)
		{
			var range = source.Skip(offset).Take(length).ToArray();

			foreach (byte @byte in range)
			{
				WriteByte(@byte);
			}
		}

		/// <summary>
		/// The write page.
		/// </summary>
		/// <param name="pageNumber">
		/// The page number.
		/// </param>
		/// <param name="source">
		/// The source.
		/// </param>
		public abstract void WritePage(long pageNumber, byte[] source);

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void IBufferedPageReader.Seek(long position)
		{
			Seek(position);
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void IBufferedPageWriter.Seek(long position)
		{
			Seek(position);
		}

		/// <summary>
		/// The initialize.
		/// </summary>
		protected void Initialize()
		{
			pageBuffer = new byte[PageSize];
			pageNumber = -1;
			Seek(0);
		}

		/// <summary>
		/// The read current page.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Invalid Operation Exception
		/// </exception>
		protected void ReadCurrentPage()
		{
			if (pageIsChanged)
			{
				throw new InvalidOperationException("Save page before read");
			}

			if (pageNumber * PageSize >= Length)
			{
				// There are no more data. Fill next page with zeros
				Array.Clear(pageBuffer, 0, PageSize);
			}
			else
			{
				ReadPage(pageNumber, pageBuffer);
			}

			pageIsChanged = false;
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		protected void Seek(long position)
		{
			Flush();

			var pageNumber = (int)Math.Floor((double)position / PageSize);

			filePointer = position;
			bufferPointer = (int)(position - (pageNumber * PageSize));

			if (pageNumber == this.pageNumber)
			{
				return;
			}

			this.pageNumber = pageNumber;

			ReadCurrentPage();
		}

		/// <summary>
		/// The write current page.
		/// </summary>
		protected void WriteCurrentPage()
		{
			if (!pageIsChanged)
			{
				return;
			}

			WritePage(pageNumber, pageBuffer);

			pageIsChanged = false;
		}
	}
}
