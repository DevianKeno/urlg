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
                Game.Main.Player?.SetControlsEnabled(false);
                
                this.coll.enabled = false;
                Debug.Log("Next level");

                Game.Main.currentLevel++;

                if (Game.Main.currentLevel > 10) /// level 10 is max
                {
                    Game.Telemetry.SaveEntriesToJson();

                    Game.Main.LoadScene(
                        new(){
                            SceneToLoad = "COMPLETE",
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
                                            SceneToLoad = "TITLE",
                                        });
                                });
                        });
                }
                else
                {
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
}