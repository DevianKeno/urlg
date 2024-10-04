using UnityEngine;

namespace RL.Levels
{
    public class Corridor : MonoBehaviour, ILoadable
    {
        [SerializeField] protected GameObject content;
        public GameObject Content
        {
            get { return content; }
            set { content = value; }
        }
        
        public void Load()
        {
            Content.SetActive(true);
        } 

        public void Unload()
        {
            Content.SetActive(false);
        }

    }
}
