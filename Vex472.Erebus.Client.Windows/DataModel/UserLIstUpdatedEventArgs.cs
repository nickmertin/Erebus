using System;

namespace Vex472.Erebus.Client.Windows.DataModel
{
    public sealed class UserListUpdatedEventArgs : EventArgs
    {
        User[] _all, _online, _recent;

        public UserListUpdatedEventArgs(User[] all, User[] online, User[] recent)
        {
            _all = all;
            _online = online;
            _recent = recent;
        }

        public User[] All => _all;

        public User[] Online => _online;

        public User[] Recent => _recent;
    }
}