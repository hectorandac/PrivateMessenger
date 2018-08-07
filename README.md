# PrivateMessenger
C# PrivateMessenger

Developing team:
* Héctor Acosta 1065948
* Tomas González 1065894 

## Comunication protocol
pmessenger [OPTION]

## Description
Allow comunication between clients in a secure manner.

  -s, 
  Initiates the server in the current machine.

  -lc,
  List clients connected.
  
  -c, [CLIENTID]
  Register the current machine as a client.
  
  -psw, [PASSWORDGEN]
  Generates a private and public to secure the current session, if no password is provided a random one will be generated for the current session.
  
  -m, [MESSEGE]
  The messege to send.
  
  -a,
  Send to all recipients.
  
  -r, [CLIENTID]
  Sets the recipient to the given client ID.

  -i, [IPADDRESS]
  Sets the server ip address.

  -p, [PORT]
  Sets the server listening port.

## AUTHORS
Written by Héctor Acosta and Tomas González.
