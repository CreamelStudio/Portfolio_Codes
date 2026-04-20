namespace Lop.Survivor
{
    /// <summary>
    /// Tick의 영향을 받아야 하는 스크립트에 상속시키는 인터페이스
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// 메서드를 구현하고 TickManager.RegisterTickEvent 에 등록시키기 
        /// </summary>
        public void OnTick();
    }
}