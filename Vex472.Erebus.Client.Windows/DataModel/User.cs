using Vex472.Erebus.Core;

namespace Vex472.Erebus.Client.Windows.DataModel
{
    public sealed class User
    {
        public User(string name, ErebusAddress addr)
        {
            Name = name;
            Address = addr;
        }

        public string Name { get; private set; }

        public ErebusAddress Address { get; private set; }

        public bool Online { get; set; }

        public override bool Equals(object obj) => obj is User && this == (User)obj;

        public override int GetHashCode() => Name.GetHashCode() ^ Address.GetHashCode();

        public static bool operator ==(User x, User y) => (ReferenceEquals(x, null) && ReferenceEquals(y, null)) || (!ReferenceEquals(x, null) && !ReferenceEquals(y, null) && x.Name == y.Name && x.Address == y.Address);

        public static bool operator !=(User x, User y) => (ReferenceEquals(x, null) ^ ReferenceEquals(y, null)) || (!ReferenceEquals(x, null) && !ReferenceEquals(y, null) && (x.Name != y.Name || x.Address != y.Address));
    }
}