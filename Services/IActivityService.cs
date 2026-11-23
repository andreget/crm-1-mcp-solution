using Crm.Models;

namespace Crm.Services;

public interface IActivityService
{
    List<Activity> GetAll();
    List<Activity> GetByCustomer(int customerId);
    List<Activity> GetOpen();
    void Add(Activity activity);
    bool SetDone(int id, bool done);
    bool Delete(int id);
    void DeleteByCustomer(int customerId);
}
