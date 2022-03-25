//
//  Author:
//       Benton Stark <benton.stark@gmail.com>
//
//  Copyright (c) 2016 Benton Stark
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Runtime.Serialization;

namespace TcpProxy.Ftps
{

    /// <summary>
    /// This exception is thrown when the FTP client is unable to establish a data connection with the FTP server.
    /// Data connection are temporary, secondary connnections used to transfer files and other types of data between the
    /// FTP client and the FTP server.  The method in which data connections are established is determined by the type
    /// of data transfer mode specified when connection to an FTP server (e.g. Passive, Active)
    /// </summary>
    [Serializable()]
    public class FtpsDataConnectionException : FtpsConnectionClosedException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FtpsDataConnectionException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        public FtpsDataConnectionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsDataConnectionException(string message, Exception innerException)
            :
           base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Stream context information.</param>
        protected FtpsDataConnectionException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsDataConnectionException(string message, FtpsResponse response, Exception innerException)
            :  base(message, response, innerException)
        {   }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        public FtpsDataConnectionException(string message, FtpsResponse response)
            : base(message, response)
        {   }

    }

}