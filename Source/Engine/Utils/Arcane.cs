using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Utils
{
    /// <summary>
    /// Represents a high-performance, fixed-capacity wrapper over a standard managed array.
    /// Optimized for scenarios requiring minimal overhead and maximum throughput by bypassing 
    /// standard safety checks (e.g., bounds checking) during element insertion.
    /// </summary>
    /// <remarks>
    /// <b>Performance Note:</b> This class utilizes <see cref="Unsafe"/> and <see cref="MemoryMarshal"/> 
    /// to achieve near-native execution speeds. 
    /// <b>Safety Warning:</b> This implementation prioritizes speed over memory safety. 
    /// Improper use of indexers or exceeding capacity in <see cref="Add(T)"/> results in 
    /// undefined behavior or Access Violation exceptions.
    /// </remarks>
    /// <typeparam name="T">The type of elements stored in the collection.</typeparam>
    public sealed class Arcane<T> {
        /// <summary>
        /// Internal storage buffer.
        /// </summary>
        private readonly T[] elements;

        /// <summary>
        /// Current logical size of the collection (number of elements added).
        /// </summary>
        private int _index = 0;

        /// <summary>
        /// Gets the total capacity of the underlying storage buffer.
        /// </summary>
        /// <value>The maximum number of elements the <see cref="Arcane{T}"/> can hold.</value>
        public int Length => _index;
        public readonly int Capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="Arcane{T}"/> class with a specified fixed capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of elements to allocate in the managed heap.</param>
        /// <exception cref="System.OverflowException">Thrown if capacity is less than 0.</exception>
        public Arcane(int capacity)
        {
            this.elements = new T[capacity];
            Capacity = capacity;
        }

        /// <summary>
        /// Gets or sets the element at the specified index using standard managed access.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown if index is outside the bounds of the array.</exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(elements), index);
            }
        }

        /// <summary>
        /// Adds an element to the collection using raw memory pointer arithmetic.
        /// Bypasses array bounds checking for maximum performance.
        /// </summary>
        /// <param name="element">The item to be added to the end of the collection.</param>
        /// <remarks>
        /// <b>Critical Performance:</b> This method uses <see cref="MemoryMarshal.GetArrayDataReference{T}(T[])"/> 
        /// to fetch the base address and <see cref="Unsafe.Add{T}(ref T, int)"/> to calculate the offset. 
        /// This effectively compiles down to a single MOV instruction in x64 assembly.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Add(in T element)
        {
            Debug.Assert(_index < elements.Length, "Capacity exceeded!");
            // Obtain a reference to the 0-th element without pinning or null-checks
            ref T start = ref MemoryMarshal.GetArrayDataReference(elements);

            // Calculate the target memory address and write the value, then increment the tail index
            Unsafe.Add(ref start, _index++) = element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast() {
            Debug.Assert(_index > 0, "Collection is already empty!");
            _index--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _index = 0;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(elements, 0, elements.Length);
            }
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            if ((uint)_index + (uint)items.Length > (uint)Capacity)
                throw new ArgumentOutOfRangeException();

            items.CopyTo(elements.AsSpan(_index));
            _index += items.Length;
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="Arcane{T}"/>.
        /// </summary>
        /// <param name="element">The object to locate in the collection.</param>
        /// <returns><see langword="true"/> if item is found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <b>Algorithm:</b> Performs a linear search using <see cref="Span{T}.Contains(T)"/>.
        /// In modern .NET runtimes, this is SIMD-accelerated (Single Instruction, Multiple Data) 
        /// for primitive types, providing O(n) complexity with highly optimized throughput.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T element) => AsSpan().Contains(element);

        /// <summary>
        /// Returns a <see cref="Span{T}"/> representation of the logically populated portion of the array.
        /// </summary>
        /// <returns>A span over the initialized elements of the collection.</returns>
        /// <remarks>
        /// The resulting span allows for safe, high-performance interoperability with 
        /// modern .NET APIs without copying the underlying data.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => elements.AsSpan(0, _index);
    }
}