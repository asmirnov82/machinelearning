// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Data.Analysis
{
    internal sealed unsafe class NativeMemoryBuffer : IDisposable
    {
        private bool _isDisposed = false;
        private readonly object _lock = new object();

        private byte* _ptr;

        internal byte* Ptr { get => _ptr; }

        public long Size { get; private set; }

        public NativeMemoryBuffer(long size = 0, bool skipZeroClear = false)
        {
            Debug.Assert(size >= 0);

            Size = size;

            if (size == 0)
            {
                _ptr = (byte*)Unsafe.AsPointer(ref Unsafe.NullRef<byte>());
                return;
            }

            _ptr = NativeMemoryUtility.Allocate(size, skipZeroClear);

            GC.AddMemoryPressure(size);
        }

        public void Resize(long size, bool skipZeroClear = true)
        {
            Debug.Assert(size >= 0);

            lock (_lock)
            {
                if (Size == size || _isDisposed)
                    return;

                var newPtr = NativeMemoryUtility.Reallocate(Ptr, Size, size, skipZeroClear);

                var delta = size - Size;

                if (delta > 0)
                    GC.AddMemoryPressure(delta);
                else
                    GC.RemoveMemoryPressure(-delta);

                _ptr = newPtr;
                Size = size;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueByRef<T>(long index)
            where T : unmanaged
        {
            var memoryIndex = index * Unsafe.SizeOf<T>();
            return ref Unsafe.AsRef<T>(Ptr + memoryIndex);
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        ~NativeMemoryBuffer()
        {
            DisposeInternal();
        }

        private void DisposeInternal()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    return;

                if (Unsafe.IsNullRef(ref Unsafe.AsRef<byte>(Ptr)))
                    return;

                NativeMemoryUtility.Free(Ptr);
                _ptr = null;

                GC.RemoveMemoryPressure(Size);

                _isDisposed = true;
            }
        }
    }
}
