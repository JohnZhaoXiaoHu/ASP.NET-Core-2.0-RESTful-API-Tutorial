using System;
using System.Threading.Tasks;

namespace MyRestful.Core.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> SaveAsync();
    }
}
