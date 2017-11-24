using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nspProgramList;
using nspCFPInfrastructures;

namespace nspUnifyModel
{
    /// <summary>
    /// This class, representative of ech step need to executed in unify model
    /// </summary>
    public class classProcessTask
    {
        //In some case need reject running marking - Ex: in jumping control.
        public bool blRequestReject { get; set; } 

        //Task Info
        public int intProcessID { get; set; } //Task belong to what process
        public classStepDataInfor clsPLStepInfo { get; set; } //Contains all information: Step Position, Step Name, Function ID, Parameter...

        //constructor
        public classProcessTask(int intProcessID)
        {
        }
    }

    /// <summary>
    /// This class, for support control multi-child process of unify model
    /// </summary>
    public class classUnifyChildProcessInfo
    {
        //ID of each child process
        public int intProcessID { get; set; }

        //Class Item result
        public nspCFPInfrastructures.clsItemResultInformation clsItemInfo { get; set; }

        //For normal sequence
        public int intCurrentToken { get; set; } //What is token of running step
        public int intCurrentStep { get; set; } //what is step number of running step

        //For jumping request
        public bool blRequestJump { get; set; } //Indicate request jumping or not
        public int intTargetStep { get; set; } //Indicate what step child process want to jump to
        public int intTargetToken { get; set; } //Indicate target token (calculated from target step number)

        //Constructor
        public classUnifyChildProcessInfo()
        {
            this.clsItemInfo = new nspCFPInfrastructures.clsItemResultInformation();
        }
    }

    /// <summary>
    /// What is unify model?
    ///     1. Unify model created with purpose to unify all CFP model and running mode:
    ///         - Master Process Model
    ///         - Child Process Model
    ///             + Normal Mode
    ///             + Group Mode
    ///         - Single Thread Model
    ///        Unify Model will be mother of all above model!
    ///     2. Each Unify model can be contains multi-child process
    ///     
    /// </summary>
    public class classUnifyModel
    {
        //ID of Unify Model
        public int intProcessID { get; set; }

        //List of Child Process contains
        public List<classUnifyChildProcessInfo> lstUnifyChildProcessInfo { get; set; }

        //Program List class
        public classProgramList clsProgramList { get; set; }

        //Step List class
        public classStepList clsStepList { get; set; }

        //List of task Unify Model need to execute
        public List<classProcessTask> lstclsProcessTask { get; set; }

        //For MEF parts discover
        public nspMEFLoading.clsMEFLoading.clsExtensionHandle clsExtension { get; set; }

        //For Setting File
        public clsProcessSettingReading clsChildSetting { get; set; }

        /// <summary>
        /// Sequence to control checking process
        /// </summary>
        public void UnifyRunningSequence()
        {
            int i = 0;
            //Running each task, and assign result to each child process
            for(i=0;i<this.lstclsProcessTask.Count;i++)
            {

            }
        }


        //Ini for class
        public void classUnifyModelIni()
        {
            //Loading Program List

        }

        //constructor
        public classUnifyModel()
        {
            this.lstUnifyChildProcessInfo = new List<classUnifyChildProcessInfo>();
            this.lstclsProcessTask = new List<classProcessTask>();
        }
    }
}
