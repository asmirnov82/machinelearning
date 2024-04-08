// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Data.Analysis
{
    internal sealed class NativeMemoryList<T> : IEnumerable<T>, IDisposable
        where T : unmanaged
    {
        private readonly NativeMemoryBuffer _valueBuffer;

        private long _length;

        public long Length => _length;
        public long Capacity { get; private set; }

        public NativeMemoryList(long length = 0, bool skipZeroClear = false)
        {
            Debug.Assert(length >= 0);

            _length = length;
            Capacity = _length;

            _valueBuffer = new NativeMemoryBuffer(_length * Unsafe.SizeOf<T>(), skipZeroClear);
        }

        public NativeMemoryList(IEnumerable<T> values)
        {
            Debug.Assert(values != null);

            if (values is IReadOnlyCollection<T> collection)
            {
                _length = collection.Count;
                Capacity = _length;

                _valueBuffer = new NativeMemoryBuffer(_length * Unsafe.SizeOf<T>());

                int i = 0;
                foreach (var value in values)
                    _valueBuffer.GetValueByRef<T>(i++) = value;
            }
            else
            {
                _length = 0;
                Capacity = 0;

                _valueBuffer = new NativeMemoryBuffer();

                foreach (var value in values)
                    Add(value);
            }
        }

        public ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert((ulong)index < (ulong)Length);
                return ref _valueBuffer.GetValueByRef<T>(index);
            }
        }

        public void Add(T value)
        {
            if (Length == Capacity)
            {
                EnsureCapacity(Length + 1);
            }
            this[_length++] = value;
        }

        public void AddMany(T value, long count)
        {
            EnsureCapacity(Length + count);

            var i = Length;
            _length += count;

            while (i < _length)
                this[i++] = value;
        }

        public void AddRange(IEnumerable<T> values)
        {
            if (values is IReadOnlyCollection<T> collection)
            {
                var i = _length;
                Resize(_length + collection.Count);

                foreach (var value in collection)
                    this[i++] = value;
            }
            else
            {
                foreach (var value in values)
                    Add(value);
            }
        }

        public void Resize(long length)
        {
            Debug.Assert(length >= 0);

            _valueBuffer.Resize(length * Unsafe.SizeOf<T>());

            Capacity = length;
            _length = length;
        }

        public void EnsureCapacity(long capacity)
        {
            if (capacity <= Capacity)
                return;

            //double the size of internal buffer
            var newCapacity = Math.Max(capacity, Capacity * 2);
            _valueBuffer.Resize(newCapacity * Unsafe.SizeOf<T>());
            Capacity = newCapacity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _length; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            _valueBuffer.Dispose();
        }
    }
}
