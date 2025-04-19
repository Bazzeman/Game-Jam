using UnityEngine;

namespace Bas.Pennings.DevTools
{
    public abstract class AbstractSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of script '{typeof(T)}' is already destroyed. Returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    instance ??= FindFirstObjectByType<T>() ?? new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(instance.gameObject);

                    return instance;
                }
            }
        }

        /// <summary>
        /// If you want to use this method, please use "protected override void Awake()" and call the base method using "base.Awake()".
        /// </summary>
        protected virtual void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning($"[Singleton] Another instance of script '{typeof(T)}' already exists on game object '{instance.gameObject.name}'.");
                Debug.LogWarning($"[Singleton] Destroying this instance on game object '{gameObject.name}'.");
                Destroy(gameObject);
                return;
            }

            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy() => applicationIsQuitting = instance == this;
        private void OnApplicationQuit() => applicationIsQuitting = true;
    }
}
