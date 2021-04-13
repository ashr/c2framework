# c2framework
Various C2 Framework Dropper/Stager generator to incorporate into AtomicRedTeam.

First iteration was Metasploit stager autogeneration and RC script creation, which is a bit silly and I probably need to rather use the API.

Second iteration was the Empire API integration, which is very much in development still, but functional at the moment is:

empire-dropper stagers - Show stagers available in Empire API

empire-dropper encrypt - Encrypt text for the config.json file (API Username and Password)

empire-dropper create listenertype listenername stagertype Option1=Option1Value Option2=Option2Value - Create an empire listener, generate stager of type using specified parameters and invoke

Currently only support windows/launcher_bat, currently testing windows/csharp_exe, but I've had to make a change to the stager code in Empire for the API to return the base64 data of the generated Zipfile instead of just the file name. I'll log an issue at Empire devs at some point to discuss this.

![go9n6-d0yp8](https://user-images.githubusercontent.com/171286/114510025-79c43180-9c36-11eb-9b2c-c6e65d287cd2.gif)

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
