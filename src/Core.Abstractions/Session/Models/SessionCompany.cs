using System;

namespace Core.Session
{
    [Obsolete]
    internal sealed class SessionCompany : CoreSessionContainer<Guid?>
    {
        public SessionCompany()
        {
        }

        public SessionCompany(Guid? id, string name = default) : base(id, name)
        {
        }

    }
}
