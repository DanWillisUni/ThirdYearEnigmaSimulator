# EnigmaSimulator
## File Structure:
For my filestructure I have 4 main projects
### 1. EngimaSimulator
This is a webapplication written to simulate the engima.<br />
This has all the runtime logs stored in the folder called "logs" these are then split by year > month > day.<br />
Configuration is where all of the models used in the configuration are stored.<br />
The webapp is an MVC model so the relivent Models,Views and Controllers are in those dirctories.<br />
Saves contains any save of the enigma configuration. Currently this is just the temp file but it is where all the other saves would go.<br />
Services contains all the other nessasary services such as the file handler service.<br />
The appsettings JSON should be the only thing that is needed to be changed.
### 2. EnigmaBreaker
This is a console application for decrypting enigma.<br />
This has all the runtime logs stored in the folder called "logs" these are then split by year > month > day.<br />
Configuration is where all of the models used in the configuration are stored.<br />
Services contains all the other nessasary services including all the fitness functions and file handlers.<br />
The appsettings JSON should be the only thing that is needed to be changed.<br />
The resources directory is split in half, half of it is texts and the other half is results from experiments.
### 3. SharedServices
This is a class library that is referanced by both the Simulator and the Breaker.<br />
This contains all the services and models that are used for both the breaker and the simulator. <br />
It contains the EncodingService, EnigmaModel, PhysicalConfiguration and RotorModel.
### 4. Tests
This project has all the Unit tests that I wrote to test all the aspects of the projects.
## To Clone/Run
### Clone project
This project was made in Visual Studio so below are the steps to clone this project for Visual Studio community edition 2022.<br />
1. Download and open Visual Studio.<br />
2. On the Get started tab click "Clone a repository"<br />
3. Enter the Repository location (https://github.com/DanWillisUni/EnigmaSimulator.git)<br />
4. Enter the location locally to clone the project to.<br />
5. Click Clone.<br />
Alternatively see this documentation: https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository
### Run Simulator
Once the project is cloned and open in Visual Studio<br />
1. Select the "EnigmaSimulator" project<br />
2. Right click.
3. Then click "Set as Startup project"<br />
4. Click run. <br />
5. If Debug is selected this will open a new browser window with the main simulator page on. If it is Release an Executable called "EnigmaSimulator" will be generated in \EngimaSimulator\bin\Release\netcoreapp3.1
### Run Breaker
Once the project is cloned and open in Visual Studio<br />
1. Select the "EnigmaBreaker" project<br />
2. Right click.<br />
3. Then click "Set as Startup project"<br />
4. Open the appsettings.json file<br />
5. If you want to run tests in the Services.Measure class set BasicSettings.isMeasure to true, else false.<br />
6. If you want to input the ciphertext into the console set BasicSettings.inputFormat to "USER" else set to "RAND" for random text with random Enigma configuration.<br />
7. Click run.<br />
8. If Debug is selected this will run the code in a new Console window. If it is Release an Executable called "EnigmaBreaker" will be generated in \EngimaBreaker\bin\Release\netcoreapp3.1
