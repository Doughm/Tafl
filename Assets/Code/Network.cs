using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

public class Network
{
    // - - - - - - - - - - - - - //
    //	 TCP Synchronous 1 to 1  //
    // - - - - - - - - - - - - - //
    Socket socket;
    byte[] buffer;
    string tempStr;
    string convertedStr;
    List<string> messages = new List<string>();
    int bufferSize = 4096;

    public Network()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Blocking = false;
        buffer = new byte[bufferSize];
    }

    //sets the port that is listening for connections
    public void host(ushort port)
    {
        socket.Bind(new IPEndPoint(0, port));
    }

    //sets the port that is listening for connections
    public void stopHost()
    {
        socket.Close();
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    //connects to a computer, returns true if a connection is established
    public bool connect(string ipAddress, ushort port)
    {
        try
        {
            socket.Connect(ipAddress, port);
            return true;
        }
        catch
        {
            return false;
        }
    }

    //gets any incoming connections, and increments the listening port
    public bool listenForConnections()
    {
        try
        {
            socket.Listen(100);
            socket = socket.Accept();
            return true;
        }
        catch
        {
            return false;
        }
    }

    //disconnects from all computers
    public void disconnect()
    {
        socket.Close();
    }

    //sends a message over a TCP stream to all connected users
    public void sendMessage(string message)
    {
        socket.Send(Encoding.UTF8.GetBytes(message + '|'));
    }

    //receives messages over a TCP stream from all connected users
    public void receiveMessages()
    {
        while (true)
        {
            if (socket.Available == 0)
            {
                break;
            }
            Array.Clear(buffer, 0, buffer.Length);
            socket.Receive(buffer);
            tempStr = string.Empty;
            convertedStr = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim('\0');
            for (int i = 0; i < convertedStr.Length; i++)
            {
                if (convertedStr[i] != '|')
                {
                    tempStr += convertedStr[i];
                }
                if (convertedStr[i] == '|')
                {
                    messages.Add(tempStr);
                    tempStr = string.Empty;
                }
            }
        }
    }

    //sets a socket to blocking
    public void setBlocking(bool block)
    {
        socket.Blocking = block;
    }

    //gets a message in que
    public string getMessage(int index)
    {
        if (messages.Count > 0)
        {
            return messages[index];
        }
        return "getMessage() error";
    }

    //gets the number of messages
    public int getNumberOfMessages()
    {
        return messages.Count;
    }

    //clears the message buffer
    public void clearMessages()
    {
        messages.Clear();
    }
}
