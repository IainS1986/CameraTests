using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.Services.Interfaces
{
    public interface ITestService
    {
        Task<int> Increment();
    }
}
