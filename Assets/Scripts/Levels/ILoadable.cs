using UnityEngine;

namespace URLG
{
    /// <summary>
    /// Represents objects that can be loaded/unloaded.
    /// </summary>
    public interface ILoadable
    {
        public GameObject Content { get; set; }
        public void Load();
        public void Unload();
    }
}