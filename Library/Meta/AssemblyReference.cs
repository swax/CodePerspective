/***************************************************************************************************
 * Copyright (C) - Andrea Bertolotto - 2006-2010
 *
 * a.bertolotto - 2006/08/23 14.21.00 - Created
 ***************************************************************************************************/

namespace XLibrary.Meta
{
    internal class AssemblyReference
    {
        private readonly string referenceName;
        private readonly string version;
        private readonly string publicKeyOrToken;
        private readonly long referenceOffset;

        public AssemblyReference(string referenceName, string version, string publicKeyOrToken, long referenceOffset)
        {
            this.referenceName = referenceName;
            this.version = version;
            this.publicKeyOrToken = publicKeyOrToken;
            this.referenceOffset = referenceOffset;
        }

        public string ReferenceName
        {
            get { return this.referenceName; }
        }

        public string Version
        {
            get { return this.version; }
        }

        public string PublicKeyOrToken
        {
            get { return this.publicKeyOrToken; }
        }

        public long ReferenceOffset
        {
            get { return this.referenceOffset; }
        }
    }
}