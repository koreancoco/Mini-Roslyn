// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// define TRACE_LEAKS to get additional diagnostics that can lead to the leak sources. note: it will
// make everything about 2-3x slower
// 
// #define TRACE_LEAKS

// define DETECT_LEAKS to detect possible leaks
// #if DEBUG
// #define DETECT_LEAKS  //for now always enable DETECT_LEAKS in debug.
// #endif

using System;
using System.Diagnostics;
using System.Threading;

#if DETECT_LEAKS
using System.Runtime.CompilerServices;

#endif
namespace CodeAnalysis.Utilities
{
    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit. The main
    /// purpose is that limited number of frequently used objects can be kept in the pool for
    /// further recycling.
    /// 
    /// Notes: 
    /// 1) it is not the goal to keep all returned objects. Pool is not meant for storage. If there
    ///    is no space in the pool, extra returned objects will be dropped.
    /// 
    /// 2) it is implied that if object was obtained from a pool, the caller will return it back in
    ///    a relatively short time. Keeping checked out objects for long durations is ok, but 
    ///    reduces usefulness of pooling. Just new up your own.
    /// 
    /// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice. 
    /// Rationale: 逻辑依据
    ///    If there is no intent for reusing the object, do not use pool - just use "new". 
    /// </summary>
    internal class ObjectPool<T> where T : class
    {
        
        private struct Element
        {
            internal T Value;
        }

        /// <remarks>
        /// Not using System.Func{T} because this file is linked into the (debugger) Formatter,
        /// which does not have that type (since it compiles against .NET 2.0).
        /// </remarks>
        internal delegate T Factory();

        // Storage for the pool objects. The first item is stored in a dedicated field because we
        // expect to be able to satisfy most requests from it.
        private T _firstItem;
        /*
         三种实现方式：
         1.List<T>
            内部通过数组实现，但是相较于数组，多了一层间接寻址，所以性能上会差一些。
         2.T[]
            数组在内存中是连续存储的，访问它们时，CPU缓存能更好地工作。
         3.Element[]
            读性能和T[]差不多
            😊写性能是T[]的两倍
            😘由于在栈上分配，所以GC性能更好（这也是object pool最青睐的优势），但需要确保不会超过栈的大小
            😋需要通过Element.Value访问T，看起来是多了一次寻址，但是编译器会内联访问Element.Value的操作，
            从而减少访问次数。
        
         多了一层间接寻址指的是在访问List<T>中的元素时，需要经过一次额外的寻址操作。详细解释如下：
         List<T>的内部实现使用了一个私有的数组（T[]）来存储其元素。当访问List<T>中的元素时，例如list[i].A，
         实际上会发生以下操作：
            1.从List<T>实例中获取私有数组的引用。
            2.使用索引i访问私有数组中的元素。
            3.访问元素的属性A。
         
         Element[] 写性能优于T[]的原因如下：
             1.结构体内存分配：
                Element是一个值类型，也就是结构体。结构体通常在栈上分配内存，这意味着它们的分配和回收成本更低。
                在您的例子中，_elementItems数组实际上是在连续的内存区域中分配一系列结构体，
                而_items则是分配一系列引用类型对象。这种情况下，Element[]可能会在写操作上有一定的性能优势，
                因为它直接将值存储在连续的内存块中，而不需要额外的引用来定位对象。

            2.CPU缓存友好：
                正如之前所提到的，现代计算机的CPU具有多级缓存，能够快速地访问最近使用过的数据。
                Element[]在这方面可能更具优势，因为它将值类型直接存储在连续的内存区域中。这意味着在进行大量写操作时，
                CPU缓存能够更高效地访问和更新Element[]中的数据。
                
            3.内存碎片：
                使用T[]时，对象在堆上分配，元素之间的地址不连续，可能会导致内存碎片。而使用Element[]时，
                结构体会被连续地分配在内存中，减少了内存碎片的产生。虽然内存碎片对写性能的影响可能并不明显，
                但在某些情况下，这种连续内存分配可能会带来一定的性能提升。
         */
        private readonly Element[] _items;

        // factory is stored for the lifetime of the pool. We will call this only when pool needs to
        // expand. compared to "new T()", Func gives more flexibility to implementers and faster
        // than "new T()".
        private readonly Factory _factory;

#if DETECT_LEAKS
        private static readonly ConditionalWeakTable<T, LeakTracker> leakTrackers = new ConditionalWeakTable<T, LeakTracker>();

        private class LeakTracker : IDisposable
        {
            private volatile bool disposed;

#if TRACE_LEAKS
            internal volatile object Trace = null;
#endif

            public void Dispose()
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }

            private string GetTrace()
            {
#if TRACE_LEAKS
                return Trace == null ? "" : Trace.ToString();
#else
                return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#endif
            }

            ~LeakTracker()
            {
                if (!this.disposed && !Environment.HasShutdownStarted)
                {
                    var trace = GetTrace();

                    // If you are seeing this message it means that object has been allocated from the pool 
                    // and has not been returned back. This is not critical, but turns pool into rather 
                    // inefficient kind of "new".
                    Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {typeof(T)}. \n Location of the leak: \n {GetTrace()} TRACEOBJECTPOOLLEAKS_END");
                }
            }
        }
#endif      

        internal ObjectPool(Factory factory)
            : this(factory, Environment.ProcessorCount * 2)
        { }

        internal ObjectPool(Factory factory, int size)
        {
            Debug.Assert(size >= 1);
            _factory = factory;
            _items = new Element[size - 1];
        }

        private T CreateInstance()
        {
            var inst = _factory();
            return inst;
        }

        /// <summary>
        /// Produces an instance.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically 
        /// reducing how far we will typically search.
        /// </remarks>
        internal T Allocate()
        {
            // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            T inst = _firstItem;
            // inst == null 说明需要创建对象
            // 如果不为null，说明有对象可以使用，我们企图使用它，将其置为null，表示已经被使用
            // 但此时假如有另一个线程也在使用这个对象，那么它也会将其置为null，这时候同一个对象被分配了两次，线程不安全
            /*
Time  ──────────────────────────────────────>
 
Thread A:
        1. inst = _firstItem
        2. CompareExchange(_firstItem, null, inst)
        3. (如果成功) 返回inst
        4. (如果失败) 调用AllocateSlow()

Thread B:
        1. inst = _firstItem
        2. CompareExchange(_firstItem, null, inst)
        3. (如果成功) 返回inst
        4. (如果失败) 调用AllocateSlow()
        现在我们来分析为什么这段代码是线程安全的：

        1.线程A和线程B同时进入Allocate方法，首先尝试获取对象池中的第一个对象（步骤1）。
        2.线程A和线程B分别尝试使用Interlocked.CompareExchange将_firstItem设置为null，表示已经获取了该对象（步骤2）。
        3.Interlocked.CompareExchange方法是原子操作，它会保证在同一时刻，只有一个线程可以成功地将_firstItem设置为null。
        其他线程在执行CompareExchange时会发现_firstItem的值已经被改变，因此无法成功地将其设置为null。
        4.如果线程A（或线程B）成功地将_firstItem设置为null，那么它会返回该对象（步骤3）。如果失败，则会调用AllocateSlow()
        方法尝试从对象池中获取其他可用的对象（步骤4）。        

             */
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
            {
                inst = AllocateSlow();
            }

#if DETECT_LEAKS
            var tracker = new LeakTracker();
            leakTrackers.Add(inst, tracker);

#if TRACE_LEAKS
            var frame = CaptureStackTrace();
            tracker.Trace = frame;
#endif
#endif
            return inst;
        }

        /// <summary>
        /// 需要确保线程安全，可能原因如下：
        /// 1.多个线程同时调用Allocate时，如果_firstItem是null，那么多个线程会同时进入AllocateSlow方法
        /// 2.多个线程同时调用Allocate时，如果_firstItem不是null，
        /// 那么有且仅有一个线程返回对象，其他线程发现对象被取走之后会进入AllocateSlow方法
        /// </summary>
        /// <returns></returns>
        private T AllocateSlow()
        {
            var items = _items;

            for (int i = 0; i < items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional. 
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                T inst = items[i].Value;// optimistically 如果在这里就interlock，那么遍历到null的时候也会lock，影响性能
                if (inst != null) // interlock only when we have a candidate 这时候inst不为null，说明可能有对象可以使用
                {
                    // interlock确保只有一个线程能取走对象，失败的对象会进入下一次循环
                    if (inst == Interlocked.CompareExchange(ref items[i].Value, null, inst))
                    {
                        return inst;
                    }
                }
            }

            return CreateInstance();
        }

        /// <summary>
        /// Returns objects to the pool.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically 
        /// reducing how far we will typically search in Allocate.
        /// </remarks>
        internal void Free(T obj)
        {
            Validate(obj);
            ForgetTrackedObject(obj);

            if (_firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                /*
                N.B. CompareExchange的性能开销是普通赋值的2.5倍 
                Q:Free方法中为何不使用interlocked来确保多个线程不会同时写入_firstItem
                
                A:在Free方法中，作者故意选择不使用Interlocked操作来确保多个线程不会同时写入_firstItem。
                    这是一个权衡，牺牲了一些线程安全性以换取性能的提升。考虑以下两种情况：
                        1.使用Interlocked操作：这将确保所有线程在写入_firstItem时都是线程安全的，
                        但代价是性能降低，因为Interlocked操作通常比普通操作要慢。
                        
                        2.不使用Interlocked操作：这可能导致两个线程同时写入_firstItem，在极端情况下，
                        这意味着其中一个对象实例会被覆盖并丢失。然而，这种情况发生的概率相对较低，而且性能得到了提高。

                    作者选择了第二种方案，因为在实际应用中，这种极端情况发生的概率很低，并且即使发生，
                    损失的对象实例只会被垃圾回收器回收，对整体系统性能的影响较小。相比之下，使用Interlocked操作会在
                    每次调用Free方法时都产生性能开销，这在高并发场景下可能导致明显的性能降低。
                       
                我说第二种方案（不使用Interlocked操作）中这种极端情况发生的概率较小时，意味着在实际应用中，
                很少会有两个线程在几乎同一时刻尝试将对象返回到对象池的_firstItem。考虑以下示例：
                    例：假设有两个线程A和B，它们都在使用对象池中的对象。在线程A完成工作后，它将对象实例返回到对象池。
                    几乎同时，线程B也完成了工作，并尝试将另一个对象实例返回到对象池。在这种情况下，如果线程A和线程B在几乎相
                    同的时间点尝试写入_firstItem，那么其中一个对象实例可能会被覆盖。

                然而，在实际应用中，线程A和线程B完成工作并尝试将对象实例返回到对象池的准确时间点很难完全一致。
                即使它们彼此接近，CPU调度和其他因素可能会导致它们之间的间隔足够大，以避免发生竞争条件。

                因此，第二种方案中的极端情况（即两个线程几乎同时尝试写入_firstItem）在实际应用中发生的概率相对较低。
                这就是为什么在这种情况下，作者愿意牺牲一定的线程安全性，以换取性能提升。  
                       
                                
                 */
                _firstItem = obj;
            }
            else
            {
                FreeSlow(obj);
            }
        }

        private void FreeSlow(T obj)
        {
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i].Value = obj;
                    break;
                }
            }
        }

        /// <summary>
        /// Removes an object from leak tracking.  
        /// 
        /// This is called when an object is returned to the pool.  It may also be explicitly 
        /// called if an object allocated from the pool is intentionally not being returned
        /// to the pool.  This can be of use with pooled arrays if the consumer wants to 
        /// return a larger array to the pool than was originally allocated.
        /// </summary>
        [Conditional("DEBUG")]
        internal void ForgetTrackedObject(T old, T replacement = null)
        {
#if DETECT_LEAKS
            LeakTracker tracker;
            if (leakTrackers.TryGetValue(old, out tracker))
            {
                tracker.Dispose();
                leakTrackers.Remove(old);
            }
            else
            {
                var trace = CaptureStackTrace();
                Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nObject of type {typeof(T)} was freed, but was not from pool. \n Callstack: \n {trace} TRACEOBJECTPOOLLEAKS_END");
            }

            if (replacement != null)
            {
                tracker = new LeakTracker();
                leakTrackers.Add(replacement, tracker);
            }
#endif
        }

#if DETECT_LEAKS
        private static Lazy<Type> _stackTraceType = new Lazy<Type>(() => Type.GetType("System.Diagnostics.StackTrace"));

        private static object CaptureStackTrace()
        {
            return Activator.CreateInstance(_stackTraceType.Value);
        }
#endif

        [Conditional("DEBUG")]
        private void Validate(object obj)
        {
            Debug.Assert(obj != null, "freeing null?");

            /*
             *
                        Heap:
                        +---------------------------------------+
                        | ObjectPool<T>                         |
                        |  - _firstItem                         |
                        |  - _items --------------------------> | Element[] (on heap)
                        |  - _factory                           |
                        +---------------------------------------+

                        Stack:
                        +---------------------+
                        | Validate method     |
                        |  - items ----------> | Element[] (on heap, same as _items)
                        +---------------------+
             * var items = _items;这一行代码将_items数组的引用赋值给一个局部变量items。
             * 这个做法的目的是为了在访问数组时提高性能。将数组引用分配给局部变量后，访问这个局部变量通常比直接访问类
             * 的成员变量更快。这是因为局部变量存储在栈上，而类成员变量存储在堆上,首先访问堆上的ObjectPool<T>实例(this指针),
             * 然后通过_items引用访问数组。从栈上访问数据通常比从堆上访问数据更快。因此，这个做法并不是作者的编码喜好，
             * 而是为了提高性能。在循环中，将类成员变量分配给局部变量，然后访问局部变量，这是一种常见的优化手段。
             */
            var items = this._items;
            for (int i = 0; i < items.Length; i++)
            {
                var value = items[i].Value;
                if (value == null)
                {
                    return;
                }

                Debug.Assert(value != obj, "freeing twice?");
            }
        }
    }
}
