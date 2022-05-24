namespace Ballistics
{
    public interface PoolingObject
    {
        /// <summary>
        /// called from PoolManager when the object reawakes
        /// </summary>
        void ReAwake();
    }
}
