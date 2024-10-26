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
        public float PressTime = 0.5f;
        int selected;
        bool _hasSelected = false;
        bool _isPressingKey = false;

        float _pressTimer;

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
        HoldButton yesHoldBtn;
        MouseEvents noBtnMouseEvents;
        HoldButton noHoldBtn;

        
        void Awake()
        {
            yesBtn.onClick.AddListener(TagTargetLiked);
            yesBtnMouseEvents = yesBtn.GetComponent<MouseEvents>();
            yesHoldBtn = yesBtn.GetComponent<HoldButton>();
            yesBtnMouseEvents.OnMouseEnter += Select;

            noBtn.onClick.AddListener(TagTargetDisliked);
            noBtnMouseEvents = noBtn.GetComponent<MouseEvents>();
            noHoldBtn = noBtn.GetComponent<HoldButton>();
            noBtnMouseEvents.OnMouseEnter += Select;
        }

        void Start()
        {
            _pressTimer = 0f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A) && !_isPressingKey)
            {
                _isPressingKey = true;
                selected = 1;
                Select(yesBtnMouseEvents, null);
            }
            else if (Input.GetKeyDown(KeyCode.D) && !_isPressingKey)
            {
                _isPressingKey = true;
                selected = 0;
                Select(noBtnMouseEvents, null);
            }

            if ((Input.GetKey(KeyCode.A) && selected == 1) || (Input.GetKey(KeyCode.D) && selected == 0))
            {
                _pressTimer += Time.deltaTime;
                
                if (selected == 0)
                {
                    noHoldBtn.Progress = _pressTimer / PressTime;
                }
                else if (selected == 1)
                {
                    yesHoldBtn.Progress = _pressTimer / PressTime;
                }

                if (_pressTimer >= PressTime)
                {
                    Submit();

                    ResetHold(); // Reset after successful hold
                }
            }
            else
            {
                ResetHold();
            }
        }

        void ResetHold()
        {
            _hasSelected = false;
            _isPressingKey = false;
            _pressTimer = 0f;
            
            noHoldBtn.Progress = 0f;
            yesHoldBtn.Progress = 0f;
            
            selected = -1;
            if (selector != null) Destroy(selector.gameObject);
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
            if (!_hasSelected && selected >= 0) return;

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