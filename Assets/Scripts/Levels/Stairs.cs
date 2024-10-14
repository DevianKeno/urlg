using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL.Levels
{
    public class Stairs : Tile
    {
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                this.coll.enabled = false;
                Debug.Log("Next level");

                Game.Main.currentLevel++;

                Game.Main.LoadScene(
                    new(){
                        SceneToLoad = "LOADING",
                        Mode = LoadSceneMode.Additive,
                        PlayTransition = true, },
                    onLoadSceneCompleted: () =>
                    {
                        Game.Main.UnloadScene(
                            "LEVEL",
                            onUnloadSceneCompleted: () =>
                            {
                                Game.Main.LoadScene(
                                    new(){
                                        SceneToLoad = "LEVEL",
                                    });
                            });
                    });
            }
        }
    }
}