using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Utils
{
    /// <summary>
    /// Represents an extremely aggressive, fixed-capacity data buffer.
    /// Sacrifices ALL memory safety, bounds checking, and initialization guarantees 
    /// to achieve maximum possible native-like throughput.
    /// </summary>
    /// <remarks>
    /// <b>DANGER:</b> This class provides zero safety nets. 
    /// Exceeding <see cref="Capacity"/> will corrupt contiguous memory and cause Access Violations.
    /// Using reference types (classes) with <see cref="Clear"/> will cause memory leaks.
    /// </remarks>
    /// <typeparam name="T">The type of elements stored in the collection.</typeparam>
    public sealed class Arcane<T>
    {
        private readonly T[] elements;
        private int _index = 0;

        /// <summary>
        /// Current logical size of the collection.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index;
        }

        public readonly int Capacity;

        /// <summary>
        /// Allocates the buffer without zeroing out memory (for unmanaged types).
        /// </summary>
        public Arcane(int capacity)
        {
            // Skips zero-initialization for value types, saving significant CPU cycles.
            // The array will contain raw memory garbage until overwritten.
            this.elements = GC.AllocateUninitializedArray<T>(capacity);
            Capacity = capacity;
        }

        /// <summary>
        /// Direct memory access to the element at the specified index. NO bounds checking.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(elements), index);
        }

        /// <summary>
        /// Appends an element via raw pointer offset calculation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T element)
        {
            ref T start = ref MemoryMarshal.GetArrayDataReference(elements);
            Unsafe.Add(ref start, _index++) = element;
        }

        /// <summary>
        /// Shifts the tail pointer back. Does not clear memory. NO underflow checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            _index--;
        }

        /// <summary>
        /// Instantly resets the logical size to 0. 
        /// WARNING: Will leak memory if T is a reference type (class/string) and the buffer is kept alive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _index = 0;
        }

        /// <summary>
        /// Copies a block of memory directly into the buffer. NO bounds checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items)
        {
            ref T destination = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(elements), _index);

            // Constructing a Span directly over the raw memory bypasses array boundaries validation,
            // allowing the underlying memmove/SIMD copy to execute instantly.
            items.CopyTo(MemoryMarshal.CreateSpan(ref destination, items.Length));
            _index += items.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T element) => AsSpan().Contains(element);

        /// <summary>
        /// Creates a Span bypassing standard array slice bounds checks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference(elements), _index);
        }
    }
}