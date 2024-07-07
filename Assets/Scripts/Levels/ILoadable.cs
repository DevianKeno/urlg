using UnityEngine;

namespace RL
{
    public interface ILoadable
    {
        public GameObject Content { get; set; }
        public void Load();
        public void Unload();
    }
}