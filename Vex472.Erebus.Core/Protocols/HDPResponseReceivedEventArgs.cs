using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vex472.Erebus.Core.Protocols
{
    /// <summary>
    /// Provides data for the <see cref="HDP.ResponseReceived"/> event.
    /// </summary>
    public sealed class HDPResponseReceivedEventArgs : EventArgs
    {
        ErebusAddress _server;
        HDPService _service;

        internal HDPResponseReceivedEventArgs(ErebusAddress server, HDPService service)
        {
            _server = server;
            _service = service;
        }

        /// <summary>
        /// Gets the address of the host that responded.
        /// </summary>
        public ErebusAddress ServerAddress => _server;

        /// <summary>
        /// Gets the service that is provided by the host.
        /// </summary>
        public HDPService Service => _service;
    }
}