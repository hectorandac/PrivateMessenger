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
  "publicKey": Public Key for encription,  <= Optional
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

## How 

## AUTHORS
Written by Héctor Acosta, Raul Ovalle and Tomas González.
