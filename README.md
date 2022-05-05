# EnigmaSimulator
## File Structure:
For my filestructure I have 4 main projects
### 1. EngimaSimulator
This is a webapplication written to simulate the engima.
This has all the runtime logs stored in the folder called "logs" these are then split by year > month > day
Configuration is where all of the models used in the configuration are stored.
The webapp is an MVC model so the relivent Models,Views and Controllers are in those dirctories.
Saves contains any save of the enigma configuration. Currently this is just the temp file but it is where all the other saves would go.
Services contains all the other nessasary services such as the file handler service.
The appsettings JSON should be the only thing that is needed to be changed.
### 2. EnigmaBreaker
This is a console application for decrypting enigma.
This has all the runtime logs stored in the folder called "logs" these are then split by year > month > day
Configuration is where all of the models used in the configuration are stored.
Services contains all the other nessasary services including all the fitness functions and file handlers
The appsettings JSON should be the only thing that is needed to be changed.
The resources directory is split in half, half of it is texts and the other half is results from experiments.
### 3. SharedServices
This is a class library that is referanced by both the Simulator and the Breaker.
This contains all the services and models that are used for both the breaker and the simulator. 
It contains the EncodingService, EnigmaModel, PhysicalConfiguration and RotorModel.
### 4. Tests
This project has all the Unit tests that I wrote to test all the aspects of the projects.

## Documentation
