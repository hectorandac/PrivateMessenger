# PrivateMessenger
C# PrivateMessenger

Developing team:
* Héctor Acosta 1065948
* Tomas González 1065894
* Raul Ovalle 

## Comunication protocol
The protocol that uses this application for comunication is based an an asymetric structure. The clients requests have to contain the following atributes for the server to understand it:

### CLIENT REQUEST
```
request: {
  "requestType": "GET-USERS" | "REGISTER-USER" | "MESSAGE-USER",  <= Obligatory
  "userId": String identifying the user,  <= Optional
  "message": Message to send,  <= Optional
  "recipietnId": Identifier for the recipient  <= Optional
}
```

### SERVER RESPONSE TO USER
response: String of the information required by user  <= Optional

## Description
Allow comunication between clients in a secure manner.

  server, 
  Initiates the server in the current machine.

  client,
  Initiates a client comunication.

  -lc,
  List clients connected.
  
  -u, [CLIENTID]
  Register the current machine as a client.
  
  -r, [CLIENTID]
  Sets the recipient to the given client ID.

  -i, [IPADDRESS]
  Sets the server ip address.

  -p, [PORT]
  Sets the server listening port.
  
  -h | ? | --help,
  Gives you the documentation of the given command

## How to install
All the releases are located in the following files, select your own depending on yout system:
* [Linux](https://github.com/hectorandac/PrivateMessenger/tree/master/bin/Release/netcoreapp2.0/linux-x64/publish)
* [Mac OS 10.12](https://github.com/hectorandac/PrivateMessenger/tree/master/bin/Release/netcoreapp2.0/osx.10.12-x64/publish)
* [Windows 10](https://github.com/hectorandac/PrivateMessenger/tree/master/bin/Release/netcoreapp2.0/win10-x64/publish)

Run the commands in your platform directory ```cd <Your platform directory>``` and executing the command ```pmessenger``` or ```pmessenger.exe``` on windows, Go to how to use for more details on how to use the commands.

## How to use
This section explains how to use the software, we will use a windows system as example on how to type the commands.

### Initiate the server
Use the command:
```
pmessenger.exe server
```
This command will start the server in the default ip (127.0.0.1) and port (10094). You can customize these properties appending the options -i and -p respectively to modify them.

### Initiate the client
Once the server is running we can start registering some clients:
```
pmessenger.exe client -u hectorandac
```
Notice the option -u it's very important to specify a unique id for each client so the server knows where to send the message. After you are connected you can use the option -r <Client ID> to chat with that client.

## AUTHORS
Written by Héctor Acosta, Raul Ovalle and Tomas González.
