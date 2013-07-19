﻿/*
 * Copyright (C) 2013-2013 Arctium Emulation <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework.Logging;

namespace AuthServer.Network
{
    public class Server
    {
        readonly TcpListener listener;

        public Server(string ip, ushort port)
        {
            var bindIP = IPAddress.None;

            if (!IPAddress.TryParse(ip, out bindIP))
            {
                Log.Message(LogType.Error, "AuthServer can't be started: Invalid IP-Address ({0})", ip);
                Console.ReadKey(true);

                Environment.Exit(0);
            }

            try
            {
                listener = new TcpListener(bindIP, port);
                listener.Start();

                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(100);

                        if (listener.Pending())
                        {
                            var worker = new AuthSession();

                            worker.threadManager.Reset();

                            listener.BeginAcceptSocket(new AsyncCallback(worker.Accept), listener);

                            worker.threadManager.WaitOne();
                        }
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Log.Message(LogType.Error, "{0}", ex.Message);
            }
        }
    }
}
