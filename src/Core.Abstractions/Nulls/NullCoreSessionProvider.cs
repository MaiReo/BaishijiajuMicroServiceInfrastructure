using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class NullCoreSessionProvider : ICoreSessionProvider
    {
        public ICoreSession Session => null;
    }
}
