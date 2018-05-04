using MvvmCrossTest.Core.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.Services
{
    public class TestService : ITestService
    {
        private int m_count = 0;
        private TimeSpan m_wait = TimeSpan.FromSeconds(1);

        public async Task<int> Increment()
        {
            await Task.Delay((int)m_wait.TotalMilliseconds);

            return ++m_count;
        }
    }
}
