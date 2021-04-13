# c2framework
Various C2 Framework Dropper/Stager generator to incorporate into AtomicRedTeam.

First iteration was Metasploit stager autogeneration and RC script creation, which is a bit silly and I probably need to rather use the API.

Second iteration was the Empire API integration, which is very much in development still, but functional at the moment is:

empire-dropper stagers - Show stagers available in Empire API

empire-dropper encrypt - Encrypt text for the config.json file (API Username and Password)

empire-dropper create listenertype listenername stagertype Option1=Option1Value Option2=Option2Value - Create an empire listener, generate stager of type using specified parameters and invoke

Staging windows/launcher_bat:

empire-dropper create http lname1 windows/launcher_bat

![go9n6-d0yp8](https://user-images.githubusercontent.com/171286/114510025-79c43180-9c36-11eb-9b2c-c6e65d287cd2.gif)

Staging windows/csharp_exe:

empire-dropper create http lname2 windows/csharp_exe

![empire-dropper-csharp](https://user-images.githubusercontent.com/171286/114545757-a1c68b80-9c5c-11eb-837e-d046c822a6a6.gif)

For windows/csharp_exe I had to modify Empire API (Which I will chat to the empire devs to), the API only returns the path to the CS project instead of the base64 of the zip file, but it's very simple to enable this yourself:

Edit ./lib/stagers/windows/csharp_exe.py in the root Empire source directory and replace 


    return outfile
 

with


    zipFile = open(outfile+".zip", 'rb')
    zip_read = zipFile.read()
 
    return zip_read

![image](https://user-images.githubusercontent.com/171286/114546417-69737d00-9c5d-11eb-98c8-d07da7276380.png)


Default Username and Password in the config.json file is 'username' and 'password'. If you set up your Empire API using those creds it should work after changing the IP to the API of Empire. If you do use different creds, encrypt them with 'empire-dropper encrypt mynewpassword' and pop into config.json.

TODO:
Empire 
- In memory payload loading and execution for all stagers
- On disk loading and execution for all stagers

Cobalt Strike:
- TODO

Posh
- TODO

Covenant
- TODO
