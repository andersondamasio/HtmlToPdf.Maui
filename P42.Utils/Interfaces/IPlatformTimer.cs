using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace P42.Utils
{
    interface IPlatformTimer
    {
        void StartTimer(TimeSpan interval, Func<bool> callback);

        void StartTimer(TimeSpan interval, Func<Task<bool>> callback);
    }
}
