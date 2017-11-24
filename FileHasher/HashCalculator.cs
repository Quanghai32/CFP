using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace FileHasher
{
    public class HashCalculator
    {
        public string FileName { get; private set; }

        public HashCalculator(string fileName)
        {
            this.FileName = fileName;
        }


        private static Mutex mut = new Mutex();

        public string CalculateFileHash()
        {
            mut.WaitOne();
            //
            string strRet = "";
            if (Path.GetExtension(this.FileName).Equals(".dll", System.StringComparison.InvariantCultureIgnoreCase)
                || Path.GetExtension(this.FileName).Equals(".exe", System.StringComparison.InvariantCultureIgnoreCase))
            {
                strRet =  GetAssemblyFileHash();
            }
            else
            {
                strRet = GetFileHash();
            }

            //
            mut.ReleaseMutex();

            //
            return strRet;
        }


        private string GetFileHash()
        {
            return CalculateHashFromStream(File.OpenRead(this.FileName));
        }

        private string GetAssemblyFileHash()
        {
            //string tempFileName = null;
            try
            {
                //try to open the assembly to check if this is a .NET one
                var assembly = Assembly.LoadFile(this.FileName);
                //Calculate Hash File
                //tempFileName = Disassembler.GetDisassembledFile(this.FileName);
                //return CalculateHashFromStream(File.OpenRead(tempFileName));

                var result = Disassembler.GetHashCodeDisassembledFile(this.FileName);
                //var result = Disassembler.GetSimpleHashCodeDisassembledFile(this.FileName);
                return result;
            }
            catch(BadImageFormatException)
            {
                return GetFileHash();
            }
            //finally
            //{
            //    if (File.Exists(tempFileName))
            //    {
            //        File.Delete(tempFileName);
            //    }
            //}
        }

        private string CalculateHashFromStream(Stream stream)
        {
            using (var readerSource = new System.IO.BufferedStream(stream, 1200000))
            {
                using (var md51 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                {
                    md51.ComputeHash(readerSource);

                    //Dispose after using
                    readerSource.Dispose();
                    stream.Dispose();

                    //Because very big memory consuming, we need to force Garbage collection do its job immediately
                    GC.Collect();

                    //
                    return Convert.ToBase64String(md51.Hash);
                }
            }
        }
    }
}
