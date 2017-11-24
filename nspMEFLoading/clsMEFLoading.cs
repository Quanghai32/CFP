using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using nspCFPInfrastructures;
using nspProgramList;
using System.ComponentModel.Composition.ReflectionModel;


namespace nspMEFLoading
{
    public class clsMEFLoading
    {
        public interface IPluginExtension
        {
            void PluginEnumerateCommon();
        }

        [Export(typeof(IPluginExtension))]
        public class PluginExtensionLoader : IPluginExtension
        {
            [ImportMany(typeof(nspINTERFACE.IPluginExecute))]
            public List<Lazy<nspINTERFACE.IPluginExecute, nspINTERFACE.IPluginInfo>> lstPluginCollection; //Content all user extensions

            public List<FunctionCatalog> IFunctionCatalog; //Contain all function info of all plugin

            //For normal function ID register
            public void PluginEnumerateCommon()
            {
                if (lstPluginCollection == null)
                {
                    MessageBox.Show("Error: lstPluginCollection not yet initialized!", "PluginEnumerate() Error");
                    return;
                }

                this.IFunctionCatalog = new List<FunctionCatalog>();

                int i = 0;
                for (i = 0; i < lstPluginCollection.Count; i++)
                {
                    //Looking for information from plugin
                    List<List<object>> lstlstInput = new List<List<object>>();
                    List<List<object>> lstlstOutput = new List<List<object>>();
                    List<string> lstTemp = new List<string>();

                    lstPluginCollection[i].Value.IGetPluginInfo(lstlstInput, out lstlstOutput);

                    if (lstlstOutput.Count == 0) continue;

                    //Analyze received info

                    foreach (string strTest in lstlstOutput[0]) //Get the list object return
                    {
                        lstTemp = new List<string>(strTest.Split(','));
                        //Check valid condition
                        if (lstTemp.Count < 3) continue; //Not satify minimum data (JigID-HardID-FuncID)
                        int intJigID = 0;
                        int intHardID = 0;
                        if (int.TryParse(lstTemp[0], out intJigID) == false) continue; //JigID is not numeric
                        if (int.TryParse(lstTemp[1], out intHardID) == false) continue; //JigID is not numeric

                        //Looking in Function Catalog, if function is not Exist, then we create new one
                        int j = 0;
                        for (j = 2; j < lstTemp.Count; j++)
                        {
                            int intFuncID = 0;
                            if (int.TryParse(lstTemp[j], out intFuncID) == false) continue; //No care if FunID is not numeric
                            //Looking in all catalog, if not exist, then we create a new item
                            bool blNewFunc = true;
                            foreach (FunctionCatalog catalogTemp in this.IFunctionCatalog)
                            {
                                if ((catalogTemp.intJigID == intJigID) && (catalogTemp.intHardID == intHardID) && (catalogTemp.intFuncID == intFuncID))
                                {
                                    blNewFunc = false;
                                    MessageBox.Show("Error: Duplicated Function ID happen with Jig ID = " + intJigID.ToString() +
                                                        " , Hard ID = " + intHardID.ToString() + " Func ID = " + intFuncID.ToString(), "PluginEnumerate: Duplicating Function ID warning");
                                }
                            }
                            //If not yet exist in catalog, then create new one
                            if (blNewFunc == true)
                            {
                                var FuncTemp = new FunctionCatalog();
                                FuncTemp.intPartID = i;
                                FuncTemp.strPartDesc = lstPluginCollection[i].Metadata.IPluginInfo.ToString();
                                FuncTemp.intJigID = intJigID;
                                FuncTemp.intHardID = intHardID;
                                FuncTemp.intFuncID = intFuncID;
                                //Add to catalog
                                this.IFunctionCatalog.Add(FuncTemp);
                            }

                        } //End for j

                    } //End Foreach lststrOutput
                     

                } //End for loop i
            }

            //For special control function name register
            public void PluginEnumerateSpecialControl()
            {
                if (lstPluginCollection == null)
                {
                    MessageBox.Show("Error: PluginEnumerateSpecialControl not yet initialized!", "PluginEnumerateSpecialControl() Error");
                    return;
                }

                this.IFunctionCatalog = new List<FunctionCatalog>();

                int i = 0;
                int j = 0;
                int k = 0;
                for (i = 0; i < lstPluginCollection.Count; i++)
                {
                    //Looking for information from plugin
                    List<List<object>> lstlstInput = new List<List<object>>();
                    List<List<object>> lstlstOutput = new List<List<object>>();
                    List<string> lstTemp = new List<string>();

                    //Get info from plug-in 
                    lstPluginCollection[i].Value.IGetPluginInfo(lstlstInput, out lstlstOutput);

                    if (lstlstOutput.Count == 0) continue;

                    //Analyze received info
                    for(j=0;j<lstlstOutput.Count;j++)
                    {
                        if (lstlstOutput[j].Count == 0) continue;

                        lstTemp = new List<string>(lstlstOutput[j][0].ToString().ToUpper().Trim().Split(','));

                        if (lstTemp.Count == 0) continue;
                        if (lstTemp[0] != "SPECIAL") continue;

                        //Looking in all catalog, if not exist, then we create a new item
                        for(k=1;k<lstTemp.Count;k++)
                        {
                            string strTemp = lstTemp[k];
                            if (strTemp.Trim() == "") continue;

                            bool blNewFunc = true;
                            foreach (FunctionCatalog catalogTemp in this.IFunctionCatalog)
                            {
                                if(catalogTemp.strSpecialCmdCode == strTemp)
                                {
                                    blNewFunc = false;
                                    MessageBox.Show("Error: Duplicated Special control function name [" + strTemp + "]", "PluginEnumerateSpecialControl: Duplicating Special control function name warning");
                                }
                            }

                            //If not yet exist in catalog, then create new one
                            if (blNewFunc == true)
                            {
                                var FuncTemp = new FunctionCatalog();
                                FuncTemp.intPartID = i;
                                FuncTemp.strPartDesc = lstPluginCollection[i].Metadata.IPluginInfo.ToString();
                                FuncTemp.strSpecialCmdCode = strTemp;

                                //Add to catalog
                                this.IFunctionCatalog.Add(FuncTemp);
                            }
                        }
                    }

                } //End for loop i
            }
        }

        public class clsExtensionHandle
        {
            [Import(typeof(IPluginExtension))]
            public PluginExtensionLoader IPluginLoader;// = new PluginExtensionLoader();

            //public AssemblyCatalog CFPAssemblyCatalog { get; set; }

            public CompositionContainer _container = new CompositionContainer();
            public List<Lazy<nspINTERFACE.IPluginExecute, nspINTERFACE.IPluginInfo>> lstPluginCollection = new List<Lazy<nspINTERFACE.IPluginExecute, nspINTERFACE.IPluginInfo>>();
            public List<FunctionCatalog> IFunctionCatalog = new List<FunctionCatalog>(); //Contain all function info of all plugin
            public List<FunctionCatalog> lstFuncCatalogAllStep = new List<FunctionCatalog>();
            public List<string> lststrLoadFiles = new List<string>();

            //For special control
            public List<FunctionCatalog> lstFuncCatalogSpecialControl = new List<FunctionCatalog>();

            public int ExtensionLoader(string strFolderSearchName)
            {
                var catalog = new AggregateCatalog();
                //catalog.Catalogs.Add(CFPAssemblyCatalog); //This must be passed from System "Program" in CheckerFrameProgram. To use some System Func ID related to system (JigId =0 & JigID = 1000)

                //Add catalog from "Extensions" Directory
                //string strExtensionsPath = Application.StartupPath + @"\Extensions";
                string strExtensionsPath = Application.StartupPath + @"\" + strFolderSearchName;

                //catalog.Catalogs.Add(new DirectoryCatalog(strExtensionsPath));
                var test = new DirectoryCatalog(strExtensionsPath);
                lststrLoadFiles = test.LoadedFiles.ToList<string>();

                catalog.Catalogs.Add(test);


                //var assemblies = catalog.Parts.Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly).Distinct().ToList();
                //var assemblies = test.Parts.Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly).Distinct().ToList();
                //test.Parts.

                //Create compositioner container with parts in catalog
                _container = new CompositionContainer(catalog);

                //Fill the imports of this object                       
                try
                {
                    _container.ComposeParts(this);
                    return 0; //OK code
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ExtensionLoader() Fail!");
                    return 1; //Unexpected error code
                }

            }


            public void PluginLoader(List<classStepDataInfor> lstInput, string strFolderSearchName)
            {
                int intRet = 0;

                intRet = this.ExtensionLoader(strFolderSearchName);
                if (intRet != 0)
                {
                    MessageBox.Show("ExtensionLoader() fail. Cannot load Plugin. Error return: " + intRet.ToString(), "PluginLoader() Error");
                    Environment.Exit(0);
                }
                //For ini MEF parts
                this.IPluginLoader.PluginEnumerateCommon();

                this.IFunctionCatalog = this.IPluginLoader.IFunctionCatalog;
                this.lstPluginCollection = this.IPluginLoader.lstPluginCollection;
                //Create new list of function catalog correspond to each step
                this.lstFuncCatalogAllStep = new List<FunctionCatalog>();
                int i, j;
                for (i = 0; i < lstInput.Count; i++)
                {
                    FunctionCatalog CataTemp = new FunctionCatalog();
                    bool blFlagFound = false;
                    //Looking in all function catalog
                    for (j = 0; j < this.IFunctionCatalog.Count; j++)
                    {
                        int intJigID = lstInput[i].intJigId;
                        int intHardID = lstInput[i].intHardwareId;
                        int intFuncID = lstInput[i].intFunctionId;

                        if ((intJigID == this.IFunctionCatalog[j].intJigID) && (intHardID == this.IFunctionCatalog[j].intHardID) &&
                            (intFuncID == this.IFunctionCatalog[j].intFuncID))
                        {
                            blFlagFound = true;
                            CataTemp = this.IFunctionCatalog[j];
                            this.lstFuncCatalogAllStep.Add(CataTemp);
                            break;
                        }
                    } //End for loop

                    if (blFlagFound == false) //output error message
                    {
                        MessageBox.Show("Program List Error: we cannot find Function ID of step number " + lstInput[i].intStepNumber.ToString() +
                                        " in Function Catalog!", " Extension Loader");
                    }
                } //End for Loop

            }


            public void PluginLoaderSpecialControl(string strFolderSearchName)
            {
                int intRet = 0;

                intRet = this.ExtensionLoader(strFolderSearchName);
                if (intRet != 0)
                {
                    MessageBox.Show("ExtensionLoader() fail. Cannot load Plugin. Error return: " + intRet.ToString(), "PluginLoader() Error");
                    Environment.Exit(0);
                }
                //For ini MEF parts
                this.IPluginLoader.PluginEnumerateSpecialControl();

                this.IFunctionCatalog = this.IPluginLoader.IFunctionCatalog; //Just list of function catalog is enough!
                this.lstPluginCollection = this.IPluginLoader.lstPluginCollection;
            }
       
            //Constructor
            public clsExtensionHandle()
            {
                //CFPAssemblyCatalog = new AssemblyCatalog(typeof(clsExtensionHandle).Assembly);
            }
        }

    }
}
