using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Mathematics;
using UnityEngine;

namespace Jolt
{
    internal static unsafe partial class Bindings
    {
        public static NativeHandle<JPH_StateRecorder> JPH_StateRecorder_Create(IStateRecorder listener)
        {
            AssertInitialized();

            // Getting the listeners to work requires a lot of indirection, because the listeners are represented as
            // heap objects in the native plugin with their own lifetimes. The joltc constructor for the listener takes
            // a "user data" parameter that lets us provide context to the callbacks when they are invoked.
            //
            // During initialization, we provide static callbacks to joltc. When we construct a new native listener, we
            // also create a GCHandle for the associated managed listener. These are tracked in the ManagedReference
            // static class. When Jolt invokes the native listener, it invokes the joltc static listener, which invokes
            // our own static listeners with the GCHandle parameter, which we dereference to obtain the user listener.

            var gch = GCHandle.Alloc(listener);
            var ptr = GCHandle.ToIntPtr(gch);

            var handle = CreateHandle(UnsafeBindings.JPH_StateRecorder_Create(ptr));

            ManagedReference.Add(handle, gch);

            return handle;
        }

        public static void JPH_StateRecorder_Destroy(NativeHandle<JPH_StateRecorder> listener)
        {
            AssertInitialized();

            if (ManagedReference.Remove(listener, out var gch))
            {
                gch.Free();
            }
            else
            {
                Debug.LogError("Missing GCHandle for managed state recorder!");
            }

            UnsafeBindings.JPH_StateRecorder_Destroy(listener);

            listener.Dispose();
        }

        /// <summary>
        /// Set the static callback pointers for JPH_StateRecorder.
        /// </summary>
        private static void InitializeStateRecorders()
        {
            fixed (JPH_StateRecorder_Procs* ptr = &UnsafeStateRecorderProcs)
            {
                UnsafeBindings.JPH_StateRecorder_SetProcs(ptr);
            }
        }

        /// <summary>
        /// Static procs for marshalling; the lookup from static to instance listener happens in each method.
        /// </summary>
        private static readonly JPH_StateRecorder_Procs UnsafeStateRecorderProcs = new JPH_StateRecorder_Procs
        {
            ReadBytes = GetDelegatePointer((UnsafeReadBytes)UnsafeReadBytesCallback),
            IsEOF = GetDelegatePointer((UnsafeIsEOF)UnsafeIsEOFCallback),
            IsFailed = GetDelegatePointer((UnsafeIsFailed)UnsafeIsFailedCallback),
            WriteBytes = GetDelegatePointer((UnsafeWriteBytes)UnsafeWriteBytesCallback),
        };

        /// <summary>
        /// Unsafe static delegate for ReadBytes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void UnsafeReadBytes(IntPtr udata, void* outData, nuint inNumBytes);

        /// <summary>
        /// Unsafe static delegate for IsEOF.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeIsEOF(IntPtr udata);

        /// <summary>
        /// Unsafe static delegate for IsFailed.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeIsFailed(IntPtr udata);

        /// <summary>
        /// Unsafe static delegate for WriteBytes.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void UnsafeWriteBytes(IntPtr udata, void* inData, nuint inNumBytes);

        /// <summary>
        /// Unsafe static implementation for ReadBytes.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveBody))]
        private static void UnsafeReadBytesCallback(IntPtr udata, void* outData, nuint inNumBytes)
        {
            try
            {
                ManagedReference.Deref<IStateRecorder>(udata).ReadBytes(outData, inNumBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Unsafe static implementation for IsEOF.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveConstraint))]
        private static bool UnsafeIsEOFCallback(IntPtr udata)
        {
            try
            {
                return ManagedReference.Deref<IStateRecorder>(udata).IsEOF();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }

        /// <summary>
        /// Unsafe static implementation for IsFailed.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveContact))]
        private static bool UnsafeIsFailedCallback(IntPtr udata)
        {
            try
            {
                return ManagedReference.Deref<IStateRecorder>(udata).IsFailed();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }

        /// <summary>
        /// Unsafe static implementation for ShouldRestoreContact.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeWriteBytes))]
        private static void UnsafeWriteBytesCallback(IntPtr udata, void* inData, nuint inNumBytes)
        {
            try
            {
                ManagedReference.Deref<IStateRecorder>(udata).WriteBytes(inData, inNumBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
