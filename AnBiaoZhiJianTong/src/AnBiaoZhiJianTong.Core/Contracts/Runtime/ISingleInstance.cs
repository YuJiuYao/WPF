namespace AnBiaoZhiJianTong.Core.Contracts.Runtime
{
    public interface ISingleInstance
    {
        bool AcquireMutex();
        void ReleaseMutex();
        void BringExistingToFront();
        void ShowAlreadyRunningNotice(); 
    }
}
