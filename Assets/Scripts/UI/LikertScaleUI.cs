using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RL.CellularAutomata;
using RL.Levels;

namespace RL.UI
{
    public class LikertScaleUI : Window
    {
        int selected;
        bool _hasSelected = false;
        Room targetRoom;
        public Room TargetRoom => targetRoom;

        public event Action OnLiked;
        public event Action OnDisliked;

        GameObject selector;

        [Header("Elements")]
        [SerializeField] GameObject pointersContainer;
        [SerializeField] Button yesBtn;
        [SerializeField] Button noBtn;
        MouseEvents yesBtnMouseEvents;
        MouseEvents noBtnMouseEvents;

        
        void Awake()
        {
            yesBtn.onClick.AddListener(TagTargetLiked);
            yesBtnMouseEvents = yesBtn.GetComponent<MouseEvents>();
            yesBtnMouseEvents.OnMouseEnter += Select;

            noBtn.onClick.AddListener(TagTargetDisliked);
            noBtnMouseEvents = noBtn.GetComponent<MouseEvents>();
            noBtnMouseEvents.OnMouseEnter += Select;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                selected = 1;
                Select(yesBtnMouseEvents, null);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                selected = 0;
                Select(noBtnMouseEvents, null);
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Submit();
            }
        }

        void Select(object sender, PointerEventData e)
        {
            var btn = ((MouseEvents) sender).Button;
            if (selector == null)
            {
                selector = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Selector"), transform);
                selector.transform.SetAsLastSibling();
            }
            
            _hasSelected = true;

            LeanTween.cancel(selector);
            LeanTween.move(selector, btn.transform.position, 0.0f);
            LeanTween.size(selector.transform as RectTransform, (btn.transform as RectTransform).sizeDelta, 0.1f)
                .setEaseOutSine();
        }

        void Submit()
        {
            if (!_hasSelected) return;

            Game.Telemetry.SaveRoomStats(selected, targetRoom.Stats);

            Hide(destroy: true);
        }


        #region Public methods

        public void SetTargetRoom(Room room)
        {
            this.targetRoom = room;
        }

        public void TagTargetLiked()
        {
            if (!TargetRoomIsNull()) return;

            OnLiked?.Invoke();
            Hide(destroy: true);
        }

        public void TagTargetDisliked()
        {
            if (!TargetRoomIsNull()) return;

            OnDisliked?.Invoke();
            Hide(destroy: true);
        }

        public void ShowPointers()
        {
            pointersContainer?.SetActive(true);
        }

        public void HidePointers()
        {
            pointersContainer?.SetActive(false);
        }

        #endregion

        
        bool TargetRoomIsNull()
        {
            if (targetRoom == null)
            {
                Debug.LogError("Current target room is null");
                return true;
            }
            return false;
        }
    }
}