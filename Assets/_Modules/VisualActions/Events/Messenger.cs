/*
 * Advanced C# messenger by Ilya Suzdalnitski. V1.0
 *
 * Based on Rod Hyde's "CSharpMessenger" and Magnus Wolffelt's "CSharpMessenger Extended".
 *
 * Features:
 	* Prevents a MissingReferenceException because of a reference to a destroyed message handler.
 	* Option to log all messages
 	* Extensive error detection, preventing silent bugs
 *
 * Usage examples:
 	1. Messenger.AddListener<GameObject>("prop collected", PropCollected);
 	   Messenger.Broadcast<GameObject>("prop collected", prop);
 	2. Messenger.AddListener<float>("speed changed", SpeedChanged);
 	   Messenger.Broadcast<float>("speed changed", 0.5f);
 *
 * Messenger cleans up its evenTable automatically upon loading of a new level.
 *
 * Don't forget that the messages that should survive the cleanup, should be marked with Messenger.MarkAsPermanent(string)
 *
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
//#define REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BW.EventSystem
{
    internal static class Messenger
    {
        #region Internal variables

        // static Messenger()
        // {
        //     new GameObject("MessengerHelper").AddComponent<MessengerHelper>();
        // }

//         //Disable the unused variable warning
// #pragma warning disable 0414
//         //Ensures that the MessengerHelper will be created automatically upon start of the game.
//         private static MessengerHelper messengerHelper = (new GameObject("MessengerHelper")).AddComponent<MessengerHelper>();
// #pragma warning restore 0414

        private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        private static Dictionary<string, Delegate> permanentTable = new Dictionary<string, Delegate>();

        #endregion

        #region Helper methods

        public static void Cleanup(bool includePermanent = false)
        {
#if LOG_ALL_MESSAGES
            Debug.Log("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
#endif

            eventTable.Clear();

            if (includePermanent)
            {
                permanentTable.Clear();
            }
        }

        public static void PrintEventTable()
        {
            Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

            foreach (KeyValuePair<string, Delegate> pair in eventTable)
            {
                Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
            }

            Debug.Log("\n");
        }

        #endregion

        #region Message logging and exception throwing

        public static void OnListenerAdding(Dictionary<string, Delegate> eventTable, string eventType, Delegate listenerBeingAdded)
        {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
            Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

            if (!eventTable.ContainsKey(eventType))
            {
                eventTable.Add(eventType, null);
            }

            Delegate d = eventTable[eventType];
            if (d != null && d.GetType() != listenerBeingAdded.GetType())
            {
                throw new ListenerException(string.Format(
                    "Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}",
                    eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
            }
        }

        public static void OnListenerRemoving(Dictionary<string, Delegate> eventTable, string eventType, Delegate listenerBeingRemoved)
        {
#if LOG_ALL_MESSAGES
            Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif

            if (eventTable.ContainsKey(eventType))
            {
                Delegate d = eventTable[eventType];

                if (d == null)
                {
                    Debug.Log($"Attempting to remove listener with for event type \"{eventType}\" but current listener is null.");
                }
                else if (d.GetType() != listenerBeingRemoved.GetType())
                {
                    Debug.Log(
                        $"Attempting to remove listener with inconsistent signature for event type {eventType}. Current listeners have type {d.GetType().Name} and listener being removed has type {listenerBeingRemoved.GetType().Name}");
                }
            }
            else
            {
                Debug.Log($"Attempting to remove listener for type \"{eventType}\" but Messenger doesn't know about this event type.");
            }
        }

        public static void OnListenerRemoved(Dictionary<string, Delegate> eventTable, string eventType)
        {
            if (eventTable[eventType] == null)
            {
                eventTable.Remove(eventType);
            }
        }

        public static void OnBroadcasting(string eventType)
        {
#if REQUIRE_LISTENER
            if (!eventTable.ContainsKey(eventType)) {
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
            }
#endif
        }

        private static BroadcastException CreateBroadcastSignatureException(string eventType)
        {
            return new BroadcastException($"Broadcasting message \"{eventType}\" but listeners have a different signature than the broadcaster.");
        }

        private class BroadcastException : Exception
        {
            public void set_HelpLink(string value)
            {
            }

            public MethodBase get_TargetSite()
            {
                return null;
            }

            public string get_Message()
            {
                return null;
            }

            public string get_StackTrace()
            {
                return null;
            }

            public Exception get_InnerException()
            {
                return null;
            }

            public void set_Source(string value)
            {
            }

            public string get_Source()
            {
                return null;
            }

            public string get_HelpLink()
            {
                return null;
            }

            public BroadcastException(string msg) : base(msg)
            {
            }
        }

        private class ListenerException : Exception
        {
            public void set_HelpLink(string value)
            {
            }

            public MethodBase get_TargetSite()
            {
                return null;
            }

            public string get_Message()
            {
                return null;
            }

            public string get_StackTrace()
            {
                return null;
            }

            public Exception get_InnerException()
            {
                return null;
            }

            public void set_Source(string value)
            {
            }

            public string get_Source()
            {
                return null;
            }

            public string get_HelpLink()
            {
                return null;
            }

            public ListenerException(string msg) : base(msg)
            {
            }
        }

        #endregion

        #region AddListener

        //No parameters
        public static void AddListener(string eventType, Callback handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback) eventTable[eventType] + handler;
            }
        }

        //Single parameter
        public static void AddListener<T>(string eventType, Callback<T> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T>) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T>) eventTable[eventType] + handler;
            }
        }

        //Two parameters
        public static void AddListener<T, U>(string eventType, Callback<T, U> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U>) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U>) eventTable[eventType] + handler;
            }
        }

        //Three parameters
        public static void AddListener<T, U, V>(string eventType, Callback<T, U, V> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V>) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V>) eventTable[eventType] + handler;
            }
        }

        //Four parameters
        public static void AddListener<T, U, V, N>(string eventType, Callback<T, U, V, N> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V, N>) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V, N>) eventTable[eventType] + handler;
            }
        }

        //Five parameters
        public static void AddListener<T, U, V, N, M>(string eventType, Callback<T, U, V, N, M> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerAdding(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V, N, M>) permanentTable[eventType] + handler;
            }
            else
            {
                OnListenerAdding(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V, N, M>) eventTable[eventType] + handler;
            }
        }

        #endregion

        #region RemoveListener

        //No parameters
        public static void RemoveListener(string eventType, Callback handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        //Single parameter
        public static void RemoveListener<T>(string eventType, Callback<T> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T>) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T>) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        //Two parameters
        public static void RemoveListener<T, U>(string eventType, Callback<T, U> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U>) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U>) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        //Three parameters
        public static void RemoveListener<T, U, V>(string eventType, Callback<T, U, V> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V>) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V>) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        //Four parameters
        public static void RemoveListener<T, U, V, N>(string eventType, Callback<T, U, V, N> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V, N>) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V, N>) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        //Five parameters
        public static void RemoveListener<T, U, V, N, M>(string eventType, Callback<T, U, V, N, M> handler, bool permanent = false)
        {
            if (permanent)
            {
                OnListenerRemoving(permanentTable, eventType, handler);
                permanentTable[eventType] = (Callback<T, U, V, N, M>) permanentTable[eventType] - handler;
                OnListenerRemoved(permanentTable, eventType);
            }
            else
            {
                OnListenerRemoving(eventTable, eventType, handler);
                eventTable[eventType] = (Callback<T, U, V, N, M>) eventTable[eventType] - handler;
                OnListenerRemoved(eventTable, eventType);
            }
        }

        #endregion

        #region Broadcast

        private static void InvokeCallback(Dictionary<string, Delegate> eventTable, string eventType)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                Callback callback = d as Callback;

                if (callback != null)
                {
                    callback();
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        private static void InvokeCallback<T>(Dictionary<string, Delegate> eventTable, string eventType, T arg1)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T>;

                if (callback != null)
                {
                    callback(arg1);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        private static void InvokeCallback<T, U>(Dictionary<string, Delegate> eventTable, string eventType, T arg1, U arg2)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T, U>;

                if (callback != null)
                {
                    callback(arg1, arg2);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        private static void InvokeCallback<T, U, V>(Dictionary<string, Delegate> eventTable, string eventType, T arg1, U arg2, V arg3)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T, U, V>;

                if (callback != null)
                {
                    callback(arg1, arg2, arg3);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        private static void InvokeCallback<T, U, V, N>(Dictionary<string, Delegate> eventTable, string eventType, T arg1, U arg2, V arg3, N arg4)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T, U, V, N>;

                if (callback != null)
                {
                    callback(arg1, arg2, arg3, arg4);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        private static void InvokeCallback<T, U, V, N, M>(Dictionary<string, Delegate> eventTable, string eventType, T arg1, U arg2, V arg3, N arg4, M arg5)
        {
            Delegate d;
            if (eventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T, U, V, N, M>;

                if (callback != null)
                {
                    callback(arg1, arg2, arg3, arg4, arg5);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }
        }

        //No parameters
        public static void Broadcast(string eventType)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);
            InvokeCallback(eventTable, eventType);
            InvokeCallback(permanentTable, eventType);
        }

        //Single parameter
        public static void Broadcast<T>(string eventType, T arg1)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);

            InvokeCallback<T>(eventTable, eventType, arg1);
            InvokeCallback<T>(permanentTable, eventType, arg1);
        }

        //Two parameters
        public static void Broadcast<T, U>(string eventType, T arg1, U arg2)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);

            InvokeCallback<T, U>(eventTable, eventType, arg1, arg2);
            InvokeCallback<T, U>(permanentTable, eventType, arg1, arg2);
        }

        //Three parameters
        public static void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);

            InvokeCallback<T, U, V>(eventTable, eventType, arg1, arg2, arg3);
            InvokeCallback<T, U, V>(permanentTable, eventType, arg1, arg2, arg3);
        }

        //Four parameters
        public static void Broadcast<T, U, V, N>(string eventType, T arg1, U arg2, V arg3, N arg4)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);

            InvokeCallback(eventTable, eventType, arg1, arg2, arg3, arg4);
            InvokeCallback(permanentTable, eventType, arg1, arg2, arg3, arg4);
        }

        //Five parameters
        public static void Broadcast<T, U, V, N, M>(string eventType, T arg1, U arg2, V arg3, N arg4, M arg5)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            OnBroadcasting(eventType);

            InvokeCallback(eventTable, eventType, arg1, arg2, arg3, arg4, arg5);
            InvokeCallback(permanentTable, eventType, arg1, arg2, arg3, arg4, arg5);
        }

        #endregion
    }
}