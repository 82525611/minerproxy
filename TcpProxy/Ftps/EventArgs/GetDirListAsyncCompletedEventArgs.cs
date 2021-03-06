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
using System.ComponentModel;

namespace TcpProxy.Ftps
{
    /// <summary>
    /// Provides data for the GetDirListAsyncCompleted event.
    /// </summary>
    public class GetDirListAsyncCompletedEventArgs : AsyncCompletedEventArgs 
	{
		private FtpsItemCollection _directoryListing;

        /// <summary>
        ///  Initializes a new instance of the PutFileAsyncCompletedEventArgs class.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <param name="canceled">A value indicating whether the asynchronous operation was canceled.</param>
        /// <param name="directoryListing">A FtpItemCollection containing the directory listing.</param>
        public GetDirListAsyncCompletedEventArgs(Exception error, bool canceled, FtpsItemCollection directoryListing)
               : base(error, canceled, null)
		{
            _directoryListing = directoryListing;
		}

        /// <summary>
        /// Directory listing collection.
        /// </summary>
        public FtpsItemCollection DirectoryListingResult
        {
            get 
            {
                //base.RaiseExceptionIfNecessary();
                return _directoryListing; 
            }
        }
    }

} 
