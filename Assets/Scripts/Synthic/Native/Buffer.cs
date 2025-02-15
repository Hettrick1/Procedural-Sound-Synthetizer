using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace Synthic.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct BufferHandler<T> : IDisposable where T : unmanaged
    {
        public int Length { get; private set; }
        public T* Pointer { get; private set; }
        public bool Allocated => (IntPtr)Pointer != IntPtr.Zero;

        public BufferHandler(int length)
        {
            Length = length;
            Pointer = (T*)UnsafeUtility.Malloc(Length * sizeof(T),
                UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
        }

        public void Dispose()
        {
            if (!Allocated) return;
            UnsafeUtility.Free(Pointer, Allocator.Persistent);
            Pointer = (T*)IntPtr.Zero;
        }
        public void CopyTo(T[] managedArray)
        {
            if (!Allocated) throw new ObjectDisposedException("Cannot copy. Buffer has been disposed");
            int length = Math.Min(managedArray.Length, Length);
            GCHandle gcHandle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
            UnsafeUtility.MemCpy((void*)gcHandle.AddrOfPinnedObject(), Pointer, length * sizeof(T));
            gcHandle.Free();
        }
        public void CopyTo(BufferHandler<T> buffer)
        {
            if (!Allocated) throw new ObjectDisposedException("Cannot copy. Source buffer has been disposed");
            if (!buffer.Allocated) throw new ObjectDisposedException("Cannot copy. Dest buffer has been disposed");
            int length = Math.Min(Length, buffer.Length);
            UnsafeUtility.MemCpy(Pointer, buffer.Pointer, length * sizeof(T));
        }
        public T this[int index]
        {
            get
            {
                CheckAndThrow(index);
                return *(T*)((long)Pointer + index * sizeof(T));
            }

            set
            {
                CheckAndThrow(index);
                *(T*)((long)Pointer + index * sizeof(T)) = value;
            }
        }
        private void CheckAndThrow(int index)
        {
            if (!Allocated) throw new ObjectDisposedException("Buffer is disposed");
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException($"index:{index} out of range:0-{Length}");
        }
    }
}
