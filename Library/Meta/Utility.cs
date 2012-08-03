/***************************************************************************************************
 * Copyright (C) - Andrea Bertolotto - 2006-2010 - The Code Project Open License (CPOL)
 * 
 * a.bertolotto - 2007/04/24 12.49.24 - Changed Blob index sizing. This was fixed, but should be
 *                                      read from assembly structure
 * a.bertolotto - 2006/08/24 12.04.08 - Created
 ***************************************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Text;
using Asmex.FileViewer;

namespace XLibrary.Meta
{
    public class MetaUtility
    {
        // CLI Reference available at: http://msdn.microsoft.com/netframework/ecma/

        #region CLI Header related constants

        /// <summary>
        /// Offset in CLI Header where Flags starts - see CLI Reference Partition II - § 25.3.3
        /// 16 = Cb 4 bytes + MajorRuntimeVersion 2 bytes + MinorRuntimeVersion 2 bytes + MetaData 8 bytes
        /// </summary>
        private const long CLI_HEADER_FLAGS_OFFSET = 16;

        /// <summary>
        /// Size of RVA of the hash data - see CLI Reference Partition II - § 25.3.3
        /// </summary>
        private const int CLI_HEADER_STRONG_NAME_SIGNATURE_LENGTH = 8;

        /// <summary>
        /// Flag constants defined in CLI Reference Partition II - § 25.3.3.1
        /// </summary>
        private const int COMIMAGE_FLAGS_VALUE_ILONLY = 0x00000001;

        private const int COMIMAGE_FLAGS_VALUE_STRONGNAMESIGNED = 0x00000008;

        #endregion

        #region References Table related constants

        /// <summary>
        /// Offset in row where PublicKeyOrToken starts - see CLI Reference Partition II - § 22.5
        /// 12 = MajorVersion 2 bytes + MinorVersion 2 bytes + BuildNumber 2 bytes + RevisionNumber 2 bytes + Flags 4 bytes
        /// </summary>
        private const long REFERENCE_TABLE_PUBLIC_KEY_OR_TOKEN_OFFSET = 12;

        #endregion

        #region Assembly Table related constants

        /// <summary>
        /// Offset in Assembly Table where Flags starts - see CLI Reference Partition II - § 22.2
        /// 12 = HashAlgId 4 bytes + MajorVersion 2 bytes + MinorVersion 2 bytes + BuildNumber 2 bytes + RevisionNumber 2 bytes
        /// </summary>
        private const long ASSEMBLY_TABLE_FLAGS_OFFSET = 12;

        /// <summary>
        /// Flag constants defined in CLI Reference Partition II - § 23.1.2
        /// </summary>
        private const int ASSEMBLY_TABLE_FLAGS_VALUE_PUBLICKEY = 0x0001;

        /// <summary>
        /// Offset in Assembly Table where Public Key index starts - see CLI Reference Partition II - § 22.2
        /// 16 = HashAlgId 4 bytes + MajorVersion 2 bytes + MinorVersion 2 bytes + BuildNumber 2 bytes + RevisionNumber 2 bytes + Flags 4 bytes
        /// </summary>
        private const long ASSEMBLY_TABLE_PUBLIC_KEY_INDEX_OFFSET = 16;

        #endregion

        #region Private explicit constructor

        /// <summary>
        /// Private constructor - this class only contains static methods
        /// </summary>
        private MetaUtility()
        {
            // This class is intended to contain only static methods, so a private constructor, preventing it from being instantiated.
            // At this time this could also be done declaring whole class as static, cause we are using .NET 2.0
        }

        #endregion

        #region WritePattern

        /// <summary>
        /// Write a byte pattern at specified offset in stream
        /// </summary>
        /// <param name="fileStream">FileStream to write into</param>
        /// <param name="pattern">Byte array with values to write</param>
        /// <param name="position">Offset in stream where writing will start</param>
        public static void WritePattern(FileStream fileStream, byte[] pattern, long position)
        {
            if (position <= (fileStream.Length - pattern.Length + 1))
            {
                fileStream.Position = position;
                for (int patternPointer = 0; patternPointer < pattern.Length; patternPointer++)
                {
                    fileStream.WriteByte(pattern[patternPointer]);
                }
            }
        }

        #endregion

        #region CheckFileExistsAndWriteable method

        /// <summary>
        /// Check if passed file exists on file system and is accessible and writable
        /// </summary>
        /// <param name="fileName">Assembly path and filename</param>
        /// <returns>Returns a bool indicating if file exists and is writable</returns>
        public static bool CheckFileExistsAndWriteable(string fileName)
        {
            bool returnValue = false;

            FileStream fs = null;
            try
            {
                if (File.Exists(fileName))
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                    if (fs.CanWrite)
                    {
                        returnValue = true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                returnValue = false;
            }
            catch (IOException)
            {
                returnValue = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return returnValue;
        }

        #endregion

        #region CheckFileExistsAndReadable method

        /// <summary>
        /// Check if passed file exists on file system and is accessible and readable
        /// </summary>
        /// <param name="fileName">Assembly path and filename</param>
        /// <returns>Returns a bool indicating if file exists and is writable</returns>
        public static bool CheckFileExistsAndReadable(string fileName)
        {
            bool returnValue = false;

            FileStream fs = null;
            try
            {
                if (File.Exists(fileName))
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    if (fs.CanRead && fs.CanSeek)
                    {
                        returnValue = true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                returnValue = false;
            }
            catch (IOException)
            {
                returnValue = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return returnValue;
        }

        #endregion

        #region CLIHeaderFlagToString method

        /// <summary>
        /// Decodes passed flag value in human readable form
        /// For possible values and description see - CLI Reference Partition II - § 25.3.3.1
        /// </summary>
        /// <param name="flag">Flag value</param>
        /// <returns>Returns a string with flag description</returns>
        ///
        public static string CLIHeaderFlagToString(uint flag)
        {
            StringBuilder flagString = new StringBuilder();

            if ((flag & COMIMAGE_FLAGS_VALUE_ILONLY) == COMIMAGE_FLAGS_VALUE_ILONLY)
            {
                flagString.Append("COMIMAGE_FLAGS_ILONLY | ");
            }

            if ((flag & 0x00000002) == 0x00000002)
            {
                flagString.Append("COMIMAGE_FLAGS_32BITREQUIRED | ");
            }

            if ((flag & COMIMAGE_FLAGS_VALUE_STRONGNAMESIGNED) == COMIMAGE_FLAGS_VALUE_STRONGNAMESIGNED)
            {
                flagString.Append("COMIMAGE_FLAGS_STRONGNAMESIGNED | ");
            }

            if ((flag & 0x00010000) == 0x00010000)
            {
                flagString.Append("COMIMAGE_FLAGS_TRACKDEBUGDATA | ");
            }

            if (flagString.Length > 0)
            {
                return flagString.Remove(flagString.Length - 3, 3).ToString();
            }
            
            return string.Empty;
        }

        #endregion

        #region AssemblyFlagToString method

        /// <summary>
        /// Decodes passed flag value in human readable form
        /// For possible values and description see - CLI Reference Partition II - § 23.1.2
        /// </summary>
        /// <param name="flag">Flag value</param>
        /// <returns>Returns a string with flag description</returns>
        public static string AssemblyFlagToString(uint flag)
        {
            StringBuilder flagString = new StringBuilder();

            if ((flag & ASSEMBLY_TABLE_FLAGS_VALUE_PUBLICKEY) == ASSEMBLY_TABLE_FLAGS_VALUE_PUBLICKEY)
            {
                flagString.Append("PublicKey | ");
            }

            if ((flag & 0x0000) == 0x0000)
            {
                flagString.Append("SideBySideCompatible | ");
            }

            if ((flag & 0x0030) == 0x0030)
            {
                flagString.Append("<reserved> | ");
            }

            if ((flag & 0x0100) == 0x0100)
            {
                flagString.Append("Retargetable | ");
            }

            if ((flag & 0x0030) == 0x8000)
            {
                flagString.Append("EnableJITcompileTracking | ");
            }

            if ((flag & 0x0100) == 0x4000)
            {
                flagString.Append("DisableJITcompileOptimizer | ");
            }

            if (flagString.Length > 0)
            {
                return flagString.Remove(flagString.Length - 3, 3).ToString();
            }
            
            return string.Empty;
        }

        #endregion

        #region PatchReference method

        /// <summary>
        /// Patch assembly reference to remove public key evidences
        /// </summary>
        /// <param name="fileName">Assembly path and filename</param>
        /// <param name="referenceOffset">File offset for assembly reference in Assembly Reference Table</param>
        /// <param name="blobIndexSize">Blob Index size in bytes</param>
        /// <returns>Returns a bool indicating if assembly data can be patched successfully</returns>
        public static bool PatchReference(string fileName, long referenceOffset, int blobIndexSize)
        {
            bool returnValue;

            if (CheckFileExistsAndWriteable(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);

                if (referenceOffset > 0)
                {
                    byte[] noPublicKeyOrToken = new byte[blobIndexSize];
                    for (short counter = 0; counter < blobIndexSize; counter++)
                    {
                        noPublicKeyOrToken[counter] = 0x00;
                    }

                    WritePattern(fs, noPublicKeyOrToken, referenceOffset + REFERENCE_TABLE_PUBLIC_KEY_OR_TOKEN_OFFSET);
                }

                fs.Flush();
                fs.Close();

                returnValue = true;
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }

        #endregion

        #region PatchAssemblyStrongSigning method

        /// <summary>
        /// Patch assembly to remove strong name evidences
        /// </summary>
        /// <param name="fileName">Assembly path and filename</param>
        /// <param name="cliHeaderFlag">CLI Header flag value</param>
        /// <param name="cliHeaderFlagOffset">File offset for CLI Header flag</param>
        /// <param name="strongNameSignatureOffset">File offset for CLI Header strong name signature</param>
        /// <param name="publicKeyIndexOffset">File offset for Public Key Index in Assembly Table</param>
        /// <param name="assemblyFlag">Assembly Table Flag value</param>
        /// <param name="assemblyFlagOffset">File offset for Assembly Table Flag</param>
        /// <param name="blobIndexSize">Blob Index size in bytes</param>
        /// <returns>Returns a bool indicating if assembly data can be patched successfully</returns>
        public static bool PatchAssemblyStrongSigning(string fileName, uint cliHeaderFlag, long cliHeaderFlagOffset, long strongNameSignatureOffset,
                                                      long publicKeyIndexOffset, uint assemblyFlag, long assemblyFlagOffset, int blobIndexSize, bool removePublicKey)
        {
            bool returnValue;

            if (CheckFileExistsAndWriteable(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);

                #region Runtime flags in CLI Header patch

                if (cliHeaderFlagOffset > 0)
                {
                    byte[] newFlag = new byte[1];
                    newFlag[0] = (byte) (cliHeaderFlag - COMIMAGE_FLAGS_VALUE_STRONGNAMESIGNED);
                    WritePattern(fs, newFlag, cliHeaderFlagOffset);
                }

                #endregion

                #region StrongNameSignature RVA in CLI Header patch

                if (strongNameSignatureOffset > 0)
                {
                    byte[] noSignature = new byte[CLI_HEADER_STRONG_NAME_SIGNATURE_LENGTH];
                    for (short counter = 0; counter < CLI_HEADER_STRONG_NAME_SIGNATURE_LENGTH; counter++)
                    {
                        noSignature[counter] = 0x00;
                    }
                    WritePattern(fs, noSignature, strongNameSignatureOffset);
                }

                #endregion

                if (removePublicKey)
                {
                    #region Public Key Index in Assembly Table patch

                    if (publicKeyIndexOffset > 0)
                    {
                        byte[] publicKeyIndexLength = new byte[blobIndexSize];
                        for (short counter = 0; counter < blobIndexSize; counter++)
                        {
                            publicKeyIndexLength[counter] = 0x00;
                        }
                        WritePattern(fs, publicKeyIndexLength, publicKeyIndexOffset);
                    }

                    #endregion

                    #region Assembly Table Flags patch

                    if (assemblyFlagOffset > 0)
                    {
                        byte[] newAssemblyFlag = new byte[1];
                        newAssemblyFlag[0] = (byte)(assemblyFlag - ASSEMBLY_TABLE_FLAGS_VALUE_PUBLICKEY);
                        WritePattern(fs, newAssemblyFlag, assemblyFlagOffset);
                    }

                    #endregion
                }

                fs.Flush();
                fs.Close();

                returnValue = true;
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }

        #endregion

        #region IsAssemblyStrongSigned method

        /// <summary>
        /// Returns true if assembly is signed according to its CLI Header Flag value
        /// </summary>
        /// <param name="cliHeaderFlag">Unsigned int representing CLI Header flag</param>
        /// <returns>Returns a bool indicating if assembly is strong signed</returns>
        public static bool IsAssemblyStrongSigned(uint cliHeaderFlag)
        {
            return (cliHeaderFlag >= COMIMAGE_FLAGS_VALUE_STRONGNAMESIGNED);
        }

        #endregion

        #region GetAssemblyData method

        /// <summary>
        /// Decodes assembly metadata, headers and tables to required values ad file offsets
        /// </summary>
        /// <param name="fileName">Assembly path and filename</param>
        /// <param name="cliHeaderFlag">Reference to CLI Header flag value</param>
        /// <param name="cliHeaderFlagOffset">Reference to CLI Header flag file offset</param>
        /// <param name="strongNameSignatureOffset">Reference to CLI Header strong name signature file offset</param>
        /// <param name="publicKeyIndexOffset">Reference to Public Key Index in Assembly Table file offset</param>
        /// <param name="publicKeyOffset">Reference to Public Key file offset</param>
        /// <param name="assemblyFlag">Reference to Assembly Table Flag value</param>
        /// <param name="assemblyFlagOffset">Reference to Assembly Table Flag file offset</param>
        /// <param name="compiledRuntimeVersion">Reference to string with compiled runtime version</param>
        /// <param name="assemblyReferences">Reference to array to hold assembly references</param>
        /// <param name="blobIndexSize">Blob Index size in bytes</param>
        /// <returns>Returns a bool indicating if assembly data can be retieved successfully</returns>
        public static bool GetAssemblyData(string fileName, ref uint cliHeaderFlag, ref long cliHeaderFlagOffset, ref long strongNameSignatureOffset,
                                           ref long publicKeyIndexOffset, ref long publicKeyOffset, ref uint assemblyFlag, ref long assemblyFlagOffset,
                                           ref string compiledRuntimeVersion, ref ArrayList assemblyReferences, ref int blobIndexSize, bool getSigOnly)
        {
            MModule mod;
            BinaryReader r;

            FileStream stream = null;

            cliHeaderFlag = 0;
            cliHeaderFlagOffset = 0;
            strongNameSignatureOffset = 0;
            publicKeyIndexOffset = 0;
            publicKeyOffset = 0;
            compiledRuntimeVersion = String.Empty;
            assemblyFlag = 0;
            assemblyFlagOffset = 0;
            assemblyReferences.Clear();

            try
            {
                if (File.Exists(fileName))
                {
                    stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    r = new BinaryReader(stream);

                    mod = new MModule(r);

                    try
                    {
                        if (mod.ModHeaders.COR20Header != null)
                        {
                            cliHeaderFlag = mod.ModHeaders.COR20Header.Flags;
                            cliHeaderFlagOffset = mod.ModHeaders.COR20Header.Start + CLI_HEADER_FLAGS_OFFSET;
                            // if Strong Name Signature RVA is zero, no Strong Signature is available
                            strongNameSignatureOffset = (mod.ModHeaders.COR20Header.StrongNameSignature.Rva == 0
                                                             ? 0
                                                             : mod.ModHeaders.COR20Header.StrongNameSignature.Start);
                            compiledRuntimeVersion = mod.ModHeaders.MetaDataHeaders.StorageSigAndHeader.VersionString.Replace("\0", String.Empty);
                            blobIndexSize = mod.MDTables.GetBlobIndexSize();

                            // next loop sum tables byte length till reaching Assembly Table - this would give Assembly Table start offset
                            long assemblyTableOffset = mod.ModHeaders.MetaDataTableHeader.End;
                            for (int tablesCounter = 0; tablesCounter < (int) Types.Assembly; tablesCounter++)
                            {
                                assemblyTableOffset += mod.MDTables.Tables[tablesCounter].RawData.Length;
                            }

                            Table tableAssembly = mod.MDTables.GetTable(Types.Assembly);
                            if (tableAssembly.Count > 0) // this should be 1
                            {
                                if (tableAssembly[0][6].RawData == 0)
                                {
                                    // Index for public key points no nothing
                                    publicKeyOffset = 0;
                                }
                                else
                                {
                                    publicKeyOffset = mod.BlobHeap.Start + tableAssembly[0][6].RawData + 1;
                                }

                                assemblyFlag = (uint) tableAssembly[0][5].Data;

                                if (assemblyTableOffset > 0)
                                {
                                    publicKeyIndexOffset = assemblyTableOffset + ASSEMBLY_TABLE_PUBLIC_KEY_INDEX_OFFSET;
                                    assemblyFlagOffset = assemblyTableOffset + ASSEMBLY_TABLE_FLAGS_OFFSET;
                                }
                            }

                            if (getSigOnly)
                                return true;

                            // next loop sum tables byte length till reaching Assembly References Table - this would give Assembly References Table start offset
                            long referenceTableOffset = mod.ModHeaders.MetaDataTableHeader.End;
                            for (int tablesCounter = 0; tablesCounter < (int) Types.AssemblyRef; tablesCounter++)
                            {
                                referenceTableOffset += mod.MDTables.Tables[tablesCounter].RawData.Length;
                            }

                            if (referenceTableOffset > 0)
                            {
                                Table tableAssemblyReferences = mod.MDTables.GetTable(Types.AssemblyRef);
                                if (tableAssemblyReferences.Count > 0)
                                {
                                    for (int referencesCounter = 0; referencesCounter < tableAssemblyReferences.Count; referencesCounter++)
                                    {
                                        AssemblyReference reference =
                                            new AssemblyReference(tableAssemblyReferences[referencesCounter][6].Data.ToString(),
                                                                  tableAssemblyReferences[referencesCounter][0].Data + "." +
                                                                  tableAssemblyReferences[referencesCounter][1].Data + "." +
                                                                  tableAssemblyReferences[referencesCounter][2].Data + "." +
                                                                  tableAssemblyReferences[referencesCounter][3].Data,
                                                                  tableAssemblyReferences[referencesCounter][5].Data.ToString(),
                                                                  referenceTableOffset + (referencesCounter*tableAssemblyReferences.RowSize));
                                        assemblyReferences.Add(reference);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        r.Close();
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return true;
        }

        #endregion
    }

    public class MetaInfo
    {
        public string FilePath;
        public long cliHeaderFlagOffset;
        public uint cliHeaderFlag;
        public long strongNameSignatureOffset;
        public long publicKeyOffset;
        public long publicKeyIndexOffset;
        public long assemblyFlagOffset;
        public uint assemblyFlag;
        public string compiledRuntimeVersion = String.Empty;
        public ArrayList assemblyReferences = new ArrayList();
        public int blobIndexSize;

        public MetaInfo(string filePath)
        {
            FilePath = filePath;
        }
        public bool Load()
        {
            return MetaUtility.GetAssemblyData(
                    FilePath, ref cliHeaderFlag, ref cliHeaderFlagOffset,
                    ref strongNameSignatureOffset, ref publicKeyIndexOffset,
                    ref publicKeyOffset, ref assemblyFlag, ref assemblyFlagOffset,
                    ref compiledRuntimeVersion, ref assemblyReferences, ref blobIndexSize,
                    getSigOnly:true);
        }

        public bool RemoveSignature()
        {
            if (!MetaUtility.IsAssemblyStrongSigned(cliHeaderFlag))
                return false;

            return MetaUtility.PatchAssemblyStrongSigning(
                FilePath, cliHeaderFlag, cliHeaderFlagOffset, strongNameSignatureOffset,
                publicKeyIndexOffset, assemblyFlag, assemblyFlagOffset, blobIndexSize,
                removePublicKey: false);
        }
    }
}