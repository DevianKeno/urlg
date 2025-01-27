/*

Component Title: Swap Weapons Window
Data written: September 28, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Implementation of the window that appears when the user is prompted
    if they want to swap their currently equipped weapons.

Data Structures:
    N/A
*/

using UnityEngine;
using UnityEngine.UI;

using RL.Player;

namespace RL.UI
{
    public class SwapWeaponsWindow : Window
    {
        PlayerController player;
        int selectedIndex = 0;

        [SerializeField] WeaponIconUI weaponIcon1;
        [SerializeField] WeaponIconUI weaponIcon2;
        [SerializeField] Button weaponBtn1;
        [SerializeField] Button weaponBtn2;
        [SerializeField] Button doneBtn;

        void Start()
        {
            this.player = Game.Main.Player;

            weaponIcon1.ProjectileData = player.Weapon1.ProjectileData;
            weaponIcon2.ProjectileData = player.Weapon2.ProjectileData;
            
            weaponBtn1.onClick.AddListener(() =>
            {
                SelectIndex(0);
                SwapSelected();
            });
            weaponBtn2.onClick.AddListener(() =>
            {
                SelectIndex(1);
                SwapSelected();
            });
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectIndex(0);
                SwapSelected();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectIndex(1);
                SwapSelected();
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Submit();
            }
        }

        void Submit()
        {
            player.UpdateWeapons();
            player.SaveWeapons();
            IsVisible = true;
            Hide(destroy: true);
        }

        public void SelectIndex(int index)
        {
            selectedIndex = index;
        }

        public void SwapSelected()
        {
            player.SwapEquipped(selectedIndex);
            RefreshVisible();
        }

        public void RefreshVisible()
        {
            weaponIcon1.ProjectileData = player.Weapon1.ProjectileData;
            weaponIcon2.ProjectileData = player.Weapon2.ProjectileData;
        }
    }
}