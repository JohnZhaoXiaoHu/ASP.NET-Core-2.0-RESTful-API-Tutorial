using System.Threading.Tasks;
using MyRestful.Core.Interfaces;

namespace MyRestful.Infrastructure
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly MyContext _myContext;

        public UnitOfWork(MyContext myContext)
        {
            _myContext = myContext;
        }

        public async Task<bool> SaveAsync()
        {
            return await _myContext.SaveChangesAsync() > 0;
        }
    }
}
