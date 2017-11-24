using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspCFPInfrastructures
{
    //Sharing service to sending command to "Child Control Process Model object" (unique in all application)
    public interface IMasterProCmdService
    {
        object MasterProcessCommand(List<List<object>> lstlstobjCommand, out List<List<object>> lstlstobjReturn);
    }

    //Sharing service to sending command to "Child Control Process Model object" (unique in all application)
    public interface IChildProCmdService
    {
        object ChildControlCommand(List<List<object>> lstlstobjCommand, out List<List<object>> lstlstobjReturn);
    }
}
