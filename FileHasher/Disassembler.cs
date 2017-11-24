using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FileHasher
{
    public class Disassembler
    {
        public static Regex regexMVID = new Regex("//\\s*MVID\\:\\s*\\{[a-zA-Z0-9\\-]+\\}", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexImageBase = new Regex("//\\s*Image\\s+base\\:\\s0x[0-9A-Fa-f]*", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexTimeStamp = new Regex("//\\s*Time-date\\s+stamp\\:\\s*0x[0-9A-Fa-f]*", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexPrivateImplementationDetails = new Regex(@"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex regexRemoveEmptyLines = new Regex(@"^\s+$[\r\n]*", RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Lazy<Assembly> currentAssembly = new Lazy<Assembly>(() =>
        {
            return MethodBase.GetCurrentMethod().DeclaringType.Assembly;
        });

        private static readonly Lazy<string> executingAssemblyPath = new Lazy<string>(() =>
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        });

        private static readonly Lazy<string> currentAssemblyFolder = new Lazy<string>(() =>
        {
            return Path.GetDirectoryName(currentAssembly.Value.Location);
        });

        private static readonly Lazy<string[]> arrResources = new Lazy<string[]>(() =>
        {
            return currentAssembly.Value.GetManifestResourceNames();
        });

        private const string ildasmArguments = "/all /text \"{0}\"";

        public static string ILDasmFileLocation
        {
            get
            {
                return Path.Combine(executingAssemblyPath.Value, "ildasm.exe");
            }
        }

        static Disassembler()
        {
            //extract the ildasm file to the executing assembly location
            ExtractFileToLocation("ildasm.exe", ILDasmFileLocation);
        }

        /// <summary>
        /// Saves the file from embedded resource to a given location.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the embedded resource.</param>
        /// <param name="fileName">Name of the file.</param>
        protected static void SaveFileFromEmbeddedResource(string embeddedResourceName, string fileName)
        {
            if (File.Exists(fileName))
            {
                //the file already exists, we can add deletion here if we want to change the version of the 7zip
                return;
            }
            FileInfo fileInfoOutputFile = new FileInfo(fileName);

            using (FileStream streamToOutputFile = fileInfoOutputFile.OpenWrite())
            using (Stream streamToResourceFile = currentAssembly.Value.GetManifestResourceStream(embeddedResourceName))
            {
                const int size = 4096;
                byte[] bytes = new byte[4096];
                int numBytes;
                while ((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
                {
                    streamToOutputFile.Write(bytes, 0, numBytes);
                }

                streamToOutputFile.Close();
                streamToResourceFile.Close();
            }
        }

        /// <summary>
        /// Searches the embedded resource and extracts it to the given location.
        /// </summary>
        /// <param name="fileNameInDll">The file name in DLL.</param>
        /// <param name="outFileName">Name of the out file.</param>
        protected static void ExtractFileToLocation(string fileNameInDll, string outFileName)
        {
            string resourcePath = arrResources.Value.Where(resource => resource.EndsWith(fileNameInDll, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (resourcePath == null)
            {
                throw new Exception(string.Format("Cannot find {0} in the embedded resources of {1}", fileNameInDll, currentAssembly.Value.FullName));
            }
            SaveFileFromEmbeddedResource(resourcePath, outFileName);
        }

        public static string GetDisassembledFile(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
            {
                throw new InvalidOperationException(string.Format("The file {0} does not exist!", assemblyFilePath));
            }

            string tempFileName = Path.GetTempFileName();
            var startInfo = new ProcessStartInfo(ILDasmFileLocation, string.Format(ildasmArguments, assemblyFilePath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode > 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Generating IL code for file {0} failed with exit code - {1}. Log: {2}",
                        assemblyFilePath, process.ExitCode, output));
                }

                File.WriteAllText(tempFileName, output);
            }

            RemoveUnnededRows(tempFileName);
            return tempFileName;
        }

        private static void RemoveUnnededRows(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            //remove MVID
            fileContent = regexMVID.Replace(fileContent, string.Empty);
            //remove Image Base
            fileContent = regexImageBase.Replace(fileContent, string.Empty);
            //remove Time Stamp
            fileContent = regexTimeStamp.Replace(fileContent, string.Empty);
            //Remove <PrivateImplementationDetails> Information
            fileContent = regexPrivateImplementationDetails.Replace(fileContent, string.Empty);
            //Remove IL Comments
            fileContent = StripComments(fileContent);

            File.WriteAllText(fileName, fileContent);
        }

        public static string GetHashCodeDisassembledFile(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
            {
                throw new InvalidOperationException(string.Format("The file {0} does not exist!", assemblyFilePath));
            }

            //string tempFileName = Path.GetTempFileName();
            var startInfo = new ProcessStartInfo(ILDasmFileLocation, string.Format(ildasmArguments, assemblyFilePath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode > 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Generating IL code for file {0} failed with exit code - {1}. Log: {2}",
                        assemblyFilePath, process.ExitCode, output));
                }
                process.Dispose();

                var removeUnneedString = RemoveUnnededRows2(output);
                var result = GetStringSha256Hash(removeUnneedString);

                //Force Garbage Collection collect immediately to prevent memory leak in start-up time
                GC.Collect();
                //System.Threading.Thread.Sleep(100);
                //
                return result;
            }
        }

        private static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;
            //
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                string result = BitConverter.ToString(hash).Replace("-", String.Empty);
                //
                sha.Dispose();
                //
                return result;
            }
        }


        public static string GetSimpleHashCodeDisassembledFile(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
            {
                throw new InvalidOperationException(string.Format("The file {0} does not exist!", assemblyFilePath));
            }

            string tempFileName = Path.GetTempFileName();
            var startInfo = new ProcessStartInfo(ILDasmFileLocation, string.Format(ildasmArguments, assemblyFilePath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode > 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Generating IL code for file {0} failed with exit code - {1}. Log: {2}",
                        assemblyFilePath, process.ExitCode, output));
                }

                var removeUnneedString = RemoveUnnededRows2(output);
                var result = removeUnneedString.GetHashCode().ToString();
                return result;
            }
        }

        private static string RemoveUnnededRows2(string fileContent)
        {
            //Remove IL Comments first
            fileContent = StripComments(fileContent);
            //Remove empty lines
            fileContent = regexRemoveEmptyLines.Replace(fileContent, string.Empty);
            ////Remove all white space
            //fileContent = fileContent.Replace(" ", string.Empty);

            //remove MVID
            fileContent = regexMVID.Replace(fileContent, string.Empty);
            //remove Image Base
            fileContent = regexImageBase.Replace(fileContent, string.Empty);
            //remove Time Stamp
            fileContent = regexTimeStamp.Replace(fileContent, string.Empty);
            //Remove <PrivateImplementationDetails> Information
            fileContent = regexPrivateImplementationDetails.Replace(fileContent, string.Empty);
            
            return fileContent;
        }


        private static string StripComments(string code)
        {
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(code, re, "$1");
        }

        public static string DisassembleFile(string assemblyFilePath)
        {
            string disassembledFile = GetDisassembledFile(assemblyFilePath);
            try
            {
                return File.ReadAllText(disassembledFile);
            }
            finally
            {
                if (File.Exists(disassembledFile))
                {
                    File.Delete(disassembledFile);
                }
            }
        }
    }
}
