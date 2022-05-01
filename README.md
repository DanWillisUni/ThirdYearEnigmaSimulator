# EnigmaSimulator
## File Structure:
For my filestructure I have 4 main projects
### 1. EngimaSimulator
This is a webapplication written to simulate the engima.
This has all the runtime logs stored in the folder called "logs" these are then split by year > month > day
Configuration is where all of the models used in the configuration are stored.
### 2. EnigmaBreaker
This is a console application for
### 3. SharedServices
This is a class library that is referanced by both the Simulator and the Breaker.
This contains all the services and models that are used for both the breaker and the simulator. 
It contains the EncodingService, EnigmaModel, PhysicalConfiguration and RotorModel.
### 4. Tests
This project has all the Unit tests that I wrote to test all the aspects of the projects.

## Documentation
