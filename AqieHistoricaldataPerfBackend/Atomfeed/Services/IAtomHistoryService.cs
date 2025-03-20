using static AqieHistoricaldataPerfBackend.Atomfeed.Models.AtomHistoryModel;

namespace AqieHistoricaldataPerfBackend.Atomfeed.Services
{
    public interface IAtomHistoryService
    {
        public Task<string> AtomHealthcheck();
        //public Task<string> GetAtomHourlydata(string name);
        public Task<string> GetAtomHourlydata(querystringdata data);
        
    }
}
