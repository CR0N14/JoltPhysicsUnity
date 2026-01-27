using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Mathematics;
using UnityEngine;

namespace Jolt
{
    internal static unsafe partial class Bindings
    {
        public static NativeHandle<JPH_StateRecorderFilter> JPH_StateRecorderFilter_Create(IStateRecorderFilter listener)
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

            var handle = CreateHandle(UnsafeBindings.JPH_StateRecorderFilter_Create(ptr));

            ManagedReference.Add(handle, gch);

            return handle;
        }

        public static void JPH_StateRecorderFilter_Destroy(NativeHandle<JPH_StateRecorderFilter> listener)
        {
            AssertInitialized();

            if (ManagedReference.Remove(listener, out var gch))
            {
                gch.Free();
            }
            else
            {
                Debug.LogError("Missing GCHandle for managed state recorder filter!");
            }

            UnsafeBindings.JPH_StateRecorderFilter_Destroy(listener);

            listener.Dispose();
        }

        /// <summary>
        /// Set the static callback pointers for JPH_StateRecorderFilter.
        /// </summary>
        private static void InitializeStateRecorderFilters()
        {
            fixed (JPH_StateRecorderFilter_Procs* ptr = &UnsafeStateRecorderFilterProcs)
            {
                UnsafeBindings.JPH_StateRecorderFilter_SetProcs(ptr);
            }
        }

        /// <summary>
        /// Static procs for marshalling; the lookup from static to instance listener happens in each method.
        /// </summary>
        private static readonly JPH_StateRecorderFilter_Procs UnsafeStateRecorderFilterProcs = new JPH_StateRecorderFilter_Procs
        {
            ShouldSaveBody = GetDelegatePointer((UnsafeShouldSaveBody)UnsafeShouldSaveBodyCallback),
            ShouldSaveConstraint = GetDelegatePointer((UnsafeShouldSaveConstraint)UnsafeShouldSaveConstraintCallback),
            ShouldSaveContact = GetDelegatePointer((UnsafeShouldSaveContact)UnsafeShouldSaveContactCallback),
            ShouldRestoreContact = GetDelegatePointer((UnsafeShouldRestoreContact)UnsafeShouldRestoreContactCallback),
        };

        /// <summary>
        /// Unsafe static delegate for OnContactValidate.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeShouldSaveBody(IntPtr udata, JPH_Body* body);

        /// <summary>
        /// Unsafe static delegate for ShouldSaveConstraint.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeShouldSaveConstraint(IntPtr udata, JPH_Constraint* constraint);

        /// <summary>
        /// Unsafe static delegate for ShouldSaveContact.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeShouldSaveContact(IntPtr udata, BodyID body1, BodyID body2);

        /// <summary>
        /// Unsafe static delegate for ShouldRestoreContact.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool UnsafeShouldRestoreContact(IntPtr udata, BodyID body1, BodyID body2);

        /// <summary>
        /// Unsafe static implementation for ShouldSaveBody.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveBody))]
        private static bool UnsafeShouldSaveBodyCallback(IntPtr udata, JPH_Body* body)
        {
            try
            {
                return ManagedReference.Deref<IStateRecorderFilter>(udata).ShouldSaveBody(); // TODO forward args
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Unsafe static implementation for ShouldSaveConstraint.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveConstraint))]
        private static bool UnsafeShouldSaveConstraintCallback(IntPtr udata, JPH_Constraint* constraint)
        {
            try
            {
                return ManagedReference.Deref<IStateRecorderFilter>(udata).ShouldSaveConstraint(); // TODO forward args
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Unsafe static implementation for ShouldSaveContact.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldSaveContact))]
        private static bool UnsafeShouldSaveContactCallback(IntPtr udata, BodyID body1, BodyID body2)
        {
            try
            {
                return ManagedReference.Deref<IStateRecorderFilter>(udata).ShouldSaveContact(); // TODO forward args
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Unsafe static implementation for ShouldRestoreContact.
        /// </summary>
        [MonoPInvokeCallback(typeof(UnsafeShouldRestoreContact))]
        private static bool UnsafeShouldRestoreContactCallback(IntPtr udata, BodyID body1, BodyID body2) // TODO forward args
        {
            try
            {
                return ManagedReference.Deref<IStateRecorderFilter>(udata).ShouldRestoreContact();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }
    }
}
