namespace Lop.Survivor
{
    // # System
    using System;
    using System.Collections;
    using System.Collections.Generic;

    // # Unity
    using UnityEngine;

    public class TickManager : MonoBehaviour
    {
        public static TickManager Instance { get; private set; }
        public float tickTime;
        public TimeManager timeManager;

        public float elapsedTicks = default;

        public Action OnTick = null; // 틱이 돌 때 실행할 이벤트 
        public Action OnDayInitialize = null; // 하루가 초기화 될 때 실행할 이벤트

        private WaitForSeconds tickWaitForSeconds;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if(!LOPNetworkManager.Instance.isConnected)
            {
                Debug.Log("[TickManager] 싱글플레이 모드 - 코루틴 시작");
                tickWaitForSeconds = new WaitForSeconds(tickTime);
                StartCoroutine(Co_OnTick());
            }
        }

        public void ServerOnTick(float syncElapsedTime)
        {
            SyncTimer(syncElapsedTime);
            OnTick?.Invoke();
        }

        public void SyncTimer(float syncElapsedTime)
        {
            if (elapsedTicks == syncElapsedTime) return;

            //Debug.Log($"[TickManager] Sync Timer: {elapsedTicks} -> {syncElapsedTime}");
            elapsedTicks = syncElapsedTime;
            timeManager.SyncTime(syncElapsedTime);
        }
         
        private IEnumerator Co_OnTick()
        {
            while (true)
            {
                // 경과된 틱 수를 1 증가 시키기
                elapsedTicks += 1.0f;
                OnTick?.Invoke();

                yield return tickWaitForSeconds;
            }
        } 

        // 틱 이벤트를 등록시키는 함수 
        public void RegisterTickEvent(ActionType actionType, Action action)
        {
            switch (actionType)
            {
                case ActionType.Tick: OnTick += action; break;
                case ActionType.DayInitialize: OnDayInitialize += action; break;
            }
        }

        // 틱 이벤트를 삭제시키는 함수 
        public void DestroyTickEvent(ActionType actionType, Action action)
        {
            switch (actionType)
            {
                case ActionType.Tick: OnTick -= action; break;
                case ActionType.DayInitialize: OnDayInitialize -= action; break;
            }
            Debug.Log($"{actionType} 이벤트 삭제 완료");
        }

        public void TriggerDayInitialize()
        {
            OnDayInitialize?.Invoke();
        }
    }
}