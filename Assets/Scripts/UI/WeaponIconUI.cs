using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using URLG.Projectiles;

namespace RL.UI
{
    public class WeaponIconUI : MonoBehaviour
    {
        [SerializeField] ProjectileData projectileData;
        public ProjectileData ProjectileData
        {
            get
            {
                return projectileData;
            }
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