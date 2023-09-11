using System.Diagnostics;

namespace CodeAnalysis.Utilities
{
    /// <summary>
    /// 高效之处在于栈上访问数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value,nq}")]
    public struct ArrayElement<T>
    {
        public T Value;

        /// <summary>
        /// 使用隐式转换运算符，它简化了在 ArrayElement<T> 与泛型类型 T 之间进行转换的操作
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static implicit operator T(ArrayElement<T> element)
        {
            return element.Value;
        }
        
        /*以下代码没有的原因是开发者希望直接在原地更新数组元素，而不是通过将值转换为ArrayElement<T>类型。
         将值直接赋给Value属性（例如，elements[i].Value = v）可以避免额外的转换操作，提高代码的效率。另
         外，由于ArrayElement<T>是一个结构体，x86 ABI要求即使结构体可以适应寄存器，也必须在返回缓冲区中返回结构体。
         而且，由于结构体包含引用，因此在使用带有检查的GC屏障写入缓冲区时，JIT并不知道写入是否指向堆栈还是堆位置。
         直接赋值给Value属性可以轻松避免这些冗余操作。
        所以，为了代码效率和简洁性，这个类选择了不提供从T到ArrayElement<T>的隐式转换。
        这样设计鼓励开发者直接更新数组元素，而不是通过转换。*/
        // public static implicit operator ArrayElement<T>(T element)
        // {
        //     return element;
        // }

        //NOTE: there is no opposite conversion operator T -> ArrayElement<T>
        //
        // that is because it is preferred to update array elements in-place
        // "elements[i].Value = v" results in much better code than "elements[i] = (ArrayElement<T>)v"
        //
        // The reason is that x86 ABI requires that structs must be returned in
        // a return buffer even if they can fit in a register like this one.
        // Also since struct contains a reference, the write to the buffer is done with a checked GC barrier 
        // as JIT does not know if the write goes to a stack or a heap location.
        // Assigning to Value directly easily avoids all this redundancy.

        public static ArrayElement<T>[] MakeElementArray(T[] items)
        {
            if (items == null)
            {
                return null;
            }

            var array = new ArrayElement<T>[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                array[i].Value = items[i];
            }

            return array;
        }

        public static T[] MakeArray(ArrayElement<T>[] items)
        {
            if (items == null)
            {
                return null;
            }

            var array = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                array[i] = items[i].Value;
            }

            return array;
        }
    }
}