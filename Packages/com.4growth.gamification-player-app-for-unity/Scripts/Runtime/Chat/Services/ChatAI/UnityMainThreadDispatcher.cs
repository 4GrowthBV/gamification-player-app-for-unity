using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static UnityMainThreadDispatcher _instance = null;

        /// <summary>
        /// Access the singleton instance of the dispatcher.
        /// if no instance exists, a new GameObject with a dispatcher will be created.
        /// </summary>
        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
                return _instance;
            }
        }

        void Awake()
        {
            // Ensure that only one instance exists.
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            // Execute all queued actions.
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
            }
        }

        /// <summary>
        /// Enqueue an Action to be executed on the main thread.
        /// </summary>
        public void Enqueue(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Enqueue a coroutine to be executed on the main thread.
        /// </summary>
        public void Enqueue(IEnumerator action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Enqueue(() => StartCoroutine(action));
        }
    }
}
