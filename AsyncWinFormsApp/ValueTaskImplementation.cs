using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWinFormsApp
{
    public class ValueTaskImplementation
    {
        public async ValueTask<int> GetIntAsync()
        { 
            await Task.Delay(1000);
            return 123;
        }
    }
}
