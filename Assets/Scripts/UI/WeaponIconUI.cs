/*

Component Title: Weapon Icon User Interface
Data written: September 15, 2024
Date revised: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Implementation of the visual display where
    the two weapons currently equipped by the are displayed.

Data Structures:
    N/A
*/

using UnityEngine;
using UnityEngine.UI;

using RL.Projectiles;

namespace RL.UI
{
    public class WeaponIconUI : MonoBehaviour
    {
        [SerializeField] ProjectileData projectileData;
        public ProjectileData ProjectileData
        {
            get { return projectileData; }
            set
            {
                projectileData = value;
                Refresh();
            }
        }

        [SerializeField] Image icon;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            if (ProjectileData == null)
            {
                icon.CrossFadeAlpha(0f, 0f, true);
            }
            else
            {
                icon.sprite = ProjectileData.Sprite;
                icon.CrossFadeAlpha(1f, 0f, true);
            }
        }
    }
}